namespace Enterprise.Client.JAS.Business.JXC
{
	/// <summary>
	/// Including classes for Field Positions for JXC Lines. Enum is not suitable because inheritance is used.
	/// </summary>
	public static class JXCConstants
	{
		public const char Delimiter = ';';
		public const string Version = "3100";
		public const string NotAvailable = "N/A";
		public const int LineTypeLength = 4;
		public const string DateFormat = "dd/MM/yyyy";
		public const string JXCWarningPrefix = "JXC: ";

		#region ContainerTypes

		public static class ContainerTypes
		{
			public const string Other = "99";
		}

		#endregion

		#region Message Types & Categories

		public enum MessageCategories
		{
			Unknown,
			Air,
			Ocean,
			Financial,
			Miscellaneous
		}

		public abstract class MessageTypes
		{
			// Air
			public const string MAWB = LineTypes.MAWB;
			public const string DAWB = LineTypes.DAWB;
			public const string CHAB = LineTypes.CHAB;
			public const string PSAB = LineTypes.PSAB;

			// Ocean
			public const string OMAN = LineTypes.OMAN;
			public const string COHB = LineTypes.COHB;
			public const string PSBL = LineTypes.PSBL;

			// Miscellaneous
			public const string LINK = LineTypes.LINK;
			public const string GSUM = LineTypes.GSUM;
			public const string CCCV = LineTypes.CCCV;

			// Financial
			public const string AINV = LineTypes.AINV;
			public const string ACDT = LineTypes.ACDT;
			public const string MINV = LineTypes.MINV;
			public const string MCDT = LineTypes.MCDT;
			public const string NINV = LineTypes.NINV;
			public const string NCDT = LineTypes.NCDT;
			public const string ISMY = LineTypes.ISMY;
			public const string APRS = LineTypes.APRS;

			public static MessageCategories GetMessageCategoryFromMessageType(string messageType)
			{
				MessageCategories result;

				switch (messageType)
				{
					case MAWB:
					case DAWB:
					case CHAB:
					case PSAB:
						result = MessageCategories.Air;
						break;

					case OMAN:
					case COHB:
					case PSBL:
						result = MessageCategories.Ocean;
						break;

					case LINK:
					case GSUM:
					case CCCV:
						result = MessageCategories.Miscellaneous;
						break;

					case AINV:
					case ACDT:
					case MINV:
					case MCDT:
					case NINV:
					case NCDT:
					case ISMY:
					case APRS:
						result = MessageCategories.Financial;
						break;

					default:
						result = MessageCategories.Unknown;
						break;
				}

				return result;
			}
		}

		#endregion

		#region Lines

		#region LineTypes

		public static class LineTypes
		{
			public const string HEAD = "HEAD";
			public const string TRLR = "TRLR";

			public const string FBDN = "FBDN";
			public const string OTHR = "OTHR";
			public const string HAWB = "HAWB";
			public const string CHAB = "CHAB";
			public const string PSAB = "PSAB";
			public const string MAWB = "MAWB";
			public const string DAWB = "DAWB";
			public const string DHAB = "DHAB";

			public const string PSTA = "PSTA";
			public const string REFR = "REFR";
			public const string SHMK = "SHMK";

			public const string OHBL = "OHBL";
			public const string PSBL = "PSBL";
			public const string COHB = "COHB";
			public const string OMAN = "OMAN";
			public const string CHGS = "CHGS";
			public const string CONT = "CONT";
			public const string DOHB = "DOHB";

			public const string LINK = "LINK";
			public const string GSUM = "GSUM";
			public const string CCCV = "CCCV";

			public const string AINV = "AINV";
			public const string ACDT = "ACDT";
			public const string MINV = "MINV";
			public const string MCDT = "MCDT";
			public const string NINV = "NINV";
			public const string NCDT = "NCDT";
			public const string INVD = "INVD";
			public const string ISMY = "ISMY";
			public const string INV = "INV";
			public const string CDT = "CDT";
			public const char MaritimeTransactionPrefix = 'M';
			public const char AirTransactionPrefix = 'A';
			public const char NonShipmentTransactionPrefix = 'N';

			public const string APRS = "APRS";
			public const string APRH = "APRH";
		}

		#endregion

		#region TypeOfRecords

		public static class TypeOfRecord
		{
			public const string New = "N";
			public const string Amended = "A";
			public const string Deleted = "D";
		}

		#endregion

		#region HEAD

		public static class HEADFieldPositions
		{
			public const int DestOfficeCode = 0;
			public const int SendingOfficeCode = 1;
			public const int DestNettingCode = 2;
			public const int SendingNettingCode = 3;
			public const int FreightDest = 4;
		}

		public const string NoNettingCode = "NONET";
		public const int HEADFieldCount = 5;

		#endregion

		#region TRLR

		public const int TRLRFieldCount = 0;

		#endregion

		#region FBDN

