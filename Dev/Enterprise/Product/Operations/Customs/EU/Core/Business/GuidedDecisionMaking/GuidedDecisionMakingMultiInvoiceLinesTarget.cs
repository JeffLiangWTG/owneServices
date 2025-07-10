using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business;

public class GuidedDecisionMakingMultiInvoiceLinesTarget : IGuidedDecisionMakingTarget
{
	public GuidedDecisionMakingMultiInvoiceLinesTarget(List<JobComInvoiceLine> invoiceLines)
	{
		this.invoiceLines = Argument.NotNull(invoiceLines, nameof(invoiceLines));
	}

	readonly List<JobComInvoiceLine> invoiceLines;

	public ZString TariffCode { set => invoiceLines.ForEach(line => line.JI_Tariff = value); }
	public ZString CountryOfOrigin { set => invoiceLines.ForEach(line => line.JI_CountryOfOrigin = value); }
	public ZString Preference { set => invoiceLines.ForEach(line => line.JI_PrimaryPreference = value); }
	public ZString QuotaOrderNumber { set => invoiceLines.ForEach(line => line.JI_ConcessionOrder = value); }
	public ZDecimal CustomsFirstQuantity { set => invoiceLines.ForEach(line => line.JI_CustomsQuantity = value); }
	public ZString CustomsFirstUnitQty { set => invoiceLines.ForEach(line => line.JI_CustomsUnitQty = value); }
	public ZDecimal CustomsSecondQuantity { set => invoiceLines.ForEach(line => line.JI_CustomsSecondQuantity = value); }
	public ZString CustomsSecondUnitQty { set => invoiceLines.ForEach(line => line.JI_CustomsSecondUnitQty = value); }
	public ZDecimal CustomsThirdQuantity { set => invoiceLines.ForEach(line => line.JI_CustomsThirdQuantity = value); }
	public ZString CustomsThirdUnitQty { set => invoiceLines.ForEach(line => line.JI_CustomsThirdUnitQty = value); }
	public virtual ZString CountryOfDestination { set => invoiceLines.ForEach(line => line.ZG_CountryOfDestination = value); }

	bool IsUCC6 => this.invoiceLines.FirstOrDefault()?.Declaration.IsUCC6 ?? false;

	ZString[] AdditionalDocumentCodeTypes => new ZString[] { UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalInformation, UniversalReferenceConstants.RefCusCodeListType.Code.ImportAdditionalInformation, UniversalReferenceConstants.RefCusCodeListType.Code.ExportAdditionalReference, UniversalReferenceConstants.RefCusCodeListType.Code.ImportAdditionalReference, UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument, UniversalReferenceConstants.RefCusCodeListType.Code.ImportTransportDocument };

	public void SetVATCode(ZString vatCode, ZString vatAdditionalCode) => SetVATCodeCore(vatCode, vatAdditionalCode);

	protected virtual void SetVATCodeCore(ZString vatCode, ZString vatAdditionalCode)
	{
		invoiceLines.ForEach(line => line.JI_TaxOrFeeDetail = line.Lookups.TaxOrFeeDetailEntities.FirstOrDefault(d => d.VATCode.Equals(vatCode, System.StringComparison.CurrentCultureIgnoreCase) && d.AdditionalCode.EqualsIgnoringCase(vatAdditionalCode))?.PK ?? ZGuid.Empty);
	}

	public void SetAdditionalCodes(List<ZString> codesToSet)
	{
		invoiceLines.ForEach(line =>
		{
			ClearAdditionalCodes(line);
			SetSelectedAdditionalCodes(line, codesToSet);
		});
	}

	void ClearAdditionalCodes(JobComInvoiceLine invoiceLine)
	{
		invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
		invoiceLine.JI_SupplementaryCode2 = ZString.Empty;
		invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
	}

	void SetSelectedAdditionalCodes(JobComInvoiceLine invoiceLine, List<ZString> codes)
	{
		foreach (var s in codes.Take(2))
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
		foreach (var s in codes.Skip(2))
		{
			invoiceLine.AdditionalSupplementaryCodes.AddNew(s);
		}
	}

	public void SetSupportingAndAdditionalDocuments(List<(ZString Code, ZString Reference, ZDateTime DateOfIssue)> documentsToSet)
	{
		invoiceLines.ForEach(line =>
		{
			SetSupportingAndAdditionalDocuments(line, documentsToSet);
		});
	}

	void SetSupportingAndAdditionalDocuments(JobComInvoiceLine invoiceLine, List<(ZString Code, ZString Reference, ZDateTime DateOfIssue)> documentsToSet)
	{
		var dataGroupingForAdditionalDocumentCodes = invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.AdditionalDocumentCodes);
		var additionalDocumentCodeList = ZZRefCusCodeListCombinedCollection.GetCachedCollection(invoiceLine.Factory, dataGroupingForAdditionalDocumentCodes, AdditionalDocumentCodeTypes, ZDate.Today, null);

		if (IsUCC6 && !additionalDocumentCodeList.IsLoaded)
		{
			additionalDocumentCodeList.Load();
		}

		foreach (var documentToAdd in documentsToSet)
		{
			var documentCodeType = additionalDocumentCodeList.Cast<ZZRefCusCodeListCombined>().FirstOrDefault(x => x.ZZD_Code == documentToAdd.Code);
			if (IsUCC6 && documentCodeType != null)
			{
				SetAdditionalDocument(invoiceLine, documentToAdd, documentCodeType.ZZD_CodeType);
			}
			else
			{
				SetSupportingDocument(invoiceLine, documentToAdd);
			}
		}
	}

	void SetSupportingDocument(JobComInvoiceLine invoiceLine, (ZString Code, ZString Reference, ZDateTime DateOfIssue) supportingDocumentToAdd)
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

	void SetAdditionalDocument(JobComInvoiceLine invoiceLine, (ZString Code, ZString Reference, ZDateTime DateOfIssue) additionalDocumentToAdd, ZString documentCodeType)// ZZRefCusCodeListCombinedCollection additionalDocumentCodeList)
	{
		var document = invoiceLine.AdditionalInfos.Cast<AdditionalInfo>().FirstOrDefault(x => x.CSI_Code == additionalDocumentToAdd.Code);
		//var documentCodeType = additionalDocumentCodeList.Cast<ZZRefCusCodeListCombined>().FirstOrDefault(x => x.ZZD_Code == additionalDocumentToAdd.Code).ZZD_CodeType;

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
}
