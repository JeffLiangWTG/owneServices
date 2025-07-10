using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;

namespace Enterprise.Client.EDI.ScavengingImportServiceTask
{
	public class ScavengingPartitionHelperTest : NUnit.Framework.TestCase
	{
		public void TestLatestPartitionTime()
		{
			Db.Connection.ExecuteNonQuery("ALTER DATABASE CURRENT ADD FILEGROUP[CIHGROUP201606]");
			Db.Connection.ExecuteNonQuery("ALTER DATABASE CURRENT ADD FILEGROUP[CIHGROUP201607]");
			var date = ScavengingPartitioner.LatestPartitionTime.ToDateTime();
			AssertDateTimeWithinOneSecond("Should exactly be 2016-07-01 00:00:00", new DateTime(2016, 7, 1), date);
		}

		public void TestEnsureDBPartitionExists()
		{
			try
			{
				var date = new DateTime(2011, 11, 11);
				AssertNull("CIHGROUP201112 not exists, yet", FindPartition(date));
				ScavengingPartitioner.EnsureDBPartitionExists(date);
				AssertEquals("CIHGROUP201112 exists", "CIHGROUP201112", FindPartition(date));
			}
			catch (SqlException ex) when (ex.ErrorCode == 5009 || ex.ErrorCode == 5149)
			{
				Assert(true);
			}
		}

		public void TestPartitionExists()
		{
			try
			{
				var date = new DateTime(2011, 11, 11);
				Assert("CIHGROUP201112 not exists, yet", !ScavengingPartitioner.PartitionExists(date));
				ScavengingPartitioner.CreatePartition(date);
				Assert("CIHGROUP201112 exists", ScavengingPartitioner.PartitionExists(date));
			}
			catch (SqlException ex) when (ex.ErrorCode == 5009 || ex.ErrorCode == 5149)
			{
				Assert(true);
			}
		}