		public static class FBDNFieldPositions
		{
			public const int NoOfPiecesOrRCP = 0;
			public const int TypeOfPieces = 1;
			public const int GrossWeight = 2;
			public const int WeightInKgsOrLbs = 3;
			public const int RateClassCode = 4;
			public const int IATACommodityItemNumber = 5;
			public const int HTSCommodityItemNumber = 6;
			public const int ChargeableWeight = 8;
			public const int RateCharge = 9;
			public const int Total = 10;
			public const int NatureAndQtyOfGoods = 11;
			public const int DescriptionOfGoods = 12;
		}

		public static class FBDNFieldBoundaries
		{
			public const int NoOfPiecesMinLength = 1;
			public const int NoOfPiecesMaxLength = 4;
			public const int CommodityItemNumberMaxLength = 7;
			public const int NatureAndQtyOfGoodsMaxLength = 240;
			public const int GoodsDescriptionMaxLength = 35;

			public const string WeightUnit = @"^[kL]$";
			public const string RateClassCode = "^(B|C|E|K|M|N|Q|R|S|U|X|Y)$";
		}

		public const int FBDNFieldCount = 16;

		#endregion

		#region AWB

		#region Base

		public abstract class AWBFieldPositions
		{
			public abstract int TypeOfRecord { get; }
			public abstract int OriginCityCode { get; }
			public abstract int AirlinePrefix { get; }
			public abstract int MAWBSerialNo { get; }
			public abstract int ShipperName { get; }
			public abstract int ShipperAddress1 { get; }
			public abstract int ShipperAddress2 { get; }
			public abstract int ShipperAccountNo { get; }
			public abstract int ConsigneeName { get; }
			public abstract int ConsigneeAddress1 { get; }
			public abstract int ConsigneeAddress2 { get; }
			public abstract int ConsigneeAccountNo { get; }
			public abstract int CarrierName { get; }
			public abstract int CarrierAddress1 { get; }
			public abstract int CarrierAddress2 { get; }
			public abstract int CarrierCity { get; }
			public abstract int AgentName { get; }
			public abstract int AgentAddress1 { get; }
			public abstract int AgentAddress2 { get; }
			public abstract int AgentAddress3 { get; }
			public abstract int AgentIATACode { get; }
			public abstract int AgentAccountNo { get; }
			public abstract int AirportOfDeparture { get; }
			public abstract int AccountingInfo1 { get; }
			public abstract int To1st { get; }
			public abstract int By1st { get; }
			public abstract int To2nd { get; }
			public abstract int By2nd { get; }
			public abstract int To3rd { get; }
			public abstract int By3rd { get; }
			public abstract int Currency { get; }
			public abstract int ChargeCode { get; }
			public abstract int WeightPPDorCOL { get; }
			public abstract int OtherPPDorCOL { get; }
			public abstract int DeclaredValue { get; }
			public abstract int CurrencyCodeForDeclaredValue { get; }
			public abstract int CustomsValue { get; }
			public abstract int CurrencyCodeForCustomsValue { get; }
			public abstract int AirportOfDestination { get; }
			public abstract int FlightNo1 { get; }
			public abstract int FlightDate1 { get; }
			public abstract int FlightNo2 { get; }
			public abstract int FlightDate2 { get; }
			public abstract int Insurance { get; }
			public abstract int HandlingInfo1 { get; }
			public abstract int HandlingInfo2 { get; }
			public abstract int HandlingInfo3 { get; }
			public abstract int TotalNoOfPieces { get; }
			public abstract int TotalGrossWeight { get; }
			public abstract int WeightUnit { get; }
			public abstract int Total { get; }
			public abstract int PrepaidWeightCharge { get; }
			public abstract int CollectWeightCharge { get; }
			public abstract int PrepaidValuationCharge { get; }
			public abstract int CollectValuationCharge { get; }
			public abstract int PrepaidTax { get; }
			public abstract int CollectTax { get; }
			public abstract int PrepaidOtherChargesDueAgent { get; }
			public abstract int CollectOtherChargesDueAgent { get; }
			public abstract int PrepaidOtherChargesDueCarrier { get; }
			public abstract int CollectOtherChargesDueCarrier { get; }
			public abstract int TotalPrepaid { get; }
			public abstract int TotalCollect { get; }
			public abstract int SignatureOfShipperOrAgent { get; }
			public abstract int DateOfIssue { get; }
			public abstract int PlaceOfIssue { get; }
			public abstract int SignatureOfCarrierOrAgent { get; }
			public abstract int ShipperCountryCode { get; }
			public abstract int ConsigneeCountryCode { get; }
			public abstract int ShipperCity { get; }
			public abstract int ShipperState { get; }
			public abstract int ShipperPostCode { get; }
			public abstract int ConsigneeCity { get; }
			public abstract int ConsigneeState { get; }
			public abstract int ConsigneePostCode { get; }
		}

		public abstract class AWBFieldBoundaries
		{
			#region Header

			public const int OriginCodeLength = 3;
			public const int AirlinePrefixLength = 3;
			public const int MAWBSerialNoLength = 8;

			#endregion

			#region Shipper

			public const int ShipperAccountMaxLength = 14;

			public const int ShipperNameMinLength = 1;
			public const int ShipperNameMaxLength = 100;

			public const int ShipperAddressMinLength = 1;

			public const int ShipperCityMinLength = 1;
			public const int ShipperCityMaxLength = 17;

