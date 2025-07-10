using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_AGG__JobParentsByControllingCustomerAndAgent2))]
	internal class vw_AGG__JobParentsByControllingCustomerAndAgent2Test : BiCreateScriptTest
	{
		/// <summary>
		/// Basic auto-generated test.
		/// Please replace it with more elaborated tests.
		/// </summary>

		[ExpectNoExceptions]
		public void TestRun()
		{
			HashSet<string> columns = GetColumns();
			TestColumnsAreAsExpected(ScriptToTest.Name, columns);

			PrepareTestData();
			Execute();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 1, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, "Test 1:", new Guid("E34C3B3A-8634-4B08-BEC2-658AB0B7F567"), new Guid("A20EB899-737B-422C-8D71-654E2DE9200B"), new Guid("6BEF8C65-9C59-4015-9DF6-651E54027BEA"), "Test Org 2", "Test Org 1");
			});
		}

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>();

			columns.Add("CAG");
			columns.Add("CAGOrgName");
			columns.Add("CCB");
			columns.Add("CCBOrgName");
			columns.Add("ControllingAgentOrgKey");
			columns.Add("ControllingCustomerAndAgentDeclarationKey");
			columns.Add("ControllingCustomerOrgKey");
			columns.Add("E2_ParentID");

			return columns;
		}

		void AssertRowValues(DataTable resultTable, string testName, Guid? e2_ParentID, Guid? cag, Guid? ccb, string cagOrgName, string ccbOrgName)
		{
			var selectqry = string.Format("E2_ParentID {0} AND CAG {1} AND CCB {2} AND CAGOrgName {3} AND CCBOrgName {4}",
				e2_ParentID == null ? "IS NULL" : "='" + e2_ParentID + "'",
				cag == null ? "IS NULL" : "= '" + cag + "'",
				ccb == null ? "IS NULL" : "='" + ccb + "'",
				cagOrgName == null ? "IS NULL" : "= '" + cagOrgName + "'",
				ccbOrgName == null ? "IS NULL" : "= '" + ccbOrgName + "'"
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals(testName, 1, rows.Length);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		DataTable SelectRows()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[InternationalLogistics].[vw_AGG__JobParentsByControllingCustomerAndAgent2]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Customs].[BAS__ControllingCustomerAndAgentDeclaration]
				([ControllingCustomerAndAgentDeclarationKey], [ControllingCustomerAndAgentDeclarationID], [ControllingAgentOrgID], [ControllingCustomerOrgID])
				VALUES
					(1, 'E34C3B3A-8634-4B08-BEC2-658AB0B7F567', 'A20EB899-737B-422C-8D71-654E2DE9200B', '6BEF8C65-9C59-4015-9DF6-651E54027BEA');

				INSERT [{0}].[Organization].[BAS__Organization]
				([OrganizationKey], [OrganizationID], [FullName])
				VALUES
					(1, '6BEF8C65-9C59-4015-9DF6-651E54027BEA', 'Test Org 1'),
					(2, 'A20EB899-737B-422C-8D71-654E2DE9200B', 'Test Org 2');
				", ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
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
