using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Core;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	[TestedType(typeof(CoreFinancePayableOrder))]
	sealed class CoreFinancePayableOrderTest : RefStlScriptWithDefaultsTest
	{
		public void TestCannotInsertBlankOrderNumber()
		{
			const string sqlText = @"
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @OaPk UNIQUEIDENTIFIER = (SELECT TOP(1) OA_PK FROM dbo.OrgAddress);

				INSERT dbo.AccPayableOrderHeader (APH_PK, APH_GC, APH_OrderNumber, APH_OA_Buyer, APH_SystemCreateTimeUtc, APH_SystemCreateUser, APH_Disposition, APH_GoodsReceivedStatus, APH_Stage, APH_Type) VALUES
					(newid(), @GcPk, null, @OaPk, '2015-01-05', 'US1', 'DSP', 'RCV', 'STH', 'BPE')";

			AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery(sqlText));
		}

		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			const string sqlText = @"
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @OaPk UNIQUEIDENTIFIER = (SELECT TOP(1) OA_PK FROM dbo.OrgAddress);

				INSERT dbo.AccPayableOrderHeader (APH_PK, APH_GC, APH_OrderNumber, APH_OA_Buyer, APH_SystemCreateTimeUtc, APH_SystemCreateUser, APH_Disposition, APH_GoodsReceivedStatus, APH_Stage, APH_Type) VALUES
					(newid(), @GcPk, 'APH001', @OaPk, '2015-01-05', 'US1', 'DSP', 'RCV', 'STH', 'BPE'),
					(newid(), @GcPk, 'APH002', @OaPk, '2015-01-25', 'US1', 'DSP', 'RCV', 'STH', 'BPE'),
					(newid(), @GcPk, 'APH003', @OaPk, '2015-02-05', 'US1', 'DSP', 'RCV', 'STH', 'BPE'),
					(newid(), @GcPk, 'APH004', @OaPk, '2014-01-05', 'US1', 'DSP', 'RCV', 'STH', 'BPE'),
					(newid(), @GcPk, 'APH005', @OaPk, '2016-04-05', 'US1', 'DSP', 'RCV', 'STH', 'BPE')";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Payable Orders", 2, transactions.Count());

			var transaction1 = transactions.Single(t => t.ServiceOccuredUTC == new DateTime(2015, 1, 5));
			AssertEquals("[Order1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[Order1] UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("[Order1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[Order1] TransactionReference01", "APH001", transaction1.Reference1);

			var transaction2 = transactions.Single(t => t.ServiceOccuredUTC == new DateTime(2015, 1, 25));
			AssertEquals("[Order2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[Order2] UserCode", "US1", transaction2.ClientStaffCode);
			AssertEquals("[Order2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[Order2] TransactionReference01", "APH002", transaction2.Reference1);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2015, 1);
			}
		}
	}
}
