using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	[TestedType(typeof(EDIDedupOrgFilterBusinessObject))]
	public class EDIDedupOrgFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestEnterpriseCodeFilter()
		{
			var bizo = new EDIDedupOrgFilterBusinessObject();
			var enterpriseCodeFilter = bizo.ModuleFilters["Enterprise Code"] as ModuleGuidFilter;
			AssertNotNull(enterpriseCodeFilter);
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = "DDD123SYD1";
			org2.OH_Code = "DDD123SYD2";
			org3.OH_Code = "DDD123SYD3";
			org1.CreateAndLoadLicenceForOrg();
			org2.CreateAndLoadLicenceForOrg();
			org3.CreateAndLoadLicenceForOrg();
			org1.LicenceEnterpriseCode = "AAA";
			org2.LicenceEnterpriseCode = "AAB";
			org3.LicenceEnterpriseCode = "BAB";
			Factory.Save();
			enterpriseCodeFilter.Property = org1.LicEnterprise.PK;
			enterpriseCodeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			enterpriseCodeFilter.IsActive = true;
			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, enterpriseCodeFilter.Query);
			orgCollection.Load();
			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));
			enterpriseCodeFilter.Property = org1.LicEnterprise.PK;
			enterpriseCodeFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			enterpriseCodeFilter.IsActive = true;
			orgCollection = new OrgHeaderCollection(Factory, enterpriseCodeFilter.Query);
			orgCollection.Load();
			AssertCollectionNotContains(org1, orgCollection);
			AssertCollectionContains(org2, orgCollection);
			AssertCollectionContains(org3, orgCollection);
		}

		public void TestEnterpriseIDFilter()
		{
			var bizo = new EDIDedupOrgFilterBusinessObject();
			var enterpriseCodeFilter = bizo.ModuleFilters["Enterprise ID"] as ModuleGuidFilter;
			AssertNotNull(enterpriseCodeFilter);
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = "DDD123SYD1";
			org2.OH_Code = "DDD123SYD2";
			org3.OH_Code = "DDD123SYD3";
			org1.CreateAndLoadLicenceForOrg();
			org2.CreateAndLoadLicenceForOrg();
			org3.CreateAndLoadLicenceForOrg();
			org1.LicenceEnterpriseCode = "AAA";
			org2.LicenceEnterpriseCode = "AAB";
			org3.LicenceEnterpriseCode = "BAB";
			Factory.Save();
			enterpriseCodeFilter.Property = org1.LicEnterprise.PK;
			enterpriseCodeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			enterpriseCodeFilter.IsActive = true;
			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, enterpriseCodeFilter.Query);
			orgCollection.Load();
			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));
			enterpriseCodeFilter.Property = org1.LicEnterprise.PK;
			enterpriseCodeFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			enterpriseCodeFilter.IsActive = true;
			orgCollection = new OrgHeaderCollection(Factory, enterpriseCodeFilter.Query);
			orgCollection.Load();
			AssertCollectionNotContains(org1, orgCollection);
			AssertCollectionContains(org2, orgCollection);
			AssertCollectionContains(org3, orgCollection);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EDIDedupOrgFilterBusinessObject();
	}
}
