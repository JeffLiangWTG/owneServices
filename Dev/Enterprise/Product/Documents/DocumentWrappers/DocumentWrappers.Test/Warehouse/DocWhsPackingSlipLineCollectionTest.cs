using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ReleaseAndOrderLinePair = System.Collections.Generic.KeyValuePair<Enterprise.Warehouse.Transactions.Business.WhsReleaseLine, Enterprise.Warehouse.Transactions.Business.WhsPickableDocketLine>;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsPackingSlipLineCollection))]
	sealed class DocWhsPackingSlipLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocWhsPackingSlipLineCollection>
	{
		#region Sorting

		#region TestSort

		public void TestSort_Orders()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var p1 = Helper.CreateProduct(data.Org1, "C3");
			p1.OP_Desc = "DESC2";

			var p2 = Helper.CreateProduct(data.Org1, "C1");
			p2.OP_Desc = "DESC3";

			var p3 = Helper.CreateProduct(data.Org1, "C2");
			p3.OP_Desc = "DESC1";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", p1, 10m, true, false);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", p2, 10m, true, false);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", p3, 10m, true, false);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			Helper.CreateWhsOrderLine(order, p1, 1, 1, 0);
			Helper.CreateWhsOrderLine(order, p2, 1, 3, 0);
			Helper.CreateWhsOrderLine(order, p3, 1, 2, 0);

			var tempFactory = new BusinessObjectFactory();
			var temp = new List<ReleaseAndOrderLinePair>();

			CreateReleaseLine(order.Lines[2]);
			CreateReleaseLine(order.Lines[2]);

			foreach (WhsOrderLine line in order.Lines)
			{
				if (line.ReleaseLines.Count == 0)
				{
					CreateReleaseLine(line);
				}
				temp.AddRange(line.ReleaseLines.Select(r => new ReleaseAndOrderLinePair(r, line)));
			}

			var packingLines = new DocWhsPackingSlipLineCollection(temp, tempFactory);
			AssertEquals("Precondition - Ensure packing lines were created", 4, packingLines.Count);

			IPackingSlipWrapperCollection<DocWhsPackingSlipLine> packingSlipCollection = packingLines;
			PackingLineWrapperCollectionHelperLegacy.Sort(packingSlipCollection.Wrappers, order.Lines, "ProductCode");
			AssertEquals("ProductCode Sorting", "00003", packingLines[0].PositionAfterSorting);
			AssertEquals("ProductCode Sorting", "00001", packingLines[1].PositionAfterSorting);
			AssertEquals("ProductCode Sorting", "00002", packingLines[2].PositionAfterSorting);
			AssertEquals("ProductCode Sorting", "00002", packingLines[3].PositionAfterSorting);
			ClearPositionAfterSorting(packingLines);

			PackingLineWrapperCollectionHelperLegacy.Sort(packingSlipCollection.Wrappers, order.Lines, "ProductDescription");
			AssertEquals("ProductDescription Sorting", "00002", packingLines[0].PositionAfterSorting);
			AssertEquals("ProductDescription Sorting", "00003", packingLines[1].PositionAfterSorting);
			AssertEquals("ProductDescription Sorting", "00001", packingLines[2].PositionAfterSorting);
			AssertEquals("ProductDescription Sorting", "00001", packingLines[3].PositionAfterSorting);
			ClearPositionAfterSorting(packingLines);

			PackingLineWrapperCollectionHelperLegacy.Sort(packingSlipCollection.Wrappers, order.Lines, "LineNo");
			AssertEquals("LineNo Sorting", "00001", packingLines[0].PositionAfterSorting);
			AssertEquals("LineNo Sorting", "00003", packingLines[1].PositionAfterSorting);
			AssertEquals("LineNo Sorting", "00002", packingLines[2].PositionAfterSorting);
			AssertEquals("LineNo Sorting", "00002", packingLines[3].PositionAfterSorting);
			ClearPositionAfterSorting(packingLines);

			PackingLineWrapperCollectionHelperLegacy.Sort(packingSlipCollection.Wrappers, order.Lines, "");
			AssertEquals("No Sorting", "00001", packingLines[0].PositionAfterSorting);
			AssertEquals("No Sorting", "00002", packingLines[1].PositionAfterSorting);
			AssertEquals("No Sorting", "00003", packingLines[2].PositionAfterSorting);
			AssertEquals("No Sorting", "00003", packingLines[3].PositionAfterSorting);
		}

		#endregion

		#region TestSort_WithAttributeNeutralRollUp

		public void TestSort_WithAttributeNeutralRollUp()
		{
			var tempFactory = new BusinessObjectFactory();

			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part2.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, 2, 0);
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 2m, 4, 0);
			var line3 = Helper.CreateWhsOrderLine(order, data.Part2, 3m, 1, 0);
			var line4 = Helper.CreateWhsOrderLine(order, data.Part2, 4m, 3, 0);

			CreateReleaseLine(line1);
			CreateReleaseLine(line4);

			var packingLinesCollection = new DocWhsPackingSlipLineCollection(tempFactory);
			var packingLine1 = DocWhsPackingSlipLine.New(line1.ReleaseLines[0], line1, tempFactory);
			var packingLine4 = DocWhsPackingSlipLine.New(line4.ReleaseLines[0], line4, tempFactory);
			packingLinesCollection.Add(packingLine1);
			packingLinesCollection.Add(packingLine4);
			packingLine1.AddParentToRollUp(line2);
			packingLine4.AddParentToRollUp(line3);
			AssertEquals("Precondition - Ensure packing lines were created", 2, packingLinesCollection.Count);

			IPackingSlipWrapperCollection<DocWhsPackingSlipLine> packingSlipCollection = packingLinesCollection;
			PackingLineWrapperCollectionHelperLegacy.Sort(packingSlipCollection.Wrappers, order.Lines, "LineNo");
			AssertEquals("Incorrect position after sorting", "00002", packingLinesCollection[0].PositionAfterSorting);
			AssertEquals("Incorrect position after sorting", "00001", packingLinesCollection[1].PositionAfterSorting);
			ClearPositionAfterSorting(packingLinesCollection);

			PackingLineWrapperCollectionHelperLegacy.Sort(packingSlipCollection.Wrappers, order.Lines, "");
			AssertEquals("Incorrect position after sorting", "00001", packingLinesCollection[0].PositionAfterSorting);
			AssertEquals("Incorrect position after sorting", "00003", packingLinesCollection[1].PositionAfterSorting);
		}

		#endregion

		#region TestSortForWorkOrder

		public void TestSortForWorkOrder()
		{
			BOMDataHelper.CreateBOMProducts();

			// Polish for 1 bike, 2 engines, 4 wheels
			BOMDataHelper.CreateProductInInventory("7 Polish", BOMDataHelper.BOM.Polish, 7m);

			// stock for one bike
			BOMDataHelper.CreateProductInInventory("1 Engine", BOMDataHelper.BOM.BikeEngine, 1m);
			BOMDataHelper.CreateProductInInventory("2 Wheels", BOMDataHelper.BOM.BikeWheel, 2m);

			// stock for 2 engines, includes building of 8 pistons
			BOMDataHelper.CreateProductInInventory("2 Engine Blocks", BOMDataHelper.BOM.EngineBlock, 2m);
			BOMDataHelper.CreateProductInInventory("1 Engine Piston", BOMDataHelper.BOM.EnginePiston, 1m);
			BOMDataHelper.CreateProductInInventory("8 Piston Cranks", BOMDataHelper.BOM.PistonCrank, 8m);
			BOMDataHelper.CreateProductInInventory("8 Piston Heads", BOMDataHelper.BOM.PistonHead, 8m);
			BOMDataHelper.CreateProductInInventory("8 Piston Rings", BOMDataHelper.BOM.PistonRing, 8m);

			// stock for 4 wheels
			BOMDataHelper.CreateProductInInventory("4 Rims", BOMDataHelper.BOM.WheelRim, 4m);
			BOMDataHelper.CreateProductInInventory("4 Tyres", BOMDataHelper.BOM.WheelTyre, 4m);

			Factory.Save();

			// create a work order for 1 bikes to be assembled.
			WhsWorkOrder bikeWorkOrder = Helper.CreateWhsWorkOrder(BOMDataHelper.Org1, BOMDataHelper.Whs1);
			bikeWorkOrder.WD_ExternalReference = "abc";
			Helper.CreateWhsWorkOrderLine(bikeWorkOrder, BOMDataHelper.BOM.Bike, 1m);

			// pick the order.
			Helper.CreatePickNew(bikeWorkOrder);

			var tempFactory = new BusinessObjectFactory();
			var temp = new List<ReleaseAndOrderLinePair>();
			foreach (WhsWorkOrderLine workOrderLine in bikeWorkOrder.AllLines)
			{
				CreateReleaseLine(workOrderLine);
				if (workOrderLine.ReleaseLines.Count == 0)
				{
					CreateReleaseLine(workOrderLine);
				}
				temp.AddRange(workOrderLine.ReleaseLines.Select(r => new ReleaseAndOrderLinePair(r, workOrderLine)));
			}

			var packingLines = new DocWhsPackingSlipLineCollection(temp, tempFactory);
			AssertEquals("Precondition - Ensure packing lines were created", 17, packingLines.Count);

			PackingLineWrapperCollectionHelperLegacy.SortForWorkOrder(packingLines.Cast<DocWhsPackingSlipLine>());
			for (int idx = 0; idx < packingLines.Count; idx++)
			{
				AssertEquals("LineNo Sorting", packingLines[idx].LineNo, packingLines[idx].PositionAfterSorting);
			}
		}

		#endregion

		WhsReleaseLine CreateReleaseLine(WhsPickableDocketLine orderLine)
		{
			return orderLine.ReleaseLines.AddNew();
		}

		void ClearPositionAfterSorting(DocWhsPackingSlipLineCollection collection)
		{
			foreach (DocWhsPackingSlipLine line in collection)
			{
				line.PositionAfterSorting = "";
			}
		}

		#endregion

		// interfaces

		#region TestIPackingSlipWrapperCollection

		public void TestIPackingSlipWrapperCollection()
		{
			IPackingSlipWrapperCollection<DocWhsPackingSlipLine> collection = new DocWhsPackingSlipLineCollection(Factory);
			AssertEquals("Collection should be empty.", 0, collection.Wrappers.Count());

			var orderLine = Factory.New<WhsOrderLine>();
			var wrapper = DocWhsPackingSlipLine.New(new WhsReleaseLine(orderLine), orderLine, Factory);
			collection.Add(wrapper);
			AssertContainsExactElementsInAnyOrder(new[] { wrapper }, collection.Wrappers);
		}

		#endregion

		//

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var receive = Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, Data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference), Data.Part1, 10m);
			AssertEquals("Precondition", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, Data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference), Data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			return DocWhsPackingSlipLine.New(orderLine.ReleaseLines[0], orderLine, Factory);
		}

		TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup Data
		{
			get { return data ?? (data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(Factory)); }
		}
		TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup data;

		protected override DocWhsPackingSlipLineCollection GetCollectionToTest()
		{
			return new DocWhsPackingSlipLineCollection(Factory);
		}

		TestDataForBOM BOMDataHelper
		{
			get { return bomTestHelper ?? (bomTestHelper = new TestDataForBOM(Factory)); }
		}
		TestDataForBOM bomTestHelper;

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion
	}
}
