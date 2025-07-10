using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.MCP.CusDec.Testing
{
	class McpDestin8WebServiceTester_GetCredentialsTEST : TestCaseWithFactory
	{
		public void TestGetCredentialsForDestin8WebserviceFromRegistry()
		{
			ZString badge = "FEY";
			CreateTestMcpSettingInRegistry(badge, Factory);

			CredentialsSettingCollection collBack = GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			CredentialsSetting credBack = collBack.FindByBadgeCode(badge);
			AssertEquals(username, credBack.Username);
			AssertEquals(password, credBack.Password);
			AssertEquals(printer, credBack.Printer);
			AssertEquals(company, credBack.Company);
		}

		public const string company = "CAW";
		public const string printer = "CAW9";
		public const string username = "CAW8";
		public const string password = "password";

		public static void CreateTestMcpSettingInRegistry(ZString badge, BusinessObjectFactory factory)
		{
			// First create regular badge entry, then create credentials entry
			BadgeCodeSettingCollection badgeSettingColl = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty) ?? new BadgeCodeSettingCollection();

			new BadgeCodeSettingCollection();
			BadgeCodeSetting badgeSetting = new BadgeCodeSetting(factory);
			badgeSetting.BadgeCode = badge;
			badgeSetting.CSPCode = "MCP";
			badgeSettingColl.Add(badgeSetting);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeSettingColl);

			CredentialsSetting cred = GetTestCredential(badge);
			CredentialsSettingCollection credsColl = GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty) ?? new CredentialsSettingCollection();
			credsColl.Add(cred);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, credsColl);
		}

		public static CredentialsSetting GetTestCredential(ZString badge)
		{
			CredentialsSetting cred = new CredentialsSetting();
			cred.BadgeCode = badge;
			cred.Company = company;
			cred.Username = username;
			cred.Printer = printer;
			cred.Password = password;
			return cred;
		}
	}
}