			public const int ShipperStateMaxLength = 9;

			public const int ShipperPostCodeMaxLength = 12;

			#endregion

			#region Consignee

			public const int ConsigneeAccountMaxLength = 14;

			public const int ConsigneeNameMinLength = 1;
			public const int ConsigneeNameMaxLength = 100;

			public const int ConsigneeAddressMinLength = 1;

			public const int ConsigneeCityMinLength = 1;
			public const int ConsigneeCityMaxLength = 17;

			public const int ConsigneeStateMaxLength = 9;

			public const int ConsigneePostCodeMaxLength = 12;

			#endregion

			#region Carrier

			public const int CarrierNameMinLength = 1;
			public const int CarrierNameMaxLength = 75;

			#endregion

			#region Agent

			public const int AgentNameMaxLength = 75;
			public const int AgentAddressMaxLength = 35;
			public const int AgentPlaceMaxLength = 17;
			public const string AgentIATACodeRegex = @"^([0-9]{2}[-/][0-9][-/ ][0-9]{1,4}([-/][0-9]{1,4})?)?$";
			public const int AgentAccountNoMaxLength = 14;

			#endregion

			#region Routing

			public const int ToLength = 3;
			public const int ByLength = 2;

			#endregion

			#region Freight Declarations

			public const string ChargeCode = @"^(ca|cb|cc|ce|cg|ch|cp|cx|cz|nc|ng|np|nt|nx|nz|pc|pd|pe|pf|pg|ph|pp|px|pz)?$";
			public const string WeightPPDorCOL = @"^[PC]$";
			public const string OtherPPDorCOL = @"^[PC]$";

			#endregion

			#region Flight Info

			public const int AirportOfDepartureMinLength = 1;
			public const int AirportOfDepartureMaxLength = 35;

			public const int AirportOfDestinationMinLength = 1;
			public const int AirportOfDestinationMaxLength = 25;

			public const int FlightCarrierCodeLength = 2;
			public const string FlightNumberRegex = @"^[0-9]{3,4}[a-z]?$";

			#endregion

			#region Freight Info

			public const int TotalNoOfPiecesMinLength = 1;
			public const int TotalNoOfPiecesMaxLength = 4;

			#endregion

			#region Footer

			public const int PlaceOfIssueMinLength = 1;
			public const int PlaceOfIssueMaxLength = 17;
			public const int SignatureMaxLength = 35;

			#endregion

			public const int AccountingInfoMaxLength = 35;
		}

		#endregion

		#region MAWB

		public class MAWBFieldPositions : AWBFieldPositions
		{
			public const int BookedStatus = 1;
			public const int ShipperAddress4 = 9;
			public const int ConsigneeAddress4 = 15;
			public const int JASCommodityCode = 65;
			public const int ShipperStreetAddress = 111;
			public const int ConsigneeStreetAddress = 115;

			#region Overrides

			public override int TypeOfRecord { get { return 0; } }
			public override int OriginCityCode { get { return 2; } }
			public override int AirlinePrefix { get { return 3; } }
			public override int MAWBSerialNo { get { return 4; } }
			public override int ShipperName { get { return 5; } }
			public override int ShipperAddress1 { get { return 6; } }
			public override int ShipperAddress2 { get { return 7; } }
			public override int ShipperAccountNo { get { return 10; } }
			public override int ConsigneeName { get { return 11; } }
			public override int ConsigneeAddress1 { get { return 12; } }
			public override int ConsigneeAddress2 { get { return 13; } }
			public override int ConsigneeAccountNo { get { return 16; } }
			public override int CarrierName { get { return 17; } }
			public override int CarrierAddress1 { get { return 18; } }
			public override int CarrierAddress2 { get { return 19; } }
			public override int CarrierCity { get { return 21; } }
			public override int AgentName { get { return 22; } }
			public override int AgentAddress1 { get { return 23; } }
			public override int AgentAddress2 { get { return 24; } }
			public override int AgentAddress3 { get { return 25; } }
			public override int AgentIATACode { get { return 26; } }
			public override int AgentAccountNo { get { return 27; } }
			public override int AirportOfDeparture { get { return 28; } }
			public override int AccountingInfo1 { get { return 29; } }
			public override int To1st { get { return 36; } }
			public override int By1st { get { return 37; } }
			public override int To2nd { get { return 38; } }
			public override int By2nd { get { return 39; } }
			public override int To3rd { get { return 40; } }
			public override int By3rd { get { return 41; } }
			public override int Currency { get { return 42; } }
			public override int ChargeCode { get { return 43; } }
			public override int WeightPPDorCOL { get { return 44; } }
			public override int OtherPPDorCOL { get { return 45; } }
			public override int DeclaredValue { get { return 46; } }
			public override int CurrencyCodeForDeclaredValue { get { return 47; } }
			public override int CustomsValue { get { return 48; } }
			public override int CurrencyCodeForCustomsValue { get { return 49; } }
			public override int AirportOfDestination { get { return 50; } }
			public override int FlightNo1 { get { return 51; } }
			public override int FlightDate1 { get { return 52; } }
			public override int FlightNo2 { get { return 53; } }
			public override int FlightDate2 { get { return 54; } }
			public override int Insurance { get { return 55; } }
			public override int HandlingInfo1 { get { return 56; } }
			public override int HandlingInfo2 { get { return 57; } }
			public override int HandlingInfo3 { get { return 58; } }
			public override int TotalNoOfPieces { get { return 66; } }
			public override int TotalGrossWeight { get { return 67; } }
			public override int WeightUnit { get { return 68; } }
			public override int Total { get { return 69; } }
			public override int PrepaidWeightCharge { get { return 70; } }
			public override int CollectWeightCharge { get { return 71; } }
			public override int PrepaidValuationCharge { get { return 72; } }
			public override int CollectValuationCharge { get { return 73; } }
			public override int PrepaidTax { get { return 74; } }
			public override int CollectTax { get { return 75; } }
			public override int PrepaidOtherChargesDueAgent { get { return 76; } }
			public override int CollectOtherChargesDueAgent { get { return 77; } }
			public override int PrepaidOtherChargesDueCarrier { get { return 78; } }
			public override int CollectOtherChargesDueCarrier { get { return 79; } }
			public override int TotalPrepaid { get { return 80; } }
			public override int TotalCollect { get { return 81; } }
			public override int SignatureOfShipperOrAgent { get { return 97; } }
			public override int DateOfIssue { get { return 98; } }
			public override int PlaceOfIssue { get { return 99; } }
			public override int SignatureOfCarrierOrAgent { get { return 100; } }
			public override int ShipperCountryCode { get { return 109; } }
			public override int ConsigneeCountryCode { get { return 110; } }
			public override int ShipperCity { get { return 112; } }
			public override int ShipperState { get { return 113; } }
			public override int ShipperPostCode { get { return 114; } }
			public override int ConsigneeCity { get { return 116; } }
			public override int ConsigneeState { get { return 117; } }
			public override int ConsigneePostCode { get { return 118; } }

