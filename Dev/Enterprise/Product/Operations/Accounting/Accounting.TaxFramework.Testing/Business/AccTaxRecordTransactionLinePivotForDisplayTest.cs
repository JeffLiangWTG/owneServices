using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	[TestedType(typeof(AccTaxRecordTransactionLinePivotForDisplay))]
	public class AccTaxRecordTransactionLinePivotForDisplayTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTaxTransactionCollection()
		{
			var bizObj = GetNewBusinessObject() as AccTaxRecordTransactionLinePivotForDisplay;
			var collection = bizObj.TaxTransactionCollection;
			AssertEquals(typeof(AccTaxTransactionCollection), collection.GetType());
			AssertContainsExactElementsInAnyOrder(new string[] { "PIB", "FK1" }, collection.Select(x => x.ATT_TaxSystemCode));
		}

		public void TestTransactionLinesForOtherTaxesDisplay()
		{
			var bizObj = GetNewBusinessObject() as AccTaxRecordTransactionLinePivotForDisplay;
			var collection = bizObj.TransactionLinesForOtherTaxesDisplay;
			AssertEquals(typeof(TransactionLineForOtherTaxesDisplayCollection), collection.GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AccTaxRecordTransactionLinePivotForDisplay(TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice), TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice));
		}

		protected override void SetUp()
		{
			base.SetUp();
			creator = new TestObjectCreator(Factory);
			Invoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "001", creator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);

			var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxTransaction.ATT_TaxSystemCode = "PIB";
			taxTransaction.ATT_AH = invoice.PK;

			taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxTransaction.ATT_TaxSystemCode = "FK1";
			taxTransaction.ATT_AH = invoice.PK;

			taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxTransaction.ATT_TaxSystemCode = "FK2";
		}

		public void TestTransactionLinesForOtherTaxesDisplay_ResponseToEvent_OnOtherTaxesCalculated()
		{
			var mockITaxFrameworkConfigurationHelper = new TaxFrameworkTestObjectCreator(Factory).SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			Invoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "001", creator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			AssertGreaterThan(invoice.Lines.Count, 0);

			var bizObj = GetNewBusinessObject() as AccTaxRecordTransactionLinePivotForDisplay;
			AssertEquals(0, bizObj.TaxTransactionCollection.Count);
			AssertEquals("Invoice Lines should be not be added to TransactionLinesForOtherTaxesDisplay", 0, bizObj.TransactionLinesForOtherTaxesDisplay.Count);

			taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = true;
			AssertContainsExactElementsInAnyOrder("Invoice Lines should be added to TransactionLinesForOtherTaxesDisplay",
				invoice.Lines.Cast<InvoicingLineBase>().Select(l => TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(l)), bizObj.TransactionLinesForOtherTaxesDisplay);

			var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxTransaction.ATT_TaxSystemCode = "PIB";
			taxTransaction.ATT_AH = invoice.PK;
			AssertNotEquals(0, bizObj.TaxTransactionCollection.Count);
			taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = false;
			AssertContainsExactElementsInAnyOrder("Invoice Lines should not be deleted from TransactionLinesForOtherTaxesDisplay",
				invoice.Lines.Cast<InvoicingLineBase>().Select(l => TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(l)), bizObj.TransactionLinesForOtherTaxesDisplay);

			taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = true;
			AssertNotEquals("Precondition: Invoice Lines are present in TransactionLinesForOtherTaxesDisplay", 0, bizObj.TransactionLinesForOtherTaxesDisplay.Count);
			ObjectFactory.Get<ITaxProcessor>().DeleteTaxesNotInDB(taxRecordParent);
			AssertEquals(0, bizObj.TaxTransactionCollection.Count);
			AssertEquals("Invoice Lines should be deleted from TransactionLinesForOtherTaxesDisplay", 0, bizObj.TransactionLinesForOtherTaxesDisplay.Count);

			taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = true;
			bizObj = GetNewBusinessObject() as AccTaxRecordTransactionLinePivotForDisplay;
			AssertContainsExactElementsInAnyOrder("Invoice Lines should be added to TransactionLinesForOtherTaxesDisplay",
				invoice.Lines.Cast<InvoicingLineBase>().Select(l => TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(l)), bizObj.TransactionLinesForOtherTaxesDisplay);
		}

		[SuspendCriticalValidation]
		public void TestTransactionLinesForOtherTaxesDisplay_InitialLoad()
		{
			var invoiceWithOtherTaxes = invoice;
			Invoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "001", creator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			AssertEquals(0, displayObj.TaxRecordTransactionLinePivotForDisplay.TaxTransactionCollection.Count);

			Assert(!taxRecordParent.IsTaxTransactionsCalculatedBeforePosting);
			var bizObj = GetNewBusinessObject() as AccTaxRecordTransactionLinePivotForDisplay;
			AssertEquals("Invoice Lines should not be added to TransactionLinesForOtherTaxesDisplay", 0, bizObj.TransactionLinesForOtherTaxesDisplay.Count);

			taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = true;
			bizObj = GetNewBusinessObject() as AccTaxRecordTransactionLinePivotForDisplay;
			AssertNull("Invoice Lines should be not be added without call to TransactionLinesForOtherTaxesDisplay", bizObj.TransactionLinesForOtherTaxesDisplay_ForTestOnly);
			AssertEquals("No OnOtherTaxesCalculatedBeforePosting_Changed events are hooked on the Invoice", 0, taxRecordParent.OnOtherTaxesCalculatedBeforePosting_ChangedEventHandlerCount_ForTestOnly);
			AssertContainsExactElementsInAnyOrder("Invoice Lines should be added to TransactionLinesForOtherTaxesDisplay",
				invoice.Lines.Cast<InvoicingLineBase>().Select(l => TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(l)), bizObj.TransactionLinesForOtherTaxesDisplay);

			Invoice = invoiceWithOtherTaxes;
			AssertGreaterThan(displayObj.TaxRecordTransactionLinePivotForDisplay.TaxTransactionCollection.Count, 0);

			taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = false;
			bizObj = GetNewBusinessObject() as AccTaxRecordTransactionLinePivotForDisplay;
			AssertContainsExactElementsInAnyOrder("Invoice Lines should be added to TransactionLinesForOtherTaxesDisplay",
				invoice.Lines.Cast<InvoicingLineBase>().Select(l => TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(l)), bizObj.TransactionLinesForOtherTaxesDisplay);

			taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = true;
			bizObj = GetNewBusinessObject() as AccTaxRecordTransactionLinePivotForDisplay;
			AssertContainsExactElementsInAnyOrder("Invoice Lines should be added to TransactionLinesForOtherTaxesDisplay",
				invoice.Lines.Cast<InvoicingLineBase>().Select(l => TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(l)), bizObj.TransactionLinesForOtherTaxesDisplay);

			Factory.Save();
			Assert(invoice.IsInDatabase);

			invoice = NewFactory().Load<InvoicingBase>(invoice.PK);
			bizObj = GetNewBusinessObject() as AccTaxRecordTransactionLinePivotForDisplay;
			Assert(!((ITransactionWithLinesForDisplayOtherTaxes)displayObj).ShouldShowTransactionLines);
			AssertEquals("No OnOtherTaxesCalculatedBeforePosting_Changed events are hooked on the Invoice", 0, taxRecordParent.OnOtherTaxesCalculatedBeforePosting_ChangedEventHandlerCount_ForTestOnly);
		}

		InvoicingBase Invoice
		{
			set
			{
				invoice = value;
				taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
				displayObj = TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice);
			}
		}
		InvoicingBase invoice;
		TestObjectCreator creator;
		InvoicingBaseTaxRecordParent taxRecordParent;
		InvoicingBaseForDisplayOtherTaxes displayObj;
	}
}
