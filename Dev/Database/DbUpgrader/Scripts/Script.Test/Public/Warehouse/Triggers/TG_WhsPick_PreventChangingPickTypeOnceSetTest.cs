using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Warehouse.Triggers
{
	[TestedType(typeof(TG_WhsPick_PreventChangingPickTypeOnceSet))]
	class TG_WhsPick_PreventChangingPickTypeOnceSetTest : DBCreateTriggerScriptTest
	{
		public void TestPickTypeSetOnce_IfORD_ThenHLD() { AssertGivenPickTypeNotChangeable("ORD", "HLD"); }
		public void TestPickTypeSetOnce_IfORD_ThenWOR() { AssertGivenPickTypeNotChangeable("ORD", "WOR"); }
		public void TestPickTypeSetOnce_IfORD_ThenDWO() { AssertGivenPickTypeNotChangeable("ORD", "DWO"); }

		public void TestPickTypeSetOnce_IfHLD_ThenORD() { AssertGivenPickTypeNotChangeable("HLD", "ORD"); }
		public void TestPickTypeSetOnce_IfHLD_ThenWOR() { AssertGivenPickTypeNotChangeable("HLD", "WOR"); }
		public void TestPickTypeSetOnce_IfHLD_ThenDWO() { AssertGivenPickTypeNotChangeable("HLD", "DWO"); }

		public void TestPickTypeSetOnce_IfWOR_ThenORD() { AssertGivenPickTypeNotChangeable("WOR", "ORD"); }
		public void TestPickTypeSetOnce_IfWOR_ThenHLD() { AssertGivenPickTypeNotChangeable("WOR", "HLD"); }
		public void TestPickTypeSetOnce_IfWOR_ThenDWO() { AssertGivenPickTypeNotChangeable("WOR", "DWO"); }

		public void TestPickTypeSetOnce_IfDWO_ThenORD() { AssertGivenPickTypeNotChangeable("DWO", "ORD"); }
		public void TestPickTypeSetOnce_IfDWO_ThenHLD() { AssertGivenPickTypeNotChangeable("DWO", "HLD"); }
		public void TestPickTypeSetOnce_IfDWO_ThenWOR() { AssertGivenPickTypeNotChangeable("DWO", "WOR"); }

		void AssertGivenPickTypeNotChangeable(string initalPickType, string attemptChangeToPickType)
		{
			var sql = new SqlQueryBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs1 = new WhsWarehouse("WH1", branch1.PK).WithDockDoor(sql);
			var pick = new WhsPick(whs1, "P1", "ENT", initalPickType).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals("Prequisite", initalPickType, pick.WP_PickType);

			WhsPick.UpdateWhere(pick.PK).Set(p => p.WP_PickType, initalPickType).Post(TestConnection);

			AssertExceptionThrown(
					"Trigger should not allow a PickType to be changed once set.",
					typeof(SqlException),
					"Attempt to change Pick Type value once set is invalid.",
					() =>
					{
						WhsPick.UpdateWhere(pick.PK).Set(p => p.WP_PickType, attemptChangeToPickType).Post(TestConnection);
					},
					assertStartsWith: true);
		}
	}
}
