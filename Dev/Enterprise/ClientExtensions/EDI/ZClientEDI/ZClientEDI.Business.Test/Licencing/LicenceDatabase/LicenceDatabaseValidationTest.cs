using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	internal class LicenceDatabaseValidationTest : BusinessObjectValidationTestCase
	{
		public void TestServerCode()
		{
			EDIOrgHeader testHeader = Factory.New<EDIOrgHeader>();
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase testDatabase = testHeader.LicCompany.LicDatabases.AddNew();
			testDatabase.LD_ServerCode = "XDB";

			LicenceDatabase dup1 = testHeader.LicCompany.LicEnterprise.Databases.AddNew();
			dup1.LD_ServerCode = "XDB";
			AssertHasError("A database with the same code already exists on the enterprise", dup1.LD_ServerCodeInfo, "A database with this code already exists for this client.");

			LicenceDatabase dup3 = testHeader.LicCompany.LicDatabases.AddNew();
			dup3.LD_ServerCode = "A";
			AssertHasError("Code must be at least 3 characters", dup3.LD_ServerCodeInfo, "The Server Code must be exactly 3 characters in length");
		}

		public void TestServerCode_BOR()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var db = lic.Database;
			var bor = BillingTestHelper.CreateAnotherDatabase(lic, "BBB").Database;
			bor.LD_Product = "BOR";
			Factory.Save();

			bor.RunPreSaveValidation();
			AssertNoWarnings(bor.LD_ServerCodeInfo);

			BillingTestHelper.CreateAnotherDatabase(lic, "CCC");
			Factory.Save();
			bor.RunPreSaveValidation();
			AssertHasWarning(bor.LD_ServerCodeInfo, "The client Enterprise has multiple CargoWise licenses. Ensure the BOR License is unique for each company under this enterprise code.");

			bor.LD_IsActive = false;
			Factory.Save();
			bor.RunPreSaveValidation();
			AssertNoWarnings(bor.LD_ServerCodeInfo);
		}

		public void TestLicenceType()
		{
			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			LicenceDatabase database = enterprise.Databases.AddNew();

			database.LD_LicenceType = "???";
			AssertHasErrors(database.LD_LicenceTypeInfo);
			AssertNoWarnings(database.LD_LicenceTypeInfo);

			database.LD_LicenceType = database.Lookups.DatabaseTypesList[0].Code;
			AssertNoErrors(database.LD_LicenceTypeInfo);
			AssertNoWarnings(database.LD_LicenceTypeInfo);

			database.LD_LicenceType = "";
			AssertHasError(database.LD_LicenceTypeInfo, "Please enter a License Type.");
		}

		public void TestDatabaseSecurityMode()
		{
			var database = Factory.NewWithValidTestData<LicenceDatabase>();

			database.LD_DBServerSecurityMode = "???";
			AssertHasErrors(database.LD_DBServerSecurityModeInfo);

			database.LD_DBServerSecurityMode = database.Lookups.DatabaseSecurityModesList[0].Code;
			AssertNoErrors(database.LD_DBServerSecurityModeInfo);

			database.LD_DBServerSecurityMode = ZString.Empty;
			AssertHasErrors(database.LD_DBServerSecurityModeInfo);

			database.LD_DBServerSecurityMode = DatabaseSecurityModePairList.Codes.Locked;
			Factory.Save();
			EDISecurityCheckpoints.OrgLicenceModifySetDatabaseSecurityModeToOpen.IsAllowed = false;
			database.Validation.ValidateLD_DBServerSecurityMode();
			AssertNoErrors("ServerSecurityMode has not change, validation should not add an error", database.LD_DBServerSecurityModeInfo);

			database.LD_DBServerSecurityMode = DatabaseSecurityModePairList.Codes.OpenMode;
			AssertHasErrorContaining(database.LD_DBServerSecurityModeInfo, "do not have rights to set server security mode to open");

			EDISecurityCheckpoints.OrgLicenceModifySetDatabaseSecurityModeToOpen.IsAllowed = true;
			database.Validation.ValidateLD_DBServerSecurityMode();
			AssertHasWarningContaining(database.LD_DBServerSecurityModeInfo, "requires management approval");

			database.LD_DBServerSecurityMode = DatabaseSecurityModePairList.Codes.ExOpen;
			AssertHasErrorContaining(database.LD_DBServerSecurityModeInfo, "Cannot set security to deprecated open-before-SQL2012 mode");
		}

		public void TestEmailAddresses()
		{
			EDIDataRegistry.Instance.AllSystemMessagesViaEhubReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "16.10.20.0");
			var httpOnlyBuild = CreateReleaseBuild(16, 10, 20, 0, ReleaseRings.Codes.ALP);

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var db = lic.Database;
			var db2 = BillingTestHelper.CreateAnotherDatabase(lic, "BBB").Database;
			db2.LD_PublicEmailAddressForUpdate = "alexander.korotun@cargowise.com";
			Factory.Save();

			db.LD_PublicEmailAddressForUpdate = "";
			AssertNoErrors("Blank is allowed", db.LD_PublicEmailAddressForUpdateInfo);

			db.LD_PublicEmailAddressForUpdate = "blah";
			AssertHasErrors("Invalid - has errors", db.LD_PublicEmailAddressForUpdateInfo);

			db.LD_PublicEmailAddressForUpdate = "blah@me.com";
			AssertNoErrors("Valid email", db.LD_PublicEmailAddressForUpdateInfo);

			db.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			db.LD_PublicEmailAddressForUpdate = "";
			AssertHasWarning(db.LD_PublicEmailAddressForUpdateInfo, "If you do not populate this field, you will not be able to deploy an upgrade to this database through ediProd.");

			db.LD_HL_CurrentRunningVersion = httpOnlyBuild.PK;
			db.Validation.ValidateLD_PublicEmailAddressForUpdate();
			AssertNoNotifications(db.LD_PublicEmailAddressForUpdateInfo);

			db.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			db.Validation.ValidateLD_PublicEmailAddressForUpdate();
			AssertNoWarnings(db.LD_PublicEmailAddressForUpdateInfo);

			db.LD_AvailableUpgradeMethod = "";
			db.Validation.ValidateLD_PublicEmailAddressForUpdate();
			AssertNoWarnings(db.LD_PublicEmailAddressForUpdateInfo);

			db.LD_PublicEmailAddressForUpdate = "alexander.korotun@cargowise.com";
			AssertHasError("A database with the same email already exists on the enterprise", db.LD_PublicEmailAddressForUpdateInfo, "A database with this email already exists for this client.");
		}

		public void TestUpgradeMethodBlankEmail()
		{
			EDIDataRegistry.Instance.AllSystemMessagesViaEhubReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "16.10.20.0");
			var legacyVersion = new HttpDownload();
			var httpLegacyBuild = CreateReleaseBuild(legacyVersion.VersionMajorNumber, legacyVersion.VersionMinorNumber, legacyVersion.VersionReleaseNumber, 0, ReleaseRings.Codes.ALP);
			var httpOnlyBuild = CreateReleaseBuild(16, 10, 20, 0, ReleaseRings.Codes.ALP);

			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			LicenceDatabase database = enterprise.Databases.AddNew();

			database.LD_HL_CurrentRunningVersion = httpLegacyBuild.PK;
			database.LD_PublicEmailAddressForUpdateInfo.Value = new ZString("");
			database.LD_AvailableUpgradeMethodInfo.Value = new ZString("");
			AssertNoErrors("No errors when everything is empty", database.LD_AvailableUpgradeMethodInfo);

			database.LD_AvailableUpgradeMethodInfo.Value = new ZString(UpgradeMethods.Codes.Http);
			AssertHasError(database.LD_AvailableUpgradeMethodInfo, "Upgrade Method should be empty or Blocked when Email Address for Upgrades is empty and Current Version is before November 2016.");

			database.LD_AvailableUpgradeMethodInfo.Value = new ZString(UpgradeMethods.Codes.Blocked);
			AssertNoErrors(database.LD_AvailableUpgradeMethodInfo);

			database.LD_HL_CurrentRunningVersion = httpOnlyBuild.PK;
			database.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			AssertNoNotifications(database.LD_AvailableUpgradeMethodInfo);
		}

		public void TestUpgradeMethodHttp()
		{
			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			LicenceDatabase database = enterprise.Databases.AddNew();
			database.LD_PublicEmailAddressForUpdateInfo.Value = new ZString("a@b.c");

			database.LD_AvailableUpgradeMethodInfo.Value = new ZString(UpgradeMethods.Codes.Http);
			AssertHasWarning(database.LD_AvailableUpgradeMethodInfo, "Selected Http Download Upgrade Method is not supported by Current Version of the client's system.");

			HttpDownload version = new HttpDownload();
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.CurrentVersion.HL_MajorVersion = version.VersionMajorNumber;
			database.CurrentVersion.HL_MinorVersion = version.VersionMinorNumber;
			database.CurrentVersion.HL_Release = version.VersionReleaseNumber - 1;
			database.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			AssertHasWarning(database.LD_AvailableUpgradeMethodInfo, "Selected Http Download Upgrade Method is not supported by Current Version of the client's system.");

			database.CurrentVersion.HL_Release = version.VersionReleaseNumber;
			database.Validation.ValidateLD_AvailableUpgradeMethod();
			AssertNoErrors("No errors when supporting version", database.LD_AvailableUpgradeMethodInfo);
		}

		public void TestUpgradeMethodLegacyDatabase()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();
			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			LicenceDatabase database = enterprise.Databases.AddNew();
			LicenceHeader header = database.LicHeadersForAllCompanies.AddNew();

			database.LD_Product = "BBB";
			database.LD_AvailableUpgradeMethod = "";
			AssertHasError(database.LD_AvailableUpgradeMethodInfo, "The upgrade method must be Blocked. This database product cannot have any deployment method.");

			database.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			AssertNoErrors("No errors when legacy database and upgrade method is blocked", database.LD_AvailableUpgradeMethodInfo);
		}

		public void TestValidateReleaseRing()
		{
			LicenceDatabase database = Factory.NewWithValidTestData<LicenceDatabase>();
			AssertNoErrors("Precondition: LD_ReleaseRing should not have errors.", database.LD_ReleaseRingInfo);
			Assert("Precondition: Lookups.ReleaseRings[0].Code should not be empty.", database.Lookups.ReleaseRings[0].Code.Length > 0);

			database.LD_ReleaseRing = "";
			AssertHasError(database.LD_ReleaseRingInfo, "Please enter a " + database.LD_ReleaseRingInfo.Description + ".");

			database.LD_ReleaseRing = "!";
			AssertHasError(database.LD_ReleaseRingInfo, "Enter a valid " + database.LD_ReleaseRingInfo.Description + ".");

			database.LD_ReleaseRing = database.Lookups.ReleaseRings[0].Code;
			AssertNoErrors(database.LD_ReleaseRingInfo);
		}

		public void TestValidateReleaseRing_ToHigherRing()
		{
			EDISecurityCheckpoints.OrgLicenceModifyLicDatabaseModifyToHigherRing.IsAllowed = false;

			LicenceDatabase database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_ReleaseRing = ReleaseRings.Codes.GP1;
			AssertNoErrors(database.LD_ReleaseRingInfo);

			database.LD_ReleaseRing = ReleaseRings.Codes.STD;
			AssertNoErrors(database.LD_ReleaseRingInfo);
			Factory.Save();

			database.LD_ReleaseRing = ReleaseRings.Codes.GPR;
			AssertNoErrors(database.LD_ReleaseRingInfo);

			database.LD_ReleaseRing = ReleaseRings.Codes.GP1;
			AssertNoErrors(database.LD_ReleaseRingInfo);

			database.LD_ReleaseRing = ReleaseRings.Codes.DPR;
			AssertHasError(database.LD_ReleaseRingInfo, "You do not have the permission to modify to a higher Release Ring (i.e. STD->DPR is NOT allowed, STD->GP1 is allowed)");

			database.LD_ReleaseRing = ReleaseRings.Codes.ALP;
			AssertHasError(database.LD_ReleaseRingInfo, "You do not have the permission to modify to a higher Release Ring (i.e. STD->DPR is NOT allowed, STD->GP1 is allowed)");

			EDISecurityCheckpoints.OrgLicenceModifyLicDatabaseModifyToHigherRing.IsAllowed = true;
			database.LD_ReleaseRing = ReleaseRings.Codes.DPR;
			AssertNoErrors(database.LD_ReleaseRingInfo);

			database.LD_ReleaseRing = ReleaseRings.Codes.ALP;
			AssertNoErrors(database.LD_ReleaseRingInfo);
		}

		public void TestChangeCurrentValidReleaseRingToInvalidReleaseRing()
		{
			EDISecurityCheckpoints.OrgLicenceModifyLicDatabaseModifyToHigherRing.IsAllowed = false;

			LicenceDatabase database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_ReleaseRing = ReleaseRings.Codes.GP1;
			AssertNoErrors(database.LD_ReleaseRingInfo);

			Factory.Save();

			database.LD_ReleaseRing = "ETL";
			AssertEquals("errors", 2, database.LD_ReleaseRingInfo.GetErrors().Count());
		}

		public void TestValidateReleaseRing_ToRestrictedRing()
		{
			EDISecurityCheckpoints.OrgLicenceModifyLicDatabaseModifyToRestrictedRing.IsAllowed = false;

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_ReleaseRing = ReleaseRings.Codes.DPR;
			AssertHasError(database.LD_ReleaseRingInfo, "You do not have the permission to modify to a restricted Release Ring");

			database.LD_ReleaseRing = ReleaseRings.Codes.STD;
			AssertHasError(database.LD_ReleaseRingInfo, "You do not have the permission to modify to a restricted Release Ring");

			database.LD_ReleaseRing = ReleaseRings.Codes.GPC;
			AssertHasError(database.LD_ReleaseRingInfo, "You do not have the permission to modify to a restricted Release Ring");

			database.LD_ReleaseRing = ReleaseRings.Codes.GP1;
			AssertNoErrors(database.LD_ReleaseRingInfo);

			database.LD_ReleaseRing = ReleaseRings.Codes.LPB;
			AssertHasError(database.LD_ReleaseRingInfo, "You do not have the permission to modify to a restricted Release Ring");

			database.LD_ReleaseRing = ReleaseRings.Codes.GPR;
			AssertHasError(database.LD_ReleaseRingInfo, "You do not have the permission to modify to a restricted Release Ring");

			database.LD_ReleaseRing = ReleaseRings.Codes.ALP;
			AssertHasError(database.LD_ReleaseRingInfo, "You do not have the permission to modify to a restricted Release Ring");

			database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_ReleaseRing = ReleaseRings.Codes.GP1;
			Factory.Save();

			database.LD_ReleaseRing = ReleaseRings.Codes.DPR;
			AssertHasError(database.LD_ReleaseRingInfo, "You do not have the permission to modify to a restricted Release Ring");

			database.LD_ReleaseRing = ReleaseRings.Codes.STD;
			AssertHasError(database.LD_ReleaseRingInfo, "You do not have the permission to modify to a restricted Release Ring");

			database.LD_ReleaseRing = ReleaseRings.Codes.GPC;
			AssertHasError(database.LD_ReleaseRingInfo, "You do not have the permission to modify to a restricted Release Ring");

			database.LD_ReleaseRing = ReleaseRings.Codes.GP1;
			AssertNoErrors(database.LD_ReleaseRingInfo);

			database.LD_ReleaseRing = ReleaseRings.Codes.LPB;
			AssertHasError(database.LD_ReleaseRingInfo, "You do not have the permission to modify to a restricted Release Ring");

			database.LD_ReleaseRing = ReleaseRings.Codes.GPR;
			AssertHasError(database.LD_ReleaseRingInfo, "You do not have the permission to modify to a restricted Release Ring");

			EDISecurityCheckpoints.OrgLicenceModifyLicDatabaseModifyToRestrictedRing.IsAllowed = true;
			database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_ReleaseRing = ReleaseRings.Codes.DPR;
			AssertNoErrors(database.LD_ReleaseRingInfo);
			Factory.Save();

			EDISecurityCheckpoints.OrgLicenceModifyLicDatabaseModifyToRestrictedRing.IsAllowed = false;
			database.LD_ReleaseRing = ReleaseRings.Codes.STD;
			AssertNoErrors(database.LD_ReleaseRingInfo);

			database.LD_ReleaseRing = ReleaseRings.Codes.LPB;
			AssertNoErrors(database.LD_ReleaseRingInfo);

			database.LD_ReleaseRing = ReleaseRings.Codes.DPR;
			AssertNoErrors(database.LD_ReleaseRingInfo);

			database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LicEnterprise.LE_EnterpriseCode = "HYE";
			database.LD_ReleaseRing = ReleaseRings.Codes.DPR;
			AssertNoErrors(database.LD_ReleaseRingInfo);
		}

		public void TestValidateTechContact()
		{
			TestValidateTechOrAdminContact(LicenceDatabaseSchema.LD_OC_ContractInstallerOrInternalTechContact);
		}

		public void TestValidateAdminContact()
		{
			TestValidateTechOrAdminContact(LicenceDatabaseSchema.LD_OC_LicenseeAdminContact);
		}

		void TestValidateTechOrAdminContact(SchemaGuidColumn column)
		{
			LicenceDatabase database = Factory.New<LicenceDatabase>();
			ZPropertyInfo info = database.ZPropertyInfoHash[column.Name];

			OrgContact contact = Factory.New<OrgContact>();
			info.Value = contact.PK;
			AssertEquals(1, info.GetWarnings().GetUniqueMessageList().Length);
			AssertHasWarning(info, "This contact does not have an email address specified.");

			contact.OC_Email = "meh@meh.com";
			((IBusinessObjectInternals)info.BizObj).Validate(info);
			AssertEquals(0, info.GetWarnings().Count());

			info.Value = ZGuid.Empty;
			AssertEquals(1, info.GetWarnings().GetUniqueMessageList().Length);
			AssertHasWarningContaining(info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestValidateHostedLocation()
		{
			var list = new Enterprise.Registry.Business.CodeDescriptionBoolCollection();
			list.Add("NCW", (NoResString)"Not Hosted With CargoWise", false);
			list.Add("MEL", (NoResString)"Melbourne", true);
			list.Add("BNE", (NoResString)"Brisbane", true);
			list.Add("NY", (NoResString)"New York", true);
			EDIDataRegistry.Instance.DatabaseHostedLocations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			LicenceDatabase database = Factory.NewWithValidTestData<LicenceDatabase>();
			AssertEquals("NCW", database.LD_HostedLocation);
			AssertNoErrors(database.LD_HostedLocationInfo);

			database.LD_HostedLocation = "";
			database.Validation.ValidateLD_HostedLocation();
			AssertHasErrors(database.LD_HostedLocationInfo);

			database.LD_HostedLocation = "XXX";
			database.Validation.ValidateLD_HostedLocation();
			AssertHasErrors(database.LD_HostedLocationInfo);

			database.LD_HostedLocation = "BNE";
			database.Validation.ValidateLD_HostedLocation();
			AssertNoErrors(database.LD_HostedLocationInfo);

			AssertNoExceptionThrown(delegate()
			{
				Factory.Save();
			});
		}

		public void TestValidateLD_EnablePackageDownloadOptimization()
		{
			var list = new Enterprise.Registry.Business.CodeDescriptionBoolCollection();
			list.Add("NCW", (NoResString)"Not Hosted With CargoWise", false);
			list.Add("NY", (NoResString)"New York", true);
			EDIDataRegistry.Instance.DatabaseHostedLocations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			LicenceDatabase database = Factory.NewWithValidTestData<LicenceDatabase>();
			AssertEquals(false, database.LD_EnablePackageDownloadOptimization);

			database.LD_HostedLocation = "NCW";
			database.LD_EnablePackageDownloadOptimization = true;
			database.Validation.ValidateLD_EnablePackageDownloadOptimization();
			AssertHasErrors(database.LD_EnablePackageDownloadOptimizationInfo);

			database.LD_HostedLocation = "NY";
			database.LD_EnablePackageDownloadOptimization = true;
			database.Validation.ValidateLD_EnablePackageDownloadOptimization();
			AssertNoErrors(database.LD_EnablePackageDownloadOptimizationInfo);
		}

		public void TestBillingParty()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "BBB");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "CCC");

			var db1 = lic1.Database;
			var db2 = lic3.Database;
			db1.LD_OH_BillingParty = lic1.Company.LC_OH;
			AssertNoErrors("party has a licence on DB", db1.LD_OH_BillingPartyInfo);

			db1.LD_OH_BillingParty = lic2.Company.LC_OH;
			AssertNoErrors("party has a licence on DB", db1.LD_OH_BillingPartyInfo);

			db1.LD_OH_BillingParty = lic3.Company.LC_OH;
			AssertHasErrors("party not on this DB", db1.LD_OH_BillingPartyInfo);

			db1.LD_OH_BillingParty = ZGuid.Empty;
			AssertNoErrors("empty is allowed", db1.LD_OH_BillingPartyInfo);

			db2.LD_OH_BillingParty = lic3.Company.LC_OH;
			AssertNoErrors("party has a licence on DB", db2.LD_OH_BillingPartyInfo);

			lic1.LA_IsActive = false;
			db1.LD_OH_BillingParty = lic1.Company.LC_OH;
			AssertHasErrors("party licence is not active", db1.LD_OH_BillingPartyInfo);

			lic1.LA_IsActive = true;
			lic1.Company.Header.OH_IsActive = false;
			db1.Validation.ValidateLD_OH_BillingParty();
			AssertHasErrors("party org is not active", db1.LD_OH_BillingPartyInfo);
		}

		public void TestCurrentRunningVersion()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();

			AssertCurrentRunningVersionProduct("Products match", "AAA", "AAA", expectError: false);
			AssertCurrentRunningVersionProduct("Products not match", "AAA", "AA1", expectError: true);
			AssertCurrentRunningVersionProduct("ENT and CW1 are compatible", ProductTypes.Codes.Enterprise, ProductTypes.Codes.CargoWiseOne, expectError: false);
			AssertCurrentRunningVersionProduct("ENT and CWN are compatible", ProductTypes.Codes.Enterprise, ProductTypes.Codes.CargoWiseNext, expectError: false);
			AssertCurrentRunningVersionProduct("ENT and PRW are compatible", ProductTypes.Codes.Enterprise, ProductTypes.Codes.ProductivityWise, expectError: false);
			AssertCurrentRunningVersionProduct("CWN and CW1 are compatible", ProductTypes.Codes.CargoWiseNext, ProductTypes.Codes.CargoWiseOne, expectError: false);
			AssertCurrentRunningVersionProduct("CWN and PRW are compatible", ProductTypes.Codes.CargoWiseNext, ProductTypes.Codes.ProductivityWise, expectError: false);

			void AssertCurrentRunningVersionProduct(string message, string currentVersionProduct, string databaseProduct, bool expectError)
			{
				var releaseBuild = Factory.NewWithValidTestData<ReleaseBuild>();
				var database = Factory.NewWithValidTestData<LicenceDatabase>();
				releaseBuild.HL_Product = currentVersionProduct;
				database.LD_Product = databaseProduct;
				database.LD_HL_CurrentRunningVersion = releaseBuild.PK;
				database.Validation.ValidateLD_HL_CurrentRunningVersion();

				if (expectError)
				{
					AssertHasErrors(message, database.LD_HL_CurrentRunningVersionInfo);
				}
				else
				{
					AssertNoErrors(message, database.LD_HL_CurrentRunningVersionInfo);
				}
			}
		}

		public void TestProduct()
		{
			var list = new SystemProductCollection();

			list.AddNew("MAR", (NoResString)"Mario", true);
			list.AddNew("LGI", (NoResString)"Luigi", true);
			list.AddNew("YSH", (NoResString)"Yoshi", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var db = Factory.NewWithValidTestData<LicenceDatabase>();
			db.Validation.ValidateLD_Product();
			var propInfo = db.LD_ProductInfo;
			AssertNoErrors(propInfo);

			db.LD_Product = "";
			AssertHasErrors(propInfo);

			db.LD_Product = "MAR";
			AssertNoErrors(propInfo);

			db.LD_Product = "ZZZ";
			AssertHasErrors(propInfo);

			db.LD_Product = "LGI";
			Factory.Save();
			Assert("pre:", db.IsInDatabase);
			Assert("pre:", !propInfo.HasChanges);

			// Remove the code from the registry
			list = new SystemProductCollection();
			list.AddNew("MAR", (NoResString)"Mario", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			db.Validation.ValidateLD_Product();
			AssertNoErrors("unknown code becomes valid if it is saved in the DB", propInfo);

			db.LD_Product = "";
			AssertHasErrors("can't change to blank once saved", propInfo);

			db.LicEnterprise.LE_EnterpriseCode = "";
			db.LD_Product = "CW1";
			AssertHasErrors("Enterprise Code is mandatory if a database with product CW1 is attached", propInfo);
		}

		public void TestProduct_HasUsageData()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();

			LicenceHeader testHeader = BillingTestHelper.CreateLicence(Factory, "DDD");
			LicenceModules coreModule = testHeader.Modules.FindByCode(LegacyLicence.Codes.Core);

			ClientCompany clientCompany = BillingTestHelper.FindOrCreateClientCompany(testHeader);
			var db = testHeader.Database;

			ClientStaff clientStaff = Factory.New<ClientStaff>();
			clientStaff.LS_LD = db.PK;
			clientStaff.LS_FullName = "Staff1";

			Factory.Save();

			db.LD_Product = ProductTypes.Codes.Enterprise;
			AssertNoErrors(db.LD_ProductInfo);

			db.LD_Product = "AAA";
			AssertNoErrors(db.LD_ProductInfo);

			db.LD_Product = ProductTypes.Codes.Enterprise;
			var usage = BillingTestHelper.CreateEdiLicenceUsage(clientCompany, clientStaff, "ODM", "COR", new ZDateTime(2018, 6, 1));
			Factory.Save();

			db.LD_Product = "AAA";
			AssertHasError(db.LD_ProductInfo, "Licence Usage data exists, product cannot be changed for billing and accounting reasons.");
		}

		public void TestStatus()
		{
			var db = Factory.NewWithValidTestData<LicenceDatabase>();
			db.LD_Product = "CW1";
			db.Validation.ValidateLD_Status();
			var propInfo = db.LD_StatusInfo;
			AssertNoErrors(propInfo);

			db.LD_Status = "XYZ";
			AssertHasErrors(propInfo);

			db.LD_Status = DatabaseStatusList.Codes.REG;
			AssertHasError(propInfo, "A database can only be registered from within the installation itself.");

			Factory.Save();

			db.Validation.ValidateLD_Status();
			AssertNoErrors("REG is valid once saved", propInfo);

			db.LD_Status = "";
			AssertNoErrors("can change to blank", propInfo);
			AssertHasWarning(propInfo, "You are unregistering this database. Service tasks will not be able to run.");

			db.LD_Status = DatabaseStatusList.Codes.Preregistered;
			AssertNoErrors("can change to blank", propInfo);
			AssertHasWarning(propInfo, "You are unregistering this database. Service tasks will not be able to run.");

			db.LD_Status = DatabaseStatusList.Codes.NON;
			AssertNoErrors("can change to blank", propInfo);
			AssertHasWarning(propInfo, "You are unregistering this database. Service tasks will not be able to run.");

			Factory.Save();
			db.LD_Status = DatabaseStatusList.Codes.REG;
			AssertHasError(propInfo, "A database can only be registered from within the installation itself.");

			db.LD_Product = "ABU";
			db.LD_Status = DatabaseStatusList.Codes.NON;
			db.LD_Status = DatabaseStatusList.Codes.REG;
			AssertNoErrors(propInfo);
		}

		public void TestManualLicenceExpiry()
		{
			var today = ZDateTime.Today;

			var db = Factory.NewWithValidTestData<LicenceDatabase>();
			db.Validation.ValidateLD_ManualLicenceExpiry();
			var propInfo = db.LD_ManualLicenceExpiryInfo;
			AssertNoErrors(propInfo);

			db.LD_ManualLicenceExpiry = today.AddYears(-30);
			AssertNoNotifications(propInfo);

			db.LD_ManualLicenceExpiry = today.AddYears(30);
			AssertNoNotifications(propInfo);
		}

		public void TestCheckProductionDatabaseServerCode()
		{
			var db = Factory.NewWithValidTestData<LicenceDatabase>();

			var otherDb = db.LicEnterprise.Databases.AddNew();
			otherDb.LD_LicenceType = DatabaseTypes.Codes.Production;
			otherDb.LD_Billable = DatabaseBillableFlagList.Codes.YesCustomer;
			otherDb.LD_ServerCode = "AAA";
			otherDb.LD_IsActive = false;

			Factory.Save();

			db.LD_LicenceType = DatabaseTypes.Codes.Test;
			db.LD_Billable = DatabaseBillableFlagList.Codes.YesCustomer;
			db.ProductionDatabaseServerCode = "";
			db.RunPreSaveValidation();
			AssertHasError(db.ProductionDatabaseServerCodeInfo, "Please enter a value.");

			db.LD_LicenceType = DatabaseTypes.Codes.Production;
			db.LD_Billable = DatabaseBillableFlagList.Codes.YesCustomer;
			db.ProductionDatabaseServerCode = "";
			db.RunPreSaveValidation();
			AssertNoErrors(db.ProductionDatabaseServerCodeInfo);

			db.LD_LicenceType = DatabaseTypes.Codes.Test;
			db.LD_Billable = DatabaseBillableFlagList.Codes.No;
			db.ProductionDatabaseServerCode = "";
			db.RunPreSaveValidation();
			AssertNoErrors(db.ProductionDatabaseServerCodeInfo);

			db.LD_LicenceType = DatabaseTypes.Codes.Test;
			db.LD_Billable = DatabaseBillableFlagList.Codes.YesPartner;
			db.ProductionDatabaseServerCode = "BZZ";
			db.RunPreSaveValidation();
			AssertHasError(db.ProductionDatabaseServerCodeInfo, "Enter a valid selection.");

			db.LD_LicenceType = DatabaseTypes.Codes.Test;
			db.LD_Billable = DatabaseBillableFlagList.Codes.YesPartner;
			db.ProductionDatabaseServerCode = "AAA";
			db.RunPreSaveValidation();
			AssertHasError(db.ProductionDatabaseServerCodeInfo, "Enter a valid selection.");
		}

		public void TestCheckLD_Billable()
		{
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			db1.LD_Billable = "Y";
			AssertNoErrors(db1.LD_BillableInfo);
			db1.LD_Billable = "N";
			AssertNoErrors(db1.LD_BillableInfo);
			db1.LD_Billable = "";
			AssertHasError(db1.LD_BillableInfo, "Please enter a value.");
			db1.LD_Billable = "1";
			AssertHasError(db1.LD_BillableInfo, "Enter a valid selection.");
			db1.LD_Billable = "X";
			AssertHasError(db1.LD_BillableInfo, "Please enter a Yes/No value.");

			db1.LD_Billable = "X";
			Factory.Save();
			db1.RunPreSaveValidation();
			AssertHasWarning(db1.LD_BillableInfo, "Please enter a value.");

			db1.LD_Billable = "Y";
			Factory.Save();
			db1.LD_Billable = "X";
			db1.RunPreSaveValidation();
			AssertHasError(db1.LD_BillableInfo, "Please enter a Yes/No value.");
		}

		public void TestCheckLD_OH_WebAccessOrg()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var anotherOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var db = licence.Database;
			db.Validation.ValidateLD_OH_WebAccessOrg();
			AssertNotNull(db.WebAccessOrg);
			AssertNoErrors(db.LD_OH_WebAccessOrgInfo);

			db.LD_OH_WebAccessOrg = anotherOrg.PK;
			AssertHasWarning(db.LD_OH_WebAccessOrgInfo, "The selected Master Org doesn't have relationship with the database");

			db.LD_OH_WebAccessOrg = ZGuid.Empty;
			AssertHasWarnings(db.LD_OH_WebAccessOrgInfo);

			db.LD_IsActive = false;
			db.Validation.ValidateLD_OH_WebAccessOrg();
			AssertNoErrors(db.LD_OH_WebAccessOrgInfo);

			db.LD_IsActive = true;
			db.Validation.ValidateLD_OH_WebAccessOrg();
			AssertHasWarnings(db.LD_OH_WebAccessOrgInfo);
		}

		public void TestCheckLD_OH_WebAccessOrg_WarningWithWebAPIDefaultEnterpriseID()
		{
			using (EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "E000004"))
			{
				var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
				var anotherOrg = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				var db = licence.Database;
				db.Validation.ValidateLD_OH_WebAccessOrg();

				db.LD_OH_WebAccessOrg = anotherOrg.PK;
				AssertHasWarning(db.LD_OH_WebAccessOrgInfo, "The selected Master Org doesn't have relationship with the database");

				db.LicEnterprise.LE_EnterpriseID = "E000004";
				db.Validation.ValidateLD_OH_WebAccessOrg();
				AssertNoWarning(db.LD_OH_WebAccessOrgInfo, "The selected Master Org doesn't have relationship with the database");
			}
		}

		public void TestCheckLD_AllowAutoLogin()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_AllowAutoLogin = true;
			AssertNoErrors(db.LD_AllowAutoLoginInfo);

			db.LD_OH_WebAccessOrg = ZGuid.Empty;
			db.Validation.ValidateLD_AllowAutoLogin();
			AssertHasErrors(db.LD_AllowAutoLoginInfo);

			db.LD_AllowAutoLogin = false;
			AssertNoErrors(db.LD_AllowAutoLoginInfo);
		}

		public void TestSystemReferenceID()
		{
			var testHeader = Factory.New<EDIOrgHeader>();
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.CreateAndLoadLicenceForOrg();
			var testDatabase = testHeader.LicCompany.LicDatabases.AddNew();
			testDatabase.LD_ServerCode = "XDB";
			testDatabase.LD_Product = "BOR";
			testDatabase.LD_TenantID = "Ref#001";

			var dup1 = testHeader.LicCompany.LicEnterprise.Databases.AddNew();
			dup1.LD_Product = "BOR";
			dup1.LD_TenantID = "Ref#001";
			AssertHasError(dup1.LD_TenantIDInfo, "A database with this Tenant ID already exists for this product.");

			dup1.LD_TenantID = "Ref#002";
			AssertNoErrors(dup1.LD_TenantIDInfo);
		}

		public void TestUpgradeSchedulesInfo()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			org.CreateAndLoadLicenceForOrg();
			var db = org.LicCompany.LicDatabases.AddNew();
			db.LD_ServerCode = "XDB";
			db.LD_Product = "BOR";
			db.LD_TenantID = "Ref#001";

			db.LD_NextRunTimeUtcMUG = ZDateTime.Invalid;
			db.LD_NextRunTimeUtcUPG = ZDateTime.Invalid;
			db.Validation.ValidateLD_NextRunTimeUtcMUG();
			db.Validation.ValidateLD_NextRunTimeUtcUPG();
			AssertNoErrors(db.LD_NextRunTimeUtcMUGInfo);
			AssertNoErrors(db.LD_NextRunTimeUtcUPGInfo);

			db.LD_ScheduleStateMUG = @"PZ??AkA?????@Y=,J?s? ?&e?i]}??B?S?{_?B`mT??x?V????G?<?{? ?f?Yy???V????Z??=????KCn4H?{_E???*??B?@?:?D?7???? 4????l?r??Y??O???a<R?Bo?e??\b?q}?W6?S'VK'LswH???";
			db.LD_ScheduleStateUPG = @"PZ??1? ??P$????%K???S??!(?-m}]???p?q?T??#8jpo?{??? S?k?L? ??]??N?i8??[bb???S?q%U?(-q?`??1?n99*???S?O??h???";

			db.Validation.ValidateLD_ScheduleStateMUG();
			db.Validation.ValidateLD_ScheduleStateUPG();
			AssertNoErrors(db.LD_ScheduleStateMUGInfo);
			AssertNoErrors(db.LD_ScheduleStateUPGInfo);
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
	}
}
