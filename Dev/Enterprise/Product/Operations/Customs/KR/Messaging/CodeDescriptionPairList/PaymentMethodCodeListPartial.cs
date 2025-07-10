namespace Enterprise.Customs.KR.Messaging
{
	partial class PaymentMethodCodeList
	{
		public static bool IsImportTypeCodeERequired(string code)
		{
			return code == Codes._11
				|| code == Codes._13
				|| code == Codes._14
				|| code == Codes._18
				|| code == Codes._33
				|| code == Codes._43
				|| code == Codes._53;
		}
	}
}
