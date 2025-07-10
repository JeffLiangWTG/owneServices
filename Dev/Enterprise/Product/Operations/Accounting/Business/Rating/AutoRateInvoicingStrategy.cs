using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class AutoRateInvoicingStrategy : IAutoRatingStrategy
	{
		public AutoRateInvoicingStrategy(IBusiness businessObject, Job job, RatingAdaptersProvider parentProvider = null)
		{
			this.businessObject = Argument.NotNull(businessObject, "businessObject");
			this.job = job;
			ParentProvider = parentProvider;
		}

		readonly IBusiness businessObject;
		readonly Job job;

		public IBusiness HostBusinessEntity
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return businessObject; }
		}

		public Job Job
		{
			get { return job ?? new Job.Loader(HostBusinessEntity.Factory, (IJobHeaderParent)HostBusinessEntity).Load(); }
		}

		#region AddAutoRates

		public virtual AutoRatesAdditionResult AddAutoRates(IAutoRatingGUIInteractor interactor, AutoRateInfoCollection autoRatingResults, CostSell costOrSell, ZString[] adapterIDs, IEnumerable<IAutoRating> adapters = null)
		{
			using (Job.SuppressAutoRatingOverride())
			using (Job.Charges.SuspendListChanged())
			using (Job.FilteredCharges.SuspendListChanged())
			{
				Job.InitializeParentFromGenericJobWithSettingDefaults();
				var autoRatingResultsTemp = new AutoRateInfoCollection(Job.Factory);
				autoRatingResultsTemp.AddRange(autoRatingResults);
				CalculateCreditorOverrides(autoRatingResultsTemp, costOrSell);
				RemoveResultsWherePostedOrApportionedChargeExist(autoRatingResultsTemp, costOrSell, interactor);

				var createdCharges = new List<ChargeWrapper>();
				var modifiedCharges = new List<ChargeWrapper>();

				// This context creation is temporary change to detect deleting Job Charge by Auto Rating functionality.
				var chargesWithContext = new BusinessObjectListWithContext<Charge, BusinessContext>(BusinessContext.ReportDeletingCharges);

				try
				{
					chargesWithContext.AddRange(Job.Charges.ToArray<Charge>());

					var existingCharges = chargesWithContext.Where(c => !c.IsDeleted).ToArray();
					var matches = autoRatingResultsTemp.FindBestMatches(existingCharges, costOrSell);

					// Order by duplicates first, before the charge is updated by the original match
					foreach (var match in matches.OrderBy(x => x.duplicateCharge ? 0 : 1))
					{
						if (_Rating.IsBeingCopiedFromQuote(match.charge))
						{
							autoRatingResultsTemp.Remove(match.rateInfo);
							interactor.Information((NoResString)"Copying Charge '" + match.rateInfo.SingleLineDescription + (NoResString)"' from Quote");
						}
						else if (match.charge.CanReautorate(costOrSell, adapterIDs))
						{
							var charge = (Charge)match.charge;
							if (match.duplicateCharge)
							{
								charge = CreateRevenueDuplicate(charge);
							}
							UpdateExistingCharge(charge, match.rateInfo, costOrSell, createdCharges, modifiedCharges);
							autoRatingResultsTemp.Remove(match.rateInfo);
						}
					}
				}
				finally
				{
					chargesWithContext.Clear();
				}

				var newCharges = GetNewChargesFromAutoRatingResults(autoRatingResultsTemp, costOrSell, interactor);
				createdCharges.AddRange(newCharges);

				var result = new AutoRatesAdditionResult(createdCharges, modifiedCharges, 0, autoRatingResults.ToList(), costOrSell);

				Job.JH_RatingHasBeenRun = true;
				Job.ReOpenJobStatus();

				return result;
			}
		}

		Charge CreateRevenueDuplicate(Charge charge)
		{
			var newCharge = Job.Charges.AddNew();
			newCharge.CopyPersistentValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(charge);
			CopyBases(charge, newCharge);
			return newCharge;
		}

		static void CopyBases(Charge charge, Charge newCharge)
		{
			foreach (JobPaymentBasis basis in charge.PaymentBases)
			{
				var newBasis = (JobPaymentBasis)basis.Clone();
				newBasis.PBS_JR = newCharge.PK;
				newCharge.PaymentBases.Add(newBasis);
			}
		}

		void CalculateCreditorOverrides(AutoRateInfoCollection autoRatingResults, CostSell costOrSell)
		{
			var invJob = Job;
			var parentConsumerType = ParentProvider?.ConsumerType;
			var jobDepartment = invJob.Department;

			foreach (var info in autoRatingResults)
			{
				// use criteria ConsumerType not job JobType to distinguish bizo's that have the same job header, like shipment and transport booking
				var consumerType = info.ConsumerType;
				var chargeCode = info.ChargeCode;
				if (chargeCode != null && consumerType != null)
				{
					var creditorOverrides = chargeCode.CreditorOverrides;

					// shipment revenue charges must use consol creditor overrides for consol level charges
					// for the costs to link up with the revenue
					if (costOrSell == CostSell.Revenue && parentConsumerType != null && chargeCode.AC_IsGroupageCharge)
					{
						consumerType = parentConsumerType;
					}
					else if (invJob.PlugInData is IGateway gatewayConsol
						&& (gatewayConsol.GatewayBillingSupporter?.IsGatewayBillingEnabled() ?? false))
					{
						consumerType = JobInvoicingConsumerTypes.GatewayConsol;
					}

					var department = BaseCharge.CalculateDepartmentFromJobTypeAndChargeCodeType(invJob.Factory, invJob, chargeCode, jobDepartment);
					var defaultCreditor = creditorOverrides.GetCreditorOverride(
												invJob.PlugInData?.InvoicingSupporter,
												costOrSell,
												consumerType,
												invJob.Direction,
												invJob.TransportMode,
												department?.PK,
												invJob.OverseasAgentIsApplicable,
												JobHeaderHelper.GetOverseasCreditorPK(invJob));
					if (defaultCreditor != null)
					{
						info.CreditorOverridePK = defaultCreditor.Value;
					}
				}
			}
		}

		void RemoveResultsWherePostedOrApportionedChargeExist(AutoRateInfoCollection autoRatingResults, CostSell costOrSell, ILogger logger)
		{
			if (Job.Charges.Any() && autoRatingResults.Any())
			{
				bool ShouldNotAutoRateCharge(Charge charge)
				{
					var result = costOrSell == CostSell.Cost
						? charge.JR_Calc_CostRatingBehavior == JobChargeLookups.StopFromAutorating
						: charge.JR_Calc_SellRatingBehavior == JobChargeLookups.StopFromAutorating;

					return result;
				}

				var stopAutoRatingCharges = Job.Charges.Cast<Charge>().Where(ShouldNotAutoRateCharge).ToList();

				bool ShouldRemoveAutoRateInfo(AutoRateInfo info)
				{
					if (costOrSell == CostSell.Cost)
					{
						return stopAutoRatingCharges
							.Any(c =>
								c.ChargeCode?.PK == info.ChargeCode?.PK
								&& EmptyOrEqual(c.JR_CostReference, info.OperationalJobRef)
								&& HasSameCreditor(info, c)
								&& EmptyOrEqual(c.JR_Calc_RelatedJobNumber, info.JobRef)
							);
					}
					else
					{
						return stopAutoRatingCharges
							.Any(c =>
								c.ChargeCode?.PK == info.ChargeCode?.PK
								&& EmptyOrEqual(c.JR_SellReference, info.OperationalJobRef)
								&& EqualOrEmpty(c.JR_OH_SellAccount, info.DebtorOverridePK)
								&& EmptyOrEqual(c.JR_Calc_RelatedJobNumber, info.JobRef)
							);
					}
				}

				var ratingResultsToRemove = autoRatingResults.Where(ShouldRemoveAutoRateInfo).ToList();
				foreach (var key in ratingResultsToRemove)
				{
					autoRatingResults.Remove(key);
					logger.Information($"Skipping Rate '{key.SingleLineDescription}' as an apportioned or posted charge with the same code already exists on the Job with 'Rating Behavior' set to STP. (Job Number : {key.JobRef})");
				}
			}
		}

		static bool HasSameCreditor(AutoRateInfo info, Charge c) => info.HasSameCreditor(c.JR_OH_CostAccount);

		bool EqualOrEmpty(ZGuid pk1, ZGuid pk2)
		{
			return pk1.IsEmpty || pk2.IsEmpty || pk1.Equals(pk2);
		}

		bool EmptyOrEqual(string str1, string str2)
		{
			return string.IsNullOrEmpty(str1) || str1.Equals(str2);
		}

		void UpdateExistingCharge(Charge existingCharge, AutoRateInfo rateInfo, CostSell costOrSell, List<ChargeWrapper> createdCharges, List<ChargeWrapper> modifiedCharges)
		{
			var autoratedCharge = Job.SetAmountsOnCharge(existingCharge, rateInfo, costOrSell);
			if (autoratedCharge != null)
			{
				RemoveCreditorForIntercompanyRevenueChargeWhenBothCostAndSellAccountAreOrgProxies(autoratedCharge, rateInfo, costOrSell);
				if (autoratedCharge.JR_Sell_LocalSellAmount != 0 && !autoratedCharge.JR_OH_SellAccount.IsValid)
				{
					autoratedCharge.ResetChargeDebtor();
				}

				if (!autoratedCharge.IsInDatabase)
				{
					createdCharges.Add(new ChargeWrapper(autoratedCharge, rateInfo));
				}
				else if (autoratedCharge.HasChanges)
				{
					modifiedCharges.Add(new ChargeWrapper(autoratedCharge, rateInfo));
				}

				SetGatewaySellChargeInternalJob(autoratedCharge, rateInfo);
			}
		}

		IEnumerable<ChargeWrapper> GetNewChargesFromAutoRatingResults(AutoRateInfoCollection autoRatingResultsTemp, CostSell costOrSell, IAutoRatingGUIInteractor interactor)
		{
			var createdChargesToAdd = new List<ChargeWrapper>();
			var existingCharges = Job.Charges.Cast<BaseCharge>().ToList();
			var errorMessages = new HashSet<string>();

			if (Job.Parent.IsGatewayBillingEnabled())
			{
				var apportionmentsListing = ((IJobCostingPlugIn)Job.Parent).GetApportionments(true);
				apportionmentsListing.LoadChildShipmentsAndAcquireMutexesWhereRequired();
			}

			foreach (AutoRateInfo rateInfo in autoRatingResultsTemp.Where(x => !x.IsInclusiveCalculator))
			{
				var newCharge = Job.Charges.AddNew();

				using (newCharge.GetValidationSuspender())
				{
					bool hasAmount = !rateInfo.Amount.IsEmpty || rateInfo.HasExplicitZeroAmount;
					var isIntercompanyTariff = rateInfo.Line?.IsIntercompanyTariff() ?? false;
					newCharge.SetChargeCodeAndCreditorForNewAutoRated(rateInfo.ChargeCode, costOrSell, hasAmount, rateInfo.ProviderPK, rateInfo.CreditorOverridePK, isIntercompanyTariff);

					if (rateInfo.InvoicingSupporter != null)
					{
						newCharge.JR_CostReference = rateInfo.OperationalJobRef;
					}

					if (newCharge.RelatedShipments().Any(s => s.JobNumber == rateInfo.JobRef))
					{
						newCharge.JR_Calc_RelatedJobNumber = rateInfo.JobRef;
					}

					newCharge.JR_SellReference = rateInfo.SellReferenceNumber;

					SetGatewaySellChargeInternalJob(newCharge, rateInfo);
					Job.SetAmountsOnCharge(newCharge, rateInfo, costOrSell, canUpdateCreditor: false);
					ResetDebtorCreditorForIntercompanyCharge(newCharge, rateInfo, costOrSell, isIntercompanyTariff);
				}

				RunLiteValidation(newCharge.Validation);

				createdChargesToAdd.Add(new ChargeWrapper(newCharge, rateInfo));

				if (newCharge.HasErrors())
				{
					var errors = new ZNotificationCollector(newCharge, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
					errors.GetErrors().ForEach(e => errorMessages.Add(e.Message));
				}
			}

			if (errorMessages.Any())
			{
				var summaryMessage = Res.GetString("494c0a18-4cf2-4c0f-9639-00dca298aa34", "{0} has encountered the following errors while AutoRating:", Job.JH_JobNum);
				interactor.ErrorWithSummary(summaryMessage, errorMessages);
			}

			if (existingCharges.Any())
			{
				var comparer = new ChargePaymentBasisComparer(costOrSell, ChargeComparison.SameChargeableButDifferentRate);
				var duplicateCharges = existingCharges
					.Intersect(createdChargesToAdd.Select(x => x.Charge).Cast<BaseCharge>(), comparer).Distinct();

				AddWarningIfDuplicateChargesCreated(duplicateCharges, interactor);
			}

			return createdChargesToAdd;
		}

		void ResetDebtorCreditorForIntercompanyCharge(Charge chargeToUpdate, AutoRateInfo rateInfo, CostSell costOrSell, bool isIntercompanyTariff)
		{
			if (isIntercompanyTariff)
			{
				if (!rateInfo.HasExplicitZeroAmount)
				{
					if (chargeToUpdate.JR_LocalCostAmt == 0 && chargeToUpdate.JR_OSCostAmt == 0)
					{
						chargeToUpdate.JR_OH_CostAccount = ZGuid.Empty;
					}

					if (chargeToUpdate.JR_LocalSellAmt == 0 && chargeToUpdate.JR_OSSellAmt == 0)
					{
						chargeToUpdate.JR_OH_SellAccount = ZGuid.Empty;
					}
				}

				if (!chargeToUpdate.JR_OH_SellAccount.IsValid
					&& (chargeToUpdate.JR_LocalSellAmt != 0 || rateInfo.HasExplicitZeroAmount))
				{
					var job = chargeToUpdate.InvoicingJob;
					var debtorPK = job.GetDebtorPK(chargeToUpdate);
					if (debtorPK.IsValid)
					{
						var debtor = job.Factory.Load<OrgHeader>(debtorPK);
						if (debtor.OH_IsDebtor)
						{
							chargeToUpdate.JR_OH_SellAccount = debtorPK;
							RemoveCreditorForIntercompanyRevenueChargeWhenBothCostAndSellAccountAreOrgProxies(chargeToUpdate, rateInfo, costOrSell);

							Job.SetChargeRevenueDescription(rateInfo, chargeToUpdate);
							chargeToUpdate.SetRevenueCalculationDescription(rateInfo);
						}
					}
				}
			}
		}

		/// <summary>
		/// if Creditor and Debtor are both org-proxies and it's revenue, we will clear Creditor to be able to set Debtor without validation error
		/// </summary>
		/// <param name="chargeToUpdate"></param>
		/// <param name="rateInfo"></param>
		/// <param name="costOrSell"></param>
		void RemoveCreditorForIntercompanyRevenueChargeWhenBothCostAndSellAccountAreOrgProxies(Charge chargeToUpdate, AutoRateInfo rateInfo, CostSell costOrSell)
		{
			if (costOrSell == CostSell.Revenue
				&& !chargeToUpdate.JR_OH_CostAccount.IsEmpty
				&& !chargeToUpdate.JR_CostRated
				&& !chargeToUpdate.JR_CostRatingOverride
				&& AutoJRJRegistryStatusHelper.IsAutoJRJEnabled()
				&& (rateInfo.Line?.IsIntercompanyTariff() ?? false)
				&& chargeToUpdate.SellAccountIsOrgProxy
				&& chargeToUpdate.CostAccountIsOrgProxy)
			{
				chargeToUpdate.JR_OH_CostAccount = ZGuid.Empty;
			}
		}

		void RunLiteValidation(ChargeValidation validation)
		{
			validation.ValidateJR_AC();
			validation.ValidateJR_Desc();

			validation.ValidateJR_JH_InternalJob();
			validation.ValidateJR_GE();

			validation.ValidateJR_InvoiceType();

			validation.ValidateJR_OH_SellAccount();
			validation.ValidateJR_OH_CostAccount();

			validation.ValidateJR_RX_NKSellCurrency();
			validation.ValidateJR_OSSellAmt();
			validation.ValidateJR_LocalSellAmt();
			validation.ValidateJR_AgentDeclaredSellAmt();

			validation.ValidateJR_RX_NKCostCurrency();
			validation.ValidateJR_OSCostAmt();
			validation.ValidateJR_LocalCostAmt();
			validation.ValidateJR_AgentDeclaredCostAmt();

			validation.ValidateRow();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Warning string")]
		void AddWarningIfDuplicateChargesCreated(IEnumerable<BaseCharge> duplicateCharges, IAutoRatingGUIInteractor interactor)
		{
			if (duplicateCharges.Any())
			{
				var message = Res.GetString("e34e0d78-c396-4b7b-bf8b-04a1963676cf", "Autorating has produced new charges for the same charge codes as pre-existing charges which resulted in following charges conflicting:") + "\r\n\r\n";

				foreach (var chargeCode in duplicateCharges)
				{
					message += "  " + "\u2022" + " ";
					message += chargeCode + System.Environment.NewLine;
				}

				message += "\r\n" + Res.GetString("bebf70c0-271b-410e-bae5-317b4be3280d", @"Autorating deletes pre-existing charges unless one of the following conditions apply:
 - the amount is Posted
 - the amount is Apportioned
 - the amount's 'Rating Behavior' is set to NEW
 - the charge type of the charge code is 'Comment'.
Pre-existing posted charge for the same debtor/creditor causes Autorating to discard matching newly autorated charges.");
				interactor.Warning(message);
			}
		}

		void SetGatewaySellChargeInternalJob(Charge gatewaySellCharge, AutoRateInfo rateInfo)
		{
			var setInternalJob = gatewaySellCharge.Job.IsGatewayBillingJob();
			setInternalJob = setInternalJob && !gatewaySellCharge.IsInternalJobInfoDisabled;
			setInternalJob = setInternalJob && AutoJRJRegistryStatusHelper.IsAutoJRJEnabled();

			if (setInternalJob)
			{
				var shipmentJob = rateInfo.InvoicingSupporter?.Job;

				setInternalJob = setInternalJob && shipmentJob != null;
				setInternalJob = setInternalJob && shipmentJob.PK != gatewaySellCharge.JR_JH;

				if (setInternalJob)
				{
					if (AccountingMasterFilesRegistry.Instance.GetInternalJobConfigurationSetting.Value)
					{
						gatewaySellCharge.JR_JH_InternalJob = GatewayInvoiceTargetJobFinder.GetInternalJob(shipmentJob, gatewaySellCharge.JR_Calc_RelatedJobNumber, gatewaySellCharge.InvoicingJob);
					}
					else
					{
						gatewaySellCharge.JR_JH_InternalJob = shipmentJob.PK;
					}
				}
			}
		}

		#endregion

		public bool ShouldAddAutoRates => Job != null && (!Job.IsClosed || JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, job));

		public bool Supports(CostSell costOrSell)
		{
			return true;
		}

		public RatingAdaptersProvider ParentProvider { get; }

		public override bool Equals(object obj)
		{
			var other = obj as AutoRateInvoicingStrategy;
			return other != null && other.HostBusinessEntity.Equals(HostBusinessEntity);
		}

		public override int GetHashCode()
		{
			return HostBusinessEntity.GetHashCode();
		}
	}
}
