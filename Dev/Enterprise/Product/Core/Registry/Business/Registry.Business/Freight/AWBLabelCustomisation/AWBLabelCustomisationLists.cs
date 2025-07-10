using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class AWBLabelOptionalInformationList : UntranslatableCodeDescriptionPairList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public static class Codes
		{
			public const string Blank = "";

			public const string ConsolOrigin = "Origin";
			public const string ConsolDestination = "Destination";
			public const string ConsolPieceNumber = "Consol Piece Number";
			public const string ConsolPieceCount = "Consol Total No. of Pieces";
			public const string ConsolPieceNumberOfCount = "Consol Piece Number of Total";
			public const string ConsolWeight = "Consol Weight";

			public const string HousebillNumber = "Housebill Number";
			public const string ShipmentPieceNumber = "Shipment Piece Number";
			public const string ShipmentPieceCount = "Shipment Total No. of Pieces";
			public const string ShipmentPieceNumberOfCount = "Shipment Piece Number of Total";
			public const string ShipmentWeight = "Shipment Weight";
			public const string ShipmentHandlingInformation = "Shipment Handling Information";

			public const string IssuingCompanyName = "Company";

			public const string HAWBNo = "HAWB NO";
			public const string PieceCount = "PIECE COUNT";
			public const string Origin = "ORIGIN";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public AWBLabelOptionalInformationList()
			: base("AWB is in English only")
		{
			AddPair(Codes.Blank, Codes.Blank);

			AddPair(Codes.ConsolOrigin, Codes.ConsolOrigin);
			AddPair(Codes.ConsolDestination, Codes.ConsolDestination);
			AddPair(Codes.ConsolPieceNumber, Codes.ConsolPieceNumber);
			AddPair(Codes.ConsolPieceCount, Codes.ConsolPieceCount);
			AddPair(Codes.ConsolPieceNumberOfCount, Codes.ConsolPieceNumberOfCount);
			AddPair(Codes.ConsolWeight, Codes.ConsolWeight);

			AddPair(Codes.HousebillNumber, Codes.HousebillNumber);
			AddPair(Codes.ShipmentPieceNumber, Codes.ShipmentPieceNumber);
			AddPair(Codes.ShipmentPieceCount, Codes.ShipmentPieceCount);
			AddPair(Codes.ShipmentPieceNumberOfCount, Codes.ShipmentPieceNumberOfCount);
			AddPair(Codes.ShipmentWeight, Codes.ShipmentWeight);
			AddPair(Codes.ShipmentHandlingInformation, Codes.ShipmentHandlingInformation);

			AddPair(Codes.IssuingCompanyName, Codes.IssuingCompanyName);
		}
	}

	public class AWBLabelCustomDesignList : UntranslatableCodeDescriptionPairList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public static class Codes
		{
			public const string Default = "Default Design";
			public const string Design1 = "Custom Design #1";
			public const string Design2 = "Custom Design #2";
			public const string Design3 = "Custom Design #3";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Not translatable reason")]
		public AWBLabelCustomDesignList()
			: base("AWB is in English only")
		{
			AddPair(Codes.Default, Codes.Default);
			AddPair(Codes.Design1, Codes.Design1);
			AddPair(Codes.Design2, Codes.Design2);
			AddPair(Codes.Design3, Codes.Design3);
		}
	}
}
