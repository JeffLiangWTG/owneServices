#region SuppressResourceStringsCheckRegion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Billing;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.Billing.StlCollector.Retriever
{
	class BillingDataCollector
	{
		readonly protected ILogger generalLogger;
		readonly IUserAttendedStlRetrieverLogger userAttendedLogger;
		protected readonly BusinessObjectFactory bizoFactory;
		readonly BillingTransactionFactory transactionFactory;

		public BillingDataCollector(ILogger logger)
			: this(logger, new BusinessObjectFactory(), new ScriptLoader(logger))
		{
		}

		public BillingDataCollector(ILogger logger, BusinessObjectFactory bizoFactory, IScriptLoader scriptLoader)
			: this(logger, bizoFactory, scriptLoader.Load(bizoFactory))
		{
		}

		public BillingDataCollector(ILogger logger, BusinessObjectFactory bizoFactory, IEnumerable<IStlScriptWithConfig> stlScripts)
		{
			ValidateLogger(logger);
			generalLogger = logger;
			userAttendedLogger = (logger as IUserAttendedStlRetrieverLogger);
			Scripts = stlScripts;
			this.bizoFactory = bizoFactory;
			transactionFactory = new BillingTransactionFactory();
		}

		public IEnumerable<IStlScriptWithConfig> Scripts { get; private set; }

		public IEnumerable<IStlScriptWithConfig> MandatoryScripts => Scripts.Where(s => s.Script.IsMandatoryForMilestones);

		void ValidateLogger(ILogger logger)
		{
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}
		}

		public void CollectAndSendData(CancellationToken token, RecurringRange collectionRange, bool lastCollection = false)
		{
			CheckRangeIsValid(collectionRange);

			var scriptsToCollect = Scripts.Where(i => i.NextStartTimeUtc < collectionRange.EndDateTimeExclusive && i.Script.CollectionException == null).Select(i => new KeyValuePair<IStlScriptWithConfig, RecurringRange>(i, GetItemRange(i, collectionRange))).ToArray();
			var noMandatoryWatermarksSet = MandatoryScripts.All(i => !i.HighWaterMarkSettings.HasHighWaterMarkBeenSet);
			var mandatoryScriptsToCollect = scriptsToCollect.Where(i => i.Key.Script.IsMandatoryForMilestones);
			var scriptsToCollectBeforeGeneratingMilestone = (noMandatoryWatermarksSet ? mandatoryScriptsToCollect : mandatoryScriptsToCollect.Where(i => i.Key.HighWaterMarkSettings.HasHighWaterMarkBeenSet && i.Value.ScriptShouldBeRun(i.Key))).Select(i => i.Key).ToArray();
			var milestoneReported = false;

			foreach (var stlItemWithRange in scriptsToCollect)
			{
				token.ThrowIfCancellationRequested();

				var stlItem = stlItemWithRange.Key;
				var itemDateRange = stlItemWithRange.Value;
				IEnumerable<IStlTransaction> transactionsSent = null;
				if (itemDateRange.ScriptShouldBeRun(stlItem))
				{
					var sendTransactions = itemDateRange.ScriptResultsShouldBeSubmitted(stlItem);
					var transactionsCollected = CollectStlItemTransactions(stlItem, itemDateRange, sendTransactions, bizoFactory);
					transactionsSent = sendTransactions ? transactionsCollected : null;
				}

				if (stlItem.Script.CollectionException == null)
				{
					if (stlItem.Script.StlGrain == StlDataGrain.Snapshot && (lastCollection || stlItem.Script.CollectionStartDateUtc == DateTime.MinValue))
					{
						stlItem.HighWaterMarkSettings.HighWaterMark = DateTime.MaxValue;
					}
					else
					{
						stlItem.HighWaterMarkSettings.HighWaterMark = itemDateRange.EndDateTimeExclusive;
					}

					var milestoneReportedWithThisItem = false;
					if (!milestoneReported &&
						collectionRange.StlMilestoneTimestamp.HasValue &&
						stlItem.Script.IsMandatoryForMilestones &&
						stlItem.Script.CollectionOccurred &&
						scriptsToCollectBeforeGeneratingMilestone.All(i => i.NextStartTimeUtc >= collectionRange.StlMilestoneTimestamp.Value))
					{
						AddStlDailyMilestone(collectionRange, bizoFactory);
						milestoneReportedWithThisItem = true;
						milestoneReported = true;
					}

					bizoFactory.Save();

					if (transactionsSent != null && transactionsSent.Any())
					{
						LogUserAttendedInfo(string.Format(CultureInfo.InvariantCulture, "- {0} transaction(s) sent", transactionsSent.Count()));
					}
					if (milestoneReportedWithThisItem)
					{
						LogStlDailyMilestone(collectionRange);
					}
				}
			}
		}

		static RecurringRange GetItemRange(IStlScriptWithConfig stlScriptWithConfig, RecurringRange collectionRange)
		{
			if (stlScriptWithConfig.NextStartTimeUtc > collectionRange.StartDateTimeInclusive)
			{
				return new RecurringRange(stlScriptWithConfig.NextStartTimeUtc, collectionRange.EndDateTimeExclusive);
			}

			return collectionRange;
		}

		public IEnumerable<IStlTransaction> CollectAndSendMonthData(CancellationToken token, AusydMonthRange monthRange, string specificCodeToCollect)
		{
			var allTransactionsCollected = new List<IStlTransaction>();
			CheckRangeIsValid(monthRange);

			var stlScriptsToCollect = monthRange.SelectApplicableScripts((specificCodeToCollect == null) ? Scripts : Scripts.Where(s => s.Script.Code == specificCodeToCollect));
			if (stlScriptsToCollect.Any())
			{
				InitProgress(stlScriptsToCollect.Count() + 1);

				var bizoFactory = new BusinessObjectFactory(Db.Connection);
				foreach (var stlItem in stlScriptsToCollect)
				{
					token.ThrowIfCancellationRequested();
					var transactionsCollected = CollectStlItemTransactions(stlItem, monthRange, sendTransactions: true, bizoFactory);
					bizoFactory.Save();
					if (transactionsCollected != null && transactionsCollected.Any())
					{
						allTransactionsCollected.AddRange(transactionsCollected);
						LogUserAttendedInfo(string.Format(CultureInfo.InvariantCulture, "- {0} transaction(s) sent", transactionsCollected.Count()));
					}
				}

				// If collecting all items (no specificCodeToCollect), add STL milestone for the last day of the month.
				if (monthRange.StlMilestoneTimestamp.HasValue && specificCodeToCollect == null && MandatoryScripts.Any(i => i.Script.CollectionOccurred))
				{
					AddStlDailyMilestone(monthRange, bizoFactory);
					bizoFactory.Save();
					LogStlDailyMilestone(monthRange);
				}
			}
			else if (specificCodeToCollect != null)
			{
				throw new BillingException(string.Format(CultureInfo.CurrentCulture, "No script found for {0}.", specificCodeToCollect));
			}

			return allTransactionsCollected;
		}

		void CheckRangeIsValid(IDateTimeRange range)
		{
			if (!range.IsValid)
			{
				throw new BillingException(string.Format(CultureInfo.CurrentCulture, "Attempt to collect data for an invalid date range: {0}.", range.ToString()));
			}
		}

		IEnumerable<IStlTransaction> CollectStlItemTransactions(IStlScriptWithConfig stlItem, IDateTimeRange dateTimeRange, bool sendTransactions, BusinessObjectFactory bizoFactory)
		{
			try
			{
				LogUserAttendedStep("Collecting - " + stlItem.Script.Code + " - " + stlItem.Script.Feature);
				var stlTransactions = RetrieveFeatureData(stlItem.Script, dateTimeRange);
				if (sendTransactions && stlTransactions.Any())
				{
					SendTransactions(stlTransactions, stlItem.Script.IsMandatoryForMilestones ? SubmissionPriority.MandatoryForMilestone : SubmissionPriority.NotMandatoryForMilestone, bizoFactory);
				}

				stlItem.Script.CollectionOccurred = true;
				return stlTransactions;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var message = string.Format(CultureInfo.CurrentCulture, "Data collection failed for item [{0}].\r\n{1}", stlItem.Script.Code, ex.Message);
				stlItem.Script.CollectionException = new BillingException(message, ex);
				ReportCollectionException(stlItem);
				return null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System Service Task")]
		void ReportCollectionException(IStlScriptWithConfig stlItem)
		{
			var ex = stlItem.Script.CollectionException;
			if (IsKnownException(ex))
			{
				Log(LogType.Warning, $"Known collection exception skipped for reporting. Message: {ex}");
			}
			if (stlItem.IsExceptionWithinThreshold(ExceptionThreshold))
			{
				Log(LogType.Warning, ex.ToString());
			}
			else
			{
				var message = "Error collecting STL data: " + ex.Message;
				generalLogger.Log(LogType.Error, message, ex);
				var productRegistration = ObjectFactory.Get<IProductRegistration>();
				if (!productRegistration.IsWiseTechGlobalInternalSystem())
				{
					if (ex.InnerException is RefDataException refException)
					{
						if (refException.ReportIssue)
						{
							ExceptionReporter.Instance.ReportDeveloperException(message, message, ex);
						}
					}
					else
					{
						ExceptionReporter.Instance.ReportDeveloperException(message, message, ex);
					}
				}
			}
		}

		/// <summary>
		/// If date range crosses an AUSYD date boundary => Send dummy STL Milestone transaction.
		/// </summary>
		void AddStlDailyMilestone(IDateTimeRange range, BusinessObjectFactory bizoFactory)
		{
			var milestoneDateAsString = SqlFormatInfo.ToSqlDateString(range.StlMilestoneTimestamp.Value.Date);
			var milestoneType = (range is AusydMonthRange) ? "MONTH" : "DAY";
			var additionalRefsJson = new JObject();
			var manadoryScriptsArray = new JArray();
			foreach (var code in MandatoryScripts.Select(s => s.Script.Code))
			{
				manadoryScriptsArray.Add(code);
			}
			additionalRefsJson.Add("MandatoryItemsCount", MandatoryScripts.Count());
			additionalRefsJson.Add("CargoWiseVersion", ReleaseInfo.Instance.VersionNumber.ToString());
			additionalRefsJson.Add("BilledPriceItemCodes", manadoryScriptsArray);
			var additionalRefs = JsonConvert.SerializeObject(additionalRefsJson, Formatting.Indented);

			var stlMilestoneTransaction = transactionFactory.CreateTransaction("STL", 1, range.StlMilestoneTimestamp.Value, milestoneDateAsString, reference2: milestoneType, additionalRefs: additionalRefs);
			SendTransactions(new IStlTransaction[] { stlMilestoneTransaction }, SubmissionPriority.MandatoryForMilestone, bizoFactory);
		}

		void LogStlDailyMilestone(IDateTimeRange range)
		{
			var milestoneDateAsString = SqlFormatInfo.ToSqlDateString(range.StlMilestoneTimestamp.Value.Date);
			var milestoneType = (range is AusydMonthRange) ? "MONTH" : "DAY";
			Log(LogType.Information, $"STL Milestone Created - {milestoneDateAsString} ({milestoneType}) MandatoryItemsCount({MandatoryScripts.Count()}) Version({ReleaseInfo.Instance.VersionNumber.ToString()})");
		}

		BillingManager BillingMgr
		{
			get
			{
				return billingMgr_UsePtyInstead ?? (billingMgr_UsePtyInstead = new BillingManager());
			}
		}
		BillingManager billingMgr_UsePtyInstead;

		void SendTransactions(IEnumerable<IStlTransaction> stlTransactions, SubmissionPriority submissionPriority, BusinessObjectFactory bizoFactory)
		{
			if (stlTransactions.First().GetType() == typeof(BillingTransaction))
			{
				BillingMgr.AddTransactions(stlTransactions.Select(t =>
				{
					if (!SkipValidation)
					{
						var billingTransaction = ToBillingAPITransaction((BillingTransaction)t);
						try
						{
							CargoWise.Billing.API.BillingTransactionValidator.ValidateTransaction(billingTransaction);
						}
						catch (CargoWise.Billing.API.ValidationException ex)
						{
							var sb = new StringBuilder();
							sb.AppendLine(ex.Message);
							ex.Errors.ForEach(error => sb.AppendLine(error));
							throw new BillingException(sb.ToString(), ex);
						}
					}
					return new BillingTransactionWrapper((BillingTransaction)t, submissionPriority);
				}), bizoFactory);
			}
			else
			{
				BillingMgr.AddTransactions(stlTransactions.Select(t => (UsageTransaction)t), bizoFactory);
			}
		}
		public static CargoWise.Billing.API.BillingTransaction ToBillingAPITransaction(BillingTransaction stlTransaction)
		{
			return new CargoWise.Billing.API.BillingTransaction
			{
				BillableCount = stlTransaction.BillableCount,
				ClientNumber = stlTransaction.ClientNumber,
				ClientID = stlTransaction.ClientID,
				ClientStaffCode = stlTransaction.ClientStaffCode,
				Category = stlTransaction.Category,
				PriceItemCode = stlTransaction.PriceItemCode,
				ReportingSource = stlTransaction.ReportingSource,
				ServiceOccuredUTC = stlTransaction.ServiceOccuredUTC,
				Reference1 = stlTransaction.Reference1,
				Reference2 = stlTransaction.Reference2,
				Reference3 = stlTransaction.Reference3,
				Reference4 = stlTransaction.Reference4,
				Reference5 = stlTransaction.Reference5,
				AdditionalRefs = stlTransaction.AdditionalRefs,
				Version = stlTransaction.Version,
				MessageTrackingID = stlTransaction.MessageTrackingID,
			};
		}

		#region Logging

		void InitProgress(int count)
		{
			if (userAttendedLogger != null)
			{
				userAttendedLogger.InitProgress(count.ToString(CultureInfo.CurrentCulture));
			}
		}

		void LogUserAttendedStep(string message)
		{
			if (userAttendedLogger != null)
			{
				userAttendedLogger.TaskProgress(message);
			}
			else
			{
				Log(LogType.Information, message);
			}
		}

		void LogUserAttendedInfo(string message)
		{
			if (userAttendedLogger != null)
			{
				userAttendedLogger.TaskInfo(message);
			}
			else
			{
				Log(LogType.Information, message);
			}
		}
		void Log(LogType type, string message)
		{
			generalLogger.Log(type, message);
		}

		#endregion

		/// <summary>
		/// PriceItemCode					VCHAR(3)	Billing Feature Code (price list)
		/// BillableCount					INT			Item count (subitems)
		/// ReportingSource					VCHAR(3)	To identify where this request came from = STL
		/// ServiceOccuredUTC				DATETIME	When the transaction/service occurred. Eg: create time of a shipment
		/// ClientID						VCHAR(9)	9-char Licence Code to bill to
		/// ClientNumber					VCHAR(?)	New invariant licence ID (CargoWise One)
		/// ClientStaffCode					VCHAR(3)	User who created the transaction, if available
		/// Reference1						VCHAR(50)	Primary Transaction Reference
		/// Reference2						VCHAR(50)	Secondary Transaction Reference (if required for transaction uniqueness)
		/// Reference3						VCHAR(50)	Additional Reference (if required for transaction uniqueness)
		/// Reference4						VCHAR(50)	Additional Reference (if required for transaction uniqueness)
		/// </summary>
		protected virtual IEnumerable<IStlTransaction> RetrieveFeatureData(IStlItem stlScript, IDateTimeRange dateTimeRange)
		{
			return stlScript.Run(dateTimeRange);
		}
		public virtual bool SkipValidation { get; set; }
		public virtual TimeSpan ExceptionThreshold
		{
			get
			{
				var exceptionThresholdHours = SystemDataRegistry.Instance.HighWaterMarkExceptionThresholdInHours.Value;
				if (exceptionThresholdHours <= 0)
				{
					exceptionThresholdHours = SystemDataRegistry.Instance.HighWaterMarkExceptionThresholdInHours.DefaultValue;
				}

				return TimeSpan.FromHours(exceptionThresholdHours);
			}
			protected internal set
			{
			}
		}

		readonly Dictionary<string, string[]> KnownExceptionMessages = new(StringComparer.Ordinal)
		{
			{ nameof(SqlException), new[] { "596" } }
		};
		bool IsKnownException(Exception ex)
		{
			if (ex?.InnerException is SqlException sqlEx)
			{
				var errorNumber = sqlEx.Number.ToString();
				var typeName = sqlEx.GetType().Name;
				if (KnownExceptionMessages.TryGetValue(typeName, out var knownErrorCodes))
				{
					return knownErrorCodes.Contains(errorNumber);
				}
			}
			return false;
		}
	}
}

#endregion
