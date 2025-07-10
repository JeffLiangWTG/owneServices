using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class OrderValueObjectDataAdapterForBatchShipmentTest : TestCaseWithFactory
	{
		public void TestShouldUpdateShipmentOrdersWhenImportingConsol()
		{
			AssertShouldUpdateShipmentOrders(SystemDataRegistry.Instance.UpdateShipmentOrdersDuringConsolAutomaticImport);
		}

		public void TestShouldUpdateShipmentOrdersWhenImportingShipment()
		{
			AssertShouldUpdateShipmentOrders(SystemDataRegistry.Instance.UpdateShipmentOrdersDuringShipmentAutomaticImport);
		}

		void AssertShouldUpdateShipmentOrders(BooleanRegistryItem updateShipmentOrdersFlag)
		{
			updateShipmentOrdersFlag.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Notification.Clear();
			var matchedOrders = Factory.Load<Order>(TestHelper.OrderQuery("Order1", TestHelper.Supplier.MainAddress.PK, TestHelper.Buyer.MainAddress.PK));
			AssertEquals(ZString.Format("Precondition: order with order no {0}, {1} exists", "Order1", TestHelper.Supplier.OH_Code), 1, matchedOrders.Length);
			Assert("order is attached to shipment", matchedOrders[0].IsShipmentAttached);

			var dataAdapter = new OrderValueObjectDataAdapterForBatchShipment(null, updateShipmentOrdersFlag.Value);
			var importDataContext = new ValueObjectImportContext(Factory, Notification);

			var result = dataAdapter.CreateOrUpdateFromValueObject(OrderXSD, importDataContext);

			AssertEquals("Order returned", matchedOrders[0].PK, result.PK);

			AssertContains("Warning: Order Order1 is linked to a shipment and cannot be updated.", Notification.AsString);
			AssertEquals("Order 's details shouldn't be updated", 1, result.OrderLines.Count);
			AssertEquals("Order 's details shouldn't be updated", Core.Constants.CountryCodes.ChristmasIsland, result.JD_RN_NKCountryOfSupply);
			AssertEquals("Order 's details shouldn't be updated", Core.Constants.IncoTerms.FreeOnBoard, result.JD_IncoTerm);
			AssertEquals("Order 's details shouldn't be updated", 1, result.OrderLines.Count);

			updateShipmentOrdersFlag.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			dataAdapter = new OrderValueObjectDataAdapterForBatchShipment(null, updateShipmentOrdersFlag.Value);

			Notification.Clear();
			result = dataAdapter.CreateOrUpdateFromValueObject(OrderXSD, importDataContext);

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
			declaration.JE_JS = Shipment.PK;

			Notification.Clear();
			result = dataAdapter.CreateOrUpdateFromValueObject(OrderXSD, importDataContext);
			AssertEquals("Order returned", matchedOrders[0].PK, result.PK);

			AssertContains(ZString.Format("{0} is linked to a declaration and cannot be updated", result.HumanReadableName), Notification.AsString);
			AssertEquals("Order 's details shouldn't be updated", Core.Constants.IncoTerms.FreeOnBoard, result.JD_IncoTerm);

			declaration.JE_JS = Guid.Empty;
			Shipment.AttachedOrders[0].JD_JE = declaration.PK;

			Notification.Clear();
			result = dataAdapter.CreateOrUpdateFromValueObject(OrderXSD, importDataContext);
			AssertEquals("Order returned", matchedOrders[0].PK, result.PK);

			AssertContains(ZString.Format("{0} is linked to a declaration and cannot be updated", result.HumanReadableName), Notification.AsString);
			AssertEquals("Order 's details shouldn't be updated", Core.Constants.IncoTerms.FreeOnBoard, result.JD_IncoTerm);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Shipment = TestHelper.CreateShipmentWithOrder("House bill 1", "Order1");
		}

		XmlAndFlatFileTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new XmlAndFlatFileTestHelper(Factory)); }
		}
		XmlAndFlatFileTestHelper testHelper;

		Xsd.Order OrderXSD
		{
			get { return orderXSD ?? (orderXSD = TestHelper.CreateOrderXSD()); }
		}
		Xsd.Order orderXSD;

		ForwardingShipment Shipment;

		NotificationBuffer Notification
		{
			get { return notification ?? (notification = new NotificationBuffer()); }
		}
		NotificationBuffer notification;

		#endregion
	}
}
