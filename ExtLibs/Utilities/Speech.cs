using System;
using System.Text.RegularExpressions;
using log4net;

namespace MissionPlanner.Utilities
{
    public class Speech: IDisposable, ISpeech
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Speech Instance { get; } = new Speech();

        public static bool speechEnable { get; set; } = false;
        
        System.Diagnostics.Process _speechlinux;
        

        bool MONO = false;

        public bool IsReady 
        {
            get {
                if (MONO)
                {
                    return true;
                }
                else
                {
                    try
                    {
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                    return false;
                }
            }
        }

        public Speech()
        {
            var t = Type.GetType("Mono.Runtime");
            MONO = (t != null);

            log.Info("TTS: init, mono = " + MONO);

            
        }

        public void SpeakAsync(string text)
        {
            if (text == null)
                return;

            text = Regex.Replace(text, @"\bPreArm\b", "Pre Arm", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bdist\b", "distance", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bNAV\b", "Navigation", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\b([0-9]+)m\b", "$1 meters", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\b([0-9]+)ft\b", "$1 feet", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\b([0-9]+)\bbaud\b", "$1 baudrate", RegexOptions.IgnoreCase);

            if (MONO)
            {
                try
                {
                    //if (_speechlinux == null)
                    {
                        _speechlinux = new System.Diagnostics.Process();
                        _speechlinux.StartInfo.RedirectStandardInput = true;
                        _speechlinux.StartInfo.UseShellExecute = false;
                        _speechlinux.StartInfo.FileName = "festival";
                        _speechlinux.Start();
                        _speechlinux.Exited += new EventHandler(_speechlinux_Exited);

                        log.Info("TTS: start " + _speechlinux.StartTime);

                    }
                    
                    _speechlinux.StandardInput.WriteLine("(SayText \"" + text + "\")");
                    _speechlinux.StandardInput.WriteLine("(quit)");

                    _speechlinux.Close();
                }
                catch { } // ignore errors
                
            }

            log.Info("TTS: say " + text);
        }

        void _speechlinux_Exited(object sender, EventArgs e)
        {
            log.Info("TTS: exit " + _speechlinux.ExitTime);
        }

        public void SpeakAsyncCancelAll()
        {
            if (MONO)
            {
                try
                {
                    if (_speechlinux != null)
                        _speechlinux.Close();
                }
                catch { }
            }
        }

        public void Dispose()
        {

            GC.SuppressFinalize(this);
        }
    }
}