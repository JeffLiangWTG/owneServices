using System;
using System.Collections;
using CargoWise.Data;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Client.JAS;
using Enterprise.Client.JAS.Business;
using Enterprise.Client.JAS.Business.AWB;
using Enterprise.Client.JAS.Business.Cognos;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Client.JAS.Module;
using Enterprise.Core.Modules;
using Enterprise.DocumentWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestClientDisplayName()
		{
			AssertEquals("JAS", ClientOverride.Instance.ClientDisplayName);
		}

		public void TestControllerOverrides()
		{
			string defaultCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertControllerOverrides(ControllerIDs.JobConsol, typeof(JASJobConsolController));
				AssertControllerOverrides(ControllerIDs.JobShipment, typeof(JASJobShipmentController));
				AssertControllerOverrides(ControllerIDs.ARInvoice, typeof(JASARInvoiceController));
				AssertControllerOverrides(ControllerIDs.ARCreditNote, typeof(JASARCreditNoteController));
				AssertControllerOverrides(ControllerIDs.ARAdjustmentNote, typeof(JASARAdjustmentNoteController));
				AssertControllerOverrides(ControllerIDs.AccGLAccountDescriptor, typeof(CognosAccGLAccountDescriptorController));
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.SouthAfrica);
				AssertControllerOverrides(ControllerIDs.JobConsol, typeof(JASJobConsolController));
				AssertControllerOverrides(ControllerIDs.JobShipment, typeof(JASJobShipmentController));
				AssertControllerOverrides(ControllerIDs.ARInvoice, typeof(JASARInvoiceController));
				AssertControllerOverrides(ControllerIDs.ARCreditNote, typeof(JASARCreditNoteController));
				AssertControllerOverrides(ControllerIDs.ARAdjustmentNote, typeof(JASARAdjustmentNoteController));
				AssertControllerOverrides(ControllerIDs.AccGLAccountDescriptor, typeof(CognosAccGLAccountDescriptorController));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(defaultCountry);
			}
		}

		public void TestModuleOverrides()
		{
			ZString initialCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				using (ZModule module = ZModuleFactory.Instance.Create(ModuleIDs.AccGLAccountDescriptor))
				{
					AssertEquals(typeof(CognosAccGLAccountDescriptorModule), module.GetType());
				}

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.SouthAfrica);
				using (ZModule module = ZModuleFactory.Instance.Create(ModuleIDs.AccGLAccountDescriptor))
				{
					AssertEquals(typeof(CognosAccGLAccountDescriptorModule), module.GetType());
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(initialCountryCode);
			}
		}

		public void TestClientTypeDeciders()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AssertEquals(typeof(JASForwardingShipment), factory.New(typeof(ForwardingShipment)).GetType());
			AssertEquals(typeof(JASForwardingConsol), factory.New(typeof(ForwardingConsol)).GetType());
			AssertEquals(typeof(JASOrgHeader), factory.New(typeof(OrgHeader)).GetType());
			AssertEquals(typeof(JASConsolExportAWBHeader), factory.New(typeof(ConsolExportAWBHeader)).GetType());
			AssertEquals(typeof(JASShipmentExportAWBHeader), factory.New(typeof(ShipmentExportAWBHeader)).GetType());
			AssertEquals(typeof(ForwardingContainer), factory.New(typeof(ForwardingContainer)).GetType());
			AssertEquals(typeof(JASForwardingPackLine), factory.New(typeof(ForwardingPackLine)).GetType());
			AssertEquals(typeof(JASJob), factory.NewJobForTesting<Job>().GetType());
			AssertEquals(typeof(JASARInvoice), factory.New(typeof(ARInvoice)).GetType());
			AssertEquals(typeof(JASARCreditNote), factory.New(typeof(ARCreditNote)).GetType());
			AssertEquals(typeof(JASARAdjustmentNote), factory.New(typeof(ARAdjustmentNote)).GetType());
			AssertEquals(typeof(CognosAccGLAccountDescriptor), factory.New(typeof(AccGLAccountDescriptor)).GetType());
			AssertEquals(typeof(JASOrgCreditorGroup), factory.New(typeof(OrgCreditorGroup)).GetType());
			AssertEquals(typeof(JASOrgDebtorGroup), factory.New(typeof(OrgDebtorGroup)).GetType());
		}

		public void TestClientsEnum()
		{
			AssertEquals(Clients.JAS, ClientOverride.Instance.Client);
		}

		public void NewClientModules()
		{
			AssertNotNull("NewClientModules", ClientOverride.Instance.NewClientModules);
			AssertEquals("NewClientModules's lenght", 4, ClientOverride.Instance.NewClientModules.Length);
			NewClientModuleInfo preMatchingExportModuleInfo = GetNewClientModuleInfo(ClientModuleRegistration.ExportPrematchingTransactions);
			AssertEquals("Category", ModuleTreeLoaderConstant.Category.Operations, preMatchingExportModuleInfo.CategoryName);
			AssertEquals("Section", ClientModuleRegistration.IDs.JASSpecific, preMatchingExportModuleInfo.SectionName);
			AssertEquals("ModuleID", ClientModuleRegistration.ExportPrematchingTransactions, preMatchingExportModuleInfo.ID);
			AssertEquals("InfoID", ClientModuleRegistration.ExportPrematchingTransactions, preMatchingExportModuleInfo.Info.IDForTest);
			AssertEquals("InfoAssemblyName", "ZClientJAS", preMatchingExportModuleInfo.Info.AssemblyNameForTest);
			AssertEquals("InfoClassFullName", "Enterprise.Client.JAS.Module.PreMatchingExportModule", preMatchingExportModuleInfo.Info.ClassFullNameForTest);
			AssertEquals("InfoCountryCode", "", preMatchingExportModuleInfo.Info.CountryCodeForTest);
			NewClientModuleInfo importMatchedTransactionsModuleInfo = GetNewClientModuleInfo(ClientModuleRegistration.ImportMatchedTransactions);
			AssertEquals("Category", ModuleTreeLoaderConstant.Category.Operations, importMatchedTransactionsModuleInfo.CategoryName);
			AssertEquals("Section", ClientModuleRegistration.IDs.JASSpecific, importMatchedTransactionsModuleInfo.SectionName);
			AssertEquals("ModuleID", ClientModuleRegistration.ImportMatchedTransactions, importMatchedTransactionsModuleInfo.ID);
			AssertEquals("InfoID", ClientModuleRegistration.ImportMatchedTransactions, importMatchedTransactionsModuleInfo.Info.IDForTest);
			AssertEquals("InfoAssemblyName", "ZClientJAS", importMatchedTransactionsModuleInfo.Info.AssemblyNameForTest);
			AssertEquals("InfoClassFullName", "Enterprise.Client.JAS.Module.MatchedTransactionImportModule", importMatchedTransactionsModuleInfo.Info.ClassFullNameForTest);
			AssertEquals("InfoCountryCode", "", importMatchedTransactionsModuleInfo.Info.CountryCodeForTest);
			NewClientModuleInfo exportCognosCsvModuleInfo = GetNewClientModuleInfo(ClientModuleRegistration.ExportCognosCsv);
			AssertEquals("Category", ModuleTreeLoaderConstant.Category.Operations, exportCognosCsvModuleInfo.CategoryName);
			AssertEquals("Section", ClientModuleRegistration.IDs.JASSpecific, exportCognosCsvModuleInfo.SectionName);
			AssertEquals("ModuleID", ClientModuleRegistration.ExportCognosCsv, exportCognosCsvModuleInfo.ID);
			AssertEquals("InfoID", ClientModuleRegistration.ExportCognosCsv, exportCognosCsvModuleInfo.Info.IDForTest);
			AssertEquals("InfoAssemblyName", "ZClientJAS", exportCognosCsvModuleInfo.Info.AssemblyNameForTest);
			AssertEquals("InfoClassFullName", "Enterprise.Client.JAS.Module.CognosCsvExportModule", exportCognosCsvModuleInfo.Info.ClassFullNameForTest);
			AssertEquals("InfoCountryCode", "", exportCognosCsvModuleInfo.Info.CountryCodeForTest);
			NewClientModuleInfo importJXCFileModuleInfo = GetNewClientModuleInfo(ClientModuleRegistration.ImportJXCFile);
			AssertEquals("Category", ModuleTreeLoaderConstant.Category.Operations, importJXCFileModuleInfo.CategoryName);
			AssertEquals("Section", ClientModuleRegistration.IDs.JASSpecific, importJXCFileModuleInfo.SectionName);
			AssertEquals("ModuleID", ClientModuleRegistration.ImportJXCFile, importJXCFileModuleInfo.ID);
			AssertEquals("InfoID", ClientModuleRegistration.ImportJXCFile, importJXCFileModuleInfo.Info.IDForTest);
			AssertEquals("InfoAssemblyName", "ZClientJAS", importJXCFileModuleInfo.Info.AssemblyNameForTest);
			AssertEquals("InfoClassFullName", "Enterprise.Client.JAS.Module.JXCImportModule", importJXCFileModuleInfo.Info.ClassFullNameForTest);
			AssertEquals("InfoCountryCode", "", importJXCFileModuleInfo.Info.CountryCodeForTest);
		}

		public void TestNewClientControllers()
		{
			AssertNotNull("NewClientControllers", ClientOverride.Instance.NewClientControllers);
			AssertEquals("NewClientControllers's lenght", 4, ClientOverride.Instance.NewClientControllers.Length);
			ControllerInfo preMatchingControllerInfo = GetNewClientControllerInfo(ClientControllerRegistration.ExportAccountingDataForPreMatching);
			AssertEquals("InfoID", ClientControllerRegistration.ExportAccountingDataForPreMatching, preMatchingControllerInfo.IDForTest);
			AssertEquals("InfoAssemblyName", "ZClientJAS", preMatchingControllerInfo.AssemblyNameForTest);
			AssertEquals("InfoClassFullName", "Enterprise.Client.JAS.Module.PreMatchingExportController", preMatchingControllerInfo.ClassFullNameForTest);
			AssertEquals("InfoCountryCode", "", preMatchingControllerInfo.CountryCodeForTest);
			ControllerInfo importMatchedTransactionsControllerInfo = GetNewClientControllerInfo(ClientControllerRegistration.ImportMatchedTransactions);
			AssertEquals("InfoID", ClientControllerRegistration.ImportMatchedTransactions, importMatchedTransactionsControllerInfo.IDForTest);
			AssertEquals("InfoAssemblyName", "ZClientJAS", importMatchedTransactionsControllerInfo.AssemblyNameForTest);
			AssertEquals("InfoClassFullName", "Enterprise.Client.JAS.Module.MatchedTransactionImportController", importMatchedTransactionsControllerInfo.ClassFullNameForTest);
			AssertEquals("InfoCountryCode", "", importMatchedTransactionsControllerInfo.CountryCodeForTest);
			ControllerInfo exportCognosControllerInfo = GetNewClientControllerInfo(ClientControllerRegistration.ExportCognosCsv);
			AssertEquals("InfoID", ClientControllerRegistration.ExportCognosCsv, exportCognosControllerInfo.IDForTest);
			AssertEquals("InfoAssemblyName", "ZClientJAS", exportCognosControllerInfo.AssemblyNameForTest);
			AssertEquals("InfoClassFullName", "Enterprise.Client.JAS.Module.CognosCsvExportController", exportCognosControllerInfo.ClassFullNameForTest);
			AssertEquals("InfoCountryCode", "", exportCognosControllerInfo.CountryCodeForTest);
			ControllerInfo importJXCControllerInfo = GetNewClientControllerInfo(ClientControllerRegistration.ImportJXCFile);
			AssertEquals("InfoID", ClientControllerRegistration.ImportJXCFile, importJXCControllerInfo.IDForTest);
			AssertEquals("InfoAssemblyName", "ZClientJAS", importJXCControllerInfo.AssemblyNameForTest);
			AssertEquals("InfoClassFullName", "Enterprise.Client.JAS.Module.JXCImportController", importJXCControllerInfo.ClassFullNameForTest);
			AssertEquals("InfoCountryCode", "", importJXCControllerInfo.CountryCodeForTest);
		}

		public void TestTableSchemas()
		{
			AssertEquals(5, ClientOverride.Instance.TableSchemas.Length);
			AssertEquals(typeof(ClientAccTransactionHeaderWithJobInfoSchema), ClientOverride.Instance.TableSchemas[0].GetType());
			AssertEquals(typeof(ClientCognosAccGLAccountDescriptorExtraInfoSchema), ClientOverride.Instance.TableSchemas[1].GetType());
			AssertEquals(typeof(ClientCognosSubClassificationAccountCreditorMappingSchema), ClientOverride.Instance.TableSchemas[2].GetType());
			AssertEquals(typeof(ClientCognosSubClassificationAccountDebtorMappingSchema), ClientOverride.Instance.TableSchemas[3].GetType());
			AssertEquals(typeof(ClientCognosGroupingFlagsSchema), ClientOverride.Instance.TableSchemas[4].GetType());
		}

		public void TestDBSchemaUpgradeInfo()
		{
			AssertEquals(typeof(JASClientDbSchemaUpgradeInfo), ClientOverride.Instance.DbSchemaExtensionObjects.GetType());
		}

		[ExpectNoExceptions]
		public void TestDbSchemaUpgradeInfo_CreatingAndDroppingDBObjects()
		{
			Db.Connection.BeginTransaction();
			try
			{
				ArrayList scripts = new ArrayList();
				scripts.AddRange(ClientOverride.Instance.DbSchemaExtensionObjects.TableCreationScripts);
				scripts.AddRange(ClientOverride.Instance.DbSchemaExtensionObjects.ViewAndRoutineCreationScripts);
				scripts.Reverse();
				foreach (DatabaseObjectCreateScript script in scripts)
				{
					try
					{
						ExecuteNonQuery(script.DropScript);
					}
					catch
					{
					}
				}

				scripts.Reverse();
				foreach (DatabaseObjectCreateScript script in scripts)
				{
					ExecuteNonQuery(script.CreateScript);
				}

				AssertEquals("The create script should create the view", true, ExistsDbObject(ClientAccTransactionHeaderWithJobInfoSchema.Constants.TableName));
				scripts.Reverse();
				foreach (DatabaseObjectCreateScript script in scripts)
				{
					ExecuteNonQuery(script.DropScript);
				}

				AssertEquals("The drop script should drop the view", false, ExistsDbObject(ClientAccTransactionHeaderWithJobInfoSchema.Constants.TableName));
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		public void TestAdditionalUserVisibleRegistryItems()
		{
			AssertEquals(JASDataRegistry.Instance, ClientOverride.Instance.AdditionalRegistryItemSet);
		}

		public void TestInitialiseAndUninitialise()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ClientOverride.Instance.Uninitialise();
			AssertType(typeof(PredefinedNoteTypes), PredefinedNoteTypes.Instance);
			AssertEquals("Testing overrides", typeof(DocForwardingShipment), DocForwardingShipment.New(shipment, Factory).GetType());
			ClientOverride.Instance.Initialise();
			AssertType(typeof(JASPredefinedNoteTypes), PredefinedNoteTypes.Instance);
			AssertEquals("Testing overrides", typeof(DocJASForwardingShipment), DocForwardingShipment.New(shipment, Factory).GetType());
		}

		public void TestNewModuleSections()
		{
			AssertEquals("Should add JAS Client Specific section", 1, ClientOverride.Instance.NewModuleSectionsToAddForClient.Length);
			ModuleSectionAddOn section = ClientOverride.Instance.NewModuleSectionsToAddForClient[0] as ModuleSectionAddOn;
			AssertEquals(ModuleTreeLoaderConstant.Category.Operations.Name, section.CategoryName);
			AssertEquals(ClientModuleRegistration.IDs.JASSpecific, section.Name);
			AssertEquals(ClientModuleRegistration.Names.JASSpecific, section.DisplayText);
			AssertEquals(JASSecurityCheckpoints.JASSpecific.Code, section.SecurityCheckpoint.Code);
			AssertEquals(IconTypes.Dollar, section.Icon);
			AssertEquals(IconTypes.Dollar20x16, section.GroupImage);
			AssertEquals(ClientModuleRegistration.Subcategory.JAS.Name, section.Subcategory.Name);
		}

		public void TestClientSpecificSecurityCheckpointsAddedToSecurityInstance()
		{
			AssertNotNull("Should be added", Env.Security.FindCheckPoint(new CheckpointLookupKey(ClientModuleRegistration.IDs.JASSpecific)));
			AssertNotNull("Should be added", Env.Security.FindCheckPoint(new CheckpointLookupKey(ClientModuleRegistration.IDs.ExportCognosFile)));
			AssertNotNull("Should be added", Env.Security.FindCheckPoint(new CheckpointLookupKey(ClientModuleRegistration.IDs.ExportPrematchingTransactions)));
			AssertNotNull("Should be added", Env.Security.FindCheckPoint(new CheckpointLookupKey(ClientModuleRegistration.IDs.ImportJXCFile)));
			AssertNotNull("Should be added", Env.Security.FindCheckPoint(new CheckpointLookupKey(ClientModuleRegistration.IDs.ImportMatchedTransactions)));
		}

		#region Implementation
		NewClientModuleInfo GetNewClientModuleInfo(ClientModuleIdentifier moduleID)
		{
			foreach (NewClientModuleInfo moduleInfo in ClientOverride.Instance.NewClientModules)
			{
				if (moduleInfo.ID == moduleID)
				{
					return moduleInfo;
				}
			}

			return null;
		}

		ControllerInfo GetNewClientControllerInfo(ClientControllerID controllerID)
		{
			foreach (ControllerInfo controllerInfo in ClientOverride.Instance.NewClientControllers)
			{
				if (controllerInfo.ID == controllerID)
				{
					return controllerInfo;
				}
			}

			return null;
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}

		void AssertControllerOverrides(ControllerID controllerID, Type expectedControllerType)
		{
			ZController controller = ZControllerFactory.Create(controllerID);
			AssertEquals(expectedControllerType.FullName, expectedControllerType, controller.GetType());
		}

		bool ExistsDbObject(string objectName)
		{
			DbCommand cmd = Db.Connection.Command("select name FROM sys.objects where name='" + objectName + "'");
			object result = cmd.ExecuteScalar();
			return (result != null);
		}

		void ExecuteNonQuery(string sQL)
		{
			DbCommand cmd = Db.Connection.Command(sQL);
			cmd.ExecuteNonQuery();
		}

		#endregion
		protected override void SetUp()
		{
			base.SetUp();
			Factory = new BusinessObjectFactory();
		}

		BusinessObjectFactory Factory;
	}
}
