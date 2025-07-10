using System.Collections;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestedType(typeof(Xsd.Shipment))]
	sealed class XsdShipmentTest : ValueObjectTestCase
	{
		public void TestHouseBill()
		{
			Xsd.Shipment shipment = new Xsd.Shipment();
			shipment.ShipmentIdentifier = new Xsd.ShipmentIdentifierCollection();

			Xsd.ShipmentIdentifier identifier1 = shipment.ShipmentIdentifier.AddNew();
			Xsd.ShipmentIdentifier identifier2 = shipment.ShipmentIdentifier.AddNew();

			identifier1.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Other;
			identifier1.Value = "Identifier1";
			identifier2.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			identifier2.Value = "Identifier2";

			AssertEquals("House Value", "Identifier2", shipment.Housebill);
			identifier2.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Other;
			AssertEquals("Should be no house Bill", string.Empty, shipment.Housebill);
		}

		public void TestGetPackagesForContainer()
		{
			Xsd.Shipment shipment = new Xsd.Shipment();
			shipment.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			shipment.ShipmentDetails.Packages = new Xsd.PackageCollection();

			Xsd.Package package1 = shipment.ShipmentDetails.Packages.AddNew();
			package1.ContainerNumber = "Container1";
			Xsd.Package package2 = shipment.ShipmentDetails.Packages.AddNew();
			package2.ContainerNumber = "Container1";
			Xsd.Package package3 = shipment.ShipmentDetails.Packages.AddNew();
			package3.ContainerNumber = "Container2";

			Xsd.PackageCollection c1Collection = shipment.GetPackagesForContainer("Container1");
			Xsd.PackageCollection c2Collection = shipment.GetPackagesForContainer("Container2");
			Xsd.PackageCollection c3Collection = shipment.GetPackagesForContainer("Container3");

			AssertEquals("Count should be 2 for 'Container1'", 2, c1Collection.Count);
			Assert("Should have package 1", ((IList)c1Collection).Contains(package1));
			Assert("Should have package 2", ((IList)c1Collection).Contains(package2));

			AssertEquals("Count should be 1 for 'Container2'", 1, c2Collection.Count);
			Assert("Should have package 3", ((IList)c2Collection).Contains(package3));

			AssertEquals("Container3 package collection should be empty", 0, c3Collection.Count);
		}

		public void TestGetMasterAndHouseBillIdentifiers()
		{
			Xsd.Consol consol = new Xsd.Consol();
			Xsd.Shipment shipment = new Xsd.Shipment();

			Xsd.ConsolIdentifier consolIdentifer = consol.ConsolIdentifier.AddNew();
			consolIdentifer.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			consolIdentifer.Value = "ConsolMasterBill";

			Xsd.ShipmentIdentifier shipmentIdentifier = shipment.ShipmentIdentifier.AddNew();
			shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			shipmentIdentifier.Value = "HouseBill1";
			shipmentIdentifier.Masterbill = "ShipmentMasterBill";

			Xsd.ShipmentIdentifier shipmentIdentifier2 = shipment.ShipmentIdentifier.AddNew();
			shipmentIdentifier2.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			shipmentIdentifier2.Value = "HouseBill2";

			Xsd.ShipmentIdentifier decoyShipmentIdentifier = shipment.ShipmentIdentifier.AddNew();
			decoyShipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.CoLoadMaster;
			decoyShipmentIdentifier.Value = "decoy";

			Xsd.MasterAndHouseBill[] identifiers = shipment.GetMasterAndHouseBillIdentifiers(consol);
			AssertEquals("2 master/house bill pairs should be found", 2, identifiers.Length);
			AssertEquals("ShipmentMasterBill", identifiers[0].MasterBill);
			AssertEquals("HouseBill1", identifiers[0].HouseBill);
			AssertEquals("ConsolMasterBill", identifiers[1].MasterBill);
			AssertEquals("HouseBill2", identifiers[1].HouseBill);
		}

		public void TestGetMasterAndHouseBillIdentifiers_WithBlankMasterAndHouseBill()
		{
			Xsd.Shipment shipment = new Xsd.Shipment();

			Xsd.ShipmentIdentifier shipmentIdentifier = shipment.ShipmentIdentifier.AddNew();
			shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			shipmentIdentifier.Value = "";
			shipmentIdentifier.Masterbill = "";

			Xsd.MasterAndHouseBill[] identifiers = shipment.GetMasterAndHouseBillIdentifiers(null);
			AssertEquals("No valid identifiers should be found", 0, identifiers.Length);
		}

		public void TestCompileTimeCheck()
		{
			Xsd.Shipment value = null;
			value = new Xsd.ShipmentCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
