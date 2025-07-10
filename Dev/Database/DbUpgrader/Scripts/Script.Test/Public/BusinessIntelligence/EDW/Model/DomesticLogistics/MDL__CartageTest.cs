using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.DomesticLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.DomesticLogistics.Testing
{
	[TestedType(typeof(MDL__Cartage))]
	internal class MDL__CartageTest : BiCreateScriptTest
	{
		/// <summary>
		/// Basic auto-generated test.
		/// Please replace it with more elaborated tests.
		/// </summary>

		[ExpectNoExceptions]
		public void TestRun()
		{
			Execute();
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

