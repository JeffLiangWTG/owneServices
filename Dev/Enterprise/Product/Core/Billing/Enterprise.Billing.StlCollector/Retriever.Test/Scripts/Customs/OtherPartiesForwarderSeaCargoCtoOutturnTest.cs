using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(OtherPartiesForwarderSeaCargoCtoOutturn))]
	sealed class OtherPartiesForwarderSeaCargoCtoOutturnTest : RefStlScriptWithDefaultsTest
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

				DECLARE @C6Pk01 UNIQUEIDENTIFIER = newid();
				DECLARE @C6Pk02 UNIQUEIDENTIFIER = newid();
				DECLARE @C6Pk03 UNIQUEIDENTIFIER = newid();
				DECLARE @C6Pk04 UNIQUEIDENTIFIER = newid();
				DECLARE @C6Pk05 UNIQUEIDENTIFIER = newid();
				DECLARE @C6Pk06 UNIQUEIDENTIFIER = newid();
				DECLARE @C6Pk07 UNIQUEIDENTIFIER = newid();
				DECLARE @C6Pk08 UNIQUEIDENTIFIER = newid();
				DECLARE @C6Pk09 UNIQUEIDENTIFIER = newid();

				DECLARE @CmPkSg UNIQUEIDENTIFIER = newid();
				DECLARE @CmPkGb UNIQUEIDENTIFIER = newid();
				DECLARE @CsPkSg UNIQUEIDENTIFIER = newid();
				DECLARE @CsPkGb UNIQUEIDENTIFIER = newid();
				DECLARE @CsPkGe UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkSg UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
				DECLARE @GcPkGb UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkSg UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSg);
				DECLARE @GbPkGb UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkGb);

				UPDATE dbo.GlbCompany SET GC_RN_NKCountryCode = 'GB', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = @GcPkGb;

				INSERT dbo.GlbDepartment (GE_PK) VALUES ( @CsPkGe );

				INSERT dbo.CusMawb (CM_PK, CM_GB) VALUES
					(@CmPkGb, @GbPkGb),
					(@CmPkSg, @GbPkSg);

				INSERT dbo.CusHawb (CS_PK, CS_CM, CS_HAWB) VALUES
					(@CsPkGb, @CmPkGb, 'HAWB1'),
					(@CsPkSg, @CmPkSg, 'HAWB2');

				INSERT dbo.CusOutturnHeader (C6_PK, C6_SendersMessageReference, C6_VoyageNum) VALUES
					(@C6Pk01, 'M1', 'V1'),
					(@C6Pk02, 'M2', 'V2'),
					(@C6Pk03, 'M3', 'V3'),
					(@C6Pk04, 'M4', 'V4'),
					(@C6Pk05, 'M5', 'V5'),
					(@C6Pk06, 'M6', 'V6'),
					(@C6Pk07, 'M7', 'V7'),
					(@C6Pk08, 'M8', 'V8'),
					(@C6Pk09, 'M9', 'V9');

				INSERT dbo.CusUnderbond (c4_PK, c4_c6, C4_MAWB, C4_SendersMessageReference, C4_ApplicationCode) VALUES
					(@C4Pk01, @C6Pk01, 'MAWB1', 'M1', 'AUU'),
					(@C4Pk02, @C6Pk02, 'MAWB2', 'M2', 'AUU'),
					(@C4Pk03, @C6Pk03, 'MAWB3', 'M3', 'AUU'),
					(@C4Pk04, @C6Pk04, 'MAWB4', 'M4', 'AUU'),
					(@C4Pk05, @C6Pk05, 'MAWB5', 'M5', 'AUU'),
					(@C4Pk06, @C6Pk06, 'MAWB6', 'M6', 'AUU'),
					(@C4Pk07, @C6Pk07, ''     , 'M7', 'AUU'),
					(@C4Pk08, @C6Pk08, 'MAWB8', 'M8', 'AUU'),
					(@C4Pk09, @C6Pk09, 'MAWB9', 'M9', 'AUU');

				INSERT dbo.CusOutturn (C5_PK, C5_ParentID, C5_OutturnResultType, C5_CargoType, C5_ContainerNumber, C5_MasterBill, C5_HouseBill, C5_c4_Underbond, C5_OuterPacks, C5_ParentTableCode, C5_PackagesOutturned) VALUES
					(@C5Pk01, @CsPkGb, 'OR1', 'CT1', 'CN01', ''    , ''    , @C4Pk01, 1, 'CS', 1),
					(@C5Pk02, @CsPkSg, 'OR2', 'CT2', 'CN02', 'MB02', 'HB02', @C4Pk02, 2, 'CS', 2),
					(@C5Pk03, newid(), 'OR3', 'CT3', ''    , 'MB03', 'HB03', @C4Pk03, 3, 'CS', 3),
					(@C5Pk04, newid(), 'OR4', 'CT4', ''    , ''    , 'HB04', @C4Pk04, 4, 'CS', 4),
					(@C5Pk05, @CsPkSg, 'OR5', 'CT5', '  '  , '  '  , '  '  , @C4Pk05, 5, 'CS', 5),
					(@C5Pk06, newid(), 'OR6', 'CT6', ''    , ''    , ''    , @C4Pk06, 6, 'CS', 6),
					(@C5Pk07, newid(), 'OR7', 'CT7', ''    , ''    , ''    , @C4Pk07, 7, 'CS', 7),
					(@C5Pk08, newid(), 'OR8', 'CT8', ''    , ''    , ''    , @C4Pk08, 8, 'CS', 8),
					(@C5Pk09, newid(), 'OR9', 'CT9', ''    , ''    , ''    , @C4Pk09, 9, 'CS', 9);

				INSERT dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_SE_NKEvent, SL_PostedTimeUtc, SL_EventTime, SL_GS_NKUser) VALUES
					(newid(), 'CusOutturn', @C4Pk01, 'ADD', '2013-10-01', '2013-10-01', 'US1'),
					(newid(), 'CusOutturn', @C4Pk02, 'ADD', '2013-10-02', '2013-10-02', 'US2'),
					(newid(), 'CusOutturn', @C4Pk03, 'ADD', '2013-10-13', '2013-10-13', 'US3'),
					(newid(), 'CusOutturn', @C4Pk04, 'ADD', '2013-10-14', '2013-10-14', 'US4'),
					(newid(), 'CusOutturn', @C4Pk05, 'ADD', '2013-10-15', '2013-10-15', 'US5'),
					(newid(), 'CusOutturn', @C4Pk06, 'ADD', '2013-10-26', '2013-10-26', 'US6'),
					(newid(), 'CusOutturn', @C4Pk07, 'ADD', '2013-10-27', '2013-10-27', 'US7'),
					(newid(), 'CusOutturn', @C4Pk08, 'ADD', '2013-10-28', '2013-10-28', 'US8'),
					(newid(), 'CusOutturn', @C4Pk09, 'ADD', '2013-10-29', '2013-10-29', 'US9');

				INSERT dbo.EDIMessage (EM_PK, EM_GB, EM_GE, EM_LinkUniqueID, EM_LinkTable, EM_SystemCreateTimeUtc, EM_ReceiveTransmit, EM_MessageType, EM_MessageSubType, EM_Status, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES
					(newid(), @GbPkSg, @CsPkGe, @C6Pk01, 'CusOutturnHeader', '2013-09-01', 'TRX', 'AUT', 'ORG', 'SNT', 'US1', '2013-09-01', 'US1'), -- ignored, date out of range
					(newid(), @GbPkSg, @CsPkGe, @C6Pk01, 'CusOutturnHeader', '2013-10-01', 'TRX', 'AUT', 'ORG', 'SNT', 'US1', '2013-10-01', 'US1'), -- ignored, minimum date out of range
					(newid(), @GbPkSg, @CsPkGe, @C6Pk02, 'CusOutturnHeader', '2013-10-02', 'TRX', 'AUT', 'ORG', 'SNT', 'US2', '2013-10-02', 'US2'), -- CTN
					(newid(), @GbPkSg, @CsPkGe, @C6Pk03, 'CusOutturnHeader', '2013-10-13', 'TRX', 'AUT', 'ORG', 'SNT', 'US3', '2013-10-13', 'US3'), -- MBL
					(newid(), @GbPkSg, @CsPkGe, @C6Pk04, 'CusOutturnHeader', '2013-10-14', 'TRX', 'AUT', 'ORG', 'SNT', 'US4', '2013-10-14', 'US4'), -- HBL
					(newid(), @GbPkSg, @CsPkGe, @C6Pk05, 'CusOutturnHeader', '2013-10-15', 'TRX', 'AUT', 'ORG', 'SNT', 'US5', '2013-10-15', 'US5'), -- HAWB
					(newid(), @GbPkSg, @CsPkGe, @C6Pk06, 'CusOutturnHeader', '2013-10-26', 'TRX', 'AUT', 'ORG', 'SNT', 'US6', '2013-10-26', 'US6'), -- MAWB
					(newid(), @GbPkSg, @CsPkGe, @C6Pk07, 'CusOutturnHeader', '2013-10-27', 'TRX', 'SUT', 'ORG', 'SNT', 'US7', '2013-10-27', 'US7'), -- REF
					(newid(), @GbPkSg, @CsPkGe, @C6Pk08, 'CusOutturnHeader', '2013-10-28', 'TRX', 'SUT', 'ORG', 'QUE', 'US8', '2013-10-28', 'US8'), -- ignored, wrong status
					(newid(), @GbPkSg, @CsPkGe, @C6Pk08, 'CusOutturnHeader', '2013-10-28', 'TRX', 'XXX', 'ORG', 'SNT', 'US8', '2013-10-28', 'US8'), -- ignored, wrong type
					(newid(), @GbPkSg, @CsPkGe, @C6Pk08, 'CusOutturnHeader', '2013-10-28', 'TRX', 'SUT', 'XUS', 'SNT', 'US8', '2013-10-28', 'US8'), -- ignored, wrong subtype
					(newid(), @GbPkSg, @CsPkGe, @C6Pk08, 'CusOutturnHeader', '2013-10-28', 'TRX', 'AUT', 'XUS', 'SNT', 'US8', '2013-10-28', 'US8'), -- ignored, wrong subtype
					(newid(), @GbPkSg, @CsPkGe, @C6Pk08, 'CusOutturnHeader', '2013-10-28', 'RX' , 'AUT', 'ORG', 'SNT', 'US8', '2013-10-28', 'US8'), -- ignored, wrong mode
					(newid(), @GbPkGb, @CsPkGe, @C6Pk09, 'CusOutturnHeader', '2013-10-29', 'TRX', 'AUT', 'ORG', 'SNT', 'US9', '2013-10-29', 'US9'); -- ignored, company country is GB
				";

			var query = string.Format(sqlText, C5Pk01, C5Pk02, C5Pk03, C5Pk04, C5Pk05, C5Pk06, C5Pk07);
			TestConnection.ExecuteNonQuery(query);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 6, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "M2");
			AssertEquals("[T1] CompanyCode", "SIN", transaction1.GetCompanyCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2013, 10, 2), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] TransactionReference02", "CTN: CT2 CN02 MB02 HB02 2 OR2", transaction1.Reference2);
			AssertEquals("[T1] TransactionGuidReference", C5Pk02, Guid.Parse(transaction1.Reference5));
			AssertEquals("[T1] BranchCode", "SIN", transaction1.GetBranchCode());
			AssertEquals("[T1] UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);

			var transaction2 = FindRowByRef1(transactions, "M3");
			AssertEquals("[T2] CompanyCode", "SIN", transaction2.GetCompanyCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2013, 10, 13), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] TransactionReference02", "MBL: MB03 3 OR3", transaction2.Reference2);
			AssertEquals("[T2] TransactionGuidReference", C5Pk03, Guid.Parse(transaction2.Reference5));
			AssertEquals("[T2] BranchCode", "SIN", transaction2.GetBranchCode());
			AssertEquals("[T2] UserCode", "US3", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);

			var transaction3 = FindRowByRef1(transactions, "M4");
			AssertEquals("[T3] CompanyCode", "SIN", transaction3.GetCompanyCode());
			AssertEquals("[T3] TransactionDateUtc", new DateTime(2013, 10, 14), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] TransactionReference02", "HBL: HB04 4 OR4", transaction3.Reference2);
			AssertEquals("[T3] TransactionGuidReference", C5Pk04, Guid.Parse(transaction3.Reference5));
			AssertEquals("[T3] BranchCode", "SIN", transaction3.GetBranchCode());
			AssertEquals("[T3] UserCode", "US4", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);

			var transaction4 = FindRowByRef1(transactions, "M5");
			AssertEquals("[T4] CompanyCode", "SIN", transaction4.GetCompanyCode());
			AssertEquals("[T4] TransactionDateUtc", new DateTime(2013, 10, 15), transaction4.ServiceOccuredUTC);
			AssertEquals("[T4] TransactionReference02", "HAWB: HAWB2 5 OR5", transaction4.Reference2);
			AssertEquals("[T4] TransactionGuidReference", C5Pk05, Guid.Parse(transaction4.Reference5));
			AssertEquals("[T4] BranchCode", "SIN", transaction4.GetBranchCode());
			AssertEquals("[T4] UserCode", "US5", transaction4.ClientStaffCode);
			AssertEquals("[T4] ItemCount", 1, transaction4.BillableCount);

			var transaction5 = FindRowByRef1(transactions, "M6");
			AssertEquals("[T5] CompanyCode", "SIN", transaction5.GetCompanyCode());
			AssertEquals("[T5] TransactionDateUtc", new DateTime(2013, 10, 26), transaction5.ServiceOccuredUTC);
			AssertEquals("[T5] TransactionReference02", "MAWB: MAWB6 6 OR6", transaction5.Reference2);
			AssertEquals("[T5] TransactionGuidReference", C5Pk06, Guid.Parse(transaction5.Reference5));
			AssertEquals("[T5] BranchCode", "SIN", transaction5.GetBranchCode());
			AssertEquals("[T5] UserCode", "US6", transaction5.ClientStaffCode);
			AssertEquals("[T5] ItemCount", 1, transaction5.BillableCount);

			var transaction6 = FindRowByRef1(transactions, "M7");
			AssertEquals("[T6] CompanyCode", "SIN", transaction6.GetCompanyCode());
			AssertEquals("[T6] TransactionDateUtc", new DateTime(2013, 10, 27), transaction6.ServiceOccuredUTC);
			AssertEquals("[T6] TransactionReference02", "REF: M7 7 OR7", transaction6.Reference2);
			AssertEquals("[T6] TransactionGuidReference", C5Pk07, Guid.Parse(transaction6.Reference5));
			AssertEquals("[T6] BranchCode", "SIN", transaction6.GetBranchCode());
			AssertEquals("[T6] UserCode", "US7", transaction6.ClientStaffCode);
			AssertEquals("[T6] ItemCount", 1, transaction6.BillableCount);
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
