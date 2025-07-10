using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Warehouse.Testing
{
	[TestedType(typeof(PopulateWhsDocketCanceledColumns))]
	class PopulateWhsDocketCanceledColumnsTest : DataTransformationTestCase
	{
		const int BatchSize = 1000;
		const string ConstraintName = "Constraint_WD_CanceledTimeUtc";
		const string TriggerName = "TG_WhsDocket_SetCanceledColumns";
		const string LastProcessedChunkPKName = "PopulateWhsDocketCanceledColumns.LastProcessedChunkPK";
		const string PreviousCanceledByUser = "PRV";
		const string RecentCanceledByUser = "RCT";
		const string NotCanceledByUser = "NCD";
		const string ReActiveUser = "ACT";
		readonly DateTime PreviousDateTime = new DateTime(2024, 8, 1, 0, 0, 0);
		readonly DateTime MoreRecentDateTime = new DateTime(2024, 8, 14, 0, 0, 0);

		public void TestTriggerIsCreatedCorrectly()
		{
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("Trigger should be created.", true, DbObjectCreator.TriggerExists(TestConnection, WhsDocketSchema.Constants.TableName, TriggerName));

			PrepareTestData();

			var countOfTriggeredRows = WhsDocket.CountInDB(Db.Connection, d => d.WD_GS_NKCanceledBy != string.Empty && d.WD_CanceledTimeUtc != null);
			AssertEquals("Trigger should populate all WD_GS_NKCanceledBy/WD_CanceledTimeUtc from WD_SystemLastEditUser/WD_SystemLastEditTimeUtc.", 500, countOfTriggeredRows);
			var countOfUnTriggeredRows = WhsDocket.CountInDB(Db.Connection, d => d.WD_GS_NKCanceledBy == string.Empty || d.WD_CanceledTimeUtc == null);
			AssertEquals("Trigger should not populate for uncanceled dockets.", 50, countOfUnTriggeredRows);
			var updateAllDocketToUncanceledSQL = @"Update WhsDocket set WD_DocketStatus = 'ENT', WD_SystemLastEditTimeUtc = SYSUTCDATETIME(), WD_SystemLastEditUser = '~BP' WHERE WD_DocketStatus = 'CAN'";
			Db.Connection.ExecuteNonQuery(updateAllDocketToUncanceledSQL);
			var countOfCanceledRows = WhsDocket.CountInDB(Db.Connection, d => d.WD_DocketStatus == "CAN" || d.WD_GS_NKCanceledBy != string.Empty || d.WD_CanceledTimeUtc != null);
			AssertEquals("Trigger should clear canceled columns for uncanceled rows.", 0, countOfCanceledRows);
		}

		public void TestTransformSetsCanceledColumns()
		{
			PrepareTestData();

			var countOfAllRows = WhsDocket.CountInDB(Db.Connection);
			var countOfRowsToPopulate = WhsDocket.CountInDB(Db.Connection, d => d.WD_DocketStatus == "CAN");
			AssertEquals("Count of all rows.", 550, countOfAllRows);
			AssertEquals("Count of canceled rows to populate.", 500, countOfRowsToPopulate);
			AssertEquals("Precondition - At this point, trigger should not be created.", false, DbObjectCreator.TriggerExists(TestConnection, WhsDocketSchema.Constants.TableName, TriggerName));

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Uncanceled rows should not be updated.", 50, WhsDocket.CountInDB(TestConnection, d => d.WD_GS_NKCanceledBy == string.Empty && d.WD_CanceledTimeUtc == null));
			AssertEquals("All canceled rows are updated.", 500, WhsDocket.CountInDB(TestConnection, d => d.WD_GS_NKCanceledBy != string.Empty && d.WD_CanceledTimeUtc != null));
		}

		public void TestMultiBatchShouldAllBeProcessed()
		{
			PrepareTestData();
			PrepareTestAdditionalBatch(1000, BatchSize * 5);

			var countOfAllRows = WhsDocket.CountInDB(Db.Connection);
			AssertEquals("Count of all rows.", 5550, countOfAllRows);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var countPreviouslyCanceledRows = CountRowsWhsDocketTableForUser(PreviousCanceledByUser, PreviousDateTime);
			var countRecentlyCanceledRows = CountRowsWhsDocketTableForUser(RecentCanceledByUser, MoreRecentDateTime);
			var countNotCanceledRows = CountRowsWhsDocketTableForUser("", null);

			CombineAssertions("WP_GS_NKFinalizedBy rows are updated with correct values", () =>
			{
				AssertEquals("Previously Canceled", 5445, countPreviouslyCanceledRows);
				AssertEquals("Recently Canceled", 55, countRecentlyCanceledRows);
				AssertEquals("Not Canceled", 50, countNotCanceledRows);
			});
		}

		public void TestNoneDocketRecord()
		{
			var countOfAllRows = WhsDocket.CountInDB(Db.Connection);
			AssertEquals("Count of all rows.", 0, countOfAllRows);
			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("Count of all rows.", 0, countOfAllRows);
		}

		public void TestNumberRowsToUpdateNotEvenlyDivisibleByBatchSize()
		{
			PrepareTestData();
			PrepareTestAdditionalBatch(1000, BatchSize * 7 + 413);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var countPreviouslyCanceledRows = CountRowsWhsDocketTableForUser(PreviousCanceledByUser, PreviousDateTime);
			var countRecentlyCanceledRows = CountRowsWhsDocketTableForUser(RecentCanceledByUser, MoreRecentDateTime);
			var countNotCanceledRows = CountRowsWhsDocketTableForUser("", null);

			CombineAssertions("Canceled rows are updated with correct values", () =>
			{
				AssertEquals("Previously Canceled", 7833, countPreviouslyCanceledRows);
				AssertEquals("Recently Canceled", 80, countRecentlyCanceledRows);
				AssertEquals("Not Canceled", 50, countNotCanceledRows);
			});
		}

		public void TestCanceledColumnsSetToLastEditColumns()
		{
			PrepareTestAdditionalBatch(1000, BatchSize, logEveryDocket: false);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var countPreviouslyCanceledRows = CountRowsWhsDocketTableForUser("~BP", PreviousDateTime.AddHours(3));
			AssertEquals("All rows are updated with Last Edit Columns", 1000, countPreviouslyCanceledRows);
		}

		public void TestSetsFinalizedByWhenEmptySL_GS_NKUser()
		{
			PrepareTestAdditionalBatch(1000, BatchSize, "", logEveryDocket: true);

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("Should skip log with empty SL_GS_NKUser.", 1000, CountRowsWhsDocketTableForUser(PreviousCanceledByUser, PreviousDateTime));
		}

		public void TestStatusExtendedProperty_DeletesAllExtendedPropertiesWhenFinished()
		{
			PrepareTestData();

			var transform = GetNewTestTransformationInstance();
			transform.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("Extended property 'LastProcessedChunkPKName' should be removed once Transform is complete.", null, ExtProperty.Database.Select(TestConnection, LastProcessedChunkPKName));
		}

		int CountRowsWhsDocketTableForUser(string user, DateTime? dateTime)
		{
			return WhsDocket.CountInDB(TestConnection, d => d.WD_GS_NKCanceledBy == user && d.WD_CanceledTimeUtc == dateTime);
		}

		protected override void AssertTransformationResults()
		{
			var canceledByColumn = WhsDocketSchema.WD_GS_NKCanceledBy;
			var canceledTimeUtcColumn = WhsDocketSchema.WD_CanceledTimeUtc;
			AssertEquals("WD_GS_NKCanceledBy should get created.", true, DbObjectCreator.ColumnExists(TestConnection, canceledByColumn.TableName, canceledByColumn.Name));
			AssertEquals("WD_CanceledTimeUtc should get created.", true, DbObjectCreator.ColumnExists(TestConnection, canceledTimeUtcColumn.TableName, canceledTimeUtcColumn.Name));

			var docket1 = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketID == "O37").Single();
			var docket2 = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketID == "NO50").Single();
			var docket3 = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_DocketID == "O200").Single();

			docket1
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_GS_NKCanceledBy has been set.", d => d.WD_GS_NKCanceledBy, PreviousCanceledByUser)
				.ExpectEquals("WD_CanceledTimeUtc has been set.", d => d.WD_CanceledTimeUtc, PreviousDateTime)
				.VerifyAll();
			docket2
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_GS_NKCanceledBy has NOT been set.", d => d.WD_GS_NKCanceledBy, "")
				.ExpectEquals("WD_CanceledTimeUtc has NOT been set.", d => d.WD_CanceledTimeUtc, null)
				.VerifyAll();
			docket3
				.BuildAssertion(TestConnection)
				.ExpectEquals("WD_GS_NKCanceledBy has been set to last event from StmALog.", d => d.WD_GS_NKCanceledBy, RecentCanceledByUser)
				.ExpectEquals("WD_GS_NKCanceledBy has been set to last event from StmALog.", d => d.WD_CanceledTimeUtc, MoreRecentDateTime)
				.VerifyAll();
		}

		protected override void PrepareTestData()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var client = new OrgHeader("Client2").AppendInsertAndReturnObject(sql);

			var dockets = new List<WhsDocketOld_V01>();
			var logs = new List<StmALog>();

			for (var i = 0; i < BatchSize / 2; i++)
			{
				var whsCanceledDocket = new WhsDocketOld_V01(client.PK, whs.PK, "ORD", "ORD", "CAN", "O" + i)
				{
					WD_SystemLastEditTimeUtc = PreviousDateTime,
					WD_SystemLastEditUser = PreviousCanceledByUser
				};

				dockets.Add(whsCanceledDocket);

				logs.Add(
					new StmALog(whsCanceledDocket.PK, WhsDocketSchema.Constants.TableName, "INA", PreviousDateTime)
					{
						SL_Reference = "RF",
						SL_EventTime = PreviousDateTime,
						SL_GS_NKUser = PreviousCanceledByUser
					});

				if (i % 100 == 0) // Add a more recent StmALog from different users every 100 dockets
				{
					logs.Add(
						new StmALog(whsCanceledDocket.PK, WhsDocketSchema.Constants.TableName, "ACT", MoreRecentDateTime.AddHours(-1))
						{
							SL_Reference = "RF",
							SL_EventTime = MoreRecentDateTime.AddHours(-1),
							SL_GS_NKUser = ReActiveUser
						});
					logs.Add(
						new StmALog(whsCanceledDocket.PK, WhsDocketSchema.Constants.TableName, "INA", MoreRecentDateTime)
						{
							SL_Reference = "RF",
							SL_EventTime = MoreRecentDateTime,
							SL_GS_NKUser = RecentCanceledByUser
						});
				}

				if (i % 10 == 0) // Every 10th add a non-canceled docket
				{
					var whsNonCanceledDocket = new WhsDocketOld_V01(client.PK, whs.PK, "ORD", "ORD", "ENT", "NO" + i)
					{
						WD_SystemLastEditTimeUtc = PreviousDateTime,
						WD_SystemLastEditUser = NotCanceledByUser
					};

					dockets.Add(whsNonCanceledDocket);

					logs.Add(
						new StmALog(whsNonCanceledDocket.PK, WhsDocketSchema.Constants.TableName, "ADD", PreviousDateTime)
						{
							SL_Reference = "RF",
							SL_EventTime = PreviousDateTime,
							SL_GS_NKUser = NotCanceledByUser
						});
				}
			}

			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, WhsDocketSchema.Constants.SqlSchemaName, WhsDocketSchema.Constants.TableName, ConstraintName))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
				TestConnection.ExecuteNonQuery(WhsDocketOld_V01.GetBulkInsertStatement(dockets));
				TestConnection.ExecuteNonQuery(StmALog.GetBulkInsertStatement(logs));
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateWhsDocketCanceledColumns();

		void PrepareTestAdditionalBatch(int startingPickNo, int numberOfDockets, string userStmALog = RecentCanceledByUser, bool logEveryDocket = true)
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH2", "TRW").WithDockDoor(sql);
			var client = new OrgHeader("Client3").AppendInsertAndReturnObject(sql);

			var dockets = new List<WhsDocketOld_V01>();
			var logs = new List<StmALog>();

			for (var i = startingPickNo; i < startingPickNo + numberOfDockets; i++)
			{
				var whsCanceledDocket = new WhsDocketOld_V01(client.PK, whs.PK, "ORD", "ORD", "CAN", "O" + i)
				{
					WD_SystemLastEditTimeUtc = PreviousDateTime.AddHours(3),
					WD_SystemLastEditUser = "~BP" // Just as fallback columns
				};

				dockets.Add(whsCanceledDocket);
				if (logEveryDocket)
				{
					logs.Add(new StmALog(whsCanceledDocket.PK, WhsDocketSchema.Constants.TableName, "INA", PreviousDateTime) { SL_Reference = "RF", SL_EventTime = PreviousDateTime, SL_GS_NKUser = PreviousCanceledByUser });
					if (i % 100 == 0) // Add an extra StmALog entry for every 100 dockets
					{
						logs.Add(new StmALog(whsCanceledDocket.PK, WhsDocketSchema.Constants.TableName, "ACT", MoreRecentDateTime.AddHours(-1)) { SL_Reference = "RF", SL_EventTime = MoreRecentDateTime.AddHours(-1), SL_GS_NKUser = ReActiveUser });
						logs.Add(new StmALog(whsCanceledDocket.PK, WhsDocketSchema.Constants.TableName, "INA", MoreRecentDateTime) { SL_Reference = "RF", SL_EventTime = MoreRecentDateTime, SL_GS_NKUser = userStmALog });
					}
				}
			}

			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, WhsDocketSchema.Constants.SqlSchemaName, WhsDocketSchema.Constants.TableName, ConstraintName))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
				TestConnection.ExecuteNonQuery(WhsDocketOld_V01.GetBulkInsertStatement(dockets));
				if (logs.Count > 0)
				{
					TestConnection.ExecuteNonQuery(StmALog.GetBulkInsertStatement(logs));
				}
			}
		}

		protected override void SetUp()
		{
			new DbColumnDependencyRemover(WhsDocketSchema.Constants.TableName, WhsDocketSchema.Constants.WD_GS_NKCanceledBy).DropRelateObjects(Db.Connection);
			TestConnection.ExecuteNonQuery("ALTER TABLE WhsDocket DROP COLUMN WD_GS_NKCanceledBy");
			new DbColumnDependencyRemover(WhsDocketSchema.Constants.TableName, WhsDocketSchema.Constants.WD_CanceledTimeUtc).DropRelateObjects(Db.Connection);
			TestConnection.ExecuteNonQuery("ALTER TABLE WhsDocket DROP COLUMN WD_CanceledTimeUtc");
		}
	}
}
