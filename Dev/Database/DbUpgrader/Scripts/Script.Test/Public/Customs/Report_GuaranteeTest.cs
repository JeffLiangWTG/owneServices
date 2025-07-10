using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	[TestedType(typeof(Report_Guarantee))]
	class Report_GuaranteeTest : CustomsReportDbCreateScriptTest
	{
		public void TestConstraintIsAsExpected()
		{
			var sqlText = "SELECT COUNT(*) FROM  sys.check_constraints WHERE NAME = 'Constraint_CPL_TransactionType' AND DEFINITION = '([CPL_TransactionType]=''OBA'' OR [CPL_TransactionType]=''TRA'' OR [CPL_TransactionType]=''ADJ'' OR [CPL_TransactionType]=''OBL'' OR [CPL_TransactionType]=''CUS'')'";
			Assert("The case statement in Report_Guarantee should be updated whenever Constraint_CPL_TransactionType is changed.", Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText)) > 0);
		}

		public void TestFilterByIncludeCountryList()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "CountryCode",
				(ReportGuaranteeParameters.IncludeCountryList, "AU,NZ"),
				(ReportGuaranteeParameters.EffectiveDate, DBNull.Value),
				(ReportGuaranteeParameters.TransactionDateFrom, DateTime.Today),
				(ReportGuaranteeParameters.TransactionDateTo, DateTime.Today));
			AssertContainsExactElementsInAnyOrder("Expect 1 rows", new string[] { "AU" }, filteredRows);
		}

		public void TestFilterByEffectiveDate()
		{
			var filteredRows = GetFilteredRows(selectedColumn: "GuaranteeNumber",
				(ReportGuaranteeParameters.IncludeCountryList, "AU,US"),
				(ReportGuaranteeParameters.EffectiveDate, new DateTime(2020, 1, 1)),
				(ReportGuaranteeParameters.TransactionDateFrom, DateTime.Today),
				(ReportGuaranteeParameters.TransactionDateTo, DateTime.Today));
			AssertContainsExactElementsInAnyOrder("Expect 2 rows", new string[] { "NUMBER1", "NUMBER2" }, filteredRows);
		}

		public void TestFilterByTransactionDate()
		{
			var sql = $@"INSERT INTO dbo.CusPermitLineTransaction (CPL_PK, CPL_CPH_PermitHeader, CPL_Reference, CPL_TransactionDate, CPL_TransactionType, CPL_TransactionCategory, CPL_TransactionStatus, CPL_ReferenceNumberLine, CPL_SystemCreateTimeUtc, CPL_SystemCreateUser, CPL_SystemLastEditTimeUtc, CPL_SystemLastEditUser) VALUES
('6A6C9CDF-A2C6-4736-AFD6-4781F4822E35', 'F36121E2-EBFC-4784-9064-CE2545001A58', 'A', '2023-07-01', 'ADJ', 'VAL', 'CON', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('46E210B7-BA01-496E-88BE-BA4A50984C63', 'F36121E2-EBFC-4784-9064-CE2545001A58', 'B', '2023-07-02', 'CUS', 'VAL', 'CON', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('6163FC53-0B03-43DE-A296-9FF21189CD7D', 'F36121E2-EBFC-4784-9064-CE2545001A58', 'C', '2023-07-03', 'OBA', 'VAL', 'DEL', 2, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('4C6B0AD1-9FCE-4D9E-B5CA-209C793EA0D9', 'F36121E2-EBFC-4784-9064-CE2545001A58', 'D', '2023-07-04', 'OBA', 'VAL', 'CON', 3, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('34FEAFE9-78DE-4F45-A6FF-07A217346234', 'F36121E2-EBFC-4784-9064-CE2545001A58', 'E', '2023-07-05', 'OBL', 'VAL', 'CON', 4, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('3E6C7D3F-9058-4A35-A186-2BC63E5296C5', 'F36121E2-EBFC-4784-9064-CE2545001A58', 'F', '2023-07-06', 'TRA', 'VAL', 'CON', 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			Db.Connection.ExecuteNonQuery(sql);

			var filteredRows = GetFilteredRows(selectedColumn: "TransactionTypeDescription",
				(ReportGuaranteeParameters.IncludeCountryList, "AU"),
				(ReportGuaranteeParameters.EffectiveDate, DBNull.Value),
				(ReportGuaranteeParameters.TransactionDateFrom, new DateTime(2023, 7, 1)),
				(ReportGuaranteeParameters.TransactionDateTo, new DateTime(2023, 7, 7)));
			AssertContainsExactElementsInAnyOrder(new string[] { "Manual Adjustment", "Value Supplied by Customs", "Opening Balance", "Opening Balance Adjustment", "Automated Transaction" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "TransactionTypeReference",
				(ReportGuaranteeParameters.IncludeCountryList, "AU"),
				(ReportGuaranteeParameters.EffectiveDate, DBNull.Value),
				(ReportGuaranteeParameters.TransactionDateFrom, new DateTime(2023, 7, 2)),
				(ReportGuaranteeParameters.TransactionDateTo, new DateTime(2023, 7, 6)));
			AssertContainsExactElementsInAnyOrder(new string[] { "B", "D", "E" }, filteredRows);
		}

		protected override void SetUp()
		{
			base.SetUp();
			organisationPK = TestDataCreator.CreateOrganisation("TESTORG", "TEST ORGANISATION");
			var sql = $@"
INSERT INTO dbo.CusPermitHeader (CPH_PK, CPH_OH_PermitHolder, CPH_StartDate, CPH_EndDate, CPH_Number, CPH_QtyValIndicator, CPH_RN_NKCountryCode, CPH_Type, CPH_ApplicationCode, CPH_SystemCreateTimeUtc, CPH_SystemCreateUser, CPH_SystemLastEditTimeUtc, CPH_SystemLastEditUser) VALUES
('F36121E2-EBFC-4784-9064-CE2545001A58', '{organisationPK}', '2020-01-01', '2021-01-01', 'NUMBER1', 'BTH', 'AU', 'IMP', 'GUA', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('7DBD7776-F9D3-40B4-BA84-C859D8B7ABAA', '{organisationPK}', '2020-01-01', '2021-01-01', 'NUMBER2', 'BTH', 'US', 'IMP', 'GUA', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('D8E30748-F6EF-4149-AC6F-22AE1F28C107', '{organisationPK}', '2020-06-01', '2021-01-01', 'NUMBER3', 'BTH', 'US', 'IMP', 'GUA', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
";
			Db.Connection.ExecuteNonQuery(sql);
		}

		Guid organisationPK;

		protected override IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters()
		{
			yield return (SqlDbType.VarChar, ReportGuaranteeParameters.IncludeCountryList);
			yield return (SqlDbType.Date, ReportGuaranteeParameters.EffectiveDate);
			yield return (SqlDbType.SmallDateTime, ReportGuaranteeParameters.TransactionDateFrom);
			yield return (SqlDbType.SmallDateTime, ReportGuaranteeParameters.TransactionDateTo);
		}

		class ReportGuaranteeParameters
		{
			public const string IncludeCountryList = "@IncludeCountryList";
			public const string EffectiveDate = "@EffectiveDate";
			public const string TransactionDateFrom = "@TransactionDateFrom";
			public const string TransactionDateTo = "@TransactionDateTo";
		}
	}
}
