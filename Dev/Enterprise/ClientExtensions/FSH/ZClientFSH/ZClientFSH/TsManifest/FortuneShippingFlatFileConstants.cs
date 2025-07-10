

namespace Enterprise.Client.FSH.TsManifest
{
	public abstract class Constants
	{
		#region RecordIds

		public abstract class RecordIDs
		{
			public const int HeadRecord = 0;
			public const int CargoRemarks = 48;
			public const int TrailerRecord = 99;
			public const int ContainerFieldsRecord = 51;
			public const int CargoFields = 41;
			public const int VslAndVoyFields = 10;
			public const int Hazardous = 43;
			public const int FirstRecordOf1BL = 12;
			public const int PlaceInfoOf1BL = 13;
			public const int RoutingInfoOf1BL = 14;
			public const int FreightAndChargesFields = 15;
			public const int ShipperFields = 16;
			public const int ConsigneeFields = 17;
			public const int Notify1PartyFields = 18;
			public const int Notify2PartyFields = 19;
			public const int Notify3PartyFields = 20;
			public const int BlClauses = 21;
			public const int DeliveryAddressFields = 22;
			public const int CargoDescription = 47;
			public const int HouseColoBillInfo = 61;
			public const int CargoMarks = 44;
		}

		#endregion

		#region CtnStatuses

		public abstract class CtnStatuses
		{
			public const string FCL = "F";
			public const string LCL = "L";
			public const string Empty = "E";
		}

		#endregion

		#region PaymentMethods

		public abstract class PaymentMethods
		{
			public const string Prepay = "P";
			public const string Collect = "C";
		}

		#endregion

		#region HeadRecord

		public abstract class HeadRecord
		{
			public const int RecordId = 0;
			public const int MessageType = 1;
			public const int FileDescription = 2;
			public const int FileFunction = 3;
			public const int SenderCode = 4;
			public const int ReceiverCode = 5;
			public const int FileCreateTime = 6;
		}
		#endregion

		#region CargoRemarks

		public abstract class CargoRemarks
		{
			public const int RecordId = 0;
			public const int Remarks = 1;
		}
		#endregion

		#region TrailerRecord

		public abstract class TrailerRecord
		{
			public const int RecordId = 0;
			public const int RecordTotalOfFile = 1;
		}
		#endregion

		#region ContainerFieldsRecord

		public abstract class ContainerFieldsRecord
		{
			public const int RecordId = 0;
			public const int CargoSequenceNo = 1;
			public const int CtnNo = 2;
			public const int SealNo1 = 3;
			public const int CtnSizeAndType = 4;
			public const int CtnStatus = 5;
			public const int CtnNumbersOfPackages = 6;
			public const int NetWeight = 7;
			public const int TareWeight = 8;
			public const int CtnCargoMeasurement = 9;
			public const int TemperatureId = 10;
			public const int TemperatureSetting = 11;
			public const int MinTemperature = 12;
			public const int MaxTemperature = 13;
			public const int Vent = 14;
			public const int ShipperOwnedUnit = 15;
			public const int CargoRecevingDate = 16;
			public const int SealNo2 = 17;
			public const int SealNo3 = 18;
			public const int SealNo4 = 19;
			public const int SealNo5 = 20;
			public const int SealNo6 = 21;
			public const int SealNo7 = 22;
			public const int SealNo8 = 23;
			public const int SealNo9 = 24;
		}
		#endregion

		#region CargoFields

		public abstract class CargoFields
		{
			public const int RecordId = 0;
			public const int CargoSequenceNo = 1;
			public const int CommodityGroup = 2;
			//public const int Commodity = 3;
			public const int NumberOfPackages = 3;
			public const int CargoType = 4;
			public const int PackagesDescription = 5;
			public const int CargoGrossWeight = 6;
			public const int CargoNetWeight = 7;
			public const int CargoGrossCube = 8;
			public const int CtnSizeAndType = 9;
			public const int Tariff = 10;
			public const int HsCode = 11;
		}
		#endregion

		#region VslAndVoyFields

		public abstract class VslAndVoyFields
		{
			public const int RecordId = 0;
			public const int VesselCode = 1;
			public const int Vessel = 2;
			public const int NationalityCode = 3;
			public const int Voyage = 4;
			public const int ShippingLine = 5;
		}
		#endregion

		#region Hazardous

