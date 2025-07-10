using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.Customs;

namespace Enterprise.Customs.IT.Business;

public sealed class CusPollingTransactionProcessor : BatchProcess
{
	public CusPollingTransactionProcessor(LoggingInformation logger) : base(logger)
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Service Task Logs")]
	protected override void Execute(CancellationToken cancellationToken)
	{
		int deletedTransactionsCount = 0, reQueuedTransactionsCount = 0, ignoredTransactionsCount = 0;
		var factory = new BusinessObjectFactory();

		var pollingTransactions = LoadCusPollingTransactions(factory);

		try
		{
			foreach (var transaction in pollingTransactions)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (transaction.IsExpired())
				{
					transaction.Delete();
					deletedTransactionsCount++;
				}
				else if (transaction.IsEligibleForProcessing())
				{
					transaction.ReQueue();
					reQueuedTransactionsCount++;
				}
				else
				{
					ignoredTransactionsCount++;
				}
			}
			factory.Save();

			LogProcessingResult(deletedTransactionsCount, " CusPollingTransaction(s) have been deleted due to expiration.");
			LogProcessingResult(reQueuedTransactionsCount, " CusPollingTransaction(s) have been re-queued for transmission.");
			LogProcessingResult(ignoredTransactionsCount, " CusPollingTransaction(s) have been ignored because CPT_EarliestTimeOfNextAttemptUtc is in the future.");
		}
		catch (ZSaveException e)
		{
			ZExceptionReporting.HandleSaveException(e);
		}
	}

	#region Implementation

	static CusPollingTransaction[] LoadCusPollingTransactions(BusinessObjectFactory factory)
	{
		var query = new ZDBOnlyQuery(typeof(CusPollingTransaction));
		query.AddToFilter(CusPollingTransactionSchema.CPT_ApplicationCode, EDIMessage.ApplicationCodes.ITCustomsXTrade);
		query.AddToFilter(CusPollingTransactionSchema.CPT_Type, new string[] { EDIMessageTypeList.Codes.IvistoRequest, EDIMessageTypeList.Codes.IrildesRequest });
		query.AddToFilter(CusPollingTransactionSchema.CPT_Status, CusPollingTransactionStatus.Codes.OPN);

		var branchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
		branchSubQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);

		var interchangeSubQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.PK);
		interchangeSubQuery.AddSubQuery(EDIInterchangeSchema.EI_GB, branchSubQuery, JoinCondition.And);

		query.AddSubQuery(CusPollingTransactionSchema.CPT_ParentID, interchangeSubQuery, JoinCondition.And);
		return factory.Load<CusPollingTransaction>(query);
	}

	void LogProcessingResult(int affectedRecords, string message)
	{
		if (affectedRecords > 0)
		{
			Logger.Log(affectedRecords + message);
		}
	}

	#endregion
}
