using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.ServiceTasks.EventLogs;
using Enterprise.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Client.EDI.ServiceTask.RetrievedEventLogsServiceTask.Code,
	"Retrieved Event Logs Service Task",
	"SYS",
	typeof(Enterprise.Client.EDI.ServiceTask.RetrievedEventLogsServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "5Minutes",
	DefaultScheduleRunEvery = "60minutes",
	ActiveByDefault = true
	)]

namespace Enterprise.Client.EDI.ServiceTask
{
	public class RetrievedEventLogsServiceTask : ServiceProviderImpl
	{
		public const string Code = "ELS";

		//Run Task
		#region Implementation

		public override void RunTask(CancellationToken token)
		{
			var eventLogDataTransmissionHandler = new EventLogDataTransmissionHandler();
			var retriever = GetNewEventLogRetriever();

			var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				foreach (var machineName in MachineNames)
				{
					token.ThrowIfCancellationRequested();
					var xelement = retriever.Retrieve(machineName, eventLogDataTransmissionHandler, ServiceLogger);
					if (xelement != null)
					{
						new EventLogProcessor(ServiceLogger).Process(xelement, machineName, eventLogDataTransmissionHandler);
						eventLogDataTransmissionHandler.SyncEventLogHighWaterMarkRegistryAndListProvidersAndTheirCapacityRegistry();
					}
				}
			}
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		internal virtual IEventLogRetriever GetNewEventLogRetriever()
		{
			return new EventLogRetriever();
		}

		internal virtual IEnumerable<string> MachineNames
		{
			get
			{
				return RegistryMachinesHandler.GetLatestAccessibleHostNameList();
			}
		}

		#endregion
	}
}
