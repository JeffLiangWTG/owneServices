namespace Enterprise.Client.EDI.VersionReporting.BatchProcessor
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using Enterprise.Client.EDI.UserAccountReporting.BatchProcessor;
	using Enterprise.MasterFiles.Business;

	public class UserAccountReportMessageAction : ReportMessageAction
	{
		public UserAccountReportMessageAction(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{
		}

		internal override IEmailAttachmentProcessor CreateProcessor(string from, INotifications notifications)
		{
			return new UserAccountReportProcessor(NotificationLogger.CreateLogger(notifications));
		}
	}
}
