using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(InvoiceLineViewCollection))]
sealed class InvoiceLineViewCollectionTest : BusinessObjectCollectionViewTestCase<InvoiceLineViewCollection>
{
	public void TestTypedIndexer()
	{
		var collection = new InvoiceLineViewCollection(Declaration);
		var invoiceLine = collection.AddNew();
		AssertEquals(invoiceLine, collection[0]);
	}

	public void TestElementType()
	{
		var collection = new InvoiceLineViewCollection(Declaration);
		var invoiceLine = collection.AddNew();
		AssertType<JobComInvoiceLine>(invoiceLine);
		AssertType<JobComInvoiceLine>(collection[0]);
	}

	protected override InvoiceLineViewCollection GetCollectionToTest()
	{
		var collection = new InvoiceLineViewCollection(Declaration);
		return collection;
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var result = Factory.New<JobComInvoiceLine>();
		result.JI_JZ = Declaration.Invoices[0].PK;
		return result;
	}

	JobDeclaration Declaration
	{
		get
		{
			if (declaration == null)
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.Invoices.AddNew();
			}
			return declaration;
		}
	}
	JobDeclaration declaration;
}
