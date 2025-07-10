using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.CLE.OrdersDataImport.Testing
{
	public class OrderConverterTest : TestCaseWithFactory
	{
		public void TestMapping()
		{
			Xsd.Orders ordersValue = new Xsd.Orders();
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			using (StreamReader reader = new StreamReader(new MemoryStream(resourceRetriever.GetBytes(resourceName))))
			{
				Converter.ImportFlatFile(ordersValue, new CsvFlatFileFormat(), reader);
			}

			AssertEquals("There should be 5 order values in the collection", 5, ordersValue.Order.Count);
			Xsd.Order orderValue = ordersValue.Order[0];
			AssertNotNull(orderValue);
			AssertEquals(2, orderValue.OrderLines.Count);
			AssertEquals("352326/46", orderValue.OrderIdentifier.OrderNumber);
			AssertEquals((byte)0, orderValue.OrderIdentifier.OrderNumberSplit);
			AssertEquals(new ZDateTime(2006, 1, 2), orderValue.OrderDetail.ExWorksRequiredBy);
			AssertEquals(Xsd.OrderTransportMode.AIR, orderValue.OrderDetail.TransportMode);
			AssertEquals(Xsd.OrderContainerMode.LCL, orderValue.OrderDetail.ContainerMode);
			AssertEquals(new ZDateTime(2008, 9, 12), orderValue.OrderDetail.OrderDateTime);
			AssertEquals(new ZDateTime(2008, 3, 4), orderValue.OrderDetail.DeliveryRequiredBy);
			AssertEquals("INC", orderValue.OrderDetail.Incoterm);
			AssertEquals("WORFRESYD", orderValue.OrderDetail.Supplier.OwnerCode);
			AssertEquals("", orderValue.OrderDetail.Supplier.OrganisationDetails.Name);
			AssertEquals("", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].AddressLine1);
			AssertEquals("", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].AddressLine2);
			AssertEquals("", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].CityOrSuburb);
			AssertEquals("", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].StateOrProvince);
			AssertEquals("", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].PostCode);
			AssertEquals("", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].Location.Value);
			AssertEquals("AAAPOWSYD", orderValue.OrderDetail.Buyer.OwnerCode);
			AssertEquals("AAA POWERS PTY LTD", orderValue.OrderDetail.Buyer.OrganisationDetails.Name);
			AssertEquals("ADDRESS", orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].AddressLine1);
			AssertEquals("", orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].AddressLine2);
			AssertEquals("SYDNEY", orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].CityOrSuburb);
			AssertEquals("NSW", orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].StateOrProvince);
			AssertEquals("", orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].PostCode);
			AssertEquals(Xsd.UNLOCO.FromPortCode(Factory, "AUSYD").Value, orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].Location.Value);
			AssertEquals(256.23m, orderValue.OrderDetail.OrderTotal.Value);
			AssertEquals("USD", orderValue.OrderDetail.OrderTotal.CurrencyCode);
			AssertEquals("SYDAPP", orderValue.OrderDetail.Custom.Text5);
			AssertEquals("AUSY", orderValue.OrderDetail.ShipmentPlanning.LoadPort.Value);
			AssertEquals("UAEV", orderValue.OrderDetail.ShipmentPlanning.DischargePort.Value);
			AssertEquals("delivery point", orderValue.OrderDetail.ShipmentPlanning.GoodsDelivTo);
			AssertEquals(1, orderValue.OrderDetail.DocAddresses.DocAddress.Count);
			AssertEquals("Some code...", orderValue.OrderDetail.DocAddresses.DocAddress[0].AddressReference.Organisation.OwnerCode);
			AssertEquals(1, orderValue.OrderDetail.DocAddresses.DocAddress[0].AddressReference.AddressSequenceRef);
			AssertEquals(Xsd.DocAddressAddressType.SCP, orderValue.OrderDetail.DocAddresses.DocAddress[0].AddressType);
			AssertEquals("Contact2", orderValue.OrderDetail.Custom.Contact2);
			Xsd.OrderOrderLine orderLine = orderValue.OrderLines[0];
			AssertEquals(1346, orderLine.OrderLineNo);
			AssertEquals("34.53.63", orderLine.OrderLineDetail.Product);
			AssertEquals("wisky", orderLine.OrderLineDetail.Description);
			AssertEquals(235.53m, orderLine.OrderLineDetail.QtyOrdered.Value);
			AssertEquals("UQ", orderLine.OrderLineDetail.QtyOrdered.DimensionType);
			AssertEquals(8.2m, orderLine.OrderLineDetail.LinePrice.Value);
			AssertEquals(new ZDateTime(2008, 3, 4), orderLine.OrderLineDetail.DropDate);
			AssertEquals("special instractions", orderLine.OrderLineDetail.SpecialInstructions);
			AssertEquals(1, orderLine.OrderLineDeliveries.Count);
			AssertEquals("AAAPOWSYD", orderLine.OrderLineDeliveries[0].DeliveryDetails.Address.Organisation.OwnerCode);
			AssertEquals(1, orderLine.OrderLineDeliveries[0].DeliveryDetails.Address.AddressSequenceRef);
			AssertEquals(1, orderLine.OrderLineDeliveries[0].DeliveryDetails.Address.Organisation.OrganisationDetails.Addresses.Count);
			AssertEquals("delivery point", orderLine.OrderLineDeliveries[0].DeliveryDetails.Address.Organisation.OrganisationDetails.Addresses[0].AddressCode);
			AssertEquals(new ZDateTime(2008, 3, 10), orderLine.OrderLineDetail.Custom.Date1);
			AssertEquals(new ZDateTime(2008, 3, 13), orderLine.OrderLineDetail.Custom.Date5);
			AssertEquals("Attrib5", orderLine.OrderLineDetail.Custom.Text5);
			AssertEquals(23.33m, orderLine.OrderLineDetail.Custom.Decimal5);
			AssertEquals(true, orderLine.OrderLineDetail.Custom.Flag5);
			AssertEquals("customtext1", orderLine.OrderLineDetail.Custom.CustomText1);
			orderLine = orderValue.OrderLines[1];
			AssertEquals(2342, orderLine.OrderLineNo);
			AssertEquals("ffff", orderLine.OrderLineDetail.Product);
			AssertEquals("bear", orderLine.OrderLineDetail.Description);
			orderValue = ordersValue.Order[1];
			AssertEquals("352333/99", orderValue.OrderIdentifier.OrderNumber);
			AssertNotNull(orderValue);
			AssertEquals(1, orderValue.OrderLines.Count);
			AssertEquals("SYDAGESYD", orderValue.OrderDetail.Supplier.OwnerCode);
			AssertEquals("SYDNEY AGENTS", orderValue.OrderDetail.Supplier.OrganisationDetails.Name);
			AssertEquals("SYDNEY AIRPORT", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].AddressLine1);
			AssertEquals("", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].AddressLine2);
			AssertEquals("Sydney", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].CityOrSuburb);
			AssertEquals("NSW", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].StateOrProvince);
			AssertEquals("", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].PostCode);
			AssertEquals(Xsd.UNLOCO.FromPortCode(Factory, "AUSYD").Value, orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].Location.Value);
			AssertEquals("SPORTSMEL", orderValue.OrderDetail.Buyer.OwnerCode);
			AssertEquals("SPORTSGIRL PTY LTD", orderValue.OrderDetail.Buyer.OrganisationDetails.Name);
			AssertEquals("580 CHURCH STREET RICHMOND", orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].AddressLine1);
			AssertEquals("VICTORIA 3121 AUSTRALIA", orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].AddressLine2);
			AssertEquals("Melbourne", orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].CityOrSuburb);
			AssertEquals("VIC", orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].StateOrProvince);
			AssertEquals("", orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].PostCode);
			AssertEquals(Xsd.UNLOCO.FromPortCode(Factory, "AUMEL").Value, orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].Location.Value);
			AssertEquals("AUSSS", orderValue.OrderDetail.ShipmentPlanning.LoadPort.Value);
			AssertEquals("UAIII", orderValue.OrderDetail.ShipmentPlanning.DischargePort.Value);
			AssertEquals(0, orderValue.OrderDetail.DocAddresses.DocAddress.Count);
			AssertEquals("Contact2aa", orderValue.OrderDetail.Custom.Contact2);
			orderLine = orderValue.OrderLines[0];
			AssertEquals(4, orderLine.OrderLineNo);
			AssertEquals("33.33", orderLine.OrderLineDetail.Product);
			AssertEquals("absent", orderLine.OrderLineDetail.Description);
			AssertEquals(2235.53m, orderLine.OrderLineDetail.QtyOrdered.Value);
			AssertEquals(new ZDateTime(2008, 3, 21), orderLine.OrderLineDetail.DropDate);
			AssertEquals("custom1", orderLine.OrderLineDetail.Custom.CustomText1);
			orderValue = ordersValue.Order[2];
			AssertNotNull(orderValue);
			AssertEquals(1, orderValue.OrderLines.Count);
			AssertEquals((byte)1, orderValue.OrderIdentifier.OrderNumberSplit);
			AssertEquals(Xsd.OrderTransportMode.SEA, orderValue.OrderDetail.TransportMode);
			AssertEquals(Xsd.OrderContainerMode.FCL, orderValue.OrderDetail.ContainerMode);
			AssertEquals("FOB", orderValue.OrderDetail.Incoterm);
			AssertEquals("SYDCTOSYD", orderValue.OrderDetail.Supplier.OwnerCode);
			AssertEquals("SYDNEY CTO", orderValue.OrderDetail.Supplier.OrganisationDetails.Name);
			AssertEquals("addresses - all addresses", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].AddressLine1);
			AssertEquals("", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].AddressLine2);
			AssertEquals("Sydney", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].CityOrSuburb);
			AssertEquals("", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].StateOrProvince);
			AssertEquals("", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].PostCode);
			AssertEquals(Xsd.UNLOCO.FromPortCode(Factory, "AUSYD").Value, orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].Location.Value);
			AssertEquals("AUSYD", orderValue.OrderDetail.ShipmentPlanning.LoadPort.Value);
			AssertEquals("UAIEV", orderValue.OrderDetail.ShipmentPlanning.DischargePort.Value);
			orderLine = orderValue.OrderLines[0];
			AssertEquals(2346, orderLine.OrderLineNo);
			AssertEquals("aaaaa", orderLine.OrderLineDetail.Product);
			AssertEquals("absent", orderLine.OrderLineDetail.Description);
			AssertEquals(2245.53m, orderLine.OrderLineDetail.QtyOrdered.Value);
			AssertEquals(new ZDateTime(2008, 3, 22), orderLine.OrderLineDetail.DropDate);
			orderValue = ordersValue.Order[3];
			AssertNotNull(orderValue);
			AssertEquals(1, orderValue.OrderLines.Count);
			AssertEquals((byte)2, orderValue.OrderIdentifier.OrderNumberSplit);
			orderLine = orderValue.OrderLines[0];
			AssertEquals(3163, orderLine.OrderLineNo);
			AssertEquals("aaaaa", orderLine.OrderLineDetail.Product);
			AssertEquals("absent", orderLine.OrderLineDetail.Description);
			AssertEquals(2235.53m, orderLine.OrderLineDetail.QtyOrdered.Value);
			AssertEquals(new ZDateTime(2008, 3, 22), orderLine.OrderLineDetail.DropDate);
			AssertEquals(new ZDateTime(2009, 3, 22), orderLine.OrderLineDetail.Custom.Date2);
			orderValue = ordersValue.Order[4];
			AssertNotNull(orderValue);
			AssertEquals(1, orderValue.OrderLines.Count);
			AssertEquals((byte)3, orderValue.OrderIdentifier.OrderNumberSplit);
			orderLine = orderValue.OrderLines[0];
			AssertEquals(2631, orderLine.OrderLineNo);
			AssertEquals("aaaaa", orderLine.OrderLineDetail.Product);
			AssertEquals("absent", orderLine.OrderLineDetail.Description);
			AssertEquals(2255.53m, orderLine.OrderLineDetail.QtyOrdered.Value);
			AssertEquals(new ZDateTime(2008, 3, 23), orderLine.OrderLineDetail.DropDate);
		}

		CLEOrderDataConverter Converter
		{
			get
			{
				return converter ?? (converter = new CLEOrderDataConverter(Notifications, Factory));
			}
		}

		CLEOrderDataConverter converter;
		NotificationBuffer Notifications
		{
			get
			{
				return notifications ?? (notifications = new NotificationBuffer());
			}
		}

		NotificationBuffer notifications;
		const string resourceName = "order-4splits.csv";
	}
}
