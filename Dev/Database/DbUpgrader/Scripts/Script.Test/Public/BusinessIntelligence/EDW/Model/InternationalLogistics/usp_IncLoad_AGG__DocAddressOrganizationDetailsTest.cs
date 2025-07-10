using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(usp_IncLoad_AGG__DocAddressOrganizationDetails))]
	internal class usp_IncLoad_AGG__DocAddressOrganizationDetailsTest : BiCreateScriptTest
	{
		/// <summary>
		/// Basic auto-generated test.
		/// Please replace it with more elaborated tests.
		/// </summary>

		// Remove following test method after implementing the tests for this procedure class
		[ExpectNoExceptions]
		public void TestRun()
		{
			PrepareTestData();
			Execute();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 1, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, "Test 1:", 1, "ORG2", "Test Org 2", 2, 2, new Guid("32EC689D-09BE-40C1-A4B9-C6A37E2ECB6C"));
			});
		}

		void AssertRowValues(DataTable resultTable, string testName, int? docAddressKey, string code, string fullName, int? organizationAddressKey, int? organizationKey, Guid? parentID)
		{
			var selectqry = string.Format("DocAddressKey {0} AND Code {1} AND FullName {2} AND OrganizationAddressKey {3} AND OrganizationKey {4} AND ParentID {5}",
				docAddressKey == null ? "IS NULL" : "= " + docAddressKey,
				code == null ? "IS NULL" : "= '" + code + "'",
				fullName == null ? "IS NULL" : "='" + fullName + "'",
				organizationAddressKey == null ? "IS NULL" : "= " + organizationAddressKey,
				organizationKey == null ? "IS NULL" : "= " + organizationKey,
				parentID == null ? "IS NULL" : "= '" + parentID + "'"
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
					"SELECT * FROM [{0}].[InternationalLogistics].[vw_AGG__DocAddressOrganizationDetails]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[InternationalLogistics].[BAS__DocAddress]
				([DocAddressKey], [DocAddressID],[OrganizationAddressKey], [AddressType], [ParentTableCode],[ParentID], [AddressOverride])
				VALUES
					(1, 'E34C3B3A-8634-4B08-BEC2-658AB0B7F567', 2, 'CRD', 'JS','32EC689D-09BE-40C1-A4B9-C6A37E2ECB6C', 0);

				INSERT [{0}].[Organization].[BAS__OrganizationAddress]
				([OrganizationAddressKey], [OrganizationAddressID], [OrganizationKey])
				VALUES
					(2, '6BEF8C65-9C59-4015-9DF6-651E54027BEA', 2),
					(3, 'A20EB899-737B-422C-8D71-654E2DE9200B', 3),
					(4, 'C3F842EF-3BE5-448C-BED3-0017B232C624', 4),
					(5, 'AA1895C4-612F-42A5-B75B-0024986DE21B', 5);

				INSERT [{0}].[Organization].[BAS__Organization]
				([OrganizationKey], [OrganizationID], [FullName], [Code])
				VALUES
					(2, '4F9B9C96-BEC4-4F4C-96A3-0043A3AEAF29', 'Test Org 2', 'ORG2'),
					(3, '32EC689D-09BE-40C1-A4B9-C6A37E2ECB6C', 'Test Org 3', 'ORG3'),
					(4, '415AF163-DD9F-4C05-9E28-002EFC35E760', 'Test Org 4', 'ORG4'),
					(5, 'CB7B4B03-5072-4D4D-B045-67E248D6C773', 'Test Org 5', 'ORG5');
				", ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void Execute()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"EXEC [{0}].[{1}].[{2}]",
				ScriptDbName,
				ScriptToTest.SchemaName,
				ScriptToTest.Name
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
