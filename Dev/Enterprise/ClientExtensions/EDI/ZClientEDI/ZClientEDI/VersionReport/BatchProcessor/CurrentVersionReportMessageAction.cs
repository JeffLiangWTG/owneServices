namespace Enterprise.Client.EDI.VersionReporting.BatchProcessor
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;

	public class CurrentVersionReportMessageAction : ReportMessageAction
	{
		public CurrentVersionReportMessageAction(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{
		}

		internal override IEmailAttachmentProcessor CreateProcessor(string from, INotifications notifications)
		{
			return new CurrentVersionReportProcessor(NotificationLogger.CreateLogger(notifications), true);
		}
	}
}
