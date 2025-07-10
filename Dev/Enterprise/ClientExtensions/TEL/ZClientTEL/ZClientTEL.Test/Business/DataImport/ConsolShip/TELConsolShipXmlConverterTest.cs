using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Client.TEL.Definition;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.TEL.Import.Testing
{
	public class TELConsolShipXmlConverterTest : TestCaseWithFactory
	{
		public void TestConvert()
		{
			Xsd.Consol[] xsdObjects = Converter.Convert(GetExternalTestValueObjects());
			Assert(xsdObjects.Length > 0);
			Xsd.Consol consolValue = xsdObjects[0];
			AssertNotNull("Consol value should not be null", consolValue);
			AssertConsolDetails(consolValue);
		}

		#region Implementation
		void AssertConsolDetails(Xsd.Consol consolValue)
		{
			AssertEquals("Masterbill:", "MASTER1", consolValue.Masterbill);
			Xsd.ConsolConsolDetail consolDetails = consolValue.ConsolDetail;
			AssertEquals("AgentReference:", "", consolDetails.AgentReference);
			AssertEquals("ConsolType:", Xsd.ConsolType.Agent, consolDetails.ConsolType);
			AssertEquals("TransportMode:", Xsd.ConsolTransportMode.SEA, consolDetails.TransportMode);
			AssertEquals("TransportModeSpecified:", true, consolDetails.TransportModeSpecified);
			AssertEquals("ContainerMode:", Xsd.ContainerMode.FCL, consolDetails.ContainerMode);
			AssertEquals("Sending Agent", "1", consolDetails.SendingAgent.OwnerCode);
			AssertEquals("Receiving Agent", "2", consolDetails.ReceivingAgent.OwnerCode);
			AssertEquals("Carrier", "4", consolDetails.Carrier.OwnerCode);
			AssertEquals("Legs", 1, consolDetails.PlannedLegs.Count);
			Xsd.PlannedLeg mainLegValue = consolDetails.PlannedLegs[0];
			AssertNotNull("Main Vessel leg should exist", mainLegValue);
			AssertEquals("PortOfDischarge", "DCode", mainLegValue.PortOfDischarge.Port.Value);
			AssertEquals("PortOfLoading", "LCode", mainLegValue.PortOfLoading.Port.Value);
			Xsd.SailingForPlannedLegs sailing = (Xsd.SailingForPlannedLegs)mainLegValue.Item;
			AssertNotNull("Sailing on main leg:", sailing);
			AssertEquals("Vessel Name", "VESSEL1", sailing.VesselName);
			AssertEquals("Voyage", "V1", sailing.VoyageNo);
			AssertEquals(1, consolDetails.Containers.Count);
			Xsd.Container containerValue = consolDetails.Containers[0];
			AssertEquals("ContainerNumber:", "11", containerValue.ContainerNumber);
			AssertEquals("Seal:", "22", containerValue.Seal);
			AssertEquals("PackingMode:", Xsd.ContainerMode.FCL, containerValue.PackingMode);
			AssertEquals("ContainerType.ContainerCode", "20GP", containerValue.ContainerType.ContainerCode);
			AssertEquals(1, consolValue.Shipments.Count);
			AssertShipmentDetails(consolValue.Shipments[0]);
		}

		void AssertShipmentDetails(Xsd.Shipment shipment)
		{
			AssertEquals("Housebill:", "HOUSE1", shipment.Housebill);
			Xsd.ShipmentShipmentDetails shipmentDetails = shipment.ShipmentDetails;
			AssertEquals("Consignee:", "CONSIGNEE", shipment.ShipmentDetails.Consignee.OrganisationDetails.Name);
			Xsd.OrgAddress consigneeAddress = shipment.ShipmentDetails.Consignee.OrganisationDetails.Addresses[0];
			AssertEquals("Consignee Addr1", "CONSIGNEE ADDR 1", consigneeAddress.AddressLine1);
			AssertEquals("Consignee Addr2", "CONSIGNEE ADDR 2", consigneeAddress.AddressLine2);
			AssertEquals("Consignee City", "CONSIGNEE ADDR 3", consigneeAddress.CityOrSuburb);
			AssertEquals("Consignor:", "CONSIGNOR", shipment.ShipmentDetails.Consignor.OrganisationDetails.Name);
			Xsd.OrgAddress consignorAddress = shipment.ShipmentDetails.Consignor.OrganisationDetails.Addresses[0];
			AssertEquals("Consignor Addr1", "CONSIGNOR ADDR 1", consignorAddress.AddressLine1);
			AssertEquals("Consignor Addr2", "CONSIGNOR ADDR 2", consignorAddress.AddressLine2);
			AssertEquals("Consignor City", "CONSIGNOR ADDR 3", consignorAddress.CityOrSuburb);
			AssertEquals("DeliveryAgent:", "OS AGENT", shipment.ShipmentDetails.Deliver.DeliveryAgent.OrganisationDetails.Name);
			Xsd.OrgAddress deliveryAgentAddress = shipment.ShipmentDetails.Deliver.DeliveryAgent.OrganisationDetails.Addresses[0];
			AssertEquals("DeliveryAgent Addr1", "OS AGENT ADDR 1", deliveryAgentAddress.AddressLine1);
			AssertEquals("DeliveryAgent Addr2", "OS AGENT ADDR 2", deliveryAgentAddress.AddressLine2);
			AssertEquals("DeliveryAgent City", "OS AGENT ADDR 3", deliveryAgentAddress.CityOrSuburb);
			AssertEquals("Notify:", "SHIP NOTIFY", shipment.ShipmentDetails.NotifyParty.Organisation.OrganisationDetails.Name);
			Xsd.OrgAddress notifyAddress = shipment.ShipmentDetails.NotifyParty.Organisation.OrganisationDetails.Addresses[0];
			AssertEquals("Notify Addr1", "SHIP NOTIFY ADDR 1", notifyAddress.AddressLine1);
			AssertEquals("Notify Addr2", "SHIP NOTIFY ADDR 2", notifyAddress.AddressLine2);
			AssertEquals("Notify City", "SHIP NOTIFY ADDR 3", notifyAddress.CityOrSuburb);
			AssertEquals("Port of destination", "DPort", shipmentDetails.PortofDestination.Port.Value);
			AssertEquals("Port of origin", "OPort", shipmentDetails.PortOfOrigin.Port.Value);
			AssertEquals(Xsd.TransportMode.SEA, shipmentDetails.TransportMode);
			AssertEquals(Xsd.ContainerMode.FCL, shipmentDetails.PackingMode);
			AssertEquals("GoodsDescription", "SOME TEXT ABOUT THE SHIPMENT", shipmentDetails.GoodsDescription);
			AssertEquals("Marks", "", shipmentDetails.MarksAndNumbers);
			AssertEquals("Incoterm", Core.Constants.IncoTerms.FreeOnBoard, shipment.ShipmentDetails.Incoterm);
			AssertEquals(1, shipment.ShipmentDetails.Packages.Count);
			AssertPackages(shipment.ShipmentDetails.Packages[0]);
		}

		void AssertPackages(Xsd.Package packValue)
		{
			AssertEquals("11", packValue.ContainerNumber);
			AssertEquals(1, packValue.NumberOfPacks);
			AssertEquals(33.3M, packValue.Weight.Value);
			AssertEquals(44.4M, packValue.Volume.Value);
			AssertEquals("", packValue.GoodsDescription);
			AssertEquals("", packValue.MarksAndNumbers);
			AssertEquals("TTT", packValue.PackType);
		}

		IValueObject[] GetExternalTestValueObjects()
		{
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var testFilePath = resourceRetriever.GetBytes("single.xml");
			IValueObject[] result;
			using (StreamReader reader = new StreamReader(new MemoryStream(testFilePath)))
			{
				TELConsolShipXmlDocument document = new TELConsolShipXmlDocument(reader, new NotificationBuffer());
				result = document.ConvertToValueObjects();
			}

			return result;
		}

		TELConsolShipXmlConverter Converter
		{
			get
			{
				return converter ?? (converter = new TELConsolShipXmlConverter());
			}
		}

		TELConsolShipXmlConverter converter;
		#endregion
	}
}
