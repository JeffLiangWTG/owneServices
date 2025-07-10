using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Client.UPE;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.DataImport;
using Enterprise.Client.UPE.GUI;
using Enterprise.Client.UPE.GUI.DataImport;
using Enterprise.Client.UPE.Module;
using Enterprise.Core.Modules;
using Enterprise.CustomerService.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.ExportManifest.GUI;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client
{
	public class ClientOverride : ClientHook
	{
		public static ClientOverride Instance
		{
			get { return instance ?? (instance = new ClientOverride()); }
		}
		[ThreadStatic]
		static ClientOverride instance;

		#region Document Engine Code Description Pair Providers

		public override Dictionary<string, Type> DocumentEngineCodeDescriptionPairProviders
		{
			get
			{
				Dictionary<string, Type> result = new Dictionary<string, Type>();
				if (ShouldIgnoreUPECustomisations) { return result; }
				result.Add("zones", typeof(UPETransportZonesCodeDescriptionPairProvider));
				return result;
			}
		}
		#endregion

		#region IClientHook members

		public override string ClientDisplayName
		{
			get { return "UPS Express"; }
		}

		public override Clients Client
		{
			get { return Clients.UPE; }
		}

		public override bool HasCompanySpecificOverrides => true;

		public override bool IsCorrectCompanyForOverrides(Guid companyPK)
		{
			return UPEDataRegistry.Instance.EnableUPECustomisationsItem.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
		}

#if DEBUG
		public override IClientSpecificArchiveManagerHelper GetClientSpecificArchiveManagerHelper()
		{
			return new UPESpecificArchiveManagerHelper();
		}
#endif
		public override void DeleteClientSpecificOrphans(object logger)
		{
			var clientLogger = (IArchiveLogger)logger;
			string sqlText = @"IF EXISTS (SELECT null FROM sys.tables WHERE name = 'clientPrintBatch')
                            	BEGIN
                                Delete from clientPrintBatch where T7_PK not in (Select  T6_T7 from ClientPrintBatchItem)
                                END";
			Db.Connection.ExecuteNonQuery(sqlText);

			clientLogger.LogInfo(string.Format(CultureInfo.InvariantCulture, "Deleted orphan records from: clientPrintBatch"));
		}

		public override void SetupClientSpecificRelationships(object systemSetup)
		{
			var clientSystemSetup = (IArchiveSystemSetup)systemSetup;
			clientSystemSetup.AddRelationship(CusHAWBSchema.PK, ClientBISIShipmentHeaderSchema.T8_CS);
			clientSystemSetup.AddRelationship(ClientBISIShipmentHeaderSchema.PK, ClientBISIShipmentChargeSchema.T9_T8);
			clientSystemSetup.AddRelationship(CusHAWBSchema.PK, ClientPrintBatchItemSchema.T6_ParentID);
			clientSystemSetup.AddRelationship(JobDeclarationSchema.PK, ClientPrintBatchItemSchema.T6_ParentID);
			clientSystemSetup.AddRelationship(JobDeclarationSchema.PK, ClientOrgRematchSchema.T5_JE);
			clientSystemSetup.AddRelationship(JobDeclarationSchema.PK, ClientRefundSchema.T10_JE);
			clientSystemSetup.AddRelationship(CusHAWBSchema.PK, ClientRefundSchema.T10_CS);
			clientSystemSetup.AddRelationship(JobDeclarationSchema.JE_HouseBill, ClientXPLDUploadLogSchema.U3_TrackingNumber);
			clientSystemSetup.AddRelationship(JobDeclarationSchema.JE_DeclarationReference, ClientXPLDUploadLogSchema.U3_TrackingNumber);
			clientSystemSetup.AddRelationship(JobDeclarationSchema.JE_AgentsReference, ClientXPLDUploadLogSchema.U3_TrackingNumber);
			clientSystemSetup.AddRelationship(CusHAWBSchema.CS_HAWB, ClientXPLDUploadLogSchema.U3_TrackingNumber);
			clientSystemSetup.AddRelationship(AsycudaBillSchema.ABL_BillNumber, ClientXPLDUploadLogSchema.U3_TrackingNumber);
		}

		protected override void InitialiseCore()
		{
			if (RegistryEnableUPECustomisations)
			{
				UPEAirCargoShipmentMenu.RegisterThisTypeOverride();
				UPEAirCargoMasterMenu.RegisterThisTypeOverride();
				UPESimilarOrgMatchForApproval.RegisterThisSubTypeOverride();
				UPEPredefinedNoteTypes.RegisterThisSubTypeOverride();
				UPEDocDeclaration.RegisterThisSubTypeOverride();
				UPEActiveProcessQueue.RegisterThisSubTypeOverride();
				RateDataImporter.RegisterType(typeof(UPERateDataImporter));
			}
		}

		protected override void UninitialiseCore()
		{
			base.UninitialiseCore();
			clientTypeDeciders = null;
			fNewClientControllersCore = null;
			fNewClientModulesCore = null;
		}

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get
			{
				var dataRegistryInstance = UPEDataRegistry.Instance;
				bool isUserUpeEnabled = GlbStaff.CurrentUser.IsSupportUser;

				if (!isUserUpeEnabled)
				{
					if (GlbStaff.CurrentUser.HomeBranch != null && GlbStaff.CurrentUser.HomeBranch.Company != null)
					{
						isUserUpeEnabled = dataRegistryInstance.EnableUPECustomisationsItem.GetValueWithoutFallback(GlbStaff.CurrentUser.HomeBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
					}

					if (!isUserUpeEnabled || !RegistryEnableUPECustomisations)
					{
						dataRegistryInstance = null;
					}
				}

				return dataRegistryInstance;
			}
		}

		protected override ITableSchema[] GetTableSchemas()
		{
			return new ITableSchema[]
			{
				ClientPrintBatchSchema.Instance,
				ClientPrintBatchItemSchema.Instance,
				ClientOrgRematchSchema.Instance,
				ClientADPScoringSchema.Instance,
				ClientBISIShipmentHeaderSchema.Instance,
				ClientBISIShipmentChargeSchema.Instance,
				ClientRefundSchema.Instance,
				ClientPWSHeaderSchema.Instance,
				ClientPWSChargeSchema.Instance,
				ClientXPLDUploadLogSchema.Instance
			};
		}

		#endregion

		#region NewClientModules

		protected override NewClientModuleInfo[] NewClientModulesCore
		{
			get
			{
				string categoryName = ModuleTreeLoaderConstant.Category.Operations.Name;

				if (fNewClientModulesCore == null)
				{
					var list = new List<NewClientModuleInfo>();
					if (ShouldIgnoreUPECustomisations)
					{ return fNewClientModulesCore = Array.Empty<NewClientModuleInfo>(); }
					list.Add(new NewClientModuleInfo(categoryName, UPESection, new ModuleInfo(ClientModuleRegistration.JobAirSailing, typeof(Freight.Module.JobAirSailingModule))));
					list.Add(new NewClientModuleInfo(categoryName, UPESection, new ModuleInfo(ClientModuleRegistration.HouseAirCargo, typeof(UPEAirCargoModule), "AU")));
					list.Add(new NewClientModuleInfo(categoryName, UPESection, new ModuleInfo(ClientModuleRegistration.AirCargo, typeof(UPEAirCargoConsolReportingModule), "AU")));
					list.Add(new NewClientModuleInfo(categoryName, UPESection, new ModuleInfo(ClientModuleRegistration.Organisation, typeof(MasterFiles.Module.OrganisationModule))));
					list.Add(new NewClientModuleInfo(categoryName, UPESection, new ModuleInfo(ClientModuleRegistration.OrgMatchApproval, typeof(MasterFiles.Module.OrgMatchApprovalModule))));
					list.Add(new NewClientModuleInfo(categoryName, UPESection, new ModuleInfo(ClientModuleRegistration.ImportClassification, typeof(Customs.AU.Module.ImportClassificationModule))));
					list.Add(new NewClientModuleInfo(categoryName, UPESection, new ModuleInfo(ClientModuleRegistration.Allocation, typeof(AllocationModule))));
					list.Add(new NewClientModuleInfo(categoryName, UPESection, new ModuleInfo(ClientModuleRegistration.SupplierPart, typeof(Customs.AU.Module.OrgSupplierPartModule))));
					list.Add(new NewClientModuleInfo(categoryName, UPESection, new ModuleInfo(ClientModuleRegistration.JobDeclaration, typeof(UPEJobDeclarationModule))));
					list.Add(new NewClientModuleInfo(categoryName, UPESection, new ModuleInfo(ClientModuleRegistration.Callout, typeof(CalloutModule))));
					list.Add(new NewClientModuleInfo(categoryName, UPESection, new ModuleInfo(ClientModuleRegistration.Enquiry, typeof(EnquiryModule))));
					list.Add(new NewClientModuleInfo(categoryName, UPESection, new ModuleInfo(ClientModuleRegistration.Checkout, typeof(CheckoutModule))));
					list.Add(new NewClientModuleInfo(categoryName, UPESection, new ModuleInfo(ClientModuleRegistration.DogHitXRay, typeof(DogHitXRayModule))));
					list.Add(new NewClientModuleInfo(categoryName, UPESection, new ModuleInfo(ClientModuleRegistration.BatchPrinting, typeof(UPEPrintBatchModule))));
					list.Add(new NewClientModuleInfo(categoryName, UPESection, new ModuleInfo(ClientModuleRegistration.Reports, typeof(UPEReportsModule))));
					list.Add(new NewClientModuleInfo(categoryName, UPESection, new ModuleInfo(ClientModuleRegistration.Dashboard, typeof(DashboardModule))));
					fNewClientModulesCore = list.ToArray();
				}
				return fNewClientModulesCore;
			}
		}
		NewClientModuleInfo[] fNewClientModulesCore;

		protected override ModuleIdentifier[] NewReportModulesCore
		{
			get => new [] { ClientModuleRegistration.Reports };
		}

		#endregion

		#region NewClientControllers

		protected override ControllerInfo[] NewClientControllersCore
		{
			get
			{
				if (fNewClientControllersCore == null)
				{
					ArrayList list = new ArrayList();
					if (ShouldIgnoreUPECustomisations)
					{ return fNewClientControllersCore = (ControllerInfo[])list.ToArray(typeof(ControllerInfo)); }
					list.Add(new ControllerInfo(ClientControllerRegistration.AirCargo, typeof(UPEAirCargoController)));
					list.Add(new ControllerInfo(ClientControllerRegistration.AirCargoConsol, typeof(UPEAirCargoConsolController)));
					list.Add(new ControllerInfo(ClientControllerRegistration.JobDeclaration, typeof(UPEJobDeclarationController)));
					list.Add(new ControllerInfo(ClientControllerRegistration.Callout, typeof(CalloutController)));
					list.Add(new ControllerInfo(ClientControllerRegistration.Enquiry, typeof(EnquiryController)));
					list.Add(new ControllerInfo(ClientControllerRegistration.Checkout, typeof(CheckoutController)));
					list.Add(new ControllerInfo(ClientControllerRegistration.DogHitXRay, typeof(DogHitXRayController)));
					list.Add(new ControllerInfo(ClientControllerRegistration.Allocation, typeof(AllocationController)));
					list.Add(new ControllerInfo(ClientControllerRegistration.Dashboard, typeof(DashboardController)));
					fNewClientControllersCore = (ControllerInfo[])list.ToArray(typeof(ControllerInfo));
				}
				return fNewClientControllersCore;
			}
		}
		ControllerInfo[] fNewClientControllersCore;

		#endregion

		#region NewModuleSections

		protected override IModuleSectionAddOn[] NewModuleSectionsToAddForClientCore
		{
			get
			{
				List<IModuleSectionAddOn> sections = new List<IModuleSectionAddOn>();
				if (ShouldIgnoreUPECustomisations)
				{ return sections.ToArray(); }
				sections.Add(new ModuleSectionAddOn(ModuleTreeLoaderConstant.Category.Operations.Name, UPESection, (NoResString)"&UPS Express", IncidentApproval.DefaultClientSpecificMenuSection, Env.Security.OrgMatchApproval, IconTypes.ClientUPS, IconTypes.ClientUPS20x16, ClientModuleRegistration.Subcategory.UPE));
				return sections.ToArray();
			}
		}

		const string UPESection = "UPESection";

		#endregion

		#region Module / Controller Overrides

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();
			if (ShouldIgnoreUPECustomisations)
			{ return moduleOverrides; }
			ClientOverrideModuleIdentifier exportID = new ClientOverrideModuleIdentifier(ModuleIDs.Customs.AU.ExportCustomsManifest);
			ClientOverrideModuleInfo manifestModuleInfo = new ClientOverrideModuleInfo(exportID, typeof(UPEExportCustomsManifestModule).Assembly.FullName, typeof(UPEExportCustomsManifestModule).FullName, Core.Constants.CountryCodes.Australia);
			moduleOverrides.AddModuleOverride(manifestModuleInfo);

			ClientOverrideModuleIdentifier masterAirCargoID = new ClientOverrideModuleIdentifier(ModuleIDs.Customs.AU.AirCargo, "Consol Reporting");
			ClientOverrideModuleInfo masterAirCargoInfo = new ClientOverrideModuleInfo(masterAirCargoID, typeof(UPEAirCargoMasterModule).Assembly.FullName, typeof(UPEAirCargoMasterModule).FullName, Core.Constants.CountryCodes.Australia);
			moduleOverrides.AddModuleOverride(masterAirCargoInfo);

			ClientOverrideModuleIdentifier organisationID = new ClientOverrideModuleIdentifier(ModuleIDs.Organisation);
			ClientOverrideModuleInfo organisationInfo = new ClientOverrideModuleInfo(organisationID, typeof(UPEOrganisationModule).Assembly.FullName, typeof(UPEOrganisationModule).FullName);
			moduleOverrides.AddModuleOverride(organisationInfo);

			ClientOverrideModuleIdentifier sgAccessID = new ClientOverrideModuleIdentifier(ModuleIDs.Customs.ASYCUDA.SGAccess.Manifest);
			ClientOverrideModuleInfo sgAccessInfo = new ClientOverrideModuleInfo(sgAccessID, typeof(UPESGAccessModule).Assembly.FullName, typeof(UPESGAccessModule).FullName);
			moduleOverrides.AddModuleOverride(sgAccessInfo);
			return moduleOverrides;
		}

		protected override ControllerOverrides GetControllerOverrides()
		{
			var controllerOverrides = new ControllerOverrides();
			if (ShouldIgnoreUPECustomisations)
			{ return controllerOverrides; }
			ClientOverrideControllerID orgMatchApprovalID = new ClientOverrideControllerID(ControllerIDs.OrgMatchApproval);
			ClientOverrideControllerInfo orgMatchApprovalControllerInfo = new ClientOverrideControllerInfo(orgMatchApprovalID, typeof(UPEOrgMatchApprovalController).Assembly.FullName, typeof(UPEOrgMatchApprovalController).FullName);
			controllerOverrides.AddControllerOverride(orgMatchApprovalControllerInfo);

			ClientOverrideControllerID processQueueID = new ClientOverrideControllerID(ControllerIDs.ProcessQueue);
			ClientOverrideControllerInfo processQueueControllerInfo = new ClientOverrideControllerInfo(processQueueID, typeof(UPEProcessQueueController).Assembly.FullName, typeof(UPEProcessQueueController).FullName);
			controllerOverrides.AddControllerOverride(processQueueControllerInfo);
			return controllerOverrides;
		}

		#endregion

		public override IExtensionObjects DbSchemaExtensionObjects { get; } = new UPEClientDbSchemaUpgradeInfo();

		#region Type Deciders

		public override ITypeDeciderDictionary ClientTypeDeciders
		{
			get
			{
				if (clientTypeDeciders == null)
				{
					if (!IsInitialised)
					{ return null; } //this one is called before we finish logging in, so we need to handle it separately

					Dictionary<Type, ITypeDecider> hashtable = new Dictionary<Type, ITypeDecider>();
					if (ShouldIgnoreUPECustomisations)
					{ return clientTypeDeciders = new TypeDeciderDictionary(hashtable); }
					hashtable.Add(typeof(ManifestImporter), new TypeDeciderImpl(typeof(UPEManifestImporter)));
					hashtable.Add(typeof(ExportManifestMenu), new TypeDeciderImpl(typeof(UPEExportManifestMenu)));
					hashtable.Add(typeof(JobDeclaration), new TypeDeciderImpl(typeof(UPEJobDeclaration)));
					hashtable.Add(typeof(CusHAWB), new UPECusHAWBTypeDecider());
					hashtable.Add(typeof(JobRelatedWayBill), new TypeDeciderImpl(typeof(UPEJobRelatedWayBill)));
					hashtable.Add(typeof(ProcessQueue), new UPEProcessQueueTypeDecider());
					hashtable.Add(typeof(ProcessQueueLog), new TypeDeciderImpl(typeof(UPEProcessQueueLog)));
					hashtable.Add(typeof(RateTransportProvider), new TypeDeciderImpl(typeof(UPERateTransportProvider)));
					hashtable.Add(typeof(RateTransportZone), new TypeDeciderImpl(typeof(UPERateTransportZone)));
					hashtable.Add(typeof(CusHAWBConsigneeMatchApproval), new TypeDeciderImpl(typeof(UPECusHAWBConsigneeMatchApproval)));
					hashtable.Add(typeof(CusHAWBConsignorMatchApproval), new TypeDeciderImpl(typeof(UPECusHAWBConsignorMatchApproval)));
					hashtable.Add(typeof(CusHAWBImporterMatchApproval), new TypeDeciderImpl(typeof(UPECusHAWBImporterMatchApproval)));
					hashtable.Add(typeof(OrgHeader), new TypeDeciderImpl(typeof(UPEOrgHeader)));
					hashtable.Add(typeof(OrgMiscServ), new TypeDeciderImpl(typeof(UPEOrgMiscServ)));
					hashtable.Add(typeof(OrgCusCode), new TypeDeciderImpl(typeof(UPEOrgCusCode)));
					hashtable.Add(typeof(OrgStaffAssignments), new TypeDeciderImpl(typeof(UPEOrgStaffAssignment)));
					hashtable.Add(typeof(CusMAWB), new TypeDeciderImpl(typeof(UPECusMAWB)));
					hashtable.Add(typeof(StmNote), new TypeDeciderImpl(typeof(UPEStmNote)));
					hashtable.Add(typeof(CusEntryHeader), new TypeDeciderImpl(typeof(UPECusEntryHeader)));
					hashtable.Add(typeof(RefLocoMap), new TypeDeciderImpl(typeof(UPERefLocoMap)));
					hashtable.Add(typeof(OrgStaffAssignmentsLookupsImplementer), new TypeDeciderImpl(typeof(UPEOrgStaffAssignmentsLookupsImplementer)));
					clientTypeDeciders = new TypeDeciderDictionary(hashtable);
				}
				return clientTypeDeciders;
			}
		}
		ITypeDeciderDictionary clientTypeDeciders;

		#endregion

		bool ShouldIgnoreUPECustomisations
		{
			get
			{
				return (!IsInitialised || !RegistryEnableUPECustomisations);
			}
		}

#if DEBUG
		bool RegistryEnableUPECustomisations => IsUpgrading || (IsCorrectCompanyForOverrides_ForTest ?? UPEDataRegistry.Instance.EnableUPECustomisations);
#else
		bool RegistryEnableUPECustomisations => IsUpgrading || UPEDataRegistry.Instance.EnableUPECustomisations;
#endif

	}
}

