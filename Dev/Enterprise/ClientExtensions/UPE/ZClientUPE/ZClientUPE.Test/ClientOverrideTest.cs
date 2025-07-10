using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.DataImport;
using Enterprise.Client.UPE.GUI;
using Enterprise.Client.UPE.GUI.DataImport;
using Enterprise.Client.UPE.Module;
using Enterprise.Core.Modules;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.ExportManifest.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.ReportTesting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;

namespace Enterprise.Client.UPE.Testing
{
	[TestedType(typeof(ClientOverride))]
	class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestDocumentEngineFilters()
		{
			Assert("DocumentEngine filters should have", ClientOverride.Instance.DocumentEngineCodeDescriptionPairProviders.ContainsKey("zones"));
			AssertEquals("DocumentEngine filters should have", typeof(UPETransportZonesCodeDescriptionPairProvider), ClientOverride.Instance.DocumentEngineCodeDescriptionPairProviders["zones"]);
		}

		public void TestInitialiseAndUnitialise()
		{
			bool wasInitialized = UPEAirCargoShipmentMenu.IsSubTypeRegistered;

			ClientOverride.Instance.Uninitialise();
			AssertEquals(false, UPEAirCargoShipmentMenu.IsSubTypeRegistered);
			AssertEquals(false, UPEAirCargoMasterMenu.IsSubTypeRegistered);
			AssertEquals(false, UPESimilarOrgMatchForApproval.IsSubTypeRegistered);
			AssertType(typeof(PredefinedNoteTypes), PredefinedNoteTypes.Instance);
			AssertEquals(false, UPEDocDeclaration.IsSubTypeRegistered);
			AssertEquals(false, UPEActiveProcessQueue.IsSubTypeRegistered);
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AssertEquals(typeof(RateDataImporter), RateDataImporter.New(factory.New<RatingHeader>()).GetType());

			ClientOverride.Instance.Initialise(true);
			AssertEquals(true, UPEAirCargoShipmentMenu.IsSubTypeRegistered);
			AssertEquals(true, UPEAirCargoMasterMenu.IsSubTypeRegistered);
			AssertEquals(true, UPESimilarOrgMatchForApproval.IsSubTypeRegistered);
			AssertType(typeof(UPEPredefinedNoteTypes), PredefinedNoteTypes.Instance);
			AssertEquals(true, UPEDocDeclaration.IsSubTypeRegistered);
			AssertEquals(true, UPEActiveProcessQueue.IsSubTypeRegistered);
			AssertEquals(typeof(UPERateDataImporter), RateDataImporter.New(factory.New<RatingHeader>()).GetType());

			ClientOverride.Instance.Uninitialise();
			AssertEquals(false, UPEAirCargoShipmentMenu.IsSubTypeRegistered);
			AssertEquals(false, UPEAirCargoMasterMenu.IsSubTypeRegistered);
			AssertEquals(false, UPESimilarOrgMatchForApproval.IsSubTypeRegistered);
			AssertType(typeof(PredefinedNoteTypes), PredefinedNoteTypes.Instance);
			AssertEquals(false, UPEDocDeclaration.IsSubTypeRegistered);
			AssertEquals(false, UPEActiveProcessQueue.IsSubTypeRegistered);
			AssertEquals(typeof(RateDataImporter), RateDataImporter.New(factory.New<RatingHeader>()).GetType());

			if (wasInitialized)
			{
				ClientOverride.Instance.Initialise();
			}
		}

		public void TestClientDisplayName()
		{
			AssertEquals("UPS Express", ClientOverride.Instance.ClientDisplayName);
		}

