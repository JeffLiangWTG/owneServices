using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgHeaderWebContract : MyAccountWebContract
	{
		public EDIOrgHeaderWebContract(OrgContact loggedInContact) : base(loggedInContact)
		{
		}

		protected override NotificationEmailTemplate ContractTemplate => EDIDataRegistry.Instance.MyAccountTermsAndConditionsContent.Value;

		protected override bool ShouldSendNotificationEmail => true;

		protected override string NotificationEmailSenderName => EDIDataRegistry.Instance.MyAccountTermsAndConditionsNotificationEmailSenderName.Value;

		protected override string NotificationEmailSenderAddress => EDIDataRegistry.Instance.MyAccountTermsAndConditionsNotificationEmailSenderAddress.Value;

		protected override NotificationEmailTemplate NotificationEmailTemplate => EDIDataRegistry.Instance.MyAccountTermsAndConditionsNotificationEmailTemplate.Value;

		public override EnterpriseBusinessObject ContractSignatory => LoggedInOrganisation;
	}
}