using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(usp_IniLoad_AGG__JobConsolidationExtended))]
	internal class usp_IniLoad_AGG__JobConsolidationExtendedTest : BiCreateScriptTest
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
				AssertRowValues(resultTable, "Test 1:", 2, "ORG2", "Test Org 2", 2, new Guid("E34C3B3A-8634-4B08-BEC2-658AB0B7F567"), 1, 5, 4, "ORG4", "Test Org 4", 4, 3, "ORG3", "Test Org 3", 5, new Guid("32EC689D-09BE-40C1-A4B9-C6A37E2ECB6C"), new Guid("415AF163-DD9F-4C05-9E28-002EFC35E760"), new Guid("4F9B9C96-BEC4-4F4C-96A3-0043A3AEAF29"), new Guid("CB7B4B03-5072-4D4D-B045-67E248D6C773"), "ORG5");
			});
		}

		void AssertRowValues(DataTable resultTable, string testName, int? carrierAddressKey, string carrierCode, string carrierFullName, int? carrierOrganizationKey, Guid? consolidationID, int? consolidationKey, int? creditorAddressKey, int? receivingAgentAddressKey, string receivingAgentCode, string receivingAgentFullName, int? receivingAgentOrganizationKey,
			int? sendingAgentAddressKey, string sendingAgentCode, string sendingAgentFullName, int? creditorOrganizationKey, Guid? sendingAgentID, Guid? receivingAgentID, Guid? carrierID, Guid? creditorID, string creditorCode)
		{
			var selectqry = string.Format("CarrierAddressKey {0} AND CarrierCode {1} AND CarrierFullName {2} AND CarrierOrganizationKey {3} AND ConsolidationID {4} AND ConsolidationKey {5} AND CreditorAddressKey {6} AND ReceivingAgentAddressKey {7} AND ReceivingAgentCode {8} AND ReceivingAgentFullName {9} AND ReceivingAgentOrganizationKey {10} AND SendingAgentAddressKey {11} AND SendingAgentCode {12} AND SendingAgentFullName {13} AND CreditorOrganizationKey {14} AND SendingAgentID {15} AND ReceivingAgentID {16} AND CarrierID {17} AND CreditorID {18} AND CreditorCode {19}",
				carrierAddressKey == null ? "IS NULL" : "= " + carrierAddressKey,
				carrierCode == null ? "IS NULL" : "= '" + carrierCode + "'",
				carrierFullName == null ? "IS NULL" : "='" + carrierFullName + "'",
				carrierOrganizationKey == null ? "IS NULL" : "= " + carrierOrganizationKey,
				consolidationID == null ? "IS NULL" : "= '" + consolidationID + "'",
				consolidationKey == null ? "IS NULL" : "= " + consolidationKey,
				creditorAddressKey == null ? "IS NULL" : "= " + creditorAddressKey,
				receivingAgentAddressKey == null ? "IS NULL" : "= " + receivingAgentAddressKey,
				receivingAgentCode == null ? "IS NULL" : "= '" + receivingAgentCode + "'",
				receivingAgentFullName == null ? "IS NULL" : "='" + receivingAgentFullName + "'",
				receivingAgentOrganizationKey == null ? "IS NULL" : "= " + receivingAgentOrganizationKey,
				sendingAgentAddressKey == null ? "IS NULL" : "= " + sendingAgentAddressKey,
				sendingAgentCode == null ? "IS NULL" : "= '" + sendingAgentCode + "'",
				sendingAgentFullName == null ? "IS NULL" : "='" + sendingAgentFullName + "'",
				creditorOrganizationKey == null ? "IS NULL" : "= " + creditorOrganizationKey,
				sendingAgentID == null ? "IS NULL" : "= '" + sendingAgentID + "'",
				receivingAgentID == null ? "IS NULL" : "= '" + receivingAgentID + "'",
				carrierID == null ? "IS NULL" : "= '" + carrierID + "'",
				creditorID == null ? "IS NULL" : "= '" + creditorID + "'",
				creditorCode == null ? "IS NULL" : "= '" + creditorCode + "'"
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
					"SELECT * FROM [{0}].[InternationalLogistics].[AGG__JobConsolidationExtended]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[InternationalLogistics].[BAS__Consolidation]
				([ConsolidationKey], [ConsolidationID], [CarrierAddressKey], [SendingAgentAddressKey], [ReceivingAgentAddressKey], [CreditorAddressKey])
				VALUES
					(1, 'E34C3B3A-8634-4B08-BEC2-658AB0B7F567', 2, 3, 4, 5);

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
