using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class RefCityPCodeUpgradeTaskTest : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public void TestR0_PKIsViolated()
		{
			var postCodePK = Db.Connection.ExecuteScalar<Guid>("SELECT TOP 1 R0_RK FROM dbo.RefCityPCodePivot WHERE R0_IsSystem = 1 GROUP BY R0_RK HAVING COUNT(*) > 5");
			for (var i = 0; i < 20; i++)
			{
				var townPK = Db.Connection.ExecuteScalar<Guid>(string.Format(@"SELECT TOP 1 R9_PK FROM dbo.RefCityTown WHERE R9_PK NOT IN (SELECT R0_R9 FROM dbo.RefCityPCodePivot WHERE R0_RK = '{0}')", postCodePK));
				Db.Connection.ExecuteNonQuery(string.Format("INSERT INTO dbo.RefCityPCodePivot (R0_PK, R0_RK, R0_R9) VALUES (newid(), '{0}', '{1}')", postCodePK, townPK));
			}
			new RefCityPCodeUpgradeTask().Run();
		}

		public void TestTableUpgradeOrder()
		{
			RefCityPCodeUpgradeTaskForTableOrderTest testUpgradeTask = new RefCityPCodeUpgradeTaskForTableOrderTest();
			testUpgradeTask.Run();

			AssertEquals("Should have 3 tables: PostCode, CityTown & Pivot", 3, testUpgradeTask.TablesInUpgradingOrder.Length);
			AssertEquals("1st table", RefPostCodeSchema.Constants.TableName, testUpgradeTask.TablesInUpgradingOrder[0]);
			AssertEquals("2nd table", RefCityTownSchema.Constants.TableName, testUpgradeTask.TablesInUpgradingOrder[1]);
			AssertEquals("3rd table", RefCityPCodePivotSchema.Constants.TableName, testUpgradeTask.TablesInUpgradingOrder[2]);
		}

		public void TestOnlySystemRecordsAreUpdatedCity()
		{
			const string assertStateSql = "SELECT R9_LocalLanguageName FROM dbo.RefCityTown WHERE R9_InternationalName = 'Sydney' and R9_RW_NKState = 'NSW' and R9_RN_NKCountry = 'AU'";
			const string assertIsSystemSql = "SELECT R9_IsSystem FROM dbo.RefCityTown WHERE R9_InternationalName = 'Sydney' and R9_RW_NKState = 'NSW' and R9_RN_NKCountry = 'AU'";

			Db.Connection.ExecuteNonQuery("UPDATE dbo.RefCityTown SET R9_IsSystem = '0', R9_LocalLanguageName = 'WoolloomoollooLilliPilly' WHERE R9_InternationalName = 'Sydney' and R9_RW_NKState = 'NSW' and R9_RN_NKCountry = 'AU'");

			AssertEquals("[PRE-CONDITION] R9_LocalLanguageName for Sydney", "WoolloomoollooLilliPilly", Db.Connection.ExecuteScalar(assertStateSql).ToString());
			AssertEquals("[PRE-CONDITION] R9_IsSystem for Sydney", false, (bool)Db.Connection.ExecuteScalar(assertIsSystemSql));

			new RefCityPCodeUpgradeTask().Run();

			AssertEquals("UpgradeTask should not update non-system City", "WoolloomoollooLilliPilly", Db.Connection.ExecuteScalar(assertStateSql).ToString());
			AssertEquals("Record shoudl stay non system", false, (bool)Db.Connection.ExecuteScalar(assertIsSystemSql));

			Db.Connection.ExecuteNonQuery("UPDATE dbo.RefCityTown SET R9_IsSystem = '1' WHERE R9_InternationalName = 'Sydney' and R9_RW_NKState = 'NSW' and R9_RN_NKCountry = 'AU'");
			AssertEquals("[PRE-CONDITION] R9_LocalLanguageName for Sydney", "WoolloomoollooLilliPilly", Db.Connection.ExecuteScalar(assertStateSql).ToString());
			AssertEquals("[PRE-CONDITION] R9_IsSystem for Sydney", true, (bool)Db.Connection.ExecuteScalar(assertIsSystemSql));

			new RefCityPCodeUpgradeTask().Run();

			AssertEquals("UpgradeTask should update change state as record is system now", "", Db.Connection.ExecuteScalar(assertStateSql).ToString());
		}

		public void TestOnlySystemRecordsAreUpdatedPostCode()
		{
			const string assertStateSql = "SELECT RK_Lattitude FROM dbo.RefPostCode WHERE RK_CityTownPostCode = '2229' and RK_RN_NKCountry = 'AU'";
			const string assertIsSystemSql = "SELECT RK_IsSystem FROM dbo.RefPostCode WHERE RK_CityTownPostCode = '2229' and RK_RN_NKCountry = 'AU'";

			Db.Connection.ExecuteNonQuery("UPDATE dbo.RefPostCode SET RK_IsSystem = '0', RK_Lattitude = 1.111111 WHERE RK_CityTownPostCode = '2229' and RK_RN_NKCountry = 'AU'");

			AssertEquals("[PRE-CONDITION] RK_Lattitude for PostCode 2229", "1.111111", Db.Connection.ExecuteScalar(assertStateSql).ToString());
			AssertEquals("[PRE-CONDITION] RK_IsSystem for PostCode 2229", false, (bool)Db.Connection.ExecuteScalar(assertIsSystemSql));

			new RefCityPCodeUpgradeTask().Run();

			AssertEquals("UpgradeTask should not update non-system PostCode", "1.111111", Db.Connection.ExecuteScalar(assertStateSql).ToString());
			AssertEquals("Record should stay non system", false, (bool)Db.Connection.ExecuteScalar(assertIsSystemSql));

			Db.Connection.ExecuteNonQuery("UPDATE dbo.RefPostCode SET RK_IsSystem = '1' WHERE RK_CityTownPostCode = '2229' and RK_RN_NKCountry = 'AU'");

			AssertEquals("[PRE-CONDITION] RK_Lattitude for PostCode 2229", "1.111111", Db.Connection.ExecuteScalar(assertStateSql).ToString());
			AssertEquals("[PRE-CONDITION] RK_IsSystem for PostCode 2229", true, (bool)Db.Connection.ExecuteScalar(assertIsSystemSql));

			new RefCityPCodeUpgradeTask().Run();

			AssertEquals("UpgradeTask should update RK_Lattitude as record is system now", "0.000000", Db.Connection.ExecuteScalar(assertStateSql).ToString());
		}

		[ExpectNoExceptions()]
		public void TestComparingColumnsNames()
		{
			// real life test case which requires State (optional column) to be the last one in the list.

			Db.Connection.ExecuteNonQuery("UPDATE dbo.RefCityTown SET R9_IsSystem = '0', R9_RW_NKState = '' WHERE R9_InternationalName = 'Sydney' and R9_RW_NKState = 'NSW' and R9_RN_NKCountry = 'AU'");
			Db.Connection.ExecuteNonQuery("UPDATE dbo.RefCityTown SET R9_IsSystem = '1', R9_IsActive = '0' WHERE R9_InternationalName = 'Acton' and R9_RW_NKState = 'ACT' and R9_RN_NKCountry = 'AU'");

			new RefCityPCodeUpgradeTask().Run();
		}
	}
}
