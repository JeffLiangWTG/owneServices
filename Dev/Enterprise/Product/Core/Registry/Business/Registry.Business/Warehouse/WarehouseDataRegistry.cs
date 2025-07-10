using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Integration.Warehouse;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using SharedGlowRegistry = CargoWise.Definitions.GlowRegistry;

namespace Enterprise.Registry.Business.Warehouse
{
	public sealed class WarehouseDataRegistry : RegistryItemSet, IWarehouseDataRegistry
	{
		WarehouseDataRegistry()
		{
		}

		#region Instance

		public static WarehouseDataRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new WarehouseDataRegistry();
				}

				return fInstance;
			}
		}
		[ThreadStatic]
		static WarehouseDataRegistry fInstance;

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Warehouse_Adjustments { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("c31ee947-4f02-4064-84a1-1c470b372c8a", "Adjustments")); } }
			public static MultilingualString Warehouse_Release { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("47835100-70e5-436e-bae9-a86a1e167240", "Release")); } }
			public static MultilingualString Warehouse_Orders { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("61c3f491-6de6-411d-b36c-202a006f4707", "Orders")); } }
			public static MultilingualString Warehouse_Freight { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("1c5afa6f-1735-49f5-847e-7109258afe43", "Freight")); } }
			public static MultilingualString Warehouse_Locations { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("0365b983-c1ef-45a3-acdf-d6c49851d710", "Locations")); } }
			public static MultilingualString Warehouse_Picking { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("b14dba26-55b2-4a31-8cb9-840890c3236c", "Picking")); } }
			public static MultilingualString Warehouse_Receive { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("2d9dfa51-9a1c-4074-912f-8ac1bb73e0fd", "Receive")); } }
			public static MultilingualString Warehouse_InventoryAccuracyManagement { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("b7c271db-c23f-4372-b891-f87afc0e0994", "Inventory Accuracy Management")); } }
			public static MultilingualString Warehouse_InventoryAccuracyManagement_LegacyStocktake { get { return CombineCategories(Warehouse_InventoryAccuracyManagement, ResString.GetMultilingualString("212af754-4642-40bf-8564-b74ee115144b", "Legacy Stocktake")); } }
			public static MultilingualString Warehouse_TaskManagement { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("43c69733-ee71-4d34-a5f8-d00d6398f832", "Task Management")); } }
			public static MultilingualString Warehouse_Transfers { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("WarehouseDataRegistry|Warehouse_Transfers", "Transfers")); } }
			public static MultilingualString Warehouse_ABCAnalysis { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("ee7da1a5-17ea-4587-91dd-e2bf84270712", "ABC Analysis")); } }
			public static MultilingualString Warehouse_JobImport { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("5fda7b47-5cad-4774-a325-171b3102cec3", "Job Import")); } }
			public static MultilingualString Warehouse_Invoicing { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("9256efe2-b720-40e0-9601-092cff327511", "Invoicing")); } }
			public static MultilingualString Warehouse_Scanning { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("dba3a6a3-77c6-4a61-a68f-5d02de382aef", "Scanning")); } }
			public static MultilingualString Warehouse_PerformanceReporting { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("4bb3d608-7f80-43ee-a7d2-4a685d8bd0f2", "Performance Reporting")); } }
			public static MultilingualString Warehouse_PackingSlipTitle { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("168b230f-e46c-4347-98b0-e6d7217abd66", "Packing Slip Title")); } }
			public static MultilingualString Warehouse_TransitWarehouse { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("64ed047e-dbe1-4e5c-b525-dccfee7c1703", "Transit Warehouse")); } }
			public static MultilingualString Warehouse_MHE { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("cd311228-3905-4f0c-8c81-f952834748b1", "Material Handling Equipment")); } }
			public static MultilingualString Warehouse_TransitWarehouse_Unload { get { return CombineCategories(Warehouse_TransitWarehouse, ResString.GetMultilingualString("2ef0405d-2a8b-4bbe-b0d8-9caf26ca3a81", "Unload")); } }
			public static MultilingualString Warehouse_TransitWarehouse_Load { get { return CombineCategories(Warehouse_TransitWarehouse, ResString.GetMultilingualString("372f6636-03fe-40b3-aaa1-7aa15c90eb85", "Load")); } }
			public static MultilingualString Warehouse_TransitWarehouse_Unload_DriverSignature { get { return CombineCategories(Warehouse_TransitWarehouse_Unload, ResString.GetMultilingualString("6bd46202-1363-46c9-89ee-2f3e8a6f51fe", "Driver Signature")); } }
			public static MultilingualString Warehouse_TransitWarehouse_Load_DriverSignature { get { return CombineCategories(Warehouse_TransitWarehouse_Load, ResString.GetMultilingualString("987528ef-aa1a-4f47-b9cc-0ab95fc001bc", "Driver Signature")); } }
			public static MultilingualString Warehouse_TransitWarehouse_ChargeableForTransportationUnit { get { return CombineCategories(Warehouse_TransitWarehouse, ResString.GetMultilingualString("d3874dee-4c37-4338-8fb0-8e0ead53061d", "Chargeable Transportation Unit")); } }
			public static MultilingualString Warehouse_Chargeable { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("c2517eae-6087-400e-b951-2896d2ecbae7", "Chargeable")); } }
			public static MultilingualString Warehouse_VolCam { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("851af910-58e0-4dff-bdbb-a0e4ec47d9af", "Vol-Cam")); } }
			public static MultilingualString Warehouse_GateManagement { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("f61628a3-41fb-410a-808b-6af812742d2a", "Gate Management")); } }
			public static MultilingualString Warehouse_ContainerYard { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("7e3efab4-120b-456c-b55c-ec0100390bf6", "Container Yard")); } }
			public static MultilingualString Warehouse_GateManagement_Authentication { get { return CombineCategories(Warehouse_GateManagement, ResString.GetMultilingualString("2F986BB9-6A6B-419F-B8C6-DD2A1CC5F91C", "Authentication")); } }
		}

		#endregion

		#region Adjustments

		public CodeDescriptionPairListWithDefaultCodeRegistryItem AdjustmentReasonCodes
		{
			get
			{
				return GetItem("AdjustmentReasonCodes", delegate
				{
					var item = new CodeDescriptionPairListWithDefaultCodeRegistryItem
					(
						"AdjustmentReasonCodes",
						Categories.Warehouse_Adjustments,
						ResString.GetMultilingualString("e1ad2a98-f65f-4f39-a7fc-568493b935ee", "Adjustment Reason Codes"),
						ResString.GetMultilingualString("e1ad2a98-f65f-4f39-a7fc-568493b935ee", "Adjustment Reason Codes"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DefaultAjustmentReasonCodes,
						false
					);

					return item;
				});
			}
		}

		CodeDescriptionPairList DefaultAjustmentReasonCodes
		{
			get
			{
				var defaultList = new AdjustmentReasonCodesCodeList();
				defaultList.DefaultCode = AdjustmentReasonCodesCodeList.Codes.StocktakeAdjustment;
				return defaultList;
			}
		}

		#endregion

		#region Chareagable

		public ChargeableFactorRegistryItem WarehouseChargeableFactorStorage
		{
			get
			{
				return GetItem<ChargeableFactorRegistryItem>("WarehouseChargeableFactorStorage", () =>
				{
					var defaultFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.Air, ConversionFactor.Standard.Imperial.Domestic);
					return new ChargeableFactorRegistryItem(
						"WarehouseChargeableFactorStorage",
						Categories.Warehouse_Chargeable,
						ResString.GetMultilingualString("c570ffc4-9ed6-4312-a332-4d485c03684b", "Chargeable Factor for Warehouse Storage"),
						FreightDataRegistry.GetChargeableFactorVolumeToWeightHint(defaultFactor),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultFactor);
				});
			}
		}

		public ChargeableFactorRegistryItem WarehouseChargeableFactorHandling
		{
			get
			{
				return GetItem<ChargeableFactorRegistryItem>("WarehouseChargeableFactorHandling", () =>
				{
					var defaultFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.Air, ConversionFactor.Standard.Imperial.Domestic);
					return new ChargeableFactorRegistryItem(
						"WarehouseChargeableFactorHandling",
						Categories.Warehouse_Chargeable,
						ResString.GetMultilingualString("75900944-533d-4624-a67a-383e1ba0c939", "Chargeable Factor for Warehouse Handling"),
						FreightDataRegistry.GetChargeableFactorVolumeToWeightHint(defaultFactor),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultFactor);
				});
			}
		}

		#endregion

		#region IFSOrgProxy

		public GuidRegistryItem IFSOrgProxy
		{
			get
			{
				return GetItem("IFSOrgProxy", delegate
				{
					var result = new GuidRegistryItem(
						"IFSOrgProxy",
						Categories.Warehouse_Release,
						ResString.GetMultilingualString("b6dc5ad5-6447-4289-8a69-74e2164fc122", "IFS Organization"),
						ResString.GetMultilingualString("8747d74b-5c7f-438d-b429-c98370b54e97", "You should create an IFS Organization and then setup IFS Code Mappings on the Organization's 'EDI Code Mapping' tab."),
						RegistryStorageFlags.System);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader);
					return result;
				});
			}
		}

		#endregion

		#region Orders

		public BooleanRegistryItem CreateNewAddressOnUnmatchedAddressForOrders
		{
			get
			{
				return GetItem("CreateNewAddressOnUnmatchedAddressForOrders", delegate
				{
					var result = new BooleanRegistryItem("CreateNewAddressOnUnmatchedAddressForOrders",
						Categories.Warehouse_Orders,
						ResString.GetMultilingualString("66768ec9-a98d-48d4-ba15-0ca3804698f2", "Create new Organization Address on Unmatched Address"),
						ResString.GetMultilingualString("96fb04be-913f-4629-94a0-93f008d74e13",
							"When this item is enabled, during Legacy XML Import of a Warehouse Order, a new Organization Address will be automatically created when an Organization is matched but the Address does not match."),
						RegistryStorageFlags.System,
						true);

					return result;
				});
			}
		}

		public BooleanRegistryItem PreventOrderLinesUpdateWhenOrderIsInPicking
		{
			get
			{
				return GetItem("PreventOrderLinesUpdateWhenOrderIsInPicking", delegate
				{
					var result = new BooleanRegistryItem("PreventOrderLinesUpdateWhenOrderIsInPicking",
						Categories.Warehouse_Orders,
						ResString.GetMultilingualString("e2ae998f-affa-4de4-b69a-64c6fa7f3d72", "Prevent Updates to Orders After Pick is Created"),
						ResString.GetMultilingualString("df2a7f8d-a5fb-48b5-a4c3-ef886eec841c", "When this item is enabled, updates to order lines will not be allowed once the order is in picking."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);

					return result;
				});
			}
		}

		#region GroupOrderedInventoryByCustomAttributes

		public BooleanRegistryItem GroupOrderedInventoryByCustomAttributes
		{
			get
			{
				const string groupOrderedInventoryByCustomAttributes = nameof(GroupOrderedInventoryByCustomAttributes);
				return GetItem(groupOrderedInventoryByCustomAttributes, () =>
				{
					return new BooleanRegistryItem(groupOrderedInventoryByCustomAttributes,
						Categories.Warehouse_Orders,
						ResString.GetMultilingualString("589f507f-716c-4b86-9b55-58ad6213e37c", "Group Ordered Inventory By Custom Attributes"),
						ResString.GetMultilingualString("f1cbe5ff-ad7b-447c-8f9b-6e226253b31a", "Enables displaying and grouping Warehouse Pick Ordered Inventory By Custom Attributes."),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						false
					);
				});
			}
		}

		#endregion

		#endregion

		#region Freight Location Is True WHS Location

		public BooleanRegistryItem FreightLocationIsTrueWHSLocation
		{
			get
			{
				return GetItem("FreightLocationIsTrueWHSLocation", delegate
				{
					return new BooleanRegistryItem(
						"FreightLocationIsTrueWHSLocation",
						Categories.Warehouse_Freight,
						ResString.GetMultilingualString("875d6c37-ba10-4f4f-8503-ace751d5c3cb", "Freight Location Is True WHS Location"),
						ResString.GetMultilingualString("bd0e06cc-3ad2-4ec3-89ed-4aabf88dda83", "By setting this, the Freight Location will become a true Warehouse location and be validated as such."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region Transit Warehouse

		#region TransitWarehouseBranchesThatAllowMixedSecurityFreight

		public BranchCollectionRegistryItem TransitWarehouseBranchesThatAllowMixedSecurityFreight
		{
			get
			{
				return GetItem("TransitWarehouseBranchesThatAllowMixedSecurityFreight", delegate
				{
					var item = new BranchCollectionRegistryItem
					(
						"TransitWarehouseBranchesThatAllowMixedSecurityFreight",
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("70cc2531-8d99-4d0f-8bf6-c92674bb465d", "Branches that allow Secure and Non-Secure freight on vehicles"),
						ResString.GetMultilingualString("032bbb29-1e13-49df-acf5-57867215926d",
							"Add Branches for Transit Warehouses that treat Secure Freight mixed with Non-Secure Freight (on Vehicles) as secure."),
						RegistryStorageFlags.System
					);

					return item;
				});
			}
		}

		#endregion

		#region EnablePackageSealNumbers

		public BooleanRegistryItem EnablePackageSealNumbers
		{
			get
			{
				return GetItem("EnablePackageSealNumbers", delegate
				{
					return new BooleanRegistryItem("EnablePackageSealNumbers",
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("WarehouseDataRegistry|EnablePackageSealNumbers|Caption", "Enable Package Seal Numbers"),
						ResString.GetMultilingualString("WarehouseDataRegistry|EnablePackageSealNumbers|Hint", "Will enable Seal Numbers on Package."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region EnableEcommercePortal

		public BooleanRegistryItem EnableEcommercePortal
		{
			get
			{
				return GetItem("EnableEcommercePortal", delegate
				{
					return new BooleanRegistryItem("EnableEcommercePortal",
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("WarehouseDataRegistry|EnableEcommercePortal|Caption", "Enable Ecommerce Portal"),
						ResString.GetMultilingualString("WarehouseDataRegistry|EnableEcommercePortal|Hint", "Enabling this creates a link on the Desktop Portal to the Ecommerce Module."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region EnableImportingCoLoadMastersWithoutSubs

		public BooleanRegistryItem EnableImportingCoLoadMastersWithoutSubs
		{
			get
			{
				return GetItem("EnableImportingCoLoadMastersWithoutSubs", delegate
				{
					return new BooleanRegistryItem("EnableImportingCoLoadMastersWithoutSubs",
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("WarehouseDataRegistry|EnableImportingCoLoadMastersWithoutSubs|Caption", "Enable Importing Co-Load Masters Without Subs"),
						ResString.GetMultilingualString("WarehouseDataRegistry|EnableImportingCoLoadMastersWithoutSubs|Hint", "Will enable Importing Co-Load Masters without Sub-shipments when importing UXML shipment from Forwarding."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region PackageScreeningMethods

		public CodeDescriptionPairListWithDefaultCodeRegistryItem PackageScreeningMethods
		{
			get
			{
				var packageScreeningMethodDefaultList = new PackageScreeningMethodList().PackageScreeningMethodCodeDescriptionList;

				return GetItem<CodeDescriptionPairListWithDefaultCodeRegistryItem>("PackageScreeningMethods", delegate
				{
					return new CodeDescriptionPairListWithDefaultCodeRegistryItem(
						"PackageScreeningMethods",
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("d7e68a17-2964-4fab-b569-06cb83135c53", "Package Screening Methods"),
						ResString.GetMultilingualString("6ce075ad-ee15-4af0-8469-c0c2d64a5668", "Add Package Screening Methods used to determine if a freight can be released."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						packageScreeningMethodDefaultList, false, false, 3, false);
				});
			}
		}

		#endregion

		#region DefaultPhotoDocumentType

		public StringRegistryItem DefaultPhotoDocumentType
		{
			get
			{
				return GetItem(nameof(DefaultPhotoDocumentType), () =>
				{
					var factory = new BusinessObjectFactory();
					var documentTypeforCapturedPhotosList = GetDocTypeCollection(factory)
						.Cast<IRefDocType>()
						.ToList();

					var index = documentTypeforCapturedPhotosList
						.FindIndex(docType => docType.RT_DocType == "MCF");

					var selectedDefaultValue = documentTypeforCapturedPhotosList[index].RT_DocType;

					StringRegistryItem result = new StringRegistryItem(
						"DefaultPhotoDocumentType",
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("e3e65a47-2874-4had-b468-76gf85635c93", "Default Photo Document Type"),
						ResString.GetMultilingualString("6yt098ad-ed15-4vf0-8219-c5s2g64a5778", "The selected document type will be defaulted when taking/uploading photos to the eDocs from mobile devices."),
						RegistryStorageFlags.System,
						selectedDefaultValue);

					result.EditorInfo = new CodeFindBoxRegistryEditorInfo(ModuleIDs.RefDocType, GetDocTypeCollection);
					return result;
				});
			}
		}

		IBusinessObjectCollection GetDocTypeCollection(BusinessObjectFactory factory)
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IRefDocTypeCollection>(), factory);
		}

		#endregion

		#region Mandatory Package Screening For Air And Unknown TransportMode

		public BooleanRegistryItem MandatoryPackageScreeningForAirAndUnknownTransportMode
		{
			get
			{
				return GetItem("MandatoryPackageScreeningForAirAndUnknownTransportMode", delegate
				{
					return new BooleanRegistryItem("MandatoryPackageScreeningForAirAndUnknownTransportMode",
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("bbb92430-7327-4b83-a666-97aa138949e0", "Package must pass its latest screening before it can be loaded"),
						ResString.GetMultilingualString("f490c64a-913c-47b1-a245-35bd3daef96f", "Package must pass its latest screening before it can be loaded if transport mode is Air or Unknown."),
						RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, true)
					{
						OnUpdateAction = OnTransitMandatoryPackageScreeningForAirAndUnknownTransportModeItemUpdate
					};
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void OnTransitMandatoryPackageScreeningForAirAndUnknownTransportModeItemUpdate(Guid companyPK, Guid branchPK, Guid departmentPK, object registryitem)
		{
			var userName = ((IGlbStaff)Env.CurrentUser).GS_Code;
			try
			{
				var sql = "EXEC dbo.UpdatePackageStateSecurityStatus @companyBranchPK, @systemLastEditUser, @RegistryValue, @WarehouseConfigurationValue, @PackageStatePKs";

				var command = Db.Connection.Command(sql);

				command.AddParameter("@companyBranchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@systemLastEditUser", SqlDbType.Char, Convert.ToString(userName));
				command.AddParameter("@RegistryValue", SqlDbType.Bit, registryitem);
				command.AddParameter("@WarehouseConfigurationValue", SqlDbType.Bit, DBNull.Value);
				command.AddTableValuedParameter("@PackageStatePKs", TVPHelper.TVP_uniqueidentifier, Enumerable.Empty<Guid>());
				command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				ErrorReporter.ReportOnce(ex.Message, ex);
			}
		}

		#endregion

		#region Apply Package Quantity Counting Algorithm For Customs Status Calculation

		public BooleanRegistryItem ApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculation
		{
			get
			{
				const string registryName = nameof(ApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculation);
				return GetItem(registryName, delegate
				{
					return new BooleanRegistryItem(registryName,
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("5447f090-2617-4f06-af66-6ba68242a2fa", "Apply package quantity counting algorithm for customs status calculation"),
						ResString.GetMultilingualString("2fdbea65-4a9a-4685-beb1-83837f6fe5c1", "Apply package quantity counting algorithm when calculating customs status of receive consignment and packages."),
						RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, false)
					{
						OnUpdateAction = OnTransitApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculationItemUpdate
					};
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void OnTransitApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculationItemUpdate(Guid companyPK, Guid branchPK, Guid departmentPK, object registryitem)
		{
			var userName = ((IGlbStaff)Env.CurrentUser).GS_Code;
			try
			{
				var sql = "EXEC dbo.UpdatePackageStateAndRCNCustomStatus @CompanyBranchPK, @SystemLastEditUser, @CurrentUTC, @WarehouseConfigCustomControlled, @WarehouseConfigPortControlled, @IsApplyPackageQuantityCountingAlgorithm, @PackageStatePKs";

				var command = Db.Connection.Command(sql);

				command.AddParameter("@CompanyBranchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@SystemLastEditUser", SqlDbType.Char, Convert.ToString(userName));
				command.AddParameter("@CurrentUTC", SqlDbType.DateTime, DBNull.Value);
				command.AddParameter("@WarehouseConfigCustomControlled", SqlDbType.Bit, DBNull.Value);
				command.AddParameter("@WarehouseConfigPortControlled", SqlDbType.Bit, DBNull.Value);
				command.AddParameter("@IsApplyPackageQuantityCountingAlgorithm", SqlDbType.Bit, registryitem);
				command.AddTableValuedParameter("@PackageStatePKs", TVPHelper.TVP_uniqueidentifier, Enumerable.Empty<Guid>());
				command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				ErrorReporter.ReportOnce(ex.Message, ex);
			}
		}

		#endregion

		#region LabelValidationField

		public CodePairRegistryItem LabelValidationField
		{
			get
			{
				return GetItem("LabelValidationField", delegate
				{
					return new CodePairRegistryItem(
						"LabelValidationField",
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("af867ada-2f95-42d6-a482-4efbaf719dc8", "Default Label Validation Number"),
						ResString.GetMultilingualString("1c9d7383-82d5-4892-85d0-e106f17280f4", "Default Label Validation Number you would like to validate against."),
						LabelValidationFieldListProvider,
						RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						LabelValidationFieldList.DefaultCode);
				});
			}
		}

		ICodeDescriptionPairListProvider LabelValidationFieldListProvider
		{
			get
			{
				if (labelValidationFieldListProvider == null)
				{
					labelValidationFieldListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair("HSB", ResString.GetMultilingualString("ba456303-367e-49d4-94eb-0579d3adfa54", "House Bill Number"));
						list.AddPair("MAB", ResString.GetMultilingualString("90bb54d8-114d-47e5-a063-4cb2b5fb87c8", "Master Bill Number"));
						list.AddPair("FSH", ResString.GetMultilingualString("2d3d6563-c5f1-47e1-9572-50b231a19efc", "Forwarding Shipment Number"));
						list.DefaultCode = "";
						return list;
					});
				}

				return labelValidationFieldListProvider;
			}
		}
		ICodeDescriptionPairListProvider labelValidationFieldListProvider;

		public CodeDescriptionPairList LabelValidationFieldList
		{
			get
			{
				if (labelValidationField == null)
				{
					labelValidationField = LabelValidationFieldListProvider.CodeDescriptionPairList;
				}

				return labelValidationField;
			}
		}
		CodeDescriptionPairList labelValidationField;

		#endregion

		#region AdjustOutReason

		public CodeDescriptionPairListWithDefaultCodeRegistryItem AdjustOutReasons
		{
			get
			{
				return GetItem("AdjustOutReasons", delegate
				{
					var defaultValue = new CodeDescriptionPairList();

					defaultValue.AddPair("OTH", ResString.GetMultilingualString("0f01bb6d-fd78-4cd3-8344-a222d7c0d599", "Other"));
					defaultValue.AddPair("LCC", ResString.GetMultilingualString("f1dad947-76ee-4035-bba2-6ef368df3797", "Lost in Cycle Count"));
					defaultValue.AddPair("BDS", ResString.GetMultilingualString("7941a089-b1c0-4dff-b710-2fd50605699e", "Broken Down via Service"));
					defaultValue.AddPair("CTP", ResString.GetMultilingualString("98b90e5d-2fa6-4491-8dc2-de0d1fa6d558", "Converted to Packline"));
					defaultValue.DefaultCode = "OTH";

					var item = new CodeDescriptionPairListWithDefaultCodeRegistryItem
					(
						"AdjustOutReasons",
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("389b9488-34fb-4206-b10d-4c9693be7f63", "Adjust Out Reasons"),
						ResString.GetMultilingualString("389b9488-34fb-4206-b10d-4c9693be7f63", "Adjust Out Reasons"),
						RegistryStorageFlags.System,
						defaultValue,
						false
					);

					return item;
				});
			}
		}

		#endregion

		#region ShowErrorWhenDuplicateVehicleInFacility

		public BooleanRegistryItem ShowErrorWhenDuplicateVehicleInFacility
		{
			get
			{
				return GetItem("ShowErrorWhenDuplicateVehicleInFacility", () =>
				{
					return new BooleanRegistryItem("ShowErrorWhenDuplicateVehicleInFacility",
						Categories.Warehouse_GateManagement,
						ResString.GetMultilingualString("c141949c-96a6-4899-9b2b-ef6a8ad7fea1", "Show Error When Duplicate Vehicle in Facility"),
						ResString.GetMultilingualString("a2bbb737-de33-45b5-9e22-03aa2ed439dd", "By default, the system displays a warning when user creates a gate in with a vehicle registration matching a vehicle that is already in the facility. By setting this, the system displays an error instead, and user is not able to save the gate in until this is rectified."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region EnableTransitPutawayRulesEngine

		public BooleanRegistryItem EnableTransitPutawayRulesEngine
		{
			get
			{
				const string enableTransitPutawayRulesEngine = nameof(EnableTransitPutawayRulesEngine);
				return GetItem(enableTransitPutawayRulesEngine, delegate
				{
					return new BooleanRegistryItem(enableTransitPutawayRulesEngine,
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("42f42453-3588-4f67-90df-07041af27013", "Enable Directed Putaway Rules"),
						ResString.GetMultilingualString("1eb0e8d5-87f2-4bf2-a189-f9634fde142d", "Use the Transit Warehouse Directed Putaway Rules Engine in the Putaway processes and enable \"Suggest Another Location\" option on Putaway and Manual Transfers."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region EnablePalletizePackline

		public BooleanRegistryItem EnablePalletizePackline
		{
			get
			{
				const string enablePalletizePackline = nameof(EnablePalletizePackline);
				return GetItem(enablePalletizePackline, delegate
				{
					return new BooleanRegistryItem(enablePalletizePackline,
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("78c0570d-1bf5-4edf-9471-33a62ec6893a", "Enable Palletize Packline"),
						ResString.GetMultilingualString("5fbe513a-7d08-4c34-a7e3-2d8c064d991b", "Enable Palletize Packline in Mobile Application."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region EnableHandlingUnitSkipScanMode

		public BooleanRegistryItem EnableHandlingUnitSkipScanMode
		{
			get
			{
				const string enableHandlingUnitSkipScanMode = nameof(EnableHandlingUnitSkipScanMode);
				return GetItem(enableHandlingUnitSkipScanMode, delegate
				{
					return new BooleanRegistryItem(enableHandlingUnitSkipScanMode,
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("4ae7f173-3b14-4221-a820-46e02d63210e", "Enable Handling Unit Skip Scan Mode"),
						ResString.GetMultilingualString("7254651a-1d89-475a-a598-6cee4d5e2df9", "Enable Skip Scan Mode for Handling Unit."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region AllowServiceCompletionOnBookedPackages

		public BooleanRegistryItem AllowServiceCompletionOnBookedPackages
		{
			get
			{
				const string registryName = nameof(AllowServiceCompletionOnBookedPackages);
				return GetItem(registryName, delegate
				{
					return new BooleanRegistryItem(registryName,
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("0b2292fd-2eb1-448e-b5b9-4337e349a956", "Allow Service Completion on Booked Packages"),
						ResString.GetMultilingualString("37912bb5-d01a-49cd-ac75-fe0154653e7e", "When set to Yes, Services can be completed on Booked Packages.\r\nWhen set to No, Services cannot be completed on Booked Packages."),
						RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		#endregion

		#region EnablePackageIDMatchingFromPreviousTransitWarehouses

		public BooleanRegistryItem EnablePackageIDMatchingFromPreviousTransitWarehouses
		{
			get
			{
				const string enablePackageIDMatchingFromPreviousTransitWarehouses = nameof(EnablePackageIDMatchingFromPreviousTransitWarehouses);
				return GetItem(enablePackageIDMatchingFromPreviousTransitWarehouses, delegate
				{
					return new BooleanRegistryItem(enablePackageIDMatchingFromPreviousTransitWarehouses,
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("36b338a3-7499-4350-a477-0e961aa87ac0", "Enable Package ID Matching from Previous Transit Warehouses"),
						ResString.GetMultilingualString("65679800-def6-42c6-93f5-0394506f14d3", "When enabled, an package ID scanned during unload that is recognized from a previous transit warehouse will have its details imported to the current warehouse. Handling Unit IDs will have their child packages imported."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region EnableUNDGValidationWhenRTUGateIn

		public BooleanRegistryItem EnableUNDGValidationWhenRTUGateIn
		{
			get
			{
				const string enableUNDGValidationWhenRTUGateIn = nameof(EnableUNDGValidationWhenRTUGateIn);
				return GetItem(enableUNDGValidationWhenRTUGateIn, delegate
				{
					return new BooleanRegistryItem(enableUNDGValidationWhenRTUGateIn,
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("0a8cae81-4575-4f33-8c99-d098f1e14012", "Enable UNDG Validation when RTU Gate In"),
						ResString.GetMultilingualString("9555764c-0368-4216-85ab-2301c122f48f", "When enabled, UNDG validation rule will be used to validate RTU Planned Packages."),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#endregion

		#region Gate Management

		public StringRegistryItem GateBookingMessagingeHubID
		{
			get => GetItem("GateBookingMessagingeHubID", delegate
			{
				return new StringRegistryItem(
					"GateBookingMessagingeHubID",
					Categories.Warehouse_GateManagement,
					(NoResString)"Gate Booking Messaging eHub ID",
					(NoResString)"Gate Booking Messaging eHub ID",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					"");
			});
		}

		public BooleanRegistryItem EnableFacilitiesGateWebService
		{
			get => GetItem("EnableFacilitiesGateWebService", delegate
			{
				return new BooleanRegistryItem(
					"EnableFacilitiesGateWebService",
					Categories.Warehouse_GateManagement,
					(NoResString)"Enable Facilities Gate Web Service",
					(NoResString)"Enables Facilities Gate Web Service",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false);
			});
		}

		#endregion

		#region EnableAutoCreateReceiveConsignmentMatchingFromPreviousTransitWarehouses

		public BooleanRegistryItem EnableAutoCreateReceiveConsignmentMatchingFromPreviousTransitWarehouses
		{
			get
			{
				const string enableAutoCreateReceiveConsignmentMatchingFromPreviousTransitWarehouses = nameof(EnableAutoCreateReceiveConsignmentMatchingFromPreviousTransitWarehouses);
				return GetItem(enableAutoCreateReceiveConsignmentMatchingFromPreviousTransitWarehouses, delegate
				{
					return new BooleanRegistryItem(enableAutoCreateReceiveConsignmentMatchingFromPreviousTransitWarehouses,
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("8011bae3-99bf-4911-b251-e453992617e6", "Enable Auto Create Receive Consignment"),
						ResString.GetMultilingualString("000ba818-684c-488e-b443-ff0ed46b1559", "When enabled, Unload Package with ID Matching from Previous Warehouse should create Receive Consignment automatically."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region Unload

		#region AutoCreationOfPutawayTransferOnUnloadCompletion

		public BooleanRegistryItem AutoCreationOfPutawayTransferOnUnloadCompletion
		{
			get
			{
				return GetItem("AutoCreationOfPutawayTransferOnUnloadCompletion", delegate
				{
					return new BooleanRegistryItem("AutoCreationOfPutawayTransferOnUnloadCompletion",
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("WarehouseDataRegistry|AutoCreationOfPutawayTransferOnUnloadCompletion|Caption", "Auto-Creation of Putaway Transfer on Unload Completion"),
						ResString.GetMultilingualString("WarehouseDataRegistry|AutoCreationOfPutawayTransferOnUnloadCompletion|Hint", "When enabled, a putaway transfer will be automatically created when the user does not elect to putaway packages immediately after unload process.\r\nWhen disabled, when the user elects not to putaway packages immediately after unload, no putaway transfer will be created. User will need to create an ad-hoc transfer or putaway by scanning the package to putaway."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#region Default Transportation Type

		ICodeDescriptionPairListProvider DefaultTransportationTypeListProvider
		{
			get
			{
				if (defaultTransportationTypeListProvider == null)
				{
					defaultTransportationTypeListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair("NON", ResString.GetMultilingualString("a6658142-5b0d-43d1-8581-04a351af1c5b", "None"));
						list.AddPair("CNT", ResString.GetMultilingualString("863f4b04-0660-4758-b75b-2617f4d9e61c", "Sea Container"));
						list.AddPair("VEH", ResString.GetMultilingualString("eb771d81-7f30-404c-8d91-9394cbdc31fe", "Vehicle"));
						list.AddPair("ULD", ResString.GetMultilingualString("20e871aa-95e3-4571-b6f4-c2dc057bbc34", "Air ULD Container"));
						list.DefaultCode = "NON";
						return list;
					});
				}

				return defaultTransportationTypeListProvider;
			}
		}
		ICodeDescriptionPairListProvider defaultTransportationTypeListProvider;

		public CodeDescriptionPairList DefaultTransportationTypeList
		{
			get
			{
				if (defaultTransportationTypeList == null)
				{
					defaultTransportationTypeList = DefaultTransportationTypeListProvider.CodeDescriptionPairList;
				}

				return defaultTransportationTypeList;
			}
		}
		CodeDescriptionPairList defaultTransportationTypeList;

		public CodePairRegistryItem DefaultTransportationType
		{
			get
			{
				return GetItem("DefaultTransportationType", delegate
				{
					return new CodePairRegistryItem("DefaultTransportationType",
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("5f749a1d-66c2-4716-8d40-6f1c01b26ead", "Default Transportation Type"),
						ResString.GetMultilingualString("50ecf09e-43b1-4bd3-aade-c661d68b158a", "Set default Transportation Type when starting a new unload on mobile."),
						DefaultTransportationTypeListProvider,
						RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						DefaultTransportationTypeList.DefaultCode);
				});
			}
		}

		#endregion

		#region Default Status for Damaged Packages

		public BooleanRegistryItem AutoSetDamagedPackageToHeld
		{
			get
			{
				const string AutoSetDamagedPackageToHeld = nameof(AutoSetDamagedPackageToHeld);
				return GetItem(AutoSetDamagedPackageToHeld, delegate
				{
					return new BooleanRegistryItem(AutoSetDamagedPackageToHeld,
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("525960e7-1f46-4b24-a541-ea74fc2eb8fd", "Auto Set Damaged Package to Held"),
						ResString.GetMultilingualString("972d86f2-d39a-4b2b-ae3e-c1041133b191", "When enabled, damaged packages will be set to held automatically."),
						RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		#endregion

		#region Prompt For ASN

		public BooleanRegistryItem PromptForASN
		{
			get
			{
				return GetItem("PromptForASN", delegate
				{
					return new BooleanRegistryItem("PromptForASN",
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("ba2b70c3-6927-4a70-885b-d4965b3309b7", "Prompt for ASN"),
						ResString.GetMultilingualString("75398b77-6914-4d37-b1f8-830e65f65298", "Select ASN prompt shown after entering or resuming an RTU. If set to NO, the Select ASN prompt is not shown and the unload will be treated as a blind unload i.e., no active consignment and no remaining packages are shown.\r\n\r\nPlease note, this setting is ignored if an ASN is found attached to an RTU."),
						RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		#endregion

		#region RTU Transport Company Is Mandatory During Unload

		public BooleanRegistryItem RTUTransportCompanyIsMandatoryDuringUnload
		{
			get
			{
				return GetItem("RTUTransportCompanyIsMandatoryDuringUnload", delegate
				{
					return new BooleanRegistryItem("RTUTransportCompanyIsMandatoryDuringUnload",
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("c91c522e-24ee-4637-9a08-b96253842f4a", "RTU Transport Company is Mandatory for Vehicle during Unload"),
						ResString.GetMultilingualString("b85e80e8-e96f-41c9-beae-468bd0493436", "Receive Transport Unit Transport Company is mandatory during the unload process for Vehicle Load Types."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#endregion

		#region DefaultUnloadMode

		public CodePairRegistryItem DefaultUnloadMode
		{
			get
			{
				return GetItem("DefaultUnloadMode", delegate
				{
					const string DefaultUnloadMode = nameof(DefaultUnloadMode);
					var item = new CodePairRegistryItem
					(
						DefaultUnloadMode,
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("c2b18347-a146-4282-900b-f127d50fff54", "Default Unload Mode"),
						ResString.GetMultilingualString("a15df0de-725f-4817-ad20-315daee7d248", "Set default Unload Mode when starting a new unload on mobile.\r\n\r\nPlease note, if APM is selected, the mobile user will be prompted between the Full and Rapid Unload modes."),
						new CodeDescriptionPairListProvider(() => new WhsUnloadModeCodeList()),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						WhsUnloadModeCodeList.Codes.FullUnload
					);

					return item;
				});
			}
		}

		#endregion

		#region DefaultDimsOrVolumeBehaviour

		public CodePairRegistryItem DefaultDimsOrVolumeBehaviour
		{
			get
			{
				return GetItem("DefaultDimsOrVolumeBehaviour", delegate
				{
					const string DefaultDimsOrVolumeBehaviour = nameof(DefaultDimsOrVolumeBehaviour);
					var item = new CodePairRegistryItem
					(
						DefaultDimsOrVolumeBehaviour,
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("d0a7e9a2-91c9-4e7a-bc3e-607c4a1a9f42", "Dimensions Or Volume Behavior Mode"),
						ResString.GetMultilingualString("3f6d1b8e-7e7d-4c9a-ba49-0f8c5e2a91df", "This item controls whether dimensions and/or volume fields are read-only when unloading packages on the mobile device."),
						new CodeDescriptionPairListProvider(() => new DimsOrVolumeBehaviourModesCodeList()),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						DimsOrVolumeBehaviourModesCodeList.Codes.DIMorVOL
					);

					return item;
				});
			}
		}

		#endregion

		#region DriverSecurityCertificationCheckingActivated

		public BooleanRegistryItem DriverSecurityCertificationCheckingActivated
		{
			get
			{
				return GetItem("DriverSecurityCertificationCheckingActivated", delegate
				{
					return new BooleanRegistryItem(
						"DriverSecurityCertificationCheckingActivated",
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("8c610c0d-d69d-4c36-bcab-ff1a9f0e2498", "Driver Security Certification Checking Activated"),
						ResString.GetMultilingualString("ce2c7a4b-15e3-4f1b-abe7-06ecf6dc8e2f", "When registry is set to Yes, Driver Security Certification will be checked, and included in the 'Is Secure' calculation in accordance with EU and UK supply chain security regime compliance requirements.\r\nWhen registry is set to No, the 'Is Secure' calculation will exclude checking of Driver Security Certification irrespective of the contact having certification on their contact record."),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						true);
				});
			}
		}

		#endregion

		#region Driver Signature

		#region Prompt For Driver Signature After Unloading Vehicle

		public BooleanRegistryItem PromptForDriverSignatureAfterUnloadingVehicle
		{
			get
			{
				return GetItem("PromptForDriverSignatureAfterUnloadingVehicle", delegate
				{
					return new BooleanRegistryItem("PromptForDriverSignatureAfterUnloadingVehicle",
						Categories.Warehouse_TransitWarehouse_Unload_DriverSignature,
						ResString.GetMultilingualString("dcee219c-4ceb-4500-8546-b700c7723a3e", "Prompt for Driver Signature after unloading vehicle"),
						ResString.GetMultilingualString("b2acfe32-e141-45d8-9b1a-1ee0fb6547c6", "Whether or not RTU unload complete prompts for vehicle signature."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		#endregion

		#region Prompt For Driver Signature After Unloading Container

		public BooleanRegistryItem PromptForDriverSignatureAfterUnloadingContainer
		{
			get
			{
				return GetItem("PromptForDriverSignatureAfterUnloadingContainer", delegate
				{
					return new BooleanRegistryItem("PromptForDriverSignatureAfterUnloadingContainer",
						Categories.Warehouse_TransitWarehouse_Unload_DriverSignature,
						ResString.GetMultilingualString("0ff74c9b-0fad-4bc1-9551-851df8af36b2", "Prompt for Driver Signature after unloading sea container"),
						ResString.GetMultilingualString("87c32acd-30b9-4ca1-acec-9b82aa4d7a4c", "Whether or not RTU unload complete prompts for sea container signature."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		#endregion

		#region Prompt For Driver Signature After Unloading Air Container

		public BooleanRegistryItem PromptForDriverSignatureAfterUnloadingAirContainer
		{
			get
			{
				return GetItem("PromptForDriverSignatureAfterUnloadingAirContainer", delegate
				{
					return new BooleanRegistryItem("PromptForDriverSignatureAfterUnloadingAirContainer",
						Categories.Warehouse_TransitWarehouse_Unload_DriverSignature,
						ResString.GetMultilingualString("0ff74c9b-0fad-4bc1-9551-851df8af36a2", "Prompt for Driver Signature after unloading air container"),
						ResString.GetMultilingualString("87c32acd-30b9-4ca1-acec-9b82aa4d7a52", "Whether or not RTU unload complete prompts for air container signature."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		#endregion

		#endregion

		#region Capture RCN Customs Details

		public BooleanRegistryItem CaptureRCNCustomsDetails
		{
			get
			{
				return GetItem("CaptureRCNCustomsDetails", delegate
				{
					return new BooleanRegistryItem("CaptureRCNCustomsDetails",
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("8c1f7f35-5e05-4bea-9b17-01f3984b7999", "Capture RCN Customs Details"),
						ResString.GetMultilingualString("b6c67d47-ed82-428c-88c3-5a4425f445a3", "When creating or selecting a Receive Consignment on mobile, prompt the user to capture the Customs Details if not already entered."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#endregion

		#region Capture RCN Consignor

		public BooleanRegistryItem CaptureRCNConsignor
		{
			get
			{
				return GetItem("CaptureRCNConsignor", delegate
				{
					return new BooleanRegistryItem("CaptureRCNConsignor",
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("52f403be-f28c-43c4-80a6-6683262dff35", "Capture RCN Consignor"),
						ResString.GetMultilingualString("67142bce-b66d-478c-a0cf-c11e9ec99e05", "When creating or selecting a Receive Consignment on mobile, prompt the user to capture the Consignor if not already entered."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#endregion

		#region Capture RCN Consignee

		public BooleanRegistryItem CaptureRCNConsignee
		{
			get
			{
				return GetItem("CaptureRCNConsignee", delegate
				{
					return new BooleanRegistryItem("CaptureRCNConsignee",
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("53c5deee-0164-460d-ba3b-e837b3eac538", "Capture RCN Consignee"),
						ResString.GetMultilingualString("a7935892-6091-4888-a991-e5f6504c6818", "When creating or selecting a Receive Consignment on mobile, prompt the user to capture the Consignee if not already entered."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#endregion

		#region Capture RCN Booking Party

		public BooleanRegistryItem CaptureRCNBookingParty
		{
			get
			{
				return GetItem("CaptureRCNBookingParty", delegate
				{
					return new BooleanRegistryItem("CaptureRCNBookingParty",
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("8f10f05c-6d8a-4b1e-aecb-4d953ad6b244", "Capture RCN Booking Party"),
						ResString.GetMultilingualString("42466e01-8a4a-40fe-8317-3e46e458880a", "When creating or selecting a Receive Consignment on mobile, prompt the user to capture the Booking Party if not already entered."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#endregion

		#region Capture RCN Billing Client

		public BooleanRegistryItem CaptureRCNBillingClient
		{
			get
			{
				return GetItem("CaptureRCNBillingClient", delegate
				{
					return new BooleanRegistryItem("CaptureRCNBillingClient",
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("f6028b3c-6071-4278-9617-96a09a80402b", "Capture RCN Billing Client"),
						ResString.GetMultilingualString("de704ae6-3d97-4cf3-810b-066d22b74b66", "When creating or selecting a Receive Consignment on mobile, prompt the user to capture the Billing Client if not already entered."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#endregion

		#region DefaultBilltoPartyForTWConsignment

		public CodePairRegistryItem DefaultBilltoPartyForTWConsignment
		{
			get
			{
				return GetItem("DefaultBilltoPartyForTWConsignment", delegate
				{
					const string DefaultBilltoPartyForTWConsignment = nameof(DefaultBilltoPartyForTWConsignment);
					var item = new CodePairRegistryItem
					(
						DefaultBilltoPartyForTWConsignment,
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("42abab6b-a0b1-4f33-8a85-ed190694c0bf", "Default Bill to Party For TW Consignment"),
						ResString.GetMultilingualString("8b3e625d-cdf9-4b9b-8f37-51c14bb61f7b", "Override the default value in the Registry to configure the Organization to be used as the Bill to Party in Transit Warehouse"),
						new CodeDescriptionPairListProvider(() => new DefaultBilltoPartyForTWConsignmentCodeList()),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						DefaultBilltoPartyForTWConsignmentCodeList.Codes.Non
					);

					return item;
				});
			}
		}

		#endregion

		#region Capture RCN Service Level

		public BooleanRegistryItem CaptureRCNServiceLevel
		{
			get
			{
				return GetItem("CaptureRCNServiceLevel", delegate
				{
					return new BooleanRegistryItem("CaptureRCNServiceLevel",
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("fcd144aa-db3d-4f31-b6f7-03b3222c190a", "Capture RCN Service Level"),
						ResString.GetMultilingualString("34b50867-5b29-4fc5-a0fe-e33617e8e4bf", "When creating or selecting a Receive Consignment on mobile, prompt the user to capture the Service Level if not already entered."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#endregion

		#region Capture RCN Next Discharge Port

		public BooleanRegistryItem CaptureRCNNextDischargePort
		{
			get
			{
				return GetItem("CaptureRCNNextDischargePort", delegate
				{
					return new BooleanRegistryItem("CaptureRCNNextDischargePort",
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("0d52d432-e78c-4c97-ba76-f03927948031", "Capture RCN Next Discharge Port"),
						ResString.GetMultilingualString("bc416c63-50e0-4f92-a5c8-47d6c767fc5b", "When creating or selecting a Receive Consignment on mobile, prompt the user to capture the Next Discharge Port if not already entered."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#endregion

		#region Capture RCN Expected Dispatch

		public BooleanRegistryItem CaptureRCNExpectedDispatch
		{
			get
			{
				return GetItem("CaptureRCNExpectedDispatch", delegate
				{
					return new BooleanRegistryItem("CaptureRCNExpectedDispatch",
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("091aff23-c8ee-47d6-9a45-c4f2d040b60f", "Capture RCN Expected Dispatch"),
						ResString.GetMultilingualString("40e3c8bd-adaa-43d2-81da-30da82ea4324", "When creating or selecting a Receive Consignment on mobile, prompt the user to capture the Expected Dispatch if not already entered."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#endregion

		#region Capture Package Information Scan

		public BooleanRegistryItem CapturePackageInformationScan
		{
			get
			{
				return GetItem("CapturePackageInformationScan", delegate
				{
					return new BooleanRegistryItem("CapturePackageInformationScan",
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("86fc4c61-f7b9-467e-923c-712f019ca52c", "Capture Package Information Scan"),
						ResString.GetMultilingualString("2c5f1190-93bd-4d73-8c6c-b174678a0815", "When unloading a Received Transportation Unit, prompt to scan a barcode containing package information such as weight or dimensions."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#endregion

		#region Package Details Are Mandatory

		public BooleanRegistryItem PackageDetailsAreMandatory
		{
			get
			{
				return GetItem("PackageDetailsAreMandatory", delegate
				{
					return new BooleanRegistryItem("PackageDetailsAreMandatory",
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("c7a6ca0e-8892-4d50-be4f-c2c6190d0098", "Package Details are Mandatory"),
						ResString.GetMultilingualString("dc21fdda-5c5a-4b44-b648-8986830157ee", "Packages require dimensions, volume and weight to be recorded before they are Putaway, Cross-Docked or Loaded."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#endregion

		#region Allow Package Height Modification While Locked

		public BooleanRegistryItem AllowPackageHeightModificationWhileLocked
		{
			get
			{
				return GetItem("AllowPackageHeightModificationWhileLocked", delegate
				{
					return new BooleanRegistryItem("AllowPackageHeightModificationWhileLocked",
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("2212736c-37a6-4957-8af7-90f9a4f3c2d6", "Allow Package Height Modification While Locked"),
						ResString.GetMultilingualString("b5ce2f59-23c3-4838-8820-d136c4e5a7cf", "Will allow editing of package height while packages are locked, for example, for pallets with variable height."),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#endregion

		#region ActionsTakenToRCNsWhenFinishUnloading

		ICodeDescriptionPairListProvider ActionsTakenToRCNSWhenFinishUnloadingTypeListProvider
		{
			get
			{
				if (actionsTakenToRCNSWhenFinishUnloadingTypeListProvider == null)
				{
					actionsTakenToRCNSWhenFinishUnloadingTypeListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair("CLS", ResString.GetMultilingualString("b5be4b56-159b-494d-b398-756695dcdfe5", "Close Short RCNs"));
						list.AddPair("OPN", ResString.GetMultilingualString("63ec22cf-2985-46d4-a3fc-330f6fc1f8ae", "Leave Short RCNs Open"));
						list.AddPair("PMT", ResString.GetMultilingualString("aab2d3d7-ac71-4d31-a7c7-aeedafb234b7", "Prompt the operator"));
						list.DefaultCode = "PMT";
						return list;
					});
				}

				return actionsTakenToRCNSWhenFinishUnloadingTypeListProvider;
			}
		}
		ICodeDescriptionPairListProvider actionsTakenToRCNSWhenFinishUnloadingTypeListProvider;

		public CodeDescriptionPairList ActionsTakenToRCNSWhenFinishUnloadingTypeList
		{
			get
			{
				if (actionsTakenToRCNSWhenFinishUnloadingTypeList == null)
				{
					actionsTakenToRCNSWhenFinishUnloadingTypeList = ActionsTakenToRCNSWhenFinishUnloadingTypeListProvider.CodeDescriptionPairList;
				}

				return actionsTakenToRCNSWhenFinishUnloadingTypeList;
			}
		}
		CodeDescriptionPairList actionsTakenToRCNSWhenFinishUnloadingTypeList;

		public CodePairRegistryItem ActionsTakenToRCNSWhenFinishUnloading
		{
			get
			{
				return GetItem("ActionsTakenToRCNSWhenFinishUnloading", delegate
				{
					var item = new CodePairRegistryItem
					(
						"ActionsTakenToRCNSWhenFinishUnloading",
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("12980b31-ac3e-4688-89f5-4751a35da68c", "Actions Taken To RCNs When Finish Unloading"),
						ResString.GetMultilingualString("882b428d-49ef-45a1-bbf3-e312fa543996", "What action should be taken when an RTU is Finished Unloading and RCNs unloaded are received short?"),
						ActionsTakenToRCNSWhenFinishUnloadingTypeListProvider,
						RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						ActionsTakenToRCNSWhenFinishUnloadingTypeList.DefaultCode
					);
					return item;
				});
			}
		}

		#endregion

		#region DefaultReceiveConsignmentDirection

		public CodePairRegistryItem DefaultReceiveConsignmentDirection
		{
			get
			{
				return GetItem("DefaultReceiveConsignmentDirection", delegate
				{
					const string defaultReceiveConsignmentDirection = nameof(DefaultReceiveConsignmentDirection);
					var item = new CodePairRegistryItem
					(
						defaultReceiveConsignmentDirection,
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("c5d2fef9-43f4-454d-aef1-411bab5e44c6", "Default Receive Consignment Direction"),
						ResString.GetMultilingualString("823b6ac6-211c-40fb-ba9e-d2f3cc4c5313", "Set default transport direction when creating a new receive consignment."),
						new CodeDescriptionPairListProvider(() => new DefaultConsignmentDirectionCodeList()),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						DefaultConsignmentDirectionCodeList.Codes.Empty
					);

					return item;
				});
			}
		}

		#endregion

		#endregion

		#region Load

		#region DTU Transport Company Is Mandatory During Load

		public BooleanRegistryItem DTUTransportCompanyIsMandatoryDuringLoad
		{
			get
			{
				return GetItem("DTUTransportCompanyIsMandatoryDuringLoad", delegate
				{
					return new BooleanRegistryItem("DTUTransportCompanyIsMandatoryDuringLoad",
						Categories.Warehouse_TransitWarehouse_Load,
						ResString.GetMultilingualString("b0d57f09-2d94-4f3c-892b-e3d40851fbd6", "DTU Transport Company is Mandatory for Vehicle during Load"),
						ResString.GetMultilingualString("49178787-98b8-4b7f-94b3-44e9e13383b2", "Dispatch Transport Unit Transport Company is mandatory during the load process for Vehicle Load Types."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#region DefaultDispatchConsignmentDirection

		public CodePairRegistryItem DefaultDispatchConsignmentDirection
		{
			get
			{
				return GetItem("DefaultDispatchConsignmentDirection", delegate
				{
					const string defaultDispatchConsignmentDirection = nameof(DefaultDispatchConsignmentDirection);
					var item = new CodePairRegistryItem
					(
						defaultDispatchConsignmentDirection,
						Categories.Warehouse_TransitWarehouse_Load,
						ResString.GetMultilingualString("181dab79-3b18-43d3-92d0-fe62365b6c0d", "Default Dispatch Consignment Direction"),
						ResString.GetMultilingualString("2741a179-ba74-4496-b0c2-9ce16c22da93", "Set default transport direction when creating a new dispatch consignment."),
						new CodeDescriptionPairListProvider(() => new DefaultConsignmentDirectionCodeList()),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						DefaultConsignmentDirectionCodeList.Codes.Empty
					);

					return item;
				});
			}
		}

		#endregion

		#endregion

		#region Driver Signature

		#region Prompt For Driver Signature After Loading Vehicle

		public BooleanRegistryItem PromptForDriverSignatureAfterLoadingVehicle
		{
			get
			{
				return GetItem("PromptForDriverSignatureAfterLoadingVehicle", delegate
				{
					return new BooleanRegistryItem("PromptForDriverSignatureAfterLoadingVehicle",
						Categories.Warehouse_TransitWarehouse_Load_DriverSignature,
						ResString.GetMultilingualString("41163291-7e55-4963-837d-5f8c5b0b6f63", "Prompt for Driver Signature after loading vehicle"),
						ResString.GetMultilingualString("8d72eb43-4c56-4efe-aaee-539b8d497232", "Whether or not DTU load complete prompts for vehicle signature."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		#endregion

		#region Prompt For Driver Signature After Loading Container

		public BooleanRegistryItem PromptForDriverSignatureAfterLoadingContainer
		{
			get
			{
				return GetItem("PromptForDriverSignatureAfterLoadingContainer", delegate
				{
					return new BooleanRegistryItem("PromptForDriverSignatureAfterLoadingContainer",
						Categories.Warehouse_TransitWarehouse_Load_DriverSignature,
						ResString.GetMultilingualString("6a3a6012-3e65-4930-a787-7bd90d0740q2", "Prompt for Driver Signature after loading sea container"),
						ResString.GetMultilingualString("05597667-15e5-426d-8460-12e37ccd55w3", "Whether or not DTU load complete prompts for sea container signature."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		#endregion

		#region Prompt For Driver Signature After Loading Air Container

		public BooleanRegistryItem PromptForDriverSignatureAfterLoadingAirContainer
		{
			get
			{
				return GetItem("PromptForDriverSignatureAfterLoadingAirContainer", delegate
				{
					return new BooleanRegistryItem("PromptForDriverSignatureAfterLoadingAirContainer",
						Categories.Warehouse_TransitWarehouse_Load_DriverSignature,
						ResString.GetMultilingualString("6a3a6012-3e65-4930-a787-7bd90d074061", "Prompt for Driver Signature after loading air container"),
						ResString.GetMultilingualString("05597667-15e5-426d-8460-12e37ccd55a2", "Whether or not DTU load complete prompts for air container signature."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		#endregion

		#endregion

		#region Require Loading onto Vehicle for Dispatch

		#region ULDNeedLoadingOntoVehicle

		public DateTimeRegistryItem ULDNeedLoadingOntoVehicle
		{
			get
			{
				return GetItem("ULDNeedLoadingOntoVehicle", delegate
				{
					var dateRegistry = new DateTimeRegistryItem(
						"ULDNeedLoadingOntoVehicle",
						Categories.Warehouse_TransitWarehouse_Load,
						ResString.GetMultilingualString("7567ee8c-4a7d-49f6-85d3-4057cd789f96", "ULDs Require Loading onto Vehicle for Dispatch"),
						ResString.GetMultilingualString("e0bc6f28-8546-449b-b384-4ef967d31cfe", "When Registry Date is in the past, ULDs are required to be loaded onto a Vehicle. However, non-cleared and/or non-authorized packages can be picked and packed/loaded into a ULD.\r\n \r\nWhen Registry Date is Empty or in the future, ULDs can be gated out without loading them onto a Vehicle. However, non-cleared and non-authorized packages can not be picked or packed/loaded into a ULD."),
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Short),
						RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						DateTime.MinValue,
						false);
					dateRegistry.DataType = new DateWithinOneYearRegistryDataType();
					return dateRegistry;
				});
			}
		}

		#endregion

		#region CNTNeedLoadingOntoVehicle

		public BooleanRegistryItem CNTNeedLoadingOntoVehicle
		{
			get
			{
				return GetItem("CNTNeedLoadingOntoVehicle", delegate
				{
					return new BooleanRegistryItem("CNTNeedLoadingOntoVehicle",
						Categories.Warehouse_TransitWarehouse_Load,
						ResString.GetMultilingualString("fc12c8f3-fd85-4702-8fd4-96dd771b5849", "Sea Containers Require Loading onto Vehicle for Dispatch"),
						ResString.GetMultilingualString("d99b3643-09df-40da-949e-f5fda41324f1", "When Registry is Yes, Sea Containers are required to be loaded onto a Vehicle. However, non-cleared and/or non-authorized packages can be picked and packed/loaded into a Sea Container.\r\n \r\nWhen Registry is No, Sea Containers can be gated out without loading them onto a Vehicle. However, non-cleared and non-authorized packages can not be picked or packed/loaded into a Sea Container."),
						RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region Departed Package Auto-Finalization Delay

		public IntRegistryItem DepartedPackageAutoFinalizationDelay
		{
			get
			{
				return GetItem("DepartedPackageAutoFinalizationDelay", delegate
				{
					return new IntRegistryItem("DepartedPackageAutoFinalizationDelay",
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("fd07f080-9064-4a8d-86c5-7c25748cb704", "Departed Package Auto-Finalization Delay"),
						ResString.GetMultilingualString("ca62b33f-9590-4edf-b7b0-0d642127922b", @"Packages that have departed the warehouse are automatically finalized after the specified delay (in hours).

Note: Finalized packages no longer count toward DG limits."),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						24,
						0,
						2400);
				});
			}
		}

		#endregion

		#region EnableCoLoadAndAssemblyMasterSubConsignmentsCreation

		public BooleanRegistryItem EnableCoLoadAndAssemblyMasterSubConsignmentsCreation
		{
			get
			{
				const string enableCoLoadAndAssemblyMasterSubConsignmentsCreation = nameof(EnableCoLoadAndAssemblyMasterSubConsignmentsCreation);
				return GetItem(enableCoLoadAndAssemblyMasterSubConsignmentsCreation, delegate
				{
					return new BooleanRegistryItem(enableCoLoadAndAssemblyMasterSubConsignmentsCreation,
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("3e3ea204-8b70-4db0-9c17-dec06d947224", "Enable Consignment Creation for Sub-Shipments"),
						ResString.GetMultilingualString("76350b37-6209-445a-8382-c72cd5d8acda", "Enable Consignment Creation for CoLoads, Blind CoLoads and Assembly Master Sub-Shipments."),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		#endregion

		#region TransiReferenceMapping

		public TransitReferenceMappingRegistryItem TransitReferenceMapping
		{
			get
			{
				return GetItem<TransitReferenceMappingRegistryItem>("TransitReferenceMapping", delegate
				{
					var item = new TransitReferenceMappingRegistryItem(
						"TransitReferenceMapping",
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("2f4a44d6-be73-4fb3-849c-d1835c114a16", "Transit Warehouse Reference Mapping"),
						ResString.GetMultilingualString("b106c1c2-45b9-4fab-ac71-1def7659d837", "Please setup the reference mapping for transit warehouse."),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch
						);

					return item;
				});
			}
		}

		#endregion

		#region Transit Chareagable

		public ChargeableFactorRegistryItem TransitChargeableFactorForAir
		{
			get
			{
				const string transitChargeableFactorForAir = nameof(TransitChargeableFactorForAir);
				return GetItem<ChargeableFactorRegistryItem>(transitChargeableFactorForAir, () =>
				{
					var defaultFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.Air, ConversionFactor.Standard.Imperial.Domestic);
					return new ChargeableFactorRegistryItem(
						transitChargeableFactorForAir,
						Categories.Warehouse_TransitWarehouse_ChargeableForTransportationUnit,
						ResString.GetMultilingualString("14a715e3-7972-4b36-ad68-f674b5fed6fd", "Chargeable Factor for Air"),
						FreightDataRegistry.GetChargeableFactorVolumeToWeightHint(defaultFactor),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						defaultFactor);
				});
			}
		}

		public ChargeableFactorRegistryItem TransitChargeableFactorForSea
		{
			get
			{
				const string transitChargeableFactorForSea = nameof(TransitChargeableFactorForSea);
				return GetItem<ChargeableFactorRegistryItem>(transitChargeableFactorForSea, () =>
				{
					var defaultFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.Sea, ConversionFactor.Standard.Imperial.Domestic);
					return new ChargeableFactorRegistryItem(
						transitChargeableFactorForSea,
						Categories.Warehouse_TransitWarehouse_ChargeableForTransportationUnit,
						ResString.GetMultilingualString("e5f323d6-fd38-40f3-8d41-dfddc67dc8a6", "Chargeable Factor for Sea"),
						FreightDataRegistry.GetChargeableFactorVolumeToWeightHint(defaultFactor),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						defaultFactor);
				});
			}
		}

		public ChargeableFactorRegistryItem TransitChargeableFactorForRoad
		{
			get
			{
				const string transitChargeableFactorForRoad = nameof(TransitChargeableFactorForRoad);
				return GetItem<ChargeableFactorRegistryItem>(transitChargeableFactorForRoad, () =>
				{
					var defaultFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.Road, ConversionFactor.Standard.Imperial.Domestic);
					return new ChargeableFactorRegistryItem(
						transitChargeableFactorForRoad,
						Categories.Warehouse_TransitWarehouse_ChargeableForTransportationUnit,
						ResString.GetMultilingualString("52b38d0f-d02d-4787-a064-59bc6b5466d9", "Chargeable Factor for Road"),
						FreightDataRegistry.GetChargeableFactorVolumeToWeightHint(defaultFactor),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						defaultFactor);
				});
			}
		}

		#endregion

		#region Dispatch Instruction RCN Package Count Matching

		public CodePairRegistryItem DispatchInstructionRCNPackageCountMatchingType
		{
			get
			{
				return GetItem("DispatchInstructionRCNPackageCountMatchingType", delegate
				{
					return new CodePairRegistryItem(
						"DispatchInstructionRCNPackageCountMatchingType",
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("549DD91A-F3D6-41D2-8BF6-4AD69BAAB61F", "Dispatch Instruction RCN Package Count Matching Type"),
						ResString.GetMultilingualString("AE3056C2-15DA-496D-9017-FDF347A101C2", "When matching RCNs for Dispatching via UXML Import, use the following matching criteria."),
						new CodeDescriptionPairListProvider(() => DispatchInstructionRCNPackageCountMatchingTypeList),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						DispatchInstructionRCNPackageCountMatchingTypeList.DefaultCode);
				});
			}
		}

		public CodeDescriptionPairList DispatchInstructionRCNPackageCountMatchingTypeList
		{
			get
			{
				if (dispatchInstructionRCNPackageCountMatchingTypeList == null)
				{
					dispatchInstructionRCNPackageCountMatchingTypeList = new DispatchInstructionRCNPackageCountMatchingTypesList();
				}

				return dispatchInstructionRCNPackageCountMatchingTypeList;
			}
		}
		CodeDescriptionPairList dispatchInstructionRCNPackageCountMatchingTypeList;

		#endregion

		#region DefaultActiveRCNDuringUnloading

		public BooleanRegistryItem DefaultActiveRCNDuringUnloading
		{
			get
			{
				const string defaultActiveRCNDuringUnloading = nameof(DefaultActiveRCNDuringUnloading);
				return GetItem(defaultActiveRCNDuringUnloading, () =>
				{
					return new BooleanRegistryItem(defaultActiveRCNDuringUnloading,
						Categories.Warehouse_TransitWarehouse_Unload,
						ResString.GetMultilingualString("316A46B2-D117-465B-9D36-3F3899BE979E", "Default Active RCN When Multiple Expected"),
						ResString.GetMultilingualString("18A9F04C-6C4B-4B42-8CB4-80116DF9D12B", "When an ASN is selected to unload, if there are multiple RCNs expected on the ASN, default the Active Consignment to the First RCN found on the ASN.\r\n\r\nNote: If the ASN is only expecting one RCN, it will always default as the Active Consignment, regardless of the option chosen below."),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						true);
				});
			}
		}

		#region IgnoreDCNOnUXMLImportWhenPackagesCantBeFoundToAttach

		public BooleanRegistryItem IgnoreDCNOnUXMLImportWhenPackagesCantBeFoundToAttach
		{
			get
			{
				return GetItem("IgnoreDCNOnUXMLImportWhenPackagesCantBeFoundToAttach", delegate
				{
					return new BooleanRegistryItem(
						"IgnoreDCNOnUXMLImportWhenPackagesCantBeFoundToAttach",
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("1b15792a-946a-47c2-94d1-95326f4fcf98", "Ignore DCN on UXML Import when packages can't be found to attach"),
						ResString.GetMultilingualString("adda20d0-3f1b-4fd3-b341-7cc4976eac10",
@"This registry setting controls whether Dispatch Instructions, imported via UXML, are created when packages can't be found to attach to a DCN.

When set to No, the whole import will be rejected.

When set to Yes, the affected DCN will be ignored and the DLL will be created."),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region UseStaticHomePages

		public BooleanRegistryItem UseStaticHomePages
		{
			get
			{
				const string useStaticHomePages = nameof(UseStaticHomePages);
				return GetItem(useStaticHomePages, () =>
				{
					return new BooleanRegistryItem(useStaticHomePages,
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("aa507774-832c-46d2-8417-2143ee6c1664", "Use Static Home Pages"),
						ResString.GetMultilingualString("22d0fc3a-360f-4906-9cee-9c6bae94a500", "Removes most dynamic data from desktop and mobile home pages. Improves system performance by reducing server load, especially if there are many idle users logged in."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#endregion

		#region UnloadCompleteDateForRatingOfStorage

		public CodePairRegistryItem UnloadCompleteDateForRatingOfStorage
		{
			get
			{
				const string unloadCompleteDateForRatingOfStorage = nameof(UnloadCompleteDateForRatingOfStorage);
				return GetItem(unloadCompleteDateForRatingOfStorage, delegate
				{
					var item = new CodePairRegistryItem
					(
						unloadCompleteDateForRatingOfStorage,
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("b6f4429e-27a0-473a-acb8-90f13860a2c1", "Unload Complete Date for Rating of Storage"),
						ResString.GetMultilingualString("82038718-bc86-498f-8330-f3ac43e18bea", "Select the date autorating uses to calculate storage for packages."),
						new CodeDescriptionPairListProvider(() => new UnloadCompleteDateForRatingOfStorageList()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						UnloadCompleteDateForRatingOfStorageList.Codes.EachUnloadCompleteDate
					);

					return item;
				});
			}
		}

		#endregion

		#region Create Single ASN For All Containers

		public BooleanRegistryItem CreateSingleASNForAllContainers
		{
			get
			{
				const string createSingleASNForAllContainers = nameof(CreateSingleASNForAllContainers);
				return GetItem(createSingleASNForAllContainers, () =>
				{
					return new BooleanRegistryItem(createSingleASNForAllContainers,
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("df3d5b70-8686-4d77-9f2e-b90f5b7732f7", "Create Single ASN for All Containers"),
						ResString.GetMultilingualString("3db3f57b-0f66-4953-879d-39bccb7858c1", @"Always create an ASN per Forwarding Consol (via UXML), regardless of planned packing of Container.

Set to True to always create a single ASN on UXML read.
Set to False to create an ASN per Container when the TWH is an Import/Arrival TWH and the UXML indicates what packages to pack in each Container.

Please note, this registry does not control ASN creation for Export/Departure TWH."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region Create Single DLL For All Containers

		public BooleanRegistryItem CreateSingleDLLForAllContainers
		{
			get
			{
				const string CreateSingleDLLForAllContainers = nameof(CreateSingleDLLForAllContainers);
				return GetItem(CreateSingleDLLForAllContainers, () =>
				{
					return new BooleanRegistryItem(CreateSingleDLLForAllContainers,
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("735cb5f4-1e86-4a30-9867-dbc7faed7a1e", "Create Single DLL For All Containers"),
						ResString.GetMultilingualString("f031da16-cc94-445b-a471-527b745aaaf2", @"Always create a Load List per Forwarding Consol (via UXML), regardless of planned packing of Container.

Set to True to always create a single Load List on UXML read.
Set to False to create a Load List per Container when the TWH is an Export/Departure TWH the UXML indicates what packages to pack in each Container."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region EnableTransitWarehouseDashboard

		public BooleanRegistryItem EnableTransitWarehouseDashboard
		{
			get
			{
				const string enableTransitWarehouseDashboard = "EnableTransitWarehouseDashboard";

				return GetItem(enableTransitWarehouseDashboard, () =>
					new BooleanRegistryItem(
						enableTransitWarehouseDashboard,
						Categories.Warehouse_TransitWarehouse,
						(NoResString)"Enable Dashboard", // Support Only Registry Item, does not need translation
						(NoResString)"Enable Dashboard - DO NOT USE THIS ON PERSISTED TESTING SYSTEMS LIKE UAT ALPHA.", // Support Only Registry Item, does not need translation
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false));
			}
		}

		#endregion

		#region EnableCycleCountLastPackageScanCancellation

		public BooleanRegistryItem EnableCycleCountLastPackageScanCancellation
		{
			get
			{
				const string enableCycleCountLastPackageScanCancellation = nameof(EnableCycleCountLastPackageScanCancellation);
				return GetItem(enableCycleCountLastPackageScanCancellation, () =>
					new BooleanRegistryItem(
						enableCycleCountLastPackageScanCancellation,
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("20df39fb-e1dd-4728-8324-64617a94c352", "Enable Cycle Count Last Package Scan Cancellation"),
						ResString.GetMultilingualString("07e1614a-111a-48a1-a43d-89045818cb7e", "When enabled, an unexpected package scan can be canceled during a cycle count."),
						RegistryStorageFlags.Branch,
						false
				));
			}
		}

		#endregion

		#endregion

		#region References

		public CodeDescriptionPairListWithDefaultCodeRegistryItem PackageAdditionalReferenceType
		{
			get
			{
				return GetItem(TransitWarehousePackageAdditionalReferenceTypeRegistry.RegistryItemKey, delegate
				{
					var codeDescriptionPairList = new CodeDescriptionPairList();
					codeDescriptionPairList.AddPair(TransitWarehousePackageAdditionalReferenceTypes.Codes.InternetOfThings, ResString.GetMultilingualString("29cdc4a7-28c2-434e-bb7d-af39694740ff", TransitWarehousePackageAdditionalReferenceTypes.Descriptions.InternetOfThings));
					codeDescriptionPairList.AddPair(TransitWarehousePackageAdditionalReferenceTypes.Codes.ConsignmentOrDeliveryNoteNumber, ResString.GetMultilingualString("3edf3f95-e52e-4f33-af25-7c12b95894d7", TransitWarehousePackageAdditionalReferenceTypes.Descriptions.ConsignmentOrDeliveryNoteNumber));
					codeDescriptionPairList.AddPair(TransitWarehousePackageAdditionalReferenceTypes.Codes.ShipmentNumber, ResString.GetMultilingualString("d0177eff-dfe5-4615-9d9c-2acbd5a4c211", TransitWarehousePackageAdditionalReferenceTypes.Descriptions.ShipmentNumber));
					codeDescriptionPairList.AddPair(TransitWarehousePackageAdditionalReferenceTypes.Codes.CargoTrackingNumber, ResString.GetMultilingualString("60234f99-d1e6-417b-9e74-7f8fdfc4e3bd", TransitWarehousePackageAdditionalReferenceTypes.Descriptions.CargoTrackingNumber));

					return new CodeDescriptionPairListWithDefaultCodeRegistryItem(
						TransitWarehousePackageAdditionalReferenceTypeRegistry.RegistryItemKey,
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("914456ac-9744-441b-aaaf-c4659c1de5e8", "Package Additional Reference Types"),
						ResString.GetMultilingualString("c8e3e9bc-f10c-49a8-9ca6-4770017cce25", "The system defines default Additional Reference Types. Using this registry, you can define Package Additional Reference Types."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						codeDescriptionPairList, false, false, 3, false);
				});
			}
		}

		#endregion

		#region Packing Slip Title

		public MultilingualStringRegistryItem PackingSlipTitles
		{
			get
			{
				return GetItem<MultilingualStringRegistryItem>("PackingSlipTitles", delegate
				{
					var result = new MultilingualStringRegistryItem("PackingSlipTitles",
						Categories.Warehouse_PackingSlipTitle,
						ResString.GetMultilingualString("b17b74e0-f05b-4e84-a955-e09aa95f6b1c", "Packing Slip Title"),
						ResString.GetMultilingualString("4a7a405b-a101-402f-b967-d1250a28e4a2", "The title which will appear on the Packing Slip."),
						RegistryStorageFlags.Branch,
						ResString.GetMultilingualString("53f4ccf7-e88a-4f79-88ee-c9c7790bed82", "Packing Slip")
						);
					return result;
				});
			}
		}

		#endregion

		#region Picking

		#region PickMethod

		public CodeDescriptionPairListWithDefaultCodeRegistryItem PickMethod
		{
			get
			{
				return GetItem("PickMethod", delegate
				{
					var defaultValue = new CodeDescriptionPairList();

					defaultValue.AddPair("ANY", ResString.GetMultilingualString("cfbd478a-b7d6-4ae5-bf6a-97467cae7a08", "Any"));
					defaultValue.DefaultCode = "ANY";

					return new CodeDescriptionPairListWithDefaultCodeRegistryItem(
						"PickMethod",
						Categories.Warehouse_Picking,
						ResString.GetMultilingualString("51322c4e-b9d9-4809-88f2-fa45b0bbe54e", "Pick Methods"),
						ResString.GetMultilingualString("3836b49a-2aa6-4492-a215-bbfe5987ecb3", "Pick Methods are used to control the use of different equipment by location. For example you may have extra high locations that require a special hoist to access. If these Pick Methods are assigned to locations, when Putaway Sheets or Picking Slips or Stocktake Sheets are printed, they will have a page break by Pick Method so that different store-men using different equipment can work on the same job at the same time."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						defaultValue,
						true);
				});
			}
		}

		#endregion

		#region UOMPackTypes

		public UOMPackTypeRegistryItem UOMPackType
		{
			get
			{
				return GetItem<UOMPackTypeRegistryItem>("UOMPackType",
					() => new UOMPackTypeRegistryItem(
						"UOMPackType",
						Categories.Warehouse_Picking,
						ResString.GetMultilingualString("fb6fc660-5478-4707-8906-46ac1f607cdc", "UOM Pack Types"),
						ResString.GetMultilingualString("81b3540b-d682-4555-a480-c47b14027542", "A list of Warehouse UOM pack types."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						UOMPackTypeCollection.GetDefault()
						));
			}
		}

		#endregion

		#region PickGroups

		public PickGroupRegistryItem PickGroups
		{
			get
			{
				return GetItem("PickGroups", delegate
				{
					return new PickGroupRegistryItem(
						"PickGroups",
						Categories.Warehouse_Picking,
						ResString.GetMultilingualString("11f32543-c46f-4a7e-883c-cf3d9fd917aa", "Pick Groups"),
						ResString.GetMultilingualString("07555cbd-af06-4a17-93da-fe68bdfccb25",
							"The Pick Group enables automatic or manual grouping of Products for directed picking based on Product characteristics (for example Weight). " +
							"Such characteristics may impact the sequence in which products need to be picked on an Order."),
						RegistryStorageFlags.System);
				});
			}
		}

		#endregion

		#region Picking Sequence

		public PickingSequenceRegistryItem PickingSequence
		{
			get
			{
				return GetItem("WAREHOUSE_PICKING_SEQUENCE_READONLY", delegate
				{
					return new PickingSequenceRegistryItem(
						"WAREHOUSE_PICKING_SEQUENCE_READONLY",
						Categories.Warehouse_Picking,
						ResString.GetMultilingualString("20f55f48-0b5b-479e-ba0c-5a72c77f52a5", "Picking Sequence"),
						ResString.GetMultilingualString("e16053df-3643-4798-b6ce-4a02fd8f4eac", @"Pick Sequences define the order of execution for Pick Algorithms. System wide Pick Sequences are defined here. These can then be overridden per organization.

***Legacy Allocation Rules are being replaced with the Production Rules Engine. Allocation rules can be setup in the GLOW PRE portal, hyperlinks have been added in the warehouse configuration for organization.***"),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue | RegistryOptions.IsReadOnly);
				});
			}
		}

		#endregion

		#region Carton Group Sequence

		public CartonGroupSequenceRegistryItem CartonGroupSequence
		{
			get
			{
				const string cartonGroupName = "CartonGroupSequence";
				return GetItem(cartonGroupName, () =>
				{
					return new CartonGroupSequenceRegistryItem(
						cartonGroupName,
						Categories.Warehouse_Picking,
						ResString.GetMultilingualString("9f54b043-2769-4556-b34a-c044d8271f2b", "Carton Group Sequence"),
						ResString.GetMultilingualString("f245834f-46f2-4ff9-8811-66605ae14c85", "Carton Group Sequence defines the fallback sequence for the Carton Group used in the Cartonization process during Allocate Package Labels."),
						RegistryStorageFlags.Branch);
				});
			}
		}

		#endregion

		#region ShortPickingPriorityIncrement

		public IntRegistryItem ShortPickingPriorityIncrement
		{
			get
			{
				return GetItem("ShortPickingPriorityIncrement", delegate
				{
					return new IntRegistryItem("ShortPickingPriorityIncrement",
						Categories.Warehouse_Picking,
						ResString.GetMultilingualString("E065862D-610E-4BB0-B764-3D1EE4BA9A86", "Back Order Priority Increase."),
						ResString.GetMultilingualString("0A470588-43AA-4B2F-815D-E6E019B8DD9C", "This item allows the Priority that is assigned to Back Orders to be increased by taking the Priority from the original order, increasing that by a given value and assigning that higher Priority to the Back Order. Default value is increment by 1.."),
						RegistryStorageFlags.System,
						1);
				});
			}
		}

		#endregion

		#region SingleLineAutoPickFaceReplenishmentTransfers

		public BooleanRegistryItem SingleLineAutoPickFaceReplenishmentTransfers
		{
			get
			{
				return GetItem("SingleLineAutoPickFaceReplenishmentTransfers", delegate
				{
					return new BooleanRegistryItem("SingleLineAutoPickFaceReplenishmentTransfers",
							Categories.Warehouse_Transfers,
							ResString.GetMultilingualString("WarehouseDataRegistry|SingleLineAutoPickFaceReplenishmentTransfers|Caption", "Single-Line Fixed Pick Face Replenishment Transfers"),
							ResString.GetMultilingualString("WarehouseDataRegistry|SingleLineAutoPickFaceReplenishmentTransfers|Hint", "When this registry setting is enabled, the system will limit auto-replenishments for fixed pick faces to a single line per transfer."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, false);
				});
			}
		}

		#endregion

		#region PickByBiggestType

		public BooleanRegistryItem PickByBiggestType
		{
			get
			{
				return GetItem("PickByBiggestType", delegate
				{
					return new BooleanRegistryItem("PickByBiggestType",
						Categories.Warehouse_Picking,
						ResString.GetMultilingualString("WarehouseDataRegistry|PickByBiggestType|Caption", "Pick by Biggest Pack Type"),
						ResString.GetMultilingualString("WarehouseDataRegistry|PickByBiggestType|Hint", "When this registry setting is enabled, both the Pick Slip Document and RF Picking will let the user pick by Pack Type (as opposed to only units). The largest possible pack types are used, based on the Product Unit Conversions."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, defaultValue: true);
				});
			}
		}

		#endregion

		#region DeallocateLowerPriorityPicksWhenAllocatingWaitingReplenishmentPicks

		public BooleanRegistryItem DeallocateLowerPriorityPicksWhenAllocatingWaitingReplenishmentPicks
		{
			get
			{
				const string deallocateLowerPriorityPicksWhenAllocatingWaitingReplenishmentPicks = nameof(DeallocateLowerPriorityPicksWhenAllocatingWaitingReplenishmentPicks);
				return GetItem(deallocateLowerPriorityPicksWhenAllocatingWaitingReplenishmentPicks, () =>
				{
					return new BooleanRegistryItem(deallocateLowerPriorityPicksWhenAllocatingWaitingReplenishmentPicks,
						Categories.Warehouse_Picking,
						ResString.GetMultilingualString("WarehouseDataRegistry|DeallocateLowerPriorityPicksWhenAllocatingWaitingReplenishmentPicks|Caption", "Deallocate Lower Priority Picks When Allocating Waiting Replenishment Picks"),
							ResString.GetMultilingualString("WarehouseDataRegistry|DeallocateLowerPriorityPicksWhenAllocatingWaitingReplenishmentPicks|Hint", "When this registry setting is enabled, the system will deallocate lower priority picks first before allocating picks that are waiting replenishment as a lower priority pick might be withholding stock from a higher priority pick."),
							RegistryStorageFlags.System,
							false);
				});
			}
		}

		#endregion

		#endregion

		#region Putaway Sequence

		public PutawaySequenceRegistryItem PutawaySequence
		{
			get
			{
				return GetItem("WAREHOUSE_PUTAWAY_SEQUENCE", delegate
				{
					return new PutawaySequenceRegistryItem(
						"WAREHOUSE_PUTAWAY_SEQUENCE",
						Categories.Warehouse_Receive,
						ResString.GetMultilingualString("fda6eb7e-4e37-409f-a4f0-de400aa80abc", "Putaway Sequence"),
						ResString.GetMultilingualString("e47823f7-8008-41e6-9258-07cc0da22e79",
@"Putaway Sequences define the order of execution for Putaway Algorithms. System wide Putaway Sequences are defined here. These can then be overridden per organization.

***Legacy Putaway Rules are being replaced with the Putaway Production Rules Engine. Putaway rules can be setup in the GLOW PRE portal, hyperlinks have been added in the warehouse configuration for organization and product.***"),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue | RegistryOptions.IsReadOnly);
				});
			}
		}

		#endregion

		#region WaveCreationRulesFailureNotificationGroup

		public GuidRegistryItem WaveCreationRulesFailureNotificationGroup
		{
			get
			{
				return GetItem(nameof(WaveCreationRulesFailureNotificationGroup), () =>
				{
					var result = new GuidRegistryItem(
						nameof(WaveCreationRulesFailureNotificationGroup),
						Categories.Warehouse_Picking,
						ResString.GetMultilingualString("af09da11-1ea4-4c42-9e79-197c8797656d", "Wave Creation Rules Failure Notification Group"),
						ResString.GetMultilingualString("364f62fe-5fa3-4184-8b7b-0f9a2438a65c", "This group will be sent an email notification each time Wave Creation Rules are deactivated due to failures."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.NotCached | RegistryOptions.IsValueOptional,
						Core.Constants.Groups.PostMastersGroupPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region InventoryAccuracyManagement

		#region CycleCountingAutomationFailureNotificationGroup

		public GuidRegistryItem CycleCountingAutomationFailureNotificationGroup
		{
			get
			{
				const string cycleCountingAutomationFailureNotificationGroup = nameof(CycleCountingAutomationFailureNotificationGroup);
				return GetItem(cycleCountingAutomationFailureNotificationGroup, () =>
				{
					var result = new GuidRegistryItem(
						nameof(CycleCountingAutomationFailureNotificationGroup),
						Categories.Warehouse_InventoryAccuracyManagement,
						ResString.GetMultilingualString("b2b5e0ad-78d4-4b40-aee0-e09430f93263", "Cycle Count Task Creation Failure Notification Group"),
						ResString.GetMultilingualString("4d6a5df5-8588-4577-ac6f-61fd6bc85365", "This group will be sent Cycle Count Task Creation failure notifications."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.NotCached | RegistryOptions.IsValueOptional,
						Core.Constants.Groups.PostMastersGroupPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region InventoryAccuracyManagementMethod

		public CodePairRegistryItem InventoryAccuracyManagementMethod
		{
			get
			{
				return GetItem("InventoryAccuracyManagementMethod", delegate
				{
					return new CodePairRegistryItem(
						"InventoryAccuracyManagementMethod",
						Categories.Warehouse_InventoryAccuracyManagement,
						ResString.GetMultilingualString("0424e561-6b2e-4569-ab99-ea4eb271b99a", "Inventory Accuracy Management Method"),
						ResString.GetMultilingualString("3603c8a3-d2b2-4e0d-9f41-5df464b68ca2", "The Inventory Accuracy Management Method determines what process the system will use to check inventory accuracy."),
						InventoryAccuracyManagementMethodListProvider,
						RegistryStorageFlags.System,
						InventoryAccuracyManagementMethodList.DefaultCode);
				});
			}
		}

		ICodeDescriptionPairListProvider InventoryAccuracyManagementMethodListProvider
		{
			get
			{
				if (inventoryAccuracyManagementMethodListProvider == null)
				{
					inventoryAccuracyManagementMethodListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair(CycleCountCode, ResString.GetMultilingualString("0a2fa91f-e28a-49d5-90e8-ca82cdb24a6b", "Cycle Count"));
						list.AddPair(LegacyStocktakeCode, ResString.GetMultilingualString("3769b07e-4271-445c-ac05-6fdbb0999693", "Legacy Stocktake"));
						list.DefaultCode = CycleCountCode;
						return list;
					});
				}

				return inventoryAccuracyManagementMethodListProvider;
			}
		}
		ICodeDescriptionPairListProvider inventoryAccuracyManagementMethodListProvider;

		const string CycleCountCode = "WCC";
		const string LegacyStocktakeCode = "WST";

		public CodeDescriptionPairList InventoryAccuracyManagementMethodList => inventoryAccuracyManagementMethodList ?? (inventoryAccuracyManagementMethodList = InventoryAccuracyManagementMethodListProvider.CodeDescriptionPairList);
		CodeDescriptionPairList inventoryAccuracyManagementMethodList;

		public bool IsUsingLegacyStocktake => InventoryAccuracyManagementMethod.Value == LegacyStocktakeCode;

		#endregion

		#region StocktakeCycle

		public CodeDescriptionPairListRegistryItem StocktakeCycle
		{
			get
			{
				return GetItem("WarehouseStocktakeCycle", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"WarehouseStocktakeCycle",
						Categories.Warehouse_InventoryAccuracyManagement_LegacyStocktake,
						ResString.GetMultilingualString("ab7d52f9-80f2-4354-b347-c232af7bf461", "Stocktake Cycle"),
						ResString.GetMultilingualString("95a39813-a0f2-4233-8500-36e0b687b39f", "Stocktake Cycles can be used to organize cyclic stocktakes. When a stocktake/cycle count is created, one of these stocktake cycles can be selected to determine which stock is to be included in the stocktake cycle count. Example cycle codes may be days of the week / month, or types of product etc."),
						3,
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue);
				});
			}
		}

		#endregion

		#region StocktakeType

		public CodeDescriptionPairListWithDefaultCodeRegistryItem StocktakeTypes
		{
			get
			{
				return GetItem("WarehouseStocktakeType", delegate
				{
					return new CodeDescriptionPairListWithDefaultCodeRegistryItem(
						"WarehouseStocktakeType",
						Categories.Warehouse_InventoryAccuracyManagement_LegacyStocktake,
						ResString.GetMultilingualString("cd6b88cf-1755-4428-9111-f24c224625bc", "Stocktake Type"),
						ResString.GetMultilingualString("47d0b4d8-b50f-4f59-8142-62f3781750d0", "Stocktake Types be used to define customized stocktakes. The Stocktake Type on new Stocktakes are defaulted to the Stocktake Type marked below as 'Default'. Note: ATC and AZC can not be manually entered by a user, so can not be chosen as defaults."),
						RegistryStorageFlags.System,
						StocktakeTypesList,
						true
						);
				});
			}
		}

		CodeDescriptionPairList StocktakeTypesList => stocktakeTypesList ?? (stocktakeTypesList = GetstocktakeTypesList());
		CodeDescriptionPairList stocktakeTypesList;

		static CodeDescriptionPairList GetstocktakeTypesList()
		{
			var stocktakeTypes = new CodeDescriptionPairList();

			var standard = new CodeDescriptionBoolDefaultReadonly
			{
				Code = StocktakeTypeCodeList.Codes.Standard,
				Description = StocktakeTypeCodeList.Descriptions.Standard
			};
			var automaticTouchCount = new CodeDescriptionBoolDefaultReadonly
			{
				Code = StocktakeTypeCodeList.Codes.AutomaticTouchCount,
				Description = StocktakeTypeCodeList.Descriptions.AutomaticTouchCount,
				DefaultColumnReadOnly = true
			};
			var automaticZeroConfirmation = new CodeDescriptionBoolDefaultReadonly
			{
				Code = StocktakeTypeCodeList.Codes.AutomaticZeroConfirmation,
				Description = StocktakeTypeCodeList.Descriptions.AutomaticZeroConfirmation,
				DefaultColumnReadOnly = true
			};

			stocktakeTypes.Add(standard);
			stocktakeTypes.Add(automaticTouchCount);
			stocktakeTypes.Add(automaticZeroConfirmation);
			stocktakeTypes.DefaultCode = StocktakeTypeCodeList.Codes.Standard;

			return stocktakeTypes;
		}

		#endregion

		#endregion

		#region EnablePickLineLoaderBuildingStatusFilter

		public BooleanRegistryItem EnablePickLineLoaderBuildingStatusFilter
		{
			get
			{
				const string enablePickLineLoaderBuildingStatusFilter = nameof(EnablePickLineLoaderBuildingStatusFilter);
				return GetItem(enablePickLineLoaderBuildingStatusFilter, () =>
				{
					return new BooleanRegistryItem(enablePickLineLoaderBuildingStatusFilter,
						Categories.Warehouse_Picking,
						ResString.GetMultilingualString("9fcb3e8d-7463-4fe9-b1da-a8bae27d8b1e", "Enables RF Get Next Pick and Tote Picking To Filter Out Picks With Building Status"),
						ResString.GetMultilingualString("ff992f3b-493b-41ba-b45f-662a6234234b", "A Pick is created with a Building Status when at least one Order on the pick has their fulfillment rules not met."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#region CrossDockPendingOrders Buffer in Days

		public IntRegistryItem CrossDockPendingOrdersBufferInDays
		{
			get
			{
				return GetItem("CrossDockPendingOrdersBufferInDays", delegate
				{
					return new IntRegistryItem("CrossDockPendingOrdersBufferInDays",
						Categories.Warehouse_Receive,
						ResString.GetMultilingualString("a86e1694-1afc-4995-9573-4c5cb8876429", "Cross-Dock Pending Orders Buffer in Days"),
						ResString.GetMultilingualString("f6f279f2-3714-46e0-a303-a948093b44b6", "Specifies the minimum number of days to ignore orders for the cross docket pending order check. E.g. If you enter a value of 7, then all pending orders that are required within one week will be included in the warning, and all orders required more than a week in the future will be ignored.\r\n\r\nEnter a value of zero to disable this feature."),
						RegistryStorageFlags.System,
						7);
				});
			}
		}

		#endregion

		#region Max Split Count For Palletize Line

		public IntRegistryItem MaxSplitCountForPalletizeLines
		{
			get
			{
				const string registryName = nameof(MaxSplitCountForPalletizeLines);
				return GetItem(registryName, delegate
				{
					return new IntRegistryItem(registryName,
						Categories.Warehouse_Receive,
						ResString.GetMultilingualString("1f8dda01-5555-4135-8582-2fc17b9b1500", "Max Split Count For Palletize Lines"),
						ResString.GetMultilingualString("e3208e60-587e-40bb-8d16-2ca08054f251", "Specifies the maximum number of lines that can be created from a single Palletize Lines action. Creating a very large number of pallets at once is likely indicative of a data entry or setup issue and may perform poorly."),
						RegistryStorageFlags.System,
						1000);
				});
			}
		}

		#endregion

		#region Transport Co Default Rules

		public TransportCoDefaultingRulesRegistryItem TransportCoDefaultingRules
		{
			get
			{
				return GetItem("TransportCoDefaultingRules", delegate
				{
					return new TransportCoDefaultingRulesRegistryItem(
						"TransportCoDefaultingRules",
						Categories.Warehouse_Release,
						ResString.GetMultilingualString("096e3a2a-6bc6-48d1-a698-243d85df6382", "Transport Co Defaulting Rules"),
						ResString.GetMultilingualString("a18aff93-3a80-484d-a39d-7245c78f07bb", "Transport Co Defaulting Rules define the order of precedence when searching for a Default Transport Company."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
				});
			}
		}

		#endregion

		#region ABCAnalysis

		#region ABCAnalysisPeriod

		public CodePairRegistryItem ABCAnalysisPeriod
		{
			get
			{
				return GetItem("WarehouseABCAnalysisPeriod", delegate
				{
					return new CodePairRegistryItem(
						"WarehouseABCAnalysisPeriod",
						Categories.Warehouse_ABCAnalysis,
						ResString.GetMultilingualString("cab33ba8-fc39-4ff8-83ad-b4db624464b1", "ABC Analysis Period"),
						ResString.GetMultilingualString("7c797517-1442-40d8-b64d-a7ccf905f586",
							"The ABC Analysis Period determines the time range of transactions that will be used when calculating the ABC category of a product. The registry setting can be changed to suit the business requirements."),
						ABCAnalysisPeriodListProvider,
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						WhsABCAnalysisPeriodCodeList.Codes.Weekly);
				});
			}
		}

		#region ABCAnalysisPeriodList

		ICodeDescriptionPairListProvider ABCAnalysisPeriodListProvider
		{
			get
			{
				if (abcAnalysisPeriodListProvider == null)
				{
					abcAnalysisPeriodListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new WhsABCAnalysisPeriodCodeList();
						list.RemoveCode(WhsABCAnalysisPeriodCodeList.Codes.Default);
						return list;
					});
				}

				return abcAnalysisPeriodListProvider;
			}
		}
		ICodeDescriptionPairListProvider abcAnalysisPeriodListProvider;

		public CodeDescriptionPairList ABCAnalysisPeriodList
		{
			get
			{
				return abcAnalysisPeriodList ?? (abcAnalysisPeriodList = ABCAnalysisPeriodListProvider.CodeDescriptionPairList);
			}
		}
		CodeDescriptionPairList abcAnalysisPeriodList;

		#endregion

		#endregion

		#region ABCAnalysisMethod

		public CodePairRegistryItem ABCAnalysisMethod
		{
			get
			{
				return GetItem("WarehouseABCAnalysisMethod", delegate
				{
					return new CodePairRegistryItem(
						"WarehouseABCAnalysisMethod",
						Categories.Warehouse_ABCAnalysis,
						ResString.GetMultilingualString("16ddf88a-4764-43f8-8bc0-4e96ffc81af6", "ABC Analysis Method"),
						ResString.GetMultilingualString("5ad3e5e8-e42e-4d98-a910-a68dc7440760", "The ABC Analysis Method determines how the system will use the analysis period's transaction data to calculate the category."),
						ABCAnalysisMethodListProvider,
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						WhsABCAnalysisMethodCodeList.Codes.Velocity);
				});
			}
		}

		#region ABCAnalysisMethodList

		ICodeDescriptionPairListProvider ABCAnalysisMethodListProvider
		{
			get
			{
				if (abcAnalysisMethodListProvider == null)
				{
					abcAnalysisMethodListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new WhsABCAnalysisMethodCodeList();
						list.RemoveCode(WhsABCAnalysisMethodCodeList.Codes.Default);
						return list;
					});
				}

				return abcAnalysisMethodListProvider;
			}
		}
		ICodeDescriptionPairListProvider abcAnalysisMethodListProvider;

		public CodeDescriptionPairList ABCAnalysisMethodList
		{
			get { return abcAnalysisMethodList ?? (abcAnalysisMethodList = ABCAnalysisMethodListProvider.CodeDescriptionPairList); }
		}
		CodeDescriptionPairList abcAnalysisMethodList;

		#endregion

		#endregion

		#region ABCAnalysisCategories

		public ABCAnalysisCategoryRegistryItem ABCAnalysisCategories
		{
			get
			{
				return GetItem("WarehouseABCAnalysisCategories", delegate
				{
					return new ABCAnalysisCategoryRegistryItem(
						"WarehouseABCAnalysisCategories",
						Categories.Warehouse_ABCAnalysis,
						ResString.GetMultilingualString("c1ae2047-6678-41cc-a1b3-513d2f9657e9", "ABC Analysis Categories"),
						ResString.GetMultilingualString("b4a08152-be6d-49e1-a9a1-30935c8533ef", "The ABC Analysis Category determines how the period and method data are categorized and displayed to users of the Warehouse module."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						ABCAnalysisCategoryCollection.Default);
				});
			}
		}

		#endregion

		#endregion

		#region DocketReferenceType

		public CodeDescriptionPairListWithDefaultCodeRegistryItem AdditionalReferenceType
		{
			get
			{
				return GetItem(WarehouseDocketReferenceTypeRegistry.RegistryItemKey, delegate
				{
					/// Any changes made to this CodeDescriptionPairList should also be made to the following files to keep Enterprise and Glow in sync
					/// CWShared\CargoWise.Definitions\src\Definitions\Warehouse\WarehouseDocketReferenceTypeRegistry.cs
					/// Glow\DotNet\Business\Forwarding\Business.Forwarding.Service\TransitWarehouse\WarehouseAdditionalReferenceTypes.cs
					var defaultValue = new CodeDescriptionPairList();
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, ResString.GetMultilingualString("5bed0a60-c1d3-4740-8db3-1dd76775a484", WarehouseAdditionalReferenceTypes.Descriptions.BookingPartyReference));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.CustomsApprovalNumber, ResString.GetMultilingualString("e9378be4-900b-4d50-b5f9-995de69067af", WarehouseAdditionalReferenceTypes.Descriptions.CustomsApprovalNumber));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.HouseBill, ResString.GetMultilingualString("164f0ba8-7238-4ee9-a287-e0144b54992d", WarehouseAdditionalReferenceTypes.Descriptions.HouseBill));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.MarksAndNumbers, ResString.GetMultilingualString("27e9f6e4-f9ce-4b45-aea1-b16fe9be3c71", WarehouseAdditionalReferenceTypes.Descriptions.MarksAndNumbers));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.MasterBill, ResString.GetMultilingualString("f8c07d9f-a113-483e-b732-be3f6caa8e3b", WarehouseAdditionalReferenceTypes.Descriptions.MasterBill));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.Other, ResString.GetMultilingualString("8bd99d35-d7cd-48e9-8cc7-abb0f2082128", WarehouseAdditionalReferenceTypes.Descriptions.Other));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.TransportReference, ResString.GetMultilingualString("8b58e31c-6d6a-4d65-8d31-d531d7fcfed7", WarehouseAdditionalReferenceTypes.Descriptions.TransportReference));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.PreMergeJobNoCode, ResString.GetMultilingualString("deaf482e-2010-4a87-8c54-2d3c543e9d1b", WarehouseAdditionalReferenceTypes.Descriptions.PreMergeJobNoCode));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.PreMergeJobRefCode, ResString.GetMultilingualString("080573ee-8989-4f5d-aa4d-aa79ba3db9e7", WarehouseAdditionalReferenceTypes.Descriptions.PreMergeJobRefCode));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.VendorIDCode, ResString.GetMultilingualString("307f4e39-f0b5-44ca-8650-13f1a7e34bf4", WarehouseAdditionalReferenceTypes.Descriptions.VendorIDCode));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber, ResString.GetMultilingualString("f7b3e0b5-4583-4ccf-9e84-c2b8fd197408", WarehouseAdditionalReferenceTypes.Descriptions.VoyageFlightNumber));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.VehicleNumber, ResString.GetMultilingualString("83d687aa-7f7c-4fc1-b289-e2f49e971437", WarehouseAdditionalReferenceTypes.Descriptions.VehicleNumber));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.WaybillNumber, ResString.GetMultilingualString("75a6a66e-4c38-42f7-97dd-829c7a4b9110", WarehouseAdditionalReferenceTypes.Descriptions.WaybillNumber));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.DepartmentNameCode, ResString.GetMultilingualString("WarehouseDataRegistry|WarehouseDocketReferenceType|DepartmentName", WarehouseAdditionalReferenceTypes.Descriptions.DepartmentNameCode));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.DepartmentNumberCode, ResString.GetMultilingualString("WarehouseDataRegistry|WarehouseDocketReferenceType|DepartmentNumber", WarehouseAdditionalReferenceTypes.Descriptions.DepartmentNumberCode));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.OrderNumber, ResString.GetMultilingualString("344a3d2b-8897-4591-b02d-2129ca45f853", WarehouseAdditionalReferenceTypes.Descriptions.OrderNumber));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.OrderTypeCode, ResString.GetMultilingualString("WarehouseDataRegistry|WarehouseDocketReferenceType|OrderType", WarehouseAdditionalReferenceTypes.Descriptions.OrderTypeCode));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.EventTypeCode, ResString.GetMultilingualString("WarehouseDataRegistry|WarehouseDocketReferenceType|EventType", WarehouseAdditionalReferenceTypes.Descriptions.EventTypeCode));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|ExternalTransportBookingNumber", WarehouseAdditionalReferenceTypes.Descriptions.ExternalTransportBookingNumber));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.CutOffDate, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|CutOffDate", WarehouseAdditionalReferenceTypes.Descriptions.CutOffDate));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.ETDDate, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|ETDDate", WarehouseAdditionalReferenceTypes.Descriptions.ETDDate));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.DestinationPort, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|DestinationPort", WarehouseAdditionalReferenceTypes.Descriptions.DestinationPort));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.Vessel, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|Vessel", WarehouseAdditionalReferenceTypes.Descriptions.Vessel));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.InvoiceNumber, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|InvoiceNumber", WarehouseAdditionalReferenceTypes.Descriptions.InvoiceNumber));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.DriverName, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|DriverName", WarehouseAdditionalReferenceTypes.Descriptions.DriverName));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.DriverLicense, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|DriverLicense", WarehouseAdditionalReferenceTypes.Descriptions.DriverLicense));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|RunSheetNumber", WarehouseAdditionalReferenceTypes.Descriptions.RunSheetNumber));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|ForwardingConsolNumber", WarehouseAdditionalReferenceTypes.Descriptions.ForwardingConsolNumber));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.AssignedPicker, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|PickerName", WarehouseAdditionalReferenceTypes.Descriptions.AssignedPicker));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|ForwardingShipmentNumber", WarehouseAdditionalReferenceTypes.Descriptions.ForwardingShipmentNumber));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.TransportMode, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|TransportMode", WarehouseAdditionalReferenceTypes.Descriptions.TransportMode));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.SealNumber, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|SealNumber", WarehouseAdditionalReferenceTypes.Descriptions.SealNumber));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.VesselLloyds, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|VesselLloyds", WarehouseAdditionalReferenceTypes.Descriptions.VesselLloyds));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|CarrierBookingReference", WarehouseAdditionalReferenceTypes.Descriptions.CarrierBookingReference));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentDescription, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|ForwardingShipmentDescription", WarehouseAdditionalReferenceTypes.Descriptions.ForwardingShipmentDescription));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|ThirdPartyCarrierAccountNumber", WarehouseAdditionalReferenceTypes.Descriptions.ThirdPartyCarrierAccountNumber));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.T1, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|T1", WarehouseAdditionalReferenceTypes.Descriptions.T1));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.T2, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|T2", WarehouseAdditionalReferenceTypes.Descriptions.T2));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.T2L, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|T2L", WarehouseAdditionalReferenceTypes.Descriptions.T2L));
					defaultValue.AddPair(WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment, ResString.GetMultilingualString("WarehouseDataRegistry|AdditionalReferenceType|AssemblyMasterShipment", WarehouseAdditionalReferenceTypes.Descriptions.AssemblyMasterShipment));

					defaultValue.DefaultCode = WarehouseAdditionalReferenceTypes.Codes.Other;

					return new CodeDescriptionPairListWithDefaultCodeRegistryItem(
						WarehouseDocketReferenceTypeRegistry.RegistryItemKey,
						new[] { Categories.Warehouse_General, Categories.Transport_LandAndPortTransport_PortTransport },
						ResString.GetMultilingualString("53E08C5C-47C2-4842-BA62-556A1E9ADA36", "Additional Reference Types"),
						ResString.GetMultilingualString("b50ab7ef-3522-46fc-8de2-326634bed43b", "The system defines default Additional Reference Types. Using this registry, you can define custom Reference Types."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						defaultValue, false, false, 3);
				});
			}
		}

		#endregion

		#region SOHLocationWarning

		public BooleanRegistryItem SOHLocationWarning
		{
			get
			{
				return GetItem("SOHLocationWarning", delegate
				{
					return new BooleanRegistryItem("SOHLocationWarning",
						Categories.Warehouse_Receive,
						ResString.GetMultilingualString("d04009a2-a6b5-4c55-a123-c66039e380ae", "Stock On Hand Location Warning"),
						ResString.GetMultilingualString("f16e2e18-f85f-42c7-b1e9-46eb7b06829b", "Warn operator if Stock On Hand already exists in the location in which they are attempting to place the product."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, false);
				});
			}
		}

		#endregion

		#region TotalUnitsValidation

		public BooleanRegistryItem TotalUnitsValidation
		{
			get
			{
				return GetItem("TotalUnitsValidation", delegate
				{
					return new BooleanRegistryItem("TotalUnitsValidation",
						Categories.Warehouse_Receive,
						ResString.GetMultilingualString("08D7CF28-6980-45EC-90F1-7CA9A8A5588D", "Total Units Validation"),
						ResString.GetMultilingualString("4D926B1D-DFD8-4C6E-B8C7-139228FCFB45", "Enabling this setting will cause a receipt to display an error when the receipt is finalized if the Total Units field is not equal to the sum of all the line quantities. The same validation applies to work orders if this setting is enabled as work orders creates a receipt on finalization."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, false);
				});
			}
		}

		#endregion

		#region TotalPalletsValidation

		public BooleanRegistryItem TotalPalletsValidation
		{
			get
			{
				return GetItem("TotalPalletsValidation", delegate
				{
					return new BooleanRegistryItem("TotalPalletsValidation",
						Categories.Warehouse_Receive,
						ResString.GetMultilingualString("d18a11cd-007a-49f3-960f-7dfcda993345", "Total Pallets Validation"),
						ResString.GetMultilingualString("B913A6F6-BB20-4798-841E-C4660E506AF8", "Enabling this setting will display an error when the total number of received Pallets is not equal to the expected number of Pallets in the Total Pallets field."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, false);
				});
			}
		}

		#endregion

		#region ReceiveCategories

		public CodeDescriptionPairListWithDefaultCodeRegistryItem ReceiveCategories
		{
			get
			{
				const string receiveCategories = nameof(ReceiveCategories);

				// Used CodeDescriptionPairListWithDefaultCodeRegistryItem with empty default list for consistency and more intuitive validation messages.
				return GetItem(receiveCategories, () =>
				{
					return new CodeDescriptionPairListWithDefaultCodeRegistryItem(receiveCategories,
						Categories.Warehouse_Receive,
						ResString.GetMultilingualString("fbc03775-9c46-4881-be54-d941ae3b6d1d", "Receive Categories"),
						ResString.GetMultilingualString("6b3820f8-5af4-4435-b958-a24d802030a9", "A list of Warehouse Receive Categories"),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new CodeDescriptionPairList(),
						canEditDefaultValue: false,
						useDefaultFlag: false,
						codeMaxLength: 3,
						isDefaultValueMandatory: false);
				});
			}
		}

		#endregion

		#region EnableWarehouseRFVolcam

		public BooleanRegistryItem EnableRFVolcam
		{
			get
			{
				const string enableRFVolcam = nameof(EnableRFVolcam);
				return GetItem(enableRFVolcam, delegate
				{
					return new BooleanRegistryItem(enableRFVolcam,
						Categories.Warehouse_VolCam,
						ResString.GetMultilingualString("aea0213a-b7fd-4eeb-b65d-6b0106e7b5f6", "Enable Warehouse RF Vol-Cam"),
						ResString.GetMultilingualString("9d7686e2-dd34-409d-b45e-c9cc64729d30", "Enable Vol-Cam for Warehouse RF devices and the related functionality."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region ValidateConcurrentRFLogins

		public BooleanRegistryItem ValidateConcurrentRFLogins
		{
			get
			{
				const string validateConcurrentRFLogins = nameof(ValidateConcurrentRFLogins);
				return GetItem(validateConcurrentRFLogins, delegate
				{
					return new BooleanRegistryItem(validateConcurrentRFLogins,
						Categories.Warehouse_General,
						ResString.GetMultilingualString("62bdeba2-5348-4874-8344-2e11742ea35c", "Validate Concurrent RF Logins"),
						ResString.GetMultilingualString("0925c19f-6fff-4703-986d-8c6e75e3b347", "Enable concurrent login checking on the Warehouse RF web service."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		#endregion

		#region RFInactivityLimit

		public IntRegistryItem RFInactivityPeriod
		{
			get
			{
				const string rfInactivityPeriod = nameof(RFInactivityPeriod);
				return GetItem(rfInactivityPeriod, () =>
				{
					return new IntRegistryItem(rfInactivityPeriod,
						Categories.Warehouse_General,
						ResString.GetMultilingualString("852d6a05-2c93-4fc1-92db-57463ac9af26", "Android RF Inactivity Period"),
						ResString.GetMultilingualString("18332e7f-c576-4d1c-abc3-7b20eed65bab", "Specify a time (in minutes) after which the Warehouse RF application (Android only) will automatically log the user out if they have been idle. Changes will take effect on next login to the Warehouse RF application."),
						RegistryStorageFlags.System,
						0);
				});
			}
		}

		#endregion

		#region CheckAndroidDeviceVersion

		public BooleanRegistryItem CheckAndroidDeviceVersion
		{
			get
			{
				const string CheckAndroidDeviceVersion = nameof(CheckAndroidDeviceVersion);
				return GetItem(CheckAndroidDeviceVersion, delegate
				{
					return new BooleanRegistryItem(CheckAndroidDeviceVersion,
						Categories.Warehouse_General,
						(NoResString)"Validate Android Device Version",
						(NoResString)"This registry controls whether there is a need to validate the Warehouse RF Android device version in the warehouse webservice or not. By default, this registry will be set to true. THIS VALUE SHOULD ONLY BE OVERRIDDEN IN A TEST ENVIRONMENT.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						true);
				});
			}
		}

		#endregion

		#region WinCEApplicationUsageEnabledUntil

		public DateTimeRegistryItem WinCEApplicationUsageEnabledUntil
		{
			get
			{
				const string WinCEApplicationUsageEnabledUntil = nameof(WinCEApplicationUsageEnabledUntil);
				return GetItem(WinCEApplicationUsageEnabledUntil, delegate
				{
					return new DateTimeRegistryItem(WinCEApplicationUsageEnabledUntil,
						Categories.Warehouse_General,
						(NoResString)"WinCE Application Usage Enabled Until",
						(NoResString)"This registry controls the deprecation date for WinCE devices to be used. This value should only be overriden in exceptional circumstances.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						new DateTime(2026, 01, 31, 00, 00, 00, DateTimeKind.Utc));
				});
			}
		}

		#endregion

		#region VirtualWarehouseOnRFEnabledUntil

		public DateTimeRegistryItem VirtualWarehouseOnRFEnabledUntil
		{
			get
			{
				return GetItem(nameof(VirtualWarehouseOnRFEnabledUntil),
					() =>
					{
						var dateRegistry = new DateTimeRegistryItem(
							nameof(VirtualWarehouseOnRFEnabledUntil),
							Categories.Warehouse_General,
							ResString.GetMultilingualString("e7007433-9ccd-48c1-ba28-90489b0613b4", "Virtual Warehouse On RF Enabled Until"),
							ResString.GetMultilingualString("c64f3bf8-0da6-4ea8-8e30-6bc08dd574e3", "The UTC date that we support Virtual Warehouses on RF until."),
							new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Short),
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							DateTime.MinValue,
							false);
						dateRegistry.DataType = new DateWithinOneYearRegistryDataType();
						return dateRegistry;
					});
			}
		}

		#endregion

		#region ValidateDPSRestrictionsOnFinalisation

		public BooleanRegistryItem ValidateDPSRestrictionsOnFinalisation
		{
			get
			{
				const string validateDPSRestrictionsOnFinalisation = nameof(ValidateDPSRestrictionsOnFinalisation);
				return GetItem(validateDPSRestrictionsOnFinalisation, () =>
				{
					return new BooleanRegistryItem(validateDPSRestrictionsOnFinalisation,
						Categories.Warehouse_General,
						ResString.GetMultilingualString("5f5862d2-a1a8-49f8-b0d5-622f882d70af", "Check the Denied Party Screening (DPS) Status on Finalization"),
						ResString.GetMultilingualString("8f9b99d5-ff14-4a01-9fb1-6f9b9ab8d50b", "Enable DPS-specific restrictions on finalizing Warehouse Orders and Receipts."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#region AutoFinalizePick

		public BooleanRegistryItem AutoFinalizePick
		{
			get
			{
				return GetItem("AutoFinalizePick", delegate
				{
					return new BooleanRegistryItem("AutoFinalizePick",
						Categories.Warehouse_Release,
						ResString.GetMultilingualString("0a428edf-7e14-4d61-8578-a42b56bde397", "Auto Finalize Pick"),
						ResString.GetMultilingualString("e3b573b4-0ff4-42bc-9836-28dd0fe1be23", "The system will automatically finalize the Pick when all the Orders are finalized."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, false);
				});
			}
		}

		#endregion

		#region AutoFinalizeOrders

		public BooleanRegistryItem AutoFinalizeOrders
		{
			get
			{
				return GetItem("AutoFinalizeOrders", delegate
				{
					return new BooleanRegistryItem("AutoFinalizeOrders",
						Categories.Warehouse_Release,
						ResString.GetMultilingualString("fef9512d-6a42-41b4-80f7-ed3f36b7fb1d", "Auto Finalize Orders"),
						ResString.GetMultilingualString("ebe2a099-6612-45b8-bc26-630a1c835d50", "Automatically finalize orders during pick finalization."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, false);
				});
			}
		}

		#endregion

		#region DisablePrintingLabelsOnAutoPack

		public BooleanRegistryItem DisablePrintingLabelsOnAutoPack
		{
			get
			{
				const string name = "DisablePrintingLabelsOnAutoPack";
				return GetItem(name, () =>
				{
					return new BooleanRegistryItem(name,
						Categories.Warehouse_Release,
						ResString.GetMultilingualString("31dca2ab-16d4-461a-b07e-d9c940e8e610", "Disable Printing Labels on Auto-Pack"),
						ResString.GetMultilingualString("2be66e64-01cd-47d2-85d7-ba0f6aca39dd", "When ticked, Labels will no longer print after Auto-Pack operational action is used for Warehouse Orders."),
						RegistryStorageFlags.System,
						defaultValue: false);
				});
			}
		}

		#endregion

		#region Add Pallet Weight to Order

		public BooleanRegistryItem AddPalletWeightToOrder
		{
			get
			{
				return GetItem("AddPalletWeightToOrder", delegate
				{
					return new BooleanRegistryItem("AddPalletWeightToOrder",
							Categories.Warehouse_Release,
							ResString.GetMultilingualString("fd01a158-2a4e-452f-84dc-4e2979fea3cf", "Add Pallet Weight to Order when not using Packing"),
							ResString.GetMultilingualString("02981959-0c03-4769-a35b-a44f77a46c3d", "If the Order is not using Packing (Packages on the Packing Tab), add the weight of the total number of pallets specified on the Order Release to the total weight of the order by default (defaulted setting can be overridden on release). Otherwise this registry has no effect."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, false);
				});
			}
		}

		#endregion

		#region WarnFinalizePick

		public BooleanRegistryItem WarnFinalizePick
		{
			get
			{
				return GetItem("WarnFinalizePick", delegate
				{
					return new BooleanRegistryItem("WarnFinalizePick",
						Categories.Warehouse_Release,
						ResString.GetMultilingualString("67529259-f9be-46d0-84f6-489ded3432fd", "Finalize Pick Warning"),
						ResString.GetMultilingualString("5232217b-41a6-4837-bbfe-9bd3645e9039", "If all Orders are finalized but the Pick is not finalized, the system will warn you when the Release screen is closed."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, false);
				});
			}
		}

		#endregion

		#region AllowOrderToBeFinalisedWithoutCustomsClearance

		public BooleanRegistryItem AllowOrderToBeFinalisedWithoutCustomsClearance
		{
			get
			{
				const string allowOrderToBeFinalisedWithoutCustomsClearance = nameof(AllowOrderToBeFinalisedWithoutCustomsClearance);
				return GetItem(allowOrderToBeFinalisedWithoutCustomsClearance, () =>
				{
					return new BooleanRegistryItem(allowOrderToBeFinalisedWithoutCustomsClearance,
						Categories.Warehouse_Release,
						ResString.GetMultilingualString("3ec6ccfe-a3a0-4312-8439-c6be72acf45b", "Allow Orders To Be Finalized Without Customs Clearance"),
						ResString.GetMultilingualString("697aade0-f3b5-423e-8178-cb9a9cfd0502", "Allow Warehouse Orders to be released even if the Order is waiting on clearance from Customs."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region SupervisorOverrideForPartialOrderLoading

		public StringRegistryItem SupervisorOverrideForPartialOrderLoading
		{
			get
			{
				return GetItem("SupervisorOverrideForPartialOrderLoading", delegate
				{
					return new StringRegistryItem("SupervisorOverrideForPartialOrderLoading",
						Categories.Warehouse_Release,
						ResString.GetMultilingualString("1daab13f-4e6f-44ec-a1c6-37d31328d924", "Supervisor Override For Partial Order Loading"),
						ResString.GetMultilingualString("a747c69a-fe41-4f9c-9136-3cbdcb8508c4", "The supervisor override code to allow shipping partially loaded orders on RF devices. The override code is \"allow\" by default."),
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Password),
						RegistryStorageFlags.Branch,
						RegistryOptions.IsValueMandatory,
						defaultValue: (NoResString)"allow");
				});
			}
		}

		#endregion

		#region Packing Slip Order By

		public CodePairRegistryItem PackingSlipOrderBy
		{
			get
			{
				return GetItem("PackingSlipOrderBy", delegate
				{
					return new CodePairRegistryItem(
						"PackingSlipOrderBy",
						Categories.Warehouse_Release,
						ResString.GetMultilingualString("ca4d2355-723a-41ee-be63-326cd22b7510", "Packing Slip Order By"),
						ResString.GetMultilingualString("19ff7be9-78b2-4ff3-a84e-ce206e6e56ac", "Specify the default Sort Order By for Lines on Order Copy/Packing Slip Documents."),
						new PackingSlipOrderByListProvider(),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						"PRC");
				});
			}
		}

		class PackingSlipOrderByListProvider : ICodeDescriptionPairListProvider
		{
			CodeDescriptionPairList ICodeDescriptionPairListProvider.CodeDescriptionPairList
			{
				get { return (CodeDescriptionPairList)ObjectFactory.Get<IMasterFilesListProvider>().PackingSlipOrderByList(); }
			}
		}

		#endregion

		#region Break System Defined Pick Slip By Area

		public BooleanRegistryItem BreakSystemDefinedPickSlipByArea
		{
			get
			{
				return GetItem("BreakSystemDefinedPickSlipByArea", delegate
				{
					return new BooleanRegistryItem("BreakSystemDefinedPickSlipByArea",
							Categories.Warehouse_Picking,
							ResString.GetMultilingualString("0acb1281-f793-4b04-82bc-eccd68f20ad2", "Break Pick Slip by Area"),
							ResString.GetMultilingualString("1e0efff0-bc0a-4847-80e4-0b8834e08e8d", "When enabled, Picking Slip Documents will show Pick Area and Group by Area."),
							RegistryStorageFlags.System | RegistryStorageFlags.Branch, false);
				});
			}
		}

		#endregion

		#region Unsupported Pack Type Error

		public BooleanRegistryItem PackTypesValidation
		{
			get
			{
				return GetItem(nameof(PackTypesValidation), () =>
				{
					return new BooleanRegistryItem(nameof(PackTypesValidation),
						Categories.Warehouse_General,
						ResString.GetMultilingualString("2ae0bca2-cfad-4b62-8da3-f7d54a6daad7", "Pack Types Validation"),
						ResString.GetMultilingualString("f0b1e63a-974c-4bf4-a874-1f14ca0f6845", "Enabling this setting will display an error when the entered Pack Type is not defined for the Product."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, false);
				});
			}
		}

		#endregion

		#region Job Import

		#region Warehouse Job Notification Staff Roles

		public CodeDescriptionBoolRegistryItem WarehouseJobNotificationStaffRoles
		{
			get
			{
				return GetItem<CodeDescriptionBoolRegistryItem>("WarehouseJobNotificationStaffRoles", delegate
				{
					return new CodeDescriptionBoolDisallowNewRegistryItem(
						"WarehouseJobNotificationStaffRoles",
						Categories.Warehouse_JobImport,
						ResString.GetMultilingualString("b03beef9-c1c2-4b45-b704-a0e123180018", "Warehouse Job Notification Staff Roles"),
						ResString.GetMultilingualString("6f0d825d-b739-40b6-bf1f-c66ace395653", "The staff roles that will be sent email notifications for Warehouse Job changes made via the batch processor."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("98886b2e-16d8-42a1-9f70-524d44eb946f", "Send Notification To"), true, true),
						GetStaffRolesDefaultValue());
				});
			}
		}

		CodeDescriptionBoolDisallowNewCollection GetStaffRolesDefaultValue()
		{
			CodeDescriptionBoolDisallowNewCollection result = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetBoolTo(result, true, StaffAssignmentRoles.Codes.CartageCoordinator);

			return result;
		}

		#endregion

		#endregion

		#region Enforce Serial Uniqueness

		ICodeDescriptionPairListProvider SerialUniquenessTypeListProvider
		{
			get
			{
				if (serialUniquenessTypeListProvider == null)
				{
					serialUniquenessTypeListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair("CLI", ResString.GetMultilingualString("278463f2-99de-4583-b891-8ab946eb687d", "By Client"));
						list.AddPair("PRO", ResString.GetMultilingualString("2dba6fd4-7697-42d3-9b5e-747c9a03c0aa", "By Product"));
						list.DefaultCode = "PRO";
						return list;
					});
				}

				return serialUniquenessTypeListProvider;
			}
		}
		ICodeDescriptionPairListProvider serialUniquenessTypeListProvider;

		public CodeDescriptionPairList SerialUniquenessTypeList
		{
			get
			{
				if (serialUniquenessTypeList == null)
				{
					serialUniquenessTypeList = SerialUniquenessTypeListProvider.CodeDescriptionPairList;
				}

				return serialUniquenessTypeList;
			}
		}
		CodeDescriptionPairList serialUniquenessTypeList;

		public CodePairRegistryItem EnforceSerialUniqueness
		{
			get
			{
				return GetItem("EnforceUniqueSerialNumbers", delegate
				{
					return new CodePairRegistryItem("EnforceUniqueSerialNumbers",
						Categories.Warehouse_General,
						ResString.GetMultilingualString("a61b7dd6-4296-47cc-a0de-6788613673d3", "Enforce Unique Serial Numbers"),
						ResString.GetMultilingualString("5aee9256-a4f2-41e5-86db-77769f719d4f", "By default the system will ensure serial numbers are unique by product. This means the same product cannot have more than one unit of stock for the same serial number, but different products can share the same serial number. Using this registry, you can change this default to ensure serial numbers are unique by client, which means the same client cannot have more than one unit of stock for the same serial number, regardless of the product."),
						SerialUniquenessTypeListProvider,
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.PreserveTestValue, SerialUniquenessTypeList.DefaultCode);
				});
			}
		}

		public bool EnforceSerialUniquenessByProduct
		{
			get { return EnforceSerialUniqueness.Value == "PRO"; }
		}

		#endregion

		#region Invoice Detail Group By

		public InvoiceDetailGroupByRegistryItem InvoiceDetailGroupBy
		{
			get
			{
				return GetItem("InvoiceDetailGroupBy", delegate
				{
					var defaultValue = new InvoiceDetailGroupBy();

					return new InvoiceDetailGroupByRegistryItem("InvoiceDetailGroupBy",
						Categories.Warehouse_Invoicing,
						ResString.GetMultilingualString("7a3b33f6-f8db-4448-825b-fa4bbdfea2cc", "Invoice Details Group Bys"),
						ResString.GetMultilingualString("8eaedc0a-c21e-4d77-b563-a5081a6f6e22", "Specify the grouping for the Invoice Detail report. You can specify up to 3 nested groups. Leave as blank to specify the group is not used. Each group will have a sub total on the report."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						defaultValue);
				});
			}
		}

		#endregion

		#region Invoice Billing Period Day

		public IntRegistryItem InvoiceBillingPeriodDay
		{
			get
			{
				return GetItem("InvoiceBillingPeriodDay", delegate
				{
					return new IntRegistryItem("InvoiceBillingPeriodDay",
						Categories.Warehouse_Invoicing,
						ResString.GetMultilingualString("1128db70-cf86-4aa3-a266-cef30aa7f626", "Invoice Billing Period Day"),
						ResString.GetMultilingualString("438e5bbc-bf6a-4b8f-8ca9-0bf4b19f04d7", "Specifies the default day for billing periods. If weekly billing, then day of the week - if monthly billing, then day of month. This is used for Split Period Billing to determine whether Order level Storage should or should not be charged."),
						RegistryStorageFlags.System,
						1);
				});
			}
		}

		#endregion

		#region JobServices

		public CodeDescriptionPairListWithDefaultCodeRegistryItem JobServices
		{
			get
			{
				return GetItem(WarehouseJobServicesRegistry.RegistryItemKey, delegate
				{
					var defaultList = (CodeDescriptionPairList)Activator.CreateInstance(ObjectFactory.GetType<IFreightServiceTypes>());
					defaultList.DefaultCode = defaultList[0].Code; // we don't use the default, this is just to get rid of the error

					var item = new CodeDescriptionPairListWithDefaultCodeRegistryItem
					(
						WarehouseJobServicesRegistry.RegistryItemKey,
						Categories.Warehouse_General,
						ResString.GetMultilingualString("56b38609-4fa3-4288-8123-996b7805783c", "Job Services"),
						ResString.GetMultilingualString("95dbb612-0ab4-45fb-90ec-fa8d4bd7aa66", "Services that can be performed on a a Warehouse Job."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultList,
						false
					);

					return item;
				});
			}
		}

		#endregion

		#region WebServerURL

		public StringRegistryItem WarehouseWebServerURL
		{
			get
			{
				return GetItem("WarehouseWebServerURL", delegate
				{
					return new StringRegistryItem("WarehouseWebServerURL",
						Categories.Warehouse_General,
						ResString.GetMultilingualString("127f4823-72c8-4250-bfd6-8925a21e09a8", "Warehouse Web Server URL"),
						ResString.GetMultilingualString("7c1eeff6-0fee-4a97-a44b-379f2b97f3ff", "The URL used to connect to your warehouse web service."),
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Url),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue,
						defaultValue: GetRootWarehouseWebServiceUriDefaultValue());
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		static string GetRootWarehouseWebServiceUriDefaultValue()
		{
			var productRegistration = ObjectFactory.Get<IProductRegistration>();
			var registrationKey = productRegistration.Key;
			var enterpriseCode = registrationKey.EnterpriseCode;
			var databaseCode = registrationKey.ServerCode;

			if (string.IsNullOrWhiteSpace(enterpriseCode) || string.IsNullOrWhiteSpace(databaseCode))
			{
				return "ERROR: Enterprise Code or Database Code is empty!";
			}
			else
			{
				return SharedGlowRegistry.CalculateWarehouseWebServicesDefaultRootUri(enterpriseCode, databaseCode, productRegistration.IsWiseTechGlobalInternalSystem()).AbsoluteUri;
			}
		}

		#endregion

		#endregion

		#region Scanning

		#region ApplicationIdentifiers

		public ApplicationIdentifierRegistryItem ApplicationIdentifiers
		{
			get
			{
				return GetItem("ApplicationIdentifiers", delegate
				{
					return new ApplicationIdentifierRegistryItem(
						"ApplicationIdentifiers",
						Categories.Warehouse_Scanning,
						ResString.GetMultilingualString("4820163e-3e61-4a72-9e7c-56e789b1d941", "Application Identifiers"),
						ResString.GetMultilingualString("121d8cc3-03e9-4d3c-bc8c-ecae70ecf317", "Specify Application Identifiers embedded in GS1 barcodes."),
						RegistryStorageFlags.System,
						ApplicationIdentifierCollection.GetDefault());
				});
			}
		}

		#endregion

		#region DisplayPickStatus

		public BooleanRegistryItem DisplayPickStatus
		{
			get
			{
				return GetItem("DisplayPieChart", delegate
				{
					return new BooleanRegistryItem("DisplayPieChart",
						Categories.Warehouse_Scanning,
						ResString.GetMultilingualString("bccf81d6-bb23-45ed-b2fa-6c11d25b31e8", "Display Pie Chart"),
						ResString.GetMultilingualString("faf75e57-a73a-4aff-ab42-a6266804af24", "The system will display the percentage of locations that have been picked."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, true);
				});
			}
		}

		#endregion

		#region WarehouseRFJobFinalizeFailNotificationEmailGroup

		public GuidRegistryItem WarehouseRFJobFinalizationFailureNotificationGroup
		{
			get
			{
				return GetItem("WarehouseRFJobFinalizationFailureNotificationGroup", delegate
				{
					var result = new GuidRegistryItem(
						"WarehouseRFJobFinalizationFailureNotificationGroup",
						Categories.Warehouse_Scanning,
						ResString.GetMultilingualString("9cf5ace3-da97-4e53-b1eb-b9d6c8b77c44", "Warehouse RF Job Finalization Failure Notification Group"),
						ResString.GetMultilingualString("3ec3f40e-4703-44d1-ada2-6946a1590956", "This group will be sent an email notification each time a Finalization failure occurs via RF."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.NotCached | RegistryOptions.IsValueOptional,
						RegistryFactory.Instance.GetGroupPK("ALL"));

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region WarnWhenDuplicatePalletIdScanned

		public BooleanRegistryItem WarnWhenDuplicatePalletIdScanned
		{
			get
			{
				return GetItem("WarnWhenDuplicatePalletIdScanned", delegate
				{
					return new BooleanRegistryItem("WarnWhenDuplicatePalletIdScanned",
						Categories.Warehouse_Scanning,
						ResString.GetMultilingualString("9af5ace3-da97-4e53-b1eb-b9d6c8b77c44", "Warn When Duplicate Pallet ID Scanned in RF"),
						ResString.GetMultilingualString("3ec3f13e-4703-44d1-ada2-6946a1590956", "A warning pop-up will be displayed when duplicate Pallet ID is scanned in RF during Unload."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, true);
				});
			}
		}

		#endregion

		#endregion

		#region TaskManagement

		public BooleanRegistryItem ExposeWarehouseTaskManagement
		{
			get
			{
				const string ExposeWarehouseTaskManagement = "ExposeWarehouseTaskManagement";
				return GetItem(ExposeWarehouseTaskManagement, () =>
				{
					return new BooleanRegistryItem(ExposeWarehouseTaskManagement,
						Categories.Warehouse_TaskManagement,
						(NoResString)"Expose Warehouse Task Management",
						(NoResString)"Expose Warehouse Task Management functionality - DO NOT USE THIS ON PERSISTED TESTING SYSTEMS LIKE UAT ALPHA",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning
		{
			get
			{
				const string AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning = "AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning";
				return GetItem(AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning, () =>
				{
					return new BooleanRegistryItem(AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning,
						Categories.Warehouse_TaskManagement,
						ResString.GetMultilingualString("a58e84de-9989-4a6a-a51b-734ce8c4cb58", "Automatically Set Receive Task Planning Status To Ready For Planning"),
						ResString.GetMultilingualString("7bf16ade-af0f-4d69-8c0a-226997273932", "Automatically set a job to Ready For Planning when the receive has started receiving"),
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem AutomaticallySetPickTaskPlanningStatusToReadyForPlanning
		{
			get
			{
				const string AutomaticallySetPickTaskPlanningStatusToReadyForPlanning = "AutomaticallySetPickTaskPlanningStatusToReadyForPlanning";
				return GetItem(AutomaticallySetPickTaskPlanningStatusToReadyForPlanning, () =>
				{
					return new BooleanRegistryItem(AutomaticallySetPickTaskPlanningStatusToReadyForPlanning,
						Categories.Warehouse_TaskManagement,
						ResString.GetMultilingualString("d042585d-85a5-4a3f-80e8-4a3473613b48", "Automatically Set Pick Task Planning Status To Ready For Planning"),
						ResString.GetMultilingualString("05ccbc0f-273d-4a40-9bd5-ceadde9cdf3c", @"Automatically set a pick job to Ready For Planning when the following conditions are met:
1. Fulfillment Rules Met
2. Not Awaiting Replenishment
3. Cartonization / Pick By Label Flags"),
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem AutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning
		{
			get
			{
				const string AutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning = "AutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning";
				return GetItem(AutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning, () =>
				{
					return new BooleanRegistryItem(AutomaticallySetReplenishmentTransferTaskPlanningStatusToReadyForPlanning,
						Categories.Warehouse_TaskManagement,
						ResString.GetMultilingualString("dddcdf88-add6-4f03-b85a-84b90799072d", "Automatically Set Replenishment Transfer Task Planning Status To Ready For Planning"),
						ResString.GetMultilingualString("a24499d2-3395-4858-9e3b-1bf9f6436bb9", "Automatically set Replenishment Transfer to Ready For Planning on saving."),
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public BooleanRegistryItem AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning
		{
			get
			{
				const string AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning = "AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning";
				return GetItem(AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning, () =>
				{
					return new BooleanRegistryItem(AutomaticallySetNonReplenishmentTransferTaskPlanningStatusToReadyForPlanning,
						Categories.Warehouse_TaskManagement,
						ResString.GetMultilingualString("1eb773a4-71fb-4ef3-9854-1d09c0327601", "Automatically Set Transfer Task Planning Status To Ready For Planning"),
						ResString.GetMultilingualString("b022d020-d692-4626-ad12-6670b55f37c2", "Automatically set Transfer (excluding Replenishment) to Ready For Planning on saving."),
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		#endregion

		#region Locations

		#region MaxNumberOfLocationsPerRow

		public IntRegistryItem MaxNumberOfLocationsPerRow
		{
			get
			{
				return GetItem("MaxNumberOfLocationsPerRow", delegate
				{
					return new IntRegistryItem("MaxNumberOfLocationsPerRow",
						Categories.Warehouse_Locations,
						ResString.GetMultilingualString("2DA584DF-C6F6-4102-9901-49245882E431", "Max Number of Locations per Row"),
						ResString.GetMultilingualString("DC4C5150-25C3-4AC9-B02A-057A8FEEC509", "This item allows Warehouses to increase the number of Locations per Row."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						DEFAULT_LOCATION_COUNT,
						MIN_LOCATION_COUNT,
						MAX_LOCATION_COUNT);
				});
			}
		}

		const int DEFAULT_LOCATION_COUNT = 10000; // 10,000
		const int MIN_LOCATION_COUNT = 1;
		const int MAX_LOCATION_COUNT = 50000; // 50,000

		#endregion

		#region EnableRowNamePrefixValidationForFixedWidthWarehouses

		public BooleanRegistryItem EnableRowNamePrefixValidationForFixedWidthWarehouses
		{
			get
			{
				return GetItem("EnableRowNamePrefixValidationForFixedWidthWarehouses", delegate
				{
					return new BooleanRegistryItem("EnableRowNamePrefixValidationForFixedWidthWarehouses",
						Categories.Warehouse_Locations,
						ResString.GetMultilingualString("874b6290-03f1-4a0e-b718-400dbef9accc", "Enable Row Name Prefix Validation for Fixed Width Warehouses"),
						ResString.GetMultilingualString("0a7193ef-887e-4bb7-9fd4-5b203f9b1ca8", "This validation will ensure that row names used in a Fixed Width warehouse will not be a prefix (starts with) for another row name in that warehouse. This is used to prevent potential location barcode conflicts."),
						RegistryStorageFlags.System,
						defaultValue: true);
				});
			}
		}

		#endregion

		#region EnableSchemaRedesignChanges

		public BooleanRegistryItem EnableSchemaRedesignChanges
		{
			get
			{
				return GetItem("EnableSchemaRedesignChanges", delegate
				{
					return new BooleanRegistryItem("EnableSchemaRedesignChanges",
						Categories.Warehouse_General,
						(NoResString)"Enable Schema Redesign Changes",
						(NoResString)"Enable Schema Redesign Changes - DO NOT USE THIS ON PERSISTED TESTING SYSTEMS LIKE UAT ALPHA",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region AllowFinalisationDateUpTo30DaysInTheFuture

		public BooleanRegistryItem AllowWarehouseOrderFinalisationDateUpTo30DaysInTheFuture
		{
			get
			{
				return GetItem("AllowWarehouseOrderFinalisationDateUpTo30DaysInTheFuture", () =>
				{
					return new BooleanRegistryItem("AllowWarehouseOrderFinalisationDateUpTo30DaysInTheFuture",
						Categories.Warehouse_General,
						ResString.GetMultilingualString("6637707c-7969-49f2-842d-a60a68084d5c", "Allow Warehouse Order Finalization Dates Up To 30 Days In The Future"),
						ResString.GetMultilingualString("bcb65979-4541-4668-b1d6-560e48d8749d", "Allow finalization dates up to 30 days in the future on Warehouse Orders via the 'Use Required Date For Outwards Finalized Date' Warehouse option."),
						RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#endregion

		#region PerformanceReporting

		public PerformanceReportingRegistryItem PerformanceReporting
		{
			get
			{
				return GetItem("WarehousePerformanceReporting", delegate
				{
					return new PerformanceReportingRegistryItem(
						"WarehousePerformanceReporting",
						Categories.Warehouse_PerformanceReporting,
						ResString.GetMultilingualString("C3945A8C-2F60-42BE-9C1B-A3CDC649D4F8", "Performance Metrics"),
						ResString.GetMultilingualString("356F068A-BC0B-4D1B-8BB8-97AE94F00E95", "The Performance Reporting Category determines how the performance data is categorized per metric."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						PerformanceReportingMetricCollection.GetDefault());
				});
			}
		}

		#endregion

		#region PickFaceReplenishmentLastRun

		public DateTimeRegistryItem PickFaceReplenishmentLastRunUTC
		{
			get
			{
				return GetItem("PickFaceReplenishmentLastRunUTC", delegate
				{
					return new DateTimeRegistryItem(
						"PickFaceReplenishmentLastRunUTC",
						Categories.Warehouse_Picking,
						(NoResString)"Pick Face Replenishment Last Run",
						(NoResString)"Time (in UTC) of the last time the pick face replenishment service task was run.",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						new DateTime(1900, 1, 1));
				});
			}
		}

		#endregion

		#region ContainerYard

		public BooleanRegistryItem EnableContainerYard
		{
			get => GetItem("EnableContainerYard", () =>
				new BooleanRegistryItem(
					"EnableContainerYard",
					Categories.Warehouse_ContainerYard,
					(NoResString)"Enables Container Yard Module Section",
					(NoResString)"If yes, container yard module section is visible.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false));
		}

		public BooleanRegistryItem EnableContainerYardUnloadRules
		{
			get => GetItem("EnableContainerYardUnloadRules", () =>
				new BooleanRegistryItem(
					"EnableContainerYardUnloadRules",
					Categories.Warehouse_ContainerYard,
					(NoResString)"Enables Container Yard Unload Rules in PRE",
					(NoResString)"If yes, Unload tile is visible in PRE.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false));
		}

		public BooleanRegistryItem EnableContainerYardDeliveryAndPickupInstructions
		{
			get => GetItem(DeliveryAndPickupInstructionsRegistry.RegistryItemKey, () =>
				new BooleanRegistryItem(
					DeliveryAndPickupInstructionsRegistry.RegistryItemKey,
					Categories.Warehouse_ContainerYard,
					(NoResString)"Enable Container Yard Delivery and Pick up Instructions",
					(NoResString)"If yes, Container Yard Delivery and Pick up Instructions is visiable.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false));
		}

		public BooleanRegistryItem EnableMNRCombinedEstimates
		{
			get => GetItem("EnableMNRCombinedEstimates", () =>
				new BooleanRegistryItem(
					"EnableMNRCombinedEstimates",
					Categories.Warehouse_ContainerYard,
					(NoResString)"Enable combined estimates",
					(NoResString)"If yes, the structural and machinery M&R estimates for a unit will be combined into a single estimate.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false));
		}

		#region DefaultDimensionUnitForContainerYard

		public CodePairRegistryItem DefaultDimensionUnitForContainerYard
		{
			get
			{
				const string registryItemName = "DefaultDimensionUnitForContainerYard";

				return GetItem(registryItemName, delegate
				{
					return new CodePairRegistryItem
					(
						registryItemName,
						Categories.Warehouse_ContainerYard,
						ResString.GetMultilingualString("9183e193-4fb1-49c9-a5d0-c36252e324db", "Default Dimension Unit"),
						ResString.GetMultilingualString("b363ae92-9831-4d58-86a5-5b83055d0b7c", "The default unit of dimension to use."),
						DefaultDimensionUnitListForContainerYardProvider,
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DefaultDimensionUnitListForContainerYard.DefaultCode
					);
				});
			}
		}
		ICodeDescriptionPairListProvider DefaultDimensionUnitListForContainerYardProvider
		{
			get
			{
				if (defaultDimensionUnitListForContainerYardProvider == null)
				{
					defaultDimensionUnitListForContainerYardProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair("CM", ResString.GetMultilingualString("697b4667-4792-40d8-8640-b207c20ef2d8", "Centimeters"));
						list.DefaultCode = "CM";
						return list;
					});
				}

				return defaultDimensionUnitListForContainerYardProvider;
			}
		}
		ICodeDescriptionPairListProvider defaultDimensionUnitListForContainerYardProvider;

		public CodeDescriptionPairList DefaultDimensionUnitListForContainerYard
		{
			get
			{
				if (defaultDimensionUnitForContainerYardField == null)
				{
					defaultDimensionUnitForContainerYardField = DefaultDimensionUnitListForContainerYardProvider.CodeDescriptionPairList;
				}

				return defaultDimensionUnitForContainerYardField;
			}
		}
		CodeDescriptionPairList defaultDimensionUnitForContainerYardField;

		#endregion

		#endregion

		#region EnableImprovedStorageOfCustomsData

		public BooleanRegistryItem EnableImprovedStorageOfCustomsData
		{
			get
			{
				const string enableImprovedStorageOfCustomsData = nameof(enableImprovedStorageOfCustomsData);
				return GetItem(enableImprovedStorageOfCustomsData, () =>
				{
					return new BooleanRegistryItem(enableImprovedStorageOfCustomsData,
						Categories.Warehouse_General,
						(NoResString)"Enable Improved Storage Of Customs Data",
						(NoResString)"Enable Improved Storage Of Customs Data - DO NOT USE THIS ON PERSISTED TESTING SYSTEMS LIKE UAT ALPHA",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region EnableDynamicWorkOrder

		public BooleanRegistryItem EnableDynamicWorkOrder
		{
			get
			{
				const string enableDynamicWorkOrder = nameof(enableDynamicWorkOrder);
				return GetItem(enableDynamicWorkOrder, () =>
				{
					return new BooleanRegistryItem(enableDynamicWorkOrder,
						Categories.Warehouse_General,
						(NoResString)"Enable Dynamic Work Order Functionality",
						(NoResString)"Enable Dynamic Work Order Functionality - DO NOT USE THIS ON PERSISTED TESTING SYSTEMS LIKE UAT ALPHA",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		IRegistryItem IWarehouseDataRegistry.EnableDynamicWorkOrder => EnableDynamicWorkOrder;

		#endregion

		#region NumberCustomisation

		public BillCustomisationByServiceLevelRegistryItem WarehouseNumberCustomisation_Order
		{
			get
			{
				return GetItem<BillCustomisationByServiceLevelRegistryItem>("WarehouseNumberCustomisation_Order", delegate
				{
					var dataType = PrepareDocketNumberCustomisationDataType(
						ResString.GetMultilingualString("0125e36d-b3d1-4127-8c2f-9c016c7e2bbd", "Order Docket ID Format"),
						ResString.GetMultilingualString("1a2c0afe-b52d-4479-96da-ead30487f49f", "Order"),
						NumberCustomisationElementCategories.WarehouseOrder,
						ObjectFactory.GetType<IWhsOrder>());

					return new BillCustomisationByServiceLevelRegistryItem(
						"WarehouseNumberCustomisation_Order",
						Categories.Warehouse_Orders,
						ResString.GetMultilingualString("d63e4ea7-cd70-461f-bcb1-8afe0163cb5e", "Order Number Customization"),
						ResString.GetMultilingualString("3d8c24c5-bf0b-4b48-a02c-1dd9517018ce", "Override this value to customize how the Warehouse Order ID are formatted."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						dataType);
				});
			}
		}

		public BillCustomisationByServiceLevelRegistryItem WarehouseNumberCustomisation_Receive
		{
			get
			{
				return GetItem<BillCustomisationByServiceLevelRegistryItem>("WarehouseNumberCustomisation_Receive", delegate
				{
					var dataType = PrepareDocketNumberCustomisationDataType(
						ResString.GetMultilingualString("e549faac-6d4f-4502-b942-292bc0f48bad", "Receive Docket ID Format"),
						ResString.GetMultilingualString("a678c818-3bca-4219-b09e-2a622750d3ff", "Receive"),
						NumberCustomisationElementCategories.WarehouseReceive,
						ObjectFactory.GetType<IWhsReceive>());

					return new BillCustomisationByServiceLevelRegistryItem(
						"WarehouseNumberCustomisation_Receive",
						Categories.Warehouse_Receive,
						ResString.GetMultilingualString("75fd9f9f-9af5-4c51-91ab-da560d5c13ac", "Receive Number Customization"),
						ResString.GetMultilingualString("11dc652d-f3ce-4774-8542-c7766feb245d", "Override this value to customize how the Warehouse Receive ID are formatted."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						dataType);
				});
			}
		}

		BillCustomisationByServiceLevelRegistryDataType PrepareDocketNumberCustomisationDataType(
			ResourceString generatedNumberName,
			ResourceString sequenceNumberName,
			NumberCustomisationElementCategories category,
			Type macroType)
		{
			var dataType = new BillCustomisationByServiceLevelRegistryDataType();
			dataType.FountainPrefix = "W";
			dataType.GeneratedNumberName = generatedNumberName;
			dataType.SequenceNumberName = sequenceNumberName;
			dataType.MaxLength = WhsDocketSchema.WD_DocketID.MaxLength;
			dataType.Categories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.WarehouseJob | category;
			dataType.AllowNonAlphanumericCharacters = true;
			dataType.EnableMacroInsertion = true;
			dataType.MacroType = macroType;

			return dataType;
		}

		#endregion

		#region EnableSendingCIN750Message

		public BooleanRegistryItem EnableSendingCIN750Message
		{
			get
			{
				const string enableSendingCIN750Message = nameof(EnableSendingCIN750Message);
				return GetItem(enableSendingCIN750Message, () =>
				{
					return new BooleanRegistryItem(enableSendingCIN750Message,
						Categories.Warehouse_TransitWarehouse,
						ResString.GetMultilingualString("31808f1a-7cc8-4c2f-8155-7852121249e6", "Enable Sending CIN 750 Message"),
						ResString.GetMultilingualString("a8b15638-c55e-4407-9cbf-f7c044a3d24b", "If yes, Transit Modules would display Message Menu and show CIN 750 Notification Forms in the Document Menu."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region

		public BooleanRegistryItem EnableHeldGoodsForOrders
		{
			get
			{
				const string enableHeldGoodsForOrders = nameof(EnableHeldGoodsForOrders);
				return GetItem(enableHeldGoodsForOrders, delegate
				{
					return new BooleanRegistryItem(
						enableHeldGoodsForOrders,
						Categories.Warehouse_Orders,
						(NoResString)"Enable Held Goods entry on Order Line",
						(NoResString)"When this registry setting is enabled, the entry field for Held Goods will be enabled on Order Lines.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region ConsignmentPackageLabelsMandatoryOnLoad

		public BooleanRegistryItem ConsignmentPackageLabelsMandatoryOnLoad
		{
			get
			{
				const string consignmentPackageLabelsMandatoryOnLoad = nameof(ConsignmentPackageLabelsMandatoryOnLoad);
				return GetItem(consignmentPackageLabelsMandatoryOnLoad, () =>
				{
					return new BooleanRegistryItem(consignmentPackageLabelsMandatoryOnLoad,
						Categories.Warehouse_TransitWarehouse_Load,
						ResString.GetMultilingualString("3612ff53-227d-49e5-be7d-e5beaf642f2d", "Consignment Package Labels Mandatory on Load"),
						ResString.GetMultilingualString("7155e436-dd09-4fe4-ae15-f1aad6419147", "Enable this setting to make it mandatory for Packlines packed onto Handling Units to be labeled on load."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region Gate Management

		public BooleanRegistryItem EnableGateManagement
		{
			get
			{
				const string enableGateManagement = nameof(EnableGateManagement);
				return GetItem(enableGateManagement, () =>
				{
					return new BooleanRegistryItem(enableGateManagement,
						Categories.Warehouse_GateManagement,
						(NoResString)"Enable Gate Management",
						(NoResString)"Enables the 'Gate Management' section under the Warehouse module in CW1",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem AllowGateBookingCreationMenu
		{
			get
			{
				const string allowGateBookingCreationMenu = nameof(AllowGateBookingCreationMenu);
				return GetItem(allowGateBookingCreationMenu, () =>
				{
					return new BooleanRegistryItem(allowGateBookingCreationMenu,
						Categories.Warehouse_GateManagement,
						(NoResString)"Allow creation of Gate Bookings in the portal",
						(NoResString)"If yes, 'Gate Booking' and 'New Gate Booking' menu items are visible.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem DisallowTimeslotDateLaterthanCurrentDateGateIn
		{
			get => GetItem("DisallowTimeslotDateLaterthanCurrentDateGateIn", delegate
			{
				return new BooleanRegistryItem(
					"DisallowTimeslotDateLaterthanCurrentDateGateIn",
					Categories.Warehouse_GateManagement,
					ResString.GetMultilingualString("545b6275-521f-cdb3-4b10-8109450ef8c5", "Disallow Time-slot Date Later than Current Date in Gate-In"),
					ResString.GetMultilingualString("5d4889eb-a766-0e98-4156-2dea2f355130", "Disallows Time-slot Date Later than Current Date in Gate-In"),
					RegistryStorageFlags.System,
					true);
			});
		}

		public BooleanRegistryItem DisallowTWHGateInWithoutBooking
		{
			get
			{
				const string disallowTWHGateInWithoutBooking = nameof(DisallowTWHGateInWithoutBooking);
				return GetItem(disallowTWHGateInWithoutBooking, () =>
				{
					return new BooleanRegistryItem(disallowTWHGateInWithoutBooking,
						Categories.Warehouse_GateManagement,
						(NoResString)"Disallow Gate In Without Booking for Transit Warehouse Facilities",
						(NoResString)"If yes, 'Gate In Without Booking' will not be shown for Transit Warehouse branches and a Transit Warehouse cannot be chosen for a gate in without booking.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public BooleanRegistryItem EnableGateInValidation
		{
			get => GetItem("EnableGateInValidation", delegate
			{
				return new BooleanRegistryItem(
					"EnableGateInValidation",
					Categories.Warehouse_GateManagement,
					ResString.GetMultilingualString("8512e0f4-396a-4a79-974f-6a16c5301302", "Enables gate-in validation at the facility"),
					ResString.GetMultilingualString("8512e0f4-396a-4a79-974f-6a16c5301302", "Enables gate-in validation at the facility"),
					RegistryStorageFlags.System,
					true);
			});
		}

		public BooleanRegistryItem EnableUniversalIntegrationBetweenGateAndContainerYard
		{
			get
			{
				return GetItem("EnableUniversalIntegrationBetweenGateAndContainerYard", delegate
				{
					return new BooleanRegistryItem(
						"EnableUniversalIntegrationBetweenGateAndContainerYard",
						Categories.Warehouse_GateManagement,
						(NoResString)"Enable universal integration between gate and container yard",
						(NoResString)"Enables the logic that allows gate and container yard to communicate through UXML using universal job links",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableUniversalIntegrationBetweenGateAndTransitWarehouse
		{
			get
			{
				return GetItem("EnableUniversalIntegrationBetweenGateAndTransitWarehouse", delegate
				{
					return new BooleanRegistryItem(
						"EnableUniversalIntegrationBetweenGateAndTransitWarehouse",
						Categories.Warehouse_GateManagement,
						(NoResString)"Enable universal integration between gate and transit warehouse",
						(NoResString)"Enables the logic that allows gate and transit warehouse to communicate through UXML using universal job links",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		// Next Inbound OAuth Authority URLs
		public StringArrayRegistryItem GateManagementInboundOAuthAuthorityUrls
		{
			get
			{
				return GetItem("GateManagementInboundOAuthAuthorityUrls", delegate
				{
					return new StringArrayRegistryItem(
						"GateManagementInboundOAuthAuthorityUrls",
						WarehouseDataRegistry.Categories.Warehouse_GateManagement_Authentication,
						(NoResString)"OAuth Authority URLs",
						(NoResString)"Authority URLs used to validate access tokens to Gate Management web service.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						Array.Empty<string>());
				});
			}
		}

		// Next Inbound OAuth Client IDs
		public StringArrayRegistryItem GateManagementInboundOAuthClientIDs
		{
			get
			{
				return GetItem("GateManagementInboundOAuthClientIDs", delegate
				{
					return new StringArrayRegistryItem(
						"GateManagementInboundOAuthClientIDs",
						WarehouseDataRegistry.Categories.Warehouse_GateManagement_Authentication,
						(NoResString)"OAuth Client IDs",
						(NoResString)"Valid access tokens granted to client ids supplied here will be authorized to use Gate Management web service.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						Array.Empty<string>());
				});
			}
		}

		#endregion

		#region EnableWhsPeriodicInvoiceXUT

		public BooleanRegistryItem EnableWhsPeriodicInvoiceXUT
		{
			get
			{
				const string enableWhsPeriodicInvoiceXUT = nameof(EnableWhsPeriodicInvoiceXUT);
				return GetItem(enableWhsPeriodicInvoiceXUT, () =>
				{
					return new BooleanRegistryItem(enableWhsPeriodicInvoiceXUT,
						Categories.Warehouse_Invoicing,
						(NoResString)"Enable Warehouse Periodic Invoice XUT", // Does not need translation because it is only for support
						(NoResString)"If yes, Periodic Billings can be imported through Universal Transaction (XUT).",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}
		#endregion

		#region EnableMHEServiceTestingMode

		public BooleanRegistryItem EnableMHEServiceTestingMode
		{
			get
			{
				const string enableMHEServiceTestingMode = nameof(EnableMHEServiceTestingMode);
				return GetItem(enableMHEServiceTestingMode, delegate
				{
					return new BooleanRegistryItem(
						enableMHEServiceTestingMode,
						Categories.Warehouse_MHE,
						(NoResString)"Enable MHE Service Testing Mode",
						(NoResString)"Uses staging client ID for communication with MHE service.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region SaveMHEFilesToEDocs

		public BooleanRegistryItem SaveMHEFilesToEDocs
		{
			get
			{
				const string saveMHEFilesToEDocs = nameof(SaveMHEFilesToEDocs);
				return GetItem(saveMHEFilesToEDocs, delegate
				{
					return new BooleanRegistryItem(
						saveMHEFilesToEDocs,
						Categories.Warehouse_MHE,
						(NoResString)"Save MHE Files To eDocs",
						(NoResString)"If set to True, MHE files will be saved to eDocs. If set to False, a URL linking to the MHE files will be saved in the database.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region MHEServiceEndpoint

		public StringRegistryItem MHEServiceEndpoint
		{
			get
			{
				const string _MHEServiceEndpoint = nameof(MHEServiceEndpoint);
				return GetItem(_MHEServiceEndpoint, delegate
				{
					return new StringRegistryItem(_MHEServiceEndpoint,
						Categories.Warehouse_MHE,
						ResString.GetMultilingualString("aa0d1f9a-48ae-48b7-a8a9-8fd3408f084e", "MHE Service Endpoint"),
						ResString.GetMultilingualString("d02e36d2-1b79-43d8-9fcb-b732c2c5dd67", "The URL for dimensioner middleware service."),
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Url),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						"https://mheservice.wisegrid.net/");
				});
			}
		}

		#endregion
	}
}
