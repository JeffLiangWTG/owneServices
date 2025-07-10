using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

class DocumentDataProviderTest : TestCaseWithFactory
{
	public void TestNewCollection() => AssertEquals("empty collection", Enumerable.Empty<DocumentDataProvider>(), DocumentDataProvider.NewCollection(null));

	public void TestSequenceNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		invoice.PreviousDocuments.AddNew();
		invoice.PreviousDocuments.AddNew();
		declaration.Invoices.AddNew().PreviousDocuments.AddNew();

		var messageBuilderCollection = DocumentDataProvider.NewCollection(invoice.PreviousDocuments);
		AssertEquals("Count", 2, messageBuilderCollection.Count());

		CombineAssertions(() =>
		{
			AssertEquals("Sequence at 1 index", 1, messageBuilderCollection.ElementAt(0).SequenceNumber);
			AssertEquals("Sequence at 2 index", 2, messageBuilderCollection.ElementAt(1).SequenceNumber);
		});
	}

	public void TestSequenceNumberWithOffset()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		invoice.PreviousDocuments.AddNew();
		invoice.PreviousDocuments.AddNew();
		declaration.Invoices.AddNew().PreviousDocuments.AddNew();

		var messageBuilderCollection = DocumentDataProvider.NewCollection(invoice.PreviousDocuments, 10);
		AssertEquals("Count", 2, messageBuilderCollection.Count());

		CombineAssertions(() =>
		{
			AssertEquals("Sequence at 1 index", 11, messageBuilderCollection.ElementAt(0).SequenceNumber);
			AssertEquals("Sequence at 2 index", 12, messageBuilderCollection.ElementAt(1).SequenceNumber);
		});
	}

	public void TestProperties()
	{
		const string code = "789";
		const string referenceNumber = "456";
		const string referenceNumber2 = "123";
		const int itemNumber = 1;

		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var previousDocument = invoice.PreviousDocuments.AddNew();

		previousDocument.CSI_Code = code;
		previousDocument.CSI_ReferenceNumber2 = referenceNumber2;
		previousDocument.CSI_ReferenceNumber = referenceNumber;
		previousDocument.CSI_ItemNumber = itemNumber;

		var dataProvider = DocumentDataProvider.NewCollection(invoice.PreviousDocuments).First();

		CombineAssertions(() =>
		{
			AssertEquals("SequenceNumber", 1, dataProvider.SequenceNumber);
			AssertEquals("Type", code, dataProvider.Type);
			AssertEquals("ReferenceNumber", referenceNumber, dataProvider.ReferenceNumber);
			AssertEquals("ComplementOfInformation", referenceNumber2, dataProvider.ComplementOfInformation);
			AssertEquals("ItemNumber", itemNumber, dataProvider.GoodsItemNumber);
			AssertEquals("TransportEquipments count", 0, dataProvider.TransportEquipments.Count);
		});
	}
}
