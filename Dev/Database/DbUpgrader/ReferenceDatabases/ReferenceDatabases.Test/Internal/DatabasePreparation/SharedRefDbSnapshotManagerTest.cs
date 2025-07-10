using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	sealed class SharedRefDbSnapshotManagerTest : TestCase
	{
		public void TestCreate()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				IRefDbPreparationStrategy testStrategy = new RefDbPreparationStrategyForTesting(Db.DatabaseName, RefDbTypeEnum.Enterprise, "ZZ", adminConnection);
				var expectedSnapshotName = testStrategy.RefDbName + "_snapshot-20210430_165927";
				try
				{
					AdoTestUtils.DropDbIfExists(adminConnection, expectedSnapshotName);
					AdoTestUtils.DropDbIfExists(adminConnection, testStrategy.RefDbName);
					((RefDbPreparationStrategyForTesting)testStrategy).DoCreateDatabase_Exposed(adminConnection);

					CombineAssertions(() =>
					{
						var manager = new SharedRefDbSnapshotManager(testStrategy.RefDbName, new DateTime(2021, 4, 30, 16, 59, 27));
						using (manager.Create(adminConnection))
						{
							AssertEquals("Snapshot Name", expectedSnapshotName, manager.SnapshotRefDb);
							AssertEquals("Snapshot is created", true, adminConnection.DatabaseExists(manager.SnapshotRefDb));
						}

						AssertEquals("Snapshot is dropped", false, adminConnection.DatabaseExists(expectedSnapshotName));
					});
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(adminConnection, expectedSnapshotName);
					AdoTestUtils.DropDbIfExists(adminConnection, testStrategy.RefDbName);
					DbCommitTracker.Ignore(expectedSnapshotName);
					DbCommitTracker.Ignore(testStrategy.RefDbName);
				}
			}
		}

		public void TestCreate_DropOldSnapshots()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				IRefDbPreparationStrategy testStrategy = new RefDbPreparationStrategyForTesting(Db.DatabaseName, RefDbTypeEnum.Enterprise, "ZZ", adminConnection);
				var testingSnapshotName = testStrategy.RefDbName + "_snapshot-20210430_165927";
				var testingOldLessThan1Day = testStrategy.RefDbName + "_snapshot-20210430_155927";
				var testingOld1Day = testStrategy.RefDbName + "_snapshot-20210429_165927";
				var testingOldGreaterThan1Day = testStrategy.RefDbName + "_snapshot-20210429_165926";
				var testingInvalid = testStrategy.RefDbName + "_snapshot-XX";
				try
				{
					AdoTestUtils.DropDbIfExists(adminConnection, testingOldLessThan1Day);
					AdoTestUtils.DropDbIfExists(adminConnection, testingOld1Day);
					AdoTestUtils.DropDbIfExists(adminConnection, testingOldGreaterThan1Day);
					AdoTestUtils.DropDbIfExists(adminConnection, testingInvalid);
					AdoTestUtils.DropDbIfExists(adminConnection, testingSnapshotName);
					AdoTestUtils.DropDbIfExists(adminConnection, testStrategy.RefDbName);
					((RefDbPreparationStrategyForTesting)testStrategy).DoCreateDatabase_Exposed(adminConnection);

					CombineAssertions(() =>
					{
						var snapshotCreateTime = new DateTime(2021, 4, 30, 16, 59, 27);
						var manager = new SharedRefDbSnapshotManagerForTest(testStrategy.RefDbName, snapshotCreateTime);
						manager.CreateSnapshotIfNotExists(adminConnection, snapshotCreateTime.AddHours(-1).ToString(SharedRefDbSnapshotManager.TimeStampFormat), testingOldLessThan1Day);
						manager.CreateSnapshotIfNotExists(adminConnection, snapshotCreateTime.AddDays(-1).ToString(SharedRefDbSnapshotManager.TimeStampFormat), testingOld1Day);
						manager.CreateSnapshotIfNotExists(adminConnection, snapshotCreateTime.AddDays(-1).AddSeconds(-1).ToString(SharedRefDbSnapshotManager.TimeStampFormat), testingOldGreaterThan1Day);
						manager.CreateSnapshotIfNotExists(adminConnection, "XX", testingInvalid);
						AssertEquals("Old less than 1 day Snapshot exists", true, adminConnection.DatabaseExists(testingOldLessThan1Day));
						AssertEquals("Old 1 day Snapshot exists", true, adminConnection.DatabaseExists(testingOld1Day));
						AssertEquals("Old greater than 1 day Snapshot exists", true, adminConnection.DatabaseExists(testingOldGreaterThan1Day));
						AssertEquals("Invalid Snapshot exists", true, adminConnection.DatabaseExists(testingInvalid));
						manager.Create(adminConnection).Dispose();
						AssertEquals("Old less than 1 day Snapshot is dropped?", true, adminConnection.DatabaseExists(testingOldLessThan1Day));
						AssertEquals("Old 1 day Snapshot is dropped?", true, adminConnection.DatabaseExists(testingOld1Day));
						AssertEquals("Old greater than 1 day Snapshot is dropped?", false, adminConnection.DatabaseExists(testingOldGreaterThan1Day));
						AssertEquals("Invalid Snapshot is dropped?", false, adminConnection.DatabaseExists(testingInvalid));
					});
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(adminConnection, testingOldLessThan1Day);
					AdoTestUtils.DropDbIfExists(adminConnection, testingOld1Day);
					AdoTestUtils.DropDbIfExists(adminConnection, testingOldGreaterThan1Day);
					AdoTestUtils.DropDbIfExists(adminConnection, testingInvalid);
					AdoTestUtils.DropDbIfExists(adminConnection, testingSnapshotName);
					AdoTestUtils.DropDbIfExists(adminConnection, testStrategy.RefDbName);
					DbCommitTracker.Ignore(testingOldLessThan1Day);
					DbCommitTracker.Ignore(testingOld1Day);
					DbCommitTracker.Ignore(testingOldGreaterThan1Day);
					DbCommitTracker.Ignore(testingInvalid);
					DbCommitTracker.Ignore(testingSnapshotName);
					DbCommitTracker.Ignore(testStrategy.RefDbName);
				}
			}
		}

		class SharedRefDbSnapshotManagerForTest : SharedRefDbSnapshotManager
		{
			public SharedRefDbSnapshotManagerForTest(string sourceRefDb, DateTime snapshotCreateTime) : base(sourceRefDb, snapshotCreateTime)
			{
			}

			public new void CreateSnapshotIfNotExists(AdminConnection connection, string timeStamp, string snapshot) => base.CreateSnapshotIfNotExists(connection, timeStamp, snapshot);
		}
	}
}
