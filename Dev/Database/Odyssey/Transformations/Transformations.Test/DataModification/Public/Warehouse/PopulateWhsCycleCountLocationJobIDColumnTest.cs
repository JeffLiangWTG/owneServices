using System;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using Enterprise.ZArchitecture.Schema;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Warehouse.Testing
{
	[TestedType(typeof(PopulateWhsCycleCountLocationJobIDColumn))]
	class PopulateWhsCycleCountLocationJobIDColumnTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var sql = CreateLocationAndCycleCounts(createCount: 6);

			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, WhsCycleCountLocationSchema.Constants.SqlSchemaName, WhsCycleCountLocationSchema.Constants.TableName, "Constraint_WCL_JobID"))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}
		}

		protected override void AssertTransformationResults()
		{
			var whsCycleCountLocations = WhsCycleCountLocation.ShallowLoadFromDB(TestConnection).OrderBy(r => r.WCL_SystemCreateTimeUtc).Select(c => c.WCL_JobID);

			AssertContainsExactElementsInExactOrder("Job ID should added by Create Time", new string[] { "WC00000001", "WC00000002", "WC00000003", "WC00000004", "WC00000005", "WC00000006" }, whsCycleCountLocations);
		}

		#region TestOnlinePreUpgrade

		public void TestOnlinePreUpgrade()
		{
			TestTransformation(true);
		}

		public void TestOnlinePreUpgrade_WithJobIDColumnNotExists()
		{
			DBTransformationTestHelper.DropIndexIfExists(WhsCycleCountLocationSchema.Constants.TableName, WhsCycleCountLocationSchema.Constants.Indexes.NR_UX__WCL_JobID);
			DBTransformationTestHelper.DropConstraintIfExists(WhsCycleCountLocationSchema.Constants.TableName, "DF_WhsCycleCountLocation_WCL_JobID");
			DBTransformationTestHelper.DropConstraintIfExists(WhsCycleCountLocationSchema.Constants.TableName, "Constraint_WCL_JobID");
			DBTransformationTestHelper.DropColumnIfExists(WhsCycleCountLocationSchema.Constants.TableName, "WCL_JobID", TestConnection);

			var sql = CreateLocationAndCycleCounts(createCount: 20);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			Assert(!DbObjectCreator.ColumnExists(Db.Connection, WhsCycleCountLocationSchema.Constants.TableName, "WCL_JobID"));

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			Assert(DbObjectCreator.ColumnExists(Db.Connection, WhsCycleCountLocationSchema.Constants.TableName, "WCL_JobID"));

			Assert(DbObjectCreator.IndexExists(Db.Connection, WhsCycleCountLocationSchema.Constants.TableName, "NR_UX__WCL_JobID"));

			AssertEquals("There is no Rows without Job ID", 0, (int)TestConnection.ExecuteScalar(CountRowsWithoutJobIDSQL));

			AssertEquals("The next Number Fountain didnot created for pre transform", null, TestConnection.ExecuteScalar(GetTheNextNumberFountain));
		}

		[ExpectNoExceptions]
		public void TestOnlinePreUpgrade_WithTableCycleCountLocationNotExists()
		{
			new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, WhsCycleCountLocationSchema.Constants.TableName, WhsCycleCountLocationSchema.Constants.PK).DropRelateObjects(Db.Connection);
			new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, WhsCycleCountLocationSchema.Constants.TableName, WhsCycleCountLocationSchema.Constants.WCL_WL_Location).DropRelateObjects(Db.Connection);
			DbObjectCreator.DropTableIfExists(Db.Connection, WhsCycleCountLocationSchema.Constants.TableName);

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
		}

		public void TestTestOnlinePreUpgrade_PartialOnlineTransformHasRan()
		{
			var date = DateTime.Now;
			var sql = CreateLocationAndCycleCounts(createCount: 20);

			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, WhsCycleCountLocationSchema.Constants.SqlSchemaName, WhsCycleCountLocationSchema.Constants.TableName, "Constraint_WCL_JobID"))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals("All Created CycleCountLocation have no Job ID", 20, (int)TestConnection.ExecuteScalar(CountRowsWithoutJobIDSQL));

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("There is no Rows without Job ID", 0, (int)TestConnection.ExecuteScalar(CountRowsWithoutJobIDSQL));

			var sql2 = new SqlQueryBuilder();
			var row = WhsRow.ShallowLoadFromDB(TestConnection, r => r.WR_Name == "Row").Single();
			var area = WhsArea.ShallowLoadFromDB(TestConnection, a => a.WA_Name == "Area").Single();

			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 21 }.AppendInsertAndReturnObject(sql2);
			var cycleCount = new WhsCycleCountLocation(location.PK, "PWA") { WCL_SystemCreateTimeUtc = date }.AppendInsertAndReturnObject(sql2);

			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, WhsCycleCountLocationSchema.Constants.SqlSchemaName, WhsCycleCountLocationSchema.Constants.TableName, "Constraint_WCL_JobID"))
			{
				TestConnection.ExecuteNonQuery(sql2.ToStringWithNewLineBetweenAppends());
			}

			AssertNullOrEmpty("Precondition", cycleCount.WCL_JobID);

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			var cycleCountAfterTransform = WhsCycleCountLocation.ShallowLoadFromDB(TestConnection, c => c.PK == cycleCount.PK).Single();

			AssertEquals("JobId will created successfully", "WC00000021", cycleCountAfterTransform.WCL_JobID);
		}

		public void TestBatchingWorksProperly()
		{
			var sql = CreateLocationAndCycleCounts(createCount: 1500);

			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, WhsCycleCountLocationSchema.Constants.SqlSchemaName, WhsCycleCountLocationSchema.Constants.TableName, "Constraint_WCL_JobID"))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals("All Created CycleCountLocation have no Job ID", 1500, (int)TestConnection.ExecuteScalar(CountRowsWithoutJobIDSQL));

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("There is no Rows without Job ID", 0, (int)TestConnection.ExecuteScalar(CountRowsWithoutJobIDSQL));
		}

		#endregion

		#region TestOfflinePreUpgrade

		public void TestOfflinePreUpgrade()
		{
			TestTransformation(false);
		}

		public void TestOfflinePreUpgrade_WithJobIDColumnNotExists()
		{
			DBTransformationTestHelper.DropIndexIfExists(WhsCycleCountLocationSchema.Constants.TableName, WhsCycleCountLocationSchema.Constants.Indexes.NR_UX__WCL_JobID);
			DBTransformationTestHelper.DropConstraintIfExists(WhsCycleCountLocationSchema.Constants.TableName, "DF_WhsCycleCountLocation_WCL_JobID");
			DBTransformationTestHelper.DropConstraintIfExists(WhsCycleCountLocationSchema.Constants.TableName, "Constraint_WCL_JobID");
			DBTransformationTestHelper.DropColumnIfExists(WhsCycleCountLocationSchema.Constants.TableName, "WCL_JobID", TestConnection);

			Assert(!DbObjectCreator.ColumnExists(Db.Connection, WhsCycleCountLocationSchema.Constants.TableName, "WCL_JobID"));

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			Assert(!DbObjectCreator.ColumnExists(Db.Connection, WhsCycleCountLocationSchema.Constants.TableName, "WCL_JobID"));
		}

		[ExpectNoExceptions]
		public void TestOfflinePreUpgrade_WithTableCycleCountLocationNotExists()
		{
			new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, WhsCycleCountLocationSchema.Constants.TableName, WhsCycleCountLocationSchema.Constants.PK).DropRelateObjects(Db.Connection);
			new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, WhsCycleCountLocationSchema.Constants.TableName, WhsCycleCountLocationSchema.Constants.WCL_WL_Location).DropRelateObjects(Db.Connection);
			DbObjectCreator.DropTableIfExists(Db.Connection, WhsCycleCountLocationSchema.Constants.TableName);

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
		}

		#endregion

		#region TestOfflinePostUpgrade

		public void TestOfflinePostUpgrade()
		{
			var sql = CreateLocationAndCycleCounts(createCount: 30);

			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, WhsCycleCountLocationSchema.Constants.SqlSchemaName, WhsCycleCountLocationSchema.Constants.TableName, "Constraint_WCL_JobID"))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals("Precondition", null, TestConnection.ExecuteScalar(GetTheNextNumberFountain));

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			AssertEquals("The next Number Fountain should be right after OnlinePre transfromations", null, TestConnection.ExecuteScalar(GetTheNextNumberFountain));

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			AssertEquals("The next Number Fountain should be right after OfflinePre transfromations", null, TestConnection.ExecuteScalar(GetTheNextNumberFountain));

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertEquals("The next Number Fountain should be right after OfflinePost transformation", 31, (long)TestConnection.ExecuteScalar(GetTheNextNumberFountain));
		}

		public void TestOfflinePostUpgrade_NoCycleCountRecordsExists()
		{
			DeleteNumberFountainForWhsCycleCountLocation();

			AssertEquals("Precondition", null, TestConnection.ExecuteScalar(GetTheNextNumberFountain));

			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			AssertEquals("The next Number Fountain should be right after transformation", 1, (long)TestConnection.ExecuteScalar(GetTheNextNumberFountain));
		}

		#endregion

		#region TestUserDescription

		public void TestUserDescription()
		{
			AssertEquals("Populate empty Warehouse Cycle Count Location JobID.", GetNewTestTransformationInstance().UserDescription);
		}

		#endregion

		void TestTransformation(bool isOnline)
		{
			var sql = CreateLocationAndCycleCounts(createCount: 30);

			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, WhsCycleCountLocationSchema.Constants.SqlSchemaName, WhsCycleCountLocationSchema.Constants.TableName, "Constraint_WCL_JobID"))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			AssertEquals("All Created CycleCountLocation have no Job ID", 30, (int)TestConnection.ExecuteScalar(CountRowsWithoutJobIDSQL));

			GetNewTestTransformationInstance().Run(isOnline ? TransformationSection.OnlinePreUpgrade : TransformationSection.OfflinePreUpgrade, CancellationToken.None);

			AssertEquals("There is no Rows without Job ID", 0, (int)TestConnection.ExecuteScalar(CountRowsWithoutJobIDSQL));
		}

		SqlQueryBuilder CreateLocationAndCycleCounts(int createCount)
		{
			DBTransformationTestHelper.DropIndexIfExists(WhsCycleCountLocationSchema.Constants.TableName, WhsCycleCountLocationSchema.Constants.Indexes.NR_UX__WCL_JobID);
			DeleteNumberFountainForWhsCycleCountLocation();

			var date = new DateTime(2023, 8, 1);
			var sql = new SqlQueryBuilder();
			WhsRow row;
			WhsArea area;
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WHS", "PRW", branch.PK).WithDockDoor(TestConnection);
			row = new WhsRow(whs, "Row").AppendInsertAndReturnObject(sql);
			area = new WhsArea(whs.PK, "Area").AppendInsertAndReturnObject(sql);

			var locations = new WhsLocation[createCount];
			var cycleCounts = new WhsCycleCountLocation[createCount];

			for (var i = 0; i < createCount; i++)
			{
				locations[i] = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = (short)(i + 1) };
				cycleCounts[i] = new WhsCycleCountLocation(locations[i].PK, "PWA") { WCL_SystemCreateTimeUtc = date.AddHours(i) };
			}

			sql.AppendLine(WhsLocation.GetBulkInsertStatement(locations));
			sql.AppendLine(WhsCycleCountLocation.GetBulkInsertStatement(cycleCounts));

			return sql;
		}

		void DeleteNumberFountainForWhsCycleCountLocation()
		{
			var sql = $"DELETE FROM dbo.StmNums WHERE SN_Name = 'WhsCycleCountLocationID'";
			TestConnection.ExecuteNonQuery(sql);
		}

		const string CountRowsWithoutJobIDSQL = @"
SELECT
	COUNT(*)
FROM
	dbo.WhsCycleCountLocation
WHERE
	WCL_JobID = ''
";

		const string GetTheNextNumberFountain = @"
SELECT
	SN_Value
FROM
	dbo.StmNums
WHERE
	SN_Name = 'WhsCycleCountLocationID'
";

		#region Implementation

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateWhsCycleCountLocationJobIDColumn();

		#endregion
	}
}
