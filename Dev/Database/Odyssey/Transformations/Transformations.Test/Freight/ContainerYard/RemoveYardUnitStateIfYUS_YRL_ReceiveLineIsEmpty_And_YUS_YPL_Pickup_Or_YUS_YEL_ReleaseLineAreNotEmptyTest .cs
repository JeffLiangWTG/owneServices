using System;
using System.Linq;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Freight.ContainerYard;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Freight.ContainerYard
{
	[TestedType(typeof(RemoveYardUnitStateIfYUS_YRL_ReceiveLineIsEmpty_And_YUS_YPL_Pickup_Or_YUS_YEL_ReleaseLineAreNotEmpty))]
	public class RemoveYardUnitStateIfYUS_YRL_ReceiveLineIsEmpty_And_YUS_YPL_Pickup_Or_YUS_YEL_ReleaseLineAreNotEmptyTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();
			DropConstraint(CYDYardUnitStateSchema.Constants.TableName);

			var warehouse = new WhsWarehouse("WH2", "CYD").WithDockDoor(TestConnection);
			var warehouseLocations = warehouse.CreateLocations(sql, 3);
			var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);

			var releaseAdvice = new CYDReleaseAdvice(warehouse, "JOB001", fromDate, toDate).AppendInsertAndReturnObject(sql);
			var pickupHeader = new CYDPickupHeader(warehouse, "PICKUP1234").AppendInsertAndReturnObject(sql);
			var unitLine = new CYDUnitLineItem(refContainer) { YLI_Type = "CNT", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var receiveAdvice = new CYDReceiveAdvice(warehouse, "JOB001").AppendInsertAndReturnObject(sql);

			var unitLine1 = new CYDUnitLineItem(refContainer) { YLI_Type = "CNT", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice) { YRL_YLI_UnitLineItem = unitLine1 }.AppendInsertAndReturnObject(sql);

			var unitLine2 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine3 = new CYDUnitLineItem(refContainer) { YLI_Type = "CNT", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine4 = new CYDUnitLineItem(refContainer) { YLI_Type = "CNT", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);

			var releaseAdviceLine1 = new CYDReleaseAdviceLine(releaseAdvice) { YEL_YLI_UnitLineItem = unitLine2 }.AppendInsertAndReturnObject(sql);

			var deliveryHeader = new CYDDeliveryHeader(warehouse, "JOB001").AppendInsertAndReturnObject(sql);

			var delivery1 = new CYDDelivery(deliveryHeader, "YDL000000000001") { YDL_YLI_UnitLineItem = unitLine4 }.AppendInsertAndReturnObject(sql);

			var pickup1 = new CYDPickup(pickupHeader, "YPL000000000001") { YPL_YLI_UnitLineItem = unitLine3 }.AppendInsertAndReturnObject(sql);

			var tpu = new CYDTransportationUnit(warehouse, "TPU0000000001") { YTU_WL_WaitingBayLocation = warehouseLocations[0], YTU_GateInTime = new DateTime(2025, 1, 1) }.AppendInsertAndReturnObject(sql);

			var yardUnitState1 = new CYDYardUnitState(warehouse, null, null, null, "", "", "CNTG000001")
			{
				YUS_YRL_ReceiveLine = null,
				YUS_YEL_ReleaseLine = new CYDReleaseAdviceLine(releaseAdvice, refContainer) { YEL_YLI_UnitLineItem = unitLine }.AppendInsertAndReturnObject(sql),
				YUS_YLI_UnitLineItem = unitLine
			}.AppendInsertAndReturnObject(sql);

			var yardUnitState2 = new CYDYardUnitState(warehouse, null, new DateTime(2024, 6, 6, 0, 0, 0), new DateTime(2024, 6, 6, 0, 0, 0), "~BP", "~BP", "CNTG000002")
			{
				YUS_YPL_Pickup = pickup1,
				YUS_YEL_ReleaseLine = releaseAdviceLine1,
				YUS_YTU_DispatchTransportationUnit = tpu,
				YUS_YTU_ReceiveTransportationUnit = tpu,
				YUS_YDL_Delivery = delivery1,
				YUS_YRL_ReceiveLine = receiveAdviceLine1,
				YUS_GS_NKLoadUser = "Pic",
				YUS_YLI_UnitLineItem = unitLine
			}.AppendInsertAndReturnObject(sql);

			var yardUnitState3 = new CYDYardUnitState(warehouse, null, null, null, "", "", "CNTG000002")
			{
				YUS_YRL_ReceiveLine = null,
				YUS_YLI_UnitLineItem = unitLine,
				YUS_YPL_Pickup = new CYDPickup(pickupHeader, "YPL009", refContainer) { YPL_YLI_UnitLineItem = unitLine }.AppendInsertAndReturnObject(sql),
			}.AppendInsertAndReturnObject(sql);

			var movementHeader = new CYDMovementHeader(warehouse, "MITEST1234").AppendInsertAndReturnObject(sql);
			new CYDMovement(movementHeader, yardUnitState1).AppendInsertAndReturnObject(sql);
			new CYDMovement(movementHeader, yardUnitState2).AppendInsertAndReturnObject(sql);
			new CYDMovement(movementHeader, yardUnitState3).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToString());

			var yardUnitStates = CYDYardUnitState.ShallowLoadFromDB(TestConnection);
			AssertEquals("Pre-condition: Expected three CYDYardUnitStates", 3, yardUnitStates.Length);

			var movements = CYDMovement.ShallowLoadFromDB(TestConnection);
			AssertEquals("Pre-condition: Expected two CYDMovements", 3, movements.Length);
		}

		protected override void AssertTransformationResults()
		{
			var yardUnitStates = CYDYardUnitState.ShallowLoadFromDB(TestConnection);
			AssertEquals("Expected only one CYDYardUnitState to remain", 1, yardUnitStates.Length);

			if (yardUnitStates.First().YUS_YRL_ReceiveLine == null)
			{
				AssertNull("Expected the remaining CYDYardUnitState to have YUS_YEL_ReleaseLine null", yardUnitStates.First().YUS_YEL_ReleaseLine);
				AssertNull("Expected the remaining CYDYardUnitState to have YUS_YPL_Pickup null", yardUnitStates.First().YUS_YPL_Pickup);
			}

			var movements = CYDMovement.ShallowLoadFromDB(TestConnection);
			AssertEquals("Expected one CYDMovements to remain", 1, movements.Length);
		}

		void DropConstraint(string tableName)
		{
			var sql = new StringBuilder();
			sql.AppendLine($"ALTER TABLE {tableName} DROP CONSTRAINT Constraint_YUS_YRL_ReceiveLineNotNullWhenYUS_YEL_ReleaseLineAnd_YUS_YPL_PickupAreNotNull");
			sql.AppendLine($"ALTER TABLE CYDMovement DROP CONSTRAINT CYDMovement_YML_YUS_YardUnitState_FK2_CYDYardUnitState_RRR_120N");
			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override DataTransformation GetNewTestTransformationInstance() =>
			new RemoveYardUnitStateIfYUS_YRL_ReceiveLineIsEmpty_And_YUS_YPL_Pickup_Or_YUS_YEL_ReleaseLineAreNotEmpty();

		readonly DateTime toDate;

		readonly DateTime fromDate;
	}
}
