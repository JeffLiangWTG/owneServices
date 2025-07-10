using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.OrganisationBalance
{
	[TestedType(typeof(OrganisationBalance))]
	public class TestOutstandingBalance : NonPersistentBusinessObjectTestCase
	{
		OrganisationBalance TestOrgBal;

		protected override void SetUp()
		{
			TestOrgBal = new OrganisationBalance(ZGuid.Empty, ZArchitecture.Core.LedgerTypes.AccountsPayable);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrganisationBalance(ZGuid.Empty, ZArchitecture.Core.LedgerTypes.AccountsPayable);
		}

		public void TestGetSumForAP()
		{
			GlbBranch testBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			TestOrgBal.OrgPK = testOrg.PK;
			AssertEquals("The total outstanding should be 0", 0M, TestOrgBal.TotalOutstanding);
			AccTransactionHeader testContraRow = TestObjectCreator.InsertTransaction(ZArchitecture.Core.TransactionTypes.Contra);
			testContraRow.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			testContraRow.AH_OutstandingAmount = 60;
			testContraRow.AH_InvoiceAmount = 60;
			testContraRow.AH_OH = testOrg.PK;
			testContraRow.AH_IsCancelled = false;
			testContraRow.AH_TransactionNum = "$";
			testContraRow.AH_GB = testBranch.PK;
			Factory.Save();
			AccTransactionHeader testContraRow2 = TestObjectCreator.InsertTransaction(ZArchitecture.Core.TransactionTypes.Contra);
			testContraRow2.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			testContraRow2.AH_OutstandingAmount = 50;
			testContraRow2.AH_InvoiceAmount = 50;
			testContraRow2.AH_OH = testOrg.PK;
			testContraRow2.AH_IsCancelled = false;
			testContraRow2.AH_TransactionNum = "@";
			testContraRow2.AH_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			TestOrgBal = new OrganisationBalance(testOrg.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			AssertEquals("total outstanding for entered Organisation should be 50", -50m, TestOrgBal.TotalOutstanding);
			var testContraRow3 = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			testContraRow3.Line.Add(arInvoice);
			testContraRow3.AH_OutstandingAmount = 50;
			testContraRow3.AH_InvoiceAmount = 50;
			testContraRow3.AH_OH = testOrg.PK;
			testContraRow3.AH_IsCancelled = false;
			testContraRow3.AH_TransactionNum = "@";
			testContraRow3.AH_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			TestOrgBal = new OrganisationBalance(testOrg.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			AssertEquals("total outstanding for entered Organisation should be 50", -50m, TestOrgBal.TotalOutstanding);
			AccTransactionHeader testContraRow4 = TestObjectCreator.InsertTransaction(ZArchitecture.Core.TransactionTypes.Contra);
			testContraRow4.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			testContraRow4.AH_OutstandingAmount = 100;
			testContraRow4.AH_InvoiceAmount = 100;
			testContraRow4.AH_OH = testOrg.PK;
			testContraRow4.AH_IsCancelled = false;
			testContraRow4.AH_TransactionNum = "#";
			testContraRow4.AH_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			TestOrgBal = new OrganisationBalance(testOrg.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			AssertEquals("total outstanding for entered Organisation should be 150", -150m, TestOrgBal.TotalOutstanding);
		}

		public void TestGetSumForAR()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			OrganisationBalance testBalance = new OrganisationBalance(testOrg.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			AssertEquals("Total outstanding should be 0", 0M, testBalance.TotalOutstanding);
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, invoice.AH_ExchangeRate, 200m, 0m, 0m, 200m, 0m, 0m);
			invoice.AH_OH = testOrg.PK;
			ARReceipt receipt = Factory.NewWithValidTestData<ARReceipt>();
			receipt.AH_OSExTaxAmount = 100.00m;
			receipt.AH_OH = testOrg.PK;
			Factory.Save();
			testBalance = new OrganisationBalance(testOrg.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			AssertEquals("Total outstanding should be 100", 100m, testBalance.TotalOutstanding);
			ARInvoice invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			TestObjectCreator.CreateInvoiceLine(invoice2, invoice2.TransactionCurrency, invoice2.AH_ExchangeRate, 300m, 0m, 0m, 300m, 0m, 0m);
			invoice2.AH_OH = testOrg.PK;
			ARReceipt receipt2 = Factory.NewWithValidTestData<ARReceipt>();
			receipt2.AH_OSExTaxAmount = 150.00m;
			receipt2.AH_OH = testOrg.PK;
			Factory.Save();
			testBalance = new OrganisationBalance(testOrg.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			AssertEquals("Total outstanding should be 250", 250m, testBalance.TotalOutstanding);
			ARInvoice invoice3 = Factory.NewWithValidTestData<ARInvoice>();
			invoice3.AH_OH = testOrg.PK;
			invoice3.AH_TransactionType = TransactionTypes.InvoiceBatch;
			ARReceipt receipt3 = Factory.NewWithValidTestData<ARReceipt>();
			receipt3.AH_OSExTaxAmount = 100.00m;
			receipt3.AH_OH = testOrg.PK;
			Factory.Save();
			testBalance = new OrganisationBalance(testOrg.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			AssertEquals("Total outstanding should be 150", 150m, testBalance.TotalOutstanding);
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}

				return fTestObjectCreator;
			}
		}

		protected TestObjectCreator fTestObjectCreator;
	}
}
