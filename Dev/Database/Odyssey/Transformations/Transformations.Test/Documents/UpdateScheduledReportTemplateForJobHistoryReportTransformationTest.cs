using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.IO;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Documents;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Documents
{
	[TestedType(typeof(UpdateScheduledReportTemplateForJobHistoryReportTransformation))]
	public class UpdateScheduledReportTemplateForJobHistoryReportTransformationTest : DataTransformationTestCase
	{
		const string TempTableName = "ClientTransformTable_WI00769394";
		const string JobHistoryReportPK = "CFD5875F-1F58-4E39-A775-AB5FEFF9D5CC";

		protected override void PrepareTestData()
		{
			new StmScheduleTask(Guid.Parse(JobHistoryReportPK), "SU").InsertAndReturnObject(TestConnection);
		}

		protected override void AssertTransformationResults()
		{
			var tasks = StmScheduleTask.ShallowLoadFromDB(TestConnection, task => task.S5_ParentID == Guid.Parse(JobHistoryReportPK));
			AssertEquals("Single Scheduled Report for Job History Report should exist.", 1, tasks.Length);
			AssertEquals("Schedule State should remain empty.", null, tasks[0].S5_ScheduleState);

			AssertTempTableDoesNotExist();
		}

		public void TestTransformationInvokesInterfaceMethod_WithInvalidBytes()
		{
			var task = new StmScheduleTask(Guid.Parse(JobHistoryReportPK), "SU")
			{
				S5_ScheduleType = "REP",
				S5_ScheduleState = new[] { (byte)1 },
			}.InsertAndReturnObject(TestConnection);

			var instance = GetNewTestTransformationInstance();
			instance.Run();

			StmScheduleTask.AssertFromDB(TestConnection, task.PK)
				.ExpectEquals("S5_ScheduleState", t => t.S5_ScheduleState.Single(), (byte)1)
				.VerifyAll();

			var deserializationError = ErrorReporter.LastKeyReported;
			AssertEquals("1e099933-f09b-4c83-9e37-5c35e8f0e302", deserializationError);
			ErrorReporter.Clear();

			AssertTempTableDoesNotExist();
		}

		public void TestTransformationInvokesInterfaceMethod_TempTableAlreadyExists()
		{
			new StmScheduleTask(Guid.Parse(JobHistoryReportPK), "SU").InsertAndReturnObject(TestConnection);

			DbObjectCreator.CreateTableIfNotExists(Db.Connection, TempTableName, $@"
CREATE TABLE dbo.{TempTableName}
(
	JHR_S5_PK UNIQUEIDENTIFIER NOT NULL,
	JHR_FailedBatch bit NOT NULL DEFAULT ((0)),
	JHR_FailedIndividually bit NOT NULL DEFAULT ((0))
);");
			var instance = GetNewTestTransformationInstance();
			instance.Run();

			AssertTempTableDoesNotExist();
		}

		public void TestTransformationSetsUpTemporaryTable()
		{
			new StmScheduleTask(Guid.NewGuid(), "SU")
			{
				S5_ScheduleType = "REP",
			}.InsertAndReturnObject(TestConnection);
			new StmScheduleTask(Guid.Parse(JobHistoryReportPK), "TGR")
			{
				S5_ScheduleType = "LWK",
			}.InsertAndReturnObject(TestConnection);
			var jobHistoryScheduledReport1 = new StmScheduleTask(Guid.Parse(JobHistoryReportPK), "SU")
			{
				S5_ScheduleType = "REP",
			}.InsertAndReturnObject(TestConnection);
			var jobHistoryScheduledReport2 = new StmScheduleTask(Guid.Parse(JobHistoryReportPK), "SU")
			{
				S5_ScheduleType = "REP",
			}.InsertAndReturnObject(TestConnection);
			var jobHistoryScheduledReport3 = new StmScheduleTask(Guid.Parse(JobHistoryReportPK), "SU")
			{
				S5_ScheduleType = "REP",
			}.InsertAndReturnObject(TestConnection);

			var cancellationToken = CancellationToken.None;
			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			var tableExistsQuery = $"SELECT COUNT(*) WHERE OBJECT_ID('dbo.{TempTableName}', 'U') IS NOT NULL";
			var tableExists = (int)TestConnection.ExecuteScalar(tableExistsQuery);
			AssertEquals("Table should exist after Post Upgrade Offline Step.", 1, tableExists);

			var numRowsQuery = $"SELECT COUNT(*) FROM dbo.{TempTableName}";
			var numRows = (int)TestConnection.ExecuteScalar(numRowsQuery);
			AssertEquals("There should be 3 rows, one per Job History Scheduled Report.", 3, numRows);

			AssertRowIsPopulatedCorrect(jobHistoryScheduledReport1.PK);
			AssertRowIsPopulatedCorrect(jobHistoryScheduledReport2.PK);
			AssertRowIsPopulatedCorrect(jobHistoryScheduledReport3.PK);

			void AssertRowIsPopulatedCorrect(Guid scheduledTaskPK)
			{
				var numRowsQuery = $"SELECT COUNT(*) FROM dbo.{TempTableName} WHERE JHR_S5_PK = '{scheduledTaskPK}' AND JHR_FailedBatch = 0 AND JHR_FailedIndividually = 0";
				var numRows = (int)TestConnection.ExecuteScalar(numRowsQuery);
				AssertEquals("There should be a matching row for given task PK.", 1, numRows);
			}
		}

		void AssertTempTableDoesNotExist()
		{
			var tableExistsQuery = $"SELECT COUNT(*) WHERE OBJECT_ID('dbo.{TempTableName}', 'U') IS NOT NULL";
			var tableExists = (int)TestConnection.ExecuteScalar(tableExistsQuery);
			AssertEquals("Table should not exist on transformation completion.", 0, tableExists);
		}

		public void TestTransformationInvokesInterfaceMethod_WithValidData()
		{
			var org = new OrgHeader("O1").InsertAndReturnObject(TestConnection);
			PrepareValidTestDataForTransform(org.PK);

			var originalTask = StmScheduleTask.ShallowLoadFromDB(TestConnection, task => task.S5_ParentID == Guid.Parse(JobHistoryReportPK)).Single();
			var originalBytes = originalTask.S5_ScheduleState;
			var originalAsString = Encoding.UTF8.GetString(Compressor.Uncompress(originalBytes));

			var instance = GetNewTestTransformationInstance();
			AssertNoExceptionThrown(() => instance.Run());

			var modifiedTask = StmScheduleTask.ShallowLoadFromDB(TestConnection, task => task.S5_ParentID == Guid.Parse(JobHistoryReportPK)).Single();
			var modifiedBytes = modifiedTask.S5_ScheduleState;

			var modifiedAsString = Encoding.UTF8.GetString(Compressor.Uncompress(modifiedBytes));
			AssertNotEquals(originalAsString, modifiedAsString);

			AssertTempTableDoesNotExist();
		}

		void PrepareValidTestDataForTransform(Guid orgPK)
		{
			var transformType = Assembly
				.LoadFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Enterprise.DocumentEngine.Test.dll"))
				.GetType("Enterprise.DocumentEngine.Transformation.Testing.UpdateScheduledReportTemplateForJobHistoryReportTest", true);
			var transform = Activator.CreateInstance(transformType);
			transformType.GetMethod("PrepareValidTestDataForTransform").Invoke(transform, new object[] { orgPK });
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateScheduledReportTemplateForJobHistoryReportTransformation();
		}
	}
}
