using System;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(GLSummarySP_Multilingual))]
	class GLSummarySP_MultilingualEDWTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestGLSummarySP_Multilingual()
		{
			PrepareData();
			var companyPK = "878D7ACA-FFC3-49FC-9710-969CA0C0F2AC";
			var result = Excute(companyPK, 202301, 202312, StartGLAccountPK, EndGLAccountPK);
			AssertEquals("Has 7 rows when lastProcessDate is earlier than firstStartDate in the year", 7, result.Rows.Count);
			AssertEquals(2, result.Select("AccountNumber ='1223' and PeriodDebit = 200 and YTDDebit in (200, 400) and Period in (202303, 202304)").Length);
			AssertEquals(2, result.Select("AccountNumber ='3344' and OpeningBalance in (40,0) and YTDDebit in (200, 400) and CurrentBalance in (240, 440) and Period in (202303, 202304)").Length);
			AssertEquals(1, result.Select("AccountNumber ='4344' and OpeningBalance =80 and PeriodDebit =0 and YTDDebit =0 and CurrentBalance =80 and Period = 202301").Length);
			AssertEquals(1, result.Select("AccountNumber ='4344' and OpeningBalance =0 and PeriodDebit =200 and YTDDebit =200 and CurrentBalance =280 and Period = 202303").Length);
			AssertEquals(1, result.Select("AccountNumber ='4344' and OpeningBalance =0 and PeriodDebit =200 and YTDDebit =400 and CurrentBalance =480 and Period = 202304").Length);

			result = Excute(companyPK, 202201, 202212, StartGLAccountPK, EndGLAccountPK);
			AssertEquals("No data when firstStartDate in the year is earlier than lastProcessedDate", 0, result.Rows.Count);

			result = Excute(companyPK, 202301, 202312, StartGLAccountPK, MidGLAccountPK);
			AssertEquals("Has 4 rows because 4344 is not here", 4, result.Rows.Count);
			AssertEquals(2, result.Select("AccountNumber ='1223' and PeriodDebit = 200 and YTDDebit in (200, 400) and Period in (202303, 202304)").Length);
			AssertEquals(2, result.Select("AccountNumber ='3344' and OpeningBalance in (40,0) and YTDDebit in (200, 400) and CurrentBalance in (240, 440) and Period in (202303, 202304)").Length);

			result = Excute(companyPK, 202301, 202312, Guid.Empty, Guid.Empty);
			AssertEquals("Return all rows when filter GLAccount is null", 7, result.Rows.Count);
		}

		void PrepareData()
		{
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertStmData(new DateTime(2023, 01, 01));
			Helper.InsertPeriodForInputYear(2023);
			Helper.InsertPeriodForInputYear(2022);
			Helper.InsertGLAccount(1101, "123.09.08");
			Helper.InsertGLAccount(1102, "223.09.08", "BSH");
			Helper.InsertGLAccount(1103, "323.09.08", "BSH");
			StartGLAccountPK = Helper.InsertAccountDescriptor(1101, language: "ZH-CN", localAccountNumber: "122334", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptorPivot(1101, 1101);
			MidGLAccountPK = Helper.InsertAccountDescriptor(1102, "DR", 2, "ZH-CN", "3344556", "uuu", "COA", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptorPivot(1102, 1102);
			EndGLAccountPK = Helper.InsertAccountDescriptor(1103, "DR", 2, "ZH-CN", "4344556", "uur", "COA", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptorPivot(1103, 1103);
			Helper.InsertBASAccount(1103);
			Helper.InsertLocalNumberFormatData();

			Helper.InsertGLAggregate(200, 202304, 1101, "", 200);
			Helper.InsertGLAggregate(200, 202303, 1101, "", 200);
			Helper.InsertGLAggregate(200, 202302, 1101, "V", 200);
			Helper.InsertGLAggregate(200, 202203, 1101, "", 200);
			Helper.InsertGLAggregate(200, 202202, 1101, "V", 200);
			Helper.InsertGLAggregate(200, 202304, 1102, "", 200);
			Helper.InsertGLAggregate(200, 202303, 1102, "", 200);
			Helper.InsertGLAggregate(200, 202302, 1102, "V", 200);
			Helper.InsertGLAggregate(200, 202203, 1102, "", 200);
			Helper.InsertGLAggregate(200, 202202, 1102, "V", 200);
			Helper.InsertGLAggregate(200, 202303, 1103, "", 200);
			Helper.InsertGLAggregate(200, 202302, 1103, "V", 200);
			Helper.InsertGLAggregate(200, 202203, 1103, "", 200);
			Helper.InsertGLAggregate(200, 202202, 1103, "V", 200);
			Helper.InsertGLAggregate(200, 202304, 1103, "", 200);

			Helper.InsertBASAggregate(1101, 202203, 40);
			Helper.InsertBASAggregate(1102, 202203, 40);
			Helper.InsertBASAggregate(1103, 202203, 40);
		}

		DataTable Excute(string companyPK, int startPeriod, int endPeriod, Guid startGLAccountPK, Guid endGLAccountPK, string language = "ZH-CN", string countryCode = "CN")
		{
			StringBuilder sqlBuilder = new StringBuilder();
			sqlBuilder.AppendLine($"EXEC [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}]");
			sqlBuilder.Append($"@CompanyPK = '{companyPK}'");
			sqlBuilder.Append(
				$@",@StartPeriod = ").Append(startPeriod == 0 ? "NULL" : startPeriod.ToString());
			sqlBuilder.Append(
				$@",@EndPeriod = ").Append(endPeriod == 0 ? "NULL" : endPeriod.ToString());
			sqlBuilder.Append(
				$@",@StartGLAccountPK = ").Append(startGLAccountPK == Guid.Empty ? "NULL" : $"'{startGLAccountPK}'");
			sqlBuilder.Append(
				$@",@EndGLAccountPK = ").Append(endGLAccountPK == Guid.Empty ? "NULL" : $"'{endGLAccountPK}'");
			sqlBuilder.Append(
				$@",@Language = ").Append($"'{language}'");
			sqlBuilder.Append(
				$@",@CountryCode = ").Append(countryCode == null ? "NULL" : $"'{countryCode}'");

			var sqlText = string.Format(CultureInfo.InvariantCulture,
				sqlBuilder.ToString()
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		Guid StartGLAccountPK, EndGLAccountPK, MidGLAccountPK;

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}
