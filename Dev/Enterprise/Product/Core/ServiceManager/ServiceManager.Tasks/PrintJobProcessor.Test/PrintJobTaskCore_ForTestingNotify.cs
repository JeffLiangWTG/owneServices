using System.Threading;
using Enterprise.Integration;
using Enterprise.PrintProcessing;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing
{
	sealed class PrintJobTaskCore_ForTestingNotify : PrintJobTaskCore
	{
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			PrintJobManager.OnNotification?.Invoke(true, "test message");
		}

		protected override void Notify(bool successful, string message)
		{
			LoggerForTest.Log(LogType.Debug, message);
			base.Notify(successful, message);
		}
	}
}