			#endregion
		}

		public class MAWBFieldBoundaries : AWBFieldBoundaries
		{
			public const int ShipperAddressMaxLength = 35;
			public const int ConsigneeAddressMaxLength = 35;
			public const int CarrierAddressMaxLength = 35;
		}

		public const int MAWBFieldCount = 121;

		#endregion

		#region HAWB

		public class HAWBFieldPositions : AWBFieldPositions
		{
			public const int OriginTrafficFileNo = 1;
			public const int OriginOfficeCode = 2;
			public const int CountryOfFreightOrigin = 3;
			public const int HAWBSerialNo = 4;
			public const int ShipperEmail = 21;
			public const int UpdateShipper = 22;
			public const int ShipperPhone = 23;
			public const int ConsigneeEmail = 34;
			public const int UpdateConsignee = 35;
			public const int ConsigneePhone = 36;
			public const int AlsoNotifyPartyName = 39;
			public const int AlsoNotifyPartyAddress1 = 40;
			public const int AlsoNotifyPartyAddress2 = 41;
			public const int AlsoNotifyPartyPlace = 43;
			public const int AlsoNotifyPartyEmail = 44;
			public const int UpdateAlsoNotifyParty = 45;
			public const int AlsoNotifyPartyPhone = 46;
			public const int ExcludeCommercialInvoiceFromReports = 118;
			public const int AMSAgent = 119;

			#region Overrides

