using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehousePackingSlipLineWrapperCollection))]
	sealed class WarehousePackingSlipLineWrapperCollectionTest : GenericWrapperCollectionTest<WarehousePackingSlipLineWrapperCollection>
	{
		#region TestIPackingSlipWrapperCollection

		public void TestIPackingSlipWrapperCollection()
		{
			IPackingSlipWrapperCollection<WarehousePackingSlipLineWrapper> collection = new WarehousePackingSlipLineWrapperCollection(Factory);
			AssertEquals("Collection should be empty.", 0, collection.Wrappers.Count());

			var orderLine = Factory.New<WhsOrderLine>();
			var wrapper = new WarehousePackingSlipLineWrapper(new WhsReleaseLine(orderLine), orderLine, Factory);
			collection.Add(wrapper);
			AssertContainsExactElementsInAnyOrder(new[] { wrapper }, collection.Wrappers);
		}

		#endregion

		#region TestTotalExtendedLinePrice

		public void TestTotalExtendedLinePrice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			line1.WE_ExtendedLinePrice = 2.5m;
			line2.WE_ExtendedLinePrice = 3.0m;
			Helper.CreatePickNew(order);

			var pairs = new[]
			{
				new KeyValuePair<WhsReleaseLine, WhsPickableDocketLine>(line1.ReleaseLines[0], line1),
				new KeyValuePair<WhsReleaseLine, WhsPickableDocketLine>(line2.ReleaseLines[0], line2),
			};
			var wrapper = new WarehousePackingSlipLineWrapperCollection(pairs, Factory);
			AssertEquals("Total Price should be: 2.5m + 3.0m.", 5.5m, wrapper.TotalExtendedLinePrice.Amount);
		}

		public void TestTotalExtendedLinePrice_RecalculateOrderPricing()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Factory.Save();

			data.Org1.MiscServ.OM_WhsIsRecalculateOrderPricing = true;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			line1.WE_UnitPriceAfterDiscount = 2.5m;
			line2.WE_UnitPriceAfterDiscount = 3.0m;
			Helper.CreatePickNew(order);

			var pairs = new[]
			{
				new KeyValuePair<WhsReleaseLine, WhsPickableDocketLine>(line1.ReleaseLines[0], line1),
				new KeyValuePair<WhsReleaseLine, WhsPickableDocketLine>(line2.ReleaseLines[0], line2),
			};
			var wrapper = new WarehousePackingSlipLineWrapperCollection(pairs, Factory);
			AssertEquals("Total Price should be: 5 * 2.5m + 5 * 3.0m.", 27.5m, wrapper.TotalExtendedLinePrice.Amount);
		}

		public void TestTotalExtendedLinePrice_MultipleReleaseLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, today.AddDays(1), today, "A1", "A2", "A3", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, today.AddDays(1), today, "B1", "B2", "B3", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, today.AddDays(1), today, "C1", "C2", "C3", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, today.AddDays(1), today, "D1", "D2", "D3", "");
			Factory.Save();

			data.Org1.MiscServ.OM_WhsIsRecalculateOrderPricing = true;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			Helper.CreatePickNew(order);
			Helper.CreateWhsPickLine(line1, receive.Inventory[0], 3m);
			Helper.CreateWhsPickLine(line1, receive.Inventory[1], 2m);
			Helper.CreateWhsPickLine(line2, receive.Inventory[2], 3m);
			Helper.CreateWhsPickLine(line2, receive.Inventory[3], 2m);

			line1.WE_UnitPriceAfterDiscount = 2.2m;
			line2.WE_UnitPriceAfterDiscount = 3.5m;

			var pairs = new[]
			{
				new KeyValuePair<WhsReleaseLine, WhsPickableDocketLine>(line1.ReleaseLines[0], line1),
				new KeyValuePair<WhsReleaseLine, WhsPickableDocketLine>(line1.ReleaseLines[1], line1),
				new KeyValuePair<WhsReleaseLine, WhsPickableDocketLine>(line2.ReleaseLines[0], line2),
				new KeyValuePair<WhsReleaseLine, WhsPickableDocketLine>(line2.ReleaseLines[1], line2),
			};
			var wrapper = new WarehousePackingSlipLineWrapperCollection(pairs, Factory);

			AssertEquals("Total Price should be: 5 * 2.2m + 5 * 3.5m.", 28.5m, wrapper.TotalExtendedLinePrice.Amount);
		}

		#endregion

		#region Implementation

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			var orderLine = Factory.NewWithValidTestData<WhsOrderLine>();
			return new WarehousePackingSlipLineWrapper(orderLine.ReleaseLines.AddNew(), orderLine, Factory);
		}

		protected override WarehousePackingSlipLineWrapperCollection GetNewDocumentWrapperCollection()
		{
			var docket = Factory.NewWithValidTestData<WhsPickableDocket>();
			var line = docket.AllLines.AddNew();
			var releaseLine = line.ReleaseLines.AddNew();

			return new WarehousePackingSlipLineWrapperCollection(new[] { new KeyValuePair<WhsReleaseLine, WhsPickableDocketLine>(releaseLine, line) }, Factory);
		}

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
