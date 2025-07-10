using System;
using System.Text;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.BufferManagement;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BufferManagement.Testing
{
	[TestedType(typeof(GetBufferPenetration))]
	sealed class GetBufferPenetrationTest : DbCreateScriptTest
	{
		[TestDate(2014, 10, 20)] // Midnight UTC, 11am AEDT
		public void TestBufferPenetration_ShouldUseDepartmentWorkingHours()
		{
			var systemPK = Guid.NewGuid();
			var bufferPK = Guid.NewGuid();

			var jobHeaderPK = Guid.NewGuid();
			var workflowPK = Guid.NewGuid();

			var insertSql = new StringBuilder();
			var workflowReleaseTime = referenceDate.AddMinutes(-6).ToSqlFormat();

			insertSql.AppendFormat(BMDbTestHelper.BMSystemInsertSql, systemPK, "WTGDEV");
			insertSql.AppendFormat(BMDbTestHelper.GetBMComponentInsertSql(bufferPK, systemPK, "BUF", "Dat Buffer", timespanInMinutes: 60));

			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeaderPK, jobHeaderPK);
			insertSql.AppendFormat(BMDbTestHelper.ProcessHeaderWithReleaseDetailsInsertSql, workflowPK, jobHeaderPK, jobHeaderPK, bufferPK, workflowReleaseTime);

			TestConnection.ExecuteNonQuery(insertSql.ToString());

			AssertBufferPenetration("workflow should convert UTC to local for working hours calculation", workflowPK, 0.1m);

			var updateSql = string.Format("UPDATE dbo.ProcessHeader SET FH_ReleaseDateTime = '{0}' WHERE FH_PK = '{1}'", referenceDate.AddHours(-3).ToSqlFormat(), workflowPK);
			TestConnection.ExecuteNonQuery(updateSql);

			AssertBufferPenetration("3 hours prior is only one working hour ago", workflowPK, 1m);
		}

		[TestDate(2014, 10, 20)]
		public void TestBufferPenetration_ConsidersIteratedWorkflows_WhenReleaseDateOnIteratedWorkflowIsNull()
		{
			var workflowPK = Guid.NewGuid();
			var workflowIterPK = Guid.NewGuid();

			bool iteratedWorkflowReleaseDateIsNull = true;
			bool syncBufferPenetration = true;
			SetupIteratedWorkflowTest(iteratedWorkflowReleaseDateIsNull, syncBufferPenetration, out workflowPK, out workflowIterPK);

			// so, the buffer penetration should be:
			// (Age - PlannedDuration + Estimates) / BufferLength.
			// BufferLength = 100
			// Age = 60
			// PlanedDuration = 50
			// Estimate = (20 * 1.5) + 7 = 37
			AssertBufferPenetration("Expected workflow penetration for parent is (60 - 50 + 37) / 100 = 0.47", workflowPK, 0.47m);

			AssertBufferPenetration("Expecting a non-NULL penetration for the iterated workflow, whose penetration should be the same as its parent.", workflowIterPK, 0.47m);
		}

		[TestDate(2014, 10, 20)]
		public void TestBufferPenetration_ConsidersIteratedWorkflows_WhenReleaseDateOnIteratedWorkflowIsNotNull()
		{
			var workflowPK = Guid.NewGuid();
			var workflowIterPK = Guid.NewGuid();

			bool iteratedWorkflowReleaseDateIsNull = false;
			bool syncBufferPenetration = true;
			SetupIteratedWorkflowTest(iteratedWorkflowReleaseDateIsNull, syncBufferPenetration, out workflowPK, out workflowIterPK);

			// so, the buffer penetration should be:
			// (Age - PlannedDuration + Estimates) / BufferLength.
			// BufferLength = 100
			// Age = 60
			// PlanedDuration = 50
			// Estimate = (20 * 1.5) + 7 = 37
			AssertBufferPenetration("Expected workflow penetration for parent is (60 - 50 + 37) / 100 = 0.47", workflowPK, 0.47m);

			AssertBufferPenetration("Expecting a non-NULL penetration for the iterated workflow, whose penetration should be the same as its parent.", workflowIterPK, 0.47m);
		}

		[TestDate(2014, 10, 20)]
		public void TestBufferPenetration_ConsidersIteratedWorkflows_WhenNotSyncingBufferPenetration()
		{
			var workflowPK = Guid.NewGuid();
			var workflowIterPK = Guid.NewGuid();

			bool iteratedWorkflowReleaseDateIsNull = false;
			bool syncBufferPenetration = false;
			SetupIteratedWorkflowTest(iteratedWorkflowReleaseDateIsNull, syncBufferPenetration, out workflowPK, out workflowIterPK);

			// for the parent workflow, the buffer penetration should be:
			// (Age - PlannedDuration + Estimates) / BufferLength.
			// BufferLength = 100
			// Age = 60
			// PlanedDuration = 50
			// Estimate = (20 * 1.5) + 7 = 37
			AssertBufferPenetration("Expected workflow penetration for parent is (60 - 50 + 37) / 100 = 0.47", workflowPK, 0.47m);

			// for the iterated workflow, the buffer penetration should be:
			// (Age - PlannedDuration + Estimates) / BufferLength.
			// BufferLength = 100
			// Age = 60
			// PlanedDuration = 40
			// Estimate = (20 * 1.5) + 20 = 50
			AssertBufferPenetration("Expecting a non-NULL penetration for the iterated workflow, whose penetration should differ from its parent.", workflowIterPK, 0.60m);
		}

		[TestDate(2014, 10, 20)]
		public void TestBufferPenetration_ConsidersTaskEstimates()
		{
			var systemPK = Guid.NewGuid();
			var bufferPK = Guid.NewGuid();

			var parentID = Guid.NewGuid();
			var jobHeaderPK = Guid.NewGuid();
			var workflowPK = Guid.NewGuid();

			var insertSql = new StringBuilder();
			insertSql.AppendFormat(BMDbTestHelper.BMSystemInsertSql, systemPK, "WTGDEV");
			var bufferLengthInMinutes = 100;
			insertSql.AppendFormat(BMDbTestHelper.GetBMComponentInsertSql(bufferPK, systemPK, "BUF", "Test Buffer", timespanInMinutes: bufferLengthInMinutes));
			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeaderPK, jobHeaderPK);

			// workflow was released 40 minutes ago
			var workflowReleaseTime = referenceDate.AddMinutes(-40).ToSqlFormat();
			insertSql.AppendFormat(BMDbTestHelper.ProcessHeaderWithReleaseDetailsInsertSql, workflowPK, jobHeaderPK, jobHeaderPK, bufferPK, workflowReleaseTime);
			insertSql.AppendLine(string.Format(@"UPDATE dbo.ProcessHeader SET FH_PlannedDurationInMinutes = 50 WHERE FH_PK = '{0}'", workflowPK));
			var closedTaskPK = Guid.NewGuid();

			var startOfTheYear = new DateTime(2014, 01, 01);
			// closed and cancelled task estimates will be ignored
			insertSql.AppendFormat(BMDbTestHelper.ProcessTaskClosedInsertSql, closedTaskPK, "CLS", startOfTheYear.AddMinutes(20).ToSqlFormat(), 2, startOfTheYear.AddMinutes(10).ToSqlFormat(), workflowPK, "CAST(0 as SMALLDATETIME)", parentID);
			var cancelledTaskPK = Guid.NewGuid();
			insertSql.AppendFormat(BMDbTestHelper.ProcessTaskInsertSql, cancelledTaskPK, "CAN", startOfTheYear.AddMinutes(20).ToSqlFormat(), 2, startOfTheYear.AddMinutes(10).ToSqlFormat(), workflowPK, parentID);
			var openTaskWithStandartEstimatePK = Guid.NewGuid();

			insertSql.AppendFormat(BMDbTestHelper.ProcessTaskInsertSql, openTaskWithStandartEstimatePK, "OPN", startOfTheYear.AddMinutes(20).ToSqlFormat(), 2, EmptyDateTime, workflowPK, parentID);
			var openTaskWithEstimatedTimeToCompletePK = Guid.NewGuid();
			insertSql.AppendFormat(BMDbTestHelper.ProcessTaskInsertSql, openTaskWithEstimatedTimeToCompletePK, "OPN", startOfTheYear.AddMinutes(30).ToSqlFormat(), 2, startOfTheYear.AddMinutes(7).ToSqlFormat(), workflowPK, parentID);
			TestConnection.ExecuteNonQuery(insertSql.ToString());
			// so, the buffer penetration should be:
			// (Age - PlannedDuration + Estimates) / BufferLength.
			// BufferLength = 100
			// Age = 40
			// PlanedDuration = 50
			// Estimate = (20 * 1.5) + 7 = 37
			AssertBufferPenetration("Expected workflow penetration is (40 - 50 + 37) / 100 = 0.27", workflowPK, 0.27m);
		}

		[TestDate(2014, 10, 20)]
		public void TestBufferPenetration_ShouldUseCCPMBufferPenetrationOrOperationalBuffer()
		{
			AssertBufferPenetration_ShouldUseCCPMBufferPenetrationOrOperationalBuffer(0.1m);
		}

		[TestDate(2014, 10, 20)]
		[RequiresLargeLogFile]
		public void TestBranchWithNoTimeZoneOffsetCached_ShouldFallbackToUtcOffsetOfZero()
		{
			TestConnection.ExecuteNonQuery("DELETE FROM RefDatabase_RefUNLOCOUtcOffset");

			AssertBufferPenetration_ShouldUseCCPMBufferPenetrationOrOperationalBuffer(0m);
		}

		void SetupIteratedWorkflowTest(bool iteratedWorkflowReleaseDateIsNull, bool syncBufferPenetration, out Guid workflow, out Guid workflowIter)
		{
			var systemPK = Guid.NewGuid();
			var bufferPK = Guid.NewGuid();

			var jobHeaderPK = Guid.NewGuid();
			var parentID = Guid.NewGuid();
			var workflowPK = Guid.NewGuid();
			var workflowIterPK = Guid.NewGuid();
			var linkPK = Guid.NewGuid();

			var insertSql = new StringBuilder();
			insertSql.AppendFormat(BMDbTestHelper.BMSystemInsertSql, systemPK, "WTGDEV");
			var bufferLengthInMinutes = 100;
			insertSql.AppendFormat(BMDbTestHelper.GetBMComponentInsertSql(bufferPK, systemPK, "BUF", "Test Buffer", timespanInMinutes: bufferLengthInMinutes));
			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeaderPK, parentID);

			// workflow was released 60 minutes ago
			var workflowReleaseTime = referenceDate.AddMinutes(-60).ToSqlFormat();
			insertSql.AppendFormat(BMDbTestHelper.ProcessHeaderWithReleaseDetailsInsertSql, workflowPK, jobHeaderPK, parentID, bufferPK, workflowReleaseTime);

			if (iteratedWorkflowReleaseDateIsNull)
			{
				insertSql.AppendFormat(BMDbTestHelper.ProcessHeaderWithNullReleaseDateTimeInsertSql, workflowIterPK, jobHeaderPK, parentID, bufferPK);
			}
			else
			{
				insertSql.AppendFormat(BMDbTestHelper.ProcessHeaderWithReleaseDetailsInsertSql, workflowIterPK, jobHeaderPK, parentID, bufferPK, workflowReleaseTime);
			}
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderLinkInsertSql(linkPK, workflowIterPK, workflowPK, "PCH", syncBufferPenetration));
			insertSql.AppendFormat(@"UPDATE dbo.ProcessHeader SET FH_PlannedDurationInMinutes = 50 WHERE FH_PK = '{0}'", workflowPK);
			insertSql.AppendFormat(@"UPDATE dbo.ProcessHeader SET FH_PlannedDurationInMinutes = 50 WHERE FH_PK = '{0}'", workflowIterPK);

			var startOfTheYear = new DateTime(2014, 01, 01);
			var openTaskWithStandardEstimatePK = Guid.NewGuid();
			insertSql.AppendFormat(BMDbTestHelper.ProcessTaskInsertSql, openTaskWithStandardEstimatePK, "OPN", startOfTheYear.AddMinutes(20).ToSqlFormat(), 2, EmptyDateTime, workflowPK, parentID);

			var openTaskWithEstimatedTimeToCompletePK = Guid.NewGuid();
			insertSql.AppendFormat(BMDbTestHelper.ProcessTaskInsertSql, openTaskWithEstimatedTimeToCompletePK, "OPN", startOfTheYear.AddMinutes(30).ToSqlFormat(), 2, startOfTheYear.AddMinutes(7).ToSqlFormat(), workflowPK, parentID);

			var iterTaskPK = Guid.NewGuid();
			insertSql.AppendFormat(BMDbTestHelper.ProcessTaskInsertSql, iterTaskPK, "OPN", startOfTheYear.AddMinutes(20).ToSqlFormat(), 2, startOfTheYear.AddMinutes(20).ToSqlFormat(), workflowIterPK, parentID);

			var iterTask2PK = Guid.NewGuid();
			insertSql.AppendFormat(BMDbTestHelper.ProcessTaskInsertSql, iterTask2PK, "OPN", startOfTheYear.AddMinutes(20).ToSqlFormat(), 2, EmptyDateTime, workflowIterPK, parentID);

			var insertSqlString = insertSql.ToString();
			TestConnection.ExecuteNonQuery(insertSqlString);

			workflow = workflowPK;
			workflowIter = workflowIterPK;
		}

		void AssertBufferPenetration_ShouldUseCCPMBufferPenetrationOrOperationalBuffer(decimal expectedWorkflow2Penetration)
		{
			var systemPK = Guid.NewGuid();
			var bufferPK = Guid.NewGuid();

			var jobHeader1PK = Guid.NewGuid();
			var jobHeader2PK = Guid.NewGuid();
			var workflow1_1PK = Guid.NewGuid();
			var workflow1_2PK = Guid.NewGuid();
			var workflow2PK = Guid.NewGuid();

			var jobHeader1ShapePK = Guid.NewGuid();
			var jobHeader2ShapePK = Guid.NewGuid();
			var workflow1ShapePK = Guid.NewGuid();
			var workflow2ShapePK = Guid.NewGuid();

			var insertSql = new StringBuilder();
			var workflowReleaseTime = referenceDate.AddMinutes(-6).ToSqlFormat();

			insertSql.AppendFormat(BMDbTestHelper.BMSystemInsertSql, systemPK, "WTGDEV");
			insertSql.AppendFormat(BMDbTestHelper.GetBMComponentInsertSql(bufferPK, systemPK, "BUF", "Dat Buffer", timespanInMinutes: 60));

			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeader1PK, jobHeader1PK);
			insertSql.AppendFormat(BMDbTestHelper.ProcessHeaderWithReleaseDetailsInsertSql, workflow1_1PK, jobHeader1PK, jobHeader1PK, bufferPK, workflowReleaseTime);
			insertSql.AppendFormat(BMDbTestHelper.ProcessHeaderWithReleaseDetailsInsertSql, workflow1_2PK, jobHeader1PK, jobHeader1PK, bufferPK, workflowReleaseTime);

			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeader2PK, jobHeader2PK);
			insertSql.AppendFormat(BMDbTestHelper.ProcessHeaderWithReleaseDetailsInsertSql, workflow2PK, jobHeader2PK, jobHeader2PK, bufferPK, workflowReleaseTime);

			insertSql.Append(BMDbTestHelper.GetShapeInsertSql(jobHeader1ShapePK, "Vote Yes", jobHeader1PK, "E", shapeType: "DIA"));
			insertSql.Append(BMDbTestHelper.GetShapeInsertSql(workflow1ShapePK, "Vote Yes", workflow1_1PK, "E", shapeType: "DIA"));

			insertSql.Append(BMDbTestHelper.GetShapeInsertSql(jobHeader2ShapePK, "Vote Yes", jobHeader2PK, "E", shapeType: "DIA"));
			insertSql.Append(BMDbTestHelper.GetShapeInsertSql(workflow2ShapePK, "Vote Yes", workflow2PK, "E", shapeType: "DIA"));

			insertSql.AppendFormat(BMDbTestHelper.GetShapeScheduleInsertSql(jobHeader1ShapePK, isBuffered: true, bufferPenetrationPercent: 690m));
			insertSql.AppendFormat(BMDbTestHelper.GetShapeScheduleInsertSql(workflow1ShapePK, isBuffered: true, bufferPenetrationPercent: 960m));
			insertSql.AppendFormat(BMDbTestHelper.GetShapeScheduleInsertSql(jobHeader2ShapePK, isBuffered: false, bufferPenetrationPercent: 690m));
			insertSql.AppendFormat(BMDbTestHelper.GetShapeScheduleInsertSql(workflow2ShapePK, isBuffered: false, bufferPenetrationPercent: 960m));

			TestConnection.ExecuteNonQuery(insertSql.ToString());

			AssertBufferPenetration("jobHeader1 shape penetration", jobHeader1PK, 6.9m);
			AssertBufferPenetration("workflow1_1 shape penetration", workflow1_1PK, 9.6m);
			AssertBufferPenetration("workflow1_2 should use jobHeader's penetration", workflow1_2PK, 6.9m);

			AssertBufferPenetration("jobHeader2 is not on a CCPM plan", jobHeader2PK, null);
			AssertBufferPenetration("workflow2 is not on a CCPM plan so should use operational buffer", workflow2PK, expectedWorkflow2Penetration);
		}

		void AssertBufferPenetration(string message, Guid processHeaderPK, decimal? expectedPenetration)
		{
			var sql = string.Format(@"
				SELECT BufferPenetration FROM dbo.ProcessHeader
				CROSS APPLY dbo.GetBufferPenetration(FH_PK, '{0}', '{1}', '{2}')
				WHERE FH_PK = '{3}'", branchPK, departmentPK, referenceDate.ToSqlFormat(), processHeaderPK);

			var result = TestConnection.ExecuteScalar(sql);

			if (expectedPenetration == null)
			{
				AssertEquals(message, DBNull.Value, result);
			}
			else
			{
				AssertEquals(message, expectedPenetration.Value, result);
			}
		}

		Guid branchPK, departmentPK;

		protected override void SetUp()
		{
			base.SetUp();

			var companyPK = TestDataCreator.CreateCompany("DEA", "AU", "AUD");
			branchPK = TestDataCreator.CreateBranch(companyPK, "DEA", "AUSYD", referenceDate);
			departmentPK = TestDataCreator.CreateDepartment("DNK", referenceDate, mondayHours: "                    ****"); // Works 10:00-12:00 on Monday
		}

		static readonly string EmptyDateTime = string.Empty;
		static readonly DateTime referenceDate = new DateTime(2014, 10, 20);
	}
}
