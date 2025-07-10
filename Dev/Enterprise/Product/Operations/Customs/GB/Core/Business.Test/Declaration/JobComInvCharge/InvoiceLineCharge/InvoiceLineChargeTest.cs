using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineCharge))]
	public class InvoiceLineChargeTest : EU.Business.Declaration.Testing.InvoiceLineChargeTest
	{
		public void TestCDSChargeCode()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var charge = invoiceLine.Charges.AddNew();
			InvChargeTestHelper.TestCDSChargeCode(charge, x => ((InvoiceLineCharge)x).CDSChargeCode);
			dec.JE_ApplicationCode = "CHF";
			InvChargeTestHelper.TestCDSChargeCode(charge, x => ((InvoiceLineCharge)x).CDSChargeCode);
		}

		protected override BaseJobDeclaration GetJobDeclarationForTest()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";
			return dec;
		}

		public new void TestValidation()
		{
			var parent = Factory.New<InvoiceLineCharge>();
			AssertType<InvoiceLineChargeValidation>(parent.Validation);
		}

		public new void TestLookups()
		{
			var parent = Factory.New<InvoiceLineCharge>();
			AssertType<InvoiceLineChargeLookups>(parent.Lookups);
		}
	}
}
