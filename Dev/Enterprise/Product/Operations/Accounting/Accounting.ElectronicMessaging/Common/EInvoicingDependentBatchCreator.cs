using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging
{
	/// <summary>
	/// This batch creator handles the dependency between an original invoice and reversal / amending transactions.
	/// </summary>
	public abstract class EInvoicingDependentBatchCreator : EInvoicingBatchCreatorBase
	{
		readonly bool WaitForOriginalTransactionForAmending;

		protected EInvoicingDependentBatchCreator(GlbCompany company) : base(company)
		{
			WaitForOriginalTransactionForAmending = ObjectFactory.Get<IGlobalEInvoicingObjectFactory>().GetCountryEInvoicingBatchCreatorStrategy(company.GC_RN_NKCountryCode)?.SupportsWaitForOriginalTransactionForAmending ?? false;
		}

		protected override IEnumerable<QueuedPivotPKs> GroupTransactionForBatching(DynamicBusinessObjectCollection transactions)
		{
			return transactions?.Select(x => QueuedPivotPKs.FromDynamicBizOCollection(new DynamicBusinessObject[] { x })) ?? Enumerable.Empty<QueuedPivotPKs>();
		}

		#region SuppressResourceStringsCheckRegion

		public override ZBool PerformBatching(ILogger serviceLogger)
		{
			var result = ZBool.False;
			serviceLogger.Log(LogType.Debug, "Batching started.");

			#region Transaction processing dependencies
			//This order cannot be modified, we must process cancellations and amendments before other transactions, since cancellations and amendments can modify the status of its parent.

			var actionTypesToInclude = Constants.EInvoicingPivotActionType.CommandActionTypes;
			actionTypesToInclude = actionTypesToInclude.AddRange(Constants.EInvoicingPivotActionType.QueryActionTypes);
			actionTypesToInclude = actionTypesToInclude.Remove(Constants.EInvoicingPivotActionType.Cancel);

			var allTransactionsForCompanyWithCancelActionType = GetAllTransactionsForCompany(new string[] { Constants.EInvoicingPivotActionType.Cancel });
			if (allTransactionsForCompanyWithCancelActionType.Any())
			{
				serviceLogger.Log(LogType.Debug, "Processing cancellation pivots.");
				CreateBatchesAndUpdatePivotsForCompany(allTransactionsForCompanyWithCancelActionType, serviceLogger);
				result = ZBool.True;
			}

			if (WaitForOriginalTransactionForAmending)
			{
				var allTransactionsForCompanyWithAmendActionType = GetAllTransactionsForCompany(new string[] { Constants.EInvoicingPivotActionType.Amend });
				if (allTransactionsForCompanyWithAmendActionType.Any())
				{
					serviceLogger.Log(LogType.Debug, "Processing amending pivots.");
					CreateBatchesAndUpdatePivotsForCompany(allTransactionsForCompanyWithAmendActionType, serviceLogger);
					result = ZBool.True;
				}

				actionTypesToInclude = actionTypesToInclude.Remove(Constants.EInvoicingPivotActionType.Amend);
			}

			var allOtherTransactionsForCompany = GetAllTransactionsForCompany(actionTypesToInclude.ToArray(), GetActionTypesWithTransactionID());
			if (allOtherTransactionsForCompany.Any())
			{
				serviceLogger.Log(LogType.Debug, "Processing all queued pivots.");
				CreateBatchesAndUpdatePivotsForCompany(allOtherTransactionsForCompany, serviceLogger);
				result = ZBool.True;
			}

			#endregion

			if (EnableRetryReadyStatusPivots)
			{
				var waitingPivotGuids = GetAllReadyToRetryStatusPivots();
				foreach (var pk in waitingPivotGuids.ToList())
				{
					using (var transaction = Db.Connection.BeginTransactionWithManager()) // Not using factories because of performance for batch operations
					{
						var pivotPk = ((ZGuid)pk["AIP_PK"]).ToGuid();
						UpdatePivotStateAndBatchState(pivotPk, Constants.EInvoicingPivotState.Batched, Constants.EInvoicingBatchState.Ready);
						transaction.CommitTransaction();
					}
				}

				result = result || waitingPivotGuids.Any();
			}

			if (!string.IsNullOrEmpty(APTransactionListRequestBatchId.Value))
			{
				result |= TryToCreateBatchForAPTransactionsRequest(serviceLogger);
			}

			serviceLogger.Log(LogType.Information, "Batching completed" + (result ? "." : " - No transactions were available for batching.")); // Message in English
			return result;
		}

		protected override ZGuid CreateNewInvoicingBatch(QueuedPivotPKs queuedPivots, DynamicBusinessObjectCollection allTransactionsForCompany)
		{
			var batchPk = ZGuid.Empty;

			if (!Db.Connection.IsInTransaction)
			{
				ErrorReporter.ReportOnce("Database transaction is not active.");
				return batchPk;
			}

			var pivotId = queuedPivots.Items.FirstOrDefault();
			var pivot = allTransactionsForCompany.FirstOrDefault(x => (ZGuid)x["AIP_PK"] == pivotId);
			if (pivot == null)
			{
				return batchPk;
			}

			var parentId = ((ZGuid)pivot["AIP_ParentID"]).ToGuid();
			var actionType = ((ZString)pivot["AIP_ActionType"]).ToString();
			var originalParentId = Guid.Empty;
			var parentGovernmentAllocatedID = ((ZString)pivot["AH_GovernmentAllocatedID"]).ToString();

			var pivotCanBeBatched = true;
			if (actionType == Constants.EInvoicingPivotActionType.Cancel)
			{
				originalParentId = GetOriginalTransactionParentId(parentId);
				pivotCanBeBatched = CancelPivotCanBeBatched(pivotId, parentId, originalParentId);
			}
			else if (WaitForOriginalTransactionForAmending && actionType == Constants.EInvoicingPivotActionType.Amend)
			{
				originalParentId = GetOriginalTransactionParentId(parentId);
				pivotCanBeBatched = AmendingPivotCanBeBatched(pivotId, parentId, originalParentId);
			}

			if (pivotCanBeBatched)
			{
				var parentIdForGovernmentAllocatedNumber = originalParentId != Guid.Empty && PopulateGovernmentAllocatedNumberWithOriginalTransactionGvtNumber ? originalParentId : parentId;
				var governmentAllocatedID = GetActionTypesWithTransactionID().Contains(actionType)
					? parentGovernmentAllocatedID
					: GetGovernmentAllocatedNumber(parentIdForGovernmentAllocatedNumber)?.ToString() ?? string.Empty;
				batchPk = CreateBatch(governmentAllocatedID);
			}

			return batchPk;
		}

		protected virtual string[] GetActionTypesWithTransactionID() => Array.Empty<string>();

		protected virtual bool CancelPivotCanBeBatched(Guid pivotId, Guid parentId, Guid originalParentId)
		{
			const string OriginalTransactionPivotNotFound = "";

			(Guid originalPivotId, string originalPivotState, _) = GetOriginalTransactionPivotPKAndState(originalParentId);
			switch (originalPivotState)
			{
				case Constants.EInvoicingPivotState.Succeed:
				case Constants.EInvoicingPivotState.Delivered:
					return true;
				case Constants.EInvoicingPivotState.Failed:
				case Constants.EInvoicingPivotState.Discarded:
					UpdatePivotStateAndBatchState(pivotId, errorDescription: Res.GetString("98957299-1BC7-4DB0-AEF7-B8B52178D345", "Cancellation request was not sent because the original transaction had encountered an error during submission."));
					return false;
				case Constants.EInvoicingPivotState.Batched:
				case Constants.EInvoicingPivotState.BatchedWithError:
				case Constants.EInvoicingPivotState.Queued:
					UpdatePivotStateAndBatchState(pivotId, errorDescription: Res.GetString("E5243FEF-5928-4C46-87B8-E65FEEF48B93", "Cancellation request was not sent because the original transaction had encountered an error during batching."));
					UpdatePivotStateAndBatchState(originalPivotId); // This is the original transaction pivot, no error necessary. Probably has its own error on it.
					return false;
				case OriginalTransactionPivotNotFound:
					UpdatePivotStateAndBatchState(pivotId, errorDescription: Res.GetString("4C92F8F0-8335-4132-AD42-586A6A9143BF", "This transaction was erroneously queued for cancellation even though the original transaction had not been submitted for e-Reporting. Please contact support."));
					return false;
				default:
					return false; // This is the "try again later" case, no error necessary.
			}
		}

		protected virtual bool AmendingPivotCanBeBatched(Guid pivotId, Guid parentId, Guid originalParentId) => true;

		protected void UpdatePivotStateAndBatchState(Guid pivotId, string pivotState = Constants.EInvoicingPivotState.Discarded, string batchState = Constants.EInvoicingBatchState.Discarded, string errorDescription = "")
		{
			if (!Db.Connection.IsInTransaction)
			{
				ErrorReporter.ReportOnce("Database transaction is not active.");
				return;
			}

			var updateSQL = new ZStringBuilder();
			updateSQL.AppendLine(@"
DECLARE @BatchId UNIQUEIDENTIFIER;
UPDATE dbo.AccEInvoicingTransactionPivot SET AIP_Status = @pivotState, @BatchId = AIP_AIB, AIP_SystemLastEditTimeUtc = GETUTCDATE(), AIP_SystemLastEditUser = @SystemLastEditUser");

			updateSQL.AppendLine(string.IsNullOrWhiteSpace(errorDescription) ? "" : @"
, AIP_ErrorDescription = @errorDescription");

			updateSQL.AppendLine(@"
WHERE AIP_PK = @pivotId;
UPDATE dbo.AccEInvoicingBatch SET AIB_Status = @batchState WHERE AIB_PK = @BatchId;");

			using (var updateCommand = Db.Connection.Command(updateSQL.ToString())) // Not using factories because of performance for batch operations
			{
				updateCommand.AddParameter("@pivotId", SqlDbType.UniqueIdentifier, pivotId);
				updateCommand.AddParameter("@batchState", SqlDbType.Char, batchState);
				updateCommand.AddParameter("@pivotState", SqlDbType.Char, pivotState);
				updateCommand.AddParameter("@errorDescription", SqlDbType.NVarChar, errorDescription);
				updateCommand.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
				updateCommand.ExecuteNonQuery();
			}
		}

		Guid GetOriginalTransactionParentId(Guid parentId)
		{
			using (var selectCommand = Db.Connection.Command("SELECT TOP 1 AH_TransactionBelongsToGroup FROM dbo.AccTransactionHeader WHERE AH_PK = @parentId")) // Not using factories because of performance for batch operations
			{
				selectCommand.AddParameter("@parentId", SqlDbType.UniqueIdentifier, parentId);
				return Guid.TryParse(selectCommand.ExecuteScalar().ToString(), out var guid) ? guid : Guid.Empty;
			}
		}

		protected (string, DateTime) GetManualOriginalTransactionValues(Guid parentId)
		{
#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
			using var selectCommand = Db.Connection.Command("SELECT TOP 1 AH_OriginalTransactionNum, AH_OriginalInvoiceDate FROM dbo.AccTransactionHeader WHERE AH_PK = @parentId"); // Not using factories because of performance for batch operations
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods
			selectCommand.AddParameter("@parentId", SqlDbType.UniqueIdentifier, parentId);

			string originalTransactionNum = null;
			DateTime originalInvoiceDate = default;
			using (var reader = selectCommand.ExecuteReader())
			{
				reader.Read();
				originalTransactionNum = reader.GetString(0);
				if (!reader.IsDBNull(1))
				{
					originalInvoiceDate = reader.GetDateTime(1);
				}
			}

			return (originalTransactionNum, originalInvoiceDate);
		}

		protected (Guid originalPivotId, string originalPivotStatus, bool wasEInvoicingEnabledForOriginal) GetOriginalTransactionPivotPKAndState(Guid parentTransactionId)
		{
			var pivotId = Guid.Empty;
			var pivotStatus = "";
			var wasEInvoicingEnabled = true;
			var selectString = $@"
SELECT TOP 1
	AIP_PK,
	AIP_Status,
	CASE WHEN AIP_Status = 'DCD' THEN AIP_ErrorDescription ELSE NULL END AS AIP_ErrorDescription
FROM dbo.AccEInvoicingTransactionPivot
WHERE AIP_ParentID = @parentId
	AND AIP_ActionType in ({GetOriginalTransactionSqlAcctionTypes})
ORDER BY CASE WHEN AIP_Status = @pivotStatusSucceed THEN 10 WHEN AIP_Status = @pivotStatusDiscarded THEN 0 ELSE 5 END DESC";

			using (var selectCommand = Db.Connection.Command(selectString)) // Not using factories because of performance for batch operations
			{
				selectCommand.AddParameter("@parentId", SqlDbType.UniqueIdentifier, parentTransactionId);
				selectCommand.AddParameter("@pivotStatusSucceed", SqlDbType.Char, Constants.EInvoicingPivotState.Succeed);
				selectCommand.AddParameter("@pivotStatusDiscarded", SqlDbType.Char, Constants.EInvoicingPivotState.Discarded);
				AddOriginalTransactionSqlAcctionTypesToCommand(selectCommand);

				using (var reader = selectCommand.ExecuteReader())
				{
					while (reader.Read())
					{
						pivotId = (Guid)reader["AIP_PK"];
						pivotStatus = (string)reader["AIP_Status"];
						if (pivotStatus == Constants.EInvoicingPivotState.Discarded)
						{
							var errorMessage = (string)reader["AIP_ErrorDescription"] ?? string.Empty;
							var hasEInvoicingDisabledErrorMessage = errorMessage.StartsWith(Business.EInvoicing.AccEInvoicingTransactionPivot.BaseErrorDescriptionWhenEInvoicingDisabled, StringComparison.Ordinal);
							wasEInvoicingEnabled = !hasEInvoicingDisabledErrorMessage;
						}
					}
				}
			}

			return (pivotId, pivotStatus, wasEInvoicingEnabled);
		}

		protected DynamicBusinessObjectCollection GetAllReadyToRetryStatusPivots()
		{
			var factory = new BusinessObjectFactory();

			var livePivotsQuery = "SELECT COUNT(*) LivePivotCount FROM dbo.AccEInvoicingTransactionPivot WHERE AIP_GC = @CompanyPK AND AIP_ActionType = @ActionType AND AIP_Status = @Status";

			var livePivotsCriteria = new ZSqlParameter[]
			{
				ZSqlParameter.New("@CompanyPK", CurrentCompany.PK, AccEInvoicingTransactionPivotSchema.AIP_GC),
				ZSqlParameter.New("@ActionType", Constants.EInvoicingPivotActionType.StatusCheck, AccEInvoicingTransactionPivotSchema.AIP_ActionType),
				ZSqlParameter.New("@Status", Constants.EInvoicingPivotState.Batched, AccEInvoicingTransactionPivotSchema.AIP_Status),
			};

			var livePivotCount = factory.LoadScalarValue<ZInt>(livePivotsQuery, livePivotsCriteria);

			var freeSlots = Math.Max(MaxLiveRetryStatusCheckPivots - livePivotCount, 0);

			var selectQuery = $"SELECT TOP {freeSlots} AIP_PK FROM dbo.AccEInvoicingBatch JOIN dbo.AccEInvoicingTransactionPivot ON AIB_PK = AIP_AIB WHERE AIP_Status = @Status AND AIP_GC = @CompanyPK AND AIP_ActionType= @ActionType AND AIP_LastSentTimeUtc <= @LatestCallTimeToRetryStatusCheck AND AIB_Status = @BatchStatus";

			var sqlParameters = new ZSqlParameter[]
			{
				ZSqlParameter.New("@Status", Constants.EInvoicingPivotState.Sent, AccEInvoicingTransactionPivotSchema.AIP_Status),
				ZSqlParameter.New("@CompanyPK", CurrentCompany.PK, AccEInvoicingTransactionPivotSchema.AIP_GC),
				ZSqlParameter.New("@ActionType", Constants.EInvoicingPivotActionType.StatusCheck, AccEInvoicingTransactionPivotSchema.AIP_ActionType),
				ZSqlParameter.New("@LatestCallTimeToRetryStatusCheck", LatestCallTimeToRetryStatusCheck, AccEInvoicingTransactionPivotSchema.AIP_LastResponseReceivedUtc),
				ZSqlParameter.New("@BatchStatus", Constants.EInvoicingBatchState.Sent, AccEInvoicingBatchSchema.AIB_Status)
			};

			var allWaitingPivotsForCompany = new DynamicBusinessObjectCollection(factory);
			allWaitingPivotsForCompany.Load(selectQuery, sqlParameters);

			return allWaitingPivotsForCompany;
		}

		protected virtual object GetGovernmentAllocatedNumber(ZGuid parentId)
		{
			var selectString = $@"SELECT TOP 1 AIB_GovernmentAllocatedNumber FROM dbo.AccEInvoicingBatch INNER JOIN dbo.AccEInvoicingTransactionPivot ON AIB_PK = AIP_AIB 
								WHERE AIP_Status IN (@pivotStatusSucceed, @pivotStatusDelivered) AND AIP_ActionType IN ({GetOriginalTransactionSqlAcctionTypes}) AND AIP_ParentId = @parentId";
			using (var selectCommand = Db.Connection.Command(selectString)) // Not using factories because of performance for batch operations
			{
				selectCommand.AddParameter("@parentId", SqlDbType.UniqueIdentifier, parentId.ToGuid());
				selectCommand.AddParameter("@pivotStatusSucceed", SqlDbType.Char, Constants.EInvoicingPivotState.Succeed);
				selectCommand.AddParameter("@pivotStatusDelivered", SqlDbType.Char, Constants.EInvoicingPivotState.Delivered);
				AddOriginalTransactionSqlAcctionTypesToCommand(selectCommand);

				return selectCommand.ExecuteScalar();
			}
		}

		ZString GetOriginalTransactionSqlAcctionTypes => WaitForOriginalTransactionForAmending ? "@actionTypeSUB, @actionTypeAMD" : "@actionTypeSUB";
		void AddOriginalTransactionSqlAcctionTypesToCommand(DbCommand command)
		{
			command.AddParameter("@actionTypeSUB", SqlDbType.Char, Constants.EInvoicingPivotActionType.Submit);
			if (WaitForOriginalTransactionForAmending)
			{
				command.AddParameter("@actionTypeAMD", SqlDbType.Char, Constants.EInvoicingPivotActionType.Amend);
			}
		}

		protected virtual ZBool EnableRetryReadyStatusPivots { get; } = false;
		protected virtual ZBool PopulateGovernmentAllocatedNumberWithOriginalTransactionGvtNumber { get; } = true;
		protected virtual ZDateTime LatestCallTimeToRetryStatusCheck { get; }
		protected virtual ZInt MaxLiveRetryStatusCheckPivots { get; } = 100;
		#endregion
	}
}
