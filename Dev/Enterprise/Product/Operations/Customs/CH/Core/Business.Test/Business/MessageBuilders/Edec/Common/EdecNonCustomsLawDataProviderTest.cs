using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

internal class EdecNonCustomsLawDataProviderTest : TestCaseWithFactory
{
	public void TestNewCollectionWhenEntryLineIsNull()
	{
		AssertEquals("Argument is null", 0, EdecNonCustomsLawDataProvider.NewCollection(null).Count());
	}

	public void TestNewCollectionShouldReturnEmptyCollectionWhenNoInvoiceLines()
	{
		var providers = EdecNonCustomsLawDataProvider.NewCollection(entryLine);

		AssertEquals(0, providers.Count());
	}

	public void TestNewCollectionShouldReturnEmptyCollectionWhenInvoiceLineHasNoNonCustomsLaw()
	{
		var providers = EdecNonCustomsLawDataProvider.NewCollection(entryLine);

		AssertEquals(0, providers.Count());
	}

	public void TestNewCollectionShouldReturnAllItemsWhenInvoiceLineHasNonCustomsLaws()
	{
		var invoiceLine = AddInvoiceLine();
		AddNonCustomsLaw(invoiceLine, "123");
		AddNonCustomsLaw(invoiceLine, "456");

		var providers = EdecNonCustomsLawDataProvider.NewCollection(entryLine).ToArray();

		AssertEquals(2, providers.Length);
		AssertEquals("123", providers[0].NonCustomsLawType);
		AssertEquals("456", providers[1].NonCustomsLawType);
	}

	public void TestNewCollectionShouldFilterItemsWhenCodeIsEmpty()
	{
		var invoiceLine = AddInvoiceLine();
		AddNonCustomsLaw(invoiceLine, "123");
		AddNonCustomsLaw(invoiceLine, string.Empty);

		var providers = EdecNonCustomsLawDataProvider.NewCollection(entryLine).ToArray();

		AssertEquals(1, providers.Length);
		AssertEquals("123", providers[0].NonCustomsLawType);
	}

	public void TestNewCollectionShouldFilterDuplicatedNonCustomsLawsWhenCustomsLawsHaveSameCSI_Code()
	{
		var invoiceLine = AddInvoiceLine();
		AddNonCustomsLaw(invoiceLine, "123");
		AddNonCustomsLaw(invoiceLine, "123");

		var providers = EdecNonCustomsLawDataProvider.NewCollection(entryLine).ToArray();

		AssertEquals(1, providers.Length);
		AssertEquals("123", providers[0].NonCustomsLawType);
	}

	public void TestNewCollectionShouldReturnNonCustomsLawsFromAllInvoiceLines()
	{
		var invoiceLine = AddInvoiceLine();
		AddNonCustomsLaw(invoiceLine, "123");
		var invoiceLine2 = AddInvoiceLine();
		AddNonCustomsLaw(invoiceLine2, "456");

		var providers6 = EdecNonCustomsLawDataProvider.NewCollection(entryLine).ToArray();

		AssertEquals(2, providers6.Length);
		AssertEquals("123", providers6[0].NonCustomsLawType);
		AssertEquals("456", providers6[1].NonCustomsLawType);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		invoice = declaration.Invoices.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.AllEntryLines.AddNew();
	}

	JobComInvoiceLine AddInvoiceLine()
	{
		var invoiceLine = invoice.InvoiceLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);
		return invoiceLine;
	}

	static NonCustomsLaw AddNonCustomsLaw(JobComInvoiceLine invoiceLine, string code)
	{
		var nonCustomsLaw = invoiceLine.NonCustomsLaws.AddNew();
		nonCustomsLaw.CSI_Code = code;
		return nonCustomsLaw;
	}

	JobComInvoiceHeader invoice;
	CusEntryLine entryLine;
}
