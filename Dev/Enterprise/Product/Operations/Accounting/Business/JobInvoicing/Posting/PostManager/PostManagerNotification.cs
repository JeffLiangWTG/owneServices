using System;
using CargoWise.ComponentModel;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public enum PostManagerValidationType
	{
		Undefined,
		ValidateJobAndParentSavedOnly,
		DifferencesInReloadedChargesValidation,
		PeriodValidation,
		JobNotFoundValidation,
		DebtorValidation,
		CreditorValidation,
		InvoiceTypeValidation,
		JobsContainErrorsValidation,
		NoChargesValidation,
		GSTApplicabilityValidation,
		RevenueRecognitionDateValidation,
		ChargeCodeValidation,
		ChargesCanBePostedValidation,
		ChargesHaveCFXAccountValidation,
		ChequeBookValidation,
		PaymentTypeSecurityValidation,
		ConsolCostRelativeValidation,
		RoundingRegistryItemValidation,
		StampDutyValidation,
		CreditLimitValidation,
		CustomsDisbursementChargesDuplicateCheckValidation,
		PaymentApprovalSecurityValidation,
		BranchDepartmentCombinationsValidation,
		ExchangeRateValidation,
		MissingTaxRegistrationNumberValidation,
		JobChargePostingHashMismatch,
		JobChargePostingAdditionalValidation,
		TaxDateValidationForAR,
		TaxDateValidationForAP,
		JobChargeSupplyTypeValidation,
		ConsolCostTaxBranchValidation,
		JobChargeTaxBranchValidation
	}

	[Serializable]
	public class PostManagerNotification : Notification
	{
		public PostManagerNotification(INotificationType notificationType, string message, PostManagerValidationType validationType)
			: base(notificationType, message)
		{
			ValidationType = validationType;
		}

		public PostManagerValidationType ValidationType { get; private set; }
	}
}
