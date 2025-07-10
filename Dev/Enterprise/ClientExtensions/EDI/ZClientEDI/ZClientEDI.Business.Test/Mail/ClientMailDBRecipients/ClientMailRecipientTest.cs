using CargoWise.EntityFramework;
using Enterprise.Client.EDI.AutoDeploy.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Business.Testing;
using MailManager;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Mail.Business.Test
{
	[TestedType(typeof(ClientMailRecipient))]
	internal class ClientMailRecipientTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var mail = factory.NewWithValidTestData<MailItem>();
			mail.MI_Direction = DirectionList.Codes.Receive;
			var recipient = mail.AddRecipientForUserCommunication("test@test.com");

			UpgradesToClient upgrade = factory.NewWithValidTestData<UpgradesToClient>();
			var licHeader = BillingTestHelper.CreateLicence(factory, "AAA");
			upgrade.L1_LD = licHeader.LA_LD;
			upgrade.L1_RequestedUpgradeMethod = "HTP";
			upgrade.L1_ActualUpgradeMethod = "HTP";
			upgrade.L1_HL = factory.New<ReleaseBuild>().PK;

			var upgradeLink = factory.New<ClientMailRecipient>();
			upgradeLink.MRX_L1 = upgrade.PK;
			upgradeLink.MRX_MR = recipient.PK;
			return upgradeLink;
		}
	}
}
