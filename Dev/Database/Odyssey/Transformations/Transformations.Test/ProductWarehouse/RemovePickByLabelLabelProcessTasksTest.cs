using System;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ProductWarehouse
{
	[TestedType(typeof(RemovePickByLabelLabelProcessTasks))]
	class RemovePickByLabelLabelProcessTasksTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			new ProcessHeader("WPL", pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		protected override void AssertPreConditions()
		{
			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, header => header.FH_WorkflowType == "WPL"));
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(0, ProcessHeader.CountInDB(TestConnection, header => header.FH_WorkflowType == "WPL"));
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskWithProcessHeader()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processHeader = new ProcessHeader("WOU", order.PK, "WD").AppendInsertAndReturnObject(sql);
			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL") { P9_FH_ProcessHeader = processHeader.PK }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, header => header.FH_WorkflowType == "WOU"));

			GetNewTestTransformationInstance().Run();

			AssertEquals("WhsPickByLabelLabel process task is deleted.", 0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals("Non WhsPickByLabelLabel ProcessHeader is not deleted.", 1, ProcessHeader.CountInDB(TestConnection, header => header.FH_WorkflowType == "WOU"));
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessHeaderWithProcessTask()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processHeader = new ProcessHeader("WPL", pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var processTask = new ProcessTasks(order.PK, "WD") { P9_FH_ProcessHeader = processHeader.PK }.AppendInsertAndReturnObject(sql);
			var processTaskLastEditTime = processTask.P9_SystemLastEditTimeUtc;

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WD"));
			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, header => header.FH_WorkflowType == "WPL"));

			GetNewTestTransformationInstance().Run();

			ProcessTasks.ShallowLoadFromDB(TestConnection, task => task.PK == processTask.PK).Single()
				.BuildAssertion(TestConnection)
				.ExpectEquals("P9_FH_ProcessHeader is emptied.", task => task.P9_FH_ProcessHeader, null)
				.ExpectNotEquals("P9_SystemLastEditTimeUtc", task => task.P9_SystemLastEditTimeUtc, processTaskLastEditTime)
				.ExpectEquals("P9_SystemLastEditUser", task => task.P9_SystemLastEditUser, "~BP")
				.VerifyAll();

			AssertEquals("WhsPickByLabelLabel ProcessHeader is deleted.", 0, ProcessHeader.CountInDB(TestConnection, header => header.FH_WorkflowType == "WPL"));
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskReferencedBy_ProcessTaskIterationLink_ContainmentBarrierTask()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var processTaskIterationLink =  new ProcessTaskIterationLink("GRP", processTask.PK, 1).AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, ProcessTaskIterationLink.CountInDB(TestConnection, link => link.P9I_P9_ContainmentBarrierTask == processTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(0, ProcessTaskIterationLink.CountInDB(TestConnection, link => link.PK == processTaskIterationLink.PK));
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskReferencedBy_ProcessTaskIterationLink_ContainmentBarrierTask_ParentProcessTaskIterationLink()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var processTaskIterationLink = new ProcessTaskIterationLink("GRP", processTask.PK, 1).AppendInsertAndReturnObject(sql);
			var nonPickByLabelProcessTask = new ProcessTasks(order.PK, "WD") { P9_FormFlowType = "WPT" }.AppendInsertAndReturnObject(sql);
			var childLink = new ProcessTaskIterationLink("GRP", nonPickByLabelProcessTask.PK, 2) { P9I_P9I_ParentIteration = processTaskIterationLink.PK }.AppendInsertAndReturnObject(sql);
			var childLinkLastEditTime = childLink.P9I_SystemLastEditTimeUtc;

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, ProcessTaskIterationLink.CountInDB(TestConnection, link => link.P9I_P9_ContainmentBarrierTask == processTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(0, ProcessTaskIterationLink.CountInDB(TestConnection, link => link.PK == processTaskIterationLink.PK));
			ProcessTaskIterationLink.ShallowLoadFromDB(base.TestConnection, childLink => childLink.PK == childLink.PK).Single()
				.BuildAssertion(base.TestConnection)
				.ExpectEquals("P9I_P9I_ParentIteration is emptied.", childLink => childLink.P9I_P9I_ParentIteration, null)
				.ExpectNotEquals("P9I_SystemLastEditTimeUtc", childLink => childLink.P9I_SystemLastEditTimeUtc, childLinkLastEditTime)
				.ExpectEquals("P9I_SystemLastEditUser", childLink => childLink.P9I_SystemLastEditUser, "~BP")
				.VerifyAll();
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskReferencedBy_ProcessTaskIterationLink_IterationTask()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var nonPickByLabelProcessTask = new ProcessTasks(order.PK, "WD") { P9_FormFlowType = "WPT" }.AppendInsertAndReturnObject(sql);
			var link = new ProcessTaskIterationLink("GRP", nonPickByLabelProcessTask.PK, 1) { P9I_P9_IterationTask = processTask.PK }.AppendInsertAndReturnObject(sql);
			var linkLastEditTime = link.P9I_SystemLastEditTimeUtc;

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, ProcessTaskIterationLink.CountInDB(TestConnection, link => link.P9I_P9_IterationTask == processTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(0, ProcessTaskIterationLink.CountInDB(TestConnection, link => link.P9I_P9_IterationTask == processTask.PK));
			ProcessTaskIterationLink.ShallowLoadFromDB(TestConnection, link => link.P9I_LinkType == "GRP").Single()
				.BuildAssertion(TestConnection)
				.ExpectEquals("P9I_P9_IterationTask is emptied.", link => link.P9I_P9_IterationTask, null)
				.ExpectNotEquals("P9I_SystemLastEditTimeUtc", link => link.P9I_SystemLastEditTimeUtc, linkLastEditTime)
				.ExpectEquals("P9I_SystemLastEditUser", link => link.P9I_SystemLastEditUser, "~BP")
				.VerifyAll();
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskReferencedBy_ProcessTaskIterationLinkPivot()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var nonPickByLabelProcessTask = new ProcessTasks(order.PK, "WD") { P9_FormFlowType = "WPT" }.AppendInsertAndReturnObject(sql);
			var processTaskIterationLinkNonPickByLabel = new ProcessTaskIterationLink("GRP", nonPickByLabelProcessTask.PK, 1).AppendInsertAndReturnObject(sql);
			var processTaskIterationLinkPivot = new ProcessTaskIterationLinkPivot(order.PK, "WD", processTask.PK, processTaskIterationLinkNonPickByLabel.PK).AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, ProcessTaskIterationLinkPivot.CountInDB(TestConnection, link => link.P9P_P9_Task == processTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(0, ProcessTaskIterationLinkPivot.CountInDB(TestConnection, link => link.PK == processTaskIterationLinkPivot.PK));
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskReferencedBy_ProcessTaskRequiredCapability()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var capability = new GlbCapability("CAP").AppendInsertAndReturnObject(sql);
			var processTaskRequiredCapability = new ProcessTaskRequiredCapability(processTask.PK, capability.PK).AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, ProcessTaskRequiredCapability.CountInDB(TestConnection, link => link.PP_P9_Task == processTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(0, ProcessTaskRequiredCapability.CountInDB(TestConnection, link => link.PK == processTaskRequiredCapability.PK));
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskReferencedBy_ProcessTaskExtraResource()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var processTaskExtraResource = new ProcessTaskExtraResource(processTask.PK).AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, ProcessTaskExtraResource.CountInDB(TestConnection, link => link.PE_P9 == processTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(0, ProcessTaskExtraResource.CountInDB(TestConnection, link => link.PK == processTaskExtraResource.PK));
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskReferencedBy_ProcessTaskNotification()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var processTaskNotification = new ProcessTaskNotification(processTask.PK).AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, ProcessTaskNotification.CountInDB(TestConnection, link => link.PQ_P9 == processTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(0, ProcessTaskNotification.CountInDB(TestConnection, link => link.PK == processTaskNotification.PK));
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskReferencedBy_ProcessTaskRequiredSkill()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var processTaskRequiredSkill = new ProcessTaskRequiredSkill(processTask.PK, "RANDOM").AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, ProcessTaskRequiredSkill.CountInDB(TestConnection, link => link.P9S_P9 == processTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(0, ProcessTaskRequiredSkill.CountInDB(TestConnection, link => link.PK == processTaskRequiredSkill.PK));
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskReferencedBy_ProcessEstimateLog()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var processEstimateLog = new ProcessEstimateLog(processTask.PK, "P9").AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, ProcessEstimateLog.CountInDB(TestConnection, link => link.P9E_ParentId == processTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(0, ProcessEstimateLog.CountInDB(TestConnection, link => link.PK == processEstimateLog.PK));
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskReferencedBy_ProcessTasksSecure()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var processTasksSecure = new ProcessTasksSecure(processTask.PK, "1234567890").AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, ProcessTasksSecure.CountInDB(TestConnection, link => link.P9H_P9_Parent == processTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(0, ProcessTasksSecure.CountInDB(TestConnection, link => link.PK == processTasksSecure.PK));
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskReferencedBy_ProcessWorkflowException()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var processWorkflowException = new ProcessWorkflowException(processTask.PK).AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, ProcessWorkflowException.CountInDB(TestConnection, link => link.WEX_P9_ProcessTask == processTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(0, ProcessWorkflowException.CountInDB(TestConnection, link => link.PK == processWorkflowException.PK));
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskReferencedBy_WhsCycleCountLocation()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var cycleCount = new WhsCycleCountLocation(location.PK, "PWA") { WCL_JobID = "CC1", WCL_P9_Task = processTask.PK }.AppendInsertAndReturnObject(sql);
			var cycleCountLastEditTime = cycleCount.WCL_SystemLastEditTimeUtc;

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, WhsCycleCountLocation.CountInDB(TestConnection, link => link.WCL_P9_Task == processTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			WhsCycleCountLocation.ShallowLoadFromDB(TestConnection, cycleCount => cycleCount.PK == cycleCount.PK).Single()
				.BuildAssertion(TestConnection)
				.ExpectEquals("WCL_P9_Task is emptied.", cycleCount => cycleCount.WCL_P9_Task, null)
				.ExpectNotEquals("WCL_SystemLastEditTimeUtc", cycleCount => cycleCount.WCL_SystemLastEditTimeUtc, cycleCountLastEditTime)
				.ExpectEquals("WCL_SystemLastEditUser", cycleCount => cycleCount.WCL_SystemLastEditUser, "~BP")
				.VerifyAll();
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskReferencedBy_WhsDocket()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);

			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "NEW", "ORD1") { WD_P9_PackingTask = processTask.PK }.AppendInsertAndReturnObject(sql);
			var orderLastEditTime = order2.WD_SystemLastEditTimeUtc;

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, WhsDocket.CountInDB(TestConnection, link => link.WD_P9_PackingTask == processTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			WhsDocket.ShallowLoadFromDB(TestConnection, docket => docket.PK == order2.PK).Single()
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_P9_PackingTask is emptied.", docket => docket.WD_P9_PackingTask, null)
				.ExpectNotEquals("WD_SystemLastEditTimeUtc", docket => docket.WD_SystemLastEditTimeUtc, orderLastEditTime)
				.ExpectEquals("WD_SystemLastEditUser", docket => docket.WD_SystemLastEditUser, "~BP")
				.VerifyAll();
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskReferencedBy_WhsDocketLine()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);
			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);

			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = DateTime.Now, WD_GS_NKFinalizedBy = "Me" }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 20m, location.PK)
			{
				WE_StockOnHand = 0m,
				WE_OriginalInventoryStatus = "AVL",
				WE_UnloadedTime = new DateTimeOffset(DateTime.Now),
				WE_GS_NKUnloadedBy = "Bob"
			}.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "T1") { WD_FinalisedDate = DateTime.Now, WD_GS_NKFinalizedBy = "Me" }.AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 20m, whs.WW_DefaultOutboundDockDoor.FK)
			{
				WE_FinalisedDate = DateTime.Now,
				WE_StockOnHand = 20m,
				WE_WL_TransferFrom = location.PK,
				WE_CurrentInventoryStatus = "AVL",
				WE_OriginalInventoryStatus = "STA",
				WE_DocketLineStatus = "FIN",
				WE_P9_Task = processTask.PK
			}.AppendInsertAndReturnObject(sql);

			var transferLineLastEditTime = transferLine.WE_SystemLastEditTimeUtc;

			new WhsPickLine(receiveLine, transferLine, 20m) { WZ_PickedDateTime = DateTime.Now, WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, WhsDocketLine.CountInDB(TestConnection, link => link.WE_P9_Task == processTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			WhsDocketLine.ShallowLoadFromDB(TestConnection, docketLine => docketLine.PK == transferLine.PK).Single()
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_P9_PackingTask is emptied.", docketLine => docketLine.WE_P9_Task, null)
				.ExpectNotEquals("WD_SystemLastEditTimeUtc", docketLine => docketLine.WE_SystemLastEditTimeUtc, transferLineLastEditTime)
				.ExpectEquals("WD_SystemLastEditUser", docketLine => docketLine.WE_SystemLastEditUser, "~BP")
				.VerifyAll();
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskReferencedBy_WhsPickByLabelJob()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob1 = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob1, package).AppendInsertAndReturnObject(sql);

			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var pickByLabelJob2 = new WhsPickByLabelJob(whs, "B") { WTK_P9_Task = processTask.PK }.AppendInsertAndReturnObject(sql);
			var pickByLabelJobLastEditTime = pickByLabelJob2.WTK_SystemLastEditTimeUtc;

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, WhsPickByLabelJob.CountInDB(TestConnection, link => link.WTK_P9_Task == processTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			WhsPickByLabelJob.ShallowLoadFromDB(TestConnection, pbl => pbl.PK == pickByLabelJob2.PK).Single()
				.BuildAssertion(TestConnection)
				.ExpectEquals("WTK_P9_Task is emptied.", pickByLabelJob => pickByLabelJob.WTK_P9_Task, null)
				.ExpectNotEquals("WTK_SystemLastEditTimeUtc", pickByLabelJob => pickByLabelJob.WTK_SystemLastEditTimeUtc, pickByLabelJobLastEditTime)
				.ExpectEquals("WTK_SystemLastEditUser", pickByLabelJob => pickByLabelJob.WTK_SystemLastEditUser, "~BP")
				.VerifyAll();
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskReferencedBy_WhsPickLine()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var today = DateTime.Today;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, location.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_PalletID = "ABC",
				WE_StockOnHand = 0m,
				WE_UnloadedTime = new DateTimeOffset(today),
				WE_GS_NKUnloadedBy = "A",
				WE_AdjustmentArrivalDate = today,
				WE_SystemCreateTimeUtc = DateTime.UtcNow,
				WE_SystemLastEditTimeUtc = DateTime.UtcNow,
				WE_SystemCreateUser = "A",
				WE_SystemLastEditUser = "A"
			}.AppendInsertAndReturnObject(sql);

			var pickWithOrder = new WhsPick(whs, "P1", "NEW").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1", "O1") { WD_WP = pickWithOrder.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);

			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);

			var pickLine = new WhsPickLine(receiveLine, orderLine, 10m)
			{
				WZ_PickedDateTime = today,
				WZ_GS_NKAssignedTo = "US1",
				WZ_SystemCreateTimeUtc = DateTime.UtcNow,
				WZ_SystemLastEditTimeUtc = DateTime.UtcNow,
				WZ_SystemCreateUser = "A",
				WZ_SystemLastEditUser = "A",
				WZ_P9_Task = processTask.PK
			}.AppendInsertAndReturnObject(sql);

			var pickLineLastEditTime = pickLine.WZ_SystemLastEditTimeUtc;

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, WhsPickLine.CountInDB(TestConnection, link => link.WZ_P9_Task == processTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			WhsPickLine.ShallowLoadFromDB(TestConnection, pickLine => pickLine.PK == pickLine.PK).Single()
				.BuildAssertion(TestConnection)
				.ExpectEquals("WZ_P9_Task is emptied.", pickLine => pickLine.WZ_P9_Task, null)
				.ExpectNotEquals("WZ_SystemLastEditTimeUtc", pickLine => pickLine.WZ_SystemLastEditTimeUtc, pickLineLastEditTime)
				.ExpectEquals("WZ_SystemLastEditUser", pickLine => pickLine.WZ_SystemLastEditUser, "~BP")
				.VerifyAll();
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskReferencedBy_WorkItem_DefectCausedByTask()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var workItem = new WorkItem() { WKI_WorkItemNumber = "WI00000001", WKI_P9_DefectCausedByTask = processTask.PK }.AppendInsertAndReturnObject(sql);
			var workItemLastEditTime = workItem.WKI_SystemLastEditTimeUtc;

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, WorkItem.CountInDB(TestConnection, link => link.WKI_P9_DefectCausedByTask == processTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			WorkItem.ShallowLoadFromDB(TestConnection, workItem => workItem.PK == workItem.PK).Single()
				.BuildAssertion(TestConnection)
				.ExpectEquals("WKI_P9_DefectCausedByTask is emptied.", workItem => workItem.WKI_P9_DefectCausedByTask, null)
				.ExpectNotEquals("WKI_SystemLastEditTimeUtc", workItem => workItem.WKI_SystemLastEditTimeUtc, workItemLastEditTime)
				.ExpectEquals("WKI_SystemLastEditUser", workItem => workItem.WKI_SystemLastEditUser, "~BP")
				.VerifyAll();
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessTaskReferencedBy_WorkItem_DefectFirstMissedInTask()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var workItem = new WorkItem() { WKI_WorkItemNumber = "WI00000001", WKI_P9_DefectFirstMissedInTask = processTask.PK }.AppendInsertAndReturnObject(sql);
			var workItemLastEditTime = workItem.WKI_SystemLastEditTimeUtc;

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1, WorkItem.CountInDB(TestConnection, link => link.WKI_P9_DefectFirstMissedInTask == processTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			WorkItem.ShallowLoadFromDB(TestConnection, workItem => workItem.PK == workItem.PK).Single()
				.BuildAssertion(TestConnection)
				.ExpectEquals("WKI_P9_DefectCausedByTask is emptied.", workItem => workItem.WKI_P9_DefectCausedByTask, null)
				.ExpectNotEquals("WKI_SystemLastEditTimeUtc", workItem => workItem.WKI_SystemLastEditTimeUtc, workItemLastEditTime)
				.ExpectEquals("WKI_SystemLastEditUser", workItem => workItem.WKI_SystemLastEditUser, "~BP")
				.VerifyAll();
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessHeaderReferencedBy_BMReleaseSequenceItem()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processHeader = new ProcessHeader("WPL", pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var releaseGroup = new GlbGroup("GRP").AppendInsertAndReturnObject(sql);
			var bmReleaseSequence = new BMReleaseSequence("DEF", releaseGroup.PK).AppendInsertAndReturnObject(sql);
			var bmReleaseSequenceItem = new BMReleaseSequenceItem(processHeader.PK, bmReleaseSequence.PK).AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.FH_WorkflowType == "WPL"));
			AssertEquals(1, BMReleaseSequenceItem.CountInDB(TestConnection, link => link.BMI_FH_ProcessHeader == processHeader.PK));
			AssertEquals(1, BMReleaseSequence.CountInDB(TestConnection, seq => seq.BMR_Name == "DEF"));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.FH_WorkflowType == "WPL"));
			AssertEquals(0, BMReleaseSequenceItem.CountInDB(TestConnection, link => link.PK == bmReleaseSequenceItem.PK));
			AssertEquals(1, BMReleaseSequence.CountInDB(TestConnection, seq => seq.BMR_Name == "DEF"));
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessHeaderReferencedBy_ProcessHeader()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processHeader = new ProcessHeader("WPL", pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var processHeaderChild = new ProcessHeader("WOU", order.PK, "WD") { FH_FH_ParentHeader = processHeader.PK }.AppendInsertAndReturnObject(sql);
			var processHeaderChildLastEditTime = processHeaderChild.FH_SystemLastEditTimeUtc;

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.FH_WorkflowType == "WPL"));
			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, link => link.FH_FH_ParentHeader == processHeader.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.FH_WorkflowType == "WPL"));
			ProcessHeader.ShallowLoadFromDB(TestConnection, processHeader => processHeader.PK == processHeaderChild.PK).Single()
				.BuildAssertion(TestConnection)
				.ExpectEquals("FH_FH_ParentHeader is emptied.", processHeader => processHeader.FH_FH_ParentHeader, null)
				.ExpectNotEquals("FH_SystemLastEditTimeUtc", processHeader => processHeader.FH_SystemLastEditTimeUtc, processHeaderChildLastEditTime)
				.ExpectEquals("FH_SystemLastEditUser", processHeader => processHeader.FH_SystemLastEditUser, "~BP")
				.VerifyAll();
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessHeaderReferencedBy_ProcessTaskIterationLink()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processHeader = new ProcessHeader("WPL", pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var nonPickByLabelProcessTask = new ProcessTasks(order.PK, "WD") { P9_FormFlowType = "WPT" }.AppendInsertAndReturnObject(sql);
			var processTaskIterationLink = new ProcessTaskIterationLink("GRP", nonPickByLabelProcessTask.PK, 1) { P9I_FH_IterationWorkflow = processHeader.PK }.AppendInsertAndReturnObject(sql);
			var processTaskIterationLinkLastEditTime = processTaskIterationLink.P9I_SystemLastEditTimeUtc;

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.FH_WorkflowType == "WPL"));
			AssertEquals(1, ProcessTaskIterationLink.CountInDB(TestConnection, link => link.P9I_FH_IterationWorkflow == processHeader.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.FH_WorkflowType == "WPL"));
			ProcessTaskIterationLink.ShallowLoadFromDB(TestConnection, link => link.PK == processTaskIterationLink.PK).Single()
				.BuildAssertion(TestConnection)
				.ExpectEquals("P9I_FH_IterationWorkflow is emptied.", link => link.P9I_FH_IterationWorkflow, null)
				.ExpectNotEquals("P9I_SystemLastEditTimeUtc", link => link.P9I_SystemLastEditTimeUtc, processTaskIterationLinkLastEditTime)
				.ExpectEquals("P9I_SystemLastEditUser", link => link.P9I_SystemLastEditUser, "~BP")
				.VerifyAll();
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessHeaderReferencedBy_ProcessHeaderLink_ProcessHeaderTo()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processHeaderPickByLabel = new ProcessHeader("WPL", pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var processHeaderFrom = new ProcessHeader("WOU", order.PK, "WD").AppendInsertAndReturnObject(sql);
			var processHeaderLink = new ProcessHeaderLink("GRP", processHeaderFrom.PK, processHeaderPickByLabel.PK).AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.FH_WorkflowType == "WPL"));
			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.PK == processHeaderFrom.PK));
			AssertEquals(1, ProcessHeaderLink.CountInDB(TestConnection, processHeader => processHeader.FP_FH_HeaderTo == processHeaderPickByLabel.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.PK == processHeaderPickByLabel.PK));
			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.PK == processHeaderFrom.PK));
			AssertEquals(0, ProcessHeaderLink.CountInDB(base.TestConnection, processHeaderLink => processHeaderLink.PK == processHeaderLink.PK));
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessHeaderReferencedBy_ProcessHeaderLink_ProcessHeaderFrom()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processHeaderPickByLabel = new ProcessHeader("WPL", pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var processHeaderTo = new ProcessHeader("WOU", order.PK, "WD").AppendInsertAndReturnObject(sql);
			var processHeaderLink = new ProcessHeaderLink("GRP", processHeaderPickByLabel.PK, processHeaderTo.PK).AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.FH_WorkflowType == "WPL"));
			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.PK == processHeaderTo.PK));
			AssertEquals(1, ProcessHeaderLink.CountInDB(TestConnection, processHeader => processHeader.FP_FH_HeaderFrom == processHeaderPickByLabel.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.PK == processHeaderPickByLabel.PK));
			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.PK == processHeaderTo.PK));
			AssertEquals(0, ProcessHeaderLink.CountInDB(TestConnection, processHeader => processHeader.PK == processHeaderLink.PK));
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessHeaderReferencedBy_ProcessHeaderLink_ReferencedBy_ProcessHeaderLink()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processHeaderPickByLabel = new ProcessHeader("WPL", pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var processHeaderTo = new ProcessHeader("WOU", order.PK, "WD").AppendInsertAndReturnObject(sql);
			var processHeaderLink = new ProcessHeaderLink("GRP", processHeaderPickByLabel.PK, processHeaderTo.PK).AppendInsertAndReturnObject(sql);
			var bmcnShape = new BMNCNShape("ABC").AppendInsertAndReturnObject(sql);
			var bmncnAttachment = new BMNCNAttachment(bmcnShape.PK, bmcnShape.PK) { BNA_FP_ProcessHeaderLink = processHeaderLink.PK }.AppendInsertAndReturnObject(sql);
			var bmncnAttachmentLastEditTime = bmncnAttachment.BNA_SystemLastEditTimeUtc;

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.FH_WorkflowType == "WPL"));
			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.PK == processHeaderTo.PK));
			AssertEquals(1, ProcessHeaderLink.CountInDB(TestConnection, link => link.FP_FH_HeaderFrom == processHeaderPickByLabel.PK));
			AssertEquals(1, BMNCNAttachment.CountInDB(TestConnection, bmncnAttachment => bmncnAttachment.BNA_FP_ProcessHeaderLink == processHeaderLink.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.FH_WorkflowType == "WPL"));
			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.PK == processHeaderTo.PK));
			AssertEquals(0, ProcessHeaderLink.CountInDB(TestConnection, link => link.FP_FH_HeaderFrom == processHeaderPickByLabel.PK));
			BMNCNAttachment.ShallowLoadFromDB(TestConnection, attachment => attachment.PK == bmncnAttachment.PK).Single()
				.BuildAssertion(TestConnection)
				.ExpectEquals("BNA_FP_ProcessHeaderLink is emptied.", attachment => attachment.BNA_FP_ProcessHeaderLink, null)
				.ExpectNotEquals("BNA_SystemLastEditTimeUtc", attachment => attachment.BNA_SystemLastEditTimeUtc, bmncnAttachmentLastEditTime)
				.ExpectEquals("BNA_SystemLastEditUser", attachment => attachment.BNA_SystemLastEditUser, "~BP")
				.VerifyAll();
		}

		public void TestRemovePickByLabelLabelProcessTasks_PickByLabelProcessHeaderReferencedBy_ProcessEstimateLog()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var package = SetupAndReturnPackage(sql, order);
			var pickByLabelJob = new WhsPickByLabelJob(whs, "A").AppendInsertAndReturnObject(sql);
			var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

			var processHeaderPickByLabel = new ProcessHeader("WPL", pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			var processEstimateLog = new ProcessEstimateLog(processHeaderPickByLabel.PK, "FH").AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.FH_WorkflowType == "WPL"));
			AssertEquals(1, ProcessEstimateLog.CountInDB(TestConnection, processEstimateLog => processEstimateLog.P9E_ParentId == processHeaderPickByLabel.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.FH_WorkflowType == "WPL"));
			AssertEquals(0, ProcessEstimateLog.CountInDB(TestConnection, processEstimateLog => processEstimateLog.PK == processEstimateLog.PK));
		}

		public void TestRemovePickByLabelLabelProcessTasks_NonPickByLabelProcessTaskReferencedByOtherTables()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var today = DateTime.Today;
			var receive1 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive1, product.PK, 10m, location.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_PalletID = "ABC",
				WE_StockOnHand = 0m,
				WE_UnloadedTime = new DateTimeOffset(today),
				WE_GS_NKUnloadedBy = "A",
				WE_AdjustmentArrivalDate = today,
				WE_SystemCreateTimeUtc = DateTime.UtcNow,
				WE_SystemLastEditTimeUtc = DateTime.UtcNow,
				WE_SystemCreateUser = "A",
				WE_SystemLastEditUser = "A"
			}.AppendInsertAndReturnObject(sql);

			var pickWithOrder = new WhsPick(whs, "P1", "NEW").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "O1", "O1") { WD_WP = pickWithOrder.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);

			var nonPickByLabelProcessTask = new ProcessTasks(order.PK, "WD") { P9_FormFlowType = "WPT" }.AppendInsertAndReturnObject(sql);

			new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "NEW", "ORD2") { WD_P9_PackingTask = nonPickByLabelProcessTask.PK }.AppendInsertAndReturnObject(sql);
			var pickLine = new WhsPickLine(receiveLine1, orderLine, 10m)
			{
				WZ_PickedDateTime = today,
				WZ_GS_NKAssignedTo = "US1",
				WZ_SystemCreateTimeUtc = DateTime.UtcNow,
				WZ_SystemLastEditTimeUtc = DateTime.UtcNow,
				WZ_SystemCreateUser = "A",
				WZ_SystemLastEditUser = "A",
				WZ_P9_Task = nonPickByLabelProcessTask.PK
			}.AppendInsertAndReturnObject(sql);

			var processTaskIterationLink = new ProcessTaskIterationLink("GRP", nonPickByLabelProcessTask.PK, 2).AppendInsertAndReturnObject(sql);
			var processTaskIterationLinkPivot = new ProcessTaskIterationLinkPivot(order.PK, "WD", nonPickByLabelProcessTask.PK, processTaskIterationLink.PK).AppendInsertAndReturnObject(sql);
			var capability = new GlbCapability("CAP").AppendInsertAndReturnObject(sql);
			var processTaskRequiredCapability = new ProcessTaskRequiredCapability(nonPickByLabelProcessTask.PK, capability.PK).AppendInsertAndReturnObject(sql);
			var processTaskExtraResource = new ProcessTaskExtraResource(nonPickByLabelProcessTask.PK).AppendInsertAndReturnObject(sql);
			var processTaskNotification = new ProcessTaskNotification(nonPickByLabelProcessTask.PK).AppendInsertAndReturnObject(sql);
			var processTaskRequiredSkill = new ProcessTaskRequiredSkill(nonPickByLabelProcessTask.PK, "RANDOM").AppendInsertAndReturnObject(sql);
			var processEstimateLog = new ProcessEstimateLog(nonPickByLabelProcessTask.PK, "P9").AppendInsertAndReturnObject(sql);
			var processTasksSecure = new ProcessTasksSecure(nonPickByLabelProcessTask.PK, "1234567890").AppendInsertAndReturnObject(sql);
			var processWorkflowException = new ProcessWorkflowException(nonPickByLabelProcessTask.PK).AppendInsertAndReturnObject(sql);

			new WhsCycleCountLocation(location.PK, "PWA") { WCL_JobID = "CC1", WCL_P9_Task = nonPickByLabelProcessTask.PK }.AppendInsertAndReturnObject(sql);

			var receive2 = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = DateTime.Now, WD_GS_NKFinalizedBy = "Me" }.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive2, product.PK, 20m, location.PK)
			{
				WE_StockOnHand = 0m,
				WE_OriginalInventoryStatus = "AVL",
				WE_UnloadedTime = new DateTimeOffset(DateTime.Now),
				WE_GS_NKUnloadedBy = "Bob"
			}.AppendInsertAndReturnObject(sql);

			var transfer = new WhsDocket(client.PK, whs.PK, "TFR", "TFR", "FIN", "T1") { WD_FinalisedDate = DateTime.Now, WD_GS_NKFinalizedBy = "Me" }.AppendInsertAndReturnObject(sql);
			var transferLine = new WhsDocketLine(transfer, product.PK, 20m, whs.WW_DefaultOutboundDockDoor.FK)
			{
				WE_FinalisedDate = DateTime.Now,
				WE_StockOnHand = 20m,
				WE_WL_TransferFrom = location.PK,
				WE_CurrentInventoryStatus = "AVL",
				WE_OriginalInventoryStatus = "STA",
				WE_DocketLineStatus = "FIN",
				WE_P9_Task = nonPickByLabelProcessTask.PK
			}.AppendInsertAndReturnObject(sql);
			new WhsPickLine(receiveLine2, transferLine, 20m) { WZ_PickedDateTime = DateTime.Now, WZ_GS_NKAssignedTo = "A" }.AppendInsertAndReturnObject(sql);

			new WhsPickByLabelJob(whs, "A") { WTK_P9_Task = nonPickByLabelProcessTask.PK }.AppendInsertAndReturnObject(sql);
			new WorkItem() { WKI_WorkItemNumber = "WI00000001", WKI_P9_DefectCausedByTask = nonPickByLabelProcessTask.PK, WKI_P9_DefectFirstMissedInTask = nonPickByLabelProcessTask.PK }.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.PK == nonPickByLabelProcessTask.PK));
			AssertEquals(1, ProcessTaskIterationLink.CountInDB(TestConnection, link => link.P9I_P9_ContainmentBarrierTask == nonPickByLabelProcessTask.PK));
			AssertEquals(1, ProcessTaskIterationLinkPivot.CountInDB(TestConnection, linkPivot => linkPivot.P9P_P9_Task == nonPickByLabelProcessTask.PK));
			AssertEquals(1, ProcessTaskRequiredCapability.CountInDB(TestConnection, requiredCapability => requiredCapability.PP_P9_Task == nonPickByLabelProcessTask.PK));
			AssertEquals(1, ProcessTaskExtraResource.CountInDB(TestConnection, extraResource => extraResource.PE_P9 == nonPickByLabelProcessTask.PK));
			AssertEquals(1, ProcessTaskNotification.CountInDB(TestConnection, notification => notification.PQ_P9 == nonPickByLabelProcessTask.PK));
			AssertEquals(1, ProcessTaskRequiredSkill.CountInDB(TestConnection, requiredSkill => requiredSkill.P9S_P9 == nonPickByLabelProcessTask.PK));
			AssertEquals(1, ProcessEstimateLog.CountInDB(TestConnection, estimateLog => estimateLog.P9E_ParentId == nonPickByLabelProcessTask.PK));
			AssertEquals(1, ProcessTasksSecure.CountInDB(TestConnection, tasksSecure => tasksSecure.P9H_P9_Parent == nonPickByLabelProcessTask.PK));
			AssertEquals(1, ProcessWorkflowException.CountInDB(TestConnection, workFlowException => workFlowException.WEX_P9_ProcessTask == nonPickByLabelProcessTask.PK));
			AssertEquals(1, WhsDocket.CountInDB(TestConnection, docket => docket.WD_P9_PackingTask == nonPickByLabelProcessTask.PK));
			AssertEquals(1, WhsDocketLine.CountInDB(TestConnection, docketLine => docketLine.WE_P9_Task == nonPickByLabelProcessTask.PK));
			AssertEquals(1, WhsCycleCountLocation.CountInDB(TestConnection, cycleCount => cycleCount.WCL_P9_Task == nonPickByLabelProcessTask.PK));
			AssertEquals(1, WhsPickByLabelJob.CountInDB(TestConnection, pickByLabelJob => pickByLabelJob.WTK_P9_Task == nonPickByLabelProcessTask.PK));
			AssertEquals(1, WhsPickLine.CountInDB(TestConnection, pickLine => pickLine.WZ_P9_Task == nonPickByLabelProcessTask.PK));
			AssertEquals(1, WorkItem.CountInDB(TestConnection, workItem => workItem.WKI_P9_DefectFirstMissedInTask == nonPickByLabelProcessTask.PK));
			AssertEquals(1, WorkItem.CountInDB(TestConnection, workItem => workItem.WKI_P9_DefectCausedByTask == nonPickByLabelProcessTask.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(1, ProcessTasks.CountInDB(TestConnection, task => task.PK == nonPickByLabelProcessTask.PK));
			AssertEquals(1, ProcessTaskIterationLink.CountInDB(TestConnection, link => link.P9I_P9_ContainmentBarrierTask == nonPickByLabelProcessTask.PK));
			AssertEquals(1, ProcessTaskIterationLinkPivot.CountInDB(TestConnection, linkPivot => linkPivot.P9P_P9_Task == nonPickByLabelProcessTask.PK));
			AssertEquals(1, ProcessTaskRequiredCapability.CountInDB(TestConnection, requiredCapability => requiredCapability.PP_P9_Task == nonPickByLabelProcessTask.PK));
			AssertEquals(1, ProcessTaskExtraResource.CountInDB(TestConnection, extraResource => extraResource.PE_P9 == nonPickByLabelProcessTask.PK));
			AssertEquals(1, ProcessTaskNotification.CountInDB(TestConnection, notification => notification.PQ_P9 == nonPickByLabelProcessTask.PK));
			AssertEquals(1, ProcessTaskRequiredSkill.CountInDB(TestConnection, requiredSkill => requiredSkill.P9S_P9 == nonPickByLabelProcessTask.PK));
			AssertEquals(1, ProcessEstimateLog.CountInDB(TestConnection, estimateLog => estimateLog.P9E_ParentId == nonPickByLabelProcessTask.PK));
			AssertEquals(1, ProcessTasksSecure.CountInDB(TestConnection, tasksSecure => tasksSecure.P9H_P9_Parent == nonPickByLabelProcessTask.PK));
			AssertEquals(1, ProcessWorkflowException.CountInDB(TestConnection, workFlowException => workFlowException.WEX_P9_ProcessTask == nonPickByLabelProcessTask.PK));
			AssertEquals(1, WhsDocket.CountInDB(TestConnection, docket => docket.WD_P9_PackingTask == nonPickByLabelProcessTask.PK));
			AssertEquals(1, WhsDocketLine.CountInDB(TestConnection, docketLine => docketLine.WE_P9_Task == nonPickByLabelProcessTask.PK));
			AssertEquals(1, WhsCycleCountLocation.CountInDB(TestConnection, cycleCount => cycleCount.WCL_P9_Task == nonPickByLabelProcessTask.PK));
			AssertEquals(1, WhsPickByLabelJob.CountInDB(TestConnection, pickByLabelJob => pickByLabelJob.WTK_P9_Task == nonPickByLabelProcessTask.PK));
			AssertEquals(1, WhsPickLine.CountInDB(TestConnection, pickLine => pickLine.WZ_P9_Task == nonPickByLabelProcessTask.PK));
			AssertEquals(1, WorkItem.CountInDB(TestConnection, workItem => workItem.WKI_P9_DefectFirstMissedInTask == nonPickByLabelProcessTask.PK));
			AssertEquals(1, WorkItem.CountInDB(TestConnection, workItem => workItem.WKI_P9_DefectCausedByTask == nonPickByLabelProcessTask.PK));
		}

		public void TestRemovePickByLabelLabelProcessTasks_NonPickByLabelProcessHeaderReferencedByOtherTables()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			var order1 = SetupAndReturnWarehouseOrder(sql, whs, location, client, product);
			var nonPickByLabelProcessHeader1 = new ProcessHeader("WPL", order1.PK, "WTL").AppendInsertAndReturnObject(sql);
			var releaseGroup = new GlbGroup("GRP").AppendInsertAndReturnObject(sql);
			var bmReleaseSequence = new BMReleaseSequence("DEF", releaseGroup.PK).AppendInsertAndReturnObject(sql);
			new BMReleaseSequenceItem(nonPickByLabelProcessHeader1.PK, bmReleaseSequence.PK).AppendInsertAndReturnObject(sql);

			new ProcessHeader("WOU", order1.PK, "WD") { FH_FH_ParentHeader = nonPickByLabelProcessHeader1.PK }.AppendInsertAndReturnObject(sql);
			
			var nonPickByLabelProcessTask = new ProcessTasks(order1.PK, "WD") { P9_FormFlowType = "WPT" }.AppendInsertAndReturnObject(sql);
			new ProcessTaskIterationLink("GRP", nonPickByLabelProcessTask.PK, 1) { P9I_FH_IterationWorkflow = nonPickByLabelProcessHeader1.PK }.AppendInsertAndReturnObject(sql);

			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "NEW", "O2", "O2").AppendInsertAndReturnObject(sql);
			var nonPickByLabelProcessHeader2 = new ProcessHeader("WOU", order2.PK, "WD").AppendInsertAndReturnObject(sql);
			var processHeaderLink1 = new ProcessHeaderLink("GRP", nonPickByLabelProcessHeader2.PK, nonPickByLabelProcessHeader1.PK).AppendInsertAndReturnObject(sql);

			var order3 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "NEW", "O3", "O3").AppendInsertAndReturnObject(sql);
			var nonPickByLabelProcessHeader3 = new ProcessHeader("WOU", order3.PK, "WD").AppendInsertAndReturnObject(sql);
			new ProcessHeaderLink("GRP", nonPickByLabelProcessHeader1.PK, nonPickByLabelProcessHeader3.PK).AppendInsertAndReturnObject(sql);

			var bmcnShape = new BMNCNShape("ABC").AppendInsertAndReturnObject(sql);
			new BMNCNAttachment(bmcnShape.PK, bmcnShape.PK) { BNA_FP_ProcessHeaderLink = processHeaderLink1.PK }.AppendInsertAndReturnObject(sql);

			var processEstimateLog = new ProcessEstimateLog(nonPickByLabelProcessHeader1.PK, "FH").AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.PK == nonPickByLabelProcessHeader1.PK));
			AssertEquals(1, BMReleaseSequenceItem.CountInDB(TestConnection, link => link.BMI_FH_ProcessHeader == nonPickByLabelProcessHeader1.PK));
			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, link => link.FH_FH_ParentHeader == nonPickByLabelProcessHeader1.PK));
			AssertEquals(1, ProcessTaskIterationLink.CountInDB(TestConnection, link => link.P9I_FH_IterationWorkflow == nonPickByLabelProcessHeader1.PK));
			AssertEquals(1, ProcessHeaderLink.CountInDB(TestConnection, link => link.FP_FH_HeaderTo == nonPickByLabelProcessHeader1.PK));
			AssertEquals(1, ProcessHeaderLink.CountInDB(TestConnection, link => link.FP_FH_HeaderTo == nonPickByLabelProcessHeader3.PK));
			AssertEquals(1, ProcessHeaderLink.CountInDB(TestConnection, link => link.FP_FH_HeaderFrom == nonPickByLabelProcessHeader1.PK));
			AssertEquals(1, ProcessHeaderLink.CountInDB(TestConnection, link => link.FP_FH_HeaderFrom == nonPickByLabelProcessHeader2.PK));
			AssertEquals(1, BMNCNAttachment.CountInDB(TestConnection, link => link.BNA_FP_ProcessHeaderLink == processHeaderLink1.PK));
			AssertEquals(1, ProcessEstimateLog.CountInDB(TestConnection, link => link.P9E_ParentId == nonPickByLabelProcessHeader1.PK));

			GetNewTestTransformationInstance().Run();

			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, processHeader => processHeader.PK == nonPickByLabelProcessHeader1.PK));
			AssertEquals(1, BMReleaseSequenceItem.CountInDB(TestConnection, link => link.BMI_FH_ProcessHeader == nonPickByLabelProcessHeader1.PK));
			AssertEquals(1, ProcessHeader.CountInDB(TestConnection, link => link.FH_FH_ParentHeader == nonPickByLabelProcessHeader1.PK));
			AssertEquals(1, ProcessTaskIterationLink.CountInDB(TestConnection, link => link.P9I_FH_IterationWorkflow == nonPickByLabelProcessHeader1.PK));
			AssertEquals(1, ProcessHeaderLink.CountInDB(TestConnection, link => link.FP_FH_HeaderTo == nonPickByLabelProcessHeader1.PK));
			AssertEquals(1, ProcessHeaderLink.CountInDB(TestConnection, link => link.FP_FH_HeaderTo == nonPickByLabelProcessHeader3.PK));
			AssertEquals(1, ProcessHeaderLink.CountInDB(TestConnection, link => link.FP_FH_HeaderFrom == nonPickByLabelProcessHeader1.PK));
			AssertEquals(1, ProcessHeaderLink.CountInDB(TestConnection, link => link.FP_FH_HeaderFrom == nonPickByLabelProcessHeader2.PK));
			AssertEquals(1, BMNCNAttachment.CountInDB(TestConnection, link => link.BNA_FP_ProcessHeaderLink == processHeaderLink1.PK));
			AssertEquals(1, ProcessEstimateLog.CountInDB(TestConnection, link => link.P9E_ParentId == nonPickByLabelProcessHeader1.PK));
		}

		public void TestOnlinePostUpgradeTransformation_Batching()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			BatchInsertData(1005);

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(1005, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(1005, ProcessHeader.CountInDB(TestConnection, header => header.FH_ParentTableCode == "WTL"));

			GetNewTestTransformationInstance().Run();

			AssertEquals(0, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(0, ProcessHeader.CountInDB(TestConnection, header => header.FH_ParentTableCode == "WTL"));

			void BatchInsertData(int batchSize)
			{
				var receives = new WhsDocket[batchSize];
				var receiveLines = new WhsDocketLine[batchSize];
				var picks = new WhsPick[batchSize];
				var orders = new WhsDocket[batchSize];
				var orderLines = new WhsDocketLine[batchSize];
				var pickLines = new WhsPickLine[batchSize];
				var packageJobs = new PkgPackageJob[batchSize];
				var packageHeaders = new PkgPackageHeader[batchSize];
				var packages = new PkgPackage[batchSize];
				var pickByLabelJobs = new WhsPickByLabelJob[batchSize];
				var pickByLabelLabels = new WhsPickByLabelLabel[batchSize];
				var processTasks = new ProcessTasks[batchSize];
				var processHeaders = new ProcessHeader[batchSize];

				var today = DateTime.Today;
				for (var i = 0; i < 1005; i++)
				{
					var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", $"R{i}") { WD_FinalisedDate = today };
					receives[i] = receive;

					var receiveLine = new WhsDocketLine(receive, product.PK, 10m, location.PK)
					{
						WE_OriginalInventoryStatus = "AVL",
						WE_CurrentInventoryStatus = "AVL",
						WE_PalletID = $"ABC{i}",
						WE_StockOnHand = 0m,
						WE_UnloadedTime = new DateTimeOffset(today),
						WE_GS_NKUnloadedBy = "A",
						WE_AdjustmentArrivalDate = today,
						WE_SystemCreateTimeUtc = DateTime.UtcNow,
						WE_SystemLastEditTimeUtc = DateTime.UtcNow,
						WE_SystemCreateUser = "A",
						WE_SystemLastEditUser = "A"
					};
					receiveLines[i] = receiveLine;

					var pickWithOrder = new WhsPick(whs, $"P{i}", "NEW");
					picks[i] = pickWithOrder;

					var orderId = $"O{i}";
					var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", orderId, orderId) { WD_WP = pickWithOrder.PK };
					orders[i] = order;

					var orderLine = new WhsDocketLine(order, product.PK, 10m);
					orderLines[i] = orderLine;

					var pickLine = new WhsPickLine(receiveLine, orderLine, 10m)
					{
						WZ_PickedDateTime = today,
						WZ_GS_NKAssignedTo = "US1",
						WZ_SystemCreateTimeUtc = DateTime.UtcNow,
						WZ_SystemLastEditTimeUtc = DateTime.UtcNow,
						WZ_SystemCreateUser = "A",
						WZ_SystemLastEditUser = "A"
					};
					pickLines[i] = pickLine;

					var packageJob = new PkgPackageJob(order.PK) { KJ_ParentTableCode = "WD", KJ_JobID = $"JOB{i}" };
					packageJobs[i] = packageJob;

					var packageHeader = new PkgPackageHeader($"PKG{i}", DateTime.UtcNow, "~BP");
					packageHeaders[i] = packageHeader;

					var package = new PkgPackage(packageJob, "CNT", 1) { KP_KPH_PackageHeader = packageHeader };
					packages[i] = package;

					var pickByLabelJob = new WhsPickByLabelJob(whs, i >= 1000 ? $"A{i % 1000}" : $"{i}");
					pickByLabelJobs[i] = pickByLabelJob;

					var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package);
					pickByLabelLabels[i] = pickByLabelLabel;

					var processTask = new ProcessTasks(pickByLabelLabel.PK, "WTL");
					processTasks[i] = processTask;

					var processHeader = new ProcessHeader("WPL", pickByLabelLabel.PK, "WTL");
					processHeaders[i] = processHeader;
				}

				sql.AppendLine(WhsDocket.GetBulkInsertStatement(receives));
				sql.AppendLine(WhsDocketLine.GetBulkInsertStatement(receiveLines));
				sql.AppendLine(WhsPick.GetBulkInsertStatement(picks));
				sql.AppendLine(WhsDocket.GetBulkInsertStatement(orders));
				sql.AppendLine(WhsDocketLine.GetBulkInsertStatement(orderLines));
				sql.AppendLine(WhsPickLine.GetBulkInsertStatement(pickLines));
				sql.AppendLine(PkgPackageJob.GetBulkInsertStatement(packageJobs));
				sql.AppendLine(PkgPackageHeader.GetBulkInsertStatement(packageHeaders));
				sql.AppendLine(PkgPackage.GetBulkInsertStatement(packages));
				sql.AppendLine(WhsPickByLabelJob.GetBulkInsertStatement(pickByLabelJobs));
				sql.AppendLine(WhsPickByLabelLabel.GetBulkInsertStatement(pickByLabelLabels));
				sql.AppendLine(ProcessTasks.GetBulkInsertStatement(processTasks));
				sql.AppendLine(ProcessHeader.GetBulkInsertStatement(processHeaders));
			}
		}

		public void TestOnlinePostUpgradeTransformation_CancellationToken()
		{
			var batchSize = 3;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WhE").WithDockDoor(TestConnection);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("Pr2").AppendInsertAndReturnObject(sql);

			for (var i = 1; i <= batchSize + 2; i++)
			{
				var order = SetupAndReturnWarehouseOrder(sql, whs, location, client, product, i);
				var package = SetupAndReturnPackage(sql, order, i);
				var pickByLabelJob = new WhsPickByLabelJob(whs, $"A{i}").AppendInsertAndReturnObject(sql);
				var pickByLabelLabel = new WhsPickByLabelLabel(pickByLabelJob, package).AppendInsertAndReturnObject(sql);

				new ProcessTasks(pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
				new ProcessHeader("WPL", pickByLabelLabel.PK, "WTL").AppendInsertAndReturnObject(sql);
			}

			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsPickLine_StockOnHandIsBalanced, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced))
			using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(Db.Connection, TestWhsDataSetupHelper.TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.Constants.PK, TestWhsDataSetupHelper.WhsCheckTransactionAndPickQtyIsCorrect_ForInsert))
			{
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals(5, ProcessTasks.CountInDB(TestConnection, task => task.P9_ParentTableCode == "WTL"));
			AssertEquals(5, ProcessHeader.CountInDB(TestConnection, header => header.FH_ParentTableCode == "WTL"));

			AssertExceptionThrown<OperationCanceledException>(() => GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, new CancellationToken(true)));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new RemovePickByLabelLabelProcessTasks();

		static WhsDocket SetupAndReturnWarehouseOrder(SqlQueryBuilder sql, WhsWarehouse whs, WhsLocation location, OrgHeader client, OrgSupplierPart product, int index = 1)
		{
			var today = DateTime.Today;
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", $"R{index}") { WD_FinalisedDate = today }.AppendInsertAndReturnObject(sql);
			var receiveLine = new WhsDocketLine(receive, product.PK, 10m, location.PK)
			{
				WE_OriginalInventoryStatus = "AVL",
				WE_CurrentInventoryStatus = "AVL",
				WE_PalletID = $"ABC{index}",
				WE_StockOnHand = 0m,
				WE_UnloadedTime = new DateTimeOffset(today),
				WE_GS_NKUnloadedBy = "A",
				WE_AdjustmentArrivalDate = today,
				WE_SystemCreateTimeUtc = DateTime.UtcNow,
				WE_SystemLastEditTimeUtc = DateTime.UtcNow,
				WE_SystemCreateUser = "A",
				WE_SystemLastEditUser = "A"
			}.AppendInsertAndReturnObject(sql);

			var pickWithOrder = new WhsPick(whs, $"P{index}", "NEW").AppendInsertAndReturnObject(sql);
			var orderId = $"O{index}";
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", orderId, orderId) { WD_WP = pickWithOrder.PK }.AppendInsertAndReturnObject(sql);
			var orderLine = new WhsDocketLine(order, product.PK, 10m).AppendInsertAndReturnObject(sql);

			new WhsPickLine(receiveLine, orderLine, 10m)
			{
				WZ_PickedDateTime = today,
				WZ_GS_NKAssignedTo = "US1",
				WZ_SystemCreateTimeUtc = DateTime.UtcNow,
				WZ_SystemLastEditTimeUtc = DateTime.UtcNow,
				WZ_SystemCreateUser = "A",
				WZ_SystemLastEditUser = "A"
			}.AppendInsertAndReturnObject(sql);
			return order;
		}

		static PkgPackage SetupAndReturnPackage(SqlQueryBuilder sql, WhsDocket order, int index = 1)
		{
			var packageJob = new PkgPackageJob(order.PK) { KJ_ParentTableCode = "WD", KJ_JobID = $"JOB{index}" }.AppendInsertAndReturnObject(sql);
			var packageHeader = new PkgPackageHeader($"PKG{index}", DateTime.UtcNow, "~BP").AppendInsertAndReturnObject(sql);
			var package = new PkgPackage(packageJob, "CNT", 1) { KP_KPH_PackageHeader = packageHeader }.AppendInsertAndReturnObject(sql);
			return package;
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.ProcessTasks DROP CONSTRAINT Constraint_P9_ParentTableCode_NoCheck");
		}
	}
}
