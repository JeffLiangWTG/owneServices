using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class ShipmentChargeDataListEnumTest : TestCase
	{
		public void TestCorrectIntegerValues()
		{
			AssertEquals("Incorrect type code int value", 201, (int)ShipmentChargeTypeCode.Duty);
			AssertEquals("Incorrect type code int value", 206, (int)ShipmentChargeTypeCode.GST);
			AssertEquals("Incorrect type code int value", 216, (int)ShipmentChargeTypeCode.Other);
			AssertEquals("Incorrect type code int value", 224, (int)ShipmentChargeTypeCode.Quarantine);
			AssertEquals("Incorrect type code int value", 231, (int)ShipmentChargeTypeCode.Tradegate);
			AssertEquals("Incorrect type code int value", 309, (int)ShipmentChargeTypeCode.QuarantinePermit);
			AssertEquals("Incorrect type code int value", 348, (int)ShipmentChargeTypeCode.Security);
			AssertEquals("Incorrect type code int value", 405, (int)ShipmentChargeTypeCode.Disbursement);
			AssertEquals("Incorrect type code int value", 436, (int)ShipmentChargeTypeCode.Terminal);
			AssertEquals("Incorrect type code int value", 445, (int)ShipmentChargeTypeCode.Insurance);
			AssertEquals("Incorrect type code int value", 500, (int)ShipmentChargeTypeCode.Freight);
			AssertEquals("Incorrect type code int value", 524, (int)ShipmentChargeTypeCode.ExtendedAreaSurcharge);
			AssertEquals("Incorrect type code int value", 545, (int)ShipmentChargeTypeCode.FuelSurcharge);
			AssertEquals("Incorrect type code int value", 565, (int)ShipmentChargeTypeCode.QuantumView);
			AssertEquals("Incorrect type code int value", 410, (int)ShipmentChargeTypeCode.ChargePerLine);
			AssertEquals("Incorrect type code int value", 431, (int)ShipmentChargeTypeCode.ContactFee);
		}
	}
}
