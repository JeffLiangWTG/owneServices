using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__ProfitDeclarationLookup))]
	internal class vw_CUS__ProfitDeclarationLookupTest : BiCreateScriptTest
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
			AssertEquals("Rowcount", 1, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, new Guid("24B74BE6-687E-4788-8DC4-90BFBFEF67B9"), new Guid("9139D092-3FB6-4A30-8EF2-7D4B353E3824"), "SPAARO_WW", new Guid("24B74BE6-687E-4788-8DC4-90BFBFEF67B9"), "SPAIN AR ORG", new Guid("A16B161E-99F1-4439-B0E1-92DC1245D6FA"), "BAR", "EAS", new Guid("8C3B5BA4-4012-406A-9810-7BD6A746E6B6"), "FIA", "E", new Guid("ABE1D8D8-A709-4BFA-88E3-53997AA925E2"), "SMV", new Guid("908011D6-B1B9-40FE-AD80-AC4796BC7B5B"), 1, 1, 1, 1, 1, 2, 1);
			});
		}

		void AssertRowValues(DataTable resultTable, Guid? organizationAddressOrganizationID, Guid? organizationAddressID, string organizationCode, Guid? organizationID, string organizationFullName, Guid? branchID, string branchCode, string accountingGroupCode, Guid? departmentID, string departmentCode, string operatorCode, Guid? operatorStaffID, string salesCode, Guid? salesStaffID, int? branchKey, int? departmentKey, int? jobHeaderKey, int? organizationAddressKey, int? organizationKey, int? operatorStaffKey, int? salesStaffKey)
		{
			var selectqry = string.Format("OrganizationAddressOrganizationID {0} AND OrganizationAddressID {1} AND OrganizationCode {2} AND OrganizationID {3} AND OrganizationFullName {4} AND BranchID {5} AND BranchCode {6} AND AccountingGroupCode {7} AND DepartmentID {8} AND DepartmentCode {9} AND OperatorCode {10} AND OperatorStaffID {11} AND SalesCode {12} AND SalesStaffID {13} AND BranchKey {14} AND DepartmentKey {15} AND JobHeaderKey {16} AND OrganizationAddressKey {17} AND OrganizationKey {18} AND OperatorStaffKey {19} AND SalesStaffKey {20}",
				organizationAddressOrganizationID == null ? "IS NULL" : "= '" + organizationAddressOrganizationID + "'",
				organizationAddressID == null ? "IS NULL" : "= '" + organizationAddressID + "'",
				organizationCode == null ? "IS NULL" : "= '" + organizationCode + "'",
				organizationID == null ? "IS NULL" : "= '" + organizationID + "'",
				organizationFullName == null ? "IS NULL" : "= '" + organizationFullName + "'",
				branchID == null ? "IS NULL" : "= '" + branchID + "'",
				branchCode == null ? "IS NULL" : "= '" + branchCode + "'",
				accountingGroupCode == null ? "IS NULL" : "= '" + accountingGroupCode + "'",
				departmentID == null ? "IS NULL" : "= '" + departmentID + "'",
				departmentCode == null ? "IS NULL" : "= '" + departmentCode + "'",
				operatorCode == null ? "IS NULL" : "= '" + operatorCode + "'",
				operatorStaffID == null ? "IS NULL" : "= '" + operatorStaffID + "'",
				salesCode == null ? "IS NULL" : "= '" + salesCode + "'",
				salesStaffID == null ? "IS NULL" : "= '" + salesStaffID + "'",
				branchKey == null ? "IS NULL" : "= " + branchKey,
				departmentKey == null ? "IS NULL" : "= " + departmentKey,
				jobHeaderKey == null ? "IS NULL" : "= " + jobHeaderKey,
				organizationAddressKey == null ? "IS NULL" : "= " + organizationAddressKey,
				organizationKey == null ? "IS NULL" : "= " + organizationKey,
				operatorStaffKey == null ? "IS NULL" : "= " + operatorStaffKey,
				salesStaffKey == null ? "IS NULL" : "= " + salesStaffKey
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals("Rowcount should be 1", 1, rows.Length);
		}

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>();

			columns.Add("OrganizationAddressOrganizationID");
			columns.Add("OrganizationAddressID");
			columns.Add("OrganizationCode");
			columns.Add("OrganizationID");
			columns.Add("OrganizationFullName");
			columns.Add("BranchID");
			columns.Add("BranchCode");
			columns.Add("AccountingGroupCode");
			columns.Add("DepartmentID");
			columns.Add("DepartmentCode");
			columns.Add("OperatorCode");
			columns.Add("OperatorStaffID");
			columns.Add("SalesCode");
			columns.Add("SalesStaffID");
			columns.Add("BranchKey");
			columns.Add("DepartmentKey");
			columns.Add("JobHeaderKey");
			columns.Add("OrganizationAddressKey");
			columns.Add("OrganizationKey");
			columns.Add("OperatorStaffKey");
			columns.Add("SalesStaffKey");

			return columns;
		}

		void PrepareTestData()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[BAS__JobHeader]
					([JobHeaderKey], [JobHeaderID], [LocalAgentAddressID], [BranchID], [DepartmentID], [RepresentativeOperator], [RepresentativeSales])
					VALUES
						(1,  'A37A03AF-D354-4E84-8E27-57815AE15282',	'9139D092-3FB6-4A30-8EF2-7D4B353E3824',	'A16B161E-99F1-4439-B0E1-92DC1245D6FA', '8C3B5BA4-4012-406A-9810-7BD6A746E6B6','E', 'SMV');

				INSERT [{0}].[Organization].[BAS__OrganizationAddress]
					([OrganizationAddressKey], [OrganizationAddressID], [OrganizationID])
					VALUES
						(1, '9139D092-3FB6-4A30-8EF2-7D4B353E3824', '24B74BE6-687E-4788-8DC4-90BFBFEF67B9');

				INSERT [{0}].[Organization].[BAS__Organization]
					([OrganizationID], [OrganizationKey], [Code], [FullName])
					VALUES
						('24B74BE6-687E-4788-8DC4-90BFBFEF67B9', 1, 'SPAARO_WW', 'SPAIN AR ORG');

				INSERT [{0}].[Organization].[BAS__Branch]
					([BranchKey], [BranchID], [BranchCode], [AccountingGroupCode])
					VALUES
						(1, 'A16B161E-99F1-4439-B0E1-92DC1245D6FA', 'BAR', 'EAS');

				INSERT INTO [{0}].[Organization].[BAS__Department]
					([DepartmentKey], [DepartmentID], [Code])
					VALUES
						(1, '8C3B5BA4-4012-406A-9810-7BD6A746E6B6', 'FIA' );

				INSERT INTO [{0}].[Organization].[BAS__Staff]
					([StaffKey], [StaffID], [Code])
					VALUES
						(1, '908011D6-B1B9-40FE-AD80-AC4796BC7B5B', 'SMV' ),
						(2, 'ABE1D8D8-A709-4BFA-88E3-53997AA925E2', 'E' );",

					ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable SelectRows()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[Finance].[vw_CUS__ProfitDeclarationLookup]",
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
