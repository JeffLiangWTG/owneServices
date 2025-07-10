using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	partial class LocalExportAmendmentDataItemIDList
	{
		public static string GetDataItemIDDescription(string code)
		{
			var result = ZString.Empty;

			switch (code)
			{
				case Codes._11A:
				case Codes._12:
				case Codes._13:
				case Codes._14:
				case Codes._15:
				case Codes._16:
				case Codes._18:
				case Codes._38:
				case Codes._39:
					result = nameof(ILocalExportEntryHeader);
					break;
				case Codes._11B:
				case Codes._11C:
				case Codes._11D:
					result = nameof(ILocalExportOtherTransportMeans);
					break;
				default:
					result = nameof(ILocalExportEntryLine);
					break;
			}

			return result;
		}
	}
}
