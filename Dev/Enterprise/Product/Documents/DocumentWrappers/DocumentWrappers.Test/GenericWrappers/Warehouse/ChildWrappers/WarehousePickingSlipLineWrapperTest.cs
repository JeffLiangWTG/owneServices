using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehousePickingSlipLineWrapper))]
	public class WarehousePickingSlipLineWrapperTest : WarehouseGenericLineWrapperTest
	{
		#region Current

		#region TestPickGroup

		public void TestPickGroup()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var pickLine = Factory.New<WhsPickLine>();
			var wrapper = GetNewDocWrapper(pickLine);
			AssertEquals("", wrapper.PickGroup);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			pickLine.WZ_WE_TransactionLine = orderLine.PK;
			AssertEquals("None", wrapper.PickGroup);

			var collection = new PickGroupCollection();
			var pickGroup = collection.AddNew();
			pickGroup.Description = (NoResString)"Heavy Products";

			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				orderLine.WE_PickGroup = 1;
				AssertEquals("Heavy Products", wrapper.PickGroup);
			}
		}

		#endregion

		#region TestPickGroupNumber

		public void TestPickGroupNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var pickLine = Factory.New<WhsPickLine>();
			var wrapper = GetNewDocWrapper(pickLine);
			AssertEquals(new ZShort(0), wrapper.PickGroupNumber);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			pickLine.WZ_WE_TransactionLine = orderLine.PK;
			AssertEquals(short.MaxValue, wrapper.PickGroupNumber);

			var collection = new PickGroupCollection();
			var pickGroup = collection.AddNew();
			pickGroup.Description = (NoResString)"Heavy Products";

			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				orderLine.WE_PickGroup = 1;
				AssertEquals(new ZShort(1), wrapper.PickGroupNumber);
			}
		}

		#endregion

		#region TestRowPathSequence

		public void TestRowPathSequence()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 1, 1);
			row.WR_PickPathSequence = 4;
			var pickLine = Factory.New<WhsPickLine>();

			var wrapper = GetNewDocWrapper(pickLine);
			AssertEquals(new ZShort(0), wrapper.RowPathSequence);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, row.Locations[0], "");
			var inventory = receive.Inventory[0];
			pickLine.WZ_WE_InventoryLine = inventory.WI_WE_InDocketLine;
			AssertEquals(new ZShort(4), wrapper.RowPathSequence);
		}

		public void TestRowPathSequence_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 1, 1);
			row.WR_PickPathSequence = 4;

			var location = row.Locations.FindByColumnLevelTray(1, 1, 1);
			var pickLine = Factory.New<WhsPickLine>();
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, location, data.Whs1.DefaultOutboundDockDoorLocation);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location, "");
			pickLine.WZ_WE_InventoryLine = transferLine.PK;
			pickLine.WZ_WE_OriginalPickedInventoryLine = receive.Lines[0].PK;

			var wrapper = GetNewDocWrapper(pickLine);
			AssertEquals(new ZShort(4), wrapper.RowPathSequence);
		}

		#endregion

		#region TestPickPathSequence

		public void TestPickPathSequence()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 1, 1);
			var location = row.Locations[0];
			location.WLV_PickPathSequence = 4;
			var pickLine = Factory.New<WhsPickLine>();

			var wrapper = GetNewDocWrapper(pickLine);
			AssertEquals(0, wrapper.PickPathSequence);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location, "");
			var inventory = receive.Inventory[0];
			pickLine.WZ_WE_InventoryLine = inventory.WI_WE_InDocketLine;
			AssertEquals(new ZShort(4), wrapper.PickPathSequence);
		}

		public void TestPickPathSequence_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 1, 1);
			var location = row.Locations.FindByColumnLevelTray(1, 1, 1);
			location.WLV_PickPathSequence = 4;
			var pickLine = Factory.New<WhsPickLine>();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, location, data.Whs1.DefaultOutboundDockDoorLocation);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location, "");
			pickLine.WZ_WE_InventoryLine = transferLine.PK;
			pickLine.WZ_WE_OriginalPickedInventoryLine = receive.Lines[0].PK;

			var wrapper = GetNewDocWrapper(pickLine);
			AssertEquals(new ZShort(4), wrapper.PickPathSequence);
		}

		#endregion

		#region TestPickArea

		public void TestPickArea()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var location = data.Whs1.FindLocation("A-1-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", new TestNotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location);
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickingSlipWrapper = new WarehousePickingSlipWrapper(pick, Factory);

			var docWrapper = pickingSlipWrapper.PickingLines.Cast<WarehousePickingSlipLineWrapper>().Single(l => l.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("DocWrapper PickArea is empty when Registry BreakSystemDefinedPickSlipByArea is disabled", string.Empty, docWrapper.PickArea);

			using (WarehouseDataRegistry.Instance.BreakSystemDefinedPickSlipByArea.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("DocWrapper PickArea is correct when Registry BreakSystemDefinedPickSlipByArea is enabled", location.PickingArea.WA_NameMultilingual, docWrapper.PickArea);
			}
		}

		#endregion

		#region TestPickMethod

		public void TestPickMethod()
		{
			WhsLocation location = Factory.NewWithValidTestData<WhsLocation>();
			location.WLV_PickMethod = "ANY";

			Inventory.WI_WL = location.PK;
			AssertEquals("DocWrapper PickMethod is incorrect", "Any", DocWrapper.PickMethod);

			Inventory.WI_WL = ZGuid.Empty;
			AssertEquals("Should not blow up if no location on inventory", "", DocWrapper.PickMethod);
			Inventory.WI_WL = location.PK; // clear up			
		}

		public void TestPickMethod_InTransit()
		{
			var pickMethods = new SystemDefinableCodeDescriptionBoolCollection();
			var pickMethod = pickMethods.AddNew();
			pickMethod.Code = "TRB";
			pickMethod.Bool = true;
			pickMethod.Description = (NoResString)"Tractor Beam";

			using (WarehouseDataRegistry.Instance.PickMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, pickMethods))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 1, 1);
				var location = row.Locations.FindByColumnLevelTray(1, 1, 1);
				location.WLV_PickMethod = "TRB";
				var pickLine = Factory.New<WhsPickLine>();

				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, location, data.Whs1.DefaultOutboundDockDoorLocation);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location, "");
				pickLine.WZ_WE_InventoryLine = transferLine.PK;
				pickLine.WZ_WE_OriginalPickedInventoryLine = receive.Lines[0].PK;

				var wrapper = GetNewDocWrapper(pickLine);
				AssertEquals("Tractor Beam", wrapper.PickMethod);
			}
		}

		#endregion

		#region TestExtraDetails

		public void TestExtraDetails()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Inventory.WI_OP = ZGuid.Empty;
			AssertEquals("DocWrapper ExtraDetails is incorrect", "0 ; 0 ; 0 PLT", DocWrapper.ExtraDetails);

			Inventory.WI_OH_Client = data.Org1.PK;
			Inventory.WI_OP = data.Part1.PK;
			AssertEquals("DocWrapper ExtraDetails is incorrect", "0.0 KG; 0.00 M3; 0 PLT", DocWrapper.ExtraDetails);

			Helper.CreateProductUnit(data.Part1, "PLT", 5);
			DocWrapper.Units = 10m;
			data.Part1.OP_Cubic = 10m;
			data.Part1.OP_CubicUQ = "M3";
			data.Part1.OP_Weight = 20m;
			data.Part1.OP_WeightUQ = "KG";
			AssertEquals("DocWrapper ExtraDetails is incorrect", "200 KG; 100 M3; 2 PLT", DocWrapper.ExtraDetails);

			// setup pickline such that we have a valid unit conversion for pack type on the product
			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			PickLine.WZ_WE_TransactionLine = order.Lines.AddNew().PK;
			Helper.CreateProductUnit(data.Part1, "BOX", 2);

			WhsProductParamsByWhsAndClient productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_F3_NKReleasedPackType = "BOX";

			DocWrapper.Units = 10.121212m;

			AssertEquals("DocWrapper ExtraDetails is incorrect", "202.42 KG; 101.2121 M3; 5.1 BOXES; 2.0 PLT", DocWrapper.ExtraDetails);

			data.Part1.OP_CubicUQ = "L";
			data.Part1.OP_WeightUQ = "LB";
			AssertEquals("DocWrapper ExtraDetail is incirrect", "202.42 LB; 101.2121 L; 5.1 BOXES; 2.0 PLT", DocWrapper.ExtraDetails);
		}

		#endregion

		#region TestVolume

		public void TestVolume()
		{
			Inventory.WI_OP = Factory.New<OrgSupplierPart>().PK;
			AssertEquals("DocWrapper Volume is incorrect", 0m, DocWrapper.Volume);

			DocWrapper.Units = 10m;
			Inventory.SupplierPart.OP_Cubic = 10m;
			AssertEquals("DocWrapper Volume is incorrect", 100m, DocWrapper.Volume);

			Inventory.SupplierPart.OP_Cubic = 20m;
			DocWrapper.Units = 20m;
			AssertEquals("DocWrapper Volume is incorrect", 400m, DocWrapper.Volume);

			DocWrapper.Units = 20.1212m;
			AssertEquals("DocWrapper Volume is incorrect", 402.4240m, DocWrapper.Volume);

			PickLine.WZ_Units = 50m;
			AssertEquals("DocWrapper Volume should depend on DocWrapper.Units field and not PickLine.WZ_Units", 402.4240m, DocWrapper.Volume);
		}

		#endregion

		#region TestVolumeUQ

		public void TestVolumeUQ()
		{
			Inventory.WI_OP = Factory.New<OrgSupplierPart>().PK;
			Inventory.SupplierPart.OP_CubicUQ = "IN";
			AssertEquals("DocWrapper Weight is incorrect", "IN", DocWrapper.VolumeUQ);

			Inventory.SupplierPart.OP_CubicUQ = "M3";
			AssertEquals("DocWrapper Weight is incorrect", "M3", DocWrapper.VolumeUQ);
		}

		#endregion

		#region TestWeight

		public void TestWeight()
		{
			Inventory.WI_OP = Factory.New<OrgSupplierPart>().PK;
			Inventory.SupplierPart.OP_Weight = 10m;
			DocWrapper.Units = 10m;
			AssertEquals("DocWrapper Weight is incorrect", 100m, DocWrapper.Weight);

			Inventory.SupplierPart.OP_Weight = 20m;
			DocWrapper.Units = 20m;
			AssertEquals("DocWrapper Weight is incorrect", 400m, DocWrapper.Weight);

			DocWrapper.Units = 20.1212m;
			AssertEquals("DocWrapper Weight is incorrect", 402.42m, DocWrapper.Weight);

			PickLine.WZ_Units = 50m;
			AssertEquals("DocWrapper Weight should depend on DocWrapper.Units field and not PickLine.WZ_Units", 402.42m, DocWrapper.Weight);
		}

		#endregion

		#region TestWeightUQ

		public void TestWeightUQ()
		{
			Inventory.WI_OP = Factory.New<OrgSupplierPart>().PK;
			Inventory.SupplierPart.OP_WeightUQ = "KG";
			AssertEquals("DocWrapper Weight is incorrect", "KG", DocWrapper.WeightUQ);

			Inventory.SupplierPart.OP_WeightUQ = "PB";
			AssertEquals("DocWrapper Weight is incorrect", "PB", DocWrapper.WeightUQ);
		}

		#endregion

		#region TestUnitsUQ

		public void TestUnitsUQ()
		{
			Inventory.WI_OP = Factory.New<OrgSupplierPart>().PK;
			Inventory.SupplierPart.OP_StockKeepingUnit = "TST";
			AssertEquals("DocWrapper UnitsUQ is incorrect", "TST", DocWrapper.UnitsUQ);
		}

		#endregion

		#region Part Attributes

		#region TestPackingDate

		public void TestPackingDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.VIN);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Factory.Save();

			var dateTime_FromInventory = ZDate.Today.AddDays(3);
			var dateTime_FromOrderLine = ZDate.Today.AddDays(5);

			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, dateTime_FromInventory, "SN:1", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure Receive is Finalised.", true, receive.IsFinalised);

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - ensure order is Picked.", true, order.IsAttachedToPickButNotFinalised);

			orderLine.WE_PackingDate = dateTime_FromOrderLine; // do it after picking, so the inventory could be allocated.
			var warehousePickingSlipLineWrapper = WarehousePickingSlipLineWrapper.New(orderLine.PickLines[0], Factory);

			AssertEquals("warehousePickingSlipLineWrapper.PackingDate should come from Inventory", dateTime_FromInventory, warehousePickingSlipLineWrapper.PackingDate);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("warehousePickingSlipLineWrapper.PackingDate should come from Order Line", dateTime_FromOrderLine, warehousePickingSlipLineWrapper.PackingDate);
		}

		#endregion

		#region TestExpiryDate

		public void TestExpiryDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.VIN);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var dateTime_FromInventory = ZDate.Today.AddDays(3);
			var dateTime_FromOrderLine = ZDate.Today.AddDays(5);

			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, dateTime_FromInventory, ZDate.Empty, "SN:1", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure Receive is Finalised.", true, receive.IsFinalised);

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - ensure order is Picked.", true, order.IsAttachedToPickButNotFinalised);

			orderLine.WE_ExpiryDate = dateTime_FromOrderLine; // do it after picking, so the inventory could be allocated.
			var warehousePickingSlipLineWrapper = WarehousePickingSlipLineWrapper.New(orderLine.PickLines[0], Factory);

			AssertEquals("warehousePickingSlipLineWrapper.ExpiryDate should come from Inventory", dateTime_FromInventory, warehousePickingSlipLineWrapper.ExpiryDate);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("warehousePickingSlipLineWrapper.ExpiryDate should come from Order Line", dateTime_FromOrderLine, warehousePickingSlipLineWrapper.ExpiryDate);
		}

		#endregion

		#region TestPackingDateWithLabel

		public void TestPackingDateWithLabel()
		{
			var testDate1 = ZDate.Today;
			var testDate2 = ZDate.Today.AddDays(1);
			Inventory.WI_PackingDate = testDate1;
			AssertEquals("DocWrapper PackingDateWithLabel is incorrect", new ZString("Packing Date: " + testDate1.ToShortDateString()), DocWrapper.PackingDateWithLabel);
		}

		#endregion

		#region TestExpiryDateWithLabel

		public void TestExpiryDateWithLabel()
		{
			var testDate1 = ZDate.Today;
			var testDate2 = ZDate.Today.AddDays(1);
			Inventory.WI_ExpiryDate = testDate1;
			AssertEquals("DocWrapper ExpiryDateWithLabel is incorrect", new ZString("Expiry Date: " + testDate1.ToShortDateString()), DocWrapper.ExpiryDateWithLabel);
		}

		#endregion

		#region TestPartAttribute1

		public void TestPartAttribute1()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.VIN);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Factory.Save();

			var serial_FromInventory = "SN:1";
			var serial_FromOrderLine = "SN:2";

			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, serial_FromInventory, "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure Receive is Finalised.", true, receive.IsFinalised);

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - ensure order is Picked.", true, order.IsAttachedToPickButNotFinalised);

			orderLine.WE_PartAttrib1 = serial_FromOrderLine; // do it after picking, so the inventory could be allocated.
			var warehousePickingSlipLineWrapper = WarehousePickingSlipLineWrapper.New(orderLine.PickLines[0], Factory);
			AssertEquals("warehousePickingSlipLineWrapper.PartAttribute1 should come from Inventory", serial_FromInventory, warehousePickingSlipLineWrapper.PartAttribute1);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("warehousePickingSlipLineWrapper.PartAttribute1 should come from Order Line", serial_FromOrderLine, warehousePickingSlipLineWrapper.PartAttribute1);
		}

		#endregion

		#region TestPartAttribute2

		public void TestPartAttribute2()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.VIN);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Factory.Save();

			var serial_FromInventory = "SN:1";
			var serial_FromOrderLine = "SN:2";

			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", serial_FromInventory, "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure Receive is Finalised.", true, receive.IsFinalised);

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - ensure order is Picked.", true, order.IsAttachedToPickButNotFinalised);

			orderLine.WE_PartAttrib2 = serial_FromOrderLine; // do it after picking, so the inventory could be allocated.
			var warehousePickingSlipLineWrapper = WarehousePickingSlipLineWrapper.New(orderLine.PickLines[0], Factory);

			AssertEquals("warehousePickingSlipLineWrapper.PartAttribute2 should come from Inventory", serial_FromInventory, warehousePickingSlipLineWrapper.PartAttribute2);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("warehousePickingSlipLineWrapper.PartAttribute2 should come from Order Line", serial_FromOrderLine, warehousePickingSlipLineWrapper.PartAttribute2);
		}

		#endregion

		#region TestPartAttribute3

		public void TestPartAttribute3()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.VIN);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Factory.Save();

			var serial_FromInventory = "SN:1";
			var serial_FromOrderLine = "SN:2";

			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", serial_FromInventory, "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure Receive is Finalised.", true, receive.IsFinalised);

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - ensure order is Picked.", true, order.IsAttachedToPickButNotFinalised);

			orderLine.WE_PartAttrib3 = serial_FromOrderLine; // do it after picking, so the inventory could be allocated.
			var warehousePickingSlipLineWrapper = WarehousePickingSlipLineWrapper.New(orderLine.PickLines[0], Factory);

			AssertEquals("warehousePickingSlipLineWrapper.PartAttribute3 should come from Inventory", serial_FromInventory, warehousePickingSlipLineWrapper.PartAttribute3);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("warehousePickingSlipLineWrapper.PartAttribute3 should come from Order Line", serial_FromOrderLine, warehousePickingSlipLineWrapper.PartAttribute3);
		}

		#endregion

		#region TestTrackedSerialNumber

		public void TestTrackedSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var serial_FromInventory = "SN:1";
			var serial_FromOrderLine = "SN:2";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory.WI_SerialNumber = serial_FromInventory;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure Receive is Finalised.", true, receive.IsFinalised);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Factory.Save();

			Helper.CreatePickNew(order);
			AssertEquals("Precondition - ensure order is Picked.", true, order.IsAttachedToPickButNotFinalised);

			orderLine.WE_SerialNumber = serial_FromOrderLine; // do it after picking, so the inventory could be allocated.
			var warehousePickingSlipLineWrapper = WarehousePickingSlipLineWrapper.New(orderLine.PickLines[0], Factory);

			AssertEquals("warehousePickingSlipLineWrapper.TrackedSerialNumber should come from Inventory", serial_FromInventory, warehousePickingSlipLineWrapper.TrackedSerialNumber);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RollUpAttributesOnDocuments = true;
			AssertEquals("warehousePickingSlipLineWrapper.TrackedSerialNumber should come from Order Line", serial_FromOrderLine, warehousePickingSlipLineWrapper.TrackedSerialNumber);
		}

		#endregion

		#region TestPartAttribute1WithLabel

		public void TestPartAttribute1WithLabel()
		{
			CombineAssertions(() =>
			{
				var receiveLine = Factory.New<WhsReceiveLine>();
				var pickLine = Factory.New<WhsPickLine>();
				pickLine.WZ_WE_InventoryLine = receiveLine.PK;
				var wrapper = GetNewDocWrapper(pickLine);
				AssertEquals("DocWrapper PartAttribute1WithLabel shoule be empty", ZString.Empty, wrapper.PartAttribute1WithLabel);

				var receive = Factory.New<WhsReceive>();
				receiveLine.WE_WD = receive.PK;
				receive.WD_OH_Client = Factory.New<OrgHeader>().PK;
				receiveLine.WE_PartAttrib1 = ZString.Empty;
				AssertEquals("DocWrapper PartAttribute1WithLabel shoule be empty", ZString.Empty, wrapper.PartAttribute1WithLabel);

				receiveLine.WE_PartAttrib1 = "TEST";
				receive.Client.MiscServ.OM_IMPartAttrib1Name = ZString.Empty;
				AssertEquals("DocWrapper PartAttribute1WithLabel is correct", "Attribute 1: TEST", wrapper.PartAttribute1WithLabel);

				receiveLine.WE_PartAttrib1 = "TEST";
				receive.Client.MiscServ.OM_IMPartAttrib1Name = "LABEL";
				AssertEquals("DocWrapper PartAttribute1WithLabel is correct", "LABEL: TEST", wrapper.PartAttribute1WithLabel);
			});
		}

		public void TestPartAttribute1WithLabel_Translatable()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_WE_InventoryLine = receiveLine.PK;

			var receive = Factory.New<WhsReceive>();
			receiveLine.WE_WD = receive.PK;
			var client = Factory.New<OrgHeader>();
			receive.WD_OH_Client = client.PK;

			var miscServ = client.MiscServ;
			receiveLine.WE_PartAttrib1 = "TEST";
			miscServ.OM_IMPartAttrib1Name = "LABEL";
			var wrapper = GetNewDocWrapper(pickLine);
			AssertEquals("PartAttribute1WithLabel in English", "LABEL: TEST", wrapper.PartAttribute1WithLabel);

			var resKey = miscServ.OM_IMPartAttrib1NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("PartAttribute1WithLabel in Chinese", "标签: TEST", wrapper.PartAttribute1WithLabel);
			}
		}

		#endregion

		#region TestPartAttribute2WithLabel

		public void TestPartAttribute2WithLabel()
		{
			CombineAssertions(() =>
			{
				var receiveLine = Factory.New<WhsReceiveLine>();
				var pickLine = Factory.New<WhsPickLine>();
				pickLine.WZ_WE_InventoryLine = receiveLine.PK;
				var wrapper = GetNewDocWrapper(pickLine);
				AssertEquals("DocWrapper PartAttribute2WithLabel shoule be empty", ZString.Empty, wrapper.PartAttribute2WithLabel);

				var receive = Factory.New<WhsReceive>();
				receiveLine.WE_WD = receive.PK;
				receive.WD_OH_Client = Factory.New<OrgHeader>().PK;
				receiveLine.WE_PartAttrib2 = ZString.Empty;
				AssertEquals("DocWrapper PartAttribute2WithLabel shoule be empty", ZString.Empty, wrapper.PartAttribute2WithLabel);

				receiveLine.WE_PartAttrib2 = "TEST";
				receive.Client.MiscServ.OM_IMPartAttrib2Name = ZString.Empty;
				AssertEquals("DocWrapper PartAttribute2WithLabel is correct", "Attribute 2: TEST", wrapper.PartAttribute2WithLabel);

				receiveLine.WE_PartAttrib2 = "TEST";
				receive.Client.MiscServ.OM_IMPartAttrib2Name = "LABEL";
				AssertEquals("DocWrapper PartAttribute2WithLabel is correct", "LABEL: TEST", wrapper.PartAttribute2WithLabel);
			});
		}

		public void TestPartAttribute2WithLabel_Translatable()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_WE_InventoryLine = receiveLine.PK;

			var receive = Factory.New<WhsReceive>();
			receiveLine.WE_WD = receive.PK;
			var client = Factory.New<OrgHeader>();
			receive.WD_OH_Client = client.PK;

			var miscServ = client.MiscServ;
			receiveLine.WE_PartAttrib2 = "TEST";
			miscServ.OM_IMPartAttrib2Name = "LABEL";
			var wrapper = GetNewDocWrapper(pickLine);
			AssertEquals("PartAttribute2WithLabel in English", "LABEL: TEST", wrapper.PartAttribute2WithLabel);

			var resKey = miscServ.OM_IMPartAttrib2NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("PartAttribute2WithLabel in Chinese", "标签: TEST", wrapper.PartAttribute2WithLabel);
			}
		}

		#endregion

		#region TestPartAttribute3WithLabel

		public void TestPartAttribute3WithLabel()
		{
			CombineAssertions(() =>
			{
				var receiveLine = Factory.New<WhsReceiveLine>();
				var pickLine = Factory.New<WhsPickLine>();
				pickLine.WZ_WE_InventoryLine = receiveLine.PK;
				var wrapper = GetNewDocWrapper(pickLine);
				AssertEquals("DocWrapper PartAttribute3WithLabel shoule be empty", ZString.Empty, wrapper.PartAttribute3WithLabel);

				var receive = Factory.New<WhsReceive>();
				receiveLine.WE_WD = receive.PK;
				receive.WD_OH_Client = Factory.New<OrgHeader>().PK;
				receiveLine.WE_PartAttrib3 = ZString.Empty;
				AssertEquals("DocWrapper PartAttribute3WithLabel shoule be empty", ZString.Empty, wrapper.PartAttribute3WithLabel);

				receiveLine.WE_PartAttrib3 = "TEST";
				receive.Client.MiscServ.OM_IMPartAttrib3Name = ZString.Empty;
				AssertEquals("DocWrapper PartAttribute3WithLabel is correct", "Attribute 3: TEST", wrapper.PartAttribute3WithLabel);

				receiveLine.WE_PartAttrib3 = "TEST";
				receive.Client.MiscServ.OM_IMPartAttrib3Name = "LABEL";
				AssertEquals("DocWrapper PartAttribute3WithLabel is correct", "LABEL: TEST", wrapper.PartAttribute3WithLabel);
			});
		}

		public void TestPartAttribute3WithLabel_Translatable()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_WE_InventoryLine = receiveLine.PK;

			var receive = Factory.New<WhsReceive>();
			receiveLine.WE_WD = receive.PK;
			var client = Factory.New<OrgHeader>();
			receive.WD_OH_Client = client.PK;

			var miscServ = client.MiscServ;
			receiveLine.WE_PartAttrib3 = "TEST";
			miscServ.OM_IMPartAttrib3Name = "LABEL";
			var wrapper = GetNewDocWrapper(pickLine);
			AssertEquals("PartAttribute3WithLabel in English", "LABEL: TEST", wrapper.PartAttribute3WithLabel);

			var resKey = miscServ.OM_IMPartAttrib3NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("PartAttribute3WithLabel in Chinese", "标签: TEST", wrapper.PartAttribute3WithLabel);
			}
		}

		#endregion

		#region TestTrackedSerialWithLabel

		public void TestTrackedSerialWithLabel()
		{
			CombineAssertions(() =>
			{
				var receiveLine = Factory.New<WhsReceiveLine>();
				var pickLine = Factory.New<WhsPickLine>();
				pickLine.WZ_WE_InventoryLine = receiveLine.PK;
				var wrapper = GetNewDocWrapper(pickLine);
				AssertEquals("DocWrapper TrackedSerialWithLabel should be empty", ZString.Empty, wrapper.TrackedSerialWithLabel);

				var receive = Factory.New<WhsReceive>();
				receiveLine.WE_WD = receive.PK;
				receive.WD_OH_Client = Factory.New<OrgHeader>().PK;
				receiveLine.WE_SerialNumber = ZString.Empty;
				AssertEquals("DocWrapper TrackedSerialWithLabel should be empty", ZString.Empty, wrapper.TrackedSerialWithLabel);

				receiveLine.WE_SerialNumber = "TEST";
				AssertEquals("DocWrapper TrackedSerialWithLabel is correct", "Tracked Serial Number: TEST", wrapper.TrackedSerialWithLabel);
			});
		}

		#endregion

		#endregion

		#region TestLocationString

		public void TestLocationString()
		{
			var whs = Helper.CreateWarehouse("1");
			var locations = Helper.CreateRowAndGenerateLocations(whs, "AA", 4, 3, 2).Locations;
			Inventory.WI_WL = locations[locations.Count - 1].PK;
			AssertEquals("DocWrapper LocationString is incorrect", "AA-4-3-2", DocWrapper.LocationString);
		}

		public void TestLocationString_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 2, 1);
			var location = row.Locations.FindByColumnLevelTray(1, 1, 1);
			var pickLine = Factory.New<WhsPickLine>();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, location, data.Whs1.DefaultOutboundDockDoorLocation);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location, "");
			pickLine.WZ_WE_InventoryLine = transferLine.PK;
			pickLine.WZ_WE_OriginalPickedInventoryLine = receive.Lines[0].PK;

			var wrapper = GetNewDocWrapper(pickLine);
			AssertEquals("C-1", wrapper.LocationString);
		}

		#endregion

		#region TestClientName

		public void TestClientName()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_WE_InventoryLine = receiveLine.PK;
			var wrapper = GetNewDocWrapper(pickLine);
			AssertEquals("DocWrapper client name should be empty", ZString.Empty, wrapper.ClientName);

			var receive = Factory.New<WhsReceive>();
			receiveLine.WE_WD = receive.PK;
			AssertEquals("DocWrapper client name should be empty", ZString.Empty, wrapper.ClientName);

			receive.WD_OH_Client = Factory.New<OrgHeader>().PK;
			receive.Client.OH_FullName = "TEST";
			AssertEquals("DocWrapper ClientName is incorrect", "TEST", wrapper.ClientName);
		}

		#endregion

		#region TestClientCode

		public void TestClientCode()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_WE_InventoryLine = receiveLine.PK;
			var wrapper = GetNewDocWrapper(pickLine);
			AssertEquals("DocWrapper client code should be empty", ZString.Empty, wrapper.ClientCode);

			var receive = Factory.New<WhsReceive>();
			receiveLine.WE_WD = receive.PK;
			AssertEquals("DocWrapper client code should be empty", ZString.Empty, wrapper.ClientCode);

			receive.WD_OH_Client = Factory.New<OrgHeader>().PK;
			receive.Client.OH_Code = "TST";
			AssertEquals("DocWrapper ClientName is incorrect", "TST", wrapper.ClientCode);
		}

		#endregion

		#region TestProductCode

		public void TestProductCode()
		{
			Inventory.WI_OP = ZGuid.Empty;
			AssertEquals("DocWrapper ProductCode should be empty", ZString.Empty, DocWrapper.ProductCode);
			Inventory.WI_OP = Factory.New<OrgSupplierPart>().PK;
			Inventory.SupplierPart.OP_PartNum = "TEST";
			AssertEquals("DocWrapper ProductCode is incorrect", "TEST", DocWrapper.ProductCode);
		}

		#endregion

		#region TestProductDesc

		public void TestProductDesc()
		{
			Inventory.WI_OP = ZGuid.Empty;
			AssertEquals("DocWrapper ProductDesc should be empty", ZString.Empty, DocWrapper.ProductDescription);
			Inventory.WI_OP = Factory.New<OrgSupplierPart>().PK;
			Inventory.SupplierPart.OP_Desc = "TEST";
			AssertEquals("DocWrapper ProductDesc is incorrect", "TEST", DocWrapper.ProductDescription);
		}

		#endregion

		#region TestProduct

		public void TestProduct()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_WE_InventoryLine = receiveLine.PK;
			var owner = Helper.CreateClient();
			var part = Helper.CreateProduct(owner, "P1");
			part.OP_Width = 10m;
			var receive = Factory.New<WhsReceive>();
			receiveLine.WE_WD = receive.PK;
			receive.WD_OH_Client = owner.PK;
			receiveLine.WE_OP = part.PK;
			var wrapper = GetNewDocWrapper(pickLine);
		
			AssertEquals(part, wrapper.Product.WrappedObject);
			AssertEquals(10m, wrapper.Product.Product.OP_Width);
		}

		#endregion

		#region TestSupplierProductDesc

		public void TestSupplierProductDesc()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_WE_InventoryLine = receiveLine.PK;
			var wrapper = GetNewDocWrapper(pickLine);
			AssertEquals("DocWrapper ProductDesc2 is incorrect", ZString.Empty, wrapper.SupplierProductDesc);

			var owner = Helper.CreateClient();
			var part = Helper.CreateProduct(owner, "P1");
			var receive = Factory.New<WhsReceive>();
			receiveLine.WE_WD = receive.PK;
			receive.WD_OH_Client = owner.PK;
			receiveLine.WE_OP = part.PK;

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var partRelation1 = Helper.CreateProductClientRelationShip(supplier, part, OrgPartRelation.RelationshipTypes.Supplier);
			partRelation1.OU_LocalPartDescription = "YYY";
			AssertEquals("Document Wrapper ProductDesc2 is incorrect", ZString.Empty, wrapper.SupplierProductDesc);

			var partRelation2 = Helper.CreateProductClientRelationShip(owner, part, OrgPartRelation.RelationshipTypes.Owner);
			AssertEquals("Document Wrapper ProductDesc2 is incorrect", ZString.Empty, wrapper.SupplierProductDesc);

			partRelation2.OU_LocalPartDescription = "XXX";
			AssertEquals("Document Wrapper ProductDesc2 is incorrect", "XXX", wrapper.SupplierProductDesc);
		}

		#endregion

		#region TestPallets

		public void TestPallets()
		{
			AssertEquals("DocWrapper Pallets should be 0", 0m, DocWrapper.Pallets);

			OrgHeader org = Helper.CreateClient();
			OrgSupplierPart part = Helper.CreateProduct(org, "P1");
			Helper.CreateProductUnit(part, "PLT", 5);

			Inventory.WI_OH_Client = org.PK;
			Inventory.WI_OP = part.PK;
			AssertEquals("DocWrapper Pallets should be 0", 0m, DocWrapper.Pallets);

			DocWrapper.Units = 10;
			AssertEquals("DocWrapper Pallets is incorrect", 2m, DocWrapper.Pallets);

			DocWrapper.Units = 10.121212m;
			AssertEquals("DocWrapper Pallets is incorrect", 2.0m, DocWrapper.Pallets);

			PickLine.WZ_Units = 50m;
			AssertEquals("DocWrapper Pallets should depend on DocWrapper.Units field and not PickLine.WZ_Units", 2.0m, DocWrapper.Pallets);
		}

		#endregion

		#region TestUnits

		public void TestUnits()
		{
			PickLine.WZ_Units = 10;
			DocWrapper = GetNewDocWrapper(PickLine);
			AssertEquals("DocWrapper Units is incorrect", 10m, DocWrapper.Units);

			PickLine.WZ_Units = 20;
			AssertEquals("DocWrapper Units is incorrect", 10m, DocWrapper.Units);

			DocWrapper.Units = 50;
			AssertEquals("DocWrapper Units is incorrect", 50m, DocWrapper.Units);
			AssertEquals("Pick Line Units is incorrect", 20m, PickLine.WZ_Units);
		}

		#endregion

		#region TestPalletID

		public void TestPalletID()
		{
			AssertEquals("DocWrapper Pallet ID should be empty", ZString.Empty, DocWrapper.PalletID);
			Inventory.WI_PalletID = "P_ID_001";
			AssertEquals("DocWrapper Pallet ID is incorrect", "P_ID_001", DocWrapper.PalletID);
		}

		public void TestPalletID_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();
			var pickLine = Factory.New<WhsPickLine>();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, data.Whs1.DefaultLocation, data.Whs1.DefaultOutboundDockDoorLocation);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-1");
			pickLine.WZ_WE_InventoryLine = transferLine.PK;
			pickLine.WZ_WE_OriginalPickedInventoryLine = receive.Lines[0].PK;

			var wrapper = GetNewDocWrapper(pickLine);
			AssertEquals("PLT-1", wrapper.PalletID);
		}

		#endregion

		#region TestArrivalDateAndLabel

		public void TestArrivalDateAndLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", DateTime.Now.AddDays(-2), data.Part1, 1m);
			AssertEquals("Precondition - ensure Receive is Finalised.", true, receive1.IsFinalised);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", DateTime.Now.AddDays(-1), data.Part1, 1m);
			AssertEquals("Precondition - ensure Receive is Finalised.", true, receive2.IsFinalised);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - ensure order is picking.", true, order.IsAttachedToPickButNotFinalised);

			var pickLine1 = pick.GetAllPickLines().Single(p => p.InventoryLine.PK == receive1.Lines[0].PK);
			var pickLine2 = pick.GetAllPickLines().Single(p => p.InventoryLine.PK == receive2.Lines[0].PK);
			var warehousePickingSlipLineWrapper1 = WarehousePickingSlipLineWrapper.New(pickLine1, Factory);
			var warehousePickingSlipLineWrapper2 = WarehousePickingSlipLineWrapper.New(pickLine2, Factory);

			var newFactory = new BusinessObjectFactory();
			var receive1InNewFactory = newFactory.Load<WhsReceive>(receive1.PK);
			var receive2InNewFactory = newFactory.Load<WhsReceive>(receive2.PK);
			AssertEquals("WarehousePickingSlipLineWrapper.ArrivalDate should come from Inventory.", receive1InNewFactory.WD_ArrivalDate.ToZDateTime(), warehousePickingSlipLineWrapper1.ArrivalDate);
			AssertEquals("WarehousePickingSlipLineWrapper.ArrivalDate should come from Inventory.", receive2InNewFactory.WD_ArrivalDate.ToZDateTime(), warehousePickingSlipLineWrapper2.ArrivalDate);
		}

		#endregion

		#region TestArrivalDateAndLabel_NoErrorIfNotLinkedToInventoryLine

		public void TestArrivalDateAndLabel_NoErrorIfNotLinkedToInventoryLine()
		{
			var pickLine = Factory.New<WhsPickLine>();
			var warehousePickingSlipLineWrapper = WarehousePickingSlipLineWrapper.New(pickLine, Factory);
			AssertEquals("Should return empty date with no exception.", ZDateTime.Empty, warehousePickingSlipLineWrapper.ArrivalDate);
		}

		#endregion

		#region TestReleaseUnitsAndUQ

		public void TestReleaseUnitsAndUQ()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "PCE"; // it should not be UNT.
			Helper.CreateProductUnit(data.Part1, "BOX", 10);
			Helper.CreateProductUnit(data.Part1, "UNT", 2); // previous defect was always doing UNT conversion, so setup a conversion to make sure it isn't used.

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			PickLine.WZ_WE_TransactionLine = order.Lines.AddNew().PK;
			PickLine.Inventory.WI_OP = data.Part1.PK;

			WhsProductParamsByWhsAndClient productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);

			DocWrapper.Units = 15m;
			AssertEquals("DocWrapper ReleaseUnitsAndUQ is incorrect.", "", DocWrapper.ReleaseUnitsAndUQ);

			productParams.W3_F3_NKReleasedPackType = "BOX";
			AssertEquals("DocWrapper ReleaseUnitsAndUQ is incorrect.", "1.5 BOXES", DocWrapper.ReleaseUnitsAndUQ);

			DocWrapper.Units = 25m;
			AssertEquals("DocWrapper ReleaseUnitsAndUQ is incorrect.", "2.5 BOXES", DocWrapper.ReleaseUnitsAndUQ);

			PickLine.WZ_Units = 50m;
			AssertEquals("DocWrapper ReleaseUnitsAndQU should be dependant on DocWrapper.Units rather that PickLine.WZ_Units.", "2.5 BOXES", DocWrapper.ReleaseUnitsAndUQ);
		}

		#endregion

		#region TestIsLocationEmptyAfterFinalisingPick

		#region TestIsLocationEmptyAfterFinalisingPick_VerifyEmptyLocationsTurnedOff

		public void TestIsLocationEmptyAfterFinalisingPick_VerifyEmptyLocationsTurnedOff()
		{
			AssertIsLocationEmptyAfterFinalisingPick(false);
		}

		#endregion

		#region TestIsLocationEmptyAfterFinalisingPick_VerifyEmptyLocationsTurnedOn

		public void TestIsLocationEmptyAfterFinalisingPick_VerifyEmptyLocationsTurnedOn()
		{
			AssertIsLocationEmptyAfterFinalisingPick(true);
		}

		#endregion

		#region TestIsLocationEmptyAfterFinalisingPick_WithDamagedInventory

		public void TestIsLocationEmptyAfterFinalisingPick_WithDamagedInventory()
		{
			AssertIsLocationEmptyAfterFinalisingPickWithDifferentInventoryTypes(InventoryHoldCodes.Codes.Damaged);
		}

		#endregion

		#region TestIsLocationEmptyAfterFinalisingPick_WithHeldInventory

		public void TestIsLocationEmptyAfterFinalisingPick_WithHeldInventory()
		{
			AssertIsLocationEmptyAfterFinalisingPickWithDifferentInventoryTypes(InventoryHoldCodes.Codes.Held);
		}

		#endregion

		#region TestIsLocationEmptyAfterFinalisingPick_WithCommittedInventory

		public void TestIsLocationEmptyAfterFinalisingPick_WithCommittedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			data.Whs1.WW_VerifyEmptyLocations = true;
			var location = data.Whs1.FindLocation("A");

			// setup receive, order and pick
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", new TestNotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, location);
			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			Factory.Save();

			var pickWithCommittedInventory = Helper.CreatePickNew(order1);
			AssertEquals("Precondition", 1, pickWithCommittedInventory.GetAllPickLines().Count());

			var pick = Helper.CreatePickNew(order2);
			AssertEquals("Precondition", 1, pick.GetAllPickLines().Count());

			var docWrapper1 = WarehousePickingSlipLineWrapper.New(pickWithCommittedInventory.GetAllPickLines().Single(l => l.Inventory.WI_OP == data.Part1.PK), Factory);
			AssertEquals(false, docWrapper1.IsLocationEmptyAfterPickFinalisation);

			var docWrapper2 = WarehousePickingSlipLineWrapper.New(pick.GetAllPickLines().Single(l => l.Inventory.WI_OP == data.Part1.PK), Factory);
			AssertEquals(false, docWrapper2.IsLocationEmptyAfterPickFinalisation);
		}

		#endregion

		#region TestIsLocationEmptyAfterFinalisingPick_WithHeldDamagedAndCommittedInventory

		public void TestIsLocationEmptyAfterFinalisingPick_WithHeldDamagedAndCommittedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			data.Whs1.WW_VerifyEmptyLocations = true;

			var location = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", new TestNotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "", "", InventoryHoldCodes.Codes.Held);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "", "", InventoryHoldCodes.Codes.Damaged);
			receive.FinaliseDocket();

			AssertEquals("Pre-condition", true, receive.IsFinalised);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			Factory.Save();

			var pickWithCommittedInventory = Helper.CreatePickNew(order1);
			AssertEquals(1, pickWithCommittedInventory.GetAllPickLines().Count());

			var pick = Helper.CreatePickNew(order2);
			AssertEquals(1, pick.GetAllPickLines().Count());

			var docWrapper1 = WarehousePickingSlipLineWrapper.New(pickWithCommittedInventory.GetAllPickLines().Single(l => l.Inventory.WI_OP == data.Part1.PK), Factory);
			AssertEquals(false, docWrapper1.IsLocationEmptyAfterPickFinalisation);

			var docWrapper2 = WarehousePickingSlipLineWrapper.New(pick.GetAllPickLines().Single(l => l.Inventory.WI_OP == data.Part1.PK), Factory);
			AssertEquals(false, docWrapper2.IsLocationEmptyAfterPickFinalisation);
		}

		#endregion

		void AssertIsLocationEmptyAfterFinalisingPick(bool isVerifiyEmptyLocations)
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			data.Whs1.WW_VerifyEmptyLocations = isVerifiyEmptyLocations;
			var emptyLocationAfterPickFinalisation = data.Whs1.FindLocation("A-1-1");
			var nonEmptylocationAfterPickFinalisation = data.Whs1.FindLocation("A-1-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", new TestNotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, emptyLocationAfterPickFinalisation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 2m, nonEmptylocationAfterPickFinalisation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertEquals("Precondition", true, receive.IsFinalised);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order, data.Part2, 1m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 2, pick.GetAllPickLines().Count());

			var pickingSlipWrapper = new WarehousePickingSlipWrapper(pick, Factory);

			var docWrapperForEmptyLocation = pickingSlipWrapper.PickingLines.Cast<WarehousePickingSlipLineWrapper>().Single(l => l.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("IsLocationEmptyAfterPickFinalisation property is only set if the warehouse has the 'WW_VerifyEmptyLocations' flag enabled.",
				isVerifiyEmptyLocations, docWrapperForEmptyLocation.IsLocationEmptyAfterPickFinalisation);

			var docWrapperForNonEmptyLocation = pickingSlipWrapper.PickingLines.Cast<WarehousePickingSlipLineWrapper>().Single(l => l.ProductCode == data.Part2.OP_PartNum);
			AssertEquals(false, docWrapperForNonEmptyLocation.IsLocationEmptyAfterPickFinalisation);
		}

		void AssertIsLocationEmptyAfterFinalisingPickWithDifferentInventoryTypes(string heldCode)
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			data.Whs1.WW_VerifyEmptyLocations = true;
			var location = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", new TestNotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "", "", heldCode);
			receive.FinaliseDocket();
			AssertEquals("Precondition", true, receive.IsFinalised);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 1, pick.GetAllPickLines().Count());

			var docWrapper = WarehousePickingSlipLineWrapper.New(pick.GetAllPickLines().Single(l => l.Inventory.WI_OP == data.Part1.PK), Factory);
			AssertEquals(false, docWrapper.IsLocationEmptyAfterPickFinalisation);
		}

		#endregion

		#region TestNoOfAttribsAndAttributePrintSizeFactor

		protected override bool CanSetProductAttributesInWrapperCore => false;

		#endregion

		#endregion

		#region Implementation

		public WhsPickLine PickLine
		{
			get
			{
				if (pickLine == null)
				{
					pickLine = Factory.New<WhsPickLine>();
					pickLine.WZ_WE_InventoryLine = Inventory.WI_WE_InDocketLine;
				}
				return pickLine;
			}
			set { pickLine = value; }
		}

		public WhsInventoryView Inventory
		{
			get { return inventory ?? (inventory = Factory.NewWithValidTestData<WhsReceiveLine>().Inventory[0]); }
			set { inventory = value; }
		}

		WarehousePickingSlipLineWrapper DocWrapper
		{
			get { return docWrapper ?? (docWrapper = GetNewDocWrapper(PickLine)); }
			set { docWrapper = value; }
		}

		protected virtual WarehousePickingSlipLineWrapper GetNewDocWrapper(WhsPickLine pickLine)
		{
			return WarehousePickingSlipLineWrapper.New(pickLine, Factory);
		}

		#region Abstract members Implementation

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return DocWrapper;
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
CrossDockConsigneeAddress : 
DangerousGoodsSubstance :  is null
ExtendedLinePrice :  is null
ManufacturerAddress : 
Product : (No Default Field Value Available on Product)
RecommendedUnitPrice : 
Registry : (No Default Field Value Available on Registry)
UnitDiscountAmount : 
UnitDiscountPercent : 
UnitPriceAfterDiscount : 
UnitsMet : 
UnitsOrdered : 
UnitsPicked : 
UnitsShort :
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return DocWrapper;
		}

		protected override WarehouseGenericLineWrapper GetNewWarehouseLineWrapper(BusinessObject bizO)
		{
			WhsPickLine pickLine = bizO as WhsPickLine;
			return new WarehousePickingSlipLineWrapper(pickLine, Factory);
		}

		#endregion

		WhsPickLine pickLine;
		WhsInventoryView inventory;
		WarehousePickingSlipLineWrapper docWrapper;

		protected override BusinessObject GetNewBusinessObject()
		{
			return pickLine;
		}

		#endregion
	}
}
