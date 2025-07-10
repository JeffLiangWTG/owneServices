using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.eServices;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.eServices
{
	[TestedType(typeof(EDIClientsScript))]
	sealed class EDIClientsScriptTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override sealed IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2025, 4);
			}
		}

		public void TestVersions()
		{
			CombineAssertions("Test minimal and maximal CW1 version", () =>
			{
				AssertEquals("25.3.19.236", ScriptToTest.MinCW1Version);
				AssertEquals("", ScriptToTest.MaxCW1Version);
			});
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			var resutllist = transactions.Select(t => string.Join(",", t.Reference1, t.Reference2, t.Reference3, t.Reference4)).ToList();

			var expectedAdditionalRef1 = @"{""TechnicalContact"":""Test Contact1"",""InboundActive"":true,""InboundAuthorizationType"":""OAU"",""InboundSelfManagedOAuth"":true,""InboundBranch"":""B11"",""InboundDepartment"":""DE1"",""OutboundActive"":true,""OutboundAuthorizationType"":""OAU"",""OutboundEndpoint"":""https:\/\/auth.example.com\/test"",""OutboundGrantType"":""CCT""}";
			var expectedAdditionalRef2 = @"{""TechnicalContact"":""Test Contact1"",""InboundActive"":true,""InboundAuthorizationType"":""BAU"",""InboundSelfManagedOAuth"":false,""InboundBranch"":""B12"",""InboundDepartment"":""DE1"",""OutboundActive"":false,""OutboundAuthorizationType"":""OAU"",""OutboundEndpoint"":""https:\/\/auth.example.com\/test"",""OutboundGrantType"":""CCD""}";
			var expectedAdditionalRef3 = @"{""TechnicalContact"":""Test Contact2"",""InboundActive"":false,""InboundAuthorizationType"":""BAU"",""InboundSelfManagedOAuth"":true,""InboundBranch"":""B11"",""InboundDepartment"":""DE1"",""OutboundActive"":true,""OutboundAuthorizationType"":""BAU"",""OutboundEndpoint"":""https:\/\/auth.example.com\/test"",""OutboundGrantType"":""""}";
			var expectedAdditionalRef4 = @"{""TechnicalContact"":""Test Contact2"",""InboundActive"":false,""InboundAuthorizationType"":""BAU"",""InboundSelfManagedOAuth"":false,""InboundBranch"":""   "",""InboundDepartment"":""   "",""OutboundActive"":false,""OutboundAuthorizationType"":""BAU"",""OutboundEndpoint"":""https:\/\/auth.example.com\/test"",""OutboundGrantType"":""""}";
			var expectedAdditionalRef5 = @"{""TechnicalContact"":""Test Contact1"",""InboundActive"":false,""InboundAuthorizationType"":""BAU"",""InboundSelfManagedOAuth"":true,""InboundBranch"":""B11"",""InboundDepartment"":""DE1"",""OutboundActive"":false,""OutboundAuthorizationType"":""BAU"",""OutboundEndpoint"":""https:\/\/auth.example.com\/test"",""OutboundGrantType"":""""}";
			var expectedAdditionalRef6 = @"{""TechnicalContact"":""Test Contact1"",""InboundActive"":false,""InboundAuthorizationType"":""BAU"",""InboundSelfManagedOAuth"":false,""InboundBranch"":""B12"",""InboundDepartment"":""DE1"",""OutboundActive"":false,""OutboundAuthorizationType"":""BAU"",""OutboundEndpoint"":""https:\/\/auth.example.com\/test"",""OutboundGrantType"":""""}";
			var expectedAdditionalRef7 = @"{""TechnicalContact"":""Test Contact1"",""InboundActive"":true,""InboundAuthorizationType"":""BAU"",""InboundSelfManagedOAuth"":true,""InboundBranch"":""B11"",""InboundDepartment"":""DE1"",""OutboundActive"":true,""OutboundAuthorizationType"":""BAU"",""OutboundEndpoint"":""https:\/\/auth.example.com\/test"",""OutboundGrantType"":""""}";

			AssertEquals("Number of Transactions", 7, transactions.Count());

			AssertRow(transactions, "NEW", "EAD", "TestClient1.test", "true", expectedAdditionalRef1, "2025-04-30 11:58");
			AssertRow(transactions, "EDT", "EAD", "TestClient2.test", "true", expectedAdditionalRef2, "2025-04-30 12:58");
			AssertRow(transactions, "EDT", "EAD", "TestClient3.test", "true", expectedAdditionalRef3, "2025-04-30 13:58");
			AssertRow(transactions, "NEW", "EAD", "TestClient4.test", "true", expectedAdditionalRef4, "2025-04-30 10:58");
			AssertRow(transactions, "EDT", "AAA", "TestClient5.test", "false", expectedAdditionalRef5, "2025-04-30 10:58");
			AssertRow(transactions, "EDT", "AAA", "TestClient6.test", "false", expectedAdditionalRef6, "2025-04-30 10:58");
			AssertRow(transactions, "EDT", "AAA", "TestClient7.test", "true", expectedAdditionalRef7, "2025-04-30 10:00");
		}

		void AssertRow(IEnumerable<IStlTransaction> transactions, string expectedRef1, string expectedRef2, string expectedRef3, string expectedRef4, string expectedAdditionalRefs, string expectedTransactionDate)
		{
			var transaction = transactions.Single(t => t.Reference3 == expectedRef3);
			AssertEquals($"Check Reference1 for {expectedRef3}", expectedRef1, transaction.Reference1);
			AssertEquals($"Check Reference2 for {expectedRef3}", expectedRef2, transaction.Reference2);
			AssertEquals($"Check Reference4 for {expectedRef3}", expectedRef4, transaction.Reference4);
			AssertEquals($"Check AdditionalRefs for {expectedRef3}", expectedAdditionalRefs, transaction.AdditionalRefs);
			AssertEquals($"Check TransactionDate for {expectedRef3}", expectedTransactionDate, transaction.ServiceOccuredUTC.ToString("yyyy-MM-dd HH:mm"));
		}

		protected override void PrepareTestData()
		{
			var sqlText = $@"
			-- Declare EcpPk variables
			DECLARE @EcpPk01 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcpPk02 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcpPk03 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcpPk04 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcpPk05 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcpPk06 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcpPk07 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcpPk08 UNIQUEIDENTIFIER = NEWID();

			-- Declare EccPk variables
			DECLARE @EccPk01 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk02 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk03 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk04 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk05 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk06 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk07 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk08 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk09 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk10 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk11 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk12 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk13 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk14 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk15 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EccPk16 UNIQUEIDENTIFIER = NEWID();

			-- Declare EcaPk variables
			DECLARE @EcaPk01 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcaPk02 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcaPk03 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcaPk04 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcaPk05 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcaPk06 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcaPk07 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcaPk08 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcaPk09 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcaPk10 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcaPk11 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcaPk12 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcaPk13 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcaPk14 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcaPk15 UNIQUEIDENTIFIER = NEWID();
			DECLARE @EcaPk16 UNIQUEIDENTIFIER = NEWID();

			DECLARE @OhPk01 UNIQUEIDENTIFIER = NEWID();
			DECLARE @OhPk02 UNIQUEIDENTIFIER = NEWID();
			DECLARE @OaPk01 UNIQUEIDENTIFIER = NEWID();
			DECLARE @OaPk02 UNIQUEIDENTIFIER = NEWID();
			DECLARE @OcPk01 UNIQUEIDENTIFIER = NEWID();
			DECLARE @OcPk02 UNIQUEIDENTIFIER = NEWID();
			DECLARE @PerPk01 UNIQUEIDENTIFIER = NEWID();
			DECLARE @PerPk02 UNIQUEIDENTIFIER = NEWID();
			DECLARE @GcPk1 UNIQUEIDENTIFIER = NEWID();
			DECLARE @GcPk2 UNIQUEIDENTIFIER = NEWID();
			DECLARE @GbPk1 UNIQUEIDENTIFIER = NEWID();
			DECLARE @GbPk2 UNIQUEIDENTIFIER = NEWID();
			DECLARE @GePk UNIQUEIDENTIFIER = NEWID();

			INSERT dbo.GlbPerson (PER_PK, PER_FullName) VALUES
				(@PerPk01, 'Test Contact Full Name 1'),
				(@PerPk02, 'Test Contact Full Name 2');

			INSERT dbo.OrgHeader (OH_PK, OH_Code,OH_FullName) VALUES
				(@OhPk01,'OH1','OrgHeader1'),
				(@OhPk02,'OH2','OrgHeader2');

			INSERT dbo.OrgContact (OC_PK, OC_OH, OC_PER, OC_ContactName) VALUES
				(@OcPk01, @OhPk01, @PerPk01, 'Test Contact1'),
				(@OcPk02, @OhPk02, @PerPk01, 'Test Contact2');

			INSERT dbo.OrgAddress (OA_PK, OA_OH, OA_Address1) VALUES
				(@OaPk01 ,@OhPk01, 'Address01'),
				(@OaPk02 ,@OhPk02, 'Address02');

			INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES
				(@GcPk1, 'DK1', 'DK company', 'EUR', 'DK'),
				(@GcPk2, 'CN1', 'CN company', 'CNY', 'CN');

			INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy, GB_RL_NKHomePort) VALUES
				(@GbPk1, 'B11', @GcPk1, @OhPk01, 'AUSYD'),
				(@GbPk2, 'B12', @GcPk2, @OhPk01, 'CNSZG');
			INSERT dbo.GlbDepartment (GE_PK, GE_Code) VALUES 
				(@GePk, 'DE1');

			INSERT INTO dbo.EDICommunicationParty (ECP_PK, ECP_IsActive, ECP_Name, ECP_Summary, ECP_ApplicationCode, ECP_OC_TechnicalContact, ECP_SystemCreateTimeUtc, ECP_SystemCreateUser, ECP_SystemLastEditTimeUtc, ECP_SystemLastEditUser) VALUES
				(@EcpPk01, 1, 'TestClient1', 'test', 'EAD', @OcPk01, '2025-4-30 10:58', 'DAT', '2025-4-30 10:58', 'DAT'), -- Active inbound with OAuth auth type/Active outbound with OAuth auth type
				(@EcpPk02, 1, 'TestClient2', 'test', 'EAD', @OcPk01, '2025-3-30 10:58', 'DAT', '2025-4-30 8:58', 'DAT'), -- Active inbound with basic auth type/inactive outbound
				(@EcpPk03, 1, 'TestClient3', 'test', 'EAD', @OcPk02, '2025-3-30 10:58', 'DAT', '2025-4-30 9:58', 'DAT'), -- inactive inbound/Active outbound with basic auth type
				(@EcpPk04, 1, 'TestClient4', 'test', 'EAD', @OcPk02, '2025-4-30 10:58', 'DAT', '2025-4-30 10:58', 'DAT'), -- inactive inbound/inactive outbound
				(@EcpPk05, 0, 'TestClient5', 'test', 'AAA', @OcPk01, '2025-3-30 10:58', 'DAT', '2025-4-30 10:58', 'DAT'), -- create last month, update party this month
				(@EcpPk06, 0, 'TestClient6', 'test', 'AAA', @OcPk01, '2025-3-30 10:58', 'DAT', '2025-3-30 10:58', 'DAT'), -- create last month, update conf this month
				(@EcpPk07, 1, 'TestClient7', 'test', 'AAA', @OcPk01, '2025-3-30 10:58', 'DAT', '2025-3-30 10:58', 'DAT'), -- create last month, update auth this month
				(@EcpPk08, 1, 'TestClient8', 'test', 'AAA', @OcPk01, '2025-3-30 10:58', 'DAT', '2025-3-30 10:58', 'DAT'); -- out of range
			INSERT INTO dbo.EDICommunicationAuth (
				ECA_PK, ECA_AuthorizationMode, ECA_AuthorizationEndpoint, ECA_ClientID, ECA_ClientSecret, ECA_Username, ECA_Password, ECA_Scopes,
				ECA_SystemCreateTimeUtc, ECA_SystemCreateUser, ECA_SystemLastEditTimeUtc, ECA_SystemLastEditUser, ECA_EncodedPrivateKey, ECA_FlowCode, ECA_Certificate, ECA_OperationId,
				ECA_RenewalEncodedPrivateKey, ECA_RenewalOperationId) VALUES
				(@EcaPk01, 'OAU', 'https://auth.example.com/test', 'client-id-001','secret-001', '', '', 'scope1',
				'2025-04-30 10:58', 'DAT', '2025-04-30 10:58', 'DAT', NULL, '', NULL, 'op-id-001', NULL, 'renew-op-id-001'),
				(@EcaPk02, 'OAU', 'https://auth.example.com/test', 'client-id-002','secret-002', '', '', 'scope2',
				'2025-04-30 10:58', 'DAT', '2025-04-30 11:58', 'DAT', NULL, 'CCT', NULL, 'op-id-002', NULL, 'renew-op-id-001'),
				(@EcaPk03, 'BAU', '', '', '', 'user002', 'pass002', '',
				'2025-04-30 10:58', 'DAT', '2025-04-30 10:00:00', 'DAT', NULL, '', NULL, 'op-id-003',NULL,''),
				(@EcaPk04, 'OAU', 'https://auth.example.com/test', 'client-id-004','secret-004', '', '', 'scope4',
				'2025-04-30 10:58', 'DAT', '2025-04-30 12:58', 'DAT', NULL, 'CCD', NULL, 'op-id-001', NULL, 'renew-op-id-001'),
				(@EcaPk05, 'BAU', '', '', '', 'user005', 'pass005', '',
				'2025-04-30 10:58', 'DAT', '2025-04-30 11:00:00', 'DAT', NULL, '', NULL, 'op-id-005', NULL,''),
				(@EcaPk06, 'BAU', '', '', '', 'user006', 'pass006', '',
				'2025-04-30 10:58', 'DAT', '2025-04-30 12:00:00', 'DAT', NULL, '', NULL, 'op-id-006', NULL,''),
				(@EcaPk07, 'BAU', '', '', '', 'user007', 'pass007', '',
				'2025-04-30 10:58', 'DAT', '2025-04-30 10:00:00', 'DAT', NULL, '', NULL, 'op-id-007', NULL, ''),
				(@EcaPk08, 'BAU', '', '', '', 'user008', 'pass008', '',
				'2025-04-30 10:58', 'DAT', '2025-04-30 10:00:00', 'DAT', NULL, '', NULL, 'op-id-008', NULL, ''),
				(@EcaPk09, 'BAU', '', '', '', 'user009', 'pass009', '',
				'2025-03-30 10:58', 'DAT', '2025-03-30 10:00:00', 'DAT', NULL, '', NULL, 'op-id-009', NULL, ''),
				(@EcaPk10, 'BAU', '', '', '', 'user010', 'pass010', '',
				'2025-03-30 10:58', 'DAT', '2025-03-30 10:00:00', 'DAT', NULL, '', NULL, 'op-id-010', NULL, ''),
				(@EcaPk11, 'BAU', '', '', '', 'user011', 'pass011', '',
				'2025-03-30 10:58', 'DAT', '2025-03-30 10:00:00', 'DAT', NULL, '', NULL, 'op-id-011', NULL, ''),
				(@EcaPk12, 'BAU', '', '', '', 'user012', 'pass012', '',
				'2025-03-30 10:58', 'DAT', '2025-03-30 10:00:00', 'DAT', NULL, '', NULL, 'op-id-012', NULL, ''),
				(@EcaPk13, 'BAU', '', '', '', 'user013', 'pass013', '',
				'2025-03-30 10:58', 'DAT', '2025-04-30 10:00:00', 'DAT', NULL, '', NULL, 'op-id-013', NULL, ''),
				(@EcaPk14, 'BAU', '', '', '', 'user014', 'pass014', '',
				'2025-03-30 10:58', 'DAT', '2025-04-30 10:00:00', 'DAT', NULL, '', NULL, 'op-id-014', NULL, ''),
				(@EcaPk15, 'BAU', '', '', '', 'user015', 'pass015', '',
				'2025-03-30 10:58', 'DAT', '2025-03-30 10:00:00', 'DAT', NULL, '', NULL, 'op-id-015', NULL, ''),
				(@EcaPk16, 'BAU', '', '', '', 'user016', 'pass016', '',
				'2025-03-30 10:58', 'DAT', '2025-03-30 10:00:00', 'DAT', NULL, '', NULL, 'op-id-016', NULL, '');
			INSERT INTO dbo.EDICommunicationPartyConfig (ECC_PK, ECC_IsActive, ECC_Status, ECC_ECP_Party, ECC_Direction, ECC_Endpoint, ECC_ECA_Auth, ECC_GB_Branch, ECC_GE_Department, ECC_IsSelfManaged, ECC_SystemCreateTimeUtc, ECC_SystemCreateUser, ECC_SystemLastEditTimeUtc, ECC_SystemLastEditUser) VALUES
				(@EccPk01, 1, 'CNF', @EcpPk01, 'IN', '', @EcaPk01, @GbPk1, @GePk, 1, '2025-4-30 10:58', 'DAT', '2025-4-30 10:58', 'DAT'),
				(@EccPk02, 1, 'CNF', @EcpPk01, 'OUT', 'https://auth.example.com/test', @EcaPk02, null, null, 0, '2025-4-30 10:58', 'DAT', '2025-4-30 10:58', 'DAT'),
				(@EccPk03, 1, 'CNF', @EcpPk02, 'IN', '', @EcaPk03, @GbPk2, @GePk, 0, '2025-4-30 10:58', 'DAT', '2025-4-30 10:58', 'DAT'),
				(@EccPk04, 0, 'CNF', @EcpPk02, 'OUT', 'https://auth.example.com/test', @EcaPk04, null, null, 0, '2025-4-30 10:58', 'DAT', '2025-4-30 10:58', 'DAT'),
				(@EccPk05, 0, 'CNF', @EcpPk03, 'IN', '', @EcaPk05, @GbPk1, @GePk, 1, '2025-4-30 10:58', 'DAT', '2025-4-30 13:58', 'DAT'),
				(@EccPk06, 1, 'CNF', @EcpPk03, 'OUT', 'https://auth.example.com/test', @EcaPk06, null, null, 0, '2025-4-30 10:58', 'DAT', '2025-4-30 10:58', 'DAT'),
				(@EccPk07, 0, 'CNF', @EcpPk04, 'IN', '', @EcaPk07, null, null, 0, '2025-4-30 10:58', 'DAT', '2025-4-30 10:58', 'DAT'),
				(@EccPk08, 0, 'CNF', @EcpPk04, 'OUT', 'https://auth.example.com/test', @EcaPk08, null, null, 0, '2025-4-30 10:58', 'DAT', '2025-4-30 10:58', 'DAT'),
				(@EccPk09, 0, 'CNF', @EcpPk05, 'IN', '', @EcaPk09, @GbPk1, @GePk, 1, '2025-3-30 10:58', 'DAT', '2025-3-30 10:58', 'DAT'),
				(@EccPk10, 0, 'CNF', @EcpPk05, 'OUT', 'https://auth.example.com/test', @EcaPk10, null, null, 0, '2025-3-30 10:58', 'DAT', '2025-3-30 10:58', 'DAT'),
				(@EccPk11, 0, 'CNF', @EcpPk06, 'IN', '', @EcaPk11, @GbPk2, @GePk, 0, '2025-3-30 10:58', 'DAT', '2025-4-30 10:58', 'DAT'),
				(@EccPk12, 0, 'CNF', @EcpPk06, 'OUT', 'https://auth.example.com/test', @EcaPk12, null, null, 0, '2025-3-30 10:58', 'DAT', '2025-4-30 10:58', 'DAT'),
				(@EccPk13, 1, 'CNF', @EcpPk07, 'IN', '', @EcaPk13, @GbPk1, @GePk, 1, '2025-3-30 10:58', 'DAT', '2025-3-30 10:58', 'DAT'),
				(@EccPk14, 1, 'CNF', @EcpPk07, 'OUT', 'https://auth.example.com/test', @EcaPk14, null, null, 0, '2025-3-30 10:58', 'DAT', '2025-3-30 10:58', 'DAT'),
				(@EccPk15, 1, 'CNF', @EcpPk08, 'IN', '', @EcaPk15, @GbPk2, @GePk, 0, '2025-3-30 10:58', 'DAT', '2025-3-30 10:58', 'DAT'),
				(@EccPk16, 1, 'CNF', @EcpPk08, 'OUT', 'https://auth.example.com/test', @EcaPk16, null, null, 0, '2025-3-30 10:58', 'DAT', '2025-3-30 10:58', 'DAT');";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