		public abstract class Hazardous
		{
			public const int RecordId = 0;
			public const int ImoClass = 1;
			public const int ImoUnNo = 2;
			public const int ImoPage = 3;
			public const int FlashPoint = 4;
			public const int Ems = 5;
			public const int Mfag = 6;
			public const int CargoNetWeight = 7;
			public const int TechnicalDescription = 8;
			public const int PackageDescription = 9;
		}
		#endregion

		#region FirstRecordOf1BL

		public abstract class FirstRecordOf1BL
		{
			public const int RecordId = 0;
			public const int BLNo = 1;
			public const int PreVesselCode = 2;
			public const int PreVessel = 3;
			public const int PreVoyage = 4;
			public const int PlaceCodeOfReceipt = 5;
			public const int PlaceOfReceipt = 6;
			public const int LoadPortCode = 7;
			public const int LoadPort = 8;
			public const int BLCYCFSItem = 9;
			public const int PrepaidOrCollect = 10;
			public const int LoadDate = 11;
			public const int QuarantineCoding = 12;
			public const int DateOfIssue = 13;
			public const int Currency = 14;
			public const int ExchangeRate = 15;
			public const int CargoStatus = 16;
			public const int ShippersReference = 17;
			public const int SlotOperator = 18;
			public const int TradeCode = 19;
			public const int BlOtherRef = 20;
			public const int BlType = 21;
			public const int PayableAt = 22;
			public const int PayerCode = 23;
			public const int NoOfCopyBl = 24;
			public const int NoOfOriginalBl = 25;
			public const int CustomsClearedPlace = 26;
			public const int SlotShare = 27;
		}
		#endregion

		#region PlaceInfoOf1BL

		public abstract class PlaceInfoOf1BL
		{
			public const int RecordId = 0;
			public const int PortOfDischargeCode = 1;
			public const int PortOfDischarge = 2;
			public const int PlaceOfDeliveryCode = 3;
			public const int PlaceOfDelivery = 4;
			public const int PlaceOfDestinationCode = 5;
			public const int PlaceOfDestination = 6;
			public const int PlaceCodeOfBLIssue = 7;
			public const int PlaceOfBLIssue = 8;
			public const int TransferPort1 = 9;
			public const int TransferPort2 = 10;
			public const int TransferPort3 = 11;
			public const int TransferPort4 = 12;
		}
		#endregion

		#region RoutingInfoOf1BL

		public abstract class RoutingInfoOf1BL
		{
			public const int RecordId = 0;
			public const int PortFrom = 1;
			public const int FromDate = 2;
			public const int PortTo = 3;
			public const int ToDate = 4;
			public const int VesselCode = 5;
			public const int Voyage = 6;
			public const int Seq = 7;
			public const int Mot = 8;
		}
		#endregion

		#region FreightAndChargesFields

		public abstract class FreightAndChargesFields
		{
			public const int RecordId = 0;
			public const int ChargeCode = 1;
			public const int FrChRemark = 2;
			public const int PayableAtPop = 3;
			public const int PayableAt = 4;
			public const int Quantity = 5;
			public const int Currency = 6;
			public const int ChargeRate = 7;
			public const int UnitOfQuantity = 8;
			public const int Amount = 9;
			public const int PrepaidOrCollect = 10;
			public const int CtnSizeAndType = 11;
			public const int PayerCode = 12;
			public const int IgCode = 13;
		}
		#endregion

		#region BlClauses

		public abstract class BlClauses
		{
			public const int RecordId = 0;
			public const int ClauseCode = 1;
			public const int BlClauseText = 2;
		}
		#endregion

		#region AddressFields

		public abstract class AddressFields
		{
			public const int RecordId = 0;
			public const int AddressCode = 1;
			public const int Address1 = 2;
			public const int Address2 = 3;
			public const int Address3 = 4;
			public const int Address4 = 5;
			public const int Address5 = 6;
		}
		#endregion

		#region CargoDescription

		public abstract class CargoDescription
		{
			public const int RecordId = 0;
			public const int Description = 1;
		}
		#endregion

		#region HouseColoBillInfo

		public abstract class HouseColoBillInfo
		{
			public const int RecordId = 0;
			public const int HouseBillNo = 1;
			public const int NoOfPackages = 2;
			public const int ColoBillNo = 3;
		}
		#endregion
	}
}
