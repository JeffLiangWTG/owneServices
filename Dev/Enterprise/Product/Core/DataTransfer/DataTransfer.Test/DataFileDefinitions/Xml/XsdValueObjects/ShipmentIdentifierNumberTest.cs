using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.ShipmentIdentifier))]
	sealed class ShipmentIdentifierNumberTest : ValueObjectTestCase
	{
		public void TestGetMasterBill()
		{
			Xsd.Consol consol = new Xsd.Consol();
			Xsd.ConsolIdentifier consolIdentifier = consol.ConsolIdentifier.AddNew();
			consolIdentifier.ConsolIdentifierType = ConsolIdentifierType.MasterWaybill;
			consolIdentifier.Value = "ConsolMasterBill";

			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.ShipmentIdentifier shipmentIdentifier = shipment.ShipmentIdentifier.AddNew();
			shipmentIdentifier.Masterbill = "ShipmentMasterBill";

			AssertEquals("ShipmentMasterBill", shipmentIdentifier.GetMasterBill(consol));
			AssertEquals("ShipmentMasterBill", shipmentIdentifier.GetMasterBill(null));
			shipmentIdentifier.Masterbill = ZString.Empty;
			AssertEquals("", shipmentIdentifier.GetMasterBill(null));
			AssertEquals("ConsolMasterBill", shipmentIdentifier.GetMasterBill(consol));
		}

		public void TestGetMasterAndHouseBill()
		{
			Xsd.Consol consol = new Xsd.Consol();
			Xsd.ConsolIdentifier consolIdentifier = consol.ConsolIdentifier.AddNew();
			consolIdentifier.ConsolIdentifierType = ConsolIdentifierType.MasterWaybill;
			consolIdentifier.Value = "MasterBill";

			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.ShipmentIdentifier shipmentIdentifier = shipment.ShipmentIdentifier.AddNew();
			shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			shipmentIdentifier.Value = "HouseBill";

			Xsd.MasterAndHouseBill masterAndHouseBill = shipmentIdentifier.GetMasterAndHouseBill(consol);
			AssertEquals("MasterBill", masterAndHouseBill.MasterBill);
			AssertEquals("HouseBill", masterAndHouseBill.HouseBill);

			consolIdentifier.Value = "";
			shipmentIdentifier.Value = "";
			AssertNull("Null should be returned if there is no valid master/house bill", shipmentIdentifier.GetMasterAndHouseBill(consol));
		}

		public void TestCompileTimeCheck()
		{
			Xsd.ShipmentIdentifier value = null;
			value = new Xsd.ShipmentIdentifierCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
