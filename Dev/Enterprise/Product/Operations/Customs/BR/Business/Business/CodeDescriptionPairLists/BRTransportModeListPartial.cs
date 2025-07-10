using Enterprise.Customs.Business.CustomsLists;

namespace Enterprise.Customs.BR.Business
{
	public partial class BRTransportModeList
	{
		public static string GetTransportMeansForBRTransportMode(string code)
		{
			return code?.ToUpper() switch
			{
				Codes.OTH => TransportMeansList.OTH,
				Codes.FIC => TransportMeansList.FIC,
				_ => string.Empty,
			};
		}

		public static class TransportMeansList
		{
			public const string OTH = "O";
			public const string FIC = "F";
		}

		public static string MapCW1CodeToBRTransportMode(string transportMode, string transportMeans) => transportMode?.ToUpper() == Codes.OTH ? transportMeans?.ToUpper() == TransportMeansList.OTH ? Codes.OTH : Codes.FIC : transportMode;

		public static string MapBRTransportModeToCW1Code(string code) => code?.ToUpper() == Codes.OTH || code?.ToUpper() == Codes.FIC ? TransportTypeGenericList.Codes.Other : code;
	}
}
