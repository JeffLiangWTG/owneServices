using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Freight.AirlineMessagingCargoIMPVersion;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Integration.Forwarding;

[assembly: UsesConstants(typeof(TWAdditionalReferenceTypesForUniversalXML))]

namespace Enterprise.Registry.Business
{
	public sealed class FreightDataRegistry : RegistryItemSet
	{
		FreightDataRegistry() { }

		#region Instance / AddRegistryItems

		public static FreightDataRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new FreightDataRegistry();
				}
				return fInstance;
			}
		}

		[ThreadStatic]
		static FreightDataRegistry fInstance;

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Freight_HouseBills_HouseBillofLadingTypes_FIATAHBLSettings { get { return CombineCategories(Freight_HouseBills_HouseBillofLadingTypes, ResString.GetMultilingualString("644a31f7-6f73-4241-a187-0639296daacb", "FIATA HBL Settings")); } }
			public static MultilingualString Freight_HouseBills_Clauses { get { return CombineCategories(Freight_HouseBills, ResString.GetMultilingualString("3bef6f76-4826-47ae-8c8e-ec67ec0ac026", "Clauses")); } }
			public static MultilingualString Freight_HouseBills_NumberCustomizations { get { return CombineCategories(Freight_HouseBills, ResString.GetMultilingualString("201ef3c4-04b3-4f92-a40e-fc773a5bc571", "Number Customizations")); } }
			public static MultilingualString Freight_CFS { get { return CombineCategories(Freight, ResString.GetMultilingualString("0a6a0071-ed95-46a7-8c39-3a1f6a193685", "CFS")); } }
			public static MultilingualString Freight_CFS_Outturn { get { return CombineCategories(Freight_CFS, ResString.GetMultilingualString("aa8d7702-d9e6-4299-8299-0256502515c7", "Outturn")); } }
			public static MultilingualString Freight_AWB_HAWB_CustomizableExtraText { get { return CombineCategories(Freight_AWB_HAWB, ResString.GetMultilingualString("913a3589-7ef4-48c8-a40a-393ba1325e91", "Customizable Extra Text")); } }
			public static MultilingualString Freight_AWB_HAWB_TermsandConditions { get { return CombineCategories(Freight_AWB_HAWB, ResString.GetMultilingualString("ccd7edeb-7320-4be0-a473-dd4c07534369", "Terms and Conditions")); } }
			public static MultilingualString Freight_AWB_MAWB_CustomizableExtraText { get { return CombineCategories(Freight_AWB_MAWB, ResString.GetMultilingualString("6c6f7ac2-681b-4f06-837c-d85435057651", "Customizable Extra Text")); } }
			public static MultilingualString Freight_AWB_MAWB_TermsandConditions { get { return CombineCategories(Freight_AWB_MAWB, ResString.GetMultilingualString("66f76de4-dfa3-48c5-bb89-a2470746a6b5", "Terms and Conditions")); } }
			public static MultilingualString Freight_AWB_CargoIMP { get { return CombineCategories(Freight_AWB, ResString.GetMultilingualString("9f99f844-8ca0-42e9-bc4a-7bf0ac783eb2", "CargoIMP")); } }
			public static MultilingualString Freight_AWB_CargoIMP_FWBMessaging { get { return CombineCategories(Freight_AWB_CargoIMP, ResString.GetMultilingualString("b29791ff-bac8-4f97-957c-7fcbe9cd3482", "FWB Messaging")); } }
			public static MultilingualString Freight_AWB_HongKong { get { return CombineCategories(Freight_AWB, ResString.GetMultilingualString("5f495e6b-1fa5-431d-a5c6-1a2a695f4a6b", "Hong Kong")); } }
			public static MultilingualString Freight_INCOTERMs { get { return CombineCategories(Freight, ResString.GetMultilingualString("2b25c471-ea7a-378b-448b-079289b55ef1", "Incoterms")); } }
			public static MultilingualString Freight_Consolidations { get { return CombineCategories(Freight, ResString.GetMultilingualString("63543e98-0283-411b-b4aa-0b991bb0aee2", "Consolidations")); } }
			public static MultilingualString Freight_Consolidations_Phases { get { return CombineCategories(Freight_Consolidations, ResString.GetMultilingualString("1bc44efb-ae30-460c-a171-c54a3ef0942f", "Phases")); } }
			public static MultilingualString Freight_Consolidations_Canada { get { return CombineCategories(Freight_Consolidations, ResString.GetMultilingualString("12a1d9fc-8430-4002-922d-5cef33b16863", "Canada")); } }
			public static MultilingualString Freight_Consolidations_OceanCarrierMessaging { get { return CombineCategories(Freight_Consolidations, ResString.GetMultilingualString("079b046b-2999-4575-a930-4e639ee5149b", "Ocean Carrier Messaging")); } }
			public static MultilingualString Freight_Shipment_HBLDeliveryMode { get { return CombineCategories(Freight_Shipment, ResString.GetMultilingualString("deccc8cb-d363-4731-ba81-4aab3257cddd", "HBL Delivery Mode")); } }
			public static MultilingualString Freight_Shipment_CustomAttributes { get { return CombineCategories(Freight_Shipment, ResString.GetMultilingualString("3dae08d9-77f2-46f6-8193-475541b63d14", "Custom Attributes")); } }
			public static MultilingualString Freight_Shipment_Sendingarnumer { get { return CombineCategories(Freight_Shipment, ResString.GetMultilingualString("b3ad6882-7b87-4839-8b60-3461ebf03bb3", "Sendingarnumer")); } }
			public static MultilingualString Freight_Shipment_UnitedArabEmirates { get { return CombineCategories(Freight_Shipment, ResString.GetMultilingualString("4d86e185-670f-4b74-9cc1-fc5afdc01f7b", "United Arab Emirates")); } }
			public static MultilingualString Freight_Shipment_Phases { get { return CombineCategories(Freight_Shipment, ResString.GetMultilingualString("1bc44efb-ae30-460c-a171-c54a3ef0942f", "Phases")); } }
			public static MultilingualString Freight_Shipment_Canada { get { return CombineCategories(Freight_Shipment, ResString.GetMultilingualString("a048d7a0-6bcf-43d7-bc94-868dcc6c9ba6", "Canada")); } }
			public static MultilingualString Freight_Shipment_DeliveryDueDate { get { return CombineCategories(Freight_Shipment, ResString.GetMultilingualString("cccb8ad2-f305-461b-8019-19ac419948df", "Delivery Due Date")); } }
			public static MultilingualString Freight_SupplyChainSecurity { get { return CombineCategories(Freight, ResString.GetMultilingualString("2be29239-980b-4a86-b106-f133d72f69d6", "Supply Chain Security")); } }
			public static MultilingualString Freight_SupplyChainSecurity_Japan { get { return CombineCategories(Freight_SupplyChainSecurity, ResString.GetMultilingualString("8b4ae0a7-b0c7-479e-a63c-c3c9a0687b64", "Japan")); } }
			public static MultilingualString Freight_SupplyChainSecurity_HongKong { get { return CombineCategories(Freight_SupplyChainSecurity, ResString.GetMultilingualString("7bbfe6ed-1e4b-43e2-b4b1-7c3155a37d46", "Hong Kong")); } }
			public static MultilingualString Freight_SupplyChainSecurity_AU { get { return CombineCategories(Freight_SupplyChainSecurity, ResString.GetMultilingualString("64b47a83-b5c2-4b24-9c3d-4c16f3966e17", "Australia")); } }
			public static MultilingualString Freight_SupplyChainSecurity_US { get { return CombineCategories(Freight_SupplyChainSecurity, ResString.GetMultilingualString("f3b3b1b3-4b3b-4b3b-4b3b-4b3b4b3b4b3b", "United States")); } }
			public static MultilingualString Freight_SupplyChainSecurity_EU { get { return CombineCategories(Freight_SupplyChainSecurity, ResString.GetMultilingualString("0c8b848c-5255-495c-8a70-31e87e0a3f32", "European Union")); } }
			public static MultilingualString Freight_SupplyChainSecurity_UK { get { return CombineCategories(Freight_SupplyChainSecurity, ResString.GetMultilingualString("0c5b848c-5256-495c-8a70-31e87e0a2f35", "United Kingdom")); } }
			public static MultilingualString Freight_SupplyChainSecurity_SG { get { return CombineCategories(Freight_SupplyChainSecurity, ResString.GetMultilingualString("c651cde4-0c0a-4801-9213-932d8b88c54b", "Singapore")); } }
			public static MultilingualString Freight_SupplyChainSecurity_TW { get { return CombineCategories(Freight_SupplyChainSecurity, ResString.GetMultilingualString("23aa03ff-35c5-4451-8133-8f0114834b5b", "Taiwan")); } }
			public static MultilingualString Freight_SupplyChainSecurity_CA { get { return CombineCategories(Freight_SupplyChainSecurity, ResString.GetMultilingualString("362b28c7-3247-434f-9fbf-7b3a4cda2a5f", "Canada")); } }
			public static MultilingualString Freight_SupplyChainSecurity_ZA { get { return CombineCategories(Freight_SupplyChainSecurity, ResString.GetMultilingualString("ba70b1cb-1d8b-47c5-bb34-7bc37c5f3c90", "South Africa")); } }
			public static MultilingualString Freight_Bookings { get { return CombineCategories(Freight, ResString.GetMultilingualString("b75b748b-4728-4a59-b17e-ac2d0c6a97af", "Bookings")); } }
			public static MultilingualString Freight_Bookings_HBLDeliveryMode { get { return CombineCategories(Freight_Bookings, ResString.GetMultilingualString("deccc8cb-d363-4731-ba81-4aab3257cddd", "HBL Delivery Mode")); } }
			public static MultilingualString Freight_Routing { get { return CombineCategories(Freight, ResString.GetMultilingualString("5fdba37b-9d5b-4ffb-9955-ed4ced695d94", "Routing")); } }
			public static MultilingualString Freight_Routing_AutomaticUpdatingofPlannedLegs { get { return CombineCategories(Freight_Routing, ResString.GetMultilingualString("ec8f77d0-619b-4d86-83a4-f3850823b77b", "Automatic Updating of Planned Legs")); } }
			public static MultilingualString Freight_Routing_AutomaticUpdatingofPlannedLegs_Sea { get { return CombineCategories(Freight_Routing_AutomaticUpdatingofPlannedLegs, ResString.GetMultilingualString("97748e63-fcce-4584-86ed-28c3ace3bf70", "Sea")); } }
			public static MultilingualString Freight_Notifications_SailingSchedules { get { return CombineCategories(Freight_Notifications, ResString.GetMultilingualString("ff6f5212-6e8f-44d6-8bf3-7524c6ec1fb9", "Sailing Schedules")); } }
			public static MultilingualString Freight_Notifications_ContainerAvailability { get { return CombineCategories(Freight_Notifications, ResString.GetMultilingualString("b91d1e04-af29-48b5-a9ee-f2998a82a680", "Container Availability")); } }
			public static MultilingualString Freight_ComTrac { get { return CombineCategories(Freight, ResString.GetMultilingualString("3b44cb52-c3f8-4e7e-abbc-774af8aa7d0b", "ComTrac")); } }
			public static MultilingualString Freight_Chargeable { get { return CombineCategories(Freight, ResString.GetMultilingualString("e2bdc463-d005-487c-b78d-ba44d9926c45", "Chargeable")); } }
			public static MultilingualString Freight_Chargeable_DomesticChargeable { get { return CombineCategories(Freight_Chargeable, ResString.GetMultilingualString("206588b7-2b62-4aa3-a54c-1435da6e31e1", "Domestic Chargeable")); } }
			public static MultilingualString Freight_Chargeable_InternationalChargeable { get { return CombineCategories(Freight_Chargeable, ResString.GetMultilingualString("6c007b67-3f53-429e-b6e9-2c8b67186467", "International Chargeable")); } }
			public static MultilingualString Freight_Chargeable_LoadingMeters { get { return CombineCategories(Freight_Chargeable, ResString.GetMultilingualString("05bb700b-df97-430d-b710-61cb625306f1", "Loading Meters")); } }
			public static MultilingualString Freight_POD { get { return CombineCategories(Freight, ResString.GetMultilingualString("dff19738-65ec-4cd5-8d77-15de83f6a8ef", "POD")); } }
			public static MultilingualString Freight_CustomsNumbers { get { return CombineCategories(Freight, ResString.GetMultilingualString("8de61339-f0b5-4f06-bffe-8292fddc89dc", "Customs Numbers")); } }
			public static MultilingualString Freight_Organizations { get { return CombineCategories(Freight, ResString.GetMultilingualString("36a81a84-83a8-4768-adcc-01e9fa247167", "Organizations")); } }
			public static MultilingualString Freight_Organizations_DefaultPortTransportCompany { get { return CombineCategories(Freight_Organizations, ResString.GetMultilingualString("10425c06-9de5-42e1-aed2-627896a76178", "Default Port Transport Company")); } }
			public static MultilingualString Freight_Organizations_DefaultFumigationContractor { get { return CombineCategories(Freight_Organizations, ResString.GetMultilingualString("d5f3d46d-152f-496a-8af0-2d1e5af105c9", "Default Fumigation Contractor")); } }
			public static MultilingualString Freight_Vessel { get { return CombineCategories(Freight, ResString.GetMultilingualString("cdfa949a-678a-4248-9d36-fbc2a827e0cd", "Vessel")); } }
			public static MultilingualString Freight_ALPO { get { return CombineCategories(Freight, ResString.GetMultilingualString("7a7807b2-dd14-4432-9402-3da59a617ba3", "ALPO")); } }
			public static MultilingualString Freight_ALPO_Export { get { return CombineCategories(Freight_ALPO, ResString.GetMultilingualString("6d0e2052-8454-4710-b16c-4441dc87eb7d", "Export")); } }
			public static MultilingualString Freight_AMS { get { return CombineCategories(Freight, ResString.GetMultilingualString("3519fb2c-1b13-4d97-8cb0-c4b72146045f", "AMS")); } }
			public static MultilingualString Freight_AMS_CAMIR { get { return CombineCategories(Freight_AMS, ResString.GetMultilingualString("4B6641B4-827E-4296-8FF3-53F64B1A9A35", "CAMIR")); } }
			public static MultilingualString Freight_India { get { return CombineCategories(Freight, ResString.GetMultilingualString("429247C1-3141-4178-9608-3524BA6165DF", "India")); } }
			public static MultilingualString Freight_SailingScheduleFeed { get { return CombineCategories(Freight, ResString.GetMultilingualString("fba6b556-9c1f-451d-93a8-f19fc1aa5506", "Sailing Schedule Feed")); } }
			public static MultilingualString Schedules { get { return ResString.GetMultilingualString("1fce395b-e0ac-4e29-92de-a1a689042b96", "Schedules"); } }
			public static MultilingualString Schedules_SailingSchedule { get { return CombineCategories(Schedules, ResString.GetMultilingualString("25694926-678b-4f71-a3f0-891773f7a170", "Sailing Schedule")); } }
			public static MultilingualString Freight_GlobalTracking { get { return CombineCategories(Freight, ResString.GetMultilingualString("b2240557-ab31-4988-8680-2660cac97b20", "Global Tracking")); } }
			public static MultilingualString Freight_GlobalTracking_GlobalSailingSchedules { get { return CombineCategories(Freight_GlobalTracking, ResString.GetMultilingualString("22e48536-3db9-4043-b699-1f523dcc20f4", "Global Sailing Schedules")); } }
			public static MultilingualString Freight_EBookings => CombineCategories(Categories.Freight_Bookings, ResString.GetMultilingualString("de5a12bd-52b0-4f05-b302-fdf05443e6a5", "Electronic Bookings"));
			public static MultilingualString Freight_HouseBills_HouseBillofLadingTypes { get { return CombineCategories(Freight_HouseBills, ResString.GetMultilingualString("4510e50c-1327-4b89-84be-a371725712fa", "House Bill of Lading Types")); } }
			public static MultilingualString Freight_GlobalTracking_RouteVisualizer { get { return CombineCategories(Freight_GlobalTracking, ResString.GetMultilingualString("aa267f7e-3937-40bf-be33-0cef97a52d0f", "Route Visualizer")); } }
			public static MultilingualString Freight_GlobalTracking_CargoTracker { get { return CombineCategories(Freight_GlobalTracking, ResString.GetMultilingualString("1e1483cb-ebd6-446a-91b9-7ff5e5707192", "Cargo Tracker")); } }
			public static MultilingualString Freight_GlobalTracking_MarketIntelligenceAndAnalytics { get { return CombineCategories(Freight_GlobalTracking, ResString.GetMultilingualString("9277c3e9-8cc9-4a06-9787-dee87bf6c5de", "Market Intelligence and Analytics")); } }
			public static MultilingualString Freight_SupplierBooking => CombineCategories(Freight, ResString.GetMultilingualString("ff0364db-df02-45b0-8f0a-353e32717b98", "Supplier Booking"));
			public static MultilingualString Freight_Contracts => CombineCategories(Freight, ResString.GetMultilingualString("ec9833a8-4d25-73b7-4a97-4ccd8a5370ab", "Contracts"));
			public static MultilingualString Freight_GreenhouseGasEmission { get { return CombineCategories(Freight, ResString.GetMultilingualString("171e0e0d-610e-4035-915e-e3638fc53455", "Greenhouse Gas Emissions")); } }
			public static MultilingualString Freight_Compliance => CombineCategories(Freight, ResString.GetMultilingualString("1E84AA4D-B252-469C-9793-4EA9F089CAC7", "Compliance"));
			public static MultilingualString Freight_AWB_AirlineMessaging { get { return CombineCategories(Freight_AWB, ResString.GetMultilingualString("677dea36-2f8d-4f02-b972-03d2582a7cbb", "Airline Messaging")); } }
		}

		#endregion

		#region Release Types

		public ReleaseTypesRegistryItem ReleaseTypes
		{
			get
			{
				return GetItem<ReleaseTypesRegistryItem>("ReleaseTypes", delegate
				{
					CodeDescriptionPairList defaultValues = new CodeDescriptionPairList(OLookUpEditType.ShipmentReleaseType);
					defaultValues.DefaultCode = defaultValues.Count != 0 ? defaultValues[0].Code : "";

					ReleaseTypes defaultTypes = new ReleaseTypes(defaultValues);
					defaultTypes.OriginalsNumber = 3;
					defaultTypes.CopiesNumber = 3;

					foreach (ReleaseType relType in defaultTypes.Types)
					{
						if (relType.Code == Constants.ShipmentReleaseTypes.ExpressBofL)
						{
							relType.OriginalsNumber = 0;
							relType.CopiesNumber = 1;
						}
						else if (relType.Code == Constants.ShipmentReleaseTypes.SeaWaybill)
						{
							relType.OriginalsNumber = 0;
							relType.CopiesNumber = 3;
						}
						else
						{
							relType.OriginalsNumber = 3;
							relType.CopiesNumber = 3;
						}
					}

					return new ReleaseTypesRegistryItem(
						"ReleaseTypes",
						Categories.Freight,
						ResString.GetMultilingualString("17d83b8e-a5f3-4039-805b-2e7ffcb180f9", "Release Types"),
						ResString.GetMultilingualString("8079ae8c-2c7b-4e12-ade7-a9c2cbb9f688", "This list defines the different Release Type options available on a shipment and consolidation. You can add your own items to this list but cannot change code and description of the system defined items."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						defaultTypes);
				});
			}
		}

		public CodePairRegistryItem ReleaseType
		{
			get
			{
				return GetItem<CodePairRegistryItem>("ReleaseType", delegate
				{
					return new CodePairRegistryItem(
						"ReleaseType",
						Categories.Freight_HouseBills,
						ResString.GetMultilingualString("ab76150d-32d7-41b2-8a62-b745ba0ae02d", "Release Type"),
						ResString.GetMultilingualString("e1d969a4-ac99-474b-ac4f-14ac1c273839", "Default Release Type for Shipments"),
						ReleaseTypes,
						true,
						true,
						null,
						RegistryStorageFlags.All,
						RegistryOptions.Default,
						"",
						false
					);
				});
			}
		}

		#endregion

		#region Outurn

		#region Outurn Responsible Party ID Override

		public OutturnRespPartyIDCodeDescriptionPairListRegistryItem OuturnResponsiblePartyIDOverride
		{
			get
			{
				return GetItem<OutturnRespPartyIDCodeDescriptionPairListRegistryItem>("OuturnResponsiblePartyIDOverride", delegate
				{
					return new OutturnRespPartyIDCodeDescriptionPairListRegistryItem(
						"OuturnResponsiblePartyIDOverride",
						Categories.Freight_CFS_Outturn,
						null,
						5,
						new CodeDescriptionPairListEditorInfo(true, true, CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, ResString.GetMultilingualString("ae5aeae0-60ab-474e-932a-4c6235fc2cd9", "CCP Code"), ResString.GetMultilingualString("375c047f-bc41-46cc-8e16-9793965354b0", "ABN Number")),
						RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						new CodeDescriptionPairList(),
						false,
						Categories.Freight_CFS_Outturn
						);
				});
			}
		}

		#endregion

		#endregion

		#region AWB

		public BooleanRegistryItem PrintTotalNumberOfPiecesOnShipmentAWBBarcodeLabel
		{
			get
			{
				return GetItem<BooleanRegistryItem>("PrintTotalNumberOfPiecesOnShipmentAWBBarcodeLabel", delegate
				{
					return new BooleanRegistryItem(
						"PrintTotalNumberOfPiecesOnShipmentAWBBarcodeLabel",
						Categories.Freight_AWB,
						ResString.GetMultilingualString("ccd2c747-ea23-4819-a26c-a4ae43e67f1d", "Print Total Number of Pieces on Shipment AWB Barcode Label"),
						ResString.GetMultilingualString("8cf66325-e346-4cca-800d-9f6addf8bff1", "For AWB Barcode Label printed from a shipment show 'Total Number of Pieces' only if this registry item is set to 'Yes'."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		public BooleanRegistryItem DefaultShipmentGoodsValueToHAWBAndDirectMAWB
		{
			get
			{
				return GetItem<BooleanRegistryItem>("DefaultShipmentGoodsValueToHAWBAndDirectMAWB", delegate
				{
					return new BooleanRegistryItem(
						"DefaultShipmentGoodsValueToHAWBAndDirectMAWB",
						Categories.Freight_AWB,
						ResString.GetMultilingualString("6992cd37-3113-41a0-b023-16e757d3a87b", "Default Shipment Goods Value to the HAWB and Direct MAWB"),
						ResString.GetMultilingualString("d7a97ef3-4620-476c-aae0-2c6c92bf99ca", "Specify whether the Shipment Goods Value should default to the HAWB and Direct MAWB."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem IssuedByDetailsUseShipmentOrigin
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IssuedByDetailsUseShipmentOrigin", delegate
				{
					return new BooleanRegistryItem(
						"IssuedByDetailsUseShipmentOrigin",
						Categories.Freight_AWB_HAWB,
						ResString.GetMultilingualString("98f509cc-7a5a-4c18-9d72-ad96fd85cad3", "Use shipment origin port to determine 'Issued By' details."),
						ResString.GetMultilingualString("e9d99000-fafa-47d4-a75d-00a271c08a2d", "If turned on, shipment origin port will be used to determine 'Issued By' details instead of using consol Flight 1 load port."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public BooleanRegistryItem SetHAWBCurrencyToFirstCollectInvoiceCurrency
		{
			get
			{
				return GetItem<BooleanRegistryItem>("SetHAWBCurrencyToFirstCollectInvoiceCurrency", delegate
				{
					return new BooleanRegistryItem(
						"SetHAWBCurrencyToFirstCollectInvoiceCurrency",
						Categories.Freight_AWB_HAWB,
						ResString.GetMultilingualString("bd987559-02c8-451f-9934-82fcade5c49b", "Use Currency of First Collect Freight Charge"),
						ResString.GetMultilingualString("c4c449e5-6ca2-4341-8303-a7606ceb983a", "If turned on, the HAWB Currency will default to the currency of the first collect freight accounting charge. The default behavior is to use the current company's currency."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem EnforceUniqueHAWBNumbers
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnforceUniqueHAWBNumbers", delegate
				{
					return new BooleanRegistryItem(
						"EnforceUniqueHAWBNumbers",
						Categories.Freight_AWB_HAWB,
						ResString.GetMultilingualString("6A91DCA3-5059-4E06-9F3B-A87A904EBE0D", "Enforce Unique HAWB Numbers"),
						ResString.GetMultilingualString("1F3900E0-7910-4B5E-9364-461D7DFDBD0F", @"When this registry setting is set to 'Yes' the system will ensure HAWB numbers are unique and allow you to regenerate house bill numbers using 'Regenerate House Bill Number' menu action even if HAWB number is read only.
Default setting is 'No', meaning the warning will be given on duplicate HAWB(and HBL) numbers."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}
		public BooleanRegistryItem SetHAWBCurrencyToFirstPrepaidInvoiceCurrency
		{
			get
			{
				return GetItem<BooleanRegistryItem>("SetHAWBCurrencyToFirstPrepaidInvoiceCurrency", delegate
				{
					return new BooleanRegistryItem(
						"SetHAWBCurrencyToFirstPrepaidInvoiceCurrency",
						Categories.Freight_AWB_HAWB,
						ResString.GetMultilingualString("66e4eb35-5758-42bf-8b28-cbcc3457870c", "Use Currency of First Prepaid Freight Charge"),
						ResString.GetMultilingualString("1e707750-ecd3-49a0-9d1b-9712c2c14567", "If turned on, the HAWB Currency will default to the currency of the first prepaid freight accounting charge. The default behavior is to use the current company's currency."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem ShowCollectOtherWarning
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ShowCollectOtherWarning", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"ShowCollectOtherWarning",
						Categories.Freight_AWB_MAWB,
						ResString.GetMultilingualString("4c957105-c5bc-4b76-a7c7-760418014975", "Show Collect Other Charge Warning"),
						ResString.GetMultilingualString("f4cae954-3d07-44e3-be33-b21c8355bb1c", "Turn this on to validate that if the MAWB Payment Method is Collect then at least 1 charge must be specified."),
						RegistryStorageFlags.All,
						false);
					return result;
				});
			}
		}

		public CodePairRegistryItem MAWBBillingSellRate
			=> GetItem("MAWBBillingSellRate", () => new CodePairRegistryItem(
				"MAWBBillingSellRate",
				Categories.Freight_AWB_MAWB,
				ResString.GetMultilingualString("08803870-3185-48c0-a6ea-1ce2b73afe8b", "Populate Billing Sell Rate on Direct Air Consolidations"),
				ResString.GetMultilingualString("81fc2912-aad4-4515-aa33-3be81d47a671", "Use this registry to define Sell Rates to display on the Master Air Waybill for collect and/or prepaid direct air consolidations instead of consol Chargeable Rate."),
				OLookUpEditType.MAWBBillingSellRateModes,
				RegistryStorageFlags.Company,
				DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
				Constants.AWB.MAWBBillingSellRateModes.None));

		public GuidArrayRegistryItem PrintHawbNumbersInBodyOfMawb
		{
			get
			{
				return GetItem<GuidArrayRegistryItem>("PRINT_HAWB_NUMBERS_IN_BODY_OF_MAWB", delegate
				{
					GuidArrayRegistryItem result = new GuidArrayRegistryItem(
						"PRINT_HAWB_NUMBERS_IN_BODY_OF_MAWB",
						Categories.Freight_AWB_MAWB,
						ResString.GetMultilingualString("f670af31-ef28-4c69-9464-a6d86be28dd6", "Print HAWB Numbers in Body of MAWB"),
						ResString.GetMultilingualString("a5802d73-2c19-41aa-94f8-0a718b2e3bdb", "The HAWB numbers will print in the description of the MAWB when the consignee is in any of the countries/regions listed below."),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						new Guid[] { Core.Constants.CountryGuids.Brazil });

					result.EditorInfo = new CountryListRegistryEditorInfo();
					return result;
				});
			}
		}

		public IntRegistryItem MAWBRecyclePeriod
		{
			get
			{
				return GetItem<IntRegistryItem>("MAWBRecyclePeriod", delegate
				{
					return new IntRegistryItem(
						"MAWBRecyclePeriod",
						Categories.Freight_AWB_MAWB,
						ResString.GetMultilingualString("77D11CB4-9BE3-4722-B37E-AA44B5ED04BA", "Allow re-use of MAWB number after (months)."),
						ResString.GetMultilingualString("5571E59D-B97D-4BC1-9C29-9BC5EB947648", "The number of months after which a MAWB number may be re-used on another shipment."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						12, 0, 120);
				});
			}
		}

		public IntRegistryItem MAWBDefaultLowStockLevel
		{
			get
			{
				return GetItem<IntRegistryItem>("MAWBDefaultLowStockLevel", delegate
				{
					return new IntRegistryItem(
						"MAWBDefaultLowStockLevel",
						Categories.Freight_AWB_MAWB,
						ResString.GetMultilingualString("adee5c0a-1dfb-c0a0-43fc-eafb91185963", "Default Low Stock Level"),
						ResString.GetMultilingualString("368dd221-a5ef-c489-4b9a-778268f3327c", "Sets the minimum default value to warn users of low MAWB stock levels. This will apply as the fallback warning value when there is no MAWB threshold configuration stored for the airline record that the CW1 branch is using."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						10, 1, 9999);
				});
			}
		}

		public BooleanRegistryItem AllocateMAWBNumberFromMAWBStockUponXMLImport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AllocateMAWBNumberFromMAWBStockUponXMLImport", delegate
				{
					return new BooleanRegistryItem(
						"AllocateMAWBNumberFromMAWBStockUponXMLImport",
						Categories.Freight_AWB_MAWB,
						ResString.GetMultilingualString("11AFBEB0-A12A-455D-9334-A7F9C3BB70C0", "Allocate MAWB number from MAWB Stock upon XML Import"),
						ResString.GetMultilingualString("4AD9DF8F-0D94-4F9E-B31A-7D40CF8C9BC0", "Enable this Registry setting to allow the system to auto-allocate MAWB Stock to a Consol on the import of Universal Shipment XML’s when only the MAWB Prefix is found inside the XML."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		public BooleanRegistryItem AllowMatchingOfMAWBsFromOtherBranches
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AllowMatchingOfMAWBsFromOtherBranches", delegate
				{
					return new BooleanRegistryItem(
						"AllowMatchingOfMAWBsFromOtherBranches",
						Categories.Freight_AWB_MAWB,
						ResString.GetMultilingualString("d421f39e-81ef-4fc1-8051-0c5172bd70b6", "Allow MAWB Stock from Company Branches."),
						ResString.GetMultilingualString("ce80ccd9-880e-435d-ac33-47af3a379cf2", "Allow The Use of MAWB Stock Allocated to Another Branch within the same Company."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public BooleanRegistryItem IncludeReprintStatusOnMAWBReprints
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IncludeReprintStatusOnMAWBReprints", delegate
				{
					return new BooleanRegistryItem(
						"IncludeReprintStatusOnMAWBReprints",
						Categories.Freight_AWB_MAWB,
						ResString.GetMultilingualString("e353ccdd-c477-483d-bbea-63980ce5060b", "Include Reprint text on Neutral MAWB"),
						ResString.GetMultilingualString("f03ba52c-4f68-4d09-b61a-fc6d760ac3b5", "Specifies whether the 'REPRINT' text will appear on Neutral MAWB documents that are being re-printed."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		public BooleanRegistryItem SendFWBNatureAndQuantityOfGoodsType
		{
			get
			{
				return GetItem<BooleanRegistryItem>("SendFWBNatureAndQuantityOfGoodsType", delegate
				{
					return new BooleanRegistryItem(
						"SendFWBNatureAndQuantityOfGoodsType",
						Categories.Freight_AWB_CargoIMP_FWBMessaging,
						ResString.GetMultilingualString("0ef44ee6-98d8-4620-bf92-a717ec85788d", "Send Nature and Quantity of Goods Type on FWB"),
						ResString.GetMultilingualString("31c95356-1657-433a-a3b6-9b960376fd34", "Specifies whether the 'Nature and Quantity of Goods Type' will be sent on the FWB"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		public AWBRoundingRegistryItem AWBRoundings
		{
			get
			{
				return GetItem<AWBRoundingRegistryItem>("AWBRoundings", delegate
				{
					return new AWBRoundingRegistryItem(
						"AWBRoundings",
						Categories.Freight_AWB,
						ResString.GetMultilingualString("9609a770-170b-4b43-afe3-e6047af7fb38", "Rounding Of Chargeable Weight"),
						ResString.GetMultilingualString("062159d0-97ea-4b98-bef5-2d51e43514cc", "Specify rounding options for Chargeable Weight for different types of Air Waybills."),
						RegistryStorageFlags.Company,
						AWBRoundingCollection.GetDefault());
				});
			}
		}

		public ApprovedExporterRegistryItem AWBApprovedExporterText
		{
			get
			{
				return GetItem<ApprovedExporterRegistryItem>("AWBApprovedExporterText", delegate
				{
					var item = new ApprovedExporterRegistryItem(
						"AWBApprovedExporterText",
						Categories.Freight_SupplyChainSecurity,
						ResString.GetMultilingualString("05cf1bbf-4539-4ac4-8dbc-b578f220631a", "Exporter Text - Approved"),
						ResString.GetMultilingualString("801ce465-c6b9-4857-aacd-0a13f79b42c3", "The text that will display on the AWB for Approved Exporter movements. To include this on other documents, you must reference the macro {0}.", "<InspectedShipmentText>"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						"SPX");

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.AWBCustomisableText);
					item.DataType = new EnglishOnlyStringRegistryDataType();

					return item;
				});
			}
		}

		public ApprovedExporterRegistryItem AWBNonApprovedExporterText
		{
			get
			{
				return GetItem<ApprovedExporterRegistryItem>("AWBNonApprovedExporterText", delegate
				{
					var item = new ApprovedExporterRegistryItem(
						"AWBNonApprovedExporterText",
						Categories.Freight_SupplyChainSecurity,
						ResString.GetMultilingualString("9c514259-32a8-4e94-999e-4f59d9f08b75", "Exporter Text - Non Approved"),
						ResString.GetMultilingualString("8c01522a-6b2e-4ad6-98f4-90353ad57ed1", "The text that will display on the AWB for Non-Approved Exporter movements. To include this on other documents, you must reference the macro {0}.", "<InspectedShipmentText>"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						(NoResString)"Not secured");

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.AWBCustomisableText);
					item.DataType = new EnglishOnlyStringRegistryDataType();

					return item;
				});
			}
		}

		public BooleanRegistryItem AllowAutoCalculationOfInternationalTax
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AllowAutoCalculationOfInternationalTax", delegate
				{
					return new BooleanRegistryItem("AllowAutoCalculationOfInternationalTax",
						Categories.Freight_AWB,
						ResString.GetMultilingualString("b3fcddf9-6566-4692-837b-23169040ba6d", @"Allow Auto Calculation of Tax for International AWBs"),
						ResString.GetMultilingualString("d9c46ec2-1aea-45f2-9a34-38989c93cc51", "This determines whether tax is automatically calculated for International AWBs."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch, false);
				});
			}
		}

		public class ApprovedExporterRegistryItem : RegistryItemImpl
		{
			public ApprovedExporterRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storageFlag, string prefix)
				: base(name, category, caption, hint, RegistryDataTypes.StringType, storageFlag)
			{
				this.Prefix = prefix;
			}

			readonly string Prefix;

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				using (var command = Db.Connection.Command("select RN_Code from dbo.RefCountry join dbo.GlbCompany on GC_RN_NKCountryCode = RN_Code where GC_PK = @Code"))
				{
					command.AddParameterBasedOnDbColumn("@Code", companyPK, GlbCompanySchema.PK);
					var countryCode = (string)command.ExecuteScalar();
					if (countryCode == Core.Constants.CountryCodes.Germany)
					{
						return string.Format((NoResString)"{0}; <CustomsCode(<BranchProxy>, DE, GCA)>; <BranchProxyName>", Prefix);
					}
					else
					{
						return base.GetDefaultValueCore(companyPK, branchPK, departmentPK);
					}
				}
			}
		}

		public AWBLabelCustomisationRegistryItem AWBBarcodeLabelCustomisation
		{
			get
			{
				return GetItem<AWBLabelCustomisationRegistryItem>("AWBBarcodeLabelCustomisation", delegate
				{
					return new AWBLabelCustomisationRegistryItem(
						"AWBBarcodeLabelCustomisation",
						Categories.Freight_AWB,
						ResString.GetMultilingualString("a5810750-e819-41d3-82ed-6b58b1d30840", "AWB Barcode Label Customization"),
						ResString.GetMultilingualString("34b13d2b-4a4e-4354-8a04-e0bec2994475", "Allows the user to customize the layout of the ‘Optional Information’ section on the IATA 606 AWB Barcode label."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AWBLabelCustomisation.GetDefault());
				});
			}
		}

		public StringRegistryItem HAWBHandlingInformationExtraText
		{
			get
			{
				return GetItem<StringRegistryItem>("HAWBHandlingInformationExtraText", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"HAWBHandlingInformationExtraText",
						Categories.Freight_AWB_HAWB_CustomizableExtraText,
						ResString.GetMultilingualString("5446ce88-049d-4b25-9a5a-3a4e069a1706", "Handling Information Extra Text"),
						ResString.GetMultilingualString("0b58a624-f21b-4a61-a78f-3475a279fe46", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text."),
						RegistryStorageFlags.All);

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.AWBCustomisableText);
					item.DataType = new EnglishOnlyStringRegistryDataType();

					return item;
				});
			}
		}

		public StringRegistryItem HAWBNatureAndQtyOfGoodsExtraText
		{
			get
			{
				return GetItem<StringRegistryItem>("HAWBNatureAndQtyOfGoodsExtraText", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"HAWBNatureAndQtyOfGoodsExtraText",
						Categories.Freight_AWB_HAWB_CustomizableExtraText,
						ResString.GetMultilingualString("5682a977-f8ae-42e1-9361-7a64f58f7293", "Nature and Quantity of Goods Extra Text"),
						ResString.GetMultilingualString("41348d4a-39a7-44be-8f3b-e15dafa8d9a7", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch);

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.AWBCustomisableText);
					item.DataType = new EnglishOnlyStringRegistryDataType();

					return item;
				});
			}
		}

		public StringRegistryItem AWBSecurityStatementTextToUSAAndUSTerritories
		{
			get
			{
				return GetItem<StringRegistryItem>("AWBSecurityStatementTextToUSAAndUSTerritories", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"AWBSecurityStatementTextToUSAAndUSTerritories",
						Categories.Freight_AWB_SecurityStatementToUSA,
						ResString.GetMultilingualString("B07F5A2F-B8C8-44F3-ACC9-99F32844EDFA", "Statement Text"),
						ResString.GetMultilingualString("693DB553-104C-40AA-AEB3-C583A12D1484",
							"Security statement confirming that goods to or via the USA and its overseas territories have not originated from, transferred from, or transited through any country listed under the registry: Freight > AWB > Security Statement to the USA > List of Countries of Origin or Transshipment.\r\n\r\nThis statement is included in the eAWB message and shown on the Security Declaration."),
						RegistryStorageFlags.System,
						(NoResString)"<CompanyName> has reviewed all available documentation and has determined that none of the cargo being offered in this consignment or consolidation has/either originated in, transferred from, or transited through any point in <TSASecurityStatementCountriesText>.");

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.AWBCustomisableText);
					item.DataType = new EnglishOnlyStringRegistryDataType();

					return item;
				});
			}
		}

		public BooleanRegistryItem MessagingViaPelicanServer
		{
			get
			{
				return GetItem("MessagingViaPelicanServer", delegate
				{
					return new BooleanRegistryItem(
						"MessagingViaPelicanServer",
						Categories.Freight_AWB_AirlineMessaging,
						ResString.GetMultilingualString("159f3e13-adf3-4b70-a7c4-66621efbb56f", "Messaging via API"),
						ResString.GetMultilingualString("d4af92ca-d786-448b-93ba-6aacc88aaa2f", "Specifies whether airline messages will are sent via airline messaging API gateway."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public StringRegistryItem PelicanApiUrl
		{
			get
			{
				return GetItem("PelicanApiUrl", delegate
				{
					var item = new StringRegistryItem(
						"PelicanApiUrl",
						Categories.Freight_AWB_AirlineMessaging,
						(NoResString)"API URL",
						(NoResString)"Specifies the API gateway URL used for sending airline messages.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						(NoResString)"https://airmessaging.wisegrid.net/");
					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Url);
					return item;
				});
			}
		}

		public IntRegistryItem PelicanApiTimeout
		{
			get
			{
				return GetItem("PelicanApiTimeout", delegate
				{
					return new IntRegistryItem(
						"PelicanApiTimeout",
						Categories.Freight_AWB_AirlineMessaging,
						ResString.GetMultilingualString("b4e4fbb4-cf48-4bbe-9a93-cd0961415e1e", "API Request Timeout"),
						ResString.GetMultilingualString("67e937af-5c38-4f35-bb46-e5653869aa2c", "Specifies the timeout (in seconds) when the airline messaging API gateway is requested or called."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						30,
						1,
						300);
				});
			}
		}

		public IntRegistryItem PelicanApiRetryAttempts
		{
			get
			{
				return GetItem("PelicanApiRetryAttempts", delegate
				{
					return new IntRegistryItem(
						"PelicanApiRetryAttempts",
						Categories.Freight_AWB_AirlineMessaging,
						ResString.GetMultilingualString("1bb1ac2b-584c-4e3b-9da8-081d85a6425b", "API Request Retry Attempts"),
						ResString.GetMultilingualString("b76d0add-7b1e-4170-b9bf-c70a24d54c3f", "Specifies the maximum number of failed request attempts to the airline messaging API gateway before an official error is returned."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						3,
						0,
						10);
				});
			}
		}

		public CargoImpVersionRegistryItem AirlineMessagingCargoImpVersion
		{
			get
			{
				return GetItem("AirlineMessagingCargoImpVersion", delegate
				{
					return new CargoImpVersionRegistryItem(
						"AirlineMessagingCargoImpVersion",
						Categories.Freight_AWB_AirlineMessaging,
						ResString.GetMultilingualString("62a398ed-a108-4744-87da-cbcc4ca85ee9", "CargoIMP Version"),
						ResString.GetMultilingualString("1f23bfe5-1a85-43f2-8557-5703267b9c95", "Use this registry to set the FWB/FHL version sent to specific airlines based on their AWB prefix. V16 (FWB/16 and FHL/4) is the default message version setting for all airlines.\r\n\r\nPlease ensure you have received confirmation from the airline regarding their FWB/FHL message version capabilities before changing the default setting to avoid any message rejections."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default);
				});
			}
		}

		public GuidArrayRegistryItem AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement
		{
			get
			{
				return GetItem<GuidArrayRegistryItem>("AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement", delegate
				{
					var result = new GuidArrayRegistryItem(
						"AWBCountriesOfOriginOrTranshipmentInUSAAndUSTerritoriesSecurityStatement",
						Categories.Freight_AWB_SecurityStatementToUSA,
						ResString.GetMultilingualString("2A7F5B5B-4EAB-491A-8D4B-C85B7F658324", "List of Countries/Regions of Origin or Transhipment"),
						ResString.GetMultilingualString("BFAE9142-2237-4F4C-AB80-91516EA088F2", "List of countries/regions to be included in the Security Statement, confirming that goods to or via the USA have not originated from, transferred from, or transited through any of the countries/regions listed below.\r\n\r\nThis statement is included in the eAWB message and shown on the Security Declaration."),
						RegistryStorageFlags.System,
						new Guid[] { Constants.CountryGuids.Egypt, Constants.CountryGuids.Somalia, Constants.CountryGuids.SyrianArabRepublic, Constants.CountryGuids.Yemen });

					result.EditorInfo = new CountryListRegistryEditorInfo();
					return result;
				});
			}
		}

		public StringRegistryItem HAWBOptionalShippingInfoOneExtraText
		{
			get
			{
				return GetItem<StringRegistryItem>("HAWBOptionalShippingInfoOneExtraText", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"HAWBOptionalShippingInfoOneExtraText",
						Categories.Freight_AWB_HAWB_CustomizableExtraText,
						ResString.GetMultilingualString("3d4bc9aa-0b40-4d52-a2f7-6758874df29a", "Optional Shipping Info 1 Extra Text"),
						ResString.GetMultilingualString("cbc0536f-efa4-4276-a434-2c286beec97e", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						"TERMS: <INCO>");

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.AWBCustomisableText);
					item.DataType = new EnglishOnlyStringRegistryDataType();

					return item;
				});
			}
		}

		public StringRegistryItem HAWBOptionalShippingInfoTwoExtraText
		{
			get
			{
				return GetItem<StringRegistryItem>("HAWBOptionalShippingInfoTwoExtraText", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"HAWBOptionalShippingInfoTwoExtraText",
						Categories.Freight_AWB_HAWB_CustomizableExtraText,
						ResString.GetMultilingualString("9543ed80-6e64-4a83-b79e-48afe66060eb", "Optional Shipping Info 2 Extra Text"),
						ResString.GetMultilingualString("b8abde41-a700-46d6-9000-7c92a982c31a", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch);

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.AWBCustomisableText);
					item.DataType = new EnglishOnlyStringRegistryDataType();

					return item;
				});
			}
		}

		#region AWBExtraTextGrid

		public CodeDescriptionPairListRegistryItem HAWBAccountingInfoExtraText
		{
			get
			{
				return GetItem("HAWBAccountingInfoExtraText", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"HAWBAccountingInfoExtraText",
						Categories.Freight_AWB_HAWB_CustomizableExtraText,
						AccountingInfoExtraTextCaption,
						AccountingInfoExtraTextHint,
						256,
						new AWBGridRegistryEditorInfo(),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false,
						RegistryOptions.Default,
						new ReadOnlyCodeDescriptionPairList(),
						false);
				});
			}
		}

		public class AWBGridRegistryEditorInfo : CodeDescriptionPairListEditorInfo { }

		ResourceString AccountingInfoExtraTextHint
		{
			get { return ResString.GetMultilingualString("0b9e3585-d407-4268-8e5f-0398d2b05be1", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text. To insert a macro, please place the cursor inside the cell in the grid and press the 'Insert Field' button."); }
		}

		ResourceString AccountingInfoExtraTextCaption
		{
			get { return ResString.GetMultilingualString("284d8498-5f60-4682-9049-9cdc25119bc0", "Accounting Info Extra Text"); }
		}

		public CodeDescriptionPairListRegistryItem MAWBAccountingInfoExtraText
		{
			get
			{
				return GetItem("MAWBAccountingInfoExtraText", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"MAWBAccountingInfoExtraText",
						Categories.Freight_AWB_MAWB_CustomizableExtraText,
						AccountingInfoExtraTextCaption,
						AccountingInfoExtraTextHint,
						256,
						new AWBGridRegistryEditorInfo(),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false,
						RegistryOptions.Default,
						new ReadOnlyCodeDescriptionPairList(),
						false);
				});
			}
		}

		#endregion

		public StringRegistryItem MAWBHandlingInformationExtraText
		{
			get
			{
				return GetItem<StringRegistryItem>("MAWBHandlingInformationExtraText", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"MAWBHandlingInformationExtraText",
						Categories.Freight_AWB_MAWB_CustomizableExtraText,
						ResString.GetMultilingualString("daed5f39-db2e-4d65-83d8-6b1eabba5f83", "Handling Information Extra Text"),
						ResString.GetMultilingualString("c9e758ef-4233-4750-9bbe-c64ed23da0e7", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text."),
						RegistryStorageFlags.All,
						"");

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.AWBCustomisableText);
					item.DataType = new EnglishOnlyStringRegistryDataType();

					return item;
				});
			}
		}

		public StringRegistryItem MAWBNatureAndQtyOfGoodsExtraText
		{
			get
			{
				return GetItem<StringRegistryItem>("MAWBNatureAndQtyOfGoodsExtraText", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"MAWBNatureAndQtyOfGoodsExtraText",
						Categories.Freight_AWB_MAWB_CustomizableExtraText,
						ResString.GetMultilingualString("6df10c77-fdb5-4d24-b853-a8cb5bd46ec5", "Nature and Quantity of Goods Extra Text"),
						ResString.GetMultilingualString("a42c2cf6-0ef2-47f2-8515-b600108612ea", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						"");

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.AWBCustomisableText);
					item.DataType = new EnglishOnlyStringRegistryDataType();

					return item;
				});
			}
		}

		public StringRegistryItem MAWBOptionalShippingInfoOneExtraText
		{
			get
			{
				return GetItem<StringRegistryItem>("MAWBOptionalShippingInfoOneExtraText", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"MAWBOptionalShippingInfoOneExtraText",
						Categories.Freight_AWB_MAWB_CustomizableExtraText,
						ResString.GetMultilingualString("89ebf856-b5b6-4b3c-8c92-4f546b9c853a", "Optional Shipping Info 1 Extra Text"),
						ResString.GetMultilingualString("e15343d8-08e4-44e6-85b4-74d6b6fd2d6f", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						"");

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.AWBCustomisableText);
					item.DataType = new EnglishOnlyStringRegistryDataType();

					return item;
				});
			}
		}

		public StringRegistryItem MAWBOptionalShippingInfoTwoExtraText
		{
			get
			{
				return GetItem<StringRegistryItem>("MAWBOptionalShippingInfoTwoExtraText", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"MAWBOptionalShippingInfoTwoExtraText",
						Categories.Freight_AWB_MAWB_CustomizableExtraText,
						ResString.GetMultilingualString("d16065ad-a89c-4988-8552-5aee97fbb675", "Optional Shipping Info 2 Extra Text"),
						ResString.GetMultilingualString("0a9bf221-197e-4d4b-aa42-9d3e9bf0a6c6", "Text specified here will appear on AWB documentation. You can use macros such as <Now> to allow for replaceable text."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						"");

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.AWBCustomisableText);
					item.DataType = new EnglishOnlyStringRegistryDataType();

					return item;
				});
			}
		}

		public BooleanRegistryItem MAWBSuppressULDTareWeight
		{
			get
			{
				return GetItem<BooleanRegistryItem>("MAWBSuppressULDTareWeight", delegate
				{
					return new BooleanRegistryItem(
						"MAWBSuppressULDTareWeight",
						Categories.Freight_AWB_MAWB,
						ResString.GetMultilingualString("82733311-3A3F-4B6A-A4D5-9A5CF1582974", "Suppress ULD Tare Weight on MAWB"),
						ResString.GetMultilingualString("FBF69E9C-CC57-4948-A92C-C12502F41AA4", "When enabled, show Gross Weight as 0 for the ULD Rate Class X.\r\n\r\nChanging this registry can cause non-compliant MAWB messages to be sent, please use with caution."),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public SummarizeULDSLACConfigRegistryItem SummarizeULDSLACs
		{
			get
			{
				return GetItem<SummarizeULDSLACConfigRegistryItem>("SummarizeULDSLACConfig", delegate
				{
					return new SummarizeULDSLACConfigRegistryItem(
						"SummarizeULDSLACConfig",
						Categories.Freight_AWB_MAWB,
						ResString.GetMultilingualString("cc353cda-c0f0-4cd9-9a5d-e4b75b1cb847", "Summarize ULD SLAC (Shipper Load and Count)"),
						ResString.GetMultilingualString("9b1abc13-9771-48cd-9713-f76429bff6ec", @"Use this Registry to configure for each country of destination whether SLAC (Shipper’s Load and Count) should be totaled for display in the Master Air Waybill regardless of the number of ULD(s) on the Consolidation. Tick the box for ‘Use Shipment Inners’ for MAWB SLAC to display the total of Shipment inner packs (as per LSE Consol) instead of outer packs (per ULD Consol)."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default);
				});
			}
		}

		#region Terms & Conditions

		#region HAWB

		public StringRegistryItem HAWBTermsAndConditionsHeading
		{
			get
			{
				return GetItem<StringRegistryItem>("HAWBTermsAndConditionsHeading", delegate
				{
					return new StringRegistryItem(
						"HAWBTermsAndConditionsHeading",
						Categories.Freight_AWB_HAWB_TermsandConditions,
						ResString.GetMultilingualString("64a3b0ff-f33a-4efc-84a1-7618087cf622", "Heading"),
						ResString.GetMultilingualString("92060eca-1aa7-404a-8e6e-e044186b6f0f", "Terms & Conditions Heading Text"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AWBTermsAndConditionsText.Heading);
				});
			}
		}

		public StringRegistryItem HAWBTermsAndConditionsIntroduction
		{
			get
			{
				return GetItem<StringRegistryItem>("HAWBTermsAndConditionsIntroduction", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"HAWBTermsAndConditionsIntroduction",
						Categories.Freight_AWB_HAWB_TermsandConditions,
						ResString.GetMultilingualString("53a9190a-614e-47ed-a987-16c745746953", "Introduction"),
						ResString.GetMultilingualString("f430537b-aaeb-432f-bbe9-44ccb6a130f2", "Terms & Conditions Introduction Text"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AWBTermsAndConditionsText.Introduction);

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return item;
				});
			}
		}

		public StringRegistryItem HAWBTermsAndConditionsMiddleHeading
		{
			get
			{
				return GetItem<StringRegistryItem>("HAWBTermsAndConditionsMiddleHeading", delegate
				{
					return new StringRegistryItem(
						"HAWBTermsAndConditionsMiddleHeading",
						Categories.Freight_AWB_HAWB_TermsandConditions,
						ResString.GetMultilingualString("ef3c148b-668f-4ed4-954b-6d712ba79043", "Middle Heading"),
						ResString.GetMultilingualString("56f869ed-6dbc-4bb5-881d-3f8f7b32e562", "Terms & Conditions Middle Heading Text"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AWBTermsAndConditionsText.MiddleHeading);
				});
			}
		}

		public StringRegistryItem HAWBTermsAndConditionsBody1
		{
			get
			{
				return GetItem<StringRegistryItem>("HAWBTermsAndConditionsBody1", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"HAWBTermsAndConditionsBody1",
						Categories.Freight_AWB_HAWB_TermsandConditions,
						ResString.GetMultilingualString("e456542e-43b0-4bb3-b834-a9ef8d343429", "Body 1"),
						ResString.GetMultilingualString("04f23289-1c13-42a5-a55d-9e56c6fdb130", "Terms & Conditions Body Text 1"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AWBTermsAndConditionsText.Body1);

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return item;
				});
			}
		}

		public StringRegistryItem HAWBTermsAndConditionsBody2
		{
			get
			{
				return GetItem<StringRegistryItem>("HAWBTermsAndConditionsBody2", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"HAWBTermsAndConditionsBody2",
						Categories.Freight_AWB_HAWB_TermsandConditions,
						ResString.GetMultilingualString("c5631683-29a8-4fcd-9e32-eef75d7ad5ff", "Body 2"),
						ResString.GetMultilingualString("a568c42e-ff58-4902-97e5-aef95b7a52c0", "Terms & Conditions Body Text 2"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AWBTermsAndConditionsText.Body2);

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return item;
				});
			}
		}

		#endregion

		#region MAWB

		#region SuppressResourceStringsCheckRegion

		public StringRegistryItem MAWBTermsAndConditionsHeading
		{
			get
			{
				return GetItem<StringRegistryItem>("MAWBTermsAndConditionsHeading", delegate
				{
					return new StringRegistryItem(
						"MAWBTermsAndConditionsHeading",
						Categories.Freight_AWB_MAWB_TermsandConditions,
						(NoResString)"Heading",
						(NoResString)"Terms & Conditions Heading Text",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsHidden,
						AWBTermsAndConditionsText.Heading);
				});
			}
		}

		public StringRegistryItem MAWBTermsAndConditionsIntroduction
		{
			get
			{
				return GetItem<StringRegistryItem>("MAWBTermsAndConditionsIntroduction", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"MAWBTermsAndConditionsIntroduction",
						Categories.Freight_AWB_MAWB_TermsandConditions,
						(NoResString)"Introduction",
						(NoResString)"Terms & Conditions Introduction Text",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsHidden,
						AWBTermsAndConditionsText.Introduction);

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return item;
				});
			}
		}

		public StringRegistryItem MAWBTermsAndConditionsMiddleHeading
		{
			get
			{
				return GetItem<StringRegistryItem>("MAWBTermsAndConditionsMiddleHeading", delegate
				{
					return new StringRegistryItem(
						"MAWBTermsAndConditionsMiddleHeading",
						Categories.Freight_AWB_MAWB_TermsandConditions,
						(NoResString)"Middle Heading",
						(NoResString)"Terms & Conditions Middle Heading Text",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsHidden,
						AWBTermsAndConditionsText.MiddleHeading);
				});
			}
		}

		public StringRegistryItem MAWBTermsAndConditionsBody1
		{
			get
			{
				return GetItem<StringRegistryItem>("MAWBTermsAndConditionsBody1", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"MAWBTermsAndConditionsBody1",
						Categories.Freight_AWB_MAWB_TermsandConditions,
						(NoResString)"Body 1",
						(NoResString)"Terms & Conditions Body Text 1",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsHidden,
						AWBTermsAndConditionsText.Body1);

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return item;
				});
			}
		}

		public StringRegistryItem MAWBTermsAndConditionsBody2
		{
			get
			{
				return GetItem<StringRegistryItem>("MAWBTermsAndConditionsBody2", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"MAWBTermsAndConditionsBody2",
						Categories.Freight_AWB_MAWB_TermsandConditions,
						(NoResString)"Body 2",
						(NoResString)"Terms & Conditions Body Text 2",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsHidden,
						AWBTermsAndConditionsText.Body2);

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return item;
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#region CommunityRegionsForDirectionCalculation

		public GuidArrayRegistryItem CommunityRegionsForDirectionCalculation
		{
			get
			{
				return GetItem("CommunityRegionsForDirectionCalculation", delegate
				{
					var result = new GuidArrayRegistryItem("CommunityRegionsForDirectionCalculation",
							Categories.Freight,
							ResString.GetMultilingualString("1574277f-5be9-49e8-a4f5-cc8e832aa992", "Community Region for Direction Calculation Purposes"),
							ResString.GetMultilingualString("f7d204c8-c86c-4fe6-ae03-a19ae9511097", "Use this registry to specify which countries/regions in your company’s country/region should be treated as local to this region when trading with the rest of the world for job direction calculation purposes.\r\n\r\nAny jobs originated in any country/region listed here including your home country/region with the destination outside of this region will be treated as export jobs, and any jobs originated outside of this group with a destination inside of this region including your home country/region will be treated as import jobs.\r\n\r\nThe registry settings will allow you to override the default cross-trade calculation (when jobs originated by a logged in country/region with origin or destination outside of this country/region are treated as cross-trade ) which in turn will take an effect on Shipment and Bill Number generation (when job direction is used in number generating algorithms), on calculation of local client and overseas agent, department and revenue recognition date."),
							RegistryStorageFlags.Company);

					result.EditorInfo = new CountryListRegistryEditorInfo();
					return result;
				});
			}
		}

		#endregion

		#region Freight Chargeable Weight Roundings

		public ChargeableWeightRoundingRegistryItem FreightChargeableWeightRoundings
		{
			get
			{
				return GetItem<ChargeableWeightRoundingRegistryItem>("FreightChargeableWeightRoundings", delegate
				{
					return new ChargeableWeightRoundingRegistryItem(
						"FreightChargeableWeightRoundings",
						Categories.Freight,
						ResString.GetMultilingualString("2fb9d035-2a75-4c46-943c-d3f024275e20", "Rounding Of Chargeable Weight - Air"),
						ResString.GetMultilingualString("bac4aec7-3137-4c91-9bdb-c5b24d307dc2", "Specify rounding options for Chargeable Weight for Air freight"),
						RegistryStorageFlags.Company,
						GetDefaultFreightChargeableWeightRoundings());
				});
			}
		}

		static ChargeableWeightRoundingCollection GetDefaultFreightChargeableWeightRoundings()
		{
			ChargeableWeightRoundingCollection result = new ChargeableWeightRoundingCollection();

			ChargeableWeightRounding rounding = result.AddNew();
			rounding.RoundingMode = nameof(ChargeableWeightRoundingType.None);
			rounding.RoundingScale = ChargeableWeightRoundingScales.DefaultScale;
			return result;
		}

		#endregion

		#region House Bills

		#region EnableHouseBillOfLadingRegistryItems

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem EnableHouseBillOfLadingRegistryItems
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableHouseBillOfLadingRegistryItems", delegate
				{
					return new BooleanRegistryItem(
						"EnableHouseBillOfLadingRegistryItems",
						Categories.Freight_HouseBills_HouseBillofLadingTypes,
						(NoResString)"Enable House Bill of Lading Registry Items",
						(NoResString)"By default the House Bill of Lading Registry Items are visible in the Registry. Change this setting to NO to make them hidden.\r\nThis is a Support only registry setting.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		#endregion

		public CodeDescriptionPairListWithDefaultCodeRegistryItem HouseBillOfLadingTypesForSea
		{
			get
			{
				return GetItem("HouseBillOfLadingTypes", delegate
				{
					var houseBillOfLadingTypesList = new CodeDescriptionPairList();
					houseBillOfLadingTypesList.AddPair(Constants.HouseBillOfLadingTypes.Code.ITClubAustralia, Constants.HouseBillOfLadingTypes.Description.ITClubAustralia);
					houseBillOfLadingTypesList.AddPair(Constants.HouseBillOfLadingTypes.Code.ITClubAustraliaPreprinted, Constants.HouseBillOfLadingTypes.Description.ITClubAustraliaPreprinted);
					houseBillOfLadingTypesList.AddPair(Constants.HouseBillOfLadingTypes.Code.ITClubNewZealand, Constants.HouseBillOfLadingTypes.Description.ITClubNewZealand);
					houseBillOfLadingTypesList.AddPair(Constants.HouseBillOfLadingTypes.Code.ITClubNewZealandPreprinted, Constants.HouseBillOfLadingTypes.Description.ITClubNewZealandPreprinted);
					houseBillOfLadingTypesList.AddPair(Constants.HouseBillOfLadingTypes.Code.TTClubAustraliaNZ, Constants.HouseBillOfLadingTypes.Description.TTClubAustraliaNZ);
					houseBillOfLadingTypesList.AddPair(Constants.HouseBillOfLadingTypes.Code.TTClubAustraliaNZPreprinted, Constants.HouseBillOfLadingTypes.Description.TTClubAustraliaNZPreprinted);
					houseBillOfLadingTypesList.AddPair(Constants.HouseBillOfLadingTypes.Code.FIATAHBL, Constants.HouseBillOfLadingTypes.Description.FIATAHBL);
					houseBillOfLadingTypesList.AddPair(Constants.HouseBillOfLadingTypes.Code.FIATAHBLPreprinted, Constants.HouseBillOfLadingTypes.Description.FIATAHBLPreprinted);
					houseBillOfLadingTypesList.AddPair(Constants.HouseBillOfLadingTypes.Code.TANHBL, Constants.HouseBillOfLadingTypes.Description.TANHBL);
					houseBillOfLadingTypesList.AddPair(Constants.HouseBillOfLadingTypes.Code.TANHBLPreprinted, Constants.HouseBillOfLadingTypes.Description.TANHBLPreprinted);
					houseBillOfLadingTypesList.AddPair(Constants.HouseBillOfLadingTypes.Code.CargowiseBill, Constants.HouseBillOfLadingTypes.Description.CargowiseBill);
					houseBillOfLadingTypesList.AddPair(Constants.HouseBillOfLadingTypes.Code.CargowiseBillPreprinted, Constants.HouseBillOfLadingTypes.Description.CargowiseBillPreprinted);
					houseBillOfLadingTypesList.AddPair(Constants.HouseBillOfLadingTypes.Code.DataHawkBill, Constants.HouseBillOfLadingTypes.Description.DataHawkBill);
					houseBillOfLadingTypesList.AddPair(Constants.HouseBillOfLadingTypes.Code.TTClubUnitedStates, Constants.HouseBillOfLadingTypes.Description.TTClubUnitedStates);
					houseBillOfLadingTypesList.AddPair(Constants.HouseBillOfLadingTypes.Code.TTClubUnitedStatesPreprinted, Constants.HouseBillOfLadingTypes.Description.TTClubUnitedStatesPreprinted);

					return new CodeDescriptionPairListWithDefaultCodeRegistryItem("HouseBillOfLadingTypes",
						Categories.Freight_HouseBills_HouseBillofLadingTypes,
						ResString.GetMultilingualString("4CE3013D-26F1-45A4-BC39-28F8016FF1E6", "For Sea"), null, RegistryStorageFlags.Company | RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						houseBillOfLadingTypesList, true, true, 3, false);
				});
			}
		}

		public CodeDescriptionPairListWithDefaultCodeRegistryItem HouseBillOfLadingTypesForRoad
		{
			get
			{
				return GetItem("HouseBillOfLadingTypesForRoad", delegate
				{
					var houseBillOfLadingTypesForRailAndRoadList = new CodeDescriptionPairList();
					houseBillOfLadingTypesForRailAndRoadList.AddPair(Constants.HouseBillOfLadingTypes.Code.FIATAHBL, Constants.HouseBillOfLadingTypes.Description.FIATAHBL);
					houseBillOfLadingTypesForRailAndRoadList.AddPair(Constants.HouseBillOfLadingTypes.Code.CargowiseBill, Constants.HouseBillOfLadingTypes.Description.CargowiseBill);
					houseBillOfLadingTypesForRailAndRoadList.AddPair(Constants.HouseBillOfLadingTypes.Code.CartaPorteSpanish, Constants.HouseBillOfLadingTypes.Description.CartaPorteSpanish);

					return new CodeDescriptionPairListWithDefaultCodeRegistryItem("HouseBillOfLadingTypesForRoad",
						Categories.Freight_HouseBills_HouseBillofLadingTypes,
						ResString.GetMultilingualString("E37A77A7-0C4C-4613-9F54-7662AB5A9690", "For Road"), null, RegistryStorageFlags.Company | RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						houseBillOfLadingTypesForRailAndRoadList, true, true, 3, false);
				});
			}
		}

		public CodeDescriptionPairListWithDefaultCodeRegistryItem HouseBillOfLadingTypesForRail
		{
			get
			{
				return GetItem("HouseBillOfLadingTypesForRail", delegate
				{
					var houseBillOfLadingTypesForRailAndRoadList = new CodeDescriptionPairList();
					houseBillOfLadingTypesForRailAndRoadList.AddPair(Constants.HouseBillOfLadingTypes.Code.FIATAHBL, Constants.HouseBillOfLadingTypes.Description.FIATAHBL);
					houseBillOfLadingTypesForRailAndRoadList.AddPair(Constants.HouseBillOfLadingTypes.Code.CargowiseBill, Constants.HouseBillOfLadingTypes.Description.CargowiseBill);

					return new CodeDescriptionPairListWithDefaultCodeRegistryItem("HouseBillOfLadingTypesForRail",
						Categories.Freight_HouseBills_HouseBillofLadingTypes,
						ResString.GetMultilingualString("ED30480D-3177-4937-B1A4-8FAB2D273CB6", "For Rail"), null, RegistryStorageFlags.Company | RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						houseBillOfLadingTypesForRailAndRoadList, true, true, 3, false);
				});
			}
		}

		#endregion

		#region PrintOuterBreakdownPackingDetailsOnBOL

		public BooleanRegistryItem ShowPackLineDetailsOnHouseBills
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ShowPackLineDetailsOnHouseBills", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"ShowPackLineDetailsOnHouseBills",
						Categories.Freight_HouseBills,
						ResString.GetMultilingualString("72fee50f-9a4a-48ad-ac25-d5246b22e4d8", "Show Pack Line Details On House Bills"),
						ResString.GetMultilingualString("8a198cf0-3165-4340-8b53-45336f266af4", "Enable this option to show the breakdown of Outer Packline details for each container on House Bills."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
					if (!EnableHouseBillOfLadingRegistryItems.Value)
					{
						result.Options |= RegistryOptions.IsHidden;
					}
					return result;
				});
			}
		}

		#endregion

		#region BOLSendingForwarder

		public BooleanRegistryItem BOLSendingForwarderCurrentBranchOrgProxy
		{
			get
			{
				return GetItem<BooleanRegistryItem>("BOLSendingForwarderCurrentBranchOrgProxy", delegate
				{
					return new BooleanRegistryItem(
						"BOLSendingForwarderCurrentBranchOrgProxy",
						Categories.Freight_HouseBills,
						ResString.GetMultilingualString("d0def910-d8A5-4bd7-a5d2-a7127ed43c9d", "Set Bill of Lading Sending Forwarder"),
						ResString.GetMultilingualString("627b614f-97c5-42bd-8d96-5aaa7b4a4899", "This registry item controls whether the Sending Forwarder displayed on Bill of Lading is the consol's sending agent or current branch's organization proxy.\r\n\r\nSelect 'Yes' - The Current Branch's organization proxy will be used.\r\n\r\nSelect 'No' - The consol's sending agent will be used."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region BOLClause

		public MultilingualStringRegistryItem BOLClause
		{
			get
			{
				ResourceString defaultValue = (Env.CurrentCompany?.Country?.Code == Constants.CountryCodes.UnitedStates && ZDateTime.Now < USDestinationControlStatementEffectiveDate) ? ResString.GetMultilingualString("2245f534-b9b4-4be3-8b16-07f85790673b", "These commodities, technology or software were exported from the United States in accordance with the Export Administration Regulations. Diversion contrary to U.S. law is prohibited.") : null;

				return GetItem<MultilingualStringRegistryItem>("BOLClause", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"BOLClause",
						Categories.Freight_HouseBills_Clauses,
						ResString.GetMultilingualString("b8b87107-46a0-46b0-bd3a-c05f05d8bd10", "Standard"),
						ResString.GetMultilingualString("ccfce3c1-7d3a-4441-81dd-abba5167d0e2", "This house bill clause will appear on house bills in the main body section."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, defaultValue);
					if (!EnableHouseBillOfLadingRegistryItems.Value)
					{
						result.Options |= RegistryOptions.IsHidden;
					}
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem BOLClauseITAR
		{
			get
			{
				ResourceString defaultValue = Env.CurrentCompany?.Country?.Code == Constants.CountryCodes.UnitedStates ? ResString.GetMultilingualString("8b95c084-66e0-456f-bb54-7522432bbbe1", "These commodities are authorized by the U.S. Government for export only to <BillOfLading.DeclarationDestinationCountry/Region> for end use by <BillOfLading.USUltimateConsignee>. They may not be transferred, transhipped on a non-continuous voyage, or otherwise be disposed of in any other country/region, either in their original form or after being incorporated into other end-items, without the prior approval of the U.S. Department of State.") : null;
				return GetItem<MultilingualStringRegistryItem>("BOLClauseITAR", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"BOLClauseITAR",
						Categories.Freight_HouseBills_Clauses,
						ResString.GetMultilingualString("e3d01ec5-7745-4cec-9c68-1edf925546a0", "Re-export/re-transfer 123.9 (B)"),
						ResString.GetMultilingualString("410e93ed-108d-43ef-8b82-25d2405ddf8d", "This house bill clause for International Traffic in Arms Regulations (ITAR) shipments will appear on house bills in the main body section. This is based on the License Number and Registration Number being completed on the Invoice Header on the Export Declaration tab of the Shipment."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company, defaultValue);
					if (!EnableHouseBillOfLadingRegistryItems.Value || Env.CurrentCompany?.Country?.Code != Constants.CountryCodes.UnitedStates || ZDateTime.Now >= USDestinationControlStatementEffectiveDate)
					{
						result.Options = RegistryOptions.IsHidden;
					}
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		ZDateTime USDestinationControlStatementEffectiveDate
		{
			get { return new ZDateTime(2016, 11, 15); }
		}

		#endregion

		#region FIATA HBL Settings

		#region FIATAAuthorised
		public BooleanRegistryItem FIATAAuthorised
		{
			get
			{
				return GetItem<BooleanRegistryItem>("FIATAAuthorised", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"FIATAAuthorised",
						Categories.Freight_HouseBills_HouseBillofLadingTypes_FIATAHBLSettings,
						ResString.GetMultilingualString("efcfd3a6-ae68-457b-8c66-8c215477cf95", "FIATA Authorization (Y/N)"),
						ResString.GetMultilingualString("55382a02-07c4-4058-a9fe-a7cafa132da7", "Only a valid and financial member of a forwarding association that is itself a member of FIATA is authorized to use the FIATA logo on the FIATA Bill of Lading."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
					result.EditorInfo = new BooleanRegistryEditorInfo(ResString.GetMultilingualString("772619fd-32a7-46cf-ba9f-8b02ca7fd574", "We confirm that we are a valid and financial member of a forwarding association that is itself a member of FIATA"));
					if (!EnableHouseBillOfLadingRegistryItems.Value)
					{
						result.Options |= RegistryOptions.IsHidden;
					}
					return result;
				});
			}
		}
		#endregion

		#region EnableFIATAHouseBillsFeatures
		public BooleanRegistryItem EnableFIATAHouseBillsFeatures
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableFIATAHouseBillsFeatures", delegate
				{
					return new BooleanRegistryItem(
						"EnableFIATAHouseBillsFeatures",
						Categories.Freight_HouseBills_HouseBillofLadingTypes_FIATAHBLSettings,
						ResString.GetMultilingualString("D00D511A-57D0-4C66-9918-667772FEA717", "Enable electronic FIATA Bills of Lading (eFBL)"),
						ResString.GetMultilingualString("CD9D5614-D9BF-4992-BA5E-FC8642ABBBC9", "If yes, electronic FIATA Bills of Lading functionality will be enabled for to use."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}
		#endregion

		#endregion

		#region HouseBillOfLadingLogoImages

		public RegistryImageCollectionRegistryItem HouseBillOfLadingLogoImages
		{
			get
			{
				return GetItem<RegistryImageCollectionRegistryItem>("HouseBillOfLadingLogoImages", delegate
				{
					RegistryImageCollectionRegistryItem result = new RegistryImageCollectionRegistryItem(
						"HouseBillOfLadingLogoImages", Categories.Freight_HouseBills_HouseBillofLadingTypes, ResString.GetMultilingualString("68583508-2984-4e69-929b-467f0e52cc22", "Logos"), null, RegistryStorageFlags.All);
					if (!EnableHouseBillOfLadingRegistryItems.Value)
					{
						result.Options |= RegistryOptions.IsHidden;
					}
					return result;
				});
			}
		}

		#endregion

		#region HBLPackLinesDisplayOrder

		public CodePairRegistryItem HBLPackLinesDisplayOrder
		{
			get
			{
				return GetItem("HBLPackLinesDisplayOrder", delegate
				{
					return new CodePairRegistryItem(new RegistryItemImpl(
						"HBLPackLinesDisplayOrder",
						Categories.Freight_HouseBills,
						ResString.GetMultilingualString("5fb63f0d-2b02-434a-b87b-655de2cabefa", "Show Pack Lines in Order"),
						ResString.GetMultilingualString("b8c939ec-8efe-40a2-b41c-4bd807e73aa9", "Enable this option to show pack lines in order on House Bills."),
						new HBLPackLinesDisplayOrderRegistryDataType(HBLPackLinesDisplayOrderListProvider),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						Constants.HBLPackLinesDisplayOrders.ContainerAndPackingOrder));
				});
			}
		}

		public ICodeDescriptionPairListProvider HBLPackLinesDisplayOrderListProvider
		{
			get
			{
				return new CodeDescriptionPairListProvider(() =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(Constants.HBLPackLinesDisplayOrders.ContainerAndPackingOrder, ResString.GetMultilingualString("5bd98bee-9dd0-41e7-aacc-79b45c43d734", "Container + Packing Order"));
					list.AddPair(Constants.HBLPackLinesDisplayOrders.ShowDGCargoFirst, ResString.GetMultilingualString("db21d83c-c3f6-47db-96be-471b490f4fd4", "Show DG Cargo First"));
					return list;
				});
			}
		}

		#endregion

		#region House Bill of Lading Types

		public HouseBillOfLadingTypeCollectionRegistryItem HouseBillOfLadingLogoTypes
		{
			get
			{
				return GetItem<HouseBillOfLadingTypeCollectionRegistryItem>("HouseBillOfLadingLogoTypes", delegate
				{
					HouseBillOfLadingTypeCollectionRegistryItem result = new HouseBillOfLadingTypeCollectionRegistryItem(
						"HouseBillOfLadingLogoTypes",
						Categories.Freight_HouseBills_HouseBillofLadingTypes,
						ResString.GetMultilingualString("3c256369-6112-434f-b49b-248dee29c708", "House Bill of Lading Settings"),
						null,
						RegistryStorageFlags.All,
						GetHouseBillOfLadingTypesDefaultValue());
					if (!EnableHouseBillOfLadingRegistryItems.Value)
					{
						result.Options = RegistryOptions.IsHidden;
					}
					return result;
				});
			}
		}

		HouseBillOfLadingTypeCollection GetHouseBillOfLadingTypesDefaultValue()
		{
			HouseBillOfLadingTypeCollection result = new HouseBillOfLadingTypeCollection();

			result.SuspendValidation();

			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "EAG", ResString.GetMultilingualString("732c29eb-d380-4ae4-9529-e2b04dfda3a5", "CargoWise Bill"), "", false, true, PrintLogoOptions.Codes.All);
			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "EAP", ResString.GetMultilingualString("f0dce89d-f19d-486f-ad53-1e1e9c2a3922", "CargoWise Bill Preprinted"), "", true, false, PrintLogoOptions.Codes.None);
			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "FIA", ResString.GetMultilingualString("2b5efa0a-af2c-4174-b3e0-1b1d6da48fb7", "FIATA Bill"), "FIA", false, true, PrintLogoOptions.Codes.All);
			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "FIP", ResString.GetMultilingualString("0149829c-e785-4bd0-815f-8b02b0b132fc", "FIATA Bill Preprinted"), "", true, false, PrintLogoOptions.Codes.None);
			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "IAU", ResString.GetMultilingualString("e68fd930-4231-4f95-8982-46e953bace80", "IT Club Australia"), "IAU", false, true, PrintLogoOptions.Codes.All);
			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "ISI", ResString.GetMultilingualString("8402f610-0a38-4afc-b5be-7946eeeddb7f", "IT Club Australia No Terms"), "", false, true, PrintLogoOptions.Codes.All);
			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "INN", ResString.GetMultilingualString("f2d2e469-197c-44b5-9996-b7c4ea4506a5", "IT Club Australia No Terms No Law"), "", false, true, PrintLogoOptions.Codes.All);
			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "ITP", ResString.GetMultilingualString("5ec01c50-edfb-400f-ba07-948b24553aec", "IT Club Australia Preprinted"), "", true, false, PrintLogoOptions.Codes.None);
			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "TNZ", ResString.GetMultilingualString("37667c33-6e55-4012-af3f-9a23d90ec17b", "TT Club Bill"), "TTC", false, true, PrintLogoOptions.Codes.All);
			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "TTN", ResString.GetMultilingualString("25f0f033-3b81-47b9-a515-b591d497b4cb", "TT Club Bill No Terms"), "", false, true, PrintLogoOptions.Codes.All);
			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "TTP", ResString.GetMultilingualString("e3c7053c-9464-46c1-8ea2-f906f4f87742", "TT Club Bill Preprinted"), "", true, false, PrintLogoOptions.Codes.None);
			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "TAN", ResString.GetMultilingualString("9c4f7281-419c-4e29-be06-df8c7aac3ce5", "TAN Bill of Lading"), "", false, true, PrintLogoOptions.Codes.All);
			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "TAP", ResString.GetMultilingualString("79d9b77c-a148-45aa-bb60-78cef31c68ea", "TAN Bill Preprinted"), "", true, false, PrintLogoOptions.Codes.None);
			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "DHK", ResString.GetMultilingualString("ea3db6c3-fb31-4e2c-9f40-96afb2cd304e", "Datahawk Bill of Lading"), "", false, true, PrintLogoOptions.Codes.All);
			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "INZ", ResString.GetMultilingualString("35e0638f-c4b3-408d-a1a2-a78edb57fa9d", "IT Club New Zealand"), "INZ", false, true, PrintLogoOptions.Codes.All);
			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "INP", ResString.GetMultilingualString("95128e48-aeb6-4604-837a-ebeaa7c096e2", "IT Club New Zealand Preprinted"), "", true, false, PrintLogoOptions.Codes.None);
			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "TUS", ResString.GetMultilingualString("f026ae92-f267-41fe-a9cb-25db263ee0da", "TT Club United States"), "TUS", false, false, PrintLogoOptions.Codes.None);
			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "TUP", ResString.GetMultilingualString("b30ab196-77e7-4f86-86b5-b171cc3f7930", "TT Club United States Preprinted"), "", true, false, PrintLogoOptions.Codes.None);
			SetUpHouseBillOfLadingTypeWithValues(result.AddNew(), "CPT", ResString.GetMultilingualString("e2eb93b2-e22e-453e-bfc7-45ef2143700c", "Carta Porte - Spanish"), "", false, true, PrintLogoOptions.Codes.All);

			result.ResumeValidation();

			return result;
		}

		void SetUpHouseBillOfLadingTypeWithValues(HouseBillOfLadingType hblType, ZString code, MultilingualString description, ZString termsAndConditions, ZBool prePrinted, ZBool printLogo, ZString printLogoInFormBuilder)
		{
			hblType.Code = code;
			hblType.Description = description;
			hblType.TermsAndConditionsCode = termsAndConditions;
			hblType.PrePrinted = prePrinted;
			hblType.PrintLogo = printLogo;
			hblType.PrintLogoInFormBuilder = printLogoInFormBuilder;
		}

		#endregion

		#region Addtional House Bill of Lading Types

		public AdditionalHouseBillOfLadingTypeCollectionRegistryItem AddtionalHouseBillOfLadingTypes
		{
			get
			{
				return GetItem<AdditionalHouseBillOfLadingTypeCollectionRegistryItem>("AdditionalHBLTypes", delegate
				{
					var defaultValues = new AdditionalHouseBillOfLadingTypeCollection();

					var yusType = new AdditionalHouseBillOfLadingType
					{
						Code = Constants.AddtionalHouseBillTypeMenu.Code.YusenHBL,
						Description = (NoResString)Constants.AddtionalHouseBillTypeMenu.YusenHBLMenuName
					};

					var yusStyle1 = new AdditionalHouseBillOfLadingType
					{
						Code = Constants.AddtionalHouseBillTypeMenu.Code.YusenHBLStyle1,
						Description = (NoResString)Constants.AddtionalHouseBillTypeMenu.YusenHBLStyle1MenuName
					};

					var yusStyle2 = new AdditionalHouseBillOfLadingType
					{
						Code = Constants.AddtionalHouseBillTypeMenu.Code.YusenHBLStyle2,
						Description = (NoResString)Constants.AddtionalHouseBillTypeMenu.YusenHBLStyle2MenuName
					};

					var yusStyle3 = new AdditionalHouseBillOfLadingType
					{
						Code = Constants.AddtionalHouseBillTypeMenu.Code.YusenHBLStyle3,
						Description = (NoResString)Constants.AddtionalHouseBillTypeMenu.YusenHBLStyle3MenuName
					};

					var yusStyle4 = new AdditionalHouseBillOfLadingType
					{
						Code = Constants.AddtionalHouseBillTypeMenu.Code.YusenHBLStyle4,
						Description = (NoResString)Constants.AddtionalHouseBillTypeMenu.YusenHBLStyle4MenuName
					};

					var dhlType = new AdditionalHouseBillOfLadingType
					{
						Code = Constants.AddtionalHouseBillTypeMenu.Code.DHLHBL,
						Description = (NoResString)Constants.AddtionalHouseBillTypeMenu.DHLHBLMenuName,
					};

					var dhlStyle1 = new AdditionalHouseBillOfLadingType
					{
						Code = Constants.AddtionalHouseBillTypeMenu.Code.DHLHBLStyle1,
						Description = (NoResString)Constants.AddtionalHouseBillTypeMenu.DHLHBLStyle1MenuName,
					};

					var dhlStyle2 = new AdditionalHouseBillOfLadingType
					{
						Code = Constants.AddtionalHouseBillTypeMenu.Code.DHLHBLStyle2,
						Description = (NoResString)Constants.AddtionalHouseBillTypeMenu.DHLHBLStyle2MenuName,
					};

					defaultValues.AddRange(new[]
					{
						yusType,
						yusStyle1,
						yusStyle2,
						yusStyle3,
						yusStyle4,
						dhlType,
						dhlStyle1,
						dhlStyle2
					});

					var result = new AdditionalHouseBillOfLadingTypeCollectionRegistryItem(
							"AdditionalHBLTypes",
							Categories.Freight_HouseBills_HouseBillofLadingTypes,
							(NoResString)"Additional Types",
							(NoResString)"This is for support to enable NVOCC specific house bill types.",
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							defaultValues);

					if (!EnableHouseBillOfLadingRegistryItems.Value)
					{
						result.Options = RegistryOptions.IsHidden;
					}

					return result;
				});
			}
		}

		#endregion

		#region House Bill of Lading Terms and Conditions Images

		public HouseBillOfLadingTermsAndConditionsCollectionRegistryItem HouseBillOfLadingTermsAndConditionsImages
		{
			get
			{
				return GetItem<HouseBillOfLadingTermsAndConditionsCollectionRegistryItem>("HouseBillOfLadingTermsAndConditionsImages", delegate
				{
					HouseBillOfLadingTermsAndConditionsCollectionRegistryItem result = new HouseBillOfLadingTermsAndConditionsCollectionRegistryItem(
						"HouseBillOfLadingTermsAndConditionsImages",
						Categories.Freight_HouseBills_HouseBillofLadingTypes,
						ResString.GetMultilingualString("3dce874c-fc68-4533-9224-6acbc674b503", "Terms & Conditions"),
						null,
						RegistryStorageFlags.All);
					if (!EnableHouseBillOfLadingRegistryItems.Value)
					{
						result.Options |= RegistryOptions.IsHidden;
					}
					return result;
				});
			}
		}

		#endregion

		#region House Bill Shipment Number Customisation

		public BillCustomisationRegistryItem HouseBillShipmentNumberCustomisation
		{
			get
			{
				return GetItem<BillCustomisationRegistryItem>("HouseBillShipmentNumberCustomisation", delegate
				{
					BillCustomisationRegistryDataType dataType = new BillCustomisationRegistryDataType();
					dataType.FountainPrefix = null;
					dataType.GeneratedNumberName = ResString.GetMultilingualString("3a742596-8de3-4059-b323-9c0f81902b3a", "Shipment Number");
					dataType.SequenceNumberName = ResString.GetMultilingualString("A98E42E1-6C9A-4D02-9C0E-EC4CB18A8A91", "Shipment");
					dataType.MaxLength = JobShipmentSchema.JS_UniqueConsignRef.MaxLength;

					return new BillCustomisationRegistryItem(
						"HouseBillShipmentNumberCustomisation",
						Categories.Freight_HouseBills_NumberCustomizations,
						ResString.GetMultilingualString("f1eadd75-0e1a-44b8-9874-3ea1d65dc9f6", "Shipment Number"),
						ResString.GetMultilingualString("39e59053-d7c0-4db1-973f-98eb6f92d6e9", "Override this value to customize how shipment numbers are formatted."),
						RegistryStorageFlags.All,
						dataType
						);
				});
			}
		}

		#endregion

		#region House Bill Number Customisation

		public BooleanRegistryItem HouseBillNumberRegeneration
		{
			get
			{
				return GetItem<BooleanRegistryItem>("HouseBillNumberRegeneration", delegate
				{
					return new BooleanRegistryItem(
						"HouseBillNumberRegeneration",
						Categories.Freight_HouseBills_NumberCustomizations,
						ResString.GetMultilingualString("81651809-0181-42a2-9703-5cbe65562bdf", "House Bill Number Regeneration"),
						ResString.GetMultilingualString("98731b4f-2752-4a24-87cd-aef354185757", "If turned on and Shipment's Service Level is changed, the user will be asked upon saving if they wish to regenerate the House Bill Number."),
						RegistryStorageFlags.All, false);
				});
			}
		}

		public BillCustomisationByServiceLevelRegistryItem HouseBillNumberCustomisation
		{
			get
			{
				return GetItem<BillCustomisationByServiceLevelRegistryItem>("HouseBillNumberCustomisation", delegate
				{
					BillCustomisationByServiceLevelRegistryDataType dataType = new BillCustomisationByServiceLevelRegistryDataType();
					dataType.FountainPrefix = "S";
					dataType.GeneratedNumberName = ResString.GetMultilingualString("c1164089-1456-4d8c-b048-4fd15374c3ff", "House Bill");
					dataType.SequenceNumberName = ResString.GetMultilingualString("A98E42E1-6C9A-4D02-9C0E-EC4CB18A8A91", "Shipment");
					dataType.MaxLength = JobShipmentSchema.JS_HouseBill.MaxLength;
					dataType.AllowNonAlphanumericCharacters = true;
					dataType.EnableMacroInsertion = true;
					dataType.MacroType = ObjectFactory.GetType<IForwardingShipment>();

					return new BillCustomisationByServiceLevelRegistryItem(
						"HouseBillNumberCustomisation",
						Categories.Freight_HouseBills_NumberCustomizations,
						ResString.GetMultilingualString("158bb916-2ad6-4b73-84d0-58d55ab005ae", "House Bill Number"),
						ResString.GetMultilingualString("50ddf218-9990-4eed-bb9c-8b509478d1c6", "Override this value to customize how House Bills are formatted for ALL Transport Modes."),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue,
						dataType,
						HouseBillShipmentNumberCustomisation
						);
				});
			}
		}

		public BillCustomisationByServiceLevelRegistryItem HouseBillNumberCustomisation_AIR
		{
			get
			{
				return GetItem<BillCustomisationByServiceLevelRegistryItem>("HouseBillNumberCustomisation_AIR", delegate
				{
					BillCustomisationByServiceLevelRegistryDataType dataType = new BillCustomisationByServiceLevelRegistryDataType();
					dataType.FountainPrefix = "S";
					dataType.GeneratedNumberName = ResString.GetMultilingualString("A8FD4CEC-F2EF-4a00-BDBC-CDA3691FDCF9", "House Bill");
					dataType.SequenceNumberName = ResString.GetMultilingualString("A98E42E1-6C9A-4D02-9C0E-EC4CB18A8A91", "Shipment");
					dataType.MaxLength = JobShipmentSchema.JS_HouseBill.MaxLength;
					dataType.AllowNonAlphanumericCharacters = true;
					dataType.EnableMacroInsertion = true;
					dataType.MacroType = ObjectFactory.GetType<IForwardingShipment>();

					return new BillCustomisationByServiceLevelRegistryItem(
						"HouseBillNumberCustomisation_AIR",
						Categories.Freight_HouseBills_NumberCustomizations,
						ResString.GetMultilingualString("18e639e2-0c74-4579-841c-e2d226f7bef8", "House Bill Number - AIR"),
						ResString.GetMultilingualString("7d137380-0995-431a-a19b-a5afb41a446a", "Override this value to customize how House Bills are formatted for Transport Mode AIR."),
						RegistryStorageFlags.All,
						dataType,
						HouseBillNumberCustomisation);
				});
			}
		}

		public BillCustomisationByServiceLevelRegistryItem HouseBillNumberCustomisation_SEA
		{
			get
			{
				return GetItem<BillCustomisationByServiceLevelRegistryItem>("HouseBillNumberCustomisation_SEA", delegate
				{
					BillCustomisationByServiceLevelRegistryDataType dataType = new BillCustomisationByServiceLevelRegistryDataType();
					dataType.FountainPrefix = "S";
					dataType.GeneratedNumberName = ResString.GetMultilingualString("F6ED783C-3FD7-4210-B42F-77A855563C72", "House Bill");
					dataType.SequenceNumberName = ResString.GetMultilingualString("A98E42E1-6C9A-4D02-9C0E-EC4CB18A8A91", "Shipment");
					dataType.MaxLength = JobShipmentSchema.JS_HouseBill.MaxLength;
					dataType.AllowNonAlphanumericCharacters = true;
					dataType.EnableMacroInsertion = true;
					dataType.MacroType = ObjectFactory.GetType<IForwardingShipment>();

					return new BillCustomisationByServiceLevelRegistryItem(
						"HouseBillNumberCustomisation_SEA",
						Categories.Freight_HouseBills_NumberCustomizations,
						ResString.GetMultilingualString("c8f19e1d-76df-4407-92cc-dc3600b8b29e", "House Bill Number - SEA"),
						ResString.GetMultilingualString("2662e81e-3153-4432-b8f1-bdb76d3436b4", "Override this value to customize how House Bills are formatted for Transport Mode SEA."),
						RegistryStorageFlags.All,
						dataType,
						HouseBillNumberCustomisation);
				});
			}
		}

		public BillCustomisationByServiceLevelRegistryItem HouseBillNumberCustomisation_RAIL
		{
			get
			{
				return GetItem<BillCustomisationByServiceLevelRegistryItem>("HouseBillNumberCustomisation_RAIL", delegate
				{
					BillCustomisationByServiceLevelRegistryDataType dataType = new BillCustomisationByServiceLevelRegistryDataType();
					dataType.FountainPrefix = "S";
					dataType.GeneratedNumberName = ResString.GetMultilingualString("521A0BC4-A5A1-4ec9-B3E1-DD9D195963CA", "House Bill");
					dataType.SequenceNumberName = ResString.GetMultilingualString("A98E42E1-6C9A-4D02-9C0E-EC4CB18A8A91", "Shipment");
					dataType.MaxLength = JobShipmentSchema.JS_HouseBill.MaxLength;
					dataType.AllowNonAlphanumericCharacters = true;
					dataType.EnableMacroInsertion = true;
					dataType.MacroType = ObjectFactory.GetType<IForwardingShipment>();

					return new BillCustomisationByServiceLevelRegistryItem(
						"HouseBillNumberCustomisation_RAIL",
						Categories.Freight_HouseBills_NumberCustomizations,
						ResString.GetMultilingualString("7c9ee925-fad9-42b8-b498-a204e5426401", "House Bill Number - RAIL"),
						ResString.GetMultilingualString("2f88dd3d-827a-4f25-9d15-3aa2d63e4d8b", "Override this value to customize how House Bills are formatted for Transport Mode RAIL."),
						RegistryStorageFlags.All,
						dataType,
						HouseBillNumberCustomisation);
				});
			}
		}

		public BillCustomisationByServiceLevelRegistryItem HouseBillNumberCustomisation_ROAD
		{
			get
			{
				return GetItem<BillCustomisationByServiceLevelRegistryItem>("HouseBillNumberCustomisation_ROAD", delegate
				{
					BillCustomisationByServiceLevelRegistryDataType dataType = new BillCustomisationByServiceLevelRegistryDataType();
					dataType.FountainPrefix = "S";
					dataType.GeneratedNumberName = ResString.GetMultilingualString("3B8A5AC1-0F5F-46ae-AA4C-D6E17E9512FA", "House Bill");
					dataType.SequenceNumberName = ResString.GetMultilingualString("A98E42E1-6C9A-4D02-9C0E-EC4CB18A8A91", "Shipment");
					dataType.MaxLength = JobShipmentSchema.JS_HouseBill.MaxLength;
					dataType.AllowNonAlphanumericCharacters = true;
					dataType.EnableMacroInsertion = true;
					dataType.MacroType = ObjectFactory.GetType<IForwardingShipment>();

					return new BillCustomisationByServiceLevelRegistryItem(
						"HouseBillNumberCustomisation_ROAD",
						Categories.Freight_HouseBills_NumberCustomizations,
						ResString.GetMultilingualString("dbbc3714-70fb-4666-9a88-1e41ca4de39b", "House Bill Number - ROAD"),
						ResString.GetMultilingualString("08548a6d-6708-4863-8b53-5313ab2a25cb", "Override this value to customize how House Bills are formatted for Transport Mode ROAD."),
						RegistryStorageFlags.All,
						dataType,
						HouseBillNumberCustomisation);
				});
			}
		}
		#endregion

		#region House Bill of Lading Image Container

		internal ImageRegistryItem RegistryImageContainer
		{
			get
			{
				return GetItem<ImageRegistryItem>("REGISTRY_IMAGE_CONTAINER", delegate
				{
					return new ImageRegistryItem(
						"REGISTRY_IMAGE_CONTAINER",
						null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue);
				});
			}
		}

		#endregion

		#region PrintChargesBilledToLocalClientAtDestAsCollect

		public PrintChargesBilledToLocalClientAtDestAsCollectCollectionRegistryItem PrintChargesBilledToLocalClientAtDestAsCollect
		{
			get
			{
				return GetItem("PrintChargesBilledToLocalClientAtDestAsCollect", delegate
				{
					return new PrintChargesBilledToLocalClientAtDestAsCollectCollectionRegistryItem(
						"PrintChargesBilledToLocalClientAtDestAsCollect",
						Categories.Freight_HouseBills,
						ResString.GetMultilingualString("fcb4bfd9-f7d4-4d58-a950-646d246d2a50", "Print Charges billed to Local Client at Destination as Collect"),
						ResString.GetMultilingualString("8bc3aeec-fd04-4e95-8eea-e81c05490b4d", "This registry allows you to configure Shipments under the combination of Transport Mode, Country and Direction printing charges billed to Local Client at Destination as Collect Charges."),
						RegistryStorageFlags.System,
						RawDataRegistry.Instance.UseFormBuilderHouseBills.Value ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		#endregion

		#endregion

		#region Incoterms

		#region Courier

		public CodePairRegistryItem CourierIncoTerm
		{
			get
			{
				return GetItem("CourierIncoTerm", delegate
				{
					return new CodePairRegistryItem(
						"CourierIncoTerm",
						Categories.Freight_INCOTERMs,
						ResString.GetMultilingualString("c0b09b79-1870-21bd-4d46-becab217289a", "Courier Incoterm"),
						ResString.GetMultilingualString("e8c0aa0a-711b-c39d-48c7-609903f9aca6", "Default a Courier specific Incoterm to display on all courier manifest shipments."),
						new IncoTermsCodeDescriptionPairListProvider(IncoTermsListType.ActiveIncoTerms),
						RegistryStorageFlags.BranchDepartment,
						RegistryOptions.PreserveTestValue,
						ZDateTime.UtcNow < Constants.IncoTerms.Incoterms2020EffectiveDate ? Constants.IncoTerms.DeliveredAtPlace : Constants.IncoTerms.DeliveredAtPlaceUnloaded);
				});
			}
		}

		#endregion

		#endregion

		#region Container

		public CodePairRegistryItem VGMVerifiedByDefaultsTo
		{
			get
			{
				return GetItem("VGMVerifiedByDefaultsTo", delegate
				{
					return new CodePairRegistryItem("VGMVerifiedByDefaultsTo",
						Categories.Freight_Container,
						ResString.GetMultilingualString("759e0d63-6d59-42a0-802e-566270e73ed5", "VGM Verified By Defaults To"),
						ResString.GetMultilingualString("099015b9-6334-4ad6-b670-591c2806e3ec",
							"This governs which address appears in the Consol > Container > VGM > VGM Verified By for non-Direct consols by default"),
						VGMVerifiedPartiesListProvider,
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						ZString.Empty);
				});
			}
		}

		public ICodeDescriptionPairListProvider VGMVerifiedPartiesListProvider
		{
			get
			{
				return new CodeDescriptionPairListProvider(() =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(ZString.Empty, ZString.Empty);
					list.AddPair(Constants.VGMVerifiedParties.OwnSendingAgent, ResString.GetMultilingualString("a8c67351-f788-4aca-b58e-64720c8825b9", "Own Sending Agent"));
					list.AddPair(Constants.VGMVerifiedParties.AnySendingAgent, ResString.GetMultilingualString("dcc8b0de-2f7b-4c6f-ac63-ca7eae49ab66", "Any Sending Agent"));
					list.AddPair(Constants.VGMVerifiedParties.DepartureCFS, ResString.GetMultilingualString("31c75dc3-2eed-4b90-8144-00b4ad5dd735", "Departure CFS"));
					return list;
				});
			}
		}

		public DeliveryModeRegistryItem ContainerDeliveryModeList
		{
			get
			{
				return GetItem<DeliveryModeRegistryItem>("ContainerDeliveryModeList", delegate
				{
					return new DeliveryModeRegistryItem(
						"ContainerDeliveryModeList",
						Categories.Freight_Container,
						ResString.GetMultilingualString("bb5a6242-5f8d-4baa-a300-a871b84cecb7", "Container Delivery Mode List"),
						ResString.GetMultilingualString("55970324-069d-4f4c-938e-adfcf1fe741a", "A list of container delivery mode types."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DeliveryModeCollection.GetDefault());
				});
			}
		}

		public BooleanRegistryItem ContainerDeliveryModeOverride
		{
			get
			{
				return GetItem("ContainerDeliveryModeOverride", delegate
				{
					return new BooleanRegistryItem(
						"ContainerDeliveryModeOverride",
						Categories.Freight_Container,
						(NoResString)"Container Delivery Mode Override",
						(NoResString)"Use this registry upon customer request to substitute system defined Container Delivery Modes with user preferred options in Forwarding and Liner & Agency containers.  System defined codes will be substituted on the forms and documents but not in the database schema or Universal Shipment XML as to not break the integrity of external integrations.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false
						);
				});
			}
		}

		public ContainerPenaltyFreeDaysOptionsRegistryItem DefaultContainerDetentionFreeDaysForImport
		{
			get
			{
				return GetItem("DefaultContainerDetentionFreeDaysForImport", delegate
				{
					return new ContainerPenaltyFreeDaysOptionsRegistryItem(
						"DefaultContainerDetentionFreeDaysForImport",
						Categories.Freight_Container,
						ResString.GetMultilingualString("a78b6812-2f79-4c76-87d0-1694560b74b7", "Container Detention Free Days for Import"),
						ResString.GetMultilingualString("e3a24001-ca81-4bb0-84be-89d25122a7a0", "Number of days that a container will be held in detention free of charge.\r\n\r\nIf Unlimited Free Days is ticked this assumes no penalty to apply."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						new ContainerPenaltyFreeDaysOptions { FreeDays = 10, UnlimitedFreeDays = false });
				});
			}
		}

		public ContainerPenaltyFreeDaysOptionsRegistryItem DefaultContainerDetentionFreeDaysForExport
		{
			get
			{
				return GetItem("DefaultContainerDetentionFreeDaysForExport", delegate
				{
					return new ContainerPenaltyFreeDaysOptionsRegistryItem(
						"DefaultContainerDetentionFreeDaysForExport",
						Categories.Freight_Container,
						ResString.GetMultilingualString("6cbe752f-aefc-4cf1-ac4e-112ce823f682", "Container Detention Free Days for Export"),
						ResString.GetMultilingualString("3d6da0de-0c7e-479e-8215-4c31f4050320", "Number of days that a container will be held in detention free of charge.\r\n\r\nIf Unlimited Free Days is ticked this assumes no penalty to apply."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						new ContainerPenaltyFreeDaysOptions { FreeDays = 10, UnlimitedFreeDays = false });
				});
			}
		}

		public ContainerPenaltyFreeDaysOptionsRegistryItem DefaultFreeCTOStorageDaysForImport
		{
			get
			{
				return GetItem("DefaultFreeCTOStorageDaysForImport", delegate
				{
					return new ContainerPenaltyFreeDaysOptionsRegistryItem(
						"DefaultFreeCTOStorageDaysForImport",
						Categories.Freight_Container,
						ResString.GetMultilingualString("e6cebf04-a3db-4449-89a9-48a117c6e4b8", "CTO Storage Free Days for Import"),
						ResString.GetMultilingualString("f4058b69-c752-4c1f-bc32-f36beaf28954", "Number of days that a container will be stored for free with the CTO for imports before incurring CTO storage charges.\r\n\r\nIf Unlimited Free Days is ticked this assumes no penalty to apply."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						new ContainerPenaltyFreeDaysOptions { FreeDays = 0, UnlimitedFreeDays = false });
				});
			}
		}

		public ContainerPenaltyFreeDaysOptionsRegistryItem DefaultFreeCTOStorageDaysForExport
		{
			get
			{
				return GetItem("DefaultFreeCTOStorageDaysForExport", delegate
				{
					return new ContainerPenaltyFreeDaysOptionsRegistryItem(
						"DefaultFreeCTOStorageDaysForExport",
						Categories.Freight_Container,
						ResString.GetMultilingualString("8c2a673d-2288-41e6-98e3-dfcea169d885", "CTO Storage Free Days for Export"),
						ResString.GetMultilingualString("bf7d2613-292d-4ea0-9276-5fc01c4fe128", "Number of days that a container will be stored for free with the CTO for exports before incurring CTO storage charges.\r\n\r\nIf Unlimited Free Days is ticked this assumes no penalty to apply."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						new ContainerPenaltyFreeDaysOptions { FreeDays = 0, UnlimitedFreeDays = false });
				});
			}
		}

		public CodeDescriptionPairListRegistryItem ContainerQualityList
		{
			get
			{
				return GetItem<CodeDescriptionPairListRegistryItem>("ContainerQualityList", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"ContainerQualityList",
						Categories.Freight_Container,
						ResString.GetMultilingualString("82e72991-41fe-4695-a29d-65f319e6ea9c", "Container Quality List"),
						ResString.GetMultilingualString("dd416e5f-0c85-4309-9d60-c25244df8324", "A list of container quality types."),
						3,
						RegistryStorageFlags.System,
						ContainerQualityListDefault);
				});
			}
		}

		ReadOnlyCodeDescriptionPairList ContainerQualityListDefault
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair("GEN", ResString.GetMultilingualString("53ce86c1-63af-451a-9387-1afcf5cfa1f3", "General"));
				result.AddPair("RIC", ResString.GetMultilingualString("a18a558f-05df-4817-b2c9-aae6a49b6bce", "Rice"));
				result.AddPair("HID", ResString.GetMultilingualString("b2d0e340-36f5-4ece-98a0-6e06b167a0d2", "Hides"));
				result.AddPair("VEN", ResString.GetMultilingualString("018db088-aee3-488b-9028-2cab2fffae5e", "Vent General"));
				result.AddPair("REP", ResString.GetMultilingualString("d91aa087-b1d1-4072-bbe5-93e0b6bde41f", "Reposition"));
				result.AddPair("LBG", ResString.GetMultilingualString("b0221bba-9876-4d36-85dd-888b360dd638", "Liner Bag"));
				result.AddPair("SFR", ResString.GetMultilingualString("65c8cc16-9598-48f6-9029-df4eb4acde3a", "Super Freezer"));
				result.AddPair("FOD", ResString.GetMultilingualString("2d65700e-b678-4462-b94e-682d868894d5", "Food"));
				result.AddPair("GOH", ResString.GetMultilingualString("b5afcbd4-3308-4c7f-9083-5717daaca2f9", "Garments on Hangers"));
				result.AddPair("GOS", ResString.GetMultilingualString("2127fb69-a743-49f4-8f25-726baa221a7c", "Garments on Hangers Single Bar"));
				result.AddPair("GOD", ResString.GetMultilingualString("c5ba82c7-cd60-4542-8b25-ebf43e927dff", "Garments on Hangers Double Bar"));
				return result;
			}
		}

		public CodeDescriptionPairListRegistryItem ContainerStatusList
		{
			get
			{
				return GetItem<CodeDescriptionPairListRegistryItem>("ContainerStatusList", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"ContainerStatusList",
						Categories.Freight_Container,
						ResString.GetMultilingualString("d632c804-fec7-4b12-9ab8-6adedee822ec", "Container Status List"),
						ResString.GetMultilingualString("fd72b5df-0fba-40d8-9b4a-c176a9aefa07", "A list of container status options."),
						3,
						RegistryStorageFlags.System,
						ContainerStatusListDefault);
				});
			}
		}

		ReadOnlyCodeDescriptionPairList ContainerStatusListDefault
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair("AVL", ResString.GetMultilingualString("Freight|ContainerStatusList|AVL", "Available"));
				result.AddPair("INS", ResString.GetMultilingualString("Freight|ContainerStatusList|INS", "To Be Inspected"));
				result.AddPair("WAS", ResString.GetMultilingualString("Freight|ContainerStatusList|WAS", "Wash"));
				result.AddPair("APP", ResString.GetMultilingualString("Freight|ContainerStatusList|APP", "Approved"));
				result.AddPair("AWA", ResString.GetMultilingualString("Freight|ContainerStatusList|AWA", "Awaiting Approval"));
				result.AddPair("REJ", ResString.GetMultilingualString("Freight|ContainerStatusList|REJ", "Rejected"));
				result.AddPair("DAM", ResString.GetMultilingualString("Freight|ContainerStatusList|DAM", "Damaged"));
				result.AddPair("DM1", ResString.GetMultilingualString("Freight|ContainerStatusList|DM1", "Damage Minor"));
				result.AddPair("DM2", ResString.GetMultilingualString("Freight|ContainerStatusList|DM2", "Damage Medium"));
				result.AddPair("DM3", ResString.GetMultilingualString("Freight|ContainerStatusList|DM3", "Damage Major"));
				return result;
			}
		}

		#endregion

		#region Landed Costing Preferences

		public LandedCostingPreferencesRegistryItem LandedCostingPreferences
		{
			get
			{
				return GetItem<LandedCostingPreferencesRegistryItem>("LandedCostingPreferences", delegate
				{
					return new LandedCostingPreferencesRegistryItem(
						"LandedCostingPreferences",
						Categories.Freight,
						ResString.GetMultilingualString("e6db48d1-b8b7-4e2a-bf7d-401030d8c13d", "Landed Costing Preferences"),
						ResString.GetMultilingualString("2479cd77-1e8f-4f92-8e30-aceaab223a1e", "These preferences will be used when generating Landed Costings. You can specify a list of Landed Costing Groups, and then specify what Charge Code Groups and / or Charge Codes are part of this group."),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						GetLandedCostingPreferencesDefaultValue());
				});
			}
		}

		LandedCostingGroupCollection GetLandedCostingPreferencesDefaultValue()
		{
			LandedCostingGroupCollection result = new LandedCostingGroupCollection();

			LandedCostingGroup landedCostingGroup1 = result.AddNew();
			LandedCostingGroup landedCostingGroup2 = result.AddNew();
			LandedCostingGroup landedCostingGroup3 = result.AddNew();
			LandedCostingGroup landedCostingGroup4 = result.AddNew();
			LandedCostingGroup landedCostingGroup5 = result.AddNew();

			landedCostingGroup1.GroupID = 1;
			landedCostingGroup2.GroupID = 2;
			landedCostingGroup3.GroupID = 3;
			landedCostingGroup4.GroupID = 4;
			landedCostingGroup5.GroupID = 5;

			landedCostingGroup1.GroupName = (NoResString)"Origin Charges";
			landedCostingGroup2.GroupName = (NoResString)"Intl Freight Charges";
			landedCostingGroup3.GroupName = (NoResString)"Destination Charges";
			landedCostingGroup4.GroupName = (NoResString)"Entry Charges";
			landedCostingGroup5.GroupName = (NoResString)"Brokerage Charges";

			landedCostingGroup1.CostDistributionCode = "AWV";
			landedCostingGroup2.CostDistributionCode = "AWV";
			landedCostingGroup3.CostDistributionCode = "AWV";
			landedCostingGroup4.CostDistributionCode = "AWV";
			landedCostingGroup5.CostDistributionCode = "AWV";

			AddChargeCodeToLandedCostingGroup(landedCostingGroup1, new string[] { "ORG", "LOD" });
			AddChargeCodeToLandedCostingGroup(landedCostingGroup2, new string[] { "FRT" });
			AddChargeCodeToLandedCostingGroup(landedCostingGroup3, new string[] { "DST", "UNL" });
			AddChargeCodeToLandedCostingGroup(landedCostingGroup4, Array.Empty<string>());
			AddChargeCodeToLandedCostingGroup(landedCostingGroup5, new string[] { "BRK" });

			return result;
		}

		void AddChargeCodeToLandedCostingGroup(LandedCostingGroup landedCostingGroup, string[] chargeGroups)
		{
			foreach (string chargeGroup in chargeGroups)
			{
				ChargeGroupAndChargeCode charge = landedCostingGroup.Charges.AddNew();
				charge.ChargeGroupCode = chargeGroup;
			}
		}

		#endregion

		#region Shipper Load and Count

		public CodePairRegistryItem ShipperLoadAndCount
		{
			get
			{
				return GetItem<CodePairRegistryItem>("ShipperLoadAndCount", delegate
				{
					var shipperLoadAndCountCodeListProvider = new CodeDescriptionPairListProvider(() => new Freight.ShipperLoadAndCountCodeList());

					return new CodePairRegistryItem(
						"ShipperLoadAndCount",
						Categories.Documents_Forwarding_Shipment_BillofLading,
						ResString.GetMultilingualString("90a79768-fc82-4478-9c2c-3ae5af018e25", "Shipper Load and Count"),
						ResString.GetMultilingualString("c390eb81-1de7-4bc0-9066-2c5e78427aa9", "Shipper Load and Count"),
						shipperLoadAndCountCodeListProvider,
						false,
						true,
						new ComboBoxRegistryEditorInfo(shipperLoadAndCountCodeListProvider, true),
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						RawDataRegistry.Instance.UseFormBuilderHouseBills.Value ? RegistryOptions.IsOnlyForSupport : RegistryOptions.PreserveTestValue,
						Freight.ShipperLoadAndCountCodeList.Codes.ShipperLoadAndCount,
						false);
				});
			}
		}

		#endregion

		#region Consolidations

		#region ShowSubHouseBills

		public BooleanRegistryItem ShowSubHouseBills
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ShowSubHouseBills", delegate
				{
					return new BooleanRegistryItem(
						"ShowSubHouseBills",
						Categories.Freight_Consolidations,
						ResString.GetMultilingualString("ed59253b-d318-456d-87ca-96edea57eb88", "Show Sub House Bills"),
						ResString.GetMultilingualString("eb5b0841-f800-4386-b18e-b8986707d657", "Specify whether you would like, by default, to see sub house bills from the consol screen"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						false
						);
				});
			}
		}

		#endregion

		#region EnableSendingForwardingConsolToTWHAsynchronously

		public BooleanRegistryItem EnableSendingForwardingConsolToTWHAsynchronously
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableSendingForwardingConsolToTWHAsynchronously", delegate
				{
					return new BooleanRegistryItem(
						"EnableSendingForwardingConsolToTWHAsynchronously",
						Categories.Freight_Consolidations,
						ResString.GetMultilingualString("f2019a10-221f-4852-9858-dc3faf2aaca7", "Enable Sending Forwarding Consol to Transit Warehouse Asynchronously"),
						ResString.GetMultilingualString("11b5fe37-8f40-41d6-8fec-06cc6a97a94a", "Specify the default value for sending forwarding consol to Transit Warehouse Asynchronously."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		#endregion

		#region Cargo Only Voyage

		public BooleanRegistryItem CargoOnlyVoyageDefault
		{
			get
			{
				return GetItem<BooleanRegistryItem>("CargoOnlyVoyageDefault", delegate
				{
					return new BooleanRegistryItem(
						"CargoOnlyVoyageDefault",
						Categories.Freight_Consolidations,
						ResString.GetMultilingualString("2040e564-657c-496c-90b2-1d6b53e443f4", "Cargo Only Default"),
						ResString.GetMultilingualString("3f292906-cb08-45c8-81e2-612cf939b068", "Specify the default value for the Cargo Only indicator."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#region Validation Of Sending/Receiving Agents

		public BooleanRegistryItem SuppressValidationOfConsolsSendingOrReceivingAgents
		{
			get
			{
				return GetItem<BooleanRegistryItem>("SuppressValidationOfConsolsSendingOrReceivingAgents", delegate
				{
					return new BooleanRegistryItem(
						"SuppressValidationOfConsolsSendingOrReceivingAgents",
						Categories.Freight_Consolidations,
						ResString.GetMultilingualString("cd366323-9b7f-4242-bf74-08a1e0bc3a7c", "Validation Of Sending/Receiving Agents"),
						ResString.GetMultilingualString("9f54dbcd-b35f-44a2-bd1d-dca426ab6552",
							"Override this value if you would like to suppress validations of Sending Agent and Receiving Agent organizations against their Freight Handling settings. Recommended practice is to use the default registry value and validate Sending and Receiving Agents against their published, handling or appointed agent details, transport modes and locations."),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Canada Consolidation CargoControlNumbers

		public CodePairRegistryItem CanadaConsolCargoControlNumberCustomization
		{
			get
			{
				return GetItem<CodePairRegistryItem>("CanadaConsolCargoControlNumberCustomization", delegate
				{
					var listProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair(Constants.ConsolidationCCNCustomizationTypes.Code.Non, Constants.ConsolidationCCNCustomizationTypes.Description.Non);
						list.AddPair(Constants.ConsolidationCCNCustomizationTypes.Code.MasterBill, Constants.ConsolidationCCNCustomizationTypes.Description.MasterBill);
						return list;
					});

					return new CodePairRegistryItem(
						"CanadaConsolCargoControlNumberCustomization",
						Categories.Freight_Consolidations_Canada,
						ResString.GetMultilingualString("59d2b35a-a586-4455-812b-a39dc5d6281f", "Cargo Control Number Customization"),
						ResString.GetMultilingualString("884ca0b4-3466-41ca-8b1e-e1c3fe825e15", "If MBL is selected then the CCN on AIR consols will be defaulted to the MAWB and other transport modes will be the carrier code from the carrier organization + the master bill number."),
						listProvider,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Constants.ConsolidationCCNCustomizationTypes.Code.Non);
				});
			}
		}

		public CodePairRegistryItem CanadaConsolPreviousCargoControlNumberCustomization
		{
			get
			{
				return GetItem<CodePairRegistryItem>("CanadaConsolPreviousCargoControlNumberCustomization", delegate
				{
					var listProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair(Constants.ConsolidationCCNCustomizationTypes.Code.Non, Constants.ConsolidationCCNCustomizationTypes.Description.Non);
						list.AddPair(Constants.ConsolidationCCNCustomizationTypes.Code.CarrierCode, Constants.ConsolidationCCNCustomizationTypes.Description.CarrierCode);
						list.AddPair(Constants.ConsolidationCCNCustomizationTypes.Code.CCN, Constants.ConsolidationCCNCustomizationTypes.Description.CCN);
						return list;
					});

					return new CodePairRegistryItem(
						"CanadaConsolPreviousCargoControlNumberCustomization",
						Categories.Freight_Consolidations_Canada,
						ResString.GetMultilingualString("8348a007-37e2-4425-be6e-312ebc7787c7", "Previous Cargo Control Number Customization"),
						ResString.GetMultilingualString("dab39206-e81e-44bb-a241-107453a856a8", "The PCN may default to the carrier code to then be completed, or the CCN."),
						listProvider,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Constants.ConsolidationCCNCustomizationTypes.Code.CarrierCode);
				});
			}
		}

		#endregion

		#region Enable Goods Value for Shipping Instruction

		public BooleanRegistryItem EnableGoodsValueForShippingInstruction
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableGoodsValueForShippingInstruction", delegate
				{
					return new BooleanRegistryItem(
						"EnableGoodsValueForShippingInstruction",
						Categories.Freight_Consolidations_OceanCarrierMessaging,
						ResString.GetMultilingualString("d997eb11-fb7c-dd88-497b-9cf6e02ee764", "Shipping Instruction Goods Value"),
						ResString.GetMultilingualString("e1c16467-6a25-fbaf-4835-076bd5261ebe", "Override this setting if you would like to remove Goods Value from Carrier Shipping Instruction. Do you want to show Goods Value in Shipping Instruction?"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#region Enable SCMTR Date for Shipping Instruction

		public DateTimeRegistryItem SCMTREnableDate
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("SCMTREnableDate", delegate
				{
					return new DateTimeRegistryItem(
						"SCMTREnableDate",
						Categories.Freight_Consolidations_OceanCarrierMessaging,
						ResString.GetMultilingualString("518F84FD-0EB6-41E0-86F3-C4DD88A81256", "Enable SCMTR Date"),
						ResString.GetMultilingualString("D00312C5-C66D-4023-B303-A319ED9348B9", "This registry setting controls when the India Customs' Sea Cargo Manifest and Transshipment Regulations (SCMTR) will take effect. After this date the validations will be added to the Shipping Instruction for cargo originating from, destined to, or transiting through India."),
						RegistryStorageFlags.System,
						new DateTime(2022, 1, 1, 0, 0, 0));
				});
			}
		}

		#endregion

		#region Enable Package Grouping

		public BooleanRegistryItem EnablePackageGrouping
		{
			get
			{
				var caption = ResString.GetMultilingualString("2f2f3bf3-8bef-4699-b07d-57f65fd281bd", "Enable Package Grouping");
				var hint = ResString.GetMultilingualString("44ac64fc-4308-41f1-a50e-64794be5dd7f", @"Enable package grouping functionality on Booking Request, Shipping Order (China) & Shipping Instruction messages.

					Note: Overriding this registry to 'No' may result in rejection of your electronic messages.
					This registry will be set to 'Yes' and read-only from 31/01/2025.");

				if (ZDateTime.Today > new ZDateTime(2025, 01, 31))
				{
					return GetItem("EnablePackageGroupingV2", delegate
					{
						return new BooleanRegistryItem(
							"EnablePackageGroupingV2",
							Categories.Freight_Consolidations_OceanCarrierMessaging,
							caption,
							hint,
							RegistryStorageFlags.System,
							RegistryOptions.IsReadOnly,
							true);
					});
				}

				return GetItem("EnablePackageGrouping", delegate
				{
					return new BooleanRegistryItem(
						"EnablePackageGrouping",
						Categories.Freight_Consolidations_OceanCarrierMessaging,
						caption,
						hint,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						true);
				});
			}
		}

		#endregion

		#region Allow Company Tax ID Override

		public BooleanRegistryItem AllowCompanyTaxIDOverride
		{
			get
			{
				return GetItem("AllowCompanyTaxIDOverride", delegate
				{
					return new BooleanRegistryItem(
						"AllowCompanyTaxIDOverride",
						Categories.Freight_Consolidations_OceanCarrierMessaging,
						ResString.GetMultilingualString("f04aff78-d890-4883-8121-f94f60915902", "Allow Company Tax ID Override"),
						ResString.GetMultilingualString("85214d3d-8855-41bb-a5cb-2dcc0b14784a", "Enable this setting to allow company Tax ID override on the Shipping Instruction form.\r\n" +
							"Important: invalid override may result in a rejection from the carrier."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Enable Ocean Carrier Messaging Connection Validation

		public BooleanRegistryItem EnableOceanCarrierMessagingConnectionValidation
		{
			get
			{
				return GetItem("EnableOceanCarrierMessagingConnectionValidation", delegate
				{
					return new BooleanRegistryItem(
						"EnableOceanCarrierMessagingConnectionValidation",
						Categories.Freight_Consolidations_OceanCarrierMessaging,
						(NoResString)"Enable Ocean Carrier Messaging Connection Validation",
						(NoResString)"Override this setting, if you would like to enable ocean carrier messaging connection validation.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public BooleanRegistryItem EnableCarrierMessagingConnectionValidationTestUrl
		{
			get
			{
				return GetItem("EnableCarrierMessagingConnectionValidationTestUrl", delegate
				{
					return new BooleanRegistryItem(
						"EnableCarrierMessagingConnectionValidationTestUrl",
						Categories.Freight_Consolidations_OceanCarrierMessaging,
						(NoResString)"Enable Ocean Carrier Messaging Connection Test Url",
						(NoResString)"Override this setting to choose between production/test Ocean Carrier Messaging Connection Url.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public IntRegistryItem CarrierMessagingConnectionValidationTimeout
		{
			get
			{
				return GetItem("CarrierMessagingConnectionValidationTimeout", delegate
				{
					return new IntRegistryItem(
						"CarrierMessagingConnectionValidationTimeout",
						Categories.Freight_Consolidations_OceanCarrierMessaging,
						ResString.GetMultilingualString("55af8564-1777-4007-a2f3-50bb1f016f30", "The timeout in seconds for Ocean Carrier Route Validation service"),
						ResString.GetMultilingualString("7cfd4652-fa82-4806-8f88-6ef048c78d5d", "Override this setting to set timeout when connecting to Ocean Carrier Route Validation service."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						30,
						5,
						100);
				});
			}
		}

		#endregion

		#region OSMG

		public BooleanRegistryItem ConsolAllowAccessRegardlessOfShipmentsOSMGRights
		{
			get
			{
				return GetItem("ConsolAllowAccessRegardlessOfShipmentsOSMGRights", () =>
				{
					return new BooleanRegistryItem(
						"ConsolAllowAccessRegardlessOfShipmentsOSMGRights",
						Categories.Freight_Consolidations,
						ResString.GetMultilingualString("a50f6b2f-743d-499b-a7c8-69a83113af2b", "Allow access to consol regardless of shipments OSMG rights"),
						ResString.GetMultilingualString("a9e7a484-a846-4d8a-9f3e-64fa7137f140", @"When this registry setting is set to ' Yes', the OSMG security rights on shipments will be ignored while accessing a consolidation.
When set to 'No', a consolidation will be accessible only if the user has rights to access all of its related shipments."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#endregion

		#region Shipment

		#region SEC Event

		public BooleanRegistryItem EnableEnhancedSECEvent
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableEnhancedSECEvent", delegate
				{
					return new BooleanRegistryItem(
						"EnableEnhancedSECEvent",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("1d9b432d-bab1-411e-be51-6e7220f032db", "Enable Enhanced SEC Events"),
						ResString.GetMultilingualString("b5b41aec-f2c4-427a-8fbc-3b6e8e4af8b8", "Import XML changes should only apply when this registry is enabled. Workflow(TWH) should use the registry. Fallback to existing behavior if registry is disabled. Workflow(GUI) should remove the registry and the fallback from Workflow(UXML) and Workflow(TWH)."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region Visibility Event Delivery

		public BooleanRegistryItem EnableVisibilityEventDelivery
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableVisibilityEventDelivery", delegate
				{
					return new BooleanRegistryItem(
						"EnableVisibilityEventDelivery",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("25960607-a46d-4c08-bab1-3bcfecada5c8", "Enable Visibility Event Delivery"),
						ResString.GetMultilingualString("ffc42aa8-180e-44ee-a9ba-9551cbdb4701", "Enable Visibility Event Delivery if set to true"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region Default Rate Origin/Destination

		public BooleanRegistryItem DefaultRateOriginDestination
		{
			get
			{
				return GetItem<BooleanRegistryItem>("DefaultRateOriginDestination", delegate
				{
					return new BooleanRegistryItem(
						"DefaultRateOriginDestination",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("19b513bb-1469-48fa-890a-25011dd17016", "Default Rate Origin/Destination"),
						ResString.GetMultilingualString("ef49d20d-0e70-4292-9c22-6d03a9f82400", "Enable this option to default values of Rate Origin and Rate Destination fields from the Related City/Port of the corresponding Shipment > Pickup > CFS and Shipment > Delivery > CFS addresses."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region Calculate Delivery Due Date By Transport Mode

		public CalculateDeliveryDueDateOptionsRegistryItem CalculateDeliveryDueDateByTransportMode
		{
			get
			{
				return GetItem("CalculateDeliveryDueDateByTransportMode", delegate
				{
					var defaultList = new CalculateDeliveryDueDateOptions(new CalculateDeliveryDueDateTransportModeList(RegistryFactory.Instance).GetDefaultCalculateDeliveryDueDateOptions()) { IsActive = false };

					return new CalculateDeliveryDueDateOptionsRegistryItem(
						"CalculateDeliveryDueDateByTransportMode",
						Categories.Freight_Shipment_DeliveryDueDate,
						ResString.GetMultilingualString("ADD9E11B-358D-4231-A360-0F5402A2EB5A", "Calculate Delivery Due Date"),
						ResString.GetMultilingualString("D09891D2-D487-4E94-9D63-2250853FD31A", "Enable Delivery Due Date calculation on Bookings and Shipments."),
						RegistryStorageFlags.System,
						ObjectFactory.Get<IDeliveryDueDateFeatureControlHelper>().Enabled ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
						defaultList);
				});
			}
		}

		#endregion

		#region Calculate Delivery Date With Exceptions

		public CalculateDeliveryDateWithExceptionsOptionsRegistryItem CalculateDeliveryDateWithExceptions
		{
			get
			{
				return GetItem("CalculateDeliveryDateWithExceptions", () =>
					new CalculateDeliveryDateWithExceptionsOptionsRegistryItem(
					"CalculateDeliveryDateWithExceptions",
					Categories.Freight_Shipment_DeliveryDueDate,
					ResString.GetMultilingualString("BDF9E11B-354D-4231-A360-0F5401A2EB51", "Calculate Delivery Date with Exceptions"),
					ResString.GetMultilingualString("CDD9E11B-351D-4231-A360-0F5405A2EB52", "Override this registry to specify the maximum combined delay duration per calendar day."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					ObjectFactory.Get<IDeliveryDueDateFeatureControlHelper>().Enabled ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
					new CalculateDeliveryDateWithExceptionsOptions { UnlimitedDuration = ZBool.True }));
			}
		}

		#endregion

		#region Default Shipment Origin from Consol Load

		public BooleanRegistryItem DefaultShipmentOriginFromConsolLoad
		{
			get
			{
				return GetItem<BooleanRegistryItem>("DefaultShipmentOriginFromConsolLoad", delegate
				{
					return new BooleanRegistryItem(
						"DefaultShipmentOriginFromConsolLoad",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("fce152f8-1131-4e8c-86e4-23d78dfde45b", "Default Shipment Origin from Consol Load"),
						ResString.GetMultilingualString("29e58bc1-4daa-4f83-a1e3-805ba147524f", "Enable this option to default Shipment Origin from Consol Load."),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region Default Shipment Destination from Consol Discharge

		public BooleanRegistryItem DefaultShipmentDestinationFromConsolDischarge
		{
			get
			{
				return GetItem<BooleanRegistryItem>("DefaultShipmentDestinationFromConsolDischarge", delegate
				{
					return new BooleanRegistryItem(
						"DefaultShipmentDestinationFromConsolDischarge",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("99287c18-ef46-4e84-933c-58ebfbe267ed", "Default Shipment Destination from Consol Discharge"),
						ResString.GetMultilingualString("59e4a522-125f-477f-b258-31185f542512", "Enable this option to default Shipment Destination from Consol Discharge."),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region Shipment Custom Attributes

		class ShipmentCustomAttributeRegistryItem : CaptionAndHintRegistryItem
		{
			public ShipmentCustomAttributeRegistryItem(string name, MultilingualString caption)
				: base(name,
				FreightDataRegistry.Categories.Freight_Shipment_CustomAttributes,
				caption,
				ResString.GetMultilingualString("2a3ba021-e35f-4059-a447-75c8c0eaae2e", "A user defined field that is available on the shipment and declaration screens."),
				RegistryStorageFlags.System,
				new CaptionAndHint("",
					string.Format((NoResString)"Custom defined column caption can be changed in the Registry, under {0}/Custom Attributes.", FreightDataRegistry.Categories.Freight_Shipment_CustomAttributes.GetUnresolvedString())))
			{
			}
		}

		#region Text 1

		public CaptionAndHintRegistryItem ShipmentCustomText1
		{
			get
			{
				return GetCustomField("ShipmentCustomText1", ResString.GetMultilingualString("c7184a82-a4fe-4210-b69b-98f67c6c43b8", "Text 1"), RegistryOptions.PreserveTestValue);
			}
		}

		#endregion

		#region Text 2

		public CaptionAndHintRegistryItem ShipmentCustomText2
		{
			get
			{
				return GetCustomField("ShipmentCustomText2", ResString.GetMultilingualString("74d17e20-7bfc-4943-9003-999d2d55c5ce", "Text 2"), RegistryOptions.PreserveTestValue);
			}
		}

		#endregion

		#region Date 1

		public CaptionAndHintRegistryItem ShipmentCustomDate1
		{
			get
			{
				return GetCustomField("ShipmentCustomDate1", ResString.GetMultilingualString("e6e410d9-cea5-40ca-9032-455c19d1ff46", "Date 1"), RegistryOptions.PreserveTestValue);
			}
		}

		#endregion

		#region Date 2

		public CaptionAndHintRegistryItem ShipmentCustomDate2
		{
			get
			{
				return GetCustomField("ShipmentCustomDate2", ResString.GetMultilingualString("5c1ea943-eef7-4eed-9aa2-60157074c303", "Date 2"), RegistryOptions.PreserveTestValue);
			}
		}

		#endregion

		#region Decimal No. 1

		public CaptionAndHintRegistryItem ShipmentCustomDecimalNo1
		{
			get
			{
				return GetCustomField("ShipmentCustomDecimalNo1", ResString.GetMultilingualString("06d6f3fd-1cbd-46e0-8ce1-a5aeb03f2b92", "Decimal No. 1"), RegistryOptions.PreserveTestValue);
			}
		}

		#endregion

		#region Decimal No. 2

		public CaptionAndHintRegistryItem ShipmentCustomDecimalNo2
		{
			get
			{
				return GetCustomField("ShipmentCustomDecimalNo2", ResString.GetMultilingualString("99939413-d8ce-496c-af6f-cd224674eb4c", "Decimal No. 2"), RegistryOptions.PreserveTestValue);
			}
		}

		#endregion

		#region Flag 1

		public CaptionAndHintRegistryItem ShipmentCustomFlag1
		{
			get
			{
				return GetCustomField("ShipmentCustomFlag1", ResString.GetMultilingualString("b4946c96-465b-4d4d-96ce-66550bdffb59", "Flag 1"), RegistryOptions.PreserveTestValue);
			}
		}

		#endregion

		#region Flag 2

		public CaptionAndHintRegistryItem ShipmentCustomFlag2
		{
			get
			{
				return GetCustomField("ShipmentCustomFlag2", ResString.GetMultilingualString("d9238c5a-e2c0-4ef8-90cc-859e79936f50", "Flag 2"), RegistryOptions.PreserveTestValue);
			}
		}
		#endregion

		CaptionAndHintRegistryItem GetCustomField(string key, MultilingualString name, RegistryOptions options)
		{
			var captionAndHint = GetItem<CaptionAndHintRegistryItem>(key, delegate
			{
				return new ShipmentCustomAttributeRegistryItem(key, name);
			});
			captionAndHint.Value.CaptionMaxLength = GenCustomAddOnValueSchema.XV_Name.MaxLength;
			captionAndHint.DefaultValue.CaptionMaxLength = GenCustomAddOnValueSchema.XV_Name.MaxLength;
			captionAndHint.Options = options;
			return captionAndHint;
		}
		#endregion

		#region Sendingarnumer

		bool IsCurrentCompanyIceland
		{
			get
			{
				return Env.CurrentCompany?.Country?.Code == Core.Constants.CountryCodes.Iceland;
			}
		}

		public StringRegistryItem AirExpressShipmentServiceLevelCode
		{
			get
			{
				return GetItem<StringRegistryItem>("AirExpressShipmentServiceLevelCodeSendingarnumer", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"AirExpressShipmentServiceLevelCodeSendingarnumer",
						Categories.Freight_Shipment_Sendingarnumer,
						ResString.GetMultilingualString("cfe76438-ffbb-440c-8b68-a4d07e88710b", "Air Express Service Level Code"),
						ResString.GetMultilingualString("17eb3edc-9fe6-483d-946e-c6cf9bb2a3ab", "This specifies the service level code used on a Consol to signify that the consol is an Air Express movement. This service level needs to be set up on the relevant carrier's organization under the 'Carrier Service Levels' section."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						!IsCurrentCompanyIceland ? RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue : RegistryOptions.PreserveTestValue,
						"EXP");
					return result;
				});
			}
		}

		public DecimalRegistryItem AirExpressShipmentBreakValueInEuro
		{
			get
			{
				return GetItem<DecimalRegistryItem>("AirExpressShipmentBreakValueInEuro", delegate
				{
					return new DecimalRegistryItem(
						"AirExpressShipmentBreakValueInEuro",
						Categories.Freight_Shipment_Sendingarnumer,
						ResString.GetMultilingualString("ffd37107-4bc2-4693-8f46-fe04dc5c930d", "Air Express Shipment Break Value In Euro"),
						ResString.GetMultilingualString("fd543df0-26bf-45ea-bec1-8e682e2e5bf6", @"Set the amount in Euros for the goods value threshold. This threshold is used to generate the Sendingarnumer’s GGGG segment which indicates the Express Carrier Code.
If the goods value is less than the amount specified here, the GGGG number is always H002.
If the goods value is equal to or more than the amount specified here, the GGGG number is ""H"" followed by a sequential number starting at ""004""."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						!IsCurrentCompanyIceland ? RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue : RegistryOptions.PreserveTestValue,
						22);
				});
			}
		}

		public StringRegistryItem ForwarderSendingarnumerCodeForSeafreight
		{
			get
			{
				return GetItem<StringRegistryItem>("ForwarderSendingarnumerCodeForSeafreight", delegate
				{
					return new StringRegistryItem(
						"ForwarderSendingarnumerCodeForSeafreight",
						Categories.Freight_Shipment_Sendingarnumer,
						ResString.GetMultilingualString("b9bd81ed-40f9-4add-95c8-dd88868abe77", "Forwarder Sendingarnumer Code For Sea freight"),
						ResString.GetMultilingualString("9fd1f894-f7c9-4824-ad89-0a4db4df80e7", "Specifies the forwarder code for sea freight."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						!IsCurrentCompanyIceland ? RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue : RegistryOptions.PreserveTestValue,
						"J");
				});
			}
		}

		public StringRegistryItem ForwarderSendingarnumerCodeForAirfreight
		{
			get
			{
				return GetItem<StringRegistryItem>("ForwarderSendingarnumerCodeForAirfreight", delegate
				{
					return new StringRegistryItem(
						"ForwarderSendingarnumerCodeForAirfreight",
						Categories.Freight_Shipment_Sendingarnumer,
						ResString.GetMultilingualString("66a114fc-595f-43e6-8c69-7214e51ccf43", "Forwarder Sendingarnumer Code For Air freight"),
						ResString.GetMultilingualString("8cdd3c68-0e34-42e0-ade2-be271dc1b822", "Specifies the forwarder code for air freight."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						!IsCurrentCompanyIceland ? RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue : RegistryOptions.PreserveTestValue,
						"W");
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public IntRegistryItem LastSequenceNumberForMultilineConsols
		{
			get
			{
				return GetItem<IntRegistryItem>("LastSequenceNumberForMultilineConsols", delegate
				{
					return new IntRegistryItem(
						"LastSequenceNumberForMultilineConsols",
						Categories.Freight_Shipment_Sendingarnumer,
						(NoResString)"Last Sequence Number For Multi-line Consols",
						(NoResString)"This is hidden item, which store last sequence number for multi-line consols",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue,
						1);
				});
			}
		}

		#endregion

		#endregion

		#region Default Delivery When Delay Is Not Set

		public BooleanRegistryItem DefaultDeliveryWhenDelayIsNotSet
		{
			get
			{
				return GetItem<BooleanRegistryItem>("DefaultDeliveryWhenDelayIsNotSet", delegate
				{
					return new BooleanRegistryItem(
						"DefaultDeliveryWhenDelayIsNotSet",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("3da864d0-c838-4ca6-9109-a92481d0ff5f", "Default Delivery When Delay Is Not Set"),
						ResString.GetMultilingualString("248af1cc-7e82-4378-87f9-9b180336855e", "The Estimated Delivery on Shipments and Declarations is set from the Est. Delay Until Delivery on the Port Default Delivery Time record.  When yes, the Estimated Delivery will always default from the Port Default Delivery Time record.  When no, the Estimated Delivery will not default from the Port Default Delivery Time record if the Est. Delay Until Delivery is 0."),
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		#endregion

		#region Storage Number Customisation

		public BillCustomisationRegistryItem StorageNumberCustomisation
		{
			get
			{
				return GetItem<BillCustomisationRegistryItem>("StorageNumberCustomisation", delegate
				{
					BillCustomisationRegistryDataType dataType = new BillCustomisationRegistryDataType();
					dataType.FountainPrefix = null;
					dataType.GeneratedNumberName = ResString.GetMultilingualString("b199f563-db92-4c75-bb09-08d53d912a7a", "Storage Number");
					dataType.SequenceNumberName = ResString.GetMultilingualString("A98E42E1-6C9A-4D02-9C0E-EC4CB18A8A91", "Shipment");
					dataType.MaxLength = 15;
					dataType.AllowNonAlphanumericCharacters = true;
					dataType.EnableMacroInsertion = false;

					return new BillCustomisationRegistryItem(
						"StorageNumberCustomisation",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("88a0214f-701c-45a7-9e7d-500f9d190d2e", "Storage Number Customization"),
						ResString.GetMultilingualString("56662848-0801-462c-b3c7-822488f49dee", "Override this value to customize how storage numbers are formatted."),
						RegistryStorageFlags.All,
						dataType
						);
				});
			}
		}

		#endregion

		#region Storage Number Button Activation

		public BooleanRegistryItem StorageNumberButtonActivation
		{
			get
			{
				return GetItem<BooleanRegistryItem>("StorageNumberButtonActivation", delegate
				{
					return new BooleanRegistryItem(
						"StorageNumberButtonActivation",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("e8a77cc6-97fd-430f-a05e-bf6421773065", "Storage Number Button Activation"),
						ResString.GetMultilingualString("1e4a3b41-0aa0-448f-9e21-2ce5fddb390e", "Use this property to activate generate storage number button on eDocs tab on the shipment form."),
						RegistryStorageFlags.All,
						false);
				});
			}
		}

		#endregion

		#region Brokerage Job Creation

		public BooleanRegistryItem CreateBrokerageJobAutomatically
		{
			get
			{
				return GetItem<BooleanRegistryItem>("CreateBrokerageJobAutomatically", delegate
				{
					return new BooleanRegistryItem(
						"CreateBrokerageJobAutomatically",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("f171d017-2a7a-4abc-86fe-ba883e38b9d4", CreateBrokerageJobAutomaticallyCaption),
						ResString.GetMultilingualString("79801DFA-5C2A-4A77-ACF9-CB228E49DF24", CreateBrokerageJobAutomaticallyCaptionHint),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Is going to be used in Resource string")]
		internal const string CreateBrokerageJobAutomaticallyCaption = "Create Brokerage Job Automatically";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Is going to be used in Resource string")]
		internal const string CreateBrokerageJobAutomaticallyCaptionHint = "Set to Yes if you want to remove the display of the pop up window that asks 'Are you sure you want to create a declaration now?' when creating brokerage jobs in shipments.";

		#endregion

		#region Consignor / Shipper Terminology

		public MultilingualStringRegistryItem ConsignorShipperTerminology
		{
			get
			{
				ResourceString defaultTerminology = Env.CurrentCompany?.Country?.Code == Constants.CountryCodes.UnitedStates ||
					Env.CurrentCompany?.Country?.Code == Constants.CountryCodes.Canada ?
						ResString.GetMultilingualString("Freight.Shipper", "Shipper") :
						ResString.GetMultilingualString("Freight.Consignor", "Consignor");
				return GetItem<MultilingualStringRegistryItem>("ConsignorShipperTerminology", delegate
				{
					return new MultilingualStringRegistryItem(
						"ConsignorShipperTerminology",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("4395a205-e5e6-473c-bf39-c12dab25ce3e", "Consignor / Shipper Terminology"),
						ResString.GetMultilingualString("91507f70-533f-42a5-9c61-34f414269c57", "By setting this, all references to the consignor / shipper will appear as per this setting."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue,
						defaultTerminology);
				});
			}
		}

		#endregion

		#region Export Statement Settings

		public ExportStatementSettingRegistryItem ExportStatementSettings
		{
			get
			{
				return GetItem<ExportStatementSettingRegistryItem>("ExportStatementSetting", delegate
				{
					return new ExportStatementSettingRegistryItem(
						"ExportStatementSetting",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("988b76b4-c1da-4ab6-9f63-fc3bbf4b42c3", "Export Statements"),
						ResString.GetMultilingualString("cc4d7e82-f259-48ad-8d55-7237e5cac302", "The different Statements to be shown on certain documents like House Bill or Manifest"),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						GetExportStatementSettingDefaultValues());
				});
			}
		}

		CountryExportStatementSettingCollection GetExportStatementSettingDefaultValues()
		{
			var defaultValue = new CountryExportStatementSettingCollection();
			Constants.CountryCodes.UsaAndTerritoriesList.ForEach(c => defaultValue.AddDefaultValues(c));
			return defaultValue;
		}

		#endregion

		#region Export Destination Value In UXML

		public BooleanRegistryItem ExportDestinationValueInUXML
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ExportDestinationValueInUXML", delegate
				{
					return new BooleanRegistryItem(
						"ExportDestinationValueInUXML",
						Categories.Freight_Shipment,
						 (NoResString)"Export Destination Value In UXML",
						 (NoResString)"If turned on, Destination Good Value, Currency and Destination Exchange Rate calculated on Shipment will be exported into Universal Shipment XML.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region Supply Chain Security

		public CodePairRegistryItem ShipmentInspectionTypeDefault
		{
			get
			{
				return GetItem<CodePairRegistryItem>("ShipmentInspectionTypesDefault", delegate
				{
					ResourceString hint = ResString.GetMultilingualString("3a616080-f437-4571-ae0f-a8198b5a1692", "This is the default value for the Shipment and Pack Line Inspection Type, when consignor/shipper is not known/approved.\r\n\r\n"
						+ "This registry does not apply to countries with a licensed Supply Chain Security module that have their own configuration for inspection type for unknown organizations.");

					return new CodePairRegistryItem(
						"ShipmentInspectionTypesDefault",
						Categories.Freight_SupplyChainSecurity,
						ResString.GetMultilingualString("499deda3-39e7-413c-9d15-a56262cf9b20", "Inspection Type Default for Unknown Organizations"),
						hint,
						AviationSecurityDefaultListProvider,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						AviationSecurity_Unknown_Code);
				});
			}
		}

		public DateTimeRegistryItem EXMExemptionCodeRemovalDate
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("EXMExemptionCodeRemovalDate", delegate
				{
					return new DateTimeRegistryItem(
						"EXMExemptionCodeRemovalDate",
						Categories.Freight_SupplyChainSecurity,
						ResString.GetMultilingualString("629a6bb3-be1a-486c-9c18-0039ee1b7ab5", "EXM Exemption Code Removal Date"),
						ResString.GetMultilingualString("d3ac8617-f79f-4780-ba6e-005cc6918de6", "Indicates the date on which the EXM Exemption Code was removed from CW1.\r\n\r\n" +
						"Shipments created before this date that still contain the EXM code will not be affected by validation."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsHidden);
				});
			}
		}

		ICodeDescriptionPairListProvider AviationSecurityDefaultListProvider
		{
			get
			{
				return new CodeDescriptionPairListProvider(() =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair("");
					foreach (ShipmentInspectionType pair in ShipmentInspectionTypesDefaultList.Types)
					{
						list.AddPair(pair.Code, pair.Description);
					}
					list.AddPair(AviationSecurity_Unknown_Code, AviationSecurity_Unknown_Description);
					return list;
				});
			}
		}

		public const string AviationSecurity_Unknown_Code = "UNK";
		public static MultilingualString AviationSecurity_Unknown_Description
		{
			get { return ResString.GetMultilingualString("6d173a9a-dc48-4a1c-84ee-81406c2a308e", "Unknown - No Security Measures Taken"); }
		}

		public const string AviationSecurity_PackLine_IsSecured_Code = "APP";
		public static MultilingualString AviationSecurity_PackLine_IsSecured_Description
		{
			get { return ResString.GetMultilingualString("56eeddf1-2330-458c-8a67-6e218bc5afc3", "Approved"); }
		}

		public CodePairRegistryItem ShipmentInspectionOrganisationToUse
		{
			get
			{
				return GetItem<CodePairRegistryItem>("ShipmentInspectionOrganisationToUse", delegate
				{
					ResourceString hint = ResString.GetMultilingualString("74361397-618a-4c06-9e56-78e6c1c318ce", "This Registry allows you to configure at a Company level, which organization, the Consignor/Shipper or the Local Client from the Billing tab, should be used for the calculation of Known/Approved security inspection status on Shipments.\r\n\r\n"
						+ "This registry does not apply to countries with a licensed Supply Chain Security module that have their own configuration for \"Organizations to Use for Supply Chain Security\".");

					return new CodePairRegistryItem(
						"ShipmentInspectionOrganisationToUse",
						Categories.Freight_SupplyChainSecurity,
						ResString.GetMultilingualString("3de6e6dd-b4c1-4873-b49c-f35909738373", "Organizations To Use for Supply Chain Security"),
						hint,
						AviationSecurityOrganisationListProvider,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						SupplyChainSecurityOrganisationTypes.Consignor);
				});
			}
		}

		ICodeDescriptionPairListProvider AviationSecurityOrganisationListProvider
		{
			get
			{
				return new CodeDescriptionPairListProvider(() =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(SupplyChainSecurityOrganisationTypes.Consignor, SupplyChainSecurityOrganisationTypes.GetDescription(SupplyChainSecurityOrganisationTypes.Consignor));
					list.AddPair(SupplyChainSecurityOrganisationTypes.LocalClient, SupplyChainSecurityOrganisationTypes.GetDescription(SupplyChainSecurityOrganisationTypes.LocalClient));
					return list;
				});
			}
		}

		public ShipmentInspectionTypeRegistryItem ShipmentInspectionTypeRegistryItem
		{
			get
			{
				return GetItem<ShipmentInspectionTypeRegistryItem>("ShipmentInspectionTypes", delegate
				{
					ResourceString hint = ResString.GetMultilingualString("eaa01866-6337-4204-b8c0-e98284e42350", "A list of possible methods of Shipment Inspection, where the Shipper is not a known/approved shipper.\r\n\r\n"
						+ "This registry does not apply to countries with a licensed Supply Chain Security module that have their own configuration for inspection types.");

					ShipmentInspectionTypeRegistryItem result = new ShipmentInspectionTypeRegistryItem(
						"ShipmentInspectionTypes",
						Categories.Freight_SupplyChainSecurity,
						ResString.GetMultilingualString("0b701a9e-308b-4784-b724-e37d49aef482", "Default IATA Inspection Types"),
						hint,
						RegistryStorageFlags.System,
						ShipmentInspectionTypesDefaultList);

					return result;
				});
			}
		}

		ShipmentInspectionTypes ShipmentInspectionTypesDefaultList
		{
			get
			{
				if (shipmentInspectionTypesDefaultList == null)
				{
					shipmentInspectionTypesDefaultList = new ShipmentInspectionTypes(ZString.Empty, new ShipmentInspectionTypeLists(RegistryFactory.Instance).SystemDefinedList_IATA);
				}

				return shipmentInspectionTypesDefaultList;
			}
		}
		ShipmentInspectionTypes shipmentInspectionTypesDefaultList;

		public StringRegistryItem ApprovedOrganisationRequiredDocType
		{
			get
			{
				return GetItem<StringRegistryItem>("ApprovedOrganisationRequiredDocType", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"ApprovedOrganisationRequiredDocType",
						Categories.Freight_SupplyChainSecurity,
						ResString.GetMultilingualString("7a5f856a-0bc2-4523-ab88-9fc6c3b76c8f", "Default Required Document Type for Approved Organization"),
						ResString.GetMultilingualString("2ce941b1-01a1-4db7-b273-305f044a5d99", "Select the Document Type that must be attached to an organization before flagging it as Known/Approved."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company);
					result.EditorInfo = new CodeFindBoxRegistryEditorInfo(ModuleIDs.RefDocType, GetDocTypeCollection);
					return result;
				});
			}
		}

		IBusinessObjectCollection GetDocTypeCollection(BusinessObjectFactory factory)
		{
			return (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IRefDocTypeCollection>(), factory);
		}

		#region AllAvailableShipmentInspectionOrganisations

		SupplyChainSecurityOrganisationToUseCollection GetShipmentInspectionOrganisations(ZString countryCode)
		{
			var allAvailableShipmentInspectionOrganisations = new SupplyChainSecurityOrganisationToUseCollection(countryCode);
			allAvailableShipmentInspectionOrganisations.Add(SupplyChainSecurityOrganisationTypes.Consignor, SupplyChainSecurityOrganisationTypes.GetDescription(SupplyChainSecurityOrganisationTypes.Consignor), SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes);
			allAvailableShipmentInspectionOrganisations.Add(SupplyChainSecurityOrganisationTypes.LocalClient, SupplyChainSecurityOrganisationTypes.GetDescription(SupplyChainSecurityOrganisationTypes.LocalClient), SupplyChainSecurityOrganisationToUse.ValidationCodes.No);

			if (countryCode != Constants.CountryCodes.HongKong)
			{
				allAvailableShipmentInspectionOrganisations.Add(SupplyChainSecurityOrganisationTypes.ShipmentPickupFrom, SupplyChainSecurityOrganisationTypes.GetDescription(SupplyChainSecurityOrganisationTypes.ShipmentPickupFrom), SupplyChainSecurityOrganisationToUse.ValidationCodes.No);
			}

			allAvailableShipmentInspectionOrganisations.Add(SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany, SupplyChainSecurityOrganisationTypes.GetDescription(SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany), SupplyChainSecurityOrganisationToUse.ValidationCodes.No);
			allAvailableShipmentInspectionOrganisations.Add(SupplyChainSecurityOrganisationTypes.ShipmentPackingCFS, SupplyChainSecurityOrganisationTypes.GetDescription(SupplyChainSecurityOrganisationTypes.ShipmentPackingCFS), SupplyChainSecurityOrganisationToUse.ValidationCodes.No);
			allAvailableShipmentInspectionOrganisations.Add(SupplyChainSecurityOrganisationTypes.CoLoadMasterShipmentSendingForwarder, SupplyChainSecurityOrganisationTypes.GetDescription(SupplyChainSecurityOrganisationTypes.CoLoadMasterShipmentSendingForwarder), SupplyChainSecurityOrganisationToUse.ValidationCodes.No);

			if (countryCode != Constants.CountryCodes.Australia)
			{
				allAvailableShipmentInspectionOrganisations.Add(SupplyChainSecurityOrganisationTypes.ConsolSendingAgent, SupplyChainSecurityOrganisationTypes.GetDescription(SupplyChainSecurityOrganisationTypes.ConsolSendingAgent), SupplyChainSecurityOrganisationToUse.ValidationCodes.No);
			}

			allAvailableShipmentInspectionOrganisations.Add(SupplyChainSecurityOrganisationTypes.ConsolAirline, SupplyChainSecurityOrganisationTypes.GetDescription(SupplyChainSecurityOrganisationTypes.ConsolAirline), SupplyChainSecurityOrganisationToUse.ValidationCodes.No);
			allAvailableShipmentInspectionOrganisations.Add(SupplyChainSecurityOrganisationTypes.ConsolTransportCompany, SupplyChainSecurityOrganisationTypes.GetDescription(SupplyChainSecurityOrganisationTypes.ConsolTransportCompany), SupplyChainSecurityOrganisationToUse.ValidationCodes.No);
			allAvailableShipmentInspectionOrganisations.Add(SupplyChainSecurityOrganisationTypes.ConsolCFS, SupplyChainSecurityOrganisationTypes.GetDescription(SupplyChainSecurityOrganisationTypes.ConsolCFS), SupplyChainSecurityOrganisationToUse.ValidationCodes.No);
			allAvailableShipmentInspectionOrganisations.Add(SupplyChainSecurityOrganisationTypes.ConsolCoLoadWithOrganization, SupplyChainSecurityOrganisationTypes.GetDescription(SupplyChainSecurityOrganisationTypes.ConsolCoLoadWithOrganization), SupplyChainSecurityOrganisationToUse.ValidationCodes.No);
			allAvailableShipmentInspectionOrganisations.Add(SupplyChainSecurityOrganisationTypes.ConsolForwarderYouHaveBorrowedMAWBStockFrom, SupplyChainSecurityOrganisationTypes.GetDescription(SupplyChainSecurityOrganisationTypes.ConsolForwarderYouHaveBorrowedMAWBStockFrom), SupplyChainSecurityOrganisationToUse.ValidationCodes.No);

			return allAvailableShipmentInspectionOrganisations;
		}

		#endregion

		#region Japan

		public ShipmentInspectionTypeRegistryItem ShipmentInspectionTypes_Japan
		{
			get
			{
				return GetItem<ShipmentInspectionTypeRegistryItem>("ShipmentInspectionTypes_JP", delegate
				{
					ShipmentInspectionTypes defaultValue = new ShipmentInspectionTypes(Constants.CountryCodes.Japan, new ShipmentInspectionTypeLists(RegistryFactory.Instance).SystemDefinedList_Japan);

					MultilingualString hint = EnableSupplyChainSecurity_JP.Value
					? ResString.GetMultilingualString("f4d6dde0-08fb-4055-84d4-4bbbba28e6dd", "A list of possible methods of Shipment Inspection for Japan, where the Shipper is not a known/approved shipper.")
					: ResString.GetMultilingualString("b16ef153-6d72-4d80-9652-8ac567f4acca", "A list of possible methods of Shipment Inspection for Japan, where the Shipper is not a known/approved shipper. The '{0}' registry item must be enabled in order to use this list.", EnableSupplyChainSecurity_JP.Caption);

					ShipmentInspectionTypeRegistryItem result = new ShipmentInspectionTypeRegistryItem(
						"ShipmentInspectionTypes_JP",
						Categories.Freight_SupplyChainSecurity_Japan,
						ResString.GetMultilingualString("94893db7-48cc-4cf6-95bd-6d01fc9b5a13", "Inspection Types"),
						hint,
						RegistryStorageFlags.System,
						defaultValue);
					return result;
				});
			}
		}

		public CodePairRegistryItem ShipmentInspectionTypeDefault_Japan
		{
			get
			{
				MultilingualString hint = EnableSupplyChainSecurity_JP.Value
					? ResString.GetMultilingualString("ee308f7f-3f88-4009-94c3-58044ea61d4c", "This is the default value for the Shipment and Pack Line Inspection Type, when consignor/shipper is not known/approved.")
					: ResString.GetMultilingualString("40f57cf5-f356-41d2-a295-02bf251ed5bf", "This is the default value for the Shipment and Pack Line Inspection Type, when consignor/shipper is not known/approved. The '{0}' registry item must be enabled in order to use this value.", EnableSupplyChainSecurity_JP.Caption);

				return GetItem<CodePairRegistryItem>("ShipmentInspectionTypesDefault_JP", delegate
				{
					return new CodePairRegistryItem(
						"ShipmentInspectionTypesDefault_JP",
						Categories.Freight_SupplyChainSecurity_Japan,
						ResString.GetMultilingualString("267e4891-447d-464a-b6c8-f7b483b89484", "Inspection Type Default for Unknown Organizations"),
						hint,
						AviationSecurityDefaultListProvider_Japan,
						RegistryStorageFlags.System,
						AviationSecurity_Unknown_Code);
				});
			}
		}

		ICodeDescriptionPairListProvider AviationSecurityDefaultListProvider_Japan
		{
			get
			{
				return new CodeDescriptionPairListProvider(() =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair("");
					foreach (ShipmentInspectionType pair in FreightDataRegistry.Instance.ShipmentInspectionTypes_Japan.Value.Types)
					{
						list.AddPair(pair.Code, pair.Description);
					}

					list.AddPair(AviationSecurity_Unknown_Code, AviationSecurity_Unknown_Description);

					return list;
				});
			}
		}

		public BooleanRegistryItem EnableSupplyChainSecurity_JP
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableSupplyChainSecurity_JP", delegate
				{
					return new BooleanRegistryItem(
						"EnableSupplyChainSecurity_JP",
						Categories.Freight_SupplyChainSecurity_Japan,
						ResString.GetMultilingualString("32db5cf3-1a12-4aa0-9db0-8604debb9dc0", "Enable Supply Chain Security for Japan"),
						ResString.GetMultilingualString("e3c72135-59dd-4fed-913e-5d4f1813658c", "Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country/region’s air export processes."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#region Australia

		public BooleanRegistryItem EnableSupplyChainSecurity_AU
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableSupplyChainSecurity_AU", delegate
				{
					return new BooleanRegistryItem(
						"EnableSupplyChainSecurity_AU",
						Categories.Freight_SupplyChainSecurity_AU,
						ResString.GetMultilingualString("ffaf5249-07c4-4edb-8121-ff540118688d", "Enable Supply Chain Security for Australia"),
						ResString.GetMultilingualString("f31d198c-bbc5-4db8-abc1-6ea64b92f5d4", "Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country/region’s air export processes."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public CodePairRegistryItem ShipmentInspectionTypeDefault_Australia
		{
			get
			{
				var hint = EnableSupplyChainSecurity_AU.Value
					? ResString.GetMultilingualString("7a487e52-7827-4ae6-bd9e-a8ad36112278", "This is the default value for the Shipment and Pack Line Inspection Type, when consignor/shipper is not known/approved.")
					: ResString.GetMultilingualString("72e0eced-0f55-4916-a65b-e91302c8ed62", "This is the default value for the Shipment and Pack Line Inspection Type, when consignor/shipper is not known/approved. The '{0}' registry item must be enabled in order to use this value.", EnableSupplyChainSecurity_AU.Caption);

				return GetItem<CodePairRegistryItem>("ShipmentInspectionTypesDefault_AU", delegate
				{
					return new CodePairRegistryItem(
						"ShipmentInspectionTypesDefault_AU",
						Categories.Freight_SupplyChainSecurity_AU,
						ResString.GetMultilingualString("d1da19c4-4cca-45e2-81ba-c353ba2f8d14", "Inspection Type Default for Unknown Organizations"),
						hint,
						AviationSecurityDefaultListProvider_Australia,
						RegistryStorageFlags.System,
						AviationSecurity_Unknown_Code);
				});
			}
		}

		ICodeDescriptionPairListProvider AviationSecurityDefaultListProvider_Australia
		{
			get
			{
				return new CodeDescriptionPairListProvider(() =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair("");
					foreach (ShipmentInspectionType pair in FreightDataRegistry.Instance.ShipmentInspectionTypes_Australia.Value.Types)
					{
						list.AddPair(pair.Code, pair.Description);
					}

					list.AddPair(AviationSecurity_Unknown_Code, AviationSecurity_Unknown_Description);

					return list;
				});
			}
		}

		public SupplyChainSecurityOrganisationToUseRegistryItem ShipmentInspectionOrganisationToUse_Australia
		{
			get
			{
				return GetItem<SupplyChainSecurityOrganisationToUseRegistryItem>("ShipmentInspectionOrganisationToUse_AU", delegate
				{
					var allAvailableShipmentInspectionOrganisations = GetShipmentInspectionOrganisations(Constants.CountryCodes.Australia);

					var hint = ResString.GetMultilingualString("06184974-839f-4619-ab9f-1b70c3886bf5",
						@"This Registry allows you to configure at a Company level, which organizations tendering and handling cargo should be used for the calculation of Known/Approved security inspection status on Shipments.  Each organization can be flagged as either:
-	Yes – Check the Known/Approved status of this organization (either Known Consignor, Account Consignor or Regulated Agent) and if not approved, calculate the Shipment as Unknown.
-	Warning – Check the Known/Approved status of this organization and if not Known/Approved, show a warning on the Inspection field.
-	No – Do not check the Known/Approved status of this organization.");

					var result = new SupplyChainSecurityOrganisationToUseRegistryItem(
						"ShipmentInspectionOrganisationToUse_AU",
						Categories.Freight_SupplyChainSecurity_AU,
						ResString.GetMultilingualString("48214630-0b42-4a6c-9bb8-db812ad566a4", "Organizations to Use for Supply Chain Security"),
						hint,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						allAvailableShipmentInspectionOrganisations);

					return result;
				});
			}
		}

		public ShipmentInspectionTypeRegistryItem ShipmentInspectionTypes_Australia
		{
			get
			{
				return GetItem<ShipmentInspectionTypeRegistryItem>("ShipmentInspectionTypes_AU", delegate
				{
					ShipmentInspectionTypes defaultValue = new ShipmentInspectionTypes(Constants.CountryCodes.Australia, new ShipmentInspectionTypeLists(RegistryFactory.Instance).SystemDefinedList_Australia);

					MultilingualString hint = EnableSupplyChainSecurity_AU.Value
						? ResString.GetMultilingualString("103c44bc-3782-44e2-87d6-9fc6a7bdc045", "A list of possible methods of Shipment Inspection for Australia, where the Shipper is not a known/approved shipper.")
						: ResString.GetMultilingualString("4a477cbc-8e7f-49e7-b78c-4e605edd0f14", "A list of possible methods of Shipment Inspection for Australia, where the Shipper is not a known/approved shipper. The '{0}' registry item must be enabled in order to use this list.", EnableSupplyChainSecurity_AU.Caption);

					ShipmentInspectionTypeRegistryItem result = new ShipmentInspectionTypeRegistryItem(
						"ShipmentInspectionTypes_AU",
						Categories.Freight_SupplyChainSecurity_AU,
						ResString.GetMultilingualString("7f0614e1-7a8e-4cbc-b6cc-fb45d031d255", "Inspection Types"),
						hint,
						RegistryStorageFlags.System,
						defaultValue);
					return result;
				});
			}
		}

		#endregion

		#region United States

		public BooleanRegistryItem EnableSupplyChainSecurity_US
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableSupplyChainSecurity_US", delegate
				{
					return new BooleanRegistryItem(
						"EnableSupplyChainSecurity_US",
						Categories.Freight_SupplyChainSecurity_US,
						ResString.GetMultilingualString("1d9b432d-bab1-411e-be51-6e7220f032dc", "Enable Supply Chain Security for United States"),
						ResString.GetMultilingualString("b5b41aec-f2c4-427a-8fbc-3b6e8e4af8b9", "Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country/region’s air export processes."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		#endregion

		#region Hong Kong

		public SupplyChainSecurityOrganisationToUseRegistryItem ShipmentInspectionOrganisationToUse_HongKong
		{
			get
			{
				return GetItem<SupplyChainSecurityOrganisationToUseRegistryItem>("ShipmentInspectionOrganisationToUse_HK", delegate
				{
					var allAvailableShipmentInspectionOrganisations = GetShipmentInspectionOrganisations(Constants.CountryCodes.HongKong);

					MultilingualString hint = ResString.GetMultilingualString("d865585d-3f5a-4e48-88f8-ace81b60fdbc",
		@"This Registry allows you to configure at a Company level, which organizations tendering and handling cargo should be used for the calculation of Known/Approved security inspection status on Shipments.  Each organization can be flagged as either:
-	Yes – Check the Known/Approved status of this organization (either Known Consignor, Account Consignor or Regulated Agent) and if not approved, calculate the Shipment as Unknown.
-	Warning – Check the Known/Approved status of this organization and if not Known/Approved, show a warning on the Inspection field.
-	No – Do not check the Known/Approved status of this organization.");

					SupplyChainSecurityOrganisationToUseRegistryItem result = new SupplyChainSecurityOrganisationToUseRegistryItem(
						"ShipmentInspectionOrganisationToUse_HK",
						Categories.Freight_SupplyChainSecurity_HongKong,
						ResString.GetMultilingualString("b1760c9e-63d5-4388-b931-cab1b6c841f8", "Organizations to Use for Supply Chain Security"),
						hint,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						allAvailableShipmentInspectionOrganisations);
					return result;
				});
			}
		}

		public ShipmentInspectionTypeRegistryItem ShipmentInspectionTypes_HongKong
		{
			get
			{
				return GetItem<ShipmentInspectionTypeRegistryItem>("ShipmentInspectionTypes_HK", delegate
				{
					ShipmentInspectionTypes defaultValue = new ShipmentInspectionTypes(Constants.CountryCodes.HongKong, new ShipmentInspectionTypeLists(RegistryFactory.Instance).SystemDefinedList_HongKong);

					MultilingualString hint = EnableSupplyChainSecurity_HK.Value
						? ResString.GetMultilingualString("224b2bbf-bedc-4fbf-a8fe-755b4475a26a", "A list of possible methods of Shipment Inspection for Hong Kong, where the Shipper is not a known/approved shipper.")
						: ResString.GetMultilingualString("aa9fd59d-e7c3-4446-922e-2a26cacd283d", "A list of possible methods of Shipment Inspection for Hong Kong, where the Shipper is not a known/approved shipper. The '{0}' registry item must be enabled in order to use this list.", EnableSupplyChainSecurity_HK.Caption);

					ShipmentInspectionTypeRegistryItem result = new ShipmentInspectionTypeRegistryItem(
						"ShipmentInspectionTypes_HK",
						Categories.Freight_SupplyChainSecurity_HongKong,
						ResString.GetMultilingualString("89c55ef1-58db-4825-a033-aa1bdca8a2d2", "Inspection Types"),
						hint,
						RegistryStorageFlags.System,
						defaultValue);
					return result;
				});
			}
		}

		public CodePairRegistryItem ShipmentInspectionTypeDefault_HongKong
		{
			get
			{
				MultilingualString hint = EnableSupplyChainSecurity_HK.Value
					? ResString.GetMultilingualString("6fc0e60b-5671-49ef-b227-25c953859dd3", "This is the default value for the Shipment and Pack Line Inspection Type, when consignor/shipper is not known/approved.")
					: ResString.GetMultilingualString("0ad8d107-19b7-45b7-ab74-ba71adf7dfc6", "This is the default value for the Shipment and Pack Line Inspection Type, when consignor/shipper is not known/approved. The '{0}' registry item must be enabled in order to use this value.", EnableSupplyChainSecurity_HK.Caption);

				return GetItem<CodePairRegistryItem>("ShipmentInspectionTypesDefault_HK", delegate
				{
					return new CodePairRegistryItem(
						"ShipmentInspectionTypesDefault_HK",
						Categories.Freight_SupplyChainSecurity_HongKong,
						ResString.GetMultilingualString("f59ec462-5b8c-4519-8d29-acefa78da515", "Inspection Type Default for Unknown Organizations"),
						hint,
						AviationSecurityDefaultListProvider_HongKong,
						RegistryStorageFlags.System,
						AviationSecurity_Unknown_Code);
				});
			}
		}

		ICodeDescriptionPairListProvider AviationSecurityDefaultListProvider_HongKong
		{
			get
			{
				return new CodeDescriptionPairListProvider(() =>
				{
					CodeDescriptionPairList list = new CodeDescriptionPairList();
					list.AddPair("");
					foreach (ShipmentInspectionType pair in FreightDataRegistry.Instance.ShipmentInspectionTypes_HongKong.Value.Types)
					{
						list.AddPair(pair.Code, pair.Description);
					}

					list.AddPair(AviationSecurity_Unknown_Code, AviationSecurity_Unknown_Description);

					return list;
				});
			}
		}

		public BooleanRegistryItem EnableSupplyChainSecurity_HK
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableSupplyChainSecurity_HK", delegate
				{
					return new BooleanRegistryItem(
						"EnableSupplyChainSecurity_HK",
						Categories.Freight_SupplyChainSecurity_HongKong,
						ResString.GetMultilingualString("eea665b1-c051-4b90-9f50-1e281e0c1200", "Enable Supply Chain Security for Hong Kong"),
						ResString.GetMultilingualString("a3b71968-cbaf-4ff0-af58-4e521b6ff3f1", "Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country/region’s air export processes."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#region European Union

		public SupplyChainSecurityOrganisationToUseRegistryItem ShipmentInspectionOrganisationToUse_EuropeanUnion
		{
			get
			{
				return GetItem<SupplyChainSecurityOrganisationToUseRegistryItem>("ShipmentInspectionOrganisationToUse_EU", delegate
				{
					var allAvailableShipmentInspectionOrganisations = GetShipmentInspectionOrganisations(Constants.CountryCodes.EuropeanUnion);

					var hint = ResString.GetMultilingualString("f96c2106-f7d3-401a-abb1-5e0e55e26d8c", @"This Registry allows you to configure at a Company level, which organizations tendering and handling cargo should be used for the calculation of Known/Approved security inspection status on Shipments.  Each organization can be flagged as either:
-	Yes – Check the Known/Approved status of this organization (either Known Consignor, Account Consignor or Regulated Agent) and if not approved, calculate the Shipment as Unknown.
-	Warning – Check the Known/Approved status of this organization and if not Known/Approved, show a warning on the Inspection field.
-	No – Do not check the Known/Approved status of this organization.");

					var result = new SupplyChainSecurityOrganisationToUseRegistryItem(
						"ShipmentInspectionOrganisationToUse_EU",
						Categories.Freight_SupplyChainSecurity_EU,
						ResString.GetMultilingualString("f67b106e-499f-4ffa-b71f-3f6a9836a391", "Organizations to Use for Supply Chain Security"),
						hint,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						allAvailableShipmentInspectionOrganisations);
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableSupplyChainSecurity_EU
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableSupplyChainSecurity_EU", delegate
				{
					return new BooleanRegistryItem(
						"EnableSupplyChainSecurity_EU",
						Categories.Freight_SupplyChainSecurity_EU,
						ResString.GetMultilingualString("28a7421f-0152-4aa5-860d-769eab3a4859", "Enable Supply Chain Security for the European Union"),
						ResString.GetMultilingualString("30ad9b75-8593-4f51-9544-99ccf638f4ce", "Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country/region’s air export processes."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public AviationSecurityTrainingRestrictionRegistryItem AviationSecurityTrainingRestrictions_EU
		{
			get
			{
				return GetItem<AviationSecurityTrainingRestrictionRegistryItem>("AviationSecurityTrainingRestrictions_EU", () =>
				{
					return new AviationSecurityTrainingRestrictionRegistryItem("AviationSecurityTrainingRestrictions_EU",
						Categories.Freight_SupplyChainSecurity_EU,
						ResString.GetMultilingualString("8d886ba8-05fb-402f-80ea-34623a9fc271", "Aviation Security Training Restrictions for the European Union"),
						ResString.GetMultilingualString("8aee4c44-2859-41fc-83d1-1815a49faad4", "Override this registry to configure certification restrictions for editing the Shipment and Consolidation fields related to aviation security, as well as for movement of restricted documents issued from air export jobs from EU, Iceland, Switzerland, Norway or Liechtenstein, and organizations located in these countries."),
						RegistryStorageFlags.System,
						new AviationSecurityTrainingRestriction(true, false));
				});
			}
		}

		public ShipmentInspectionTypeRegistryItem ShipmentInspectionTypes_EU
		{
			get
			{
				return GetItem<ShipmentInspectionTypeRegistryItem>("ShipmentInspectionTypes_EU", delegate
				{
					var defaultValue = new ShipmentInspectionTypes(Constants.CountryCodes.EuropeanUnion, new ShipmentInspectionTypeLists(RegistryFactory.Instance).SystemDefinedList_EuropeanUnion);

					var hint = EnableSupplyChainSecurity_EU.Value
					? ResString.GetMultilingualString("3c36179e-7ff9-487d-95e3-997060e6c625", "A list of possible methods of Shipment Inspection for the European Union, where the Shipper is not a known/approved shipper.")
					: ResString.GetMultilingualString("b673c94a-a083-4f9f-97cf-abdfe809df81", "A list of possible methods of Shipment Inspection for the European Union, where the Shipper is not a known/approved shipper. The '{0}' registry item must be enabled in order to use this list.", EnableSupplyChainSecurity_EU.Caption);

					var result = new ShipmentInspectionTypeRegistryItem(
						"ShipmentInspectionTypes_EU",
						Categories.Freight_SupplyChainSecurity_EU,
						ResString.GetMultilingualString("b2e2815c-6060-4faa-b1c7-616d2ff7a2fa", "Inspection Types"),
						hint,
						RegistryStorageFlags.System,
						defaultValue);

					return result;
				});
			}
		}

		#endregion

		#region United Kingdom

		public SupplyChainSecurityOrganisationToUseRegistryItem ShipmentInspectionOrganisationToUse_UK
		{
			get
			{
				return GetItem<SupplyChainSecurityOrganisationToUseRegistryItem>("ShipmentInspectionOrganisationToUse_UK", delegate
				{
					var allAvailableShipmentInspectionOrganisations = GetShipmentInspectionOrganisations(Constants.CountryCodes.UnitedKingdom);

					var hint = ResString.GetMultilingualString("D7A526F5-CF55-483F-BFCC-8F6140FE6D8D", @"This Registry allows you to configure the system to automatically recalculate the Approved security inspection status to Unknown when a flagged Organization is removed, replaced or its address is changed on the Shipment or Consol. Each Organization can be flagged as either:
-	Yes/Warning – The Inspection status will be recalculated to Unknown and a warning message will be displayed.
-	No – The Inspection status will not be recalculated.");

					var result = new SupplyChainSecurityOrganisationToUseRegistryItem(
						"ShipmentInspectionOrganisationToUse_UK",
						Categories.Freight_SupplyChainSecurity_UK,
						ResString.GetMultilingualString("a67b206e-495b-4ffa-b71f-3f6a9836a491", "Organizations to Use for Supply Chain Security"),
						hint,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						allAvailableShipmentInspectionOrganisations);
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableSupplyChainSecurity_UK
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableSupplyChainSecurity_UK", delegate
				{
					return new BooleanRegistryItem(
						"EnableSupplyChainSecurity_UK",
						Categories.Freight_SupplyChainSecurity_UK,
						ResString.GetMultilingualString("a8a7421f-0152-4aa5-560d-769e2b3a4859", "Enable Supply Chain Security for United Kingdom"),
						ResString.GetMultilingualString("30ad9b75-8593-4f51-9544-99ccf638f4ce", "Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country/region’s air export processes."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#region Singapore

		public BooleanRegistryItem EnableSupplyChainSecurity_SG
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableSupplyChainSecurity_SG", delegate
				{
					return new BooleanRegistryItem(
						"EnableSupplyChainSecurity_SG",
						Categories.Freight_SupplyChainSecurity_SG,
						ResString.GetMultilingualString("c7cd91f3-7bde-4020-b4f5-8c5f13a1cdbc", "Enable Supply Chain Security for Singapore"),
						ResString.GetMultilingualString("ba0f447e-381c-4a81-b7fa-696c5dd6871a", "Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country/region’s air export processes."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#region Taiwan

		public BooleanRegistryItem EnableSupplyChainSecurity_TW
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableSupplyChainSecurity_TW", delegate
				{
					return new BooleanRegistryItem(
						"EnableSupplyChainSecurity_TW",
						Categories.Freight_SupplyChainSecurity_TW,
						ResString.GetMultilingualString("81861286-2624-4823-a00b-0b12f2fcd042", "Enable Supply Chain Security for Taiwan"),
						ResString.GetMultilingualString("1c163c28-3b9e-4fa7-8d81-219dc2ac51d4", "Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country’s air export processes."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public StringRegistryItem RegulatedAgentNumber_TW
		{
			get
			{
				return GetItem("RegulatedAgentNumber_TW", delegate
				{
					return new StringRegistryItem("RegulatedAgentNumber_TW",
						Categories.Freight_SupplyChainSecurity_TW,
						ResString.GetMultilingualString("dfba8575-2e87-475d-9d11-7a77de3d93eb", "Regulated Agent Number"), ResString.GetMultilingualString("55e8c200-02fc-40f8-912d-9c27a20907eb", "Agent is approved to handle restricted articles"),
						new StringRegistryDataType(0, 20), null, RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default, "");
				});
			}
		}

		#endregion

		#region Canada

		public BooleanRegistryItem EnableSupplyChainSecurity_CA
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableSupplyChainSecurity_CA", delegate
				{
					return new BooleanRegistryItem(
						"EnableSupplyChainSecurity_CA",
						Categories.Freight_SupplyChainSecurity_CA,
						ResString.GetMultilingualString("dea7ddfc-e19c-46fb-b598-b2409a0d38cf", "Enable Supply Chain Security for Canada"),
						ResString.GetMultilingualString("a3757706-5eea-40ad-969e-996d668fcb54", "Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country’s air export processes."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public ShipmentInspectionTypeRegistryItem ShipmentInspectionTypes_Canada
		{
			get
			{
				return GetItem<ShipmentInspectionTypeRegistryItem>("ShipmentInspectionTypes_CA", delegate
				{
					ShipmentInspectionTypes defaultValue = new ShipmentInspectionTypes(Constants.CountryCodes.Canada, new ShipmentInspectionTypeLists(RegistryFactory.Instance).SystemDefinedList_Canada);

					MultilingualString hint = EnableSupplyChainSecurity_CA.Value
						? ResString.GetMultilingualString("52d26359-045f-47df-9533-9bc762768e90", "A list of possible methods of Shipment Inspection for Canada, where the Shipper is not a known/approved shipper.")
						: ResString.GetMultilingualString("02128ac0-ee4a-4bfb-ac0a-3b9a685b14b2", "A list of possible methods of Shipment Inspection for Canada, where the Shipper is not a known/approved shipper. The '{0}' registry item must be enabled in order to use this list.", EnableSupplyChainSecurity_CA.Caption);

					ShipmentInspectionTypeRegistryItem result = new ShipmentInspectionTypeRegistryItem(
						"ShipmentInspectionTypes_CA",
						Categories.Freight_SupplyChainSecurity_CA,
						ResString.GetMultilingualString("1b9ca614-401f-4fed-b15b-c1bbded01fd4", "Inspection Types"),
						hint,
						RegistryStorageFlags.System,
						defaultValue);
					return result;
				});
			}
		}

		public SupplyChainSecurityOrganisationToUseRegistryItem ShipmentInspectionOrganisationToUse_Canada
		{
			get
			{
				return GetItem<SupplyChainSecurityOrganisationToUseRegistryItem>("ShipmentInspectionOrganisationToUse_CA", delegate
				{
					var allAvailableShipmentInspectionOrganisations = GetShipmentInspectionOrganisations(Constants.CountryCodes.Canada);

					var hint = ResString.GetMultilingualString("a28c474a-b557-4517-949e-313bf7670d2e", @"This Registry allows you to configure at a Company level, which organizations tendering and handling cargo should be used for the calculation of Known/Approved security inspection status on Shipments.  Each organization can be flagged as either:
-	Yes – Check the Known/Approved status of this organization (either Known Consignor, Account Consignor or Regulated Agent) and if not approved, calculate the Shipment as Unknown.
-	Warning – Check the Known/Approved status of this organization and if not Known/Approved, show a warning on the Inspection field.
-	No – Do not check the Known/Approved status of this organization.");

					var result = new SupplyChainSecurityOrganisationToUseRegistryItem(
						"ShipmentInspectionOrganisationToUse_CA",
						Categories.Freight_SupplyChainSecurity_CA,
						ResString.GetMultilingualString("7bca77c9-7165-422f-89a0-417a21bc4d40", "Organizations to Use for Supply Chain Security"),
						hint,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						allAvailableShipmentInspectionOrganisations);
					return result;
				});
			}
		}

		#endregion

		#region South Africa

		public BooleanRegistryItem EnableSupplyChainSecurity_ZA
		{
			get
			{
				return GetItem("EnableSupplyChainSecurity_ZA", delegate
				{
					return new BooleanRegistryItem(
						"EnableSupplyChainSecurity_ZA",
						Categories.Freight_SupplyChainSecurity_ZA,
						ResString.GetMultilingualString("EDFA2F5C-873C-49F6-9FF6-6E7F12063FE2", "Enable Supply Chain Security for South Africa"),
						ResString.GetMultilingualString("98F64AB8-B573-48EF-BDAE-907B94D5A01E", "Supply Chain Security is part of the base CW package so is not charged for separately. Enabling Supply Chain Security provides deep functionality supporting the country’s air export processes."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public ShipmentInspectionTypeRegistryItem ShipmentInspectionTypes_SouthAfrica
		{
			get
			{
				return GetItem<ShipmentInspectionTypeRegistryItem>("ShipmentInspectionTypes_ZA", delegate
				{
					ShipmentInspectionTypes defaultValue = new ShipmentInspectionTypes(Constants.CountryCodes.SouthAfrica, new ShipmentInspectionTypeLists(RegistryFactory.Instance).SystemDefinedList_SouthAfrica);

					MultilingualString hint = EnableSupplyChainSecurity_ZA.Value
						? ResString.GetMultilingualString("f16e6455-afa4-45cd-b92b-7079a81711be", "A list of possible methods of Shipment Inspection for South Africa, where the Shipper is not a known/approved shipper.")
						: ResString.GetMultilingualString("e88edf37-4dee-4170-b626-ea5fe58b6ce8", "A list of possible methods of Shipment Inspection for South Africa, where the Shipper is not a known/approved shipper. The '{0}' registry item must be enabled in order to use this list.", EnableSupplyChainSecurity_ZA.Caption);

					ShipmentInspectionTypeRegistryItem result = new ShipmentInspectionTypeRegistryItem(
						"ShipmentInspectionTypes_ZA",
						Categories.Freight_SupplyChainSecurity_ZA,
						ResString.GetMultilingualString("7a20c5b7-50fa-4683-b76f-4d698a80bcd7", "Inspection Types"),
						hint,
						RegistryStorageFlags.System,
						defaultValue);
					return result;
				});
			}
		}

		public SupplyChainSecurityOrganisationToUseRegistryItem ShipmentInspectionOrganisationToUse_SouthAfrica
		{
			get
			{
				return GetItem("ShipmentInspectionOrganisationToUse_ZA", delegate
				{
					var allAvailableShipmentInspectionOrganisations = GetShipmentInspectionOrganisations(Constants.CountryCodes.SouthAfrica);

					var hint = ResString.GetMultilingualString("CC56687F-298B-418E-AECC-9A6820B46699", @"This Registry allows you to configure at a Company level, which organizations tendering and handling cargo should be used for the calculation of Known/Approved security inspection status on Shipments.  Each organization can be flagged as either:
-	Yes – Check the Known/Approved status of this organization (either Known Consignor, Account Consignor or Regulated Agent) and if not approved, calculate the Shipment as Unknown.
-	Warning – Check the Known/Approved status of this organization and if not Known/Approved, show a warning on the Inspection field.
-	No – Do not check the Known/Approved status of this organization.");

					var result = new SupplyChainSecurityOrganisationToUseRegistryItem(
						"ShipmentInspectionOrganisationToUse_ZA",
						Categories.Freight_SupplyChainSecurity_ZA,
						ResString.GetMultilingualString("F5F91EBD-ADAB-4C40-B748-DA93E5FBE858", "Organizations to Use for Supply Chain Security"),
						hint,
						RegistryStorageFlags.System,
						allAvailableShipmentInspectionOrganisations);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region Shipper COD Payment Types

		public CodeDescriptionPairListRegistryItem ShipperCODPaymentTypes
		{
			get
			{
				var allDefaultValues = new CodeDescriptionPairList();
				allDefaultValues.AddPair("COC", ResString.GetMultilingualString("4a6a00a3-2399-4b50-8f3e-811961ba1e04", "Company Check"));
				allDefaultValues.AddPair("CEC", ResString.GetMultilingualString("3ee724d3-174a-46fe-b6a2-6b45b5aaf8a8", "Certified Check"));
				allDefaultValues.AddPair("CAC", ResString.GetMultilingualString("316f685b-b314-48ef-a608-2e3fe5fe2de0", "Cashier's Check"));
				allDefaultValues.AddPair("BAC", ResString.GetMultilingualString("eaa62091-08b1-4afb-8161-e92e759e6da7", "Bank Check"));

				MultilingualString[] categories = { Categories.Freight_Shipment, WarehouseDataRegistry.Categories.Warehouse_Freight };
				CodeDescriptionPairListRegistryItem item = GetItem<CodeDescriptionPairListRegistryItem>("ShipperCODPaymentTypes", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						new RegistryItemImplWithDynamicDefaultValue("ShipperCODPaymentTypes",
						categories,
						ResString.GetMultilingualString("c9a68282-a849-4388-919c-4a5a58568fa1", "Shipper COD Payment Types"),
						ResString.GetMultilingualString("d73295a9-611b-4938-8523-dc0a7d215c59", "A list of allowed Shipper COD Payment Types for domestic shipments."),
						new CodeDescriptionPairListRegistryDataType(3),
						new CodeDescriptionPairListEditorInfo(),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						ShipperCODPaymentTypesDefaultValueGetter),
						true,
						allDefaultValues);
				});

				return item;
			}
		}

		static CodeDescriptionPairList ShipperCODPaymentTypesDefaultValueGetter(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var countryCode = string.Empty;
			var company = (BusinessObject)new BusinessObjectFactory().Load<IGlbCompany>(companyPK);
			if (company != null)
			{
				countryCode = (ZString)company[GlbCompanySchema.GC_RN_NKCountryCode];
			}

			var list = new CodeDescriptionPairList();
			list.AddPair("COC", ResString.GetMultilingualString("4a6a00a3-2399-4b50-8f3e-811961ba1e04", "Company Check"));

			if (countryCode == Core.Constants.CountryCodes.UnitedStates)
			{
				list.AddPair("CEC", ResString.GetMultilingualString("3ee724d3-174a-46fe-b6a2-6b45b5aaf8a8", "Certified Check"));
				list.AddPair("CAC", ResString.GetMultilingualString("316f685b-b314-48ef-a608-2e3fe5fe2de0", "Cashier's Check"));
			}
			else
			{
				list.AddPair("BAC", ResString.GetMultilingualString("eaa62091-08b1-4afb-8161-e92e759e6da7", "Bank Check"));
			}

			return list;
		}

		#endregion

		#region Default Insurance Value

		public BooleanRegistryItem InsuranceValueDefaulting
		{
			get
			{
				return GetItem<BooleanRegistryItem>("InsuranceValueDefaulting", delegate
				{
					return new BooleanRegistryItem(
						"InsuranceValueDefaulting",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("4f130df5-7118-433c-bef4-ffafead2cf59", "Default Insurance Value from Goods Value"),
						ResString.GetMultilingualString("3e22b742-5c97-439d-a57a-df46390d834a", "Enable this option to default Insurance Value from Goods Value."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#region Default Shipment Controlling Customer

		public BooleanRegistryItem DefaultShipmentControllingCustomer
		{
			get
			{
				return GetItem<BooleanRegistryItem>("DefaultShipmentControllingCustomer", delegate
				{
					return new BooleanRegistryItem(
						"DefaultShipmentControllingCustomer",
						new MultilingualString[] { Categories.Freight_Shipment, Categories.Customs },
						ResString.GetMultilingualString("25c7c51a-ab59-4aa6-a7a1-d6ba44ebae20", "Default Shipment Controlling Customer"),
						ResString.GetMultilingualString("7bdda6b5-24bc-44c9-ba16-51b1c7a9b07f", "Enable this option to default the Shipment Controlling Customer when saving a New shipment."),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public EntityPrecedenceRuleRegistryItem DefaultControllingCustomerRule
		{
			get
			{
				var hint = ResString.GetMultilingualString("f2455b00-edb9-4076-bd89-2c96313572ae", @"This registry specifies an order of precedence for calculation of which shipment or booking organization the Controlling Customer should be defaulted from.

Use override to reprioritize these parties if required.

Notes:

Freight Bill To Party – is  a local client organization responsible for paying freight as per Incoterms (i.e. local client at origin for freight prepaid or destination for freight collect), and this is not a logistics provider such as a broker, freight forwarder etc (in which case the system will check next party in the priority list).

Booking Party - is the Client organization on the quick booking/booking with quote.

Consignor/Shipper for prepaid freight – if a shipment is freight prepaid, the system will first try to find a Controlling Customer for IFT (Invoice Freight Jobs To) party of the Shipper, and if blank – Controlling Customer of the Shipper organization.  Consignee for collect freight – will first try to find Controlling Customer for IFT party of Consignee with the fallback to Consignee organization.");

				return GetItem("DefaultControllingCustomerPrecedence", delegate
				{
					var result = new EntityPrecedenceRuleRegistryItem(
						"DefaultControllingCustomerPrecedence",
						new MultilingualString[] { Categories.Freight_Shipment, Categories.Customs },
						ResString.GetMultilingualString("75621bf3-9a83-47d7-a0e9-5e6b2e7277bf", "Controlling Customer – Precedence Defaulting Logic"),
						hint,
						null,
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						GetDefaultControllingCustomerOrgTypes());

					return result;
				});
			}
		}

		EntityPrecedenceRuleItemCollection GetDefaultControllingCustomerOrgTypes()
		{
			var result = new EntityPrecedenceRuleItemCollection
			{
				new EntityPrecedenceRuleItem
				{
					Code = Constants.DefaultControllingCustomerOrgTypes.Code.BookingParty,
					Description = Constants.DefaultControllingCustomerOrgTypes.Description.BookingParty,
					Bool = true
				},
				new EntityPrecedenceRuleItem
				{
					Code = Constants.DefaultControllingCustomerOrgTypes.Code.BillToParty,
					Description = Constants.DefaultControllingCustomerOrgTypes.Description.BillToParty,
					Bool = true
				},
				new EntityPrecedenceRuleItem
				{
					Code = Constants.DefaultControllingCustomerOrgTypes.Code.ConsignorConsignee,
					Description = Constants.DefaultControllingCustomerOrgTypes.Description.ConsignorConsignee,
					Bool = true
				}
			};

			return result;
		}

		#endregion

		#region Controlling Customer - Use Sales Rep

		public BooleanRegistryItem ControllingCustomerUseSalesRep
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ControllingCustomerUseSalesRep", delegate
				{
					return new BooleanRegistryItem(
						"ControllingCustomerUseSalesRep",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("11a8353e-d88e-485a-86e5-a7473725cf0a", "Controlling Customer - Use Sales Rep"),
						ResString.GetMultilingualString("ee1f9a8a-bc74-4cc0-82be-e5f1761da93d", "Set this registry to Yes to default Controlling Customer's sales rep to the billing tab of a job in lieu of Local Client's sales rep."),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Default Controlling Agent

		public BooleanRegistryItem DefaultShipmentControllingAgent
		{
			get
			{
				return GetItem("DefaultShipmentControllingAgent", delegate
				{
					return new BooleanRegistryItem(
						"DefaultShipmentControllingAgent",
						new MultilingualString[] { Categories.Freight_Shipment, Categories.Customs },
						ResString.GetMultilingualString("a5e52a3b-f694-451c-9859-81ec15838cf4", "Default Shipment Controlling Agent"),
						ResString.GetMultilingualString("ad9c0060-c1aa-4975-a767-c219080f7109", "Enable this option to default the Shipment Controlling Agent when saving a New shipment."),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public EntityPrecedenceRuleRegistryItem DefaultControllingAgentRule
		{
			get
			{
				ResourceString hint = ResString.GetMultilingualString("17380c1c-f486-4901-8cc6-039bd74fab3c", @"This registry specifies an order of precedence for calculation of which shipment or booking organization the Controlling Agent should be defaulted from.

Use override to re-prioritize these parties if required.

Notes:
Freight Bill To Party - is a local client organization responsible for paying freight as per Incoterms (i.e. local client at origin for freight prepaid or destination for freight collect), and this is not a logistics provider such as a broker, freight forwarder etc (in which case the system will check next party in the priority list).

Booking Party - is the Client organization on the quick booking/booking with quote.

Shipper for prepaid freight – if a shipment is freight prepaid, the system will first try to find a Controlling Agent for IFT (Invoice Freight Jobs To) party of the Shipper, and if blank – Controlling Agent of the Shipper organization.  Consignee for collect freight – will first try to find Controlling Agent for IFT party of Consignee with the fallback to Consignee organization.");

				return GetItem("DefaultControllingAgentPrecedence", delegate
				{
					EntityPrecedenceRuleRegistryItem result = new EntityPrecedenceRuleRegistryItem(
						"DefaultControllingAgentPrecedence",
						new MultilingualString[] { Categories.Freight_Shipment, Categories.Customs },
						ResString.GetMultilingualString("2acbe52b-6cba-42a9-9ebb-379982e25dc6", "Controlling Agent – Precedence Defaulting Logic"),
						hint,
						null,
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						GetDefaultControllingAgentOrgTypes());

					return result;
				});
			}
		}

		EntityPrecedenceRuleItemCollection GetDefaultControllingAgentOrgTypes()
		{
			var result = new EntityPrecedenceRuleItemCollection
			{
				new EntityPrecedenceRuleItem()
				{
					Code = Constants.DefaultControllingAgentOrgTypes.Code.ControllingCustomer,
					Description = Constants.DefaultControllingAgentOrgTypes.Description.ControllingCustomer,
					Bool = true
				},
				new EntityPrecedenceRuleItem
				{
					Code = Constants.DefaultControllingAgentOrgTypes.Code.BillToParty,
					Description = Constants.DefaultControllingAgentOrgTypes.Description.BillToParty,
					Bool = true
				},
				new EntityPrecedenceRuleItem
				{
					Code = Constants.DefaultControllingAgentOrgTypes.Code.BookingParty,
					Description = Constants.DefaultControllingAgentOrgTypes.Description.BookingParty,
					Bool = true
				},
				new EntityPrecedenceRuleItem
				{
					Code = Constants.DefaultControllingAgentOrgTypes.Code.ConsignorConsignee,
					Description = Constants.DefaultControllingAgentOrgTypes.Description.ConsignorConsignee,
					Bool = true
				}
			};

			return result;
		}

		#endregion

		#region Mandatory Controlling Customer Effective Date

		readonly ResourceString mandatoryControllingCustomerEffectiveDateHint = ResString.GetMultilingualString("0C26AD6C-8A33-4EE6-9FD8-85E60B1E8502", "The date specified in this registry will trigger validation of user or group security rights set to prevent \"Allow Save Without Controlling Customer\" on shipments and bookings created on or after this date.\r\n\r\nOverride this registry to specify the date in UTC format.");

		public DateTimeRegistryItem MandatoryControllingCustomerEffectiveDateAll
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("MandatoryControllingCustomerEffectiveDateAll", delegate
				{
					return new DateTimeRegistryItem(
						"MandatoryControllingCustomerEffectiveDateAll",
						Categories.Freight_Shipment_EffectiveDateForMandatoryControllingCustomer,
						ResString.GetMultilingualString("C8E56CC1-FC50-4E79-B9FF-699DA9B09C5E", "All Transport Modes"),
						mandatoryControllingCustomerEffectiveDateHint,
						RegistryStorageFlags.Company
						);
				});
			}
		}

		public DateTimeRegistryItem MandatoryControllingCustomerEffectiveDateSea
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("MandatoryControllingCustomerEffectiveDateSea", delegate
				{
					return new DateTimeRegistryItem(
						"MandatoryControllingCustomerEffectiveDateSea",
						Categories.Freight_Shipment_EffectiveDateForMandatoryControllingCustomer,
						ResString.GetMultilingualString("41542778-8C06-4139-9BE6-0A70D9AE28F5", "For Sea"),
						mandatoryControllingCustomerEffectiveDateHint,
						RegistryStorageFlags.Company
						);
				});
			}
		}

		public DateTimeRegistryItem MandatoryControllingCustomerEffectiveDateAir
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("MandatoryControllingCustomerEffectiveDateAir", delegate
				{
					return new DateTimeRegistryItem(
						"MandatoryControllingCustomerEffectiveDateAir",
						Categories.Freight_Shipment_EffectiveDateForMandatoryControllingCustomer,
						ResString.GetMultilingualString("7FC8AB8C-5322-451B-8AE6-37B728FB0A20", "For Air"),
						mandatoryControllingCustomerEffectiveDateHint,
						RegistryStorageFlags.Company
						);
				});
			}
		}

		public DateTimeRegistryItem MandatoryControllingCustomerEffectiveDateRoad
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("MandatoryControllingCustomerEffectiveDateRoad", delegate
				{
					return new DateTimeRegistryItem(
						"MandatoryControllingCustomerEffectiveDateRoad",
						Categories.Freight_Shipment_EffectiveDateForMandatoryControllingCustomer,
						ResString.GetMultilingualString("DF76CA5F-A01E-4076-80D3-9623E3894899", "For Road"),
						mandatoryControllingCustomerEffectiveDateHint,
						RegistryStorageFlags.Company
						);
				});
			}
		}

		public DateTimeRegistryItem MandatoryControllingCustomerEffectiveDateRail
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("MandatoryControllingCustomerEffectiveDateRail", delegate
				{
					return new DateTimeRegistryItem(
						"MandatoryControllingCustomerEffectiveDateRail",
						Categories.Freight_Shipment_EffectiveDateForMandatoryControllingCustomer,
						ResString.GetMultilingualString("69AD6D30-8B9A-4E69-ADCF-A29FF2DD731D", "For Rail"),
						mandatoryControllingCustomerEffectiveDateHint,
						RegistryStorageFlags.Company
						);
				});
			}
		}

		#endregion

		#region Mandatory Controlling Agent Effective Date

		readonly ResourceString mandatoryControllingAgentEffectiveDateHint = ResString.GetMultilingualString("95231E7A-6C1C-4633-98DA-651346B7E57B", "The date specified in this registry will trigger validation of user or group security rights set to prevent \"Allow Save Without Controlling Agent\" on shipments and bookings created on or after this date.\r\n\r\nOverride this registry to specify the date in UTC format.");

		public DateTimeRegistryItem MandatoryControllingAgentEffectiveDateAll
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("MandatoryControllingAgentEffectiveDateAll", delegate
				{
					return new DateTimeRegistryItem(
						"MandatoryControllingAgentEffectiveDateAll",
						Categories.Freight_Shipment_EffectiveDateForMandatoryControllingAgent,
						ResString.GetMultilingualString("C8E56CC1-FC50-4E79-B9FF-699DA9B09C5E", "All Transport Modes"),
						mandatoryControllingAgentEffectiveDateHint,
						RegistryStorageFlags.Company
						);
				});
			}
		}

		public DateTimeRegistryItem MandatoryControllingAgentEffectiveDateSea
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("MandatoryControllingAgentEffectiveDateSea", delegate
				{
					return new DateTimeRegistryItem(
						"MandatoryControllingAgentEffectiveDateSea",
						Categories.Freight_Shipment_EffectiveDateForMandatoryControllingAgent,
						ResString.GetMultilingualString("41542778-8C06-4139-9BE6-0A70D9AE28F5", "For Sea"),
						mandatoryControllingAgentEffectiveDateHint,
						RegistryStorageFlags.Company
						);
				});
			}
		}

		public DateTimeRegistryItem MandatoryControllingAgentEffectiveDateAir
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("MandatoryControllingAgentEffectiveDateAir", delegate
				{
					return new DateTimeRegistryItem(
						"MandatoryControllingAgentEffectiveDateAir",
						Categories.Freight_Shipment_EffectiveDateForMandatoryControllingAgent,
						ResString.GetMultilingualString("7FC8AB8C-5322-451B-8AE6-37B728FB0A20", "For Air"),
						mandatoryControllingAgentEffectiveDateHint,
						RegistryStorageFlags.Company
						);
				});
			}
		}

		public DateTimeRegistryItem MandatoryControllingAgentEffectiveDateRoad
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("MandatoryControllingAgentEffectiveDateRoad", delegate
				{
					return new DateTimeRegistryItem(
						"MandatoryControllingAgentEffectiveDateRoad",
						Categories.Freight_Shipment_EffectiveDateForMandatoryControllingAgent,
						ResString.GetMultilingualString("DF76CA5F-A01E-4076-80D3-9623E3894899", "For Road"),
						mandatoryControllingAgentEffectiveDateHint,
						RegistryStorageFlags.Company
						);
				});
			}
		}

		public DateTimeRegistryItem MandatoryControllingAgentEffectiveDateRail
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("MandatoryControllingAgentEffectiveDateRail", delegate
				{
					return new DateTimeRegistryItem(
						"MandatoryControllingAgentEffectiveDateRail",
						Categories.Freight_Shipment_EffectiveDateForMandatoryControllingAgent,
						ResString.GetMultilingualString("69AD6D30-8B9A-4E69-ADCF-A29FF2DD731D", "For Rail"),
						mandatoryControllingAgentEffectiveDateHint,
						RegistryStorageFlags.Company
						);
				});
			}
		}

		#endregion

		#region SeaMinimumChargeableWeight

		public DecimalRegistryItem SeaMinimumChargeableWeight
		{
			get
			{
				#region hintText
				MultilingualString hintText = ResString.GetMultilingualString("1211f6d9-1375-4ced-a68b-b32c9b36bd22", @"The Minimum Chargeable Unit (SEA Shipments *only*).

When the chargeable unit calculated is less than the minimum specified, the system will display the specified value as the ""Chargeable Weight"" in the Shipment Form.

By default, the value is 0, which indicates that no minimum chargeable is applicable.");
				#endregion

				return GetItem<DecimalRegistryItem>("SeaMinimumChargeableWeight", delegate
				{
					return new DecimalRegistryItem(
						"SeaMinimumChargeableWeight",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("10a21f91-8855-49f6-bb37-8b6b1209bf82", "Minimum Chargeable"),
						hintText,
						new NumericRegistryEditorInfo(3),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						0, 0, 1000000);
				});
			}
		}

		#endregion

		#region Estimated Export Clearance Date Mandatory

		public BooleanRegistryItem EstimatedExportCustomsClearanceDateMandatory
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EstimatedExportCustomsClearanceDateMandatory", delegate
				{
					return new BooleanRegistryItem(
						"EstimatedExportCustomsClearanceDateMandatory",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("09c0d6e2-7f1f-4f0a-a227-c8ae8a94391f", "Estimated Export Customs Clearance Date Mandatory"),
						ResString.GetMultilingualString("944b61d3-b071-42c4-a381-28da06cce3f7", "Specify whether the shipment Estimated Export Customs Clearance Date should be a mandatory field. This setting only applies to Export Shipments."),
						RegistryStorageFlags.BranchDepartment,
						false);
				});
			}
		}

		#endregion

		#region Job Services

		public CodeDescriptionBoolRegistryItem JobServices
		{
			get
			{
				return GetItem("JobServices", delegate
				{
					var defaultList = new CodeDescriptionBoolCollection(3);
					defaultList.AddSystemDefined(Constants.FreightServiceType.Codes.Fumigation, Constants.FreightServiceType.Descriptions.Fumigation, true);
					defaultList.AddSystemDefined(Constants.FreightServiceType.Codes.QuarantineInspection, Constants.FreightServiceType.Descriptions.QuarantineInspection, true);
					defaultList.AddSystemDefined(Constants.FreightServiceType.Codes.CustomsHold, Constants.FreightServiceType.Descriptions.CustomsHold, true);
					defaultList.AddSystemDefined(Constants.FreightServiceType.Codes.QuarantineUnpack, Constants.FreightServiceType.Descriptions.QuarantineUnpack, true);
					defaultList.AddSystemDefined(Constants.FreightServiceType.Codes.Tailgate, Constants.FreightServiceType.Descriptions.Tailgate, true);
					defaultList.AddSystemDefined(Constants.FreightServiceType.Codes.ExtraInspection, Constants.FreightServiceType.Descriptions.ExtraInspection, true);
					defaultList.AddSystemDefined(Constants.FreightServiceType.Codes.Survey, Constants.FreightServiceType.Descriptions.Survey, true);
					defaultList.AddSystemDefined(Constants.FreightServiceType.Codes.Cleaning, Constants.FreightServiceType.Descriptions.Cleaning, true);
					defaultList.AddSystemDefined(Constants.FreightServiceType.Codes.Washing, Constants.FreightServiceType.Descriptions.Washing, true);
					defaultList.AddSystemDefined(Constants.FreightServiceType.Codes.SteamCleaning, Constants.FreightServiceType.Descriptions.SteamCleaning, true);
					defaultList.AddSystemDefined(Constants.FreightServiceType.Codes.FCLContainerStorage, Constants.FreightServiceType.Descriptions.FCLContainerStorage, true);
					defaultList.AddSystemDefined(Constants.FreightServiceType.Codes.FCLUnderbondStorage, Constants.FreightServiceType.Descriptions.FCLUnderbondStorage, true);
					defaultList.AddSystemDefined(Constants.FreightServiceType.Codes.Overpack, Constants.FreightServiceType.Descriptions.Overpack, true);
					defaultList.AddSystemDefined(Constants.FreightServiceType.Codes.ControlByCustoms, Constants.FreightServiceType.Descriptions.ControlByCustoms, true);
					defaultList.AddSystemDefined(Constants.FreightServiceType.Codes.BreakDown, Constants.FreightServiceType.Descriptions.BreakDown, true);

					return new CodeDescriptionBoolRegistryItem(
						"JobServices",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("c7ed03e6-5c5f-4484-a8a2-b53cc34fb808", "Job Services"),
						ResString.GetMultilingualString("c9b26275-e8b2-4be4-86dd-03946874ab1b", "Services that can be performed on a Shipment or Container"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("2eeb3128-c7aa-4590-aac2-ad845ad1c022", "Not Used"), false),
						defaultList);
				});
			}
		}

		#endregion

		#endregion

		#region Spot Quote\Quick Booking

		public CodePairRegistryItem DefaultBookingNewButton
		{
			get
			{
				return GetItem<CodePairRegistryItem>("DefaultBookingNewButton", delegate
				{
					return new CodePairRegistryItem(
						"DefaultBookingNewButton",
						Categories.Freight_Bookings,
						ResString.GetMultilingualString("197553f3-95e3-4aff-870a-1e0be634cb12", "Default Booking New Button"),
						ResString.GetMultilingualString("6fdd7c79-5b54-4f4d-88a5-9a117f17c85b", "Setup Default Action For 'New' Button on Bookings Module."),
						new CodeDescriptionPairListProvider(() => new Freight.BookingNewButtonLabelList()),
						RegistryStorageFlags.All,
						Freight.BookingNewButtonLabelList.Codes.BookingWithQuote);
				});
			}
		}

		#endregion

		#region Routing

		public CodePairRegistryItem DefaultRoutingLegStatus
		{
			get
			{
				return GetItem<CodePairRegistryItem>("DefaultRoutingLegStatus", delegate
				{
					var lookUpListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var lookUpList = new CodeDescriptionPairList();
						lookUpList.AddPair(Constants.TransportStatus.Confirmed, Constants.TransportStatusDescriptions.Confirmed);
						lookUpList.AddPair(Constants.TransportStatus.Planned, Constants.TransportStatusDescriptions.Planned);
						return lookUpList;
					});

					return new CodePairRegistryItem(
						"DefaultRoutingLegStatus",
						Categories.Freight_Routing,
						ResString.GetMultilingualString("114f565d-7f19-4779-96af-2f538c68796b", "Default Routing Leg Status"),
						ResString.GetMultilingualString("5b96c07d-def6-43b7-be17-3660f71c3691", "The default status of a routing leg."),
						lookUpListProvider,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						Constants.TransportStatus.Confirmed);
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public StringRegistryItem S8CargoWebServiceURL_Primary
		{
			get
			{
				return GetItem<StringRegistryItem>("S8CargoWebServiceURL_Primary", delegate
				{
					string @default = "https://s8a.cargowise.com/s8cwebsv/s8cwebsv.asmx";
#if DEBUG
					@default = "https://s8test.cargowise.com/s8cwebsv/s8cwebsv.asmx";
#endif

					return new StringRegistryItem(
						"S8CargoWebServiceURL_Primary",
						Categories.Freight_Routing,
						(NoResString)"Web Service URL - Primary",
						(NoResString)"The Primary URL used to connect to S8 Cargo's Web Service",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						@default);
				});
			}
		}

		public StringRegistryItem S8CargoWebServiceURL_Faillover
		{
			get
			{
				return GetItem<StringRegistryItem>("S8CargoWebServiceURL_Faillover", delegate
				{
					string @default = "https://s8b.cargowise.com/s8cwebsv/s8cwebsv.asmx";
#if DEBUG
					@default = "https://s8test.cargowise.com/s8cwebsv/s8cwebsv.asmx";
#endif

					return new StringRegistryItem(
						"S8CargoWebServiceURL_Faillover",
						Categories.Freight_Routing,
						(NoResString)"Web Service URL - Fail-Over",
						(NoResString)"The Fail-Over URL used to connect to S8 Cargo's Web Service",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						@default);
				});
			}
		}

		public StringRegistryItem S8CargoWebServiceURL_Current
		{
			get
			{
				return GetItem<StringRegistryItem>("S8CargoWebServiceURL_Current", delegate
				{
					return new StringRegistryItem(
						"S8CargoWebServiceURL_Current",
						Categories.Freight_Routing,
						(NoResString)"Web Service URL - Current",
						(NoResString)"The URL used to connect to S8 Cargo's Web Service",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						string.Empty);
				});
			}
		}

		public StringRegistryItem S8CargoWebServiceUserID
		{
			get
			{
				return GetItem<StringRegistryItem>("S8CargoWebServiceUserID", delegate
				{
					string @default = "Enterprise";
#if DEBUG
					@default = "CWISETEST";
#endif

					return new StringRegistryItem(
						"S8CargoWebServiceUserID",
						Categories.Freight_Routing,
						(NoResString)"Web Service User ID",
						(NoResString)"The User ID used to connect to S8 Cargo's Web Service",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						@default);
				});
			}
		}

		public StringRegistryItem S8CargoWebServicePassword
		{
			get
			{
				return GetItem<StringRegistryItem>("SPServCWargoice8DEncrypted", delegate
				{
					string @default = "";
					@default += "xryozdaubczt";
					@default += (1080 - 1063 + 3 - 10).ToString();
					@default = @default.Replace("y", "").Replace("x", "");
					@default += "n";
					@default = @default.Replace("a", "").Replace("z", "");
					@default = "P" + @default;
					@default = @default.Replace("b", "");
#if DEBUG
					@default = "TestDevelop";
#endif

					StringRegistryItem item = new StringRegistryItem(
						"SPServCWargoice8DEncrypted",
						Categories.Freight_Routing,
						(NoResString)"Web Service Password",
						null,
						new SecureStringRegistryDataType(),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						@default);

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return item;
				});
			}
		}

		public StringRegistryItem S8CargoWebServiceSessionToken
		{
			get
			{
				return GetItem<StringRegistryItem>("S8CargoWebServiceSessionToken", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"S8CargoWebServiceSessionToken",
						Categories.Freight_Routing,
						(NoResString)"Web Service Session Token",
						(NoResString)"The Current Token for transactions to S8 Cargo's Web Service",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						"");
					return item;
				});
			}
		}

		public StringRegistryItem S8CargoWebServiceSessionFailOverToken
		{
			get
			{
				return GetItem<StringRegistryItem>("S8CargoWebServiceSessionFailOverToken", delegate
				{
					var item = new StringRegistryItem(
						"S8CargoWebServiceSessionFailOverToken",
						Categories.Freight_Routing,
						(NoResString)"Web Service Session Fail-Over Token",
						(NoResString)"The Current Fail-Over Token for transactions to S8 Cargo's Web Service",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						"");
					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
					return item;
				});
			}
		}

		public IntRegistryItem S8CommunicationTimeoutInSeconds
		{
			get
			{
				return GetItem("S8CommunicationTimeoutInSeconds", delegate
				{
					return new IntRegistryItem(
						"S8CommunicationTimeoutInSeconds",
						Categories.Freight_Routing,
						ResString.GetMultilingualString("3a99ef38-0d21-4164-8285-411e2c1bb452", "Communication Timeout In Seconds"),
						ResString.GetMultilingualString("c2018a14-dc3b-4005-a7a3-3e588620b972", "Communication Timeout in seconds for the connection."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 10,
						minValue: 1,
						maxValue: 60);
				});
			}
		}

		public IntRegistryItem S8LoginSuppressionTimeoutInMinutes
		{
			get
			{
				return GetItem("S8LoginSuppressionTimeoutInMinutes", delegate
				{
					return new IntRegistryItem(
						"S8LoginSuppressionTimeoutInMinutes",
						Categories.Freight_Routing,
						(NoResString)"Login Suppression Timeout In Minutes",
						(NoResString)"Suppression Timeout is minutes of next login after previous login failed.",
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						defaultValue: 20,
						minValue: 0,
						maxValue: 60);
				});
			}
		}
		#endregion

		#region AutomaticUpdatingofPlannedLegs_Air

		public BooleanRegistryItem AutomaticUpdatingofPlannedLegs_Air
		{
			get
			{
				return GetItem("AutomaticUpdatingofPlannedLegs_Air", delegate
				{
					return new BooleanRegistryItem(
						"AutomaticUpdatingofPlannedLegs_Air",
						Categories.Freight_Routing_AutomaticUpdatingofPlannedLegs,
						ResString.GetMultilingualString("477534ae-ffe5-4556-99e1-ba29669f9ab5", "Air"),
						ResString.GetMultilingualString("4190742c-cb90-40ff-a3e5-54d27f8c5056", "This will enable the automatic updating of Planned Legs from Actual Events as they are received/processed."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region AutomaticUpdatingofPlannedLegs_Sea

		public BooleanRegistryItem AutomaticUpdatingofPlannedLegs_Sea_FromBookingConfirmation
		{
			get
			{
				return GetItem("AutomaticUpdatingofPlannedLegs_Sea_FromBookingConfirmation", delegate
				{
					return new BooleanRegistryItem(
						"AutomaticUpdatingofPlannedLegs_Sea_FromBookingConfirmation",
						Categories.Freight_Routing_AutomaticUpdatingofPlannedLegs_Sea,
						ResString.GetMultilingualString("6dc11b4c-a516-4b7c-a3cc-8bbcddf577c5", "From Booking Confirmation"),
						ResString.GetMultilingualString("030d3378-c497-49ed-9fb8-ed636f2214da", "If enabled, the system will automatically update routing legs on Consols using information received in the Booking Confirmation message.\r\nIf disabled, the system will still process Booking Confirmation message, update relevant details on Consols, however, it will not override Consol routing legs.\r\nRouting details provided in Booking Confirmation message are visible in the Routing > Actual > Booking Confirmation grid, for review and manual update as required."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public BooleanRegistryItem AutomaticUpdatingofPlannedLegs_Sea_FromContainerAutomationEvents
		{
			get
			{
				return GetItem("AutomaticUpdatingofPlannedLegs_Sea_FromContainerAutomationEvents", delegate
				{
					return new BooleanRegistryItem(
						"AutomaticUpdatingofPlannedLegs_Sea_FromContainerAutomationEvents",
						Categories.Freight_Routing_AutomaticUpdatingofPlannedLegs_Sea,
						ResString.GetMultilingualString("e37df5e0-2fc1-416a-bf48-3411e61c4a0f", "From Container Automation Events"),
						ResString.GetMultilingualString("4adba645-f190-40f2-9d79-434838f84c3e", "If enabled, Consol routing legs with matching load and/or discharge ports will be automatically updated with vessel name, voyage number and dates from Container Automation Events.\r\nIf disabled, Consol routing legs will still be automatically updated with estimated and actual dates matching load and/or discharge ports on the legs, but vessel and voyage details on these legs will not be automatically overridden.\r\nRouting details provided in the latest Container Automation messages are visible in the Routing > Actual > Events grid as legs constructed from these events for review and manual update as required."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#endregion

		#region Notifications

		#region Sailing Schedules

		#region ScheduleChangeNotificationGroups

		public GuidRegistryItem SeaScheduleChangeNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("SeaScheduleChangeNotificationGroup", delegate
				{
					return new GuidRegistryItem(
						"SeaScheduleChangeNotificationGroup",
						Categories.Freight_Notifications_SailingSchedules,
						ResString.GetMultilingualString("7c0f1037-b3ab-46d7-ac1a-5fb1047b75bb", "Sailing Schedule Change Notification Group"),
						ResString.GetMultilingualString("b55572f4-46cd-480e-a1fe-69cadc81e75c", "Staff group to notify when the dates of one or more sailing schedules have changed."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueOptional | RegistryOptions.PreserveTestValue,
						Core.Constants.Groups.AllPK);
				});
			}
		}

		public GuidRegistryItem AirScheduleChangeNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("AirScheduleChangeNotificationGroup", delegate
				{
					return new GuidRegistryItem(
						"AirScheduleChangeNotificationGroup",
						Categories.Freight_Notifications_SailingSchedules,
						ResString.GetMultilingualString("64509907-f08f-4c1e-8492-cb2e1ae3d6c1", "Flight Schedule Change Notification Group"),
						ResString.GetMultilingualString("7b2f3331-e834-410d-9a51-192b47814bcf", "Staff group to notify when the dates of one or more flight schedules have changed."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueOptional | RegistryOptions.PreserveTestValue,
						Core.Constants.Groups.AllPK);
				});
			}
		}

		public GuidRegistryItem RoadScheduleChangeNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("RoadScheduleChangeNotificationGroup", delegate
				{
					return new GuidRegistryItem(
						"RoadScheduleChangeNotificationGroup",
						Categories.Freight_Notifications_SailingSchedules,
						ResString.GetMultilingualString("bc45631f-ff15-4f6d-b9ff-4299286a9f8b", "Trucking Schedule Change Notification Group"),
						ResString.GetMultilingualString("3b8aaa36-d83e-4fd3-be90-fe97d686df62", "Staff group to notify when the dates of one or more trucking schedules have changed."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueOptional | RegistryOptions.PreserveTestValue,
						Core.Constants.Groups.AllPK);
				});
			}
		}

		public GuidRegistryItem RailScheduleChangeNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("RailScheduleChangeNotificationGroup", delegate
				{
					return new GuidRegistryItem(
						"RailScheduleChangeNotificationGroup",
						Categories.Freight_Notifications_SailingSchedules,
						ResString.GetMultilingualString("b272afea-109b-469a-adcb-163ffaac5b6b", "Rail Schedule Change Notification Group"),
						ResString.GetMultilingualString("d99f83cb-b459-4627-922e-7d217b0775a0", "Staff group to notify when the dates of one or more rail schedules have changed."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueOptional | RegistryOptions.PreserveTestValue,
						Core.Constants.Groups.AllPK);
				});
			}
		}

		#endregion

		#endregion

		#region HourlyFreightNotificationEmailsLastSent
		#region SuppressResourceStringsCheckRegion

		public DateTimeRegistryItem HourlyFreightNotificationEmailsLastSent
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("HourlyFreightNotificationEmailsLastSent", delegate
				{
					return new DateTimeRegistryItem(
						"HourlyFreightNotificationEmailsLastSent",
						Categories.Freight_Notifications,
						(NoResString)"Sailing/Flight/Trucking/Rail Hourly Freight Notification Email Last Sent",
						(NoResString)"Sailing/Flight/Trucking/Rail Hourly Freight Notification Email Last Sent",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						DateTime.MinValue);
				});
			}
		}

		#endregion
		#endregion

		#endregion

		#region 1-Stop Integration

		#region OneStopContainerEventsEnabled

		public BooleanRegistryItem HasOneStopAUContainerIntegration
		{
			get
			{
				return GetItem<BooleanRegistryItem>("HasOneStopAUContainerIntegration", delegate
				{
					return new BooleanRegistryItem("HasOneStopAUContainerIntegration", Categories.Freight_ComTrac,
						(NoResString)"Factory Enable AU",
						(NoResString)"This is for support to enable OneStop AU.",
						RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
				});
			}
		}

		public static bool HasOneStopAU
		{
			get { return Instance.HasOneStopAUContainerIntegration.Value; }
			set { Instance.HasOneStopAUContainerIntegration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public BooleanRegistryItem HasOneStopNZContainerIntegration
		{
			get
			{
				return GetItem<BooleanRegistryItem>("HasOneStopNZContainerIntegration", delegate
				{
					return new BooleanRegistryItem("HasOneStopNZContainerIntegration", Categories.Freight_ComTrac,
						(NoResString)"Factory Enable NZ",
						(NoResString)"This is for support to enable OneStop NZ.",
						RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
				});
			}
		}

		public static bool HasOneStopNZ
		{
			get { return Instance.HasOneStopNZContainerIntegration.Value; }
			set { Instance.HasOneStopNZContainerIntegration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public static bool OneStopAUContainerIntegrationIsEnabled
		{
			get { return HasOneStopAU && Instance.OneStopContainerEventsEnabled.Value; }
		}

		public static bool OneStopNZContainerIntegrationIsEnabled
		{
			get { return HasOneStopNZ && Instance.OneStopContainerEventsEnabledNZ.Value; }
		}

		public BooleanRegistryItem OneStopContainerEventsEnabled
		{
			get
			{
				return GetItem("OneStopContainerEventsEnabled", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"OneStopContainerEventsEnabled",
						Categories.Freight_ComTrac,
						ResString.GetMultilingualString("917052ab-93e2-4ae4-9b4b-d7481c6f0a9c", "Enable ComTrac AU Container Event Date Synchronization"),
						ResString.GetMultilingualString("256e47fa-9e3d-47c8-9c1f-422b623d7737", @"Enable automatic synchronization of Australian container event dates from 1-Stop.
Only containers registered after this setting have been applied will have event dates synchronized."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						true);

					if (!HasOneStopAU)
					{
						result.Options |= RegistryOptions.IsHidden;
					}

					return result;
				});
			}
		}

		public BooleanRegistryItem OneStopContainerEventsEnabledNZ
		{
			get
			{
				return GetItem("OneStopContainerEventsEnabledNZ", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"OneStopContainerEventsEnabledNZ",
						Categories.Freight_ComTrac,
						ResString.GetMultilingualString("e3813b5a-1cc6-47b8-a348-eb032d77e90b", "Enable ComTrac NZ Container Event Date Synchronization"),
						ResString.GetMultilingualString("65f00c75-6703-478b-b98f-97ae8cba1416", @"Enable automatic synchronization of New Zealand container event dates from 1-Stop.
Only containers registered after this setting have been applied will have event dates synchronized."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);

					if (!HasOneStopNZ)
					{
						result.Options |= RegistryOptions.IsHidden;
					}

					return result;
				});
			}
		}

		#endregion

		#region ComTracTestMode
		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem ComTracTestMode
		{
			get
			{
				return GetItem("ComTracTestMode",
								 () => new BooleanRegistryItem("ComTracTestMode",
															 Categories.Freight_ComTrac,
															 (NoResString)"ComTrac Test Mode",
															 (NoResString)"Enable this to send subscription emails to test server (stop20@test.1-stop.biz)",
															 RegistryStorageFlags.System,
															 RegistryOptions.IsOnlyForSupport,
															 false));
			}
		}

		#endregion
		#endregion

		#region OneStopNotificationGroup

		public GuidRegistryItem OneStopNotificationGroup
		{
			get
			{
				return GetItem("OneStopNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"OneStopNotificationGroup",
						Categories.Freight_ComTrac,
						ResString.GetMultilingualString("f6ace5d4-2bec-4d5b-9d7e-195461276729", "ComTrac Notification Group"),
						ResString.GetMultilingualString("76016b72-9d06-4363-a836-e4427bab1892", "Staff group to notify of ComTrac conditions that require attention."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsValueOptional | RegistryOptions.PreserveTestValue,
						Core.Constants.Groups.AllPK);

					if (!HasOneStopAU && !HasOneStopNZ)
					{
						result.Options |= RegistryOptions.IsHidden;
					}
					return result;
				});
			}
		}

		#endregion

		#region SailingSchedulesFeedLastReceived

		public ZDateTime SailingSchedulesFeedLastReceived
		{
			get
			{
				if (sailingSchedulesFeedLastReceivedLastCached.IsEmpty || ZDateTime.Now > sailingSchedulesFeedLastReceivedLastCached.AddHours(2))
				{
					sailingSchedulesFeedLastReceivedCachedValue = SailingSchedulesFeedLastReceivedItem.Value;
					if (!sailingSchedulesFeedLastReceivedCachedValue.IsValid)
					{
						sailingSchedulesFeedLastReceivedCachedValue = ZDateTime.Empty;
					}
					sailingSchedulesFeedLastReceivedLastCached = ZDateTime.Now;
				}
				return sailingSchedulesFeedLastReceivedCachedValue;
			}
			set
			{
				SailingSchedulesFeedLastReceivedItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.IsEmpty ? DateTime.MinValue : value.ToDateTime());
				sailingSchedulesFeedLastReceivedLastCached = ZDateTime.Empty;
			}
		}
		ZDateTime sailingSchedulesFeedLastReceivedCachedValue;
		ZDateTime sailingSchedulesFeedLastReceivedLastCached;

		#region SuppressResourceStringsCheckRegion
		internal DateTimeRegistryItem SailingSchedulesFeedLastReceivedItem
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("OneStopSailingSchedulesLastReceived", delegate
				{
					return new DateTimeRegistryItem(
						"OneStopSailingSchedulesLastReceived",
						Categories.Freight_SailingScheduleFeed,
						(NoResString)"Sailing schedule feed data was last received",
						(NoResString)"Sailing schedule feed data was last received",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						DateTime.MinValue);
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region Chargeable

		#region Implementation

		public readonly string[] WeightChargableTransportModes = new string[]
			{
				Constants.TransportModes.Air,
				Constants.TransportModes.AirSea,
				Constants.TransportModes.Road,
				Constants.TransportModes.Courier,
				Constants.TransportModes.Mail,
				Constants.TransportModes.WarehouseHandling,
				Constants.TransportModes.Storage
			};

		public readonly string[] VolumeChargableTransportModes = new string[]
			{
				Constants.TransportModes.Rail,
				Constants.TransportModes.Sea,
				Constants.TransportModes.SeaAir
			};

		public string GetDefaultChargeableWeightUnit(bool metric)
		{
			return metric ? Constants.Weight.Kilograms : Constants.Weight.Pounds;
		}

		public string GetDefaultChargeableVolumeUnit(bool metric)
		{
			return metric ? Constants.Volume.CubicMetres : Constants.Volume.CubicFeet;
		}

		#region Hints

		MultilingualString GetHintForTranportMode(string transportMode, ChargeableFactor defaultFactor)
		{
			if (WeightChargableTransportModes.Contains(transportMode))
			{
				return GetChargeableFactorVolumeToWeightHint(defaultFactor);
			}
			else if (VolumeChargableTransportModes.Contains(transportMode))
			{
				return GetChargeableFactorWeightToVolumeHint(defaultFactor);
			}

			return null;
		}

		public static MultilingualString GetChargeableFactorVolumeToWeightHint(ChargeableFactor defaultFactor)
		{
			return MultilingualString.Join("\r\n\r\n",
				ResString.GetMultilingualString("a78d1a2d-0f1d-42b6-8f50-5e2d4cc6e57a", "The chargeable factor will be used to convert the actual volume into a chargeable weight.\r\n\r\nThe chargeable weight is the greater of the actual weight or actual volume divided by the chargeable factor."),
				ResString.GetMultilingualString("73173540-53d9-4435-824a-a137513c77b3", "Max of [ weight(KG) , volume(CM3) / factor ] = KG\r\n\r\nDefault chargeable factor is {0}.", defaultFactor.MetricFactorForBinding.ConversionFactor.ToShortString()),
				ResString.GetMultilingualString("cebe81ad-8c9c-4bf9-ac2c-e37d58b84bdd", "Max of [ weight(LB) , volume(CI) / factor ] = LB\r\n\r\nDefault chargeable factor is {0}.", defaultFactor.ImperialFactorForBinding.ConversionFactor.ToShortString())
			);
		}

		static MultilingualString GetChargeableFactorWeightToVolumeHint(ChargeableFactor defaultFactor)
		{
			return MultilingualString.Join("\r\n\r\n",
				ResString.GetMultilingualString("f5f980b2-f0dd-4abd-8050-2f0c126f31f5", "The chargeable factor will be used to convert the actual weight into a chargeable volume.\r\n\r\nThe chargeable volume is the greater of the actual volume or actual weight divided by the chargeable factor."),
				ResString.GetMultilingualString("e87554f6-23f0-4544-9770-1263ba86af1f", "Max of [ volume(M3) , weight(KG) / factor ] = M3\r\n\r\nDefault chargeable factor is {0}.", defaultFactor.MetricFactorForBinding.ConversionFactor.ToShortString()),
				ResString.GetMultilingualString("064cec6c-dd2e-4c67-beb5-b9731ebea561", "Max of [ volume(CF) , weight(LB) / factor ] = CF\r\n\r\nDefault chargeable factor is {0}.", defaultFactor.ImperialFactorForBinding.ConversionFactor.ToShortString())
			);
		}

		#endregion

		#endregion

		#region Domestic Chargeable

		#region DomesticChargeableFactorAir

		public ChargeableFactorRegistryItem DomesticChargeableFactorAir
		{
			get
			{
				return GetItem<ChargeableFactorRegistryItem>("DomesticChargeableFactorAir", () =>
				{
					var defaultFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.Air, ConversionFactor.Standard.Imperial.Domestic);

					return new ChargeableFactorRegistryItem(
						"DomesticChargeableFactorAir",
						Categories.Freight_Chargeable_DomesticChargeable,
						ResString.GetMultilingualString("a5ef1060-d1b5-4aab-983e-b4de21f83b0a", "Chargeable Factor for Air"),
						GetHintForTranportMode(Constants.TransportModes.Air, defaultFactor),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						defaultFactor);
				});
			}
		}

		#endregion

		#region DomesticChargeableFactorRoad

		public ChargeableFactorRegistryItem DomesticChargeableFactorRoad
		{
			get
			{
				return GetItem<ChargeableFactorRegistryItem>("DomesticChargeableFactorRoad", () =>
				{
					var defaultFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.Road, ConversionFactor.Standard.Imperial.Domestic);

					return new ChargeableFactorRegistryItem(
						"DomesticChargeableFactorRoad",
						Categories.Freight_Chargeable_DomesticChargeable,
						ResString.GetMultilingualString("3fae7042-ed08-4720-a559-62575a19bb18", "Chargeable Factor for Road"),
						GetHintForTranportMode(Constants.TransportModes.Road, defaultFactor),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						defaultFactor);
				});
			}
		}

		#endregion

		#region DomesticChargeableFactorCourier

		public ChargeableFactorRegistryItem DomesticChargeableFactorCourier
		{
			get
			{
				return GetItem<ChargeableFactorRegistryItem>("DomesticChargeableFactorCourier", () =>
				{
					var defaultFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.Air, ConversionFactor.Standard.Imperial.Domestic);

					return new ChargeableFactorRegistryItem(
						"DomesticChargeableFactorCourier",
						Categories.Freight_Chargeable_DomesticChargeable,
						ResString.GetMultilingualString("0fc5c07b-6eed-4c32-8018-db1fcef5c154", "Chargeable Factor for Courier"),
						GetHintForTranportMode(Constants.TransportModes.Courier, defaultFactor),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						defaultFactor);
				});
			}
		}

		#endregion

		#region DomesticChargeableFactorSea

		public ChargeableFactorRegistryItem DomesticChargeableFactorSea
		{
			get
			{
				return GetItem<ChargeableFactorRegistryItem>("DomesticChargeableFactorSea", () =>
				{
					var defaultFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.Sea, ConversionFactor.Standard.Imperial.Sea);

					return new ChargeableFactorRegistryItem(
						"DomesticChargeableFactorSea",
						Categories.Freight_Chargeable_DomesticChargeable,
						ResString.GetMultilingualString("8df2cb9a-c38b-433d-a0a1-59266952546b", "Chargeable Factor for Sea"),
						GetHintForTranportMode(Constants.TransportModes.Sea, defaultFactor),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						defaultFactor);
				});
			}
		}

		#endregion

		#region DomesticChargeableFactorRail

		public ChargeableFactorRegistryItem DomesticChargeableFactorRail
		{
			get
			{
				return GetItem<ChargeableFactorRegistryItem>("DomesticChargeableFactorRail", () =>
				{
					var defaultFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.Rail, ConversionFactor.Standard.Imperial.Rail);

					return new ChargeableFactorRegistryItem(
						"DomesticChargeableFactorRail",
						Categories.Freight_Chargeable_DomesticChargeable,
						ResString.GetMultilingualString("26166dd3-e694-426c-8142-90a04458ec14", "Chargeable Factor for Rail"),
						GetHintForTranportMode(Constants.TransportModes.Rail, defaultFactor),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						defaultFactor);
				});
			}
		}

		#endregion

		#endregion

		#region International Chargeable

		#region InternationalChargeableFactorAir

		public ChargeableFactorRegistryItem InternationalChargeableFactorAir
		{
			get
			{
				return GetItem("InternationalChargeableFactorAir", () =>
				{
					var defaultFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.Air, ConversionFactor.Standard.Imperial.Air);

					return new ChargeableFactorRegistryItem(
						"InternationalChargeableFactorAir",
						Categories.Freight_Chargeable_InternationalChargeable,
						ResString.GetMultilingualString("f15929ef-9c01-4349-9367-5c37b103cdd5", "Chargeable Factor for Air"),
						GetHintForTranportMode(Constants.TransportModes.Air, defaultFactor),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						defaultFactor);
				});
			}
		}

		#endregion

		#region InternationalChargeableFactorRoad

		public ChargeableFactorRegistryItem InternationalChargeableFactorRoad
		{
			get
			{
				return GetItem<ChargeableFactorRegistryItem>("InternationalChargeableFactorRoad", () =>
				{
					var defaultFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.Road, ConversionFactor.Standard.Imperial.Domestic);

					return new ChargeableFactorRegistryItem(
						"InternationalChargeableFactorRoad",
						Categories.Freight_Chargeable_InternationalChargeable,
						ResString.GetMultilingualString("8c75d2f4-7682-49b7-9379-04b8a80b4997", "Chargeable Factor for Road"),
						GetHintForTranportMode(Constants.TransportModes.Road, defaultFactor),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						defaultFactor);
				});
			}
		}

		#endregion

		#region InternationalChargeableFactorCourier

		public ChargeableFactorRegistryItem InternationalChargeableFactorCourier
		{
			get
			{
				return GetItem<ChargeableFactorRegistryItem>("InternationalChargeableFactorCourier", () =>
				{
					var defaultFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.Air, ConversionFactor.Standard.Imperial.Domestic);

					return new ChargeableFactorRegistryItem(
						"InternationalChargeableFactorCourier",
						Categories.Freight_Chargeable_InternationalChargeable,
						ResString.GetMultilingualString("0a74972b-694b-4f19-a840-8013d198ac5f", "Chargeable Factor for Courier"),
						GetHintForTranportMode(Constants.TransportModes.Courier, defaultFactor),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						defaultFactor);
				});
			}
		}

		#endregion

		#region InternationalChargeableFactorSea

		public ChargeableFactorRegistryItem InternationalChargeableFactorSea
		{
			get
			{
				return GetItem("InternationalChargeableFactorSea", delegate
				{
					var defaultFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.Sea, ConversionFactor.Standard.Imperial.Sea);
					return new ChargeableFactorRegistryItem(
						"InternationalChargeableFactorSea",
						Categories.Freight_Chargeable_InternationalChargeable,
						ResString.GetMultilingualString("d41c6f1c-fcb2-456f-9dcf-4e57d4dbbf2c", "Chargeable Factor for Sea"),
						GetHintForTranportMode(Constants.TransportModes.Sea, defaultFactor),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						defaultFactor);
				});
			}
		}

		#endregion

		#region InternationalChargeableFactorRail

		public ChargeableFactorRegistryItem InternationalChargeableFactorRail
		{
			get
			{
				return GetItem<ChargeableFactorRegistryItem>("InternationalChargeableFactorRail", () =>
				{
					var defaultFactor = new ChargeableFactor(ConversionFactor.Standard.Metric.Rail, ConversionFactor.Standard.Imperial.Rail);
					return new ChargeableFactorRegistryItem(
						"InternationalChargeableFactorRail",
						Categories.Freight_Chargeable_InternationalChargeable,
						ResString.GetMultilingualString("619483d5-3b34-4b81-b065-9fc905bf4a8f", "Chargeable Factor for Rail"),
						GetHintForTranportMode(Constants.TransportModes.Rail, defaultFactor),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						defaultFactor);
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region Loading Meters

		public BooleanRegistryItem EnableRoadLoadingMeters
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableRoadLoadingMeters", delegate
				{
					return new BooleanRegistryItem(
						"EnableRoadLoadingMeters",
						Categories.Freight_Chargeable_LoadingMeters,
						ResString.GetMultilingualString("b0edb2da-a164-4b09-86eb-7957b6720c47", "Enable Road Loading Meters"),
						ResString.GetMultilingualString("14e52191-0fac-40e3-8835-f6281884c377", "Override default to Yes to expose Loading Meters on Shipments & Consols."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public DecimalRegistryItem RoadLoadingMetersWeightPerLDM
		{
			get
			{
				return GetItem<DecimalRegistryItem>("RoadLoadingMetersWeightPerLDM", delegate
				{
					return new DecimalRegistryItem(
						"RoadLoadingMetersWeightPerLDM",
						Categories.Freight_Chargeable_LoadingMeters,
						ResString.GetMultilingualString("a8185e9b-e4ea-4eac-b0a1-ed42fe902585", "Weight Per Loading Meter"),
						ResString.GetMultilingualString("351cead3-c8c6-4d8a-982f-1ce576b67d2a", "Equivalent weight in kilograms of Loading Meters will be calculated using this factor."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						1750m);
				});
			}
		}

		#endregion

		#region Customs Permit/Clearance Numbers

		public CodeDescriptionPairListRegistryItem CustomsPermitClearanceNumbers
		{
			get
			{
				return GetItem<CodeDescriptionPairListRegistryItem>("CustomsPermitClearanceNumbers", delegate
				{
					CodeDescriptionPairList @default = new CodeDescriptionPairList();
					@default.AddPair("TSN", ResString.GetMultilingualString("77630e85-3c95-4f46-8165-8bb8a788acd6", "Transhipment Number"));
					@default.AddPair("ATA", ResString.GetMultilingualString("946640ba-b98d-4ad5-a589-8fc1aaa499d4", "ATA Carnet Number"));

					return new CodeDescriptionPairListRegistryItem(
						"CustomsPermitClearanceNumbers",
						Categories.Freight_CustomsNumbers,
						ResString.GetMultilingualString("47a55c92-02aa-4c0e-9683-b0816293eefb", "Customs Permit/Clearance Number Types"),
						ResString.GetMultilingualString("cefd5bf1-c057-434d-aab4-b94674ea9cae", "This list defines the customs permit/clearance types available on the Shipment screen for the currently chosen country/region. Note that this only applies to countries/regions which do not have a defined list of permit/clearance numbers."),
						3,
						RegistryStorageFlags.Company,
						@default);
				});
			}
		}

		#endregion

		#region Customs Additional Reference Numbers

		public CustomsReferenceNumberTypesRegistryItem CustomsAdditionalReferenceNumbers
		{
			get
			{
				return GetItem("CustomsAdditionalReferenceNumbers", () =>
				{
					var defaultValue = new CustomsReferenceNumberTypeCollection();
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.COC, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|COC", "Customs Office Code (Override)"));
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|AMS", "AMS Number"));
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.UBR, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|UBR", "Under Bond Approval Reference Number"));
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|CON", "Carrier Contract Number"), true);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CLC, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|CLC", "Client Contract Number"), true);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|BKG", "Carrier Booking Reference"), false);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.RLB, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|RLB", "Railway Bill Number"), false);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.UniqueConsignmentReference, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|UCR", "External (3rd party) Unique Consignment Reference"), false);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomsAuthorisationReference, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|CAR", "Customs Authorization Reference"), false);  // The correct spelling is authorisation.  See the specifications, "Automated Fallback in the CCS-UK Environment. Specification for Forwarder Systems. Version 1.1 (For distribution)" . Sigh.
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.LetterOfCreditNumber, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|LCR", "Letter Of Credit Number"));
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoAdvice, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|ACA", "Pentant Advance Cargo Advice Reference"), false);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoControlNumber, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|CCN", "Cargo Control Number"), false);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.PreviousCargoControlNumber, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|PCN", "Previous Cargo Control Number"), false);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BagReference, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|BAG", "Courier Bag Reference"), true);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CourierConsignmentReference, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|COU", "Courier Consignment Reference"), true);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|NAC", "Contract Named Account"), true);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.OtherAgentReference, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|OAG", "Other Agent Reference"), true);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CarrierQuoteNumber, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|CQN", "Carrier Quote Number"), false);

					if (RatingDataRegistry.Instance.EnableSpotRatingBehaviourFeature.Value)
					{
						defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.SpotReference, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|SPO", "Spot Rate"), false);
					}

					defaultValue.Add(TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceive, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|TWR", "Transit Warehouse Receive"), true);
					defaultValue.Add(CustomsReferenceNumberType.eHubInterchangeReference.HIR, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|HIR", "eHub Interchange Reference"), true);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|ACI", "Advance Cargo Information Reference"), false);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ImportSecurityFilingReference, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|ISF", "US Import Security Filing (ISF) Reference"), true);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.DeclarationReference, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|JDR", "Declaration Reference"), true);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|CLR", "Customer Load Reference"), false);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CarrierMessageReference, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|CMR", "Carrier Message Reference"), true);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ExporterEORINumber, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumberList|EOE", "Exporter EORI Number"), true);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ImporterEORINumber, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumberList|EOI", "Importer EORI Number"), true);
					defaultValue.Add(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoTrackingNote, ResString.GetMultilingualString("Freight|CustomsAdditionalReferenceNumbersList|CTK", "Cargo Tracking Note"), true);

					return new CustomsReferenceNumberTypesRegistryItem(
						"CustomsAdditionalReferenceNumbers",
						Categories.Freight_CustomsNumbers,
						ResString.GetMultilingualString("048133b3-e050-4de4-a123-e572816d670c", "Customs Additional Reference Number Types"),
						ResString.GetMultilingualString("8e4b92b6-1315-4d73-94f8-d0a63b52c083", $"This list defines the additional reference number types available under the Consol > Details > Numbers tab, Containers > Numbers tab, Shipment > Additional Detail > Reference Numbers of the Shipment. These reference types may also be used across modules in {Constants.ProductName}, such as Liner & Agency > Bill of Lading > Containers. These types are in addition to any number types already in the system unless a country specific reference number type already uses the entered type code. The flag \"Edit via UXML only\" restricts the ability to add, edit or delete the corresponding Additional Reference Number via UXML only."),
						RegistryStorageFlags.System,
						defaultValue);
				});
			}
		}

		public MultilingualStringRegistryItem NotClearedByAgentStatement
		{
			get
			{
				MultilingualString defaultValue = null;
				switch (Env.CurrentCompany?.Country?.Code)
				{
					case Constants.CountryCodes.Germany:
						defaultValue = (NoResString)@"! ! ! ! ! A C H T U N G  Z O L L G U T ! ! ! ! !
UNVERZOLLTE WARE, DIE SOFORT INNERHALB DER GESTELLUNGSFRIST ZUSAMMEN
MIT DEM BEIGEFUEGTEN ZOLLVERSANDSCHEIN ( T1 )
!!                                                                  !!
MRN NR.: <NotClearedByAgentNumber> VOM: <NotClearedByAgentIssueDate.Date>  GESTELLUNGSFRIST: <NotClearedByAgentExpiryDate.Date>
!!                                                                  !!
DEM FUER DEN EMPFANGSORT ZUSTAENDIGEN ZOLLAMT VORZUFUEHREN IST.
ZUR ORDNUNGSGEMAESSEN WIEDERGESTELLUNG VERPFLICHTEN WIR/ICH UNS.
ICH/WIR SIND UEBER DIE PFLICHTEN FUER EINE ZOLLGUTBEFOERDERUNG BELEHRT
WORDEN. ZOLLPLOMBEN UND -SCHNUERE DUERFEN NICHT BESCHAEDIGT WERDEN.";
						break;
				}

				return GetItem<MultilingualStringRegistryItem>("NotClearedByAgentStatement", delegate
				{
					return new MultilingualStringRegistryItem(
						"NotClearedByAgentStatement",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("18a6a593-e8c9-43cd-8489-879cdac15988", "Not Cleared by Agent Statement"),
						ResString.GetMultilingualString("eecc97e7-d97a-4fc8-bbce-87bddb6ff0f3", "This statement can be printed on any shipment related document by placing the macro {0} on your document. The text will only print if a 'Not Cleared By Agent' Customs Entry type is chosen on the shipment.", "<EvaluateInnerContent(<Shipment.NotClearedByAgentStatement>)>"),
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Memo),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						defaultValue);
				});
			}
		}

		#endregion

		#region UAEDeliveryOrderNumberPrefix

		public StringRegistryItem UAEDeliveryOrderNumberPrefix
		{
			get
			{
				return GetItem<StringRegistryItem>("UAEDeliveryOrderNumberPrefix", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"UAEDeliveryOrderNumberPrefix",
						Categories.Freight_Shipment_UnitedArabEmirates,
						ResString.GetMultilingualString("9ae52afe-a4cc-4728-8c6f-d5d46c3aa51d", "UAE Delivery Order Number Prefix"),
						ResString.GetMultilingualString("a517648d-c002-45a6-ad6f-93c65c5bb2ef", "This specifies the prefix used on a import sea shipment to generate delivery order number."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						Env.CurrentCompany?.Country?.Code == Core.Constants.CountryCodes.UnitedArabEmirates ? RegistryOptions.Default : RegistryOptions.IsHidden,
						"A0");
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
					return result;
				});
			}
		}

		#endregion

		#region CanadaCargoControlNumbers

		public CodePairRegistryItem CanadaCargoControlNumberCustomization
		{
			get
			{
				return GetItem<CodePairRegistryItem>("CanadaCargoControlNumberCustomization", delegate
				{
					var listProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair(Constants.ShipmentCCNCustomizationTypes.Code.HouseBill, Constants.ShipmentCCNCustomizationTypes.Description.HouseBill);
						list.AddPair(Constants.ShipmentCCNCustomizationTypes.Code.ShipmentNumber, Constants.ShipmentCCNCustomizationTypes.Description.ShipmentNumber);
						list.AddPair(Constants.ShipmentCCNCustomizationTypes.Code.NumberFountain, Constants.ShipmentCCNCustomizationTypes.Description.NumberFountain);
						return list;
					});

					return new CodePairRegistryItem(
						"CanadaCargoControlNumberCustomization",
						Categories.Freight_Shipment_Canada,
						ResString.GetMultilingualString("97c60c14-c907-47e5-b4d3-9b7199b89942", "Cargo Control Number Customization"),
						ResString.GetMultilingualString("f0648363-60f6-4937-9398-11af2a64a928", "Use the last x digits of the shipment HAWB/HBL or shipment number to be the last x digits of the cargo control number, where x is specified in the registry item 'Cargo Control Number House Bill/Shipment Digits'.  If not, the last 8 digits of the CCN will be generated from system number fountain."),
						listProvider,
						RegistryStorageFlags.System,
						Constants.ShipmentCCNCustomizationTypes.Code.HouseBill);
				});
			}
		}

		public IntRegistryItem CanadaCargoControlNumberHBLDigits
		{
			get
			{
				return GetItem<IntRegistryItem>("CanadaCargoControlNumberHBLDigits", delegate
				{
					IntRegistryItem result = new IntRegistryItem(
						"CanadaCargoControlNumberHBLDigits",
						Categories.Freight_Shipment_Canada,
						ResString.GetMultilingualString("14B793DE-5CB3-4909-A82C-F7AAB2292370", "Cargo Control Number House Bill/Shipment Digits"),
						ResString.GetMultilingualString("9A484732-E1B1-4512-B70C-F700EBFEDEA2", "The number of digits of the shipment HAWB/HBL or Shipment number to be used in generating the Cargo Control Number. A value of zero will ensure the entire HAWB/HBL is included."),
						RegistryStorageFlags.System,
						8);
					result.DataType = new IntRegistryDataType(0, 35);
					return result;
				});
			}
		}

		public IntRegistryItem CanadaCargoControlNumberBranchPrefix
		{
			get
			{
				return GetItem<IntRegistryItem>("CanadaCargoControlNumberBranchPrefix", delegate
				{
					IntRegistryItem result = new IntRegistryItem(
						"CanadaCargoControlNumberBranchPrefix",
						Categories.Freight_Shipment_Canada,
						ResString.GetMultilingualString("4b8eba3c-dd10-4278-8123-91abadaffc97", "Cargo Control Number Branch Prefix"),
						ResString.GetMultilingualString("1dc5acd6-887a-4fc9-9d9e-1d92e53eee2f", "If cargo control numbers are generated from system number fountain, this specifies the branch prefix used on generating new numbers.\r\nPrefix must contain two digits, i.e. from 10 to 99."),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						0);
					result.DataType = new IntRegistryDataType(10, 99);
					return result;
				});
			}
		}

		#endregion

		#region Organisations

		#region Default Cartage Company

		public GuidRegistryItem LCLCartageCompany
		{
			get
			{
				return GetItem<GuidRegistryItem>("LCLCartageCompany", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"LCLCartageCompany",
						Categories.Freight_Organizations_DefaultPortTransportCompany,
						ResString.GetMultilingualString("df97fc9d-daa4-4d7d-8c81-544950054179", "LCL Port Transport Company"),
						null,
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.ShippingProvider);
					return result;
				});
			}
		}

		public GuidRegistryItem FCLCartageCompany
		{
			get
			{
				return GetItem<GuidRegistryItem>("FCLCartageCompany", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"FCLCartageCompany",
						Categories.Freight_Organizations_DefaultPortTransportCompany,
						ResString.GetMultilingualString("2a78af35-761f-42b0-893f-59bb84d6af2e", "FCL Port Transport Company"),
						null,
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.ShippingProvider);
					return result;
				});
			}
		}

		public GuidRegistryItem AIRCartageCompany
		{
			get
			{
				return GetItem<GuidRegistryItem>("AIRCartageCompany", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"AIRCartageCompany",
						Categories.Freight_Organizations_DefaultPortTransportCompany,
						ResString.GetMultilingualString("226cbca9-c7c6-4ca3-82bd-af27ce0d36fa", "AIR Port Transport Company"),
						null,
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.ShippingProvider);
					return result;
				});
			}
		}

		#endregion

		#region Default Fumigation Contractor

		public GuidRegistryItem LCLFumigationContractor
		{
			get
			{
				return GetItem<GuidRegistryItem>("LCLFumigationContractor", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"LCLFumigationContractor",
						Categories.Freight_Organizations_DefaultFumigationContractor,
						ResString.GetMultilingualString("b4114dbb-90b3-4e9d-aa67-c07c16ab72d8", "LCL Fumigation Contractor"),
						null,
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.FumigationContractors);
					return result;
				});
			}
		}

		public GuidRegistryItem FCLFumigationContractor
		{
			get
			{
				return GetItem<GuidRegistryItem>("FCLFumigationContractor", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"FCLFumigationContractor",
						Categories.Freight_Organizations_DefaultFumigationContractor,
						ResString.GetMultilingualString("0808e701-433f-493b-b65a-c3c9c1519c48", "FCL Fumigation Contractor"),
						null,
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.FumigationContractors);
					return result;
				});
			}
		}

		public GuidRegistryItem AIRFumigationContractor
		{
			get
			{
				return GetItem<GuidRegistryItem>("AIRFumigationContractor", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"AIRFumigationContractor",
						Categories.Freight_Organizations_DefaultFumigationContractor,
						ResString.GetMultilingualString("13dccddd-c65e-40d6-8197-227ffd5cee58", "AIR Fumigation Contractor"),
						null,
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.FumigationContractors);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region Vessels

		public StringRegistryItem VesselCustomAttribute1Caption
		{
			get
			{
				return GetItem("VesselCustomAttribute1Caption", delegate
				{
					return new StringRegistryItem("VesselCustomAttribute1Caption",
						Categories.Freight_Vessel, ResString.GetMultilingualString("4dbca994-a4b6-4b60-a9b3-adf4539c9b86", "Custom Attribute 1 Caption"), null, RegistryStorageFlags.System);
				});
			}
		}

		public StringRegistryItem VesselCustomAttribute2Caption
		{
			get
			{
				return GetItem("VesselCustomAttribute2Caption", delegate
				{
					return new StringRegistryItem("VesselCustomAttribute2Caption",
						Categories.Freight_Vessel, ResString.GetMultilingualString("7377b4c5-864b-41ef-9d62-5283f210d1f3", "Custom Attribute 2 Caption"), null, RegistryStorageFlags.System);
				});
			}
		}

		public StringRegistryItem VesselCustomAttribute3Caption
		{
			get
			{
				return GetItem("VesselCustomAttribute3Caption", delegate
				{
					return new StringRegistryItem("VesselCustomAttribute3Caption",
						Categories.Freight_Vessel, ResString.GetMultilingualString("89ffdacb-b179-43db-8a41-a263aab6abf5", "Custom Attribute 3 Caption"), null, RegistryStorageFlags.System);
				});
			}
		}

		public StringRegistryItem VesselCustomFlag1Caption
		{
			get
			{
				return GetItem("VesselCustomFlag1Caption", delegate
				{
					return new StringRegistryItem("VesselCustomFlag1Caption",
						Categories.Freight_Vessel, ResString.GetMultilingualString("103ef181-8245-4edb-ac27-6437b4996955", "Custom Flag 1 Caption"), null, RegistryStorageFlags.System);
				});
			}
		}

		public StringRegistryItem VesselCustomDecimal1Caption
		{
			get
			{
				return GetItem("VesselCustomDecimal1Caption", delegate
				{
					return new StringRegistryItem("VesselCustomDecimal1Caption",
						Categories.Freight_Vessel, ResString.GetMultilingualString("331ed2ac-1e81-404a-9af0-f593aef314f6", "Custom Decimal 1 Caption"), null, RegistryStorageFlags.System);
				});
			}
		}

		public BooleanRegistryItem VesselInReferenceFileMandatoryOnShipments
		{
			get
			{
				return GetItem("VesselInReferenceFileMandatoryOnShipments", delegate
				{
					return new BooleanRegistryItem("VesselInReferenceFileMandatoryOnShipments",
						Categories.Freight_Vessel,
						ResString.GetMultilingualString("4E4CEAF9-D754-4D8A-A823-8DE729F23515", "Reference File attached to Vessel Mandatory"),
						ResString.GetMultilingualString("044DE1FA-D18A-40E2-965D-09C2369DBA54", @"Set to ""Yes"" to change the warning to an error when users either input a Vessel that is not attached to a Reference File or an Inactive Vessel on Operational Jobs."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region ALPO Integration

		public StringRegistryItem ALPOExportClientNo
		{
			get
			{
				return GetItem<StringRegistryItem>("ALPOExportClientNo", delegate
				{
					return new StringRegistryItem(
						"ALPOExportClientNo",
						Categories.Freight_ALPO_Export,
						ResString.GetMultilingualString("6d7078f3-de2a-4d32-879e-1100b5900396", "Client Identification in ALPO (ZKV)"),
						ResString.GetMultilingualString("e9d133fb-186f-4d51-b252-95e7b670f3c7", "Specify the Client Identification for your organization. This will be the ZKV in the ALPO Application."),
						RegistryStorageFlags.Company);
				});
			}
		}

		public StringRegistryItem ALPOExportFileName
		{
			get
			{
				return GetItem<StringRegistryItem>("ALPOExportFileName", delegate
				{
					return new StringRegistryItem(
						"ALPOExportFileName",
						Categories.Freight_ALPO_Export,
						ResString.GetMultilingualString("ca7b34e1-92d2-4e70-95d9-8b2c047998cb", "Default Filename for ALPO Export (ZKV)"),
						ResString.GetMultilingualString("4057b278-704b-401a-9698-e2313f4bfeb1", "Specify the Default Filename for your organization without extension. A time stamp will automatically be added when the export is performed."),
						RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region AMS

		public BooleanRegistryItem UseFIRMSCodeAsFilerForPTTMessages
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UseFIRMSCodeAsFilerForPTTMessages", () => new BooleanRegistryItem(
					"UseFIRMSCodeAsFilerForPTTMessages",
					Categories.Freight_AMS,
					ResString.GetMultilingualString("AD0BDA06-CEB2-4122-B08D-A680EDA5F869", "Use FIRMS code as Filer for PTT Messages (NOVCC)"),
					ResString.GetMultilingualString("0BF5D037-A82A-46BC-9B4F-2D8BEE6034A6", "If 'Yes', Stand Alone PTT(NVOCC) messages will use the FIRMS code on the Branch or Company Organization Proxy as the AMS Filer, in lieu of a SCAC code."),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					false
					));
			}
		}

		public ManifestGroupNotificationRegistryItem USAMSGroupNotification
		{
			get
			{
				return GetItem<ManifestGroupNotificationRegistryItem>("USAMSGroupNotification", delegate
				{
					return new ManifestGroupNotificationRegistryItem(
						"USAMSGroupNotification",
						Categories.Freight_AMS,
						ResString.GetMultilingualString("87c07f4c-cb59-44e1-a948-1cd78b56c80d", "Notification Group"),
						ResString.GetMultilingualString("F7F4713B-F1EB-45AC-AFDE-F6B9520A620B", "Group to receive AMS Messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email."),
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						ManifestGroupNotification.Default);
				});
			}
		}

		public ManifestGroupNotificationRegistryItem USHVLVAMSGroupNotification
		{
			get
			{
				return GetItem<ManifestGroupNotificationRegistryItem>("USHVLVAMSGroupNotification", delegate
				{
					return new ManifestGroupNotificationRegistryItem(
						"USHVLVAMSGroupNotification",
						Categories.Freight_AMS,
						ResString.GetMultilingualString("bbd4e47a-0e04-445a-95eb-d0643bbda697", "HVLV Notifications"),
						ResString.GetMultilingualString("f5daec29-42d1-4298-bbf6-326ef5456978", "Group to Receive AMS messages sent by Customs for Bills that originate from HVL shipments. If 'Send Error Only' ticked, Only when an error occurs then send the notification email."),
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new ManifestGroupNotification(Constants.EmailTo.StaffMember, Guid.Empty, true));
				});
			}
		}

		#endregion

		#region Attached Job Limits

		public IntRegistryItem ShipmentsPerConsolLimit
		{
			get
			{
				return GetItem("ShipmentsPerConsolLimit", () =>
					new IntRegistryItem(
					"ShipmentsPerConsolLimit",
					Categories.Freight,
					(NoResString)"Maximum number of Shipments per Consol",
					(NoResString)"This registry determines the maximum number of Shipments that may be attached to a Consol.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					1000));
			}
		}

		public DateTimeRegistryItem ShipmentsPerConsolLimitIntroductionTimeUTC
		{
			get
			{
				return GetItem("ShipmentsPerConsolLimitIntroductionTimeUTC", delegate
				{
					return new DateTimeRegistryItem(
						"ShipmentsPerConsolLimitIntroductionTimeUTC",
						Categories.Freight,
						(NoResString)"Maximum number of Shipments per Consol Active Time",
						(NoResString)"Time (in UTC) of introduction of the 'Maximum number of Shipments per Consol' registry.",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden);
				});
			}
		}

		public IntRegistryItem OrdersPerShipmentLimit
		{
			get
			{
				return GetItem("OrdersPerShipmentLimit", () =>
					new IntRegistryItem(
					"OrdersPerShipmentLimit",
					Categories.Freight,
					(NoResString)"Maximum number of Orders per Shipment",
					(NoResString)"This registry determines the maximum number of Orders that may be attached to a Shipment.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					100));
			}
		}

		public DateTimeRegistryItem OrdersPerShipmentLimitIntroductionTimeUTC
		{
			get
			{
				return GetItem("OrdersPerShipmentLimitIntroductionTimeUTC", delegate
				{
					return new DateTimeRegistryItem(
						"OrdersPerShipmentLimitIntroductionTimeUTC",
						Categories.Freight,
						(NoResString)"Maximum number of Orders per Shipment Active Time",
						(NoResString)"Time (in UTC) of introduction of the 'Maximum number of Orders per Shipment' registry.",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden);
				});
			}
		}

		#endregion

		#region Electronic Signature on MAWB, HAWB and HBL documents

		public BooleanRegistryItem PrintSignatureForHAWBDocuments
		{
			get
			{
				return GetItem<BooleanRegistryItem>("PrintSignatureForHAWBDocuments", () => new BooleanRegistryItem(
					"PrintSignatureForHAWBDocuments",
					Categories.Freight_AWB_HAWB,
					ResString.GetMultilingualString("7d01d103-8175-487f-8bb6-71505f99f12b", "Print Signature"),
					ResString.GetMultilingualString("d7233028-4441-4c1c-a034-ea7fc5125fc0", "Use this Registry to allow staff signature to be printed on HAWB documents where a dedicated signature box is provided."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					false
					));
			}
		}

		public BooleanRegistryItem PrintSignatureForMAWBDocuments
		{
			get
			{
				return GetItem<BooleanRegistryItem>("PrintSignatureForMAWBDocuments", () => new BooleanRegistryItem(
					"PrintSignatureForMAWBDocuments",
					Categories.Freight_AWB_MAWB,
					ResString.GetMultilingualString("aa26d16d-1fcb-4fce-8964-6f95c8a5c360", "Print Signature"),
					ResString.GetMultilingualString("69e2fb54-2cc6-46f9-a0e5-f3d047e5fdb2", "Use this Registry to allow staff signature to be printed on MAWB document. Warning!  Not all airlines may support this approach, check with all airlines concerned prior to enabling this Registry."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					false
					));
			}
		}

		public BooleanRegistryItem PrintSignatureForHBLDocuments
		{
			get
			{
				return GetItem<BooleanRegistryItem>("PrintSignatureForHBLDocuments", () => new BooleanRegistryItem(
					"PrintSignatureForHBLDocuments",
					Categories.Freight_HouseBills,
					ResString.GetMultilingualString("c0247093-5938-4004-a1b0-0f22a7a19f1b", "Print Signature"),
					ResString.GetMultilingualString("b9ba40eb-927c-4e1a-9868-52d010e0b699", "Use this Registry to allow staff signature to be printed on House Bill documents where a dedicated signature box is provided."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					false
					));
			}
		}

		#endregion

		#region HBL Delivery Mode

		public HBLDeliveryModeRegistryItem HBLDeliveryMode_FCL
		{
			get
			{
				return GetItem<HBLDeliveryModeRegistryItem>("HBLDeliveryModeFCL", delegate
				{
					var defaultList = new HBLDeliveryModes(Constants.ContainerModes.FCL, new HBLDeliveryModeLists(RegistryFactory.Instance).GetDefaultHBLDeliveryModeList(Constants.ContainerModes.FCL));

					return new HBLDeliveryModeRegistryItem(
						"HBLDeliveryModeFCL",
						Categories.Freight_Shipment_HBLDeliveryMode,
						ResString.GetMultilingualString("028b5dd6-3a6e-48b7-aef7-144b6d16ab9e", "HBL Delivery Mode for FCL"),
						ResString.GetMultilingualString("fea2d70b-ae06-420a-a582-3d1503b8adf0", "Override this registry to choose the default HBL Delivery Mode value for [FCL] Bookings and Shipments. If services are not applicable to you, you can hide them from being selected on a Booking or Shipment by removing \"Show in List\" flag."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultList);
				});
			}
		}

		public HBLDeliveryModeRegistryItem HBLDeliveryMode_LCL
		{
			get
			{
				return GetItem<HBLDeliveryModeRegistryItem>("HBLDeliveryModeLCL", delegate
				{
					var defaultList = new HBLDeliveryModes(Constants.ContainerModes.LCL, new HBLDeliveryModeLists(RegistryFactory.Instance).GetDefaultHBLDeliveryModeList(Constants.ContainerModes.LCL));

					return new HBLDeliveryModeRegistryItem(
						"HBLDeliveryModeLCL",
						Categories.Freight_Shipment_HBLDeliveryMode,
						ResString.GetMultilingualString("c4b779c2-c251-4517-bc67-9bc9cbf65696", "HBL Delivery Mode for LCL"),
						ResString.GetMultilingualString("8796f61d-2469-43b3-9f7e-ec8e576968d6", "Override this registry to choose the default HBL Delivery Mode value for [LCL] Bookings and Shipments. If services are not applicable to you, you can hide them from being selected on a Booking or Shipment by removing \"Show in List\" flag."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultList);
				});
			}
		}

		public HBLDeliveryModeRegistryItem HBLDeliveryMode_BBK_ROR_BLK_LQD
		{
			get
			{
				return GetItem<HBLDeliveryModeRegistryItem>("HBLDeliveryModeBBKRORBLKLQD", delegate
				{
					var defaultList = new HBLDeliveryModes(Constants.ContainerModes.Bulk, new HBLDeliveryModeLists(RegistryFactory.Instance).GetDefaultHBLDeliveryModeList(Constants.ContainerModes.Bulk));

					return new HBLDeliveryModeRegistryItem(
						"HBLDeliveryModeBBKRORBLKLQD",
						Categories.Freight_Shipment_HBLDeliveryMode,
						ResString.GetMultilingualString("c1409142-514f-4554-af1d-07db7c5fb776", "HBL Delivery Mode for Break Bulk, Roll On/Roll Off, Bulk, Liquid"),
						ResString.GetMultilingualString("132b5e10-de6f-4267-a574-1d70ad34d952", "Override this registry to choose the default HBL Delivery Mode value for [Break Bulk, Roll On/Roll Off, Bulk, Liquid] Shipment. If services are not applicable to you, you can hide them from being selected on a Shipment by removing \"Show in List\" flag."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultList);
				});
			}
		}

		public HBLDeliveryModeRegistryItem HBLDeliveryMode_BCN
		{
			get
			{
				return GetItem<HBLDeliveryModeRegistryItem>("HBLDeliveryModeBCN", delegate
				{
					var defaultList = new HBLDeliveryModes(Constants.ContainerModes.BuyersConsol, new HBLDeliveryModeLists(RegistryFactory.Instance).GetDefaultHBLDeliveryModeList(Constants.ContainerModes.BuyersConsol));

					return new HBLDeliveryModeRegistryItem(
						"HBLDeliveryModeBCN",
						Categories.Freight_Shipment_HBLDeliveryMode,
						ResString.GetMultilingualString("581b597e-89b8-4318-ba7f-df2db873c59a", "HBL Delivery Mode for BCN"),
						ResString.GetMultilingualString("10c16b94-1934-4c58-83c6-24d3e6d976a7", "Override this registry to choose the default HBL Delivery Mode value for [BCN] Shipment. If services are not applicable to you, you can hide them from being selected on a Shipment by removing \"Show in List\" flag."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultList);
				});
			}
		}

		public HBLDeliveryModeRegistryItem HBLDeliveryMode_SCN
		{
			get
			{
				return GetItem<HBLDeliveryModeRegistryItem>("HBLDeliveryModeSCN", delegate
				{
					var defaultList = new HBLDeliveryModes(Constants.ContainerModes.ShippersConsol, new HBLDeliveryModeLists(RegistryFactory.Instance).GetDefaultHBLDeliveryModeList(Constants.ContainerModes.ShippersConsol));

					return new HBLDeliveryModeRegistryItem(
						"HBLDeliveryModeSCN",
						Categories.Freight_Shipment_HBLDeliveryMode,
						ResString.GetMultilingualString("4ddf75b9-0ad4-4d94-90f1-4eefbb57ee04", "HBL Delivery Mode for SCN"),
						ResString.GetMultilingualString("6fd0ad01-6cf9-412e-9a06-03e78d845c58", "Override this registry to choose the default HBL Delivery Mode value for [SCN] Shipment. If services are not applicable to you, you can hide them from being selected on a Shipment by removing \"Show in List\" flag."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultList);
				});
			}
		}

		public HBLDeliveryModeRegistryItem HBLDeliveryMode_LSE
		{
			get
			{
				return GetItem<HBLDeliveryModeRegistryItem>("HBLDeliveryModeLSE", delegate
				{
					var defaultList = new HBLDeliveryModes(Constants.ContainerModes.Loose, new HBLDeliveryModeLists(RegistryFactory.Instance).GetDefaultHBLDeliveryModeList(Constants.ContainerModes.Loose));
					return new HBLDeliveryModeRegistryItem(
						"HBLDeliveryModeLSE",
						Categories.Freight_Shipment_HBLDeliveryMode,
						ResString.GetMultilingualString("3B664B31-AD7B-478A-98F6-31C3458CBF7B", "HBL Delivery Mode for LSE"),
						ResString.GetMultilingualString("328D0BCA-7963-4897-8ED4-D360C488E3B6", "Override this registry to choose the default HBL Delivery Mode value for LSE Bookings and Shipments. If services are not applicable to you, you can hide them from being selected on a Booking or Shipment by removing 'Show in List' flag."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultList);
				});
			}
		}

		public HBLDeliveryModeRegistryItem HBLDeliveryMode_ULD
		{
			get
			{
				return GetItem<HBLDeliveryModeRegistryItem>("HBLDeliveryModeULD", delegate
				{
					var defaultList = new HBLDeliveryModes(Constants.ContainerModes.ULD, new HBLDeliveryModeLists(RegistryFactory.Instance).GetDefaultHBLDeliveryModeList(Constants.ContainerModes.ULD));
					return new HBLDeliveryModeRegistryItem(
						"HBLDeliveryModeULD",
						Categories.Freight_Shipment_HBLDeliveryMode,
						ResString.GetMultilingualString("20CFB3EB-DD80-479F-B23C-449D09DD8BBA", "HBL Delivery Mode for ULD"),
						ResString.GetMultilingualString("320E6856-95AC-414A-B055-C89CA78C4E0D", "Override this registry to choose the default HBL Delivery Mode value for ULD Bookings and Shipments. If services are not applicable to you, you can hide them from being selected on a Booking or Shipment by removing 'Show in List' flag."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultList);
				});
			}
		}

		#endregion

		#region Global Tracking

		#region Container Automation

		public BooleanRegistryItem ContainerAutomation
		{
			get
			{
				BooleanRegistryItem GetContainerAutomationRegistryItem()
				{
					return GetItem("ContainerAutomation", delegate
					{
						return new BooleanRegistryItem(
							"ContainerAutomation",
							Categories.Freight_GlobalTracking,
							(NoResString)"Container Automation",
							(NoResString)"Enable Container Automation?",
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							true);
					});
				}

				var item = GetContainerAutomationRegistryItem();
				if (!item.DefaultValue)
				{
					RemoveItemFromCacheIfOlderThan("ContainerAutomation", TimeSpan.Zero);
					item = GetContainerAutomationRegistryItem();
				}

				return item;
			}
		}

		#region Global Tracking Shipment Visibility

		public GlobalTrackingShipmentVisibilityOptionsRegistryItem GlobalTrackingShipmentVisibility
		{
			get
			{
				return GetItem("GlobalTrackingShipmentVisibility", () =>
					new GlobalTrackingShipmentVisibilityOptionsRegistryItem(
					"GlobalTrackingShipmentVisibility",
					Categories.Freight_GlobalTracking,
					ResString.GetMultilingualString("ADF9E11B-354A-4231-A360-0F2401A2EB58", "Shipment Visibility"),
					ResString.GetMultilingualString("ADD9E11B-351A-4231-A360-0F1405A2EB59", "Enable Shipment Visibility?"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					new GlobalTrackingShipmentVisibilityOptions { IsActive = false, IsDefaultContainerAutomation = false, ServiceEhubIDs = DefaultServiceEhubIDs }));
			}
		}

		GlobalTrackingShipmentVisibilityServiceEhubIDCollection DefaultServiceEhubIDs
		{
			get
			{
				if (defaultServiceEhubIDs == null)
				{
					defaultServiceEhubIDs = new GlobalTrackingShipmentVisibilityServiceEhubIDList().GetDefaultGlobalTrackingShipmentVisibilityServiceEhubIDs();
				}

				return defaultServiceEhubIDs;
			}
		}
		GlobalTrackingShipmentVisibilityServiceEhubIDCollection defaultServiceEhubIDs;

		#endregion

		public StringRegistryItem ContainerAutomationEHubID
		{
			get
			{
				return GetItem<StringRegistryItem>("ContainerAutomationEHubID", delegate
				{
					var item = new StringRegistryItem(
						"ContainerAutomationEHubID",
						Categories.Freight_GlobalTracking,
						(NoResString)"Container Automation eHub ID",
						(NoResString)"Container Automation eHub ID",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"CONTAINER_TRACKING");

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
					item.DataType = new EnglishOnlyStringRegistryDataType();

					return item;
				});
			}
		}

		#endregion

		#region AWB Tracking

		public BooleanRegistryItem AWBTracking
		{
			get
			{
				BooleanRegistryItem GetAWBTrackingRegistryItem()
				{
					return GetItem("AWBTracking", delegate
					{
						return new BooleanRegistryItem(
							"AWBTracking",
							Categories.Freight_GlobalTracking,
							(NoResString)"AWB Tracking",
							(NoResString)"Enable AWB Tracking?",
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							true);
					});
				}

				var item = GetAWBTrackingRegistryItem();
				if (!item.DefaultValue)
				{
					RemoveItemFromCacheIfOlderThan("AWBTracking", TimeSpan.Zero);
					item = GetAWBTrackingRegistryItem();
				}

				return item;
			}
		}

		public StringRegistryItem AWBTrackingEHubID
		{
			get
			{
				return GetItem<StringRegistryItem>("AWBTrackingEHubID", delegate
				{
					var item = new StringRegistryItem(
						"AWBTrackingEHubID",
						Categories.Freight_GlobalTracking,
						(NoResString)"AWB Tracking eHub ID",
						(NoResString)"AWB Tracking eHub ID",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"FLIGHT_MONITORING_SYSTEM");

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
					item.DataType = new EnglishOnlyStringRegistryDataType();

					return item;
				});
			}
		}

		#endregion

		#region Global Sailing Schedules

		public BooleanRegistryItem EnableScheduleFeedService
		{
			get
			{
				return GetItem("EnableScheduleFeedService", delegate
				{
					return new BooleanRegistryItem(
						"EnableScheduleFeedService",
						Categories.Freight_GlobalTracking_GlobalSailingSchedules,
						ResString.GetMultilingualString("94530929-3924-41ed-b9b3-bba3df356446", "Enable Global Sailing Schedules"),
						ResString.GetMultilingualString("276dd382-6f4c-43cb-b69a-550c11882ddd", "Enable Global Sailing Schedules?"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public StringRegistryItem OnlineSailingSchedulesUrl
		{
			get
			{
				return GetItem<StringRegistryItem>("OnlineSailingSchedulesUrl", delegate
				{
					return new StringRegistryItem(
						"OnlineSailingSchedulesUrl",
						Categories.Freight_GlobalTracking_GlobalSailingSchedules,
						ResString.GetMultilingualString("e9238f8e-f2c4-4b6a-85b9-87d5dde1f1bc", "Global Sailing Schedules Service URL"),
						ResString.GetMultilingualString("621b2486-ff64-45ae-a9ac-d81155efd341", "The URL used to connect to Global Sailing Schedules"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"https://gss.wisegrid.net");
				});
			}
		}

		public StringRegistryItem OnlineSailingSchedulesTokenOverride
		{
			get
			{
				return GetItem(nameof(OnlineSailingSchedulesTokenOverride), delegate
				{
					return new StringRegistryItem(
						nameof(OnlineSailingSchedulesTokenOverride),
						Categories.Freight_GlobalTracking_GlobalSailingSchedules,
						ResString.GetMultilingualString("7e38dd50-7654-a996-4b24-b21caa02ac7a", "GSS Token Override"),
						ResString.GetMultilingualString("89233651-1865-6899-465e-572b2b8027b2", "An override for the auth. token used to connect to Global Sailing Schedules"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"");
				});
			}
		}

		public StringRegistryItem ScheduleFeedTrackingEHubID
		{
			get
			{
				return GetItem<StringRegistryItem>("ScheduleFeedTrackingEHubID", delegate
				{
					var item = new StringRegistryItem(
						"ScheduleFeedTrackingEHubID",
						Categories.Freight_GlobalTracking_GlobalSailingSchedules,
						ResString.GetMultilingualString("BDD9BB53-4BBE-4D95-969D-ACD348B46D7D", "Schedule Feed Tracking eHub ID"),
						ResString.GetMultilingualString("78269847-303C-4536-8970-226C02C78F66", "Schedule Feed Tracking eHub ID"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"SCHEDULE_FEED_SERVICE");

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
					item.DataType = new EnglishOnlyStringRegistryDataType();

					return item;
				});
			}
		}

		public IntRegistryItem OnlineSailingSchedulesAutoInitiatedTimeoutInSeconds
		{
			get
			{
				return GetItem("OnlineSailingSchedulesAutoInitiatedTimeoutInSeconds", delegate
				{
					return new IntRegistryItem(
						"OnlineSailingSchedulesAutoInitiatedTimeoutInSeconds",
						Categories.Freight_GlobalTracking_GlobalSailingSchedules,
						ResString.GetMultilingualString("a8061e9a-6c85-41a0-ac03-6f659f28efc2", "Auto Requests Timeout"),
						ResString.GetMultilingualString("00d2b731-6ab8-4ec6-aec5-96a57492d2d0", "Timeout is seconds when automatic requests are executed to populate dates"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: 5,
						minValue: 0,
						maxValue: 60);
				});
			}
		}

		public IntRegistryItem OnlineSailingSchedulesUserInitiatedTimeoutInSeconds
		{
			get
			{
				return GetItem("OnlineSailingSchedulesUserInitiatedTimeoutInSeconds", delegate
				{
					return new IntRegistryItem(
						"OnlineSailingSchedulesUserInitiatedTimeoutInSeconds",
						Categories.Freight_GlobalTracking_GlobalSailingSchedules,
						ResString.GetMultilingualString("f7ac027f-11b6-4407-ae6d-a16ab13ab0f1", "User Initiated Requests Timeout"),
						ResString.GetMultilingualString("8b0bd163-73c4-4152-b815-641d32114899", "Timeout is seconds when user initiated search requests are executed"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: 20,
						minValue: 0,
						maxValue: 60);
				});
			}
		}

		#endregion // Global Sailing Schedules

		#region eBookings

		public EBookingApiUrlRegistryItem EBookingServicesApiUrl
		{
			get
			{
				return GetItem<EBookingApiUrlRegistryItem>("EBookingServicesApiUrl", delegate
				{
					return new EBookingApiUrlRegistryItem("EBookingServicesApiUrl",
						Categories.Freight_EBookings,
						ResString.GetMultilingualString("da2fdfe4-a624-40c5-a0b4-59e0a0c7f70e", "eBooking API Service"),
						ResString.GetMultilingualString("a6c66781-5b54-4732-828d-82d0a1c86fa8", "The URL used to connect to eBooking API"),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new EBookingApiUrls(EBookingApiUrls.Constants.ProdCode));
				});
			}
		}

		public BooleanRegistryItem EnableAirlineConnectMenu =>
			GetItem("EnableAirlineConnectMenu", () =>
				new BooleanRegistryItem(
					"EnableAirlineConnectMenu",
					Categories.Freight_EBookings,
					(NoResString)"Show AirlineConnect Menu Item",
					(NoResString)"When this registry is enabled, the AirlineConnect menu item will be shown from Actions menu of consol form.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					true));

		public StringRegistryItem CustomEBookingApiServiceUrl
		{
			get
			{
				return GetItem("CustomEBookingApiServiceUrl", delegate
				{
					return new StringRegistryItem(
						"CustomEBookingApiServiceUrl",
						Categories.Freight_EBookings,
						ResString.GetMultilingualString("c241bf61-07cf-44be-85ea-dcaa9c2e2aa7", "eBooking CUSTOM API Service URL"),
						ResString.GetMultilingualString("7955f680-bda9-4fcc-a096-11b7fe5cfd83", "The URL used to connect to a customized eBooking API Service URL. If you override default value, your customized URL will be used instead of eBooking API service URL"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						string.Empty);
				});
			}
		}

		public IntRegistryItem EBookingApiTimeoutInSeconds
		{
			get
			{
				return GetItem("EBookingApiTimeoutInSeconds", delegate
				{
					return new IntRegistryItem(
						"EBookingApiTimeoutInSeconds",
						Categories.Freight_EBookings,
						ResString.GetMultilingualString("2c7a9009-0ab2-4e6f-abe4-d01dfcb8f956", "eBooking API Requests Timeout"),
						ResString.GetMultilingualString("6a8d240c-c360-4215-9696-abae4f516dd9", "Timeout is seconds when eBooking requests are executed"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 120,
						minValue: 1,
						maxValue: 3600);
				});
			}
		}

		public BooleanRegistryItem ShowDetailedRequestFailureForEBookings
		{
			get
			{
				return GetItem("ShowDetailedRequestFailureForEBookings", delegate
				{
					return new BooleanRegistryItem(
						"ShowDetailedRequestFailureForEBookings",
						Categories.Freight_EBookings,
						ResString.GetMultilingualString("95341b7a-5538-4387-a03e-cda142dd52fb", "Show Detailed Request Failure For eBookings"),
						ResString.GetMultilingualString("4d39fca2-75a6-4623-a809-a1b4cb348b18", "When set to true it will display detailed request failure"),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public BooleanRegistryItem IncludeAuthorizationWhenSendingEBookings
		{
			get
			{
				return GetItem("IncludeAuthorizationWhenSendingEBookings", delegate
				{
					return new BooleanRegistryItem(
						"IncludeAuthorizationWhenSendingEBookings",
						Categories.Freight_EBookings,
						(NoResString)"Include Authorization When Sending eBookings",
						(NoResString)"When set to true the request will contain authorization details",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public BooleanRegistryItem EnableAirBookingCarrierConfiguration
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableAirBookingCarrierConfiguration", delegate
				{
					return new BooleanRegistryItem(
						"EnableAirBookingCarrierConfiguration",
						Categories.Freight_EBookings,
						(NoResString)"Enable eBooking Carrier Configuration",
						(NoResString)"By default the eBooking Carrier Configuation is enabled. Change this setting to Yes to enable eBooking Carrier Configuration.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public IntRegistryItem EBookingCarrierConfigurationApiTimeoutInSeconds
		{
			get
			{
				return GetItem("EBookingCarrierConfigurationApiTimeoutInSeconds", delegate
				{
					return new IntRegistryItem(
						"EBookingCarrierConfigurationApiTimeoutInSeconds",
						Categories.Freight_EBookings,
						ResString.GetMultilingualString("aab0ba43-db54-4376-b989-d81b86ca448f", "eBooking Carrier Configuration Request Timeout"),
						ResString.GetMultilingualString("042ac8ab-6f58-47aa-84d4-108c5844ccd2", "Timeout is seconds when eBooking requests are executed"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 5,
						minValue: 1,
						maxValue: 300);
				});
			}
		}

		public EBookingAirCarrierConfigurationRegistryItem EBookingCarrierConfiguration
		{
			get
			{
				return GetItem<EBookingAirCarrierConfigurationRegistryItem>("EBookingCarrierConfiguration", delegate
				{
					return new EBookingAirCarrierConfigurationRegistryItem(
						"EBookingCarrierConfiguration",
						Categories.Freight_EBookings,
						(NoResString)"eBooking Carrier Configuration",
						(NoResString)"The last eBooking Carrier Configuration received from eBooking API",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						new EBookingCarrierConfiguration()
					);
				});
			}
		}

		public PacklineWeightDistributionRegistryItem PacklineWeightDistribution
		{
			get
			{
				return GetItem("PacklineWeightDistribution", delegate
				{
					return new PacklineWeightDistributionRegistryItem(
						"PacklineWeightDistribution",
						Categories.Freight_EBookings,
						ResString.GetMultilingualString("3B73D8BF-C0DD-4262-ACA2-27A178373743", "Packline Weight Distribution"),
						ResString.GetMultilingualString("0EC412D3-0F6C-4E77-BDEF-A5A277FE9B4E", "By default the Packline Weight Distribution is disabled. Change this setting to Yes to enable the Actual Weight Distribution with the option to also enable the Volumetric Weight Distribution as well."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new PacklineWeightDistributionConfiguration());
				});
			}
		}

		#endregion

		#region Automatic Container Creation

		public AutomaticContainerCreationRegistryItem AutomaticContainerCreation =>
			GetItem("DisableAutomaticContainerCreation", delegate
			{
				return new AutomaticContainerCreationRegistryItem(
					"DisableAutomaticContainerCreation",
					Categories.Freight_GlobalTracking,
					ResString.GetMultilingualString("e2f372ae-694e-46d0-b163-e85451024d8d", "Automatic Container Creation"),
					ResString.GetMultilingualString("1d7686f3-d952-4144-8168-96de5e740c46",
					@"This registry setting controls the behavior of the creation of Containers on a Consolidation via Container Automation.

Always Create: This option allows Container Automation events to automatically populate container numbers onto Consolidation when the messages received detect a new Container number which does not already exist on the Consolidation.
Note - A CID Event (Change of Identifier) will be created when a container number is populated.

Create up to ATD or Shipping Instruction: This is the default option for this functionality. This option allows Container Automation events to automatically populate container numbers onto Consolidation, but only up to the point that the Consolidation either receives an ATD, or the Shipping Instruction message is submitted electronically. After that point, Containers will no longer auto-populate on the Consolidation.

Never Create: This option will disable any automatic Container creation from messages received from Container Automation."));
			});

		#endregion

		#region Last Foreign and First Arrival Port Matcher

		public BooleanRegistryItem EnableLastForeignAndFirstArrivalPortMatcher =>
			GetItem("EnableLastForeignAndFirstArrivalPortMatcher", () =>
				new BooleanRegistryItem(
					"EnableLastForeignAndFirstArrivalPortMatcher",
					Categories.Freight_GlobalTracking,
					(NoResString)"Enable Last Foreign and First Arrival Port Matcher",
					(NoResString)"When this registry is enabled, Last Foreign and First Arrival Ports will be automatically populated.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					true));

		#endregion

		#region Route Visualizer
		public BooleanRegistryItem EnableRouteVisualizer =>
			GetItem("EnableRouteVisualizer", () =>
				new BooleanRegistryItem(
					"EnableRouteVisualizer",
					Categories.Freight_GlobalTracking_RouteVisualizer,
					(NoResString)"Enable Route Visualizer",
					(NoResString)"When this registry is enabled, the Route Visualizer Map button will be shown on to all users on the Routing tab. Otherwise, the button is only visible to CW1 support user.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					true));

		public StringRegistryItem RouteVisualizerUrl =>
			GetItem("RouteVisualizerUrl", () =>
				new StringRegistryItem(
					"RouteVisualizerUrl",
					Categories.Freight_GlobalTracking_RouteVisualizer,
					(NoResString)"Route Visualizer URL",
					(NoResString)"Determines the base server URL used for the Route Visualizer Map.",
					new StringRegistryDataType(CharacterCase.Normal, 8, 255),           // Min of 8 to fit "http://a"
					new TextRegistryEditorInfo(TextEditorType.Url),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					"https://vt.wisegrid.net/"));

		public StringRegistryItem RouteVisualizerApiUrl =>
			GetItem("RouteVisualizerApiUrl", () =>
				new StringRegistryItem(
					"RouteVisualizerApiUrl",
					Categories.Freight_GlobalTracking_RouteVisualizer,
					(NoResString)"Route Visualizer API URL",
					(NoResString)"Determines the base server URL used for the Route Visualizer API.",
					new StringRegistryDataType(CharacterCase.Normal, 8, 255), // Min of 8 to fit "http://a"
					new TextRegistryEditorInfo(TextEditorType.Url),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					"https://api-vt.wisegrid.net/"));

		public BooleanRegistryItem EnableRouteVisualizerMyAccountLogin =>
			GetItem("EnableRouteVisualizerMyAccountLogin", () =>
				new BooleanRegistryItem(
					"EnableRouteVisualizerMyAccountLogin",
					Categories.Freight_GlobalTracking_RouteVisualizer,
					(NoResString)"Enable Route Visualizer MyAccount integration",
					(NoResString)"When this registry is enabled, the Route Visualizer will use MyAccount service to handle authentication. Otherwise Route Visualizer will use the existing authentication mechanism.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false));
		#endregion

		#region CargoTracker

		public BooleanRegistryItem EnableCargoTracker =>
			GetItem("EnableCargoTracker", () =>
				new BooleanRegistryItem(
					"EnableCargoTracker",
					Categories.Freight_GlobalTracking_CargoTracker,
					ResString.GetMultilingualString("DE9052A9-08E0-4CEF-8FCA-A980B457065E", "Enable Cargo Tracker"),
					ResString.GetMultilingualString("C88D7C71-1BE6-4C50-9898-8257217656C7", "When this registry is enabled, the View Consignment in Cargo Tracker menu item will be shown in Consol form Actions menu to all users."),
					RegistryStorageFlags.System,
					true));

		public StringRegistryItem CargoTrackerUrl =>
			GetItem("CargoTrackerUrl", () =>
				new StringRegistryItem(
					"CargoTrackerUrl",
					Categories.Freight_GlobalTracking_CargoTracker,
					(NoResString)"Cargo Tracker URL",
					(NoResString)"Determines the base server URL used for Cargo Tracker.",
					new StringRegistryDataType(CharacterCase.Normal, 8, 255),           // Min of 8 to fit "http://a"
					new TextRegistryEditorInfo(TextEditorType.Url),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					"https://vt.wisegrid.net/"));

		public StringRegistryItem CargoTrackerApiAudienceId =>
			GetItem("CargoTrackerApiAudienceId", () =>
				new StringRegistryItem(
					"CargoTrackerApiAudienceId",
					Categories.Freight_GlobalTracking_CargoTracker,
					(NoResString)"Cargo Tracker API Audience ID",
					(NoResString)"Determines the Audience ID used to generate the authentication token required for accessing Cargo Tracker API.",
					new StringRegistryDataType(),
					new TextRegistryEditorInfo(TextEditorType.Guid),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					"8c619652-9df1-43e9-a14c-6abb26ec363e"));

		public StringRegistryItem AisApiAudienceId =>
			GetItem("AisApiAudienceId", () =>
				new StringRegistryItem(
					"AisApiAudienceId",
					Categories.Freight_GlobalTracking_CargoTracker,
					(NoResString)"AIS API Audience ID",
					(NoResString)"Determines the Audience ID used to generate the authentication token required for accessing AIS API.",
					new StringRegistryDataType(),
					new TextRegistryEditorInfo(TextEditorType.Guid),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					"659d969a-dee4-4756-8a8d-69e6b84e2c8c"));

		#endregion

		#region Performance Reporting

		public StringRegistryItem ReportingUrl =>
			GetItem("ReportingUrl", () =>
				new StringRegistryItem(
					"ReportingUrl",
					Categories.Freight_GlobalTracking_MarketIntelligenceAndAnalytics,
					(NoResString)"Reporting URL",
					(NoResString)"Determines the base server URL used for the Performance Reports.",
					new StringRegistryDataType(CharacterCase.Normal, 8, 255), // Min of 8 to fit "http://a"
					new TextRegistryEditorInfo(TextEditorType.Url),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					"https://cpr.wisegrid.net"));

		#endregion

		#endregion

		#region EnableBookingConfirmation

		public BooleanRegistryItem EnableBookingConfirmation
		{
			get => GetItem<BooleanRegistryItem>("EnableBookingConfirmation", () =>
				new BooleanRegistryItem(
					"EnableBookingConfirmation",
					Categories.Freight_Consolidations_OceanCarrierMessaging,
					(NoResString)"Enable Booking Confirmation form",
					(NoResString)"If yes, enables new Booking Confirmation form for testing. It will also send FormVersion 2 in Booking Request/Shipping Instruction message.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
					true));
		}

		#endregion

		#region EnableDraftBillOfLadingForm

		public BooleanRegistryItem EnableDraftBillOfLadingForm
		{
			get => GetItem<BooleanRegistryItem>("EnableDraftBillOfLadingForm", () =>
				new BooleanRegistryItem(
					"EnableDraftBillOfLadingForm",
					Categories.Freight_Consolidations_OceanCarrierMessaging,
					(NoResString)"Enable Draft Bill Of Lading form",
					(NoResString)"If yes, enable new Draft Bill Of Lading form for testing.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
					false));
		}

		#endregion

		#region EnableBoleroEBLIntegration

		public BoleroEBLConfigurationRegistryItem EnableBoleroEBLIntegration
		{
			get
			{
				var isBoleroIntegrationFeatureEnabled = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.IntegrationBoleroElectronicMasterBLIntegration) != null;

				return GetItem("EnableBoleroEBLIntegration", () =>
					new BoleroEBLConfigurationRegistryItem(
						"EnableBoleroEBLIntegration",
						Categories.Freight_Consolidations,
						ResString.GetMultilingualString("bcbb8ecc-4f9c-4fc4-9c70-cdf32dd3b265", "Enable Bolero eBL Integration"),
						ResString.GetMultilingualString("c87ce2d8-7a0d-4c04-b698-085e75ecee21", "If yes, enable Bolero eBL Integration for testing."),
						RegistryStorageFlags.System,
						isBoleroIntegrationFeatureEnabled ? RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers : RegistryOptions.IsHidden,
						new BoleroEBLConfiguration { EnableEBLIntegration = isBoleroIntegrationFeatureEnabled })
				);
			}
		}

		#endregion

		#region EnableBoleroEHBLIntegration

		public BoleroEBLConfigurationRegistryItem EnableBoleroEHBLIntegration
		{
			get => GetItem<BoleroEBLConfigurationRegistryItem>("EnableBoleroEHBLIntegration", () =>
				new BoleroEBLConfigurationRegistryItem(
					"EnableBoleroEHBLIntegration",
					Categories.Freight_Shipment,
					(NoResString)"Enable Bolero eHBL Integration",
					(NoResString)"If yes, enable Bolero eHBL Integration for testing.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
					new BoleroEBLConfiguration())
				);
		}

		#endregion

		#region CO2eUserRequestProcessingMethod

		public CodePairRegistryItem CO2eUserRequestProcessingMethod
		{
			get
			{
				return GetItem("CO2eUserRequestProcessingMethod", delegate
				{
					return new CodePairRegistryItem(
						"CO2eUserRequestProcessingMethod",
						Categories.Freight_GreenhouseGasEmission,
						ResString.GetMultilingualString("2B285391-F1D0-444A-BC52-3B4C39B5086A", "User request processing method"),
						ResString.GetMultilingualString("7F25C74A-D64D-4F46-8FC1-870AFB665E1D", "Select a method how the CO2 calculation requests are processed."),
						new CodeDescriptionPairListProvider(() => new CO2eUserRequestProcessingMethodCodeList()),
						RegistryStorageFlags.System,
						ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled ? RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers : RegistryOptions.IsHidden,
						CO2eUserRequestProcessingMethodCodeList.Codes.Api);
				});
			}
		}

		#endregion

		#region CO2eApiUrl

		public StringRegistryItem CO2eApiUrl
		{
			get
			{
				return GetItem("CO2eApiUrl", delegate
				{
					return new StringRegistryItem(
						"CO2eApiUrl",
						Categories.Freight_GreenhouseGasEmission,
						ResString.GetMultilingualString("B0581B54-B539-422C-88DF-0869F6D6E32C", "CO2e API Service URL"),
						ResString.GetMultilingualString("5D24890A-EE82-4308-9757-EBCB8F64F44E", "The URL used to connect to CO2e API."),
						RegistryStorageFlags.System,
						ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled ? RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers : RegistryOptions.IsHidden,
						"https://ec.wisegrid.net/");
				});
			}
		}

		#endregion

		#region CO2eApiRequestsTimeout

		public IntRegistryItem CO2eApiRequestsTimeout
		{
			get
			{
				return GetItem("CO2eApiRequestsTimeout", delegate
				{
					return new IntRegistryItem(
						"CO2eApiRequestsTimeout",
						Categories.Freight_GreenhouseGasEmission,
						ResString.GetMultilingualString("CFB848F3-B510-41E4-A192-7A10A9D899C3", "CO2e API Requests Timeout"),
						ResString.GetMultilingualString("B1888BE6-2E39-454A-B2D7-BF1A4C460BB8", "Timeout in seconds when greenhouse gas emissions requests are executed"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled ? RegistryOptions.Default : RegistryOptions.IsHidden,
						defaultValue: 15,
						minValue: 1,
						maxValue: 3600);
				});
			}
		}

		#endregion

		#region EnableGlobalFlightSchedulesCalculation

		public BooleanRegistryItem EnableGlobalFlightSchedulesCalculation
		{
			get
			{
				return GetItem("EnableGlobalFlightSchedulesCalculation", delegate
				{
					return new BooleanRegistryItem(
						"EnableGlobalFlightSchedulesCalculation",
						Categories.Freight_GreenhouseGasEmission,
						ResString.GetMultilingualString("7978AC97-45A8-4450-99B1-1188135C8AF4", "Enable Global Flight Schedules Calculation"),
						ResString.GetMultilingualString("9EDA84EA-BCD7-4586-A5C3-4B10875ABA5B", "Enables GHG calculation for Global Flight Schedule"),
						RegistryStorageFlags.System,
						ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled ? RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers : RegistryOptions.IsHidden,
						true);
				});
			}
		}

		#endregion

		#region EnableGlobalSailingSchedulesCalculation

		public BooleanRegistryItem EnableGlobalSailingSchedulesCalculation
		{
			get
			{
				return GetItem("EnableGlobalSailingSchedulesCalculation", delegate
				{
					return new BooleanRegistryItem(
						"EnableGlobalSailingSchedulesCalculation",
						Categories.Freight_GreenhouseGasEmission,
						ResString.GetMultilingualString("A8F9AC59-4FA3-4480-AD66-343938C89F50", "Enable Global Sailing Schedules Calculation"),
						ResString.GetMultilingualString("21D9C562-FE65-4C13-8501-642AC4A4E5A5", "Enables GHG calculation for Global Sailing Schedule"),
						RegistryStorageFlags.System,
						ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled ? RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers : RegistryOptions.IsHidden,
						false);
				});
			}
		}

		#endregion

		#region EnablePrePostCarriageCalculation

		public BooleanRegistryItem EnablePrePostCarriageCalculation
		{
			get
			{
				return GetItem("EnablePrePostCarriageCalculation", delegate
				{
					return new BooleanRegistryItem(
						"EnablePrePostCarriageCalculation",
						Categories.Freight_GreenhouseGasEmission,
						ResString.GetMultilingualString("B6D0FCA2-2FF7-425C-A4DE-9EA23D12B616", "Enables Pre/On Carriage Calculation"),
						ResString.GetMultilingualString("15169BA2-B256-41F1-8E88-2301E0A860CC", "When enabled, the GHG calculation request via Action menu or Workflow trigger will include pre/on carriage legs"),
						RegistryStorageFlags.System,
						ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled ? RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers : RegistryOptions.IsHidden,
						true);
				});
			}
		}

		#endregion

		#region UseHBLDeliveryModeForGHGCalculation

		public BooleanRegistryItem UseHBLDeliveryModeForGHGCalculation
		{
			get
			{
				return GetItem("UseHBLDeliveryModeForGHGCalculation", delegate
				{
					return new BooleanRegistryItem(
						"UseHBLDeliveryModeForGHGCalculation",
						Categories.Freight_GreenhouseGasEmission,
						ResString.GetMultilingualString("D3111DAB-254A-4C84-A63A-A18ECB75F172", "Use HBL Delivery Mode"),
						ResString.GetMultilingualString("4DC15B95-F523-492D-8F6C-93C8B3558977", "If yes, system uses HBL Delivery Mode (when applicable) to determine the Pre-Carriage and On-Carriage legs included in the total CO2e emissions."),
						RegistryStorageFlags.System,
						ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled ? RegistryOptions.Default : RegistryOptions.IsHidden,
						true);
				});
			}
		}

		#endregion

		#region AddEmissionsCalculationLogForShipment

		public BooleanRegistryItem AddEmissionsCalculationLogForShipment
		{
			get
			{
				return GetItem("AddEmissionsCalculationLogForShipment", delegate
				{
					return new BooleanRegistryItem(
						"AddEmissionsCalculationLogForShipment",
						Categories.Freight_GreenhouseGasEmission,
						(NoResString)"Add Emissions Calculation Log For Shipment",
						(NoResString)"If yes, an Emissions Calculation Log will be added to the Shipment > Notes tab when the GHG calculation succeeds.",
						RegistryStorageFlags.System,
						ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled ? RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers : RegistryOptions.IsHidden,
						false);
				});
			}
		}

		#endregion

		#region EnableSailingGenerationOnServiceTaskSaving

		public BooleanRegistryItem EnableSailingGenerationOnServiceTaskSaving
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableSailingGenerationOnServiceTaskSaving", delegate
				{
					return new BooleanRegistryItem(
						"EnableSailingGenerationOnServiceTaskSaving",
						Categories.Freight,
						(NoResString)"Enable Sailing Generation On Service Task Saving",
						(NoResString)"If set to 'Yes', sailing will be generated on service task saving.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
						true);
				});
			}
		}

		#endregion

		#region EnableMexicanPortIntegrationFeatures
		public BooleanRegistryItem EnableMexicanPortIntegrationFeatures
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableMexicanPortIntegrationFeatures", delegate
				{
					return new BooleanRegistryItem(
						"EnableMexicanPortIntegrationFeatures",
						Categories.Freight,
						(NoResString)"Enables various Mexican port integration features",
						(NoResString)"If yes, enable new Mexican port integration functionality for testing.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		#endregion

		#region EnableSpanishPortIntegrationFeatures
		public BooleanRegistryItem EnableSpanishPortIntegrationFeatures
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableSpanishPortIntegrationFeatures", delegate
				{
					return new BooleanRegistryItem(
						"EnableSpanishPortIntegrationFeatures",
						Categories.Freight,
						(NoResString)"Enables various Spanish sea port integration features",
						(NoResString)"If yes, enable new Spanish sea port integration functionality for testing.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		#endregion

		#region EnableChinaCustomsTaxNumberTable

		public BooleanRegistryItem EnableChinaCustomsTaxNumberTable
		{
			get => GetItem<BooleanRegistryItem>("EnableChinaCustomsTaxNumberTable", delegate
			{
				return new BooleanRegistryItem(
					"EnableChinaCustomsTaxNumberTable",
					Categories.Freight,
					(NoResString)"Uses the database table for tax number information",
					(NoResString)"If yes, use the tax number table in the database for china customs",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
					true);
			});
		}

		#endregion

		#region EnableInternationalTradeDocumentsFunctionality

		public BooleanRegistryItem EnableInternationalTradeDocumentsFunctionality
		{
			get => GetItem("EnableInternationalTradeDocumentsFunctionality", () =>
				new BooleanRegistryItem(
					"EnableInternationalTradeDocumentsFunctionality",
					Categories.Freight,
					(NoResString)"Enables Certificate of Origin functionaltiy for International Trade Documents",
					(NoResString)"If yes, enables Certificate of Origin functionality.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
					false));
		}

		#endregion

		#region Freight/Contracts Registry Items

		public BooleanRegistryItem EnableCarrierAndClientContractModules
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableCarrierAndClientContractModulesCWNext", delegate
				{
					return new BooleanRegistryItem("EnableCarrierAndClientContractModulesCWNext",
						Categories.Freight_Contracts,
						(NoResString)"Enable Carrier Contract & Client Contract Modules",
						(NoResString)@"Set this registry to 'Yes' to enable Carrier Contract & Allocations module and Client Contract & Allocations module for production of clients who have subscribed to these functions.
Set this registry to 'Yes' to enable Carrier Contract & Allocations module and Client Contract & Allocations module for trial or testing in a non-Production environment.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem AllowSameUNLOCOForContractInternationalZones
		{
			get
			{
				return GetItem("AllowSameUNLOCOForContractInternationalZones", delegate
				{
					return new BooleanRegistryItem(
						"AllowSameUNLOCOForContractInternationalZones",
						Categories.Freight_Contracts,
						ResString.GetMultilingualString("188748a4-5fd0-008f-43d2-75cf0c7ecd5a", "Allow the same UNLOCO for multiple Contract International Zones"),
						ResString.GetMultilingualString("7d113d34-5c5b-ef8e-45cf-3c7446923349", @"Allow the same UNLOCO to be added to multiple International Zones with Type of ‘CON’.

Note: Setting this registry to Yes may result in Allocation Routes with different but overlapping International Zones as Load / Discharge Ports being loaded into the ‘Contract & Allocation Routes Search Form’ launched from relevant jobs."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region Delivery Due Date API

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:WordSpellingRule")]
		public StringRegistryItem DeliveryDueDateAPIInboundAuthentications
		{
			get
			{
				return GetItem<StringRegistryItem>("DeliveryDueDateAPIInboundAuthentications", delegate
				{
					var result = new StringRegistryItem(
						"DeliveryDueDateAPIInboundAuthentications",
						Categories.Freight,
						ResString.GetMultilingualString("9B77C44A-FD0B-4098-B494-5A142C6A4782", "Delivery Due Date API Inbound Authentications"),
						ResString.GetMultilingualString("FD06AF30-8040-495C-A974-8E01E1320DFE", "This configuration, defines username/password that is needed to allow access to Delivery Due Date API."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						null);

					result.EditorInfo = new AuthenticationRegistryItemEditorInfo();
					return result;
				});
			}
		}
		#endregion

		#region FreightEnableComplianceWise

		public EnableComplianceWiseRegistryItem FreightEnableComplianceWise
		{
			get
			{
				return GetItem("FreightEnableComplianceWise", delegate
				{
					return new EnableComplianceWiseRegistryItem("FreightEnableComplianceWise",
						Categories.Freight_Compliance,
						ResString.GetMultilingualString("39464742-5CE8-4B1B-BD84-835212086AC9", "Enable ComplianceWise"),
						ResString.GetMultilingualString("76888B6B-EDED-492F-BD9C-B07204E05F19", @"When disabled, ComplianceWise will not be available in the Booking, Shipment and Consolidation modules.

When enabled, the Booking, Shipment and Consolidation modules will use the ComplianceWise system to perform compliance risk checks. Compliance workflows and other compliance capabilities will be available to use in these modules.

Refer to WiseTech Academy eLearning to prepare and plan your transition to ComplianceWise."),
						RegistryStorageFlags.System,
						RawDataRegistry.Instance.EnableComplianceRisk.Value ? RegistryOptions.IsOnlyForSupport : (RegistryOptions.IsOnlyForSupport | RegistryOptions.IsHidden),
						new EnableComplianceWiseRegistryBusinessObject
						{
							EnableComplianceWise = true
						});
				});
			}
		}

		#endregion

		#region Assembly Master On Direct Consol Compliance Disclaimer

		public BooleanRegistryItem AssemblyMasterOnDirectConsolComplianceDisclaimer
		{
			get
			{
				return GetItem("AssemblyMasterOnDirectConsolComplianceDisclaimer", () =>
				{
					return new BooleanRegistryItem(
						"AssemblyMasterOnDirectConsolComplianceDisclaimer",
						Categories.Freight_Consolidations,
						ResString.GetMultilingualString("43beece1-ad0e-4556-b72c-85393c7a0a6c", "Assembly Master on Direct Consol - Compliance Disclaimer"),
						ResString.GetMultilingualString("9048609b-5d42-497c-98a4-1640d274383d", @"Enable this system registry to allow users to check the tick box 'I acknowledge' to confirm their awareness that using Assembly Master as Direct Master might breach customs law.
When it is set to No, users will be required to type the disclaimer 'I AM AWARE THAT ASSEMBLY MASTER AS DIRECT MASTER MAY BRIDGE CUSTOMS LAW'."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion
	}
}
