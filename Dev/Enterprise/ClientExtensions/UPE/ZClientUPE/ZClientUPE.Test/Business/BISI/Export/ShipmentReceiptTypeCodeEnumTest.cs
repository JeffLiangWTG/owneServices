using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class ShipmentReceiptTypeCodeEnumTest : TestCase
	{
		public void TestCorrectIntegerValues()
		{
			AssertEquals("Incorrect type code int value", 4, (int)ShipmentReceiptTypeCode.GIROCode);
			AssertEquals("Incorrect type code int value", 5, (int)ShipmentReceiptTypeCode.MAWBNumber);
			AssertEquals("Incorrect type code int value", 6, (int)ShipmentReceiptTypeCode.CycleDate);
			AssertEquals("Incorrect type code int value", 7, (int)ShipmentReceiptTypeCode.CycleNumber);
			AssertEquals("Incorrect type code int value", 8, (int)ShipmentReceiptTypeCode.OBCTaxCertificateNumber);
			AssertEquals("Incorrect type code int value", 9, (int)ShipmentReceiptTypeCode.OBCPayDeclarationNumber);
			AssertEquals("Incorrect type code int value", 10, (int)ShipmentReceiptTypeCode.InvoiceQuantity);
		}

		public void TestReceiptTypeCode()
		{
			var shipmentData = new ShipmentDataForTest();
			shipmentData.ShipmentRef = "S0015678";
			shipmentData.ImportDate = new ZDateTime(2017, 12, 20);
			var receiptData = new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, "G02");
			AssertEquals("Incorrect type code int value", ShipmentReceiptTypeCode.GIROCode, receiptData.ReceiptTypeCode);
		}
	}
}
