using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public abstract class ApplicationExtender
	{
		protected ApplicationExtender()
		{
		}

		public static ApplicationExtender New(string applicationCode)
		{
			ApplicationExtender result;
			switch (applicationCode)
			{
				case DeclarationApplicationCodeList.Codes.DeltaG:
				case DeclarationApplicationCodeList.Codes.Interface:
				case Customs.Business.DeclarationApplicationCodeList.Codes.Builtin:
					result = new DeltaGApplicationExtender();
					break;
				case DeclarationApplicationCodeList.Codes.DeltaIE:
					result = new DeltaIEApplicationExtender();
					break;
				default:
					result = new DeltaGApplicationExtender();
					break;
			}

			return result;
		}

		public IValueSetStrategy GetJobDeclarationValueSetStrategy(JobDeclaration declaration) => GetJobDeclarationValueSetStrategyCore(declaration);
		protected abstract IValueSetStrategy GetJobDeclarationValueSetStrategyCore(JobDeclaration declaration);

		public IValueSetStrategy GetCusEntryInstructionValueSetStrategy(CusEntryInstruction entryInstruction) => GetCusEntryInstructionValueSetStrategyCore(entryInstruction);
		protected abstract IValueSetStrategy GetCusEntryInstructionValueSetStrategyCore(CusEntryInstruction entryInstruction);

		public IValueSetStrategy GetCusAuthorizationUsageValueSetStrategy(CusAuthorizationUsage authorizationUsage) => GetCusAuthorizationUsageValueSetStrategyCore(authorizationUsage);
		protected abstract IValueSetStrategy GetCusAuthorizationUsageValueSetStrategyCore(CusAuthorizationUsage authorizationUsage);

		public EU.Business.Declaration.VATDeferStrategy GetVATDeferStrategy(JobDeclaration declaration) => GetVATDeferStrategyCore(declaration);
		protected abstract EU.Business.Declaration.VATDeferStrategy GetVATDeferStrategyCore(JobDeclaration declaration);

		public bool IsUCC6 => IsUCC6Core;
		protected abstract bool IsUCC6Core { get; }

		public ZString AmendmentSnapshotMessageType => AmendmentSnapshotMessageTypeCore;
		protected abstract ZString AmendmentSnapshotMessageTypeCore { get; }

		public ZString GetEffectiveCountryOfOrigin(JobComInvoiceLine invoiceLine) => GetEffectiveCountryOfOriginCore(invoiceLine);
		protected abstract ZString GetEffectiveCountryOfOriginCore(JobComInvoiceLine invoiceLine);

		public ZString GetDataGroupingForCusProcedure(JobDeclaration declaration) => GetDataGroupingForCusProcedureCore(declaration);
		protected abstract ZString GetDataGroupingForCusProcedureCore(JobDeclaration declaration);

		public ZString GetDataGroupingForAdditionalDocumentCodes(JobDeclaration declaration) => GetDataGroupingForAdditionalDocumentCodesCore(declaration);
		protected abstract ZString GetDataGroupingForAdditionalDocumentCodesCore(JobDeclaration declaration);

		public CodeDescriptionPairList GetDefinedDeclarationTypeList(JobDeclaration declaration) => GetDefinedDeclarationTypeListCore(declaration);
		protected abstract CodeDescriptionPairList GetDefinedDeclarationTypeListCore(JobDeclaration declaration);

		internal JobDeclarationValidation GetNewJobDeclarationValidation(JobDeclaration declaration) => GetNewJobDeclarationValidationCore(declaration);
		protected abstract JobDeclarationValidation GetNewJobDeclarationValidationCore(JobDeclaration declaration);

		internal JobDeclarationLookups GetNewJobDeclarationLookups(JobDeclaration declaration) => GetNewJobDeclarationLookupsCore(declaration);
		protected abstract JobDeclarationLookups GetNewJobDeclarationLookupsCore(JobDeclaration declaration);

		public AdditionalInfoValidation GetAdditionalInfoValidation(AdditionalInfo additionalInfo) => GetAdditionalInfoValidationCore(additionalInfo);
		protected abstract AdditionalInfoValidation GetAdditionalInfoValidationCore(AdditionalInfo additionalInfo);

		internal VATNumberSupporter GetVATNumberSupporter(JobDeclaration declaration) => GetVATNumberSupporterCore(declaration);
		protected abstract VATNumberSupporter GetVATNumberSupporterCore(JobDeclaration declaration);

		internal OrgCusAccount GetCustomsProfileRelatedAccount(JobDeclaration declaration) => GetCustomsProfileRelatedAccountCore(declaration);
		protected abstract OrgCusAccount GetCustomsProfileRelatedAccountCore(JobDeclaration declaration);

		public JobComInvoiceHeaderValidation GetJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader) => GetJobComInvoiceHeaderValidationCore(invoiceHeader);
		protected abstract JobComInvoiceHeaderValidation GetJobComInvoiceHeaderValidationCore(JobComInvoiceHeader invoiceHeader);

		public AddInfoCusEntryInstructionLookups GetAddInfoCusEntryInstructionLookups(AddInfoCusEntryInstruction addCusEntryInstruction) => GetAddInfoCusEntryInstructionLookupsCore(addCusEntryInstruction);
		protected abstract AddInfoCusEntryInstructionLookups GetAddInfoCusEntryInstructionLookupsCore(AddInfoCusEntryInstruction addCusEntryInstruction);

		public JobComInvoiceHeaderLookups GetJobComInvoiceHeaderLookups(JobComInvoiceHeader invoiceHeader) => GetJobComInvoiceHeaderLookupsCore(invoiceHeader);
		protected abstract JobComInvoiceHeaderLookups GetJobComInvoiceHeaderLookupsCore(JobComInvoiceHeader invoiceHeader);

		public IValueSetStrategy GetJobComInvoiceHeaderValueSetStrategy(JobComInvoiceHeader invoiceHeader) => GetJobComInvoiceHeaderValueSetStrategyCore(invoiceHeader);
		protected abstract IValueSetStrategy GetJobComInvoiceHeaderValueSetStrategyCore(JobComInvoiceHeader invoiceHeader);

		public IReadOnlyStrategy GetJobComInvoiceHeaderReadOnlyStrategy(JobComInvoiceHeader invoiceHeader) => GetJobComInvoiceHeaderReadOnlyStrategyCore(invoiceHeader);
		protected abstract IReadOnlyStrategy GetJobComInvoiceHeaderReadOnlyStrategyCore(JobComInvoiceHeader invoiceHeader);

		public IValuePostProcessingStrategy GetJobComInvoiceHeaderValuePostProcessingStrategy(JobComInvoiceHeader invoiceHeader) => GetJobComInvoiceHeaderValuePostProcessingStrategyCore(invoiceHeader);
		protected abstract IValuePostProcessingStrategy GetJobComInvoiceHeaderValuePostProcessingStrategyCore(JobComInvoiceHeader invoiceHeader);

		public JobComInvoiceLineValueSetStrategy GetJobComInvoiceLineValueSetStrategy(JobComInvoiceLine invoiceLine) => GetJobComInvoiceLineValueSetStrategyCore(invoiceLine);
		protected abstract JobComInvoiceLineValueSetStrategy GetJobComInvoiceLineValueSetStrategyCore(JobComInvoiceLine invoiceLine);

		public JobComInvoiceLineValidation GetJobComInvoiceLineValidation(JobComInvoiceLine invoiceLine) => GetJobComInvoiceLineValidationCore(invoiceLine);
		protected abstract JobComInvoiceLineValidation GetJobComInvoiceLineValidationCore(JobComInvoiceLine invoiceLine);

		public JobComInvoiceLineLookups GetJobComInvoiceLineLookups(JobComInvoiceLine invoiceLine) => GetJobComInvoiceLineLookupsCore(invoiceLine);
		protected abstract JobComInvoiceLineLookups GetJobComInvoiceLineLookupsCore(JobComInvoiceLine invoiceLine);

		public CusEntryInstructionValidation GetCusEntryInstructionValidation(CusEntryInstruction instruction) => GetCusEntryInstructionValidationCore(instruction);
		protected abstract CusEntryInstructionValidation GetCusEntryInstructionValidationCore(CusEntryInstruction instruction);

		public CusEntryInstructionLookups GetCusEntryInstructionLookups(CusEntryInstruction instruction) => GetCusEntryInstructionLookupsCore(instruction);
		protected abstract CusEntryInstructionLookups GetCusEntryInstructionLookupsCore(CusEntryInstruction instruction);

		public CusAuthorizationUsageLookups GetCusAuthorizationUsageLookups(CusAuthorizationUsage cusAuthorizationUsage) => GetCusAuthorizationUsageLookupsCore(cusAuthorizationUsage);
		protected abstract CusAuthorizationUsageLookups GetCusAuthorizationUsageLookupsCore(CusAuthorizationUsage cusAuthorizationUsage);

		public AddInfoJobComInvoiceHeaderValidation GetAddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader addInfoJobComInvoiceHeader) => GetAddInfoJobComInvoiceHeaderValidationCore(addInfoJobComInvoiceHeader);
		protected abstract AddInfoJobComInvoiceHeaderValidation GetAddInfoJobComInvoiceHeaderValidationCore(AddInfoJobComInvoiceHeader addInfoJobComInvoiceHeader);

		public AddInfoCusEntryInstructionValidation GetAddInfoCusEntryInstructionValidation(AddInfoCusEntryInstruction addInfoCusEntryInstruction) => GetAddInfoCusEntryInstructionValidationCore(addInfoCusEntryInstruction);
		protected abstract AddInfoCusEntryInstructionValidation GetAddInfoCusEntryInstructionValidationCore(AddInfoCusEntryInstruction addInfoCusEntryInstruction);

		public ZString GetEntryStatusDescription(JobDeclaration declaration, ZString entryStatus) => GetEntryStatusDescriptionCore(declaration, entryStatus);
		protected abstract ZString GetEntryStatusDescriptionCore(JobDeclaration declaration, ZString entryStatus);

		public ZString ReCalculateStatusDetails(CusEntryHeader entryHeader) => ReCalculateStatusDetailsCore(entryHeader);
		protected abstract ZString ReCalculateStatusDetailsCore(CusEntryHeader entryHeader);

		public bool CanBeRevertedToLastBAE(CusEntryHeader entryHeader) => CanBeRevertedToLastBAECore(entryHeader);
		protected abstract bool CanBeRevertedToLastBAECore(CusEntryHeader entryHeader);

		public bool IsEntryInstructionOutOfInward(CusEntryInstruction instruction) => IsEntryInstructionOutOfInwardCore(instruction);
		protected abstract bool IsEntryInstructionOutOfInwardCore(CusEntryInstruction instruction);

		public ZString GetCorrelationIDPrefix() => GetCorrelationIDPrefixCore();
		protected abstract ZString GetCorrelationIDPrefixCore();

		public bool IsEntryStatusCleared(CusEntryHeader entry) => IsEntryStatusClearedCore(entry);
		protected abstract bool IsEntryStatusClearedCore(CusEntryHeader entry);

		public IEnumerable<OrgCusAccount> GetDeltaAccounts(JobDeclaration declaration) => GetDeltaAccountsCore(declaration);
		protected abstract IEnumerable<OrgCusAccount> GetDeltaAccountsCore(JobDeclaration declaration);

		public CusEntryLineFeeLookups GetCusEntryLineFeeLookups(CusEntryLineFee fee) => GetCusEntryLineFeeLookupsCore(fee);
		protected abstract CusEntryLineFeeLookups GetCusEntryLineFeeLookupsCore(CusEntryLineFee fee);
	}
}
