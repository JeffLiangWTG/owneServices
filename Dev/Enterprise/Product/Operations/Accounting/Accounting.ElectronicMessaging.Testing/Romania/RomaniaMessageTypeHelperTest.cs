using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Romania.Testing
{
	public sealed class RomaniaMessageTypeHelperTest : TestCase
	{
		public void TestGetMessageType()
		{
			AssertEquals(RomaniaEInvoiceAPICommandList.Codes.QueryInvoiceRequest, RomaniaMessageTypeHelper.GetMessageType(EInvoicingPivotState.Delivered));

			AssertEquals(RomaniaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, RomaniaMessageTypeHelper.GetMessageType(EInvoicingPivotState.Sent));
			AssertEquals(RomaniaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, RomaniaMessageTypeHelper.GetMessageType(EInvoicingPivotState.Batched));
		}
	}
}
