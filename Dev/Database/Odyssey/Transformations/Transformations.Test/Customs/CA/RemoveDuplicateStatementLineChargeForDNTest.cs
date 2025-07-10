using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.CA
{
	[TestedType(typeof(RemoveDuplicateStatementLineChargeForDN))]
	public class RemoveDuplicateStatementLineChargeForDNTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RemoveDuplicateStatementLineChargeForDN();
		}

		protected override void AssertTransformationResults()
		{
			AssertResult(charge11_1, 1);
			AssertResult(charge11_2, 0);
			AssertResult(charge21_1, 1);
			AssertResult(charge21_2, 0);
			AssertResult(charge22_1, 1);
			AssertResult(charge22_2, 0);
			AssertResult(charge31_1, 1);
			AssertResult(charge31_2, 1);
			AssertResult(charge32_1, 1);
			AssertResult(charge32_2, 1);
			AssertResult(charge41_1, 1);
			AssertResult(charge41_2, 1);
			AssertResult(charge51_1, 1);
			AssertResult(charge51_2, 1);
			AssertResult(charge61_1, 1);
			AssertResult(charge61_2, 1);
			AssertResult(charge71_1, 1);
			AssertResult(charge71_2, 1);
		}

		void AssertResult(Guid pk, int expectedCount)
		{
			var sql = $"SELECT 1 FROM dbo.CusStatementLineCharge WHERE B4_PK = '{pk}'";
			using (var cmd = TestConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				var actualCount = 0;
				if (reader.Read())
				{
					actualCount++;
				}
				AssertEquals(expectedCount, actualCount);
			}
		}

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var caCompanyPK = testDataCreator.CreateGlbCompany("AAA", "CA");
			var caBranchPK = testDataCreator.CreateGlbBranch("DDD", caCompanyPK);

			var header1 = CreateCusStatementHeader(caCompanyPK, "2024-10-01", 0, "I", "DN-1");
			var line11 = CreateCusStatementLine(header1);
			CreateCusStatementLineCharge(charge11_1, line11, "DTY", "2024-10-01");
			CreateCusStatementLineCharge(charge11_2, line11, "DTY", "2024-10-02");

			var header2 = CreateCusStatementHeader(caCompanyPK, "2024-10-01", 0, "I", "DN-2");
			var line21 = CreateCusStatementLine(header2);
			CreateCusStatementLineCharge(charge21_1, line21, "DTY", "2024-10-01");
			CreateCusStatementLineCharge(charge21_2, line21, "DTY", "2024-10-02");
			var line22 = CreateCusStatementLine(header2);
			CreateCusStatementLineCharge(charge22_1, line22, "DTY", "2024-10-01");
			CreateCusStatementLineCharge(charge22_2, line22, "DTY", "2024-10-02");

			var header3 = CreateCusStatementHeader(caCompanyPK, "2024-10-01", 0, "I", "DN-3");
			var line31 = CreateCusStatementLine(header3);
			CreateCusStatementLineCharge(charge31_1, line31, "DTY", "2024-10-01");
			CreateCusStatementLineCharge(charge31_2, line31, "GST", "2024-10-02");
			var line32 = CreateCusStatementLine(header3);
			CreateCusStatementLineCharge(charge32_1, line32, "DTY", "2024-10-01");
			CreateCusStatementLineCharge(charge32_2, line32, "GST", "2024-10-02");

			var header4 = CreateCusStatementHeader(caCompanyPK, "2024-09-30", 0, "I", "DN-4");
			var line41 = CreateCusStatementLine(header4);
			CreateCusStatementLineCharge(charge41_1, line41, "DTY", "2024-10-01");
			CreateCusStatementLineCharge(charge41_2, line41, "DTY", "2024-10-02");

			var header5 = CreateCusStatementHeader(caCompanyPK, "2024-10-01", 1, "I", "DN-5");
			var line51 = CreateCusStatementLine(header5);
			CreateCusStatementLineCharge(charge51_1, line51, "DTY", "2024-10-01");
			CreateCusStatementLineCharge(charge51_2, line51, "DTY", "2024-10-02");

			var header6 = CreateCusStatementHeader(caCompanyPK, "2024-10-01", 0, "R", "DN-6");
			var line61 = CreateCusStatementLine(header6);
			CreateCusStatementLineCharge(charge61_1, line61, "DTY", "2024-10-01");
			CreateCusStatementLineCharge(charge61_2, line61, "DTY", "2024-10-02");

			var header7 = CreateCusStatementHeader(caCompanyPK, "2024-10-01", 0, "I", "ABC");
			var line71 = CreateCusStatementLine(header7);
			CreateCusStatementLineCharge(charge71_1, line71, "DTY", "2024-10-01");
			CreateCusStatementLineCharge(charge71_2, line71, "DTY", "2024-10-02");
		}

		Guid CreateCusStatementHeader(Guid companyPK, string createTime, int isMonthlyStatement, string statementType, string statementNumber)
		{
			var pk = Guid.NewGuid();
			var sql = $"INSERT INTO dbo.CusStatementHeader (B2_PK, B2_GC, B2_SystemCreateTimeUtc, B2_IsMonthlyStatement, B2_StatementType, B2_StatementNumber, B2_SystemCreateUser, B2_SystemLastEditTimeUtc, B2_SystemLastEditUser) VALUES('{pk}', '{companyPK}', '{createTime}', {isMonthlyStatement}, '{statementType}', '{statementNumber}', '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(sql);
			return pk;
		}

		Guid CreateCusStatementLine(Guid parent)
		{
			var pk = Guid.NewGuid();
			var sql = $"INSERT INTO dbo.CusStatementLine (B3_PK, B3_B2, B3_SystemCreateTimeUtc, B3_SystemCreateUser, B3_SystemLastEditTimeUtc, B3_SystemLastEditUser) VALUES('{pk}', '{parent}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(sql);
			return pk;
		}

		void CreateCusStatementLineCharge(Guid pk, Guid parent, string chargeType, string createTime)
		{
			var sql = $"INSERT INTO dbo.CusStatementLineCharge (B4_PK, B4_B3, B4_ChargeType, B4_SystemCreateTimeUtc, B4_SystemCreateUser, B4_SystemLastEditTimeUtc, B4_SystemLastEditUser) VALUES('{pk}', '{parent}', '{chargeType}', '{createTime}', '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(sql);
		}

		Guid charge11_1 = Guid.NewGuid();
		Guid charge11_2 = Guid.NewGuid();
		Guid charge21_1 = Guid.NewGuid();
		Guid charge21_2 = Guid.NewGuid();
		Guid charge22_1 = Guid.NewGuid();
		Guid charge22_2 = Guid.NewGuid();
		Guid charge31_1 = Guid.NewGuid();
		Guid charge31_2 = Guid.NewGuid();
		Guid charge32_1 = Guid.NewGuid();
		Guid charge32_2 = Guid.NewGuid();
		Guid charge41_1 = Guid.NewGuid();
		Guid charge41_2 = Guid.NewGuid();
		Guid charge51_1 = Guid.NewGuid();
		Guid charge51_2 = Guid.NewGuid();
		Guid charge61_1 = Guid.NewGuid();
		Guid charge61_2 = Guid.NewGuid();
		Guid charge71_1 = Guid.NewGuid();
		Guid charge71_2 = Guid.NewGuid();

		public override string[] expectedIndex => new string[] { "NONCLUSTERED INDEX [_WTG__Remove the Duplicate Charges of Statement Line of Daily Notice._1] ON [dbo].[CusStatementHeader] ([B2_StatementNumber]) INCLUDE ([B2_GC], [B2_StatementType]) WHERE ([B2_SystemCreateTimeUtc]>='2024-10-01' AND [B2_IsMonthlyStatement]=(0) AND [B2_StatementType]<>'R') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)" };
	}
}
