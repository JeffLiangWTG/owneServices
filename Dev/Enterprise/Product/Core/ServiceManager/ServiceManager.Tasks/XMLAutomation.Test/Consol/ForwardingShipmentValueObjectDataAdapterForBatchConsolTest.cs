using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class ForwardingShipmentValueObjectDataAdapterForBatchConsolTest : TestCaseWithFactory
	{
		public void TestStandAloneShipmentFound()
		{
			TestHelper.Notification.Clear();
			var shipment = SetupShipment();
			shipment.JS_HouseBill = "HAWBXXX";
			Factory.Save();

			Assert("Precondition: existing shipment is not attached to any consol", shipment.Consols.Count == 0);

			Xsd.Shipment shipmentXsd = SetupXSDWithShipment();
			Xsd.ShipmentIdentifier shipmentIdentifier = shipmentXsd.ShipmentIdentifier.AddNew();
			shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			shipmentIdentifier.Value = "HAWBXXX";

			var consol = SetupConsol();

			var dataAdapter = new ForwardingShipmentValueObjectDataAdapterForBatchConsol(consol, null);
			var importContext = new ValueObjectImportContext(Factory, TestHelper.Notification);

			var importedShipment = dataAdapter.CreateOrUpdateFromValueObject(shipmentXsd, importContext);

			AssertEquals("imported shipment ", shipment.PK, importedShipment.PK);
			Assert("shipment is attached to consol after import", shipment.Consols.Count == 1);

			AssertEquals("the consol that the shipment attached to", consol.PK, shipment.Consols[0].PK);
			AssertContains(ZString.Format("Attaching shipment '{0}' to consol '{1}' ......", shipment.JobNumber, consol.JK_UniqueConsignRef), TestHelper.Notification.AsString);
		}

		public void TestExistingShipmentFoundButAlreadyAttachedtoOtherConsol()
		{
			TestHelper.Notification.Clear();

			var consol1 = SetupConsol();
			var shipment = SetupShipment();
			shipment.JS_HouseBill = "HAWBXXX";
			shipment.Consols.Add(consol1);

			Factory.Save();

			var consol2 = SetupConsol();
			consol2.JK_MasterBillNum = "MAWB1";

			Xsd.Shipment shipmentXsd = SetupXSDWithShipment();
			Xsd.ShipmentIdentifier shipmentIdentifier = shipmentXsd.ShipmentIdentifier.AddNew();
			shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			shipmentIdentifier.Value = "HAWBXXX";

			var importContext = new ValueObjectImportContext(Factory, TestHelper.Notification);
			var dataAdapter = new ForwardingShipmentValueObjectDataAdapterForBatchConsol(consol2, null);
			var importedShipment = dataAdapter.CreateOrUpdateFromValueObject(shipmentXsd, importContext);

			AssertNotEquals("As the existing shipment with housebill 'HAWBXXX' already attached to consol. A new shipment will be created instead", shipment.PK, importedShipment.PK);
			AssertEquals("no of attached consols of the imported shipment", 1, shipment.Consols.Count);
			AssertEquals("Shipment is attached to consol with masterbill 'MAWB1'", consol2.PK, importedShipment.Consols[0].PK);
			AssertContains(ZString.Format("Cannot attach shipment '{0}' to consol '{1}' as it is already attached to other consol. A new shipment will be created instead.", shipment.JobNumber, consol2.JK_UniqueConsignRef), TestHelper.Notification.AsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SystemDataRegistry.Instance.AllowLinkingStandaloneShipmentToConsol.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.HouseBill);
		}

		Xsd.Shipment SetupXSDWithShipment()
		{
			Xsd.Shipment shipmentXSD = new Xsd.Shipment();
			shipmentXSD.ShipmentDetails.Consignee.OwnerCode = "CONSIGNEE";
			shipmentXSD.ShipmentDetails.Consignor.OwnerCode = "CONSIGNOR";
			shipmentXSD.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			shipmentXSD.ShipmentDetails.PortOfOrigin.Port.Value = "HKHKG";
			shipmentXSD.ShipmentDetails.PortofDestination.Port.Value = "AUSYD";

			return shipmentXSD;
		}

		ForwardingShipment SetupShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>(TestBusinessObjectKind.MinimumRequiredToSave);
			return shipment;
		}

		ForwardingConsol SetupConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>(TestBusinessObjectKind.MinimumRequiredToSave);
			consol.JK_MasterBillNum = "MAWB";
			return consol;
		}

		XmlAndFlatFileTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new XmlAndFlatFileTestHelper(Factory)); }
		}
		XmlAndFlatFileTestHelper testHelper;
	}
}
