using System;
using System.Text;
using CargoWise.DbUpgrader.Scripts.Definitions.BufferManagement;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BufferManagement.Testing
{
	[TestedType(typeof(GetBufferZone))]
	sealed class GetBufferZoneTest : DbCreateScriptTest
	{
		public void TestBufferPenetration_ShouldFilterWithinFunction()
		{
			var jobHeaderPK = Guid.NewGuid();
			var workflow1PK = Guid.NewGuid();
			var workflow2PK = Guid.NewGuid();

			var insertSql = new StringBuilder();
			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeaderPK, jobHeaderPK);
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow1PK, jobHeaderPK, jobHeaderPK));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow2PK, jobHeaderPK, jobHeaderPK));

			TestConnection.ExecuteNonQuery(insertSql.ToString());

			var branchPK = BMDbTestHelper.GetABranchPK(TestConnection);
			var departmentPK = BMDbTestHelper.GetADepartmentPK(TestConnection);

			var sql = string.Format(@"
				SELECT BufferZone
				FROM dbo.GetBufferZone('{0}', '{1}', '{2}', DEFAULT)
				", workflow1PK, branchPK, departmentPK);

			var rows = 0;

			using (var reader = TestConnection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					rows++;
				}
			}

			AssertEquals("Should only find one row for the workflow actually specified to the function", 1, rows);
		}

		public void TestBufferPenetration_ShouldUseCCPMBufferPenetrationOrOperationalBuffer()
		{
			var jobHeaderPK = Guid.NewGuid();
			var workflow1PK = Guid.NewGuid();
			var workflow2PK = Guid.NewGuid();
			var workflow3PK = Guid.NewGuid();
			var workflow4PK = Guid.NewGuid();
			var workflow5PK = Guid.NewGuid();

			var jobHeader1ShapePK = Guid.NewGuid();
			var workflow1ShapePK = Guid.NewGuid();
			var workflow2ShapePK = Guid.NewGuid();
			var workflow3ShapePK = Guid.NewGuid();
			var workflow4ShapePK = Guid.NewGuid();

			var insertSql = new StringBuilder();

			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeaderPK, jobHeaderPK);
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow1PK, jobHeaderPK, jobHeaderPK));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow2PK, jobHeaderPK, jobHeaderPK));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow3PK, jobHeaderPK, jobHeaderPK));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow4PK, jobHeaderPK, jobHeaderPK));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow5PK, jobHeaderPK, jobHeaderPK));

			insertSql.Append(BMDbTestHelper.GetShapeInsertSql(jobHeader1ShapePK, "Vote Yes", jobHeaderPK, "E", shapeType: "DIA"));
			insertSql.Append(BMDbTestHelper.GetShapeInsertSql(workflow1ShapePK, "Vote Yes", workflow1PK, "E", shapeType: "DIA"));
			insertSql.Append(BMDbTestHelper.GetShapeInsertSql(workflow2ShapePK, "Vote Yes", workflow2PK, "E", shapeType: "DIA"));
			insertSql.Append(BMDbTestHelper.GetShapeInsertSql(workflow3ShapePK, "Vote Yes", workflow3PK, "E", shapeType: "DIA"));
			insertSql.Append(BMDbTestHelper.GetShapeInsertSql(workflow4ShapePK, "Vote Yes", workflow4PK, "E", shapeType: "DIA"));

			insertSql.AppendFormat(BMDbTestHelper.GetShapeScheduleInsertSql(jobHeader1ShapePK, isBuffered: true, bufferPenetrationPercent: 99m));
			insertSql.AppendFormat(BMDbTestHelper.GetShapeScheduleInsertSql(workflow1ShapePK, isBuffered: true, bufferPenetrationPercent: 32m));
			insertSql.AppendFormat(BMDbTestHelper.GetShapeScheduleInsertSql(workflow2ShapePK, isBuffered: true, bufferPenetrationPercent: 34m));
			insertSql.AppendFormat(BMDbTestHelper.GetShapeScheduleInsertSql(workflow3ShapePK, isBuffered: true, bufferPenetrationPercent: 67m));
			insertSql.AppendFormat(BMDbTestHelper.GetShapeScheduleInsertSql(workflow4ShapePK, isBuffered: true, bufferPenetrationPercent: 101m));

			TestConnection.ExecuteNonQuery(insertSql.ToString());

			AssertBufferZone("jobHeader buffer zone", jobHeaderPK, 1);
			AssertBufferZone("workflow1 buffer zone", workflow1PK, 3);
			AssertBufferZone("workflow2 buffer zone", workflow2PK, 2);
			AssertBufferZone("workflow3 buffer zone", workflow3PK, 1);
			AssertBufferZone("workflow4 buffer zone", workflow4PK, 0);
			AssertBufferZone("workflow5 should inherit jobHeader buffer zone", workflow5PK, 1);
		}

		void AssertBufferZone(string message, Guid processHeaderPK, int? expectedZone)
		{
			var branchPK = BMDbTestHelper.GetABranchPK(TestConnection);
			var departmentPK = BMDbTestHelper.GetADepartmentPK(TestConnection);
			var sql = string.Format(@"
				SELECT BufferZone FROM dbo.ProcessHeader
				CROSS APPLY dbo.GetBufferZone(FH_PK, '{0}', '{1}', DEFAULT)
				WHERE FH_PK = '{2}'", branchPK, departmentPK, processHeaderPK);

			var result = TestConnection.ExecuteScalar(sql);

			if (expectedZone == null)
			{
				AssertEquals(message, DBNull.Value, result);
			}
			else
			{
				AssertEquals(message, expectedZone.Value, (int)result);
			}
		}
	}
}

