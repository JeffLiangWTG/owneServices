using System.Diagnostics;
using System.ServiceProcess;
using WTG.StaticAnalysis.Annotation;

#if DEBUG
namespace Setup.Testing
{
	[CodeAlive("For testing only")]
	public partial class DummyService : ServiceBase
	{
		public DummyService()
		{
			InitializeComponent();

			ServiceName = "Dummy Remote Printing Test Service";

			EventLog.Log = "Application";
			CanHandlePowerEvent = true;
			CanHandleSessionChangeEvent = true;
			CanPauseAndContinue = true;
			CanShutdown = true;
			CanStop = true;
		}

		protected override void OnStart(string[] args)
		{
		}

		protected override void OnStop()
		{
		}
	}
}
#endif