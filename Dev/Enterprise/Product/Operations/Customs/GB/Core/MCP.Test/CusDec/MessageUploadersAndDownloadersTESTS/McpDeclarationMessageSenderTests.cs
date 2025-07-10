using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Chief.CusDec.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.MCP.Testing
{
	class McpDeclarationMessageSenderTests : TestCaseWithFactory
	{
		// NB this also tests the function of CredentialsAndBadgeChecker (Chief) and the CNS sender. 

		public void TestCheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials_MissingCred()
		{
			JobDeclaration mcpDec = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(this.Factory);
			Customs.Business.SendsMessagesToCustomsShutterUpperer sendSilently = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			CreateAndStoreBadge("FEY");

			McpDeclarationMessageSender sender = new McpDeclarationMessageSender();
			mcpDec.JE_CustomsProfile = "FEY";
			Assert(!sender.CheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials(mcpDec, sendSilently));
			AssertContains("No credentials could be found for badge FEY", sendSilently.InvalidOperationText);
			MakeCredential();
			Assert(sender.CheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials(mcpDec, sendSilently));
		}

		public void TestCheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials_LockedOutCred()
		{
			var mcpDec = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(this.Factory);
			var sendSilently = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			CreateAndStoreBadge("FEY");
			MakeCredential(50);
			var sender = new McpDeclarationMessageSender();
			mcpDec.JE_CustomsProfile = "FEY";
			Assert(!sender.CheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials(mcpDec, sendSilently));
			AssertContains("suspended after repeated failed logins", sendSilently.InvalidOperationText);
			AssertContains("only after undertaking the above four steps for each affected badge", sendSilently.InvalidOperationText);
			AssertContains("1)", sendSilently.InvalidOperationText);

			sendSilently = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			GBCustomsDataRegistry.Instance.McpWebServiceLockoutThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 51);
			Assert(sender.CheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials(mcpDec, sendSilently));
			AssertNotContains("suspended after repeated failed logins", sendSilently.InvalidOperationText);
			AssertNotContains("only after undertaking the above four steps for each affected badge", sendSilently.InvalidOperationText);
			AssertNotContains("1)", sendSilently.InvalidOperationText);

			sendSilently = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			GBCustomsDataRegistry.Instance.McpWebServiceLockoutThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, -1);
			Assert(sender.CheckRegistryOptionsForDeclarationAreGoodBeforeSendingForExampleBadgeCredentials(mcpDec, sendSilently));
			AssertNotContains("suspended after repeated failed logins", sendSilently.InvalidOperationText);
			AssertNotContains("only after undertaking the above four steps for each affected badge", sendSilently.InvalidOperationText);
			AssertNotContains("1)", sendSilently.InvalidOperationText);
		}

		void MakeCredential(int failureCount = 0)
		{
			CredentialsSetting mcpCred = new CredentialsSetting();
			mcpCred.BadgeCode = "FEY";
			mcpCred.Username = "CAW8";
			mcpCred.Password = "foo";
			mcpCred.Printer = "x";
			mcpCred.Company = "y";
			mcpCred.WebServiceFailureCount = failureCount;
			CredentialsSettingCollection allCreds = new CredentialsSettingCollection();
			allCreds.Add(mcpCred);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allCreds);
		}

		void CreateAndStoreBadge(string badgeCode)
		{
			BadgeCodeSettingCollection collection = new BadgeCodeSettingCollection();
			var badge = collection.AddNew();
			badge.BadgeCode = badgeCode;
			badge.CSPCode = Enterprise.Customs.GB.Registry.GatewayList.Codes.MCP_CUSDECOnly;
			collection.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, collection);
		}
	}
}
