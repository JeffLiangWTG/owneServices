using System;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.AutoDeploy;
using Enterprise.Client.EDI.AutoDeploy.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.Environment.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(UpgradeRequest))]
	public class UpgradeRequestTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertEquals("SupportedUpgradeMethod is Blocked", UpgradeMethods.Codes.Blocked, TestRequest.SupportedUpgradeMethod);
			AssertEquals("ClientContactPK is not specified", ZGuid.Empty, TestRequest.ClientContactPK);
			AssertNull("ClientContact is null", TestRequest.ClientContact);
			AssertEquals("CLientContactEmailAddress is empty", ZString.Empty, TestRequest.ClientContactEmailAddress);
			Assert("ScheduledDateTime is empty", TestRequest.ScheduledDateTime.IsEmpty);
		}

		public void TestEnterpriseCode()
		{
			TestLicDB.LicEnterprise.LE_EnterpriseCode = "TST";
			AssertEquals("EnterpriseCode", "TST", TestRequest.EnterpriseCode);
		}

		public void TestCompanyName()
		{
			TestOrg.OH_FullName = "Test Org";
			AssertEquals("CompanyName", "Test Org", TestRequest.CompanyName);
		}

		public void TestPreferredUpgradeMethod()
		{
			TestLicDB.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			AssertEquals("PreferredUpgradeMEthod", UpgradeMethods.Codes.Blocked, TestRequest.PreferredUpgradeMethod);
		}

		public void TestServerCode()
		{
			TestLicDB.LD_ServerCode = "TST";
			AssertEquals("ServerCode", "TST", TestRequest.ServerCode);
		}

		public void TestProduct()
		{
			TestLicDB.LD_Product = ProductTypes.Codes.CargoWiseOne;
			AssertEquals("Product", "CW1", TestRequest.Product);

			TestLicDB.LD_Product = ProductTypes.Codes.CargoWiseNext;
			AssertEquals("Product", "CWN", TestRequest.Product);

			const string errorMessage = "The release build product does not match to license database product.";
			TestLicDB.LD_ReleaseRing = ReleaseRings.Codes.GP1;
			TestHttpBuild.HL_ReleaseStatus = ReleaseRings.Codes.GP1;

			TestHttpBuild.HL_Product = ProductTypes.Codes.Enterprise;
			TestRequest.SetSelectedReleaseBuild(TestHttpBuild);
			AssertHasError(TestRequest.ProductInfo, errorMessage);

			TestLicDB.LD_Product = ProductTypes.Codes.CargoWiseOne;
			TestRequest.SetSelectedReleaseBuild(TestHttpBuild);
			AssertNoNotifications(TestRequest.ProductInfo);

			TestLicDB.LD_Product = ProductTypes.Codes.ProductivityWise;
			TestRequest.SetSelectedReleaseBuild(TestHttpBuild);
			AssertNoNotifications(TestRequest.ProductInfo);

			TestHttpBuild.HL_Product = ProductTypes.Codes.CargoWiseNext;
			TestRequest.SetSelectedReleaseBuild(TestHttpBuild);
			AssertHasError(TestRequest.ProductInfo, errorMessage);

			TestLicDB.LD_Product = ProductTypes.Codes.CargoWiseNext;
			TestRequest.SetSelectedReleaseBuild(TestHttpBuild);
			AssertNoNotifications(TestRequest.ProductInfo);
		}

		public void TestSupportedUpgradeMethod()
		{
			AssertEquals("Current value", UpgradeMethods.Codes.Blocked, TestRequest.SupportedUpgradeMethod);

			TestRequest.SupportedUpgradeMethod = UpgradeMethods.Codes.Blocked;
			AssertEquals("Has assigned value", UpgradeMethods.Codes.Blocked, TestRequest.SupportedUpgradeMethod);
			AssertHasError(TestRequest.SupportedUpgradeMethodInfo, "Please select another method if you want to send an upgrade to this database or delete the row if you do not want an upgrade to be sent.");

			TestRequest.SupportedUpgradeMethod = UpgradeMethods.Codes.Http;
			string htpWarning = UpgradeRequest.HttpUploadVersionMessage;
			AssertHasWarning(TestRequest.SupportedUpgradeMethodInfo, htpWarning);

			TestRequest.SupportedUpgradeMethod = UpgradeMethods.Codes.Blocked;
			AssertNoWarning(TestRequest.SupportedUpgradeMethodInfo, htpWarning);

			TestLicDB.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			TestRequest.SupportedUpgradeMethod = UpgradeMethods.Codes.Http;
			AssertHasWarning(TestRequest.SupportedUpgradeMethodInfo, "You are ignoring client's Preferred UpgradeMethod " + UpgradeMethods.Codes.Blocked);
		}

		public void TestGetSupportedUpgradeMethods()
		{
			AssertSupportedMethods(TestRequest.GetSupportedUpgradeMethods(), new string[] { UpgradeMethods.Codes.Blocked });

			TestLicDB.LD_PublicEmailAddressForUpdate = "Test@edi.com.au";
			AssertSupportedMethods(TestRequest.GetSupportedUpgradeMethods(), new string[] { UpgradeMethods.Codes.Blocked });

			TestLicDB.LD_HL_CurrentRunningVersion = TestEWABuild.PK;
			AssertSupportedMethods(TestRequest.GetSupportedUpgradeMethods(), new string[] { UpgradeMethods.Codes.Blocked });

			TestLicDB.LD_HL_CurrentRunningVersion = TestHttpBuild.PK;
			AssertSupportedMethods(TestRequest.GetSupportedUpgradeMethods(), new string[] { UpgradeMethods.Codes.Blocked, UpgradeMethods.Codes.Http });
		}

		public void TestScheduledDateTime()
		{
			Assert("ScheduledDateTime is empty", TestRequest.ScheduledDateTime.IsEmpty);
			ZDateTime manualSetTime = ZDateTime.Now.AddMinutes(100);

			TestRequest.ScheduledDateTime = manualSetTime;
			AssertEquals("Assigned Value", manualSetTime, TestRequest.ScheduledDateTime);

			TestLicDB.LD_HL_CurrentRunningVersion = TestHttpBuild.PK;

			TestRequest.SupportedUpgradeMethod = UpgradeMethods.Codes.Http;
			AssertEquals("Manually set Time was preserved", manualSetTime, TestRequest.ScheduledDateTime);

			TestRequest.ScheduledDateTime = manualSetTime;
			AssertEquals("Assigned Value", manualSetTime, TestRequest.ScheduledDateTime);
		}

		public void TestCompanyContactAndEmail()
		{
			AssertNull("ClientContact is null", TestRequest.ClientContact);

			TestRequest.ClientContactPK = TestContact.PK;
			AssertEquals("Assigned Contact", TestContact, TestRequest.ClientContact);
			AssertEquals("Contact Email Address", "test.contact@edi.com", TestRequest.ClientContactEmailAddress);

			TestRequest.ClientContactPK = ZGuid.Empty;
			AssertNull("Contact is null", TestRequest.ClientContact);
			AssertEquals("Contact Email Address is empty", ZString.Empty, TestRequest.ClientContactEmailAddress);

			TestRequest.ClientContactPK = TestContact.PK;
			AssertEquals("Assigned Contact", TestContact, TestRequest.ClientContact);

			TestRequest.ClientContactEmailAddress = "custom@edi.com";
			AssertEquals("Assigned Email address", "custom@edi.com", TestRequest.ClientContactEmailAddress);
		}

		public void TestClientContactEmailAddressValidation()
		{
			AssertEquals("Email is Empty", ZString.Empty, TestRequest.ClientContactEmailAddress);

			TestRequest.SupportedUpgradeMethod = UpgradeMethods.Codes.Blocked;
			AssertNoError(TestRequest.ClientContactEmailAddressInfo, "You should specify a valid email address for notification.");

			TestRequest.SupportedUpgradeMethod = UpgradeMethods.Codes.Http;
			AssertHasError(TestRequest.ClientContactEmailAddressInfo, "You should specify a valid email address for notification.");

			string badEmail = "Bad.Email";
			TestRequest.ClientContactEmailAddress = badEmail;
			AssertEquals("Email is Bad", badEmail, TestRequest.ClientContactEmailAddress);
			AssertHasError(TestRequest.ClientContactEmailAddressInfo, "You should specify a valid email address for notification.");

			TestRequest.SupportedUpgradeMethod = UpgradeMethods.Codes.Blocked;
			AssertNoError(TestRequest.ClientContactEmailAddressInfo, "You should specify a valid email address for notification.");
		}

		public void TestAssignLicemceAdminContactFromLicDatabase()
		{
			TestLicDB.LD_OC_LicenseeAdminContact = TestContact.PK;
			AssertEquals("Lic Admin Contact set by dafault", TestContact, TestRequest.ClientContact);
		}

		public void TestAssignInstallerContactFromLicDatabase()
		{
			TestLicDB.LD_OC_ContractInstallerOrInternalTechContact = TestContact.PK;
			TestOrg.Contacts.AddNew();
			TestLicDB.LD_OC_LicenseeAdminContact = TestOrg.Contacts[1].PK;

			AssertEquals("Installer Contact set by dafault. Admin contact was ignored.", TestContact, TestRequest.ClientContact);
		}

		public void TestSetBestSupportedUpgradeMethodForSupportingAllNoPrferredMethod()
		{
			TestLicDB.LD_PublicEmailAddressForUpdate = "some.address@edi.com";
			TestLicDB.LD_HL_CurrentRunningVersion = TestHttpBuild.PK;
			TestLicDB.LD_AvailableUpgradeMethod = "";

			SetDesiredAndAssertActualUpgradeMethod("", UpgradeMethods.Codes.Http);
			SetDesiredAndAssertActualUpgradeMethod(UpgradeMethods.Codes.Http, UpgradeMethods.Codes.Http);
		}

		public void TestSetBestSupportedUpgradeMethodForNoUpgradeEmail()
		{
			TestLicDB.LD_HL_CurrentRunningVersion = TestHttpBuild.PK;
			TestLicDB.LD_AvailableUpgradeMethod = "";
			AssertEquals("No Upgrade email", ZString.Empty, TestLicDB.LD_PublicEmailAddressForUpdate);

			SetDesiredAndAssertActualUpgradeMethod("", UpgradeMethods.Codes.Blocked);
			SetDesiredAndAssertActualUpgradeMethod(UpgradeMethods.Codes.Http, UpgradeMethods.Codes.Blocked);
		}

		public void TestSetBestSupportedUpgradeMethodForNoUpgradeEmail_HttpOnly()
		{
			TestLicDB.LD_HL_CurrentRunningVersion = TestHttpOnlyBuild.PK;
			TestLicDB.LD_AvailableUpgradeMethod = "";
			AssertEquals("No Upgrade email", ZString.Empty, TestLicDB.LD_PublicEmailAddressForUpdate);

			SetDesiredAndAssertActualUpgradeMethod("", UpgradeMethods.Codes.Http);
			SetDesiredAndAssertActualUpgradeMethod(UpgradeMethods.Codes.Http, UpgradeMethods.Codes.Http);
		}

		public void TestSetBestSupportedUpgradeMethodForSupportingEWANoPreferredMethod()
		{
			TestLicDB.LD_PublicEmailAddressForUpdate = "some.address@edi.com";
			TestLicDB.LD_HL_CurrentRunningVersion = this.TestEWABuild.PK;
			TestLicDB.LD_AvailableUpgradeMethod = "";

			SetDesiredAndAssertActualUpgradeMethod("", UpgradeMethods.Codes.Blocked);
			SetDesiredAndAssertActualUpgradeMethod(UpgradeMethods.Codes.Http, UpgradeMethods.Codes.Blocked);
		}

		public void TestSetDefaultScheduledTime()
		{
			AssertEquals("Upgrade Method is Blocked", UpgradeMethods.Codes.Blocked, TestRequest.SupportedUpgradeMethod);
			Assert("ScheduledDateTime is empty", TestRequest.ScheduledDateTime.IsEmpty);
			TestLicDB.LD_HL_CurrentRunningVersion = TestHttpBuild.PK;
			TestRequest.SupportedUpgradeMethod = UpgradeMethods.Codes.Http;
			ZDateTime defaultTime = ZDateTime.Now.AddMinutes(100);

			TestRequest.SetDefaultScheduledTime(defaultTime);
			AssertEquals("Assigned Value", defaultTime, TestRequest.ScheduledDateTime);

			ZDateTime defaultTime1 = ZDateTime.Now.AddMinutes(5);
			TestRequest.SetDefaultScheduledTime(defaultTime1);
			AssertEquals("New default value set", defaultTime1, TestRequest.ScheduledDateTime);

			TestRequest.SetDefaultScheduledTime(defaultTime);
			AssertEquals("Assigned Value", defaultTime, TestRequest.ScheduledDateTime);

			TestRequest.SupportedUpgradeMethod = UpgradeMethods.Codes.Blocked;
			AssertEquals("Time is Empty", ZDateTime.Empty, TestRequest.ScheduledDateTime);
			TestRequest.SetDefaultScheduledTime(defaultTime);
			AssertEquals("Time still Empty", ZDateTime.Empty, TestRequest.ScheduledDateTime);
		}

		public void TestCreateUpgradesToClient()
		{
			TestRequest.SupportedUpgradeMethod = "";
			AssertNull("No UpgradesToClient should be created", TestRequest.CreateUpgradesToClient(TestHttpBuild));

			TestRequest.SupportedUpgradeMethod = UpgradeMethods.Codes.Blocked;
			AssertNull("No UpgradesToClient should be created", TestRequest.CreateUpgradesToClient(TestHttpBuild));

			TestRequest.SupportedUpgradeMethod = UpgradeMethods.Codes.Http;
			TestRequest.ClientContactPK = TestContact.PK;
			ZDateTime scheduledTime = ZDateTime.Now;
			TestRequest.ScheduledDateTime = scheduledTime;
			UpgradesToClient upgrade = TestRequest.CreateUpgradesToClient(TestHttpBuild);

			AssertNotNull("UpgradesToClient should be created", upgrade);
			AssertEquals("SelectedReleaseBuild", TestHttpBuild.PK, upgrade.L1_HL);
			AssertEquals("Current User", Factory.Load<GlbStaff>(Env.CurrentUser.PK).GS_Code, upgrade.L1_GS_NKStaffCode);
			AssertEquals("LicDatabase", TestLicDB.PK, upgrade.L1_LD);
			AssertEquals("Organisation", TestOrg.PK, upgrade.L1_OH);
			AssertEquals("RequestedUpgradeMethod", UpgradeMethods.Codes.Http, upgrade.L1_RequestedUpgradeMethod);
			AssertEquals("CurrentStatus", UpgradesToClientStatus.Codes.Queued, upgrade.L1_CurrentStatus);
			AssertEquals("RequestedDateTime", scheduledTime, upgrade.L1_RequestedDateTime);
			AssertEquals("ClientContact", TestContact.PK, upgrade.L1_OC);
		}

		public void TestCreateUpgradesToClientNonUserLoggedin()
		{
			EnvProvider providerToSave = Env.GetCurrentProvider();
			var nullEnvironment = new NullEnvProvider();

			TestRequest.SupportedUpgradeMethod = "";
			AssertNull("No UpgradesToClient should be created", TestRequest.CreateUpgradesToClient(TestHttpBuild));

			TestRequest.SupportedUpgradeMethod = UpgradeMethods.Codes.Blocked;
			AssertNull("No UpgradesToClient should be created", TestRequest.CreateUpgradesToClient(TestHttpBuild));

			TestRequest.SupportedUpgradeMethod = UpgradeMethods.Codes.Http;
			TestRequest.ClientContactPK = TestContact.PK;
			ZDateTime scheduledTime = ZDateTime.Now;
			TestRequest.ScheduledDateTime = scheduledTime;

			try
			{
				nullEnvironment.Enable();
				AssertEquals("Precondition: Env.CurrentUser", null, Env.CurrentUser);
				UpgradesToClient upgrade = TestRequest.CreateUpgradesToClient(TestHttpBuild);

				AssertNotNull("UpgradesToClient should be created", upgrade);
				AssertEquals("SelectedReleaseBuild", TestHttpBuild.PK, upgrade.L1_HL);
				AssertEquals("Web User", "ZZ", upgrade.L1_GS_NKStaffCode);
				AssertEquals("LicDatabase", TestLicDB.PK, upgrade.L1_LD);
				AssertEquals("Organisation", TestOrg.PK, upgrade.L1_OH);
				AssertEquals("RequestedUpgradeMethod", UpgradeMethods.Codes.Http, upgrade.L1_RequestedUpgradeMethod);
				AssertEquals("CurrentStatus", UpgradesToClientStatus.Codes.Queued, upgrade.L1_CurrentStatus);
				AssertEquals("RequestedDateTime", scheduledTime, upgrade.L1_RequestedDateTime);
				AssertEquals("ClientContact", TestContact.PK, upgrade.L1_OC);
			}
			finally
			{
				providerToSave.Enable();
				nullEnvironment.Dispose();
			}
		}

		public void TestNotificationMessage()
		{
			TestLicDB.LD_ServerCode = "TST";
			TestLicDB.LD_PublicEmailAddressForUpdate = "some.address@edi.com";
			TestLicDB.LD_HL_CurrentRunningVersion = this.TestHttpBuild.PK;
			ZDateTime scheduledTime = ZDateTime.Now;
			TestRequest.SupportedUpgradeMethod = UpgradeMethods.Codes.Http;
			TestRequest.ScheduledDateTime = scheduledTime;

			string expectedMessage = string.Format(CultureInfo.CurrentCulture, "Server TST at {0} via {1}.\r\n", scheduledTime, UpgradeMethods.Descriptions.Http);
			AssertEquals("Notification Message", expectedMessage, TestRequest.NotificationMessage);

			TestRequest.SupportedUpgradeMethod = UpgradeMethods.Codes.Blocked;
			expectedMessage = string.Format(CultureInfo.CurrentCulture, "Server TST is {0}.\r\n", UpgradeMethods.Descriptions.Blocked);
			AssertEquals("Notification Message for Blocked", expectedMessage, TestRequest.NotificationMessage);

			TestLicDB.LD_HL_CurrentRunningVersion = ZGuid.Empty;
			TestLicDB.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			UpgradeRequest request2 = new UpgradeRequest(Factory, TestOrg, TestLicDB);
			request2.ScheduledDateTime = scheduledTime;
			request2.SupportedUpgradeMethod = UpgradeMethods.Codes.Blocked;
			expectedMessage = string.Format(CultureInfo.CurrentCulture, "Server TST is {0}.\r\n", UpgradeMethods.Descriptions.Blocked);
			AssertEquals("Notification Message for Blocked", expectedMessage, TestRequest.NotificationMessage);
		}

		public void TestSetSelectedReleaseBuildValidatesReleaseRing()
		{
			AssertNoErrors("Precondition: ReleaseRing should not have errors.", TestRequest.ReleaseRingInfo);

			TestLicDB.LD_ReleaseRing = ReleaseRings.Codes.GPR;
			SecurityCheckpoint checkpoint = EDISecurityCheckpoints.OrgLicenceModifySendUpgradeToHigherRingDB;

			checkpoint.IsAllowed = true;
			TestHttpBuild.HL_ReleaseStatus = ReleaseRings.Codes.ALP;
			TestRequest.SetSelectedReleaseBuild(TestHttpBuild);
			AssertHasWarning(TestRequest.ReleaseRingInfo, "This server's release ring is higher than the selected build's release ring.");
			AssertNoErrors(TestRequest.ReleaseRingInfo);

			string errorMessage = string.Format(CultureInfo.CurrentCulture, "You cannot send the selected build to this server because its release ring is higher than the build's release ring.{0}{0}If you need to send upgrade packages to client databases on a higher ring than the package, please ask your administrator to change either your Staff or Group Security Rights to allow access to:{0}{0}{1}", System.Environment.NewLine, checkpoint.DisplayTextPathToSecurityRight);

			checkpoint.IsAllowed = false;
			TestHttpBuild.HL_ReleaseStatus = ReleaseRings.Codes.DPR;
			TestRequest.SetSelectedReleaseBuild(TestHttpBuild);
			AssertHasError(TestRequest.ReleaseRingInfo, errorMessage);
			AssertNoWarnings(TestRequest.ReleaseRingInfo);

			TestRequest.SetSelectedReleaseBuild(TestHttpBuild);
			AssertHasError(TestRequest.ReleaseRingInfo, errorMessage);
			AssertNoWarnings(TestRequest.ReleaseRingInfo);

			TestHttpBuild.HL_ReleaseStatus = ReleaseRings.Codes.STD;
			TestRequest.SetSelectedReleaseBuild(TestHttpBuild);
			AssertHasError(TestRequest.ReleaseRingInfo, errorMessage);

			TestEWABuild.HL_ReleaseStatus = ReleaseRings.Codes.GPR;
			TestRequest.SetSelectedReleaseBuild(TestEWABuild);
			AssertNoNotifications(TestRequest.ReleaseRingInfo);
		}

		public void TestSetSelectedReleaseBuildValidatesSqlVersion()
		{
			AssertNoNotifications("Precondition: SqlVersion should not have any notifications.", TestRequest.SqlVersionInfo);

			var bumpingDateBuild = CreateReleaseBuild(SqlCutOverHelperTestHelper.DateWhenMinimumGenerationBump, ReleaseRings.Codes.ALP);
			var oldBuild = CreateReleaseBuild(2, 0, 5919, 0, ReleaseRings.Codes.ALP);
			EDISecurityCheckpoints.OrgLicenceModifySendUpgradeToDBNotOnLowestSupportedSqlVersion.IsAllowed = false;
			var minimumRequireGeneration = SqlServerVersionNumber.SupportedVersions.Min().Generation;

			TestLicDB.LD_SQLVersion = "Other";
			TestRequest.SetSelectedReleaseBuild(bumpingDateBuild);
			AssertNoErrors(TestRequest.SqlVersionInfo);
			AssertNoWarnings(TestRequest.SqlVersionInfo);

			TestLicDB.LD_SQLVersion = "Sql2008";
			TestRequest.SetSelectedReleaseBuild(bumpingDateBuild);
			AssertNoErrors(TestRequest.SqlVersionInfo);
			AssertNoWarnings(TestRequest.SqlVersionInfo);

			TestRequest.SetSelectedReleaseBuild(oldBuild);
			AssertNoNotifications(TestRequest.SqlVersionInfo);

			TestLicDB.LD_SQLVersion = minimumRequireGeneration.Name;
			TestRequest.SetSelectedReleaseBuild(bumpingDateBuild);
			AssertNoNotifications(TestRequest.SqlVersionInfo);

			EDISecurityCheckpoints.OrgLicenceModifySendUpgradeToDBNotOnLowestSupportedSqlVersion.IsAllowed = true;
			TestLicDB.LD_SQLVersion = "Sql2008";
			TestRequest.SetSelectedReleaseBuild(bumpingDateBuild);
			AssertNoErrors(TestRequest.SqlVersionInfo);
			AssertNoWarnings(TestRequest.SqlVersionInfo);
		}

		public void TestValidateClientContactPK()
		{
			TestRequest.ClientContactPK = ZGuid.NewZGuid();
			AssertHasError(TestRequest.ClientContactPKInfo, "Enter a valid code.");

			TestRequest.ClientContactPK = ZGuid.Invalid;
			AssertHasError(TestRequest.ClientContactPKInfo, "Enter a valid code.");

			TestRequest.ClientContactPK = ZGuid.Empty;
			AssertNoErrors(TestRequest.ClientContactPKInfo);

			TestRequest.ClientContactPK = TestOrg.Contacts[0].PK;
			AssertNoErrors(TestRequest.ClientContactPKInfo);

			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			TestRequest.ClientContactPK = contact.PK;
			AssertNoErrors(TestRequest.ClientContactPKInfo);
		}

		public void TestSqlVersion()
		{
			TestLicDB.LD_SQLVersion = "SQL2000";
			AssertEquals("SqlVersion", "SQL2000", TestRequest.SqlVersion);
			TestLicDB.LD_SQLVersion = "SQL2005";
			AssertEquals("SqlVersion", "SQL2005", TestRequest.SqlVersion);
			TestLicDB.LD_SQLVersion = "SQL2008";
			AssertEquals("SqlVersion", "SQL2008", TestRequest.SqlVersion);
			TestLicDB.LD_SQLVersion = "SQL2012";
			AssertEquals("SqlVersion", "SQL2012", TestRequest.SqlVersion);
		}

		public void TestDatabaseHasMultipleEnterprises()
		{
			LicenceDatabase database = TestOrg.LicCompany.LicDatabases[0];
			LicenceEnterprise enterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			database.LD_LE = enterprise2.PK;
			AssertNotEquals(testOrg.LicCompany.LC_LE, enterprise2.PK);
			UpgradeRequest request = new UpgradeRequest(Factory, TestOrg, database);
			AssertHasRowError(request, "Database is linked to multiple enterprises and must not be upgraded until this is fixed");

			database.LD_LE = testOrg.LicCompany.LC_LE;
			request = new UpgradeRequest(Factory, TestOrg, database);
			AssertNoRowErrors(request);
		}

		public void TestClientContacts()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Contact A11";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Contact A22";
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicDatabases.AddNew();
			Factory.Save();

			AssertEquals("Pre-condition", 2, org.Contacts.Count);

			var upgrader = new UpgradeRequestCollectionContainer(new BusinessObjectFactory(), org);
			upgrader.Upgrades[0].ClientContacts.RemoveAll();
			Factory.Save();
			AssertEquals("Org contacts collection should have no changes", 2, org.Contacts.Count);
		}

		#region Implementation

		#region Assertion

		protected void AssertSupportedMethods(CodeDescriptionPairList supportedList, params string[] methodsToAssert)
		{
			AssertEquals("Number of entries in Supported list", methodsToAssert.Length, supportedList.Count);

			foreach (string methodCode in methodsToAssert)
			{
				Assert("Contains code:" + methodCode, supportedList.ContainsCode(methodCode));
			}
		}

		protected void SetDesiredAndAssertActualUpgradeMethod(string desiredMethod, string actualMethod)
		{
			TestRequest.SetBestSupportedUpgradeMethod(desiredMethod);
			AssertEquals("Schould be " + actualMethod + " upgrade method", actualMethod, TestRequest.SupportedUpgradeMethod);
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UpgradeRequest(Factory, TestOrg, TestLicDB);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testOrg = null;
			fTestRequest = null;
		}

		#endregion

		#region TestData

		protected EDIOrgHeader TestOrg
		{
			get
			{
				if (testOrg == null)
				{
					testOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
					testOrg.CreateAndLoadLicenceForOrg();
					testOrg.LicCompany.LicDatabases.AddNew();
					testOrg.Contacts.AddNew();
					testOrg.Contacts[0].OC_ContactName = "Test Contact";
					testOrg.Contacts[0].OC_Email = "test.contact@edi.com";
				}
				return testOrg;
			}
		}

		EDIOrgHeader testOrg;

		protected UpgradeRequest TestRequest
		{
			get
			{
				if (fTestRequest == null)
				{
					fTestRequest = new UpgradeRequest(Factory, TestOrg, TestLicDB);
				}
				return fTestRequest;
			}
		}

		UpgradeRequest fTestRequest;

		protected LicenceDatabase TestLicDB
		{
			get { return TestOrg.LicCompany.LicHeadersForAllDatabases[0].Database; }
		}

		protected ReleaseBuild TestEWABuild
		{
			get
			{
				if (testEWABuild == null)
				{
					testEWABuild = CreateReleaseBuild(1, 1, 1938, 0);
				}
				return testEWABuild;
			}
		}

		ReleaseBuild testEWABuild;

		protected ReleaseBuild TestHttpBuild
		{
			get
			{
				if (testHttpBuild == null)
				{
					FirstSupportingVersion supportingVersion = new HttpDownload();
					testHttpBuild = CreateReleaseBuild(supportingVersion.VersionMajorNumber, supportingVersion.VersionMinorNumber, supportingVersion.VersionReleaseNumber, 0);
				}
				return testHttpBuild;
			}
		}

		ReleaseBuild testHttpBuild;

		protected ReleaseBuild TestHttpOnlyBuild
		{
			get
			{
				if (testHttpOnlyBuild == null)
				{
					EDIDataRegistry.Instance.AllSystemMessagesViaEhubReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "16.10.20.0");
					testHttpOnlyBuild = CreateReleaseBuild(16, 10, 20, 0);
				}
				return testHttpOnlyBuild;
			}
		}

		ReleaseBuild testHttpOnlyBuild;

		protected OrgContact TestContact
		{
			get { return TestOrg.Contacts[0]; }
		}

		#endregion

		ReleaseBuild CreateReleaseBuild(DateTime date, string releaseStatus)
		{
			return CreateReleaseBuild(date.Year - 2000, date.Month, date.Day, 0, releaseStatus);
		}

		ReleaseBuild CreateReleaseBuild(int majorVersion, int minorVersion, int release, int patch)
		{
			return CreateReleaseBuild(majorVersion, minorVersion, release, patch, string.Empty);
		}

		ReleaseBuild CreateReleaseBuild(int majorVersion, int minorVersion, int release, int patch, string releaseStatus)
		{
			ReleaseBuild result = Factory.NewWithValidTestData<ReleaseBuild>();
			result.HL_MajorVersion = majorVersion;
			result.HL_MinorVersion = minorVersion;
			result.HL_Release = release;
			result.HL_Patch = patch;
			result.HL_ReleaseStatus = releaseStatus;
			return result;
		}

		#endregion
	}
}
