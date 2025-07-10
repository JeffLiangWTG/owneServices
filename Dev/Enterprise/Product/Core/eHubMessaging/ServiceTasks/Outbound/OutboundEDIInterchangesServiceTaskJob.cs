using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.ErrorManagement;
using CargoWise.ComponentModel;
using CargoWise.Data.Utils;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	abstract class OutboundEDIInterchangesServiceTaskJob : OutboundServiceTaskJob<EDIInterchange, LightweightOutboundInterchangeCandidate>
	{
		protected OutboundEDIInterchangesServiceTaskJob(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier, IAdaptorFactory adaptorFactory = null, IZQueryFactory queryFactory = null, IDynamicBusinessObjectCollectionFactory dynamicBizoFactory = null)
			: base(serviceTaskSupport, notifier, adaptorFactory)
		{
			QueryFactory = queryFactory ?? new ZQueryFactory();
			DynamicBizoFactory = dynamicBizoFactory ?? new DynamicBusinessObjectCollectionFactory();
		}

		protected abstract string PendingItemsIndex { get; }

		internal IZQueryFactory QueryFactory { get; private set; }

		internal IDynamicBusinessObjectCollectionFactory DynamicBizoFactory { get; private set; }

		internal string CurrentRecipient { get; set; }

		protected internal Dictionary<string, object> CurrentBatchCriteria { get; private set; }

		internal SqlApplicationLock CurrentBatchLock { get; set; }

		protected internal abstract string InterchangeQueuedStatus { get; }

		internal abstract string InterchangeSuccessStatus { get; }

		internal virtual TimeSpan InterchangeLockTimeout => TimeSpan.FromMinutes(5);

		protected abstract BillingDataSource BillingDataSourceCode { get; }

		protected abstract BillingInterfaceName BillingInterfaceName { get; }

		protected internal virtual IEnumerable<SchemaColumn> ExtraColumnsToBatchPendingInterchangesBy
		{
			get
			{
				yield break;
			}
		}

		protected virtual bool ShouldHoldLocksUntilAllBatchesAreProcessed => true;

		internal override void ProcessMessagesCore()
		{
			var itemsToProcess = PendingItemsBatchSize;
			while (itemsToProcess > 0)
			{
				var maxItemsThisBatch = Math.Min(AdapterOutboxCountLimit, itemsToProcess);
				var pendingItems = GetPendingItems(maxItemsThisBatch);
				NotifyCandidateCount(pendingItems.Count);
				if (pendingItems.Count <= 0)
				{
					return;
				}

				var items = ValidatePendingItems(pendingItems);
				items = CheckFeatureControl(items);
				NotifyOutboundCount(items.Count);
				if (items.Count > 0)
				{
					SendBatch(items);
				}

				OnAtLeastOneMessageProcessed();
				itemsToProcess -= pendingItems.Count;

				if (pendingItems.Count < maxItemsThisBatch)
				{
					return;
				}
			}
		}

		internal virtual IReadOnlyCollection<LightweightOutboundInterchangeCandidate> CheckFeatureControl(IReadOnlyCollection<LightweightOutboundInterchangeCandidate> items)
		{
			return items;
		}

		internal virtual bool TryGetExceptionMessageForInterchange(eHubAdapterException ex, EDIInterchange interchange, out string exceptionMessage)
		{
			exceptionMessage = null;

			if (Faults.ContainsKey(interchange.EI_SessionGUID.ToGuid()))
			{
				exceptionMessage = Faults[interchange.EI_SessionGUID.ToGuid()];
				if (string.IsNullOrWhiteSpace(exceptionMessage))
				{
					exceptionMessage = ex.Message;
				}
			}

			return exceptionMessage != null;
		}

		protected internal virtual IeHubMessage CreateeHubMessage(EDIInterchange interchange)
		{
			return EHubMessageDirector.CreateMessage(interchange, Notifier);
		}

		internal override IReadOnlyList<KeyValuePair<LightweightOutboundInterchangeCandidate, IeHubMessage>> CreateMessages(IReadOnlyCollection<LightweightOutboundInterchangeCandidate> items)
		{
			var messages = new List<KeyValuePair<LightweightOutboundInterchangeCandidate, IeHubMessage>>();

			foreach (var item in items)
			{
				var pair = new KeyValuePair<LightweightOutboundInterchangeCandidate, IeHubMessage>(item, CreateeHubMessage(item.FullItem));
				if (pair.Value != null)
				{
					messages.Add(pair);
				}
				else
				{
					SaveSend(new[] { pair });
				}
			}

			return messages.AsReadOnly();
		}

		protected override void AddMessageToOutbox(IMessageOutbox outbox, KeyValuePair<LightweightOutboundInterchangeCandidate, IeHubMessage> messagePair)
		{
			base.AddMessageToOutbox(outbox, messagePair);
			candidatesInOutbox.Add(messagePair.Key);
		}

		internal override void ExecuteInternal()
		{
			var branchRecipientPairs = GetBatchingCriteriaForTopPendingInterchanges();
			if (branchRecipientPairs.Count <= 0)
			{
				NotifyCandidateCount(branchRecipientPairs.Count);
				return;
			}

			var recipientLocks = new DisposableList(0);
			ExceptionAggregation.Using(recipientLocks, () =>
			{
				var groupedByBranch = branchRecipientPairs.GroupBy(i => (ZGuid)i[EDIInterchangeSchema.Constants.EI_GB]);
				foreach (var branchGroup in groupedByBranch)
				{
					CurrentBranchPk = branchGroup.Key;
					using (DisposableEnvironment.ForBranch(CurrentBranchPk.ToGuid(), reportInactive: false))
					{
						NotifyExecutingForCompany();

						try
						{
							foreach (var batch in branchGroup)
							{
								var recipient = batch[EDIInterchangeSchema.Constants.EI_To].ToString();
								if (TryAcquireRecipientLock(branchGroup.Key, recipient, batch, out var recipientMutex))
								{
									if (ShouldHoldLocksUntilAllBatchesAreProcessed)
									{
										recipientLocks.Add(recipientMutex);
									}
									using (ShouldHoldLocksUntilAllBatchesAreProcessed ? null : recipientMutex)
									{
										SetCurrentCriteriaAndProcessMessages(recipientMutex, batch, recipient);
									}
									AfterProcessingBatch?.Invoke(batch);
								}
							}
						}
						catch (Exception ex) when (HandleCompanyLevelException(ex, ref recipientLocks))
						{
						}
					}
				}
			});
		}

		void SetCurrentCriteriaAndProcessMessages(SqlApplicationLock recipientMutex, Dictionary<string, object> batch, string recipient)
		{
			CurrentBatchLock = recipientMutex;
			CurrentBatchCriteria = batch;
			CurrentRecipient = recipient;

			ProcessMessages();
		}

		public event Action<Dictionary<string, object>> AfterProcessingBatch;

		bool TryAcquireRecipientLock(ZGuid branchPK, string recipient, Dictionary<string, object> allBatchingCriteria, out SqlApplicationLock recipientMutex)
		{
			var extraLockParams = string.Join(string.Empty, ExtraColumnsToBatchPendingInterchangesBy.Select(c => $"_{allBatchingCriteria[c.Name]?.ToString() ?? (NoResString)"(null)"}"));

			if (DbConnection.TryGetLock($"{MutexPrefix}{branchPK}_{recipient}{extraLockParams}", out recipientMutex))
			{
				return true;
			}

			if (string.IsNullOrEmpty(extraLockParams))
			{
				NotifyVerbose(string.Format(CultureInfo.CurrentCulture, Res.GetString("FA099D59-25A8-4874-9443-9F0B3695907F", "Skipped branch [{0}] and recipient [{1}] as this was already being processed.", branchPK, recipient)));
			}
			else
			{
				NotifyVerbose(string.Format(CultureInfo.CurrentCulture, Res.GetString("53F914D2-2664-486E-9309-CE3A6E589697", "Skipped branch [{0}], recipient [{1}], and extra parameters [{2}] as this was already being processed.", branchPK, recipient, extraLockParams)));
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		internal virtual IReadOnlyCollection<Dictionary<string, object>> GetBatchingCriteriaForTopPendingInterchanges()
		{
			var extraColumns = string.Join(string.Empty, ExtraColumnsToBatchPendingInterchangesBy.Select(c => ", " + c.Name));
			var sql = string.Format(CultureInfo.InvariantCulture, $@"
WITH CTE AS
(
SELECT TOP {PendingItemsSearchLimit}
	{EDIInterchangeSchema.Constants.EI_GB},
	{EDIInterchangeSchema.Constants.EI_To}
	{extraColumns}
FROM {EDIInterchangeSchema.Constants.SqlSchemaName}.{EDIInterchangeSchema.Constants.TableName} WITH (INDEX={PendingItemsIndex})
WHERE {FilterForAllCompanies}
ORDER BY {EDIInterchangeSchema.Constants.EI_SystemCreateTimeUtc} ASC
)
SELECT 
	DISTINCT {EDIInterchangeSchema.Constants.EI_GB}, {EDIInterchangeSchema.Constants.EI_To}{extraColumns}
FROM CTE");
			var sqlParams = GetParametersForFilterForAllCompanies();
			var queryResult = DynamicBizoFactory.Create(FactoryProvider.Current);
			queryResult.Load(sql, sqlParams);
			NotifyVerbose(Res.GetString("43faffa6-7447-42d8-a909-6e0fa7e93afb", "{0} Batching Criteria Records Fetched", queryResult.Count));
			return queryResult.Cast<DynamicBusinessObject>().Select(ToDictionary).ToList().AsReadOnly();

			Dictionary<string, object> ToDictionary(DynamicBusinessObject bizo)
			{
				var result = new Dictionary<string, object>();

				result[EDIInterchangeSchema.Constants.EI_GB] = bizo[EDIInterchangeSchema.Constants.EI_GB];
				result[EDIInterchangeSchema.Constants.EI_To] = bizo[EDIInterchangeSchema.Constants.EI_To];

				foreach (var column in ExtraColumnsToBatchPendingInterchangesBy)
				{
					result[column.Name] = bizo[column.Name];
				}

				return result;
			}
		}

		internal abstract int PendingItemsBatchSize { get; }
		internal abstract int PendingItemsSearchLimit { get; }

		string FilterForAllCompanies
		{
			get
			{
				return string.Format(CultureInfo.InvariantCulture, (NoResString)@"
	{0} = @ActiveStatus
	AND {1} = @Transmit
	AND {2} = @InterchangeStatus
	AND NOT ({3} > 0 AND {3} <= 5 AND {4} > @LowRetryTimeLimit)
	AND NOT ({3} > 5 AND {4} > @HighRetryTimeLimit)
	AND {5}",
					EDIInterchangeSchema.Constants.EI_IsActive,
					EDIInterchangeSchema.Constants.EI_ReceiveTransmit,
					EDIInterchangeSchema.Constants.EI_Status,
					EDIInterchangeSchema.Constants.EI_RetryCount, EDIInterchangeSchema.Constants.EI_SystemLastEditTimeUtc,
					GetBranchRecipientPairsExtraCondition);
			}
		}

		ZSqlParameterCollection GetParametersForFilterForAllCompanies()
		{
			var sqlParams = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@ActiveStatus", ZBool.True, EDIInterchangeSchema.EI_IsActive),
				ZSqlParameter.New("@Transmit", EDIInterchange.Direction.Transmit, EDIInterchangeSchema.EI_ReceiveTransmit),
				ZSqlParameter.New("@InterchangeStatus", InterchangeQueuedStatus, EDIInterchangeSchema.EI_Status),
				ZSqlParameter.New("@LowRetryTimeLimit", ZDateTime.UtcNow.AddMinutes(-5), EDIInterchangeSchema.EI_SystemLastEditTimeUtc),
				ZSqlParameter.New("@HighRetryTimeLimit", ZDateTime.UtcNow.AddHours(-1), EDIInterchangeSchema.EI_SystemLastEditTimeUtc),
			};
			GetBranchRecipientPairsExtraParameters(sqlParams);
			return sqlParams;
		}

		internal override GlbCompany CurrentCompany
		{
			get { return base.CurrentCompany; }
			set
			{
				throw new NotSupportedException("Cannot set CurrentCompany directly for outbound interchange, need to set the CurrentBranchPk instead");
			}
		}

		internal ZGuid CurrentBranchPk
		{
			get { return currentBranchPk; }
			set
			{
				currentBranchPk = value;
				if (value.IsValid)
				{
					base.CurrentCompany = Companies.FirstOrDefault(_ => _.Branches.FirstOrDefault(b => b.PK == value) != null);
					if (CurrentCompany == null)
					{
						ServiceTaskSupport.CompanySettingsManager.ClearCache();
						base.CurrentCompany = Companies.FirstOrDefault(_ => _.Branches.FirstOrDefault(b => b.PK == value) != null);
					}
				}
				else
				{
					base.CurrentCompany = null;
				}
			}
		}

		internal virtual IReadOnlyCollection<LightweightOutboundInterchangeCandidate> GetPendingItems(int maxItems)
		{
			NotifyVerbose(Res.GetString("85BE87D2-842B-4850-B10A-D78E3C52CB04", "calling Send Gateway Messages()"));

			FactoryProvider.CreateNewWithoutSave();
			var zQuery = QueryFactory.Create(EDIInterchangeSchema.EI_IsActive, ZBool.True);
			zQuery.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			zQuery.AddToFilter(EDIInterchangeSchema.EI_Status, InterchangeQueuedStatus);

			zQuery.AddToFilter(EDIInterchangeSchema.EI_To, CurrentRecipient);
			zQuery.AddToFilter(EDIInterchangeSchema.EI_GB, CurrentBranchPk);

			foreach (var column in ExtraColumnsToBatchPendingInterchangesBy)
			{
				var value = CurrentBatchCriteria[column.Name];
				if (column.ColumnType == SchemaColumnType.Guid && value is ZGuid guid && guid == ZGuid.Empty)
				{
					zQuery.AddToFilter(column, null);
				}
				else
				{
					zQuery.AddToFilter(column, value);
				}
			}

			var lowRetryQuery = new ZQuery(EDIInterchangeSchema.EI_RetryCount, SQLComparisonOperator.LessThanOrEqualTo, 0);
			lowRetryQuery.AddToFilter(JoinCondition.Or, EDIInterchangeSchema.EI_RetryCount, SQLComparisonOperator.GreaterThan, 5);
			lowRetryQuery.AddToFilter(JoinCondition.Or, EDIInterchangeSchema.EI_SystemLastEditTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow.AddMinutes(-5));
			zQuery.AddToFilter(lowRetryQuery);
			var highRetryQuery = new ZQuery(EDIInterchangeSchema.EI_RetryCount, SQLComparisonOperator.LessThanOrEqualTo, 5);
			highRetryQuery.AddToFilter(JoinCondition.Or, EDIInterchangeSchema.EI_SystemLastEditTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow.AddHours(-1));
			zQuery.AddToFilter(highRetryQuery);
			GetPendingItemsAddExtraFilters(zQuery);
			zQuery.MaximumRows = maxItems;
			zQuery.OrderBy = EDIInterchangeSchema.Constants.EI_SystemCreateTimeUtc;
			zQuery.TableIndexHints.Add(new TableIndexHint(PendingItemsIndex));

			var queryResult = FactoryProvider.Current.Load<EDIInterchange>(zQuery);
			var result = queryResult.Select(i => new LightweightOutboundInterchangeCandidate(i)).ToList();
			NotifyVerbose(Res.GetString("d6271c23-0769-43fc-bea3-670ca821d412", "{0} messages Fetched", result.Count));
			return result.AsReadOnly();
		}

		protected virtual string GetBranchRecipientPairsExtraCondition
		{
			get { return "1 = 1"; }
		}

		protected virtual void GetBranchRecipientPairsExtraParameters(ZSqlParameterCollection collection)
		{
		}

		protected virtual void GetPendingItemsAddExtraFilters(ZQuery pendingItemsQuery)
		{
		}

		protected override void FailInterchanges(IEnumerable<EDIInterchange> items)
		{
			foreach (var interchange in items)
			{
				new MessageStatusManager(interchange).Update(EDIInterchangeStatusList.Codes.Failed, EDIInterchangeStatusList.Codes.Failed);
			}

			FactoryProvider.SaveCurrentAndCreateNew();
			Notifier.Notify(new WarningNotification(WarningType.Warning, Res.GetString("f21d10b8-4b9b-45f3-8d3e-3028ce8a88da", "interchange(s) Rejected as from Demo Company.")));
		}

		internal override void CallAdapterToSendMessages(IeHubAdapter adapter)
		{
			ThrowIfCancellationRequested();
			try
			{
				NotifyVerbose(Res.GetString("dd5db9c4-da95-4698-82b4-ccbbd5ffb6fd", "Call adapter to send messages, Current out-box Size: {0}", adapter.Outbox.SizeInKiloBytes));
				NotifyVerbose(Res.GetString("213f4c03-809c-4fc9-b026-d3b3e44c729d", "Call adapter to send messages, Current out-box Count: {0}", adapter.Outbox.Count));
				adapter.SendMessages();
				Notifier.Notify(new InfoNotification(Res.GetString("2370fa5e-ca57-47fc-a78d-4e5e95a2781a", "{0} interchange(s) sent to {1}.", candidatesInOutbox.Count, ServiceTaskSupport.ServiceTaskName)));
			}
			catch (eHubAdapterException ex)
			{
				var failedInterchangeList = new List<KeyValuePair<EDIInterchange, string>>();
				if (ex.GetMessageExceptionDictionary().IsNullOrEmpty())
				{
					var errorMessage = string.Empty;
					if (IsIndividualMessageExceedingMaxReceiveError(ex, adapter.Outbox, ref errorMessage))
					{
						var ehubMessage = adapter.Outbox.First();
						AddFailedInterchange(ehubMessage.TrackingID, errorMessage, ex, failedInterchangeList);
					}
					else
					{
						throw;
					}
				}
				else
				{
					ex.GetMessageExceptionDictionary().ForEach(x => AddFailedInterchange(x.Key, x.Value, ex, failedInterchangeList));
				}

				Notifier.Notify(new ErrorNotification(ErrorType.Error, eHubServiceTask.GetErrorLog(ex, Res.GetString("72830c4d-74fd-4717-9d06-e0ac1a5c1c4e", "{0} interchange(s) sent to {2}. {1} interchange(s) failed to send to {2}.", candidatesInOutbox.Count - failedInterchangeList.Count, failedInterchangeList.Count, ServiceTaskSupport.ServiceTaskName))));
			}
		}

		void AddFailedInterchange(Guid key, string errorMessage, eHubAdapterException ex, List<KeyValuePair<EDIInterchange, string>> failedInterchangeList)
		{
			errorMessage = errorMessage.IsNullOrEmpty() ? ex.Message : errorMessage;
			failedInterchangeList.Add(new KeyValuePair<EDIInterchange, string>(candidatesInOutbox.First(c => c.SessionGuid == key).FullItem, errorMessage));
			Faults.Add(key, errorMessage);
		}

		protected override void SaveSendCore(IEnumerable<KeyValuePair<LightweightOutboundInterchangeCandidate, IeHubMessage>> messages)
		{
			foreach (var pair in messages)
			{
				var interchange = pair.Key.FullItem;

				if (pair.Value == null)
				{
					NotifyVerbose(Res.GetString("1b74054f-7493-4fac-bc99-bd60a665c71a", "Preparing message for interchange PK {0} failed.", interchange.PK.ToString()));
					new MessageStatusManager(interchange).Update(EDIInterchangeStatusList.Codes.Failed, EDIInterchangeStatusList.Codes.Failed);
				}
				else
				{
					FinalizeInterchange(interchange);
				}
			}

			FactoryProvider.SaveCurrentAndUpdateRecordCounts();
		}

		protected override void FinalizeSend(IeHubAdapter adapter)
		{
			base.FinalizeSend(adapter);
			candidatesInOutbox.Clear();
		}

		protected virtual void FinalizeInterchange(EDIInterchange interchange)
		{
			if (!interchange.EI_Status.ToString().Equals(InterchangeQueuedStatus, StringComparison.InvariantCultureIgnoreCase))
			{
				NotifyVerbose(Res.GetString("533C9BB0-4F25-4F40-AF06-F03A28D797F1", "Interchange PK {0} cannot be finalized after sending as it is no longer in the queue.  It was most likely sent more than once and has already been acknowledged so it can be ignored.", interchange.PK.ToString()));
				return;
			}

			if (Faults.TryGetValue(interchange.EI_SessionGUID.ToGuid(), out var errorMessage))
			{
				new MessageStatusManager(interchange).Fail(Res.GetString("74E922C7-2856-482F-BD2C-201B913F12C5", "{0} Error: {1}", ServiceTaskSupport.ServiceTaskName, errorMessage));
			}
			else
			{
				new MessageStatusManager(interchange).Update(InterchangeSuccessStatus);
			}
		}

		internal BusinessObjectFactoryProvider FactoryProvider
		{
			get
			{
				if (factoryProvider == null)
				{
					factoryProvider = new BusinessObjectFactoryProvider(DbConnection);
					factoryProvider.Current.RefreshEnabled = false;
				}
				return factoryProvider;
			}
		}
		BusinessObjectFactoryProvider factoryProvider;
		protected readonly List<LightweightOutboundInterchangeCandidate> candidatesInOutbox = new List<LightweightOutboundInterchangeCandidate>();
		ZGuid currentBranchPk;
	}
}
