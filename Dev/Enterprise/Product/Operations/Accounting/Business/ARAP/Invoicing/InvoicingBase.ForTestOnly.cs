#if DEBUG

using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class InvoicingBase
	{
		public bool IsImportingConsolApportionment_ForTestOnly
		{
			set { IsImportingConsolApportionment = value; }
		}

		public ZBool IsApportionmentGSTMandatory_ForTestOnly => IsApportionmentGSTMandatory;

		public void ImportJobChargeAmountIntoLine_ForTestOnly(JobInvoicing.Charge charge, InvoicingLineBase line)
		{
			ImportJobChargeAmountIntoLine(charge, line);
		}

		public bool Level1AuthorisationRequired_ForTestOnly(AmountBasedMultiLevelAuthorisationRequirement authorisationRequirement)
		{
			return Level1AuthorisationRequired(authorisationRequirement);
		}

		public bool Level2AuthorisationRequired_ForTestOnly(AmountBasedMultiLevelAuthorisationRequirement authorisationRequirement)
		{
			return Level2AuthorisationRequired(authorisationRequirement);
		}

		public bool ValidateExpectedInvoiceTotalSecurityIsAllowed_ForTestOnly => ValidateExpectedInvoiceTotalSecurityIsAllowed;

		public bool ValidateExpectedInvoiceTotal_ReadOnly_ForTestOnly => ValidateExpectedInvoiceTotal_ReadOnly;

		public bool IsReceivableOrPayable_ForTestOnly => IsReceivableOrPayable;

		public bool IsARAP_ForTestOnly => IsARAP;

		public bool IsInvoiceOrCreditNote_ForTestOnly => IsInvoiceOrCreditNote;

		public bool IsInvoiceOrCreditNoteOrAdjustmentNote_ForTestOnly => IsInvoiceOrCreditNoteOrAdjustmentNote;

		public bool SupportHeaderReference_ForTestOnly => SupportHeaderReference;

		public AccountingNumberFountainWrapper NumberFountainForInternalRef_ForTestOnly => NumberFountainForInternalRef;

		public string GetParentTableCodeFromInvoiceType_ForTestOnly(OperationsInvoiceTypes invoiceType)
		{
			return GetParentTableCodeFromInvoiceType(invoiceType);
		}

		public void CopyTransactionLine_ForTestOnly(InvoicingLineBase fromLine, InvoicingLineBase toLine, bool populateAmount = true)
		{
			CopyTransactionLine(fromLine, toLine, populateAmount);
		}

		public TransactionHeader CopyTransaction_ForTestOnly()
		{
			return CopyTransaction();
		}

		public ZGuid GetChargeCodeTaxPK_ForTestOnly(ZGuid lineChargeCode)
		{
			return GetChargeCodeTaxPK(lineChargeCode);
		}

		public bool MustTransform_ForTestOnly
		{
			get { return MustTransform; }
			set { MustTransform = value; }
		}

		public void GenerateReverseTransactionCore_ForTestOnly(bool mustTransform)
		{
			GenerateReverseTransactionCore(mustTransform);
		}

		public AccountingNumberFountainWrapper NumberFountainForSelfBillingInvoice_ForTestOnly => NumberFountainForSelfBillingInvoice;

		public void SetDefaultValues_ForTestOnly()
		{
			SetDefaultValues();
		}

		public bool AH_OH_ReadOnly_ForTestOnly => AH_OH_ReadOnly;

		public bool AH_ConsolidatedInvoiceRef_ReadOnly_ForTestOnly => AH_ConsolidatedInvoiceRef_ReadOnly;

		public bool InvoiceRemittanceReference_ReadOnly_ForTestOnly => InvoiceRemittanceReference_ReadOnly;

		public bool IsUseJobExchangeRateApplicable_ForTestOnly => IsUseJobExchangeRateApplicable;

		public void AdjustLocalRoundedValuesForImportedConsolCosts_ForTestOnly()
		{
			AdjustLocalRoundedValuesForImportedConsolCosts();
		}

		public Dictionary<Tuple<ZGuid, ZShort>, List<ZGuid>> GetDuplicateLineSequenceLookup_ForTestOnly()
		{
			return duplicateLineSequenceLookup;
		}

		public ComplianceSubTypeRule ComplianceSubTypeRule_ForTestOnly => ComplianceSubTypeRule;

		public bool IsCreatingAPInvoiceOrCRD_ForTestOnly => IsCreatingAPInvoiceOrCRD;

		public bool IsCreatingUAInvoiceOrCRD_ForTestOnly => IsCreatingUAInvoiceOrCRD;

		public TermsAndDueDateCalculationProvider FTermsAndDueDateCalculationProvider_ForTestOnly
		{
			get { return fTermsAndDueDateCalculationProvider; }
			set { fTermsAndDueDateCalculationProvider = value; }
		}

		public IEnumerable<JobInvoicing.Job> GetAllJobs_ForTestOnly()
		{
			return GetAllJobs();
		}

		public void RecalculateHeaderAmounts_ForTestOnly()
		{
			RecalculateHeaderAmounts();
		}

		public ZBool LinesContainTax_ForTestOnly => LinesContainTax;

		public InvoiceRemittanceConfiguration InvoiceRemittanceConfiguration_ForTestOnly
		{
			get { return InvoiceRemittanceConfiguration; }
			set { InvoiceRemittanceConfiguration = value; }
		}

		public bool IsSettingAH_ExchangeRate_ForTestOnly
		{
			get { return IsSettingAH_ExchangeRate; }
			set { IsSettingAH_ExchangeRate = value; }
		}

		public HiddenStmNote GetDetailsNoteIfItExists_ForTestOnly(BusinessObjectFactory factory = null)
		{
			return GetDetailsNoteIfItExists(factory);
		}

		public string IncompleteTransactionDataDescription_ForTestOnly => IncompleteTransactionDataDescription;

		public bool IsComplianceSequenceFailedToAssign_ForTestOnly
		{
			get => IsComplianceSequenceFailedToAssign;
			set => IsComplianceSequenceFailedToAssign = value;
		}

		public static bool ShouldClearAttachedFileContent_ForTestOnly
		{
			get => shouldClearAttachedFileContent_ForTestOnly;
			set => shouldClearAttachedFileContent_ForTestOnly = value;
		}

		[ThreadStatic]
		static bool shouldClearAttachedFileContent_ForTestOnly;

		[ThreadStatic]
		public static bool ShouldCheckTransactionHeaderReferenceATH_ForTestOnly;

		public void OnFactorySavingBeforeTransactionCore_ForTestOnly()
		{
			OnFactorySavingBeforeTransactionCore();
		}

		public void OnSaved_ForTestOnly()
		{
			OnSaved(true);
		}

		public void AcceptDataRowChangesToTestOriginalValueWhenWeCantSave_ForTestOnly() =>
			((INeedRow)this).Row.AcceptChanges();

		public void SetIsValidationOfValidateExpectedInvoiceTotalEnabled_ForTestOnly(bool value) =>
			IsValidationOfValidateExpectedInvoiceTotalEnabled = value;

		public void SetValidateExpectedInvoiceTotal_ForTestOnly(bool value) =>
			ValidateExpectedInvoiceTotal = value;

		public void SetIsSetFromDraftInvoice_ForTestOnly(bool value) =>
			IsSetFromDraftInvoice = value;

		public void ClearReadOnlyForAssociatedDraftInvoice_ForTestOnly() =>
			readOnlyForAssociatedDraftInvoice = null;

		public bool GetIsSetFromDraftInvoice_ForTestOnly() => IsSetFromDraftInvoice;
	}
}

#endif
