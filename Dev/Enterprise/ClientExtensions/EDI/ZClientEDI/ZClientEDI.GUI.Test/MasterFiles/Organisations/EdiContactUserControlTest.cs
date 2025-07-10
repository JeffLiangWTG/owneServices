using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	[TestedType(typeof(EdiContactsUserControlForTest))]
	public class EdiContactsUserControlTest : OrganisationSecurityContainerControlBaseTest
	{
		public class EdiContactsUserControlForTest : EdiContactsUserControl
		{
			public ContactSendEmailSetResetPassword GetContactSendEmailSetResetPasswordValue =>
				GetContactSendEmailSetResetPassword();
		}

		protected override OrganisationSecurityContainerControl GetNewControlForTesting() => new EdiContactsUserControlForTest();

		protected override string[] SecurityContainerPropertiesEnabledForThisControl => new[] { "IsModifyContact", "IsModifyContactContactDetails", "IsModifyContactDocDeliveryDetails", "IsModifyContactPersonalInformation" };

		public void TestGetContactSendEmailSetResetPasswordIsOverriden()
		{
			using (var contactUserControl = new EdiContactsUserControlForTest())
			{
				Assert(contactUserControl.GetContactSendEmailSetResetPasswordValue is EdiContactSendEmailSetResetPassword);
			}
		}
	}
}
