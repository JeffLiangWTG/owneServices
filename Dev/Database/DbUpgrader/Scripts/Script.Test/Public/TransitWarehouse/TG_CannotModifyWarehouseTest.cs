using System;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;

namespace Enterprise.Build.Database.Script.Testing.Public.TransitWarehouse
{
	abstract class TG_CannotModifyWarehouseTest : DBCreateTriggerScriptTest
	{
		protected abstract string TableName { get; }
		protected abstract string WarehouseFKName { get; }
		protected abstract string PKName { get; }
		protected abstract string LastEditTime { get; }
		protected abstract string LastEditUser { get; }

		const string ExpectedErrorMessage = "Attempt to change warehouse.";

		public void TestTrigger_WarehouseCheck_WarehouseChanged()
		{
			var (entityPK, warehousePK, newWarehousePK) = SetupData();

			AssertExceptionThrown("Should throw an exception.", typeof(SqlException), ExpectedErrorMessage,
				() => TestConnection.ExecuteNonQuery(
$@"UPDATE {TableName} SET {WarehouseFKName} = '{newWarehousePK}', {LastEditTime} = GetUtcdate(), {LastEditUser} = '~BP' WHERE {PKName} = '{entityPK}'"), true);
		}

		public void TestTrigger_WarehouseCheck_WarehouseNoChange()
		{
			var (entityPK, warehousePK, newWarehousePK) = SetupData();

			AssertNoExceptionThrown("Should not throw exception", () => TestConnection.ExecuteNonQuery(
$@"UPDATE {TableName} SET {WarehouseFKName} = '{warehousePK}', {LastEditTime} = GetUtcdate(), {LastEditUser} = '~BP' WHERE {PKName} = '{entityPK}'"));
		}

		(Guid entityPK, Guid warehousePK, Guid newWarehousePK) SetupData()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var newWhs = new WhsWarehouse("WH2", "TRW", branch.PK).WithDockDoor(TestConnection);
			var entityPK = GetEntityPK(sql, whs);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			return (entityPK, whs.PK, newWhs.PK);
		}

		protected abstract Guid GetEntityPK(SqlQueryBuilder sql, WhsWarehouse whs);
	}
}
