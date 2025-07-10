using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__GLTransactionLineExtended))]
	internal class vw_CUS__GLTransactionLineExtendedTest : BiCreateScriptTest
	{
		/// <summary>
		/// Basic auto-generated test.
		/// Please replace it with more elaborated tests.
		/// </summary>

		[ExpectNoExceptions]
		public void TestRun()
		{
			var columns = GetColumns();
			TestColumnsAreAsExpected(ScriptToTest.Name, columns);

			PrepareTestData();
			Execute();

			var resultTable = SelectRows();
			AssertEquals("Rowcount", 2, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, "OR1", "SYD", "AU1", "CC", "CHR", "G2", "G1", "WIP", 100.00, "Test Company A", "CTT");
				AssertRowValues(resultTable, 2, "OR2", "MEL", "TW1", "CC", "CHR", "G2", "G1", "ACR", 200.00, "Test Company B", "CTT");
			});
		}

		void AssertRowValues(DataTable resultTable, int? glTransactionLineKey, string orgCode, string branchCode, string deptCode, string chargeCode, string chargeGroup, string expGrpCode, string salesGrpCode, string lineType, double? lineAmount, string orgName, string chargeType)
		{
			var selectqry = string.Format("GLTransactionLineKey {0} AND OrganizationCode {1} AND BranchCode {2} AND DeptCode {3} AND ChargeCode {4} AND ChargeGroup {5} AND ExpenseGroupCode {6} AND SalesGroupCode {7} AND LineType {8} AND LineAmount {9} AND OrganizationName {10} AND ChargeType {11}",
				glTransactionLineKey == null ? "IS NULL" : "= " + glTransactionLineKey,
				orgCode == null ? "IS NULL" : "= '" + orgCode + "'",
				branchCode == null ? "IS NULL" : "= '" + branchCode + "'",
				deptCode == null ? "IS NULL" : "= '" + deptCode + "'",
				chargeCode == null ? "IS NULL" : "= '" + chargeCode + "'",
				chargeGroup == null ? "IS NULL" : "= '" + chargeGroup + "'",
				expGrpCode == null ? "IS NULL" : "= '" + expGrpCode + "'",
				salesGrpCode == null ? "IS NULL" : "= '" + salesGrpCode + "'",
				lineType == null ? "IS NULL" : "= '" + lineType + "'",
				lineAmount == null ? "IS NULL" : "= " + lineAmount,
				orgName == null ? "IS NULL" : "= '" + orgName + "'",
				chargeType == null ? "IS NULL" : "= '" + chargeType + "'"
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals("Rowcount should be 1", 1, rows.Length);
		}

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>();

			columns.Add("BranchCode");
			columns.Add("BranchID");
			columns.Add("BranchKey");
			columns.Add("ChargeCode");
			columns.Add("ChargeCodeExpenseGroupKey");
			columns.Add("ChargeCodeID");
			columns.Add("ChargeCodeKey");
			columns.Add("ChargeCodeSalesGroupKey");
			columns.Add("ChargeGroup");
			columns.Add("DepartmentID");
			columns.Add("DepartmentKey");
			columns.Add("DeptCode");
			columns.Add("ExpenseGroupCode");
			columns.Add("ExpenseGroupsKey");
			columns.Add("GLTransactionLineBranchKey");
			columns.Add("GLTransactionLineChargeCodeKey");
			columns.Add("GLTransactionLineDepartmentKey");
			columns.Add("GLTransactionLineID");
			columns.Add("GLTransactionLineJobHeaderKey");
			columns.Add("GLTransactionLineKey");
			columns.Add("GLTransactionLineOrganizationKey");
			columns.Add("LineAmount");
			columns.Add("LineType");
			columns.Add("OrganizationCode");
			columns.Add("OrganizationID");
			columns.Add("OrganizationKey");
			columns.Add("PostDateTime");
			columns.Add("ReverseDateTime");
			columns.Add("SalesGroupCode");
			columns.Add("SalesGroupsKey");
			columns.Add("CompanyID");
			columns.Add("GLTransactionHeaderID");
			columns.Add("GLTransactionHeaderKey");
			columns.Add("Description");
			columns.Add("RevenueRecognitionType");
			columns.Add("LineAmountAdjustment");
			columns.Add("WIPAmount");
			columns.Add("CSTAmount");
			columns.Add("ACRAmount");
			columns.Add("REVAmount");
			columns.Add("SalesGroupID");
			columns.Add("ExpenseGroupID");
			columns.Add("ChargeType");
			columns.Add("OrganizationName");

			return columns;
		}

		void PrepareTestData()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[BAS__AccGLTransactionLine]
					([AccGLTransactionLineKey], [AccGLTransactionLineID], [OrganizationKey], [LineAmount], [LineType], [BranchKey], [DepartmentKey],[ChargeCodeKey])
					VALUES
						(1, newid(), 1, 100.00, 'WIP', 1, 1, 1),
						(2, newid(), 2, 200.00, 'ACR', 2, 2, 1);

				INSERT [{0}].[Organization].[BAS__Organization]
					([OrganizationKey], [OrganizationID], [Code], [FullName])
					VALUES
						(1, newid(), 'OR1', 'Test Company A'),
						(2, newid(), 'OR2', 'Test Company B');

				INSERT [{0}].[Organization].[BAS__Branch]
					([BranchKey], [BranchID], [BranchCode])
					VALUES
						(1, newid(), 'SYD'),
						(2, newid(), 'MEL');

				INSERT [{0}].[Organization].[BAS__Department]
					([DepartmentKey], [DepartmentID], [Code])
					VALUES
						(1, newid(), 'AU1'),
						(2, newid(), 'TW1');

				INSERT [{0}].[Finance].[BAS__ChargeCode]
					([ChargeCodeKey], [ChargeCodeID], [ChargeGroup], [SalesGroupKey], [ExpenseGroupKey], [Code], [ChargeType])
					VALUES
						(1, newid(), 'CHR', 1, 2, 'CC', 'CTT');

				INSERT [{0}].[Finance].[BAS__Groups]
					([GroupsKey], [GroupsID], [Code])
					VALUES
						(1, newid(), 'G1'),
						(2, newid(), 'G2');",

					ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable SelectRows()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[Finance].[vw_CUS__GLTransactionLineExtended]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		void Execute()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT * FROM [{0}].[{1}].[{2}]",
				ScriptDbName,
				ScriptToTest.SchemaName,
				ScriptToTest.Name
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
