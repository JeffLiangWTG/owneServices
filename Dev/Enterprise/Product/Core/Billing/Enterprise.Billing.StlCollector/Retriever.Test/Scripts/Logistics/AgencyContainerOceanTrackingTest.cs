using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(AgencyContainerOceanTracking))]
	sealed class AgencyContainerOceanTrackingTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @E9Pk UNIQUEIDENTIFIER = newid();
				DECLARE @RcsPk UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPk);
				DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP (1) GE_PK FROM dbo.GlbDepartment);
				DECLARE @RcPk UNIQUEIDENTIFIER = (SELECT TOP(1) RC_PK FROM dbo.RefContainer);
				INSERT dbo.RefContainerStock (R6_PK, R6_ContainerNum, R6_RC, R6_OwnerType) VALUES (@RcsPk, 'RCS01', @RcPk, 'CAR');
				INSERT dbo.JobContainerMove (E9_PK, E9_MovementType, E9_R6, E9_MovementDate) VALUES (@E9Pk, 'MT1', @RcsPk, '2016-05-01');
				INSERT dbo.EDIMessage (EM_PK, EM_LinkUniqueID, EM_LinkTable, EM_GB, EM_GE, EM_MessageNum, EM_ApplicationCode, EM_ReceiveTransmit, EM_Status, EM_IsActive, EM_SystemCreateTimeUtc, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES
					(newid(), newid(), 'EDIMessage', @GbPk, @GePk, 'EM01', 'CMG', 'RCV', 'RKN', 1, '2016-05-01', 'US1', '2016-05-01', 'US1'),
					(newid(), @E9Pk  , 'EDIMessage', @GbPk, @GePk, 'EM02', 'CMG', 'RCV', 'RKN', 1, '2016-06-02', 'US2', '2016-06-02', 'US2'),
					(newid(), @E9Pk  , 'EDIMessage', @GbPk, @GePk, 'EM03', 'CMG', 'TRX', 'RKN', 1, '2016-05-03', 'US3', '2016-05-03', 'US3'),
					(newid(), @E9Pk  , 'EDIMessage', @GbPk, @GePk, 'EM04', 'XXX', 'RCV', 'RKN', 1, '2016-05-04', 'US4', '2016-05-04', 'US4'),
					(newid(), @E9Pk  , 'EDIMessage', @GbPk, @GePk, 'EM05', 'CMG', 'RCV', 'XXX', 1, '2016-05-05', 'US5', '2016-05-05', 'US5'),
					(newid(), @E9Pk  , 'EDIMessage', @GbPk, @GePk, 'EM06', 'CMG', 'RCV', 'RKN', 1, '2016-05-06', 'US6', '2016-05-06', 'US6'),
					(newid(), @E9Pk  , 'EDIMessage', @GbPk, @GePk, 'EM07', 'CMG', 'RCV', 'RKN', 0, '2016-05-07', 'US7', '2016-05-07', 'US7');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());

			var transaction1 = transactions.First();
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2016, 5, 6), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US6", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "MSG#: EM06", transaction1.Reference1);
			AssertEquals("[T1] TransactionReference02", "MT1", transaction1.Reference2);
			AssertEquals("[T1] TransactionReference03", new DateTime(2016, 5, 1), Convert.ToDateTime(transaction1.Reference3));
			AssertEquals("[T1] TransactionReference04", "RCS01", transaction1.Reference4);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2016, 5);
			}
		}
	}
}
