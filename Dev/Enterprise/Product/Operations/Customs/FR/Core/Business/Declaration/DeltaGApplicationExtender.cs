using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	class DeltaGApplicationExtender : ApplicationExtender
	{
		protected override IValueSetStrategy GetJobDeclarationValueSetStrategyCore(JobDeclaration declaration)
		{
			return new DeltaGJobDeclarationValueSetStrategy(declaration);
		}

		protected override IValueSetStrategy GetCusEntryInstructionValueSetStrategyCore(CusEntryInstruction entryInstruction)
		{
			return new CusEntryInstructionValueSetStrategy(entryInstruction);
		}

		protected override IValueSetStrategy GetCusAuthorizationUsageValueSetStrategyCore(CusAuthorizationUsage authorizationUsage)
		{
			return new CusAuthorizationUsageValueSetStrategy(authorizationUsage);
		}

		protected override CodeDescriptionPairList GetDefinedDeclarationTypeListCore(JobDeclaration declaration)
		{
			var descriptions = new CodeDescriptionPairList();
			if (declaration.IsImport)
			{
				descriptions = new DeltaGImportDeclarationTypeList();
			}
			else if (declaration.IsExport)
			{
				descriptions = new DeltaGExportDeclarationTypeList();
			}

			return descriptions;
		}

		protected override bool IsUCC6Core => false;

		protected override ZString AmendmentSnapshotMessageTypeCore => DeclarationApplicationCodeList.Codes.DeltaG;

		protected override ZString GetEffectiveCountryOfOriginCore(JobComInvoiceLine invoiceLine)
		{
			return ZString.Empty;
		}

		protected override ZString GetDataGroupingForCusProcedureCore(JobDeclaration declaration)
		{
			return ZString.Empty;
		}

		protected override ZString GetDataGroupingForAdditionalDocumentCodesCore(JobDeclaration declaration)
		{
			return ZString.Empty;
		}

		protected override JobDeclarationValidation GetNewJobDeclarationValidationCore(JobDeclaration declaration)
		{
			return new DeltaGJobDeclarationValidation(declaration);
		}

		protected override JobDeclarationLookups GetNewJobDeclarationLookupsCore(JobDeclaration declaration)
		{
			return new DeltaGJobDeclarationLookups(declaration);
		}

		protected override AdditionalInfoValidation GetAdditionalInfoValidationCore(AdditionalInfo additionalInfo)
		{
			return new AdditionalInfoValidation(additionalInfo);
		}

		protected override VATNumberSupporter GetVATNumberSupporterCore(JobDeclaration declaration)
		{
			return new DeltaGVATNumberSupporter(declaration);
		}

		protected override OrgCusAccount GetCustomsProfileRelatedAccountCore(JobDeclaration declaration)
		{
			return declaration.JE_CustomsProfile.IsEmpty ? null : declaration.DeltaAccounts.FirstOrDefault(x => x.CZ_Account == declaration.JE_CustomsProfile && x.CZ_Type == declaration.JE_DeltaMode);
		}

		protected override JobComInvoiceHeaderValidation GetJobComInvoiceHeaderValidationCore(JobComInvoiceHeader invoiceHeader)
		{
			return new DeltaGJobComInvoiceHeaderValidation(invoiceHeader);
		}

		protected override AddInfoCusEntryInstructionLookups GetAddInfoCusEntryInstructionLookupsCore(AddInfoCusEntryInstruction addCusEntryInstruction)
		{
			return new DeltaGAddInfoCusEntryInstructionLookups(addCusEntryInstruction);
		}

		protected override JobComInvoiceHeaderLookups GetJobComInvoiceHeaderLookupsCore(JobComInvoiceHeader invoiceHeader)
		{
			return new DeltaGJobComInvoiceHeaderLookups(invoiceHeader);
		}

		protected override JobComInvoiceLineValueSetStrategy GetJobComInvoiceLineValueSetStrategyCore(JobComInvoiceLine invoiceLine)
		{
			return new JobComInvoiceLineValueSetStrategy(invoiceLine);
		}

		protected override JobComInvoiceLineValidation GetJobComInvoiceLineValidationCore(JobComInvoiceLine invoiceLine)
		{
			return new DeltaGJobComInvoiceLineValidation(invoiceLine);
		}

		protected override JobComInvoiceLineLookups GetJobComInvoiceLineLookupsCore(JobComInvoiceLine invoiceLine)
		{
			return new JobComInvoiceLineLookups(invoiceLine);
		}

		protected override CusEntryInstructionValidation GetCusEntryInstructionValidationCore(CusEntryInstruction instruction)
		{
			return new DeltaGCusEntryInstructionValidation(instruction);
		}

		protected override CusEntryInstructionLookups GetCusEntryInstructionLookupsCore(CusEntryInstruction instruction)
		{
			return new CusEntryInstructionLookups(instruction);
		}

		protected override CusAuthorizationUsageLookups GetCusAuthorizationUsageLookupsCore(CusAuthorizationUsage cusAuthorizationUsage)
		{
			return new DeltaGCusAuthorizationUsageLookups(cusAuthorizationUsage);
		}

		protected override AddInfoJobComInvoiceHeaderValidation GetAddInfoJobComInvoiceHeaderValidationCore(AddInfoJobComInvoiceHeader addInfoJobComInvoiceHeader)
		{
			return new DeltaGAddInfoJobComInvoiceHeaderValidation(addInfoJobComInvoiceHeader);
		}

		protected override AddInfoCusEntryInstructionValidation GetAddInfoCusEntryInstructionValidationCore(AddInfoCusEntryInstruction addInfoCusEntryInstruction)
		{
			return new DeltaGAddInfoCusEntryInstructionValidation(addInfoCusEntryInstruction);
		}

		protected override IValueSetStrategy GetJobComInvoiceHeaderValueSetStrategyCore(JobComInvoiceHeader invoiceHeader)
		{
			return new DeltaGJobComInvoiceHeaderValueSetStrategy(invoiceHeader);
		}

		protected override IReadOnlyStrategy GetJobComInvoiceHeaderReadOnlyStrategyCore(JobComInvoiceHeader invoiceHeader)
		{
			return null;
		}

		protected override IValuePostProcessingStrategy GetJobComInvoiceHeaderValuePostProcessingStrategyCore(JobComInvoiceHeader invoiceHeader)
		{
			return new DeltaGJobComInvoiceHeaderValuePostProcessingStrategy(invoiceHeader);
		}

		protected override ZString GetEntryStatusDescriptionCore(JobDeclaration declaration, ZString entryStatus)
		{
			return declaration.Factory.GetCachedValue<EntryStatusDescriptionCodeList>().GetDescriptionFromCode(entryStatus);
		}

		protected override ZString ReCalculateStatusDetailsCore(CusEntryHeader entryHeader)
		{
			return EntryStatusDescriptionCodeList.Codes.ES010;
		}

		protected override bool CanBeRevertedToLastBAECore(CusEntryHeader entryHeader) => entryHeader.Messages.Cast<FREDIMessage>().Any(message => message.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Receive && new DeltaGStatusResolver(entryHeader, message).CheckIsRectificationError());

		protected override EU.Business.Declaration.VATDeferStrategy GetVATDeferStrategyCore(JobDeclaration declaration) => new DeltaGVATDeferStrategy(declaration);

		protected override bool IsEntryInstructionOutOfInwardCore(CusEntryInstruction instruction) => instruction.CEI_Style == DeltaGExportDeclarationTypeList.Codes.ReExportOfNonUnionGoodsWithEI;

		protected override ZString GetCorrelationIDPrefixCore() => ZString.Empty;

		protected override bool IsEntryStatusClearedCore(CusEntryHeader entry) => false;

		protected override IEnumerable<OrgCusAccount> GetDeltaAccountsCore(JobDeclaration declaration) => declaration.ActualClientAndDeclarant.SelectMany(x => OrgCusAccount.Loader.LoadByCodeAndCountry(declaration.Factory, x.PK, declaration.DeltaGAccountCode, Core.Constants.CountryCodes.France)).Distinct();

		protected override CusEntryLineFeeLookups GetCusEntryLineFeeLookupsCore(CusEntryLineFee fee) => new DeltaGCusEntryLineFeeLookups(fee);
	}
}
