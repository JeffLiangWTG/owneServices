using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaGVATNumberSupporter : VATNumberSupporter
	{
		public DeltaGVATNumberSupporter(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override void SetIdentifiedVATNumberCore()
		{
			AddVATNumberDocumentToCusSupportingCollection(VATDeferStrategyCodeList.Codes.IdentifiedVATNumber);
		}

		protected override bool HasIdentifiedVATNumberCore(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.Declaration?.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber) ?? false;
		}

		protected override bool CodeIsVATNumberRelatedCore(SupportingDocument supportingDocument)
		{
			return supportingDocument.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber || supportingDocument.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber;
		}

		protected override string RequiresVATNumberDocumentButThereIsNoneMessageCore(JobComInvoiceLine invoiceLine)
		{
			return Res.GetString("06D0E6FE-23D9-4088-A8B7-215FE1A4F792", "Procedure {0} on invoice line {1} requires a 1008 or G008 supporting document for VAT number.", invoiceLine.JI_Procedure, invoiceLine.JI_LineNo);
		}

		protected override string NotRequiresVATNumberDocumentButThereIsOneMessageCore()
		{
			return Res.GetString("C4C2B69C-33DB-4E21-8C21-8B0AB1B3F1E9", "No procedure requires a 1008 nor G008 supporting document for VAT number.");
		}

		protected override bool HasVATSpecialMentionCore(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.Declaration?.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == UniversalReferenceConstants.RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance) ?? false;
		}

		protected override string RequiresVATSpecialMentionButThereIsNoneMessageCore(JobComInvoiceLine invoiceLine)
		{
			return Res.GetString("B610F0EF-6F94-452A-9B69-1D38A7E854A0", "Procedure {0} on invoice line {1} requires a G0008 special mention for VAT number.", invoiceLine.JI_Procedure, invoiceLine.JI_LineNo);
		}

		protected override ZString GetVATDeferNumberForAutoliquidationCore(OrgHeader header) => GetVATDeferNumberFromOrgHeader(header);
	}
}
