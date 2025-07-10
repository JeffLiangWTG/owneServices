using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIEApplicationExtender : ApplicationExtender
	{
		protected override IValueSetStrategy GetJobDeclarationValueSetStrategyCore(JobDeclaration declaration)
		{
			return new DeltaIEJobDeclarationValueSetStrategy(declaration);
		}

		protected override IValueSetStrategy GetCusEntryInstructionValueSetStrategyCore(CusEntryInstruction entryInstruction)
		{
			return new DeltaIECusEntryInstructionValueSetStrategy(entryInstruction);
		}

		protected override IValueSetStrategy GetCusAuthorizationUsageValueSetStrategyCore(CusAuthorizationUsage authorizationUsage)
		{
			return new DeltaIECusAuthorizationUsageValueSetStrategy(authorizationUsage);
		}

		protected override CodeDescriptionPairList GetDefinedDeclarationTypeListCore(JobDeclaration declaration)
		{
			var descriptions = new CodeDescriptionPairList();
			if (declaration.IsImport)
			{
				descriptions = new DeltaIEImportDeclarationTypeList();
			}
			else if (declaration.IsExport)
			{
				descriptions = new DeltaIEExportDeclarationTypeList();
			}

			return descriptions;
		}

		protected override bool IsUCC6Core => true;

		protected override ZString AmendmentSnapshotMessageTypeCore => DeclarationApplicationCodeList.Codes.DeltaIE;

		protected override ZString GetEffectiveCountryOfOriginCore(JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine.IsImport && !invoiceLine.ZG_CountryOfSupply.IsEmpty)
			{
				return invoiceLine.ZG_CountryOfSupply;
			}
			return ZString.Empty;
		}

		protected override ZString GetDataGroupingForCusProcedureCore(JobDeclaration declaration)
		{
			return Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE;
		}

		protected override ZString GetDataGroupingForAdditionalDocumentCodesCore(JobDeclaration declaration)
		{
			return Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE;
		}

		protected override JobDeclarationValidation GetNewJobDeclarationValidationCore(JobDeclaration declaration)
		{
			return new DeltaIEJobDeclarationValidation(declaration);
		}

		protected override JobDeclarationLookups GetNewJobDeclarationLookupsCore(JobDeclaration declaration)
		{
			return new DeltaIEJobDeclarationLookups(declaration);
		}

		protected override AdditionalInfoValidation GetAdditionalInfoValidationCore(AdditionalInfo additionalInfo)
		{
			return new DeltaIEAdditionalInfoValidation(additionalInfo);
		}

		protected override VATNumberSupporter GetVATNumberSupporterCore(JobDeclaration declaration)
		{
			return new DeltaIEVATNumberSupporter(declaration);
		}

		protected override OrgCusAccount GetCustomsProfileRelatedAccountCore(JobDeclaration declaration)
		{
			return declaration.JE_CustomsProfile.IsEmpty ? null : declaration.DeltaAccounts.FirstOrDefault(x => x.CZ_Account == declaration.JE_CustomsProfile);
		}

		protected override JobComInvoiceHeaderValidation GetJobComInvoiceHeaderValidationCore(JobComInvoiceHeader invoiceHeader)
		{
			return new DeltaIEJobComInvoiceHeaderValidation(invoiceHeader);
		}

		protected override AddInfoCusEntryInstructionLookups GetAddInfoCusEntryInstructionLookupsCore(AddInfoCusEntryInstruction addCusEntryInstruction)
		{
			return new DeltaIEAddInfoCusEntryInstructionLookups(addCusEntryInstruction);
		}

		protected override JobComInvoiceHeaderLookups GetJobComInvoiceHeaderLookupsCore(JobComInvoiceHeader invoiceHeader)
		{
			return new DeltaIEJobComInvoiceHeaderLookups(invoiceHeader);
		}

		protected override JobComInvoiceLineValueSetStrategy GetJobComInvoiceLineValueSetStrategyCore(JobComInvoiceLine invoiceLine)
		{
			return new DeltaIEJobComInvoiceLineValueSetStrategy(invoiceLine);
		}

		protected override JobComInvoiceLineValidation GetJobComInvoiceLineValidationCore(JobComInvoiceLine invoiceLine)
		{
			return new DeltaIEJobComInvoiceLineValidation(invoiceLine);
		}

		protected override JobComInvoiceLineLookups GetJobComInvoiceLineLookupsCore(JobComInvoiceLine invoiceLine)
		{
			return new DeltaIEJobComInvoiceLineLookups(invoiceLine);
		}

		protected override CusEntryInstructionValidation GetCusEntryInstructionValidationCore(CusEntryInstruction instruction)
		{
			return new CusEntryInstructionValidation(instruction);
		}

		protected override CusEntryInstructionLookups GetCusEntryInstructionLookupsCore(CusEntryInstruction instruction)
		{
			return new DeltaIECusEntryInstructionLookups(instruction);
		}

		protected override CusAuthorizationUsageLookups GetCusAuthorizationUsageLookupsCore(CusAuthorizationUsage cusAuthorizationUsage)
		{
			return new DeltaIECusAuthorizationUsageLookups(cusAuthorizationUsage);
		}

		protected override AddInfoJobComInvoiceHeaderValidation GetAddInfoJobComInvoiceHeaderValidationCore(AddInfoJobComInvoiceHeader addInfoJobComInvoiceHeader)
		{
			return new DeltaIEAddInfoJobComInvoiceHeaderValidation(addInfoJobComInvoiceHeader);
		}

		protected override AddInfoCusEntryInstructionValidation GetAddInfoCusEntryInstructionValidationCore(AddInfoCusEntryInstruction addInfoCusEntryInstruction)
		{
			return new AddInfoCusEntryInstructionValidation(addInfoCusEntryInstruction);
		}

		protected override IValueSetStrategy GetJobComInvoiceHeaderValueSetStrategyCore(JobComInvoiceHeader invoiceHeader)
		{
			return new DeltaIEJobComInvoiceHeaderValueSetStrategy(invoiceHeader);
		}

		protected override IReadOnlyStrategy GetJobComInvoiceHeaderReadOnlyStrategyCore(JobComInvoiceHeader invoiceHeader)
		{
			return new DeltaIEJobComInvoiceHeaderReadOnlyStrategy(invoiceHeader);
		}

		protected override IValuePostProcessingStrategy GetJobComInvoiceHeaderValuePostProcessingStrategyCore(JobComInvoiceHeader invoiceHeader)
		{
			return new DeltaIEJobComInvoiceHeaderValuePostProcessingStrategy(invoiceHeader);
		}

		protected override ZString GetEntryStatusDescriptionCore(JobDeclaration declaration, ZString entryStatus)
		{
			return declaration.IsImport ? declaration.Factory.GetCachedValue<DeltaIEImportCusEntryStatusList>().GetDescriptionFromCode(entryStatus)
							: declaration.Factory.GetCachedValue<EntryStatusDescriptionCodeList>().GetDescriptionFromCode(entryStatus);
		}

		protected override ZString ReCalculateStatusDetailsCore(CusEntryHeader entryHeader)
		{
			return entryHeader.IsImport ? string.Empty : EntryStatusDescriptionCodeList.Codes.ES010;
		}

		protected override bool CanBeRevertedToLastBAECore(CusEntryHeader entryHeader) => false;

		protected override EU.Business.Declaration.VATDeferStrategy GetVATDeferStrategyCore(JobDeclaration declaration) => new DeltaIEVATDeferStrategy(declaration);

		protected override bool IsEntryInstructionOutOfInwardCore(CusEntryInstruction instruction) => false;

		protected override ZString GetCorrelationIDPrefixCore() => GlbCompany.CurrentCompany.GetLicenceCode();

		protected override bool IsEntryStatusClearedCore(CusEntryHeader entry) => entry.CH_EntryStatus == DeltaIEImportCusEntryStatusList.Codes.Released;

		protected override IEnumerable<OrgCusAccount> GetDeltaAccountsCore(JobDeclaration declaration)
		{
			var deltaAccountCode = OrgCusAccountCodeList.Codes.DEC;
			var representative = declaration.Representative;
			var orgHeaders = declaration.ActualClientAndDeclarant;
			if (representative?.Header != null)
			{
				orgHeaders = orgHeaders.Append(representative.Header);
			}
			return orgHeaders.SelectMany(x => OrgCusAccount.Loader.LoadByCodeAndCountry(declaration.Factory, x.PK, deltaAccountCode, Core.Constants.CountryCodes.France)).Distinct();
		}

		protected override CusEntryLineFeeLookups GetCusEntryLineFeeLookupsCore(CusEntryLineFee fee) => new DeltaIECusEntryLineFeeLookups(fee);
	}
}
