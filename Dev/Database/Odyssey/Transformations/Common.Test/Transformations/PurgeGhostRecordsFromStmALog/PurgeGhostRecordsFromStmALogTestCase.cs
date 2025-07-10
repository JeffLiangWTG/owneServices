using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.Common.Testing
{
	public abstract class PurgeGhostRecordsFromStmALogTestCase : TransactionedTestCase // to be replaced with DataTransformationTestCase
	{
		#region Testing With A Small And Existing Parent Table

		public void TestPurgeGhostRecords_WhenParentTableIsSmallAndPresent()
		{
			if (!ParentTableExists)
			{
				Assert(true);
				return;
			}

			PrepareForTestingWithSmallAndPresentParentTable();

			AddStmALogRecord(GetParentTableName(), DateTime.UtcNow, parentPKs.First());
			AddStmALogRecord("ProcessHeaders", DateTime.UtcNow);

			GetNewTestTransformationInstance().Run();

			AssertEquals(false, Db.Connection.Exists($"FROM StmALog WHERE SL_Table = '{GetParentTableName()}'"));
			AssertEquals(true, Db.Connection.Exists("FROM StmALog WHERE SL_Table = 'ProcessHeaders'"));
		}

		public void TestShouldSearchRecordsByPKs_WhenParentTableIsSmallAndPresent()
		{
			if (!ParentTableExists)
			{
				Assert(true);
				return;
			}

			PrepareForTestingWithSmallAndPresentParentTable();

			var executedCommand = GetSingleExecutedCommand();
			AssertContains("SL_Parent IN", executedCommand);
		}

		#endregion

		#region Testing With A Big Or Absent Parent Table

		public void TestPurgeGhostRecords_WhenParentTableIsBigOrAbsent()
		{
			PrepareForTestingWithBigOrAbsentParentTable();

			AddStmALogRecord(GetParentTableName(), DateTime.UtcNow);
			AddStmALogRecord("ProcessHeaders", DateTime.UtcNow);

			GetNewTestTransformationInstance().Run();

			AssertEquals(false, Db.Connection.Exists($"FROM StmALog WHERE SL_Table = '{GetParentTableName()}'"));
			AssertEquals(true, Db.Connection.Exists("FROM StmALog WHERE SL_Table = 'ProcessHeaders'"));
		}

		public void TestShouldSucceed_WhenCancelledBeforeCompletingFirstBatchAndThenRestarted_WhenParentTableIsBigOrAbsent()
		{
			PrepareForTestingWithBigOrAbsentParentTable();

			AddStmALogRecord(GetParentTableName(), DateTime.UtcNow);

			AssertExceptionThrown<OperationCanceledException>(() => GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, new CancellationToken(canceled: true)));
			AssertEquals(true, Db.Connection.Exists($"FROM StmALog WHERE SL_Table = '{GetParentTableName()}'"));

			GetNewTestTransformationInstance().Run();
			AssertEquals(false, Db.Connection.Exists($"FROM StmALog WHERE SL_Table = '{GetParentTableName()}'"));
		}

		public void TestShouldNotProcessRecordsFromTheBeginningOnEachRun_WhenParentTableIsBigOrAbsent()
		{
			PrepareForTestingWithBigOrAbsentParentTable();

			var pk1 = AddStmALogRecord(GetParentTableName(), DateTime.UtcNow.AddDays(-3));
			var pk2 = AddStmALogRecord(GetParentTableName(), DateTime.UtcNow.AddDays(-2));
			var pk3 = AddStmALogRecord(GetParentTableName(), DateTime.UtcNow.AddDays(-1));

			var source = new CancellationTokenSource();
			var token = source.Token;
			var transform = GetNewTestTransformationInstance();
			transform.BatchProcessed += (sender, args) => { source.Cancel(); };
			AssertExceptionThrown<OperationCanceledException>(() => transform.Run(TransformationSection.OnlinePostUpgrade, token));
			AssertContainsExactElementsInAnyOrder("There should be some records left after the first batch", new[] { pk2, pk3 }, GetStmALogRecords());

			var pk4 = AddStmALogRecord(GetParentTableName(), DateTime.UtcNow.AddDays(-5));
			GetNewTestTransformationInstance().Run();
			AssertContainsExactElementsInAnyOrder("Should not process from the beginning of the table (the newly added record should stay unprocessed)", new[] { pk4 }, GetStmALogRecords());

			GetNewTestTransformationInstance().Run();
			AssertEquals("When all batches are done, the watermark should be removed (the new run should process everything)", false, Db.Connection.Exists("FROM StmALog WHERE SL_Table = 'ProcessTasks'"));
		}

		IEnumerable<Guid> GetStmALogRecords()
		{
			var pks = new List<Guid>();
			Db.Connection.ExecuteReader($@"SELECT SL_PK FROM StmALog
WHERE SL_Table = '{GetParentTableName()}'",
				record => pks.Add(record.GetGuid(0)));
			return pks;
		}

		public void TestShouldSearchRecordsByTableName_WhenParentTableIsBigOrAbsent()
		{
			PrepareForTestingWithBigOrAbsentParentTable();

			var executedCommand = GetSingleExecutedCommand();
			AssertContains("SL_Table = ", executedCommand);
		}

		#endregion

		#region Implementation

		protected abstract PurgeGhostRecordsFromStmALog GetNewTestTransformationInstance();
		protected bool ParentTableExists => GetNewTestTransformationInstance().ParentTableExists;
		protected abstract string GetParentTableName();
		protected virtual string GetParentTableColumnNames() => null;
		protected virtual string GetValuesToInsertIntoParentTable(Guid pk, int i) => null;

		protected Guid AddStmALogRecord(string parentTableName, DateTime time, Guid? parentPK = null)
		{
			var pk = Guid.NewGuid();
			var sqlTime = time.ToSqlFormat();
			var sql = $@"
INSERT INTO StmALog (SL_PK,SL_Table,SL_Parent,SL_IsEstimate,SL_IsCancelled,SL_Reference,SL_PostedTimeUtc,SL_EventTime,SL_GS_NKUser,SL_SE_NKEvent,SL_GB_NKBranch,SL_GE_NKDepartment,SL_FireWorkflow,SL_DataSource,SL_EventTimeUtc) VALUES
('{pk}', '{parentTableName}', '{parentPK ?? Guid.NewGuid()}', 'N', 'N', '', '{sqlTime}', '{sqlTime}', '~BP', 'ADD', 'BNE', 'BRN', 0, 'C', null)
";
			Db.Connection.ExecuteNonQuery(sql);
			return pk;
		}

		protected string GetSingleExecutedCommand()
		{
			AddStmALogRecord("ProcessHeaders", DateTime.UtcNow); // need to have at least one row in StmALog to make purge by table code work

			var source = new CancellationTokenSource();
			var token = source.Token;
			var transform = GetNewTestTransformationInstance();
			transform.BatchProcessed += (sender, args) => { source.Cancel(); };

			using (Db.Connection.TrackExecutedCommands())
			{
				try
				{
					transform.Run(TransformationSection.OnlinePostUpgrade, token);
				}
				catch (OperationCanceledException)
				{
				}
				var executedCommand = Db.Connection.ExecutedCommands.SingleOrDefault(c => c.Contains("DELETE FROM dbo.StmALog"));

				AssertNotNull(executedCommand);

				return executedCommand;
			}
		}

		#region Preparing Data For Test Methods

		IReadOnlyCollection<Guid> parentPKs;

		void PrepareForTestingWithSmallAndPresentParentTable()
		{
			Assert("Must not run this when the parent table is missing", ParentTableExists);
			parentPKs = InsertMultipleRecordsIntoParentTable(3);
		}

		void PrepareForTestingWithBigOrAbsentParentTable()
		{
			if (ParentTableExists)
			{
				InsertMultipleRecordsIntoParentTable(1000); // the transformation considers a table to be big when there are more than 800 records, but it uses DataUtils.GetApproximateRowCountForTable which is not 100% accurate, so we need to add a bit more records than that max value just to be sure
			}
		}

		protected IReadOnlyCollection<Guid> InsertMultipleRecordsIntoParentTable(int count)
		{
			var pks = new List<Guid>();
			var parentTableColumnNames = GetParentTableColumnNames();
			AssertNotNullOrEmpty("Parent table column names should be specified when the parent table exists", parentTableColumnNames);
			var sqlBuilder = new StringBuilder($@"INSERT INTO {GetParentTableName()}
({GetParentTableColumnNames()})
VALUES
");

			for (int i = 0; i < count; i++)
			{
				var pk = Guid.NewGuid();
				pks.Add(pk);
				var values = GetValuesToInsertIntoParentTable(pk, i);
				AssertNotNullOrEmpty("Values to insert into the parent table should be specified when the parent table exists", values);
				sqlBuilder.AppendLine($"({values}){(i < count - 1 ? "," : "")}");
			}

			var sql = sqlBuilder.ToString();
			Db.Connection.ExecuteNonQuery(sql);

			return pks;
		}

		#endregion

		#endregion

		#region Setup and Tear Down

		protected virtual void InsertObjectsRelatedToParentRows()
		{
		}

		protected override void SetUp()
		{
			base.SetUp();

			Db.Connection.ExecuteNonQuery("DELETE FROM StmALog");

			if (ParentTableExists)
			{
				Db.Connection.ExecuteNonQuery($"DELETE FROM {GetParentTableName()}");
				InsertObjectsRelatedToParentRows();
			}
		}

		#endregion
	}
}
