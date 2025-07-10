using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.OSP.Data_Import.Testing
{
	public class OSPCombilineConverterTest : TestCaseWithFactory
	{
		public void TestConvert()
		{
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var testFile = resourceRetriever.GetBytes("80618BIR.TXT");
			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
			OSPCombilineConverter converter = new OSPCombilineConverter(interchange, Factory);
			Xsd.Consol consol = converter.Convert(new StreamReader(new MemoryStream(testFile)), new NotificationBuffer());
			#region Consol Asserts
			AssertNotNull(consol);
			AssertEquals("NOVA", interchange.InterchangeInfo.EDIOrganisation.OwnerCode);
			AssertEquals("MI08910641001", consol.Masterbill);
			AssertEquals("ITLSP", consol.ConsolDetail.PortOfLoading.Port.Value);
			AssertEquals("NZAKL", consol.ConsolDetail.PortOfDischarge.Port.Value);
			AssertEquals(new ZDateTime(2008, 5, 29), consol.ConsolDetail.PortOfLoading.EstimatedDateTime);
			AssertEquals(ZDateTime.Empty, consol.ConsolDetail.PortOfDischarge.EstimatedDateTime);
			AssertEquals(1, consol.ConsolDetail.PlannedLegs.Count);
			AssertEquals("ITLSP", consol.ConsolDetail.PlannedLegs[0].PortOfLoading.Port.Value);
			AssertEquals("NZAKL", consol.ConsolDetail.PlannedLegs[0].PortOfDischarge.Port.Value);
			AssertEquals(new ZDateTime(2008, 5, 29), consol.ConsolDetail.PlannedLegs[0].PortOfLoading.EstimatedDateTime);
			AssertEquals(ZDateTime.Empty, consol.ConsolDetail.PlannedLegs[0].PortOfDischarge.EstimatedDateTime);
			AssertNotNull(consol.ConsolDetail.PlannedLegs[0].Item);
			AssertEquals(new ZDateTime(2008, 5, 29), consol.ConsolDetail.PlannedLegs[0].Item.ETD);
			AssertEquals(ZDateTime.Empty, consol.ConsolDetail.PlannedLegs[0].Item.ETA);
			AssertEquals("CAU", ((Xsd.SailingForPlannedLegs)consol.ConsolDetail.PlannedLegs[0].Item).VesselName);
			AssertEquals("NM117S", ((Xsd.SailingForPlannedLegs)consol.ConsolDetail.PlannedLegs[0].Item).VoyageNo);
			AssertEquals("NOVA", consol.ConsolDetail.SendingAgent.OwnerCode);
			AssertEquals("OSP", consol.ConsolDetail.ReceivingAgent.OwnerCode);
			AssertEquals("040", consol.ConsolDetail.Carrier.OwnerCode);
			AssertEquals(Xsd.ConsolTransportMode.SEA, consol.ConsolDetail.TransportMode);
			AssertEquals(Xsd.ConsolType.Agent, consol.ConsolDetail.ConsolType);
			AssertEquals(Xsd.PaymentType.CCX, consol.ConsolDetail.PaymentType);
			AssertEquals(Xsd.ContainerMode.GRP, consol.ConsolDetail.ContainerMode);
			#endregion
			#region Containers Asserts
			AssertEquals(1, consol.ConsolDetail.Containers.Count);
			AssertEquals("INKU2636164", consol.ConsolDetail.Containers[0].ContainerNumber);
			AssertEquals("HC40", consol.ConsolDetail.Containers[0].ContainerType.ContainerCode);
			AssertEquals(1, consol.ConsolDetail.Containers[0].ContainerCount);
			AssertEquals("0029539", consol.ConsolDetail.Containers[0].Seal);
			AssertEquals(Xsd.ContainerMode.GRP, consol.ConsolDetail.Containers[0].PackingMode);
			#endregion
			#region Shipments Asserts
			AssertEquals(2, consol.Shipments.Count);
			AssertEquals("MI08105981", consol.Shipments[0].Housebill);
			AssertEquals("FO", consol.Shipments[0].ShipmentDetails.Incoterm);
			AssertEquals(Xsd.TransportMode.SEA, consol.Shipments[0].ShipmentDetails.TransportMode);
			AssertEquals(Xsd.ContainerMode.FCL, consol.Shipments[0].ShipmentDetails.PackingMode);
			AssertEquals(new ZDateTime(2008, 5, 7, 15, 14, 0), consol.Shipments[0].ShipmentDetails.Deliver.DeliveryFrom);
			AssertEquals(new ZDateTime(2008, 5, 6), consol.Shipments[0].ShipmentDetails.HBLIssueDate);
			AssertEquals("TEXTILES", consol.Shipments[0].ShipmentDetails.GoodsDescription);
			AssertEquals("CR", consol.Shipments[0].ShipmentDetails.TotalOuterPacksQty.DimensionType);
			AssertEquals(4m, consol.Shipments[0].ShipmentDetails.TotalOuterPacksQty.Value);
			AssertEquals(Core.Constants.Weight.Kilograms, consol.Shipments[0].ShipmentDetails.Weight.DimensionType);
			AssertEquals(Core.Constants.Volume.CubicMetres, consol.Shipments[0].ShipmentDetails.Volume.DimensionType);
			AssertEquals(67.4m, consol.Shipments[0].ShipmentDetails.Weight.Value);
			AssertEquals(0.97m, consol.Shipments[0].ShipmentDetails.Volume.Value);
			AssertEquals("PO # 015117\r\n", consol.Shipments[0].ShipmentDetails.MarksAndNumbers);
			AssertEquals("USD", consol.Shipments[0].ShipmentDetails.GoodsValue.CurrencyCode);
			AssertEquals(352.0m, consol.Shipments[0].ShipmentDetails.GoodsValue.Value);
			AssertEquals("002607", consol.Shipments[0].ShipmentDetails.Consignor.OwnerCode);
			AssertEquals("VELVETEX SPA", consol.Shipments[0].ShipmentDetails.Consignor.OrganisationDetails.Name);
			AssertEquals(1, consol.Shipments[0].ShipmentDetails.Consignor.OrganisationDetails.Addresses.Count);
			AssertEquals("VIA SCHIO - MACROLOTTO", consol.Shipments[0].ShipmentDetails.Consignor.OrganisationDetails.Addresses[0].AddressLine1);
			AssertEquals("PRATO", consol.Shipments[0].ShipmentDetails.Consignor.OrganisationDetails.Addresses[0].CityOrSuburb);
			AssertEquals("NS", consol.Shipments[0].ShipmentDetails.Consignor.OrganisationDetails.Addresses[0].StateOrProvince);
			AssertEquals("59100", consol.Shipments[0].ShipmentDetails.Consignor.OrganisationDetails.Addresses[0].PostCode);
			AssertEquals("BLAHHH", consol.Shipments[0].ShipmentDetails.Consignee.OwnerCode);
			AssertEquals("WARWICK (NEW ZELAND) LTD", consol.Shipments[0].ShipmentDetails.Consignee.OrganisationDetails.Name);
			AssertEquals(1, consol.Shipments[0].ShipmentDetails.Consignee.OrganisationDetails.Addresses.Count);
			AssertEquals("AUCKLAND NEW ZELAND", consol.Shipments[0].ShipmentDetails.Consignee.OrganisationDetails.Addresses[0].AddressLine1);
			AssertEquals("", consol.Shipments[0].ShipmentDetails.Consignee.OrganisationDetails.Addresses[0].CityOrSuburb);
			AssertEquals("BH", consol.Shipments[0].ShipmentDetails.Consignee.OrganisationDetails.Addresses[0].StateOrProvince);
			AssertEquals("90210", consol.Shipments[0].ShipmentDetails.Consignee.OrganisationDetails.Addresses[0].PostCode);
			AssertEquals(consol.Shipments[0].ShipmentDetails.Consignee, consol.Shipments[0].ShipmentDetails.NotifyParty.Organisation);
			AssertEquals(1, consol.Shipments[0].ShipmentDetails.NotifyParty.ContactSequenceRef);
			AssertEquals("MI08106533", consol.Shipments[1].Housebill);
			AssertEquals("TEST PICK UP ORG", consol.Shipments[1].ShipmentDetails.Pickup.Address.CompanyName);
			AssertEquals("S.S. 88 14 2266", consol.Shipments[1].ShipmentDetails.Pickup.Address.AddressLine1);
			AssertEquals("BERLIN", consol.Shipments[1].ShipmentDetails.Pickup.Address.CityOrSuburb);
			AssertEquals("90210", consol.Shipments[1].ShipmentDetails.Pickup.Address.PostCode);
			AssertEquals("FURNITURE", consol.Shipments[1].ShipmentDetails.GoodsDescription);
			AssertEquals("CN", consol.Shipments[1].ShipmentDetails.TotalOuterPacksQty.DimensionType);
			AssertEquals(2m, consol.Shipments[1].ShipmentDetails.TotalOuterPacksQty.Value);
			AssertEquals(Core.Constants.Weight.Kilograms, consol.Shipments[1].ShipmentDetails.Weight.DimensionType);
			AssertEquals(Core.Constants.Volume.CubicMetres, consol.Shipments[1].ShipmentDetails.Volume.DimensionType);
			AssertEquals(275.0m, consol.Shipments[1].ShipmentDetails.Weight.Value);
			AssertEquals(3.6m, consol.Shipments[1].ShipmentDetails.Volume.Value);
			AssertEquals("PO#32193\r\nPO#32193 - TEST CRaP\r\n", consol.Shipments[1].ShipmentDetails.MarksAndNumbers);
			#endregion
			#region Packs Asserts
			AssertEquals(1, consol.Shipments[0].ShipmentDetails.Packages.Count);
			AssertEquals(4, consol.Shipments[0].ShipmentDetails.Packages[0].NumberOfPacks);
			AssertEquals("PO # 015117", consol.Shipments[0].ShipmentDetails.Packages[0].MarksAndNumbers);
			AssertEquals("TEXTILES", consol.Shipments[0].ShipmentDetails.Packages[0].GoodsDescription);
			AssertEquals(Core.Constants.Weight.Kilograms, consol.Shipments[0].ShipmentDetails.Packages[0].Weight.DimensionType);
			AssertEquals(Core.Constants.Volume.CubicMetres, consol.Shipments[0].ShipmentDetails.Packages[0].Volume.DimensionType);
			AssertEquals(67.4m, consol.Shipments[0].ShipmentDetails.Packages[0].Weight.Value);
			AssertEquals(0.97m, consol.Shipments[0].ShipmentDetails.Packages[0].Volume.Value);
			AssertEquals("INKU2636164", consol.Shipments[0].ShipmentDetails.Packages[0].ContainerNumber);
			AssertEquals("CR", consol.Shipments[0].ShipmentDetails.Packages[0].PackType);
			AssertEquals(2, consol.Shipments[1].ShipmentDetails.Packages.Count);
			AssertEquals(1, consol.Shipments[1].ShipmentDetails.Packages[0].NumberOfPacks);
			AssertEquals("PO#32193", consol.Shipments[1].ShipmentDetails.Packages[0].MarksAndNumbers);
			AssertEquals("FURNITURE", consol.Shipments[1].ShipmentDetails.Packages[0].GoodsDescription);
			AssertEquals(Core.Constants.Weight.Kilograms, consol.Shipments[1].ShipmentDetails.Packages[0].Weight.DimensionType);
			AssertEquals(Core.Constants.Volume.CubicMetres, consol.Shipments[1].ShipmentDetails.Packages[0].Volume.DimensionType);
			AssertEquals(85.0m, consol.Shipments[1].ShipmentDetails.Packages[0].Weight.Value);
			AssertEquals(1.2m, consol.Shipments[1].ShipmentDetails.Packages[0].Volume.Value);
			AssertEquals("INKU2636164", consol.Shipments[1].ShipmentDetails.Packages[0].ContainerNumber);
			AssertEquals("CN", consol.Shipments[1].ShipmentDetails.Packages[0].PackType);
			AssertEquals(1, consol.Shipments[1].ShipmentDetails.Packages[1].NumberOfPacks);
			AssertEquals("PO#32193 - TEST CRaP", consol.Shipments[1].ShipmentDetails.Packages[1].MarksAndNumbers);
			AssertEquals("FURNITURE\r\nWHITE, SIKASIL,\r\n000000000000000000", consol.Shipments[1].ShipmentDetails.Packages[1].GoodsDescription);
			AssertEquals(Core.Constants.Weight.Kilograms, consol.Shipments[1].ShipmentDetails.Packages[1].Weight.DimensionType);
			AssertEquals(Core.Constants.Volume.CubicMetres, consol.Shipments[1].ShipmentDetails.Packages[1].Volume.DimensionType);
			AssertEquals(190.0m, consol.Shipments[1].ShipmentDetails.Packages[1].Weight.Value);
			AssertEquals(2.4m, consol.Shipments[1].ShipmentDetails.Packages[1].Volume.Value);
			AssertEquals("INKU2636164", consol.Shipments[1].ShipmentDetails.Packages[1].ContainerNumber);
			AssertEquals("CN", consol.Shipments[1].ShipmentDetails.Packages[1].PackType);
			#endregion
		}
	}
}
