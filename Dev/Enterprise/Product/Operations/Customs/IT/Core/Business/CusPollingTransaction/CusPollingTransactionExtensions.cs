using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.Customs;

namespace Enterprise.Customs.IT.Business;

static class CusPollingTransactionExtensions
{
	public static bool IsExpired(this CusPollingTransaction pollingTransaction)
	{
		Argument.NotNull(pollingTransaction, nameof(pollingTransaction));

		return pollingTransaction.CPT_SystemCreateTimeUtc < ZDateTime.UtcNow.AddDays(-ExpirationLimitDays);
	}

	public static bool IsEligibleForProcessing(this CusPollingTransaction pollingTransaction)
	{
		Argument.NotNull(pollingTransaction, nameof(pollingTransaction));

		return pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc <= ZDateTime.UtcNow;
	}

	public static void ReQueue(this CusPollingTransaction pollingTransaction)
	{
		Argument.NotNull(pollingTransaction, nameof(pollingTransaction));

		var interchange = pollingTransaction.ParentObject as EDIInterchange;
		Argument.NotNull(interchange, "EDIInterchange CusPollingTransaction parent object");

		pollingTransaction.CPT_Status = CusPollingTransactionStatus.Codes.PND;
		interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
	}

	public static CusPollingTransaction CreateOrReOpenPollingTransaction(this EDIInterchange interchange)
	{
		Argument.NotNull(interchange, nameof(interchange));

		var factory = interchange.Factory;
		var pollingTransaction = LoadPendingPollingTransaction(factory, interchange);
		if (pollingTransaction is null)
		{
			pollingTransaction = factory.New<CusPollingTransaction>();
			pollingTransaction.CPT_ApplicationCode = interchange.EI_ApplicationCode;
			pollingTransaction.CPT_ParentID = interchange.PK;
			pollingTransaction.CPT_ParentTableCode = interchange.TablePrefix;
			pollingTransaction.CPT_Type = interchange.EI_InterchangeType;
			pollingTransaction.CPT_TransactionID = interchange.EI_SessionGUID.ToString();
			pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = ZDateTime.UtcNow.AddDays(1);
		}
		else
		{
			pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc.AddDays(1);
		}
		pollingTransaction.CPT_Status = CusPollingTransactionStatus.Codes.OPN;
		return pollingTransaction;
	}

	public static void DeletePollingTransactions(this EDIInterchange interchange)
	{
		Argument.NotNull(interchange, nameof(interchange));

		var transactionsToDelete = LoadPollingTransactions(interchange.Factory, interchange);
		transactionsToDelete.DeleteAll();
	}

	static CusPollingTransaction LoadPendingPollingTransaction(BusinessObjectFactory factory, EDIInterchange interchange)
	{
		var query = GetBaseQuery(interchange)
			.AddToFilter(CusPollingTransactionSchema.CPT_Status, CusPollingTransactionStatus.Codes.PND);

		return factory.LoadTop1<CusPollingTransaction>(query);
	}

	static CusPollingTransaction[] LoadPollingTransactions(BusinessObjectFactory factory, EDIInterchange interchange)
	{
		var query = GetBaseQuery(interchange);

		return factory.Load<CusPollingTransaction>(query);
	}

	static ZQuery GetBaseQuery(EDIInterchange interchange) => new ZQuery(CusPollingTransactionSchema.CPT_ParentID, interchange.PK)
		.AddToFilter(CusPollingTransactionSchema.CPT_Type, interchange.EI_InterchangeType)
		.AddToFilter(CusPollingTransactionSchema.CPT_ApplicationCode, interchange.EI_ApplicationCode);

	const int ExpirationLimitDays = 240;
}
