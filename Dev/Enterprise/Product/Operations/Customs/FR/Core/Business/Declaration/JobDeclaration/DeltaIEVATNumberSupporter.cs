using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIEVATNumberSupporter : VATNumberSupporter
	{
		public DeltaIEVATNumberSupporter(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override void SetIdentifiedVATNumberCore()
		{
			AddPostponedVATToFiscalReferenceCollection();
		}

		protected override bool HasIdentifiedVATNumberCore(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.EntryInstruction?.FiscalReferences.Cast<CusFiscalReference>().Any(x => x.CFR_Code == DeltaIEFiscalReferenceCodeList.Codes.FR7) ?? false;
		}

		protected override bool CodeIsVATNumberRelatedCore(SupportingDocument supportingDocument)
		{
			return supportingDocument.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber;
		}

		protected override string RequiresVATNumberDocumentButThereIsNoneMessageCore(JobComInvoiceLine invoiceLine)
		{
			return Res.GetString("A113C78D-B876-4C67-A715-9F0D76DA51DE", "Procedure {0} on invoice line {1} requires a FR7 fiscal reference or G008 supporting document for VAT number.", invoiceLine.JI_Procedure, invoiceLine.JI_LineNo);
		}

		protected override string NotRequiresVATNumberDocumentButThereIsOneMessageCore()
		{
			return Res.GetString("FE6E96E1-3552-4F0B-A68A-D8947C241617", "No procedure requires a FR7 fiscal reference nor G008 supporting document for VAT number.");
		}

		protected override bool HasVATSpecialMentionCore(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.Declaration?.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance && x.IsAnAdditionalInformation) ?? false;
		}

		protected override string RequiresVATSpecialMentionButThereIsNoneMessageCore(JobComInvoiceLine invoiceLine)
		{
			return Res.GetString("7DA64DC6-9BEE-4213-83AA-2296869B4367", "Procedure {0} on invoice line {1} requires a G0008 additional information of type INF for VAT number.", invoiceLine.JI_Procedure, invoiceLine.JI_LineNo);
		}

		protected override ZString GetVATDeferNumberForAutoliquidationCore(OrgHeader header) => GetVATDeferNumberWithFR3FiscalFallBack(header);
	}
}
