using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Chief.CspPuller.Testing
{
	sealed class CspMessagePullerTest : TestCaseWithFactory
	{
		public void TestMaybeIncrementLoginFailureCount()
		{
			var ex = new Exception("401");
			var result = false;
			AssertNoExceptionThrown(() =>
			{
				result = CspMessagePuller.MaybeIncrementLoginFailureCount(ex, credential, companyCredentials);
			});
			Assert("MaybeIncrementLoginFailureCount result should be true", result);
			AssertEquals("credential.WebServiceFailureCount", 2, credential.WebServiceFailureCount);
		}

		public void TestMaybeClearLoginFailureCount()
		{
			var result = false;
			AssertNoExceptionThrown(() =>
			{
				result = CspMessagePuller.MaybeClearLoginFailureCount(credential, companyCredentials);
			});
			Assert("MaybeClearLoginFailureCount result should be true", result);
			AssertEquals("credential.WebServiceFailureCount", 0, credential.WebServiceFailureCount);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var badges = new BadgeCodeSettingCollection();
			var badge = badges.AddNew();
			badge.CSPCode = GatewayList.Codes.MCP_CUSDECOnly;
			badge.BadgeCode = "XYZ";
			var badge2 = badges.AddNew();
			badge2.CSPCode = GatewayList.Codes.Pentant;
			badge2.BadgeCode = "ABC";

			credential = new CredentialsSetting();
			credential.BadgeCode = badge.BadgeCode;
			credential.Company = "XYZ";
			credential.Password = "password";
			credential.Username = "username";
			credential.WebServiceFailureCount = 1;
			var credential2 = new CredentialsSetting();
			credential2.BadgeCode = badge2.BadgeCode;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			companyCredentials = new CredentialsSettingCollection();
			companyCredentials.Add(credential);
			companyCredentials.Add(credential2);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, companyCredentials);
			Factory.Save();

			badges.Remove(badge2);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
		}

		CredentialsSettingCollection companyCredentials;
		CredentialsSetting credential;
	}
}
