using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.Aggregator.Test;
using Enterprise.Accounting.Export.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.Business.Testing
{
	public class ControlAccountsTest : TestCaseWithFactory
	{
		public void TestSetupControlAccounts()
		{
			var testHelper = new BatchTestHelper(Factory);
			TestBatchAggregator testAggregator = new TestBatchAggregator(testHelper);
			testHelper.SetControlAccounts();
			testAggregator.SetControlAccount(AccountingUtils.ARControl, Guid.Empty);
			testAggregator.SetControlAccount(AccountingUtils.ARSuspenseControlAccount, Guid.Empty);
			testAggregator.SetControlAccount(AccountingUtils.APSuspenseControlAccount, Guid.Empty);
			testAggregator.SetControlAccount(AccountingUtils.JobRevenueJournalControlAccount, Guid.Empty);

			var dataAccess = new BatchExportDataAccess(((IDbConnectionInternals)Db.Connection).ADOConnection, ((IDbConnectionInternals)Db.Connection).ADOTransaction);
			string companyCode = GlbCompany.CurrentCompany.GC_Code;
			AssertExceptionThrown("No setup Control Accounts.", typeof(DataObjectValidationException),
@"Please set up the control account(s) in the registry Accounting > General Ledger Defaults > Control Account: 

- AR Control Account.
- AR Suspense Control Account.
- AP Suspense Control Account.
- Job Revenue Journal Control Account.", () => new ControlAccounts(dataAccess, companyCode));

			testAggregator.SetControlAccount(AccountingUtils.ARControl, testHelper.ARControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.ARSuspenseControlAccount, testHelper.ARSuspenseControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APSuspenseControlAccount, testHelper.APSuspenseControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.JobRevenueJournalControlAccount, testHelper.JobRevenueJournalControlAccount);
			AssertNoExceptionThrown("setup done Control Accounts.", () => new ControlAccounts(dataAccess, companyCode));
		}
	}
}
