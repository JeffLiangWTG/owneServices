using System;
using System.Linq;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Freight.ContainerYard;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PreUpgrade.Public.Yard
{
	[TestedType(typeof(UpdateInvalidYardUnitStateByUnloadTimeAndLoadTimeChecks))]
	public class UpdateInvalidYardUnitStateByUnloadTimeAndLoadTimeChecksTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();
			DropConstraint(CYDYardUnitStateSchema.Constants.TableName);

			var warehouse = new WhsWarehouse("WH1", "CYD").WithDockDoor(TestConnection);
			var warehouseLocations = warehouse.CreateLocations(sql, 2);

			var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);
			var unitLineItem1 = new CYDUnitLineItem(refContainer).AppendInsertAndReturnObject(sql);
			var unitLineItem2 = new CYDUnitLineItem(refContainer).AppendInsertAndReturnObject(sql);
			var unitLineItem3 = new CYDUnitLineItem(refContainer).AppendInsertAndReturnObject(sql);
			var unitLine1 = new CYDUnitLineItem(refContainer) { YLI_Type = "CNT", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine2 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine3 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine4 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine5 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine6 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine7 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine8 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);

			var receiveAdvice = new CYDReceiveAdvice(warehouse, "YRAJOB001").AppendInsertAndReturnObject(sql);
			var validReceiveLine1 = new CYDReceiveAdviceLine(receiveAdvice) { YRL_YLI_UnitLineItem = unitLine1 }.AppendInsertAndReturnObject(sql);
			var validReceiveLine2 = new CYDReceiveAdviceLine(receiveAdvice) { YRL_YLI_UnitLineItem = unitLine6 }.AppendInsertAndReturnObject(sql);
			var validReceiveLine3 = new CYDReceiveAdviceLine(receiveAdvice) { YRL_YLI_UnitLineItem = unitLine6 }.AppendInsertAndReturnObject(sql);

			var deliveryHeader = new CYDDeliveryHeader(warehouse, "DeliveryJob001").AppendInsertAndReturnObject(sql);
			var delivery1 = new CYDDelivery(deliveryHeader, "YDL00000011") { YDL_YLI_UnitLineItem = unitLine2 }.AppendInsertAndReturnObject(sql);
			var delivery2 = new CYDDelivery(deliveryHeader, "YDL00000012") { YDL_YLI_UnitLineItem = unitLine5 }.AppendInsertAndReturnObject(sql);

			var releaseAdvice = new CYDReleaseAdvice(warehouse, "YREJOB001").AppendInsertAndReturnObject(sql);
			var validreleaseAdviceLine1 = new CYDReleaseAdviceLine(releaseAdvice) { YEL_YLI_UnitLineItem = unitLine3 }.AppendInsertAndReturnObject(sql);
			var validreleaseAdviceLine2 = new CYDReleaseAdviceLine(releaseAdvice) { YEL_YLI_UnitLineItem = unitLine8 }.AppendInsertAndReturnObject(sql);

			var pickupHeader = new CYDPickupHeader(warehouse, "PICKUPJob001").AppendInsertAndReturnObject(sql);
			var pickup1 = new CYDPickup(pickupHeader, "YPL00000011") { YPL_YLI_UnitLineItem = unitLine4 }.AppendInsertAndReturnObject(sql);
			var pickup2 = new CYDPickup(pickupHeader, "YPL00000012") { YPL_YLI_UnitLineItem = unitLine7 }.AppendInsertAndReturnObject(sql);

			var validTransportUnit = new CYDTransportationUnit(warehouse, "TPU000000111").AppendInsertAndReturnObject(sql);

			//This will not trigger transformation.
			yardUnitState1 = new CYDYardUnitState(warehouse, null, loadTime: null, unloadTime: null,
				loadUser: string.Empty, unloadUser: string.Empty, unitLineItem: unitLineItem1)
			{
				YUS_YDL_Delivery = delivery2,
				YUS_YRL_ReceiveLine = validReceiveLine2,
				YUS_YTU_ReceiveTransportationUnit = null,
			}.AppendInsertAndReturnObject(sql);

			//Pickup and Dispatch are not null, will trigger transformation.
			yardUnitState2 = new CYDYardUnitState(warehouse, null, loadTime: new DateTime(2024, 6, 6, 0, 0, 0), unloadTime: new DateTime(2024, 6, 6, 0, 0, 0),
				unloadUser: "BP2", loadUser: "BP2", unitLineItem: unitLineItem2)
			{
				YUS_YTU_ReceiveTransportationUnit = validTransportUnit,
				YUS_YRL_ReceiveLine = validReceiveLine3,
				YUS_YDL_Delivery = delivery2,
				YUS_YEL_ReleaseLine = validreleaseAdviceLine2,
				YUS_YPL_Pickup = null,
				YUS_YTU_DispatchTransportationUnit = null
			}.AppendInsertAndReturnObject(sql);

			//Receive transportation unit is null, will trigger transformation.
			yardUnitState3 = new CYDYardUnitState(warehouse, null, loadTime: new DateTime(2024, 6, 6, 0, 0, 0), unloadTime: new DateTime(2023, 6, 6, 0, 0, 0),
				loadUser: "BP3", unloadUser: "BP4", unitLineItem: unitLineItem3)
			{
				YUS_YPL_Pickup = pickup1,
				YUS_YTU_DispatchTransportationUnit = validTransportUnit,
				YUS_YEL_ReleaseLine = validreleaseAdviceLine1,
				YUS_YDL_Delivery = delivery1,
				YUS_YRL_ReceiveLine = validReceiveLine1,
				YUS_YTU_ReceiveTransportationUnit = null
			}.AppendInsertAndReturnObject(sql);

			//Transformation should not trigger.
			yardUnitState4 = new CYDYardUnitState(warehouse, null, loadTime: new DateTime(2024, 6, 6, 0, 0, 0), unloadTime: new DateTime(2023, 6, 6, 0, 0, 0),
				loadUser: "BP3", unloadUser: "BP4", unitLineItem: unitLineItem3)
			{
				YUS_YPL_Pickup = pickup1,
				YUS_YTU_DispatchTransportationUnit = validTransportUnit,
				YUS_YEL_ReleaseLine = validreleaseAdviceLine1,
				YUS_YDL_Delivery = delivery1,
				YUS_YRL_ReceiveLine = validReceiveLine1,
				YUS_YTU_ReceiveTransportationUnit = validTransportUnit
			}.AppendInsertAndReturnObject(sql);

			var movementHeader = new CYDMovementHeader(warehouse, "MITEST4567").AppendInsertAndReturnObject(sql);
			new CYDMovement(movementHeader, yardUnitState1) { YML_YTU_ToTransportationUnit = validTransportUnit }.AppendInsertAndReturnObject(sql);
			new CYDMovement(movementHeader, yardUnitState2) { YML_YTU_ToTransportationUnit = validTransportUnit }.AppendInsertAndReturnObject(sql);
			new CYDMovement(movementHeader, yardUnitState3) { YML_YTU_FromTransportationUnit = validTransportUnit }.AppendInsertAndReturnObject(sql);
			new CYDMovement(movementHeader, yardUnitState4) { YML_YTU_ToTransportationUnit = validTransportUnit, YML_YTU_FromTransportationUnit = validTransportUnit }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var initialStates = CYDYardUnitState.ShallowLoadFromDB(TestConnection);
			AssertEquals("Pre-condition: Expected three CYDYardUnitStates", 4, initialStates.Length);

			var movements = CYDMovement.ShallowLoadFromDB(TestConnection);
			AssertEquals("Pre-condition: Expected three CYDMovements", 4, movements.Length);
		}

		protected override void AssertTransformationResults()
		{
			var updatedStates = CYDYardUnitState.ShallowLoadFromDB(TestConnection);

			var scenario1 = updatedStates.Single(s => s.PK == yardUnitState1.PK);
			AssertNull("LoadTime should be null", scenario1.YUS_LoadTime);
			AssertNullOrEmpty("LoadUser should be null", scenario1.YUS_GS_NKLoadUser);
			AssertNull("UnloadTime should be null", scenario1.YUS_UnloadTime);
			AssertNull("CurrentYardLocation should be null", scenario1.YUS_WL_CurrentYardLocation);

			var scenario2 = updatedStates.Single(s => s.PK == yardUnitState2.PK);
			AssertNull("UnloadTime should be null", scenario2.YUS_UnloadTime);
			AssertNullOrEmpty("UnloadUser should be null", scenario2.YUS_GS_NKUnloadUser);
			AssertNull("CurrentYardLocation should be null", scenario2.YUS_WL_CurrentYardLocation);
			AssertNullOrEmpty("LoadUser should be null", scenario2.YUS_GS_NKLoadUser);
			AssertNull("LoadTime should be null", scenario2.YUS_LoadTime);

			var scenario3 = updatedStates.Single(s => s.PK == yardUnitState3.PK);
			AssertNull("LoadTime should be null", scenario3.YUS_LoadTime);
			AssertNull("UnLoadTime should be null", scenario3.YUS_UnloadTime);
			AssertNull("Location should be null", scenario3.YUS_WL_CurrentYardLocation);
			AssertNullOrEmpty("LoadUser should be null", scenario3.YUS_GS_NKLoadUser);
			AssertNullOrEmpty("UnloadUser should be null", scenario3.YUS_GS_NKUnloadUser);

			var scenario4 = updatedStates.Single(s => s.PK == yardUnitState4.PK);
			AssertNotNull("LoadTime should not be null", scenario4.YUS_LoadTime);
			AssertNotNull("UnloadTime should not be null", scenario4.YUS_UnloadTime);
			AssertNull("CurrentYardLocation should be null", scenario4.YUS_WL_CurrentYardLocation);
			AssertNotNullOrEmpty("LoadUser should not be null", scenario4.YUS_GS_NKLoadUser);
			AssertNotNullOrEmpty("UnloadUser should not be null", scenario4.YUS_GS_NKUnloadUser);

			//Scenario 2 and 3 should have been affected by transformation.
			var movements = CYDMovement.ShallowLoadFromDB(TestConnection);
			AssertEquals("Expected two CYDMovements to remain", 2, movements.Length);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateInvalidYardUnitStateByUnloadTimeAndLoadTimeChecks();

		void DropConstraint(string tableName)
		{
			var sql = new StringBuilder();
			sql.AppendLine($"ALTER TABLE {tableName} DROP CONSTRAINT Constraint_LoadTimeChecks");
			sql.AppendLine($"ALTER TABLE {tableName} DROP CONSTRAINT Constraint_UnloadTimeChecks");
			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		CYDYardUnitState yardUnitState1;
		CYDYardUnitState yardUnitState2;
		CYDYardUnitState yardUnitState3;
		CYDYardUnitState yardUnitState4;
	}
}
