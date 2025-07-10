using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestedType(typeof(Xsd.Consol))]
	sealed class XsdConsolValueTest : ValueObjectTestCase
	{
		public void TestMasterBill()
		{
			Xsd.Consol consol = new Xsd.Consol();
			consol.ConsolIdentifier = new Xsd.ConsolIdentifierCollection();

			Xsd.ConsolIdentifier identifier1 = consol.ConsolIdentifier.AddNew();
			Xsd.ConsolIdentifier identifier2 = consol.ConsolIdentifier.AddNew();

			identifier1.ConsolIdentifierType = Xsd.ConsolIdentifierType.Other;
			identifier1.Value = "Identifier1";
			identifier2.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier2.Value = "Identifier2";

			AssertEquals("Master Bill Value", "Identifier2", consol.Masterbill);
			identifier2.ConsolIdentifierType = Xsd.ConsolIdentifierType.Other;
			AssertEquals("Should be no Master Bill", string.Empty, consol.Masterbill);
		}

		public void TestMasterbill_WithAmbiguousReturnsEmpty()
		{
			Xsd.Consol consol = new Xsd.Consol();
			consol.ConsolIdentifier = new Xsd.ConsolIdentifierCollection();

			Xsd.ConsolIdentifier identifier1 = consol.ConsolIdentifier.AddNew();
			Xsd.ConsolIdentifier identifier2 = consol.ConsolIdentifier.AddNew();

			identifier1.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier1.Value = "Identifier1";
			identifier2.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier2.Value = "Identifier2";

			AssertEquals("Ambiguous Master Bill should yield no result", true, consol.Masterbill.IsEmpty);
		}

		public void TestGetShipmentsForContainer()
		{
			Xsd.Consol consol = new Xsd.Consol();
			consol.Shipments = new Xsd.ShipmentCollection();

			Xsd.Shipment shipment1 = consol.Shipments.AddNew();
			Xsd.Shipment shipment2 = consol.Shipments.AddNew();
			Xsd.Shipment shipment3 = consol.Shipments.AddNew();

			shipment1.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			shipment1.ShipmentDetails.Packages = new Xsd.PackageCollection();

			shipment2.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			shipment2.ShipmentDetails.Packages = new Xsd.PackageCollection();

			shipment3.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			shipment3.ShipmentDetails.Packages = new Xsd.PackageCollection();

			Xsd.Package package1 = shipment1.ShipmentDetails.Packages.AddNew();
			Xsd.Package package2 = shipment1.ShipmentDetails.Packages.AddNew();
			Xsd.Package package3 = shipment1.ShipmentDetails.Packages.AddNew();

			package1.ContainerNumber = "C1";
			package2.ContainerNumber = "C2";
			package3.ContainerNumber = "C3";

			package1 = shipment2.ShipmentDetails.Packages.AddNew();
			package2 = shipment3.ShipmentDetails.Packages.AddNew();
			package3 = shipment3.ShipmentDetails.Packages.AddNew();

			package1.ContainerNumber = "C2";
			package2.ContainerNumber = "C2";
			package3.ContainerNumber = "C3";

			Xsd.ShipmentCollection shipments = consol.GetShipmentsForContainer("C1");
			AssertEquals("Shipment count container 1", 1, shipments.Count);

			shipments = consol.GetShipmentsForContainer("C2");
			AssertEquals("Shipment count container 2", 3, shipments.Count);

			shipments = consol.GetShipmentsForContainer("C3");
			AssertEquals("Shipment count container 3", 2, shipments.Count);
		}

		public void TestGetPackagesForContainer()
		{
			Xsd.Consol consol = new Xsd.Consol();
			consol.Shipments = new Xsd.ShipmentCollection();

			Xsd.Shipment shipment1 = consol.Shipments.AddNew();
			Xsd.Shipment shipment2 = consol.Shipments.AddNew();
			Xsd.Shipment shipment3 = consol.Shipments.AddNew();

			shipment1.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			shipment1.ShipmentDetails.Packages = new Xsd.PackageCollection();

			shipment2.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			shipment2.ShipmentDetails.Packages = new Xsd.PackageCollection();

			shipment3.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			shipment3.ShipmentDetails.Packages = new Xsd.PackageCollection();

			Xsd.Package package1 = shipment1.ShipmentDetails.Packages.AddNew();
			Xsd.Package package2 = shipment1.ShipmentDetails.Packages.AddNew();
			Xsd.Package package3 = shipment1.ShipmentDetails.Packages.AddNew();

			package1.ContainerNumber = "C1";
			package2.ContainerNumber = "C2";
			package3.ContainerNumber = "C3";

			package1 = shipment2.ShipmentDetails.Packages.AddNew();
			package2 = shipment2.ShipmentDetails.Packages.AddNew();
			package3 = shipment3.ShipmentDetails.Packages.AddNew();

			package1.ContainerNumber = "C2";
			package2.ContainerNumber = "C2";
			package3.ContainerNumber = "C3";

			Xsd.PackageCollection packages = consol.GetPackagesForContainer("C1");
			AssertEquals("Package count container 1", 1, packages.Count);

			packages = consol.GetPackagesForContainer("C2");
			AssertEquals("Package count container 2", 3, packages.Count);

			packages = consol.GetPackagesForContainer("C3");
			AssertEquals("Package count container 3", 2, packages.Count);
		}

		public void TestCompileTimeCheck()
		{
			Xsd.Consol value = null;
			value = new Xsd.ConsolCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
