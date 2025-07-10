using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Accounting.Business
{
	public sealed class RatingStrategiesRepository
	{
		public RatingStrategiesRepository(IAutoRatingGUIInteractor interactor, IAutoRatingStrategy strategyOverride, IRatingSupporter ratingSupporter, BillingType billingType, AdditionalJobsAction additionalJobsAction, HashSet<Job> additionalJobsToDispose)
		{
			Interactor = interactor;
			NewlyCreatedJobHeadersDuringAutoRating = additionalJobsToDispose ?? new HashSet<Job>();
			Strategies = GetRatingStrategies(strategyOverride, ratingSupporter, billingType, additionalJobsAction, NewlyCreatedJobHeadersDuringAutoRating);
		}

		internal IAutoRatingStrategy[] Strategies { get; }
		internal IAutoRatingGUIInteractor Interactor { get; }

		public HashSet<Job> NewlyCreatedJobHeadersDuringAutoRating { get; }

		#region Get Rating Strategies

		IAutoRatingStrategy[] GetRatingStrategies(IAutoRatingStrategy strategyOverride, IRatingSupporter ratingSupporter, BillingType billingType, AdditionalJobsAction additionalJobsAction, HashSet<Job> additionalJobsToDispose)
		{
			var strategy = strategyOverride ?? GetRatingStrategyFromRatingSupporter(ratingSupporter, billingType);
			if (strategy == null)
			{
				return Array.Empty<IAutoRatingStrategy>();
			}

			var strategies = new HashSet<IAutoRatingStrategy>();
			if (strategy is AutoRateInvoicingStrategy && ratingSupporter is BusinessObject bizo)
			{
				LoadOrCreateJob(bizo, ratingSupporter, additionalJobsToDispose);
			}

			strategies.Add(strategy);

			var hasAdditionalJobsAction = additionalJobsAction != AdditionalJobsAction.NoAction
				&& !(billingType == BillingType.Invoicing
					&& ratingSupporter is IJobCostingPlugIn
					&& ratingSupporter is IJobInvoicingPlugIn);

			if (hasAdditionalJobsAction)
			{
				var provider = ratingSupporter.AdaptersProvider;
				if (provider != null)
				{
					foreach (var jobInvoicingPlugIn in provider.GetAdditionalJobs())
					{
						var businessObject = jobInvoicingPlugIn as BusinessObject;
						if (businessObject != null)
						{
							var job = LoadOrCreateJob(businessObject, ratingSupporter, additionalJobsToDispose);
							if (ShouldJobBeAutoRated(ratingSupporter, job, additionalJobsAction))
							{
								strategies.Add(new AutoRateInvoicingStrategy(businessObject, job, provider));
							}

							if (job == null)
							{
								Interactor.Warning(Res.GetString("8A4FBB72-A472-41E4-8930-AEFF3562821F", "Job header with billing charges for {0} could NOT be created because it is locked by another process. Please try again later.", businessObject.HumanReadableName));
							}
						}
					}
				}
			}

			hasAlreadyPromptedUser = false;
			shouldJobBeCreated = false;

			return strategies.ToArray();
		}

		static bool ShouldJobBeAutoRated(IRatingSupporter supporter, Job job, AdditionalJobsAction additionalJobsAction) =>
			additionalJobsAction == AdditionalJobsAction.AutoRateAdditionalInvoicingJobs && job != null && !(supporter is IPeriodicRatingSupporter && job.JH_ExcludeFromPeriodicRating);

		IAutoRatingStrategy GetRatingStrategyFromRatingSupporter(IRatingSupporter ratingSupporter, BillingType billingType)
		{
			var businessObject = (IBusiness)ratingSupporter;
			var costing = ratingSupporter as IJobCostingPlugIn;
			if (!costing.HasCostSupporterPK())
			{
				costing = null;
			}
			var invoicing = ratingSupporter as IJobInvoicingPlugIn;

			if (invoicing != null && billingType == BillingType.PrintInvoicing)
			{
				if (IsHLSShipment(ratingSupporter))
				{
					return new AutoRateHLSPrintingStrategy(businessObject, new Job.Loader(invoicing.Factory, invoicing).Load(false));
				}

				return new AutoRatePrintingStrategy(businessObject, new Job.Loader(invoicing.Factory, invoicing).Load(false));
			}

			if (IsHLSShipment(ratingSupporter))
			{
				return new AutoRateHLSStrategy(businessObject, new Job.Loader(invoicing.Factory, invoicing).Load(false));
			}

			if (invoicing != null && (billingType == BillingType.Invoicing && costing != null || costing == null))
			{
				return new AutoRateInvoicingStrategy(businessObject, new Job.Loader(invoicing.Factory, invoicing).Load(false));
			}

			if (invoicing != null && billingType == BillingType.EqualizeAndRate && businessObject is ForwardingConsol consol && consol.FillGatewayBillingTabForAutorating())
			{
				return new AutoRateInvoicingStrategy(businessObject, new Job.Loader(invoicing.Factory, invoicing).Load(false));
			}

			if (costing != null)
			{
				return new AutoRateApportionmentStrategy(businessObject, costing.GetApportionments(), costingPlugIn: costing);
			}

			return null;
		}

		static bool IsHLSShipment(IRatingSupporter ratingSupporter) => ratingSupporter != null
			&& ratingSupporter.AdaptersProvider != null
			&& ratingSupporter.AdaptersProvider.AdaptersProviderOptions == AdaptersProviderOptions.HLSShipment;

		#region Jobs

		Job LoadOrCreateJob(BusinessObject businessObject, IRatingSupporter ratingSupporter, HashSet<Job> additionalJobsToDispose)
		{
			Job job = null;
			var jobInvoicingPlugIn = businessObject as IJobInvoicingPlugIn;

			if (jobInvoicingPlugIn != null)
			{
				job = new Job.Loader(jobInvoicingPlugIn).Load(false);

				if (job == null)
				{
					var shouldSuppress = ratingSupporter is IAutoRatingSuppressJobCreationPrompt suppressor && suppressor.SuppressJobCreationPrompt;
					if (!shouldSuppress)
					{
						PromptUserShouldJobsBeCreated(businessObject.HumanReadableName);
					}
					else
					{
						shouldJobBeCreated = true;
					}

					if (shouldJobBeCreated)
					{
						job = new Job.Loader(jobInvoicingPlugIn).TryCreateWithMutex();
						if (job != null)
						{
							additionalJobsToDispose?.Add(job);
						}
					}
				}

				if (job != null)
				{
					businessObject.RegisterEditableChildObject(job);
				}
			}

			return job;
		}

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule")]
		void PromptUserShouldJobsBeCreated(ZString jobName)
		{
			if (!hasAlreadyPromptedUser)
			{
				var message = Res.GetString("c054fe93-60d1-4368-8154-55cc393b0381", @"An Invoicing Record for {0} has not been created. 
Auto rating can only run on Jobs with Invoicing Records.
Do you want to create all the required Invoicing Records now?", jobName);

				hasAlreadyPromptedUser = true;
				shouldJobBeCreated = Interactor.YesNoWarning(message);
			}
		}

		bool hasAlreadyPromptedUser;
		bool shouldJobBeCreated;

		#endregion

		#endregion

		#region Get IAutoRating (i.e. Rating Adapters)

		public RatingAdaptersProvider GetRatingAdaptersProvider(IAutoRatingStrategy strategy) => providers.GetOrAdd(strategy, () => strategy.AdaptersProvider());
		readonly Dictionary<IAutoRatingStrategy, RatingAdaptersProvider> providers = new Dictionary<IAutoRatingStrategy, RatingAdaptersProvider>();

		public IAutoRating[] GetRatingAdapters(IAutoRatingStrategy strategy, AutoRateOptions options)
		{
			var costOrSell = options.AutoratingProcess;

			if (strategy.Supports(costOrSell))
			{
				if (!(strategy.ParentProvider?.IncludeChildrenProviders(costOrSell, options.BillingType) ?? true))
				{
					return Array.Empty<IAutoRating>();
				}

				var provider = strategy.AdaptersProvider();
				if (provider != null)
				{
					var key = new Key(strategy, costOrSell);

					return ratingAdapters.GetOrAdd(key, () => provider.GetAdapters(Interactor, options).ToArray());
				}
			}

			return Array.Empty<IAutoRating>();
		}

		readonly Dictionary<Key, IAutoRating[]> ratingAdapters = new Dictionary<Key, IAutoRating[]>();

		struct Key
		{
			internal Key(IAutoRatingStrategy strategy, CostSell costOrSell)
			{
				this.strategy = strategy;
				this.costOrSell = costOrSell;
			}

			readonly IAutoRatingStrategy strategy;
			readonly CostSell costOrSell;

			public override bool Equals(object obj)
			{
				var otherKey = (Key)obj;
				return otherKey.strategy == strategy && otherKey.costOrSell == costOrSell;
			}

			public override int GetHashCode()
			{
				return strategy.GetHashCode() ^ costOrSell.GetHashCode();
			}
		}

		#endregion
	}
}
