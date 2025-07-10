using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Customs.Testing
{
	[TestedType(typeof(MDL__EntryNumber))]
	internal class MDL__EntryNumberTest : BiCreateScriptTest
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
			AssertEquals("Rowcount", 1, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, "OTH", "91120393", 42492, "ORD", 123, "DtbBooking");
			});
		}
		void AssertRowValues(DataTable resultTable, string category, string entryNum, int? entryNumberKey, string entryType, int? parentConsolKey, string parentTable)
		{
			var selectqry = string.Format("[Category] {0} AND [Entry Num] {1} AND [Entry Number Key] {2} AND [Entry Type] {3} AND [Parent Consol Key] {4} AND [Parent Table] {5}",
				category == null ? "IS NULL" : "= '" + category + "'",
				entryNum == null ? "IS NULL" : "= '" + entryNum + "'",
				entryNumberKey == null ? "IS NULL" : "= " + entryNumberKey,
				entryType == null ? "IS NULL" : "= '" + entryType + "'",
				parentConsolKey == null ? "IS NULL" : "= " + parentConsolKey,
				parentTable == null ? "IS NULL" : "= '" + parentTable + "'"
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals("Rowcount should be 1", 1, rows.Length);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		DataTable SelectRows()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[Customs].[MDL__EntryNumber]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Customs].[BAS__EntryNumber]
					([EntryNumberID], [Category], [EntryNum], [EntryNumberKey], [EntryType], [ParentConsolKey], [ParentTable])
					VALUES
						(newid(), 'OTH', '91120393', 42492, 'ORD', 123, 'DtbBooking');",
					ScriptDbName
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

