using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(PortTransportIntelligence))]
	sealed class PortTransportIntelligenceTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;
		protected override void PrepareTestData()
		{
			var sqlText = @"
			DECLARE @utcNow smalldatetime = GetUtcDate();
			DECLARE @UserCode1 NVARCHAR(50) = 'US1';
			DECLARE @GcPk UNIQUEIDENTIFIER = (
				SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM'
			);
			DECLARE @GbPk UNIQUEIDENTIFIER = (
				SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM'
			);
			DECLARE @GePk UNIQUEIDENTIFIER = (
				SELECT TOP(1) GE_PK FROM dbo.GlbDepartment
			);

			INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
				(NEWID(), 'SY1', @GcPk),
				(NEWID(), 'TK1', @GcPk);

			--JobShipment
			DECLARE @JSPk01 UNIQUEIDENTIFIER = newid();

			--JobContainer
			DECLARE @JCPK01 UNIQUEIDENTIFIER = newid();
			DECLARE @JCPK02 UNIQUEIDENTIFIER = newid();

			--JobCartage
			DECLARE @JJPK01 UNIQUEIDENTIFIER = newid();

			--OrgHeader
			DECLARE @OHPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @OHPk02 UNIQUEIDENTIFIER = newid();
			DECLARE @OHPk03 UNIQUEIDENTIFIER = newid();
			DECLARE @OHPk04 UNIQUEIDENTIFIER = newid();
			DECLARE @OHPk05 UNIQUEIDENTIFIER = newid();

			--OrgAddress
			DECLARE @OAPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @OAPk02 UNIQUEIDENTIFIER = newid();
			DECLARE @OAPk03 UNIQUEIDENTIFIER = newid();
			DECLARE @OAPk04 UNIQUEIDENTIFIER = newid();
			DECLARE @OAPk05 UNIQUEIDENTIFIER = newid();

			--JobDocAddress
			DECLARE @E2Pk01 UNIQUEIDENTIFIER = newid();
			DECLARE @E2Pk02 UNIQUEIDENTIFIER = newid();
			DECLARE @E2Pk03 UNIQUEIDENTIFIER = newid();
			DECLARE @E2Pk04 UNIQUEIDENTIFIER = newid();
			DECLARE @E2Pk05 UNIQUEIDENTIFIER = newid();

			--JobCartageRunSheet
			DECLARE @EYPk01 UNIQUEIDENTIFIER = newid();

			--JobBookedCtgMove
			DECLARE @EWPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @EWPk02 UNIQUEIDENTIFIER = newid();

			--JobContainerLegs
			DECLARE @JUPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @JUPk02 UNIQUEIDENTIFIER = newid();
			DECLARE @JUPk03 UNIQUEIDENTIFIER = newid();
			DECLARE @JUPk04 UNIQUEIDENTIFIER = newid();

			-- Ensure unique JU_SplitDeliverySuffix for each EW_JJ
			DECLARE @Suffix1 NVARCHAR(10) = 'A';
			DECLARE @Suffix2 NVARCHAR(10) = 'B';
			DECLARE @Suffix3 NVARCHAR(10) = 'C';
			DECLARE @Suffix4 NVARCHAR(10) = 'D';

			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES
				(@OHPk01, 'OH1'),
				(@OHPk02, 'OH2'),
				(@OHPk03, 'OH3'),
				(@OHPk04, 'OH4'),
				(@OHPk05, 'OH5');

			INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Code, OA_Address1, OA_City, OA_State, OA_PostCode, OA_RN_NKCountryCode, OA_SystemCreateTimeUtc) VALUES
				(@OAPk01, @OHPk01, 'PST: 10 HUTCHESON Street', '10 BEDFORD SQUARE', 'Sydney', 'LND', '1900', 'AU', @utcNow),
				(@OAPk02, @OHPk02, 'PST: 12 YORKSHIRE Street', '12 WEST YORKSHIRE', 'Como', 'QLD', '2000', 'AU', @utcNow),
				(@OAPk03, @OHPk03, 'PST: 13 HUTCHESON Street', '13 BEDFORD SQUARE', '', 'LND', '2001', 'AU', @utcNow),
				(@OAPk04, @OHPk04, 'PST: 14 HUTCHESON Street', '14 BEDFORD SQUARE', '', 'LND', '2002', 'AU', @utcNow),
				(@OAPk05, @OHPk05, 'PST: 15 HUTCHESON Street', '15 BEDFORD SQUARE', '', 'LND', '2003', 'AU', @utcNow);

			INSERT INTO dbo.JobDocAddress (E2_PK, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_City, E2_State, E2_RN_NKCountryCode, E2_OA_Address, E2_AddressOverride, E2_ValidationStatus, E2_CompanyName, E2_SystemCreateTimeUtc, E2_SystemCreateUser, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser) VALUES
				(@E2Pk01, @JJPK01, 'JJ', 'LCT', 'Edgecliff', 'NSW', 'AU', @OAPK01, 0, 'NRQ', '', @utcNow, @UserCode1, @utcNow, @UserCode1),
				(@E2Pk02, @JJPK01, 'JJ', 'LCI', '', '', 'AU', @OAPK02, 0, 'NRQ', '', @utcNow, @UserCode1, @utcNow, @UserCode1),
				(@E2Pk03, @JJPK01, 'JJ', 'LCY', '', '', 'AU', @OAPK03, 0, 'NRQ', '', @utcNow, @UserCode1, @utcNow, @UserCode1);

			INSERT INTO dbo.JobCartageRunSheet (EY_PK, EY_RunSheetNumber, EY_StartTime, EY_EndTime, EY_OH_TransportCo, EY_SystemCreateTimeUtc, EY_SystemCreateUser, EY_SystemLastEditTimeUtc, EY_SystemLastEditUser) VALUES
				(@EYPk01,'RS00001009','2024-10-29 00:00:00', '2024-10-29 23:59:00',@OHPk01, @utcNow, @UserCode1, @utcNow, @UserCode1)

			INSERT INTO dbo.JobContainer (JC_PK) VALUES
				(@JCPK01),
				(@JCPK02);

			INSERT INTO [dbo].[JobShipment] (JS_PK,JS_UniqueConsignRef,JS_ShipmentType,JS_TransportMode,JS_PackingMode,JS_RL_NKOrigin,JS_RL_NKDestination) VALUES
			(@JSPk01,N'JsUniqueConsignRef01',N'STD',N'SEA',N'FCL',N'CNSAA',N'AUS2E');

			INSERT INTO dbo.JobCartage (JJ_PK, JJ_IsValid, JJ_IsCancelled, JJ_ShippingTransportMode, JJ_ContainerMode, JJ_Direction, JJ_ConsignmentID, JJ_E3_NKJobType, JJ_DropMode, JJ_GB, JJ_ParentID, JJ_ParentTableCode, JJ_SystemLastEditTimeUtc, JJ_SystemLastEditUser, JJ_SystemCreateTimeUtc, JJ_SystemCreateUser) VALUES
				(@JJPK01, 1, 0, 'SEA', 'CNT', 'IMP', 'T00001013', 'ISCC', 'WUP', @GbPk, @JSPk01, 'JS', @utcNow, @UserCode1, @utcNow, @UserCode1);

			INSERT INTO dbo.JobBookedCtgMove (EW_PK, EW_JJ, EW_JC_Container, EW_E2PickupAddressID, EW_E2WaitPointAddressID, EW_E2DeliveryAddressID, EW_DisplayOrder, EW_SystemCreateTimeUtc, EW_SystemCreateUser, EW_SystemLastEditTimeUtc, EW_SystemLastEditUser) VALUES
				(@EWPk01, @JJPK01, @JCPK01, @E2Pk01, @E2Pk02, NULL, 1, @utcNow, @UserCode1, @utcNow, @UserCode1),
				(@EWPk02, @JJPK01, @JCPK02, @E2Pk01, @E2Pk02, NULL, 1, @utcNow, @UserCode1, @utcNow, @UserCode1);

			INSERT INTO dbo.JobContainerLegs (JU_PK, JU_EW, JU_DisplayOrder, JU_SplitDeliverySuffix, JU_PlannedPickupTime, JU_PlannedPickupTimeEnd, JU_EstimatedDeliveryTime, JU_EstimatedDeliveryTimeEnd, JU_E2PickupAddressID, JU_PickupTimeIn, JU_PickupTimeOut, JU_CartagePickupDemurrage, JU_E2WaitPointAddressID, JU_WaitPointTimeIn, JU_WaitPointTimeOut, JU_CartageWaitPointDemurrage, JU_E2DeliveryAddressID, JU_DeliverTimeIn, JU_DeliverTimeOut, JU_CartageDeliveryDemurrage, JU_EY_RunSheet, JU_SystemCreateTimeUtc, JU_SystemLastEditTimeUtc, JU_SystemCreateUser) VALUES
				(@JUPk01, @EWPk01, 2, @Suffix1, '2024-10-23 13:29:00', NULL, '2024-10-26 16:29:00', NULL, @E2Pk01, '2024-10-24 14:29:00', '2024-10-25 15:29:00', '2024-01-02 01:00:00', NULL, '2024-10-27 17:29:00', '2024-10-28 18:30:00', '2024-01-02 01:01:00', @E2Pk03, '2024-10-26 13:30:00', '2024-10-26 14:30:00', '2024-01-01 01:00:00', @EYPk01, '2024-10-23 13:29:00', '2024-10-23 13:29:00', @UserCode1),
				(@JUPk02, @EWPk01, 1, @Suffix2, '2024-10-24 13:29:00', NULL, '2024-10-27 16:29:00', NULL, @E2Pk02, '2024-10-25 14:29:00', '2024-10-26 15:29:00', '2024-01-03 01:00:00', NULL, '2024-10-28 17:29:00', '2024-10-29 18:30:00', '2024-01-03 01:01:00', @E2Pk01, '2024-10-27 13:30:00', NULL, NULL, NULL, '2024-10-24 13:29:00', '2024-10-24 13:29:00', @UserCode1),
				(@JUPk03, @EWPk02, 2, @Suffix3, '2024-10-25 13:29:00', NULL, '2024-10-28 16:29:00', NULL, @E2Pk01, '2024-10-26 14:29:00', '2024-10-27 15:29:00', '2024-01-04 01:00:00', NULL, '2024-10-29 17:29:00', '2024-10-30 18:30:00', '2024-01-04 01:01:00', @E2Pk03, '2024-10-28 13:30:00', NULL, NULL, NULL, '2024-10-25 13:29:00', '2024-10-25 13:29:00', @UserCode1),
				(@JUPk04, @EWPk02, 1, @Suffix4, '2024-10-26 13:29:00', NULL, '2024-10-29 16:29:00', NULL, @E2Pk02, '2024-10-27 14:29:00', '2024-10-28 15:29:00', '2024-01-05 01:00:00', NULL, '2024-10-30 17:29:00', '2024-10-31 18:30:00', '2024-01-05 01:01:00', @E2Pk01, '2024-10-29 13:30:00', NULL, NULL, NULL, '2024-10-26 13:29:00', '2024-10-26 13:29:00', @UserCode1);
