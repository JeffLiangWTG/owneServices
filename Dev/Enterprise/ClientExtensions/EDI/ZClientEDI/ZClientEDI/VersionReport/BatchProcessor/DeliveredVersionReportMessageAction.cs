namespace Enterprise.Client.EDI.VersionReporting.BatchProcessor
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;

	public class DeliveredVersionReportMessageAction : ReportMessageAction
	{
		public DeliveredVersionReportMessageAction(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{
		}

		internal override IEmailAttachmentProcessor CreateProcessor(string from, INotifications notifications)
		{
			return new DeliveredVersionReportProcessor(from, NotificationLogger.CreateLogger(notifications));
		}
	}
}
