namespace Enterprise.Client.MFI.CaroTrans
{
	#region Constants

	public static class Constants
	{
		#region ContainerRecord

		public static class ContainerRecord
		{
			public const int FieldLength = 22;

			public const int RecordType = 0;
			public const int ShipCode = 1;
			public const int ContainerId = 2;
			public const int ContainerNumber = 3;
			public const int SailingNumber = 4;
			public const int Vessel = 5;
			public const int Voyage = 6;
			public const int NumberOfBills = 7;
			public const int ContainerSize = 8;
			public const int OBL = 9;
			public const int ITDate = 10;
			public const int SailDate = 11;
			public const int ArrivalDate = 12;
			public const int PortOfLoading = 13;
			public const int PortOfDischarge = 14;
			public const int PlaceOfReceipt = 15;
			public const int PlaceOfDelivery = 16;
			public const int Pieces = 17;
			public const int Weight = 18;
			public const int CBM = 19;
			public const int Curr = 20;
			public const int ExchRate = 21;
		}

		#endregion

		#region BillRecord

		public static class BillRecord
		{
			public const int FieldLength = 44;

			public const int RecordType = 0;
			public const int ContainerId = 1;
			public const int ContainerNumber = 2;
			public const int BOL = 3;
			public const int Vessel = 4;
			public const int Voyage = 5;
			public const int ForwardingAgent = 6;
			public const int ShipperCode = 7;
			public const int ShipperName = 8;
			public const int ShipperAdd1 = 9;
			public const int ShipperAdd2 = 10;
			public const int ShipperAdd3 = 11;
			public const int ShipperCity = 12;
			public const int ShipperState = 13;
			public const int ShipperZip = 14;
			public const int ShipperPhone = 15;
			public const int ShipperFax = 16;
			public const int ConsigneeCode = 17;
			public const int ConsigneeName = 18;
			public const int ConsigneeAdd1 = 19;
			public const int ConsigneeAdd2 = 20;
			public const int ConsigneeAdd3 = 21;
			public const int ConsigneeCity = 22;
			public const int ConsigneeState = 23;
			public const int ConsigneeZip = 24;
			public const int ConsigneePhone = 25;
			public const int ConsigneeFax = 26;
			public const int NotifyCode = 27;
			public const int NotifyName = 28;
			public const int NotifyAdd1 = 29;
			public const int NotifyAdd2 = 30;
			public const int NotifyAdd3 = 31;
			public const int NotifyCity = 32;
			public const int NotifyState = 33;
			public const int NotifyZip = 34;
			public const int NotifyPhone = 35;
			public const int NotifyFax = 36;
			public const int NumberBillBodyLines = 37;
			public const int NumberChargeLines = 38;
			public const int PlaceOfReceipt = 39;
			public const int PlaceOfDelivery = 40;
			public const int Terms = 41;
			public const int CTIAgentCode = 42;
			public const int MFIAgentCode = 43;

			public const int AddressLineMaxLength = 35;
		}

		#endregion

		#region BillBodyRecord

		public static class BillBodyRecord
		{
			public const int FieldLength = 10;

			public const int RecordType = 0;
			public const int ContainerId = 1;
			public const int ContainerNumber = 2;
			public const int BOL = 3;
			public const int SequenceNumber = 4;
			public const int Marks = 5;
			public const int MarksMaxLength = 14;
			public const int Packages = 6;
			public const int Description = 7;
			public const int DescriptionMaxLength = 30;
			public const int Weight = 8;
			public const int CBM = 9;
		}

		#endregion

		#region ChargesRecord

		public static class ChargesRecord
		{
			public const int FieldLength = 10;

			public const int RecordType = 0;
			public const int ContainerId = 1;
			public const int ContainerNumber = 2;
			public const int BOL = 3;
			public const int SequenceNumber = 4;
			public const int Rate = 5;
			public const int Basis = 6;
			public const int Description = 7;
			public const int PrepaidAmaount = 8;
			public const int CollectAmount = 9;
		}

		#endregion

		#region TotalsRecord

		public static class TotalsRecord
		{
			public const int FieldLength = 10;

			public const int RecordType = 0;
			public const int Number01Records = 1;
			public const int Number02Records = 2;
			public const int Number03Records = 3;
			public const int Number04Records = 4;
		}

		#endregion

		#region RecordTypes

		public static class RecordTypes
		{
			public const string ContainerRec = "01";
			public const string BillRec = "02";
			public const string BillBodyRec = "03";
			public const string ChargesRec = "04";
			public const string TotalsRec = "05";
		}

		#endregion

		#region OrderRecord

		public static class OrderRecord
		{
			public const int FieldsCount = 20;

			public const int RecordID = 0;
			public const int BLNumber = 1;
			public const int Reference = 2;
			public const int Origin = 3;
			public const int Dest = 4;
			public const int BookDate = 5;
			public const int ConsigneeName = 6;
			public const int DockReceiptsPieces = 7;
			public const int DockReceiptsWeight = 8;
			public const int DockReceiptsCubic = 9;
			public const int DockReceipted = 10;
			public const int RepoDispatch = 11;
			public const int RepoDispatchLocation = 12;
			public const int RepoArrive = 13;
			public const int RepoArriveLocation = 14;
			public const int VesselName = 15;
			public const int VoyageNo = 16;
			public const int ContainerNo = 17;
			public const int OnBoardDate = 18;
			public const int ArrivalDate = 19;
		}

		#endregion

		#region OrderRecordTypes

		public static class OrderRecordTypes
		{
			public const string Order = "01";
			public const string AuditTotals = "99";
		}

		#endregion

		#region ShipmentRecord

		public static class ShipmentRecord
		{
			public const int FieldsCount = 10;
			public static readonly int[] FieldsLength = new int[FieldsCount] { 2, 13, 20, 22, 5, 15, 8, 8, 8, 8 };

			public const int RecordID = 0;
			public const int BLNumber = 1;
			public const int AgentReference = 2;
			public const int Vessel = 3;
			public const int Voyage = 4;
			public const int Container = 5;
			public const int ArrivalDate = 6;
			public const int AvailableDate = 7;
			public const int DeliveryPickupDate = 8;
			public const int CustomsClearanceDate = 9;
		}

		#endregion

		#region ShipmentRecordTypes

		public static class ShipmentRecordTypes
		{
			public const string Shipment = "01";
			public const string AuditTotals = "99";
		}

		#endregion

		#region FileExtensions

		public const string CaroTransOrderClientSpecificFileExtension = "CTI";

		#endregion

		public const string ShipmentDataExportFile = "CaroTransShipmentDataExportFile";
		public const string DateFormat = "yyyyMMdd";
		public const string FileNamePrefix = "CO";
		public const string CargoTransDefaultFileExtension = "mfd";
		public const string LongDateTimeFormat = "yyyyMMddHHmmss";
		public const string CaroTrans = "CaroTrans_";

		#region InternalTestVariable
		internal const string InternalDefaultCaroTransExportFileWithExtensionTest = ShipmentDataExportFile + "." + CargoTransDefaultFileExtension;
		#endregion
	}
	#endregion
}
