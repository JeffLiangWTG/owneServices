using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Licensing;
using CargoWise.Schema;
using CargoWise.Services.Calendar;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(LicenceHeader))]
	public class LicenceHeaderTest : SecurityBusinessObjectTestCase
	{
		public void TestLA_IsActive()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			Factory.Save();

			var orgs = licence.Database.Lookups.WebAccessOrgs;
			orgs.Load();
			AssertEquals(1, orgs.Count);

			licence.Database.Validation.ValidateLD_OH_WebAccessOrg();
			AssertNoWarnings(licence.Database.LD_OH_WebAccessOrgInfo);

			licence.LA_IsActive = false;
			Factory.Save();
			orgs = licence.Database.Lookups.WebAccessOrgs;
			orgs.Load();
			AssertEquals(1, licence.Database.Lookups.WebAccessOrgs.Count);

			licence.Database.Validation.ValidateLD_OH_WebAccessOrg();
			AssertHasWarning(licence.Database.LD_OH_WebAccessOrgInfo, "The selected Master Org doesn't have relationship with the database");
		}

		[TestDateIncremental(minutes: 30)]
		public void TestClientCompany()
		{
			var lic1 = BillingTestHelper.CreateLicenceWithoutClientCompany(Factory, "AAA", "SYD", "PRD");
			AssertNull(lic1.ClientCompany);

			var clientCompany = BillingTestHelper.CreateClientCompany(lic1.Database, lic1.Company);
			Factory.Save();
			AssertEquals(clientCompany, lic1.ClientCompany);
		}

		public void TestLicenceHeaderDeleteClearsForeignKeys()
		{
			LicenceHeader licence = Factory.New<LicenceHeader>();

			EDIProject project = Factory.New<EDIProject>();
			project.LicenceHeaderPK = licence.PK;

			licence.Delete();
			AssertEquals("Deleting licence should clear FK", ZGuid.Empty, project.LicenceHeaderPK);
			AssertEquals("Project should not be deleted", false, project.IsDeleted);
		}

		public void TestSendCalendarReminder()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Tiger Woods";

			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_FullName = "Tiger's Org";
			testHeader.MainAddress.OA_Address1 = "Par Street";
			testHeader.MainAddress.OA_City = "Somewhere";
			testHeader.MainAddress.OA_State = "NSW";
			testHeader.MainAddress.OA_PostCode = "2015";
			testHeader.MainAddress.OA_Phone = "2222";
			testHeader.MainAddress.OA_Mobile = "3333";
			testHeader.MainAddress.OA_Fax = "4444";
			testHeader.MainAddress.OA_Email = "5555@6666.com";
			testHeader.OH_Code = "TIGER";

			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(database);

			licHeader.LA_SiteLiveDate = new ZDateTime(2006, 5, 5);
			Factory.Save();
			Reminder rem = licHeader.RemindersCreatedInOnSavedForTesting[0];
			AssertEquals(ReminderType.Confirmed, rem.ReminderType);
		}

		public void TestCancelCalendarReminder()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Tiger Woods";

			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_FullName = "Tiger's Org";
			testHeader.MainAddress.OA_Address1 = "Par Street";
			testHeader.MainAddress.OA_City = "Somewhere";
			testHeader.MainAddress.OA_State = "NSW";
			testHeader.MainAddress.OA_PostCode = "2015";
			testHeader.MainAddress.OA_Phone = "2222";
			testHeader.MainAddress.OA_Mobile = "3333";
			testHeader.MainAddress.OA_Fax = "4444";
			testHeader.MainAddress.OA_Email = "5555@6666.com";
			testHeader.OH_Code = "TIGER";

			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(database);

			licHeader.LA_SiteLiveDate = ZDateTime.Now;
			Factory.Save();
			licHeader.RemindersCreatedInOnSavedForTesting.Clear();

			licHeader.LA_SiteLiveDate = ZDateTime.Empty;
			Factory.Save();

			Reminder rem = licHeader.RemindersCreatedInOnSavedForTesting[0];
			AssertEquals(ReminderType.Cancellation, rem.ReminderType);
		}

		public void TestCalendarReminder()
		{
			GlbStaff someStaff = Factory.New<GlbStaff>();
			someStaff.GS_FullName = "Puff Daddy";

			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_FullName = "Zubins Org Is Good";
			testHeader.MainAddress.OA_Address1 = "Some Street";
			testHeader.MainAddress.OA_City = "Somewhere";
			testHeader.MainAddress.OA_State = "NSW";
			testHeader.MainAddress.OA_PostCode = "2015";
			testHeader.MainAddress.OA_Phone = "2222";
			testHeader.MainAddress.OA_Mobile = "3333";
			testHeader.MainAddress.OA_Fax = "4444";
			testHeader.MainAddress.OA_Email = "5555@6666.com";
			testHeader.OH_Code = "ZUBIN";

			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(database);

			licHeader.LA_SiteLiveDate = new ZDateTime(2006, 5, 5);
			Factory.Save();

			Reminder rem = licHeader.RemindersCreatedInOnSavedForTesting[0];
			AssertEquals("1 recipient", 1, rem.Recipients.Count);
			AssertEquals("1 recipient - GoLiveUpdates", "Go-Live Updates", rem.Recipients[0].Name);
			AssertEquals("1 recipient - GoLiveUpdates", EDIDataRegistry.Instance.GoLiveUpdatesEmailAddress.Value, rem.Recipients[0].Email);

			string body = "Client Zubins Org Is Good (ZUBIN) has a Go-Live date of 05-May-06" + System.Environment.NewLine + System.Environment.NewLine;
			body += "Contact Details:" + System.Environment.NewLine;
			body += "SOME STREET SOMEWHERE NSW 2015" + System.Environment.NewLine;
			body += "Phone: 2222" + System.Environment.NewLine;
			body += "Mobile: 3333" + System.Environment.NewLine;
			body += "Fax: 4444" + System.Environment.NewLine;
			body += "Email: 5555@6666.com" + System.Environment.NewLine;

			AssertEquals("Correct Subject", "Client Go-Live - ZUBIN", rem.Subject);
			AssertEquals("Correct Body", body, rem.Body);
			AssertEquals("Correct From Date", licHeader.LA_SiteLiveDate.Date, rem.LocalDateFrom.Date);
			AssertEquals("Correct To Date", licHeader.LA_SiteLiveDate.Date, rem.LocalDateTo.Date);
			AssertEquals("Correct Alarm Period - 2 days before golive date", new TimeSpan(2, 0, 0, 0), rem.AlarmPeriod);

			testHeader.StaffAssignments.OverallSalesRep = someStaff.GS_Code;
			Factory.Save();
			rem = licHeader.RemindersCreatedInOnSavedForTesting[0];
			AssertEquals("1 recipient", 1, rem.Recipients.Count);
			AssertEquals("1 recipient - GoLiveUpdates", "Go-Live Updates", rem.Recipients[0].Name);

			someStaff.GS_EmailAddress = "zubin.appoo@cargowise.com";
			licHeader.LA_SupportMode = "XYZ";

			licHeader.LA_SiteLiveDate = new ZDateTime(2006, 5, 7);
			Factory.Save();
			rem = licHeader.RemindersCreatedInOnSavedForTesting[0];
			AssertEquals("2 recipients", 2, rem.Recipients.Count);
			AssertEquals("1st recipient - GoLiveUpdates", "Go-Live Updates", rem.Recipients[0].Name);
			AssertEquals("2nd recipient - GoLiveUpdates", "Puff Daddy", rem.Recipients[1].Name);

			licHeader.RemindersCreatedInOnSavedForTesting.Clear();
			Factory.Save();
			AssertEquals(0, licHeader.RemindersCreatedInOnSavedForTesting.Count);
		}

		public void TestLoadFromLicenceCode()
		{
			OrgHeader orgOne = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgTwo = Factory.NewWithValidTestData<OrgHeader>();

			LicenceDatabase dbOne = Factory.New<LicenceDatabase>();
			LicenceDatabase dbTwo = Factory.New<LicenceDatabase>();
			LicenceDatabase dbThree = Factory.New<LicenceDatabase>();

			LicenceEnterprise enterpriseOne = Factory.New<LicenceEnterprise>();
			LicenceEnterprise enterpriseTwo = Factory.New<LicenceEnterprise>();

			LicenceCompany companyOne = Factory.New<LicenceCompany>();
			LicenceCompany companyTwo = Factory.New<LicenceCompany>();

			LicenceHeader headerOne = Factory.New<LicenceHeader>();
			LicenceHeader headerTwo = Factory.New<LicenceHeader>();

			enterpriseOne.LE_EnterpriseCode = "ONE";
			enterpriseOne.LE_OH = orgOne.PK;

			enterpriseTwo.LE_EnterpriseCode = "TWO";
			enterpriseTwo.LE_OH = orgTwo.PK;

			dbOne.LD_LE = enterpriseOne.PK;
			dbOne.LD_ServerCode = "AAA";

			dbTwo.LD_LE = enterpriseOne.PK;
			dbTwo.LD_ServerCode = "BBB";

			dbThree.LD_LE = enterpriseTwo.PK;
			dbThree.LD_ServerCode = "BBB";

			companyOne.LC_CompanyCode = "ZZZ";
			companyOne.LC_LE = enterpriseOne.PK;
			companyOne.LC_OH = orgOne.PK;

			companyTwo.LC_CompanyCode = "ZZZ";
			companyTwo.LC_LE = enterpriseTwo.PK;
			companyTwo.LC_OH = orgTwo.PK;

			headerOne.LA_LC = companyOne.PK;
			headerOne.LA_LD = dbOne.PK;

			headerTwo.LA_LC = companyOne.PK;
			headerTwo.LA_LD = dbTwo.PK;

			Factory.Save();

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			LicenceHeader actual = LicenceHeader.LoadFromLicenceCode(otherFactory, "ONEZZZBBB");
			AssertEquals(headerTwo.PK, actual.PK);

			actual = LicenceHeader.LoadFromLicenceCode(otherFactory, "ONE");
			AssertNull(actual);

			actual = LicenceHeader.LoadFromLicenceCode(otherFactory, "ZZZ");
			AssertNull(actual);
		}

		#region Logging

		[TestDate(2005, 11, 16, 10, 10, 10)]
		public void TestLogging()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(database);
			licHeader.LA_SupportMode = "ABA";
			AssertEquals("Logs count", 0, licHeader.Logs.GetAllLogs().Count);
			Factory.Save();

			AssertEquals("Logs count", 1, licHeader.Logs.GetAllLogs().Count);

			ZString expectedReference = "Server";
			AssertEquals("Expected log reference", expectedReference, licHeader.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem).SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);

			licHeader.LA_SupportMode = "LLL";
			ZDateTime refTime = new ZDateTime(2005, 6, 6);
			ZString refTimeString = refTime.ToShortDateString();
			ZString refTimeAdd1DayString = refTime.AddDays(1).ToShortDateString();

			database.LD_ServerCode = "DDD";
			Factory.Save();

			expectedReference = "Server DDD: Support Mode: LLL(ABA)";
			AssertEquals("Expected log reference", expectedReference, licHeader.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);

			licHeader.LA_SiteLiveDate = refTime;
			licHeader.LA_SupportStartDate = refTime;
			licHeader.LA_AgreedLiveDate = refTime;
			licHeader.LA_InstallationCompleteDate = refTime;

			Factory.Save();

			expectedReference = "Server DDD:"
				+ " Installation Complete: " + refTimeString + "()"
				+ " Agreed Live: " + refTimeString + "()"
				+ " Site Live: " + refTimeString + "()"
				+ " Support Start: " + refTimeString + "()";

			expectedReference = expectedReference.SubstringSafe(0, StmALogSchema.SL_Reference.MaxLength);

			AssertEquals("Expected log reference", expectedReference, licHeader.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);

			licHeader.LA_SupportStartDate = refTime.AddDays(1);
			licHeader.LA_SiteLiveDate = ZDateTime.Empty;

			Factory.Save();

			expectedReference = "Server DDD:"
			+ " Site Live: (" + refTimeString + ")"
			+ " Support Start: " + refTimeAdd1DayString + "(" + refTimeString + ")";

			expectedReference = expectedReference.SubstringSafe(0, StmALogSchema.SL_Reference.MaxLength);

			AssertEquals("Expected log reference", expectedReference, licHeader.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var header = Factory.New<LicenceHeader>();
			var module = header.Modules.AddNew();

			AssertEquals("Should not include module related events.", 0, header.BusinessObjectsWithRelatedEvents.Length);
		}

		#endregion

		public void TestLicenceCode()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			testHeader.LicCompany.LicEnterprise.LE_EnterpriseCode = "XYZ";
			testHeader.LicCompany.LC_CompanyCode = "DEF";
			database.LD_ServerCode = "ABC";
			var licHeader = testHeader.LicCompany.GetHeader(database);
			AssertEquals("Licence Code", "XYZDEFABC", licHeader.LicenceCode);
		}

		public void TestLicenceSupportModeIsDefaultedToSTD()
		{
			var header = Factory.New<LicenceHeader>();
			AssertEquals("STD", header.LA_SupportMode);
		}

		public void TestCoreModuleUserCount()
		{
			LicenceHeader header = Factory.NewWithValidTestData<LicenceHeader>();
			header.Database.LD_Product = ProductTypes.Codes.Enterprise;
			AssertEquals("CoreModuleUserCount", ZShort.Zero, header.CoreModuleUserCount);

			header = Factory.NewWithValidTestData<LicenceHeader>();
			header.Database.LD_Product = ProductTypes.Codes.Enterprise;
			((LicenceModules)header.Modules.Find(new ZQuery(LicenceModulesSchema.LM_GroupModuleCode, LegacyLicence.Codes.Core))[0]).LM_UserCount = 5;
			AssertEquals("CoreModuleUserCount", (short)5, header.CoreModuleUserCount);
		}

		public void TestGetCoreModule()
		{
			LicenceHeader header = Factory.New<LicenceHeader>();
			header.Modules.RemoveAndDeleteAll();
			AssertNull(header.GetCoreModule());

			LicenceModules module = header.Modules.AddNew();
			AssertNull(header.GetCoreModule());

			module.LM_GroupModuleCode = LegacyLicence.Codes.Core;
			AssertEquals(module, header.GetCoreModule());

			LicenceHeader header2 = Factory.New<LicenceHeader>();
			header2.Modules.RemoveAndDeleteAll();
			LicenceModules module2 = header2.Modules.AddNew();
			module2.LM_GroupModuleCode = Env.Licence.Forwarder.Name;
			AssertNull(header2.GetCoreModule());
		}

		public void TestDatabaseCode()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			database.LD_ServerCode = "ABC";
			var licHeader = testHeader.LicCompany.GetHeader(database);
			AssertEquals("Code is ABC", "ABC", licHeader.DatabaseCode);
		}

		public void TestDatabaseIsActive()
		{
			var testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			var database = testHeader.LicCompany.LicDatabases.AddNew();
			database.LD_ServerCode = "ABC";
			database.LD_IsActive = false;
			var licHeader = testHeader.LicCompany.GetHeader(database);
			AssertEquals(false, licHeader.DatabaseIsActive);
			database.LD_IsActive = true;
			AssertEquals(true, licHeader.DatabaseIsActive);
		}

		public void TestCompanyCode()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceCompany company = testHeader.LicCompany;
			LicenceDatabase database = company.LicDatabases.AddNew();
			company.LC_CompanyCode = "ABC";
			var licHeader = testHeader.LicCompany.GetHeader(database);
			AssertEquals("Code is ABC", "ABC", licHeader.CompanyCode);
		}

		public void TestAddressAsString()
		{
			EDIOrgHeader testHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			testHeader.OH_Code = "ABCXYZ";
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();
			LicenceDatabase licDB = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(licDB);
			AssertEquals("Address is blank", "", licHeader.AddressAsString);

			licDB.LD_OA_SoftwareInstallAddressDetails = testHeader.MainAddress.PK;
			AssertEquals(testHeader.MainAddress.AddressAsASingleLine, licHeader.AddressAsString);
		}

		public void TestOrganisationCodeAndFullName()
		{
			EDIOrgHeader organisation = Factory.New<EDIOrgHeader>();
			organisation.OH_FullName = "Krispy Kreme";
			organisation.OH_Code = "KRSKRM";
			organisation.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = organisation.LicCompany.LicDatabases.AddNew();
			var licHeader = organisation.LicCompany.GetHeader(database);
			AssertEquals("OrganisationCode", "KRSKRM", licHeader.OrganisationCode);
			AssertEquals("OrganisationFullName", "Krispy Kreme", licHeader.OrganisationFullName);
		}

		public void Test24HrSupport()
		{
			LicenceHeader header = Factory.New<LicenceHeader>();
			Assert(!header.Is24HrSupport);

			header.LA_SupportMode = LicenceHeaderLookups.SupportModeConstants.Codes.Standard;
			Assert(!header.Is24HrSupport);

			header.LA_SupportMode = LicenceHeaderLookups.SupportModeConstants.Codes.Hour24;
			Assert(header.Is24HrSupport);

			header.LA_SupportMode = LicenceHeaderLookups.SupportModeConstants.Codes.NoSupport;
			Assert(!header.Is24HrSupport);
		}

		public void TestLA_LicenceAdvStdOth()
		{
			var moduleList = LicenceModuleList.Instance.NamesIncludingChildren;
			LicenceHeader licenceHeader = Factory.NewWithValidTestData<LicenceHeader>();
			licenceHeader.Database.LD_Product = ProductTypes.Codes.Enterprise;
			AssertEquals("Default value", LicenceAdvStdOthList.Codes.Advanced, licenceHeader.LA_LicenceAdvStdOth);
			AssertEquals("Modules are auto populated", moduleList.Count, licenceHeader.Modules.Count);
			LicenceModules lastCoreModule = licenceHeader.GetCoreModule();
			AssertEquals("Default licence type", LicenceTypes.Codes.NON, lastCoreModule.LM_LicenceType);

			foreach (ICodeDescription item in new LicenceAdvStdOthList())
			{
				string code = item.Code;
				if (code != LicenceAdvStdOthList.Codes.Advanced)
				{
					licenceHeader.LA_LicenceAdvStdOth = code;
					AssertEquals(code, licenceHeader.LA_LicenceAdvStdOth);
					AssertEquals(code + " Modules count should stay the same", moduleList.Count, licenceHeader.Modules.Count);
					AssertEquals(code + " Modules should not be removed and recreated", lastCoreModule, licenceHeader.GetCoreModule());
					Assert(code + " Default licence type", lastCoreModule.LM_LicenceType == LicenceTypes.Codes.NON || lastCoreModule.LM_LicenceType == LicenceTypes.Codes.ODM);
				}
			}
		}

		public void TestLA_LicenceAdvStdOth_UpdateForChangedEdition()
		{
			EDIOrgHeader organisation = Factory.NewWithValidTestData<EDIOrgHeader>();
			organisation.CreateAndLoadLicenceForOrg();
			LicenceDatabase licenceDatabase = organisation.LicCompany.LicDatabases.AddNew();
			var licHeader = organisation.LicCompany.GetHeader(licenceDatabase);
			licenceDatabase.LD_Product = ProductTypes.Codes.Enterprise;
			LicenceModules accLicence = licHeader.Modules.FindByCode(Env.Licence.Accountant.Name);
			accLicence.LM_LicenceType = LicenceTypes.Codes.PUR;
			LicenceModules cfsLicence = licHeader.Modules.FindByCode(Env.Licence.CFSManager.Name);
			cfsLicence.LM_LicenceType = LicenceTypes.Codes.REN;

			ReleaseBuild build = Factory.New<ReleaseBuild>();
			licenceDatabase.LD_HL_CurrentRunningVersion = build.PK;
			build.VersionNumber = LicenceHeader.LineLevelOnDemandLicenceVersion;
			Factory.Save();

			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			AssertEquals("Line Level ODM is supported", LicenceTypes.Codes.ODM, accLicence.LM_LicenceType);
			AssertEquals("Line Level ODM is supported", LicenceTypes.Codes.ODM, cfsLicence.LM_LicenceType);

			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Standard;
			accLicence.LM_LicenceType = LicenceTypes.Codes.REN;
			cfsLicence.LM_LicenceType = LicenceTypes.Codes.PUR;

			foreach (ICodeDescription item in new LicenceAdvStdOthList())
			{
				if (LicenceHeader.CalcIsPureOnDemand(item.Code))
				{
					licHeader.LA_LicenceAdvStdOth = item.Code;
					AssertEquals("Line Level ODM is supported", LicenceTypes.Codes.ODM, accLicence.LM_LicenceType);
					AssertEquals("Line Level ODM is supported", LicenceTypes.Codes.ODM, cfsLicence.LM_LicenceType);

					licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Standard;
					accLicence.LM_LicenceType = LicenceTypes.Codes.REN;
					cfsLicence.LM_LicenceType = LicenceTypes.Codes.PUR;
				}
			}

			build.VersionNumber = LicenceHeader.LineLevelOnDemandLicenceVersion.AddRelease(-1);
			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			AssertEquals("Line Level ODM is not supported", LicenceTypes.Codes.NON, accLicence.LM_LicenceType);
			AssertEquals("Line Level ODM is not supported", LicenceTypes.Codes.NON, cfsLicence.LM_LicenceType);
		}

		public void TestSortSetter()
		{
			LicenceHeader licenceHeader = Factory.New<LicenceHeader>();

			licenceHeader.Modules.RemoveAndDeleteAll();
			AssertEquals(licenceHeader.Modules.Count, 0);

			licenceHeader.Modules.AddNew();
			AssertEquals(licenceHeader.Modules.Count, 1);

			licenceHeader.HideUnlicenced = true;
			AssertNoExceptionThrown("Issue 0087007", () => licenceHeader.HideUnlicenced = true);
		}

		public void TestSupportsLineLevelOnDemandLicenceTypes()
		{
			LicenceHeader licenceHeader = Factory.New<LicenceHeader>();
			Assert("Database not specified", licenceHeader.SupportsLineLevelOnDemandLicenceTypes == TriState.NotDetermined);

			LicenceDatabase licenceDatabase = Factory.New<LicenceDatabase>();
			licenceHeader.LA_LD = licenceDatabase.PK;
			Assert("Build not specified", licenceHeader.SupportsLineLevelOnDemandLicenceTypes == TriState.NotDetermined);

			ReleaseBuild build = Factory.New<ReleaseBuild>();
			licenceDatabase.LD_HL_CurrentRunningVersion = build.PK;

			build.VersionNumber = LicenceHeader.LineLevelOnDemandLicenceVersion.AddRelease(-1);
			Assert(licenceHeader.SupportsLineLevelOnDemandLicenceTypes == TriState.False);

			build.VersionNumber = LicenceHeader.LineLevelOnDemandLicenceVersion;
			Assert(licenceHeader.SupportsLineLevelOnDemandLicenceTypes == TriState.True);
		}

		public void TestSupportsOpenLicence()
		{
			LicenceHeader licenceHeader = Factory.New<LicenceHeader>();
			Assert("Database not specified", !licenceHeader.SupportsOpenLicence);

			LicenceDatabase licenceDatabase = Factory.New<LicenceDatabase>();
			licenceHeader.LA_LD = licenceDatabase.PK;
			Assert("Build not specified", !licenceHeader.SupportsOpenLicence);

			ReleaseBuild build = Factory.New<ReleaseBuild>();
			licenceDatabase.LD_HL_CurrentRunningVersion = build.PK;

			build.VersionNumber = new VersionNumber(1, 4, 3748, 0);
			Assert(!licenceHeader.SupportsOpenLicence);

			build.VersionNumber = new VersionNumber(1, 4, 3749, 0);
			Assert(licenceHeader.SupportsOpenLicence);
		}

		public void TestIsCargoWiseInstallation()
		{
			LicenceHeader header = Factory.NewWithValidTestData<LicenceHeader>();
			LicenceEnterprise enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			LicenceCompany company = Factory.NewWithValidTestData<LicenceCompany>();
			header.LA_LC = company.PK;
			company.LC_LE = enterprise.PK;

			Factory.Save();

			AssertEquals(false, header.IsCargoWiseInstallation);

			InternalIncidentLicenceSettings settings = new InternalIncidentLicenceSettings();
			LicenceEnterpriseKey key = new LicenceEnterpriseKey();
			key.LE_PK = enterprise.PK;
			settings.LicenceEnterpriseKeys.Add(key);
			settings.EdiProd_LicencePK = header.PK;
			settings.UAT_ALP_LicencePK = header.PK;
			settings.UAT_DPR_LicencePK = header.PK;
			settings.UAT_GPC_LicencePK = header.PK;
			settings.UAT_GPR_LicencePK = header.PK;
			settings.UAT_STD_LicencePK = header.PK;
			EDIDataRegistry.Instance.InternalIncidentLicenceSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			AssertEquals(true, header.IsCargoWiseInstallation);
		}

		public void TestLegacyDatabaseDoesNotRegenerateModules()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			database.LD_Product = "AAA";
			Factory.Save();
			var licHeader = testHeader.LicCompany.GetHeader(database);
			AssertEquals("No Modules exist for this licence", 0, licHeader.Modules.Count);
		}

		public void TestCompany()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();

			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(database);
			AssertEquals("Header Company is correct", testHeader.LicCompany, licHeader.Company);
		}

		public void TestDatabase()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();

			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(database);
			AssertEquals("Header Database is correct", database, licHeader.Database);
		}

		public void TestDatabaseCurrentVersionExeVersion()
		{
			TestDatabaseProperty("DatabaseCurrentVersionExeVersion", ReleaseBuild.Schema.ExeVersion, LicenceDatabaseSchema.LD_HL_CurrentRunningVersion);
		}

		public void TestDatabaseCurrentVersionRelease()
		{
			TestDatabaseProperty("DatabaseCurrentVersionRelease", ReleaseBuild.Schema.ReleaseDisplayText, LicenceDatabaseSchema.LD_HL_CurrentRunningVersion);
		}

		public void TestDatabaseSentVersionExeVersion()
		{
			TestDatabaseProperty("DatabaseSentVersionExeVersion", ReleaseBuild.Schema.ExeVersion, LicenceDatabaseSchema.LD_HL_CurrentSentVersion);
		}

		public void TestDatabaseSentVersionRelease()
		{
			TestDatabaseProperty("DatabaseSentVersionRelease", ReleaseBuild.Schema.ReleaseDisplayText, LicenceDatabaseSchema.LD_HL_CurrentSentVersion);
		}

		public void TestModules()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "AAA");
			AssertEquals("Server Module count is == # modules", LicenceModuleList.Instance.NamesIncludingChildren.Count, licHeader.Modules.Count);
		}

		public void TestPurchasedModules()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "AAA");
			licHeader.Modules.FindByCode(BillingConstants.CoreModuleCode).LM_UserCount = 5;
			Factory.Save();
			Assert("multiple modules in DB", Factory.GetDatabaseCount(typeof(LicenceModules)) > 1);

			var factory2 = new BusinessObjectFactory();
			licHeader = factory2.Load<LicenceHeader>(licHeader.PK);
			var purchasedModules = licHeader.PurchasedModules;
			AssertEquals(1, purchasedModules.Count);
			AssertEquals(BillingConstants.CoreModuleCode, purchasedModules[0].LM_GroupModuleCode);
			IBusinessObjectFactoryInternals internals = factory2;
			AssertEquals("only purchased modules are loaded into memory", 1, internals.AllBusinessObjects.Count(x => x is LicenceModules));
		}

		public void TestDefaultValues()
		{
			LicenceHeader header = Factory.New<LicenceHeader>();
			AssertEquals("ADV is by default for LicenceAdvStdOth", LicenceAdvStdOthList.Codes.Advanced, header.LA_LicenceAdvStdOth);
			AssertEquals("Default is Standard", LicenceHeaderLookups.SupportModeConstants.Codes.Standard, header.LA_SupportMode);
		}

		public void TestEmailSentWhenDatesChange()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var testStaff = Factory.New<GlbStaff>();
			testStaff.GS_EmailAddress = "test@cargowise.com";
			testStaff.GS_LoginName = "test123";
			testStaff.GS_FullName = "Go away";

			var testHeader = HeaderForTest;
			testHeader.MainAddress.OA_Address1 = "30 Some Street";
			testHeader.MainAddress.OA_City = "Sydney";
			testHeader.MainAddress.OA_State = "NSW";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();

			var db = testHeader.LicCompany.LicDatabases.AddNew();
			db.LD_Product = ProductTypes.Codes.Enterprise;
			db.LD_OA_SoftwareInstallAddressDetails = testHeader.MainAddress.PK;
			var contact = HeaderForTest.Contacts.AddNew();
			contact.OC_ContactName = "Mary Jane";
			db.LD_OC_LicenseeAdminContact = contact.PK;
			db.LD_PublicEmailAddressForUpdate = "test@cargowise.com";
			db.LD_LicenceType = DatabaseTypes.Codes.Production;
			db.LD_DatabaseNumber = Base27Encoding.Decode("DBDB");
			testHeader.LicCompany.LC_CompanyNumber = Base27Encoding.Decode("CMPN");
			var licHeader = testHeader.LicCompany.GetHeader(db);

			var coreModule = licHeader.Modules.FindByCode(LegacyLicence.Codes.Core);
			coreModule.LM_Calc_IsEnabled = true;
			coreModule.LM_LicenceType = LicenceTypes.Codes.PUR;

			var salesManModule = licHeader.Modules.FindByCode(Env.Licence.RelationshipManager.Name);
			salesManModule.LM_Calc_IsEnabled = true;
			salesManModule.LM_UserCount = 13;
			salesManModule.LM_LicenceType = LicenceTypes.Codes.TRI;
			salesManModule.LM_ExpiryDate = new ZDateTime(2005, 3, 13);

			AssertEquals("Precondition: No emails sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			licHeader.LA_InstallationCompleteDate = new ZDateTime(2005, 5, 20);
			Factory.Save();
			AssertEquals("No emails sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			licHeader.LA_InstallationCompleteDate = new ZDateTime(2005, 5, 28);
			Factory.Save();
			AssertEquals("No email sent only as installation) as no sales rep set", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email Recipients Count", 2, sentEmail.Recipients.Count);
			AssertEquals("Email Subject", "Company " + licHeader.Company.Header.OH_FullName + " installation date modified.", sentEmail.Subject);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			testHeader.StaffAssignments.OverallSalesRep = testStaff.GS_Code;
			licHeader.LA_InstallationCompleteDate = new ZDateTime(2005, 2, 24);
			Factory.Save();
			AssertEquals("2 emails sent as install start date changed (implementation and installation)", 2, Env.OutgoingMailManager.EmailsCreated.Count);

			db.LD_HostServerName = "Zubs";
			db.LD_ServerCode = "BNE";
			Factory.Save();
			AssertEquals("No further emails were sent as nothing was changed", 2, Env.OutgoingMailManager.EmailsCreated.Count);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			licHeader.LA_SupportStartDate = new ZDateTime(2004, 1, 27);
			Factory.Save();
			AssertEquals("An email was sent as support date was changed", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			sentEmail = Env.OutgoingMailManager.EmailsCreated[0];

			string expectedEmailBody = @"
Implementation Dates were changed for the below Organisation.

Company Name:				My Organisation
Enterprise / Company Code:		MYO / BNE
Database Code:				BNE
Global Database / Company ID:	DBDB / CMPN
Software Install Address:		30 Some Street, Sydney, NSW
Licensee Admin Contact:			Mary Jane
Email Address for Updates:		test@cargowise.com


	Installation Complete Date:			24-Feb-05  (was 24-Feb-05)
	Agreed Live Date:		  (was )
	Estimated Live Date:			  (was )
	Site Live Date:			  (was )
	Support Start Date:		27-Jan-04  (was )  CHANGED


The above Organisation is licensed to use the following modules:

	Core                                    		0 users		Purchased
	PRA Messaging (Per Transaction)         		0 users		Charge Per Transaction
	ACIReporting (Per Transaction)          		0 users		Charge Per Transaction
	ACIeManifestReporting (Per Transaction) 		0 users		Charge Per Transaction
	AMSReporting                            		0 users		Charge Per Transaction
	StowPlanReporting                       		0 users		Charge Per Transaction
	CMDReporting                            		0 users		Charge Per Transaction
	Manifest (US e-Manifest)                		0 users		Charge Per Transaction
	ImportQuarantine (eBACCa)               		0 users		Charge Per Transaction
	SalesMarketing                          		13 users		Trial		Expires: 13-Mar-05
	ShippingManager Port Authority Messaging (Per Transaction)		0 users		Charge Per Transaction
	ShippingManager E-IDO Messaging (Per Transaction)		0 users		Charge Per Transaction
	DataWizard                              		0 users		Charge Per Transaction


These changes were made by " + GlbStaff.CurrentUser.GS_FullName + @".
";
			AssertMultilineASCIIEquals("SentEmail.Body", expectedEmailBody.Trim(), sentEmail.Body);
		}

		public void TestTrainingEmailSentWhenInstallationDatesChange()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.MainAddress.OA_Address1 = "30 Some Street";
			testHeader.MainAddress.OA_City = "Sydney";
			testHeader.MainAddress.OA_State = "NSW";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();

			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			database.LD_ServerCode = "BNE";
			database.LD_ServerCode = "BNE";
			database.LD_OA_SoftwareInstallAddressDetails = testHeader.MainAddress.PK;
			OrgContact contact = HeaderForTest.Contacts.AddNew();
			contact.OC_ContactName = "Mary Jane";
			database.LD_OC_LicenseeAdminContact = contact.PK;
			database.LD_PublicEmailAddressForUpdate = "test@cargowise.com";
			database.LD_DatabaseNumber = Base27Encoding.Decode("DBDB");
			testHeader.LicCompany.LC_CompanyNumber = Base27Encoding.Decode("CMPN");
			var licHeader = testHeader.LicCompany.GetHeader(database);
			LicenceModules salesManModule = licHeader.Modules.FindByCode(Env.Licence.RelationshipManager.Name);

			AssertEquals("Precondition: No emails sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			licHeader.LA_AgreedLiveDate = new ZDateTime(2005, 5, 20);
			licHeader.LA_InstallationCompleteDate = new ZDateTime(2005, 5, 21);
			Factory.Save();
			AssertEquals("No emails sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			licHeader.LA_AgreedLiveDate = new ZDateTime(2005, 5, 28);
			licHeader.LA_InstallationCompleteDate = new ZDateTime(2005, 5, 30);
			Factory.Save();
			AssertEquals("Email sent as install date changed", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			EmailDef sentEmail = Env.OutgoingMailManager.EmailsCreated[0];

			database.LD_HostServerName = "Zubs";
			Factory.Save();
			AssertEquals("No further emails were sent as nothing was changed", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			string expectedEmailBody = "Installation Dates were changed for the below Organisation.\r\n\r\nCompany Name:\t\t\t\tMy Organisation\r\nEnterprise / Company Code:\t\tMYO / BNE\r\nDatabase Code:\t\t\t\tBNE\r\nGlobal Database / Company ID:\tDBDB / CMPN\r\nSoftware Install Address:\t\t30 Some Street, Sydney, NSW\r\nLicensee Admin Contact:\t\t\tMary Jane\r\nEmail Address for Updates:\t\ttest@cargowise.com\r\n\r\n\tInstallation Completion Date:\t\t30-May-05  (was 21-May-05)\r\n\r\nThese changes were made by " + GlbStaff.CurrentUser.GS_FullName + ".";

			AssertEquals("Email Body", expectedEmailBody, sentEmail.Body);

			licHeader.LA_InstallationCompleteDate = new ZDateTime(2005, 5, 25);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Factory.Save();
			AssertEquals("Email sent as install complete date changed", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestCanAutoDeployLicenceKey()
		{
			var testHeader = HeaderForTest;
			testHeader.OH_FullName = "XB Test Organisation";
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.MainAddress.OA_Address1 = "88 Burke St";
			testHeader.MainAddress.OA_City = "Sydney";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();
			Factory.Save();

			var db = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(db);
			db.LD_ServerCode = "XXZ";
			db.LD_Product = ProductTypes.Codes.Enterprise;
			db.LD_PublicEmailAddressForUpdate = "";
			db.LD_HL_CurrentRunningVersion = ZGuid.Empty;
			licHeader.Modules[0].LM_UserCount = 4;
			licHeader.Modules[0].LM_LicenceType = "REN";
			licHeader.Modules[0].LM_ExpiryDate = ZDateTime.Today;
			Assert("Cannot AutoDeploy - no Version and Email", !testHeader.CanAutoDeployLicenceKey(licHeader));

			db.LD_PublicEmailAddressForUpdate = "XerxesTest@cargowise.com";
			Assert("Cannot AutoDeploy - no Version", !testHeader.CanAutoDeployLicenceKey(licHeader));

			var build = Factory.New<ReleaseBuild>();
			build.HL_ExeVersionDate = new ZDateTime(2005, 10, 01); // Before new licencing
			db.LD_HL_CurrentRunningVersion = build.PK;
			Assert("Cannot AutoDeploy - Version before 6/10/05", !testHeader.CanAutoDeployLicenceKey(licHeader));

			build.HL_ExeVersionDate = LicenceDatabase.DateFromWhichLicenceIsAutoDeployable;
			Assert("Can AutoDeploy", testHeader.CanAutoDeployLicenceKey(licHeader));
		}

		public void TestGenerateAndAutoDeployLicenceKey()
		{
			var testHeader = HeaderForTest;
			testHeader.OH_FullName = "XB Test Organisation";
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.MainAddress.OA_Address1 = "88 Burke St";
			testHeader.MainAddress.OA_City = "Sydney";
			testHeader.MainAddress.OA_Phone = "33333333";
			testHeader.MainAddress.OA_Fax = "44444444";
			testHeader.MainAddress.OA_Email = "test9@cargowise.com";
			testHeader.MainWebURL.PU_URL = "www.cargowise.com/test";
			var address2 = testHeader.Addresses.AddNew();
			address2.OA_CompanyNameOverride = "Branch 2";
			address2.OA_Address1 = "2nd St";
			address2.OA_City = "Brisbane";
			address2.OA_RL_NKRelatedPortCode = "AUBNE";
			address2.OA_State = "Queensland";
			address2.OA_Phone = "22222222";
			address2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();
			testHeader.LicCompany.LC_CompanyNumber = Base27Encoding.Decode("CMPN2");
			Factory.Save();

			var goodBuild = Factory.New<ReleaseBuild>();
			goodBuild.HL_ExeVersionDate = LicenceDatabase.DateFromWhichLicenceIsAutoDeployable;

			var db = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(db);
			db.LD_Product = ProductTypes.Codes.Enterprise;
			db.LD_ServerCode = "XXZ";
			db.LD_DatabaseNumber = Base27Encoding.Decode("DBDB2");
			db.LD_PublicEmailAddressForUpdate = "XerxesTest@cargowise.com";
			db.LD_HL_CurrentRunningVersion = ZGuid.Empty;
			licHeader.Modules[0].LM_UserCount = 4;
			licHeader.Modules[0].LM_LicenceType = "REN";
			licHeader.Modules[0].LM_ExpiryDate = ZDateTime.Today;
			Assert("Should NOT be able to deploy a key - No Version", !testHeader.GenerateAndAutoDeployLicenceKey(licHeader));

			db.LD_HL_CurrentRunningVersion = goodBuild.PK;
			Assert("Header is created", testHeader.LicCompany.LicHeadersForAllDatabases.Count == 1);
			Assert("Should be able to deploy a key", testHeader.CanAutoDeployLicenceKey(licHeader));

			testHeader.GenerateAndAutoDeployLicenceKey(licHeader);
			AssertEquals("One email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var sentMessage = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("One Recipient", 1, sentMessage.Recipients.Count);
			AssertEquals("Recipient is XerxeTest", "XerxesTest@cargowise.com", sentMessage.Recipients[0]);
			AssertEquals("Subject", "ediEnterprise Reference Data Update", sentMessage.Subject);
			AssertEquals("One Attachment", 1, sentMessage.Attachments.Count);
			AssertEquals("Attachment File Name", SystemUpdatePacketMailSender.AutoDeployLicenceKeyFileName, sentMessage.Attachments[0].DisplayName);

			var packet = new SystemUpdatePacket(Encoding.UTF8.GetString(sentMessage.Attachments[0].Data));
			AssertEquals("System Expiry Blank", "", packet.EncryptedSysRegKey);
			AssertEquals("Licence Key Created", testHeader.GenerateLicenceKey(licHeader), packet.EncryptedLicenceKey);

			var lic = new LegacyLicence(packet.EncryptedLicenceKey);
			AssertEquals("33333333", lic.Company.Phone);
			AssertEquals("44444444", lic.Company.Fax);
			AssertEquals("test9@cargowise.com", lic.Company.Email);
			AssertEquals("www.cargowise.com/test", lic.Company.WebAddress);

			AssertEquals("Use Org name if branch name not present", "XB Test Organisation", lic.Branches[0].BranchName);
			AssertEquals("Use branch override name if present", "Branch 2", lic.Branches[1].BranchName);

			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestCanGenerateLicenceKey()
		{
			var testHeader = HeaderForTest;
			testHeader.OH_RL_NKClosestPort = "AUSYD";

			AssertNull("Licence Enterprise is null before creation", testHeader.LicEnterprise);
			AssertNull("Licence Company is null before creation", testHeader.LicCompany);
			Assert("Should not be able to generate a key", !testHeader.CanGenerateLicenceKey(null));

			testHeader.CreateAndLoadLicenceForOrg();
			AssertNotNull("Licence Enterprise created", testHeader.LicEnterprise);
			AssertNotNull("Licence Company created", testHeader.LicCompany);

			var db = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(db);
			db.LD_Product = ProductTypes.Codes.Enterprise;
			licHeader.Modules[0].LM_UserCount = 4;
			licHeader.Modules[0].LM_LicenceType = "REN";
			licHeader.Modules[0].LM_ExpiryDate = ZDateTime.Today;

			Assert("Header is created", testHeader.LicCompany.LicHeadersForAllDatabases.Count == 1);
			Assert("Should be able to generate a key", testHeader.CanGenerateLicenceKey(licHeader));
		}

		[ExpectNoExceptions]
		public void TestGenerateLicenceKeyToFileSystem()
		{
			string fileName1 = "TGBLOG-XXZ.key";
			string fileName2 = "TGBLOG-ZZX.key";

			var testHeader = CreateHeader(Factory, "TGBLOG", "AUBNE");
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();
			testHeader.CreateAndLoadLicenceForOrg();
			Factory.Save();
			testHeader.GenerateNewLicenceCode();

			var branchAddress1 = testHeader.MainAddress;
			branchAddress1.OA_Code = "OFC: 88 Burke St";
			branchAddress1.OA_Address1 = "88 Burke St";
			branchAddress1.OA_Address2 = "Alexandria";
			branchAddress1.OA_City = "Sydney";
			branchAddress1.OA_State = "NSW";
			branchAddress1.OA_Phone = "02 90251113";
			branchAddress1.OA_Fax = "02 90252223";
			branchAddress1.OA_Email = "sydney@cargowise.com";
			branchAddress1.OA_CompanyNameOverride = "NSW Branch";
			branchAddress1.OA_OH = testHeader.PK;
			branchAddress1.OA_RL_NKRelatedPortCode = "AUSYD";

			var branchAddress2 = testHeader.Addresses.AddNew();
			branchAddress2.OA_Code = "OFC: 99 Burke St";
			branchAddress2.OA_Address1 = "99 Burke St";
			branchAddress2.OA_Address2 = "Melbourne";
			branchAddress2.OA_City = "Melbourne";
			branchAddress2.OA_State = "VIC";
			branchAddress2.OA_Phone = "03 90251113";
			branchAddress2.OA_Fax = "03 90252223";
			branchAddress2.OA_Email = "melbourne@cargowise.com";
			branchAddress2.OA_OH = testHeader.PK;
			branchAddress2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
			branchAddress2.OA_RL_NKRelatedPortCode = "AUMEL";

			var branchAddressWithoutUNLOCO = testHeader.Addresses.AddNew();
			branchAddressWithoutUNLOCO.OA_Code = "Nowhere";
			branchAddressWithoutUNLOCO.OA_Address1 = "1 Demo St";
			branchAddressWithoutUNLOCO.OA_OH = testHeader.PK;
			branchAddressWithoutUNLOCO.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
			branchAddressWithoutUNLOCO.OA_RL_NKRelatedPortCode = "";

			var db = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(db);
			db.LD_Product = ProductTypes.Codes.Enterprise;
			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			db.LD_ServerCode = "XXZ";
			licHeader.Modules[0].LM_UserCount = 4;
			licHeader.Modules[0].LM_LicenceType = "REN";
			licHeader.Modules[0].LM_ExpiryDate = ZDateTime.Today;
			licHeader.LA_AMS_USMode = "PRD";
			licHeader.LA_SupportMode = LicenceHeaderLookups.SupportModeConstants.Codes.Hour24;
			db.LD_HostedLocation = "SYD";

			var db2 = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader2 = testHeader.LicCompany.GetHeader(db2);
			db2.LD_Product = ProductTypes.Codes.Enterprise;
			licHeader2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Advanced;
			db2.LD_ServerCode = "ZZX";
			licHeader2.Modules[0].LM_UserCount = 4;
			licHeader2.Modules[0].LM_LicenceType = "REN";
			licHeader2.Modules[0].LM_ExpiryDate = ZDateTime.Today;
			db2.LD_HostedLocation = "TRA";

			try
			{
				string[] dirListBefore = Directory.GetFiles(Temp.TempPath, "*.key");
				Assert("There are no generated key files in the base dir", dirListBefore.Length == 0);

				testHeader.GenerateLicenceKeyToFileSystem(Temp.TempPath, licHeader);

				string[] dirListAfter1 = Directory.GetFiles(Temp.TempPath, "*.key");
				Assert("There is one key file", dirListAfter1.Length == 1);

				string keyFilePath = Path.Combine(Temp.TempPath, fileName1);
				Assert("Generated Key File was found", Array.IndexOf(dirListAfter1, keyFilePath) != -1);
				using (StreamReader reader1 = File.OpenText(keyFilePath))
				{
					string key1 = reader1.ReadToEnd();
					reader1.Close();
					var reloadedKey = new LegacyLicence(key1);

					AssertEquals("LicenceType", LicenceAdvStdOthList.Codes.OnDemand, reloadedKey.Type);
					AssertEquals("SupportMode", LicenceHeaderLookups.SupportModeConstants.Descriptions.Hour24, reloadedKey.SupportMode);
					AssertEquals("Hosted Location", "SYD", reloadedKey.ObsoleteHostedLocation);
					AssertEquals("OrgPK", testHeader.PK.ToGuid(), reloadedKey.Company.OrgPKThatGeneratedThisLicence);
					AssertEquals("EnterpriseCode", "TGB", reloadedKey.Company.EnterpriseCode);
					AssertEquals("Number of branches generated (exclude office address with no UNLOCO", 2, reloadedKey.Branches.Count);

					AssertEquals("BranchCode", "SYD", reloadedKey.Branches[0].Code);
					AssertEquals("Address1", branchAddress1.OA_Address1, reloadedKey.Branches[0].Address1);
					AssertEquals("Address2", branchAddress1.OA_Address2, reloadedKey.Branches[0].Address2);
					AssertEquals("City", branchAddress1.OA_City, reloadedKey.Branches[0].City);
					AssertEquals("State", branchAddress1.OA_State, reloadedKey.Branches[0].State);
					AssertEquals("Email", branchAddress1.OA_Email, reloadedKey.Branches[0].Email);
					AssertEquals("Phone", branchAddress1.OA_Phone, reloadedKey.Branches[0].Phone);
					AssertEquals("Fax", branchAddress1.OA_Fax, reloadedKey.Branches[0].Fax);
					AssertEquals("BranchName", "NSW Branch", reloadedKey.Branches[0].BranchName);
					AssertEquals("HomePortNK", branchAddress1.OA_RL_NKRelatedPortCode, reloadedKey.Branches[0].HomePortNK);
					AssertEquals("AMSMode is PRD", licHeader.LA_AMS_USMode, reloadedKey.InstallationDetails.AMSMode);

					AssertEquals("BranchCode", "MEL", reloadedKey.Branches[1].Code);
					AssertEquals("Address2", branchAddress2.OA_Address2, reloadedKey.Branches[1].Address2);
					AssertEquals("Address2", branchAddress2.OA_Address2, reloadedKey.Branches[1].Address2);
					AssertEquals("City", branchAddress2.OA_City, reloadedKey.Branches[1].City);
					AssertEquals("State", branchAddress2.OA_State, reloadedKey.Branches[1].State);
					AssertEquals("Email", branchAddress2.OA_Email, reloadedKey.Branches[1].Email);
					AssertEquals("Phone", branchAddress2.OA_Phone, reloadedKey.Branches[1].Phone);
					AssertEquals("Fax", branchAddress2.OA_Fax, reloadedKey.Branches[1].Fax);
					AssertEquals("BranchName", testHeader.OH_FullName, reloadedKey.Branches[1].BranchName);
					AssertEquals("HomePortNK", branchAddress2.OA_RL_NKRelatedPortCode, reloadedKey.Branches[1].HomePortNK);
				}

				testHeader.GenerateLicenceKeyToFileSystem(Temp.TempPath, licHeader2);
				string[] dirListAfter2 = Directory.GetFiles(Temp.TempPath, "*.key");
				Assert("There are 2 key files", dirListAfter2.Length == 2);
				Assert("Generated Key File was found", Array.IndexOf(dirListAfter2, Path.Combine(Temp.TempPath, fileName2)) != -1);

				keyFilePath = Path.Combine(Temp.TempPath, fileName2);
				using (StreamReader reader = File.OpenText(keyFilePath))
				{
					string key = reader.ReadToEnd();
					reader.Close();
					LegacyLicence reloadedKey = new LegacyLicence(key);
					AssertEquals("Hosted Location only set for CargoWise locations", "", reloadedKey.ObsoleteHostedLocation);
				}
			}
			finally
			{
				File.Delete(Path.Combine(Temp.TempPath, fileName1));
				File.Delete(Path.Combine(Temp.TempPath, fileName2));
			}
		}

		public void TestGenerateLicenceKey_ForOldODMClient()
		{
			var testHeader = CreateHeader(Factory, "TGBLOG", "AUBNE");
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();
			testHeader.CreateAndLoadLicenceForOrg();
			Factory.Save();
			testHeader.GenerateNewLicenceCode();

			var db = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(db);
			db.LD_Product = ProductTypes.Codes.Enterprise;
			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			licHeader.CoreModule.LM_LicenceType = LicenceTypes.Codes.NON;
			db.LD_ServerCode = "XXZ";
			AssertEquals("NON", licHeader.Modules[0].LM_LicenceType);
			licHeader.LA_AMS_USMode = "PRD";

			using (TempDirectory tempDir = new TempDirectory())
			{
				testHeader.GenerateLicenceKeyToFileSystem(tempDir.DirectoryName, licHeader);

				string licenceKeyFile = Path.Combine(tempDir.DirectoryName, "TGBLOG-XXZ.key");
				using (StreamReader reader = File.OpenText(licenceKeyFile))
				{
					string key = reader.ReadToEnd();
					LegacyLicence licenceKey = new LegacyLicence(key);
					AssertEquals("", licenceKey.Type);
					Assert("Should be identified as an old licence", string.IsNullOrEmpty(licenceKey.Type));
					Assert(((IExposeDeprecatedOnDemandModeFlag)licenceKey.InstallationDetails).OnDemandMode);
				}
			}
		}

		public void TestOrgIsInAllowedCountry()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			Assert("Org Is In Australia - Allowed", testHeader.OrgIsInAllowedCountry());

			testHeader.OH_RL_NKClosestPort = "THARA";
			Assert("Org Is In Thailand - Allowed", testHeader.OrgIsInAllowedCountry());

			testHeader.OH_RL_NKClosestPort = "ZAAAM";
			Assert("Org Is In South Africa - Allowed", testHeader.OrgIsInAllowedCountry());

			testHeader.OH_RL_NKClosestPort = "VNBEN";
			Assert("Org Is In Viet Nam - Allowed", testHeader.OrgIsInAllowedCountry());

			testHeader.OH_RL_NKClosestPort = "FJAQS";
			Assert("Org Is In Fiji - Allowed", testHeader.OrgIsInAllowedCountry());

			testHeader.OH_RL_NKClosestPort = "NZAKA";
			Assert("Org Is In New Zealand - Allowed", testHeader.OrgIsInAllowedCountry());

			testHeader.OH_RL_NKClosestPort = "SGAYC";
			Assert("Org Is In Singapore - Allowed", testHeader.OrgIsInAllowedCountry());

			testHeader.OH_RL_NKClosestPort = "IDABU";
			Assert("Org Is In Indonesia - Allowed", testHeader.OrgIsInAllowedCountry());

			testHeader.OH_RL_NKClosestPort = "XXXXX";
			Assert("Org Is In Made Up Country - Not Allowed", !testHeader.OrgIsInAllowedCountry());
		}

		public void TestABNandACNSetProperly()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			Assert("LicenceBusinessRegNo is blank", testHeader.LicenceTaxationRegNo.IsEmpty);
			Assert("LicenceTaxationRegNo is blank", testHeader.LicenceBusinessRegNo.IsEmpty);

			testHeader.OH_RL_NKClosestPort = "AUSYD";
			OrgCusCode aUABNCode = CreateNewCusCode(testHeader, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "AUABN", "AU");
			OrgCusCode aUACNCode = CreateNewCusCode(testHeader, OrgCusCode.CodeTypes.CorporationCode, "AUACN", "AU");
			AssertEquals("LicenceBusinessRegNo is AUABN - The Business Code is the same as the Tax Code for AU", "AUABN", testHeader.LicenceTaxationRegNo);
			AssertEquals("LicenceTaxationRegNo is AUACN - The Business Code is the same as the Tax Code for AU", "AUACN", testHeader.LicenceBusinessRegNo);

			testHeader.OH_RL_NKClosestPort = "SGSIN";
			OrgCusCode sGABNCode = CreateNewCusCode(testHeader, OrgCusCode.CodeTypes.GSTCode, "SGABN", "SG");
			OrgCusCode sGUENCode = CreateNewCusCode(testHeader, OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "SGUEN", "SG");
			AssertEquals("LicenceBusinessRegNo is SGABN - Tax Reg No", "SGABN", testHeader.LicenceTaxationRegNo);
			AssertEquals("LicenceEntityNumberRegNo is SGUEN - Business Reg No", "SGUEN", testHeader.LicenceBusinessRegNo);
		}

		public void TestReadOnlySecurity()
		{
			LicenceHeader licHeader = Factory.New<LicenceHeader>();
			string[] propertyNamesToExcept = new string[] { LicenceHeaderSchema.LA_LastLicenceSyncCheck.Name, LicenceHeader.Schema.LA_LastDiscrepancyChange };
			EDISecurityCheckpoints.OrgLicenceModifySupportAndContractDetails.IsAllowed = true;
			EDISecurityCheckpoints.OrgLicenceModifyInstallationDetails.IsAllowed = true;
			AssertPropertyInfosReadOnly(licHeader, false, propertyNamesToExcept);

			propertyNamesToExcept = new string[] {
													LicenceHeaderSchema.LA_AgreedLiveDate.Name,
													LicenceHeaderSchema.LA_AMS_USMode.Name,
													LicenceHeaderSchema.LA_EstimatedLiveDate.Name,
													LicenceHeaderSchema.LA_SiteLiveDate.Name,
													LicenceHeaderSchema.LA_InstallationCompleteDate.Name
													};
			EDISecurityCheckpoints.OrgLicenceModifySupportAndContractDetails.IsAllowed = false;
			EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed = false;
			AssertPropertyInfosReadOnly(licHeader, true, propertyNamesToExcept);

			propertyNamesToExcept = new string[] { LicenceHeaderSchema.LA_AgreedLiveDate.Name, LicenceHeaderSchema.LA_LastLicenceSyncCheck.Name, LicenceHeader.Schema.LA_LastDiscrepancyChange };
			EDISecurityCheckpoints.OrgLicenceModifySupportAndContractDetails.IsAllowed = true;
			EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed = true;
			EDISecurityCheckpoints.OrgLicenceModifyAgreedGoLive.IsAllowed = false;
			AssertPropertyInfosReadOnly(licHeader, false, propertyNamesToExcept);
		}

		public void TestReadOnlySecurityForSupportAndContract()
		{
			LicenceHeader licHeader = Factory.New<LicenceHeader>();
			string[] propertyNamesToExcept = new string[] { LicenceHeaderSchema.LA_LastLicenceSyncCheck.Name, LicenceHeader.Schema.LA_LastDiscrepancyChange };
			EDISecurityCheckpoints.OrgLicenceModifyInstallationDetails.IsAllowed = true;
			EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed = true;
			EDISecurityCheckpoints.OrgLicenceModifySupportAndContractDetails.IsAllowed = true;
			AssertPropertyInfosReadOnly(licHeader, false, propertyNamesToExcept);

			propertyNamesToExcept = new string[] {
													LicenceHeaderSchema.LA_SupportMode.Name,
													LicenceHeaderSchema.LA_SupportStartDate.Name,
													LicenceHeaderSchema.LA_ContractExpiryDate.Name,
													LicenceHeaderSchema.LA_SpecialSupportConditions.Name
													};
			EDISecurityCheckpoints.OrgLicenceModifyInstallationDetails.IsAllowed = false;
			EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed = false;
			EDISecurityCheckpoints.OrgLicenceModifyAgreedGoLive.IsAllowed = false;
			AssertPropertyInfosReadOnly(licHeader, true, propertyNamesToExcept);
		}

		public void TestIsPureOnDemand()
		{
			AssertEquals("count", 12, new LicenceAdvStdOthList().Count);
			AssertEquals(true, LicenceHeader.CalcIsPureOnDemand(LicenceAdvStdOthList.Codes.OnDemand));
			AssertEquals(true, LicenceHeader.CalcIsPureOnDemand(LicenceAdvStdOthList.Codes.ConversionToODPL));
			AssertEquals(false, LicenceHeader.CalcIsPureOnDemand(LicenceAdvStdOthList.Codes.SeatTransaction));

			AssertEquals(true, LicenceHeader.CalcIsPureOnDemand(LicenceAdvStdOthList.Codes.OnDemand));
			AssertEquals(false, LicenceHeader.CalcIsPureOnDemand(LicenceAdvStdOthList.Codes.ConcurrentCountry));
			AssertEquals(false, LicenceHeader.CalcIsPureOnDemand(LicenceAdvStdOthList.Codes.ConcurrentExpress));
			AssertEquals(false, LicenceHeader.CalcIsPureOnDemand(LicenceAdvStdOthList.Codes.ConcurrentRegional));
			AssertEquals(false, LicenceHeader.CalcIsPureOnDemand(LicenceAdvStdOthList.Codes.ConcurrentUniversal));

			AssertEquals(false, LicenceHeader.CalcIsPureOnDemand(LicenceAdvStdOthList.Codes.Hybrid));

			AssertEquals(false, LicenceHeader.CalcIsPureOnDemand(LicenceAdvStdOthList.Codes.Advanced));
			AssertEquals(false, LicenceHeader.CalcIsPureOnDemand(LicenceAdvStdOthList.Codes.Global));
			AssertEquals(false, LicenceHeader.CalcIsPureOnDemand(LicenceAdvStdOthList.Codes.Standard));

			LicenceHeader licHeader = Factory.New<LicenceHeader>();
			foreach (ICodeDescription item in new LicenceAdvStdOthList())
			{
				licHeader.LA_LicenceAdvStdOth = item.Code;
				AssertEquals(item.Code, LicenceHeader.CalcIsPureOnDemand(item.Code), licHeader.IsPureOnDemand);
			}
		}

		public void TestDiscrepancyText()
		{
			EDIOrgHeader org = CreateHeader(Factory, "TGBLOG", "AUBNE");
			org.OH_RL_NKClosestPort = "AUSYD";
			org.CreateAndLoadLicenceForOrg();
			var db = org.LicCompany.LicDatabases.AddNew();
			var licHeader = org.LicCompany.GetHeader(db);

			AssertEquals("", licHeader.DiscrepancyText);
			licHeader.DiscrepancyText = "foo";
			AssertEquals("foo", licHeader.DiscrepancyText);
		}

		[TestDate(2010, 1, 1)]
		public void TestIsLive()
		{
			LicenceHeader licHeader = Factory.New<LicenceHeader>();
			licHeader.LA_SiteLiveDate = ZDateTime.Empty;
			Assert("site live date empty", !licHeader.IsLive);

			licHeader.LA_SiteLiveDate = ZDateTime.Now.AddDays(1);
			Assert("live date in the future", !licHeader.IsLive);

			licHeader.LA_SiteLiveDate = ZDateTime.Now;
			Assert("live date today", licHeader.IsLive);

			licHeader.LA_SiteLiveDate = ZDateTime.Now.AddDays(-1);
			Assert("live date in the past", licHeader.IsLive);

			licHeader.LA_IsActive = false;
			Assert("not active", !licHeader.IsLive);
		}

		[TestDate(2016, 4, 6)]
		public void TestEdition()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "AAA", "PRD", false);

			var priceHeader = lic.Company.PriceHeaders.AddNew();

			var priceLink = lic.Database.PriceHeaderLinks.AddNew();
			priceLink.PHL_L6 = priceHeader.PK;
			priceLink.PHL_ValidFrom = new ZDateTime(2016, 5, 1);
			priceLink.PHL_RX_NKCurrency = "AUD";

			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Hybrid;
			AssertEquals(LicenceAdvStdOthList.Codes.Hybrid, lic.Edition);

			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			AssertEquals(LicenceAdvStdOthList.Codes.SeatTransaction, lic.Edition);

			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			AssertEquals(LicenceAdvStdOthList.Codes.OnDemand, lic.Edition);

			TestDateAttribute.Date = new DateTime(2016, 5, 1);
			lic.Database.InvalidateBillingModel();

			AssertEquals(LicenceAdvStdOthList.Codes.SeatTransaction, lic.Edition);
		}

		public void TestLA_LD()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "AAA", "PRD", false);
			var db = lic.Database;
			db.LD_Product = ProductTypes.Codes.BorderWise;
			var lic2 = BillingTestHelper.CreateAnotherLicence(db, "CO2", false);
			AssertEquals(LicenceAdvStdOthList.Codes.SeatTransaction, lic2.LA_LicenceAdvStdOth);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(database);
			return licHeader;
		}

		EDIOrgHeader HeaderForTest
		{
			get
			{
				if (fHeaderForTest == null)
				{
					fHeaderForTest = CreateHeader(Factory, "TGBLOG", "AUBNE");
					fHeaderForTest.OH_FullName = "My Organisation";
					OrgAddress newAddress = fHeaderForTest.Addresses.AddNew();
					newAddress.OA_Address1 = "666 Test Address";
				}

				return fHeaderForTest;
			}
		}
		EDIOrgHeader fHeaderForTest;

		EDIOrgHeader CreateHeader(BusinessObjectFactory inputFactory, string code, string uNLOCO)
		{
			var fHeaderForTest = inputFactory.NewWithValidTestData<EDIOrgHeader>();
			fHeaderForTest.MainAddress.OA_Address1 = "TestAddress";
			fHeaderForTest.OH_Code = code;
			fHeaderForTest.OH_RL_NKClosestPort = uNLOCO;
			return fHeaderForTest;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var testHeader = factory.New<EDIOrgHeader>();
			testHeader.OH_Code = "XYZABC";
			testHeader.MainAddress.OA_Address1 = "123";
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(database);
			return licHeader;
		}

		OrgCusCode CreateNewCusCode(OrgHeader header, ZString codeType, ZString regNo, ZString countryCode)
		{
			OrgCusCode returnCode = header.CustomsCodes.AddNew();
			returnCode.OK_CodeType = codeType;
			returnCode.OK_CustomsRegNo = regNo;
			returnCode.OK_RN_NKCodeCountry = countryCode;

			return returnCode;
		}

		void TestDatabaseProperty(string propertyToTest, string buildPropertyBeingProxied, SchemaColumn databaseBuildForeignKey)
		{
			LicenceHeader licence = Factory.New<LicenceHeader>();
			AssertEquals(propertyToTest, ZString.Empty, licence[propertyToTest]);

			LicenceDatabase database = Factory.New<LicenceDatabase>();
			licence.LA_LD = database.PK;

			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_Release = 200;
			build.HL_Patch = 10;
			database[databaseBuildForeignKey] = build.PK;

			AssertEquals(propertyToTest, build[buildPropertyBeingProxied], licence[propertyToTest]);
		}

		#endregion
	}
}