		public void TestNewClientModules()
		{
			AssertEquals("NewClientModules in correct order", ClientOverride.Instance.NewClientModules[0].ID, ClientModuleRegistration.JobAirSailing);
			AssertEquals("NewClientModules in correct order", ClientOverride.Instance.NewClientModules[1].ID, ClientModuleRegistration.HouseAirCargo);
			AssertEquals("NewClientModules in correct order", ClientOverride.Instance.NewClientModules[2].ID, ClientModuleRegistration.AirCargo);
			AssertEquals("NewClientModules in correct order", ClientOverride.Instance.NewClientModules[3].ID, ClientModuleRegistration.Organisation);
			AssertEquals("NewClientModules in correct order", ClientOverride.Instance.NewClientModules[4].ID, ClientModuleRegistration.OrgMatchApproval);
			AssertEquals("NewClientModules in correct order", ClientOverride.Instance.NewClientModules[5].ID, ClientModuleRegistration.ImportClassification);
			AssertEquals("NewClientModules in correct order", ClientOverride.Instance.NewClientModules[6].ID, ClientModuleRegistration.Allocation);
			AssertEquals("NewClientModules in correct order", ClientOverride.Instance.NewClientModules[7].ID, ClientModuleRegistration.SupplierPart);
			AssertEquals("NewClientModules in correct order", ClientOverride.Instance.NewClientModules[8].ID, ClientModuleRegistration.JobDeclaration);
			AssertEquals("NewClientModules in correct order", ClientOverride.Instance.NewClientModules[9].ID, ClientModuleRegistration.Callout);
			AssertEquals("NewClientModules in correct order", ClientOverride.Instance.NewClientModules[10].ID, ClientModuleRegistration.Enquiry);
			AssertEquals("NewClientModules in correct order", ClientOverride.Instance.NewClientModules[11].ID, ClientModuleRegistration.Checkout);
			AssertEquals("NewClientModules in correct order", ClientOverride.Instance.NewClientModules[12].ID, ClientModuleRegistration.DogHitXRay);
			AssertEquals("NewClientModules in correct order", ClientOverride.Instance.NewClientModules[13].ID, ClientModuleRegistration.BatchPrinting);
			AssertEquals("NewClientModules in correct order", ClientOverride.Instance.NewClientModules[14].ID, ClientModuleRegistration.Reports);
			AssertEquals("NewClientModules in correct order", ClientOverride.Instance.NewClientModules[15].ID, ClientModuleRegistration.Dashboard);
		}

		public void TestNewClientControllers()
		{
			AssertEquals("NewClientControllers in correct order", ClientOverride.Instance.NewClientControllers[0].ID, ClientControllerRegistration.AirCargo);
			AssertEquals("NewClientControllers in correct order", ClientOverride.Instance.NewClientControllers[1].ID, ClientControllerRegistration.AirCargoConsol);
			AssertEquals("NewClientControllers in correct order", ClientOverride.Instance.NewClientControllers[2].ID, ClientControllerRegistration.JobDeclaration);
			AssertEquals("NewClientControllers in correct order", ClientOverride.Instance.NewClientControllers[3].ID, ClientControllerRegistration.Callout);
			AssertEquals("NewClientControllers in correct order", ClientOverride.Instance.NewClientControllers[4].ID, ClientControllerRegistration.Enquiry);
			AssertEquals("NewClientControllers in correct order", ClientOverride.Instance.NewClientControllers[5].ID, ClientControllerRegistration.Checkout);
			AssertEquals("NewClientControllers in correct order", ClientOverride.Instance.NewClientControllers[6].ID, ClientControllerRegistration.DogHitXRay);
			AssertEquals("NewClientControllers in correct order", ClientOverride.Instance.NewClientControllers[7].ID, ClientControllerRegistration.Allocation);
			AssertEquals("NewClientControllers in correct order", ClientOverride.Instance.NewClientControllers[8].ID, ClientControllerRegistration.Dashboard);
		}

