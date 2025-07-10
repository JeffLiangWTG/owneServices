using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Romania
{
	public static class RomaniaMessageTypeHelper
	{
		public static string GetMessageType(string pivotState)
		{
			if (pivotState == EInvoicingPivotState.Delivered)
			{
				return RomaniaEInvoiceAPICommandList.Codes.QueryInvoiceRequest;
			}
			else
			{
				return RomaniaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
			}
		}
	}
}
