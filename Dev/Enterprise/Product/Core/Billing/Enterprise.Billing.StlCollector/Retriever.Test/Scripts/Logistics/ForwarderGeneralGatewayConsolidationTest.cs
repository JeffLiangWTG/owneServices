using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwarderGeneralGatewayConsolidation))]
	sealed class ForwarderGeneralGatewayConsolidationTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @JkPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @JkPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @JkPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @JkPk05 UNIQUEIDENTIFIER = newid();
				DECLARE @JkPk06 UNIQUEIDENTIFIER = newid();
				DECLARE @JkPk07 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
				DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP(1) GE_PK FROM dbo.GlbDepartment);
				INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_IsForwarding, JK_AgentType, JK_SendingForwarderHandlingType) VALUES
					(@JkPk01, 'JK01', 0, 'AGT', 'GTA'),
					(@JkPk02, 'JK02', 1, 'AGT', 'GTA'),
					(@JkPk03, 'JK03', 1, 'XXX', ''),
					(newid(), 'JK04', 1, 'AGT', 'GTA'),
					(@JkPk05, 'JK05', 1, 'CLD', 'GTA'),
					(@JkPk06, 'JK06', 1, 'CLA', 'GTA'),
					(@JkPk07, 'JK07', 1, 'DRT', 'GTA');
				INSERT dbo.JobHeader (JH_PK, JH_GC, JH_ParentID, JH_JobNum, JH_SystemCreateTimeUtc, JH_SystemCreateUser, JH_GB, JH_GE, JH_Status) VALUES
					(newid(), @GcPk, @JkPk01, 'JH01', '2013-12-01', 'US1', @GbPk, @GePk, 'WRK'),
					(newid(), @GcPk, @JkPk02, 'JH02', '2013-12-02', 'US2', @GbPk, @GePk, 'WRK'),
					(newid(), @GcPk, @JkPk03, 'JH03', '2013-12-03', 'US3', @GbPk, @GePk, 'WRK'),
					(newid(), @GcPk, @JkPk05, 'JH05', '2013-12-05', 'US5', @GbPk, @GePk, 'WRK'),
					(newid(), @GcPk, @JkPk06, 'JH06', '2013-12-06', 'US6', @GbPk, @GePk, 'WRK'),
					(newid(), @GcPk, @JkPk07, 'JH07', '2013-12-07', 'US7', @GbPk, @GePk, 'WRK');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 4, transactions.Count());
			var transaction1 = FindRowByRef1(transactions, "JH02");
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2013, 12, 2), transaction1.ServiceOccuredUTC);

			AssertEquals("UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference02", "JK02", transaction1.Reference2);

			var transaction2 = FindRowByRef1(transactions, "JH05");
			AssertEquals("UserCode", "US5", transaction2.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction2.BillableCount);
			AssertEquals("TransactionReference02", "JK05", transaction2.Reference2);

			var transaction3 = FindRowByRef1(transactions, "JH06");
			AssertEquals("UserCode", "US6", transaction3.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction3.BillableCount);
			AssertEquals("TransactionReference02", "JK06", transaction3.Reference2);

			var transaction4 = FindRowByRef1(transactions, "JH07");
			AssertEquals("UserCode", "US7", transaction4.ClientStaffCode);
			AssertEquals("ItemCount", 1, Convert.ToInt32(transaction4.BillableCount));
			AssertEquals("TransactionReference02", "JK07", transaction4.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2013, 12);
			}
		}
	}
}
