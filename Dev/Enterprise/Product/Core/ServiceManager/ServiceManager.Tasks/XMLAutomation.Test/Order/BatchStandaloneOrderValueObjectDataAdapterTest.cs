using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Forwarding.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	[TestedType(typeof(BatchStandaloneOrderValueObjectDataAdapter))]
	sealed class BatchStandaloneOrderValueObjectDataAdapterTest : OrderValueObjectDataAdapterTest
	{
		public void TestShouldUpdateExistingObject()
		{
			Notification.Clear();
			var order = SetupOrderWithOrderLines();
			var dec = Factory.New<BaseJobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var comInvoiceLine = invoice.InvoiceLines.AddNew();
			comInvoiceLine.JI_JO = order.OrderLines[0].PK;

			Factory.Save();

			Assert("Precondition: registry is not activated", !SystemDataRegistry.Instance.CompleteOrderLineUpdate.Value);

			var orderXSD = CreateOrderXSD(order.JD_OrderNumber, Buyer.OH_Code);

			var context = new ValueObjectImportContext(Factory, Notification);

			var orderreturned = DataAdapter.CreateOrUpdateFromValueObject(orderXSD, context);

			AssertEquals("order returned", order.PK, orderreturned.PK);

			var orderline1 = order.OrderLines.FirstOrDefault(l => l.JO_LineNo == 12);
			AssertNotNull(orderline1);
			AssertEquals("order line with line no 12 is updated", 100m, orderline1.JO_LinePrice);

			SystemDataRegistry.Instance.CompleteOrderLineUpdate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Notification.Clear();

			orderline1.JO_LinePrice = 300m;

			orderreturned = DataAdapter.CreateOrUpdateFromValueObject(orderXSD, context);

			var expectedMessage = "Cannot update Order 111111 as it has order lines that are already linked to commercial invoice lines.";
			AssertEquals("order returned", order.PK, orderreturned.PK);
			AssertEquals("Notification has warnings", true, Notification.HasWarnings);
			AssertContains("Notification should have the expected warning message", expectedMessage, Notification.AsString);

			orderline1 = order.OrderLines.FirstOrDefault(l => l.JO_LineNo == 12);
			AssertNotNull(orderline1);
			AssertEquals("order line with line no 12 shouldn't be updated", 300m, orderline1.JO_LinePrice);
		}

		public void TestShouldUpdateShipmentOrdersDuringAutomaticImport()
		{
			var testHelper = new XmlAndFlatFileTestHelper(Factory);
			var orderXSD = testHelper.CreateOrderXSD();
			var shipment = testHelper.CreateShipmentWithOrder("House bill 1", "Order1");

			SystemDataRegistry.Instance.UpdateShipmentOrdersDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Notification.Clear();
			var matchedOrders = Factory.Load<Order>(testHelper.OrderQuery("Order1", testHelper.Supplier.MainAddress.PK, testHelper.Buyer.MainAddress.PK));
			AssertEquals(ZString.Format("Precondition: order with order no {0}, {1} exists", "Order1", testHelper.Supplier.OH_Code), 1, matchedOrders.Length);
			Assert("order is attached to shipment", matchedOrders[0].IsShipmentAttached);

			var importDataContext = new ValueObjectImportContext(Factory, Notification);

			var result = DataAdapter.CreateOrUpdateFromValueObject(orderXSD, importDataContext);

			AssertEquals("Order returned", matchedOrders[0].PK, result.PK);

			AssertContains("Warning: Order Order1 is linked to a shipment and cannot be updated.", Notification.AsString);
			AssertEquals("Order 's details shouldn't be updated", 1, result.OrderLines.Count);
			AssertEquals("Order 's details shouldn't be updated", Core.Constants.CountryCodes.ChristmasIsland, result.JD_RN_NKCountryOfSupply);
			AssertEquals("Order 's details shouldn't be updated", Core.Constants.IncoTerms.FreeOnBoard, result.JD_IncoTerm);
			AssertEquals("Order 's details shouldn't be updated", 1, result.OrderLines.Count);

			SystemDataRegistry.Instance.UpdateShipmentOrdersDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Notification.Clear();
			result = DataAdapter.CreateOrUpdateFromValueObject(orderXSD, importDataContext);

			AssertEquals("Order returned", matchedOrders[0].PK, result.PK);

			AssertNotContains("Warning: Order Order1 is linked to a shipment and cannot be updated.", Notification.AsString);

			AssertEquals("Order 's details have been updated", 2, result.OrderLines.Count);
			AssertEquals("Order 's details have been updated", Core.Constants.CountryCodes.HongKong, result.JD_RN_NKCountryOfSupply);
			AssertEquals("Order 's details have been updated", Core.Constants.IncoTerms.CostAndInsurance, result.JD_IncoTerm);

			var orderLine1 = result.OrderLines.FirstOrDefault(line => line.JO_LineNo == 1);
			AssertEquals("order line 's details have been updated", "vodka", orderLine1.JO_Partno);
			AssertEquals("order line 's details have been updated", 236.31m, orderLine1.JO_LinePrice);

			result.JD_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			result.JD_RN_NKCountryOfSupply = Core.Constants.CountryCodes.Congo;

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			declaration.JE_JS = shipment.PK;

			Notification.Clear();
			result = dataAdapter.CreateOrUpdateFromValueObject(orderXSD, importDataContext);
			AssertEquals("Order returned", matchedOrders[0].PK, result.PK);

			AssertContains(ZString.Format("{0} is linked to a declaration and cannot be updated", result.HumanReadableName), Notification.AsString);
			AssertEquals("Order 's details shouldn't be updated", Core.Constants.IncoTerms.FreeOnBoard, result.JD_IncoTerm);

			SystemDataRegistry.Instance.UpdateShipmentOrdersDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Notification.Clear();
			result = dataAdapter.CreateOrUpdateFromValueObject(orderXSD, importDataContext);
			AssertEquals("Order returned", matchedOrders[0].PK, result.PK);

			AssertContains(ZString.Format("{0} is linked to a declaration and cannot be updated", result.HumanReadableName), Notification.AsString);
			AssertEquals("Order 's details shouldn't be updated", Core.Constants.IncoTerms.FreeOnBoard, result.JD_IncoTerm);
		}

		public void TestRemoveMissingOrderLines()
		{
			Notification.Clear();
			var order = SetupOrderWithOrderLines();

			var line2 = order.OrderLines.AddNew();
			line2.JO_LineNo = 13;
			line2.JO_Partno = "EGG";
			line2.JO_LinePrice = 100;

			Assert("Precondition: registry is not activated", !SystemDataRegistry.Instance.CompleteOrderLineUpdate.Value);

			var orderXSD = CreateOrderXSD(order.JD_OrderNumber, Buyer.OH_Code);
			AssertEquals("Precondition: Order XSD only has one line", 1, orderXSD.OrderLines.Count);
			AssertEquals("Precondition: Order XSD only have line with line no 12", orderXSD.OrderLines[0].OrderLineNo, 12);

			var context = new ValueObjectImportContext(Factory, Notification);

			var orderreturned = DataAdapter.CreateOrUpdateFromValueObject(orderXSD, context);

			AssertEquals("order returned", order.PK, orderreturned.PK);
			AssertEquals("order remains has 2 order lines even though there is only one line in xsd", 2, order.OrderLines.Count);

			var orderline1Returned = order.OrderLines.FirstOrDefault(l => l.JO_LineNo == 12);
			AssertNotNull(orderline1Returned);
			AssertEquals("order line with line no 12 is updated", 100m, orderline1Returned.JO_LinePrice);

			var orderline2Returned = order.OrderLines.FirstOrDefault(l => l.JO_LineNo == 13);
			AssertNotNull(orderline2Returned);

			SystemDataRegistry.Instance.CompleteOrderLineUpdate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Notification.Clear();

			orderline1Returned.JO_LinePrice = 300m;
			context = new ValueObjectImportContext(Factory, Notification);

			orderreturned = DataAdapter.CreateOrUpdateFromValueObject(orderXSD, context);

			var expectedMessage = "Order line with line no 13 will be removed as it doesn't exist in the XML.";
			AssertEquals("order returned", order.PK, orderreturned.PK);
			AssertEquals("no of order lines", 1, order.OrderLines.Count);

			AssertEquals("Notification has warnings", true, Notification.HasWarnings);
			AssertContains("Notification should have the expected warning message", expectedMessage, Notification.AsString);

			AssertEquals("Order line returned should be no 12", 12, order.OrderLines[0].JO_LineNo);
			AssertEquals("order line with line no 12 should be updated", 100m, order.OrderLines[0].JO_LinePrice);
		}

		#region Implementation

		Order SetupOrderWithOrderLines()
		{
			using var advOrm = AdvOrmFeatureHelper.GetMockedDisposable(isEnabled: true);

			var order = Factory.NewWithValidTestData<Order>(TestBusinessObjectKind.MinimumRequiredToSave);
			order.JD_OrderNumber = "111111";
			order.BuyerPK = Buyer.PK;

			var line1 = order.OrderLines.AddNew();
			line1.JO_LineNo = 12;
			line1.JO_Partno = "PANCAKE";
			line1.JO_LinePrice = 200;

			return order;
		}

		Xsd.Order CreateOrderXSD(ZString orderNum, ZString buyerCode)
		{
			Xsd.Order orderXSD = new Xsd.Order();
			Xsd.OrderOrderIdentifier identifier = new Xsd.OrderOrderIdentifier() { OrderNumber = orderNum };
			orderXSD.OrderIdentifier = identifier;
			orderXSD.OrderDetail.Buyer.EDICode = buyerCode;

			Xsd.OrderOrderLine line1 = orderXSD.OrderLines.AddNew();
			line1.OrderLineNo = 12;
			line1.OrderLineDetail.LinePrice.Value = 100;

			return orderXSD;
		}

		BatchStandaloneOrderValueObjectDataAdapter DataAdapter
		{
			get { return dataAdapter ?? (dataAdapter = new BatchStandaloneOrderValueObjectDataAdapter()); }
		}
		BatchStandaloneOrderValueObjectDataAdapter dataAdapter;

		protected override Type GetDataAdapterType()
		{
			return typeof(BatchStandaloneOrderValueObjectDataAdapter);
		}

		NotificationBuffer Notification
		{
			get { return notification ?? (notification = new NotificationBuffer()); }
		}
		NotificationBuffer notification;

		#endregion
	}
}
