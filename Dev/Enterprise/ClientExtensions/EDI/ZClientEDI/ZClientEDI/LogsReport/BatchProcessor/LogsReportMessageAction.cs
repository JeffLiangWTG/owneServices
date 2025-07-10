namespace Enterprise.Client.EDI.LogsReporting.BatchProcessor
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using Enterprise.Client.EDI.VersionReporting.BatchProcessor;
	using Enterprise.MasterFiles.Business;

	public class LogsReportMessageAction : ReportMessageAction
	{
		public LogsReportMessageAction(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{
		}

		internal override IEmailAttachmentProcessor CreateProcessor(string from, INotifications notifications)
		{
			return new LogsReportProcessor();
		}
	}
}
