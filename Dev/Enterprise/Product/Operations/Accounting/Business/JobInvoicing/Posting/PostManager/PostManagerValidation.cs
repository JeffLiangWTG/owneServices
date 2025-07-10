using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	/// <summary>
	/// Validation of jobs before proceeding with Posting
	/// </summary>
	public partial class PostManagerValidation
	{
		public PostManagerValidation(IEnumerable<Job> jobs, JobInvoicingPostingOption postingOption, IEnumerable<Job> originalJobs, bool isBulkPosting = false)
		{
			this.OriginalJobs = originalJobs;
			this.Jobs = jobs;
			this.PostingOption = postingOption;
			this.IsBulkPosting = isBulkPosting;
		}

		/// <summary>
		/// Validates Job and Parent to ensure they are saved
		/// </summary>
		/// <returns>Error message or blank string if everything is OK</returns>
		public string ValidateJobAndParentSavedOnly()
		{
			return RunJobsAreSavedValidation();
		}

		/// <summary>
		/// Validates the data before posting
		/// </summary>
		/// <returns>Error message or blank string if everything is OK</returns>
		public PostManagerNotification Validate()
		{
			// Implemented so that subsequent validation wouldn't run if the one before failed to make sure that no unnecessary checks for nulls are done
			return ValidateErrorNotificationCore() ?? ValidateWarningNotificationCore();
		}

		protected virtual PostManagerNotification ValidateErrorNotificationCore()
		{
			return RunValidateNotification(() => GetErrorValidations());
		}

		protected IEnumerable<Func<PostManagerNotification>> GetErrorValidations()
		{
			if (!IsBulkPosting)
			{
				yield return CreateErrorValidator(ValidateJobAndParentSavedOnly, PostManagerValidationType.ValidateJobAndParentSavedOnly);
			}
			yield return CreateErrorValidator(RunDifferencesInReloadedChargesValidation, PostManagerValidationType.DifferencesInReloadedChargesValidation);
			yield return CreateErrorValidator(RunPeriodValidation, PostManagerValidationType.PeriodValidation);
			yield return CreateErrorValidator(RunJobNotFoundValidation, PostManagerValidationType.JobNotFoundValidation);
			yield return CreateErrorValidator(RunJobsContainErrorsValidation, PostManagerValidationType.JobsContainErrorsValidation); //This should be the first job related validation in here.
			yield return CreateErrorValidator(RunChargeCodeValidation, PostManagerValidationType.ChargeCodeValidation);
			yield return CreateErrorValidator(RunDebtorValidation, PostManagerValidationType.DebtorValidation);
			yield return CreateErrorValidator(RunCreditorValidation, PostManagerValidationType.CreditorValidation);
			yield return CreateErrorValidator(RunInvoiceTypeValidation, PostManagerValidationType.InvoiceTypeValidation);
			yield return CreateErrorValidator(RunNoChargesValidation, PostManagerValidationType.NoChargesValidation);
			yield return CreateErrorValidator(RunBranchDepartmentCombinationsValidation, PostManagerValidationType.BranchDepartmentCombinationsValidation);
			yield return CreateErrorValidator(RunGSTApplicabilityValidation, PostManagerValidationType.GSTApplicabilityValidation);
			yield return CreateErrorValidator(RunRevenueRecognitionDateValidation, PostManagerValidationType.RevenueRecognitionDateValidation);
			yield return CreateErrorValidator(RunChargesCanBePostedValidation, PostManagerValidationType.ChargesCanBePostedValidation);
			yield return CreateErrorValidator(RunChargesHaveCFXAccountValidation, PostManagerValidationType.ChargesHaveCFXAccountValidation);
			yield return CreateErrorValidator(RunChequeBookValidation, PostManagerValidationType.ChequeBookValidation);
			if (PostingOption == JobInvoicingPostingOption.Costs || PostingOption == JobInvoicingPostingOption.All)
			{
				yield return CreateErrorValidator(RunPaymentTypeSecurityValidation, PostManagerValidationType.PaymentTypeSecurityValidation);
				yield return CreateErrorValidator(RunConsolCostRelativeValidation, PostManagerValidationType.ConsolCostRelativeValidation);
			}
			if (PostingOption != JobInvoicingPostingOption.Costs)
			{
				yield return CreateErrorValidator(RunRoundingRegistryItemValidation, PostManagerValidationType.RoundingRegistryItemValidation);
			}
			yield return CreateErrorValidator(RunStampDutyValidation, PostManagerValidationType.StampDutyValidation);
			yield return CreateErrorValidator(RunExchangeRateValidation, PostManagerValidationType.ExchangeRateValidation);
			yield return CreateErrorValidator(RunJobChargeSupplyTypeValidation, PostManagerValidationType.JobChargeSupplyTypeValidation);
			yield return CreateErrorValidator(RunConsolCostTaxBranchValidation, PostManagerValidationType.ConsolCostTaxBranchValidation);
			yield return CreateErrorValidator(RunJobChargeSellTaxBranchValidation, PostManagerValidationType.JobChargeTaxBranchValidation);
			if (PostingOption == JobInvoicingPostingOption.Revenue || PostingOption == JobInvoicingPostingOption.Agent || PostingOption == JobInvoicingPostingOption.All)
			{
				yield return CreateErrorValidator(() => RunOperationalTaxDateErrorValidation(true), PostManagerValidationType.TaxDateValidationForAR);
			}
			if (PostingOption == JobInvoicingPostingOption.Costs || PostingOption == JobInvoicingPostingOption.All)
			{
				yield return CreateErrorValidator(() => RunOperationalTaxDateErrorValidation(false), PostManagerValidationType.TaxDateValidationForAP);
			}
		}

		protected virtual PostManagerNotification ValidateWarningNotificationCore()
		{
			return RunValidateNotification(() => GetWarningValidations());
		}

		protected IEnumerable<Func<PostManagerNotification>> GetWarningValidations()
		{
			if (IsRevenueBeingPosted(PostingOption))
			{
				yield return CreateWarningValidator(RunCreditLimitValidation, PostManagerValidationType.CreditLimitValidation);
			}
			yield return CreateWarningValidator(RunCustomsDisbursementChargesDuplicateCheckValidation, PostManagerValidationType.CustomsDisbursementChargesDuplicateCheckValidation);
			yield return CreateWarningValidator(RunCompanyAndOrgsRegistrationNumbersValidation, PostManagerValidationType.MissingTaxRegistrationNumberValidation);
			if (PostingOption == JobInvoicingPostingOption.Revenue || PostingOption == JobInvoicingPostingOption.Agent || PostingOption == JobInvoicingPostingOption.All)
			{
				yield return CreateWarningValidator(() => RunOperationalTaxDateWarningValidation(true), PostManagerValidationType.TaxDateValidationForAR);
			}
			if (PostingOption == JobInvoicingPostingOption.Costs || PostingOption == JobInvoicingPostingOption.ConsolCosts || PostingOption == JobInvoicingPostingOption.All)
			{
				yield return CreateWarningValidator(() => RunOperationalTaxDateWarningValidation(false), PostManagerValidationType.TaxDateValidationForAP);
			}
		}

		#region Implementation

		PostManagerNotification RunValidateNotification(Func<IEnumerable<Func<PostManagerNotification>>> getValidations)
		{
			PostManagerNotification result = null;
			foreach (var action in getValidations())
			{
				if (action != null && (result = action()) != null)
				{
					return result;
				}
			}
			return null;
		}

		protected bool IsRevenueBeingPosted(JobInvoicingPostingOption postingOption)
		{
			return postingOption != JobInvoicingPostingOption.Costs && postingOption != JobInvoicingPostingOption.CustomsDSBChargeAPOnly;
		}

		protected PostManagerNotification GetErrorNotification(string errorMessage, PostManagerValidationType errorType)
		{
			return new PostManagerNotification(CargoWise.EntityFramework.NotificationType.Error, errorMessage, errorType);
		}

		PostManagerNotification GetWarningNotification(string warningMessage, PostManagerValidationType errorType)
		{
			return new PostManagerNotification(CargoWise.EntityFramework.NotificationType.Warning, warningMessage, errorType);
		}

		protected virtual string RunDifferencesInReloadedChargesValidation()
		{
			string result = "";

			if (OriginalJobs != null && OriginalJobs.Any() && !ReferenceEquals(OriginalJobs, Jobs))
			{
				foreach (Job job in Jobs)
				{
					var originalJob = OriginalJobs.FirstOrDefault(j => j.PK == job.PK);
					if (originalJob != null)
					{
						var originalJobChargesPKs = GetCharges(originalJob).Where(t => t.IsInDatabase).Select(t => t.PK).OrderBy(x => x).ToList();
						var jobChargesPKs = GetCharges(job).Select(t => t.PK).OrderBy(x => x).ToList();

						if (!Enumerable.SequenceEqual(originalJobChargesPKs, jobChargesPKs))
						{
							return Res.GetString("bf9501ea-4215-4843-84fb-418deffb7a4d", @"This job cannot be posted because changes have been made by another user since you have saved. These changes have been reloaded.");
						}
					}
				}
			}

			return result;
		}

		protected virtual string RunCustomsDisbursementChargesDuplicateCheckValidation()
		{
			string result = "";

			foreach (Job job in Jobs)
			{
				var customsChargeCodePKs = Enterprise.Registry.Business.Customs.EntryChargeTypeList.GetAllChargeCodePKsOf(job.Branch.GB_GC, job.Branch.Company.GC_RN_NKCountryCode);

				var duplicateDisbursementChargeGroup = (
					from chg in GetCharges(job)
					where customsChargeCodePKs.Contains(chg.JR_AC)
					group chg by new { chg.JR_AC, chg.JR_Calc_RelatedJobNumber } into grp
					where grp.Count() > 1
					select grp
				).FirstOrDefault();

				if (duplicateDisbursementChargeGroup != null)
				{
					var chargeGroup = duplicateDisbursementChargeGroup.First();
					if (job.IsGatewayBillingJob())
					{
						var relatedJob = chargeGroup.JR_Calc_RelatedJobNumber.IsEmpty ? Res.GetString("67f9a8b8-a2df-400e-8567-60b30b0c76e9", "<empty>") : (string)chargeGroup.JR_Calc_RelatedJobNumber;
						result = Res.GetString("9c295d11-6225-469a-8e0e-105c66e42ad6", "There are multiple customs disbursement charges with the same charge code for the same Related Job Number: {0}, {1}",
							chargeGroup.ChargeCode.AC_Code,
							relatedJob);
					}
					else
					{
						result = Res.GetString("03f423cb-8c74-4c81-bffc-b438478690da", @"There are multiple customs disbursement charges with the same charge code: {0}",
						chargeGroup.ChargeCode.AC_Code);
					}
				}
			}

			return result;
		}

		protected virtual string RunRoundingRegistryItemValidation()
		{
			string result = "";
			if (GlbCompany.CurrentCompany.Country.Code == Enterprise.Core.Constants.CountryCodes.Japan &&
				AccountingConfigurationRegistry.Instance.JapanIATAImportAirLocalClientFRTChargeGroupRounding.Value == Constants.RoundingRules.Codes.JapanYenWithCharge &&
				AccountingConfigurationRegistry.Instance.RoundingChargeCode.Value == Guid.Empty)
			{
				result = Res.GetString("6C33850F-D922-4BC0-A094-C735E02E0436", "The registry 'Japan IATA Import Air Local Client FRT Charge Group Rounding' is configured to add a charge line when posting charges on this shipment.\r\nThe following registry must be configured with an appropriate charge code before posting:\r\n'Accounting > Job Invoicing > Rounding > Rounding Charge Code'");
			}
			return result;
		}

		protected string RunChargesCanBePostedValidation()
		{
			string result = "";
			int totalChargesToPost = 0;

			var errors = new List<string>();
			var chargeValidationErrors = new List<string>();
			var enableValidationForCost = AccountingConfigurationRegistry.Instance.EnableValidationForChargeWhenPostTransactions.Value;

			foreach (Job jobToPost in Jobs)
			{
				foreach (Charge charge in GetCharges(jobToPost))
				{
					if (jobToPost.PlugInData != null)
					{
						var postInfoRev = !charge.IsRevenuePosted && IsSellEligibleToPost(charge) ? jobToPost.PlugInData.InvoicingSupporter.ConsumerType.ShouldPostCharges(jobToPost.PlugInData, charge.JR_InvoiceType, false) : null;
						var postInfoCost = !charge.IsCostPosted && IsCostEligibleToPost(charge) ? jobToPost.PlugInData.InvoicingSupporter.ConsumerType.ShouldPostCharges(jobToPost.PlugInData, charge.JR_InvoiceType, true) : null;
						bool shouldAddTotalChargesToPost = false;

						if (postInfoRev != null && postInfoRev.PostAllowed)
						{
							shouldAddTotalChargesToPost = true;
						}

						if (postInfoCost != null && postInfoCost.PostAllowed)
						{
							shouldAddTotalChargesToPost = true;

							if (enableValidationForCost)
							{
								chargeValidationErrors.AddRange(charge.Validation.ValidateCostPropertyInfos());
							}
						}

						if (shouldAddTotalChargesToPost)
						{
							totalChargesToPost++;
						}
						else
						{
							if (postInfoRev != null && !errors.Contains(postInfoRev.ReasonForDisallowing))
							{
								errors.Add(postInfoRev.ReasonForDisallowing);
							}
							if (postInfoCost != null && !errors.Contains(postInfoCost.ReasonForDisallowing))
							{
								errors.Add(postInfoCost.ReasonForDisallowing);
							}
						}
					}
				}
			}

			if (totalChargesToPost == 0 && errors.Count > 0 || chargeValidationErrors.Count > 0)
			{
				char bullet = (char)8226;
				errors.AddRange(chargeValidationErrors);
				string errorString = System.Environment.NewLine + bullet + string.Join(System.Environment.NewLine + bullet, errors.Distinct().ToArray());
				result = Res.GetString("34e4220f-4c27-47cc-823f-47d7a3405bda", "You cannot currently post charges on this job due to the following reason(s):{0}", errorString);
			}

			return result;
		}

		protected string RunChargesHaveCFXAccountValidation()
		{
			string result = "";
			if (AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				List<ZString> departmentList = new List<ZString>();
				foreach (Job jobToPost in Jobs)
				{
					foreach (Charge charge in GetCharges(jobToPost))
					{
						if (!charge.IsRevenuePosted && IsSellEligibleToPost(charge) && charge.JR_CFXAmt > 0)
						{
							Guid cfxAccount = AccountingConfigurationRegistry.Instance.CFXAccount.GetFallBackValueAtAllLevels(charge.Branch.Company.PK.ToGuid(), Guid.Empty, charge.Department.PK.ToGuid());
							if (cfxAccount == Guid.Empty)
							{
								if (!departmentList.Contains(charge.Department.GE_Code))
								{
									departmentList.Add(charge.Department.GE_Code);
								}
							}
						}
					}
				}
				if (departmentList.Count > 0)
				{
					string departments = "";
					foreach (ZString dept in departmentList)
					{
						departments += " - " + dept + "\n";
					}
					result = Res.GetString("21f861d4-158a-492c-9340-13e0cf72be42", "Posting cannot occur on job.  There are charge(s) with department(s) that do not have a CFX Account set in the registry.\r\n\r\nThe following departments do not have a CFX Account set:\r\n{0}\r\nEither set the system or department level CFX Account in the registry under Accounting -> General Ledger Defaults -> Link Account -> CFX Account.", departments);
				}
			}
			return result;
		}

		protected readonly JobInvoicingPostingOption PostingOption;
		protected readonly bool IsBulkPosting;
		protected IEnumerable<Job> Jobs;
		protected IEnumerable<Job> OriginalJobs;

		protected virtual IEnumerable<Charge> GetCharges(Job job)
		{
			return job.Charges.Cast<Charge>();
		}

		protected string RunPeriodValidation()
		{
			AccountingPeriodCalculator acctPeriodCalc = new AccountingPeriodCalculator(JobsFactoryOrNew);
			if (acctPeriodCalc.GetPeriodFromDate(ZDateTime.Today) == AccountingPeriodCalculator.InvalidPeriod)
			{
				return AccountingPeriodCalculator.GetInvalidPeriodValidationError(ZDateTime.Today);
			}
			else
			{
				return "";
			}
		}

		BusinessObjectFactory JobsFactoryOrNew
		{
			get { return Jobs.Any() ? Jobs.First().Factory : (jobsFactoryOrNew ?? (jobsFactoryOrNew = new BusinessObjectFactory())); }
		}
		BusinessObjectFactory jobsFactoryOrNew;

		protected virtual string RunJobNotFoundValidation()
		{
			return Jobs == null || !Jobs.Any() ? Res.GetString("d04b21b8-48e4-4256-80d4-550f3899f5a6", "You cannot post because no Job Invoices have been created.") : "";
		}

		protected virtual bool ShouldAllowLoadingChargesDuringJobValidation => false;

		protected virtual string RunJobsContainErrorsValidation()
		{
			foreach (var jobToPost in GetJobsToValidate(Jobs))
			{
				using (ShouldAllowLoadingChargesDuringJobValidation ? null : jobToPost.ChargesLoadSuspender.GetSuspender())
				{
					//JobToPost.RunPreSaveValidation was replaced here as it validates all editable children but only when we have them. 
					//We register editable child in getters of related properties. So, if those getters are not called before this validation we will not validate them.
					//It means that RunPreSaveValidation can't always guaranty that all possible children will be validated. 
					//Furthermore, in most cases they are not validated here as we do this validation in new factory and we don't call most of the production logic (getters) before.
					//To make this validation more predictable we decided to validate only job and exclude validation of exchange rates as we think that they were not validated before this change in most of the cases.
					//If we someone knows a case when job.ExchangeRates validation is required here, then this validation should be added to this class as separate method to validate it explicitly and not accidentally as it was before with JobToPost.RunPreSaveValidation call.
					jobToPost.Validation.ValidateAll();
				}

				using (jobToPost.ChargesLoadSuspender.GetSuspender())
				{
					//We need chargesToSuspendValidation and all logic around it only until proper validation based on posting option is implemented in WI00137528
					var chargesToSuspendValidation = new HashSet<Charge>();
					if (!AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value)
					{
						chargesToSuspendValidation.UnionWith(jobToPost.Charges.Where(x => !(((ILightValidationInternals)x).IsValid)));
						foreach (var charge in chargesToSuspendValidation)
						{
							foreach (ZPropertyInfo info in charge.ZPropertyInfoHash)
							{
								if (info.HasErrors() || info.HasMessageErrors())
								{
									((IBusinessObjectInternals)charge).Validate(info);
								}
							}
							charge.SuspendValidation();
						}

						jobToPost.Charges.RunPreSaveValidation();

						foreach (var charge in chargesToSuspendValidation)
						{
							charge.ResumeValidation();
						}
					}
				}
				if (jobToPost.HasErrors)
				{
					ZStringBuilder result = new ZStringBuilder(Res.GetString("2218e63d-ead3-475f-83bd-26b8ff2327ee", "You cannot post because job {0} has errors. Please fix errors before posting.", jobToPost.JH_JobNum));
					ZNotificationCollector notificationsCollector = new ZNotificationCollector(jobToPost, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);

					var errorMessages = notificationsCollector.GetErrors().Select(n => n.Message).Distinct();

					foreach (var error in errorMessages)
					{
						result.Append(error);
					}

					return result.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine + " - ");
				}
			}

			return "";
		}

		protected virtual IEnumerable<Job> GetJobsToValidate(IEnumerable<Job> jobs)
		{
			return jobs;
		}

		protected string RunGSTApplicabilityValidation()
		{
			var result = ZString.Empty;

			foreach (var jobToPost in Jobs)
			{
				if (!jobToPost.HasErrors)
				{
					foreach (var charge in GetCharges(jobToPost))
					{
						var taxBranchValidationMessage = AccountingConstants.TaxBranchConflictErrorMessage;
						var msgAfterValidationMessage = Res.GetString("E7F3766B-0F23-48D5-B241-1CEB6CFDCE1A", "To resolve this, go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.");

						if (!charge.IsCostPosted && IsCostEligibleToPost(charge))
						{
							if (!charge.IsCostGSTRateActual)
							{
								result = Res.GetString("27934955-5083-46ff-b9f0-3bb4e2eda4bf", "Tax IDs on unposted charges in the billing tab conflict with the creditor \"Tax is Applicable\" flag.") + "\r\n\r\n";
							}
							if (!charge.IsCostTaxBranchActual)
							{
								result += taxBranchValidationMessage + "\r\n\r\n";
							}
							if (!result.IsEmpty)
							{
								result = Res.GetString("FB0CB7FC-FA5D-4C4F-A1B2-F632522ABCF3", "Tax Data Validation error.") + "\r\n\r\n" + result + msgAfterValidationMessage;
								return result;
							}
						}
						if (!charge.IsRevenuePosted && IsSellEligibleToPost(charge))
						{
							if (!charge.IsSellGSTRateActual)
							{
								result = Res.GetString("198cd00f-0629-40ec-8b3d-457119e0251f", "Tax IDs on unposted charges in the billing tab conflict with the debtor \"Tax is Applicable\" flag.") + "\r\n\r\n";
							}
							if (!charge.IsSellTaxBranchActual)
							{
								result += taxBranchValidationMessage + "\r\n\r\n";
							}
							if (!result.IsEmpty)
							{
								result = Res.GetString("406BC7FF-57B9-44C7-8129-153A067CC1D9", "Job {0} has error: Tax Data Validation error.", jobToPost.JH_JobNum) + "\r\n\r\n" + result + msgAfterValidationMessage;
								return result;
							}
						}
					}
				}
			}
			return result;
		}

		protected string RunChargeCodeValidation()
		{
			if (Jobs.Any())
			{
				var inactiveChargeCodes = new List<ZString>();

				foreach (var partialPKs in AccountingUtils.ChunksOf(Jobs.Select(c => c.PK.ToString()), 500))
				{
					var query = @"select distinct AC_Code
from dbo.JobCharge
join dbo.AccChargeCode on JR_AC = AC_PK
left join dbo.AccTransactionLines ARLines on JR_AL_ARLine = ARLines.AL_PK
left join dbo.AccTransactionLines APLines on JR_AL_APLine = APLines.AL_PK
where JR_JH in (@JobPKs)
and AC_GC = @CurrentCompanyPK and AC_IsActive = 0
and ((JR_LocalSellAmt <> 0 and (JR_AL_ARLine is null or ARLines.AL_LineType = @WIP)) or 
(JR_LocalCostAmt <> 0 and (JR_AL_APLine is null or APLines.AL_LineType = @ACR)))";

					var parameters = new ZSqlParameter[] {
						ZSqlParameter.New("@JobPKs", partialPKs, JobChargeSchema.JR_JH, true),
						ZSqlParameter.New("@CurrentCompanyPK", GlbCompany.CurrentCompany.PK, GlbCompanySchema.PK),
						ZSqlParameter.New("@WIP", TransactionLineTypes.WIP, AccTransactionLinesSchema.AL_LineType),
						ZSqlParameter.New("@ACR", TransactionLineTypes.Accrual, AccTransactionLinesSchema.AL_LineType) };

					var businessObjCollection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
					businessObjCollection.Load(query, parameters);

					if (businessObjCollection.Count > 0)
					{
						inactiveChargeCodes.AddRange(from DynamicBusinessObject chargeCode in businessObjCollection select (ZString)chargeCode[AccChargeCodeSchema.Constants.AC_Code]);
					}
				}

				if (inactiveChargeCodes.Count > 0)
				{
					if (Globals.IsUserInteractive && AccountingConfigurationRegistry.Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.Value)
					{
						inactiveChargeCodes.RemoveAll(x => !Jobs.SelectMany(GetCharges).Where(IsAllowedToPostSellCharge).Select(y => y.ChargeCode?.AC_Code).Contains(x));
					}

					return inactiveChargeCodes.Count > 0 ? Res.GetString("6430e4fa-2b11-4b56-8db1-7844b8dc2d03", "Invalid Charge Code(s): {0}.", string.Join(", ", inactiveChargeCodes)) : "";
				}
			}

			return "";
		}

		ZString taxDateExceedDateRangeInPastWarningMessageForAR;
		ZString taxDateExceedDateRangeInFutureWarningMessageForAR;
		ZString taxDateExceedDateRangeInPastWarningMessageForAP;
		ZString taxDateExceedDateRangeInFutureWarningMessageForAP;

		protected string RunOperationalTaxDateErrorValidation(bool isPostingAR)
		{
			var ledger = isPostingAR ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
			if (isPostingAR)
			{
				taxDateExceedDateRangeInPastWarningMessageForAR = ZString.Empty;
				taxDateExceedDateRangeInFutureWarningMessageForAR = ZString.Empty;
			}
			else
			{
				taxDateExceedDateRangeInPastWarningMessageForAP = ZString.Empty;
				taxDateExceedDateRangeInFutureWarningMessageForAP = ZString.Empty;
			}
			foreach (var jobToPost in Jobs)
			{
				var isOperationalDateMissing = false;
				var operationalDate = ZDate.Empty;
				var taxDateDescription = ZString.Empty;
				if (!jobToPost.HasErrors)
				{
					var taxDateOption = ((IPostingJob)jobToPost).GetTaxDateDefaultingOptionForJob(jobToPost.Factory, ledger);
					if (taxDateOption != null
						&& taxDateOption.TaxDateOption != TaxDateDefaultingOption.Code.InvoiceDate
						&& taxDateOption.TaxDateOption != TaxDateDefaultingOption.Code.Today)
					{
						var plugIn = jobToPost.GetInvoicingSupporter();
						if (plugIn != null)
						{
							var jobCharges = GetCharges(jobToPost);
							var chargesToCheck = isPostingAR ?
														jobCharges.Where(charge =>
														!charge.IsRevenuePosted
														&& IsSellEligibleToPost(charge)
														&& charge.JR_AT_SellGSTRate.IsValid
														&& charge.JR_SellTaxDate.IsEmpty)
													:
														jobCharges.Where(charge =>
														!charge.IsCostPosted
														&& IsCostEligibleToPost(charge)
														&& charge.JR_AT_CostGSTRate.IsValid
														&& charge.JR_CostTaxDate.IsEmpty
														&& !charge.JR_IsApportioned);
							if (chargesToCheck.Any())
							{
								if (taxDateOption != null)
								{
									(operationalDate, taxDateDescription) = ((IPostingJob)jobToPost).GetTaxDateBasedOnRegistryDefaultingOption(plugIn, taxDateOption.TaxDateOption, ZDate.Empty);
								}
								isOperationalDateMissing = operationalDate.IsEmpty;
								if (isOperationalDateMissing)
								{
									return Res.GetString("1397624E-41A2-41D2-A8A3-C411A84DEB50", "Posting is prevented. The following information has not been recorded for this job. It is required in order to set the Tax Date and must be recorded before posting can occur: '{0}'.", taxDateDescription);
								}
								else
								{
									var today = ZDateTime.Now;
									var limits = new TypeValidationLimits();
									if (operationalDate > today.AddYears(limits.FutureYearsBeforeError))
									{
										return Res.GetString("AB1DD869-1516-4714-8094-8157E8553628", "Tax Date is not valid. Tax Date is more than {0} years from now, based on '{1}'.", limits.FutureYearsBeforeError, taxDateDescription);
									}
									else if (operationalDate < today.AddYears(-limits.PastYearsBeforeError))
									{
										return Res.GetString("E713A4E7-51DF-4488-9601-B9A0F67BAE09", "Tax Date is not valid. Tax Date is more than {0} years old, based on '{1}'.", limits.PastYearsBeforeError, taxDateDescription);
									}
									else
									{
										// check if tax rates exist on the operation date for every possible tax type
										var taxRates = isPostingAR ? chargesToCheck.Select(c => c.SellGSTRate).Distinct() : chargesToCheck.Select(c => c.CostGSTRate).Distinct();
										if (taxRates.Any(x => x != null && !x.DoesRateExists(operationalDate)))
										{
											return Res.GetString("6D35F943-61B8-4807-AA52-3E2D72119825", "Tax Date is not valid. No rate is found for the selected date, based on '{0}'.", taxDateDescription);
										}

										var taxDateInFutureWarningMessage = Res.GetString("A391CE88-8F80-4D63-8819-FC0C2246D934", "Tax Date is more than {0} year from now, based on '{1}'. Do you wish to continue?", limits.FutureYearsBeforeWarning, taxDateDescription);
										var taxDateInPastWarningMessage = Res.GetString("1F27E18B-9AA9-437D-8527-31B3FD47B790", "Tax Date is more than {0} year old, based on '{1}'. Do you wish to continue?", limits.PastYearsBeforeWarning, taxDateDescription);
										if (isPostingAR)
										{
											SetTaxDateWarningMessage(ref taxDateExceedDateRangeInFutureWarningMessageForAR, ref taxDateExceedDateRangeInPastWarningMessageForAR);
										}
										else
										{
											SetTaxDateWarningMessage(ref taxDateExceedDateRangeInFutureWarningMessageForAP, ref taxDateExceedDateRangeInPastWarningMessageForAP);
										}

										void SetTaxDateWarningMessage(ref ZString futureWarningMessage, ref ZString pastWarningMessage)
										{
											if (futureWarningMessage.IsEmpty && operationalDate > today.AddYears(limits.FutureYearsBeforeWarning))
											{
												futureWarningMessage = taxDateInFutureWarningMessage;
											}
											else if (pastWarningMessage.IsEmpty && operationalDate < today.AddYears(-limits.PastYearsBeforeWarning))
											{
												pastWarningMessage = taxDateInPastWarningMessage;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return "";
		}

		protected string RunOperationalTaxDateWarningValidation(bool isPostingAR)
		{
			if (isPostingAR)
			{
				return !taxDateExceedDateRangeInFutureWarningMessageForAR.IsEmpty ? taxDateExceedDateRangeInFutureWarningMessageForAR :
						!taxDateExceedDateRangeInPastWarningMessageForAR.IsEmpty ? taxDateExceedDateRangeInPastWarningMessageForAR : ZString.Empty;
			}
			else
			{
				return !taxDateExceedDateRangeInFutureWarningMessageForAP.IsEmpty ? taxDateExceedDateRangeInFutureWarningMessageForAP :
						!taxDateExceedDateRangeInPastWarningMessageForAP.IsEmpty ? taxDateExceedDateRangeInPastWarningMessageForAP : ZString.Empty;
			}
		}

		protected string RunDebtorValidation()
		{
			foreach (Job jobToPost in Jobs)
			{
				if (!jobToPost.HasErrors)
				{
					foreach (Charge charge in GetCharges(jobToPost))
					{
						if (!charge.IsRevenuePosted && IsSellEligibleToPost(charge))
						{
							if (charge.SellAccount != null && !charge.SellAccount.OH_IsActive)
							{
								return Res.GetString("6d0c06ad-806b-4f93-af19-63b1b64e898a", "Job {0} has error: The Debtor is inactive - it may not be used.", jobToPost.JH_JobNum);
							}

							charge.Validation.ValidateJR_OH_SellAccount();
							if (charge.JR_OH_SellAccountInfo.HasErrors())
							{
								return Res.GetString("062c080d-e18d-4929-a449-07ea247dd309", "Job {1} has error: Invalid Debtor: {0}", charge.JR_OH_SellAccountInfo.Notifications.GetErrors().ToUniqueMessageListString(), jobToPost.JH_JobNum);
							}
						}
					}
				}
			}
			return "";
		}

		protected string RunCreditorValidation()
		{
			foreach (Job jobToPost in Jobs)
			{
				if (!jobToPost.HasErrors)
				{
					foreach (Charge charge in GetCharges(jobToPost))
					{
						if (!charge.IsCostPosted && IsCostEligibleToPost(charge))
						{
							if (charge.CostAccount != null && !charge.CostAccount.OH_IsActive)
							{
								return Res.GetString("f0f86317-b449-4a8a-9557-5949b00b76ef", "Job {0} has error: The Creditor is inactive - it may not be used.", jobToPost.JH_JobNum);
							}
						}
					}
				}
			}
			return "";
		}

		protected string RunChequeBookValidation()
		{
			foreach (Job jobToPost in Jobs)
			{
				if (!jobToPost.HasErrors)
				{
					ISecurityCheckpoint rootSecurity = jobToPost.PlugInData != null ? jobToPost.PlugInData.InvoicingSupporter.JobInvoicingSecurity : null;
					ISecurityCheckpoint invoicingSecurity = rootSecurity != null ? rootSecurity.FindChild(rootSecurity.Code + SecurityCore.Invoicing) : null;
					ISecurityCheckpoint security = invoicingSecurity != null ? invoicingSecurity.FindChild(rootSecurity.Code + SecurityCore.AllowPostingOfPaymentsWhereCheckBookBranchIsDifferentToLoginBranch) : null;
					bool validateChequeBookBranch = security == null || !security.IsAllowed;

					foreach (Charge charge in GetCharges(jobToPost))
					{
						if (!charge.IsCostPosted && IsCostEligibleToPost(charge) && charge.ChequeBook != null && charge.ChequeBook.BankAccount != null)
						{
							if (charge.ChequeBook.AK_AutoPrintCheque && charge.ChequeBook.BankAccount.AB_SO_ChequeTemplate.IsEmpty)
							{
								return Res.GetString("485EBF7C-5439-4BFE-9C4B-5C58E2ACA6E9", "Auto printing of check is not configured properly.");
							}
							if (charge.ChequeBook.AK_AutoPrintCheque && !Env.Security.PrintCheque.IsAllowed)
							{
								return Res.GetString("ED6E695B-A2AA-410B-AA8D-F69A5E8E70F1", "You do not have the permission to print Check. Please Contact System Administrator.");
							}
							if (validateChequeBookBranch && charge.ChequeBook.AK_GB != GlbBranch.CurrentBranch.PK)
							{
								if (security == null)
								{
									string message = string.Format((NoResString)"Type {0} does not have a 'Billing > Allow posting of payments where check book branch is different to login branch' security checkpoint.", jobToPost.PlugInData.GetType());
									ExceptionReporter.Instance.ReportDeveloperException(message, message, new Exception(message + System.Environment.NewLine + (new System.Diagnostics.StackTrace().ToString())));
								}

								return Res.GetString("b5079e79-558f-4d2b-b98c-06584f38f6d7", "You are trying to post a payment using a check book from the '{0}' branch while logged into the '{1}' branch.", charge.ChequeBook.Branch.GB_Code, GlbBranch.CurrentBranch.GB_Code) + System.Environment.NewLine +
									(security != null ?
										Res.GetString("9940a8ea-d588-4394-a5e3-564064987952", "You do not have sufficient security rights to post checks from another branch.") + System.Environment.NewLine +
										Res.GetString("7b67fecb-b47f-4cd5-aeef-286262961f75", "Please use a check book from the '{0}' branch to post this payment or contact your system administrator to give you the following security rights: {1}.", GlbBranch.CurrentBranch.GB_Code, security.DisplayTextPathToSecurityRight) :
										Res.GetString("1695056e-0a89-4e4c-b975-5160e92866df", "There is no appropriate security right to allow posting. Please contact your system administrator.")
									);
							}
						}
					}
				}
			}
			return "";
		}

		protected string RunInvoiceTypeValidation()
		{
			foreach (Job jobToPost in Jobs)
			{
				if (!jobToPost.HasErrors)
				{
					foreach (Charge charge in GetCharges(jobToPost))
					{
						if (!charge.IsRevenuePosted && IsSellEligibleToPost(charge))
						{
							charge.Validation.ValidateJR_InvoiceType();
							if (charge.JR_InvoiceTypeInfo.HasErrors())
							{
								return Res.GetString("6297a6b5-8bb4-4cac-bf29-44dfa4293f9b", "Job {1} has error: Invalid invoice type: {0}", charge.JR_InvoiceTypeInfo.Notifications.GetErrors().ToUniqueMessageListString(), jobToPost.JH_JobNum);
							}
						}
					}
				}
			}
			return "";
		}

		struct OrgWithAmount
		{
			public OrgHeader org;
			public ZDecimal amount;
		}

		protected string RunCreditLimitValidation()
		{
			Dictionary<ZString, OrgWithAmount> debtorsToValidate = new Dictionary<ZString, OrgWithAmount>();

			foreach (Job jobToPost in Jobs)
			{
				if (!jobToPost.HasErrors)
				{
					foreach (Charge charge in GetCharges(jobToPost))
					{
						if (charge.SellAccount != null && !charge.IsRevenuePosted && IsSellEligibleToPost(charge))
						{
							var totalRevenue = charge.TotalLocalRevenueAmount + charge.JR_Sell_LocalWHTAmount;
							if (!debtorsToValidate.ContainsKey(charge.SellAccount.OH_Code))
							{
								debtorsToValidate.Add(charge.SellAccount.OH_Code, new OrgWithAmount() { org = charge.SellAccount, amount = totalRevenue });
							}
							else
							{
								OrgWithAmount orgWithAmount = debtorsToValidate[charge.SellAccount.OH_Code];
								orgWithAmount.amount += totalRevenue;
							}
						}
					}
				}
			}

			var result = new StringBuilder();
			var localRevenueOptionIsPosted = AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.Value == Constants.CreditLimitChecking.Posted;
			var globalRevenueOptionIsPosted = AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInGlobalCreditLimitCalculation.Value == Constants.CreditLimitChecking.Posted;

			foreach (var orgWithAmount in debtorsToValidate.Values)
			{
				var message = orgWithAmount.org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, localRevenueOptionIsPosted ? orgWithAmount.amount : ZDecimal.Zero);
				if (string.IsNullOrEmpty(message) && localRevenueOptionIsPosted != globalRevenueOptionIsPosted && !orgWithAmount.org.MiscServ.ARGlobalCreditLimit.IsEmpty)
				{
					message = orgWithAmount.org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, globalRevenueOptionIsPosted ? orgWithAmount.amount : ZDecimal.Zero);
				}
				if (!string.IsNullOrEmpty(message))
				{
					result.AppendLine(message);
					result.AppendLine();
				}
			}

			return result.ToString().TrimEnd();
		}

		protected virtual bool IsCostEligibleToPost(Charge charge)
		{
			bool result = false;
			if (charge.CostAccount != null && (!charge.JR_APInvoiceNum.IsEmpty || charge.CostAccount.CompanyData.OB_APCostsSelfBilled))
			{
				switch (PostingOption)
				{
					case JobInvoicingPostingOption.All:
					case JobInvoicingPostingOption.Costs:
						result = charge.HasCostAmount;
						break;
					case JobInvoicingPostingOption.CustomsDSBChargeAPOnly:
						result = charge.IsDisbursementInvoiceCharge && charge.PostAPWhenInvokedByCustomInvoiceCreator && charge.HasCostAmount;
						break;
					case JobInvoicingPostingOption.ConsolCosts:
						result = charge.JR_IsApportioned && charge.HasCostAmount;
						break;
				}
			}
			return result;
		}

		protected virtual bool IsSellEligibleToPost(Charge charge)
		{
			bool result = false;
			switch (PostingOption)
			{
				case JobInvoicingPostingOption.All:
				case JobInvoicingPostingOption.Revenue:
					result = charge.JR_LocalSellAmt != 0;
					break;
				case JobInvoicingPostingOption.LocalClient:
					result = charge.IsLocalClientCharge && charge.JR_LocalSellAmt != 0;
					break;
				case JobInvoicingPostingOption.Agent:
					result = IsAgentCharge(charge) && charge.JR_LocalSellAmt != 0;
					break;
				case JobInvoicingPostingOption.Gateway:
					result = IsGatewayCharge(charge) && charge.JR_LocalSellAmt != 0;
					break;
				case JobInvoicingPostingOption.Disbursement:
					result = charge.IsDisbursementInvoiceCharge && charge.JR_LocalSellAmt != 0;
					break;
				case JobInvoicingPostingOption.CustomsDSBChargeAROnly:
					result = charge.IsDisbursementInvoiceCharge && charge.PostARWhenInvokedByCustomInvoiceCreator && charge.JR_LocalSellAmt != 0;
					break;
				case JobInvoicingPostingOption.AllSisterCompanyCharges:
					result = charge.IsSisterCompanyCharge(false) && charge.JR_LocalSellAmt != 0;
					break;
				case JobInvoicingPostingOption.LocalSisterCompanyChargesOnly:
					result = charge.IsSisterCompanyCharge(true) && charge.JR_LocalSellAmt != 0;
					break;
			}
			return result && IsAllowedToPostSellCharge(charge);
		}

		protected virtual bool IsAllowedToPostSellCharge(Charge charge) => charge.IsAllowedToPostSellCharge;

		protected virtual ZBool IsAgentCharge(Charge charge) => charge.IsAgentCharge;

		protected virtual ZBool IsGatewayCharge(Charge charge) => false;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message builder")]
		protected string RunJobsAreSavedValidation()
		{
			string result = "";
			var isAllowedToSkipValidation = AccountingMasterFilesRegistry.Instance.AllowTriggersToSkipSaveEverythingBeforePostingValidation.Value;

			foreach (Job jobToPost in OriginalJobs)
			{
				if (isAllowedToSkipValidation && jobToPost.Factory.HasContext(BusinessContext.PostingChargesFromLogWalker))
				{
					continue;
				}

				BusinessObject jobParent = jobToPost.Parent as BusinessObject;
				if (jobParent != null && jobParent.Factory == jobToPost.Factory && (jobParent.HasChanges || !jobParent.IsInDatabase))
				{
					result += Res.GetString("6401fe8c-3e7a-4a9c-8f26-ea0d8e8bb2a2", "Please save this form before posting costs and/or charges.") + "\n";
				}
				if (jobToPost.HasChanges && !jobToPost.IsDeleted)
				{
					result += Res.GetString("8f727ff3-2ead-48f9-80df-c46779de7d31", "Please save job {0} before posting costs and/or charges.", jobToPost.JH_JobNum) + "\n";
					var changeSet = jobToPost.Factory.GetChanges().GetChangedObjects().Select(j => j.SessionInstance).Where(j => j.IsInDatabase && j.HasChanges && !j.IsDeleted);
					var extraMessage = new ZStringBuilder();

					var changesBizos = from c in changeSet
									   group c by c.GetType().Name into g
									   select new
									   {
										   Type = g.Key,
										   Name = g.First().HumanReadableName,
										   BizosCount = g.Count(),
										   ChangedFields = g.SelectMany(t => t.GetFieldsListWithChanges()),
										   JobNumbers = g.Key == nameof(Job) ? string.Join(", ", g.Select(t => ((Job)t).JH_JobNum)) : string.Empty
									   };

					foreach (var bizo in changesBizos)
					{
						var changedFieldsMessage = "The following fields have changed: ";

						if (bizo.Type == nameof(Job))
						{
							extraMessage.AppendLine(Res.GetString("1eac9e60-1283-450e-a587-bee284ddbd56", "{0} Job ({1}) have changed.", bizo.BizosCount, bizo.JobNumbers));
						}
						else
						{
							extraMessage.AppendLine(Res.GetString("3c6979b1-862b-4bbf-9331-98a9638bf0c1", "{0} {1} have changed.", bizo.BizosCount, bizo.Name));
						}
						extraMessage.AppendLine(changedFieldsMessage + string.Join(", ", bizo.ChangedFields.Distinct()));
					}

					if (extraMessage.Length > 0)
					{
						result += "\n----\n\n" + extraMessage + "\n";
					}	
				}
			}
			return result;
		}

		protected virtual string RunNoChargesValidation()
		{
			bool foundCharges = false;
			foreach (Job jobToPost in Jobs)
			{
				if (GetCharges(jobToPost).Any())
				{
					foundCharges = true;
					break;
				}
			}
			return (foundCharges) ? "" : Res.GetString("4c3d4a48-eebb-451e-84ae-0fe55d300de2", "Please enter charges before posting.");
		}

		protected string RunRevenueRecognitionDateValidation()
		{
			var result = ZString.Empty;
			foreach (Job jobToPost in GetJobsToValidate(Jobs))
			{
				Charge[] jobToPostCharges = GetCharges(jobToPost).ToArray();
				foreach (Charge charge in jobToPostCharges)
				{
					var isToBeCheckedForRevenueRecognitionValidationError = (!charge.IsCostPosted && charge.JR_LocalCostAmt != 0m && IsCostEligibleToPost(charge))
						|| (!charge.IsRevenuePosted && charge.JR_LocalSellAmt != 0m && IsSellEligibleToPost(charge));
					if (isToBeCheckedForRevenueRecognitionValidationError)
					{
						result = GetRevenueRecognitionValidationError(charge);
					}
					CriticalValidationInfoCollectorService.GetOrCreateService(charge.Factory).AddInfoWhenAllowed(charge.ChargeCode.PK,
						CriticalValidationInfoCollectorServiceKeyType.RevenueRecognitionTypeFromJobDuringPreSaveValidation,
						() => FormattableString.Invariant(
$@"ChargeCode PK: {charge.ChargeCode.PK}
Job PK: {jobToPost.PK}

Revenue Recognition Type Details: 
{jobToPost.GetRevenueRecognitionDetails(charge.ChargeCode)}

isToBeCheckedForRevenueRecognitionValidationError: {isToBeCheckedForRevenueRecognitionValidationError}
GetRevenueRecognitionValidationError result: {result}"));
					if (result != string.Empty)
					{
						return result;
					}
				}

				CriticalValidationHelpers.ReportArrayChanges(GetCharges(jobToPost).ToArray(), jobToPostCharges, "RunRevenueRecognitionDateValidation",
																	CriticalValidationHelpers.ArrayChangedWarningMessage, false, (charge) => charge.GetJobChargeInfo());
			}
			return result;
		}

		protected virtual string GetRevenueRecognitionValidationError(Charge charge)
		{
			if (charge.InvoicingJob?.Validation is JobValidation jobValidation)
			{
				return jobValidation.GetRevenueRecognitionDateValidationError(Res.GetString("3CEED5C2-CCA7-4b75-844A-90182B953D09", "Posting is prevented. The following information has not been recorded for this job. It is required for revenue recognition purposes and must be recorded before posting can occur: {0:G}"), charge.ChargeCode, isPosting: true);
			}
			return string.Empty;
		}

		protected string RunPaymentTypeSecurityValidation()
		{
			foreach (Job jobToPost in Jobs)
			{
				foreach (Charge chrg in GetCharges(jobToPost))
				{
					if (!chrg.IsCostPosted)
					{
						bool isAllowed = true;
						switch (chrg.JR_PaymentType)
						{
							case ReceiptTypes.Cheque:
								isAllowed = Env.Security.NewPayablesPaymentCheque.IsAllowed || Env.Security.APPaymentProcessingNewCheque.IsAllowed;
								break;

							case ReceiptTypes.Cash:
								isAllowed = Env.Security.NewPayablesPaymentCash.IsAllowed || Env.Security.APPaymentProcessingNewCash.IsAllowed;
								break;

							case ReceiptTypes.CreditCard:
								isAllowed = Env.Security.NewPayablesPaymentCreditCard.IsAllowed || Env.Security.APPaymentProcessingNewCreditCard.IsAllowed;
								break;

							case ReceiptTypes.DirectDebit:
								isAllowed = Env.Security.NewPayablesPaymentDirectDebit.IsAllowed || Env.Security.APPaymentProcessingNewDirectDebit.IsAllowed;
								break;

							case ReceiptTypes.EFT:
								isAllowed = Env.Security.NewPayablesPaymentEFT.IsAllowed || Env.Security.APPaymentProcessingNewEFT.IsAllowed;
								break;
							case ReceiptTypes.ScheduledEFT:
								isAllowed = Env.Security.NewPayablesPaymentSFT.IsAllowed || Env.Security.APPaymentProcessingNewSFT.IsAllowed;
								break;
							case ReceiptTypes.CollectionRequest:
								isAllowed = Env.Security.NewPayablesPaymentCRQ.IsAllowed || Env.Security.APPaymentProcessingNewCRQ.IsAllowed;
								break;
						}

						if (!isAllowed)
						{
							return Res.GetString("26a662b0-c948-48d3-859e-7a5ebc80f4f5", "You do not have appropriate security rights to post charges with payment type '{0}'.", chrg.JR_PaymentType);
						}
					}
				}
			}

			return "";
		}

		protected string RunConsolCostRelativeValidation()
		{
			foreach (Job jobToPost in Jobs)
			{
				foreach (Charge charge in GetCharges(jobToPost))
				{
					IEnumerable<INotification> warnings = Array.Empty<INotification>();
					string expectedWarningMessage = string.Empty;
					JobConsolCost consolCost = charge.Factory.Load<JobConsolCost>(charge.JR_E6);
					if (consolCost != null)
					{
						consolCost.Validation.ValidateE6_InvoiceNum();
						warnings = consolCost.E6_InvoiceNumInfo.GetWarnings();
						expectedWarningMessage = ForwardingConsolCostingValidation.UnapprovedInvoicesWithPaymentWarningMessage;

						if (consolCost.E6_AH_APInvoice.IsEmpty && (consolCost.E6_InvoiceDate != charge.JR_APInvoiceDate || consolCost.E6_PaymentDate != charge.JR_PaymentDate))
						{
							return BaseChargeValidation.ConsolCostIsInconsistentWithApportionChargeErrorMessage();
						}
					}
					else
					{
						charge.Validation.ValidateJR_APInvoiceNum();
						warnings = charge.JR_APInvoiceNumInfo.GetWarnings();
						expectedWarningMessage = ChargeWithCostValidation.UnapprovedInvoicesWithPaymentWarningMessage;
					}
					foreach (INotification warning in warnings)
					{
						if (warning.Message == expectedWarningMessage)
						{
							return expectedWarningMessage;
						}
					}
				}
			}
			return string.Empty;
		}

		protected string RunStampDutyValidation()
		{
			bool returnError = false;

			if (IsRevenueBeingPosted(PostingOption)
				&& GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Italy
				&& AccountingConfigurationRegistry.Instance.StampDutyChargeCode.Value == Guid.Empty)
			{
				Guid[] taxIDsAttractingStampDuty = AccountingConfigurationRegistry.Instance.TaxIDsAttractingStampDuty.GetAsGuidArray();

				foreach (Job jobToPost in Jobs)
				{
					if (returnError)
					{
						break;
					}

					PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(jobToPost.ReceivableCharges);

					foreach (PostingChargeKey key in distributedCharges.Keys)
					{
						if (returnError)
						{
							break;
						}

						IReceivablesPostingChargeCollection charges = distributedCharges[key];
						ZDecimal total = 0m;

						if (charges.Count > 0 && charges[0].Debtor != null && charges[0].Debtor.UNLOCO != null)
						{
							if (charges[0].Debtor.UNLOCO.RL_RN_NKCountryCode == Core.Constants.CountryCodes.Italy)
							{
								foreach (Charge charge in charges.Where(x => IsAllowedToPostSellCharge((Charge)x)))
								{
									foreach (Guid taxID in taxIDsAttractingStampDuty)
									{
										if (charge.JR_AT_SellGSTRate == taxID && !charge.JR_IsRevenuePosted)
										{
											total += charge.JR_LocalSellAmt;
										}
									}
								}

								if (total > AccountingConfigurationRegistry.Instance.StampDutyThreshold.Value)
								{
									returnError = true;
								}
							}
						}
					}
				}
			}

			return returnError ? Res.GetString("46ce5407-4a45-40b4-a67e-420f46c51bfc", "The invoice being posted attracts stamp duty, but there is no 'Stamp Duty Charge Code' defined in the registry. Please define an appropriate charge code in the registry under Accounting > Receivable Defaults > Default Settings > Stamp Duty Charge Code.") : string.Empty;
		}

		protected string RunCompanyAndOrgsRegistrationNumbersValidation()
		{
			var result = ZString.Empty;
			if (CountrySpecificValidationHelper.NeedToCheckCompanyAndOrgsRegistrationNumber())
			{
				var errorList = new List<ZString>();
				if (GlbCompany.CurrentCompany.GC_BusinessRegNo.IsEmpty)
				{
					errorList.Add(Res.GetString("1ED43BA4-E937-4C20-ABB4-836A88219387", "Tax Registration Number of the Login Company [{0}]", GlbCompany.CurrentCompany.GC_Code));
				}
				var sellOrgHeader = new HashSet<OrgHeader>();
				var costOrgHeader = new HashSet<OrgHeader>();
				foreach (Job jobToPost in Jobs)
				{
					if (!jobToPost.HasErrors)
					{
						foreach (Charge charge in GetCharges(jobToPost))
						{
							if (!charge.IsRevenuePosted && IsSellEligibleToPost(charge))
							{
								if (charge.SellAccount != null && charge.SellAccount.PrimaryRegistrationNumber.Number.IsEmpty)
								{
									sellOrgHeader.Add(charge.SellAccount);
								}
							}
							if (!charge.IsCostPosted && IsCostEligibleToPost(charge))
							{
								if (charge.CostAccount != null && charge.CostAccount.PrimaryRegistrationNumber.Number.IsEmpty)
								{
									costOrgHeader.Add(charge.CostAccount);
								}
							}
						}
					}
				}
				if (sellOrgHeader.Count > 0)
				{
					errorList.Add(Res.GetString("211A0DD7-0CD7-49AC-BDC0-0396040D2543", "Tax Registration Number of the Debtor [{0}]", string.Join(",", sellOrgHeader.Select(x => x.OH_Code))));
				}
				if (costOrgHeader.Count > 0)
				{
					errorList.Add(Res.GetString("AD2F3FA9-9988-41E6-B12B-F96782C6C4BD", "Tax Registration Number of the Creditor [{0}]", string.Join(",", costOrgHeader.Select(x => x.OH_Code))));
				}
				if (errorList.Count > 0)
				{
					errorList.Insert(0, Res.GetString("8E880D42-5964-4529-8D42-46E4407F66B2", "The transaction is missing information that is mandatory for your country/region reporting:"));
					result = string.Join("\r\n", errorList);
				}
			}
			return result;
		}

		protected string RunBranchDepartmentCombinationsValidation()
		{
			foreach (Job jobToPost in Jobs)
			{
				if (!jobToPost.HasErrors)
				{
					foreach (Charge charge in GetCharges(jobToPost))
					{
						if ((!charge.IsRevenuePosted && IsSellEligibleToPost(charge)) || (!charge.IsCostPosted && IsCostEligibleToPost(charge)))
						{
							var errorMsg = GlbBranchCombinationValidation.CheckBranchDepartmentCombination(charge.Branch, charge.Department);
							if (!string.IsNullOrEmpty(errorMsg))
							{
								return Res.GetString("bf1678ae-d0ad-473c-a6df-e23fd7d0b85e", "This job cannot be posted. {0}", errorMsg);
							}
						}
					}
				}
			}
			return "";
		}

		protected virtual string RunExchangeRateValidation()
		{
			foreach (Job jobToPost in Jobs)
			{
				if (!jobToPost.HasErrors)
				{
					foreach (Charge charge in GetCharges(jobToPost))
					{
						string errorMessage;

						if (!charge.IsRevenuePosted && IsSellEligibleToPost(charge))//the sell rate will be calculated via buy rate later.
						{
							var chargeIsInLocalInvoiceCurrencyForPosting = charge.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR);
							if (AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum.AR, chargeIsInLocalInvoiceCurrencyForPosting, charge.JR_GC) != AccountingConstants.InvoicePostingExchangeRateOption.Default.Code)
							{
								errorMessage = ExchangeRateCalculator.CheckExchangeRate(
								jobToPost,
								charge.BillInInvoiceCurrency ? charge.JR_RX_NKSellInvoiceCurrency : charge.JR_SellCurrency,
								chargeIsInLocalInvoiceCurrencyForPosting,
								ExchangeRateValidLedgerEnum.AR,
								charge.JR_OH_SellAccount,
								charge.JR_Calc_ARInvoiceDate,
								ZDateTime.Now,
								charge.JR_SellTaxDate);

								if (!string.IsNullOrWhiteSpace(errorMessage))
								{
									return Res.GetString("bf1678ae-d0ad-473c-a6df-e23fd7d0b85e", @"This job cannot be posted. {0}", errorMessage);
								}
							}
						}

						if (!charge.IsCostPosted && IsCostEligibleToPost(charge))
						{
							var isLocalInvoiceCurrency = charge.JR_CostCurrency == charge.Company.GC_RX_NKLocalCurrency;
							if (AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum.AP, isLocalInvoiceCurrency, charge.JR_GC) != AccountingConstants.InvoicePostingExchangeRateOption.Default.Code)
							{
								errorMessage = ExchangeRateCalculator.CheckExchangeRate(
								jobToPost,
								charge.JR_CostCurrency,
								isLocalInvoiceCurrency,
								ExchangeRateValidLedgerEnum.AP,
								charge.JR_OH_CostAccount,
								charge.JR_APInvoiceDate,
								ZDateTime.Now,
								charge.JR_CostTaxDate);

								if (!string.IsNullOrWhiteSpace(errorMessage))
								{
									return Res.GetString("bf1678ae-d0ad-473c-a6df-e23fd7d0b85e", @"This job cannot be posted. {0}", errorMessage);
								}
							}
						}
					}
				}
			}
			return "";
		}

		#region Supply Type

		protected virtual ZBool IsEnableValidationForSupplyTypeOfConsolApportionedCharges => false;
		protected virtual string SellSupplyTypeErrorMessage => Res.GetString("575A729D-6C66-4FDE-B4BE-1B504F0A4146", "Sell Supply Type must be entered.");

		protected virtual string RunJobChargeSupplyTypeValidation()
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
			{
				return string.Empty;
			}

			foreach (Job jobToPost in Jobs)
			{
				if (!jobToPost.HasErrors)
				{
					foreach (Charge charge in GetCharges(jobToPost))
					{
						if (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.Value)
						{
							var postInfoRev = !charge.IsRevenuePosted && IsSellEligibleToPost(charge) ? jobToPost.PlugInData.InvoicingSupporter.ConsumerType.ShouldPostCharges(jobToPost.PlugInData, charge.JR_InvoiceType, false) : null;
							var postInfoCost = !charge.IsCostPosted && IsCostEligibleToPost(charge) ? jobToPost.PlugInData.InvoicingSupporter.ConsumerType.ShouldPostCharges(jobToPost.PlugInData, charge.JR_InvoiceType, true) : null;

							if (postInfoRev != null &&
								postInfoRev.PostAllowed &&
								ShouldMandatorySellSupplyType(charge.JR_InvoiceType) &&
								charge.JR_SellSupplyType.IsEmpty)
							{
								return SellSupplyTypeErrorMessage;
							}

							if (postInfoCost != null && postInfoCost.PostAllowed && charge.JR_CostSupplyType.IsEmpty)
							{
								return Res.GetString("B651A8DA-62E4-407C-A629-1D03B9378E83", "Cost Supply Type must be entered.");
							}
						}

						if (IsEnableValidationForSupplyTypeOfConsolApportionedCharges)
						{
							var consolCost = charge.Factory.Load<JobConsolCost>(charge.JR_E6);
							if (consolCost != null && !consolCost.IsPosted && consolCost.ApportionmentCharges.OfType<ApportionSplitCharge>().Any(x => x.JR_CostSupplyType != consolCost.E6_SupplyType))
							{
								return Res.GetString("2F3D1698-B2EF-4016-BB0D-E0240F045486", "The Cost Supply Type value of all apportioned charges must be the same. Please re-enter the Consol Cost's Cost Supply Type to update the apportioned charges.");
							}
						}
					}
				}
			}
			return string.Empty;
		}

		protected virtual bool ShouldMandatorySellSupplyType(ZString invoiceType) => !InvoiceTypeCalculationProvider.IsDeferredInvoiceType(invoiceType);

		#endregion

		#region Create ValidateWrapper

		protected Func<PostManagerNotification> CreateErrorValidator(Func<string> validation, PostManagerValidationType errorType)
		{
			return () =>
			{
				string result = validation();
				return string.IsNullOrEmpty(result) ? null : GetErrorNotification(result, errorType);
			};
		}

		protected Func<PostManagerNotification> CreateWarningValidator(Func<string> validation, PostManagerValidationType warningType)
		{
			return () =>
			{
				string result = validation();
				return string.IsNullOrEmpty(result) ? null : GetWarningNotification(result, warningType);
			};
		}

		#endregion

		#endregion

		protected virtual string RunConsolCostTaxBranchValidation()
		{
			return string.Empty;
		}

		protected virtual string RunJobChargeSellTaxBranchValidation()
		{
			return string.Empty;
		}
	}
}
