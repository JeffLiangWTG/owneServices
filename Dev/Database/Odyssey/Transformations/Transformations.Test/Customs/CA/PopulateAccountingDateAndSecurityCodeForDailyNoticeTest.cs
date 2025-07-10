using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.CA
{
	[TestedType(typeof(PopulateAccountingDateAndSecurityCodeForDailyNotice))]
	public class PopulateAccountingDateAndSecurityCodeForDailyNoticeTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertB2_ProcessDate(pk1, new DateTime(2024, 11, 1), "20241");
			AssertB2_ProcessDate(pk2, new DateTime(2024, 11, 1), "20242");
			AssertB2_ProcessDate(pk3, null, "");
			AssertB2_ProcessDate(pk4, null, "");
			AssertB2_ProcessDate(pk5, null, "");
			AssertB2_ProcessDate(pk6, new DateTime(2024, 11, 2), "20244");
			AssertB2_ProcessDate(pk7, null, "");
			AssertB2_ProcessDate(pk8, new DateTime(2024, 9, 30), "20245");
			AssertB2_ProcessDate(pk9, null, "");
			AssertB2_ProcessDate(pk10, null, "");
		}

		void AssertB2_ProcessDate(Guid pk, DateTime? accountingDate, string entryFilerCode)
		{
			var sql = $"SELECT B2_ProcessDate, B2_EntryFilerCode FROM dbo.CusStatementHeader WHERE B2_PK = '{pk}'";
			using (var cmd = TestConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				var count = 0;
				if (reader.Read())
				{
					count++;
					if (accountingDate == null)
					{
						AssertEquals(DBNull.Value, reader[0]);
					}
					else
					{
						AssertEquals(accountingDate, (DateTime)reader[0]);
					}
					AssertEquals(entryFilerCode, (string)reader[1]);
				}
				AssertEquals(1, count);
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new PopulateAccountingDateAndSecurityCodeForDailyNotice();
		}

		protected override void PrepareTestData()
		{
			var dataCreator = new TransformationTestDataCreator();
			var caCompanyPK = dataCreator.CreateCompany(Guid.NewGuid(), "CP1", "CA", "CAD");
			var usCompanyPK = dataCreator.CreateCompany(Guid.NewGuid(), "CP2", "US", "USD");
			CreateCusStatementHeader(caCompanyPK, pk1, "2024-10-01", 0, "I", "DN-1", "", "", new string[] { "2024-11-01" }, new string[] { "20241112" });
			CreateCusStatementHeader(caCompanyPK, pk2, "2024-10-01", 0, "I", "DN-2", "", "", new string[] { "2024-11-01", "2024-11-02" }, new string[] { "20242112", "20242112" });
			CreateCusStatementHeader(caCompanyPK, pk3, "2024-09-30", 0, "I", "DN-3", "", "", new string[] { "2024-11-01" }, new string[] { "20243112" });
			CreateCusStatementHeader(caCompanyPK, pk4, "2024-10-01", 1, "I", "DN-4", "", "", new string[] { "2024-11-01" }, new string[] { "20243112" });
			CreateCusStatementHeader(caCompanyPK, pk5, "2024-10-01", 0, "R", "DN-5", "", "", new string[] { "2024-11-01" }, new string[] { "20243112" });
			CreateCusStatementHeader(caCompanyPK, pk6, "2024-10-01", 0, "I", "DN-6", "2024-11-02", "20244", new string[] { "2024-11-01" }, new string[] { "20243112" });
			CreateCusStatementHeader(caCompanyPK, pk7, "2024-10-01", 0, "I", "ABC", "", "", new string[] { "2024-11-01" }, new string[] { "20243112" });
			CreateCusStatementHeader(caCompanyPK, pk8, "2024-10-01", 0, "I", "DN-8", "", "", new string[] { "2024-09-30", "2024-11-02" }, new string[] { "20245112", "20245112" });
			CreateCusStatementHeader(caCompanyPK, pk9, "2024-10-01", 0, "I", "DN-9", "", "", new string[] { "" }, new string[] { "" });
			CreateCusStatementHeader(usCompanyPK, pk10, "2024-10-01", 0, "I", "DN-10", "", "", new string[] { "2024-11-01" }, new string[] { "20241112" });
		}

		void CreateCusStatementHeader(Guid companyPK, Guid pk, string createTime, int isMonthlyStatement, string statementType, string statementNumber, string accountingDate, string entryFilerCode, string[] lineDates, String[] entryNums)
		{
			var sql = "";
			if (accountingDate.IsNullOrEmpty())
			{
				sql += $"INSERT INTO dbo.CusStatementHeader (B2_PK, B2_GC, B2_SystemCreateTimeUtc, B2_IsMonthlyStatement, B2_StatementType, B2_StatementNumber, B2_EntryFilerCode, B2_SystemCreateUser, B2_SystemLastEditTimeUtc, B2_SystemLastEditUser) VALUES('{pk}', '{companyPK}', '{createTime}', {isMonthlyStatement}, '{statementType}', '{statementNumber}', '{entryFilerCode}', '~BP', GetUtcDate(), '~BP')";
			}
			else
			{
				sql += $"INSERT INTO dbo.CusStatementHeader (B2_PK, B2_GC, B2_SystemCreateTimeUtc, B2_IsMonthlyStatement, B2_StatementType, B2_StatementNumber, B2_EntryFilerCode, B2_ProcessDate, B2_SystemCreateUser, B2_SystemLastEditTimeUtc, B2_SystemLastEditUser) VALUES('{pk}', '{companyPK}', '{createTime}', {isMonthlyStatement}, '{statementType}', '{statementNumber}', '{entryFilerCode}', '{accountingDate}', '~BP', GetUtcDate(), '~BP')";
			}

			for (int i = 0; i < lineDates.Length; i++)
			{
				var lineDate = lineDates[i];
				var entryNum = entryNums[i];
				if (lineDate.IsNullOrEmpty())
				{
					sql += $"\r\nINSERT INTO dbo.CusStatementLine (B3_PK, B3_B2, B3_EntryNum, B3_EntryFilerCode, B3_SystemCreateTimeUtc, B3_SystemCreateUser, B3_SystemLastEditTimeUtc, B3_SystemLastEditUser) VALUES(NEWID(), '{pk}', '{entryNum}', 'SV9', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
				}
				else
				{
					sql += $"\r\nINSERT INTO dbo.CusStatementLine (B3_PK, B3_B2, B3_EntryNum, B3_EntryFilerCode, B3_ScheduledProcessDate, B3_SystemCreateTimeUtc, B3_SystemCreateUser, B3_SystemLastEditTimeUtc, B3_SystemLastEditUser) VALUES(NEWID(), '{pk}', '{entryNum}', 'SV9', '{lineDate}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
				}
			}

			Db.Connection.ExecuteNonQuery(sql);
		}

		Guid pk1 = Guid.NewGuid();
		Guid pk2 = Guid.NewGuid();
		Guid pk3 = Guid.NewGuid();
		Guid pk4 = Guid.NewGuid();
		Guid pk5 = Guid.NewGuid();
		Guid pk6 = Guid.NewGuid();
		Guid pk7 = Guid.NewGuid();
		Guid pk8 = Guid.NewGuid();
		Guid pk9 = Guid.NewGuid();
		Guid pk10 = Guid.NewGuid();

		public override string[] expectedIndex => new string[] { "NONCLUSTERED INDEX [_WTG__Populate Accounting Date And Account Security Code For Daily Notice Statement._1] ON [dbo].[CusStatementHeader] ([B2_StatementNumber]) INCLUDE ([B2_EntryFilerCode], [B2_GC], [B2_ProcessDate], [B2_StatementType], [B2_SystemLastEditTimeUtc], [B2_SystemLastEditUser]) WHERE ([B2_SystemCreateTimeUtc]>='2024-10-01' AND [B2_IsMonthlyStatement]=(0) AND [B2_StatementType]<>'R') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)" };
	}
}
