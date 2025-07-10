using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__GLTransactionLineExtended))]
	internal class usp_IncLoad_CUS__GLTransactionLineExtendedTest : BiCreateScriptTest
	{
		/// <summary>
		/// Basic auto-generated test.
		/// Please replace it with more elaborated tests.
		/// </summary>

		[ExpectNoExceptions]
		public void TestRun()
		{
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
						(2, newid(), 'G2');

				INSERT INTO [{0}].[biadmin].[TransformedRow] (SchemaName, TableName, KeyValue)
				SELECT 'Finance', 'BAS__AccGLTransactionLine', AccGLTransactionLineKey
				FROM [{0}].[Finance].[BAS__AccGLTransactionLine];
",

					ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable SelectRows()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[Finance].[CUS__GLTransactionLineExtended]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		string GetIncLoadSQLText()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT [IncrementalLoadQuery] FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'CUS__GLTransactionLineExtended'",
					ScriptDbName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var iniLoadSQLText = record.ItemArray[0].ToString();
			return iniLoadSQLText;
		}

		void Execute()
		{
			var incLoadSQLText = GetIncLoadSQLText();
			var sqlText1 = "USE " + ScriptDbName + " " + incLoadSQLText;
			var sqlText2 = "USE " + Db.DatabaseName;

			TestConnection.ExecuteNonQuery(sqlText1);
			TestConnection.ExecuteNonQuery(sqlText2);
		}
	}
}
