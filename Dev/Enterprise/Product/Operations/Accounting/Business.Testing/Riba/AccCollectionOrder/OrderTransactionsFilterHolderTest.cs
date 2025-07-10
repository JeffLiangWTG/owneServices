using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	[TestedType(typeof(OrderTransactionsFilterHolder))]
	public class OrderTransactionsFilterHolderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAddTransactionsIntoOrder()
		{
			ARInvoice invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoice invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			AccCollectionBatch batch = Factory.New<AccCollectionBatch>();
			AccCollectionOrder order = Factory.New<AccCollectionOrder>();
			order.ACO_ACB = batch.PK;
			order.ACO_CollectionDate = ZDate.Today;
			AssertEquals(0, order.CollectionOrderLines.Count);
			OrderTransactionsFilterHolder holder = new OrderTransactionsFilterHolder(order);
			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory);
			transactions.Add(invoice1);
			transactions.Add(invoice2);
			holder.AddTransactionsIntoOrder(transactions);
			AssertEquals(2, order.CollectionOrderLines.Count);
			AccCollectionOrderLine line1 = order.CollectionOrderLines.First(x => x.AOL_AH == invoice1.PK);
			AssertNotNull(line1);
			Assert(line1.IncludeInOrder);
			AccCollectionOrderLine line2 = order.CollectionOrderLines.First(x => x.AOL_AH == invoice2.PK);
			AssertNotNull(line2);
			Assert(line2.IncludeInOrder);
			var logs = batch.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_Reference, string.Format("New transactions added to Order Number {0}.", order.ACO_OrderNumber)));
			AssertEquals(1, logs.Length);
		}

		public void TestCheckAnyTransactionsUsedByActiveOrderLine()
		{
			ARInvoice invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			AccCollectionBatch batch = Factory.New<AccCollectionBatch>();
			batch.ACB_AB = Factory.NewWithValidTestData<AccBankAccount>().PK;
			batch.ACB_BatchNumber = "00001001";
			batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			batch.ACB_TotalAmount = 100m;
			AccCollectionOrder order = Factory.New<AccCollectionOrder>();
			order.ACO_ACB = batch.PK;
			order.ACO_CollectionDate = ZDate.Today;
			order.ACO_Amount = 100m;
			order.ACO_OH_Debtor = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			AccCollectionOrderLine orderLine = Factory.New<AccCollectionOrderLine>();
			orderLine.AOL_ACO = order.PK;
			orderLine.AOL_AH = invoice1.PK;
			Factory.Save();
			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory);
			transactions.Add(invoice1);
			OrderTransactionsFilterHolder holder = new OrderTransactionsFilterHolder(order);
			var result = holder.CheckAnyTransactionsUsedByActiveOrderLine(transactions);
			AssertEquals(invoice1.AH_TransactionNum, result);
		}

		[TestDate(2019, 08, 09)]
		public void TestCheckAnyTransactionsHaveInvalidDueDate()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_DueDate = ZDateTime.Today.AddMinutes(5);

			var batch = Factory.New<AccCollectionBatch>();
			batch.ACB_AB = Factory.NewWithValidTestData<AccBankAccount>().PK;
			batch.ACB_BatchNumber = "00001001";
			batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			batch.ACB_TotalAmount = 100m;

			var order = Factory.New<AccCollectionOrder>();
			order.ACO_ACB = batch.PK;
			order.ACO_CollectionDate = ZDate.Today;
			order.ACO_Amount = 100m;
			order.ACO_OH_Debtor = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			order.ACO_CollectionDate = ZDate.Today;

			var orderLine = Factory.New<AccCollectionOrderLine>();
			orderLine.AOL_ACO = order.PK;
			orderLine.AOL_AH = invoice.PK;
			Factory.Save();

			var transactions = new TransactionHeaderCollection(Factory);
			transactions.Add(invoice);
			var holder = new OrderTransactionsFilterHolder(order);
			var result = holder.CheckAnyTransactionsHaveInvalidDueDate(transactions);
			AssertEquals(invoice.AH_DueDate.Date, order.ACO_CollectionDate);
			Assert(!result);

			invoice.AH_DueDate = ZDateTime.Today.AddDays(1);
			Factory.Save();
			result = holder.CheckAnyTransactionsHaveInvalidDueDate(transactions);
			AssertNotEquals(invoice.AH_DueDate.Date, order.ACO_CollectionDate);
			Assert(result);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			AccCollectionOrder parent = Factory.NewWithValidTestData<AccCollectionOrder>();
			return new OrderTransactionsFilterHolder(parent);
		}
	}
}
