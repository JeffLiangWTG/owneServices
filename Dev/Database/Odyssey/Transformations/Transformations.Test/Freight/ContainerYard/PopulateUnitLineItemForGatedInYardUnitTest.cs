using System;
using System.Linq;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ContainerYard;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Freight.ContainerYard;

[TestedType(typeof(PopulateUnitLineItemForGatedInYardUnit))]
class PopulateUnitLineItemForGatedInYardUnitTest : DataTransformationTestCase
{
	protected override void PrepareTestData()
	{
		var sql = new StringBuilder();

		var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);
		var warehouse = new WhsWarehouse("WH1", "CYD").WithDockDoor(TestConnection);
		var warehouseLocations = warehouse.CreateLocations(sql, 1);

		var unitLineItem = new CYDUnitLineItem(refContainer)
		{
			YLI_Type = "CNT",
			YLI_Quantity = 1
		}.AppendInsertAndReturnObject(sql);

		var deliveryHeader = new CYDDeliveryHeader(warehouse, "JOB001").AppendInsertAndReturnObject(sql);
		var delivery = new CYDDelivery(deliveryHeader, "YDL000000000001")
		{
			YDL_YLI_UnitLineItem = unitLineItem
		}.AppendInsertAndReturnObject(sql);

		var tpu = new CYDTransportationUnit(warehouse, "TPU0000000001")
		{
			YTU_WL_WaitingBayLocation = warehouseLocations[0],
			YTU_GateInTime = new DateTime(2025, 1, 1)
		}.AppendInsertAndReturnObject(sql);

		new CYDYardUnitState(warehouse, null, null, null, loadUser: "", unloadUser: "", unitID: "CNT001", delivery: delivery)
		{
			YUS_YTU_ReceiveTransportationUnit = tpu
		}.AppendInsertAndReturnObject(sql);
		new CYDYardUnitState(warehouse, null, null, null, loadUser: "", unloadUser: "", unitID: "CNT002").AppendInsertAndReturnObject(sql);

		TestConnection.ExecuteNonQuery(sql.ToString());
		}

	protected override void AssertPreConditions()
	{
		var unitLineItems = CYDUnitLineItem.ShallowLoadFromDB(TestConnection);
		AssertEquals("One unit line item for delivery", 1, unitLineItems.Length);

		var yardUnits = CYDYardUnitState.ShallowLoadFromDB(TestConnection);
		var yardUnit1 = yardUnits.First(u => string.Equals(u.YUS_UnitID, "CNT001"));
		AssertNull("Gated in yard unit does not have line item", yardUnit1.YUS_YLI_UnitLineItem);

		var yardUnit2 = yardUnits.First(u => string.Equals(u.YUS_UnitID, "CNT002"));
		AssertNull("Other yard unit does not have line item", yardUnit2.YUS_YLI_UnitLineItem);
	}

	protected override void AssertTransformationResults()
	{
		var unitLineItems = CYDUnitLineItem.ShallowLoadFromDB(TestConnection);
		AssertEquals("A new line item is created for gated in yard unit", 2, unitLineItems.Length);

		var yardUnits = CYDYardUnitState.ShallowLoadFromDB(TestConnection);
		var yardUnit1 = yardUnits.First(u => string.Equals(u.YUS_UnitID, "CNT001"));
		AssertNotNull("Gated in yard unit should have line item", yardUnit1.YUS_YLI_UnitLineItem);

		var yardUnit2 = yardUnits.First(u => string.Equals(u.YUS_UnitID, "CNT002"));
		AssertNull("Other yard unit should not have line item", yardUnit2.YUS_YLI_UnitLineItem);
	}

	protected override DataTransformation GetNewTestTransformationInstance() => new PopulateUnitLineItemForGatedInYardUnit();
}
