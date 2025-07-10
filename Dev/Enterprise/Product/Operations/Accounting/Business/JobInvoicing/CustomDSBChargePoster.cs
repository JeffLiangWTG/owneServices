using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class CustomsDisbursementChargePoster : ICustomsDisbursementChargePoster
	{
		public CustomsDisbursementChargePoster(ChargePosterBehaviours chargePosterBehaviours, ZGuid[] customsDSBChargeCodes)
		{
			this.ChargePosterBehaviours = chargePosterBehaviours;
			this.customsDSBChargeCodeList = customsDSBChargeCodes;
			this.customsDSBChargeCodes = new ReadOnlyCollection<ZGuid>(this.customsDSBChargeCodeList);
		}

		readonly ChargePosterBehaviours ChargePosterBehaviours;
		readonly ReadOnlyCollection<ZGuid> customsDSBChargeCodes;
		readonly ZGuid[] customsDSBChargeCodeList;

		#region Invoice Creation

		/// <summary>
		/// Triggers AutoBilling to create Customs Charges as long as there are no errors.
		///
		/// Sending an email is an option that is passed to ChargePosterBehaviours.
		/// IAutoBillingResult should have warnings and notifications that users need to see at the end of this operation.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html tags")]
		public IAutoBillingResult RaiseInvoices(IAccIntegrationDataProvider provider)
		{
			var company = provider.Company;
			if (company.PK != GlbCompany.CurrentCompany.PK)
			{
				var errorMessage = "<p>"
					+ Res.GetString("B351D608-C712-4464-A116-696E30CCA38B", "Auto-Billing is attempted in the company '{0}' for a {1} {2} that belongs to the company {3}.", GlbCompany.CurrentCompany.GC_Code, provider.JobType, provider.ReferenceID, company.GC_Code)
					+ "</p><p>"
					+ AutoCustomInvoiceFailureEmail.GetCustomsChargeDetails(provider)
					+ "</p>";

				throw new CustomsInvoiceRaiseException(errorMessage);
			}

			var allAutoRateInfos = new Dictionary<ICustomsJobInfo, AutoRateInfoCollection>();
			var validationResults = new Dictionary<ZGuid, List<CUSDSBPostingValidationResult>>();
			var customsJobs = new List<ICustomsJobInfo>();
			var factory = provider.Factory;
			var autoRated = false;

			foreach (var dataProvider in provider.InvDataProviders)
			{
				var customsJob = dataProvider.CustomsJob;

				if (customsJob != null && customsJob.Branch != null && customsJob.Branch.GB_GC != GlbCompany.CurrentCompany.PK)
				{
					var errorMessage = "<p>"
						+ Res.GetString("74EC816A-B46D-467C-9158-47D1006E2EB5", "Auto-Billing is attempted in the company {0} for a customs declaration {1} that belongs to the company {2}. System will not proceed and will not perform auto-billing.", GlbCompany.CurrentCompany.GC_Code, customsJob.JobNumber, customsJob.Branch.Company.GC_Code)
						+ "</p><p>"
						+ AutoCustomInvoiceFailureEmail.GetCustomsChargeDetails(provider)
						+ "</p>";

					throw new CustomsInvoiceRaiseException(errorMessage);
				}

				var validationResult = ProcessForOneDataProvider(dataProvider, out var customsDSBAutoRateInfos);

				if (customsJob != null && !customsJobs.Contains(customsJob))
				{
					customsJobs.Add(customsJob);
				}

				if (!validationResult.HasErrors)
				{
					if (customsJob != null && customsDSBAutoRateInfos != null)
					{
						if (!allAutoRateInfos.TryGetValue(customsJob, out var aggregatedAutoRateInfos))
						{
							aggregatedAutoRateInfos = new AutoRateInfoCollection(customsJob.Factory);
						}
						AggregateSellAndCostAmountInto(aggregatedAutoRateInfos, customsDSBAutoRateInfos);
						allAutoRateInfos[customsJob] = aggregatedAutoRateInfos;
						autoRated = true;
					}
				}

				AddValidationResultIfNecessary(validationResults, customsJob, validationResult);
			}
			try
			{
				if (autoRated)
				{
					factory.Save();
					provider.LogPostingResult(Res.GetString("2e6f142b-69a9-413d-8310-4baf7f63acbf", "Job(s) is auto rated and saved."));
				}

				var customsJobsToPost = customsJobs.ToArray();

				bool hasChanges = PostAPInvoiceIfRequired(customsJobsToPost, validationResults);

				if (hasChanges)
				{
					provider.LogPostingResult(Res.GetString("ec564081-3fc8-46f1-b3f3-0962a62227df", "AP posted for ") + provider.JobType + " " + provider.ReferenceID);
					factory.Save();
				}

				var aRPosted = ValidateAndPostARInvoicesIfRequired(customsJobsToPost, allAutoRateInfos, validationResults);

				if (aRPosted)
				{
					provider.LogPostingResult(Res.GetString("244dc09b-1af9-4d2f-aed7-e8396960ae10", "AR posted for ") + provider.JobType + " " + provider.ReferenceID);
					factory.Save();
				}

				hasChanges |= aRPosted;

				return ProcessResultFromValidationResults(validationResults, factory, provider.ReferenceID, provider.BusinessObjectPK, provider.ControllerID, provider.JobType, hasChanges);
			}
			catch (OnSavingCriticalCheckException ex)
			{
				throw new CustomsInvoiceRaiseException("<p>" + ex.Message + "</p><p>" + AutoCustomInvoiceFailureEmail.GetCustomsChargeDetails(provider) + "</p>");
			}
		}

		void AddValidationResultIfNecessary(Dictionary<ZGuid, List<CUSDSBPostingValidationResult>> validationResults, ICustomsJobInfo customsJob, CUSDSBPostingValidationResult validationResult)
		{
			if (validationResult.HasMessages)
			{
				var key = customsJob?.PK ?? ZGuid.Empty;
				if (!validationResults.TryGetValue(key, out var listOfValidations))
				{
					listOfValidations = new List<CUSDSBPostingValidationResult>();
				}

				if (!listOfValidations.Contains(validationResult))
				{
					listOfValidations.Add(validationResult);
				}

				validationResults[key] = listOfValidations;
			}
		}

		#endregion

		#region Invoice Posting

		CUSDSBPostingValidationResult ProcessForOneDataProvider(IAccInvoiceDataProvider dataProvider, out AutoRateInfoCollection autoRateInfos)
		{
			autoRateInfos = RunAutoRating(dataProvider);

			var validator = new CustomsDSBChargePostValidator(ChargePosterBehaviours, customsDSBChargeCodeList);
			var result = validator.ValidateFor(dataProvider, autoRateInfos);

			var customsJob = dataProvider.CustomsJob;

			if (customsJob != null)
			{
				UpdateUnpostedExistingCharges(customsJob, dataProvider, autoRateInfos);

				if (!dataProvider.HasBeenWithdrawn)
				{
					if (ShouldAutoRateDSB && !NoCustomsDisbursementChargeToAdd(autoRateInfos))
					{
						using (var invoicingJob = GetOrCreateCustomsJob(customsJob))
						{
							if (invoicingJob != null)
							{
								invoicingJob.SetContext(BusinessContext.JobIsSavedSoonAfterDisposal_RecheckIfThisStillTrue);
								CreateOrUpdateCustomsDSBCharges(customsJob, dataProvider, autoRateInfos, invoicingJob);
								BalanceDisbursementCharges(invoicingJob, customsJob);
							}
							else
							{
								throw new CustomsInvoiceRaiseException(Res.GetString("1C39B8BB-973A-4D49-9EB6-2FC07BF8DB29", "Failed to load/create Invoicing Job, please manually create a valid Invoicing Job in 'Billing' tab."));
							}
						}
					}
				}
			}

			return result;
		}

		void UpdateUnpostedExistingCharges(ICustomsJobInfo customsJob, IAccInvoiceDataProvider dataProvider, AutoRateInfoCollection autoRateInfos)
		{
			if (dataProvider.HasBeenWithdrawn || NoCustomsDisbursementChargeToAdd(autoRateInfos))
			{
				var invoicingJob = GetExistingJob(customsJob);

				if (invoicingJob != null)
				{
					var chargesToClear = invoicingJob.Charges.Where(c => !c.JR_IsApportioned && IsCustomsDSBCharge(c) && dataProvider.MatchCustomsChargesToClear(c.JR_APInvoiceNum, c.JR_Desc)).ToArray();

					foreach (Charge charge in chargesToClear)
					{
						using (charge.SuppressAutoRatingOverride())
						{
							ClearAmountsForNoRate(invoicingJob, charge);
						}
					}
				}
			}
		}

		void ClearAmountsForNoRate(Job invoicingJob, Charge charge)
		{
			using (ReOpenJobIfChargeHasChanges(charge, invoicingJob))
			{
				if (!charge.IsCostPosted)
				{
					charge.JR_LocalCostAmt = 0m;
					charge.JR_OSCostAmt = 0m;
				}

				if (!charge.IsRevenuePosted)
				{
					charge.JR_LocalSellAmt = 0m;
					charge.JR_EstimatedRevenue = 0;
					charge.JR_OSSellAmt = 0m;
				}
			}
		}

		bool NoCustomsDisbursementChargeToAdd(AutoRateInfoCollection rateInfos)
		{
			foreach (var rateInfo in rateInfos)
			{
				if (!rateInfo.LocalAmount.IsEmpty
					|| IsForCustomsDeferredCharge(rateInfo)
					|| IsForCustomsDSBCharge(rateInfo))
				{
					return false;
				}
			}

			return true;
		}

		bool IsForCustomsDeferredCharge(AutoRateInfo rateInfo)
		{
			return rateInfo.ChargeCode.PK.ToGuid() == RatingDataRegistry.Instance.CustomDeferredChargeCode.Value
					&& RatingDataRegistry.Instance.IncludeCustomDeferredChargeInInvoicing.Value;
		}

		AutoRateInfoCollection RunAutoRating(IAccInvoiceDataProvider dataProvider)
		{
			AutoRateInfoCollection result = null;

			if (dataProvider.CustomsJob != null)
			{
				ICustomsCharges[] charges = dataProvider.CustomsCharges;
				var customsChargeManager = new CustomsChargesManager(dataProvider.CustomsJob as IBusiness);
				customsChargeManager.PopulateChargeCodesForEmptyCustomsCharges(charges);
				result = customsChargeManager.RateCustomsCharges(charges);
			}

			return result;
		}

		/// <summary>
		/// Performs validations for posting AR and if no errors exist, it attempts to post AR invoices.
		/// </summary>
		/// <returns>Whether any AR invoices are posted</returns>
		bool ValidateAndPostARInvoicesIfRequired(ICustomsJobInfo[] customsJobs, Dictionary<ICustomsJobInfo, AutoRateInfoCollection> allAutoRateInfos, Dictionary<ZGuid, List<CUSDSBPostingValidationResult>> validationResults)
		{
			bool hasRaised = false;

			CustomsDSBChargePostValidator validator = new CustomsDSBChargePostValidator(ChargePosterBehaviours, customsDSBChargeCodeList);

			foreach (ICustomsJobInfo customsJob in customsJobs)
			{
				allAutoRateInfos.TryGetValue(customsJob, out var customsChargesAutoRateInfos);

				if (customsChargesAutoRateInfos != null)
				{
					CUSDSBPostingValidationResult validationResult = validator.ValidateFor(customsJob, customsChargesAutoRateInfos);

					AddValidationResultIfNecessary(validationResults, customsJob, validationResult);

					if (!HasErrors(validationResults, customsJob))
					{
						hasRaised |= PostARInvoicesIfRequired(customsJob);

						if (HasUnpostedARWhenAPPostedForCustomsDSBCharges(customsJob) && !(validationResult.AutoPostingNotification?.SuppressUnpostARNotificationWhenAPPosted ?? false))
						{
							validationResult.AddWarning(Res.GetString("5eb5ddfd-c5f6-4916-92fc-d3151f2509fc", "The job does not have an AR invoice posted while there is an AP invoice posted for Customs Disbursement costs."));

							AddValidationResultIfNecessary(validationResults, customsJob, validationResult);
						}
					}
				}
			}

			return hasRaised;
		}

		bool HasUnpostedARWhenAPPostedForCustomsDSBCharges(ICustomsJobInfo customsJob)
		{
			Job invoicingJob = GetExistingJob(customsJob);

			//System should warn if no AR is posted while AP is posted.
			return invoicingJob != null && invoicingJob.Charges.HasUnpostedARWhenAPPosted(customsDSBChargeCodeList);
		}

		/// <returns>Whether any AP invoices are posted for the passed CustomsJobs</returns>
		bool PostAPInvoiceIfRequired(ICustomsJobInfo[] allCustomsJobs, Dictionary<ZGuid, List<CUSDSBPostingValidationResult>> validationResults)
		{
			bool hasRaised = false;

			if (ShouldAPPostDSB || ShouldARAPPostNonDSB)
			{
				foreach (ICustomsJobInfo customsJob in allCustomsJobs)
				{
					if (!HasErrors(validationResults, customsJob))
					{
						Job invoicingJob = GetExistingJob(customsJob);
						JobInvoicingPostingOption option = !ShouldARAPPostNonDSB && ShouldAPPostDSB ? JobInvoicingPostingOption.CustomsDSBChargeAPOnly : JobInvoicingPostingOption.Costs;
						if (invoicingJob != null)
						{
							hasRaised |= PostInvoice(invoicingJob, option);
						}
					}
				}
			}

			return hasRaised;
		}

		bool HasErrors(Dictionary<ZGuid, List<CUSDSBPostingValidationResult>> validationResults, ICustomsJobInfo customsJob)
		{
			bool result = false;

			if (validationResults.TryGetValue(customsJob.PK, out var listOfValidations))
			{
				foreach (CUSDSBPostingValidationResult validationResult in listOfValidations)
				{
					if (validationResult.HasErrors)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		/// <returns>Whether an AR invoice is posted for the passed CustomsJob</returns>
		bool PostARInvoicesIfRequired(ICustomsJobInfo parent)
		{
			bool hasRaised = false;

			if (ShouldARPostDSB || ShouldARAPPostNonDSB)
			{
				Job invoicingJob = GetExistingJob(parent);
				if (invoicingJob != null)
				{
					var option = !ShouldARAPPostNonDSB && ShouldARPostDSB ? JobInvoicingPostingOption.CustomsDSBChargeAROnly : JobInvoicingPostingOption.Revenue;

					hasRaised |= PostInvoice(invoicingJob, option);
				}
			}

			return hasRaised;
		}

		/// <returns>Whether an invoice is posted</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded HTML")]
		bool PostInvoice(Job invoicingJob, JobInvoicingPostingOption option)
		{
			bool invoiceIsRaised = false;
			if (invoicingJob != null)
			{
				Predicate<Charge> isEligibleForAPPosting = null;
				Predicate<Charge> isEligibleForARPosting = null;

				if (ShouldAPPostDSB && !ShouldARAPPostNonDSB)//Customs DSB only
				{
					isEligibleForAPPosting = x => IsCustomsDSBCharge(x)
												  && x.PostAPWhenInvokedByCustomInvoiceCreator
												  && (x.JR_LocalCostAmt > 0m || (x.JR_LocalCostAmt < 0m && ShouldPostNegativeCost));
				}
				else if (!ShouldAPPostDSB && ShouldARAPPostNonDSB)//Non-DSB only
				{
					isEligibleForAPPosting = x => !IsCustomsDSBCharge(x);
				}
				else
				{
					isEligibleForAPPosting = x => !IsCustomsDSBCharge(x) ||
												  (
													  x.PostAPWhenInvokedByCustomInvoiceCreator
													  && (x.JR_LocalCostAmt > 0m || (x.JR_LocalCostAmt < 0m && ShouldPostNegativeCost))
												  );
				}

				if (ShouldARPostDSB && !ShouldARAPPostNonDSB)//Customs DSB only
				{
					isEligibleForARPosting = x => IsCustomsDSBCharge(x) && x.PostARWhenInvokedByCustomInvoiceCreator && x.JR_LocalSellAmt > 0m;
				}
				else if (!ShouldARPostDSB && ShouldARAPPostNonDSB)//Non-DSB only
				{
					isEligibleForARPosting = x => !IsCustomsDSBCharge(x);
				}
				else
				{
					isEligibleForARPosting = x => !IsCustomsDSBCharge(x) || (x.PostARWhenInvokedByCustomInvoiceCreator && x.JR_LocalSellAmt > 0m);
				}

				var jobCollection = new[] { invoicingJob };
				var prepostValidation = new PostManagerValidation(jobCollection, option, jobCollection);
				var validationResult = prepostValidation.Validate();
				if (validationResult != null && validationResult.Type == CargoWise.ComponentModel.NotificationType.Error)
				{
					var errorMessage = "<p>"
						+ Res.GetString("0F9B6A1C-E9C6-4CEA-9D72-D6D2C2996419", "The following errors occurred while trying to post {0} transactions on Job {1}: {2}{3}"
							, GetTransactionType(option), invoicingJob.JH_JobNum, System.Environment.NewLine, validationResult.Message)
						+ "</p><p>"
						+ AutoCustomInvoiceFailureEmail.GetCustomsChargeDetails(invoicingJob)
						+ "</p>";

					throw new CustomsInvoiceRaiseException(errorMessage);
				}

				InvoicingPostManager postManager = new InvoicingPostManager(invoicingJob, isEligibleForAPPosting, isEligibleForARPosting);
				postManager.OnNothingPosted += delegate { invoiceIsRaised = false; };

				string lastPostingError = "";
				postManager.OnCriticalPostError += (sender, e) => { lastPostingError = e.ErrorMessage; };

				invoiceIsRaised = true;//should be set before running the following code which might trigger the above delegation

				postManager.CreateTransactions(option);

				if ((invoiceIsRaised && postManager.CancelPosting) || !string.IsNullOrEmpty(lastPostingError))
				{
					var errorMessage = "<p>"
						+ Res.GetString("AA8857E6-01F0-432F-A66A-9AC4A8791736", "Job {0} has errors and transactions posting of {1} is canceled. Errors: {2}{3}",
							invoicingJob.JH_JobNum, GetTransactionType(option), System.Environment.NewLine, lastPostingError)
						+ "</p><p>"
						+ AutoCustomInvoiceFailureEmail.GetCustomsChargeDetails(invoicingJob)
						+ "</p>";

					throw new CustomsInvoiceRaiseException(errorMessage);
				}
			}
			return invoiceIsRaised;
		}

		static string GetTransactionType(JobInvoicingPostingOption option)
		{
			switch (option)
			{
				case JobInvoicingPostingOption.CustomsDSBChargeAPOnly: return Res.GetString("8d40040f-384f-4089-937e-db24c79d736d", "Customs DSB AP");
				case JobInvoicingPostingOption.CustomsDSBChargeAROnly: return Res.GetString("efcf587e-a41c-4e0b-993f-798fbaa55152", "Customs DSB AR");
				case JobInvoicingPostingOption.Costs: return "AP";
				case JobInvoicingPostingOption.Revenue: return "AR";
				default: return option.ToString();
			}
		}

		#endregion

		#region Job Related

		Job GetOrCreateCustomsJob(ICustomsJobInfo parent)
		{
			Job jobHeader = GetExistingJob(parent);

			if (jobHeader == null)
			{
				GlbDepartment department = GetParentDepartment(parent);
				var branch = parent.Branch;
				if (department != null && branch != null)
				{
					jobHeader = GetOrCreateJob(parent);
					if (jobHeader != null && !jobHeader.IsInDatabase)
					{
						jobHeader.JH_GB = branch.PK;
						jobHeader.JH_GE = department.PK;
					}
				}
			}

			return jobHeader;
		}

		bool IsCustomsDSBCharge(ZGuid chargePK)
		{
			return customsDSBChargeCodes.Contains(chargePK);
		}

		bool IsCustomsDSBCharge(Charge charge)
		{
			return charge != null && IsCustomsDSBCharge(charge.JR_AC);
		}

		bool IsForCustomsDSBCharge(AutoRateInfo rateInfo)
		{
			return IsCustomsDSBCharge(rateInfo.ChargeCode.PK);
		}

		void CreateOrUpdateCustomsDSBCharges(ICustomsJobInfo parent, IAccInvoiceDataProvider dataProvider, AutoRateInfoCollection customsDSBAutoRateInfos, Job invoicingJob)
		{
			var factory = parent.Factory;
			var creditorPK = parent.CreditorPK;
			var invoiceNumberToUse = dataProvider.GetAPInvoiceNumberToMatch(factory, creditorPK);

			var hasAPPosted = new APInvoice.Loader(factory).LoadTop1NotReversed(dataProvider.UniqueNumber, creditorPK, invoicingJob.PK) != null;
			var hasARPosted = new ARInvoice.Loader(factory).LoadTop1NotReversed(invoicingJob.PK, customsDSBChargeCodeList) != null;

			var shouldAPPostDSB = ShouldAPPostDSB;
			var shouldARPostDSB = ShouldARPostDSB;
			var shouldAutoRateDSB = ShouldAutoRateDSB;
			var shouldUpdate = shouldAPPostDSB || shouldARPostDSB || shouldAutoRateDSB;

			if (shouldUpdate)
			{
				var errorsOnCharge = new List<string>();

				foreach (var rateInfo in customsDSBAutoRateInfos)
				{
					var existingCharges = invoicingJob.Charges.GetMatchingUnapportionedChargesFromThisJobOnly(rateInfo.ChargeCode.PK, dataProvider.UniqueNumber, dataProvider.PreviousUniqueNumber);

					var sum = existingCharges.Sum(x => x.JR_LocalCostAmt);

					var targetAmount = rateInfo.LocalAmount;
					var isForCustomsDSBCharge = IsForCustomsDSBCharge(rateInfo);
					if (!targetAmount.IsEmpty && isForCustomsDSBCharge && !IsCostGSTApplicable(invoicingJob, rateInfo))
					{
						targetAmount += rateInfo.OverriddenGSTAmount;
					}
					ZDecimal diff = targetAmount - sum;

					if (diff.IsEmpty
							&& targetAmount.IsEmpty
							&& isForCustomsDSBCharge
							&& !IsForCustomsDeferredCharge(rateInfo)
							&& (rateInfo.OverriddenGSTAmount.IsEmpty || IsCostGSTApplicable(invoicingJob, rateInfo)))
					{
						continue;
					}

					var hasChargeCodeARPosted = new ARInvoice.Loader(factory).LoadTop1NotReversed(invoicingJob.PK, new[] { rateInfo.ChargeCode.PK }) != null;
					var chargeToUpdateForInvoiceDetails = new List<Charge>();
					var unpostedChargesToUpdate = GetUnpostedChargeToUpdate(existingCharges).ToArray();
					if (unpostedChargesToUpdate.Any())
					{
						if (isForCustomsDSBCharge && (!diff.IsEmpty || IsForCustomsDeferredCharge(rateInfo)) && !unpostedChargesToUpdate.Any(AreCostAndRevenueBothUnposted))
						{
							chargeToUpdateForInvoiceDetails.Add(CreateDifferenceCharge(invoicingJob, rateInfo, existingCharges.Count, invoiceNumberToUse, diff));
						}
						else
						{
							UpdateExistingChargeIfUnposted(invoicingJob, rateInfo, existingCharges, unpostedChargesToUpdate, diff, invoiceNumberToUse);
						}

						chargeToUpdateForInvoiceDetails.AddRange(unpostedChargesToUpdate);
					}
					else if (isForCustomsDSBCharge && !diff.IsEmpty && ((!existingCharges.Any() && !hasChargeCodeARPosted) || (existingCharges.Any() && existingCharges.All(x => x.IsCostPosted && x.IsRevenuePosted))))
					{
						if (!existingCharges.Any() && !hasChargeCodeARPosted)
						{
							invoicingJob.PlugInData = parent.TopLevelObjectForJobToReference;
						}
						var differenceCharge = CreateDifferenceCharge(invoicingJob, rateInfo, existingCharges.Count, invoiceNumberToUse, diff);
						if (!hasAPPosted)
						{
							using (differenceCharge.SuppressAutoRatingOverride())
							{
								differenceCharge.JR_APInvoiceNum = dataProvider.APInvoiceNumberAlwaysIncludeChargeCode ? CreateAPInvoiceNum(invoiceNumberToUse, rateInfo.ChargeCode.AC_Code, 0) : invoiceNumberToUse;
							}
						}
						chargeToUpdateForInvoiceDetails.Add(differenceCharge);
					}
					else if (shouldAutoRateDSB && !hasAPPosted && !hasARPosted)
					{
						invoicingJob.PlugInData = parent.TopLevelObjectForJobToReference;
						chargeToUpdateForInvoiceDetails.Add(CreateNewCharge(invoicingJob, rateInfo));
					}

					foreach (var charge in chargeToUpdateForInvoiceDetails)
					{
						using (ReOpenJobIfChargeHasChanges(charge, invoicingJob))
						using (charge.SuppressAutoRatingOverride())
						{
							if (!charge.IsRevenuePosted)
							{
								charge.PostARWhenInvokedByCustomInvoiceCreator = shouldARPostDSB && HasValidSellAmount(charge);
							}

							if (!charge.IsCostPosted)
							{
								var hasValidCostAmount = HasValidCostAmount(charge);
								charge.PostAPWhenInvokedByCustomInvoiceCreator = hasValidCostAmount && shouldAPPostDSB;
								if (hasValidCostAmount)
								{
									if (charge.JR_APInvoiceNum.IsEmpty)
									{
										charge.JR_APInvoiceNum = dataProvider.APInvoiceNumberAlwaysIncludeChargeCode ? CreateAPInvoiceNum(invoiceNumberToUse, rateInfo.ChargeCode.AC_Code, 0) : invoiceNumberToUse;
									}

									if (shouldAPPostDSB)
									{
										charge.JR_APInvoiceDate = dataProvider.InvoiceDate;
										if (!dataProvider.IsAutoBillingDueDateFromPaymentTerms)
										{
											charge.JR_PaymentDate = dataProvider.APDueDate;
										}
									}
								}
								else
								{
									charge.JR_APInvoiceNum = ZString.Empty;
								}
							}
						}

						ValidateChargeWithCost(charge, errorsOnCharge);
					}
				}

				if (errorsOnCharge.Count > 0)
				{
					throw new CustomsInvoiceRaiseException(errorsOnCharge.ToStringWithNewLineBetweenStrings());
				}
			}
		}

		static IDisposable ReOpenJobIfChargeHasChanges(Charge charge, Job job)
		{
			if (charge != null)
			{
				charge.HasChangesChanged -= Charge_HasChangesChanged;
				charge.HasChangesChanged += Charge_HasChangesChanged;
			}

			return new DisposableAction(() =>
			{
				if (charge != null)
				{
					charge.HasChangesChanged -= Charge_HasChangesChanged;
				}
			});

			void Charge_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
			{
				if (e.ObjectJustWasChanged)
				{
					ReOpenJobIfClosed(job, true);
				}
			}
		}

		static void ReOpenJobIfClosed(Job job, bool hasChanges)
		{
			if (hasChanges && job != null && job.IsClosed)
			{
				job.ReOpen();

				if (job.IsClosed && !job.CanReOpenClosedJobSecurity(JobHeaderStatus.Working.Code))
				{
					throw new CustomsInvoiceRaiseException(Env.Security.ReopenJob.ErrorMessageForNotAllowed);
				}
			}
		}

		bool IsCostGSTApplicable(Job invoicingJob, AutoRateInfo rateInfo)
		{
			var result = GlbCompany.CurrentCompany.GC_IsGSTRegistered;

			if (result)
			{
				var creditor = invoicingJob.Factory.Load<OrgHeader>(rateInfo.ProviderPK);
				var gstRatePk = invoicingJob.GetGSTID(creditor, rateInfo.ChargeCode, invoicingJob.Branch, CostSell.Cost, out _);
				var gstRate = invoicingJob.Factory.Load<AccTaxRate>(gstRatePk);
				result = gstRate != null && !gstRate.GetRate(ZDate.Empty).IsEmpty;
			}

			return result;
		}

		Charge CreateDifferenceCharge(Job invoicingJob, AutoRateInfo rateInfo, ZInt index, ZString invoiceNumberToUse, ZDecimal diff)
		{
			ReOpenJobIfClosed(invoicingJob, true);
			var desc = GetDesc(rateInfo);
			var newCharge = CreateNewCharge(invoicingJob, rateInfo);
			using (newCharge.SuppressAutoRatingOverride())
			{
				newCharge.JR_APInvoiceNum = CreateAPInvoiceNum(invoiceNumberToUse, rateInfo.ChargeCode.AC_Code, index);

				if (newCharge.IsCostForeign)
				{
					newCharge.JR_OSCostAmt = diff;
				}
				else
				{
					newCharge.JR_LocalCostAmt = diff;
				}

				newCharge.JR_EstimatedCost = diff;
				newCharge.JR_EstimatedRevenue = diff;
				newCharge.JR_Desc = desc;
			}

			return newCharge;
		}

		void ValidateChargeWithCost(Charge charge, List<string> errorsOnCharge)
		{
			charge.ValidateJR_ACWhenCreateWIPandAccrual();
			if (charge.HasRowErrors)
			{
				errorsOnCharge.Add(charge.RowErrors.ToUniqueMessageListString());
			}
		}

		static ZString CreateAPInvoiceNum(ZString invoiceNumberToUse, ZString chargeCode, ZInt index)
		{
			var result = ZString.Empty;

			if (!invoiceNumberToUse.IsEmpty)
			{
				const string separator = IAccInvoiceDataProviderExtensionMethods.Separator;

				result = invoiceNumberToUse + separator + chargeCode;
				if (index > 0)
				{
					result += separator + index;
				}
			}

			return result;
		}

		static Charge CreateNewCharge(Job invoicingJob, AutoRateInfo rateInfo)
		{
			ReOpenJobIfClosed(invoicingJob, true);
			var newCharge = invoicingJob.Charges.AddNew();
			using (newCharge.SuppressAutoRatingOverride())
			{
				var chargeCode = rateInfo.ChargeCode;
				if (chargeCode != null)
				{
					newCharge.JR_AC = chargeCode.PK;
				}

				var invoicingSupporter = rateInfo.InvoicingSupporter;
				if (invoicingSupporter != null)
				{
					newCharge.JR_CostReference = invoicingSupporter.OperationalJobRef;
				}

				newCharge.JR_ChargeType = (chargeCode?.AC_ChargeType ?? ZString.Empty) == Constants.ChargeType.Comment ? Constants.ChargeType.Comment : Constants.ChargeType.Disbursement;
				newCharge = invoicingJob.SetAmountsOnCharge(newCharge, rateInfo, CostSell.Cost);
				invoicingJob.JH_RatingHasBeenRun = true;
				return newCharge;
			}
		}

		static bool HasValidCostAmount(Charge charge)
		{
			return charge.JR_LocalCostAmt != ZDecimal.Zero || charge.IsCommentChargeCode;
		}

		static bool HasValidSellAmount(Charge charge)
		{
			return charge.JR_LocalSellAmt > 0m || charge.IsCommentChargeCode;
		}

		IEnumerable<Charge> GetUnpostedChargeToUpdate(IEnumerable<Charge> existingCharges)
		{
			foreach (Charge charge in existingCharges)
			{
				if (ShouldAPPostDSB && !charge.IsCostPosted ||
					ShouldARPostDSB && !charge.IsRevenuePosted ||
					ShouldAutoRateDSB && IsCostUnpostedOrRevenueUnposted(charge))
				{
					yield return charge;
				}
			}
		}

		bool ShouldAutoRateDSB
		{
			get { return (ChargePosterBehaviours & ChargePosterBehaviours.AutoRateDSB) == ChargePosterBehaviours.AutoRateDSB; }
		}

		bool ShouldAPPostDSB
		{
			get { return (ChargePosterBehaviours & ChargePosterBehaviours.APPostDSB) == ChargePosterBehaviours.APPostDSB; }
		}

		bool ShouldARAPPostNonDSB
		{
			get { return (ChargePosterBehaviours & ChargePosterBehaviours.ARAPPostNonDSB) == ChargePosterBehaviours.ARAPPostNonDSB; }
		}

		bool ShouldARPostDSB
		{
			get { return (ChargePosterBehaviours & ChargePosterBehaviours.ARPostDSB) == ChargePosterBehaviours.ARPostDSB; }
		}

		bool ShouldSendEmail
		{
			get { return (ChargePosterBehaviours & ChargePosterBehaviours.SendEmail) == ChargePosterBehaviours.SendEmail; }
		}

		bool ShouldPostNegativeCost
		{
			get { return (ChargePosterBehaviours & ChargePosterBehaviours.PostNegativeCost) == ChargePosterBehaviours.PostNegativeCost; }
		}

		void BalanceDisbursementCharges(Job invoicingJob, ICustomsJobInfo customsJob)
		{
			foreach (ZGuid chargeCodePK in customsDSBChargeCodes)
			{
				BalanceDisbursementChargesCore(invoicingJob, chargeCodePK, customsJob.CreditorPK);
			}
		}

		void BalanceDisbursementChargesCore(Job jobToBalance, ZGuid chargeCodePK, ZGuid creditorPK)
		{
			var localDSBCostTotal = ZDecimal.Zero;
			var localDSBSellTotal = ZDecimal.Zero;

			foreach (Charge dSBCharge in jobToBalance.Charges)
			{
				if (dSBCharge.JR_AC == chargeCodePK && !dSBCharge.IsCostPosted && !dSBCharge.IsRevenuePosted)
				{
					localDSBCostTotal += dSBCharge.JR_LocalCostAmt;
					localDSBSellTotal += dSBCharge.JR_LocalSellAmt;
				}
			}

			if ((localDSBCostTotal - localDSBSellTotal) != 0m)
			{
				ReOpenJobIfClosed(jobToBalance, true);
				var newCharge = jobToBalance.Charges.AddNew();
				using (newCharge.SuppressAutoRatingOverride())
				{
					newCharge.JR_AC = chargeCodePK;
					newCharge.JR_OH_CostAccount = creditorPK;

					if (localDSBSellTotal > localDSBCostTotal)
					{
						newCharge.JR_LocalCostAmt = localDSBSellTotal - localDSBCostTotal;
						using (newCharge.Calculations.SuspendCalculations())
						{
							newCharge.JR_LocalSellAmt = 0m;
							newCharge.JR_OSSellAmt = 0m;
						}
					}
					else if (localDSBCostTotal > localDSBSellTotal)
					{
						newCharge.JR_LocalSellAmt = localDSBCostTotal - localDSBSellTotal;
						using (newCharge.Calculations.SuspendCalculations())
						{
							newCharge.JR_LocalCostAmt = 0m;
							newCharge.JR_OSCostAmt = 0m;
						}
					}
				}
			}
		}

		static ZString GetDesc(AutoRateInfo rateInfo)
		{
			var desc = rateInfo.InvoiceLineDescription;

			if (!rateInfo.AdditionalInvoiceLineDescription.IsEmpty)
			{
				desc += System.Environment.NewLine + Res.GetString("96C6AEC2-7A28-4B62-88B3-87108286CAB9", "Current Amounts");
				desc += System.Environment.NewLine + rateInfo.AdditionalInvoiceLineDescription;
			}

			return desc.SubstringSafe(0, AutoJobCharge.Schema.JR_DescMaxLength);
		}

		bool IsCostUnpostedOrRevenueUnposted(Charge charge)
		{
			return !charge.IsCostPosted || !charge.IsRevenuePosted;
		}

		bool AreCostAndRevenueBothUnposted(Charge charge)
		{
			return !charge.IsCostPosted && !charge.IsRevenuePosted;
		}

		void UpdateExistingChargeIfUnposted(Job invoicingJob, AutoRateInfo rateInfo, List<Charge> existingCharges, Charge[] chargesToUpdate, ZDecimal diff, ZString invoiceNumberToUse)
		{
			var hasOnlyOneChargeToUpdate = chargesToUpdate.Length == 1;

			var lastUnpostedCustomsDSBCharge = chargesToUpdate.LastOrDefault(x => IsCustomsDSBCharge(x) && AreCostAndRevenueBothUnposted(x));
			bool IsLastUnpostedCustomsDSBCharge(Charge charge) => lastUnpostedCustomsDSBCharge != null && lastUnpostedCustomsDSBCharge == charge;

			var index = 0;
			foreach (var chargeToUpdate in chargesToUpdate)
			{
				if (!chargeToUpdate.IsRevenueCharge && !chargeToUpdate.JR_IsApportioned && IsCostUnpostedOrRevenueUnposted(chargeToUpdate))
				{
					using (ReOpenJobIfChargeHasChanges(chargeToUpdate, invoicingJob))
					using (chargeToUpdate.SuppressAutoRatingOverride())
					{
						if (!diff.IsEmpty || IsForCustomsDeferredCharge(rateInfo))
						{
							chargeToUpdate.JR_Desc = GetDesc(rateInfo);
						}

						if (hasOnlyOneChargeToUpdate || (IsLastUnpostedCustomsDSBCharge(chargeToUpdate) && !diff.IsEmpty))
						{
							ZDecimal cost = chargeToUpdate.IsCostForeign
								? chargeToUpdate.JR_OSCostAmt + diff
								: chargeToUpdate.JR_LocalCostAmt + diff;

							UpdateExistingChargeForCostIfNecessary(invoicingJob, rateInfo, chargeToUpdate, cost);

							UpdateExistingChargeForRevenueIfNecessary(invoicingJob, rateInfo, chargeToUpdate, cost);

							if (!chargeToUpdate.IsCostPosted && HasValidCostAmount(chargeToUpdate) && !invoiceNumberToUse.IsEmpty)
							{
								if (hasOnlyOneChargeToUpdate)
								{
									if (!chargeToUpdate.JR_LocalCostAmt.IsEmpty)
									{
										if (existingCharges.Count > 1 && !chargeToUpdate.IsCostPosted && !chargeToUpdate.IsRevenuePosted)
										{
											chargeToUpdate.JR_APInvoiceNum = CreateAPInvoiceNum(invoiceNumberToUse, rateInfo.ChargeCode.AC_Code, existingCharges.IndexOf(chargeToUpdate));
										}
										else
										{
											var invoiceNum = chargeToUpdate.JR_APInvoiceNum;
											if (!invoiceNum.IsEmpty && invoiceNum != invoiceNumberToUse)
											{
												chargeToUpdate.JR_APInvoiceNum = CreateAPInvoiceNum(invoiceNumberToUse, rateInfo.ChargeCode.AC_Code, index);
											}
										}
									}
								}
								else
								{
									chargeToUpdate.JR_APInvoiceNum = CreateAPInvoiceNum(invoiceNumberToUse, rateInfo.ChargeCode.AC_Code, index);
								}
							}
						}
					}
				}

				index++;
			}
		}

		static void UpdateExistingChargeForCostIfNecessary(Job invoicingJob, AutoRateInfo rateInfo, Charge chargeToUpdate, ZDecimal cost)
		{
			using (ReOpenJobIfChargeHasChanges(chargeToUpdate, invoicingJob))
			{
				if (!chargeToUpdate.IsCostPosted)
				{
					chargeToUpdate.JR_OH_CostAccount = rateInfo.ProviderPK;

					var costCurrency = RefCurrency.LoadFromCurrencyCode(chargeToUpdate.Factory, rateInfo.Currency);

					if (costCurrency != null)
					{
						chargeToUpdate.JR_RX_NKCostCurrency = costCurrency.RX_Code;
					}

					if (chargeToUpdate.IsCostForeign)
					{
						chargeToUpdate.JR_OSCostAmt = cost;
					}
					else
					{
						chargeToUpdate.JR_LocalCostAmt = cost;
					}

					chargeToUpdate.JR_EstimatedCost = cost;

					chargeToUpdate.JR_AgentDeclaredCostAmt = !rateInfo.AgentAmount.IsEmpty ? rateInfo.AgentAmount : rateInfo.Amount;

					chargeToUpdate.JR_CostRated = !rateInfo.Amount.IsEmpty;

					chargeToUpdate.SetCostCalculationDescription(rateInfo);
				}
			}
		}

		static void UpdateExistingChargeForRevenueIfNecessary(Job invoicingJob, AutoRateInfo rateInfo, Charge chargeToUpdate, ZDecimal cost)
		{
			using (ReOpenJobIfChargeHasChanges(chargeToUpdate, invoicingJob))
			{
				if (!chargeToUpdate.IsRevenuePosted)
				{
					if (rateInfo.DebtorOverridePK.IsValid && chargeToUpdate.JR_OH_SellAccount != rateInfo.DebtorOverridePK)
					{
						chargeToUpdate.JR_OH_SellAccount = rateInfo.DebtorOverridePK;
					}

					var revenueCurrency = RefCurrency.LoadFromCurrencyCode(chargeToUpdate.Factory, rateInfo.Currency);

					if (revenueCurrency != null && chargeToUpdate.JR_RX_NKSellCurrency != revenueCurrency.RX_Code)
					{
						chargeToUpdate.JR_RX_NKSellCurrency = revenueCurrency.RX_Code;
					}

					if (chargeToUpdate.IsCostForeign && chargeToUpdate.JR_OSSellAmt != cost)
					{
						chargeToUpdate.JR_OSSellAmt = cost;
					}
					else if (chargeToUpdate.JR_LocalSellAmt != cost)
					{
						chargeToUpdate.JR_LocalSellAmt = cost;
					}

					chargeToUpdate.JR_EstimatedRevenue = cost;

					chargeToUpdate.JR_AgentDeclaredSellAmt = !rateInfo.AgentAmount.IsEmpty ? rateInfo.AgentAmount : rateInfo.Amount;

					chargeToUpdate.JR_SellRated = !rateInfo.Amount.IsEmpty;

					chargeToUpdate.SetRevenueCalculationDescription(rateInfo);
				}
			}
		}

		Job GetOrCreateJob(ICustomsJobInfo customsJob)
		{
			return new Job.Loader(customsJob.TopLevelObjectForJobToReference).TryLoadOrCreateWithMutex();
		}

		Job GetExistingJob(ICustomsJobInfo customsJob)
		{
			return new Job.Loader(customsJob.TopLevelObjectForJobToReference).Load();
		}

		#endregion

		#region Customs Rating Related

		void AggregateSellAndCostAmountInto(AutoRateInfoCollection result, AutoRateInfoCollection theOther)
		{
			foreach (AutoRateInfo passedRateInfo in theOther)
			{
				bool hasBeenMerged = false;

				foreach (AutoRateInfo resultInfo in result)
				{
					if (resultInfo.CanBeMergedWith(passedRateInfo))
					{
						resultInfo.Bases.AddRange(passedRateInfo.Bases);
						hasBeenMerged = true;
						break;
					}
				}

				if (!hasBeenMerged)
				{
					result.Add(passedRateInfo);
				}
			}
		}

		#endregion

		#region Email Sending

		AutoBillingResult ProcessResultFromValidationResults(Dictionary<ZGuid, List<CUSDSBPostingValidationResult>> validationResults, BusinessObjectFactory factory, ZString jobNumber, ZGuid jobPK, ControllerID controllerID, string jobType, bool hasChanges)
		{
			bool isFailure = false;

			Dictionary<ZGuid, List<CUSDSBPostingValidationResult>> emailResult = new Dictionary<ZGuid, List<CUSDSBPostingValidationResult>>(ShouldSendEmail ? validationResults.Count : 0);
			ZStringBuilder allMessages = new ZStringBuilder();

			foreach (List<CUSDSBPostingValidationResult> list in validationResults.Values)
			{
				foreach (CUSDSBPostingValidationResult validationResult in list)
				{
					if (validationResult.HasErrors)
					{
						isFailure = true;
					}

					if (validationResult.HasMessages)
					{
						if (ShouldSendEmail)
						{
							SetEmailNotification(emailResult, validationResult);
						}

						allMessages.Append(validationResult.MessageIncludingDiscrepancyDetails + "\r\n");
					}
				}
			}

			if (ShouldSendEmail)
			{
				AutoCustomInvoiceFailureEmail emailSender = new AutoCustomInvoiceFailureEmail();

				foreach (ZGuid recipient in emailResult.Keys)
				{
					emailSender.Send(isFailure, recipient, factory, emailResult[recipient], jobNumber, jobPK, controllerID, jobType);
				}
			}

			AutoBillingResult result = new AutoBillingResult();
			result.WasSuccessful = !isFailure;
			result.Message = allMessages.ToStringWithNewLineBetweenAppends();
			result.HasChanges = hasChanges;
			return result;
		}

		void SetEmailNotification(Dictionary<ZGuid, List<CUSDSBPostingValidationResult>> emailBodies, CUSDSBPostingValidationResult validationResult)
		{
			ZGuid[] recipients = validationResult.AutoPostingNotification?.EmailRecipients ?? Array.Empty<ZGuid>();

			if (recipients != null)
			{
				foreach (ZGuid recipient in recipients)
				{
					if (!emailBodies.ContainsKey(recipient))
					{
						emailBodies[recipient] = new List<CUSDSBPostingValidationResult>();
					}

					emailBodies[recipient].Add(validationResult);
				}
			}
		}

		#endregion

		#region Parent Department

		GlbDepartment GetParentDepartment(ICustomsJobInfo parent)
		{
			ZGuid departmentPK = parent.TopLevelObjectForJobToReference.InvoicingSupporter.OverriddenDepartmentPK;

			if (!departmentPK.IsValid)
			{
				bool isImport = parent.InvoicingSupporter.IsImport;

				IJobInvoicingPlugIn departmentDataProvider = parent.TopLevelObjectForJobToReference;

				departmentPK = DepartmentChooser.New(parent.Factory).GetDepartment(departmentDataProvider, !isImport, !isImport, DepartmentChooser.Anything);
			}

			return parent.Factory.Load<GlbDepartment>(departmentPK);
		}

		#endregion
	}
}
