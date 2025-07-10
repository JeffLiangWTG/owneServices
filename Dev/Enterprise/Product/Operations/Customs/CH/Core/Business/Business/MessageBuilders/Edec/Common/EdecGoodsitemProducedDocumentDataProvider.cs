using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;

namespace Enterprise.Customs.CH.Business;

public class EdecGoodsitemProducedDocumentDataProvider : IEdecGoodsitemProducedDocument
{
	public static IEnumerable<EdecGoodsitemProducedDocumentDataProvider> NewCollection(CusEntryLine entryLine)
	{
		return entryLine != null ? GetProducedDocumentsDataProviders(entryLine.InvoiceLines.Cast<JobComInvoiceLine>()) : Enumerable.Empty<EdecGoodsitemProducedDocumentDataProvider>();
	}

	public static EdecGoodsitemProducedDocumentDataProvider New(SupportingDocument supportingDocument) => supportingDocument == null ? null : new EdecGoodsitemProducedDocumentDataProvider(supportingDocument);

	EdecGoodsitemProducedDocumentDataProvider(SupportingDocument supportingDocument)
	{
		this.supportingDocument = Argument.NotNull(supportingDocument, nameof(supportingDocument));
	}

	readonly SupportingDocument supportingDocument;

	public string DocumentType => supportingDocument.CSI_Code;

	public string DocumentReferenceNumber => supportingDocument.CSI_ReferenceNumber;

	public DateTime? IssueDate => supportingDocument.CSI_DateOfIssue.IsValid ? supportingDocument.CSI_DateOfIssue.ToDateTime() : null;

	public string AdditionalInformation => supportingDocument.CSI_ReferenceNumber2;

	#region Implementation

	static IEnumerable<EdecGoodsitemProducedDocumentDataProvider> GetProducedDocumentsDataProviders(IEnumerable<JobComInvoiceLine> invoiceLines)
	{
		var producedDocumentsProviders = GetInvoiceHeaderAndLineProducedDocumentsDataProviders(invoiceLines);

#if NETFRAMEWORK
		return producedDocumentsProviders.DistinctBy(o => new { o.DocumentType, o.DocumentReferenceNumber, o.AdditionalInformation, o.IssueDate });
#else
		return IEnumerableExtensions.DistinctBy(producedDocumentsProviders, o => new { o.DocumentType, o.DocumentReferenceNumber, o.AdditionalInformation, o.IssueDate });
#endif
	}

	static IEnumerable<EdecGoodsitemProducedDocumentDataProvider> GetInvoiceHeaderAndLineProducedDocumentsDataProviders(IEnumerable<JobComInvoiceLine> invoiceLines)
	{
		var invoiceHeaders = invoiceLines.Select(x => x.InvoiceHeader).Distinct().Cast<JobComInvoiceHeader>();
		foreach (var dataProvider in invoiceHeaders.SelectMany(x => GetProducedDocumentsDataProvidersFromSupportingDocuments(x.SupportingDocuments)))
		{
			yield return dataProvider;
		}

		foreach (var dataProvider in invoiceLines.SelectMany(x => GetProducedDocumentsDataProvidersFromSupportingDocuments(x.SupportingDocuments)))
		{
			yield return dataProvider;
		}
	}

	static IEnumerable<EdecGoodsitemProducedDocumentDataProvider> GetProducedDocumentsDataProvidersFromSupportingDocuments(SupportingDocumentCollection supportingDocumentsCollection)
	{
		return supportingDocumentsCollection.Cast<SupportingDocument>().Select(x => New(x));
	}

	#endregion
}
