using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Environment;
using DeclarationApplicationCodeList = Enterprise.Customs.GB.Registry.Business.DeclarationApplicationCodeList;
using Eu = Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public abstract class ApplicationExtender
	{
		protected ApplicationExtender()
		{
		}

		public static ApplicationExtender New(string applicationCode)
		{
			var builders = ObjectFactory.Get<Hashtable>("GBApplicationExtenders");
			var result = GetExtender(applicationCode);
			return result ?? GetExtender(DeclarationApplicationCodeList.Codes.CHIEF);

			ApplicationExtender GetExtender(string code)
			{
				ApplicationExtender extender = null;
				var objectHandle = (ObjectHandle)builders[code];
				if (objectHandle != null)
				{
					extender = (ApplicationExtender)objectHandle.GetObject();
				}
				return extender;
			}
		}
		#region MOP
		public Eu.CusEntryLine.MoPLevel GetMoPDetailsLevel() => GetMoPDetailsLevelCore();
		protected abstract Eu.CusEntryLine.MoPLevel GetMoPDetailsLevelCore();
		public IEnumerable<ZString> GetDeferredMethodsOfPayment(CusEntryLine entryLine) => GetDeferredMethodsOfPaymentCore(entryLine);
		protected abstract IEnumerable<ZString> GetDeferredMethodsOfPaymentCore(CusEntryLine entryLine);
		public IEnumerable<ZString> GetGuaranteeDeferredMethodsOfPayment(CusEntryLine entryLine) => GetGuaranteeDeferredMethodsOfPaymentCore(entryLine);
		protected abstract IEnumerable<ZString> GetGuaranteeDeferredMethodsOfPaymentCore(CusEntryLine entryLine);

		#endregion

		internal CusEntryInstructionValidation GetNewCusEntryInstructionValidation(CusEntryInstruction cei) => GetNewCusEntryInstructionValidationCore(cei);
		protected abstract CusEntryInstructionValidation GetNewCusEntryInstructionValidationCore(CusEntryInstruction cei);

		internal IValueSetStrategy GetNewCusEntryInstructionValueSetStrategy(CusEntryInstruction cei) => GetNewCusEntryInstructionValueSetStrategyCore(cei);
		protected virtual IValueSetStrategy GetNewCusEntryInstructionValueSetStrategyCore(CusEntryInstruction cei) => null;

		internal JobDeclarationValidation GetNewJobDeclarationValidation(JobDeclaration declaration) => GetNewJobDeclarationValidationCore(declaration);
		protected abstract JobDeclarationValidation GetNewJobDeclarationValidationCore(JobDeclaration declaration);

		internal AdditionalInfoLookups GetNewAdditionalInfoLookups(MultiLineAddInfos.AdditionalInfo additionalInfo) => GetNewAdditionalInfoLookupsCore(additionalInfo);

		protected virtual AdditionalInfoLookups GetNewAdditionalInfoLookupsCore(MultiLineAddInfos.AdditionalInfo additionalInfo) => new AdditionalInfoLookups(additionalInfo);

		internal ZInt MaxNumberOfAdditionalProcedureCodes => MaxNumberOfAdditionalProcedureCodesCore;
		protected abstract ZInt MaxNumberOfAdditionalProcedureCodesCore { get; }

		public void DefaultLocationOfGoods(JobDeclaration declaration) => DefaultLocationOfGoodsCore(declaration);
		protected abstract void DefaultLocationOfGoodsCore(JobDeclaration declaration);

		internal Eu.JobComInvoiceLineValidation GetJobComInvoiceLineValidation(JobComInvoiceLine invoiceLine) => GetJobComInvoiceLineValidationCore(invoiceLine);
		protected abstract Eu.JobComInvoiceLineValidation GetJobComInvoiceLineValidationCore(JobComInvoiceLine invoiceLine);

		internal JobComInvoiceLineLookups GetJobComInvoiceLineLookups(JobComInvoiceLine invoiceLine) => GetJobComInvoiceLineLookupsCore(invoiceLine);
		protected virtual JobComInvoiceLineLookups GetJobComInvoiceLineLookupsCore(JobComInvoiceLine invoiceLine)
		{
			return new JobComInvoiceLineLookups(invoiceLine);
		}

		internal void SetDefaultsForNewChildOnInvoiceLine(JobDeclaration declaration, JobComInvoiceLine invoiceLine) => SetDefaultsForNewChildOnInvoiceLineCore(declaration, invoiceLine);
		protected virtual void SetDefaultsForNewChildOnInvoiceLineCore(JobDeclaration declaration, JobComInvoiceLine invoiceLine) { }

		public ZString GetJobComInvoiceLineDefaultMethodOfPayment(JobDeclaration declaration) => GetJobComInvoiceLineDefaultMethodOfPaymentCore(declaration);
		protected virtual ZString GetJobComInvoiceLineDefaultMethodOfPaymentCore(JobDeclaration declaration) => ZString.Empty;

		public EUAddInfoValidation GetAddInfoInvoiceLineValidation(AddInfoJobComInvoiceLine addInfo) => GetAddInfoInvoiceLineValidationCore(addInfo);
		protected abstract EUAddInfoValidation GetAddInfoInvoiceLineValidationCore(AddInfoJobComInvoiceLine addInfo);

		public CusCodeDataValidation GetFECChallengeValidation(CusCodeData fecData) => GetFECChallengeValidationCore(fecData);
		protected abstract CusCodeDataValidation GetFECChallengeValidationCore(CusCodeData fecData);

		internal TaxValidationHelper GetTaxValidationHelper(IEuTax parent, IEnumerable<IEuTax> allTaxes) => GetTaxValidationHelperCore(parent, allTaxes);
		protected abstract TaxValidationHelper GetTaxValidationHelperCore(IEuTax parent, IEnumerable<IEuTax> allTaxes);

		internal void DefaultLocationCountry(JobDeclaration declaration) => DefaultLocationCountryCore(declaration);
		protected abstract void DefaultLocationCountryCore(JobDeclaration declaration);

		DataTransferHelper dataTransferHelper;
		public DataTransferHelper DataTransferHelper => dataTransferHelper ?? (dataTransferHelper = GetDataTransferHelper());
		protected abstract DataTransferHelper GetDataTransferHelper();

		internal ZString GetEntryNumberType(JobDeclaration declaration) => GetEntryNumberTypeCore(declaration);
		protected abstract ZString GetEntryNumberTypeCore(JobDeclaration declaration);

		internal JiProcedureHelper GetInvoiceLineHelper(JobComInvoiceLine jobComInvoiceLine) => GetInvoiceLineHelperCore(jobComInvoiceLine);
		protected abstract JiProcedureHelper GetInvoiceLineHelperCore(JobComInvoiceLine jobComInvoiceLine);

		internal JobDeclarationValueSetStrategy GetValueStrategy(JobDeclaration declaration) => GetValueStrategyCore(declaration);
		protected abstract JobDeclarationValueSetStrategy GetValueStrategyCore(JobDeclaration declaration);

		internal CusEntryLineValidation GetCusEntryLineValidation(CusEntryLine cusEntryLine) => GetCusEntryLineValidationCore(cusEntryLine);
		protected abstract CusEntryLineValidation GetCusEntryLineValidationCore(CusEntryLine cusEntryLine);

		internal MergeManager GetMergeManager(JobDeclaration jobDec) => GetMergeManagerCore(jobDec);
		protected abstract MergeManager GetMergeManagerCore(JobDeclaration jobDec);

		internal ZString GuaranteeBondType => GuaranteeBondTypeCore;
		protected abstract ZString GuaranteeBondTypeCore { get; }

		internal ZString GetDefaultDataGroupingCode(JobDeclaration declaration) => GetDefaultDataGroupingCodeCore(declaration);
		protected abstract ZString GetDefaultDataGroupingCodeCore(JobDeclaration declaration);

		internal ZString GetDefaultDataGroupingCodeForTariffs(JobDeclaration declaration) => GetDefaultDataGroupingCodeForTariffsCore(declaration);
		protected abstract ZString GetDefaultDataGroupingCodeForTariffsCore(JobDeclaration declaration);

		internal ZString GetDefaultDataGroupingCodeForDutyRateCodes(JobDeclaration declaration) => GetDefaultDataGroupingCodeForDutyRateCodesCore(declaration);
		protected abstract ZString GetDefaultDataGroupingCodeForDutyRateCodesCore(JobDeclaration declaration);

		internal C88CreationStrategy CreateC88CreationStrategy(JobDeclaration declaration) => CreateC88CreationStrategyCore(declaration);
		protected abstract C88CreationStrategy CreateC88CreationStrategyCore(JobDeclaration declaration);

		public ZString GetNewEntryLocalReferenceNumber(BusinessObjectFactory factory) => GetNewEntryLocalReferenceNumberCore(factory);
		protected abstract ZString GetNewEntryLocalReferenceNumberCore(BusinessObjectFactory factory);

		UCCHelper uccHelper;
		internal UCCHelper UCCHelper => uccHelper ?? (uccHelper = GetUCCHelper());
		protected abstract UCCHelper GetUCCHelper();

		public string GetIApportionInvoiceHolderCountryContext(JobDeclaration declaration) => GetIApportionInvoiceHolderCountryContextCore(declaration);
		protected virtual string GetIApportionInvoiceHolderCountryContextCore(JobDeclaration declaration) => declaration.CountryCode;

		public ZString GetCDSChargeCode(BaseJobComInvHeaderCharge charge) => GetCDSChargeCodeCore(charge);
		protected abstract ZString GetCDSChargeCodeCore(BaseJobComInvHeaderCharge charge);

		internal JobComInvoiceLineValueSetStrategy GetJobComInvoiceLineValueSetStrategy(JobComInvoiceLine invoiceLine) => GetJobComInvoiceLineValueSetStrategyCore(invoiceLine);
		protected abstract JobComInvoiceLineValueSetStrategy GetJobComInvoiceLineValueSetStrategyCore(JobComInvoiceLine invoiceLine);

		internal AddInfoJobComInvoiceLineValueSetStrategy GetAddInfoJobComInvoiceLineValueSetStrategy(AddInfoJobComInvoiceLine addInfoJobComInvoiceLine) => GetAddInfoJobComInvoiceLineValueSetStrategyCore(addInfoJobComInvoiceLine);
		protected virtual AddInfoJobComInvoiceLineValueSetStrategy GetAddInfoJobComInvoiceLineValueSetStrategyCore(AddInfoJobComInvoiceLine addInfoJobComInvoiceLine) => new AddInfoJobComInvoiceLineValueSetStrategy(addInfoJobComInvoiceLine);

		internal CusEntryHeaderValidation GetCusEntryHeaderValidation(Eu.CusEntryHeader entryHeader) => GetCusEntryHeaderValidationCore(entryHeader);
		protected abstract CusEntryHeaderValidation GetCusEntryHeaderValidationCore(Eu.CusEntryHeader entryHeader);

		internal Eu.JobComInvoiceHeaderValidation GetJobComInvoiceHeaderValidation(Eu.JobComInvoiceHeader invoiceHeader) => GetJobComInvoiceHeaderValidationCore(invoiceHeader);
		protected abstract Eu.JobComInvoiceHeaderValidation GetJobComInvoiceHeaderValidationCore(Eu.JobComInvoiceHeader invoiceHeader);

		public GBAutoSendCustomsMessageProcessor GetEntryDeclarationMessageProcessor(JobDeclaration declaration)
		{
			return GetEntryDeclarationMessageProcessorCore(declaration);
		}

		protected abstract GBAutoSendCustomsMessageProcessor GetEntryDeclarationMessageProcessorCore(JobDeclaration declaration);

		public void SetDefaultValueForInvoiceLineTax(JobComInvoiceLineTax tax) => SetDefaultValueForInvoiceLineTaxCore(tax);

		protected virtual void SetDefaultValueForInvoiceLineTaxCore(JobComInvoiceLineTax tax) { }

		public CustomsOfficeRequirement GetMainOffice(JobDeclaration declaration)
		{
			return GetMainOfficeCore(declaration);
		}

		protected abstract CustomsOfficeRequirement GetMainOfficeCore(JobDeclaration declaration);

		public IEnumerable<CustomsOfficeRequirement> GetOtherCustomsOfficeRequirements(JobDeclaration declaration)
		{
			return GetOtherCustomsOfficeRequirementsCore(declaration);
		}
		protected virtual IEnumerable<CustomsOfficeRequirement> GetOtherCustomsOfficeRequirementsCore(JobDeclaration declaration) =>
			declaration.Factory.GetCachedValue("GB.JobDeclarationCustomsOfficeRequirementHelper.OtherRequirements." + declaration.JE_ApplicationCode, () => new List<CustomsOfficeRequirement>());

		public ZBool IsCDSChargeTypeItemLevel(BaseJobComInvHeaderCharge charge) => IsCDSChargeTypeItemLevelCore(charge);
		protected abstract ZBool IsCDSChargeTypeItemLevelCore(BaseJobComInvHeaderCharge charge);

		public BondedWarehouseMessageProcessor GetBondedWarehouseMessageProcessor(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<EmailDef, Enterprise.Messaging.Business.EDIMessage> sendMail, bool shouldCheckMessageStatus = false) => GetBondedWarehouseMessageProcessorCore(messagePK, emailReportThatHasBeenDelayed, sendMail, shouldCheckMessageStatus);

		protected abstract BondedWarehouseMessageProcessor GetBondedWarehouseMessageProcessorCore(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<EmailDef, Enterprise.Messaging.Business.EDIMessage> sendMail, bool shouldCheckMessageStatus = false);

		public IList<PermitRecord> GetPermitRecords(CusEntryHeader entryHeader) => GetPermitRecordsCore(entryHeader);

		protected abstract IList<PermitRecord> GetPermitRecordsCore(CusEntryHeader entryHeader);

		public ZString GetComplementaryJobPreviousDocumentCodeType(ZString defaultValue) => GetComplementaryJobPreviousDocumentCodeTypeCore(defaultValue);

		protected abstract ZString GetComplementaryJobPreviousDocumentCodeTypeCore(ZString defaultValue);

		public Customs.Business.CusEntryLineFeeValidation GetCusEntryLineFeeValidation(Eu.AutoCusEntryLineFee entryLineFee, Func<Customs.Business.CusEntryLineFeeValidation> defaultValidationAction) => GetCusEntryLineFeeValidationCore(entryLineFee, defaultValidationAction);

		protected abstract Customs.Business.CusEntryLineFeeValidation GetCusEntryLineFeeValidationCore(Eu.AutoCusEntryLineFee entryLineFee, Func<Customs.Business.CusEntryLineFeeValidation> defaultValidationAction);

		public ZString GetEntryHeaderStatusDescription(ZString entryStatus, ZString fallbackValue) => GetEntryHeaderStatusDescriptionCore(entryStatus, fallbackValue);

		protected abstract ZString GetEntryHeaderStatusDescriptionCore(ZString entryStatus, ZString fallbackValue);

		public ZString GetOldEntryStatus(CusEntryHeader entryHeader) => GetOldEntryStatusCore(entryHeader);

		protected abstract ZString GetOldEntryStatusCore(CusEntryHeader entryHeader);

		public ZString GetEntryStatus(CusEntryHeader entryHeader) => GetEntryStatusCore(entryHeader);

		protected abstract ZString GetEntryStatusCore(CusEntryHeader entryHeader);

		public EntryLineVatCalculator GetEntryLineVatCalculator(CusEntryLine entryLine, Func<CusEntryLine, EntryLineVatCalculator> baseFunctionality) => GetEntryLineVatCalculatorCore(entryLine, baseFunctionality);

		protected abstract EntryLineVatCalculator GetEntryLineVatCalculatorCore(CusEntryLine entryLine, Func<CusEntryLine, EntryLineVatCalculator> baseFunctionality);

		public Customs.Common.IApportionStrategy GetApportionStrategy() => GetApportionStrategyCore();
		protected virtual Customs.Common.IApportionStrategy GetApportionStrategyCore() => new Customs.Common.ApportionStrategy();

		public string DeferredFeeMethodOfPaymentCode => DeferredFeeMethodOfPaymentCodeCore;
		public IFeeRounder ChargeAmountNoRounder => GetChargeAmountNoRounderCore();
		protected virtual IFeeRounder GetChargeAmountNoRounderCore() => new FeeNoRounder();

		protected abstract string DeferredFeeMethodOfPaymentCodeCore { get; }

		public ZDecimal GetGSTRate(CusEntryLine entryLine, ZDecimal baseValue) => GetGSTRateCore(entryLine, baseValue);

		protected abstract ZDecimal GetGSTRateCore(CusEntryLine entryLine, ZDecimal baseValue);

		public ZString GetDutyRateDescription(CusEntryLine entryLine, ZString baseValue) => GetDutyRateDescriptionCore(entryLine, baseValue);

		protected abstract ZString GetDutyRateDescriptionCore(CusEntryLine entryLine, ZString baseValue);

		public ZString GetDutyAmountsAsString(JobComInvoiceLine invoiceLine, Func<ZString> baseFunctionality) => GetDutyAmountsAsStringCore(invoiceLine, baseFunctionality);

		protected abstract ZString GetDutyAmountsAsStringCore(JobComInvoiceLine invoiceLine, Func<ZString> baseFunctionality);

		public void AfterUniversalCopy(JobDeclaration declaration) => AfterUniversalCopyCore(declaration);

		protected abstract void AfterUniversalCopyCore(JobDeclaration declaration);

		public string GetFeeCodeFromRateCode(CusEntryLine entryLine, string rateCode) => GetFeeCodeFromRateCodeCore(entryLine, rateCode);
		protected virtual string GetFeeCodeFromRateCodeCore(CusEntryLine entryLine, string rateCode) => rateCode;

		public ISet<ZString> GetListOfStatusCodesForAutoBilling(JobDeclaration declaration) => listOfStatusCodesForAutoBilling ?? (listOfStatusCodesForAutoBilling = GetListOfStatusCodesForAutoBillingCore(declaration));
		ISet<ZString> listOfStatusCodesForAutoBilling;

		protected abstract ISet<ZString> GetListOfStatusCodesForAutoBillingCore(JobDeclaration declaration);

		public ZString GetDefaultMethodOfPaymentValue(CusEntryLineFee fee) => GetDefaultMethodOfPaymentValueCore(fee);

		protected abstract ZString GetDefaultMethodOfPaymentValueCore(CusEntryLineFee fee);

		public FiscalRepresentativeDefaulter GetFiscalRepresentativeDefaulter() => GetFiscalRepresentativeDefaulterCore();
		protected abstract FiscalRepresentativeDefaulter GetFiscalRepresentativeDefaulterCore();
	}
}
