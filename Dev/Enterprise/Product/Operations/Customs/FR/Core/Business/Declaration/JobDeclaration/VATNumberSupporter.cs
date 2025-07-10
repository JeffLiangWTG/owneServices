using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public abstract class VATNumberSupporter
	{
		protected VATNumberSupporter(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		protected readonly JobDeclaration declaration;

		public void SetIdentifiedVATNumber() => SetIdentifiedVATNumberCore();
		protected abstract void SetIdentifiedVATNumberCore();

		public void SetUnidentifiedVATNumber()
		{
			AddVATNumberDocumentToCusSupportingCollection(VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber);
		}

		bool HasVATSpecialMention(JobComInvoiceLine invoiceLine) => HasVATSpecialMentionCore(invoiceLine);
		protected abstract bool HasVATSpecialMentionCore(JobComInvoiceLine invoiceLine);

		public bool HasIdentifiedVATNumber(JobComInvoiceLine invoiceLine) => HasIdentifiedVATNumberCore(invoiceLine);
		protected abstract bool HasIdentifiedVATNumberCore(JobComInvoiceLine invoiceLine);

		public bool HasUnidentifiedVATNumber(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.Declaration?.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber) ?? false;
		}

		public void ValidateVATNumber(ZPropertyInfo propertyInfo)
		{
			if (GetVATDeferNumberFromImporter() == FRConstants.VATRegistrationNumberValue.OCCASIONNEL)
			{
				if (RequiresVATSpecialMentionButThereIsNone(out JobComInvoiceLine invoiceLine))
				{
					propertyInfo.AddMessageError(RequiresVATSpecialMentionButThereIsNoneMessage(invoiceLine));
				}
			}
			else
			{
				if (RequiresVATNumberDocumentButThereIsNone(out JobComInvoiceLine invoiceLine))
				{
					propertyInfo.AddMessageError(RequiresVATNumberDocumentButThereIsNoneMessage(invoiceLine));
				}
				else if (NotRequiresVATNumberDocumentButThereIsOne())
				{
					propertyInfo.AddMessageError(NotRequiresVATNumberDocumentButThereIsOneMessage());
				}
			}
		}

		string RequiresVATSpecialMentionButThereIsNoneMessage(JobComInvoiceLine invoiceLine) => RequiresVATSpecialMentionButThereIsNoneMessageCore(invoiceLine);
		protected abstract string RequiresVATSpecialMentionButThereIsNoneMessageCore(JobComInvoiceLine invoiceLine);

		string RequiresVATNumberDocumentButThereIsNoneMessage(JobComInvoiceLine invoiceLine) => RequiresVATNumberDocumentButThereIsNoneMessageCore(invoiceLine);
		protected abstract string RequiresVATNumberDocumentButThereIsNoneMessageCore(JobComInvoiceLine invoiceLine);

		string NotRequiresVATNumberDocumentButThereIsOneMessage() => NotRequiresVATNumberDocumentButThereIsOneMessageCore();
		protected abstract string NotRequiresVATNumberDocumentButThereIsOneMessageCore();

		ZBool RequiresVATSpecialMentionButThereIsNone(out JobComInvoiceLine invoiceLine)
		{
			invoiceLine = declaration.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => x.RequiresVATNumberDocument && !HasVATSpecialMention(x));
			return invoiceLine != null;
		}

		ZBool RequiresVATNumberDocumentButThereIsNone(out JobComInvoiceLine invoiceLine)
		{
			invoiceLine = declaration.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => x.RequiresVATNumberDocument && !HasUnidentifiedVATNumber(x) && !HasIdentifiedVATNumber(x));
			return invoiceLine != null;
		}

		ZBool NotRequiresVATNumberDocumentButThereIsOne()
		{
			return declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.RequiresVATNumberDocument && (HasUnidentifiedVATNumber(x) || HasIdentifiedVATNumber(x)));
		}

		public ZString GetVATDeferNumberForAutoliquidation(OrgHeader header) => GetVATDeferNumberForAutoliquidationCore(header);

		protected virtual ZString GetVATDeferNumberForAutoliquidationCore(OrgHeader header) => ZString.Empty;

		public ZString GetVATDeferNumberFromImporter() => declaration.Importer != null ? GetVATDeferNumberFromOrgHeader(declaration.Importer) : ZString.Empty;

		public ZString GetVATDeferNumberFromOrgHeader(OrgHeader header) => header.CustomsCodes.GetCustomsRegNo(OrgCusCode.FranceCodeTypes.TVA, declaration.CountryCode);

		internal ZString GetVATDeferNumberWithFR3FiscalFallBack(OrgHeader header)
		{
			var vatDeferNumber = ZString.Empty;
			var euOrgAddInfo = EUOrgImpAddInfo.Get(header, Core.Constants.CountryCodes.France);
			if (euOrgAddInfo is EUOrgImpAddInfo)
			{
				euOrgAddInfo.Deserialise();
				if (euOrgAddInfo.ZO_UseFr3FiscalRepresentation && header.AllRelatedParties.Cast<OrgRelatedParty>().FirstOrDefault(orp => orp.PR_PartyType == RelatedPartyTypeList.Codes.CustomsPostponedVatAccounting)?.RelatedParty is OrgHeader cpvRelatedParty)
				{
					vatDeferNumber = declaration.VATNumberSupporter.GetVATDeferNumberFromOrgHeader(cpvRelatedParty);
				}
			}
			if (vatDeferNumber.IsEmpty)
			{
				vatDeferNumber = declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().SelectMany(instruction => instruction.FiscalReferences).FirstOrDefault(fiscal => fiscal.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative)?.CFR_Reference ?? ZString.Empty;
			}
			if (vatDeferNumber.IsEmpty)
			{
				vatDeferNumber = declaration.VATNumberSupporter.GetVATDeferNumberFromOrgHeader(header);
			}
			return vatDeferNumber;
		}

		public bool CodeIsVATNumberRelated(SupportingDocument supportingDocument) => CodeIsVATNumberRelatedCore(supportingDocument);
		protected abstract bool CodeIsVATNumberRelatedCore(SupportingDocument supportingDocument);

		protected void AddVATNumberDocumentToCusSupportingCollection(ZString vatNumberDocumentType)
		{
			var cusSupportingCollection = declaration.SupportingDocuments;
			var cusSupportingInfo = cusSupportingCollection.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_Code == VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber || x.CSI_Code == VATDeferStrategyCodeList.Codes.IdentifiedVATNumber);
			if (cusSupportingInfo == null)
			{
				cusSupportingInfo = cusSupportingCollection.AddNew();
				cusSupportingInfo.CSI_Code = vatNumberDocumentType;
			}
			else if (cusSupportingInfo.CSI_Code != vatNumberDocumentType)
			{
				cusSupportingInfo.CSI_Code = vatNumberDocumentType;
			}
		}

		protected void AddPostponedVATToFiscalReferenceCollection()
		{
			var entryInstructions = declaration.CustomsEntryInstructions;
			foreach (var entryInstruction in entryInstructions)
			{
				var fiscalReference = entryInstruction.FiscalReferences.Cast<CusFiscalReference>().FirstOrDefault(x => x.CFR_Code == DeltaIEFiscalReferenceCodeList.Codes.FR7);
				if (fiscalReference == null)
				{
					fiscalReference = entryInstruction.FiscalReferences.AddNew();
					fiscalReference.CFR_Code = DeltaIEFiscalReferenceCodeList.Codes.FR7;
				}
			}
		}
	}
}
