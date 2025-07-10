using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(LicenceModulesDependentCollection))]
	public class LicenceModulesDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestMergeWithDefaultModules()
		{
			EDIOrgHeader organisation = Factory.NewWithValidTestData<EDIOrgHeader>();
			organisation.CreateAndLoadLicenceForOrg();
			AssertNotNull("organisation has LicencedCompany", organisation.LicCompany);
			LicenceDatabase db = organisation.LicCompany.LicDatabases.AddNew();
			var licHeader = organisation.LicCompany.GetHeader(db);
			db.LD_Product = ProductTypes.Codes.Enterprise;
			db.LD_PublicEmailAddressForUpdate = organisation.Addresses[0].OA_Address1;
			int expectedModuleCount = LicenceModuleList.Instance.NamesIncludingChildren.Count;
			AssertNotEquals("expectedModuleCount", 0, expectedModuleCount);

			LicenceModulesDependentCollection collection = licHeader.Modules;
			AssertEquals("LicHeader.Modules.Count", expectedModuleCount, collection.Count);
			string expectedCollectionContents = GetStringBasedRepresentationOf(collection);

			collection.RemoveAndDeleteAll();
			collection.MergeWithDefaultModules();
			AssertEquals("LicHeader.Modules.Count", expectedModuleCount, collection.Count);
			string actualCollectionContents = GetStringBasedRepresentationOf(collection);
			AssertMultilineASCIIEquals("LicHeader.Modules contents", expectedCollectionContents, actualCollectionContents);
		}

		[ExpectNoExceptions]
		public void TestMergeWithDefaultModulesWithObsoleteModulesThatHasUsageData()
		{
			var organisation = Factory.NewWithValidTestData<EDIOrgHeader>();
			organisation.CreateAndLoadLicenceForOrg();
			var database = organisation.LicCompany.LicDatabases.AddNew();
			var licHeader = organisation.LicCompany.GetHeader(database);

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_LD = database.PK;
			clientCompany.LCC_Code = organisation.LicCompany.LC_CompanyCode;

			var clientStaff = Factory.New<ClientStaff>();
			var randomModuleWithUsageData = licHeader.Modules.AddNew();
			var clientUsage = Billing.Business.Test.BillingTestHelper.CreateEdiLicenceUsage(clientCompany, clientStaff, "ODM", randomModuleWithUsageData.LM_GroupModuleCode, new ZDateTime(2018, 6, 1));

			var collection = licHeader.Modules;
			collection.MergeWithDefaultModules();
		}

		public void TestDefaultModules_eBACCa()
		{
			EDIOrgHeader orgHeader = HeaderForTest;
			AssertNull("Precondition: TestHeader has no company", orgHeader.LicCompany);

			orgHeader.CreateAndLoadLicenceForOrg();
			AssertNotNull("Precondition: TestHeader has company", orgHeader.LicCompany);

			LicenceDatabase db = orgHeader.LicCompany.LicDatabases.AddNew();
			db.LD_Product = ProductTypes.Codes.Enterprise;
			db.LD_PublicEmailAddressForUpdate = orgHeader.Addresses[0].OA_Address1;
			var licHeader = orgHeader.LicCompany.GetHeader(db);

			LicenceModules importQuarantineMessagingModule = licHeader.Modules.FindByCode(Env.Licence.ImportQuarantineMessaging.Name);
			AssertNotNull("importQuarantineMessagingModule", importQuarantineMessagingModule);
			AssertEquals("importQuarantineMessagingModule.LM_LicenceType", LicenceTypes.Codes.CPT, importQuarantineMessagingModule.LM_LicenceType);
			AssertEquals("importQuarantineMessagingModule.LM_UserCount", ZShort.Zero, importQuarantineMessagingModule.LM_UserCount);
		}

		public void TestDefaultModules_OptionalCPT()
		{
			EDIOrgHeader orgHeader = HeaderForTest;
			AssertNull("Precondition: TestHeader has no company", orgHeader.LicCompany);

			orgHeader.CreateAndLoadLicenceForOrg();
			AssertNotNull("Precondition: TestHeader has company", orgHeader.LicCompany);

			LicenceDatabase db = orgHeader.LicCompany.LicDatabases.AddNew();
			db.LD_Product = ProductTypes.Codes.Enterprise;
			db.LD_PublicEmailAddressForUpdate = orgHeader.Addresses[0].OA_Address1;
			var licHeader = orgHeader.LicCompany.GetHeader(db);

			foreach (string code in new string[] {
				Env.Licence.OneStopSailingScheduleFeedIntegration.Name,
				Env.Licence.OneStopAUContainerIntegration.Name,
				Env.Licence.OneStopNZContainerIntegration.Name })
			{
				LicenceModules module = licHeader.Modules.FindByCode(code);
				AssertNotNull(code, module);
				AssertEquals(code + " LM_LicenceType", LicenceTypes.Codes.NON, module.LM_LicenceType);
				AssertEquals(code + " LM_UserCount", ZShort.Zero, module.LM_UserCount);
				module.LM_Calc_IsEnabled = true;
				AssertEquals(code + " LM_LicenceType", LicenceTypes.Codes.CPT, module.LM_LicenceType);
			}
		}

		public void TestDefaultModules()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			AssertNull("TestHeader has no company", testHeader.LicCompany);

			testHeader.CreateAndLoadLicenceForOrg();
			AssertNotNull("TestHeader has company", testHeader.LicCompany);
			Assert("Company contains no Databases", testHeader.LicCompany.LicDatabases.Count == 0);

			LicenceDatabase db = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(db);
			db.LD_Product = ProductTypes.Codes.Enterprise;
			db.LD_PublicEmailAddressForUpdate = testHeader.Addresses[0].OA_Address1;
			AssertEquals("Company contains 1 database", 1, testHeader.LicCompany.LicDatabases.Count);
			AssertEquals("# modules in company is correct", LicenceModuleList.Instance.NamesIncludingChildren.Count, licHeader.Modules.Count);
		}

		public void TestUpdateForChangedEdition()
		{
			LicenceHeader licHeader = Factory.New<LicenceHeader>();
			LicenceDatabase db = Factory.New<LicenceDatabase>();
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_ReleaseStatus = ReleaseRings.Codes.DPR;
			build.VersionNumber = LicenceHeader.LineLevelOnDemandLicenceVersion;
			db.LD_HL_CurrentRunningVersion = build.PK;
			db.LD_Product = ProductTypes.Codes.Enterprise;
			licHeader.LA_LD = db.PK;
			LicenceModulesDependentCollection collection = licHeader.Modules;
			AssertEquals(LicenceModuleList.Instance.NamesIncludingChildren.Count, collection.Count);

			var core = licHeader.GetCoreModule();
			LicenceModules cptModule = collection.FindByCode(Env.Licence.ImportQuarantineMessaging.Name);
			LicenceModules optionalModule = collection.FindByCode(Env.Licence.Accountant.Name);
			LicenceModules manuallyEnabledModule = collection.FindByCode(Env.Licence.NativeXMLConnector.Name);

			optionalModule.LM_LicenceType = LicenceTypes.Codes.OTM;
			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.ConcurrentCountry;
			AssertEquals("module type invalid for new edition is set to default", LicenceTypes.Codes.NON, optionalModule.LM_LicenceType);
			AssertEquals("Calc_IsEnabled called before module type set to NON", true, optionalModule.LM_Calc_IsEnabled);

			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			AssertEquals("OnDemand enabled for new licence", LicenceTypes.Codes.ODM, core.LM_LicenceType);
			AssertEquals("OnDemand enabled for new licence", LicenceTypes.Codes.ODM, optionalModule.LM_LicenceType);
			AssertEquals("OnDemand enabled for new licence", true, core.LM_Calc_IsEnabled);
			AssertEquals("OnDemand enabled for new licence", true, optionalModule.LM_Calc_IsEnabled);
			AssertEquals("CPT module type", LicenceTypes.Codes.CPT, cptModule.LM_LicenceType);
			AssertEquals("manually enabled module not automatically enabled", LicenceTypes.Codes.NON, manuallyEnabledModule.LM_LicenceType);
			AssertEquals("manually enabled module not automatically enabled", false, manuallyEnabledModule.LM_Calc_IsEnabled);

			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Advanced;
			AssertEquals("module type invalid for new edition is set to default", LicenceTypes.Codes.NON, core.LM_LicenceType);
			AssertEquals("module type invalid for new edition is set to default", LicenceTypes.Codes.NON, optionalModule.LM_LicenceType);
			AssertEquals("CPT module type", LicenceTypes.Codes.CPT, cptModule.LM_LicenceType);

			optionalModule.LM_Calc_IsEnabled = false;
			core.LM_Calc_IsEnabled = true;
			AssertEquals("optional module unchanged by enabling COR", LicenceTypes.Codes.NON, optionalModule.LM_LicenceType);
			AssertEquals("optional module unchanged by enabling COR", false, optionalModule.LM_Calc_IsEnabled);
			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			AssertEquals("module type invalid for new edition is set to default", LicenceTypes.Codes.ODM, core.LM_LicenceType);
			AssertEquals("optional module unchanged", LicenceTypes.Codes.ODM, optionalModule.LM_LicenceType);
			AssertEquals("CPT module type", LicenceTypes.Codes.CPT, cptModule.LM_LicenceType);

			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Advanced;
			AssertEquals("module type invalid for new edition is set to default", LicenceTypes.Codes.NON, core.LM_LicenceType);
			AssertEquals("module type invalid for new edition is set to default", LicenceTypes.Codes.NON, optionalModule.LM_LicenceType);
			AssertEquals("CPT module type", LicenceTypes.Codes.CPT, cptModule.LM_LicenceType);

			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			AssertEquals("module type restored to ODM", LicenceTypes.Codes.ODM, core.LM_LicenceType);
			AssertEquals("module type restored to ODM", LicenceTypes.Codes.ODM, optionalModule.LM_LicenceType);
			AssertEquals("CPT module type", LicenceTypes.Codes.CPT, cptModule.LM_LicenceType);

			// old version
			build.VersionNumber = LicenceHeader.LineLevelOnDemandLicenceVersion.AddRelease(-1);
			core.LM_Calc_IsEnabled = false;
			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Advanced;
			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			AssertEquals("OnDemand NOT enabled for new licence for old software", LicenceTypes.Codes.NON, core.LM_LicenceType);
			AssertEquals("OnDemand NOT enabled for new licence for old software", LicenceTypes.Codes.NON, optionalModule.LM_LicenceType);
			AssertEquals("CPT module type", LicenceTypes.Codes.CPT, cptModule.LM_LicenceType);

			// unknown version
			db.LD_HL_CurrentRunningVersion = ZGuid.Empty;
			core.LM_Calc_IsEnabled = false;
			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Advanced;
			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			AssertEquals("OnDemand enabled for new licence if version unknown", LicenceTypes.Codes.ODM, core.LM_LicenceType);
			AssertEquals("OnDemand enabled for new licence if version unknown", LicenceTypes.Codes.ODM, optionalModule.LM_LicenceType);
			AssertEquals("CPT module type", LicenceTypes.Codes.CPT, cptModule.LM_LicenceType);

			// ODM -> STL
			build.VersionNumber = LicenceHeader.LineLevelOnDemandLicenceVersion;
			db.LD_HL_CurrentRunningVersion = build.PK;
			core.LM_Calc_IsEnabled = true;
			optionalModule.LM_Calc_IsEnabled = false;
			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			AssertEquals("converting to STL enables optional modules", LicenceTypes.Codes.ODM, optionalModule.LM_LicenceType);
		}

		public void TestModulesReadonly()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase db = testHeader.LicCompany.LicDatabases.AddNew();
			db.LD_Product = ProductTypes.Codes.Enterprise;
			db.LD_PublicEmailAddressForUpdate = testHeader.Addresses[0].OA_Address1;
			var licHeader = testHeader.LicCompany.GetHeader(db);
			foreach (LicenceModules module in licHeader.Modules)
			{
				module.LM_Calc_IsEnabled = true;
				module.LM_LicenceType = LicenceTypes.Codes.REN;
			}
			CheckModulesReadonly(licHeader.Modules, false);
		}

		public void TestMergeWithDefaultModules_NewLicenceCheckPointsWillBeAdded()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase db = testHeader.LicCompany.LicDatabases.AddNew();
			db.LD_Product = ProductTypes.Codes.Enterprise;
			db.LD_OA_SoftwareInstallAddressDetails = testHeader.Addresses[0].PK;
			var licHeader = testHeader.LicCompany.GetHeader(db);

			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_ReleaseStatus = ReleaseRings.Codes.DPR;
			build.VersionNumber = LicenceHeader.LineLevelOnDemandLicenceVersion;
			db.LD_HL_CurrentRunningVersion = build.PK;

			licHeader.Modules[0].LM_UserCount = 10;

			string moduleCodeNotInEnvironment = licHeader.Modules[1].LM_GroupModuleCode; //pretend that the environment now has a new licence checkpoin.
			licHeader.Modules[1].Delete();

			LicenceModules newManuallyEnabledModule = licHeader.Modules.FindByCode(Env.Licence.NativeXMLConnector.Name);
			newManuallyEnabledModule.Delete();

			var coreModule = licHeader.GetCoreModule();

			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Hybrid;
			coreModule.LM_LicenceType = LicenceTypes.Codes.OTM;
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			LicenceHeader reloadedLicenceHeader = factory2.Load<LicenceHeader>(licHeader.PK);
			if (LegacyLicence.Instance.GetAllCheckpoints().Count != reloadedLicenceHeader.Modules.Count)
			{
				int n = Math.Max(LegacyLicence.Instance.GetAllCheckpoints().Count, reloadedLicenceHeader.Modules.Count);
				var cp = Env.Licence.GetAllCheckpoints().OrderBy(x => x.Name).ToArray();
				var m = reloadedLicenceHeader.Modules.Cast<LicenceModules>().OrderBy(x => x.LM_GroupModuleCode).ToArray();
				for (int i = 0; i < n; ++i)
				{
					var c1 = (i < cp.Length) ? cp[i].Name : "";
					var c2 = (i < m.Length) ? (string)(m[i].LM_GroupModuleCode) : "";
					AssertEquals(i.ToString(), c1, c2);
				}
			}
			AssertEquals("Modules count", LegacyLicence.Instance.GetAllCheckpoints().Count, reloadedLicenceHeader.Modules.Count);
			AssertEquals("Module 1 Code", moduleCodeNotInEnvironment, reloadedLicenceHeader.Modules[1].LM_GroupModuleCode);
			AssertEquals("Module 0 UserCount", 10, (int)reloadedLicenceHeader.Modules[0].LM_UserCount);
			AssertEquals("New module type", LicenceTypes.Codes.NON, reloadedLicenceHeader.Modules[1].LM_LicenceType);
			newManuallyEnabledModule = reloadedLicenceHeader.Modules.FindByCode(Env.Licence.NativeXMLConnector.Name);
			AssertEquals("New manual module not enabled", LicenceTypes.Codes.NON, newManuallyEnabledModule.LM_LicenceType);

			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			coreModule.LM_LicenceType = LicenceTypes.Codes.ODM;
			Factory.Save();
			factory2 = new BusinessObjectFactory();
			reloadedLicenceHeader = factory2.Load<LicenceHeader>(licHeader.PK);
			AssertEquals("New module type", LicenceTypes.Codes.ODM, reloadedLicenceHeader.Modules[1].LM_LicenceType);
			newManuallyEnabledModule = reloadedLicenceHeader.Modules.FindByCode(Env.Licence.NativeXMLConnector.Name);
			AssertEquals("New manual module not enabled", LicenceTypes.Codes.NON, newManuallyEnabledModule.LM_LicenceType);

			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.ConcurrentExpress;
			coreModule.LM_LicenceType = LicenceTypes.Codes.PUR;
			Factory.Save();
			factory2 = new BusinessObjectFactory();
			reloadedLicenceHeader = factory2.Load<LicenceHeader>(licHeader.PK);
			AssertEquals("New module type", LicenceTypes.Codes.NON, reloadedLicenceHeader.Modules[1].LM_LicenceType);
			newManuallyEnabledModule = reloadedLicenceHeader.Modules.FindByCode(Env.Licence.NativeXMLConnector.Name);
			AssertEquals("New manual module not enabled", LicenceTypes.Codes.NON, newManuallyEnabledModule.LM_LicenceType);

			licHeader.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Advanced;
			coreModule.LM_LicenceType = LicenceTypes.Codes.REN;
			Factory.Save();
			factory2 = new BusinessObjectFactory();
			reloadedLicenceHeader = factory2.Load<LicenceHeader>(licHeader.PK);
			AssertEquals("New module type", LicenceTypes.Codes.NON, reloadedLicenceHeader.Modules[1].LM_LicenceType);
			newManuallyEnabledModule = reloadedLicenceHeader.Modules.FindByCode(Env.Licence.NativeXMLConnector.Name);
			AssertEquals("New manual module not enabled", LicenceTypes.Codes.NON, newManuallyEnabledModule.LM_LicenceType);
		}

		public void TestMergeWithDefaultModules_OldLicenceCheckPointsWillBeDeleted()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase newDatabase = testHeader.LicCompany.LicDatabases.AddNew();
			newDatabase.LD_Product = ProductTypes.Codes.Enterprise;
			newDatabase.LD_OA_SoftwareInstallAddressDetails = testHeader.Addresses[0].PK;
			var licHeader = testHeader.LicCompany.GetHeader(newDatabase);

			licHeader.Modules[0].LM_UserCount = 10;
			LicenceModules oldModulePretendedToExist = licHeader.Modules.AddNew();
			oldModulePretendedToExist.LM_GroupModuleCode = "ZZZ";

			AssertEquals("Modules count", LegacyLicence.Instance.GetAllCheckpoints().Count + 1, licHeader.Modules.Count);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			var reloadedLicenceHeader = factory2.Load<LicenceHeader>(licHeader.PK);
			AssertEquals("Modules count", LegacyLicence.Instance.GetAllCheckpoints().Count, reloadedLicenceHeader.Modules.Count);
			AssertEquals("Module 0 UserCount", 10, (int)reloadedLicenceHeader.Modules[0].LM_UserCount);
		}

		public void TestHasLineLevelOnDemandLicenceType()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase db = testHeader.LicCompany.LicDatabases.AddNew();
			db.LD_Product = ProductTypes.Codes.Enterprise;
			db.LD_OA_SoftwareInstallAddressDetails = testHeader.Addresses[0].PK;
			var licHeader = testHeader.LicCompany.GetHeader(db);
			LicenceModulesDependentCollection modules = licHeader.Modules;

			foreach (LicenceModules module in modules)
			{
				module.LM_LicenceType = LicenceTypes.Codes.NON;
			}

			AssertEquals(false, modules.HasLineLevelOnDemandLicenceType);

			modules[0].LM_LicenceType = LicenceTypes.Codes.ODM;
			AssertEquals(true, modules.HasLineLevelOnDemandLicenceType);
			modules[0].LM_LicenceType = LicenceTypes.Codes.PUR;
			AssertEquals(false, modules.HasLineLevelOnDemandLicenceType);

			modules[modules.Count - 1].LM_LicenceType = LicenceTypes.Codes.ODM;
			AssertEquals(true, modules.HasLineLevelOnDemandLicenceType);
			modules[modules.Count - 1].LM_LicenceType = LicenceTypes.Codes.PUR;
			AssertEquals(false, modules.HasLineLevelOnDemandLicenceType);
		}

		public void TestPopulateFromQuote()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase newDatabase = testHeader.LicCompany.LicDatabases.AddNew();
			newDatabase.LD_Product = ProductTypes.Codes.Enterprise;
			newDatabase.LD_OA_SoftwareInstallAddressDetails = testHeader.Addresses[0].PK;
			var licHeader = testHeader.LicCompany.GetHeader(newDatabase);
			LicenceModulesDependentCollection modules = licHeader.Modules;

			Dictionary<string, int> userCounts = new Dictionary<string, int>();
			userCounts.Add(LegacyLicence.Codes.Core, 10);
			userCounts.Add(Env.Licence.ShippingManager.Name, 5);
			modules.PopulateFromQuote(userCounts);

			AssertEquals(10, (int)modules.FindByCode(LegacyLicence.Codes.Core).LM_UserCount);
			AssertEquals(5, (int)modules.FindByCode(Env.Licence.ShippingManager.Name).LM_UserCount);
		}

		#region Implementation

		protected override Type GetExpectedCollectionType()
		{
			return typeof(LicenceModulesDependentCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase newDatabase = testHeader.LicCompany.LicDatabases.AddNew();
			var licHeader = testHeader.LicCompany.GetHeader(newDatabase);
			return new LicenceModulesDependentCollection(licHeader, Factory);
		}

		public EDIOrgHeader HeaderForTest
		{
			get
			{
				EDIOrgHeader fHeaderForTest = Factory.NewWithValidTestData<EDIOrgHeader>();
				fHeaderForTest.OH_Code = "TGBLOG";
				fHeaderForTest.OH_RL_NKClosestPort = "AUBNE";

				OrgAddress newAddress = fHeaderForTest.Addresses.AddNew();
				newAddress.OA_Address1 = "666 Test Address";

				return fHeaderForTest;
			}
		}

		void CheckModulesReadonly(LicenceModulesDependentCollection moduleCollection, bool isReadOnly)
		{
			for (int i = 0; i < moduleCollection.Count; i++)
			{
				if (i == 0) // The first row is special and we **ALWAYS** want it to be writeable
				{
					Assert("First Row is writeable", !moduleCollection[i].LM_UserCountInfo.ReadOnly);
					Assert("First Row is writeable", !moduleCollection[i].LM_ExpiryDateInfo.ReadOnly);
					Assert("First Row is writeable", !moduleCollection[i].LM_LicenceTypeInfo.ReadOnly);
				}
				else
				{
					AssertEquals("Row " + i.ToString() + ".ReadOnly() should be " + isReadOnly.ToString(), isReadOnly, moduleCollection[i].LM_UserCountInfo.ReadOnly);
					AssertEquals("Row " + i.ToString() + ".ReadOnly() should be " + isReadOnly.ToString(), isReadOnly, moduleCollection[i].LM_ExpiryDateInfo.ReadOnly);
					AssertEquals("Row " + i.ToString() + ".ReadOnly() should be " + isReadOnly.ToString(), isReadOnly, moduleCollection[i].LM_LicenceTypeInfo.ReadOnly);
				}
			}
		}

		static string GetStringBasedRepresentationOf(LicenceModulesDependentCollection collection)
		{
			collection.Sort(LicenceModulesSchema.LM_GroupModuleCode.Name);
			ZStringBuilder result = new ZStringBuilder();
			foreach (LicenceModules module in collection)
			{
				result.Append(module.LM_GroupModuleCode + " : " + module.LM_LicenceType);
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		#endregion
	}
}
