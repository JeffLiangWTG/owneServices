namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class ShipnetConstants
	{
		#region Codes

		public static class Codes
		{
			public static class RecordID
			{
				public const string FileHeader = "HDR";
				public const string BookingHeader = "BHD";
				public const string ShipperDetails = "SPR";
				public const string ConsigneeDetails = "CNE";
				public const string NotifyPartyDetails = "NP1";
				public const string UltimateConsigneeDetails = "UCN";
				public const string FreightForwarderDetails = "FWD";
				public const string CustomerDetails = "CUS";
				public const string CargoGroup = "CGR";
				public const string CargoGroupDescriptions = "CGD";
				public const string CargoGroupItems = "CIT";
				public const string CargoItemDescriptions = "CID";
				public const string EquipmentDetails = "EQD";
				public const string ChargeDetail = "CHL";
				public const string Summary = "CNT";
			}

			public static class EDIFACTCargoType
			{
				public const string FCL = "1";
				public const string LCLFCL = "2";
				public const string LCLLCL = "3";
				public const string BB = "5";
				public const string U = "9";
			}

			public static class EmptyFull
			{
				public const string Empty = "4";
				public const string Full = "5";
			}

			public static class PaymentType
			{
				public const string Prepaid = "1";
				public const string Collect = "2";
			}
		}

		#endregion

		#region FieldLengths

		public static class FieldsCount
		{
			public const int FileHeader = 24;
			public const int BookingHeader = 30;
			public const int PartyDetails = 11;
			public const int UltimateConsigneeDetails = 11;
			public const int CargoGroup = 12;
			public const int CargoGroupItems = 21;
			public const int CargoDescriptions = 4;
			public const int EquipmentDetails = 16;
			public const int ChargeDetail = 5;
			public const int Summary = 10;
		}

		#endregion

		#region Common

		public static class Common
		{
			public const int RecordID = 0;
			public const int RecordIDMaxLength = 3;
			public const int RecordIDPosition = 0;
		}

		#endregion

		#region FileHeader

		public static class FileHeader
		{
			public const int Context = 1;
			public const int ContextMaxLength = 10;
			public const int ContextPosition = 3;
			public const int SenderID = 2;
			public const int SenderIDMaxLength = 35;
			public const int SenderIDPosition = 13;
			public const int RecipientID = 3;
			public const int RecipientIDMaxLength = 35;
			public const int RecipientIDPosition = 48;
			public const int MessageType = 4;
			public const int MessageTypeMaxLength = 10;
			public const int MessageTypePosition = 83;
			public const int MessageVersion = 5;
			public const int MessageVersionMaxLength = 5;
			public const int MessageVersionPosition = 93;
			public const int VesselCode = 6;
			public const int VesselCodeMaxLength = 10;
			public const int VesselCodePosition = 98;
			public const int VesselName = 7;
			public const int VesselNameMaxLength = 35;
			public const int VesselNamePosition = 108;
			public const int VesselLloyds = 8;
			public const int VesselLloydsMaxLength = 10;
			public const int VesselLloydsPosition = 143;
			public const int VesselOperator = 9;
			public const int VesselOperatorMaxLength = 10;
			public const int VesselOperatorPosition = 153;
			public const int Carrier = 10;
			public const int CarrierMaxLength = 35;
			public const int CarrierPosition = 163;
			public const int ModeOfTransport = 11;
			public const int ModeOfTransportMaxLength = 2;
			public const int ModeOfTransportPosition = 198;
			public const int VoyageNumber = 12;
			public const int VoyageNumberMaxLength = 10;
			public const int VoyageNumberPosition = 200;
			public const int AddTareWeight = 13;
			public const int AddTareWeightMaxLength = 1;
			public const int AddTareWeightPosition = 210;
			public const int LastPortOfLoadCode = 14;
			public const int LastPortOfLoadCodeMaxLength = 5;
			public const int LastPortOfLoadCodePosition = 211;
			public const int LastPortOfLoadName = 15;
			public const int LastPortOfLoadNameMaxLength = 35;
			public const int LastPortOfLoadNamePosition = 216;
			public const int SailDate = 16;
			public const int SailDateMaxLength = 8;
			public const int SailDatePosition = 251;
			public const int SailTime = 17;
			public const int SailTimeMaxLength = 6;
			public const int SailTimePosition = 259;
			public const int FirstPODCode = 18;
			public const int FirstPODCodeMaxLength = 5;
			public const int FirstPODCodePosition = 265;
			public const int FirstPODName = 19;
			public const int FirstPODNameMaxLength = 35;
			public const int FirstPODNamePosition = 270;
			public const int ArrivalDate = 20;
			public const int ArrivalDateMaxLength = 8;
			public const int ArrivalDatePosition = 305;
			public const int ArrivalTime = 21;
			public const int ArrivalTimeMaxLength = 6;
			public const int ArrivalTimePosition = 313;
			public const int BerthCTO = 22;
			public const int BerthCTOMaxLength = 15;
			public const int BerthCTOPosition = 319;
			public const int SendersABN = 23;
			public const int SendersABNMaxLength = 11;
			public const int SendersABNPosition = 334;
		}

		#endregion

		#region BookingHeader

		public static class BookingHeader
		{
			public const int ReferenceNo = 1;
			public const int ReferenceNoMaxLength = 20;
			public const int ReferenceNoPosition = 3;
			public const int SecondaryReference = 2;
			public const int SecondaryReferenceMaxLength = 20;
			public const int SecondaryReferencePosition = 23;
			public const int AcceptanceLocationCode = 3;
			public const int AcceptanceLocationCodeMaxLength = 10;
			public const int AcceptanceLocationCodePosition = 43;
			public const int AcceptanceLocationName = 4;
			public const int AcceptanceLocationNameMaxLength = 35;
			public const int AcceptanceLocationNamePosition = 53;
			public const int AcceptancePortCode = 5;
			public const int AcceptancePortCodeMaxLength = 5;
			public const int AcceptancePortCodePosition = 88;
			public const int LoadPortCode = 6;
			public const int LoadPortCodeMaxLength = 5;
			public const int LoadPortCodePosition = 93;
			public const int TranshipmentPortCode = 7;
			public const int TranshipmentPortCodeMaxLength = 5;
			public const int TranshipmentPortCodePosition = 98;
			public const int DischargePortCode = 8;
			public const int DischargePortCodeMaxLength = 5;
			public const int DischargePortCodePosition = 103;
			public const int DeliveryPortCode = 9;
			public const int DeliveryPortCodeMaxLength = 5;
			public const int DeliveryPortCodePosition = 108;
			public const int DeliveryLocationCode = 10;
			public const int DeliveryLocationCodeMaxLength = 10;
			public const int DeliveryLocationCodePosition = 113;
			public const int DeliveryLocationName = 11;
			public const int DeliveryLocationNameMaxLength = 35;
			public const int DeliveryLocationNamePosition = 123;
			public const int OriginCountryCode = 12;
			public const int OriginCountryCodeMaxLength = 5;
			public const int OriginCountryCodePosition = 158;
			public const int OriginCountryName = 13;
			public const int OriginCountryNameMaxLength = 35;
			public const int OriginCountryNamePosition = 163;
			public const int DestinationCountryCode = 14;
			public const int DestinationCountryCodeMaxLength = 2;
			public const int DestinationCountryCodePosition = 198;
			public const int DestinationCountryName = 15;
			public const int DestinationCountryNameMaxLength = 35;
			public const int DestinationCountryNamePosition = 200;
			public const int OriginTerms = 16;
			public const int OriginTermsMaxLength = 5;
			public const int OriginTermsPosition = 235;
			public const int DestinationTerms = 17;
			public const int DestinationTermsMaxLength = 5;
			public const int DestinationTermsPosition = 240;
			public const int PaymentTerms = 18;
			public const int PaymentTermsMaxLength = 1;
			public const int PaymentTermsPosition = 245;
			public const int EDIFACTCargoType = 19;
			public const int EDIFACTCargoTypeMaxLength = 1;
			public const int EDIFACTCargoTypePosition = 246;
			public const int FAKIndicator = 20;
			public const int FAKIndicatorMaxLength = 10;
			public const int FAKIndicatorPosition = 247;
			public const int ETA = 21;
			public const int ETAMaxLength = 8;
			public const int ETAPosition = 257;
			public const int ETD = 22;
			public const int ETDMaxLength = 8;
			public const int ETDPosition = 265;
			public const int UnderBond = 23;
			public const int UnderBondMaxLength = 1;
			public const int UnderBondPosition = 273;
			public const int OriginPremiseCode = 24;
			public const int OriginPremiseCodeMaxLength = 10;
			public const int OriginPremiseCodePosition = 274;
			public const int OriginPremiseName = 25;
			public const int OriginPremiseNameMaxLength = 35;
			public const int OriginPremiseNamePosition = 284;
			public const int DestPremiseCode = 26;
			public const int DestPremiseCodeMaxLength = 10;
			public const int DestPremiseCodePosition = 319;
			public const int DestPremiseName = 27;
			public const int DestPremiseNameMaxLength = 35;
			public const int DestPremiseNamePosition = 329;
			public const int Berth = 28;
			public const int BerthMaxLength = 15;
			public const int BerthPosition = 364;
			public const int CAN = 29;
			public const int CANMaxLength = 15;
			public const int CANPosition = 379;
		}

		#endregion

		#region PartyDetails

		public static class PartyDetails
		{
			public const int PartyCode = 1;
			public const int PartyCodeMaxLength = 15;
			public const int PartyCodePosition = 3;
			public const int PartyName = 2;
			public const int PartyNameMaxLength = 35;
			public const int PartyNamePosition = 18;
			public const int PartyAddress1 = 3;
			public const int PartyAddress1MaxLength = 35;
			public const int PartyAddress1Position = 53;
			public const int PartyAddress2 = 4;
			public const int PartyAddress2MaxLength = 35;
			public const int PartyAddress2Position = 88;
			public const int PartyAddress3 = 5;
			public const int PartyAddress3MaxLength = 35;
			public const int PartyAddress3Position = 123;
			public const int PartyAddress4 = 6;
			public const int PartyAddress4MaxLength = 35;
			public const int PartyAddress4Position = 158;
			public const int PartyAddress5 = 7;
			public const int PartyAddress5MaxLength = 35;
			public const int PartyAddress5Position = 193;
			public const int PartyACNNumber = 8;
			public const int PartyACNNumberMaxLength = 20;
			public const int PartyACNNumberPosition = 228;
			public const int PartyFwdrRegNo = 9;
			public const int PartyFwdrRegNoMaxLength = 20;
			public const int PartyFwdrRegNoPosition = 248;
			public const int PartyReference = 10;
			public const int PartyReferenceMaxLength = 20;
			public const int PartyReferencePosition = 268;
		}

		#endregion

		#region UltimateConsigneeDetails

		public static class UltimateConsigneeDetails
		{
			public const int PartyACNNumberMaxLength = 10;
			public const int PartyFwdrRegNoPosition = 238;
			public const int PartyReferencePosition = 258;
		}

		#endregion

		#region CargoGroup

		public static class CargoGroup
		{
			public const int CGRSequence = 1;
			public const int CGRSequenceMaxLength = 4;
			public const int CGRSequencePosition = 3;
			public const int NumberOfContainers = 2;
			public const int NumberOfContainersMaxLength = 10;
			public const int NumberOfContainersPosition = 7;
			public const int SizeType = 3;
			public const int SizeTypeMaxLength = 5;
			public const int SizeTypePosition = 17;
			public const int EmptyFull = 4;
			public const int EmptyFullMaxLength = 1;
			public const int EmptyFullPosition = 22;
			public const int TotalGrossWeightKGS = 5;
			public const int TotalGrossWeightKGSMaxLength = 15;
			public const int TotalGrossWeightKGSPosition = 23;
			public const int TotalNetWeightKGS = 6;
			public const int TotalNetWeightKGSMaxLength = 15;
			public const int TotalNetWeightKGSPosition = 38;
			public const int TotalGrossCubeCBM = 7;
			public const int TotalGrossCubeCBMMaxLength = 15;
			public const int TotalGrossCubeCBMPosition = 53;
			public const int TotalNetCubeCBM = 8;
			public const int TotalNetCubeCBMMaxLength = 15;
			public const int TotalNetCubeCBMPosition = 68;
			public const int TotalNoOfPackages = 9;
			public const int TotalNoOfPackagesMaxLength = 10;
			public const int TotalNoOfPackagesPosition = 83;
			public const int TotalContainerTare = 10;
			public const int TotalContainerTareMaxLength = 15;
			public const int TotalContainerTarePosition = 93;
			public const int CAN = 11;
			public const int CANMaxLength = 15;
			public const int CANPosition = 108;
		}

		#endregion

		#region CargoDescriptions

		public static class CargoDescriptions
		{
			public const int CGRSequence = 1;
			public const int CGRSequenceMaxLength = 4;
			public const int CGRSequencePosition = 3;
			public const int Marks = 2;
			public const int MarksMaxLength = 250;
			public const int MarksPosition = 7;
			public const int Description = 3;
			public const int DescriptionMaxLength = 250;
			public const int DescriptionPosition = 257;
		}

		#endregion

		#region CargoGroupItems

		public static class CargoGroupItems
		{
			public const int CGRSequence = 1;
			public const int CGRSequenceMaxLength = 4;
			public const int CGRSequencePosition = 3;
			public const int NumberOfPackages = 2;
			public const int NumberOfPackagesMaxLength = 10;
			public const int NumberOfPackagesPosition = 7;
			public const int TypeOfPackages = 3;
			public const int TypeOfPackagesMaxLength = 5;
			public const int TypeOfPackagesPosition = 17;
			public const int CargoCode = 4;
			public const int CargoCodeMaxLength = 15;
			public const int CargoCodePosition = 22;
			public const int CargoGroup = 5;
			public const int CargoGroupMaxLength = 15;
			public const int CargoGroupPosition = 37;
			public const int CargoDescription = 6;
			public const int CargoDescriptionMaxLength = 30;
			public const int CargoDescriptionPosition = 52;
			public const int GrossWeightKGS = 7;
			public const int GrossWeightKGSMaxLength = 15;
			public const int GrossWeightKGSPosition = 82;
			public const int NetWeightKGS = 8;
			public const int NetWeightKGSMaxLength = 15;
			public const int NetWeightKGSPosition = 97;
			public const int GrossCubeCBM = 9;
			public const int GrossCubeCBMMaxLength = 15;
			public const int GrossCubeCBMPosition = 112;
			public const int NetCubeCBM = 10;
			public const int NetCubeCBMMaxLength = 15;
			public const int NetCubeCBMPosition = 127;
			public const int ECNNumber = 11;
			public const int ECNNumberMaxLength = 15;
			public const int ECNNumberPosition = 142;
			public const int Hazardous = 12;
			public const int HazardousMaxLength = 1;
			public const int HazardousPosition = 157;
			public const int HazardousClass = 13;
			public const int HazardousClassMaxLength = 7;
			public const int HazardousClassPosition = 158;
			public const int UNNumber = 14;
			public const int UNNumberMaxLength = 4;
			public const int UNNumberPosition = 165;
			public const int Page = 15;
			public const int PageMaxLength = 7;
			public const int PagePosition = 169;
			public const int Flashpoint = 16;
			public const int FlashpointMaxLength = 11;
			public const int FlashpointPosition = 176;
			public const int TemperatureType = 17;
			public const int TemperatureTypeMaxLength = 1;
			public const int TemperatureTypePosition = 187;
			public const int Fumigated = 18;
			public const int FumigatedMaxLength = 1;
			public const int FumigatedPosition = 188;
			public const int ReportableDocument = 19;
			public const int ReportableDocumentMaxLength = 1;
			public const int ReportableDocumentPosition = 189;
			public const int PersonalEffects = 20;
			public const int PersonalEffectsMaxLength = 1;
			public const int PersonalEffectsPosition = 190;
		}

		#endregion

		#region EquipmentDetails

		public static class EquipmentDetails
		{
			public const int CGRSequence = 1;
			public const int CGRSequenceMaxLength = 4;
			public const int CGRSequencePosition = 3;
			public const int EquipmentNumber = 2;
			public const int EquipmentNumberMaxLength = 12;
			public const int EquipmentNumberPosition = 7;
			public const int SizeType = 3;
			public const int SizeTypeMaxLength = 5;
			public const int SizeTypePosition = 19;
			public const int EmptyFull = 4;
			public const int EmptyFullMaxLength = 1;
			public const int EmptyFullPosition = 24;
			public const int TotalGrossWeightKGS = 5;
			public const int TotalGrossWeightKGSMaxLength = 15;
			public const int TotalGrossWeightKGSPosition = 25;
			public const int TotalNetWeightKGS = 6;
			public const int TotalNetWeightKGSMaxLength = 15;
			public const int TotalNetWeightKGSPosition = 40;
			public const int TotalGrossCubeCBM = 7;
			public const int TotalGrossCubeCBMMaxLength = 15;
			public const int TotalGrossCubeCBMPosition = 55;
			public const int TotalNetCubeCBM = 8;
			public const int TotalNetCubeCBMMaxLength = 15;
			public const int TotalNetCubeCBMPosition = 70;
			public const int TotalTareWeight = 9;
			public const int TotalTareWeightMaxLength = 15;
			public const int TotalTareWeightPosition = 85;
			public const int NoOfPackages = 10;
			public const int NoOfPackagesMaxLength = 10;
			public const int NoOfPackagesPosition = 100;
			public const int Packages = 11;
			public const int PackagesMaxLength = 5;
			public const int PackagesPosition = 110;
			public const int SealNumber1 = 12;
			public const int SealNumber1MaxLength = 15;
			public const int SealNumber1Position = 115;
			public const int SealNumber2 = 13;
			public const int SealNumber2MaxLength = 15;
			public const int SealNumber2Position = 130;
			public const int Hazardous = 14;
			public const int HazardousMaxLength = 1;
			public const int HazardousPosition = 145;
			public const int ShippersOwn = 15;
			public const int ShippersOwnMaxLength = 1;
			public const int ShippersOwnPosition = 146;
		}

		#endregion

		#region Charge Detail

		public static class ChargeDetail
		{
			public const int ChargeCode = 1;
			public const int ChargeCodeMaxLength = 10;
			public const int ChargeCodePosition = 3;
			public const int PrepaidCollect = 2;
			public const int PrepaidCollectMaxLength = 1;
			public const int PrepaidCollectPosition = 13;
			public const int Currency = 3;
			public const int CurrencyMaxLength = 3;
			public const int CurrencyPosition = 14;
			public const int Amount = 4;
			public const int AmountMaxLength = 21;
			public const int AmountPosition = 17;
		}

		#endregion

		#region Summary

		public static class Summary
		{
			public const int NumberOfMTContainers = 1;
			public const int NumberOfMTContainersMaxLength = 10;
			public const int NumberOfMTContainersPosition = 3;
			public const int NumberOfFullContainers = 2;
			public const int NumberOfFullContainersMaxLength = 10;
			public const int NumberOfFullContainersPosition = 13;
			public const int TotalGrossWeightKGS = 3;
			public const int TotalGrossWeightKGSMaxLength = 15;
			public const int TotalGrossWeightKGSPosition = 23;
			public const int TotalNetWeightKGS = 4;
			public const int TotalNetWeightKGSMaxLength = 15;
			public const int TotalNetWeightKGSPosition = 38;
			public const int FileTotalTareWeight = 5;
			public const int FileTotalTareWeightMaxLength = 15;
			public const int FileTotalTareWeightPosition = 53;
			public const int TotalGrossCubeCBM = 6;
			public const int TotalGrossCubeCBMMaxLength = 15;
			public const int TotalGrossCubeCBMPosition = 68;
			public const int TotalNetCubeCBM = 7;
			public const int TotalNetCubeCBMMaxLength = 15;
			public const int TotalNetCubeCBMPosition = 83;
			public const int TotalNoOfPackages = 8;
			public const int TotalNoOfPackagesMaxLength = 10;
			public const int TotalNoOfPackagesPosition = 98;
			public const int TotalNumberOfRecords = 9;
			public const int TotalNumberOfRecordsMaxLength = 10;
			public const int TotalNumberOfRecordsPosition = 108;
		}

		#endregion
	}
}
