using System;
using System.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	public class RefPackTypeUpgradeTaskTest : TransactionedTestCase
	{
		const string countTotalPackTypeSql = "SELECT COUNT(*) FROM dbo.RefPackType WHERE F3_Code = 'XXX'";

		public void TestF3_IsUpdatable()
		{
			RefPackTypeUpgradeTask task = new RefPackTypeUpgradeTask(new RefPackTypeDataFile());
			int initialPackTypeCount = GetCount(countTotalPackTypeSql);
			const string countSqlXXX = "SELECT COUNT(*) FROM dbo.RefPackType WHERE F3_Code = 'XXX'";

			// Tests for user packType
			AssertEquals("[PRE-CONDITION] XXX Not Found", 0, GetCount(countSqlXXX));

			string sqlText = "INSERT INTO dbo.RefPackType(F3_PK, F3_Code, F3_IsSystem, F3_IsUpdatable) VALUES (NEWID(), 'XXX', 0, 0)";
			TestConnection.ExecuteNonQuery(sqlText);
			AssertEquals("[PRE-CONDITION] XXX Found", 1, GetCount(countSqlXXX));
			AssertEquals("Current ref pack type count", initialPackTypeCount + 1, GetCount(countTotalPackTypeSql));

			task.Run();
			AssertEquals("UpgradeTask should not delete user non-updatable packType", 1, GetCount(countSqlXXX));
			AssertEquals("Current ref pack type count", initialPackTypeCount + 1, GetCount(countTotalPackTypeSql));

			sqlText = "UPDATE dbo.RefPackType SET F3_IsUpdatable = 1 WHERE F3_Code = 'XXX'";
			TestConnection.ExecuteNonQuery(sqlText);
			AssertEquals("[PRE-CONDITION] XXX Found", 1, GetCount(countSqlXXX));
			AssertEquals("Current ref pack type count", initialPackTypeCount + 1, GetCount(countTotalPackTypeSql));

			task.Run();
			AssertEquals("UpgradeTask should delete user updatable packType", 0, GetCount(countSqlXXX));
			AssertEquals("Current ref pack type count", initialPackTypeCount, GetCount(countTotalPackTypeSql));

			// Tests for system packType
			const string countSqlDOZ = "SELECT COUNT(*) FROM dbo.RefPackType WHERE F3_Code = 'DOZ'";
			const string selectSqlDescription = "SELECT F3_Description FROM dbo.RefPackType WHERE F3_Code = 'DOZ'";
			AssertEquals("[PRE-CONDITION] DOZ Found", 1, GetCount(countSqlDOZ));
			AssertEquals("[PRE-CONDITION] DOZ F3_Description  = 'Dozen'", "Dozen", TestConnection.ExecuteScalar(selectSqlDescription).ToString());

			TestConnection.ExecuteNonQuery("UPDATE dbo.RefPackType SET F3_Description = 'Whouah!', F3_IsUpdatable = 1 WHERE F3_Code = 'DOZ'");
			AssertEquals("[PRE-CONDITION] DOZ F3_Description  = 'Whouah!'", "Whouah!", TestConnection.ExecuteScalar(selectSqlDescription).ToString());

			task.Run();
			AssertEquals("UpgradeTask should update updatable system packType", "Dozen", TestConnection.ExecuteScalar(selectSqlDescription).ToString());
			AssertEquals("Current ref pack type count", initialPackTypeCount, GetCount(countTotalPackTypeSql));

			TestConnection.ExecuteNonQuery("UPDATE dbo.RefPackType SET F3_Description = 'Whouah!', F3_IsUpdatable = 0 WHERE F3_Code = 'DOZ'");
			task.Run();
			AssertEquals("UpgradeTask should not update non-updatable system packType", "Whouah!", TestConnection.ExecuteScalar(selectSqlDescription).ToString());
			AssertEquals("Current ref pack type count", initialPackTypeCount, GetCount(countTotalPackTypeSql));

			sqlText = "INSERT INTO dbo.RefPackType(F3_PK, F3_Code, F3_IsSystem, F3_IsUpdatable) VALUES (NEWID(), 'XXX', 1, 0)";
			TestConnection.ExecuteNonQuery(sqlText);
			AssertEquals("Current ref pack type count", initialPackTypeCount + 1, GetCount(countTotalPackTypeSql));

			task.Run();
			AssertEquals("UpgradeTask should not delete not-updatable system packType", 1, GetCount(countSqlXXX));
			AssertEquals("Current ref pack type count", initialPackTypeCount + 1, GetCount(countTotalPackTypeSql));

			sqlText = "UPDATE dbo.RefPackType SET F3_IsUpdatable = 1 WHERE F3_Code = 'XXX'";
			TestConnection.ExecuteNonQuery(sqlText);

			task.Run();
			AssertEquals("UpgradeTask should delete updatable system packType", 0, GetCount(countSqlXXX));
			AssertEquals("Current ref pack type count", initialPackTypeCount, GetCount(countTotalPackTypeSql));
		}

		public void TestHandlesDuplicateCodeOnInsertWhenPackageTypeIsNotUpdatable()
		{
			RefPackTypeUpgradeTask task = new RefPackTypeUpgradeTask(new RefPackTypeDataFile());
			int initialPackTypeCount = GetCount(countTotalPackTypeSql);

			string sqlText = "SELECT F3_PK FROM dbo.RefPackType WHERE F3_Code = 'DOZ'";
			Guid systemDozPk = (Guid)TestConnection.ExecuteScalar(sqlText);
			Guid userDozPk = Guid.NewGuid();

			sqlText = string.Format(@"DELETE FROM dbo.RefPackType WHERE F3_PK = '{0}'", systemDozPk);
			TestConnection.ExecuteNonQuery(sqlText);
			AssertEquals("[before upgrade] DOZ system pack type is in the database?", expected: false, IsRefPackTypeInDatabase(systemDozPk, "DOZ"));

			sqlText = string.Format(@"INSERT dbo.RefPackType (F3_PK, F3_Code, F3_IsSystem, F3_IsUpdatable) VALUES ('{0}', 'DOZ', 0, 0)", userDozPk.ToString());
			TestConnection.ExecuteNonQuery(sqlText);
			AssertEquals("[before upgrade] DOZ user pack type is in the database?", expected: true, IsRefPackTypeInDatabase(userDozPk, "DOZ"));

			task.Run();

			AssertEquals("[after upgrade] DOZ system pack type is in the database?", expected: false, IsRefPackTypeInDatabase(systemDozPk, "DOZ"));
			AssertEquals("[after upgrade] DOZ user pack type is in the database?", expected: true, IsRefPackTypeInDatabase(userDozPk, "DOZ"));
			AssertEquals("Current ref pack type count", initialPackTypeCount, GetCount(countTotalPackTypeSql));
		}

		public void TestHandlesDuplicateCodeOnInsertWhenPackageTypeIsNotUpdatableAndIsSystem()
		{
			RefPackTypeUpgradeTask task = new RefPackTypeUpgradeTask(new RefPackTypeDataFile());
			int initialPackTypeCount = GetCount(countTotalPackTypeSql);

			string sqlText = "SELECT F3_PK FROM dbo.RefPackType WHERE F3_Code = 'DOZ'";
			Guid systemDozPk = (Guid)TestConnection.ExecuteScalar(sqlText);
			Guid userDozPk = Guid.NewGuid();

			sqlText = string.Format(@"DELETE FROM dbo.RefPackType WHERE F3_PK = '{0}'", systemDozPk);
			TestConnection.ExecuteNonQuery(sqlText);
			AssertEquals("[before upgrade] DOZ system pack type is in the database?", expected: false, IsRefPackTypeInDatabase(systemDozPk, "DOZ"));

			sqlText = string.Format(@"INSERT dbo.RefPackType (F3_PK, F3_Code, F3_IsSystem, F3_IsUpdatable) VALUES ('{0}', 'DOZ', 1, 0)", userDozPk.ToString());
			TestConnection.ExecuteNonQuery(sqlText);
			AssertEquals("[before upgrade] DOZ user pack type is in the database?", expected: true, IsRefPackTypeInDatabase(userDozPk, "DOZ"));

			task.Run();

			AssertEquals("[after upgrade] DOZ system pack type is in the database?", expected: false, IsRefPackTypeInDatabase(systemDozPk, "DOZ"));
			AssertEquals("[after upgrade] DOZ user pack type is in the database?", expected: true, IsRefPackTypeInDatabase(userDozPk, "DOZ"));
			AssertEquals("Current ref pack type count", initialPackTypeCount, GetCount(countTotalPackTypeSql));
		}

		public void TestHandlesDuplicateCodeOnInsertWhenPackageTypeIsUpdatable()
		{
			RefPackTypeUpgradeTask task = new RefPackTypeUpgradeTask(new RefPackTypeDataFile());
			int initialPackTypeCount = GetCount(countTotalPackTypeSql);

			string sqlText = "SELECT F3_PK FROM dbo.RefPackType WHERE F3_Code = 'DOZ'";
			Guid systemDozPk = (Guid)TestConnection.ExecuteScalar(sqlText);

			var userDozPKs = new[] { Guid.Parse("00000000-CD05-43B3-87F4-A03CA33B17E9"), Guid.Parse("FFFFFFFF-CD05-43B3-87F4-A03CA33B17E9") };
			foreach (var userDozPk in userDozPKs)
			{
				sqlText = string.Format(@"DELETE FROM dbo.RefPackType WHERE F3_PK = '{0}'", systemDozPk);
				TestConnection.ExecuteNonQuery(sqlText);
				AssertEquals("[before upgrade] DOZ system pack type is not in the database", expected: false, IsRefPackTypeInDatabase(systemDozPk, "DOZ"));

				sqlText = string.Format(@"INSERT dbo.RefPackType (F3_PK, F3_Code, F3_IsSystem, F3_IsUpdatable) VALUES ('{0}', 'DOZ', 0, 1)", userDozPk.ToString());
				TestConnection.ExecuteNonQuery(sqlText);
				AssertEquals("[before upgrade] DOZ user pack type is in the database", expected: true, IsRefPackTypeInDatabase(userDozPk, "DOZ"));

				task.Run();

				AssertEquals("[after upgrade] DOZ system pack type is in the database", expected: true, IsRefPackTypeInDatabase(systemDozPk, "DOZ"));
				AssertEquals("[after upgrade] DOZ user pack type is not in the database", expected: false, IsRefPackTypeInDatabase(userDozPk, "DOZ"));
				AssertEquals("Current ref pack type count", initialPackTypeCount, GetCount(countTotalPackTypeSql));
			}
		}

		public void TestHandlesDuplicateCodeOnUpdateWhenPackageTypeIsNotUpdatable()
		{
			RefPackTypeUpgradeTask task = new RefPackTypeUpgradeTask(new RefPackTypeDataFile());
			int initialPackTypeCount = GetCount(countTotalPackTypeSql);

			string sqlText = "SELECT F3_PK FROM dbo.RefPackType WHERE F3_Code = 'DOZ'";
			Guid systemDozPk = (Guid)TestConnection.ExecuteScalar(sqlText);
			Guid userDozPk = Guid.NewGuid();

			sqlText = string.Format(@"
					UPDATE dbo.RefPackType SET F3_Code = 'DOY' WHERE F3_PK = '{0}'
					INSERT dbo.RefPackType (F3_PK, F3_Code, F3_IsSystem, F3_IsUpdatable) VALUES ('{1}', 'DOZ', 0, 0)", systemDozPk.ToString(), userDozPk.ToString());
			TestConnection.ExecuteNonQuery(sqlText);
			AssertEquals("[before upgrade] DOY system pack type (former DOZ) is in the database?", expected: true, IsRefPackTypeInDatabase(systemDozPk, "DOY"));
			AssertEquals("[before upgrade] DOZ user pack type (DOZ) is in the database?", expected: true, IsRefPackTypeInDatabase(userDozPk, "DOZ"));

			task.Run();

			AssertEquals("[after upgrade] DOZ system pack type (DOZ->DOY->DOZ) is in the database?", expected: false, IsRefPackTypeInDatabase(systemDozPk, "DOZ"));
			AssertEquals("[after upgrade] DOZ user pack type is in the database?", expected: true, IsRefPackTypeInDatabase(userDozPk, "DOZ"));
			AssertEquals("Current ref pack type count", initialPackTypeCount, GetCount(countTotalPackTypeSql));
		}

		public void TestHandlesDuplicateCodeOnUpdateWhenPackageTypeIsUpdatable()
		{
			RefPackTypeUpgradeTask task = new RefPackTypeUpgradeTask(new RefPackTypeDataFile());
			int initialPackTypeCount = GetCount(countTotalPackTypeSql);

			string sqlText = "SELECT F3_PK FROM dbo.RefPackType WHERE F3_Code = 'DOZ'";
			Guid systemDozPk = (Guid)TestConnection.ExecuteScalar(sqlText);

			var userDozPKs = new[] { Guid.Parse("{00000000-D9E0-4539-A993-1AD5E44C9CCA}"), Guid.Parse("{FFFFFFFF-D9E0-4539-A993-1AD5E44C9CCA}") };
			foreach (var userDozPk in userDozPKs)
			{
				sqlText = string.Format(@"
					UPDATE dbo.RefPackType SET F3_Code = 'DOY' WHERE F3_PK = '{0}'
					INSERT dbo.RefPackType (F3_PK, F3_Code, F3_IsSystem, F3_IsUpdatable) VALUES ('{1}', 'DOZ', 0, 1)", systemDozPk.ToString(), userDozPk.ToString());
				TestConnection.ExecuteNonQuery(sqlText);
				AssertEquals("[before upgrade] DOY system pack type (former DOZ) is in the database?", expected: true, IsRefPackTypeInDatabase(systemDozPk, "DOY"));
				AssertEquals("[before upgrade] DOZ user pack type (DOZ) is in the database?", expected: true, IsRefPackTypeInDatabase(userDozPk, "DOZ"));

				task.Run();

				AssertEquals("[after upgrade] DOZ system pack type (DOZ->DOY->DOZ)is in the database?", expected: true, IsRefPackTypeInDatabase(systemDozPk, "DOZ"));
				AssertEquals("[after upgrade] DOZ user pack type is in the database?", expected: false, IsRefPackTypeInDatabase(userDozPk, "DOZ"));
				AssertEquals("Current ref pack type count", initialPackTypeCount, GetCount(countTotalPackTypeSql));
			}
		}

		public void TestNoConcurrencyErrorThrownWhenMultipleMatchesInBothPKAndCode()
		{
			var task = new RefPackTypeUpgradeTask(new RefPackTypeDataFile());
			var initialPackTypeCount = GetCount(countTotalPackTypeSql);

			var sqlText = "SELECT F3_PK FROM dbo.RefPackType WHERE F3_Code = 'PLT'";
			var systemPltPK = (Guid)TestConnection.ExecuteScalar(sqlText);
			sqlText = string.Format(@"
					DELETE RefPackType WHERE F3_Code='ROR';
					UPDATE dbo.RefPackType SET F3_Code = 'ROR' WHERE F3_PK = '{0}'", systemPltPK);
			TestConnection.ExecuteNonQuery(sqlText);
			AssertEquals("[before upgrade] PLT system pack type is in the database?", expected: false, IsRefPackTypeInDatabase(systemPltPK, "PLT"));
			AssertEquals("[before upgrade] ROR system pack type (former PLT) is in the database?", expected: true, IsRefPackTypeInDatabase(systemPltPK, "ROR"));

			AssertNoExceptionThrown(() => task.Run());

			sqlText = "SELECT F3_PK FROM dbo.RefPackType WHERE F3_Code = 'ROR'";
			var systemRorPK = (Guid)TestConnection.ExecuteScalar(sqlText);
			AssertNotEquals(systemRorPK, systemPltPK);
			AssertEquals("[after upgrade] PLT system pack type is in the database?", expected: true, IsRefPackTypeInDatabase(systemPltPK, "PLT"));
			AssertEquals("[after upgrade] ROR system pack type is in the database?", expected: true, IsRefPackTypeInDatabase(systemRorPK, "ROR"));
			AssertEquals("Current ref pack type count", initialPackTypeCount, GetCount(countTotalPackTypeSql));
		}

		public void TestCheckAndResolveUniqueCodeConflict()
		{
			var table = new DataTable();
			table.Columns.Add("F3_PK", typeof(Guid));
			table.Columns.Add("F3_Code", typeof(string));
			table.Columns.Add("F3_IsUpdatable", typeof(bool));
			table.Rows.Add(Guid.NewGuid(), "AAA", false);
			table.Rows.Add(Guid.NewGuid(), "BBB", true);
			table.AcceptChanges();

			var task = new RefPackTypeUpgradeTaskForTest();
			var result = task.CheckAndResolveUniqueCodeConflict(table, "AAA");
			var rowA = table.Select("F3_Code = 'AAA'")[0];
			AssertEquals(expected: false, result);
			AssertEquals(DataRowState.Unchanged, rowA.RowState);

			result = task.CheckAndResolveUniqueCodeConflict(table, "BBB");
			AssertEquals(expected: true, result);
			var rowB = table.Select("F3_Code = 'BBB'");
			AssertEquals(expected: 0, rowB.Length);

			result = task.CheckAndResolveUniqueCodeConflict(table, "CCC");
			AssertEquals(expected: true, result);
		}

		bool IsRefPackTypeInDatabase(Guid pk, string code)
		{
			string sqlText = string.Format(
				"SELECT count(*) FROM dbo.RefPackType WHERE F3_PK = '{0}' AND F3_Code = '{1}'",
				pk.ToString(), code);
			return (GetCount(sqlText) == 1);
		}

		int GetCount(string sql)
		{
			return (int)TestConnection.ExecuteScalar(sql);
		}
	}

	class RefPackTypeUpgradeTaskForTest : RefPackTypeUpgradeTask
	{
		internal new bool CheckAndResolveUniqueCodeConflict(DataTable targetTable, string code)
		{
			return base.CheckAndResolveUniqueCodeConflict(targetTable, code);
		}
	}
}
