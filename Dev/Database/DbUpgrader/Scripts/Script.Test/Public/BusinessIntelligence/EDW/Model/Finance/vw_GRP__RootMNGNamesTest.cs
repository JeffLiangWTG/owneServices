using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_GRP__RootMNGNames))]
	internal class vw_GRP__RootMNGNamesTest : BiCreateScriptTest
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
				AssertRowValues(resultTable, new Guid("257123E1-589D-4926-A8FF-5A186317F57F"), new Guid("32EC689D-09BE-40C1-A4B9-C6A37E2ECB6C"), "Org 3", new Guid("CB7B4B03-5072-4D4D-B045-67E248D6C773"), "Org 1");
				AssertRowValues(resultTable, new Guid("32EC689D-09BE-40C1-A4B9-C6A37E2ECB6C"), new Guid("CB7B4B03-5072-4D4D-B045-67E248D6C773"), "Org 3", new Guid("CB7B4B03-5072-4D4D-B045-67E248D6C773"), "Org 3");
			});
		}

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>
			{
				"OrgPK",
				"RelatedParty",
				"RootOrg",
				"MNGName",
				"RelatedMNGName"
			};

			return columns;
		}

		void AssertRowValues(DataTable resultTable, Guid? orgPK, Guid? relatedParty, string mNGName, Guid? rootOrg, string relatedMNGName)
		{
			var selectqry = string.Format("OrgPK {0} AND RelatedParty {1} AND MNGName {2} AND RootOrg {3} AND RelatedMNGName {4}",
				orgPK == null ? "IS NULL" : "= '" + orgPK + "'",
				relatedParty == null ? "IS NULL" : "= '" + relatedParty + "'",
				mNGName == null ? "IS NULL" : "= '" + mNGName + "'",
				rootOrg == null ? "IS NULL" : "= '" + rootOrg + "'",
				relatedMNGName == null ? "IS NULL" : "= '" + relatedMNGName + "'"
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
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[Finance].[vw_GRP__RootMNGNames]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"

				INSERT [{0}].[Organization].[BAS__OrganizationRelatedParty](OrganizationRelatedPartyID, OrganizationRelatedPartyKey, CompanyKey, ParentOrganizationID, PartyType, RelatedParty)
																		VALUES ('6F1755E3-D182-4844-9399-E8585E8D3BE8', 1, null, '32EC689D-09BE-40C1-A4B9-C6A37E2ECB6C', 'MNG', 'CB7B4B03-5072-4D4D-B045-67E248D6C773'),
																		 ('0DFAE11D-2EBE-4C8C-8DAA-B0EA68C2E126', 2, null, '257123E1-589D-4926-A8FF-5A186317F57F', 'MNG', '32EC689D-09BE-40C1-A4B9-C6A37E2ECB6C'),
																		 ('030D36AF-940C-4166-8631-E07742040DE3', 3, 1, 'CB7B4B03-5072-4D4D-B045-67E248D6C773', 'ABC', 'CB7B4B03-5072-4D4D-B045-67E248D6C773')

				INSERT [{0}].[Organization].[BAS__Organization](OrganizationID, OrganizationKey, FullName)
															VALUES('32EC689D-09BE-40C1-A4B9-C6A37E2ECB6C', 1, 'Org 1'),
															('257123E1-589D-4926-A8FF-5A186317F57F', 2, 'Org 2'),
															('CB7B4B03-5072-4D4D-B045-67E248D6C773', 3, 'Org 3')",
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
