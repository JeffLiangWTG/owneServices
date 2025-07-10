using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__AlternateGLAccountAttributeInfo))]
	internal class vw_CUS__AlternateGLAccountAttributeInfoTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			var columns = new[] {
				"AlternateChartKey",
				"GLAccountKey",
				"Sequence",
				"AlternateGlAccountKey",
				"Attribute_ORG",
				"Attribute_SPR",
				"Attribute_OCG",
				"Attribute_TIC",
				"Attribute_LFE",
				"Attribute_LFO",
				"CashFlowType",
				"Units",
				"ParentGLAccountNo",
				"ParentGLAccountName",
				"OrganizationKey"
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
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT * FROM [{0}].[{1}].[{2}]",
				ScriptDbName,
				ScriptToTest.SchemaName,
				ScriptToTest.Name
			);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}
	}
}