";

			TestConnection.ExecuteNonQuery(sqlText);
		}
		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 4, transactions.Count());
			var tb1Transactions = transactions.Where(x => x.Reference1 == "T00001013");
			AssertEquals("Number of Transactions for T00001013", 4, tb1Transactions.Count());
			AssertEquals("Number of Transactions for T00001013 with Reference2 (ContainerMode) equal to CNT", 4, tb1Transactions.Count(x => x.Reference2 == "CNT"));
			AssertEquals("Number of Transactions for T00001013 with Reference3 (ParentTableCode) equal to JS", 4, tb1Transactions.Count(x => x.Reference3 == "JS"));
			AssertEquals("Number of Transactions for T00001013 with Reference3 (TransportCompanyCode) equal to OH1", 1, tb1Transactions.Count(x => x.Reference4 == "OH1"));

			VerifyTransaction(tb1Transactions, new Dictionary<string, string>
			{
				{ "PlannedPickupTime", "2024-10-23T13:29:00" },
				{ "PickupTimeIn", "2024-10-24T14:29:00" },
				{ "EstimatedDeliveryTime", "2024-10-26T16:29:00" },
				{ "DeliverTimeIn", "2024-10-26T13:30:00" },
				{ "PickupPostCode", "1900" },
				{ "PickupCity", "Edgecliff" },
				{ "PickupState", "NSW" },
				{ "PickupCountryCode", "AU" },
				{ "DeliveryPostCode", "2001" },
				{ "DeliveryCity", "" },
				{ "DeliveryState", "LND" },
				{ "DeliveryCountryCode", "AU" }
			});

			VerifyTransaction(tb1Transactions, new Dictionary<string, string>
			{
				{ "PlannedPickupTime", "2024-10-24T13:29:00" },
				{ "PickupTimeIn", "2024-10-25T14:29:00" },
				{ "EstimatedDeliveryTime", "2024-10-27T16:29:00" },
				{ "DeliverTimeIn", "2024-10-27T13:30:00" },
				{ "PickupPostCode", "2000" },
				{ "PickupCity", "Como" },
				{ "PickupState", "QLD" },
				{ "PickupCountryCode", "AU" },
				{ "DeliveryPostCode", "1900" },
				{ "DeliveryCity", "Edgecliff" },
				{ "DeliveryState", "NSW" },
				{ "DeliveryCountryCode", "AU" }
			});

			VerifyTransaction(tb1Transactions, new Dictionary<string, string>
			{
				{ "PlannedPickupTime", "2024-10-25T13:29:00" },
				{ "PickupTimeIn", "2024-10-26T14:29:00" },
				{ "EstimatedDeliveryTime", "2024-10-28T16:29:00" },
				{ "DeliverTimeIn", "2024-10-28T13:30:00" },
				{ "PickupPostCode", "1900" },
				{ "PickupCity", "Edgecliff" },
				{ "PickupState", "NSW" },
				{ "PickupCountryCode", "AU" },
				{ "DeliveryPostCode", "2001" },
				{ "DeliveryCity", "" },
				{ "DeliveryState", "LND" },
				{ "DeliveryCountryCode", "AU" }
			});

			VerifyTransaction(tb1Transactions, new Dictionary<string, string>
			{
				{ "PlannedPickupTime", "2024-10-26T13:29:00" },
				{ "PickupTimeIn", "2024-10-27T14:29:00" },
				{ "EstimatedDeliveryTime", "2024-10-29T16:29:00" },
				{ "DeliverTimeIn", "2024-10-29T13:30:00" },
				{ "PickupPostCode", "2000" },
				{ "PickupCity", "Como" },
				{ "PickupState", "QLD" },
				{ "PickupCountryCode", "AU" },
				{ "DeliveryPostCode", "1900" },
				{ "DeliveryCity", "Edgecliff" },
				{ "DeliveryState", "NSW" },
				{ "DeliveryCountryCode", "AU" }
			});
		}
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2024, 10);
		public void TestIsSystemLevelFeature()
		{
			AssertEquals("IsSystemLevel", false, ScriptToTest.IsSystemLevel);
		}

		void VerifyTransaction(IEnumerable<IStlTransaction> transactions, Dictionary<string, string> keyValuePairs)
		{
			var expectedValues = BuildExpectedValues(keyValuePairs);
			var expectedValuesString = ConvertDictionaryToString(expectedValues);
			var transaction = transactions.Single(x => x.AdditionalRefs == $"{{{expectedValuesString}}}");
			AssertNotNull(transaction);
		}

		Dictionary<string, string> BuildExpectedValues(Dictionary<string, string> keyValuePairs)
		{
			return new Dictionary<string, string>(keyValuePairs);
		}

		string ConvertDictionaryToString(Dictionary<string, string> dictionary)
		{
			return dictionary.Select(kv => $"\"{kv.Key}\":\"{kv.Value}\"")
							 .Aggregate((current, next) => current + "," + next);
		}
	}
}
