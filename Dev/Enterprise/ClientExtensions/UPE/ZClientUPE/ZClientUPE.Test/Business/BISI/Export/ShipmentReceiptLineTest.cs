using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	class ShipmentReceiptLineTest : TestCase
	{
		public void TestLineAsString()
		{
			var shipmentData = new ShipmentDataForTest();
			shipmentData.ShipmentRef = "S0015678";
			shipmentData.ImportDate = new ZDateTime(2017, 12, 06);
			var receiptData = new ShipmentReceiptData(ShipmentReceiptTypeCode.MAWBNumber, "010-12345");
			var receiptLine = new ShipmentReceiptLine(shipmentData, receiptData);
			var expectedResult = "S0015678   AU3000002017-12-06ADD005010-12345                                                                                                                                                                                                                                                                ";
			AssertEquals(expectedResult, receiptLine.LineAsString);
		}

		public void TestGIROCode()
		{
			var shipmentData = new ShipmentDataForTest();
			shipmentData.ShipmentRef = "S0015678";
			shipmentData.ImportDate = new ZDateTime(2017, 12, 20);
			var receiptData = new ShipmentReceiptData(ShipmentReceiptTypeCode.MAWBNumber, "010-12345");
			var receiptLine = new ShipmentReceiptLine(shipmentData, receiptData);
			AssertEquals("GIROCode should be blank", "", receiptLine.GIROCode);
			receiptData = new ShipmentReceiptData(ShipmentReceiptTypeCode.GIROCode, ReceiptGIROCodeTypes.GSTPaidThruGIRO);
			receiptLine = new ShipmentReceiptLine(shipmentData, receiptData);
			AssertEquals("GIROCode should now have a value", "G05", receiptLine.GIROCode);
		}

		public void TestIsTaxCertificate()
		{
			var shipmentData = new ShipmentDataForTest();
			shipmentData.ShipmentRef = "S0015678";
			shipmentData.ImportDate = new ZDateTime(2017, 12, 20);
			var receiptData = new ShipmentReceiptData(ShipmentReceiptTypeCode.MAWBNumber, "010-12345");
			var receiptLine = new ShipmentReceiptLine(shipmentData, receiptData);
			AssertEquals("IsTaxCertificate", false, receiptLine.IsTaxCertificate);
			receiptData = new ShipmentReceiptData(ShipmentReceiptTypeCode.OBCTaxCertificateNumber, "Y729384572");
			receiptLine = new ShipmentReceiptLine(shipmentData, receiptData);
			AssertEquals("IsTaxCertificate", true, receiptLine.IsTaxCertificate);
		}
	}
}
