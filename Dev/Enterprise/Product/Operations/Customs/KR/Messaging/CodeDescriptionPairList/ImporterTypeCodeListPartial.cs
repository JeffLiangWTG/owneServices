namespace Enterprise.Customs.KR.Messaging
{
	partial class ImporterTypeCodeList
	{
		public static string ConvertPaidByCode(string paidByCode)
		{
			var result = string.Empty;
			switch (paidByCode)
			{
				case PaidByCodeList.Codes.CLI:
					result = Codes.A;
					break;
				case PaidByCodeList.Codes.OTH:
					result = Codes.B;
					break;
				default:
					result = paidByCode;
					break;
			}
			return result;
		}
	}
}
