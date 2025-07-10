using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Integration.Accounting
{
	public interface IAccounting
	{
		IRegistry Registry { get; }
		bool IsENettOrganisation(ZGuid organisationPk);
		Guid ARAccountGroup { get; }
		Guid APAccountGroup { get; }
		int CollectionCallFollowUpDays { get; }
		ZString SelfBillingInvoiceTransactionNumberPrefix(Guid companyPk);
		ZString InvoiceTransactionNumberPrefix(Guid companyPk);
		bool ShouldCollectionCallCreateFollowUpAppointments { get; }
		ZString GLAccountFormat { get; }
		ZString GetGLAccountFormat(string accountNum);
		bool OverrideInterOfficeBillingTaxIDToNOTREPORT { get; }
		bool OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy { get; }
		ZGuid GroupMemberBillingDefaultInvoiceTaxMessage { get; }
		bool EnableLocalChargeCodeDescriptionDefault { get; }
		bool ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors { get; }
		Guid PLAppropriationAccount { get; }
		IRegistryItem PlAppropriationAccountRegistryItem { get; }
		ICodeDescriptionPairList QueryClaimTypeCodeDescriptionPairList { get; }
		ICodeDescriptionPairList ClaimReasonCodeDescriptionPairList { get; }
		ICodeDescriptionPairList ClaimStatusCodeDescriptionPairList { get; }
		ICodeDescriptionPairList JobProfitLossReasonCodeDescriptionPairList { get; }
		ICodeDescriptionPairList GLPresentationJournalCategoriesList(Guid companyPK);
		string GetCategorisWithChildren(ZString categoryCode);
		ICodeDescriptionPairList GLPresentationJournalCategoriesGroupList { get; }
		ICodeDescriptionPairList CashFlowCategoryCodeDescriptionList { get; }
		ICodeDescriptionPairList ReversalReasonCodesList { get; }
		ICodeDescriptionPairList GoodsReceivedStatusCodesList { get; }
		ZGuid ProfitShareChargeCode { get; }
		Guid BSAccountStartAccount { get; }
		ZGuid ReportOrder_GLAccountSecondReportStartsFrom(ZString language, ZString country);
		string ReportOrder_AccountsOrderBeginsWith(ZString language, ZString country);
		bool PrintLogoOnCheque { get; }
		decimal CurrentPrimeRate { get; }
		bool ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(BusinessObject plugin);
		string AddJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistryItemLocation { get; }
		bool UseJobNumberBasedInvoiceNumbers { get; }
		Guid CustomsOther { get; }
		Guid CustomsImportOther { get; }
		Guid CustomsExWarehouse { get; }
		Guid CustomsImportAirUld { get; }
		Guid CustomsImportSeaFcl { get; }
		Guid CustomsImportSeaLcl { get; }
		Guid CustomsImportRail { get; }
		Guid CustomsImportRoad { get; }
		Guid CustomsImportPost { get; }
		Guid CustomsExportSeaFcl { get; }
		Guid CustomsExportSeaLcl { get; }
		Guid CustomsExportAirUld { get; }
		Guid CustomsExportRail { get; }
		Guid CustomsExportRoad { get; }
		Guid CustomsExportPost { get; }
		string DefaultPaymentType { get; }
		string[] GetCaptionsOfRegistryItemsUsingGLHeader(Guid gLHeaderPK);
		bool CollectConstructorCallStackDetails { get; }
		IRegistryItem IncludeUnpostedRevenueInCreditLimitCalculation { get; }
		IRegistryItem IncludeUnpostedRevenueInGlobalCreditLimitCalculation { get; }
		void SetStampDutyConfiguration(Guid companyPK, string taxIDs, decimal stampDutyFixedAmount, decimal stampDutyThreshold);
		void SetMainGSTTaxIDConfiguration(Guid companyPK, Guid taxID);
		void SetMainFreeGSTTaxIDConfiguration(Guid companyPK, Guid taxID);
		void SetMainGSTReverseTaxIDConfiguration(Guid companyPK, Guid taxID);
		void SetMainFreeGSTReverseTaxIDConfiguration(Guid companyPK, Guid taxID);
		void SetMainNotReportableTaxIDConfiguration(Guid companyPK, Guid taxID);
		void SetCASSFileImportDefaultTaxIDConfiguration(Guid companyPK, Guid standardRatedTaxID, Guid zeroRatedTaxID);
		void SetTaxIDsAttractingStampDutyForTransformation();
		void SetTaxIDsAttractingStampDuty(Guid companyPK, string taxIds);
		string TaxIDsAttractingStampDuty(Guid companyPK);
		decimal StampDutyFixedAmount(Guid companyPK);
		decimal StampDutyThreshold(Guid companyPK);
		bool WIPMustHaveDebtorCode(Guid companyPK);
		void SetWIPMustHaveDebtorCode(Guid companyPK, bool value);
		bool AccrualMustHaveCreditorCode(Guid companyPK);
		void SetAccrualMustHaveCreditorCode(Guid companyPK, bool value);
		bool IsInvoicePaymentWebServiceEnabled(Guid companyPK);
		bool UseWebServiceForCreditLimit { get; }
		bool IsMiscInvoiceInPeriodicInvoiceEnabled(Guid companyPK);
		bool TaxRecognitionDefaultingRules_IsOrganisationOverridePermitted(string ledger);
		ICreditControlledDocumentsCheckConfiguration[] GetCreditControlledDocumentsCheckConfiguration();
		ICreditControlledDocumentsCheckConfiguration[] GetGlobalCreditControlledDocumentsCheckConfiguration();
		bool JobInvoicingCFXEnabled(Guid companyPK);
		bool IsGLAccountUsedInSystemLevelRegistry(ZGuid glHeaderPk);
		bool IsNotAllowedForSeparateNumbering(ZGuid glHeaderPk);
		bool IsNotAllowedForDissectionAttributes(ZGuid glHeaderPk);
		bool IsNotAllowedForSeparateNumberingRegistry(IRegistryItem registryItem);
		bool IsNotAllowedForDissectionAttributesRegistry(IRegistryItem registryItem);
		bool IsGLAccountUsedInCompanyLevelRegistry(ZGuid glHeaderPk, ZGuid[] companyPks);
		int InvoiceNumberLength(Guid companyPk);
		int InvoiceNumberInNumericLength(Guid companyPk);
		bool HasSubAccounts(BusinessObject headerOrLine);
		string GetSubAccountsInfo(BusinessObject headerOrLine);
		Guid APControlAccount { get; }
		Guid ARControlAccount { get; }
		Guid GSTInputControlAccount { get; }
		Guid GSTOutputControlAccount { get; }
		Guid PendingGSTInputControlAccount { get; }
		Guid PendingGSTOutputControlAccount { get; }
		bool EnableBulkDisbursementJobsClosure { get; }
		bool GetIsExistDsbBatchByCharge(ZGuid chargePK);
		DateTime GetGenerateJournalEntriesStartDate(Guid companyPK);
		bool IsEPaymentFunctionalityEnabledForAnyProvider(Guid companyPK);
		bool IsEPaymentFunctionalityEnabledForOFX(Guid companyPK);
		bool IsIncludeCashAdvanceRequestsInCreditControlledDocumentEvaluationEnabled(Guid companyPK);
		bool IsIncludedInElectronicProcessingChargeConfiguration(ZDateTime jobOpenDate, string jobType);
		decimal GetGSTVATConversionExchangeRate(ZGuid transactionHeaderPK, ZGuid companyPK);
		string RSADecrypt(string cipherText);

#if DEBUG
		IDisposable SetupEnableEPaymentFunctionalityRegistry(Guid companyPK, bool isOFXEPaymentsEnabled);
		void AddReportOrder_AccountsOrderValue(ZString language, ZString country, ZString accountsOrderBeginsWith, ZGuid glAccountSecondReportStartsFrom);
		IDisposable SetupIncludeCashAdvanceRequestsInCreditControlledDocumentEvaluation(Guid companyPK, bool shouldIncludeCashAdvanceRequests);
		IDisposable SetupGLPresentationJournalCategoriesListRegistry(Guid companyPK, ZString categoryCode, ZString categoryDescription);
#endif
	}

	public interface IRegistry
	{
		IRegistryItem CreditorCreditLimitNotifyGroup { get; }
		IRegistryItem CreditLimitCheckTemporaryCreditLimitIncreaseThreshold { get; }
		IRegistryItem ARCreditControlledDocumentsApprovalNotifyGroup { get; }
		IRegistryItem IsNettingSystem { get; }
		IRegistryItem NettingControlAccount { get; }
		IRegistryItem NettingParticipationStartDate { get; }
		IRegistryItem NettingSystemOrganisation { get; }
		IRegistryItem ProfitShareChargeCode { get; }
		IRegistryItem JobDeactivationConfiguration { get; }
		IRegistryItem EnablePayablesInvoiceProcessingPortal { get; }
		IRegistryItem EnableImportingUniversalTransactionIntoPayableDraftInvoices { get; }
		string GetInvoicePostingExchangeRateOptionAR(ZGuid companyPK, bool isLocalInvoiceCurrencyType);
		string GetInvoicePostingExchangeRateOptionAP(ZGuid companyPK, bool isLocalInvoiceCurrencyType);
		IRegistryItem ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation { get; }
		IRegistryItem GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation { get; }
		ICodeDescriptionPairList NoteGLAccountsStatisticalUnitsofMeasurement(Guid companyPk);
		ICodeDescriptionPairList TaxMessageGroupsManagement(Guid companyPK);
		IRegistryItemInternals CurrencyAdjustmentExchangeGainAccount { get; }
		IRegistryItemInternals CurrencyAdjustmentExchangeLossAccount { get; }
		IRegistryItem EnableElectronicProcessingChargeFunctionality { get; }

#if DEBUG
		IRegistryItem JobBranchDefaultOrderRule { get; }
		IRegistryItem UseWebServiceForCreditLimit { get; }
		IRegistryItem UseWebServiceForOutstandingBalance { get; }
		IRegistryItem UseWebServiceForUnpostedRevenue { get; }
		IRegistryItem CreditLimitCheckWebServiceUrl { get; }
		IRegistryItem CreateWIPOrAccrualWhenNoInvoicesPosted_ForTestOnly { get; }
		IRegistryItem AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob_ForTestOnly { get; }
		IRegistryItem JobInvoicingCFXEnabled { get; }
		IRegistryItem ARSuspenseControlAccount_ForTestOnly { get; }
		IRegistryItem APSuspenseControlAccount_ForTestOnly { get; }
		IRegistryItem JobRevenueJournalControlAccount_ForTestOnly { get; }
		void SetInvoicePostingExchangeRateOptionAP(ZGuid companyPK, string option);
		void SetInvoicePostingExchangeRateOptionAR(ZGuid companyPK, string option);
#endif
	}

	public interface ICreditControlledDocumentsCheckConfiguration
	{
		ZString InvoiceType { get; }
		ZInt NumberOfDaysOverdue { get; }
		ZDecimal Amount { get; }
		ZString Range { get; }
		ZString AuthorisationRequirement { get; }
	}

	public interface IInvoiceRemittance
	{
		ZString InvoiceNumber { get; }
		ZString InvoiceNumberInNumeric { get; }
		ZString InvoiceTotalInLocalCurrency { get; }
		ZString InvoiceTotalInInvoiceCurrency { get; }
		ZString Message { get; }
		ZString BillerCode { get; }
		ZString BillerAccountNumber { get; }
		ZString DebtorOrganizationCode { get; }
		ZString DebtorClientNumber { get; }
		ZString InvoiceTransactionReference { get; }
	}

	public interface IComplianceNumberSequence
	{
		ZBool IsCorrected { get; }
		ZString ComplianceTransactionType { get; }
		ZString ComplianceSubType { get; }
		ZDateTime ComplianceDocumentDate { get; }
		ZDateTime InvoiceDate { get; }
		ZDateTime PostDate { get; }
		ZString GetMatchingComplianceSubType();
	}
}
