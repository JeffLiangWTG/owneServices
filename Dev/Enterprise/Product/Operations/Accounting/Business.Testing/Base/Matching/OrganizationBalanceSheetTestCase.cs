using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(OrganizationBalanceSheet))]
	public class OrganizationBalanceSheetTestCase : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected OrganizationBalanceSheet TestOrgBalanceSheet;
		protected OrgHeader TestOrg1;
		protected OrgHeader TestOrg2;
		protected OrgHeader TestOrg3;
		protected OrgHeader TestOrg4;
		protected OrgHeader TestOrg5;
		protected OrgHeader TestOrg6;

		// Note: numbers at the end of transaction variable names 
		// refer to the organization of the transaction

		// Test AR Receipts
		protected ARReceipt TestARRec;
		protected ARReceipt TestARRec2;
		protected ARReceipt TestARRec3;
		protected ARReceipt TestARRec4;

		// Test AR Invoices
		protected ARInvoice TestARInv;
		protected ARInvoice TestARInv2;
		protected ARInvoice TestARInv3;
		protected ARInvoice TestARInv4;
		protected ARInvoice TestARInv4_2;

		// Test AR CreditNotes
		protected ARCreditNote ARCRD1;
		protected ARCreditNote ARCRD2;
		protected ARCreditNote ARCRD3;

		// Test AP Payments 
		protected APPayment TestAPPay;
		protected APPayment TestAPPay2;
		protected APPayment TestAPPay3;

		// Test AP Invoices
		protected APInvoice TestAPInv;
		protected APInvoice TestAPInv2;
		protected APInvoice TestAPInv3;
		protected APInvoice TestAPInv5;
		protected APInvoice TestAPInv5_2;

		// Test AP CreditNotes
		protected APCreditNote APCRD1;
		protected APCreditNote APCRD2;
		protected APCreditNote APCRD3;
		protected APCreditNote APCRD6;
		protected APCreditNote APCRD6_2;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrganizationBalanceSheet(ZGuid.Empty, ZString.Empty, Factory);
		}

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();

			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg4 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg5 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg6 = Factory.NewWithValidTestData<OrgHeader>();

			// Test AR Receipts
			TestARRec = Factory.NewWithValidTestData<ARReceipt>();
			TestARRec2 = Factory.New<ARReceipt>();
			TestARRec3 = Factory.New<ARReceipt>();
			TestARRec4 = Factory.New<ARReceipt>();

			// Test AR Invoices
			TestARInv = Factory.NewWithValidTestData<ARInvoice>();
			TestARInv2 = Factory.NewWithValidTestData<ARInvoice>();
			TestARInv3 = Factory.NewWithValidTestData<ARInvoice>();
			TestARInv4 = Factory.NewWithValidTestData<ARInvoice>();

			// Test AP Payments
			TestAPPay = Factory.NewWithValidTestData<APPayment>();
			TestAPPay2 = Factory.New<APPayment>();
			TestAPPay3 = Factory.New<APPayment>();

			// Test AP Invoices
			TestAPInv = Factory.NewWithValidTestData<APInvoice>();
			TestAPInv2 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInv3 = Factory.NewWithValidTestData<APInvoice>();
		}

		#endregion

		#region SetupTestDataSet1

		protected void SetupTestDataSet1(ZString primaryLedger)
		{
			TestOrgBalanceSheet = new OrganizationBalanceSheet(TestOrg1.PK, primaryLedger, Factory);

			// Test Org 1
			ARCRD1 = Factory.NewWithValidTestData<ARCreditNote>();
			ARCRD1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(ARCRD1, ARCRD1.TransactionCurrency, ARCRD1.AH_ExchangeRate, 12.34m, 0m, 0m, 12.34m, 0m, 0m);

			TestAPInv.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInv, TestAPInv.TransactionCurrency, TestAPInv.AH_ExchangeRate, 20.65m, 0m, 0m, 20.65m, 0m, 0m);

			APCRD1 = Factory.NewWithValidTestData<APCreditNote>();
			APCRD1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(APCRD1, APCRD1.TransactionCurrency, APCRD1.AH_ExchangeRate, 31.73m, 0m, 0m, 31.73m, 0m, 0m);

			// Test Org 2
			ARCRD2 = Factory.NewWithValidTestData<ARCreditNote>();
			ARCRD2.AH_OH = TestOrg2.PK;
			TestObjectCreator.CreateInvoiceLine(ARCRD2, ARCRD2.TransactionCurrency, ARCRD2.AH_ExchangeRate, 14.56m, 0m, 0m, 14.56m, 0m, 0m);

			TestARInv2.AH_OH = TestOrg2.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInv2, TestARInv2.TransactionCurrency, TestARInv2.AH_ExchangeRate, 65.09m, 0m, 0m, 65.09m, 0m, 0m);

			TestAPInv2.AH_OH = TestOrg2.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInv2, TestAPInv2.TransactionCurrency, TestAPInv2.AH_ExchangeRate, 52.34m, 0m, 0m, 52.34m, 0m, 0m);

			APCRD2 = Factory.NewWithValidTestData<APCreditNote>();
			APCRD2.AH_OH = TestOrg2.PK;
			TestObjectCreator.CreateInvoiceLine(APCRD2, APCRD2.TransactionCurrency, APCRD2.AH_ExchangeRate, 34.09m, 0m, 0m, 34.09m, 0m, 0m);

			// Test Org 3
			ARCRD3 = Factory.NewWithValidTestData<ARCreditNote>();
			ARCRD3.AH_OH = TestOrg3.PK;
			TestObjectCreator.CreateInvoiceLine(ARCRD3, ARCRD3.TransactionCurrency, ARCRD3.AH_ExchangeRate, 100.9m, 0m, 0m, 100.9m, 0m, 0m);

			APCRD3 = Factory.NewWithValidTestData<APCreditNote>();
			APCRD3.AH_OH = TestOrg3.PK;
			TestObjectCreator.CreateInvoiceLine(APCRD3, APCRD3.TransactionCurrency, APCRD3.AH_ExchangeRate, 5.02m, 0m, 0m, 5.02m, 0m, 0m);

			// Test Org 4
			TestARInv4.AH_OH = TestOrg4.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInv4, TestARInv4.TransactionCurrency, TestARInv4.AH_ExchangeRate, 15.15m, 0m, 0m, 15.15m, 0m, 0m);

			TestARInv4_2 = Factory.NewWithValidTestData<ARInvoice>();
			TestARInv4_2.AH_OH = TestOrg4.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInv4_2, TestARInv4_2.TransactionCurrency, TestARInv4_2.AH_ExchangeRate, 25.25m, 0m, 0m, 25.25m, 0m, 0m);

			// Test Org 5 
			TestAPInv5 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInv5.AH_OH = TestOrg5.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInv5, TestAPInv5.TransactionCurrency, TestAPInv5.AH_ExchangeRate, 12.12m, 0m, 0m, 12.12m, 0m, 0m);

			TestAPInv5_2 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInv5_2.AH_OH = TestOrg5.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInv5_2, TestAPInv5_2.TransactionCurrency, TestAPInv5_2.AH_ExchangeRate, 22.22m, 0m, 0m, 22.22m, 0m, 0m);

			// Test Org 6
			APCRD6 = Factory.NewWithValidTestData<APCreditNote>();
			APCRD6.AH_OH = TestOrg6.PK;
			TestObjectCreator.CreateInvoiceLine(APCRD6, APCRD6.TransactionCurrency, APCRD6.AH_ExchangeRate, 10.10m, 0m, 0m, 10.10m, 0m, 0m);

			APCRD6_2 = Factory.NewWithValidTestData<APCreditNote>();
			APCRD6_2.AH_OH = TestOrg6.PK;
			TestObjectCreator.CreateInvoiceLine(APCRD6_2, APCRD6_2.TransactionCurrency, APCRD6_2.AH_ExchangeRate, 20.20m, 0m, 0m, 20.20m, 0m, 0m);

			/*
			 * Adding the transactions to the MatchingCollection
			 */

			// Test Org 2
			TestOrgBalanceSheet.AddIMatching(ARCRD2);
			TestOrgBalanceSheet.AddIMatching(TestARInv2);
			TestOrgBalanceSheet.AddIMatching(TestAPInv2);
			TestOrgBalanceSheet.AddIMatching(APCRD2);

			// Test Org 3
			TestOrgBalanceSheet.AddIMatching(ARCRD3);
			TestOrgBalanceSheet.AddIMatching(APCRD3);

			// Test Org 4
			TestOrgBalanceSheet.AddIMatching(TestARInv4);
			TestOrgBalanceSheet.AddIMatching(TestARInv4_2);

			// Test Org 5
			TestOrgBalanceSheet.AddIMatching(TestAPInv5);
			TestOrgBalanceSheet.AddIMatching(TestAPInv5_2);

			// Test Org 6
			TestOrgBalanceSheet.AddIMatching(APCRD6);
			TestOrgBalanceSheet.AddIMatching(APCRD6_2);
		}

		#endregion

		#endregion

		#region TestGetUniqueOrganizations

		public void TestGetUniqueOrganizations()
		{
			TestOrgBalanceSheet = new OrganizationBalanceSheet(TestOrg1.PK, ZString.Empty, Factory);
			IMatchingCollection existing = new IMatchingCollection(Factory);

			TestARRec.AH_OH = TestOrg1.PK;
			existing.Add(TestARRec);

			TestARRec2.AH_OH = TestOrg1.PK;
			existing.Add(TestARRec2);

			IMatchingCollection offset = new IMatchingCollection(Factory);

			TestARInv.AH_OH = TestOrg2.PK;
			offset.Add(TestARInv);

			TestARInv2.AH_OH = TestOrg3.PK;
			offset.Add(TestARInv2);

			TestOrgBalanceSheet.AddIMatchingCollection(existing);
			TestOrgBalanceSheet.AddIMatchingCollection(offset);
			List<ZGuid> uniOrg = TestOrgBalanceSheet.GetUniqueOrganizations_ForTestOnly();
			AssertEquals("Should be 3 orgs", 3, uniOrg.Count);
		}

		#endregion

		#region TestGetTransferCollectionWithAPTransfers

		public void TestGetTransferCollectionWithAPTransfers()
		{
			TestOrgBalanceSheet = new OrganizationBalanceSheet(TestOrg1.PK, ZString.Empty, Factory);

			TestAPPay.AH_OH = TestOrg1.PK;
			TestAPPay.AH_LocalExTaxAmount = 45.67M;
			TestAPPay.AH_OSTotalAmount = 45.67M;

			TestAPPay2.AH_OH = TestOrg2.PK;
			TestAPPay2.AH_LocalExTaxAmount = 98.31M;
			TestAPPay2.AH_OSTotalAmount = 98.31M;

			TestAPPay3.AH_OH = TestOrg3.PK;
			TestAPPay3.AH_LocalExTaxAmount = 29.01M;
			TestAPPay3.AH_OSTotalAmount = 29.01M;

			TestAPInv.AH_OH = TestOrg1.PK;
			TestAPInv.AH_LocalExTaxAmount = 70.38M;
			TestAPInv.AH_OSTotalAmount = 70.38M;

			TestAPInv2.AH_OH = TestOrg2.PK;
			TestAPInv2.AH_LocalExTaxAmount = 30.09M;
			TestAPInv2.AH_OSTotalAmount = 30.09M;

			TestAPInv3.AH_OH = TestOrg3.PK;
			TestAPInv3.AH_LocalExTaxAmount = 72.52M;
			TestAPInv3.AH_OSTotalAmount = 72.52M;

			/*
			 TestOrg1 Balance is -24.71
			 TestOrg2 Balance is 68.22
			 TestOrg3 Balance is -43.51
			*/

			TestOrgBalanceSheet.AddIMatching(TestAPPay);
			TestOrgBalanceSheet.AddIMatching(TestAPPay2);
			TestOrgBalanceSheet.AddIMatching(TestAPPay3);
			TestOrgBalanceSheet.AddIMatching(TestAPInv);
			TestOrgBalanceSheet.AddIMatching(TestAPInv2);
			TestOrgBalanceSheet.AddIMatching(TestAPInv3);
			TestOrgBalanceSheet.Transactions.SetPartialPaidAmount();

			IMatchingCollection transfers = TestOrgBalanceSheet.CreateDynamicTransactions();

			AssertEquals("There should be 2 Transfers", 2, transfers.Count);

			TransferFilter filter1 = new TransferFilter();
			filter1.TransferLedger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			filter1.TransferFromOrg = TestOrg2.PK;
			filter1.TransferFromAmount = -68.22M;
			filter1.TransferToOrg = TestOrg1.PK;
			filter1.TransferToAmount = 68.22M;
			Transfer matchingTransfer1 = transfers.GetMatchingTransfer(filter1);

			AssertNotNull("Collection should contain a Transfer with the above values", matchingTransfer1);
			AssertEquals("Description should be as shown", "AP TRANSFER FROM " + TestOrg2.OH_Code + " TO " + TestOrg1.OH_Code + " (SYSTEM GENERATED)", matchingTransfer1.AH_Desc);
			AssertEquals("OSTotal of From Row should be -68.22", -68.22M, matchingTransfer1.TransferFrom.AH_OSTotal);
			AssertEquals("OSTotal of To Row should be 68.22", 68.22M, matchingTransfer1.TransferTo.AH_OSTotal);
			AssertEquals("OutstandingAmount of From Row should be -68.22", -68.22M, matchingTransfer1.TransferFrom.AH_OutstandingAmount);
			AssertEquals("OutstandingAmount of To Row should be 68.22", 68.22M, matchingTransfer1.TransferTo.AH_OutstandingAmount);

			TransferFilter filter2 = new TransferFilter();
			filter2.TransferLedger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			filter2.TransferFromOrg = TestOrg3.PK;
			filter2.TransferFromAmount = 43.51M;
			filter2.TransferToOrg = TestOrg1.PK;
			filter2.TransferToAmount = -43.51M;
			Transfer matchingTransfer2 = transfers.GetMatchingTransfer(filter2);

			AssertNotNull("Collection should contain a Transfer with the above values", matchingTransfer2);
			AssertEquals("Description should be as shown", "AP TRANSFER FROM " + TestOrg3.OH_Code + " TO " + TestOrg1.OH_Code + " (SYSTEM GENERATED)", matchingTransfer2.AH_Desc);
			AssertEquals("OSTotal of From Row should be 43.51", 43.51M, matchingTransfer2.TransferFrom.AH_OSTotal);
			AssertEquals("OSTotal of To Row should be -43.51", -43.51M, matchingTransfer2.TransferTo.AH_OSTotal);
			AssertEquals("OutstandingAmount of From Row should be 43.51", 43.51M, matchingTransfer2.TransferFrom.AH_OutstandingAmount);
			AssertEquals("OutstandingAmount of To Row should be -43.51", -43.51M, matchingTransfer2.TransferTo.AH_OutstandingAmount);
		}

		#endregion

		#region TestGetTransferCollectionWith3OrgsAndARTransfers

		public void TestGetTransferCollectionWith3OrgsAndARTransfers()
		{
			TestOrgBalanceSheet = new OrganizationBalanceSheet(TestOrg2.PK, ZString.Empty, Factory);
			IMatchingCollection existing = new IMatchingCollection(Factory);

			TestARRec.AH_OH = TestOrg1.PK;
			TestARRec.AH_LocalExTaxAmount = 180M;
			TestARRec.AH_OSTotalAmount = 180M;
			existing.Add(TestARRec);

			TestARRec2.AH_OH = TestOrg1.PK;
			TestARRec2.AH_LocalExTaxAmount = 340M;
			TestARRec2.AH_OSTotalAmount = 340M;
			existing.Add(TestARRec2);

			TestARRec3.AH_OH = TestOrg2.PK;
			TestARRec3.AH_LocalExTaxAmount = 190M;
			TestARRec3.AH_OSTotalAmount = 190M;
			existing.Add(TestARRec3);

			TestARRec4.AH_OH = TestOrg2.PK;
			TestARRec4.AH_LocalExTaxAmount = 380M;
			TestARRec4.AH_OSTotalAmount = 380M;
			existing.Add(TestARRec4);

			IMatchingCollection offset = new IMatchingCollection(Factory);

			TestARInv.AH_OH = TestOrg2.PK;
			TestARInv.AH_LocalExTaxAmount = 450M;
			TestARInv.AH_OSTotalAmount = 450M;
			offset.Add(TestARInv);

			TestARInv2.AH_OH = TestOrg3.PK;
			TestARInv2.AH_LocalExTaxAmount = 560M;
			TestARInv2.AH_OSTotalAmount = 560M;
			offset.Add(TestARInv2);

			TestARInv3.AH_OH = TestOrg1.PK;
			TestARInv3.AH_LocalExTaxAmount = 80M;
			TestARInv3.AH_OSTotalAmount = 80M;
			offset.Add(TestARInv3);

			/*
			 * TestOrg1 Balance is -440
			 * TestOrg2 Balance is -120
			 * TestOrg3 Balance is 560
			 */

			TestOrgBalanceSheet.AddIMatchingCollection(existing);
			TestOrgBalanceSheet.AddIMatchingCollection(offset);
			TestOrgBalanceSheet.Transactions.SetPartialPaidAmount();

			TestOrgBalanceSheet.CreateSubBalances();
			IMatchingCollection transfers = TestOrgBalanceSheet.CreateDynamicTransactions();
			AssertEquals("Collection should contain 2 Transfers", 2, transfers.Count);

			TransferFilter filter1 = new TransferFilter();
			filter1.TransferLedger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			filter1.TransferFromAmount = 440M;
			filter1.TransferFromOrg = TestOrg1.PK;
			filter1.TransferToAmount = -440M;
			filter1.TransferToOrg = TestOrg2.PK;

			AssertNotNull("There should be a transfer with the above values in the collection", transfers.GetMatchingTransfer(filter1));

			TransferFilter filter2 = new TransferFilter();
			filter2.TransferLedger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			filter2.TransferFromAmount = -560M;
			filter2.TransferFromOrg = TestOrg3.PK;
			filter2.TransferToAmount = 560M;
			filter2.TransferToOrg = TestOrg2.PK;

			AssertNotNull("There should be a transfer with the above values in the collection", transfers.GetMatchingTransfer(filter2));
		}

		#endregion

		#region TestGetTransferCollectionWith4OrgsAndARTransfers

		public void TestGetTransferCollectionWith4OrgsAndARTransfers()
		{
			TestOrgBalanceSheet = new OrganizationBalanceSheet(TestOrg4.PK, ZString.Empty, Factory);
			IMatchingCollection offset = new IMatchingCollection(Factory);

			TestARRec.AH_OH = TestOrg1.PK;
			TestARRec.AH_LocalExTaxAmount = 34.69M;
			TestARRec.AH_OSTotalAmount = 34.69M;
			offset.Add(TestARRec);

			TestARRec2.AH_OH = TestOrg2.PK;
			TestARRec2.AH_LocalExTaxAmount = 56.51M;
			TestARRec2.AH_OSTotalAmount = 56.51M;
			offset.Add(TestARRec2);

			TestARRec3.AH_OH = TestOrg3.PK;
			TestARRec3.AH_LocalExTaxAmount = 93.07M;
			TestARRec3.AH_OSTotalAmount = 93.07M;
			offset.Add(TestARRec3);

			TestARRec4.AH_OH = TestOrg4.PK;
			TestARRec4.AH_LocalExTaxAmount = 107.46M;
			TestARRec4.AH_OSTotalAmount = 107.46M;
			offset.Add(TestARRec4);

			IMatchingCollection existing = new IMatchingCollection(Factory);

			TestARInv.AH_OH = TestOrg1.PK;
			TestARInv.AH_LocalExTaxAmount = 39.70M;
			TestARInv.AH_OSTotalAmount = 39.70M;
			existing.Add(TestARInv);

			TestARInv2.AH_OH = TestOrg2.PK;
			TestARInv2.AH_LocalExTaxAmount = 158.35M;
			TestARInv2.AH_OSTotalAmount = 158.35M;
			existing.Add(TestARInv2);

			TestARInv3.AH_OH = TestOrg3.PK;
			TestARInv3.AH_LocalExTaxAmount = 53.68M;
			TestARInv3.AH_OSTotalAmount = 53.68M;
			existing.Add(TestARInv3);

			TestARInv4.AH_OH = TestOrg4.PK;
			TestARInv4.AH_LocalExTaxAmount = 40M;
			TestARInv4.AH_OSTotalAmount = 40M;
			existing.Add(TestARInv4);

			/*
			 * TestOrg1 Balance is 5.01
			 * TestOrg2 Balance is 101.84
			 * TestOrg3 Balance is -39.39
			 * TestOrg4 Balance is -67.46
			 */

			TestOrgBalanceSheet.AddIMatchingCollection(existing);
			TestOrgBalanceSheet.AddIMatchingCollection(offset);
			TestOrgBalanceSheet.Transactions.SetPartialPaidAmount();

			TestOrgBalanceSheet.CreateSubBalances();
			IMatchingCollection transfers = TestOrgBalanceSheet.CreateDynamicTransactions();
			AssertEquals("Collection should contain 3 Transfers", 3, transfers.Count);

			//				TestOrgBalanceSheet.AddIMatchingCollection(Transfers);
			//				AssertEquals("Balance for TestOrg1 should be 0", 0M, TestOrgBalanceSheet.Transactions.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			//				AssertEquals("Balance for TestOrg2 should be 0", 0M, TestOrgBalanceSheet.Transactions.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			//				AssertEquals("Balance for TestOrg3 should be 0", 0M, TestOrgBalanceSheet.Transactions.GetOrganizationBalanceAmount(TestOrg3.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			//				AssertEquals("Balance for TestOrg4 should be 0", 0M, TestOrgBalanceSheet.Transactions.GetOrganizationBalanceAmount(TestOrg4.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));

			TransferFilter org1Filter = new TransferFilter();
			org1Filter.TransferLedger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			org1Filter.TransferFromOrg = TestOrg1.PK;
			org1Filter.TransferFromAmount = -5.01M;
			org1Filter.TransferToOrg = TestOrg4.PK;
			org1Filter.TransferToAmount = 5.01M;
			AssertNotNull("ARTransfer from Org1 to Org4 should exist", transfers.GetMatchingTransfer(org1Filter));

			TransferFilter org2Filter = new TransferFilter();
			org2Filter.TransferLedger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			org2Filter.TransferFromOrg = TestOrg2.PK;
			org2Filter.TransferFromAmount = -101.84M;
			org2Filter.TransferToOrg = TestOrg4.PK;
			org2Filter.TransferToAmount = 101.84M;
			AssertNotNull("ARTransfer from Org2 to Org4 should exist", transfers.GetMatchingTransfer(org2Filter));

			TransferFilter org3Filter = new TransferFilter();
			org3Filter.TransferLedger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			org3Filter.TransferFromOrg = TestOrg3.PK;
			org3Filter.TransferFromAmount = 39.39M;
			org3Filter.TransferToOrg = TestOrg4.PK;
			org3Filter.TransferToAmount = -39.39M;
			AssertNotNull("ARTransfer from Org3 to Org4 should exist", transfers.GetMatchingTransfer(org3Filter));
		}

		#endregion

		#region TestCreate2SubBalances

		public void TestCreate2SubBalances()
		{
			// Note: Expect that OrgBalances will be created for ALL organizations
			TestOrgBalanceSheet = new OrganizationBalanceSheet(TestOrg1.PK, ZString.Empty, Factory);

			IMatchingCollection existing = new IMatchingCollection(Factory);

			TestARRec.AH_OH = TestOrg1.PK;
			TestARRec.AH_LocalExTaxAmount = 180M;
			TestARRec.AH_OSTotalAmount = 180M;
			existing.Add(TestARRec);

			TestARRec2.AH_OH = TestOrg1.PK;
			TestARRec2.AH_LocalExTaxAmount = 340M;
			TestARRec2.AH_OSTotalAmount = 340M;
			existing.Add(TestARRec2);

			TestARRec3.AH_OH = TestOrg2.PK;
			TestARRec3.AH_LocalExTaxAmount = 70M;
			TestARRec3.AH_OSTotalAmount = 70M;
			existing.Add(TestARRec3);

			TestARRec4.AH_OH = TestOrg2.PK;
			TestARRec4.AH_LocalExTaxAmount = 380M;
			TestARRec4.AH_OSTotalAmount = 380M;
			existing.Add(TestARRec4);

			IMatchingCollection offset = new IMatchingCollection(Factory);

			TestARInv.AH_OH = TestOrg2.PK;
			TestARInv.AH_LocalExTaxAmount = 450M;
			TestARInv.AH_OSTotalAmount = 450M;
			offset.Add(TestARInv);

			TestARInv2.AH_OH = TestOrg3.PK;
			TestARInv2.AH_LocalExTaxAmount = 560M;
			TestARInv2.AH_OSTotalAmount = 560M;
			offset.Add(TestARInv2);

			TestARInv3.AH_OH = TestOrg1.PK;
			TestARInv3.AH_LocalExTaxAmount = 80M;
			TestARInv3.AH_OSTotalAmount = 80M;
			offset.Add(TestARInv3);

			TestOrgBalanceSheet.AddIMatchingCollection(existing);
			TestOrgBalanceSheet.AddIMatchingCollection(offset);
			TestOrgBalanceSheet.Transactions.SetPartialPaidAmount();
			TestOrgBalanceSheet.CreateSubBalances();

			AssertEquals("Should be 2 orgs ", 2, TestOrgBalanceSheet.ARSubBalances.Count);
			AssertNotNull("The org should be TestOrg3", TestOrgBalanceSheet.ARSubBalances.GetOrganizationSubBalance(TestOrg3.PK));
			AssertEquals("The balance amount should be 560", 560M, TestOrgBalanceSheet.ARSubBalances.GetOrganizationSubBalance(TestOrg3.PK).Amount);

			AssertNotNull("One org should be TestOrg1", TestOrgBalanceSheet.ARSubBalances.GetOrganizationSubBalance(TestOrg1.PK));
			AssertEquals("Balance of TestOrg1 should be -440", -440M, TestOrgBalanceSheet.ARSubBalances.GetOrganizationSubBalance(TestOrg1.PK).Amount);
		}

		#endregion

		#region TestCreate3SubBalances

		public void TestCreate3SubBalances()
		{
			// Note: Expect that OrgBalances will be created for ALL organizations
			TestOrgBalanceSheet = new OrganizationBalanceSheet(TestOrg2.PK, ZString.Empty, Factory);
			IMatchingCollection existing = new IMatchingCollection(Factory);

			TestARRec.AH_OH = TestOrg1.PK;
			TestARRec.AH_LocalOutstandingAmount = 40.89M;
			TestARRec.AH_OSTotalAmount = 40.89M;
			existing.Add(TestARRec);

			TestARRec2.AH_OH = TestOrg2.PK;
			TestARRec2.AH_LocalOutstandingAmount = 36.24M;
			TestARRec2.AH_OSTotalAmount = 36.24M;
			existing.Add(TestARRec2);

			TestARRec3.AH_OH = TestOrg3.PK;
			TestARRec3.AH_OSTotalAmount = 70.90M;
			TestARRec3.AH_LocalOutstandingAmount = 70.90M;
			existing.Add(TestARRec3);

			TestARRec4.AH_OH = TestOrg2.PK;
			TestARRec4.AH_OSTotalAmount = 38.61M;
			TestARRec4.AH_LocalOutstandingAmount = 38.61M;
			existing.Add(TestARRec4);

			IMatchingCollection offset = new IMatchingCollection(Factory);

			TestARInv.AH_OH = TestOrg2.PK;
			TestARInv.AH_OSTotalAmount = 150.39M;
			TestARInv.AH_LocalOutstandingAmount = 150.39M;
			offset.Add(TestARInv);

			TestARInv2.AH_OH = TestOrg3.PK;
			TestARInv2.AH_OSTotalAmount = 60.04M;
			TestARInv2.AH_LocalOutstandingAmount = 60.04M;
			offset.Add(TestARInv2);

			TestARInv3.AH_OH = TestOrg1.PK;
			TestARInv3.AH_OSTotalAmount = 80.94M;
			TestARInv3.AH_LocalOutstandingAmount = 80.94M;
			offset.Add(TestARInv3);

			TestOrgBalanceSheet.AddIMatchingCollection(existing);
			TestOrgBalanceSheet.AddIMatchingCollection(offset);
			TestOrgBalanceSheet.Transactions.SetPartialPaidAmount();
			TestOrgBalanceSheet.CreateSubBalances();

			AssertEquals("Should be 3 OrgSubBalances", 3, TestOrgBalanceSheet.ARSubBalances.Count);
			AssertNotNull("One org should be TestOrg1", TestOrgBalanceSheet.ARSubBalances.GetOrganizationSubBalance(TestOrg1.PK));
			AssertEquals("The balance amount should be 40.05", 40.05M, TestOrgBalanceSheet.ARSubBalances.GetOrganizationSubBalance(TestOrg1.PK).Amount);

			AssertNotNull("One org should be TestOrg2", TestOrgBalanceSheet.ARSubBalances.GetOrganizationSubBalance(TestOrg2.PK));
			AssertEquals("The balance amount should be 75.54", 75.54M, TestOrgBalanceSheet.ARSubBalances.GetOrganizationSubBalance(TestOrg2.PK).Amount);

			AssertNotNull("One org should be TestOrg3", TestOrgBalanceSheet.ARSubBalances.GetOrganizationSubBalance(TestOrg3.PK));
			AssertEquals("Balance of TestOrg3 should be -10.86", -10.86M, TestOrgBalanceSheet.ARSubBalances.GetOrganizationSubBalance(TestOrg3.PK).Amount);
		}

		#endregion

		#region TestMatchingARInvoiceReceiptAgainst2APInvoices

		public void TestMatchingARInvoiceReceiptAgainst2APInvoices()
		{
			// TestOrg1 is the primary organization
			TestOrgBalanceSheet = new OrganizationBalanceSheet(TestOrg1.PK, ZString.Empty, Factory);

			TestARInv.AH_OH = TestOrg1.PK;
			TestARInv.AH_LocalExTaxAmount = 70M;
			TestARInv.AH_OSTotalAmount = 70M;

			TestAPInv.AH_OH = TestOrg1.PK;
			TestAPInv.AH_LocalExTaxAmount = 30M;
			TestAPInv.AH_OSTotalAmount = 30M;

			TestAPInv2.AH_OH = TestOrg2.PK;
			TestAPInv2.AH_LocalExTaxAmount = 10M;
			TestAPInv2.AH_OSTotalAmount = 10M;

			TestARRec.AH_OH = TestOrg3.PK;
			TestARRec.AH_LocalExTaxAmount = 30M;
			TestARRec.AH_OSTotalAmount = 30M;

			TestOrgBalanceSheet.AddIMatching(TestARInv);
			TestOrgBalanceSheet.AddIMatching(TestAPInv);
			TestOrgBalanceSheet.AddIMatching(TestAPInv2);
			TestOrgBalanceSheet.AddIMatching(TestARRec);
			TestOrgBalanceSheet.Transactions.SetPartialPaidAmount();

			IMatchingCollection dynamicTransactions = TestOrgBalanceSheet.CreateDynamicTransactions();

			TransferFilter transferFilter1 = new TransferFilter();
			transferFilter1.TransferFromOrg = TestOrg3.PK;
			transferFilter1.TransferFromAmount = 30M;
			transferFilter1.TransferToOrg = TestOrg1.PK;
			transferFilter1.TransferToAmount = -30M;
			transferFilter1.TransferLedger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;

			AssertNotNull("AR transfer from TestOrg3 to TestOrg1 should be created", dynamicTransactions.GetMatchingTransfer(transferFilter1));

			ContraFilter contraFilter1 = new ContraFilter();
			contraFilter1.APOrg = TestOrg1.PK;
			contraFilter1.APAmount = 30M;
			contraFilter1.AROrg = TestOrg1.PK;
			contraFilter1.ARAmount = -30M;

			AssertNotNull("Contra from TestOrg1 to TestOrg1 should be created", dynamicTransactions.GetMatchingContra(contraFilter1));

			ContraFilter contraFilter2 = new ContraFilter();
			contraFilter2.APOrg = TestOrg2.PK;
			contraFilter2.APAmount = 10M;
			contraFilter2.AROrg = TestOrg1.PK;
			contraFilter2.ARAmount = -10M;

			AssertNotNull("Contra from TestOrg2 to TestOrg1 should be created", dynamicTransactions.GetMatchingContra(contraFilter2));

			IMatchingCollection testIMatchings = new IMatchingCollection(Factory);
			testIMatchings.AddRange(TestOrgBalanceSheet.Transactions);
			testIMatchings.AddRange(dynamicTransactions);

			AssertEquals("Balance for TestOrg1 in AR should be 0", 0M, testIMatchings.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance for TestOrg2 in AR should be 0", 0M, testIMatchings.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance for TestOrg1 in AP should be 0", 0M, testIMatchings.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance for TestOrg2 in AP should be 0", 0M, testIMatchings.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));

			AssertEquals("Balance for AP should be 0", 0M, testIMatchings.GetLedgerBalanceAmount(ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance for AR should be 0", 0M, testIMatchings.GetLedgerBalanceAmount(ZArchitecture.Core.LedgerTypes.AccountsReceivable));
		}

		#endregion

		#region TestMatchingARInvoiceAndAPInvoiceFromDifferentOrg

		public void TestMatchingARInvoiceAPInvoiceFromDifferentOrg()
		{
			TestOrgBalanceSheet = new OrganizationBalanceSheet(TestOrg1.PK, ZString.Empty, Factory);

			TestARInv.AH_OH = TestOrg1.PK;
			TestARInv.AH_LocalExTaxAmount = 23.45M;
			TestARInv.AH_OSTotalAmount = 23.45M;

			TestAPInv.AH_OH = TestOrg2.PK;
			TestAPInv.AH_LocalExTaxAmount = 23.45M;
			TestAPInv.AH_OSTotalAmount = 23.45M;

			TestOrgBalanceSheet.AddIMatching(TestARInv);
			TestOrgBalanceSheet.AddIMatching(TestAPInv);
			TestOrgBalanceSheet.Transactions.SetPartialPaidAmount();

			IMatchingCollection dynamicTransactions = TestOrgBalanceSheet.CreateDynamicTransactions();

			Assert("One Contra should be dynamically created", dynamicTransactions[0] is Contra);
			Contra dynamicContra = (Contra)dynamicTransactions[0];

			AssertEquals("APContraRow should have InvoiceAmount equal to +23.45", 23.45M, dynamicContra.APRow.AH_InvoiceAmount);
			AssertEquals("APContraRow should have OutstandingAmt equal to +23.45", 23.45M, dynamicContra.APRow.AH_OutstandingAmount);
			AssertEquals("APContraRow should have OSTotal equal to 23.45", 23.45M, dynamicContra.APRow.AH_OSTotal);
			AssertEquals("APContraRow should have TestOrg2 as the organization", TestOrg2.PK, dynamicContra.APRow.AH_OH);

			AssertEquals("ARContraRow should have InvoiceAmount equal to -23.45", -23.45M, dynamicContra.ARRow.AH_InvoiceAmount);
			AssertEquals("ARContraRow should have OutstandingAmt equal to -23.45", -23.45M, dynamicContra.ARRow.AH_OutstandingAmount);
			AssertEquals("ARContraRow should have OSTotal equal to -23.45", -23.45M, dynamicContra.ARRow.AH_OSTotal);
			AssertEquals("ARContraRow should have TestOrg1 as the organization", TestOrg1.PK, dynamicContra.ARRow.AH_OH);
		}

		#endregion

		#region TestMatchingContrasAndInvoicesFromBothLedgers

		public void TestMatchingContrasAndInvoicesFromBothLedgers()
		{
			TestOrgBalanceSheet = new OrganizationBalanceSheet(TestOrg3.PK, ZString.Empty, Factory);

			Contra testContra1 = Contra.New(Factory);
			testContra1.APRow.AH_OH = TestOrg1.PK;
			testContra1.APRow.AH_LocalExTaxAmount = 48M;
			testContra1.APRow.AH_OSTotalAmount = 48M;

			testContra1.ARRow.AH_OH = TestOrg2.PK;
			testContra1.ARRow.AH_LocalExTaxAmount = 48M;
			testContra1.ARRow.AH_OSTotalAmount = 48M;

			TestAPInv.AH_OH = TestOrg3.PK;
			TestAPInv.AH_LocalExTaxAmount = 80M;
			TestAPInv.AH_OSTotalAmount = 80M;

			TestARInv.AH_OH = TestOrg1.PK;
			TestARInv.AH_LocalExTaxAmount = 40M;
			TestARInv.AH_OSTotalAmount = 40M;

			TestARInv2.AH_OH = TestOrg2.PK;
			TestARInv2.AH_LocalExTaxAmount = 40M;
			TestARInv2.AH_OSTotalAmount = 40M;

			TestOrgBalanceSheet.AddIMatching(testContra1);
			TestOrgBalanceSheet.AddIMatching(TestAPInv);
			TestOrgBalanceSheet.AddIMatching(TestARInv);
			TestOrgBalanceSheet.AddIMatching(TestARInv2);
			TestOrgBalanceSheet.Transactions.SetPartialPaidAmount();

			IMatchingCollection dynamicTransactions = TestOrgBalanceSheet.CreateDynamicTransactions();

			IMatchingCollection testTransactions = new IMatchingCollection(Factory);
			testTransactions.AddRange(dynamicTransactions);
			testTransactions.AddRange(TestOrgBalanceSheet.Transactions);

			AssertEquals("Balance of Org1 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org1 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org2 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org2 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org3 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg3.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org3 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg3.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));

			AssertEquals("Balance of AP should be 0", 0M, testTransactions.GetLedgerBalanceAmount(ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of AR should be 0", 0M, testTransactions.GetLedgerBalanceAmount(ZArchitecture.Core.LedgerTypes.AccountsReceivable));

			TransferFilter transferOrg1ToOrg3 = new TransferFilter();
			transferOrg1ToOrg3.TransferLedger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			transferOrg1ToOrg3.TransferFromOrg = TestOrg1.PK;
			transferOrg1ToOrg3.TransferFromAmount = -48M;
			transferOrg1ToOrg3.TransferToOrg = TestOrg3.PK;
			transferOrg1ToOrg3.TransferToAmount = 48M;

			AssertNotNull("Transfer from Org1 to Org3 should be created", dynamicTransactions.GetMatchingTransfer(transferOrg1ToOrg3));
		}

		#endregion

		#region TestMatchingInvoicesAndCreditNotesFromBothLedgers

		public void TestMatchingInvoicesAndCreditNotesFromBothLedgers()
		{
			TestOrgBalanceSheet = new OrganizationBalanceSheet(TestOrg1.PK, ZString.Empty, Factory);

			// TestOrg1
			ARCreditNote testARCrd1 = Factory.NewWithValidTestData<ARCreditNote>();
			testARCrd1.AH_OH = TestOrg1.PK;
			testARCrd1.AH_LocalExTaxAmount = 12.34M;
			testARCrd1.AH_OSTotalAmount = 12.34M;

			TestARInv.AH_OH = TestOrg1.PK;
			TestARInv.AH_LocalExTaxAmount = 34.56M;
			TestARInv.AH_OSTotalAmount = 34.56M;

			TestAPInv.AH_OH = TestOrg1.PK;
			TestAPInv.AH_LocalExTaxAmount = 20.65M;
			TestAPInv.AH_OSTotalAmount = 20.65M;

			APCreditNote testAPCrd1 = Factory.NewWithValidTestData<APCreditNote>();
			testAPCrd1.AH_OH = TestOrg1.PK;
			testAPCrd1.AH_LocalExTaxAmount = 62.03M;
			testAPCrd1.AH_OSTotalAmount = 62.03M;

			// TestOrg2
			ARCreditNote testARCrd2 = Factory.NewWithValidTestData<ARCreditNote>();
			testARCrd2.AH_OH = TestOrg2.PK;
			testARCrd2.AH_LocalExTaxAmount = 14.56M;
			testARCrd2.AH_OSTotalAmount = 14.56M;

			TestARInv2.AH_OH = TestOrg2.PK;
			TestARInv2.AH_LocalExTaxAmount = 65.09M;
			TestARInv2.AH_OSTotalAmount = 65.09M;

			TestAPInv2.AH_OH = TestOrg2.PK;
			TestAPInv2.AH_LocalExTaxAmount = 52.34M;
			TestAPInv2.AH_OSTotalAmount = 52.34M;

			APCreditNote testAPCrd2 = Factory.NewWithValidTestData<APCreditNote>();
			testAPCrd2.AH_OH = TestOrg2.PK;
			testAPCrd2.AH_LocalExTaxAmount = 34.09M;
			testAPCrd2.AH_OSTotalAmount = 34.09M;

			// TestOrg3
			ARCreditNote testARCrd3 = Factory.NewWithValidTestData<ARCreditNote>();
			testARCrd3.AH_OH = TestOrg3.PK;
			testARCrd3.AH_LocalExTaxAmount = 100.9M;
			testARCrd3.AH_OSTotalAmount = 100.9M;

			APCreditNote testAPCrd3 = Factory.NewWithValidTestData<APCreditNote>();
			testAPCrd3.AH_OH = TestOrg3.PK;
			testAPCrd3.AH_LocalExTaxAmount = 5.02M;
			testAPCrd3.AH_OSTotalAmount = 5.02M;

			TestOrgBalanceSheet.AddIMatching(testARCrd1);
			TestOrgBalanceSheet.AddIMatching(TestARInv);
			TestOrgBalanceSheet.AddIMatching(TestAPInv);
			TestOrgBalanceSheet.AddIMatching(testAPCrd1);

			TestOrgBalanceSheet.AddIMatching(testARCrd2);
			TestOrgBalanceSheet.AddIMatching(TestARInv2);
			TestOrgBalanceSheet.AddIMatching(TestAPInv2);
			TestOrgBalanceSheet.AddIMatching(testAPCrd2);

			TestOrgBalanceSheet.AddIMatching(testARCrd3);
			TestOrgBalanceSheet.AddIMatching(testAPCrd3);
			TestOrgBalanceSheet.Transactions.SetPartialPaidAmount();

			IMatchingCollection dynamicTransactions = TestOrgBalanceSheet.CreateDynamicTransactions();

			Factory.Save();
			IMatchingCollection testTransactions = new IMatchingCollection(Factory);
			testTransactions.AddRange(dynamicTransactions);
			testTransactions.AddRange(TestOrgBalanceSheet.Transactions);

			AssertEquals("Balance of Org1 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org1 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org2 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org2 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org3 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg3.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org3 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg3.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
		}

		#endregion

		#region TestMatchingINV_CRD_APPaymentFromBothLedgers

		public void TestMatchingINV_CRD_APPaymentFromBothLedgers()
		{
			SetupTestDataSet1(ZArchitecture.Core.LedgerTypes.AccountsPayable);
			TestOrg1.OH_IsCreditor = true;

			TestAPPay.AH_OH = TestOrg1.PK;
			TestAPPay.AH_LocalExTaxAmount = 24.24M;
			TestAPPay.AH_OSTotalAmount = 24.24M;

			TestARInv.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInv, TestARInv.TransactionCurrency, TestARInv.AH_ExchangeRate, 4.26m, 0m, 0m, 4.26m, 0m, 0m);

			// Test Org 1
			TestOrgBalanceSheet.AddIMatching(ARCRD1);
			TestOrgBalanceSheet.AddIMatching(TestARInv);
			TestOrgBalanceSheet.AddIMatching(TestAPInv);
			TestOrgBalanceSheet.AddIMatching(APCRD1);
			TestOrgBalanceSheet.AddIMatching(TestAPPay);
			TestOrgBalanceSheet.Transactions.SetPartialPaidAmount();

			IMatchingCollection dynamicTransactions = TestOrgBalanceSheet.CreateDynamicTransactions();

			Factory.Save();

			IMatchingCollection testTransactions = new IMatchingCollection(Factory);
			testTransactions.AddRange(dynamicTransactions);
			testTransactions.AddRange(TestOrgBalanceSheet.Transactions);

			AssertEquals("Balance of Org1 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org1 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org2 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org2 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org3 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg3.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org3 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg3.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org4 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg4.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org4 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg4.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org5 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg5.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org5 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg5.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org6 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg6.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org6 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg6.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));

			AssertEquals("Balance of AP should be zero", 0M, testTransactions.GetLedgerBalanceAmount(ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of AR should be zero", 0M, testTransactions.GetLedgerBalanceAmount(ZArchitecture.Core.LedgerTypes.AccountsReceivable));

			ContraFilter org1ToOrg1 = new ContraFilter();
			org1ToOrg1.APOrg = TestOrg1.PK;
			org1ToOrg1.APAmount = -8.08M;
			org1ToOrg1.AROrg = TestOrg1.PK;
			org1ToOrg1.ARAmount = 8.08M;
			AssertNotNull("Contra for 8.08 should exist", dynamicTransactions.GetMatchingContra(org1ToOrg1));

			ContraFilter org2ToOrg1 = new ContraFilter();
			org2ToOrg1.APOrg = TestOrg1.PK;
			org2ToOrg1.APAmount = 50.53M;
			org2ToOrg1.AROrg = TestOrg2.PK;
			org2ToOrg1.ARAmount = -50.53M;
			AssertNotNull("Contra for 50.53 should exist", dynamicTransactions.GetMatchingContra(org2ToOrg1));

			ContraFilter org3ToOrg1 = new ContraFilter();
			org3ToOrg1.APOrg = TestOrg1.PK;
			org3ToOrg1.APAmount = -100.9M;
			org3ToOrg1.AROrg = TestOrg3.PK;
			org3ToOrg1.ARAmount = 100.9M;
			AssertNotNull("Contra for 100.9 should exist", dynamicTransactions.GetMatchingContra(org3ToOrg1));

			ContraFilter org4ToOrg1 = new ContraFilter();
			org4ToOrg1.APOrg = TestOrg1.PK;
			org4ToOrg1.APAmount = 40.4M;
			org4ToOrg1.AROrg = TestOrg4.PK;
			org4ToOrg1.ARAmount = -40.4M;
			AssertNotNull("Contra for 40.4 should exist", dynamicTransactions.GetMatchingContra(org4ToOrg1));
		}

		#endregion

		#region TestMatchingINV_CRD_SingleARPaymentFromBothLedgers

		public void TestMatchingINV_CRD_SingleARPaymentFromBothLedgers()
		{
			SetupTestDataSet1(ZArchitecture.Core.LedgerTypes.AccountsReceivable);

			ARPayment aRPAY = Factory.NewWithValidTestData<ARPayment>();
			aRPAY.AH_OH = TestOrg1.PK;
			aRPAY.AH_LocalExTaxAmount = 27.24M;
			aRPAY.AH_OSTotalAmount = 27.24M;

			TestOrgBalanceSheet.AddIMatching(aRPAY);
			TestOrgBalanceSheet.Transactions.SetPartialPaidAmount();

			IMatchingCollection dynamicTransactions = TestOrgBalanceSheet.CreateDynamicTransactions();

			Factory.Save();

			IMatchingCollection testTransactions = new IMatchingCollection(Factory);
			testTransactions.AddRange(dynamicTransactions);
			testTransactions.AddRange(TestOrgBalanceSheet.Transactions);

			AssertEquals("Balance of Org1 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org1 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org2 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org2 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org3 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg3.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org3 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg3.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org4 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg4.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org4 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg4.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org5 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg5.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org5 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg5.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org6 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg6.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org6 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg6.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));

			AssertEquals("Balance of AP should be zero", 0M, testTransactions.GetLedgerBalanceAmount(ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of AR should be zero", 0M, testTransactions.GetLedgerBalanceAmount(ZArchitecture.Core.LedgerTypes.AccountsReceivable));
		}

		#endregion

		#region TestMatchingINV_CRD_APReceiptFromBothLedgers

		public void TestMatchingINV_CRD_APReceiptFromBothLedgers()
		{
			SetupTestDataSet1(ZArchitecture.Core.LedgerTypes.AccountsPayable);
			TestOrg1.OH_IsCreditor = true;

			APReceipt aPREC = Factory.NewWithValidTestData<APReceipt>();
			aPREC.AH_OH = TestOrg1.PK;
			aPREC.AH_LocalExTaxAmount = 43.46M;
			aPREC.AH_OSTotalAmount = 43.46M;

			TestARInv.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInv, TestARInv.TransactionCurrency, TestARInv.AH_ExchangeRate, 71.96m, 0m, 0m, 71.96m, 0m, 0m);

			// Test Org 1
			TestOrgBalanceSheet.AddIMatching(ARCRD1);
			TestOrgBalanceSheet.AddIMatching(TestARInv);
			TestOrgBalanceSheet.AddIMatching(TestAPInv);
			TestOrgBalanceSheet.AddIMatching(APCRD1);
			TestOrgBalanceSheet.AddIMatching(aPREC);
			TestOrgBalanceSheet.Transactions.SetPartialPaidAmount();

			IMatchingCollection dynamicTransactions = TestOrgBalanceSheet.CreateDynamicTransactions();

			Factory.Save();

			IMatchingCollection testTransactions = new IMatchingCollection(Factory);
			testTransactions.AddRange(dynamicTransactions);
			testTransactions.AddRange(TestOrgBalanceSheet.Transactions);

			AssertEquals("Balance of Org1 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org1 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org2 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org2 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org3 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg3.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org3 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg3.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org4 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg4.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org4 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg4.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org5 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg5.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org5 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg5.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org6 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg6.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org6 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg6.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));

			AssertEquals("Balance of AP should be zero", 0M, testTransactions.GetLedgerBalanceAmount(ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of AR should be zero", 0M, testTransactions.GetLedgerBalanceAmount(ZArchitecture.Core.LedgerTypes.AccountsReceivable));

			ContraFilter org1ToOrg1 = new ContraFilter();
			org1ToOrg1.APOrg = TestOrg1.PK;
			org1ToOrg1.APAmount = 59.62M;
			org1ToOrg1.AROrg = TestOrg1.PK;
			org1ToOrg1.ARAmount = -59.62M;
			AssertNotNull("Contra from Org1 to Org1 should exist", dynamicTransactions.GetMatchingContra(org1ToOrg1));

			ContraFilter org2ToOrg1 = new ContraFilter();
			org2ToOrg1.APOrg = TestOrg1.PK;
			org2ToOrg1.APAmount = 50.53M;
			org2ToOrg1.AROrg = TestOrg2.PK;
			org2ToOrg1.ARAmount = -50.53M;
			AssertNotNull("Contra from Org2 to Org1 should exist", dynamicTransactions.GetMatchingContra(org2ToOrg1));

			ContraFilter org3ToOrg1 = new ContraFilter();
			org3ToOrg1.APOrg = TestOrg1.PK;
			org3ToOrg1.APAmount = -100.9M;
			org3ToOrg1.AROrg = TestOrg3.PK;
			org3ToOrg1.ARAmount = 100.9M;
			AssertNotNull("Contra from Org3 to Org1 should exist", dynamicTransactions.GetMatchingContra(org3ToOrg1));

			ContraFilter org4ToOrg1 = new ContraFilter();
			org4ToOrg1.APOrg = TestOrg1.PK;
			org4ToOrg1.APAmount = 40.4M;
			org4ToOrg1.AROrg = TestOrg4.PK;
			org4ToOrg1.ARAmount = -40.4M;
			AssertNotNull("Contra from Org4 to Org1 should exist", dynamicTransactions.GetMatchingContra(org4ToOrg1));
		}

		#endregion

		#region TestMatchingINV_CRD_ARPaymentFromBothLedgers

		public void TestMatchingINV_CRD_ARPaymentFromBothLedgers()
		{
			SetupTestDataSet1(ZArchitecture.Core.LedgerTypes.AccountsReceivable);

			// Test Org 1
			TestAPInv.AH_OH = TestOrg1.PK;
			TestAPInv.Lines[0].AL_LocalExTaxAmount = TestAPInv.Lines[0].AL_OSExTaxAmount = 70m;

			APCRD1 = Factory.NewWithValidTestData<APCreditNote>();
			APCRD1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(APCRD1, APCRD1.TransactionCurrency, APCRD1.AH_ExchangeRate, 62.03m, 0m, 0m, 62.03m, 0m, 0m);

			TestARInv.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInv, TestARInv.TransactionCurrency, TestARInv.AH_ExchangeRate, 34.56m, 0m, 0m, 34.56m, 0m, 0m);

			ARPayment aRPAY = Factory.NewWithValidTestData<ARPayment>();
			aRPAY.AH_OH = TestOrg1.PK;
			aRPAY.AH_LocalExTaxAmount = 12.99M;
			aRPAY.AH_OSTotalAmount = 12.99M;

			// Test Org 1
			TestOrgBalanceSheet.AddIMatching(ARCRD1);
			TestOrgBalanceSheet.AddIMatching(TestARInv);
			TestOrgBalanceSheet.AddIMatching(TestAPInv);
			TestOrgBalanceSheet.AddIMatching(APCRD1);
			TestOrgBalanceSheet.AddIMatching(aRPAY);
			TestOrgBalanceSheet.Transactions.SetPartialPaidAmount();

			IMatchingCollection dynamicTransactions = TestOrgBalanceSheet.CreateDynamicTransactions();

			Factory.Save();

			IMatchingCollection testTransactions = new IMatchingCollection(Factory);
			testTransactions.AddRange(dynamicTransactions);
			testTransactions.AddRange(TestOrgBalanceSheet.Transactions);

			AssertEquals("Balance of Org1 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org1 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org2 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org2 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg2.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org3 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg3.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org3 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg3.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org4 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg4.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org4 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg4.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org5 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg5.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org5 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg5.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of Org6 in AR = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg6.PK, ZArchitecture.Core.LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of Org6 in AP = 0", 0M, testTransactions.GetOrganizationBalanceAmount(TestOrg6.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable));

			AssertEquals("Balance of AP should be zero", 0M, testTransactions.GetLedgerBalanceAmount(ZArchitecture.Core.LedgerTypes.AccountsPayable));
			AssertEquals("Balance of AR should be zero", 0M, testTransactions.GetLedgerBalanceAmount(ZArchitecture.Core.LedgerTypes.AccountsReceivable));

			TransferFilter org4ToOrg1Transfer = new TransferFilter();
			org4ToOrg1Transfer.TransferLedger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			org4ToOrg1Transfer.TransferFromOrg = TestOrg4.PK;
			org4ToOrg1Transfer.TransferFromAmount = -40.4M;
			org4ToOrg1Transfer.TransferToOrg = TestOrg1.PK;
			org4ToOrg1Transfer.TransferToAmount = 40.4M;
			AssertNotNull("Transfer should be created with value 40.4", dynamicTransactions.GetMatchingTransfer(org4ToOrg1Transfer));

			ContraFilter org1ToOrg1Filter = new ContraFilter();
			org1ToOrg1Filter.APOrg = TestOrg1.PK;
			org1ToOrg1Filter.APAmount = 7.97M;
			org1ToOrg1Filter.AROrg = TestOrg1.PK;
			org1ToOrg1Filter.ARAmount = -7.97M;
			AssertNotNull("Contra should be created with value 7.97", dynamicTransactions.GetMatchingContra(org1ToOrg1Filter));
		}

		#endregion

		#region TestMatchingINV_CRD_ARReceiptFromBothLedgers

		#endregion

		#region TestUsingAPAsPrimaryLedger

		public void TestUsingAPAsPrimaryLedger()
		{
			TestOrg1.OH_IsCreditor = true;

			TestOrgBalanceSheet = new OrganizationBalanceSheet(TestOrg1.PK, ZArchitecture.Core.LedgerTypes.AccountsPayable, Factory);

			TestARInv.AH_OH = TestOrg1.PK;
			TestARInv.AH_LocalExTaxAmount = 20M;
			TestARInv.AH_OSTotalAmount = 20M;

			TestAPInv.AH_OH = TestOrg1.PK;
			TestAPInv.AH_LocalExTaxAmount = 30M;
			TestAPInv.AH_OSTotalAmount = 30M;

			TestARInv2.AH_OH = TestOrg2.PK;
			TestARInv2.AH_LocalExTaxAmount = 10M;
			TestARInv2.AH_OSTotalAmount = 10M;

			TestOrgBalanceSheet.AddIMatching(TestARInv);
			TestOrgBalanceSheet.AddIMatching(TestAPInv);
			TestOrgBalanceSheet.AddIMatching(TestARInv2);
			TestOrgBalanceSheet.Transactions.SetPartialPaidAmount();

			IMatchingCollection dynamicTransactions = TestOrgBalanceSheet.CreateDynamicTransactions();

			ContraFilter org2ToOrg1Contra = new ContraFilter();
			org2ToOrg1Contra.APOrg = TestOrg1.PK;
			org2ToOrg1Contra.APAmount = 10M;
			org2ToOrg1Contra.AROrg = TestOrg2.PK;
			org2ToOrg1Contra.ARAmount = -10M;
			AssertNotNull("Contra from Org2 to Org1 should be created", dynamicTransactions.GetMatchingContra(org2ToOrg1Contra));

			ContraFilter org1ToOrg1Contra = new ContraFilter();
			org1ToOrg1Contra.APOrg = TestOrg1.PK;
			org1ToOrg1Contra.APAmount = 20M;
			org1ToOrg1Contra.AROrg = TestOrg1.PK;
			org1ToOrg1Contra.ARAmount = -20M;
			AssertNotNull("Contra from Org1 to Org1 should be created", dynamicTransactions.GetMatchingContra(org1ToOrg1Contra));
		}

		#endregion

		#region TestPrimaryOrgBalancesSettlementOrgsImbalances

		[MasterFiles.Business.Testing.SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestPrimaryOrgLedgersisZeroSettlementOrgsLedgersAreNoteWhenPrimaryIsARAndAP()
		{
			OrgHeader settlementOrg1 = TestObjectCreator.AALSHI;
			OrgHeader settlementOrg2 = TestObjectCreator.ABIGAS;
			OrgHeader settlementOrg3 = TestObjectCreator.XLINDU;
			OrgHeader primaryOrg = TestObjectCreator.ZECTRA;
			settlementOrg1.CompanyData.OB_IsDebtor = true;
			settlementOrg2.CompanyData.OB_IsDebtor = true;
			settlementOrg2.CompanyData.OB_IsCreditor = true;
			settlementOrg3.CompanyData.OB_IsCreditor = true;
			primaryOrg.CompanyData.OB_IsDebtor = false;
			primaryOrg.CompanyData.OB_IsCreditor = true;

			Factory.Save();

			IMatchingCollection matchedTransactions = GetTransactionsToMatch(settlementOrg1, settlementOrg2, settlementOrg3, primaryOrg);

			OrganizationBalanceSheet balanceSheet = new OrganizationBalanceSheet(primaryOrg.PK, LedgerTypes.AccountsPayable, Factory);
			balanceSheet.AddIMatchingCollection(matchedTransactions);

			IMatchingCollection dynamicTransactions = balanceSheet.CreateDynamicTransactions();
			AssertEquals("expect to see 2 transfers (two rows each) + 2 contras(two rows each)", dynamicTransactions.Count, 4);

			AssertOneAndOnlyOneExists(dynamicTransactions, LedgerTypes.AccountsPayable, TransactionTypes.Transfer, 7m, settlementOrg2, primaryOrg);
			AssertOneAndOnlyOneExists(dynamicTransactions, LedgerTypes.AccountsPayable, TransactionTypes.Transfer, 6m, settlementOrg3, primaryOrg);
			AssertOneAndOnlyOneExists(dynamicTransactions, LedgerTypes.AccountsReceivable, TransactionTypes.Contra, -8m, settlementOrg1, primaryOrg);
			AssertOneAndOnlyOneExists(dynamicTransactions, LedgerTypes.AccountsReceivable, TransactionTypes.Contra, -5m, settlementOrg2, primaryOrg);

			balanceSheet = new OrganizationBalanceSheet(primaryOrg.PK, LedgerTypes.AccountsPayable, Factory);
			balanceSheet.AddIMatchingCollection(matchedTransactions);
			balanceSheet.AddIMatchingCollection(dynamicTransactions);

			OrganizationSubBalanceCollection aRSubBalances = balanceSheet.ARSubBalances;
			OrganizationSubBalanceCollection aPSubBalances = balanceSheet.APSubBalances;

			AssertEquals("Should no longer be any AR sub balalnces", 0, aRSubBalances.Count);
			AssertEquals("Should no longer be any AP sub balalnces", 0, aPSubBalances.Count);
		}

		#region Implementation
		IMatchingCollection GetTransactionsToMatch(OrgHeader settlementOrg1, OrgHeader settlementOrg2, OrgHeader settlementOrg3, OrgHeader primaryOrg)
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));

			ARInvoice aRInvoiceOrg1 = TestObjectCreator.CreateARInvoice<ARInvoice>("ARInvoiceOrg1", TestObjectCreator.AUD, 1m, settlementOrg1);
			Assert(aRInvoiceOrg1.AH_FullyPaidDate.IsEmpty);
			TestObjectCreator.CreateARInvoiceLine(aRInvoiceOrg1, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "Test Line", 8);
			aRInvoiceOrg1.AH_FullyPaidDate = ZDateTime.Empty;
			TestObjectCreator.CreateJobCharge(aRInvoiceOrg1.Lines[0], TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

			APInvoice aPInvoiceOrg2 = TestObjectCreator.CreateAPInvoice<APInvoice>("APInvoiceOrg2", TestObjectCreator.AUD, 1m, 7m, 0, 0, 7m, 0, 0, settlementOrg2, false);
			ARInvoice aRInvoiceOrg2 = TestObjectCreator.CreateARInvoice<ARInvoice>("ARInvoiceOrg2", TestObjectCreator.AUD, 1m, settlementOrg2);
			TestObjectCreator.CreateARInvoiceLine(aRInvoiceOrg2, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "Test Line", 5);
			TestObjectCreator.CreateJobCharge(aRInvoiceOrg2.Lines[0], TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

			APInvoice aPInvoiceOrg3 = TestObjectCreator.CreateAPInvoice<APInvoice>("APInvoiceOrg3", TestObjectCreator.AUD, 1m, 6m, 0, 0, 6m, 0, 0, settlementOrg3, false);

			APInvoice aPInvoicePrimary = TestObjectCreator.CreateAPInvoice<APInvoice>("APInvoicePri", TestObjectCreator.AUD, 1m, 5m, 0, 0, 5m, 0, 0, primaryOrg, false);
			APPayment aPPaymentPrimary = TestObjectCreator.CreateCashAPPaymentForInvoice(aPInvoicePrimary);

			aRInvoiceOrg1.Validation.ValidateAll();

			aPInvoiceOrg2.Validation.ValidateAll();
			aRInvoiceOrg2.Validation.ValidateAll();

			aPInvoiceOrg3.Validation.ValidateAll();

			aPInvoicePrimary.Validation.ValidateAll();
			aPPaymentPrimary.Validation.ValidateAll();

			AssertNoErrors(aRInvoiceOrg1);
			AssertNoErrors(aPInvoiceOrg2);
			AssertNoErrors(aRInvoiceOrg2);
			AssertNoErrors(aPInvoiceOrg3);
			AssertNoErrors(aPInvoicePrimary);
			AssertNoErrors(aPPaymentPrimary);

			decimal sumOfTransactions = aRInvoiceOrg1.AH_InvoiceAmount + aPInvoiceOrg2.AH_InvoiceAmount + aRInvoiceOrg2.AH_InvoiceAmount + aPInvoiceOrg3.AH_InvoiceAmount + aPInvoicePrimary.AH_InvoiceAmount + aPPaymentPrimary.AH_InvoiceAmount;
			AssertEquals("Transactions can be matched together because they sum to zero", 0m, sumOfTransactions);

			Factory.Save();

			IMatchingCollection matchedTransactions = new IMatchingCollection(Factory);
			matchedTransactions.AddRange(new IMatching[] { aRInvoiceOrg1, aPInvoiceOrg2, aRInvoiceOrg2, aPInvoiceOrg3, aPInvoicePrimary, aPPaymentPrimary });

			foreach (IMatching transaction in matchedTransactions)
			{
				transaction.OSPartialPaymentAmount = ((AccTransactionHeader)transaction).AH_InvoiceAmount;
			}

			return matchedTransactions;
		}

		void AssertOneAndOnlyOneExists(IMatchingCollection transactions, string ledgerType, string type, Decimal fromAmount, OrgHeader fromOrg, OrgHeader toOrg)
		{
			switch (type)
			{
				case TransactionTypes.Contra:
					AssertContraValues(transactions, ledgerType, fromAmount, fromOrg, toOrg);
					break;
				case TransactionTypes.Transfer:
					AssertTransferValues(transactions, ledgerType, fromAmount, fromOrg, toOrg);
					break;
				default:
					throw new InvalidOperationException("Expected Contra or Transfer");
			}
		}

		void AssertContraValues(IMatchingCollection transactions, string fromLedger, Decimal fromAmount, OrgHeader fromOrg, OrgHeader toOrg)
		{
			ContraFilter contraFilter = new ContraFilter();
			if (fromLedger == LedgerTypes.AccountsPayable)
			{
				contraFilter.APAmount = fromAmount;
				contraFilter.APOrg = fromOrg.PK;
				contraFilter.ARAmount = -fromAmount;
				contraFilter.AROrg = toOrg.PK;
			}
			else
			{
				contraFilter.APAmount = -fromAmount;
				contraFilter.APOrg = toOrg.PK;
				contraFilter.ARAmount = fromAmount;
				contraFilter.AROrg = fromOrg.PK;
			}
			AssertNotNull("Should find contra Form Org " + fromOrg.OH_Code + ", to org" + toOrg.OH_Code + ", Amount" + fromAmount.ToString(), transactions.GetMatchingContra(contraFilter));
		}

		void AssertTransferValues(IMatchingCollection transactions, string ledgerType, Decimal fromAmount, OrgHeader fromOrg, OrgHeader toOrg)
		{
			TransferFilter transferFilter = new TransferFilter();
			transferFilter.TransferLedger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			transferFilter.TransferFromOrg = fromOrg.PK;
			transferFilter.TransferFromAmount = fromAmount;
			transferFilter.TransferToOrg = toOrg.PK;
			transferFilter.TransferToAmount = -fromAmount;
			AssertNotNull("Should find transfer for Ledger " + ledgerType + ", Form Org " + fromOrg.OH_Code + ", to org " + toOrg.OH_Code + ", amount " + fromAmount.ToString(), transactions.GetMatchingTransfer(transferFilter));
		}
		#endregion

		#endregion

		#region Implementation

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion
	}
}