			public override int TypeOfRecord { get { return 0; } }
			public override int OriginCityCode { get { return 5; } }
			public override int AirlinePrefix { get { return 6; } }
			public override int MAWBSerialNo { get { return 7; } }
			public override int CarrierName { get { return 8; } }
			public override int CarrierAddress1 { get { return 9; } }
			public override int CarrierAddress2 { get { return 10; } }
			public override int CarrierCity { get { return 12; } }
			public override int ShipperName { get { return 13; } }
			public override int ShipperAddress1 { get { return 14; } }
			public override int ShipperAddress2 { get { return 15; } }
			public override int ShipperCity { get { return 17; } }
			public override int ShipperState { get { return 18; } }
			public override int ShipperPostCode { get { return 19; } }
			public override int ShipperCountryCode { get { return 20; } }
			public override int ShipperAccountNo { get { return 24; } }
			public override int ConsigneeName { get { return 26; } }
			public override int ConsigneeAddress1 { get { return 27; } }
			public override int ConsigneeAddress2 { get { return 28; } }
			public override int ConsigneeCity { get { return 30; } }
			public override int ConsigneeState { get { return 31; } }
			public override int ConsigneePostCode { get { return 32; } }
			public override int ConsigneeCountryCode { get { return 33; } }
			public override int ConsigneeAccountNo { get { return 37; } }
			public override int AgentName { get { return 47; } }
			public override int AgentAddress1 { get { return 48; } }
			public override int AgentAddress2 { get { return 49; } }
			public override int AgentAddress3 { get { return 50; } }
			public override int AgentIATACode { get { return 51; } }
			public override int AgentAccountNo { get { return 52; } }
			public override int AirportOfDeparture { get { return 53; } }
			public override int AccountingInfo1 { get { return 54; } }
			public override int To1st { get { return 61; } }
			public override int By1st { get { return 63; } }
			public override int To2nd { get { return 64; } }
			public override int By2nd { get { return 65; } }
			public override int To3rd { get { return 66; } }
			public override int By3rd { get { return 67; } }
			public override int Currency { get { return 68; } }
			public override int ChargeCode { get { return 69; } }
			public override int WeightPPDorCOL { get { return 70; } }
			public override int OtherPPDorCOL { get { return 71; } }
			public override int DeclaredValue { get { return 72; } }
			public override int CurrencyCodeForDeclaredValue { get { return 73; } }
			public override int CustomsValue { get { return 74; } }
			public override int CurrencyCodeForCustomsValue { get { return 75; } }
			public override int AirportOfDestination { get { return 76; } }
			public override int FlightNo1 { get { return 77; } }
			public override int FlightDate1 { get { return 78; } }
			public override int FlightNo2 { get { return 79; } }
			public override int FlightDate2 { get { return 80; } }
			public override int Insurance { get { return 81; } }
			public override int HandlingInfo1 { get { return 82; } }
			public override int HandlingInfo2 { get { return 83; } }
			public override int HandlingInfo3 { get { return 84; } }
			public override int TotalNoOfPieces { get { return 93; } }
			public override int TotalGrossWeight { get { return 94; } }
			public override int WeightUnit { get { return 95; } }
			public override int Total { get { return 96; } }
			public override int PrepaidWeightCharge { get { return 97; } }
			public override int CollectWeightCharge { get { return 98; } }
			public override int PrepaidValuationCharge { get { return 99; } }
			public override int CollectValuationCharge { get { return 100; } }
			public override int PrepaidTax { get { return 101; } }
			public override int CollectTax { get { return 102; } }
			public override int PrepaidOtherChargesDueAgent { get { return 103; } }
			public override int CollectOtherChargesDueAgent { get { return 104; } }
			public override int PrepaidOtherChargesDueCarrier { get { return 105; } }
			public override int CollectOtherChargesDueCarrier { get { return 106; } }
			public override int TotalPrepaid { get { return 107; } }
			public override int TotalCollect { get { return 108; } }
			public override int SignatureOfShipperOrAgent { get { return 109; } }
			public override int DateOfIssue { get { return 110; } }
			public override int PlaceOfIssue { get { return 111; } }
			public override int SignatureOfCarrierOrAgent { get { return 112; } }

			#endregion
		}

		public class HAWBFieldBoundaries : AWBFieldBoundaries
		{
			public const int PhoneMaxLength = 25;

			public const int AlsoNotifyNameMaxLength = 100;
			public const int AlsoNotifyAddressMaxLength = 75;
			public const int AlsoNotifyPlaceMaxLength = 17;

			public const int ShipperAddressMaxLength = 75;
			public const int ConsigneeAddressMaxLength = 75;
			public const int CarrierAddressMaxLength = 75;
		}

		public const int HAWBFieldCount = 130;
		public const int CHABPSABFieldCount = 119;

		#endregion

		#endregion

		#region OTHR

		public static class OTHRFieldPositions
		{
			public const int PrepaidOrCollect = 0;
			public const int IATAChargeCode1 = 1;
			public const int ChargeDescription1 = 2;
			public const int ChargeAmount1 = 3;
			public const int IATAChargeCode2 = 4;
			public const int ChargeDescription2 = 5;
			public const int ChargeAmount2 = 6;
			public const int IATAChargeCode3 = 7;
			public const int ChargeDescription3 = 8;
			public const int ChargeAmount3 = 9;
		}

		public static class OTHRFieldBoundaries
		{
			public const int ChargeDescriptionMaxLength = 35;
		}

		public const int OTHRFieldCount = 10;

		#endregion

		#region PSTA

		public static class PSTAFieldPositions
		{
			public const int StatusCode = 0;
			public const int StatusDate = 1;
			public const int StatusTime = 2;
			public const int StatusInfo = 3;
		}

		public const int PSTAFieldCount = 4;

		#endregion

		#region REFR

		public static class REFRFieldPositions
		{
			public const int Reference = 0;
			public const int FromShipperOrConsignee = 1;
		}

		public static class REFRFieldBoundaries
		{
			public const int ReferenceMaxLength = 60;
		}

		public const int REFRFieldCount = 2;

		#endregion

		#region OHBL

