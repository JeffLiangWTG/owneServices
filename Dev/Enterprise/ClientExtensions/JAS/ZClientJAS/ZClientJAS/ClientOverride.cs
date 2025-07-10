using System;
using System.Collections.Generic;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Client.JAS;
using Enterprise.Client.JAS.Business;
using Enterprise.Client.JAS.Business.AWB;
using Enterprise.Client.JAS.Business.Cognos;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Client.JAS.Module;
using Enterprise.Core.Environment;
using Enterprise.Core.Modules;
using Enterprise.CustomerService.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
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

		#region IClientHook Members

		protected override void InitialiseCore()
		{
			base.InitialiseCore();
			JASPredefinedNoteTypes.RegisterThisSubTypeOverride();
			DocJASForwardingShipment.RegisterThisSubTypeOverride();
		}

		public override string ClientDisplayName
		{
			get { return "JAS"; }
		}

		public override Clients Client
		{
			get { return Clients.JAS; }
		}

		#region New Security Checkpoints

		public override void AddClientSpecificSecurityCheckpointsToSecurityInstance(IZSecurity securityInstance)
		{
			JASSecurityCheckpoints jasSecurityCheckpoints = new JASSecurityCheckpoints();
			jasSecurityCheckpoints.AddJASSpecificSecurityCheckpoint(securityInstance);
			jasSecurityCheckpoints.AddExportPrematchingTransactionsSecurityCheckpoint(securityInstance);
			jasSecurityCheckpoints.AddImportMatchedTransactionsSecurityCheckpoint(securityInstance);
			jasSecurityCheckpoints.AddImportJXCFileSecurityCheckpoint(securityInstance);
			jasSecurityCheckpoints.AddExportCognosFileSecurityCheckpoint(securityInstance);
		}

		#endregion

		#region NewClientModulesCore

		protected override NewClientModuleInfo[] NewClientModulesCore
		{
			get
			{
				if (fNewClientModulesCore == null)
				{
					fNewClientModulesCore = new[]
					{
						new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.IDs.JASSpecific, new ModuleInfo(ClientModuleRegistration.ExportPrematchingTransactions, "ZClientJAS", "Enterprise.Client.JAS.Module.PreMatchingExportModule")),
						new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.IDs.JASSpecific, new ModuleInfo(ClientModuleRegistration.ImportMatchedTransactions, "ZClientJAS", "Enterprise.Client.JAS.Module.MatchedTransactionImportModule")),
						new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.IDs.JASSpecific, new ModuleInfo(ClientModuleRegistration.ExportCognosCsv, "ZClientJAS", "Enterprise.Client.JAS.Module.CognosCsvExportModule")),
						new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Operations, ClientModuleRegistration.IDs.JASSpecific, new ModuleInfo(ClientModuleRegistration.ImportJXCFile, "ZClientJAS", "Enterprise.Client.JAS.Module.JXCImportModule"))
					};
				}
				return fNewClientModulesCore;
			}
		}

		NewClientModuleInfo[] fNewClientModulesCore;

		#endregion

		#region NewClientControllersCore

		protected override ControllerInfo[] NewClientControllersCore
		{
			get
			{
				if (fNewClientControllersCore == null)
				{
					fNewClientControllersCore = new ControllerInfo[4];
					fNewClientControllersCore[0] = new ControllerInfo(ClientControllerRegistration.ExportAccountingDataForPreMatching, "ZClientJAS", "Enterprise.Client.JAS.Module.PreMatchingExportController");
					fNewClientControllersCore[1] = new ControllerInfo(ClientControllerRegistration.ImportMatchedTransactions, "ZClientJAS", "Enterprise.Client.JAS.Module.MatchedTransactionImportController");
					fNewClientControllersCore[2] = new ControllerInfo(ClientControllerRegistration.ExportCognosCsv, "ZClientJAS", "Enterprise.Client.JAS.Module.CognosCsvExportController");
					fNewClientControllersCore[3] = new ControllerInfo(ClientControllerRegistration.ImportJXCFile, "ZClientJAS", "Enterprise.Client.JAS.Module.JXCImportController");
				}
				return fNewClientControllersCore;
			}
		}

		ControllerInfo[] fNewClientControllersCore;

		#endregion

		#region ControllerOverrides

		protected override ControllerOverrides GetControllerOverrides()
		{
			ClientOverrideControllerID consolID = new ClientOverrideControllerID(ControllerIDs.JobConsol);
			ClientOverrideControllerInfo consolControllerInfo = new ClientOverrideControllerInfo(
				consolID, typeof(JASJobConsolController).Assembly.FullName,
				typeof(JASJobConsolController).FullName);

			ClientOverrideControllerID shipmentID = new ClientOverrideControllerID(ControllerIDs.JobShipment);
			ClientOverrideControllerInfo shipmentControllerInfo = new ClientOverrideControllerInfo(
				shipmentID, typeof(JASJobShipmentController).Assembly.FullName,
				typeof(JASJobShipmentController).FullName);

			ClientOverrideControllerID aRInvoiceID = new ClientOverrideControllerID(ControllerIDs.ARInvoice);
			ClientOverrideControllerInfo aRInvoiceControllerInfo = new ClientOverrideControllerInfo(
				aRInvoiceID, typeof(JASARInvoiceController).Assembly.FullName,
				typeof(JASARInvoiceController).FullName);

			ClientOverrideControllerID aRCreditNoteID = new ClientOverrideControllerID(ControllerIDs.ARCreditNote);
			ClientOverrideControllerInfo aRCreditNoteControllerInfo = new ClientOverrideControllerInfo(
				aRCreditNoteID, typeof(JASARCreditNoteController).Assembly.FullName,
				typeof(JASARCreditNoteController).FullName);

			ClientOverrideControllerID aRAdjustmentNoteID = new ClientOverrideControllerID(ControllerIDs.ARAdjustmentNote);
			ClientOverrideControllerInfo aRAdjustmentNoteControllerInfo = new ClientOverrideControllerInfo(
				aRAdjustmentNoteID, typeof(JASARAdjustmentNoteController).Assembly.FullName,
				typeof(JASARAdjustmentNoteController).FullName);

			ClientOverrideControllerID accGLAccountDescriptorID = new ClientOverrideControllerID(ControllerIDs.AccGLAccountDescriptor);
			ClientOverrideControllerInfo accGLAccountDescriptorControllerInfo = new ClientOverrideControllerInfo(
				accGLAccountDescriptorID, typeof(CognosAccGLAccountDescriptorController).Assembly.FullName,
				typeof(CognosAccGLAccountDescriptorController).FullName);

			var controllerOverrides = new ControllerOverrides();
			controllerOverrides.AddControllerOverride(consolControllerInfo);
			controllerOverrides.AddControllerOverride(shipmentControllerInfo);
			controllerOverrides.AddControllerOverride(aRInvoiceControllerInfo);
			controllerOverrides.AddControllerOverride(aRCreditNoteControllerInfo);
			controllerOverrides.AddControllerOverride(aRAdjustmentNoteControllerInfo);
			controllerOverrides.AddControllerOverride(accGLAccountDescriptorControllerInfo);
			return controllerOverrides;
		}

		#endregion

		#region ClientTypeDeciders

		public override ITypeDeciderDictionary ClientTypeDeciders
		{
			get
			{
				if (fClientTypeDeciders == null)
				{
					Dictionary<Type, ITypeDecider> hashtable = new Dictionary<Type, ITypeDecider>();
					hashtable.Add(typeof(ForwardingShipment), new TypeDeciderImpl(typeof(JASForwardingShipment)));
					hashtable.Add(typeof(ForwardingConsol), new TypeDeciderImpl(typeof(JASForwardingConsol)));
					hashtable.Add(typeof(OrgHeader), new TypeDeciderImpl(typeof(JASOrgHeader)));
					hashtable.Add(typeof(ConsolExportAWBHeader), new TypeDeciderImpl(typeof(JASConsolExportAWBHeader)));
					hashtable.Add(typeof(ShipmentExportAWBHeader), new TypeDeciderImpl(typeof(JASShipmentExportAWBHeader)));
					hashtable.Add(typeof(ForwardingContainer), new TypeDeciderImpl(typeof(ForwardingContainer)));
					hashtable.Add(typeof(ForwardingPackLine), new TypeDeciderImpl(typeof(JASForwardingPackLine)));
					hashtable.Add(typeof(Job), new TypeDeciderImpl(typeof(JASJob)));
					hashtable.Add(typeof(ARInvoice), new TypeDeciderImpl(typeof(JASARInvoice)));
					hashtable.Add(typeof(ARCreditNote), new TypeDeciderImpl(typeof(JASARCreditNote)));
					hashtable.Add(typeof(ARAdjustmentNote), new TypeDeciderImpl(typeof(JASARAdjustmentNote)));
					hashtable.Add(typeof(AccGLAccountDescriptor), new TypeDeciderImpl(typeof(CognosAccGLAccountDescriptor)));
					hashtable.Add(typeof(OrgDebtorGroup), new TypeDeciderImpl(typeof(JASOrgDebtorGroup)));
					hashtable.Add(typeof(OrgCreditorGroup), new TypeDeciderImpl(typeof(JASOrgCreditorGroup)));
					fClientTypeDeciders = new TypeDeciderDictionary(hashtable);
				}
				return fClientTypeDeciders;
			}
		}

		ITypeDeciderDictionary fClientTypeDeciders;

		#endregion

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return JASDataRegistry.Instance; }
		}

		#region DbSchemaUpgradeInfo

		protected override ITableSchema[] GetTableSchemas()
		{
			return new ITableSchema[]
			{
				ClientAccTransactionHeaderWithJobInfoSchema.Instance,
				ClientCognosAccGLAccountDescriptorExtraInfoSchema.Instance,
				ClientCognosSubClassificationAccountCreditorMappingSchema.Instance,
				ClientCognosSubClassificationAccountDebtorMappingSchema.Instance,
				ClientCognosGroupingFlagsSchema.Instance
			};
		}

		public override IExtensionObjects DbSchemaExtensionObjects => fDbSchemaUpgradeInfo ?? (fDbSchemaUpgradeInfo = new JASClientDbSchemaUpgradeInfo());

		IExtensionObjects fDbSchemaUpgradeInfo;

		public void SetDbSchemaUpgradeInfoForTest(IExtensionObjects schemaUpgradeInfo)
		{
			fDbSchemaUpgradeInfo = schemaUpgradeInfo;
		}

		#endregion

		#region ModuleOverrides

		protected override ModuleOverrides GetModuleOverrides()
		{
			ClientOverrideModuleIdentifier accGLAccountDescriptorID = new ClientOverrideModuleIdentifier(ModuleIDs.AccGLAccountDescriptor);
			ClientOverrideModuleInfo accGLAccountDescriptorModuleInfo = new ClientOverrideModuleInfo(accGLAccountDescriptorID, typeof(CognosAccGLAccountDescriptorModule).Assembly.FullName, typeof(CognosAccGLAccountDescriptorModule).FullName);

			var moduleOverrides = new ModuleOverrides();
			moduleOverrides.AddModuleOverride(accGLAccountDescriptorModuleInfo);
			return moduleOverrides;
		}

		#endregion

		#region NewModuleSectionsToAddForClientCore

		protected override IModuleSectionAddOn[] NewModuleSectionsToAddForClientCore
		{
			get
			{
				if (fNewModuleSectionsToAddForClientCore == null)
				{
					fNewModuleSectionsToAddForClientCore = new ModuleSectionAddOn[]
					{
						new ModuleSectionAddOn(ModuleTreeLoaderConstant.Category.Operations.Name,
							ClientModuleRegistration.IDs.JASSpecific,
							(NoResString)ClientModuleRegistration.Names.JASSpecific,
							IncidentApproval.DefaultClientSpecificMenuSection,
							JASSecurityCheckpoints.JASSpecific,
							IconTypes.Dollar,
							IconTypes.Dollar20x16,
							ClientModuleRegistration.Subcategory.JAS)
					};
				}
				return fNewModuleSectionsToAddForClientCore;
			}
		}

		IModuleSectionAddOn[] fNewModuleSectionsToAddForClientCore;

		#endregion

		#endregion
	}
}

#region Implementation
#endregion
