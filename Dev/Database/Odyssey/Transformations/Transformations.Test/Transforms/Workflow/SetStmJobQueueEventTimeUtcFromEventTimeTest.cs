using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Workflow;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Workflow
{
	[TestedType(typeof(SetStmJobQueueEventTimeUtcFromEventTime))]
	class SetStmJobQueueEventTimeUtcFromEventTimeTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new SetStmJobQueueEventTimeUtcFromEventTime();
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			MakeSJ_EventTimeUtcNullable();

			Insert3StmJobQueues();
		}

		static void MakeSJ_EventTimeUtcNullable()
		{
			if (DbObjectCreator.ColumnExists(Db.Connection, "StmJobQueue", "SJ_ProcessOnOrAfterUtc"))
			{
				Db.Connection.ExecuteNonQuery(@"CREATE CLUSTERED INDEX [NR_RC__SJ_FilterName_SJ_Status_SJ_ProcessOnOrAfterUtc] ON [StmJobQueue] ([SJ_FilterName] ASC,[SJ_Status] ASC,[SJ_PostedTimeUtc] ASC,[SJ_EventTime] ASC)
with (drop_existing = on)");
				Db.Connection.ExecuteNonQuery(@"ALTER TABLE dbo.StmJobQueue DROP COLUMN SJ_ProcessOnOrAfterUtc");
				Db.Connection.ExecuteNonQuery(@"ALTER TABLE dbo.StmJobQueue ALTER COLUMN SJ_EventTimeUtc DATETIME NULL");
			}
		}

		void Insert3StmJobQueues()
		{
			TestConnection.ExecuteNonQuery(@"
insert into StmJobQueue (SJ_PK, SJ_FilterName, SJ_Status, SJ_ALogReference, SJ_PostedTimeUtc, SJ_EventTime, SJ_EventTimeUtc, SJ_ParentID, SJ_ParentTableCode) values (NEWID(), 'TEST1', 'QUE', NEWID(), GETDATE(), GETDATE(), NULL, NEWID(), 'JS');
insert into StmJobQueue (SJ_PK, SJ_FilterName, SJ_Status, SJ_ALogReference, SJ_PostedTimeUtc, SJ_EventTime, SJ_EventTimeUtc, SJ_ParentID, SJ_ParentTableCode) values (NEWID(), 'TEST2', 'QUE', NEWID(), GETDATE(), GETDATE(), NULL, NEWID(), 'JS');
insert into StmJobQueue (SJ_PK, SJ_FilterName, SJ_Status, SJ_ALogReference, SJ_PostedTimeUtc, SJ_EventTime, SJ_EventTimeUtc, SJ_ParentID, SJ_ParentTableCode) values (NEWID(), 'TEST3', 'QUE', NEWID(), GETDATE(), GETDATE(), NULL, NEWID(), 'JS');
			");
		}

		protected override void AssertPreConditions()
		{
			base.AssertPreConditions();

			AssertEquals(3, TestConnection.ExecuteScalar("select count(*) from StmJobQueue where SJ_EventTimeUtc IS NULL"));
		}

		protected override void AssertTransformationResults()
		{
			base.AssertTransformationResults();
			AssertEquals(0, TestConnection.ExecuteScalar("select count(*) from StmJobQueue where SJ_EventTimeUtc IS NULL"));
			AssertEquals(3, TestConnection.ExecuteScalar("select count(*) from StmJobQueue where SJ_EventTimeUtc = SJ_EventTime"));
		}

		public void TestSJ_EventTimeUtcDoesNotExist()
		{
			MakeSJ_EventTimeUtcNullable();
			Insert3StmJobQueues();
			Db.Connection.ExecuteNonQuery(@"ALTER TABLE dbo.StmJobQueue DROP COLUMN SJ_EventTimeUtc");

			RunTransformation();
			AssertEquals(0, TestConnection.ExecuteScalar("select count(*) from StmJobQueue where SJ_EventTimeUtc IS NULL"));
			AssertEquals(3, TestConnection.ExecuteScalar("select count(*) from StmJobQueue where SJ_EventTimeUtc = SJ_EventTime"));
		}
	}
}
