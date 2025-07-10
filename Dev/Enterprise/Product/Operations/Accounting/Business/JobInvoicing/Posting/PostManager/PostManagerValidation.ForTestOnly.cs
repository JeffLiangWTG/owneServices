#if DEBUG

using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class PostManagerValidation
	{
		public JobInvoicingPostingOption PostingOption_ForTestOnly
		{
			get => PostingOption;
		}

		public bool IsBulkPosting_ForTestOnly => IsBulkPosting;
		
		public string RunJobsContainErrorsValidation_ForTestOnly()
		{
			return RunJobsContainErrorsValidation();
		}

		public string RunCreditLimitValidation_ForTestOnly()
		{
			return RunCreditLimitValidation();
		}

		public string RunCompanyAndOrgsRegistrationNumbersValidation_ForTestOnly()
		{
			return RunCompanyAndOrgsRegistrationNumbersValidation();
		}

		public string RunRevenueRecognitionDateValidation_ForTestOnly()
		{
			return RunRevenueRecognitionDateValidation();
		}

		public string RunOperationalTaxDateErrorValidation_ForTestOnly(bool isPostingAR)
		{
			return RunOperationalTaxDateErrorValidation(isPostingAR);
		}

		public string RunOperationalTaxDateWarningValidation_ForTestOnly(bool isPostingAR)
		{
			return RunOperationalTaxDateWarningValidation(isPostingAR);
		}

		public string RunExchangeRateValidation_ForTestOnly()
		{
			return RunExchangeRateValidation();
		}

		public string RunGSTApplicabilityValidation_ForTestOnly()
		{
			return RunGSTApplicabilityValidation();
		}

		public string RunBranchDepartmentCombinationsValidation_ForTestOnly()
		{
			return RunBranchDepartmentCombinationsValidation();
		}

		public string RunInvoiceTypeValidation_ForTestOnly()
		{
			return RunInvoiceTypeValidation();
		}

		public string RunDebtorValidation_ForTestOnly()
		{
			return RunDebtorValidation();
		}

		public string RunChargesCanBePostedValidation_ForTestOnly()
		{
			return RunChargesCanBePostedValidation();
		}

		public string RunChequeBookValidation_ForTestOnly()
		{
			return RunChequeBookValidation();
		}

		public bool IsSellEligibleToPost_ForTestOnly(Charge charge)
		{
			return IsSellEligibleToPost(charge);
		}

		public string RunChargeCodeValidation_ForTestOnly()
		{
			return RunChargeCodeValidation();
		}

		public string RunChargesHaveCFXAccountValidation_ForTestOnly()
		{
			return RunChargesHaveCFXAccountValidation();
		}

		public string RunStampDutyValidation_ForTestOnly()
		{
			return RunStampDutyValidation();
		}

		public bool IsRevenueBeingPosted_ForTestOnly(JobInvoicingPostingOption postingOption)
		{
			return IsRevenueBeingPosted(postingOption);
		}

		public string RunDifferencesInReloadedChargesValidation_ForTestOnly()
		{
			return RunDifferencesInReloadedChargesValidation();
		}

		public bool IsCostEligibleToPost_ForTestOnly(Charge charge)
		{
			return IsCostEligibleToPost(charge);
		}

		public string RunJobChargeSupplyTypeValidation_ForTestOnly()
		{
			return RunJobChargeSupplyTypeValidation();
		}

		public string RunConsolCostTaxBranchValidation_ForTestOnly()
		{
			return RunConsolCostTaxBranchValidation();
		}

		public string RunJobChargeSellTaxBranchValidation_ForTestOnly()
		{
			return RunJobChargeSellTaxBranchValidation();
		}

		public bool ShouldMandatorySellSupplyType_ForTestOnly(ZString invoiceType) => ShouldMandatorySellSupplyType(invoiceType);
	}
}

#endif
