using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(DirectDebitFileFilterBusinessObject))]
	public class DirectDebitFileFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		public void TestBatchNumberFilter()
		{
			DirectDebitBatchHeader directBatchHeader1 = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			DirectDebitBatchHeader directBatchHeader2 = Factory.NewWithValidTestData<DirectDebitBatchHeader>();

			directBatchHeader1.IsManuallySetTransactionNumber_ForTestOnly = true;
			directBatchHeader2.IsManuallySetTransactionNumber_ForTestOnly = true;

			directBatchHeader1.AH_TransactionNum = "00001111";
			directBatchHeader2.AH_TransactionNum = "00022222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Batch Number"];

			filter.Property = "1111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain DirectBatchHeader1", FilterCollection.Contains(directBatchHeader1));
			Assert("Expecting collection not to contain DirectBatchHeader2", !FilterCollection.Contains(directBatchHeader2));

			filter.Property = "00022222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain DirectBatchHeader1", !FilterCollection.Contains(directBatchHeader1));
			Assert("Expecting collection to contain DirectBatchHeader2", FilterCollection.Contains(directBatchHeader2));

			filter.Property = "3333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain DirectBatchHeader1", !FilterCollection.Contains(directBatchHeader1));
			Assert("Expecting collection not to contain DirectBatchHeader2", !FilterCollection.Contains(directBatchHeader2));
		}

		public void TestBankListProperty()
		{
			AssertNotNull(FilterBO.BankList);
		}

		public void TestBankAccountFilter()
		{
			AccBankAccount account1 = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount account2 = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount account3 = Factory.NewWithValidTestData<AccBankAccount>();

			DirectDebitBatchHeader directBatchHeader1 = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			DirectDebitBatchHeader directBatchHeader2 = Factory.NewWithValidTestData<DirectDebitBatchHeader>();

			directBatchHeader1.AH_AB = account1.PK;
			directBatchHeader2.AH_AB = account2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Bank Account"];

			filter.Property = account1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain DirectBatchHeader1", FilterCollection.Contains(directBatchHeader1));
			Assert("Expecting collection not to contain DirectBatchHeader2", !FilterCollection.Contains(directBatchHeader2));

			filter.Property = account3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain DirectBatchHeader1", !FilterCollection.Contains(directBatchHeader1));
			Assert("Expecting collection not to contain DirectBatchHeader2", !FilterCollection.Contains(directBatchHeader2));
		}

		public void TestPaymentTransactionNumberFilter()
		{
			DataRegistry.Instance.MultiSearchSeparator = ",";

			DirectDebitBatchHeader directBatchHeader1 = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			DirectDebitBatchHeader directBatchHeader2 = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			DirectDebitBatchHeader directBatchHeader3 = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			APPayment payment1 = Factory.NewWithValidTestData<APPayment>();
			ARPayment payment2 = Factory.NewWithValidTestData<ARPayment>();
			DirectPayment payment3 = Factory.NewWithValidTestData<DirectPayment>();
			payment3.AH_TransactionNum = "00001002";
			payment3.IsManuallySetTransactionNumber_ForTestOnly = true;
			Factory.Save(); // number fountain will auto-assign transaction numbers

			payment1.AH_ReceiptBatchNo = directBatchHeader1.AH_TransactionNum;
			payment2.AH_ReceiptBatchNo = directBatchHeader2.AH_TransactionNum;
			payment3.AH_ReceiptBatchNo = directBatchHeader3.AH_TransactionNum;
			Factory.Save();

			ModuleFountainFilter filter = (ModuleFountainFilter)FilterBO["Payment Transaction #"];
			filter.Property = payment1.AH_TransactionNum;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain DirectBatchHeader1", FilterCollection.Contains(directBatchHeader1));
			Assert("Expecting collection not to contain DirectBatchHeader2", !FilterCollection.Contains(directBatchHeader2));
			Assert("Expecting collection not to contain DirectBatchHeader3", !FilterCollection.Contains(directBatchHeader3));

			filter.Property = payment2.AH_TransactionNum;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain DirectBatchHeader1", !FilterCollection.Contains(directBatchHeader1));
			Assert("Expecting collection to contain DirectBatchHeader2", FilterCollection.Contains(directBatchHeader2));
			Assert("Expecting collection not to contain DirectBatchHeader3", !FilterCollection.Contains(directBatchHeader3));

			filter.Property = payment3.AH_TransactionNum;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain DirectBatchHeader1", !FilterCollection.Contains(directBatchHeader1));
			Assert("Expecting collection not to contain DirectBatchHeader2", !FilterCollection.Contains(directBatchHeader2));
			Assert("Expecting collection to contain DirectBatchHeader3", FilterCollection.Contains(directBatchHeader3));

			filter.Property = payment1.AH_TransactionNum + "," + payment2.AH_TransactionNum;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain DirectBatchHeader1", FilterCollection.Contains(directBatchHeader1));
			Assert("Expecting collection to contain DirectBatchHeader2", FilterCollection.Contains(directBatchHeader2));
			Assert("Expecting collection not to contain DirectBatchHeader3", !FilterCollection.Contains(directBatchHeader3));

			filter.Property = payment1.AH_TransactionNum + "," + payment2.AH_TransactionNum + "," + payment3.AH_TransactionNum + ",','','a',asdfgasdfgasdfgasdfgasdfgasdfgasdfgasdfg";
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain DirectBatchHeader1", FilterCollection.Contains(directBatchHeader1));
			Assert("Expecting collection to contain DirectBatchHeader2", FilterCollection.Contains(directBatchHeader2));
			Assert("Expecting collection to contain DirectBatchHeader3", FilterCollection.Contains(directBatchHeader3));
		}

		public void TestPaymentTransactionReferenceNumberFilter()
		{
			DataRegistry.Instance.MultiSearchSeparator = ",";

			DirectDebitBatchHeader directBatchHeader1 = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			DirectDebitBatchHeader directBatchHeader2 = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			DirectDebitBatchHeader directBatchHeader3 = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			APPayment payment1 = Factory.NewWithValidTestData<APPayment>();
			ARPayment payment2 = Factory.NewWithValidTestData<ARPayment>();
			DirectPayment payment3 = Factory.NewWithValidTestData<DirectPayment>();
			Factory.Save(); // number fountain will auto-assign transaction numbers

			payment1.AH_ReceiptBatchNo = directBatchHeader1.AH_TransactionNum;
			payment2.AH_ReceiptBatchNo = directBatchHeader2.AH_TransactionNum;
			payment3.AH_ReceiptBatchNo = directBatchHeader3.AH_TransactionNum;
			payment1.AH_ChequeOrReference = "abc";
			payment2.AH_ChequeOrReference = "xyz";
			payment3.AH_ChequeOrReference = "qwe";
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Payment Transaction Reference #"];
			filter.Property = "abc";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain DirectBatchHeader1", FilterCollection.Contains(directBatchHeader1));
			Assert("Expecting collection not to contain DirectBatchHeader2", !FilterCollection.Contains(directBatchHeader2));
			Assert("Expecting collection not to contain DirectBatchHeader3", !FilterCollection.Contains(directBatchHeader3));

			filter.Property = "xyz";
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain DirectBatchHeader1", !FilterCollection.Contains(directBatchHeader1));
			Assert("Expecting collection to contain DirectBatchHeader2", FilterCollection.Contains(directBatchHeader2));
			Assert("Expecting collection not to contain DirectBatchHeader3", !FilterCollection.Contains(directBatchHeader3));

			filter.Property = "qwe";
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain DirectBatchHeader1", !FilterCollection.Contains(directBatchHeader1));
			Assert("Expecting collection not to contain DirectBatchHeader2", !FilterCollection.Contains(directBatchHeader2));
			Assert("Expecting collection to contain DirectBatchHeader3", FilterCollection.Contains(directBatchHeader3));

			filter.Property = "abc,xyz";
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain DirectBatchHeader1", FilterCollection.Contains(directBatchHeader1));
			Assert("Expecting collection to contain DirectBatchHeader2", FilterCollection.Contains(directBatchHeader2));
			Assert("Expecting collection not to contain DirectBatchHeader3", !FilterCollection.Contains(directBatchHeader3));

			filter.Property = "abc,xyz,qwe,','','a',asdfgasdfgasdfgasdfgasdfgasdfgasdfgasdfg";
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain DirectBatchHeader1", FilterCollection.Contains(directBatchHeader1));
			Assert("Expecting collection to contain DirectBatchHeader2", FilterCollection.Contains(directBatchHeader2));
			Assert("Expecting collection to contain DirectBatchHeader3", FilterCollection.Contains(directBatchHeader3));
		}

		public void TestPaymentTransactionAmountFilter()
		{
			DirectDebitBatchHeader directBatchHeader1 = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			DirectDebitBatchHeader directBatchHeader2 = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			DirectDebitBatchHeader directBatchHeader3 = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			Factory.Save(); // number fountain will auto-assign transaction numbers

			APPayment payment1 = Factory.NewWithValidTestData<APPayment>();
			ARPayment payment2 = Factory.NewWithValidTestData<ARPayment>();
			DirectPayment payment3 = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 1500, 500, 1500, 500);
			payment1.AH_ReceiptBatchNo = directBatchHeader1.AH_TransactionNum;
			payment2.AH_ReceiptBatchNo = directBatchHeader2.AH_TransactionNum;
			payment3.AH_ReceiptBatchNo = directBatchHeader3.AH_TransactionNum;
			payment1.AH_InvoiceAmount = 50;
			payment1.AH_GSTAmount = 0;
			payment1.AH_OSTotal = 50;
			payment1.AH_OutstandingAmount = 50;
			payment2.AH_InvoiceAmount = 500;
			payment2.AH_GSTAmount = 0;
			payment2.AH_OSTotal = 500;
			payment2.AH_OutstandingAmount = 500;

			Factory.Save();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)FilterBO["Payment Transaction Amount"];
			filter.Property1 = 0;
			filter.Property2 = 50;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain DirectBatchHeader1", FilterCollection.Contains(directBatchHeader1));
			Assert("Expecting collection not to contain DirectBatchHeader2", !FilterCollection.Contains(directBatchHeader2));
			Assert("Expecting collection not to contain DirectBatchHeader3", !FilterCollection.Contains(directBatchHeader3));

			filter.Property1 = 51;
			filter.Property2 = 500;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain DirectBatchHeader1", !FilterCollection.Contains(directBatchHeader1));
			Assert("Expecting collection to contain DirectBatchHeader2", FilterCollection.Contains(directBatchHeader2));
			Assert("Expecting collection not to contain DirectBatchHeader3", !FilterCollection.Contains(directBatchHeader3));

			filter.Property1 = 501;
			filter.Property2 = 5000;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain DirectBatchHeader1", !FilterCollection.Contains(directBatchHeader1));
			Assert("Expecting collection not to contain DirectBatchHeader2", !FilterCollection.Contains(directBatchHeader2));
			Assert("Expecting collection to contain DirectBatchHeader3", FilterCollection.Contains(directBatchHeader3));
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DirectDebitFileFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			FilterCollection = new DirectDebitBatchHeaderCollection(Factory);
			FilterBO = (DirectDebitFileFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		DirectDebitBatchHeaderCollection FilterCollection;
		DirectDebitFileFilterBusinessObject FilterBO;

		#endregion
	}
}
