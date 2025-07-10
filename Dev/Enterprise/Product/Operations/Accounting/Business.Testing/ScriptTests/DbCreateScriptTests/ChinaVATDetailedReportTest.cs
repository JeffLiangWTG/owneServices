using System;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Build.Database.Script.TestFramework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.DbCreateScriptTests
{
	class ChinaVATDetailedReportTest : ScriptTest
	{
		public void TestChinaVATDetailedReport()
		{
			PrepareData();
			var result = Execute(TestDbHelper.DefaultCompanyPK, "ZZB");
			AssertEquals(1, result.Rows.Count);
			AssertEquals(1,
						 result.Select(
						 "VTT=4 and T18=2 and V04=3 and V18 =1 and V04_YED =3 and V18_YED = 1 and Period = 201601")
						 .Length);
		}

		void PrepareData()
		{
			var dbHelper = new TestDbHelper(TestConnection);
			dbHelper.SetRegistryGLDLastProcessedDate(TestDbHelper.DefaultCompanyPK, new DateTime(2018, 01, 01));
			var glAccount1 = TestObjectCreator.GLHeader1.PK.ToGuid();
			var glAccount2 = TestObjectCreator.GLHeader2.PK.ToGuid();
			var desc1 = TestObjectCreator.CreateAccountDescriptor(TestObjectCreator.GLHeader1, "230198", "VAT", "V18", "ZH-CN", "",
				"CN", "CR");
			var desc2 = TestObjectCreator.CreateAccountDescriptor(TestObjectCreator.GLHeader2, "2830198", "VAT", "V04", "ZH-CN", "",
				"CN", "CR");
			TestObjectCreator.CreateGLDescriptorPivotLight(desc1, TestObjectCreator.GLHeader1);
			TestObjectCreator.CreateGLDescriptorPivotLight(desc2, TestObjectCreator.GLHeader2);

			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company)
								values(NEWID(), 201512, 2015, '12/01/2015', '12/30/2015 23:59', 1, 1, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company)
								values(NEWID(), 201601, 2016, '01/01/2016', '01/31/2016 23:59', 1, 1, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company)
								values(NEWID(), 201602, 2016, '02/01/2016', '02/29/2016 23:59', 1, 1, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

			dbHelper.InsertGLAggregate(1, "", 201601, glAccount1, dbHelper.DefaultBranchPK,
				dbHelper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			dbHelper.InsertGLAggregate(2, "", 201512, glAccount1, dbHelper.DefaultBranchPK,
				dbHelper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			dbHelper.InsertGLAggregate(3, "", 201601, glAccount2, dbHelper.DefaultBranchPK,
				dbHelper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			dbHelper.InsertGLAggregate(4, "", 201512, glAccount2, dbHelper.DefaultBranchPK,
				dbHelper.DefaultDepartmentPK, TestDbHelper.DefaultCompanyPK);
			Factory.Save();
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

		protected string ScriptDbName
		{
			get { return Db.DatabaseName; }
		}
	}
}

