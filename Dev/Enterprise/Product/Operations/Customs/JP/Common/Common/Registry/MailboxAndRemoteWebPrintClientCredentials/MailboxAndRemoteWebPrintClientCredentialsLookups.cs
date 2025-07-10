using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Common
{
	public class MailboxAndRemoteWebPrintClientCredentialsLookups : ZLookups
	{
		public MailboxAndRemoteWebPrintClientCredentialsLookups(BusinessObject parent) : base(parent)
		{
		}

		public ICodeDescriptionPairList StatusList => new XtCredentialStatusList();

		public GlbGroupCollection FailureNotificationGroupList => failureNotificationGroupList ??= new GlbGroupCollection(Factory);
		GlbGroupCollection failureNotificationGroupList;

		public new MailboxAndRemoteWebPrintClientCredentials Parent => (MailboxAndRemoteWebPrintClientCredentials)base.Parent;

		protected override BusinessObjectFactory Factory => Parent.Factory;
	}
}
