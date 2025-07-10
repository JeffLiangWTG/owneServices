using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	public class InvoicingBaseForDisplayOtherTaxesTest : TestCaseWithFactory
	{
		public void TestTaxRecordTransactionLinePivotForDisplay_RegisterTaxTransactionCollectionAsEditableChild()
		{
			var taxConfig = taxFrameworkTestObjectCreator.CreateTaxConfiguration(TaxConfigurationLedgers.AccountsPayable.Code);
			var taxRate = creator.CreateTaxRate("TID", "TID Desc", 6);
			Factory.Save();

			var apInvoice = creator.CreateInvoice(typeof(APInvoice));
			var line = creator.CreateInvoiceLine(apInvoice, creator.GLHeader1.PK, 100m);

			var taxTransaction = taxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters() { TransactionHeader = apInvoice, TaxConfiguration = taxConfig, TaxId = taxRate, TaxRate = (16, 1) });
			taxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxTransaction.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line));

			_ = TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(apInvoice).TaxRecordTransactionLinePivotForDisplay;

			CombineAssertions(() =>
			{
				//The test to simulate the scenario that invoice has no error when tax transaction has no changes and no errors. Invoice Has errors when tax transaction fields modified i.e., has changes and errors.
				taxTransaction.HasChanges = false;
				Assert("Precondition:", !taxTransaction.HasChanges);
				Assert("Precondition:", !taxTransaction.HasErrors);
				Assert(!apInvoice.HasErrors);

				taxTransaction.ATT_Rate = 110;
				//The test verifies that, after modifying tax transaction editable fields, errors on the tax transactions if any, are identified on invoice level. So, a random error has been used to test here.
				//The below error is when tax transaction rate is greater than 100.
				Assert("Precondition:", taxTransaction.HasChanges);
				Assert("Precondition:", taxTransaction.HasErrors);
				AssertHasError("Precondition:", taxTransaction.ATT_RateInfo, "Rate must be less than 100%");

				Assert(apInvoice.HasChanges);
				Assert(apInvoice.HasErrors);
			});
		}

		public void TestTaxRecordParentPassedAsParameter()
		{
			var collection = objForTest.TaxRecordTransactionLinePivotForDisplay.TaxTransactionCollection;
			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);
			ITaxRecordParent taxRecordParentAsParam = null;
			taxProcessorMock.Setup(t => t.DeleteTaxRecordNotInDB(It.IsAny<ITaxRecordParent>(), It.IsAny<AccTaxTransaction>())).Callback<ITaxRecordParent, AccTaxTransaction>((t1, t2) => taxRecordParentAsParam = t1);
			collection.Delete(null);
			AssertEquals(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice), taxRecordParentAsParam);
		}

		public void TestAllPropertiesReferToUnderlyingInvoicingBase()
		{
			var mockITaxFrameworkConfigurationHelper = taxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();

			var displayObj = objForTest as ITransactionWithLinesForDisplayOtherTaxes;
			AssertEquals(false, displayObj.IsTaxTransactionsCalculatedBeforePosting);
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = true;
			AssertEquals(true, displayObj.IsTaxTransactionsCalculatedBeforePosting);
			AssertEquals(invoice, displayObj.Transaction);

			AssertEquals(0, displayObj.Lines.Count());
			var line1 = creator.CreateInvoiceLine(invoice, 100m);
			AssertContainsExactElementsInAnyOrder(new[] { line1 }.Select(l => TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(l)), displayObj.Lines);
			var line2 = creator.CreateInvoiceLine(invoice, 100m);
			AssertContainsExactElementsInAnyOrder(new[] { line1, line2 }.Select(l => TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(l)), displayObj.Lines);

			Assert(!taxRecordParent.ShouldCalculateTaxTransactions);
			Assert(!displayObj.ShouldShowTransactionLines);
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);
			Assert(taxRecordParent.ShouldCalculateTaxTransactions);
			Assert(displayObj.ShouldShowTransactionLines);

			AssertEquals(0, taxRecordParent.OnOtherTaxesCalculatedBeforePosting_ChangedEventHandlerCount_ForTestOnly);
			var handler1 = new EventHandler(DummyEventHandler);
			displayObj.OnOtherTaxesCalculatedBeforePosting_Changed += handler1;
			AssertEquals(1, taxRecordParent.OnOtherTaxesCalculatedBeforePosting_ChangedEventHandlerCount_ForTestOnly);
			var handler2 = new EventHandler(DummyEventHandler);
			displayObj.OnOtherTaxesCalculatedBeforePosting_Changed += handler2;
			AssertEquals(2, taxRecordParent.OnOtherTaxesCalculatedBeforePosting_ChangedEventHandlerCount_ForTestOnly);
			displayObj.OnOtherTaxesCalculatedBeforePosting_Changed -= handler1;
			AssertEquals(1, taxRecordParent.OnOtherTaxesCalculatedBeforePosting_ChangedEventHandlerCount_ForTestOnly);
			displayObj.OnOtherTaxesCalculatedBeforePosting_Changed -= handler2;
			AssertEquals(0, taxRecordParent.OnOtherTaxesCalculatedBeforePosting_ChangedEventHandlerCount_ForTestOnly);

			void DummyEventHandler(object sender, EventArgs e)
			{
			}
		}

		protected override void SetUp()
		{
			creator = new TestObjectCreator(Factory);
			taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory);
			invoice = creator.CreateInvoice(typeof(APInvoice));
			objForTest = TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice);
		}

		InvoicingBaseForDisplayOtherTaxes objForTest;
		InvoicingBase invoice;
		TestObjectCreator creator;
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;
	}
}
