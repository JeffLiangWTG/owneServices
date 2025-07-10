using System;
using System.Collections.Generic;
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
	[TestedType(typeof(PopulateWhsPickPickTypeColumn))]
	sealed class PopulateWhsPickPickTypeColumnTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateWhsPickPickTypeColumn();
		protected override void PrepareTestData()
		{
			var pickDate = DateTime.Today;
			var sql = new SqlQueryBuilder();

			var client = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);

			var unusedPick = new WhsPickOld_V02(whs, "P0", "NEW").AppendInsertAndReturnObject(sql);

			var orderPick = new WhsPickOld_V02(whs, "P1", "NEW").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "D1")
			{
				WD_WP = orderPick.PK,
			}.AppendInsertAndReturnObject(sql);

			var workOrderPick = new WhsPickOld_V02(whs, "P2", "NEW").AppendInsertAndReturnObject(sql);
			var workOrderOrder = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "ATP", "D2")
			{
				WD_WP = workOrderPick.PK,
			}.AppendInsertAndReturnObject(sql);

			var dynamicWorkOrderPick = new WhsPickOld_V02(whs, "P3", "NEW").AppendInsertAndReturnObject(sql);
			var dynamicWorkOrder = new WhsDocket(client.PK, whs.PK, "DWO", "ASS", "ATP", "D3")
			{
				WD_WP = dynamicWorkOrderPick.PK,
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override void AssertTransformationResults()
		{
			var updatedPicks = WhsPick.ShallowLoadFromDB(TestConnection);

			AssertEquals(2, updatedPicks.Count(pick => pick.WP_PickType == "ORD"));
			AssertEquals(1, updatedPicks.Count(pick => pick.WP_PickType == "WOR"));
			AssertEquals(1, updatedPicks.Count(pick => pick.WP_PickType == "DWO"));
			AssertPickTypesMatchDocketTypes(updatedPicks, TestConnection);
		}

		public void TestCoveringTrigger_InsertPickAndDocket()
		{
			GetNewTestTransformationInstance().Run();

			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsDocketSchema.Constants.TableName, "TG_WhsDocket_SetPickType"));

			var sql = new SqlQueryBuilder();

			var client = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);

			var unusedPick = new WhsPickOld_V02(whs, "P0", "NEW").AppendInsertAndReturnObject(sql);

			var orderPick = new WhsPickOld_V02(whs, "P1", "NEW").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "D1") { WD_WP = orderPick.PK }.AppendInsertAndReturnObject(sql);

			var workOrderPick = new WhsPickOld_V02(whs, "P2", "NEW").AppendInsertAndReturnObject(sql);
			var workOrder = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "ATP", "D2") { WD_WP = workOrderPick.PK }.AppendInsertAndReturnObject(sql);

			var dynamicWorkOrderPick = new WhsPickOld_V02(whs, "P3", "NEW").AppendInsertAndReturnObject(sql);
			var dynamicWorkOrder = new WhsDocket(client.PK, whs.PK, "DWO", "ASS", "ATP", "D3") { WD_WP = dynamicWorkOrderPick.PK }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var updatedPicks = WhsPick.ShallowLoadFromDB(TestConnection);
			AssertEquals(2, updatedPicks.Count(pick => pick.WP_PickType == "ORD"));
			AssertEquals(1, updatedPicks.Count(pick => pick.WP_PickType == "WOR"));
			AssertEquals(1, updatedPicks.Count(pick => pick.WP_PickType == "DWO"));
			AssertPickTypesMatchDocketTypes(updatedPicks, TestConnection);
		}

		public void TestCoveringTrigger_InsertDocket()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);

			var preUnusedPick = new WhsPickOld_V02(whs, "P0", "NEW").AppendInsertAndReturnObject(sql);
			var preOrderPick = new WhsPickOld_V02(whs, "P1", "NEW").AppendInsertAndReturnObject(sql);
			var preWorkOrderPick = new WhsPickOld_V02(whs, "P2", "NEW").AppendInsertAndReturnObject(sql);
			var preDynamicWorkOrderPick = new WhsPickOld_V02(whs, "P3", "NEW").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			sql.Clear();

			GetNewTestTransformationInstance().Run();

			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsDocketSchema.Constants.TableName, "TG_WhsDocket_SetPickType"));

			var insertedOrder = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "DEP", "D1") { WD_WP = preOrderPick.PK }.AppendInsertAndReturnObject(sql);
			var insertedWorkOrderOrder = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "ATP", "D2") { WD_WP = preWorkOrderPick.PK }.AppendInsertAndReturnObject(sql);
			var insertedDynamicWorkOrder = new WhsDocket(client.PK, whs.PK, "DWO", "ASS", "ATP", "D3") { WD_WP = preDynamicWorkOrderPick.PK }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var updatedPicks = WhsPick.ShallowLoadFromDB(TestConnection);
			AssertEquals(2, updatedPicks.Count(pick => pick.WP_PickType == "ORD"));
			AssertEquals(1, updatedPicks.Count(pick => pick.WP_PickType == "WOR"));
			AssertEquals(1, updatedPicks.Count(pick => pick.WP_PickType == "DWO"));
			AssertPickTypesMatchDocketTypes(updatedPicks, TestConnection);
		}

		public void TestCoveringTrigger_UpdateDocket()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);

			var preUnusedPick = new WhsPickOld_V02(whs, "P0", "NEW").AppendInsertAndReturnObject(sql);
			var preOrderPick = new WhsPickOld_V02(whs, "P1", "NEW").AppendInsertAndReturnObject(sql);
			var preWorkOrderPick = new WhsPickOld_V02(whs, "P2", "NEW").AppendInsertAndReturnObject(sql);
			var preDynamicWorkOrderPick = new WhsPickOld_V02(whs, "P3", "NEW").AppendInsertAndReturnObject(sql);

			var preOrder = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "NEW", "D1").AppendInsertAndReturnObject(sql);
			var preWorkOrder = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "NEW", "D2").AppendInsertAndReturnObject(sql);
			var preDynamicWorkOrder = new WhsDocket(client.PK, whs.PK, "DWO", "ASS", "NEW", "D3").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			sql.Clear();

			GetNewTestTransformationInstance().Run();

			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsDocketSchema.Constants.TableName, "TG_WhsDocket_SetPickType"));

			WhsDocket.UpdateWhere(preOrder.PK).Set(d => d.WD_DocketStatus, "ATP").Set(d => d.WD_WP, preOrderPick.PK).Post(TestConnection);
			WhsDocket.UpdateWhere(preWorkOrder.PK).Set(d => d.WD_DocketStatus, "ATP").Set(d => d.WD_WP, preWorkOrderPick.PK).Post(TestConnection);
			WhsDocket.UpdateWhere(preDynamicWorkOrder.PK).Set(d => d.WD_DocketStatus, "ATP").Set(d => d.WD_WP, preDynamicWorkOrderPick.PK).Post(TestConnection);

			var updatedPicks = WhsPick.ShallowLoadFromDB(TestConnection);
			AssertEquals(2, updatedPicks.Count(pick => pick.WP_PickType == "ORD"));
			AssertEquals(1, updatedPicks.Count(pick => pick.WP_PickType == "WOR"));
			AssertEquals(1, updatedPicks.Count(pick => pick.WP_PickType == "DWO"));
			AssertPickTypesMatchDocketTypes(updatedPicks, TestConnection);
		}

		public void TestCoveringTrigger_InsertMissmatchDocket()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);

			var preOrderPick1 = new WhsPickOld_V02(whs, "P1", "NEW").AppendInsertAndReturnObject(sql);
			var preOrderPick2 = new WhsPickOld_V02(whs, "P2", "NEW").AppendInsertAndReturnObject(sql);
			var preWorkOrderPick1 = new WhsPickOld_V02(whs, "P3", "NEW").AppendInsertAndReturnObject(sql);
			var preWorkOrderPick2 = new WhsPickOld_V02(whs, "P4", "NEW").AppendInsertAndReturnObject(sql);
			var preDynamicWorkOrderPick1 = new WhsPickOld_V02(whs, "P5", "NEW").AppendInsertAndReturnObject(sql);
			var preDynamicWorkOrderPick2 = new WhsPickOld_V02(whs, "P6", "NEW").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			sql.Clear();

			GetNewTestTransformationInstance().Run();

			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsDocketSchema.Constants.TableName, "TG_WhsDocket_SetPickType"));

			var insertedOrder1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ATP", "D1") { WD_WP = preWorkOrderPick1.PK }.AppendInsertAndReturnObject(sql);
			var insertedOrder2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ATP", "D2") { WD_WP = preDynamicWorkOrderPick1.PK }.AppendInsertAndReturnObject(sql);

			var insertedDynamicWorkOrder1 = new WhsDocket(client.PK, whs.PK, "DWO", "ASS", "ATP", "D5") { WD_WP = preOrderPick2.PK }.AppendInsertAndReturnObject(sql);
			var insertedDynamicWorkOrder2 = new WhsDocket(client.PK, whs.PK, "DWO", "ASS", "ATP", "D6") { WD_WP = preWorkOrderPick2.PK }.AppendInsertAndReturnObject(sql);

			var insertedWorkOrder1 = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "ATP", "D3") { WD_WP = preOrderPick1.PK }.AppendInsertAndReturnObject(sql);
			var insertedWorkOrder2 = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "ATP", "D4") { WD_WP = preDynamicWorkOrderPick2.PK }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var updatedPicks = WhsPick.ShallowLoadFromDB(TestConnection);
			AssertEquals(2, updatedPicks.Count(pick => pick.WP_PickType == "ORD"));
			AssertEquals(2, updatedPicks.Count(pick => pick.WP_PickType == "WOR"));
			AssertEquals(2, updatedPicks.Count(pick => pick.WP_PickType == "DWO"));
			AssertPickTypesMatchDocketTypes(updatedPicks, TestConnection);
		}

		public void TestCoveringTrigger_UpdateMissmatchDocket()
		{
			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);

			var preOrderPick1 = new WhsPickOld_V02(whs, "P1", "NEW").AppendInsertAndReturnObject(sql);
			var preOrderPick2 = new WhsPickOld_V02(whs, "P2", "NEW").AppendInsertAndReturnObject(sql);
			var preWorkOrderPick1 = new WhsPickOld_V02(whs, "P3", "NEW").AppendInsertAndReturnObject(sql);
			var preWorkOrderPick2 = new WhsPickOld_V02(whs, "P4", "NEW").AppendInsertAndReturnObject(sql);
			var preDynamicWorkOrderPick1 = new WhsPickOld_V02(whs, "P5", "NEW").AppendInsertAndReturnObject(sql);
			var preDynamicWorkOrderPick2 = new WhsPickOld_V02(whs, "P6", "NEW").AppendInsertAndReturnObject(sql);

			var preOrder1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "NEW", "D1").AppendInsertAndReturnObject(sql);
			var preOrder2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "NEW", "D2").AppendInsertAndReturnObject(sql);
			var preWorkOrder1 = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "NEW", "D3").AppendInsertAndReturnObject(sql);
			var preWorkOrder2 = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "NEW", "D4").AppendInsertAndReturnObject(sql);
			var preDynamicWorkOrder1 = new WhsDocket(client.PK, whs.PK, "DWO", "ASS", "NEW", "D5").AppendInsertAndReturnObject(sql);
			var preDynamicWorkOrder2 = new WhsDocket(client.PK, whs.PK, "DWO", "ASS", "NEW", "D6").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			sql.Clear();

			GetNewTestTransformationInstance().Run();

			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsDocketSchema.Constants.TableName, "TG_WhsDocket_SetPickType"));

			WhsDocket.UpdateWhere(preOrder1.PK).Set(d => d.WD_DocketStatus, "ATP").Set(d => d.WD_WP, preWorkOrderPick1.PK).Post(TestConnection);
			WhsDocket.UpdateWhere(preOrder2.PK).Set(d => d.WD_DocketStatus, "ATP").Set(d => d.WD_WP, preDynamicWorkOrderPick1.PK).Post(TestConnection);

			WhsDocket.UpdateWhere(preWorkOrder1.PK).Set(d => d.WD_DocketStatus, "ATP").Set(d => d.WD_WP, preOrderPick1.PK).Post(TestConnection);
			WhsDocket.UpdateWhere(preWorkOrder2.PK).Set(d => d.WD_DocketStatus, "ATP").Set(d => d.WD_WP, preDynamicWorkOrderPick2.PK).Post(TestConnection);

			WhsDocket.UpdateWhere(preDynamicWorkOrder1.PK).Set(d => d.WD_DocketStatus, "ATP").Set(d => d.WD_WP, preOrderPick2.PK).Post(TestConnection);
			WhsDocket.UpdateWhere(preDynamicWorkOrder2.PK).Set(d => d.WD_DocketStatus, "ATP").Set(d => d.WD_WP, preWorkOrderPick2.PK).Post(TestConnection);

			var updatedPicks = WhsPick.ShallowLoadFromDB(TestConnection);
			AssertEquals(2, updatedPicks.Count(pick => pick.WP_PickType == "ORD"));
			AssertEquals(2, updatedPicks.Count(pick => pick.WP_PickType == "WOR"));
			AssertEquals(2, updatedPicks.Count(pick => pick.WP_PickType == "DWO"));
			AssertPickTypesMatchDocketTypes(updatedPicks, TestConnection);
		}

		public void TestCoveringTrigger_InsertMixedDocket_Baseline()
		{
			GetNewTestTransformationInstance().Run();

			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsDocketSchema.Constants.TableName, "TG_WhsDocket_SetPickType"));

			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);

			var pick1 = new WhsPick(whs, "P1", "NEW", "ORD").AppendInsertAndReturnObject(sql);
			var dockets_WOR_DWO = new WhsDocket[]
			{
				new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "ATP", "D11") { WD_WP = pick1.PK },
				new WhsDocket(client.PK, whs.PK, "DWO", "ASS", "ATP", "D12") { WD_WP = pick1.PK },
			};
			sql.AppendLine(WhsDocket.GetBulkInsertStatement(dockets_WOR_DWO));

			var pick2 = new WhsPick(whs, "P2", "NEW", "ORD").AppendInsertAndReturnObject(sql);
			var dockets_DWO_WOR = new WhsDocket[]
			{
				new WhsDocket(client.PK, whs.PK, "DWO", "ASS", "ATP", "D21") { WD_WP = pick2.PK },
				new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "ATP", "D22") { WD_WP = pick2.PK },
			};
			sql.AppendLine(WhsDocket.GetBulkInsertStatement(dockets_DWO_WOR));

			var pick3 = new WhsPick(whs, "P3", "NEW", "ORD").AppendInsertAndReturnObject(sql);
			var dockets_ORD_WOR_ORD = new WhsDocket[]
			{
				new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ATP", "D31") { WD_WP = pick3.PK },
				new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "ATP", "D32") { WD_WP = pick3.PK },
				new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ATP", "D33") { WD_WP = pick3.PK },
			};
			sql.AppendLine(WhsDocket.GetBulkInsertStatement(dockets_ORD_WOR_ORD));

			var pick4 = new WhsPick(whs, "P4", "NEW", "WOR").AppendInsertAndReturnObject(sql);
			var dockets_WOR_ORD_WOR = new WhsDocket[]
			{
				new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "ATP", "D41") { WD_WP = pick4.PK },
				new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ATP", "D42") { WD_WP = pick4.PK },
				new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "ATP", "D43") { WD_WP = pick4.PK },
			};
			sql.AppendLine(WhsDocket.GetBulkInsertStatement(dockets_WOR_ORD_WOR));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var updatedPicks = WhsPick.ShallowLoadFromDB(TestConnection).ToDictionary(p => p.PK, p => p);
			AssertEquals("PickType should match last inserted Docket in batch", "DWO", updatedPicks[pick1.PK].WP_PickType);
			AssertEquals("PickType should match last inserted Docket in batch", "WOR", updatedPicks[pick2.PK].WP_PickType);
			AssertEquals("PickType should match last inserted Docket in batch with DocketType different to PickType", "WOR", updatedPicks[pick3.PK].WP_PickType);
			AssertEquals("PickType should match last inserted Docket in batch with DocketType different to PickType", "ORD", updatedPicks[pick4.PK].WP_PickType);
		}

		public void TestBulk()
		{
			var whsPicks = new List<WhsPickOld_V02>();
			var whsDockets = new List<WhsDocket>();

			var pickDate = DateTime.Today;
			var sql = new SqlQueryBuilder();

			var client = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);

			for (var i = 0; i < 1000; i++)
			{
				var pick = new WhsPickOld_V02(whs, $"ORD-{i}", "NEW");
				var docket = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ATP", $"ORD-{i}") { WD_WP = pick.PK };
				whsPicks.Add(pick);
				whsDockets.Add(docket);
			}
			for (var i = 0; i < 1000; i++)
			{
				var pick = new WhsPickOld_V02(whs, $"WOR-{i}", "NEW");
				var docket = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "ATP", $"WOR-{i}") { WD_WP = pick.PK };
				whsPicks.Add(pick);
				whsDockets.Add(docket);
			}
			for (var i = 0; i < 1000; i++)
			{
				var pick = new WhsPickOld_V02(whs, $"DWO-{i}", "NEW");
				var docket = new WhsDocket(client.PK, whs.PK, "DWO", "ASS", "ATP", $"DWO-{i}") { WD_WP = pick.PK };
				whsPicks.Add(pick);
				whsDockets.Add(docket);
			}
			sql.AppendLine(WhsPickOld_V02.GetBulkInsertStatement(whsPicks));
			sql.AppendLine(WhsDocket.GetBulkInsertStatement(whsDockets));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			AssertEquals("Precondition", 3000, WhsPickOld_V02.CountInDB(TestConnection));

			var transformation = GetNewTestTransformationInstance();
			transformation.Run();

			AssertEquals(3000,  WhsPick.CountInDB(TestConnection));
			AssertEquals(1000,  WhsPick.CountInDB(TestConnection, pick => pick.WP_PickType == "ORD"));
			AssertEquals(1000,  WhsPick.CountInDB(TestConnection, pick => pick.WP_PickType == "WOR"));
			AssertEquals(1000,  WhsPick.CountInDB(TestConnection, pick => pick.WP_PickType == "DWO"));
		}

		public void TestIsOnlinePreUpgradeTransform()
		{
			PrepareTestData();
			AssertEquals("Precondition", 4, WhsPickOld_V02.CountInDB(TestConnection));

			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var updatedPicks = WhsPick.ShallowLoadFromDB(TestConnection);
			AssertEquals(2, updatedPicks.Count(pick => pick.WP_PickType == "ORD"));
			AssertEquals(1, updatedPicks.Count(pick => pick.WP_PickType == "WOR"));
			AssertEquals(1, updatedPicks.Count(pick => pick.WP_PickType == "DWO"));
			AssertPickTypesMatchDocketTypes(updatedPicks, TestConnection);
		}

		#region Implementation

		protected override void SetUp()
		{
			new DbColumnDependencyRemover(WhsPickSchema.Constants.TableName, WhsPickSchema.Constants.WP_PickType).DropRelateObjects(Db.Connection);
			TestConnection.ExecuteNonQuery("ALTER TABLE WhsPick DROP COLUMN IF EXISTS WP_PickType");
		}

		static void AssertPickTypesMatchDocketTypes(WhsPick[] picks, DbConnection connection)
		{
			var dockets = WhsDocket.ShallowLoadFromDB(connection);

			foreach(var pick in picks)
			{
				var attachedDockets = dockets.Where(docket => docket.WD_WP == pick.PK).ToArray();
				var expectedPickType = attachedDockets.FirstOrDefault()?.WD_DocketType ?? "ORD";

				AssertEquals(expectedPickType, pick.WP_PickType);
				foreach(var attachedOrder in attachedDockets)
				{
					AssertEquals(expectedPickType, attachedOrder.WD_DocketType);
				}
			}
		}

		#endregion
	}
}
