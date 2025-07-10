using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocAPPayment))]
	sealed class DocAPPaymentTest : DocumentWrapperTestCase
	{
		#region TestNewWithARPayment

		public void TestNewWithARPayment()
		{
			var aRPayment = Factory.New<ARPayment>();
			DocumentWrapper docPayment = DocumentWrapperFactory.CreateWrapper(Enterprise.Core.Constants.DataContext.APPayment, aRPayment);

			AssertNotNull("DocPayment should have been instantiated", docPayment);
			Assert("Should be of type DocAPPayment", docPayment.GetType().IsAssignableFrom(typeof(DocAPPayment)));
		}

		#endregion

		#region TestNewWithAPPayment

		public void TestNewWithAPPayment()
		{
			var aPPayment = Factory.New<APPayment>();
			DocumentWrapper docPayment = DocumentWrapperFactory.CreateWrapper(Enterprise.Core.Constants.DataContext.APPayment, aPPayment);

			AssertNotNull("DocPayment should have been instantiated", docPayment);
			Assert("Should be of type DocAPPayment", docPayment.GetType().IsAssignableFrom(typeof(DocAPPayment)));
		}

		#endregion

		#region Approval Footer

		public void TestApprovalFooterFields()
		{
			APPaymentApprovalWithAuthorisation approval = Factory.New<APPaymentApprovalWithAuthorisation>();

			AssertEquals("ShowApprovalFooter", ZBool.False, PaymentWrapper.ShowNewAuthorisationFooter);

			approval.AV_AH = Payment.PK;
			AssertEquals("ShowApprovalFooter", ZBool.True, PaymentWrapper.ShowNewAuthorisationFooter);

			AssertEquals("FirstAuthorisationDescription", "First Authorization (" + approval.Level1AuthorisationStatus + ")", PaymentWrapper.FirstAuthorisationDescription);
			AssertEquals("SecondAuthorisationDescription", "Second Authorization (" + approval.Level2AuthorisationStatus + ")", PaymentWrapper.SecondAuthorisationDescription);
			AssertEquals("ThirdAuthorisationDescription", "Third Authorization (" + approval.Level3AuthorisationStatus + ")", PaymentWrapper.ThirdAuthorisationDescription);

			approval.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
			approval.AV_GS_NKApproval2nd = GlbStaff.CurrentUser.GS_Code;
			approval.AV_GS_NKApproval3rd = GlbStaff.CurrentUser.GS_Code;

			AssertEquals("FirstAuthorisation", approval.Approval1st.GS_FullName, PaymentWrapper.FirstAuthorisation);
			AssertEquals("SecondAuthorisation", approval.Approval2nd.GS_FullName, PaymentWrapper.SecondAuthorisation);
			AssertEquals("ThirdAuthorisation", approval.Approval3rd.GS_FullName, PaymentWrapper.ThirdAuthorisation);
		}

		#endregion

		#region MICR Number

		public void TestMICRNumber()
		{
			var bank = Factory.New<AccBankAccount>();
			bank.AB_BankName = "Test Bank Account";
			bank.AB_BSB = "123456789-";
			bank.AB_AccountNum = "010203040506-";
			bank.AB_BankAddress = "123 Address Test";
			bank.AB_GC = GlbCompany.CurrentCompany.PK;
			bank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bank.AB_GB = GlbBranch.CurrentBranch.PK;
			Payment.AH_AB = bank.PK;
			Payment.AH_ChequeOrReference = "1357924680-";

			AssertEquals("MICRNumber", "C1357924680DC A123456789DA 010203040506DC", PaymentWrapper.MICRNumber);
		}

		#endregion

		#region Payment Transaction Summary

		public void TestPaymentTransactionSummary()
		{
			Payment.AH_TransactionNum = "XXX1";
			APInvoice invoice1 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "Fracht01");
			APInvoice invoice2 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "Fracht02");

			CreateMatchLink(Payment, 200M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice1, -200M, "GROUP1");

			CreateAPInvoiceLine(invoice1, 200M);
			invoice1.AH_OutstandingAmount = 0M;
			invoice1.AH_FullyPaidDate = ZDateTime.Now;
			Payment.AH_OSTotal = Payment.AH_InvoiceAmount = 200M;
			Payment.AH_FullyPaidDate = ZDateTime.Now;

			Factory.Save();

			string summary = PaymentWrapper.PaymentTransactionSummary;

			AssertEquals("Should show transaction#, amount and currency for each transaction.", "Fracht01   200.00   AUD" + System.Environment.NewLine, summary);
		}

		public void TestPaymentTransactionSummaryRowTooLong()
		{
			var registryValue = new MaximumAllowedTransactionAmount()
			{
				MaximumAllowedHeaderAmount = 1200000000000000M,
				MaximumAllowedLineAmount = 1200000000000000M
			};

			using (AccountingMasterFilesRegistry.Instance.SystemDefinedMaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			using (AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				Payment.AH_TransactionNum = "XXX1";

				APInvoice invoice1 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "Fracht123456");

				CreateMatchLink(Payment, 999999999999M, "GROUP1");
				CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice1, -999999999999M, "GROUP1");

				CreateAPInvoiceLine(invoice1, 999999999999M);
				invoice1.AH_OutstandingAmount = 0M;
				invoice1.AH_FullyPaidDate = ZDateTime.Now;
				Payment.AH_OSTotal = Payment.AH_InvoiceAmount = 999999999999M;
				Payment.AH_FullyPaidDate = ZDateTime.Now;

				Factory.Save();

				string summary = PaymentWrapper.PaymentTransactionSummary;

				AssertEquals("Row is > 38 chars; too long to display.", "PLEASE REFER TO REMITTANCE ADVICE FOR DETAILED LIST OF TRANSACTIONS", summary);
			}
		}

		public void TestPaymentTransactionSummaryTooManyRows()
		{
			Payment.AH_TransactionNum = "XXX1";

			APInvoice invoice1 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "Fracht01");
			APInvoice invoice2 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "Fracht02");
			APInvoice invoice3 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "Fracht03");
			APInvoice invoice4 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "Fracht04");
			APInvoice invoice5 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "Fracht05");
			APInvoice invoice6 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "Fracht06");
			APInvoice invoice7 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "Fracht07");
			APInvoice invoice8 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "Fracht08");
			APInvoice invoice9 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "Fracht09");
			APInvoice invoice10 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "Fracht10");
			APInvoice invoice11 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "Fracht11");

			CreateMatchLink(Payment, 1100M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice1, -100M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice2, -100M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice3, -100M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice4, -100M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice5, -100M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice6, -100M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice7, -100M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice8, -100M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice9, -100M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice10, -100M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice11, -100M, "GROUP1");

			CreateAPInvoiceLine(invoice1, 100M);
			invoice1.AH_OutstandingAmount = 0M;
			invoice1.AH_FullyPaidDate = ZDateTime.Now;
			CreateAPInvoiceLine(invoice2, 100M);
			invoice2.AH_OutstandingAmount = 0M;
			invoice2.AH_FullyPaidDate = ZDateTime.Now;
			CreateAPInvoiceLine(invoice3, 100M);
			invoice3.AH_OutstandingAmount = 0M;
			invoice3.AH_FullyPaidDate = ZDateTime.Now;
			CreateAPInvoiceLine(invoice4, 100M);
			invoice4.AH_OutstandingAmount = 0M;
			invoice4.AH_FullyPaidDate = ZDateTime.Now;
			CreateAPInvoiceLine(invoice5, 100M);
			invoice5.AH_OutstandingAmount = 0M;
			invoice5.AH_FullyPaidDate = ZDateTime.Now;
			CreateAPInvoiceLine(invoice6, 100M);
			invoice6.AH_OutstandingAmount = 0M;
			invoice6.AH_FullyPaidDate = ZDateTime.Now;
			CreateAPInvoiceLine(invoice7, 100M);
			invoice7.AH_OutstandingAmount = 0M;
			invoice7.AH_FullyPaidDate = ZDateTime.Now;
			CreateAPInvoiceLine(invoice8, 100M);
			invoice8.AH_OutstandingAmount = 0M;
			invoice8.AH_FullyPaidDate = ZDateTime.Now;
			CreateAPInvoiceLine(invoice9, 100M);
			invoice9.AH_OutstandingAmount = 0M;
			invoice9.AH_FullyPaidDate = ZDateTime.Now;
			CreateAPInvoiceLine(invoice10, 100M);
			invoice10.AH_OutstandingAmount = 0M;
			invoice10.AH_FullyPaidDate = ZDateTime.Now;
			CreateAPInvoiceLine(invoice11, 100M);
			invoice11.AH_OutstandingAmount = 0M;
			invoice11.AH_FullyPaidDate = ZDateTime.Now;
			Payment.AH_OSTotal = Payment.AH_InvoiceAmount = 1100M;
			Payment.AH_FullyPaidDate = ZDateTime.Now;

			Factory.Save();

			string summary = PaymentWrapper.PaymentTransactionSummary;

			AssertEquals("More than 10 rows; too long to display.", "PLEASE REFER TO REMITTANCE ADVICE FOR DETAILED LIST OF TRANSACTIONS", summary);
		}

		#endregion

		#region Remittance Advice And Payment Voucher Fields

		public void TestOSTotalForRemittanceAdvice()
		{
			AssertEquals(0M, PaymentWrapper.OSTotalForRemittanceAdvice);
			Payment.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Payment;
			Payment.AH_OSTotalAmount = 1000M;
			AssertEquals(1000M, PaymentWrapper.OSTotalForRemittanceAdvice);
			AssertEquals(1000M, PaymentWrapper.Cheque.ChequeAmount);
		}

		public void TestShowingOfSystemGeneratedContraAndTransfers()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			Payment.AH_TransactionNum = "PAY 000382";
			Payment.AH_OSTotal = 1000M;
			Payment.AH_InvoiceAmount = 1000M;

			APInvoice invoice1 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 1");
			CreateAPInvoiceLine(invoice1, 1000M);
			invoice1.AH_OutstandingAmount = 0M;

			ContraRow contra1AR = creator.CreateContraRow(typeof(ARContraRow), "00001001", 300M, false, 1);
			ContraRow contra1AP = creator.CreateContraRow(typeof(APContraRow), "00001001", -300M, false, 2);

			ContraRow contra2AR = creator.CreateContraRow(typeof(ARContraRow), "00001002", 300M, true, 1);
			ContraRow contra2AP = creator.CreateContraRow(typeof(APContraRow), "00001002", -300M, true, 2);

			TransferRow transfer1AR = creator.CreateTransferRow(typeof(ARTransferFromRow), "00001001", 200M, false, 1);
			TransferRow transfer1AP = creator.CreateTransferRow(typeof(APTransferToRow), "00001001", -200M, false, 2);

			TransferRow transfer2AR = creator.CreateTransferRow(typeof(ARTransferFromRow), "00001002", 200M, true, 1);
			TransferRow transfer2AP = creator.CreateTransferRow(typeof(APTransferToRow), "00001002", -200M, true, 2);

			CreateMatchLink(Payment, 1000M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice1, -1000M, "GROUP1");

			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, contra1AP, -300M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, contra1AR, 300M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, contra2AP, -300M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, contra2AR, 300M, "GROUP1");

			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, transfer1AP, -200M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, transfer1AR, 200M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, transfer2AP, -200M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, transfer2AR, 200M, "GROUP1");

			Payment.AH_FullyPaidDate = ZDateTime.Now;

			Factory.Save();
			AssertEquals("Should be 5 Transactions", 5, PaymentWrapper.Invoices.Count);
			AssertEquals("1st Transaction should be an contra", ZArchitecture.Core.TransactionTypes.Contra, PaymentWrapper.Invoices[0].TransactionType);
			AssertEquals("2nd Transaction should be an contra", ZArchitecture.Core.TransactionTypes.Contra, PaymentWrapper.Invoices[1].TransactionType);
			AssertEquals("3rd Transaction should be an transfer", ZArchitecture.Core.TransactionTypes.Transfer, PaymentWrapper.Invoices[2].TransactionType);
			AssertEquals("4th Transaction should be an transfer", ZArchitecture.Core.TransactionTypes.Transfer, PaymentWrapper.Invoices[3].TransactionType);
			AssertEquals("5th Transaction should be an invoice", ZArchitecture.Core.TransactionTypes.Invoice, PaymentWrapper.Invoices[4].TransactionType);
		}

		public void TestShowingOfSystemGeneratedJournalsRemittanceAdviceAndCheques()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ARPayment payment1 = Factory.NewWithValidTestData<ARPayment>();
			DocAPPayment paymentWrapper1 = DocAPPayment.New(payment1, Factory);
			payment1.AH_TransactionNum = "PAY 000382";
			payment1.AH_InvoiceAmount = 100M;
			payment1.AH_OSExTaxAmount = 100M;
			payment1.AH_LocalExTaxAmount = 100M;
			payment1.AH_OutstandingAmount = 0M;

			Journal journal1AR = creator.CreateJournal<ARJournal>(100.0m, ZDateTime.Now, creator.ABIGAS.PK);
			journal1AR.AH_TransactionCreatedByMatching = true;
			journal1AR.AH_OutstandingAmount = 0;
			Journal journal2AR = creator.CreateJournal<ARJournal>(-100.0m, ZDateTime.Now, creator.ABIGAS.PK);
			journal2AR.AH_TransactionCreatedByMatching = true;
			journal2AR.AH_OutstandingAmount = 0;

			CreateMatchLink(payment1, 100M, "GROUP1");
			CreateMatchLink(((IMatching)payment1).CurrentMatchGroup, journal1AR, 100M, "GROUP1");
			CreateMatchLink(((IMatching)payment1).CurrentMatchGroup, journal2AR, 100M, "GROUP1");

			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_GB = GlbBranch.CurrentBranch.PK;
			headerToMatch.AH_InvoiceAmount = -300M;
			headerToMatch.AH_Ledger = "AR";
			headerToMatch.AH_TransactionType = "INV";
			headerToMatch.AH_RX_NKTransactionCurrency = GlbBranch.CurrentBranch.Country.RN_RX_NKLocalCurrency;
			headerToMatch.AH_FullyPaidDate = ZDateTime.Now;

			payment1.AH_FullyPaidDate = ZDateTime.Now;
			journal1AR.AH_InvoiceAmount = 100M;
			journal1AR.AH_FullyPaidDate = ZDateTime.Now;
			journal2AR.AH_InvoiceAmount = 100M;
			journal2AR.AH_FullyPaidDate = ZDateTime.Now;

			AccTransactionMatchLink linkToMatch = ((IMatching)payment1).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = headerToMatch.PK;
			linkToMatch.AP_Amount = -300M;
			linkToMatch.AP_MatchGroupNum = "GROUP1";

			payment1.AH_FullyPaidDate = ZDateTime.Now;
			headerToMatch.AH_OutstandingAmount = 0M;
			TestObjectCreator.SetupMatchLinkMatchDate(payment1);

			Factory.Save();
			AssertEquals("Should be 1 Transaction", 1, paymentWrapper1.Invoices.Count);
			AssertEquals("Should be 1 Transaction", 1, paymentWrapper1.FirstPageInvoices.Count);
		}

		public void TestInvoiceAmountForPaymentVoucher()
		{
			AssertEquals(0M, PaymentWrapper.InvoiceAmountForPaymentVoucher);

			Payment.AH_LocalExTaxAmount = 200M;
			Payment.AH_LocalTaxAmount = 20M;
			AssertEquals(200M, PaymentWrapper.InvoiceAmountForPaymentVoucher);
		}

		public void TestApportionedAmountLocalCurrPayment()
		{
			Payment.AH_TransactionNum = "PAY 000382";

			APInvoice invoice1 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 1");
			APInvoice invoice2 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 2");
			APInvoice invoice3 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 3");

			CreateMatchLink(Payment, 730M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice1, -250M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice2, -320M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice3, -160M, "GROUP1");

			RefCurrency localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			RefCurrency uSCurr = GetRefCurrency("USD");
			RefCurrency eUCurr = GetRefCurrency("EUR");

			Payment.AH_RX_NKTransactionCurrency = localCurrency.RX_Code;
			invoice1.AH_RX_NKTransactionCurrency = uSCurr.RX_Code;
			invoice1.AH_ExchangeRate = 0.8m;
			invoice2.AH_RX_NKTransactionCurrency = localCurrency.RX_Code;
			invoice3.AH_RX_NKTransactionCurrency = eUCurr.RX_Code;
			invoice3.AH_ExchangeRate = 0.5m;

			CreateAPInvoiceLine(invoice1, 250m);
			CreateAPInvoiceLine(invoice2, 320M);
			CreateAPInvoiceLine(invoice3, 160M);
			Payment.AH_InvoiceAmount = 730M;
			Payment.AH_OSTotal = 730M;
			Payment.AH_FullyPaidDate = ZDateTime.Now;
			invoice1.AH_OutstandingAmount = 0m;
			invoice2.AH_OutstandingAmount = 0m;
			invoice3.AH_OutstandingAmount = 0m;

			Factory.Save();
			AssertEquals("Should be 3 Invoices", 3, PaymentWrapper.Invoices.Count);
			DocTransactionHeaderCollection result = PaymentWrapper.Invoices;
			result.Sort("TransactionNumber", ListSortDirection.Ascending);
			AssertEquals(250.00m, result[0].ApportionedAmount.Round(2));
			AssertEquals(320.00m, result[1].ApportionedAmount.Round(2));
			AssertEquals(160.00m, result[2].ApportionedAmount.Round(2));
		}

		public void TestApportionedAmountOSCurrPayment()
		{
			Payment.AH_TransactionNum = "PAY 000339";

			APInvoice invoice1 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 1");
			APInvoice invoice2 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 2");
			APInvoice invoice3 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 3");

			CreateMatchLink(Payment, 730M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice1, -250M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice2, -320M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice3, -160M, "GROUP1");

			RefCurrency localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			RefCurrency uSCurr = GetRefCurrency("USD");
			RefCurrency eUCurr = GetRefCurrency("EUR");

			Payment.AH_RX_NKTransactionCurrency = uSCurr.RX_Code;
			Payment.AH_ExchangeRate = 0.9m;
			invoice1.AH_RX_NKTransactionCurrency = uSCurr.RX_Code;
			invoice1.AH_ExchangeRate = 0.8m;
			invoice2.AH_RX_NKTransactionCurrency = localCurrency.RX_Code;
			invoice3.AH_RX_NKTransactionCurrency = eUCurr.RX_Code;
			invoice3.AH_ExchangeRate = 0.5m;

			CreateAPInvoiceLine(invoice1, 250m, uSCurr.RX_Code, 0.8m);
			CreateAPInvoiceLine(invoice2, 320M, localCurrency.RX_Code, 1m);
			CreateAPInvoiceLine(invoice3, 160M, eUCurr.RX_Code, 0.5m);
			Payment.AH_InvoiceAmount = 730M;
			Payment.AH_OSTotal = 657M;
			Payment.AH_FullyPaidDate = ZDateTime.Now;
			invoice1.AH_OutstandingAmount = 0m;
			invoice2.AH_OutstandingAmount = 0m;
			invoice3.AH_OutstandingAmount = 0m;

			Factory.Save();
			AssertEquals("Should be 3 Invoices", 3, PaymentWrapper.Invoices.Count);
			DocTransactionHeaderCollection result = PaymentWrapper.Invoices;
			result.Sort("TransactionNumber", ListSortDirection.Ascending);
			AssertEquals(200.00m, result[0].ApportionedAmount.Round(2));
			AssertEquals(304.67m, result[1].ApportionedAmount.Round(2));
			AssertEquals(152.33m, result[2].ApportionedAmount.Round(2));
		}

		#region Remittance Advice with multiple currency payments and invoices match groups

		public void TestApportionedAmountWhenMatchGroupIsBalancedWithoutMiscellanouesTransaction_SelectedPaymentCurrencyIsForeign()
		{
			var paymentInUSD = CreateMatchGroupWithoutMiscellanouesTransactionAndGetPayment(false);

			var testWrapper = DocAPPayment.New(paymentInUSD, Factory);
			AssertEquals("Should be 5 transactions", 5, testWrapper.Invoices.Count);

			var headerCollection = testWrapper.Invoices;
			AssertEquals(-73m, headerCollection[0].ApportionedAmount); //AUD PAY
			AssertEquals(195.43m, headerCollection[1].ApportionedAmount);  //USD INV			
			AssertEquals(68.54m, headerCollection[2].ApportionedAmount); //AUD INV
			AssertEquals(92.48m, headerCollection[3].ApportionedAmount);  //EUR INV 
			AssertEquals(-81.36m, headerCollection[4].ApportionedAmount);  //EUR CRD

			AssertRemittanceAdviceIsBalanced(paymentInUSD, headerCollection);
		}

		public void TestApportionedAmountWhenMatchGroupIsBalancedWithoutMiscellanouesTransaction_SelectedPaymentCurrencyIsLocal()
		{
			var paymentInAUD = CreateMatchGroupWithoutMiscellanouesTransactionAndGetPayment(true);

			var testWrapper = DocAPPayment.New(paymentInAUD, Factory);
			AssertEquals("Should be 5 transactions", 5, testWrapper.Invoices.Count);

			var headerCollection = testWrapper.Invoices;
			AssertEquals(-276.83m, headerCollection[0].ApportionedAmount); //USD PAY
			AssertEquals(256.23m, headerCollection[1].ApportionedAmount);  //USD INV
			AssertEquals(103.77m, headerCollection[2].ApportionedAmount); //AUD INV
			AssertEquals(140m, headerCollection[3].ApportionedAmount);  //EUR INV 
			AssertEquals(-123.17m, headerCollection[4].ApportionedAmount);  //EUR CRD

			AssertRemittanceAdviceIsBalanced(paymentInAUD, headerCollection);
		}

		public void TestApportionedAmountWhenMatchGroupIsBalancedWithAMiscellanouesTransaction_SelectedPaymentCurrencyIsForeign()
		{
			var paymentInUSD = CreateMatchGroupWithAMiscellanouesTransactionAndGetPayment(false);

			var testWrapper = DocAPPayment.New(paymentInUSD, Factory);
			AssertEquals("Should be 5 transactions", 5, testWrapper.Invoices.Count);

			var headerCollection = testWrapper.Invoices;
			AssertEquals(-65.7m, headerCollection[0].ApportionedAmount); //AUD PAY
			AssertEquals(195.43m, headerCollection[1].ApportionedAmount); //USD INV			
			AssertEquals(57.49m, headerCollection[2].ApportionedAmount); //AUD INV 
			AssertEquals(77.57m, headerCollection[3].ApportionedAmount); //EUR INV
			AssertEquals(-62.7m, headerCollection[4].ApportionedAmount); //EUR CRD

			AssertRemittanceAdviceIsBalanced(paymentInUSD, headerCollection);
		}

		public void TestApportionedAmountWhenMatchGroupIsBalancedWithAMiscellanouesTransaction_SelectedPaymentCurrencyIsLocal()
		{
			var paymentInAUD = CreateMatchGroupWithAMiscellanouesTransactionAndGetPayment(true);

			var testWrapper = DocAPPayment.New(paymentInAUD, Factory);
			AssertEquals("Should be 5 transactions", 5, testWrapper.Invoices.Count);

			var headerCollection = testWrapper.Invoices;
			AssertEquals(-276.83m, headerCollection[0].ApportionedAmount); //USD PAY
			AssertEquals(238.12m, headerCollection[1].ApportionedAmount); //USD INV
			AssertEquals(103.77m, headerCollection[2].ApportionedAmount); //AUD INV 
			AssertEquals(130.11m, headerCollection[3].ApportionedAmount); //EUR INV
			AssertEquals(-105.17m, headerCollection[4].ApportionedAmount); //EUR CRD

			AssertRemittanceAdviceIsBalanced(paymentInAUD, headerCollection);
		}

		public void TestApportionedAmountWhenSumOfEligibleTransactionsIsZero()
		{
			var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			var uSDCurrency = GetRefCurrency("USD");
			var eURCurrency = GetRefCurrency("EUR");

			var exchangeDifference = TestObjectCreator.CreateExchangeDifference<APExchangeDifference>(233.77m, ZDateTime.Now, TestObjectCreator.ABIGAS.PK);

			var paymentInUSD = CreateAPPayment(uSDCurrency, 0.73m, 276.83m, "00001001");
			var paymentInAUD = CreateAPPayment(localCurrency, 1m, 100m, "00001002");
			var paymentInEUR = CreateAPPayment(eURCurrency, 0.6124m, 113.17m, "00001003");
			var invoiceInUSD = CreateAPInvoice(uSDCurrency, 0.7627m, 256.23m, "00001004");

			CreateMatchLink(paymentInUSD, 276.83M, "GROUP1");
			CreateMatchLink(((IMatching)paymentInUSD).CurrentMatchGroup, invoiceInUSD, -256.23M, "GROUP1");
			CreateMatchLink(((IMatching)paymentInUSD).CurrentMatchGroup, paymentInAUD, 100M, "GROUP1");
			CreateMatchLink(((IMatching)paymentInUSD).CurrentMatchGroup, paymentInEUR, 113.17M, "GROUP1");
			CreateMatchLink(((IMatching)paymentInUSD).CurrentMatchGroup, exchangeDifference, -233.77M, "GROUP1");

			Factory.Save();

			var testWrapper = DocAPPayment.New(paymentInUSD, Factory);
			AssertEquals("Should be 3 transactions", 3, testWrapper.Invoices.Count);

			var headerCollection = testWrapper.Invoices;
			AssertEquals(89.27m, headerCollection[0].ApportionedAmount); //AUD PAY
			AssertEquals(-82.61m, headerCollection[1].ApportionedAmount); //EUR PAY			
			AssertEquals(195.43m, headerCollection[2].ApportionedAmount); //USD INV

			AssertRemittanceAdviceIsBalanced(paymentInUSD, headerCollection);
		}

		#region helpers

		void AssertRemittanceAdviceIsBalanced(APPayment selectedPayment, DocTransactionHeaderCollection headerCollection)
		{
			var invoiceAmount = selectedPayment.AH_OSTotal.Round(selectedPayment.TransactionCurrency.Decimals);
			ZDecimal totalApportionedAmount = 0m;
			for (int i = 0; i < headerCollection.Count; i++)
			{
				totalApportionedAmount += headerCollection[i].ApportionedAmount;
			}
			AssertEquals(invoiceAmount, totalApportionedAmount);
		}

		APPayment CreateAPPayment(RefCurrency currency, ZDecimal exchangeRate, ZDecimal invoiceAmount, ZString transactionNumber)
		{
			var apPayment = Factory.NewWithValidTestData<APPayment>();
			apPayment.AH_TransactionNum = transactionNumber;
			apPayment.AH_RX_NKTransactionCurrency = currency.RX_Code;
			apPayment.AH_ExchangeRate = exchangeRate;
			apPayment.AH_InvoiceAmount = invoiceAmount;
			apPayment.AH_OSTotal = invoiceAmount * exchangeRate;
			apPayment.AH_FullyPaidDate = ZDateTime.Now;
			return apPayment;
		}

		APInvoice CreateAPInvoice(RefCurrency currency, ZDecimal exchangeRate, ZDecimal invoiceAmount, ZString transactionNumber)
		{
			var apInvoice = Factory.New<APInvoice>();
			apInvoice.AH_TransactionNum = transactionNumber;
			apInvoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			apInvoice.AH_ExchangeRate = exchangeRate;

			var line = Factory.NewWithValidTestData<APInvoiceLine>();
			line.AL_RX_NKTransactionCurrency = currency.Code;
			line.AL_ExchangeRate = exchangeRate;
			line.AL_LocalExTaxAmount = invoiceAmount;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			apInvoice.Lines.Add(line);

			apInvoice.AH_OutstandingAmount = 0m;
			apInvoice.AH_FullyPaidDate = ZDateTime.Now;
			return apInvoice;
		}

		APCreditNote CreateAPCreditNote(RefCurrency currency, ZDecimal exchangeRate, ZDecimal invoiceAmount, ZString transactionNumber)
		{
			var apCreditNote = Factory.New<APCreditNote>();
			apCreditNote.AH_TransactionNum = transactionNumber;
			apCreditNote.AH_RX_NKTransactionCurrency = currency.RX_Code;
			apCreditNote.AH_ExchangeRate = exchangeRate;

			var creditNoteLine = Factory.New<APCreditNoteLine>();
			creditNoteLine.AL_RX_NKTransactionCurrency = currency.Code;
			creditNoteLine.AL_ExchangeRate = exchangeRate;
			creditNoteLine.AL_LocalExTaxAmount = invoiceAmount;
			creditNoteLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			apCreditNote.Lines.Add(creditNoteLine);

			apCreditNote.AH_OutstandingAmount = 0m;
			apCreditNote.AH_FullyPaidDate = ZDateTime.Now;
			return apCreditNote;
		}

		APPayment CreateMatchGroupWithoutMiscellanouesTransactionAndGetPayment(bool isTestingLocalCurrency)
		{
			var localCurrency = GetRefCurrency("AUD");
			var uSDCurrency = GetRefCurrency("USD");
			var eURCurrency = GetRefCurrency("EUR");

			var paymentInUSD = CreateAPPayment(uSDCurrency, 0.73m, 276.83m, "00001001");
			var paymentInAUD = CreateAPPayment(localCurrency, 1m, 100m, "00001002");
			var invoiceInUSD = CreateAPInvoice(uSDCurrency, 0.7627m, 256.23m, "00001003");
			var invoiceInAUD = CreateAPInvoice(localCurrency, 1m, 103.77m, "00001004");
			var invoiceInEUR = CreateAPInvoice(eURCurrency, 0.6597m, 140m, "00001005");
			var creditNoteInEUR = CreateAPCreditNote(eURCurrency, 0.6124m, 123.17m, "00001006");

			CreateMatchLink(paymentInUSD, 276.83m, "GROUP1");
			CreateMatchLink(((IMatching)paymentInUSD).CurrentMatchGroup, invoiceInUSD, -256.23m, "GROUP1");
			CreateMatchLink(((IMatching)paymentInUSD).CurrentMatchGroup, invoiceInAUD, -103.77m, "GROUP1");
			CreateMatchLink(((IMatching)paymentInUSD).CurrentMatchGroup, invoiceInEUR, -140m, "GROUP1");
			CreateMatchLink(((IMatching)paymentInUSD).CurrentMatchGroup, paymentInAUD, 100m, "GROUP1");
			CreateMatchLink(((IMatching)paymentInUSD).CurrentMatchGroup, creditNoteInEUR, 123.17, "GROUP1");

			Factory.Save();

			return (isTestingLocalCurrency) ? paymentInAUD : paymentInUSD;
		}

		APPayment CreateMatchGroupWithAMiscellanouesTransactionAndGetPayment(bool isTestingLocalCurrency)
		{
			var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			var uSDCurrency = GetRefCurrency("USD");
			var eURCurrency = GetRefCurrency("EUR");

			var exchangeDifference = TestObjectCreator.CreateExchangeDifference<APExchangeDifference>(-20m, ZDateTime.Now, TestObjectCreator.ABIGAS.PK);

			var paymentInUSD = CreateAPPayment(uSDCurrency, 0.73m, 276.83m, "00001001");
			var paymentInAUD = CreateAPPayment(localCurrency, 1m, 90m, "00001002");
			var invoiceInUSD = CreateAPInvoice(uSDCurrency, 0.7627m, 256.23m, "00001003");
			var invoiceInAUD = CreateAPInvoice(localCurrency, 1m, 103.77m, "00001004");
			var invoiceInEUR = CreateAPInvoice(eURCurrency, 0.6597m, 140m, "00001005");
			var creditNoteInEUR = CreateAPCreditNote(eURCurrency, 0.6124m, 113.17m, "00001006");

			CreateMatchLink(paymentInUSD, 276.83m, "GROUP1");
			CreateMatchLink(((IMatching)paymentInUSD).CurrentMatchGroup, invoiceInUSD, -256.23M, "GROUP1");
			CreateMatchLink(((IMatching)paymentInUSD).CurrentMatchGroup, invoiceInAUD, -103.77m, "GROUP1");
			CreateMatchLink(((IMatching)paymentInUSD).CurrentMatchGroup, invoiceInEUR, -140m, "GROUP1");
			CreateMatchLink(((IMatching)paymentInUSD).CurrentMatchGroup, paymentInAUD, 90M, "GROUP1");
			CreateMatchLink(((IMatching)paymentInUSD).CurrentMatchGroup, creditNoteInEUR, 113.17M, "GROUP1");
			CreateMatchLink(((IMatching)paymentInUSD).CurrentMatchGroup, exchangeDifference, 20M, "GROUP1");

			Factory.Save();
			return (isTestingLocalCurrency) ? paymentInAUD : paymentInUSD;
		}

		#endregion

		#endregion

		public void TestNumberOfDifferentCurrencies()
		{
			Payment.AH_TransactionNum = "PAY 000339";
			APInvoice invoice1 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 1");
			APInvoice invoice2 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 2");
			APInvoice invoice3 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 3");

			CreateMatchLink(Payment, 520M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice1, -200M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice2, -320M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice3, -80M, "GROUP1");
			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_Ledger = "AP";
			headerToMatch.AH_TransactionType = "INV";
			headerToMatch.AH_InvoiceAmount = 80M;
			headerToMatch.AH_FullyPaidDate = ZDateTime.Now;
			AccTransactionMatchLink linkToMatch = ((IMatching)Payment).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = headerToMatch.PK;
			linkToMatch.AP_Amount = 80M;
			linkToMatch.AP_MatchGroupNum = "GROUP1";

			RefCurrency aUCurr = GetRefCurrency("AUD");
			RefCurrency uSCurr = GetRefCurrency("USD");
			RefCurrency eUCurr = GetRefCurrency("EUR");
			headerToMatch.AH_RX_NKTransactionCurrency = uSCurr.RX_Code;
			Payment.AH_RX_NKTransactionCurrency = uSCurr.RX_Code;
			Payment.AH_ExchangeRate = 1.2m;
			invoice1.AH_RX_NKTransactionCurrency = uSCurr.RX_Code;
			invoice2.AH_RX_NKTransactionCurrency = aUCurr.RX_Code;
			invoice3.AH_RX_NKTransactionCurrency = uSCurr.RX_Code;

			CreateAPInvoiceLine(invoice1, 200m, uSCurr.RX_Code);
			invoice1.AH_FullyPaidDate = ZDateTime.Now;
			invoice1.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice2, 320m, aUCurr.RX_Code);
			invoice2.AH_FullyPaidDate = ZDateTime.Now;
			invoice2.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice3, 80m, uSCurr.RX_Code);
			invoice3.AH_FullyPaidDate = ZDateTime.Now;
			invoice3.AH_OutstandingAmount = 0m;
			Payment.AH_InvoiceAmount = 520M;
			Payment.AH_FullyPaidDate = ZDateTime.Now;
			TestObjectCreator.SetupMatchLinkMatchDate(Payment);

			Factory.Save();
			AssertEquals(2, PaymentWrapper.NumberOfDifferentCurrencies);

			invoice2.AH_RX_NKTransactionCurrency = uSCurr.RX_Code;
			invoice2.AH_InvoiceAmount = -320M;
			foreach (APInvoiceLine line in invoice2.Lines)
			{
				line.AL_RX_NKTransactionCurrency = invoice2.AH_RX_NKTransactionCurrency;
			}

			PaymentWrapper = DocAPPayment.New(Payment, Factory);
			AssertEquals(1, PaymentWrapper.NumberOfDifferentCurrencies);

			invoice3.AH_RX_NKTransactionCurrency = aUCurr.RX_Code;
			invoice1.AH_RX_NKTransactionCurrency = eUCurr.RX_Code;
			invoice1.AH_InvoiceAmount = -200M;
			invoice3.AH_InvoiceAmount = -80M;

			PaymentWrapper = DocAPPayment.New(Payment, Factory);
			AssertEquals(3, PaymentWrapper.NumberOfDifferentCurrencies);
		}

		[ExpectNoExceptions]
		public void TestChequePayToWithAddressNoExceptionWhenCountryCodeMissing()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			var aPOrg = Factory.New<OrgHeader>();
			aPOrg.OH_Code = "AP_ORG TEST";
			aPOrg.MainAddress.OA_Address1 = "Test Main Address";
			aPOrg.OH_FullName = "AP ORG";

			var contact = aPOrg.Contacts.AddNew();
			contact.OC_ContactName = "Jamie";
			OrgDocument doc = contact.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.Payables.Code;

			Payment.AH_GE = GlbDepartment.CurrentDepartment.PK;
			Payment.AH_GB = GlbBranch.CurrentBranch.PK;
			Payment.AH_OH = aPOrg.PK;
			Payment.AH_ChequeDrawer = "Somebody";
			Payment.AH_InvoiceDate = ZDateTime.Today;

			OrgAddress payablesForeignAddress = aPOrg.Addresses.AddNew();
			payablesForeignAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);
			payablesForeignAddress.OA_OH = aPOrg.PK;
			payablesForeignAddress.OA_Address1 = "Test Payables Address";
			payablesForeignAddress.OA_RN_NKCountryCode = "";
			payablesForeignAddress.OA_RL_NKRelatedPortCode = "";

			Factory.Save();

			AssertEquals("AP ORG\nTEST PAYABLES ADDRESS", PaymentWrapper.ChequePayToWithAddress.ToString());
		}

		public void TestRemittanceAdviceContactPAYType()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			AssertEquals("", PaymentWrapper.RemittanceAdviceContact);

			var aPOrg = Factory.New<OrgHeader>();
			aPOrg.OH_Code = "AP_ORG TEST";
			aPOrg.MainAddress.OA_Address1 = "Test Main Address";
			aPOrg.OH_FullName = "AP ORG";

			var contact = aPOrg.Contacts.AddNew();
			contact.OC_ContactName = "Jamie";
			OrgDocument doc = contact.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.Payables.Code;

			Payment.AH_GE = GlbDepartment.CurrentDepartment.PK;
			Payment.AH_GB = GlbBranch.CurrentBranch.PK;
			Payment.AH_OH = aPOrg.PK;
			Payment.AH_ChequeDrawer = "Somebody";
			Payment.AH_InvoiceDate = ZDateTime.Today;
			Factory.Save();

			AssertContains("JAMIE", PaymentWrapper.RemittanceAdviceContact);
			AssertContains("TEST MAIN ADDRESS", PaymentWrapper.RemittanceAdviceContact);
			AssertContains("TEST MAIN ADDRESS", PaymentWrapper.ChequePayToWithAddress);

			OrgAddress payablesForeignAddress = aPOrg.Addresses.AddNew();
			payablesForeignAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);
			payablesForeignAddress.OA_OH = aPOrg.PK;
			payablesForeignAddress.OA_Address1 = "Test Foreign Payables Address";
			payablesForeignAddress.OA_RL_NKRelatedPortCode = "USNLN";
			Factory.Save();

			AssertContains("JAMIE", PaymentWrapper.RemittanceAdviceContact);
			AssertContains("TEST FOREIGN PAYABLES ADDRESS", PaymentWrapper.RemittanceAdviceContact);
			AssertContains("TEST FOREIGN PAYABLES ADDRESS", PaymentWrapper.ChequePayToWithAddress);

			OrgAddress payablesAddress = aPOrg.Addresses.AddNew();
			payablesAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);
			payablesAddress.OA_OH = aPOrg.PK;
			payablesAddress.OA_Address1 = "Test Local Payables Address";
			payablesAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			AssertContains("JAMIE", PaymentWrapper.RemittanceAdviceContact);
			AssertContains("TEST LOCAL PAYABLES ADDRESS", PaymentWrapper.RemittanceAdviceContact);
			AssertContains("TEST LOCAL PAYABLES ADDRESS", PaymentWrapper.ChequePayToWithAddress);
		}

		public void TestChequePayTo()
		{
			AssertEquals("", PaymentWrapper.ChequePayTo);
			AssertEquals("", PaymentWrapper.Cheque.ChequePayTo);

			var aPOrg = Factory.New<OrgHeader>();
			aPOrg.OH_Code = "AP_ORG TEST";
			aPOrg.MainAddress.OA_Address1 = "AP_ORG Address";
			aPOrg.OH_FullName = "AP ORG";

			Payment.AH_OH = aPOrg.PK;
			Payment.AH_ChequeDrawer = "Drawer 123";
			PaymentWrapper = DocAPPayment.New(Payment, Factory);
			AssertEquals("AP ORG", PaymentWrapper.ChequePayTo);
			AssertEquals("AP ORG", PaymentWrapper.Cheque.ChequePayTo);
		}

		public void TestNotIsReversal()
		{
			AssertEquals(ZBool.False, PaymentWrapper.IsReversal);

			Payment.AH_InvoiceDate = ZDateTime.Today;
			Payment.AH_GE = GlbDepartment.CurrentDepartment.PK;
			Payment.AH_GB = GlbBranch.CurrentBranch.PK;
			Payment.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Payment;
			Payment.AH_FullyPaidDate = ZDateTime.Now;

			CreateMatchLink(Payment, 1000M, "GRPI000");
			Payment.AH_OSTotal = Payment.AH_InvoiceAmount = 1000M;

			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = -1000M;

			AccTransactionMatchLink linkToMatch = ((IMatching)Payment).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = headerToMatch.PK;
			linkToMatch.AP_Amount = -1000M;
			linkToMatch.AP_MatchGroupNum = "GRPI000";
			TestObjectCreator.SetupMatchLinkMatchDate(Payment);

			Factory.Save();
			AssertEquals("Not a reversal transaction", ZBool.False, PaymentWrapper.IsReversal);
		}

		public void TestIsReversal()
		{
			Payment.AH_IsCancelled = ZBool.True;

			CreateMatchLink(Payment, -1000M, "GRPI000");
			Payment.AH_OSTotal = Payment.AH_InvoiceAmount = -1000M;
			Payment.AH_FullyPaidDate = ZDateTime.Now;

			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = 1000M;

			AccTransactionMatchLink linkToMatch = ((IMatching)Payment).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = headerToMatch.PK;
			linkToMatch.AP_Amount = 1000M;
			linkToMatch.AP_MatchGroupNum = "GRPI000";
			TestObjectCreator.SetupMatchLinkMatchDate(Payment);

			Factory.Save();
			AssertEquals("Should be a reversal transaction", ZBool.True, PaymentWrapper.IsReversal);
		}

		public void TestShowOriginalAmountForPAY()
		{
			Payment.AH_TransactionNum = "PAY 000837";

			APInvoice invoice1 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 1");
			APInvoice invoice2 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 2");
			APInvoice invoice3 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 3");

			CreateMatchLink(Payment, 300M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice1, -200M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice2, -20M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice3, -80M, "GROUP1");

			RefCurrency aUCurr = GetRefCurrency("AUD");
			RefCurrency uSCurr = GetRefCurrency("USD");
			RefCurrency sGCurr = GetRefCurrency("SGD");

			Payment.AH_RX_NKTransactionCurrency = aUCurr.RX_Code;
			invoice1.AH_RX_NKTransactionCurrency = aUCurr.RX_Code;
			invoice2.AH_RX_NKTransactionCurrency = uSCurr.RX_Code;
			invoice3.AH_RX_NKTransactionCurrency = aUCurr.RX_Code;

			CreateAPInvoiceLine(invoice1, 200m, aUCurr.RX_Code);
			invoice1.AH_FullyPaidDate = ZDateTime.Now;
			invoice1.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice2, 20m, uSCurr.RX_Code);
			invoice2.AH_FullyPaidDate = ZDateTime.Now;
			invoice2.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice3, 80m, aUCurr.RX_Code);
			invoice3.AH_FullyPaidDate = ZDateTime.Now;
			invoice3.AH_OutstandingAmount = 0m;
			Payment.AH_OSTotal = Payment.AH_InvoiceAmount = 300M;
			Payment.AH_FullyPaidDate = ZDateTime.Now;

			Factory.Save();
			AssertEquals("Y", PaymentWrapper.ShowOriginalAmount.ToString());
			AssertEquals("Y", PaymentWrapper.ShowOriginalAmountForPAY.ToString());

			invoice2.AH_RX_NKTransactionCurrency = aUCurr.RX_Code;
			invoice2.AH_InvoiceAmount = -20M;

			PaymentWrapper = DocAPPayment.New(Payment, Factory);
			AssertEquals("N", PaymentWrapper.ShowOriginalAmount.ToString());
			AssertEquals("N", PaymentWrapper.ShowOriginalAmountForPAY.ToString());
		}

		public void TestNumberOfTransactionsAndTotalsOnCheque()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			AssertEquals("Must be true by default", ZBool.True, PaymentWrapper.PrintRemittanceOnCheque);

			APInvoice invoice1 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 1 ");
			APInvoice invoice2 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 2 ");
			APInvoice invoice3 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 3 ");

			RefCurrency localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			RefCurrency uSCurr = GetRefCurrency(Core.Constants.CurrencyCodes.UnitedStates);
			RefCurrency sGCurr = GetRefCurrency(Core.Constants.CurrencyCodes.Singapore);
			RefCurrency nZCurr = GetRefCurrency(Core.Constants.CurrencyCodes.NewZealand);

			Payment.AH_RX_NKTransactionCurrency = localCurrency.RX_Code;
			invoice1.AH_RX_NKTransactionCurrency = uSCurr.RX_Code;
			invoice2.AH_RX_NKTransactionCurrency = localCurrency.RX_Code;
			invoice3.AH_RX_NKTransactionCurrency = sGCurr.RX_Code;

			CreateMatchLink(Payment, 300M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice1, -200M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice2, -20M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice3, -80M, "GROUP1");

			CreateAPInvoiceLine(invoice1, 200m, uSCurr.RX_Code);
			invoice1.AH_FullyPaidDate = ZDateTime.Now;
			invoice1.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice2, 20m, localCurrency.RX_Code);
			invoice2.AH_FullyPaidDate = ZDateTime.Now;
			invoice2.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice3, 80m, sGCurr.RX_Code);
			invoice3.AH_FullyPaidDate = ZDateTime.Now;
			invoice3.AH_OutstandingAmount = 0m;

			Payment.AH_OSTotal = Payment.AH_InvoiceAmount = 300M;
			Payment.AH_FullyPaidDate = ZDateTime.Now;

			Factory.Save();

			PaymentWrapper = DocAPPayment.New(Payment, Factory);
			AssertEquals("NumberOfTransactionsAndTotalsOnCheque must be 7 lines, this is 3 lines with 4 rows for grouping", (ZDecimal)7, PaymentWrapper.NumberOfTransactionsAndTotalsOnCheque);
		}

		public void TestPrintRemittanceOnCheque()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			AssertEquals("Must be true by default", ZBool.True, PaymentWrapper.PrintRemittanceOnCheque);

			APInvoice invoice1 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 1 ");
			APInvoice invoice2 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 2 ");
			APInvoice invoice3 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 3 ");
			APInvoice invoice4 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 4 ");
			APInvoice invoice5 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 5 ");
			APInvoice invoice6 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 6 ");
			APInvoice invoice7 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 7 ");
			APInvoice invoice8 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 8 ");
			APInvoice invoice9 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 9 ");
			APInvoice invoice10 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 10");
			APInvoice invoice11 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 11");
			APInvoice invoice12 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 12");
			APInvoice invoice13 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 13");
			APInvoice invoice14 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 14");
			APInvoice invoice15 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 15");
			APInvoice invoice16 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 16");
			APInvoice invoice17 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 17");
			APInvoice invoice18 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 18");
			APInvoice invoice19 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 19");

			RefCurrency localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			RefCurrency uSCurr = GetRefCurrency(Core.Constants.CurrencyCodes.UnitedStates);
			RefCurrency sGCurr = GetRefCurrency(Core.Constants.CurrencyCodes.Singapore);
			RefCurrency nZCurr = GetRefCurrency(Core.Constants.CurrencyCodes.NewZealand);

			Payment.AH_RX_NKTransactionCurrency = localCurrency.RX_Code;
			invoice1.AH_RX_NKTransactionCurrency = uSCurr.RX_Code;
			invoice2.AH_RX_NKTransactionCurrency = localCurrency.RX_Code;
			invoice3.AH_RX_NKTransactionCurrency = sGCurr.RX_Code;

			CreateMatchLink(Payment, 1580M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice1, -200M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice2, -20M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice3, -80M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice4, -80M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice5, -80M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice6, -80M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice7, -80M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice8, -80M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice9, -80M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice10, -80M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice11, -80M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice12, -80M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice13, -80M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice14, -80M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice15, -80M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice16, -80M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice17, -80M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice18, -80M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice19, -80M, "GROUP1");

			CreateAPInvoiceLine(invoice1, 200m, uSCurr.RX_Code);
			invoice1.AH_FullyPaidDate = ZDateTime.Now;
			invoice1.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice2, 20m, localCurrency.RX_Code);
			invoice2.AH_FullyPaidDate = ZDateTime.Now;
			invoice2.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice3, 80m, sGCurr.RX_Code);
			invoice3.AH_FullyPaidDate = ZDateTime.Now;
			invoice3.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice4, 80M);
			invoice4.AH_FullyPaidDate = ZDateTime.Now;
			invoice4.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice5, 80M);
			invoice5.AH_FullyPaidDate = ZDateTime.Now;
			invoice5.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice6, 80M);
			invoice6.AH_FullyPaidDate = ZDateTime.Now;
			invoice6.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice7, 80M);
			invoice7.AH_FullyPaidDate = ZDateTime.Now;
			invoice7.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice8, 80M);
			invoice8.AH_FullyPaidDate = ZDateTime.Now;
			invoice8.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice9, 80M);
			invoice9.AH_FullyPaidDate = ZDateTime.Now;
			invoice9.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice10, 80M);
			invoice10.AH_FullyPaidDate = ZDateTime.Now;
			invoice10.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice11, 80M);
			invoice11.AH_FullyPaidDate = ZDateTime.Now;
			invoice11.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice12, 80M);
			invoice12.AH_FullyPaidDate = ZDateTime.Now;
			invoice12.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice13, 80M);
			invoice13.AH_FullyPaidDate = ZDateTime.Now;
			invoice13.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice14, 80M);
			invoice14.AH_FullyPaidDate = ZDateTime.Now;
			invoice14.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice15, 80M);
			invoice15.AH_FullyPaidDate = ZDateTime.Now;
			invoice15.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice16, 80M);
			invoice16.AH_FullyPaidDate = ZDateTime.Now;
			invoice16.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice17, 80M);
			invoice17.AH_FullyPaidDate = ZDateTime.Now;
			invoice17.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice18, 80M);
			invoice18.AH_FullyPaidDate = ZDateTime.Now;
			invoice18.AH_OutstandingAmount = 0m;
			CreateAPInvoiceLine(invoice19, 80M);
			invoice19.AH_FullyPaidDate = ZDateTime.Now;
			invoice19.AH_OutstandingAmount = 0m;
			Payment.AH_OSTotal = Payment.AH_InvoiceAmount = 1580M;
			Payment.AH_FullyPaidDate = ZDateTime.Now;

			Factory.Save();

			PaymentWrapper = DocAPPayment.New(Payment, Factory);
			AssertEquals("Cheque can fit 24 lines, this is 19 lines with 4 rows for currency grouping = 23 lines.", "Y", PaymentWrapper.PrintRemittanceOnCheque.ToString());
			AssertEquals("Number of invoices in 'First Page Invoices'", 19, PaymentWrapper.FirstPageInvoices.Count);

			invoice4.AH_RX_NKTransactionCurrency = nZCurr.RX_Code;
			invoice4.AH_InvoiceAmount = -80M;

			PaymentWrapper = DocAPPayment.New(Payment, Factory);
			AssertEquals("Cheque CANNOT fit this: 19 lines with 6 rows for currency grouping = 25 lines.", "N", PaymentWrapper.PrintRemittanceOnCheque.ToString());
			AssertEquals("Number of invoices in 'First Page Invoices'", 18, PaymentWrapper.FirstPageInvoices.Count);
		}

		public void TestPaymentCurrency()
		{
			RefCurrency aUCurr = GetRefCurrency("AUD");
			RefCurrency uSCurr = GetRefCurrency("USD");

			Payment.AH_RX_NKTransactionCurrency = uSCurr.RX_Code;
			Payment.AH_ExchangeRate = 1.2m;
			Factory.Save();
			AssertEquals("USD", PaymentWrapper.PaymentCurrency.Code);
			AssertEquals("USD", PaymentWrapper.Cheque.PaymentCurrency.Code);
		}

		[Enterprise.Accounting.Integration.Testing.SuspendCriticalValidateTransactionHeaderSameCompany]
		public void TestPayments()
		{
			Payment.AH_TransactionNum = "XXX1";

			APInvoice invoice1 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 1");
			APExchangeDifference invoice2 = Factory.New<APExchangeDifference>();
			APDiscount invoice3 = Factory.New<APDiscount>();
			APJournal invoice4 = Factory.New<APJournal>();
			APInvoice invoice5 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 2");
			invoice5.AH_Desc = "SYSTEM GENERATED Invoice";
			invoice5.AH_TransactionCreatedByMatching = true;

			CreateMatchLink(Payment, 300M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice1, -200M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice2, -20M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice3, -70M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice4, -10M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice5, -120M, "GROUP1");

			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			headerToMatch.AH_Ledger = "AP";
			headerToMatch.AH_TransactionType = "INV";
			headerToMatch.AH_RX_NKTransactionCurrency = GlbBranch.CurrentBranch.Country.RN_RX_NKLocalCurrency;
			headerToMatch.AH_InvoiceAmount = 120M;
			headerToMatch.AH_FullyPaidDate = ZDateTime.Now;
			AccTransactionMatchLink linkToMatch = ((IMatching)Payment).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = headerToMatch.PK;
			linkToMatch.AP_Amount = 120M;
			linkToMatch.AP_MatchGroupNum = "GROUP1";

			CreateAPInvoiceLine(invoice1, 200M);
			invoice2.AH_InvoiceAmount = invoice2.AH_OSExTaxAmount = -20M;
			invoice3.AH_InvoiceAmount = invoice3.AH_OSExTaxAmount = -70M;
			invoice4.AH_InvoiceAmount = invoice4.AH_OSExTaxAmount = -10M;
			CreateAPInvoiceLine(invoice5, 120M);
			Payment.AH_InvoiceAmount = Payment.AH_OSExTaxAmount = 300M;

			invoice1.AH_OutstandingAmount = 0M;
			invoice1.AH_FullyPaidDate = ZDateTime.Now;
			invoice2.AH_OutstandingAmount = 0M;
			invoice2.AH_FullyPaidDate = ZDateTime.Now;
			invoice3.AH_OutstandingAmount = 0M;
			invoice3.AH_FullyPaidDate = ZDateTime.Now;
			invoice4.AH_OutstandingAmount = 0M;
			invoice4.AH_FullyPaidDate = ZDateTime.Now;
			invoice5.AH_OutstandingAmount = 0M;
			invoice5.AH_FullyPaidDate = ZDateTime.Now;
			Payment.AH_OutstandingAmount = 0M;
			Payment.AH_FullyPaidDate = ZDateTime.Now;
			TestObjectCreator.SetupMatchLinkMatchDate(Payment);

			Factory.Save();

			AssertEquals(5, PaymentWrapper.Payments.Count);
			AssertEquals(4, PaymentWrapper.Invoices.Count);
			AssertEquals(6, PaymentWrapper.ReceiptMatches.Count);
		}

		[Enterprise.Accounting.Integration.Testing.SuspendCriticalValidateTransactionHeaderSameCompany]
		public void TestFirstPageInvoicesRowCount()
		{
			PaymentWrapper = DocAPPayment.New(Payment, Factory);
			AssertEquals(0, PaymentWrapper.FirstPageInvoicesRowCount);

			APInvoice invoice1 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 1 ");
			APInvoice invoice2 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 2 ");

			RefCurrency localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			RefCurrency uSCurr = GetRefCurrency(Core.Constants.CurrencyCodes.UnitedStates);
			RefCurrency sGCurr = GetRefCurrency(Core.Constants.CurrencyCodes.Singapore);
			RefCurrency nZCurr = GetRefCurrency(Core.Constants.CurrencyCodes.NewZealand);

			Payment.AH_RX_NKTransactionCurrency = localCurrency.RX_Code;
			invoice1.AH_RX_NKTransactionCurrency = uSCurr.RX_Code;
			invoice2.AH_RX_NKTransactionCurrency = localCurrency.RX_Code;

			CreateMatchLink(Payment, 300M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice1, -200M, "GROUP1");
			CreateMatchLink(((IMatching)Payment).CurrentMatchGroup, invoice2, -80M, "GROUP1");

			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			headerToMatch.AH_Ledger = "AP";
			headerToMatch.AH_TransactionType = "PAY";
			headerToMatch.AH_RX_NKTransactionCurrency = GlbBranch.CurrentBranch.Country.RN_RX_NKLocalCurrency;
			headerToMatch.AH_InvoiceAmount = -20M;
			headerToMatch.AH_FullyPaidDate = ZDateTime.Now;
			AccTransactionMatchLink linkToMatch = ((IMatching)Payment).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = headerToMatch.PK;
			linkToMatch.AP_Amount = -20M;
			linkToMatch.AP_MatchGroupNum = "GROUP1";

			CreateAPInvoiceLine(invoice1, 200m, uSCurr.RX_Code, 1m);
			CreateAPInvoiceLine(invoice2, 80M, localCurrency.RX_Code, 1m);

			Payment.AH_InvoiceAmount = Payment.AH_OSExTaxAmount = 300M;

			invoice1.AH_OutstandingAmount = 0M;
			invoice1.AH_FullyPaidDate = ZDateTime.Now;
			invoice2.AH_OutstandingAmount = 0M;
			invoice2.AH_FullyPaidDate = ZDateTime.Now;
			Payment.AH_OutstandingAmount = 0M;
			Payment.AH_FullyPaidDate = ZDateTime.Now;

			TestObjectCreator.SetupMatchLinkMatchDate(Payment);

			Factory.Save();

			PaymentWrapper = DocAPPayment.New(Payment, Factory);
			AssertEquals(2, PaymentWrapper.FirstPageInvoicesRowCount);
		}

		public void TestCheque()
		{
			var aUD = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
			Payment.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Payment;
			Payment.AH_OSTotalAmount = 1000M;
			Payment.AH_RX_NKTransactionCurrency = aUD.RX_Code;

			AssertNotNull(PaymentWrapper.Cheque);
			AssertEquals("ONE THOUSAND DOLLARS ONLY", PaymentWrapper.Cheque.ChequeAmountInWords);
		}

		public void TestPostDateSplit()
		{
			var aUD = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
			Payment.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Payment;
			Payment.AH_OSTotalAmount = 1000M;
			Payment.AH_PostDate = ZDateTime.Today;
			Payment.AH_RX_NKTransactionCurrency = aUD.RX_Code;

			ZString todayInCorrectFormat = "";
			todayInCorrectFormat += ZDateTime.Today.ToString("dd").Substring(0, 1) + "   ";
			todayInCorrectFormat += ZDateTime.Today.ToString("dd").Substring(1, 1) + "   ";
			todayInCorrectFormat += ZDateTime.Today.ToString("MM").Substring(0, 1) + "   ";
			todayInCorrectFormat += ZDateTime.Today.ToString("MM").Substring(1, 1) + "   ";
			todayInCorrectFormat += ZDateTime.Today.ToString("yy").Substring(0, 1) + "   ";
			todayInCorrectFormat += ZDateTime.Today.ToString("yy").Substring(1, 1);

			AssertEquals("PostDateSplit", todayInCorrectFormat, PaymentWrapper.PostDateSplit);
		}

		#endregion

		#region TestBarcode

		public void TestBarcode()
		{
			Payment.AH_TransactionNum = "ABC";
			AssertEquals(true, PaymentWrapper.Barcode.Contains(Core.Constants.DocManagerCodes.APPayment + "="));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			Payment = Factory.New<APPayment>();
			PaymentWrapper = DocAPPayment.New(Payment, Factory);
			base.SetUp();
		}

		APPayment Payment;
		DocAPPayment PaymentWrapper;

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
					DocAPPayment.New(Payment, Factory)
			};
		}

		APInvoice CreateInvoice(ZString type, ZString transactionNumber)
		{
			var invoice = Factory.New<APInvoice>();
			invoice.AH_TransactionNum = transactionNumber;
			invoice.AH_TransactionType = type;
			return invoice;
		}

		APInvoiceLine CreateAPInvoiceLine(APInvoice invoice, decimal amount, string currency = "AUD", decimal rate = 1m)
		{
			APInvoiceLine line = Factory.NewWithValidTestData<APInvoiceLine>();

			line.AL_RX_NKTransactionCurrency = currency;
			line.AL_ExchangeRate = rate;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			line.AL_LocalExTaxAmount = amount;

			invoice.Lines.Add(line);
			return line;
		}

		public void CreateMatchLink(TransactionHeader invoice, ZDecimal amount, ZString groupNum)
		{
			CreateMatchLink(((IMatching)invoice).CurrentMatchGroup, invoice, amount, groupNum);
		}

		public void CreateMatchLink(TransactionMatchLinkGroup group, TransactionHeader header, ZDecimal amount, ZString groupNum)
		{
			AccTransactionMatchLink matchLink = group.AddNew();
			matchLink.AP_AH = header.PK;
			matchLink.AP_Amount = amount;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = groupNum;
		}

		RefCurrency GetRefCurrency(ZString currencyCode)
		{
			return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);
		}

		#endregion
	}
}
