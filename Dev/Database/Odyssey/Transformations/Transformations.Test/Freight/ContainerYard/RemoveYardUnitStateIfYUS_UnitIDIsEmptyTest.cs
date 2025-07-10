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
	[TestedType(typeof(RemoveUnitAndMovementIfYUS_UnitIDIsEmpty))]
	public class RemoveUnitAndMovementIfYUS_UnitIDIsEmptyTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();
			DropConstraint(CYDYardUnitStateSchema.Constants.TableName);

			var warehouse = new WhsWarehouse("WH1", "CYD").WithDockDoor(TestConnection);
			var warehouseLocations = warehouse.CreateLocations(sql, 3);
			var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);
			var unitLineItem = new CYDUnitLineItem(refContainer).AppendInsertAndReturnObject(sql);
			var receiveAdvice = new CYDReceiveAdvice(warehouse, "JOB001").AppendInsertAndReturnObject(sql);

			var unitLine1 = new CYDUnitLineItem(refContainer) { YLI_Type = "CNT", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice) { YRL_YLI_UnitLineItem = unitLine1 }.AppendInsertAndReturnObject(sql);

			var unitLine2 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine2 = new CYDReceiveAdviceLine(receiveAdvice) { YRL_YLI_UnitLineItem = unitLine2 }.AppendInsertAndReturnObject(sql);

			var releaseAdvice = new CYDReleaseAdvice(warehouse, "JOB001").AppendInsertAndReturnObject(sql);

			var unitLine3 = new CYDUnitLineItem(refContainer) { YLI_Type = "CNT", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var releaseAdviceLine1 = new CYDReleaseAdviceLine(releaseAdvice) { YEL_YLI_UnitLineItem = unitLine1 }.AppendInsertAndReturnObject(sql);

			var unitLine4 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var releaseAdviceLine2 = new CYDReleaseAdviceLine(releaseAdvice) { YEL_YLI_UnitLineItem = unitLine2 }.AppendInsertAndReturnObject(sql);

			var unitLine5 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine6 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine7 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine8 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);

			var tpu = new CYDTransportationUnit(warehouse, "TPU0000000001") { YTU_WL_WaitingBayLocation = warehouseLocations[0], YTU_GateInTime = new DateTime(2025, 1, 1) }.AppendInsertAndReturnObject(sql);

			var deliveryHeader = new CYDDeliveryHeader(warehouse, "JOB001").AppendInsertAndReturnObject(sql);

			var delivery1 = new CYDDelivery(deliveryHeader, "YDL000000000001") { YDL_YLI_UnitLineItem = unitLine5 }.AppendInsertAndReturnObject(sql);
			var delivery2 = new CYDDelivery(deliveryHeader, "YDL000000000002") { YDL_YLI_UnitLineItem = unitLine6 }.AppendInsertAndReturnObject(sql);

			var pickupHeader = new CYDPickupHeader(warehouse, "JOB001").AppendInsertAndReturnObject(sql);

			var pickup1 = new CYDPickup(pickupHeader, "YPL000000000001") { YPL_YLI_UnitLineItem = unitLine7 }.AppendInsertAndReturnObject(sql);
			var pickup2 = new CYDPickup(pickupHeader, "YPL000000000002") { YPL_YLI_UnitLineItem = unitLine8 }.AppendInsertAndReturnObject(sql);

			var yardUnitState1 = new CYDYardUnitState(warehouse, warehouseLocations[1], new DateTime(2024, 6, 6, 0, 0, 0), new DateTime(2024, 6, 6, 0, 0, 0), "~BP", "~BP", "", delivery: delivery1, unitLineItem: unitLineItem)
			{
				YUS_YEL_ReleaseLine = releaseAdviceLine1,
				YUS_YPL_Pickup = pickup1,
				YUS_YTU_DispatchTransportationUnit = tpu,
				YUS_YTU_ReceiveTransportationUnit = tpu,
				YUS_YRL_ReceiveLine = receiveAdviceLine1
			}.AppendInsertAndReturnObject(sql);
			var yardUnitState2 = new CYDYardUnitState(warehouse, warehouseLocations[1], new DateTime(2024, 6, 6, 0, 0, 0), new DateTime(2024, 6, 6, 0, 0, 0), delivery: delivery2, unitLineItem: unitLineItem)
			{
				YUS_YEL_ReleaseLine = releaseAdviceLine2,
				YUS_YPL_Pickup = pickup2,
				YUS_YTU_DispatchTransportationUnit = tpu,
				YUS_YTU_ReceiveTransportationUnit = tpu,
				YUS_YRL_ReceiveLine = receiveAdviceLine2
			}.AppendInsertAndReturnObject(sql);

			var movementHeader1 = new CYDMovementHeader(warehouse, "MITEST0999").AppendInsertAndReturnObject(sql);
			var movementHeader2 = new CYDMovementHeader(warehouse, "MITEST0987").AppendInsertAndReturnObject(sql);

			new CYDMovement(movementHeader1, yardUnitState1).AppendInsertAndReturnObject(sql);
			new CYDMovement(movementHeader2, yardUnitState2).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var yardUnitStates = CYDYardUnitState.ShallowLoadFromDB(TestConnection);
			var emptyUnitIdCount = yardUnitStates.Count(y => string.IsNullOrEmpty(y.YUS_UnitID));
			AssertEquals("Pre-condition: Expected there to be one CYDYardUnitState with empty YUS_UnitID", 1, emptyUnitIdCount);
			AssertEquals("Pre-condition: Expected there to be two CYDYardUnitStates", 2, yardUnitStates.Length);

			var movements = CYDMovement.ShallowLoadFromDB(TestConnection);
			AssertEquals("Pre-condition: Expected there to be two CYDMovements", 2, movements.Length);
		}

		protected override void AssertTransformationResults()
		{
			var yardUnitStates = CYDYardUnitState.ShallowLoadFromDB(TestConnection);
			var emptyUnitIdCount = yardUnitStates.Count(y => string.IsNullOrEmpty(y.YUS_UnitID));
			AssertEquals("Expected no CYDYardUnitState with empty YUS_UnitID", 0, emptyUnitIdCount);
			AssertEquals("Expected one CYDYardUnitState to remain", 1, yardUnitStates.Length);
			AssertEquals("Expected the remaining CYDYardUnitState to have YUS_UnitID 'NotEmpty'", "CNTG000001", yardUnitStates.First().YUS_UnitID);

			var movements = CYDMovement.ShallowLoadFromDB(TestConnection);
			AssertEquals("Expected one CYDMovement to remain", 1, movements.Length);
			AssertEquals("Expected the remaining CYDMovement to be associated with the remaining CYDYardUnitState", yardUnitStates.First().PK, movements.First().YML_YUS_YardUnitState.FK);
		}

		void DropConstraint(string tableName)
		{
			var sql = new StringBuilder();
			sql.AppendLine($"ALTER TABLE {tableName} DROP CONSTRAINT Constraint_YUS_WL_CurrentYardLocationNotNullForUnloadedNotLoadedUnit");
			sql.AppendLine($"ALTER TABLE {tableName} DROP CONSTRAINT Constraint_YUS_UnitID");
			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new RemoveUnitAndMovementIfYUS_UnitIDIsEmpty();
	}
}
