using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(TransactionCurrencySummaryRowCollection))]
	public class TransactionCurrencySummaryRowCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TransactionCurrencySummaryRowCollection>
	{
		public void TestAddNewByCurrency()
		{
			var creator = new TestObjectCreator(Factory);
			ARInvoice invoice1 = creator.CreateARInvoice<ARInvoice>("001", creator.AUD, 1, creator.ABIGAS);

			invoice1.AH_OSTotalAmount = 100m;
			invoice1.AH_LocalTaxAmount = 100m;
			invoice1.AH_LocalOutstandingAmount = 300m;
			invoice1.AH_OSTotal = 100;
			invoice1.AH_DueDate = new ZDate(2019, 1, 1);
			creator.CreateInvoiceLine(invoice1, 100m, setTaxes: false);

			ARInvoice invoice2 = creator.CreateARInvoice<ARInvoice>("003", creator.USD, 2, creator.ABIGAS);

			invoice2.AH_OSTotalAmount = 300m;
			invoice2.AH_LocalOutstandingAmount = 800m;
			invoice2.AH_LocalTaxAmount = 600m;
			invoice2.AH_OSTotal = 100;
			invoice2.AH_DueDate = new ZDate(2019, 3, 1);
			creator.CreateInvoiceLine(invoice2, 300m, setTaxes: false);

			var transactionCurrencySummary = new TransactionCurrencySummary(new AccTransactionHeader[] { invoice1, invoice2 }, Factory);
			transactionCurrencySummaryRowCollection = transactionCurrencySummary.TransactionCurrencySummaryRows;

			AssertEquals(2, transactionCurrencySummaryRowCollection.Count);
			AssertEquals("AUD", transactionCurrencySummaryRowCollection[0].Currency);
			AssertEquals("USD", transactionCurrencySummaryRowCollection[1].Currency);
		}

		protected override TransactionCurrencySummaryRowCollection GetCollectionToTest()
		{
			return new TransactionCurrencySummaryRowCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TransactionCurrencySummaryRow(Factory);
		}

		TransactionCurrencySummaryRowCollection transactionCurrencySummaryRowCollection;
	}
}
