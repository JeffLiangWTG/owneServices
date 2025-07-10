using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.PatternMatching;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.PatternMatching.Testing
{
	[TestedType(typeof(SP_GetPatternMatchingData))]
	class SP_GetPatternMatchingDataTest : DbCreateScriptTest
	{
		public void TestPatternMatchingTableHasHashedValues()
		{
			var patternTablePks = GetPatternMatchingInformation("SP_GetPatternMatchingData", "PMP", new[] { 123456789, 246801214, -13579753 });

			AssertCollectionContains(pk1, patternTablePks);
			AssertCollectionContains(pk2, patternTablePks);
			AssertCollectionContains(pk3, patternTablePks);
		}

		public void TestPatternMatchingTableRetrievesNothingWithNoDataInSpecifiedTable()
		{
			var patternTablePks = GetPatternMatchingInformation("SP_GetPatternMatchingData", "PMA", new[] { 1234567890, 246801214, -13579753 });

			AssertEquals(0, patternTablePks.Count);
		}

		public void TestPatternTableRetrievesOnlyInfoWithMatchingHashedValues()
		{
			var patternTablePks = GetPatternMatchingInformation("SP_GetPatternMatchingData", "PMP", new[] { -13579753, 123456789 });

			AssertCollectionContains(pk1, patternTablePks);
			AssertCollectionNotContains(pk2, patternTablePks);
			AssertCollectionContains(pk3, patternTablePks);
		}

		public void TestPatternTableFiltersPerson()
		{
			pk1 = new Guid();

			TestConnection.ExecuteNonQuery($"INSERT INTO [GlbPerson] (PER_PK, PER_FullName) VALUES ('{pk1}', 'Homer Simpson')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.PatternMatchingPhone (PMP_PK, PMP_HashedValue, PMP_ParentTableCode, PMP_ParentID, PMP_PER, PMP_RN_NKCountryCode) VALUES (NEWID(), 187187187, 'OC', newID(), '" + pk1.ToString() + "', 'AU')");

			var patternTablePks = new List<Guid>();

			using (var cmd = CreateCommand("PMP", new[] { 187187187 }))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					patternTablePks.Add(Guid.Parse(reader["Person"].ToString()));
				}
			}

			AssertEquals(1, patternTablePks.Count);
			AssertEquals(pk1, patternTablePks.First());
		}

		Guid pk1;
		Guid pk2;
		Guid pk3;

		DbCommand CreateCommand(string tableCode, int[] hashedValues)
		{
			var cmd = TestConnection.Command("SP_GetPatternMatchingData");
			var hashedValuesTable = new DataTable();
			hashedValuesTable.Columns.Add("Value", typeof(int));
			foreach (var value in hashedValues)
			{
				hashedValuesTable.Rows.Add(value);
			}

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.AddParameter("@tableToSearch", SqlDbType.Char, tableCode);
			cmd.AddTableValuedParameter("@hashedValues", "dbo.TVP_int", hashedValuesTable);

			return cmd;
		}

		List<Guid> GetPatternMatchingInformation(string sql, string tableCode, int[] hashedValues)
		{
			InsertPatternMatchingTable();

			var patternTablePks = new List<Guid>();
			var hashedValuesTable = new DataTable();
			hashedValuesTable.Columns.Add("Value", typeof(int));
			foreach (var value in hashedValues)
			{
				hashedValuesTable.Rows.Add(value);
			}

			using (var cmd = CreateCommand(tableCode, hashedValues))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						patternTablePks.Add(Guid.Parse(reader["ParentID"].ToString()));
					}
				}
			}

			return patternTablePks;
		}

		void InsertPatternMatchingTable()
		{
			pk1 = Guid.NewGuid();
			pk2 = Guid.NewGuid();
			pk3 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery("INSERT INTO dbo.PatternMatchingPhone (PMP_PK, PMP_HashedValue, PMP_ParentTableCode, PMP_ParentId, PMP_RN_NKCountryCode) VALUES (NEWID(), 123456789, 'OC', '" + pk1.ToString() + "', 'AU')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.PatternMatchingPhone (PMP_PK, PMP_HashedValue, PMP_ParentTableCode, PMP_ParentId, PMP_RN_NKCountryCode) VALUES (NEWID(), 246801214, 'OA', '" + pk2.ToString() + "', 'AU')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.PatternMatchingPhone (PMP_PK, PMP_HashedValue, PMP_ParentTableCode, PMP_ParentId, PMP_RN_NKCountryCode) VALUES (NEWID(), -13579753, 'GS', '" + pk3.ToString() + "', 'AU')");
		}
	}
}

