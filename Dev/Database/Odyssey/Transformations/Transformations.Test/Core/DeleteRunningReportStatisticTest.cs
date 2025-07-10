using System;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Core;

[TestedType(typeof(DeleteRunningReportStatistic))]
public class DeleteRunningReportStatisticTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance()
	{
		return new DeleteRunningReportStatistic();
	}

	protected override void PrepareTestData()
	{
		var helper = new TestDbHelper(TestConnection);
		helper.Insert(StmReportRunSchema.Constants.TableName, new
		{
			RRI_PK = Guid.NewGuid(),
			RRI_StartTimeUtc = DateTime.UtcNow,
			RRI_ReportDescription = "Preview Task",
			RRI_Status = "RUN"
		});
	}

	protected override void AssertTransformationResults()
	{
		var sql = @"SELECT COUNT(*) FROM dbo.StmReportRun WHERE RRI_ReportDescription ='Preview Task' AND RRI_Status = 'RUN' ";
		AssertEquals(0, TestConnection.ExecuteScalar<int>(sql));
	}
}