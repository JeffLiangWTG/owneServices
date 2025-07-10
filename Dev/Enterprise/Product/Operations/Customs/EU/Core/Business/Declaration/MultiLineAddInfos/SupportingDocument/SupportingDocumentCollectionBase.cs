using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

public class SupportingDocumentCollection<T> : Customs.Business.CusSupportingInfoCollection<T>, ISupportingDocumentCollection<T>
	where T : SupportingDocument
{
	public SupportingDocumentCollection(BusinessObject parent) : base(parent, Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument)
	{
	}

	public T AddNew(ZString code, ZString referenceNumber)
	{
		var supportingDocument = AddNew();
		supportingDocument.CSI_Code = code;
		supportingDocument.CSI_ReferenceNumber = referenceNumber;
		return supportingDocument;
	}

	public T AddNewInvoiceDocumentAndCopyDataFromInvoice(ZString code)
	{
		var newInvoiceDocument = AddNew();
		newInvoiceDocument.CSI_Code = code;

		if (Master is JobComInvoiceHeader invoice)
		{
			CopyDataFromInvoice(newInvoiceDocument, invoice);
		}

		return newInvoiceDocument;
	}

	public T AddNewIfNotExsistWithSameCodeAndReference(ZString code, ZString referenceNumber)
	{
		T supportingDocument = null;
		if (!this.Cast<T>().Any(x => x.CSI_Code == code && x.CSI_ReferenceNumber == referenceNumber))
		{
			supportingDocument = AddNew(code, referenceNumber);
		}
		return supportingDocument;
	}

	void CopyDataFromInvoice(T invoiceDocument, JobComInvoiceHeader invoice)
	{
		invoiceDocument.CSI_ReferenceNumber = invoice.JZ_InvoiceNumber;
		invoiceDocument.CSI_DateOfIssue = invoice.JZ_InvoiceDate;
		invoiceDocument.CSI_RN_NKCountryCode = invoice.Supplier_Effective?.MainAddress?.OA_RN_NKCountryCode ?? ZString.Empty;
	}

	public EUSupportingDocumentHelper Helper
	{
		get
		{
			if (helper == null && Master is ICanBeImportOrExport importOrExportParent)
			{
				helper = GetNewSupportingDocumentHelper(importOrExportParent);
			}
			return helper;
		}
	}
	EUSupportingDocumentHelper helper;

	public void DeleteAllDocumentsHavingCode(ZString code)
	{
		this.Cast<T>().Where(supportingDocument => supportingDocument.CSI_Code == code).DeleteAll();
	}

	protected virtual EUSupportingDocumentHelper GetNewSupportingDocumentHelper(ICanBeImportOrExport parentImportOrExpot)
	{
		return new EUSupportingDocumentHelper(parentImportOrExpot);
	}
}
