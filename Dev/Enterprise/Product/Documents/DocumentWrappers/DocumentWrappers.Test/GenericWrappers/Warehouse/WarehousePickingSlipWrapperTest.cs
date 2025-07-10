using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Barcode.Business;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Warehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehousePickingSlipWrapper))]
	public class WarehousePickingSlipWrapperTest : WarehouseJobGenericWrapperTest
	{
		#region Related Business Objects

		#region Collections

		protected override void TestPackagesCore()
		{
			base.TestPackagesCore();

			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			Factory.Save();

			Pick.Orders.Add(order1);
			Pick.Orders.Add(order2);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			Factory.Save();

			var order1package1 = Helper.CreatePackage("KEG", "123", packageJob1.Packages);
			var order1package2 = Helper.CreatePackage("KEG", "456", order1package1.Packages);
			var order1package3 = Helper.CreatePackage("KEG", "789", order1package2.Packages);
			var order1package4 = Helper.CreatePackage("KEG", "123", packageJob1.Packages);
			packageJob1.Selected.UpdateSelectedPackages(new[] { order1package1, order1package4 });

			var order2package1 = Helper.CreatePackage("KEG", "123", packageJob2.Packages);
			var order2package2 = Helper.CreatePackage("KEG", "456", order2package1.Packages);
			var order2package3 = Helper.CreatePackage("KEG", "789", order2package2.Packages);
			var order2package4 = Helper.CreatePackage("KEG", "123", packageJob2.Packages);

			var wrapper = new WarehousePickingSlipWrapper(Pick, Factory);
			AssertContainsExactElementsInAnyOrder(
				new PkgPackage[] { order1package1, order1package2, order1package3, order1package4, order2package1, order2package2, order2package3, order2package4 },
				wrapper.Packages.Cast<PackageWrapper>().Select(item => item.WrappedObject));
		}

		public void TestPackagesAreSorted()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Factory.Save();

			Pick.Orders.Add(order);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			Factory.Save();

			var package1 = Helper.CreatePackage("KEG", "KEG - 4", packageJob.Packages);
			var package2 = Helper.CreatePackage("KEG", "KEG - 2", package1.Packages);
			var package3 = Helper.CreatePackage("KEG", "KEG - 1", package2.Packages);
			var package4 = Helper.CreatePackage("KEG", "KEG - 3", packageJob.Packages);

			CombineAssertions("Packages should be sorted", delegate
			{
				var packages = DocWrapper.Packages;
				AssertEquals("packages[0].RefNumber", "KEG - 1", packages[0].RefNumber);
				AssertEquals("packages[1].RefNumber", "KEG - 2", packages[1].RefNumber);
				AssertEquals("packages[2].RefNumber", "KEG - 3", packages[2].RefNumber);
				AssertEquals("packages[3].RefNumber", "KEG - 4", packages[3].RefNumber);
			});
		}

		#endregion

		#region TestOrders

		protected override void TestOrdersCore()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("TST WHS");
			var part = Helper.CreateProduct(client, "Prod");
			Factory.Save();

			var pick = (WhsPick)WhsBusinessObject;
			pick.Orders.RemoveAll();
			AssertEquals("Precondition: Pick shoud have no orders", 0, pick.Orders.Count);

			var docWrapper = GetNewDocWrapper(pick);
			AssertEquals("DocWrapper Orders count is incorrect", 0, docWrapper.Orders.Count);

			var order1 = Helper.CreateWhsOrder(client, whs);
			Helper.CreateWhsOrderLine(order1, part, 10m);

			var order2 = Helper.CreateWhsOrder(client, whs);
			Helper.CreateWhsOrderLine(order2, part, 10m);

			var order3 = Helper.CreateWhsOrder(client, whs);
			Helper.CreateWhsOrderLine(order3, part, 10m);

			pick.Orders.Add(order1);
			pick.Orders.Add(order2);
			pick.Orders.Add(order3);

			AssertEquals("Precondition: Pick should have 3 Orders", 3, pick.Orders.Count);
			AssertEquals("DocWrapper Orders Count is incorrect", 3, docWrapper.Orders.Count);

			foreach (WhsOrder order in Pick.Orders)
			{
				var orderIsMissing = true;
				foreach (WarehouseOrderWrapper orderWrapper in docWrapper.Orders)
				{
					if (order == (WhsOrder)orderWrapper.WrappedObject)
					{
						orderIsMissing = false;
						break;
					}
					AssertEquals("Order is not in the wrapped Orders collection", true, orderIsMissing);
				}
			}
		}

		public void TestOrders_WorkOrder()
			=> TestOrders_WorkOrder<WarehouseWorkOrderWrapper>(data => Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.BikeWheel, 1));

		public void TestOrders_DynamicWorkOrder()
			=> TestOrders_WorkOrder<WarehouseDynamicWorkOrderWrapper>(data => Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.BikeWheel, 1));

		void TestOrders_WorkOrder<T>(Func<TestDataForBOM, WhsComponentOrder> createWorkOrder)
			where T : WarehousePickableDocketWrapper
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMSubComponentsInInventory();
			Factory.Save();

			var workOrder = createWorkOrder(data);
			var pick = Helper.CreatePickNew(workOrder);

			var wrapper = GetNewWarehouseJobGenericWrapper(pick);
			var orders = wrapper.Orders;
			var singleOrderWrapper = orders.Single();
			AssertType<T>(singleOrderWrapper);
			AssertEquals(workOrder, ((T)singleOrderWrapper).WrappedObject);
		}

		#endregion

		#region TestPickingLines

		#region TestPickingLines

		protected override void TestPickingLinesCore()
		{
			var data = new TestDataForInventory(Factory);

			data.Whs1 = Helper.CreateWarehouse("Warehouse");
			data.Org1 = Helper.CreateClient("Client");
			data.Part1 = Helper.CreateProduct(data.Org1, "Product 1");
			data.Part2 = Helper.CreateProduct(data.Org1, "Product 2");

			var row1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "A", 2, 2, 2);
			var row2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 2, 2);

			data.CreateSimpleInventoryManyLines(data.Whs1, data.Org1, data.Part1, new ZDecimal[] { 10, 10, 10, 10, 10 }, "1");
			data.Receive11.Lines[0].WE_WL = row1.Locations[0].PK;
			data.Receive11.Lines[1].WE_WL = row1.Locations[0].PK;
			data.Receive11.Lines[2].WE_WL = row1.Locations[1].PK;
			data.Receive11.Lines[3].WE_WL = row1.Locations[2].PK;
			data.Receive11.Lines[4].WE_WL = row1.Locations[3].PK;

			data.CreateSimpleInventoryManyLines(data.Whs1, data.Org1, data.Part2, new ZDecimal[] { 20, 20, 20, 20, 20 }, "2");
			data.Receive11.Lines[0].WE_WL = row2.Locations[0].PK;
			data.Receive11.Lines[1].WE_WL = row2.Locations[1].PK;
			data.Receive11.Lines[2].WE_WL = row2.Locations[2].PK;
			data.Receive11.Lines[3].WE_WL = row2.Locations[3].PK;
			data.Receive11.Lines[4].WE_WL = row2.Locations[4].PK;

			data.CreateSimpleInventoryManyLines(data.Whs1, data.Org1, data.Part2, new ZDecimal[] { 30, 30 }, "3");
			data.Receive11.Lines[0].WE_WL = row2.Locations[0].PK;
			data.Receive11.Lines[1].WE_WL = row2.Locations[1].PK;

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "W1");
			var orderLine11 = Helper.CreateWhsOrderLine(order1, data.Part1, 50m);
			var orderLine12 = Helper.CreateWhsOrderLine(order1, data.Part2, 80m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "W2");
			var orderLine21 = Helper.CreateWhsOrderLine(order2, data.Part2, 50m);
			var orderLine22 = Helper.CreateWhsOrderLine(order2, data.Part2, 30m);
			Factory.Save();

			Pick.Orders.Add(order1);
			Pick.Orders.Add(order2);
			Pick.AutoAllocateItemsWithMock();

			AssertEquals("Document Wrapper Lines Collection should have other count", 9, DocWrapper.PickingLines.Count);
			AssertEquals("Document Wrapper Line must have specific Units value", 20m, DocWrapper.PickingLines[0].Units);
			AssertEquals("Document Wrapper Line must have specific Units value", 10m, DocWrapper.PickingLines[1].Units);
			AssertEquals("Document Wrapper Line must have specific Units value", 10m, DocWrapper.PickingLines[2].Units);
			AssertEquals("Document Wrapper Line must have specific Units value", 10m, DocWrapper.PickingLines[3].Units);
			AssertEquals("Document Wrapper Line must have specific Units value", 50m, DocWrapper.PickingLines[4].Units);
			AssertEquals("Document Wrapper Line must have specific Units value", 50m, DocWrapper.PickingLines[5].Units);
			AssertEquals("Document Wrapper Line must have specific Units value", 20m, DocWrapper.PickingLines[6].Units);
			AssertEquals("Document Wrapper Line must have specific Units value", 20m, DocWrapper.PickingLines[7].Units);
			AssertEquals("Document Wrapper Line must have specific Units value", 20m, DocWrapper.PickingLines[8].Units);
		}

		public void TestPickingLinesCoreWithoutAllocatedLines()
		{
			// setup warehouse and locations
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_VerifyEmptyLocations = true;
			var location = data.Whs1.FindLocation("A");

			// setup receive, order and pick
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			receive.FinaliseDocket();
			Factory.Save();
			AssertEquals("Precondition - Receive should be Finalised.", true, receive.IsFinalised);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order);
			var docWrapper = GetNewDocWrapper(pick);
			AssertNoExceptionThrown(
				"No exception should be thrown if document is generated without allocated pick lines",
				() =>
				{
					var count = docWrapper.PickingLines.Count;
				});
			AssertEquals("There should be no picklines.", 0, docWrapper.PickingLines.Count);
		}

		public void TestPickingLines_DoesNotReturnPickByBOMKitPickLines()
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

			AssertEquals("Precondition: 2 pick lines in total.", 2, pick.GetAllPickLines().Count());

			var docWrapper = GetNewDocWrapper(pick);
			AssertNoExceptionThrown("No exception should be thrown if document is generated without allocated pick lines", () => { var count = docWrapper.PickingLines.Count; });
			AssertEquals("Should only return the component pick line.", 1, docWrapper.PickingLines.Count);
			AssertEquals("Should only return the component pick line.", 20m, docWrapper.PickingLines[0].Units);
		}

		public void TestPickingLines_DoesNotReturnPickByBOMKitPickLines_WithExistingMainProductInventory()
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

			AssertEquals("Precondition: 3 pick lines in total.", 3, pick.GetAllPickLines().Count());

			var docWrapper = GetNewDocWrapper(pick);
			AssertNoExceptionThrown("No exception should be thrown if document is generated without allocated pick lines", () => { var count = docWrapper.PickingLines.Count; });
			AssertEquals("Should only return the component pick line and the kit pick line picking existing inventory.", 2, docWrapper.PickingLines.Count);
			var line1 = docWrapper.PickingLines.Cast<WarehousePickingSlipLineWrapper>().Single(l => l.ProductCode == mainProduct.OP_PartNum);
			var line2 = docWrapper.PickingLines.Cast<WarehousePickingSlipLineWrapper>().Single(l => l.ProductCode == bomComponentProduct.OP_PartNum);
			AssertEquals(6m, line1.Units);
			AssertEquals(8m, line2.Units);
		}

		public void TestPickingLines_DBHits()
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
			var docWrapper = new WarehousePickingSlipWrapper(pickInNewFactory, newFactory);

			var expectedHits = new Dictionary<string, int>
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedHits, newFactory))
			{
				var pickLines = docWrapper.PickingLines;
				AssertEquals(10, pickLines.Count);
			}
		}

		public void TestPickingLines_DoesNotReturnReservedPickLines()
		{
			var whs = Helper.CreateWarehouse("wh1", "a", 2, 2);
			var org = Helper.CreateClient("ABC");
			Factory.Save();

			var part1 = Helper.CreateProduct(org, "a");
			var part2 = Helper.CreateProduct(org, "b");
			var part3 = Helper.CreateProduct(org, "c");
			var part4 = Helper.CreateProduct(org, "d");
			var part5 = Helper.CreateProduct(org, "e");
			var part6 = Helper.CreateProduct(org, "f");
			var part7 = Helper.CreateProduct(org, "g");
			var part8 = Helper.CreateProduct(org, "h");
			var part9 = Helper.CreateProduct(org, "i");
			var part10 = Helper.CreateProduct(org, "j");
			var part11 = Helper.CreateProduct(org, "k");
			var part12 = Helper.CreateProduct(org, "l");
			var part13 = Helper.CreateProduct(org, "m");
			var part14 = Helper.CreateProduct(org, "n");
			var part15 = Helper.CreateProduct(org, "o");
			var part16 = Helper.CreateProduct(org, "p");
			var part17 = Helper.CreateProduct(org, "q");

			var location = whs.FindLocation("a-1-1");
			AssertNotNull("Location should not be null", location);

			var received1 = Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part1, 1m, null, "", false, false);
			var received2 = Helper.CreateWhsReceiveWithInventory(org, whs, "R2", part2, 1m, null, "", false, false);
			var received3 = Helper.CreateWhsReceiveWithInventory(org, whs, "R3", part3, 1m, null, "", false, false);
			var received4 = Helper.CreateWhsReceiveWithInventory(org, whs, "R4", part4, 1m, null, "", false, false);
			var received5 = Helper.CreateWhsReceiveWithInventory(org, whs, "R5", part5, 1m, null, "", false, false);
			var received6 = Helper.CreateWhsReceiveWithInventory(org, whs, "R6", part6, 1m, null, "", false, false);
			var received7 = Helper.CreateWhsReceiveWithInventory(org, whs, "R7", part7, 1m, null, "", false, false);
			var received8 = Helper.CreateWhsReceiveWithInventory(org, whs, "R8", part8, 1m, null, "", false, false);
			var received9 = Helper.CreateWhsReceiveWithInventory(org, whs, "R9", part9, 1m, location, "a");
			var received10 = Helper.CreateWhsReceiveWithInventory(org, whs, "R10", part10, 1m, location, "a");
			var received11 = Helper.CreateWhsReceiveWithInventory(org, whs, "R11", part11, 1m, location, "a");
			var received12 = Helper.CreateWhsReceiveWithInventory(org, whs, "R12", part12, 1m, location, "a");
			var received13 = Helper.CreateWhsReceiveWithInventory(org, whs, "R13", part13, 1m, location, "a");
			var received14 = Helper.CreateWhsReceiveWithInventory(org, whs, "R14", part14, 1m, null, "", false, false);
			var received15 = Helper.CreateWhsReceiveWithInventory(org, whs, "R15", part15, 1m, null, "", false, false);
			var received16 = Helper.CreateWhsReceiveWithInventory(org, whs, "R16", part16, 1m, location, "a");
			var received17 = Helper.CreateWhsReceiveWithInventory(org, whs, "R17", part17, 1m, location, "a");

			var inventory1 = received1.Inventory[0];
			var inventory2 = received2.Inventory[0];
			var inventory3 = received3.Inventory[0];
			var inventory4 = received4.Inventory[0];
			var inventory5 = received5.Inventory[0];
			var inventory6 = received6.Inventory[0];
			var inventory7 = received7.Inventory[0];
			var inventory8 = received8.Inventory[0];
			var inventory14 = received14.Inventory[0];
			var inventory15 = received15.Inventory[0];

			var order = Helper.CreateWhsOrder(org, whs);
			var orderLine1 = Helper.CreateWhsOrderLine(order, part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, part2, 1m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, part3, 1m);
			var orderLine4 = Helper.CreateWhsOrderLine(order, part4, 1m);
			var orderLine5 = Helper.CreateWhsOrderLine(order, part5, 1m);
			var orderLine6 = Helper.CreateWhsOrderLine(order, part6, 1m);
			var orderLine7 = Helper.CreateWhsOrderLine(order, part7, 1m);
			var orderLine8 = Helper.CreateWhsOrderLine(order, part8, 1m);
			var orderLine9 = Helper.CreateWhsOrderLine(order, part9, 1m);
			var orderLine10 = Helper.CreateWhsOrderLine(order, part10, 1m);
			var orderLine11 = Helper.CreateWhsOrderLine(order, part11, 1m);
			var orderLine12 = Helper.CreateWhsOrderLine(order, part12, 1m);
			var orderLine13 = Helper.CreateWhsOrderLine(order, part13, 1m);
			var orderLine14 = Helper.CreateWhsOrderLine(order, part14, 1m);
			var orderLine15 = Helper.CreateWhsOrderLine(order, part15, 1m);
			var orderLine16 = Helper.CreateWhsOrderLine(order, part16, 1m);
			var orderLine17 = Helper.CreateWhsOrderLine(order, part17, 1m);

			var reservedPickLine1 = orderLine1.ReserveStockIfAbleTo(inventory1);
			var reservedPickLine2 = orderLine2.ReserveStockIfAbleTo(inventory2);
			var reservedPickLine3 = orderLine3.ReserveStockIfAbleTo(inventory3);
			var reservedPickLine4 = orderLine4.ReserveStockIfAbleTo(inventory4);
			var reservedPickLine5 = orderLine5.ReserveStockIfAbleTo(inventory5);
			var reservedPickLine6 = orderLine6.ReserveStockIfAbleTo(inventory6);
			var reservedPickLine7 = orderLine7.ReserveStockIfAbleTo(inventory7);
			var reservedPickLine8 = orderLine8.ReserveStockIfAbleTo(inventory8);
			var reservedPickLine14 = orderLine14.ReserveStockIfAbleTo(inventory14);
			var reservedPickLine15 = orderLine15.ReserveStockIfAbleTo(inventory15);

			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var docWrapper = GetNewDocWrapper(pick);
			AssertNoExceptionThrown("No exception should be thrown when sorting picking lines as part of getting picking lines", () => { var count = docWrapper.PickingLines.Count; });
		}

		#endregion

		#region TestPickingLines_Sorting

		public void TestPickingLines_Sorting()
		{
			var data = new TestDataForInventory(Factory);

			data.Whs1 = Helper.CreateWarehouse("1");
			data.Org1 = Helper.CreateClient("1");
			data.Part1 = Helper.CreateProduct(data.Org1, "SortP1");
			data.Part2 = Helper.CreateProduct(data.Org1, "SortP2");

			var row1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "A", 2, 2, 2);
			var row2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 2, 2);

			data.CreateSimpleInventoryManyLines(data.Whs1, data.Org1, data.Part1, new ZDecimal[] { 20, 20, 20, 20, 20 });

			data.Receive11.Lines[0].WE_WL = row2.Locations[4].PK;
			data.Receive11.Lines[1].WE_WL = row1.Locations[4].PK;
			data.Receive11.Lines[2].WE_WL = row1.Locations[2].PK;
			data.Receive11.Lines[3].WE_WL = row1.Locations[1].PK;
			data.Receive11.Lines[4].WE_WL = row1.Locations[0].PK;

			data.CreateSimpleInventoryManyLines(data.Whs1, data.Org1, data.Part2, new ZDecimal[] { 20, 20, 20, 20, 20 }, "2");

			data.Receive11.Lines[0].WE_WL = row1.Locations[4].PK;
			data.Receive11.Lines[1].WE_WL = row2.Locations[3].PK;
			data.Receive11.Lines[2].WE_WL = row2.Locations[2].PK;
			data.Receive11.Lines[3].WE_WL = row2.Locations[1].PK;
			data.Receive11.Lines[4].WE_WL = row2.Locations[0].PK;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 100m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 100m);
			Factory.Save();

			Pick.Orders.Add(order);
			Pick.AutoAllocateItemsWithMock();

			AssertLocationString("A-1-1-1", DocWrapper.PickingLines[0].LocationString);
			AssertLocationString("A-1-1-2", DocWrapper.PickingLines[1].LocationString);
			AssertLocationString("A-1-2-1", DocWrapper.PickingLines[2].LocationString);
			AssertLocationString("A-2-1-1", DocWrapper.PickingLines[3].LocationString);
			AssertLocationString("A-2-1-1", DocWrapper.PickingLines[4].LocationString);
			AssertLocationString("B-1-1-1", DocWrapper.PickingLines[5].LocationString);
			AssertLocationString("B-1-1-2", DocWrapper.PickingLines[6].LocationString);
			AssertLocationString("B-1-2-1", DocWrapper.PickingLines[7].LocationString);
			AssertLocationString("B-2-1-1", DocWrapper.PickingLines[9].LocationString);
		}

		public void TestPickingLines_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, location2, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 10m, location1, "PLT-1");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 40m);
			var pick = Helper.CreatePickNew(order);

			foreach (var pickLine in pick.GetAllPickLines().ToArray())
			{
				Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			}

			var wrapper = GetNewDocWrapper(pick);
			AssertEquals("Should have 3 different combinations.", 3, wrapper.PickingLines.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "A-1|PLT-1", "A-1|", "A-2|" },
				wrapper.PickingLines.Cast<WarehousePickingSlipLineWrapper>().Select(w => w.LocationString + "|" + w.PalletID));
		}

		void AssertLocationString(ZString expectedString, ZString actualString)
		{
			AssertEquals("DocWrapper LocationString is incorrect", expectedString, actualString);
		}

		#endregion

		#region TestPickLines_IsSplitToMultipleWrapper

		public void TestPickLines_IsSplitToMultipleWrapper()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 2, 1);
			var client = Helper.CreateClient("CLIENT1");
			var product = Helper.CreateProduct(client, "P1");
			product.OP_StockKeepingUnit = "UNT";

			var unit = product.PartUnits.AddNew();
			unit.OF_PackType = "UNT";
			unit.OF_ParentPackType = "BOX";
			unit.OF_QuantityInParent = 15;

			// create stock
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 20m, allocateLocations: true, finalise: true);
			Factory.Save();

			// order and pick stock
			var order = Helper.CreateWhsOrder(client, whs, "O1");
			Helper.CreateWhsOrderLine(order, product, 20m);
			var pick = Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);

			var docWrapper = GetNewDocWrapper(pick);
			AssertEquals("Precondition", true, docWrapper.IsPickByBiggestEnabled);
			AssertEquals(1, docWrapper.PickingLines.Count);
			AssertEquals(true, docWrapper.PickingLines[0] is WarehouseGroupedPickingSlipLineWrapper);

			var wrapperLine1 = (WarehouseGroupedPickingSlipLineWrapper)docWrapper.PickingLines[0];
			AssertEquals("1" + System.Environment.NewLine + "5", wrapperLine1.UnitsGroupedPackQty);
			AssertEquals("BOX" + System.Environment.NewLine + "UNT", wrapperLine1.UnitsGroupedPackType);
			AssertEquals("15" + System.Environment.NewLine + "5", wrapperLine1.UnitsGroupedQty);
			AssertEquals("UNT" + System.Environment.NewLine + "UNT", wrapperLine1.UnitsGroupedStockKeepingUnit);
		}

		#endregion

		#region TestPickLines_IsNotSplitToMultipleWrapperWhenPickByBiggestTypeIsOff

		public void TestPickLines_IsNotSplitToMultipleWrapperWhenPickByBiggestTypeIsOff()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 2, 1);
			var client = Helper.CreateClient("CLIENT1");
			var product = Helper.CreateProduct(client, "P1");
			product.OP_StockKeepingUnit = "UNT";

			var unit = product.PartUnits.AddNew();
			unit.OF_PackType = "UNT";
			unit.OF_ParentPackType = "BOX";
			unit.OF_QuantityInParent = 15;

			// create stock
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 20m, allocateLocations: true, finalise: true);
			Factory.Save();

			// order and pick stock
			var order = Helper.CreateWhsOrder(client, whs, "O1");
			Helper.CreateWhsOrderLine(order, product, 20m);
			var pick = Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);

			WarehouseDataRegistry.Instance.PickByBiggestType.SetValue(Guid.Empty, whs.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, false);
			var docWrapper = GetNewDocWrapper(pick);
			AssertEquals("Precondition", false, docWrapper.IsPickByBiggestEnabled);
			AssertEquals(1, docWrapper.PickingLines.Count);
			AssertEquals(false, docWrapper.PickingLines[0] is WarehouseGroupedPickingSlipLineWrapper);
			AssertEquals(true, docWrapper.PickingLines[0] is WarehousePickingSlipLineWrapper);

			var wrapperLine1 = docWrapper.PickingLines[0];
			AssertEquals("", wrapperLine1.UnitsGroupedPackQty);
			AssertEquals("", wrapperLine1.UnitsGroupedPackType);
			AssertEquals("", wrapperLine1.UnitsGroupedQty);
			AssertEquals("", wrapperLine1.UnitsGroupedStockKeepingUnit);
		}

		#endregion

		#region TestPickLines_SplitLineByOrderPickGroup

		public void TestPickLines_SplitLineByOrderPickGroup_OneOrder()
		{
			TestPickLines_SplitLineByOrderPickGroupCore(true);
		}

		public void TestPickLines_SplitLineByOrderPickGroup_MultiOrder()
		{
			TestPickLines_SplitLineByOrderPickGroupCore(false);
		}

		void TestPickLines_SplitLineByOrderPickGroupCore(bool inOneOrder)
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 2, 1);
			var client = Helper.CreateClient("CLIENT1");
			var product = Helper.CreateProduct(client, "P1");
			product.OP_StockKeepingUnit = "UNT";
			var locations = whs.Rows.Single(r => r.WR_Name == "A").Locations;
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 80m, locations[0], string.Empty);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", product, 120m, locations[1], string.Empty);

			var collection = new PickGroupCollection();
			collection.AddNew().Description = (NoResString)"Group 1";
			collection.AddNew().Description = (NoResString)"Group 2";
			Factory.Save();

			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				WhsPickableDocket[] orders;
				WhsOrderLine orderLine2;
				if (inOneOrder)
				{
					var order = Helper.CreateWhsOrder(client, whs, "O1");
					var orderLine1 = Helper.CreateWhsOrderLine(order, product, 96m);
					orderLine2 = Helper.CreateWhsOrderLine(order, product, 96m);
					orderLine1.WE_PickGroup = 1;
					orderLine2.WE_PickGroup = 2;
					orders = new[] { order };
				}
				else
				{
					var order1 = Helper.CreateWhsOrder(client, whs, "O1");
					var order2 = Helper.CreateWhsOrder(client, whs, "O2");
					var orderLine1 = Helper.CreateWhsOrderLine(order1, product, 96m);
					orderLine2 = Helper.CreateWhsOrderLine(order2, product, 96m);
					orderLine1.WE_PickGroup = 1;
					orderLine2.WE_PickGroup = 2;
					orders = new[] { order1, order2 };
				}
				var pick = Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: orders);

				var docWrapper = GetNewDocWrapper(pick);
				AssertEquals("Should split by Pick group and location.", 3, docWrapper.PickingLines.Count);

				AssertContainsExactElementsInAnyOrder(new[] { "Group 1", "Group 2" }, docWrapper.PickingLines.Select(pl => ((WarehousePickingSlipLineWrapper)pl).PickGroup).Distinct().ToArray());
				AssertEquals(96m * 2, docWrapper.PickingLines.Sum(pl => ((WarehousePickingSlipLineWrapper)pl).Units));

				orderLine2.WE_PickGroup = 1; // same
				var docWrapper2 = GetNewDocWrapper(pick);
				AssertEquals("Should split only by location.", 2, docWrapper2.PickingLines.Count);

				AssertEquals(true, docWrapper2.PickingLines.All(pl => ((WarehousePickingSlipLineWrapper)pl).PickGroup.Equals("Group 1")));

				orderLine2.WE_PickGroup = 0; // no group
				var docWrapper3 = GetNewDocWrapper(pick);
				AssertEquals("Should split by Pick group and location. even some lines are not grouped.", 3, docWrapper.PickingLines.Count);

				AssertContainsExactElementsInAnyOrder(new[] { "None", "Group 1" }, docWrapper.PickingLines.Select(pl => ((WarehousePickingSlipLineWrapper)pl).PickGroup).Distinct().ToArray());
			}
		}

		#endregion

		#region TestPickingLines_RollupPickLineCollectionIntoDocWrapperCollection

		public void TestPickingLines_RollupPickLineCollectionIntoDocWrapperCollection()
		{
			var whs = Helper.CreateWarehouse("WHS", "A", 2, 1);
			var locations = whs.Rows.Single(r => r.WR_Name == "A").Locations;
			var client1 = Helper.CreateClient("CLIENT1");
			var client2 = Helper.CreateClient("CLIENT2");
			Helper.SetClientAllAttributeType(client1, true);
			Helper.SetClientAllAttributeType(client2, true);

			var part1 = Helper.CreateProduct(client1, "P1");
			var part2 = Helper.CreateProduct(client1, "P2");
			Helper.CreateProductClientRelationShip(client2, part1);

			Helper.SetProductAllAttributeUse(client1, part1, true, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(client1, part2, true, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(client2, part1, true, useSerialNumber: false);

			var twoDaysForNow = ZDate.Today.AddDays(2);
			var tomorrow = twoDaysForNow.AddDays(1);

			var receive1 = Helper.CreateWhsReceive(client1, whs, "R1", Notify);
			var inventory_Ethalon = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, locations[0], tomorrow, tomorrow, "PA1", "PA2", "PA3", "");
			var inventory_Ethalon2 = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, locations[0], tomorrow, tomorrow, "PA1", "PA2", "PA3", ""); // just to show that it still roll up lines.
			var inventory_differentProduct = Helper.CreateWhsReceiveInventoryLine(receive1, part2, 1m, locations[0], tomorrow, tomorrow, "PA1", "PA2", "PA3", "");
			var inventory_differentLocation = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, locations[1], tomorrow, tomorrow, "PA1", "PA2", "PA3", "");
			var inventory_differentExpiryDate = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, locations[0], twoDaysForNow, tomorrow, "PA1", "PA2", "PA3", "");
			var inventory_differentPackingDate = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, locations[0], tomorrow, twoDaysForNow, "PA1", "PA2", "PA3", "");
			var inventory_differentPartAttrib1 = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, locations[0], tomorrow, tomorrow, "PA11", "PA2", "PA3", "");
			var inventory_differentPartAttrib2 = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, locations[0], tomorrow, tomorrow, "PA1", "PA22", "PA3", "");
			var inventory_differentPartAttrib3 = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, locations[0], tomorrow, tomorrow, "PA1", "PA2", "PA33", "");
			var inventory_differentPalletID = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, locations[0], tomorrow, tomorrow, "PA1", "PA2", "PA3", "");
			inventory_differentPalletID.WI_PalletID = "PLT-1";
			receive1.FinaliseDocket();
			AssertEquals("Precondition - endure Receive is Finalised", true, receive1.IsFinalised);

			var receive2 = Helper.CreateWhsReceive(client2, whs, "R2", Notify);
			var inventory_differentClient = Helper.CreateWhsReceiveInventoryLine(receive2, part1, 1m, locations[0], tomorrow, tomorrow, "PA1", "PA2", "PA3", "");
			receive2.FinaliseDocket();
			AssertEquals("Precondition - endure Receive is Finalised", true, receive2.IsFinalised);

			var order1 = Helper.CreateWhsOrder(client1, whs, "O1");
			Helper.CreateWhsOrderLine(order1, part1, 9m);
			Helper.CreateWhsOrderLine(order1, part2, 1m);

			var order2 = Helper.CreateWhsOrder(client2, whs, "O2");
			Helper.CreateWhsOrderLine(order2, part1, 1m);
			Factory.Save();

			Pick.PickOrdersWithAllocationMock(new WhsPickableDocket[] { order1, order2 });

			AssertEquals("Only 1 line should be rolled up all 10 others should be separate", 10, DocWrapper.PickingLines.Count);

			AssertDocWhsPickLineExist("Could not match inventory_Ethalon", inventory_Ethalon, 2m, DocWrapper.PickingLines);
			AssertDocWhsPickLineExist("Could not match inventory_Ethalon2", inventory_Ethalon2, 2m, DocWrapper.PickingLines); // should find the same PickLine as for inventory_Ethalon
			AssertDocWhsPickLineExist("Could not match inventory_differentClient", inventory_differentClient, 1m, DocWrapper.PickingLines);
			AssertDocWhsPickLineExist("Could not match inventory_differentProduct", inventory_differentProduct, 1m, DocWrapper.PickingLines);
			AssertDocWhsPickLineExist("Could not match inventory_differentLocation", inventory_differentLocation, 1m, DocWrapper.PickingLines);
			AssertDocWhsPickLineExist("Could not match inventory_differentExpiryDate", inventory_differentExpiryDate, 1m, DocWrapper.PickingLines);
			AssertDocWhsPickLineExist("Could not match inventory_differentPackingDate", inventory_differentPackingDate, 1m, DocWrapper.PickingLines);
			AssertDocWhsPickLineExist("Could not match inventory_differentPartAttrib1", inventory_differentPartAttrib1, 1m, DocWrapper.PickingLines);
			AssertDocWhsPickLineExist("Could not match inventory_differentPartAttrib2", inventory_differentPartAttrib2, 1m, DocWrapper.PickingLines);
			AssertDocWhsPickLineExist("Could not match inventory_differentPartAttrib3", inventory_differentPartAttrib3, 1m, DocWrapper.PickingLines);
			AssertDocWhsPickLineExist("Could not match inventory_differentPalletID", inventory_differentPalletID, 1m, DocWrapper.PickingLines);
		}

		void AssertDocWhsPickLineExist(string message, WhsInventoryView matchedInventory, ZDecimal matchedUnits, WarehousePickingSlipLineWrapperCollection docPickLinesCollection)
		{
			foreach (WarehousePickingSlipLineWrapper docPickLine in docPickLinesCollection)
			{
				if (docPickLine.ClientCode == matchedInventory.Client.OH_Code &&
					docPickLine.ProductCode == matchedInventory.SupplierPart.OP_PartNum &&
					docPickLine.LocationString == matchedInventory.LocationString &&
					docPickLine.PartAttribute1 == matchedInventory.WI_PartAttrib1 &&
					docPickLine.PartAttribute2 == matchedInventory.WI_PartAttrib2 &&
					docPickLine.PartAttribute3 == matchedInventory.WI_PartAttrib3 &&
					docPickLine.ExpiryDate == matchedInventory.WI_ExpiryDate &&
					docPickLine.PackingDate == matchedInventory.WI_PackingDate &&
					docPickLine.PalletID == matchedInventory.WI_PalletID)
				{
					AssertEquals("Pick Line exist, but the number of units is incorrect", matchedUnits, docPickLine.Units);
					return;
				}
			}
			Assert(message, false);
		}

		#endregion

		#region TestPickingLines_RollupPickLineCollectionIntoDocWrapperCollection_AttributesNeutral

		public void TestPickingLines_RollupPickLineCollectionIntoDocWrapperCollection_AttributesNeutral()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			var today = ZDate.Today;
			var yesterday = today.AddDays(-1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, today, "", "", "RED", "");
			inventory1.WI_SerialNumber = "SN:001";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, today, "", "", "RED", "");
			inventory2.WI_SerialNumber = "SN:002";
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, yesterday, "", "", "BLUE", "");
			inventory3.WI_SerialNumber = "SN:003";
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, yesterday, "", "", "BLUE", "");
			inventory4.WI_SerialNumber = "SN:004";
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, yesterday, "", "", "", "");
			inventory5.WI_SerialNumber = "SN:005";
			var inventory6 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, yesterday, "", "", "", "");
			inventory6.WI_SerialNumber = "SN:006";
			var inventory7 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, yesterday, "", "", "", "");
			inventory7.WI_SerialNumber = "SN:007";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - ensure Receive is Finalised", true, receive.IsFinalised);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine_PackingDate = Helper.CreateWhsOrderLine(order, data.Part1, 2m, ZDate.Empty, today, "", "", "", "", "");
			var orderLine_Colour = Helper.CreateWhsOrderLine(order, data.Part1, 2m, ZDate.Empty, ZDate.Empty, "", "", "BLUE", "", "");
			var orderLine_NoAttributes = Helper.CreateWhsOrderLine(order, data.Part1, 2m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var orderLine_SerialNumber = Helper.CreateWhsOrderLine(order, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			orderLine_SerialNumber.WE_SerialNumber = "SN:007";
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(order);
			AssertEquals("Precondition - ensure order was picking", true, order.IsAttachedToPickButNotFinalised);

			var docWhsPick1 = new WarehousePickingSlipWrapper(pick, Factory);
			AssertEquals("Should be created separate Pick Lines since all Serial Number Attributes are different. Should be 2+2+2+1 = 7 lines.", 7, docWhsPick1.PickingLines.Count);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			var docWhsPick2 = new WarehousePickingSlipWrapper(pick, Factory);
			AssertEquals("Pick Lines should not be rolled up by Ordered Attributes since Roll Up is False. Should be 2+2+2+1 = 7 lines.", 7, docWhsPick2.PickingLines.Count);

			data.Part1.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			var docWhsPick3 = new WarehousePickingSlipWrapper(pick, Factory);
			AssertEquals("Pick Lines should be rolled up by Ordered Attributes since Pick Mode is Attributes Neutral and Roll Up is True. Should be 1+1+1+1 = 4 lines.", 4, docWhsPick3.PickingLines.Count);
		}

		#endregion

		#endregion

		#region TestHasMultipleUnits

		protected override void TestHasMultipleStockKeepingUnitsCore()
		{
			AssertEquals("Pre-condition", ZBool.False, DocWrapper.HasMultipleStockKeepingUnits);

			var data = new TestDataSimpleEnvironment(Factory);
			var part1 = data.Part1;
			var part2 = data.Part2;
			part1.OP_StockKeepingUnit = Constants.PkgUnit.Bag;
			part2.OP_StockKeepingUnit = Constants.PkgUnit.Bag;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Pre-condition, received should be finalized", true, receive.IsFinalised);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");

			Helper.CreateWhsOrderLine(order1, part1, 5m);
			Helper.CreateWhsOrderLine(order1, part2, 5m);

			Helper.CreateWhsOrderLine(order2, part1, 5m);
			Helper.CreateWhsOrderLine(order2, part2, 5m);

			var pick1 = Helper.CreatePickNew(order1);
			var wrapper1 = new WarehousePickingSlipWrapper(pick1, Factory);

			AssertEquals(false, wrapper1.HasMultipleStockKeepingUnits);

			part1.OP_StockKeepingUnit = Constants.PkgUnit.Container;
			AssertEquals(true, wrapper1.HasMultipleStockKeepingUnits);

			pick1.Orders.Add(order2);
			part1.OP_StockKeepingUnit = Constants.PkgUnit.Bag;
			AssertEquals(false, wrapper1.HasMultipleStockKeepingUnits);

			part2.OP_StockKeepingUnit = Constants.PkgUnit.Container;
			AssertEquals(true, wrapper1.HasMultipleStockKeepingUnits);
		}

		protected override void TestHasMultipleStockKeepingUnitsCore_SomeSupplierPartIsNull()
		{
			AssertEquals("Pre-condition", ZBool.False, DocWrapper.HasMultipleStockKeepingUnits);

			var data = new TestDataSimpleEnvironment(Factory);
			var part1 = data.Part1;
			var part2 = data.Part2;
			part1.OP_StockKeepingUnit = Constants.PkgUnit.Bag;
			part2.OP_StockKeepingUnit = Constants.PkgUnit.Bag;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "qwert", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive1, part2, 10m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertEquals("Pre-condition, received was not finalized", true, receive1.IsFinalised);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order1");

			var orderLine1 = Helper.CreateWhsOrderLine(order1, part1, 5m);
			Helper.CreateWhsOrderLine(order1, part2, 5m);

			var pick1 = Helper.CreatePickNew(order1);
			var wrapper1 = new WarehousePickingSlipWrapper(pick1, Factory);

			AssertEquals(false, wrapper1.HasMultipleStockKeepingUnits);

			part1.OP_StockKeepingUnit = Constants.PkgUnit.Container;
			AssertEquals(true, wrapper1.HasMultipleStockKeepingUnits);

			orderLine1.WE_OP = ZGuid.Empty;
			AssertEquals(false, wrapper1.HasMultipleStockKeepingUnits);
		}

		public void TestHasMultipleStockKeepingUnitsShouldIgnoreCase()
		{
			var whs = Helper.CreateWarehouse("warehouse");
			var client = Helper.CreateClient("Client");
			var product1 = Helper.CreateProduct(client, "Product 1");
			var product2 = Helper.CreateProduct(client, "Product 2");

			var order = Helper.CreateWhsOrder(client, whs, "O1");
			Helper.CreateWhsOrderLine(order, product1, 1m);
			Helper.CreateWhsOrderLine(order, product2, 2m);

			var pick = Helper.CreatePickNew(order);
			var wrapper = new WarehousePickingSlipWrapper(pick, Factory);

			product1.OP_StockKeepingUnit = "bag";
			product2.OP_StockKeepingUnit = "cnt";
			AssertEquals(true, wrapper.HasMultipleStockKeepingUnits);

			product1.OP_StockKeepingUnit = "BAG";
			product2.OP_StockKeepingUnit = "bag";
			AssertEquals(false, wrapper.HasMultipleStockKeepingUnits);
		}

		protected override void TestHasMultipleWeightUnitsCore()
		{
			base.TestHasMultipleWeightUnitsCore();
			AssertEquals("Pre-condition", ZBool.False, DocWrapper.HasMultipleWeightUnits);
			var data = new TestDataSimpleEnvironment(Factory);
			var part1 = data.Part1;
			var part2 = data.Part2;
			part1.OP_WeightUQ = Constants.Weight.Kilograms;
			part2.OP_WeightUQ = Constants.Weight.Kilograms;

			//get inventory
			WhsReceive receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "qwert", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, part2, 10m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertEquals("Pre-condition, received was not finalized", true, receive1.IsFinalised);

			WhsOrder order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order1");
			WhsOrder order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order2");

			WhsOrderLine orderLine1 = Helper.CreateWhsOrderLine(order1, part1, 5m);
			WhsOrderLine orderLine2 = Helper.CreateWhsOrderLine(order1, part2, 5m);

			WhsOrderLine orderLine3 = Helper.CreateWhsOrderLine(order2, part1, 5m);
			WhsOrderLine orderLine4 = Helper.CreateWhsOrderLine(order2, part2, 5m);

			WhsPick pick1 = Helper.CreatePickNew(order1);
			WarehousePickingSlipWrapper wrapper1 = new WarehousePickingSlipWrapper(pick1, Factory);

			//1 order no multiples
			AssertEquals(false, wrapper1.HasMultipleWeightUnits);
			//1 order with multiples
			part1.OP_WeightUQ = Constants.Weight.Pounds;
			AssertEquals(true, wrapper1.HasMultipleWeightUnits);
			//2 orders no multiples
			pick1.Orders.Add(order2);
			part1.OP_WeightUQ = Constants.Weight.Kilograms;
			AssertEquals(false, wrapper1.HasMultipleWeightUnits);
			//2 orders with multiples
			part1.OP_WeightUQ = Constants.Weight.Pounds;
			AssertEquals(true, wrapper1.HasMultipleWeightUnits);
		}

		protected override void TestHasMultipleWeightUnitsCore_SomeSupplierPartIsNull()
		{
			base.TestHasMultipleWeightUnitsCore();
			AssertEquals("Pre-condition", ZBool.False, DocWrapper.HasMultipleWeightUnits);
			var data = new TestDataSimpleEnvironment(Factory);
			var part1 = data.Part1;
			var part2 = data.Part2;
			part1.OP_WeightUQ = Constants.Weight.Kilograms;
			part2.OP_WeightUQ = Constants.Weight.Kilograms;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "qwert", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, part2, 10m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertEquals("Pre-condition, received was not finalized", true, receive1.IsFinalised);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order1");

			var orderLine1 = Helper.CreateWhsOrderLine(order1, part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, part2, 5m);

			var pick1 = Helper.CreatePickNew(order1);
			var wrapper1 = new WarehousePickingSlipWrapper(pick1, Factory);

			//1 order no multiples
			AssertEquals(false, wrapper1.HasMultipleWeightUnits);
			//1 order with multiples
			part1.OP_WeightUQ = Constants.Weight.Pounds;
			AssertEquals(true, wrapper1.HasMultipleWeightUnits);
			//2 orders no multiples
			orderLine1.WE_OP = ZGuid.Empty;
			Assert(!wrapper1.HasMultipleWeightUnits);
		}

		public void TestHasMultipleWeightUnitsShouldIgnoreCase()
		{
			var data = new TestDataForInventory(Factory);
			var whs = Helper.CreateWarehouse("warehouse");
			var client = Helper.CreateClient("Client");
			var product1 = Helper.CreateProduct(client, "Product 1");
			var product2 = Helper.CreateProduct(client, "Product 2");

			var order = Helper.CreateWhsOrder(client, whs, "O1");
			Helper.CreateWhsOrderLine(order, product1, 1m);
			Helper.CreateWhsOrderLine(order, product2, 2m);

			var pick = Helper.CreatePickNew(order);
			var wrapper = new WarehousePickingSlipWrapper(pick, Factory);

			product1.OP_WeightUQ = "gr";
			product2.OP_WeightUQ = "kg";
			AssertEquals(true, wrapper.HasMultipleWeightUnits);

			product1.OP_WeightUQ = "KG";
			product2.OP_WeightUQ = "kg";
			AssertEquals(false, wrapper.HasMultipleWeightUnits);
		}

		public void TestHasMultipleVolumeUnitsShouldIgnoreCase()
		{
			var data = new TestDataForInventory(Factory);
			var whs = Helper.CreateWarehouse("warehouse");
			var client = Helper.CreateClient("Client");
			var product1 = Helper.CreateProduct(client, "Product 1");
			var product2 = Helper.CreateProduct(client, "Product 2");

			var order = Helper.CreateWhsOrder(client, whs, "O1");
			Helper.CreateWhsOrderLine(order, product1, 1m);
			Helper.CreateWhsOrderLine(order, product2, 2m);

			var pick = Helper.CreatePickNew(order);
			var wrapper = new WarehousePickingSlipWrapper(pick, Factory);

			AssertEquals(false, wrapper.HasMultipleVolumeUnits);
			product1.OP_CubicUQ = "ml";
			product2.OP_CubicUQ = "m3";
			AssertEquals(true, wrapper.HasMultipleVolumeUnits);

			product1.OP_CubicUQ = "Ml";
			product2.OP_CubicUQ = "ml";
			AssertEquals(false, wrapper.HasMultipleVolumeUnits);
		}

		protected override void TestHasMultipleVolumeUnitsCore()
		{
			base.TestHasMultipleVolumeUnitsCore();
			AssertEquals("Pre-condition", ZBool.False, DocWrapper.HasMultipleVolumeUnits);
			var data = new TestDataSimpleEnvironment(Factory);
			var part1 = data.Part1;
			var part2 = data.Part2;
			part1.OP_CubicUQ = Constants.Volume.CubicMetres;
			part2.OP_CubicUQ = Constants.Volume.CubicMetres;

			//get inventory
			WhsReceive receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "qwert", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, part2, 10m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertEquals("Pre-condition, received was not finalized", true, receive1.IsFinalised);

			WhsOrder order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order1");
			WhsOrder order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order2");

			WhsOrderLine orderLine1 = Helper.CreateWhsOrderLine(order1, part1, 5m);
			WhsOrderLine orderLine2 = Helper.CreateWhsOrderLine(order1, part2, 5m);

			WhsOrderLine orderLine3 = Helper.CreateWhsOrderLine(order2, part1, 5m);
			WhsOrderLine orderLine4 = Helper.CreateWhsOrderLine(order2, part2, 5m);

			WhsPick pick1 = Helper.CreatePickNew(order1);
			WarehousePickingSlipWrapper wrapper1 = new WarehousePickingSlipWrapper(pick1, Factory);

			//1 order no multiples
			AssertEquals(false, wrapper1.HasMultipleVolumeUnits);
			//1 order with multiples
			part1.OP_CubicUQ = Constants.Volume.CubicYards;
			AssertEquals(true, wrapper1.HasMultipleVolumeUnits);
			//2 orders no multiples
			pick1.Orders.Add(order2);
			part1.OP_CubicUQ = Constants.Volume.CubicMetres;
			AssertEquals(false, wrapper1.HasMultipleVolumeUnits);
			//2 orders with multiples
			part1.OP_CubicUQ = Constants.Volume.CubicYards;
			AssertEquals(true, wrapper1.HasMultipleVolumeUnits);
		}

		#endregion

		#endregion

		#region Properties

		protected override void TestJobNumberHeadingCore()
		{
			AssertEquals("Pick No", DocWrapper.JobNumberHeading);
		}

		protected override void TestSecondaryReferenceCore()
		{
			WhsOrder order = Factory.New<WhsOrder>();
			order.Lines.AddNew();
			order.WD_WP = Pick.PK;
			Pick.ClearOrdersCache();
			Pick.Orders[0].WD_ExternalReference = "A12345";
			AssertEquals("A12345", DocWrapper.SecondaryReference.Value);
			AssertEquals("Order No", DocWrapper.SecondaryReference.Label);

			MakePickMOP(Pick);
			AssertEquals("", DocWrapper.SecondaryReference.Value);
			AssertEquals("", DocWrapper.SecondaryReference.Label);
		}

		protected override void TestSecondaryHeadingCore()
		{
			AssertEquals("Order Details", DocWrapper.SecondaryHeading);
		}

		public void TestBOMStagingAreaParts_DynamicWorkOrder()
		{
			var pick = Factory.New<WhsPick>();
			pick.WP_PickType = PickType.Codes.DynamicWorkOrder;
			var dynamicWorkOrder = Factory.New<WhsDynamicWorkOrder>();
			dynamicWorkOrder.WD_WP = pick.PK;

			var wrapper = new WarehousePickingSlipWrapper(pick, Factory);
			AssertEquals(0, wrapper.BOMStagingAreaParts.Count);
		}

		protected override void TestSOPConsigneeAddressCore()
		{
			AssertEquals("Precondition: Pick has no orders", 0, Pick.Orders.Count);
			AssertNull("DocWrapper SOPConsigneeAddress should be null", DocWrapper.SOPConsigneeAddress);

			WhsOrder order = Factory.New<WhsOrder>();
			order.WD_WP = Pick.PK;
			order.Lines.AddNew();
			Pick.ClearOrdersCache();

			OrgAddress address = Factory.New<OrgAddress>();
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "AU";
			consignee.OH_FullName = "My Organisation Name";
			address.OA_Address1 = "My Address 1";
			address.OA_Address2 = "My Address 2";
			address.OA_City = "My City";
			address.OA_PostCode = "My Code";
			address.OA_State = "My State";
			address.OA_RL_NKRelatedPortCode = "US";

			address.OA_OH = consignee.PK;
			order.ConsigneePK = consignee.PK;
			order.ConsigneeAddressPK = address.PK;

			AssertEquals("SOPConsigneeAddress is of type DocDocAddress", typeof(DocDocAddress), DocWrapper.SOPConsigneeAddress.GetType());

			DocDocAddress orderDocAddress = DocDocAddress.New(order.ConsigneeDocAddress, Factory);
			AssertEqualsDocDocAddress(orderDocAddress, DocWrapper.SOPConsigneeAddress);
			MakePickMOP(Pick);
			AssertNull("DocWrapper SOPConsigneeAddress should be null", DocWrapper.SOPConsigneeAddress);

			MakePickSOP(Pick);
			AssertEqualsDocDocAddress(orderDocAddress, DocWrapper.SOPConsigneeAddress);
		}

		void AssertEqualsDocDocAddress(DocDocAddress docAddress1, DocDocAddress docAddress2)
		{
			AssertEquals(docAddress1.CompanyName, docAddress2.CompanyName);
			AssertEquals(docAddress1.Address1, docAddress2.Address1);
			AssertEquals(docAddress1.Address2, docAddress2.Address2);
			AssertEquals(docAddress1.City, docAddress2.City);
			AssertEquals(docAddress1.PostCode, docAddress2.PostCode);
			AssertEquals(docAddress1.State, docAddress2.State);
			AssertEquals(docAddress1.Country.Code, docAddress2.Country.Code);
		}

		protected override void TestSOPConsigneeAddressLabelCore()
		{
			MakePickSOP(Pick);
			AssertEquals("DocWrapper SOPConsigneeNameLabel is incorrect", "Consignee", DocWrapper.SOPConsigneeAddressLabel);

			MakePickMOP(Pick);
			AssertEquals("DocWrapper SOPConsigneeNameLabel should be empty string", "", DocWrapper.SOPConsigneeAddressLabel);
		}

		protected override void TestSOPRequiredDateCore()
		{
			var testDate = ZDateTimeOffset.Today.AddHours(1).AddMinutes(2).AddSeconds(3);
			var order = Factory.New<WhsOrder>();
			order.WD_WP = Pick.PK;
			order.Lines.AddNew();
			Pick.ClearOrdersCache();

			var sopOrder = Pick.Orders[0];
			sopOrder.WD_RequiredDate = testDate; // setting once, sets WD_RequiredDate to end of day
			sopOrder.WD_RequiredDate = testDate; // setting twice, sets the time (does not set end of day)
			AssertEquals("WD_RequiredDate should have time set.", testDate, sopOrder.WD_RequiredDate);
			AssertEquals("DocWrapper SOPRequiredDate.Value is incorrect", testDate.ToZDateTime().ToLongTimeString(), DocWrapper.SOPRequiredDate.Value);
			AssertEquals("DocWrapper SOPRequiredDate.ValueAsDate is incorrect", testDate.ToZDateTime(), DocWrapper.SOPRequiredDate.ValueAsDate);
			AssertEquals("DocWrapper SOPRequiredDateLabel is incorrect", new ZString("Order Required Date"), DocWrapper.SOPRequiredDate.Label);

			MakePickMOP(Pick);
			AssertEquals("DocWrapper SOPRequiredDate should be empty string", ZString.Empty, DocWrapper.SOPRequiredDate.Value);
			AssertEquals("DocWrapper SOPRequiredDateLabel should be empty string", ZString.Empty, DocWrapper.SOPRequiredDate.Label);
		}

		protected override void TestSOPSpecialInstructionsCore()
		{
			MakePickSOP(Pick);
			Pick.Orders[0].WD_HandlingInstructions = "Hello";
			AssertEquals("DocWrapper SOPSpecialInstructions is incorrect", new ZString("Hello"), DocWrapper.SOPSpecialInstructions.Value);
			AssertEquals("DocWrapper SOPSpecialInstructionsLabel is incorrect", new ZString("Special Inst."), DocWrapper.SOPSpecialInstructions.Label);

			MakePickMOP(Pick);
			AssertEquals("DocWrapper SOPSpecialInstructions should be empty string", ZString.Empty, DocWrapper.SOPSpecialInstructions.Value);
			AssertEquals("DocWrapper SOPSpecialInstructionsLabel should be empty string", new ZString("Special Inst."), DocWrapper.SOPSpecialInstructions.Label);
		}

		protected override void TestSOPOrderNumberCore()
		{
			WhsOrder order = Factory.New<WhsOrder>();
			order.Lines.AddNew();
			order.WD_WP = Pick.PK;
			Pick.ClearOrdersCache();
			Pick.Orders[0].WD_ExternalReference = "TEST";
			AssertEquals("DocWrapper SOPOrderNum is incorrect", new ZString("TEST"), DocWrapper.SOPOrderNumber.Value);
			AssertEquals("DocWrapper SOPOrderNumLabel is incorrect", new ZString("Order No"), DocWrapper.SOPOrderNumber.Label);

			MakePickMOP(Pick);
			AssertEquals("DocWrapper SOPOrderNum should be empty string", ZString.Empty, DocWrapper.SOPOrderNumber.Value);
			AssertEquals("DocWrapper SOPOrderNumLabel should be empty string", ZString.Empty, DocWrapper.SOPOrderNumber.Label);
		}

		public void TestSOPOrderNumberCore_WhsWorkOrder()
		{
			var workOrder = Factory.New<WhsWorkOrder>();
			workOrder.Lines.AddNew();
			workOrder.WD_WP = Pick.PK;
			Pick.WP_PickType = PickType.Codes.WorkOrder;
			Pick.ClearOrdersCache();

			Pick.Orders[0].WD_ExternalReference = "TEST";
			AssertEquals("DocWrapper SOPOrderNum is incorrect", new ZString("TEST"), DocWrapper.SOPOrderNumber.Value);
			AssertEquals("DocWrapper SOPOrderNumLabel is incorrect", new ZString("Work Order No"), DocWrapper.SOPOrderNumber.Label);
		}

		#region TestSOPStagingAreaName

		public void TestSOPStagingAreaName()
		{
			var ddlLocationType = Helper.CreateLocationType("123", LocationClasses.Codes.DDL);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			locationA1.WLV_WLT_LocationType = ddlLocationType.PK;
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			var pickWrapper = GetNewDocWrapper(pick);
			AssertEquals("Precondition - pick without DDL should return nothing.", "", pickWrapper.SOPStagingAreaName.Label);
			AssertEquals("Precondition - pick without DDL should return nothing.", "", pickWrapper.SOPStagingAreaName.Value);

			pick.WP_WL_DockDoor = locationA1.PK;
			AssertEquals("When pick have a Dock Door Location should return label.", "Dock Door Location", pickWrapper.SOPStagingAreaName.Label);
			AssertEquals("When pick have a Dock Door Location should return label.", "A-1", pickWrapper.SOPStagingAreaName.Value);
		}

		#endregion

		protected override void TestWarehouseNameCore()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("TEST");

			AssertNull("Precondition: Pick Warehouse should not be set", Pick.Warehouse);

			Pick.WP_WW_Whs = whs.PK;
			AssertEquals("Pick WarehouseName is incorrect", "TEST", Pick.Warehouse.WW_WarehouseName);
			AssertEquals("Document Wrapper WarehouseName is incorrect", new ZString("TEST"), DocWrapper.WarehouseName.Value);
			AssertEquals("Document Wrapper WarehouseName Label is incorrect", new ZString("Warehouse"), DocWrapper.WarehouseName.Label);
		}

		public void TestWarehouseName_TranslatableCore()
		{
			var warehouse = Helper.CreateWarehouse("TEST");
			Pick.WP_WW_Whs = warehouse.PK;

			AssertEquals("Pick WarehouseName in English", "TEST", Pick.Warehouse.WW_WarehouseName);
			AssertEquals("Document Wrapper WarehouseName in English", new ZString("TEST"), DocWrapper.WarehouseName.Value);
			AssertEquals("Document Wrapper WarehouseName Label is incorrect", new ZString("Warehouse"), DocWrapper.WarehouseName.Label);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "TEST").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试"));
				AssertEquals("Document Wrapper WarehouseName in Chinese", "测试", DocWrapper.WarehouseName.Value);
			}
		}

		protected override void TestPickNoCore()
		{
			AssertEquals("Precondition: PickNo should be empty", ZString.Empty, DocWrapper.PickNo.Value);
			AssertEquals("Precondition: PickNo Label should be empty", new ZString("Pick No"), DocWrapper.PickNo.Label);

			Pick.WP_PickNo = "P00002222";
			AssertEquals("Document Wrapper PickNo is incorrect", "P00002222", DocWrapper.PickNo.Value);
			AssertEquals("Document Wrapper PickNo Label is incorrect", new ZString("Pick No"), DocWrapper.PickNo.Label);
		}

		protected override void TestPrimaryBarcodeTextCore()
		{
			AssertEquals("Precondition: BarcodeTextForPickNo should be empty", ZString.Empty, DocWrapper.PrimaryBarcodeText);
			Pick.WP_PickNo = "P00002222";
			TextBarcode barcode = new TextBarcode(DocWrapper.PickNo.Value);
			AssertEquals("Document Wrapper BarcodeTextForPickNo is incorrect", barcode.TextAs128sFontString, DocWrapper.PrimaryBarcodeText);
		}

		protected override void TestPickingInstructionsCore()
		{
			StmNote pickingInstructionNote = Pick.Notes.AddNew();
			pickingInstructionNote.ST_Description = PredefinedNoteTypes.Instance.PickingInstructions.Description;
			pickingInstructionNote.ST_NoteText = "Pick note";
			AssertEquals("Document Wrapper PickingInstruction is incorrect", "Pick note", DocWrapper.PickingInstructions.Value);
			AssertEquals("Document Wrapper PickingInstruction Label is incorrect", "Picking Inst.", DocWrapper.PickingInstructions.Label);
		}

		protected override void TestSOPTransportCompanyCore()
		{
			OrgHeader transportCo = Factory.New<OrgHeader>();
			transportCo.OH_FullName = "Transport Co";

			MakePickMOP(Pick);
			AssertEquals("", DocWrapper.SOPTransportCompany.Value);
			AssertEquals("DocWrapper SOPTransportCompanyLabel should be empty string", ZString.Empty, DocWrapper.SOPTransportCompany.Label);

			Pick.Orders[0].TransportCoPK = transportCo.PK;
			AssertEquals("", DocWrapper.SOPTransportCompany.Value);
			AssertEquals("DocWrapper SOPTransportCompanyLabel should be empty string", ZString.Empty, DocWrapper.SOPTransportCompany.Label);

			Pick.Orders[0].TransportCoDocAddress.E2_AddressOverride = true;
			Pick.Orders[0].TransportCoDocAddress.E2_CompanyName = "Overridden Transport Co";
			AssertEquals("", DocWrapper.SOPTransportCompany.Value);
			AssertEquals("DocWrapper SOPTransportCompanyLabel should be empty string", ZString.Empty, DocWrapper.SOPTransportCompany.Label);

			Pick.Orders.RemoveAll(); // need to refresh TransportCoDocAddress on Orders
			MakePickSOP(Pick);
			AssertEquals("", DocWrapper.SOPTransportCompany.Value);
			AssertEquals("DocWrapper SOPTransportCompanyLabel is incorrect", new ZString("Transport Company"), DocWrapper.SOPTransportCompany.Label);

			Pick.Orders[0].TransportCoPK = transportCo.PK;
			AssertEquals("Transport Co", DocWrapper.SOPTransportCompany.Value);
			AssertEquals("DocWrapper SOPTransportCompanyLabel is incorrect", new ZString("Transport Company"), DocWrapper.SOPTransportCompany.Label);

			Pick.Orders[0].TransportCoDocAddress.E2_AddressOverride = true;
			Pick.Orders[0].TransportCoDocAddress.E2_CompanyName = "Overridden Transport Co";
			AssertEquals("Overridden Transport Co", DocWrapper.SOPTransportCompany.Value);
			AssertEquals("DocWrapper SOPTransportCompanyLabel is incorrect", new ZString("Transport Company"), DocWrapper.SOPTransportCompany.Label);
		}

		protected override void TestSOPCarrierServiceLevelCore()
		{
			var transportCo = Factory.New<OrgHeader>();
			OrgCarrierServiceLevel serviceLevel = transportCo.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "AAA";
			serviceLevel.PL_CarrierServiceLevelDescription = "Test Service";

			MakePickMOP(Pick);
			AssertEquals("", DocWrapper.SOPCarrierServiceLevel.Value);
			AssertEquals("DocWrapper SOPCarrierServiceLevelLabel should be empty string", ZString.Empty, DocWrapper.SOPCarrierServiceLevel.Label);

			MakePickSOP(Pick);
			AssertEquals("", DocWrapper.SOPCarrierServiceLevel.Value);
			AssertEquals("DocWrapper SOPCarrierServiceLevelLabel should be empty string", new ZString("Carrier Service Level"), DocWrapper.SOPCarrierServiceLevel.Label);

			Pick.Orders[0].TransportCoPK = transportCo.PK;
			Pick.Orders[0].WD_PL_NKCarrierServiceLevel = "AAA";
			AssertEquals("Test Service", DocWrapper.SOPCarrierServiceLevel.Value);
			AssertEquals("DocWrapper SOPCarrierServiceLevelLabel is incorrect", new ZString("Carrier Service Level"), DocWrapper.SOPCarrierServiceLevel.Label);

			MakePickMOP(Pick);
			AssertEquals("", DocWrapper.SOPCarrierServiceLevel.Value);
			AssertEquals("For a Multi-Order Pick, no Carrier Service Level should be displayed as they can differ across Orders.", "", DocWrapper.SOPCarrierServiceLevel.Label);
		}

		void MakePickSOP(WhsPick pick)
		{
			if (pick.Orders.Count == 0)
			{
				AddOrderToPick(pick);
			}
			else
			{
				while (pick.Orders.Count > 1)
				{
					pick.Orders.Remove(pick.Orders[pick.Orders.Count - 1]);
				}
			}
		}

		void MakePickMOP(WhsPick pick)
		{
			while (pick.Orders.Count < 2)
			{
				AddOrderToPick(pick);
			}
		}

		void AddOrderToPick(WhsPick pick)
		{
			WhsOrder order = Factory.New<WhsOrder>();
			order.Lines.AddNew();
			pick.Orders.Add(order);
		}

		void SetupItemToPick(WhsPick pick, WhsOrder order)
		{
			pick.Orders.Add(order);
		}

		public void SetupInventoryWithItemToPick(WhsPick pick, ZDecimal prod1PickedQty, ZDecimal prod1OrderedQty, ZDecimal prod2PickedQty, ZDecimal prod2OrderedQty)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			if (prod1PickedQty != 0)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, prod1PickedQty, "TEST1");
			}
			if (prod2PickedQty != 0)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, prod2PickedQty, "TEST2");
			}

			if (prod1PickedQty != 0 || prod2PickedQty != 0)
			{
				receive.AllocateLocationsWithMock();
				AssertEquals("Precondition: Putaway should be created", true, receive.IsPuttingAway);

				receive.FinaliseDocket();
				AssertEquals("Precondition: Receive should be finalized", true, receive.IsFinalised);
			}

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, prod1OrderedQty, "TEST1", "DummyOutward-1");
			Helper.CreateWhsOrderLine(order, data.Part2, prod2OrderedQty, "TEST2", "DummyOutward-2");
			Factory.Save();

			SetupItemToPick(pick, order);

			foreach (WhsPickOrderedInventory itemToPick in pick.OrderedInventories)
			{
				if (itemToPick.AvailableInventories.Count > 0)
				{
					itemToPick.AvailableInventories[0].PickLineQuantity = itemToPick.AvailableInventories[0].QuantityAvailableToPick;
				}
				if (itemToPick.SupplierPart == data.Part1)
				{
					AssertEquals("Precondition: ItemToPick for Part1 PickLineQuantity not correct", prod1PickedQty, itemToPick.PickLineQuantity);
					AssertEquals("Precondition: ItemToPick for Part1 QuantityOrdered not correct", prod1OrderedQty, itemToPick.QuantityOrdered);
					AssertEquals("Precondition: ItemToPick for Part1 QuantityShort not correct", (prod1PickedQty < prod1OrderedQty ? prod1OrderedQty - prod1PickedQty : 0), itemToPick.QuantityShort);
				}
				else
				{
					AssertEquals("Precondition: ItemToPick for Part2 PickLineQuantity not correct", prod2PickedQty, itemToPick.PickLineQuantity);
					AssertEquals("Precondition: ItemToPick for Part2 QuantityOrdered not correct", prod2OrderedQty, itemToPick.QuantityOrdered);
					AssertEquals("Precondition: ItemToPick for Part2 QuantityShort not correct", (prod2PickedQty < prod2OrderedQty ? prod2OrderedQty - prod2PickedQty : 0), itemToPick.QuantityShort);
				}
			}
		}

		#endregion

		#region TestWarehouseBOWrapper_FactoryCached

		protected override void SetWhsBusinessObjectWarehouse(BusinessObject bizO, ZGuid warehousePK)
		{
			((WhsPick)bizO).WP_WW_Whs = warehousePK;
		}

		protected override BusinessObject GetNewWhsBusinessObjectInSpecifiedFactory(BusinessObjectFactory factory)
		{
			return factory.New<WhsPick>();
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapperInSpecifiedFactory(BusinessObject bizO, BusinessObjectFactory factory)
		{
			return new WarehousePickingSlipWrapper((WhsPick)bizO, factory);
		}

		#endregion

		#region Implementation

		public WhsPick Pick => (WhsPick)WhsBusinessObject;

		TestNotificationBuffer Notify
		{
			get => notify ?? (notify = new TestNotificationBuffer());
			set => notify = value;
		}

		public WarehousePickingSlipWrapper DocWrapper => GetNewDocWrapper(Pick);

		protected virtual WarehousePickingSlipWrapper GetNewDocWrapper(WhsPick pick)
		{
			return new WarehousePickingSlipWrapper(pick, Factory);
		}

		TestNotificationBuffer notify;

		#endregion

		protected override BusinessObject GetNewWhsBusinessObject()
		{
			return Factory.New<WhsPick>();
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WarehousePickingSlipWrapper((WhsPick)bizO, Factory);
		}

		public override void TestWrapperMappingsEmpty()
		{
			WarehousePickingSlipWrapper emptyWrapper = new WarehousePickingSlipWrapper(null, Factory);
			AssertEquals("PickNo", ZString.Empty, emptyWrapper.PickNo.Value);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return DocWrapper;
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
	}
}
