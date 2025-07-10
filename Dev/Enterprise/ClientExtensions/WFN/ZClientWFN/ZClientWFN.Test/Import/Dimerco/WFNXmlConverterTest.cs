using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using BtaXsd = Enterprise.Client.WFN.Definition;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.WFN.Testing
{
	public class WFNXmlConverterTest : TestCaseWithFactory
	{
		public void TestConvert()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var pathToFile = resourceRetriever.SaveResourceToFile("Import.TestFiles.Test.xml");
				using (StreamReader reader = new StreamReader(pathToFile))
				{
					WFNXmlDocument document = new WFNXmlDocument(reader, new NotificationBuffer());
					IValueObject[] valueObjects = document.ConvertToValueObjects();
					IValueObject[] consols = Converter.Convert(document.ConvertToValueObjects());
					AssertNotNull(consols);
					AssertEquals(1, consols.Length);
					AssertNotNull(consols[0] as Xsd.Consol);
				}
			}
		}

		public void TestProcessConsolDetails()
		{
			Xsd.Consol consol = new Xsd.Consol();
			BtaXsd.MASTER master = GetXsdMaster();
			Converter.ProcessConsolDetails(consol, master);
			AssertNotNull(consol.ConsolIdentifier);
			AssertNotNull(consol.ConsolDetail);
			AssertNotNull(consol.ConsolDetail.SendingAgent);
			AssertNotNull(consol.ConsolDetail.PortOfLoading);
			AssertNotNull(consol.ConsolDetail.PortOfDischarge);
			AssertNotNull(consol.ConsolDetail.Item);
			AssertNotNull(consol.ConsolDetail.PlannedLegs);
			AssertEquals("02307174974", consol.ConsolIdentifier[0].Value);
			AssertEquals("ConsolIdentifier", Xsd.ConsolIdentifierType.MasterWaybill, consol.ConsolIdentifier[0].ConsolIdentifierType);
			AssertEquals("Agent", Xsd.ConsolType.Agent, consol.ConsolDetail.ConsolType);
			AssertEquals(true, consol.ConsolIdentifier[0].ConsolIdentifierTypeSpecified);
			AssertEquals("ReceivingAgent", "DIMSHA", consol.ConsolDetail.SendingAgent.OwnerCode);
			AssertEquals("Port of Load", "HKHKG", consol.ConsolDetail.PortOfLoading.Port.Value);
			AssertEquals("Port of Discharge", "USPVD", consol.ConsolDetail.PortOfDischarge.Port.Value);
			AssertEquals(new ZDateTime(2003, 9, 7), consol.ConsolDetail.PortOfLoading.EstimatedDateTime);
			AssertEquals(new ZDateTime(2003, 9, 8), consol.ConsolDetail.PortOfDischarge.EstimatedDateTime);
			AssertEquals(Xsd.ConsolTransportMode.AIR, consol.ConsolDetail.TransportMode);
			AssertEquals(Xsd.ContainerMode.LSE, consol.ConsolDetail.ContainerMode);
			AssertEquals(Xsd.PaymentType.PPD, consol.ConsolDetail.PaymentType);
			AssertEquals(1, consol.ConsolDetail.PlannedLegs.Count);
			Xsd.PlannedLeg leg = consol.ConsolDetail.PlannedLegs[0];
			AssertEquals(Xsd.TransportMode.AIR, leg.TransportMode);
			AssertEquals(Xsd.PlannedLegTransportType.Flight1, leg.TransportType);
			AssertEquals(new ZDateTime(2003, 9, 7), leg.PortOfLoading.EstimatedDateTime);
			AssertEquals(new ZDateTime(2003, 9, 8), leg.PortOfDischarge.EstimatedDateTime);
			AssertEquals("HKHKG", leg.PortOfLoading.Port.Value);
			AssertEquals("USPVD", leg.PortOfDischarge.Port.Value);
			AssertEquals("FX012", ((Xsd.FlightWithFlightNumber)leg.Item).FlightNoJourneyNoTruckRegNo);
		}

		public void TestProcessShipments()
		{
			Xsd.ShipmentCollection shipments = new Xsd.ShipmentCollection();
			BtaXsd.MASTER master = GetXsdMaster();
			Converter.ProcessShipments(shipments, master);
			AssertEquals(1, shipments.Count);
			AssertNotNull(shipments[0].ShipmentDetails.Consignee);
			AssertNotNull(shipments[0].ShipmentDetails.Consignee.OrganisationDetails.Addresses);
			AssertNotNull(shipments[0].ShipmentDetails.Consignor);
			AssertNotNull(shipments[0].ShipmentDetails.Consignor.OrganisationDetails.Addresses);
			AssertNotNull(shipments[0].ShipmentDetails.NotifyParty);
			AssertNotNull(shipments[0].ShipmentDetails.NotifyParty.Organisation);
			AssertNotNull(shipments[0].ShipmentDetails.NotifyParty.Organisation.OrganisationDetails.Addresses);
			AssertNotNull(shipments[0].ShipmentDetails.PortOfOrigin.Port);
			AssertNotNull(shipments[0].ShipmentDetails.PortofDestination.Port);
			AssertNotNull(shipments[0].ShipmentDetails.Weight);
			AssertNotNull(shipments[0].ShipmentDetails.ChargeableWeight);
			AssertNotNull(shipments[0].ShipmentDetails.Volume);
			AssertNotNull(shipments[0].ShipmentDetails.OrderReferences);
			AssertEquals("DIM17033029", shipments[0].Housebill);
			Xsd.ShipmentIdentifier identifier = shipments[0].ShipmentIdentifier[0];
			AssertEquals(Xsd.ShipmentIdentifierType.Housebill, identifier.ShipmentIdentifierType);
			AssertEquals("DIM17033029", identifier.Value);
			AssertEquals("consignee", shipments[0].ShipmentDetails.Consignee.OwnerCode);
			AssertEquals("consignee name", shipments[0].ShipmentDetails.Consignee.OrganisationDetails.Name);
			Xsd.Organisation consignee = shipments[0].ShipmentDetails.Consignee;
			AssertEquals("CNEE bla Addr1", consignee.OrganisationDetails.Addresses[0].AddressLine1);
			AssertEquals("CNEE bla Addr2", consignee.OrganisationDetails.Addresses[0].AddressLine2);
			AssertEquals("CNEE bla Addr3", consignee.OrganisationDetails.Addresses[0].CityOrSuburb);
			AssertEquals("shipper", shipments[0].ShipmentDetails.Consignor.OwnerCode);
			AssertEquals("shipper name1", shipments[0].ShipmentDetails.Consignor.OrganisationDetails.Name);
			Xsd.Organisation consignor = shipments[0].ShipmentDetails.Consignor;
			AssertEquals("shipper address 1", consignor.OrganisationDetails.Addresses[0].AddressLine1);
			AssertEquals("shipper address 2", consignor.OrganisationDetails.Addresses[0].AddressLine2);
			AssertEquals("shipper address 3", consignor.OrganisationDetails.Addresses[0].CityOrSuburb);
			AssertEquals("notify", shipments[0].ShipmentDetails.NotifyParty.Organisation.OwnerCode);
			AssertEquals("notify name", shipments[0].ShipmentDetails.NotifyParty.Organisation.OrganisationDetails.Name);
			Xsd.ContactReference notify = shipments[0].ShipmentDetails.NotifyParty;
			AssertEquals("NTFYAddr1", notify.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
			AssertEquals("NTFYAddr2", notify.Organisation.OrganisationDetails.Addresses[0].AddressLine2);
			AssertEquals("NTFYAddr3", notify.Organisation.OrganisationDetails.Addresses[0].CityOrSuburb);
			AssertEquals("MARKS", shipments[0].ShipmentDetails.MarksAndNumbers);
			AssertEquals("IMITATION JEWELRY", shipments[0].ShipmentDetails.GoodsDescription);
			AssertEquals("Shipment Port of Origin", "HKHKG", shipments[0].ShipmentDetails.PortOfOrigin.Port.Value);
			AssertEquals("Shipment Port of Destination", "USPVD", shipments[0].ShipmentDetails.PortofDestination.Port.Value);
			AssertEquals("ETD", new ZDateTime(2003, 5, 2), shipments[0].ShipmentDetails.PortOfOrigin.EstimatedDateTime);
			AssertEquals("ETA", new ZDateTime(2003, 5, 3), shipments[0].ShipmentDetails.PortofDestination.EstimatedDateTime);
			AssertEquals("Weight", 110.0m, shipments[0].ShipmentDetails.Weight.Value);
			AssertEquals("ChargeableWeight", 109.0m, shipments[0].ShipmentDetails.ChargeableWeight.Value);
			AssertEquals("Volume", 37.4m, shipments[0].ShipmentDetails.Volume.Value);
			AssertEquals("Weight dimension type", Core.Constants.Volume.CubicMetres, shipments[0].ShipmentDetails.Volume.DimensionType);
			AssertEquals("Weight dimension value", Core.Constants.Weight.Pounds, shipments[0].ShipmentDetails.Weight.DimensionType);
			AssertEquals("OrderReferences", "ASER356", shipments[0].ShipmentDetails.OrderReferences[0]);
			AssertEquals("OrderReferences", "ASER555", shipments[0].ShipmentDetails.OrderReferences[1]);
		}

		public void TestGetOrgAddress()
		{
			Xsd.OrgAddress address = Converter.GetOrgAddress("ADDR1", "ADDR2", "ADDR3", "PHONE", "FAX", "POSTCODE");
			AssertNotNull(address);
			AssertEquals("ADDR1", address.AddressLine1);
			AssertEquals("ADDR2", address.AddressLine2);
			AssertEquals("ADDR3", address.CityOrSuburb);
			AssertEquals("POSTCODE", address.PostCode);
			AssertEquals("PHONE", address.TelephoneNumbers[0].Value);
			AssertEquals(Xsd.TelephoneNumberNumberType.Business, address.TelephoneNumbers[0].NumberType);
			AssertEquals("FAX", address.TelephoneNumbers[1].Value);
			AssertEquals(Xsd.TelephoneNumberNumberType.Fax, address.TelephoneNumbers[1].NumberType);
		}

#region Implementation
#region GetXsdMaster
		BtaXsd.MASTER GetXsdMaster()
		{
			BtaXsd.MASTER result = new BtaXsd.MASTER();
#region master details
			result.MASTER_MASTERNO = "023-07174974";
			result.MASTER_AGENT_CODE = "PASHTET HOOLIGANS FIRM";
			result.MASTER_SHPR_CODE = "DIMSHA";
			result.MASTER_CNEE_NAME = "INTERNATIONAL DELIVERY SERVICES, INC";
			result.MASTER_CNEE_ADDR1 = "CNEE_ADDR1";
			result.MASTER_CNEE_ADDR2 = "CNEE_ADDR2";
			result.MASTER_CNEE_ADDR3 = "CNEE_ADDR3";
			result.MASTER_SHPR_NAME = "DIMERCO SHIPPER";
			result.MASTER_SHPR_ADDR1 = "INTERNATIONAL DELIVERY SERVICES, INC";
			result.MASTER_SHPR_ADDR1 = "SHPR_ADDR1";
			result.MASTER_SHPR_ADDR2 = "SHPR_ADDR2";
			result.MASTER_SHPR_ADDR3 = "SHPR_ADDR3";
			result.MASTER_NTFY_NAME = "DIMERCO SHIPPER";
			result.MASTER_NTFY_ADDR1 = "INTERNATIONAL DELIVERY SERVICES, INC";
			result.MASTER_NTFY_ADDR1 = "NTFY_ADDR1";
			result.MASTER_NTFY_ADDR2 = "NTFY_ADDR2";
			result.MASTER_NTFY_ADDR3 = "NTFY_ADDR3";
			result.MASTER_DEPT = "HKG";
			result.MASTER_DSTN = "PVD";
			result.MASTER_FLTNO1 = "FX012/07/PVD";
			result.MASTER_ETD1 = "20030907";
			result.MASTER_ETA1 = "20030908";
			result.MASTER_FRT_TYPE = "P";
			result.MASTER_TRANSMODE = "A";
#endregion
			result.HOUSE = new BtaXsd.HOUSECollection();
			BtaXsd.HOUSE house = new BtaXsd.HOUSE();
#region house details
			house.HOUSE_HOUSENO = "DIM17-033029";
			house.HOUSE_CNEE_CODE = "consignee";
			house.HOUSE_CNEE_NAME = "consignee name";
			house.HOUSE_CNEE_ADDR1 = "CNEE bla Addr1";
			house.HOUSE_CNEE_ADDR2 = "CNEE bla Addr2";
			house.HOUSE_CNEE_ADDR3 = "CNEE bla Addr3";
			house.HOUSE_CNEE_ADDR4 = "CNEE bla Addr4";
			house.HOUSE_CNEE_ADDR5 = "CNEE bla Addr5";
			house.HOUSE_CNEE_PHONE = "CNe phone";
			house.HOUSE_CNEE_FAX = "cne fax";
			house.HOUSE_SHPR_CODE = "shipper";
			house.HOUSE_SHPR_NAME = "shipper name1";
			house.HOUSE_SHPR_ADDR1 = "shipper address 1";
			house.HOUSE_SHPR_ADDR2 = "shipper address 2";
			house.HOUSE_SHPR_ADDR3 = "shipper address 3";
			house.HOUSE_SHPR_ADDR4 = "shipper address 4";
			house.HOUSE_SHPR_ADDR5 = "shipper address 5";
			house.HOUSE_SHPR_PHONE = "shipper phone";
			house.HOUSE_SHPR_FAX = "shipper fax";
			house.HOUSE_NTFY_CODE = "notify";
			house.HOUSE_NTFY_NAME = "notify name";
			house.HOUSE_NTFY_ADDR1 = "NTFYAddr1";
			house.HOUSE_NTFY_ADDR2 = "NTFYAddr2";
			house.HOUSE_NTFY_ADDR3 = "NTFYAddr3";
			house.HOUSE_NTFY_ADDR4 = "NTFYAddr4";
			house.HOUSE_NTFY_ADDR5 = "NTFYAddr5";
			house.HOUSE_NTFY_PHONE = "notify phone";
			house.HOUSE_NTFY_FAX = "notify fax";
			house.HOUSE_DESCR = "IMITATION JEWELRY";
			house.HOUSE_DSTN = "PVD";
			house.HOUSE_DEPT = "HKG";
			house.HOUSE_FLTNO = "347624";
			house.HOUSE_ETD = "20030502";
			house.HOUSE_ETA = "20030503";
			house.HOUSE_MARKS = "MARKS";
			house.HOUSE_PONO = "ASER356/ASER555";
			house.HOUSE_ATD = "20030906";
			house.HOUSE_FRT_TYPE = "P";
			house.HOUSE_FRT_AMT = "0.00";
			house.HOUSE_WTUOM = "L";
			house.HOUSE_NWT = "0.00";
			house.HOUSE_CWT = "109.00";
			house.HOUSE_GWT = "110.00";
			house.HOUSE_VWT = "37.40";
			house.HOUSE_PCS = "2";
#endregion
			result.HOUSE.Add(house);
			return result;
		}

#endregion
		WFNXmlConverterForTest Converter
		{
			get
			{
				return converter ?? (converter = new WFNXmlConverterForTest(new BusinessObjectFactory()));
			}
		}

		WFNXmlConverterForTest converter;

#region Test Classes
		public class WFNXmlConverterForTest : WFNXmlConverter
		{
			public WFNXmlConverterForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public new void ProcessConsolDetails(Xsd.Consol consol, BtaXsd.MASTER master)
			{
				base.ProcessConsolDetails(consol, master);
			}

			public new Xsd.OrgAddress GetOrgAddress(ZString addr1, ZString addr2, ZString addr3, ZString phone, ZString fax, ZString postcode)
			{
				return base.GetOrgAddress(addr1, addr2, addr3, phone, fax, postcode);
			}

			public new void ProcessShipments(Xsd.ShipmentCollection shipments, BtaXsd.MASTER master)
			{
				base.ProcessShipments(shipments, master);
			}

			public new void AddLeg(Xsd.PlannedLegCollection legs, Xsd.PlannedLegTransportType transportType, BtaXsd.MASTER master)
			{
				base.AddLeg(legs, transportType, master);
			}
		}
#endregion
#endregion
	}
}
