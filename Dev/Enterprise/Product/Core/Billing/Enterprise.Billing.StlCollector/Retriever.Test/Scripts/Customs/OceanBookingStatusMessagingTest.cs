using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(OceanBookingStatusMessaging))]
	sealed class OceanBookingStatusMessagingTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @JkPk UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPk);
				DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP (1) GE_PK FROM dbo.GlbDepartment);
				INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef) VALUES (@JkPk, 'JK01');
				INSERT dbo.EDIMessage (EM_PK, EM_LinkUniqueID, EM_LinkTable, EM_GB, EM_GE, EM_MessageNum, EM_ApplicationCode, EM_ReceiveTransmit, EM_SystemCreateTimeUtc, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES
					(newid(), newid(), 'CusUnderbond', @GbPk, @GePk, 'EM01', 'INT', 'TRX', '2014-07-01', 'US1', '2014-07-01', 'US1'),
					(newid(), @JkPk  , 'CusUnderbond', @GbPk, @GePk, 'EM02', 'INT', 'TRX', '2014-08-02', 'US2', '2014-08-02', 'US2'),
					(newid(), @JkPk  , 'CusUnderbond', @GbPk, @GePk, 'EM03', 'INT', 'XXX', '2014-07-03', 'US3', '2014-07-03', 'US3'),
					(newid(), @JkPk  , 'CusUnderbond', @GbPk, @GePk, 'EM04', 'XXX', 'TRX', '2014-07-04', 'US4', '2014-07-04', 'US4'),
					(newid(), @JkPk  , 'CusUnderbond', @GbPk, @GePk, 'EM05', 'INT', 'TRX', '2014-07-05', 'US5', '2014-07-05', 'US5');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());

			var transaction1 = transactions.First();
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 7, 5), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US5", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "JK01", transaction1.Reference1);
			AssertEquals("[T1] TransactionReference02", "EM05", transaction1.Reference2);
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
