using System;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(ChinaVATDetailedReport))]
	class ChinaVATDetailedReportTest : BiCreateScriptTest
	{
		public void TestChinaVATDetailedReport()
		{
			PrepareData();
			var result = Execute(TestDbHelper.DefaultCompanyPK, "CBH");
			AssertEquals(1, result.Rows.Count);
			AssertEquals(1,
				result.Select(
						"VTT=4 and T18=2 and V04=3 and V18 =1 and V04_YED =3 and V18_YED = 1 and Period = 201601")
					.Length);
		}

		void PrepareData()
		{
			var helper = new PrepareDataHelper(TestConnection, ScriptDbName);
			helper.InsertCompanyBranchAndDepartment();
			helper.InsertStmData(new DateTime(2018, 01, 01));
			helper.InsertGLAccount(1, "2010.00.00");
			helper.InsertGLAccount(2, "2010.00.10");
			helper.InsertAccountDescriptor(glAccountDescriptorKey: 1, gLAccountKey: 1, localAccountNumber: "230198", reportType: "VAT",
				reportCategory: "V18", language: "ZH-CN");
			helper.InsertAccountDescriptor(glAccountDescriptorKey: 2, gLAccountKey: 2, localAccountNumber: "2830198", reportType: "VAT",
				reportCategory: "V04", language: "ZH-CN");
			helper.InsertAccountDescriptorPivot(1, 1);
			helper.InsertAccountDescriptorPivot(2, 2);
			helper.InsertPeriodManagement(1,2015,12);
			helper.InsertPeriodManagement(2, 2016, 1);
			helper.InsertPeriodManagement(3, 2016, 2);
			helper.InsertBASGLAggregate(1,201601,1);
			helper.InsertBASGLAggregate(2, 201512, 1);
			helper.InsertBASGLAggregate(3, 201601, 2);
			helper.InsertBASGLAggregate(4, 201512, 2);
		}

		DataTable Execute(Guid companyPK, string branchs, int period = 201601)
		{
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendLine($"EXEC [{ScriptDbName}].[dbo].[ChinaVATDetailedReport]");
			sqlBuilder.Append($"@CompanyPK = '{companyPK}'");
			sqlBuilder.Append(
				$@",@Branch = ").Append(branchs == null ? "NULL" : $"'{branchs}'");
			sqlBuilder.Append(
				$@",@Period = ").Append(period == 0 ? "NULL" : $"{period}");

			var sqlText = string.Format(CultureInfo.InvariantCulture,
				sqlBuilder.ToString()
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}
