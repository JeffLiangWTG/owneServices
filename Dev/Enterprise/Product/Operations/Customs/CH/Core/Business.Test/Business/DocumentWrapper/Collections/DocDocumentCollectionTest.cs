using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DocDocumentCollection))]
sealed class DocDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocDocumentCollection>
{
	protected override DocDocumentCollection GetCollectionToTest()
	{
		return new DocDocumentCollection(Factory);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Enterprise.Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
		declaration.JE_ClusterKey = 1;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var previousDocument = invoiceHeader.PreviousDocuments.AddNew();

		return DocDocumentDataWrapper.New(previousDocument, Factory);
	}
}
