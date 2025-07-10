using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Licensing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(LicenceDatabase))]
	public class LicenceDatabaseTest : SecurityBusinessObjectTestCase
	{
		public void TestLD_Status()
		{
			var build = Factory.New<ReleaseBuild>();
			build.HL_MajorVersion = 1;
			build.HL_MinorVersion = 4;
			build.HL_Release = 5890;
			build.HL_Patch = 9;

			EDIDataRegistry.Instance.UserCreatedCompanyReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "15.5.4.0");
			AssertEquals(TriState.False, LicenceDatabase.SupportsUserCreatedCompanies(build));

			var db = Factory.NewWithValidTestData<LicenceDatabase>();
			db.LD_Status = DatabaseStatusList.Codes.NON;
			db.LD_Password = "pwd";
			db.LD_Status = DatabaseStatusList.Codes.REG;
			Factory.Save();

			db.LD_Status = DatabaseStatusList.Codes.NON;
			AssertEquals("password not changed by setting status REG -> NON", "pwd", db.LD_Password);

			db.LD_Status = DatabaseStatusList.Codes.REG;
			AssertEquals("password not changed by setting status NON -> REG", "pwd", db.LD_Password);

			db.LD_HostDBName = "whatever";
			Factory.Save();
			AssertEquals("password not changed by setting status REG -> NON -> REG and saving", "pwd", db.LD_Password);

			db.LD_Status = DatabaseStatusList.Codes.NON;
			AssertEquals("password not changed by setting status REG -> NON", "pwd", db.LD_Password);

			Factory.Save();
			AssertEquals("password reset by setting status REG -> NON and saving", "-", db.LD_Password);

			db.LD_Password = "pwd";
			db.LD_Status = DatabaseStatusList.Codes.REG;
			Factory.Save();

			db.LD_Status = "";
			AssertEquals("password not changed", "pwd", db.LD_Password);
			Factory.Save();
			AssertEquals("password reset by saving", "-", db.LD_Password);

			db.LD_Status = DatabaseStatusList.Codes.REG;
			db.LD_HL_CurrentRunningVersion = build.PK;
			db.LD_Password = "pwd";
			Factory.Save();

			db.LD_Status = "";
			Factory.Save();
			AssertEquals("password reset to blank for old registration software", "", db.LD_Password);
		}

		public void TestDatabaseId()
		{
			var db = Factory.New<LicenceDatabase>();
			AssertEquals("", db.DatabaseId);

			db.LD_DatabaseNumber = 1;
			AssertEquals(Base27Encoding.Encode(db.LD_DatabaseNumber), db.DatabaseId);
		}

		public void TestVersionSupportsCr8Cr9()
		{
			var licenceDb = Factory.New<LicenceDatabase>();

			AssertEquals(false, licenceDb.VersionSupportsCr8Cr9);

			var build = Factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(1, 1, 2, 2);
			licenceDb.LD_HL_CurrentRunningVersion = build.PK;

			AssertEquals(false, licenceDb.VersionSupportsCr8Cr9);

			EDIDataRegistry.Instance.Cr8Cr9ReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.3");
			AssertEquals(false, licenceDb.VersionSupportsCr8Cr9);

			EDIDataRegistry.Instance.Cr8Cr9ReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.2");
			AssertEquals(true, licenceDb.VersionSupportsCr8Cr9);

			EDIDataRegistry.Instance.Cr8Cr9ReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.1.0");
			AssertEquals(true, licenceDb.VersionSupportsCr8Cr9);

			EDIDataRegistry.Instance.Cr8Cr9ReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.1.1");
			AssertEquals(false, licenceDb.VersionSupportsCr8Cr9);

			EDIDataRegistry.Instance.Cr8Cr9ReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.3, 1.1.1.1");
			AssertEquals(false, licenceDb.VersionSupportsCr8Cr9);

			EDIDataRegistry.Instance.Cr8Cr9ReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.3, 1.1.1.1, 1.1.1.0");
			AssertEquals(true, licenceDb.VersionSupportsCr8Cr9);
		}

		[TestDate(2006, 10, 06, 10, 10, 10)]
		public void TestShouldUpdateFromHeartbeat()
		{
			LicenceDatabase testDB = Factory.New<LicenceDatabase>();
			AssertEquals("By default it should update the first time", true, testDB.ShouldUpdateFromHeartbeat);

			testDB.LD_LastHeartbeat = TestDateAttribute.Date.AddHours(-23).AddMinutes(1);
			AssertEquals("It should NOT update the heartbeat - not 23 hours passed since start date", false, testDB.ShouldUpdateFromHeartbeat);

			testDB.LD_LastHeartbeat = TestDateAttribute.Date.AddHours(-23);
			AssertEquals("It should NOT update the heartbeat - not 23 hours passed since start date (must be over 23 hours)", false, testDB.ShouldUpdateFromHeartbeat);

			testDB.LD_LastHeartbeat = TestDateAttribute.Date.AddHours(-23).AddMinutes(-1);
			AssertEquals("It should update the heartbeat - 23 hours passed since start date", true, testDB.ShouldUpdateFromHeartbeat);

			testDB.LD_LastHeartbeat = TestDateAttribute.Date.AddDays(6); // Date In Future somehow
		}

		public void TestReleaseRingDescription()
		{
			LicenceDatabase testDB = Factory.New<LicenceDatabase>();
			testDB.LD_ReleaseRing = ReleaseRings.Codes.ALP;
			AssertEquals("The list descriptions should match", ReleaseRings.Lookup(ReleaseRings.Codes.ALP).LongDescription, testDB.ReleaseRingDescription);

			testDB.LD_ReleaseRing = "123";
			AssertEquals("The list descriptions should be empty", string.Empty, testDB.ReleaseRingDescription);
		}

		public void TestIsEnterpriseFamilyDatabase()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();
			var database = Factory.New<LicenceDatabase>();

			database.LD_Product = ProductTypes.Codes.Enterprise;
			Assert("This database should be Enterprise family", database.IsEnterpriseFamilyDatabase);

			database.LD_Product = ProductTypes.Codes.CargoWiseOne;
			Assert("This database should be Enterprise family", database.IsEnterpriseFamilyDatabase);

			database.LD_Product = ProductTypes.Codes.ProductivityWise;
			Assert("This database should be Enterprise family", database.IsEnterpriseFamilyDatabase);

			database.LD_Product = "BBB";
			Assert("This database should not be Enterprise family", !database.IsEnterpriseFamilyDatabase);

			database.LD_Product = ProductTypes.Codes.CargoWiseNext;
			Assert("This database should be Enterprise family", database.IsEnterpriseFamilyDatabase);
		}

		public void TestLoad()
		{
			EDIOrgHeader organisation1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader organisation2 = Factory.NewWithValidTestData<EDIOrgHeader>();

			organisation1.CreateAndLoadLicenceForOrg();
			organisation2.CreateAndLoadLicenceForOrg();

			organisation1.LicenceEnterpriseCode = "LE1";
			organisation2.LicenceEnterpriseCode = "LE2";

			organisation1.LicCompany.LC_CompanyCode = "LC1";
			organisation2.LicCompany.LC_CompanyCode = "LC2";

			LicenceDatabase database1 = organisation1.LicCompany.LicDatabases.AddNew();
			LicenceDatabase database2 = organisation2.LicCompany.LicDatabases.AddNew();
			LicenceDatabase database1b = organisation1.LicCompany.LicDatabases.AddNew();
			LicenceDatabase database1c = organisation1.LicCompany.LicDatabases.AddNew();

			database1.LD_ServerCode = "LD1";
			database2.LD_ServerCode = "LD2";
			database1b.LD_ServerCode = "LD3";
			database1c.LD_ServerCode = "LD4";
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertEquals("Should have loaded database1.", database1.PK, LicenceDatabase.Load(newFactory, "LE1", "LC1", "LD1").PK);
			AssertEquals("Should have loaded database2.", database2.PK, LicenceDatabase.Load(newFactory, "LE2", "LC2", "LD2").PK);
			AssertEquals("Should have loaded database1b.", database1b.PK, LicenceDatabase.Load(newFactory, "LE1", "LC1", "LD3").PK);

			AssertNull("Should not have loaded anything.", LicenceDatabase.Load(newFactory, "LE3", "LC1", "LD1"));
			AssertNull("Should not have loaded anything.", LicenceDatabase.Load(newFactory, "LE1", "LC3", "LD1"));

			AssertEquals("Should have loaded database1.", database1.PK, LicenceDatabase.Load(newFactory, "LE1", "", "LD1").PK);
			AssertEquals("Should have loaded database2.", database2.PK, LicenceDatabase.Load(newFactory, "LE2", "", "LD2").PK);
			AssertNull("Should not have loaded anything.", LicenceDatabase.Load(newFactory, "LE3", "", "LD1"));
			AssertEquals("Should have loaded database1b", database1b.PK, LicenceDatabase.Load(newFactory, "LE1", "", "LD3").PK);
		}

		public void TestLoadFromEnterpriseAndServerCode()
		{
			EDIOrgHeader organisation1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader organisation2 = Factory.NewWithValidTestData<EDIOrgHeader>();

			organisation1.CreateAndLoadLicenceForOrg();
			organisation2.CreateAndLoadLicenceForOrg();

			organisation1.LicenceEnterpriseCode = "LE1";
			organisation2.LicenceEnterpriseCode = "LE2";

			organisation1.LicCompany.LC_CompanyCode = "LC1";
			organisation2.LicCompany.LC_CompanyCode = "LC2";

			LicenceDatabase database1a = organisation1.LicCompany.LicDatabases.AddNew();
			LicenceDatabase database1b = organisation1.LicCompany.LicDatabases.AddNew();
			LicenceDatabase database1c = organisation1.LicCompany.LicDatabases.AddNew();

			LicenceDatabase database2 = organisation2.LicCompany.LicDatabases.AddNew();

			database1a.LD_ServerCode = "LD1";
			database1b.LD_ServerCode = "LD2";
			database1c.LD_ServerCode = "LD3";
			database2.LD_ServerCode = "LD1";
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertEquals("Should have loaded database1a", database1a.PK, LicenceDatabase.LoadFromEnterpriseAndServerCode(newFactory, "LE1", "LD1").PK);
			AssertEquals("Should have loaded database1b", database1b.PK, LicenceDatabase.LoadFromEnterpriseAndServerCode(newFactory, "LE1", "LD2").PK);
			AssertEquals("Should have loaded database1c", database1c.PK, LicenceDatabase.LoadFromEnterpriseAndServerCode(newFactory, "LE1", "LD3").PK);
			AssertEquals("Should have loaded database2", database2.PK, LicenceDatabase.LoadFromEnterpriseAndServerCode(newFactory, "LE2", "LD1").PK);

			AssertNull("Should not have loaded anything", LicenceDatabase.LoadFromEnterpriseAndServerCode(newFactory, "LE3", "LD1"));
			AssertNull("Should not have loaded anything", LicenceDatabase.LoadFromEnterpriseAndServerCode(newFactory, "LE1", "LD4"));
		}

		#region Builds

		[TestDate(2015, 6, 7, 11, 30, 0)]
		public void TestCurrentVersion()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();
			ReleaseBuild build = Factory.New<ReleaseBuild>();

			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();

			database.LD_HL_CurrentRunningVersion = build.PK;
			AssertEquals(build.PK, database.CurrentVersion.PK);
			Assert("Upgrade Method is empty", database.LD_AvailableUpgradeMethod.IsEmpty);

			ReleaseBuild buildEWA = Factory.New<ReleaseBuild>();

			database.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			database.LD_HL_CurrentRunningVersion = buildEWA.PK;
			AssertEquals(buildEWA.PK, database.CurrentVersion.PK);
			AssertEquals("Upgrade Method still BLK", UpgradeMethods.Codes.Blocked, database.LD_AvailableUpgradeMethod);

			database.LD_AvailableUpgradeMethod = "";

			ReleaseBuild buildHTP = Factory.New<ReleaseBuild>();
			HttpDownload httpVersion = new HttpDownload();
			buildHTP.HL_MajorVersion = httpVersion.VersionMajorNumber;
			buildHTP.HL_MinorVersion = httpVersion.VersionMinorNumber;
			buildHTP.HL_Release = httpVersion.VersionReleaseNumber;
			database.LD_HL_CurrentRunningVersion = buildHTP.PK;
			AssertEquals(buildHTP.PK, database.CurrentVersion.PK);
			AssertEquals("Upgrade Method is HTP", UpgradeMethods.Codes.Http, database.LD_AvailableUpgradeMethod);
		}

		public void TestSentVersion()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();
			ReleaseBuild build = Factory.New<ReleaseBuild>();

			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			database.LD_HL_CurrentSentVersion = build.PK;
			AssertEquals(build.PK, database.SentVersion.PK);
		}

		public void TestCurrentVersionExeDate()
		{
			TestVersionProperty("CurrentVersionExeDate", ReleaseBuildSchema.Constants.HL_ExeVersionDate, LicenceDatabaseSchema.LD_HL_CurrentRunningVersion);
		}

		public void TestCurrentVersionRelease()
		{
			TestVersionProperty("CurrentVersionRelease", ReleaseBuild.Schema.ReleaseDisplayText, LicenceDatabaseSchema.LD_HL_CurrentRunningVersion);
		}

		public void TestSentVersionExeDate()
		{
			TestVersionProperty("SentVersionExeDate", ReleaseBuildSchema.Constants.HL_ExeVersionDate, LicenceDatabaseSchema.LD_HL_CurrentSentVersion);
		}

		public void TestSentVersionRelease()
		{
			TestVersionProperty("SentVersionRelease", ReleaseBuild.Schema.ReleaseDisplayText, LicenceDatabaseSchema.LD_HL_CurrentSentVersion);
		}

		void TestVersionProperty(string propertyToTest, string buildPropertyBeingProxied, SchemaColumn databaseBuildForeignKey)
		{
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_ReleaseStatus = ReleaseRings.Codes.DPR;
			build.HL_Release = 200;
			build.HL_Patch = 10;
			build.HL_ExeVersionDate = ZDateTime.Now;
			DatabaseForTest[databaseBuildForeignKey] = build.PK;
			AssertEquals(propertyToTest, build[buildPropertyBeingProxied], DatabaseForTest[propertyToTest]);
		}

		public void TestLatestSentOrCurrentVersion()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(1, 2, 3, 4);
			ReleaseBuild laterBuild = Factory.New<ReleaseBuild>();
			laterBuild.VersionNumber = new VersionNumber(1, 2, 4, 5);

			LicenceDatabase currentNullSentNull = testHeader.LicCompany.LicDatabases.AddNew();
			LicenceDatabase currentNotNullSentNull = testHeader.LicCompany.LicDatabases.AddNew();
			LicenceDatabase currentNullSentNotNull = testHeader.LicCompany.LicDatabases.AddNew();
			LicenceDatabase currentLater = testHeader.LicCompany.LicDatabases.AddNew();
			LicenceDatabase sentLater = testHeader.LicCompany.LicDatabases.AddNew();

			currentNotNullSentNull.LD_HL_CurrentRunningVersion = build.PK;
			currentNullSentNotNull.LD_HL_CurrentSentVersion = build.PK;
			currentLater.LD_HL_CurrentRunningVersion = laterBuild.PK;
			currentLater.LD_HL_CurrentSentVersion = build.PK;
			sentLater.LD_HL_CurrentSentVersion = laterBuild.PK;
			sentLater.LD_HL_CurrentRunningVersion = build.PK;

			LicenceDatabase database = currentNullSentNull;
			AssertNull(database.CurrentVersion);
			AssertNull(database.SentVersion);
			AssertNull(database.LatestSentOrCurrentVersion);

			database = currentNotNullSentNull;
			AssertNotNull(database.CurrentVersion);
			AssertNull(database.SentVersion);
			AssertEquals(database.LatestSentOrCurrentVersion, database.CurrentVersion);

			database = currentNullSentNotNull;
			AssertNull(database.CurrentVersion);
			AssertNotNull(database.SentVersion);
			AssertEquals(database.LatestSentOrCurrentVersion, database.SentVersion);

			database = currentLater;
			AssertNotNull(database.CurrentVersion);
			AssertNotNull(database.SentVersion);
			AssertEquals(database.LatestSentOrCurrentVersion, database.CurrentVersion);

			database = sentLater;
			AssertNotNull(database.CurrentVersion);
			AssertNotNull(database.SentVersion);
			AssertEquals(database.LatestSentOrCurrentVersion, database.SentVersion);
		}

		#endregion

		#region LicenceHeader

		public void TestLicenceHeadersForAllCompanies()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_Code = "ABCXYZ";
			testHeader.MainAddress.OA_Address1 = "Address 1";
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();

			Factory.Save();
			AssertEquals("There is 1 item in the collection", 1, database.LicHeadersForAllCompanies.Count);
		}

		public void TestActiveLicenceHeadersForAllCompanies()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_Code = "ABCXYZ";
			testHeader.MainAddress.OA_Address1 = "Address 1";
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();

			Factory.Save();
			AssertEquals("There is 1 item in the collection", 1, database.ActiveLicHeadersForAllCompanies.Count);

			testHeader.LicCompany.GetHeader(database).LA_IsActive = false;
			database.ActiveLicHeadersForAllCompanies.Load();
			AssertEquals("inactive not included", 0, database.ActiveLicHeadersForAllCompanies.Count);
		}

		#endregion

		#region Company

		public void TestCompanyCode()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_Code = "ABCXYZ";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			AssertEquals("Code is same", "ABCXYZ", database.CompanyCode);
		}

		public void TestCompanyName()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_FullName = "ABCXYZ International";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			AssertEquals("Name is same", "ABCXYZ International", database.CompanyName);
		}

		#endregion

		#region User Management

		public void TestShouldCloneContactOnWebAccessOrgChanged()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			AssertEquals("Database is not saved yet", false, db.ShouldCloneContactOnWebAccessOrgChanged);
			var webAccessOrgPk = db.LD_OH_WebAccessOrg;

			var contact = licence.Company.Header.Contacts.AddNew();
			var userAccount = Factory.New<EdiCustomerUserAccount>();
			userAccount.EUA_UserID = "AAA";
			userAccount.EUA_LD = db.PK;
			userAccount.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();
			AssertEquals("All conditions fulfilled", true, db.ShouldCloneContactOnWebAccessOrgChanged);

			userAccount.EUA_OC_WebAccessContact = ZGuid.Empty;
			Factory.Save();
			AssertEquals("Database has no user linked to contact", false, db.ShouldCloneContactOnWebAccessOrgChanged);

			db.LD_StaffFirstReportUtc = ZDateTime.UtcNow;
			db.LD_IsActive = false;
			AssertEquals("Database is not active", false, db.ShouldCloneContactOnWebAccessOrgChanged);

			db.LD_IsActive = true;
			db.LD_OH_WebAccessOrg = ZGuid.Empty;
			AssertEquals("Web access org is not set", false, db.ShouldCloneContactOnWebAccessOrgChanged);

			userAccount.EUA_OC_WebAccessContact = contact.PK;
			db.LD_OH_WebAccessOrg = webAccessOrgPk;
			Factory.Save();
			db.LD_LicenceType = DatabaseTypes.Codes.Production;
			db.LD_Product = ProductTypes.Codes.EHub;
			AssertEquals("Should work for any product", true, db.ShouldCloneContactOnWebAccessOrgChanged);
		}

		#endregion

		#region Connection Details

		public void TestConnectionDetailsNote()
		{
			LicenceDatabase licDB = Factory.New<LicenceDatabase>();
			AssertNotNull(licDB.ConnectionDetailsNote);

			HiddenRtfNote note1 = licDB.ConnectionDetailsNote;
			HiddenRtfNote note2 = licDB.ConnectionDetailsNote;

			AssertEquals(note1, note2);
		}

		public void TestDeleteLicenceDatabaseDeletesConnectionDetails()
		{
			BusinessObjectFactory clearFactory1 = new BusinessObjectFactory();
			BusinessObjectFactory clearFactory2 = new BusinessObjectFactory();

			OrgHeader testHeaderWithAddress = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			EDIOrgHeader testHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();

			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			database.LD_OA_SoftwareInstallAddressDetails = testHeaderWithAddress.MainAddress.PK;
			ZBlob blobForTest = new ZBlob(new byte[4]);
			database.ConnectionDetails = blobForTest;

			Factory.Save();

			ZQuery filter = new ZQuery(StmNoteSchema.ST_ParentID, database.PK).AddToFilter(StmNoteSchema.ST_Table, database.TableName);
			var hiddenNote = clearFactory1.LoadTop1<HiddenStmNote>(filter);
			AssertNotNull("HiddenNote exists in the database", hiddenNote);
			testHeader.LicCompany.LicDatabases.RemoveAndDeleteAll();
			Factory.Save();
			hiddenNote = clearFactory2.LoadTop1<HiddenStmNote>(filter);
		}

		#endregion

		#region Logging

		public void TestLogging()
		{
			var company = Factory.NewWithValidTestData<LicenceCompany>();
			LicenceDatabase licDB = company.LicDatabases.AddNew();
			licDB.LD_ServerCode = "MMM";
			licDB.LD_DBServerSecurityMode = "LLL";
			licDB.LD_LicenceType = "PPP";
			licDB.LD_AvailableUpgradeMethod = "BLK";
			licDB.LD_ReleaseRing = "GPR";

			AssertEquals("Logs Count", 0, licDB.Logs.GetAllLogs().Count);

			Factory.Save();

			ZString expectedReference = "";
			AssertEquals("No ADD event log", null, licDB.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem));

			licDB.LD_ServerCode = "NNN";
			licDB.LD_DBServerSecurityMode = "OOO";
			licDB.LD_LicenceType = "QQQ";
			licDB.LD_AvailableUpgradeMethod = "SSS";
			licDB.LD_ReleaseRing = "DPR";

			Factory.Save();

			expectedReference = "Server NNN(MMM) - DB Security Mode: OOO(LLL) Licence Type: QQQ(PPP) Upgrade Method: SSS(BLK) Ring: DPR(GPR)";
			AssertEquals("Log Reference should be", expectedReference, licDB.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);

			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_MajorVersion = 1;
			build.HL_MinorVersion = 1;
			build.HL_Release = 2008;
			build.HL_Patch = 123;
			licDB.LD_HL_CurrentRunningVersion = build.PK;
			Factory.Save();
			expectedReference = "Server NNN - Version: 1.1.2008.123()";
			AssertEquals("Log Reference should be", expectedReference, licDB.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);

			var build2 = Factory.New<ReleaseBuild>();
			build2.HL_MajorVersion = 2;
			build2.HL_MinorVersion = 2;
			build2.HL_Release = 2009;
			build2.HL_Patch = 456;
			licDB.LD_HL_CurrentRunningVersion = build2.PK;
			Factory.Save();
			expectedReference = "Server NNN - Upgrade Method: HTP(SSS) Version: 2.2.2009.456(1.1.2008.123)";
			AssertEquals("Log Reference should be", expectedReference, licDB.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);
		}

		public void TestNoAutoLog()
		{
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			Factory.Save();
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var licenceDatabase1 = factory1.Load<LicenceDatabase>(licenceDatabase.PK);
			AssertEquals(0, licenceDatabase1.Logs.Find(log => log.SL_SE_NKEvent == "ADD").Count());
			licenceDatabase.LD_Product = "ENT";
			Factory.Save();
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var licenceDatabase2 = factory2.Load<LicenceDatabase>(licenceDatabase.PK);
			AssertEquals(0, licenceDatabase2.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
		}

		#endregion

		#region Related Logs

		public void TestBusinessObjectsWithRelatedEvents()
		{
			LicenceCompany company = Factory.New<LicenceCompany>();
			LicenceDatabase licDB = company.LicDatabases.AddNew();
			AssertEquals("Should be no business object with related logs", 0, licDB.BusinessObjectsWithRelatedEvents.Length);

			licDB.Connections.AddNew();
			AssertEquals("Should be 1 business objects with related logs", 1, licDB.BusinessObjectsWithRelatedEvents.Length);
		}

		#endregion

		#region Connections

		public void TestConnectionsSaveAndDelete()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();

			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			database.LD_OA_SoftwareInstallAddressDetails = testHeader.MainAddress.PK;
			database.Connections.AddNew();
			database.Connections.AddNew();
			AssertEquals("Database Connections count is 2", 2, database.Connections.Count);

			Factory.Save();

			var testLoadingHeader = Factory.Load<EDIOrgHeader>(testHeader.PK);
			AssertEquals("Database Connections count is 2", 2, testLoadingHeader.LicCompany.LicDatabases[0].Connections.Count);

			testLoadingHeader.Delete();

			var connections = Factory.Load<LicenceConnection>(new ZQuery(LicenceConnectionSchema.LK_LD, SQLComparisonOperator.Equal, database.PK));
			AssertEquals("All associated connections should be deleted from the database", 0, connections.Length);
		}

		#endregion

		#region Address As String

		public void TestAddressAsString()
		{
			EDIOrgHeader testHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			testHeader.OH_Code = "ABCXYZ";
			testHeader.OH_RL_NKClosestPort = "AUBNE";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();
			LicenceDatabase licDB = testHeader.LicEnterprise.Databases.AddNew();
			AssertEquals("Address is blank", "", licDB.AddressAsString);

			licDB.LD_OA_SoftwareInstallAddressDetails = testHeader.MainAddress.PK;
			AssertEquals(testHeader.MainAddress.AddressAsASingleLine, licDB.AddressAsString);
		}

		#endregion

		#region Default Values

		public void TestDefaultValuesSet()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase licDatabase = testHeader.LicCompany.LicDatabases.AddNew();
			AssertEquals("LD_LicenceType", licDatabase.LD_LicenceType, DatabaseTypes.Codes.Production);
			AssertEquals("LD_DBServerSecurityMode", licDatabase.LD_DBServerSecurityMode, DatabaseSecurityModePairList.Codes.Locked);
			AssertEquals("LD_Product", licDatabase.LD_Product, ProductTypes.Codes.CargoWiseOne);
			AssertEquals(licDatabase.LD_Billable, DatabaseBillableFlagList.Codes.YesCustomer);
		}

		#endregion

		#region LicenceExpiry

		public void TestCustomExpiryNotes()
		{
			HeaderForTest.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = HeaderForTest.LicCompany.LicDatabases.AddNew();
			Factory.Save();

			database.CustomExpiredNote.Text = "MSG1";
			database.CustomExpiryWeekNote.Text = "MSG2";
			database.CustomExpiryMonthNote.Text = "MSG3";

			AssertEquals("MSG1", database.CustomExpiredNote.Text);
			AssertEquals("MSG2", database.CustomExpiryWeekNote.Text);
			AssertEquals("MSG3", database.CustomExpiryMonthNote.Text);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var db2 = factory2.Load<LicenceDatabase>(database.PK);
			AssertEquals("MSG1", db2.CustomExpiredNote.Text);
			AssertEquals("MSG2", db2.CustomExpiryWeekNote.Text);
			AssertEquals("MSG3", db2.CustomExpiryMonthNote.Text);
		}

		#endregion

		#region Licence Key Generation

		public void TestDatabaseVersionIsDeployable()
		{
			LicenceDatabase testDB = Factory.New<LicenceDatabase>();
			Assert("Not Deployable - no version", !testDB.VersionIsDeployable);

			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_ExeVersionDate = new ZDateTime(2005, 10, 01);
			testDB.LD_HL_CurrentRunningVersion = build.PK;
			Assert("Not Deployable - version before new licencing changes", !testDB.VersionIsDeployable);

			build = Factory.New<ReleaseBuild>();
			build.HL_ExeVersionDate = new ZDateTime(2005, 10, 10);
			testDB.LD_HL_CurrentRunningVersion = build.PK;
			Assert("Not Deployable - Newer changes have been made thus extending the minimum deployment date", !testDB.VersionIsDeployable);

			build.HL_ExeVersionDate = new ZDateTime(2005, 11, 12);
			Assert("Not Deployable - version contains Licence Update bug", !testDB.VersionIsDeployable);

			build.HL_ExeVersionDate = LicenceDatabase.DateFromWhichLicenceIsAutoDeployable;
			Assert("Deployable - version can be deployed", testDB.VersionIsDeployable);
		}

		public void TestPublicEmailIsDeployable()
		{
			LicenceDatabase testDB = Factory.New<LicenceDatabase>();
			Assert("Not Deployable - no email", !testDB.PublicEmailIsDeployable);

			testDB.LD_PublicEmailAddressForUpdate = "blah";
			Assert("Not Deployable - invalid email", !testDB.PublicEmailIsDeployable);

			testDB.LD_PublicEmailAddressForUpdate = "blah@blah.com";
			Assert("Deployable - valid email", testDB.PublicEmailIsDeployable);
		}

		#endregion

		#region Request Version Report

		[TestDate(2011, 7, 1, 10, 20, 0)]
		public void TestRequestVersionReportFromLegacySystem()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_Code = "ABCXYZ";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();

			database.LD_PublicEmailAddressForUpdate = ZString.Empty;
			database.RequestVersionReportFromLegacySystem();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			database.LD_PublicEmailAddressForUpdate = "123213";
			database.RequestVersionReportFromLegacySystem();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_ExeVersionDate = LicenceDatabase.DateFromWhichLicenceIsAutoDeployable;
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_PublicEmailAddressForUpdate = "xerxes@edi.com.au";
			database.RequestVersionReportFromLegacySystem();
			var emails = Factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.MI_Subject, LicenceDatabase.LegacyVersionReportRequestSubject));
			AssertEquals(1, emails.Length);

			database.LD_PublicEmailAddressForUpdate = "10mindelay@cargowise.com";
			database.RequestVersionReportAsConfirmationFromLegacySystem();
			emails = Factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.MI_Subject, LicenceDatabase.LegacyVersionReportRequestSubject));
			AssertEquals(2, emails.Length);
			var delayedEmail = emails.First(x => x.MailRecipients[0].EmailAddress == "10mindelay@cargowise.com");
			AssertEquals(TestDateAttribute.Date + TimeSpan.FromMinutes(10), delayedEmail.MI_SendDateTime);
		}

		#endregion

		#region Reset Heartbeat

		public void TestResetHeartbeat()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_Code = "ABCXYZ";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			database.LD_PublicEmailAddressForUpdate = "xerxes@edi.com.au";
			database.LD_HostServerSID = ZGuid.NewZGuid();
			database.LD_LicenceExpiry = ZDateTime.Today.AddDays(45);
			database.LD_DatabaseFilePathDetail = "Database information";
			database.LD_HostDBName = "DBName";
			database.LD_HostDBInstance = "INSTANCE";
			database.LD_HostServerName = "SERVERNAME";
			database.LD_InternalPop3EmailAddress = "XHC";
			database.LD_InternalPop3Port = 110;
			database.LD_InternalPop3UserName = "xerxesb";
			database.LD_InternalSmtpEmailAddress = "XHC";
			database.LD_InternalSmtpPort = 25;

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			bool result = database.ResetHeartbeat();
			Assert("Not Reset", !result);
			Assert("SID is NOT reset - Version Not Deployable", database.LD_HostServerSID != ZGuid.Empty);

			EDIDataRegistry.Instance.AllSystemMessagesViaEhubReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "16.10.20.0");
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_Product = ProductTypes.Codes.Enterprise;
			build.HL_ExeVersionDate = LicenceDatabase.DateFromWhichDatabaseHeartbeatCanBeReset;
			database.LD_HL_CurrentRunningVersion = build.PK;

			database.LD_Status = DatabaseStatusList.Codes.REG;
			Assert("Not Reset", !database.ResetHeartbeat());

			database.LD_Status = DatabaseStatusList.Codes.Preregistered;
			Assert("Reset", database.ResetHeartbeat());

			database.LD_Status = DatabaseStatusList.Codes.NON;
			Assert("Reset", database.ResetHeartbeat());

			build.VersionNumber = new VersionNumber(16, 10, 20, 0);
			Assert("Not Reset", !database.ResetHeartbeat());

			AssertEquals("Public EMail Address not reset", "xerxes@edi.com.au", database.LD_PublicEmailAddressForUpdate);
			AssertEquals("SID is reset", ZGuid.Empty, database.LD_HostServerSID);
			AssertEquals("LicenceExpiry Date is cleared", ZDateTime.Empty, database.LD_LicenceExpiry);
			AssertEquals("LastHeartbeat Date is cleared", ZDateTime.Empty, database.LD_LastHeartbeat);
			var emails = Factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.MI_Subject, LicenceDatabase.LegacyVersionReportRequestSubject));
			AssertEquals("version report should be requested", 2, emails.Length);
		}

		#endregion

		#region Supported Upgrade Method

		#region Has No Email Address and Current Version

		public void TestSupportedUpgradeMethodNoEmailNoCurrentVersion()
		{
			DatabaseForTest.LD_PublicEmailAddressForUpdate = "";
			DatabaseForTest.LD_HL_CurrentRunningVersion = ZGuid.Empty;

			DatabaseForTest.LD_AvailableUpgradeMethod = "";
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Blocked, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertIsSupportedMethodForNoEmail();

			DatabaseForTest.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Blocked, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertIsSupportedMethodForNoEmail();

			DatabaseForTest.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Blocked, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertIsSupportedMethodForNoEmail();
		}

		#endregion

		#region Has Email Address but no Current Version

		public void TestSupportedUpgradeMethodHasEmailNoCurrentVersion()
		{
			DatabaseForTest.LD_PublicEmailAddressForUpdate = "email@address.com";

			DatabaseForTest.LD_AvailableUpgradeMethod = "";
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Blocked, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertIsSupportedMethodForHasEmailNoVersionOrWithoutACK();

			DatabaseForTest.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Blocked, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertIsSupportedMethodForHasEmailNoVersionOrWithoutACK();

			DatabaseForTest.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Blocked, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertIsSupportedMethodForHasEmailNoVersionOrWithoutACK();
		}

		#endregion

		#region Has Email Address and Current Version Not Supporting Acknowledgment or Http

		public void TestSupportedUpgradeMethodHasEmailCurrentVersionWithoutACK()
		{
			DatabaseForTest.LD_PublicEmailAddressForUpdate = "email@address.com";
			DatabaseForTest.LD_HL_CurrentRunningVersion = BuildForTest.PK;
			AssertIsSupportedMethodForHasEmailNoVersionOrWithoutACK();

			DatabaseForTest.LD_AvailableUpgradeMethod = "";
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Blocked, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertIsSupportedMethodForHasEmailNoVersionOrWithoutACK();

			DatabaseForTest.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Blocked, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertIsSupportedMethodForHasEmailNoVersionOrWithoutACK();

			DatabaseForTest.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Blocked, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertIsSupportedMethodForHasEmailNoVersionOrWithoutACK();
		}

		#endregion

		#region Has Email Address and Current Version Supporting Acknowledgment but Not Http

		public void TestSupportedUpgradeMethodHasEmailCurrentVersionWithACK()
		{
			DatabaseForTest.LD_PublicEmailAddressForUpdate = "email@address.com";
			DatabaseForTest.LD_HL_CurrentRunningVersion = BuildForTest.PK;

			DatabaseForTest.LD_AvailableUpgradeMethod = "";
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Blocked, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertIsSupportedMethodForHasEmailVersionWithACK();

			DatabaseForTest.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Blocked, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertIsSupportedMethodForHasEmailVersionWithACK();

			DatabaseForTest.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			AssertEquals("HTP not supported", UpgradeMethods.Codes.Blocked, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertIsSupportedMethodForHasEmailVersionWithACK();
		}

		#endregion

		#region Has Email Address and Current Version Supporting Acknowledgment and Http

		public void TestSupportedUpgradeMethodHasEmailCurrentVersionSupportsHttp()
		{
			DatabaseForTest.LD_PublicEmailAddressForUpdate = "email@address.com";
			HttpDownload httpVersion = new HttpDownload();
			BuildForTest.HL_MajorVersion = httpVersion.VersionMajorNumber;
			BuildForTest.HL_MinorVersion = httpVersion.VersionMinorNumber;
			BuildForTest.HL_Release = httpVersion.VersionReleaseNumber;
			DatabaseForTest.LD_HL_CurrentRunningVersion = BuildForTest.PK;

			DatabaseForTest.LD_AvailableUpgradeMethod = "";
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Http, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertIsSupportedMethodForHasEmailVersionSupportsHttp();

			DatabaseForTest.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Blocked, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertIsSupportedMethodForHasEmailVersionSupportsHttp();

			DatabaseForTest.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Http, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertIsSupportedMethodForHasEmailVersionSupportsHttp();
		}

		#endregion

		#region Has No Email Address but has Current Version Supporting Acknowledgment and Http

		public void TestSupportedUpgradeMethodNoEmailCurrentVersionSupportsHttp()
		{
			DatabaseForTest.LD_PublicEmailAddressForUpdate = "";
			HttpDownload httpVersion = new HttpDownload();
			BuildForTest.HL_MajorVersion = httpVersion.VersionMajorNumber;
			BuildForTest.HL_MinorVersion = httpVersion.VersionMinorNumber;
			BuildForTest.HL_Release = httpVersion.VersionReleaseNumber;
			DatabaseForTest.LD_HL_CurrentRunningVersion = BuildForTest.PK;

			DatabaseForTest.LD_AvailableUpgradeMethod = "";
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Blocked, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertIsSupportedMethodForNoEmail();

			DatabaseForTest.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Blocked, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertIsSupportedMethodForNoEmail();

			DatabaseForTest.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Blocked, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertIsSupportedMethodForNoEmail();
		}

		#endregion

		#region Has No Email Address but has Current Version Supporting Acknowledgment and Http

		public void TestSupportedUpgradeMethodCurrentVersionSupportsAllSystemMessages()
		{
			EDIDataRegistry.Instance.AllSystemMessagesViaEhubReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "16.10.15.0");
			DatabaseForTest.LD_PublicEmailAddressForUpdate = "";
			BuildForTest.HL_MajorVersion = 16;
			BuildForTest.HL_MinorVersion = 10;
			BuildForTest.HL_Release = 15;
			DatabaseForTest.LD_HL_CurrentRunningVersion = BuildForTest.PK;

			DatabaseForTest.LD_AvailableUpgradeMethod = "";
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Http, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertEquals("Empty Upgrade Method is not supported", false, DatabaseForTest.IsUpgradeMethodSupported(""));
			Assert("Blocked Upgrade Method is always supported", DatabaseForTest.IsUpgradeMethodSupported(UpgradeMethods.Codes.Blocked));
			Assert("Http Upgrade Method is supported because of current version info", DatabaseForTest.IsUpgradeMethodSupported(UpgradeMethods.Codes.Http));

			DatabaseForTest.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Blocked, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertEquals("Empty Upgrade Method is not supported", false, DatabaseForTest.IsUpgradeMethodSupported(""));
			Assert("Blocked Upgrade Method is always supported", DatabaseForTest.IsUpgradeMethodSupported(UpgradeMethods.Codes.Blocked));
			Assert("Http Upgrade Method is supported because of current version info", DatabaseForTest.IsUpgradeMethodSupported(UpgradeMethods.Codes.Http));

			DatabaseForTest.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			AssertEquals("GetHighestSupportedUpgradeMethod()", UpgradeMethods.Codes.Http, DatabaseForTest.GetHighestSupportedUpgradeMethod());
			AssertEquals("Empty Upgrade Method is not supported", false, DatabaseForTest.IsUpgradeMethodSupported(""));
			Assert("Blocked Upgrade Method is always supported", DatabaseForTest.IsUpgradeMethodSupported(UpgradeMethods.Codes.Blocked));
			Assert("Http Upgrade Method is supported because of current version info", DatabaseForTest.IsUpgradeMethodSupported(UpgradeMethods.Codes.Http));
		}

		#endregion

		#endregion

		#region Properties

		public void TestControlsReadOnlyByDefault()
		{
			AssertEquals("LD_LicenceExpiryInfo.ReadOnly", true, DatabaseForTest.LD_LicenceExpiryInfo.ReadOnly);
			AssertEquals("LD_DatabaseFilePathDetail.ReadOnly", true, DatabaseForTest.LD_DatabaseFilePathDetailInfo.ReadOnly);
			AssertEquals("LD_NoOfActivePrintQueuesInfo.ReadOnly", true, DatabaseForTest.LD_NoOfActivePrintQueuesInfo.ReadOnly);
			AssertEquals("LD_SQLEditionInfo.ReadOnly", true, DatabaseForTest.LD_SQLEditionInfo.ReadOnly);
			AssertEquals("LD_SQLVersionInfo.ReadOnly", true, DatabaseForTest.LD_SQLVersionInfo.ReadOnly);
			AssertEquals("LD_SQLVerStringInfo.ReadOnly", true, DatabaseForTest.LD_SQLVerStringInfo.ReadOnly);
			AssertEquals("LD_LastHeartbeatInfo.ReadOnly", true, DatabaseForTest.LD_LastHeartbeatInfo.ReadOnly);
			AssertEquals("LD_OutboundEAdaptorUrlInfo.ReadOnly", true, DatabaseForTest.LD_OutboundEAdaptorUrlInfo.ReadOnly);
			AssertEquals("LD_NextRunTimeUtcUPG.ReadOnly", true, DatabaseForTest.LD_NextRunTimeUtcUPGInfo.ReadOnly);
			AssertEquals("LD_NextRunTimeUtcMUG.ReadOnly", true, DatabaseForTest.LD_NextRunTimeUtcMUGInfo.ReadOnly);
			AssertEquals("LD_ScheduleStateUPG.ReadOnly", true, DatabaseForTest.LD_ScheduleStateUPGInfo.ReadOnly);
			AssertEquals("LD_ScheduleStateMUG.ReadOnly", true, DatabaseForTest.LD_ScheduleStateMUGInfo.ReadOnly);
			AssertEquals("LD_TokenAuthenticationEnabled.ReadOnly", true, DatabaseForTest.LD_TokenAuthenticationEnabledInfo.ReadOnly);
		}

		public void TestLD_SQLVerStringForDisplay()
		{
			DatabaseForTest.LD_SQLVerString = "x\ny";
			AssertEquals("LD_SQLVerStringForDisplay", "x\r\ny", DatabaseForTest.LD_SQLVerStringForDisplay);
		}

		public void TestLD_OC_ContractInstallerOrInternalTechContact()
		{
			LicenceDatabase licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			OrgContact orgContact = Factory.NewWithValidTestData<OrgContact>();

			licenceDatabase.LD_OC_ContractInstallerOrInternalTechContact = orgContact.PK;
			AssertEquals(licenceDatabase.LD_OC_ContractInstallerOrInternalTechContact, orgContact.PK);
		}

		#region Enterprise Code / Id

		public void TestEnterpriseCodeId()
		{
			LicenceEnterprise le1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			le1.LE_EnterpriseCode = "ABC";
			LicenceEnterprise le2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			le2.LE_EnterpriseCode = "CBA";
			LicenceDatabase ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
			ld1.LD_LE = le1.PK;
			LicenceDatabase ld2 = Factory.NewWithValidTestData<LicenceDatabase>();
			ld2.LD_LE = le2.PK;

			Factory.Save();
			AssertEquals("LicenceDatabaseOne.EnterpriseCode", le1.LE_EnterpriseCode, ld1.EnterpriseCode);
			AssertEquals("LicenceDatabaseTwo.EnterpriseCode", le2.LE_EnterpriseCode, ld2.EnterpriseCode);
			AssertEquals("LicenceDatabaseOne.EnterpriseID", le1.LE_EnterpriseID, ld1.EnterpriseID);
			AssertEquals("LicenceDatabaseTwo.EnterpriseID", le2.LE_EnterpriseID, ld2.EnterpriseID);
		}

		#endregion

		#endregion

		#region Read Only Security

		public void TestReadOnlySecurity()
		{
			bool oldDatabaseDetailsSecurityCheckpoint = EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed;
			bool oldConnectionDetailsCheckPointValue = EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed;

			EDIOrgHeader organization = HeaderForTest;
			organization.CreateAndLoadLicenceForOrg();
			organization.GenerateNewLicenceCode();
			LicenceDatabase database = organization.LicCompany.LicDatabases.AddNew();

			try
			{
				string[] propertyNamesToExcept = new string[]
				{
					LicenceDatabaseSchema.Constants.LD_LicenceExpiry,
					LicenceDatabaseSchema.Constants.LD_ManualLicenceExpiry,
					LicenceDatabaseSchema.Constants.LD_DatabaseFilePathDetail,
					LicenceDatabaseSchema.Constants.LD_NoOfActivePrintQueues,
					LicenceDatabaseSchema.Constants.LD_LastHeartbeat,
					LicenceDatabaseSchema.Constants.LD_SQLEdition,
					LicenceDatabaseSchema.Constants.LD_SQLVersion,
					LicenceDatabaseSchema.Constants.LD_SQLVerString,
					LicenceDatabaseSchema.Constants.LD_OSName,
					LicenceDatabaseSchema.Constants.LD_OSVersion,
					LicenceDatabaseSchema.Constants.LD_SystemManufacturer,
					LicenceDatabaseSchema.Constants.LD_BIOSDate,
					LicenceDatabaseSchema.Constants.LD_TotalPhysicalMemoryMB,
					LicenceDatabaseSchema.Constants.LD_NoOfProcessorCores,
					LicenceDatabaseSchema.Constants.LD_ProcessorType,
					LicenceDatabaseSchema.Constants.LD_ProcessorSpeedMHz,
					LicenceDatabaseSchema.Constants.LD_VirtualMachineDetected,
					LicenceDatabaseSchema.Constants.LD_StaffFirstReportUtc,
					LicenceDatabaseSchema.Constants.LD_OH_WebAccessOrg,
					LicenceDatabaseSchema.Constants.LD_OutboundEAdaptorUrl,
					LicenceDatabaseSchema.Constants.LD_NextRunTimeUtcUPG,
					LicenceDatabaseSchema.Constants.LD_NextRunTimeUtcMUG,
					LicenceDatabaseSchema.Constants.LD_ScheduleStateUPG,
					LicenceDatabaseSchema.Constants.LD_ScheduleStateMUG,
					LicenceDatabaseSchema.Constants.LD_TokenAuthenticationEnabled,
					LicenceDatabaseSchema.Constants.LD_FeatureControlRuleLastSyncUtc,
					LicenceDatabaseSchema.Constants.LD_FeatureControlRuleLastSyncContent,
				};

				EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed = true;
				AssertPropertyInfosReadOnly(database, false, propertyNamesToExcept);

				Array.Resize(ref propertyNamesToExcept, propertyNamesToExcept.Length + 1);
				propertyNamesToExcept[propertyNamesToExcept.Length - 1] = LicenceDatabase.Schema.ConnectionDetails;

				EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed = false;
				AssertPropertyInfosReadOnly(database, false, propertyNamesToExcept);

				propertyNamesToExcept = Array.Empty<string>();
				EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed = false;
				AssertPropertyInfosReadOnly(database, true, propertyNamesToExcept);

				// Reset the database because ConnectionDetails is now readonly for that object instance.
				database = organization.LicCompany.LicDatabases.AddNew();

				propertyNamesToExcept = new string[] { LicenceDatabase.Schema.ConnectionDetails };
				EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed = false;
				EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed = true;
				AssertPropertyInfosReadOnly(database, true, propertyNamesToExcept);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed = oldDatabaseDetailsSecurityCheckpoint;
				EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed = oldConnectionDetailsCheckPointValue;
			}
		}

		#endregion

		#region Legacy Database Types

		public void TestLegacyDatabaseDeletesLicenceModules()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();
			var testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			var database = testHeader.LicCompany.LicDatabases.AddNew();
			database.LD_Product = ProductTypes.Codes.Enterprise;
			Factory.Save();
			var licHeader = testHeader.LicCompany.GetHeader(database);
			Assert("Modules Exist for this licence", licHeader.Modules.Count > 0);

			database.LD_Product = "AAA";
			Factory.Save();
			AssertEquals("No Modules exist for this licence", 0, licHeader.Modules.Count);
		}

		//TODO uncomment when form in gui project
		//[GuiTest, ExpectNoExceptions]
		//public void TestCalculateEditionDuringSaveOfJustCreatedLegacyDatabase()
		//{
		//	EDIDataRegistry.CreateProductsAndModulesForTest();
		//	EDIOrgHeader org = HeaderForTest;
		//	org.CreateAndLoadLicenceForOrg();
		//	LicenceDatabase database = org.LicCompany.LicDatabases.AddNew();
		//	database.LD_Product = ProductTypes.Codes.Enterprise;
		//	AssertEquals("Precondition: Generated licence is set to ENT", ProductTypes.Codes.Enterprise,
		//		database.LD_Product);
		//	Assert("Precondition: Generated licence has modules", database.LicHeader.Modules.Count > 0);
		//	Factory.Save();

		//	using (EDIOrganisationForm form = new EDIOrganisationForm(org))
		//	{
		//		form.Show();
		//		form.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Edit;
		//		form.OrganisationsTabControl.SelectedIndex = 15;
		//		LicenceKeyBuilderTabPage licenceTabPage = (LicenceKeyBuilderTabPage)form.OrganisationsTabControl.TabPages[15];
		//		ModuleButtonGridForLicencing databasesGrid =
		//			(ModuleButtonGridForLicencing)FindSubControlByName(licenceTabPage, "DatabasesModuleButtonGrid");
		//		databasesGrid.SelectFirstRowIfOnlyRowInGrid();
		//		database.LD_Product = "BBB";
		//		database.LicHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OtherLegacyApplication;

		//		// CalculateEdition will be hit during the save
		//		Factory.Save();
		//		AssertEquals("Post-condition: licence is set to non-ENT", "BBB", database.LD_Product);
		//		AssertEquals("Post-condition: Generated licence has no modules", 0, database.LicHeader.Modules.Count);
		//	}
		//}

		/*Control FindSubControlByName(Control parentControl, string controlName)
		{
			Control result = null;

			if (parentControl.Name == controlName)
			{
				result = parentControl;
			}
			else if (parentControl.Controls.Count > 0)
			{
				foreach (Control subControl in parentControl.Controls)
				{
					result = FindSubControlByName(subControl, controlName);
					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}*/

		#endregion

		public void TestHasMultipleEnterprises()
		{
			HeaderForTest.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = HeaderForTest.LicCompany.LicDatabases.AddNew();

			AssertEquals(false, database.HasMultipleEnterprises);
			LicenceEnterprise enterprise1 = HeaderForTest.LicCompany.LicEnterprise;
			AssertNotNull(enterprise1);

			LicenceEnterprise enterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			database.LD_LE = enterprise2.PK;
			AssertEquals(true, database.HasMultipleEnterprises);
			database.LD_LE = HeaderForTest.LicCompany.LicEnterprise.PK;
			AssertEquals(false, database.HasMultipleEnterprises);

			LicenceHeader header2 = Factory.New<LicenceHeader>();
			LicenceCompany company2 = Factory.New<LicenceCompany>();
			header2.LA_LC = company2.PK;
			company2.LC_LE = enterprise2.PK;
			database.LicHeadersForAllCompanies.Add(header2);
			AssertEquals(true, database.HasMultipleEnterprises);
			company2.LC_LE = enterprise1.PK;
			AssertEquals(false, database.HasMultipleEnterprises);
		}

		[TestDate(2016, 1, 19)]
		[TestUtcOffset(11, 0, 0)]
		public void TestSendUpdateForSystemExpiry()
		{
			ZDateTime expiryTime = ZDateTime.Today.AddDays(3);
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_Code = "ABCXYZ";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			database.LD_PublicEmailAddressForUpdate = "xerxes@edi.com.au";
			database.LD_HostServerSID = ZGuid.NewZGuid();
			database.LD_LicenceExpiry = expiryTime;
			database.LD_DatabaseFilePathDetail = "Database information";
			database.LD_HostDBName = "DBName";
			database.LD_HostDBInstance = "INSTANCE";
			database.LD_HostServerName = "SERVERNAME";
			database.LD_InternalPop3EmailAddress = "XHC";
			database.LD_InternalPop3Port = 110;
			database.LD_InternalPop3UserName = "xerxesb";
			database.LD_InternalSmtpEmailAddress = "XHC";
			database.LD_InternalSmtpPort = 25;
			database.LD_DatabaseNumber = 98765;
			database.CustomExpiredNote.Text = "expired";
			database.CustomExpiryMonthNote.Text = "expiry within a month";

			var sender = new SystemUpdatePacketSenderForTest();
			database.SystemUpdatePacketSender = sender;
			database.SendUpdateForSystemExpiry(testHeader.LicCompany.LC_CompanyCode);
			AssertEquals("LD_LicenceExpiry unchanged", expiryTime, database.LD_LicenceExpiry);
			AssertEquals(1, sender.SendCalls);
			var sentKey = SystemRegistrationKey.NewFromEncryptedXmlKey(sender.Packet.EncryptedSysRegKey);
			AssertEquals("expired", sentKey.ExpiredMessage);
			AssertEquals("", sentKey.ExpiryWeekMessage);
			AssertEquals("expiry within a month", sentKey.ExpiryMonthMessage);
			AssertEquals(Base27Encoding.Encode(98765), sentKey.SystemId);
			AssertEquals(11.0d, sentKey.CurrentBillingTimeZoneUtcOffset);
			AssertEquals(10.0d, sentKey.NextBillingTimeZoneUtcOffset);
			AssertEquals(new DateTime(2016, 4, 2, 16, 0, 0), sentKey.NextUtcOffsetEffectiveTimeUtc);
		}

		public void TestHostedLocation()
		{
			var list = new Enterprise.Registry.Business.CodeDescriptionBoolCollection();
			list.Add("NCW", (NoResString)"Not Hosted With CargoWise", false);
			list.Add("MEL", (NoResString)"Melbourne", true);
			list.Add("BNE", (NoResString)"Brisbane", true);
			list.Add("NY", (NoResString)"New York", true);
			EDIDataRegistry.Instance.DatabaseHostedLocations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			AssertEquals("Not Hosted With CargoWise", DatabaseForTest.HostedLocationDesciption);

			DatabaseForTest.LD_HostedLocation = "MEL";
			AssertEquals("Melbourne", DatabaseForTest.HostedLocationDesciption);

			DatabaseForTest.LD_HostedLocation = "NY";
			AssertEquals("New York", DatabaseForTest.HostedLocationDesciption);
		}

		public void TestVersionCanReceiveSystemMessageCSR()
		{
			LicenceDatabase db = Factory.New<LicenceDatabase>();
			AssertEquals("no version", TriState.NotDetermined, db.VersionCanReceiveSystemMessageCSR);

			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_ExeVersionDate = new ZDateTime(2005, 10, 01);
			build.VersionNumber = new VersionNumber(1, 4, LicenceDatabase.FirstReleaseSystemMessageCSR - 1, 0);
			db.LD_HL_CurrentRunningVersion = build.PK;
			AssertEquals("version too old", TriState.False, db.VersionCanReceiveSystemMessageCSR);

			build.VersionNumber = new VersionNumber(1, 4, LicenceDatabase.FirstReleaseSystemMessageCSR, 0);
			AssertEquals("version ok", TriState.True, db.VersionCanReceiveSystemMessageCSR);
		}

		public void TestVersionCanReceiveSystemMessageRDU()
		{
			LicenceDatabase db = Factory.New<LicenceDatabase>();
			AssertEquals("no version", TriState.NotDetermined, db.VersionCanReceiveSystemMessageRDU);

			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_ExeVersionDate = new ZDateTime(2005, 10, 01);
			build.VersionNumber = new VersionNumber(1, 4, LicenceDatabase.FirstReleaseSystemMessageRDU - 1, 0);
			db.LD_HL_CurrentRunningVersion = build.PK;
			AssertEquals("version too old", TriState.False, db.VersionCanReceiveSystemMessageRDU);

			build.VersionNumber = new VersionNumber(1, 4, LicenceDatabase.FirstReleaseSystemMessageRDU, 0);
			AssertEquals("version ok", TriState.True, db.VersionCanReceiveSystemMessageRDU);
		}

		public void TestBuildIsSupported()
		{
			AssertEquals(false, LicenceDatabase.BuildIsSupported(new VersionNumber(1, 1, 2, 2), ""));
			AssertEquals(false, LicenceDatabase.BuildIsSupported(new VersionNumber(1, 1, 2, 2), "foo"));

			AssertEquals(false, LicenceDatabase.BuildIsSupported(new VersionNumber(1, 1, 2, 2), "1.1.2.3"));
			AssertEquals(false, LicenceDatabase.BuildIsSupported(new VersionNumber(1, 1, 2, 2), "1.1.3.0"));
			AssertEquals(false, LicenceDatabase.BuildIsSupported(new VersionNumber(1, 1, 2, 2), "2.1.1.0"));
			AssertEquals(false, LicenceDatabase.BuildIsSupported(new VersionNumber(1, 1, 2, 2), "1.1.2.3, 1.1.3.0, 2.1.1.0"));

			AssertEquals(true, LicenceDatabase.BuildIsSupported(new VersionNumber(1, 1, 2, 2), "1.1.1.0"));
			AssertEquals(true, LicenceDatabase.BuildIsSupported(new VersionNumber(1, 1, 2, 2), "1.1.2.0"));
			AssertEquals(true, LicenceDatabase.BuildIsSupported(new VersionNumber(1, 1, 2, 2), "1.1.2.2"));

			AssertEquals(true, LicenceDatabase.BuildIsSupported(new VersionNumber(1, 1, 2, 2), "1.1.1.0, 1.1.2.0, 1.1.2.2"));

			AssertEquals(true, LicenceDatabase.BuildIsSupported(new VersionNumber(1, 1, 2, 2), "1.1.2.3, 1.1.2.0"));
			AssertEquals(true, LicenceDatabase.BuildIsSupported(new VersionNumber(1, 1, 2, 2), "foo, 1.1.2.3, 1.1.2.0"));

			AssertEquals(true, LicenceDatabase.BuildIsSupported(new VersionNumber(2, 0, 8, 2), "1.4.4547.206"));

			AssertEquals(false, LicenceDatabase.BuildIsSupported(new VersionNumber(16, 7, 13, 0), "16.6.8.612,16.7.14.0, 1.4.6045.0"));
			AssertEquals(false, LicenceDatabase.BuildIsSupported(new VersionNumber(16, 6, 9, 0), "16.6.8.612,16.7.14.0, 1.4.6045.0"));
			AssertEquals(false, LicenceDatabase.BuildIsSupported(new VersionNumber(16, 6, 9, 612), "16.6.8.612,16.7.14.0, 1.4.6045.0"));
			AssertEquals(true, LicenceDatabase.BuildIsSupported(new VersionNumber(16, 7, 14, 0), "16.6.8.612,16.7.14.0, 1.4.6045.0"));
			AssertEquals(false, LicenceDatabase.BuildIsSupported(new VersionNumber(16, 6, 8, 611), "16.6.8.612,16.7.14.0, 1.4.6045.0"));
			AssertEquals(true, LicenceDatabase.BuildIsSupported(new VersionNumber(16, 6, 8, 612), "16.6.8.612,16.7.14.0, 1.4.6045.0"));

			AssertEquals(true, LicenceDatabase.BuildIsSupported(new VersionNumber(16, 1, 1, 612), "1.4.5000.0"));
			AssertEquals(false, LicenceDatabase.BuildIsSupported(new VersionNumber(16, 1, 1, 612), "1.4.5001.0"));

			AssertEquals(true, LicenceDatabase.BuildIsSupported(new VersionNumber(16, 1, 1, 612), "2.0.215.0"));
		}

		public void TestCanSupportBiDirectionIncidentMessage()
		{
			var build = Factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(1, 1, 2, 2);
			AssertEquals(TriState.False, LicenceDatabase.CanSupportBiDirectionIncidentMessage(build));

			EDIDataRegistry.Instance.ERequestsReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.3");
			AssertEquals(TriState.False, LicenceDatabase.CanSupportBiDirectionIncidentMessage(build));

			EDIDataRegistry.Instance.ERequestsReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.2");
			AssertEquals(TriState.True, LicenceDatabase.CanSupportBiDirectionIncidentMessage(build));

			EDIDataRegistry.Instance.ERequestsReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.1.0");
			AssertEquals(TriState.True, LicenceDatabase.CanSupportBiDirectionIncidentMessage(build));

			EDIDataRegistry.Instance.ERequestsReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.1.1");
			AssertEquals(TriState.False, LicenceDatabase.CanSupportBiDirectionIncidentMessage(build));

			EDIDataRegistry.Instance.ERequestsReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.3, 1.1.1.1");
			AssertEquals(TriState.False, LicenceDatabase.CanSupportBiDirectionIncidentMessage(build));

			EDIDataRegistry.Instance.ERequestsReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.3, 1.1.1.1, 1.1.1.0");
			AssertEquals(TriState.True, LicenceDatabase.CanSupportBiDirectionIncidentMessage(build));

			AssertEquals(TriState.NotDetermined, LicenceDatabase.CanSupportBiDirectionIncidentMessage(null));
		}

		public void TestCanSupportCustomExpiryMessages()
		{
			var build = Factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(1, 1, 2, 2);
			AssertEquals(TriState.False, LicenceDatabase.CanSupportCustomExpiryMessages(build));

			EDIDataRegistry.Instance.CustomExpiryMessagesReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.3");
			AssertEquals(TriState.False, LicenceDatabase.CanSupportCustomExpiryMessages(build));

			EDIDataRegistry.Instance.CustomExpiryMessagesReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.2");
			AssertEquals(TriState.True, LicenceDatabase.CanSupportCustomExpiryMessages(build));

			EDIDataRegistry.Instance.CustomExpiryMessagesReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.1.0");
			AssertEquals(TriState.True, LicenceDatabase.CanSupportCustomExpiryMessages(build));

			EDIDataRegistry.Instance.CustomExpiryMessagesReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.1.1");
			AssertEquals(TriState.False, LicenceDatabase.CanSupportCustomExpiryMessages(build));

			EDIDataRegistry.Instance.CustomExpiryMessagesReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.3, 1.1.1.1");
			AssertEquals(TriState.False, LicenceDatabase.CanSupportCustomExpiryMessages(build));

			EDIDataRegistry.Instance.CustomExpiryMessagesReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.3, 1.1.1.1, 1.1.1.0");
			AssertEquals(TriState.True, LicenceDatabase.CanSupportCustomExpiryMessages(build));

			AssertEquals(TriState.NotDetermined, LicenceDatabase.CanSupportCustomExpiryMessages(null));
		}

		public void TestCanSupportSendIncidentEmailFromClient()
		{
			var build = Factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(1, 1, 2, 2);
			AssertEquals(TriState.False, LicenceDatabase.CanSupportSendIncidentEmailFromClient(build));

			EDIDataRegistry.Instance.ERequestV2ReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.3");
			AssertEquals(TriState.False, LicenceDatabase.CanSupportSendIncidentEmailFromClient(build));

			EDIDataRegistry.Instance.ERequestV2ReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.2");
			AssertEquals(TriState.True, LicenceDatabase.CanSupportSendIncidentEmailFromClient(build));

			EDIDataRegistry.Instance.ERequestV2ReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.1.0");
			AssertEquals(TriState.True, LicenceDatabase.CanSupportSendIncidentEmailFromClient(build));

			EDIDataRegistry.Instance.ERequestV2ReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.1.1");
			AssertEquals(TriState.False, LicenceDatabase.CanSupportSendIncidentEmailFromClient(build));

			EDIDataRegistry.Instance.ERequestV2ReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.3, 1.1.1.1");
			AssertEquals(TriState.False, LicenceDatabase.CanSupportSendIncidentEmailFromClient(build));

			EDIDataRegistry.Instance.ERequestV2ReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.3, 1.1.1.1, 1.1.1.0");
			AssertEquals(TriState.True, LicenceDatabase.CanSupportSendIncidentEmailFromClient(build));

			AssertEquals(TriState.NotDetermined, LicenceDatabase.CanSupportSendIncidentEmailFromClient(null));
		}

		public void TestUsageOwnerLicence()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "BBB");
			var lic3 = BillingTestHelper.CreateAnotherLicence(lic1, "BBB");
			var db = lic1.Database;

			db.LD_OH_BillingParty = lic2.Company.LC_OH;
			AssertEquals(lic2, db.UsageOwnerLicence);

			db.LD_OH_BillingParty = lic3.Company.LC_OH;
			AssertEquals(lic3, db.UsageOwnerLicence);
		}

		public void TestUsageOwnerOrFirstLicence()
		{
			var licRecent = BillingTestHelper.CreateLicence(Factory, "AAA");
			var licOldInactive = BillingTestHelper.CreateAnotherLicence(licRecent, "BBB");
			var licOldActive = BillingTestHelper.CreateAnotherLicence(licRecent, "CCC");
			var licOldActive2 = BillingTestHelper.CreateAnotherLicence(licRecent, "DDD");
			var licOther = BillingTestHelper.CreateLicence(Factory, "ZZZ");
			var db1 = licRecent.Database;

			licOldActive.LA_AgreedLiveDate = licOldActive.LA_AgreedLiveDate.AddYears(-1);
			licOldActive2.LA_AgreedLiveDate = licOldActive.LA_AgreedLiveDate;

			licOldInactive.LA_AgreedLiveDate = licOldInactive.LA_AgreedLiveDate.AddYears(-2);
			licOldInactive.LA_IsActive = false;

			Factory.Save();

			AssertEquals("default owner is oldest, active, first company code", licOldActive, db1.UsageOwnerOrFirstLicence);

			db1.LD_OH_BillingParty = licRecent.Company.LC_OH;
			Factory.Save();
			AssertEquals("default owner is BillingParty", licRecent, db1.UsageOwnerOrFirstLicence);

			db1.LD_OH_BillingParty = licOther.Company.LC_OH;
			Factory.Save();
			AssertEquals("default owner is on the DB if the BillingParty is not", licOldActive, db1.UsageOwnerOrFirstLicence);
		}

		public void TestOnSaving_DatabaseNumber()
		{
			var db1 = BillingTestHelper.CreateLicence(Factory, "AAA").Database;
			Factory.Save();

			var db2 = BillingTestHelper.CreateLicence(Factory, "BBB").Database;
			Factory.Save();

			AssertEquals(100, db1.LD_DatabaseNumber);
			AssertEquals(101, db2.LD_DatabaseNumber);
		}

		public void TestOnSaving_TechnicalContactNotificationGroup()
		{
			var db1 = BillingTestHelper.CreateLicence(Factory, "AAA").Database;
			Factory.Save();

			AssertEquals(ZGuid.Empty, db1.LD_OC_ContractInstallerOrInternalTechContact);
			var contact = db1.LicEnterprise.Organisation.Contacts.First() as EDIOrgContact;
			AssertEquals(false, contact.IsInformationServicesTechnicalAdministrator);

			db1.LD_OC_ContractInstallerOrInternalTechContact = contact.PK;
			AssertEquals(false, contact.IsInformationServicesTechnicalAdministrator);
			Factory.Save();
			AssertEquals(true, contact.IsInformationServicesTechnicalAdministrator);

			contact.IsInformationServicesTechnicalAdministrator = false;
			db1.LD_OC_ContractInstallerOrInternalTechContact = ZGuid.Empty;
			Factory.Save();

			TestConnection.ExecuteNonQuery($"UPDATE dbo.LicenceDatabase SET LD_OSName = '@1234!!!!', LD_SystemLastEditTimeUtc = GETUTCDATE(), LD_SystemLastEditUser = 'E' WHERE LD_PK = '{db1.PK}';");

			db1.LD_OC_ContractInstallerOrInternalTechContact = contact.PK;
			AssertEquals(false, contact.IsInformationServicesTechnicalAdministrator);
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			AssertEquals(false, contact.IsInformationServicesTechnicalAdministrator);
		}

		public void TestCopyPersistentValuesFrom()
		{
			var db1 = Factory.New<LicenceDatabase>();
			var db2 = Factory.New<LicenceDatabase>();
			db1.LD_DatabaseNumber = 200;
			db2.LD_DatabaseNumber = 300;
			db2.CopyPersistentValuesFrom(db1);
			AssertEquals(300, db2.LD_DatabaseNumber);
		}

		public void TestSupportsUserCreatedCompanies()
		{
			var build = Factory.New<ReleaseBuild>();
			build.HL_MajorVersion = 5;
			build.HL_MinorVersion = 6;
			build.HL_Release = 7;
			build.HL_Patch = 8;

			AssertEquals(TriState.False, LicenceDatabase.SupportsUserCreatedCompanies(build));

			EDIDataRegistry.Instance.UserCreatedCompanyReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "5.6.7.9");
			AssertEquals(TriState.False, LicenceDatabase.SupportsUserCreatedCompanies(build));

			EDIDataRegistry.Instance.UserCreatedCompanyReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "5.6.7.8");
			AssertEquals(TriState.True, LicenceDatabase.SupportsUserCreatedCompanies(build));
		}

		public void TestPriceHeaderLinkForDate()
		{
			var db = Factory.New<LicenceDatabase>();
			var date1 = new ZDateTime(2015, 1, 1);
			var date2 = new ZDateTime(2015, 2, 1);
			var date3 = new ZDateTime(2015, 3, 1);
			var link1 = db.PriceHeaderLinks.AddNew();
			var link2 = db.PriceHeaderLinks.AddNew();
			var link3 = db.PriceHeaderLinks.AddNew();

			link1.PHL_ValidFrom = date1;
			link2.PHL_ValidFrom = date2;
			link3.PHL_ValidFrom = date3;
			link3.PHL_ValidTo = date3.AddMonths(12);

			AssertNull(db.PriceHeaderLinkForDate(date1.AddDays(-1)));
			AssertEquals(link1, db.PriceHeaderLinkForDate(date1));
			AssertEquals(link2, db.PriceHeaderLinkForDate(date2));
			AssertEquals(link3, db.PriceHeaderLinkForDate(date3));
			AssertEquals(link3, db.PriceHeaderLinkForDate(date3.AddMonths(1)));
			AssertNull(db.PriceHeaderLinkForDate(date3.AddMonths(13)));
		}

		[TestDate(2016, 6, 20)]
		public void TestLD_LicenceType()
		{
			var prodLic1 = BillingTestHelper.CreateLicence(Factory, "PR1");
			var prodLic2a = BillingTestHelper.CreateLicence(Factory, "PR2");
			var prodLic2b = BillingTestHelper.CreateAnotherDatabase(prodLic2a, "PR3");
			Factory.Save();

			var testLic1 = BillingTestHelper.CreateAnotherDatabase(prodLic1, "TS1");
			var testLic2 = BillingTestHelper.CreateAnotherDatabase(prodLic2a, "TS2");
			testLic1.Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			testLic2.Database.LD_LicenceType = DatabaseTypes.Codes.Test;

			AssertEquals("LD_LD_ParentDatabase is set if only one production database", prodLic1.LA_LD, testLic1.Database.LD_LD_ParentDatabase);
			AssertEquals("LD_LD_ParentDatabase is NOT set if multiple production databases", ZGuid.Empty, testLic2.Database.LD_LD_ParentDatabase);

			AssertEquals(ZDate.Empty, testLic1.Database.LD_ManualLicenceExpiry);
			testLic1.Database.LD_LicenceType = DatabaseTypes.Codes.WisecloudTrial;
			AssertEquals(new ZDateTime(2016, 6, 20).AddDays(60), testLic1.Database.LD_ManualLicenceExpiry);
		}

		[TestDate(2016, 4, 5)]
		public void TestBillingModel()
		{
			var prodLic1 = BillingTestHelper.CreateLicence(Factory, "PR1");
			var tstLic1 = BillingTestHelper.CreateAnotherDatabase(prodLic1, "TS1");
			var tstLic2 = BillingTestHelper.CreateAnotherDatabase(prodLic1, "TS2");
			tstLic1.Database.LD_LD_ParentDatabase = prodLic1.LA_LD;
			tstLic1.Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			tstLic2.Database.LD_LicenceType = DatabaseTypes.Codes.Test;

			var priceHeader = prodLic1.Company.PriceHeaders.AddNew();

			var priceLink = prodLic1.Database.PriceHeaderLinks.AddNew();
			priceLink.PHL_L6 = priceHeader.PK;
			priceLink.PHL_ValidFrom = new ZDateTime(2016, 5, 1);
			priceLink.PHL_RX_NKCurrency = "AUD";

			Factory.Save();

			AssertEquals(BillingConstants.BillingModel.ODM, prodLic1.Database.BillingModel);
			AssertEquals(BillingConstants.BillingModel.ODM, tstLic1.Database.BillingModel);

			TestDateAttribute.Date = new DateTime(2016, 5, 1);
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var db1 = factory2.Load<LicenceDatabase>(prodLic1.LA_LD);
			var db2 = factory2.Load<LicenceDatabase>(tstLic1.LA_LD);

			AssertEquals(BillingConstants.BillingModel.STL, db1.BillingModel);
			AssertEquals(BillingConstants.BillingModel.STL, db2.BillingModel);
		}

		public void TestLicenceCodeForSystemMessage()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "SYD", "PRD", true);
			var db1 = lic.Database;
			AssertEquals("ENTSYDPRD", db1.LicenceCodeForSystemMessage);

			lic.ClientCompany.LCC_Code = "NEW";
			AssertEquals("ENTNEWPRD", db1.LicenceCodeForSystemMessage);

			db1.CompanyCodeFromLastHeartbeat = "ZZZ";
			AssertEquals("ENTZZZPRD", db1.LicenceCodeForSystemMessage);
		}

		public void TestDefaultPriceCurrency()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CM1", "DB1");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "EN1", "CM2", "DB1");

			var db1 = lic1.Database;
			var db2 = lic2.Database;
			AssertEquals(db1.PK, db2.PK);
			var headers = db1.ActiveLicHeadersForAllCompanies.OfType<LicenceHeader>().ToArray();
			AssertEquals(2, headers.Length);
			var header1 = headers[0];
			var header2 = headers[1];
			db1.LD_IsBilledPerCompany = false;
			header1.LA_RX_NKPriceCurrency = "";
			header2.LA_RX_NKPriceCurrency = "";

			Factory.Save();
			AssertEquals(false, db1.LD_IsBilledPerCompany);
			AssertEquals("", header1.LA_RX_NKPriceCurrency);
			AssertEquals("", header2.LA_RX_NKPriceCurrency);

			db1.LD_IsBilledPerCompany = true;
			header1.LA_RX_NKPriceCurrency = "GBP";
			Factory.Save();

			AssertEquals(true, db1.LD_IsBilledPerCompany);
			AssertEquals("GBP", header1.LA_RX_NKPriceCurrency);
			AssertEquals("GBP", header2.LA_RX_NKPriceCurrency);
		}

		public void TestLD_BillableDescription()
		{
			var db = Factory.New<LicenceDatabase>();

			db.LD_BillableDescription = DatabaseBillableFlagList.Descriptions.YesCustomer;
			AssertEquals(db.LD_Billable, DatabaseBillableFlagList.Codes.YesCustomer);
			db.LD_BillableDescription = DatabaseBillableFlagList.Descriptions.YesPartner;
			AssertEquals(db.LD_Billable, DatabaseBillableFlagList.Codes.YesPartner);
			db.LD_BillableDescription = DatabaseBillableFlagList.Descriptions.No;
			AssertEquals(db.LD_Billable, DatabaseBillableFlagList.Codes.No);
			db.LD_BillableDescription = DatabaseBillableFlagList.Descriptions.Null;
			AssertEquals(db.LD_Billable, DatabaseBillableFlagList.Codes.Null);
			db.LD_BillableDescription = "ABC1";
			AssertEquals(db.LD_Billable, DatabaseBillableFlagList.Codes.InvalidCode);

			db.LD_Billable = DatabaseBillableFlagList.Codes.YesCustomer;
			AssertEquals(db.LD_BillableDescription, DatabaseBillableFlagList.Descriptions.YesCustomer);
			db.LD_Billable = DatabaseBillableFlagList.Codes.YesPartner;
			AssertEquals(db.LD_BillableDescription, DatabaseBillableFlagList.Descriptions.YesPartner);
			db.LD_Billable = DatabaseBillableFlagList.Codes.No;
			AssertEquals(db.LD_BillableDescription, DatabaseBillableFlagList.Descriptions.No);
			db.LD_Billable = DatabaseBillableFlagList.Codes.Null;
			AssertEquals(db.LD_BillableDescription, DatabaseBillableFlagList.Descriptions.Null);
			db.LD_Billable = DatabaseBillableFlagList.Codes.InvalidCode;
			AssertEquals(db.LD_BillableDescription, "");
		}

		public void TestIsMultiTenantDatabase()
		{
			var db = Factory.New<LicenceDatabase>();
			db.LD_Product = "#01";
			AssertEquals(false, db.IsMultiTenantDatabase);

			db.LD_Product = MultiTenantDatabaseProductTypeList.Codes.CSP;
			AssertEquals(true, db.IsMultiTenantDatabase);
		}

		public void TestUpdateMessagingConfigOnAllMultiTenantDatabases()
		{
			var mtdbCode = MultiTenantDatabaseProductTypeList.Codes.CSP;

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CM1", "DB1");
			lic1.Database.LD_Product = mtdbCode;
			var lic2 = BillingTestHelper.CreateLicence(Factory, "EN2", "CM2", "DB2");
			lic2.Database.LD_Product = mtdbCode;
			var lic3 = BillingTestHelper.CreateLicence(Factory, "EN3", "CM1", "DB3");
			lic3.Database.LD_Product = "#01";
			lic3.Database.GetOrCreateTrustedSystem();
			var lic4 = BillingTestHelper.CreateLicence(Factory, "EN4", "CM4", "DB4");
			lic4.Database.LD_Product = "#02";
			Factory.Save();

			AssertNull(lic1.Database.TrustedSystem);
			AssertNull(lic2.Database.TrustedSystem);
			AssertEquals("#01", lic3.Database.TrustedSystem.ETS_Product);
			AssertNull(lic4.Database.TrustedSystem);

			lic1.Database.GetOrCreateTrustedSystem().ETS_SystemEndpointUrl = "http://123";
			AssertEquals(true, lic1.Database.IsMultiTenantDatabase);
			AssertEquals(true, lic2.Database.IsMultiTenantDatabase);
			lic1.Database.UpdateMessagingConfigOnAllMultiTenantDatabases();
			Factory.Save();
			AssertEquals("http://123", lic1.Database.TrustedSystem.ETS_SystemEndpointUrl);
			AssertEquals(mtdbCode, lic1.Database.TrustedSystem.ETS_Product);
			AssertEquals(lic2.Database.TrustedSystem, lic1.Database.TrustedSystem);
			AssertEquals("http://123", lic2.Database.TrustedSystem.ETS_SystemEndpointUrl);
			AssertEquals("#01", lic3.Database.TrustedSystem.ETS_Product);
			AssertNull(lic4.Database.TrustedSystem);

			lic2.Database.LD_ETS_TrustedSystem = ZGuid.Empty;
			Factory.Save();
			AssertNull(lic2.Database.TrustedSystem);
			AssertEquals(null, lic2.Database.TrustedSystem);
			lic1.Database.TrustedSystem.ETS_SystemEndpointUrl = "http://456";
			lic1.Database.UpdateMessagingConfigOnAllMultiTenantDatabases();
			Factory.Save();
			AssertNotEquals(null, lic2.Database.TrustedSystem);
			AssertEquals(lic2.Database.TrustedSystem, lic1.Database.TrustedSystem);
			AssertEquals("http://456", lic2.Database.TrustedSystem.ETS_SystemEndpointUrl);
			AssertEquals("#01", lic3.Database.TrustedSystem.ETS_Product);
			AssertNull(lic4.Database.TrustedSystem);
		}

		public void TestDisableAutoLogin()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LD_AllowAutoLogin = true;
			AssertEquals(true, db1.LD_AllowAutoLogin);
			Factory.Save();
			AssertEquals(true, db1.LD_AllowAutoLogin);

			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			db2.LicEnterprise.LE_IsInternal = true;
			db2.LD_AllowAutoLogin = true;
			AssertEquals(true, db2.LD_AllowAutoLogin);
			Factory.Save();
			AssertEquals(false, db2.LD_AllowAutoLogin);
		}

		public void TestEDIWebAccessOrg()
		{
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			AssertEquals(ZGuid.Empty, licenceDatabase.LD_OH_WebAccessOrg);
			AssertNull(licenceDatabase.EDIWebAccessOrg);

			var orgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgHeader.OH_Code = "TESTCODE";
			licenceDatabase.LD_OH_WebAccessOrg = orgHeader.PK;
			AssertNotNull(licenceDatabase.EDIWebAccessOrg);
			AssertEquals("TESTCODE", licenceDatabase.EDIWebAccessOrg.OH_Code);
		}

		public void TestEDIWebAccessOrg_InternalEnterpriseDefault()
		{
			var orgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgHeader.OH_Code = "TESTCODE";

			var defaultMasterOrgs = new CodeDescriptionPairList();
			defaultMasterOrgs.AddPair("WTL", "TESTCODE");
			EDIDataRegistry.Instance.InternalEnterpriseMasterOrgs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultMasterOrgs);

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_OH = orgHeader.PK;
			enterprise.LE_EnterpriseCode = "WTL";
			enterprise.LE_IsInternal = true;

			Factory.Save();

			var licenceDatabase = Factory.New<LicenceDatabase>();
			licenceDatabase.LD_LE = enterprise.PK;
			AssertEquals(ZGuid.Empty, licenceDatabase.LD_OH_WebAccessOrg);
			AssertNull(licenceDatabase.EDIWebAccessOrg);
			Factory.Save();

			licenceDatabase.Reload();
			AssertNotNull(licenceDatabase.EDIWebAccessOrg);
			AssertEquals("TESTCODE", licenceDatabase.EDIWebAccessOrg.OH_Code);

			licenceDatabase.LD_OH_WebAccessOrg = ZGuid.Empty;
			Factory.Save();
			licenceDatabase.Reload();
			AssertNotNull(licenceDatabase.EDIWebAccessOrg);
			AssertEquals("TESTCODE", licenceDatabase.EDIWebAccessOrg.OH_Code);
		}

		public void TestSystemID()
		{
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			AssertEquals(ZGuid.Empty, licenceDatabase.LD_ETS_TrustedSystem);
			AssertNull(licenceDatabase.TrustedSystem);
			AssertEquals(ZString.Empty, licenceDatabase.SystemID);

			licenceDatabase.GetOrCreateTrustedSystem().ETS_SystemID = "SYS#001";
			AssertEquals(false, licenceDatabase.LD_ETS_TrustedSystem.IsEmpty);
			AssertNotNull(licenceDatabase.TrustedSystem);
			AssertEquals("SYS#001", licenceDatabase.SystemID);
		}

		public void TestDeactivateAllUserAccounts()
		{
			var licenceDatabase1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var licenceDatabase2 = Factory.NewWithValidTestData<LicenceDatabase>();

			var orgContact1 = Factory.NewWithValidTestData<OrgContact>();
			var orgContact2 = Factory.NewWithValidTestData<OrgContact>();
			var orgContact3 = Factory.NewWithValidTestData<OrgContact>();
			var orgContact4 = Factory.NewWithValidTestData<OrgContact>();

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_UserID = "AAA";
			userAccount1.EUA_LD = licenceDatabase1.PK;
			userAccount1.EUA_OC_WebAccessContact = orgContact1.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_UserID = "BBB";
			userAccount2.EUA_LD = licenceDatabase2.PK;
			userAccount2.EUA_OC_WebAccessContact = orgContact2.PK;

			var userAccount3 = Factory.New<EdiCustomerUserAccount>();
			userAccount3.EUA_UserID = "CCC";
			userAccount3.EUA_LD = licenceDatabase1.PK;
			userAccount3.EUA_OC_WebAccessContact = orgContact3.PK;

			var userAccount4 = Factory.New<EdiCustomerUserAccount>();
			userAccount4.EUA_UserID = "DDD";
			userAccount4.EUA_LD = licenceDatabase2.PK;
			userAccount4.EUA_OC_WebAccessContact = orgContact3.PK;

			var userAccount5 = Factory.New<EdiCustomerUserAccount>();
			userAccount5.EUA_UserID = "EEE";
			userAccount5.EUA_LD = licenceDatabase1.PK;
			userAccount5.EUA_OC_WebAccessContact = orgContact4.PK;

			var userAccount6 = Factory.New<EdiCustomerUserAccount>();
			userAccount6.EUA_UserID = "FFF";
			userAccount6.EUA_LD = licenceDatabase1.PK;
			userAccount6.EUA_OC_WebAccessContact = orgContact4.PK;

			Factory.Save();

			AssertEquals(true, licenceDatabase1.LD_IsActive);
			AssertEquals(true, licenceDatabase2.LD_IsActive);
			AssertEquals(true, userAccount1.EUA_IsActive);
			AssertEquals(true, userAccount2.EUA_IsActive);
			AssertEquals(true, userAccount3.EUA_IsActive);
			AssertEquals(true, userAccount4.EUA_IsActive);
			AssertEquals(true, userAccount5.EUA_IsActive);
			AssertEquals(true, userAccount6.EUA_IsActive);
			AssertEquals(true, orgContact1.OC_IsActive);
			AssertEquals(true, orgContact2.OC_IsActive);
			AssertEquals(true, orgContact3.OC_IsActive);
			AssertEquals(true, orgContact4.OC_IsActive);

			licenceDatabase1.LD_IsActive = false;
			Factory.Save();

			AssertEquals(false, licenceDatabase1.LD_IsActive);
			AssertEquals(true, licenceDatabase2.LD_IsActive);
			AssertEquals(false, userAccount1.EUA_IsActive);
			AssertEquals(true, userAccount2.EUA_IsActive);
			AssertEquals(false, userAccount3.EUA_IsActive);
			AssertEquals(true, userAccount4.EUA_IsActive);
			AssertEquals(false, userAccount5.EUA_IsActive);
			AssertEquals(false, userAccount6.EUA_IsActive);
			AssertEquals(false, orgContact1.OC_IsActive);
			AssertEquals(true, orgContact2.OC_IsActive);
			AssertEquals(true, orgContact3.OC_IsActive);
			AssertEquals(false, orgContact4.OC_IsActive);
		}

		public void TestLogChanges()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TESTORG1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "TESTORG2";
			var licenceDatabase1 = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase1.LD_OH_WebAccessOrg = ZGuid.Empty;
			Factory.Save();

			licenceDatabase1.LD_OH_WebAccessOrg = org1.PK;
			Factory.Save();
			var log = licenceDatabase1.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"LD_OH_WebAccessOrg changed from ,00000000-0000-0000-0000-000000000000 to {org1.OH_Code},{org1.PK}")).Single();
			AssertNotNull(log);

			licenceDatabase1.LD_OH_WebAccessOrg = org2.PK;
			Factory.Save();
			log = licenceDatabase1.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"LD_OH_WebAccessOrg changed from {org1.OH_Code},{org1.PK} to {org2.OH_Code},{org2.PK}")).Single();
			AssertNotNull(log);
		}

		[TestDate(2025, 01, 01)]
		public void TestSetLD_FCS_FeatureSetShouldUpdateFeatureSetLastEditTimeAndUser()
		{
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var featureSetA = Factory.NewWithValidTestData<FeatureControlSet>();
			var featureSetB = Factory.NewWithValidTestData<FeatureControlSet>();
			var newStaffA = Factory.NewWithValidTestData<GlbStaff>();
			var newStaffB = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			licenceDatabase.LD_FCS_FeatureSet = featureSetA.PK;
			TestDateAttribute.AddDays(1);

			using (Env.SetTemporaryUserContext(new UserContext(newStaffA.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				Factory.Save();
			}

			var newFactory = new BusinessObjectFactory();
			var featureSetAReloaded = newFactory.Load<FeatureControlSet>(featureSetA.PK);

			AssertEquals(licenceDatabase.LD_SystemLastEditTimeUtc, featureSetAReloaded.FCS_SystemLastEditTimeUtc);
			AssertEquals(licenceDatabase.LD_SystemLastEditUser, featureSetAReloaded.FCS_SystemLastEditUser);
			AssertEquals(newStaffA.GS_Code, featureSetAReloaded.FCS_SystemLastEditUser);

			licenceDatabase.LD_FCS_FeatureSet = featureSetB.PK;
			TestDateAttribute.AddDays(1);

			using (Env.SetTemporaryUserContext(new UserContext(newStaffB.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				Factory.Save();
			}

			newFactory = new BusinessObjectFactory();
			featureSetAReloaded = newFactory.Load<FeatureControlSet>(featureSetA.PK);
			var featureSetBReloaded = newFactory.Load<FeatureControlSet>(featureSetA.PK);

			AssertEquals("Both feature set last edit times should be updated", licenceDatabase.LD_SystemLastEditTimeUtc, featureSetAReloaded.FCS_SystemLastEditTimeUtc);
			AssertEquals("Both feature set last edit times should be updated", new ZDateTime(2025, 01, 03), featureSetAReloaded.FCS_SystemLastEditTimeUtc);
			AssertEquals("Both feature set last edit times should be updated", licenceDatabase.LD_SystemLastEditUser, featureSetAReloaded.FCS_SystemLastEditUser);
			AssertEquals("Both feature set last edit times should be updated", licenceDatabase.LD_SystemLastEditTimeUtc, featureSetBReloaded.FCS_SystemLastEditTimeUtc);
			AssertEquals("Both feature set last edit times should be updated", licenceDatabase.LD_SystemLastEditUser, featureSetBReloaded.FCS_SystemLastEditUser);
			AssertEquals("Both feature set last edit users should be updated", newStaffB.GS_Code, featureSetAReloaded.FCS_SystemLastEditUser);
			AssertEquals("Both feature set last edit users should be updated", newStaffB.GS_Code, featureSetBReloaded.FCS_SystemLastEditUser);
		}

		[TestDate(2025, 01, 01)]
		public void TestResetLD_FCS_FeatureSetShouldUpdatePreviousFeatureSetLastEditTimeAndUser()
		{
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var featureSet = Factory.NewWithValidTestData<FeatureControlSet>();
			var newStaffA = Factory.NewWithValidTestData<GlbStaff>();
			var newStaffB = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			licenceDatabase.LD_FCS_FeatureSet = featureSet.PK;
			TestDateAttribute.AddDays(1);

			using (Env.SetTemporaryUserContext(new UserContext(newStaffA.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				Factory.Save();
			}

			var newFactory = new BusinessObjectFactory();
			var featureSetReloaded = newFactory.Load<FeatureControlSet>(featureSet.PK);

			AssertEquals("Precondition", licenceDatabase.LD_SystemLastEditTimeUtc, featureSetReloaded.FCS_SystemLastEditTimeUtc);
			AssertEquals("Precondition", licenceDatabase.LD_SystemLastEditUser, featureSetReloaded.FCS_SystemLastEditUser);
			AssertEquals("Precondition", newStaffA.GS_Code, featureSetReloaded.FCS_SystemLastEditUser);

			licenceDatabase.LD_FCS_FeatureSet = ZGuid.Empty;
			TestDateAttribute.AddDays(1);

			using (Env.SetTemporaryUserContext(new UserContext(newStaffB.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				Factory.Save();
			}

			AssertEquals("Should be updated since database was removed from feature set", licenceDatabase.LD_SystemLastEditTimeUtc, featureSetReloaded.FCS_SystemLastEditTimeUtc);
			AssertEquals("Should be updated since database was removed from feature set", new ZDateTime(2025, 01, 03), featureSetReloaded.FCS_SystemLastEditTimeUtc);
			AssertEquals("Should be updated since database was removed from feature set", licenceDatabase.LD_SystemLastEditUser, featureSetReloaded.FCS_SystemLastEditUser);
			AssertEquals("Should be updated since database was removed from feature set", newStaffB.GS_Code, featureSetReloaded.FCS_SystemLastEditUser);
		}

		[TestDate(2025, 01, 01)]
		public void TestSetLD_FCS_FeatureSetOnUnsavedLicenceDatabaseShouldUpdateFeatureSetLastEditTimeAndUser()
		{
			var featureSet = Factory.NewWithValidTestData<FeatureControlSet>();
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_FCS_FeatureSet = featureSet.PK;
			TestDateAttribute.AddDays(1);

			using (Env.SetTemporaryUserContext(new UserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				Factory.Save();
			}

			var newFactory = new BusinessObjectFactory();
			var featureSetReloaded = newFactory.Load<FeatureControlSet>(featureSet.PK);

			AssertEquals(licenceDatabase.LD_SystemLastEditTimeUtc, featureSetReloaded.FCS_SystemLastEditTimeUtc);
			AssertEquals(licenceDatabase.LD_SystemLastEditUser, featureSetReloaded.FCS_SystemLastEditUser);
			AssertEquals(newStaff.GS_Code, featureSetReloaded.FCS_SystemLastEditUser);
		}

		[TestDate(2025, 01, 01)]
		public void TestUnchangedLD_FCS_FeatureSetShouldNotUpdateFeatureSetLastEditTimeAndUser()
		{
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var featureSetA = Factory.NewWithValidTestData<FeatureControlSet>();
			var featureSetB = Factory.NewWithValidTestData<FeatureControlSet>();
			var oldStaff = Factory.NewWithValidTestData<GlbStaff>();
			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			licenceDatabase.LD_FCS_FeatureSet = featureSetA.PK;
			Factory.Save();

			var originalEditUser = featureSetB.FCS_SystemLastEditUser;

			AssertEquals("Precondition", featureSetA.PK, licenceDatabase.LD_FCS_FeatureSet);

			licenceDatabase.LD_FCS_FeatureSet = featureSetB.PK;
			licenceDatabase.LD_FCS_FeatureSet = featureSetA.PK;
			licenceDatabase.LD_DatabaseNumber = 1;
			TestDateAttribute.AddDays(1);

			using (Env.SetTemporaryUserContext(new UserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				Factory.Save();
			}

			var newFactory = new BusinessObjectFactory();
			var featureSetAReloaded = newFactory.Load<FeatureControlSet>(featureSetA.PK);
			var featureSetBReloaded = newFactory.Load<FeatureControlSet>(featureSetB.PK);

			AssertEquals("Should remain unchanged since database's feature set was returned to original value", new ZDateTime(2025, 01, 01), featureSetAReloaded.FCS_SystemLastEditTimeUtc);
			AssertEquals("Should remain unchanged since database's feature set was returned to original value", originalEditUser, featureSetAReloaded.FCS_SystemLastEditUser);
			AssertEquals("Should remain unchanged since database's feature set was returned to original value", new ZDateTime(2025, 01, 01), featureSetBReloaded.FCS_SystemLastEditTimeUtc);
			AssertEquals("Should remain unchanged since database's feature set was returned to original value", originalEditUser, featureSetBReloaded.FCS_SystemLastEditUser);
		}

		#region Test HTML Properties

		public void TestHtmlProperty()
		{
			var licenceDatabase = (LicenceDatabase)GetNewBusinessObject();
			AssertEquals(ZBlob.Empty, licenceDatabase.ConnectionDetails);
			AssertEquals(ZBlob.Empty, licenceDatabase.ConnectionDetails_HTML);

			licenceDatabase.ConnectionDetails_HTML = ZBlob.FromUTF8("<p>123</p>");

			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(licenceDatabase.ConnectionDetails.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", licenceDatabase.ConnectionDetails_HTML.ToUTF8());
		}

		public void TestHtmlFromTextProperty()
		{
			var licenceDatabase = (LicenceDatabase)GetNewBusinessObject();
			AssertEquals(ZBlob.Empty, licenceDatabase.ConnectionDetails);
			AssertEquals(ZBlob.Empty, licenceDatabase.ConnectionDetails_HTML);

			licenceDatabase.ConnectionDetails = ZBlob.FromUTF8("1234\r\n5678");

			AssertEquals("<p>1234</p><p>5678</p>", licenceDatabase.ConnectionDetails_HTML.ToUTF8());

			licenceDatabase.ConnectionDetails = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");

			AssertEquals("<p>rtf</p>", licenceDatabase.ConnectionDetails_HTML.ToUTF8());
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			return database;
		}

		EDIOrgHeader HeaderForTest
		{
			get
			{
				if (headerForTest == null)
				{
					headerForTest = Factory.NewWithValidTestData<EDIOrgHeader>();
					headerForTest.OH_RL_NKClosestPort = "AUBNE";
					headerForTest.OH_FullName = "My Organisation";
					headerForTest.OH_Code = "TGBLOG";

					OrgAddress newAddress = headerForTest.Addresses.AddNew();
					newAddress.OA_Address1 = "666 Test Address";
				}

				return headerForTest;
			}
		}
		EDIOrgHeader headerForTest;

		protected LicenceDatabase DatabaseForTest
		{
			get
			{
				if (databaseForTest == null)
				{
					databaseForTest = Factory.New<LicenceDatabase>();
				}
				return databaseForTest;
			}
		}
		LicenceDatabase databaseForTest;

		protected ReleaseBuild BuildForTest
		{
			get
			{
				if (buildForTest == null)
				{
					buildForTest = Factory.New<ReleaseBuild>();

					buildForTest.HL_MajorVersion = 1;
					buildForTest.HL_MinorVersion = 1;
					buildForTest.HL_Release = 1937;
					buildForTest.HL_Patch = 0;
				}
				return buildForTest;
			}
		}
		ReleaseBuild buildForTest;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			return database;
		}

		protected override void TearDown()
		{
			base.TearDown();
			databaseForTest = null;
		}

		#region Assertion Methods

		protected void AssertIsSupportedMethodForNoEmail()
		{
			AssertEquals("Empty Upgrade Method is not supported", false, DatabaseForTest.IsUpgradeMethodSupported(""));
			Assert("Blocked Upgrade Method is always supported", DatabaseForTest.IsUpgradeMethodSupported(UpgradeMethods.Codes.Blocked));
			AssertEquals("Http Upgrade Method is not supported because of empty Email address and no current version info", false, DatabaseForTest.IsUpgradeMethodSupported(UpgradeMethods.Codes.Http));
		}

		protected void AssertIsSupportedMethodForHasEmailNoVersionOrWithoutACK()
		{
			AssertEquals("Empty Upgrade Method is not supported", false, DatabaseForTest.IsUpgradeMethodSupported(""));
			Assert("Blocked Upgrade Method is always supported", DatabaseForTest.IsUpgradeMethodSupported(UpgradeMethods.Codes.Blocked));
			AssertEquals("Http Upgrade Method is not supported because of no current version info", false, DatabaseForTest.IsUpgradeMethodSupported(UpgradeMethods.Codes.Http));
		}

		protected void AssertIsSupportedMethodForHasEmailVersionWithACK()
		{
			AssertEquals("Empty Upgrade Method is not supported", false, DatabaseForTest.IsUpgradeMethodSupported(""));
			Assert("Blocked Upgrade Method is always supported", DatabaseForTest.IsUpgradeMethodSupported(UpgradeMethods.Codes.Blocked));
			AssertEquals("Http Upgrade Method is not supported because of empty Email address and no current version info", false, DatabaseForTest.IsUpgradeMethodSupported(UpgradeMethods.Codes.Http));
		}

		protected void AssertIsSupportedMethodForHasEmailVersionSupportsHttp()
		{
			AssertEquals("Empty Upgrade Method is not supported", false, DatabaseForTest.IsUpgradeMethodSupported(""));
			Assert("Blocked Upgrade Method is always supported", DatabaseForTest.IsUpgradeMethodSupported(UpgradeMethods.Codes.Blocked));
			Assert("Http Upgrade Method is not supported because of empty Email address and no current version info", DatabaseForTest.IsUpgradeMethodSupported(UpgradeMethods.Codes.Http));
		}

		#endregion

		#endregion
	}
}
