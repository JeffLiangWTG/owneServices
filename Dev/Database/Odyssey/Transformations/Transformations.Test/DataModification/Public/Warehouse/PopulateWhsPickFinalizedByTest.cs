using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.DataModification.Public.Warehouse
{
	[TestedType(typeof(PopulateWhsPickFinalizedByColumn))]
	class PopulateWhsPickFinalizedByColumnTest : DataTransformationTestCase
	{
		const int BatchSize = 1000;
		const string TriggerName = "TG_WhsPick_SetFinalizedBy";
		const string StoredFromPKName = "PopulateWhsPickFinalizedByColumn.WP_GS_NKFinalizedBy.FromPK";
		const string StoredToPKName = "PopulateWhsPickFinalizedByColumn.WP_GS_NKFinalizedBy.ToPK";
		const string StoredDefaultFinalizedBy = "PopulateWhsPickFinalizedByColumn.WP_GS_NKFinalizedBy.DefaultFinalizedby";
		const string TransformationHasRunToCompletion = "PopulateWhsPickFinalizedByColumn.WP_GS_NKFinalizedBy.TransformationRunToCompletion";

		public void TestFinalizedByColumnIsCreated()
		{
			var column = WhsPickSchema.WP_GS_NKFinalizedBy;
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Column WP_GS_NKFinalizedBy should get created.", true, DbObjectCreator.ColumnExists(TestConnection, WhsPickSchema.Constants.TableName, WhsPickSchema.Constants.WP_GS_NKFinalizedBy));
			AssertEquals("Column WP_GS_NKFinalizedBy should be created with correct data type.", "varchar", DbObjectCreator.GetColumnType(TestConnection, column.TableName, WhsPickSchema.Constants.WP_GS_NKFinalizedBy));
			AssertEquals("Column WP_GS_NKFinalizedBy should have correct length.", column.MaxLength, 3);
		}

		public void TestTriggerIsCreatedCorrectly()
		{
			var transform = GetNewTestTransformationInstance();
			var expectedTriggerDefinition = @"
CREATE TRIGGER dbo.TG_WhsPick_SetFinalizedBy
	ON dbo.WhsPick
	FOR INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	IF UPDATE (WP_FinalizedDateUtc)
	BEGIN
		UPDATE wp
		SET
			wp.WP_GS_NKFinalizedBy = IIF(i.WP_FinalizedDateUtc IS NOT NULL, i.WP_SystemLastEditUser, ''),
			wp.WP_SystemLastEditUser = i.WP_SystemLastEditUser,
			wp.WP_SystemLastEditTimeUtc = i.WP_SystemLastEditTimeUtc
		FROM
			WhsPick wp
			JOIN inserted i ON wp.WP_PK = i.WP_PK
	END
END";

			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsPickSchema.Constants.TableName, TriggerName));
			AssertEquals("Trigger Definition is correct", expectedTriggerDefinition, DbObjectCreator.GetTriggerDefinition(TestConnection, TriggerName));

			PrepareTestData();

			var countOfTriggeredRows = WhsPick.CountInDB(Db.Connection, p => p.WP_GS_NKFinalizedBy != string.Empty);
			AssertEquals("Trigger should populate all WP_GS_NKFinalizedBy from WP_SystemLastEditUser.", BatchSize / 2, countOfTriggeredRows);
		}

		public void TestTransformSetsFinalizedBy()
		{
			PrepareTestData();

			var countOfRows = WhsPick.CountInDB(Db.Connection);
			var countOfRowsToPopulate = WhsPick.CountInDB(Db.Connection, p => p.WP_FinalizedDateUtc != null);
			AssertEquals("There are some WP_GS_NKFinalizedBy rows to update.", 505, countOfRows);
			AssertEquals("There are some WP_GS_NKFinalizedBy rows to update.", 500, countOfRowsToPopulate);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			// 505 picks are added by PrepareTestData, only 5 are unfinalized
			AssertEquals("All WP_GS_NKFinalizedBy rows are updated.", 5, CountRowsWhsPickTableForUser(""));
			AssertEquals("All WP_GS_NKFinalizedBy rows are updated.", 500, WhsPick.CountInDB(TestConnection, p => p.WP_GS_NKFinalizedBy != string.Empty));
		}

		public void TestNoneDocketRecord()
		{
			var countOfAllRows = WhsDocket.CountInDB(Db.Connection);
			AssertEquals("Count of all rows.", 0, countOfAllRows);
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("Count of all rows.", 0, countOfAllRows);
		}

		public void TestMultiBatchShouldAllBeProcessed()
		{
			PrepareTestData();
			PrepareTestAdditionalBatch(1000, BatchSize * 5, "SE3", logEveryPick: true);

			var countSQL = "SELECT COUNT(*) FROM WhsPick WHERE WP_SystemLastEditTimeUtc IS NOT NULL AND WP_SystemLastEditUser != ''";
			var countOfRowsToPopulate = (int)TestConnection.ExecuteScalar(countSQL);
			AssertEquals("There are some WP_GS_NKFinalizedBy rows to update.", true, countOfRowsToPopulate > 0);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var countSE1RowsAfter = CountRowsWhsPickTableForUser("SE1");
			var countSE2RowsAfter = CountRowsWhsPickTableForUser("SE2");
			var countSE3RowsAfter = CountRowsWhsPickTableForUser("SE3");
			var countEmptyRowsAfter = CountRowsWhsPickTableForUser("");

			CombineAssertions("WP_GS_NKFinalizedBy rows are updated with correct values", () =>
			{
				AssertEquals("SE1", 498, countSE1RowsAfter);
				AssertEquals("SE2", 4950, countSE2RowsAfter);
				AssertEquals("SE3", 52, countSE3RowsAfter);
				AssertEquals("Empty", 5, countEmptyRowsAfter);
			});
		}

		public void TestNumberRowsToUpdateNotEvenlyDivisibleByBatchSize()
		{
			PrepareTestData();
			PrepareTestAdditionalBatch(1000, BatchSize / 2, "SE3", logEveryPick: true);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var countSE1RowsAfter = CountRowsWhsPickTableForUser("SE1");
			var countSE2RowsAfter = CountRowsWhsPickTableForUser("SE2");
			var countSE3RowsAfter = CountRowsWhsPickTableForUser("SE3");
			var countEmptyRowsAfter = CountRowsWhsPickTableForUser("");

			CombineAssertions("WP_GS_NKFinalizedBy rows are updated with correct values", () =>
			{
				AssertEquals("SE1", 498, countSE1RowsAfter);
				AssertEquals("SE2", 495, countSE2RowsAfter);
				AssertEquals("SE3", 7, countSE3RowsAfter);
				AssertEquals("Empty", 5, countEmptyRowsAfter);
			});
		}

		public void TestFinalizedBySetToLastEditUser()
		{
			PrepareTestData();
			PrepareTestAdditionalBatch(1000, BatchSize, "SE3", logEveryPick: false);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var countSE1RowsAfter = CountRowsWhsPickTableForUser("SE1");
			var countSE2RowsAfter = CountRowsWhsPickTableForUser("SE2");
			var countSE3RowsAfter = CountRowsWhsPickTableForUser("SE3");
			var countEmptyRowsAfter = CountRowsWhsPickTableForUser("");

			CombineAssertions("WP_GS_NKFinalizedBy rows are updated with correct values", () =>
			{
				AssertEquals("SE1", 498, countSE1RowsAfter);
				AssertEquals("SE2", 990, countSE2RowsAfter);
				AssertEquals("SE3", 12, countSE3RowsAfter);
				AssertEquals("Empty", 5, countEmptyRowsAfter);
			});
		}

		public void TestTransformationNoUpdatesRequired()
		{
			PrepareTestAdditionalBatch(0, BatchSize, "SE2", logEveryPick: true);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var countSE1RowsAfter = CountRowsWhsPickTableForUser("SE1");
			var countSE2RowsAfter = CountRowsWhsPickTableForUser("SE2");
			var countSE3RowsAfter = CountRowsWhsPickTableForUser("SE3");
			var countEmptyRowsAfter = CountRowsWhsPickTableForUser("");

			CombineAssertions("WP_GS_NKFinalizedBy rows are updated with correct values", () =>
			{
				AssertEquals("SE1", 0, countSE1RowsAfter);
				AssertEquals("SE2", 1000, countSE2RowsAfter);
				AssertEquals("SE3", 0, countSE3RowsAfter);
				AssertEquals("Empty", 0, countEmptyRowsAfter);
			});
		}

		public void TestStatusExtendedProperty_WhenAlreadySetToFinished()
		{
			ExtProperty.Database.Update(TestConnection, TransformationHasRunToCompletion, bool.TrueString);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Status should be updated to Finished once Transform is complete.", bool.TrueString, ExtProperty.Database.Select(TestConnection, TransformationHasRunToCompletion));
		}

		public void TestStatusExtendedProperty_DeletesAllExtendedPropertiesWhenFinished()
		{
			PrepareTestData();

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Extended property 'StoredFromPKName' should be removed once Transform is complete.", null, ExtProperty.Database.Select(TestConnection, StoredFromPKName));
			AssertEquals("Extended property 'StoredToPKName' should be removed once Transform is complete.", null, ExtProperty.Database.Select(TestConnection, StoredToPKName));
			AssertEquals("Extended property 'StoredDefaultFinalizedBy' should be removed once Transform is complete.", null, ExtProperty.Database.Select(TestConnection, StoredDefaultFinalizedBy));
		}

		public void TestStatusExtendedProperty_DeletesPropertyBeforeStarting()
		{
			ExtProperty.Database.Update(TestConnection, TransformationHasRunToCompletion, bool.TrueString);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			AssertEquals("Extended property 'TransformationHasRunToCompletion' should be removed before Transform is started.", null, ExtProperty.Database.Select(TestConnection, TransformationHasRunToCompletion));
		}

		public void TestRunMultipleTimes()
		{
			PrepareTestData();

			var countOfRows = WhsPick.CountInDB(Db.Connection);
			var countOfRowsToPopulate = WhsPick.CountInDB(Db.Connection, p => p.WP_FinalizedDateUtc != null);
			AssertEquals("There are some WP_GS_NKFinalizedBy rows to update.", 505, countOfRows);
			AssertEquals("There are some WP_GS_NKFinalizedBy rows to update.", 500, countOfRowsToPopulate);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			// 505 picks are added by PrepareTestData, only 5 are unfinalized
			AssertEquals("All WP_GS_NKFinalizedBy rows are updated.", 5, CountRowsWhsPickTableForUser(""));
			AssertEquals("All WP_GS_NKFinalizedBy rows are updated.", 500, WhsPick.CountInDB(TestConnection, p => p.WP_GS_NKFinalizedBy != string.Empty));

			// Drop the default and trigger
			new DbColumnDependencyRemover(WhsPickSchema.Constants.TableName, WhsPickSchema.Constants.WP_GS_NKFinalizedBy).DropRelateObjects(Db.Connection);
			Db.Connection.ExecuteNonQuery("ALTER TABLE [dbo].[WhsPick] ADD  CONSTRAINT [DF_WhsPick_WP_GS_NKFinalizedBy]  DEFAULT ('') FOR [WP_GS_NKFinalizedBy]");

			PrepareTestAdditionalBatch(1000, BatchSize, "SE3", logEveryPick: false);
			AssertEquals("Precondition.", 1005, CountRowsWhsPickTableForUser(""));

			ExtProperty.Database.Update(TestConnection, TransformationHasRunToCompletion, bool.FalseString);
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("The additional 1000 rows should have a FinalizedBy user set.", 5, CountRowsWhsPickTableForUser(""));
		}

		public void TestSetsFinalizedByWhenEmptySL_GS_NKUser()
		{
			PrepareTestData();
			PrepareTestAdditionalBatch(1000, BatchSize, "", logEveryPick: true);

			var countOfRows = WhsPick.CountInDB(Db.Connection);
			var countOfRowsToPopulate = WhsPick.CountInDB(Db.Connection, p => p.WP_FinalizedDateUtc != null);
			AssertEquals("There are some WP_GS_NKFinalizedBy rows to update.", 1505, countOfRows);
			AssertEquals("There are some WP_GS_NKFinalizedBy rows to update.", 1500, countOfRowsToPopulate);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			// 1505 picks are added by PrepareTestData+PrepareTestAdditionalBatch, only 5 are unfinalized
			AssertEquals("All WP_GS_NKFinalizedBy rows are updated.", 5, CountRowsWhsPickTableForUser(""));
			AssertEquals("All WP_GS_NKFinalizedBy rows are updated.", 1500, WhsPick.CountInDB(TestConnection, p => p.WP_GS_NKFinalizedBy != string.Empty));
		}

		protected override void AssertTransformationResults()
		{
			var column = WhsPickSchema.WP_GS_NKFinalizedBy;
			AssertEquals("Column WP_GS_NKFinalizedBy should get created.", true, DbObjectCreator.ColumnExists(TestConnection, column.TableName, column.Name));

			var pick1 = WhsPickOld_V02.ShallowLoadFromDB(TestConnection, p1 => p1.WP_PickNo == "P1").Single();
			var pick2 = WhsPickOld_V02.ShallowLoadFromDB(TestConnection, p2 => p2.WP_PickNo == "NP0").Single();
			var pick3 = WhsPickOld_V02.ShallowLoadFromDB(TestConnection, p3 => p3.WP_PickNo == "P0").Single();

			pick1
				.BuildAssertion(TestConnection)
				.ExpectEquals("FinalizedBy has been set.", p1 => p1.WP_GS_NKFinalizedBy, "SE1")
				.VerifyAll();
			pick2
				.BuildAssertion(TestConnection)
				.ExpectEquals("FinalizedBy has NOT been set.", p2 => p2.WP_GS_NKFinalizedBy, "")
				.VerifyAll();
			pick3
				.BuildAssertion(TestConnection)
				.ExpectEquals("FinalizedBy has been set to last event from StmALog.", p3 => p3.WP_GS_NKFinalizedBy, "SE3")
				.VerifyAll();
		}

		protected override void PrepareTestData()
		{
			var prevHour = DateTime.UtcNow.AddHours(-1);
			var now = DateTime.UtcNow;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);

			var picks = new List<WhsPickOld_V01>();
			var logs = new List<StmALog>();

			for (var i = 0; i < BatchSize / 2; i++)
			{
				var whsFinalizedPick = new WhsPickOld_V01(whs, $"P{i}", "FIN")
				{
					WP_FinalizedDateUtc = prevHour,
					WP_SystemLastEditTimeUtc = prevHour,
					WP_SystemLastEditUser = "SE1"
				};

				picks.Add(whsFinalizedPick);

				logs.Add(
					new StmALog(whsFinalizedPick.PK, WhsPickSchema.Constants.TableName, "FIN", prevHour)
					{
						SL_Reference = "RF",
						SL_EventTime = prevHour,
						SL_GS_NKUser = "SE1"
					});

				if (i % 300 == 0) // Add a more recent StmALog from different user every 300 picks
				{
					logs.Add(
						new StmALog(whsFinalizedPick.PK, WhsPickSchema.Constants.TableName, "FIN", now)
						{
							SL_Reference = "RF",
							SL_EventTime = now,
							SL_GS_NKUser = "SE3"
						});
				}

				if (i % 100 == 0) // Every 100th add a non-finalized pick
				{
					var whsNonFinalizedPick = new WhsPickOld_V01(whs, "NP" + Convert.ToString(i), "NEW")
					{
						WP_SystemLastEditTimeUtc = prevHour,
						WP_SystemLastEditUser = "SE4"
					};

					picks.Add(whsNonFinalizedPick);

					logs.Add(
						new StmALog(whsNonFinalizedPick.PK, WhsPickSchema.Constants.TableName, "NEW", prevHour)
						{
							SL_Reference = "RF",
							SL_EventTime = prevHour,
							SL_GS_NKUser = "SE5"
						});
				}
			}

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			TestConnection.ExecuteNonQuery(WhsPickOld_V01.GetBulkInsertStatement(picks));
			TestConnection.ExecuteNonQuery(StmALog.GetBulkInsertStatement(logs));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateWhsPickFinalizedByColumn();

		void PrepareTestAdditionalBatch(int startingPickNo, int numberOfPicks, string userStmALog, bool logEveryPick)
		{
			var utcNow = DateTime.UtcNow;
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH2", "TRW").WithDockDoor(sql);

			var picks = new List<WhsPickOld_V01>();
			var logs = new List<StmALog>();

			for (var i = startingPickNo; i < startingPickNo + numberOfPicks; i++)
			{
				var whsPick = new WhsPickOld_V01(whs, $"P{i}", "FIN")
				{
					WP_FinalizedDateUtc = utcNow.AddHours(-1),
					WP_SystemLastEditTimeUtc = utcNow.AddHours(-1),
					WP_SystemLastEditUser = "SE2"
				};

				picks.Add(whsPick);

				if (logEveryPick)
				{
					logs.Add(new StmALog(whsPick.PK, WhsPickSchema.Constants.TableName, "FIN", now.AddHours(-1)) { SL_Reference = "RF", SL_EventTime = now.AddHours(-1), SL_GS_NKUser = "SE2" });
				}

				if (i % 100 == 0) // Add an extra StmALog entry for every 100 picks
				{
					logs.Add(new StmALog(whsPick.PK, WhsPickSchema.Constants.TableName, "FIN", now) { SL_Reference = "RF", SL_EventTime = now, SL_GS_NKUser = userStmALog });
				}
			}

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			TestConnection.ExecuteNonQuery(WhsPickOld_V01.GetBulkInsertStatement(picks));
			TestConnection.ExecuteNonQuery(StmALog.GetBulkInsertStatement(logs));
		}

		protected override void SetUp()
		{
			new DbColumnDependencyRemover(WhsPickSchema.Constants.TableName, WhsPickSchema.Constants.WP_GS_NKFinalizedBy).DropRelateObjects(Db.Connection);
			TestConnection.ExecuteNonQuery("ALTER TABLE WhsPick DROP COLUMN WP_GS_NKFinalizedBy");
			new DbColumnDependencyRemover(WhsPickSchema.Constants.TableName, WhsPickSchema.Constants.WP_PickType).DropRelateObjects(Db.Connection);
			TestConnection.ExecuteNonQuery("ALTER TABLE WhsPick DROP COLUMN WP_PickType");
		}

		int CountRowsWhsPickTableForUser(string user)
		{
			return WhsPick.CountInDB(TestConnection, p => p.WP_GS_NKFinalizedBy == user);
		}
	}
}
