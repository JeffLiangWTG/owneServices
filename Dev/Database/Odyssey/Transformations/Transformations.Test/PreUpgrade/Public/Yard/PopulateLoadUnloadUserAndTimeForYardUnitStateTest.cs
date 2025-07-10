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
	[TestedType(typeof(PopulateLoadUnloadUserAndTimeForYardUnitState))]
	public class PopulateLoadUnloadUserAndTimeForYardUnitStateTest : DataTransformationTestCase
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
			var receiveAdviceLine4 = new CYDReceiveAdviceLine(receiveAdvice) { YRL_YLI_UnitLineItem = unitLine4 }.AppendInsertAndReturnObject(sql);

			var unitLine5 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine5 = new CYDReceiveAdviceLine(receiveAdvice) { YRL_YLI_UnitLineItem = unitLine5 }.AppendInsertAndReturnObject(sql);

			var unitLine6 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine7 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine8 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine9 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine10 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine11 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var unitLine12 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);

			var releaseAdvice = new CYDReleaseAdvice(warehouse, "JOB001").AppendInsertAndReturnObject(sql);

			var unitLine13 = new CYDUnitLineItem(refContainer) { YLI_Type = "CNT", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var releaseAdviceLine1 = new CYDReleaseAdviceLine(releaseAdvice) { YEL_YLI_UnitLineItem = unitLine1 }.AppendInsertAndReturnObject(sql);

			var unitLine14 = new CYDUnitLineItem(refContainer) { YLI_Type = "BLK", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var releaseAdviceLine2 = new CYDReleaseAdviceLine(releaseAdvice) { YEL_YLI_UnitLineItem = unitLine2 }.AppendInsertAndReturnObject(sql);

			var tpu = new CYDTransportationUnit(warehouse, "TPU0000000001") { YTU_WL_WaitingBayLocation = warehouseLocations[0], YTU_GateInTime = new DateTime(2025, 1, 1) }.AppendInsertAndReturnObject(sql);

			var deliveryHeader = new CYDDeliveryHeader(warehouse, "JOB001").AppendInsertAndReturnObject(sql);

			var delivery1 = new CYDDelivery(deliveryHeader, "YDL000000000001") { YDL_YLI_UnitLineItem = unitLine6 }.AppendInsertAndReturnObject(sql);
			var delivery2 = new CYDDelivery(deliveryHeader, "YDL000000000002") { YDL_YLI_UnitLineItem = unitLine7 }.AppendInsertAndReturnObject(sql);
			var delivery3 = new CYDDelivery(deliveryHeader, "YDL000000000003") { YDL_YLI_UnitLineItem = unitLine8 }.AppendInsertAndReturnObject(sql);
			var delivery4 = new CYDDelivery(deliveryHeader, "YDL000000000004") { YDL_YLI_UnitLineItem = unitLine9 }.AppendInsertAndReturnObject(sql);
			var delivery5 = new CYDDelivery(deliveryHeader, "YDL000000000005") { YDL_YLI_UnitLineItem = unitLine10 }.AppendInsertAndReturnObject(sql);

			var pickupHeader = new CYDPickupHeader(warehouse, "JOB001").AppendInsertAndReturnObject(sql);

			var pickup1 = new CYDPickup(pickupHeader, "YPL000000000001") { YPL_YLI_UnitLineItem = unitLine11 }.AppendInsertAndReturnObject(sql);
			var pickup2 = new CYDPickup(pickupHeader, "YPL000000000002") { YPL_YLI_UnitLineItem = unitLine12 }.AppendInsertAndReturnObject(sql);

			new CYDYardUnitState(warehouse, warehouseLocations[0], new DateTime(2024, 6, 6, 0, 0, 0), null, unloadUser: "", delivery: delivery1, unitLineItem: unitLineItem)
			{
				YUS_YTU_ReceiveTransportationUnit = tpu,
				YUS_YRL_ReceiveLine = receiveAdviceLine1
			}.AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, null, new DateTime(2024, 5, 5, 0, 0, 0), new DateTime(2024, 6, 6, 0, 0, 0), delivery: delivery2, unitLineItem: unitLineItem)
			{
				YUS_YEL_ReleaseLine = releaseAdviceLine1,
				YUS_YPL_Pickup = pickup1,
				YUS_YTU_DispatchTransportationUnit = tpu,
				YUS_YTU_ReceiveTransportationUnit = tpu,
				YUS_YRL_ReceiveLine = receiveAdviceLine2
			}.AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, null, new DateTime(2024, 5, 5, 0, 0, 0), new DateTime(2024, 6, 6, 0, 0, 0), unloadUser: "", loadUser: "", delivery: delivery3, unitLineItem: unitLineItem)
			{
				YUS_YEL_ReleaseLine = releaseAdviceLine2,
				YUS_YPL_Pickup = pickup2,
				YUS_YTU_DispatchTransportationUnit = tpu,
				YUS_YTU_ReceiveTransportationUnit = tpu,
				YUS_YRL_ReceiveLine = receiveAdviceLine3
			}.AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, warehouseLocations[0], new DateTime(2024, 6, 6, 0, 0, 0), null, loadUser: "", delivery: delivery4, unitLineItem: unitLineItem)
			{
				YUS_YTU_ReceiveTransportationUnit = tpu,
				YUS_YRL_ReceiveLine = receiveAdviceLine4
			}.AppendInsertAndReturnObject(sql);
			new CYDYardUnitState(warehouse, warehouseLocations[0], new DateTime(2024, 6, 6, 0, 0, 0), null, delivery: delivery5, unitLineItem: unitLineItem)
			{
				YUS_YTU_ReceiveTransportationUnit = tpu,
				YUS_YRL_ReceiveLine = receiveAdviceLine5
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
			var yardUnitStates = CYDYardUnitState.ShallowLoadFromDB(TestConnection);
			AssertEquals("Pre-condition: Expected there to be two CYDYardUnitStates", 5, yardUnitStates.Length);
		}
		protected override void AssertTransformationResults()
		{
			var yardUnitStates = CYDYardUnitState.ShallowLoadFromDB(TestConnection);
			var yardUnitStateWithCurrentYardLocation = yardUnitStates.Count(y => y.YUS_WL_CurrentYardLocation != null);
			AssertEquals((yardUnitStates.All(y =>
				(y.YUS_LoadTime == null && y.YUS_UnloadTime == null && string.IsNullOrEmpty(y.YUS_GS_NKLoadUser) && string.IsNullOrEmpty(y.YUS_GS_NKUnloadUser)) ||
				(y.YUS_LoadTime == null && y.YUS_UnloadTime != null && string.IsNullOrEmpty(y.YUS_GS_NKLoadUser) && !string.IsNullOrEmpty(y.YUS_GS_NKUnloadUser)) ||
				(y.YUS_LoadTime != null && y.YUS_UnloadTime != null && !string.IsNullOrEmpty(y.YUS_GS_NKLoadUser) && !string.IsNullOrEmpty(y.YUS_GS_NKUnloadUser))
			)), true);
		}

		void DropConstraint(string tableName)
		{
			var sql = new StringBuilder();
			sql.AppendLine($"ALTER TABLE {tableName} DROP CONSTRAINT Constraint_LoadTimeAndLoadUserBothSetOrBothEmpty");
			sql.AppendLine($"ALTER TABLE {tableName} DROP CONSTRAINT Constraint_UnloadTimeAndUnloadUserBothSetOrBothEmpty");
			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateLoadUnloadUserAndTimeForYardUnitState();
	}
}
