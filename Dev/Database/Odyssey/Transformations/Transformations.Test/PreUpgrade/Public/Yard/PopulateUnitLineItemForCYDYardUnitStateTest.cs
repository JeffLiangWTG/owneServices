using System;
using System.Linq;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Freight.ContainerYard;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.PreUpgrade.Yard.Testing
{
	[TestedType(typeof(PopulateUnitLineItemForCYDYardUnitState))]
	public class PopulateUnitLineItemForCYDYardUnitStateTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			if (DbObjectCreator.ColumnExists(TestConnection, CYDYardUnitStateSchema.Constants.TableName, CYDYardUnitStateSchema.YUS_YLI_UnitLineItem.Name))
			{
				DBTransformationTestHelper.DropFunctionIfExists("Report_ContainerInventoryDetails");
				DBTransformationTestHelper.DropIndexIfExists(CYDYardUnitStateSchema.Constants.TableName, "FK_RX__YUS_YLI_UnitLineItem");
				DBTransformationTestHelper.DropConstraintIfExists(CYDYardUnitStateSchema.Constants.TableName, "CYDYardUnitState_YUS_YLI_UnitLineItem_FK2_CYDUnitLineItem_RRR_120N", TestConnection);
				DBTransformationTestHelper.DropConstraintIfExists(CYDYardUnitStateSchema.Constants.TableName, "Constraint_UnloadTimeNotNullThenYUS_YLI_UnitLineItemNotNull", TestConnection);
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDYardUnitStateSchema.Constants.TableName, "YUS_YLI_UnitLineItem", "UNIQUEIDENTIFIER");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDUnitLineItemSchema.Constants.TableName, "YLI_IsDamaged", "BIT", defaultValue: "0");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDUnitLineItemSchema.Constants.TableName, "YLI_TareWeight", "DECIMAL(9,3)", defaultValue: "0");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDUnitLineItemSchema.Constants.TableName, "YLI_GrossWeight", "DECIMAL(9, 3)", defaultValue: "0");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDUnitLineItemSchema.Constants.TableName, "YLI_UnitOfWeight", "VARCHAR(2)", defaultValue: "''");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDUnitLineItemSchema.Constants.TableName, "YLI_IsPreAdvice", "BIT", defaultValue: "0");
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CYDDeliverySchema.Constants.TableName, "YDL_DeliveryID", "VARCHAR(20)", defaultValue: "''");
			}

			var sql = new StringBuilder();

			var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("WH1", "CYD").WithDockDoor(TestConnection);
			var warehouseLocations = warehouse.CreateLocations(sql, 3);

			var deliveryHeader = new CYDDeliveryHeader(warehouse, "JOB001").AppendInsertAndReturnObject(sql);
			var unitLineItem1 = new CYDUnitLineItem(refContainer).AppendInsertAndReturnObject(sql);
			var unitLineItem2 = new CYDUnitLineItem(refContainer, type: "GEN").AppendInsertAndReturnObject(sql);
			var unitLineItem3 = new CYDUnitLineItem(refContainer, type: "GEN", isEmpty: false).AppendInsertAndReturnObject(sql);
			var receiveAdvice = new CYDReceiveAdvice(warehouse, "JOB001").AppendInsertAndReturnObject(sql);

			var unitLine1 = new CYDUnitLineItem(refContainer) { YLI_Type = "CNT", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice) { YRL_YLI_UnitLineItem = unitLine1 }.AppendInsertAndReturnObject(sql);

			var unitLine2 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine2 = new CYDReceiveAdviceLine(receiveAdvice) { YRL_YLI_UnitLineItem = unitLine2 }.AppendInsertAndReturnObject(sql);

			var delivery1 = new CYDDelivery(deliveryHeader, "YDL000000000001")
			{
				YDL_YLI_UnitLineItem = unitLineItem1
			}.AppendInsertAndReturnObject(sql);

			var delivery2 = new CYDDelivery(deliveryHeader, "YDL000000000002")
			{
				YDL_YLI_UnitLineItem = unitLineItem2
			}.AppendInsertAndReturnObject(sql);

			var delivery3 = new CYDDelivery(deliveryHeader, "YDL000000000003")
			{
				YDL_YLI_UnitLineItem = unitLineItem3
			}.AppendInsertAndReturnObject(sql);

			var tpu = new CYDTransportationUnit(warehouse, "TPU0000000001")
			{
				YTU_WL_WaitingBayLocation = warehouseLocations[0],
				YTU_GateInTime = new DateTime(2025, 1, 1)
			}.AppendInsertAndReturnObject(sql);

			new CYDYardUnitState(warehouse, warehouseLocations[0], new DateTime(2024, 6, 6, 0, 0, 0), null, loadUser: "", delivery: delivery1, unitID: "CNT00001")
			{
				YUS_YRL_ReceiveLine = receiveAdviceLine1,
				YUS_YTU_ReceiveTransportationUnit = tpu
			}.AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, warehouseLocations[0], null, null, loadUser: "", unloadUser: "", delivery: delivery2, unitID: "GEN00001").AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, warehouseLocations[0], new DateTime(2024, 6, 6, 0, 0, 0), null, loadUser: "", delivery: delivery3, unitID: "GEN00002")
			{
				YUS_YRL_ReceiveLine = receiveAdviceLine2,
				YUS_YTU_ReceiveTransportationUnit = tpu
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, CYDYardUnitStateSchema.Constants.TableName, CYDYardUnitStateSchema.YUS_YLI_UnitLineItem.Name));
			var yardUnits = CYDYardUnitState.ShallowLoadFromDB(TestConnection);

			AssertEquals("There should be three yard units", 3, yardUnits.Length);

			foreach (var yardUnit in yardUnits)
			{
				if (string.Equals(yardUnit.YUS_UnitID, "CNT00001", StringComparison.OrdinalIgnoreCase))
				{
					var unitLine = CYDUnitLineItem.ShallowLoadFromDB(TestConnection).Single(l => l.PK == yardUnit.YUS_YLI_UnitLineItem);
					AssertNotNull(unitLine);
					AssertEquals("CNT", unitLine.YLI_Type);
					AssertEquals(true, unitLine.YLI_IsEmpty);
					AssertEquals((short)1, unitLine.YLI_Quantity);
				}
				else if (string.Equals(yardUnit.YUS_UnitID, "GEN00002", StringComparison.OrdinalIgnoreCase))
				{
					var unitLine = CYDUnitLineItem.ShallowLoadFromDB(TestConnection).Single(l => l.PK == yardUnit.YUS_YLI_UnitLineItem);
					AssertNotNull(unitLine);
					AssertEquals("GEN", unitLine.YLI_Type);
					AssertEquals(false, unitLine.YLI_IsEmpty);
					AssertEquals((short)1, unitLine.YLI_Quantity);
				}
				else
				{
					// No unit line item has been created as it is not unloaded yet.
					AssertNull(yardUnit.YUS_YLI_UnitLineItem);
					AssertEquals("GEN00001", yardUnit.YUS_UnitID);
				}
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateUnitLineItemForCYDYardUnitState();
	}
}
