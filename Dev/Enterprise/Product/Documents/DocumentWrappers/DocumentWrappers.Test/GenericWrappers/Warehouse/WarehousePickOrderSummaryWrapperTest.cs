using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehousePickOrderSummaryWrapper))]
	sealed class WarehousePickOrderSummaryWrapperTest : WarehousePickingSlipWrapperTest
	{
		#region TestDocumentTitleCore

		protected override void TestDocumentTitleCore()
		{
			AssertEquals("Pick Order Summary", PickOrderSummaryWrapper.DocumentTitle);
		}

		#endregion

		#region TestSecondaryHeading

		protected override void TestSecondaryHeadingCore()
		{
			AssertEquals("Pick Details", PickOrderSummaryWrapper.SecondaryHeading);
		}

		#endregion

		#region TestProductLinesCount

		protected override void TestProductLinesCountCore()
		{
			AssertEquals("Precondition: Pick should have no ItemsToPick", 0, Pick.OrderedInventories.Count);
			AssertEquals("ProductLinesCount is incorrect", 0, PickOrderSummaryWrapper.ProductLinesCount);

			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("TST WHS");
			var prod1 = Helper.CreateProduct(client, "P1");
			var prod2 = Helper.CreateProduct(client, "P2");
			var order = Helper.CreateWhsOrder(client, whs);
			Helper.CreateWhsOrderLine(order, prod1, 10m, "TEST1", "DummyOutward-1");
			Helper.CreateWhsOrderLine(order, prod1, 20m, "TEST2", "DummyOutward-2");
			Helper.CreateWhsOrderLine(order, prod2, 30m);

			Pick.Orders.Add(order);
			var wrapper = new WarehousePickOrderSummaryWrapper(Pick, Factory);
			AssertEquals("Precondition: ItemsToPick count is incorrect", 3, Pick.OrderedInventories.Count);
			AssertEquals("ProductLinesCount is incorrect", 2, wrapper.ProductLinesCount);
		}

		public void TestProductLinesCount_KitProductAreAllFromPickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct, 2m, "UNT");

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var kitOrderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var wrapper = new WarehousePickOrderSummaryWrapper(pick, Factory);
			AssertEquals(2, pick.OrderedInventories.Count);
			AssertEquals(1, wrapper.JobLines.Count);
			AssertEquals(1, wrapper.ProductLinesCount);
		}

		public void TestProductLinesCount_KitProductArePartiallyFromPickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct, 2m, "UNT");

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 6m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var kitOrderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var wrapper = new WarehousePickOrderSummaryWrapper(pick, Factory);
			AssertEquals(2, pick.OrderedInventories.Count);
			AssertEquals(2, wrapper.JobLines.Count);
			AssertEquals(2, wrapper.ProductLinesCount);
		}

		#endregion

		#region TestJobLines

		protected override void TestJobLinesCore()
		{
			var pick = (WhsPick)GetNewWhsBusinessObject();
			var wrapper1 = GetNewDocWrapper(pick);
			AssertEquals(0, pick.OrderedInventories.Count);
			AssertEquals(0, wrapper1.JobLines.Count);

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			Helper.CreateWhsOrderLine(order, Helper.CreateProduct(data.Org1, "P3"), 10m);
			pick.Orders.Add(order);

			var wrapper2 = new WarehousePickOrderSummaryWrapper(pick, Factory);
			AssertEquals(3, pick.OrderedInventories.Count);
			AssertEquals(3, wrapper2.JobLines.Count);
		}

		public void TestJobLines_KitProductAreAllFromPickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct, 2m, "UNT");

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var kitOrderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var wrapper = new WarehousePickOrderSummaryWrapper(pick, Factory);
			AssertEquals(2, pick.OrderedInventories.Count);
			AssertEquals("Kits are all picked from Pick by BOM, hide it.", 1, wrapper.JobLines.Count);
			AssertEquals(20m, wrapper.JobLines[0].UnitsOrdered.NativeValue);
			AssertEquals(20m, wrapper.JobLines[0].UnitsPicked.NativeValue);
		}

		public void TestJobLines_KitProductArePartiallyFromPickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct, 2m, "UNT");

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 6m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var kitOrderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var wrapper = new WarehousePickOrderSummaryWrapper(pick, Factory);
			AssertEquals(2, pick.OrderedInventories.Count);
			AssertEquals("Kits are partially picked from Pick by BOM, subtract these qty.", 2, wrapper.JobLines.Count);
			var line1 = wrapper.JobLines.Cast<WarehousePickOrderedInventoryWrapper>().Single(l => l.ProductCode == mainProduct.OP_PartNum);
			var line2 = wrapper.JobLines.Cast<WarehousePickOrderedInventoryWrapper>().Single(l => l.ProductCode == bomComponentProduct.OP_PartNum);
			AssertEquals(6m, line1.UnitsOrdered.NativeValue);
			AssertEquals(6m, line1.UnitsPicked.NativeValue);
			AssertEquals(8m, line2.UnitsOrdered.NativeValue);
			AssertEquals(8m, line2.UnitsPicked.NativeValue);
		}

		public void TestJobLines_KitProductArePartiallyFromPickByBOM_WithShortfall()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct, 2m, "UNT");

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 6m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 4m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var kitOrderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var wrapper = new WarehousePickOrderSummaryWrapper(pick, Factory);
			AssertEquals(2, pick.OrderedInventories.Count);
			AssertEquals("Kits are partially picked from Pick by BOM, subtract these qty.", 2, wrapper.JobLines.Count);
			var line1 = wrapper.JobLines.Cast<WarehousePickOrderedInventoryWrapper>().Single(l => l.ProductCode == mainProduct.OP_PartNum);
			var line2 = wrapper.JobLines.Cast<WarehousePickOrderedInventoryWrapper>().Single(l => l.ProductCode == bomComponentProduct.OP_PartNum);
			AssertEquals(8m, line1.UnitsOrdered.NativeValue);
			AssertEquals(6m, line1.UnitsPicked.NativeValue);
			AssertEquals(4m, line2.UnitsOrdered.NativeValue);
			AssertEquals(4m, line2.UnitsPicked.NativeValue);
		}

		public void TestJobLines_WithShortfall()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var wrapper = new WarehousePickOrderSummaryWrapper(pick, Factory);
			AssertEquals(1, pick.OrderedInventories.Count);
			AssertEquals(1, wrapper.JobLines.Count);
			AssertEquals(10m, wrapper.JobLines[0].UnitsOrdered.NativeValue);
			AssertEquals(6m, wrapper.JobLines[0].UnitsPicked.NativeValue);
		}

		public void TestJobLines_DBHits()
		{
			var count = 10;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var inventoryLocation = data.Whs1.FindLocation("A-1");

			for (var i = 0; i < count; i++)
			{
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, $"R{i}");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation, $"PLT-{i}");
				receive.FinaliseDocketWithoutUserConfirmation();
				AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			}
			Factory.Save();

			var orders = new WhsOrder[count];
			for (var i = 0; i < count; i++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, $"O{i}");
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				orders[i] = order;
			}

			var pick = Helper.CreatePickNew(orders);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			var wrapper = new WarehousePickOrderSummaryWrapper(pickInNewFactory, newFactory);

			var expectedHits = new Dictionary<string, int>
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedHits, newFactory))
			{
				var jobLines = wrapper.JobLines;
				AssertEquals(1, jobLines.Count);
				AssertEquals(100m, jobLines[0].UnitsOrdered.NativeValue);
			}
		}

		#endregion

		#region TestJobs

		protected override void TestJobsCore()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("TST WHS");
			var part = Helper.CreateProduct(client, "Prod");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, whs, part, 10);
			var order2 = Helper.CreateWhsOrderWithOrderLine(client, whs, part, 10);
			var order3 = Helper.CreateWhsOrderWithOrderLine(client, whs, part, 10);

			var pick = (WhsPick)WhsBusinessObject;
			pick.Orders.RemoveAll();
			AssertEquals("Pre-condition", 0, pick.Orders.Count);
			AssertEquals("Pre-condition", 0, PickOrderSummaryWrapper.Jobs.Count);

			pick.Orders.Add(order1);
			pick.Orders.Add(order2);
			pick.Orders.Add(order3);

			var wrapper = new WarehousePickOrderSummaryWrapper(pick, Factory);
			AssertEquals("Precondition: Pick should have 3 Orders", 3, pick.Orders.Count);
			AssertEquals(3, wrapper.Jobs.Count);

			foreach (WhsOrder order in pick.Orders)
			{
				var orderIsMissing = true;
				foreach (WarehousePickableDocketWrapper docketWrapper in wrapper.Jobs)
				{
					if (order == (WhsOrder)docketWrapper.WrappedObject)
					{
						orderIsMissing = false;
						break;
					}
				}
				AssertEquals("Order is not in the wrapped Orders collection", false, orderIsMissing);
			}
		}

		#endregion

		#region TestSecondaryReferenceCore

		protected override void TestSecondaryReferenceCore()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("TST WHS");
			var part = Helper.CreateProduct(client, "Prod");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, whs, part, 10);
			var order2 = Helper.CreateWhsOrderWithOrderLine(client, whs, part, 10);
			var order3 = Helper.CreateWhsOrderWithOrderLine(client, whs, part, 10);

			order1.WD_ExternalReference = "O101";
			order2.WD_ExternalReference = "O102";
			order3.WD_ExternalReference = "O103";

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order1);
			pick.Orders.Add(order2);
			pick.Orders.Add(order3);

			var wrapper = new WarehousePickOrderSummaryWrapper(pick, Factory);
			AssertEquals("Precondition: Pick should have 3 Orders", 3, pick.Orders.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "Order Number|O101", "Order Number|O102", "Order Number|O103" }, wrapper.Jobs.Cast<WarehousePickableDocketWrapper>().Select(j => j.SecondaryReference.Label + "|" + j.SecondaryReference.Value));
			AssertEquals(3, wrapper.Jobs.Count);
		}

		#endregion

		#region Overriden Abstract Members

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return PickOrderSummaryWrapper;
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
CarrierAccount :  is null
CarrierServiceLevel :  is null
Client :  is null
ClientRequestedBillToParty :  is null
CODAmount :  is null
CODType :  is null
ConfirmationInstructions : 
Consignee :  is null
ConsigneeAddress :  is null
Consignor :  is null
CubicSent :  is null
CustomerReference : 
CustomsStatus :  is null
Destination :  is null
DistributionCentreAddress :  is null
DropMode :  is null
DropOffAddress :  is null
FinalisedDate : 
Forwarder :  is null
FulfillRule :  is null
GoodsBillToAddress :  is null
HandlingInstructions : 
IncoTerm :  is null
Insurance :  is null
JobClient :  is null
PackagesSent :  is null
PickingInstructions : 
PickMethod : 
PickNo : 
PickNumberReference : 
PickOption :  is null
PickUpAddress :  is null
PrimaryBarcode : 
Registry : (No Default Field Value Available on Registry)
RequiredDate : 
SalesChannel :  is null
SecondaryReference : 
ServiceLevel :  is null
SOPCarrierServiceLevel : 
SOPOrderNumber : 
SOPRequiredDate : 
SOPSpecialInstructions : 
SOPStagingAreaName : 
SOPTransportCompany : 
SplitNumber : 
StagingAreaName : 
StagingLocationString : 
Status : 
Supplier :  is null
SupplierBuyerLink :  is null
SupplierDocAddress :  is null
TotalExtendedLinePrice :  is null
TotalLoadedPackages : 
TotalLoadedUnits : 
TotalLoadedVolume : 
TotalLoadedWeight : 
TotalOuterPackagesVolume : 
TotalOuterPackagesWeight : 
TransportationUnit :  is null
TransportBillToAddress :  is null
TransportCoAddress :  is null
TransportCompany :  is null
TransportReference : 
VehicleReference : 
Warehouse :  is null
WarehouseName : 
WeightSent :  is null
WhoCreated : 
WhoFinalised :
";
			}
		}

		#endregion

		#region GetNewWarehouseJobGenericWrapper

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WarehousePickOrderSummaryWrapper((WhsPick)bizO, Factory);
		}

		#endregion

		#region PickShortfallItemsWrapper

		WarehousePickOrderSummaryWrapper PickOrderSummaryWrapper
		{
			get { return pickOrderSummaryWrapper ?? (pickOrderSummaryWrapper = (WarehousePickOrderSummaryWrapper)Wrapper); }
		}
		WarehousePickOrderSummaryWrapper pickOrderSummaryWrapper;

		#endregion

	}
}
