using System;
using System.Linq.Expressions;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsDocket_PreventClientChangeWhenStartedReceiving))]
	class TG_WhsDocket_PreventClientChangeWhenStartedReceivingTest : DBCreateTriggerScriptTest
	{
		public void TestTrigger_Client()
		{
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);

			var client1 = new OrgHeader("CLIENT1").InsertAndReturnObject(TestConnection);
			var client2 = new OrgHeader("CLIENT2").InsertAndReturnObject(TestConnection);

			var receive1 = new WhsDocket(client1.PK, whs1.PK, "INW", "REC", "ENT", "R1").InsertAndReturnObject(TestConnection);
			var receive2 = new WhsDocket(client1.PK, whs1.PK, "INW", "REC", "ENT", "R2") { WD_StartedReceivingTimeUtc = DateTime.UtcNow }.InsertAndReturnObject(TestConnection);

			AssertTrigger_Update("Update must be allowed when the receive has not started receiving.", receive1.PK, d => d.WD_OH_Client, client2.PK, isSuccess: true);
			AssertTrigger_Update("Update must be allowed when not changing Client.", receive2.PK, d => d.WD_WW_Whs, whs2.PK, isSuccess: true);
			AssertTrigger_Update("Update must be allowed when the receive has started receiving but the value has not changed.", receive2.PK, d => d.WD_OH_Client, client1.PK, isSuccess: true);
			AssertTrigger_Update("Update must be prevented when the receive has starting receiving.", receive2.PK, d => d.WD_OH_Client, client2.PK, isSuccess: false);
		}

		public void TestTrigger_Client_Suspended()
		{
			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var branch2 = new GlbBranch("BR2").InsertAndReturnObject(TestConnection);
			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(TestConnection);
			var whs2 = new WhsWarehouse("WH2", branch2.PK).WithDockDoor(TestConnection);

			var client1 = new OrgHeader("CLIENT1").InsertAndReturnObject(TestConnection);
			var client2 = new OrgHeader("CLIENT2").InsertAndReturnObject(TestConnection);

			var receive = new WhsDocket(client1.PK, whs1.PK, "INW", "REC", "ENT", "R2") { WD_StartedReceivingTimeUtc = DateTime.UtcNow }.InsertAndReturnObject(TestConnection);

			using (TestConnection.BeginTransactionWithManager())
			{
				TestConnection.ExecuteNonQuery($"EXEC dbo.SuspendTrigger '{nameof(TG_WhsDocket_PreventClientChangeWhenStartedReceiving)}'");

				AssertTrigger_Update("Update must be prevented when the receive has starting receiving.", receive.PK, d => d.WD_OH_Client, client2.PK, isSuccess: true);

				TestConnection.ExecuteNonQuery($"EXEC dbo.ResumeTrigger '{nameof(TG_WhsDocket_PreventClientChangeWhenStartedReceiving)}'");
			}
		}

		public void TestTrigger_ClientCanBeChangedAtTheSameTimeAsStartedReceiving()
		{
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);

			var client1 = new OrgHeader("CLIENT1").InsertAndReturnObject(TestConnection);
			var client2 = new OrgHeader("CLIENT2").InsertAndReturnObject(TestConnection);

			var receive = new WhsDocket(client1.PK, whs.PK, "INW", "REC", "ENT", "R1").InsertAndReturnObject(TestConnection);
			AssertNoExceptionThrown(() =>
				WhsDocket.UpdateWhere(receive.PK)
				.Set(d => d.WD_OH_Client, client2.PK)
				.Set(d => d.WD_StartedReceivingTimeUtc, DateTime.UtcNow)
				.Post(TestConnection));
		}

		void AssertTrigger_Update<T>(string assertionMessage, Guid docketPK, Expression<Func<WhsDocket, T>> columnToUpdate, T valueToUpdateTo, bool isSuccess)
		{
			if (isSuccess)
			{
				AssertNoExceptionThrown(assertionMessage, UpdateColumn);
			}
			else
			{
				AssertExceptionThrown(assertionMessage, typeof(SqlException), "Attempt to change client for a Receive that has started Receiving.", UpdateColumn, true);
			}

			void UpdateColumn()
			{
				WhsDocket.UpdateWhere(docketPK).Set(columnToUpdate, valueToUpdateTo).Post(TestConnection);
			}
		}
	}
}

