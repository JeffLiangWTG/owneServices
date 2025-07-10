using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(CommunicationSpecialEntryUnderbond))]
	sealed class CommunicationSpecialEntryUnderbondTest : RefStlScriptWithDefaultsTest
	{
		readonly Guid C4Pk01 = Guid.NewGuid();
		readonly Guid C4Pk02 = Guid.NewGuid();
		readonly Guid C4Pk03 = Guid.NewGuid();
		readonly Guid C4Pk04 = Guid.NewGuid();
		readonly Guid C4Pk05 = Guid.NewGuid();

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @C4Pk01 UNIQUEIDENTIFIER =  '{0}';
				DECLARE @C4Pk02 UNIQUEIDENTIFIER =  '{1}';
				DECLARE @C4Pk03 UNIQUEIDENTIFIER =  '{2}';
				DECLARE @C4Pk04 UNIQUEIDENTIFIER =  '{3}';
				DECLARE @C4Pk05 UNIQUEIDENTIFIER =  '{4}';
				DECLARE @C4Pk06 UNIQUEIDENTIFIER = newid();
				DECLARE @C4Pk07 UNIQUEIDENTIFIER = newid();
				DECLARE @C4Pk08 UNIQUEIDENTIFIER = newid();

				DECLARE @CaPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @CvPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @CbPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @CnPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @CjPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @CxPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @CxPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @CxPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @CxPk04 UNIQUEIDENTIFIER = newid();

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
				INSERT dbo.CusMawb (CM_PK, CM_GB, CM_MAWB) VALUES
					(@CmPkGb, @GbPkGb, 'MAWB1'),
					(@CmPkSg, @GbPkSg, 'MAWB2');
				INSERT dbo.CusHawb (CS_PK, CS_CM, CS_HAWB) VALUES
					(@CsPkGb, @CmPkGb, 'HAWB1'),
					(@CsPkSg, @CmPkSg, 'HAWB2');

				INSERT dbo.CusSCAOceanBill(CB_PK, CB_SystemCreateTimeUtc, CB_SystemCreateUser, CB_SystemLastEditTimeUtc, CB_SystemLastEditUser) VALUES (@CbPk01, GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.CusSCAContainer(CN_PK, CN_CB, CN_ContainerNumber, CN_SystemCreateTimeUtc, CN_SystemCreateUser, CN_SystemLastEditTimeUtc, CN_SystemLastEditUser) VALUES
					(@CnPk01, @CbPk01, 'C1', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.CusSCADepotContainer(CJ_PK, CJ_ContainerNumber, CJ_SystemCreateTimeUtc, CJ_SystemCreateUser, CJ_SystemLastEditTimeUtc, CJ_SystemLastEditUser) VALUES
					(@CjPk01, 'DC1', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.CusSCAHouse(CA_PK, CA_HouseBill, CA_SystemCreateTimeUtc, CA_SystemCreateUser, CA_SystemLastEditTimeUtc, CA_SystemLastEditUser) VALUES
					(@CaPk01, 'HB1', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.CusSCADepotHouse(CX_PK, CX_HouseBill, CX_SystemCreateTimeUtc, CX_SystemCreateUser, CX_SystemLastEditTimeUtc, CX_SystemLastEditUser) VALUES
					(@CxPk01, 'DHB1', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@CxPk02, 'DHB2', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@CxPk03, 'DHB3', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(@CxPk04, 'DHB4', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.CusSCAPivot(CV_PK, CV_CA, CV_SystemCreateTimeUtc, CV_SystemCreateUser, CV_SystemLastEditTimeUtc, CV_SystemLastEditUser) VALUES
					(@CvPk01, @CaPk01, GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT dbo.CusUnderbond (c4_PK, C4_ParentID, C4_MAWB, C4_SendersMessageReference, C4_ParentTableCode, C4_MovementReason, C4_DestinationPremiseID, C4_PiecesManifested, C4_PackageType, C4_ApplicationCode) VALUES
					(@C4Pk01, @CmPkSg, 'MAWB1', 'MSG1', 'CM', 'MV1', 111, 1, 'P1', 'AUU'),
					(@C4Pk02, @CnPk01, 'MAWB2', 'MSG2', 'CN', 'MV2', 222, 2, 'P2', 'AUU'),
					(@C4Pk03, @CjPk01, 'MAWB3', 'MSG3', 'CJ', 'MV3', 333, 3, 'P3', 'AUU'),
					(@C4Pk04, @CxPk01, 'MAWB4', 'MSG4', 'CX', 'MV4', 444, 4, 'P4', 'AUU'),
					(@C4Pk05, @CvPk01, 'MAWB5', 'MSG5', 'CV', 'MV5', 555, 5, 'P5', 'AUU'),
					(@C4Pk06, @CxPk03, 'MAWB6', 'MSG6', 'CX', 'MV6', 666, 6, 'P6', 'AUU'),
					(@C4Pk07, @CxPk03, 'MAWB7', 'MSG7', 'CX', 'MV7', 777, 7, 'P7', 'AUU'),
					(@C4Pk08, @CxPk04, 'MAWB8', 'MSG8', 'CX', 'MV8', 888, 8, 'P8', 'AUU');

				INSERT dbo.EDIMessage (EM_PK, EM_GB, EM_GE, EM_LinkUniqueID, EM_LinkTable, EM_SystemCreateTimeUtc, EM_ReceiveTransmit, EM_MessageType, EM_MessageSubType, EM_Status, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES
					(newid(), @GbPkSg, @CsPkGe, @C4Pk01, 'CusUnderbond', '2013-10-01', 'TRX', 'UBM', 'ORG', 'SNT', 'US1', '2013-10-01', 'US1'), -- CusMAWB
					(newid(), @GbPkSg, @CsPkGe, @C4Pk02, 'CusUnderbond', '2013-10-02', 'TRX', 'UBM', 'ORG', 'SNT', 'US2', '2013-10-02', 'US2'), -- CusSCAContainer
					(newid(), @GbPkSg, @CsPkGe, @C4Pk03, 'CusUnderbond', '2013-10-13', 'TRX', 'UBM', 'ORG', 'SNT', 'US3', '2013-10-13', 'US3'), -- CusSCADepotContainer
					(newid(), @GbPkSg, @CsPkGe, @C4Pk04, 'CusUnderbond', '2013-10-14', 'TRX', 'UBM', 'ORG', 'SNT', 'US4', '2013-10-14', 'US4'), -- CusSCADepotHouse
					(newid(), @GbPkSg, @CsPkGe, @C4Pk05, 'CusUnderbond', '2013-10-15', 'TRX', 'UBM', 'ORG', 'SNT', 'US5', '2013-10-15', 'US5'), -- Pivot (CusSCAHouse)
					(newid(), @GbPkGb, @CsPkGe, @C4Pk06, 'CusUnderbond', '2013-10-26', 'TRX', 'UBM', 'ORG', 'SNT', 'US6', '2013-10-26', 'US6'), -- ignored, company country is GB
					(newid(), @GbPkSg, @CsPkGe, @C4Pk07, 'CusUnderbond', '2013-09-27', 'TRX', 'UBM', 'ORG', 'SNT', 'US7', '2013-09-27', 'US7'), -- ignored, date out of range
					(newid(), @GbPkSg, @CsPkGe, @C4Pk07, 'CusUnderbond', '2013-10-27', 'TRX', 'UBM', 'ORG', 'SNT', 'US7', '2013-10-27', 'US7'), -- ignored, minimum date out of range
					(newid(), @GbPkSg, @CsPkGe, @C4Pk08, 'CusUnderbond', '2013-10-28', 'TRX', 'SUT', 'ORG', 'QUE', 'US8', '2013-10-28', 'US8'), -- ignored, wrong status
					(newid(), @GbPkSg, @CsPkGe, @C4Pk08, 'CusUnderbond', '2013-10-28', 'TRX', 'XXX', 'ORG', 'SNT', 'US8', '2013-10-28', 'US8'), -- ignored, wrong type
					(newid(), @GbPkSg, @CsPkGe, @C4Pk08, 'CusUnderbond', '2013-10-28', 'TRX', 'SUT', 'XUS', 'SNT', 'US8', '2013-10-28', 'US8'), -- ignored, wrong subtype
					(newid(), @GbPkSg, @CsPkGe, @C4Pk08, 'CusUnderbond', '2013-10-28', 'TRX', 'AUT', 'XUS', 'SNT', 'US8', '2013-10-28', 'US8'), -- ignored, wrong subtype
					(newid(), @GbPkSg, @CsPkGe, @C4Pk08, 'CusUnderbond', '2013-10-28', 'RX' , 'AUT', 'ORG', 'SNT', 'US8', '2013-10-28', 'US8'); -- ignored, wrong mode
				";

			var query = string.Format(sqlText, C4Pk01, C4Pk02, C4Pk03, C4Pk04, C4Pk05);
			TestConnection.ExecuteNonQuery(query);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 5, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "MSG1");
			AssertEquals("[T1] CompanyCode", "SIN", transaction1.GetCompanyCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2013, 10, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] TransactionReference02", "MV1 111 MAWB: MAWB2", transaction1.Reference2);
			AssertEquals("[T1] TransactionGuidReference", C4Pk01, Guid.Parse(transaction1.Reference5));
			AssertEquals("[T1] BranchCode", "SIN", transaction1.GetBranchCode());
			AssertEquals("[T1] UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);

			var transaction2 = FindRowByRef1(transactions, "MSG2");
			AssertEquals("[T2] CompanyCode", "SIN", transaction2.GetCompanyCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2013, 10, 2), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] TransactionReference02", "MV2 222 CNT: C1", transaction2.Reference2);
			AssertEquals("[T2] TransactionGuidReference", C4Pk02, Guid.Parse(transaction2.Reference5));
			AssertEquals("[T2] BranchCode", "SIN", transaction2.GetBranchCode());
			AssertEquals("[T2] UserCode", "US2", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);

			var transaction3 = FindRowByRef1(transactions, "MSG3");
			AssertEquals("[T3] CompanyCode", "SIN", transaction3.GetCompanyCode());
			AssertEquals("[T3] TransactionDateUtc", new DateTime(2013, 10, 13), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] TransactionReference02", "MV3 333 CNT: DC1", transaction3.Reference2);
			AssertEquals("[T3] TransactionGuidReference", C4Pk03, Guid.Parse(transaction3.Reference5));
			AssertEquals("[T3] BranchCode", "SIN", transaction3.GetBranchCode());
			AssertEquals("[T3] UserCode", "US3", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);

			var transaction4 = FindRowByRef1(transactions, "MSG4");
			AssertEquals("[T4] CompanyCode", "SIN", transaction4.GetCompanyCode());
			AssertEquals("[T4] TransactionDateUtc", new DateTime(2013, 10, 14), transaction4.ServiceOccuredUTC);
			AssertEquals("[T4] TransactionReference02", "MV4 444 House: DHB1", transaction4.Reference2);
			AssertEquals("[T4] TransactionGuidReference", C4Pk04, Guid.Parse(transaction4.Reference5));
			AssertEquals("[T4] BranchCode", "SIN", transaction4.GetBranchCode());
			AssertEquals("[T4] UserCode", "US4", transaction4.ClientStaffCode);
			AssertEquals("[T4] ItemCount", 1, transaction4.BillableCount);

			var transaction5 = FindRowByRef1(transactions, "MSG5");
			AssertEquals("[T5] CompanyCode", "SIN", transaction5.GetCompanyCode());
			AssertEquals("[T5] TransactionDateUtc", new DateTime(2013, 10, 15), transaction5.ServiceOccuredUTC);
			AssertEquals("[T5] TransactionReference02", "MV5 555 House: HB1 5 P5", transaction5.Reference2);
			AssertEquals("[T5] TransactionGuidReference", C4Pk05, Guid.Parse(transaction5.Reference5));
			AssertEquals("[T5] BranchCode", "SIN", transaction5.GetBranchCode());
			AssertEquals("[T5] UserCode", "US5", transaction5.ClientStaffCode);
			AssertEquals("[T5] ItemCount", 1, transaction5.BillableCount);
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
