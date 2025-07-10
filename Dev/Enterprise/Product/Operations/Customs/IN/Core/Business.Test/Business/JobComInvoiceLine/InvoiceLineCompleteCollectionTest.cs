using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(InvoiceLineCompleteCollection))]
sealed class InvoiceLineCompleteCollectionTest : Customs.Business.Testing.InvoiceLineCompleteCollectionTest
{
	public void TestChildType()
	{
		AssertType<JobComInvoiceLine>(Collection.AddNew());
	}

	public void TestDefaultJI_ValuationMarkup()
	{
		BaseJobDeclaration declaration = BaseJobDeclaration.New(base.Factory);
		BaseJobComInvoiceHeader baseJobComInvoiceHeader = declaration.Invoices.AddNew();
		InvoiceLineCompleteCollection collection = (InvoiceLineCompleteCollection)declaration.InvoiceLines;

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		BaseJobComInvoiceLine line_isExport = collection.AddNew();
		AssertEquals(110.00m, line_isExport.JI_ValuationMarkup);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		BaseJobComInvoiceLine line_isImport = collection.AddNew();
		AssertEquals(0.00m, line_isImport.JI_ValuationMarkup);
	}

	protected override BusinessObjectCollection GetCollectionToTest() => new InvoiceLineCompleteCollection(Declaration as JobDeclaration);

	protected override BaseJobDeclaration GetMeANewJobDeclaration() => Factory.New<JobDeclaration>();

	public void TestSetDefaultTransitCountry()
	{
		var invoiceLineCompleteCollection = new InvoiceLineCompleteCollection(Declaration as JobDeclaration);
		var invoiceLine1 = invoiceLineCompleteCollection.AddNew();
		AssertEquals("Transit Country Empty", ZString.Empty, invoiceLine1.JI_RN_NKCountryOfTransit);

		invoiceLine1.JI_RN_NKCountryOfTransit = "IN";
		var invoiceLine2 = invoiceLineCompleteCollection.AddNew();

		AssertEquals("Transit Country default from previous line", "IN", invoiceLine2.JI_RN_NKCountryOfTransit);
	}
}
