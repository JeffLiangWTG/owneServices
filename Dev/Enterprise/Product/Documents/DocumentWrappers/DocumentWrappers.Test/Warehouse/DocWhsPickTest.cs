using System;
using System.Linq;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsPick))]
	sealed class DocWhsPickTest : DocumentWrapperTestCase
	{
		#region Related Business Objecta

		#region Collections

		#region TestOrders

		public void TestOrders()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("TST WHS");
			var part = Helper.CreateProduct(client, "Prod");
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			var docWrapper = DocWrapper = DocWhsPick.New(pick, Factory);
			AssertEquals("Precondition: Pick shoud have no orders", 0, pick.Orders.Count);
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

			foreach (var order in pick.Orders.Cast<WhsOrder>())
			{
				var orderIsMissing = true;
				foreach (var orderWrapper in docWrapper.Orders.Cast<DocWhsPickableDocket>())
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

		#endregion

		#region TestNonPickedItemsCollection

		public void TestNonPickedItemsCollection()
		{
			AssertEquals("Precondition: WhsPick ItemsToPick Collection should be empty", 0, Pick.OrderedInventories.Count);
			AssertEquals("Precondition: Document Wrapper NonPickedItems Collection should be empty", 0, DocWrapper.NonPickedItems.Count);

			SetupInventoryWithItemToPick(10m, 10m, 10, 20m);
			AssertEquals("Should have no NonPickedItems items", 0, DocWrapper.NonPickedItems.Count);
		}

		public void TestNonPickedItemsCollection_OneUnpickedItem()
		{
			AssertEquals("Precondition: WhsPick ItemsToPick Collection should be empty", 0, Pick.OrderedInventories.Count);
			AssertEquals("Precondition: Document Wrapper NonPickedItems Collection should be empty", 0, DocWrapper.NonPickedItems.Count);

			SetupInventoryWithItemToPick(10m, 10m, 0m, 25m);
			AssertEquals("Should have only one NonPickedItems item", 1, DocWrapper.NonPickedItems.Count);
		}

		public void TestNonPickedItemsCollection_MultipleUnpickedItems()
		{
			AssertEquals("Precondition: WhsPick ItemsToPick Collection should be empty", 0, Pick.OrderedInventories.Count);
			AssertEquals("Precondition: Document Wrapper NonPickedItems Collection should be empty", 0, DocWrapper.NonPickedItems.Count);

			SetupInventoryWithItemToPick(0m, 15m, 0m, 25m);
			AssertEquals("Should have only two NonPickedItems items", 2, DocWrapper.NonPickedItems.Count);
		}

		#endregion

		#region TestShortfallItemsCollection

		public void TestShortfallItemsCollection()
		{
			AssertEquals("Precondition: WhsPick ItemsToPick Collection should be empty", 0, Pick.OrderedInventories.Count);
			AssertEquals("Precondition: Document Wrapper ShortfallItems Collection should be empty", 0, DocWrapper.ShortfallItems.Count);

			SetupInventoryWithItemToPick(10m, 10m, 20m, 20m);
			AssertEquals("Should have no shortfall items", 0, DocWrapper.ShortfallItems.Count);
		}

		public void TestShortfallItemsCollection_OneItemInShortfall()
		{
			AssertEquals("Precondition: WhsPick ItemsToPick Collection should be empty", 0, Pick.OrderedInventories.Count);
			AssertEquals("Precondition: Document Wrapper ShortfallItems Collection should be empty", 0, DocWrapper.ShortfallItems.Count);

			SetupInventoryWithItemToPick(10m, 10m, 20m, 25m);
			AssertEquals("Should have only one shortfall item", 1, DocWrapper.ShortfallItems.Count);
		}

		public void TestShortfallItemsCollection_MultipleItemsInShortfall()
		{
			AssertEquals("Precondition: WhsPick ItemsToPick Collection should be empty", 0, Pick.OrderedInventories.Count);
			AssertEquals("Precondition: Document Wrapper ShortfallItems Collection should be empty", 0, DocWrapper.ShortfallItems.Count);

			SetupInventoryWithItemToPick(10m, 15m, 20m, 25m);
			AssertEquals("Should have only two shortfall items", 2, DocWrapper.ShortfallItems.Count);
		}

		#endregion

		#region TestItemsToPickCollection

		public void TestItemsToPickCollection()
		{
			var pick = Factory.New<WhsPick>();
			var docWrapper = DocWhsPick.New(pick, Factory);
			AssertEquals("Precondition: WhsPick ItemsToPick Collection should be empty", 0, pick.OrderedInventories.Count);
			AssertEquals("Precondition: Document Wrapper ItemsToPick Collection should be empty", 0, docWrapper.ItemsToPick.Count);

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			Helper.CreateWhsOrderLine(order, Helper.CreateProduct(data.Org1, "P3"), 10m);
			pick.Orders.Add(order);
			AssertEquals("WhsPick ItemsToPick Collection should have count of 3", 3, pick.OrderedInventories.Count);
			AssertEquals("Document Wrapper ItemsToPick Collection should have count of 3", 3, docWrapper.ItemsToPick.Count);
		}

		#endregion

		#region TestLines

		#region TestLines

		public void TestLines()
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

			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "W1");
			var orderLine11 = Helper.CreateWhsOrderLine(order1, data.Part1, 50m);
			var orderLine12 = Helper.CreateWhsOrderLine(order1, data.Part2, 80m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "W2");
			var orderLine21 = Helper.CreateWhsOrderLine(order2, data.Part2, 50m);
			var orderLine22 = Helper.CreateWhsOrderLine(order2, data.Part2, 30m);

			Pick.Orders.Add(order1);
			Pick.Orders.Add(order2);
			Pick.AutoAllocateItemsWithMock();

			AssertEquals("Document Wrapper Lines Collection should have other count", 9, DocWrapper.Lines.Count);
			AssertEquals("Document Wrapper Line must have specific Units value", 20m, DocWrapper.Lines[0].Units);
			AssertEquals("Document Wrapper Line must have specific Units value", 10m, DocWrapper.Lines[1].Units);
			AssertEquals("Document Wrapper Line must have specific Units value", 10m, DocWrapper.Lines[2].Units);
			AssertEquals("Document Wrapper Line must have specific Units value", 10m, DocWrapper.Lines[3].Units);
			AssertEquals("Document Wrapper Line must have specific Units value", 50m, DocWrapper.Lines[4].Units);
			AssertEquals("Document Wrapper Line must have specific Units value", 50m, DocWrapper.Lines[5].Units);
			AssertEquals("Document Wrapper Line must have specific Units value", 20m, DocWrapper.Lines[6].Units);
			AssertEquals("Document Wrapper Line must have specific Units value", 20m, DocWrapper.Lines[7].Units);
			AssertEquals("Document Wrapper Line must have specific Units value", 20m, DocWrapper.Lines[8].Units);
		}

		#endregion

		#region TestLines_Sorting

		public void TestLines_Sorting()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataForInventory(Factory);

			data.Whs1 = helper.CreateWarehouse("1");
			data.Org1 = helper.CreateClient("1");
			data.Part1 = helper.CreateProduct(data.Org1, "SortP1");
			data.Part2 = helper.CreateProduct(data.Org1, "SortP2");

			var row1 = helper.CreateRowAndGenerateLocations(data.Whs1, "A", 2, 2, 2);
			var row2 = helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 2, 2);

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

			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 100m);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 100m);

			Pick.Orders.Add(order);
			Pick.AutoAllocateItemsWithMock();

			AssertLocationString("A-1-1-1", DocWrapper.Lines[0].LocationString);
			AssertLocationString("A-1-1-2", DocWrapper.Lines[1].LocationString);
			AssertLocationString("A-1-2-1", DocWrapper.Lines[2].LocationString);
			AssertLocationString("A-2-1-1", DocWrapper.Lines[3].LocationString);
			AssertLocationString("A-2-1-1", DocWrapper.Lines[4].LocationString);
			AssertLocationString("B-1-1-1", DocWrapper.Lines[5].LocationString);
			AssertLocationString("B-1-1-2", DocWrapper.Lines[6].LocationString);
			AssertLocationString("B-1-2-1", DocWrapper.Lines[7].LocationString);
			AssertLocationString("B-2-1-1", DocWrapper.Lines[9].LocationString);
		}

		void AssertLocationString(ZString expectedString, ZString actualString)
		{
			AssertEquals("DocWrapper LocationString is incorrect", expectedString, actualString);
		}

		#endregion

		#region TestLines_RollupPickLineCollectionIntoDocWrapperCollection

		public void TestLines_RollupPickLineCollectionIntoDocWrapperCollection()
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

			var twoDaysFromNow = ZDate.Today.AddDays(2);
			var tomorrow = twoDaysFromNow.AddDays(1);

			var receive1 = Helper.CreateWhsReceive(client1, whs, "R1", Notify);
			var docketLine_Ethalon = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, locations[0], tomorrow, tomorrow, "PA1", "PA2", "PA3", "").InDocketLine;
			var docketLine_Ethalon2 = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, locations[0], tomorrow, tomorrow, "PA1", "PA2", "PA3", "").InDocketLine; // just to show that it still roll up lines.
			var docketLine_differentProduct = Helper.CreateWhsReceiveInventoryLine(receive1, part2, 1m, locations[0], tomorrow, tomorrow, "PA1", "PA2", "PA3", "").InDocketLine;
			var docketLine_differentLocation = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, locations[1], tomorrow, tomorrow, "PA1", "PA2", "PA3", "").InDocketLine;
			var docketLine_differentExpiryDate = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, locations[0], twoDaysFromNow, tomorrow, "PA1", "PA2", "PA3", "").InDocketLine;
			var docketLine_differentPackingDate = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, locations[0], tomorrow, twoDaysFromNow, "PA1", "PA2", "PA3", "").InDocketLine;
			var docketLine_differentPartAttrib1 = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, locations[0], tomorrow, tomorrow, "PA11", "PA2", "PA3", "").InDocketLine;
			var docketLine_differentPartAttrib2 = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, locations[0], tomorrow, tomorrow, "PA1", "PA22", "PA3", "").InDocketLine;
			var docketLine_differentPartAttrib3 = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, locations[0], tomorrow, tomorrow, "PA1", "PA2", "PA33", "").InDocketLine;
			var docketLine_differentPalletID = Helper.CreateWhsReceiveInventoryLine(receive1, part1, 1m, locations[0], tomorrow, tomorrow, "PA1", "PA2", "PA3", "").InDocketLine;
			docketLine_differentPalletID.WE_PalletID = "PLT-1";
			receive1.FinaliseDocket();
			AssertEquals("Precondition - endure Receive is Finalised", true, receive1.IsFinalised);

			var receive2 = Helper.CreateWhsReceive(client2, whs, "R2", Notify);
			var docketLine_differentClient = Helper.CreateWhsReceiveInventoryLine(receive2, part1, 1m, locations[0], tomorrow, tomorrow, "PA1", "PA2", "PA3", "").InDocketLine;
			receive2.FinaliseDocket();
			AssertEquals("Precondition - endure Receive is Finalised", true, receive2.IsFinalised);

			var order1 = Helper.CreateWhsOrder(client1, whs, "O1");
			Helper.CreateWhsOrderLine(order1, part1, 9m);
			Helper.CreateWhsOrderLine(order1, part2, 1m);

			var order2 = Helper.CreateWhsOrder(client2, whs, "O2");
			Helper.CreateWhsOrderLine(order2, part1, 1m);
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(new WhsPickableDocket[] { order1, order2 });

			var docketWrapper = DocWhsPick.New(pick, Factory);
			AssertEquals("Only 1 line should be rolled up all 10 others should be separate", 10, docketWrapper.Lines.Count);
			AssertDocWhsPickLineExist("Could not match inventory_Ethalon", docketLine_Ethalon, 2m, docketWrapper.Lines);
			AssertDocWhsPickLineExist("Could not match inventory_Ethalon2", docketLine_Ethalon2, 2m, docketWrapper.Lines); // should find the same PickLine as for inventory_Ethalon
			AssertDocWhsPickLineExist("Could not match inventory_differentClient", docketLine_differentClient, 1m, docketWrapper.Lines);
			AssertDocWhsPickLineExist("Could not match inventory_differentProduct", docketLine_differentProduct, 1m, docketWrapper.Lines);
			AssertDocWhsPickLineExist("Could not match inventory_differentLocation", docketLine_differentLocation, 1m, docketWrapper.Lines);
			AssertDocWhsPickLineExist("Could not match inventory_differentExpiryDate", docketLine_differentExpiryDate, 1m, docketWrapper.Lines);
			AssertDocWhsPickLineExist("Could not match inventory_differentPackingDate", docketLine_differentPackingDate, 1m, docketWrapper.Lines);
			AssertDocWhsPickLineExist("Could not match inventory_differentPartAttrib1", docketLine_differentPartAttrib1, 1m, docketWrapper.Lines);
			AssertDocWhsPickLineExist("Could not match inventory_differentPartAttrib2", docketLine_differentPartAttrib2, 1m, docketWrapper.Lines);
			AssertDocWhsPickLineExist("Could not match inventory_differentPartAttrib3", docketLine_differentPartAttrib3, 1m, docketWrapper.Lines);
			AssertDocWhsPickLineExist("Could not match inventory_differentPalletID", docketLine_differentPalletID, 1m, docketWrapper.Lines);
		}

		void AssertDocWhsPickLineExist(string message, WhsDocketLine matchedDocketLine, ZDecimal matchedUnits, DocWhsPickLineCollection docPickLinesCollection)
		{
			foreach (DocWhsPickLine docPickLine in docPickLinesCollection)
			{
				if (docPickLine.ClientCode == matchedDocketLine.Docket.Client.OH_Code &&
					docPickLine.ProductCode == matchedDocketLine.SupplierPart.OP_PartNum &&
					docPickLine.LocationString == matchedDocketLine.LocationString &&
					docPickLine.PartAttribute1 == matchedDocketLine.WE_PartAttrib1 &&
					docPickLine.PartAttribute2 == matchedDocketLine.WE_PartAttrib2 &&
					docPickLine.PartAttribute3 == matchedDocketLine.WE_PartAttrib3 &&
					docPickLine.ExpiryDate == matchedDocketLine.WE_ExpiryDate &&
					docPickLine.PackingDate == matchedDocketLine.WE_PackingDate &&
					docPickLine.PalletID == matchedDocketLine.WE_PalletID)
				{
					AssertEquals("Pick Line exist, but the number of units is incorrect", matchedUnits, docPickLine.Units);
					return;
				}
			}
			Assert(message, false);
		}

		#endregion

		#region TestLines_RollupPickLineCollectionIntoDocWrapperCollection_AttributesNeutral

		public void TestLines_RollupPickLineCollectionIntoDocWrapperCollection_AttributesNeutral()
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

			var docWhsPick1 = DocWhsPick.New(pick, Factory);
			AssertEquals("Should be created separate Pick Lines since all Serial Number Attributes are different. Should be 2+2+2+1 = 7 lines", 7, docWhsPick1.Lines.Count);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			var docWhsPick2 = DocWhsPick.New(pick, Factory);
			AssertEquals("Pick Lines shouldn't be rolled up by Ordered Attributes since Pick Mode is Attributes Neutral and Roll Up is false. Should be 2+2+2+1 = 7 lines", 7, docWhsPick2.Lines.Count);

			data.Part1.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			var docWhsPick3 = DocWhsPick.New(pick, Factory);
			AssertEquals("Pick Lines should be rolled up by Ordered Attributes since Pick Mode is Attributes Neutral and Roll Up is true. Should be 1+1+1+1 = 4 lines", 4, docWhsPick3.Lines.Count);
		}

		#endregion

		#endregion

		#endregion

		#region Properties

		public void TestSOPConsigneeAddress()
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

		#endregion

		#endregion

		#region Properties

		#region ZString Fields

		public void TestSOPConsigneeAddressLabel()
		{
			MakePickSOP(Pick);
			AssertEquals("DocWrapper SOPConsigneeNameLabel is incorrect", "Consignee:", DocWrapper.SOPConsigneeAddressLabel);

			MakePickMOP(Pick);
			AssertEquals("DocWrapper SOPConsigneeNameLabel should be empty string", "", DocWrapper.SOPConsigneeAddressLabel);
		}

		public void TestSOPRequiredDate()
		{
			var testDate = new ZDateTimeOffset(DateTime.Now);
			WhsOrder order = Factory.New<WhsOrder>();
			order.WD_WP = Pick.PK;
			order.Lines.AddNew();
			Pick.ClearOrdersCache();
			Pick.Orders[0].WD_RequiredDate = testDate;
			AssertEquals("DocWrapper SOPRequiredDate is incorrect", testDate.ToShortDateString(), DocWrapper.SOPRequiredDate);
			MakePickMOP(Pick);
			AssertEquals("DocWrapper SOPRequiredDate should be empty string", ZString.Empty, DocWrapper.SOPRequiredDate);
		}

		public void TestSOPRequiredDateLabel()
		{
			MakePickSOP(Pick);
			AssertEquals("DocWrapper SOPRequiredDateLabel is incorrect", new ZString("Required Date:"), DocWrapper.SOPRequiredDateLabel);
			MakePickMOP(Pick);
			AssertEquals("DocWrapper SOPRequiredDateLabel should be empty string", ZString.Empty, DocWrapper.SOPRequiredDateLabel);
		}

		public void TestSOPSpecialInstructions()
		{
			MakePickSOP(Pick);
			Pick.Orders[0].WD_HandlingInstructions = "Hello";
			AssertEquals("DocWrapper SOPSpecialInstructions is incorrect", "Hello", DocWrapper.SOPSpecialInstructions);
			MakePickMOP(Pick);
			AssertEquals("DocWrapper SOPSpecialInstructions should be empty string", ZString.Empty, DocWrapper.SOPSpecialInstructions);
		}

		public void TestSOPSpecialInstructionsLabel()
		{
			MakePickSOP(Pick);
			AssertEquals("DocWrapper SOPSpecialInstructionsLabel is incorrect", ZString.Empty, DocWrapper.SOPSpecialInstructionsLabel);
			Pick.Orders[0].WD_HandlingInstructions = "Order Special Instructions";
			AssertEquals("DocWrapper SOPSpecialInstructionsLabel is incorrect", new ZString("Special Inst:"), DocWrapper.SOPSpecialInstructionsLabel);
			MakePickMOP(Pick);
			AssertEquals("DocWrapper SOPSpecialInstructionsLabel should be empty string", ZString.Empty, DocWrapper.SOPSpecialInstructionsLabel);
		}

		public void TestSOPOrderNumLabel()
		{
			MakePickSOP(Pick);
			AssertEquals("DocWrapper SOPOrderNumLabel is incorrect", new ZString("Order No.:"), DocWrapper.SOPOrderNumLabel);
			MakePickMOP(Pick);
			AssertEquals("DocWrapper SOPOrderNumLabel should be empty string", ZString.Empty, DocWrapper.SOPOrderNumLabel);
		}

		public void TestSOPOrderNum()
		{
			WhsOrder order = Factory.New<WhsOrder>();
			order.Lines.AddNew();
			order.WD_WP = Pick.PK;
			Pick.ClearOrdersCache();
			Pick.Orders[0].WD_ExternalReference = "TEST";
			AssertEquals("DocWrapper SOPOrderNum is incorrect", new ZString("TEST"), DocWrapper.SOPOrderNum);
			MakePickMOP(Pick);
			AssertEquals("DocWrapper SOPOrderNum should be empty string", ZString.Empty, DocWrapper.SOPOrderNum);
		}

		#region TestDockDoorLocation

		public void TestDockDoorLocationLabel()
		{
			var ddlLocationType = Helper.CreateLocationType("123", LocationClasses.Codes.DDL);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			locationA1.WLV_WLT_LocationType = ddlLocationType.PK;
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			var pickWrapper = DocWhsPick.New(pick, Factory);
			AssertEquals("Precondition - pick without DDL should return nothing.", "", pickWrapper.DockDoorLocationLabel);

			pick.WP_WL_DockDoor = locationA1.PK;
			AssertEquals("When pick have a Dock Door Location should return label.", "Dock Door Location:", pickWrapper.DockDoorLocationLabel);
		}

		public void TestDockDoorLocation()
		{
			var ddlLocationType = Helper.CreateLocationType("123", LocationClasses.Codes.DDL);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			locationA1.WLV_WLT_LocationType = ddlLocationType.PK;
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			var pickWrapper = DocWhsPick.New(pick, Factory);
			AssertEquals("Precondition - pick without DDL should return nothing.", "", pickWrapper.DockDoorLocation);

			pick.WP_WL_DockDoor = locationA1.PK;
			AssertEquals("When pick have a Dock Door Location, it should be returned.", "A-1", pickWrapper.DockDoorLocation);
		}

		#endregion

		public void TestWarehouseName()
		{
			WhsTestHelperFunctions helper = new WhsTestHelperFunctions(Factory);
			WhsWarehouse whs = helper.CreateWarehouse("TEST");

			AssertNull("Precondition: Pick Warehouse should not be set", Pick.Warehouse);

			Pick.WP_WW_Whs = whs.PK;
			AssertEquals("Pick WarehouseName is incorrect", "TEST", Pick.Warehouse.WW_WarehouseName);
			AssertEquals("Document Wrapper WarehouseName is incorrect", "TEST", DocWrapper.WarehouseName);
		}

		public void TestWarehouseName_Translatable()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var warehouse = helper.CreateWarehouse("TEST");

			Pick.WP_WW_Whs = warehouse.PK;
			AssertEquals("Pick WarehouseName", "TEST", Pick.Warehouse.WW_WarehouseName);
			AssertEquals("Document Wrapper WarehouseName in English", "TEST", DocWrapper.WarehouseName);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "TEST").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试"));
				AssertEquals("WarehouseName in Chinese", "测试", DocWrapper.WarehouseName);
			}
		}

		public void TestPickNo()
		{
			AssertEquals("Precondition: PickNo should be empty", ZString.Empty, DocWrapper.PickNo);
			Pick.WP_PickNo = "P00002222";
			AssertEquals("Document Wrapper PickNo is incorrect", "P00002222", DocWrapper.PickNo);
		}

		public void TestPickMethod()
		{
			AssertEquals("Precondition: Pick Method should be empty", ZString.Empty, DocWrapper.Method);
			Pick.WP_PickOption = "AUT";
			AssertEquals("Pick Method set", "AUTO PICK", DocWrapper.Method);
		}

		public void TestBarcodeTextForPickNo()
		{
			AssertEquals("Precondition: BarcodeTextForPickNo should be empty", ZString.Empty, DocWrapper.BarcodeTextForPickNo);
			Pick.WP_PickNo = "P00002222";
			TextBarcode barcode = new TextBarcode(DocWrapper.PickNo);
			AssertEquals("Document Wrapper BarcodeTextForPickNo is incorrect", barcode.TextAs128sFontString, DocWrapper.BarcodeTextForPickNo);
		}

		public void TestPickingInstructions()
		{
			StmNote pickingInstructionNote = Pick.Notes.AddNew();
			pickingInstructionNote.ST_Description = PredefinedNoteTypes.Instance.PickingInstructions.Description;
			pickingInstructionNote.ST_NoteText = "Pick note";
			AssertEquals("Document Wrapper PickingInstruction is incorrect", "Pick note", DocWrapper.PickingInstructions);
		}

		public void TestSOPTransportCompanyLabel()
		{
			MakePickSOP(Pick);
			AssertEquals("DocWrapper SOPTransportCompanyLabel is incorrect", "Transport Company:", DocWrapper.SOPTransportCompanyLabel);

			MakePickMOP(Pick);
			AssertEquals("DocWrapper SOPTransportCompanyLabel should be empty string", "", DocWrapper.SOPTransportCompanyLabel);
		}

		public void TestSOPTransportCompany()
		{
			OrgHeader transportCo = Factory.New<OrgHeader>();
			transportCo.OH_FullName = "Transport Co";

			MakePickMOP(Pick);
			AssertEquals("", DocWrapper.SOPTransportCompany);

			Pick.Orders[0].TransportCoPK = transportCo.PK;
			AssertEquals("", DocWrapper.SOPTransportCompany);

			Pick.Orders[0].TransportCoDocAddress.E2_AddressOverride = true;
			Pick.Orders[0].TransportCoDocAddress.E2_CompanyName = "Overridden Transport Co";
			AssertEquals("", DocWrapper.SOPTransportCompany);

			Pick.Orders.RemoveAll(); // need to refresh TransportCoDocAddress on Orders
			MakePickSOP(Pick);
			AssertEquals("", DocWrapper.SOPTransportCompany);

			Pick.Orders[0].TransportCoPK = transportCo.PK;
			AssertEquals("Transport Co", DocWrapper.SOPTransportCompany);

			Pick.Orders[0].TransportCoDocAddress.E2_AddressOverride = true;
			Pick.Orders[0].TransportCoDocAddress.E2_CompanyName = "Overridden Transport Co";
			AssertEquals("Overridden Transport Co", DocWrapper.SOPTransportCompany);
		}

		public void TestSOPServiceLevelLabel()
		{
			MakePickSOP(Pick);
			AssertEquals("DocWrapper SOPServiceLevelLabel is incorrect", "Service Level:", DocWrapper.SOPServiceLevelLabel);

			MakePickMOP(Pick);
			AssertEquals("DocWrapper SOPServiceLevelLabel should be empty string", "", DocWrapper.SOPServiceLevelLabel);
		}

		#region TestSOPServiceLevel

		public void TestSOPServiceLevel()
		{
			var transportCo = Factory.New<OrgHeader>();
			OrgCarrierServiceLevel serviceLevel = transportCo.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "AAA";
			serviceLevel.PL_CarrierServiceLevelDescription = "Test Service";

			MakePickMOP(Pick);
			AssertEquals("", DocWrapper.SOPServiceLevel);

			MakePickSOP(Pick);
			AssertEquals("", DocWrapper.SOPServiceLevel);

			Pick.Orders[0].TransportCoPK = transportCo.PK;
			Pick.Orders[0].WD_PL_NKCarrierServiceLevel = "AAA";
			AssertEquals("Test Service", DocWrapper.SOPServiceLevel);

			MakePickMOP(Pick);
			Pick.Orders[0].WD_PL_NKCarrierServiceLevel = "AAA";
			AssertEquals("For a Multi-Order Pick, no Service Level should be displayed as they can differ across Orders.", "", DocWrapper.SOPServiceLevel);
		}

		#endregion

		#endregion

		#region ZDecimal Fields

		public void TestOrdersCount()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("TST WHS");
			var part = Helper.CreateProduct(client, "Prod");
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			var docWrapper = DocWrapper = DocWhsPick.New(pick, Factory);
			AssertEquals("Precondition: Pick shoud have no orders", 0, pick.Orders.Count);
			AssertEquals("DocWrapper OrdersCount is incorrect", ZDecimal.Zero, docWrapper.OrdersCount);

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
			AssertEquals("DocWrapper OrdersCount is incorrect", new ZDecimal(3), docWrapper.OrdersCount);
		}

		public void TestProductLinesCount()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("TST WHS");
			var prod1 = Helper.CreateProduct(client, "P1");
			var prod2 = Helper.CreateProduct(client, "P2");
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			var docWrapper = DocWrapper = DocWhsPick.New(pick, Factory);
			AssertEquals("Precondition: Pick should have no ItemsToPick", 0, pick.OrderedInventories.Count);
			AssertEquals("DocWrapper ProductLinesCount is incorrect", ZDecimal.Zero, DocWrapper.ProductLinesCount);

			var order = Helper.CreateWhsOrder(client, whs);
			Helper.CreateWhsOrderLine(order, prod1, 10m, "TEST1", "DummyOutward-1");
			Helper.CreateWhsOrderLine(order, prod1, 20m, "TEST2", "DummyOutward-2");
			Helper.CreateWhsOrderLine(order, prod2, 30m);

			pick.Orders.Add(order);
			AssertEquals("Precondition: ItemsToPick count is incorrect", 3, pick.OrderedInventories.Count);
			AssertEquals("DocWrapper ProductLinesCount is incorrect", new ZDecimal(2), docWrapper.ProductLinesCount);
		}

		[ExpectNoExceptions]
		public void TestProductLinesCountDoesNotRaiseExceptionWhenNoProductIsSupplied()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("TST WHS");
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, whs);
			Helper.CreateWhsOrderLine(order, ZGuid.Empty, 10m, "TEST1", "DummyOutward-1");

			var pick = Factory.New<WhsPick>();
			var docWrapper = DocWrapper = DocWhsPick.New(pick, Factory);

			pick.Orders.Add(order);
			AssertEquals("DocWrapper ProductLinesCount should be 0", new ZDecimal(0), docWrapper.ProductLinesCount);
		}

		#endregion

		#region TestAutoPrintFields

		public void TestAutoPrintPickingSlip()
		{
			AssertNull("Pre-Condition", Pick.Warehouse);
			AssertEquals(false, DocWrapper.AutoPrintPickingSlip);

			WhsWarehouse whs = Helper.CreateWarehouse("Test");
			Pick.WP_WW_Whs = whs.PK;

			Pick.Warehouse.WW_AutoPrintPickingSlip = false;
			AssertEquals(false, DocWrapper.AutoPrintPickingSlip);

			Pick.Warehouse.WW_AutoPrintPickingSlip = true;
			AssertEquals(true, DocWrapper.AutoPrintPickingSlip);
		}

		public void TestAutoPrintOrderSummary()
		{
			AssertNull("Pre-Condition", Pick.Warehouse);
			AssertEquals(false, DocWrapper.AutoPrintOrderSummary);

			WhsWarehouse whs = Helper.CreateWarehouse("Test");
			Pick.WP_WW_Whs = whs.PK;

			Pick.Warehouse.WW_AutoPrintOrderSummaryOnPick = false;
			AssertEquals(false, DocWrapper.AutoPrintOrderSummary);

			Pick.Warehouse.WW_AutoPrintOrderSummaryOnPick = true;
			AssertEquals(true, DocWrapper.AutoPrintOrderSummary);
		}

		public void TestAutoPrintNonPickedItems()
		{
			AssertNull("Pre-Condition", Pick.Warehouse);
			AssertEquals(false, DocWrapper.AutoPrintNonPickedItems);

			WhsWarehouse whs = Helper.CreateWarehouse("Test");
			Pick.WP_WW_Whs = whs.PK;

			Pick.Warehouse.WW_AutoPrintPickingNonPickedItems = false;
			AssertEquals(false, DocWrapper.AutoPrintNonPickedItems);

			Pick.Warehouse.WW_AutoPrintPickingNonPickedItems = true;
			AssertEquals(true, DocWrapper.AutoPrintNonPickedItems);
		}

		public void TestAutoPrintShortfallItems()
		{
			AssertNull("Pre-Condition", Pick.Warehouse);
			AssertEquals(false, DocWrapper.AutoPrintShortfallItems);

			WhsWarehouse whs = Helper.CreateWarehouse("Test");
			Pick.WP_WW_Whs = whs.PK;

			Pick.Warehouse.WW_AutoPrintPickingShortfallItems = false;
			AssertEquals(false, DocWrapper.AutoPrintShortfallItems);

			Pick.Warehouse.WW_AutoPrintPickingShortfallItems = true;
			AssertEquals(true, DocWrapper.AutoPrintShortfallItems);
		}

		#endregion

		#endregion

		#region Implementation

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

		void SetupItemToPick(WhsOrder order)
		{
			Pick = Factory.New<WhsPick>();
			Pick.Orders.Add(order);

			AssertEquals("Precondition: Incorrect number of ItemsToPick", order.Lines.Count, Pick.OrderedInventories.Count);
			DocWrapper = DocWhsPick.New(Pick, Factory);
			AssertNotNull("Precondition: DocWrapper was not created", DocWrapper);
		}

		void SetupInventoryWithItemToPick(ZDecimal prod1PickedQty, ZDecimal prod1OrderedQty, ZDecimal prod2PickedQty, ZDecimal prod2OrderedQty)
		{
			TestDataSimpleEnvironment data = new TestDataSimpleEnvironment(Factory);
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

			SetupItemToPick(order);

			foreach (WhsPickOrderedInventory itemToPick in Pick.OrderedInventories)
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

		public void TestIssueWhenReallocatingToManualLocationInPick()
		{
			var whs = Helper.CreateWarehouse("1", "A1", 2, 2);
			whs.WW_AutoPrintPickingSlip = true;
			var client = Helper.CreateClient("TEST1", "TEST1");
			var product = Helper.CreateProduct(client, "1");
			Factory.Save();

			var location1 = whs.FindLocation("A1-1-1");
			var location2 = whs.FindLocation("A1-2-2");
			var location3 = whs.FindLocation("A1-2-1");

			var receive = Helper.CreateWhsReceive(client.PK, whs.PK, "111", Notify);

			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, product, 3, location1, string.Empty, InventoryStatus.Codes.Available);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, product, 2, location2, string.Empty, InventoryStatus.Codes.Available);
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive, product, 1, location3, string.Empty, InventoryStatus.Codes.Available);

			receive.FinaliseDocket();

			var order = Helper.CreateWhsOrder(client.PK, whs.PK, Notify);
			order.WD_PickOption = WhsPickOption.Codes.ManualWithAutoAllocate;
			var orderLine = Helper.CreateWhsOrderLine(order, product.PK, 3);
			var reservePickLine = orderLine.ReserveStockIfAbleTo(receiveLine1);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItemsWithMock();

			AssertEquals(1, pick.GetAllPickLines().Count());
			AssertEquals(1, order.Lines.Count);
			AssertEquals(1, pick.OrderedInventories.Count);
			AssertEquals(3, pick.OrderedInventories[0].AvailableInventories.Count);

			AssertEquals(3.0m, pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity);
			AssertEquals(0.0m, pick.OrderedInventories[0].AvailableInventories[1].PickLineQuantity);
			AssertEquals(0.0m, pick.OrderedInventories[0].AvailableInventories[2].PickLineQuantity);

			Assert(pick.OrderedInventories[0].AvailableInventories[0].Allocate);
			Assert(!pick.OrderedInventories[0].AvailableInventories[1].Allocate);
			Assert(!pick.OrderedInventories[0].AvailableInventories[2].Allocate);

			pick.OrderedInventories[0].AvailableInventories[0].Allocate = false;
			pick.OrderedInventories[0].AvailableInventories[1].Allocate = true;

			Assert(!pick.OrderedInventories[0].AvailableInventories[0].Allocate);
			Assert(pick.OrderedInventories[0].AvailableInventories[1].Allocate);
			Assert(!pick.OrderedInventories[0].AvailableInventories[2].Allocate);

			AssertEquals("Pick lines should be attached to orders even after allocation and load", 2, pick.GetAllPickLines().Count());

			Factory.Save();

			AssertEquals(0.0m, pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity);
			AssertEquals(1.0m, pick.OrderedInventories[0].AvailableInventories[1].PickLineQuantity);
			AssertEquals(0.0m, pick.OrderedInventories[0].AvailableInventories[2].PickLineQuantity);

			var docWrapper = new WarehousePickingSlipWrapper(pick, Factory);

			AssertEquals("Only one picking line should appear in the picking slip", 1, docWrapper.PickingLines.Count);
			AssertEquals("The one line in question should not have zero units requested", 1.0m, docWrapper.PickingLines[0].Units);
			AssertEquals("The one line in question should be from Location 3", location3.WLV_LocationString, docWrapper.PickingLines[0].LocationString);

			var docWrapperLegacy = DocWhsPick.New(pick, Factory);

			AssertEquals("Only one picking line should appear in the LEGACY picking slip", 1, docWrapperLegacy.Lines.Count);
			AssertEquals("The one LEGACY line in question should not have zero units requested", 1.0m, docWrapperLegacy.Lines[0].Units);
			AssertEquals("The one LEGACY line in question should be from Location 3", location3.WLV_LocationString, docWrapperLegacy.Lines[0].LocationString);

			Assert(docWrapper.PickingLines is DocumentWrapperCollection);
			Assert(docWrapperLegacy.Lines is DocumentWrapperCollection);
		}

		protected override void SetUp()
		{
			Helper = new WhsTestHelperFunctions(Factory);
			Notify = new TestNotificationBuffer();
			Pick = Factory.New<WhsPick>();
			DocWrapper = DocWhsPick.New(Pick, Factory);
			AssertNotNull("Wrapper not null", DocWrapper);
			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocWrapper };
		}

		WhsPick Pick;
		DocWhsPick DocWrapper;
		WhsTestHelperFunctions Helper;
		TestNotificationBuffer Notify;

		#endregion
	}
}
