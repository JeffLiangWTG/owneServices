using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.AutoDeploy.Business.Test
{
	[TestedType(typeof(UpgradesToClient))]
	public class UpgradesToClientTest : EnterpriseBusinessObjectTestCase
	{
		#region Related BO and Properties

		[TestDate(2006, 04, 11, 9, 59, 00)]
		public void TestCurrentStatuschangeNotification()
		{
			EDIOrgHeader testHeader = HeaderForTest;

			UpgradesToClient testUpgrade = UpgradeToClientForTest;
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			GlbStaff user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@edi.com.au";
			testUpgrade.L1_LD = database.PK;
			testUpgrade.L1_GS_NKStaffCode = user.GS_Code;
			testUpgrade.L1_HL = build.PK;

			Env.OutgoingMailManager.EmailsCreated.Clear();
			testUpgrade.L1_CurrentStatus = UpgradesToClientStatus.Codes.Blocked;

			AssertEquals("NotificationHandler Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email1 = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("One To Recipient", 1, email1.Recipients.Count);
			AssertEquals("Recipient", "test@edi.com.au", email1.Recipients[0]);
			AssertEquals("Email Subject", "Blocked Upgrade notification", email1.Subject);
			string expectedEmailBody = string.Format(CultureInfo.CurrentCulture, "Sending upgrade {0} to the client {1}, server {2} using {3} was blocked at {4}",
				build.PackageName, testUpgrade.EnterpriseCode, database.LD_ServerCode, testUpgrade.L1_ActualUpgradeMethod, ZDateTime.Now);
			AssertEquals("Message Body", expectedEmailBody, email1.Body);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			testUpgrade.L1_CurrentStatus = UpgradesToClientStatus.Codes.Failed;

			AssertEquals("NotificationHandler Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email2 = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("One To Recipient", 1, email2.Recipients.Count);
			AssertEquals("Recipient", "test@edi.com.au", email2.Recipients[0]);
			AssertEquals("Email Subject", "Upgrade delivery failure notification", email2.Subject);
			expectedEmailBody = string.Format(CultureInfo.CurrentCulture, "Sending upgrade {0} to the client {1}, server {2} using {3} failed at {4}",
				build.PackageName, testUpgrade.EnterpriseCode, database.LD_ServerCode, testUpgrade.L1_ActualUpgradeMethod, ZDateTime.Now);
			AssertEquals("Message Body", expectedEmailBody, email2.Body);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			testUpgrade.L1_CurrentStatus = UpgradesToClientStatus.Codes.Received;

			AssertEquals("NotificationHandler Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email3 = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("One To Recipient", 1, email3.Recipients.Count);
			AssertEquals("Recipient", "test@edi.com.au", email3.Recipients[0]);
			AssertEquals("Email Subject", "Upgrade delivery notification", email3.Subject);
			expectedEmailBody = string.Format(CultureInfo.CurrentCulture, "Upgrade {0} was successfully delivered to the client {1}, server {2} using {3} at {4}",
				build.PackageName, testUpgrade.EnterpriseCode, database.LD_ServerCode, testUpgrade.L1_ActualUpgradeMethod, ZDateTime.Now);
			AssertEquals("Message Body", expectedEmailBody, email3.Body);
		}

		public void TestRelatedBusinessObjectsProperties()
		{
			EDIOrgHeader testHeader = HeaderForTest;

			ReleaseBuild build = Factory.New<ReleaseBuild>();

			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			OrgContact contact = testHeader.Contacts.AddNew();

			UpgradesToClient testUpgrade = UpgradeToClientForTest;
			testUpgrade.L1_HL = build.PK;
			testUpgrade.L1_LD = database.PK;
			testUpgrade.L1_OC = contact.PK;

			AssertEquals("Build PK", build.PK, testUpgrade.L1_HL);
			AssertEquals("LicenceDatabase PK", database.PK, testUpgrade.L1_LD);
			AssertEquals("OrgHeader PK", testHeader.PK, testUpgrade.L1_OH);
			AssertEquals("Contact PK", contact.PK, testUpgrade.L1_OC);

			AssertEquals("Build", build, testUpgrade.Build);
			AssertEquals("LicDatabase", database, testUpgrade.LicDatabase);
			AssertEquals("Organisation", testHeader, testUpgrade.Organisation);
			AssertEquals("Contact", contact, testUpgrade.Contact);
			AssertEquals("Actual UpgradeMethod is the highest supported", UpgradeMethods.Codes.Blocked, testUpgrade.L1_ActualUpgradeMethod);
		}

		public void TestActualUpgradeMethodNoEmail()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();

			UpgradesToClient testUpgrade = UpgradeToClientForTest;

			testUpgrade.L1_LD = database.PK;

			AssertEquals("Requested UpgradeMethod is empty", "", testUpgrade.L1_RequestedUpgradeMethod);
			AssertEquals("Actual UpgradeMethod is the highest supported (Blocked)", UpgradeMethods.Codes.Blocked, testUpgrade.L1_ActualUpgradeMethod);

			testUpgrade.L1_RequestedUpgradeMethod = UpgradeMethods.Codes.Http;
			AssertEquals("Actual UpgradeMethod is the highest supported (Blocked)", UpgradeMethods.Codes.Blocked, testUpgrade.L1_ActualUpgradeMethod);
		}

		public void TestActualUpgradeMethodHasEmailVersionWithACK()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			database.LD_PublicEmailAddressForUpdate = "test@edi.com.au";

			ReleaseBuild build = BuildForTest;
			database.LD_HL_CurrentRunningVersion = build.PK;

			UpgradesToClient testUpgrade = UpgradeToClientForTest;

			testUpgrade.L1_LD = database.PK;

			AssertEquals("Requested UpgradeMethod is empty", "", testUpgrade.L1_RequestedUpgradeMethod);
			AssertEquals("Actual UpgradeMethod is the highest supported", UpgradeMethods.Codes.Blocked, testUpgrade.L1_ActualUpgradeMethod);

			testUpgrade.L1_RequestedUpgradeMethod = UpgradeMethods.Codes.Http;
			AssertEquals("Actual UpgradeMethod is the highest supported", UpgradeMethods.Codes.Blocked, testUpgrade.L1_ActualUpgradeMethod);
		}

		public void TestActualUpgradeMethodHasEmailVersionSupportsHttp()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			database.LD_PublicEmailAddressForUpdate = "test@edi.com.au";

			ReleaseBuild build = BuildForTest;
			HttpDownload httpVersion = new HttpDownload();
			build.HL_MajorVersion = httpVersion.VersionMajorNumber;
			build.HL_MinorVersion = httpVersion.VersionMinorNumber;
			build.HL_Release = httpVersion.VersionReleaseNumber;
			database.LD_HL_CurrentRunningVersion = build.PK;

			UpgradesToClient testUpgrade = UpgradeToClientForTest;

			testUpgrade.L1_LD = database.PK;

			AssertEquals("Requested UpgradeMethod is empty", "", testUpgrade.L1_RequestedUpgradeMethod);
			AssertEquals("Actual UpgradeMethod is the highest supported (HTP)", UpgradeMethods.Codes.Http, testUpgrade.L1_ActualUpgradeMethod);

			testUpgrade.L1_RequestedUpgradeMethod = UpgradeMethods.Codes.Http;
			AssertEquals("Actual UpgradeMethod is the highest supported (HTP)", UpgradeMethods.Codes.Http, testUpgrade.L1_ActualUpgradeMethod);
		}

		public void TestNewProperties()
		{
			EDIOrgHeader testHeader = HeaderForTest;

			var build = Factory.New<MockReleaseBuild>();
			build.SetClientSpecificCodes();

			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();

			UpgradesToClient testUpgrade = UpgradeToClientForTest;

			testUpgrade.L1_HL = build.PK;
			testUpgrade.L1_LD = database.PK;

			AssertEquals("Build PK", build.PK, testUpgrade.L1_HL);
			AssertEquals("LicenceDatabase PK", database.PK, testUpgrade.L1_LD);
			AssertEquals("OrgHeader PK", testHeader.PK, testUpgrade.L1_OH);

			AssertEquals("Enterprise Code", "TGB", testUpgrade.EnterpriseCode);

			testUpgrade.LicDatabase.LicHeadersForAllCompanies[0].Company.LicEnterprise.LE_EnterpriseCode = "ABC";
			AssertEquals("Enterprise Code", "ABC", testUpgrade.EnterpriseCode);
			AssertEquals("Client Specific Code", "", testUpgrade.ClientSpecificCode);
		}

		public void TestClientSpecificCode()
		{
			HeaderForTest.LicenceEnterpriseCode = "ABC";
			LicenceDatabase database = HeaderForTest.LicCompany.LicDatabases.AddNew();
			var build = Factory.New<MockReleaseBuild>();
			build.HL_ReleaseStatus = ReleaseRings.Codes.ALP;
			build.SetClientSpecificCodes();

			UpgradeToClientForTest.L1_LD = database.PK;
			UpgradeToClientForTest.L1_HL = build.PK;
			AssertEquals("ClientSpecificCode", "", UpgradeToClientForTest.ClientSpecificCode);

			build.HL_ReleaseStatus = ReleaseRings.Codes.GPR;
			AssertEquals("ClientSpecificCode", "", UpgradeToClientForTest.ClientSpecificCode);

			MockReleaseBuild mockBuild = Factory.New<MockReleaseBuild>();
			mockBuild.HL_ReleaseStatus = ReleaseRings.Codes.GPR;
			mockBuild.SetClientSpecificCodes("ABC");
			UpgradeToClientForTest.L1_HL = mockBuild.PK;
			AssertEquals("ClientSpecificCode", "ABC", UpgradeToClientForTest.ClientSpecificCode);
		}

		#endregion

		#region Default Values

		public void TestDefaultValuesSet()
		{
			UpgradesToClient testUpgrade = UpgradeToClientForTest;
			AssertEquals("EnterpriseCode is empty string", ZString.Empty, testUpgrade.EnterpriseCode);
			AssertEquals("ClientSpecificCode is empty string", ZString.Empty, testUpgrade.ClientSpecificCode);
			AssertNull("LicDatabase is null", testUpgrade.LicDatabase);
			AssertNull("Organisation is null", testUpgrade.Organisation);
		}

		#endregion

		#region CompareTo

		public void TestCompareToAndCouldBeGroupedWith()
		{
			EDIOrgHeader testHeader = HeaderForTest;

			var build = Factory.New<MockReleaseBuild>();
			build.SetClientSpecificCodes();
			HttpDownload httpVersion = new HttpDownload();
			build.HL_MajorVersion = httpVersion.VersionMajorNumber;
			build.HL_MinorVersion = httpVersion.VersionMinorNumber;
			build.HL_Release = httpVersion.VersionReleaseNumber;
			build.HL_ExeVersionDate = ZDateTime.Now.AddDays(-1);

			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			database.LD_PublicEmailAddressForUpdate = "test@edi.com.au";
			database.LD_HL_CurrentRunningVersion = build.PK;

			UpgradesToClient testUpgrade = UpgradeToClientForTest;
			testUpgrade.L1_HL = build.PK;
			testUpgrade.L1_LD = database.PK;
			testUpgrade.L1_RequestedDateTime = ZDateTime.Now;

			AssertEquals("Equals to itself", 0, testUpgrade.CompareTo(testUpgrade));
			Assert("Could be grouped with itself", testUpgrade.CouldBeGroupedWith(testUpgrade));

			UpgradesToClient testCompare = Factory.New<UpgradesToClient>();
			testCompare.L1_HL = build.PK;
			testCompare.L1_LD = database.PK;
			testCompare.L1_RequestedDateTime = testUpgrade.L1_RequestedDateTime;

			AssertEquals("Equals", 0, testUpgrade.CompareTo(testCompare));
			AssertEquals("Equals", 0, testCompare.CompareTo(testUpgrade));
			Assert("Could be grouped with", testUpgrade.CouldBeGroupedWith(testCompare));

			testCompare.L1_CurrentStatus = UpgradesToClientStatus.Codes.Blocked;

			AssertEquals("Status less", -1, testUpgrade.CompareTo(testCompare));
			AssertEquals("Status great", 1, testCompare.CompareTo(testUpgrade));
			AssertEquals("Could not be grouped with", false, testUpgrade.CouldBeGroupedWith(testCompare));

			testCompare.L1_CurrentStatus = testUpgrade.L1_CurrentStatus;
			AssertEquals("Equals", 0, testUpgrade.CompareTo(testCompare));
			AssertEquals("Equals", 0, testCompare.CompareTo(testUpgrade));
			Assert("Could be grouped with", testUpgrade.CouldBeGroupedWith(testCompare));

			ReleaseBuild compareBuild = Factory.New<ReleaseBuild>();
			compareBuild.HL_Release = build.HL_Release;
			compareBuild.HL_ExeVersionDate = ZDateTime.Now;
			testCompare.L1_HL = compareBuild.PK;

			AssertEquals("ExeDate less", -1, testUpgrade.CompareTo(testCompare));
			AssertEquals("ExeDate great", 1, testCompare.CompareTo(testUpgrade));
			AssertEquals("Could not be grouped with", false, testUpgrade.CouldBeGroupedWith(testCompare));

			testCompare.L1_HL = build.PK;
			AssertEquals("Equals", 0, testUpgrade.CompareTo(testCompare));
			AssertEquals("Equals", 0, testCompare.CompareTo(testUpgrade));
			Assert("Could be grouped with", testUpgrade.CouldBeGroupedWith(testCompare));

			EDIOrgHeader compareHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			compareHeader.OH_RL_NKClosestPort = "AUBNE";
			compareHeader.OH_FullName = "EDI Compare Organisation";
			compareHeader.OH_Code = "EDIBNE";
			compareHeader.CreateAndLoadLicenceForOrg();
			compareHeader.GenerateNewLicenceCode();
			LicenceDatabase compareDatabase = compareHeader.LicCompany.LicDatabases.AddNew();
			compareDatabase.LD_HL_CurrentRunningVersion = build.PK;
			testCompare.L1_LD = compareDatabase.PK;

			AssertEquals("ClientSpecific less", 1, testUpgrade.CompareTo(testCompare));
			AssertEquals("ClientSpecific great", -1, testCompare.CompareTo(testUpgrade));
			AssertEquals("Could not be grouped with", false, testUpgrade.CouldBeGroupedWith(testCompare));

			var originalCompareEnterpriseCode = testCompare.Organisation.LicEnterprise.LE_EnterpriseCode;
			testCompare.Organisation.LicEnterprise.LE_EnterpriseCode = testUpgrade.EnterpriseCode;
			testCompare.L1_ActualUpgradeMethod = testUpgrade.L1_ActualUpgradeMethod;
			AssertEquals("Precondition", 0, testUpgrade.CompareTo(testCompare));

			testCompare.Organisation.LicEnterprise.LE_EnterpriseCode = originalCompareEnterpriseCode;

			testCompare.L1_LD = database.PK;
			AssertEquals("Equals", 0, testUpgrade.CompareTo(testCompare));
			AssertEquals("Equals", 0, testCompare.CompareTo(testUpgrade));
			Assert("Could be grouped with", testUpgrade.CouldBeGroupedWith(testCompare));

			testCompare.L1_RequestedUpgradeMethod = UpgradeMethods.Codes.Http;
			testUpgrade.L1_RequestedUpgradeMethod = UpgradeMethods.Codes.Blocked;
			AssertEquals("HTP", UpgradeMethods.Codes.Http, testCompare.L1_ActualUpgradeMethod);

			AssertEquals("UpgradeMethod less", -1, testUpgrade.CompareTo(testCompare));
			AssertEquals("Upgrademethod great", 1, testCompare.CompareTo(testUpgrade));
			AssertEquals("Could not be grouped with", false, testUpgrade.CouldBeGroupedWith(testCompare));

			testUpgrade.L1_RequestedUpgradeMethod = UpgradeMethods.Codes.Http;
			testCompare.L1_RequestedUpgradeMethod = testUpgrade.L1_RequestedUpgradeMethod;
			AssertEquals("Equals", 0, testUpgrade.CompareTo(testCompare));
			AssertEquals("Equals", 0, testCompare.CompareTo(testUpgrade));
			Assert("Could be grouped with", testUpgrade.CouldBeGroupedWith(testCompare));

			testCompare.L1_RequestedDateTime = testUpgrade.L1_RequestedDateTime.AddMinutes(10);

			AssertEquals("RequestedTime less", -1, testUpgrade.CompareTo(testCompare));
			AssertEquals("RequestedTime great", 1, testCompare.CompareTo(testUpgrade));
			Assert("Could be grouped with", testUpgrade.CouldBeGroupedWith(testCompare));
		}

		public void TestCompareTo_ComparesClientSpecificCode_ClientSpecificAndGeneric()
		{
			var org1Code = "ABC";
			var testHeader1 = HeaderForTest;
			testHeader1.LicCompany.LicEnterprise.LE_EnterpriseCode = org1Code;
			var database1 = testHeader1.LicCompany.LicDatabases.AddNew();

			var org2Code = "DEF";
			var testHeader2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			testHeader2.CreateAndLoadLicenceForOrg();
			testHeader2.GenerateNewLicenceCode();
			testHeader2.LicCompany.LicEnterprise.LE_EnterpriseCode = org2Code;
			var database2 = testHeader2.LicCompany.LicDatabases.AddNew();

			var build = Factory.New<MockReleaseBuild>();
			build.SetClientSpecificCodes(org1Code);

			var testUpgradeDatabase1 = UpgradeToClientForTest;
			testUpgradeDatabase1.L1_HL = build.PK;
			testUpgradeDatabase1.L1_LD = database1.PK;

			var testUpgradeDatabase2 = Factory.New<UpgradesToClient>();
			testUpgradeDatabase2.L1_HL = build.PK;
			testUpgradeDatabase2.L1_LD = database2.PK;

			AssertEquals("Not Equals", 1, testUpgradeDatabase1.CompareTo(testUpgradeDatabase2));
			AssertEquals("Not Equals", -1, testUpgradeDatabase2.CompareTo(testUpgradeDatabase1));
			AssertEquals("Database 1 is client specific; database 2 is generic; cannot be grouped", false, testUpgradeDatabase1.CouldBeGroupedWith(testUpgradeDatabase2));
		}

		public void TestCompareTo_ComparesClientSpecificCode_DifferentClientSpecificCodes()
		{
			var org1Code = "ABC";
			var testHeader1 = HeaderForTest;
			testHeader1.LicCompany.LicEnterprise.LE_EnterpriseCode = org1Code;
			var database1 = testHeader1.LicCompany.LicDatabases.AddNew();

			var org2Code = "DEF";
			var testHeader2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			testHeader2.CreateAndLoadLicenceForOrg();
			testHeader2.GenerateNewLicenceCode();
			testHeader2.LicCompany.LicEnterprise.LE_EnterpriseCode = org2Code;
			var database2 = testHeader2.LicCompany.LicDatabases.AddNew();

			var build = Factory.New<MockReleaseBuild>();
			build.SetClientSpecificCodes(org1Code, org2Code);

			var testUpgradeDatabase1 = UpgradeToClientForTest;
			testUpgradeDatabase1.L1_HL = build.PK;
			testUpgradeDatabase1.L1_LD = database1.PK;

			var testUpgradeDatabase2 = Factory.New<UpgradesToClient>();
			testUpgradeDatabase2.L1_HL = build.PK;
			testUpgradeDatabase2.L1_LD = database2.PK;

			AssertEquals("Not Equals", -1, testUpgradeDatabase1.CompareTo(testUpgradeDatabase2));
			AssertEquals("Not Equals", 1, testUpgradeDatabase2.CompareTo(testUpgradeDatabase1));
			AssertEquals("Database 1 is client specific; database 2 is different client specific; cannot be grouped", false, testUpgradeDatabase1.CouldBeGroupedWith(testUpgradeDatabase2));
		}

		public void TestCompareTo_ComparesClientSpecificCode_SameClientSpecificCode()
		{
			var org1Code = "ABC";
			var testHeader1 = HeaderForTest;
			testHeader1.LicCompany.LicEnterprise.LE_EnterpriseCode = org1Code;
			var database1 = testHeader1.LicCompany.LicDatabases.AddNew();
			var database2 = testHeader1.LicCompany.LicDatabases.AddNew();

			var build = Factory.New<MockReleaseBuild>();
			build.SetClientSpecificCodes(org1Code);

			var testUpgradeDatabase1 = UpgradeToClientForTest;
			testUpgradeDatabase1.L1_HL = build.PK;
			testUpgradeDatabase1.L1_LD = database1.PK;

			var testUpgradeDatabase2 = Factory.New<UpgradesToClient>();
			testUpgradeDatabase2.L1_HL = build.PK;
			testUpgradeDatabase2.L1_LD = database2.PK;

			AssertEquals("Equals", 0, testUpgradeDatabase1.CompareTo(testUpgradeDatabase2));
			AssertEquals("Equals", 0, testUpgradeDatabase2.CompareTo(testUpgradeDatabase1));
			Assert("Database 1 and 2 are same client specific; can be grouped", testUpgradeDatabase1.CouldBeGroupedWith(testUpgradeDatabase2));
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();

			ReleaseBuild build = Factory.New<ReleaseBuild>();

			OrgContact contact = testHeader.Contacts.AddNew();

			UpgradesToClient testUpgrade = UpgradeToClientForTest;
			testUpgrade.L1_HL = build.PK;
			testUpgrade.L1_LD = database.PK;
			testUpgrade.L1_OC = contact.PK;

			return testUpgrade;
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

					headerForTest.CreateAndLoadLicenceForOrg();
					headerForTest.GenerateNewLicenceCode();
				}

				return headerForTest;
			}
		}
		EDIOrgHeader headerForTest;

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

		UpgradesToClient UpgradeToClientForTest
		{
			get
			{
				if (upgradeToClientForTest == null)
				{
					upgradeToClientForTest = Factory.New<UpgradesToClient>();
				}
				return upgradeToClientForTest;
			}
		}

		UpgradesToClient upgradeToClientForTest;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<UpgradesToClient>();
		}

		#endregion
	}
}
