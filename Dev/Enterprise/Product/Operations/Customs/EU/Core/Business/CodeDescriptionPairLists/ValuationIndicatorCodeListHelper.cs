using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public static class ValuationIndicatorCodeListHelper
	{
		public static string GetFromLineIfSetElseFromHeader(ZBool headerIndicator, ZString lineIndicator)
		{
			if (lineIndicator == ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader)
			{
				return headerIndicator ? Yes : No;
			}
			else
			{
				return lineIndicator == ValuationIndicatorCodeList.Codes.Yes ? Yes : No;
			}
		}
		const string Yes = "1";
		const string No = "0";
	}
}
