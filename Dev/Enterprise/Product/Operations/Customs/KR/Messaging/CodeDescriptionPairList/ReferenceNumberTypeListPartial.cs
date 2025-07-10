namespace Enterprise.Customs.KR.Messaging
{
	public partial class ReferenceNumberTypeList
	{
		public static string GetMappedCW1CodeFromCustomsCode(string customCode)
		{
			return customCode == Constants.ApplicationNumberType.XXC ? Codes.CMN : (customCode == Constants.ApplicationNumberType.ABT ? Codes.IMP : customCode);
		}
		public static string GetMappedCustomsCode(string cw1Code)
		{
			return cw1Code == Codes.CMN ? Constants.ApplicationNumberType.XXC : (cw1Code == Codes.IMP ? Constants.ApplicationNumberType.ABT : cw1Code);
		}
	}
}
