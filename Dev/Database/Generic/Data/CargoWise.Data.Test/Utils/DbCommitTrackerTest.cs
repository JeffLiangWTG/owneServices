using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	[UseSnapshotProtection]
	sealed class DbCommitTrackerTest : TestCase
	{
		public void TestTracksInsertUpdate()
		{
			Db.Connection.ExecuteNonQuery("inseRt into dbo.DummyBizo (Z0_PK) values (NEWID())");
			AssertContainsExactElementsInAnyOrder(["inseRt into dbo.DummyBizo (Z0_PK) values (NEWID())"], CommittedUpdatesExcludingBackgroundCommandsNormalized);
		}

		[UseSnapshotProtection([DatabaseType.Main, DatabaseType.SingleSharedRef])]
		public void TestTrackInsertOnSynonymsTable()
		{
			const string sql = "inseRt into dbo.RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description) values (NEWID(), 'EUN', 'European Union')";
			Db.Connection.ExecuteNonQuery(sql);
			AssertContainsExactElementsInAnyOrder(
				[$"Database: {RefDbTableNameResolver.SingleRefDatabaseName}  {sql}",
				$"Database: {Db.Connection.CurrentDatabase}  {sql}"],
				CommittedUpdatesExcludingBackgroundCommandsSingleLine);
		}

		public void TestTrackInsertUsingSp_executesql()
		{
			const string sql = "exec sp_executesql N'inseRt into dbo.DummyBizo (Z0_PK) values (NEWID())'";
			Db.Connection.ExecuteNonQuery(sql);
			AssertContainsExactElementsInAnyOrder([sql], CommittedUpdatesExcludingBackgroundCommandsNormalized);
		}

		public void TestTracksUpdate()
		{
			Db.Connection.ExecuteNonQuery("uPdate dbo.DummyBizo set Z0_Code = 'ABC' where Z0_PK = NEWID()");
			AssertContainsExactElementsInAnyOrder(["uPdate dbo.DummyBizo set Z0_Code = 'ABC' where Z0_PK = NEWID()"], CommittedUpdatesExcludingBackgroundCommandsNormalized);
		}

		[UseSnapshotProtection([DatabaseType.Main, DatabaseType.SingleSharedRef])]
		public void TestTrackUpdateOnSynonymsTable()
		{
			const string sqlInsert = "inseRt into dbo.RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description) values (NEWID(), 'EUN', 'European Union')";
			const string sqlUpdate = "uPdate dbo.RefDatabase_RefDataGrouping set ZZZ_Description = 'EU' where ZZZ_DataGrouping = 'EUN'";

			Db.Connection.ExecuteNonQuery(sqlInsert);
			Db.Connection.ExecuteNonQuery(sqlUpdate);

			AssertContainsExactElementsInAnyOrder(
				[$"Database: {RefDbTableNameResolver.SingleRefDatabaseName}  {sqlInsert}",
				$"Database: {RefDbTableNameResolver.SingleRefDatabaseName}  {sqlUpdate}",
				$"Database: {Db.Connection.CurrentDatabase}  {sqlInsert}",
				$"Database: {Db.Connection.CurrentDatabase}  {sqlUpdate}"],
				CommittedUpdatesExcludingBackgroundCommandsSingleLine);
		}

		public void TestTracksDelete()
		{
			Db.Connection.ExecuteNonQuery("deleTe from dbo.DummyBizo where Z0_PK = NEWID()");
			AssertContainsExactElementsInAnyOrder(["deleTe from dbo.DummyBizo where Z0_PK = NEWID()"], CommittedUpdatesExcludingBackgroundCommandsNormalized);
		}

		[UseSnapshotProtection([DatabaseType.Main, DatabaseType.SingleSharedRef])]
		public void TestTrackDeleteOnSynonymsTable()
		{
			const string sqlInsert = "inseRt into dbo.RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description) values (NEWID(), 'EUN', 'European Union')";
			const string sqlDelete = "deleTe from dbo.RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'EUN'";

			Db.Connection.ExecuteNonQuery(sqlInsert);
			Db.Connection.ExecuteNonQuery(sqlDelete);

			AssertContainsExactElementsInAnyOrder(
				[$"Database: {RefDbTableNameResolver.SingleRefDatabaseName}  {sqlInsert}",
				$"Database: {RefDbTableNameResolver.SingleRefDatabaseName}  {sqlDelete}",
				$"Database: {Db.Connection.CurrentDatabase}  {sqlInsert}",
				$"Database: {Db.Connection.CurrentDatabase}  {sqlDelete}"],
				CommittedUpdatesExcludingBackgroundCommandsSingleLine);
		}

		public void TestDoesNotTrackSelect()
		{
			Db.Connection.ExecuteScalar("select top 1 datalength(SU_ActionDataUpdateBlob) from dbo.StmMenuItem");
			AssertContainsExactElementsInAnyOrder([], CommittedUpdatesExcludingBackgroundCommandsNormalized);
		}

		public void TestTransaction()
		{
			Db.Connection.BeginTransaction();
			Db.Connection.ExecuteNonQuery("insert into dbo.DummyBizo (Z0_PK) values ('3C57AAFA-E64A-4996-A617-DFD00BCE868D')");
			Db.Connection.CommitTransaction();
			AssertContainsExactElementsInAnyOrder(["insert into dbo.DummyBizo (Z0_PK) values ('3C57AAFA-E64A-4996-A617-DFD00BCE868D')"], CommittedUpdatesExcludingBackgroundCommandsNormalized);

			Db.Connection.BeginTransaction();
			Db.Connection.ExecuteNonQuery("insert into dbo.DummyBizo (Z0_PK) values ('1EF53EEE-6BC7-42A3-98A8-D82138749F3F')");
			Db.Connection.RollbackTransaction();
			AssertContainsExactElementsInAnyOrder(["insert into dbo.DummyBizo (Z0_PK) values ('3C57AAFA-E64A-4996-A617-DFD00BCE868D')"], CommittedUpdatesExcludingBackgroundCommandsNormalized);
		}

		public void TestTracksMultipleConnections()
		{
			Db.Connection.ExecuteNonQuery("insert into dbo.DummyBizo (Z0_PK) values ('3C57AAFA-E64A-4996-A617-DFD00BCE868D')");
			AssertContainsExactElementsInAnyOrder(["insert into dbo.DummyBizo (Z0_PK) values ('3C57AAFA-E64A-4996-A617-DFD00BCE868D')"], CommittedUpdatesExcludingBackgroundCommandsNormalized);
			using (var conn1 = Db.NewAdminConnection())
			using (var conn2 = Db.NewAdminConnection())
			{
				conn1.BeginTransaction();
				conn2.BeginTransaction();
				conn1.ExecuteNonQuery("insert into dbo.DummyBizo (Z0_PK) values ('1EF53EEE-6BC7-42A3-98A8-D82138749F3F')");
				conn2.ExecuteNonQuery("insert into dbo.DummyBizo (Z0_PK) values ('665315D3-41F4-4C6F-8EA3-E9EC9D8CEB6E')");
				conn1.CommitTransaction();
				conn2.RollbackTransaction();
				AssertContainsExactElementsInAnyOrder(["insert into dbo.DummyBizo (Z0_PK) values ('3C57AAFA-E64A-4996-A617-DFD00BCE868D')", "insert into dbo.DummyBizo (Z0_PK) values ('1EF53EEE-6BC7-42A3-98A8-D82138749F3F')"], CommittedUpdatesExcludingBackgroundCommandsNormalized);
			}
		}

		public void TestPrintsParameters()
		{
			var pk = new Guid("960b94da-6f87-4e09-b2cf-77bf434e5b82");
			Db.Connection.ExecuteNonQuery("insert into dbo.DummyBizo (Z0_PK, Z0_Code) values (@pk, @myParam)", (cmd) =>
			{
				cmd.AddParameter("@pk", sqlDbType: SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@myParam", sqlDbType: SqlDbType.VarChar, "TEST");
			});

			AssertContainsExactElementsInAnyOrder(
			[
				$"""insert into dbo.DummyBizo (Z0_PK, Z0_Code) values (@pk, @myParam){Environment.NewLine}{Environment.NewLine}Parameters:{Environment.NewLine}   [@pk = "960b94da-6f87-4e09-b2cf-77bf434e5b82", UniqueIdentifier(0)]{Environment.NewLine}   [@myParam = "TEST", VarChar(4)]"""
			], CommittedUpdatesExcludingBackgroundCommandsNormalized);
		}

		public void TestDoNotTrackOnDeleteCascade()
		{
			var parentTableName = $"ParentTable_{Guid.NewGuid():N}";
			var childTableName = $"ChildTable_{Guid.NewGuid():N}";

			var sql = $@"
				CREATE TABLE dbo.{parentTableName} (
					ParentID UNIQUEIDENTIFIER PRIMARY KEY,
					ParentData NVARCHAR(50)
				);

				CREATE TABLE dbo.{childTableName} (
					ChildID UNIQUEIDENTIFIER PRIMARY KEY,
					ParentID UNIQUEIDENTIFIER,
					ChildData NVARCHAR(50),
					CONSTRAINT FK_{childTableName}_{parentTableName} FOREIGN KEY (ParentID)
						REFERENCES dbo.{parentTableName} (ParentID)
						ON DELETE CASCADE
				);
			";

			Db.Connection.ExecuteNonQuery(sql);

			AssertContainsExactElementsInAnyOrder([], CommittedUpdatesExcludingBackgroundCommandsNormalized);
		}

		IEnumerable<string> CommittedUpdatesExcludingBackgroundCommandsNormalized
			// Background tasks that are always running (eg heartbeat) may be run while the test is happening, so we need to exclude these values
			=> CommittedUpdatesExcludingBackgroundCommands.Select(c => Regex.Replace(c, @"^Database:\s\w+\s*", ""));

		IEnumerable<string> CommittedUpdatesExcludingBackgroundCommandsSingleLine
			=> CommittedUpdatesExcludingBackgroundCommands.Select(c => c.Replace(Environment.NewLine, " "));

		IEnumerable<string> CommittedUpdatesExcludingBackgroundCommands
			=> DbCommitTracker.CommittedUpdates.Where(command => !command.Contains("StmServiceHeartBeat"));
	}
}
