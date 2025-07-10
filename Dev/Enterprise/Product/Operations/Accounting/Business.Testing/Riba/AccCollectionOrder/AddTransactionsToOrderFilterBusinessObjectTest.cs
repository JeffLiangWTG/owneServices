using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Riba
{
	[TestedType(typeof(AddTransactionsToOrderFilterBusinessObject))]
	public class AddTransactionsToOrderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilterByAgreedPaymentMethod()
		{
			arInvoice1.AH_AgreedPaymentMethodOverride = "CRQ";
			arInvoice2.AH_AgreedPaymentMethodOverride = "CRQ";
			arInvoice3.AH_AgreedPaymentMethodOverride = "TRF";
			arCreditNote1.AH_AgreedPaymentMethodOverride = "";
			arJournal.AH_AgreedPaymentMethodOverride = "";
			Factory.Save();
			ModuleTextFilter apmFilter = (ModuleTextFilter)FilterBO["Agreed Payment Method"];
			apmFilter.IsActive = true;
			apmFilter.Property = "CRQ";
			var transactions = new TransactionHeaderCollection(Factory, FilterBO.Filter);
			transactions.Load();
			AssertEquals("There should be 2 matching transactions", 2, transactions.Count);
			Assert("Collection should contain ARInv1", transactions.Contains(arInvoice1));
			Assert("Collection should contain ARInv2", transactions.Contains(arInvoice2));

			arInvoice3.AH_AgreedPaymentMethodOverride = "CRQ";
			arCreditNote1.AH_AgreedPaymentMethodOverride = "CRQ";
			arJournal.AH_AgreedPaymentMethodOverride = "CRQ";
			Factory.Save();
			transactions = new TransactionHeaderCollection(Factory, FilterBO.Filter);
			transactions.Load();
			AssertEquals("There should be 5 matching transactions", 5, transactions.Count);
			Assert("Collection should contain ARInv1", transactions.Contains(arInvoice1));
			Assert("Collection should contain ARInv2", transactions.Contains(arInvoice2));
			Assert("Collection should contain ARInv3", transactions.Contains(arInvoice3));
			Assert("Collection should contain ARCrd1", transactions.Contains(arCreditNote1));
			Assert("Collection should contain ARJnl", transactions.Contains(arJournal));
		}

		public void TestFilterByTransactionType()
		{
			ModuleTextFilter invoiceTypeFilter = (ModuleTextFilter)FilterBO["Transaction Type"];
			invoiceTypeFilter.IsActive = true;
			invoiceTypeFilter.Property = "INV";
			var transactions = new TransactionHeaderCollection(Factory, FilterBO.Filter);
			transactions.Load();
			AssertEquals("There should be 3 matching transactions", 3, transactions.Count);
			Assert("Collection should contain ARInv1", transactions.Contains(arInvoice1));
			Assert("Collection should contain ARInv2", transactions.Contains(arInvoice2));
			Assert("Collection should contain ARInv3", transactions.Contains(arInvoice3));

			invoiceTypeFilter.Property = "CRD";
			transactions = new TransactionHeaderCollection(Factory, FilterBO.Filter);
			transactions.Load();
			AssertEquals("There should be 1 matching transactions", 1, transactions.Count);
			Assert("Collection should contain ARCred1", transactions.Contains(arCreditNote1));

			invoiceTypeFilter.Property = "JNL";
			transactions = new TransactionHeaderCollection(Factory, FilterBO.Filter);
			transactions.Load();
			AssertEquals("There should be 1 matching transactions", 1, transactions.Count);
			Assert("Collection should contain ARJnl", transactions.Contains(arJournal));
		}

		public void TestFilterByDisbursementType()
		{
			ModuleTextFilter invoiceTypeFilter = (ModuleTextFilter)FilterBO["Disbursement Invoice"];
			invoiceTypeFilter.IsActive = true;
			invoiceTypeFilter.Property = "DSB";
			var transactions = new TransactionHeaderCollection(Factory, FilterBO.Filter);
			transactions.Load();
			AssertEquals("There should be 2 matching transactions", 2, transactions.Count);
			Assert("Collection should contain ARInv2", transactions.Contains(arInvoice2));
			Assert("Collection should contain ARInv3", transactions.Contains(arInvoice3));

			invoiceTypeFilter.Property = "STD";
			transactions = new TransactionHeaderCollection(Factory, FilterBO.Filter);
			transactions.Load();
			AssertEquals("There should be 3 matching transactions", 3, transactions.Count);
			Assert("Collection should contain ARInv1", transactions.Contains(arInvoice1));
			Assert("Collection should contain ARCrd1", transactions.Contains(arCreditNote1));
			Assert("Collection should contain ARJnl", transactions.Contains(arJournal));

			invoiceTypeFilter.Property = "ALL";
			transactions = new TransactionHeaderCollection(Factory, FilterBO.Filter);
			transactions.Load();
			AssertEquals("There should be 5 matching transactions", 5, transactions.Count);
			Assert("Collection should contain ARInv1", transactions.Contains(arInvoice1));
			Assert("Collection should contain ARInv2", transactions.Contains(arInvoice2));
			Assert("Collection should contain ARInv3", transactions.Contains(arInvoice3));
			Assert("Collection should contain ARCrd1", transactions.Contains(arCreditNote1));
			Assert("Collection should contain ARJnl", transactions.Contains(arJournal));
		}

		TestObjectCreator creator;
		AddTransactionsToOrderFilterBusinessObject FilterBO;
		ARInvoice arInvoice1, arInvoice2, arInvoice3;
		APInvoice apInvoice1;
		ARCreditNote arCreditNote1;
		APCreditNote apCreditNote1;
		ARJournal arJournal;

		protected override void SetUp()
		{
			base.SetUp();
			creator = new TestObjectCreator(Factory);
			var date1 = new ZDateTime(2014, 10, 10);
			var date2 = new ZDateTime(2014, 12, 12);

			arInvoice1 = (ARInvoice)creator.CreateInvoiceWithLine(typeof(ARInvoice), "", creator.AUD, 1, 10, 0, 10, 0);
			arInvoice1.AH_OH = creator.ABIGAS.PK;
			arInvoice1.AH_DueDate = date1;
			arInvoice1.AH_TransactionCategory = "FIN";
			arInvoice2 = (ARInvoice)creator.CreateInvoiceWithLine(typeof(ARInvoice), "", creator.USD, 1, 10, 0, 10, 0);
			arInvoice2.AH_OH = creator.ABIGAS.PK;
			arInvoice2.AH_DueDate = date1;
			arInvoice2.AH_TransactionCategory = "DBD";
			arInvoice3 = (ARInvoice)creator.CreateInvoiceWithLine(typeof(ARInvoice), "", creator.USD, 1, 10, 0, 10, 0);
			arInvoice3.AH_OH = creator.AALSHI.PK;
			arInvoice3.AH_DueDate = date1;
			arInvoice3.AH_TransactionCategory = "DBT";
			apInvoice1 = (APInvoice)creator.CreateInvoiceWithLine(typeof(APInvoice), "Test001", creator.AUD, 1, 10, 0, 10, 0);
			apInvoice1.AH_OH = creator.AALSHI.PK;
			apInvoice1.AH_DueDate = date1;
			apInvoice1.AH_TransactionCategory = "FIN";
			arCreditNote1 = (ARCreditNote)creator.CreateInvoiceWithLine(typeof(ARCreditNote), "", creator.USD, 1, 10, 0, 10, 0);
			arCreditNote1.AH_OH = creator.AALSHI.PK;
			arCreditNote1.AH_DueDate = date2;
			arCreditNote1.AH_TransactionCategory = "FIN";
			apCreditNote1 = (APCreditNote)creator.CreateInvoiceWithLine(typeof(APCreditNote), "Test002", creator.USD, 1, 10, 0, 10, 0);
			apCreditNote1.AH_OH = creator.AALSHI.PK;
			apCreditNote1.AH_DueDate = date2;
			apCreditNote1.AH_TransactionCategory = "FIN";
			arJournal = creator.CreateJournal<ARJournal>(10, date1, creator.ABIGAS.PK);
			Factory.Save();
			FilterBO = (AddTransactionsToOrderFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AddTransactionsToOrderFilterBusinessObject();
		}
	}
}
