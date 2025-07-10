using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	public class TaxFrameworkObjectFactoryTest : TestCaseWithFactory
	{
		public void TestInvoicingBaseTaxFrameworkViewModel()
		{
			AssertNull(TaxFrameworkObjectFactory.GetInvoicingBaseTaxFrameworkViewModel(null));
			var obj = TaxFrameworkObjectFactory.GetInvoicingBaseTaxFrameworkViewModel(invoice);
			AssertType<InvoicingBaseTaxFrameworkViewModel>(obj);
			AssertEquals("Singleton", obj, TaxFrameworkObjectFactory.GetInvoicingBaseTaxFrameworkViewModel(invoice));
		}

		public void TestInvoicingLineBaseTaxable()
		{
			AssertNull(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(null));
			var obj = TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line);
			AssertEquals("Singleton", obj, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line));
		}

		public void TestInvoicingLineBaseForOtherTaxesDisplay()
		{
			AssertNull(TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(null));
			var obj = TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(line);
			AssertEquals("Singleton", obj, TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(line));
		}

		public void TestInvoicingBaseTaxRecordParent()
		{
			AssertNull(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(null));
			var obj = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			AssertEquals("Singleton", obj, TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice));
		}

		public void TestInvoicingBaseForDisplayOtherTaxes()
		{
			AssertNull(TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(null));
			var obj = TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice);
			AssertEquals("Singleton", obj, TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice));
		}

		public void TestWHTAmountLoader()
		{
			AssertNull(TaxFrameworkObjectFactory.GetWHTAmountLoader(null));
			var obj = TaxFrameworkObjectFactory.GetWHTAmountLoader(Factory);
			AssertType<WHTAmountLoader>(obj);
			AssertEquals("Singleton", obj, TaxFrameworkObjectFactory.GetWHTAmountLoader(Factory));
		}

		public void TestAPJournalBasedWHTAmountCalculator()
		{
			AssertNull(TaxFrameworkObjectFactory.GetAPJournalBasedWHTAmountCalculator(null));
			var obj = TaxFrameworkObjectFactory.GetAPJournalBasedWHTAmountCalculator(Factory);
			AssertType<WithholdingJournalCreationManager>(obj);
			AssertEquals("Singleton", obj, TaxFrameworkObjectFactory.GetAPJournalBasedWHTAmountCalculator(Factory));
		}

		public void TestTaxRealisationEnabler()
		{
			AssertNull(TaxFrameworkObjectFactory.GetTaxRealisationEnabler(null));
			var obj = TaxFrameworkObjectFactory.GetTaxRealisationEnabler(Factory);
			AssertType<TaxRealisationEnabler>(obj);
			AssertEquals("Singleton", obj, TaxFrameworkObjectFactory.GetTaxRealisationEnabler(Factory));
		}

		protected override void SetUp()
		{
			creator = new TestObjectCreator(Factory);
			invoice = creator.CreateInvoice(typeof(APInvoice));
			line = creator.CreateInvoiceLine(invoice, 100M);
		}

		InvoicingBase invoice;
		InvoicingLineBase line;
		TestObjectCreator creator;
	}
}
