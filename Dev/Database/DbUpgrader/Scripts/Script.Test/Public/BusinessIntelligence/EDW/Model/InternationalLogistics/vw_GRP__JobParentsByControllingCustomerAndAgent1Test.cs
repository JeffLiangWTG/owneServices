using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_GRP__JobParentsByControllingCustomerAndAgent1))]
	internal class vw_GRP__JobParentsByControllingCustomerAndAgent1Test : BiCreateScriptTest
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
				AssertRowValues(resultTable, new Guid("4F9B9C96-BEC4-4F4C-96A3-0043A3AEAF29"), new Guid("32EC689D-09BE-40C1-A4B9-C6A37E2ECB6C"), "Org 1", null, null);
				AssertRowValues(resultTable, new Guid("13A2CD7D-848D-4481-9F30-004D6291A045"), null, null, new Guid("CB7B4B03-5072-4D4D-B045-67E248D6C773"), "Org 3");
			});
		}

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>
			{
				"E2_ParentID",
				"CAG",
				"CAGOrgName",
				"CCB",
				"CCBOrgName"
			};

			return columns;
		}

		void AssertRowValues(DataTable resultTable, Guid? parentID, Guid? cag, string cagOrgName, Guid? ccb, string ccbOrgName)
		{
			var selectqry = string.Format("E2_ParentID {0} AND CAG {1} AND CAGOrgName {2} AND CCB {3} AND CCBOrgName {4}",
				parentID == null ? "IS NULL" : "= '" + parentID + "'",
				cag == null ? "IS NULL" : "= '" + cag + "'",
				cagOrgName == null ? "IS NULL" : "= '" + cagOrgName + "'",
				ccb == null ? "IS NULL" : "= '" + ccb + "'",
				ccbOrgName == null ? "IS NULL" : "= '" + ccbOrgName + "'"
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
					"SELECT * FROM [{0}].[InternationalLogistics].[vw_GRP__JobParentsByControllingCustomerAndAgent1]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"

				INSERT [{0}].[InternationalLogistics].[BAS__ControllingCustomerAndAgentAddress](ControllingCustomerAndAgentAddressID, ControllingCustomerAndAgentAddressKey, ParentID, AddressType, OrganizationAddressKey)
															VALUES('C3F842EF-3BE5-448C-BED3-0017B232C624', 1, '4F9B9C96-BEC4-4F4C-96A3-0043A3AEAF29', 'CAG', 1),
															('AA1895C4-612F-42A5-B75B-0024986DE21B', 2, '4F9B9C96-BEC4-4F4C-96A3-0043A3AEAF29', 'CAG', 1),
															('415AF163-DD9F-4C05-9E28-002EFC35E760', 3, '13A2CD7D-848D-4481-9F30-004D6291A045', 'SCP', 3)

				INSERT [{0}].[Organization].[BAS__OrganizationAddress](OrganizationAddressID,OrganizationAddressKey, OrganizationKey, Code, Address1)
																		VALUES ('6F1755E3-D182-4844-9399-E8585E8D3BE8', 1, 1, 'ADD1', '25 Ocean St, Alexandria, NSW, Australia'),
																		 ('0DFAE11D-2EBE-4C8C-8DAA-B0EA68C2E126', 2, 2, 'ADD2', '58 Frederick St, Mascot, NSW, Australia'),
																		 ('030D36AF-940C-4166-8631-E07742040DE3', 3, 3, 'ADD3', '55 Oxford St, Newtown, NSW, Australia')

				INSERT [{0}].[Organization].[BAS__Organization](OrganizationID, OrganizationKey, Code, FullName)
															VALUES('32EC689D-09BE-40C1-A4B9-C6A37E2ECB6C', 1, 'O1', 'Org 1'),
															('257123E1-589D-4926-A8FF-5A186317F57F', 2, 'O2', 'Org 2'),
															('CB7B4B03-5072-4D4D-B045-67E248D6C773', 3, 'O3', 'Org 3')",
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
