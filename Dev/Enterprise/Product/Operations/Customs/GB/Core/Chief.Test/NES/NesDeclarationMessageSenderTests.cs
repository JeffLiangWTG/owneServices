using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Chief.CusDec.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Chief.NES.Testing
{
	public class NesDeclarationMessageSenderTests : TestCaseWithFactory
	{
		public void TestCheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials()
		{
			var declaration = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(this.Factory);
			Customs.Business.SendsMessagesToCustomsShutterUpperer sendSilently = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			CreateAndStoreBadge("AAA");
			var sender = GetNewDeclarationMessageSender();
			declaration.JE_CustomsProfile = "AAA";
			Assert(!sender.CheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials(declaration, sendSilently));
			AssertContains("No credentials could be found for badge AAA", sendSilently.InvalidOperationText);
			MakeCredential();
			Assert(sender.CheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials(declaration, sendSilently));
		}

		protected virtual ChiefDeclarationMessageSender GetNewDeclarationMessageSender()
		{
			return new NesDeclarationMessageSender();
		}

		protected virtual string CspCode
		{
			get { return Enterprise.Customs.GB.Registry.GatewayList.Codes.NES; }
		}

		protected void MakeCredential(int failureCount = 0)
		{
			var credential = new CredentialsSetting();
			credential.BadgeCode = "AAA";
			credential.Printer = "x";
			credential.Company = "y";
			var allCreds = new CredentialsSettingCollection();
			allCreds.Add(credential);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allCreds);
		}

		protected void CreateAndStoreBadge(string badgeCode)
		{
			var collection = new BadgeCodeSettingCollection();
			var badge = collection.AddNew();
			badge.BadgeCode = badgeCode;
			badge.CSPCode = CspCode;
			collection.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, collection);
		}
	}
}
