namespace Enterprise.Customs.CN.DataTransfer
{
	#region SuppressResourceStringsCheckRegion
	public static class Constants
	{
		public static class DocumentaryAddressTypes
		{
			public const string Buyer = "Buyer";
		}

		public static class AddInfoKeys
		{
			public static class EntryInstruction
			{
				public const string MarksAndNumbers = "MarksAndNumbers";
				public const string CustomsMessageRemarks = "CustomsMessageRemarks";
				public const string BillOfLading = "BillOfLading";
				public const string BillOfLadingDate = "BillOfLadingDate";
				public const string CIQRequires = "CIQRequires";
			}

			public static class InvoiceLine
			{
				public const string CIQIngredient = "CIQIngredient";
			}

			public static class JobDeclaration
			{
				public const string BrokerNumber = "BrokerNumber";
				public const string BrokerName = "BrokerName";
				public const string OperatorCardID = "OperatorCardID";
				public const string OperatorName = "OperatorName";
				public const string DestinationParty = "DestinationParty";
			}

			public static class EntryHeader
			{
				public const string FreightFeeCurrencyCode = "FreightFeeCurrencyCode";
				public const string FreightFeeMarkCode = "FreightFeeMarkCode";
				public const string FreightFeeAmount = "FreightFeeAmount";
				public const string InsuranceFeeCurrencyCode = "InsuranceFeeCurrencyCode";
				public const string InsuranceFeeMarkCode = "InsuranceFeeMarkCode";
				public const string InsuranceFeeAmount = "InsuranceFeeAmount";
				public const string OtherFeeCurrencyCode = "OtherFeeCurrencyCode";
				public const string OtherFeeMarkCode = "OtherFeeMarkCode";
				public const string OtherFeeAmount = "OtherFeeAmount";
				public const string OverseasPartyCode = "OverseasPartyCode";
				public const string MarksAndNumbers = "MarksAndNumbers";
				public const string BillOfLading = "BillOfLading";
				public const string VesselName = "VesselName";
				public const string Voyage = "Voyage";
				public const string Remarks = "Remarks";
			}

			public static class EntryLine
			{
				public const string NameOfGoods = "NameOfGoods";
				public const string GoodsSpecModel = "GoodsSpecModel";
				public const string UnitPrice = "UnitPrice";
				public const string TotalPrice = "TotalPrice";
				public const string CurrencyCode = "CurrencyCode";
			}
		}

		public static class EntryInstruction
		{
			public static class Codes
			{
				public const string EIA = "EIA";
			}

			public static class Descriptions
			{
				public const string EIA = "Entry Instruction Attachment";
			}
		}

		public static class Attachment
		{
			public static class Keys
			{
				public const string AttachmentFileName = "FileName";
				public const string AttachmentType = "AttachmentType";
				public const string AttachmentNumber = "AttachmentNumber";
				public const string EntryLineLinks = nameof(EntryLineLinks);
			}
		}

		public static class Universal
		{
			public static class ContextType
			{
				public const string CustomsDeclarationNumber = "CustomsDeclarationNumber";
				public const string CustomsDeclarationDate = "CustomsDeclarationDate";
				public const string CustomsOffice = "CustomsOffice";
				public const string DeclarationUnifiedNumber = "DeclarationUnifiedNumber";
				public const string EntryStatus = "EntryStatus";
				public const string ImportExportDate = "ImportExportDate";
				public const string LocalReferenceNumber = "LocalReferenceNumber";
				public const string Note = "Note";
				public const string ResponseCode = "ResponseCode";
				public const string ResponseDetail = "ResponseDetail";
			}

			public static class DataProvider
			{
				public const string ChinaSingleWindow = "CSW";
			}

			public const string EventTime = "EventTime";
		}
	}
	#endregion
}
