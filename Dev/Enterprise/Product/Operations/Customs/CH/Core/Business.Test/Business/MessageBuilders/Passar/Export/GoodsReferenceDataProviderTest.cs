using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class GoodsReferenceDataProviderTest : TestCaseWithFactory
{
	public void TestNewCollection() => AssertEquals(0, GoodsReferenceDataProvider.NewCollection(null).Count());

	public void TestProperties()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryLine1 = Factory.New<CusEntryLine>();
		entryLine1.CL_LineNumber = 10;
		invoiceLine1.JI_CL = entryLine1.PK;
		var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryLine2 = Factory.New<CusEntryLine>();
		entryLine2.CL_LineNumber = 11;
		invoiceLine2.JI_CL = entryLine2.PK;
		var invoiceLine3 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine3.JI_CL = entryLine2.PK;
		var invoiceLine4 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var container = declaration.CusContainers.AddNew();
		container.InvoiceLinePivotCollection.AddNew(invoiceLine1);
		container.InvoiceLinePivotCollection.AddNew(invoiceLine2);
		container.InvoiceLinePivotCollection.AddNew(invoiceLine3);
		container.InvoiceLinePivotCollection.AddNew(invoiceLine4);

		var goodsReferences = GoodsReferenceDataProvider.NewCollection(declaration.CusContainers.Single()).ToArray();

		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, goodsReferences.Length);
			AssertEquals("Sequence at 1 index", 1, goodsReferences[0].SequenceNumber);
			AssertEquals("Sequence at 2 index", 2, goodsReferences[1].SequenceNumber);

			AssertEquals("DeclarationGoodsItemNumber at 1 index", 10, goodsReferences[0].DeclarationGoodsItemNumber);
			AssertEquals("DeclarationGoodsItemNumber at 2 index", 11, goodsReferences[1].DeclarationGoodsItemNumber);
		});
	}
}
