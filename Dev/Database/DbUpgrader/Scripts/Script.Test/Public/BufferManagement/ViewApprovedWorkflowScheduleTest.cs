using System;
using System.Linq;
using System.Text;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.BufferManagement;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BufferManagement.Testing
{
	[TestedType(typeof(ViewApprovedWorkflowSchedule))]
	sealed class ViewApprovedWorkflowScheduleTest : DbCreateScriptTest
	{
		public void TestBufferPenetration()
		{
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

			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeader1PK, jobHeader1PK);
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow1_1PK, jobHeader1PK, jobHeader1PK));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow1_2PK, jobHeader1PK, jobHeader1PK));

			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeader2PK, jobHeader2PK);
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow2PK, jobHeader2PK, jobHeader2PK));

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
			AssertBufferPenetration("workflow1_2 does not have a shape (would use jobHeader's penetration)", workflow1_2PK, null);

			AssertBufferPenetration("jobHeader2 is not on a CCPM plan", jobHeader2PK, 0m);
			AssertBufferPenetration("workflow2 is not on a CCPM plan", workflow2PK, 0m);
		}

		void AssertBufferPenetration(string message, Guid processHeaderPK, decimal? expectedPenetration)
		{
			var sql = $"SELECT VWS_BufferPenetration FROM dbo.ViewApprovedWorkflowSchedule WHERE VWS_PK = '{processHeaderPK}'";
			var result = TestConnection.ExecuteScalar(sql);

			AssertEquals(message, expectedPenetration, result);
		}

		public void TestQueryPlan_ShouldNotInvolveProcessHeaderTable()
		{
			var planalyzer = new QueryPlanalyzer("SELECT * FROM dbo.ViewApprovedWorkflowSchedule", TestConnection);
			AssertContainsExactElementsInAnyOrder(Array.Empty<QueryPlanalyzer.IndexAccessDetails>(), planalyzer.IndexScans.Where(x => x.TableName == ProcessHeaderSchema.Constants.TableName));
			AssertContainsExactElementsInAnyOrder(Array.Empty<QueryPlanalyzer.IndexAccessDetails>(), planalyzer.IndexSeeks.Where(x => x.TableName == ProcessHeaderSchema.Constants.TableName));
			AssertContainsExactElementsInAnyOrder(Array.Empty<QueryPlanalyzer.IndexAccessDetails>(), planalyzer.TableScans.Where(x => x.TableName == ProcessHeaderSchema.Constants.TableName));
		}
	}
}

