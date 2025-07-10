namespace Enterprise.Customs.KR.Messaging
{
	partial class PaidByCodeList
	{
		public static string ConvertImporterTypeCode(string importerTypeCode)
		{
			var result = string.Empty;
			switch (importerTypeCode)
			{
				case ImporterTypeCodeList.Codes.A:
					result = Codes.CLI;
					break;
				case ImporterTypeCodeList.Codes.B:
					result = Codes.OTH;
					break;
				default:
					result = importerTypeCode;
					break;
			}
			return result;
		}
	}
}
