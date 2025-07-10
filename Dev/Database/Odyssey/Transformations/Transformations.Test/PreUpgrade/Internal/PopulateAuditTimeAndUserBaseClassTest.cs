using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Database.Shared;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.PreUpgrade.Testing
{
	sealed class PopulateAuditTimeAndUserBaseClassTest : TransactionedTestCase
	{
		public void TestShouldProvideDefaultTimeIfLogMissing_False()
		{
			TestShouldProvideDefaultTimeIfLogMissing_Core(shouldProvideDefaultTimeIfLogMissing: false, expectedResult: "is null");
		}

		public void TestShouldProvideDefaultTimeIfLogMissing_True()
		{
			TestShouldProvideDefaultTimeIfLogMissing_Core(shouldProvideDefaultTimeIfLogMissing: true, expectedResult: "is not null");
		}

		public void TestUpdateRowsWhenNeededOnly()
		{
			CreateDummyTableWithOneRow();

			// create row with source to update
			var pk = Guid.NewGuid();
			Db.Connection.ExecuteNonQuery(@"
INSERT dbo.TestAuditTable (ZZZ_PK)
SELECT
	ZZZ_PK = @pk

INSERT dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_PostedTimeUtc, SL_GS_NKUser, SL_SE_NKEvent, SL_EventTime)
SELECT
	SL_PK              = NEWID()
	, SL_Table         = 'TestAuditTable'
	, SL_Parent        = @pk
	, SL_PostedTimeUtc = GETUTCDATE()
	, SL_GS_NKUser     = 'MSH'
	, SL_SE_NKEvent    = 'ADD'
	, SL_EventTime     = GETDATE()

"
				, cmd =>
				{
					cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				});

			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transformation = new PopulateAuditTimeAndUserForDummyTestAuditTableSchema();
			transformation.Initialise(new VersionLabel(0, 0), manager);
			transformation.ShouldProvideDefaultTimeIfLogMissingForTesting = false;
			transformation.ShouldOverwriteOldValues_ForTest = false;

			// first run - update some rows (different to defaults)
			transformation.Run();
			AssertContains("    1 row(s) updated", string.Join(System.Environment.NewLine, manager.OutputTextCollection.Cast<string>()));

			// second run - no updates
			manager.OutputTextCollection.Clear();
			transformation.Run();
			AssertContains("    0 row(s) updated", string.Join(System.Environment.NewLine, manager.OutputTextCollection.Cast<string>()));
		}

		public void TestTheOnlyOneDiffTriggersUpdate_CreateTime()
		{
			CreateDummyTableWithOneRow();

			// create row with source to update
			CreateOneRowWithSource(createTime: true);

			AssertTriggersUpdate();
		}

		public void TestTheOnlyOneDiffTriggersUpdate_CreateUser()
		{
			CreateDummyTableWithOneRow();

			// create row with source to update
			CreateOneRowWithSource(createUser: true);

			AssertTriggersUpdate();
		}

		public void TestTheOnlyOneDiffTriggersUpdate_EditTime()
		{
			CreateDummyTableWithOneRow();

			// create row with source to update
			CreateOneRowWithSource(editTime: true);

			AssertTriggersUpdate();
		}

		public void TestTheOnlyOneDiffTriggersUpdate_EditUser()
		{
			CreateDummyTableWithOneRow();

			// create row with source to update
			CreateOneRowWithSource(editUser: true);

			AssertTriggersUpdate();
		}

		void CreateOneRowWithSource(bool createTime = false, bool createUser = false, bool editTime = false, bool editUser = false)
		{
			var defaultTime = "NULL";
			var defaultUser = "''";
			var postedTime = "'2021-10-12'";
			var postedUser = "'MSH'";

			var pk = Guid.NewGuid();
			Db.Connection.ExecuteNonQuery($@"
INSERT dbo.TestAuditTable (ZZZ_PK, ZZZ_SystemCreateTimeUtc, ZZZ_SystemCreateUser, ZZZ_SystemLastEditTimeUtc, ZZZ_SystemLastEditUser)
SELECT
	ZZZ_PK = @pk
	, ZZZ_SystemCreateTimeUtc   = {(createTime ? defaultTime : postedTime)}
	, ZZZ_SystemCreateUser      = {(createUser ? defaultUser : postedUser)}
	, ZZZ_SystemLastEditTimeUtc = {(editTime ? defaultTime : postedTime)}
	, ZZZ_SystemLastEditUser    = {(editUser ? defaultUser : postedUser)}

INSERT dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_PostedTimeUtc, SL_GS_NKUser, SL_SE_NKEvent, SL_EventTime)
SELECT
	SL_PK              = NEWID()
	, SL_Table         = 'TestAuditTable'
	, SL_Parent        = @pk
	, SL_PostedTimeUtc = {postedTime}
	, SL_GS_NKUser     = {postedUser}
	, SL_SE_NKEvent    = 'ADD'
	, SL_EventTime     = GETDATE()

"
				, cmd =>
				{
					cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				});
		}

		void AssertTriggersUpdate()
		{
			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transformation = new PopulateAuditTimeAndUserForDummyTestAuditTableSchema();
			transformation.Initialise(new VersionLabel(0, 0), manager);
			transformation.ShouldProvideDefaultTimeIfLogMissingForTesting = false;
			transformation.ShouldOverwriteOldValues_ForTest = false;

			// first run - update some rows (different to defaults)
			transformation.Run();
			AssertContains("    1 row(s) updated", string.Join(System.Environment.NewLine, manager.OutputTextCollection.Cast<string>()));
		}

		void TestShouldProvideDefaultTimeIfLogMissing_Core(bool shouldProvideDefaultTimeIfLogMissing, string expectedResult)
		{
			CreateDummyTableWithOneRow();
			var transformation = new PopulateAuditTimeAndUserForDummyTestAuditTableSchema();
			transformation.Initialise(new VersionLabel(0, 0), new DummyUpgradeManager());

			AssertRowWithExpectedValueInDB("Precondition", DummyTestAuditTableSchema.Constants.ZZZ_SystemCreateTimeUtc, "is null");
			AssertRowWithExpectedValueInDB("Precondition", DummyTestAuditTableSchema.Constants.ZZZ_SystemLastEditTimeUtc, "is null");

			transformation.ShouldProvideDefaultTimeIfLogMissingForTesting = shouldProvideDefaultTimeIfLogMissing;
			transformation.Run();

			AssertRowWithExpectedValueInDB("Should only populate value when ProvideDefaultTimeIfLogMissing is true.", DummyTestAuditTableSchema.Constants.ZZZ_SystemCreateTimeUtc, expectedResult);
			AssertRowWithExpectedValueInDB("Should only populate value when ProvideDefaultTimeIfLogMissing is true.", DummyTestAuditTableSchema.Constants.ZZZ_SystemLastEditTimeUtc, expectedResult);
		}

		void AssertRowWithExpectedValueInDB(string message, string columnName, string expectedResult)
		{
			AssertEquals(message, 1, TestConnection.ExecuteScalar($"select Count(*) as result from TestAuditTable where {columnName} " + expectedResult));
		}

		public void TestShouldBeAbleToFilterRows_ToPopulate_BasedOnPassedSQL()
		{
			CreateDummyTableWithRows("A", "B", "A", "B", "B", "B");
			var transformation = new PopulateAuditTimeAndUserForDummyTestAuditTableSchema_WithRowFilter();

			AssertExpectedRowCount_WithExpectedValuesAndFilter("Precondition", expectedCount: 2, DummyTestAuditTableSchema.Constants.ZZZ_SystemCreateTimeUtc, expectedResult: "is null", filterValue: "A");
			AssertExpectedRowCount_WithExpectedValuesAndFilter("Precondition", expectedCount: 4, DummyTestAuditTableSchema.Constants.ZZZ_SystemCreateTimeUtc, expectedResult: "is null", filterValue: "B");

			AssertExpectedRowCount_WithExpectedValuesAndFilter("Precondition", expectedCount: 2, DummyTestAuditTableSchema.Constants.ZZZ_SystemLastEditTimeUtc, expectedResult: "is null", filterValue: "A");
			AssertExpectedRowCount_WithExpectedValuesAndFilter("Precondition", expectedCount: 4, DummyTestAuditTableSchema.Constants.ZZZ_SystemLastEditTimeUtc, expectedResult: "is null", filterValue: "B");

			transformation.RowFilterForTesting = @"ZZZ_FilterValue = 'B'";
			transformation.ShouldProvideDefaultTimeIfLogMissingForTesting = true; // so transfrom provided default values.
			transformation.Run();

			AssertExpectedRowCount_WithExpectedValuesAndFilter("Should NOT update unexpected filter values", expectedCount: 2, DummyTestAuditTableSchema.Constants.ZZZ_SystemCreateTimeUtc, expectedResult: "is null", filterValue: "A");
			AssertExpectedRowCount_WithExpectedValuesAndFilter("Should update expected filter values", expectedCount: 4, DummyTestAuditTableSchema.Constants.ZZZ_SystemCreateTimeUtc, expectedResult: "is not null", filterValue: "B");

			AssertExpectedRowCount_WithExpectedValuesAndFilter("Should NOT update expected filter values", expectedCount: 2, DummyTestAuditTableSchema.Constants.ZZZ_SystemLastEditTimeUtc, expectedResult: "is null", filterValue: "A");
			AssertExpectedRowCount_WithExpectedValuesAndFilter("Should update expected filter values", expectedCount: 4, DummyTestAuditTableSchema.Constants.ZZZ_SystemLastEditTimeUtc, expectedResult: "is not null", filterValue: "B");
		}

		void AssertExpectedRowCount_WithExpectedValuesAndFilter(string message, int expectedCount, string columnName, string expectedResult, string filterValue)
		{
			AssertEquals(message, expectedCount, TestConnection.ExecuteScalar($"select Count(*) as result from TestAuditTable where {columnName} {expectedResult} and ZZZ_FilterValue = '{filterValue}'"));
		}

		public void TestTransform_NullHighWatermark()
		{
			ExtProperty.Table.Delete(
				TestConnection,
				DummyTestAuditTableSchema.Instance.SqlSchemaName,
				DummyTestAuditTableSchema.Instance.TableName,
				HighWatermarks.PopulateAuditTimeAndUserHighWatermark);
			TestTransform_NoHighWatermark(false);
		}

		public void TestTransform_EmptyHighWatermark()
		{
			ExtProperty.Table.Update(
				TestConnection,
				DummyTestAuditTableSchema.Instance.SqlSchemaName,
				DummyTestAuditTableSchema.Instance.TableName,
				HighWatermarks.PopulateAuditTimeAndUserHighWatermark,
				string.Empty);
			TestTransform_NoHighWatermark(false);
		}

		public void TestTransform_NoHighWatermark_ShouldOverwriteOldValues()
		{
			ExtProperty.Table.Delete(
				TestConnection,
				DummyTestAuditTableSchema.Instance.SqlSchemaName,
				DummyTestAuditTableSchema.Instance.TableName,
				HighWatermarks.PopulateAuditTimeAndUserHighWatermark);
			TestTransform_NoHighWatermark(true);
		}

		void TestTransform_NoHighWatermark(bool shouldOverwriteOldValues)
		{
			// Arrange
			CreateDummyTable();

			var emptyRowPk1 = Guid.NewGuid();
			var emptyRowCreateTime1 = new DateTime(2019, 1, 2, 13, 45, 23, DateTimeKind.Utc);
			InsertEmptyRow(TestConnection, emptyRowPk1);
			InsertStmALogRow(TestConnection, emptyRowPk1, emptyRowCreateTime1, "E", "ADD");

			var emptyRowPk2 = Guid.NewGuid();
			var emptyRowCreateTime2 = new DateTime(2019, 11, 30, 1, 23, 45, DateTimeKind.Utc);
			InsertEmptyRow(TestConnection, emptyRowPk2);
			InsertStmALogRow(TestConnection, emptyRowPk2, emptyRowCreateTime2, "F", "ADD");

			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new PopulateAuditTimeAndUserForDummyTestAuditTableSchema();
			transform.Initialise(new VersionLabel(0, 0), manager);
			transform.ShouldOverwriteOldValues_ForTest = shouldOverwriteOldValues;
			transform.ShouldProvideDefaultTimeIfLogMissingForTesting = false;

			// Act
			transform.Run();

			// Assert
			AssertContains("Using No High Watermark", string.Join(System.Environment.NewLine, manager.OutputTextCollection.Cast<string>()));
			AssertRowCountUpdated(manager, 2);
			AssertRowDataIsExpected(emptyRowPk1, emptyRowCreateTime1, "E", emptyRowCreateTime1, "E");
			AssertRowDataIsExpected(emptyRowPk2, emptyRowCreateTime2, "F", emptyRowCreateTime2, "F");
		}

		public void TestTransform_NoHighWatermark_HasOldValues()
		{
			// Arrange
			ExtProperty.Table.Delete(
				TestConnection,
				DummyTestAuditTableSchema.Instance.SqlSchemaName,
				DummyTestAuditTableSchema.Instance.TableName,
				HighWatermarks.PopulateAuditTimeAndUserHighWatermark);
			CreateDummyTable();

			var notEmptyRowPk1 = Guid.NewGuid();
			var notEmptyRowCreateTime1 = new DateTime(2019, 1, 2, 13, 11, 21, DateTimeKind.Utc);
			var notEmptyRowEditTime1 = new DateTime(2020, 12, 23, 1, 2, 2, DateTimeKind.Utc);
			InsertRow(TestConnection, notEmptyRowPk1, notEmptyRowCreateTime1, "E", notEmptyRowCreateTime1, "E");
			InsertStmALogRow(TestConnection, notEmptyRowPk1, notEmptyRowEditTime1, "F", "EDT");

			var emptyRowPk2 = Guid.NewGuid();
			var emptyRowCreateTime2 = new DateTime(2019, 1, 2, 14, 12, 22, DateTimeKind.Utc);
			InsertEmptyRow(TestConnection, emptyRowPk2);
			InsertStmALogRow(TestConnection, emptyRowPk2, emptyRowCreateTime2, "E", "ADD");

			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new PopulateAuditTimeAndUserForDummyTestAuditTableSchema();
			transform.Initialise(new VersionLabel(0, 0), manager);
			transform.ShouldOverwriteOldValues_ForTest = false;
			transform.ShouldProvideDefaultTimeIfLogMissingForTesting = false;

			// Act
			transform.Run();

			// Assert
			AssertContains("Using No High Watermark", string.Join(System.Environment.NewLine, manager.OutputTextCollection.Cast<string>()));
			AssertRowCountUpdated(manager, 1);
			AssertRowDataIsExpected(notEmptyRowPk1, notEmptyRowCreateTime1, "E", notEmptyRowCreateTime1, "E"); // should not be overwritten
			AssertRowDataIsExpected(emptyRowPk2, emptyRowCreateTime2, "E", emptyRowCreateTime2, "E");
		}

		public void TestTransform_NoHighWatermark_HasOldValues_ShouldOverwriteOldValues()
		{
			// Arrange
			ExtProperty.Table.Delete(
				TestConnection,
				DummyTestAuditTableSchema.Instance.SqlSchemaName,
				DummyTestAuditTableSchema.Instance.TableName,
				HighWatermarks.PopulateAuditTimeAndUserHighWatermark);
			CreateDummyTable();

			var notEmptyRowPk1 = Guid.NewGuid();
			var notEmptyRowCreateTime1 = new DateTime(2019, 1, 2, 13, 11, 21, DateTimeKind.Utc);
			var notEmptyRowEditTime1 = new DateTime(2020, 12, 23, 1, 2, 2, DateTimeKind.Utc);
			InsertRow(TestConnection, notEmptyRowPk1, notEmptyRowCreateTime1, "E", notEmptyRowCreateTime1, "E");
			InsertStmALogRow(TestConnection, notEmptyRowPk1, notEmptyRowEditTime1, "F", "EDT");

			var emptyRowPk2 = Guid.NewGuid();
			var emptyRowCreateTime2 = new DateTime(2000, 1, 2, 14, 12, 22, DateTimeKind.Utc);
			InsertEmptyRow(TestConnection, emptyRowPk2);
			InsertStmALogRow(TestConnection, emptyRowPk2, emptyRowCreateTime2, "E", "ADD");

			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new PopulateAuditTimeAndUserForDummyTestAuditTableSchema();
			transform.Initialise(new VersionLabel(0, 0), manager);
			transform.ShouldOverwriteOldValues_ForTest = true;
			transform.ShouldProvideDefaultTimeIfLogMissingForTesting = false;

			// Act
			transform.Run();

			// Assert
			AssertContains("Using No High Watermark", string.Join(System.Environment.NewLine, manager.OutputTextCollection.Cast<string>()));
			AssertRowCountUpdated(manager, 2);
			AssertRowDataIsExpected(notEmptyRowPk1, notEmptyRowEditTime1, "F", notEmptyRowEditTime1, "F");
			AssertRowDataIsExpected(emptyRowPk2, emptyRowCreateTime2, "E", emptyRowCreateTime2, "E");
		}

		public void TestTransform_HasHighWatermarkRecorded()
		{
			TestTransform_HasHighWatermarkRecorded(false);
		}

		public void TestTransform_HasHighWatermarkRecorded_ShouldOverwriteOldValues()
		{
			TestTransform_HasHighWatermarkRecorded(true);
		}

		void TestTransform_HasHighWatermarkRecorded(bool shouldOverwriteOldValues)
		{
			// Arrange
			CreateDummyTable();

			var highWatermarkDateTime = new DateTime(2019, 1, 1, 4, 39, 50, DateTimeKind.Utc);
			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new PopulateAuditTimeAndUserForDummyTestAuditTableSchema();
			transform.Initialise(new VersionLabel(0, 0), manager);
			transform.ShouldOverwriteOldValues_ForTest = shouldOverwriteOldValues;
			transform.ShouldProvideDefaultTimeIfLogMissingForTesting = false;
			RecordHighWatermark(transform.TableSchema.SqlSchemaName, transform.TableSchema.TableName, highWatermarkDateTime);

			var oneMinute = TimeSpan.FromMinutes(1d);
			var oneHour = TimeSpan.FromHours(1d);
			var createTimes = new[]
			{
					highWatermarkDateTime - oneHour - oneHour,
					highWatermarkDateTime - oneHour - oneMinute,
					highWatermarkDateTime - oneHour,
					highWatermarkDateTime - oneHour + oneMinute,
					highWatermarkDateTime - oneMinute,
					highWatermarkDateTime,
					highWatermarkDateTime + oneMinute,
			};

			var timePks = new Dictionary<DateTime, Guid>();
			foreach (var createTime in createTimes)
			{
				var pk = Guid.NewGuid();
				timePks[createTime] = pk;
				InsertEmptyRow(TestConnection, pk);
				InsertStmALogRow(TestConnection, pk, createTime, "E", "ADD");
			}

			// Act
			transform.Run();

			// Assert
			AssertContains($"Using High Watermark: [2019-01-01T04:39:50]", string.Join(System.Environment.NewLine, manager.OutputTextCollection.Cast<string>()));
			AssertRowCountUpdated(manager, createTimes.Where(WillBeModified).Count());

			foreach (var createTime in createTimes)
			{
				var pk = timePks[createTime];
				if (WillBeModified(createTime))
				{
					AssertRowDataIsExpected(pk, createTime, "E", createTime, "E");
				}
				else
				{
					AssertRowDataIsEmpty(pk);
				}
			}

			bool WillBeModified(DateTime time) => time > highWatermarkDateTime - oneHour;
		}

		public void TestTransform_HasHighWatermarkRecorded_HasOldValues()
		{
			// Arrange
			CreateDummyTable();

			var highWatermarkDateTime = new DateTime(2019, 10, 16, 15, 54, 39, DateTimeKind.Utc);

			var notEmptyRowPk1 = Guid.NewGuid();
			var notEmptyRowCreateTime1 = new DateTime(2018, 2, 12, 14, 51, 1, DateTimeKind.Utc);
			var notEmptyEditTimeBeforeWatermark1 = highWatermarkDateTime - TimeSpan.FromHours(5d);
			InsertRow(TestConnection, notEmptyRowPk1, notEmptyRowCreateTime1, "E", notEmptyRowCreateTime1, "E");
			InsertStmALogRow(TestConnection, notEmptyRowPk1, notEmptyEditTimeBeforeWatermark1, "F", "EDT");

			var notEmptyRowPk2 = Guid.NewGuid();
			var notEmptyRowCreateTime2 = new DateTime(2018, 2, 12, 14, 51, 1, DateTimeKind.Utc);
			var notEmptyEditTimeAfterWatermark2 = highWatermarkDateTime + TimeSpan.FromHours(1d);
			InsertRow(TestConnection, notEmptyRowPk2, notEmptyRowCreateTime2, "E", notEmptyRowCreateTime2, "E");
			InsertStmALogRow(TestConnection, notEmptyRowPk2, notEmptyEditTimeAfterWatermark2, "F", "EDT");

			var emptyRowPk3 = Guid.NewGuid();
			var emptyRowCreateTimeBeforeWatermark3 = highWatermarkDateTime - TimeSpan.FromDays(10d);
			InsertEmptyRow(TestConnection, emptyRowPk3);
			InsertStmALogRow(TestConnection, emptyRowPk3, emptyRowCreateTimeBeforeWatermark3, "E", "ADD");

			var emptyRowPk4 = Guid.NewGuid();
			var emptyRowCreateTimeAfterWatermark4 = highWatermarkDateTime;
			InsertEmptyRow(TestConnection, emptyRowPk4);
			InsertStmALogRow(TestConnection, emptyRowPk4, emptyRowCreateTimeAfterWatermark4, "E", "ADD");

			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new PopulateAuditTimeAndUserForDummyTestAuditTableSchema();
			transform.Initialise(new VersionLabel(0, 0), manager);
			transform.ShouldOverwriteOldValues_ForTest = false;
			transform.ShouldProvideDefaultTimeIfLogMissingForTesting = false;
			RecordHighWatermark(transform.TableSchema.SqlSchemaName, transform.TableSchema.TableName, highWatermarkDateTime);

			// Act
			transform.Run();

			// Assert
			AssertContains($"Using High Watermark: [2019-10-16T15:54:39]", string.Join(System.Environment.NewLine, manager.OutputTextCollection.Cast<string>()));
			AssertRowCountUpdated(manager, 2);
			AssertRowDataIsExpected(notEmptyRowPk1, notEmptyRowCreateTime1, "E", notEmptyRowCreateTime1, "E"); // not overwritten due to before high watermark
			AssertRowDataIsExpected(notEmptyRowPk2, notEmptyRowCreateTime2, "E", notEmptyEditTimeAfterWatermark2, "F");
			AssertRowDataIsEmpty(emptyRowPk3); // before high watermark
			AssertRowDataIsExpected(emptyRowPk4, emptyRowCreateTimeAfterWatermark4, "E", emptyRowCreateTimeAfterWatermark4, "E");
		}

		public void TestTransform_HasHighWatermarkRecorded_HasOldValues_ShouldOverwriteOldValues()
		{
			// Arrange
			CreateDummyTable();

			var highWatermarkDateTime = new DateTime(2019, 10, 16, 15, 54, 39, DateTimeKind.Utc);

			var notEmptyRowPk1 = Guid.NewGuid();
			var notEmptyRowCreateTime1 = new DateTime(2018, 2, 12, 14, 51, 1, DateTimeKind.Utc);
			var notEmptyEditTimeBeforeWatermark1 = highWatermarkDateTime - TimeSpan.FromHours(2d);
			InsertRow(TestConnection, notEmptyRowPk1, notEmptyRowCreateTime1, "E", notEmptyRowCreateTime1, "E");
			InsertStmALogRow(TestConnection, notEmptyRowPk1, notEmptyEditTimeBeforeWatermark1, "F", "EDT");

			var notEmptyRowPk2 = Guid.NewGuid();
			var notEmptyRowCreateTime2 = new DateTime(2018, 2, 12, 14, 51, 1, DateTimeKind.Utc);
			var notEmptyEditTimeAfterWatermark2 = highWatermarkDateTime + TimeSpan.FromHours(1d);
			InsertRow(TestConnection, notEmptyRowPk2, notEmptyRowCreateTime2, "E", notEmptyRowCreateTime2, "E");
			InsertStmALogRow(TestConnection, notEmptyRowPk2, notEmptyEditTimeAfterWatermark2, "F", "EDT");

			var emptyRowPk3 = Guid.NewGuid();
			var emptyRowCreateTimeBeforeWatermark3 = highWatermarkDateTime - TimeSpan.FromDays(10d);
			InsertEmptyRow(TestConnection, emptyRowPk3);
			InsertStmALogRow(TestConnection, emptyRowPk3, emptyRowCreateTimeBeforeWatermark3, "E", "ADD");

			var emptyRowPk4 = Guid.NewGuid();
			var emptyRowCreateTimeAfterWatermark4 = highWatermarkDateTime;
			InsertEmptyRow(TestConnection, emptyRowPk4);
			InsertStmALogRow(TestConnection, emptyRowPk4, emptyRowCreateTimeAfterWatermark4, "E", "ADD");

			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new PopulateAuditTimeAndUserForDummyTestAuditTableSchema();
			transform.Initialise(new VersionLabel(0, 0), manager);
			transform.ShouldOverwriteOldValues_ForTest = true;
			transform.ShouldProvideDefaultTimeIfLogMissingForTesting = false;
			RecordHighWatermark(transform.TableSchema.SqlSchemaName, transform.TableSchema.TableName, highWatermarkDateTime);

			// Act
			transform.Run();

			// Assert
			AssertContains($"Using High Watermark: [2019-10-16T15:54:39]", string.Join(System.Environment.NewLine, manager.OutputTextCollection.Cast<string>()));
			AssertRowCountUpdated(manager, 2);
			AssertRowDataIsExpected(notEmptyRowPk1, notEmptyRowCreateTime1, "E", notEmptyRowCreateTime1, "E"); // not overwritten due to before high watermark
			AssertRowDataIsExpected(notEmptyRowPk2, notEmptyEditTimeAfterWatermark2, "F", notEmptyEditTimeAfterWatermark2, "F");
			AssertRowDataIsEmpty(emptyRowPk3); // before high watermark
			AssertRowDataIsExpected(emptyRowPk4, emptyRowCreateTimeAfterWatermark4, "E", emptyRowCreateTimeAfterWatermark4, "E");
		}

		static void AssertRowCountUpdated(UpgradeManagerForTestWithOutputBuffer manager, int rowCountUpdated)
		{
			AssertContains(
				$"    {rowCountUpdated} row(s) updated",
				string.Join(System.Environment.NewLine, manager.OutputTextCollection.Cast<string>()));
		}

		void AssertRowDataIsExpected(
			Guid pk,
			DateTime createTime,
			string createUser,
			DateTime lastEditTime,
			string lastEditUser)
		{
			var result = TestConnection.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM TestAuditTable
WHERE
	ZZZ_PK = @pk
	AND ZZZ_SystemCreateTimeUtc = @createTime
	AND ZZZ_SystemCreateUser = @createUser
	AND ZZZ_SystemLastEditTimeUtc = @lastEditTime
	AND ZZZ_SystemLastEditUser = @lastEditUser
",
				cmd =>
				{
					cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
					cmd.AddParameter("@createTime", SqlDbType.SmallDateTime, createTime);
					cmd.AddParameter("@createUser", SqlDbType.VarChar, createUser);
					cmd.AddParameter("@lastEditTime", SqlDbType.SmallDateTime, lastEditTime);
					cmd.AddParameter("@lastEditUser", SqlDbType.VarChar, lastEditUser);
				});
			AssertEquals(1, result);
		}

		void AssertRowDataIsEmpty(Guid pk)
		{
			var result = TestConnection.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM TestAuditTable
WHERE
	ZZZ_PK = @pk
	AND ZZZ_SystemCreateTimeUtc is null
	AND ZZZ_SystemCreateUser = ''
	AND ZZZ_SystemLastEditTimeUtc is null
	AND ZZZ_SystemLastEditUser = ''
",
				cmd => cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk));
			AssertEquals(1, result);
		}

		void RecordHighWatermark(string schemaName, string tableName, DateTimeOffset dateTimeOffset)
		{
			ExtProperty.Table.Update(
				TestConnection,
				schemaName,
				tableName,
				HighWatermarks.PopulateAuditTimeAndUserHighWatermark,
				dateTimeOffset.UtcDateTime.ToString("s"));
		}

		#region CreateDummyTableWithRows

		void CreateDummyTableWithOneRow()
		{
			CreateDummyTableWithRows("A");
		}

		void CreateDummyTableWithRows(params string[] filterValues)
		{
			if (filterValues.Length > 0)
			{
				CreateDummyTable();

				var sql = new SqlQueryBuilder();

				foreach (var filterValue in filterValues)
				{
					sql.Append(string.Format($@"
INSERT INTO [dbo].[TestAuditTable]
	(ZZZ_PK, ZZZ_FilterValue, ZZZ_SystemCreateUser, ZZZ_SystemLastEditUser)
VALUES
	(NEWID(), '{filterValue}', '~BP', '~BP')
"));
				}

				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		void CreateDummyTable()
		{
			CreateDummyTable(TestConnection);
		}

		internal static void CreateDummyTable(DbConnection connection)
		{
			connection.ExecuteNonQuery(@"
CREATE TABLE [dbo].[TestAuditTable]
(
	[ZZZ_PK]                    uniqueidentifier NOT NULL,
	[ZZZ_FilterValue]           varchar(3)       NOT NULL DEFAULT 'A',
	[ZZZ_SystemCreateTimeUtc]   smalldatetime        NULL,
	[ZZZ_SystemCreateUser]      varchar(3)       NOT NULL DEFAULT '',
	[ZZZ_SystemLastEditTimeUtc] smalldatetime        NULL,
	[ZZZ_SystemLastEditUser]    varchar(3)       NOT NULL DEFAULT '',
)");
		}

		internal static void InsertEmptyRow(DbConnection connection, Guid pk)
		{
			InsertRow(connection, pk, null, string.Empty, null, string.Empty);
		}

		static void InsertRow(
			DbConnection connection,
			Guid pk,
			DateTime? createTime,
			string createUser,
			DateTime? lastEditTime,
			string lastEditUser)
		{
			connection.ExecuteNonQuery(@"
INSERT dbo.TestAuditTable
	(ZZZ_PK
	,ZZZ_SystemCreateTimeUtc
	,ZZZ_SystemCreateUser
	,ZZZ_SystemLastEditTimeUtc
	,ZZZ_SystemLastEditUser)
VALUES
	(@pk
	,@createTime
	,@createUser
	,@lastEditTime
	,@lastEditUser)",
				cmd =>
				{
					cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
					cmd.AddParameter("@createTime", SqlDbType.SmallDateTime, (object)createTime ?? DBNull.Value);
					cmd.AddParameter("@createUser", SqlDbType.VarChar, createUser);
					cmd.AddParameter("@lastEditTime", SqlDbType.SmallDateTime, (object)lastEditTime ?? DBNull.Value);
					cmd.AddParameter("@lastEditUser", SqlDbType.VarChar, lastEditUser);
				});
		}

		static void InsertStmALogRow(
			DbConnection connection,
			Guid pk,
			DateTime postedTime,
			string user,
			string logEvent)
		{
			connection.ExecuteNonQuery($@"
INSERT dbo.StmALog
	(SL_PK
	,SL_Table
	,SL_Parent
	,SL_PostedTimeUtc
	,SL_GS_NKUser
	,SL_SE_NKEvent
	,SL_EventTime)
VALUES
	(NEWID()
	,'TestAuditTable'
	,@pk
	,@postedTime
	,@user
	,@logEvent
	,GETDATE())",
				cmd =>
				{
					cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
					cmd.AddParameter("@postedTime", SqlDbType.DateTime, postedTime.ToUniversalTime());
					cmd.AddParameter("@user", SqlDbType.Char, user);
					cmd.AddParameter("@logEvent", SqlDbType.Char, logEvent);
				});
		}

		#endregion // CreateDummyTableWithRows

		#region PopulateAuditTimeAndUserForDummyTestAuditTableSchema

		public class PopulateAuditTimeAndUserForDummyTestAuditTableSchema : PopulateAuditTimeAndUser
		{
			public override ITableSchema TableSchema => new DummyTestAuditTableSchema();

			protected override bool ShouldProvideDefaultTimeIfLogMissing => ShouldProvideDefaultTimeIfLogMissingForTesting;

			public bool ShouldProvideDefaultTimeIfLogMissingForTesting { get; set; }

			protected override ITableSchema GetTableSchema(string tableName) => DummyTestAuditTableSchema.Instance;

			protected override string GetColumnNamePrefix(string tableName) => DummyTestAuditTableSchema.Constants.Prefix;

			public override bool ShouldOverwriteOldValues => ShouldOverwriteOldValues_ForTest;

			public bool ShouldOverwriteOldValues_ForTest { get; set; } = true;
		}

		public class PopulateAuditTimeAndUserForDummyTestAuditTableSchema_WithRowFilter : PopulateAuditTimeAndUserForDummyTestAuditTableSchema
		{
			protected override string RowFilter => RowFilterForTesting;

			public string RowFilterForTesting { get; set; }
		}

		#endregion

		#region DummyTestAuditTableSchema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Testing")]
		public sealed class DummyTestAuditTableSchema : CargoWise.Schema.Schema, ITableSchema
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline")]
			static DummyTestAuditTableSchema()
			{
				var column = 0;
				Instance = new DummyTestAuditTableSchema();
				PK = new SchemaPKColumn(Instance, Constants.PK, true);
				ZZZ_FilterValue = new SchemaStringColumn(Instance, Constants.ZZZ_FilterValue, column++, SqlDbType.VarChar, "", !IsNullable, 3, false, false, TVPHelper.TVP_varchar);
				ZZZ_SystemCreateTimeUtc = new SchemaDateTimeColumn(Instance, Constants.ZZZ_SystemCreateTimeUtc, column++, SqlDbType.SmallDateTime, DBNull.Value, !IsNullable, false, "dbo.TVP_smalldatetime");
				ZZZ_SystemCreateUser = new SchemaStringColumn(Instance, Constants.ZZZ_SystemCreateUser, column++, SqlDbType.VarChar, "", !IsNullable, 3, false, false, TVPHelper.TVP_varchar);
				ZZZ_SystemLastEditTimeUtc = new SchemaDateTimeColumn(Instance, Constants.ZZZ_SystemLastEditTimeUtc, column++, SqlDbType.SmallDateTime, DBNull.Value, !IsNullable, false, "dbo.TVP_smalldatetime");
				ZZZ_SystemLastEditUser = new SchemaStringColumn(Instance, Constants.ZZZ_SystemLastEditUser, column++, SqlDbType.VarChar, "", !IsNullable, 3, false, false, TVPHelper.TVP_varchar);
			}

			public static readonly SchemaPKColumn PK;
			public static readonly SchemaStringColumn ZZZ_FilterValue;
			public static readonly SchemaDateTimeColumn ZZZ_SystemCreateTimeUtc;
			public static readonly SchemaStringColumn ZZZ_SystemCreateUser;
			public static readonly SchemaDateTimeColumn ZZZ_SystemLastEditTimeUtc;
			public static readonly SchemaStringColumn ZZZ_SystemLastEditUser;
			public static readonly DummyTestAuditTableSchema Instance;

			public string TableName => "TestAuditTable";

			public SchemaColumnCollection All => new SchemaColumnCollection(new SchemaColumn[] { PK, ZZZ_FilterValue, ZZZ_SystemCreateTimeUtc, ZZZ_SystemCreateUser, ZZZ_SystemLastEditTimeUtc, ZZZ_SystemLastEditUser });

			SchemaPKColumn ITableSchema.PK => PK;

			public string SqlSchemaName => Constants.SqlSchemaName;

			#region NotImplemented

			public SchemaColumn GetSchemaColumn(string columnName) => throw new NotImplementedException();

			public string PkIndexName => throw new NotImplementedException();

			#endregion

			public static class Constants
			{
				public const string SqlSchemaName = "dbo";
				public const string Prefix = "ZZZ";
				public const string PK = "ZZZ_PK";
				public const string ZZZ_FilterValue = "ZZZ_FilterValue";
				public const string ZZZ_SystemCreateTimeUtc = "ZZZ_SystemCreateTimeUtc";
				public const string ZZZ_SystemCreateUser = "ZZZ_SystemCreateUser";
				public const string ZZZ_SystemLastEditTimeUtc = "ZZZ_SystemLastEditTimeUtc";
				public const string ZZZ_SystemLastEditUser = "ZZZ_SystemLastEditUser";
			}
		}

		#endregion
	}
}