		public void TestModuleOverrides()
		{
			ZString initialCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			try
			{
				using (ZModule module = ZModuleFactory.Instance.Create(ModuleIDs.Customs.AU.ExportCustomsManifest))
				{
					AssertEquals(typeof(UPEExportCustomsManifestModule), module.GetType());
				}
				using (ZModule module = ZModuleFactory.Instance.Create(ModuleIDs.Organisation))
				{
					AssertEquals(typeof(UPEOrganisationModule), module.GetType());
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(initialCountryCode);
			}
		}

		public void TestModuleOverridesSG()
		{
			ZString initialCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			try
			{
				using (ZModule module = ZModuleFactory.Instance.Create(ModuleIDs.Customs.ASYCUDA.SGAccess.Manifest))
				{
					var type = module.GetType();
					AssertEquals(typeof(UPESGAccessModule), type);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(initialCountryCode);
			}
		}

		public void TestNewModuleSections()
		{
			AssertEquals("Should add UPE Client Specific section", 1, ClientOverride.Instance.NewModuleSectionsToAddForClient.Length);
			ModuleSectionAddOn section = ClientOverride.Instance.NewModuleSectionsToAddForClient[0] as ModuleSectionAddOn;
			AssertEquals(ModuleTreeLoaderConstant.Category.Operations.Name, section.CategoryName);
			AssertEquals("UPESection", section.Name);
			AssertEquals("&UPS Express", section.DisplayText);
			AssertEquals(Env.Security.OrgMatchApproval.Code, section.SecurityCheckpoint.Code);
			AssertEquals(IconTypes.ClientUPS, section.Icon);
			AssertEquals(IconTypes.ClientUPS20x16, section.GroupImage);
			AssertEquals(ClientModuleRegistration.Subcategory.UPE.Name, section.Subcategory.Name);
		}

		public void TestControllerOverrides()
		{
			ZController controller = ZControllerFactory.Create(ControllerIDs.ProcessQueue);
			AssertEquals(typeof(UPEProcessQueueController), controller.GetType());

			ZString initialCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			try
			{
				controller = ZControllerFactory.Create(ControllerIDs.OrgMatchApproval);
				AssertEquals(typeof(UPEOrgMatchApprovalController), controller.GetType());
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(initialCountryCode);
			}
		}

		public new void TestBusinessObjectsAreTypeDecidedCorrectly()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			base.TestBusinessObjectsAreTypeDecidedCorrectly();
		}

		public void TestClientTypeDeciders()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;

			BusinessObjectFactory factory = new BusinessObjectFactory();
			AssertEquals(typeof(UPEManifestImporter), ManifestImporter.GetImporter(factory, "Test").GetType());
			using (ExportManifestMenu menu = ExportManifestMenu.GetMenu())
			{
				AssertEquals(typeof(UPEExportManifestMenu), menu.GetType());
			}
			AssertEquals(typeof(UPEPredefinedNoteTypes), UPEPredefinedNoteTypes.Instance.GetType());
			AssertEquals(typeof(UPEJobDeclaration), factory.New(typeof(JobDeclaration)).GetType());
			AssertEquals(typeof(UPECusHAWBTypeDecider), ClientOverride.Instance.ClientTypeDeciders[typeof(CusHAWB)].GetType());
			AssertEquals(typeof(UPEJobRelatedWayBill), factory.New(typeof(JobRelatedWayBill)).GetType());
			AssertEquals(typeof(UPERateTransportProvider), factory.New(typeof(RateTransportProvider)).GetType());
			AssertEquals(typeof(UPERateTransportZone), factory.New(typeof(RateTransportZone)).GetType());
			AssertEquals(typeof(UPECusHAWBConsigneeMatchApproval), factory.New(typeof(CusHAWBConsigneeMatchApproval)).GetType());
			AssertEquals(typeof(UPECusHAWBConsignorMatchApproval), factory.New(typeof(CusHAWBConsignorMatchApproval)).GetType());
			AssertEquals(typeof(UPECusHAWBImporterMatchApproval), factory.New(typeof(CusHAWBImporterMatchApproval)).GetType());
			AssertEquals(typeof(UPEProcessQueueTypeDecider), ClientOverride.Instance.ClientTypeDeciders[typeof(ProcessQueue)].GetType());
			AssertEquals(typeof(UPEOrgHeader), factory.New(typeof(OrgHeader)).GetType());
			AssertEquals(typeof(UPEOrgMiscServ), factory.New(typeof(OrgMiscServ)).GetType());
			AssertEquals(typeof(UPEOrgCusCode), factory.New(typeof(OrgCusCode)).GetType());
			AssertEquals(typeof(UPEOrgStaffAssignment), factory.New(typeof(OrgStaffAssignments)).GetType());
			AssertEquals(typeof(UPEProcessQueueLog), factory.New(typeof(ProcessQueueLog)).GetType());
			AssertEquals(typeof(UPECusMAWB), factory.New(typeof(CusMAWB)).GetType());
			AssertEquals(typeof(UPEStmNote), factory.New(typeof(StmNote)).GetType());
			AssertEquals(typeof(UPECusEntryHeader), factory.New(typeof(CusEntryHeader)).GetType());
			AssertEquals(typeof(UPERefLocoMap), factory.New(typeof(RefLocoMap)).GetType());
			AssertEquals(typeof(UPEOrgStaffAssignmentsLookupsImplementer), OrgStaffAssignmentsLookupsImplementer.Get(factory).GetType());
		}

