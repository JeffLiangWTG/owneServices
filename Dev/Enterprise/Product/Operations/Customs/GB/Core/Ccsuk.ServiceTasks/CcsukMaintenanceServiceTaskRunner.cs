using System.Threading;

namespace Enterprise.Customs.GB.Ccsuk.ServiceTasks
{
	class CcsukMaintenanceServiceTaskRunner
	{
		public CcsukMaintenanceServiceTaskRunner(Integration.ILogger serviceLogger)
		{
			this.serviceLogger = serviceLogger;
		}

		internal void DoEverything(CancellationToken token)
		{
			new CcsukMaintenanceServiceTaskRunner_Archiver(serviceLogger).ArchiveOldRecordsAndSendReports(token);
			// List other tasks here....
		}

		readonly Integration.ILogger serviceLogger;
	}
}
