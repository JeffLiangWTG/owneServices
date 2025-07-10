using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_CUS__JobConsolidationExtended))]
	internal class usp_IniLoad_CUS__JobConsolidationExtendedTest : BiCreateScriptTest
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
			AssertEquals("Rowcount", 2, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, 1, "Org1", "CTLHKG0031", "Carrier Orgnization 1", new Guid("F74FE2B1-FAC4-4709-9603-DD1982C07DD4"), 1, "CHALINHKG", "Charter Link Logistics Limited", 4, new Guid("1E4CAABD-872D-49DC-8B91-5CD848D143FC"), 1, 4, "CHALINHKG", new Guid("BF30453C-6FC6-4547-B63F-922FB7B20950"), 4, 3, "Org3", "ReceivingAgent Orgnization 3", new Guid("BE86C7E9-C463-4D2B-8A71-2E0915C80122"), 3, 2, "Org2", "SendingAgent Orgnization 2", new Guid("483731E2-D6DC-4E87-85B0-5AE1776788B6"), 2);
				AssertRowValues(resultTable, 2, 2, "Org2", "OOCL Logistics", "SendingAgent Orgnization 2", new Guid("483731E2-D6DC-4E87-85B0-5AE1776788B6"), 2, "Org3", "ReceivingAgent Orgnization 3", 3, new Guid("5F1DDD4F-5C9F-4ADD-A520-122A08CCD73A"), 2, 3, "Org3", new Guid("BE86C7E9-C463-4D2B-8A71-2E0915C80122"), 3, 2, "Org2", "SendingAgent Orgnization 2", new Guid("483731E2-D6DC-4E87-85B0-5AE1776788B6"), 2, 3, "Org3", "ReceivingAgent Orgnization 3", new Guid("BE86C7E9-C463-4D2B-8A71-2E0915C80122"), 3);
			});
		}

		void AssertRowValues(DataTable resultTable, int? jobConsolidationExtendedKey, int? carrierAddressKey, string carrierCode, string carrierContractNumber, string carrierFullName, Guid? carrierID, int? carrierOrganizationKey, string coLoadCode, string coLoadFullName, int? coLoadOrganizationKey, Guid? consolidationID, int? consolidationKey, int? creditorAddressKey, string creditorCode, Guid? creditorID, int? creditorOrganizationKey, int? receivingAgentAddressKey, string receivingAgentCode, string receivingAgentFullName, Guid? receivingAgentID, int? receivingAgentOrganizationKey, int? sendingAgentAddressKey, string sendingAgentCode, string sendingAgentFullName, Guid? sendingAgentID, int? sendingAgentOrganizationKey)
		{
			var selectqry = string.Format("JobConsolidationExtendedKey {0} AND CarrierAddressKey {1} AND CarrierCode {2} AND CarrierContractNumber {3} AND CarrierFullName {4} AND CarrierID {5} AND CarrierOrganizationKey {6} AND CoLoadCode {7} AND CoLoadFullName {8} AND CoLoadOrganizationKey {9} AND ConsolidationID {10} AND ConsolidationKey {11} AND CreditorAddressKey {12} AND CreditorCode {13} AND CreditorID {14} AND CreditorOrganizationKey{15} AND ReceivingAgentAddressKey {16} AND ReceivingAgentCode {17} AND ReceivingAgentFullName {18} AND ReceivingAgentID {19} AND ReceivingAgentOrganizationKey {20} AND SendingAgentAddressKey {21} AND SendingAgentCode {22} AND SendingAgentFullName {23} AND SendingAgentID {24} AND SendingAgentOrganizationKey {25}",
				jobConsolidationExtendedKey == null ? "IS NULL" : "= " + jobConsolidationExtendedKey,
				carrierAddressKey == null ? "IS NULL" : "= " + carrierAddressKey,
				carrierCode == null ? "IS NULL" : "= '" + carrierCode + "'",
				carrierContractNumber == null ? "IS NULL" : "= '" + carrierContractNumber + "'",
				carrierFullName == null ? "IS NULL" : "= '" + carrierFullName + "'",
				carrierID == null ? "IS NULL" : "= '" + carrierID + "'",
				carrierOrganizationKey == null ? "IS NULL" : "= " + carrierOrganizationKey,
				coLoadCode == null ? "IS NULL" : "= '" + coLoadCode + "'",
				coLoadFullName == null ? "IS NULL" : "= '" + coLoadFullName + "'",
				coLoadOrganizationKey == null ? "IS NULL" : "= " + coLoadOrganizationKey,
				consolidationID == null ? "IS NULL" : "= '" + consolidationID + "'",
				consolidationKey == null ? "IS NULL" : "= " + consolidationKey,
				creditorAddressKey == null ? "IS NULL" : "= " + creditorAddressKey,
				creditorCode == null ? "IS NULL" : "= '" + creditorCode + "'",
				creditorID == null ? "IS NULL" : "= '" + creditorID + "'",
				creditorOrganizationKey == null ? "IS NULL" : "= " + creditorOrganizationKey,
				receivingAgentAddressKey == null ? "IS NULL" : "= " + receivingAgentAddressKey,
				receivingAgentCode == null ? "IS NULL" : "= '" + receivingAgentCode + "'",
				receivingAgentFullName == null ? "IS NULL" : "= '" + receivingAgentFullName + "'",
				receivingAgentID == null ? "IS NULL" : "= '" + receivingAgentID + "'",
				receivingAgentOrganizationKey == null ? "IS NULL" : "= " + receivingAgentOrganizationKey,
				sendingAgentAddressKey == null ? "IS NULL" : "= " + sendingAgentAddressKey,
				sendingAgentCode == null ? "IS NULL" : "= '" + sendingAgentCode + "'",
				sendingAgentFullName == null ? "IS NULL" : "= '" + sendingAgentFullName + "'",
				sendingAgentID == null ? "IS NULL" : "= '" + sendingAgentID + "'",
				sendingAgentOrganizationKey == null ? "IS NULL" : "= " + sendingAgentOrganizationKey
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals("Rowcount should be 1", 1, rows.Length);
		}

		void PrepareTestData()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[InternationalLogistics].[BAS__Consolidation](ConsolidationID, ConsolidationKey, CarrierContractNumber, CarrierAddressKey, SendingAgentAddressKey, ReceivingAgentAddressKey, CreditorAddressKey, ConsolidationType)
														VALUES ('1E4CAABD-872D-49DC-8B91-5CD848D143FC', 1, 'CTLHKG0031', 1, 2, 3, 4, 'CLD'),
															   ('5F1DDD4F-5C9F-4ADD-A520-122A08CCD73A', 2, 'OOCL Logistics', 2, 3, 2, 3, 'CLD');

				INSERT [{0}].[Organization].[BAS__OrganizationAddress]([OrganizationAddressKey], [OrganizationAddressID], [OrganizationKey])
														VALUES (1, newid(), 1),
															   (2, newid(), 2),
															   (3, newid(), 3),
															   (4, newid(), 4);

				INSERT [{0}].[Organization].[BAS__Organization]([OrganizationKey], [OrganizationID], [Code], [FullName])
														VALUES	(1, 'F74FE2B1-FAC4-4709-9603-DD1982C07DD4', 'Org1', 'Carrier Orgnization 1'),
																(2, '483731E2-D6DC-4E87-85B0-5AE1776788B6', 'Org2', 'SendingAgent Orgnization 2'),
																(3, 'BE86C7E9-C463-4D2B-8A71-2E0915C80122', 'Org3', 'ReceivingAgent Orgnization 3'),
																(4, 'BF30453C-6FC6-4547-B63F-922FB7B20950', 'CHALINHKG', 'Charter Link Logistics Limited');",

					ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable SelectRows()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[InternationalLogistics].[CUS__JobConsolidationExtended]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
				string GetIniLoadSQLText()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT [InitialLoadQuery] FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'InternationalLogistics' AND [ModelTableName] = 'CUS__JobConsolidationExtended'",
					ScriptDbName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var iniLoadSQLText = record.ItemArray[0].ToString();
			return iniLoadSQLText;
		}

		void Execute()
		{
			var iniLoadSQLText = GetIniLoadSQLText();
			var sqlText1 = "USE " + ScriptDbName + " " + iniLoadSQLText;
			var sqlText2 = "USE " + Db.DatabaseName;

			TestConnection.ExecuteNonQuery(sqlText1);
			TestConnection.ExecuteNonQuery(sqlText2);
		}
	}
}
