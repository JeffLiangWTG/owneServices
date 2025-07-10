using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(LicenceModules))]
	public class LicenceModulesTest : SecurityBusinessObjectTestCase
	{
		public void TestSettingLM_Calc_IsEnabledWhenDefaultIsSomethingOtherThanNonLicencedSetsItBackToTheDefault()
		{
			EDIOrgHeader orgHeader = HeaderForTest;
			orgHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase licenceDatabase = orgHeader.LicCompany.LicDatabases.AddNew();
			licenceDatabase.LD_Product = ProductTypes.Codes.Enterprise;
			var licenceHeader = orgHeader.LicCompany.GetHeader(licenceDatabase);
			LicenceModules importQuarantineMessagingModule = licenceHeader.Modules.FindByCode(Env.Licence.ImportQuarantineMessaging.Name);
			LicenceModules optionalCpt = licenceHeader.Modules.FindByCode(Env.Licence.OneStopAUContainerIntegration.Name);
			AssertNotNull("Precondition: importQuarantineMessagingModule", importQuarantineMessagingModule);
			LicenceModules coreModule = licenceHeader.GetCoreModule();
			AssertNotNull("Precondition: coreModule", coreModule);

			importQuarantineMessagingModule.LM_Calc_IsEnabled = false;
			AssertEquals("importQuarantineMessagingModule.LM_LicenceType", LicenceTypes.Codes.NON, importQuarantineMessagingModule.LM_LicenceType);
			AssertEquals("importQuarantineMessagingModule.LM_UserCount", ZShort.Zero, importQuarantineMessagingModule.LM_UserCount);
			optionalCpt.LM_Calc_IsEnabled = false;
			AssertEquals("optionalCpt.LM_LicenceType", LicenceTypes.Codes.NON, optionalCpt.LM_LicenceType);
			AssertEquals("optionalCpt.LM_UserCount", ZShort.Zero, optionalCpt.LM_UserCount);

			coreModule.LM_LicenceType = LicenceTypes.Codes.PUR;
			coreModule.LM_UserCount = 44;

			AssertEquals("importQuarantineMessagingModule.LM_LicenceType", LicenceTypes.Codes.NON, importQuarantineMessagingModule.LM_LicenceType);
			AssertEquals("importQuarantineMessagingModule.LM_UserCount", ZShort.Zero, importQuarantineMessagingModule.LM_UserCount);
			AssertEquals("optionalCpt.LM_LicenceType", LicenceTypes.Codes.NON, optionalCpt.LM_LicenceType);
			AssertEquals("optionalCpt.LM_UserCount", ZShort.Zero, optionalCpt.LM_UserCount);

			importQuarantineMessagingModule.LM_Calc_IsEnabled = true;
			AssertEquals("importQuarantineMessagingModule.LM_LicenceType", LicenceTypes.Codes.CPT, importQuarantineMessagingModule.LM_LicenceType);
			AssertEquals("importQuarantineMessagingModule.LM_UserCount", ZShort.Zero, importQuarantineMessagingModule.LM_UserCount);

			optionalCpt.LM_Calc_IsEnabled = true;
			AssertEquals("optionalCpt.LM_LicenceType", LicenceTypes.Codes.CPT, importQuarantineMessagingModule.LM_LicenceType);
			AssertEquals("optionalCpt.LM_UserCount", ZShort.Zero, importQuarantineMessagingModule.LM_UserCount);
		}

		public void TestDefaultValues()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			Factory.Save();
			Assert("No Save Errors", testHeader.NotificationsIncludingChildren.GetErrors().Count() == 0);

			LicenceDatabase db = testHeader.LicCompany.LicDatabases.AddNew();
			db.LD_Product = ProductTypes.Codes.Enterprise;
			var licHeader = testHeader.LicCompany.GetHeader(db);
			AssertEquals("Module Type Default to None", "NON", licHeader.Modules[0].LM_LicenceType);
		}

		public void TestLicenceTypeSetsExpiryForRental()
		{
			LicenceHeader header = Factory.New<LicenceHeader>();
			header.LA_ContractExpiryDate = new ZDateTime(2006, 5, 28);

			LicenceModules module = header.Modules.AddNew();
			module.LM_LicenceType = LicenceTypes.Codes.PUR;
			AssertEquals("Only set for rental", ZDateTime.Empty, module.LM_ExpiryDate);

			module.LM_LicenceType = LicenceTypes.Codes.REN;
			AssertEquals("Set for rental", header.LA_ContractExpiryDate, module.LM_ExpiryDate);
		}

		public void TestOnSavingCheckForDuplicates()
		{
			int initialModuleCount = Factory.Load<LicenceModules>(new ZQuery()).Length;
			ZString groupCode1 = "GC1";
			ZString groupCode2 = "GC2";
			LicenceHeader header1 = Factory.NewWithValidTestData<LicenceHeader>();
			LicenceHeader header2 = Factory.NewWithValidTestData<LicenceHeader>();
			header1.Modules.RemoveAndDeleteAll();
			header2.Modules.RemoveAndDeleteAll();
			Factory.Save();

			LicenceModules module1 = header1.Modules.AddNew();
			LicenceModules module2 = header2.Modules.AddNew();
			module1.LM_GroupModuleCode = groupCode1;
			module2.LM_GroupModuleCode = groupCode1;
			Factory.Save();
			AssertEquals("Licence Modules with different parent Licence Headers should not overwrite each other", initialModuleCount + 2, Factory.Load<LicenceModules>(new ZQuery()).Length);

			LicenceModules module3 = header1.Modules.AddNew();
			module3.LM_GroupModuleCode = groupCode2;
			Factory.Save();
			AssertEquals("Licence Modules with different Group Module Codes should not overwrite each other", initialModuleCount + 3, Factory.Load<LicenceModules>(new ZQuery()).Length);

			LicenceModules module4 = header1.Modules.AddNew();
			module4.LM_GroupModuleCode = groupCode1;
			Factory.Save();
			AssertEquals("Licence Modules with same parent Licence Headers and Group Module Codes should overwrite each other", initialModuleCount + 3, Factory.Load<LicenceModules>(new ZQuery()).Length);
			AssertEquals("Licence Modules with same parent Licence Headers and Group Module Codes should overwrite each other", true, module1.IsDeleted);
		}

		public void TestLM_Calc_IsEnabled()
		{
			LicenceHeader licHeader = Factory.NewWithValidTestData<LicenceHeader>();
			licHeader.Database.LD_Product = ProductTypes.Codes.Enterprise;
			object lazyLoadModules = licHeader.Modules;
			LicenceModules coreModule = licHeader.GetCoreModule();
			coreModule.LM_LicenceType = LicenceTypes.Codes.REN;
			coreModule.LM_UserCount = 5;
			coreModule.LM_ExpiryDate = new ZDateTime(2009, 5, 5);

			LicenceModules forwarderModule = (LicenceModules)licHeader.Modules.Find(new ZQuery(LicenceModulesSchema.LM_GroupModuleCode, Env.Licence.Forwarder.Name))[0];
			AssertEquals("Precondition", ZShort.Zero, forwarderModule.LM_UserCount);
			AssertEquals("Precondition", ZDateTime.Empty, forwarderModule.LM_ExpiryDate);
			AssertEquals("Precondition", LicenceTypes.Codes.NON, forwarderModule.LM_LicenceType);

			forwarderModule.LM_Calc_IsEnabled = true;
			AssertEquals((short)5, forwarderModule.LM_UserCount);
			AssertEquals(new ZDateTime(2009, 5, 5), forwarderModule.LM_ExpiryDate);
			AssertEquals(LicenceTypes.Codes.REN, forwarderModule.LM_LicenceType);

			forwarderModule.LM_Calc_IsEnabled = false;
			AssertEquals(ZShort.Zero, forwarderModule.LM_UserCount);
			AssertEquals(ZDateTime.Empty, forwarderModule.LM_ExpiryDate);
			AssertEquals(LicenceTypes.Codes.NON, forwarderModule.LM_LicenceType);
			AssertNoErrors(forwarderModule.LM_LicenceTypeInfo);

			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Hybrid;
			coreModule.LM_LicenceType = LicenceTypes.Codes.ODM;
			coreModule.LM_UserCount = 5;

			forwarderModule.LM_Calc_IsEnabled = true;
			AssertEquals(ZShort.Zero, forwarderModule.LM_UserCount);
			AssertEquals(ZDateTime.Empty, forwarderModule.LM_ExpiryDate);
			AssertEquals(LicenceTypes.Codes.ODM, forwarderModule.LM_LicenceType);
		}

		public void TestLM_ExpiryDate()
		{
			LicenceHeader licHeader = Factory.NewWithValidTestData<LicenceHeader>();
			licHeader.Database.LD_Product = ProductTypes.Codes.Enterprise;
			object lazyLoadModules = licHeader.Modules;
			LicenceModules coreModule = licHeader.GetCoreModule();
			Assert("Disabled", coreModule.LM_ExpiryDateInfo.ReadOnly);

			coreModule.LM_Calc_IsEnabled = true;
			coreModule.LM_LicenceType = LicenceTypes.Codes.PUR;
			Assert(coreModule.LM_ExpiryDateInfo.ReadOnly);

			coreModule.LM_LicenceType = LicenceTypes.Codes.NON;
			Assert(coreModule.LM_ExpiryDateInfo.ReadOnly);

			coreModule.LM_LicenceType = LicenceTypes.Codes.ODM;
			Assert(coreModule.LM_ExpiryDateInfo.ReadOnly);

			coreModule.LM_LicenceType = LicenceTypes.Codes.OPN;
			Assert(coreModule.LM_ExpiryDateInfo.ReadOnly);

			coreModule.LM_LicenceType = LicenceTypes.Codes.OTM;
			Assert(coreModule.LM_ExpiryDateInfo.ReadOnly);

			coreModule.LM_LicenceType = LicenceTypes.Codes.PUR;
			Assert(coreModule.LM_ExpiryDateInfo.ReadOnly);

			coreModule.LM_LicenceType = LicenceTypes.Codes.SRU;
			Assert(coreModule.LM_ExpiryDateInfo.ReadOnly);

			coreModule.LM_LicenceType = LicenceTypes.Codes.TRI;
			Assert(!coreModule.LM_ExpiryDateInfo.ReadOnly);

			coreModule.LM_LicenceType = LicenceTypes.Codes.REN;
			Assert(!coreModule.LM_ExpiryDateInfo.ReadOnly);

			LicenceModules forwarderModule = (LicenceModules)licHeader.Modules.Find(new ZQuery(LicenceModulesSchema.LM_GroupModuleCode, Env.Licence.Forwarder.Name))[0];
			forwarderModule.LM_Calc_IsEnabled = true;
			forwarderModule.LM_LicenceType = LicenceTypes.Codes.REN;
			AssertEquals("Precondition", ZDateTime.Empty, forwarderModule.LM_ExpiryDate);

			coreModule.LM_ExpiryDate = new ZDateTime(2009, 6, 6);
			AssertEquals("Should not change other modules", ZDateTime.Empty, forwarderModule.LM_ExpiryDate);
			Assert(!forwarderModule.LM_ExpiryDateInfo.ReadOnly);

			forwarderModule.LM_Calc_IsEnabled = false;
			Assert("Disabled", forwarderModule.LM_ExpiryDateInfo.ReadOnly);
		}

		public void TestLM_LicenceType()
		{
			LicenceHeader licHeader = Factory.NewWithValidTestData<LicenceHeader>();
			licHeader.Database.LD_Product = ProductTypes.Codes.Enterprise;
			object lazyLoadModules = licHeader.Modules;
			LicenceModules coreModule = licHeader.GetCoreModule();
			Assert("Disabled", coreModule.LM_LicenceTypeInfo.ReadOnly);

			coreModule.LM_Calc_IsEnabled = true;
			Assert(!coreModule.LM_LicenceTypeInfo.ReadOnly);

			LicenceModules forwarderModule = (LicenceModules)licHeader.Modules.Find(new ZQuery(LicenceModulesSchema.LM_GroupModuleCode, Env.Licence.Forwarder.Name))[0];
			forwarderModule.LM_Calc_IsEnabled = true;
			AssertEquals(LicenceTypes.Codes.PUR, forwarderModule.LM_LicenceType);

			coreModule.LM_LicenceType = LicenceTypes.Codes.REN;
			AssertEquals("Should not change other modules", LicenceTypes.Codes.PUR, forwarderModule.LM_LicenceType);
			Assert(!forwarderModule.LM_LicenceTypeInfo.ReadOnly);

			forwarderModule.LM_Calc_IsEnabled = false;
			Assert("Disabled", forwarderModule.LM_LicenceTypeInfo.ReadOnly);
		}

		public void TestLM_LicenceType_DefaultingOtherProperties()
		{
			LicenceModules module = Factory.New<LicenceModules>();
			module.LM_UserCount = 23;
			module.LM_LicenceType = LicenceTypes.Codes.ODM;
			AssertEquals("Should be reset to 0", (short)0, module.LM_UserCount);

			module.LM_UserCount = 11;
			module.LM_LicenceType = LicenceTypes.Codes.ODM;
			AssertEquals((short)11, module.LM_UserCount);

			module.LM_LicenceType = LicenceTypes.Codes.OPN;
			AssertEquals((short)11, module.LM_UserCount);

			module.LM_LicenceType = LicenceTypes.Codes.SRU;
			AssertEquals("Should be reset to 9999", (short)9999, module.LM_UserCount);

			LicenceHeader licHeader = Factory.NewWithValidTestData<LicenceHeader>();
			licHeader.Database.LD_Product = ProductTypes.Codes.Enterprise;
			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.ConcurrentUniversal;
			module = licHeader.GetCoreModule();
			module.LM_Calc_IsEnabled = true;
			module.LM_UserCount = 17;
			AssertEquals(LicenceTypes.Codes.PUR, module.LM_LicenceType);

			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Hybrid;
			AssertEquals(LicenceTypes.Codes.ODM, module.LM_LicenceType);
			AssertEquals("Hybrid licence should not reset user count to zero", (short)17, module.LM_UserCount);
		}

		public void TestLM_UserCount()
		{
			LicenceHeader licHeader = Factory.NewWithValidTestData<LicenceHeader>();
			licHeader.Database.LD_Product = ProductTypes.Codes.Enterprise;
			object lazyLoadModules = licHeader.Modules;
			LicenceModules coreModule = licHeader.GetCoreModule();
			Assert("Disabled", coreModule.LM_UserCountInfo.ReadOnly);

			coreModule.LM_Calc_IsEnabled = true;
			Assert(!coreModule.LM_UserCountInfo.ReadOnly);

			LicenceModules forwarderModule = (LicenceModules)licHeader.Modules.Find(new ZQuery(LicenceModulesSchema.LM_GroupModuleCode, Env.Licence.Forwarder.Name))[0];
			forwarderModule.LM_Calc_IsEnabled = true;
			AssertEquals("Precondition", ZShort.Zero, forwarderModule.LM_UserCount);

			coreModule.LM_UserCount = (short)90;
			AssertEquals("Should not change other modules", (short)0, forwarderModule.LM_UserCount);
			Assert(!forwarderModule.LM_UserCountInfo.ReadOnly);

			forwarderModule.LM_Calc_IsEnabled = false;
			Assert("Disabled", forwarderModule.LM_UserCountInfo.ReadOnly);

			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			Assert(coreModule.LM_UserCountInfo.ReadOnly);
		}

		public void TestParentModule()
		{
			LicenceModules campaignManagerModule = Factory.New<LicenceModules>();
			AssertNull(campaignManagerModule.ParentModule);

			campaignManagerModule.LM_GroupModuleCode = Env.Licence.RelationshipCampaignManager.Name;
			AssertNull(campaignManagerModule.ParentModule);

			LicenceHeader licHeader = Factory.New<LicenceHeader>();
			licHeader.Modules.RemoveAndDeleteAll();
			licHeader.Modules.Add(campaignManagerModule);
			AssertNull(campaignManagerModule.ParentModule);

			LicenceModules marketingManagerModule = Factory.New<LicenceModules>();
			marketingManagerModule.LM_GroupModuleCode = Env.Licence.RelationshipManager.Name;
			licHeader.Modules.Add(marketingManagerModule);
			AssertEquals(marketingManagerModule, campaignManagerModule.ParentModule);
		}

		public void TestGetNonOnDemandParentModule()
		{
			LicenceHeader licHeader = Factory.NewWithValidTestData<LicenceHeader>();
			licHeader.Database.LD_Product = ProductTypes.Codes.Enterprise;
			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			foreach (LicenceModules module in licHeader.Modules)
			{
				module.LM_LicenceType = LicenceTypes.Codes.ODM;
			}
			LicenceModules coldCallModule = (LicenceModules)licHeader.Modules.Find(new ZQuery(LicenceModulesSchema.LM_GroupModuleCode, Env.Licence.RelationshipColdCallRegister.Name))[0];
			AssertNull(coldCallModule.GetNonOnDemandParentModule());

			coldCallModule.ParentModule.LM_LicenceType = LicenceTypes.Codes.OTM;
			AssertEquals(coldCallModule.ParentModule, coldCallModule.GetNonOnDemandParentModule());

			coldCallModule.ParentModule.LM_LicenceType = LicenceTypes.Codes.ODM;
			AssertNull(coldCallModule.GetNonOnDemandParentModule());

			coldCallModule.ParentModule.ParentModule.LM_LicenceType = LicenceTypes.Codes.OPN;
			AssertEquals(coldCallModule.ParentModule.ParentModule, coldCallModule.GetNonOnDemandParentModule());
		}

		public void TestListAttributes()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(LicenceModules),
				LicenceModulesSchema.Constants.LM_LicenceType,
				false,
				(a) => a.ListDataSourceMember == "Lookups.LicenceTypesList");
		}

		public void TestHasLineLevelOnDemandLicenceType()
		{
			LicenceModules module = Factory.New<LicenceModules>();
			module.LM_LicenceType = LicenceTypes.Codes.NON;
			Assert(!module.HasLineLevelOnDemandLicenceType);

			module.LM_LicenceType = LicenceTypes.Codes.ODM;
			Assert(module.HasLineLevelOnDemandLicenceType);

			module.LM_LicenceType = LicenceTypes.Codes.SRU;
			Assert(module.HasLineLevelOnDemandLicenceType);

			module.LM_LicenceType = LicenceTypes.Codes.SRU;
			Assert(module.HasLineLevelOnDemandLicenceType);

			module.LM_LicenceType = LicenceTypes.Codes.OTM;
			Assert(module.HasLineLevelOnDemandLicenceType);

			module.LM_LicenceType = LicenceTypes.Codes.REN;
			Assert(!module.HasLineLevelOnDemandLicenceType);

			module.LM_LicenceType = LicenceTypes.Codes.OPN;
			Assert(!module.HasLineLevelOnDemandLicenceType);
		}

		public void TestIsEnabledSetToFalseShouldClearLicenceDetails()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			Factory.Save();
			Assert("No Save Errors", testHeader.NotificationsIncludingChildren.GetErrors().Count() == 0);

			LicenceDatabase db = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(db);
			db.LD_Product = ProductTypes.Codes.Enterprise;
			db.LD_OA_SoftwareInstallAddressDetails = testHeader.Addresses[0].PK;

			licHeader.Modules[0].LM_LicenceType = LicenceTypes.Codes.PUR;
			licHeader.Modules[1].LM_Calc_IsEnabled = true;

			AssertEquals("Module 1 LicenceType", LicenceTypes.Codes.PUR, licHeader.Modules[1].LM_LicenceType);

			licHeader.Modules[1].LM_Calc_IsEnabled = false;
			AssertEquals("Module 1 LicenceType", LicenceTypes.Codes.NON, licHeader.Modules[1].LM_LicenceType);

			licHeader.Modules[1].RunPreSaveValidation();
			AssertEquals("HasErrors", false, licHeader.Modules[1].HasErrors);
		}

		[TestDate(2012, 1, 1)]
		public void TestOnCreateAutoAdminLog()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			database.LD_ServerCode = "DSC";
			Factory.Save();

			var licHeader = testHeader.LicCompany.GetHeader(database);
			Factory.Save();
			var licHeaderLogCountOnSave = licHeader.Logs.GetAllLogs().Count;

			LicenceModules module = licHeader.Modules.AddNew();
			module.LM_GroupModuleCode = "---";
			module.LM_LicenceType = LicenceTypes.Codes.NON;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			Factory.Save();
			AssertEquals("No logs should exist for adding a module", 0, module.Logs.GetAllLogs().Count);
			AssertEquals("No logs should exist for adding a module", licHeaderLogCountOnSave, licHeader.Logs.GetAllLogs().Count);

			module.LM_UserCount = 15;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			Factory.Save();

			AssertLogReference(licHeader, licHeaderLogCountOnSave + 1, "Server DSC Module " + module.LM_Calc_GroupModuleDescription
				+ ": User Limit: 15(0)");
			var log1 = licHeader.Logs.MostRecentLogByEventTime(Events.EditedARecord);

			module.LM_LicenceType = LicenceTypes.Codes.PUR;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			Factory.Save();
			AssertLogReference(licHeader, licHeaderLogCountOnSave + 2, "Server DSC Module " + module.LM_Calc_GroupModuleDescription
				+ ": Licence Type: " + LicenceTypes.Codes.PUR + "(" + LicenceTypes.Codes.NON + ")");
			var log2 = licHeader.Logs.MostRecentLogByEventTime(Events.EditedARecord);
			AssertEquals("event time", log1.SL_EventTime.AddMinutes(1), log2.SL_EventTime);

			ZDateTime firstDate = new ZDateTime(2004, 1, 1, 1, 1, 1);
			ZDateTime secondDate = new ZDateTime(2005, 1, 1, 1, 1, 1);

			module.LM_ExpiryDate = firstDate;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			Factory.Save();
			AssertLogReference(licHeader, licHeaderLogCountOnSave + 3, "Server DSC Module " + module.LM_Calc_GroupModuleDescription
				+ ": Expiry Date: " + firstDate.ToShortDateString() + "()");

			module.LM_ExpiryDate = secondDate;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			Factory.Save();
			AssertLogReference(licHeader, licHeaderLogCountOnSave + 4, "Server DSC Module " + module.LM_Calc_GroupModuleDescription
				+ ": Expiry Date: " + secondDate.ToShortDateString() + "(" + firstDate.ToShortDateString() + ")");

			module.LM_ExpiryDate = ZDateTime.Empty;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			Factory.Save();
			AssertLogReference(licHeader, licHeaderLogCountOnSave + 5, "Server DSC Module " + module.LM_Calc_GroupModuleDescription
				+ ": Expiry Date: (" + secondDate.ToShortDateString() + ")");

			module.LM_GroupModuleCode = "YYY";
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			Factory.Save();
			AssertEquals("Number of logs should not have changed", licHeaderLogCountOnSave + 5, licHeader.Logs.GetAllLogs().Count);
			AssertEquals("No logs should exist for module", 0, module.Logs.GetAllLogs().Count);

			module.LM_LA = ZGuid.Empty;
			module.LM_UserCount = 1;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			Factory.Save();
			AssertLogReference(module, 1, "Server N/A Module " + module.LM_Calc_GroupModuleDescription
				+ ": User Limit: 1(15)");
			AssertEquals("Number of logs on previous Licence Header should not have changed", licHeaderLogCountOnSave + 5, licHeader.Logs.GetAllLogs().Count);
		}

		public void TestReadOnlySecurity()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed;

			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase db = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(db);
			db.LD_Product = ProductTypes.Codes.Enterprise;
			foreach (LicenceModules module in licHeader.Modules)
			{
				module.LM_Calc_IsEnabled = true;
				module.LM_LicenceType = LicenceTypes.Codes.REN;
			}

			try
			{
				string[] propertyNamesToExcept = Array.Empty<string>();
				EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed = true;
				AssertPropertyInfosReadOnly(licHeader.Modules[0], false, propertyNamesToExcept);

				propertyNamesToExcept = Array.Empty<string>();
				EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed = false;
				AssertPropertyInfosReadOnly(licHeader.Modules[0], true, propertyNamesToExcept);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed = oldValue;
			}
		}

		public void TestCopyTransientPropertiesFrom()
		{
			LicenceModules module1 = Factory.New<LicenceModules>();
			module1.LM_LicenceType = LicenceTypes.Codes.NON;
			AssertEquals(false, module1.LM_Calc_IsEnabled);

			LicenceModules module2 = Factory.New<LicenceModules>();
			module2.CopyTransientPropertiesFrom(module1);
			AssertEquals(false, module2.LM_Calc_IsEnabled);

			LicenceModules module3 = Factory.New<LicenceModules>();
			module3.LM_LicenceType = LicenceTypes.Codes.ODM;
			AssertEquals(true, module3.LM_Calc_IsEnabled);
			module2.CopyTransientPropertiesFrom(module3);
			AssertEquals(true, module2.LM_Calc_IsEnabled);
		}

		#region Implementation

		void AssertLogReference(EnterpriseBusinessObject bizObj, int expectedCount, ZString expectedLastLogReference)
		{
			AssertEquals("Number of logs that should exist", expectedCount, bizObj.Logs.GetAllLogs().Count);
			AssertEquals("Last added log should have reference", expectedLastLogReference, bizObj.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);
		}

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

		protected override BusinessObject GetNewBusinessObject()
		{
			EDIOrgHeader testHeader = Factory.New<EDIOrgHeader>();
			testHeader.OH_Code = "XYZABC";
			testHeader.MainAddress.OA_Address1 = "Address1";
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase db = testHeader.LicCompany.LicDatabases.AddNew();
			db.LD_Product = ProductTypes.Codes.Enterprise;
			var licHeader = testHeader.LicCompany.GetHeader(db);
			return licHeader.Modules[0];
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			EDIOrgHeader testHeader = factory.New<EDIOrgHeader>();
			testHeader.OH_Code = "XYZABC";
			testHeader.MainAddress.OA_Address1 = "Address1";
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase db = testHeader.LicCompany.LicDatabases.AddNew();
			db.LD_Product = ProductTypes.Codes.Enterprise;
			var licHeader = testHeader.LicCompany.GetHeader(db);
			return licHeader.Modules[0];
		}

		#endregion
	}
}
