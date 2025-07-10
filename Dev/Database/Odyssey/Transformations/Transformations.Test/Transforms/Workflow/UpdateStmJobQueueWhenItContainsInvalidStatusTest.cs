using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Workflow;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Workflow
{
	[TestedType(typeof(UpdateStmJobQueueWhenItContainsInvalidStatus))]
	public class UpdateStmJobQueueWhenItContainsInvalidStatusTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateStmJobQueueWhenItContainsInvalidStatus();
		protected override void PrepareTestData()
		{
			Db.Connection.ExecuteNonQuery("ALTER TABLE dbo.StmJobQueue NOCHECK CONSTRAINT Constraint_SJ_Status");

			TestConnection.ExecuteNonQuery("TRUNCATE TABLE dbo.StmJobQueue");
			var dataCreator = new TransformationTestDataCreator();
			dataCreator.CreateStmJobQueue(DateTime.Now, "112", "ERR", Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, "JS");
			dataCreator.CreateStmJobQueue(DateTime.Now, "112", "FAI", Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, "JS");
			dataCreator.CreateStmJobQueue(DateTime.Now, "112", "ABS", Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, "JS");
			dataCreator.CreateStmJobQueue(DateTime.Now, "112", "QUE", Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, "JS");
			dataCreator.CreateStmJobQueue(DateTime.Now, "112", "PRS", Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, "JS");
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(5, TestConnection.ExecuteScalar($"select count(*) from dbo.StmJobQueue WHERE SJ_Status IN ('FAI', 'PRS', 'QUE')"));
			AssertEquals(3, TestConnection.ExecuteScalar($"select count(*) from dbo.StmJobQueue WHERE SJ_Status = 'FAI'"));
		}
	}
}
