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
	[TestedType(typeof(UpdateYUS_WL_CurrentYardLocationAndYUS_UnloadTime))]
	public class UpdateYUS_WL_CurrentYardLocationAndYUS_UnloadTimeTest : DataTransformationTestCase
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

			var unitLine3 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine3 = new CYDReceiveAdviceLine(receiveAdvice) { YRL_YLI_UnitLineItem = unitLine3 }.AppendInsertAndReturnObject(sql);

			var unitLine4 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine5 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine6 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine7 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);

			var releaseAdvice = new CYDReleaseAdvice(warehouse, "JOB001").AppendInsertAndReturnObject(sql);

			var unitLine8 = new CYDUnitLineItem(refContainer) { YLI_Type = "CNT", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var releaseAdviceLine1 = new CYDReleaseAdviceLine(releaseAdvice) { YEL_YLI_UnitLineItem = unitLine8 }.AppendInsertAndReturnObject(sql);

			var tpu = new CYDTransportationUnit(warehouse, "TPU0000000001") { YTU_WL_WaitingBayLocation = warehouseLocations[0], YTU_GateInTime = new DateTime(2025, 1, 1) }.AppendInsertAndReturnObject(sql);

			var deliveryHeader = new CYDDeliveryHeader(warehouse, "JOB001").AppendInsertAndReturnObject(sql);

			var delivery1 = new CYDDelivery(deliveryHeader, "YDL000000000001") { YDL_YLI_UnitLineItem = unitLine4 }.AppendInsertAndReturnObject(sql);
			var delivery2 = new CYDDelivery(deliveryHeader, "YDL000000000002") { YDL_YLI_UnitLineItem = unitLine5 }.AppendInsertAndReturnObject(sql);
			var delivery3 = new CYDDelivery(deliveryHeader, "YDL000000000003") { YDL_YLI_UnitLineItem = unitLine6 }.AppendInsertAndReturnObject(sql);

			var pickupHeader = new CYDPickupHeader(warehouse, "JOB001").AppendInsertAndReturnObject(sql);

			var pickup1 = new CYDPickup(pickupHeader, "YPL000000000001") { YPL_YLI_UnitLineItem = unitLine7 }.AppendInsertAndReturnObject(sql);

			new CYDYardUnitState(warehouse, null, new DateTime(2024, 6, 6, 0, 0, 0), null, "", "~BP", unitLineItem: unitLineItem)
			{
				YUS_YDL_Delivery = delivery1,
				YUS_YTU_ReceiveTransportationUnit = tpu,
				YUS_YRL_ReceiveLine = receiveAdviceLine1
			}.AppendInsertAndReturnObject(sql);

			new CYDYardUnitState(warehouse, warehouseLocations[1], new DateTime(2024, 6, 6, 0, 0, 0), new DateTime(2024, 6, 6, 0, 0, 0), unitLineItem: unitLineItem)
			{
				YUS_YPL_Pickup = pickup1,
				YUS_YEL_ReleaseLine = releaseAdviceLine1,
				YUS_YTU_DispatchTransportationUnit = tpu,
				YUS_YDL_Delivery = delivery2,
				YUS_YTU_ReceiveTransportationUnit = tpu,
				YUS_YRL_ReceiveLine = receiveAdviceLine2
			}.AppendInsertAndReturnObject(sql);

			new CYDYardUnitState(warehouse, warehouseLocations[0], null, null, "", "", unitLineItem: unitLineItem).AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, null, null, null, "", "", unitLineItem: unitLineItem).AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, warehouseLocations[2], new DateTime(2024, 6, 6, 6, 0, 0), null, "", "~BP", unitLineItem: unitLineItem)
			{
				YUS_YDL_Delivery = delivery3,
				YUS_YTU_ReceiveTransportationUnit = tpu,
				YUS_YRL_ReceiveLine = receiveAdviceLine3
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var yardUnitStates = CYDYardUnitState.ShallowLoadFromDB(TestConnection);
			var yardUnitStatesWithCurrentYardLocation = yardUnitStates.Count(y => y.YUS_WL_CurrentYardLocation != null);
			AssertEquals("Pre-condition: Expected there to be five CYDYardUnitStates", 5, yardUnitStates.Length);
			AssertEquals("Pre-condition: Expected there to be three CYDYardUnitStates with CurrentYardLocation", 3, yardUnitStatesWithCurrentYardLocation);
		}

		protected override void AssertTransformationResults()
		{
			var yardUnitStates = CYDYardUnitState.ShallowLoadFromDB(TestConnection);
			var yardUnitStatesWithCurrentYardLocation = yardUnitStates.Count(y => y.YUS_WL_CurrentYardLocation != null);
			AssertEquals("Expected 2 YardUnitStates to have YUS_WL_CurrentYardLocation value", 2, yardUnitStatesWithCurrentYardLocation);
			AssertEquals((yardUnitStates.All(y =>
				(y.YUS_LoadTime == null && y.YUS_UnloadTime == null) ||
				(y.YUS_LoadTime != null && y.YUS_UnloadTime != null && y.YUS_WL_CurrentYardLocation == null) ||
				(y.YUS_LoadTime == null && y.YUS_UnloadTime != null && y.YUS_WL_CurrentYardLocation != null)
			)), true);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateYUS_WL_CurrentYardLocationAndYUS_UnloadTime();

		void DropConstraint(string tableName)
		{
			var sql = new StringBuilder();
			sql.AppendLine($"ALTER TABLE {tableName} DROP CONSTRAINT Constraint_YUS_WL_CurrentYardLocationNotNullForUnloadedNotLoadedUnit");
			TestConnection.ExecuteNonQuery(sql.ToString());
		}
	}
}
