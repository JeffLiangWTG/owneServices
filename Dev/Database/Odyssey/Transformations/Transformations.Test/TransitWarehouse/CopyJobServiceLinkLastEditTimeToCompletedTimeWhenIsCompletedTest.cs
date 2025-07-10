using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse.Testing
{
	[TestedType(typeof(CopyJobServiceLinkLastEditTimeToCompletedTimeWhenIsCompleted))]
	public class CopyJobServiceLinkLastEditTimeToCompletedTimeWhenIsCompletedTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, JobServiceLinkSchema.Constants.TableName, "ESL_IsCompleted", "bit");
			DBTransformationTestHelper.DropColumnIfExists(JobServiceLinkSchema.Constants.TableName, "ESL_CompletedTime");
			DBTransformationTestHelper.DropConstraintIfExists(JobServiceLinkSchema.Constants.TableName, "JobServiceLink_ESL_ES_FK2_JobService_RRR_120N");

			var sql = new StringBuilder();

			jslLastEditTime = new DateTime(2024, 1, 1, 7, 29, 0, DateTimeKind.Utc);

			jobServiceLink1 = new JobServiceLinkOld_V01("KP", Guid.NewGuid(), 10, Guid.NewGuid(), true, jslLastEditTime, jslLastEditTime).AppendInsertAndReturnObject(sql);
			jobServiceLink2 = new JobServiceLinkOld_V01("KP", Guid.NewGuid(), 10, Guid.NewGuid(), false, jslLastEditTime, jslLastEditTime).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override void AssertTransformationResults()
		{
			JobServiceLink.AssertFromDB(TestConnection, jobServiceLink1.PK)
				.ExpectEquals("ESL_CompletedTime should be set", h => h.ESL_CompletedTime, jslLastEditTime)
				.VerifyAll();

			JobServiceLink.AssertFromDB(TestConnection, jobServiceLink2.PK)
				.ExpectEquals("ESL_CompletedTime should not be set", h => h.ESL_CompletedTime, null)
				.VerifyAll();
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new CopyJobServiceLinkLastEditTimeToCompletedTimeWhenIsCompleted();

		JobServiceLinkOld_V01 jobServiceLink1;
		JobServiceLinkOld_V01 jobServiceLink2;
		DateTime jslLastEditTime;
	}
}
