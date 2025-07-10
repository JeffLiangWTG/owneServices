using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__GLHeaderSubAccounts))]
	internal class vw_CUS__GLHeaderSubAccountsTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			var columns = new[]
			{
				"GLAccountKey",
				"HasSalesExpenseGroupsSubAccount",
				"HasOrganisationSubAccount",
				"HasStaffAndResourcesSubAccount",
				"HasStaffGroupSubAccount"
			};
			var result = Execute();
			foreach (string column in columns)
			{
				Assert($"{column} should be contained", result.Columns.Contains(column));
			}
			AssertEquals(columns.Length, result.Columns.Count);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		DataTable Execute()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT * FROM [{0}].[{1}].[{2}]",
				ScriptDbName,
				ScriptToTest.SchemaName,
				ScriptToTest.Name
			);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}
	}
}