		public void TestGetRowCountOfFileGroup()
		{
			const string dummyTableName = "DummyBizo";
			var date = new ZDateTime(2010, 03, 18);
			AssertEquals("Empty dummybizo Table", 0, GetRowCount(dummyTableName));
			AssertEquals("Empty Filegroup", 0, ScavengingPartitioner.GetRowCountOfFileGroup(date));
			PartitionTable("DummyBizo", "Z0_PK", "Z0_Date");
			Db.Connection.ExecuteNonQuery(@"
insert into dbo.dummybizo(Z0_PK, Z0_Date) values (newid(), '2010-03-18')
insert into dbo.dummybizo(Z0_PK, Z0_Date) values (newid(), '2010-04-18')
insert into dbo.dummybizo(Z0_PK, Z0_Date) values (newid(), '2010-04-19')
");
			AssertEquals("Dummybizo Table has 3 rows", 3, GetRowCount(dummyTableName));
			AssertEquals("CIHGROUP201003 has 1 row", 1, ScavengingPartitioner.GetRowCountOfFileGroup(date));
			AssertEquals("CIHGROUP201004 has 2 rows", 2, ScavengingPartitioner.GetRowCountOfFileGroup(date.AddMonths(1)));
		}

		public void TestTruncatePartition()
		{
			const string dummyTableName = "DummyBizo";
			var date = new ZDateTime(2010, 03, 18);
			AssertEquals("Empty dummybizo Table", 0, GetRowCount(dummyTableName));
			AssertEquals("Empty Filegroup", 0, ScavengingPartitioner.GetRowCountOfFileGroup(date));
			PartitionTable("DummyBizo", "Z0_PK", "Z0_Date");
			Db.Connection.ExecuteNonQuery(@"
DROP TABLE DummyDependentBizo --It has a foreign key pointing to DummyBizo
insert into dbo.dummybizo(Z0_PK, Z0_Date) values (newid(), '2010-03-18')
insert into dbo.dummybizo(Z0_PK, Z0_Date) values (newid(), '2010-04-19')
insert into dbo.dummybizo(Z0_PK, Z0_Date) values (newid(), '2010-04-18')
");
			AssertEquals("Dummybizo Table has 3 rows", 3, GetRowCount(dummyTableName));
			DropEverythingThatBlocksTruncate("DummyBizo", "PK_UX__Z0_PK", new[] { "NR_RX__Z0_BitFiltered" }, new[] { "ZZDummyBizo", "ZZDummyBizo_Idx" });
			ScavengingPartitioner.TruncatePartition(date, "DummyBizo");
			var dummies = new System.Collections.Generic.List<DateTime>();
			Db.Connection.ExecuteReader("SELECT Z0_DATE FROM dbo.DUMMYBIZO ORDER BY Z0_DATE", record => dummies.Add(record.GetDateTime(0)));
			AssertEquals(2, dummies.Count);
			AssertDateTimeWithinOneSecond("2010-04-18 one stays", date.ToDateTime().AddMonths(1), dummies[0]);
			AssertDateTimeWithinOneSecond("2010-04-19 one stays", date.ToDateTime().AddMonths(1).AddDays(1), dummies[1]);
		}

		#region Implementation
		public static void DropEverythingThatBlocksTruncate(string tableName, string constraintName, string[] indexNames, string[] viewNames = null)
		{
			bool changeTrackingEnabled = false;
			Db.Connection.ExecuteReader($"SELECT 1 FROM sys.change_tracking_tables where OBJECT_NAME(object_id) = '{tableName}'", reader =>
			{
				changeTrackingEnabled = true;
			});
			if (changeTrackingEnabled)
			{
				Db.Connection.ExecuteNonQuery($"ALTER TABLE [dbo].[{tableName}] DISABLE CHANGE_TRACKING;");
			}

			foreach (var indexName in indexNames)
			{
				Db.Connection.ExecuteNonQuery($"DROP INDEX [{indexName}] ON [dbo].[{tableName}]");
			}

			Db.Connection.ExecuteNonQuery($"ALTER TABLE [dbo].[{tableName}] DROP CONSTRAINT [{constraintName}]");

			if (viewNames != null)
			{
				viewNames.ForEach(viewName => Db.Connection.ExecuteNonQuery($"DROP VIEW [{viewName}]"));
			}
		}

		int GetRowCount(string tableName)
		{
			return Db.Connection.ExecuteScalar<int>($"SELECT COUNT(3) FROM {tableName}");
		}

		string FindPartition(DateTime date)
		{
			string result = null;
			var partitionTime = date.AddMonths(1).ToString("yyyyMM", CultureInfo.InvariantCulture);
			var filegroupName = "CIHGROUP" + partitionTime;
			var script = string.Format(CultureInfo.InvariantCulture, "SELECT 1 FROM sys.filegroups WHERE name='{0}'", filegroupName);
			Db.Connection.ExecuteReader(script, (_) =>
			{
				result = filegroupName;
			});
			return result;
		}

		public static void PartitionTable(string tableName, string indexField, string schemeField, string indexName = null)
		{
			var sql = $@"
CREATE CLUSTERED INDEX [{indexName ?? "ClusteredIndex-DummyTest"}] ON [dbo].[{tableName}]
(
	[{indexField}] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = {(indexName == null ? "OFF" : "ON")}, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [CIHPARTITIONSCHEME]([{schemeField}])";
			Db.Connection.ExecuteNonQuery(sql);
		}

		protected override void SetUp()
		{
			adminConnection = Db.NewAdminConnection();
			snapshot = CargoWise.Data.Testing.SnapshotCreator.CreateSnapshot(adminConnection, () => Db.Connection.CloseConnection());
			CreatePartitionScheme();
			base.SetUp();
		}

		public static void CreatePartitionScheme()
		{
			var testDir = NUnit.Framework.TempForTest.TempPath;
			Db.Connection.ExecuteNonQuery($@"
ALTER DATABASE CURRENT ADD FILEGROUP [CIHGROUP201003]
ALTER DATABASE CURRENT ADD FILEGROUP [CIHGROUP201004]
ALTER DATABASE CURRENT ADD FILE ( NAME = N'CIHDATA201003', FILENAME = N'{testDir}\CIHDATA201003.ndf' , SIZE = 512KB , FILEGROWTH = 128KB ) TO FILEGROUP [CIHGROUP201003]
ALTER DATABASE CURRENT ADD FILE ( NAME = N'CIHDATA201004', FILENAME = N'{testDir}\CIHDATA201004.ndf' , SIZE = 512KB , FILEGROWTH = 128KB ) TO FILEGROUP [CIHGROUP201004]
CREATE PARTITION FUNCTION [CIHPARTITIONFUNCTION](DATETIME) AS RANGE LEFT FOR VALUES (NULL, '2010-04-01', '2010-05-01')
CREATE PARTITION SCHEME [CIHPARTITIONSCHEME] AS PARTITION [CIHPARTITIONFUNCTION] TO ([PRIMARY], [CIHGROUP201003], [CIHGROUP201004], [PRIMARY])
");
		}

		protected override void TearDown()
		{
			snapshot.Dispose();
			adminConnection.Dispose();
			base.TearDown();
			Db.Connection.EnsureIsOpen();
		}

		IDisposable snapshot;
		AdminConnection adminConnection;
		#endregion
	}
}
