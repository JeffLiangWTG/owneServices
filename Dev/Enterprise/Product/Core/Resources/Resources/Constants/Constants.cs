using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using Res = Resources.Res;
using ResString = Resources.ResString;

namespace Enterprise.Core
{
	public static partial class Constants
	{
		#region User

		public static class GlobalUserTypes
		{
			public const string Resource = "RES";
			public const string CLUser = "CLU";
			public const string Staff = "STA";
		}

		#endregion

		#region Modules

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Global Module Names Constants")]
		public static class GlobalModuleNamesConstants
		{
			public const string Forwarding = "Forwarding";
			public const string CFS = "CFS";
			public const string Orders = "Orders";
			public const string Transport = "Transport";
			public const string Warehouse = "Warehouse";
			public const string ShipsAgency = "Ships Agency";
			public const string CustomsDeclarations = "Customs Declarations";
			public const string HVLV = "HVL";
		}

		#endregion

		#region Web

		public static class WebWorkflowType
		{
			public const string ForwardingShipment = "SHP";
			public const string WarehouseReceive = "WIN";
			public const string Declaration = "BRK";
			public const string ISF = "ISF";
			public const string ForwardingBooking = "FBK";
			public const string Container = "CTN";
			public const string Order = "ORD";
			public const string BillOfLading = "BOL";
			public const string ShippingBooking = "BKN";
			public const string Cartage = "TRN";
			public const string WarehouseOrder = "WOU";
		}

		#endregion

		#region SystemUpgrade

		public static class SystemUpgrade
		{
			public const string DbUpgradeLock = nameof(SystemUpgrade) + "-" + nameof(DbUpgradeLock);
			public const string DbUpgradeLockExtPty = DbUpgradeLock + ".ExtProperty";
		}

		#endregion

		public static class DefaultControllingAgentOrgTypes
		{
			public static class Code
			{
				public const string ControllingCustomer = "CCR";
				public const string BillToParty = "BTP";
				public const string BookingParty = "BKD";
				public const string ConsignorConsignee = "CRE";
			}

			public static class Description
			{
				public static MultilingualString ControllingCustomer
				{
					get { return ResString.GetMultilingualString("c5959a5b-7ba6-4010-99e7-4925f42288a5", "Controlling Customer"); }
				}

				public static MultilingualString BillToParty
				{
					get { return ResString.GetMultilingualString("e1e409cc-5983-4b70-91e9-118f36e199c0", "Freight Bill To"); }
				}

				public static MultilingualString BookingParty
				{
					get { return ResString.GetMultilingualString("4168a0b0-2d8b-47a1-bf13-11ba5f2d917b", "Booking Party"); }
				}

				public static MultilingualString ConsignorConsignee
				{
					get { return ResString.GetMultilingualString("8af7af51-69f6-4b7c-9292-88eed939b6fe", "Consignor/Consignee"); }
				}
			}
		}

		#region PartAttributes

		public static class PartAttributes
		{
			public static class Code
			{
				public const string Attribute1 = "AT1";
				public const string Attribute2 = "AT2";
				public const string Attribute3 = "AT3";
				public const string SerialNumber = "SER";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Column identifier")]
			public const string SerialNumberPropertyName = "Serial Number";
		}

		#endregion

		public static class DefaultControllingCustomerOrgTypes
		{
			public static class Code
			{
				public const string BillToParty = "BTP";
				public const string BookingParty = "BKD";
				public const string ConsignorConsignee = "CRE";
			}

			public static class Description
			{
				public static MultilingualString BillToParty
				{
					get { return ResString.GetMultilingualString("fb7db97d-3a17-42d4-a0c6-cb687a111a62", "Freight Bill To"); }
				}

				public static MultilingualString BookingParty
				{
					get { return ResString.GetMultilingualString("4cc6b7a0-538b-4881-a9fb-bea3b246ed8d", "Booking Party"); }
				}

				public static MultilingualString ConsignorConsignee
				{
					get { return ResString.GetMultilingualString("c0bdce43-2015-446e-a6e6-c97f14206aeb", "Consignor/Consignee"); }
				}
			}
		}

		#region Registry Constants

		public static class RequireReasonForCLRRegistryConstants
		{
			public static class Code
			{
				public const string Default = "DEF";
				public const string Other = "OTH";
			}

			public static class Description
			{
				public static MultilingualString Default => ResString.GetMultilingualString("AD2442F3-A93B-436F-8E19-21D1A44C796D", "Default");
				public static MultilingualString FreeText => ResString.GetMultilingualString("885E28E7-7F2D-40A0-BF37-5517611BB3EF", "Free Text");
				public static MultilingualString Other => ResString.GetMultilingualString("7BFADC5C-2B93-499C-AE6F-F7B010A64C88", "Other");
			}
		}

		public static class AVSRegistryConstants
		{
			public static class AddressTypes
			{
				public static MultilingualString All
				{
					get { return ResString.GetMultilingualString("f3ed3914-7d34-421a-a388-70b745edc2e9", "All"); }
				}
			}

			public static class ControllerNames
			{
				public static MultilingualString All
				{
					get { return ResString.GetMultilingualString("f3ed3914-7d34-421a-a388-70b745edc2e9", "All"); }
				}
			}
		}

		public static class LastDatabaseRestoreRegistryConstants
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
			public const string CompletionDateFormat = "dd/MM/yyyy h:mm:ss tt";
		}

		public static class ExportToExcelFormats
		{
			public static class Code
			{
				public const string Xls = "XLS";
				public const string Xlsx = "XLSX";
			}

			public static class Description
			{
				public static MultilingualString Xls => ResString.GetMultilingualString("57D69961-E95C-4B46-840A-076FC0975BF8", "XLS Excel 97-2003 Workbook");
				public static MultilingualString Xlsx => ResString.GetMultilingualString("EAB9D91F-6B4A-4EF1-BA5E-5220FBD41C78", "XLSX Excel Workbook");
			}
		}

		#endregion

		public static class LicenceConstants
		{
			public const string NotHostedWithCargoWise = "NCW";
			public static readonly Guid EncryptionKey = new Guid("41D5F94C-91C9-4DD8-A036-03B2245C9AC4");
		}

		public static class WiseTechGlobalInternalSystemCodes
		{
			public static string[] AllCodes => new[] { EDI, HYE, INZ, WTL, WUT, EHW };

			public const string EDI = "EDI";
			public const string HYE = "HYE";
			public const string INZ = "INZ";
			public const string WTL = "WTL";
			public const string WUT = "WUT";
			public const string EHW = "EHW";
		}

		public static class ShipmentNumberImportTypes
		{
			public static class Code
			{
				public const string HouseBill = "HBL";
				public const string ShipmentNumber = "SHP";
				public const string ShipmentThenHouseBill = "SHB";
			}

			public static class Description
			{
				public static MultilingualString HouseBill { get { return ResString.GetMultilingualString("3679eb8f-1886-4480-89b3-821312584b10", "Update existing shipment by matching HBL."); } }
				public static MultilingualString ShipmentNumber { get { return ResString.GetMultilingualString("65af6b92-0fd1-4c36-84d0-e8f15afc90a5", "Update existing shipment by matching Shipment number in XML (Agent Reference tag)."); } }
				public static MultilingualString ShipmentThenHouseBill { get { return ResString.GetMultilingualString("1b416b63-5239-49c4-8ccc-00e7e3d7f564", "Update existing shipment by matching Shipment number in XML with fallback to matching by HBL."); } }
			}
		}

		public static class HouseBillOfLadingTypes
		{
			public static class Code
			{
				public const string ITClubAustralia = "IAU";
				public const string ITClubAustraliaNoTerms = "ISI";
				public const string ITClubAustraliaNoTermsNoLaw = "INN";
				public const string ITClubAustraliaPreprinted = "ITP";
				public const string ITClubNewZealand = "INZ";
				public const string ITClubNewZealandPreprinted = "INP";
				public const string TTClubAustraliaNZ = "TNZ";
				public const string TTClubAustraliaNZPreprinted = "TTP";
				public const string FIATAHBL = "FIA";
				public const string FIATAHBLPreprinted = "FIP";
				public const string TANHBL = "TAN";
				public const string TANHBLPreprinted = "TAP";
				public const string CargowiseBill = "EAG";
				public const string CargowiseBillPreprinted = "EAP";
				public const string DataHawkBill = "DHK";
				public const string TTClubUnitedStates = "TUS";
				public const string TTClubUnitedStatesPreprinted = "TUP";
				public const string CartaPorteSpanish = "CPT";
			}

			public static class Description
			{
				public static MultilingualString ITClubAustralia { get { return ResString.GetMultilingualString("54ac08a3-29f7-42ac-b3d0-88d2802c332b", "IT Club Australia"); } }
				public static MultilingualString ITClubAustraliaPreprinted { get { return ResString.GetMultilingualString("23438acd-aec3-4aff-9650-4f7959456895", "IT Club Australia Preprinted"); } }
				public static MultilingualString ITClubNewZealand { get { return ResString.GetMultilingualString("73ec7da4-f14a-4505-9744-57732b81f128", "IT Club New Zealand"); } }
				public static MultilingualString ITClubNewZealandPreprinted { get { return ResString.GetMultilingualString("77512916-e5f3-4229-ac82-a414579cccfb", "IT Club New Zealand Preprinted"); } }
				public static MultilingualString TTClubAustraliaNZ { get { return ResString.GetMultilingualString("b8beaba9-6973-4793-a53c-9116587d64c4", "TT Club / Australia / NZ"); } }
				public static MultilingualString TTClubAustraliaNZPreprinted { get { return ResString.GetMultilingualString("0507aa9c-cd04-4946-a9c4-e938adcdec17", "TT Club / Australia / NZ Preprinted"); } }
				public static MultilingualString FIATAHBL { get { return ResString.GetMultilingualString("6f969c53-2efa-4534-b26c-0cb856d2e5ce", "FIATA HBL"); } }
				public static MultilingualString FIATAHBLPreprinted { get { return ResString.GetMultilingualString("65dad2d4-ab47-4ba9-a4f9-0ab23d714f05", "FIATA HBL Preprinted"); } }
				public static MultilingualString TANHBL { get { return ResString.GetMultilingualString("0fa51076-c585-4d09-9306-b93ca70f4263", "TAN HBL"); } }
				public static MultilingualString TANHBLPreprinted { get { return ResString.GetMultilingualString("2098374d-8e29-4037-aab3-5a7615dcec25", "TAN HBL Preprinted"); } }
				public static MultilingualString CargowiseBill { get { return ResString.GetMultilingualString("cb744f81-c67b-4430-9320-462652de1c13", "CargoWise Bill"); } }
				public static MultilingualString CargowiseBillPreprinted { get { return ResString.GetMultilingualString("b6d9ecd5-b426-4ebc-803d-a4960c02e820", "CargoWise Bill Preprinted"); } }
				public static MultilingualString DataHawkBill { get { return ResString.GetMultilingualString("baffd296-686b-41f5-95ba-e220f5fb7073", "DataHawk Bill"); } }
				public static MultilingualString TTClubUnitedStates { get { return ResString.GetMultilingualString("64a92941-83b0-4fe3-ad6d-82e18e5d8b2d", "TT Club United States"); } }
				public static MultilingualString TTClubUnitedStatesPreprinted { get { return ResString.GetMultilingualString("76f94ec3-d7b7-4c7c-841c-f00e46a8631c", "TT Club United States Preprinted"); } }
				public static MultilingualString CartaPorteSpanish { get { return ResString.GetMultilingualString("76430a41-a9d3-4bf4-8f17-b160440a3bd8", "Carta Porte - Spanish"); } }
			}
		}

		public static class AirBookingStatus
		{
			public static class Code
			{
				public const string All = "ALL";
				public const string AllFlightsConfirmed = "ALC";
				public const string ConfirmationPending = "CNP";
				public const string CancellationPending = "CAP";
				public const string NotRequested = "NRQ";
				public const string Rejected = "REJ";
			}

			public static class Description
			{
				public static MultilingualString All { get { return ResString.GetMultilingualString("8ae276c6-2861-01be-4502-b1890d786f92", "All status"); } }
				public static MultilingualString AllFlightsConfirmed { get { return ResString.GetMultilingualString("e06d67d0-8336-d0b3-444c-d8f3696507ec", "All flights confirmed"); } }
				public static MultilingualString ConfirmationPending { get { return ResString.GetMultilingualString("864b743a-a916-87a5-4346-4bf1491a86d7", "Confirmation Pending"); } }
				public static MultilingualString CancellationPending { get { return ResString.GetMultilingualString("b700d70e-809d-c987-42cf-6e39e338d1f3", "Cancellation Pending"); } }
				public static MultilingualString NotRequested { get { return ResString.GetMultilingualString("098ec3cb-eb7e-1e88-4eb0-f71d8bf9b1b5", "Not Requested"); } }
				public static MultilingualString Rejected { get { return ResString.GetMultilingualString("bb1389d6-c324-ac90-47ac-1064ccf628db", "Rejected"); } }
			}
		}

		public static class AddtionalHouseBillTypeMenu
		{
			public static class Code
			{
				public const string YusenHBL = "YUS";
				public const string YusenHBLStyle1 = "YS1";
				public const string YusenHBLStyle2 = "YS2";
				public const string YusenHBLStyle3 = "YS3";
				public const string YusenHBLStyle4 = "YS4";
				public const string DHLHBL = "DHL";
				public const string DHLHBLStyle1 = "DH1";
				public const string DHLHBLStyle2 = "DH2";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu Item")]
			public const string YusenHBLMenuName = "Yusen HBL";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu Item")]
			public const string YusenHBLStyle1MenuName = "Yusen HBL Style 1";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu Item")]
			public const string YusenHBLStyle2MenuName = "Yusen HBL Style 2";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu Item")]
			public const string YusenHBLStyle3MenuName = "Yusen HBL Style 3";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu Item")]
			public const string YusenHBLStyle4MenuName = "Yusen HBL Style 4";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu Item")]
			public const string DHLHBLMenuName = "DHL HBL";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu Item")]
			public const string DHLHBLStyle1MenuName = "DHL HBL Style 1";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu Item")]
			public const string DHLHBLStyle2MenuName = "DHL HBL Style 2";
		}

		public static class ShipmentCCNCustomizationTypes
		{
			public static class Code
			{
				public const string HouseBill = "HBL";
				public const string ShipmentNumber = "SHP";
				public const string NumberFountain = "NFN";
			}

			public static class Description
			{
				public static MultilingualString HouseBill { get { return ResString.GetMultilingualString("02DDABB0-1233-4E32-81C0-373CF4E2C5F0", "Use the last x digits of the shipment HAWB/HBL number"); } }
				public static MultilingualString ShipmentNumber { get { return ResString.GetMultilingualString("28CDF267-19F7-4714-9B0A-805C3A8B9C32", "Use the last x digits of the shipment number"); } }
				public static MultilingualString NumberFountain { get { return ResString.GetMultilingualString("E7895971-2EB3-4B3B-9A7C-DBA7E85F268B", "Use 8 digits from system number fountain"); } }
			}
		}

		public static class ShipmentAutomaticImportOptions
		{
			public static class Code
			{
				public const string Update = "UPD";
				public const string NoImport = "NOI";
				public const string Create = "CRT";
			}

			public static class Description
			{
				public static MultilingualString Update { get { return ResString.GetMultilingualString("53600bf5-1177-45a3-a735-9708912ceb27", "Update existing Shipment"); } }
				public static MultilingualString NoImport { get { return ResString.GetMultilingualString("c9a622cb-75f4-45b8-9224-e940c84ad8c7", "Do not import the Shipment"); } }
				public static MultilingualString Create { get { return ResString.GetMultilingualString("e7387b73-29aa-496b-bba3-5191c71726c8", "Create new Shipment"); } }
			}
		}

		public static class ConsolidationCCNCustomizationTypes
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Default Customization Type")]
			public static class Code
			{
				public const string Non = "No";
				public const string MasterBill = "MBL";
				public const string CarrierCode = "CCC";
				public const string CCN = "CCN";
			}

			public static class Description
			{
				public static MultilingualString Non { get { return ResString.GetMultilingualString("84345b39-e3ef-498a-91b4-1c884feeffbd", "No Customization"); } }
				public static MultilingualString MasterBill { get { return ResString.GetMultilingualString("fd6e5b74-c052-4392-ad99-5a1c9a7a2299", "Use Master Bill Number"); } }
				public static MultilingualString CarrierCode { get { return ResString.GetMultilingualString("cf96150e-49bb-49f8-9cce-3a1d1c04038d", "Carrier Code Only"); } }
				public static MultilingualString CCN { get { return ResString.GetMultilingualString("42a4467a-41b7-4c72-a7c1-604e4043358b", "Copy CCN"); } }
			}
		}

		public static class EFreightStatus
		{
			public static class Code
			{
				public const string NON = "NON";
				public const string EAP = "EAP";
				public const string EAW = "EAW";
				public const string ECC = "ECC";
			}

			public static class Description
			{
				public static MultilingualString NON { get { return ResString.GetMultilingualString("b3268961-614a-4e05-9bcc-463d09d289d2", "e-freight NOT supported"); } }
				public static MultilingualString EAP { get { return ResString.GetMultilingualString("6b62c892-a6e4-47d3-b162-2565d0365bb1", "e-freight Consignment with Accompanying Paper Documents"); } }
				public static MultilingualString EAW { get { return ResString.GetMultilingualString("9b1ac378-64bb-4767-8565-d3569546b577", "e-freight Consignment with No Accompanying Paper Documents"); } }
				public static MultilingualString ECC { get { return ResString.GetMultilingualString("723c0913-098f-4e84-aed1-4e7d1cdd2557", "Consignment established with an electronically concluded cargo contract with no accompanying paper Air Waybill"); } }
			}
		}

		public static class FreightShipmentDirection
		{
			public static class Code
			{
				public const string All = "ALL";
				public const string Import = "IMP";
				public const string Export = "EXP";
				public const string Domestic = "DOM";
				public const string Other = "OTH";
				public const string CrossTrade = "CRS";
			}

			public static class Description
			{
				public static MultilingualString All { get { return ResString.GetMultilingualString("a5a88c32-b189-4929-b920-8ab8ab051759", "All"); } }
				public static MultilingualString Import { get { return ResString.GetMultilingualString("ec3db510-eef5-491d-9eb1-a97eb0ddb18e", "Import"); } }
				public static MultilingualString Export { get { return ResString.GetMultilingualString("aa66eb80-e722-47ca-ab83-b20237c92bcb", "Export"); } }
				public static MultilingualString Domestic { get { return ResString.GetMultilingualString("1867d638-8895-49da-a73f-177a546872a5", "Domestic"); } }
				public static MultilingualString Other { get { return ResString.GetMultilingualString("6f0ff1e1-5f4e-48a5-9c82-7476367d6c8c", "Other"); } }
			}
		}

		public static class InvoicePostingExchangeRateCurrencyType
		{
			public static class Code
			{
				public const string Local = "LOC";
				public const string Foreign = "FOR";
				public const string NotApplicable = "";
			}

			public static class Description
			{
				public static MultilingualString Local { get { return ResString.GetMultilingualString("beb0c0e7-daff-415a-a0cf-750fa79e1d28", "Local Invoice Currency"); } }
				public static MultilingualString Foreign { get { return ResString.GetMultilingualString("a7b3c1bd-691c-49f4-92eb-9dd3bbe8c0e9", "Foreign Invoice Currency"); } }
				public static MultilingualString NotApplicable { get { return ResString.GetMultilingualString("c4877d1b-e2d1-4864-976d-5ba2a7daef97", "Applies to local and foreign invoice currency"); } }
			}
		}

		public static class JobBillingExchangeRatePreference
		{
			public static class Code
			{
				public const string TodaysRate = "TDR";
				public const string ConsolExchangeRate = "CER";
				public const string HistoricalRateFromActualArrivalDate = "HAR";
				public const string HistoricalRateFromActualDepartureDate = "HDR";
				public const string HistoricalRateFromEstimatedArrivalDate = "HEA";
				public const string HistoricalRateFromEstimatedDepartureDate = "HED";
				public const string HistoricalRateFromEstimatedArrivalAtLoadPortDate = "HEL";
				public const string HistoricalRateFromActualArrivalAtLoadPortDate = "HAL";
				public const string PickupDate = "ESP";
				public const string DeliveryDate = "ESD";
				public const string FinalizedDate = "FIN";
				public const string RequiredDate = "REQ";
				public const string HouseBillIssueDate = "HBD";
				public const string ShipmentOrBrokeragePickupDate = "PIC";
				public const string ShipmentOrBrokerageDeliveryDate = "DEL";
			}

			public static class Description
			{
				public static MultilingualString TodaysRate { get { return ResString.GetMultilingualString("2ad93bae-3c63-4f42-b7de-2fb075ddaad5", "Today's Rate"); } }
				public static MultilingualString ConsolExchangeRate { get { return ResString.GetMultilingualString("f65ea54d-2657-467c-a756-d5ea0790732e", "Consol Exchange Rate"); } }
				public static MultilingualString HistoricalRateFromActualArrivalDate { get { return ResString.GetMultilingualString("44a733d8-2c0a-4871-8cc0-f5313bdac22a", "Historical Rate from Actual/Estimated Arrival Date"); } }
				public static MultilingualString HistoricalRateFromActualDepartureDate { get { return ResString.GetMultilingualString("cfe6add2-0a37-4248-8b39-efe3ec20ad6b", "Historical Rate from Actual/Estimated Departure Date"); } }
				public static MultilingualString HistoricalRateFromEstimatedArrivalDate { get { return ResString.GetMultilingualString("c1d10f68-a652-4097-8da0-e2bb15c6cf00", "Historical Rate from Estimated Arrival Date"); } }
				public static MultilingualString HistoricalRateFromEstimatedDepartureDate { get { return ResString.GetMultilingualString("054dd504-fb5b-42ca-81c7-36659416e606", "Historical Rate from Estimated Departure Date"); } }
				public static MultilingualString HistoricalRateFromEstimatedArrivalAtLoadPortDate { get { return ResString.GetMultilingualString("7D3F4B86-0172-4114-9780-D4AE95C8D07C", "Historical Rate from Estimated Arrival at Load Port Date"); } }
				public static MultilingualString HistoricalRateFromActualArrivalAtLoadPortDate { get { return ResString.GetMultilingualString("8e7b7cce-6adb-4556-b123-e40d08fd7ee3", "Historical Rate from Actual/Estimated Arrival at Load Port Date"); } }
				public static MultilingualString PickupDate { get { return ResString.GetMultilingualString("08699e60-d206-4fb7-98f5-f5e17065a5ab", "Pickup Date"); } }
				public static MultilingualString DeliveryDate { get { return ResString.GetMultilingualString("95cb48a4-366a-400c-8854-21ef80985a42", "Delivery Date"); } }
				public static MultilingualString FinalizedDate { get { return ResString.GetMultilingualString("2ec2684b-76e9-4c9c-90c9-26ad0f3a34ae", "Finalized Date"); } }
				public static MultilingualString RequiredDate { get { return ResString.GetMultilingualString("e7406fa5-1343-44dc-8833-09ce76d8973e", "Required Date"); } }
				public static MultilingualString HouseBillIssueDate { get { return ResString.GetMultilingualString("A083216A-3C75-4700-A07C-28A751237C54", "House Bill Issue Date"); } }
				public static MultilingualString ShipmentOrBrokeragePickupDate { get { return ResString.GetMultilingualString("B90751DB-853B-44E4-A249-82218A1F43AF", "Pickup Date"); } }
				public static MultilingualString ShipmentOrBrokerageDeliveryDate { get { return ResString.GetMultilingualString("6E4BD581-4BBC-457E-A506-FDF538FAF996", "Delivery Date"); } }
			}
		}

		public static class FacilityType
		{
			public static class Code
			{
				public const string Terminal = "CTO";
				public const string ContainerYard = "CYD";
				public const string TransitWarehouse = "CFS";
			}

			public static class Description
			{
				public static MultilingualString Terminal { get { return ResString.GetMultilingualString("830FA007-6426-42C5-A487-CC24170EE0CD", "Terminal"); } }
				public static MultilingualString ContainerYard { get { return ResString.GetMultilingualString("B3EE4593-2E01-4C56-9E60-4F4ADC2952F0", "Container Yard"); } }
				public static MultilingualString TransitWarehouse { get { return ResString.GetMultilingualString("AD0DDC3F-FC7A-42C7-AE87-2AFA1BCAD18F", "Transit Warehouse"); } }
			}
		}

		public static string CompanyBrandingName => BrandingFactory.Instance.CompanyBrandingName;

		public static string ProductName => BrandingFactory.Instance.ProductName;

		public static string ProductSupportName => BrandingFactory.Instance.ProductSupportName;

		public const string PasswordOK = "OK";

		public const string GatewaySuffixForJobHeaderDeprecated = "GW";

		/// <summary>
		/// Lists logical names for BusinessObject/Data in Enterprise, e.g. Shipment documents use the Shipment Data which is identified by a shipment PK.
		/// </summary>
		public enum DataContext
		{
			// Make sure they are all less than 35 characters because of database VC(35)
			None = 0,
			AccountingVoucher = 1,
			AgencyShipment = 111,
			AP = 2,
			APPayment = 3,
			AR = 4,
			ARInvoice = 5,
			ARBatchInvoice = 6,
			ATD = 7,
			AuCusInvc = 8,
			AUCustoms = 9,
			AWB = 10,
			BaseConsol = 11,
			BusinessObject = 120,
			Cartage = 12,
			CartageAdvice = 13,
			ChargeSheet = 14,
			Cheques = 15,
			ChinaJournalListing = 18,
			CFSAndForwardingShipment = 16,
			CFSContainerLeg = 17,
			CFSRating = 20,
			CMRPAYRECMessage = 21,
			CMRREFACCMessage = 22,
			CombinedCartageAdvice = 23,
			ComInvoiceHeader = 24,
			CommercialInvoice = 25,
			CommonConsol = 26,
			CompanyCampaign = 27,
			Consol = 28,
			Container = 29,
			ContainerLeg = 30,
			ContainerRego = 31,
			CusEntryHeader = 33,
			CusHAWB = 34,
			Declaration = 35,
			DeclarationWithCusEntryHeaders = 36,
			DepositBatch = 37,
			DirectPayment = 38,
			DocumentCoverSheet = 39,
			DocumentDailyWorkSheet = 40,
			EFTPaymentAdvice = 41,
			ERA = 42,
			ForwardingShipment = 44,
			ForwardingConsol = 45,
			ForwardingPreAdvice = 46,
			ForwardingShipmentAndConsol = 47,
			FreightLabels = 48,
			GL = 49,
			GLJournal = 50,
			GatePassContainerLeg = 51,
			GatePassPackLines = 52,
			GatePassShipment = 53,
			HotCheque = 54,
			IMO = 55,
			ImportCargoLabel = 56,
			Incident = 57,
			JobComInvoiceHeader = 58,
			JobInvoicingJob = 59,
			LandedCostEntryHeaders = 60,
			LandedCostHeader = 61,
			LetterOfIndemnity = 63,
			LoadListDocument = 64,
			LoadListConsol = 65,
			LTL = 66,
			MasterTransferRecord = 67,
			Notes = 68,
			NZECIDeclaration = 69,
			NZCustoms = 70,
			Order = 71,
			WhsOrder = 72,
			Org = 73,
			Organisation = 74,
			OrgBuyerLink = 75,
			OrgSupplierLink = 76,
			PackLines = 78,
			PackUnpackContainerRego = 79,
			PreAlert = 80,
			ProfitShareDetail = 81,
			Quotation = 82,
			Rating = 83,
			RefDocType = 85,
			RequestForMissingDocuments = 86,
			RequestForService = 87,
			Sailing = 88,
			Service = 89,
			Shipment = 90,
			ShipmentDeclaration = 91,
			ShipmentReceival = 92,
			ShipperDepartureNotice = 93,
			ShippingOrder = 94,
			Statement = 95,
			TallyContainer = 96,
			TimeSlotRequest = 97,
			TrainingCourse = 98,
			TransactionHeader = 99,
			TransportRating = 100,
			UnitTest = 101,
			UNLOCO = 102,
			WarehouseRating = 103,
			Worksheet = 104,
			WowIndentOrder = 105,
			WhsInvoiceDetail = 106,
			WhsInventory = 107,
			WhsReceive = 108,
			PaymentApproval = 109,
			WhsPick = 110,
			WhsDeliveryLabels = 112,
			GenericFreightJob = 113,
			GenericCommercialInvoice = 114,
			WhsReceiveInvoiceJobHistory = 116,
			WhsOrdersInvoiceJobHistory = 117,
			SEVDailyWorkSheet = 118,
			PickListDocument = 119,
			WhsPickShortfallItems = 121,
			WhsPickNonPickedItems = 122,
			SGPrintPermit = 123,
			WhsPackageLabels = 124,
			CTOCusMAWB = 125,
			SEVRedeliveryPickList = 126,
			MapGenericFreightJob = 127,
			SGRefundInfo = 129,
			AirCTOExport = 130,
			WhsAdjustment = 131,
			AgencyContainer = 132,
			WhsStocktake = 133,
			WhsTransfer = 134,
			SystemSectionRepository = 135,
			EUR1 = 136,
			WhsLocationLabels = 137,
			CommonWorkSheet = 138,
			GenericFreightJobRouting = 139,
			ContainerRelease = 140,
			CommonCartage = 141,
			CommonCartageLeg = 142,
			CommonContainer = 143,
			CusEntryHeaderENS = 144,
			DetentionInvoice = 145,
			SADH = 146,
			QuotedBooking = 147,
			APInvoice = 148,
			WhsPalletIDLabels = 149,
			ShippingRating = 151,
			AgencyVoyageAccount = 152,
			WhsWorkOrder = 153,
			MasterBill7512Departure = 154,
			MasterBill7512Arrival = 155,
			MasterBill7512Export = 156,
			MasterBill7512TOL = 157,
			GenericFreightJobByContainerIfFCL = 158,
			PickupDeliveryConfirm = 159,
			CASSBilling = 160,
			AccQueryClaim = 161,
			AgencyDetentionAdvice = 162,
			ConsolidatedTransportBooking = 163,
			ShippingDetentionRating = 164,
			OrganisationIRS1099 = 165,
			OperationalActions = 166,
			GenericFreightJobServices = 167,
			GenericPickupDeliveryConfirm = 168,
			CostsComparer = 169,
			StatementSummary = 170,
			CusInBondHeader = 171,
			WhsPickableDocket = 172,
			GenericLocalTransportLeg = 173,
			CargoWiseBilling = 174,
			GenericFreightJobFromShipment = 175,
			GenericChargeSheet = 176,
			JobRevenueJournal = 177,
			GenericFreightJobFrmShipByContIfFCL = 178,
			InBond7512Departure = 179,
			DtbConsolidation = 180,
			DtbBooking = 181,
			GbCcsuk = 182,
			GenericFreightJobByComInv = 183,
			GenericFreightJobTransportBooking = 184,
			GenericFreightJobByReleaseStatus = 185,
			GenericFreightJobOrders = 186,
			GenericFreightJobOrder = 187,
			UsProtest = 188,
			GenericBasicLabelAll = 189,
			GenericBasicLabel = 190,
			GenericProductLabelAll = 191,
			GenericProductLabel = 192,
			GenericFreightJobByContainer = 193,
			GenericDeliveryLabelAll = 194,
			GenericDeliveryLabel = 195,
			ChiefEAD = 196,
			GbTaxEstimator = 197,
			ForwardingConsolGbADS = 198,
			GenericFreightJobInvoice = 199,
			GenericProductDeliveryLabelAll = 200,
			GenericProductDeliveryLabel = 201,
			ClassroomSession = 202,
			ClassroomAttendee = 203,
			GenericFreightJobWhsPick = 204,
			GenericFreightJobByPackages = 205,
			GbMcpPhs11 = 206,
			Project = 207,
			ComplianceSeqBook = 208,
			CAeManifest = 209,
			WorkItem = 210,
			EuNcts = 211,
			JPAFRHeader = 212,
			ConsignmentJobService = 213,
			GenericRetailersLabel = 214,
			GenericRetailersLabelAll = 215,
			WhsArea = 216,
			IndiaManifest = 217,
			ComplianceReport = 218,
			CommissionApprovalReq = 219,
			CollectionBatch = 220,
			CommissionPayment = 221,
			PayableOrder = 222,
			NettingParticipantStatement = 223,
			DispatchConsignment = 224,
			ReceiveConsignment = 225,
			GenericCarrierLabel = 226,
			GenericCarrierLabelAll = 227,
			GlbStaff = 228,
			GlbGroup = 229,
			UXML = 230,
			AccountingJournal = 231,
			GenericDeliveryIDLabelAll = 232,
			LTConsignmentJobService = 233,
			GenericImportCargoLabel = 234,
			AsycudaManifestHeader = 235,
			GenericAuditVarianceLabel = 236,
			GenericAuditVarianceLabelAll = 237,
			OrgOpportunity = 238,
			WhsAdHocServiceJob = 239,
			SkillTestResultBusinessObject = 240,
			OneOffQuote = 241,
			TempStorageHeader = 242,
			GlbPerson = 243,
			ContainerYardRating = 244,
			GbCDSEntryHeader = 245,
			GenericNewPackageID = 246,
			GenericNewPackageIDs = 247,
			AccreditationAttempt = 248,
			ARComplianceDocument = 249,
			CollectionOrder = 250,
			GenericFreightJobBySelectedPackages = 251,
			BankReconciliation = 252,
			TempStorageRegHeader = 253,
			WhsVASOrder = 254,
			ITSadAttachment = 256,
			ESSADH = 257,
			DisbursementJobsCloseBatch = 258,
			CusPackingList = 259,
			ExportDeclarationCertificate = 260,
			GenericNewAndExistingPackageIDs = 262,
			LiquidationDetails = 263,
			ElectronicInvoice = 264,
			ContainerLoadListHeader = 265,
			SalesCall = 266,
			IEEAD = 267,
			CHCustoms = 268,
			T2L = 269,
			WhsDynamicWorkOrder = 270,
			FRSADH = 271,
			T2LF = 272,
			ContainerLoadPlanHeader = 273,
			GenericNewPackageIDs1Doc = 274,
			GenericNewAndExistingPackageIDs1Doc = 275,
			CHCusEntryHeader = 276,
			IDD = 277,
			GenericFreightJobByPackages1Doc = 278,
			GenericFreightJobBySelectedPkgs1Doc = 279,
			Dummy = 900,
			DummyChildren = 901,
			JobSupplierBooking = 902,
			MiscRequest = 903,
			ITTADAttachment = 904,
			CashAdvanceRequest = 300,
			CYDReceiveAdvice = 905,
			CYDReleaseAdvice = 906,
			CYDPickupHeader = 907,
			CarrierShipmentHeader = 908,
			CYDYardUnitState = 909,
			CYDTransportationUnit = 910,
			EMCSDeclaration = 911,
			IEImportAccompanyingDocument = 912,
			IEIADClearanceSlip = 913,
			CarrierShipmentCargo = 914,
			TempStorageRegPremises = 915,
			MNRWorkOrder = 916,
			MNRSurvey = 917,
			MNRWorkOrderLine = 918,
			ZADA306 = 919,
			EuNcts5TAD = 920,
			CarrierVoyage = 921,
			CarrierVoyagePortCall = 922,
			CYDAdHocServiceOrder = 923,
			EuPnts = 924,
			CYDDeliveryHeader = 925,
		}

		public static class GenPivotTypes
		{
			public const string ProcessManagement = "WRK";
			public const string AirlineSignedAWBAgreementWithCompany = "ACE";
			public const string ClientShipmentLink = "CSL";
			public const string XmlEdiMessage = "XEM";
			public const string RelatedActivity = "RAT";
			public const string GlbGroupCoverageArea = "GGC";
			public const string OrgContactDeniedWarehouse = "OCW";
			public const string ProfitShareUserCharges = "PSC";
			public const string CusStorageDocPivotEdiMessage = "CSM";
			public const string CusDV1Detail = "DV1";
			public const string CusNctsContainer = "NCT";
			public const string CusStorageHeaderConsol = "CHC";
			public const string CusStorageHeaderShipment = "CHS";
			public const string CusStorageHeaderDeclaration = "CHD";
			public const string CusStorageHeaderNctsHeader = "CHN";
			public const string Incident = "INC";
			public const string Opportunity = "OPP";
			public const string HighVolumeLowValue = "HVL";
			public const string WorkItemCascade = "WKC";

#if DEBUG
			public const string InvalidCodeForTesting = "XXX";
#endif
		}

		public static class MovementCodes
		{
			public const string Import = "IMP";
			public const string Export = "EXP";
			public const string Indeterminate = "IND";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Colour Depth Constants")]
		public static class ColourDepth
		{
			public const string BlackAndWhite = "Black And White";
			public const string Colour256 = "256 Colour";
			public const string TrueColour = "True Colour";
		}

		public static class FileFormats
		{
			public const string BMP = "BMP";
			public const string CSV = "CSV";
			public const string DOC = "DOC";
			public const string EDP = "EDP";
			public const string GIF = "GIF";
			public const string JPG = "JPG";
			public const string JPEG = "JPEG";
			public const string PNG = "PNG";
			public const string PDF = "PDF";
			public const string PDFA = "PDFA";
			public const string REF = "REF";
			public const string TIF = "TIF";
			public const string TIFF = "TIFF";
			public const string XLS = "XLS";
			public const string XLSX = "XLSX";
			public const string HTML = "HTML";
			public const string HTMF = "HTMF";
			public const string XML = "XML";
			public const string TXT = "TXT";
		}

		public static class DebitCredit
		{
			public const string Debit = "DR";
			public const string Credit = "CR";
		}

		public static class AutoPrintTypes
		{
			public const string AutoPrint = "AUT";
			public const string Manual = "MAN";
		}

		public static class AWB
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Cargo IMP Service Provider Constants")]
			public static class CargoIMPServiceProviderConstants
			{
				public const string CCN = "CCN";
				public const string Descartes = "DESCARTES";
				public const string BTviaCCN = "BT via CCN";
				public const string IATA = "IATA";
				public const string HUB = "HUB";
				public const string EDP = "EDP";
			}

			public static class Dimensions
			{
				public const string DEF = "DEF";
				public const string M3 = "M3";
				public const string PKS = "PKS";
				public const string ALL = "ALL";
				public const string NDA = "NDA";
			}

			public static class ContactCodes
			{
				public const string FAX = "FX";
				public const string TELEPHONE = "TE";
				public const string TELEX = "TL";
			}

			public static class AccountingCodes
			{
				public const string MCO = "MCO";
				public const string GEN = "GEN";
				public const string STL = "STL";
				public const string RET = "RET";
				public const string SRN = "SRN";
				public const string GBL = "GBL";
				public const string SPE = "SPE";
				public const string CNE = "CNE";
				public const string AHD = "AHD";
				public const string ANM = "ANM";
				public const string ANB = "ANB";
				public const string AIS = "AIS";
				public const string ASF = "ASF";
				public const string VKC = "VKC";
				public const string AED = "AED";
				public const string ABT = "ABT";
				public const string IPA = "IPA";
				public const string IPW = "IPW";
				public const string BDT = "BDT";
				public const string BDC = "BDC";
				public const string BDN = "BDN";
			}

			public static class ChargeCodes
			{
				public const string AC = "AC";
				public const string AS = "AS";
				public const string AT = "AT";
				public const string AW = "AW";
				public const string BF = "BF";
				public const string BI = "BI";
				public const string BM = "BM";
				public const string BR = "BR";
				public const string CA = "CA";
				public const string CB = "CB";
				public const string CC = "CC";
				public const string CD = "CD";
				public const string CF = "CF";
				public const string CG = "CG";
				public const string CH = "CH";
				public const string CI = "CI";
				public const string CJ = "CJ";
				public const string DB = "DB";
				public const string DC = "DC";
				public const string DD = "DD";
				public const string DF = "DF";
				public const string DG = "DG";
				public const string DH = "DH";
				public const string DI = "DI";
				public const string DJ = "DJ";
				public const string DK = "DK";
				public const string DV = "DV";
				public const string EA = "EA";
				public const string FA = "FA";
				public const string FB = "FB";
				public const string FC = "FC";
				public const string FE = "FE";
				public const string FF = "FF";
				public const string FI = "FI";
				public const string GA = "GA";
				public const string GT = "GT";
				public const string HB = "HB";
				public const string HR = "HR";
				public const string IA = "IA";
				public const string IN = "IN";
				public const string JA = "JA";
				public const string KA = "KA";
				public const string LA = "LA";
				public const string LC = "LC";
				public const string LE = "LE";
				public const string LF = "LF";
				public const string LG = "LG";
				public const string LH = "LH";
				public const string MA = "MA";
				public const string MB = "MB";
				public const string MC = "MC";
				public const string MD = "MD";
				public const string ME = "ME";
				public const string MF = "MF";
				public const string MG = "MG";
				public const string MH = "MH";
				public const string MI = "MI";
				public const string MJ = "MJ";
				public const string MK = "MK";
				public const string ML = "ML";
				public const string MM = "MM";
				public const string MN = "MN";
				public const string MO = "MO";
				public const string MP = "MP";
				public const string MQ = "MQ";
				public const string MR = "MR";
				public const string MS = "MS";
				public const string MT = "MT";
				public const string MU = "MU";
				public const string MV = "MV";
				public const string MW = "MW";
				public const string MX = "MX";
				public const string MY = "MY";
				public const string MZ = "MZ";
				public const string NE = "NE";
				public const string NS = "NS";
				public const string PA = "PA";
				public const string PB = "PB";
				public const string PK = "PK";
				public const string PU = "PU";
				public const string RA = "RA";
				public const string RB = "RB";
				public const string RC = "RC";
				public const string RD = "RD";
				public const string SA = "SA";
				public const string SB = "SB";
				public const string SC = "SC";
				public const string SD = "SD";
				public const string SE = "SE";
				public const string SF = "SF";
				public const string SI = "SI";
				public const string SO = "SO";
				public const string SP = "SP";
				public const string SR = "SR";
				public const string SS = "SS";
				public const string ST = "ST";
				public const string SU = "SU";
				public const string TC = "TC";
				public const string TI = "TI";
				public const string TR = "TR";
				public const string TV = "TV";
				public const string TX = "TX";
				public const string UB = "UB";
				public const string UC = "UC";
				public const string UD = "UD";
				public const string UE = "UE";
				public const string UF = "UF";
				public const string UG = "UG";
				public const string UH = "UH";
				public const string VA = "VA";
				public const string VB = "VB";
				public const string VC = "VC";
				public const string WA = "WA";
				public const string XB = "XB";
				public const string XD = "XD";
				public const string ZA = "ZA";
				public const string ZB = "ZB";
				public const string ZC = "ZC";
				public const string ZD = "ZD";
				public const string ZE = "ZE";
			}

			public static class AWBLabelSize
			{
				public const string FiveInch = "5In";
				public const string SixInch = "6In";
			}

			public static class PPDCollect
			{
				public const string Prepaid = "P";
				public const string Collect = "C";
			}

			public static class EntitlementCode
			{
				public const string Agent = "A";
				public const string Carrier = "C";
				public const string Split = "S";
			}

			public static class MAWBBillingSellRateModes
			{
				public const string None = "NON";
				public const string CollectOnly = "COL";
				public const string PrepaidOnly = "PPD";
				public const string Both = "BTH";
			}

			public static class RateLineUQ
			{
				public const string Kilos = "K";
				public const string Pounds = "L";
			}

			public static class RateClass
			{
				public const string MinimumCharge = "M";
				public const string NormalCharge = "N";
				public const string QuantityRate = "Q";
				public const string BasicCharge = "B";
				public const string RatePerKilogram = "K";
				public const string SpecificCommodityRate = "C";
				public const string ClassRateReduction = "R";
				public const string ClassRateSurcharge = "S";
				public const string UnitLoadDeviceBasicCharge = "U";
				public const string UnitLoadDeviceAdditionalCharge = "E";
				public const string UnitLoadDeviceAdditionalInformation = "X";
				public const string UnitLoadDeviceDiscount = "Y";
				public const string WeightIncrease = "W";
				public const string InternationalPriorityService = "P";
			}

			public static class PaperTypes
			{
				public const string Iata = "IA";
				public const string IataOld = "IO";
				public const string Traxon = "TR";
				public const string Letter = "LE";
			}

			public static class NatureAndQtyOfGoodsTypes
			{
				public const string GoodsDescription = "G";
				public const string Consolidation = "C";
				public const string Dimensions = "D";
				public const string Volume = "V";
				public const string ULDNumber = "U";
				public const string ShippersLoadAndCount = "S";
				public const string HarmonisedCommodityCode = "H";
				public const string CountryOfGoodsOrigin = "O";
				public const string LithiumBattery = "L";
			}

			public static class LithiumBatteryTypes
			{
				public static class Codes
				{
					public const string PI965 = "PI965";
					public const string PI966 = "PI966";
					public const string PI967 = "PI967";
					public const string PI968 = "PI968";
					public const string PI969 = "PI969";
					public const string PI970 = "PI970";
					public const string LMB = "LMB";
				}

				public static class Descriptions
				{
					public static MultilingualString PI965 { get { return ResString.GetMultilingualString("26a7cbf7-716c-cdbd-47be-0d25d210351e", "Lithium ion batteries in compliance with Section II of PI965 CAO"); } }
					public static MultilingualString PI966 { get { return ResString.GetMultilingualString("990f1dec-5fc2-4bb5-b416-8574ed6bf9d3", "Lithium ion batteries in compliance with Section II of PI966"); } }
					public static MultilingualString PI967 { get { return ResString.GetMultilingualString("f8e532a2-4b33-47f3-a243-8486bce628f9", "Lithium ion batteries in compliance with Section II of PI967"); } }
					public static MultilingualString PI968 { get { return ResString.GetMultilingualString("c13e29af-878a-4482-bb79-708d656a5942", "Lithium metal batteries in compliance with Section II of PI968 CAO"); } }
					public static MultilingualString PI969 { get { return ResString.GetMultilingualString("882890a5-327a-49b6-a9a8-60f394e562d0", "Lithium metal batteries in compliance with Section II of PI969"); } }
					public static MultilingualString PI970 { get { return ResString.GetMultilingualString("9366da81-b858-4e5f-9928-1b7cfacc3880", "Lithium metal batteries in compliance with Section II of PI970"); } }
					public static MultilingualString LMB { get { return ResString.GetMultilingualString("2eb7a05f-e785-4266-a5b0-9ce6332ef618", "Lithium metal batteries - forbidden for transport aboard passenger aircraft"); } }
				}
			}

			public static class AsAgreedTypes
			{
				public static class Codes
				{
					public const string All = "ALL";
					public const string Prepaid = "PPD";
					public const string Collect = "CLT";
					public const string None = "NON";
				}

				public static class Descriptions
				{
					public static MultilingualString All { get { return ResString.GetMultilingualString("9de492ed-f174-4bdd-af90-4a8c36e7feea", "Replace All Charges with ‘{0}’", "As Agreed"); } }
					public static MultilingualString Prepaid { get { return ResString.GetMultilingualString("a6377c14-a09c-4f59-ad2b-f52d7e3771bb", "Replace Prepaid Charges with ‘{0}’", "As Agreed"); } }
					public static MultilingualString Collect { get { return ResString.GetMultilingualString("9f74fa11-2dd1-4816-8967-39dbb6938d39", "Replace Collect Charges with ‘{0}’", "As Agreed"); } }
					public static MultilingualString None { get { return ResString.GetMultilingualString("02c2a693-c0ef-4518-beaa-7d6007540c27", "Do not replace rate amounts with ‘{0}’", "As Agreed"); } }
				}
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "NatureAndQtyOfGoods Details")]
			public static class NatureAndQtyOfGoodsDetails
			{
				public const string ConsolAsPerList = "Consolidation as per attached list";
			}

			public static class AWBAddressNotAllowed
			{
				public const string ToBeAnnounced = "TBA";
			}
		}

		public static class RatingDateFilterTypes
		{
			public static class Codes
			{
				public const string Default = "DEF";
				public const string Standard = "STD";
				public const string Departure = "DEP";
				public const string Arrival = "ARR";
				public const string Custom = "CUS";
			}

			public static class Descriptions
			{
				public static MultilingualString Default { get { return ResString.GetMultilingualString("B4AE444F-769B-4856-A2EE-6B1B232C2154", "Default From Registry"); } }
				public static MultilingualString Standard { get { return ResString.GetMultilingualString("0e3ae319-e11d-46eb-93b5-3650aa3f6665", "Use Departure Date for Loading, Origin, Freight and Insurance charges, and Arrival Date for Unloading and Destination charges"); } }
				public static MultilingualString Departure { get { return ResString.GetMultilingualString("97e02364-f9a2-4a62-a8a7-7756ec69f2ea", "Use Departure Date for all charges"); } }
				public static MultilingualString Arrival { get { return ResString.GetMultilingualString("70ce8b61-2733-43ee-b0c9-61f3173cb6f5", "Use Arrival Date for all charges"); } }
				public static MultilingualString Custom { get { return ResString.GetMultilingualString("b8ee4ac6-199a-4b17-b0f9-c2479b5e4623", "Use Customized setup as specified"); } }
			}
		}

		public static class CargoTypes
		{
			public const string General = "GEN";
			public const string Reefer = "REF";
			public const string Frozen = "FRO";
			public const string Chiller = "CHI";
			public const string Hazardous = "HAZ";
		}

		// REMEMBER TO UPDATE Contracts in OrgSupBuyLinkTrnMode
		public static class ContainerModes
		{
			public const string All = "ALL";
			public const string AgentConsol = "CON";
			public const string AIR = "AIR";
			public const string BreakBulk = "BBK";
			public const string Bulk = "BLK";
			public const string Liquid = "LQD";
			public const string BuyersConsol = "BCN";
			public const string ShippersConsol = "SCN";
			public const string FCL = "FCL";
			public const string FTL = "FTL";
			public const string Groupage = "GRP";
			public const string LCL = "LCL";
			public const string Loose = "LSE"; //Loose replaces Air
			public const string LTL = "LTL";
			public const string Mail = "MAI";
			public const string OnBoardCourier = "OBC";
			public const string Other = "OTH";
			public const string ULD = "ULD";
			public const string Unaccompanied = "UNA";
			public const string FreightAllKind = "FAK";
			public const string FCLMixedShipper = "FCX";
			public const string Empty = "EMP";
			public const string RollOnRollOff = "ROR";
			public const string Combination = "COM";
			public const string Containerised = "CNT";
			public const string NonContainerised = "NCT";

			public static bool IsContainerised(string code)
			{
				return
					code == Core.Constants.ContainerModes.BuyersConsol
					|| code == Core.Constants.ContainerModes.ShippersConsol
					|| code == Core.Constants.ContainerModes.FCL
					|| code == Core.Constants.ContainerModes.Groupage
					|| code == Core.Constants.ContainerModes.LCL
					|| code == Core.Constants.ContainerModes.FCLMixedShipper
					|| code == Core.Constants.ContainerModes.Combination
					|| code == Core.Constants.ContainerModes.FreightAllKind
					|| code == Core.Constants.ContainerModes.Containerised;
			}

			public static bool IsLCLType(string code)
			{
				return
					code == Constants.ContainerModes.LCL
					|| code == Constants.ContainerModes.Groupage
					|| code == Constants.ContainerModes.Loose
					|| code == Constants.ContainerModes.LTL
					|| code == Constants.ContainerModes.AIR
					|| code == Constants.ContainerModes.BuyersConsol
					|| code == Constants.ContainerModes.ShippersConsol
					|| code == Constants.ContainerModes.FreightAllKind
					|| code == Constants.ContainerModes.ULD;
			}

			public static bool IsFCLType(string code)
			{
				return FCLTypes.Contains(code);
			}

			public static string[] FCLTypes
			{
				get
				{
					if (fclTypes == null)
					{
						fclTypes = new[]
						{
							Constants.ContainerModes.FCL,
							Constants.ContainerModes.ULD,
							Constants.ContainerModes.Bulk,
							Constants.ContainerModes.Liquid
						};
					}

					return fclTypes;
				}
			}

			public static string[] LCLTypes
			{
				get
				{
					if (lclTypes == null)
					{
						lclTypes = new[]
						{
							Constants.ContainerModes.LCL,
							Constants.ContainerModes.Groupage,
							Constants.ContainerModes.Loose,
							Constants.ContainerModes.LTL,
							Constants.ContainerModes.AIR,
							Constants.ContainerModes.FreightAllKind
						};
					}

					return lclTypes;
				}
			}

			[ThreadStatic]
			static string[] fclTypes;

			[ThreadStatic]
			static string[] lclTypes;
		}

		public static class RatingContractTypes
		{
			public const string Provider = "PRO";
			public const string Client = "CLI";
		}

		public static class CartageLegDispatchStatusList
		{
			public static class Codes
			{
				public const string Delivered = "DL";
				public const string Delivering = "DG";
				public const string Futile = "FU";
				public const string PickedUp = "PU";
				public const string PickingUp = "PG";
				public const string Rejected = "RJ";
				public const string Runsheet = "RS";
				public const string WIP = "WIP";
				public const string NotStarted = "NS";
			}

			public static class Descriptions
			{
				public static MultilingualString Delivered { get { return ResString.GetMultilingualString("0564e355-7f3a-460e-b680-0490d1013cbb", "Delivered"); } }
				public static MultilingualString Delivering { get { return ResString.GetMultilingualString("7d68825d-38f7-4e61-ade6-0e84fdb7a969", "Delivering"); } }
				public static MultilingualString Futile { get { return ResString.GetMultilingualString("552ed82f-8d10-42f4-a10c-016a67641e31", "Futile"); } }
				public static MultilingualString PickedUp { get { return ResString.GetMultilingualString("ef27cdf7-5946-4c5a-8fe8-f811e0a6ebed", "Picked up"); } }
				public static MultilingualString PickingUp { get { return ResString.GetMultilingualString("7a685cbc-b7cf-409c-a187-868b8d31f7b9", "Picking up"); } }
				public static MultilingualString Rejected { get { return ResString.GetMultilingualString("c0cea067-2f68-43df-903f-f2c3a6a7da09", "Rejected"); } }
				public static MultilingualString Runsheet { get { return ResString.GetMultilingualString("3034fc51-fdbb-43d6-a0e3-d7a6a7e58c16", "Run sheet"); } }
				public static MultilingualString WIP { get { return ResString.GetMultilingualString("37c86961-7e2e-4499-bb29-926d330e92a1", "Work In Progress"); } }
				public static MultilingualString NotStarted { get { return ResString.GetMultilingualString("8d7feb27-2cfa-48d5-81fe-2b0136f95297", "Not Started"); } }
			}
		}

		public static class ContainerModeDescriptions
		{
			public static MultilingualString All { get { return ResString.GetMultilingualString("765EAFDF-9F6F-41D8-9468-839538FFEDFD", "All Container Modes"); } }
			public static MultilingualString AgentConsol { get { return ResString.GetMultilingualString("FC19F9ED-0559-40D5-B73A-70E49614CDFD", "Agent Consolidation"); } }
			public static MultilingualString AIR { get { return ResString.GetMultilingualString("6D3370CC-4DE3-4F6C-BCE7-9E4033DFE149", "Air"); } }
			public static MultilingualString BreakBulk { get { return ResString.GetMultilingualString("98A9CC06-A07D-43D6-93CD-737D1C9C5CA7", "Break Bulk"); } }
			public static MultilingualString Bulk { get { return ResString.GetMultilingualString("1873C4CA-B9FD-4636-BCC3-FD26D69BAD89", "Bulk"); } }
			public static MultilingualString Liquid { get { return ResString.GetMultilingualString("5075D083-1177-4257-BA5C-76D2B699972F", "Liquid"); } }
			public static MultilingualString BuyersConsol { get { return ResString.GetMultilingualString("F4463DCD-048C-487D-8F28-62420A7B269B", "Buyer's Consolidation"); } }
			public static MultilingualString ShippersConsol { get { return ResString.GetMultilingualString("52190c88-f562-43bc-8149-ed53221f39ae", "Shipper's Consolidation"); } }
			public static MultilingualString FCL { get { return ResString.GetMultilingualString("637DF454-06AB-4489-A161-49C581F46ADF", "Full Container Load"); } }
			public static MultilingualString FTL { get { return ResString.GetMultilingualString("50868F39-87DA-4F18-9788-91C413542FA5", "Full Truck Load"); } }
			public static MultilingualString Groupage { get { return ResString.GetMultilingualString("E53D5AF7-DA37-4F8A-944A-9FC20534C200", "Groupage / Freight All Kinds"); } }
			public static MultilingualString LCL { get { return ResString.GetMultilingualString("A121384F-4196-49C3-AD48-B24725207765", "Less Container Load"); } }
			public static MultilingualString Loose { get { return ResString.GetMultilingualString("18B0ACF4-73D5-4ADD-B6E7-7CD4D5E4BF1B", "Loose"); } } //Loose replaces Air
			public static MultilingualString LTL { get { return ResString.GetMultilingualString("B36D2624-ACA6-491D-B08F-9024269A2837", "Less Truck Load"); } }
			public static MultilingualString Mail { get { return ResString.GetMultilingualString("5038D31B-2E6A-4094-9672-8484099D8D3A", "Mail"); } }
			public static MultilingualString OnBoardCourier { get { return ResString.GetMultilingualString("A39DF1EC-3E17-4675-8B26-F21D5E8F9A12", "On Board Courier"); } }
			public static MultilingualString Other { get { return ResString.GetMultilingualString("0095E6AC-6393-498D-9DA9-876A0C0D4688", "Other"); } }
			public static MultilingualString ULD { get { return ResString.GetMultilingualString("594002F7-4E31-448F-AC4E-6D31F9F3393D", "Unit Load Device"); } }
			public static MultilingualString Unaccompanied { get { return ResString.GetMultilingualString("0011C97C-74A6-4625-9686-D30F990936D6", "Unaccompanied"); } }
			public static MultilingualString FreightAllKind { get { return ResString.GetMultilingualString("1AAFE75B-94BE-486E-8D8E-07043BA526EC", "Freight All Kinds"); } }
			public static MultilingualString FCLMixedShipper { get { return ResString.GetMultilingualString("FA5A6498-83BB-4CE5-B716-59F0A9730BF0", "FCL Mixed Shipper"); } }
			public static MultilingualString Empty { get { return ResString.GetMultilingualString("7C0FDEC5-0E8E-4483-87CB-EA592BD5C07E", "Empty"); } }
			public static MultilingualString RollOnRollOff { get { return ResString.GetMultilingualString("C40D4B42-1E3C-487A-AD57-0A5910DF1BCC", "Roll On/Roll Off"); } }
			public static MultilingualString Combination { get { return ResString.GetMultilingualString("40DCFC53-E59C-45B7-9253-C8095E58FA7B", "Combination"); } }
			public static MultilingualString Containerised { get { return ResString.GetMultilingualString("4A6F3802-A958-498D-8E04-F88C1292067C", "Containerized"); } }
			public static MultilingualString NonContainerised { get { return ResString.GetMultilingualString("9C266C50-8373-4C87-8ADB-4FD43A208FBF", "Non-Containerized"); } }

			public static MultilingualString GetDescription(string containerMode)
			{
				switch (containerMode)
				{
					case ContainerModes.All:
						return ContainerModeDescriptions.All;
					case ContainerModes.AgentConsol:
						return ContainerModeDescriptions.AgentConsol;
					case ContainerModes.AIR:
						return ContainerModeDescriptions.AIR;
					case ContainerModes.BreakBulk:
						return ContainerModeDescriptions.BreakBulk;
					case ContainerModes.Bulk:
						return ContainerModeDescriptions.Bulk;
					case ContainerModes.Liquid:
						return ContainerModeDescriptions.Liquid;
					case ContainerModes.BuyersConsol:
						return ContainerModeDescriptions.BuyersConsol;
					case ContainerModes.ShippersConsol:
						return ContainerModeDescriptions.ShippersConsol;
					case ContainerModes.FCL:
						return ContainerModeDescriptions.FCL;
					case ContainerModes.FTL:
						return ContainerModeDescriptions.FTL;
					case ContainerModes.Groupage:
						return ContainerModeDescriptions.Groupage;
					case ContainerModes.LCL:
						return ContainerModeDescriptions.LCL;
					case ContainerModes.Loose:
						return ContainerModeDescriptions.Loose;
					case ContainerModes.LTL:
						return ContainerModeDescriptions.LTL;
					case ContainerModes.Mail:
						return ContainerModeDescriptions.Mail;
					case ContainerModes.OnBoardCourier:
						return ContainerModeDescriptions.OnBoardCourier;
					case ContainerModes.Other:
						return ContainerModeDescriptions.Other;
					case ContainerModes.ULD:
						return ContainerModeDescriptions.ULD;
					case ContainerModes.Unaccompanied:
						return ContainerModeDescriptions.Unaccompanied;
					case ContainerModes.FreightAllKind:
						return ContainerModeDescriptions.FreightAllKind;
					case ContainerModes.FCLMixedShipper:
						return ContainerModeDescriptions.FCLMixedShipper;
					case ContainerModes.Empty:
						return ContainerModeDescriptions.Empty;
					case ContainerModes.RollOnRollOff:
						return ContainerModeDescriptions.RollOnRollOff;
					case ContainerModes.Combination:
						return ContainerModeDescriptions.Combination;
					case ContainerModes.Containerised:
						return ContainerModeDescriptions.Containerised;
					case ContainerModes.NonContainerised:
						return ContainerModeDescriptions.NonContainerised;
					default:
						return (NoResString)"";
				}
			}
		}

		public static class ContainerOwnership
		{
			public static class Codes
			{
				public const string CarrierOwned = "CAR";
				public const string ShipperOwned = "SHP";
				public const string Leased = "LEA";
			}

			public static class Descriptions
			{
				public static MultilingualString Leased { get { return ResString.GetMultilingualString("eeae8dab-01f9-410e-9cff-c93ff13ea347", "Leased"); } }
				public static MultilingualString CarrierOwned { get { return ResString.GetMultilingualString("74cbf5ba-95cf-4d41-8468-ca41f2433040", "Carrier Owned"); } }
				public static MultilingualString ShipperOwned { get { return ResString.GetMultilingualString("8dca349f-cc8e-46ad-ae76-d694678998de", "Shipper Owned"); } }
			}
		}

		public static class ContainerSealParties
		{
			public static class Codes
			{
				public const string CarrierShippingLine = "CAR";
				public const string ConsignorShipper = "CRD";
				public const string Terminal = "CTO";
				public const string Customs = "CUS";
				public const string Quarantine = "QRT";
			}

			public static class Descriptions
			{
				public static MultilingualString CarrierShippingLine { get { return ResString.GetMultilingualString("378a78c3-84e7-46da-9d37-c96829eff4cd", "Carrier/Shipping Line"); } }
				public static MultilingualString ConsignorShipper { get { return ResString.GetMultilingualString("5966eb12-e282-4cdf-b62d-2229ec69eca4", "Consignor/Shipper"); } }
				public static MultilingualString Terminal { get { return ResString.GetMultilingualString("5023bc51-0d89-4dd7-ab54-c62b51e02985", "Terminal"); } }
				public static MultilingualString Customs { get { return ResString.GetMultilingualString("7124d884-6089-4378-bb8c-4a8dbc98f2e4", "Customs"); } }
				public static MultilingualString Quarantine { get { return ResString.GetMultilingualString("fb3cc643-150a-4811-ba27-85ace9e8b1d7", "Quarantine"); } }
			}
		}

		public static class ContainerTypes
		{
			public const string Refrigerated = "RFG";
			public const string DryStorage = "DRY";
			public const string OpenTop = "TOP";
			public const string FlatRack = "FLT";
			public const string Bolster = "BLS";
			public const string Tank = "TNK";
			public const string Other = "OTH";
			public const string MAFI = "MAF";
			public const string AircraftPallet = "APL";
			public const string AircraftContainer = "ACN";
			public const string AutomobileTransportEquipment = "ATE";
			public const string AircraftEngineTransportEquipment = "AET";
			public const string FireResistantContainer = "FRC";
			public const string CattleStalls = "CST";
			public const string HorseStalls = "HST";
			public const string ThermalAircraftContainer = "TAC";
		}

		public static class HarmonizedCodes
		{
			public const string NCMCode = "NCM";
			public const string HSCode = "HS";
		}

		public static class ContainerMarking
		{
			public const string NoMarks = "N/M";
		}

		public static class ContainerTypeDescriptions
		{
			public static MultilingualString Refrigerated { get { return ResString.GetMultilingualString("608F4ADC-CBF0-456B-9035-F597678485DA", "Refrigerated"); } }
			public static MultilingualString DryStorage { get { return ResString.GetMultilingualString("A53BDCFC-54AE-403E-AB4A-87B833B73455", "Dry Storage"); } }
			public static MultilingualString OpenTop { get { return ResString.GetMultilingualString("C0785EE8-E622-4546-8AD9-9095006D8DCD", "Open Top"); } }
			public static MultilingualString FlatRack { get { return ResString.GetMultilingualString("6A204D04-06D0-454D-8C1D-9087AB8506E8", "Flat Rack"); } }
			public static MultilingualString Bolster { get { return ResString.GetMultilingualString("7BB325C6-83C8-4E54-BC4A-92C2278EBD6B", "Bolster"); } }
			public static MultilingualString Tank { get { return ResString.GetMultilingualString("6F2E8CF9-3036-480E-99E1-DE8B611E26E1", "Tank"); } }
			public static MultilingualString Other { get { return ResString.GetMultilingualString("1F7FD4FF-D8F4-44FD-A695-2F50F32A0143", "Other"); } }
			public static MultilingualString MAFI { get { return ResString.GetMultilingualString("D64D34AB-3E62-4588-A741-0421B73DD9C5", "MAFI"); } }
			public static MultilingualString AircraftPallet { get { return ResString.GetMultilingualString("E7A4646F-4A80-4349-B51B-18B9D5190389", "Aircraft pallet"); } }
			public static MultilingualString AircraftContainer { get { return ResString.GetMultilingualString("ACAA9AAC-2B3E-4BB8-A400-DFF266B9B3AB", "Aircraft container"); } }
			public static MultilingualString AutomobileTransportEquipment { get { return ResString.GetMultilingualString("14DB6F7F-B1E9-46EB-857B-15AC83AE69A1", "Automobile transport equipment"); } }
			public static MultilingualString AircraftEngineTransportEquipment { get { return ResString.GetMultilingualString("9CE854C9-797E-4E9E-A6CB-78DEC81A1095", "Aircraft Engine Transport equipment"); } }
			public static MultilingualString FireResistantContainer { get { return ResString.GetMultilingualString("99243CF9-13ED-4A13-9602-E65E4137EC13", "Fire Resistant Container"); } }
			public static MultilingualString CattleStalls { get { return ResString.GetMultilingualString("29A631AF-D72B-4C37-B3E2-3E3AECE6D85B", "Cattle stalls"); } }
			public static MultilingualString HorseStalls { get { return ResString.GetMultilingualString("C0D094F6-77EE-4528-ADA0-8D6F4A877406", "Horse stalls"); } }
			public static MultilingualString ThermalAircraftContainer { get { return ResString.GetMultilingualString("934F73B9-5083-4D71-914E-B74B66454B52", "Thermal aircraft container"); } }
		}

		public static class DeliveryModes
		{
			public static class Codes
			{
				public const string CFS_CFS = "CFS/CFS";
				public const string CY_CY = "CY/CY";
				public const string CY_CFS = "CY/CFS";
				public const string CFS_CY = "CFS/CY";
			}

			public static class Descriptions
			{
				public static MultilingualString CFS_CFS { get { return ResString.GetMultilingualString("48CA509D-986A-40CA-80E8-F14619B8EE7B", "CFS/CFS"); } }
				public static MultilingualString CY_CY { get { return ResString.GetMultilingualString("BC151959-5C10-420F-B863-B56CFAD42CE3", "CY/CY"); } }
				public static MultilingualString CY_CFS { get { return ResString.GetMultilingualString("CC2C411C-2046-43E4-8B11-87F2A434E8D7", "CY/CFS"); } }
				public static MultilingualString CFS_CY { get { return ResString.GetMultilingualString("A7BDF678-38CD-471D-B1A5-9F94FA1A4A07", "CFS/CY"); } }
			}
		}

		public static class HBLDeliveryModes
		{
			public static class Codes
			{
				public const string ARPT_ARPT = "ARPT/ARPT";
				public const string ARPT_DOOR = "ARPT/DOOR";
				public const string ARPT_CFS = "ARPT/CFS";
				public const string CFS_ARPT = "CFS/ARPT";
				public const string CFS_CFS = "CFS/CFS";
				public const string CFS_CY = "CFS/CY";
				public const string CFS_DOOR = "CFS/DOOR";
				public const string CY_CY = "CY/CY";
				public const string CY_CFS = "CY/CFS";
				public const string CY_DOOR = "CY/DOOR";
				public const string DOOR_ARPT = "DOOR/ARPT";
				public const string DOOR_DOOR = "DOOR/DOOR";
				public const string DOOR_CFS = "DOOR/CFS";
				public const string DOOR_CY = "DOOR/CY";
				public const string DOOR_PORT = "DOOR/PORT";
				public const string PORT_DOOR = "PORT/DOOR";
				public const string PORT_PORT = "PORT/PORT";
			}

			public static class Descriptions
			{
				public static MultilingualString CFS_CFS { get { return ResString.GetMultilingualString("1b024fe2-a42f-4e83-916c-c4d383791e5d", "CFS/CFS"); } }
				public static MultilingualString CFS_CY { get { return ResString.GetMultilingualString("39eaf2dc-6f51-42dd-80fd-239e7d087562", "CFS/CY"); } }
				public static MultilingualString CFS_DOOR { get { return ResString.GetMultilingualString("e2acde83-6b4c-45ef-9d41-4f1129ea4d55", "CFS/DOOR"); } }
				public static MultilingualString CY_CY { get { return ResString.GetMultilingualString("989e7f68-9284-4a30-813f-06f83f37ce43", "CY/CY"); } }
				public static MultilingualString CY_CFS { get { return ResString.GetMultilingualString("7902ae05-b6f7-4b63-88a7-7f3260d34fc6", "CY/CFS"); } }
				public static MultilingualString CY_DOOR { get { return ResString.GetMultilingualString("7f496c7a-2690-4f4f-ab77-e32fa406009e", "CY/DOOR"); } }
				public static MultilingualString DOOR_DOOR { get { return ResString.GetMultilingualString("d35cc022-a5a8-4894-857c-2d1e9ad25300", "DOOR/DOOR"); } }
				public static MultilingualString DOOR_CFS { get { return ResString.GetMultilingualString("1e62b7a0-c4ab-40a1-8d84-ed646465d8a5", "DOOR/CFS"); } }
				public static MultilingualString DOOR_CY { get { return ResString.GetMultilingualString("71026917-01c9-4a43-9364-2c0a33f35573", "DOOR/CY"); } }
				public static MultilingualString PORT_PORT { get { return ResString.GetMultilingualString("e25ab80b-48b0-4978-8201-e862963d519e", "PORT/PORT"); } }
				public static MultilingualString DOOR_PORT { get { return ResString.GetMultilingualString("63d0bdad-e78b-4657-95f3-2c8aa949a274", "DOOR/PORT"); } }
				public static MultilingualString PORT_DOOR { get { return ResString.GetMultilingualString("082fce7a-e509-4523-bfd8-9869bee95f27", "PORT/DOOR"); } }
				public static MultilingualString ARPT_DOOR { get { return ResString.GetMultilingualString("9FD6FB46-E15F-4599-B3E8-7BD2318965F4", "AIRPORT/DOOR"); } }
				public static MultilingualString ARPT_CFS { get { return ResString.GetMultilingualString("E085094A-C92B-4B85-8CC2-2AFE39A63A10", "AIRPORT/CFS"); } }
				public static MultilingualString DOOR_ARPT { get { return ResString.GetMultilingualString("CC2F5B9F-C118-40E2-9E45-F69BBEDA1601", "DOOR/AIRPORT"); } }
				public static MultilingualString CFS_ARPT { get { return ResString.GetMultilingualString("321CA237-6E96-471D-89F9-A2BF3154A185", "CFS/AIRPORT"); } }
				public static MultilingualString ARPT_ARPT { get { return ResString.GetMultilingualString("E317E33F-634B-4290-B4C7-EF92F98EAB35", "AIRPORT/AIRPORT"); } }
			}
		}

		public static class DeliveryTypes
		{
			public const string DirectToCNE = "DirectToCNE";
		}

		public static class DocumentEngine
		{
			public static class EmailParsing
			{
				public const string StartTag = "(*";
				public const string EndTag = "*)";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Legacy Documents constant")]
			public static class MenuPaths
			{
				public const string LegacyDocuments = "Legacy Documents";
			}
		}

		public static class EmailFormat
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Email Field Codes")]
			public static class EmailFieldCodes
			{
				public const string CompanyBrandName = "Company/Brand Name";
				public const string CompanyAddress = "Company Address";
				public const string CompanyPhone = "Company Phone Number";
				public const string CompanyFax = "Company Fax Number";
				public const string CompanyEmail = "Company Email Address";
				public const string CompanyWeb = "Company Web Address";

				public const string BranchName = "Branch Name";
				public const string BranchCode = "Branch Code";
				public const string BranchAddress = "Branch Address";
				public const string BranchPhone = "Branch Phone Number";
				public const string BranchFax = "Branch Fax Number";
				public const string BranchEmail = "Branch Email Address";
				public const string BranchWeb = "Branch Web Address";

				public const string UserName = "User Full Name";
				public const string UserTitle = "User Title";
				public const string UserWorkPhone = "User Work Phone Number";
				public const string UserFax = "User Fax Number";
				public const string UserMobile = "User Mobile Number";
				public const string UserEmail = "User Email Address";

				public const string DocumentName = "Document Name";
				public const string ScheduledTaskDescription = "Scheduled Task Description";
			}

			public static class EmailFieldDescriptions
			{
				public static MultilingualString CompanyBrandName => ResString.GetMultilingualString("f46090e3-cabf-4abc-9a8a-1d93f7b161bf", "Company/Brand Name");
				public static MultilingualString CompanyAddress => ResString.GetMultilingualString("9F87E6CA-6F95-4F28-B675-45B07DF5DE42", "Company Address");
				public static MultilingualString CompanyPhone => ResString.GetMultilingualString("13A3903C-A6D0-4491-A4B4-43A167C04AAC", "Company Phone Number");
				public static MultilingualString CompanyFax => ResString.GetMultilingualString("A3F24BEE-33F5-4B61-9754-B1BDE91BBE8A", "Company Fax Number");
				public static MultilingualString CompanyEmail => ResString.GetMultilingualString("F73A0964-87ED-4580-B455-BACC7C99792C", "Company Email Address");
				public static MultilingualString CompanyWeb => ResString.GetMultilingualString("B5375EA4-2C93-4D72-8509-7E4C8D8D3A6E", "Company Web Address");

				public static MultilingualString BranchName => ResString.GetMultilingualString("A7D3AC55-3EEA-417E-A24C-D9726F0F839E", "Branch Name");
				public static MultilingualString BranchCode => ResString.GetMultilingualString("B86CE09D-BEAA-4B46-B93E-C281EA625CBB", "Branch Code");
				public static MultilingualString BranchAddress => ResString.GetMultilingualString("DE8D953C-920E-452F-B26A-87D9F4D632F4", "Branch Address");
				public static MultilingualString BranchPhone => ResString.GetMultilingualString("ED6D9F46-4976-49AE-B864-070DCB749499", "Branch Phone Number");
				public static MultilingualString BranchFax => ResString.GetMultilingualString("AC69120B-091B-45B1-82ED-9FCC5CDB5593", "Branch Fax Number");
				public static MultilingualString BranchEmail => ResString.GetMultilingualString("8C64D0C4-B967-4352-9D3F-259192BE22DF", "Branch Email Address");
				public static MultilingualString BranchWeb => ResString.GetMultilingualString("FC548D97-E382-4E83-9E21-27FA3D2E3B4C", "Branch Web Address");

				public static MultilingualString UserName => ResString.GetMultilingualString("B573FC53-7741-4703-9AAB-044466687385", "User Full Name");
				public static MultilingualString UserTitle => ResString.GetMultilingualString("A0FC07ED-CE59-4F23-B205-5E755210E613", "User Title");
				public static MultilingualString UserWorkPhone => ResString.GetMultilingualString("50AA4FEA-314E-4AFC-8256-DEAD3F7CE230", "User Work Phone Number");
				public static MultilingualString UserFax => ResString.GetMultilingualString("303E02ED-9ECD-4AF1-B9D4-B0C029786130", "User Fax Number");
				public static MultilingualString UserMobile => ResString.GetMultilingualString("1D88D49C-4E3D-4B8C-B121-81BD0449DA62", "User Mobile Number");
				public static MultilingualString UserEmail => ResString.GetMultilingualString("3DBAE9FF-4586-4092-9871-17838463A057", "User Email Address");

				public static MultilingualString DocumentName => ResString.GetMultilingualString("419D0A18-F375-458D-A815-57A5B6CE726B", "Document Name");
			}
		}

		public static class JobInvoicingSellerType
		{
			public const string Agent = "AGN";
			public const string BillTo = "BIL";
		}

		public static class JobInvoicingDefaultDepartmentConsolType
		{
			public const string All = "ALL";
			public const string NoConsol = "NCN";
		}

		public static class ContactNotifyModes
		{
			public const string Email = "EML";
			public const string Electronic = "ELC";
			public const string EPrint = "EPR";
			public const string Fax = "FAX";
			public const string Print = "PRN";
			public const string Ftp = "FTP";
			public const string DoNotDeliver = "DND";
			public const string EDoc = "EDC";

			public static IEnumerable<string> All => typeof(ContactNotifyModes).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(x => x.IsLiteral && !x.IsInitOnly)
				.Select(x => x.GetRawConstantValue().ToString());
		}

		public static class ContactNotifyModeDescriptions
		{
			public static MultilingualString Email => ResString.GetMultilingualString("79f129da-8330-4c35-877f-172159993368", "E-Mail");
			public static MultilingualString Fax => ResString.GetMultilingualString("be1343ed-4fa0-41e5-b610-48d046aaad5e", "Fax");
			public static MultilingualString Print => ResString.GetMultilingualString("3972312b-6803-49bc-b36b-8185beaf6c74", "Print");
			public static MultilingualString EPrint => ResString.GetMultilingualString("c6d00ec8-d0d1-4c90-8627-8a77324e6dc5", "ePrint");
		}

		public static class EmailFromAddressTypes
		{
			public static class Codes
			{
				public const string Default = "DEF";
				public const string Main = "MAI";
			}

			public static class Descriptions
			{
				public static MultilingualString Default { get { return ResString.GetMultilingualString("6C838233-6F6C-480F-B1CC-9FA3D2BAF685", "Default"); } }
				public static MultilingualString Main { get { return ResString.GetMultilingualString("8F4BA80A-6AD0-4FD2-8212-78AECCCB32EF", "Main"); } }
			}
		}

		public static class EmailTo
		{
			public const string NoEmails = "NOE";
			public const string StaffMember = "ESM";
			public const string NominatedGroup = "ENG";
			public const string StaffMemberAndNominatedGroup = "ESG";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Department Activity Types")]
		public static class DepartmentActivityTypes
		{
			public const string Customs = "Customs";
			public const string DepotCFS = "Depot CFS";
			public const string Forwarding = "Forwarding";
			public const string Linehaul = "Linehaul";
			public const string Cartage = "Cartage";
			public const string Shipping = "Ships Agency";
			public const string Warehouse = "Warehouse";
			public const string Gateway = "Gateway";
			public const string Miscellaneous = "Miscellaneous";
		}

		public static class ContainerPackingMode
		{
			public const string Import = "UNP";
			public const string Export = "PAK";
			public const string All = "ALL";
		}

		public static class ContainerGrossWeightVerificationTypes
		{
			public static class Codes
			{
				public const string NotVerified = "NON";
				public const string Method1Container = "CNT";
				public const string Method2Packages = "PKG";
				public const string WeightAtTerminal = "WTA";
				public const string RationalMethod = "RTL";
				public const string NotRequired = "NRQ";
			}

			public static class Descriptions
			{
				public static MultilingualString NotVerified { get { return ResString.GetMultilingualString("B69F9642-8622-452E-A33F-5FB3FB531907", "Not Verified"); } }
				public static MultilingualString Method1Container { get { return ResString.GetMultilingualString("A1AC4F61-B79A-4F4F-BBC6-FDE524357BE1", "Method 1 - Container"); } }
				public static MultilingualString Method2Packages { get { return ResString.GetMultilingualString("A0BADDD6-454F-4A37-B92B-86BC2F49A3E9", "Method 2 - Packages"); } }
				public static MultilingualString WeightAtTerminal { get { return ResString.GetMultilingualString("480d1973-ad07-48a6-aafe-e97a2e735c2b", "Weight at Terminal"); } }
				public static MultilingualString RationalMethod { get { return ResString.GetMultilingualString("70d732e9-6ec1-46ac-9e99-a0242109d8bf", "Package by Exporter/Tare by Carrier"); } }
				public static MultilingualString NotRequired { get { return ResString.GetMultilingualString("fa9f82a6-432e-46ee-bbea-f3eea02c0145", "Not Required"); } }
			}
		}

		public static class ContainerGrossWeightVerificationStatuses
		{
			public static class Codes
			{
				public const string NotVerified = "NON";
				public const string NotRequired = "NRQ";
				public const string NotSent = "NST";
				public const string Sent = "SNT";
				public const string AmendedNotSent = "AMN";
				public const string Acknowledged = "ACK";
				public const string Rejected = "REJ";
				public const string Accepted = "ACC";
				public const string WithdrawSent = "WDW";
				public const string WithdrawAcknowledged = "WAC";
				public const string WithdrawRejected = "WRJ";
			}

			public static class Descriptions
			{
				public static MultilingualString NotVerified => ResString.GetMultilingualString("58142015-e708-4504-a045-40eeae46f0de", "Not Verified");
				public static MultilingualString NotRequired => ResString.GetMultilingualString("966fed4d-6293-41a8-aaf7-93af2c3043d9", "Not Required");
				public static MultilingualString NotSent => ResString.GetMultilingualString("cad5038b-a0a7-4b4e-9065-6237cc7a7b24", "Not Sent");
				public static MultilingualString Sent => ResString.GetMultilingualString("e549d6d7-2a66-4685-82f4-2d9d27f88095", "Sent");
				public static MultilingualString AmendedNotSent => ResString.GetMultilingualString("90fd988b-399e-457b-b858-6eb132059120", "Amended Not Sent");
				public static MultilingualString Acknowledged => ResString.GetMultilingualString("216316a0-6065-495a-a91e-b4e3d94d777c", "Acknowledged");
				public static MultilingualString Rejected => ResString.GetMultilingualString("ecf4b479-6d4e-4fe7-b97c-3a8b67e4d75a", "Rejected");
				public static MultilingualString Accepted => ResString.GetMultilingualString("22190394-c846-4fc7-a14d-f997c44ee9bb", "Accepted");
				public static MultilingualString WithdrawSent => ResString.GetMultilingualString("a0724e31-4dcf-452a-95fe-69be396aa630", "Withdraw Sent");
				public static MultilingualString WithdrawAcknowledged => ResString.GetMultilingualString("ad231711-f320-4c35-bea5-4f7ca5840218", "Withdraw Acknowledged");
				public static MultilingualString WithdrawRejected => ResString.GetMultilingualString("0646536f-c265-402b-83d4-a4a636fe5f2d", "Withdraw Rejected");
			}
		}

		public static class IncoTerms
		{
			public const string ExWorks = "EXW";
			public const string FreeCarrier = "FCA";
			public const string FreeAlongsideShip = "FAS";
			public const string FreeOnBoard = "FOB";
			public const string CostAndFreight = "CFR";
			public const string CostInsuranceAndFreight = "CIF";
			public const string CarriagePaidTo = "CPT";
			public const string CarriageAndInsurancePaidTo = "CIP";
			public const string DeliveredDutyPaid = "DDP";

			// incoterms 2000
			public const string DeliveredAtFrontier = "DAF";
			public const string DeliveredExShip = "DES";
			public const string DeliveredExQuay = "DEQ";
			public const string DeliveredDutyUnpaid = "DDU";

			// incoterms 2010
			public const string DeliveredAtPlace = "DAP";
			public const string DeliveredAtTerminal = "DAT";

			// Incoterms 2020
			public const string FreeCarrierSeller = "FC1";
			public const string FreeCarrierBuyer = "FC2";
			public const string DeliveredAtPlaceUnloaded = "DPU";

			// AU Legacy IncoTerms
			public const string CostAndInsurance = "C&I";
			public const string CostFreightWithAmpersand = "C&F";
			public const string LandedIntoStore = "LIS";
			public const string PackedAtFactory = "PAF";
			public const string UnpackedAtFactory = "UAF";
			public const string UnpackedCostAndFreight = "UCF";
			public const string UnpackedCostInsuranceAndFreight = "UCI";
			public const string UnpackedFreeOnBoard = "UFB";

			// UCC6
			public const string Other = "XXX";

			public static readonly DateTime Incoterms2000EffectiveDate = new DateTime(2000, 1, 1);
			public static readonly DateTime Incoterms2010EffectiveDate = new DateTime(2011, 1, 1);
			public static readonly DateTime Incoterms2020EffectiveDate = new DateTime(2020, 1, 1);

			public static readonly ReadOnlyCollection<string> Incoterms2000 = Array.AsReadOnly(new string[]
			{
				ExWorks, FreeCarrier, FreeAlongsideShip, FreeOnBoard, CostAndFreight, CostInsuranceAndFreight, CarriagePaidTo,
				CarriageAndInsurancePaidTo, DeliveredDutyPaid, DeliveredAtFrontier, DeliveredExShip, DeliveredExQuay, DeliveredDutyUnpaid
			});

			public static readonly ReadOnlyCollection<string> Incoterms2010 = Array.AsReadOnly(new string[]
			{
				ExWorks, FreeCarrier, FreeAlongsideShip, FreeOnBoard, CostAndFreight, CostInsuranceAndFreight, CarriagePaidTo,
				CarriageAndInsurancePaidTo, DeliveredDutyPaid, DeliveredAtTerminal, DeliveredAtPlace
			});

			public static readonly ReadOnlyCollection<string> Incoterms2020 = Array.AsReadOnly(new string[]
			{
				ExWorks, FreeCarrier, FreeCarrierSeller, FreeCarrierBuyer, FreeAlongsideShip, FreeOnBoard, CostAndFreight,
				CostInsuranceAndFreight, CarriagePaidTo, CarriageAndInsurancePaidTo, DeliveredDutyPaid, DeliveredAtPlace, DeliveredAtPlaceUnloaded
			});

			public static string DefaultIncoFromPaymentType(string paymentType)
			{
				switch (paymentType)
				{
					case PaymentType.Collect:
						return FreeOnBoard;

					case PaymentType.Prepaid:
						return CostAndFreight;

					default:
						return string.Empty;
				}
			}

			public static string GetMappedOfficialIncoterm(string incoTermCode)
			{
				return (incoTermCode == FreeCarrierSeller || incoTermCode == FreeCarrierBuyer) ? FreeCarrier : incoTermCode;
			}

			public static class Descriptions
			{
				#region SuppressResourceStringsCheckRegion

				public static MultilingualString ExWorks => ResString.GetMultilingualString("197805C9-4F78-473C-9C7F-4443F0CBB5C9", "Ex Works");
				public static MultilingualString FreeCarrier => ResString.GetMultilingualString("78A6A2B5-49C1-41B1-BA1C-C7ACCD528AB8", "FCA - Free Carrier (seller is responsible for origin, buyer for loading)");
				public static MultilingualString FreeAlongsideShip => ResString.GetMultilingualString("8F27D046-F52D-4AF1-A557-445AB51EA56E", "Free Alongside Ship");
				public static MultilingualString FreeOnBoard => ResString.GetMultilingualString("EA8A7BA8-5FEC-4830-AEAF-F61C9863A1EB", "Free On Board");
				public static MultilingualString CostAndFreight => ResString.GetMultilingualString("D78686E2-4819-498D-A33E-3F9EBC1C70EE", "Cost And Freight");
				public static MultilingualString CostInsuranceAndFreight => ResString.GetMultilingualString("4B2C9B3C-7BC2-41D4-A864-809477E55044", "Cost, Insurance And Freight");
				public static MultilingualString CarriagePaidTo => ResString.GetMultilingualString("D8CE9E4E-15E5-44B7-9D98-B82251DD40B4", "Carriage Paid To");
				public static MultilingualString CarriageAndInsurancePaidTo => ResString.GetMultilingualString("7BEE576D-D7D3-42AB-95D3-650CFAF91E6B", "Carriage and Insurance Paid To");
				public static MultilingualString DeliveredDutyPaid => ResString.GetMultilingualString("BC9D2E53-C843-4722-92DE-DA6F6E3A5585", "Delivered Duty Paid");

				// incoterms 2000
				public static MultilingualString DeliveredAtFrontier => ResString.GetMultilingualString("F7F5DAAA-48BD-4192-A4A2-092D2768DCCC", "Delivered At Frontier");
				public static MultilingualString DeliveredExShip => ResString.GetMultilingualString("A13B4D93-82C2-45B4-AB93-1FB64F095E91", "Delivered Ex Ship");
				public static MultilingualString DeliveredExQuay => ResString.GetMultilingualString("25981140-526D-4576-B3F2-18DCDE910C04", "Delivered Ex Quay");
				public static MultilingualString DeliveredDutyUnpaid => ResString.GetMultilingualString("88B548B9-DBC4-4E58-915B-FBA57C4D0F0F", "Delivered Duty Unpaid");

				// incoterms 2010
				public static MultilingualString DeliveredAtPlace => ResString.GetMultilingualString("A1B99B9D-B5BB-4C0D-B279-6EEBC5DE86E3", "Delivered At Place");
				public static MultilingualString DeliveredAtTerminal => ResString.GetMultilingualString("3597351B-CCD9-47EF-83F8-4833E904DA80", "Delivered At Terminal");

				// Incoterms 2020
				public static MultilingualString FreeCarrierSeller => ResString.GetMultilingualString("92E5E1B6-C424-4DC8-9196-76B530155AAC", "FCA - Free Carrier (seller is responsible for origin and loading)");
				public static MultilingualString FreeCarrierBuyer => ResString.GetMultilingualString("2379F091-816E-4EB5-9CC5-0CFC1A82336E", "FCA - Free Carrier (buyer is responsible for origin and loading)");
				public static MultilingualString DeliveredAtPlaceUnloaded => ResString.GetMultilingualString("ED28FFFF-0E9D-4203-B61F-F8535AD9EFD4", "Delivered at Place Unloaded");

				// UCC6
				public static MultilingualString Other => ResString.GetMultilingualString("4CB2F7C7-EDCE-4D5E-BBF4-2AC577C1644A", "Delivery terms other than those listed above");

				#endregion

				public static readonly ImmutableDictionary<string, MultilingualString> DefaultCodeDescriptionPairs = new Dictionary<string, MultilingualString>()
				{
					{ IncoTerms.ExWorks, ExWorks },
					{ IncoTerms.FreeCarrier, FreeCarrier },
					{ IncoTerms.FreeAlongsideShip, FreeAlongsideShip },
					{ IncoTerms.FreeOnBoard, FreeOnBoard },
					{ IncoTerms.CostAndFreight, CostAndFreight },
					{ IncoTerms.CostInsuranceAndFreight, CostInsuranceAndFreight },
					{ IncoTerms.CarriagePaidTo, CarriagePaidTo },
					{ IncoTerms.CarriageAndInsurancePaidTo, CarriageAndInsurancePaidTo },
					{ IncoTerms.DeliveredDutyPaid, DeliveredDutyPaid },
					{ IncoTerms.DeliveredAtFrontier, DeliveredAtFrontier },
					{ IncoTerms.DeliveredExShip, DeliveredExShip },
					{ IncoTerms.DeliveredExQuay, DeliveredExQuay },
					{ IncoTerms.DeliveredDutyUnpaid, DeliveredDutyUnpaid },
					{ IncoTerms.DeliveredAtPlace, DeliveredAtPlace },
					{ IncoTerms.DeliveredAtTerminal, DeliveredAtTerminal },
					{ IncoTerms.FreeCarrierSeller, FreeCarrierSeller },
					{ IncoTerms.FreeCarrierBuyer, FreeCarrierBuyer },
					{ IncoTerms.DeliveredAtPlaceUnloaded, DeliveredAtPlaceUnloaded },
					{ IncoTerms.Other, Other }
				}.ToImmutableDictionary();
			}
		}

		public static class DomesticPaymentTerms
		{
			public const string Prepaid = "PPD";
			public const string Collect = "CLT";
			public const string CollectThirdParty = "C3P";
			public const string CollectCOD = "FCD";
		}

		public static class PaymentParty
		{
			public const string Consignee = "CNE";
			public const string Consignor = "CNR";
		}

		public static class InvoiceTerms
		{
			public const string CashOnDelivery = "COD";
			public const string PaymentInAdvance = "PIA";
			public const string FromCustomsClearanceDate = "CUS";
			public const string FromInvoiceDate = "INV";
			public const string FromMonthEnd = "MTH";
			public const string FromWeekEnd = "EWK";
			public const string FromPeriodEnd = "PER";
			public const string FromShipmentDate = "SHP";
			public const string MonthsFromInvoiceCycleDate = "MIC";
			public const string TermDaysAndDebtorPaymentCycle = "DPC";
			public const string MultipleInstallments = "MLI";
			public const string LaterOfShipmentOrInvoiceDate = "LSI";
			public const string FromDeliveryOrPickupDate = "DLP";
		}

		public static class TransactionCreationRestriction
		{
			public const string None = "NON";
			public const string Invoice = "INV";
			public const string All = "ALL";
			public const string OutstandingBalance = "BAL";
		}

		public static class Sales
		{
			public static class CommunicationType
			{
				public const string PhoneCall = "PHN";
				public const string Meeting = "MTG";
				public const string Email = "EML";
				public const string OnlineConference = "ONC";

				public const string FirstCall = "1ST";
				public const string FollowUp = "FUP";
				public const string Service = "SRV";
			}

			public static class CommissionType
			{
				public const string NoCommission = "NOC";
				public const string NewSale = "NEW";
				public const string ExistingClient = "EXS";
				public const string Standard = "STD";
			}

			public static class CompetitorActivity
			{
				public const string Freight = "FRT";
				public const string Brokerage = "BRK";
				public const string FrtAndBrk = "F&B";
				public const string Transport = "TRN";
				public const string FrtAndTrn = "F&T";
				public const string BrkAndTrn = "B&T";
				public const string TrnBrkFrt = "TBF";
				public const string Warehouse = "WHS";
				public const string FrtAndWhs = "F&W";
				public const string BrkAndWhs = "B&W";
				public const string TrnAndWhs = "T&W";
				public const string FrtBrkWhs = "FBW";
				public const string FrtTrnWhs = "FTW";
				public const string All = "ALL";
			}

			public static class EffectOnCosts
			{
				public const string SmlIncrease = "SIC";
				public const string MedIncrease = "MIC";
				public const string LrgIncrease = "LIC";
				public const string Negligible = "NIL";
				public const string SmlDecrease = "SDC";
				public const string MedDecrease = "MDC";
				public const string LrgDecrease = "LDC";
			}

			public static class GrowthOutlook
			{
				public const string CLC = "CLC";
				public const string CLL = "CLL";
				public const string LDC = "LDC";
				public const string LDL = "LDL";
				public const string SDC = "SDC";
				public const string SDL = "SDL";
				public const string NOR = "NOR";
				public const string SIE = "SIE";
				public const string SIN = "SIN";
				public const string LIE = "LIE";
				public const string LIN = "LIN";
			}

			public static class LeadType
			{
				public const string Website = "WEB";
				public const string TelemarketingOrg = "TMK";
				public const string WordOfMouth = "WOM";
				public const string Other = "OTH";
			}

			public static class Mode
			{
				public const string Import = "IMP";
				public const string Export = "EXP";
			}

			public static class Status
			{
				public const string Scheduled = "SCH";
				public const string Completed = "COM";
				public const string Cancelled = "CAN";

				public const string CurrentClient = "CUR";
				public const string HotProspect = "HOT";
				public const string WarmProspect = "WRM";
				public const string ColdProspect = "CLD";
				public const string MoreWork = "WIP";
				public const string ConfirmedEntire = "CNF";
				public const string ConfirmedPartial = "PCF";
				public const string NoInterestNow = "NOI";
				public const string NoInterestAtAll = "DIS";
			}

			public static class Style
			{
				public const string PriceBased = "PRC";
				public const string ServiceBase = "SRV";
				public const string PriceAndService = "PRS";
				public const string Integration = "INT";
				public const string Technology = "TEC";
				public const string Size = "SIZ";
				public const string Coverage = "COV";
				public const string Specialist = "SPC";
				public const string Other = "OTH";
			}

			public static class Territory
			{
				public const string Branch = "BRN";
			}
		}

		public static class CustomLabels
		{
			public static class Order
			{
				public const string Prefix = "OrderHeader";
				public const string CustomDate1 = "OrderHeader.CustomDate1";
				public const string CustomDate2 = "OrderHeader.CustomDate2";
				public const string CustomAttribute1 = "OrderHeader.CustomAttrib1";
				public const string CustomAttribute2 = "OrderHeader.CustomAttrib2";
				public const string CustomAttribute3 = "OrderHeader.CustomAttrib3";
				public const string CustomAttribute4 = "OrderHeader.CustomAttrib4";
				public const string CustomAttribute5 = "OrderHeader.CustomAttrib5";
				public const string CustomFlag1 = "OrderHeader.CustomFlag1";
				public const string CustomFlag2 = "OrderHeader.CustomFlag2";
				public const string CustomFlag3 = "OrderHeader.CustomFlag3";
				public const string CustomFlag4 = "OrderHeader.CustomFlag4";
				public const string CustomFlag5 = "OrderHeader.CustomFlag5";
				public const string CustomDecimal1 = "OrderHeader.CustomDecimal1";
				public const string CustomDecimal2 = "OrderHeader.CustomDecimal2";
				public const string CustomDecimal3 = "OrderHeader.CustomDecimal3";
				public const string CustomDecimal4 = "OrderHeader.CustomDecimal4";
				public const string CustomDecimal5 = "OrderHeader.CustomDecimal5";
				public const string CustomContact1 = "OrderHeader.CustomContact1";
				public const string CustomContact2 = "OrderHeader.CustomContact2";
				public const string GoodsOrigin = "OrderHeader.GoodsOrigin";
				public const string GoodsDestination = "OrderHeader.GoodsDestination";
				public const string UserTrackDate1 = "OrderHeader.UserTrackDate1";
				public const string UserTrackDate2 = "OrderHeader.UserTrackDate2";
				public const string UserTrackDate3 = "OrderHeader.UserTrackDate3";
				public const string UserTrackDate4 = "OrderHeader.UserTrackDate4";

				public static class Descriptions
				{
					public static MultilingualString CustomContact1 { get { return ResString.GetMultilingualString("4b4307f9-19c8-4f8c-b3ea-f585d6b48ee1", "Contact {0}", 1); } }
					public static MultilingualString CustomContact2 { get { return ResString.GetMultilingualString("4b4307f9-19c8-4f8c-b3ea-f585d6b48ee1", "Contact {0}", 2); } }
					public static MultilingualString GoodsOrigin { get { return ResString.GetMultilingualString("cbd80d75-d5a8-4753-aac9-7409bd095aae", "Goods Origin"); } }
					public static MultilingualString GoodsDestination { get { return ResString.GetMultilingualString("70c0d9c4-036d-472f-a2fb-4625094b7f0d", "Goods Dest."); } }
					public static MultilingualString UserTrackDate1 { get { return ResString.GetMultilingualString("870f6c35-635c-4fb5-a2de-2373052b7a83", "User Track Date {0}", 1); } }
					public static MultilingualString UserTrackDate2 { get { return ResString.GetMultilingualString("870f6c35-635c-4fb5-a2de-2373052b7a83", "User Track Date {0}", 2); } }
					public static MultilingualString UserTrackDate3 { get { return ResString.GetMultilingualString("870f6c35-635c-4fb5-a2de-2373052b7a83", "User Track Date {0}", 3); } }
					public static MultilingualString UserTrackDate4 { get { return ResString.GetMultilingualString("870f6c35-635c-4fb5-a2de-2373052b7a83", "User Track Date {0}", 4); } }
				}
			}

			public static class OrderLine
			{
				public const string Prefix = "OrderLine";
				public const string CustomAttribute1 = "OrderLine.CustomAttrib1";
				public const string CustomAttribute2 = "OrderLine.CustomAttrib2";
				public const string CustomAttribute3 = "OrderLine.CustomAttrib3";
				public const string CustomAttribute4 = "OrderLine.CustomAttrib4";
				public const string CustomAttribute5 = "OrderLine.CustomAttrib5";
				public const string CustomAttribute6 = "OrderLine.CustomAttrib6";
				public const string CustomTextBlob1 = "OrderLine.CustomTextBlob1";
				public const string CustomText1 = "OrderLine.CustomText1";
				public const string CustomFlag1 = "OrderLine.CustomFlag1";
				public const string CustomFlag2 = "OrderLine.CustomFlag2";
				public const string CustomFlag3 = "OrderLine.CustomFlag3";
				public const string CustomFlag4 = "OrderLine.CustomFlag4";
				public const string CustomFlag5 = "OrderLine.CustomFlag5";
				public const string CustomDate1 = "OrderLine.CustomDate1";
				public const string CustomDate2 = "OrderLine.CustomDate2";
				public const string CustomDate3 = "OrderLine.CustomDate3";
				public const string CustomDate4 = "OrderLine.CustomDate4";
				public const string CustomDate5 = "OrderLine.CustomDate5";
				public const string CustomDecimal1 = "OrderLine.CustomDecimal1";
				public const string CustomDecimal2 = "OrderLine.CustomDecimal2";
				public const string CustomDecimal3 = "OrderLine.CustomDecimal3";
				public const string CustomDecimal4 = "OrderLine.CustomDecimal4";
				public const string CustomDecimal5 = "OrderLine.CustomDecimal5";
			}

			public static class OrderLineDelivery
			{
				public const string Prefix = "OrderLineDelivery";
				public const string CustomAttribute1 = "OrderDelivery.CustomAttribute1";
				public const string CustomAttribute2 = "OrderDelivery.CustomAttribute2";
				public const string CustomAttribute3 = "OrderDelivery.CustomAttribute3";
				public const string CustomAttribute4 = "OrderDelivery.CustomAttribute4";
				public const string CustomAttribute5 = "OrderDelivery.CustomAttribute5";
				public const string CustomFlag1 = "OrderDelivery.CustomFlag1";
				public const string CustomFlag2 = "OrderDelivery.CustomFlag2";
				public const string CustomFlag3 = "OrderDelivery.CustomFlag3";
				public const string CustomFlag4 = "OrderDelivery.CustomFlag4";
				public const string CustomFlag5 = "OrderDelivery.CustomFlag5";
				public const string CustomDate1 = "OrderDelivery.CustomDate1";
				public const string CustomDate2 = "OrderDelivery.CustomDate2";
				public const string CustomDate3 = "OrderDelivery.CustomDate3";
				public const string CustomDate4 = "OrderDelivery.CustomDate4";
				public const string CustomDate5 = "OrderDelivery.CustomDate5";
				public const string CustomDecimal1 = "OrderDelivery.CustomDecimal1";
				public const string CustomDecimal2 = "OrderDelivery.CustomDecimal2";
				public const string CustomDecimal3 = "OrderDelivery.CustomDecimal3";
				public const string CustomDecimal4 = "OrderDelivery.CustomDecimal4";
				public const string CustomDecimal5 = "OrderDelivery.CustomDecimal5";
			}

			public static class OrderLineDeliverContainer
			{
				public const string Prefix = "OrderLineDeliverContainer";
				public const string CustomAttribute1 = "OrderContainer.CustomAttribute1";
				public const string CustomAttribute2 = "OrderContainer.CustomAttribute2";
				public const string CustomAttribute3 = "OrderContainer.CustomAttribute3";
				public const string CustomFlag1 = "OrderContainer.CustomFlag1";
				public const string CustomFlag2 = "OrderContainer.CustomFlag2";
				public const string CustomFlag3 = "OrderContainer.CustomFlag3";
				public const string CustomDate1 = "OrderContainer.CustomDate1";
				public const string CustomDate2 = "OrderContainer.CustomDate2";
				public const string CustomDate3 = "OrderContainer.CustomDate3";
				public const string CustomDecimal1 = "OrderContainer.CustomDecimal1";
				public const string CustomDecimal2 = "OrderContainer.CustomDecimal2";
				public const string CustomDecimal3 = "OrderContainer.CustomDecimal3";

				public const string QuantityInvoiced = "OrderContainer.QuantityInvoiced";
				public const string QuantityDelivered = "OrderContainer.QuantityDelivered";

				public static class Descriptions
				{
					public static MultilingualString QuantityInvoiced { get { return ResString.GetMultilingualString("6108bdbb-7c91-492c-bf1b-1a05b6e486b0", "Qty Invoiced"); } }
					public static MultilingualString QuantityDelivered { get { return ResString.GetMultilingualString("7f12b761-d308-4510-b635-9c5fa5017914", "Qty Delivered"); } }
				}
			}

			public static class Parts
			{
				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Consol code constant")]
				public const string Prefix = "Parts";
				public const string CustomAttribute1 = "Parts.CustomAttribute1";
				public const string CustomAttribute2 = "Parts.CustomAttribute2";
				public const string CustomAttribute3 = "Parts.CustomAttribute3";
				public const string CustomAttribute4 = "Parts.CustomAttribute4";
				public const string CustomAttribute5 = "Parts.CustomAttribute5";
				public const string CustomFlag1 = "Parts.CustomFlag1";
				public const string CustomFlag2 = "Parts.CustomFlag2";
				public const string CustomFlag3 = "Parts.CustomFlag3";
				public const string CustomFlag4 = "Parts.CustomFlag4";
				public const string CustomFlag5 = "Parts.CustomFlag5";
				public const string CustomDate1 = "Parts.CustomDate1";
				public const string CustomDate2 = "Parts.CustomDate2";
				public const string CustomDate3 = "Parts.CustomDate3";
				public const string CustomDate4 = "Parts.CustomDate4";
				public const string CustomDate5 = "Parts.CustomDate5";
				public const string CustomDecimal1 = "Parts.CustomDecimal1";
				public const string CustomDecimal2 = "Parts.CustomDecimal2";
				public const string CustomDecimal3 = "Parts.CustomDecimal3";
				public const string CustomDecimal4 = "Parts.CustomDecimal4";
				public const string CustomDecimal5 = "Parts.CustomDecimal5";

				public const string OrderMultiple = "Parts.OrderMultiple";
				public const string VendorPack = "Parts.VendorPack";
				public const string Department = "Parts.Department";
				public const string Division = "Parts.Division";

				public static class Descriptions
				{
					public static MultilingualString OrderMultiple { get { return ResString.GetMultilingualString("bad92666-1c62-4794-b335-7d02fe39392d", "Order Multiple"); } }
					public static MultilingualString VendorPack { get { return ResString.GetMultilingualString("c78d1287-f9a5-4597-bbd6-607ec16b70c0", "Vendor Pack"); } }
					public static MultilingualString Department { get { return ResString.GetMultilingualString("fe5e7676-a67f-45a5-8685-c4299c870cb2", "Department"); } }
					public static MultilingualString Division { get { return ResString.GetMultilingualString("26038089-1b84-4d77-8887-b649c232cbdb", "Division"); } }
				}
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Consol code constant")]
			public static class Organisation
			{
				public const string Prefix = "Organisation";
				public const string CustomAttribute1 = "Organisation.CustomAttribute1";
				public const string CustomAttribute2 = "Organisation.CustomAttribute2";
				public const string CustomAttribute3 = "Organisation.CustomAttribute3";
				public const string CustomFlag1 = "Organisation.CustomFlag1";
				public const string CustomFlag2 = "Organisation.CustomFlag2";
				public const string CustomFlag3 = "Organisation.CustomFlag3";
				public const string CustomFlag4 = "Organisation.CustomFlag4";
				public const string CustomDate1 = "Organisation.CustomDate1";
				public const string CustomDate2 = "Organisation.CustomDate2";
				public const string CustomDate3 = "Organisation.CustomDate3";
				public const string CustomDecimal1 = "Organisation.CustomDecimal1";
				public const string CustomDecimal2 = "Organisation.CustomDecimal2";
				public const string CustomDecimal3 = "Organisation.CustomDecimal3";
			}

			public static class ComInvoiceLine
			{
				public const string Prefix = "ComInvoiceLine";
				public const string CustomAttribute1 = "ComInvoiceLine.CustomAttribute1";
				public const string CustomAttribute2 = "ComInvoiceLine.CustomAttribute2";
				public const string CustomAttribute3 = "ComInvoiceLine.CustomAttribute3";
				public const string CustomAttribute4 = "ComInvoiceLine.CustomAttribute4";
				public const string CustomAttribute5 = "ComInvoiceLine.CustomAttribute5";
				public const string CustomAttribute6 = "ComInvoiceLine.CustomAttribute6";
				public const string CustomText1 = "ComInvoiceLine.CustomText1";
				public const string CustomFlag1 = "ComInvoiceLine.CustomFlag1";
				public const string CustomFlag2 = "ComInvoiceLine.CustomFlag2";
				public const string CustomFlag3 = "ComInvoiceLine.CustomFlag3";
				public const string CustomDate1 = "ComInvoiceLine.CustomDate1";
				public const string CustomDate2 = "ComInvoiceLine.CustomDate2";
				public const string CustomDate3 = "ComInvoiceLine.CustomDate3";
				public const string CustomDecimal1 = "ComInvoiceLine.CustomDecimal1";
				public const string CustomDecimal2 = "ComInvoiceLine.CustomDecimal2";
				public const string CustomDecimal3 = "ComInvoiceLine.CustomDecimal3";
			}

			public static class CusContainer
			{
				public const string Prefix = "CusContainer";
				public const string CustomAttribute1 = "CusContainer.CustomAttribute1";
				public const string CustomFlag1 = "CusContainer.CustomFlag1";
				public const string CustomDate1 = "CusContainer.CustomDate1";
				public const string CustomDecimal1 = "CusContainer.CustomDecimal1";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Consol code constant")]
			public static class Consol
			{
				public const string Prefix = "Consol";
				public const string CustomDate1 = "Consol.CustomDate1";
				public const string CustomDate2 = "Consol.CustomDate2";
				public const string CustomFlag1 = "Consol.CustomFlag1";
				public const string CustomFlag2 = "Consol.CustomFlag2";
				public const string CustomString1 = "Consol.CustomString1";
				public const string CustomString2 = "Consol.CustomString2";
				public const string CustomNumber1 = "Consol.CustomNumber1";
				public const string CustomNumber2 = "Consol.CustomNumber2";
			}

			public static class WhsDocket
			{
				public const string Prefix = "WhsDocket";
				public const string CustomAttribute1 = "WhsDocket.CustomAttrib1";
				public const string CustomAttribute2 = "WhsDocket.CustomAttrib2";
				public const string CustomAttribute3 = "WhsDocket.CustomAttrib3";
				public const string CustomAttribute4 = "WhsDocket.CustomAttrib4";
				public const string CustomAttribute5 = "WhsDocket.CustomAttrib5";
				public const string CustomDate1 = "WhsDocket.CustomDate1";
				public const string CustomDate2 = "WhsDocket.CustomDate2";
				public const string CustomDecimal1 = "WhsDocket.CustomDecimal1";
				public const string CustomDecimal2 = "WhsDocket.CustomDecimal2";
				public const string CustomDecimal3 = "WhsDocket.CustomDecimal3";
				public const string CustomDecimal4 = "WhsDocket.CustomDecimal4";
				public const string CustomDecimal5 = "WhsDocket.CustomDecimal5";
				public const string CustomFlag1 = "WhsDocket.CustomFlag1";
				public const string CustomFlag2 = "WhsDocket.CustomFlag2";
				public const string CustomFlag3 = "WhsDocket.CustomFlag3";
				public const string CustomFlag4 = "WhsDocket.CustomFlag4";
				public const string CustomFlag5 = "WhsDocket.CustomFlag5";
			}

			public static class WhsDocketLine
			{
				public const string Prefix = "WhsDocketLine";
				public const string CustomAttribute1 = "WhsDocketLine.CustomAttrib1";
				public const string CustomAttribute2 = "WhsDocketLine.CustomAttrib2";
				public const string CustomAttribute3 = "WhsDocketLine.CustomAttrib3";
				public const string CustomAttribute4 = "WhsDocketLine.CustomAttrib4";
				public const string CustomAttribute5 = "WhsDocketLine.CustomAttrib5";
				public const string CustomAttribute6 = "WhsDocketLine.CustomAttrib6";
				public const string CustomDate1 = "WhsDocketLine.CustomDate1";
				public const string CustomDate2 = "WhsDocketLine.CustomDate2";
				public const string CustomDate3 = "WhsDocketLine.CustomDate3";
				public const string CustomDate4 = "WhsDocketLine.CustomDate4";
				public const string CustomDate5 = "WhsDocketLine.CustomDate5";
				public const string CustomDecimal1 = "WhsDocketLine.CustomDecimal1";
				public const string CustomDecimal2 = "WhsDocketLine.CustomDecimal2";
				public const string CustomDecimal3 = "WhsDocketLine.CustomDecimal3";
				public const string CustomDecimal4 = "WhsDocketLine.CustomDecimal4";
				public const string CustomDecimal5 = "WhsDocketLine.CustomDecimal5";
				public const string CustomFlag1 = "WhsDocketLine.CustomFlag1";
				public const string CustomFlag2 = "WhsDocketLine.CustomFlag2";
				public const string CustomFlag3 = "WhsDocketLine.CustomFlag3";
				public const string CustomFlag4 = "WhsDocketLine.CustomFlag4";
				public const string CustomFlag5 = "WhsDocketLine.CustomFlag5";
				public const string CustomTextBlob1 = "WhsDocketLine.CustomTextBlob1";
			}

			public static class CustomsPackingList
			{
				public const string Prefix = "CusPackingList";
				public const string CustomAttribute1 = "CusPackingList.CustomAttrib1";
				public const string CustomAttribute2 = "CusPackingList.CustomAttrib2";
				public const string CustomFlag1 = "CusPackingList.CustomFlag1";
				public const string CustomFlag2 = "CusPackingList.CustomFlag2";
				public const string CustomDate1 = "CusPackingList.CustomDate1";
				public const string CustomDate2 = "CusPackingList.CustomDate2";
				public const string CustomDecimal1 = "CusPackingList.CustomDecimal1";
				public const string CustomDecimal2 = "CusPackingList.CustomDecimal2";
			}

			public static class CusPackage
			{
				public const string Prefix = "CusPackage";
				public const string CustomAttribute1 = "CusPackage.CustomAttrib1";
				public const string CustomAttribute2 = "CusPackage.CustomAttrib2";
				public const string CustomFlag1 = "CusPackage.CustomFlag1";
				public const string CustomFlag2 = "CusPackage.CustomFlag2";
				public const string CustomDate1 = "CusPackage.CustomDate1";
				public const string CustomDate2 = "CusPackage.CustomDate2";
				public const string CustomDecimal1 = "CusPackage.CustomDecimal1";
				public const string CustomDecimal2 = "CusPackage.CustomDecimal2";
			}

			public static class Descriptions
			{
				public static MultilingualString CustomDate(int i)
				{
					return ResString.GetMultilingualString("a360bd16-9e28-447f-a1bf-a0ae5d46e965", "Custom Date {0}", i);
				}

				public static MultilingualString CustomAttribute(int i)
				{
					return ResString.GetMultilingualString("ee91430b-2762-4853-9b91-1327723cfbe9", "Custom Attribute {0}", i);
				}

				public static MultilingualString CustomFlag(int i)
				{
					return ResString.GetMultilingualString("3fd3b7d3-05d1-4a2e-b135-3c7440196e77", "Custom Flag {0}", i);
				}

				public static MultilingualString CustomNumber(int i)
				{
					return ResString.GetMultilingualString("5ae6ccb4-fa4d-452e-9438-d0435e1edaaf", "Custom Number {0}", i);
				}

				public static MultilingualString CustomText(int i)
				{
					return ResString.GetMultilingualString("4f1079f3-bdb5-4fd9-8279-2c1734f32e11", "Custom Text {0}", i);
				}
			}
		}

		public static class CustomDocuments
		{
			public static class ProfitShareCalculationWorkSheet
			{
				public const string HideRevenueCostFigures = "ProfitShareDocument.RevenueCost";
			}
		}

		public static class AirDensity
		{
			public const string Light = "LGT";
			public const string Heavy = "HVY";
			public const string Normal = "NRM";
		}

		#region Port Transport

		public static class CartageJobType
		{
			public const string FCLImport = "FUI";
			public const string FCLExport = "FPE";
			public const string FCLPack = "FPD";
			public const string FCLUnpack = "FUD";

			public const string LCLImport = "LDI";
			public const string LCLExport = "LED";

			public const string AirImport = "AIM";
			public const string AirExport = "AEX";

			public const string EmptyYardToExporter = "FYE";
			public const string EmptyImporterToYard = "FIY";
			public const string EmptyYardToCFS = "FYD";
			public const string EmptyCFSToYard = "FDY";

			public const string FullExporterToCTO = "FEC";
			public const string FullCTOToImporter = "FCI";
			public const string FullCTOToCFS = "FCD";
			public const string FullCFSToCTO = "FDC";

			public const string FTL = "FTL";
			public const string LTL = "LTL"; //Destination
			public const string LTE = "LTE"; //Origin
			public const string FCL = "FCL"; //Destination
			public const string FCE = "FCE"; //Origin

			public const string CTOToImporterToYard = "FIW";
			public const string YardToExporterToCTO = "FEW";

			public const string DomesticLoosePickup = "DPU";
			public const string DomesticLooseDelivery = "DDL";

			public const string DomesticContainerizedPickup = "DCP";
			public const string DomesticContainerizedDelivery = "DCD";

			//CFS->CNR: Returned to Consignor from CFS (Don't support FCL)
			public const string ReturnToCNR = "RTC";

			//New
			public const string NEW_AirExport = "EALF";
			public const string NEW_AirImport = "IALC";
			public const string NEW_AirImportLooseCTOtoCFS = "IALL";
			public const string NEW_AirImportULDCTOtoCFS = "IAFF";
			public const string NEW_AirExportLooseCFStoCTO = "EALL";
			public const string NEW_AirExportULDCFStoCTO = "EAFF";

			public const string NEW_DomesticContainerizedDelivery = "DRFC";
			public const string NEW_DomesticContainerizedPickup = "ORFC";
			public const string NEW_DomesticLooseDelivery = "DRLC";
			public const string NEW_DomesticLoosePickup = "ORLC";

			public const string NEW_EmptyCFStoCYD = "ISEF";
			public const string NEW_EmptyCNEtoCYD = "ISEC";
			public const string NEW_EmptyCYDtoCFS = "ESEF";
			public const string NEW_EmptyCYDtoSHP = "ESES";

			public const string NEW_FCLCFStoCTO = "ESFF";
			public const string NEW_FCLCTOtoCFS = "ISFF";
			public const string NEW_FCLCTOtoCNE = "ISFC";

			public const string NEW_FCLCTOtoCNEWAITtoCYD = "ISCW";
			public const string NEW_FCLCYDtoSHPWAITtoCTO = "ESCW";

			public const string NEW_FCLExportPack = "ESCF";
			public const string NEW_FCLExportToSHP = "ESCS";

			public const string NEW_FCLImportUnpack = "ISCF";
			public const string NEW_FCLImportToCNE = "ISCC";

			public const string NEW_FCLPackLooseFromSHP = "ESMY";
			public const string NEW_FCLUnpackLooseToCNE = "ISMY";

			public const string NEW_FCLSHPtoCTO = "ESFS";

			public const string NEW_LCLExport = "ESLF";
			public const string NEW_LCLImport = "ISLC";

			public const string NEW_LineHaulFTLCFStoCFS = "HRTF";
			public const string NEW_MilkRun = "LRLN";
			public const string NEW_ReturnedCFStoSHP = "ERLR";
			public const string NEW_StagingCTOtoCFStoCNEtoCYD = "ISC3";

			public const string NEW_WarehouseContainerizedDelivery = "DRCW";
			public const string NEW_WarehouseLooseDelivery = "DRLW";
		}

		public static class CartageLegType
		{
			public const string FCLImport = "FUI";
			public const string FCLExport = "FPE";
			public const string FCLPack = "FPD";
			public const string FCLUnpack = "FUD";

			public const string LCLImport = "LDI";
			public const string LCLExport = "LED";

			public const string AirImport = "AIM";
			public const string AirExport = "AEX";

			public const string EmptyYardToExporter = "FYE";
			public const string EmptyImporterToYard = "FIY";
			public const string EmptyYardToCFS = "FYD";
			public const string EmptyCFSToYard = "FDY";

			public const string FullExporterToCTO = "FEC";
			public const string FullCTOToImporter = "FCI";
			public const string FullCTOToCFS = "FCD";
			public const string FullCFSToCTO = "FDC";

			public const string FTL = "FTL";
			public const string LTL = "LTL"; //Destination
			public const string LTE = "LTE"; //Origin
			public const string FCL = "FCL"; //Destination
			public const string FCE = "FCE"; //Origin

			public const string CTOToImporterToYard = "FIW";
			public const string YardToExporterToCTO = "FEW";

			public const string DomesticLoosePickup = "DPU";
			public const string DomesticLooseDelivery = "DDL";

			public const string DomesticContainerizedPickup = "DCP";
			public const string DomesticContainerizedDelivery = "DCD";

			//CFS->CNR: Returned to Consignor from CFS (Don't support FCL)
			public const string ReturnToCNR = "RTC";
		}

		public static class CartageDirectionChar
		{
			public const string Import = "I";
			public const string Export = "E";
			public const string Origin = "O";
			public const string Destination = "D";
			public const string Local = "L";
			public const string LineHaul = "H";
		}

		public static class CartageDirection
		{
			public const string Import = "IMP";
			public const string Export = "EXP";
			public const string Origin = "ORG";
			public const string Destination = "DST";
			public const string Local = "LOC";
			public const string LineHaul = "LIN";
		}

		public static class CartageDirectionDescription
		{
			public static MultilingualString Import { get { return ResString.GetMultilingualString("ec3db510-eef5-491d-9eb1-a97eb0ddb18e", "Import"); } }
			public static MultilingualString Export { get { return ResString.GetMultilingualString("aa66eb80-e722-47ca-ab83-b20237c92bcb", "Export"); } }
			public static MultilingualString Origin { get { return ResString.GetMultilingualString("8692879a-292e-4cb5-84fa-d0dcedb579df", "Origin Pickup"); } }
			public static MultilingualString Destination { get { return ResString.GetMultilingualString("c2ece963-0cfd-475e-b91c-c51477f54158", "Destination Delivery"); } }
			public static MultilingualString Local { get { return ResString.GetMultilingualString("41be25db-8086-421a-abde-5fe04976864d", "Port Transport Movement"); } }
			public static MultilingualString LineHaul { get { return ResString.GetMultilingualString("21072651-6fb5-4552-a795-4a6e3b46ba28", "Line Haul Movement"); } }
		}

		public static class CartageContainerModeChar
		{
			public const string Containerized = "C";
			public const string FCL = "F";
			public const string Loose = "L";
			public const string EmptyContainer = "E";
			public const string FTL = "T";
			public const string Mixed = "M";
		}

		public static class CartageContainerMode
		{
			public const string Containerized = "CNT";
			public const string Loose = "LSE";

			public const string FCL = "FCL";
			public const string EmptyContainer = "EMP";
			public const string FTL = "FTL";
			public const string Mixed = "MIX";
		}

		public static class CartageContainerModeDescription
		{
			public static MultilingualString Containerized { get { return ResString.GetMultilingualString("b1f1e4d0-5afa-4314-97ca-448a7ff403a9", "Containerized"); } }
			public static MultilingualString Loose { get { return ResString.GetMultilingualString("c2b1d4dd-296f-47d9-9b12-ed097eeac78b", "Air/Loose/Less than Truck Load"); } }

			public static MultilingualString FCL { get { return ResString.GetMultilingualString("45ecb60b-711a-444b-88c7-af1f2632269d", "FCL"); } }
			public static MultilingualString EmptyContainer { get { return ResString.GetMultilingualString("d0a10ce6-d3bc-4138-914e-0729979a149f", "Empty Container"); } }
			public static MultilingualString FTL { get { return ResString.GetMultilingualString("f425d6a2-0f9f-4886-827c-f6f317414d6c", "Full Truck Load"); } }
			public static MultilingualString Mixed { get { return ResString.GetMultilingualString("2edb89fc-8349-4f9e-8f4e-8a32bf1c08e2", "Mixed"); } }
		}

		public static class CartageAdditional
		{
			public const string Futile = "FUT";
			public const string AdditionalService = "SVC";
		}

		public static class CartageAdditionalDescritpion
		{
			public static MultilingualString Futile { get { return ResString.GetMultilingualString("552ed82f-8d10-42f4-a10c-016a67641e31", "Futile"); } }
			public static MultilingualString AdditionalService { get { return ResString.GetMultilingualString("14d327c1-40fb-4b4c-8463-f0c5397bb1b1", "Additional Service"); } }
		}

		public static class ConsolInvoicingStyles
		{
			public const string Master = "MAS";
			public const string Apportion = "APP";
			public const string ApportionInvoiceMaster = "MAB";
		}

		public static class PickupDeliveryConfirmTypes
		{
			public const string OriginPickup = "PCU";
			public const string DestinationDelivery = "DLV";
			public const string OriginCFSArrival = "CPA";
			public const string OriginCFSDeparture = "CPD";
			public const string DestinationCFSArrival = "CDA";
			public const string DestinationCFSDeparture = "CDD";
		}

		#endregion

		public static class ShipmentTypes
		{
			public const string StandardHouse = "STD";
			public const string CoLoadMaster = "CLD";
			public const string BlindCoLoadMaster = "CLB";
			public const string BuyersConsolLead = "BCN";
			public const string ShippersConsolLead = "SCN";
			public const string AssemblyMaster = "ASM";
			public const string HighVolumeLowValueLegacy = "HLS";
			public const string HighVolumeLowValueMaster = "HVM";
			public const string HighVolumeLowValue = "HVL";
			public const string ThirdPartyOwnershipHouse = "3PT";
		}

		public static class ShipmentTypeDescriptions
		{
			public static MultilingualString StandardHouse { get { return ResString.GetMultilingualString("Common|ShipmentType|StandardHouse", "Standard House"); } }
			public static MultilingualString CoLoadMaster { get { return ResString.GetMultilingualString("Common|ShipmentType|CoLoadMaster", "Co-Load Master"); } }
			public static MultilingualString BlindCoLoadMaster { get { return ResString.GetMultilingualString("Common|ShipmentType|BlindCoLoadMaster", "Blind Co-Load Master"); } }
			public static MultilingualString BuyersConsolLead { get { return ResString.GetMultilingualString("Common|ShipmentType|BuyersConsolLead", "Buyer's Consol Lead"); } }
			public static MultilingualString ShippersConsolLead { get { return ResString.GetMultilingualString("Common|ShipmentType|ShippersConsolLead", "Shipper's Consol Lead"); } }
			public static MultilingualString AssemblyMaster { get { return ResString.GetMultilingualString("Common|ShipmentType|AssemblyMaster", "Assembly Master"); } }
			public static MultilingualString HighVolumeLowValueLegacy { get { return ResString.GetMultilingualString("Common|ShipmentType|HighVolumeLowValueLegacy", "HVLV Shipper Consolidation (Legacy)"); } }
			public static MultilingualString HighVolumeLowValueMaster { get { return ResString.GetMultilingualString("Common|ShipmentType|HighVolumeLowValueMaster", "HVLV Shipper Consolidation (Master)"); } }
			public static MultilingualString HighVolumeLowValue { get { return ResString.GetMultilingualString("Common|ShipmentType|HighVolumeLowValue", "HVLV Shipper Consolidation"); } }
			public static MultilingualString ThirdPartyOwnershipHouse { get { return ResString.GetMultilingualString("Common|ShipmentType|ThirdPartyOwnershipHouse", "Third Party Ownership House"); } }
		}

		public static class ELoadListStatuses
		{
			public const string Open = "OPN";
			public const string Closed = "CLS";
			public const string Consolidated = "CON";
			public const string Lodged = "LDG";
			public const string Failed = "FAL";
		}

		public static class ELoadListStatusDescription
		{
			public static MultilingualString Open { get { return ResString.GetMultilingualString("eManifest|eLoadList|Open", "Open"); } }
			public static MultilingualString Closed { get { return ResString.GetMultilingualString("eManifest|eLoadList|Closed", "Closed"); } }
			public static MultilingualString Consolidated { get { return ResString.GetMultilingualString("eManifest|eLoadList|Consolidated", "Consolidated"); } }
			public static MultilingualString Lodged { get { return ResString.GetMultilingualString("eManifest|eLoadList|Lodged", "Lodged"); } }
		}

		public static class EventReferenceReservedParameters
		{
			public static class Codes
			{
				public const string Reference = "REF";
			}

			public static class Descriptions
			{
				public static MultilingualString Reference
				{
					get { return ResString.GetMultilingualString("67288BCC-895C-4B9E-A0B2-03B752421051", "Reference"); }
				}
			}
		}

		public static class EventReferenceParameters
		{
			public static class Descriptions
			{
				public static MultilingualString ApprovalParty
				{
					get { return ResString.GetMultilingualString("8254204a-d65c-465f-a05d-d21a144c54e0", "Approval Party"); }
				}

				public static MultilingualString CustomsDeclarationNumber
				{
					get { return ResString.GetMultilingualString("cfae4cc1-6e93-4450-83d3-471144d55a2c", "Customs Declaration Number"); }
				}

				public static MultilingualString CustomsStatus
				{
					get { return ResString.GetMultilingualString("1e2733a3-7603-4b01-b68f-221856fdef63", "Customs Status"); }
				}

				public static MultilingualString Facility
				{
					get { return ResString.GetMultilingualString("F90BB106-79EF-4C49-9EC6-F04ACBFE308A", "Facility"); }
				}

				public static MultilingualString Location
				{
					get { return ResString.GetMultilingualString("143E9A41-1BC9-4476-9AF7-A5B603710F20", "Location"); }
				}

				public static MultilingualString Department
				{
					get { return ResString.GetMultilingualString("8DF14E66-E695-4B7C-A6A3-EC1A6C5CD891", "Department"); }
				}

				public static MultilingualString Service
				{
					get { return ResString.GetMultilingualString("F8595A6E-402A-4E82-B81E-2BCADA46D14B", "Service"); }
				}

				public static MultilingualString Reason
				{
					get { return ResString.GetMultilingualString("5F871BF3-73D0-43FE-8B55-37D780FBA18A", "Reason"); }
				}

				public static MultilingualString MessageType
				{
					get { return ResString.GetMultilingualString("7B7EE870-45E9-4657-8272-BE1D5366DDB7", "Message Type"); }
				}

				public static MultilingualString MessageSubType
				{
					get { return ResString.GetMultilingualString("B15EB180-CB1C-49CC-A309-6D4B2079ADDB", "Message Sub Type"); }
				}

				public static MultilingualString Old
				{
					get { return ResString.GetMultilingualString("2A98BE9D-0611-4653-AF50-22B9B68993E5", "Old"); }
				}

				public static MultilingualString New
				{
					get { return ResString.GetMultilingualString("6B55FF38-1526-4AD0-B910-5FA48268AA71", "New"); }
				}

				public static MultilingualString Type
				{
					get { return ResString.GetMultilingualString("AFC8035F-C724-42C8-A106-85A8050807C8", "Type"); }
				}

				public static MultilingualString Name
				{
					get { return ResString.GetMultilingualString("5C2E647F-62B0-40D6-A8AA-49F61712D98B", "Name"); }
				}

				public static MultilingualString Partial
				{
					get { return ResString.GetMultilingualString("5233A9EE-60BB-4F32-B101-D59E74A9BB8D", "Partial"); }
				}

				public static MultilingualString Total
				{
					get { return ResString.GetMultilingualString("58A27661-7D94-48BF-8F99-70FCD274F0FB", "Total"); }
				}

				public static MultilingualString VoyageFlightNumber
				{
					get { return ResString.GetMultilingualString("0F820551-05B6-4A87-9477-69B7F857939B", "Voyage/Flight Number"); }
				}

				public static MultilingualString FlightDate
				{
					get { return ResString.GetMultilingualString("EF4C610D-A092-41FA-A92E-86623177DAC7", "Flight Date"); }
				}

				public static MultilingualString EstimatedTimeOfArrival
				{
					get { return ResString.GetMultilingualString("90ca164b-64e6-46f6-b716-6dba432445e7", "Estimated Time Of Arrival"); }
				}

				public static MultilingualString EquipmentReferenceNumber
				{
					get { return ResString.GetMultilingualString("606B222B-70FF-45C1-A165-3B3AEB9FCAEA", "Equipment Reference Number"); }
				}

				public static MultilingualString CustomsReferenceNumber
				{
					get { return ResString.GetMultilingualString("1D4C8A94-7983-468D-A473-0F80AFEF0E34", "Customs Reference Number"); }
				}

				public static MultilingualString ReferenceNumber
				{
					get { return ResString.GetMultilingualString("8805e82b-06d3-43ba-a963-9a1912fb690e", "Reference Number"); }
				}

				public static MultilingualString RequestNumber
				{
					get { return ResString.GetMultilingualString("6b8a6e44-0502-444e-8b5e-42ca003d38b0", "Request Number"); }
				}

				public static MultilingualString ExternalDocumentType
				{
					get { return ResString.GetMultilingualString("bb71ea13-ad26-4502-85a2-3b8b8140f033", "External Document Type"); }
				}

				public static MultilingualString ReceiptNumber
				{
					get { return ResString.GetMultilingualString("be7b2004-2c28-4bf4-b2f9-76a7583a63bd", "Receipt Number"); }
				}

				public static MultilingualString Weight
				{
					get { return ResString.GetMultilingualString("e8542483-805a-4bc5-bc6b-4fc05ca7ce5b", "Weight"); }
				}

				public static MultilingualString Length
				{
					get { return ResString.GetMultilingualString("a5b0f6b4-1ebd-4eda-ba85-4c2007d4eb1b", "Length"); }
				}

				public static MultilingualString Width
				{
					get { return ResString.GetMultilingualString("790df739-b23f-4dd0-8b33-356c34a111b2", "Width"); }
				}

				public static MultilingualString Height
				{
					get { return ResString.GetMultilingualString("0f3d7fdd-277a-4605-86f2-2ff24aa8b836", "Height"); }
				}

				public static MultilingualString Volume
				{
					get { return ResString.GetMultilingualString("7786ffd1-579a-4826-b579-ef2a8a7c6f62", "Volume"); }
				}

				public static MultilingualString Mode
				{
					get { return ResString.GetMultilingualString("133870a2-27e9-4edb-9a89-d92d5bf45794", "Mode"); }
				}

				public static MultilingualString Quantity
				{
					get { return ResString.GetMultilingualString("937f14a4-aa19-4087-8077-363db7ac50c7", "Quantity"); }
				}

				public static MultilingualString InnerPackQuantity
				{
					get { return ResString.GetMultilingualString("f9a7e26e-ea7f-4c01-b05d-d487f3d0b3c3", "Inner Pack Quantity"); }
				}

				public static MultilingualString OuterPackQuantity
				{
					get { return ResString.GetMultilingualString("BD9A54EC-8616-49BC-BC7B-C395BBF0A8C9", "Outer Pack Quantity"); }
				}

				public static MultilingualString Status
				{
					get { return ResString.GetMultilingualString("8475ea67-7f57-4579-80ec-58ed753f41d2", "Status"); }
				}

				public static MultilingualString Score
				{
					get { return ResString.GetMultilingualString("09440B54-9242-49CD-9AA9-BF402411E735", "Score"); }
				}

				public static MultilingualString Company
				{
					get { return ResString.GetMultilingualString("9C4C43D0-23FC-4922-951C-4A3A36494230", "Company"); }
				}

				public static MultilingualString TriggerCode
				{
					get { return ResString.GetMultilingualString("5ee3446a-e1a1-4bd5-bffe-4ce192f25934", "Trigger Code"); }
				}

				public static MultilingualString JobNumber
				{
					get { return ResString.GetMultilingualString("97905e64-5030-4544-8f89-767c5506aad2", "Job Number"); }
				}

				public static MultilingualString EventCode
				{
					get { return ResString.GetMultilingualString("8eff7456-078c-47d3-9838-288512b39e70", "Event Code"); }
				}

				public static MultilingualString Description
				{
					get { return ResString.GetMultilingualString("1793d30d-5645-455b-84a6-cd3d4897fd19", "Description"); }
				}

				public static MultilingualString Warehouse
				{
					get { return ResString.GetMultilingualString("b59385f3-55ac-4585-a0d8-14c5a59349f1", "Warehouse"); }
				}

				public static MultilingualString TaskCode
				{
					get { return ResString.GetMultilingualString("f3390b78-5155-4102-ac11-118051cca2a8", "Task Code"); }
				}

				public static MultilingualString MilestoneCode
				{
					get { return ResString.GetMultilingualString("01a7aaef-2b52-42d1-bcb6-1dccb4fe41b3", "Milestone Code"); }
				}

				public static MultilingualString MilestoneMergedCode
				{
					get { return ResString.GetMultilingualString("99dd5d4c-dbe9-4670-8cd0-705a1b4bb5e2", "Milestone Merged Code"); }
				}

				public static MultilingualString TriggerMergedCode
				{
					get { return ResString.GetMultilingualString("4103cf5a-14b4-4b9b-a16a-85b0d761f700", "Trigger Merged Code"); }
				}

				public static MultilingualString MachineNameCode
				{
					get { return ResString.GetMultilingualString("05475a46-97c3-4bfd-8576-27a7bdd3532b", "Machine Name Code"); }
				}

				public static MultilingualString ProcessIDCode
				{
					get { return ResString.GetMultilingualString("d747bee1-e952-4be9-a201-f8fbf99a014a", "Process IDC ode"); }
				}

				public static MultilingualString ThreadIDCode
				{
					get { return ResString.GetMultilingualString("7bafad45-771a-4108-8c85-925de73c815b", "Thread ID Code"); }
				}

				public static MultilingualString Startable
				{
					get { return ResString.GetMultilingualString("0eb31613-4a87-422a-b892-ec0f436bda1a", "Startable"); }
				}

				public static MultilingualString Assigned
				{
					get { return ResString.GetMultilingualString("64ba81b8-0133-4cb0-8733-7cbf19ebb00e", "Assigned"); }
				}

				public static MultilingualString To
				{
					get { return ResString.GetMultilingualString("e6ddace9-43ae-4357-9bb6-46445381e5a8", "To"); }
				}

				public static MultilingualString From
				{
					get { return ResString.GetMultilingualString("2eb6e84f-0547-4803-87be-d435cc972b5b", "From"); }
				}

				public static MultilingualString ChangeMode
				{
					get { return ResString.GetMultilingualString("016de6f5-c9b3-4da5-9b58-c36d82dd10c1", "Change Mode"); }
				}

				public static MultilingualString PenetrationPercentage
				{
					get { return ResString.GetMultilingualString("9378dcfc-4182-47b7-a34c-86055fff4b43", "Penetration Percentage"); }
				}

				public static MultilingualString WorkflowPenetration
				{
					get { return ResString.GetMultilingualString("7e4de9d7-db8b-4733-8469-747bf219dc29", "Workflow Penetration"); }
				}

				public static MultilingualString TaskPenetration
				{
					get { return ResString.GetMultilingualString("da59fdc4-c67f-4328-9fb0-ff16abeea191", "Task Penetration"); }
				}

				public static MultilingualString WorkflowZone
				{
					get { return ResString.GetMultilingualString("e6852fd7-7253-4dc6-8ae1-209e888383eb", "Workflow Zone"); }
				}

				public static MultilingualString DocumentSource
				{
					get { return ResString.GetMultilingualString("02031224-9154-4159-85b6-8e4cfe528bd0", "Document Source"); }
				}

				public static MultilingualString ServiceTask
				{
					get { return ResString.GetMultilingualString("0a32b02b-70ed-4bbd-ab70-35b73001532d", "Service Task"); }
				}

				public static MultilingualString CommunicationStatus
				{
					get { return ResString.GetMultilingualString("881c6ee8-6d97-4c59-8bb2-16203a561fda", "Communication Status"); }
				}

				public static MultilingualString Link
				{
					get { return ResString.GetMultilingualString("de120a15-2c5a-4fe5-b306-e257d9a5eec2", "Link"); }
				}

				public static MultilingualString Group
				{
					get { return ResString.GetMultilingualString("02e73c6b-6945-4461-b24f-ea22bd455e98", "Group"); }
				}

				public static MultilingualString Staff
				{
					get { return ResString.GetMultilingualString("22687661-9425-472e-97eb-2bff3f051bd5", "Staff"); }
				}

				public static MultilingualString Capability
				{
					get { return ResString.GetMultilingualString("F044B9F6-6F03-40D2-B05B-9351D19C07C6", "Capability"); }
				}

				public static MultilingualString Tag
				{
					get { return ResString.GetMultilingualString("2380efec-eec1-4c22-bee1-3f6f2ed26816", "Tag"); }
				}

				public static MultilingualString Action
				{
					get { return ResString.GetMultilingualString("cdbc260c-f508-48da-85e2-5b4c1f2b5d65", "Action"); }
				}

				public static MultilingualString Rule
				{
					get { return ResString.GetMultilingualString("1768b2e0-8650-44c7-94fd-0aef38289e06", "Rule"); }
				}

				public static MultilingualString MasterBillShipper
				{
					get { return ResString.GetMultilingualString("cf05d7b9-d8aa-4cbf-8422-9378ebd5817a", "Master Bill Shipper"); }
				}

				public static MultilingualString LogSubscriber
				{
					get { return ResString.GetMultilingualString("b20f3de3-0b98-4d63-8a4b-e89f5d557d12", "Log Subscriber"); }
				}

				public static MultilingualString DeclarationID
				{
					get { return ResString.GetMultilingualString("e50cc583-7d81-4fc4-b638-0467d53de78f", "Declaration ID"); }
				}

				public static MultilingualString TradeBalanceService
				{
					get { return ResString.GetMultilingualString("7a2984f5-3e38-490c-924a-832a5307a0b7", "Trade Balance Service"); }
				}

				public static MultilingualString FieldChange
				{
					get { return ResString.GetMultilingualString("d13fd178-6aa9-489c-90d0-4527120ced50", "Field Change"); }
				}

				public static MultilingualString MessageSyntaxOrBusinessRuleErrors
				{
					get { return ResString.GetMultilingualString("82b87fd1-67a0-4af1-a110-89fb1160b4a7", "Message Syntax Or Business Rule Errors"); }
				}

				public static MultilingualString What
				{
					get { return ResString.GetMultilingualString("ba3b78c7-3670-4623-949f-fd849fcaf0f7", "What"); }
				}

				public static MultilingualString CapacityConstraintedResource
				{
					get { return ResString.GetMultilingualString("fb7adc24-7c08-49b8-a289-6ac2d14ada1d", "Capacity Constrained Resource"); }
				}

				public static MultilingualString ReleaseGroup
				{
					get { return ResString.GetMultilingualString("0af01ae7-914c-4930-8d8e-17dd06e791a6", "Release Group"); }
				}

				public static MultilingualString ContentID
				{
					get { return ResString.GetMultilingualString("9331919a-2be1-4c71-970f-b1f4c0903ef3", "Content ID"); }
				}

				public static MultilingualString Argument
				{
					get { return ResString.GetMultilingualString("30ee5b65-2e64-48d7-8df2-06654e1370e7", "Argument"); }
				}

				public static MultilingualString ParentPrimaryKey
				{
					get { return ResString.GetMultilingualString("f3f403e5-3a00-4118-a034-83c524008ed4", "Parent Primary Key"); }
				}

				public static MultilingualString TriggerAction
				{
					get { return ResString.GetMultilingualString("3d37b907-cdc9-4323-8575-a57e18ac6159", "Trigger Action"); }
				}

				public static MultilingualString ParentType
				{
					get { return ResString.GetMultilingualString("ad3bbf3a-0b88-4228-8ec3-d8a04e5154a2", "Parent Type"); }
				}

				public static MultilingualString TimeOffset
				{
					get { return ResString.GetMultilingualString("9535CA83-A233-4365-BF06-729EBD4D2FC9", "Time Offset"); }
				}

				public static MultilingualString Estimate
				{
					get { return ResString.GetMultilingualString("C9417103-6AF8-47BB-A3DF-B5155A45DF28", "Estimate"); }
				}

				public static MultilingualString User
				{
					get { return ResString.GetMultilingualString("DCA9DF16-ACB2-45A8-9901-F1C10BD392FA", "User"); }
				}

				public static MultilingualString Branch
				{
					get { return ResString.GetMultilingualString("BA5C2111-9DDB-4C35-A0B0-EF3C1A9DC708", "Branch"); }
				}

				public static MultilingualString Maximum
				{
					get { return ResString.GetMultilingualString("3C3A1EB8-0E00-4054-AD57-4F652CA59BA6", "Maximum"); }
				}

				public static MultilingualString File
				{
					get { return ResString.GetMultilingualString("6E50C1AD-A976-4054-B476-DBBA719F1C38", "File"); }
				}

				public static MultilingualString Product
				{
					get { return ResString.GetMultilingualString("10DFA703-A8E0-41B2-B8D5-B4C4220A0F4B", "Product"); }
				}

				public static MultilingualString ProductArea
				{
					get { return ResString.GetMultilingualString("61E8C448-3BCC-4485-877F-1BEC9A77E2C2", "Product Area"); }
				}

				public static MultilingualString Module
				{
					get { return ResString.GetMultilingualString("58782A83-CF72-41C0-82F3-A66B375851FB", "Module"); }
				}

				public static MultilingualString SourceModuleId
				{
					get { return ResString.GetMultilingualString("16B9B279-0A04-454B-80E8-A6A4B984B8E6", "Source Module Id"); }
				}

				public static MultilingualString Packages
				{
					get { return ResString.GetMultilingualString("9c0f916a-2434-465e-a5a6-7fac876aa311", "Packages"); }
				}

				public static MultilingualString InterchangeNumber
				{
					get { return ResString.GetMultilingualString("15C7A5B1-6AD3-4AF2-9B63-16BB63537704", "Interchange Number"); }
				}

				public static MultilingualString Organization
				{
					get { return ResString.GetMultilingualString("fab9a120-bc6a-4b52-8f94-7426880b2fbb", "Organization"); }
				}

				public static MultilingualString Email
				{
					get { return ResString.GetMultilingualString("4a78e884-e407-4db1-a819-b12a5c066337", "Email"); }
				}

				public static MultilingualString HouseBill
				{
					get { return ResString.GetMultilingualString("813E0F44-98CD-4A01-9E5F-D032318D93C6", "House Bill"); }
				}

				public static MultilingualString MasterBill
				{
					get { return ResString.GetMultilingualString("CFC53E42-C3CC-4D72-88A4-173A1DFD3FE4", "Master Bill"); }
				}

				public static MultilingualString Count
				{
					get { return ResString.GetMultilingualString("6558F583-5138-43AC-9665-9D6D77197DDF", "Count"); }
				}

				public static MultilingualString Requested
				{
					get { return ResString.GetMultilingualString("65D37BE2-2D89-4EFF-AEA6-28CB8D732EE8", "Requested"); }
				}

				public static MultilingualString IsoCode
				{
					get { return ResString.GetMultilingualString("46843072-becb-089c-4461-30c8e2438fd6", "ISO Code"); }
				}

				public static MultilingualString Direction
				{
					get { return ResString.GetMultilingualString("8dfffbd4-87bb-07b0-47a7-a2ab40ea7304", "Direction"); }
				}

				public static MultilingualString VehicleRegistration
				{
					get { return ResString.GetMultilingualString("ba2dcc09-60af-1e86-43f4-9a2ab847f428", "Vehicle Registration"); }
				}

				public static MultilingualString GateInReference
				{
					get { return ResString.GetMultilingualString("f68d6a25-ccf2-1c87-489a-1417a63d784a", "Gate In Reference"); }
				}

				public static MultilingualString GateOutReference
				{
					get { return ResString.GetMultilingualString("0ce756a8-959a-1a92-42ae-c53861d2f36a", "Gate Out Reference"); }
				}

				public static MultilingualString DriverLicense
				{
					get { return ResString.GetMultilingualString("65114935-a013-a998-4102-dadbdbdc0f6f", "Driver License"); }
				}

				public static MultilingualString DriverName
				{
					get { return ResString.GetMultilingualString("da62b344-c528-d4b0-4b66-eb6823830134", "Driver Name"); }
				}

				public static MultilingualString ContentAvailable
				{
					get { return ResString.GetMultilingualString("8802a62d-9e7b-401d-9ad3-4ea0cf333067", "Content Available"); }
				}

				public static MultilingualString Priority
				{
					get { return ResString.GetMultilingualString("6ca54167-6032-4727-a8b1-3bca520373a5", "Priority"); }
				}

				public static MultilingualString UnNumber
				{
					get { return ResString.GetMultilingualString("9ec052c2-57dd-4b44-8a6d-130ed1088d07", "UN Number"); }
				}

				public static MultilingualString Standard
				{
					get { return ResString.GetMultilingualString("5d6bcb3e-3b4f-47a2-94a4-7296e9cd00f8", "Standard"); }
				}

				public static MultilingualString ContractNumber
				{
					get { return ResString.GetMultilingualString("0089859c-974d-4b09-9ea0-c44ab3f6c44f", "Contract Number"); }
				}
				public static MultilingualString StartGrade
				{
					get { return ResString.GetMultilingualString("1e7417a4-3c2e-475f-a05d-833cb431f111", "Starting Grade"); }
				}
				public static MultilingualString EndGrade
				{
					get { return ResString.GetMultilingualString("5ddb7fae-d21d-45fc-969e-bdeeecaef9b9", "End Grade"); }
				}

				public static MultilingualString Currency
				{
					get { return ResString.GetMultilingualString("9590DB0F-E116-4381-84F0-91A437BFE171", "Currency"); }
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Department type")]
		public static class Departments
		{
			public const string Warehouse = "Warehouse";
			public const string TransportProvider = "Transport Provider";
			public const string TransitWarehouse = "Transit Warehouse";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "EventReferenceParameterTypes")]
		public static class EventReferenceActionAuthorisedTypes
		{
			public const string AuthorisedToLeave = "Authorised To Leave";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "EventReferenceParameterTypes")]
		public static class EventReferenceParameterTypes
		{
			public const string Adjustment = "Adjustment";
			public const string AutoPack = "AutoPack";
			public const string BulkCarrierLabelBooking = "Bulk Carrier Label Booking";
			public const string Cartonization = "Cartonization";
			public const string Consol = "Consol";
			public const string Container = "Container";
			public const string ContainerID = "ContainerID";
			public const string ContainerMovement = "Container Movement";
			public const string ContainerTracking = "Container Tracking";
			public const string Complete = "COMPLETE";
			public const string AWBAutomation = "AWB Automation";
			public const string ScheduleFeed = "Schedule Feed";
			public const string EmptyContainer = "EMT";
			public const string FullContainer = "FUL";
			public const string HoldCode = "Hold Code";
			public const string LastFreeDay = "Last Free Day";
			public const string Order = "Order";
			public const string Organisation = "Organisation";
			public const string PackType = "Pack Type";
			public const string Partial = "PARTIAL";
			public const string Pick = "Pick";
			public const string Product = "Product";
			public const string PutawayJob = "Putaway Job";
			public const string Receive = "Receive";
			public const string RequiredFrom = "Required From";
			public const string RequiredTo = "Required To";
			public const string RunSheet = "Run Sheet";
			public const string Stocktake = "Stocktake";
			public const string Shipment = "Shipment";
			public const string Transport = "Transport";
			public const string PickupTransport = "Pickup Transport";
			public const string DeliveryTransport = "Delivery Transport";
			public const string Transfer = "Transfer";
			public const string WorkOrder = "Work Order";
			public const string DynamicWorkOrder = "Dynamic Work Order";
			public const string AVSQuery = "AVS QUERY";
			public const string ShipmentPreAdvice = "Shipment Pre Advice";
			public const string ImportPreAdvice = "Import Pre Advice";
			public const string ExportPreAdvice = "Export Pre Advice";
			public const string CompanyCode = "COM";
			public const string LPDNotification = "LPD Notification";
			public const string LDENotification = "LDE Notification";
			public const string WaveCreation = "WaveCreation";
			public const string CommercialInvoice = "Commercial Invoice";
			public const string ShipmentVisibility = "Shipment Visibility";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "EventReferenceParameterTypes")]
		public static class EventReferenceParameterReasons
		{
			public const string Accepted = "Accepted";
			public const string AmendmentProcessing = "Amendment Processing";
			public const string AuditFailed = "Audit Failed";
			public const string AuditPassed = "Audit Passed";
			public const string BadUnitConversion = "Bad Unit Conversion";
			public const string BOMProductPickedWithoutWorkOrder = "BOM Product picked without Work Order";
			public const string CargoReporting = "Cargo Reporting";
			public const string CargoReportCreated = "Cargo Report Created";
			public const string CargoReportAmendPending = "Cargo Report Amend Pending";
			public const string CargoReportAmended = "Cargo Report Amended";
			public const string ContainerNumberAdvised = "Container Number Advised";
			public const string Delivery = "Delivery";
			public const string ElectronicBookingReceived = "Electronic Booking Received";
			public const string ElectronicShippingInstructionReceived = "Electronic Shipping Instruction Received";
			public const string FailedToCartonize = "Failed To Cartonize";
			public const string Futile = "Futile";
			public const string LineHasBeenPicked = "Line has been picked";
			public const string LineHasBeenReleaseCaptured = "Line has been Release Captured";
			public const string NoCartonGroup = "No Carton Group";
			public const string NoUOMType = "No UOM Type";
			public const string Outturn = "Outturn";
			public const string Pack = "Pack";
			public const string Pickup = "Pickup";
			public const string PreScreened = "Pre-Screened";
			public const string Rejected = "Rejected";
			public const string Unpack = "Unpack";
			public const string Shortfall = "Shortfall";
			public const string NotPickable = "NotPickable";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "EventReferenceParameterTypes")]
		public static class EventReferenceMessageTypes
		{
			public const string CargoRelease = "Cargo Release";
			public const string AirBooking = "Air Booking";
			public const string ImportReleaseOrder = "Import Release Order";
			public const string ImportReleaseOrderWithdrawal = "Import Release Order Withdrawal";
			public const string LoadManifest = "Load Manifest";
			public const string DangerousGoodsLoadManifest = "Dangerous Goods Load Manifest";
			public const string DischargeManifest = "Discharge Manifest";
			public const string DangerousGoodsDischargeManifest = "Dangerous Goods Discharge Manifest";
			public const string DangerousGoodsTransitManifest = "Dangerous Goods Transit Manifest";
			public const string LoadManifestReplacement = "Load Manifest Replacement";
			public const string DangerousGoodsLoadManifestReplacement = "Dangerous Goods Load Manifest Replacement";
			public const string DischargeManifestReplacement = "Discharge Manifest Replacement";
			public const string DangerousGoodsDischargeManifestReplacement = "Dangerous Goods Discharge Manifest Replacement";
			public const string DangerousGoodsTransitManifestReplacement = "Dangerous Goods Transit Manifest Replacement";
			public const string LoadManifestCancellation = "Load Manifest Cancellation";
			public const string DangerousGoodsLoadManifestCancellation = "Dangerous Goods Load Manifest Cancellation";
			public const string DischargeManifestCancellation = "Discharge Manifest Cancellation";
			public const string DangerousGoodsDischargeManifestCancellation = "Dangerous Goods Discharge Manifest Cancellation";
			public const string DangerousGoodsTransitManifestCancellation = "Dangerous Goods Transit Manifest Cancellation";
			public const string ShipmentStatus = "Shipment Status";
			public const string JobStatus = "Job Status";
			public const string VerifiedGrossContainerWeight = "Verified Gross Container Weight";
			public const string ResetToOriginal = "Reset To Original";
			public const string ExportPreAdviceNotification = "Export Pre-Advice Notification";
			public const string CarrierBookingAgent = "Carrier Booking Agent";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "EventReferenceParameterTypes")]
		public static class EventReferenceParameterTriggers
		{
			public const string OperationalActions = "Operational Actions";
			public const string Documents = "Documents";
			public const string Workflow = "Workflow";
			public const string Menu = "Menu";
			public const string Api = "Api";
		}

		// REMEMBER TO UPDATE Contracts in OrgSupBuyLinkTrnMode
		public static class TransportModes
		{
			public const string Air = "AIR";
			public const string Sea = "SEA";
			public const string AirSea = "FAS";
			public const string SeaAir = "FSA";
			public const string Road = "ROA";
			public const string Rail = "RAI";
			public const string Mail = "MAI";
			public const string Courier = "COU";
			public const string Other = "OTH";
			public const string Unknown = "UNK";
			public const string All = "ALL";
			public const string Storage = "STO";
			public const string WarehouseHandling = "HND";
			public const string BorderWaterBorne = "BWB";
			public const string Truck = "TRK";
			public const string Auto = "AUT";
			public const string Pedestrian = "PED";
			public const string PassengerHandCarried = "PHC";
			public const string FixedTransportInstallations = "FIX";
			public const string OwnPropulsion = "OWN";
			public const string InlandWaterwayTransport = "IWT";
			public const string RollOnRollOff = "ROR";
		}

		public static class TransportModeDescriptions
		{
			public static MultilingualString Air { get { return ResString.GetMultilingualString("Common|TransportMode|Air", "Air Freight"); } }
			public static MultilingualString Sea { get { return ResString.GetMultilingualString("Common|TransportMode|Sea", "Sea Freight"); } }
			public static MultilingualString AirSea { get { return ResString.GetMultilingualString("Common|TransportMode|AirSea", "First by Air then by Sea Freight"); } }
			public static MultilingualString SeaAir { get { return ResString.GetMultilingualString("Common|TransportMode|SeaAir", "First by Sea then by Air Freight"); } }
			public static MultilingualString Road { get { return ResString.GetMultilingualString("Common|TransportMode|Road", "Road Freight"); } }
			public static MultilingualString Rail { get { return ResString.GetMultilingualString("Common|TransportMode|Rail", "Rail Freight"); } }
			public static MultilingualString Mail { get { return ResString.GetMultilingualString("Common|TransportMode|Mail", "Post/Mail"); } }
			public static MultilingualString Courier { get { return ResString.GetMultilingualString("Common|TransportMode|Courier", "Courier"); } }
			public static MultilingualString Other { get { return ResString.GetMultilingualString("Common|TransportMode|Other", "Other"); } }
			public static MultilingualString Unknown { get { return ResString.GetMultilingualString("Common|TransportMode|Unknown", "Unknown"); } }
			public static MultilingualString All { get { return ResString.GetMultilingualString("Common|TransportMode|All", "All"); } }
			public static MultilingualString Storage { get { return ResString.GetMultilingualString("Common|TransportMode|Storage", "Storage"); } }
			public static MultilingualString FixedTransportInstallations { get { return ResString.GetMultilingualString("Common|TransportMode|FixedTransportInstallations", "Fixed Transport Installations"); } }
			public static MultilingualString InlandWaterwayTransport { get { return ResString.GetMultilingualString("Common|TransportMode|InlandWaterwayTransport", "Inland Waterways"); } }
			public static MultilingualString OwnPropulsion { get { return ResString.GetMultilingualString("Common|TransportMode|OwnPropulsion", "Own Propulsion"); } }
			public static MultilingualString RollOnRollOff { get { return ResString.GetMultilingualString("Common|TransportMode|RollOnRollOff", "Roll On Roll Off Freight"); } }
		}

		public static class TransportCodes
		{
			public const string Sea = "1";
			public const string Rail = "2";
			public const string Road = "3";
			public const string Air = "4";
			public const string Mail = "5";
		}

		public static class TransportParentTypes
		{
			public const string Consol = "CON";
			public const string ImporterSecurityFiling = "ISF";
			public const string Shipment = "SHP";
			public const string ShipmentPreAdvice = "SPA";
			public const string Declaration = "DEC";
			public const string AgencyShipment = "ASH";
			public const string CommercialInvoice = "INV";
			public const string TransportBooking = "DTB";
			public const string AsycudaManifest = "ASY";
			public const string TransitReceiveConsignment = "WRC";
			public const string TransitReceiveASN = "WRP";
			public const string TransitDispatchLoadList = "WDL";
		}

		public static class TransportStatus
		{
			public const string Planned = "PLN";
			public const string Confirmed = "CNF";
			public const string Held = "HLD";
			public const string Requested = "RQD";
			public const string CancellationRequested = "CRQ";
			public const string Unable = "UBL";
			public const string Queued = "QUE";
			public const string FlightNotOperating = "FNO";
			public const string Cancelled = "CAN";
		}

		public static class TransportStatusDescriptions
		{
			public static MultilingualString Planned { get { return ResString.GetMultilingualString("95d7c473-29fd-42e5-8b7a-d075cf6d52e0", "Planned"); } }
			public static MultilingualString Confirmed { get { return ResString.GetMultilingualString("d78cea01-b86c-4ea6-910c-96def8e291cd", "Confirmed"); } }
			public static MultilingualString Held { get { return ResString.GetMultilingualString("740bc29b-a0c1-406a-936a-4d29b17fc127", "Held - See Note"); } }
			public static MultilingualString Requested { get { return ResString.GetMultilingualString("e022df6f-51ee-4496-be5f-c17a73d62545", "Requested"); } }
			public static MultilingualString CancellationRequested { get { return ResString.GetMultilingualString("54b7fa2b-119b-44f4-a5c4-1ce27ade2390", "Cancellation Requested"); } }
			public static MultilingualString Unable { get { return ResString.GetMultilingualString("225f65dd-f5ca-4ec6-8e67-1d24f2bacc50", "Unable"); } }
			public static MultilingualString Queued { get { return ResString.GetMultilingualString("194f9666-16bc-4778-9527-08402476fa92", "Queued"); } }
			public static MultilingualString FlightNotOperating { get { return ResString.GetMultilingualString("8d655079-f623-469f-a58a-4dfeeb53a5bf", "Flight Not Operating"); } }
			public static MultilingualString Cancelled { get { return ResString.GetMultilingualString("32ceaa47-710d-4825-a084-4243bed939d3", "Canceled"); } }
		}

		public static class OrgRoles
		{
			public const string LocalClient = "LOC";
			public const string OverseasAgent = "OAG";
		}

		public static class PortDirection
		{
			public const string Load = "LOAD";
			public const string Transit = "TRANSIT";
			public const string Discharge = "DISCHARGE";
		}

		public static class CarrierCategory
		{
			public const string PreferredAll = "PRF";
			public const string PreferredThisCountry = "PRC";
			public const string Secondary = "SEC";
			public const string Occasional = "USE";
			public const string DoNotUse = "DNU";
		}

		public static class ServicesCategory
		{
			public const string Preferred = "PRF";
			public const string Secondary = "SEC";
			public const string Occasional = "USE";
			public const string DoNotUse = "DNU";
		}

		#region Bool Char/Strings

		public const string BooleanTrueString = "Y";
		public const string BooleanFalseString = "N";
		public const char BooleanTrueChar = 'Y';
		public const char BooleanFalseChar = 'N';

		#endregion

		public static class AccountType
		{
			public const string BalanceSheetAccount = "BSH";
			public const string ProfitAndLossAccount = "P&L";
			public const string Total = "TTL";
			public const string Header = "HDR";
			public const string OpeningBalance = "OBA";
			public const string ClosingBalance = "CBA";
			public const string Consolidation = "CLN";
			public const string Alternate = "ALT";
			public const string Note = "NTE";
			public const string Undefined = "XXX";
			public const string ChartOnly = "CHT";
			public const string Rollup = "RUP";
			public const string Group = "GRP";
		}

		public static class SubAccountType
		{
			public const string Organization = "ORG";
			public const string SalesGroup = "SEG";
			public const string StaffAndResources = "STR";
			public const string StaffGroup = "SGP";
		}

		public static class SubAccountTypeDescriptions
		{
			public static MultilingualString Organization { get { return ResString.GetMultilingualString("346d5560-41db-4bd6-911d-9907c4f57264", "Organization"); } }
			public static MultilingualString SalesGroup { get { return ResString.GetMultilingualString("38da6497-9198-4cfb-8e14-d5eb6493110c", "Sales/Expense Groups"); } }
			public static MultilingualString StaffAndResources { get { return ResString.GetMultilingualString("307f0168-50fe-4c92-b87a-896f1e783a01", "Staff and Resources"); } }
			public static MultilingualString StaffGroup { get { return ResString.GetMultilingualString("4f774fec-0830-4ad7-9d59-707b24696c46", "Staff Group"); } }
		}

		public static class ChargeType
		{
			public const string Disbursement = "DSB";
			public const string Margin = "MRG";
			public const string NonAccrual = "NON";
			public const string Overhead = "OVR";
			public const string Revenue = "REV";
			public const string Comment = "CMT";
			public const string ManualJobAccrual = "MJA";
		}

		public static class ComplianceDocumentStatus
		{
			public const string Added = "ADD";
			public const string Voided = "VOD";
			public const string NumberSet = "SET";
			public const string Finalised = "FIN";
			public const string FinalisedSpecialVoided = "VAF";
		}

		public static class ComplianceRollupBehaviourType
		{
			public const string SinglePageSummarize = "SSM";
			public const string SinglePageReferAttached = "SRA";
			public const string MultiPageNoLimitation = "MNL";
		}

		public static class ComplianceBookAllocationLevel
		{
			public const string Company = "COM";
			public const string Branch = "BRN";
			public const string BranchDepartment = "BDP";
			public const string Counter = "CTR";
		}

		public static class StatementCollectionLetterType
		{
			public const string StatementOfAccount = "SOA";
			public const string FirstReminder = "1RM";
			public const string SecondReminder = "2RM";
			public const string CollectionLetter = "COL";
			public const string DemandLetter = "DEM";
		}

		[CodeAlive("Used in AU CONTRLMessageProcessor")]
		public enum AUCTLMessageSubType { ACK, REJ }

		[CodeAlive("Used in EXDRMessageProcessor")]
		public enum EXDRMessageSubType { CAN, CLR, ERR, REJ, REV, WDW }

		public static class ACPeriodFormat
		{
			public const string Month = "CAL";
			public const string FourFourFive = "445";
			public const string FourWeeks = "4WK";
			public const string Weeks = "1WK";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "PaymentRemittancePrintOption")]
		public static class PaymentRemittancePrintOption
		{
			public const string PrintPaymentVoucher = "Print Payment Voucher";
			public const string PrintRemittanceAdvice = "Print Remittance Advice";
			public const string PrintBoth = "Print Both";
		}

		public static class Groups
		{
			public const string ALL = "ALL";
			public static readonly Guid AllPK = new Guid("94755E71-A87A-4034-8DFA-785773A49607");
			public static readonly Guid PostMastersGroupPK = new Guid("55896E13-12BE-4FD4-AC94-2956795A5BE2");
		}

		public static class Genders
		{
			public const string Man = "M";
			public const string Woman = "F";
			public const string NonBinary = "Q";
			public const string Agender = "A";
			public const string Custom = "C";
			public const string NotSpecified = "N";
		}

		public static class GenderDescriptions
		{
			public static MultilingualString Man { get { return ResString.GetMultilingualString("605ED290-E42B-4E96-AD6F-E2512CAA0743", "Man"); } }
			public static MultilingualString Woman { get { return ResString.GetMultilingualString("E70A69E6-78C2-4C99-987F-5901C75F2568", "Woman"); } }
			public static MultilingualString NotSpecified { get { return ResString.GetMultilingualString("25CB4E99-F9D0-4631-B19A-DA24A754D4BC", "Not Specified"); } }
			public static MultilingualString Agender { get { return ResString.GetMultilingualString("8CA2BBA5-2FC2-42C0-929F-77360402D245", "Agender"); } }
			public static MultilingualString Custom { get { return ResString.GetMultilingualString("38A0851A-9D04-410A-A17D-D171962AC97E", "Custom"); } }
			public static MultilingualString NonBinary { get { return ResString.GetMultilingualString("266262ff-27dd-4ba1-a717-fd1e84664122", "Non-Binary"); } }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Salutation gender codes")]
		public static class SalutationGenders
		{
			public const string Man = "Man";
			public const string Woman = "Woman";
			public const string All = "All";
		}

		public static class SalutationGendersDescription
		{
			public static MultilingualString All { get { return ResString.GetMultilingualString("887cd969-d79b-4b3f-bf76-1d94b594cef8", "Suitable to all genders"); } }
			public static MultilingualString Man { get { return ResString.GetMultilingualString("f23a5e83-7b8e-4c72-95d6-0adddc12f3b4", "Suitable to man"); } }
			public static MultilingualString Woman { get { return ResString.GetMultilingualString("5284346b-c596-400b-914c-806f0ce2eacb", "Suitable to woman"); } }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Salutation gender codes")]
		public static class SalutationMacros
		{
			public const string Name = "[Name]";
			public const string JobCategory = "[JobCategory]";
		}

		public static class DefaultSalutations
		{
			#region Salutations for both genders

			public static MultilingualString DefaultSalutation
			{
				get
				{
					return ResString.GetMultilingualString("7BF330D2-AC81-4824-A1AE-D6C73C28CC78", "Dear {0}", SalutationMacros.Name);
				}
			}

			public static MultilingualString DearJobTitle
			{
				get
				{
					return ResString.GetMultilingualString("d4a2ff1b-d5f5-46e3-b3de-77eb27180147", "Dear {0}", SalutationMacros.JobCategory);
				}
			}

			public static MultilingualString HowAreYou
			{
				get
				{
					return ResString.GetMultilingualString("a0849e99-bdb7-40a6-8391-796e4f4c4532", "{0} {1}, How are you?", new object[] { Core.Constants.SalutationMacros.Name, Core.Constants.SalutationMacros.JobCategory });
				}
			}

			public static MultilingualString MyDearSirMadam
			{
				get
				{
					return ResString.GetMultilingualString("657f0aab-b110-4681-9fcc-8bfd8b7046a3", "My dear Sir/Madam");
				}
			}

			public static MultilingualString SirMadam
			{
				get
				{
					return ResString.GetMultilingualString("3b2c69c1-ea2c-47f6-aee4-a5e084c92fb3", "Sir/Madam");
				}
			}

			public static MultilingualString ToWhomItMayConcern
			{
				get
				{
					return ResString.GetMultilingualString("aa2b6061-d421-452a-9371-7fb53ef91fd5", "To whom it may concern");
				}
			}

			public static MultilingualString DearUser
			{
				get
				{
					return ResString.GetMultilingualString("95c492cb-4ad7-4e41-9532-82b9ab42859e", "Dear user");
				}
			}

			public static MultilingualString ToWhomItMayConcernHowAreYou
			{
				get
				{
					return ResString.GetMultilingualString("55f2a597-7482-48d4-bd8a-93e5dcb7983b", "To whom it may concern, How are you?");
				}
			}

			public static MultilingualString SirMadamHowAreYou
			{
				get
				{
					return ResString.GetMultilingualString("4514a9ae-9ba6-42ba-a24b-92e9628f963f", "Sir/Madam, How are you?");
				}
			}

			#endregion

			#region Salutations for Male

			public static MultilingualString DearNameMale
			{
				get
				{
					return ResString.GetMultilingualString("50a6467f-94ea-4dc8-8d66-fb8d834f46ad", "[m] Dear {0}", new object[] { Core.Constants.SalutationMacros.Name });
				}
			}

			public static MultilingualString DearProfessionNameMale
			{
				get
				{
					return ResString.GetMultilingualString("e50dafd9-4799-4a3b-8bb9-bda1195f446f", "[m] Dear {0} {1}", new object[] { Core.Constants.SalutationMacros.JobCategory, Core.Constants.SalutationMacros.Name });
				}
			}

			public static MultilingualString DearMale
			{
				get
				{
					return ResString.GetMultilingualString("90a25143-e229-40ac-9b69-b8380c2e58bb", "[m] Dear");
				}
			}

			public static MultilingualString MyDearSir
			{
				get
				{
					return ResString.GetMultilingualString("38cac5ac-0f06-49e9-9139-71642b6fc7a0", "[m] My dear Sir");
				}
			}

			public static MultilingualString Sir
			{
				get
				{
					return ResString.GetMultilingualString("1db24703-b940-46e9-b9dd-e90b86fcfbae", "[m] Sir");
				}
			}

			public static MultilingualString DearUserMale
			{
				get
				{
					return ResString.GetMultilingualString("85dab99c-86cd-482d-8306-c7efa9656367", "[m] Dear user");
				}
			}

			#endregion

			#region Salutation for Woman

			public static MultilingualString DearNameFemale
			{
				get
				{
					return ResString.GetMultilingualString("ed1f075a-174d-4a16-a1a8-59f0d754bd74", "[f] Dear {0}", new object[] { Core.Constants.SalutationMacros.Name });
				}
			}

			public static MultilingualString DearProfessionNameFemale
			{
				get
				{
					return ResString.GetMultilingualString("408229ee-fc79-4c8b-b8e0-f32c75ff06d7", "[f] Dear {0} {1}", new object[] { Core.Constants.SalutationMacros.JobCategory, Core.Constants.SalutationMacros.Name });
				}
			}

			public static MultilingualString DearFemale
			{
				get
				{
					return ResString.GetMultilingualString("ed31b963-0147-49d7-8f90-1a687df3f3b8", "[f] Dear");
				}
			}

			public static MultilingualString MyDearMadam
			{
				get
				{
					return ResString.GetMultilingualString("f33cad52-e293-4c6c-907d-73b0cf97a126", "[f] My dear Madam");
				}
			}

			public static MultilingualString Madam
			{
				get
				{
					return ResString.GetMultilingualString("cdb8a3c3-d7f4-46e7-aed4-a5837ad5b02a", "[f] Madam");
				}
			}

			public static MultilingualString DearUserFemale
			{
				get
				{
					return ResString.GetMultilingualString("7dc558f7-6424-45cd-940f-bd7501d17481", "[f] Dear user");
				}
			}

			#endregion
		}

		public static class CargoWiseOneGenCustomAddOnRuleIDs
		{
			public static readonly Guid PhoneNumberFormatValidation = new Guid("EA191CCE-5588-4E02-A81E-094ECB8846B1");
			public static readonly Guid TestGuidForWarningAcknowledgment = new Guid("E13365A6-73BF-4847-AEF6-2BA0E896B613");
		}

		public static Dictionary<Guid, ResourceString> CargoWiseOneGenCustomAddOnRuleIDsList
		{
			get
			{
				if (cargoWiseOneGenCustomAddOnRuleIDs == null)
				{
					cargoWiseOneGenCustomAddOnRuleIDs = new Dictionary<Guid, ResourceString>();
					cargoWiseOneGenCustomAddOnRuleIDs.Add(CargoWiseOneGenCustomAddOnRuleIDs.PhoneNumberFormatValidation, ResString.GetMultilingualString("ABC089DF-5CFA-4C16-B9ED-B88FC32A385A", "Phone Validation"));
					cargoWiseOneGenCustomAddOnRuleIDs.Add(CargoWiseOneGenCustomAddOnRuleIDs.TestGuidForWarningAcknowledgment, ResString.GetMultilingualString("E13365A6-73BF-4847-AEF6-2BA0E896B613", "Test Validation"));
				}
				return cargoWiseOneGenCustomAddOnRuleIDs;
			}
		}
		[ThreadStatic]
		static Dictionary<Guid, ResourceString> cargoWiseOneGenCustomAddOnRuleIDs;

		public static class WorkPermitStatuses
		{
			public const string WorkPermit = "WPR";
			public const string Residence = "RES";
			public const string SkillVisa = "SKL";
			public const string Student = "STU";
			public const string None = "NON";
		}

		public static class WorkPermitStatuseDescriptions
		{
			public static MultilingualString WorkPermit { get { return ResString.GetMultilingualString("D2859B3D-DD12-473E-935E-3E87C48B024E", "Work Permit"); } }
			public static MultilingualString Residence { get { return ResString.GetMultilingualString("9ACC04D6-3BE7-4A65-9DB0-A583C0FF1CAE", "Resident of this Country/Region"); } }
			public static MultilingualString SkillVisa { get { return ResString.GetMultilingualString("1D605FC9-8F18-4757-B688-D99569D77933", "Skill Migration Visa"); } }
			public static MultilingualString Student { get { return ResString.GetMultilingualString("9C176BA1-4D6E-4224-BA2A-8D2BF82B4D51", "Student"); } }
			public static MultilingualString None { get { return ResString.GetMultilingualString("909572CE-5068-41DE-A580-27CF49516754", "None"); } }
		}

		public static class Availabilties
		{
			public const string FullTime = "FUL";
			public const string PartTime = "PAR";
			public const string Casual = "CAS";
			public const string Contract = "CNT";
		}

		public abstract class Languages : SharedConstants.Languages
		{
		}

		public abstract class GLLanguages : Languages
		{
			public const string ZZZ_ExternalLinkToGeneralLedger = "ZZZ";
		}

		public static class ExchangeRateTypes
		{
			public static class Code
			{
				public const string BuyRate = "BUY";
				public const string SellRate = "SEL";
				public const string CustomsMeasureEURExRate = "CUD";
				public const string CustomsRate = "CUS";
				public const string CustomsRateSecondary = "CUE";
				public const string GlobalCreditControl = "GCB";
				public const string PeriodEndRate = "PER";
				public const string IATARate = "IAT";
				public const string RemBuyRate = "RMB";
				public const string RemSellRate = "RMS";
				public const string C01Rate = "C01";
				public const string C02Rate = "C02";
				public const string C03Rate = "C03";
				public const string C04Rate = "C04";
				public const string C05Rate = "C05";
				public const string C06Rate = "C06";
				public const string C07Rate = "C07";
				public const string C08Rate = "C08";
				public const string C09Rate = "C09";
				public const string C10Rate = "C10";
				public const string C11Rate = "C11";
				public const string C12Rate = "C12";
				public const string C13Rate = "C13";
				public const string C14Rate = "C14";
				public const string C15Rate = "C15";
				public const string C16Rate = "C16";
				public const string C17Rate = "C17";
				public const string C18Rate = "C18";
				public const string C19Rate = "C19";
				public const string C20Rate = "C20";
				public const string C21Rate = "C21";
				public const string C22Rate = "C22";
				public const string C23Rate = "C23";
				public const string C24Rate = "C24";
				public const string C25Rate = "C25";
				public const string C26Rate = "C26";
				public const string C27Rate = "C27";
				public const string C28Rate = "C28";
				public const string C29Rate = "C29";
				public const string C30Rate = "C30";
				public const string C31Rate = "C31";
				public const string C32Rate = "C32";
				public const string C33Rate = "C33";
				public const string C34Rate = "C34";
				public const string C35Rate = "C35";
				public const string C36Rate = "C36";
				public const string C37Rate = "C37";
				public const string C38Rate = "C38";
				public const string C39Rate = "C39";
				public const string C40Rate = "C40";
				public const string C41Rate = "C41";
				public const string C42Rate = "C42";
				public const string C43Rate = "C43";
				public const string C44Rate = "C44";
				public const string C45Rate = "C45";
				public const string C46Rate = "C46";
				public const string C47Rate = "C47";
				public const string C48Rate = "C48";
				public const string C49Rate = "C49";
				public const string C50Rate = "C50";
				public const string C51Rate = "C51";
				public const string C52Rate = "C52";
				public const string C53Rate = "C53";
				public const string C54Rate = "C54";
				public const string C55Rate = "C55";
				public const string C56Rate = "C56";
				public const string C57Rate = "C57";
				public const string C58Rate = "C58";
				public const string C59Rate = "C59";
				public const string C60Rate = "C60";
				public const string C61Rate = "C61";
				public const string C62Rate = "C62";
				public const string C63Rate = "C63";
				public const string C64Rate = "C64";
				public const string C65Rate = "C65";
				public const string C66Rate = "C66";
				public const string C67Rate = "C67";
				public const string C68Rate = "C68";
				public const string C69Rate = "C69";
				public const string C70Rate = "C70";
				public const string C71Rate = "C71";
				public const string C72Rate = "C72";
				public const string C73Rate = "C73";
				public const string C74Rate = "C74";
				public const string C75Rate = "C75";
				public const string C76Rate = "C76";
				public const string C77Rate = "C77";
				public const string C78Rate = "C78";
				public const string C79Rate = "C79";
				public const string C80Rate = "C80";
				public const string C81Rate = "C81";
				public const string C82Rate = "C82";
				public const string C83Rate = "C83";
				public const string C84Rate = "C84";
				public const string C85Rate = "C85";
				public const string C86Rate = "C86";
				public const string C87Rate = "C87";
				public const string C88Rate = "C88";
				public const string C89Rate = "C89";
				public const string C90Rate = "C90";
				public const string C91Rate = "C91";
				public const string C92Rate = "C92";
				public const string C93Rate = "C93";
				public const string C94Rate = "C94";
				public const string C95Rate = "C95";
				public const string C96Rate = "C96";
				public const string C97Rate = "C97";
				public const string C98Rate = "C98";
				public const string C99Rate = "C99";
				public const string L01Rate = "L01";
				public const string L02Rate = "L02";
				public const string L03Rate = "L03";
				public const string L04Rate = "L04";
				public const string L05Rate = "L05";
				public const string L06Rate = "L06";
				public const string L07Rate = "L07";
				public const string L08Rate = "L08";
				public const string L09Rate = "L09";
				public const string L10Rate = "L10";
				public const string L11Rate = "L11";
				public const string L12Rate = "L12";
				public const string L13Rate = "L13";
				public const string L14Rate = "L14";
				public const string L15Rate = "L15";
				public const string L16Rate = "L16";
				public const string L17Rate = "L17";
				public const string L18Rate = "L18";
				public const string L19Rate = "L19";
				public const string L20Rate = "L20";
				public const string L21Rate = "L21";
				public const string L22Rate = "L22";
				public const string L23Rate = "L23";
				public const string L24Rate = "L24";
				public const string L25Rate = "L25";
				public const string L26Rate = "L26";
				public const string L27Rate = "L27";
				public const string L28Rate = "L28";
				public const string L29Rate = "L29";
				public const string L30Rate = "L30";
				public const string L31Rate = "L31";
				public const string L32Rate = "L32";
				public const string L33Rate = "L33";
				public const string L34Rate = "L34";
				public const string L35Rate = "L35";
				public const string L36Rate = "L36";
				public const string L37Rate = "L37";
				public const string L38Rate = "L38";
				public const string L39Rate = "L39";
				public const string L40Rate = "L40";
				public const string L41Rate = "L41";
				public const string L42Rate = "L42";
				public const string L43Rate = "L43";
				public const string L44Rate = "L44";
				public const string L45Rate = "L45";
				public const string L46Rate = "L46";
				public const string L47Rate = "L47";
				public const string L48Rate = "L48";
				public const string L49Rate = "L49";
				public const string L50Rate = "L50";
				public const string L51Rate = "L51";
				public const string L52Rate = "L52";
				public const string L53Rate = "L53";
				public const string L54Rate = "L54";
				public const string L55Rate = "L55";
				public const string L56Rate = "L56";
				public const string L57Rate = "L57";
				public const string L58Rate = "L58";
				public const string L59Rate = "L59";
				public const string L60Rate = "L60";
				public const string L61Rate = "L61";
				public const string L62Rate = "L62";
				public const string L63Rate = "L63";
				public const string L64Rate = "L64";
				public const string L65Rate = "L65";
				public const string L66Rate = "L66";
				public const string L67Rate = "L67";
				public const string L68Rate = "L68";
				public const string L69Rate = "L69";
				public const string L70Rate = "L70";
				public const string L71Rate = "L71";
				public const string L72Rate = "L72";
				public const string L73Rate = "L73";
				public const string L74Rate = "L74";
				public const string L75Rate = "L75";
				public const string L76Rate = "L76";
				public const string L77Rate = "L77";
				public const string L78Rate = "L78";
				public const string L79Rate = "L79";
				public const string L80Rate = "L80";
				public const string L81Rate = "L81";
				public const string L82Rate = "L82";
				public const string L83Rate = "L83";
				public const string L84Rate = "L84";
				public const string L85Rate = "L85";
				public const string L86Rate = "L86";
				public const string L87Rate = "L87";
				public const string L88Rate = "L88";
				public const string L89Rate = "L89";
				public const string L90Rate = "L90";
				public const string L91Rate = "L91";
				public const string L92Rate = "L92";
				public const string L93Rate = "L93";
				public const string L94Rate = "L94";
				public const string L95Rate = "L95";
				public const string L96Rate = "L96";
				public const string L97Rate = "L97";
				public const string L98Rate = "L98";
				public const string L99Rate = "L99";
			}

			public static class Description
			{
				public static MultilingualString BuyRate => ResString.GetMultilingualString("cc1a7c78-ac56-4b2f-b904-ad81650e3bf8", "Buy Rate");
				public static MultilingualString SellRate => ResString.GetMultilingualString("201557a7-a568-4fce-b9a6-5075e87d0159", "Sell Rate");
				public static MultilingualString CustomsMeasureEURExRate => ResString.GetMultilingualString("6774C76E-2B96-415D-9B74-1C79E46C962A", "Customs Rate (Customs Union)");
				public static MultilingualString CustomsRate => ResString.GetMultilingualString("72feb67b-898a-4b33-b350-959910ee6005", "Customs Rate (Default/Import)");
				public static MultilingualString CustomsRateSecondary => ResString.GetMultilingualString("2f27b675-518b-45e2-9f46-333d4dfd9b48", "Customs Rate (Secondary/Export)");
				public static MultilingualString GlobalCreditControl => ResString.GetMultilingualString("c4fc8d01-ad49-43b9-9ac0-5541be89c92c", "Global Credit Control");
				public static MultilingualString PeriodEndRate => ResString.GetMultilingualString("3b778820-ca56-4dfc-a525-e35d695f0c8b", "Period End Rate");
				public static MultilingualString IATARate => ResString.GetMultilingualString("d599dafa-1cdc-44a7-af50-7aef5135c05c", "IATA Exchange Rate");
				public static MultilingualString C01Rate => ResString.GetMultilingualString("e1b02ede-c6de-4b39-a0a7-3a59dce12ee1", "Custom Rate 01");
				public static MultilingualString C02Rate => ResString.GetMultilingualString("2f295230-adc8-4f83-8614-fd3d51207f74", "Custom Rate 02");
				public static MultilingualString C03Rate => ResString.GetMultilingualString("b1bcce72-8234-4c4b-90ff-b5054c07b7ea", "Custom Rate 03");
				public static MultilingualString C04Rate => ResString.GetMultilingualString("e1e76d5e-d926-44d9-8041-16353e5b362a", "Custom Rate 04");
				public static MultilingualString C05Rate => ResString.GetMultilingualString("8a5e7fdc-9cd8-49bf-8ed1-a49ffa91ec0c", "Custom Rate 05");
				public static MultilingualString C06Rate => ResString.GetMultilingualString("d6955b9b-913a-4a21-862e-3f2136dcd3a0", "Custom Rate 06");
				public static MultilingualString C07Rate => ResString.GetMultilingualString("c973138c-48c1-4279-8375-c23c07e2b877", "Custom Rate 07");
				public static MultilingualString C08Rate => ResString.GetMultilingualString("731477f1-6dab-4a1a-bc6f-6dd273dda7f0", "Custom Rate 08");
				public static MultilingualString C09Rate => ResString.GetMultilingualString("6851b251-0ca5-4b01-9165-536d73d298e2", "Custom Rate 09");
				public static MultilingualString C10Rate => ResString.GetMultilingualString("9ddf8f86-b266-4923-aaeb-ab3cf9c2f2cf", "Custom Rate 10");
				public static MultilingualString C11Rate => ResString.GetMultilingualString("9a6fc764-8441-4176-b08f-062f68ef8cb2", "Custom Rate 11");
				public static MultilingualString C12Rate => ResString.GetMultilingualString("f4d0e7fc-8d9c-4f0d-ade6-5a714af65a3d", "Custom Rate 12");
				public static MultilingualString C13Rate => ResString.GetMultilingualString("77E45B86-6911-454B-8A7E-76AD810D2B15", "Custom Rate 13");
				public static MultilingualString C14Rate => ResString.GetMultilingualString("B2977C85-328B-47A2-8057-B3B147B2A63E", "Custom Rate 14");
				public static MultilingualString C15Rate => ResString.GetMultilingualString("48839A6B-35C2-430F-B9DC-BC6F75157E32", "Custom Rate 15");
				public static MultilingualString C16Rate => ResString.GetMultilingualString("D0060EFF-67FD-4F72-945B-1B3CCFA02F19", "Custom Rate 16");
				public static MultilingualString C17Rate => ResString.GetMultilingualString("5C53B133-FCFF-4EB6-AF05-076D3172313C", "Custom Rate 17");
				public static MultilingualString C18Rate => ResString.GetMultilingualString("2A2F683C-FAB3-4B2D-A1C0-7B4A33BE4BE5", "Custom Rate 18");
				public static MultilingualString C19Rate => ResString.GetMultilingualString("8EE5E568-E0BE-4746-B683-B6293660971B", "Custom Rate 19");
				public static MultilingualString C20Rate => ResString.GetMultilingualString("50B0168C-4790-488F-9053-F7D9CFED5765", "Custom Rate 20");
				public static MultilingualString C21Rate => ResString.GetMultilingualString("0611DA86-6008-4015-831A-6D0585F9BF0C", "Custom Rate 21");
				public static MultilingualString C22Rate => ResString.GetMultilingualString("63B26A8C-6BF8-47EF-B2D9-EEA5733C8A4C", "Custom Rate 22");
				public static MultilingualString C23Rate => ResString.GetMultilingualString("951228E8-4FDF-4394-942A-06E8A044F606", "Custom Rate 23");
				public static MultilingualString C24Rate => ResString.GetMultilingualString("B4578E1D-7D1B-45C6-B856-B144D5F02BB6", "Custom Rate 24");
				public static MultilingualString C25Rate => ResString.GetMultilingualString("80110FC8-C39A-4E45-8718-D7233A1E7874", "Custom Rate 25");
				public static MultilingualString C26Rate => ResString.GetMultilingualString("52EE3C58-A605-4D32-93FC-C1E37F70FF17", "Custom Rate 26");
				public static MultilingualString C27Rate => ResString.GetMultilingualString("C71E7A42-625E-4F86-8C17-F54B6A62E26B", "Custom Rate 27");
				public static MultilingualString C28Rate => ResString.GetMultilingualString("59747A62-8518-415B-ACB6-B865E44C74F1", "Custom Rate 28");
				public static MultilingualString C29Rate => ResString.GetMultilingualString("EE26A8C7-6F93-434A-B12D-113B947AB63C", "Custom Rate 29");
				public static MultilingualString C30Rate => ResString.GetMultilingualString("E857B606-4B24-4856-A1CD-602F50A7C856", "Custom Rate 30");
				public static MultilingualString C31Rate => ResString.GetMultilingualString("6ED0E00E-F326-4B7B-90B9-EED31C556B5A", "Custom Rate 31");
				public static MultilingualString C32Rate => ResString.GetMultilingualString("2F7B8EA5-CC2D-4976-AF58-D8F1F798E3D8", "Custom Rate 32");
				public static MultilingualString C33Rate => ResString.GetMultilingualString("A88A4CCE-AE90-4ED6-A2F2-3375C6C570DA", "Custom Rate 33");
				public static MultilingualString C34Rate => ResString.GetMultilingualString("C2E469C1-582D-44CD-A8CA-9BEB89496D5C", "Custom Rate 34");
				public static MultilingualString C35Rate => ResString.GetMultilingualString("02D33A2B-1726-4E20-B158-8A324BAAB6C9", "Custom Rate 35");
				public static MultilingualString C36Rate => ResString.GetMultilingualString("C077C196-C896-4090-B170-6630DA6958B4", "Custom Rate 36");
				public static MultilingualString C37Rate => ResString.GetMultilingualString("37C38239-B5EB-4B6A-894F-3610FAA871A9", "Custom Rate 37");
				public static MultilingualString C38Rate => ResString.GetMultilingualString("6E6FB6A9-B6C2-4F08-8C1B-849FEDFD5DF5", "Custom Rate 38");
				public static MultilingualString C39Rate => ResString.GetMultilingualString("238B8CAB-2388-45EE-9649-AE6A64AD1BE0", "Custom Rate 39");
				public static MultilingualString C40Rate => ResString.GetMultilingualString("E41AB397-FAF0-4BB3-B737-BB3655929B63", "Custom Rate 40");
				public static MultilingualString C41Rate => ResString.GetMultilingualString("5DC893B4-993A-4195-98CC-EBF726909484", "Custom Rate 41");
				public static MultilingualString C42Rate => ResString.GetMultilingualString("854C19F8-3EFD-4EFB-9F12-AB65E8FD4282", "Custom Rate 42");
				public static MultilingualString C43Rate => ResString.GetMultilingualString("1280B9B0-3957-4648-93A9-138A08B70520", "Custom Rate 43");
				public static MultilingualString C44Rate => ResString.GetMultilingualString("74417F61-FC58-433F-AE77-368B91C7DA58", "Custom Rate 44");
				public static MultilingualString C45Rate => ResString.GetMultilingualString("0B2CFEDB-A016-4501-AAE0-FE6495DD3063", "Custom Rate 45");
				public static MultilingualString C46Rate => ResString.GetMultilingualString("17B4FCB0-596F-4CC9-8D7D-7DB33F27DACA", "Custom Rate 46");
				public static MultilingualString C47Rate => ResString.GetMultilingualString("056EFCC6-5170-4A33-A7A5-CC4E59268AC2", "Custom Rate 47");
				public static MultilingualString C48Rate => ResString.GetMultilingualString("CD42694D-4A81-4757-ADA7-EAC6BEA3F4E0", "Custom Rate 48");
				public static MultilingualString C49Rate => ResString.GetMultilingualString("7084CC2A-BA94-482A-8699-8308FCE2C06F", "Custom Rate 49");
				public static MultilingualString C50Rate => ResString.GetMultilingualString("1F61AE2D-8058-4831-9D8C-51A4EDFE3570", "Custom Rate 50");
				public static MultilingualString C51Rate => ResString.GetMultilingualString("CF37BF65-4E28-497D-A1E8-0F6DC3CAAC03", "Custom Rate 51");
				public static MultilingualString C52Rate => ResString.GetMultilingualString("F6564A59-A0D0-4BD2-918E-D97BC0B50906", "Custom Rate 52");
				public static MultilingualString C53Rate => ResString.GetMultilingualString("8A6BEA2E-ED97-4695-916F-720494104535", "Custom Rate 53");
				public static MultilingualString C54Rate => ResString.GetMultilingualString("C1EFB546-50F3-4AB1-9945-6E60EEE297E4", "Custom Rate 54");
				public static MultilingualString C55Rate => ResString.GetMultilingualString("44AD36D1-1FF2-4318-8F6D-379CF1609C38", "Custom Rate 55");
				public static MultilingualString C56Rate => ResString.GetMultilingualString("ED881E8D-12B1-4085-97A3-97D70B22C374", "Custom Rate 56");
				public static MultilingualString C57Rate => ResString.GetMultilingualString("785EF4F1-179E-4932-AEC6-63095473AD81", "Custom Rate 57");
				public static MultilingualString C58Rate => ResString.GetMultilingualString("CA28A0F9-179C-4FCD-839F-C487C6E90936", "Custom Rate 58");
				public static MultilingualString C59Rate => ResString.GetMultilingualString("4DA44AEE-9E61-4CF1-ADAA-3A04256831C9", "Custom Rate 59");
				public static MultilingualString C60Rate => ResString.GetMultilingualString("5E8A4F44-A19C-47AB-AB13-CE2AEF37A5A3", "Custom Rate 60");
				public static MultilingualString C61Rate => ResString.GetMultilingualString("4D857087-1A12-446B-B9A9-0E6EF9DF48C7", "Custom Rate 61");
				public static MultilingualString C62Rate => ResString.GetMultilingualString("362A8748-208B-4E90-81BD-464224A9B112", "Custom Rate 62");
				public static MultilingualString C63Rate => ResString.GetMultilingualString("AB164A0F-DEF5-4178-B746-1BEEBE3B1242", "Custom Rate 63");
				public static MultilingualString C64Rate => ResString.GetMultilingualString("F881BFE0-E6E7-4DB7-84B0-3DAE4E9BA00A", "Custom Rate 64");
				public static MultilingualString C65Rate => ResString.GetMultilingualString("7D4DB151-ABB7-4C31-9D23-B7EA9C24C6FB", "Custom Rate 65");
				public static MultilingualString C66Rate => ResString.GetMultilingualString("B1F3104F-37C7-4D19-BE1D-F10B004A148D", "Custom Rate 66");
				public static MultilingualString C67Rate => ResString.GetMultilingualString("A649EA87-58A5-4F43-954A-DBAFD6C9E244", "Custom Rate 67");
				public static MultilingualString C68Rate => ResString.GetMultilingualString("BA070404-D702-41B9-A2D3-197287D1C9FA", "Custom Rate 68");
				public static MultilingualString C69Rate => ResString.GetMultilingualString("FEBF88CC-7938-42C8-A53B-9B671563B3D6", "Custom Rate 69");
				public static MultilingualString C70Rate => ResString.GetMultilingualString("7A0BB5A1-38A8-4D20-BF82-262041EE219E", "Custom Rate 70");
				public static MultilingualString C71Rate => ResString.GetMultilingualString("AB3242E3-D5B9-47FC-A544-2CEBFD6FF3D8", "Custom Rate 71");
				public static MultilingualString C72Rate => ResString.GetMultilingualString("D4CFC886-AAA2-41B7-A7AE-960DC5205653", "Custom Rate 72");
				public static MultilingualString C73Rate => ResString.GetMultilingualString("2EAE1EAA-DECD-4D42-8D1C-17CF1F5759B6", "Custom Rate 73");
				public static MultilingualString C74Rate => ResString.GetMultilingualString("48C5B04C-D127-4FD2-89F3-2F019F857880", "Custom Rate 74");
				public static MultilingualString C75Rate => ResString.GetMultilingualString("246941CA-846C-46E2-8DBA-440BEAE21133", "Custom Rate 75");
				public static MultilingualString C76Rate => ResString.GetMultilingualString("A25F62DF-E8A0-4611-A682-E1B2989FE7D6", "Custom Rate 76");
				public static MultilingualString C77Rate => ResString.GetMultilingualString("965B30DD-1367-4434-BDB5-B527FFCF16FC", "Custom Rate 77");
				public static MultilingualString C78Rate => ResString.GetMultilingualString("99931E24-C1ED-4C0D-8D24-3EADA3234759", "Custom Rate 78");
				public static MultilingualString C79Rate => ResString.GetMultilingualString("A7820421-A8A1-422E-9506-CCA979B6F4B0", "Custom Rate 79");
				public static MultilingualString C80Rate => ResString.GetMultilingualString("9B05F477-0238-4D02-A981-AF5D331DDBCD", "Custom Rate 80");
				public static MultilingualString C81Rate => ResString.GetMultilingualString("EF9F7B13-EC24-4948-B459-3DCDF4DDD15A", "Custom Rate 81");
				public static MultilingualString C82Rate => ResString.GetMultilingualString("AFD130F8-BFFA-46C2-BB00-3CC8A027F350", "Custom Rate 82");
				public static MultilingualString C83Rate => ResString.GetMultilingualString("5D81F621-8BBB-40DB-ABDF-F80C62CBA001", "Custom Rate 83");
				public static MultilingualString C84Rate => ResString.GetMultilingualString("F15512E2-791C-468B-B775-8DB9D8792409", "Custom Rate 84");
				public static MultilingualString C85Rate => ResString.GetMultilingualString("76E0C290-F9B1-4223-AF1B-8271AE3F01D6", "Custom Rate 85");
				public static MultilingualString C86Rate => ResString.GetMultilingualString("D2DF933A-0429-493E-876B-2842AC119E02", "Custom Rate 86");
				public static MultilingualString C87Rate => ResString.GetMultilingualString("EE86F29F-71CB-496D-ACDC-F042F637C6D3", "Custom Rate 87");
				public static MultilingualString C88Rate => ResString.GetMultilingualString("C05EF512-B1B3-497E-8CF5-954D11B8BC50", "Custom Rate 88");
				public static MultilingualString C89Rate => ResString.GetMultilingualString("2F4B1F38-E8F4-45C3-8971-BAEAA7C2FA1C", "Custom Rate 89");
				public static MultilingualString C90Rate => ResString.GetMultilingualString("252A922D-C3F4-4F2D-962B-2DDD7C1D6EBC", "Custom Rate 90");
				public static MultilingualString C91Rate => ResString.GetMultilingualString("C0545CA6-D200-4903-90E1-724608021430", "Custom Rate 91");
				public static MultilingualString C92Rate => ResString.GetMultilingualString("C5DE5DB7-6690-48FD-8A80-2A89BE3B2C75", "Custom Rate 92");
				public static MultilingualString C93Rate => ResString.GetMultilingualString("DA115791-957D-4150-8DD4-A1397AC69E3F", "Custom Rate 93");
				public static MultilingualString C94Rate => ResString.GetMultilingualString("F62E45DE-E9EC-498B-A050-496F8FBA0D2E", "Custom Rate 94");
				public static MultilingualString C95Rate => ResString.GetMultilingualString("F73A6D25-756F-493E-9AF0-8D043CF2150D", "Custom Rate 95");
				public static MultilingualString C96Rate => ResString.GetMultilingualString("DCAA21A0-3E51-488F-AAF9-1FCE894B72E9", "Custom Rate 96");
				public static MultilingualString C97Rate => ResString.GetMultilingualString("6A355D9B-88DC-46D2-A774-D82B34E67B3B", "Custom Rate 97");
				public static MultilingualString C98Rate => ResString.GetMultilingualString("8FECCFBF-8A99-4643-812E-7CF0D99D975E", "Custom Rate 98");
				public static MultilingualString C99Rate => ResString.GetMultilingualString("87878C27-47E0-401E-B26B-913D2B31EC9A", "Custom Rate 99");
				public static MultilingualString L01Rate => ResString.GetMultilingualString("7B5C67AF-3F78-4A69-9C2D-7DAF4D679A45", "Local Rate 01");
				public static MultilingualString L02Rate => ResString.GetMultilingualString("B75D51AF-E795-46EB-AB24-CB205427AD57", "Local Rate 02");
				public static MultilingualString L03Rate => ResString.GetMultilingualString("C097E896-7FC4-4F03-B1F1-56B5281194E8", "Local Rate 03");
				public static MultilingualString L04Rate => ResString.GetMultilingualString("BB6BCEB3-8282-475B-923C-B7E30B770708", "Local Rate 04");
				public static MultilingualString L05Rate => ResString.GetMultilingualString("6A0DC44E-1700-4782-9EA6-C2414AA8E547", "Local Rate 05");
				public static MultilingualString L06Rate => ResString.GetMultilingualString("25981DA7-40A3-4AF6-AA3D-476A40171D72", "Local Rate 06");
				public static MultilingualString L07Rate => ResString.GetMultilingualString("6F80C0A6-4054-44D7-BF89-D5A30E773FBA", "Local Rate 07");
				public static MultilingualString L08Rate => ResString.GetMultilingualString("582B3A5E-07F6-4BD5-9178-4260099D1F61", "Local Rate 08");
				public static MultilingualString L09Rate => ResString.GetMultilingualString("AFFCC175-5524-4C30-AA6F-DEE28BB14AF4", "Local Rate 09");
				public static MultilingualString L10Rate => ResString.GetMultilingualString("544B704D-7A54-4050-889F-66E07C8939F0", "Local Rate 10");
				public static MultilingualString L11Rate => ResString.GetMultilingualString("B02977FF-6411-473D-9729-FA24B798F548", "Local Rate 11");
				public static MultilingualString L12Rate => ResString.GetMultilingualString("6AB1BE1E-C429-40FF-979F-DB824EC76718", "Local Rate 12");
				public static MultilingualString L13Rate => ResString.GetMultilingualString("79207ACA-152E-42AE-B6D1-F021A1EFEDEF", "Local Rate 13");
				public static MultilingualString L14Rate => ResString.GetMultilingualString("D07B796F-A064-4483-B048-8A82EA56320E", "Local Rate 14");
				public static MultilingualString L15Rate => ResString.GetMultilingualString("C2C46A71-9C2C-46BD-8E73-3B9E687F5BCA", "Local Rate 15");
				public static MultilingualString L16Rate => ResString.GetMultilingualString("03266976-AD6C-4294-9F66-72AD663503F6", "Local Rate 16");
				public static MultilingualString L17Rate => ResString.GetMultilingualString("A4EA970B-90AF-446E-8C71-52CC5C013242", "Local Rate 17");
				public static MultilingualString L18Rate => ResString.GetMultilingualString("75D7CD3B-6F63-430C-ADAF-19CD81ECAE75", "Local Rate 18");
				public static MultilingualString L19Rate => ResString.GetMultilingualString("6D9DA0CE-4ECF-45AE-B0E0-DBF0C16AB4E3", "Local Rate 19");
				public static MultilingualString L20Rate => ResString.GetMultilingualString("99169485-D059-4F97-8C04-57F49DF85F0F5", "Local Rate 20");
				public static MultilingualString L21Rate => ResString.GetMultilingualString("0F875523-86B0-4CC7-91A7-F3B52ECA1A7E", "Local Rate 21");
				public static MultilingualString L22Rate => ResString.GetMultilingualString("EF58E21C-ACD7-4849-AFB3-25A8CDC2CF87", "Local Rate 22");
				public static MultilingualString L23Rate => ResString.GetMultilingualString("7493C74C-28D6-4B4F-A703-6494C674E219", "Local Rate 23");
				public static MultilingualString L24Rate => ResString.GetMultilingualString("6009055E-2908-4160-BDFF-B1C1EBA04BA7", "Local Rate 24");
				public static MultilingualString L25Rate => ResString.GetMultilingualString("E6CCC866-26C8-4F8E-AE44-F0A676614C2F", "Local Rate 25");
				public static MultilingualString L26Rate => ResString.GetMultilingualString("2DA35EBD-7E3B-40DA-A807-ACBE4E819A87", "Local Rate 26");
				public static MultilingualString L27Rate => ResString.GetMultilingualString("B1B55B56-7E60-43A4-8F29-D5E830E29B12", "Local Rate 27");
				public static MultilingualString L28Rate => ResString.GetMultilingualString("997D988E-2BBA-4118-AD84-9E131E042C42", "Local Rate 28");
				public static MultilingualString L29Rate => ResString.GetMultilingualString("FE0DA8AA-A46F-4FCA-8E77-012270F47C22", "Local Rate 29");
				public static MultilingualString L30Rate => ResString.GetMultilingualString("F90E0E50-AC77-4E40-9A61-1F226B7FA0B9", "Local Rate 30");
				public static MultilingualString L31Rate => ResString.GetMultilingualString("25468A3B-EE8C-4248-9B0E-C81FFA80DD18", "Local Rate 31");
				public static MultilingualString L32Rate => ResString.GetMultilingualString("FA0FA0E8-E5E1-4753-9887-7A65C46B7DE3", "Local Rate 32");
				public static MultilingualString L33Rate => ResString.GetMultilingualString("1D361C0C-F51D-4DCF-8ED4-BD49F786E8DC", "Local Rate 33");
				public static MultilingualString L34Rate => ResString.GetMultilingualString("1EFF805E-FB3A-410A-93D1-DD0550B44DCC", "Local Rate 34");
				public static MultilingualString L35Rate => ResString.GetMultilingualString("BF663761-B539-43D4-B3C8-E538A4848936", "Local Rate 35");
				public static MultilingualString L36Rate => ResString.GetMultilingualString("93FEB2E8-349A-46FC-8219-C971AAE67D1F", "Local Rate 36");
				public static MultilingualString L37Rate => ResString.GetMultilingualString("56960D56-A52F-49DF-A349-3B43AA6AC8C3", "Local Rate 37");
				public static MultilingualString L38Rate => ResString.GetMultilingualString("62F74128-10F5-4D61-88EE-0C8BA6A4109B", "Local Rate 38");
				public static MultilingualString L39Rate => ResString.GetMultilingualString("468A90AC-8A66-4309-8387-73C602DC34C1", "Local Rate 39");
				public static MultilingualString L40Rate => ResString.GetMultilingualString("07012029-A091-439C-9B02-1FE53470FC43", "Local Rate 40");
				public static MultilingualString L41Rate => ResString.GetMultilingualString("D68709C8-B545-4AEF-8A7B-7194E2530814", "Local Rate 41");
				public static MultilingualString L42Rate => ResString.GetMultilingualString("C14DAA39-289A-436B-BA41-33A40C8F6CB2", "Local Rate 42");
				public static MultilingualString L43Rate => ResString.GetMultilingualString("8FE8D654-4AC8-4526-859B-873DBCA458C4", "Local Rate 43");
				public static MultilingualString L44Rate => ResString.GetMultilingualString("B9E54C14-24D6-4E13-9F29-DC195F4E5739", "Local Rate 44");
				public static MultilingualString L45Rate => ResString.GetMultilingualString("1D5D1853-B9D4-4CE0-8AC7-03FCA4C8A5AE", "Local Rate 45");
				public static MultilingualString L46Rate => ResString.GetMultilingualString("28544B7B-06F0-4A6E-BF09-B644C19706AB", "Local Rate 46");
				public static MultilingualString L47Rate => ResString.GetMultilingualString("C08D714C-3497-40E1-B97E-28BAB7201873", "Local Rate 47");
				public static MultilingualString L48Rate => ResString.GetMultilingualString("D043FE55-167C-4B39-875A-2D1FC5FC8F41", "Local Rate 48");
				public static MultilingualString L49Rate => ResString.GetMultilingualString("D6E387A0-4A10-4BE3-B971-EA486D564BEE", "Local Rate 49");
				public static MultilingualString L50Rate => ResString.GetMultilingualString("05311B46-F954-4013-96A4-F1FC7868E3CC", "Local Rate 50");
				public static MultilingualString L51Rate => ResString.GetMultilingualString("AEE63B20-FC11-46A0-8C5F-8DF6F10A2A03", "Local Rate 51");
				public static MultilingualString L52Rate => ResString.GetMultilingualString("C7E41702-00ED-49B6-8E9D-4BD5545BE6B5", "Local Rate 52");
				public static MultilingualString L53Rate => ResString.GetMultilingualString("86F5C9FF-3447-40DB-9A64-4105B1DF0128", "Local Rate 53");
				public static MultilingualString L54Rate => ResString.GetMultilingualString("83EF47B5-FCB9-4B9B-9402-C199508125C7", "Local Rate 54");
				public static MultilingualString L55Rate => ResString.GetMultilingualString("50D37871-D4B4-4B41-A98E-5A040AA4A99D", "Local Rate 55");
				public static MultilingualString L56Rate => ResString.GetMultilingualString("E3C7DEB6-3B3E-405D-8E6B-1DC78BB288B2", "Local Rate 56");
				public static MultilingualString L57Rate => ResString.GetMultilingualString("A5053972-1365-429E-B6EA-CB74FB60D345", "Local Rate 57");
				public static MultilingualString L58Rate => ResString.GetMultilingualString("E2F57203-D78D-49B0-B6B7-6C996D486FEB", "Local Rate 58");
				public static MultilingualString L59Rate => ResString.GetMultilingualString("09B37E28-1F89-412D-BEC0-55E0B4E8D4ED", "Local Rate 59");
				public static MultilingualString L60Rate => ResString.GetMultilingualString("0898FD00-2772-490E-9B2D-4053B335BB95", "Local Rate 60");
				public static MultilingualString L61Rate => ResString.GetMultilingualString("7144FC17-473C-4713-B4ED-32DE1E4A806B", "Local Rate 61");
				public static MultilingualString L62Rate => ResString.GetMultilingualString("F4E2A1A0-08E1-43D2-A7BE-25F2C7A66F3B", "Local Rate 62");
				public static MultilingualString L63Rate => ResString.GetMultilingualString("5D052DD0-4252-4131-B72B-D0F5495BE451", "Local Rate 63");
				public static MultilingualString L64Rate => ResString.GetMultilingualString("94123B8C-99C2-4E09-A37E-17D8E792208B", "Local Rate 64");
				public static MultilingualString L65Rate => ResString.GetMultilingualString("04409C9B-14B4-4FAA-889E-8F8FF3DB5860", "Local Rate 65");
				public static MultilingualString L66Rate => ResString.GetMultilingualString("C3F313B0-F78F-4383-A53C-F59A8C4BDA02", "Local Rate 66");
				public static MultilingualString L67Rate => ResString.GetMultilingualString("6F8BABDD-8100-422C-9C5F-5F925DA119E1", "Local Rate 67");
				public static MultilingualString L68Rate => ResString.GetMultilingualString("7D69A889-F889-467D-8142-6E76FE252E53", "Local Rate 68");
				public static MultilingualString L69Rate => ResString.GetMultilingualString("09086B9F-13F7-4541-A0DD-6983D95F11DF", "Local Rate 69");
				public static MultilingualString L70Rate => ResString.GetMultilingualString("24423827-4F93-43A9-A403-E9A824DE3017", "Local Rate 70");
				public static MultilingualString L71Rate => ResString.GetMultilingualString("AF2A14CA-BF6F-4904-BD01-7F75DDECCFDA", "Local Rate 71");
				public static MultilingualString L72Rate => ResString.GetMultilingualString("C3497E4C-153D-4587-8F16-EC6F2A66ADD8", "Local Rate 72");
				public static MultilingualString L73Rate => ResString.GetMultilingualString("0140D3D8-0C20-46A7-A013-FD00FDBECA61", "Local Rate 73");
				public static MultilingualString L74Rate => ResString.GetMultilingualString("0204066E-D709-4194-B02C-32123A9C723D", "Local Rate 74");
				public static MultilingualString L75Rate => ResString.GetMultilingualString("8BA28ECA-A28A-401C-B854-F84520449612", "Local Rate 75");
				public static MultilingualString L76Rate => ResString.GetMultilingualString("64436FF9-6AC6-4C37-9CB2-B96E3A654D75", "Local Rate 76");
				public static MultilingualString L77Rate => ResString.GetMultilingualString("7B713CF1-18CD-40AB-9579-873AEE0B0F58", "Local Rate 77");
				public static MultilingualString L78Rate => ResString.GetMultilingualString("011F675F-7EBE-46A3-8198-4094DCE9A4B1", "Local Rate 78");
				public static MultilingualString L79Rate => ResString.GetMultilingualString("88679732-2345-463A-A6F2-454799EA86B3", "Local Rate 79");
				public static MultilingualString L80Rate => ResString.GetMultilingualString("90309FDC-CFAF-4D1F-AB8E-8A67205FB927", "Local Rate 80");
				public static MultilingualString L81Rate => ResString.GetMultilingualString("247D1B70-8BB1-4E30-AA5D-0F3D1F890673", "Local Rate 81");
				public static MultilingualString L82Rate => ResString.GetMultilingualString("AEEE26B4-9D83-4DB3-9EEB-2A82C482EE5C", "Local Rate 82");
				public static MultilingualString L83Rate => ResString.GetMultilingualString("A361FE2B-7445-4276-BED9-209892246324", "Local Rate 83");
				public static MultilingualString L84Rate => ResString.GetMultilingualString("AA5F6729-B8C0-481C-A8D8-6FF6CA410B82", "Local Rate 84");
				public static MultilingualString L85Rate => ResString.GetMultilingualString("AFDE0CFC-677B-41F6-85D1-996BDDE61752", "Local Rate 85");
				public static MultilingualString L86Rate => ResString.GetMultilingualString("01879D51-CDF2-4959-AA98-35870941530C", "Local Rate 86");
				public static MultilingualString L87Rate => ResString.GetMultilingualString("4BBC1C55-6F1B-4FAE-AD15-6702D91871EF", "Local Rate 87");
				public static MultilingualString L88Rate => ResString.GetMultilingualString("A6BBB0DD-A617-494F-8DF3-4CE917CFC49C", "Local Rate 88");
				public static MultilingualString L89Rate => ResString.GetMultilingualString("39D87684-7D91-451D-9D0E-51457604B6E3", "Local Rate 89");
				public static MultilingualString L90Rate => ResString.GetMultilingualString("7BB47705-38B8-4CEC-A856-96F5CC11679F", "Local Rate 90");
				public static MultilingualString L91Rate => ResString.GetMultilingualString("9A75DB0E-4480-445C-B0D5-C14B95E92A41", "Local Rate 91");
				public static MultilingualString L92Rate => ResString.GetMultilingualString("95884468-F5B5-4845-849B-AB162207AA6D", "Local Rate 92");
				public static MultilingualString L93Rate => ResString.GetMultilingualString("BAED3FFA-0E2F-41C4-9035-CE2FCBC3CDC3", "Local Rate 93");
				public static MultilingualString L94Rate => ResString.GetMultilingualString("FE0C0EDB-E2ED-4E45-9F9E-812F9C73B8E3", "Local Rate 94");
				public static MultilingualString L95Rate => ResString.GetMultilingualString("011523A9-7028-48B8-8E98-A8B17A951BF4", "Local Rate 95");
				public static MultilingualString L96Rate => ResString.GetMultilingualString("0D7A22D0-C031-4A22-83A6-14FEDA8C7C88", "Local Rate 96");
				public static MultilingualString L97Rate => ResString.GetMultilingualString("E7A38AC0-F7BD-48FD-9761-A7C1B894BC2C", "Local Rate 97");
				public static MultilingualString L98Rate => ResString.GetMultilingualString("CBF5DABD-BFBE-4E1E-8EE7-52C73CDE0E16", "Local Rate 98");
				public static MultilingualString L99Rate => ResString.GetMultilingualString("EA5DAA87-8BCE-493A-8BE2-D1AC4D0EACEE", "Local Rate 99");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Payment status types")]
		public static class PaymentStatusTypes
		{
			public const string AllTransactions = "DISPLAY ALL TRANSACTIONS";
			public const string UnpaidTransactions = "DISPLAY UNPAID TRANSACTIONS";
			public const string PaidTransactions = "DISPLAY PAID TRANSACTIONS";
			public const string PartPaidTransactions = "DISPLAY PART PAID TRANSACTIONS";
		}

		public static class OrgPatternMatchOverrideRelationships
		{
			public const string Organisation = "ORG";
			public const string Port = "PTC";
			public const string Currency = "CUR";
			public const string Country = "COU";
			public const string Commodities = "COM";
			public const string DropMode = "DRP";
			public const string Equipment = "EQP";
			public const string IncoTerm = "INC";
			public const string ContainerType = "CNT";
			public const string ChargeCodes = "CHC";
			public const string PackageType = "PKG";
			public const string EventCode = "EVT";
			public const string Warehouse = "WHS";
			public const string ServiceLevel = "SER";
			public const string IntZone = "IZN";
			public const string CarrierServiceLevel = "CSL";
			public const string DocumentType = "DOC";
		}

		public static class OrgPatternMatchOverrideContexts
		{
			public static class Codes
			{
				public const string OceanCarrierMessage = "OCM";
			}

			public static class Descriptions
			{
				public static MultilingualString OceanCarrierMessage => ResString.GetMultilingualString("9a8fdff7-c9a5-4fc5-93dd-627b4f796de3", "Ocean Carrier Message");
			}
		}

		public static class DiscrepancyReason
		{
			public const string ShortLanded = "SHL";
			public const string Surplus = "SUR";
			public const string Damaged = "DMG";
			public const string Pillaged = "PIL";
			public const string Lost = "LST";
		}

		public static class DDRFileFormat
		{
			public const string AB1 = "AB1";
			public const string AB2 = "AB2";
			public const string ANZ = "ANZ";
			public const string ASB = "ASB";
			public const string BBL = "BBL";
			public const string BCS = "BCS";
			public const string BNZ = "BNZ";
			public const string CBA = "CBA";
			public const string NAB = "NAB";
			public const string WBC = "WBC";
			public const string WNZ = "WNZ";
			public const string BTM = "BTM";
			public const string CUS = "CUS";
			public const string HSB = "HSB";
		}

		public static class ReceivedAs
		{
			public const string Default = "";
			public const string ScannedIn = "SCN";
			public const string PackedPackage = "PKG";
			public const string PackedPacline = "PKL";
			public const string PackedNonTracked = "PNT";
			public const string Adhoc = "ADC";
			public const string ScannedInAsInner = "SCI";
			public const string BuiltInWarehouse = "BIW";
		}

		public static class PkgUnit
		{
			public const string Bag = "BAG";
			public const string BulkBag = "BBG";
			public const string BreakBulk = "BBK";
			public const string BaleCompressed = "BLC";
			public const string BaleUncompressed = "BLU";
			public const string Bundle = "BND";
			public const string Bottle = "BOT";
			public const string Box = "BOX";
			public const string Basket = "BSK";
			public const string Case = "CAS";
			public const string Container = "CNT";
			public const string Coil = "COI";
			public const string Cradle = "CRD";
			public const string Crate = "CRT";
			public const string Carton = "CTN";
			public const string Cylinder = "CYL";
			public const string Dozen = "DOZ";
			public const string Drum = "DRM";
			public const string Envelope = "ENV";
			public const string Gross = "GRS";
			public const string Keg = "KEG";
			public const string Mix = "MIX";
			public const string Pail = "PAI";
			public const string Piece = "PCE";
			public const string Package = "PKG";
			public const string Pallet = "PLT";
			public const string Reel = "REL";
			public const string Roll = "RLL";
			public const string RollOnRollOff = "ROR";
			public const string Sheet = "SHT";
			public const string Skid = "SKD";
			public const string Spool = "SPL";
			public const string Tote = "TOT";
			public const string Tube = "TUB";
			public const string Unit = "UNT";

			public static MultilingualString GetDescription(string unit, PluralState pluralState = PluralState.NonPlural)
			{
				switch (pluralState)
				{
					case PluralState.NonPlural:
						return GetNonPluralDescription(unit);

					case PluralState.Plural:
						return GetPluralDescription(unit);

					case PluralState.PluralOrNonPlural:
						return GetPluralOrNonPluralDescription(unit);

					default:
						return (NoResString)"";
				}
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "No code description pair list availble so this is the simpliest way to match up the description with the code")]
			public static MultilingualString GetNonPluralDescription(string unit)
			{
				switch (unit)
				{
					case Bag:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Bag", "Bag");
					case BaleCompressed:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|BaleCompressed", "Bale, Compressed");
					case BaleUncompressed:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|BaleUncompressed", "Bale, Uncompressed");
					case Basket:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Basket", "Basket");
					case Bottle:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Bottle", "Bottle");
					case Box:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Box", "Box");
					case BreakBulk:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|BreakBulk", "Break Bulk");
					case BulkBag:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|BulkBag", "Bulk Bag");
					case Bundle:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Bundle", "Bundle");
					case Carton:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Carton", "Carton");
					case Case:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Case", "Case");
					case Coil:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Coil", "Coil");
					case Container:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Container", "Container");
					case Cradle:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Cradle", "Cradle");
					case Crate:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Crate", "Crate");
					case Cylinder:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Cylinder", "Cylinder");
					case Dozen:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Dozen", "Dozen");
					case Drum:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Drum", "Drum");
					case Envelope:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Envelope", "Envelope");
					case Gross:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Gross", "Gross");
					case Keg:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Keg", "Keg");
					case Mix:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Mix", "Mix");
					case Package:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Package", "Package");
					case Pail:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Pail", "Pail");
					case Pallet:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Pallet", "Pallet");
					case Piece:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Piece", "Piece");
					case Reel:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Reel", "Reel");
					case Roll:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Roll", "Roll");
					case RollOnRollOff:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|RollOnRollOff", "Roll-on/roll-off");
					case Sheet:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Sheet", "Sheet");
					case Skid:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Skid", "Skid");
					case Spool:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Spool", "Spool");
					case Tote:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Tote", "Tote");
					case Tube:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Tube", "Tube");
					case Unit:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Unit", "Unit");
					default:
						return (NoResString)"";
				}
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "No code description pair list availble so this is the simpliest way to match up the description with the code")]
			public static MultilingualString GetPluralDescription(string unit)
			{
				switch (unit)
				{
					case Bag:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Bag", "Bags");
					case BaleCompressed:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|BaleCompressed", "Bales, Compressed");
					case BaleUncompressed:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|BaleUncompressed", "Bales, Uncompressed");
					case Basket:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Basket", "Baskets");
					case Bottle:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Bottle", "Bottles");
					case Box:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Box", "Boxes");
					case BreakBulk:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|BreakBulk", "Break Bulk");
					case BulkBag:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|BulkBag", "Bulk Bags");
					case Bundle:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Bundle", "Bundles");
					case Carton:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Carton", "Cartons");
					case Case:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Case", "Cases");
					case Coil:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Coil", "Coils");
					case Container:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Container", "Containers");
					case Cradle:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Cradle", "Cradles");
					case Crate:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Crate", "Crates");
					case Cylinder:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Cylinder", "Cylinders");
					case Dozen:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Dozen", "Dozen");
					case Drum:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Drums", "Drums");
					case Envelope:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Envelopes", "Envelopes");
					case Gross:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Gross", "Gross");
					case Keg:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Keg", "Kegs");
					case Mix:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Mix", "Mixes");
					case Package:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Package", "Packages");
					case Pail:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Pail", "Pails");
					case Pallet:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Pallet", "Pallets");
					case Piece:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Piece", "Pieces");
					case Reel:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Reel", "Reels");
					case Roll:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Roll", "Rolls");
					case RollOnRollOff:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|RollOnRollOff", "Roll-on/roll-offs");
					case Sheet:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Sheet", "Sheets");
					case Skid:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Skid", "Skids");
					case Spool:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Spool", "Spools");
					case Tote:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Tote", "Totes");
					case Tube:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Tube", "Tubes");
					case Unit:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|Plural|Unit", "Units");
					default:
						return (NoResString)"";
				}
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "No code description pair list availble so this is the simpliest way to match up the description with the code")]
			public static MultilingualString GetPluralOrNonPluralDescription(string unit)
			{
				switch (unit)
				{
					case Bag:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Bag", "Bag(s)");
					case BaleCompressed:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|BaleCompressed", "Bale(s), Compressed");
					case BaleUncompressed:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|BaleUncompressed", "Bale(s), Uncompressed");
					case Basket:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Basket", "Basket(s)");
					case Bottle:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Bottle", "Bottle(s)");
					case Box:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Box", "Box(s)");
					case BreakBulk:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|BreakBulk", "Break Bulk");
					case BulkBag:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|BulkBag", "Bulk Bag(s)");
					case Bundle:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Bundle", "Bundle(s)");
					case Carton:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Carton", "Carton(s)");
					case Case:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Case", "Case(s)");
					case Coil:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Coil", "Coil(s)");
					case Container:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Container", "Container(s)");
					case Cradle:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Cradle", "Cradle(s)");
					case Crate:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Crate", "Crate(s)");
					case Cylinder:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Cylinder", "Cylinder(s)");
					case Dozen:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Dozen", "Dozen");
					case Drum:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Drum", "Drum(s)");
					case Envelope:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Envelope", "Envelope(s)");
					case Gross:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Gross", "Gross");
					case Keg:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Keg", "Keg(s)");
					case Mix:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Mix", "Mix(s)");
					case Package:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Package", "Package(s)");
					case Pail:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Pail", "Pail(s)");
					case Pallet:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Pallet", "Pallet(s)");
					case Piece:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Piece", "Piece(s)");
					case Reel:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Reel", "Reel(s)");
					case Roll:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Roll", "Roll(s)");
					case RollOnRollOff:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|RollOnRollOff", "Roll-on/roll-off(s)");
					case Sheet:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Sheet", "Sheet(s)");
					case Skid:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Skid", "Skid(s)");
					case Spool:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Spool", "Spool(s)");
					case Tote:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Tote", "Tote(s)");
					case Tube:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Tube", "Tube(s)");
					case Unit:
						return ResString.GetMultilingualString("Resources|Constants|PkgUnit|PluralOrNonPlural|Unit", "Unit(s)");
					default:
						return (NoResString)"";
				}
			}
		}

		public static class BusinessQuantityUnit
		{
			public const string Container = "CN";
		}

		#region Weight, Volume, Dimension, Distance, Length, Area

		public static class Dimension
		{
			[BaseUnit(0.001)]
			public const string Millimetres = Length.Millimetres;

			[BaseUnit(0.01)]
			public const string Centimetres = Length.Centimetres;

			[BaseUnit(1)]
			public const string Metres = Length.Metres;

			[BaseUnit(0.0254)]
			public const string Inches = Length.Inches;

			[BaseUnit(0.3048)]
			public const string Feet = Length.Feet;

			[BaseUnit(0.9144)]
			public const string Yards = Length.Yards;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021")]
			public static readonly string[] Codes = new string[] { Millimetres, Centimetres, Metres, Inches, Feet, Yards };

			public static bool ContainsCode(string code) => UnitConverter.ContainsCode(code, Codes);

			public static MultilingualString GetDescription(string unit, PluralState pluralState) => Length.GetDescription(unit, pluralState);

			public static decimal Convert(decimal sourceValue, string sourceUnitCode, string targetUnitCode, bool applyDefaultRounding = true) => UnitConverter.Convert(typeof(Dimension), sourceValue, sourceUnitCode, targetUnitCode, applyDefaultRounding);

			public static decimal ConvertSafe(decimal sourceValue, string sourceUnitCode, string targetUnitCode, bool applyDefaultRounding = true)
			{
				if (ContainsCode(sourceUnitCode) && ContainsCode(targetUnitCode))
				{
					return Convert(sourceValue, sourceUnitCode, targetUnitCode, applyDefaultRounding);
				}
				return 0m;
			}
		}

		public static class Distance
		{
			[BaseUnit(1000)]
			public const string Kilometres = Length.Kilometres;

			[BaseUnit(1609.344)]
			public const string Miles = Length.Miles;

			[BaseUnit(1852)]
			public const string NauticalMiles = "NM";

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021")]
			public static readonly string[] Codes = new string[] { Kilometres, Miles, NauticalMiles };

			public static bool ContainsCode(string code) => UnitConverter.ContainsCode(code, Codes);

			public static MultilingualString GetDescription(string unit, PluralState pluralState)
			{
				if (string.Compare(unit, NauticalMiles, StringComparison.OrdinalIgnoreCase) == 0)
				{
					switch (pluralState)
					{
						case PluralState.NonPlural:
							return ResString.GetMultilingualString("Resources|Constants|Distance|NauticalMile", "Nautical Mile");
						case PluralState.Plural:
							return ResString.GetMultilingualString("Resources|Constants|Distance|NauticalMiles", "Nautical Miles");
						case PluralState.PluralOrNonPlural:
							return ResString.GetMultilingualString("Resources|Constants|Length|NauticalMilesPluralOrNonPlural", "Nautical Mile(s)");
						default:
							return (NoResString)string.Empty;
					}
				}

				return Length.GetDescription(unit, pluralState);
			}

			public static decimal Convert(decimal sourceValue, string sourceUnitCode, string targetUnitCode, bool applyDefaultRounding = true) => UnitConverter.Convert(typeof(Distance), sourceValue, sourceUnitCode, targetUnitCode, applyDefaultRounding);

			public static decimal ConvertSafe(decimal sourceValue, string sourceUnitCode, string targetUnitCode, bool applyDefaultRounding = true)
			{
				if (ContainsCode(sourceUnitCode) && ContainsCode(targetUnitCode))
				{
					return Convert(sourceValue, sourceUnitCode, targetUnitCode, applyDefaultRounding);
				}
				return 0m;
			}
		}

		public static class Length
		{
			[BaseUnit(0.001)]
			public const string Millimetres = "MM";

			[BaseUnit(0.01)]
			public const string Centimetres = "CM";

			[BaseUnit(1)]
			public const string Metres = "M";

			[BaseUnit(1000)]
			public const string Kilometres = "KM";

			[BaseUnit(1609.344)]
			public const string Miles = "MI";

			[BaseUnit(0.0254)]
			public const string Inches = "IN";

			[BaseUnit(0.3048)]
			public const string Feet = "FT";

			[BaseUnit(0.9144)]
			public const string Yards = "YD";

			public static readonly string[] Codes = new string[] { Millimetres, Centimetres, Metres, Kilometres, Miles, Inches, Feet, Yards };

			public static bool ContainsCode(string code)
			{
				return UnitConverter.ContainsCode(code, Codes);
			}

			public static MultilingualString GetDescription(string unit, PluralState pluralState)
			{
				switch (pluralState)
				{
					case PluralState.NonPlural:
						switch (unit)
						{
							case Centimetres:
								return ResString.GetMultilingualString("Resources|Constants|Length|Centimeter", "Centimeter");
							case Feet:
								return ResString.GetMultilingualString("Resources|Constants|Length|Foot", "Foot");
							case Inches:
								return ResString.GetMultilingualString("Resources|Constants|Length|Inch", "Inch");
							case Kilometres:
								return ResString.GetMultilingualString("Resources|Constants|Length|Kilometer", "Kilometer");
							case Metres:
								return ResString.GetMultilingualString("Resources|Constants|Length|Meter", "Meter");
							case Miles:
								return ResString.GetMultilingualString("Resources|Constants|Length|Mile", "Mile");
							case Millimetres:
								return ResString.GetMultilingualString("Resources|Constants|Length|Millimeter", "Millimeter");
							case Yards:
								return ResString.GetMultilingualString("Resources|Constants|Length|Yard", "Yard");
							default:
								return (NoResString)"";
						}

					case PluralState.Plural:
						switch (unit)
						{
							case Centimetres:
								return ResString.GetMultilingualString("Resources|Constants|Length|Centimetres", "Centimeters");
							case Feet:
								return ResString.GetMultilingualString("Resources|Constants|Length|Feet", "Feet");
							case Inches:
								return ResString.GetMultilingualString("Resources|Constants|Length|Inches", "Inches");
							case Kilometres:
								return ResString.GetMultilingualString("Resources|Constants|Length|Kilometres", "Kilometers");
							case Metres:
								return ResString.GetMultilingualString("Resources|Constants|Length|Metres", "Meters");
							case Miles:
								return ResString.GetMultilingualString("Resources|Constants|Length|Miles", "Miles");
							case Millimetres:
								return ResString.GetMultilingualString("Resources|Constants|Length|Millimetres", "Millimeters");
							case Yards:
								return ResString.GetMultilingualString("Resources|Constants|Length|Yards", "Yards");
							default:
								return (NoResString)"";
						}

					case PluralState.PluralOrNonPlural:
						switch (unit)
						{
							case Centimetres:
								return ResString.GetMultilingualString("Resources|Constants|Length|CentimetresPluralOrNonPlural", "Centimeter(s)");
							case Feet:
								return ResString.GetMultilingualString("Resources|Constants|Length|FeetPluralOrNonPlural", "Feet");
							case Inches:
								return ResString.GetMultilingualString("Resources|Constants|Length|InchesPluralOrNonPlural", "Inch(es)");
							case Kilometres:
								return ResString.GetMultilingualString("Resources|Constants|Length|KilometresPluralOrNonPlural", "Kilometer(s)");
							case Metres:
								return ResString.GetMultilingualString("Resources|Constants|Length|MetresPluralOrNonPlural", "Meter(s)");
							case Miles:
								return ResString.GetMultilingualString("Resources|Constants|Length|MilesPluralOrNonPlural", "Mile(s)");
							case Millimetres:
								return ResString.GetMultilingualString("Resources|Constants|Length|MillimetresPluralOrNonPlural", "Millimeter(s)");
							case Yards:
								return ResString.GetMultilingualString("Resources|Constants|Length|YardsPluralOrNonPlural", "Yard(s)");
							default:
								return (NoResString)"";
						}

					default:
						return (NoResString)"";
				}
			}

			public static decimal Convert(decimal sourceValue, string sourceUnitCode, string targetUnitCode, bool applyDefaultRounding = true)
			{
				return UnitConverter.Convert(typeof(Length), sourceValue, sourceUnitCode, targetUnitCode, applyDefaultRounding);
			}

			public static decimal ConvertSafe(decimal sourceValue, string sourceUnitCode, string targetUnitCode, bool applyDefaultRounding = true)
			{
				if (ContainsCode(sourceUnitCode) && ContainsCode(targetUnitCode))
				{
					return Convert(sourceValue, sourceUnitCode, targetUnitCode, applyDefaultRounding);
				}
				return 0m;
			}

			/// <summary>
			/// Gets just the number of feet from a decimal unit of length in feet.
			/// Eg. returns 10 if decimal is 10.5ft
			/// </summary>
			/// <param name="feetDecimal"></param>
			/// <returns>decimal the number of feet</returns>
			public static decimal GetFeetFromFeetDecimal(decimal feetDecimal)
			{
				return (decimal)Math.Floor((double)feetDecimal);
			}

			/// <summary>
			/// Gets just the number of inches from a decimal unit of length in feet.
			/// Eg. returns 6 (inches) if decimal is 10.5ft
			/// </summary>
			/// <param name="feetDecimal"></param>
			/// <returns></returns>
			public static decimal GetInchesFromFeetDecimal(decimal feetDecimal)
			{
				decimal inchesInFeet = feetDecimal - GetFeetFromFeetDecimal(feetDecimal);
				return UnitConverter.Convert(typeof(Length), inchesInFeet, Length.Feet, Length.Inches);
			}

			public static bool IsImperial(string unitCode)
			{
				if (unitCode == null)
				{
					return false;
				}

				return unitCode == Miles
					|| unitCode == Feet
					|| unitCode == Inches
					|| unitCode == Yards;
			}
		}

		public static class Volume
		{
			[BaseUnit(1)]
			public const string CubicMetres = "M3";

			[BaseUnit(0.001)]
			public const string CubicDecimetres = "D3";

			[BaseUnit(0.0283168466)]
			public const string CubicFeet = "CF";

			[BaseUnit(0.764554858)]
			public const string CubicYards = "CY";

			[BaseUnit(0.000016387064)]
			public const string CubicInches = "CI";

			[BaseUnit(0.001)]
			public const string Litre = "L";

			[BaseUnit(1000)]
			public const string MegaLitre = "ML";

			[BaseUnit(0.127551)]    // 61cm x 51cm x 41cm
			public const string TeaChest = "TE";

			[BaseUnit(0.000001)]
			public const string CubicCentimeters = "CC";

			[BaseUnit(0.003785411784)]
			public const string USGallons = "GA";

			[BaseUnit(0.00454609188)]
			public const string ImperialGallons = "GI";

			public static readonly string[] Codes = new string[]
				{
					CubicMetres,
					CubicDecimetres,
					CubicFeet,
					CubicYards,
					CubicInches,
					Litre,
					MegaLitre,
					TeaChest,
					CubicCentimeters,
					USGallons,
					ImperialGallons
				};

			public static bool ContainsCode(string code)
			{
				return UnitConverter.ContainsCode(code, Codes);
			}

			public static MultilingualString GetDescription(string code, PluralState pluralState)
			{
				switch (pluralState)
				{
					case PluralState.Plural:
						switch (code)
						{
							case CubicDecimetres:
								return ResString.GetMultilingualString("Resources|Constants|Volume|CubicDecimetres", "Cubic Decimeters");
							case CubicFeet:
								return ResString.GetMultilingualString("Resources|Constants|Volume|CubicFeet", "Cubic Feet");
							case CubicInches:
								return ResString.GetMultilingualString("Resources|Constants|Volume|CubicInches", "Cubic Inches");
							case CubicMetres:
								return ResString.GetMultilingualString("Resources|Constants|Volume|CubicMetres", "Cubic Meters");
							case CubicYards:
								return ResString.GetMultilingualString("Resources|Constants|Volume|CubicYards", "Cubic Yards");
							case Litre:
								return ResString.GetMultilingualString("Resources|Constants|Volume|Litre", "Liter");
							case MegaLitre:
								return ResString.GetMultilingualString("Resources|Constants|Volume|MegaLitre", "Mega Liter");
							case TeaChest:
								return ResString.GetMultilingualString("Resources|Constants|Volume|TeaChest", "Tea Chest");
							case CubicCentimeters:
								return ResString.GetMultilingualString("Resources|Constants|Volume|CubicCentimeters", "Cubic Centimeters");
							case USGallons:
								return ResString.GetMultilingualString("Resources|Constants|Volume|USGallons", "US Gallons");
							case ImperialGallons:
								return ResString.GetMultilingualString("Resources|Constants|Volume|ImperialGallons", "Imperial Gallons");
							default:
								return (NoResString)"";
						}
					case PluralState.PluralOrNonPlural:
						switch (code)
						{
							case CubicDecimetres:
								return ResString.GetMultilingualString("Resources|Constants|Volume|CubicDecimetresPluralOrNonPlural", "Cubic Decimeter(s)");
							case CubicFeet:
								return ResString.GetMultilingualString("Resources|Constants|Volume|CubicFeetPluralOrNonPlural", "Cubic Feet");
							case CubicInches:
								return ResString.GetMultilingualString("Resources|Constants|Volume|CubicInchesPluralOrNonPlural", "Cubic Inch(es)");
							case CubicMetres:
								return ResString.GetMultilingualString("Resources|Constants|Volume|CubicMetresPluralOrNonPlural", "Cubic Meter(s)");
							case CubicYards:
								return ResString.GetMultilingualString("Resources|Constants|Volume|CubicYardsPluralOrNonPlural", "Cubic Yard(s)");
							case Litre:
								return ResString.GetMultilingualString("Resources|Constants|Volume|LitrePluralOrNonPlural", "Liter(s)");
							case MegaLitre:
								return ResString.GetMultilingualString("Resources|Constants|Volume|MegaLitrePluralOrNonPlural", "Mega Liter(s)");
							case TeaChest:
								return ResString.GetMultilingualString("Resources|Constants|Volume|TeaChestPluralOrNonPlural", "Tea Chest(s)");
							case CubicCentimeters:
								return ResString.GetMultilingualString("Resources|Constants|Volume|CubicCentimetersPluralOrNonPlural", "Cubic Centimeters(s)");
							case USGallons:
								return ResString.GetMultilingualString("Resources|Constants|Volume|USGallonPluralOrNonPlural", "US Gallon(s)");
							case ImperialGallons:
								return ResString.GetMultilingualString("Resources|Constants|Volume|ImperialGallonPluralOrNonPlural", "Imperial Gallon(s)");
							default:
								return (NoResString)"";
						}
					case PluralState.NonPlural:
						switch (code)
						{
							case CubicDecimetres:
								return ResString.GetMultilingualString("Resources|Constants|Volume|CubicDecimetreNonPlural", "Cubic Decimeter");
							case CubicFeet:
								return ResString.GetMultilingualString("Resources|Constants|Volume|CubicFootNonPlural", "Cubic Foot");
							case CubicInches:
								return ResString.GetMultilingualString("Resources|Constants|Volume|CubicInchNonPlural", "Cubic Inch");
							case CubicMetres:
								return ResString.GetMultilingualString("Resources|Constants|Volume|CubicMetreNonPlural", "Cubic Meter");
							case CubicYards:
								return ResString.GetMultilingualString("Resources|Constants|Volume|CubicYardNonPlural", "Cubic Yard");
							case Litre:
								return ResString.GetMultilingualString("Resources|Constants|Volume|LitreNonPlural", "Liter");
							case MegaLitre:
								return ResString.GetMultilingualString("Resources|Constants|Volume|MegaLitreNonPlural", "Mega Liter");
							case TeaChest:
								return ResString.GetMultilingualString("Resources|Constants|Volume|TeaChestNonPlural", "Tea Chest");
							case CubicCentimeters:
								return ResString.GetMultilingualString("Resources|Constants|Volume|CubicCentimetersNonPlural", "Cubic Centimeters");
							case USGallons:
								return ResString.GetMultilingualString("Resources|Constants|Volume|USGallonNonPlural", "US Gallon");
							case ImperialGallons:
								return ResString.GetMultilingualString("Resources|Constants|Volume|ImperialGallonNonPlural", "Imperial Gallon");
							default:
								return (NoResString)"";
						}
					default:
						return (NoResString)"";
				}
			}

			public static decimal Convert(decimal sourceValue, string sourceUnitCode, string targetUnitCode, bool applyDefaultRounding = true)
			{
				return UnitConverter.Convert(typeof(Volume), sourceValue, sourceUnitCode, targetUnitCode, applyDefaultRounding);
			}

			public static decimal ConvertSafe(decimal sourceValue, string sourceUnitCode, string targetUnitCode, bool applyDefaultRounding = true)
			{
				if (ContainsCode(sourceUnitCode) && ContainsCode(targetUnitCode))
				{
					return Convert(sourceValue, sourceUnitCode, targetUnitCode, applyDefaultRounding);
				}
				return 0m;
			}

			public static double GetBaseUnit(string unitCode)
			{
				return UnitConverter.GetBaseValue(unitCode, typeof(Volume));
			}

			public static bool IsImperial(string unitCode)
			{
				if (unitCode == null)
				{
					return false;
				}

				return unitCode == CubicYards
					|| unitCode == CubicFeet
					|| unitCode == CubicInches;
			}
		}

		public static class Time
		{
			public const string Hours = "HR";
			public const string Days = "DY";
			public const string Weeks = "WK";
		}

		public static class LoadingLength
		{
			public const string LoadingMeters = "LM";

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
			public static readonly IEnumerable<string> Codes = new[] { LoadingMeters };

			public static bool ContainsCode(string code)
			{
				return UnitConverter.ContainsCode(code, Codes.ToArray());
			}

			public static bool IsImperial(string unitCode)
			{
				return false;
			}
		}

		public static class Weight
		{
			[BaseUnit(0.0002)]
			public const string MetricCarat = "MC";

			[BaseUnit(0.000001)]
			public const string Milligrams = "MG";

			[BaseUnit(0.001)]
			public const string Grams = "G";

			[BaseUnit(0.1)]
			public const string Hectograms = "HG";

			[BaseUnit(1)]
			public const string Kilograms = "KG";

			[BaseUnit(0.45359237)]
			public const string Pounds = "LB";

			[BaseUnit(0.0283495231)]
			public const string Ounces = "OZ";

			[BaseUnit(0.3732417217)]
			public const string PoundsTroy = "LT";

			[BaseUnit(0.03110347677)]
			public const string OuncesTroy = "OT";

			[BaseUnit(1000)]
			public const string Tonnes = "T";

			[BaseUnit(1000000)]
			public const string Kilotonnes = "KT";

			[BaseUnit(907.184995886)]
			public const string ShortTons = "TN";

			[BaseUnit(1016.04691)]
			public const string LongTons = "TL";

			[BaseUnit(100)]
			public const string Decitons = "DT";

			public static readonly string[] Codes = new string[] { MetricCarat, Milligrams, Grams, Hectograms, Kilograms, Pounds, Ounces, PoundsTroy, OuncesTroy, Tonnes, Kilotonnes, ShortTons, LongTons, Decitons };

			public static bool ContainsCode(string code)
			{
				return UnitConverter.ContainsCode(code, Codes);
			}

			public static MultilingualString GetDescription(string code, PluralState pluralState)
			{
				switch (pluralState)
				{
					case PluralState.Plural:
						switch (code)
						{
							case Decitons:
								return ResString.GetMultilingualString("Resources|Constants|Weight|Decitons", "Decitons");
							case Grams:
								return ResString.GetMultilingualString("Resources|Constants|Weight|Grams", "Grams");
							case Hectograms:
								return ResString.GetMultilingualString("Resources|Constants|Weight|Hectograms", "Hectograms");
							case Kilograms:
								return ResString.GetMultilingualString("Resources|Constants|Weight|Kilograms", "Kilograms");
							case Kilotonnes:
								return ResString.GetMultilingualString("Resources|Constants|Weight|Kilotonnes", "Kilotons");
							case LongTons:
								return ResString.GetMultilingualString("Resources|Constants|Weight|LongTons", "Long Tons (2240 lb)");
							case MetricCarat:
								return ResString.GetMultilingualString("Resources|Constants|Weight|MetricCarat", "Metric Carat");
							case Milligrams:
								return ResString.GetMultilingualString("Resources|Constants|Weight|Milligrams", "Milligrams");
							case Ounces:
								return ResString.GetMultilingualString("Resources|Constants|Weight|Ounces", "Ounces");
							case OuncesTroy:
								return ResString.GetMultilingualString("Resources|Constants|Weight|OuncesTroy", "Ounces Troy");
							case Pounds:
								return ResString.GetMultilingualString("Resources|Constants|Weight|Pounds", "Pounds");
							case PoundsTroy:
								return ResString.GetMultilingualString("Resources|Constants|Weight|PoundsTroy", "Pounds Troy");
							case ShortTons:
								return ResString.GetMultilingualString("Resources|Constants|Weight|ShortTons", "Short Tons (2000 lb)");
							case Tonnes:
								return ResString.GetMultilingualString("Resources|Constants|Weight|Tonnes", "Tonnes");
							default:
								return (NoResString)"";
						}
					case PluralState.PluralOrNonPlural:
						switch (code)
						{
							case Decitons:
								return ResString.GetMultilingualString("Resources|Constants|Weight|DecitonsPluralOrNonPlural", "Deciton(s)");
							case Grams:
								return ResString.GetMultilingualString("Resources|Constants|Weight|GramsPluralOrNonPlural", "Gram(s)");
							case Hectograms:
								return ResString.GetMultilingualString("Resources|Constants|Weight|HectogramsPluralOrNonPlural", "Hectogram(s)");
							case Kilograms:
								return ResString.GetMultilingualString("Resources|Constants|Weight|KilogramsPluralOrNonPlural", "Kilogram(s)");
							case Kilotonnes:
								return ResString.GetMultilingualString("Resources|Constants|Weight|KilotonnesPluralOrNonPlural", "Kiloton(s)");
							case LongTons:
								return ResString.GetMultilingualString("Resources|Constants|Weight|LongTonsPluralOrNonPlural", "Long Ton(s) (2240 lb)");
							case MetricCarat:
								return ResString.GetMultilingualString("Resources|Constants|Weight|MetricCaratPluralOrNonPlural", "Metric Carat(s)");
							case Milligrams:
								return ResString.GetMultilingualString("Resources|Constants|Weight|MilligramsPluralOrNonPlural", "Milligram(s)");
							case Ounces:
								return ResString.GetMultilingualString("Resources|Constants|Weight|OuncesPluralOrNonPlural", "Ounce(s)");
							case OuncesTroy:
								return ResString.GetMultilingualString("Resources|Constants|Weight|OuncesTroyPluralOrNonPlural", "Ounce(s) Troy");
							case Pounds:
								return ResString.GetMultilingualString("Resources|Constants|Weight|PoundsPluralOrNonPlural", "Pound(s)");
							case PoundsTroy:
								return ResString.GetMultilingualString("Resources|Constants|Weight|PoundsTroyPluralOrNonPlural", "Pound(s) Troy");
							case ShortTons:
								return ResString.GetMultilingualString("Resources|Constants|Weight|ShortTonsPluralOrNonPlural", "Short Ton(s) (2000 lb)");
							case Tonnes:
								return ResString.GetMultilingualString("Resources|Constants|Weight|TonnesPluralOrNonPlural", "Tonne(s)");
							default:
								return (NoResString)"";
						}
					case PluralState.NonPlural:
						switch (code)
						{
							case Decitons:
								return ResString.GetMultilingualString("Resources|Constants|Weight|DecitonNonPlural", "Deciton");
							case Grams:
								return ResString.GetMultilingualString("Resources|Constants|Weight|GramNonPlural", "Gram");
							case Hectograms:
								return ResString.GetMultilingualString("Resources|Constants|Weight|HectogramNonPlural", "Hectogram");
							case Kilograms:
								return ResString.GetMultilingualString("Resources|Constants|Weight|KilogramNonPlural", "Kilogram");
							case Kilotonnes:
								return ResString.GetMultilingualString("Resources|Constants|Weight|KilotonneNonPlural", "Kiloton");
							case LongTons:
								return ResString.GetMultilingualString("Resources|Constants|Weight|LongTonNonPlural", "Long Ton (2240 lb)");
							case MetricCarat:
								return ResString.GetMultilingualString("Resources|Constants|Weight|MetricCaratNonPlural", "Metric Carat");
							case Milligrams:
								return ResString.GetMultilingualString("Resources|Constants|Weight|MilligramNonPlural", "Milligram");
							case Ounces:
								return ResString.GetMultilingualString("Resources|Constants|Weight|OunceNonPlural", "Ounce");
							case OuncesTroy:
								return ResString.GetMultilingualString("Resources|Constants|Weight|OunceTroyNonPlural", "Ounce Troy");
							case Pounds:
								return ResString.GetMultilingualString("Resources|Constants|Weight|PoundNonPlural", "Pound");
							case PoundsTroy:
								return ResString.GetMultilingualString("Resources|Constants|Weight|PoundTroyNonPlural", "Pound Troy");
							case ShortTons:
								return ResString.GetMultilingualString("Resources|Constants|Weight|ShortTonNonPlural", "Short Ton (2000 lb)");
							case Tonnes:
								return ResString.GetMultilingualString("Resources|Constants|Weight|TonneNonPlural", "Tonne");
							default:
								return (NoResString)"";
						}
					default:
						return (NoResString)"";
				}
			}

			public static decimal Convert(decimal sourceValue, string sourceUnitCode, string targetUnitCode, bool applyDefaultRounding = true)
			{
				return UnitConverter.Convert(typeof(Weight), sourceValue, sourceUnitCode, targetUnitCode, applyDefaultRounding);
			}

			public static decimal ConvertSafe(decimal sourceValue, string sourceUnitCode, string targetUnitCode, bool applyDefaultRounding = true)
			{
				if (ContainsCode(sourceUnitCode) && ContainsCode(targetUnitCode))
				{
					return Convert(sourceValue, sourceUnitCode, targetUnitCode, applyDefaultRounding);
				}
				return 0m;
			}

			public static bool IsImperial(string unitCode)
			{
				if (unitCode == null)
				{
					return false;
				}

				return unitCode == Pounds
					|| unitCode == Ounces
					|| unitCode == PoundsTroy
					|| unitCode == OuncesTroy
					|| unitCode == ShortTons
					|| unitCode == LongTons;
			}
		}

		public static class Temperature
		{
			public const string Centigrade = "C";
			public const string Fahrenheit = "F";
			public const string Kelvin = "K";

			public static readonly string[] Codes = new[] { Centigrade, Fahrenheit, Kelvin };

			public static MultilingualString GetDescription(string code)
			{
				MultilingualString result = null;

				switch (code)
				{
					case Centigrade:
						result = ResString.GetMultilingualString("Resources|Constants|Temperature|Centigrade", "Centigrade");
						break;

					case Fahrenheit:
						result = ResString.GetMultilingualString("Resources|Constants|Temperature|Fahrenheit", "Fahrenheit");
						break;

					case Kelvin:
						result = ResString.GetMultilingualString("Resources|Constants|Temperature|Kelvin", "Kelvin");
						break;
				}

				return result ?? (NoResString)"";
			}

			public static decimal Convert(decimal sourceValue, string sourceUnitCode, string targetUnitCode)
			{
				CheckCodeIsValid(sourceUnitCode);
				CheckCodeIsValid(targetUnitCode);

				if (sourceUnitCode == targetUnitCode)
				{
					return sourceValue;
				}

				if (sourceUnitCode == Centigrade)
				{
					if (targetUnitCode == Fahrenheit)
					{
						return (((decimal)9 / 5) * sourceValue) + 32;
					}
					else
					{
						return sourceValue + 273.15m;
					}
				}
				else if (sourceUnitCode == Fahrenheit)
				{
					if (targetUnitCode == Centigrade)
					{
						return (sourceValue - 32) * ((decimal)5 / 9);
					}
					else
					{
						return (sourceValue + 459.67m) * ((decimal)5 / 9);
					}
				}
				else if (sourceUnitCode == Kelvin)
				{
					if (targetUnitCode == Centigrade)
					{
						return sourceValue - 273.15m;
					}
					else
					{
						return (((decimal)9 / 5) * sourceValue) - 459.67m;
					}
				}
				else
				{
					throw new NotSupportedException(string.Format("No temperature conversion from '{0}' to '{1}' exists.", sourceUnitCode, targetUnitCode));
				}
			}

			static void CheckCodeIsValid(string code)
			{
				if (!Codes.Contains(code))
				{
					throw new ArgumentException(string.Format("Unit '{0}' is not a recognized Temperature unit.", code));
				}
			}
		}

		#region Radioactive Units

		public static class RadioactiveUnits
		{
			[BaseUnit(1000000)]
			public const string Terabecquerel = "TBQ";

			[BaseUnit(1000)]
			public const string Gigabecquerel = "GBQ";

			[BaseUnit(1)]
			public const string Megabecquerel = "MBQ";

			[BaseUnit(37000)]
			public const string Curie = "CUR";

			[BaseUnit(37)]
			public const string Millicurie = "MCI";

			[BaseUnit(0.037)]
			public const string Microcurie = "UCI";

			#region SuppressResourceStringsCheckRegion

			public static string GetSymbol(string unit)
			{
				switch (unit)
				{
					case Terabecquerel:
						return "TBq";

					case Gigabecquerel:
						return "GBq";

					case Megabecquerel:
						return "MBq";

					case Curie:
						return "Ci";

					case Millicurie:
						return "mCi";

					case Microcurie:
						return "uCi";

					default:
						return string.Empty;
				}
			}

			#endregion

			public static string[] Codes => codes
				?? (codes = new string[]
				{
					Terabecquerel, Gigabecquerel, Megabecquerel, Curie, Millicurie, Microcurie
				});

			[ThreadStatic]
			static string[] codes;

			public static bool ContainsCode(string code)
			{
				return UnitConverter.ContainsCode(code, Codes);
			}

			public static MultilingualString GetDescription(string unit)
			{
				switch (unit)
				{
					case Terabecquerel:
						return ResString.GetMultilingualString("Resources|Constants|RadioactiveUnits|Terabecquerel", "Terabecquerel");
					case Gigabecquerel:
						return ResString.GetMultilingualString("Resources|Constants|RadioactiveUnits|Gigabecquerel", "Gigabecquerel");
					case Megabecquerel:
						return ResString.GetMultilingualString("Resources|Constants|RadioactiveUnits|Megabecquerel", "Megabecquerel");
					case Curie:
						return ResString.GetMultilingualString("Resources|Constants|RadioactiveUnits|Curie", "Curie");
					case Millicurie:
						return ResString.GetMultilingualString("Resources|Constants|RadioactiveUnits|Millicurie", "Millicurie");
					case Microcurie:
						return ResString.GetMultilingualString("Resources|Constants|RadioactiveUnits|Microcurie", "Microcurie");
					default:
						return (NoResString)"";
				}
			}

			public static decimal Convert(decimal sourceValue, string sourceUnitCode, string targetUnitCode, bool applyDefaultRounding = true)
			{
				return UnitConverter.Convert(typeof(RadioactiveUnits), sourceValue, sourceUnitCode, targetUnitCode, applyDefaultRounding);
			}

			public static decimal ConvertSafe(decimal sourceValue, string sourceUnitCode, string targetUnitCode, bool applyDefaultRounding = true)
			{
				if (ContainsCode(sourceUnitCode) && ContainsCode(targetUnitCode))
				{
					return Convert(sourceValue, sourceUnitCode, targetUnitCode, applyDefaultRounding);
				}
				return 0m;
			}
		}

		#endregion

		public static class Area
		{
			[BaseUnit(1)]
			public const string SquareMetre = "M2";

			[BaseUnit(0.0001)]
			public const string SquareCentimetre = "CM2";

			[BaseUnit(0.000001)]
			public const string SquareMillimetre = "MM2";

			[BaseUnit(0.00064516)]
			public const string SquareInch = "I2";

			[BaseUnit(0.09290304)]
			public const string SquareFoot = "F2";

			[BaseUnit(0.83612736)]
			public const string SquareYard = "Y2";

			[BaseUnit(1000000)]
			public const string SquareKilometer = "KM2";

			[BaseUnit(2589988.110336)]
			public const string SquareMile = "MI2";

			public static readonly string[] Codes = new string[] { SquareMetre, SquareCentimetre, SquareMillimetre, SquareInch, SquareFoot, SquareYard, SquareKilometer, SquareMile };

			public static bool ContainsCode(string code)
			{
				return UnitConverter.ContainsCode(code, Codes);
			}

			public static string GetAreaUnitForLengthUnit(string unitOfLength)
			{
				switch (unitOfLength)
				{
					case Core.Constants.Length.Metres:
						return SquareMetre;

					case Core.Constants.Length.Centimetres:
						return SquareCentimetre;

					case Core.Constants.Length.Millimetres:
						return SquareMillimetre;

					case Core.Constants.Length.Inches:
						return SquareInch;

					case Core.Constants.Length.Feet:
						return SquareFoot;

					case Core.Constants.Length.Yards:
						return SquareYard;

					case Core.Constants.Length.Kilometres:
						return SquareKilometer;

					case Core.Constants.Length.Miles:
						return SquareMile;

					default:
						throw new NotSupportedException(string.Format("Cannot find area unit for {0}", unitOfLength));
				}
			}

			public static MultilingualString GetDescription(string code, PluralState pluralState)
			{
				switch (pluralState)
				{
					case PluralState.Plural:
						switch (code)
						{
							case SquareCentimetre:
								return ResString.GetMultilingualString("Resources|Constants|Area|SquareCentimetre", "Square Centimeter");
							case SquareFoot:
								return ResString.GetMultilingualString("Resources|Constants|Area|SquareFoot", "Square Foot");
							case SquareInch:
								return ResString.GetMultilingualString("Resources|Constants|Area|SquareInch", "Square Inch");
							case SquareMetre:
								return ResString.GetMultilingualString("Resources|Constants|Area|SquareMetre", "Square Meter");
							case SquareMillimetre:
								return ResString.GetMultilingualString("Resources|Constants|Area|SquareMillimetre", "Square Millimeter");
							case SquareYard:
								return ResString.GetMultilingualString("Resources|Constants|Area|SquareYard", "Square Yard");
							case SquareKilometer:
								return ResString.GetMultilingualString("Resources|Constants|Area|SquareKilometer", "Square Kilometer");
							case SquareMile:
								return ResString.GetMultilingualString("Resources|Constants|Area|SquareMile", "Square Mile");
							default:
								return (NoResString)"";
						}
					case PluralState.PluralOrNonPlural:
						switch (code)
						{
							case SquareCentimetre:
								return ResString.GetMultilingualString("Resources|Constants|Area|SquareCentimetrePluralOrNonPlural", "Square Centimeter(s)");
							case SquareFoot:
								return ResString.GetMultilingualString("Resources|Constants|Area|SquareFootPluralOrNonPlural", "Square Feet");
							case SquareInch:
								return ResString.GetMultilingualString("Resources|Constants|Area|SquareInchPluralOrNonPlural", "Square Inch(es)");
							case SquareMetre:
								return ResString.GetMultilingualString("Resources|Constants|Area|SquareMetrePluralOrNonPlural", "Square Meter(s)");
							case SquareMillimetre:
								return ResString.GetMultilingualString("Resources|Constants|Area|SquareMillimetrePluralOrNonPlural", "Square Millimeter(s)");
							case SquareYard:
								return ResString.GetMultilingualString("Resources|Constants|Area|SquareYardPluralOrNonPlural", "Square Yard(s)");
							case SquareKilometer:
								return ResString.GetMultilingualString("Resources|Constants|Area|SquareKilometerPluralOrNonPlural", "Square Kilometer(s)");
							case SquareMile:
								return ResString.GetMultilingualString("Resources|Constants|Area|SquareMilePluralOrNonPlural", "Square Mile(s)");
							default:
								return (NoResString)"";
						}
					default:
						return (NoResString)"";
				}
			}

			public static decimal Convert(decimal sourceValue, string sourceUnitCode, string targetUnitCode)
			{
				return UnitConverter.Convert(typeof(Area), sourceValue, sourceUnitCode, targetUnitCode);
			}
		}

		public delegate MultilingualString UnitDescriptionCallback(string code, PluralState pluralState);

		[AttributeUsage(AttributeTargets.Field)]
		internal sealed class BaseUnitAttribute : Attribute
		{
			public BaseUnitAttribute(double value)
			{
				this.Value = value;
			}

			public readonly double Value;
		}

		public enum PluralState { Plural, NonPlural, PluralOrNonPlural }

		internal class UnitConverter
		{
			internal static bool ContainsCode(string code, string[] codes)
			{
				foreach (string one in codes)
				{
					if (one.Equals(code))
					{
						return true;
					}
				}
				return false;
			}

			internal static decimal Convert(Type uoQType, decimal sourceValue, string sourceUnitCode, string targetUnitCode, bool roundResult = true)
			{
				decimal result = 0m;

				if (sourceUnitCode.Length == 0 || targetUnitCode.Length == 0)
				{
					result = sourceValue;
				}
				else
				{
					double sourceBaseValue = GetBaseValue(sourceUnitCode, uoQType);
					double targetBaseValue = GetBaseValue(targetUnitCode, uoQType);

					double intermediateResult = ((double)sourceValue / targetBaseValue * sourceBaseValue);

					result = roundResult ? (decimal)Math.Round(intermediateResult, 6) : (decimal)intermediateResult;
				}

				return result;
			}

			static Hashtable UoQTypes
			{
				get { return uoQTypes ?? (uoQTypes = new Hashtable()); }
			}
			[ThreadStatic]
			static Hashtable uoQTypes;

			internal static double GetBaseValue(string unitCode1, Type uoQType)
			{
				string unitCode = unitCode1.ToUpper();
				Hashtable conversions = UoQTypes[uoQType] as Hashtable;
				if (conversions == null || conversions.Count == 0)
				{
					if (conversions == null)
					{
						conversions = new Hashtable();
					}
					int maxRetries = 2;
					while (maxRetries > 0)
					{
						UoQTypes[uoQType] = conversions;
						FieldInfo[] infos = uoQType.GetFields(BindingFlags.Public | BindingFlags.Static);
						foreach (FieldInfo info in infos)
						{
							object value = info.GetValue(null);
							if (value is string)
							{
								object[] unitAttributes = info.GetCustomAttributes(typeof(BaseUnitAttribute), false);
								BaseUnitAttribute unitAttrib = unitAttributes[0] as BaseUnitAttribute;
								conversions[value] = unitAttrib.Value;
							}
						}
						if (conversions.Count == 0)
						{
							maxRetries--;
							System.Threading.Thread.Sleep(100);
						}
						else
						{
							maxRetries = 0;
						}
					}
				}
				if (conversions.Contains(unitCode))
				{
					return (double)conversions[unitCode];
				}
				else
				{
					throw new ArgumentException(BuildReport(conversions, uoQType, unitCode1));
				}
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Dimension codes")]
			static string BuildReport(Hashtable conversions, Type uoQType, string unitCode)
			{
				StringBuilder report = new StringBuilder();

				var unitTypes = new Dictionary<Type, string>()
					{
						 { typeof(Area), "Area" },
					   { typeof(Length), "Length" },
					   { typeof(Volume), "Volume" },
					   { typeof(Weight), "Weight" }
					};
				var unitType = unitTypes[uoQType];

				switch (unitType)
				{
					case "Area":
						unitType = Res.GetString("DB678358-FC40-4601-B084-17552198EBFF", "Area");
						break;

					case "Length":
						unitType = Res.GetString("0E609BBA-C779-439A-B802-AB5000694C46", "Length");
						break;

					case "Volume":
						unitType = Res.GetString("20165285-4C3D-482B-86D6-C84C88F5A1D7", "Volume");
						break;

					case "Weight":
						unitType = Res.GetString("F0D57B57-D7BF-4C51-B467-8954C2CB36DC", "Weight");
						break;

					default:
						unitType = Res.GetString("91184DF2-8201-4318-B13B-04807D946063", "Unknown");
						break;
				}

				report.Append(Res.GetString("788A3773-8557-4749-87C5-B72BF705AFA1", "{0} unit '{1}' is invalid.", unitType, unitCode) + Environment.NewLine + Environment.NewLine);
				report.Append(Res.GetString("F9A45D0E-2DE8-4434-8CC2-4DFA752FAE7B", "The following are valid {0} unit types:", unitType) + Environment.NewLine);

				ArrayList sortedUnits = new ArrayList(conversions.Keys);
				sortedUnits.Sort();
				for (int i = 0; i < sortedUnits.Count; i++)
				{
					if (i > 0)
					{
						report.Append(", ");
					}

					report.Append(sortedUnits[i]);
				}

				report.Append(Environment.NewLine + Environment.NewLine);
				report.Append(Res.GetString("657290A0-3F8B-4B08-B4B6-32DD65B41263", "Please correct this error."));

				return report.ToString();
			}
		}

		#endregion

		public static class Ageing
		{
			public const string Current = "0";
			public const string OnePeriod = "1";
			public const string TwoPeriods = "2";
			public const string ThreePeriods = "3";
		}

		public static class AircraftType
		{
			public const string CAO = "CAO";
			public const string PAX = "PAX";
		}

		public static class AircraftTypeDescriptions
		{
			public static MultilingualString CAO { get { return ResString.GetMultilingualString("fee29515-aaa3-402a-895e-1b81fa5a02b4", "Cargo Aircraft Only"); } }
			public static MultilingualString PAX { get { return ResString.GetMultilingualString("c5deec44-5244-403c-ac47-018657f211a7", "Passenger And Cargo"); } }
		}

		public static class AgentType
		{
			public const string Direct = "DRT";
			public const string CoLoad = "CLD";
			public const string Agent = "AGT";
			public const string Charter = "CHT";
			public const string OnBoardCourier = "OBC";
			public const string Other = "OTH";
			public const string AWBCoload = "CLA";
			public const string AWBMaster = "CLM";
			public const string Courier = "COU";
		}

		public static class AgentTypeDescriptions
		{
			public static MultilingualString Direct { get { return ResString.GetMultilingualString("a22adb0c-50f4-4b7b-83ea-242fbf7e8797", "Direct"); } }
			public static MultilingualString CoLoad { get { return ResString.GetMultilingualString("b32adb0c-50f4-4b7b-83ea-242fbf7e8766", "Co-Load"); } }
			public static MultilingualString Agent { get { return ResString.GetMultilingualString("0736b5f3-1119-4f6e-ae22-648d81b6fce1", "Agent"); } }
			public static MultilingualString Charter { get { return ResString.GetMultilingualString("8f5cb7c0-548f-438c-ae42-694d1487f02d", "Charter"); } }
			public static MultilingualString OnBoardCourier { get { return ResString.GetMultilingualString("ee54ba24-dd30-4750-81ae-b69c54b9cfc9", "On Board Courier"); } }
			public static MultilingualString Other { get { return ResString.GetMultilingualString("6c5cb7c0-548f-438c-ae42-694d1487f02c", "Other"); } }
			public static MultilingualString AWBCoload { get { return ResString.GetMultilingualString("c6012a75-bb3f-46f7-9738-8b59bdd188f3", "AWB Co-Load"); } }
			public static MultilingualString AWBMaster { get { return ResString.GetMultilingualString("6c42718d-f5eb-4601-884b-16953f8d406c", "Multi AWB Master"); } }
			public static MultilingualString Courier { get { return ResString.GetMultilingualString("A949168A-0FA7-4F20-BC1A-EDF41F9C2891", "Courier"); } }
		}

		public static class TransportPlanningType
		{
			public const string Flight1 = "FL1";
			public const string Flight2 = "FL2";
			public const string Flight3 = "FL3";
			public const string Other = "OTH";
			public const string PreCarriage = "PRE";
			public const string OnForwarding = "ONF";
			public const string MainVessel = "MAI";
			public const string OnBoardCourier = "OBC";
			public const string Unaccompanied = "UNA";
		}

		public static class VesselType
		{
			public const string Barge = "BA";
			public const string CableShip = "CS";
			public const string CargoVessel = "CV";
			public const string Dredger = "DR";
			public const string DrillShip = "DS";
			public const string FishingVessel = "FV";
			public const string NavalVessel = "NV";
			public const string OilRig = "OR";
			public const string OtherVessels = "OV";
			public const string PassengerVessel = "PV";
			public const string ResearchVessel = "RV";
			public const string SupplyBoat = "SB";
			public const string TugBoat = "TB";
			public const string Yacht = "YA";
			public const string ContainerisedVessel = "CNT";
			public const string BulkCarrier = "BLK";
			public const string LiquidNaturalGasTanker = "LNG";
			public const string OilTanker = "OIL";
			public const string OtherTanker = "TNK";
			public const string CarCarringVessel = "CAR";
			public const string RollOnRollOff = "ROR";
			public const string DryCargoVessel = "DRY";
			public const string LiveStockVessel = "LVS";

			public static bool IsLiquidVessel(string code)
			{
				return
					code == OilTanker ||
					code == OtherTanker ||
					code == LiquidNaturalGasTanker;
			}
		}

		public static class FlightScheduleStatus
		{
			public const string Active = "ATV";
			public const string ArrivalDelay = "ADL";
			public const string Arrived = "ARV";
			public const string Cancelled = "CAN";
			public const string Departed = "DEP";
			public const string DepartureDelay = "DDL";
			public const string Diversion = "DIV";
			public const string Matched = "MTD";
			public const string PartiallyMatched = "PMD";
			public const string PreArrival = "PRA";
			public const string PreDeparture = "PRD";
			public const string Unmatched = "UMD";
			public const string Unknown = "UNK";
		}

		public static class VoyageType
		{
			public const string MainVoyage = "MAI";
			public const string SlotVoyage = "SLT";
		}

		public static class StaffCertificateType
		{
			public const string IATA = "IAT";
			public const string DG = "DGN";
			public const string Broker = "BRK";
			public const string Car = "CAR";
			public const string Truck = "TRK";
			public const string FKL = "FKL";
			public const string NID = "NID";
			public const string PAS = "PAS";
			public const string Misc = "MSC";
		}

		public static class StaffDefaultCertificateIDAndTrainingTypes
		{
			public const string APP = "APP";
			public const string BRK = "BRK";
			public const string DBH = "DBH";
			public const string DGN = "DGN";
			public const string DTA = "DTA";
			public const string FIN = "FIN";
			public const string FKL = "FKL";
			public const string IAT = "IAT";
			public const string MSC = "MSC";
			public const string NID = "NID";
			public const string PID = "PID";
			public const string TFN = "TFN";
			public const string TRK = "TRK";
			public const string WKP = "WKP";
			public const string BKG = "BKG";

			public const string BCT = "BCT";
			public const string CON = "CON";
			public const string CDN = "CDN";
			public const string CDL = "CDL";
			public const string REP = "REP";
			public const string RTD = "RTD";
			public const string CAR = "CAR";
			public const string EDL = "EDL";
			public const string HZM = "HZM";
			public const string LVC = "LVC";
			public const string MID = "MID";
			public const string NAI = "NAI";
			public const string NEX = "NEX";
			public const string OTD = "OTD";
			public const string PAS = "PAS";
			public const string PR1 = "PR1";
			public const string PR2 = "PR2";
			public const string SEN = "SEN";
			public const string AR1 = "AR1";
			public const string AR2 = "AR2";
			public const string MMD = "MMD";
			public const string USP = "USP";
			public const string VIM = "VIM";
			public const string VNI = "VNI";

			public const string CO1 = "CO1";
			public const string CO2 = "CO2";
			public const string CO3 = "CO3";
			public const string CS1 = "CS1";
			public const string CS2 = "CS2";
			public const string CM1 = "CM1";

			public const string ACE = "ACE";
			public const string APC = "APC";
			public const string CNO = "CNO";
			public const string COD = "COD";
		}

		public static CountryGuids CountryGuids
		{
			get { return CountryGuids.Instance; }
		}

		public static partial class CountryCodes
		{
			// For the list of constants, see the file CountryCodes.cs

			public static IEnumerable<string> GetAll()
				=> typeof(CountryCodes)
					.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
					.Where(x => x.IsLiteral && !x.IsInitOnly && !x.Name.StartsWith("_"))
					.Select(x => x.GetRawConstantValue().ToString());

			public static bool IsUsaOrTerritory(string code)
			{
				foreach (string country in UsaAndTerritoriesList)
				{
					if (country == code)
					{
						return true;
					}
				}
				return false;
			}

			public static IEnumerable<string> UsaAndTerritoriesList
			{
				get
				{
					if (usaAndTerritoriesList == null)
					{
						usaAndTerritoriesList = new List<string>();
						usaAndTerritoriesList.Add(UnitedStates);
						usaAndTerritoriesList.Add(PuertoRico);
						usaAndTerritoriesList.Add(VirginIslands);
						usaAndTerritoriesList.Add(Guam);
						usaAndTerritoriesList.Add(NorthernMarianaIslands);
						usaAndTerritoriesList.Add(AmericanSamoa);
					}
					return usaAndTerritoriesList;
				}
			}
			[ThreadStatic]
			static List<string> usaAndTerritoriesList;

			public static bool IsFilerIDEnabledUsaOrTerritory(string code)
			{
				if (code == UnitedStates || code == PuertoRico || code == VirginIslands)
				{
					return true;
				}

				return false;
			}

			public static bool IsFranceOrTerritory(string portOrCountry)
			{
				return FranceAndOverseasDepartmentsAndTerritories.Contains(new ZString(portOrCountry).Left(2).ToString());
			}

			public static bool IsFranceOrTerritoryNotReunion(string portOrCountry)
			{
				return FranceAndOverseasDepartmentsAndTerritories.Where(t => t != Constants.CountryCodes.Reunion).Contains(new ZString(portOrCountry).Left(2).ToString());
			}

			public static IEnumerable<string> FranceAndOverseasDepartmentsAndTerritories
			{
				get
				{
					if (franceAndOverseasDepartmentsAndTerritories == null)
					{
						franceAndOverseasDepartmentsAndTerritories = new List<string>()
						{
							Constants.CountryCodes.France,
							Constants.CountryCodes.Guadeloupe,
							Constants.CountryCodes.Martinique,
							Constants.CountryCodes.FrenchGuyana,
							Constants.CountryCodes.Reunion,
							Constants.CountryCodes.Mayotte,
							Constants.CountryCodes.FrenchPolynesia,
							Constants.CountryCodes.WallisAndFutunaIslands,
							Constants.CountryCodes.StPierreEtMiquelon,
							Constants.CountryCodes.SaintMartin,
							Constants.CountryCodes.SaintBarthelemy,
							Constants.CountryCodes.FrenchSouthernTerritories,
							Constants.CountryCodes.NewCaledonia
						};
					}
					return franceAndOverseasDepartmentsAndTerritories;
				}
			}
			[ThreadStatic]
			static List<string> franceAndOverseasDepartmentsAndTerritories;

			public static bool IsUnderFrenchCustomsJurisdiction(string portOrCountry)
			{
				return FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction.Contains(new ZString(portOrCountry).Left(2).ToString());
			}

			public static IEnumerable<string> FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction
			{
				get
				{
					if (franceAndOverseasDepartmentsUnderItsCustomsJurisdiction == null)
					{
						franceAndOverseasDepartmentsUnderItsCustomsJurisdiction = new List<string>()
						{
							Constants.CountryCodes.France,
							Constants.CountryCodes.FrenchGuyana,
							Constants.CountryCodes.Guadeloupe,
							Constants.CountryCodes.Martinique,
							Constants.CountryCodes.Mayotte,
							Constants.CountryCodes.Reunion,
							Constants.CountryCodes.SaintMartin,
							Constants.CountryCodes.SaintBarthelemy,
						};
					}
					return franceAndOverseasDepartmentsUnderItsCustomsJurisdiction;
				}
			}
			[ThreadStatic]
			static List<string> franceAndOverseasDepartmentsUnderItsCustomsJurisdiction;

			public static IEnumerable<string> GetCountriesAndTerritoriesBelongingToCustomsJurisdiction(string jurisdiction)
			{
				switch (jurisdiction)
				{
					case Constants.CountryCodes.UnitedStates:
						return new List<string>() { Constants.CountryCodes.UnitedStates, Constants.CountryCodes.PuertoRico };
					case Constants.CountryCodes.Switzerland:
						return new List<string>() { Constants.CountryCodes.Switzerland, Constants.CountryCodes.Liechtenstein };
					case Constants.CountryCodes.France:
						return FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction;
					default:
						return new List<string>() { jurisdiction };
				}
			}

			public static IEnumerable<string> EuCommonTransitCountries
			{
				get
				{
					if (euCommonTransitCountries == null)
					{
						euCommonTransitCountries = new List<string>();
						euCommonTransitCountries.Add(Core.Constants.CountryCodes.Andorra);
						euCommonTransitCountries.Add(Core.Constants.CountryCodes.Iceland);
						euCommonTransitCountries.Add(Core.Constants.CountryCodes.Liechtenstein);
						euCommonTransitCountries.Add(Core.Constants.CountryCodes.Macedonia);
						euCommonTransitCountries.Add(Core.Constants.CountryCodes.Norway);
						euCommonTransitCountries.Add(Core.Constants.CountryCodes.SanMarino);
						euCommonTransitCountries.Add(Core.Constants.CountryCodes.Serbia);
						euCommonTransitCountries.Add(Core.Constants.CountryCodes.SvalbardAndJanMayen);
						euCommonTransitCountries.Add(Core.Constants.CountryCodes.Switzerland);
						euCommonTransitCountries.Add(Core.Constants.CountryCodes.Turkey);
						euCommonTransitCountries.Add(Core.Constants.CountryCodes.UnitedKingdom);
					}
					return euCommonTransitCountries;
				}
			}
			[ThreadStatic]
			static List<string> euCommonTransitCountries;

			#region European Union Aviation Security Member List

			public static bool IsInEuropeanUnionAviationSecurityScheme(string code)
			{
				return EuropeanUnionAviationSecurityMembersList.Contains(code);
			}

			public static IEnumerable<string> EuropeanUnionAviationSecurityMembersList
			{
				get
				{
					if (europeanUnionAviationSecurityMembersList == null)
					{
						europeanUnionAviationSecurityMembersList = new List<string>
						{
							Constants.CountryCodes.Austria,
							Constants.CountryCodes.Belgium,
							Constants.CountryCodes.Bulgaria,
							Constants.CountryCodes.Croatia,
							Constants.CountryCodes.Cyprus,
							Constants.CountryCodes.CzechRepublic,
							Constants.CountryCodes.Denmark,
							Constants.CountryCodes.Estonia,
							Constants.CountryCodes.Finland,
							Constants.CountryCodes.France,
							Constants.CountryCodes.Germany,
							Constants.CountryCodes.Greece,
							Constants.CountryCodes.Hungary,
							Constants.CountryCodes.Iceland,
							Constants.CountryCodes.Ireland,
							Constants.CountryCodes.Italy,
							Constants.CountryCodes.Latvia,
							Constants.CountryCodes.Liechtenstein,
							Constants.CountryCodes.Lithuania,
							Constants.CountryCodes.Luxembourg,
							Constants.CountryCodes.Malta,
							Constants.CountryCodes.Netherlands,
							Constants.CountryCodes.Norway,
							Constants.CountryCodes.Poland,
							Constants.CountryCodes.Portugal,
							Constants.CountryCodes.Romania,
							Constants.CountryCodes.Slovakia,
							Constants.CountryCodes.Slovenia,
							Constants.CountryCodes.Spain,
							Constants.CountryCodes.Sweden,
							Constants.CountryCodes.Switzerland
						};
					}

					return europeanUnionAviationSecurityMembersList;
				}
			}
			[ThreadStatic]
			static List<string> europeanUnionAviationSecurityMembersList;

			#endregion

			#region Specific African Country Code

			public static bool IsSpecificAfricanCountryCode(string countryCode)
			{
				return SpecificAfricanCountryCodeList.Contains(countryCode);
			}

			public static IEnumerable<string> SpecificAfricanCountryCodeList
			{
				get
				{
					if (specificAfricanCountryCodeList == null)
					{
						specificAfricanCountryCodeList = new List<string>
						{
							Constants.CountryCodes.Ghana,
							Constants.CountryCodes.Angola,
							Constants.CountryCodes.Benin,
							Constants.CountryCodes.BurkinaFaso,
							Constants.CountryCodes.Burundi,
							Constants.CountryCodes.Cameroon,
							Constants.CountryCodes.Chad,
							Constants.CountryCodes.CentralAfricanRepublic,
							Constants.CountryCodes.EquatorialGuinea,
							Constants.CountryCodes.DemocraticRepublicOfCongo,
							Constants.CountryCodes.Congo,
							Constants.CountryCodes.Gabon,
							Constants.CountryCodes.Guinea,
							Constants.CountryCodes.GuineaBissau,
							Constants.CountryCodes.CoteDivoire,
							Constants.CountryCodes.Liberia,
							Constants.CountryCodes.Madagascar,
							Constants.CountryCodes.Mali,
							Constants.CountryCodes.Niger,
							Constants.CountryCodes.Senegal,
							Constants.CountryCodes.SierraLeone,
							Constants.CountryCodes.Sudan,
							Constants.CountryCodes.SouthSudan,
							Constants.CountryCodes.Togo,
							Constants.CountryCodes.Somalia
						};
					}

					return specificAfricanCountryCodeList;
				}
			}
			[ThreadStatic]
			static List<string> specificAfricanCountryCodeList;

			#endregion

			public static string GetCustomsCountryOfJurisdiction(string code)
			{
				switch (code)
				{
					case PuertoRico:
						return UnitedStates;
					case Liechtenstein:
						return Switzerland;
					case NorthernIreland_ForUseOnlyByEuInCertainScopes:
						return UnitedKingdom;
					case string territory when FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction.Contains(territory):
						return France;
				}
				return code;
			}

			public static string GetCustomsCountryOfJurisdictionOrEU(string code) => IsInEuropeanCustomsUnion(code) ? EuropeanUnion : GetCustomsCountryOfJurisdiction(code);

			public static bool IsInEuropeanCustomsUnion(string code) => ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnion(code);
		}

		public static class DecimalPlaces
		{
			public const int DefaultNumberOfDecimalsForPercentages = 2;
			public const int DefaultNumberOfDecimalsForIndividualUnits = 0;
		}

		public static class CurrencyCodes
		{
			public const string Afghanistan = "AFA";
			public const string Albania = "ALL";
			public const string Algeria = "DZD";
			public const string AmericanSamoa = "USD";
			public const string Andorra = "EUR";
			public const string Angola = "AOA";
			public const string Anguilla = "XCD";
			public const string AntiguaAndBarbuda = "XCD";
			public const string Argentina = "ARS";
			public const string Armenia = "AMD";
			public const string Aruba = "AWG";
			public const string Australia = "AUD";
			public const string Austria = "EUR";
			public const string Azerbaijan = "AZM";
			public const string Bahamas = "BSD";
			public const string Bahrain = "BHD";
			public const string Bangladesh = "BDT";
			public const string Barbados = "BBD";
			public const string Belarus = "BYR";
			public const string Belgium = "EUR";
			public const string Belize = "BZD";
			public const string Benin = "XOF";
			public const string Bermuda = "BMD";
			public const string Bhutan = "BTN";
			public const string Bolivia = "BOB";
			public const string BosniaAndHerzegovina = "BAM";
			public const string Botswana = "BWP";
			public const string Brazil = "BRL";
			public const string BruneiDarussalam = "BND";
			public const string Bulgaria = "BGL";
			public const string BulgariaNew = "BGN";
			public const string BurkinaFaso = "XOF";
			public const string Burundi = "BIF";
			public const string Cambodia = "KHR";
			public const string Cameroon = "XAF";
			public const string Canada = "CAD";
			public const string CapeVerde = "CVE";
			public const string CaymanIslands = "KYD";
			public const string CentralAfricanRepublic = "XAF";
			public const string Chad = "XAF";
			public const string Chile = "CLP";
			public const string China = "CNY";
			public const string ChristmasIsland = "AUD";
			public const string CocosKkeelingIslands = "AUD";
			public const string Colombia = "COP";
			public const string Comoros = "KMF";
			public const string Congo = "XAF";
			public const string CongoTheDemocraticRepublicOf = "CDF";
			public const string CookIslands = "NZD";
			public const string CostaRica = "CRC";
			public const string CoteDIvoire = "XOF";
			public const string Croatia = "HRK";
			public const string Cuba = "CUP";
			public const string Cyprus = "CYP";
			public const string CzechRepublic = "CZK";
			public const string Denmark = "DKK";
			public const string Djibouti = "DJF";
			public const string Dominica = "XCD";
			public const string DominicanRepublic = "DOP";
			public const string EastTimor = "USD";
			public const string Ecuador = "USD";
			public const string Egypt = "EGP";
			public const string ElSalvador = "SVC";
			public const string EquatorialGuinea = "XAF";
			public const string Eritrea = "ERN";
			public const string Estonia = "EEK";
			public const string Ethiopia = "ETB";
			public const string EuropeanUnion = "EUR";
			public const string FalklandIslands = "FKP";
			public const string FaroeIslands = "DKK";
			public const string Fiji = "FJD";
			public const string Finland = "EUR";
			public const string France = "EUR";
			public const string FrenchGuiana = "EUR";
			public const string FrenchPolynesia = "XPF";
			public const string FrenchSouthernTerritories = "EUR";
			public const string Gabon = "XAF";
			public const string Gambia = "GMD";
			public const string Georgia = "GEL";
			public const string Germany = "EUR";
			public const string Ghana = "GHS";
			public const string Gibraltar = "GIP";
			public const string Greece = "EUR";
			public const string Greenland = "DKK";
			public const string Grenada = "XCD";
			public const string Guadeloupe = "EUR";
			public const string Guam = "USD";
			public const string Guatemala = "GTQ";
			public const string Guinea = "GNF";
			public const string GuineaBissau = "XOF";
			public const string Guyana = "GYD";
			public const string Haiti = "HTG";
			public const string HeardIslandAndMcdonaldIslands = "AUD";
			public const string HolySee = "EUR";
			public const string Honduras = "HNL";
			public const string HongKong = "HKD";
			public const string Hungary = "HUF";
			public const string Iceland = "ISK";
			public const string India = "INR";
			public const string Indonesia = "IDR";
			public const string Iran = "IRR";
			public const string Iraq = "IQD";
			public const string Ireland = "EUR";
			public const string Israel = "ILS";
			public const string Italy = "EUR";
			public const string Jamaica = "JMD";
			public const string Japan = "JPY";
			public const string Jordan = "JOD";
			public const string Kazakhstan = "KZT";
			public const string Kenya = "KES";
			public const string Kiribati = "AUD";
			public const string KoreaDemocraticPeoplesRepublic = "KPW";
			public const string KoreaRepublicOf = "KRW";
			public const string Kuwait = "KWD";
			public const string Kyrgyzstan = "KGS";
			public const string Lao = "LAK";
			public const string Latvia = "LVL";
			public const string Lebanon = "LBP";
			public const string Lesotho = "LSL";
			public const string Liberia = "LRD";
			public const string LibyanArabJamahiriya = "LYD";
			public const string Liechtenstein = "CHF";
			public const string Lithuania = "LTL";
			public const string Luxembourg = "EUR";
			public const string Macau = "MOP";
			public const string Macedonia = "MKD";
			public const string Madagascar = "MGA";
			public const string Malawi = "MWK";
			public const string Malaysia = "MYR";
			public const string Maldives = "MVR";
			public const string Mali = "XOF";
			public const string Malta = "MTL";
			public const string MarshallIslands = "USD";
			public const string Martinique = "EUR";
			public const string Mauritania = "MRO";
			public const string Mauritius = "MUR";
			public const string Mayotte = "EUR";
			public const string Mexico = "MXN";
			public const string Micronesia = "USD";
			public const string Moldova = "MDL";
			public const string Monaco = "EUR";
			public const string Mongolia = "MNT";
			public const string Montserrat = "XCD";
			public const string Morocco = "MAD";
			public const string Mozambique = "MZM";
			public const string Myanmar = "MMK";
			public const string Namibia = "NAD";
			public const string Nauru = "AUD";
			public const string Nepal = "NPR";
			public const string Netherlands = "EUR";
			public const string NetherlandsAntilles = "ANG";
			public const string NewCaledonia = "XPF";
			public const string NewZealand = "NZD";
			public const string Nicaragua = "NIO";
			public const string Niger = "XOF";
			public const string Nigeria = "NGN";
			public const string Niue = "NZD";
			public const string NorfolkIsland = "AUD";
			public const string NorthernMarianaIslands = "USD";
			public const string Norway = "NOK";
			public const string Oman = "OMR";
			public const string Pakistan = "PKR";
			public const string Palau = "USD";
			public const string PalestinianTerritoryOccupied = "ILS";
			public const string Panama = "PAB";
			public const string PapuaNewGuinea = "PGK";
			public const string Paraguay = "PYG";
			public const string Peru = "PEN";
			public const string Philippines = "PHP";
			public const string Pitcairn = "NZD";
			public const string Poland = "PLN";
			public const string Portugal = "EUR";
			public const string PuertoRico = "USD";
			public const string Qatar = "QAR";
			public const string Reunion = "EUR";
			public const string Romania = "ROL";
			public const string RomaniaNew = "RON";
			public const string Russia = "RBL";
			public const string RussianFederation = "RUR";
			public const string Rwanda = "RWF";
			public const string SaintHelena = "SHP";
			public const string SaintKittsAndNevis = "XCD";
			public const string SaintLucia = "XCD";
			public const string SaintPierreAndMiquelon = "EUR";
			public const string SaintVincentAndTheGrenadines = "XCD";
			public const string Samoa = "WST";
			public const string SanMarino = "EUR";
			public const string SaoTomeAndPrincipe = "STD";
			public const string SaudiArabia = "SAR";
			public const string Senegal = "XOF";
			public const string Seychelles = "SCR";
			public const string SierraLeone = "SLL";
			public const string Singapore = "SGD";
			public const string Slovakia = "SKK";
			public const string Slovenia = "SIT";
			public const string SolomonIslands = "SBD";
			public const string Somalia = "SOS";
			public const string SouthAfrica = "ZAR";
			public const string SouthGeorgiaAndTheSouthSandwic = "GBP";
			public const string Spain = "EUR";
			public const string SriLanka = "LKR";
			public const string Sudan = "SDD";
			public const string Suriname = "SRG";
			public const string SvalbardAndJanMayen = "NOK";
			public const string Swaziland = "SZL";
			public const string Sweden = "SEK";
			public const string Switzerland = "CHF";
			public const string SyrianArabRepublic = "SYP";
			public const string Taiwan = "TWD";
			public const string Tajikistan = "TJR";
			public const string Tanzania = "TZS";
			public const string Thailand = "THB";
			public const string TimorLeste = "USD";
			public const string Togo = "XOF";
			public const string Tokelau = "NZD";
			public const string Tonga = "TOP";
			public const string TrinidadAndTobago = "TTD";
			public const string Tunisia = "TND";
			public const string Turkey = "TRY";
			public const string Turkmenistan = "TMM";
			public const string TurksAndCaicosIslands = "USD";
			public const string Tuvalu = "AUD";
			public const string Uganda = "UGX";
			public const string Ukraine = "UAH";
			public const string UnitedArabEmirates = "AED";
			public const string UnitedKingdom = "GBP";
			public const string UnitedStates = "USD";
			public const string UnitedStatesMinorOutlyingIsland = "USD";
			public const string Uruguay = "UYU";
			public const string Uzbekistan = "UZS";
			public const string Vanuatu = "VUV";
			public const string Venezuela = "VEB";
			public const string VietNam = "VND";
			public const string VirginIslands = "USD";
			public const string VirginIslandsBritish = "USD";
			public const string WallisAndFutuna = "XPF";
			public const string WesternSahara = "MAD";
			public const string Yemen = "YER";
			public const string Yugoslavia = "YUM";
			public const string Zambia = "ZMK";
			public const string Zimbabwe = "ZWD";

			public static IEnumerable<string> All => typeof(CurrencyCodes).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
			.Where(x => x.IsLiteral && !x.IsInitOnly)
			.Select(x => x.GetRawConstantValue().ToString());
		}

		public static class ShipmentReleaseTypes
		{
			public const string BankLetterOfCredit = "BRR";
			public const string BankSightDraft = "BSD";
			public const string BankTimeDraft = "BTD";
			public const string NonNegotiable = "NON";
			public const string ExpressBofL = "EBL";
			public const string Indemnity = "LOI";
			public const string OriginalReq = "OBR";
			public const string OriginalReqSurrender = "OBO";
			public const string Cheque = "CSH";
			public const string CashDoc = "CAD";
			public const string SeaWaybill = "SWB";
		}

		public static class ShipmentScreenOptions
		{
			public const string Auto = "AUT";
			public const string Consignor = "NOR";
			public const string Consignee = "NEE";
		}

		public static class ShipmentWeightUpdateOptions
		{
			public static bool IsUpdateFromConsignmentsDetails(string code)
			{
				return code == Constants.ShipmentWeightUpdateOptions.Code.ShowWarningFromConsignmentsDetails
					|| code == Constants.ShipmentWeightUpdateOptions.Code.AlwaysUpdateFromConsignmentsDetails;
			}

			public static bool IsUpdateFromPackingDetails(string code)
			{
				return code == Constants.ShipmentWeightUpdateOptions.Code.ShowWarningFromPackingDetails
					|| code == Constants.ShipmentWeightUpdateOptions.Code.AlwaysUpdateFromPackingDetails;
			}

			public static bool IsAlwaysUpdate(string code)
			{
				return code == Constants.ShipmentWeightUpdateOptions.Code.AlwaysUpdateFromPackingDetails
					|| code == Constants.ShipmentWeightUpdateOptions.Code.AlwaysUpdateFromConsignmentsDetails;
			}

			public static bool IsShowWarning(string code)
			{
				return code == Constants.ShipmentWeightUpdateOptions.Code.ShowWarningFromConsignmentsDetails
					|| code == Constants.ShipmentWeightUpdateOptions.Code.ShowWarningFromPackingDetails;
			}

			public static class Code
			{
				public const string ShowWarningFromConsignmentsDetails = "WAR";
				public const string AlwaysUpdateFromConsignmentsDetails = "UPT";
				public const string DoNotUpdate = "NPT";
				public const string ShowWarningFromPackingDetails = "WRP";
				public const string AlwaysUpdateFromPackingDetails = "PAC";
			}

			public static class Description
			{
				public static MultilingualString ShowWarningFromConsignmentsDetails { get { return ResString.GetMultilingualString("02f96ed5-9207-4183-bed5-d30195b899cb", "Show warning message before updating totals from consignments details."); } }
				public static MultilingualString AlwaysUpdateFromConsignmentsDetails { get { return ResString.GetMultilingualString("6ce2d9e4-2b80-42e0-9ad9-87c46f19d898", "Always update shipment totals using consignments details."); } }
				public static MultilingualString DoNotUpdate { get { return ResString.GetMultilingualString("28c52f67-fad3-4e4a-aa49-43e1739547af", "Do not update shipment totals"); } }
				public static MultilingualString ShowWarningFromPackingDetails { get { return ResString.GetMultilingualString("d63f8395-7071-4e73-a488-fc21a815c68d", "Show warning message before updating totals from packing details."); } }
				public static MultilingualString AlwaysUpdateFromPackingDetails { get { return ResString.GetMultilingualString("67c3136b-a320-4a6e-9d77-61ba527a86ea", "Update shipment totals using pack lines weights and volumes."); } }
			}
		}

		public static class ShippingLineMessagingRequirement
		{
			public static class Code
			{
				public const string ContractNumberMandatory = "CON";
				public const string NamedAccountMandatory = "NAM";
				public const string DGNetWeightMandatory = "DGW";
				public const string AcceptEitherAirflowOrHumidity = "AOH";
				public const string DimensionsMandatoryForOOG = "DOG";
				public const string HarmonisedCode = "HSC";
				public const string BillOfLadingProvider = "BLP";
				public const string AttachFormAsPDFInMessage = "FOM";
				public const string SealNumberMandatory = "SEL";
				public const string IntegrationViaEmailToCarrierLocalOffice = "IEL";
			}

			public static class Description
			{
				public static MultilingualString ContractNumberMandatory { get { return ResString.GetMultilingualString("ceea502b-5bd0-4e0d-bcaa-43170e0b6b0c", "Contract Number Mandatory"); } }
				public static MultilingualString NamedAccountMandatory { get { return ResString.GetMultilingualString("521175c3-bb27-4d84-8b7e-ad1ba1fe95f3", "Named Account Mandatory"); } }
				public static MultilingualString DGNetWeightMandatory { get { return ResString.GetMultilingualString("5a0ff8af-17aa-4de2-bfc7-339343a11b38", "DG Net Weight Mandatory"); } }
				public static MultilingualString AcceptEitherAirflowOrHumidity { get { return ResString.GetMultilingualString("f23c001c-842d-4b0a-8eef-8e9a4d07facf", "Accept either Airflow or Humidity"); } }
				public static MultilingualString DimensionsMandatoryForOOG { get { return ResString.GetMultilingualString("38d99472-007a-447b-a132-eef45ecb01cc", "Dimensions Mandatory for OOG"); } }
				public static MultilingualString HarmonisedCode { get { return ResString.GetMultilingualString("15abee68-7deb-4adc-8097-35794a816485", "Harmonized System (HS) Code Mandatory"); } }
				public static MultilingualString BillOfLadingProvider { get { return ResString.GetMultilingualString("a16c3664-b14b-400e-8c19-a99ddf067335", "Electronic Bill of Lading Provider Mandatory"); } }
				public static MultilingualString AttachFormAsPDFInMessage { get { return ResString.GetMultilingualString("49ebe1a1-801b-4657-83c2-cc467141318e", "Attach Form as PDF in Message"); } }
				public static MultilingualString SealNumberMandatory { get { return ResString.GetMultilingualString("2db85789-d6e4-45d6-8b42-71dc083ef54a", "Seal Number Mandatory"); } }
				public static MultilingualString IntegrationViaEmailToCarrierLocalOffice { get { return ResString.GetMultilingualString("91f37d29-cf3c-4b2c-afd6-2967bbf7b810", "Integration via email to carrier local office"); } }
			}
		}

		public static class OSMGSecurityLevels
		{
			public const string Standard = "STD";
			public const string Enhanced = "ENH";
		}

		public static class CRMSecurityCaptions
		{
			public static MultilingualString ModifyStaffAssignment => ResString.GetMultilingualString("01dc498e-9870-4ee4-afee-93431be02431", "Modify Staff Assignment");

			public static MultilingualString ViewByStaffNotAssigned => ResString.GetMultilingualString("066a5bb7-a727-4e07-a5bf-647fb07169ab", "Search and View Records Assigned to Other Login Staff");

			public static MultilingualString EditByStaffNotAssigned => ResString.GetMultilingualString("02cb5908-78e9-4cf7-b0e4-c10fe3a72b3e", "Edit by Staff Not Assigned");

			public static MultilingualString IgnoreOSMG => ResString.GetMultilingualString("04000aa5-db32-4fed-b54e-0b0b025d9337", "Permit Unconditional access regardless of Org. Security Groups");

			public static MultilingualString IgnoreTaskAssignment => ResString.GetMultilingualString("068518a1-0bfe-4f0f-8e29-0b6f047c397a", "Permit Unconditional access regardless of Task assignment");
		}

		public static class AccountsCategory
		{
			public const string Unrelated = "UNR";
			public const string WhollyOwned = "WHO";
			public const string MinorityWithReporting = "MIR";
			public const string MinorityWithNoReporting = "MIN";
			public const string Standard = "STD";
			public const string Key = "KEY";
			public const string Significant = "SIG";
			public const string Alternative = "ALT";

			public const string RelatedMinorityShareholder = "RMI";
			public const string RelatedMajorityShareholder = "RMJ";
			public const string RelatedWhollyOwningShareholder = "RWH";
			public const string GroupCompanyRelatedMinority = "GMI";
			public const string GroupCompanyRelatedMajority = "GMJ";
		}

		public static class FCLEquipmentNeeded
		{
			public const string WaitForUnpack = "WUP";
			public const string SideLoader = "SDL";
			public const string Trailer = "TRL";
			public const string LiftOffOn = "LOF";
		}

		public static class LCLAIREquipmentNeeded
		{
			public const string Premise = "PSL";
			public const string Haulier = "HSL";
			public const string HandUnloadLoad = "HUL";
			public const string HandHaulier = "HWL";
		}

		public static class PaymentType
		{
			public const string Prepaid = "PPD";
			public const string Collect = "CCX";
		}

		public static class OrderStatus
		{
			public const string All = "ALL";
			public const string Open = "PLC";
			public const string Incomplete = "INC";
			public const string Confirmed = "CNF";
			public const string Shipped = "SHP";
			public const string Delivered = "DLV";
			public const string Cancelled = "CAN";
			public const string PartDelivered = "PRT";
			public const string PartDeliveredQuantityAmendedToZero = "PRZ";
		}

		public static class SupplierBookingStatus
		{
			public const string Approved = "APP";
			public const string Cancelled = "CAN";
			public const string Incomplete = "INC";
			public const string Placed = "PLC";
			public const string Planned = "PLN";
			public const string Rejected = "REJ";
			public const string Received = "RCV";
			public const string Converted = "CNV";
			public const string Shipped = "SHP";
		}

		public static class SupplierBookingLoadMode
		{
			public const string ContainerFreightStation = "CFS";
			public const string ContainerYard = "CY";
			public const string LooseCargo = "LSE";
		}

		public static class ContainerLoadListHeaderStatus
		{
			public const string Incomplete = "INC";
			public const string Placed = "PLC";
			public const string Approved = "APP";
			public const string Rejected = "REJ";
			public const string Shipped = "SHP";
			public const string Cancelled = "CAN";
			public const string Converted = "CNV";
			public const string Planned = "PLN";
		}

		public static class ContainerLoadListHeaderLoadMode
		{
			public const string ContainerFreightStation = "CFS";
			public const string ContainerYard = "CY";
		}

		public static class PayableOrderStage
		{
			public const string Request = "REQ";
			public const string Order = "ORD";
			public const string Track = "TRC";
			public const string Receive = "RCV";
		}

		public static class PayableOrderDisposition
		{
			public const string OrderIncomplete = "OIC";
			public const string PendingApproval = "PAP";
			public const string OrderToBePlaced = "OTP";
			public const string PendingConfirmation = "PCF";
			public const string ExpectedDLVPending = "EDP";
			public const string DeliveryInProgress = "DIP";
			public const string APInvoiceToBePosted = "ITP";
			public const string PendingGoodsReceivedAudit = "PGR";
			public const string Complete = "COM";
		}

		public static class PayableOrderType
		{
			public const string VariableOverhead = "VOD";
			public const string CapitalExpense = "CEX";
			public const string IndirectCostOfSale = "ICS";
			public const string BulkPurchase = "BPE";
		}

		public static class PayableOrderGoodsStatus
		{
			public const string NotReceived = "NOT";
			public const string FullyReceived = "FRC";
			public const string PartiallyReceived = "PRC";
			public const string OverSupplied = "OSP";
			public const string OverInvoiced = "OIV";
			public const string UnderInvoiced = "UIV";
		}

		public static class PayableOrderLineStatus
		{
			public const string Placed = "PLC";
			public const string PartDelivered = "PRT";
			public const string Delivered = "DLV";
			public const string Cancelled = "CAN";
		}

		public static class VGMVerifiedParties
		{
			public const string OwnSendingAgent = "OSA";
			public const string AnySendingAgent = "ASA";
			public const string DepartureCFS = "DCF";
		}

		public static class HBLPackLinesDisplayOrders
		{
			public const string ContainerAndPackingOrder = "ContainerAndPackingOrder";
			public const string ShowDGCargoFirst = "ShowDGCargoFirst";
		}

		#region Customs

		public static class Customs
		{
			public static class Universal
			{
				public static class DataSetTypes
				{
					public const string WTGData = "Z";
					public const string OWNData = "O";
				}

				public static class RefDataGrouping
				{
					public static class Codes
					{
						public const string CommonDataGrouping = "ZZ";
						public const string EuropeanUnionEUN = "EUN";
						public const string UnitedNationsRecommendations = "UNE";
						public const string WorldCustomsOrganisationWCO = "WCO";
						public const string CustomsDeclarationService = "CDS";
						public const string IEUcc5 = "IE5";
						public const string IEUCC6V1 = "IE5";
						public const string DeltaIE = "DIE";
						public const string GulfCooperationCouncil = "GCC";
						public const string UAEManifest = "AEM";
					}
				}

				public static class RefCusCodeListTypes
				{
					public static class Codes
					{
						public const string DutyExemptionRefundCode = "DTYER";
						public const string DutyExemptionCode = "DTYE";
						public const string ITARExemptionNumber = "ITAR";
						public const string JapanTariffExemptionCode = "TRFEX";
						public const string JapanImportConsumptionTaxExemptionCode = "CTEI";
						public const string JapanExportConsumptionTaxExemptionCode = "CTEC";
						public const string JapanSpecialCargoCode = "SPC";
						public const string JapanAFRDeleteReason = "AFRDR";
						public const string JapanOtherLaws = "OLC";
						public const string JapanCustomsOfficeDepartment = "DEPTC";
						public const string JapanExportTradeControlOrdinanceAppendix = "ETCOA";
						public const string JapanImportTradeControlOrdinanceAppendix = "ITCOA";
						public const string JapanBondedAreaCode = "JPBLC";
						public const string JapanDocumentTypes = "JPDOC";
						public const string JapanPackageTypes = "JPPKG";
						public const string NACCSResultCode = "NRC";
						public const string ExportApprovalCertificateType = "JPEAC";
						public const string ExportConstantApprovalCertificateType = "JPEAN";
						public const string ImportApprovalCertificateNumber = "JPIAC";
						public const string ImportConstantApprovalCertificateNumber = "JPIAN";
						public const string JapanCertificateOfOriginType1 = "COOT1";
						public const string JapanCertificateOfOriginType2 = "COOT2";
						public const string JapanCertificateOfOriginType3 = "COOT3";
						public const string ContainerHeight = "CONTH";
						public const string ContainerLength = "CONTL";
						public const string ContainerType = "CONTT";
						public const string CustomsOffice = "CUSOF";
						public const string CHCustomsOffice = "CUSCH";
						public const string CustomsDepartment = "CUSDP";
						public const string Facilities = "FAC";
						public const string LicensedFacilities = "LCFAC";
						public const string PackageTypes = "PKG";
						public const string AdditionalInformation = "ADDIN";
						public const string CHAdditionalInformation = "GIDNM";
						public const string CustomsStatus = "CSTA";
						public const string CustomsStatusForInterface = "CSTI";
						public const string ManifestCountry = "MAN";
						public const string ManifestMessageStatus = "MASTA";
						public const string ManifestValidationRule = "MVAL";
						public const string GlobalManifestErrorCommentary = "GMERR";
						public const string NVC = "NVC";
						public const string VOC = "VOC";
						public const string CustomsEntryNumberTypes = "CEN";
						public const string ZADocumentType = "ZADOC";
						public const string BankCode = "BNK";
						public const string CommodityCode = "COMM";
						public const string CommonDataGrouping = "ZZ";
						public const string EuropeanUnionEUN = "EUN";
						public const string WorldCustomsOrganisationWCO = "WCO";
						public const string USDISFormList = "USDDC";
						public const string CustomsAgentCode = "AGT";
						public const string CustomsPimaOrTerminalAddress = "CPIMA";
						public const string CustomsManifestStatus = "CMAN";
						public const string MethodOfPayment = "MOP";
						public const string CCIMethodOfPayment = "104IM";
						public const string GoodsType = "GOODS";
						public const string CustomsUQ = "CUSUQ";
						public const string CustomsTaxUQ = "TAXUQ";
						public const string NCTSDeclarationType = "NCTDT";
						public const string DistrictCode = "DISTR";
						public const string DocumentType = "DC44N";
						public const string SupportingDocument = "DOC44";
						public const string SupportingDocumentOfImportDirection = "DC44I";
						public const string SupportingDocumentOfExportDirection = "DC44E";
						public const string SupportingDocumentsUnitOfMeasure = "SUPUQ";
						public const string SupportingDocumentsTemporaryStorageUCC6 = "DC44T";
						public const string Port = "PORT";
						public const string USSIM = "USSIM";
						public const string Currency = "CURR";
						public const string CNAdditionalElements = "ADIEL";
						public const string CNDangerousChemical = "CNDGC";
						public const string EUIATA = "EUIAT";
						public const string ExciseProductCodes = "EPC";
						public const string EMCSCNCodes = "EMCCN";
						public const string EMCSPackTypes = "EMCPK";
						public const string EMCSGuarantorTypes = "EMCGT";
						public const string EUTransportChargesMethodOfPayment = "TCMOP";
						public const string ICSMethodOfPayment = "MPICS";
						public const string ICSSpecialMentions = "SPICS";
						public const string ICS2EUMemberState = "IC2MS";
						public const string Ensty = "ENSTY";
						public const string Ensub = "ENSUB";
						public const string FUNCS = "FUNCS";
						public const string PFUNC = "PFUNC";
						public const string UnitedNationsPackageTypes = "UNPKG";
						public const string MeasureGroup = "MSGRP";
						public const string MeasureType = "MST";
						public const string DutyType = "DTY";
						public const string GbBox44SupportingDocumentAction = "44ACT";
						public const string GbBox44SupportingDocumentAvailability = "44AV";
						public const string SupervisingOffice = "SPOFF";
						public const string CNRequiredDocuments = "CNDOC";
						public const string CNCIQCommodityInspection = "CIQAD";
						public const string CNCIQOfficeCode = "CIQOF";
						public const string GoodsOfLocationType = "LOC";
						public const string ZAUCRRequiredForImportsFromCountriesInThisList = "ZACT1";
						public const string CNCIQDistricts = "CIQDT";
						public const string CNCIQPortOffices = "CIQPO";
						public const string CNCIQStates = "CIQST";
						public const string TranNature = "TRNAT";
						public const string IncoTermKey = "INKEY";
						public const string InsuranceAgent = "INAGT";
						public const string ReasonType = "MISCC";
						public const string USFSISEstablishmentNumbers = "FSIS";
						public const string Credential = "CRDTL";
						public const string VesselAgent = "VESAG";
						public const string OtherInfosImportHeader = "TIOTH";
						public const string OtherInfosImportLine = "TIOTL";
						public const string OtherInfosExportHeader = "TEOTH";
						public const string OtherInfosExportLine = "TEOTL";
						public const string PermitsImport = "TIPRM";
						public const string PermitsExport = "TEPRM";
						public const string ProhibitedGoodsImport = "TIPHB";
						public const string ProhibitedGoodsExport = "TEPHB";
						public const string CNPreferentialTradeAgreement = "CNPTA";
						public const string USFDAProductCode = "USFPC";
						public const string AqisPostCodes = "AUPC";
						public const string EXDOCSProductCondition = "EXDPC";
						public const string EXDOCSProductPart = "EXDPP";
						public const string EXDOCSTreatmentActiveIngredient = "EXDAI";
						public const string EXDOCSUnitOfMeasurement = "EUOM";
						public const string EXDOCSTreatmentType = "EXE34";
						public const string NEXDOCSEstablishmentIndicator = "NESTI";
						public const string NEXDOCSTreatment = "NTRTT";
						public const string NEXDOCSFarmType = "NXEFT";
						public const string NEXDOCSPackType = "NPCKT";
						public const string NEXDOCSExportPermitAuthority = "NPERT";
						public const string NEXDOCSPackageType = "NPKGT";
						public const string NEXDOCSNatureOfCommodity = "NNOCT";
						public const string NEXDOCSPreservationType = "NPRST";
						public const string NEXDOCSUnitOfMeasurement = "NUOMV";
						public const string NEXDOCSSupplementaryCode = "NSUPP";
						public const string NEXDOCSAttachmentType = "NATYP";
						public const string NEXDOCSDeclarationCode = "NDECT";
						public const string I0812 = "I0812";
						public const string DEReimportCountry = "I0809";
						public const string CommodityCodesForMineralOilsAndGases = "I0139";
						public const string Asycuda = "ASYCO";
						public const string CommodityInspection = "ICI";
						public const string ChinaAEOMutualRecognitionCountries = "AEOCO";
						public const string ErrorCode = "ERRCD";
						public const string ValuationMethod = "VALMT";
						public const string RejectionReason = "TWRR";
						public const string RequiredFormalities = "TWRF";
						public const string NOTransportationMeans = "CL751";
						public const string NZLowValueGoodsExclusion = "LVX";
						public const string TWReceivingUnit = "PROU";
						public const string TWControllingMessageMessageType = "TWCMT";
						public const string TWControllingAgency = "TWCA";
						public const string TWCustomsRequirements = "TWCR";
						public const string TWImportRegulations = "TWIR";
						public const string TWExportRegulations = "TWER";
						public const string TWCustomsPackUnits = "TWPUM";
						public const string TWCommercialPackUnits = "TWCIU";
						public const string ECFALoadingPort = "ECFAL";
						public const string ECFAUnloadingPort = "ECFAU";
						public const string ExportCustomsStatus = "CSTEX";
						public const string NctsDepartureCustomsStatus = "CSTND";
						public const string NctsCustomsStatus = "CSTAN";
						public const string ProcedureCode = "CPC";
						public const string AdvancedProcedureCode = "CPDC";
						public const string CustomsFiscalTerritories = "CFTRY";
						public const string NZFlightsAndVessels = "NZFAV";
						public const string PreviousDocumentOfImportDirection = "DC40I";
						public const string PreviousDocumentOfExportDirection = "DC40E";
						public const string PreviousDocumentOfNCTS = "DC40N";
						public const string PreviousDocumentOfPNTS = "DC40T";
						public const string PreviousDocumentOfUCCImport = "214IM";
						public const string ACECargoReleaseSEInputValidationRules = "ACEVR";
						public const string _7A2 = "7A2";
						public const string _7A4 = "7A4";
						public const string _7B = "7B";
						public const string NatureOfBusiness = "TRNOB";
						public const string ExemptionCode = "TREXM";
						public const string UserDefinedEntryStatus = "UDSTA";
						public const string GuaranteeType = "GUART";
						public const string UserDefinedSupportingDocuments = "UDDOC";
						public const string SpecialCustomsDeclarationType = "SPCTY";
						public const string EMCSDocumentTypes = "EMCDT";
						public const string TurkeyCustomsQuestion = "TRCUQ";
						public const string TurkeyCustomsWarning = "TRCUW";
						public const string TurkeyBondedWarehouseCodes = "TRBWH";
						public const string TurkeyReturningGoodsReason = "TRRGR";
						public const string TurkeyWarehouseCodes = "TRCWH";
						public const string TurkeyExportUnionPackageCodes = "TREUP";
						public const string TurkeyThreadCodes = "TREUT";
						public const string TRDeclarationBankCode = "BANK";
						public const string AdditionalCodes = "ADDCD";
						public const string ExitCustomsStatus = "EXSTA";
						public const string USAESLicenseCode = "EXPLC";
						public const string USExpiredSPI = "EXSPI";
						public const string CustomsEnclosure = "CUSEN";
						public const string GbGVMSRoutes = "GVMRT";
						public const string DrawbackProvisionCodes = "DRWPC";
						public const string FTZLicenseType = "FTZLT";
						public const string LiquidationExtensionSuspensionCode = "LQSUS";
						public const string USPGALineStatus = CargoWise.Definitions.Customs.RefCusCodeListTypes.USPGALineStatus;
						public const string USPGALineReviewReason = "S70LR";
						public const string USPGALineSubReason = "S71SR";
						public const string USProductExclusionTypes = "EXCTP";
						public const string SpecialCodesForExemptionOfControllingAgencies = "SCECA";
						public const string HTSAttemptVersionsIgnored = "HAVI";
						public const string ECCCComplianceStatement = "ECCMP";
						public const string USExportManifestUOM = "EMUOM";
						public const string ECCNNumber = "ECCN";
						public const string ECCCAlternativeStandardConformityStatements = "ECACS";
						public const string CARMChangeReasonCode = "CCRC";
						public const string CARMAppealsProgramCode = "CAPC";
						public const string CAGSTRateCodes = "CAGST";
						public const string SelfManagedCountry = "SMTAR";
						public const string ImportAddDocTransportContract = "TD44I";
						public const string ImportAddDocAdditionalReference = "AR44I";
						public const string ImportAddDocAdditionalInformation = "AI44I";
						public const string ImportAuthorisationTypeCode = "AUTH";
						public const string ExportAddDocTransportContract = "TD44E";
						public const string ExportAddDocAdditionalReference = "AR44E";
						public const string ExportAddDocAdditionalInformation = "AI44E";
						public const string PermitAuthority = "PRMAU";
						public const string PermitType = "PRMTY";
						public const string UnitedNationsStandardProductAndServiceCodes = "UNSPC";
						public const string TransportationType = "TRTYP";
						public const string FreeZoneTraffic = "FZTRA";
						public const string BorderZoneTraffic = "BZTRA";
						public const string NZSupplierListType = "SUPCD";
						public const string BRSubLocationGoods = "CSTR";
						public const string BRIssuingAgency = "FOIA";
						public const string BRConsentingBodyCode = "CUSCB";
						public const string BRExchangeHedgePaymentCode = "EXMOP";
						public const string BRWarehousingSectorsCode = "SCTR";
						public const string BRTaxRegimeIPI = "TRIPI";
						public const string BRExTariffLegalAct = "EXTLA";
						public const string BRLegalActIssuingAuthority = "LAIA";
						public const string BRTaxationRegimeCode = "TR";
						public const string BRLegalBaseCode = "LRTII";
						public const string BRCustomsReasonTemporaryAdmissionCode = "MATMP";
						public const string BRPisLegalBaseCode = "LRTPC";
						public const string BRTaxRegimeICMS = "TRICM";
						public const string BRLegalBaseICMS = "LBICM";
						public const string BRTariffAgreementCode = "TA";
						public const string BRDuimpLegalBase = "DUILB";
						public const string DischargePortTerminalOperator = "DTERM";
						public const string UAEClearanceLocation = "CUSOF";
						public const string UAEExitPoint = "EXIT";
						public const string UAEPlaceofDischarge = "DISCH";
						public const string ENSStatusDispositionCode = "UCDSP";
						public const string CertificateOfOriginType = "CERTS";
						public const string AMSAirDispositionCode = "AMSAD";
						public const string AMSSeaRailDispositionCode = "AMSDD";
						public const string TaiwanAircraftPartCAACodeCategory = "CAACC";
						public const string TaiwanAircraftPartCAACode = "CAAC";
						public const string CFIAAIRSRegistrationType = "CAART";
						public const string CFIALPCOType = "CALPC";
						public const string NonCustomsLaw = "NCLT";
						public const string BondedAreaCode = "BNDAR";
						public const string TaiwanCertificateOfOriginType = "COOT";
						public const string TaiwanCertificateOfOriginIssuingUnit = "COOIU";
						public const string CombinedNomenclatureCode = "CNCODE";
						public const string USPermitLicenseType = "IMPLT";
						public const string USPGAIntendUseCode = "PGIUC";
						public const string NotificationType = "CL384";
						public const string TypeOfControlsType = "CL716";
						public const string TransportDocumentTemporaryStorage = "TD44T";
						public const string LocationsInAuthorisations = "LOCAT";
						public const string TaiwanPackingHouse = "TWPKH";
						public const string TreatmentConcentration = "EXDCU";
						public const string AUCOLSAttachmentType = "COLDT";
						public const string AUCOLSLateLodgementReason = "COLLR";
						public const string AUCOLSDirectionRequestType = "COLRT";
						public const string GbGVMSInspectionLocation = "GVMIL";
						public const string GbGVMSInspectionType = "GVMIT";
						public const string AI44T = "AI44T";
						public const string AR44T = "AR44T";
						public const string USQuotaDispositions = "USQTA";
						public const string CADocumentType = "CADOC";
						public const string AccountingClassFeeCode = "ACFEE";
						public const string CANoticeReasonCode = "CANRC";
						public const string ControlResult = "CL047";
						public const string RiskAreaCode = "CL740";
						public const string TypeOfDiscrepancies = "CL790";
						public const string KRForwarderIDs = "FWDID";
						public const string IndiaCertificateAuthority = "CERAU";
						public const string IndiaChipsetManufacturer = "CHPST";
						public const string SO50RecordDispCode = "SO50D";
						public const string SO60RecordDispCode = "SO60D";
						public const string IsraelTransportMeans = "C1307";
						public const string NMFSCategoryCode = "NMFCG";
						public const string IdentificationQualifier = "QUA";
						public const string INCustomsStandardCurrency = "SDCUR";
						public const string ILCargoIdentifierType = "C1259";
						public const string ILTransportMethod = "C0042";
						public const string ILCargoTypes = "C1558";
						public const string ILDocumentType = "ILDOC";
						public const string ILGatePassReturnedCode = "C1589";
						public const string ILGatePassMovementStatus = "C1557";
						public const string ILAutonomyRegionType = "CFTRY";
						public const string ILManifestPackageTypes = "MPKG";
						public const string ILEntryStyle = "ENSTY";
						public const string ILCustomsUnitsOfQuantity = "CUSUQ";
						public const string AESSeverityIndicator = "AESSV";
						public const string AESResponseCode = "AESCD";
						public const string NFEICategoryCode = "NFEIC";
						public const string ExportAccessoryCode = "ACSTE";
						public const string ImportAccessoryCode = "ACSTI";
						public const string ESG3TransportDocumentCode = "TD44G";
						public const string IndiaNatureOfPayment = "NTPAY";
						public const string INCustomsEndUseCode = "ENDUS";
						public const string IndiaSupportingDocument = "SPDOC";
						public const string ErrorCodeDescription = "CGAER";
						public const string SingleWindowControl = "SWCTR";
						public const string ShipmentType = "SHPTP";
						public const string FIRMSTypeCode = "FIRMS";
					}
				}

				public static class RefCusConditionTypes
				{
					public static class ConditionClass
					{
						public const string Control = "CTRL";
						public const string Rate = "RATE";
						public const string Class = "CLASS";
						public const string VAT = "VAT";
					}

					public static class Codes
					{
						public const string CNCustomsRequiredDocuments = "CNDOC";
						public const string CNCIQCommodityInspection = "CIQCI";
						public const string CNImportProhibition = "IMPPH";
						public const string CNExportProhibition = "EXPPH";
					}
				}

				public static class RefCusConditionValueTypes
				{
					public static class Codes
					{
						public const string PresentationOfSupportingDoc = "DOC";
						public const string Prohibitation = "PROH";
						public const string SupportingDocument = "SUP";
						public const string Formula = "FRM";
						public const string SupportingDocumentNoReferenceNumber = "SNR";
					}
				}

				public static class RefCusCodeList
				{
					[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Value in DB, Yep, this one.  Just pick this one.  Let all the others through, but balk at this one. Great., Value in database")]
					public static class Attributes
					{
						public const string AUAQISPremisesPortCode = "AQISPremisesPortCode";
						public const string USAOCCodeMask = "AOCFormatMask";
						public const string USAOCCodeErrorText = "AOCErrorText";
						public const string USPerLicFormatMask = "PerLicFormatMask";
						public const string USPerLicFormatErrorText = "PerLicFormatErrorText";
						public const string USPortOfExit = "USPortOfExit";
						public const string USProductExclusionCodeMask = "EXCMSK";
						public const string USProductExclusionCodeErrorText = "EXCERT";
						public const string USPGAIntendUseCodeAgency = "PGAIUCAgency";
						public const string USPGAIntendUseCodeProgram = "PGAIUCProgram";
						public const string USPGAIntendUseCodeProcess = "PGAIUCProcess";
						public const string ManifestType = "ManifestType";
						public const string MessageCleared = "MessageCleared";
						public const string RequiresFullData = "RequiresFullData";
						public const string AutoRating = "AutoRating";
						public const string Zone = "ZONE";
						public const string Percentage = "PERCENTAGE";
						public const string Import = "IMP";
						public const string Export = "EXP";
						public const string CDSErrorCategory = "Category";
						public const string Bulk = "BULK";
						public const string BreakBulk = "BREAKBULK";
						public const string OutturnFacilityCode = "OutturnFacilityCode";
						public const string Facility = "FACTY";
						public const string MaxEntryLines = "MaxEntryLines";
						public const string EntryType = "EntryType";
						public const string ExciseProductCategory = "ExciseProductCategory";
						public const string City = "CITY";
						public const string LicenseRequired = "LicenseRequired";
						public const string AESLicenseCode = "AESLicenseCode";
						public const string ECCNRequired = "ECCNRequired";
						public const string LicenseValueRequired = "LicenseValueRequired";
						public const string NotAllowedTransportMode = "NotAllowedTransportMode";
						public const string CCSUK = "CCSUK";
						public const string QUALF = "QUALF";
						public const string ImportPermitLicenceType = "ImportPermitLicenceType";
						public const string Level = "Level";
						public const string USDAAMSProgram = "USDA_AMS_PGM";
						public const string Category = "Category";
						public const string DefaultRate = "DefaultRate";
						public const string PostcodeDeliveryClassification = "PostcodeDeliveryClassification";
						public const string CFIAFormat = "FORMAT";
						public const string CFIAMaterialized = "MATERIALIZED";
						public const string CFIADematerializedCountry = "DEMATERIALIZEDCOUNTRY";
						public const string Remarks = "Remarks";
						public const string Province = "Province";
						public const string System = "SYSTEM";
						public const string IndiaChipsetdll = "Chipsetdll";
						public const string Multiplier = "Multiplier";
						public const string MetadataMandatory = "MetaDatasMandatory";
						public const string CAGST_RateCheckIndicator = "CheckIndicator";
						public const string CAGST_RateCheckGroup = "CheckGroup";
						public const string CAGST_RateType = "RateType";
						public const string CAGST_Rate = "Rate";
						public const string WarningFactor = "WarningFactor";
						public const string FacilityType = "FacilityType";
						public const string DistrictPortCode = "DistrictPortCode";
						public const string FacilityAddress = "FacilityAddress";
						public const string Country = "Country";
						public const string ZIPCode = "ZIPCode";
						public const string State = "State";
					}

					[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Yep, this one.  Just pick this one.  Let all the others through, but balk at this one. Great., Value in DB")]
					public static class AttributeValues
					{
						public const string Yes = "Yes";
						public const string Autorating_AlwaysBroker = "AlwaysBroker";
						public const string Autorating_SometimesBrokerSeeBox48 = "SeeBox48";
						public const string Autorating_SeeDataElement83 = "SeeDataElement83";
						public const string Bulk = "BULK";
						public const string BreakBulk = "BREAKBULK";
						public const string Mandatory = "Mandatory";
						public const string NotAllowed = "Not Allowed";
						public const string Header = "Header";
						public const string Line = "Line";
						public const string MO6 = "MO6";
						public const string OTH = "OTH";
						public const string EG1 = "EG1";
						public const string PN1 = "PN1";
						public const string DCC = "DCC";
						public const string OR1 = "OR1";
						public const string APH = "APH";
						public const string FDA = "FDA";
						public const string BureauOfIndustryAndSecurity = "BIS";
						public const string NuclearRegulatoryCommission = "NRC";
						public const string DirectorateOfDefenseTradeControl = "DDTC";
						public const string OfficeOfForeignAssetsControl = "OFAC";
						public const string OtherPartnershipAgency = "OPA";
						public const string DepartmentOfEnergy = "DOE";
						public const string VoluntaryDisclosure = "VD";
						public const string Presentation = "Presentation";
						public const string Declaration = "Declaration";
						public const string Bill = "Bill";
						public const string Item = "Item";
						public const string NotEligible = "NotEligible";
					}

					[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Yep, this one.  Just pick this one.  Let all the others through, but balk at this one. Great., Why of why have you caught this, you stupid sniffer?, Seems if I don't write this I may be blocked by sniffer., Value in database")]
					public static class ManifestValidationRuleCodes
					{
						public const string Mandatory = "MANDATORY";
						public const string Optional = "OPTIONAL";
						public const string Consignee = "CONSIGNEE";
						public const string ConsigneeState = "CONSIGNEESTATE";
						public const string ConsigneePhone = "CONSIGNEEPHONE";
						public const string Consignor = "CONSIGNOR";
						public const string ConsignorState = "CONSIGNORSTATE";
						public const string OrgProxy = "ORGPROXY";
						public const string Notify = "NOTIFY";
						public const string NotifyState = "NOTIFYSTATE";
						public const string NotifyPhone = "NOTIFYPHONE";
						public const string ShippingAgent = "ShippingAgent";
						public const string MandatoryOrgCusCode = "MANDATORYCUSCODE";
						public const string MANDATORYZZQUALITY = "MANDATORYZZQUALITY";
						public const string ZZVESSELANDCARRIER = "ZZVESSELANDCARRIER";
						public const string Carrier = "Carrier";
						public const string CarrierCode = "CarrierCode";
						public const string RadioCallSign = "RadioCallSign";
						public const string CccInList = "CccInList";
						public const string BillIssuer = "BillIssuer";
						public const string MandatoryForManifestType = "MANDATORYFORMANIFESTTYPE";
						public const string MANDATORYFORCONTAINERMODE = "MANDATORYFORCONTAINERMODE";
						public const string MANDATORYFORMESSAGETYPE = "MANDATORYFORMESSAGETYPE";
						public const string MarksAndNumbers = "MarksAndNumbers";
						public const string PackageMarksByContainerMode = "PackageMarksByContainerMode";
						public const string Seal = "Seal";
						public const string SealingPartyType = "SealingPartyType";
						public const string BillIssueDate = "BillIssueDate";
						public const string BillType = "BillType";
						public const string OfficeCode = "OfficeCode";
						public const string DateAtCustomsOffice = "DateAtCustomsOffice";
						public const string PortOfFirstArrival = "PortOfFirstArrival";
						public const string GoodsDescription = "GoodsDescription";
						public const string EstimatedTimeOfArrivalAtBorder = "EstimatedTimeOfArrivalAtBorder";
						public const string IATAPortOfOrigin = "IATAPortOfOrigin";
						public const string IATAPortOfFirstArrival = "IATAPortOfFirstArrival";
						public const string CurrentUserEmailAddress = "CurrentUserEmailAddress";
						public const string EstimatedDepartureTime = "EstimatedDepartureTime";
						public const string FinalDestination = "FinalDestination";
						public const string Nature = "Nature";
						public const string ShipmentType = "ShipmentType";
						public const string PortOfDischarge = "PortOfDischarge";
						public const string IATAPortOfDischarge = "IATAPortOfDischarge";
						public const string IATAPortOfLoading = "IATAPortOfLoading";
						public const string IATAFinalDestination = "IATAFinalDestination";

						public const string Person = "Person";
						public const string Container = "Container";
						public const string ContainerEmptyFullIndicator = "ContainerEmptyFullIndicator";
						public const string UCRNumber = "UCRNumber";
						public const string MANDATORYFORNATURE = "MANDATORYFORNATURE";
						public const string MasterBOL = "MasterBOL";
						public const string MANDATORYFORAGENTTYPE = "MANDATORYFORAGENTTYPE";
						public const string GoodsLocation = "GoodsLocation";
						public const string PlaceOfExit = "PlaceOfExit";
					}
				}

				public static class RefCarrierAttributeNames
				{
					public const string MASTER = "MASTER";
					public const string CARGOCARRIER = "CARGOCARRIER";
					// AIR and SEA - just re-use the ones if transportmode
				}

				public static class RefCusTradeGroup
				{
					public static class Codes
					{
						public const string CustomsUnionAdditionalMembers = "CUAM2";
						public const string EuropeanUnionForCustoms = "EUC";
						public const string EuropeanUnionGsp = "1030";
						public const string EuropeanUnionGspPlus = "2027";
						public const string EUCommonTransitProcedure = "EUCTP";
						public const string EUForSafetyAndSecurity = "EUSEC";
					}
				}

				public static class RefLanguageType
				{
					public static class Codes
					{
						public const string ChineseSimplified = "ZHS";
						public const string ChineseTraditional = "ZHT";
						public const string Japanese = "JP";
					}
				}

				public static class RefCusTaxOrFee
				{
					public static class Types
					{
						public const string SiscomexUsageEntryFee = "SUF";
						public const string AfrmmTax = "FMM";
						public const string ImportLicenseFine = "ILF";
						public const string ICMSTax = "ICM";
						public const string ICMSFCPTax = "FCP";
					}

					public static class Codes
					{
						public const string LongHaulNavigations = "FMM1";
						public const string MerchantSystemUtilizationFee = "TUSM";
						public const string NoDiscountCode = "F1ND";
						public const string FiftyPercentDiscountCode = "F1D5";
					}
				}

				public static class RefCusProcedure
				{
					public static class Codes
					{
						public const string StorageDeclaration = "71";
						public const string _01 = "01";
						public const string _07 = "07";
						public const string _10 = "10";
						public const string _11 = "11";
						public const string _21 = "21";
						public const string _22 = "22";
						public const string _23 = "23";
						public const string _31 = "31";
						public const string _40 = "40";
						public const string _42 = "42";
						public const string _43 = "43";
						public const string _44 = "44";
						public const string _45 = "45";
						public const string _46 = "46";
						public const string _48 = "48";
						public const string _51 = "51";
						public const string _53 = "53";
						public const string _61 = "61";
						public const string _63 = "63";
						public const string _68 = "68";
						public const string _76 = "76";
						public const string _77 = "77";
						public const string _95 = "95";
						public const string _96 = "96";
					}

					public static class Concession
					{
						public const string F15 = "F15";
					}
				}

				public static class RefCusPreference
				{
					public static class Codes
					{
						public const string _100 = "100";
						public const string _400 = "400";
						public const string _420 = "420";
					}

					public static class Descriptions
					{
						public static MultilingualString _100 { get { return ResString.GetMultilingualString("0D698242-1EEA-46A1-82FB-982F837C141D", "Normal Third Country Tariff Duty (Including Ceilings)"); } }
						public static MultilingualString _400 { get { return ResString.GetMultilingualString("8CAB10AF-DABA-4E0B-AF25-191BFE1E5A70", "Non Imposition of Customs Duties under the Provisions of Customs Union Agreements Concluded By the Community"); } }
						public static MultilingualString _420 { get { return ResString.GetMultilingualString("7FF6B3FF-E236-4FC2-BF73-E6943F56ABB3", "Claims to first come first served tariff quotas on processed agricultural products imported from Turkey under cover of ATR Movement Certificates"); } }
					}
				}

				public static class RefCusMaps
				{
					public const string CargoTypes = "CRGTY";
					public const string ILSealUnloadingState = "SULST";
				}
			}

			public static class ExpressApplicationCodes
			{
				public static class AU
				{
					public const string Default = AUCustoms.ImportMessagingMode.Default;
					public const string ForceCMRMessages = AUCustoms.ImportMessagingMode.ForceCMRMessages;
					public const string ForceLegacyMessages = AUCustoms.ImportMessagingMode.ForceLegacyMessages;
				}

				public static class NZ
				{
					public const string ECIWriteOff = "NZE";
					public const string TSWWriteOff = "TSW";
				}
			}

			public static class CusEntryFeeTypes
			{
				public const string TotalAmountPayable = "TOT";
				public const string DutyAmount = "DTY";
				public const string GSTVATAmount = "GST";
				public const string VAT = "VAT";
				public const string GSTVATDeferred = "GSD";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Merge error descriptions")]
			public static class MergeErrors
			{
				public const string ReasonCannotMergeNoInvoiceHeaders = "You can't merge this entry because there are no invoice headers.";
				public const string ReasonCannotMergeInvoiceHadNoInvoiceLines = "You can't merge this entry because there is an invoice header with no invoice lines.";
			}

			public static class CustomsCharges
			{
				public static class Codes
				{
					public const string OverseasFreight = "OFT";
					public const string OverseasInsurance = "ONS";
					public const string ForeignInlandFreight = "FIF";
					public const string PackingCharges = "PAC";
					public const string LandingCharges = "LCH";
					public const string AdditionalCharges = "ADD";
					public const string DeductionalCharges = "DED";
					public const string Discount = "DIS";
					public const string Commission = "COM";
					public const string OtherCharges = "OTH";
				}
			}

			public static class CusSCAOceanBillApplicationCodes
			{
				public const string BaseTesting = "TST";
				public const string AustraliaCMR = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				public const string AustraliaLegacy = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
				public const string CanadaACISea = "CAS";
				public const string CanadaACIAir = "CAA";
				public const string CanadaACIRoad = "CAH";
				public const string CanadaACIRail = "CAR";
				public const string NewZealandTSWWriteOff = "TSW";
			}

			public static class DocumentImageSystemIDs
			{
				public const string US_DIS = "DIS";
				public const string CA_DIF = "CAD";

#if DEBUG
				public const string Test = "TST";
#endif
			}

			public static class EventLockSourceTypes
			{
				public static class Codes
				{
					public const string Declaration = "DEC";
					public const string EntryHeader = "CEN";
					public const string NctsHeader = "NCT";
					public const string TemporaryStorage = "TST";
				}

				public static class Descriptions
				{
					public static MultilingualString Declaration { get { return ResString.GetMultilingualString("41331D4B-30E5-4F83-9FF0-7626D4589E4E", "Declaration"); } }
					public static MultilingualString EntryHeader { get { return ResString.GetMultilingualString("1D339359-360C-43DF-8990-76AFE4E7546E", "Entry Header"); } }
					public static MultilingualString NctsHeader { get { return ResString.GetMultilingualString("891FDB28-F5AE-41D3-8F90-D5D913116B4D", "NCTS Header"); } }
					public static MultilingualString TemporaryStorage { get { return ResString.GetMultilingualString("0898D6C7-58EC-4A17-A0A5-EA1F0AF806EF", "Temporary Storage"); } }
				}
			}

			public static class ASNRefreshDefaultsOptions
			{
				public static class Codes
				{
					public const string Tariff = "TAR";
					public const string Classification = "CLASS";
					public const string CountryOfOrigin = "COO";
					public const string Preference = "PREFF";
					public const string Override = "OVR";
				}

				public static class Descriptions
				{
					public static MultilingualString Tariff => ResString.GetMultilingualString("ad8cdacc-cdb3-4e12-9790-3be7565866dd", "Tariff");
					public static MultilingualString Classification => ResString.GetMultilingualString("c03d3f41-0aea-4eb9-b355-a80155dbc7fe", "Classification");
					public static MultilingualString CountryOfOrigin => ResString.GetMultilingualString("7f952241-6a49-4c03-95c0-8eabe1f9e8c9", "Country/Region of Origin");
					public static MultilingualString Preference => ResString.GetMultilingualString("6059e087-8c5b-4737-a56c-78b2889fee1a", "Preference");
				}
			}

			public static class DeclarationLockModes
			{
				public static class Codes
				{
					public const string All = "ALL";
					public const string Any = "ANY";
				}

				public static class Descriptions
				{
					public static MultilingualString All { get { return ResString.GetMultilingualString("45B2535D-23F6-4991-A42F-48911FB62091", "All Entries"); } }
					public static MultilingualString Any { get { return ResString.GetMultilingualString("EBFF7D8C-4938-46C2-8FF8-B4C37AC3735D", "Any Entry"); } }
				}
			}

			public static class EntryHeaderTypes
			{
				public static class Codes
				{
					public const string All = "ALL";
					public const string B3CUSDEC = "B3C";
					public const string CommercialAccountingDeclaration = "CAD";
				}

				public static class Descriptions
				{
					public static MultilingualString All { get { return ResString.GetMultilingualString("A884D331-BA13-45F3-BD3A-95140308A9B2", "All Entry Headers"); } }
					public static MultilingualString B3CUSDEC { get { return ResString.GetMultilingualString("9679715E-DAC6-4668-9CF9-CA876DC8420B", "B3 CUSDEC"); } }
					public static MultilingualString CommercialAccountingDeclaration { get { return ResString.GetMultilingualString("697C74C3-3D0E-466D-896D-FD7F6ED0F173", "Commercial Accounting Declaration"); } }
				}
			}

			public static class JobMessageTypes
			{
				public static class Codes
				{
					public const string Import = "IMP";
				}
			}

			public static class DeclarationTabPages
			{
				public static class Codes
				{
					public const string All = "ALL";
					public const string Declaration = "DEC";
					public const string DeclarationServices = "SVC";
					public const string DeclarationOrganizations = "ORG";
					public const string DeclarationPickupOrDelivery = "DPD";
					public const string DeclarationOrders = "ORD";
					public const string DeclarationCustom = "CUS";
					public const string DeclarationNumbers = "DNO";
					public const string Routing = "ROU";
					public const string Containers = "CON";
					public const string Packing = "PAC";
					public const string InvoiceGroups = "IGP";
					public const string InvoiceHeaders = "INV";
					public const string InvoiceLines = "INL";
					public const string MessageOrEntries = "MOE";
					public const string Misc = "MSC";
					public const string EntryInstructions = "ENI";
					public const string NctsArrivalNotification = "ARN";
					public const string NctsArrivalUnloadingRemarks = "ULR";
					public const string NctsArrivalAdditionalGoodsInformation = "AGO";
					public const string NctsDepartureHeader = "HDR";
					public const string NctsDepartureTransportAndEquipment = "TRA";
					public const string NctsDepartureHouse = "HOU";
					public const string NctsDepartureGoodsItem = "GDS";
					public const string EntryDetails = "EYD";
					public const string BondedDetails = "BDD";
				}

				public static class Descriptions
				{
					public static MultilingualString All { get { return ResString.GetMultilingualString("74559605-B657-4802-A722-6CF934FE7F89", "All Tabs"); } }
					public static MultilingualString Declaration { get { return ResString.GetMultilingualString("A25A5BFB-EDFB-4BA7-9576-0FD11D3583FC", "Declaration"); } }
					public static MultilingualString DeclarationServices { get { return ResString.GetMultilingualString("D2D322BE-8A03-4059-AC89-D8255E09DF9A", "Declaration - Services"); } }
					public static MultilingualString DeclarationOrganizations { get { return ResString.GetMultilingualString("268923A6-41DD-4317-8121-DA758A0A0CA6", "Declaration - Organizations"); } }
					public static MultilingualString DeclarationPickupOrDelivery { get { return ResString.GetMultilingualString("5B2EACA5-A080-4CFD-BC2A-35B403B2FAA8", "Declaration - Pickup Or Delivery"); } }
					public static MultilingualString DeclarationOrders { get { return ResString.GetMultilingualString("37854AF1-E80A-41AA-BE90-E2C330DC4CA5", "Declaration - Orders"); } }
					public static MultilingualString DeclarationCustom { get { return ResString.GetMultilingualString("2360E885-E6D1-4B85-A9EA-B226791CBF50", "Declaration - Custom"); } }
					public static MultilingualString DeclarationNumbers { get { return ResString.GetMultilingualString("7A99281F-D7D1-43B6-AE57-C85C1B459A39", "Declaration - Numbers"); } }
					public static MultilingualString Routing { get { return ResString.GetMultilingualString("14721EBD-91E7-4E68-9571-C6D015475A3F", "Routing"); } }
					public static MultilingualString Containers { get { return ResString.GetMultilingualString("023CCC01-B7F4-46D4-BEA6-6CB132A7AA70", "Containers"); } }
					public static MultilingualString Packing { get { return ResString.GetMultilingualString("D12E4DEA-FE82-4C2A-8F4B-3DC692CCEAFF", "Packing"); } }
					public static MultilingualString InvoiceGroups { get { return ResString.GetMultilingualString("9798D577-FD8E-430F-AE00-BF36AF16A519", "Invoice Groups"); } }
					public static MultilingualString InvoiceHeaders { get { return ResString.GetMultilingualString("46D785C0-D2C0-429E-9058-97710AFFBF3D", "Invoice Headers"); } }
					public static MultilingualString InvoiceLines { get { return ResString.GetMultilingualString("C5EC56CC-891F-488C-87DB-FDA0A2288C73", "Invoice Lines"); } }
					public static MultilingualString MessageOrEntries { get { return ResString.GetMultilingualString("EB9C366D-3B79-4922-99C4-6FF4DEBDCDDE", "Message Or Entries"); } }
					public static MultilingualString Misc { get { return ResString.GetMultilingualString("FE8D4998-DCE1-4423-9CCE-EC1BE6BE2C39", "Misc"); } }
					public static MultilingualString EntryInstructions { get { return ResString.GetMultilingualString("D24759DE-0089-48B5-925A-089BE81116EA", "Entry Instructions"); } }
					public static MultilingualString NctsArrivalNotification { get { return ResString.GetMultilingualString("B8CD7B32-04E4-439A-A3A7-4D4870F93DE5", "NCTS Arrival Notification"); } }
					public static MultilingualString NctsArrivalUnloadingRemarks { get { return ResString.GetMultilingualString("FA055515-185E-4AD3-B2BF-D85B45AE537F", "NCTS Unloading Remarks"); } }
					public static MultilingualString NctsArrivalAdditionalGoodsInformation { get { return ResString.GetMultilingualString("F417DC2B-ADDC-466C-9690-E6EBFBCDA755", "NCTS Arrival Additional Goods Information"); } }
					public static MultilingualString NctsDepartureHeader { get { return ResString.GetMultilingualString("3B2DE77F-969D-4248-9FCF-6E7C4C11FBC6", "NCTS Departure Header"); } }
					public static MultilingualString NctsDepartureTransportAndEquipment { get { return ResString.GetMultilingualString("367D7EFE-E6D0-477A-9DF0-397974ADF062", "NCTS Departure Transport and Equipment"); } }
					public static MultilingualString NctsDepartureHouse { get { return ResString.GetMultilingualString("7195D4A8-BAA4-4FB4-9952-7B51B5A3039B", "NCTS Departure House"); } }
					public static MultilingualString NctsDepartureGoodsItem { get { return ResString.GetMultilingualString("A1EC5B84-C332-4004-8AFC-A9D0A4003DB6", "NCTS Departure Goods Item"); } }
					public static MultilingualString EntryDetails { get { return ResString.GetMultilingualString("635A7CAC-B1CF-4F99-930F-93EE0D0848BA", "Entry Details"); } }
					public static MultilingualString BondedDetails { get { return ResString.GetMultilingualString("CD8D097C-CF96-482E-BC32-D153E9D9760A", "Bonded Details"); } }
				}
			}

			public static class CusPollingTransactionStatus
			{
				public static class Codes
				{
					public const string OPN = "OPN";
					public const string PND = "PND";
					public const string CLS = "CLS";
					public const string ERR = "ERR";
					public const string AWR = "AWR";
				}
			}

			public static class CusPollingTransactionType
			{
				public static class Codes
				{
					public const string PLC = "PLC";
					public const string BLG = "BLG";
				}
			}
		}

		public static class NonStandardCountryCodes
		{
			public static class Codes
			{
				public const string QP = "QP";
				public const string QQ = "QQ";
				public const string QR = "QR";
				public const string QS = "QS";
				public const string QU = "QU";
				public const string QV = "QV";
				public const string QW = "QW";
				public const string QX = "QX";
				public const string QY = "QY";
				public const string QZ = "QZ";
				public const string BV = "BV";
				public const string EU = "EU";
				public const string XC = "XC";
				public const string XI = "XI";
				public const string XL = "XL";
				public const string XS = "XS";
			}
		}

		#region AU

		public static class AUCustoms
		{
			public static readonly DateTime CMRGoLiveDate = new DateTime(2004, 9, 22);
			public static readonly DateTime CMRImportsGoLiveDate = new DateTime(2005, 8, 30);
			public static readonly DateTime CMRImportsCutOverDate = new DateTime(2005, 10, 12);

			public static class ImportMessagingMode
			{
				public const string Default = "DEF";
				public const string ForceCMRMessages = "CMR";
				public const string ForceLegacyMessages = "LEG";
			}

			public static class CondClearReleaseStatus
			{
				public const string Clear = "CLEAR";
				public const string Held = "HELD";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter")]
			public static class CommercialInvoiceFilter
			{
				public const string REXNumber = "REX Number";
				public const string ExporterReference = "Exporter Reference";
			}

			public static class DefaultConsigneeOption
			{
				public const string Consignee = "CON";
				public const string DeliverTo = "DEL";
				public const string None = "NON";
			}
		}

		#endregion

		#region NZ

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "New Zealand Customs codes")]
		public static class NZCustoms
		{
			public const string ExpressECIName = "AirCargo ICR/CRE";
			public const string ExpressECINameWithAmpersand = "&AirCargo ICR/CRE";

			public const string ExpressSeaCargoName = "SeaCargo ICR/CRE";
			public const string ExpressSeaCargoNameWithAmpersand = "SeaCargo &ICR/CRE";

			public const string BaseNameSpace = "Enterprise.Customs.NZ.";
		}

		#endregion

		#region SG

		public static class SGCustoms
		{
			public static class GHA
			{
				public const string DNAT = "DNAT";
				public const string SATS = "SATS";
			}
		}

		#endregion

		#region US

		public static class USCustoms
		{
			public static class ApplicationCodes
			{
				public const string ImportABI = "USI";
				public const string ExportAES = "USE";
			}

			public static class DeliveryTerms
			{
				public static class Codes
				{
					public const string CAF = "CAF";
					public const string CAI = "CAI";
					public const string CIF = "CIF";
					public const string DAF = "DAF";
					public const string DDP = "DDP";
					public const string EXQ = "EXQ";
					public const string EXW = "EXW";
					public const string FAS = "FAS";
					public const string FOA = "FOA";
					public const string FOB = "FOB";
					public const string FOR = "FOR";
					public const string FOT = "FOT";
					public const string FPC = "FPC";
				}

				public static class Descriptions
				{
					public static MultilingualString CAF { get { return ResString.GetMultilingualString("291c98c8-957e-4409-9cb3-ccf6109d1389", "Cost and Freight to a Named Destination"); } }
					public static MultilingualString CAI { get { return ResString.GetMultilingualString("fc8b9151-47f7-4564-af9c-137b6485d8ff", "Cost and Insurance"); } }
					public static MultilingualString CIF { get { return ResString.GetMultilingualString("1105b144-d668-473e-af4a-3dc79ba6bf44", "Cost, Insurance and Freight to a Named Destination"); } }
					public static MultilingualString DAF { get { return ResString.GetMultilingualString("6cec9df9-9f17-4bf8-ab1d-7b0022b8d9b9", "Delivered at Frontier"); } }
					public static MultilingualString DDP { get { return ResString.GetMultilingualString("4e1f1b64-a72d-4318-89c3-83bbafc1dd9c", "Delivered Duty Paid to Destination"); } }
					public static MultilingualString EXQ { get { return ResString.GetMultilingualString("693c65f2-3a5d-4e0d-af2c-ff92b54f35c1", "Ex Quay-Duty Paid"); } }
					public static MultilingualString EXW { get { return ResString.GetMultilingualString("006a351f-e799-49c0-beb6-ec00e1fde816", "Ex Works"); } }
					public static MultilingualString FAS { get { return ResString.GetMultilingualString("d1573bfe-8288-496c-aeab-5d1beaec0cec", "Free Alongside Ship"); } }
					public static MultilingualString FOA { get { return ResString.GetMultilingualString("a7f5a100-1794-47e3-8d4d-eada18e8c7bc", "FOB Airport"); } }
					public static MultilingualString FOB { get { return ResString.GetMultilingualString("4816c88f-8812-4933-b5ea-0614d0edd6a5", "Free on Board"); } }
					public static MultilingualString FOR { get { return ResString.GetMultilingualString("c9af3652-d109-402a-ab34-04faf095e828", "Free on Rail"); } }
					public static MultilingualString FOT { get { return ResString.GetMultilingualString("531768b3-40e7-4099-bf4e-683c4a0d4dd6", "Free on Truck"); } }
					public static MultilingualString FPC { get { return ResString.GetMultilingualString("cc74861c-d597-4841-9603-97814df8c980", "Free Pipeline Connection"); } }
				}
			}

			public static class ChargeTypes
			{
				public static class Codes
				{
					public const string Allowance = "ALW";
					public const string Charge = "CHG";
					public const string CBPAdjustment = "CBP";
				}

				public static class Descriptions
				{
					public static MultilingualString Allowance { get { return ResString.GetMultilingualString("1114e332-dca6-450d-9289-f417643764f7", "Allowance"); } }
					public static MultilingualString Charge { get { return ResString.GetMultilingualString("d88538d4-d48a-4da6-9172-38d05a410e29", "Charge"); } }
					public static MultilingualString CBPAdjustment { get { return ResString.GetMultilingualString("f101ba79-8c7a-47d5-b59c-4e0b16727c89", "CBP Adjustment"); } }
				}
			}

			public static class FeeCodes
			{
				public const string Avocado = "107";
				public const string Beef = "053";
				public const string Blueberry = "106";
				public const string Coffee = "672";
				public const string Cotton = "056";
				public const string DairyFee = "110";
				public const string DistilledSpirits = "016";
				public const string DutiableMail = "496";
				public const string FreshLimes = "102";
				public const string HMF = "501";
				public const string Honey = "055";
				public const string Mango = "108";
				public const string MerchandiseInformal = "311";
				public const string MerchandiseProcessing = "499";
				public const string MerchandiseSurcharge = "500";
				public const string Mushroom = "103";
				public const string OtherAgencies = "058";
				public const string OtherExcise = "022";
				public const string Pork = "054";
				public const string Potato = "090";
				public const string Raspberry = "057";
				public const string SoftwoodLumber = "105";
				public const string Sorghum = "109";
				public const string Sugar = "079";
				public const string Tobacco = "018";
				public const string Watermelon = "104";
				public const string Wines = "017";
				public const string Pecan = "124";
				public const string ChristmasTree = "125";
				public const string Duty = "DTY";
				public const string ReconciliationInterest = "ARS";
				public const string MPC = "MPC";
				public const string AntidumpingDuty = "ADD";
				public const string CountervailingDuty = "CVD";
				public const string ExciseTaxPayable = "ETP";
				public const string ExciseTaxDeferred = "ETD";
			}
		}

		#endregion

		#endregion

		#region Document Manager

		#region RefDocTypes is now OBSOLETE
		/*  PLEASE NOTE:  RefDocTypes is now in table form which client can add their own document types in addition to our system defined ones.
		 *
		 *  Adding new system defined RefDocTypes must be done via Generator.exe (see DPIB OneNote - "Add to Reference / System Data")
		 *  You MUST ensure clients are not already using the code.
		 *  It is highly recommended that new RefDocTypes have a category specific to a team.
		 *  A serialization validation in TemplateSerializationHelper, ensures that all System DocTypes in Documents.xml must begin with the letter 'S'.
		 *
		*/
		public static class RefDocTypes
		{
			public const string AirFreightManifest = "AFM";
			public const string AgentsInvoice = "AGI";
			public const string AgentsInstruction = "AIN";
			public const string ArrivalNoticeAndChargeSheet = "ARC";
			public const string AuthorsationRelease = "ARE";
			public const string ArrivalNotice = "ARN";
			public const string AuthorityToDeal = "ATD";
			public const string BeneficiaryCertificate = "BCT";
			public const string BankDraft = "BDR";
			public const string BookingConfirmation = "BKC";
			public const string BillOfEntry = "BOE";
			public const string BankReconciliation = "BRC";
			public const string CartageAdvice = "CAD";
			public const string CartageAdviceWithReceipt = "CAR";
			public const string CustomsAuthority = "CAU";
			public const string ContainerDetentionReminder = "CDR";
			public const string ChargeSheet = "CHG";
			public const string ClaimLog = "CLL";
			public const string CommercialInvoice = "CIV";
			public const string ContainerList = "CLI";
			public const string ContainerLiabilityStatement = "CLS";
			public const string CertificateOfOrigin = "COO";
			public const string CertificateOfReceipt = "COR";
			public const string ComplianceReport = "CRP";
			public const string CustomsCertificate = "CUS";
			public const string DelayAlert = "DAL";
			public const string DocumentsAvailableNotice = "DAN";
			public const string DocumentaryCollectionForm = "DCF";
			public const string DirectDebit = "DDR";
			public const string DangerousGoodsForm = "DGF";
			public const string DisbursementNote = "DIS";
			public const string DeliveryLabels = "DLB";
			public const string DepartureNotice = "DNO";
			public const string DocumentOfOrigin = "DOO";
			public const string DeliveryOrder = "DOR";
			public const string DockReceipt = "DRC";
			public const string ExportCartageAdvice = "ECA";
			public const string ExchangeControlDeclaration = "ECD";
			public const string ExportColoadMasterManifest = "ECM";
			public const string ExportCartageAdviceWithReceipt = "ECR";
			public const string EFTRequest = "EFT";
			public const string EntryPrint = "EPR";
			public const string ETailV1Data = "EV1";
			public const string ExportReceivalAdvice = "ERA";
			public const string ExportAuthority = "EXA";
			public const string ForwardingInstruction = "FWI";
			public const string HVLVArchivedData = "HAR";
			public const string HouseAirWaybill = "HAW";
			public const string HouseBill = "HBL";
			public const string HealthCertificate = "HLT";
			public const string ImageFile = "IMG";
			public const string ImportCartageAdvice = "ICA";
			public const string ImportColoadMasterManifest = "ICM";
			public const string ImportCartageAdviceWithReceipt = "ICR";
			public const string InvoiceLinesReport = "ILR";
			public const string InsuranceCertificate = "INS";
			public const string Invoice = "INV";
			public const string InterimReceipt = "IRT";
			public const string Label = "LBL";
			public const string LandedCosting = "LCO";
			public const string Letter = "LET";
			public const string ExportLicense = "LIC";
			public const string LetterOfIndemnity = "LID";
			public const string LettersOfCredit = "LOC";
			public const string Manifest = "MAN";
			public const string MasterAirWaybill = "MAW";
			public const string MasterBill = "MBL";
			public const string OceanMasterBill = "OBL";
			public const string MiscellaneousDocument = "MSC";
			public const string MasterHouse = "MSH";
			public const string ShipmentNotes = "NOT";
			public const string OutturnReport = "OUT";
			public const string PreAlert = "PAL";
			public const string ProFormaInvoice = "PFI";
			public const string PackingDeclaration = "PKD";
			public const string PackingList = "PKL";
			public const string PowerOfAttorney = "POA";
			public const string PowerOfAttorneyCustoms = "POC";
			public const string PowerOfAttorneyForwarding = "POF";
			public const string ProfitShareCalculationWorksheet = "PRS";
			public const string InternallyCreatedPrivateDocument = "PRV";
			public const string InternallyCreatedPublicDocument = "PUB";
			public const string QuarantineCertificate = "QRC";
			public const string QuarantineRemotePrint = "QRP";
			public const string QuarantinePrintPreview = "QPP";
			public const string Quotation = "QUO";
			public const string RequestDocument = "REQ";
			public const string RequestForService = "RQS";
			public const string ShippingAdvice = "SAD";
			public const string SanitaryCertificate = "SAN";
			public const string ShippersDepartureNotice = "SDN";
			public const string SubHouseManifest = "SHM";
			public const string ShippingOrder = "SHO";
			public const string ShippersLetterOfInstruction = "SLI";
			public const string StatementOfAccount = "SOA";
			public const string FirstReminder = "1RM";
			public const string SecondReminder = "2RM";
			public const string CollectionLetter = "COL";
			public const string DemandLetter = "DEM";
			public const string WebDocument = "WEB";
			public const string WeightMeasurementReport = "WMR";
			public const string WarehousePickingSlip = "WPS";
			public const string Worksheet = "WSH";
			public const string FoodControlCertificate = "FDC";
			public const string FumigationCertificate = "FUM";
			public const string ManufacturersDeclaration = "MAN";
			public const string MotorVehicleCertificate = "MVC";
			public const string HeXiaoDan = "HXD";
			public const string QuarantinePackingDeclaration = "QPK";
			public const string VetinaryCertificate = "VET";
			public const string ReleaseRemovalAdvice = "RRA"; // RRA11, RRA12 - Enterprise.Customs.GB.MCP
			public const string ClearanceAdvice = "CLR";
			public const string Email = "EML";
			public const string VATExporterExemption = "EXV";
			public const string CustomsValuationDocument = "CVD";
			public const string Cotton = "COT";
			public const string Nafta = "NAF";
			public const string KnownConsignorAgreement = "KCA";
			public const string Resume = "RES";
			public const string CustomsFormattedFile = "CFF";
			public const string CreditReport = "CRR";
			public const string ConsignmentSecurityDeclaration = "CSD";
			public const string WithholdingTaxExemption = "WTE";
			public const string Permit = "PER";
			public const string AsycudaCusInBondMoveHeader = "PMT";
			public const string Enquiry = "STCI";
			public const string ScheduledReport = "SREP";
			public const string SystemQuotation = "SQTE";
			public const string TransitAccompanyingDocument = "TAD";
			public const string CompetentAuthorityApproval = "SCAA";
		}

		public static class RefDocTypeDescriptions
		{
			public static MultilingualString AirFreightManifest { get { return ResString.GetMultilingualString("01305bf4-f300-4e7a-b528-c103275099c7", "Air Freight Manifest"); } }
			public static MultilingualString AgentsInvoice { get { return ResString.GetMultilingualString("ae0d6143-1e6b-4a7e-9f00-5d4df6001b99", "Agents Invoice"); } }
			public static MultilingualString AgentsInstruction { get { return ResString.GetMultilingualString("973ace35-e348-4327-a812-4aebb291bbdc", "Agents Instruction"); } }
			public static MultilingualString ArrivalNoticeAndChargeSheet { get { return ResString.GetMultilingualString("ecdaecca-36b6-459a-bc08-fbab24133a0f", "Arrival Notice and Charge Sheet"); } }
			public static MultilingualString AuthorsationRelease { get { return ResString.GetMultilingualString("63157d57-9ace-47e0-a06d-5d8c24674dbc", "Authorization Release"); } }
			public static MultilingualString ArrivalNotice { get { return ResString.GetMultilingualString("6ebe2930-ae58-4561-b1f1-9cb9c1550b80", "Arrival Notice"); } }
			public static MultilingualString AuthorityToDeal { get { return ResString.GetMultilingualString("e6540360-4c34-4590-bb30-0a116f67e85e", "Authority to Deal"); } }
			public static MultilingualString BeneficiaryCertificate { get { return ResString.GetMultilingualString("10348c5e-4bce-487f-a74c-bd4c5e8a024e", "Beneficiary Certificate"); } }
			public static MultilingualString BankDraft { get { return ResString.GetMultilingualString("6dbe58e1-1f96-4a40-a4c7-a4f32676ff8e", "Bank Draft"); } }
			public static MultilingualString BookingConfirmation { get { return ResString.GetMultilingualString("b3a39f35-a180-4101-9a16-286de5fae160", "Booking Confirmation"); } }
			public static MultilingualString BillOfEntry { get { return ResString.GetMultilingualString("5a4fd3ca-2f50-4a7c-8210-a849f6c8716f", "Bill Of Entry"); } }
			public static MultilingualString CartageAdvice { get { return ResString.GetMultilingualString("cce377b5-7b8e-4ab9-8d56-36191bc431f2", "Cartage Advice"); } }
			public static MultilingualString CartageAdviceWithReceipt { get { return ResString.GetMultilingualString("858bcdb2-9652-4beb-b3c3-9c3dc1b910c8", "Cartage Advice With Receipt"); } }
			public static MultilingualString CustomsAuthority { get { return ResString.GetMultilingualString("f1cdd7fd-88e9-4065-a41c-170d602d578d", "Customs Authority"); } }
			public static MultilingualString ContainerDetentionReminder { get { return ResString.GetMultilingualString("e88be4fb-80d6-4578-9010-644c66b9177d", "Container Detention Reminder"); } }
			public static MultilingualString ChargeSheet { get { return ResString.GetMultilingualString("9910d91e-4a26-45a0-acfe-feade07ff5b6", "Charge Sheet"); } }
			public static MultilingualString ClaimLog { get { return ResString.GetMultilingualString("f042df7e-58e7-4cd5-9c77-c6ebeb796fcb", "Claim Log"); } }
			public static MultilingualString CommercialInvoice { get { return ResString.GetMultilingualString("ee96d97c-70fb-4261-9e3c-319dad82eb7e", "Commercial Invoice"); } }
			public static MultilingualString PackingList { get { return ResString.GetMultilingualString("01e20db1-7205-4ce6-b6fb-bd42291e24a8", "Packing List"); } }
			public static MultilingualString CertificateOfOrigin { get { return ResString.GetMultilingualString("ee260ded-0f0e-4e5a-9a68-f5c37b8545de", "Certificate of Origin"); } }
			public static MultilingualString CertificateOfReceipt { get { return ResString.GetMultilingualString("41d88c2c-849e-4000-82cd-06cd364fe04b", "Certificate of Receipt"); } }
			public static MultilingualString CustomsCertificate { get { return ResString.GetMultilingualString("290b3ebe-758a-42d1-9caa-eee73bea9093", "Customs Certificate"); } }
			public static MultilingualString DelayAlert { get { return ResString.GetMultilingualString("86bef2af-0e29-4029-b3b7-268acf4197cb", "Delay Alert"); } }
			public static MultilingualString DocumentsAvailableNotice { get { return ResString.GetMultilingualString("9401a0dd-5fe5-4992-b692-27092e43597a", "Documents Available Notice"); } }
			public static MultilingualString DocumentaryCollectionForm { get { return ResString.GetMultilingualString("9081ef42-020f-4d6c-8075-9955670cbb7d", "Documentary Collection Form"); } }
			public static MultilingualString DirectDebit { get { return ResString.GetMultilingualString("640c8c12-7de2-4ca0-9d32-9f37197c8d3e", "Direct Debit"); } }
			public static MultilingualString DangerousGoodsForm { get { return ResString.GetMultilingualString("43fbf1d7-05c6-4657-8637-ec673e8150bb", "Dangerous Goods Form"); } }
			public static MultilingualString DisbursementNote { get { return ResString.GetMultilingualString("99357b2e-1b04-4061-8aaf-172f48665569", "Disbursement Note"); } }
			public static MultilingualString DeliveryLabels { get { return ResString.GetMultilingualString("7cfb8d96-f903-4fc2-9f38-772c51bc214d", "Delivery Labels"); } }
			public static MultilingualString DepartureNotice { get { return ResString.GetMultilingualString("bc159850-92f2-4bef-b8c4-d8523999efcc", "Departure Notice"); } }
			public static MultilingualString DocumentOfOrigin { get { return ResString.GetMultilingualString("64845c55-037a-479d-85e0-f1ff9f6f5060", "Document Of Origin"); } }
			public static MultilingualString DeliveryOrder { get { return ResString.GetMultilingualString("e96e8e19-5bea-49fb-a861-c546e562b071", "Delivery Order"); } }
			public static MultilingualString DockReceipt { get { return ResString.GetMultilingualString("12c6773c-be60-4557-b164-2bd15b064a24", "Dock Receipt"); } }
			public static MultilingualString ExportCartageAdvice { get { return ResString.GetMultilingualString("bf0229ea-c2f7-4bb2-af9b-3e0c6d29fb42", "Export Cartage Advice"); } }
			public static MultilingualString ExchangeControlDeclaration { get { return ResString.GetMultilingualString("e2893245-8ebf-40a5-acc7-2612b59830e5", "Exchange Control Declaration"); } }
			public static MultilingualString ExportColoadMasterManifest { get { return ResString.GetMultilingualString("6a9cc477-a374-4599-87b8-6af03ccfe312", "Export Coload Master Manifest"); } }
			public static MultilingualString ExportCartageAdviceWithReceipt { get { return ResString.GetMultilingualString("c263fba7-6a79-4ed3-b5eb-5abaee60211e", "Export Cartage Advice with Receipt"); } }
			public static MultilingualString EftRequest { get { return ResString.GetMultilingualString("4cd08fa1-d87b-4e95-b6f7-e0b124d8a255", "EFT Request"); } }
			public static MultilingualString EntryPrint { get { return ResString.GetMultilingualString("bb42a4c4-4bfd-4955-8aee-fed807edad94", "Entry Print"); } }
			public static MultilingualString ExportReceivalAdvice { get { return ResString.GetMultilingualString("f0005989-0a56-4fa5-98cb-1dacb9dc2aa5", "Export Receival Advice"); } }
			public static MultilingualString ExportAuthority { get { return ResString.GetMultilingualString("dcb8a660-b4cf-4bcc-99b9-bbbc8863aea7", "Export Authority"); } }
			public static MultilingualString ForwardingInstruction { get { return ResString.GetMultilingualString("fa15ece4-4fd2-444c-a8a6-532b9fbfdfaf", "Forwarding Instruction"); } }
			public static MultilingualString HVLVArchivedData { get { return ResString.GetMultilingualString("217b3dcb-c707-4eee-be7f-8bd2383e1920", "HVLV Archived Data"); } }
			public static MultilingualString HouseAirWaybill { get { return ResString.GetMultilingualString("60035da3-5b07-4f36-94a7-1d5e3ddf0378", "House Air Waybill"); } }
			public static MultilingualString HouseBill { get { return ResString.GetMultilingualString("bc929045-9244-446f-a48a-07dd58e0eddd", "House Waybill/Bill of Lading"); } }
			public static MultilingualString HealthCertificate { get { return ResString.GetMultilingualString("8fc67fc8-b442-41d0-a2b2-d3fd7b707d07", "Health Certificate"); } }
			public static MultilingualString ImportCartageAdvice { get { return ResString.GetMultilingualString("34b07fd4-d54e-4f7f-b17d-df5c895a6673", "Import Cartage Advice"); } }
			public static MultilingualString ImportColoadMasterManifest { get { return ResString.GetMultilingualString("2784d575-5a29-4798-a5f0-c6caa681ccfc", "Import Coload Master Manifest"); } }
			public static MultilingualString ImportCartageAdviceWithReceipt { get { return ResString.GetMultilingualString("cb9e27eb-0a59-4e55-bbfa-96890f732930", "Import Cartage Advice with Receipt"); } }
			public static MultilingualString InvoiceLinesReport { get { return ResString.GetMultilingualString("064efb5c-8ffd-4a4a-923b-19cfbc88217f", "Invoice Lines Report"); } }
			public static MultilingualString InsuranceCertificate { get { return ResString.GetMultilingualString("e559afa2-ed9f-4cec-8e00-299712161af1", "Insurance Certificate"); } }
			public static MultilingualString Invoice { get { return ResString.GetMultilingualString("af508db7-72d8-4e4f-bc3a-82f7f421158e", "Invoice"); } }
			public static MultilingualString InterimReceipt { get { return ResString.GetMultilingualString("3e31c540-2c6a-4e47-8bd3-693bafb8d73e", "Interim Receipt"); } }
			public static MultilingualString Label { get { return ResString.GetMultilingualString("d1e36ba8-a442-4f66-9320-82a1a4ee41f7", "Label"); } }
			public static MultilingualString LandedCosting { get { return ResString.GetMultilingualString("4433f798-1c07-4839-a0f7-5d2182c22964", "Landed Costing"); } }
			public static MultilingualString Letter { get { return ResString.GetMultilingualString("4cace4db-adc6-4967-99d0-595ddb268598", "Letter"); } }
			public static MultilingualString ExportLicense { get { return ResString.GetMultilingualString("658f574d-0dee-4872-9167-fdffdf456682", "Export License"); } }
			public static MultilingualString LetterOfIndemnity { get { return ResString.GetMultilingualString("49e9191c-f507-48e4-bf31-9726370e2244", "Letter of Indemnity"); } }
			public static MultilingualString LettersOfCredit { get { return ResString.GetMultilingualString("b0a548f2-53b2-4639-827c-d4d166f931c8", "Letters of Credit"); } }
			public static MultilingualString Manifest { get { return ResString.GetMultilingualString("ee9000c8-7408-4ff8-b7b3-379e7e90afe1", "Manifest"); } }
			public static MultilingualString MasterAirWaybill { get { return ResString.GetMultilingualString("4ac5a2cb-806c-4660-bddd-54ee1eb8b063", "Master Air Waybill"); } }
			public static MultilingualString MasterBill { get { return ResString.GetMultilingualString("6d12d733-1082-4dde-a2c0-9432d5afba33", "Airway Bill/Ocean Bill of Lading"); } }
			public static MultilingualString MiscellaneousDocument { get { return ResString.GetMultilingualString("59ad3b08-d2a3-4294-883b-429f9d904fb5", "Miscellaneous Document"); } }
			public static MultilingualString MasterHouse { get { return ResString.GetMultilingualString("ea391963-a328-43fe-98b7-632d4925cd47", "Master House"); } }
			public static MultilingualString ShipmentNotes { get { return ResString.GetMultilingualString("2235c00d-dc5e-42ff-ad41-202801f8e5d4", "Shipment Notes"); } }
			public static MultilingualString OutturnReport { get { return ResString.GetMultilingualString("8e15515b-e4da-4bba-bbd7-15845c143509", "Outturn Report"); } }
			public static MultilingualString PreAlert { get { return ResString.GetMultilingualString("989df688-2a9e-472c-b257-f41cea1c7da1", "Pre Alert"); } }
			public static MultilingualString ProFormaInvoice { get { return ResString.GetMultilingualString("23bad584-b063-4df0-bdf5-99d590a88e89", "Pro Forma Invoice"); } }
			public static MultilingualString PackingDeclaration { get { return ResString.GetMultilingualString("cde4f41e-88e5-49ed-a4b7-b1357cc743ad", "Packing Declaration"); } }
			public static MultilingualString PowerOfAttorney { get { return ResString.GetMultilingualString("bcddb937-c23c-4a46-8e93-596b52a47103", "Power of Attorney"); } }
			public static MultilingualString PowerOfAttorneyCustoms { get { return ResString.GetMultilingualString("afe1cee2-6202-4abc-9629-74b3aa5bef9b", "Power of Attorney Customs"); } }
			public static MultilingualString PowerOfAttorneyForwarding { get { return ResString.GetMultilingualString("48472781-b68e-46f9-a1b5-fa2881530a00", "Power of Attorney Forwarding"); } }
			public static MultilingualString ProfitShareCalculationWorksheet { get { return ResString.GetMultilingualString("d323584a-d1b2-4ae5-b651-2edce0a374fb", "Profit Share Calculation Worksheet"); } }
			public static MultilingualString InternallyCreatedPrivateDocument { get { return ResString.GetMultilingualString("6b78446d-b6d1-412c-821e-6e4db41ee50c", "Internally Created Private Document"); } }
			public static MultilingualString InternallyCreatedPublicDocument { get { return ResString.GetMultilingualString("e82c8e8f-f5a0-4fcd-ae30-0c817c6a7ec6", "Internally Created Public Document"); } }
			public static MultilingualString QuarantineCertificate { get { return ResString.GetMultilingualString("13fe1600-8245-4b63-a605-7ed4607c747f", "Quarantine Certificate"); } }
			public static MultilingualString QuarantineRemotePrint { get { return ResString.GetMultilingualString("f126625e-6681-4fb0-8697-2a39e05b248e", "Quarantine Remote Print"); } }
			public static MultilingualString QuarantinePrintPreview { get { return ResString.GetMultilingualString("F8BE6972-4D84-4A79-8601-92B14FD0EA19", "Quarantine Print Preview"); } }
			public static MultilingualString Quotation { get { return ResString.GetMultilingualString("6e9a7431-63b9-438c-a30a-ad700c3563c8", "Quotation"); } }
			public static MultilingualString RequestDocument { get { return ResString.GetMultilingualString("42e87a6a-34d2-4581-812d-74355a87cd76", "Request Document"); } }
			public static MultilingualString RequestForService { get { return ResString.GetMultilingualString("23849905-3e9a-4eaf-bf4d-a01cbd437bcf", "Request for Service"); } }
			public static MultilingualString ShippingAdvice { get { return ResString.GetMultilingualString("d6a5ae75-8ca1-4003-902a-08342b5af44d", "Shipping Advice"); } }
			public static MultilingualString SanitaryCertificate { get { return ResString.GetMultilingualString("fd3c2bca-16d0-4b2c-9674-aec4ea8a02c8", "Sanitary Certificate"); } }
			public static MultilingualString ShippersDepartureNotice { get { return ResString.GetMultilingualString("fa37018f-3595-40ec-a151-d023d0435db0", "Shippers Departure Notice"); } }
			public static MultilingualString SubHouseManifest { get { return ResString.GetMultilingualString("2cc2feb6-5cca-4c16-86a2-6e7fcdaf52a8", "Sub House Manifest"); } }
			public static MultilingualString ShippingOrder { get { return ResString.GetMultilingualString("c69272ea-0ca0-42a8-8c81-748916a7eccb", "Shipping Order"); } }
			public static MultilingualString ShippersLetterOfInstruction { get { return ResString.GetMultilingualString("e214b5f1-caf9-42a6-93aa-d51c08f6aa32", "Shippers Letter of Instruction"); } }
			public static MultilingualString StatementOfAccount { get { return ResString.GetMultilingualString("499aece0-e7c1-41d9-9e3b-f18e16d6b6e4", "Statement of Account"); } }
			public static MultilingualString FirstReminder { get { return ResString.GetMultilingualString("fb91c268-f63d-4155-9e56-255d6b414960", "Request for Immediate Payment"); } }
			public static MultilingualString SecondReminder { get { return ResString.GetMultilingualString("a38aa328-0ea6-46dc-a526-f6e83a2c7a36", "2nd Request For Immediate Payment"); } }
			public static MultilingualString CollectionLetter { get { return ResString.GetMultilingualString("6077f3f6-669e-4a0c-9beb-17cfebb1689a", "Collection Letter"); } }
			public static MultilingualString DemandLetter { get { return ResString.GetMultilingualString("b4b51ac0-64a1-40d7-b17e-b31ca9e1c81f", "Letter of Demand"); } }
			public static MultilingualString WebDocument { get { return ResString.GetMultilingualString("77303da1-95a2-44d2-a089-551abc401f9d", "Web Document"); } }
			public static MultilingualString WeightMeasurementReport { get { return ResString.GetMultilingualString("fa15d474-0e5f-457c-94c6-0d18a04a0b87", "Weight/Measurement Report"); } }
			public static MultilingualString WarehousePickingSlip { get { return ResString.GetMultilingualString("06258342-73c7-4e36-8aff-10435cf85733", "Warehouse Picking Slip"); } }
			public static MultilingualString Worksheet { get { return ResString.GetMultilingualString("f422ecfb-15d0-4a4d-9815-398874aaea40", "Worksheet"); } }
			public static MultilingualString FoodControlCertificate { get { return ResString.GetMultilingualString("36ff4b55-8e3b-4d7a-84e8-0581ac982b20", "Food Control Certificate"); } }
			public static MultilingualString FumigationCertificate { get { return ResString.GetMultilingualString("a02786ec-49d6-4aa3-905d-f0304aba2181", "Fumigation Certificate"); } }
			public static MultilingualString ManufacturersDeclaration { get { return ResString.GetMultilingualString("037b2b7e-48a5-463a-8fbc-6b889f059064", "Manufacturer's Declaration"); } }
			public static MultilingualString MotorVehicleCertificate { get { return ResString.GetMultilingualString("06dbbfdd-d10b-4093-aee1-2f04ca9c9b53", "Motor Vehicle Certificate"); } }
			public static MultilingualString HeXiaoDan { get { return ResString.GetMultilingualString("543082a3-a749-4033-ab96-e182010d5b05", "He Xiao Dan"); } }
			public static MultilingualString QuarantinePackingDeclaration { get { return ResString.GetMultilingualString("e3e859be-9bad-48df-99f0-7214f5357da6", "Quarantine Packing Declaration"); } }
			public static MultilingualString VetinaryCertificate { get { return ResString.GetMultilingualString("0d714c8a-b159-4420-b5f3-f24fb752c157", "Veterinary Certificate"); } }
			public static MultilingualString ReleaseRemovalAdvice { get { return ResString.GetMultilingualString("0bae1a97-3fee-44d7-a818-f7692ee36f24", "Release/Removal Advice (RRA)"); } }
			public static MultilingualString ClearanceAdvice { get { return ResString.GetMultilingualString("93a95652-679d-4399-8913-24691c60ccea", "Clearance advice"); } }
			public static MultilingualString Email { get { return ResString.GetMultilingualString("6731db33-6f6b-4045-902f-672577680a06", "Email"); } }
			public static MultilingualString VATExporterExemption { get { return ResString.GetMultilingualString("4957aa90-6d41-4368-a226-3f339ccbf7b8", "VAT/GST Exporter Exemption (AR and AP Invoicing)"); } }
			public static MultilingualString CustomsValuationDocument { get { return ResString.GetMultilingualString("10D579A1-FBE4-4EC9-A302-D6C5A1954BF2", "Customs Valuation Document"); } }
			public static MultilingualString GLJournal { get { return ResString.GetMultilingualString("3D8D6E89-9604-479C-93F8-E9177B53EC99", "GL Journal Document"); } }
			public static MultilingualString KnownConsignorAgreement { get { return ResString.GetMultilingualString("5aa42eae-b9e3-415e-8a1d-44c8e7a55303", "Known Consignor Agreement"); } }
			public static MultilingualString CreditReport { get { return ResString.GetMultilingualString("37FFE75D-5846-420B-9B1C-5FC6F81F88AD", "Credit Report"); } }
			public static MultilingualString ConsignmentSecurityDeclaration { get { return ResString.GetMultilingualString("a55f7f44-d85f-46b0-849c-a04233084189", "Consignment Security Declaration"); } }
			public static MultilingualString Permit { get { return ResString.GetMultilingualString("9d5dfd98-f3e8-b499-4da6-b66342a0cff5", "Permit"); } }
			public static MultilingualString ScheduledReport { get { return ResString.GetMultilingualString("373EF934-046A-4CD1-AA98-AE579F49ED30", "Scheduled Report"); } }
			public static MultilingualString SystemQuotation { get { return ResString.GetMultilingualString("ab17d478-9def-43c7-ba6f-dd78c0be6852", "System Quotation"); } }
			public static MultilingualString CompetentAuthorityApproval { get { return ResString.GetMultilingualString("97B683D8-7E81-48D2-9083-891AEE59F528", "Competent Authority Approval"); } }
		}

		#endregion

		public static class ReferenceTypes
		{
			public const string All = "ALL";
			public const string ClientSupplierRelationship = "CSR";
			public const string Accounting = "ACC";
			public const string SupplyChainLogistics = "SCL";
			public const string GeneralReferenceTables = "REF";
			public const string HumanResourcesStaffEmployment = "HRE";
			public const string BusinessEntityProcessWorkflow = "BPW";
			public const string Unallocated = "UNA";
			public const string ComplianceReport = "CTR";
		}

		public static class ReferenceTypeDescriptions
		{
			public static MultilingualString All { get { return ResString.GetMultilingualString("F5713E19-4076-4427-9E48-8CBFB90EA3C4", "ALL"); } }
			public static MultilingualString ClientSupplierRelationship { get { return ResString.GetMultilingualString("B197B4F2-248A-46FF-B260-72BDC5E755E5", "Client / Supplier / Relationship"); } }
			public static MultilingualString Accounting { get { return ResString.GetMultilingualString("AC09E517-F357-40E5-85BE-41DF504318C1", "Accounting"); } }
			public static MultilingualString SupplyChainLogistics { get { return ResString.GetMultilingualString("142D4856-193C-41C0-BDF1-675947835094", "Supply Chain & Logistics"); } }
			public static MultilingualString GeneralReferenceTables { get { return ResString.GetMultilingualString("61140EC0-9B62-4BD6-93D4-0390E59A6870", "General Reference Tables"); } }
			public static MultilingualString HumanResourcesStaffEmployment { get { return ResString.GetMultilingualString("EED4BDC0-472D-46F5-BE47-B2B798E3F735", "Human Resources / Staff / Employment"); } }
			public static MultilingualString BusinessEntityProcessWorkflow { get { return ResString.GetMultilingualString("3B919AEF-3488-48F2-8559-A3CF9729DD16", "Business Entity, Process or Workflow"); } }
			public static MultilingualString ComplianceReport { get { return ResString.GetMultilingualString("BB4E5B3F-CC30-46E2-85B2-3A29E77A35AC", "Compliance Report"); } }
		}

		public static class TimeUntilDocumentExpiry
		{
			public const int Days = -40;
		}

		public static class DocManagerCodes
		{
			public const string ComplianceDocument = "CDU";
			public const string InvoicingJob = "IVJ";
			public const string APPayment = "ACP";
			public const string AccountingDraftInvoiceHeader = "AIH";
			public const string PayableInvoice = "PIN";
			public const string ReceivableInvoice = "RIN";
			public const string DirectDebitBatch = "DDR";
			public const string PayableCreditNote = "PCR";
			public const string ReceivableCreditNote = "RCR";
			public const string BankAccount = "BAC";
			public const string HotCheque = "CHQ";
			public const string CashBook = "CSH";
			public const string ChargeCode = "CHC";
			public const string TaxOverrideGroup = "TOG";
			public const string ChequeBook = "CBK";
			public const string ComplianceSequence = "CSQ";
			public const string GLAccounts = "GLA";
			public const string ARClaimsAndQueries = "RCQ";
			public const string APClaimsAndQueries = "PCQ";
			public const string Shipment = "SHP";
			public const string Booking = "BKG";
			public const string Consol = "CON";
			public const string Order = "ORD";
			public const string PayableOrder = "POD";
			public const string JobSupplierBooking = "SBK";
			public const string ContainerLoadList = "CLH";
			public const string ContainerLoadPlan = "CLP";
			public const string CollectionBatch = "COB";
			public const string CollectionOrder = "COO";
			public const string JobShipmentPrePlanning = "SPA";
			public const string TransportJob = "TRN";
			public const string LocalTransportLeg = "LTL";
			public const string DomesticTransportBooking = "DTB";
			public const string DomesticTransportBookingConsolidation = "DTC";
			public const string DomesticTransportConsignment = "DMC";
			public const string DomesticTransportRunSheet = "DRS";
			public const string DomesticTransportLinehaulManifest = "DLM";
			public const string LandTransportConsignment = "LTC";
			public const string LandTransportConsignmentAction = "LTA";
			public const string SailingSchedule = "SEA";
			public const string TruckingSchedule = "ROA";
			public const string RailSchedule = "RAI";
			public const string FlightSchedule = "AIR";
			public const string GatePass = "GPS";
			public const string Container = "CNT";
			public const string TallyContainer = "TAL";
			public const string CFSShipmentReceival = "CSR";
			public const string LoadList = "LOA";
			public const string WarehouseAdHocServiceJob = "WSJ";
			public const string WarehouseReceiveDocket = "WID";
			public const string WarehouseOrderDocket = "WOD";
			public const string WarehouseWorkOrder = "WWO";
			public const string WarehouseDynamicWorkOrder = "WDO";
			public const string WarehouseAdjustmentDocket = "WAD";
			public const string WarehouseTransferDocket = "WTD";
			public const string WarehousePick = "WPI";
			public const string WarehouseStocktake = "WST";
			public const string WarehouseInvoice = "WIV";
			public const string WarehouseInventory = "WIN";
			public const string WarehouseVASOrder = "WVO";
			public const string WarehouseLoad = "WLO";
			public const string CommercialInvoice = "CIV";
			public const string JobDeclaration = "DEC";
			public const string ExportClassification = "EXC";
			public const string ImportClassification = "IMC";
			public const string Product = "PRD";
			public const string Package = "PKG";
			public const string PacksConversion = "PAC";
			public const string PackType = "PAT";
			public const string AirCargoMaster = "ACG";
			public const string AirCargoHouse = "ACH";
			public const string AirCTO = "ACT";
			public const string CustomsExportManifest = "CEM";
			public const string VoyageManifest = "VYM";
			public const string Classification = "CLS";
			public const string Company = "CPY";
			public const string CompanySpecificOrg = "CSO";
			public const string Organisation = "ORG";
			public const string OrgOpportunity = "OPP";
			public const string ProcessTask = "TSK";
			public const string CommodityCode = "COM";
			public const string ContainerReferenceFiles = "CTN";
			public const string Currency = "CUR";
			public const string DocumentType = "DOT";
			public const string Equipment = "EQU";
			public const string ServiceLevel = "SER";
			public const string Vessel = "VSL";
			public const string NeutralMasters = "MST";
			public const string Branch = "BRN";
			public const string Department = "DEP";
			public const string UNLOCO = "UNL";
			public const string Country = "COU";
			public const string Airline = "ALN";
			public const string Group = "GRP";
			public const string Staff = "STF";
			public const string Person = "PER";
			public const string Capability = "CAP";
			public const string ArchiveSchedule = "ARS";
			public const string IncidentApproval = "IAP";
			public const string IncidentRequest = "IRQ";
			public const string CompanyCampaign = "GCC";
			public const string CompanyCampaignItem = "GCI";
			public const string Quotation = "QUO";
			public const string OneOffQuote = "QU1";
			public const string ClientRate = "CRT";
			public const string GlobalClientRate = "GCL";
			public const string Costing = "CST";
			public const string GlobalCosting = "GCO";
			public const string CompanyTariff = "CTF";
			public const string GlobalCompanyTariff = "GCT";
			public const string JobApplicant = "APP";
			public const string JobCampaign = "CAM";
			public const string JobRole = "JRO";
			public const string SCAOceanBill = "SCO";
			public const string SCAHouseBill = "SCH";
			public const string SalesEnquiry = "INQ";
			public const string CommunicationManager = "CMM";
			public const string CustomsEntry = "CEH";
			public const string CustomsPermit = "CPH";
			public const string CustomsGuarantee = "GUA";
			public const string CusUnderbond = "UNB";
			public const string CustomsContainer = "CCT";
			public const string ReceivableReceipt = "RRC";
			public const string PayableReceipt = "PRC";
			public const string ReceivableAdjustmentNote = "RAN";
			public const string PayableAdjustmentNote = "PAN";
			public const string ReceivablePayment = "RPA";
			public const string ReceivableJournal = "RJN";
			public const string PayableJournal = "PJN";
			public const string DirectReceipt = "DRC";
			public const string OpeningReceipt = "ORC";
			public const string OpeningPayment = "OPY";
			public const string CurrencyAdjustment = "BCA";
			public const string GLJournal = "GLJ";
			public const string GLJournalApprovalRequest = "GJR";
			public const string ReceivablePaymentApprovalWithAuthorisation = "RPV";
			public const string PayablePaymentApprovalWithAuthorisation = "PPV";
			public const string ReceivablePaymentApprovalWithoutAuthorisation = "RVA";
			public const string PayablePaymentApprovalWithoutAuthorisation = "PVA";
			public const string PayableTransactionPendingAllocation = "PPA";
			public const string JobRevenueJournal = "JRJ";
			public const string ComplianceReport = "ACR";
			public const string CreditControlledDocumentsApproval = "CCD";
			public const string TaxConfigurationTemplate = "TCT";
			public const string CrmOpportunity = "COP";

			public const string AgencyShipment = "ASH";
			public const string AgencyContainerManager = "CNS";
			public const string AgencyVoyageAccounting = "AVA";
			public const string AgencySundryCharges = "ASC";
			public const string AgencyBillContainers = "ACN";

			public const string Unallocated = "UNA";
			public const string WarehouseRow = "WRO";

			public const string JobCartageRunSheet = "CWS";
			public const string JobConsolidatedTransportBooking = "CTB";
			public const string LearningCentreCampaignItem = "LCI";

			public const string DetentionInvoice = "ADI";

			public const string CusStatementHeader = "STM";

			public const string JobApplication = "JAP";

			public const string ImporterSecurityFiling = "ISF";
			public const string EDIMessage = "EMS";
			public const string InBond = "INB";
			public const string NctsInBond = "NCT"; // InBond for EU
			public const string USeManifest = "MAN";
			public const string USAMS = "AMS";
			public const string NctsMoveHeader = "NCM";

			public const string BMSystem = "BMS";
			public const string BMBoard = "BMB";
			public const string BMControlCustomisation = "BMC";
			public const string ComponentRelationship = "CRL";
			public const string NetworkDiagram = "BMD";
			public const string MENTAgedScoreQuery = "MAQ";

			public const string InvoiceTaxMessage = "ITM";

			public const string CAeManifest = "CAE";

			public const string WorkItem = "WKI";
			public const string Project = "WKP";
			public const string CustomerServiceTicket = "TKT";

			public const string JPAFRHeader = "JPH";
			public const string LandedCostHeader = "LCH";
			public const string InterchangeAttachments = "IAT";

			public const string PickupDeliveryConfirm = "PDC";

			public const string TransitReceiveConsignment = "TRO";
			public const string TransitReceiveTransportationUnit = "TRU";
			public const string TransitReceiveASN = "TRA";
			public const string TransitDispatchConsignment = "TDC";
			public const string TransitDispatchTransportationUnit = "TDU";
			public const string TransitDispatchLoadList = "TLL";

			public const string PalletTransaction = "PTR";

			public const string CusOutturnHeader = "COH";
			public const string AsycudaManifest = "ASM";
			public const string AsycudaBill = "ASB";

			public const string GateTransport = "GTT";
			public const string GateTransportCFSDetail = "GTF";
			public const string GateTransportCYDetail = "GTC";

			public const string TempStorageHeader = "SJH";
			public const string TempStorageRegHeader = "SRH";
			public const string OutturnGateInOut = "OGM";
			public const string TempStorageRegPremises = "SRP";
			public const string TempStorageHeaderUCC6 = "TS6";

			public const string LocalLanguage = "LAN";

			public const string HVLVBookingHeader = "HLB";
			public const string HVLVConsignment = "HLC";
			public const string HVLVOuterPackage = "HVO";
			public const string HVLVOriginLoadList = "HVL";
			public const string OrgAgentRelationShip = "OAR";

			public const string USLowValueEntries = "SEC";
			public const string USLowValueEntriesConsignmentBill = "SCB";

			public const string CustomsPackingList = "CPL";

			public const string AsycudaCusInBondMoveHeader = "PMT";

			public const string GermanyMonthlyClosing = "GMC";
			public const string DepositBatch = "DEB";
			public const string HiringRequest = "HRQ";

			public const string OnBoarding = "OBD";

			public const string CustomsExitHeader = "CER";
			public const string CustomsExitDetail = "CED";
			public const string CustomsExitConsignment = "CEC";
			public const string CustomsExitReport = "CEP";

			public const string Holiday = "SAR";

			public const string ConsolidatedDeclaration = "CSD";

			public const string ChangeRequest = "GCR";

			public const string ReportStatistic = "RTS";

			public const string CYDAdHocServiceOrder = "CAO";
			public const string CYDReceiveAdvice = "CRA";
			public const string CYDYardUnitState = "CYU";
			public const string CYDPickupHeader = "CPK";
			public const string CYDReleaseAdvice = "CLA";
			public const string CYDTransportationUnit = "CTU";
			public const string MNRWorkOrderHeader = "MWO";
			public const string MNRWorkOrderLine = "MWL";
			public const string MNRSurvey = "MRS";

			public const string CarrierShipmentHeader = "OSH";
			public const string CarrierShipmentCargo = "OSC";

			public const string CarrierVoyage = "VOY";
			public const string CarrierVoyagePortCall = "PRT";

			public const string ReviewProcessNode = "RPN";
			public const string CusGoodsCatalog = "CGC";

			public const string DocTemplateRecord = "DTR";
		}

		public static class JobRequiredDocuments
		{
			public static class DocumentPeriods
			{
				public const string OncePerShipment = "SHP";
				public const string Periodic = "PER";
			}

			public static class DocumentPeriodDescriptions
			{
				public static MultilingualString OncePerShipment { get { return ResString.GetMultilingualString("4DABE5F9-EAEE-40CA-A9F1-589780881B6B", "Once Per Shipment"); } }
				public static MultilingualString Periodic { get { return ResString.GetMultilingualString("CCF8BBBC-FA80-4DE6-9F90-3C0F618C67F1", "Periodic (Document with Expiry Date)"); } }
			}
		}

		public static class EDocsStorageProviders
		{
			public static class Code
			{
				public const string DB = "DB";
				public const string S3 = "S3";
			}

			public static class Description
			{
				public static MultilingualString DB { get { return ResString.GetMultilingualString("ad76e058-5a2a-4560-9436-db2b66f36da9", "SQL Server DocManager database"); } }
				public static MultilingualString S3 { get { return ResString.GetMultilingualString("dc8062e1-66f6-4c55-86a7-d541d9640673", "S3 compatible storage"); } }
			}
		}

		#endregion

		#region Scheduled Report

		public static class ErrorNotificationOptions
		{
			public static class Code
			{
				public const string DEF = "DEF";
				public const string ROL = "ROL";
				public const string GRP = "GRP";
			}

			public static class Description
			{
				public static MultilingualString DEF { get { return ResString.GetMultilingualString("9ACA0BB5-197F-4A71-A47C-839F70499492", "Default Notification Method "); } }
				public static MultilingualString ROL { get { return ResString.GetMultilingualString("3277DC48-3BAF-4947-BDF5-C0E30EECA409", "Send to staff roles"); } }
				public static MultilingualString GRP { get { return ResString.GetMultilingualString("827DB4CD-AF70-4656-9FB1-329F56524F46", "Send to notification group"); } }
			}
		}

		#endregion

		public static class RunSheetNewModes
		{
			public const string RS0_Today = "RS0";
			public const string RS1_Tomorrow = "RS1";
			public const string RS2 = "RS2";
			public const string RS3 = "RS3";
			public const string RS4 = "RS4";
			public const string RS5 = "RS5";
			public const string RS6 = "RS6";
		}

		public static class AgentsReferenceDefaulting
		{
			public const string FAR = "FAR";
			public const string NSR = "NSR";
			public const string PAR = "PAR";
			public const string DEF = "DEF";
		}

		public static class StorageCalculationPeriods
		{
			public const string Default = "DEF";
			public const string Daily = "DLY";
			public const string Weekly = "WKY";
			public const string Fortnightly = "FNY";
			public const string Monthly = "MTH";
			public const string BillingPeriod = "BIL";
		}

		public static class EInvoicingBatchState
		{
			public const string Ready = "RDY";
			public const string Sent = "SNT";
			public const string Discarded = "DCD";
		}

		public static class EInvoicingPivotState
		{
			public const string Queued = "QUE";
			public const string Batched = "BCH";
			public const string BatchedWithError = "BER";
			public const string Sent = "SNT";
			public const string Delivered = "DLV";
			public const string Succeed = "SUC";
			public const string Failed = "FAL";
			public const string Discarded = "DCD";
			public const string Pending = "PEN";
			public const string AwaitingReview = "AWA";
			public const string InProcessing = "IMP";
			public const string NotEligible = "NOT";
		}

		public static class EInvoicingPivotActionType
		{
			public const string Submit = "SUB";
			public const string StatusCheck = "STA";
			public const string DocumentAction = "DOC";
			public const string DocumentDetail = "DOD";
			public const string Cancel = "CAN";
			public const string Adjustment = "ADJ";
			public const string Amend = "AMD";
			public const string Approve = "APR";
			public const string Reject = "REJ";
			public const string ConfirmTransactionReceived = "CRX";

			public static readonly ImmutableArray<string> CommandActionTypes
				= new[] { Submit, Cancel, Adjustment, Amend, ConfirmTransactionReceived, Reject }.ToImmutableArray();

			public static readonly ImmutableArray<string> QueryActionTypes
				= new[] { StatusCheck, DocumentAction, Approve, DocumentDetail }.ToImmutableArray();
		}

		public static class MenuItemFilters
		{
			public const string EDocsProviderPlaceholderTag = "<EDocsProviderPlaceholderFor>";
		}

		public static class MenuNameConstants
		{
			public static MultilingualString PostGatewayAgentCharges { get { return ResString.GetMultilingualString("MenuItem.Constants.PostGatewayAgentCharges", "Post Gateway Agent Charges"); } }
			public static MultilingualString PostOverseasAgentCharges { get { return ResString.GetMultilingualString("MenuItem.Constants.PostOverseasAgentCharges", "Post Overseas Agent Charges"); } }
			public static MultilingualString PostAllCosts { get { return ResString.GetMultilingualString("MenuItem.Constants.PostAllCosts", "Post All Costs"); } }
			public static MultilingualString PostConsolCostsOnly { get { return ResString.GetMultilingualString("MenuItem.Constants.PostConsolCostsOnly", "Post Consol Costs Only"); } }
			public static MultilingualString PostWholeConsol { get { return ResString.GetMultilingualString("MenuItem.Constants.PostWholeConsol", "Post Whole Consol"); } }
			public static MultilingualString PostAllChargesAndCosts { get { return ResString.GetMultilingualString("MenuItem.Constants.PostAllChargesAndCosts", "Post All Charges and Costs"); } }
			public static MultilingualString PostLocalClientCharges { get { return ResString.GetMultilingualString("MenuItem.Constants.PostLocalClientCharges", "Post Local Client Charges"); } }
			public static MultilingualString PostPrepaidBillToParty { get { return ResString.GetMultilingualString("MenuItem.Constants.PrepaidBillToParty", "Post Prepaid Bill-To Party Charges"); } }
			public static MultilingualString PostCollectBillToParty { get { return ResString.GetMultilingualString("MenuItem.Constants.CollectBillToParty", "Post Collect Bill-To Party Charges"); } }
			public static MultilingualString PostAllRevenueCharges { get { return ResString.GetMultilingualString("MenuItem.Constants.PostAllRevenueCharges", "Post All Revenue Charges"); } }
			public static MultilingualString ApportionRevenueToShipments { get { return ResString.GetMultilingualString("MenuItem.Constants.ApportionRevenueToShipments", "Apportion Revenue To Shipments"); } }
			public static MultilingualString PostDisbursementChargesonly { get { return ResString.GetMultilingualString("MenuItem.Constants.PostDisbursementChargesonly", "Post Disbursement Charges only"); } }
			public static MultilingualString PostCosts { get { return ResString.GetMultilingualString("MenuItem.Constants.PostCosts", "Post Costs"); } }
			public static MultilingualString PrintJobProfitDoc { get { return ResString.GetMultilingualString("MenuItem.Constants.PrintJobProfitDoc", "Print Job Profit Document"); } }
			public static MultilingualString PreviewInvoices { get { return ResString.GetMultilingualString("MenuItem.Constants.PreviewInvoices", "Preview Invoices"); } }
			public static MultilingualString ResetUnpostedLinesTaxDefault { get { return ResString.GetMultilingualString("MenuItem.Constants.ResetUnpostedLinesTaxDefault", "Reset Unposted lines Tax Default"); } }
			public static MultilingualString ImportAPInvoicesIssuedByOtherGroupCompanies { get { return ResString.GetMultilingualString("MenuItem.Constants.ImportAPInvoicesIssuedByOtherGroupCompanies", "Import AP Invoices issued by other Group Companies"); } }
			public static MultilingualString RedefaultJobBillingExchangeRate { get { return ResString.GetMultilingualString("MenuItem.Constants.RedefaultJobBillingExchangeRate", "Re-default Job Billing Exchange Rate"); } }
			public static MultilingualString PostAllSisterCompanyCharges { get { return ResString.GetMultilingualString("MenuItem.Constants.PostAllSisterCompanyCharges", "Post Charges for All Group Companies"); } }
			public static MultilingualString PostLocalSisterCompanyChargesOnly { get { return ResString.GetMultilingualString("MenuItem.Constants.PostLocalSisterCompanyChargesOnly", "Post Charges for Group Companies in My Login Country/Region"); } }
			public static MultilingualString PreviewCosts { get { return ResString.GetMultilingualString("MenuItem.Constants.PreviewCosts", "Preview Cost Confirmation Documents"); } }
			public static MultilingualString ResetValuesFromSubShipments { get { return ResString.GetMultilingualString("Forwarding.Shipment.Actions.ResetValuesFromSubShipments", "Reset Values From Sub Shipments"); } }
		}

		public static class MenuNameConstantsForPrinting
		{
			// THIS SHOULD NOT BE LOCALISED - these are used to find the correct StmMenuItem documents for printing
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "MenuNameConstantsForPrinting")]
			public static string ConsolJobProfitDocument { get { return "Consol Job Profit Document"; } }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "MenuNameConstantsForPrinting")]
			public static string QuotationPack { get { return "Quotation Pack"; } }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "MenuNameConstantsForPrinting")]
			public static string AgentPricingPage { get { return "Agent Pricing Page"; } }
		}

		public static class Workflow
		{
			public const string UndefinedTaskType = "UDF";
			public const string MilestoneType = "MIL";
			public const string ExceptionType = "EXC";
			public const string WorkflowTriggerType = "TRG";

			public static bool IsReservedTaskType(string code)
			{
				return ReservedTaskTypes.Contains(code);
			}

			public static IEnumerable<string> ReservedTaskTypes
			{
				get
				{
					return new[]
					{
						ExceptionType,
						MilestoneType,
						WorkflowTriggerType,
					};
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SectionRepositoryTemplateNames")]
		public static class SectionRepositoryTemplateNames
		{
			public const string System = "System Document Elements";
			public const string User = "Customized Document Elements";
		}

		public static class StmMenuItemTypes
		{
			public const string Documents = "DOC";
			public const string WebReports = "WEB";
			public const string OperationalActions = "ACT";
			public const string Forms = "FRM";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Business Context Prefixes")]
		public static class BusinessContextPrefixes
		{
			public const string Reports = "Rep";
		}

		public static class FindBoxMessages
		{
			public static MultilingualString NoneSelected { get { return ResString.GetMultilingualString("FindBoxMessages|NoneSelected", "{None Selected}"); } }
			public static MultilingualString InvalidSelection { get { return ResString.GetMultilingualString("FindBoxMessages|InvalidSelection", "{Invalid Selection}"); } }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Date time status description")]
		public static class DateTimeStatus
		{
			public const string Overdue = "Overdue";
			public const string Late = "Late";
			public const string OnTime = "On Time";
		}

		public static class TransportCoHotlinkOpener
		{
			public const string CargoWiseREF = "(*CargoWiseREF*)";
		}

		public static class ContainerDetentionDirection
		{
			public const string Import = "IMP";
			public const string Export = "EXP";
		}

		public static class ContainerDetentionDirectionDescription
		{
			public static MultilingualString Import { get { return ResString.GetMultilingualString("320335e8-082a-4363-8da1-d587d51fa25d", "Import"); } }
			public static MultilingualString Export { get { return ResString.GetMultilingualString("08ce362b-cf4b-4c0a-9aff-ed0c195da7a1", "Export"); } }
		}

		public static class ContainerDetentionFreeDayType
		{
			public const string CTOAvailable = "CTD";
			public const string FCLUnload = "FCD";
			public const string DayAfterFCLUnload = "FC1";
			public const string VesselArrival = "VSD";
			public const string CTOGateOut = "OUT";
			public const string WharfGateIn = "WGI";
			public const string FCLLoad = "FCL";
			public const string DayBeforeFCLLoad = "1FC";
			public const string VesselDeparture = "VED";
			public const string DayBeforeVesselDeparture = "1VE";
		}

		public static class ContainerDetentionPenaltyType
		{
			public const string DET = "DET";
			public const string STO = "STO";
			public const string MDD = "MDD";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constants")]
		public static class Mathematics
		{
			public const string Percentage = "%";
			public const string Decimals = "decimals";
		}

		public static class CostVarianceCalculationStyle
		{
			public const string LocalExTaxAmount = "AMT";
			public const string PercentageVariance = "PER";
			public const string PercentageVarianceAndMaximumLocalExTaxVariance = "PMX";
			public const string PercentageVarianceAndLocalCostAmountExTax = "PAA";
		}

		public static class CostVarianceComparisonOption
		{
			public const string Job = "JOB";
			public const string JobAndChargeCode = "JCH";
			public const string JobAndCreditor = "JCR";
			public const string ImportedChargeOrCreditor = "CJR";
			public const string ImportedChargeOrJob = "CJB";
			public const string ImportedChargeOrJobChargeCode = "CCH";
		}

		public static class StampDutyRechargeOrganizationType
		{
			public const string All = "ALL";
			public const string LocalOrganizations = "LOC";
			public const string NotRecharging = "NIL";
		}

		public static class StampDutyRechargeTransactionType
		{
			public const string All = "ALL";
			public const string ARInvoice = "INV";
		}

		public static class AuthorizationMode
		{
			public static class Codes
			{
				public const string Default = "DEF";
				public const string TwoApprovers = "TWO";
				public const string SequentialApprovers = "SEQ";
			}

			public static class Descriptions
			{
				public static MultilingualString Default { get { return ResString.GetMultilingualString("37CD0977-1C0F-4291-B687-9BFCF9C7D326", "Single Approval Required"); } }
				public static MultilingualString TwoApprovers { get { return ResString.GetMultilingualString("095924D0-9C10-4947-9557-ACEC522659EB", "Two Approvers Required"); } }
				public static MultilingualString SequentialApprovers { get { return ResString.GetMultilingualString("a004b4cd-1303-46b7-9541-9b81e199622b", "Sequential Approvals"); } }
			}
		}

		public static class TransactionCategory
		{
			public static class Codes
			{
				public const string Standard = "STD";
				public const string SelfBilling = "SBC";
				public const string ClaimRelated = "CLM";
				public const string WithholdingTax = "WHT";
				public const string TransactionNotFound = "TNF";
				public const string TransactionAlreadyPaid = "TAP";
				public const string Clearing = "CLR";
				public const string AutoJobRevenueJournal = "AJJ";
				public const string PaymentBasisWithholding = "PBW";
				public const string ClearingJournal = "CLJ";
				public const string InstalmentJournal = "INJ";
				public const string CashAdvanceInvoice = "API";
				public const string CashAdvanceReceived = "APR";
				public const string CashAdvancePaid = "APP";
				public const string UnrealizedExchangeGainLoss = "UNR";
				public const string RealizedExchangeGainLoss = "REA";
			}

			public static class Descriptions
			{
				public static MultilingualString Standard => ResString.GetMultilingualString("210fbe84-0360-4ab3-a689-75074225f0e7", "Standard");
				public static MultilingualString SelfBilling => ResString.GetMultilingualString("87876d91-c40b-4514-9f1f-6b48f73a3014", "Self Billing");
				public static MultilingualString ClaimRelated => ResString.GetMultilingualString("5bad9d72-8a83-4154-bfd4-6c0ce6323591", "Claim Related");
				public static MultilingualString WithholdingTax => ResString.GetMultilingualString("df8d6df1-a516-4405-9602-5a649f8e70c8", "Withholding Tax");
				public static MultilingualString TransactionNotFound => ResString.GetMultilingualString("c14c1522-40ef-4854-b69e-7b2c22e3c02d", "Transaction Not Found");
				public static MultilingualString TransactionAlreadyPaid => ResString.GetMultilingualString("9846e047-20ba-40f4-8d5b-d96afb959967", "Transaction Already Paid");
				public static MultilingualString Clearing => ResString.GetMultilingualString("3da306cd-3bb7-4977-9979-ba42b7582c6d", "Clearing");
				public static MultilingualString AutoJobRevenueJournal => ResString.GetMultilingualString("5f87875e-6c41-45a9-a7a0-5a6c67da65fa", "Automatic Job Revenue Journal");
				public static MultilingualString PaymentBasisWithholding => ResString.GetMultilingualString("C7E17C1E-8B9E-4408-85A9-8740CFD315C3", "Payment Basis Withholding");
				public static MultilingualString ClearingJournal => ResString.GetMultilingualString("7A6CFA55-891E-4C20-93FA-337EBA8A6104", "Clearing Journal");
				public static MultilingualString InstalmentJournal => ResString.GetMultilingualString("7DB4BEA0-9367-4B22-853C-572CF0DCA4C5", "Installment Journal");
				public static MultilingualString CashAdvanceReceived => ResString.GetMultilingualString("1f04a06a-805f-4dda-a469-7a005139b6fd", "Advance Payment Received");
				public static MultilingualString CashAdvancePaid => ResString.GetMultilingualString("97226dd1-3841-45b2-90d0-14eb451b3f36", "Advance Payment Paid");
				public static MultilingualString CashAdvanceInvoice => ResString.GetMultilingualString("3b9965d5-1881-49f4-9cdb-f9cdfe2d8128", "Advance Payment Invoice");
				public static MultilingualString UnrealizedExchangeGainLoss => ResString.GetMultilingualString("5B2C7EBD-1692-41F0-8E1A-15BAB5CA6106", "Unrealized Exchange Gain/Loss");
				public static MultilingualString RealizedExchangeGainLoss { get { return ResString.GetMultilingualString("InvoiceTypesList|RealizedExchangeGainLoss", "Realized Exchange Gain/Loss"); } }
			}
		}

		public static class OceanCarrierShipmentTypes
		{
			public static class Codes
			{
				public const string All = "";
				public const string BookingRequest = "BKG";
				public const string CarrierShipment = "SHP";
				public const string ShippingInstruction = "SIN";
				public const string BillOfLading = "BOL";
			}

			public static class Descriptions
			{
				public static MultilingualString All { get { return ResString.GetMultilingualString("49961f4c-be43-47d6-b2a1-66fbcd494b03", "All"); } }
				public static MultilingualString BookingRequest { get { return ResString.GetMultilingualString("efbcc917-79ac-4757-bcc0-541c3163da15", "Booking Request"); } }
				public static MultilingualString CarrierShipment { get { return ResString.GetMultilingualString("44d185aa-d8e2-46af-ba9e-7abe3a585602", "Carrier Shipment"); } }
				public static MultilingualString ShippingInstruction { get { return ResString.GetMultilingualString("b36f692b-ede4-4c04-9bb1-f76817b00fd0", "Shipping Instruction"); } }
				public static MultilingualString BillOfLading { get { return ResString.GetMultilingualString("fd757e1c-54e6-40b0-b129-09964cd5e298", "Bill of Lading"); } }
			}
		}

		public static class OceanCarrierCargoTypes
		{
			public static class Codes
			{
				public const string All = "";
				public const string BreakBulk = "BBK";
				public const string Container = "CNT";
				public const string RoRo = "ROR";
			}

			public static class Descriptions
			{
				public static MultilingualString All { get { return ResString.GetMultilingualString("f131e6cc-9b61-4e13-87ea-86ee0323835a", "All"); } }
				public static MultilingualString BreakBulk { get { return ResString.GetMultilingualString("336a2d0d9-e264-4507-be55-0ef749b1a3d8", "Break Bulk"); } }
				public static MultilingualString Container { get { return ResString.GetMultilingualString("bdbf6ad5-b2a2-48b8-aef3-efd0000bda5a", "Container"); } }
				public static MultilingualString RoRo { get { return ResString.GetMultilingualString("9901782d-f9ab-4bb7-9d39-811402edaafb", "RoRo"); } }
			}

			public static class SubType1Label
			{
				public static MultilingualString BreakBulk { get { return ResString.GetMultilingualString("29c76cb5-d124-4fb0-93f2-b143f93d4884", "Break Bulk Type"); } }
				public static MultilingualString Container { get { return ResString.GetMultilingualString("030e09d1-b83c-4d1d-8732-5d2338a49bb1", "Container Type"); } }
				public static MultilingualString RoRo { get { return ResString.GetMultilingualString("20e3bd5b-7546-4da4-b45b-24f6e0f4a06b", "RoRo Type"); } }
			}
		}

		public static class RoRoTypes
		{
			public static class Codes
			{
				public const string Bus = "BUS";
				public const string Car = "CAR";
				public const string HighHeavyVehicle = "HHV";
				public const string IncompleteVehicle = "ICV";
				public const string LowSpeedVehicle = "LSV";
				public const string MultiPurposePassengerVehicle = "MPV";
				public const string Motorcycle = "MCY";
				public const string OffRoadVehicle = "ORV";
				public const string SUV = "SUV";
				public const string SmallVan = "SVN";
				public const string Trailer = "TRA";
				public const string Truck = "TRU";
				public const string Van = "VAN";
			}

			public static class Descriptions
			{
				public static MultilingualString Bus { get { return ResString.GetMultilingualString("2144179d-8b6e-4639-861a-24475b072728", "Bus"); } }
				public static MultilingualString Car { get { return ResString.GetMultilingualString("700987ff-5352-4d54-a3a2-4302f42abaae", "Car"); } }
				public static MultilingualString HighHeavyVehicle { get { return ResString.GetMultilingualString("a0051f7b-1921-454a-bc56-41ecf48a55c0", "High & Heavy Vehicle"); } }
				public static MultilingualString IncompleteVehicle { get { return ResString.GetMultilingualString("192320f3-7cf8-40ff-94a4-63c789886c09", "Incomplete Vehicle"); } }
				public static MultilingualString LowSpeedVehicle { get { return ResString.GetMultilingualString("78f65e2c-5e42-4663-a90d-14df459f8910", "Low Speed Vehicle (LSV)"); } }
				public static MultilingualString MultiPurposePassengerVehicle { get { return ResString.GetMultilingualString("07a60115-d8e7-4a64-9383-f904d4c34667", "Multi-purpose Passenger Vehicle (MPV)"); } }
				public static MultilingualString Motorcycle { get { return ResString.GetMultilingualString("d23f5e08-47fd-4a04-abf6-53846192f5dd", "Motorcycle"); } }
				public static MultilingualString OffRoadVehicle { get { return ResString.GetMultilingualString("7509be89-4f50-4d43-abcd-d5b95e7b15c9", "Off Road Vehicle"); } }
				public static MultilingualString SUV { get { return ResString.GetMultilingualString("41de6b87-33b1-40d2-ad67-0c8126749cca", "SUV"); } }
				public static MultilingualString SmallVan { get { return ResString.GetMultilingualString("b0e1f4e6-17f0-4771-876c-3a98da266eb3", "Small Van"); } }
				public static MultilingualString Trailer { get { return ResString.GetMultilingualString("f3dc009a-1c8a-4576-83cb-e43f9386c8fe", "Trailer"); } }
				public static MultilingualString Truck { get { return ResString.GetMultilingualString("7cdc3863-5d19-47cf-892e-b9571a9d0854", "Truck"); } }
				public static MultilingualString Van { get { return ResString.GetMultilingualString("faed9e3b-ee90-4b1c-b108-fe70379e8bf9", "Van"); } }
			}
		}

		public static class EmailAddresses
		{
			public const string SupportRequestEmailAddress = "EnterpriseSupportRequest@edi.net.au";
			public const string EDI_FAX_GATEWAY = "Eml2Fax@eagletrac4.eagletrac.com";
			public const string EnterpriseUsageAnalysis = "EnterpriseUsageAnalysis@cargowise.com";
			public const string ErrorReportAddress = "EnterpriseIssues@edi.net.au";
			public const string CurrentVersionRecipientAddress = "EnterpriseVersionReports@edi.net.au";
			public const string DeliveredVersionRecipientAddress = "EnterpriseUpdateDeliveryReport@edi.net.au";
		}

		public static class AllowDuplicatePaymentReferenceHandlerMethods
		{
			public static class Codes
			{
				public const string AllowDuplicates = "ADP";
				public const string DisallowDuplicatesForEntireLedger = "DLG";
				public const string DisallowDuplicatesForLedgerAndTransactionType = "DLT";
			}

			public static class Descriptions
			{
				public static MultilingualString AllowDuplicates
				{
					get
					{
						return ResString.GetMultilingualString("f2b3e64b-5ca6-46f3-835d-7a8c6d5ce75b", "Allow Duplicates");
					}
				}

				public static MultilingualString DisallowDuplicatesForEntireLedger
				{
					get
					{
						return ResString.GetMultilingualString("073fa165-17fe-43d6-9980-655f71a9add0", "Disallow Duplicates for Entire Ledger");
					}
				}

				public static MultilingualString DisallowDuplicatesForLedgerAndTransactionType
				{
					get
					{
						return ResString.GetMultilingualString("416e244a-54ee-4633-9911-526ce64105a2", "Disallow Duplicates by Transaction Type for Entire Ledger");
					}
				}
			}
		}

		public static class IncludeBillingInfoInXMLMethod
		{
			public static class Codes
			{
				public const string NotInclude = "NON";
				public const string All = "ALL";
				public const string PrincipalOnly = "PPO";
			}

			public static class Descriptions
			{
				public static MultilingualString NotInclude { get { return ResString.GetMultilingualString("4c12d406-2acb-4daa-a43d-70ba7b9ef2c2", "Do not include billing info"); } }
				public static MultilingualString All { get { return ResString.GetMultilingualString("6849893c-1ac5-4a9b-a6c7-427e2516c211", "Include billing info with all charges"); } }
				public static MultilingualString PrincipalOnly { get { return ResString.GetMultilingualString("c88623cd-2b6c-46d0-a81e-94a97d227e2f", "Include billing info with only principal charges"); } }
			}
		}

		public static class ImportBillingInfoFromXMLMethod
		{
			public static class Codes
			{
				public const string Never = "NON";
				public const string NewOnly = "NEW";
				public const string Always = "ALL";
			}

			public static class Descriptions
			{
				public static MultilingualString Never { get { return ResString.GetMultilingualString("d43b3829-0aff-4501-92bc-3a146e6bd4d9", "Do not import billing info."); } }
				public static MultilingualString NewOnly { get { return ResString.GetMultilingualString("ea102927-0df0-498f-a876-db1217d59547", "Only import billing info for new bookings and bills."); } }
				public static MultilingualString Always { get { return ResString.GetMultilingualString("c280b21e-6df9-492b-8161-90287cc9fb2d", "Import or update billing info."); } }
			}
		}

		public static class ServiceTask
		{
			public const string ParentTableCode = "SH";
		}

		public static class ArchiveManager
		{
			public const string ParentTableCode = "AC";
		}

		public static class ReportSchedule
		{
			public const string ParentTableCode = "SU";
		}

		public static class ScheduleUniversalCopyTask
		{
			public const string ParentTableCode = "SUC";
		}

		public static class Recruiter
		{
			public const string LearningCentreCampaignType = "LCT";
		}

		public static class RoundingRules
		{
			public static class Codes
			{
				public const string None = "NON";
				public const string JapanYen = "JPY";
				public const string JapanYenWithCharge = "JPX";
			}

			public static class Descriptions
			{
				public static MultilingualString None { get { return ResString.GetMultilingualString("7EC505FB-9706-437F-9094-EDF35FF03B67", "No Rounding"); } }
				public static MultilingualString JapanYen { get { return ResString.GetMultilingualString("14A64E80-8981-4EFA-9956-BC5FBBA9D38E", "Japan up to the next JPY10, round existing charge"); } }
				public static MultilingualString JapanYenWithCharge { get { return ResString.GetMultilingualString("6C30B68D-DA40-492B-82CB-11D3B1F80FED", "Japan up to the next JPY10 with additional charge"); } }
			}
		}

		#region Customer Service

		public static class CustomerService
		{
			public static class CriticalityCodes
			{
				public const string CR1_SystemDown = "CR1";
				public const string CR2_ModuleDown = "CR2";
				public const string CR3_SingleFunctionNoWorkAround = "CR3";
				public const string CR4_SingleFunctionWithWorkAround = "CR4";
				public const string CR5_Training = "CR5";
				public const string CR6_FeatureRequest = "CR6";
				public const string CR7_CustomisationRequest = "CR7";
				public const string CR8_ComplianceRequirement = "CR8";
				public const string CR9_CustomerServiceRequest = "CR9";
			}
		}

		#endregion

		#region Screening Tracking Event

		public static class ScreeningMatchConfidenceRating
		{
			public const string High = "HIG";
			public const string Medium = "MED";
		}

		#endregion

		#region Screening Type

		public static class ScreeningType
		{
			public const string Manual = "MAN";
			public const string Silent = "SIL";
			public const string RescreenAdvice = "RSA";
			public const string Resynchronize = "SYNC";
			public const string ExternallySet = "EXT";
		}

		#endregion

		#region Accounting Web Service

		public static class BalanceOverdueAgingOption
		{
			public const string Balance = "BAL";
			public const string RecognisedAndUnrecognisedRevenue = "REV";
			public const string Overdue = "OVR";
			public const string Aging = "AGE";
		}

		public enum BalanceOverdueAgingOptionEnum { Unknown = -1, Balance, RecognisedAndUnrecognisedRevenue, Overdue, Aging }

		#endregion

		#region Credit Limit Checking

		public static class CreditLimitChecking
		{
			public const string Posted = "PST";
			public const string PostedAndRecognized = "REC";
			public const string PostedRecognizedAndUnrecognized = "ALL";
		}

		#endregion

		#region GenExportBatchSequence

		public static class GenExportBatchSequenceSubSystem
		{
			public const string Accounting = "ACC";
		}

		public static class DataExportBatchTypes
		{
			public static class Codes
			{
				public const string LegacyTransactionXML = "XLT";
				public const string UniversalTransactionXML = "XUT";
				public const string GLTransactionCSV = "GLT";
				public const string ExportChequePayments = "ECP";
				public const string PositivePay = "PPB";
				public const string GLConsolidations = "GLC";
			}

			public static class Descriptions
			{
				public static MultilingualString LegacyTransactionXML { get { return ResString.GetMultilingualString("A84BCA30-AB4C-4F37-A673-B52D9069A63A", "Legacy Transaction XML"); } }
				public static MultilingualString UniversalTransactionXML { get { return ResString.GetMultilingualString("F7FC563F-1877-49D2-84ED-60FC2C1CD1E5", "Universal Transaction XML"); } }
				public static MultilingualString GLTransactionCSV { get { return ResString.GetMultilingualString("E863A2BA-D799-49A9-84ED-024DC6041A05", "GL Transaction CSV"); } }
				public static MultilingualString ExportChequePayments { get { return ResString.GetMultilingualString("4ACA15BF-BABB-424F-83B1-9913C06115B1", "Export Cheque Payments"); } }
				public static MultilingualString PositivePay { get { return ResString.GetMultilingualString("F1076C9A-4385-416F-AC5B-ED4EA7D37568", "Positive Pay"); } }
				public static MultilingualString GLConsolidations { get { return ResString.GetMultilingualString("7009AF57-7A08-48F7-9BBD-00B0924E8249", "GL Consolidations"); } }
			}
		}

		public static class DataExportBatchSubTypes
		{
			public static class Codes
			{
				public const string GeneralLedgerPost = "GPS";
				public const string GeneralLedgerReverse = "GRV";
				public const string PositivePayFile = "PPF";
				public const string AccountingTransactionExportWebServicePost = "APS";
				public const string AccountingTransactionExportWebServiceReverse = "ARV";
				public const string AccountingTransactionHeaderExport = "HEX";
				public const string AccountingTransactionPostLineExport = "LEX";
				public const string AccountingTransactionReverseLineExport = "LRX";
				public const string GLConsolidationsEliminationRequiredPosting = "ERP";
				public const string GLConsolidationsEliminationRequiredReversal = "ERR";
				public const string GLConsolidationsEliminationNotRequiredPosting = "ENP";
				public const string GLConsolidationsEliminationNotRequiredReversal = "ENR";
			}

			public static class Descriptions
			{
				public static MultilingualString TransactionPosted { get { return ResString.GetMultilingualString("2FD2F018-BF76-48B3-B940-F87B71E26D01", "Transaction Posted"); } }
				public static MultilingualString TransactionReversed { get { return ResString.GetMultilingualString("3037B543-2136-4F99-8485-B074752B7671", "Transaction Reversed"); } }
				public static MultilingualString TransactionRecognizedReversal { get { return ResString.GetMultilingualString("B93D59BD-E204-4A5B-A024-BF4044CCC280", "Transaction Recognized/Reversed"); } }
				public static MultilingualString Payment { get { return ResString.GetMultilingualString("98964880-60D5-4540-B0BC-D5566213107E", "Payment"); } }
				public static MultilingualString EliminationRequiredPosting { get { return ResString.GetMultilingualString("86FB71F6-85CC-4DD0-A31E-9C4D2722BCD5", "Elimination Required Posting"); } }
				public static MultilingualString EliminationRequiredReversal { get { return ResString.GetMultilingualString("226AF336-1C35-48C1-9344-115BA6B39FE9", "Elimination Required Reversal"); } }
				public static MultilingualString EliminationNotRequiredPosting { get { return ResString.GetMultilingualString("341D105F-84D7-4B10-BBAD-9BB966D51C9F", "Elimination Not Required Posting "); } }
				public static MultilingualString EliminationNotRequiredReversal { get { return ResString.GetMultilingualString("873B4D8E-CC68-46EB-B891-F1B73A473123", "Elimination Not Required Reversal"); } }
			}
		}

		#endregion

		#region GenApprovalRequest

		public static class GenApprovalRequestSubSystem
		{
			public const string Accounting = "ACC";
		}

		public static class GenApprovalRequestApprovalType
		{
			public const string ARCreditNote = "RCN";
			public const string ARCreditControlledDocuments = "RCD";
			public const string APInvoiceCharges = "PIC";
			public const string GLJournal = "GLJ";
			public const string TransactionPendingAllocation = "TPA";
			public const string ARCreditNoteForReversal = "RIR";
		}

		public static class PaymentsRejectionReason
		{
			public static class Codes
			{
				public const string InvoiceChargesDisputed = "DIS";
				public const string InsufficientFunds = "INS";
				public const string IncorrectPaymentDetails = "INC";
				public const string IncorrectPaymentAllocation = "INA";
			}

			public static class Descriptions
			{
				public static MultilingualString InvoiceChargesDisputed { get { return ResString.GetMultilingualString("699EA504-61B0-4B87-9F72-7C9D22402978", "Invoice Charges Disputed"); } }
				public static MultilingualString InsufficientFunds { get { return ResString.GetMultilingualString("40816470-72FD-44EC-BA28-848E89190ACA", "Insufficient Funds"); } }
				public static MultilingualString IncorrectPaymentDetails { get { return ResString.GetMultilingualString("1135E795-2E69-4493-B9E1-BF28B95B9885", "Incorrect Payment Details"); } }
				public static MultilingualString IncorrectPaymentAllocation { get { return ResString.GetMultilingualString("121B7FBC-FF78-46B7-9DE5-E613FD1DD947", "Incorrect Payment Allocation"); } }
			}
		}

		public static class GenApprovalRequestReasonCode
		{
			public static class Code
			{
				public const string IncorrectOrganisationBilled = "IOB";
				public const string IncorrectRating = "IRA";
				public const string IncorrectCharges = "ICH";
				public const string IncorrectJobDetails = "IJD";
				public const string IncorrectDate = "IDA";
				public const string DamagedGoods = "DAM";
				public const string Discount = "DSC";
				public const string LateDelivery = "LDL";
			}

			public static class Description
			{
				public static MultilingualString IncorrectOrganisationBilled { get { return ResString.GetMultilingualString("3fc243c9-5e96-4c3c-a460-bf0db1b45654", "Incorrect Organization Billed"); } }
				public static MultilingualString IncorrectRating { get { return ResString.GetMultilingualString("ced844ab-e76a-4e73-a309-4274a690700c", "Incorrect Rating"); } }
				public static MultilingualString IncorrectCharges { get { return ResString.GetMultilingualString("0e7e213c-2333-4450-8312-4c622d71d4e9", "Incorrect Charges"); } }
				public static MultilingualString IncorrectJobDetails { get { return ResString.GetMultilingualString("bd96b362-9129-49c7-a49a-083bd57fb105", "Incorrect Job Details"); } }
				public static MultilingualString IncorrectDate { get { return ResString.GetMultilingualString("dd617d96-666e-4d1a-810e-0163785d15db", "Incorrect Date/s"); } }
				public static MultilingualString DamagedGoods { get { return ResString.GetMultilingualString("4bd0ae64-c70b-490d-be48-deebca9a80b0", "Damaged Goods"); } }
				public static MultilingualString Discount { get { return ResString.GetMultilingualString("76144320-6239-4331-8f12-6d26712495f8", "Discount"); } }
				public static MultilingualString LateDelivery { get { return ResString.GetMultilingualString("55c60649-5d23-4e03-a668-a247cff3dfc6", "Late Delivery"); } }
			}
		}

		public static class GenApprovalRequestApprovalStatus
		{
			public const string Requested = "REQ";
			public const string Cancelled = "CAN";
			public const string Rejected = "REJ";
			public const string Approved = "APP";
			public const string Posted = "PST";
			public const string Error = "ERR";
			public const string ApprovalRequested = "ARQ";
			public const string RejectionRequested = "RRQ";
		}

		#endregion

		#region AccPaymentBatch Status

		public static class AccPaymentBatchStatus
		{
			public const string Working = "WRK";
			public const string Completed = "CMP";
			public const string Cancelled = "CAN";
		}

		public static class AccPaymentBatchStatusDescription
		{
			public static MultilingualString Working { get { return ResString.GetMultilingualString("5176E6AD-4BED-4ACB-B7AD-C47D6AFD3538", "Working"); } }
			public static MultilingualString Completed { get { return ResString.GetMultilingualString("95BB6EAC-3384-4FEB-B950-EA8713D6908F", "Completed"); } }
			public static MultilingualString Cancelled { get { return ResString.GetMultilingualString("0FDE927A-4A5C-4BBF-B53C-B71D7C3F0F2C", "Canceled"); } }
		}

		#endregion

		#region AccDraftInvoiceHeader

		public static class AccDraftInvoiceHeaderStatus
		{
			public const string AwaitingApproval = "AWA";
			public const string Analyzing = "ANL";
			public const string ApprovedForPosting = "AFP";
			public const string Discarded = "DSC";
			public const string Draft = "DFT";
			public const string Processed = "PRS";
			public const string InReview = "RIV";
			public const string InDispute = "DSP";
		}

		#endregion

		#region Equipment Needed

		public static class EquipmentNeeded
		{
			public const string Ask = "ASK";
			public const string Any = "ANY";
		}

		#endregion

		public static class PeriodApportionmentMethods
		{
			public static class Codes
			{
				public const string Default = "DEF";
				public const string EquallyOverPeriods = "PER";
				public const string Manual = "MAN";
				public const string Day = "DAY";
			}

			public static class Descriptions
			{
				public static MultilingualString Default => ResString.GetMultilingualString("6c79a1a8-3eeb-48ab-a8ef-4690ab9065a2", "Recognize in a Post Period");
				public static MultilingualString EquallyOverPeriods => ResString.GetMultilingualString("c12c8e5f-1226-4d83-9ba9-99f39e8cbb6c", "Split Equally over Multiple Periods");
				public static MultilingualString Manual => ResString.GetMultilingualString("F7B336EE-FB82-494C-9076-62ACBCD67A7A", "Manually Enter Apportionment Amounts");
				public static MultilingualString Day => ResString.GetMultilingualString("53E677D0-EB6C-4DD6-9E63-557E772B9983", "Split based on Number of Days in Period");
			}
		}

		public static class ChargeCodeBranchDefaultingRule
		{
			public const string SpecificBranchAlways = "SBA";
			public const string ReceivingAgent = "RCA";
			public const string DeliveryAgentWithReceivingAgentFallback = "DLA";
			public const string SendingAgent = "SNA";
			public const string ArrivalCTO = "ACT";
			public const string DepartureCTO = "DCT";
			public const string ConsolArrivalLocalTransport = "ALT";
			public const string ConsolDepartureLocalTransport = "DLT";
			public const string ShipmentPickupLocalTransportCompany = "SPT";
			public const string ShipmentDeliveryLocalTransportCompany = "SDT";
			public const string ShipmentImportBroker = "IBK";
			public const string ShipmentExportBroker = "EBK";
		}

		public static class ChargeCreditorDefaultingRule
		{
			public const string DefaultingRule = "SCA";
		}

		public static class VehicleTransmissionType
		{
			public const string Automatic = "AUT";
			public const string Manual = "MAN";
		}

		public static class OrganisationTaxConfiguartionTypes
		{
			public static string NotApplicable = "NON";
			public static string Default = "DEF";
			public static string AccrualBasis = "ACR";
			public static string CashBasis = "CSH";
		}

		public static class OrganisationCreateComplianceDocumentOnPostingTypes
		{
			public const string NotApplicable = "NON";
			public const string RollupByCharge = "RCC";
			public const string PerComplianceDocumentNumber = "PCD";
			public const string NotRollup = "NRC";
		}

		public static class TransactionLineTaxBasisTypes
		{
			public static string Accrual = "A";
			public static string Cash = "C";
		}

		#region Autorating Mode

		public static class FreightRateAutoratingModes
		{
			public static class Code
			{
				public const string StandardRate = "STD";
				public const string FreightPlusRate = "FRT";
				public const string AllInRate = "AIN";
			}

			public static class Description
			{
				public static MultilingualString StandardRate { get { return ResString.GetMultilingualString("ef422b55-fc5e-48c5-9190-74f0c91566d2", "Use Standard Rates"); } }
				public static MultilingualString FreightPlusRate { get { return ResString.GetMultilingualString("ad119a6b-4a8d-4901-ad65-0e22babd1ecd", "Freight Plus Rate"); } }
				public static MultilingualString AllInRate { get { return ResString.GetMultilingualString("cdb2fdf5-835a-4fb2-8b3c-42dfa66eabb3", "All In Rate"); } }
			}
		}

		#endregion

		public static class SupplierBookingLineStatus
		{
			public static class Codes
			{
				public const string Incomplete = "INC";
				public const string Confirmed = "CON";
				public const string AcceptedAtOriginDepot = "ACC";
				public const string DepartedFromOriginDepot = "DEP";
				public const string ArrivedAtDestination = "ARV";
				public const string CustomsClearedAtDestination = "CVD";
				public const string AwaitingLocalDelivery = "RFL";
				public const string Delivered = "DLV";
			}

			public static class Descriptions
			{
				public static MultilingualString Incomplete { get { return ResString.GetMultilingualString("deefc512-564f-4ef5-92c5-bcbed464ebb5", "Incomplete"); } }
				public static MultilingualString Confirmed { get { return ResString.GetMultilingualString("593bef5c-22f2-4757-b938-f9e41abc943b", "Confirmed"); } }
				public static MultilingualString AcceptedAtOriginDepot { get { return ResString.GetMultilingualString("a4f0ce34-3142-40cc-9125-e210b65f176b", "Accepted At Origin Depot."); } }
				public static MultilingualString DepartedFromOriginDepot { get { return ResString.GetMultilingualString("a35c4e25-c84a-4482-bed5-5a13815d5b23", "Departed From Origin Depot."); } }
				public static MultilingualString ArrivedAtDestination { get { return ResString.GetMultilingualString("cd05829c-59eb-49e5-8086-fc81a7635bc8", "Arrived At Destination"); } }
				public static MultilingualString CustomsClearedAtDestination { get { return ResString.GetMultilingualString("cb11a1a3-e0f6-4163-9f9b-9ed39d5c16c5", "Customs Cleared At Destination"); } }
				public static MultilingualString AwaitingLocalDelivery { get { return ResString.GetMultilingualString("e9ed5788-f6fc-4c17-ad70-9c8e43dca38b", "Awaiting Local Delivery"); } }
				public static MultilingualString Delivered { get { return ResString.GetMultilingualString("3e652670-7fc9-475c-bb0d-bce60c10f9d8", "Delivered"); } }
			}
		}

		public sealed class TaxMessageMandatoryOptionConstants
		{
			public const string NotRequired = "NOT";
			public const string RequiredWhenTaxIsZero = "RTZ";
			public const string RequiredAlways = "REQ";
			public const string RequiredWhenExtraTaxIsNotZero = "RET";
			public const string RequiredWhenTaxHasExtraElementOrZeroTax = "REZ";
		}

		#region Glow

		public static class GLOWClientTypes
		{
			public static class Codes
			{
				public const string WebPortal = "WEB";
				public const string WindowsDesktopApplication = "WPF";
			}
		}

		public static class GLOWAuthenticationTypes
		{
			public static class Codes
			{
				public const string Staff = "STF";
				public const string Contact = "CON";
			}
		}

		public static class GlowFormFactors
		{
			public const string Desktop = "DSK";
			public const string Mobile = "MOB";
			public const string Tablet = "TAB";
		}

		#endregion

		#region GS1

		public static class GS1
		{
			public const int PrefixMinLength = 7;
			public const int PrefixMaxLength = 11;
		}

		#endregion

		#region LocationType

		public static class LocationTypes
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
			public static class Codes
			{
				public const string Port = "Port";
				public const string Country = "Country";
				public const string Zone = "Int. Zone";
				public const string IATARegion = "IATA Region";
			}

			public static class Descriptions
			{
				public static MultilingualString Port { get { return ResString.GetMultilingualString("8c5f579a-a549-49e2-bfe3-b6739919691a", "Port"); } }
				public static MultilingualString Country { get { return ResString.GetMultilingualString("658d7dbd-b750-497f-8550-d3831b52b307", "Country/Region"); } }
				public static MultilingualString Zone { get { return ResString.GetMultilingualString("be1ba84e-166d-4b6d-bdac-1a7829a88d8a", "International Zone"); } }
				public static MultilingualString IATARegion { get { return ResString.GetMultilingualString("16bf054a-82e7-4629-b391-0b49e8706143", "IATA Region"); } }
			}
		}

		#endregion

		#region DatabaseRecoveryModel

		public static class DatabaseRecoveryModel
		{
			public static class Codes
			{
				public const string Simple = "SIMPLE";
				public const string Full = "FULL";
			}

			public static class Descriptions
			{
				public static MultilingualString Simple { get { return ResString.GetMultilingualString("55547907-D35D-4113-8DA4-6C689F0A184D", "Simple recovery model"); } }
				public static MultilingualString Full { get { return ResString.GetMultilingualString("415C9B64-4F5B-4401-BED6-881E0E0C5E1D", "Full recovery model"); } }
			}
		}

		#endregion

		#region ChiefC88Options

		public static class ChiefC88Options
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
			public static class Code
			{
				public const string Plain = "Plain";
				public const string Rich = "Rich";
				public const string None = "None";
			}

			public static class Descriptions
			{
				public static MultilingualString Plain { get { return ResString.GetMultilingualString("12345678-1111-4113-8DA4-6C689F0A184D", "Print the simple version ('plain paper')."); } }
				public static MultilingualString Rich { get { return ResString.GetMultilingualString("12345678-2222-4113-8DA4-6C689F0A184D", "Print the rich version."); } }
				public static MultilingualString None { get { return ResString.GetMultilingualString("12345678-3333-4113-8DA4-6C689F0A184D", "Do not automatically print a C88."); } }
			}
		}

		#endregion

		#region CopyRecipient Type

		public static class CopyRecipientType
		{
			public const string EmailToRecipient = "TO";
			public const string CarbonCopyRecipient = "CC";
			public const string BlindCarbonCopyRecipient = "BCC";
		}

		#endregion

		#region Report Statistics Status

		public static class StmReportRunState
		{
			public const string Finished = "FIN";
			public const string Error = "ERR";
			public const string Stopped = "STP";
			public const string Running = "RUN";
			public const string Queued = "QUE";
			public const string Cancelled = "CAN";
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public static class DocumentDeliveryDefaultLanguagesFallbackType
		{
			public const string Contact = "Contact";
			public const string Address = "Address";
			public const string Organization = "Organization";
			public const string Branch = "Branch";
			public const string Company = "Company";
			public const string System = "System";
		}

		#region RateMode

		public static class RateMode
		{
			public static readonly ReadOnlyCollection<string> RateModes = new ReadOnlyCollection<string>(
				new[]
				{
					AIR, ULD, LSE, SEA, FCL, GRP, LCL, ROA, FRO, LRO, FTL, RAI, FRA, LRA, FWL, MAI, COU, OBC, UNA, BLK, BBK, ROR, BCN, SCN, ALL
				});

			public const string ALL = "ALL";

			public const string AIR = "AIR";
			public const string ULD = "ULD";
			public const string LSE = "LSE";

			public const string SEA = "SEA";
			public const string FCL = "FCL";
			public const string GRP = "GRP";
			public const string LCL = "LCL";

			public const string ROA = "ROA";
			public const string FRO = "FRO";
			public const string LRO = "LRO";
			public const string FTL = "FTL";

			public const string RAI = "RAI";
			public const string FRA = "FRA";
			public const string LRA = "LRA";
			public const string FWL = "FWL";

			public const string MAI = "MAI";
			public const string COU = "COU";

			public const string BLK = "BLK";
			public const string BBK = "BBK";
			public const string ROR = "ROR";
			public const string BCN = "BCN";
			public const string SCN = "SCN";

			public const string OBC = "OBC";
			public const string UNA = "UNA";
		}

		#endregion

		#region Gateway Debtor

		public static class GatewayDebtor
		{
			public static class Codes
			{
				public const string SendingAgent = "SGT";
				public const string ReceivingAgent = "RGT";
				public const string ShipmentPickupAgent = "SPA";
				public const string ShipmentDeliveryAgent = "SDA";
				public const string PreviousSendingAgent = "PSA";
			}

			public static class Descriptions
			{
				public static MultilingualString SendingAgent { get { return ResString.GetMultilingualString("D36CCAD6-E9EB-414A-8AFD-05FF6824E36F", "Sending Agent"); } }
				public static MultilingualString ReceivingAgent { get { return ResString.GetMultilingualString("C35C0F64-B270-4B3F-820A-4117934547D1", "Receiving Agent"); } }
				public static MultilingualString ShipmentPickupAgent { get { return ResString.GetMultilingualString("8029E770-382A-4296-8CAB-B55AD5F76DE4", "Shipment's Pickup Agent"); } }
				public static MultilingualString ShipmentDeliveryAgent { get { return ResString.GetMultilingualString("8DC0FE31-2A4B-4CD6-AD4B-B51B64BE601B", "Shipment's Delivery Agent"); } }
				public static MultilingualString PreviousSendingAgent { get { return ResString.GetMultilingualString("42E7E731-8B62-4C49-9AF1-5B9C0A52A3AF", "Previous Sending Agent"); } }
			}
		}

		#endregion

		#region Gateway Related Job

		public static class GatewayRelatedJob
		{
			public static class Codes
			{
				public const string All = "ALL";
				public const string RelatedToShipment = "SHP";
				public const string NotRelatedToJob = "NON";
			}

			public static class Descriptions
			{
				public static MultilingualString AllRelatedJob { get { return ResString.GetMultilingualString("416C0D22-FC53-4769-9253-D3EC17155E1A", "All Related Job"); } }
				public static MultilingualString RelatedToShipment { get { return ResString.GetMultilingualString("D4911F9B-888E-498D-8D83-2A05E40CB3B8", "Related to a Shipment"); } }
				public static MultilingualString NotRelatedToJob { get { return ResString.GetMultilingualString("5BFD40A2-CFCC-4134-B4BE-352E4D2A47F9", "Not Related to a Job"); } }
			}
		}

		#endregion

		#region GatewayAutoratingConfig

		public static class GatewayAutoratingRule
		{
			public static class Code
			{
				public const string AutoratingCost = "ICC";
				public const string AutoratingRevenue = "ICR";
				public const string StopAutoratingCost = "STC";
				public const string StopAutoratingCostFromICT = "SCI";
			}

			public static class Description
			{
				public static MultilingualString AutoratingCost { get { return ResString.GetMultilingualString("217523cc-e802-4c90-a9d5-6b8ef4c0453a", "Autorating Cost from ICT"); } }
				public static MultilingualString AutoratingRevenue { get { return ResString.GetMultilingualString("7ef2671a-ec12-4098-9575-000d0fe8eafb", "Autorating Revenue from ICT"); } }
				public static MultilingualString StopAutoratingCost { get { return ResString.GetMultilingualString("c5191fb0-e5be-4a83-acd1-886e96d18d00", "Stop Autorating Cost from Costing"); } }
				public static MultilingualString StopAutoratingCostFromICT { get { return ResString.GetMultilingualString("792ECC3B-9BC9-460F-81DE-82DC1006822D", "Stop Autorating Cost from ICT"); } }
			}
		}

		public static class GatewayLoginAgentRole
		{
			public static class Code
			{
				public const string NotGateway = "NGW";
				public const string Gateway = "GTW";
			}

			public static class Description
			{
				public static MultilingualString NotGateway { get { return ResString.GetMultilingualString("100f5f7c-45ef-47f5-98fa-14f8d117e7df", "Not Gateway"); } }
				public static MultilingualString Gateway { get { return ResString.GetMultilingualString("495688d9-b170-46a7-a17e-90ce8463af76", "Gateway"); } }
			}
		}

		public static class GatewayICTServiceProvider
		{
			public static class Code
			{
				public const string First = "FST";
				public const string CurrentGatewayOrganization = "CUR";
				public const string NextGatewayOrganization = "NXT";
				public const string NotAutorate = "NOT";
			}

			public static class Description
			{
				public static MultilingualString First { get { return ResString.GetMultilingualString("4c11157c-7753-4a23-93b3-a387fbfb8b2e", "First"); } }
				public static MultilingualString CurrentGatewayOrganization { get { return ResString.GetMultilingualString("ef72571b-32c6-4cc8-87e3-160d96147e38", "Current Gateway Organization"); } }
				public static MultilingualString NextGatewayOrganization { get { return ResString.GetMultilingualString("066e1a6a-4f7f-4ef0-8045-e4df860f6344", "Next Gateway Organization"); } }
				public static MultilingualString NotAutorate { get { return ResString.GetMultilingualString("9dcfeced-8f8e-413e-a6fc-65afd32af60a", "Do Not Autorate"); } }
			}
		}

		public static class GatewayPickupAgentStatus
		{
			public static class Code
			{
				public const string PickupAgent = "SPA";
				public const string NotPickupAgent = "NSP";
			}

			public static class Description
			{
				public static MultilingualString PickupAgent => ResString.GetMultilingualString("73A5D2C3-EF4D-43D0-9ED4-1D2F6EEA4BF5", "Pickup Agent");
				public static MultilingualString NotPickupAgent => ResString.GetMultilingualString("0BC163CF-7AE6-4B7D-8AE2-D944050F37E4", "Not Pickup Agent");
			}
		}

		#endregion

		#region Gateway Previous Sending Agent

		public static class GatewayPreviousSendingAgent
		{
			public static class Codes
			{
				public const string All = "ALL";
				public const string SendingAgent = "SGT";
				public const string NoPrevSendingAgent = "NON";
				public const string GatewayAgent = "GTA";
				public const string GatewayAgentWithFT = "GTT";
			}

			public static class Descriptions
			{
				public static MultilingualString All { get { return ResString.GetMultilingualString("CC6E865F-FF80-4B10-8A78-505919765011", "All"); } }
				public static MultilingualString SendingAgent { get { return ResString.GetMultilingualString("6CE4259D-CEA6-45B7-AB59-8762E9E9B8AA", "Sending Agent"); } }
				public static MultilingualString NoPrevSendingAgent { get { return ResString.GetMultilingualString("49AB0CCB-4ED9-401C-B008-BA3B4CD76DEE", "No Previous Sending Agent"); } }
				public static MultilingualString GatewayAgent { get { return ResString.GetMultilingualString("1B40D9AE-93D8-40E7-8038-73BA043B868E", "Gateway Agent"); } }
				public static MultilingualString GatewayAgentWithFT { get { return ResString.GetMultilingualString("BC908C08-3CF9-4E59-B2C7-7FCFC5D6EE2D", "Gateway Agent with Freight Tariff"); } }
			}
		}

		#endregion

		#region Gateway Agent Type

		public static class GatewayAgentType
		{
			public static class Codes
			{
				public const string SendingAgent = "SAG";
				public const string FirstSendingAgent = "FSG";
				public const string SucceedingSendingAgent = "SSG";
				public const string ReceivingAgent = "RAG";
				public const string ReceivingAgentForImport = "RIM";
			}

			public static class Descriptions
			{
				public static MultilingualString SendingAgent { get { return ResString.GetMultilingualString("c5152dfe-c93d-465b-b8db-38fb12fdda9e", "Sending Gateway Agent"); } }
				public static MultilingualString FirstSendingAgent { get { return ResString.GetMultilingualString("611624ab-0d59-4e63-932a-4c115d01fb0c", "(Obsolete) First Sending Gateway Agent"); } }
				public static MultilingualString SucceedingSendingAgent { get { return ResString.GetMultilingualString("5d644042-8ec8-42df-9cc8-a134f9760525", "(Obsolete) Succeeding Sending Gateway Agent"); } }
				public static MultilingualString ReceivingAgent { get { return ResString.GetMultilingualString("db22196e-191d-4032-8c48-0056d83b52e2", "Receiving Gateway Agent"); } }
				public static MultilingualString ReceivingAgentForImport { get { return ResString.GetMultilingualString("b44d5079-9ab8-48e2-907e-29b29c5316d0", "Receiving Gateway Agent For Import"); } }
			}

			public static bool IsSendingAgent(string gatewayAgentType) =>
				new[] { Codes.SendingAgent, Codes.FirstSendingAgent, Codes.SucceedingSendingAgent }.Contains(gatewayAgentType);

			public static bool IsReceivingAgent(string gatewayAgentType) =>
				new[] { Codes.ReceivingAgent, Codes.ReceivingAgentForImport }.Contains(gatewayAgentType);
		}

		#endregion

		#region Shipment Consolidation status

		public static class ShipmentConsolidationStatus
		{
			public static class Codes
			{
				public const string StandaloneShipment = "STS";
				public const string ConsolidatedShipment = "CNS";
			}

			public static class Descriptions
			{
				public static MultilingualString StandaloneShipment { get { return ResString.GetMultilingualString("FE151D00-EAA1-4E91-9581-C4F9D7CD42DB", "Standalone Shipment"); } }
				public static MultilingualString ConsolidatedShipment { get { return ResString.GetMultilingualString("AAA4C1CB-9231-42D4-A158-B7C32A1CE46E", "Consolidated Shipment"); } }
			}
		}

		#endregion

		#region PackageGrouping

		public static class PackageGrouping
		{
			public static class Codes
			{
				public const string DoNotGroup = "DNG";
				public const string GroupByShipment = "SHP";
				public const string GroupByPackLine = "PKL";
				public const string DefaultFromCarrier = "CAR";
			}

			public static class Description
			{
				public static MultilingualString DoNotGroup { get { return ResString.GetMultilingualString("e056859c-3da5-44d1-8c6c-ea5b205202d5", "Do not group"); } }
				public static MultilingualString GroupByShipment { get { return ResString.GetMultilingualString("c9c9d9e3-2d88-410c-86f7-078fe4a85f0a", "Group by Shipment"); } }
				public static MultilingualString GroupByPackLine { get { return ResString.GetMultilingualString("f6e1d991-a9e8-40d4-8968-45a534e312c1", "Group by Pack Line"); } }
				public static MultilingualString DefaultFromCarrier { get { return ResString.GetMultilingualString("02c962ad-9c87-4be7-b0b2-6dc54590cba4", "Default from Carrier"); } }
			}
		}

		#endregion

		#region Debtor Types

		public static class DebtorTypes
		{
			public static class Code
			{
				public const string DebtorGroup = "GRP";
				public const string DebtorOrganisation = "ORG";
			}

			public static class Description
			{
				public static MultilingualString DebtorGroup { get { return ResString.GetMultilingualString("720177EB-B388-47C9-B79A-065127115A97", "Debtor Groups"); } }
				public static MultilingualString DebtorOrganisation { get { return ResString.GetMultilingualString("F6FF5C2E-4F8D-4932-8F3D-22D4385FA3BB", "Debtors"); } }
			}
		}

		#endregion

		#region ContainerPenaltyPenaltyType

		public static class ContainerPenaltyPenaltyType
		{
			public static class Codes
			{
				public const string Detention = "DET";
				public const string MergedDemurrageAndDetention = "MDD";
				public const string Storage = "STO";
				public const string TruckWaitTime = "TWT";
			}

			public static class Descriptions
			{
				public static MultilingualString Detention { get { return ResString.GetMultilingualString("02a78ad1-7c48-4a4e-b446-24e1f26ef35f", "Detention"); } }
				public static MultilingualString MergedDemurrageAndDetention { get { return ResString.GetMultilingualString("d63b32f4-8fc8-432b-9b2f-61498bf3c115", "Merged Demurrage and Detention"); } }
				public static MultilingualString Storage { get { return ResString.GetMultilingualString("95ef5c8a-6f8e-456d-82ad-5981056a6286", "Storage"); } }
				public static MultilingualString TruckWaitTime { get { return ResString.GetMultilingualString("72d1fe29-c805-444b-87c8-12d0cdadb965", "Truck Wait Time"); } }
			}
		}

		#endregion

		#region ContainerPenaltyCreditorType

		public static class ContainerPenaltyCreditorType
		{
			public static class Codes
			{
				public const string Carrier = "CAR";
				public const string CTO = "CTO";
				public const string Transport = "TRS";
			}

			public static class Descriptions
			{
				public static MultilingualString Carrier { get { return ResString.GetMultilingualString("78df7d6c-ddd5-4849-b14d-a767b85ecb96", "Carrier"); } }
				public static MultilingualString CTO { get { return ResString.GetMultilingualString("2b3943ea-e7dc-4cc6-bb00-23ad94278319", "CTO"); } }
				public static MultilingualString Transport { get { return ResString.GetMultilingualString("cf19f22b-2d7e-4ee1-9fbb-c618e74b6e5a", "Transport"); } }
			}
		}

		#endregion

		#region ContainerPenaltyTimeUnit

		public static class ContainerPenaltyTimeUnit
		{
			public static class Codes
			{
				public const string Days = "D";
				public const string Hours = "H";
			}

			public static class Descriptions
			{
				public static MultilingualString Days { get { return ResString.GetMultilingualString("492ad439-7424-460a-9c9f-ccdbeb43bad4", "Days"); } }
				public static MultilingualString Hours { get { return ResString.GetMultilingualString("3a27a891-0718-4a08-b1bf-e694a93cdaed", "Hours"); } }
			}
		}

		#endregion

		#region ContainerPenaltyProcessType

		public static class ContainerPenaltyProcessType
		{
			public const string Export = "EXP";

			public const string Import = "IMP";

			public const string Pickup = "PIC";

			public const string Delivery = "DLV";
		}

		#endregion

		#region Dangerous Goods Country Reference

		public static class UNDGCountryReference
		{
			public static class Type
			{
				public const string PSA = "PSA";
				public const string ICPE = "ICPE";
			}

			public static class TypeDescription
			{
				public static MultilingualString PSA { get { return ResString.GetMultilingualString("FF9DAD92-4826-40F6-987F-5305071EC3BD", "PSA Group"); } }
				public static MultilingualString ICPE => ResString.GetMultilingualString("ae1fdb0b-e8c0-3c80-4470-c1498d6d3fc3", "ICPE Section Code");
			}
		}

		#endregion

		#region Ultimate Consignee Rule Types

		public static class UltimateConsigneeRuleTypes
		{
			public static class Codes
			{
				public const string NotRequired = "NON";
				public const string Mandatory = "MAN";
				public const string VerifyWithCarrierOrLocalAuthorities = "VER";
			}

			public static class Descriptions
			{
				public static MultilingualString NotRequired => ResString.GetMultilingualString("4aa8d7e2-c912-4824-a181-4eba75f30d7e", "Not Required");
				public static MultilingualString Mandatory => ResString.GetMultilingualString("e15d0df6-65fd-4e07-9bd2-ee6694f61ad3", "Mandatory");
				public static MultilingualString VerifyWithCarrierOrLocalAuthorities => ResString.GetMultilingualString("29821b96-d9a0-44f0-b7ea-2f8dae3f3c80", "Need to verify with carrier/local authorities");
			}
		}

		#endregion

		#region Tax Override

		public static class TaxOverrideTransactionContext
		{
			public static class Codes
			{
				public const string All = "ALL";
				public const string IntercompanyInvoiceImport = "INT";
				public const string Standard = "STD";
			}

			public static class Descriptions
			{
				public static MultilingualString All => ResString.GetMultilingualString("0DABCDC6-794C-4002-A450-09C443981634", "All Context");
				public static MultilingualString IntercompanyInvoiceImport => ResString.GetMultilingualString("B59DC409-9C00-4AA8-9FF7-FC7346B0D2F7", "Intercompany Invoice Import");
				public static MultilingualString Standard => ResString.GetMultilingualString("FD4A5C34-3AA1-4E56-B7A7-A946BE10808E", "Standard Context");
			}
		}

		public static class TaxOverrideDefaultingRule
		{
			public static class Codes
			{
				public const string CopyARAmount = "ART";
				public const string NotApplicable = "NON";
				public const string SumARAmount = "SUM";
			}

			public static class Descriptions
			{
				public static MultilingualString CopyARAmount => ResString.GetMultilingualString("5AE74661-9AAB-4BEC-BBB5-04811745FBB4", "Copy AR Transaction's Tax ID, Ex-Tax Amount, Tax Amount as AP");
				public static MultilingualString NotApplicable => ResString.GetMultilingualString("45F8BBC0-3E4B-486C-ABFC-E91D5596B948", "Not Applicable");
				public static MultilingualString SumARAmount => ResString.GetMultilingualString("229234A9-4290-40E0-BEE7-84290141BD92", "Sum AR Transaction's Ex-Tax Amount and Tax Amount as AP Transaction's Ex-Tax Amount");
			}
		}

		#endregion

		#region ZeroTax

		public static class ZeroTaxReferenceRateType
		{
			public const string Zero = "ZERO";
			public const string Empty = "EMPTY";
		}

		#endregion

		public static class TaxRelatedDocumentType
		{
			public const string ShippingInstruction = "ESI";
			public const string AirWayBill = "AWB";
			public const string HouseBill = "HBL";
		}

		#region FreightServiceType

		public static class FreightServiceType
		{
			public static class Codes
			{
				public const string Standard = "STD";
				public const string Fumigation = FreightServiceTypes.Codes.Fumigation;
				public const string QuarantineInspection = FreightServiceTypes.Codes.QuarantineInspection;
				public const string CustomsHold = FreightServiceTypes.Codes.CustomsHold;
				public const string QuarantineUnpack = FreightServiceTypes.Codes.QuarantineUnpack;
				public const string Tailgate = FreightServiceTypes.Codes.Tailgate;
				public const string ExtraInspection = FreightServiceTypes.Codes.ExtraInspection;
				public const string Survey = FreightServiceTypes.Codes.Survey;
				public const string Cleaning = FreightServiceTypes.Codes.Cleaning;
				public const string Washing = FreightServiceTypes.Codes.Washing;
				public const string SteamCleaning = FreightServiceTypes.Codes.SteamCleaning;
				public const string FCLContainerStorage = FreightServiceTypes.Codes.FCLContainerStorage;
				public const string FCLUnderbondStorage = FreightServiceTypes.Codes.FCLUnderbondStorage;
				public const string Overpack = FreightServiceTypes.Codes.Overpack;
				public const string ControlByCustoms = FreightServiceTypes.Codes.ControlByCustoms;
				public const string BreakDown = FreightServiceTypes.Codes.BreakDown;
			}

			public static class Descriptions
			{
				public static MultilingualString Fumigation => ResString.GetMultilingualString("b4e6be2b-4153-4452-860c-7b68f6e97b3e", FreightServiceTypes.Descriptions.Fumigation);
				public static MultilingualString QuarantineInspection => ResString.GetMultilingualString("46e36c57-7ed2-458e-8b95-9db3a61fd42b", FreightServiceTypes.Descriptions.QuarantineInspection);
				public static MultilingualString CustomsHold => ResString.GetMultilingualString("78dd67dd-7a47-48d3-aa6e-d4fbd4087c10", FreightServiceTypes.Descriptions.CustomsHold);
				public static MultilingualString QuarantineUnpack => ResString.GetMultilingualString("24d410dc-2987-4e54-8e7c-a379e4faa5a6", FreightServiceTypes.Descriptions.QuarantineUnpack);
				public static MultilingualString Tailgate => ResString.GetMultilingualString("e6e4e7d0-e33c-43e5-9c19-ee0e5986f6d1", FreightServiceTypes.Descriptions.Tailgate);
				public static MultilingualString ExtraInspection => ResString.GetMultilingualString("4a524ea7-b67e-4d63-a297-dc28506500e8", FreightServiceTypes.Descriptions.ExtraInspection);
				public static MultilingualString Survey => ResString.GetMultilingualString("3a0cda2d-943e-4958-acb3-5ebfc3c235f0", FreightServiceTypes.Descriptions.Survey);
				public static MultilingualString Cleaning => ResString.GetMultilingualString("f79eb4df-7b64-4319-9791-cc15455f0ec5", FreightServiceTypes.Descriptions.Cleaning);
				public static MultilingualString Washing => ResString.GetMultilingualString("9ee7dac2-c0fa-4c61-a841-9451bf9e4a01", FreightServiceTypes.Descriptions.Washing);
				public static MultilingualString SteamCleaning => ResString.GetMultilingualString("139dc964-eae9-4014-8089-79afcb246029", FreightServiceTypes.Descriptions.SteamCleaning);
				public static MultilingualString FCLContainerStorage => ResString.GetMultilingualString("14265cd3-7a2f-4b09-a312-4c32b459d9a6", FreightServiceTypes.Descriptions.FCLContainerStorage);
				public static MultilingualString FCLUnderbondStorage => ResString.GetMultilingualString("8ea4af0d-c791-4189-a1d4-ee55222d4df1", FreightServiceTypes.Descriptions.FCLUnderbondStorage);
				public static MultilingualString Overpack => ResString.GetMultilingualString("296f56d5-ba68-4e53-9257-7cb420dfd944", FreightServiceTypes.Descriptions.Overpack);
				public static MultilingualString ControlByCustoms => ResString.GetMultilingualString("f84425c0-d0ef-48db-a759-a44fbd84b312", FreightServiceTypes.Descriptions.ControlByCustoms);
				public static MultilingualString BreakDown => ResString.GetMultilingualString("c17d28f6-8e6a-44db-ac0d-d3ca4145578b", FreightServiceTypes.Descriptions.BreakDown);
			}
		}

		#endregion

		#region FacilityJobType

		public static class FacilityJobType
		{
			public static class Codes
			{
				public const string Cargo = "CGO";
				public const string Container = "CTR";
			}

			public static class Descriptions
			{
				public static MultilingualString Cargo { get { return ResString.GetMultilingualString("d041399f-5c07-4b5b-9530-9f195a795efc", "Cargo"); } }
				public static MultilingualString Container { get { return ResString.GetMultilingualString("0cafd938-9727-402a-b060-1081dad45c83", "Container"); } }
			}
		}

		#endregion

		#region Allocation Quantity Units

		public static class AllocationQuantityUnits
		{
			public const string Containers = "CN";
			public const string TwentyFootUnits = "TU";
		}

		#endregion

		#region SelectNDRPath

		public static class SelectNDRPath
		{
			public static class Codes
			{
				public const string MB = "MB";

				public const string CP = "CP";

				public const string SU = "SU";
			}

			public static class Descriptions
			{
				public static MultilingualString MB => ResString.GetMultilingualString("CC6C59CB-A737-4B41-8B38-31F32FF3D1F1", "Use the system email address");

				public static MultilingualString CP => ResString.GetMultilingualString("C6F8B06F-6542-40FF-A89A-ED626C106570", "Use the system email address if it is in the same domain as the sender's address");

				public static MultilingualString SU => ResString.GetMultilingualString("2F54AFB6-E8C8-444F-8783-73C3BCA0B446", "Use sender's email address");
			}
		}

		#endregion

		#region BillStatusUpdatedTypes

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "BillStatusUpdated EventReferenceParameterTypes")]
		public static class BillStatusUpdatedTypes
		{
			public const string OriginalBillPublished = "Original Bill Published";
			public const string OriginalBillSentForPublication = "Original Bill Sent for Publication";
			public const string OriginalBillTransferred = "Original Bill Transferred";
			public const string AmendmentRequested = "Amendment Requested";
			public const string AmendmentGranted = "Amendment Granted";
			public const string AmendmentDenied = "Amendment Denied";
			public const string AmendmentBillReceived = "Amendment Bill Received";
			public const string SwitchedToPaper = "Switched To Paper";
			public const string Surrendered = "Surrendered";
			public const string DenyAmendmentRejected = "Deny Amendment Rejected";
			public const string DenyAmendmentAccepted = "Deny Amendment Accepted";
			public const string OriginalBillNotPublished = "Original Bill not Published";
		}

		#endregion

		#region BillOfLadingBillType

		public static class BillOfLadingBillType
		{
			public static class Codes
			{
				public const string Straight = "STR";
				public const string ToOrder = "TOR";
				public const string BlankEndorse = "BLE";
			}

			public static class Descriptions
			{
				public static MultilingualString Straight => ResString.GetMultilingualString("1abdea36-51ca-4cf2-93ac-6cda19d04106", "Straight");
				public static MultilingualString ToOrder => ResString.GetMultilingualString("c613fb98-b5f9-49cf-b799-17a031a7b8dc", "To Order");
				public static MultilingualString BlankEndorse => ResString.GetMultilingualString("1dde8f19-ecc0-4235-b21b-be496584b39e", "Blank Endorse");
			}
		}

		#endregion

		#region BillOfLadingBillTerms

		public static class BillOfLadingBillTerms
		{
			public static class Codes
			{
				public const string Transferable = "TRA";
				public const string NonTransferable = "NTR";
			}

			public static class Descriptions
			{
				public static MultilingualString Transferable => ResString.GetMultilingualString("a73e19fb-cb86-4809-a42d-0f8b6aba4dea", "Transferable");
				public static MultilingualString NonTransferable => ResString.GetMultilingualString("94dd29e3-f513-4e17-8cc3-92c0dbdeeff5", "Non-Transferable");
			}
		}

		#endregion

		#region Glow Indexer Strategy

		public static class IndexerStrategy
		{
			public const string CT = "CT";
			public const string CDC = "CDC";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
			public const string Audit = "Audit";
		}

		#endregion

		#region Data Purge

		public static class PurgeDataRunStatusFlagCodes
		{
			public const string Running = "RUN";
			public const string Completed = "CMP";
			public const string Error = "ERR";
		}

		#endregion

		#region Gate Management

		public static class GateManagementConstants
		{
			public static class DataSources
			{
				public const string VehicleBookingSystem = "VBS";
			}

			public static class BookingTypes
			{
				public const string Regular = "REG";
				public const string Bulk = "BLK";
				public const string AdHoc = "ADH";
				public const string Priority = "PRI";
				public const string Internal = "INT";
			}

			public static class TransportBookingDirections
			{
				public static class Codes
				{
					public const string Pickup = "PIC";
					public const string Delivery = "DLV";
				}

				public static class Descriptions
				{
					public static MultilingualString Pickup => ResString.GetMultilingualString("b9cc8acb-0c85-bda3-4ff3-1ce99963359b", "Pickup");
					public static MultilingualString Delivery => ResString.GetMultilingualString("a483ab75-5cc7-5db2-4246-80e851f55100", "Delivery");
				}
			}

			public static class ReferenceTypes
			{
				public static class Codes
				{
					public const string MovementBookingNumber = "MBN";
				}
				public static class Descriptions
				{
					public static MultilingualString MovementBookingNumber => ResString.GetMultilingualString("90877325-6eeb-47ed-8efe-18c097bff66b", "Movement Booking Number");
				}
			}

			public static class FacilityJobTablePrefix
			{
				public const string CYDPickup = "YPL";
				public const string CYDDelivery = "YDL";
				public const string WhsItemReceiveTransportationUnit = "WRH";
				public const string WhsItemDispatchTransportationUnit = "WDH";
			}

			public static class FacilityValidationTypes
			{
				public const string GateIn = "gateIn";
			}
		}

		#endregion

		#region Container Yard

		public static class ContainerYardConstants
		{
			public static class YardUnitType
			{
				public static class Codes
				{
					public const string CNT = "CNT";
					public const string CHS = "CHS";
					public const string GEN = "GEN";
					public const string BLK = "BLK";
				}

				public static class Descriptions
				{
					public static MultilingualString CNT => ResString.GetMultilingualString("D606854F-1BB0-4F41-8E01-03301B57D8A5", "Container");
					public static MultilingualString CHS => ResString.GetMultilingualString("BF2A1FD8-772F-4D6C-BD15-96DAC15DC1DB", "Chassis");
					public static MultilingualString GEN => ResString.GetMultilingualString("295DDA9A-3C36-4D2C-AAC1-E340E3556CD2", "Genset");
					public static MultilingualString BLK => ResString.GetMultilingualString("7771FC3E-D76B-4C05-856B-C9D55E62EF6B", "Bulk Cargo");
				}
			}

			public static class YardUnitLoad
			{
				public static class Codes
				{
					public const string EMP = "EMP";
					public const string LAD = "LAD";
				}

				public static class Descriptions
				{
					public static MultilingualString EMP => ResString.GetMultilingualString("A5F77E6D-2DD4-4086-9E6B-523E6C74CD89", "Empty");
					public static MultilingualString LAD => ResString.GetMultilingualString("6A2D178D-BC81-459C-866A-93B8D60AB427", "Laden");
				}
			}

			public static class MaintenanceAndRepair
			{
				public static class ResponsibleParty
				{
					public const string Owner = "OWN";
					public const string Lessee = "LES";
					public const string Insurer = "INS";
					public const string ThirdParty = "THD";
				}
			}
		}

		#endregion

		#region Transit Warehouse

		public static class TransitWarehouseTransportUnitTypes
		{
			public const string ULD = "ULD";
			public const string Container = "CNT";
			public const string Vehicle = "VEH";
		}

		#endregion
	}

	#region Messaging Constants

	public static class MessagingConstants
	{
		public static string eRouterACSServerName { get { return "acsedi.edi.net.au"; } }

		public static string eRouterMYCEmailAddress { get { return "mycedi@edi.net.au"; } }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It's an email address")]
		public static string eRouterPRAEmailAddress { get { return "1-stoppra@edi.net.au"; } }

		public static class SupportEmail
		{
			public const string Global = "support@cargowise.com";
			public const string Singapore = "sg.support@edi.com.au";
		}
	}

	#endregion

	#region Country Guids

	public class CountryGuids
	{
		CountryGuids()
		{
		}

		public static CountryGuids Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CountryGuids();
				}
				return instance;
			}
		}
		[ThreadStatic]
		static CountryGuids instance;

		public Guid _TemplateCountryName_ = new Guid("B6661A73-2BD8-4DE7-ACE7-35CA577842F0");   // Anguilla
		public Guid _EUTemplateCountryName_ = new Guid("0DB1E4B0-CC61-4DFD-B941-DD25337DC7BF");   // Malta

		// This list was generated by the following mssql query:s
		// select 'public readonly Guid ' + replace(replace(replace(replace(replace(replace(replace(RN_Desc, ' ', ''), ',', ''), '.', ''), '-', ''), '''', ''), '(', ''), ')', '')  + ' = new Guid("' + convert(varchar(36), RN_PK) + '");' from dbo.RefCountry where RN_IsSystem = 'Y' order by RN_Desc;
		public readonly Guid Afghanistan = new Guid("D91DDFCB-614A-4DE0-B296-99DC6B7923B7");
		public readonly Guid Albania = new Guid("95663F91-CF79-4E20-951C-EE3C04443E8A");
		public readonly Guid Algeria = new Guid("96E31278-017F-40EC-821D-5086C2AF9E47");
		public readonly Guid AmericanSamoa = new Guid("6C71ED1A-01AC-434A-B8AB-DB2D18FCF5B5");
		public readonly Guid Andorra = new Guid("4A039A4C-DCF4-472A-872D-CA545B12AC79");
		public readonly Guid Angola = new Guid("2E398146-60B1-4FBB-89A1-28E57A1D3A80");
		public readonly Guid Anguilla = new Guid("B6661A73-2BD8-4DE7-ACE7-35CA577842F0");
		public readonly Guid Antarctica = new Guid("2DB9F51C-9F46-48A9-A19C-617923667006");
		public readonly Guid AntiguaandBarbuda = new Guid("49974FD8-5423-4273-90C8-0EDF7778B075");
		public readonly Guid Argentina = new Guid("851096F7-03A5-45DB-A62A-E16712FAA936");
		public readonly Guid Armenia = new Guid("B260121D-508A-4804-8EF7-EED9866F32F8");
		public readonly Guid Aruba = new Guid("8FA6AEAC-504D-4308-A199-950E95B32F7F");
		public readonly Guid Australia = new Guid("E4EB6E97-AA78-4C46-BF86-35B7FBB5FB0E");
		public readonly Guid Austria = new Guid("6B7A6DAA-39BF-44F9-BB12-583B0F4A5078");
		public readonly Guid Azerbaijan = new Guid("60BB064A-43F2-426E-8EAF-314AB9DFCE84");
		public readonly Guid Bahamas = new Guid("2DFDB7F1-463B-4A49-93A9-EC7B7634CC71");
		public readonly Guid Bahrain = new Guid("162EAA1E-0753-4841-B1C8-62A42883890E");
		public readonly Guid Bangladesh = new Guid("F602B117-1C52-4500-B52C-47C5F2BA0B92");
		public readonly Guid Barbados = new Guid("634D0E48-23FC-4537-85E9-088D34174E5D");
		public readonly Guid Belarus = new Guid("E638645E-3CAF-4116-8555-701754874914");
		public readonly Guid Belgium = new Guid("A8AD6185-407A-4BA8-BCA4-07FBD62ED80D");
		public readonly Guid Belize = new Guid("459D8CB9-D0BF-44FD-A323-326FB1449AB9");
		public readonly Guid Benin = new Guid("0F1B83FA-B0C3-43A1-9829-ACEC31C22AB9");
		public readonly Guid Bermuda = new Guid("8F52E33D-B209-4EEE-B4DF-C6A445186659");
		public readonly Guid Bhutan = new Guid("D19F64AC-AF2D-4A1E-AD4B-2CBD781C5ACC");
		public readonly Guid Bolivia = new Guid("80E2A68B-5FF8-4E0E-9104-1DAE436302EC");
		public readonly Guid BosniaandHerzegovina = new Guid("6686A127-50C5-49E1-9224-5B8AC308B411");
		public readonly Guid Botswana = new Guid("2091470C-E9EF-4508-BCD6-09CC21717A9C");
		public readonly Guid BouvetIsland = new Guid("D296220C-42D1-43DD-AD1B-3993FC52A599");
		public readonly Guid Brazil = new Guid("BC04FCCA-C89F-44C9-A100-1D3715EEE717");
		public readonly Guid BritishIndianOceanTerritory = new Guid("33C2FFD3-4046-4BC9-8A82-88DD2EAC1FBE");
		public readonly Guid BruneiDarussalam = new Guid("988C9BF2-E3DC-4BBC-A004-3FB97F5D951A");
		public readonly Guid Bulgaria = new Guid("F7198328-0A00-479B-8D60-8520B1F8FD00");
		public readonly Guid BurkinaFaso = new Guid("5308D896-CECB-4FE9-9E86-CDF68040174A");
		public readonly Guid Burundi = new Guid("07534C0A-0B1D-4DEE-95ED-638E0739F90C");
		public readonly Guid Cambodia = new Guid("6AA9F3ED-1415-43D6-B615-249E248BCCAB");
		public readonly Guid Cameroon = new Guid("7BC5F55A-9F38-46AB-8B24-AB740C1FF8BD");
		public readonly Guid Canada = new Guid("B1F02B9D-30A7-4BD3-B946-2ECC0BCE2A81");
		public readonly Guid CapeVerde = new Guid("DE396926-1B64-40A4-B31C-D72FA7EA0903");
		public readonly Guid CaymanIslands = new Guid("E0CA00AD-FDFB-4CFD-A988-FDA00A6A084F");
		public readonly Guid CentralAfricanRepublic = new Guid("28D8F26D-3C9C-407D-857E-2A1EDCB1773B");
		public readonly Guid Chad = new Guid("FE1270CD-7815-4E89-BB25-9CD1F84FF41B");
		public readonly Guid Chile = new Guid("17786C50-3873-482F-8F19-81D83E548127");
		public readonly Guid China = new Guid("21EAA7E3-9009-4E17-9D96-F72D36A93F86");
		public readonly Guid ChristmasIsland = new Guid("2AA6ACD8-8B8B-4D2A-9C6C-B9A588C1A7D1");
		public readonly Guid CocosKeelingIslands = new Guid("AF0AF4D2-A5B7-41C8-AFB4-8855286CE5E3");
		public readonly Guid Colombia = new Guid("6BE66B57-EB7C-4B42-BA46-C6AFFD4EEC1D");
		public readonly Guid Comoros = new Guid("B6662DB9-6C32-4D9F-9C29-A3F46AFBE11D");
		public readonly Guid Congo = new Guid("22378217-F3D6-428B-96C6-B04179207E75");
		public readonly Guid CongoTheDemocraticRepublicoft = new Guid("BC48CE84-0CF5-4576-AAB5-6F8CE4B31A0F");
		public readonly Guid CookIslands = new Guid("B98208D8-C306-44D1-9AF1-9B6DD298E36E");
		public readonly Guid CostaRica = new Guid("E3ECA0AE-1A3F-4429-AA24-1ACAAE6E38BE");
		public readonly Guid CotedIvoire = new Guid("22D0BFD2-7D5F-4286-A047-0A6975A828CB");
		public readonly Guid Croatia = new Guid("DB0F9D6A-A1AC-4FCA-8BF8-558913B904A3");
		public readonly Guid Cuba = new Guid("3492B956-D799-4788-AE7F-DD62B1FC1595");
		public readonly Guid Cyprus = new Guid("60E7F9E0-09A8-459D-9E11-257665EF6FA1");
		public readonly Guid CzechRepublic = new Guid("AF862C3A-5A23-44A7-9F4D-598A0E26A63F");
		public readonly Guid Denmark = new Guid("0FA97477-279D-4305-839D-F147F2879BFB");
		public readonly Guid Djibouti = new Guid("3064259A-390F-4ED9-A317-CBF8CE180654");
		public readonly Guid Dominica = new Guid("D236FC3B-48FB-4264-A3F5-C4E4BDD0EF20");
		public readonly Guid DominicanRepublic = new Guid("98DF4F81-9F13-4A31-99A1-99C79361FDF7");
		public readonly Guid Ecuador = new Guid("C8CB6228-ED8A-40E0-AD7D-2BEB37703A9E");
		public readonly Guid Egypt = new Guid("4B4D95E3-B723-498E-B9E6-145F69DAA2D5");
		public readonly Guid ElSalvador = new Guid("A150FAC7-CB24-4BC2-9DF4-1EFB2E65DA80");
		public readonly Guid EquatorialGuinea = new Guid("FBA53240-0287-4688-8869-D249AC282B2E");
		public readonly Guid Eritrea = new Guid("22CB7CAA-074B-42C9-BBE0-9EACEEEE6195");
		public readonly Guid Estonia = new Guid("C5239E79-954F-4823-A2C9-CF7CF33D2C7A");
		public readonly Guid Ethiopia = new Guid("B77FD4A7-8B09-42C0-B0E7-20A9C744DE7A");
		public readonly Guid FalklandIslandsMalvinas = new Guid("27BEEE22-57B0-42D2-9462-B0B6E4A5B29E");
		public readonly Guid FaroeIslands = new Guid("1AFC57D7-7773-47DC-A024-4DAA3558E9BD");
		public readonly Guid Fiji = new Guid("ABE1A99B-19F2-4F32-93DC-C37AFB53D9A4");
		public readonly Guid Finland = new Guid("E18E36F5-47FC-42F5-A3AC-99589154C71E");
		public readonly Guid France = new Guid("5DBC6C4E-97C1-4477-A40F-07BB9F88EDA7");
		public readonly Guid FrenchGuiana = new Guid("84E07693-1B09-41B9-BD1B-A8D6F7207093");
		public readonly Guid FrenchPolynesia = new Guid("42AF3555-F0E4-42D3-B678-DF644D4846BC");
		public readonly Guid FrenchSouthernTerritories = new Guid("7A8B8173-E1FF-4DAE-BFC6-A873FF9EBA4C");
		public readonly Guid Gabon = new Guid("04CAA51C-F750-46AB-BC79-F12F9C37856D");
		public readonly Guid Gambia = new Guid("D96425C6-41BF-408A-A889-6CB934BC0432");
		public readonly Guid Georgia = new Guid("8C6BEC88-B86A-4F40-9241-D2E28F11B737");
		public readonly Guid Germany = new Guid("BA89AA92-C946-449A-A5BD-A156FF892692");
		public readonly Guid Ghana = new Guid("DCD05928-7BE0-47B8-811E-A55CECB35767");
		public readonly Guid Gibraltar = new Guid("F0A7B77F-4496-4FB7-A0B3-636893831859");
		public readonly Guid Greece = new Guid("C25BB533-49F2-4FFA-8AF9-B3AB034D4BA5");
		public readonly Guid Greenland = new Guid("2A2FFC83-C722-4C27-BF33-957D7417921A");
		public readonly Guid Grenada = new Guid("340FDC29-D30F-4C47-A078-7040536DE8E9");
		public readonly Guid Guadeloupe = new Guid("A88B98FE-78D0-4D2B-8263-6A719BB19296");
		public readonly Guid Guam = new Guid("4D0A974B-CE42-4935-88FD-2990C5A5A2CD");
		public readonly Guid Guatemala = new Guid("59F3CD5F-C2AC-4DDA-9BF8-20EAF7BB7FB8");
		public readonly Guid Guernsey = new Guid("C10C4DE3-D484-49F9-AF7C-905D8AC0E902");
		public readonly Guid Guinea = new Guid("4F899E5E-0B9F-4DD5-BBCA-44F44A1C9F62");
		public readonly Guid GuineaBissau = new Guid("2682FFFA-89AC-4D23-810C-0D552F1CB2D6");
		public readonly Guid Guyana = new Guid("2DFD468A-2170-41EE-8E8E-0A0DD36E36B0");
		public readonly Guid Haiti = new Guid("0DD1E291-1289-4D4F-BECF-8ABE409F9F93");
		public readonly Guid HeardIslandandMcDonaldIslands = new Guid("917D1E16-505D-437F-BEED-51EA3F36E4AC");
		public readonly Guid HolySeeVaticanCityState = new Guid("CCF0CD70-642A-4BA3-B504-25A107FA2E12");
		public readonly Guid Honduras = new Guid("6F71304B-6940-4FE2-AFDD-C6407DE0B77E");
		public readonly Guid HongKong = new Guid("CBF5171B-810A-4C7F-A872-BE9B77020B41");
		public readonly Guid Hungary = new Guid("8AA38EBA-092E-4AF1-A235-F5E4E41E912D");
		public readonly Guid Iceland = new Guid("11100F37-D082-4BB5-A68F-5E38F8CDF9C3");
		public readonly Guid India = new Guid("CA332207-7115-4B01-A632-FFD9575EDD84");
		public readonly Guid Indonesia = new Guid("CBDDBD26-C634-4947-9BEB-8558CE852772");
		public readonly Guid InternationalWatersInstallations = new Guid("B3DAC456-A0E4-4B0E-A27A-1405B9496863");
		public readonly Guid IranIslamicRepublicof = new Guid("29D05528-AD20-4F31-A674-273B3EE8D9DF");
		public readonly Guid Iraq = new Guid("B16C8F49-1CF7-47EE-8B1A-6D123752C335");
		public readonly Guid Ireland = new Guid("B4ED9A41-0646-4F42-A9BC-BBA4568DCFDB");
		public readonly Guid IsleofMan = new Guid("BBEFCE9B-B54B-498A-902B-9957B65C18D8");
		public readonly Guid Israel = new Guid("CD8B8FDD-590F-45FB-B70E-CFA99864D98E");
		public readonly Guid Italy = new Guid("1B1B648C-2F9D-4DFE-8787-084C3FB124AA");
		public readonly Guid Jamaica = new Guid("85D7E14E-7E11-469E-9D38-55020C3699DE");
		public readonly Guid Japan = new Guid("F608D7D8-CEB3-4CA9-B81B-BBB9CDAC0B92");
		public readonly Guid Jersey = new Guid("83C7F6CD-9597-4D50-8ACF-9F7B6E461EB9");
		public readonly Guid Jordan = new Guid("0EFFAB44-75FC-4CF1-89BA-AF951B6A9A92");
		public readonly Guid Kazakhstan = new Guid("766E9B27-E763-46F6-A151-CA3748E4E55C");
		public readonly Guid Kenya = new Guid("66E3510F-0C41-4097-A30A-3A2B35F84F26");
		public readonly Guid Kiribati = new Guid("92D98E25-FBDD-41D9-822B-635C443EA397");
		public readonly Guid KoreaDemocraticPeoplesRepublic = new Guid("0304B4ED-BF28-4D66-A9D6-DBAAAEAEE0B3");
		public readonly Guid KoreaRepublicof = new Guid("D6453FDF-0B7E-43B7-B306-AD8B2BD600FE");
		public readonly Guid Kuwait = new Guid("1B791679-D25D-4DB4-91AD-1F0956BECA48");
		public readonly Guid Kyrgyzstan = new Guid("60556BEB-317A-452A-9033-36E125F444C8");
		public readonly Guid LaoPeoplesDemocraticRepublic = new Guid("FCA7E7E0-D6F5-46E0-BC3A-5CA95D45673D");
		public readonly Guid Latvia = new Guid("88845F98-6282-4660-9F72-BB3A41D82995");
		public readonly Guid Lebanon = new Guid("AF47D8B2-9CAF-475A-9AE8-3D54DAEF59AC");
		public readonly Guid Lesotho = new Guid("B637B960-5536-44B9-BAA4-F4F1E57F0B81");
		public readonly Guid Liberia = new Guid("7A34264A-D9DD-4BFB-BD90-B7D6E388D061");
		public readonly Guid LibyanArabJamahiriya = new Guid("F7E8B6C3-2E23-4613-9567-54DEA4AD0CDA");
		public readonly Guid Liechtenstein = new Guid("2C973EC5-5E19-4866-893E-7407DAABBD89");
		public readonly Guid Lithuania = new Guid("5BA3558C-D416-44F6-BAC2-323582463CEE");
		public readonly Guid Luxembourg = new Guid("64F8D2AB-E549-4952-962C-08FDBF61B203");
		public readonly Guid Macao = new Guid("7685A146-5833-4068-BE2E-92882713016A");
		public readonly Guid MacedoniaTheformerYugoslavRepu = new Guid("D51751A6-B7FC-45FB-89E6-6A9084E7F253");
		public readonly Guid Madagascar = new Guid("2299E531-1E5D-4C41-838F-9D268490C499");
		public readonly Guid Malawi = new Guid("5E8013D6-3357-42AF-80C5-455A981AB150");
		public readonly Guid Malaysia = new Guid("C09DFF15-30CE-45A2-91CB-E1E85B365828");
		public readonly Guid Maldives = new Guid("76CAB63D-7B54-424E-A63C-932864605E08");
		public readonly Guid Mali = new Guid("B7024A08-D6B5-4F55-9AA8-6B2CE5862B10");
		public readonly Guid Malta = new Guid("0DB1E4B0-CC61-4DFD-B941-DD25337DC7BF");
		public readonly Guid MarshallIslands = new Guid("A2B882E4-CD45-47FA-9DE7-F5908006527A");
		public readonly Guid Martinique = new Guid("AB58B6DF-9A22-42A5-BC47-4BAB7405AA3E");
		public readonly Guid Mauritania = new Guid("C8022800-E04D-48ED-B353-DAD28158CBBE");
		public readonly Guid Mauritius = new Guid("8B17C43C-6EC9-4C40-9A4C-AC932CF7441A");
		public readonly Guid Mayotte = new Guid("170A75B6-32B0-42E5-A240-8D14BF9A757D");
		public readonly Guid Mexico = new Guid("47FB7B52-FF22-4B51-9C31-EB086B096E78");
		public readonly Guid MicronesiaFederatedStatesof = new Guid("D0AC2D8B-61CC-41D1-854E-B04E146F5020");
		public readonly Guid MoldovaRepublicof = new Guid("E623E563-5C56-4D06-9770-445F2793062F");
		public readonly Guid Monaco = new Guid("7A54318E-40E7-4EBA-94F4-D44B21630956");
		public readonly Guid Mongolia = new Guid("B0D010B3-684B-4B78-A3AF-2C06BB21CBBE");
		public readonly Guid Montenegro = new Guid("81970D25-DE55-4AF1-A17C-B492F2E24138");
		public readonly Guid Montserrat = new Guid("DB7AC9F1-7F40-482B-9AFA-D97D91E83DE1");
		public readonly Guid Morocco = new Guid("35388DA6-443A-4AA4-B852-AED3BC510903");
		public readonly Guid Mozambique = new Guid("D1CB200C-F0DD-4D71-B474-BB2AD8B47087");
		public readonly Guid Myanmar = new Guid("E9393D5F-1548-46CC-9323-A81757B9E915");
		public readonly Guid Namibia = new Guid("3EDCF0EC-0533-4C57-ACDF-0F5780E7BFFE");
		public readonly Guid Nauru = new Guid("FE33E3CC-2DC6-49C9-8546-D981D3D733D1");
		public readonly Guid Nepal = new Guid("E1E75083-241A-4300-A6D8-EE4DF2D13607");
		public readonly Guid Netherlands = new Guid("3780DD4B-FA3E-4BAF-9FB2-451DC3707902");
		public readonly Guid NetherlandsAntilles = new Guid("3319DD92-7EB0-49F9-9997-C943DF677A4B");
		public readonly Guid NewCaledonia = new Guid("8E3784A5-09F2-418B-905C-7BC3A85231AF");
		public readonly Guid NewZealand = new Guid("7A546412-B30C-46BB-AC7A-E60FC6120D1F");
		public readonly Guid Nicaragua = new Guid("DB9BF19E-FEF5-422D-8F1A-9C5089B339C5");
		public readonly Guid Niger = new Guid("33460B84-FA96-4DA2-8162-75B3E6C8EDBD");
		public readonly Guid Nigeria = new Guid("2F226B1F-98F6-4614-99BA-2F10577A3B24");
		public readonly Guid Niue = new Guid("552E611D-D87F-4BA4-8C32-22C3F40DF39A");
		public readonly Guid NorfolkIsland = new Guid("72A8452B-FB1C-4FB8-97DC-DCA6F8B18248");
		public readonly Guid NorthernMarianaIslands = new Guid("E803878B-B4F0-4F7C-B0D8-A857FB8161EF");
		public readonly Guid Norway = new Guid("24BD408F-CA23-4131-A6D7-B40D8B50A6E1");
		public readonly Guid Oman = new Guid("EE68F0E7-F312-4284-A143-B628AF684A23");
		public readonly Guid Pakistan = new Guid("37B23B13-7BE1-4565-8B2F-52373BC33A07");
		public readonly Guid Palau = new Guid("FC66BE7D-1BEE-4FCB-8DDB-7A096A5C1F06");
		public readonly Guid Panama = new Guid("608A56DD-92D5-4651-864A-FE1DCFEA9782");
		public readonly Guid PapuaNewGuinea = new Guid("0FDDF65B-1707-4420-9797-DD9F3528D96D");
		public readonly Guid Paraguay = new Guid("7529EC00-D3D6-49A5-AB5F-DB227E7C497E");
		public readonly Guid Peru = new Guid("F363AE27-3D92-40B1-9731-98AAA3530809");
		public readonly Guid Philippines = new Guid("3FB723E1-B187-4DAA-B06B-E9A6BF970280");
		public readonly Guid Pitcairn = new Guid("9E16C786-72DD-484E-93EA-FB7686A769CF");
		public readonly Guid Poland = new Guid("C56D24C4-3CB6-4C21-87F3-B05A4EB0E4B5");
		public readonly Guid Portugal = new Guid("0867819F-404A-4709-9BB1-6BEED8F34AC6");
		public readonly Guid PuertoRico = new Guid("7FE1132A-5DCB-43E7-82FC-4190C2F610D0");
		public readonly Guid Qatar = new Guid("68835AB7-A607-497D-BB6B-04E5417B75F6");
		public readonly Guid Reunion = new Guid("5F5FA8A0-3D3E-4537-B4A7-30BD7567456B");
		public readonly Guid Romania = new Guid("91548CB3-E0BB-4766-B467-2E2F1920D5E9");
		public readonly Guid RussianFederation = new Guid("F825830B-EE5D-4300-88CD-0F049EB37E95");
		public readonly Guid Rwanda = new Guid("0AC5857D-7A71-4034-BB29-FAA04E9A11B6");
		public readonly Guid SaintBarthelemy = new Guid("03739D59-5043-4119-A64A-D83A02E753EA");
		public readonly Guid SaintHelena = new Guid("F020A6C8-3C49-45C9-955F-D6F463453DEE");
		public readonly Guid SaintKittsandNevis = new Guid("3D6E0EB4-AE88-47A6-9E68-E23BD4BB9DA8");
		public readonly Guid SaintLucia = new Guid("C59A8C47-0D70-4919-987A-3376C1253551");
		public readonly Guid SaintMartin = new Guid("FAF6CE69-3C16-4AA5-94F4-A84B9DA3FED3");
		public readonly Guid SaintPierreandMiquelon = new Guid("702774A7-B3A6-48E1-9450-88854640C7CC");
		public readonly Guid SaintVincentandtheGrenadines = new Guid("0E46DB15-6990-40D5-965E-FCCB271D7B3D");
		public readonly Guid Samoa = new Guid("5D32189C-DA70-44B9-9816-E801C451E4BA");
		public readonly Guid SanMarino = new Guid("F215F1C0-9A05-4CEE-A2CD-57E57D1CD432");
		public readonly Guid SaoTomeandPrincipe = new Guid("A7326C63-0CAA-4949-B9DD-F35D4EC49C71");
		public readonly Guid SaudiArabia = new Guid("A1DE844A-9ECC-40C7-A9BD-50B13951E643");
		public readonly Guid Senegal = new Guid("8FE760A3-188A-408B-8070-ABF857DF99E8");
		public readonly Guid Serbia = new Guid("2C2E5353-86AE-4FB6-98EC-AEB35127C077");
		public readonly Guid Seychelles = new Guid("917E9CF7-5A83-49C6-AE97-68FF11F5478D");
		public readonly Guid SierraLeone = new Guid("16CC2004-F75C-4571-AEE0-25F56F77302A");
		public readonly Guid Singapore = new Guid("DCCBAC74-137B-459B-A6AE-8A7CF7AD5A63");
		public readonly Guid Slovakia = new Guid("802962A0-0722-4E5F-807A-F9437BBB266B");
		public readonly Guid Slovenia = new Guid("B7B76AD7-A517-4641-977D-E4E2904BB756");
		public readonly Guid SolomonIslands = new Guid("7B7F1F90-0B22-408F-B6B2-07F406824CB1");
		public readonly Guid Somalia = new Guid("8D78A572-3EC1-467D-AFE0-0EC715A44540");
		public readonly Guid SouthAfrica = new Guid("43DE5231-FB99-473B-890B-2989740DB446");
		public readonly Guid SouthGeorgiaandtheSouthSandwic = new Guid("EDE3E194-4040-4359-957E-02B285036B95");
		public readonly Guid Spain = new Guid("0599EA7F-8012-418C-8264-3B15EFA9B8F0");
		public readonly Guid SriLanka = new Guid("8A93C750-7831-48D8-8650-0B9C691BBA22");
		public readonly Guid Sudan = new Guid("ECF4D0C2-4AF3-4115-9E2D-964C26C13038");
		public readonly Guid Suriname = new Guid("4E2A9273-632B-40CA-92D5-6F04F922A218");
		public readonly Guid SvalbardandJanMayen = new Guid("0B3BF763-7A1F-4A5F-9FED-6EDC5ABA70EC");
		public readonly Guid Swaziland = new Guid("A2454743-0616-4A24-B0F4-884243E8A32D");
		public readonly Guid Sweden = new Guid("92176973-9368-4445-A2DB-72FC33125472");
		public readonly Guid Switzerland = new Guid("09B097D5-6825-4C8B-B17E-DFAA96309647");
		public readonly Guid SyrianArabRepublic = new Guid("FA217340-08EC-45CC-88F0-B6EA57A27CE8");
		public readonly Guid Taiwan = new Guid("AEC68C8C-1890-43B6-84C7-FCB6C97E2516");
		public readonly Guid Tajikistan = new Guid("1F2ECBDD-9D00-4DAD-8FB9-62456C5C8BED");
		public readonly Guid TanzaniaUnitedRepublicof = new Guid("8C5FF645-2EA0-45BB-B567-ACAE98BBD73D");
		public readonly Guid Thailand = new Guid("37B8D0EE-6FFF-45A7-8F93-26283552EA11");
		public readonly Guid TimorLeste = new Guid("5F6CD2DE-FBCA-4AC8-A4AD-C18896C89F62");
		public readonly Guid Togo = new Guid("B60729B4-DDC4-4063-ABF4-D70915B7BD05");
		public readonly Guid Tokelau = new Guid("62B0F397-D72F-4C0A-977D-809A9F7E6389");
		public readonly Guid Tonga = new Guid("B5052CE3-6BA1-43DC-9912-DDC2B5BF4F38");
		public readonly Guid TrinidadandTobago = new Guid("B41DD08E-5513-4153-9B0D-CA090AF1EFBD");
		public readonly Guid Tunisia = new Guid("E2739DD4-C18D-4564-9DC6-E1D186BE895B");
		public readonly Guid Turkey = new Guid("A12B3D34-08FE-4AF5-A8D6-5B99E9AAB655");
		public readonly Guid Turkmenistan = new Guid("109D8374-E33C-4ACB-AA46-6EFF228E749F");
		public readonly Guid TurksandCaicosIslands = new Guid("4F533F55-4780-4AF7-B660-0900A1473E38");
		public readonly Guid Tuvalu = new Guid("E9663D95-1B28-47D0-AC73-F742D734D552");
		public readonly Guid Uganda = new Guid("237197FD-8DBC-4319-AAE1-950D4BC31DD2");
		public readonly Guid Ukraine = new Guid("7B231AC0-580D-4034-A76B-BCCE816CE9A9");
		public readonly Guid UnitedArabEmirates = new Guid("CB54F126-F9DA-40F3-BF0D-79B01EB2174B");
		public readonly Guid UnitedKingdom = new Guid("E0440F95-2047-40BE-B5B4-27DE5911B395");
		public readonly Guid UnitedStates = new Guid("8D85BDE0-7879-4902-AB4B-BF832521A6D3");
		public readonly Guid UnitedStatesMinorOutlyingIsland = new Guid("A5BFA4F9-7470-4E04-BAEC-49137EF2F7E7");
		public readonly Guid Uruguay = new Guid("A70203FB-B41B-438A-A0D8-6BAE48F2437C");
		public readonly Guid Uzbekistan = new Guid("D06B85F8-1E2A-4517-80A0-16060ED05A4C");
		public readonly Guid Vanuatu = new Guid("5BBBAEC6-78BE-4D96-85F0-85FEF603126D");
		public readonly Guid Venezuela = new Guid("9A0DDADD-660E-455A-835D-D13372B83789");
		public readonly Guid Vietnam = new Guid("86EC8B98-F72F-4DF9-87D6-E216BF2AD346");
		public readonly Guid VirginIslandsBritish = new Guid("3DBB4352-3C99-4FBA-8B52-B99C138D1518");
		public readonly Guid VirginIslandsUS = new Guid("0D7A2893-5709-40C4-B004-6E470A68F457");
		public readonly Guid WallisandFutuna = new Guid("B0ED386F-2F7A-438D-80BB-D1585D115E1D");
		public readonly Guid WesternSahara = new Guid("664788F2-A75A-4B5D-B810-BE7A1A0A4B89");
		public readonly Guid Yemen = new Guid("DD0B536D-A936-4FB3-B5ED-420F637DA69F");
		public readonly Guid Zambia = new Guid("88F005D4-3924-4334-AC82-C04CD8F13D6A");
		public readonly Guid Zimbabwe = new Guid("CEF5945E-550B-4D90-94C4-A889EDA23481");

		public static IEnumerable<Guid> CountriesUnderUSCustomsJurisdiction => countriesUnderUSCustomsJurisdiction ?? (countriesUnderUSCustomsJurisdiction = new[] { Instance.UnitedStates, Instance.PuertoRico });

		[ThreadStatic]
		static IEnumerable<Guid> countriesUnderUSCustomsJurisdiction;

		public static IEnumerable<Guid> AllUSCountriesForEntryFilerID => allUSCountriesForEntryFilerID ?? (allUSCountriesForEntryFilerID = new[] { Instance.UnitedStates, Instance.PuertoRico, Instance.VirginIslandsUS });

		[ThreadStatic]
		static IEnumerable<Guid> allUSCountriesForEntryFilerID;

		public static IEnumerable<Guid> CountriesUnderEUCustomsJurisdiction
		{
			get
			{
				if (countriesUnderEUCustomsJurisdiction == null)
				{
					countriesUnderEUCustomsJurisdiction = new[]
					{
						Instance.Austria,
						Instance.Belgium,
						Instance.Bulgaria,
						Instance.Croatia,
						Instance.Cyprus,
						Instance.CzechRepublic,
						Instance.Denmark,
						Instance.Germany,
						Instance.Estonia,
						Instance.Finland,
						Instance.France,
						Instance.Greece,
						Instance.Hungary,
						Instance.Ireland,
						Instance.Italy,
						Instance.Latvia,
						Instance.Lithuania,
						Instance.Luxembourg,
						Instance.Malta,
						Instance.Netherlands,
						Instance.Poland,
						Instance.Portugal,
						Instance.Romania,
						Instance.Spain,
						Instance.Sweden,
						Instance.Slovakia,
						Instance.Slovenia,
						Instance.UnitedKingdom
					};
				}
				return countriesUnderEUCustomsJurisdiction;
			}
		}
		[ThreadStatic]
		static IEnumerable<Guid> countriesUnderEUCustomsJurisdiction;

		public static IEnumerable<Guid> CountriesUnderEUCustomsJurisdictionExceptUK
		{
			get
			{
				if (countriesUnderEUCustomsJurisdictionExceptUK == null)
				{
					countriesUnderEUCustomsJurisdictionExceptUK = CountriesUnderEUCustomsJurisdiction.Except(new[] { Instance.UnitedKingdom }).ToArray();
				}
				return countriesUnderEUCustomsJurisdictionExceptUK;
			}
		}
		[ThreadStatic]
		static Guid[] countriesUnderEUCustomsJurisdictionExceptUK;

		public static IEnumerable<Guid> CountriesUnderEUCustomsJurisdictionPlusSwissNorwayTurkey => countriesUnderEUCustomsJurisdictionPlusSwissNorwayTurkey ??= CountriesUnderEUCustomsJurisdiction
			.Concat([Instance.Switzerland, Instance.Norway, Instance.Turkey])
			.ToArray();
		[ThreadStatic]
		static Guid[] countriesUnderEUCustomsJurisdictionPlusSwissNorwayTurkey;
	}

	#endregion

	#region SG Declaration Status

	public static class SGConstants
	{
		public static class DeclarationStatus
		{
			public const string JobOpenButNoMessageSent = "OPN";

			public const string DeclarationPending = "DPD";
			public const string DeclarationSent = "DSN";
			public const string DeclarationRejectedByCustoms = "DRJ";
			public const string DeclarationHadSyntaxErrors = "DER";
			public const string DeclarationPermitReceived = "DOK";
			public const string DeclarationQuery = "DQY";

			public const string AmendmentPending = "APD";
			public const string AmendmentSent = "ASN";
			public const string AmendmentRejectedByCustoms = "ARJ";
			public const string AmendmentHadSyntaxErrors = "AER";
			public const string AmendmentPermitReceived = "AOK";
			public const string AmendmentQuery = "AQY";

			public const string RefundPending = "RPD";
			public const string RefundSent = "RSN";
			public const string RefundRejectedByCustoms = "RRJ";
			public const string RefundHadSyntaxErrors = "RER";
			public const string RefundPermitReceived = "ROK";
			public const string RefundQuery = "RQY";

			public const string CancellationPending = "CPD";
			public const string CancellationSent = "CSN";
			public const string CancellationRejectedByCustoms = "CRJ";
			public const string CancellationHadSyntaxErrors = "CER";
			public const string CancellationAccepted = "COK";
			public const string CancellationQuery = "CQY";
		}
	}

	#endregion

	#region Navigation Bar Styles

	public enum NavBarStyles
	{
		Floating,
		TreeView,
		OutlookBar
	}

	#endregion

	#region Weight/Vol Display Type

	public static class WeightAndVolumeDisplayTypes
	{
		public static class Codes
		{
			public const string Actual = "ACT";
			public const string Client = "CLI";
			public const string Carrier = "CAR";
		}

		public static class Descriptions
		{
			public static MultilingualString Actual { get { return ResString.GetMultilingualString("29361eb2-4e32-4195-bc80-89060725704c", "Actual"); } }
			public static MultilingualString Client { get { return ResString.GetMultilingualString("07dcd559-135a-4272-bbb3-87cbed53c999", "Client"); } }
			public static MultilingualString Carrier { get { return ResString.GetMultilingualString("d6db54da-7555-4f19-9a2e-260018d0b4b5", "Carrier"); } }
		}
	}

	#endregion

	#region Units System

	public enum UnitsSystem
	{
		Metric,
		Imperial
	}

	#endregion

	#region TraceSourceCodes

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
	public static class AccountingTraceSourceCodes
	{
		public const string CategoryName = "Accounting";
		public const string Http = "HTTP";
		public const string CASS = "CASS";
		public const string ConsolCost = "Consol Cost";
		public const string eInvoicing = "eInvoicing";
		public const string FPOS = "FPOS";
		public const string JCS = "JCS Diagnosis";
		public const string APA = "AP Automation";
		public const string CLC = "Credit Limit Check";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
	public static class CoreTraceSourceCodes
	{
		public const string CategoryName = "Core";
		public const string Registry = "Registry";
	}
	#endregion

	#region SearchPerformedType

	public static class SearchPerformedType
	{
		public const string ModuleSql = "ModuleSql";
		public const string ModuleIndexSearch = "ModuleIndexSearch";
		public const string PaveSql = "PaveSql";
		public const string PaveIndexSearch = "PaveIndexSearch";
		public const string GlobalSearch = "GlobalSearch";
	}

	#endregion

	#region Forwarding
	public static class ContainerWeightLimitType
	{
		public const string AveragePerTEU = "AVT";
		public const string AbsolutePerTEU = "ABT";
	}
	#endregion
}
