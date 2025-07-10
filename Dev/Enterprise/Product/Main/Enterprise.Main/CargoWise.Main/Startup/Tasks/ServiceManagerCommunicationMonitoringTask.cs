using CargoWise.Application;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Startup
{
	public class ServiceManagerCommunicationMonitoringTask : AbstractApplicationStartupTask
	{
		public override string TaskDescription => string.Empty;

		public override int FailureExitCode => ExitCodes.ServiceManagerCommunicationMonitoringTaskError;

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			ObjectFactory.Get<INudgingEventsTracker>(); //Create the singleton instance so that tracking will begin.

			return true;
		}

		protected override bool GetShouldExecute(CommandLineArguments arguments) => true;
	}
}
