using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration;

public class ImportCusFiscalReferenceLookups : UCC6ImportCusFiscalReferenceLookups
{
	public ImportCusFiscalReferenceLookups(CusFiscalReference parent)
		: base(parent)
	{
	}

	public override CodeDescriptionPairList CodeList => GetCodeList(Parent);

	static CodeDescriptionPairList GetCodeList(CusFiscalReference reference)
	{
		var result = new CodeDescriptionPairList();
		var style = reference?.Parent is JobComInvoiceLine invoiceLine
			? (invoiceLine.EntryInstruction?.CEI_Style ?? ZString.Empty)
			: (reference?.Instruction?.CEI_Style ?? ZString.Empty);
		if (style == ImportCusEntryInstructionsDeclarationTypeList.Codes.FreeCirculation)
		{
			result.AddPair(FiscalReferenceCodeList.Codes.FR1_Importer, FiscalReferenceCodeList.Descriptions.FR1_Importer);
			result.AddPair(FiscalReferenceCodeList.Codes.FR2_Customer, FiscalReferenceCodeList.Descriptions.FR2_Customer);
			result.AddPair(FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth, FiscalReferenceCodeList.Descriptions.FR4_HolderOfDeferredPaymentAuth);
			result.AddPair(FiscalReferenceCodeList.Codes.FR5_Vendor, FiscalReferenceCodeList.Descriptions.FR5_Vendor);
		}
		else if (style == ImportCusEntryInstructionsDeclarationTypeList.Codes.ImportSimplified
			|| style == ImportCusEntryInstructionsDeclarationTypeList.Codes.IntroductionOfGoods
			|| style == ImportCusEntryInstructionsDeclarationTypeList.Codes.ProbablyNoControlRequired)
		{
			result.AddPair(FiscalReferenceCodeList.Codes.FR1_Importer, FiscalReferenceCodeList.Descriptions.FR1_Importer);
			result.AddPair(FiscalReferenceCodeList.Codes.FR2_Customer, FiscalReferenceCodeList.Descriptions.FR2_Customer);
			result.AddPair(FiscalReferenceCodeList.Codes.FR3_TaxRepresentative, FiscalReferenceCodeList.Descriptions.FR3_TaxRepresentative);
			result.AddPair(FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth, FiscalReferenceCodeList.Descriptions.FR4_HolderOfDeferredPaymentAuth);
			result.AddPair(FiscalReferenceCodeList.Codes.FR5_Vendor, FiscalReferenceCodeList.Descriptions.FR5_Vendor);
			result.AddPair(UCC6IMPFiscalReferenceCodeList.Codes.FR7_TaxablePerson, UCC6IMPFiscalReferenceCodeList.Descriptions.FR7_TaxablePerson);
		}

		return result;
	}
}
