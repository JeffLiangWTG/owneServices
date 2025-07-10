using System;
using System.Collections.Generic;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.EU;

[TestedType(typeof(PopulateCXH_ApplicationCode))]
class PopulateCXH_ApplicationCodeTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new PopulateCXH_ApplicationCode();

	protected override void PrepareTestData()
	{
		var companyPk = TestDataCreator.CreateCompany("CXH", "DE", "EUR");
		var branchPk = TestDataCreator.CreateBranch(companyPk, "CXH", "Port1", "DE");

		var sql = $"""
			ALTER TABLE dbo.CusExitHeader NOCHECK CONSTRAINT Constraint_CXH_ApplicationCode
			INSERT dbo.CusExitHeader (CXH_PK, CXH_ApplicationCode, CXH_ClusterKey, CXH_JobReference, CXH_GB_Branch, CXH_GC_Company, CXH_SystemCreateUser, CXH_SystemCreateTimeUtc, CXH_SystemLastEditUser, CXH_SystemLastEditTimeUtc)
			VALUES	(0x1, 'XIT', 1, 'E0001', '{branchPk}', '{companyPk}', '~BP', '2025-01-01', '~BP', '2025-01-01'),
					(0x2, 'DAC', 2, 'E0002', '{branchPk}', '{companyPk}', '~BP', '2025-01-02', '~BP', '2025-01-02'),
					(0x3, '',    3, 'E0003', '{branchPk}', '{companyPk}', '~BP', '2025-01-03', '~BP', '2025-01-03');
			""";
		TestConnection.ExecuteNonQuery(sql);
	}

	protected override void AssertTransformationResults()
	{
		var resultList = new List<(Guid, string, string, string)>();
		var utcDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
		TestConnection.ExecuteReader("SELECT * FROM dbo.CusExitHeader",
			reader => resultList.Add(((Guid)reader["CXH_PK"], (string)reader["CXH_ApplicationCode"], (string)reader["CXH_SystemLastEditUser"], ((DateTime)reader["CXH_SystemLastEditTimeUtc"]).ToString("yyyy-MM-dd"))));
		AssertContainsExactElementsInAnyOrder(new[]
		{
				(Guid.Parse("00000001-0000-0000-0000-000000000000"), "XIT", "~BP", "2025-01-01"),
				(Guid.Parse("00000002-0000-0000-0000-000000000000"), "DAC", "~BP", "2025-01-02"),
				(Guid.Parse("00000003-0000-0000-0000-000000000000"), "XIT", "E", utcDate),
			}, resultList);
	}

	public void TestWithNoExistingColumn()
	{
		TestRunAndAssertResultsTwice(PrepareTestDataWithNoExistingColumn, _ => { }, AssertTransformationResultsWithNoExistingColumn);
	}

	object PrepareTestDataWithNoExistingColumn()
	{
		PrepareTestData();
		const string sql = """
			ALTER TABLE dbo.CusExitHeader DROP CONSTRAINT DF_CusExitHeader_CXH_ApplicationCode;
			ALTER TABLE dbo.CusExitHeader DROP CONSTRAINT Constraint_CXH_ApplicationCode;
			ALTER TABLE dbo.CusExitHeader DROP COLUMN CXH_ApplicationCode;
			""";
		TestConnection.ExecuteNonQuery(sql);
		return null;
	}

	void AssertTransformationResultsWithNoExistingColumn(object o)
	{
		var resultList = new List<(Guid, string, string, string)>();
		TestConnection.ExecuteReader("SELECT * FROM dbo.CusExitHeader",
			reader => resultList.Add(((Guid)reader["CXH_PK"], (string)reader["CXH_ApplicationCode"], (string)reader["CXH_SystemLastEditUser"], ((DateTime)reader["CXH_SystemLastEditTimeUtc"]).ToString("yyyy-MM-dd"))));
		AssertContainsExactElementsInAnyOrder(new[]
		{
				(Guid.Parse("00000001-0000-0000-0000-000000000000"), "XIT", "~BP", "2025-01-01"),
				(Guid.Parse("00000002-0000-0000-0000-000000000000"), "XIT", "~BP", "2025-01-02"),
				(Guid.Parse("00000003-0000-0000-0000-000000000000"), "XIT", "~BP", "2025-01-03"),
			}, resultList);
	}
}
