using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module.Testing
{
	[TestedType(typeof(LicenceEnterpriseFilterBusinessObject))]
	public class LicenceEnterpriseFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region FilterByTokenAuthenticationEnable

		public void TestFilterByTokenAuthenticationEnable()
		{
			var enterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			var enterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise2.LE_TokenAuthenticationEnabled = true;
			var enterprise3 = Factory.NewWithValidTestData<LicenceEnterprise>();

			Factory.Save();

			var licenceEnterpriseFilter = new LicenceEnterpriseFilterBusinessObject();
			var productFilter = (ModuleTextFilter)licenceEnterpriseFilter["Token Authentication"];
			var licenceEnterpriseCollection = new LicenceEnterpriseCollection(Factory);
			productFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			productFilter.IsActive = true;

			productFilter.Property = TokenAuthenticationStatusList.Codes.Enabled;
			licenceEnterpriseCollection.Load(licenceEnterpriseFilter.Filter);

			Assert("Should only contain Licence Enterprises enterprise2", !licenceEnterpriseCollection.Contains(enterprise1.PK));
			Assert("Should only contain Licence Enterprises enterprise2", licenceEnterpriseCollection.Contains(enterprise2.PK));
			Assert("Should only contain Licence Enterprises enterprise2", !licenceEnterpriseCollection.Contains(enterprise3.PK));

			productFilter.Property = TokenAuthenticationStatusList.Codes.NotEnabled;
			licenceEnterpriseCollection.Load(licenceEnterpriseFilter.Filter);

			Assert("Should not contain Licence Enterprises with enterprise2", licenceEnterpriseCollection.Contains(enterprise1.PK));
			Assert("Should not contain Licence Enterprises with enterprise2", !licenceEnterpriseCollection.Contains(enterprise2.PK));
			Assert("Should not contain Licence Enterprises with enterprise2", licenceEnterpriseCollection.Contains(enterprise3.PK));

			productFilter.Property = TokenAuthenticationStatusList.Codes.ALL;
			licenceEnterpriseCollection.Load(licenceEnterpriseFilter.Filter);

			Assert("Should contains all Licence Enterprises", licenceEnterpriseCollection.Contains(enterprise1.PK));
			Assert("Should contains all Licence Enterprises", licenceEnterpriseCollection.Contains(enterprise2.PK));
			Assert("Should contains all Licence Enterprises", licenceEnterpriseCollection.Contains(enterprise3.PK));
		}

		#endregion

		#region FilterByEnterpriseCode
		public void TestFilterByEnterpriseCode()
		{
			ExpectedFilterResult filterByValues = new ExpectedFilterResult("ED", "N");
			CheckSearchPanelResult(filterByValues, "Enterprise Code");
		}

		public void TestFilterByEnterpriseID()
		{
			Licence1.LE_EnterpriseID = "E0001";
			Licence2.LE_EnterpriseID = "E0002";
			Licence3.LE_EnterpriseID = "X0001";
			Licence4.LE_EnterpriseID = "X0002";
			Factory.Save();
			ExpectedFilterResult filterByValues = new ExpectedFilterResult(Licence1.LE_EnterpriseID, "X000");
			CheckSearchPanelResult(filterByValues, "Enterprise ID");
		}

		#endregion
		#region FilterByOrgCode
		public void TestFilterByOrgCode()
		{
			ExpectedFilterResult filterByValues = new ExpectedFilterResult("EAGDA", "LD");
			CheckSearchPanelResult(filterByValues, "Organisation Code");
		}

		public void TestFilterByOrgCodeTwoOrgsSameEnterprise()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_FullName = "Owner Org";
			testOrg1.OH_Code = "AAAAAA";
			var lic1 = Factory.New<LicenceEnterprise>();
			lic1.LE_EnterpriseCode = "ENT";
			lic1.LE_OH = testOrg1.PK;
			var licCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licCompany.LC_LE = lic1.PK;
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.OH_FullName = "User Org";
			testOrg2.OH_Code = "BBBBBB";
			licCompany.LC_OH = testOrg2.PK;
			Factory.Save();
			((ModuleTextFilter)FilterBizObj["Organisation Code"]).Property = "AAAAAA";
			((ModuleTextFilter)FilterBizObj["Organisation Code"]).IsActive = true;
			((ModuleTextFilter)FilterBizObj["Organisation Code"]).ComparisonOperator = "contains";
			LicenceCollection.Load(FilterBizObj.Filter);
			AssertEquals("One record should be displayed", 1, LicenceCollection.Count);
			AssertCollectionContains("Correct Licence should be loaded", lic1, LicenceCollection);
			((ModuleTextFilter)FilterBizObj["Organisation Code"]).Property = "BBBBBB";
			((ModuleTextFilter)FilterBizObj["Organisation Code"]).IsActive = true;
			((ModuleTextFilter)FilterBizObj["Organisation Code"]).ComparisonOperator = "contains";
			LicenceCollection.Load(FilterBizObj.Filter);
			AssertEquals("One record should be displayed", 1, LicenceCollection.Count);
			AssertCollectionContains("Correct Licence should be loaded", lic1, LicenceCollection);
		}

		#endregion
		#region FilterByOrgName
		public void TestFilterByOrgName()
		{
			ExpectedFilterResult filterByValues = new ExpectedFilterResult("International", "Company");
			CheckSearchPanelResult(filterByValues, "Organisation Name");
		}

		#endregion
		#region FilterByReleaseRings
		public void TestFilterByReleaseRings()
		{
			((ModuleTextFilter)FilterBizObj["Release Ring"]).Property = ReleaseRings.Codes.ALP;
			((ModuleTextFilter)FilterBizObj["Release Ring"]).IsActive = true;
			LicenceCollection.Load(FilterBizObj.Filter);
			AssertEquals("One record should be displayed", 1, LicenceCollection.Count);
			AssertCollectionContains("Correct Licence should be loaded", Licence1, LicenceCollection);
			((ModuleTextFilter)FilterBizObj["Release Ring"]).Property = ReleaseRings.Codes.DPR;
			LicenceCollection.Load(FilterBizObj.Filter);
			AssertEquals("Two records should be displayed", 2, LicenceCollection.Count);
			AssertCollectionContains("Correct Licence should be loaded", Licence1, LicenceCollection);
			AssertCollectionContains("Correct Licence should be loaded", Licence2, LicenceCollection);
			((ModuleTextFilter)FilterBizObj["Release Ring"]).Property = ReleaseRings.Codes.STD;
			LicenceCollection.Load(FilterBizObj.Filter);
			AssertEquals("One record should be displayed", 1, LicenceCollection.Count);
			AssertCollectionContains("Correct Licence should be loaded", Licence3, LicenceCollection);
			((ModuleTextFilter)FilterBizObj["Release Ring"]).Property = ReleaseRings.Codes.GPR;
			LicenceCollection.Load(FilterBizObj.Filter);
			AssertEquals("One record should be displayed", 1, LicenceCollection.Count);
			AssertCollectionContains("Correct Licence should be loaded", Licence4, LicenceCollection);
			((ModuleTextFilter)FilterBizObj["Release Ring"]).Property = "ALL";
			LicenceCollection.Load(FilterBizObj.Filter);
			AssertEquals("All records should be displayed", 4, LicenceCollection.Count);
			AssertCollectionContains("Correct Licence should be loaded", Licence1, LicenceCollection);
			AssertCollectionContains("Correct Licence should be loaded", Licence2, LicenceCollection);
			AssertCollectionContains("Correct Licence should be loaded", Licence3, LicenceCollection);
			AssertCollectionContains("Correct Licence should be loaded", Licence4, LicenceCollection);
		}

		#endregion
		#region CheckSearchPanelResult
		void CheckSearchPanelResult(ExpectedFilterResult filterValues, string filterByOption)
		{
			// one result
			((ModuleTextFilter)FilterBizObj[filterByOption]).Property = filterValues.ValueForOnlyOneSearchResult;
			((ModuleTextFilter)FilterBizObj[filterByOption]).IsActive = true;
			((ModuleTextFilter)FilterBizObj[filterByOption]).ComparisonOperator = "contains";
			LicenceCollection.Load(FilterBizObj.Filter);
			AssertEquals("One record should be displayed", 1, LicenceCollection.Count);
			AssertCollectionContains("Correct Licence should be loaded", Licence1, LicenceCollection);
			// two results
			((ModuleTextFilter)FilterBizObj[filterByOption]).Property = filterValues.ValueForTwoSearchResults;
			((ModuleTextFilter)FilterBizObj[filterByOption]).IsActive = true;
			LicenceCollection.Load(FilterBizObj.Filter);
			AssertEquals("Two records should be displayed", 2, LicenceCollection.Count);
			AssertCollectionContains("Correct Licence should be loaded", Licence3, LicenceCollection);
			AssertCollectionContains("Correct Licence should be loaded", Licence4, LicenceCollection);
			// no results
			((ModuleTextFilter)FilterBizObj[filterByOption]).Property = "DLF";
			((ModuleTextFilter)FilterBizObj[filterByOption]).IsActive = true;
			LicenceCollection.Load(FilterBizObj.Filter);
			AssertEquals("No records should be displayed", 0, LicenceCollection.Count);
		}

		#endregion
		#region Implementation
		LicenceEnterpriseFilterBusinessObject FilterBizObj;
		LicenceEnterpriseCollection LicenceCollection;
		LicenceEnterprise Licence1;
		LicenceEnterprise Licence2;
		LicenceEnterprise Licence3;
		LicenceEnterprise Licence4;
		OrgHeader Org1;
		OrgHeader Org2;
		OrgHeader Org3;
		OrgHeader Org4;
		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(LicenceHeaderSchema.Constants.TableName);
			TestCaseHelper.ClearTable(LicenceCompanySchema.Constants.TableName);
			TestCaseHelper.ClearTable(LicenceDatabaseSchema.Constants.TableName);
			TestCaseHelper.ClearTable(LicenceEnterpriseSchema.Constants.TableName);
			LicenceEnterpriseCollection licEnterprises = new LicenceEnterpriseCollection(Factory);
			licEnterprises.Load();
			for (int z = 0; z < licEnterprises.Count - 1; z++)
			{
				licEnterprises[z].LE_EnterpriseCode = "ZZ" + z;
			}

			Org1 = Factory.NewWithValidTestData<OrgHeader>();
			Org1.OH_FullName = "Eagle Datamation International";
			Org1.OH_Code = "EAGDAN";
			Licence1 = Factory.New<LicenceEnterprise>();
			Licence1.LE_EnterpriseCode = "EDI";
			Licence1.LE_OH = Org1.PK;
			LicenceDatabase dB1 = Licence1.Databases.AddNew();
			dB1.LD_ReleaseRing = ReleaseRings.Codes.ALP;
			dB1.LD_ServerCode = "DB1";
			LicenceDatabase dB1a = Licence1.Databases.AddNew();
			dB1a.LD_ReleaseRing = ReleaseRings.Codes.DPR;
			dB1a.LD_ServerCode = "DBA";
			Org2 = Factory.NewWithValidTestData<OrgHeader>();
			Org2.OH_FullName = "Rohlig Australia";
			Org2.OH_Code = "ROHLIG";
			Licence2 = Factory.New<LicenceEnterprise>();
			Licence2.LE_EnterpriseCode = "ROH";
			Licence2.LE_OH = Org2.PK;
			LicenceDatabase dB2 = Licence2.Databases.AddNew();
			dB2.LD_ReleaseRing = ReleaseRings.Codes.DPR;
			dB2.LD_ServerCode = "DB2";
			Org3 = Factory.NewWithValidTestData<OrgHeader>();
			Org3.OH_FullName = "TNT the Company with a g";
			Org3.OH_Code = "TNTLDS";
			Licence3 = Factory.New<LicenceEnterprise>();
			Licence3.LE_EnterpriseCode = "TNG";
			Licence3.LE_OH = Org3.PK;
			LicenceDatabase dB3 = Licence3.Databases.AddNew();
			dB3.LD_ReleaseRing = ReleaseRings.Codes.STD;
			dB3.LD_ServerCode = "DB3";
			Org4 = Factory.NewWithValidTestData<OrgHeader>();
			Org4.OH_FullName = "DEN Some Company";
			Org4.OH_Code = "DENLDG";
			Licence4 = Factory.New<LicenceEnterprise>();
			Licence4.LE_EnterpriseCode = "DEN";
			Licence4.LE_OH = Org4.PK;
			LicenceDatabase dB4 = Licence4.Databases.AddNew();
			dB4.LD_ReleaseRing = ReleaseRings.Codes.GPR;
			dB4.LD_ServerCode = "DB4";
			FilterBizObj = new LicenceEnterpriseFilterBusinessObject();
			LicenceCollection = new LicenceEnterpriseCollection(Factory);
			Factory.Save();
		}

		class ExpectedFilterResult
		{
			public ExpectedFilterResult(string valueForOnlyOneSearchResult, string valueForTwoSearchResults)
			{
				ValueForOnlyOneSearchResult = valueForOnlyOneSearchResult;
				ValueForTwoSearchResults = valueForTwoSearchResults;
			}

			public readonly string ValueForOnlyOneSearchResult = "";
			public readonly string ValueForTwoSearchResults = "";
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new LicenceEnterpriseFilterBusinessObject();
		}
		#endregion
	}
}
