using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing
{
	abstract class PackingSlipLineWrapperCollectionHelperTest<TWrapper, TCollection> : WhsTestCaseWithFactory
			where TWrapper : IPackingSlipWrapper
			where TCollection : IPackingSlipWrapperCollection<TWrapper>
	{
		#region TestGetPropertyToSortBy

		public void TestGetPropertyToSortBy()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			AssertEquals("Default Order by should be Product.", "ProductCode", PackingSlipLineWrapperHelper.GetPropertyToSortBy(order));

			data.Org1.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.LineNo;
			AssertEquals("Order by should could from MiscServ.", "LineNo", PackingSlipLineWrapperHelper.GetPropertyToSortBy(order));

			data.Org1.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.ProductCode;
			AssertEquals("Order by should could from MiscServ.", "ProductCode", PackingSlipLineWrapperHelper.GetPropertyToSortBy(order));

			data.Org1.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.ProductDescription;
			AssertEquals("Order by should could from MiscServ.", "ProductDescription", PackingSlipLineWrapperHelper.GetPropertyToSortBy(order));

			data.Org1.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.SameAsOnGrid;
			AssertEquals("Order by is empty when set to SameAsOnGrid.", "", PackingSlipLineWrapperHelper.GetPropertyToSortBy(order));

			using (WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.LineNo))
			{
				data.Org1.MiscServ.OM_WhsPackingSlipOrderBy = "DEF";
				AssertEquals("Order by should could from Registry if MiscServ has is set to Default from Registry.", "LineNo", PackingSlipLineWrapperHelper.GetPropertyToSortBy(order));
			}
		}

		#endregion

		#region TestGetPackingLines

		public void TestGetPackingLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var line1 = order.Lines[0];
			var line2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			line1.ReleaseLines[0].Quantity = 3m;
			line1.ReleaseLines[0].PartAttribute1 = "A";

			line1.ReleaseLines.AddNew().Quantity = 2m;
			line1.ReleaseLines[1].PartAttribute1 = "B";
			Factory.Save();

			var helper = GetNewHelper();
			AssertEquals("Count should be 3 as a temporary attribute is added for the 2nd Orderline which has no attributes.", 3, helper.GetPackingLines(order).Wrappers.Count());
			Assert("Docket should not have any changes after we access PackingLines on the wrapper.", !order.HasChanges);
		}

		#endregion

		#region TestGetPackingLines_WorkOrder

		public void TestGetPackingLines_WorkOrder()
		{
			var bomDataHelper = new TestDataForBOM(Factory);

			bomDataHelper.CreateBOMProducts();

			// Polish for 1 bike, 2 engines, 4 wheels
			bomDataHelper.CreateProductInInventory("7 Polish", bomDataHelper.BOM.Polish, 7m);

			// stock for one bike
			bomDataHelper.CreateProductInInventory("1 Engine", bomDataHelper.BOM.BikeEngine, 1m);
			bomDataHelper.CreateProductInInventory("2 Wheels", bomDataHelper.BOM.BikeWheel, 2m);

			// stock for 2 engines, includes building of 8 pistons
			bomDataHelper.CreateProductInInventory("2 Engine Blocks", bomDataHelper.BOM.EngineBlock, 2m);
			bomDataHelper.CreateProductInInventory("1 Engine Piston", bomDataHelper.BOM.EnginePiston, 1m);
			bomDataHelper.CreateProductInInventory("8 Piston Cranks", bomDataHelper.BOM.PistonCrank, 8m);
			bomDataHelper.CreateProductInInventory("8 Piston Heads", bomDataHelper.BOM.PistonHead, 8m);
			bomDataHelper.CreateProductInInventory("8 Piston Rings", bomDataHelper.BOM.PistonRing, 8m);

			// stock for 4 wheels
			bomDataHelper.CreateProductInInventory("4 Rims", bomDataHelper.BOM.WheelRim, 4m);
			bomDataHelper.CreateProductInInventory("4 Tyres", bomDataHelper.BOM.WheelTyre, 4m);

			Factory.Save();

			// create a work order for 1 bikes to be assembled.
			var bikeWorkOrder = Helper.CreateWhsWorkOrder(bomDataHelper.Org1, bomDataHelper.Whs1);
			bikeWorkOrder.WD_ExternalReference = "abc";
			Helper.CreateWhsWorkOrderLine(bikeWorkOrder, bomDataHelper.BOM.Bike, 1m);

			// pick the order.
			Helper.CreatePickNew(bikeWorkOrder);

			var packingLines = GetNewHelper().GetPackingLines(bikeWorkOrder).Wrappers.ToArray();
			AssertEquals("Ensure packing lines were created.", 14, packingLines.Length);

			for (int index = 0; index < packingLines.Length; index++)
			{
				AssertEquals("LineNo Sorting.", packingLines[index].LineNo, packingLines[index].PositionAfterSorting);
			}
		}

		#endregion

		#region TestGetPackingLines_RollingUp

		public void TestGetPackingLines_RollingUp()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var relation2 = data.Part2.RelatedOrganisations[0];
			relation2.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation2.OU_RollUpAttributesOnDocuments = true;

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var relation3 = part3.RelatedOrganisations[0];
			relation3.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation3.OU_RollUpAttributesOnDocuments = true;

			var part4 = Helper.CreateProduct(data.Org1, "P4");
			var relation4 = part4.RelatedOrganisations[0];
			relation4.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation4.OU_RollUpAttributesOnDocuments = false;

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, part3, true);
			Helper.SetProductAllAttributeUse(data.Org1, part4, true);

			var tomorrow = ZDate.Today.AddDays(1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, part3, 1m, null, tomorrow, tomorrow, "PA1", "PA2", "PA3", "");
			inventoryLine1.WI_SerialNumber = "SN1";
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, part4, 1m, null, tomorrow, tomorrow, "PA1", "PA2", "PA3", "");
			inventoryLine2.WI_SerialNumber = "SN1";

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);
			// hack to get this awful test passing
			inventoryLine1.WI_InDocketLineUnits = 100m;
			inventoryLine2.WI_InDocketLineUnits = 100m;

			// Part1 - Not Attribute Neutral.
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1_1 = Helper.CreateWhsOrderLine(order, data.Part1, 4m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var orderLine1_2 = Helper.CreateWhsOrderLine(order, data.Part1, 4m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			orderLine1_2.WE_SerialNumber = "SN1";
			var orderLine1_3 = Helper.CreateWhsOrderLine(order, data.Part1, 2m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");

			// Part2 - Attribute Neutral and Roll Up.
			var orderLine2_1 = Helper.CreateWhsOrderLine(order, data.Part2, 4m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");
			var orderLine2_2 = Helper.CreateWhsOrderLine(order, data.Part2, 4m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			orderLine2_2.WE_SerialNumber = "SN1";
			var orderLine2_3 = Helper.CreateWhsOrderLine(order, data.Part2, 2m, ZDate.Empty, ZDate.Empty, "", "", "", "", "");

			// Part3 - Attribute Neutral and Roll Up. 
			var orderLine3_1 = Helper.CreateWhsOrderLine(order, part3, 5m, tomorrow, tomorrow, "PA1", "PA2", "PA3", "", "");
			orderLine3_1.WE_SerialNumber = "SN1";
			var orderLine3_2 = Helper.CreateWhsOrderLine(order, part3, 5m, tomorrow, tomorrow, "PA1", "PA2", "PA3", "", "");
			orderLine3_1.WE_SerialNumber = "SN1";

			// Part4 - Attribute Neutral and No Roll Up.
			var orderLine4_1 = Helper.CreateWhsOrderLine(order, part4, 5m, tomorrow, tomorrow, "PA1", "PA2", "PA3", "", "");
			orderLine4_1.WE_SerialNumber = "SN1";
			var orderLine4_2 = Helper.CreateWhsOrderLine(order, part4, 5m, tomorrow, tomorrow, "PA1", "PA2", "PA3", "", "");
			orderLine4_1.WE_SerialNumber = "SN1";
			Factory.Save();

			Helper.CreatePickNew(order);

			// Part1 Attribute Lines - with no Attribute Neutral on a Product and No Roll Up.
			orderLine1_1.ReleaseLines.RemoveAndDeleteAll();
			var attribLine1_1 = CreatePickableDocketLineAttribute(orderLine1_1, 1m, "", "", "", "", ZDateTime.Empty, ZDateTime.Empty);
			var attribLine1_2 = CreatePickableDocketLineAttribute(orderLine1_1, 1m, "", "", "", "", ZDateTime.Empty, ZDateTime.Empty);
			var attribLine1_3 = CreatePickableDocketLineAttribute(orderLine1_1, 1m, "PA1", "", "", "", ZDateTime.Empty, ZDateTime.Empty);
			var attribLine1_4 = CreatePickableDocketLineAttribute(orderLine1_1, 1m, "PA1", "PA2", "", "", ZDateTime.Empty, ZDateTime.Empty);

			orderLine1_2.ReleaseLines.RemoveAndDeleteAll();
			var attribLine1_5 = CreatePickableDocketLineAttribute(orderLine1_2, 1m, "PA1", "PA2", "PA3", "SN1", ZDateTime.Empty, ZDateTime.Empty);
			var attribLine1_6 = CreatePickableDocketLineAttribute(orderLine1_2, 1m, "PA1", "PA2", "PA3", "SN1", tomorrow, ZDateTime.Empty);
			var attribLine1_7 = CreatePickableDocketLineAttribute(orderLine1_2, 1m, "PA1", "PA2", "PA3", "SN1", tomorrow, tomorrow);
			var attribLine1_8 = CreatePickableDocketLineAttribute(orderLine1_2, 1m, "PA1", "PA2", "PA3", "SN1", tomorrow, tomorrow);

			// Part1 Attribute Lines - with no Attribute Neutral on a Product and No Roll Up. Different OrderLine but same Attributes shouldn't roll up.
			orderLine1_3.ReleaseLines.RemoveAndDeleteAll();
			var attribLine1_9 = CreatePickableDocketLineAttribute(orderLine1_3, 2m, "", "", "", "", ZDateTime.Empty, ZDateTime.Empty);

			// Part2 Attribute Lines - with an Attribute Neutral on a Product and Roll Up.
			orderLine2_1.ReleaseLines.RemoveAndDeleteAll();
			var attribLine2_1 = CreatePickableDocketLineAttribute(orderLine2_1, 1m, "", "", "", "", ZDateTime.Empty, ZDateTime.Empty);
			var attribLine2_2 = CreatePickableDocketLineAttribute(orderLine2_1, 1m, "", "", "", "", ZDateTime.Empty, ZDateTime.Empty);
			var attribLine2_3 = CreatePickableDocketLineAttribute(orderLine2_1, 1m, "PA1", "", "", "", ZDateTime.Empty, ZDateTime.Empty);
			var attribLine2_4 = CreatePickableDocketLineAttribute(orderLine2_1, 1m, "PA1", "PA2", "", "SN1", ZDateTime.Empty, ZDateTime.Empty);

			orderLine2_2.ReleaseLines.RemoveAndDeleteAll();
			var attribLine2_5 = CreatePickableDocketLineAttribute(orderLine2_2, 1m, "PA1", "PA2", "PA3", "SN1", ZDateTime.Empty, ZDateTime.Empty);
			var attribLine2_6 = CreatePickableDocketLineAttribute(orderLine2_2, 1m, "PA1", "PA2", "PA3", "SN1", tomorrow, ZDateTime.Empty);
			var attribLine2_7 = CreatePickableDocketLineAttribute(orderLine2_2, 1m, "PA1", "PA2", "PA3", "SN1", tomorrow, tomorrow);
			var attribLine2_8 = CreatePickableDocketLineAttribute(orderLine2_2, 1m, "PA1", "PA2", "PA3", "SN1", tomorrow, tomorrow);

			// Part2 Attribute Lines - with an Attribute Neutral on a Product and Roll Up. Different OrderLine but same Attributes should be rolled up.
			orderLine2_3.ReleaseLines.RemoveAndDeleteAll();
			var attribLine2_9 = CreatePickableDocketLineAttribute(orderLine2_3, 2m, "", "", "", "", ZDateTime.Empty, ZDateTime.Empty);

			// Part3 Attribute Lines - with an Attribute Neutral on a Product and Roll Up.
			orderLine3_1.ReleaseLines.RemoveAndDeleteAll();
			var attribLine3_1 = CreatePickableDocketLineAttribute(orderLine3_1, 5m, "PA1", "PA2", "PA3", "SN1", tomorrow, tomorrow);

			orderLine3_2.ReleaseLines.RemoveAndDeleteAll();
			var attribLine3_2 = CreatePickableDocketLineAttribute(orderLine3_2, 5m, "PA1", "PA2", "PA3", "SN1", tomorrow, tomorrow);

			// Part4 Attribute Lines - with an Attribute Neutral on a Product and No Roll Up.
			orderLine4_1.ReleaseLines.RemoveAndDeleteAll();
			var attribLine4_1 = CreatePickableDocketLineAttribute(orderLine4_1, 5m, "PA1", "PA2", "PA3", "SN1", tomorrow, tomorrow);

			orderLine4_2.ReleaseLines.RemoveAndDeleteAll();
			var attribLine4_2 = CreatePickableDocketLineAttribute(orderLine4_2, 5m, "PA1", "PA2", "PA3", "SN1", tomorrow, tomorrow);

			var packingLines = GetNewHelper().GetPackingLines(order).Wrappers.ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("22 Original attribute lines should be rolled up into 9 + 2 + 2 + 2 = 15 attribute lines.", 15, packingLines.Length);
				AssertDocWhsPackingSlipLineExist(packingLines, 1m, attribLine1_1);
				AssertDocWhsPackingSlipLineExist(packingLines, 1m, attribLine1_2);
				AssertDocWhsPackingSlipLineExist(packingLines, 1m, attribLine1_3);
				AssertDocWhsPackingSlipLineExist(packingLines, 1m, attribLine1_4);
				AssertDocWhsPackingSlipLineExist(packingLines, 1m, attribLine1_5);
				AssertDocWhsPackingSlipLineExist(packingLines, 1m, attribLine1_6);
				AssertDocWhsPackingSlipLineExist(packingLines, 1m, attribLine1_7);
				AssertDocWhsPackingSlipLineExist(packingLines, 1m, attribLine1_8);
				AssertDocWhsPackingSlipLineExist(packingLines, 2m, attribLine1_9);

				AssertRolledUpDocWhsPackingSlipLineExist(packingLines, 6m, orderLine2_1, nameof(orderLine2_1)); // Should Be Rolled Up AttribLine2_1, 2_2, 2_3, 2_4, 2_9
				AssertRolledUpDocWhsPackingSlipLineExist(packingLines, 4m, orderLine2_2, nameof(orderLine2_2)); // Should Be Rolled Up AttribLine2_5, 2_6, 2_7, 2_8
				AssertRolledUpDocWhsPackingSlipLineExist(packingLines, 6m, orderLine2_3, nameof(orderLine2_3)); // Should Be Rolled Up AttribLine2_1, 2_2, 2_3, 2_4, 2_9

				AssertRolledUpDocWhsPackingSlipLineExist(packingLines, 5m, orderLine3_1, nameof(orderLine3_1)); // SerialNumber prevents Roll Up
				AssertRolledUpDocWhsPackingSlipLineExist(packingLines, 5m, orderLine3_2, nameof(orderLine3_2)); // SerialNumber prevents Roll Up

				AssertRolledUpDocWhsPackingSlipLineExist(packingLines, 5m, orderLine4_1, nameof(orderLine4_1));
				AssertRolledUpDocWhsPackingSlipLineExist(packingLines, 5m, orderLine4_2, nameof(orderLine4_2));
			});
		}

		WhsReleaseLine CreatePickableDocketLineAttribute(WhsPickableDocketLine pickableDocketLine, ZDecimal units, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber, ZDateTime expiryDate, ZDateTime packingDate)
		{
			var releaseLine = pickableDocketLine.ReleaseLines.AddNew(partAttrib1, partAttrib2, partAttrib3, serialNumber, expiryDate.Date, packingDate.Date);
			releaseLine.Quantity = units;
			return releaseLine;
		}

		void AssertDocWhsPackingSlipLineExist(TWrapper[] packingLineWrappers, ZDecimal expectedQuantity, WhsReleaseLine expectedReleaseLine)
		{
			bool result = false;

			foreach (var packingLine in packingLineWrappers)
			{
				if (GetPartAttrib1(packingLine) == expectedReleaseLine.PartAttribute1 &&
					GetPartAttrib2(packingLine) == expectedReleaseLine.PartAttribute2 &&
					GetPartAttrib3(packingLine) == expectedReleaseLine.PartAttribute3 &&
					GetSerialNumber(packingLine) == expectedReleaseLine.SerialNumber &&
					GetExpiryDate(packingLine) == expectedReleaseLine.ExpiryDate &&
					GetPackingDate(packingLine) == expectedReleaseLine.PackingDate &&
					((WhsReleaseLine)(packingLine as DocBaseWrapper).WrappedObject).Quantity == expectedQuantity)
				{
					result = true;
					break;
				}
			}

			Assert("Packing Slip Wrapper with set parameters couldn't be found.", result);
		}

		protected abstract ZString GetPartAttrib1(TWrapper wrapper);
		protected abstract ZString GetPartAttrib2(TWrapper wrapper);
		protected abstract ZString GetPartAttrib3(TWrapper wrapper);
		protected abstract ZString GetSerialNumber(TWrapper wrapper);
		protected abstract ZDateTime GetExpiryDate(TWrapper wrapper);
		protected abstract ZDateTime GetPackingDate(TWrapper wrapper);

		void AssertRolledUpDocWhsPackingSlipLineExist(TWrapper[] packingLineWrappers, ZDecimal expectedQuantity, WhsPickableDocketLine parent, string parentName = "")
		{
			var result = false;
			foreach (var packingLine in packingLineWrappers)
			{
				if (GetPartAttrib1(packingLine) == parent.WE_PartAttrib1 &&
					GetPartAttrib2(packingLine) == parent.WE_PartAttrib2 &&
					GetPartAttrib3(packingLine) == parent.WE_PartAttrib3 &&
					GetSerialNumber(packingLine) == parent.WE_SerialNumber &&
					GetExpiryDate(packingLine) == parent.WE_ExpiryDate &&
					GetPackingDate(packingLine) == parent.WE_PackingDate &&
					GetUnitsMet(packingLine) == expectedQuantity)
				{
					result = true;
					break;
				}
			}

			Assert($"Packing Slip Wrapper with set parameters couldn't be found for pickable docketline: {parentName}.", result);
		}

		protected abstract ZDecimal GetUnitsMet(TWrapper wrapper);

		#endregion

		#region TestGetPackingLines_Sort

		public void TestGetPackingLines_Sort()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			data.Part1.OP_Desc = "DESC3";
			data.Part2.OP_Desc = "DESC1";
			part3.OP_Desc = "DESC2";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, part3, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Preconditon", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsOrderLine(order, part3, 1m, 1, 0);
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, 3, 0);
			var line3 = Helper.CreateWhsOrderLine(order, data.Part2, 1m, 2, 0);

			Helper.CreatePickNew(order);

			data.Org1.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.ProductCode;
			AssertPackingLinesSortedOrder(order, "00003", "00001", "00002");

			data.Org1.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.ProductDescription;
			AssertPackingLinesSortedOrder(order, "00002", "00003", "00001");

			data.Org1.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.LineNo;
			AssertPackingLinesSortedOrder(order, "00001", "00003", "00002");

			data.Org1.MiscServ.OM_WhsPackingSlipOrderBy = WhsPackingSlipOrderByList.Codes.SameAsOnGrid;
			AssertPackingLinesSortedOrder(order, "00001", "00002", "00003");

			data.Org1.MiscServ.OM_WhsPackingSlipOrderBy = "DEF";

			using (WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.ProductCode))
			{
				AssertPackingLinesSortedOrder(order, "00003", "00001", "00002");
			}

			using (WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.ProductDescription))
			{
				AssertPackingLinesSortedOrder(order, "00002", "00003", "00001");
			}

			using (WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.LineNo))
			{
				AssertPackingLinesSortedOrder(order, "00001", "00003", "00002");
			}

			using (WarehouseDataRegistry.Instance.PackingSlipOrderBy.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, WhsPackingSlipOrderByList.Codes.SameAsOnGrid))
			{
				AssertPackingLinesSortedOrder(order, "00001", "00002", "00003");
			}
		}

		void AssertPackingLinesSortedOrder(WhsPickableDocket pickableDocket, ZString expectedLine1Position, ZString expectedLine2Position, ZString expectedLine3Position)
		{
			var miscServ = pickableDocket.Client.MiscServ;
			var sortedBy = miscServ.OM_WhsPackingSlipOrderBy_List.GetDescriptionFromCode(miscServ.OM_WhsPackingSlipOrderBy);
			var helper = GetNewHelper();
			var packingLines = helper.GetPackingLines(pickableDocket).Wrappers.ToArray();
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine1Position, packingLines[0].PositionAfterSorting);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine2Position, packingLines[1].PositionAfterSorting);
			AssertEquals("Lines should be Sorted By:" + sortedBy, expectedLine3Position, packingLines[2].PositionAfterSorting);
		}

		#endregion

		#region TestGetPackingLines_WontBlowUpForShortPickedSerials

		public void TestGetPackingLines_WontBlowUpForShortPickedSerials()
		{
			// this is a test to simulate problem from WI00134479
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			var orderLine = (WhsOrderLine)order.Lines.Single();
			Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals("Precondition: no picklines", 0, orderLine.PickLines.Count);
			AssertEquals("Precondition: no release lines", 0, orderLine.ReleaseLines.Count);

			var helper = GetNewHelper();
			AssertNoExceptionThrown(() => helper.GetPackingLines(order));
		}

		#endregion

		#region Implementation

		protected abstract PackingSlipLineWrapperCollectionHelper<TWrapper, TCollection> GetNewHelper();

		#endregion
	}
}
