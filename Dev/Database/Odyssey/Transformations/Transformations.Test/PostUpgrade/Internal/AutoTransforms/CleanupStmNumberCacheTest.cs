using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification.Internal.AutoTransforms;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	[UseSnapshotProtection]
	public class CleanupStmNumberCacheTest : TestCase
	{
		public void TestNoGap_Old()
		{
			AddNumbers("Name_1", new long[] { 21, 22 }, 23);
			AssertNumbers("Precondition", "Name_1", new long[] { 21, 22 }, 23);

			new CleanupStmNumberCache().Run();
			AssertNumbers("Full clean up due to unused data", "Name_1", Array.Empty<long>(), 21);
		}

		public void TestNoGap_New()
		{
			AddNumbers("Name_1", new long[] { 21, 22 }, 23);
			AddNumbers("Name_1", new long[] { 19, 20 }, 23, isUsed: true);

			new CleanupStmNumberCache().Run();
			AssertNumbers("Full clean up due to unused data", "Name_1", Array.Empty<long>(), 21);
		}

		public void TestGap_WithinOldFormat()
		{
			AddNumbers("Name_1", new long[] { 1, 2, 3, /*gap*/ 5, 6, 7, /*gap*/ 9, 10, 11 }, 12);

			new CleanupStmNumberCache().Run();
			AssertNumbers("Cleaned up the tail", "Name_1", new long[] { 1, 2, 3, /*gap*/ 5, 6, 7 /*gap*/ }, 9);
		}

		public void TestGap_Within()
		{
			AddNumbers("Name_1", new long[] { 1, 2, 3, /*gap*/ 5, 6, 7, /*gap*/ 9, 10, 11 }, 12, isUsed: false);
			AddNumbers("Name_1", new long[] { 4, 8 }, 12, isUsed: true);

			new CleanupStmNumberCache().Run();
			AssertNumbers("Cleaned up the tail", "Name_1", new long[] { 1, 2, 3, /*gap*/ 5, 6, 7 /*gap*/ }, 9);
		}

		public void TestGap_EndOldFormat()
		{
			AddNumbers("Name_1", new long[] { 1, 2 }, 5);

			new CleanupStmNumberCache().Run();
			AssertNumbers("No changes due to gap at the end", "Name_1", new long[] { 1, 2 /*gap*/ }, 5);
		}

		public void TestGap_End()
		{
			AddNumbers("Name_1", new long[] { 1, 2 }, 5, isUsed: false);
			AddNumbers("Name_1", new long[] { 3, 4 }, 5, isUsed: true);

			new CleanupStmNumberCache().Run();
			AssertNumbers("No changes due to gap at the end", "Name_1", new long[] { 1, 2 /*gap*/ }, 5);
		}

		public void TestUsedNumberIsConsideredWI00186337()
		{
			// used first two numbers, last is available
			// so all should be deleted, SN_Value should be first unused number
			AddNumbers("Name_1", new long[] { 21, 22 }, 24, isUsed: true);
			AddNumbers("Name_1", new long[] { 23 }, 24 /*unused value see line above */, isUsed: false);

			new CleanupStmNumberCache().Run();
			AssertNumbers("Full clean up due to unused data", "Name_1", Array.Empty<long>(), 23);
		}

		public void TestBigInt()
		{
			// used first two numbers, last is available
			// so all should be deleted, SN_Value should be first unused number
			AddNumbers("Name_1", new long[] { 2147483647, 2147483648 }, 2147483650);

			new CleanupStmNumberCache().Run();
			AssertNumbers("No changes due to gap at the end", "Name_1", new long[] { 2147483647, 2147483648 /*gap*/ }, 2147483650);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			PrepareTestData();
		}

		void PrepareTestData()
		{
			// Cleanup tables
			Db.Connection.ExecuteNonQuery(@"
TRUNCATE TABLE dbo.StmNumberCache;
DELETE dbo.StmNums;
"
				);

			var curr_date = Db.Connection.ExecuteScalar<DateTime>("SELECT GETUTCDATE();").Date;
		}

		void AddNumbers(string parentName, long[] numbers, long parentValue, bool isUsed = false)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
if not exists (Select null from dbo.StmNums where SN_Name = '{0}') BEGIN
	INSERT dbo.StmNums (SN_Name, SN_Value) VALUES
		('{0}', {1})
END

INSERT dbo.StmNumberCache (SG_SN, SG_Value, SG_IsUsed)
SELECT
	SN_ID,
	t.Number,
	{3}
FROM
	dbo.StmNums
	CROSS JOIN
	(VALUES
		({2})
	) AS t(Number)
WHERE
	SN_Name = '{0}'
"
				, parentName
				, parentValue
				, string.Join("), (", numbers)
				, isUsed ? "1" : "0"
				);

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		void AssertNumbers(string message, string parentName, long[] numbers, long parentValue)
		{
			var expected = new List<string>(numbers.Length + 1);
			expected.Add(GetStmNums(parentName, parentValue));
			expected.AddRange(numbers.Select(n => GetStmNumberCache(parentName, n)));

			AssertContainsExactElementsInAnyOrder(message, expected, GetStmNumbers());
		}

		string GetStmNums(string name, long value)
		{
			return string.Format(CultureInfo.InvariantCulture, "StmNums: SN_Name-{0}, SN_Value-{1}", name, value);
		}

		string GetStmNumberCache(string parentName, long value)
		{
			return string.Format(CultureInfo.InvariantCulture, "StmNumberCache: {0}-{1}", parentName, value);
		}

		IEnumerable<string> GetStmNumbers()
		{
			var result = new List<string>();

			var sql = @"
SELECT
	stmt = CONCAT('StmNums: SN_Name-', SN_Name, ', SN_Value-', SN_Value)
FROM
	dbo.StmNums

UNION ALL

SELECT
	stmt = CONCAT('StmNumberCache: ', SN_Name, '-', SG_Value)
FROM
	dbo.StmNums
	JOIN dbo.StmNumberCache ON SG_SN = SN_ID
";

			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					result.Add((string)reader["stmt"]);
				}
			}

			return result;
		}

		#endregion // Implementation
	}
}
