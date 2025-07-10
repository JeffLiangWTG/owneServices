using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	public class TaxFrameworkBOLoaderTest : TestCaseWithFactory
	{
		public void TestLoadTransactionLineForOtherTaxesDisplay()
		{
			var transaction = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			ITaxFrameworkBOLoader loader = ObjectFactory.Get<ITaxFrameworkBOLoader>();

			var loaded = loader.LoadTransactionLineForOtherTaxesDisplay(Factory, new ZQuery());
			AssertEquals(0, loaded.Count());
			loaded = loader.LoadTransactionLineForOtherTaxesDisplay(Factory, new ZQuery(AccTransactionLinesSchema.AL_AH, transaction.PK));
			AssertEquals(0, loaded.Count());

			var line1 = TestObjectCreator.CreateInvoiceLine(transaction, 100m);
			loaded = loader.LoadTransactionLineForOtherTaxesDisplay(Factory, new ZQuery(AccTransactionLinesSchema.PK, line1.PK));
			AssertContainsExactElementsInAnyOrder(new[] { line1 }
			.Select(l => TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(l)), loaded);
			loaded = loader.LoadTransactionLineForOtherTaxesDisplay(Factory, new ZQuery(AccTransactionLinesSchema.AL_AH, transaction.PK));
			AssertEquals(1, loaded.Count());
			AssertContainsExactElementsInAnyOrder(transaction.Lines.Cast<InvoicingLineBase>()
				.Select(l => TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(l)), loaded);

			var line2 = TestObjectCreator.CreateInvoiceLine(transaction, 100m);
			loaded = loader.LoadTransactionLineForOtherTaxesDisplay(Factory, new ZQuery(AccTransactionLinesSchema.PK, line2.PK));
			AssertContainsExactElementsInAnyOrder(new[] { line2 }
			.Select(l => TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(l)), loaded);
			loaded = loader.LoadTransactionLineForOtherTaxesDisplay(Factory, new ZQuery(AccTransactionLinesSchema.AL_AH, transaction.PK));
			AssertEquals(2, loaded.Count());
			AssertContainsExactElementsInAnyOrder(transaction.Lines.Cast<InvoicingLineBase>()
				.Select(l => TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(l)), loaded);
		}

		public void TestLoadTaxRecordParent()
		{
			ITaxFrameworkBOLoader loader = ObjectFactory.Get<ITaxFrameworkBOLoader>();
			var taxParent = loader.LoadTaxRecordParent(Factory, ZGuid.NewZGuid());
			AssertNull(taxParent);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			taxParent = loader.LoadTaxRecordParent(Factory, invoice.PK);
			AssertEquals(invoice.PK, taxParent.PK);

			var newFactory = new BusinessObjectFactory();
			taxParent = loader.LoadTaxRecordParent(newFactory, invoice.PK);
			AssertNull(taxParent);

			Factory.Save();

			taxParent = loader.LoadTaxRecordParent(newFactory, invoice.PK);
			AssertNotNull(taxParent);
			AssertEquals(invoice.PK, taxParent.PK);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
