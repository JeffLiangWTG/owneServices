using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(HVLVCustomsDeclarationUsage))]
	sealed class HVLVCustomsDeclarationUsageTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sqlText = @"
				DECLARE @JePk1 UNIQUEIDENTIFIER = newid();
				DECLARE @JePk2 UNIQUEIDENTIFIER = newid();
				DECLARE @JePk3 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk1 UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk1 UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPk1);
				DECLARE @GcPk2 UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'EDI');
				DECLARE @GbPk2 UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPk2 AND GB_Code = 'SYD');

				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_SystemCreateTimeUtc, JE_SystemLastEditTimeUtc, JE_GB, JE_GC, JE_ClusterKey, JE_DeclarationReference) VALUES
					(@JePk1, 'AU', '2021-04-01', '2021-04-01', @GbPk1, @GcPk1, 2, 'Ref1'),
					(@JePk2, 'AU', '2021-04-02', '2021-04-02', @GbPk2, @GcPk2, 3, 'Ref2'),
					(@JePk3, 'AU', '2021-03-31', '2021-03-31', @GbPk1, @GcPk1, 4, 'Ref3');

				DECLARE @ClusterKey int = 24;
				DECLARE @BookingHeaderPK UNIQUEIDENTIFIER = newid();

				INSERT dbo.HVLVBookingHeader (HVH_PK, HVH_BookingReference, HVH_ClusterKey, HVH_OA_BillToParty, HVH_SystemCreateTimeUtc, HVH_SystemLastEditTimeUtc, HVH_SystemCreateUser, HVH_SystemLastEditUser) VALUES (@BookingHeaderPK, 'TESTBOOKINGHEADER', @ClusterKey, (Select Top 1 OA_PK from dbo.OrgAddress), '2019-03-29', '2019-03-29', 'BP~', 'BP~');
				INSERT dbo.HVLVConsignment (HVC_PK, HVC_ConsignmentId, HVC_HVH_BookingHeader, HVC_ClusterKey, HVC_Status, HVC_JE_ImportDeclaration, HVC_JE_ExportDeclaration, HVC_SystemCreateTimeUtc, HVC_SystemLastEditTimeUtc, HVC_SystemCreateUser, HVC_SystemLastEditUser) VALUES
					(newid(), 'TESTHVC001', @BookingHeaderPK, @ClusterKey, 'CLR', @JePk1, NULL, '2021-03-30', '2021-03-31', 'BP~', 'BP~'),
					(newid(), 'TESTHVC002', @BookingHeaderPK, @ClusterKey, 'CLR', @JePk2, NULL, '2021-03-30', '2021-03-31', 'BP~', 'BP~'),
					(newid(), 'TESTHVC003', @BookingHeaderPK, @ClusterKey, 'CLR', NULL, @JePk2, '2021-03-30', '2021-03-31', 'BP~', 'BP~'),
					(newid(), 'TESTHVC004', @BookingHeaderPK, @ClusterKey, 'CLR', NULL, @JePk3, '2021-03-30', '2021-03-31', 'BP~', 'BP~');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 3, transactions.Count());
				AssertRowMatchingRef1(transactions, "1", null, "DEM", new DateTime(2021, 4, 1), "", 1, "TESTHVC001");
				AssertRowMatchingRef1(transactions, "2", null, "SYD", new DateTime(2021, 4, 2), "", 1, "TESTHVC002");
				AssertRowMatchingRef1(transactions, "3", null, "SYD", new DateTime(2021, 4, 2), "", 1, "TESTHVC003");
			});
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2021, 4);
	}
}
