using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class WebOrgTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "NEW12345";
			org.OH_FullName = ZString.Replicate('A', OrgHeader.Schema.OH_FullNameMaxLength);
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_IsConsignee = false;
			org.OH_IsConsignor = false;
			org.OH_IsForwarder = false;
			org.OH_IsGlobalAccount = false;
			org.OH_IsWarehouseClient = false;
			Factory.Save();

			var webFactory = new WebFactory(Factory);
			var webOrg = new WebOrg(webFactory, org);
			AssertPropertiesEqual(org, webOrg);

			org.OH_IsConsignee = true;
			Factory.Save();
			webOrg = new WebOrg(webFactory, org);
			AssertPropertiesEqual(org, webOrg);

			org.OH_IsConsignor = true;
			Factory.Save();
			webOrg = new WebOrg(webFactory, org);
			AssertPropertiesEqual(org, webOrg);

			org.OH_IsForwarder = true;
			Factory.Save();
			webOrg = new WebOrg(webFactory, org);
			AssertPropertiesEqual(org, webOrg);

			org.OH_IsWarehouseClient = true;
			Factory.Save();
			webOrg = new WebOrg(webFactory, org);
			AssertPropertiesEqual(org, webOrg);

			org.OH_IsGlobalAccount = true;
			Factory.Save();
			webOrg = new WebOrg(webFactory, org);
			AssertPropertiesEqual(org, webOrg);
		}

		void AssertPropertiesEqual(OrgHeader org, WebOrg webOrg)
		{
			AssertEquals(org.PK, webOrg.PK);
			AssertEquals(org.OH_Code, webOrg.OH_Code);
			AssertEquals(org.OH_FullName, webOrg.OH_FullName);
			AssertEquals(org.OH_FullNameTruncated, webOrg.OH_FullNameTruncated);
			AssertEquals(org.OH_IsConsignee, webOrg.OH_IsConsignee);
			AssertEquals(org.OH_IsConsignor, webOrg.OH_IsConsignor);
			AssertEquals(org.OH_IsForwarder, webOrg.OH_IsForwarder);
			AssertEquals(org.OH_IsGlobalAccount, webOrg.OH_IsGlobalAccount);
			AssertEquals(org.OH_IsWarehouseClient, webOrg.OH_IsWarehouseClient);
			AssertEquals(org.OH_RL_NKClosestPort, webOrg.OH_RL_NKClosestPort);

			var expectedBranchOrOrgCountryCode = org.Branch == null ? org.CountryCode : org.Branch.Country.Code;
			AssertEquals(expectedBranchOrOrgCountryCode, webOrg.BranchOrOrgCountryCode);
		}
	}
}
