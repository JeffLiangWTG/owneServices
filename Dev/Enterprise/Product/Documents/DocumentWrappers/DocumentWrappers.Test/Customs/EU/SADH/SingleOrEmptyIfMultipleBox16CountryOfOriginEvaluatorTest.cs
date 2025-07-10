using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Latvia)]
	sealed class SingleOrEmptyIfMultipleBox16CountryOfOriginEvaluatorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("entryHeader required", () => new SingleOrEmptyIfMultipleBox16CountryOfOriginEvaluator(null));
			AssertNoExceptionThrown("Valid entryHeader", () => new SingleOrEmptyIfMultipleBox16CountryOfOriginEvaluator(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew()));
		}

		public void TestEvaluate()
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

			var soeimb16cooEvaluator = new SingleOrEmptyIfMultipleBox16CountryOfOriginEvaluator(entryHeader);
			AssertEquals("Same Country of origin for entries", "Spain", soeimb16cooEvaluator.Evaluate());

			invoiceLine2.JI_CountryOfOrigin = "IT";
			AssertEquals("Different Country of origin for entries", ZString.Empty, soeimb16cooEvaluator.Evaluate());
		}

		public void TestEvaluateWithEmptyEntryHeader()
		{
			var entryHeader = Factory.New<CusEntryHeader>();

			var soeimb16cooEvaluator = new SingleOrEmptyIfMultipleBox16CountryOfOriginEvaluator(entryHeader);
			AssertEquals("Empty Entry Header", ZString.Empty, soeimb16cooEvaluator.Evaluate());
		}

		public void TestEvaluateWithEmptyOrInvalidCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var soeimb16cooEvaluator = new SingleOrEmptyIfMultipleBox16CountryOfOriginEvaluator(entryHeader);

			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertEquals("Empty country code", ZString.Empty, soeimb16cooEvaluator.Evaluate());

			invoiceLine.JI_CountryOfOrigin = "QQ";
			AssertEquals("Invalid country code", ZString.Empty, soeimb16cooEvaluator.Evaluate());
		}
	}
}
