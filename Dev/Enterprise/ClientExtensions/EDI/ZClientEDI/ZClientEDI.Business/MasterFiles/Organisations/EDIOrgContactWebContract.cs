using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgContactWebContract : MyAccountWebContract
	{
		public EDIOrgContactWebContract(OrgContact loggedInContact) : base(loggedInContact)
		{
		}

		protected override NotificationEmailTemplate ContractTemplate => EDIDataRegistry.Instance.MyAccountContactTermsAndConditionsContent.Value;

		protected override bool ShouldSendNotificationEmail => false;

		protected override string NotificationEmailSenderName => null;

		protected override string NotificationEmailSenderAddress => null;

		protected override NotificationEmailTemplate NotificationEmailTemplate => null;

		public override EnterpriseBusinessObject ContractSignatory => LoggedInContact;
	}
}