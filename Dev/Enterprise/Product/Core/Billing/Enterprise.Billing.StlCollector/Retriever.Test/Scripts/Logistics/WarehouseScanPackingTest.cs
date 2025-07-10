using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Billing.Collectors.Logistics;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(WarehouseScanPacking))]
	sealed class WarehouseScanPackingTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();

			var pkgPackageJob1 = new PkgPackageJob(Guid.NewGuid(), "KJ01", "JS") { KJ_SystemCreateUser = "US1", KJ_SystemCreateTimeUtc = new DateTime(2014, 11, 30) }.AppendInsertAndReturnObject(sql);
			var pkgPackageJob2 = new PkgPackageJob(Guid.NewGuid(), "KJ02", "JS") { KJ_SystemCreateUser = "US2", KJ_SystemCreateTimeUtc = new DateTime(2014, 10, 1) }.AppendInsertAndReturnObject(sql);
			var pkgPackageJob3 = new PkgPackageJob(Guid.NewGuid(), "KJ03", "JS") { KJ_SystemCreateUser = "~BP", KJ_SystemCreateTimeUtc = DateTime.UtcNow }.AppendInsertAndReturnObject(sql);

			new StmALog(pkgPackageJob1.PK, "PkgPackageJob", "ADD", new DateTime(2014, 11, 30)) { SL_EventTime = new DateTime(2014, 11, 30),  SL_GS_NKUser = "US1" }.AppendInsertAndReturnObject(sql);
			new StmALog(pkgPackageJob2.PK, "PkgPackageJob", "ADD", new DateTime(2014, 10, 1)) { SL_EventTime = new DateTime(2014, 10, 1), SL_GS_NKUser = "US2" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = transactions.First();
			AssertEquals("CompanyCode", null, transaction1.GetCompanyCode());
			AssertEquals("BranchCode", null, transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2014, 10, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "KJ02", transaction1.Reference1);
			AssertEquals("TransactionReference02", null, transaction1.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				DateTime startDate = new DateTime(2014, 10, 1);
				return new RecurringRange(startDate, startDate.AddDays(1));
			}
		}
	}
}
