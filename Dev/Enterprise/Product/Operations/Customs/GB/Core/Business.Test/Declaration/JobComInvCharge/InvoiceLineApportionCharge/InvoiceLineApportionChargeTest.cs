using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineApportionCharge))]
	public class InvoiceLineApportionChargeTest : EU.Business.Declaration.Testing.InvoiceLineApportionChargeTest
	{
		public void TestCDSChargeCode()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var charge = invoiceLine.ApportionedCharges.AddNew();
			InvChargeTestHelper.TestCDSChargeCode(charge, x => ((InvoiceLineApportionCharge)x).CDSChargeCode);
			dec.JE_ApplicationCode = "CHF";
			InvChargeTestHelper.TestCDSChargeCode(charge, x => ((InvoiceLineApportionCharge)x).CDSChargeCode);
		}

		public void TestValidation()
		{
			var parent = Factory.New<InvoiceLineApportionCharge>();
			AssertType<InvoiceLineApportionChargeValidation>(parent.Validation);
		}

		public void TestLookups()
		{
			var parent = Factory.New<InvoiceLineApportionCharge>();
			AssertType<InvoiceLineApportionChargeLookups>(parent.Lookups);
		}
	}
}
