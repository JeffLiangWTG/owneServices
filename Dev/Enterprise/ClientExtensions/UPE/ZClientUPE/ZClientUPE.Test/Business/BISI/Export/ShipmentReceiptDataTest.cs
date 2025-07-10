using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class ShipmentReceiptDataTest : TestCase
	{
		public void TestConstructor()
		{
			var receiptData = new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, "12345678910");
			AssertEquals("Incorrect receipt type code", "004", receiptData.TypeCode);
			AssertEquals("Incorrect receipt type information", "12345678910", receiptData.TypeInfomation);
			receiptData = new ShipmentReceiptData(ShipmentReceiptTypeCode.InvoiceQuantity, "100LOT");
			AssertEquals("Incorrect receipt type code", "010", receiptData.TypeCode);
			AssertEquals("Incorrect receipt type information", "100LOT", receiptData.TypeInfomation);
		}
	}
}
