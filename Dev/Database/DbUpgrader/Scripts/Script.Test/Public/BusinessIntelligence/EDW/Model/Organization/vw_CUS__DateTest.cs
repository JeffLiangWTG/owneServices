using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Organization;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Organization.Testing
{
	[TestedType(typeof(vw_CUS__Date))]
	internal class vw_CUS__DateTest : BiCreateScriptTest
	{
		public void TestNumberOfInsertedDaysInCusDateTable()
		{
			ExecuteCusTableIniLoad();
			ExecuteCusTableIncLoad();
			var resultTable = SelectRows();
			var expectedDays = (new DateTime(DateTime.Today.Year + 4, 12, 31) - new DateTime(1994, 1, 1)).TotalDays + 1;
			AssertEquals("Rowcount", expectedDays , Convert.ToDouble(resultTable.Rows.Count));
		}

		void ExecuteCusTableIniLoad()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT InitialLoadQuery FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Organization' AND [ModelTableName] = 'CUS__Date'",
				ScriptDbName
			);
			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			string incLoadSQLText = record.ItemArray[0].ToString();
			string sqlTextDL = "USE " + ScriptDbName + " " + incLoadSQLText;
			TestConnection.ExecuteNonQuery(sqlTextDL);
		}
		void ExecuteCusTableIncLoad()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT IncrementalLoadQuery FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Organization' AND [ModelTableName] = 'CUS__Date'",
				ScriptDbName
			);
			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			string incLoadSQLText = record.ItemArray[0].ToString();
			string sqlTextDL = "USE " + ScriptDbName + " " + incLoadSQLText;
			TestConnection.ExecuteNonQuery(sqlTextDL);
		}
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		DataTable SelectRows()
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

