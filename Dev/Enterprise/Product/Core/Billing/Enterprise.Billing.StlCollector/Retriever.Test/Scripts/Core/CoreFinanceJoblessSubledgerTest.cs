using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Core;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	[TestedType(typeof(CoreFinanceJoblessSubledger))]
	sealed class CoreFinanceJoblessSubledgerTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @AhPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @AhPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @JhPkAh UNIQUEIDENTIFIER = newid();
				DECLARE @JhPkAl UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
				DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP(1) GE_PK FROM dbo.GlbDepartment);
				DECLARE @OhPk UNIQUEIDENTIFIER = (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'DEMORG');
				INSERT dbo.JobHeader (JH_PK, JH_GC, JH_ParentID, JH_JobNum, JH_GB, JH_GE, JH_Status) VALUES
					(@JhPkAh, @GcPk, newid(), '01', @GbPk, @GePk, 'WRK'),
					(@JhPkAl, @GcPk, newid(), '02', @GbPk, @GePk, 'WRK');
				INSERT dbo.AccTransactionHeader (AH_PK, AH_JH, AH_GC, AH_GB, AH_GE, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_TransactionType, AH_Ledger, AH_InvoiceDate, AH_OH) VALUES
					(@AhPk01, null   , @GcPk, @GbPk, @GePk, 'NoJH+LineNoJH'      , '2014-07-01', 'US1', 'INV', 'AR', getutcdate(), null ),
					(@AhPk02, null   , @GcPk, @GbPk, @GePk, 'NoJH+LineJH'        , '2014-07-13', 'US2', 'CRD', 'AP', getutcdate(), null ),
					(newid(), null   , @GcPk, @GbPk, @GePk, 'NoJH+NoLine+DateOut', '2014-08-01', 'US3', 'ADJ', 'GL', getutcdate(), @OhPk),
					(newid(), @JhPkAh, @GcPk, @GbPk, @GePk, 'JH+NoLine'          , '2014-07-14', 'US4', 'INV', 'AR', getutcdate(), null ),
					(newid(), null   , @GcPk, @GbPk, @GePk, 'NoJH+NoLine+DateIn' , '2014-07-31', 'US5', 'ADJ', 'AP', getutcdate(), @OhPk);
				INSERT dbo.AccTransactionLines (AL_PK, AL_AH, AL_JH, AL_GC, AL_GB, AL_GE, AL_LineType) VALUES
					(newid(), @AhPk01, null   , @GcPk, @GbPk, @GePk, 'REV'),
					(newid(), @AhPk02, @JhPkAl, @GcPk, @GbPk, @GePk, 'CST');";
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
			AssertEquals("[T1] TransactionReference01", "NoJH+LineNoJH", transaction1.Reference1);
			AssertEquals("[T1] TransactionReference02", "AR INV #001 Org:<none>", transaction1.Reference2);

			var transaction2 = transactions.Single(t => t.ServiceOccuredUTC == new DateTime(2014, 7, 31));
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T2] UserCode", "US5", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference01", "NoJH+NoLine+DateIn", transaction2.Reference1);
			AssertEquals("[T2] TransactionReference02", "AP ADJ #001 Org:DEMORG", transaction2.Reference2);
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
