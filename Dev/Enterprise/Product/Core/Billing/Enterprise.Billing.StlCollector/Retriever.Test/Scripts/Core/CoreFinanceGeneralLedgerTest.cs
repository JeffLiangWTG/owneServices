using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Core;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	[TestedType(typeof(CoreFinanceGeneralLedger))]
	sealed class CoreFinanceGeneralLedgerTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
				DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP(1) GE_PK FROM dbo.GlbDepartment);
				DECLARE @OhPk UNIQUEIDENTIFIER = (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'DEMORG');
				INSERT dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_TransactionType, AH_Ledger, AH_InvoiceDate, AH_OH) VALUES
					(newid(), @GcPk, @GbPk, @GePk, 'AH001', '2014-07-01', 'US1', 'GJL', 'GL', getutcdate(), @OhPk),
					(newid(), @GcPk, @GbPk, @GePk, 'AH002', '2013-12-01', 'US2', 'JNL', 'AR', getutcdate(), @OhPk),
					(newid(), @GcPk, @GbPk, @GePk, 'AH003', '2014-08-01', 'US3', 'OVP', 'AP', getutcdate(), null ),
					(newid(), @GcPk, @GbPk, @GePk, 'AH004', '2014-07-14', 'US4', 'GJL', 'AR', getutcdate(), @OhPk),
					(newid(), @GcPk, @GbPk, @GePk, 'AH005', '2014-07-31', 'US5', 'EXX', 'AP', getutcdate(), null );";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = transactions.Single(t => t.ServiceOccuredUTC == new DateTime(2014, 7, 1));
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "AH001", transaction1.Reference1);
			AssertEquals("[T1] TransactionReference02", "GL GJL #001 Org:DEMORG", transaction1.Reference2);

			var transaction2 = transactions.Single(t => t.ServiceOccuredUTC == new DateTime(2014, 7, 31));
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T1] UserCode", "US5", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference01", "AH005", transaction2.Reference1);
			AssertEquals("[T2] TransactionReference02", "AP EXX #001 Org:<none>", transaction2.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 7);
			}
		}
	}
}