		public static class OHBLFieldPositions
		{
			public const int TypeOfRecord = 0;
			public const int OriginTrafficFileNo = 1;
			public const int OriginOfficeCode = 2;
			public const int HouseBillOfLadingNo = 3;
			public const int BillOfLadingTypeCode = 4;
			public const int PortOfLoadingCode = 5;
			public const int CarrierSSLCode = 6;
			public const int CarrierName = 7;
			public const int CarrierAddress1 = 8;
			public const int CarrierAddress2 = 9;
			public const int CarrierCity = 11;
			public const int BillOfLadingNo = 12;
			public const int ShipperName = 13;
			public const int ShipperAddress1 = 14;
			public const int ShipperAddress2 = 15;
			public const int ShipperCity = 17;
			public const int ShipperState = 18;
			public const int ShipperPostCode = 19;
			public const int ShipperCountryCode = 20;
			public const int ShipperEmail = 21;
			public const int UpdateShipper = 22;
			public const int ShipperPhone = 23;
			public const int ShipperAccountNo = 24;
			public const int ConsigneeName = 26;
			public const int ConsigneeAddress1 = 27;
			public const int ConsigneeAddress2 = 28;
			public const int ConsigneeCity = 30;
			public const int ConsigneeState = 31;
			public const int ConsigneePostCode = 32;
			public const int ConsigneeCountryCode = 33;
			public const int ConsigneeEmail = 34;
			public const int UpdateConsignee = 35;
			public const int ConsigneePhone = 36;
			public const int ConsigneeAccountNo = 37;
			public const int DeliveryAgentName = 39;
			public const int DeliveryAgentAddress1 = 40;
			public const int DeliveryAgentAddress2 = 41;
			public const int DeliveryAgentCity = 43;
			public const int FMCNumber = 44;
			public const int PortOfLoadingName = 45;
			public const int NotifyPartyName = 46;
			public const int NotifyPartyAddress1 = 47;
			public const int NotifyPartyAddress2 = 48;
			public const int NotifyPartyCity = 50;
			public const int NotifyPartyEmail = 51;
			public const int UpdateNotifyParty = 52;
			public const int NotifyPartyPhone = 53;
			public const int UpdateNotifyParty2 = 59;
			public const int PortOfDischargeCode = 61;
			public const int Currency = 62;
			public const int TotalFreightAmount = 63;
			public const int PrepaidOrCollect = 64;
			public const int PortOfDischargeName = 65;
			public const int VesselName = 66;
			public const int VoyageSSELNumber = 67;
			public const int VesselFlagCountry = 68;
			public const int LloydsCode = 69;
			public const int OnBoardDate = 70;
			public const int SpecialInstructions1 = 71;
			public const int SpecialInstructions2 = 72;
			public const int SpecialInstructions3 = 73;
			public const int TotalNoOfPackages = 74;
			public const int PieceTypeCode = 75;
			public const int GrossWeight = 76;
			public const int KilosOrPounds = 77;
			public const int MeasurementInCBM = 78;
			public const int Rate = 79;
			public const int DescriptionOfPackagesAndGoods1 = 80;
			public const int DescriptionOfPackagesAndGoods2 = 81;
			public const int DescriptionOfPackagesAndGoods3 = 82;
			public const int DescriptionOfPackagesAndGoods4 = 83;
			public const int DescriptionOfPackagesAndGoods5 = 84;
			public const int DescriptionOfPackagesAndGoods6 = 85;
			public const int DescriptionOfPackagesAndGoods7 = 86;
			public const int DescriptionOfPackagesAndGoods8 = 87;
			public const int HarmonizedCommodityCode = 88;
			public const int TotalNoOfPackagesAndUnitsInWords = 89;
			public const int AsAgentForJASOceanServicesInc = 90;
			public const int DateOfIssue = 91;
			public const int PlaceOfIssue = 92;
			public const int LoadingDateTimeZone = 93;
			public const int ReceivingAgentName = 94;
			public const int ReceivingAgentAddress = 95;
			public const int ReceivingAgentCity = 96;
			public const int HandlingInstructions1 = 97;
			public const int HandlingInstructions2 = 98;
			public const int HandlingInstructions3 = 99;
			public const int PlaceOfReceipt = 100;
			public const int MarksAndNumbers = 101;
			public const int FreightPayableBy = 102;
			public const int PreCarriagePayableBy = 103;
			public const int OnCarriagePayableBy = 104;
			public const int NoOfOriginalBillOfLadings = 105;
			public const int PlaceOfDelivery = 106;
			public const int NoOfContainers = 107;
			public const int SpecialProcessingCode = 108;
			public const int ExcludeCommercialInvoiceFromReport = 109;
		}

		public static class OHBLFieldBoundaries
		{
			public const int PlaceNameMaxLength = 35;

			public const int HouseBillOfLadingNoMinLength = 1;
			public const int HouseBillOfLadingNoMaxLength = 20;

			public const int OriginTrafficFileNoMaxLength = 64;

			public const int BillOfLadingMaxLength = 20;

			public const int VoyageSSELNumberMinLength = 1;
			public const int VoyageSSELNumberMaxLength = 15;

			public const int CarrierNameMaxLength = 40;
			public const int CarrierAddressMaxLength = 40;
			public const int CarrierCityMaxLength = 40;

			public const int ShipperNameMaxLength = 35;
			public const int ShipperAddressMaxLength = 35;
			public const int ShipperCityMaxLength = 17;
			public const int ShipperStateMaxLength = 9;
			public const int ShipperPostalCodeMaxLength = 12;
			public const int ShipperPhoneMaxLength = 25;
			public const int ShipperAccountMaxLength = 14;

			public const int ConsigneeNameMaxLength = 35;
			public const int ConsigneeAddressMaxLength = 35;
			public const int ConsigneeCityMaxLength = 17;
			public const int ConsigneeStateMaxLength = 9;
			public const int ConsigneePostalCodeMaxLength = 12;
			public const int ConsigneePhoneMaxLength = 25;
			public const int ConsigneeAccountMaxLength = 14;

