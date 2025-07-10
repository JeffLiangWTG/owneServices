using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public sealed class AutoRateApportionmentStrategy : IAutoRatingStrategy, IAutoRatingValidator
	{
		public AutoRateApportionmentStrategy(IBusiness hostBusinessEntity, ApportionmentListing apportionmentListing, RatingAdaptersProvider parentProvider = null, IJobCostingPlugIn costingPlugIn = null)
		{
			Argument.NotNull(hostBusinessEntity, "hostBusinessEntity");
			Argument.NotNull(apportionmentListing, "apportionmentListing");
			HostBusinessEntity = hostBusinessEntity;
			this.apportionmentListing = apportionmentListing;
			this.costingPlugIn = costingPlugIn;
			ParentProvider = parentProvider;
		}

		readonly ApportionmentListing apportionmentListing;
		readonly IJobCostingPlugIn costingPlugIn;

		public IBusiness HostBusinessEntity { get; }
		public Job Job => null;

		static Dictionary<MergeKey, List<JobConsolCost>> GetExistingCostsByKey(JobConsolCostCollection collection, string ratingBehaviour)
		{
			return collection
				.Cast<JobConsolCost>()
				.Where(x => x.ChargeCode != null &&
					   x.E6_RatingBehaviour == ratingBehaviour)
				.ToKeyListDictionary(x => new MergeKey(x));
		}

		/// <summary>
		/// Adds the result of autorating into the job according to the
		/// RatingBehaviour that is set for the existing job charges.
		/// </summary>
		public AutoRatesAdditionResult AddAutoRates(IAutoRatingGUIInteractor interactor, AutoRateInfoCollection autoRatingResults, CostSell costOrSell, ZString[] adapterIDs, IEnumerable<IAutoRating> adapters = null)
		{
			var createdCosts = new List<ChargeWrapper>();
			var errorMessages = new HashSet<string>();
			var warningMessages = new HashSet<string>();

			if (autoRatingResults.Any())
			{
				var costsByKeyShouldNotBeAutorated =
					GetExistingCostsByKey(
						apportionmentListing.CostsCollection,
						RatingBehaviours.StopFromAutorating);
				var costsByKeyShouldBeReautorated =
					GetExistingCostsByKey(
						apportionmentListing.CostsCollection,
						RatingBehaviours.ReAutorateCharge);

				// Although it may seem wasteful we need to find the final carrier of
				// each autorateinfo upfront so that we can check existing charges
				// against the final creditor for each autorateinfo.
				var validAutorateInfos =
					autoRatingResults
					.Where(rateInfo =>
						rateInfo != null &&
						rateInfo.ChargeCode != null &&
						!rateInfo.IsInclusiveCalculator);
				var newCostsByKey =
					PartiallyConstructJobCosts(validAutorateInfos, apportionmentListing.CostsCollection)
					.ToKeyListDictionary(x => new MergeKey(x));

				// Step 1: Remove from the new costs any that have a corresponding
				// existing cost that has the STP rating behaviour.
				DiscardNewCostsShouldNotBeAutorated(apportionmentListing.CostsCollection, newCostsByKey, costsByKeyShouldNotBeAutorated);

				// Step 2: Remove from the existing costs, with the REA rating
				// behaviour, any that have a corresponding new cost
				DeleteExistingCostsForReAutorating(apportionmentListing.CostsCollection, newCostsByKey, costsByKeyShouldBeReautorated, warningMessages);

				// Step 3: The remaining existing costs and new costs are all meant
				// to stay and so lets finalise their creation.
				FinishConstructingJobCosts(newCostsByKey, createdCosts, costOrSell, errorMessages, warningMessages);

				var currentCosts = apportionmentListing.CostsCollection.Cast<JobConsolCost>();
				UpdateExistingSpotCosts(costOrSell, adapters, currentCosts);

				ReportErrorsAndWarnings(interactor, errorMessages, warningMessages);
			}

			return new AutoRatesAdditionResult(createdCosts, new List<ChargeWrapper>(), 0, new List<AutoRateInfo>(autoRatingResults), costOrSell);
		}

		/// <summary>
		/// Returns a non-materialised enumerable of (JobConsolCost, AutoRateInfo)
		/// the JobConsolCosts is only initialised as far as the creditor
		/// and chargecode are concerned.
		/// </summary>
		IEnumerable<(JobConsolCost, AutoRateInfo)> PartiallyConstructJobCosts(IEnumerable<AutoRateInfo> results, JobConsolCostCollection collection)
		{
			return
				results
				.Select(rateInfo =>
				{
					var cost = collection.TryAddNew();

					if (cost != null)
					{
						using (GetActionToSuppressAutoRatingChange(cost))
						{
							cost.E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge;
							cost.Attributes.Add(rateInfo.Attributes);
							cost.SetChargeCodeAndCreditorForNewAutoRated(rateInfo.ChargeCode, rateInfo.ProviderPK);
						}
					}

					return (cost, rateInfo);
				})
				.Where(x => x.cost != null);
		}

		static void DiscardNewCostsShouldNotBeAutorated(
			JobConsolCostCollection costsCollection,
			Dictionary<MergeKey, List<(JobConsolCost Cost, AutoRateInfo Info)>> newCostsByKey,
			Dictionary<MergeKey, List<JobConsolCost>> costsByKeyShouldNotBeAutorated)
		{
			if (!newCostsByKey.Any())
			{
				return;
			}

			foreach (var existingCostGroup in costsByKeyShouldNotBeAutorated)
			{
				if (newCostsByKey.TryGetValue(existingCostGroup.Key, out var newCostsGroup))
				{
					var existingUnitSet =
						existingCostGroup.Value.SelectMany(c => c.PaymentBases.Cast<JobPaymentBasis>())
						.Select(x => (string)x.PBS_ChargeableUnit)
						.ToHashSet();

					// For new cost we're discarding, we need to remove from the dictionary and the collection
					var toRemove =
						newCostsGroup
						.Where(x => x.Info.Bases.Any(b => existingUnitSet.Contains(b.Chargeable.Unit)))
						.ToList();
					newCostsGroup.RemoveAll(x => toRemove.Contains(x));
					toRemove.ForEach(x => costsCollection.RemoveAndDelete(x.Cost));
				}
			}
		}

		static void DeleteExistingCostsForReAutorating(
			JobConsolCostCollection costsCollection,
			Dictionary<MergeKey, List<(JobConsolCost Cost, AutoRateInfo Info)>> newCostsByKey,
			Dictionary<MergeKey, List<JobConsolCost>> costsByKeyShouldBeReautorated,
			HashSet<string> warningMessages)
		{
			if (!costsByKeyShouldBeReautorated.Any())
			{
				return;
			}

			foreach (var newCostGroup in newCostsByKey)
			{
				if (costsByKeyShouldBeReautorated.TryGetValue(newCostGroup.Key, out var existingCostGroup))
				{
					var newUnitSet =
						newCostGroup.Value.SelectMany(x => x.Info.Bases.Select(b => (string)b.Chargeable.Unit))
						.ToHashSet();

					foreach (var existingCost in existingCostGroup)
					{
						if (existingCost.PaymentBases.Cast<JobPaymentBasis>().Any(b => newUnitSet.Contains(b.PBS_ChargeableUnit)))
						{
							var message = Res.GetString(
								"24AC5F77-3A51-4A7C-B599-EA7CB217FE3A",
								"The existing charge '{0} = {1}' with 'Rating Behavior = REA' has been deleted as a new charge has been found with similar Charge Code, Creditor, Chargeable Unit and Supplier Cost Reference.",
								existingCost.ChargeCode.AC_Code,
								existingCost.E6_OSCostAmount.ToStringTrimZeros());

							warningMessages.Add(message);

							costsCollection.RemoveAndDelete(existingCost);
						}
					}
				}
			}
		}

		void FinishConstructingJobCosts(
					Dictionary<MergeKey, List<(JobConsolCost Cost, AutoRateInfo Info)>> newCostsByKey,
					List<ChargeWrapper> createdCosts,
					CostSell costOrSell,
					HashSet<string> errorMessages,
					HashSet<string> warningMessages)
		{
			foreach (var costAndInfo in newCostsByKey.Values.SelectMany(x => x))
			{
				UpdateNewCost(costAndInfo.Info, costAndInfo.Cost, costOrSell);
				if (CanKeepCost(costAndInfo.Info, costAndInfo.Cost, errorMessages, warningMessages))
				{
					createdCosts.Add(new ChargeWrapper(costAndInfo.Cost, costAndInfo.Info));
				}
				else
				{
					costAndInfo.Cost.Delete();
				}
			}
		}

		static bool CanKeepCost(AutoRateInfo info, JobConsolCost newCost, HashSet<string> errorMessages, HashSet<string> warningMessages)
		{
			var canKeepCost =
				IsConsolCostValid(warningMessages, errorMessages, newCost) ||
				_Rating.CanCreateInvalidJobConsolCosts;

			if (canKeepCost)
			{
				if (!info.CalculationLogs.IsEmpty)
				{
					CalculationLogsLoader.Save(newCost, info.CalculationLogs);
				}

				return true;
			}
			return false;
		}

		void ReportErrorsAndWarnings(IAutoRatingGUIInteractor interactor, HashSet<string> errorMessages, HashSet<string> warningMessages)
		{
			if (errorMessages.Any())
			{
				var summaryMessage = Res.GetString("494c0a18-4cf2-4c0f-9639-00dca298aa34", "{0} has encountered the following errors while AutoRating:", HostBusinessEntity.HumanReadableName);
				interactor.ErrorWithSummary(summaryMessage, errorMessages);
			}

			if (warningMessages.Any())
			{
				var summarymessage = Res.GetString("ecc6f18a-dcbb-4d77-a8b3-aa8c8bb9db2d", "{0} has encountered the following warnings while AutoRating:", HostBusinessEntity.HumanReadableName);
				interactor.WarningWithSummary(summarymessage, warningMessages);
			}
		}

		class MergeKey
		{
			readonly ZString supplierReference;
			readonly ZGuid creditorPK;
			readonly ZString chargeCode;

			/// <summary>
			/// Constructs a MergeKey with charge information from partially
			/// created JobConsolCost and its associated AutoRateInfo
			/// costAndInfo, costAndInfo.ConsolCost, costAndInfo.AutoRateInfo and costAndInfo.AutoRateInfo.ChargeCode should not be null
			/// </summary>
			public MergeKey((JobConsolCost ConsolCost, AutoRateInfo AutoRateInfo) costAndInfo)
			{
				this.creditorPK = costAndInfo.ConsolCost.E6_OH_Creditor;
				this.supplierReference = costAndInfo.AutoRateInfo.OperationalJobRef;
				this.chargeCode = costAndInfo.AutoRateInfo.ChargeCode.AC_Code;
			}

			/// <summary>
			/// Constructs a MergeKey based on an existing Charge on a Job
			/// cost should not be null
			/// </summary>
			public MergeKey(JobConsolCost cost)
			{
				this.creditorPK = cost.E6_OH_Creditor;
				this.supplierReference = cost.E6_CostReference;
				this.chargeCode = cost.ChargeCode.AC_Code;
			}

			public override bool Equals(object obj)
			{
				var y = obj as MergeKey;
				if (y == null)
				{
					return false;
				}

				return this.chargeCode == y.chargeCode
					&& this.creditorPK == y.creditorPK
					&& this.supplierReference == y.supplierReference;
			}

			public override int GetHashCode()
			{
				return this.chargeCode.GetHashCode()
					^ this.creditorPK.GetHashCode()
					^ this.supplierReference.GetHashCode();
			}
		}

		void UpdateNewCost(AutoRateInfo rateInfo, JobConsolCost cost, CostSell costOrSell)
		{
			using (GetActionToSuppressAutoRatingChange(cost))
			{
				if (rateInfo.IsSpot)
				{
					cost.E6_RatingBehaviour = RatingBehaviours.Spot;
				}

				var costCurrency = HostBusinessEntity.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, rateInfo.Currency);
				if (costCurrency != null)
				{
					cost.E6_RX_NKCurrency = costCurrency.RX_Code;
				}

				cost.E6_OSCostAmount = rateInfo.Amount;
				rateInfo.Bases.ConvertToJobPaymentBases(costOrSell == CostSell.Cost, cost.PaymentBases.AddNew);
				cost.AgentDeclaredOSAmount = rateInfo.AgentAmount;
				cost.ApportionAgentDeclaredCost = true;

				if (rateInfo.ChargeUnit == RatingConstants.Units.HB)
				{
					cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				}
				else
				{
					cost.SplitApportionAmount();
					cost.ApportionGSTCharges();
				}

				cost.SetCalculationDescription(rateInfo);

				ApportionAttributes(((IApportionedChargesHeader)cost).Charges.OfType<BaseCharge>(), rateInfo);
			}
		}

		/// <summary>
		/// This copies the attributes from the given RateInfo to the apportioned charges.
		/// Container Number attributes are handled specially so only numbers on the target job are copied.
		/// </summary>
		void ApportionAttributes(IEnumerable<BaseCharge> charges, AutoRateInfo mainRateInfo)
		{
			// Note, currently when AutoRateInfos are merged their attributes are not combined.
			// The surviving AutoRateInfo keeps much the same attributes as it had before merging with others.
			// To find all the original attributes we look in all the original infos.
			var originalInfoList = mainRateInfo.MergedInfos.ToList();
			foreach (var charge in charges)
			{
				var infoForSettingChargeAttributes = mainRateInfo;
				var infoIndex = 0;

				if (charge.ShipmentInfo.InvoicingSupporter is IAutoRatingApportionmentTargetSupporter target)
				{
					var containerNumbers = target.GetDistinctContainerNumbers().ToHashSet();
					for (; infoIndex < originalInfoList.Count; infoIndex++)
					{
						var info = originalInfoList[infoIndex];
						var containerNumberAttributes = info.Attributes.Get(JobChargeAttribTypeList.Codes.ContainerNumber);
						if (containerNumberAttributes.Any(a => containerNumbers.Contains(a.Value)))
						{
							infoForSettingChargeAttributes = info;
							break;
						}
					}
				}

				charge.AddAttributes(infoForSettingChargeAttributes.Attributes);

				if (infoIndex < originalInfoList.Count)
				{
					originalInfoList.RemoveAt(infoIndex);
				}
			}
		}

		void UpdateExistingSpotCosts(CostSell costOrSell, IEnumerable<IAutoRating> adapters, IEnumerable<JobConsolCost> existingCosts)
		{
			var spotCosts =
				existingCosts.Where(cost =>
					cost.E6_RatingBehaviour != RatingBehaviours.StopFromAutorating &&
					cost.E6_RatingBehaviour != RatingBehaviours.CreateNewCharge &&
					cost.E6_RatingBehaviour != RatingBehaviours.ReAutorateCharge);

			if (costOrSell != CostSell.Cost)
			{
				return;
			}

			var host = adapters?.Where(a => a is IAutoRatingSpotChargeInfo).FirstOrDefault();

			if (host == null)
			{
				return;
			}

			foreach (var cost in spotCosts)
			{
				if (!cost.IsPosted && RatingBehaviours.IsAutoRatingOverriderSpotBehaviour(cost.E6_RatingBehaviour))
				{
					var paymentBasis = cost.PaymentBases.FirstOrDefault();
					if
						(
							paymentBasis != null
							&& paymentBasis is JobPaymentBasis basis
							&& !basis.PBS_ChargeableBasis.IsEmpty
						)
					{
						var calculator = new JobChargeQuickCalculateBusinessObject(host, cost);
						calculator.RunPreSaveValidation();
						if (!calculator.HasErrors)
						{
							calculator.SetCalculationResults();
						}
					}
				}
			}
		}

		DisposableAction GetActionToSuppressAutoRatingChange(JobConsolCost cost)
		{
			var setContext = new Action(() =>
			{
				cost.SetContext(BusinessContext.SuppressAutoRatingChange);
			});

			var removeContext = new Action(() =>
			{
				cost.RemoveContext(BusinessContext.SuppressAutoRatingChange);
			});

			return new DisposableAction(setContext, removeContext);
		}

		static bool IsConsolCostValid(HashSet<string> warningMessages, HashSet<string> errorMessages, JobConsolCost consolCost)
		{
			consolCost.IgnoreValidationSuspended = true;
			consolCost.RunPreSaveValidation();

			var errors = new ZNotificationCollector(consolCost, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors().ToList();
			var warnings = new ZNotificationCollector(consolCost, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetWarnings().ToList();
			consolCost.IgnoreValidationSuspended = false;

			foreach (var error in errors)
			{
				errorMessages.Add(error.Message);
			}

			foreach (var warning in warnings)
			{
				warningMessages.Add(warning.Message);
			}

			return !errors.Any();
		}

		public bool ShouldAddAutoRates
		{
			get { return true; }
		}

		public bool Supports(CostSell costOrSell)
		{
			return costOrSell == CostSell.Cost;
		}

		public RatingAdaptersProvider ParentProvider { get; }

		ZString IAutoRatingValidator.GetAutoratingNotPermittedReason()
		{
			var shipments = costingPlugIn?.CostSupporter?.ShipmentsList;

			if (shipments != null)
			{
				foreach (var shipment in shipments)
				{
					var jobHeader = shipment.InvoicingSupporter?.Job;
					if (jobHeader?.IsReadyForFinancialClosureWithoutModifySecurity ?? false)
					{
						return Res.GetString("eb5d0f63-1abe-4d9a-95d1-63f541160586", "Autorating cannot be run as its apportionment Job has Ready For Financial Closure status.");
					}

					if (jobHeader?.IsClosed ?? false)
					{
						return Res.GetString("600109be-e4ae-4902-9262-4f68f62fcd7c", "Autorating cannot be run as its apportionment Job has the Closed status.");
					}
				}
			}

			return string.Empty;
		}

		internal void DeletedUnusedJobs(IEnumerable<Job> jobsToDeleteIfNotLinked)
		{
			var jobsLinkedToConsol = apportionmentListing.GetJobsLinkedToConsolChargesAndCosts();
			foreach (var job in jobsToDeleteIfNotLinked)
			{
				if (!jobsLinkedToConsol.Contains(job.PK))
				{
					job.Delete();
				}
			}
		}
	}
}
