using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(HVLVImportAirFreightManifestConsignments))]
	sealed class HVLVImportAirFreightManifestConsignmentsTest : RefStlScriptWithDefaultsTest
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

			DECLARE @CmPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @CmPk02 UNIQUEIDENTIFIER = newid();
			DECLARE @CmPk03 UNIQUEIDENTIFIER = newid();
			DECLARE @CmPk04 UNIQUEIDENTIFIER = newid();
			DECLARE @GcPk01 UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'EDI');
			DECLARE @GbPk01 UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPk01 ORDER BY GB_Code);
			DECLARE @CsPk01 UNIQUEIDENTIFIER = newid();
			INSERT dbo.GlbDepartment (GE_PK) VALUES (@CsPk01);

			INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef) VALUES (@JkPk01, 'con1');
			INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef) VALUES (@JkPk02, 'con2');
			INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef) VALUES (@JkPk03, 'con3');
			INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef) VALUES (@JkPk04, 'con4');

			INSERT dbo.JobShipment (JS_PK, JS_ShipmentType, JS_TransportMode, JS_RL_NKDestination, JS_IsForwardRegistered, JS_UniqueConsignRef) VALUES 
				(@JsPk01, 'HLS', 'AIR', 'AUSYD', 1, 'Consign1'),
				(@JsPk02, 'STD', 'AIR', 'AUSYD', 1, 'Consign2'),
				(@JsPk03, 'HLS', 'AIR', 'USAAA', 1, 'Consign3'),
				(@JsPk04, 'HLS', 'SEA', 'AUSYD', 1, 'Consign4'),
				(@JsPk05, 'STD', 'AIR', 'AUSYD', 1, 'Consign5'),
				(@JsPk06, 'HLS', 'AIR', 'USAAA', 1, 'Consign6'),
				(@JsPk07, 'HLS', 'SEA', 'AUSYD', 1, 'Consign7'),
				(@JsPk08, 'STD', 'SEA', 'AUSYD', 1, 'Consign8'),
				(@JsPk09, 'HLS', 'AIR', 'USAAA', 1, 'Consign9'),
				(@JsPk10, 'HLS', 'AIR', 'AUSYD', 1, 'Consign10'),
				(@JsPk11, 'STD', 'AIR', 'AUSYD', 1, 'Consign11'),
				(@JsPk12, 'HLS', 'AIR', 'USAAA', 1, 'Consign12');

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

			INSERT dbo.CusMawb (CM_PK, CM_GB, CM_ApplicationCode, CM_IsActive, CM_SystemCreateTimeUtc, CM_JK) VALUES 
				(@CmPk01, @GbPk01, 'CMR', 1, '2013-10-01', @JkPk01),
				(@CmPk02, @GbPk01, 'AAA', 1, '2013-10-02', @JkPk02),
				(@CmPk03, @GbPk01, 'CMR', 0, '2013-10-03', @JkPk03),
				(@CmPk04, @GbPk01, 'CMR', 0, '2013-09-02', @JkPk04);

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
";
			var query = string.Format(sqlText, DLPk01, DLPk02, DLPk03, DLPk04, DLPk05, DLPk06, DLPk07, DLPk08, DLPk09, DLPk10, DLPk11, DLPk12);
			TestConnection.ExecuteNonQuery(query);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());
			var transaction1 = FindRowByRef1(transactions, "Consignee1");
			AssertEquals("[T1] CompanyCode", "EDI", transaction1.GetCompanyCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2013, 10, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] TransactionReference02", "Consign1", transaction1.Reference2);
			AssertEquals("[T1] TransactionReference03", "con1", transaction1.Reference3);
			AssertEquals("[T1] TransactionReference04", "AIR", transaction1.Reference4);
			AssertEquals("[T1] TransactionGuidReference", DLPk01, Guid.Parse(transaction1.Reference5));
			AssertEquals("[T1] BranchCode", "BNE", transaction1.GetBranchCode());
			AssertEquals("[T1] UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);

			var transaction2 = FindRowByRef1(transactions, "Consignee10");
			AssertEquals("[T2] CompanyCode", "EDI", transaction2.GetCompanyCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2013, 10, 1), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] TransactionReference02", "Consign10", transaction2.Reference2);
			AssertEquals("[T2] TransactionReference03", "con4", transaction2.Reference3);
			AssertEquals("[T2] TransactionReference04", "AIR", transaction2.Reference4);
			AssertEquals("[T2] TransactionGuidReference", DLPk10, Guid.Parse(transaction2.Reference5));
			AssertEquals("[T2] BranchCode", "BNE", transaction2.GetBranchCode());
			AssertEquals("[T2] UserCode", "US9", transaction2.ClientStaffCode);
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
