using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingSingleInvoiceLineTarget : IGuidedDecisionMakingTarget
	{
		public GuidedDecisionMakingSingleInvoiceLineTarget(JobComInvoiceLine invoiceLine)
		{
			Argument.NotNull(invoiceLine, nameof(invoiceLine));
			this.invoiceLine = invoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		public ZString TariffCode { set => invoiceLine.JI_Tariff = value; }
		public ZString CountryOfOrigin { set => invoiceLine.JI_CountryOfOrigin = value; }
		public ZString Preference { set => invoiceLine.JI_PrimaryPreference = value; }
		public ZString QuotaOrderNumber { set => invoiceLine.JI_ConcessionOrder = value; }
		public ZDecimal CustomsFirstQuantity { set => invoiceLine.JI_CustomsQuantity = value; }
		public ZString CustomsFirstUnitQty { set => invoiceLine.JI_CustomsUnitQty = value; }
		public ZDecimal CustomsSecondQuantity { set => invoiceLine.JI_CustomsSecondQuantity = value; }
		public ZString CustomsSecondUnitQty { set => invoiceLine.JI_CustomsSecondUnitQty = value; }
		public ZDecimal CustomsThirdQuantity { set => invoiceLine.JI_CustomsThirdQuantity = value; }
		public ZString CustomsThirdUnitQty { set => invoiceLine.JI_CustomsThirdUnitQty = value; }
		public virtual ZString CountryOfDestination { set => invoiceLine.ZG_CountryOfDestination = value; }

		bool IsUCC6 => ((IUcc6ValueProvider)invoiceLine).IsUCC6;

		ZString DataGroupingForAdditionalDocumentCodes => invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.AdditionalDocumentCodes);

		ZString[] AdditionalDocumentCodeTypes => new ZString[] { UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalInformation, UniversalReferenceConstants.RefCusCodeListType.Code.ImportAdditionalInformation, UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalReference, UniversalReferenceConstants.RefCusCodeListType.Code.ImportAdditionalReference, UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument, UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument };

		ZZRefCusCodeListCombinedCollection AdditionalDocumentCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(invoiceLine.Factory, DataGroupingForAdditionalDocumentCodes, AdditionalDocumentCodeTypes, ZDate.Today, null);

		public void SetVATCode(ZString vatCode, ZString vatAdditionalCode) => SetVATCodeCore(vatCode, vatAdditionalCode);

		protected virtual void SetVATCodeCore(ZString vatCode, ZString vatAdditionalCode)
		{
			invoiceLine.JI_TaxOrFeeDetail = invoiceLine.Lookups.TaxOrFeeDetailEntities.FirstOrDefault(d => d.VATCode.Equals(vatCode, System.StringComparison.CurrentCultureIgnoreCase) && d.AdditionalCode.EqualsIgnoringCase(vatAdditionalCode))?.PK ?? ZGuid.Empty;
		}

		public void SetAdditionalCodes(List<ZString> codesToSet)
		{
			ClearAdditionalCodes();
			SetSelectedAdditionalCodes(codesToSet);
		}

		void ClearAdditionalCodes()
		{
			invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
			invoiceLine.JI_SupplementaryCode2 = ZString.Empty;
			invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
		}

		void SetSelectedAdditionalCodes(List<ZString> codesToSet)
		{
			SetInvoiceLineSupplementaryCodes(codesToSet);
		}

		public void SetSupportingAndAdditionalDocuments(List<(ZString Code, ZString Reference, ZDateTime DateOfIssue)> documentsToSet) 
		{
			if (IsUCC6 && !AdditionalDocumentCodeList.IsLoaded)
			{
				AdditionalDocumentCodeList.Load();
			}

			foreach (var documentToAdd in documentsToSet)
			{
				if (IsUCC6 && AdditionalDocumentCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == documentToAdd.Code))
				{
					SetAdditionalDocument(documentToAdd);
				}
				else
				{
					SetSupportingDocument(documentToAdd);
				}
			}
		}

		void SetSupportingDocument((ZString Code, ZString Reference, ZDateTime DateOfIssue) supportingDocumentToAdd)
		{
			var document = invoiceLine.SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_Code == supportingDocumentToAdd.Code);
			if (document == null)
			{
				var newSupportingDocument = invoiceLine.SupportingDocuments.AddNew();
				newSupportingDocument.CSI_Code = supportingDocumentToAdd.Code;
				newSupportingDocument.CSI_ReferenceNumber = supportingDocumentToAdd.Reference.Left(SupportingDocument.Schema.ReferenceNumberMaxLength);
				newSupportingDocument.CSI_DateOfIssue = supportingDocumentToAdd.DateOfIssue;
			}
			else
			{
				document.CSI_ReferenceNumber = supportingDocumentToAdd.Code.IsEmpty ? document.CSI_ReferenceNumber : supportingDocumentToAdd.Reference.Left(SupportingDocument.Schema.ReferenceNumberMaxLength);
				document.CSI_DateOfIssue = supportingDocumentToAdd.Code.IsEmpty ? document.CSI_DateOfIssue : supportingDocumentToAdd.DateOfIssue;
			}
		}

		void SetAdditionalDocument((ZString Code, ZString Reference, ZDateTime DateOfIssue) additionalDocumentToAdd)
		{
			var document = invoiceLine.AdditionalInfos.Cast<AdditionalInfo>().FirstOrDefault(x => x.CSI_Code == additionalDocumentToAdd.Code);
			var documentCodeType = AdditionalDocumentCodeList.Cast<ZZRefCusCodeListCombined>().FirstOrDefault(x => x.ZZD_Code == additionalDocumentToAdd.Code).ZZD_CodeType;

			if (document == null)
			{
				var newAdditionalDocument = invoiceLine.AdditionalInfos.AddNew();
				newAdditionalDocument.CSI_Code = additionalDocumentToAdd.Code;
				newAdditionalDocument.CSI_ReferenceNumber = additionalDocumentToAdd.Reference;
				newAdditionalDocument.CSI_SubType = GetCSI_SubType(documentCodeType);
			}
			else
			{
				document.CSI_ReferenceNumber = additionalDocumentToAdd.Code.IsEmpty ? document.CSI_ReferenceNumber : additionalDocumentToAdd.Reference;
				document.CSI_SubType = additionalDocumentToAdd.Code.IsEmpty ? document.CSI_SubType : GetCSI_SubType(documentCodeType);
			}
		}

		ZString GetCSI_SubType(ZString codeType)
		{
			var subType = ZString.Empty;

			if (codeType == UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalInformation || codeType == UniversalReferenceConstants.RefCusCodeListType.Code.ImportAdditionalInformation)
			{
				subType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			}
			else if (codeType == UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalReference || codeType == UniversalReferenceConstants.RefCusCodeListType.Code.ImportAdditionalReference)
			{
				subType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			}
			else if (codeType == UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument || codeType == UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument)
			{
				subType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			}

			return subType;
		}

		void SetInvoiceLineSupplementaryCodes(List<ZString> supplementaryCodes)
		{
			foreach (var s in supplementaryCodes.Take(2))
			{
				if (invoiceLine.JI_SupplementaryCode1.IsEmpty)
				{
					invoiceLine.JI_SupplementaryCode1 = s;
				}
				else if (invoiceLine.JI_SupplementaryCode2.IsEmpty)
				{
					invoiceLine.JI_SupplementaryCode2 = s;
				}
				else
				{
					invoiceLine.AdditionalSupplementaryCodes.AddNew(s);
				}
			}

			foreach (var s in supplementaryCodes.Skip(2))
			{
				invoiceLine.AdditionalSupplementaryCodes.AddNew(s);
			}
		}
	}
}
