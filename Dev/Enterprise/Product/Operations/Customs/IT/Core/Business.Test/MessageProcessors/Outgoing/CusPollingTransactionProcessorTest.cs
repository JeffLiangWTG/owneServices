using System;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CusPollingTransactionProcessorTest : TestCaseWithFactory
{
	[TestDate]
	public void TestExecuteBatch_DeleteExpiredIvistoCusPollingTransactions()
	{
		var thresholdDate = ZDateTime.UtcNow.AddDays(-240);

		var helper = new CusPollingTransactionTestDataHelper(Factory);
		var ivistoRequestInterchange = helper.CreateIvistoRequestInterchange();
		var pollingTransaction = helper.CreateIvistoPollingTransaction("OPN", ZDateTime.UtcNow, ivistoRequestInterchange, thresholdDate.AddDays(-1));
		Factory.Save();

		var logger = new LoggingInformation();
		new CusPollingTransactionProcessor(logger).ExecuteBatch();

		CombineAssertions(() =>
		{
			AssertEquals("CusPollingTransaction IsDeleted", true, pollingTransaction.IsDeleted);
			AssertEquals("EDIInterchange EI_Status", "SNT", ivistoRequestInterchange.EI_Status);
			AssertCollectionContains("Log", "\t1 CusPollingTransaction(s) have been deleted due to expiration.", logger.UserLogStrings);
		});
	}

	[TestDate]
	public void TestExecuteBatch_DeleteExpiredIrildesCusPollingTransactions()
	{
		var thresholdDate = ZDateTime.UtcNow.AddDays(-240);

		var helper = new CusPollingTransactionTestDataHelper(Factory);
		var irildesRequestInterchange = helper.CreateIrildesRequestInterchange();
		var pollingTransaction = helper.CreateIrildesPollingTransaction("OPN", ZDateTime.UtcNow, irildesRequestInterchange, thresholdDate.AddDays(-1));
		Factory.Save();

		var logger = new LoggingInformation();
		new CusPollingTransactionProcessor(logger).ExecuteBatch();

		CombineAssertions(() =>
		{
			AssertEquals("CusPollingTransaction IsDeleted", true, pollingTransaction.IsDeleted);
			AssertEquals("EDIInterchange EI_Status", "SNT", irildesRequestInterchange.EI_Status);
			AssertCollectionContains("Log", "\t1 CusPollingTransaction(s) have been deleted due to expiration.", logger.UserLogStrings);
		});
	}

	[TestDate]
	public void TestExecuteBatch_ProcessEligibleIvistoCusPollingTransactions()
	{
		var helper = new CusPollingTransactionTestDataHelper(Factory);
		var ivistoRequestInterchange = helper.CreateIvistoRequestInterchange();
		var pollingTransaction = helper.CreateIvistoPollingTransaction("OPN", ZDateTime.UtcNow.AddDays(-1), ivistoRequestInterchange, ZDateTime.UtcNow.AddDays(-2));
		Factory.Save();

		var logger = new LoggingInformation();
		new CusPollingTransactionProcessor(logger).ExecuteBatch();

		CombineAssertions(() =>
		{
			AssertEquals("CusPollingTransaction CPT_Status", "PND", pollingTransaction.CPT_Status);
			AssertEquals("EDIInterchange EI_Status", "QUE", ivistoRequestInterchange.EI_Status);
			AssertCollectionContains("Log", "\t1 CusPollingTransaction(s) have been re-queued for transmission.", logger.UserLogStrings);
		});
	}

	[TestDate]
	public void TestExecuteBatch_ProcessEligibleIrildesCusPollingTransactions()
	{
		var helper = new CusPollingTransactionTestDataHelper(Factory);
		var irildesRequestInterchange = helper.CreateIvistoRequestInterchange();
		var pollingTransaction = helper.CreateIrildesPollingTransaction("OPN", ZDateTime.UtcNow.AddDays(-1), irildesRequestInterchange, ZDateTime.UtcNow.AddDays(-2));
		Factory.Save();

		var logger = new LoggingInformation();
		new CusPollingTransactionProcessor(logger).ExecuteBatch();

		CombineAssertions(() =>
		{
			AssertEquals("CusPollingTransaction CPT_Status", "PND", pollingTransaction.CPT_Status);
			AssertEquals("EDIInterchange EI_Status", "QUE", irildesRequestInterchange.EI_Status);
			AssertCollectionContains("Log", "\t1 CusPollingTransaction(s) have been re-queued for transmission.", logger.UserLogStrings);
		});
	}

	[TestDate]
	public void TestExecuteBatch_IgnoresNotEligibleIvistoCusPollingTransactions()
	{
		var helper = new CusPollingTransactionTestDataHelper(Factory);
		var ivistoRequestInterchange = helper.CreateIvistoRequestInterchange();
		var pollingTransaction = helper.CreateIvistoPollingTransaction("OPN", ZDateTime.UtcNow.AddDays(1), ivistoRequestInterchange, ZDateTime.UtcNow.AddDays(-2));
		Factory.Save();

		var logger = new LoggingInformation();
		new CusPollingTransactionProcessor(logger).ExecuteBatch();

		CombineAssertions(() =>
		{
			AssertEquals("CusPollingTransaction CPT_Status", "OPN", pollingTransaction.CPT_Status);
			AssertEquals("EDIInterchange EI_Status", "SNT", ivistoRequestInterchange.EI_Status);
			AssertCollectionContains("Log", "\t1 CusPollingTransaction(s) have been ignored because CPT_EarliestTimeOfNextAttemptUtc is in the future.", logger.UserLogStrings);
		});
	}

	[TestDate]
	public void TestExecuteBatch_IgnoresNotEligibleIrildesCusPollingTransactions()
	{
		var helper = new CusPollingTransactionTestDataHelper(Factory);
		var irildesRequestInterchange = helper.CreateIrildesRequestInterchange();
		var pollingTransaction = helper.CreateIrildesPollingTransaction("OPN", ZDateTime.UtcNow.AddDays(1), irildesRequestInterchange, ZDateTime.UtcNow.AddDays(-2));
		Factory.Save();

		var logger = new LoggingInformation();
		new CusPollingTransactionProcessor(logger).ExecuteBatch();

		CombineAssertions(() =>
		{
			AssertEquals("CusPollingTransaction CPT_Status", "OPN", pollingTransaction.CPT_Status);
			AssertEquals("EDIInterchange EI_Status", "SNT", irildesRequestInterchange.EI_Status);
			AssertCollectionContains("Log", "\t1 CusPollingTransaction(s) have been ignored because CPT_EarliestTimeOfNextAttemptUtc is in the future.", logger.UserLogStrings);
		});
	}

	[TestDate]
	public void TestExecuteBatch_WhenCancellationIsRequested()
	{
		var helper = new CusPollingTransactionTestDataHelper(Factory);
		var ivistoRequestInterchange = helper.CreateIvistoRequestInterchange();
		var pollingTransaction = helper.CreateIvistoPollingTransaction("OPN", ZDateTime.UtcNow.AddDays(-1), ivistoRequestInterchange, ZDateTime.UtcNow.AddDays(-2));
		Factory.Save();

		var logger = new LoggingInformation();
		var processor = new CusPollingTransactionProcessor(logger);

		CombineAssertions(() =>
		{
			AssertExceptionThrown<OperationCanceledException>(() => processor.ExecuteBatch(new CancellationToken(canceled: true)));
			AssertEquals("CusPollingTransaction CPT_Status", "OPN", pollingTransaction.CPT_Status);
			AssertEquals("EDIInterchange EI_Status", "SNT", ivistoRequestInterchange.EI_Status);
			AssertEquals("Logs Count", 0, logger.UserLogStrings.Count);
		});
	}

	[TestDate]
	public void TestExecuteBatch_IgnoresTransactionBelongingToAnotherCompany()
	{
		var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
		var anotherBranch = anotherCompany.Branches.AddNew();

		var helper = new CusPollingTransactionTestDataHelper(Factory);
		var ivistoRequestInterchange = helper.CreateIvistoRequestInterchange();
		ivistoRequestInterchange.EI_GB = anotherBranch.PK;
		var pollingTransaction = helper.CreateIvistoPollingTransaction("OPN", ZDateTime.UtcNow.AddDays(-1), ivistoRequestInterchange, ZDateTime.UtcNow.AddDays(-2));
		Factory.Save();

		var logger = new LoggingInformation();
		new CusPollingTransactionProcessor(logger).ExecuteBatch();

		CombineAssertions(() =>
		{
			AssertEquals("CusPollingTransaction CPT_Status", "OPN", pollingTransaction.CPT_Status);
			AssertEquals("EDIInterchange EI_Status", "SNT", ivistoRequestInterchange.EI_Status);
			AssertEquals("Logs Count", 0, logger.UserLogStrings.Count);
		});
	}
}
