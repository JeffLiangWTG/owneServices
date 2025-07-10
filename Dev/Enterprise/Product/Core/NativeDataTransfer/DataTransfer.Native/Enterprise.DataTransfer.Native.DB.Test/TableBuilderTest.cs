using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.Shared;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.DB
{
	public class TableBuilderTest : TransactionedTestCase
	{
		public void TestBuildTable()
		{
			// Add columns
			TestUtil.Connection.ExecuteNonQuery("ALTER TABLE dbo.DummyBizo ADD JamiesColumn BIT NULL");
			TestUtil.Connection.ExecuteNonQuery("ALTER TABLE dbo.DummyBizo ADD _3_Z0_Code BIT NULL");
			// Add index
			TestUtil.Connection.ExecuteNonQuery("CREATE UNIQUE INDEX MyIndex On DummyBizo(Z0_Code)");
			// Add inline index
			TestUtil.Connection.ExecuteNonQuery($"CREATE UNIQUE INDEX {IndexInfo.ONLINE_INDEX_PREFIX}MyIndex On DummyBizo(_3_Z0_Code)");

			var table = Table.Get("DummyBizo");
			var code = table.Columns["Z0_Code"];
			AssertEquals("PK Should only appear once", 1, table.Columns.Count(x => x.Name == "Z0_PK"));
			AssertEquals("This column is not included as it does not exist in DummyBizoSchema", 0, table.Columns.Count(x => x.Name == "JamiesColumn"));

			Assert("On line index should not be included", !table.CandidateKeyConstraints.Any(k => k.Name == "_on_MyIndex"));
			Assert("On line index should not be included", !table.CandidateKeyConstraints.Any(k => k.Columns.Any(x => x.Name == "_3_Z0_Code")));

			var candidateKey = table.CandidateKeyConstraints.Single();
			AssertEquals("Index should include same columns as define in DB", 1, candidateKey.ColumnSpan);
			AssertEquals("Index should include same columns as define in DB", true, candidateKey.Contains(code));

			AssertEquals("Computed columns should not be included", false, table.Columns.Any(c => c.Name.Contains("Computed")));
		}

		public void TestBuildIndexString()
		{
			var table = Table.Get("OrgContact");
			var results = table.IndexStrings;
			var result = string.Join(",", results.ToArray());
			AssertEquals("ContactName,OrgHeader.RSL,ShippingLine,OrgHeader.Code,OrgHeader.ScreeningStatus,CODE,IsActive,OrgHeader.RSL,ShippingLine,OrgHeader.Code,OrgHeader.ScreeningStatus,CODE,IsActive,Email", result);
		}

		public void TestTableBuilderUsesParameterisedQueries()
		{
			Table.Get("OrgContact");
			var buildIndexesSql = SqlEventTracker.Instance.LastSqlQuery;
			AssertContains("@p0 = \"HEAP\"", buildIndexesSql);
			AssertContains("@p1 = \"_on_%\"", buildIndexesSql);
			AssertContains("@p2 = \"0\"", buildIndexesSql);
			AssertContains("@p3 = \"OrgContact\"", buildIndexesSql);
		}

		public void TestBuildTable_TableNameIsEmpty()
		{
			AssertExceptionThrown(
				"InvalidOperationException Should be thrown",
				typeof(InvalidOperationException),
				() => Table.Get(string.Empty));
		}

		public void TestColumnEquality()
		{
			var table = Table.Get("DummyBizo");
			AssertEquals(table.Columns["Z0_PK"], table.Columns.PrimaryKey);
		}

		public void TestBuildConstraintsWithBaseObjectAsView()
		{
			var sql = string.Format(@"IF NOT EXISTS (SELECT NULL FROM sys.synonyms sn WHERE sn.name = '{0}' AND sn.base_object_name = '{1}')
BEGIN
	IF EXISTS (SELECT NULL FROM sys.synonyms sn WHERE sn.name = '{0}')
	BEGIN
		DROP SYNONYM [dbo].[{0}]
	END
	CREATE SYNONYM [dbo].[{0}] FOR [{2}].[dbo].[{1}]
END
", "RefDatabase_RefDataGrouping", "RefDataGroupingTableView_V1", RefDbTableNameResolver.SingleRefDatabaseName);

			Db.Connection.ExecuteScalar(sql);
			var table = Table.Get("RefDatabase_RefDataGrouping");

			Assert(table.SingleColumnCandidateConstraints.Any());
		}

		public void TestGetRefDbTableSynonymWithDifferentFormat()
		{
			var tableWithDifferentFormat = new string[]
			{
				"Test_RefDb_Ent_US",
				"CW-RefDb-Ent-US-000130",
				"CW-AG-RefDb-ORDWP4-CP1AS1-Ent-US-000130",
				"CW.cloud.RefDataGrouping",
				"CW-AG-RefDb-EUAWSDBAAG003.cloud.corp-Ent-US-000130"
			};

			foreach (var tableForTest in tableWithDifferentFormat)
			{
				var sqlForCreateNewTable = string.Format(@"IF NOT EXISTS (SELECT NULL FROM [{1}].sys.objects WHERE name='{0}')
BEGIN
	CREATE Table [{1}].[dbo].[{0}]([ZZZ_PK] [uniqueidentifier] NOT NULL,
	CONSTRAINT [{0}_PK] PRIMARY KEY NONCLUSTERED ([ZZZ_PK] ASC)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
", tableForTest, RefDbTableNameResolver.SingleRefDatabaseName);
				Db.Connection.ExecuteNonQuery(sqlForCreateNewTable);

				var sqlForCreateSynonymTable = string.Format(@"IF NOT EXISTS (SELECT NULL FROM sys.synonyms sn WHERE sn.name = '{0}' AND sn.base_object_name = '{1}')
BEGIN
	IF EXISTS (SELECT NULL FROM sys.synonyms sn WHERE sn.name = '{0}')
	BEGIN
		DROP SYNONYM [dbo].[{0}]
	END
	CREATE SYNONYM [dbo].[{0}] FOR [{2}].[dbo].[{1}]
END
", "RefDatabase_RefDataGrouping", tableForTest, RefDbTableNameResolver.SingleRefDatabaseName);

				Db.Connection.ExecuteScalar(sqlForCreateSynonymTable);

				var table = Table.Get("RefDatabase_RefDataGrouping");
				Assert(table.Columns.Any());

				TableBuilder.ResetStaticCacheForTesting();
			}
		}
	}
}
