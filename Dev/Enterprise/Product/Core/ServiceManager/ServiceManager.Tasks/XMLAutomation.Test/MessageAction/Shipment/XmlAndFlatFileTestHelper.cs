using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class XmlAndFlatFileTestHelper
	{
		public XmlAndFlatFileTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public ZQuery OrderQuery(string orderNumber, ZGuid supplierAddressPK, ZGuid buyerAddressPK)
		{
			var orderQuery = new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, orderNumber);
			orderQuery.AddToFilter(JobOrderHeaderSchema.JD_OA_SupplierAddress, supplierAddressPK);
			orderQuery.AddToFilter(JobOrderHeaderSchema.JD_OA_BuyerAddress, buyerAddressPK);

			return orderQuery;
		}

		public ForwardingShipment CreateShipment(ZString housebill, ZString transportMode)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = housebill;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.ConsignorPK = Supplier.PK;
			shipment.ConsigneePK = Buyer.PK;

			return shipment;
		}

		public ForwardingShipment CreateShipmentWithOrder(ZString housebill, ZString orderNum)
		{
			var shipment = CreateShipment(housebill, orderNum);

			var order = shipment.AttachedOrders.AddNew();
			order.JD_OrderNumber = orderNum;
			order.JD_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			order.JD_RN_NKCountryOfSupply = Core.Constants.CountryCodes.ChristmasIsland;
			order.SupplierPK = Supplier.PK;

			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = 1;
			orderLine.JO_Partno = "ROCKS";
			orderLine.JO_LinePrice = 99.99m;

			shipment.AttachedOrders.Add(order);
			return shipment;
		}

		public Xsd.Shipment CreateShipmentXSD(ZString housebill, ZString transportMode)
		{
			Xsd.Shipment shipmentXSD = new Xsd.Shipment();

			Xsd.ShipmentIdentifier identifier = shipmentXSD.ShipmentIdentifier.AddNew();
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			identifier.Value = housebill;

			shipmentXSD.ShipmentDetails.Consignee.EDICode = Buyer.OH_Code;
			shipmentXSD.ShipmentDetails.Consignor.EDICode = Supplier.OH_Code;

			shipmentXSD.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			shipmentXSD.ShipmentDetails.PortOfOrigin.Port.Value = "HKHKG";
			shipmentXSD.ShipmentDetails.PortofDestination.Port.Value = "AUSYD";

			return shipmentXSD;
		}

		public Xsd.Order CreateOrderXSD()
		{
			Xsd.Order orderXSD = new Xsd.Order();
			orderXSD.OrderIdentifier.OrderNumber = "Order1";
			orderXSD.OrderDetail.Supplier.EDICode = Supplier.OH_Code;
			orderXSD.OrderDetail.Buyer.EDICode = Buyer.OH_Code;
			orderXSD.OrderDetail.CountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			orderXSD.OrderDetail.Incoterm = Core.Constants.IncoTerms.CostAndInsurance;
			orderXSD.OrderDetail.ShipmentPlanning.LoadPort.Value = "HKHKG";
			orderXSD.OrderDetail.ShipmentPlanning.LoadPort.Value = "AUSYD";

			Xsd.OrderOrderLine lineValue = orderXSD.OrderLines.AddNew();
			lineValue.OrderLineNo = 1;
			lineValue.OrderLineDetail.Product = "vodka";
			lineValue.OrderLineDetail.Description = "vodaka descr";
			lineValue.OrderLineDetail.QtyOrdered.Value = 223.35m;
			lineValue.OrderLineDetail.QtyOrdered.DimensionType = "NO";
			lineValue.OrderLineDetail.LinePrice.Value = 236.31m;
			lineValue.OrderLineDetail.DropDate = new ZDateTime(2012, 10, 10);

			Xsd.OrderOrderLine lineValue2 = orderXSD.OrderLines.AddNew();
			lineValue2.OrderLineNo = 2;
			lineValue2.OrderLineDetail.Product = "pencil";
			lineValue2.OrderLineDetail.QtyReceived.Value = 1000m;
			lineValue2.OrderLineDetail.QtyReceived.DimensionType = "PCE";

			return orderXSD;
		}

		public OrgHeader Supplier
		{
			get { return supplier ?? (supplier = Factory.LoadTop1<OrgHeader>(new ZQuery())); }
		}
		OrgHeader supplier;

		public OrgHeader Buyer
		{
			get { return buyer ?? (buyer = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, Supplier.PK))); }
		}
		OrgHeader buyer;

		public NotificationBuffer Notification
		{
			get { return notification ?? (notification = new NotificationBuffer()); }
		}
		NotificationBuffer notification;

		readonly BusinessObjectFactory Factory;
	}
}
