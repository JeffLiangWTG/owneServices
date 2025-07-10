using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	class ApportionedJobComInvHeaderChargeTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			TestInvoice invoice = Factory.New<TestInvoice>();
			TestApportionedCharge apportionedCharge = invoice.ApportionedCharges.AddNew();
			NUnit.Framework.Assert.That(apportionedCharge.J7_IsApportionedCharge, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "isApportionedCharge is set");
		}

		[ExpectNoExceptions]
		public void TestApportionedChargesIsReadOnly()
		{
			TestInvoice invoice = Factory.New<TestInvoice>();
			TestApportionedCharge apportionedCharge = invoice.ApportionedCharges.AddNew();
			TestCharge charge = invoice.Charges.AddNew();

			TestInvoiceLine invoiceLine = invoice.InvoiceLines.AddNew();
			JobComInvCharge apportionedCharge2 = invoiceLine.ApportionedCharges.AddNew();
			JobComInvCharge charge2 = invoiceLine.Charges.AddNew();

			NUnit.Framework.Assert.That(charge.ReadOnly, Is.EqualTo(false), "Invoice charge is not readonly");
			NUnit.Framework.Assert.That(apportionedCharge.ReadOnly, Is.EqualTo(true), "Invoice Apportioned charge is not readonly");
			NUnit.Framework.Assert.That(charge2.ReadOnly, Is.EqualTo(false), "Invoice charge is not readonly");
			NUnit.Framework.Assert.That(apportionedCharge2.ReadOnly, Is.EqualTo(true), "Invoice Apportioned charge is not readonly");
		}

		[ExpectNoExceptions]
		public void TestSetHasChangesDoestCauseException()
		{
			TestInvoice invoice = Factory.New<TestInvoice>();
			JobComInvCharge testCharge = invoice.ApportionedCharges.AddNew();
			testCharge.HasChanges = true;
		}
	}
}

