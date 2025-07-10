using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public class TransactionHeaderFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchHintVw_AccTransactionHeaderTaxSchemaLoadAtOneGo()
		{
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "002", TestObjectCreator.AUD, 1.0m, 200.00m, 20.00m, 200.00m, 20.00m);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "003", TestObjectCreator.AUD, 1.0m, 300.00m, 30.00m, 300.00m, 30.00m);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "004", TestObjectCreator.AUD, 1.0m, 400.00m, 40.00m, 400.00m, 40.00m);
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.ActiveFetchHintsForTable(AccTransactionHeaderSchema.Constants.TableName);
			var transactions = newFactory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, SQLComparisonOperator.StartsWith, "00"));
			AssertNotNull(transactions);
			AssertEquals("The number that loaded should be 4", 4, transactions.Length);
			//Only accessing to field to fire the view loading
			var dummyOSTaxAmount = transactions[0].AH_OSTaxAmount;
			dummyOSTaxAmount = transactions[1].AH_OSTaxAmount;
			dummyOSTaxAmount = transactions[2].AH_OSTaxAmount;
			dummyOSTaxAmount = transactions[3].AH_OSTaxAmount;

			//AccTransactionHeader: 1
			//vw_AccTransactionHeaderTax: 2
			//Hits: 3/1

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertMaxDbHits(3, newFactory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		protected TestObjectCreator TestObjectCreator;
	}
}
