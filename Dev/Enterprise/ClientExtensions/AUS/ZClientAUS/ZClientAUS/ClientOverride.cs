using System;
using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using CargoWise.Schema;
using Enterprise.Client.AUS;
using Enterprise.Client.AUS.GUI;
using Enterprise.Client.AUS.Modules;
using Enterprise.Client.AUS.Products;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client
{
	public class ClientOverride : ClientHook
	{
		#region Instance

		public static ClientOverride Instance
		{
			get { return instance ?? (instance = new ClientOverride()); }
		}
		[ThreadStatic]
		static ClientOverride instance;

		#endregion

		#region IClientHook Members

		protected override void InitialiseCore()
		{
			CommercialInvoiceMenu.Initialise();
		}

		public override Clients Client
		{
			get { return Clients.AUS; }
		}

		public override string ClientDisplayName
		{
			get { return "Austin International Trade Services P/L"; }
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
						new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, new ModuleInfo(ClientModuleRegistration.ProductImportAndExport, typeof(ProductImportAndExportModule))),
						new NewClientModuleInfo(ModuleTreeLoaderConstant.Category.Admin, ModuleTreeLoaderConstant.Section.CustomsFiles, new ModuleInfo(ClientModuleRegistration.OriginPreferenceMapping, typeof(OriginPreferenceMappingModule)))
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
					fNewClientControllersCore = new ControllerInfo[2];
					fNewClientControllersCore[0] = new ControllerInfo(ClientControllerRegistration.ProductImportAndExport, "ZClientAUS", "Enterprise.Client.AUS.Modules.ProductImportAndExportController");
					fNewClientControllersCore[1] = new ControllerInfo(ClientControllerRegistration.OriginPreferenceMapping, "ZClientAUS", "Enterprise.Client.AUS.Modules.OriginPreferenceMappingController");
				}
				return fNewClientControllersCore;
			}
		}

		ControllerInfo[] fNewClientControllersCore;

		#endregion

		#region ModuleOverrides

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();
			ClientOverrideModuleIdentifier products = new ClientOverrideModuleIdentifier(ModuleIDs.SupplierPart);
			ClientOverrideModuleInfo productsInfo = new ClientOverrideModuleInfo(products, typeof(AUSOrgSupplierPartModule).Assembly.FullName, typeof(AUSOrgSupplierPartModule).FullName, Core.Constants.CountryCodes.Australia);
			moduleOverrides.AddModuleOverride(productsInfo);
			return moduleOverrides;
		}

		#endregion

		protected override ITableSchema[] GetTableSchemas()
		{
			return new ITableSchema[]
			{
				ClientAUSProductImportRegistrySchema.Instance,
				ClientAUSOriginPreferenceMappingSchema.Instance
			};
		}

		public override IExtensionObjects DbSchemaExtensionObjects { get; } = new ExtensionObjects(TableCreationScripts, ViewAndRoutinesCreationScripts);

		static readonly ImmutableArray<DatabaseObjectCreateScript> TableCreationScripts = ImmutableArray.Create(
			new DatabaseObjectCreateScript("ClientAUSProductInterface", AUSConstants.ClientAUSProductInterfaceTable, AUSConstants.DropClientAUSProductInterfaceTable),
			new DatabaseObjectCreateScript("ClientAUSProductImportRegistry", AUSConstants.ClientAUSProductImportRegistryTable, AUSConstants.DropClientAUSProductImportRegistryTable),
			new DatabaseObjectCreateScript("ClientAUSOriginPreferenceMapping", AUSConstants.ClientAUSOriginPreferenceMappingTable, AUSConstants.DropClientAUSOriginPreferenceMappingTable)
		);

		#region Reports SQL scripts

		static readonly ImmutableArray<DatabaseViewAndRoutineCreateScript> ViewAndRoutinesCreationScripts = ImmutableArray<DatabaseViewAndRoutineCreateScript>.Empty;

		#endregion
	}
}
