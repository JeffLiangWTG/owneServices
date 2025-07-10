using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.IT.Business.Testing;

public static class CusPollingTransactionTestHelper
{
	public static void AssertUniquePollingTransactionForInterchange(EDIInterchange interchange, ZString expectedStatus, ZDateTime expectedEarliestTimeOfNextAttemptUtc, ZString expectedType)
	{
		var pollingTransactions = GetInterchangePollingTransactions(interchange);
		AssertEquals("CusPollingTransaction Found", 1, pollingTransactions.Length);

		var transaction = pollingTransactions.Single();
		Assertion.CombineAssertions(() =>
		{
			AssertEquals(nameof(transaction.CPT_ApplicationCode), "ITH", transaction.CPT_ApplicationCode);
			AssertEquals(nameof(transaction.CPT_Status), expectedStatus, transaction.CPT_Status);
			AssertEquals(nameof(transaction.CPT_EarliestTimeOfNextAttemptUtc), expectedEarliestTimeOfNextAttemptUtc, transaction.CPT_EarliestTimeOfNextAttemptUtc);
			AssertEquals(nameof(transaction.CPT_ParentID), interchange.PK, transaction.CPT_ParentID);
			AssertEquals(nameof(transaction.CPT_ParentTableCode), "EI", transaction.CPT_ParentTableCode);
			AssertEquals(nameof(transaction.CPT_Type), expectedType, transaction.CPT_Type);
			AssertEquals(nameof(transaction.CPT_TransactionID), interchange.EI_SessionGUID.ToString(), transaction.CPT_TransactionID);
		});
	}

	public static void AssertNoPollingTransactionsForInterchange(EDIInterchange interchange)
	{
		var pollingTransactions = GetInterchangePollingTransactions(interchange);
		AssertEquals("CusPollingTransaction Found", 0, pollingTransactions.Length);
	}

	static CusPollingTransaction[] GetInterchangePollingTransactions(EDIInterchange interchange)
		=> interchange.Factory.Load<CusPollingTransaction>(new ZQuery(CusPollingTransactionSchema.CPT_ParentID, interchange.PK));
}
