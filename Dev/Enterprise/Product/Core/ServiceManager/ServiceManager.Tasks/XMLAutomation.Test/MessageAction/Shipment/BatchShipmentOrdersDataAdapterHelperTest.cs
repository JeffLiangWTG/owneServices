using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class BatchShipmentOrdersDataAdapterHelperTest : ShipmentOrdersDataAdapterHelperTest
	{
		public void TestCanAttachOrderWhenImportingConsol()
		{
			AssertCanAttachOrder(SystemDataRegistry.Instance.UpdateShipmentOrdersDuringConsolAutomaticImport);
		}

		public void TestCanAttachOrderWhenImportingShipment()
		{
			AssertCanAttachOrder(SystemDataRegistry.Instance.UpdateShipmentOrdersDuringShipmentAutomaticImport);
		}

		void AssertCanAttachOrder(BooleanRegistryItem updateShipmentOrdersFlag)
		{
			updateShipmentOrdersFlag.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Xsd.Order orderXSD = TestHelper.CreateOrderXSD();
			var matchedOrder = Factory.LoadTop1<Order>(TestHelper.OrderQuery(orderXSD.OrderIdentifier.OrderNumber, TestHelper.Supplier.MainAddress.PK, TestHelper.Buyer.MainAddress.PK));

			var shipmentXSD = TestHelper.CreateShipmentXSD("House bill 1", "Order1");
			shipmentXSD.Orders.Add(orderXSD);

			var shipment = TestHelper.CreateShipment("House bill 1", Core.Constants.TransportModes.Sea);
			AssertNull("order with specified order number and buyer/supplier is not found.", matchedOrder);

			var importContext = new ValueObjectImportContext(Factory, TestHelper.Notification);
			var dataAdapter = new OrderValueObjectDataAdapterForBatchShipment(null, updateShipmentOrdersFlag.Value);
			var importArg = new ShipmentOrdersDataAdapterHelper.ImportArgs() { Shipment = shipment, Context = importContext, ShipmentValue = shipmentXSD };

			var dataAdapterHelper = new BatchShipmentOrdersDataAdapterHelperForTest(null, dataAdapter, updateShipmentOrdersFlag.Value);

			Order importedOrder = null;

			AssertEquals("CanAttachToOrder should be true", true, dataAdapterHelper.CanAttachToOrder(importArg, orderXSD, out importedOrder));
			AssertNotNull("Order created", importedOrder);
			AssertEquals("order returned is order - 'Order1'", "Order1", importedOrder.JD_OrderNumber);

			shipment.AttachedOrders.Add(importedOrder);
			Factory.Save();

			importedOrder = null;
			TestHelper.Notification.Clear();

			AssertEquals("CanAttachToOrder should be true", true, dataAdapterHelper.CanAttachToOrder(importArg, orderXSD, out importedOrder));
			AssertNotNull("Order is found", importedOrder);
			AssertEquals("order returned is order - 'Order1'", "Order1", importedOrder.JD_OrderNumber);

			importedOrder = null;
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			declaration.JE_JS = shipment.PK;
			Factory.Save();

			importedOrder = null;
			TestHelper.Notification.Clear();

			AssertEquals("CanAttachToOrder should be false as its parent shipment is already attached to declration", false, dataAdapterHelper.CanAttachToOrder(importArg, orderXSD, out importedOrder));
			AssertNotNull("Order is found", importedOrder);
			AssertEquals("order returned is order - 'Order1'", "Order1", importedOrder.JD_OrderNumber);

			shipment.AttachedOrders.RemoveFromRelationship(importedOrder);

			var anotherShipment = Factory.NewWithValidTestData<ForwardingShipment>(TestBusinessObjectKind.MinimumRequiredToSave);
			anotherShipment.AttachedOrders.Add(importedOrder);
			Factory.Save();

			importedOrder = null;
			TestHelper.Notification.Clear();

			AssertEquals("CanAttachToOrder should be false as order already attached to another shipment", false, dataAdapterHelper.CanAttachToOrder(importArg, orderXSD, out importedOrder));
			AssertNotNull("Order is found", importedOrder);
			AssertEquals("order returned is order - 'Order1'", "Order1", importedOrder.JD_OrderNumber);
		}

		#region Implementation

		class BatchShipmentOrdersDataAdapterHelperForTest : BatchShipmentOrdersDataAdapterHelper
		{
			public BatchShipmentOrdersDataAdapterHelperForTest(EventsWithSourceType triggeredByEvents, IValueObjectDataAdapter orderValueObjectDataAdapter, bool shouldUpdateShipmentOrders)
				: base(triggeredByEvents, orderValueObjectDataAdapter, shouldUpdateShipmentOrders)
			{
			}

			public new bool CanAttachToOrder(ImportArgs importArgs, Xsd.Order orderValue, out Order order)
			{
				return base.CanAttachToOrder(importArgs, orderValue, out order);
			}
		}

		XmlAndFlatFileTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new XmlAndFlatFileTestHelper(Factory)); }
		}
		XmlAndFlatFileTestHelper testHelper;

		#endregion
	}
}
