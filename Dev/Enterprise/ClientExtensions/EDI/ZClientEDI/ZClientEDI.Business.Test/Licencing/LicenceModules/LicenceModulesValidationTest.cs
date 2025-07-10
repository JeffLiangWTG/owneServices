using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	internal class LicenceModulesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUserCount()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			Factory.Save();
			Assert("No Save Errors", testHeader.NotificationsIncludingChildren.GetErrors().Count() == 0);

			LicenceDatabase db = testHeader.LicCompany.LicDatabases.AddNew();
			db.LD_Product = ProductTypes.Codes.Enterprise;
			db.LD_OA_SoftwareInstallAddressDetails = testHeader.Addresses[0].PK;
			var licHeader = testHeader.LicCompany.GetHeader(db);

			licHeader.Modules[0].LM_Calc_IsEnabled = ZBool.True;
			licHeader.Modules[0].LM_LicenceType = "PUR";
			licHeader.Modules[0].LM_UserCount = 0;
			licHeader.Modules[0].Validation.ValidateLM_UserCount();
			AssertHasErrors("User Count is Zero and module is selected", licHeader.Modules[0].LM_UserCountInfo);

			licHeader.Modules[1].LM_UserCount = 10;
			AssertHasWarnings("Module has more users than its parent", licHeader.Modules[1].LM_UserCountInfo);

			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Hybrid;
			licHeader.Modules[0].LM_LicenceType = "ODM";
			licHeader.Modules[0].LM_UserCount = 10;
			AssertNoErrors("non-zero user count allowed with Hybrid ODM", licHeader.Modules[0].LM_UserCountInfo);
		}

		public void TestLicenceType_PurchaseLimitations()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			Factory.Save();
			Assert("No Save Errors", testHeader.NotificationsIncludingChildren.GetErrors().Count() == 0);

			LicenceDatabase db = testHeader.LicCompany.LicDatabases.AddNew();
			db.LD_Product = ProductTypes.Codes.Enterprise;
			db.LD_OA_SoftwareInstallAddressDetails = testHeader.Addresses[0].PK;
			var licHeader = testHeader.LicCompany.GetHeader(db);

			licHeader.Modules[0].LM_ExpiryDate = ZDateTime.Empty;
			licHeader.Modules[0].LM_LicenceType = "PUR";
			AssertNoErrors("LicenceType: Purchase - No Expiry Date (VALID)", licHeader.Modules[0].LM_ExpiryDateInfo);

			licHeader.Modules[0].LM_ExpiryDate = ZDateTime.Now;
			AssertHasErrors("LicenceType: Purchase - Expiry Date (INVALID)", licHeader.Modules[0].LM_ExpiryDateInfo);
		}

		public void TestLicenceType_NoneLimitations()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			Factory.Save();
			Assert("No Save Errors", testHeader.NotificationsIncludingChildren.GetErrors().Count() == 0);

			LicenceDatabase db = testHeader.LicCompany.LicDatabases.AddNew();
			db.LD_Product = ProductTypes.Codes.Enterprise;
			db.LD_OA_SoftwareInstallAddressDetails = testHeader.Addresses[0].PK;
			var licHeader = testHeader.LicCompany.GetHeader(db);

			licHeader.Modules[0].LM_ExpiryDate = ZDateTime.Empty;
			licHeader.Modules[0].LM_LicenceType = "NON";
			AssertNoErrors("LicenceType: None - No Expiry Date (VALID)", licHeader.Modules[0].LM_ExpiryDateInfo);

			licHeader.Modules[0].LM_ExpiryDate = ZDateTime.Now;
			AssertHasErrors("LicenceType: None - Expiry Date (INVALID)", licHeader.Modules[0].LM_ExpiryDateInfo);
		}

		public void TestLicenceType_RentalLimitations()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			Factory.Save();
			Assert("No Save Errors", testHeader.NotificationsIncludingChildren.GetErrors().Count() == 0);

			LicenceDatabase db = testHeader.LicCompany.LicDatabases.AddNew();
			db.LD_Product = ProductTypes.Codes.Enterprise;
			db.LD_OA_SoftwareInstallAddressDetails = testHeader.Addresses[0].PK;
			var licHeader = testHeader.LicCompany.GetHeader(db);

			licHeader.Modules[0].LM_ExpiryDate = ZDateTime.Empty;
			licHeader.Modules[0].LM_LicenceType = "REN";
			licHeader.Modules[0].Validation.ValidateLM_ExpiryDate();
			AssertNoErrors("LicenceType: Rental - No Expiry Date (VALID)", licHeader.Modules[0].LM_ExpiryDateInfo);

			licHeader.Modules[0].LM_ExpiryDate = ZDateTime.Now.AddDays(7);
			AssertNoErrors("LicenceType: Rental - Expiry Date (VALID)", licHeader.Modules[0].LM_ExpiryDateInfo);

			licHeader.Modules[0].LM_ExpiryDate = ZDateTime.Now.AddDays(-7);
			AssertNoWarnings("LicenceType: Rental - Expiry Date (VALID despite being in the past)", licHeader.Modules[0].LM_ExpiryDateInfo);
		}

		public void TestLicenceType_TrialLimitations()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			Factory.Save();
			Assert("No Save Errors", testHeader.NotificationsIncludingChildren.GetErrors().Count() == 0);

			LicenceDatabase db = testHeader.LicCompany.LicDatabases.AddNew();
			db.LD_Product = ProductTypes.Codes.Enterprise;
			db.LD_OA_SoftwareInstallAddressDetails = testHeader.Addresses[0].PK;
			var licHeader = testHeader.LicCompany.GetHeader(db);

			licHeader.Modules[0].LM_LicenceType = "TRI";
			licHeader.Modules[0].Validation.ValidateLM_ExpiryDate();
			AssertHasErrors("LicenceType: Trial - No Expiry Date (INVALID)", licHeader.Modules[0].LM_ExpiryDateInfo);

			licHeader.Modules[0].LM_ExpiryDate = ZDateTime.Now.AddMonths(1);
			AssertNoErrors("LicenceType: Trial - Expiry Date in future (VALID)", licHeader.Modules[0].LM_ExpiryDateInfo);

			licHeader.Modules[0].LM_ExpiryDate = ZDateTime.Now.AddMonths(-1);
			AssertHasWarnings("LicenceType: Trial - Expiry Date In Past (WARNING)", licHeader.Modules[0].LM_ExpiryDateInfo);
		}

		public void TestLicenceType_MandatoryAndListValidation()
		{
			LicenceModules module = Factory.New<LicenceModules>();
			module.LM_LicenceType = "";
			module.Validation.ValidateLM_LicenceType();
			AssertMandatoryValidationError(module.LM_LicenceTypeInfo, true);

			module.LM_LicenceType = LicenceTypes.Codes.NON;
			AssertNoErrors(module.LM_LicenceTypeInfo);

			module.LM_LicenceType = LicenceTypes.Codes.PUR;
			AssertListValidationInvalidCodeError(module.LM_LicenceTypeInfo, true);

			LicenceHeader header = Factory.NewWithValidTestData<LicenceHeader>();
			header.Database.LD_Product = ProductTypes.Codes.Enterprise;
			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Advanced;
			module.LM_LA = header.PK;
			module.Validation.ValidateLM_LicenceType();
			AssertListValidationInvalidCodeError(module.LM_LicenceTypeInfo, false);

			module.LM_LicenceType = LicenceTypes.Codes.ODM;
			AssertListValidationInvalidCodeError(module.LM_LicenceTypeInfo, true);

			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			module = header.GetCoreModule();
			module.Validation.ValidateLM_LicenceType();
			AssertListValidationInvalidCodeError(module.LM_LicenceTypeInfo, false);

			module.LM_LicenceType = LicenceTypes.Codes.REN;
			AssertListValidationInvalidCodeError(module.LM_LicenceTypeInfo, true);

			module.LM_LicenceType = LicenceTypes.Codes.NON;
			AssertNoErrors(module.LM_LicenceTypeInfo);

			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Hybrid;
			module.LM_LicenceType = LicenceTypes.Codes.PUR;
			AssertListValidationInvalidCodeError(module.LM_LicenceTypeInfo, true);
			module.LM_LicenceType = LicenceTypes.Codes.ODM;
			AssertListValidationInvalidCodeError(module.LM_LicenceTypeInfo, false);
		}

		public void TestLicenceType_NewTypesNotSupported()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase db = testHeader.LicCompany.LicDatabases.AddNew();
			db.LD_Product = ProductTypes.Codes.Enterprise;
			var header = testHeader.LicCompany.GetHeader(db);
			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			header.Modules[0].LM_LicenceType = LicenceTypes.Codes.ODM;
			AssertNoNotifications(header.Modules[0].LM_LicenceTypeInfo);
			Factory.Save();

			header.Modules[0].Validation.ValidateLM_LicenceType();
			string expectedWarningMessage = "Licence type 'ODM' might not be supported by the client's system.\r\nTry using 'NON' instead, which actually means 'Always Allow' in the older systems";
			AssertHasWarning(header.Modules[0].LM_LicenceTypeInfo, expectedWarningMessage);

			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_ReleaseStatus = ReleaseRings.Codes.DPR;
			build.VersionNumber = LicenceHeader.LineLevelOnDemandLicenceVersion.AddRelease(-1);
			db.LD_HL_CurrentRunningVersion = build.PK;
			header.LA_LD = db.PK;

			header.Modules[0].Validation.ValidateLM_LicenceType();
			AssertHasWarning(header.Modules[0].LM_LicenceTypeInfo, expectedWarningMessage);

			build.VersionNumber = LicenceHeader.LineLevelOnDemandLicenceVersion;
			header.Modules[0].Validation.ValidateLM_LicenceType();
			AssertNoWarnings(header.Modules[0].LM_LicenceTypeInfo);
		}

		public void TestUserCount_OnDemand()
		{
			LicenceHeader header = Factory.NewWithValidTestData<LicenceHeader>();
			header.Database.LD_Product = ProductTypes.Codes.Enterprise;
			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			LicenceModules module = header.Modules.AddNew();
			module.LM_Calc_IsEnabled = true;
			module.LM_LicenceType = LicenceTypes.Codes.ODM;
			module.LM_UserCount = 20;
			AssertHasError(module.LM_UserCountInfo, "User Count must be 0 if licence type is '" + LicenceTypes.Descriptions.ODM + "'");

			module.LM_UserCount = 0;
			AssertNoErrors(module.LM_UserCountInfo);

			module.LM_LicenceType = LicenceTypes.Codes.OTM;
			module.LM_UserCount = 20;
			AssertNoErrors(module.LM_UserCountInfo);

			module.LM_UserCount = 0;
			AssertHasError(module.LM_UserCountInfo, "Please enter an 'User Count' greater than 0.");

			module.LM_LicenceType = LicenceTypes.Codes.OPN;
			module.LM_UserCount = 20;
			AssertNoErrors(module.LM_UserCountInfo);

			module.LM_UserCount = 0;
			AssertHasError(module.LM_UserCountInfo, "Please enter an 'User Count' greater than 0.");

			module.LM_LicenceType = LicenceTypes.Codes.PUR;
			module.LM_UserCount = 20;
			AssertNoErrors(module.LM_UserCountInfo);

			module.LM_UserCount = 0;
			AssertHasError(module.LM_UserCountInfo, "Please enter an 'User Count' greater than 0.");

			module.LM_LicenceType = LicenceTypes.Codes.REN;
			module.LM_UserCount = 20;
			AssertNoErrors(module.LM_UserCountInfo);

			module.LM_UserCount = 0;
			AssertHasError(module.LM_UserCountInfo, "Please enter an 'User Count' greater than 0.");

			module.LM_LicenceType = LicenceTypes.Codes.SRU;
			module.LM_UserCount = 20;
			AssertHasError(module.LM_UserCountInfo, "User Count must be 9999 if licence type is '" + LicenceTypes.Descriptions.SRU + "'");

			module.LM_UserCount = 9999;
			AssertNoErrors(module.LM_UserCountInfo);
		}

		public void TestUserCount_STL()
		{
			LicenceHeader header = Factory.NewWithValidTestData<LicenceHeader>();
			header.Database.LD_Product = ProductTypes.Codes.Enterprise;
			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			LicenceModules module = header.Modules.AddNew();
			module.LM_Calc_IsEnabled = true;
			module.LM_LicenceType = LicenceTypes.Codes.ODM;
			module.LM_UserCount = 20;
			AssertNoErrors(module.LM_UserCountInfo);
		}

		public void TestUserCount_MustNotHaveMoreUsersThanParent()
		{
			LicenceHeader header = Factory.NewWithValidTestData<LicenceHeader>();
			header.Database.LD_Product = ProductTypes.Codes.Enterprise;
			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Advanced;
			LicenceModules coreModule = header.GetCoreModule();
			coreModule.LM_UserCount = 5;

			LicenceModules forwarderModule = header.Modules.FindByCode(Env.Licence.Forwarder.Name);
			forwarderModule.LM_UserCount = 7;
			string expectedErrorMessage = string.Format(CultureInfo.CurrentCulture, "The module {0} should not have more users than the module {1}", Env.Licence.Forwarder.DisplayName, Env.Licence.Core.DisplayName);
			AssertHasWarning(forwarderModule.LM_UserCountInfo, expectedErrorMessage);

			forwarderModule.LM_UserCount = 5;
			AssertNoErrors(forwarderModule.LM_UserCountInfo);

			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			foreach (LicenceModules module in header.Modules)
			{
				module.LM_LicenceType = LicenceTypes.Codes.ODM;
			}
			LicenceModules campaignManagerModule = header.Modules.FindByCode(Env.Licence.RelationshipCampaignManager.Name);
			campaignManagerModule.LM_UserCount = 5;
			campaignManagerModule.LM_LicenceType = LicenceTypes.Codes.OTM;
			AssertNoWarnings("Parent is OnDemand", campaignManagerModule.LM_UserCountInfo);

			LicenceModules marketingManagerModule = header.Modules.FindByCode(Env.Licence.RelationshipManager.Name);
			marketingManagerModule.LM_UserCount = 2;
			marketingManagerModule.LM_LicenceType = LicenceTypes.Codes.OPN;
			AssertNoWarnings("Parent is OnDemand", marketingManagerModule.LM_UserCountInfo);

			campaignManagerModule.Validation.ValidateLM_UserCount();
			expectedErrorMessage = string.Format(CultureInfo.CurrentCulture, "The module {0} should not have more users than the module {1}", Env.Licence.RelationshipCampaignManager.DisplayName, Env.Licence.RelationshipManager.DisplayName);
			AssertHasWarning(campaignManagerModule.LM_UserCountInfo, expectedErrorMessage);

			marketingManagerModule.LM_LicenceType = LicenceTypes.Codes.ODM;
			campaignManagerModule.Validation.ValidateLM_UserCount();
			AssertNoWarnings("Parent is OnDemand", campaignManagerModule.LM_UserCountInfo);

			coreModule = header.GetCoreModule();
			coreModule.LM_UserCount = 2;
			coreModule.LM_LicenceType = LicenceTypes.Codes.OTM;
			campaignManagerModule.Validation.ValidateLM_UserCount();
			expectedErrorMessage = string.Format(CultureInfo.CurrentCulture, "The module {0} should not have more users than the module {1}", Env.Licence.RelationshipCampaignManager.DisplayName, Env.Licence.Core.DisplayName);
			AssertHasWarning(campaignManagerModule.LM_UserCountInfo, expectedErrorMessage);

			coreModule.LM_UserCount = 5;
			campaignManagerModule.Validation.ValidateLM_UserCount();
			AssertNoWarnings(campaignManagerModule.LM_UserCountInfo);
		}

		public void TestLicenceType_NewODMHasLockedCoreLicence()
		{
			LicenceHeader header = Factory.NewWithValidTestData<LicenceHeader>();
			header.Database.LD_Product = ProductTypes.Codes.Enterprise;
			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			LicenceModules coreModule = header.GetCoreModule();
			coreModule.LM_LicenceType = LicenceTypes.Codes.NON;
			AssertNoWarnings("Client's system does not support line level ODM licence type", coreModule.LM_LicenceTypeInfo);

			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_ReleaseStatus = ReleaseRings.Codes.DPR;
			build.VersionNumber = LicenceHeader.LineLevelOnDemandLicenceVersion;
			header.Database.LD_HL_CurrentRunningVersion = build.PK;
			coreModule.Validation.ValidateLM_LicenceType();
			AssertHasWarning(header.Modules[0].LM_LicenceTypeInfo, "You have locked out the Core module licence required to run ediEnterprise/CargoWise One. Please ensure this is intended.");

			LicenceModules otherModule = header.Modules.FindByCode(Env.Licence.Forwarder.Name);
			otherModule.LM_LicenceType = LicenceTypes.Codes.NON;
			otherModule.Validation.ValidateLM_LicenceType();
			AssertNoWarnings("Only required for Core module", otherModule.LM_LicenceTypeInfo);
		}

		public void TestLicenceType_OpenLicenceSupported()
		{
			LicenceHeader header = Factory.NewWithValidTestData<LicenceHeader>();
			header.Database.LD_Product = ProductTypes.Codes.Enterprise;
			header.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			header.Modules[0].LM_LicenceType = LicenceTypes.Codes.OPN;
			string expectedWarningMessage = "Licence type 'OPN' might not be supported by the client's system.\r\n. It requires build version 1.4.3749.0 or later.";
			AssertHasWarning(header.Modules[0].LM_LicenceTypeInfo, expectedWarningMessage);

			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(1, 4, 3748, 0);
			header.Database.LD_HL_CurrentRunningVersion = build.PK;

			header.Modules[0].Validation.ValidateLM_LicenceType();
			AssertHasWarning(header.Modules[0].LM_LicenceTypeInfo, expectedWarningMessage);

			build.VersionNumber = new VersionNumber(1, 4, 3749, 0);
			header.Modules[0].Validation.ValidateLM_LicenceType();
			AssertNoWarnings(header.Modules[0].LM_LicenceTypeInfo);
		}

		#region Implementation

		EDIOrgHeader HeaderForTest
		{
			get
			{
				EDIOrgHeader fHeaderForTest = Factory.NewWithValidTestData<EDIOrgHeader>();
				fHeaderForTest.OH_Code = "TGBLOG";
				fHeaderForTest.OH_RL_NKClosestPort = "AUBNE";
				fHeaderForTest.MainAddress.OA_Phone = "+61426829924";

				OrgAddress newAddress = fHeaderForTest.Addresses.AddNew();
				newAddress.OA_Address1 = "666 Test Address";
				newAddress.OA_Phone = "+61426829924";

				return fHeaderForTest;
			}
		}

		#endregion
	}
}