			public const int DeliveryAgentNameMaxLength = 40;
			public const int DeliveryAgentAddressMaxLength = 40;
			public const int DeliveryAgentCityMaxLength = 40;

			public const int NotifyPartyNameMaxLength = 40;
			public const int NotifyPartyAddressMaxLength = 40;
			public const int NotifyPartyCityMaxLength = 40;
			public const int NotifyPartyPhoneMaxLength = 25;

			public const int VesselNameMaxLength = 35;

			public const int TotalNoOfPackagesAndUnitInWordsMaxLength = 60;

			public const int ReceivingAgentNameMaxLength = 35;
			public const int ReceivingAgentAddressMaxLength = 35;

			public const int MarksAndNumbersMaxLength = 45;

			public const int PayableByMaxLength = 15;
		}

		public const int OHBLFieldCount = 110;

		#endregion

		#region OMAN

		public static class OMANFieldPositions
		{
			public const int TypeOfRecord = 0;
			public const int ManifestNo = 1;
			public const int VesselName = 2;
			public const int VoyageNo = 3;
			public const int PortOfLoadingName = 4;
			public const int PortOfDischargeName = 5;
			public const int PortOfLoadingCodeForCustoms = 6;
			public const int PortOfLoadingCode = 7;
			public const int PortOfDichargeCodeForCustoms = 8;
			public const int PortOfDischargeCode = 9;
			public const int ManifestPrintDate = 10;
			public const int EstimatedShippingDate = 11;
			public const int EstimatedArrivalDate = 12;
		}

		public static class OMANFieldBoundaries
		{
			public const int ManifestNoMaxLength = 20;
			public const int VesselNameMaxLength = 35;
			public const int PortNameMaxLength = 35;
		}

		public const int OMANFieldCount = 13;

		#endregion

		#region CHGS

		public static class CHGSFieldPositions
		{
			public const int ChargeCode = 0;
			public const int ChargeDescription = 1;
			public const int ChargeAmount = 2;
			public const int PrepaidOrCollect = 3;
			public const int Currency = 4;
		}

		public static class CHGSFieldBoundaries
		{
			public const int ChargeCode = 4;
			public const int ChargeDescription = 25;
		}

		public const int CHGSFieldCount = 5;

		#endregion

		#region CONT

		public static class CONTFieldPositions
		{
			public const int ContainerNo = 0;
			public const int SealNo = 1;
			public const int ContainerType = 3;
			public const int GrossWeightInKgs = 4;
			public const int MeasurementInCBM = 5;
			public const int NoOfPackages = 6;
			public const int GoodsDescription = 7;
			public const int TypeOfService = 8;
			public const int HTSNo = 9;
			public const int CommodityValue = 10;
			public const int CommodityCurrency = 11;
		}

		public static class CONTFieldBoundaries
		{
			public const int ContainerNoMinLength = 1;
			public const int ContainerNoMaxLength = 20;

			public const int SealNoMaxLength = 20;

			public const int DescriptionOfGoodsMaxLength = 40;
		}

		public const int CONTFieldCount = 12;

		#endregion

		#region SHMK

		public static class SHMKFieldPositions
		{
			public const int ShippingMarksAndNumbers = 0;
			public const int FreeTextDescription = 1;
			public const int NumberOfCartons = 2;
		}

		public static class SHMKFieldBoundaries
		{
			public const int ShippingMarksAndNumbersMaxLength = 60;
			public const int FreeTextDescriptionMaxLength = 250;
			public const int NumberOfCartonsMaxLength = 10;
		}

		public const int SHMKFieldCount = 3;

		#endregion

		#region DHAB

		public static class DHABFieldPosition
		{
			public const int OriginTrafficFileNo = 1;
			public const int OriginOfficeCode = 2;
			public const int HouseBillNumber = 3;
		}

		public const int DHABFieldCount = 4;

		#endregion

		#region ISMY

		public static class ISMYFieldPosition
		{
			public const int TotalCredit = 0;
			public const int TotalInvoice = 1;
		}

		public const int ISMYFieldCount = 2;

		#endregion

		#region INVD

		public static class INVDFieldPosition
		{
			public const int RevenueCode = 0;
			public const int RevenueDescription = 1;
			public const int RevenueAmount = 2;
			public const int VATCode = 3;
			public const int HouseBillNumber = 4;
			public const int ContainerNumber = 5;
			public const int SealNumber = 6;
		}

		public static class INVDFieldBoundaries
		{
			public const int DescriptionMaxLength = 45;
			public const int TaxCodeMaxLength = 4;
			public const int ChargeCodeMaxLength = 4;
			public const int HouseBillMaxLength = 20;
		}

		public const int INVDFieldCount = 7;

		#endregion

		#region INV and CDT

