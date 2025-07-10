using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.eServices;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.eServices
{
	[TestedType(typeof(EAdaptorCWConnectorSummaryScript))]
	sealed class EAdaptorCWConnectorSummaryScriptTest : RefStlScriptWithDefaultsTest
	{
		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(10, transactions.Count());

			AssertRow(transactions, 1, "DEM", "RCV", "UDQ", "AAA", "AAA", 0.950195, 0.950195, 0.950195, "DEM", 1);
			AssertRow(transactions, 2, "DEM", "TRX", "UDQ", "AAA", "AAA", 0.950195, 0.950195, 0.950195, "DEM", 1);
			AssertRow(transactions, 3, "DEM", "RCV", "NDQ", "AAA", "AAA", 0.950195, 0.950195, 0.950195, "DEM", 1);
			AssertRow(transactions, 4, "DEM", "RCV", "UDQ", "BBB", "AAA", 0.950195, 0.950195, 0.950195, "DEM", 1);
			AssertRow(transactions, 5, "DEM", "RCV", "UDQ", "AAA", "BBB", 0.950195, 0.950195, 0.950195, "DEM", 1);
			AssertRow(transactions, 6, "DEM", "RCV", "XMS", "AAA", "AAA", 1.700195, 1.700195, 1.700195, "DEM", 1);
			AssertRow(transactions, 7, "DEM", "RCV", "UDM", "AAA", "AAA", 0.950195, 0.950195, 0.950195, "DEM", 1);
			AssertRow(transactions, 8, "DEM", "RCV", "NDM", "AAA", "AAA", 0.950195, 0.950195, 0.950195, "DEM", 1);
			AssertRow(transactions, 9, "SIN", "RCV", "UDQ", "AAA", "CCC", 2.650390, 0.950195, 1.700195, "SIN", 2);
			AssertRow(transactions, 10, "DEM", "RCV", "UDQ", "OUT", "OUT", 0.950195, 0.950195, 0.950195, "DEM", 1);
		}

		void AssertRow(IEnumerable<IStlTransaction> transactions, int rowNumber, string expectedCompanyCode, string expectedDirection, string expectedAppCode, string expectedType, string expectedSubType, double expectedTotalKB, double expectedMinKB, double expectedMaxKB, string expectedBranch, int expectedCount)
		{
			var assertPrefix = $"[T{rowNumber}] ";
			var transaction = transactions.Single(t => t.Reference1 == expectedDirection && t.Reference2 == expectedAppCode && t.Reference3 == $"{expectedType}.{expectedSubType}" && t.Reference4 == string.Format("{0:0.00}", expectedTotalKB));
			AssertEquals(assertPrefix + "CompanyCode", expectedCompanyCode, transaction.GetCompanyCode());
			AssertEquals(assertPrefix + "TransactionDateUtc", new DateTime(2024, 12, 10), transaction.ServiceOccuredUTC);
			var expectedAdditionalRefs = "{" + $"\"Direction\":\"{expectedDirection}\",\"AppCode\":\"{expectedAppCode}\",\"MsgType\":\"{expectedType}\",\"MsgSubType\":\"{expectedSubType}\",\"MsgQty\":{expectedCount},\"TotalKB\":{string.Format("{0:N6}", expectedTotalKB)},\"MinKB\":{string.Format("{0:N6}", expectedMinKB)},\"MaxKB\":{string.Format("{0:N6}", expectedMaxKB)}" + "}";
			AssertEquals(assertPrefix + "AdditionalRefs", expectedAdditionalRefs, transaction.AdditionalRefs);
			AssertEquals(assertPrefix + "TransactionGuidReference", "3ADB3401-0000-0000-0000-000000000000", transaction.Reference5);
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

			var insertScript = @$"
DECLARE @EcpPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @EcpPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @EcpPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @EcpPk04 UNIQUEIDENTIFIER = NEWID();
DECLARE @EccPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @EccPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @EccPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @EccPk04 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPkDEM UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
DECLARE @GcPkSIN UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
DECLARE @GbPkDEM UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkDEM);
DECLARE @GbPkSIN UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSIN);
DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP (1) GE_PK FROM dbo.GlbDepartment);

INSERT INTO dbo.EDICommunicationParty (ECP_PK, ECP_IsActive, ECP_Name, ECP_Summary, ECP_ApplicationCode, ECP_OC_TechnicalContact, ECP_SystemCreateTimeUtc, ECP_SystemCreateUser, ECP_SystemLastEditTimeUtc, ECP_SystemLastEditUser) VALUES
	(@EcpPk01, 1, 'TestCWConnectorClient', 'test', 'AAA', null, GETDATE(), 'DAT', GETDATE(), 'DAT'),
	(@EcpPk02, 1, 'testParty', 'test', 'AAA', null, GETDATE(), 'DAT', GETDATE(), 'DAT')
INSERT INTO dbo.EDICommunicationPartyConfig (ECC_PK, ECC_IsActive, ECC_Status, ECC_ECP_Party, ECC_Direction, ECC_SystemCreateTimeUtc, ECC_SystemCreateUser, ECC_SystemLastEditTimeUtc, ECC_SystemLastEditUser) VALUES
	(@EccPk01, 1, 'CNF', @EcpPk01, 'IN', GETDATE(), 'DAT', GETDATE(), 'DAT'),
	(@EccPk02, 1, 'CNF', @EcpPk02, 'IN', GETDATE(), 'DAT', GETDATE(), 'DAT'),
	(@EccPk03, 1, 'CNF', @EcpPk01, 'OUT', GETDATE(), 'DAT', GETDATE(), 'DAT'),
	(@EccPk04, 1, 'CNF', @EcpPk02, 'OUT', GETDATE(), 'DAT', GETDATE(), 'DAT')
