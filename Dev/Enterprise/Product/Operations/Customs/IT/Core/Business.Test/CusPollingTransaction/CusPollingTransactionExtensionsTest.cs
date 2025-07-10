using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CusPollingTransactionExtensionsTest : TestCaseWithFactory
{
	[TestDate]
	public void TestIsExpired()
	{
		var pollingTransaction = Factory.New<CusPollingTransaction>();
		var now = ZDateTime.UtcNow;

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => CusPollingTransactionExtensions.IsExpired(null));

			pollingTransaction.CPT_SystemCreateTimeUtc = now.AddDays(-239);
			AssertEquals("When CPT_SystemCreateTimeUtc is less than 240 days ago", false, pollingTransaction.IsExpired());

			pollingTransaction.CPT_SystemCreateTimeUtc = now.AddDays(-240);
			AssertEquals("When CPT_SystemCreateTimeUtc is equal to 240 days ago", false, pollingTransaction.IsExpired());

			pollingTransaction.CPT_SystemCreateTimeUtc = now.AddDays(-241);
			AssertEquals("When CPT_SystemCreateTimeUtc is more than 240 days ago", true, pollingTransaction.IsExpired());
		});
	}

	[TestDate]
	public void TestIsEligibleForProcessing()
	{
		var pollingTransaction = Factory.New<CusPollingTransaction>();
		var now = ZDateTime.UtcNow;

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => CusPollingTransactionExtensions.IsEligibleForProcessing(null));

			pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = now.AddMinutes(1);
			AssertEquals("When CPT_EarliestTimeOfNextAttemptUtc is in the future", false, pollingTransaction.IsEligibleForProcessing());

			pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = now;
			AssertEquals("When CPT_EarliestTimeOfNextAttemptUtc is equal to now", true, pollingTransaction.IsEligibleForProcessing());

			pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = now.AddMinutes(-1);
			AssertEquals("When CPT_EarliestTimeOfNextAttemptUtc is in the past", true, pollingTransaction.IsEligibleForProcessing());
		});
	}

	public void TestReQueue()
	{
		AssertExceptionThrown<ArgumentNullException>(
			"When pollingTransaction is null",
			() => CusPollingTransactionExtensions.ReQueue(null));

		AssertExceptionThrown<ArgumentNullException>(
			"When pollingTransaction ParentObject is null",
			() => CusPollingTransactionExtensions.ReQueue(Factory.New<CusPollingTransaction>()));

		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_Status = "SNT";
		var pollingTransaction = Factory.New<CusPollingTransaction>();
		pollingTransaction.CPT_Status = "OPN";
		pollingTransaction.CPT_ParentID = interchange.PK;
		pollingTransaction.CPT_ParentTableCode = interchange.TablePrefix;

		CombineAssertions("PRE-CONDITION", () =>
		{
			AssertEquals("CusPollingTransaction CPT_Status", "OPN", pollingTransaction.CPT_Status);
			AssertEquals("EDIInterchange EI_Status", "SNT", interchange.EI_Status);
		});

		pollingTransaction.ReQueue();
		CombineAssertions("POST-CONDITION", () =>
		{
			AssertEquals("CusPollingTransaction CPT_Status", "PND", pollingTransaction.CPT_Status);
			AssertEquals("EDIInterchange EI_Status", "QUE", interchange.EI_Status);
		});
	}

	[TestDate]
	public void TestCreateOrReOpenPollingTransaction()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = "ITH";
		interchange.EI_InterchangeType = "XXX";

		var pollingTransaction = interchange.CreateOrReOpenPollingTransaction();
		CusPollingTransactionTestHelper.AssertUniquePollingTransactionForInterchange(interchange, "OPN", ZDateTime.UtcNow.AddDays(1), "XXX");

		pollingTransaction.CPT_Status = "PND";
		interchange.CreateOrReOpenPollingTransaction();
		CusPollingTransactionTestHelper.AssertUniquePollingTransactionForInterchange(interchange, "OPN", ZDateTime.UtcNow.AddDays(2), "XXX");
	}

	public void TestDeletePollingTransactions()
	{
		AssertExceptionThrown<ArgumentNullException>(() => CusPollingTransactionExtensions.DeletePollingTransactions(null));

		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = "AAA";
		interchange.EI_InterchangeType = "BBB";
		var transaction1 = CreatePollingTransaction(interchange, "AAA", "BBB");
		var transaction2 = CreatePollingTransaction(interchange, "AAA", "CCC");
		var transaction3 = CreatePollingTransaction(interchange, "CCC", "BBB");

		interchange.DeletePollingTransactions();

		CombineAssertions(() =>
		{
			AssertEquals("PollingTransaction that matches ApplicationCode and Type, IsDeleted", true, transaction1.IsDeleted);
			AssertEquals("PollingTransaction that only matches ApplicationCode, IsDeleted", false, transaction2.IsDeleted);
			AssertEquals("PollingTransaction that only matches Type, IsDeleted", false, transaction3.IsDeleted);
		});
	}

	CusPollingTransaction CreatePollingTransaction(EDIInterchange interchange, string applicationCode, string transactionType)
	{
		var pollingTransaction = Factory.New<CusPollingTransaction>();
		pollingTransaction.CPT_ParentID = interchange.PK;
		pollingTransaction.CPT_ParentTableCode = interchange.TablePrefix;
		pollingTransaction.CPT_ApplicationCode = applicationCode;
		pollingTransaction.CPT_Type = transactionType;
		return pollingTransaction;
	}
}
