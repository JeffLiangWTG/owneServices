using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EDIOrganisationFilterBusinessObjectCore))]
	public class EDIOrganisationFilterBusinessObjectCoreTest : OrganisationFilterBusinessObjectTest
	{
		#region SQL Version and Dates

		public void TestSupportExpiryDateFilter()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithExpiryDate("Org1", ZDateTime.Today.AddDays(2));
			EDIOrgHeader org2 = GetOrgHeaderWithExpiryDate("Org2", ZDateTime.Today.AddDays(-33));
			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["Support Expiry Date"];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Next7Days;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1", true, orgCollection.Contains(org1));
			AssertEquals("Should not contain org2", false, orgCollection.Contains(org2));
		}

		public void TestSupportExpiryDateFilterWithNulls()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithoutExpiryDate("Org1");
			EDIOrgHeader org2 = GetOrgHeaderWithExpiryDate("Org2", ZDateTime.Today.AddDays(-33));
			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["Support Expiry Date"];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1", true, orgCollection.Contains(org1));
			AssertEquals("Should not contain org2", false, orgCollection.Contains(org2));
		}

		public void TestSqlServerEditionFilter()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithSqlServer("Org1", LicenceDatabaseSchema.LD_SQLEdition, "DES");
			EDIOrgHeader org2 = GetOrgHeaderWithSqlServer("Org2", LicenceDatabaseSchema.LD_SQLEdition, "DEV");
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["SQL Server Edition"];
			filter.IsActive = true;
			filter.Property = "DES";

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1.", true, orgCollection.Contains(org1));
			AssertEquals("Should not contain org2.", false, orgCollection.Contains(org2));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1.", !orgCollection.Contains(org1));
			Assert("Should contain org2.", orgCollection.Contains(org2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "ES";
			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1.", orgCollection.Contains(org1));
			Assert("Should not contain org2.", !orgCollection.Contains(org2));
		}

		public void TestSqlServerVersionFilter()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithSqlServer("Org1", LicenceDatabaseSchema.LD_SQLVersion, "SQL2000");
			EDIOrgHeader org2 = GetOrgHeaderWithSqlServer("Org2", LicenceDatabaseSchema.LD_SQLVersion, "SQL2005");
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["SQL Server Version"];
			filter.IsActive = true;
			filter.Property = "SQL2000";

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1.", true, orgCollection.Contains(org1));
			AssertEquals("Should not contain org2.", false, orgCollection.Contains(org2));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1.", !orgCollection.Contains(org1));
			Assert("Should contain org2.", orgCollection.Contains(org2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "2000";
			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1.", orgCollection.Contains(org1));
			Assert("Should not contain org2.", !orgCollection.Contains(org2));
		}

		#endregion

		#region Licence

		public void TestLicenceEdition()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithLicencedModule("Alex's Test Org 1", "COR", LicenceTypes.Codes.PUR);
			org1.LicCompany.LicHeadersForAllDatabases[0].LA_LicenceAdvStdOth = "XYZ";

			EDIOrgHeader org2 = GetOrgHeaderWithLicencedModule("Alex's Test Org 2", "FAX", LicenceTypes.Codes.PUR);
			org2.LicCompany.LicHeadersForAllDatabases[0].LA_LicenceAdvStdOth = "ABC";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Licence Edition Type"];
			filter.Property = "XYZ";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to not contain org2", !orgCollection.Contains(org2));

			filter.Property = "ABC";
			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to not contain org2", !orgCollection.Contains(org2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "BC";
			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
		}

		public void TestLicenceReleaseRing()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithLicencedModule("Alex's Test Org 1", "COR", LicenceTypes.Codes.PUR);
			org1.LicCompany.LicDatabases[0].LD_ReleaseRing = "DPR";

			EDIOrgHeader org2 = GetOrgHeaderWithLicencedModule("Alex's Test Org 2", "FAX", LicenceTypes.Codes.PUR);
			org2.LicCompany.LicDatabases[0].LD_ReleaseRing = "STD";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Release Ring"];
			filter.Property = "DPR";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to not contain org2", !orgCollection.Contains(org2));

			filter.Property = "STD";
			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to not contain org2", !orgCollection.Contains(org2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "TD";
			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
		}

		public void TestDatabaseLicenceType()
		{
			LicenceHeader lic1 = GetLicHeader("Org 1");
			LicenceHeader lic2 = GetLicHeader("Org 2");
			EDIOrgHeader org1 = lic1.Company.Header;
			EDIOrgHeader org2 = lic2.Company.Header;
			lic1.Database.LD_LicenceType = DatabaseTypes.Codes.Production;
			lic2.Database.LD_LicenceType = DatabaseTypes.Codes.Test;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Database Licence Type"];
			filter.Property = DatabaseTypes.Codes.Production;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to not contain org2", !orgCollection.Contains(org2));

			filter.Property = DatabaseTypes.Codes.Test;
			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to not contain org2", !orgCollection.Contains(org2));
		}

		public void TestDatabaseServerCode()
		{
			LicenceHeader lic1 = GetLicHeader("Org 1");
			LicenceHeader lic2 = GetLicHeader("Org 2");
			LicenceHeader lic3 = GetLicHeader("Org 3");
			EDIOrgHeader org1 = lic1.Company.Header;
			EDIOrgHeader org2 = lic2.Company.Header;
			EDIOrgHeader org3 = lic3.Company.Header;
			lic1.Database.LD_ServerCode = "AAA";
			lic2.Database.LD_ServerCode = "AAB";
			lic3.Database.LD_ServerCode = "BAB";
			Factory.Save();

			BusinessObjectFactory clearFactory = new BusinessObjectFactory();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Database Server Code"];
			filter.Property = "AAA";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(clearFactory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "AA";
			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "B";
			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));
		}

		public void TestDatabaseServerName()
		{
			LicenceHeader lic1 = GetLicHeader("Org 1");
			LicenceHeader lic2 = GetLicHeader("Org 2");
			LicenceHeader lic3 = GetLicHeader("Org 3");
			EDIOrgHeader org1 = lic1.Company.Header;
			EDIOrgHeader org2 = lic2.Company.Header;
			EDIOrgHeader org3 = lic3.Company.Header;
			lic1.Database.LD_ServerCode = "AAA";
			lic2.Database.LD_ServerCode = "AAB";
			lic3.Database.LD_ServerCode = "BAB";
			lic1.Database.LD_ReportedHostServerName = "ZZZ";
			lic2.Database.LD_ReportedHostServerName = "ZZX";
			lic3.Database.LD_ReportedHostServerName = "XZY";
			Factory.Save();

			BusinessObjectFactory clearFactory = new BusinessObjectFactory();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Database Server Name"];
			filter.Property = "ZZZ";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(clearFactory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "ZZ";
			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "X";
			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));
		}

		public void TestDatabaseName()
		{
			LicenceHeader lic1 = GetLicHeader("Org 1");
			LicenceHeader lic2 = GetLicHeader("Org 2");
			LicenceHeader lic3 = GetLicHeader("Org 3");
			EDIOrgHeader org1 = lic1.Company.Header;
			EDIOrgHeader org2 = lic2.Company.Header;
			EDIOrgHeader org3 = lic3.Company.Header;
			lic1.Database.LD_ServerCode = "AAA";
			lic2.Database.LD_ServerCode = "AAB";
			lic3.Database.LD_ServerCode = "BAB";
			lic1.Database.LD_ReportedHostDBName = "ZZZ";
			lic2.Database.LD_ReportedHostDBName = "ZZX";
			lic3.Database.LD_ReportedHostDBName = "XZY";
			Factory.Save();

			BusinessObjectFactory clearFactory = new BusinessObjectFactory();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Database Name"];
			filter.Property = "ZZZ";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(clearFactory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "ZZ";
			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "X";
			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));
		}

		public void TestDatabaseConnectionServer()
		{
			LicenceHeader lic1 = GetLicHeader("Org 1");
			LicenceHeader lic2 = GetLicHeader("Org 2");
			LicenceHeader lic3 = GetLicHeader("Org 3");
			EDIOrgHeader org1 = lic1.Company.Header;
			EDIOrgHeader org2 = lic2.Company.Header;
			EDIOrgHeader org3 = lic3.Company.Header;
			lic1.Database.LD_ServerCode = "AAA";
			lic2.Database.LD_ServerCode = "AAB";
			lic3.Database.LD_ServerCode = "BAB";
			lic1.Database.LD_HostConnectionServerName = "ZZZ";
			lic2.Database.LD_HostConnectionServerName = "ZZX";
			lic3.Database.LD_HostConnectionServerName = "XZY";
			Factory.Save();

			BusinessObjectFactory clearFactory = new BusinessObjectFactory();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Database Connection Server"];
			filter.Property = "ZZZ";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(clearFactory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "ZZ";
			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "X";
			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));
		}

		public void TestDatabaseRegistration()
		{
			LicenceHeader lic1 = GetLicHeader("Org 1");
			LicenceHeader lic2 = GetLicHeader("Org 2");
			LicenceHeader lic3 = GetLicHeader("Org 3");
			EDIOrgHeader org1 = lic1.Company.Header;
			EDIOrgHeader org2 = lic2.Company.Header;
			EDIOrgHeader org3 = lic3.Company.Header;
			lic1.Database.LD_ServerCode = "AAA";
			lic2.Database.LD_ServerCode = "AAB";
			lic3.Database.LD_ServerCode = "BAB";
			lic1.Database.LD_Status = DatabaseStatusList.Codes.NON;
			lic2.Database.LD_Status = DatabaseStatusList.Codes.REG;
			lic3.Database.LD_Status = DatabaseStatusList.Codes.Preregistered;
			Factory.Save();

			BusinessObjectFactory clearFactory = new BusinessObjectFactory();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Database Registration"];
			filter.Property = DatabaseStatusList.Codes.NON;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(clearFactory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = DatabaseStatusList.Codes.REG;
			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", orgCollection.Contains(org3));
		}

		public void TestOrganisationBelongToDatabaseSystemInfoGroup()
		{
			Assert("Expect category is databaseSystemInfo", FilterStripBizO["Database Server Name"].Category.ToString() == "Database System Info");
			Assert("Expect category is databaseSystemInfo", FilterStripBizO["Database Name"].Category.ToString() == "Database System Info");
			Assert("Expect category is databaseSystemInfo", FilterStripBizO["Database Connection Server"].Category.ToString() == "Database System Info");
			Assert("Expect category is databaseSystemInfo", FilterStripBizO["Database Registration"].Category.ToString() == "Database System Info");
			Assert("Expect category is databaseSystemInfo", FilterStripBizO["OS Name (Free Text)"].Category.ToString() == "Database System Info");
			Assert("Expect category is databaseSystemInfo", FilterStripBizO["OS Name"].Category.ToString() == "Database System Info");
			Assert("Expect category is databaseSystemInfo", FilterStripBizO["OS Version (Free Text)"].Category.ToString() == "Database System Info");
			Assert("Expect category is databaseSystemInfo", FilterStripBizO["OS Version"].Category.ToString() == "Database System Info");
			Assert("Expect category is databaseSystemInfo", FilterStripBizO["System Manufacturer (Free Text)"].Category.ToString() == "Database System Info");
			Assert("Expect category is databaseSystemInfo", FilterStripBizO["System Manufacturer"].Category.ToString() == "Database System Info");
			Assert("Expect category is databaseSystemInfo", FilterStripBizO["BIOS Release Date"].Category.ToString() == "Database System Info");
			Assert("Expect category is databaseSystemInfo", FilterStripBizO["Total Physical Memory in MB"].Category.ToString() == "Database System Info");
			Assert("Expect category is databaseSystemInfo", FilterStripBizO["Number of Processors"].Category.ToString() == "Database System Info");
			Assert("Expect category is databaseSystemInfo", FilterStripBizO["Processor Type (Free Text)"].Category.ToString() == "Database System Info");
			Assert("Expect category is databaseSystemInfo", FilterStripBizO["Processor Type"].Category.ToString() == "Database System Info");
			Assert("Expect category is databaseSystemInfo", FilterStripBizO["Processor Speed in Mhz"].Category.ToString() == "Database System Info");
			Assert("Expect category is databaseSystemInfo", FilterStripBizO["Is Virtual Machine"].Category.ToString() == "Database System Info");
		}

		public void TestSystemShutdownDate()
		{
			LicenceHeader lic1 = GetLicHeader("Org 1");
			LicenceHeader lic2 = GetLicHeader("Org 2");
			EDIOrgHeader org1 = lic1.Company.Header;
			EDIOrgHeader org2 = lic2.Company.Header;
			lic1.Database.LD_LicenceExpiry = ZDateTime.Now.AddDays(2);
			lic2.Database.LD_LicenceExpiry = ZDateTime.Empty;
			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["System Shutdown Date"];
			filter.IsActive = true;
			filter.PropertySearch = "Next 7 Days";

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1", true, orgCollection.Contains(org1));
			AssertEquals("Should not contain org2", false, orgCollection.Contains(org2));

			filter.PropertySearch = "Has No Date";

			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
		}

		public void TestLastHearbeat()
		{
			LicenceHeader lic1 = GetLicHeader("Org 1");
			LicenceHeader lic2 = GetLicHeader("Org 2");
			EDIOrgHeader org1 = lic1.Company.Header;
			EDIOrgHeader org2 = lic2.Company.Header;
			lic1.Database.LD_LastHeartbeat = ZDateTime.Now.AddDays(-2);
			lic2.Database.LD_LastHeartbeat = ZDateTime.Empty;
			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["Last Heartbeat"];
			filter.IsActive = true;
			filter.PropertySearch = "Last 7 Days";

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1", true, orgCollection.Contains(org1));
			AssertEquals("Should not contain org2", false, orgCollection.Contains(org2));

			filter.PropertySearch = "Has No Date";

			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
		}

		public void TestSiteLiveDate()
		{
			LicenceHeader lic1 = GetLicHeader("Org 1");
			LicenceHeader lic2 = GetLicHeader("Org 2");
			EDIOrgHeader org1 = lic1.Company.Header;
			EDIOrgHeader org2 = lic2.Company.Header;
			lic1.LA_SiteLiveDate = ZDateTime.Now.AddDays(-2);
			lic2.LA_SiteLiveDate = ZDateTime.Empty;
			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["Go-Live Completed Date"];
			filter.IsActive = true;
			filter.PropertySearch = "Last 7 Days";

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1", true, orgCollection.Contains(org1));
			AssertEquals("Should not contain org2", false, orgCollection.Contains(org2));

			filter.PropertySearch = "Has No Date";

			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
		}

		public void TestLastDiscrepancyChange()
		{
			LicenceHeader lic1 = GetLicHeader("Org 1");
			LicenceHeader lic2 = GetLicHeader("Org 2");
			EDIOrgHeader org1 = lic1.Company.Header;
			EDIOrgHeader org2 = lic2.Company.Header;
			lic1.LA_LastDiscrepancyChange = ZDateTime.Now.AddDays(-2);
			lic2.LA_LastDiscrepancyChange = ZDateTime.Empty;
			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["Last Discrepancy Change"];
			filter.IsActive = true;
			filter.PropertySearch = "Last 7 Days";

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1", true, orgCollection.Contains(org1));
			AssertEquals("Should not contain org2", false, orgCollection.Contains(org2));

			filter.PropertySearch = "Has No Date";

			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
		}

		public void TestLicenceCompanyCodeFilter()
		{
			LicenceHeader lic1 = GetLicHeader("Org 1");
			LicenceHeader lic2 = GetLicHeader("Org 2");
			LicenceHeader lic3 = GetLicHeader("Org 3");
			EDIOrgHeader org1 = lic1.Company.Header;
			EDIOrgHeader org2 = lic2.Company.Header;
			EDIOrgHeader org3 = lic3.Company.Header;
			lic1.Company.LC_CompanyCode = "AAA";
			lic2.Company.LC_CompanyCode = "AAB";
			lic3.Company.LC_CompanyCode = "BAB";
			Factory.Save();

			BusinessObjectFactory clearFactory = new BusinessObjectFactory();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Licence Company Code"];
			filter.Property = "AAA";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(clearFactory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "AA";
			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "B";
			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));
		}

		public void TestModuleLicenceType()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithLicencedModule("Alex's Test Org 1", "COR", LicenceTypes.Codes.PUR);
			EDIOrgHeader org2 = GetOrgHeaderWithLicencedModule("Alex's Test Org 2", "FAX", LicenceTypes.Codes.NON);

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Module Fee Basis"];
			filter.Property = "PUR";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			BusinessObjectFactory clearFactory = new BusinessObjectFactory();

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(clearFactory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "P";
			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestModuleLicenceType_OrGroup()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithLicencedModule("Test Org 1", "COR", LicenceTypes.Codes.PUR);
			EDIOrgHeader org2 = GetOrgHeaderWithLicencedModule("Test Org 2", "FAX", LicenceTypes.Codes.TRI);
			EDIOrgHeader org3 = GetOrgHeaderWithLicencedModule("Test Org 3", "ACC", LicenceTypes.Codes.REN);
			Factory.Save();

			ModuleTextFilter filter1 = (ModuleTextFilter)FilterStripBizO["Module Fee Basis"];
			filter1.Property = "PUR";
			filter1.OrCategory = FilterOrCategory.Red;
			filter1.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter1.IsActive = true;

			ModuleTextFilter filter2 = (ModuleTextFilter)FilterStripBizO.CreateDuplicateFor("Module Fee Basis");
			filter2.Property = "TRI";
			filter2.OrCategory = FilterOrCategory.Red;
			filter2.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter2.IsActive = true;

			BusinessObjectFactory clearFactory = new BusinessObjectFactory();

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(clearFactory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));
		}

		public void TestModuleAndDatabase()
		{
			var licHeader1 = GetLicHeader("Test Org 1", "COR", LicenceTypes.Codes.PUR);
			var licHeader2 = GetLicHeader("Test Org 2", "COR", LicenceTypes.Codes.PUR);
			var licHeader3 = GetLicHeader("Test Org 3", "COR", LicenceTypes.Codes.PUR);
			var licHeader1b = AddDatabase(licHeader1.Company, "COR", LicenceTypes.Codes.REN);
			licHeader1b.Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			licHeader3.Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			Factory.Save();

			ModuleTextFilter filter1 = (ModuleTextFilter)FilterStripBizO["Module Fee Basis"];
			filter1.Property = "PUR";
			filter1.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter1.IsActive = true;

			ModuleTextFilter filter2 = (ModuleTextFilter)FilterStripBizO["Database Licence Type"];
			filter2.Property = DatabaseTypes.Codes.Test;
			filter2.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter2.IsActive = true;

			BusinessObjectFactory clearFactory = new BusinessObjectFactory();

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(clearFactory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to not contain org1", !orgCollection.Contains(licHeader1.Company.Header));
			Assert("Expect collection to not contain org2", !orgCollection.Contains(licHeader2.Company.Header));
			Assert("Expect collection to contain org3", orgCollection.Contains(licHeader3.Company.Header));
		}

		public void TestLicenceInSync()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithLicencedModule("Alex's Test Org 1", "COR", LicenceTypes.Codes.PUR);
			EDIOrgHeader org2 = GetOrgHeaderWithLicencedModule("Alex's Test Org 2", "COR", LicenceTypes.Codes.PUR);

			org1.LicCompany.LicHeadersForAllDatabases[0].LA_LastLicenceCheckInSync = true;
			org2.LicCompany.LicHeadersForAllDatabases[0].LA_LastLicenceCheckInSync = false;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Licence In Sync"];
			filter.Property = "YES";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			BusinessObjectFactory clearFactory = new BusinessObjectFactory();

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(clearFactory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		#endregion

		#region Versions

		[StressTest]
		public void TestCurrentVersion()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithVersion("Alex's Test Org 1", new ZDateTime(2014, 11, 20), 1, 4, 5408, 51);
			EDIOrgHeader org2 = GetOrgHeaderWithVersion("Alex's Test Org 2", new ZDateTime(2014, 11, 20), 14, 9, 4, 229);
			EDIOrgHeader org3 = GetOrgHeaderWithVersion("Alex's Test Org 3", new ZDateTime(2014, 11, 20), 14, 11, 20, 3);
			ZGuid build1PK = org1.LicCompany.LicDatabases[0].LD_HL_CurrentRunningVersion;
			ZGuid build2PK = org2.LicCompany.LicDatabases[0].LD_HL_CurrentRunningVersion;
			ZGuid build3PK = org3.LicCompany.LicDatabases[0].LD_HL_CurrentRunningVersion;

			Factory.Save();

			ModuleGuidsFilter filter = (ModuleGuidsFilter)FilterStripBizO["Current Version"];
			filter.Property1 = ZGuid.Empty;
			filter.Property2 = ZGuid.Empty;
			filter.IsActive = true;

			BusinessObjectFactory clearFactory = new BusinessObjectFactory();

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(clearFactory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));

			filter.Property1 = build1PK;
			filter.Property2 = build3PK;

			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));

			filter.Property1 = ZGuid.Empty;
			filter.Property2 = build1PK;

			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.Property1 = build3PK;
			filter.Property2 = ZGuid.Empty;

			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));
		}

		[StressTest]
		public void TestSentVersion()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithVersion("Alex's Test Org 1", new ZDateTime(2014, 11, 20), 1, 4, 5408, 51);
			EDIOrgHeader org2 = GetOrgHeaderWithVersion("Alex's Test Org 2", new ZDateTime(2014, 11, 20), 14, 9, 4, 229);
			EDIOrgHeader org3 = GetOrgHeaderWithVersion("Alex's Test Org 3", new ZDateTime(2014, 11, 20), 14, 11, 20, 3);
			ZGuid build1PK = org1.LicCompany.LicDatabases[0].LD_HL_CurrentSentVersion;
			ZGuid build2PK = org2.LicCompany.LicDatabases[0].LD_HL_CurrentSentVersion;
			ZGuid build3PK = org3.LicCompany.LicDatabases[0].LD_HL_CurrentSentVersion;

			Factory.Save();

			ModuleGuidsFilter filter = (ModuleGuidsFilter)FilterStripBizO["Sent Version"];
			filter.Property1 = ZGuid.Empty;
			filter.Property2 = ZGuid.Empty;
			filter.IsActive = true;

			BusinessObjectFactory clearFactory = new BusinessObjectFactory();

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(clearFactory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));

			filter.Property1 = build1PK;
			filter.Property2 = build3PK;

			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));

			filter.Property1 = ZGuid.Empty;
			filter.Property2 = build1PK;

			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.Property1 = build3PK;
			filter.Property2 = ZGuid.Empty;

			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));
		}

		#endregion

		#region Database Server Info

		public void TestOSNameFilter()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithSqlServer("Org1", LicenceDatabaseSchema.LD_OSName, "Microsoft Windows 7 Enterprise");
			EDIOrgHeader org2 = GetOrgHeaderWithSqlServer("Org2", LicenceDatabaseSchema.LD_OSName, "Microsoft Windows Server 2008 R2 Enterprise");
			EDIOrgHeader org3 = GetOrgHeaderWithSqlServer("Org3", LicenceDatabaseSchema.LD_OSName, "Microsoft Windows 7 Ultimate");
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["OS Name"];
			filter.IsActive = true;
			filter.Property = "Microsoft Windows 7";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1", true, orgCollection.Contains(org1));
			AssertEquals("Should not contain org2", false, orgCollection.Contains(org2));
			AssertEquals("Should contain org3", true, orgCollection.Contains(org3));

			filter.Property = "Enterprise";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1", true, orgCollection.Contains(org1));
			AssertEquals("Should contain org2", true, orgCollection.Contains(org2));
			AssertEquals("Should not contain org3", false, orgCollection.Contains(org3));

			filter.Property = "Microsoft Windows 7 Enterprise";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1", true, orgCollection.Contains(org1));
			AssertEquals("Should not contain org2", false, orgCollection.Contains(org2));
			AssertEquals("Should not contain org3", false, orgCollection.Contains(org3));
		}

		public void TestOSVersionFilter()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithSqlServer("Org1", LicenceDatabaseSchema.LD_OSVersion, "6.1.7600 N/A Build 7600");
			EDIOrgHeader org2 = GetOrgHeaderWithSqlServer("Org2", LicenceDatabaseSchema.LD_OSVersion, "5.2.3790 Service Pack 2 Build 3790");
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["OS Version"];
			filter.IsActive = true;
			filter.Property = "6.1.7600";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1", true, orgCollection.Contains(org1));
			AssertEquals("Should not contain org2", false, orgCollection.Contains(org2));

			filter.Property = "3790";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should not contain org1", false, orgCollection.Contains(org1));
			AssertEquals("Should contain org2", true, orgCollection.Contains(org2));
		}

		public void TestSystemManufacturerFilter()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithSqlServer("Org1", LicenceDatabaseSchema.LD_SystemManufacturer, "Supermicro");
			EDIOrgHeader org2 = GetOrgHeaderWithSqlServer("Org2", LicenceDatabaseSchema.LD_SystemManufacturer, "Microsoft Corporation");
			EDIOrgHeader org3 = GetOrgHeaderWithSqlServer("Org3", LicenceDatabaseSchema.LD_SystemManufacturer, "Gigabyte Technology Co., Ltd.");
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["System Manufacturer"];
			filter.IsActive = true;
			filter.Property = "Microsoft";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should not contain org1", false, orgCollection.Contains(org1));
			AssertEquals("Should contain org2", true, orgCollection.Contains(org2));
			AssertEquals("Should not contain org3", false, orgCollection.Contains(org3));

			filter.Property = "Gigabyte";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should not contain org1", false, orgCollection.Contains(org1));
			AssertEquals("Should not contain org2", false, orgCollection.Contains(org2));
			AssertEquals("Should contain org3", true, orgCollection.Contains(org3));
		}

		public void TestBIOSDateFilter()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithSqlServer("Org1", LicenceDatabaseSchema.LD_BIOSDate, new ZDateTime(2009, 3, 19));
			EDIOrgHeader org2 = GetOrgHeaderWithSqlServer("Org2", LicenceDatabaseSchema.LD_BIOSDate, new ZDateTime(2008, 6, 18));
			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["BIOS Release Date"];
			filter.IsActive = true;
			filter.PropertySearch = "Date range";
			filter.Property1 = new ZDateTime(2009, 1, 1);
			filter.Property2 = new ZDateTime(2010, 1, 1);

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1", true, orgCollection.Contains(org1));
			AssertEquals("Should not contain org2", false, orgCollection.Contains(org2));

			filter.Property1 = new ZDateTime(2008, 1, 1);
			filter.Property2 = new ZDateTime(2009, 1, 1);
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should not contain org1", false, orgCollection.Contains(org1));
			AssertEquals("Should contain org2", true, orgCollection.Contains(org2));
		}

		public void TestTotalPhysicalMemoryFilter()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithSqlServer("Org1", LicenceDatabaseSchema.LD_TotalPhysicalMemoryMB, 8190);
			EDIOrgHeader org2 = GetOrgHeaderWithSqlServer("Org2", LicenceDatabaseSchema.LD_TotalPhysicalMemoryMB, 4096);
			EDIOrgHeader org3 = GetOrgHeaderWithSqlServer("Org3", LicenceDatabaseSchema.LD_TotalPhysicalMemoryMB, 16379);
			Factory.Save();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)FilterStripBizO["Total Physical Memory in MB"];
			filter.IsActive = true;
			filter.Property1 = 4000;
			filter.Property2 = 8000;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should not contain org1", false, orgCollection.Contains(org1));
			AssertEquals("Should contain org2", true, orgCollection.Contains(org2));
			AssertEquals("Should not contain org3", false, orgCollection.Contains(org3));

			filter.Property1 = 8000;
			filter.Property2 = 10000;

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1", true, orgCollection.Contains(org1));
			AssertEquals("Should not contain org2", false, orgCollection.Contains(org2));
			AssertEquals("Should not contain org3", false, orgCollection.Contains(org3));
		}

		public void TestNoOfProcessorCores()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithSqlServer("Org1", LicenceDatabaseSchema.LD_NoOfProcessorCores, 1);
			EDIOrgHeader org2 = GetOrgHeaderWithSqlServer("Org2", LicenceDatabaseSchema.LD_NoOfProcessorCores, 4);
			Factory.Save();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)FilterStripBizO["Number of Processors"];
			filter.IsActive = true;
			filter.Property1 = 0;
			filter.Property2 = 1;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1", true, orgCollection.Contains(org1));
			AssertEquals("Should not contain org2", false, orgCollection.Contains(org2));

			filter.Property1 = 1;
			filter.Property2 = 4;

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1", true, orgCollection.Contains(org1));
			AssertEquals("Should contain org2", true, orgCollection.Contains(org2));
		}

		public void TestProcessorType()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithSqlServer("Org1", LicenceDatabaseSchema.LD_ProcessorType, "EM64T Family 6 Model 15 Stepping 11 GenuineIntel");
			EDIOrgHeader org2 = GetOrgHeaderWithSqlServer("Org2", LicenceDatabaseSchema.LD_ProcessorType, "Intel64 Family 6 Model 26 Stepping 5 GenuineIntel");
			EDIOrgHeader org3 = GetOrgHeaderWithSqlServer("Org3", LicenceDatabaseSchema.LD_ProcessorType, "Intel64 Family 6 Model 23 Stepping 10 GenuineIntel");

			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Processor Type"];
			filter.IsActive = true;
			filter.Property = "Intel64";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should not contain org1", false, orgCollection.Contains(org1));
			AssertEquals("Should contain org2", true, orgCollection.Contains(org2));
			AssertEquals("Should contain org3", true, orgCollection.Contains(org3));

			filter.Property = "Family 6";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1", true, orgCollection.Contains(org1));
			AssertEquals("Should contain org2", true, orgCollection.Contains(org2));
			AssertEquals("Should contain org3", true, orgCollection.Contains(org3));

			filter.Property = "Model 26";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should not contain org1", false, orgCollection.Contains(org1));
			AssertEquals("Should contain org2", true, orgCollection.Contains(org2));
			AssertEquals("Should not contain org3", false, orgCollection.Contains(org3));
		}

		public void TestProcessorSpeed()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithSqlServer("Org1", LicenceDatabaseSchema.LD_ProcessorSpeedMHz, (decimal)2394.00);
			EDIOrgHeader org2 = GetOrgHeaderWithSqlServer("Org2", LicenceDatabaseSchema.LD_ProcessorSpeedMHz, (decimal)2260.00);
			EDIOrgHeader org3 = GetOrgHeaderWithSqlServer("Org3", LicenceDatabaseSchema.LD_ProcessorSpeedMHz, (decimal)3000.00);
			Factory.Save();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)FilterStripBizO["Processor Speed in Mhz"];
			filter.IsActive = true;
			filter.Property1 = (decimal)2000.00;
			filter.Property2 = (decimal)3000.00;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1", true, orgCollection.Contains(org1));
			AssertEquals("Should contain org2", true, orgCollection.Contains(org2));
			AssertEquals("Should contain org3", true, orgCollection.Contains(org3));

			filter.Property1 = (decimal)2000.00;
			filter.Property2 = (decimal)2500.00;

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1", true, orgCollection.Contains(org1));
			AssertEquals("Should contain org2", true, orgCollection.Contains(org2));
			AssertEquals("Should not contain org3", false, orgCollection.Contains(org3));
		}

		public void TestIsVirtualMachineFilter()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithSqlServer("Org1", LicenceDatabaseSchema.LD_VirtualMachineDetected, true);
			EDIOrgHeader org2 = GetOrgHeaderWithSqlServer("Org2", LicenceDatabaseSchema.LD_VirtualMachineDetected, false);
			EDIOrgHeader org3 = GetOrgHeaderWithSqlServer("Org3", LicenceDatabaseSchema.LD_VirtualMachineDetected, false);
			Factory.Save();

			ModuleFlagsFilter filter = (ModuleFlagsFilter)FilterStripBizO["Is Virtual Machine"];
			filter.IsActive = true;
			filter.Property0 = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should contain org1", true, orgCollection.Contains(org1));
			AssertEquals("Should not contain org2", false, orgCollection.Contains(org2));
			AssertEquals("Should not contain org3", false, orgCollection.Contains(org3));

			filter.Property0 = false;

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should not contain org1", false, orgCollection.Contains(org1));
			AssertEquals("Should contain org2", true, orgCollection.Contains(org2));
			AssertEquals("Should contain org3", true, orgCollection.Contains(org3));
		}

		public void TestNumberRangeFilter()
		{
			var filter1 = (ModuleNumberRangeFilter)FilterStripBizO["Total Physical Memory in MB"];
			filter1.Property1 = 99999999999m;
			filter1.Property2 = 99999999999m;
			filter1.Validation.ValidateAll();
			AssertHasError(filter1.Property1Info, "Please enter a value less than or equal to 2,147,483,647.");

			var filter2 = (ModuleNumberRangeFilter)FilterStripBizO["Number of Processors"];
			filter2.Property1 = 99999999999m;
			filter2.Property2 = 99999999999m;
			filter2.Validation.ValidateAll();
			AssertHasError(filter2.Property1Info, "Please enter a value less than or equal to 2,147,483,647.");

			var filter3 = (ModuleNumberRangeFilter)FilterStripBizO["Number of Processors"];
			filter3.IsActive = true;
			filter3.Property1 = 1234567890m;
			filter3.Property2 = 1234567890m;
			filter3.Validation.ValidateAll();
			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertEquals(0, orgCollection.Count);
		}

		#endregion
		 
		#region Enterprise Code

		[StressTest]
		public void TestEnterpriseCodeFilter()
		{
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = "DDD123SYD1";
			org2.OH_Code = "DDD123SYD2";
			org3.OH_Code = "DDD123SYD3";
			org4.OH_Code = "DDD123SYD4";
			org1.CreateAndLoadLicenceForOrg();
			org2.CreateAndLoadLicenceForOrg();
			org3.CreateAndLoadLicenceForOrg();
			org1.LicenceEnterpriseCode = "AAA";
			org2.LicenceEnterpriseCode = "AAB";
			org3.LicenceEnterpriseCode = "BAB";
			Factory.Save();

			ModuleTextFilter codeFilter = (ModuleTextFilter)FilterStripBizO["Code"];
			codeFilter.IsActive = true;
			codeFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			codeFilter.Property = "DDD123SYD";

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Enterprise Code"];
			filter.Property = org1.LicEnterprise.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.Property = org1.LicEnterprise.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertCollectionNotContains(org1, orgCollection);
			AssertCollectionContains(org2, orgCollection);
			AssertCollectionContains(org3, orgCollection);
			AssertCollectionNotContains(org4, orgCollection);

			filter.Property = ZGuid.Empty;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter.IsActive = true;

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertCollectionNotContains(org1, orgCollection);
			AssertCollectionNotContains(org2, orgCollection);
			AssertCollectionNotContains(org3, orgCollection);
			AssertCollectionContains(org4, orgCollection);

			filter.Property = ZGuid.Empty;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.IsActive = true;

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertCollectionContains(org1, orgCollection);
			AssertCollectionContains(org2, orgCollection);
			AssertCollectionContains(org3, orgCollection);
			AssertCollectionNotContains(org4, orgCollection);
		}

		[StressTest]
		public void TestEnterpriseIDFilter()
		{
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = "DDD123SYD1";
			org2.OH_Code = "DDD123SYD2";
			org3.OH_Code = "DDD123SYD3";
			org4.OH_Code = "DDD123SYD4";
			org1.CreateAndLoadLicenceForOrg();
			org2.CreateAndLoadLicenceForOrg();
			org3.CreateAndLoadLicenceForOrg();
			org1.LicenceEnterpriseCode = "AAA";
			org2.LicenceEnterpriseCode = "AAB";
			org3.LicenceEnterpriseCode = "BAB";
			Factory.Save();

			ModuleTextFilter codeFilter = (ModuleTextFilter)FilterStripBizO["Code"];
			codeFilter.IsActive = true;
			codeFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			codeFilter.Property = "DDD123SYD";

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Enterprise ID"];
			filter.Property = org1.LicEnterprise.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.Property = org1.LicEnterprise.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertCollectionNotContains(org1, orgCollection);
			AssertCollectionContains(org2, orgCollection);
			AssertCollectionContains(org3, orgCollection);
			AssertCollectionNotContains(org4, orgCollection);

			filter.Property = ZGuid.Empty;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter.IsActive = true;

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertCollectionNotContains(org1, orgCollection);
			AssertCollectionNotContains(org2, orgCollection);
			AssertCollectionNotContains(org3, orgCollection);
			AssertCollectionContains(org4, orgCollection);

			filter.Property = ZGuid.Empty;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.IsActive = true;

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertCollectionContains(org1, orgCollection);
			AssertCollectionContains(org2, orgCollection);
			AssertCollectionContains(org3, orgCollection);
			AssertCollectionNotContains(org4, orgCollection);
		}

		#endregion

		#region Original Org Name And Code

		public void TestOriginalOrgNameAndCode()
		{
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.BrandsOrRelatedNames.AddNew().P1_RelatedName = "Original Name 1";
			org1.BrandsOrRelatedNames.AddNew().P1_RelatedName = "ORICODSYD";

			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.BrandsOrRelatedNames.AddNew().P1_RelatedName = "Test Org Name 2";
			org2.BrandsOrRelatedNames.AddNew().P1_RelatedName = "TSTCODMEL";

			EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org3.BrandsOrRelatedNames.AddNew().P1_RelatedName = "Test Org Name 3";
			org3.BrandsOrRelatedNames.AddNew().P1_RelatedName = "ORICODMEL";

			Factory.Save();

			BusinessObjectFactory clearFactory = new BusinessObjectFactory();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Original Name/Code"];
			filter.Property = "Original Name 1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(clearFactory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "ORICOD";
			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "MEL";
			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));
		}

		#endregion

		#region Product

		public void TestProductFilter()
		{
			ModuleTextFilter textFilter = (ModuleTextFilter)FilterStripBizO["Code"];
			textFilter.Property = "AAAA";
			textFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			textFilter.IsActive = true;

			var list = new SystemProductCollection();

			list.AddNew("ELM", (NoResString)"Elmo", true);
			list.AddNew("PEP", (NoResString)"PeppaPig", true);
			list.AddNew("CHM", (NoResString)"Chima", true);

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "AAAAELMO1";
			org1.OH_FullName = "ELMO's Org";
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "AAAABRNY1";
			org2.OH_FullName = "Barney's Org";
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "AAAAVADER";
			org3.OH_FullName = "Darth's Org";
			Factory.Save();

			LicenceCompany lc1 = Factory.NewWithValidTestData<LicenceCompany>();
			lc1.LC_CompanyCode = "EL1";
			lc1.LC_OH = org1.PK;
			LicenceCompany lc2 = Factory.NewWithValidTestData<LicenceCompany>();
			lc2.LC_CompanyCode = "PE1";
			lc2.LC_OH = org2.PK;
			LicenceCompany lc3 = Factory.NewWithValidTestData<LicenceCompany>();
			lc3.LC_CompanyCode = "VD1";
			lc3.LC_OH = org3.PK;

			LicenceHeader la1 = Factory.NewWithValidTestData<LicenceHeader>();
			la1.LA_LC = lc1.PK;
			la1.Database.LD_Product = "ELM";
			LicenceHeader la2 = Factory.NewWithValidTestData<LicenceHeader>();
			la2.LA_LC = lc2.PK;
			la2.Database.LD_Product = "PEP";
			LicenceHeader la3 = Factory.NewWithValidTestData<LicenceHeader>();
			la3.LA_LC = lc3.PK;
			la3.Database.LD_Product = "CHM";

			Factory.Save();

			var productFilter = (ModuleTextFilter)FilterStripBizO["Product"];
			var productCollection = new OrgHeaderCollection(Factory);

			productFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			productFilter.Property = "ELM";
			productFilter.IsActive = true;
			productCollection.Load(FilterStripBizO.Filter);

			Assert("Should only contrain headers with product 'ELM'", productCollection.Contains(org1.PK));
			Assert("Should only contrain headers with product 'ELM'", !productCollection.Contains(org2.PK));
			Assert("Should only contrain headers with product 'ELM'", !productCollection.Contains(org3.PK));

			productFilter.Property = "PEP";
			productFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			productCollection.Load(FilterStripBizO.Filter);

			Assert("Should only contrain headers with product 'PEP'", productCollection.Contains(org1.PK));
			Assert("Should only contrain headers with product 'PEP'", !productCollection.Contains(org2.PK));
			Assert("Should only contrain headers with product 'PEP'", productCollection.Contains(org3.PK));

			productFilter.IsActive = false;
			productFilter.Property = "CHM";
			productFilter.IsActive = true;
			productFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			productCollection.Load(FilterStripBizO.Filter);

			Assert("Should contrain headers with product 'CHM'", !productCollection.Contains(org1.PK));
			Assert("Should contrain headers with product 'CHM'", !productCollection.Contains(org2.PK));
			Assert("Should contrain headers with product 'CHM'", productCollection.Contains(org3.PK));
		}
		#endregion

		public void TestLicenceEmail()
		{
			EDIOrgHeader org1 = GetOrgHeaderWithSqlServer("Org1", LicenceDatabaseSchema.LD_PublicEmailAddressForUpdate, "enterprise@org1.com");
			EDIOrgHeader org2 = GetOrgHeaderWithSqlServer("Org2", LicenceDatabaseSchema.LD_PublicEmailAddressForUpdate, "enterprise@org2.com");
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Licence Email"];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "enterprise@org2.com";

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should not contain org1.", false, orgCollection.Contains(org1));
			AssertEquals("Should contain org2.", true, orgCollection.Contains(org2));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "enterprise";
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertEquals("Should contain org1.", true, orgCollection.Contains(org1));
			AssertEquals("Should contain org2.", true, orgCollection.Contains(org2));
		}

		public void TestDatabaseActive()
		{
			LicenceHeader lic1 = GetLicHeader("Org 1");
			LicenceHeader lic2 = GetLicHeader("Org 2");
			EDIOrgHeader org1 = lic1.Company.Header;
			EDIOrgHeader org2 = lic2.Company.Header;
			lic1.Database.LD_IsActive = true;
			lic2.Database.LD_IsActive = false;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Database Active"];
			filter.Property = "Active";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to not contain org2", !orgCollection.Contains(org2));

			filter.Property = "Inactive";

			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));

			filter.Property = "All";
			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
		}

		public void TestLicenceActive()
		{
			LicenceHeader lic1 = GetLicHeader("Org 1");
			LicenceHeader lic2 = GetLicHeader("Org 2");
			EDIOrgHeader org1 = lic1.Company.Header;
			EDIOrgHeader org2 = lic2.Company.Header;
			lic1.LA_IsActive = true;
			lic2.LA_IsActive = false;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Licence Active on Database"];
			filter.Property = "Active";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to not contain org2", !orgCollection.Contains(org2));

			filter.Property = "Inactive";

			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));

			filter.Property = "All";
			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
		}

		public void TestRelationshipManagerPrimaryFilter()
		{
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ZE1";
			OrgStaffAssignments assignment = org1.StaffAssignments.AddNew();
			assignment.O8_OH = org1.PK;
			assignment.O8_Department = "ALL";
			assignment.O8_GS_NKPersonResponsible = staff1.GS_Code;
			assignment.O8_Role = "RM1";

			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ZE2";
			OrgStaffAssignments assignment2 = org2.StaffAssignments.AddNew();
			assignment2.O8_OH = org2.PK;
			assignment2.O8_Department = "ALL";
			assignment2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			assignment2.O8_Role = "RM1";

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Key Account Manager - Primary"];
			filter.IsActive = true;
			filter.Property = staff1.PK;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should not contain org2.", false, orgCollection.Contains(org2));
			AssertEquals("Should contain org1.", true, orgCollection.Contains(org1));

			filter.Property = staff2.PK;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertEquals("Should not contain org1.", false, orgCollection.Contains(org1));
			AssertEquals("Should contain org2.", true, orgCollection.Contains(org2));
		}

		public void TestRelationshipManagerSecondaryFilter()
		{
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ZE1";
			OrgStaffAssignments assignment = org1.StaffAssignments.AddNew();
			assignment.O8_OH = org1.PK;
			assignment.O8_Department = "ALL";
			assignment.O8_GS_NKPersonResponsible = staff1.GS_Code;
			assignment.O8_Role = "RM2";

			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ZE2";
			OrgStaffAssignments assignment2 = org2.StaffAssignments.AddNew();
			assignment2.O8_OH = org2.PK;
			assignment2.O8_Department = "ALL";
			assignment2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			assignment2.O8_Role = "RM2";

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Key Account Manager - Secondary"];
			filter.IsActive = true;
			filter.Property = staff1.PK;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("Should not contain org2.", false, orgCollection.Contains(org2));
			AssertEquals("Should contain org1.", true, orgCollection.Contains(org1));

			filter.Property = staff2.PK;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertEquals("Should not contain org1.", false, orgCollection.Contains(org1));
			AssertEquals("Should contain org2.", true, orgCollection.Contains(org2));
		}

		public void TestDatabaseHostedLocationFilter()
		{
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.CreateAndLoadLicenceForOrg();
			org2.CreateAndLoadLicenceForOrg();
			org3.CreateAndLoadLicenceForOrg();
			org4.CreateAndLoadLicenceForOrg();
			org1.LicenceEnterpriseCode = "AAA";
			org2.LicenceEnterpriseCode = "BBB";
			org3.LicenceEnterpriseCode = "CCC";
			org4.LicenceEnterpriseCode = "DDD";

			LicenceDatabase database1 = org1.LicCompany.LicDatabases.AddNew();
			database1.FillWithValidTestData();
			database1.LD_HostedLocation = "SYD";
			LicenceDatabase database2 = org2.LicCompany.LicDatabases.AddNew();
			database2.FillWithValidTestData();
			database2.LD_HostedLocation = "CHI";
			LicenceDatabase database3 = org3.LicCompany.LicDatabases.AddNew();
			database3.FillWithValidTestData();
			database3.LD_HostedLocation = "CHI";
			LicenceDatabase database4 = org4.LicCompany.LicDatabases.AddNew();
			database4.FillWithValidTestData();
			database4.LD_HostedLocation = "NCW";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Database Hosted Location"];
			filter.Property = "SYD";
			filter.IsActive = true;

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(org1, collection);

			filter.Property = "CHI";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(org2, collection);
			AssertCollectionContains(org3, collection);

			filter.Property = "NCW";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(org4, collection);

			filter.Property = "ALL";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(3, collection.Count);
			AssertCollectionContains(org1, collection);
			AssertCollectionContains(org2, collection);
			AssertCollectionContains(org3, collection);
		}

		[TestDate(2014, 12, 3, 21, 0, 0)] // 2014-12-04 08:00 UTC+11
		[TestUtcOffset(11, 0, 0)]
		public void TestWARPDateFilter()
		{
			var yesterdayUtc = new ZDateTime(2014, 12, 3, 11, 0, 0); // 2014-12-03 22:00 UTC+11
			ModuleTextFilter textFilter = (ModuleTextFilter)FilterStripBizO["Code"];
			textFilter.Property = "AAAA";
			textFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			textFilter.IsActive = true;
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org5 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org6 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = "AAAAA";
			org2.OH_Code = "AAAAB";
			org3.OH_Code = "AAAAC";
			org4.OH_Code = "AAAAD";
			org5.OH_Code = "AAAAE";
			org6.OH_Code = "AAAAF";
			Factory.Save();

			SetupRelatedParty(org1.PK, org2.PK);
			SetupRelatedParty(org1.PK, org3.PK, yesterdayUtc);
			SetupRelatedParty(org2.PK, org4.PK);
			SetupRelatedParty(org2.PK, org5.PK, yesterdayUtc);
			var validParty = SetupRelatedParty(org5.PK, org6.PK, yesterdayUtc);
			var invalidSavedParty = SetupRelatedParty(org6.PK, org6.PK);
			using (invalidSavedParty.GetValidationSuspender())
			{
				Factory.Save();
				AssertEquals("Precondition: OrgRelatedParty.OnSaving did not override", validParty.PR_SystemCreateTimeUtc, yesterdayUtc);

				ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["WARP Date"];
				filter.IsActive = true;
				filter.PropertySearch = "Today";

				var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();

				AssertEquals("Collection contains 3 orgs", 3, orgCollection.Count);

				AssertEquals("Collection contains org 1", true, orgCollection.Contains(org1));
				AssertEquals("Collection contains org 2", true, orgCollection.Contains(org2));
				AssertEquals("Collection does not contain org 3", false, orgCollection.Contains(org3));
				AssertEquals("Collection contains org 4", true, orgCollection.Contains(org4));
				AssertEquals("Collection does not contain org 5", false, orgCollection.Contains(org5));
				AssertEquals("Collection does not contain org 6", false, orgCollection.Contains(org6));

				filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				filter.Property1 = yesterdayUtc;
				filter.Property2 = yesterdayUtc;

				orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();

				AssertEquals("Collection contains 5 orgs", 5, orgCollection.Count);

				AssertEquals("Collection contains org 1", true, orgCollection.Contains(org1));
				AssertEquals("Collection contains org 2", true, orgCollection.Contains(org2));
				AssertEquals("Collection contains org 3", true, orgCollection.Contains(org3));
				AssertEquals("Collection does not contain org 4", false, orgCollection.Contains(org4));
				AssertEquals("Collection contains org 5", true, orgCollection.Contains(org5));
				AssertEquals("Collection contains org 6", true, orgCollection.Contains(org6));
			}
		}

		public void TestWARPNominatingAgentFilter()
		{
			ModuleTextFilter textFilter = (ModuleTextFilter)FilterStripBizO["Code"];
			textFilter.Property = "AAAA";
			textFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			textFilter.IsActive = true;
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org5 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org6 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = "AAAAA";
			org2.OH_Code = "AAAAB";
			org3.OH_Code = "AAAAC";
			org4.OH_Code = "AAAAD";
			org5.OH_Code = "AAAAE";
			org6.OH_Code = "AAAAF";
			Factory.Save();

			SetupRelatedParty(org1.PK, org2.PK);
			SetupRelatedParty(org1.PK, org3.PK);
			SetupRelatedParty(org2.PK, org4.PK);
			SetupRelatedParty(org2.PK, org5.PK);
			var invalidSavedParty = SetupRelatedParty(org6.PK, org6.PK);
			using (invalidSavedParty.GetValidationSuspender())
			{
				Factory.Save();

				ModuleFlagsFilter filter = (ModuleFlagsFilter)FilterStripBizO["WARP Relationships"];
				filter["Is WARP Nominated Agent"] = true;
				filter.IsActive = true;

				OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();

				AssertEquals("Collection contains 2 orgs", 2, orgCollection.Count);
				AssertEquals("Collection contains org 1", true, orgCollection.Contains(org1));
				AssertEquals("Collection contains org 2", true, orgCollection.Contains(org2));

				filter = (ModuleFlagsFilter)FilterStripBizO["WARP Relationships"];
				filter["Is WARP Nominated Agent"] = false;
				filter.IsActive = true;

				orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();

				AssertEquals("Collection contains all 6 orgs", 6, orgCollection.Count);
			}
		}

		public void TestWARPReferringCustomerFilter()
		{
			ModuleTextFilter textFilter = (ModuleTextFilter)FilterStripBizO["Code"];
			textFilter.Property = "AAAA";
			textFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			textFilter.IsActive = true;
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org5 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org6 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = "AAAAA";
			org2.OH_Code = "AAAAB";
			org3.OH_Code = "AAAAC";
			org4.OH_Code = "AAAAD";
			org5.OH_Code = "AAAAE";
			org6.OH_Code = "AAAAF";
			Factory.Save();

			SetupRelatedParty(org1.PK, org2.PK);
			SetupRelatedParty(org1.PK, org3.PK);
			SetupRelatedParty(org2.PK, org4.PK);
			SetupRelatedParty(org2.PK, org5.PK);
			var invalidSavedParty = SetupRelatedParty(org6.PK, org6.PK);
			using (invalidSavedParty.GetValidationSuspender())
			{
				Factory.Save();

				ModuleFlagsFilter filter = (ModuleFlagsFilter)FilterStripBizO["WARP Relationships"];
				filter["Is WARP Referring Customer"] = true;
				filter.IsActive = true;

				OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();

				AssertEquals("Collection contains 4 orgs", 4, orgCollection.Count);
				AssertEquals("Collection contains org 2", true, orgCollection.Contains(org2));
				AssertEquals("Collection contains org 3", true, orgCollection.Contains(org3));
				AssertEquals("Collection contains org 4", true, orgCollection.Contains(org4));
				AssertEquals("Collection contains org 5", true, orgCollection.Contains(org5));

				filter = (ModuleFlagsFilter)FilterStripBizO["WARP Relationships"];
				filter["Is WARP Referring Customer"] = false;
				filter.IsActive = true;

				orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();

				AssertEquals("Collection contains all 6 orgs", 6, orgCollection.Count);
			}
		}

		EDIOrgRelatedParty SetupRelatedParty(ZGuid parentGuid, ZGuid relatedPartyGuid)
		{
			return SetupRelatedParty(parentGuid, relatedPartyGuid, ZDateTime.UtcNow);
		}

		EDIOrgRelatedParty SetupRelatedParty(ZGuid parentGuid, ZGuid relatedPartyGuid, ZDateTime createdTime)
		{
			EDIOrgRelatedParty party = Factory.NewWithValidTestData<EDIOrgRelatedParty>();
			party.PR_OH_Parent = parentGuid;
			party.PR_OH_RelatedParty = relatedPartyGuid;
			party.PR_PartyType = EDIOrgRelatedPartyLookups.WARPConstant;
			party.PR_SystemCreateTimeUtc = createdTime;
			return party;
		}

		#region Invoicing Filters

		public void TestInvoiceFromBranchFilter()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDU");
			BillingTestHelper.SetInvoicing(org1, branch1.PK);
			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDW");
			BillingTestHelper.SetInvoicing(org2, branch2.PK);
			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Invoice From Branch"];
			filter.IsActive = true;
			filter.Property = branch1.PK;

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));

			filter.Property = branch2.PK;
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));
		}

		public void TestDefaultTaxRateFilter()
		{
			AccTaxRate taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			AccTaxRate taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDE");
			org1.LicCompany.InvoiceDeliveries.AddNew().L9_AT_TaxId = taxRate1.PK;
			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDV");
			org2.LicCompany.InvoiceDeliveries.AddNew().L9_AT_TaxId = taxRate2.PK;
			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Default Tax Rate"];
			filter.IsActive = true;
			filter.Property = taxRate1.PK;

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));

			filter.Property = taxRate2.PK;
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));
		}

		public void TestInvoicingCurrencyFilter()
		{
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDG");
			BillingTestHelper.SetInvoiceCurrency(org1, "AUD");
			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDB");
			BillingTestHelper.SetInvoiceCurrency(org2, "USD");
			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO["Invoicing Currency"];
			filter.IsActive = true;
			filter.Property = "NZD";

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));

			filter.Property = "AUD";
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));

			filter.Property = "USD";
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));
		}

		public void TestProcessingFeeFilter()
		{
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDG");
			org1.LicCompany.SelfBilling.L4_ProcessingFee = "DDE";
			org1.LicCompany.SelfBilling.L4_ProcessingFeePercent = -3.0m;
			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDB");
			org2.LicCompany.SelfBilling.L4_ProcessingFee = "MPF";
			org2.LicCompany.SelfBilling.L4_ProcessingFeePercent = 5.0m;
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Processing Fee"];
			filter.IsActive = true;
			filter.Property = "MPF";

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));

			filter.Property = "DDE";
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));
		}

		[StressTest]
		public void TestInvoiceToOrganisationFilter()
		{
			EDIOrgHeader billingOrg1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader billingOrg2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDZ");
			org1.LicCompany.InvoiceDeliveries.AddNew().L9_OH_InvoiceTo = billingOrg1.PK;
			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDM");
			org2.LicCompany.InvoiceDeliveries.AddNew().L9_OH_InvoiceTo = billingOrg2.PK;
			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Invoice To Organisation"];
			filter.IsActive = true;

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			filter.Property = billingOrg1.PK;
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));

			filter.Property = billingOrg2.PK;
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));
		}

		#endregion

		#region Pricing Filters

		public void TestPriceListCurrencyFilter()
		{
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDI");
			ClientLicencePriceHeader priceHeader11 = org1.LicCompany.PriceHeaders.AddNew();
			priceHeader11.L6_RX_NKCurrency = "AUD";
			priceHeader11.L6_ValidFrom = new ZDateTime(2010, 11, 1);
			ClientLicencePriceHeader priceHeader12 = org1.LicCompany.PriceHeaders.AddNew();
			priceHeader12.L6_RX_NKCurrency = "USD";
			priceHeader12.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDK");
			ClientLicencePriceHeader priceHeader2 = org2.LicCompany.PriceHeaders.AddNew();
			priceHeader2.L6_RX_NKCurrency = "USD";
			priceHeader2.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO["Price Currency"];
			filter.IsActive = true;
			filter.Property = "AUD";

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));

			filter.Property = "USD";
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));
		}

		public void TestModulePriceFilter()
		{
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDA");
			ClientLicencePriceHeader priceHeader1 = org1.LicCompany.PriceHeaders.AddNew();
			priceHeader1.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			ClientLicencePriceItem item11 = priceHeader1.Items.AddNew();
			item11.L7_Code = "COR";
			item11.L7_Price = 33m;
			ClientLicencePriceItem item12 = priceHeader1.Items.AddNew();
			item12.L7_Code = "ISF";
			item12.L7_Price = 2m;

			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDC");
			ClientLicencePriceHeader priceHeader2 = org2.LicCompany.PriceHeaders.AddNew();
			priceHeader2.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			ClientLicencePriceItem item21 = priceHeader2.Items.AddNew();
			item21.L7_Code = "COR";
			item21.L7_Price = 30m;
			ClientLicencePriceItem item122 = priceHeader2.Items.AddNew();
			item122.L7_Code = "ISF";
			item122.L7_Price = 2m;

			ModuleNumberRangeFilter priceFilter = (ModuleNumberRangeFilter)FilterStripBizO["Module Price"];
			priceFilter.IsActive = true;
			priceFilter.Property1 = 31m;
			priceFilter.Property2 = 33m;

			Factory.Save();

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));

			priceFilter.Property1 = 30m;
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));

			priceFilter.Property1 = 2m;
			priceFilter.Property2 = 2m;
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));

			ModuleTextFilter moduleFilter = (ModuleTextFilter)FilterStripBizO["Price List Module Code"];
			moduleFilter.IsActive = true;
			moduleFilter.Property = "COR";
			priceFilter.Property1 = 33m;
			priceFilter.Property2 = 33m;
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));

			moduleFilter.Property = "COR";
			priceFilter.Property1 = 2m;
			priceFilter.Property2 = 2m;
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));

			moduleFilter.Property = "ISF";
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));
		}

		public void TestModuleFeeTypeFilter()
		{
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDP");
			ClientLicencePriceHeader priceHeader1 = org1.LicCompany.PriceHeaders.AddNew();
			priceHeader1.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			ClientLicencePriceItem item11 = priceHeader1.Items.AddNew();
			item11.L7_Code = "COR";
			item11.L7_FeeType = BillingConstants.FeeType.NamedUser;
			ClientLicencePriceItem item12 = priceHeader1.Items.AddNew();
			item12.L7_Code = "ISF";
			item12.L7_FeeType = BillingConstants.FeeType.NamedUser;

			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDO");
			ClientLicencePriceHeader priceHeader2 = org2.LicCompany.PriceHeaders.AddNew();
			priceHeader2.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			ClientLicencePriceItem item21 = priceHeader2.Items.AddNew();
			item21.L7_Code = "COR";
			item21.L7_FeeType = BillingConstants.FeeType.Module;
			ClientLicencePriceItem item122 = priceHeader2.Items.AddNew();
			item122.L7_Code = "ISF";
			item122.L7_FeeType = BillingConstants.FeeType.Transactional;

			EDIOrgHeader org3 = BillingTestHelper.CreateOrganisation(Factory, "DDY");
			ClientLicencePriceHeader priceHeader3 = org3.LicCompany.PriceHeaders.AddNew();
			priceHeader3.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			ClientLicencePriceItem item31 = priceHeader3.Items.AddNew();
			item31.L7_Code = "FOR";
			item31.L7_FeeType = BillingConstants.FeeType.Module;
			ClientLicencePriceItem item132 = priceHeader3.Items.AddNew();
			item132.L7_Code = "DPS";
			item132.L7_FeeType = BillingConstants.FeeType.Transactional;

			Factory.Save();

			ModuleTextFilter feeTypeFilter = (ModuleTextFilter)FilterStripBizO["Module Fee Type"];
			feeTypeFilter.IsActive = true;
			feeTypeFilter.Property = BillingConstants.FeeType.Module;

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));
			Assert("Should contain org3", collection.Contains(org3));

			feeTypeFilter.Property = BillingConstants.FeeType.Transactional;
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));
			Assert("Should contain org3", collection.Contains(org3));

			ModuleTextFilter moduleFilter = (ModuleTextFilter)FilterStripBizO["Price List Module Code"];
			moduleFilter.IsActive = true;
			moduleFilter.Property = "COR";
			feeTypeFilter.Property = BillingConstants.FeeType.Module;
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));
			Assert("Should not contain org3", !collection.Contains(org3));

			moduleFilter.Property = "ISF";
			feeTypeFilter.Property = BillingConstants.FeeType.Transactional;
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));
			Assert("Should not contain org3", !collection.Contains(org3));
		}

		#endregion

		#region Invoice Fee Filters

		public void TestInvoiceFeeCurrencyFilter()
		{
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDG");
			var fee1 = BillingTestHelper.CreateMaintenanceFee(org1.LicCompany, "Fee 1", 100m, "UPGASS");
			fee1.L8_RX_NKCurrency = "AUD";

			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDB");
			var fee2 = BillingTestHelper.CreateMaintenanceFee(org2.LicCompany, "Fee 2", 100m, "UPGASS");
			fee2.L8_RX_NKCurrency = "USD";

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO["Invoice Fee Currency"];
			filter.IsActive = true;
			filter.Property = "USD";

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));

			filter.Property = "AUD";
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));
		}

		public void TestInvoiceFeeStartDateFilter()
		{
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDG");
			var fee1 = BillingTestHelper.CreateMaintenanceFee(org1.LicCompany, "Fee 1", 100m, "UPGASS");
			fee1.L8_StartDate = ZDateTime.Empty;

			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDB");
			var fee2 = BillingTestHelper.CreateMaintenanceFee(org2.LicCompany, "Fee 2", 100m, "UPGASS");
			fee2.L8_StartDate = ZDateTime.Now.AddDays(-2);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["Invoice Fee Start Date"];
			filter.IsActive = true;
			filter.PropertySearch = "Last 7 Days";

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));

			filter.PropertySearch = "Has No Date";
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));
		}

		public void TestInvoiceFeeEndDateFilter()
		{
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDG");
			var fee1 = BillingTestHelper.CreateMaintenanceFee(org1.LicCompany, "Fee 1", 100m, "UPGASS");
			fee1.L8_EndDate = ZDateTime.Empty;

			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDB");
			var fee2 = BillingTestHelper.CreateMaintenanceFee(org2.LicCompany, "Fee 2", 100m, "UPGASS");
			fee2.L8_EndDate = ZDateTime.Now.AddDays(-2);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["Invoice Fee End Date"];
			filter.IsActive = true;
			filter.PropertySearch = "Last 7 Days";

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));

			filter.PropertySearch = "Has No Date";
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));
		}

		public void TestInvoiceFeeDescriptionFilter()
		{
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDG");
			var fee1 = BillingTestHelper.CreateMaintenanceFee(org1.LicCompany, "Fee 1", 100m, "UPGASS");

			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDB");
			var fee2 = BillingTestHelper.CreateMaintenanceFee(org2.LicCompany, "Fee 2", 100m, "UPGASS");

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Invoice Fee Description"];
			filter.IsActive = true;
			filter.Property = "Fee 2";

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));

			filter.Property = "Fee 1";
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));
		}

		public void TestInvoiceFeeCommentFilter()
		{
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDG");
			var fee1 = BillingTestHelper.CreateMaintenanceFee(org1.LicCompany, "Fee 1", 100m, "UPGASS");
			fee1.L8_Comment = "Comment1";

			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDB");
			var fee2 = BillingTestHelper.CreateMaintenanceFee(org2.LicCompany, "Fee 2", 100m, "UPGASS");
			fee2.L8_Comment = "Comment2";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Invoice Fee Comment"];
			filter.IsActive = true;
			filter.Property = "Comment2";

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));

			filter.Property = "Comment1";
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));
		}

		public void TestInvoiceFeeChargeCodeFilter()
		{
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDG");
			var fee1 = BillingTestHelper.CreateMaintenanceFee(org1.LicCompany, "Fee 1", 100m, "UPGASS");

			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDB");
			var fee2 = BillingTestHelper.CreateMaintenanceFee(org2.LicCompany, "Fee 2", 100m, "EHUB");

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Invoice Fee Charge Code"];
			filter.IsActive = true;
			filter.Property = "EHUB";

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));

			filter.Property = "UPGASS";
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));
		}

		public void TestInvoiceFeeTypeFilter()
		{
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDG");
			var fee1 = BillingTestHelper.CreateMaintenanceFee(org1.LicCompany, "Fee 1", 100m, "UPGASS");
			fee1.L8_Type = "ESV";

			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDB");
			var fee2 = BillingTestHelper.CreateMaintenanceFee(org2.LicCompany, "Fee 2", 100m, "UPGASS");
			fee2.L8_Type = "UPG";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Invoice Fee Type"];
			filter.IsActive = true;
			filter.Property = "UPG";

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));

			filter.Property = "ESV";
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));
		}

		#endregion

		#region Invoice Discount Filters

		public void TestInvoiceDiscountTypeFilter()
		{
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDG");
			var discount1 = CreateDiscount(org1.LicCompany.SelfBilling, BillingConstants.BillingSystem.ODM, BillingConstants.DiscountType.Commitment, ZDateTime.Empty, ZDateTime.Empty);

			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDB");
			var discount2 = CreateDiscount(org2.LicCompany.SelfBilling, BillingConstants.BillingSystem.ODM, BillingConstants.DiscountType.Prepayment, ZDateTime.Empty, ZDateTime.Empty);

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Invoice Discount Type"];
			filter.IsActive = true;
			filter.Property = BillingConstants.DiscountType.Prepayment;

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));

			filter.Property = BillingConstants.DiscountType.Commitment;
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));
		}

		public void TestInvoiceDiscountSystemFilter()
		{
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDG");
			var discount1 = CreateDiscount(org1.LicCompany.SelfBilling, BillingConstants.BillingSystem.ODM, BillingConstants.DiscountType.Commitment, ZDateTime.Empty, ZDateTime.Empty);

			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDB");
			var discount2 = CreateDiscount(org2.LicCompany.SelfBilling, BillingConstants.BillingSystem.DeniedPartyScreening, BillingConstants.DiscountType.Prepayment, ZDateTime.Empty, ZDateTime.Empty);

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Invoice Discount System"];
			filter.IsActive = true;
			filter.Property = BillingConstants.BillingSystem.DeniedPartyScreening;

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));

			filter.Property = BillingConstants.BillingSystem.ODM;
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));
		}

		public void TestInvoiceDiscountModuleFilter()
		{
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDG");
			var discount1 = CreateDiscount(org1.LicCompany.SelfBilling, BillingConstants.BillingSystem.ODM, BillingConstants.DiscountType.ModuleSpecific, ZDateTime.Empty, ZDateTime.Empty);
			discount1.L5_ModuleCode = "COR";

			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDB");
			var discount2 = CreateDiscount(org2.LicCompany.SelfBilling, BillingConstants.BillingSystem.ODM, BillingConstants.DiscountType.ModuleSpecific, ZDateTime.Empty, ZDateTime.Empty);
			discount2.L5_ModuleCode = "ACC";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Invoice Discount Module Code"];
			filter.IsActive = true;
			filter.Property = "ACC";

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));

			filter.Property = "COR";
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));
		}

		public void TestInvoiceDiscountStartDateFilter()
		{
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDG");
			var discount1 = CreateDiscount(org1.LicCompany.SelfBilling, BillingConstants.BillingSystem.ODM, BillingConstants.DiscountType.Commitment, ZDateTime.Empty, ZDateTime.Empty);

			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDB");
			var discount2 = CreateDiscount(org2.LicCompany.SelfBilling, BillingConstants.BillingSystem.ODM, BillingConstants.DiscountType.Prepayment, ZDateTime.Now.AddDays(-2), ZDateTime.Empty);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["Invoice Discount Start Date"];
			filter.IsActive = true;
			filter.PropertySearch = "Last 7 Days";

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));

			filter.PropertySearch = "Has No Date";
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));
		}

		public void TestInvoiceDiscountEndDateFilter()
		{
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDG");
			var discount1 = CreateDiscount(org1.LicCompany.SelfBilling, BillingConstants.BillingSystem.ODM, BillingConstants.DiscountType.Commitment, ZDateTime.Empty, ZDateTime.Empty);

			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDB");
			var discount2 = CreateDiscount(org2.LicCompany.SelfBilling, BillingConstants.BillingSystem.ODM, BillingConstants.DiscountType.Prepayment, ZDateTime.Empty, ZDateTime.Now.AddDays(-2));

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["Invoice Discount End Date"];
			filter.IsActive = true;
			filter.PropertySearch = "Last 7 Days";

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));

			filter.PropertySearch = "Has No Date";
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));
		}

		public void TestInvoiceDiscountDescriptionFilter()
		{
			EDIOrgHeader org1 = BillingTestHelper.CreateOrganisation(Factory, "DDG");
			var discount1 = CreateDiscount(org1.LicCompany.SelfBilling, BillingConstants.BillingSystem.ODM, BillingConstants.DiscountType.Commitment, ZDateTime.Empty, ZDateTime.Empty);
			discount1.L5_Description = "Discount 1";

			EDIOrgHeader org2 = BillingTestHelper.CreateOrganisation(Factory, "DDB");
			var discount2 = CreateDiscount(org2.LicCompany.SelfBilling, BillingConstants.BillingSystem.ODM, BillingConstants.DiscountType.Commitment, ZDateTime.Empty, ZDateTime.Empty);
			discount2.L5_Description = "Discount 2";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Invoice Discount Description"];
			filter.IsActive = true;
			filter.Property = "Discount 2";

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			Assert("Should not contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));

			filter.Property = "Discount 1";
			collection.Load(FilterStripBizO.Filter);
			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should not contain org2", !collection.Contains(org2));
		}

		ClientLicenceBillingDiscount CreateDiscount(ClientLicenceBilling billing, ZString system, ZString type, ZDateTime startDate, ZDateTime endDate)
		{
			var discount = billing.BillingDiscounts.AddNew();
			discount.L5_SystemCode = system;
			discount.L5_Type = type;
			discount.L5_StartDate = startDate;
			discount.L5_EndDate = endDate;
			discount.L5_BreakAmount = 10m;
			discount.L5_Discount = 10m;
			return discount;
		}

		public void TestCoreFilters()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "ZUBIN";
			org.OH_FullName = "Zubs Organisation";
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ZUBI2";
			org2.OH_FullName = "Zub2 Organisation";
			OrgHeader org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "ZUBI3";
			org3.OH_FullName = "Zub3 Organisation";
			Factory.Save();

			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "ENT";
			enterprise.LE_OH = org.PK;
			LicenceEnterprise enterprise2 = Factory.New<LicenceEnterprise>();
			enterprise2.LE_EnterpriseCode = "EN2";
			enterprise2.LE_OH = org2.PK;
			LicenceEnterprise enterprise3 = Factory.New<LicenceEnterprise>();
			enterprise3.LE_EnterpriseCode = "EN3";
			enterprise3.LE_OH = org3.PK;
			Factory.Save();

			// No. 1
			LicenceCompany company = Factory.New<LicenceCompany>();
			company.LC_CompanyCode = "COM";
			company.LC_LE = enterprise.PK;
			company.LC_OH = org.PK;
			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;

			// No. 2
			LicenceCompany company2 = Factory.New<LicenceCompany>();
			company2.LC_CompanyCode = "CO2";
			company2.LC_LE = enterprise2.PK;
			company2.LC_OH = org2.PK;
			LicenceDatabase database2 = Factory.New<LicenceDatabase>();
			database2.LD_ServerCode = "SR2";
			database2.LD_LE = enterprise2.PK;

			// No. 2
			LicenceCompany company3 = Factory.New<LicenceCompany>();
			company3.LC_CompanyCode = "CO3";
			company3.LC_LE = enterprise3.PK;
			company3.LC_OH = org3.PK;
			LicenceDatabase database3 = Factory.New<LicenceDatabase>();
			database3.LD_ServerCode = "SR3";
			database3.LD_LE = enterprise3.PK;

			LicenceHeader header1 = Factory.New<LicenceHeader>();
			header1.LA_LC = company.PK;
			header1.LA_LD = database.PK;
			LicenceModules licModules1 = Factory.New<LicenceModules>();
			licModules1.LM_LA = header1.PK;
			licModules1.LM_GroupModuleCode = "COR";
			licModules1.LM_LicenceType = "XXX";
			licModules1.LM_UserCount = 15;

			LicenceHeader header2 = Factory.New<LicenceHeader>();
			header2.LA_LC = company2.PK;
			header2.LA_LD = database2.PK;
			LicenceModules licModules2 = Factory.New<LicenceModules>();
			licModules2.LM_LA = header2.PK;
			licModules2.LM_GroupModuleCode = "COR";
			licModules2.LM_LicenceType = "ABC";
			licModules2.LM_UserCount = 10;

			LicenceHeader header3 = Factory.New<LicenceHeader>();
			header3.LA_LC = company3.PK;
			header3.LA_LD = database3.PK;
			LicenceModules licModules3 = Factory.New<LicenceModules>();
			licModules3.LM_LA = header3.PK;
			licModules3.LM_GroupModuleCode = "FRE"; // is ignored in Core-Fee-Basis and Core-Set-Count filter
			licModules3.LM_LicenceType = "XXX";
			licModules3.LM_UserCount = 10;

			Factory.Save();

			var filterBizo = new EDIOrganisationFilterBusinessObjectCore();

			((ModuleTextFilter)filterBizo["Core Fee Basis"]).Property = "Zub";
			((ModuleTextFilter)filterBizo["Core Fee Basis"]).IsActive = true;
			OrgHeaderCollection headers = new OrgHeaderCollection(Factory);
			headers.Load(filterBizo.Filter);
			AssertEquals("NO items found", 0, headers.Count);

			((ModuleTextFilter)filterBizo["Core Fee Basis"]).Property = "XXX";
			((ModuleTextFilter)filterBizo["Core Fee Basis"]).IsActive = true;
			headers.Load(filterBizo.Filter);
			AssertEquals("1 items found", 1, headers.Count);

			((ModuleTextFilter)filterBizo["Core Fee Basis"]).IsActive = false;
			ModuleNumberRangeFilter coreSeatCountFilter = (ModuleNumberRangeFilter)filterBizo["Core Seat Count"];
			coreSeatCountFilter.IsActive = true;
			coreSeatCountFilter.Property1 = 9;
			coreSeatCountFilter.Property2 = 11;
			headers.Load(filterBizo.Filter);
			AssertEquals("1 items found", 1, headers.Count);
		}

		#endregion

		#region Number Filters

		public void TestAmountOfBusinessWonFilter()
		{
			using (EDIDataRegistry.Instance.AmountOfBusinessWonLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Amount Of Business Won Caption"))
			{
				var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDI");
				org1.MiscServ.OM_CMAmountOfBusinessWon = 3;

				var org2 = BillingTestHelper.CreateOrganisation(Factory, "DDK");
				org2.MiscServ.OM_CMAmountOfBusinessWon = 9;

				Factory.Save();

				var amountOfBusinessWonFilter = (ModuleNumberRangeFilter)FilterStripBizO["Amount Of Business Won"];
				amountOfBusinessWonFilter.IsActive = true;
				amountOfBusinessWonFilter.Property1 = 1;
				amountOfBusinessWonFilter.Property2 = 5;

				var collection = new OrgHeaderCollection(Factory);
				collection.Load(FilterStripBizO.Filter);
				AssertEquals("Should contain org1", true, collection.Contains(org1));
				AssertEquals("Should not contain org2", false, collection.Contains(org2));
				AssertEquals("Test Amount Of Business Won Caption", amountOfBusinessWonFilter.MultilingualDescription.ToString());
			}
		}

		public void TestTotalClientRevenueFilter()
		{
			using (EDIDataRegistry.Instance.TotalClientRevenueLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Total Client Revenue Caption"))
			{
				var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDI");
				org1.MiscServ.OM_CMTotalClientRevenue = 4;

				var org2 = BillingTestHelper.CreateOrganisation(Factory, "DDK");
				org2.MiscServ.OM_CMTotalClientRevenue = 10;

				Factory.Save();

				var totalClientRevenueFilter = (ModuleNumberRangeFilter)FilterStripBizO["Total Client Revenue"];
				totalClientRevenueFilter.IsActive = true;
				totalClientRevenueFilter.Property1 = 1;
				totalClientRevenueFilter.Property2 = 5;

				var collection = new OrgHeaderCollection(Factory);
				collection.Load(FilterStripBizO.Filter);
				AssertEquals("Should contain org1", true, collection.Contains(org1));
				AssertEquals("Should not contain org2", false, collection.Contains(org2));
				AssertEquals("Test Total Client Revenue Caption", totalClientRevenueFilter.MultilingualDescription.ToString());
			}
		}

		public void TestWarehouseRevenueFilter()
		{
			using (EDIDataRegistry.Instance.WarehouseRevenueLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Warehouse Revenue Caption"))
			{
				var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDI");
				org1.MiscServ.OM_CMWarehouseRevenue = 5;

				var org2 = BillingTestHelper.CreateOrganisation(Factory, "DDK");
				org2.MiscServ.OM_CMWarehouseRevenue = 11;

				Factory.Save();

				var warehouseRevenueFilter = (ModuleNumberRangeFilter)FilterStripBizO["Warehouse Revenue"];
				warehouseRevenueFilter.IsActive = true;
				warehouseRevenueFilter.Property1 = 7;
				warehouseRevenueFilter.Property2 = 15;

				var collection = new OrgHeaderCollection(Factory);
				collection.Load(FilterStripBizO.Filter);
				AssertEquals("Should not contain org1", false, collection.Contains(org1));
				AssertEquals("Should contain org2", true, collection.Contains(org2));
				AssertEquals("Test Warehouse Revenue Caption", warehouseRevenueFilter.MultilingualDescription.ToString());
			}
		}

		public void TestConsultingRevenueFilter()
		{
			using (EDIDataRegistry.Instance.ConsultingRevenueLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Consulting Revenue Caption"))
			{
				var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDI");
				org1.MiscServ.OM_CMConsultingRevenue = 6;

				var org2 = BillingTestHelper.CreateOrganisation(Factory, "DDK");
				org2.MiscServ.OM_CMConsultingRevenue = 12;

				Factory.Save();

				var consultingRevenueFilter = (ModuleNumberRangeFilter)FilterStripBizO["Consulting Revenue"];
				consultingRevenueFilter.IsActive = true;
				consultingRevenueFilter.Property1 = 9;
				consultingRevenueFilter.Property2 = 15;

				var collection = new OrgHeaderCollection(Factory);
				collection.Load(FilterStripBizO.Filter);
				AssertEquals("Should not contain org1", false, collection.Contains(org1));
				AssertEquals("Should contain org2", true, collection.Contains(org2));
				AssertEquals("Test Consulting Revenue Caption", consultingRevenueFilter.MultilingualDescription.ToString());
			}
		}

		public void TestPaidUpCapitalFilter()
		{
			using (EDIDataRegistry.Instance.PaidUpCapitalLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Paid Up Capital Caption"))
			{
				var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDI");
				org1.MiscServ.OM_CMPaidUpCapital = 7;

				var org2 = BillingTestHelper.CreateOrganisation(Factory, "DDK");
				org2.MiscServ.OM_CMPaidUpCapital = 13;

				Factory.Save();

				var paidUpCapitalFilter = (ModuleNumberRangeFilter)FilterStripBizO["Paid Up Capital"];
				paidUpCapitalFilter.IsActive = true;
				paidUpCapitalFilter.Property1 = 9;
				paidUpCapitalFilter.Property2 = 15;

				var collection = new OrgHeaderCollection(Factory);
				collection.Load(FilterStripBizO.Filter);
				AssertEquals("Should not contain org1", false, collection.Contains(org1));
				AssertEquals("Should contain org2", true, collection.Contains(org2));
				AssertEquals("Test Paid Up Capital Caption", paidUpCapitalFilter.MultilingualDescription.ToString());
			}
		}

		#endregion

		public void TestCustomisedLabels()
		{
			using (EDIDataRegistry.Instance.NumberOfEmployeesLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Number Of Employees Caption"))
			using (EDIDataRegistry.Instance.AchievableBusinessLabel.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Achievable Business Caption"))
			{
				var achievableBusinessFilter = (ModuleNumberRangeFilter)FilterStripBizO["Achievable Business"];
				AssertEquals("Test Achievable Business Caption", achievableBusinessFilter.MultilingualDescription.ToString());

				var numberOfEmployeesFilter = (ModuleNumberRangeFilter)FilterStripBizO["Related Staff"];
				AssertEquals("Test Number Of Employees Caption", numberOfEmployeesFilter.MultilingualDescription.ToString());
			}
		}

		public void TestStlPriceListFilters()
		{
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = "DDD123SYD1";
			org1.CreateAndLoadLicenceForOrg();
			org1.LicenceEnterpriseCode = "AAA";

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "EN2", "AU2", "DB2");
			var priceList1 = BillingTestHelper.CreatePriceHeader(lic1.Company, "STL", "STL v1", "AUD", new ZDateTime(2016, 1, 1), false);
			var price1 = BillingTestHelper.AddPriceItem(priceList1, "C01", "TRA", "", 100m);
			var link1 = BillingTestHelper.CreatePriceLink(lic1.Database, priceList1, new ZDateTime(2016, 1, 1));
			link1.PHL_ValidTo = new ZDateTime(2017, 1, 1);
			link1.PHL_L6 = priceList1.PK;
			link1.PHL_RX_NKCurrency = "AUD";
			link1.PHL_VolumeCode = "HV";
			link1.PHL_CorePackCode = "INC";
			link1.PHL_SystemCreateUser = "U01";
			link1.PHL_SystemCreateTimeUtc = new ZDateTime(2016, 2, 1);

			var settingDIS = lic1.Database.LicenceSettings.AddNew();
			settingDIS.LS9_ValidFrom = settingDIS.LS9_ValidTo = new ZDateTime(2016, 1, 1);
			settingDIS.LS9_Type = "DIS";
			settingDIS.LS9_Name = "SPECIAL";
			settingDIS.LS9_Percent = 10;
			settingDIS.LS9_Comment = "Comment002";

			Factory.Save();

			var filter = new EDIOrganisationFilterBusinessObjectCore();
			((ModuleNkFilter)filter["STL Prices Currency"]).Property = "AUD";
			((ModuleNkFilter)filter["STL Prices Currency"]).IsActive = true;

			((ModuleTextFilter)filter["STL Setting Discount Name"]).Property = "SPECIAL";
			((ModuleTextFilter)filter["STL Setting Discount Name"]).IsActive = true;

			var orgs = new OrgHeaderCollection(Factory);
			orgs.Load(filter.Filter);

			AssertCollectionContains(lic1.Company.LicEnterprise.Organisation, orgs);
			AssertCollectionNotContains(org1, orgs);

			//If a customer had one database that matched "STl Prices Version", but not the discount,
			//and another database that matched the discount, but not the prices it would not be in the results.
			var lic3 = BillingTestHelper.CreateLicence(Factory, "EN3", "AU3", "DB3");
			var db3 = lic3.Database;
			var db4 = BillingTestHelper.CreateAnotherDatabase(lic3, "DB4").Database;

			var priceList3 = BillingTestHelper.CreatePriceHeader(lic3.Company, "STL", "STL v3", "AUD", new ZDateTime(2016, 1, 1), false);
			var price3 = BillingTestHelper.AddPriceItem(priceList3, "C03", "TRA", "", 300m);
			var link3 = BillingTestHelper.CreatePriceLink(db3, priceList1, new ZDateTime(2016, 1, 1));
			link3.PHL_ValidTo = new ZDateTime(2017, 1, 1);
			link3.PHL_L6 = priceList3.PK;
			link3.PHL_RX_NKCurrency = "AUD";
			link3.PHL_VolumeCode = "LV";
			link3.PHL_CorePackCode = "EX";
			link3.PHL_SystemCreateUser = "U03";
			link3.PHL_SystemCreateTimeUtc = new ZDateTime(2016, 2, 1);

			var settingDIS4 = db4.LicenceSettings.AddNew();
			settingDIS4.LS9_ValidFrom = settingDIS4.LS9_ValidTo = new ZDateTime(2016, 1, 1);
			settingDIS4.LS9_Type = "DIS";
			settingDIS4.LS9_Name = "SPECIAL";
			settingDIS4.LS9_Percent = 10;
			settingDIS4.LS9_Comment = "Comment004";

			Factory.Save();

			filter = new EDIOrganisationFilterBusinessObjectCore();
			((ModuleNkFilter)filter["STL Prices Currency"]).Property = "AUD";
			((ModuleNkFilter)filter["STL Prices Currency"]).IsActive = true;

			((ModuleTextFilter)filter["STL Setting Comment"]).Property = "Comment004";
			((ModuleTextFilter)filter["STL Setting Comment"]).IsActive = true;

			orgs = new OrgHeaderCollection(Factory);
			orgs.Load(filter.Filter);

			AssertCollectionNotContains(lic3.Company.LicEnterprise.Organisation, orgs);

			var settingDIS5 = db3.LicenceSettings.AddNew();
			settingDIS5.LS9_ValidFrom = settingDIS5.LS9_ValidTo = new ZDateTime(2016, 1, 1);
			settingDIS5.LS9_Type = "DIS";
			settingDIS5.LS9_Name = "SPECIAL";
			settingDIS5.LS9_Percent = 10;
			settingDIS5.LS9_Comment = "Comment004";

			Factory.Save();

			orgs.Load(filter.Filter);
			AssertCollectionContains(lic3.Company.LicEnterprise.Organisation, orgs);
		}

		#region IsMasterOrganisation

		public void TestIsMasterOrganisation()
		{
			var filterBizObj = new EDIOrganisationFilterBusinessObjectCore();
			var nameFilter = (ModuleTextFilter)filterBizObj["Name"];
			nameFilter.Property = "~!!TEST";
			nameFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			nameFilter.IsActive = true;
			var masterOrgFilter = (ModuleTextFilter)filterBizObj["Is Master Organization"];
			AssertNotNull(masterOrgFilter);
			masterOrgFilter.IsActive = true;

			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_RL_NKClosestPort = "AUSYD";
			org1.OH_FullName = "~!!TEST1Org";
			org1.OH_Code = "ENTTST";

			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "ENT";
			enterprise.LE_OH = org1.PK;

			var database = enterprise.Databases.AddNew();
			database.LD_ServerCode = "TST";
			database.LD_LicenceType = DatabaseTypes.Codes.Production;
			database.LD_Product = ProductTypes.Codes.Enterprise;

			var company = enterprise.Companies.AddNew();
			company.LC_CompanyCode = "TST";
			company.LC_OH = org1.PK;
			company.LC_LE = enterprise.PK;

			var licence = Factory.New<LicenceHeader>();
			licence.LA_LD = database.PK;
			licence.LA_LC = company.PK;

			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_FullName = "~!!TEST2Org";
			var org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org3.OH_FullName = "~!!TEST3Org";

			Factory.Save();

			var expectedOrg1 = Factory.Load<OrgHeader>(org1.PK);
			var expectedOrg2 = Factory.Load<OrgHeader>(org2.PK);
			var expectedOrg3 = Factory.Load<OrgHeader>(org3.PK);

			var subset = new OrgHeaderCollection(Factory);
			subset.Load(filterBizObj.Filter);
			AssertEquals(3, subset.Count);

			masterOrgFilter.Property = EDIOrganisationFilterBusinessObjectCore.IsMasterOrganisation.Code.MasterOrganisationsOnly;
			var orgCollection = new OrgHeaderCollection(Factory);
			orgCollection.Load(filterBizObj.Filter);
			AssertEquals(1, orgCollection.Count);
			AssertCollectionContains(expectedOrg1, orgCollection);

			masterOrgFilter.Property = EDIOrganisationFilterBusinessObjectCore.IsMasterOrganisation.Code.NonMasterOrganisationsOnly;
			orgCollection = new OrgHeaderCollection(Factory);
			orgCollection.Load(filterBizObj.Filter);
			AssertEquals(2, orgCollection.Count);
			AssertCollectionContains(expectedOrg2, orgCollection);
			AssertCollectionContains(expectedOrg3, orgCollection);

			masterOrgFilter.Property = EDIOrganisationFilterBusinessObjectCore.IsMasterOrganisation.Code.AllOrganisations;
			orgCollection = new OrgHeaderCollection(Factory);
			orgCollection.Load(filterBizObj.Filter);
			AssertEquals(3, orgCollection.Count);
			AssertCollectionContains(expectedOrg1, orgCollection);
			AssertCollectionContains(expectedOrg2, orgCollection);
			AssertCollectionContains(expectedOrg2, orgCollection);
		}

		#endregion

		#region Master Org for Database

		public void TestMasterOrgForDatabaseFilter()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "111", "SD1");
			var licence2 = BillingTestHelper.CreateLicence(Factory, "DDD", "222", "SD2");

			var org1 = licence1.Company.Header;
			var org2 = licence2.Company.Header;

			var db1 = licence1.Database;
			db1.LD_Product = ProductTypes.Codes.Enterprise;
			db1.LD_LicenceType = DatabaseTypes.Codes.Test;
			db1.LD_DatabaseNumber = 1001;
			db1.LD_OH_WebAccessOrg = org1.PK;

			var db2 = licence2.Database;
			db2.LD_Product = ProductTypes.Codes.Enterprise;
			db2.LD_LicenceType = DatabaseTypes.Codes.Production;
			db2.LD_DatabaseNumber = 2001;
			db2.LD_OH_WebAccessOrg = org2.PK;

			Factory.Save();

			var filterBizObj = FilterStripBizO;
			var orgsCollection = new EDIOrgHeaderCollection(Factory, filterBizObj.Filter);

			var relatedDbFilter = (ModuleGuidForeignCollectionFilter)filterBizObj["License Database (for Master Organization)"];
			relatedDbFilter.IsActive = true;
			relatedDbFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			var dbNumberFilter = relatedDbFilter.SelectedFilters.AddFilterStrip<ModuleNumberRangeFilter>("Database Number");
			dbNumberFilter.IsActive = true;
			dbNumberFilter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
			dbNumberFilter.Property1 = 2001;
			orgsCollection.Load(filterBizObj.Filter);
			AssertEquals(1, orgsCollection.Count);
			AssertCollectionContains(org2, orgsCollection);
		}

		#endregion

		#region Billing Org for Database

		public void TestBillingOrgForDatabaseFilter()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "111", "SD1");
			var licence2 = BillingTestHelper.CreateLicence(Factory, "DDD", "222", "SD2");
			var licence3 = BillingTestHelper.CreateLicence(Factory, "DDD", "333", "SD3");
			var licence4 = BillingTestHelper.CreateLicence(Factory, "DDD", "444", "SD4");

			var org1 = licence1.Company.Header;
			var org2 = licence2.Company.Header;
			var org3 = licence3.Company.Header;

			var db1 = licence1.Database;
			db1.LD_Product = ProductTypes.Codes.Enterprise;
			db1.LD_LicenceType = DatabaseTypes.Codes.Test;
			db1.LD_DatabaseNumber = 1001;
			var invoiceDelivery1 = licence1.Company.InvoiceDeliveries.AddNew();
			invoiceDelivery1.L9_IsBilled = true;

			var db2 = licence2.Database;
			db2.LD_Product = ProductTypes.Codes.Enterprise;
			db2.LD_LicenceType = DatabaseTypes.Codes.Production;
			db2.LD_DatabaseNumber = 2001;
			var invoiceDelivery2 = licence2.Company.InvoiceDeliveries.AddNew();
			invoiceDelivery2.L9_IsBilled = true;

			var db3 = licence3.Database;
			db3.LD_Product = ProductTypes.Codes.GLOW;
			db3.LD_LicenceType = DatabaseTypes.Codes.Production;
			db3.LD_DatabaseNumber = 3001;
			var invoiceDelivery3 = licence3.Company.InvoiceDeliveries.AddNew();
			invoiceDelivery3.L9_IsBilled = true;
			invoiceDelivery3.L9_OH_InvoiceTo = org2.PK;

			var db4 = licence4.Database;
			db4.LD_Product = ProductTypes.Codes.GLOW;
			db4.LD_LicenceType = DatabaseTypes.Codes.Production;
			db4.LD_DatabaseNumber = 4001;
			var invoiceDelivery4 = licence4.Company.InvoiceDeliveries.AddNew();
			invoiceDelivery4.L9_IsBilled = true;
			invoiceDelivery4.L9_OH_InvoiceTo = org3.PK;

			Factory.Save();

			var filterBizObj = FilterStripBizO;
			var orgsCollection = new EDIOrgHeaderCollection(Factory, filterBizObj.Filter);

			var relatedDbFilter = (BillingOrgForDatabaseModuleFilter)filterBizObj["Billing Org For Database"];
			relatedDbFilter.IsActive = true;
			relatedDbFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			var dbNumberFilter = relatedDbFilter.SelectedFilters.AddFilterStrip<ModuleNumberRangeFilter>("Database Number");
			dbNumberFilter.IsActive = true;
			dbNumberFilter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
			dbNumberFilter.Property1 = 1001;
			orgsCollection.Load(filterBizObj.Filter);
			AssertEquals(1, orgsCollection.Count);
			AssertCollectionContains(org1, orgsCollection);

			dbNumberFilter.IsActive = false;
			var dbProductFilter = relatedDbFilter.SelectedFilters.AddTextFilterStrip("Product");
			dbProductFilter.IsActive = true;
			dbProductFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			dbProductFilter.Property = ProductTypes.Codes.Enterprise;
			orgsCollection.Load(filterBizObj.Filter);
			AssertEquals(2, orgsCollection.Count);
			AssertCollectionContains(org1, orgsCollection);
			AssertCollectionContains(org2, orgsCollection);

			dbProductFilter.IsActive = false;
			var dbTypeFilter = relatedDbFilter.SelectedFilters.AddTextFilterStrip("Database Licence Type");
			dbTypeFilter.IsActive = true;
			dbTypeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			dbTypeFilter.Property = DatabaseTypes.Codes.Production;
			orgsCollection.Load(filterBizObj.Filter);
			AssertEquals(2, orgsCollection.Count);
			AssertCollectionContains(org2, orgsCollection);
			AssertCollectionContains(org3, orgsCollection);

			var enterpriseFilter = relatedDbFilter.SelectedFilters.AddGuidFilterStrip("Enterprise Code", db1.LD_LE);
			enterpriseFilter.IsActive = true;
			orgsCollection.Load(filterBizObj.Filter);
			AssertEquals("Expect no exception", 2, orgsCollection.Count);
		}

		#endregion

		#region Staff Assignments

		public void TestStaffAssignmentsProductFilters()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var org1Assignment = Factory.NewWithValidTestData<OrgStaffAssignments>();
			org1Assignment.O8_OH = org1.PK;
			org1Assignment.O8_Product = ProductTypes.Codes.Enterprise;

			var org2Assignment = Factory.NewWithValidTestData<OrgStaffAssignments>();
			org2Assignment.O8_OH = org2.PK;
			org2Assignment.O8_Product = ProductTypes.Codes.GLOW;

			Factory.Save();

			var filterStripBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBizO["Staff Assignments - Product"];
			filter.IsActive = true;
			filter.Property = ProductTypes.Codes.Enterprise;

			var orgCollection = new OrgHeaderCollection(Factory, filterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder( [org1], orgCollection);

			filter.Property = ProductTypes.Codes.GLOW;
			orgCollection = new OrgHeaderCollection(Factory, filterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder([org2], orgCollection);
		}

		#endregion

		#region Implementation

		EDIOrgHeader GetOrgHeaderWithoutExpiryDate(string orgName)
		{
			var licHeader = GetLicHeader(orgName);
			return licHeader.Company.Header;
		}

		EDIOrgHeader GetOrgHeaderWithExpiryDate(string orgName, ZDateTime expiryDate)
		{
			var licHeader = GetLicHeader(orgName);
			licHeader.LA_ContractExpiryDate = expiryDate;
			return licHeader.Company.Header;
		}

		EDIOrgHeader GetOrgHeaderWithSqlServer(string orgName, SchemaColumn column, object value)
		{
			var licHeader = GetLicHeader(orgName);
			LicenceDatabase database = licHeader.Database;
			database[column] = value;
			return licHeader.Company.Header;
		}

		EDIOrgHeader GetOrgHeaderWithVersion(string orgName, ZDateTime versionDate, int major, int minor, int release, int patch)
		{
			var licHeader = GetLicHeader(orgName);

			ReleaseBuild build = Factory.NewWithValidTestData<ReleaseBuild>();
			build.HL_ExeVersionDate = versionDate;
			build.HL_MajorVersion = major;
			build.HL_MinorVersion = minor;
			build.HL_Release = release;
			build.HL_Patch = patch;

			LicenceDatabase dataBase = licHeader.Database;
			dataBase.LD_HL_CurrentRunningVersion = build.PK;
			dataBase.LD_HL_CurrentSentVersion = build.PK;
			return licHeader.Company.Header;
		}

		EDIOrgHeader GetOrgHeaderWithLicencedModule(string orgName, string moduleCode, string licenceType)
		{
			return GetLicHeader(orgName, moduleCode, licenceType).Company.Header;
		}

		LicenceHeader GetLicHeader(string orgName, string moduleCode, string licenceType)
		{
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();

			org.OH_FullName = orgName;

			var company = Factory.NewWithValidTestData<LicenceCompany>();
			company.LC_OH = org.PK;

			return AddDatabase(company, moduleCode, licenceType);
		}

		LicenceHeader AddDatabase(LicenceCompany company, string moduleCode, string licenceType)
		{
			var dataBase = Factory.NewWithValidTestData<LicenceDatabase>();
			dataBase.LD_Product = ProductTypes.Codes.Enterprise;

			var header = Factory.NewWithValidTestData<LicenceHeader>();
			header.LA_LC = company.PK;
			header.LA_LD = dataBase.PK;

			var module = Factory.New<LicenceModules>();
			module.LM_GroupModuleCode = moduleCode;
			module.LM_LA = header.PK;
			module.LM_LicenceType = licenceType;
			if (licenceType != LicenceTypes.Codes.NON)
			{
				module.LM_UserCount = 1;
				module.LM_ExpiryDate = ZDateTime.Now.AddMonths(1);
			}

			return header;
		}

		LicenceHeader GetLicHeader(string orgName)
		{
			return GetLicHeader(orgName, "COR", "PUR");
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EDIOrganisationFilterBusinessObjectCore();
		}

		#endregion
	}
}