		public void TestClientsEnum()
		{
			AssertEquals(Clients.UPE, ClientOverride.Instance.Client);
		}

		public void TestRegistry()
		{
			ClientHookLoader.Instance.ClientHook.IsCorrectCompanyForOverrides_ForTest = null;
			var itemSet = ClientOverride.Instance.AdditionalRegistryItemSet as UPEDataRegistry;
			itemSet.EnableUPECustomisationsItem.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			var factory = new BusinessObjectFactory();
			var testUser = factory.NewWithValidTestData<GlbStaff>();

			var upeEnabledCompany = factory.NewWithValidTestData<GlbCompany>();
			upeEnabledCompany.GC_Name = "UPSExpress";
			var upeDisabledCompany = factory.NewWithValidTestData<GlbCompany>();
			upeDisabledCompany.GC_Name = "AnotherCompany";
			factory.Save();

			var upeEnabledBranch = factory.NewWithValidTestData<GlbBranch>();
			upeEnabledBranch.GB_Code = "ENB";
			upeEnabledBranch.GB_GC = upeEnabledCompany.PK;
			var upeDisabledBranch = factory.NewWithValidTestData<GlbBranch>();
			upeDisabledBranch.GB_Code = "DIS";
			upeDisabledBranch.GB_GC = upeDisabledCompany.PK;
			factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext("CWSupport", EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("AdditionalRegistryItemSet should exist for support users", UPEDataRegistry.Instance, ClientOverride.Instance.AdditionalRegistryItemSet);
				itemSet.EnableUPECustomisationsItem.SetValue(upeEnabledCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				itemSet.EnableUPECustomisationsItem.SetValue(upeDisabledCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(testUser.GS_LoginName, upeDisabledBranch.PK.ToGuid(), EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertNull("AdditionalRegistryItemSet should not exist - user's home branch is blank", ClientOverride.Instance.AdditionalRegistryItemSet);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(testUser.GS_LoginName, upeEnabledBranch.PK.ToGuid(), EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertNull("AdditionalRegistryItemSet should not exist - user's home branch is blank", ClientOverride.Instance.AdditionalRegistryItemSet);
			}

			testUser.GS_GB_HomeBranch = upeDisabledBranch.PK;
			factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(testUser.GS_LoginName, upeDisabledBranch.PK.ToGuid(), EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertNull("AdditionalRegistryItemSet should not exist - user's home branch belongs to UPE-disabled company", ClientOverride.Instance.AdditionalRegistryItemSet);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(testUser.GS_LoginName, upeEnabledBranch.PK.ToGuid(), EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertNull("AdditionalRegistryItemSet should not exist - user's home branch belongs to UPE-disabled company", ClientOverride.Instance.AdditionalRegistryItemSet);
			}
			testUser.GS_GB_HomeBranch = upeEnabledBranch.PK;
			factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(testUser.GS_LoginName, upeDisabledBranch.PK.ToGuid(), EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertNull("AdditionalRegistryItemSet should not exist - user is logged in to UPE-disabled company", ClientOverride.Instance.AdditionalRegistryItemSet);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(testUser.GS_LoginName, upeEnabledBranch.PK.ToGuid(), EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("Precondition: AdditionalRegistryItemSet should exist", UPEDataRegistry.Instance, ClientOverride.Instance.AdditionalRegistryItemSet);
			}
		}

		#region Refund Report

		[TemplateName("UPE Refund Report")]
		sealed class TestUPERefundReportTestCase : ClientSpecificTemplateTestCase
		{
			protected override Clients ClientCode
			{
				get { return Clients.UPE; }
			}
		}

		sealed class UPERefundReportTest : ClientSpecificReportTestCase
		{
			public override ClientSpecificTemplateTestCase GetClientSpecificTemplateTestCase()
			{
				return new TestUPERefundReportTestCase();
			}

			public override ModuleIdentifier ModuleIdToTest
			{
				get { return ClientModuleRegistration.Reports; }
			}

			public override string MenuName
			{
				get { return "UPE Refund Report"; }
			}

			public override string Hint
			{
				get { return ZString.Empty; }
			}
		}

		#endregion

		#region Implementation

		protected override Type ClientOverrideType
		{
			get { return typeof(ClientOverride); }
		}

		#endregion
	}
}