INSERT dbo.EDIMessage (EM_PK, EM_GB, EM_GE, EM_ECC_CommunicationPartyConfig, EM_ReceiveTransmit, EM_ApplicationCode, EM_MessageType, EM_MessageSubType, EM_MessageText, EM_MessageData, EM_SystemCreateTimeUtc, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES
	(NEWID(), @GbPkDEM, @GePk, @EccPk01, 'RCV', 'UDQ', 'AAA', 'AAA', '', CONVERT(VARBINARY(max),'{oneKbMessage}'), '2024-12-10 12:25:30', 'DAT', '2024-12-10 12:25:30', 'DAT'), -- valid message
	(NEWID(), @GbPkDEM, @GePk, @EccPk01, 'TRX', 'UDQ', 'AAA', 'AAA', '', CONVERT(VARBINARY(max),'{oneKbMessage}'), '2024-12-10 12:25:30', 'DAT', '2024-12-10 12:25:30', 'DAT'), -- valid message different transmit code
	(NEWID(), @GbPkDEM, @GePk, @EccPk01, 'RCV', 'NDQ', 'AAA', 'AAA', '', CONVERT(VARBINARY(max),'{oneKbMessage}'), '2024-12-10 12:25:30', 'DAT', '2024-12-10 12:25:30', 'DAT'), -- valid message different application code
	(NEWID(), @GbPkDEM, @GePk, @EccPk01, 'RCV', 'UDQ', 'BBB', 'AAA', '', CONVERT(VARBINARY(max),'{oneKbMessage}'), '2024-12-10 12:25:30', 'DAT', '2024-12-10 12:25:30', 'DAT'), -- valid message different message type
	(NEWID(), @GbPkDEM, @GePk, @EccPk01, 'RCV', 'UDQ', 'AAA', 'BBB', '', CONVERT(VARBINARY(max),'{oneKbMessage}'), '2024-12-10 12:25:30', 'DAT', '2024-12-10 12:25:30', 'DAT'), -- valid message different message sub type
	(NEWID(), @GbPkDEM, @GePk, @EccPk01, 'RCV', 'XMS', 'AAA', 'AAA', '', CONVERT(VARBINARY(max),'{twoKbMessage}'), '2024-12-10 12:25:30', 'DAT', '2024-12-10 12:25:30', 'DAT'), -- valid message two kb data
	(NEWID(), @GbPkDEM, @GePk, @EccPk01, 'RCV', 'UDM', 'AAA', 'AAA', '{oneKbMessage}', null, '2024-12-10 12:25:30', 'DAT', '2024-12-10 12:25:30', 'DAT'), -- valid message message data in text
	(NEWID(), @GbPkDEM, @GePk, @EccPk01, 'RCV', 'NDM', 'AAA', 'AAA', '{twoKbMessage}', CONVERT(VARBINARY(max),'{oneKbMessage}'), '2024-12-10 12:25:30', 'DAT', '2024-12-10 12:25:30', 'DAT'), -- valid message data in message text and message data
	(NEWID(), @GbPkSIN, @GePk, @EccPk01, 'RCV', 'UDQ', 'AAA', 'CCC', '', CONVERT(VARBINARY(max),'{oneKbMessage}'), '2024-12-10 12:25:30', 'DAT', '2024-12-10 12:25:30', 'DAT'), -- valid message from different branch/company
	(NEWID(), @GbPkSIN, @GePk, @EccPk01, 'RCV', 'UDQ', 'AAA', 'CCC', '', CONVERT(VARBINARY(max),'{twoKbMessage}'), '2024-12-10 12:25:30', 'DAT', '2024-12-10 12:25:30', 'DAT'), -- valid message from different branch/company two kb
	(NEWID(), @GbPkDEM, @GePk, @EccPk03, 'RCV', 'UDQ', 'OUT', 'OUT', '', CONVERT(VARBINARY(max),'{oneKbMessage}'), '2024-12-10 12:25:30', 'DAT', '2024-12-10 12:25:30', 'DAT'), -- valid outbound message
	(NEWID(), @GbPkDEM, @GePk, @EccPk01, 'RCV', 'UDQ', 'AAA', 'AAA', '', CONVERT(VARBINARY(max),'{oneKbMessage}'), '2024-11-10 12:25:30', 'DAT', '2024-12-01 12:25:30', 'DAT'), -- invalid message too old
	(NEWID(), @GbPkDEM, @GePk, @EccPk01, 'RCV', 'UDQ', 'AAA', 'AAA', '', CONVERT(VARBINARY(max),'{oneKbMessage}'), '2024-12-25 12:25:30', 'DAT', '2024-12-25 12:25:30', 'DAT'), -- invalid message too new
	(NEWID(), @GbPkDEM, @GePk, @EccPk02, 'RCV', 'UDQ', 'AAA', 'AAA', '', CONVERT(VARBINARY(max),'{oneKbMessage}'), '2024-12-10 12:25:30', 'DAT', '2024-12-10 12:25:30', 'DAT'), -- invalid message not sent by CWConnector
	(NEWID(), @GbPkDEM, @GePk, @EccPk04, 'RCV', 'UDQ', 'AAA', 'AAA', '', CONVERT(VARBINARY(max),'{oneKbMessage}'), '2024-12-10 12:25:30', 'DAT', '2024-12-10 12:25:30', 'DAT') -- invalid outbound message not sent by CWConnector";

			TestConnection.ExecuteNonQuery(insertScript);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				var startDate = new DateTime(2024, 12, 10);
				return new RecurringRange(startDate, startDate.AddDays(1));
			}
		}
	}
}
