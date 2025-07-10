using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.EU.Testing;

namespace Enterprise.Customs.BE.Business.Testing
{
	[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Belgium)]
	sealed class BEDocSADHTest : DocSADHTest
	{
		public void TestBox16CountryOfOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			var entryLine = entryHeader.MergedLines.AddNew();

			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			invoiceLine1.JI_CountryOfOrigin = "ES";
			invoiceLine2.JI_CountryOfOrigin = "ES";

			var wrapper = BEDocSADH.New(entryHeader, Factory);
			AssertEquals("Same Country of origin for entries", "Spain", wrapper.Box16CountryOfOrigin);

			invoiceLine2.JI_CountryOfOrigin = "IT";
			AssertEquals("Different Country of origin for entries", ZString.Empty, wrapper.Box16CountryOfOrigin);
		}

		public void TestBox16CountryOfOriginWithEmptyOrInvalidCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			var entryLine = entryHeader.MergedLines.AddNew();

			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			invoiceLine1.JI_CountryOfOrigin = "ES";
			invoiceLine2.JI_CountryOfOrigin = ZString.Empty;

			var wrapper = BEDocSADH.New(entryHeader, Factory);
			AssertEquals("One Country empty", ZString.Empty, wrapper.Box16CountryOfOrigin);

			invoiceLine1.JI_CountryOfOrigin = ZString.Empty;
			invoiceLine2.JI_CountryOfOrigin = ZString.Empty;
			AssertEquals("Two Countries empty", ZString.Empty, wrapper.Box16CountryOfOrigin);

			invoiceLine1.JI_CountryOfOrigin = "ES";
			invoiceLine2.JI_CountryOfOrigin = "XX";
			AssertEquals("One Country invalid", ZString.Empty, wrapper.Box16CountryOfOrigin);

			invoiceLine1.JI_CountryOfOrigin = "XX";
			invoiceLine2.JI_CountryOfOrigin = "XX";
			AssertEquals("Two Countries invalid", ZString.Empty, wrapper.Box16CountryOfOrigin);
		}

		protected override ZString CountrySpecificCurrency => "EUR";
		protected override ZString ExpectedEadBarCode => ZString.Empty;
		protected override ZDateTime ExpectedDOE => new ZDateTime(2008, 7, 1);
	}
}
