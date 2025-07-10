using System;
using CargoWise.Data;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	public sealed class HVLVDataRegistry : RegistryItemSet
	{
		HVLVDataRegistry() { }

		#region Instance / AddRegistryItems

		public static HVLVDataRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new HVLVDataRegistry();
				}
				return fInstance;
			}
		}

		[ThreadStatic]
		static HVLVDataRegistry fInstance;

		#endregion

		public override bool IsForProductivityWise => false;

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Freight_HVLV { get { return CombineCategories(Freight, ResString.GetMultilingualString("0f149565-553f-47ec-9ee1-e6b5b03276cf", "HVLV")); } }
			public static MultilingualString Freight_HVLV_Customs { get { return CombineCategories(Freight_HVLV, ResString.GetMultilingualString("4fcfc2f6-b907-4d7b-a0a4-b419f6aea23f", "Customs")); } }
			public static MultilingualString Freight_HVLV_LastMileCarrier { get { return CombineCategories(Freight_HVLV, ResString.GetMultilingualString("a5dfd0ec-ecce-4779-a372-d50ee793a850", "Last Mile Carrier")); } }
			public static MultilingualString Freight_HVLV_Customs_AU { get { return CombineCategories(Freight_HVLV_Customs, ResString.GetMultilingualString("0a20a734-5df6-4465-b0ac-321d95979a19", "Australia")); } }
			public static MultilingualString Freight_HVLV_Customs_NZ { get { return CombineCategories(Freight_HVLV_Customs, ResString.GetMultilingualString("31f18791-6092-4db0-a6a6-354b9d2996c0", "New Zealand")); } }
			public static MultilingualString Freight_HVLV_Customs_US { get { return CombineCategories(Freight_HVLV_Customs, ResString.GetMultilingualString("842142b1-a480-4ac7-b808-a3b2d1f7178b", "United States of America")); } }
			public static MultilingualString Freight_HVLV_Customs_CA { get { return CombineCategories(Freight_HVLV_Customs, ResString.GetMultilingualString("d8e156c6-8e63-4107-9063-49d1384d45bb", "Canada")); } }
			public static MultilingualString Freight_HVLV_Customs_SG { get { return CombineCategories(Freight_HVLV_Customs, ResString.GetMultilingualString("d512e8c2-0081-4b22-b3a2-7c0b27939815", "Singapore")); } }
			public static MultilingualString Freight_HVLV_Customs_AU_AirCargoReport { get { return CombineCategories(Freight_HVLV_Customs_AU, ResString.GetMultilingualString("1466afdb-94a6-4ffc-ac7a-c942b981cdc5", "Air Cargo Report")); } }
			public static MultilingualString Freight_HVLV_Customs_AU_ExportSubManifest { get { return CombineCategories(Freight_HVLV_Customs_AU, ResString.GetMultilingualString("5c7dd077-53df-4112-a2fd-bed00ee963bd", "Export Sub Manifest (ESM)")); } }
			public static MultilingualString Freight_HVLV_Customs_AU_SeaCargoReport { get { return CombineCategories(Freight_HVLV_Customs_AU, ResString.GetMultilingualString("b49da794-d034-42db-917d-2f5b46dcff17", "Sea Cargo Report")); } }
			public static MultilingualString Freight_HVLV_Customs_AU_StandAloneDeclaration { get { return CombineCategories(Freight_HVLV_Customs_AU, ResString.GetMultilingualString("16a7ad4a-d5b4-40fd-ad47-458d14db8a9b", "Stand Alone Declaration")); } }
			public static MultilingualString Freight_HVLV_Customs_CA_eManifest { get { return CombineCategories(Freight_HVLV_Customs_CA, ResString.GetMultilingualString("53a06a5e-95f9-421c-9f2d-2a01591d4885", "eManifest")); } }
			public static MultilingualString Freight_HVLV_Customs_NZ_StandAloneDeclaration { get { return CombineCategories(Freight_HVLV_Customs_NZ, ResString.GetMultilingualString("4a67313c-1db3-40a5-a021-008f6508f437", "Stand Alone Declaration")); } }
			public static MultilingualString Freight_HVLV_Customs_NZ_ICRCRE { get { return CombineCategories(Freight_HVLV_Customs_NZ, ResString.GetMultilingualString("f4a493ba-6f76-4b3f-84ef-cb283c002048", "ICR & CRE")); } }
			public static MultilingualString Freight_HVLV_Customs_NZ_AirCargoReport { get { return CombineCategories(Freight_HVLV_Customs_NZ_ICRCRE, ResString.GetMultilingualString("0ae52ebc-4304-45b9-832f-a8ca80ad7ce0", "Air Cargo ICR & CRE")); } }
			public static MultilingualString Freight_HVLV_Customs_NZ_SeaCargoReport { get { return CombineCategories(Freight_HVLV_Customs_NZ_ICRCRE, ResString.GetMultilingualString("48796b38-9f2f-48fa-b0a6-d7a91d2efd4d", "Sea Cargo ICR & CRE")); } }
			public static MultilingualString Freight_HVLV_Customs_US_ACAS { get { return CombineCategories(Freight_HVLV_Customs_US, ResString.GetMultilingualString("86788ab2-37dd-4b36-ba14-71cf13059865", "ACAS")); } }
			public static MultilingualString Freight_HVLV_Customs_US_AirAMS { get { return CombineCategories(Freight_HVLV_Customs_US, ResString.GetMultilingualString("bf77236c-2098-4bf9-ba59-a77e15ed2b3c", "Air AMS")); } }
			public static MultilingualString Freight_HVLV_Customs_US_eManifest { get { return CombineCategories(Freight_HVLV_Customs_US, ResString.GetMultilingualString("58c077c7-bd17-41dd-8978-ae7bd2060d5c", "e-Manifest")); } }
			public static MultilingualString Freight_HVLV_Customs_US_ImporterSecurityFilling { get { return CombineCategories(Freight_HVLV_Customs_US, ResString.GetMultilingualString("d4f3a339-da80-4df5-83b6-85f161bcd03d", "Importer Security Filling")); } }
			public static MultilingualString Freight_HVLV_Customs_US_SeaAMS { get { return CombineCategories(Freight_HVLV_Customs_US, ResString.GetMultilingualString("e51d3425-c559-44c5-923e-a7fd1926a9cb", "Sea AMS")); } }
			public static MultilingualString Freight_HVLV_Customs_US_LowValueEntries { get { return CombineCategories(Freight_HVLV_Customs_US, ResString.GetMultilingualString("f2cad6ab-d57d-4b15-ad78-9cc19c13442d", "Low Value Entries")); } }
			public static MultilingualString Freight_HVLV_Customs_US_StandAloneDeclaration { get { return CombineCategories(Freight_HVLV_Customs_US, ResString.GetMultilingualString("e2eef9a6-072e-4e3b-9a01-be16c7a94a4e", "Stand Alone Declaration")); } }
			public static MultilingualString Freight_HVLV_Customs_SG_SGAccessImportManifest { get { return CombineCategories(Freight_HVLV_Customs_SG, ResString.GetMultilingualString("9a268b87-98e8-453f-981f-3fac29cb456a", "SG Access Import Manifest")); } }
			public static MultilingualString Freight_HVLV_Customs_EU => CombineCategories(Freight_HVLV_Customs, ResString.GetMultilingualString("24e032d5-b259-4fd4-ba08-c890607ca401", "EU"));
			public static MultilingualString Freight_HVLV_Customs_EU_ICS2 => CombineCategories(Freight_HVLV_Customs_EU, ResString.GetMultilingualString("639001f9-950b-405e-a309-8d66a3cbf924", "ICS2"));
			public static MultilingualString Freight_HVLV_DeniedPartyScreening { get { return CombineCategories(Freight_HVLV, ResString.GetMultilingualString("dfa8cbdd-bba9-4afb-9094-d7c018a64716", "Denied Party Screening")); } }
		}

		#region IsHVLVAutoRatingEnabled

		public BooleanRegistryItem IsHVLVAutoRatingEnabled
		{
			get
			{
				return GetItem("IsHVLVAutoRatingEnabled", delegate
				{
					return new BooleanRegistryItem("IsHVLVAutoRatingEnabled", Categories.Freight_HVLV,
						ResString.GetMultilingualString("9f42182b-f065-4bc8-955d-64b277c84c68", "Enable HVLV Auto Rating"),
						ResString.GetMultilingualString("17369ff4-5be2-4d85-a258-1520d88797b9",
@"This is to enable HVLV Auto Rating.

If 'Yes' is selected, Items are rated considering Consignment/Items parameters. If 'No' is selected, Shipments are rated considering Shipment parameters."),
						RegistryStorageFlags.System, RegistryOptions.Default, true);
				});
			}
		}

		#endregion

		#region HVLV OriginLoadList

		public BooleanRegistryItem IsHVLVLoadListMasterHouseDefault
		{
			get
			{
				return GetItem("IsHVLVLoadListMasterHouseDefault", delegate
				{
					return new BooleanRegistryItem("IsHVLVLoadListMasterHouseDefault", Categories.Freight_HVLV,
						ResString.GetMultilingualString("fc310444-0fa3-481e-ba89-b1e01213ff99", "HVLV Load List Master House Default"),
						ResString.GetMultilingualString("96e69620-0aae-4326-bc64-d2639e7cc6dc", "Use to default HVLV Load List as a Master House."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.System, RegistryOptions.Default, false);
				});
			}
		}

		public BooleanRegistryItem HVLVOriginLoadListTestingMode
		{
			get
			{
				return GetItem("HVLVOriginLoadListTestingMode", delegate
				{
					return new BooleanRegistryItem("HVLVOriginLoadListTestingMode", Categories.Freight_HVLV,
						(NoResString)"Enable HVLV Origin Load List Testing Mode",
						(NoResString)"Turn this setting on to enable testing mode on HVLV Origin Load List module.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem ProcessLoadListUsingUniversalXML
		{
			get
			{
				return GetItem("ProcessLoadListUsingUniversalXML", delegate
				{
					return new BooleanRegistryItem(
						"ProcessLoadListUsingUniversalXML",
						Categories.Freight_HVLV,
						(NoResString)"Process Load List Using Universal XML",
						(NoResString)"Set to 'Yes' to process load list with universal XML. This is to replace current HVLV Origin Load List Helper with better readability and maintainability.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region HVLVClearance

		public static bool HasHVLVClearance
		{
			get { return Instance.HasHVLVClearanceItem.Value; }
			set { Instance.HasHVLVClearanceItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal BooleanRegistryItem HasHVLVClearanceItem
		{
			get
			{
				return GetItem("HasHVLVClearance", delegate
				{
					return new BooleanRegistryItem("HasHVLVClearance", Categories.Freight_HVLV, (NoResString)"HVLV Clearance",
						(NoResString)"This is to enable Legacy HVLV functionality.",
						RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
				});
			}
		}

		#endregion

		#region HVLV Last Mile Carrier Depot Calculations

		public BooleanRegistryItem HVLVAutomaticallyCalculateLMCDepotDetails
		{
			get
			{
				return GetItem("HVLVAutomaticallyCalculateLMCDepotDetails", delegate
				{
					return new BooleanRegistryItem(
						"HVLVAutomaticallyCalculateLMCDepotDetails",
						Categories.Freight_HVLV_LastMileCarrier,
						ResString.GetMultilingualString("790852da-884a-4cee-8d2b-1f7d13452bc3", "Automatically Calculate HVLV Last Mile Carrier & Depot Details"),
						ResString.GetMultilingualString("5bfdb62e-480d-42de-8dc9-ccd9459c2c9e", @"Enables automatic calculation of Last Mile Carrier & Depot Details for HVLV Consignments via Service Tasks.

Automatic calculation only works when changes made to the Port/Depot/Carrier Selection related fields on HVLV Consignment are detected by Service Tasks. For existing HVLV Consignments without changes made, please use manual action to calculate."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem HVLVConsignmentOnlyPopulatesEmptyLMCDepotDetails
		{
			get
			{
				return GetItem("HVLVConsignmentOnlyPopulatesEmptyLMCDepotDetails", delegate
				{
					return new BooleanRegistryItem(
						"HVLVConsignmentOnlyPopulatesEmptyLMCDepotDetails",
						Categories.Freight_HVLV_LastMileCarrier,
						ResString.GetMultilingualString("81aa5a04-decf-4ed3-a9f5-abfa9586f78b", "Last Mile Carrier and Depot Recalculation"),
						ResString.GetMultilingualString("14ec9a3f-8044-4c01-ac61-374aa6688884", @"Automatic re-calculation of Last Mile Carrier & Depot Details for HVLV Consignments via Service Tasks when Registry Item: Automatically Calculate HVLV Last Mile Carrier & Depot Details is enabled.

When set to 'No', behavior will be to populate and override if needed. Service Tasks will re-calculate and populate HVLV Consignment Last Mile Carrier and Depot Details when changes are made to the Port/Depot/Carrier Selection related fields on a HVLV Consignment. Already populated fields will be overridden after re-calculation.

When set to 'Yes', behavior will be to only populate if empty. Service Tasks will only re-calculate and populate HVLV Consignment Last Mile Carrier and Depot Details that are not already populated when changes are made to the Port/Depot/Carrier Selection related fields on a HVLV Consignment."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region HVLV Pre-Screening

		public HVLVPreScreeningRuleRegistryItem HVLVDetailsPreScreeningConfiguration
		{
			get
			{
				return GetItem("HVLVDetailsPreScreeningConfiguration", delegate
				{
					var result = new HVLVPreScreeningRuleRegistryItem(
						"HVLVDetailsPreScreeningConfiguration",
						Categories.Freight_HVLV,
						ResString.GetMultilingualString("e633531f-074d-43d2-8a38-e80fd8304613", "HVLV Pre-Screening"),
						ResString.GetMultilingualString("77c6a70d-53e1-45b5-b7f4-1763722446c8", "Use this registry setting to configure pre-screening on specific HVLV fields for stop words, phrases or values."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.NotCached);

					return result;
				});
			}
		}

		#endregion

		#region HVLV Chargeable

		public BooleanRegistryItem CalculateHVLVChargeablePerItem
		{
			get
			{
				return GetItem("CalculateHVLVChargeablePerItem", delegate
				{
					return new BooleanRegistryItem(
						"CalculateHVLVChargeablePerItem",
						Categories.Freight_HVLV,
						ResString.GetMultilingualString("d0b781f9-eb97-4bfe-9df5-5de54b1ca958", "Calculate HVLV Chargeable per Item"),
						ResString.GetMultilingualString("a8ba15aa-1c67-4994-aade-70457f1e6567", @"This registry determines how Chargeable is calculated for a HVLV Consignment.

When this registry is enabled, Consignment Chargeable will be calculated by first calculating the Chargeable of each attached Item, and then totaling the result.

When this registry is disabled, Consignment Chargeable will be calculated by first totaling the measurements of each attached Item, and then calculating Chargeable from the totaled measurements."),
						RegistryStorageFlags.All,
						false);
				});
			}
		}

		#endregion

		#region HVLV FHL

		public BooleanRegistryItem EnableHVLVFHLMessaging
		{
			get
			{
				return GetItem("EnableHVLVFHLMessaging", delegate
				{
					return new BooleanRegistryItem(
						"EnableHVLVFHLMessaging",
						Categories.Freight_HVLV,
						ResString.GetMultilingualString("70F647F4-7229-4442-B301-EDCA9C8574F3", "Enable HVLV FHL Messaging"),
						ResString.GetMultilingualString("826D66DA-3D1F-41A1-9103-D6E49C13C45E", @"This is to enable FHL message for each HVLV Consignment. When set to 'Yes', an FHL message will be sent for the Consignments within the HVL Shipment but not the HVL Shipment itself.
Whereas when set to 'No', the FHL message will be sent only for the HVL Shipment, not its Consignments."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						false
						);
				});
			}
		}

		#endregion

		#region HVLV Bulk Populate Consignment and Item IDs

		public BooleanRegistryItem AutoGenerateConsignmentAndItemIDs
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AutoGenerateConsignmentAndItemIDs", delegate
				{
					return new BooleanRegistryItem(
						"AutoGenerateConsignmentAndItemIDs",
						Categories.Freight_HVLV,
						ResString.GetMultilingualString("1ac37a61-f852-42c3-aae7-b059602a863e", "Auto-generate Consignment and Item IDs"),
						ResString.GetMultilingualString("3c50dde8-5526-453b-8949-8b09463bcd77", @"This registry determines how Consignment and Item IDs are generated.

When set to ‘Yes’, Consignment and Item IDs are generated from SSCC or CargoWise number fountain.

When set to ‘No’, Consignment and Item IDs are generated using system defaulting logic."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#region Commodity Code

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CW1161:Do not use constant string literals - use resource strings instead.", Justification = "Database query")]
		public GuidRegistryItem HVLVOuterPackageCommodityCode
		{
			get
			{
				return GetItem("HVLVOuterPackageCommodityCode", delegate
				{
					object defaultHVLVCommodityCodeObj = Db.Connection.ExecuteScalar(
						"select " + RefCommodityCodeSchema.PK.Name +
						" from " + RefCommodityCodeSchema.Constants.SqlSchemaName + "." + RefCommodityCodeSchema.Constants.TableName +
						" where " + RefCommodityCodeSchema.RH_Code.Name + " = @code",
						cmd => cmd.AddParameterBasedOnDbColumn("@code", "GEN", RefCommodityCodeSchema.RH_Code));
					Guid defaultHVLVCommodityCode = defaultHVLVCommodityCodeObj != null ? (Guid)defaultHVLVCommodityCodeObj : Guid.Empty;
					GuidRegistryItem result = new GuidRegistryItem(
						"HVLVOuterPackageCommodityCode",
						Categories.Freight_HVLV,
						ResString.GetMultilingualString("d9ff5c4f-ebe3-480a-8bee-24d69be35b55", "Commodity Code Default"),
						ResString.GetMultilingualString("ebcc7da5-63ae-46f0-9efc-0f3ad6ad5dbd", "Use to default the Commodity Code on the HVLV Outer Package"),
						RegistryStorageFlags.All,
						defaultHVLVCommodityCode);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.RefCommodityCode);
					return result;
				});
			}
		}

		#endregion

		#region HVLV Customs

		internal sealed class HVLVCustomsRegistryNames
		{
			internal sealed class Australia
			{
				public const string AirCargoReport_RemoveNonWesternEuropeanCharacters = "RemoveNonWesternEuropeanCharactersAUAirCargoReport";
				public const string ExportSubManifest_RemoveNonWesternEuropeanCharacters = "RemoveNonWesternEuropeanCharactersAUExportSubManifest";
				public const string SeaCargoReport_RemoveNonWesternEuropeanCharacters = "RemoveNonWesternEuropeanCharactersAUSeaCargoReport";
				public const string StandAloneDeclaration_RemoveNonWesternEuropeanCharacters = "RemoveNonWesternEuropeanCharactersAUStandAloneDeclaration";
			}

			internal sealed class Canada
			{
				public const string EManifest_RemoveNonWesternEuropeanCharacters = "RemoveNonWesternEuropeanCharactersCAeManifest";
				public const string EManifest_EnableHVLV = "EnableHVLVCAeManifest";
			}

			internal sealed class NewZealand
			{
				public const string StandAloneDeclaration_RemoveNonWesternEuropeanCharacters = "RemoveNonWesternEuropeanCharactersNZStandAloneDeclaration";
				public const string SeaCargoReport_RemoveNonWesternEuropeanCharacters = "RemoveNonWesternEuropeanCharactersNZSeaCargoReport";
				public const string AirCargoReport_RemoveNonWesternEuropeanCharacters = "RemoveNonWesternEuropeanCharactersNZAirCargoReport";
			}

			internal sealed class UnitedStates
			{
				public const string EManifest_RemoveNonWesternEuropeanCharacters = "RemoveNonWesternEuropeanCharactersUSeManifest";
				public const string AirAMS_RemoveNonWesternEuropeanCharacters = "RemoveNonWesternEuropeanCharactersUSAirAMS";
				public const string ACAS_RemoveNonWesternEuropeanCharacters = "RemoveNonWesternEuropeanCharactersUSACAS";
				public const string StandAloneDeclaration_RemoveNonWesternEuropeanCharacters = "RemoveNonWesternEuropeanCharactersUSStandAloneDeclaration";
				public const string LowValueEntries_RemoveNonWesternEuropeanCharacters = "RemoveNonWesternEuropeanCharactersUSLowValueEntries";
				public const string SeaAMS_RemoveNonWesternEuropeanCharacters = "RemoveNonWesternEuropeanCharactersUSSeaAMS";
				public const string ImporterSecurityFilling_RemoveNonWesternEuropeanCharacters = "RemoveNonWesternEuropeanCharactersUSImporterSecurityFilling";
				public const string EManifest_SkipShipmentValidation = "SkipeManifestShipmentValidation";
			}

			internal sealed class Singapore
			{
				public const string AccessImportManifest_RemoveNonWesternEuropeanCharacters = "RemoveNonWesternEuropeanCharactersSGSGAccessImportManifest";
			}

			internal sealed class EuropeanUnion
			{
				public const string EUICS2Manifest_RemoveNonWesternEuropeanCharacters = "RemoveNonWesternEuropeanCharactersEUICS2Manifest";
			}
		}

		MultilingualString RemoveNonWesternEuropeanCharactersCaption => ResString.GetMultilingualString("d1c08264-77fc-4c5d-b281-496e8652ab03", "Remove Non Western European Characters");

		RegistryStorageFlags RemoveNonWesternEuropeanCharactersRegistryStorageFlags => RegistryStorageFlags.Company | RegistryStorageFlags.Branch;

		#region Customs AU

		public BooleanRegistryItem RemoveNonWesternEuropeanCharactersAUAirCargoReport
		{
			get => GetItem<BooleanRegistryItem>(HVLVCustomsRegistryNames.Australia.AirCargoReport_RemoveNonWesternEuropeanCharacters, () =>
				new BooleanRegistryItem(
					HVLVCustomsRegistryNames.Australia.AirCargoReport_RemoveNonWesternEuropeanCharacters,
					Categories.Freight_HVLV_Customs_AU_AirCargoReport,
					RemoveNonWesternEuropeanCharactersCaption,
					ResString.GetMultilingualString("c96cf8a8-6d91-4305-92b8-9e49877fa1bf", "This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Air Cargo Reports from HVLV Shipments."),
					RemoveNonWesternEuropeanCharactersRegistryStorageFlags,
					false));
		}

		public BooleanRegistryItem RemoveNonWesternEuropeanCharactersAUExportSubManifest
		{
			get => GetItem<BooleanRegistryItem>(HVLVCustomsRegistryNames.Australia.ExportSubManifest_RemoveNonWesternEuropeanCharacters, () =>
				new BooleanRegistryItem(
					HVLVCustomsRegistryNames.Australia.ExportSubManifest_RemoveNonWesternEuropeanCharacters,
					Categories.Freight_HVLV_Customs_AU_ExportSubManifest,
					RemoveNonWesternEuropeanCharactersCaption,
					ResString.GetMultilingualString("60313ad9-76a1-4837-bb36-c6677365f366", "This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Export Sub Manifests from HVLV Shipments."),
					RemoveNonWesternEuropeanCharactersRegistryStorageFlags,
					false));
		}

		public BooleanRegistryItem RemoveNonWesternEuropeanCharactersAUSeaCargoReport
		{
			get => GetItem<BooleanRegistryItem>(HVLVCustomsRegistryNames.Australia.SeaCargoReport_RemoveNonWesternEuropeanCharacters, () =>
					new BooleanRegistryItem(
					HVLVCustomsRegistryNames.Australia.SeaCargoReport_RemoveNonWesternEuropeanCharacters,
					Categories.Freight_HVLV_Customs_AU_SeaCargoReport,
					RemoveNonWesternEuropeanCharactersCaption,
					ResString.GetMultilingualString("ab5c621e-0bd5-4f25-85c8-db8d5c8f90bf", "This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Sea Cargo Reports from HVLV Shipments."),
					RemoveNonWesternEuropeanCharactersRegistryStorageFlags,
					false));
		}

		public BooleanRegistryItem RemoveNonWesternEuropeanCharactersAUStandAloneDeclaration
		{
			get => GetItem<BooleanRegistryItem>(HVLVCustomsRegistryNames.Australia.StandAloneDeclaration_RemoveNonWesternEuropeanCharacters, () =>
				new BooleanRegistryItem(
					HVLVCustomsRegistryNames.Australia.StandAloneDeclaration_RemoveNonWesternEuropeanCharacters,
					Categories.Freight_HVLV_Customs_AU_StandAloneDeclaration,
					RemoveNonWesternEuropeanCharactersCaption,
					ResString.GetMultilingualString("20927dee-b813-4355-8ed6-8ba9171a18b4", "This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Stand Alone Declarations from HVLV Shipments."),
					RemoveNonWesternEuropeanCharactersRegistryStorageFlags,
					false));
		}

		#endregion

		#region Customs CA

		public BooleanRegistryItem RemoveNonWesternEuropeanCharactersCAeManifest
		{
			get => GetItem<BooleanRegistryItem>(HVLVCustomsRegistryNames.Canada.EManifest_RemoveNonWesternEuropeanCharacters, () =>
				new BooleanRegistryItem(
					HVLVCustomsRegistryNames.Canada.EManifest_RemoveNonWesternEuropeanCharacters,
					Categories.Freight_HVLV_Customs_CA_eManifest,
					RemoveNonWesternEuropeanCharactersCaption,
					ResString.GetMultilingualString("ae61811e-ce37-487b-abaa-a335e3f748ce", "This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating eManifests from HVLV Shipments."),
					RemoveNonWesternEuropeanCharactersRegistryStorageFlags,
					false));
		}

		#region EnableHVLVCAeManifest

		public BooleanRegistryItem EnableHVLVCAeManifest
		{
			get => GetItem(HVLVCustomsRegistryNames.Canada.EManifest_EnableHVLV, () =>
					new BooleanRegistryItem(
					HVLVCustomsRegistryNames.Canada.EManifest_EnableHVLV,
					Categories.Freight_HVLV_Customs_CA_eManifest,
					ResString.GetMultilingualString("D389CCB6-D151-4695-B5DC-87DB83D1D246", "Enable HVLV CA eManifest Forwarder Messaging"),
					ResString.GetMultilingualString("AE6AC4A0-60D5-4417-9577-5600D7902282", "This to enable eManifest Forwarder messaging for each HVLV Consignment. An eManifest message will not be sent for the shipment house bill when enabled."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default,
					false
				));
		}

		#endregion

		#endregion

		#region Customs NZ

		public BooleanRegistryItem RemoveNonWesternEuropeanCharactersNZAirCargoReport
		{
			get => GetItem<BooleanRegistryItem>(HVLVCustomsRegistryNames.NewZealand.AirCargoReport_RemoveNonWesternEuropeanCharacters, () =>
				new BooleanRegistryItem(
					HVLVCustomsRegistryNames.NewZealand.AirCargoReport_RemoveNonWesternEuropeanCharacters,
					Categories.Freight_HVLV_Customs_NZ_AirCargoReport,
					RemoveNonWesternEuropeanCharactersCaption,
					ResString.GetMultilingualString("4b24f8a9-3c27-4e31-9d0c-3d7e3b80447d", "This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Air Cargo ICR/CREs from HVLV Shipments."),
					RemoveNonWesternEuropeanCharactersRegistryStorageFlags,
					false));
		}

		public BooleanRegistryItem RemoveNonWesternEuropeanCharactersNZSeaCargoReport
		{
			get => GetItem<BooleanRegistryItem>(HVLVCustomsRegistryNames.NewZealand.SeaCargoReport_RemoveNonWesternEuropeanCharacters, () =>
				new BooleanRegistryItem(
				HVLVCustomsRegistryNames.NewZealand.SeaCargoReport_RemoveNonWesternEuropeanCharacters,
				Categories.Freight_HVLV_Customs_NZ_SeaCargoReport,
				RemoveNonWesternEuropeanCharactersCaption,
				ResString.GetMultilingualString("f3274255-4586-43df-8157-bf52acfd030c", "This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Sea Cargo ICR/CREs from HVLV Shipments."),
				RemoveNonWesternEuropeanCharactersRegistryStorageFlags,
				false));
		}

		public BooleanRegistryItem RemoveNonWesternEuropeanCharactersNZStandAloneDeclaration
		{
			get => GetItem<BooleanRegistryItem>(HVLVCustomsRegistryNames.NewZealand.StandAloneDeclaration_RemoveNonWesternEuropeanCharacters, () =>
				new BooleanRegistryItem(
				HVLVCustomsRegistryNames.NewZealand.StandAloneDeclaration_RemoveNonWesternEuropeanCharacters,
				Categories.Freight_HVLV_Customs_NZ_StandAloneDeclaration,
				RemoveNonWesternEuropeanCharactersCaption,
				ResString.GetMultilingualString("28f760d0-4bdc-4b0a-b39b-ecbb3fbe32e1", "This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Stand Alone Declarations from HVLV Shipments."),
				RemoveNonWesternEuropeanCharactersRegistryStorageFlags,
				false));
		}

		public BooleanRegistryItem EnableHVLVMultiShipmentNZICRCRE
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableHVLVMultiShipmentNZICRCRE", delegate
				{
					return new BooleanRegistryItem(
						"EnableHVLVMultiShipmentNZICRCRE",
						Categories.Freight_HVLV_Customs_NZ_ICRCRE,
						ResString.GetMultilingualString("450015ea-a4dd-464b-b026-0bf8f04b80c2", "Enable Multi Shipment ICR/CRE"),
						ResString.GetMultilingualString("a09b2a81-92f2-46e9-9679-c5a27ccb150b", @"This registry enable add multiple shipments to same New Zealand ICR/CRE if they belong to same consol.

When set to ‘Yes’, shipments linked to same consol will be added to same ICR/CRE.

When set to ‘No’, separate ICR/CRE for each shipment."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region Customs US

		public BooleanRegistryItem RemoveNonWesternEuropeanCharactersUSACAS
		{
			get => GetItem<BooleanRegistryItem>(HVLVCustomsRegistryNames.UnitedStates.ACAS_RemoveNonWesternEuropeanCharacters, () =>
				new BooleanRegistryItem(
					HVLVCustomsRegistryNames.UnitedStates.ACAS_RemoveNonWesternEuropeanCharacters,
					Categories.Freight_HVLV_Customs_US_ACAS,
					RemoveNonWesternEuropeanCharactersCaption,
					ResString.GetMultilingualString("fe788450-104c-44e3-a6e4-7d8d88d372c9", "This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating ACAS Reports from HVLV Shipments."),
					RemoveNonWesternEuropeanCharactersRegistryStorageFlags,
					false));
		}

		public BooleanRegistryItem RemoveNonWesternEuropeanCharactersUSAirAMS
		{
			get => GetItem<BooleanRegistryItem>(HVLVCustomsRegistryNames.UnitedStates.AirAMS_RemoveNonWesternEuropeanCharacters, () =>
				new BooleanRegistryItem(
					HVLVCustomsRegistryNames.UnitedStates.AirAMS_RemoveNonWesternEuropeanCharacters,
					Categories.Freight_HVLV_Customs_US_AirAMS,
					RemoveNonWesternEuropeanCharactersCaption,
					ResString.GetMultilingualString("1775603a-0bda-417a-95d9-fcc742fbcaa5", "This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Air AMS fillings from HVLV Shipments."),
					RemoveNonWesternEuropeanCharactersRegistryStorageFlags,
					false));
		}

		public BooleanRegistryItem RemoveNonWesternEuropeanCharactersUSeManifest
		{
			get => GetItem<BooleanRegistryItem>(HVLVCustomsRegistryNames.UnitedStates.EManifest_RemoveNonWesternEuropeanCharacters, () =>
					new BooleanRegistryItem(
					HVLVCustomsRegistryNames.UnitedStates.EManifest_RemoveNonWesternEuropeanCharacters,
					Categories.Freight_HVLV_Customs_US_eManifest,
					RemoveNonWesternEuropeanCharactersCaption,
					ResString.GetMultilingualString("bbf7e9c4-bd61-4db3-9ba3-0b8ed8a48fcf", "This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating e-Manifests from HVLV Shipments."),
					RemoveNonWesternEuropeanCharactersRegistryStorageFlags,
					false));
		}

		#region HVLV eManifest

		public BooleanRegistryItem SkipeManifestShipmentValidation
		{
			get => GetItem(HVLVCustomsRegistryNames.UnitedStates.EManifest_SkipShipmentValidation, () =>
				new BooleanRegistryItem(
					HVLVCustomsRegistryNames.UnitedStates.EManifest_SkipShipmentValidation,
					Categories.Freight_HVLV_Customs_US_eManifest,
					ResString.GetMultilingualString("42761A3D-92C0-458D-92F7-B1AAB4631A06", "Skip eManifest Shipment Validation"),
					ResString.GetMultilingualString("FC4B8A45-0AE1-473E-A32C-9413A9F11E8F", "Setting this registry to Yes will skip validation of e-Manifest Shipments when transmitting messages to Customs."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					false));
		}

		#endregion

		public BooleanRegistryItem RemoveNonWesternEuropeanCharactersUSImporterSecurityFilling
		{
			get => GetItem<BooleanRegistryItem>(HVLVCustomsRegistryNames.UnitedStates.ImporterSecurityFilling_RemoveNonWesternEuropeanCharacters, () =>
				new BooleanRegistryItem(
					HVLVCustomsRegistryNames.UnitedStates.ImporterSecurityFilling_RemoveNonWesternEuropeanCharacters,
					Categories.Freight_HVLV_Customs_US_ImporterSecurityFilling,
					RemoveNonWesternEuropeanCharactersCaption,
					ResString.GetMultilingualString("723e97e7-a518-42fc-9df2-3944c0319586", "This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Importer Security Fillings from HVLV Shipments."),
					RemoveNonWesternEuropeanCharactersRegistryStorageFlags,
					false));
		}

		public BooleanRegistryItem RemoveNonWesternEuropeanCharactersUSSeaAMS
		{
			get => GetItem<BooleanRegistryItem>(HVLVCustomsRegistryNames.UnitedStates.SeaAMS_RemoveNonWesternEuropeanCharacters, () =>
				new BooleanRegistryItem(
					HVLVCustomsRegistryNames.UnitedStates.SeaAMS_RemoveNonWesternEuropeanCharacters,
					Categories.Freight_HVLV_Customs_US_SeaAMS,
					RemoveNonWesternEuropeanCharactersCaption,
					ResString.GetMultilingualString("06e3569f-d7bd-479d-aff8-e270bffc9e36", "This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Sea AMS fillings from HVLV Shipments."),
					RemoveNonWesternEuropeanCharactersRegistryStorageFlags,
					false));
		}

		public BooleanRegistryItem RemoveNonWesternEuropeanCharactersUSLowValueEntries
		{
			get => GetItem<BooleanRegistryItem>(HVLVCustomsRegistryNames.UnitedStates.LowValueEntries_RemoveNonWesternEuropeanCharacters, () =>
					new BooleanRegistryItem(
					HVLVCustomsRegistryNames.UnitedStates.LowValueEntries_RemoveNonWesternEuropeanCharacters,
					Categories.Freight_HVLV_Customs_US_LowValueEntries,
					RemoveNonWesternEuropeanCharactersCaption,
					ResString.GetMultilingualString("7350139f-5d3b-45b9-8857-b75251311e08", "This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Low Value Entries from HVLV Shipments."),
					RemoveNonWesternEuropeanCharactersRegistryStorageFlags,
					false));
		}

		public BooleanRegistryItem RemoveNonWesternEuropeanCharactersUSStandAloneDeclaration
		{
			get => GetItem<BooleanRegistryItem>(HVLVCustomsRegistryNames.UnitedStates.StandAloneDeclaration_RemoveNonWesternEuropeanCharacters, () =>
				new BooleanRegistryItem(
					HVLVCustomsRegistryNames.UnitedStates.StandAloneDeclaration_RemoveNonWesternEuropeanCharacters,
					Categories.Freight_HVLV_Customs_US_StandAloneDeclaration,
					RemoveNonWesternEuropeanCharactersCaption,
					ResString.GetMultilingualString("bfb01ac4-e0af-49a8-81a2-86eea44f378c", "This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating Stand Alone Declarations from HVLV Shipments."),
					RemoveNonWesternEuropeanCharactersRegistryStorageFlags,
					false));
		}

		#endregion

		#region Customs SG

		public BooleanRegistryItem RemoveNonWesternEuropeanCharactersSGSGAccessImportManifest
		{
			get => GetItem<BooleanRegistryItem>(HVLVCustomsRegistryNames.Singapore.AccessImportManifest_RemoveNonWesternEuropeanCharacters, () =>
				new BooleanRegistryItem(
					HVLVCustomsRegistryNames.Singapore.AccessImportManifest_RemoveNonWesternEuropeanCharacters,
					Categories.Freight_HVLV_Customs_SG_SGAccessImportManifest,
					RemoveNonWesternEuropeanCharactersCaption,
					ResString.GetMultilingualString("95c609dc-cdd3-49f2-abb7-20ae68968f77", "This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating SG Access Import Manifests from HVLV Shipments."),
					RemoveNonWesternEuropeanCharactersRegistryStorageFlags,
					false));
		}

		#endregion

		#region Customs EU

		public BooleanRegistryItem RemoveNonWesternEuropeanCharactersEUICS2Manifest
		{
			get => GetItem(HVLVCustomsRegistryNames.EuropeanUnion.EUICS2Manifest_RemoveNonWesternEuropeanCharacters, () =>
				new BooleanRegistryItem(
					HVLVCustomsRegistryNames.EuropeanUnion.EUICS2Manifest_RemoveNonWesternEuropeanCharacters,
					Categories.Freight_HVLV_Customs_EU_ICS2,
					RemoveNonWesternEuropeanCharactersCaption,
					ResString.GetMultilingualString("9b5dc6f7-6850-4810-b57f-7a9dbc825b8c", "This flag is to indicate that Non Western European Characters in HVLV fields should be removed or not when creating EU ICS2 Manifests from HVLV Shipments."),
					RemoveNonWesternEuropeanCharactersRegistryStorageFlags,
					defaultValue: false));
		}

		#endregion

		#endregion

		#region Enable Security Filings For Non USA Companies

		public BooleanRegistryItem EnableSecurityFilingsForNonUSACompanies
		{
			get
			{
				return GetItem("EnableSecurityFilingsForNonUSACompanies", delegate
				{
					return new BooleanRegistryItem(
						"EnableSecurityFilingsForNonUSACompanies",
						Categories.Freight_HVLV_Customs_US,
						ResString.GetMultilingualString("4568f864-5e3c-46af-86d7-1c1c50573f42", "Enable Security Filings for non USA based Companies"),
						ResString.GetMultilingualString("2a9a56bb-36ba-48de-9ab1-59a14aeab271", "When set to 'Yes' companies outside of the USA can create Security Filings from a HVL shipment that has a destination country of the USA."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#endregion

		#region Use new 'Port/Carrier/Depot Selection' module

		public BooleanRegistryItem PortCarrierDepotSelectionModule
		{
			get
			{
				return GetItem("PortCarrierDepotSelectionModule", delegate
				{
					return new BooleanRegistryItem(
						"PortCarrierDepotSelectionModule",
						Categories.Freight_HVLV,
						(NoResString)"Use new 'Port/Carrier/Depot Selection' module",
						(NoResString)"When set to 'Yes', the new 'Port/Carrier/Depot Selection' module will be used",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region Threshold to Auto Load HVLV Consignments on Shipment Form

		public IntRegistryItem ThresholdToAutoLoadHVLVConsignmentsOnShipmentForm
		{
			get
			{
				return GetItem<IntRegistryItem>("ThresholdToAutoLoadHVLVConsignmentsOnShipmentForm", delegate
				{
					return new IntRegistryItem(
						"ThresholdToAutoLoadHVLVConsignmentsOnShipmentForm",
						Categories.Freight_HVLV,
						ResString.GetMultilingualString("c763d6b3-4f3c-47e6-85a6-887d37db470a", "HVLV Consignment Auto Load Threshold"),
						ResString.GetMultilingualString("d4941d34-9855-48a5-a852-a032277e4f68", "Specifies the number of HVLV Consignments on a HVL Shipment that will be loaded by default. When the number of HVLV Consignments exceeds the threshold, Consignments grid will not load any records and will be set to read-only."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						1000,
						0,
						10000);
				});
			}
		}

		#endregion

		#region Bulk Copy Batch Size

		public IntRegistryItem HVLVDataBulkCopyBatchSize
		{
			get
			{
				return GetItem<IntRegistryItem>("HVLVDataBulkCopyBatchSize", delegate
				{
					return new IntRegistryItem(
						name: "HVLVDataBulkCopyBatchSize",
						category: Categories.Freight_HVLV,
						caption: (NoResString)"Bulk Copy Batch Size",
						hint: (NoResString)"This registry setting controls the Batch Size of SQL Bulk Copy process when saving HVLV data.",
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForSupport,
						defaultValue: 100000,
						minValue: 100,
						maxValue: 100000);
				});
			}
		}

		#endregion

		#region Update HVL Shipments Weight, Volume and Inners Default

		public CodePairRegistryItem HVLVShipmentWeightUpdateMethod
		{
			get
			{
				return GetItem<CodePairRegistryItem>("HVLVShipmentWeightUpdateMethod", delegate
				{
					var listProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair(Constants.ShipmentWeightUpdateOptions.Code.ShowWarningFromConsignmentsDetails, Constants.ShipmentWeightUpdateOptions.Description.ShowWarningFromConsignmentsDetails);
						list.AddPair(Constants.ShipmentWeightUpdateOptions.Code.AlwaysUpdateFromConsignmentsDetails, Constants.ShipmentWeightUpdateOptions.Description.AlwaysUpdateFromConsignmentsDetails);
						list.AddPair(Constants.ShipmentWeightUpdateOptions.Code.DoNotUpdate, Constants.ShipmentWeightUpdateOptions.Description.DoNotUpdate);
						list.AddPair(Constants.ShipmentWeightUpdateOptions.Code.ShowWarningFromPackingDetails, Constants.ShipmentWeightUpdateOptions.Description.ShowWarningFromPackingDetails);
						list.AddPair(Constants.ShipmentWeightUpdateOptions.Code.AlwaysUpdateFromPackingDetails, Constants.ShipmentWeightUpdateOptions.Description.AlwaysUpdateFromPackingDetails);
						return list;
					});

					return new CodePairRegistryItem(
						"HVLVShipmentWeightUpdateMethod",
						Categories.Freight_HVLV,
						ResString.GetMultilingualString("a744da2a-1fe6-4296-a25d-d6aa6055806c", "Update HVL Shipments Weight, Volume and Inners Default"),
						ResString.GetMultilingualString("20be1e0c-82c5-4204-9903-2adcea257ccd", "Defaults how HVL Shipment weight, volume and inners are updated when not matching with Items total weight, volume and counts upon saving the shipment."),
						listProvider,
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Constants.ShipmentWeightUpdateOptions.Code.ShowWarningFromConsignmentsDetails);
				});
			}
		}

		#endregion

		#region HVLV Denied Party Screening

		public HVLVEnablePartyScreeningRegistryItem HVLVEnablePartyScreening
		{
			get
			{
				return GetItem("HVLVEnablePartyScreening", delegate
				{
					return new HVLVEnablePartyScreeningRegistryItem(
						"HVLVEnablePartyScreening",
						Categories.Freight_HVLV_DeniedPartyScreening,
						ResString.GetMultilingualString("678cab28-2b7f-4e34-b9e0-181ac21e10c2", "Enable HVLV Party Screening"),
						ResString.GetMultilingualString("8f3fd6ad-dcce-46d5-9b14-e3f3f0ef6f95",
@"Use this registry setting to enable Denied Party Screening for HVLV parties and enable new DPS result form.

When HVLV Party Screening is enabled, HVLV parties will be screened from Consol, Shipment and HVLV Booking Header.

When New DPS Result Form is enabled, potential matches will be displayed on the new form when DPS is run from a Consol that contains HVL Shipment(s), a HVL Shipment or a HVLV Booking Header."),
						RegistryStorageFlags.System,
						RegistryOptions.Default);
				});
			}
		}

		#endregion

		#region Bulk Copy

		public BooleanRegistryItem EnableHVLVDataBulkCopy
		{
			get
			{
				return GetItem("EnableHVLVDataBulkCopy", delegate
				{
					return new BooleanRegistryItem(
						"EnableHVLVDataBulkCopy",
						Categories.Freight_HVLV,
						(NoResString)"Enable Bulk Copy",
						(NoResString)"Enable SQL Bulk Copy for HVLV data.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: true);
				});
			}
		}

		#endregion
	}
}
