using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Billing.Business;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Accounting.Business
{
	public class AutoRatingStarter
	{
		public AutoRatingStarter(IBusiness objectToAutoRate, ILogger logger, AdditionalJobsAction additionalJobsAction = AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, IAutoRatingStrategy autoratingStrategy = null)
			: this(new[] { objectToAutoRate }, logger, additionalJobsAction, autoratingStrategy)
		{
		}

		public AutoRatingStarter(IBusiness[] objectsToAutorate, ILogger logger, AdditionalJobsAction additionalJobsAction = AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, IAutoRatingStrategy autoratingStrategy = null)
			: this(objectsToAutorate, new RatingContext(logger), additionalJobsAction, autoratingStrategy)
		{
		}

		public AutoRatingStarter(IBusiness[] objectsToAutorate, IRatingContext ratingContext, AdditionalJobsAction additionalJobsAction = AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, IAutoRatingStrategy autoratingStrategy = null)
		{
			this.interactor = ratingContext.Logger as LoggerDecorator;
			this.objectsToAutorate = objectsToAutorate;
			this.additionalJobsAction = additionalJobsAction;
			this.ratingContext = ratingContext;
			this.strategy = autoratingStrategy;
		}

		readonly LoggerDecorator interactor;
		readonly IBusiness[] objectsToAutorate;
		readonly AdditionalJobsAction additionalJobsAction;
		readonly IRatingContext ratingContext;
		readonly IAutoRatingStrategy strategy;

		public static ZString CostResult
		{
			get { return Res.GetString("3A0E3A3C-C661-4513-823E-4E6AB81A9FEF", "cost"); }
		}

		public static ZString RevenueResult
		{
			get { return Res.GetString("03C19CFD-6AB4-435A-A65E-F5E0AF7D2589", "revenue"); }
		}

		#region Execution

		public RatingResults ExecuteAutorating(AutoRateOptions options, bool isSaveCalledManuallyAfterSession = false, HashSet<Job> additionalJobsToDispose = default)
		{
			var factories = objectsToAutorate.Select(x => x.Factory).WhereNotNull().Distinct().ToList();
			foreach (var factory in factories)
			{
				factory.SetContext(BusinessContext.AutoRating);
			}

			try
			{
				var isEqualization = options.BillingType == BillingType.EqualizeAndRate;
				using (_Rating.Start(interactor, options, isEqualization, isSaveCalledManuallyAfterSession, options.BillingType))
				{
					if (isEqualization)
					{
						ExecuteTargets(new LoggerDecorator(), objectsToAutorate, options, additionalJobsToDispose);

						var equalizationResults = _Rating.EqualizationInfo.GetEqualizationSummary();
						if (equalizationResults.IsEmpty)
						{
							equalizationResults = Res.GetString("c5191c08-39f3-45bc-83e1-9b5050a32043", "No volume equalization discount information was found");
						}

						interactor.Information(AccountingConstants.VolumeEqualizationMessages.DiscountResults + System.Environment.NewLine + equalizationResults);
						return ExecuteTargets(interactor, objectsToAutorate, options.With(autoRateCost: true, autoRateRevenue: false), additionalJobsToDispose);
					}

					return ExecuteTargets(interactor, objectsToAutorate, options, additionalJobsToDispose);
				}
			}
			finally
			{
				foreach (var factory in factories)
				{
					factory.RemoveContext(BusinessContext.AutoRating);
				}
			}
		}

		public RatingResults SearchForRates(AutoRateOptions options)
		{
			ratingContext.SearchForRatesMode = true;
			var result = ExecuteAutorating(options);
			ratingContext.SearchForRatesMode = false;

			return result;
		}

		RatingResults ExecuteTargets(
			IAutoRatingGUIInteractor interactorToUse,
			IEnumerable<IBusiness> targets,
			AutoRateOptions options,
			HashSet<Job> additionalJobsToDispose)
		{
			var result = new RatingResults();
			var serializer = new RatingObjectSerializer();

			var results = new List<RatingResult>();

			foreach (var target in targets)
			{
				var targetRatingResult = new RatingResult
				{
					Target = target.HumanReadableName
				};

				if (options.EnableAutoRateExplorer)
				{
					targetRatingResult.AutoRatingExplorer = serializer.GetJSON(target);
				}

				using (interactor.StartRatingSession())
				using (_Rating.StartSubSession(target))
				{
					targetRatingResult.Results =
						new AutoRatingStarterCore(interactorToUse, target, ratingContext, options.BillingType, additionalJobsAction, strategy, additionalJobsToDispose)
							.ExecuteAutorating(options)
							.ToArray();
				}

				results.Add(targetRatingResult);
			}

			result.Results = results.ToArray();
			result.Logs = Array.Empty<string>();

			return result;
		}

		#endregion
	}

	internal class AutoRatingStarterCore
	{
		public AutoRatingStarterCore(IAutoRatingGUIInteractor interactor, IBusiness targetRatingSupporter, IRatingContext ratingContext, BillingType billingType, AdditionalJobsAction additionalJobsAction, IAutoRatingStrategy strategyOverride, HashSet<Job> additionalJobsToDispose)
		{
			Argument.NotNull(targetRatingSupporter, nameof(targetRatingSupporter));
			Argument.NotNull(interactor, nameof(interactor));
			Argument.NotNull(ratingContext, nameof(ratingContext));

			this.targetRatingSupporter = targetRatingSupporter;
			this.interactor = interactor;

			var ratingSupporter = targetRatingSupporter as IRatingSupporter;
			var jobInvoicingPlugin = targetRatingSupporter as IJobInvoicingPlugIn;

			if (jobInvoicingPlugin == null)
			{
				pluginSecurity = Env.Security.None;
			}
			else
			{
				pluginSecurity = billingType == BillingType.Apportionment
					? Env.Security.MaintainConsolJobInvoicing
					: jobInvoicingPlugin.InvoicingSupporter.JobInvoicingSecurity;
			}

			strategiesRepository = new RatingStrategiesRepository(interactor, strategyOverride, ratingSupporter, billingType, additionalJobsAction, additionalJobsToDispose);
			this.ratingContext = ratingContext;
		}

		readonly IAutoRatingGUIInteractor interactor;
		readonly SecurityCheckpoint pluginSecurity;
		readonly IBusiness targetRatingSupporter;
		readonly RatingStrategiesRepository strategiesRepository;
		readonly IRatingContext ratingContext;
		bool isLogCleared;

		#region Execution

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "log message, no need translation")]
		public List<RatesAdditionInfo> ExecuteAutorating(AutoRateOptions options)
		{
			var ratesAdditionInfos = new List<RatesAdditionInfo>();

			if (!IsAutoRatingAllowedBySecurity(options))
			{
				return ratesAdditionInfos;
			}
			var validationSuccess = false;
			using (interactor.DeferErrorPopup())
			{
				validationSuccess = ValidatePreAutorating(options);
				if (validationSuccess)
				{
					try
					{
						interactor.SuspendLayout();

						var chargesDeletionInfos = new List<RatesAdditionInfo>();

						using (GetValidationSuspender())
						{
							if (!ratingContext.SearchForRatesMode)
							{
								chargesDeletionInfos = ChargeCleaner.CleanupExistingCharges(strategiesRepository, options);
								// In manual mode, log is cleared only after user applies their selection
								// So if they cancel, the log isn't altered and the job isn't modified.
								if (!ratingContext.IsManualCostSelectMode)
								{
									ClearLogNotes();
								}
							}

							string currentProcessName;

							if (options.AutoRateCost)
							{
								using (_Rating.StartCost())
								{
									currentProcessName = (NoResString)"Rating costs";
									ratesAdditionInfos.AddRange(ExecuteAutorating(CostSell.Cost, currentProcessName, options.ToExecuteAutoratingCosts()));
								}
							}

							if (options.AutoRateRevenue)
							{
								using (_Rating.StartSell())
								{
									currentProcessName = (NoResString)"Rating revenue";
									ratesAdditionInfos.AddRange(ExecuteAutorating(CostSell.Revenue, currentProcessName, options.ToExecuteAutoratingRevenue()));
								}
							}

							if (options.AutoRateCost && options.AutoRateRevenue)
							{
								try
								{
									ratingContext.IsInRebateCalculationMode = true;
									using (_Rating.StartSell())
									{
										currentProcessName = "Rating rebate";
										ratesAdditionInfos.AddRange(ExecuteAutorating(CostSell.Revenue, currentProcessName, options.ToExecuteAutoratingRevenue()));
									}
								}
								finally
								{
									ratingContext.IsInRebateCalculationMode = false;
								}
							}
						}

						if (chargesDeletionInfos.Any())
						{
							AddDeletedChargesToAutoratingResults(ratesAdditionInfos, chargesDeletionInfos);
						}

						if (possibleMatchesDialogData != null && !ratesAdditionInfos.Any(x => x.RatesFound != null && x.RatesFound.Any()))
						{
							interactor.ShowPossibleMatchesDialog(possibleMatchesDialogData);
						}
					}
					finally
					{
						interactor.ResumeLayout();
					}
				}
			}

			CleanupUnusedJobs(validationSuccess);

			return ratesAdditionInfos;

			IDisposable GetValidationSuspender()
			{
				IDisposable suspender = null;

				if (targetRatingSupporter is IValidationSuspenderForAutoRating validationSuspender)
				{
					suspender = new DisposableAction(() => targetRatingSupporter.Factory.SuspendValidation(), () =>
					{
						targetRatingSupporter.Factory.ResumeValidation();
						validationSuspender.OnValidationResumed();
					});
				}

				return suspender;
			}
		}

		void CleanupUnusedJobs(bool validationSuccess)
		{
			var apportionmentStrategy = strategiesRepository.Strategies.OfType<AutoRateApportionmentStrategy>().FirstOrDefault();
			var jobsWithoutCharges = strategiesRepository.NewlyCreatedJobHeadersDuringAutoRating.Where(job => job != null && !job.IsInDatabase && job.Charges.Count == 0);
			AddFetchHintsForCleanup(jobsWithoutCharges);

			if (!validationSuccess || apportionmentStrategy == null)
			{
				jobsWithoutCharges.ForEach(x => x.Delete());
			}
			else
			{
				apportionmentStrategy.DeletedUnusedJobs(jobsWithoutCharges);
			}

			void AddFetchHintsForCleanup(IEnumerable<Job> jobList)
			{
				var jobPKs = new List<ZGuid>();
				foreach (var job in jobList)
				{
					jobPKs.Add(job.PK);
				}

				if (jobPKs.Any())
				{
					var factory = jobList.First().Factory;
					foreach (var jobPK in jobPKs)
					{
						factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, jobPK);
					}
				}
			}
		}

		internal void ClearLogNotes()
		{
			isLogCleared = true;
			if (targetRatingSupporter is IStmNoteParent ratingSupporterAsBizO)
			{
				var logNotes = ratingSupporterAsBizO.Notes
					.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description)
					.Where(x => !x.IsNull && x.IsBelongingToCurrentLoginCompany);

				logNotes.ForEach(x => x.ST_NoteText = string.Empty);
			}
		}

		void AddDeletedChargesToAutoratingResults(List<RatesAdditionInfo> ratesAdditionInfos, List<RatesAdditionInfo> chargesDeletionInfos)
		{
			foreach (var chargesDeletionInfo in chargesDeletionInfos)
			{
				var ratesAdditionInfo = ratesAdditionInfos.FirstOrDefault(x => x.Target == chargesDeletionInfo.Target && x.CostSell == chargesDeletionInfo.CostSell);
				if (ratesAdditionInfo != null)
				{
					ratesAdditionInfo.DeletedChargesCount += chargesDeletionInfo.DeletedChargesCount;
				}
				else
				{
					ratesAdditionInfos.Add(chargesDeletionInfo);
				}
			}
		}

		bool IsAutoRatingAllowedBySecurity(AutoRateOptions options)
		{
			if (options.AutoRateRevenue && !SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AutoRateRevenue))
			{
				interactor.Warning(SecurityHelper.GetErrorTextForSecurityCheckPoint(SecurityCore.AutoRateRevenue));
				return false;
			}

			if (options.AutoRateCost && !SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.AutoRateCost))
			{
				interactor.Warning(SecurityHelper.GetErrorTextForSecurityCheckPoint(SecurityCore.AutoRateCost));
				return false;
			}

			return true;
		}

		JobInvoicingSecurityHelper SecurityHelper
		{
			get { return securityHelper ?? (securityHelper = new JobInvoicingSecurityHelper(() => pluginSecurity)); }
		}

		JobInvoicingSecurityHelper securityHelper;

		ZString masterHumanReadableName;

		IEnumerable<RatesAdditionInfo> ExecuteAutorating(CostSell costOrSell, string currentProcessName, AutoRateOptions options)
		{
			var autoRatingLogHandler = new AutoRatingLogHandler();
			var ratesAdditionInfos = new List<RatesAdditionInfo>();
			try
			{
				var currentRatingStrategies = strategiesRepository.Strategies.Where(x => x.Supports(costOrSell)).ToArray();
				var progressReporter = new RatingProgressReporter(currentRatingStrategies, interactor, currentProcessName, options);

				foreach (var strategy in currentRatingStrategies)
				{
					masterHumanReadableName = strategy.HostBusinessEntity.HumanReadableName + " : ";

					try
					{
						ratesAdditionInfos.Add(RateCharges(costOrSell, strategy, progressReporter, autoRatingLogHandler, options));
					}
					catch (JobCreationException ex)
					{
						interactor.Error(ex.Message);
					}
					finally
					{
						if (strategy.Job != null)
						{
							interactor.SetJobInvoicingSecurityOverrideProvider(strategy.Job);
						}
					}
				}
			}
			catch (AutoRater.CallForPriceException ex)
			{
				interactor.Error(ex.Message);
			}
			finally
			{
				autoRatingLogHandler.CreateLogs();
			}

			return ratesAdditionInfos;
		}

		#region Actual Rating

		internal virtual AutoRatingRunner GetRunner(IAutoRatingStrategy strategy)
		{
			var runner = new AutoRatingRunner(strategy.HostBusinessEntity, ratingContext, _Rating.IsSaveCalledManuallyAfterSession);
			runner.AutoRater.OneOffQuoteSelected += (_, args) => OnOneOffQuoteSelected(args.Quote, strategy);
			runner.AutoRater.AdditionalRatesNotification += AutoRater_AdditionalRatesNotification;
			runner.OnAfterRatingNotification += fRunner_OnAfterRatingNotification;
			return runner;
		}

		RatesAdditionInfo RateCharges(
			CostSell costOrSell,
			IAutoRatingStrategy strategy,
			RatingProgressReporter progressReporter,
			IAutoRatingLogHandler autoRatingLogHandler,
			AutoRateOptions options)
		{
			var adapters = strategiesRepository.GetRatingAdapters(strategy, options);
			var adapter = GetRatingAdapter(strategy, adapters);
			var autoRateScope = RatingUsageCollector.GetAutoRateScope(ratingContext, adapter, options, costOrSell);

			using (UsageCollector.Scope(autoRateScope))
			{
				var watch = Stopwatch.StartNew();
				var result = DoRateCharges(costOrSell, strategy, progressReporter, autoRatingLogHandler, options);
				watch.Stop();

				if (result.RatesFound != null)
				{
					RatingUsageCollector.ReportAutoRate(strategy.HostBusinessEntity.Factory, result, watch.Elapsed);
				}

				return result;
			}
		}

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule")]
		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity", Justification = "Complicated process would only get more complicated by splitting it up.")]
		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmantainableCode")]

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No need to translate, log string")]
		RatesAdditionInfo DoRateCharges(CostSell costOrSell, IAutoRatingStrategy strategy, RatingProgressReporter progressReporter, IAutoRatingLogHandler autoRatingLogHandler, AutoRateOptions options)
		{
			var factory = strategy.HostBusinessEntity.Factory;
			var ratesAdditionInfo = new RatesAdditionInfo { CostSell = costOrSell == CostSell.Cost ? AutoRatingStarter.CostResult : AutoRatingStarter.RevenueResult };

			IDisposable jobSequenceSuspender = null;
			if (strategy.Job != null)
			{
				strategy.Job.IsAutoratingInProcess = true;
				jobSequenceSuspender = strategy.Job.CheckForChargesDisplaySequenceDuplicatesSuspender.GetSuspender();
			}

			try
			{
				if (!HasBeenAutoRatedByAnotherUserAfterBeingLoaded(strategy.Job, factory))
				{
					var provider = strategiesRepository.GetRatingAdaptersProvider(strategy);
					var runner = GetRunner(strategy);

					using (provider.NewRatingSession())
					{
						ratingContext.InterimAutoRatingResults.Clear();

						AutoRateInfoCollection results;
						IAutoRating[] nonCustomsJobsToRate;
						var customsJobToRate = Array.Empty<ICustomsCharges>();
						var customsChargesManager = new CustomsChargesManager(strategy.HostBusinessEntity, interactor);

						try
						{
							nonCustomsJobsToRate = strategiesRepository.GetRatingAdapters(strategy, options);

							if (!ratingContext.IsInRebateCalculationMode)
							{
								customsJobToRate = provider.CanExecuteAutoRating(null, options) ? GetCustomsJobsToRate(strategy) : Array.Empty<ICustomsCharges>();
							}

							var customsChargesResults = customsChargesManager.RateCustomsCharges(customsJobToRate);

							// We add customs charges to results as there may be freight charges which depend on customs charges
							// like percentage calculator or cost based calculator.
							ratingContext.InterimAutoRatingResults.AddRange(customsChargesResults);

							results = runner.RetrieveAllCharges(nonCustomsJobsToRate, provider, costOrSell, progressReporter);

							if (!ratingContext.IsInRebateCalculationMode)
							{
								results.CheckAndAddRange(customsChargesResults);
							}
						}
						catch (AutoRater.NoExchangeRateException e)
						{
							interactor.ShowException(e);
							if (strategy.Job != null)
							{
								var currencies = factory.Load<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, e.Currencies.Select(x => x.Code)));
								var ledgerType = costOrSell == CostSell.Cost ? ExchangeRateValidLedgerEnum.AP : ExchangeRateValidLedgerEnum.AR;
								foreach (var currency in currencies)
								{
									var invoiceCurrencyType = AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(strategy.Job.Company, ledgerType, currency.RX_Code);
									strategy.Job.AddCurrency(currency, ledgerType, invoiceCurrencyType);
								}
							}

							return ratesAdditionInfo;
						}
						catch (AutoRater.RatingCancelledException)
						{
							if (isLogCleared)
							{
								ratingContext.Logger.Information(Res.GetString("21C572FD-EE56-4DCC-9D64-4F281D1EF1E4", "Canceled"));
								runner.WriteAutoratingLogIntoNote(targetRatingSupporter as IStmNoteParent);
							}
							throw;
						}
						catch (AutoRater.CallForPriceException)
						{
							// To be handled by the caller, not the catch all below.
							throw;
						}
						catch (Exception e) when (!e.IsCriticalException())
						{
							while (e != null)
							{
								if (e is AutoRaterException)
								{
									interactor.ShowException(e);
									return ratesAdditionInfo;
								}

								e = e.InnerException;
							}

							throw;
						}

						bool hasAutoRatingResults = results.Count > 0;

						if (!results.UserCancelledAutoRating && strategy.ShouldAddAutoRates)
						{
							var entityName = ((BusinessObject)strategy.HostBusinessEntity).HumanReadableName;
							ratesAdditionInfo.Target = entityName;
							ratesAdditionInfo.CreatedCharges = Array.Empty<ChargeInfo>();
							ratesAdditionInfo.ModifiedCharges = Array.Empty<ChargeInfo>();
							ratesAdditionInfo.DeletedChargesCount = 0;

							if (!ratingContext.SearchForRatesMode)
							{
								var adapters = strategiesRepository.GetRatingAdapters(strategy, options);
								var adapterIDs = adapters.Select(a => a.OperationalJobCode).ToArray();
								var additionResult = strategy.AddAutoRates(interactor, results, costOrSell, adapterIDs, adapters);

								foreach (var chargeWrapper in additionResult.CreatedCharges.Concat(additionResult.ModifiedCharges))
								{
									chargeWrapper.RecordPercentageApplied(ratingContext);
								}

								if (!ratingContext.IsInRebateCalculationMode)
								{
									additionResult.DeletedChargesCount += customsChargesManager.DeleteCustomsChargesIfNecessary(strategy.Job, results).Count;

									LogCAREvent(autoRatingLogHandler, strategy, costOrSell, additionResult, adapters, options);
									LogQAWEvent(strategy, additionResult);
									LogBAWEvent(strategy, additionResult);
								}

								var skipWarningUsersNoRebateFound = ratingContext.IsInRebateCalculationMode && !ratesAdditionInfo.CreatedCharges.Any();
								if (!skipWarningUsersNoRebateFound)
								{
									interactor.ShowRatesAdditionResult(additionResult, entityName, costOrSell);
								}

								ratesAdditionInfo.CreatedCharges = additionResult.CreatedCharges.Select(x => new ChargeInfo(x.ChargeCode, x.Info)).OrderBy(x => x.ChargeCode).ToArray();
								ratesAdditionInfo.ModifiedCharges = additionResult.ModifiedCharges.Select(x => new ChargeInfo(x.ChargeCode, x.Info)).OrderBy(x => x.ChargeCode).ToArray();
								ratesAdditionInfo.DeletedChargesCount = additionResult.DeletedChargesCount;
							}

							ratesAdditionInfo.RatesFound = results.Select(x => new ChargeInfo(x.ChargeCode?.AC_Code, x)).OrderBy(x => x.ChargeCode).ToArray();
						}
						else if (!strategy.ShouldAddAutoRates && hasAutoRatingResults)
						{
							interactor.Information("Rates were found but not added to results due to missing or closed jobs");
						}

						if (!hasAutoRatingResults && results.UserCancelledAutoRating && provider.CanExecuteAutoRating(null, options) && (nonCustomsJobsToRate.Any() || customsJobToRate.Any()))
						{
							bool showMessage = true;
							string caption = string.Empty;
							string message = string.Empty;

							caption = Res.GetString("a5778d43-7116-49d3-947e-d9121009a786", "Autorating Canceled");

							switch (results.Cancellation.Reason)
							{
								case AutoRatingCancellation.Reasons.AdditionalRatesPrompt:
									showMessage = false;
									break;

								case AutoRatingCancellation.Reasons.RatesSecurity:
									message = results.Cancellation.Message;
									break;
							}

							if (showMessage)
							{
								var jobDescription = strategy.HostBusinessEntity.HumanReadableName == ZString.Empty
												? provider.ConsumerTypeDescription
												: strategy.HostBusinessEntity.HumanReadableName;

								if (string.IsNullOrWhiteSpace(message))
								{
									message = jobDescription.Trim();
								}

								interactor.Warning(caption + ": " + message.Trim());
							}
						}

						if (ratingContext.IsManualCostSelectMode && !isLogCleared)
						{
							ClearLogNotes();
						}

						runner.WriteAutoratingLogIntoNote(targetRatingSupporter as IStmNoteParent);

						if (ratingContext != null && DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.Value)
						{
							var cache = factory.GetCachedValue("AutoRatingStarerCore.RateCharges.RawResponse", () => new Dictionary<ZGuid, string>());
							cache.GetOrAdd(strategy.HostBusinessEntity.Identifier, () => ratingContext.RawResponse);
						}
					}
				}
			}
			finally
			{
				if (jobSequenceSuspender != null)
				{
					jobSequenceSuspender.Dispose();
				}

				if (strategy.Job != null)
				{
					strategy.Job.IsAutoratingInProcess = false;
					strategy.Job.UpdateTotals();
				}

				if (strategy.Job != null)
				{
					strategy.Job.Validation.ValidateAll();
				}
			}

			return ratesAdditionInfo;
		}

		#region SuppressResourceStringsCheckRegion

		#region CAR Event

		void LogCAREvent(IAutoRatingLogHandler autoRatingLogHandler, IAutoRatingStrategy strategy, CostSell costOrSell, AutoRatesAdditionResult result, IEnumerable<IAutoRating> adapters, AutoRateOptions options)
		{
			var logsParent = strategy?.HostBusinessEntity as IStmALogParent;
			if (logsParent == null)
			{
				return;
			}

			var modified = result.ModifiedCharges.Count();
			var created = result.CreatedCharges.Count();
			var deleted = result.DeletedChargesCount;

			var ratesFound = created > 0 || modified > 0 && deleted > 0 || result.AutoRates.Any(x => x.Amount != 0);
			if (!ratesFound)
			{
				return;
			}

			var jobNumberProvider = strategy?.HostBusinessEntity as IJobNumber;
			var company = strategy.Job?.Company ?? GlbCompany.CurrentCompany;
			var shortLog = AutoRatesAdditionResult.GetReferenceForCAREvent(company);
			var triggerSource = GetTriggerParameterValue(options.TriggerSource);
			var mode = costOrSell == CostSell.Cost ? "Cost" : "Revenue";
			var type = GetJobType(strategy, adapters);
			var jobNumber = jobNumberProvider?.JobNumber ?? ZString.Empty;

			var reference = StmALog.GenerateEventReference(
				FormattableString.Invariant($"{shortLog}, found: {result.AutoRates.Count}, created: {created}, modified: {modified}, deleted: {deleted}"),
				new Dictionary<string, string>
				{
					{ EventReferenceParameters.Mode, mode },
					{ EventReferenceParameters.Type, type },
					{ EventReferenceParameters.JobNumber, jobNumber },
					{ EventReferenceParameters.TriggerCode, triggerSource },
				});

			autoRatingLogHandler.QueueCARLogToCreateOnceAutoRatingFinished(logsParent, ZDateTime.Now, reference);
		}

		static IAutoRating GetRatingAdapter(IAutoRatingStrategy strategy, IEnumerable<IAutoRating> adapters)
		{
			var adapter = adapters.FirstOrDefault(a => a.AutoRatedFor.Any(bo => bo == strategy.HostBusinessEntity));
			if (adapter != null)
			{
				return adapter;
			}

			return adapters.FirstOrDefault();
		}

		static string GetJobType(IAutoRatingStrategy strategy, IEnumerable<IAutoRating> adapters)
		{
			// The idea is to return the type of job being autorated (i.e. Consol, Shipment, CFS Shipment, Customs Declaration, etc.).
			// There is several ways of getting it. Seems like we have several collections for different purposes.

			// First we try to get type from rating adapter (since AdapterType from rating adapter is closer to what we want).
			var adapter = GetRatingAdapter(strategy, adapters);
			if (adapter != null)
			{
				return adapter.AdapterType.ToString();
			}

			// Just in case if we can't find proper adapter (since one job may have multiple adapters) we fallback to
			// Consumer Type created for invoicing. Basically, it is similar to AdapterType from Rating Adapter but has
			// slightly different naming.
			var ratingSupporter = strategy?.HostBusinessEntity as IRatingSupporter;
			return ratingSupporter?.AdaptersProvider.ConsumerTypeDescription;
		}

		static string GetTriggerParameterValue(AutoRateTriggerSource source)
		{
			switch (source)
			{
				case AutoRateTriggerSource.Unspecified:
					return "Unspecified";

				case AutoRateTriggerSource.Api:
					return "API";

				case AutoRateTriggerSource.Menu:
					return "Menu";

				case AutoRateTriggerSource.Workflow:
					return "Workflow";

				case AutoRateTriggerSource.OperationalActions:
					return "Operational Actions";

				case AutoRateTriggerSource.PrintInvoicing:
					return "Print Invoicing";

				case AutoRateTriggerSource.GatewayBilling:
					return "Gateway Billing";

				default:
					return null;
			}
		}

		#endregion

		#region QAW Event

		public void LogQAWEvent(IAutoRatingStrategy strategy, AutoRatesAdditionResult result)
		{
			var quote = (strategy?.HostBusinessEntity as QuotedBooking)?.Quote;
			if (quote != null && quote.TH_OneTimeQuote && result.CreatedCharges.Any(x => x.IsCreatedFromWiseCost) || result.ModifiedCharges.Any(x => x.IsCreatedFromWiseCost))
			{
				var logsParent = (IStmALogParent)quote;
				if (logsParent != null && !logsParent.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.QuoteAutoratedWithWiseRatesCode))
				{
					logsParent.Logs.AddNew(Events.QuoteAutoratedWithWiseRates, "Rates Service Usage", ZDateTimeOffset.Now, true);
				}
			}
		}

		#endregion

		#region BAW Event

		public void LogBAWEvent(IAutoRatingStrategy strategy, AutoRatesAdditionResult result)
		{
			var booking = (strategy?.HostBusinessEntity as QuotedBooking)?.Booking;
			var quote = (strategy?.HostBusinessEntity as QuotedBooking)?.Quote;
			if (booking != null && !booking.JS_IsForwardRegistered && result.CreatedCharges.Any(x => x.IsCreatedFromWiseCost) || result.ModifiedCharges.Any(x => x.IsCreatedFromWiseCost))
			{
				var quoteHasQAWEvent = quote != null && ((IStmALogParent)quote).Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.QuoteAutoratedWithWiseRatesCode)).Any();

				if (!quoteHasQAWEvent)
				{
					var bookingLogsParent = (IStmALogParent)booking;
					if (bookingLogsParent != null && !bookingLogsParent.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.BookingAutoratedWithRatesServiceCode))
					{
						bookingLogsParent.Logs.AddNew(Events.BookingAutoratedWithRatesService, FormattableString.Invariant($"Rates Service Usage Booking {booking.JobNumber}"), ZDateTimeOffset.Now, true);
					}
				}
			}
		}

		#endregion

		#endregion

		#region Jobs To Rate

		internal ICustomsCharges[] GetCustomsJobsToRate(IAutoRatingStrategy strategy)
		{
			List<ICustomsCharges> result = new List<ICustomsCharges>();
			ICustomsChargesAdditionalCharges additionalCharges = ServiceLocator.GetService<ICustomsChargesAdditionalCharges>(strategy.HostBusinessEntity);
			if (additionalCharges != null)
			{
				result.AddRange(additionalCharges.AdditionalCharges);
			}

			ICustomsCharges customsCharges = ServiceLocator.GetService<ICustomsCharges>(strategy.HostBusinessEntity);
			if (customsCharges != null && customsCharges.IsActive)
			{
				result.Add(customsCharges);
			}
			return result.ToArray();
		}

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule")]
		bool HasBeenAutoRatedByAnotherUserAfterBeingLoaded(Job job, BusinessObjectFactory factory)
		{
			var log = GetRatingLogForOtherUsersAfterThisJobWasLoaded(job);
			if (log != null)
			{
				var message = Res.GetString("fc7b769e-1a71-415f-a229-8c5c2f7b2f68", @"While you have been working with this form, another user has made changes.

User: {0} ({1}) has run autorating on this job.
	
Do you want to load the changes they have made to the charges?", log.User?.GS_FullName ?? ZString.Empty, log.SL_GS_NKUser);

				if (interactor.YesNoWarning(message))
				{
					factory.ClearQueryCache(JobChargeSchema.Constants.TableName);
					job.Charges.Load();
					job.UtcTimeJobWasLoaded = ZDateTime.UtcNow;
				}

				return true;
			}

			return false;
		}

		StmALog GetRatingLogForOtherUsersAfterThisJobWasLoaded(Job job)
		{
			if (job != null && job.IsInDatabase && job.UtcTimeJobWasLoaded.IsValid && !job.UtcTimeJobWasLoaded.IsEmpty)
			{
				var query = new ZQuery();
				query.AddToFilter(StmALogSchema.SL_Parent, new[] { job.PK, job.JH_ParentID });
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ChargesHaveBeenAutoRated.Code);
				query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
				query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, AutoRatesAdditionResult.GetReferenceForCAREvent(job.Company));
				query.AddToFilter(StmALogSchema.SL_GS_NKUser, SQLComparisonOperator.NotEqual, GlbStaff.CurrentUser.GS_Code);
				query.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, job.UtcTimeJobWasLoaded);

				return job.Factory.LoadTop1<StmALog>(query);
			}

			return null;
		}

		#endregion

		#endregion

		#endregion

		#region Validation

		internal bool ValidatePreAutorating(AutoRateOptions options = default)
		{
			var result = true;

			if (!strategiesRepository.Strategies.Any())
			{
				interactor.Error(GetImpossibleToAutorateMessage());
				return false;
			}

			foreach (var strategy in strategiesRepository.Strategies)
			{
				masterHumanReadableName = strategy.HostBusinessEntity.HumanReadableName + " : ";
				var jobAutoRatingValidationErrorMessage = GetPreAutoratingErrors(strategy, options);
				if (!jobAutoRatingValidationErrorMessage.IsEmpty)
				{
					interactor.Error(jobAutoRatingValidationErrorMessage);
					result = false;
				}
				else
				{
					var warning = GetPreAutoratingWarnings(strategy);
					if (!warning.IsEmpty)
					{
						result = interactor.YesNoWarning(warning);
					}
				}
			}

			return result;
		}

		internal ZString GetPreAutoratingErrors(IAutoRatingStrategy strategy, AutoRateOptions options = default)
		{
			ZString result = "";
			var hostAsInvoicingPlugin = strategy.HostBusinessEntity as IJobInvoicingPlugIn;

			if (!JobInstantiated(hostAsInvoicingPlugin, strategy))
			{
				result = masterHumanReadableName + Res.GetString("038D286C-C141-4998-9B70-D710C0481A4A", "Job has not been created or reactivated yet.");
			}
			else if (strategy.Job != null && strategy.Job.IsDeleted)
			{
				result = masterHumanReadableName + Res.GetString("9177da6f-e7dd-493c-8257-1e871996095c", "Job is deleted.");
			}
			else if (hostAsInvoicingPlugin != null)
			{
				result = hostAsInvoicingPlugin.InvoicingSupporter.GetReasonNotToAllowAutoRate(options);
			}

			if (result.IsEmpty)
			{
				if (GetCustomsJobsToRate(strategy).Length > 0 && !GetCustomsChargeRelatedError(strategy).IsNullOrEmpty())
				{
					result = GetCustomsChargeRelatedError(strategy);
				}
				else if (strategy.Job != null && strategy.Job.ReadOnly)
				{
					result = masterHumanReadableName + Res.GetString("01ef6913-f63e-4306-9679-fa5e5caa8704", "Autorating cannot be run while the billing job is read only.");
				}
				else
				{
					result = ValidateJob(strategy);
				}
			}

			return result;
		}

		internal ZString GetPreAutoratingWarnings(IAutoRatingStrategy strategy)
		{
			if (strategy.HostBusinessEntity is IJobInvoicingPlugIn invoicingPlugIn)
			{
				return invoicingPlugIn.InvoicingSupporter.GetWarningForContinueAutoRate();
			}
			return ZString.Empty;
		}

		protected virtual bool JobInstantiated(IJobInvoicingPlugIn hostAsInvoicingPlugin, IAutoRatingStrategy strategy)
		{
			IJobCostingPlugIn consolApportionmentPlugIn = hostAsInvoicingPlugin as IJobCostingPlugIn;
			bool isInvoicingPluginAndNotGateWayBilling = hostAsInvoicingPlugin != null && consolApportionmentPlugIn == null;
			return !(isInvoicingPluginAndNotGateWayBilling &&
					hostAsInvoicingPlugin.InvoicingSupporter.ConsumerType != null &&
					hostAsInvoicingPlugin.InvoicingSupporter.ConsumerType.IsActive &&
					!(strategy.Job?.JH_IsActive ?? false));
		}

		string GetCustomsChargeRelatedError(IAutoRatingStrategy strategy)
		{
			var error = "";
			var disbursementChargeCode = strategy.HostBusinessEntity.Factory.Load<AccChargeCode>(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);
			var isDisbursementChargeNotSet = disbursementChargeCode == null;
			if (isDisbursementChargeNotSet)
			{
				error = Res.GetString("8ca4094a-3c07-42f9-8410-93c47b58dbe6", "You must set up the 'Customs Disbursement Charge Code' in the registry before Autorating can be run.");
			}

			var customsDeferredChargeCode = strategy.HostBusinessEntity.Factory.Load<AccChargeCode>(RatingDataRegistry.Instance.CustomDeferredChargeCode.Value);
			var isDeferredChargeNotSet = customsDeferredChargeCode == null && RatingDataRegistry.Instance.IncludeCustomDeferredChargeInInvoicing.Value;

			if (isDeferredChargeNotSet)
			{
				var deferredChargeError = Res.GetString("a9d4a659-229f-4db8-8a56-9212969f2226", "You have enabled 'Include Custom Deferred Charge in Invoicing' in the registry, but the corresponding 'Custom Deferred Charge' is not set up in the registry.\r\n\r\nPlease set up 'Custom Deferred Charge' or disable 'Include Custom Deferred Charge in Invoicing' in the registry.");

				error = !isDisbursementChargeNotSet
					? deferredChargeError
					: Res.GetString("bee2abb3-3a08-46f9-8e40-e24fe40cabb9", "Following registry items are not set up correctly. Please fix them before you run the auto rating.\r\n\r\n - {0}\r\n - {1}", error, deferredChargeError);
			}

			return error;
		}

		ZString ValidateJob(IAutoRatingStrategy strategy)
		{
			ZString result = "";

			if (strategy.Job != null)
			{
				if (strategy.Job.JH_Status == JobHeaderStatus.ScheduledForArchive.Code)
				{
					result = masterHumanReadableName + Res.GetString("210dc31b-35a6-4df4-8186-7fbe8f326182", "Autorating cannot be run as it is scheduled for archiving.");
				}
				else if (strategy.Job.IsClosed)
				{
					result = masterHumanReadableName + Res.GetString("83b28543-a366-43f9-bdb8-a9f814c0d2ea", "Autorating cannot be run as its Invoicing Job is closed.");
				}
				else if (strategy.Job.IsReadyForCostPosting || strategy.Job.IsReadyForRevenuePosting)
				{
					result = masterHumanReadableName + Res.GetString("1810d367-7fc3-45f5-9229-402184cc5b1a", "Autorating cannot be run while the Job has Ready for Revenue or Cost Posting status.");
				}
				else if (strategy.Job.IsReadyForFinancialClosureWithoutModifySecurity)
				{
					result = masterHumanReadableName + Res.GetString("C56C9048-DB57-488A-8BA8-3FAC0211F0ED", "Autorating cannot be run as its Invoicing Job has Ready For Financial Closure status.");
				}
				else
				{
					if (!(targetRatingSupporter is IValidationSuspenderForAutoRating validationSuspender) || validationSuspender.ShouldRunPreSaveValidationBeforeAutorating(strategy.HostBusinessEntity))
					{
						strategy.Job.RunPreSaveValidation();
					}

					if (strategy.Job.HasErrors)
					{
						result = masterHumanReadableName + Res.GetString("9dc55df4-51c3-4ba0-b52c-9c07c8f44eb5", "Autorating cannot be run because there are errors on this job. Please correct these errors before Autorating.") + "\r\n\r\n";
						foreach (string error in ((INotificationProvider)strategy.Job).Notifications.GetUniqueMessageList())
						{
							result += error + "\r\n";
						}
					}
					else
					{
						var debtorValidationResult = ((JobValidation)strategy.Job.Validation).ValidateOrganisation(false);
						if (!debtorValidationResult.IsEmpty)
						{
							result = masterHumanReadableName + Res.GetString("1fdd07e1-ad78-42fc-a47a-71c1305979d0", "Autorating cannot be run. ") + debtorValidationResult;
						}
					}
				}
			}
			else
			{
				var validator = strategy as IAutoRatingValidator;
				var autoratingNotPermitted = validator?.GetAutoratingNotPermittedReason();

				if (!string.IsNullOrEmpty(autoratingNotPermitted))
				{
					result = masterHumanReadableName + autoratingNotPermitted;
				}
			}

			return result;
		}

		ZString GetImpossibleToAutorateMessage()
		{
			return masterHumanReadableName + Res.GetString("ccb7c63e-a32d-4e2f-bb88-6001dc717b24", "You cannot perform Autorating. Please enter your invoice charges manually for this kind of job.");
		}

		#endregion

		#region Rating Notifications

		string RatingNotificationsCaption { get { return Res.GetString("38ad36ff-29b4-467e-8a95-e754f852f13f", "Autorating Notifications"); } }

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Notification string")]
		bool AutoRater_AdditionalRatesNotification(IAutoRating primaryJobToRate, AutoRater.AdditionalRatesNotifications notifications)
		{
			bool continueAutorating = true;

			if (notifications.HasRatesGoingToExpire || notifications.HasJustExpiredRates || notifications.HasUnacceptedQuotes)
			{
				string message = masterHumanReadableName + Res.GetString("e6b428f9-a01c-4d61-b665-af0988bcb696", "There are matching:") + "\r\n";
				if (notifications.HasJustExpiredRates)
				{
					message += "  " + "\u2022" + " ";
					if (notifications.HasGlobalJustExpiredRates)
					{
						message += Res.GetString("e4b702b2-08f4-4103-9d2a-9cbfe5d83354", "Company Tariff entries");
					}

					if (notifications.HasGlobalJustExpiredRates && notifications.HasClientJustExpiredRates)
					{
						message += " " + Res.GetString("7dacbe71-dc17-4b46-85a0-d767c0df6ed0", "and") + " ";
					}

					if (notifications.HasClientJustExpiredRates)
					{
						message += Res.GetString("8b856a15-c68a-48f1-b7de-e3fd18440368", "Client Rate entries");
					}

					message += " " + Res.GetString("4b4e75a6-391d-4b82-83e2-88ca7436925e", "that expired in the last {0} days", AutoRater.ExpiredRateNotificationDays) + "\r\n";
				}

				if (notifications.HasRatesGoingToExpire)
				{
					message += "  " + "\u2022" + " ";
					if (notifications.HasGlobalRatesGoingToExpire)
					{
						message += Res.GetString("e4b702b2-08f4-4103-9d2a-9cbfe5d83354", "Company Tariff entries");
					}

					if (notifications.HasGlobalRatesGoingToExpire && notifications.HasClientRatesGoingToExpire)
					{
						message += " " + Res.GetString("7dacbe71-dc17-4b46-85a0-d767c0df6ed0", "and") + " ";
					}

					if (notifications.HasClientRatesGoingToExpire)
					{
						message += Res.GetString("8b856a15-c68a-48f1-b7de-e3fd18440368", "Client Rate entries");
					}

					message += " " + Res.GetString("7277cca0-c0e8-4a71-aac1-240a0680728b", "that are going to expire within the next {0} days", AutoRater.ExpiringRateNotificationDays) + "\r\n";
				}

				if (notifications.HasUnacceptedQuotes)
				{
					message += "  " + "\u2022" + " ";
					message += Res.GetString("0ac73c52-6d1d-4d6c-b18f-3de566b6402c", "Unaccepted Quotes for this client") + "\r\n";
				}

				if (notifications.HasRatesGoingToExpire && !notifications.HasJustExpiredRates && !notifications.HasUnacceptedQuotes)
				{
					interactor.Warning(RatingNotificationsCaption + ": " + message);
				}
				else
				{
					message += "\r\n" + Res.GetString("b24936c1-9413-4daf-a33a-0d89b4bd5f09", "Do you want to continue with Autorating?");
					continueAutorating = interactor.YesNoWarning(message);
				}
			}

			if (continueAutorating && notifications.HasPossibleMatches)
			{
				possibleMatchesDialogData = notifications;
			}

			return continueAutorating;
		}

		AutoRater.AdditionalRatesNotifications possibleMatchesDialogData;

		void fRunner_OnAfterRatingNotification(string message)
		{
			if (!string.IsNullOrEmpty(message))
			{
				interactor.Warning(RatingNotificationsCaption + ": " + message);
			}
		}

		#endregion

		#region One Off Quote Matches

		void OnOneOffQuoteSelected(Quote quote, IAutoRatingStrategy strategy)
		{
			strategy.Job.JH_TH_NKQuoteNumber = quote.TH_QuoteNumber;
		}

		#endregion
	}
}
