using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Core.Workflow;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Core.Workflow.Testing
{
	[TestedType(typeof(GetCascadingProcessTasksForMasterShipment))]
	class GetCascadingProcessTasksForMasterShipmentTest : DbCreateScriptTest
	{
		public void TestGetCascadingProcessTasksForMasterShipment()
		{
			var masterShipment = Guid.NewGuid();
			var shipmentPK1 = Guid.NewGuid();
			var shipmentPK2 = Guid.NewGuid();
			var orderPK1 = Guid.NewGuid();
			var orderPK2 = Guid.NewGuid();

			using (var command = TestConnection.Command(@"INSERT INTO dbo.JobShipment (JS_PK, JS_ShipmentType) VALUES (@JS_PK, 'CLD')"))
			{
				command.AddParameterBasedOnDbColumn("@JS_PK", masterShipment, JobShipmentSchema.PK);
				command.ExecuteNonQuery();
			}

			CreateShipmentWithProcessTask(shipmentPK1, masterShipment, "STD", "CID", "S00001");
			CreateShipmentWithProcessTask(shipmentPK2, masterShipment, "STD", "CID", "S00002");

			CreateOrderWithProcessTask(orderPK1, shipmentPK1, "STD", "CID", "P00001");
			CreateOrderWithProcessTask(orderPK2, shipmentPK2, "STD", "CID", "P00002");

			var result = GetResult(masterShipment, "CID");

			AssertEquals(4, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder(new[] { shipmentPK1, shipmentPK2, orderPK1, orderPK2 },
				new[] { (Guid)result.Rows[0]["JobPK"], (Guid)result.Rows[1]["JobPK"], (Guid)result.Rows[2]["JobPK"], (Guid)result.Rows[3]["JobPK"] });
		}

		public void TestGetCascadingProcessTasksForAssemblyMasterShipment()
		{
			var asmMaster = Guid.NewGuid();
			var cldMaster = Guid.NewGuid();
			var shipment = Guid.NewGuid();
			var order = Guid.NewGuid();

			CreateShipmentWithProcessTask(asmMaster, null, "ASM", "CID", "S00001");
			CreateShipmentWithProcessTask(cldMaster, asmMaster, "CLD", "CID", "S00002");
			CreateShipmentWithProcessTask(shipment, cldMaster, "STD", "CID", "S00003");

			CreateOrderWithProcessTask(order, shipment, "STD", "CID", "P00001");

			var result = GetResult(asmMaster, "CID");

			AssertEquals(3, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder("Process tasks from master itself are not included", new[] { cldMaster, shipment, order },
				new[] { (Guid)result.Rows[0]["JobPK"], (Guid)result.Rows[1]["JobPK"], (Guid)result.Rows[2]["JobPK"] });
		}

		DataTable GetResult(Guid shipment, string eventCode)
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC GetCascadingProcessTasksForMasterShipment '{0}', '{1}'", shipment, eventCode));
		}

		void CreateShipmentWithProcessTask(Guid shipmentPK, Guid? masterShipmentPK, string shipmentType, string eventCode, string uniqueConsignRef)
		{
			using (var command1 = TestConnection.Command(@"
					INSERT INTO 
						dbo.JobShipment (
							JS_PK,
							JS_ShipmentType,
							JS_JS_ColoadMasterShipment,
							JS_UniqueConsignRef)
					VALUES (
							@JS_PK,
							@JS_ShipmentType,
							@JS_JS_ColoadMasterShipment,
							@JS_UniqueConsignRef)"))
			{
				command1.AddParameterBasedOnDbColumn("@JS_PK", shipmentPK, JobShipmentSchema.PK);
				command1.AddParameterBasedOnDbColumn("@JS_ShipmentType", shipmentType, JobShipmentSchema.JS_ShipmentType);
				command1.AddParameterBasedOnDbColumn("@JS_JS_ColoadMasterShipment", (object)masterShipmentPK ?? DBNull.Value, JobShipmentSchema.JS_JS_ColoadMasterShipment);
				command1.AddParameterBasedOnDbColumn("@JS_UniqueConsignRef", uniqueConsignRef, JobShipmentSchema.JS_UniqueConsignRef);
				command1.ExecuteNonQuery();
			}

			CreateProcessTask(shipmentPK, eventCode, "JS");
		}

		void CreateOrderWithProcessTask(Guid orderPK, Guid shipmentPK, string orderType, string eventCode, string orderNumber)
		{
			using (var command1 = TestConnection.Command(@"
					INSERT INTO
						dbo.JobOrderHeader (
							JD_PK,
							JD_OrderType,
							JD_OrderNumber,
							JD_JS,
							JD_OA_BuyerAddress)
					SELECT TOP 1
						@JD_PK,
						@JD_OrderType,
						@JD_OrderNumber,
						@JD_JS,
						OA_PK
					FROM dbo.OrgAddress"))
			{
				command1.AddParameterBasedOnDbColumn("@JD_PK", orderPK, JobOrderHeaderSchema.PK);
				command1.AddParameterBasedOnDbColumn("@JD_OrderType", orderType, JobOrderHeaderSchema.JD_OrderType);
				command1.AddParameterBasedOnDbColumn("@JD_OrderNumber", orderNumber, JobOrderHeaderSchema.JD_OrderNumber);
				command1.AddParameterBasedOnDbColumn("@JD_JS", shipmentPK, JobOrderHeaderSchema.JD_JS);
				command1.ExecuteNonQuery();
			}

			CreateProcessTask(orderPK, eventCode, "JD");
		}

		void CreateProcessTask(Guid parentPK, string eventCode, string parentTableCode)
		{
			using (var command = TestConnection.Command(@"
					INSERT INTO 
						dbo.ProcessTasks (
							P9_PK,
							P9_ParentID,
							P9_ParentTableCode,
							P9_RespondToCascadedEvents,
							P9_SE_NKMilestoneEvent)
					VALUES (
							@P9_PK,
							@P9_ParentID,
							@P9_ParentTableCode,
							@P9_RespondToCascadedEvents,
							@P9_SE_NKMilestoneEvent)"))
			{
				command.AddParameterBasedOnDbColumn("@P9_PK", Guid.NewGuid(), ProcessTasksSchema.PK);
				command.AddParameterBasedOnDbColumn("@P9_ParentID", parentPK, ProcessTasksSchema.P9_ParentID);
				command.AddParameterBasedOnDbColumn("@P9_ParentTableCode", parentTableCode, ProcessTasksSchema.P9_ParentTableCode);
				command.AddParameterBasedOnDbColumn("@P9_RespondToCascadedEvents", 1, ProcessTasksSchema.P9_RespondToCascadedEvents);
				command.AddParameterBasedOnDbColumn("@P9_SE_NKMilestoneEvent", eventCode, ProcessTasksSchema.P9_SE_NKMilestoneEvent);
				command.ExecuteNonQuery();
			}
		}
	}
}

