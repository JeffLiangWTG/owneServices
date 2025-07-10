using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__AlternateGLAccountDissection))]
	internal class vw_CUS__AlternateGLAccountDissectionTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			var columns = new[] {
				"GLAccountKey",
				"AlternateChartKey",
				"Attribute_LFE",
				"Attribute_LFO",
				"Attribute_OCG",
				"Attribute_ORG",
				"Attribute_SPR",
				"Attribute_TIC"
			};

			var result = SelectRows();
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

		DataTable SelectRows()
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[Finance].vw_CUS__AlternateGLAccountDissection");
		}
	}
}
