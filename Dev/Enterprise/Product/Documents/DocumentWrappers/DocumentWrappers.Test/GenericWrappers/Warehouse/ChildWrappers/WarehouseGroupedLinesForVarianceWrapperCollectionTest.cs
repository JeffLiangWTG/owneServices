using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseGroupedLinesForVarianceWrapperCollection))]
	sealed class WarehouseGroupedLinesForVarianceWrapperCollectionTest : GenericWrapperCollectionTest<WarehouseGroupedLinesForVarianceWrapperCollection>
	{
		public void TestWarehouseGroupedLinesForVarianceWrapperCollection_DifferentProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 2m;
			data.Part2.OP_Weight = 3m;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine1.WE_TransactionQuantity = 5m;
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 5m);
			Factory.Save();

			var collection = new WarehouseGroupedLinesForVarianceWrapperCollection(receive.Lines, Factory);
			AssertEquals("There should be 2 wrappers in the collection.", 2, collection.Count);

			var wrapper1 = collection.Cast<WarehouseGroupedLinesForVarianceWrapper>().Single(wrapper => wrapper.ProductCode == data.Part1.OP_PartNum);
			AssertEquals(15m, wrapper1.TotalUnits);
			AssertEquals(-5m, wrapper1.TotalVariance);
			AssertEquals(30m, wrapper1.TotalWeight);
			AssertEquals(20m, wrapper1.TotalExpectedReceiptQuantity);

			var wrapper2 = collection.Cast<WarehouseGroupedLinesForVarianceWrapper>().Single(wrapper => wrapper.ProductCode == data.Part2.OP_PartNum);
			AssertEquals(5m, wrapper2.TotalUnits);
			AssertEquals(0m, wrapper2.TotalVariance);
			AssertEquals(15m, wrapper2.TotalWeight);
			AssertEquals(5m, wrapper2.TotalExpectedReceiptQuantity);
		}

		public void TestWarehouseGroupedLinesForVarianceWrapperCollection_DifferentPacksUQ()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 2m;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine1.WE_TransactionQuantity = 5m;
			receiveLine1.WE_F3_NKPackType = "UNT";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine2.WE_F3_NKPackType = "UNT";
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receiveLine3.WE_F3_NKPackType = "CTN";
			Factory.Save();

			var collection = new WarehouseGroupedLinesForVarianceWrapperCollection(receive.Lines, Factory);
			AssertEquals("There should be 2 wrappers in the collection.", 2, collection.Count);

			var wrapper1 = collection.Cast<WarehouseGroupedLinesForVarianceWrapper>().Single(wrapper => wrapper.TotalUnits == 15m);
			AssertEquals(15m, wrapper1.TotalUnits);
			AssertEquals(-5m, wrapper1.TotalVariance);
			AssertEquals(30m, wrapper1.TotalWeight);
			AssertEquals(20m, wrapper1.TotalExpectedReceiptQuantity);

			var wrapper2 = collection.Cast<WarehouseGroupedLinesForVarianceWrapper>().Single(wrapper => wrapper.TotalUnits == 12m);
			AssertEquals(12m, wrapper2.TotalUnits);
			AssertEquals(0m, wrapper2.TotalVariance);
			AssertEquals(24m, wrapper2.TotalWeight);
			AssertEquals(12m, wrapper2.TotalExpectedReceiptQuantity);
		}

		public void TestWarehouseGroupedLinesForVarianceWrapperCollection_DifferentPartAttribute1()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 2m;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine1.WE_TransactionQuantity = 5m;
			receiveLine1.WE_PartAttrib1 = "ABC";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine2.WE_PartAttrib1 = "ABC";
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine3.WE_PartAttrib1 = "DEF";
			Factory.Save();

			var collection = new WarehouseGroupedLinesForVarianceWrapperCollection(receive.Lines, Factory);
			AssertEquals("There should be 2 wrappers in the collection.", 2, collection.Count);

			var wrapper1 = collection.Cast<WarehouseGroupedLinesForVarianceWrapper>().Single(wrapper => wrapper.PartAttribute1 == "ABC");
			AssertEquals(15m, wrapper1.TotalUnits);
			AssertEquals(-5m, wrapper1.TotalVariance);
			AssertEquals(30m, wrapper1.TotalWeight);
			AssertEquals(20m, wrapper1.TotalExpectedReceiptQuantity);

			var wrapper2 = collection.Cast<WarehouseGroupedLinesForVarianceWrapper>().Single(wrapper => wrapper.PartAttribute1 == "DEF");
			AssertEquals(5m, wrapper2.TotalUnits);
			AssertEquals(0m, wrapper2.TotalVariance);
			AssertEquals(10m, wrapper2.TotalWeight);
			AssertEquals(5m, wrapper2.TotalExpectedReceiptQuantity);
		}

		public void TestWarehouseGroupedLinesForVarianceWrapperCollection_DifferentPartAttribute2()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 2m;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine1.WE_TransactionQuantity = 5m;
			receiveLine1.WE_PartAttrib2 = "ABC";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine2.WE_PartAttrib2 = "ABC";
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine3.WE_PartAttrib2 = "DEF";
			Factory.Save();

			var collection = new WarehouseGroupedLinesForVarianceWrapperCollection(receive.Lines, Factory);
			AssertEquals("There should be 2 wrappers in the collection.", 2, collection.Count);

			var wrapper1 = collection.Cast<WarehouseGroupedLinesForVarianceWrapper>().Single(wrapper => wrapper.PartAttribute2WithLabel == "Attribute 2: ABC");
			AssertEquals(15m, wrapper1.TotalUnits);
			AssertEquals(-5m, wrapper1.TotalVariance);
			AssertEquals(30m, wrapper1.TotalWeight);
			AssertEquals(20m, wrapper1.TotalExpectedReceiptQuantity);

			var wrapper2 = collection.Cast<WarehouseGroupedLinesForVarianceWrapper>().Single(wrapper => wrapper.PartAttribute2WithLabel == "Attribute 2: DEF");
			AssertEquals(5m, wrapper2.TotalUnits);
			AssertEquals(0m, wrapper2.TotalVariance);
			AssertEquals(10m, wrapper2.TotalWeight);
			AssertEquals(5m, wrapper2.TotalExpectedReceiptQuantity);
		}

		public void TestWarehouseGroupedLinesForVarianceWrapperCollection_DifferentPartAttribute3()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 2m;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine1.WE_TransactionQuantity = 5m;
			receiveLine1.WE_PartAttrib3 = "ABC";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine2.WE_PartAttrib3 = "ABC";
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine3.WE_PartAttrib3 = "DEF";
			Factory.Save();

			var collection = new WarehouseGroupedLinesForVarianceWrapperCollection(receive.Lines, Factory);
			AssertEquals("There should be 2 wrappers in the collection.", 2, collection.Count);

			var wrapper1 = collection.Cast<WarehouseGroupedLinesForVarianceWrapper>().Single(wrapper => wrapper.PartAttribute3WithLabel == "Attribute 3: ABC");
			AssertEquals(15m, wrapper1.TotalUnits);
			AssertEquals(-5m, wrapper1.TotalVariance);
			AssertEquals(30m, wrapper1.TotalWeight);
			AssertEquals(20m, wrapper1.TotalExpectedReceiptQuantity);

			var wrapper2 = collection.Cast<WarehouseGroupedLinesForVarianceWrapper>().Single(wrapper => wrapper.PartAttribute3WithLabel == "Attribute 3: DEF");
			AssertEquals(5m, wrapper2.TotalUnits);
			AssertEquals(0m, wrapper2.TotalVariance);
			AssertEquals(10m, wrapper2.TotalWeight);
			AssertEquals(5m, wrapper2.TotalExpectedReceiptQuantity);
		}

		public void TestWarehouseGroupedLinesForVarianceWrapperCollection_DifferentPackingDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 2m;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine1.WE_TransactionQuantity = 5m;
			receiveLine1.WE_PackingDate = new ZDate(2020, 1, 1);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine2.WE_PackingDate = new ZDate(2020, 1, 1);
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine3.WE_PackingDate = new ZDate(2020, 2, 1);
			Factory.Save();

			var collection = new WarehouseGroupedLinesForVarianceWrapperCollection(receive.Lines, Factory);
			AssertEquals("There should be 2 wrappers in the collection.", 2, collection.Count);

			var wrapper1 = collection.Cast<WarehouseGroupedLinesForVarianceWrapper>().Single(wrapper => wrapper.PackingDateWithLabel == "Packing Date: 01-Jan-20");
			AssertEquals(15m, wrapper1.TotalUnits);
			AssertEquals(-5m, wrapper1.TotalVariance);
			AssertEquals(30m, wrapper1.TotalWeight);
			AssertEquals(20m, wrapper1.TotalExpectedReceiptQuantity);

			var wrapper2 = collection.Cast<WarehouseGroupedLinesForVarianceWrapper>().Single(wrapper => wrapper.PackingDateWithLabel == "Packing Date: 01-Feb-20");
			AssertEquals(5m, wrapper2.TotalUnits);
			AssertEquals(0m, wrapper2.TotalVariance);
			AssertEquals(10m, wrapper2.TotalWeight);
			AssertEquals(5m, wrapper2.TotalExpectedReceiptQuantity);
		}

		public void TestWarehouseGroupedLinesForVarianceWrapperCollection_DifferentExpiryDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 2m;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine1.WE_TransactionQuantity = 5m;
			receiveLine1.WE_ExpiryDate = new ZDate(2020, 1, 1);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine2.WE_ExpiryDate = new ZDate(2020, 1, 1);
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine3.WE_ExpiryDate = new ZDate(2020, 2, 1);
			Factory.Save();

			var collection = new WarehouseGroupedLinesForVarianceWrapperCollection(receive.Lines, Factory);
			AssertEquals("There should be 2 wrappers in the collection.", 2, collection.Count);

			var wrapper1 = collection.Cast<WarehouseGroupedLinesForVarianceWrapper>().Single(wrapper => wrapper.ExpiryDateWithLabel == "Expiry Date: 01-Jan-20");
			AssertEquals(15m, wrapper1.TotalUnits);
			AssertEquals(-5m, wrapper1.TotalVariance);
			AssertEquals(30m, wrapper1.TotalWeight);
			AssertEquals(20m, wrapper1.TotalExpectedReceiptQuantity);

			var wrapper2 = collection.Cast<WarehouseGroupedLinesForVarianceWrapper>().Single(wrapper => wrapper.ExpiryDateWithLabel == "Expiry Date: 01-Feb-20");
			AssertEquals(5m, wrapper2.TotalUnits);
			AssertEquals(0m, wrapper2.TotalVariance);
			AssertEquals(10m, wrapper2.TotalWeight);
			AssertEquals(5m, wrapper2.TotalExpectedReceiptQuantity);
		}

		public void TestWarehouseGroupedLinesForVarianceWrapperCollection_DifferentStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 2m;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine1.WE_TransactionQuantity = 5m;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine3.WE_WL = data.Whs1.DefaultInboundDockDoorLocation.PK;
			Factory.Save();

			var collection = new WarehouseGroupedLinesForVarianceWrapperCollection(receive.Lines, Factory);
			AssertEquals("There should be 2 wrappers in the collection.", 2, collection.Count);

			var wrapper1 = collection.Cast<WarehouseGroupedLinesForVarianceWrapper>().Single(wrapper => wrapper.Status == InventoryStatus.Codes.Arrived);
			AssertEquals(15m, wrapper1.TotalUnits);
			AssertEquals(-5m, wrapper1.TotalVariance);
			AssertEquals(30m, wrapper1.TotalWeight);
			AssertEquals(20m, wrapper1.TotalExpectedReceiptQuantity);

			var wrapper2 = collection.Cast<WarehouseGroupedLinesForVarianceWrapper>().Single(wrapper => wrapper.Status == InventoryStatus.Codes.Received);
			AssertEquals(5m, wrapper2.TotalUnits);
			AssertEquals(0m, wrapper2.TotalVariance);
			AssertEquals(10m, wrapper2.TotalWeight);
			AssertEquals(5m, wrapper2.TotalExpectedReceiptQuantity);
		}

		#region Implementation

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			var docket = Factory.NewWithValidTestData<WhsReceive>();
			var receiveLine = docket.Lines.AddNew();
			return new WarehouseGroupedLinesForVarianceWrapper(receiveLine, Factory);
		}

		protected override WarehouseGroupedLinesForVarianceWrapperCollection GetNewDocumentWrapperCollection()
		{
			var docket = Factory.NewWithValidTestData<WhsReceive>();
			docket.Lines.AddNew();
			return new WarehouseGroupedLinesForVarianceWrapperCollection(docket.Lines, Factory);
		}

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