		public abstract class INVCDTFieldPositions
		{
			public readonly int TransactionNumber;
			public readonly int SendingNettingCode = 1;
			public readonly int TransactionDate = 2;
			public readonly int ShipperName = 3;
			public readonly int ShipperAddress1 = 4;
			public readonly int ShipperAddress2 = 5;
			public readonly int ISOCountryCode = 7;
			public readonly int VATCode = 8;
			public readonly int TransactionPerOperativo = 9;
			public readonly int ManifestSerialNumber = 12;
			public readonly int DateCargoManifest = 13;
			public readonly int Reference = 16;
			public readonly int NumberOfPieces = 19;
			public readonly int GrossWeight = 20;
			public readonly int ChargeableWeight = 21;
			public readonly int WeightUnit = 22;
			public readonly int Volume = 23;
			public readonly int CurrencyCode = 24;
			public readonly int TotalTransaction = 25;

			public abstract int DestinationNettingCode { get; }
			public abstract int DestinationOfficeCode { get; }
		}

		public class AINVCDTFieldPositions : INVCDTFieldPositions
		{
			public readonly int AirlinePrefix = 10;
			public readonly int MAWBSerialNumber = 11;
			public readonly int FirstFlightDate = 14;
			public readonly int FirstFlightNumber = 15;
			public readonly int AirportOfOrigin = 17;
			public readonly int AirportOfDestination = 18;
			public readonly int ImportOrExportShipment = 26;

			public override int DestinationNettingCode { get { return 27; } }
			public override int DestinationOfficeCode { get { return 28; } }
		}

		public class MINVCDTFieldPositions : INVCDTFieldPositions
		{
			public readonly int SSLCode = 10;
			public readonly int OBLSerialNumber = 11;
			public readonly int EstimatedShippingDate = 14;
			public readonly int VesselAndVoyage = 15;
			public readonly int PortOfLoading = 17;
			public readonly int PortOfDischarge = 18;

			public override int DestinationNettingCode { get { return 26; } }
			public override int DestinationOfficeCode { get { return 27; } }
		}

		public class NINVCDTFieldPositions : INVCDTFieldPositions
		{
			public override int DestinationNettingCode { get { return 26; } }
			public override int DestinationOfficeCode { get { return 27; } }
		}

		public static class INVCDTFieldBoundaries
		{
			public const int ShipperNameMaxLength = 35;
			public const int ShipperAddressMaxLength = 35;
		}

		public const int AINVCDTFieldCount = 29;
		public const int MINVCDTFieldCount = 28;
		public const int NINVCDTFieldCount = 28;

		#endregion

		#region APRS

		public static class APRSFieldPositions
		{
			public const int TypeOfRecord = 0;
			public const int AirportOfOrigin = 1;
			public const int AirportOfDestination = 2;
			public const int AirlinePrefix = 3;
			public const int MAWBSerialNumber = 4;
			public const int DepartureDate = 5;
			public const int OriginFileReference = 6;
			public const int TotalNoOfHouseBills = 7;
			public const int TotalNoOfPieces = 8;
			public const int TotalChargeableWeight = 9;
			public const int MasterFreightCost = 10;
			public const int OtherMasterCosts = 11;
			public const int TotalHouseRevenue = 12;
			public const int TotalProfitShareDueDestination = 13;
			public const int Currency = 14;
		}

		public const int APRSFieldCount = 15;

		#endregion

		#region APRH

		public static class APRHFieldPositions
		{
			public const int HAWBSerialNumber = 0;
			public const int NoOfPieces = 1;
			public const int ChargeableWeight = 2;
			public const int CollectOrPrepaid = 3;
			public const int FreightRevenue = 4;
			public const int TotalCollectCharges = 5;
			public const int AllocatedFreightCost = 6;
			public const int AllocatedOtherCost = 7;
			public const int GrossProfit = 8;
			public const int DestinationProfitSplitPercentage = 9;
			public const int ProfitSplitDueDestination = 10;
			public const int Currency = 11;
		}

		public const int APRHFieldCount = 12;

		#endregion

		#region GSUM

		public static class GSUMFieldPositions
		{
			public const int AirMaritimeShipment = 0;
			public const int HouseBillSent = 1;
			public const int StatusCode = 2;
			public const int StatusDate = 3;
			public const int StatusTime = 4;
			public const int StatusInfo = 5;
			public const int OriginTrafficSystemFileNumber = 6;
			public const int HouseBillNumber = 7;
			public const int OriginOfficeCode = 8;
			public const int PredictedOrActual = 9;
		}

		public static class GSUMEventCodes
		{
			public const string PUP_PickedUpFromShipper = "PUP";
			public const string RSH_ReceivedFromShipper = "RSH";
			public const string CUS_ImportCustomsEntryMade = "CUS";
			public const string CLR_ImportCustomsCleared = "CLR";
			public const string OFD_OutForDelivery = "OFD";
			public const string POD_ProofOfDelivery = "POD";
			public const string DTO_DocumentTurnoverToBroker = "DTO";
		}

		public const int GSUMFieldCount = 10;

		#endregion

		#endregion

		#region Constants For Test
		internal class TestFieldPositions
		{
			public const int Field1 = 0;
			public const int Field2 = 1;
			public const int Field3 = 2;
		}

		internal const int TestFieldCount = 3;
		#endregion
	}
}
