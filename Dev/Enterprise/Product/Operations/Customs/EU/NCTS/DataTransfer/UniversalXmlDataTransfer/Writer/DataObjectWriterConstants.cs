using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.DataTransfer
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	public static class DataObjectWriterConstants
	{
		public static class Header
		{
			public static class AddInfo
			{
				public const string CustomReferenceType = "COM";
			}
		}

		public static class MovementHeader
		{
			public static class LocationDescriptions
			{
				public const string AuthorisedLocationOfGoodsCode = "Authorised Location of Goods Code";
				public const string AgreedLocationOfGoodsCode = "Agreed Location of Goods Code";
				public const string AgreedLocationOfGoods = "Agreed Location of Goods";
				public const string CustomsSubPlace = "Customs Sub Place";
			}
		}

		public static class DepartureMovementHeader
		{
			public static class AddInfo
			{
				public const string ControlResultCode = "ControlResultCode";
				public const string ControlResultDateLimit = "ControlResultDateLimit";
				public const string SecurityIndicator = "SecurityIndicator";
				public const string TransportModeAtBorder = "TransportModeAtBorder";
				public const string Box18TransportID = "Box18TransportID";
				public const string Box18TransportNationality = "Box18TransportNationality";
				public const string IsSimplifiedNctsProcedureA3 = NctsControlResult.Codes.AuthorizedTrader;
				public const string IsSimplifiedProcedure = "IsSimplifiedProcedure";
				public const string ReducedDateSetIndicator = "ReducedDatasetIndicator";
				public const string SpecificCircumstance = "SpecificCircumstance";
				public const string BM_ConveyanceNumber = nameof(NctsDepartureMovementHeader.BM_ConveyanceNumber);
				public const string BM_CustomsOfficeAtBorder = nameof(NctsDepartureMovementHeader.BM_CustomsOfficeAtBorder);
				public const string TirCarnetNumber = "TirCarnetNumber";
			}
		}

		public static class GoodsItem
		{
			public static class AddInfo
			{
				public const string DeclarationType = "DeclarationType";
				public const string TransportChargesMoP = "TransportChargesMoP";
				public const string CountryOfDestination = "CountryOfDestination";
				public const string CommercialReferenceNumber = "CommercialReferenceNumber";
				public const string UNDangerousGoodsCode = "UNDangerousGoodsCode";
				public const string UNDangerousGoodsStandard = "UNDangerousGoodsStandard";
			}
		}

		public static class ArrivalMovementHeader
		{
			public static class AddInfo
			{
				public const string SimplifiedArrivalProcedureFlag = "SimplifiedArrivalProcedureFlag";
				public const string IsSimplifiedArrivalProcedure = "1";
			}
		}
	}
}
