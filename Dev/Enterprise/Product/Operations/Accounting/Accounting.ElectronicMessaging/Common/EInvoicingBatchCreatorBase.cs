using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	#region SuppressResourceStringsCheckRegion

	public abstract class EInvoicingBatchCreatorBase
	{
		protected EInvoicingBatchCreatorBase(GlbCompany company)
		{
			CurrentCompany = Argument.NotNull(company, nameof(company));
			APTransactionListRequestBatchId = new Lazy<string>(() => GetAPTransactionListRequestBatchId());
		}

		protected GlbCompany CurrentCompany;
		internal Lazy<string> APTransactionListRequestBatchId;

		string GetAPTransactionListRequestBatchId() => GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CurrentCompany.GC_RN_NKCountryCode)?.ApTransactionListRequestBatchId;

		public virtual ZBool PerformBatching(ILogger serviceLogger)
		{
			serviceLogger.Log(LogType.Debug, "Batching sub task started.");

			var allTransactionsForCompany = GetAllTransactionsForCompany();

			if (allTransactionsForCompany.Any())
			{
				CreateBatchesAndUpdatePivotsForCompany(allTransactionsForCompany, serviceLogger);
				serviceLogger.Log(LogType.Debug, "Batching sub task completed.");
				return true;
			}
			else
			{
				serviceLogger.Log(LogType.Debug, "Batching sub task completed - No transactions were available for batching.");
				return false;
			}
		}

		protected virtual DynamicBusinessObjectCollection GetAllTransactionsForCompany(
			string[] actionTypes = null,
			string[] actionTypesJoiningTransactionHeader = null,
			bool queryJoinWithHeadersOnly = false)
		{
			actionTypes = actionTypes ?? Array.Empty<string>();

			var actionsJoinTransactionHeader = actionTypesJoiningTransactionHeader ?? Array.Empty<string>();
			var actionsNotJoinTransactionHeader = actionTypes.Except(actionsJoinTransactionHeader).ToArray();

			var hasActionsJoinHeaders = actionsJoinTransactionHeader.Any();
			var hasActionsNotJoinHeaders = actionsNotJoinTransactionHeader.Any();

			if (queryJoinWithHeadersOnly)
			{
				if (hasActionsNotJoinHeaders)
				{
					throw new ArgumentException(
						$"No values for '{nameof(actionTypes)}' should supplied, when {nameof(queryJoinWithHeadersOnly)}=true");
				}
			}

			var sqlParameters = new List<ZSqlParameter>()
			{
				ZSqlParameter.New("@Status", EInvoicingPivotState.Queued, AccEInvoicingTransactionPivotSchema.AIP_Status),
				ZSqlParameter.New("@CompanyPK", CurrentCompany.PK, AccEInvoicingTransactionPivotSchema.AIP_GC),
			};

			var selectQueryNotJoinHeaders = @"SELECT AIP_PK, AIP_GC, AIP_ActionType, AIP_ParentID, AH_GovernmentAllocatedID = '', AH_GB = CONVERT(uniqueidentifier, 0x), AH_Ledger = '', AH_TransactionType = '', AH_TransactionNum = '', OH_Code = ''
FROM dbo.AccEInvoicingTransactionPivot WHERE AIP_Status = @Status AND AIP_GC = @CompanyPK";
			if (hasActionsNotJoinHeaders)
			{
				selectQueryNotJoinHeaders = AppendActionTypeConditions(
					selectQueryNotJoinHeaders,
					sqlParameters,
					actionsNotJoinTransactionHeader,
					"@ActionType");
			}

			var selectQueryJoinHeaders = @"SELECT AIP_PK, AIP_GC, AIP_ActionType, AIP_ParentID, AH_GovernmentAllocatedID, AH_GB, AH_Ledger, AH_TransactionType, AH_TransactionNum, ISNULL(OH_Code,'') as OH_Code
FROM dbo.AccEInvoicingTransactionPivot
JOIN dbo.AccTransactionHeader ON AH_PK = AIP_ParentID
LEFT OUTER JOIN dbo.OrgHeader ON OH_PK = AH_OH WHERE AIP_Status = @Status AND AIP_GC = @CompanyPK";
			if (hasActionsJoinHeaders)
			{
				selectQueryJoinHeaders = AppendActionTypeConditions(
					selectQueryJoinHeaders,
					sqlParameters,
					actionsJoinTransactionHeader,
					"@ActionTypeWithTransactionID");
			}

			string AppendActionTypeConditions(string sqlQuery, List<ZSqlParameter> sqlParams, string[] types, string prefix)
			{
				var codesBuilder = new ZStringBuilder();
				for (int i = 0; i < types.Length; i++)
				{
					var parameterName = Invariant($"{prefix}{i + 1}");
					sqlParams.Add(ZSqlParameter.New(parameterName, types[i], AccEInvoicingTransactionPivotSchema.AIP_ActionType));
					codesBuilder.Append(parameterName);
				}

				sqlQuery += $" AND AIP_ActionType IN ({codesBuilder.ToStringWithDelimiterBetweenAppends(", ")})";
				return sqlQuery;
			}

			var collectedQuery = new ZStringBuilder();
			if (queryJoinWithHeadersOnly)
			{
				collectedQuery.Append(selectQueryJoinHeaders);
			}
			else if (hasActionsJoinHeaders)
			{
				collectedQuery.Append(selectQueryNotJoinHeaders)
							  .AppendLine("UNION ALL")
							  .AppendLine(selectQueryJoinHeaders);
			}
			else
			{
				collectedQuery.Append(selectQueryNotJoinHeaders);
			}

			var allTransactionsForCompany = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			allTransactionsForCompany.Load(collectedQuery.ToString(), sqlParameters.ToArray());
			return allTransactionsForCompany;
		}

		protected void CreateBatchesAndUpdatePivotsForCompany(DynamicBusinessObjectCollection allTransactionsForCompany, ILogger serviceLogger, string defaultPivotStatus = EInvoicingPivotState.Batched)
		{
			serviceLogger.Log(LogType.Debug, "Started creating batch");

			var transactionsGroupedForBatching = GroupTransactionForBatching(allTransactionsForCompany);
			foreach (var transactionBatchGroup in transactionsGroupedForBatching)
			{
				using (var transaction = Db.Connection.BeginTransactionWithManager()) // Not using factories because of performance for batch operations
				{
					var batchPk = CreateNewInvoicingBatch(transactionBatchGroup, allTransactionsForCompany);
					UpdateTransactionPivots(transactionBatchGroup.Items, !batchPk.IsEmpty ? batchPk.ToGuid() : Guid.Empty, defaultPivotStatus);
					transaction.CommitTransaction();
				}
			}

			serviceLogger.Log(LogType.Debug, "Completed creating batch.");
		}

		protected abstract IEnumerable<QueuedPivotPKs> GroupTransactionForBatching(DynamicBusinessObjectCollection transactions);

		protected virtual ZGuid CreateNewInvoicingBatch(QueuedPivotPKs queuedPivots, DynamicBusinessObjectCollection allTransactionsForCompany)
			=> CreateBatch();

		protected ZGuid CreateBatch(string governmentAllocatedNumber = "", string batchStatus = EInvoicingBatchState.Ready)
		{
			var batchPk = ZGuid.Empty;

			if (!Db.Connection.IsInTransaction)
			{
				ErrorReporter.ReportOnce("Database connection is not active.");
				return batchPk;
			}

			using (SwitchCompanyContextTemporarilyIfRequires())
			{
				var newBatchNumber = int.Parse(AccountingNumberFountainWrapperFactory.Instance.AccEInvoicingBatchNumber.GetNext(Db.Connection), CultureInfo.InvariantCulture);
				var insertSQL = @"INSERT INTO dbo.AccEInvoicingBatch (AIB_PK, AIB_GC, AIB_BatchNumber, AIB_SystemCreateTimeUtc, AIB_SystemCreateUser, AIB_GovernmentAllocatedNumber, AIB_Status)
							OUTPUT INSERTED.AIB_PK
							VALUES (newid(), @companyPK, @batchNo, @createdTime, @createdUser, @governmentAllocatedNumber, @batchStatus)";

				using (var insertCommand = Db.Connection.Command(insertSQL)) // Not using factories because of performance for batch operations
				{
					insertCommand.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, CurrentCompany.PK.ToGuid());
					insertCommand.AddParameter("@batchNo", SqlDbType.Int, newBatchNumber);
					insertCommand.AddParameter("@createdTime", SqlDbType.SmallDateTime, ZDateTime.UtcNow.ToDateTime());
					insertCommand.AddParameter("@createdUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
					insertCommand.AddParameter("@governmentAllocatedNumber", SqlDbType.VarChar, governmentAllocatedNumber);
					insertCommand.AddParameter("@batchStatus", SqlDbType.VarChar, batchStatus);

					using (var reader = insertCommand.ExecuteReader())
					{
						while (reader.Read())
						{
							batchPk = reader.GetGuid(0);
						}
					}
				}
				return batchPk;
			}
		}

		protected bool TryToCreateBatchForAPTransactionsRequest(ILogger serviceLogger)
		{
			if (string.IsNullOrEmpty(APTransactionListRequestBatchId.Value))
			{
				return false;
			}

			serviceLogger.Log(LogType.Debug, $"Start creating AP transaction list batch for company {CurrentCompany.GC_Code}.");

			var latestTimeToCreatePILBatch = ZDateTime.UtcNow.AddMinutes(-AccountingMasterFilesRegistry.Instance.APListAutomatedRequestSchedule.GetValueWithoutFallback(CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			var factory = new BusinessObjectFactory();

			if (checkPILActiveBatch())
			{
				using (var transaction = Db.Connection.BeginTransactionWithManager()) // Not using factories because of performance for batch operations
				{
					var newbatchPk = CreateBatch(APTransactionListRequestBatchId.Value, EInvoicingBatchState.Ready);
					transaction.CommitTransaction();
					if (!newbatchPk.IsEmpty)
					{
						serviceLogger.Log(LogType.Debug, "AP transaction list request batch has been created.");
						return true;
					}
				}
			}

			bool checkPILActiveBatch()
			{
				var query = new ZQuery(AccEInvoicingBatchSchema.AIB_GovernmentAllocatedNumber, APTransactionListRequestBatchId.ToString())
					.AddToFilter(AccEInvoicingBatchSchema.AIB_GC, CurrentCompany.PK)
					.AddToFilter(AccEInvoicingBatchSchema.AIB_SystemLastEditTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, latestTimeToCreatePILBatch.ToDateTime());
				var batch = factory.LoadTop1<AccEInvoicingBatch>(query);

				return batch == null;
			}

			serviceLogger.Log(LogType.Debug, "No AP transaction list request batch was created.");
			return false;
		}

		protected virtual void UpdateTransactionPivots(IEnumerable<Guid> transactionPivotPks, Guid batchPk, string defaultPivotStatus = EInvoicingPivotState.Batched)
		{
			if (!Db.Connection.IsInTransaction)
			{
				ErrorReporter.ReportOnce("Database connection is not active.");
				return;
			}

			if (batchPk != Guid.Empty)
			{
				var updateSQL = @"UPDATE dbo.AccEInvoicingTransactionPivot SET AIP_AIB = @batchPK, AIP_Status = @pivotStatus, AIP_SystemLastEditTimeUtc = GETUTCDATE(), AIP_SystemLastEditUser = @SystemLastEditUser
								WHERE AIP_PK IN (SELECT value FROM @pivotPKs)";
				using (var updateCommand = Db.Connection.Command(updateSQL)) // Not using factories because of performance for batch operations
				{
					updateCommand.AddParameter("@batchPK", SqlDbType.UniqueIdentifier, batchPk);
					updateCommand.AddParameter("@pivotStatus", SqlDbType.Char, defaultPivotStatus);
					updateCommand.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
					updateCommand.AddTableValuedParameter("@pivotPKs", AccEInvoicingTransactionPivotSchema.PK, transactionPivotPks);
					updateCommand.ExecuteNonQuery();
				}
			}
		}

		protected bool IsThrottlingEnabledForCompany => MaximumNumberOfInvoicesThatCanBeProcessedSimultaneously > 0;

		protected int MaximumNumberOfInvoicesThatCanBeProcessedSimultaneously => AccountingElectronicMessagingRegistry.Instance.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.GetValueWithoutFallback(CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

		protected DisposableAction SwitchCompanyContextTemporarilyIfRequires()
		{
			IDisposable tempContext = null;

			Action createAction = () =>
			{
				if (CurrentCompany.PK.ToGuid() != EnvProxy.Instance.CurrentCompany.PK)
				{
					var branchPK = GetTop1Branch();
					tempContext = Env.SetTemporaryUserContext(Env.CurrentUser.PK, branchPK, GlbDepartment.CurrentDepartment.PK.ToGuid());
				}
			};

			Action disposeAction = () => tempContext?.Dispose();

			return new DisposableAction(createAction, disposeAction);

			Guid GetTop1Branch()
			{
				using (var selectCommand = Db.Connection.Command("SELECT TOP 1 GB_PK FROM dbo.GlbBranch WHERE GB_GC = @companyPK")) // Not using factories because of performance for batch operations
				{
					selectCommand.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, CurrentCompany.PK.ToGuid());
					return (Guid)selectCommand.ExecuteScalar();
				}
			}
		}

		protected IEnumerable<DynamicBusinessObject> GroupTransactionByUniqueIdentifierForBatching(IEnumerable<DynamicBusinessObject> transactions)
		{
			var uniqueIdentifier = (ITransactionInfoHelper)new TransactionInfoHelper();
			var result = transactions.Select(t =>
									new
									{
										Key = uniqueIdentifier.GetUniqueIdentifierWithinBatch(
											(ZString)t[AccTransactionHeaderSchema.Constants.AH_Ledger],
											(ZString)t[AccTransactionHeaderSchema.Constants.AH_TransactionType],
											(ZString)t[OrgHeaderSchema.Constants.OH_Code],
											(ZString)t[AccTransactionHeaderSchema.Constants.AH_TransactionNum]),
										Transaction = t
									});
#if NETFRAMEWORK
			result = result.DistinctBy(d => d.Key);
#elif NET
			result = Enumerable.DistinctBy(result, d => d.Key);
#endif
			return result.Select(t => t.Transaction);
		}

		protected class QueuedPivotPKs
		{
			public QueuedPivotPKs()
			{
				Items = new List<Guid>();
			}

			public void Add(DynamicBusinessObject dynamicBizO)
			{
				if (dynamicBizO != null && dynamicBizO["AIP_PK"] != null)
				{
					Items.Add(new Guid(dynamicBizO["AIP_PK"].ToString()));
				}
			}

			public List<Guid> Items { get; }

			public static QueuedPivotPKs FromDynamicBizOCollection(IEnumerable<DynamicBusinessObject> bizOCollection)
			{
				var queuedPivotPKs = new QueuedPivotPKs();
				bizOCollection.ToList().ForEach(t => queuedPivotPKs.Add(t));
				return queuedPivotPKs;
			}
		}
	}

	#endregion
}
