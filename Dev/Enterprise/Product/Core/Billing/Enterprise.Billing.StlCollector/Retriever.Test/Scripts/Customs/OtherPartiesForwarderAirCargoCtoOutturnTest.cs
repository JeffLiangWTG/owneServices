using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(OtherPartiesForwarderAirCargoCtoOutturn))]
	sealed class OtherPartiesForwarderAirCargoCtoOutturnTest : RefStlScriptWithDefaultsTest
	{
		readonly Guid C5Pk01 = Guid.NewGuid();
		readonly Guid C5Pk02 = Guid.NewGuid();
		readonly Guid C5Pk03 = Guid.NewGuid();
		readonly Guid C5Pk04 = Guid.NewGuid();
		readonly Guid C5Pk05 = Guid.NewGuid();
		readonly Guid C5Pk06 = Guid.NewGuid();
		readonly Guid C5Pk07 = Guid.NewGuid();

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @C4Pk01 UNIQUEIDENTIFIER = newid();
				DECLARE @C4Pk02 UNIQUEIDENTIFIER = newid();
				DECLARE @C4Pk03 UNIQUEIDENTIFIER = newid();
				DECLARE @C4Pk04 UNIQUEIDENTIFIER = newid();
				DECLARE @C4Pk05 UNIQUEIDENTIFIER = newid();
				DECLARE @C4Pk06 UNIQUEIDENTIFIER = newid();
				DECLARE @C4Pk07 UNIQUEIDENTIFIER = newid();
				DECLARE @C4Pk08 UNIQUEIDENTIFIER = newid();
				DECLARE @C4Pk09 UNIQUEIDENTIFIER = newid();

				DECLARE @C5Pk01 UNIQUEIDENTIFIER = '{0}';
				DECLARE @C5Pk02 UNIQUEIDENTIFIER = '{1}';
				DECLARE @C5Pk03 UNIQUEIDENTIFIER = '{2}';
				DECLARE @C5Pk04 UNIQUEIDENTIFIER = '{3}';
				DECLARE @C5Pk05 UNIQUEIDENTIFIER = '{4}';
				DECLARE @C5Pk06 UNIQUEIDENTIFIER = '{5}';
				DECLARE @C5Pk07 UNIQUEIDENTIFIER = '{6}';
				DECLARE @C5Pk08 UNIQUEIDENTIFIER = newid();
				DECLARE @C5Pk09 UNIQUEIDENTIFIER = newid();

				DECLARE @CmPkSg UNIQUEIDENTIFIER = newid();
				DECLARE @CmPkGb UNIQUEIDENTIFIER = newid();
				DECLARE @CsPkSg UNIQUEIDENTIFIER = newid();
				DECLARE @CsPkGb UNIQUEIDENTIFIER = newid();
				DECLARE @CsPkGe UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkSg UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
				DECLARE @GcPkGb UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkSg UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSg);
				DECLARE @GbPkGb UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkGb);

				DECLARE @jkPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @jkPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @jkPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @jsPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @jsPk02 UNIQUEIDENTIFIER = newid();

				UPDATE dbo.GlbCompany SET GC_RN_NKCountryCode = 'GB', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = @GcPkGb;

				INSERT dbo.GlbDepartment (GE_PK) VALUES ( @CsPkGe );

				INSERT dbo.CusMawb (CM_PK, CM_GB, CM_MAWB, CM_MasterHouseBill) VALUES
					(@CmPkGb, @GbPkGb, 'CM01', 'MH01'),
					(@CmPkSg, @GbPkSg, 'CM02', 'MH02');

				INSERT dbo.CusHawb (CS_PK, CS_CM, CS_HAWB) VALUES
					(@CsPkGb, @CmPkGb, 'HAWB1'),
					(@CsPkSg, @CmPkSg, 'HAWB2');

				INSERT dbo.JobConsol (JK_PK, JK_MasterBillNum, JK_UniqueConsignRef) VALUES
					(@jkPk01, 'CM01', 'AAA'),
					(@jkPk02, 'CM02', 'BBB'),
					(@jkPk03, 'CM02', 'CCC');
				INSERT dbo.JobShipment (JS_PK, JS_ShipmentType, JS_HouseBill, JS_UniqueConsignRef) VALUES
					(@jsPk01, 'HLS', 'MH01', 'JS001'),			
					(@jsPk02, 'STD', 'MH02', 'JS002');
				INSERT dbo.JobConShipLink (JN_PK, JN_JK, JN_JS) VALUES
					(newid(), @jkPK01, @jsPk01),
					(newid(), @jkPK02, @jsPk02),
					(newid(), @jkPK03, @jsPk02);

				INSERT dbo.CusUnderbond (c4_PK, C4_MAWB, C4_SendersMessageReference, C4_ApplicationCode) VALUES
					(@C4Pk01, 'MAWB1', 'M1', 'AUU'),
					(@C4Pk02, 'MAWB2', 'M2', 'AUU'),
					(@C4Pk03, 'MAWB3', 'M3', 'AUU'),
					(@C4Pk04, 'MAWB4', 'M4', 'AUU'),
					(@C4Pk05, 'MAWB5', 'M5', 'AUU'),
					(@C4Pk06, 'MAWB6', 'M6', 'AUU'),
					(@C4Pk07, ''     , 'M7', 'AUU'),
					(@C4Pk08, 'MAWB8', 'M8', 'AUU'),
					(@C4Pk09, 'MAWB9', 'M9', 'AUU');

				INSERT dbo.CusOutturn (C5_PK, C5_ParentID, C5_OutturnResultType, C5_ContainerNumber, C5_MasterBill, C5_HouseBill, C5_c4_Underbond, C5_OuterPacks, C5_ParentTableCode, C5_LastMessageDate) VALUES
					(@C5Pk01, @CsPkGb, 'OR1', 'CN01', ''    , ''    , @C4Pk01, 1, 'CS', null),
					(@C5Pk02, @CsPkSg, 'OR2', 'CN02', ''    , 'HB02', @C4Pk02, 2, 'CS', '2013-10-2'),
					(@C5Pk03, newid(), 'OR3', ''    , 'MB03', 'HB03', @C4Pk03, 3, 'CS', '2013-10-2'),
					(@C5Pk04, newid(), 'OR4', ''    , ''    , 'HB04', @C4Pk04, 4, 'CS', '2013-10-2'),
					(@C5Pk05, @CsPkSg, 'OR5', '  '  , '  '  , '  '  , @C4Pk05, 5, 'CS', '2013-10-2'),
					(@C5Pk06, newid(), 'OR6', ''    , ''    , ''    , @C4Pk06, 6, 'CS', '2013-10-2'),
					(@C5Pk07, newid(), 'OR7', ''    , ''    , ''    , @C4Pk07, 7, 'CS', null),
					(@C5Pk08, newid(), 'OR8', ''    , ''    , ''    , @C4Pk08, 8, 'CS', null),
					(@C5Pk09, newid(), 'OR9', ''    , ''    , ''    , @C4Pk09, 9, 'CS', null);

				INSERT dbo.EDIMessage (EM_PK, EM_GB, EM_GE, EM_LinkUniqueID, EM_LinkTable, EM_SystemCreateTimeUtc, EM_ReceiveTransmit, EM_MessageType, EM_MessageSubType, EM_Status, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES
					(newid(), @GbPkSg, @CsPkGe, @C4Pk01, 'CusUnderbond', '2013-09-01', 'TRX', 'AUT', 'ORG', 'SNT', 'US1', '2013-09-01', 'US1'), -- ignored, date out of range
					(newid(), @GbPkSg, @CsPkGe, @C4Pk01, 'CusUnderbond', '2013-10-01', 'TRX', 'AUT', 'ORG', 'SNT', 'US1', '2013-10-01', 'US1'), -- ignored, minimum date out of range
					(newid(), @GbPkSg, @CsPkGe, @C4Pk02, 'CusUnderbond', '2013-10-02', 'TRX', 'AUT', 'ORG', 'SNT', 'US2', '2013-10-02', 'US2'), -- CTN
					(newid(), @GbPkSg, @CsPkGe, @C4Pk03, 'CusUnderbond', '2013-10-13', 'TRX', 'AUT', 'ORG', 'SNT', 'US3', '2013-10-13', 'US3'), -- MBL
					(newid(), @GbPkSg, @CsPkGe, @C4Pk04, 'CusUnderbond', '2013-10-14', 'TRX', 'AUT', 'ORG', 'SNT', 'US4', '2013-10-14', 'US4'), -- HBL
					(newid(), @GbPkSg, @CsPkGe, @C4Pk05, 'CusUnderbond', '2013-10-15', 'TRX', 'AUT', 'ORG', 'SNT', 'US5', '2013-10-15', 'US5'), -- HAWB
					(newid(), @GbPkSg, @CsPkGe, @C4Pk06, 'CusUnderbond', '2013-10-26', 'TRX', 'AUT', 'ORG', 'SNT', 'US6', '2013-10-26', 'US6'), -- MAWB
					(newid(), @GbPkSg, @CsPkGe, @C4Pk07, 'CusUnderbond', '2013-10-27', 'TRX', 'SUT', 'ORG', 'SNT', 'US7', '2013-10-27', 'US7'), -- Nothing
					(newid(), @GbPkSg, @CsPkGe, @C4Pk08, 'CusUnderbond', '2013-10-28', 'TRX', 'SUT', 'ORG', 'QUE', 'US8', '2013-10-28', 'US8'), -- ignored, wrong status
					(newid(), @GbPkSg, @CsPkGe, @C4Pk08, 'CusUnderbond', '2013-10-28', 'TRX', 'XXX', 'ORG', 'SNT', 'US8', '2013-10-28', 'US8'), -- ignored, wrong type
					(newid(), @GbPkSg, @CsPkGe, @C4Pk08, 'CusUnderbond', '2013-10-28', 'TRX', 'SUT', 'XUS', 'SNT', 'US8', '2013-10-28', 'US8'), -- ignored, wrong subtype
					(newid(), @GbPkSg, @CsPkGe, @C4Pk08, 'CusUnderbond', '2013-10-28', 'TRX', 'AUT', 'XUS', 'SNT', 'US8', '2013-10-28', 'US8'), -- ignored, wrong subtype
					(newid(), @GbPkSg, @CsPkGe, @C4Pk08, 'CusUnderbond', '2013-10-28', 'RX' , 'AUT', 'ORG', 'SNT', 'US8', '2013-10-28', 'US8'), -- ignored, wrong mode
					(newid(), @GbPkGb, @CsPkGe, @C4Pk09, 'CusUnderbond', '2013-10-29', 'TRX', 'AUT', 'ORG', 'SNT', 'US9', '2013-10-29', 'US9'); -- ignored, company country is GB
				";
			// Add C5_LastMessageDate for C5PK02 C5PK03 C5PK04 C5PK05 C5PK06
			var query = string.Format(sqlText, C5Pk01, C5Pk02, C5Pk03, C5Pk04, C5Pk05, C5Pk06, C5Pk07);
			TestConnection.ExecuteNonQuery(query);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "M2");
			AssertEquals("[T1] CompanyCode", "SIN", transaction1.GetCompanyCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2013, 10, 2), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] TransactionReference02", "CTN: CN02 2 OR2", transaction1.Reference2);
			AssertEquals("[T1] TransactionReference03", "MH02", transaction1.Reference3);
			AssertEquals("[T1] TransactionReference04", "MAWB: CM02", transaction1.Reference4);
			AssertEquals("[T1] TransactionGuidReference", C5Pk02, Guid.Parse(transaction1.Reference5));
			AssertEquals("[T1] BranchCode", "SIN", transaction1.GetBranchCode());
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);

			var transaction2 = FindRowByRef1(transactions, "M5");
			AssertEquals("[T2] CompanyCode", "SIN", transaction2.GetCompanyCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2013, 10, 15), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] TransactionReference02", "HAWB: HAWB2 5 OR5", transaction2.Reference2);
			AssertEquals("[T2] TransactionReference03", "MH02", transaction2.Reference3);
			AssertEquals("[T2] TransactionReference04", "MAWB: CM02", transaction2.Reference4);
			AssertEquals("[T2] TransactionGuidReference", C5Pk05, Guid.Parse(transaction2.Reference5));
			AssertEquals("[T2] BranchCode", "SIN", transaction2.GetBranchCode());
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2013, 10);
			}
		}
	}
}
