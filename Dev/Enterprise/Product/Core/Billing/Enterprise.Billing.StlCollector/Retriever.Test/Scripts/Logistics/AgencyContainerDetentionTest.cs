using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(AgencyContainerDetention))]
	sealed class AgencyContainerDetentionTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @OhPk UNIQUEIDENTIFIER = (SELECT TOP(1) OH_PK FROM dbo.OrgHeader);
				INSERT dbo.JobContainerDetention (NC_PK, NC_JobNumber, NC_GC, NC_OH_Principal, NC_OH_Client, NC_SystemCreateTimeUtc, NC_SystemCreateUser, NC_SystemLastEditTimeUtc, NC_SystemLastEditUser) VALUES
					(newid(), 'CD01', @GcPk, @OhPk, @OhPk, '2014-01-01', 'US1', '2014-01-01', 'US1'),
					(newid(), 'CD02', @GcPk, @OhPk, @OhPk, '2013-11-06', 'US2', '2013-11-06', 'US2'),
					(newid(), 'CD03', @GcPk, @OhPk, @OhPk, '2013-10-31', 'US3', '2013-10-31', 'US3');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = transactions.First();
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", null, transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2013, 11, 6), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "CD02", transaction1.Reference1);
			AssertEquals("TransactionReference02", null, transaction1.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2013, 11);
			}
		}
	}
}
