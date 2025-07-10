using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.eServices;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.eServices
{
	[TestedType(typeof(EAdaptorSummaryScript))]
	sealed class EAdaptorSummaryScriptTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override sealed IDateTimeRange TestDateTimeRange
		{
			get
			{
				var startDate = new DateTime(2021, 7, 1);
				return new RecurringRange(startDate, startDate.AddDays(1));
			}
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			var resutllist = transactions.Select(t => string.Join(",", t.Reference1, t.Reference2, t.Reference3, t.Reference4)).ToList();
			AssertEquals("Number of Transactions", 9, transactions.Count());

			AssertRow(transactions, 1, "EAD", null, "DEM", "RCV", "NDM", "AAA", "AAA", 0.950195, 0.950195, 0.950195, "DEM", 1);
			AssertRow(transactions, 2, "EAD", "TestClient2", "DEM", "RCV", "NDQ", "AAA", "AAA", 0.950195, 0.950195, 0.950195, "DEM", 1);
			AssertRow(transactions, 3, "EAD", null, "DEM", "RCV", "UDM", "AAA", "AAA", 0.950195, 0.950195, 0.950195, "DEM", 1);
			AssertRow(transactions, 4, "EAD", "TestClient1", "DEM", "RCV", "UDQ", "AAA", "AAA", 0.950195, 0.950195, 0.950195, "DEM", 1);
			AssertRow(transactions, 5, "EAD", null, "DEM", "RCV", "UDQ", "AAA", "BBB", 0.950195, 0.950195, 0.950195, "DEM", 1);
			AssertRow(transactions, 6, "EAD", null, "DEM", "RCV", "UDQ", "BBB", "AAA", 0.950195, 0.950195, 0.950195, "DEM", 1);
			AssertRow(transactions, 7, "EAD", null, "DEM", "RCV", "XMS", "AAA", "AAA", 0.950195, 0.950195, 0.950195, "DEM", 1);
			AssertRow(transactions, 8, "EAD", "TestClient2", "DEM", "TRX", "UDQ", "AAA", "AAA", 0.950195, 0.950195, 0.950195, "DEM", 1);
			AssertRow(transactions, 9, "EAD", null, "SIN", "RCV", "UDQ", "AAA", "AAA", 2.650390, 0.950195, 1.700195, "SIN", 2);
		}

		void AssertRow(IEnumerable<IStlTransaction> transactions, int rowNumber, string expectedTransportType, string expectedEdiClientName, string expectedCompanyCode, string expectedDirection, string expectedAppCode, string expectedType, string expectedSubType, double expectedTotalKB, double expectedMinKB, double expectedMaxKB, string expectedBranch, int expectedCount)
		{
			string assertPrefix = $"[T{rowNumber}] ";
			var transaction = transactions.Single(t => t.Reference1 == $"{expectedTransportType}.{expectedDirection}" && t.Reference2 == expectedEdiClientName && t.Reference3 == $"{expectedAppCode}.{expectedType}.{expectedSubType}" && t.Reference4 == string.Format("{0:0.00}", expectedTotalKB));
			AssertEquals(assertPrefix + "CompanyCode", expectedCompanyCode, transaction.GetCompanyCode());
			AssertEquals(assertPrefix + "TransactionDateUtc", new DateTime(2021, 7, 1), transaction.ServiceOccuredUTC);
			var expectedAdditionalRefs = "{" + $"\"Direction\":\"{expectedDirection}\",\"AppCode\":\"{expectedAppCode}\",\"MsgType\":\"{expectedType}\",\"MsgSubType\":\"{expectedSubType}\",\"MsgQty\":{expectedCount},\"TotalKB\":{string.Format("{0:N6}", expectedTotalKB)},\"MinKB\":{string.Format("{0:N6}", expectedMinKB)},\"MaxKB\":{string.Format("{0:N6}", expectedMaxKB)}" + "}";
			AssertEquals(assertPrefix + "AdditionalRefs", expectedAdditionalRefs, transaction.AdditionalRefs);
			AssertEquals(assertPrefix + "TransactionGuidReference", "0D643401-0000-0000-0000-000000000000", transaction.Reference5);
			AssertEquals(assertPrefix + "BranchCode", expectedBranch, transaction.GetBranchCode());
			AssertEquals(assertPrefix + "ItemCount", expectedCount, transaction.BillableCount);
		}

		protected override void PrepareTestData()
		{
			var oneKbArray = new char[973];
			for (int i = 0; i < oneKbArray.Length; oneKbArray[i] = 'K', ++i)
			{ }
			var oneKbMessage = new string(oneKbArray);

			var twoKbArray = new char[1741];
			for (int i = 0; i < twoKbArray.Length; twoKbArray[i] = 'K', ++i)
			{ }
			var twoKbMessage = new string(twoKbArray);

			var sqlText = $@"
			DECLARE @GcPkDEM UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
			DECLARE @GcPkSIN UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
			DECLARE @GbPkDEM UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkDEM);
			DECLARE @GbPkSIN UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSIN);
			DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP (1) GE_PK FROM dbo.GlbDepartment);
			DECLARE @EcpPk01 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcpPk02 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk01 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk02 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk03 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk04 UNIQUEIDENTIFIER = NEWID();

			INSERT INTO dbo.EDICommunicationParty (ECP_PK, ECP_IsActive, ECP_Name, ECP_Summary, ECP_ApplicationCode, ECP_OC_TechnicalContact, ECP_SystemCreateTimeUtc, ECP_SystemCreateUser, ECP_SystemLastEditTimeUtc, ECP_SystemLastEditUser) VALUES
				(@EcpPk01, 1, 'TestClient1', 'test', 'AAA', null, GETDATE(), 'DAT', GETDATE(), 'DAT'),
				(@EcpPk02, 1, 'TestClient2', 'test', 'AAA', null, GETDATE(), 'DAT', GETDATE(), 'DAT');
			INSERT INTO dbo.EDICommunicationPartyConfig (ECC_PK, ECC_IsActive, ECC_Status, ECC_ECP_Party, ECC_Direction, ECC_SystemCreateTimeUtc, ECC_SystemCreateUser, ECC_SystemLastEditTimeUtc, ECC_SystemLastEditUser) VALUES
				(@EccPk01, 1, 'CNF', @EcpPk01, 'IN', GETDATE(), 'DAT', GETDATE(), 'DAT'),
				(@EccPk02, 1, 'CNF', @EcpPk02, 'IN', GETDATE(), 'DAT', GETDATE(), 'DAT'),
				(@EccPk03, 1, 'CNF', @EcpPk01, 'OUT', GETDATE(), 'DAT', GETDATE(), 'DAT'),
				(@EccPk04, 1, 'CNF', @EcpPk02, 'OUT', GETDATE(), 'DAT', GETDATE(), 'DAT');

			INSERT dbo.EDIMessage (EM_PK, EM_TransportType, EM_ECC_CommunicationPartyConfig, EM_EI, EM_GB, EM_GE, EM_SystemCreateTimeUtc, EM_ReceiveTransmit, EM_ApplicationCode, EM_MessageType, EM_MessageSubType, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser, EM_MessageData, EM_MessageText) VALUES
				(0x1, 'EAD', @EccPk01, NULL, @GbPkDEM, @GePk, '2021-6-30 10:58:30', 'RCV', 'UDQ', 'AAA', 'AAA', 'DAT', '2021-6-30 10:58:30', 'DAT', CONVERT(VARBINARY(max), '{oneKbMessage}'), ''), --Outside range, too old, with EAD Client
				(0x2, 'EAD', @EccPk01, NULL, @GbPkDEM, @GePk, '2021-7-01 10:59:30', 'RCV', 'UDQ', 'AAA','AAA', 'DAT', '2021-7-01 10:59:30', 'DAT', CONVERT(VARBINARY(max), '{oneKbMessage}'), ''), --Valid receive, with EAD Client
				(0x3, 'EAD', @EccPk01, NULL, @GbPkDEM, @GePk, '2021-7-01 10:59:30', 'INT', 'UDQ', 'AAA', 'AAA', 'DAT', '2021-7-01 10:59:30', 'DAT', CONVERT(VARBINARY(max), '{oneKbMessage}'), ''), --Invalid, Internal interchange, with EAD Client
				(0x4, 'EAD', @EccPk03, NULL, @GbPkDEM, @GePk, '2021-7-02 00:01:30', 'TRX', 'UDQ', 'AAA','AAA', 'DAT', '2021-7-02 00:01:30', 'DAT', CONVERT(VARBINARY(max), '{oneKbMessage}'), ''), --Outside range, too new, with EAD Client
				(0x5, 'EAD', @EccPk04, NULL, @GbPkDEM, @GePk, '2021-7-01 10:59:30', 'TRX', 'UDQ', 'AAA', 'AAA', 'DAT', '2021-7-01 10:59:30', 'DAT', CONVERT(VARBINARY(max), '{oneKbMessage}'), ''), --Valid send, with EAD Client
				(0x6, 'HUB', @EccPk02, NULL, @GbPkDEM, @GePk, '2021-7-01 10:59:30', 'RCV', 'UDQ', 'AAA', 'AAA', 'DAT', '2021-7-01 10:59:30', 'DAT', CONVERT(VARBINARY(max), '{oneKbMessage}'), ''), --Invalid, not eAdaptor, with EAD Client
				(0x7, 'EAD', @EccPk02, NULL, @GbPkDEM, @GePk, '2021-7-01 10:59:30', 'RCV', 'NDQ', 'AAA','AAA', 'DAT', '2021-7-01 10:59:30', 'DAT', CONVERT(VARBINARY(max), '{oneKbMessage}'), ''), --Valid receive, different application code, with EAD Client
				(0x8, 'EAD', NULL, NULL, @GbPkDEM, @GePk, '2021-7-01 10:59:30', 'RCV', 'UDM', 'AAA', 'AAA', 'DAT', '2021-7-01 10:59:30', 'DAT', CONVERT(VARBINARY(max), '{oneKbMessage}'), ''), --Valid receive, different application code
				(0x9, 'EAD', NULL, NULL, @GbPkDEM, @GePk, '2021-7-01 10:59:30', 'RCV', 'NDM', 'AAA', 'AAA', 'DAT', '2021-7-01 10:59:30', 'DAT', NULL, '{oneKbMessage}'), --Valid receive, different application code, message data in text
				(0x10, 'EAD', NULL, NULL, @GbPkDEM, @GePk, '2021-7-01 10:59:30', 'RCV', 'UDQ', 'BBB', 'AAA', 'DAT', '2021-7-01 10:59:30', 'DAT', CONVERT(VARBINARY(max), '{oneKbMessage}'), ''), --Valid receive, different message type
				(0x11, 'EAD', NULL, NULL, @GbPkDEM, @GePk, '2021-7-01 10:59:30', 'RCV', 'UDQ', 'AAA', 'BBB', 'DAT', '2021-7-01 10:59:30', 'DAT', CONVERT(VARBINARY(max), '{oneKbMessage}'), ''), --Valid receive, different message sub - type
				(0x12, 'EAD', NULL, NULL, @GbPkSIN, @GePk, '2021-7-01 10:59:30', 'RCV', 'UDQ', 'AAA', 'AAA', 'DAT', '2021-7-01 10:59:30', 'DAT', CONVERT(VARBINARY(max), '{oneKbMessage}'), ''), --Valid receive, different branch\company
				(0x13, 'EAD', NULL, NULL, @GbPkSIN, @GePk, '2021-7-01 10:59:30', 'RCV', 'UDQ', 'AAA', 'AAA', 'DAT', '2021-7-01 10:59:30', 'DAT', CONVERT(VARBINARY(max), '{twoKbMessage}'), ''), --Valid receive, different branch\company, 2KB
				(0x14, 'EAD', NULL, NULL, @GbPkDEM, @GePk, '2021-7-01 10:59:30', 'RCV', 'XMS', 'AAA', 'AAA', 'DAT', '2021-7-01 10:59:30', 'DAT', CONVERT(VARBINARY(max), '{oneKbMessage}'), ''); --Valid receive, different application code";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		public void TestVersions()
		{
			CombineAssertions("Test minimal and maximal CW1 version", () =>
			{
				AssertEquals("25.4.1.231", ScriptToTest.MinCW1Version);
				AssertEquals("", ScriptToTest.MaxCW1Version);
			});
		}
	}
}
