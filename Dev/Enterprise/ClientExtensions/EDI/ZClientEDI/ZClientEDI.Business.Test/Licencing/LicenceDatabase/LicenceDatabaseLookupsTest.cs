using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	class LicenceDatabaseLookupsTest : BusinessObjectLookupsTestCase
	{
		LicenceDatabase database;

		LicenceDatabase Database
		{
			get { return database ?? (database = Factory.New<LicenceDatabase>()); }
		}

		public void TestProductTypeList()
		{
			var list = new SystemProductCollection();

			list.AddNew("ZEU", "Zeus", true);
			list.AddNew("APL", "Apollo", true);
			list.AddNew("POS", "Poseidon", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var productTypeList = Database.Lookups.ProductTypeList;

			Assert("ProductTypeList contains 'ENT' code", productTypeList.ContainsCode("ENT"));
			Assert("ProductTypeList contains ProductAndModulesList codes", productTypeList.ContainsCode("ZEU"));
			Assert("ProductTypeList contains ProductAndModulesList codes", productTypeList.ContainsCode("APL"));
			Assert("ProductTypeList contains ProductAndModulesList codes", productTypeList.ContainsCode("POS"));
		}

		public void TestProductTypeListShouldBeAlphabeticalOrder()
		{
			var list = new SystemProductCollection();

			list.AddNew("ZEU", "Zeus", true);
			list.AddNew("APL", "ZApollo", true);
			list.AddNew("POS", "Poseidon", true);

			var emptyProductCollection = new SystemProductCollection();
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emptyProductCollection);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emptyProductCollection);

			var productTypeList = Database.Lookups.ProductTypeList;
			AssertEquals("Should be in alphabetical order", "APL", productTypeList[0].Code);
			AssertEquals("Should be in alphabetical order", "ZApollo", productTypeList[0].Description);
			AssertEquals("Should be in alphabetical order", ProductTypes.Codes.CargoWise, productTypeList[1].Code);
			AssertEquals("Should be in alphabetical order", ProductTypes.Descriptions.CargoWise, productTypeList[1].Description);
			AssertEquals("Should be in alphabetical order", ProductTypes.Codes.CargoWiseOne, productTypeList[2].Code);
			AssertEquals("Should be in alphabetical order", ProductTypes.Descriptions.CargoWiseOne, productTypeList[2].Description);
			AssertEquals("Should be in alphabetical order", ProductTypes.Codes.CargoWiseNext, productTypeList[3].Code);
			AssertEquals("Should be in alphabetical order", ProductTypes.Descriptions.CargoWiseNext, productTypeList[3].Description);
			AssertEquals("Should be in alphabetical order", ProductTypes.Codes.Enterprise, productTypeList[4].Code);
			AssertEquals("Should be in alphabetical order", ProductTypes.Descriptions.Enterprise, productTypeList[4].Description);
			AssertEquals("Should be in alphabetical order", "POS", productTypeList[5].Code);
			AssertEquals("Should be in alphabetical order", "Poseidon", productTypeList[5].Description);
			AssertEquals("Should be in alphabetical order", "ZEU", productTypeList[6].Code);
			AssertEquals("Should be in alphabetical order", "Zeus", productTypeList[6].Description);
		}

		public void TestDatabaseSecurityModes()
		{
			var modeList = Database.Lookups.DatabaseSecurityModesList;
			Assert("ModeList count > 0", modeList.Count > 0);
		}

		public void TestServerTypes()
		{
			DatabaseTypes serverTypeList = Database.Lookups.DatabaseTypesList;
			Assert("ModeList count > 0", serverTypeList.Count > 0);
		}

		public void TestCurrentVersions()
		{
			AssertNotNull(Database.Lookups.CurrentVersions);
		}

		public void TestAllLicenceEnterprises()
		{
			int originalCount = Factory.GetDatabaseCount(typeof(LicenceEnterprise));
			LicenceEnterprise ent1 = Factory.New<LicenceEnterprise>();
			LicenceEnterprise ent2 = Factory.New<LicenceEnterprise>();
			LicenceEnterprise ent3 = Factory.New<LicenceEnterprise>();

			EDIOrgHeader testHeader = Factory.New<EDIOrgHeader>();
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();

			database.Lookups.AllLicenceEnterprises.Load();
			AssertEquals("Collection has " + (originalCount + 4) + " items", (originalCount + 4), database.Lookups.AllLicenceEnterprises.Count);
		}

		public void TestLicEnterpriseAddresses()
		{
			EDIOrgHeader testHeader = Factory.New<EDIOrgHeader>();
			testHeader.OH_Code = "ABCXYZ";
			testHeader.MainAddress.OA_Address1 = "Address 1";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.LicenceEnterpriseCode = "ABC";
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();

			AssertEquals("0 Addresses in collection (DBOnlyQuery)", 0, database.Lookups.LicEnterpriseAddresses.Count);
			Factory.Save();

			AssertEquals("1 Address in collection", 1, new BusinessObjectFactory().Load<LicenceDatabase>(database.PK).Lookups.LicEnterpriseAddresses.Count);

			EDIOrgHeader testHeader2 = Factory.New<EDIOrgHeader>();
			testHeader2.OH_Code = "ABCZYX";
			testHeader2.MainAddress.OA_Address1 = "Address 2";
			testHeader2.CreateAndLoadLicenceForOrg();
			testHeader2.LicenceEnterpriseCode = "ABC";
			testHeader2.LicCompany.LicDatabases.Add(database);

			Factory.Save();

			AssertEquals("2 Addresses in collection", 2, new BusinessObjectFactory().Load<LicenceDatabase>(database.PK).Lookups.LicEnterpriseAddresses.Count);
		}

		public void TestLicEnterpriseContacts()
		{
			var licDB = new BusinessObjectFactory().New<LicenceDatabase>(); // When calling Factory.save later i dont want this guy
			AssertEquals("Empty Collection", 0, licDB.Lookups.LicEnterpriseContacts.Count);

			EDIOrgHeader testHeader = Factory.New<EDIOrgHeader>();
			testHeader.OH_Code = "ABCXYZ";
			testHeader.MainAddress.OA_Address1 = "Address 1";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.LicenceEnterpriseCode = "ABC";
			testHeader.Contacts.AddNew();
			licDB = testHeader.LicCompany.LicDatabases.AddNew();

			AssertEquals("0 Contacts in collection (DBOnlyQuery)", 0, licDB.Lookups.LicEnterpriseContacts.Count);
			Factory.Save();

			licDB.Lookups.LicEnterpriseContacts.Load();
			AssertEquals("1 Contact in collection", 1, licDB.Lookups.LicEnterpriseContacts.Count);

			EDIOrgHeader testHeader2 = Factory.New<EDIOrgHeader>();
			testHeader2.OH_Code = "ABCZYX";
			testHeader2.MainAddress.OA_Address1 = "Address 1";
			testHeader2.Contacts.AddNew();
			testHeader2.CreateAndLoadLicenceForOrg();
			testHeader2.LicenceEnterpriseCode = "ABC";

			Factory.Save();

			licDB.Lookups.LicEnterpriseContacts.Load();
			AssertEquals("2 Contacts in collection", 2, licDB.Lookups.LicEnterpriseContacts.Count);
		}

		public void TestSqlServerVersionDetailsLists()
		{
			AssertEquals("SqlServerEditions.GetType()", typeof(SqlServerEditionList), Database.Lookups.SqlServerEditions.GetType());
		}

		public void TestHostedLocations()
		{
			var list = new Enterprise.Registry.Business.CodeDescriptionBoolCollection();
			list.Add("NCW", (NoResString)"Not Hosted With CargoWise", false);
			list.Add("MEL", (NoResString)"Melbourne", true);
			list.Add("BNE", (NoResString)"Brisbane", true);
			list.Add("NY", (NoResString)"New York", true);
			EDIDataRegistry.Instance.DatabaseHostedLocations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			AssertEquals(4, Database.Lookups.HostedLocations.Count);
			AssertEquals(true, database.Lookups.HostedLocations.ContainsCode("NCW"));
			AssertEquals(true, database.Lookups.HostedLocations.ContainsCode("MEL"));
			AssertEquals(true, database.Lookups.HostedLocations.ContainsCode("BNE"));
			AssertEquals(true, database.Lookups.HostedLocations.ContainsCode("NY"));
		}

		public void TestProductionDatabaseCodeDescriptionPairList()
		{
			var db = Factory.New<LicenceDatabase>();
			AssertEquals(0, db.Lookups.ProductionDatabaseCodeDescriptionPairList.Count);
			db.Delete();

			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();
			Factory.Save();

			db = org.LicCompany.LicDatabases.AddNew();
			db.LD_ServerCode = "PR1";
			AssertEquals(0, db.Lookups.ProductionDatabaseCodeDescriptionPairList.Count);

			db.LD_LicenceType = DatabaseTypes.Codes.Production;

			var db2 = org.LicCompany.LicDatabases.AddNew();
			db2.LD_LicenceType = DatabaseTypes.Codes.Test;
			db2.LD_ServerCode = "TST";

			var actual = db2.Lookups.ProductionDatabaseCodeDescriptionPairList;
			AssertEquals("first item is blank in a category list", "", actual[0].Code);
			AssertEquals("first item is blank in a category list", "", actual[0].Description);
			AssertEquals("2nd item is the category ", DatabaseTypes.Descriptions.Production, actual[1].Code);
			AssertEquals("2nd item is the category ", new CodeDescriptionPair(DatabaseTypes.Descriptions.Production, ""), actual[1]);
			AssertEquals("PR1", actual[2].Code);
			AssertEquals("General Product Release", actual[2].Description);
			AssertEquals(3, actual.Count);

			var db3 = org.LicCompany.LicDatabases.AddNew();
			db3.LD_ServerCode = "PR2";
			db3.LD_LicenceType = DatabaseTypes.Codes.Production;

			Factory.Save();

			actual = db2.Lookups.ProductionDatabaseCodeDescriptionPairList;
			AssertEquals("PR1", actual[2].Code);
			AssertEquals("General Product Release", actual[2].Description);
			AssertEquals("PR2", actual[3].Code);
			AssertEquals("General Product Release", actual[3].Description);
			AssertEquals(4, actual.Count);
		}

		public void TestGetDatabaseDescription()
		{
			// null input will throw
			AssertExceptionThrown<NullReferenceException>(() => LicenceDatabaseLookups.GetDatabaseDescription(null));

			// Test without a current version
			var db = Factory.NewWithValidTestData<LicenceDatabase>();
			db.LD_ReleaseRing = "ALP";
			db.LD_HL_CurrentRunningVersion = ZGuid.Empty;
			AssertEquals("Alpha Release", LicenceDatabaseLookups.GetDatabaseDescription(db));
			AssertNotEquals(db.Description, LicenceDatabaseLookups.GetDatabaseDescription(db));

			// Test with current version
			var version = Factory.NewWithValidTestData<ReleaseBuild>();
			db.LD_HL_CurrentRunningVersion = version.PK;
			version.ExeVersion = "1.2.3.4";
			version.HL_ExeVersionDate = ZDateTime.BrettsBirthday;
			AssertEquals("Alpha Release - 1.2.3.4 - 18-Sep-71", LicenceDatabaseLookups.GetDatabaseDescription(db));
			AssertNotEquals(db.Description, LicenceDatabaseLookups.GetDatabaseDescription(db));
		}

		public void TestBillableFlagList()
		{
			var list = Database.Lookups.BillableFlagList;
			Assert(list.ContainsCode("Y"));
			Assert(list.ContainsCode("P"));
			Assert(list.ContainsCode("N"));
			Assert(list.ContainsCode("X"));
		}

		public void TestWebAccessOrgs()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence1.Database;
			licence1.Company.Header.OH_Code = "AA11Test";

			var licence2 = BillingTestHelper.CreateAnotherLicence(db, "TS1");
			licence2.Company.Header.OH_Code = "AA12Test";

			var licence3 = BillingTestHelper.CreateAnotherLicence(db, "TS2");
			licence3.Company.Header.OH_Code = "AA13Test";
			licence3.LA_IsActive = false;

			var licence4 = BillingTestHelper.CreateAnotherLicence(db, "TS3");
			licence4.Company.Header.OH_Code = "AA14Test";
			licence4.Company.Header.OH_IsActive = false;

			Factory.Save();

			var org1 = licence1.Company.Header;
			var org2 = licence2.Company.Header;
			var org3 = licence3.Company.Header;
			var org4 = licence4.Company.Header;

			var webAccessOrgs = db.Lookups.WebAccessOrgs;
			webAccessOrgs.Load();
			AssertEquals(3, webAccessOrgs.Count);
			AssertContainsExactElementsInAnyOrder(new OrgHeader[] { org1, org2, org3 }, webAccessOrgs.ToArray());
		}

		public void TestMultiTenantDatabaseProductTypes()
		{
			var mtdbProductTypes = Database.Lookups.MultiTenantDatabaseProductTypes;
			Assert("Count > 0", mtdbProductTypes.Count > 0);
			Assert("CSP", mtdbProductTypes.ContainsCode("CSP"));
			Assert("CCH", mtdbProductTypes.ContainsCode("CCH"));
		}

		public void TestTrustedSystems()
		{
			var db = new BusinessObjectFactory().New<LicenceDatabase>();
			db.LD_Product = "CW1";
			var sys = db.GetOrCreateTrustedSystem();
			AssertEquals(1, db.Lookups.TrustedSystems.Count);
			Assert(db.Lookups.TrustedSystems.Contains(sys));
		}
	}
}
