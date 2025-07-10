using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(EXLVExportAirSeaFreightManifestConsignments))]
	sealed class EXLVExportAirSeaFreightManifestConsignmentsTest : RefStlScriptWithDefaultsTest
	{
		readonly Guid DLPk01 = Guid.NewGuid();
		readonly Guid DLPk02 = Guid.NewGuid();
		readonly Guid DLPk03 = Guid.NewGuid();
		readonly Guid DLPk04 = Guid.NewGuid();
		readonly Guid DLPk05 = Guid.NewGuid();
		readonly Guid DLPk06 = Guid.NewGuid();
		readonly Guid DLPk07 = Guid.NewGuid();
		readonly Guid DLPk08 = Guid.NewGuid();
		readonly Guid DLPk09 = Guid.NewGuid();
		readonly Guid DLPk10 = Guid.NewGuid();
		readonly Guid DLPk11 = Guid.NewGuid();
		readonly Guid DLPk12 = Guid.NewGuid();

		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
			DECLARE @DLPk01 UNIQUEIDENTIFIER = '{0}';
			DECLARE @DLPk02 UNIQUEIDENTIFIER = '{1}';
			DECLARE @DLPk03 UNIQUEIDENTIFIER = '{2}';
			DECLARE @DLPk04 UNIQUEIDENTIFIER = '{3}';
			DECLARE @DLPk05 UNIQUEIDENTIFIER = '{4}';
			DECLARE @DLPk06 UNIQUEIDENTIFIER = '{5}';
			DECLARE @DLPk07 UNIQUEIDENTIFIER = '{6}';
			DECLARE @DLPk08 UNIQUEIDENTIFIER = '{7}';
			DECLARE @DLPk09 UNIQUEIDENTIFIER = '{8}';
			DECLARE @DLPk10 UNIQUEIDENTIFIER = '{9}';
			DECLARE @DLPk11 UNIQUEIDENTIFIER = '{10}';
			DECLARE @DLPk12 UNIQUEIDENTIFIER = '{11}';

			DECLARE @DhPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @OaPk UNIQUEIDENTIFIER = (SELECT TOP(1) OA_PK FROM dbo.OrgAddress);

			DECLARE @JkPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @JkPk02 UNIQUEIDENTIFIER = newid();
			DECLARE @JkPk03 UNIQUEIDENTIFIER = newid();
			DECLARE @JkPk04 UNIQUEIDENTIFIER = newid();

			DECLARE @JsPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk02 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk03 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk04 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk05 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk06 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk07 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk08 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk09 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk10 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk11 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk12 UNIQUEIDENTIFIER = newid();

			DECLARE @CePk01 UNIQUEIDENTIFIER = newid();
			DECLARE @CePk02 UNIQUEIDENTIFIER = newid();
			DECLARE @CePk03 UNIQUEIDENTIFIER = newid();
			DECLARE @CePk04 UNIQUEIDENTIFIER = newid();
			DECLARE @GcPk01 UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'EDI');
			DECLARE @GbPk01 UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPk01 ORDER BY GB_Code);
			DECLARE @CsPk01 UNIQUEIDENTIFIER = newid();
			INSERT dbo.GlbDepartment (GE_PK) VALUES (@CsPk01);

			INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef) VALUES (@JkPk01, 'con1');
			INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef) VALUES (@JkPk02, 'con2');
			INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef) VALUES (@JkPk03, 'con3');
			INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef) VALUES (@JkPk04, 'con4');	

			INSERT dbo.JobShipment (JS_PK, JS_ShipmentType, JS_RL_NKOrigin, JS_IsForwardRegistered, JS_UniqueConsignRef) VALUES 
				(@JsPk01, 'HLS', 'AUSYD', 1, 'Consign1'),
				(@JsPk02, 'STD', 'AUSYD', 1, 'Consign2'),
				(@JsPk03, 'HLS', 'USAAA', 1, 'Consign3'),
				(@JsPk04, 'HLS', 'AUSYD', 1, 'Consign4'),
				(@JsPk05, 'STD', 'AUSYD', 1, 'Consign5'),
				(@JsPk06, 'HLS', 'USAAA', 1, 'Consign6'),
				(@JsPk07, 'HLS', 'AUSYD', 1, 'Consign7'),
				(@JsPk08, 'STD', 'AUSYD', 1, 'Consign8'),
				(@JsPk09, 'HLS', 'USAAA', 1, 'Consign9'),
				(@JsPk10, 'HLS', 'AUSYD', 1, 'Consign10'),
				(@JsPk11, 'STD', 'AUSYD', 1, 'Consign11'),
				(@JsPk12, 'HLS', 'USAAA', 1, 'Consign12');

			INSERT dbo.JobConShipLink (JN_PK, JN_JK, JN_JS) VALUES 
				(newid(), @JkPk01, @JsPk01),
				(newid(), @JkPk01, @JsPk02),
				(newid(), @JkPk01, @JsPk03),
				(newid(), @JkPk02, @JsPk04),
				(newid(), @JkPk02, @JsPk05),
				(newid(), @JkPk02, @JsPk06),
				(newid(), @JkPk03, @JsPk07),
				(newid(), @JkPk03, @JsPk08),
				(newid(), @JkPk03, @JsPk09),
				(newid(), @JkPk04, @JsPk10),
				(newid(), @JkPk04, @JsPk11),
				(newid(), @JkPk04, @JsPk12);

			INSERT dbo.JobHeader (JH_PK, JH_GC, JH_ParentID, JH_ParentTableCode, JH_JobNum, JH_SystemCreateTimeUtc, JH_GB, JH_GE, JH_SystemCreateUser, JH_Status) VALUES
					(newid(), @GcPk01, @JsPk01, 'JS', 'Consign1', '2013-10-01', @GbPk01, @CsPk01, 'US1', 'WRK'),
					(newid(), @GcPk01, @JsPk02, 'JS', 'Consign2', '2013-10-01', @GbPk01, @CsPk01, 'US2', 'WRK'),
					(newid(), @GcPk01, @JsPk04, 'JS', 'Consign4', '2013-10-01', @GbPk01, @CsPk01, 'US4', 'WRK'),
					(newid(), @GcPk01, @JsPk07, 'JS', 'Consign7', '2013-10-01', @GbPk01, @CsPk01, 'US7', 'WRK'),
					(newid(), @GcPk01, @JsPk10, 'JS', 'Consign10', '2013-10-01', @GbPk01, @CsPk01, 'US9', 'WRK');

			INSERT dbo.CusEntryNum (CE_PK, CE_ParentTable, CE_ParentID, CE_EntryType, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES 
				(@CePk01, 'JobConsol', @JkPk01, 'CRN', '2013-10-01', '~BP', '2013-10-01', '~BP'),
				(@CePk02, 'JobConsol', @JkPk02, 'CRN', '2013-11-02', '~BP', '2013-11-02', '~BP'),
				(@CePk03, 'JobConsol', @JkPk03, 'AAA', '2013-10-03', '~BP', '2013-10-03', '~BP'),
				(@CePk04, 'JobConsol', @JkPk04, 'CRN', '2013-09-01', '~BP', '2013-09-01', '~BP');

			INSERT dbo.SupplierBookingHeader (DH_PK, DH_OA_Consignor, DH_SystemCreateTimeUtc, DH_SystemLastEditTimeUtc) VALUES (@DhPk01, @OaPk, '2013-10-01', '2013-10-01');

			INSERT dbo.SupplierBookingLine (DL_PK, DL_JS_ApprovedShipment, DL_ConsigneeReference, DL_SystemCreateUser, DL_DH_BookingHeader, DL_SystemCreateTimeUtc, DL_SystemLastEditTimeUtc) VALUES
				(@DLPk01, @JsPk01, 'Consignee1', 'US1', @DhPk01, '2013-10-01', '2013-10-01'),
				(@DLPk02, @JsPk02, 'Consignee2', 'US2', @DhPk01, '2013-10-01', '2013-10-01'),
				(@DLPk03, @JsPk03, 'Consignee3', 'US3', @DhPk01, '2013-10-01', '2013-10-01'),
				(@DLPk04, @JsPk04, 'Consignee4', 'US4', @DhPk01, '2013-10-01', '2013-10-01'),
				(@DLPk05, @JsPk05, 'Consignee5', 'US5', @DhPk01, '2013-10-01', '2013-10-01'),
				(@DLPk06, @JsPk06, 'Consignee6', 'US6', @DhPk01, '2013-10-01', '2013-10-01'),
				(@DLPk07, @JsPk07, 'Consignee7', 'US7', @DhPk01, '2013-10-01', '2013-10-01'),
				(@DLPk08, @JsPk08, 'Consignee8', 'US8', @DhPk01, '2013-10-01', '2013-10-01'),
				(@DLPk09, @JsPk09, 'Consignee9', 'US9', @DhPk01, '2013-10-01', '2013-10-01'),
				(@DLPk10, @JsPk10, 'Consignee10', 'US9', @DhPk01, '2013-10-01', '2013-10-01'),
				(@DLPk11, @JsPk11, 'Consignee11', 'US9', @DhPk01, '2013-10-01', '2013-10-01'),
				(@DLPk12, @JsPk12, 'Consignee12', 'US9', @DhPk01, '2013-10-01', '2013-10-01');

			INSERT dbo.EDIMessage (EM_PK, EM_GB, EM_GE, EM_LinkUniqueID, EM_LinkTable, EM_SystemCreateTimeUtc, EM_ApplicationCode, EM_ReceiveTransmit, EM_MessageType, EM_MessageSubType, EM_Status, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES
				(newid(), @GbPk01, @CsPk01, @JkPk01, 'JobConsol', '2013-10-01', 'CMR', 'TRX', 'ESM', 'ORG', 'SNT', 'US1', '2013-10-01', 'US1'), 
				(newid(), @GbPk01, @CsPk01, @JkPk01, 'JobConsol', '2013-10-12', 'CMR', 'TRX', 'ESM', 'ORG', 'SNT', 'US1', '2013-10-12', 'US1'),
				(newid(), @GbPk01, @CsPk01, @JkPk02, 'JobConsol', '2013-10-02', 'CMR', 'TRX', 'ESM', 'ORG', 'QUE', 'US2', '2013-10-02', 'US2'),
				(newid(), @GbPk01, @CsPk01, @JkPk03, 'JobConsol', '2013-10-13', 'CMR', 'TRX', 'UBM', 'ORG', 'SNT', 'US3', '2013-10-13', 'US3'), 
				(newid(), @GbPk01, @CsPk01, @JkPk04, 'JobConsol', '2013-10-14', 'CMR', 'TRX', 'ESM', 'XUS', 'SNT', 'US4', '2013-10-14', 'US4');
";
			var query = string.Format(sqlText, DLPk01, DLPk02, DLPk03, DLPk04, DLPk05, DLPk06, DLPk07, DLPk08, DLPk09, DLPk10, DLPk11, DLPk12);
			TestConnection.ExecuteNonQuery(query);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 4, transactions.Count());
			var transaction1 = FindRowByRef1(transactions, "Consignee1");
			AssertEquals("[T1] CompanyCode", "EDI", transaction1.GetCompanyCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2013, 10, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] TransactionReference02", "Consign1", transaction1.Reference2);
			AssertEquals("[T1] TransactionReference03", "con1", transaction1.Reference3);
			AssertEquals("[T1] TransactionReference04", "SEA", transaction1.Reference4);
			AssertEquals("[T1] TransactionGuidReference", DLPk01, Guid.Parse(transaction1.Reference5));
			AssertEquals("[T1] BranchCode", "BNE", transaction1.GetBranchCode());
			AssertEquals("[T1] UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);

			var transaction2 = FindRowByRef1(transactions, "Consignee4");
			AssertEquals("[T2] CompanyCode", "EDI", transaction2.GetCompanyCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2013, 10, 1), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] TransactionReference02", "Consign4", transaction2.Reference2);
			AssertEquals("[T2] TransactionReference03", "con2", transaction2.Reference3);
			AssertEquals("[T2] TransactionReference04", "SEA", transaction2.Reference4);
			AssertEquals("[T2] TransactionGuidReference", DLPk04, Guid.Parse(transaction2.Reference5));
			AssertEquals("[T2] BranchCode", "BNE", transaction2.GetBranchCode());
			AssertEquals("[T2] UserCode", "US4", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);

			var transaction3 = FindRowByRef1(transactions, "Consignee7");
			AssertEquals("[T3] CompanyCode", "EDI", transaction3.GetCompanyCode());
			AssertEquals("[T3] TransactionDateUtc", new DateTime(2013, 10, 1), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] TransactionReference02", "Consign7", transaction3.Reference2);
			AssertEquals("[T3] TransactionReference03", "con3", transaction3.Reference3);
			AssertEquals("[T3] TransactionReference04", "SEA", transaction3.Reference4);
			AssertEquals("[T3] TransactionGuidReference", DLPk07, Guid.Parse(transaction3.Reference5));
			AssertEquals("[T3] BranchCode", "BNE", transaction3.GetBranchCode());
			AssertEquals("[T3] UserCode", "US7", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);

			var transaction4 = FindRowByRef1(transactions, "Consignee10");
			AssertEquals("[T4] CompanyCode", "EDI", transaction4.GetCompanyCode());
			AssertEquals("[T4] TransactionDateUtc", new DateTime(2013, 10, 1), transaction4.ServiceOccuredUTC);
			AssertEquals("[T4] TransactionReference02", "Consign10", transaction4.Reference2);
			AssertEquals("[T4] TransactionReference03", "con4", transaction4.Reference3);
			AssertEquals("[T4] TransactionReference04", "SEA", transaction4.Reference4);
			AssertEquals("[T4] TransactionGuidReference", DLPk10, Guid.Parse(transaction4.Reference5));
			AssertEquals("[T4] BranchCode", "BNE", transaction4.GetBranchCode());
			AssertEquals("[T4] UserCode", "US9", transaction4.ClientStaffCode);
			AssertEquals("[T4] ItemCount", 1, transaction4.BillableCount);
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
