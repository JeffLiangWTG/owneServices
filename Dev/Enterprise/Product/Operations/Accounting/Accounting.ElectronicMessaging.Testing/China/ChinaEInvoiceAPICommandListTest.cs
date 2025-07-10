using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.China.Testing
{
	class ChinaEInvoiceAPICommandListTest : TestCase
	{
		public void TestGetMessageType()
		{
			var messageType = ChinaEInvoiceAPICommandList.GetMessageType("XXX");

			AssertEquals(string.Empty, messageType);

			messageType = ChinaEInvoiceAPICommandList.GetMessageType(Core.Constants.EInvoicingPivotActionType.Submit);

			AssertEquals(ChinaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, messageType);
		}
	}
}
