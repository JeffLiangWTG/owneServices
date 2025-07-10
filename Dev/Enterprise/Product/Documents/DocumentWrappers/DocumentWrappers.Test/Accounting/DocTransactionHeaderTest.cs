using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Barcode.Business;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocTransactionHeader))]
	sealed class DocTransactionHeaderTest : DocumentWrapperTestCase
	{
		#region Construction

		public void TestNew()
		{
			AssertEquals("The new method should return should be of type", typeof(DocTransactionHeader), DocTransactionHeader.New(Invoice, Factory).GetType());

			DummyDocTransactionHeader.Register();
			AssertEquals("The new method should return should be of type", typeof(DummyDocTransactionHeader), DocTransactionHeader.New(Invoice, Factory).GetType());
		}

		#endregion

		#region Local Amount & Exchange Rate

		public void TestShowLocalAmountAndExRateOnInvoice()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader client = orgFactory.NewWithValidTestData<OrgHeader>();
			orgFactory.Save();

			AssertEquals(ZBool.False, TransactionWrapper.ShowLocalAmountAndExRateOnInvoice);

			client.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup group = client.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			orgFactory.Save();

			Invoice.AH_OH = client.PK;
			Invoice.AH_JH = Factory.NewJobForTesting<JobHeader>().PK;
			AssertEquals(false, TransactionWrapper.ShowLocalAmountAndExRateOnInvoice);

			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.AllExRate;
			orgFactory.Save();
			AssertEquals(true, TransactionWrapper.ShowLocalAmountAndExRateOnInvoice);
		}

		#endregion

		#region Amounts

		public void TestInvoiceAmountBalance_WithHighPrecisionExchangeRate()
		{
			// EndOfStatementPeriodForCalculatingMatchedAmount is 12:00:00 AM of the next day after the end of period
			// Use it as a base for other dates
			ZDateTime now = ZDateTime.Today.AddDays(1);

			AssertEquals("Invoice amount", 0M, TransactionWrapper.InvoiceAmountWithGST);
			AssertEquals("Invoice balance", 0M, TransactionWrapper.Balance);

			InvoicingLineBase invLine = (InvoicingLineBase)Invoice.Lines.AddNew();
			Invoice.AH_PostDate = ZDateTime.Today;
			Invoice.AH_InvoiceDate = ZDateTime.Today.AddDays(-3);
			invLine.AL_OSTaxAmount = 0M;
			invLine.AL_OSExTaxAmount = 5410.32M;
			Invoice.AH_ExchangeRate = 0.456789M;
			Invoice.AH_OSTotalAmount = 5410.32M;
			Invoice.AH_RX_NKTransactionCurrency = "SIN";
			Invoice.AH_DueDate = ZDateTime.Today;

			AssertEquals("Invoice Balance is fully outstanding", 5410.32M, TransactionWrapper.Balance);

			AccTransactionMatchLink matchLinkForARInvoice = CreateMatchLink(Invoice, 350M, now.AddDays(-2));

			TransactionWrapper.EndOfStatementPeriodForCalculatingMatchedAmount = now.AddDays(-5);
			AssertEquals("End of statement period is in the past. There are no MatchLinks before that date. Transaction is fully outstanding.", 5410.32M, this.TransactionWrapper.Balance);

			this.TransactionWrapper.EndOfStatementPeriodForCalculatingMatchedAmount = now.AddDays(-1);
			AssertEquals("Using MatchLinks to calculate Invoice Balance for the past", 5250.32M, TransactionWrapper.Balance);

			this.TransactionWrapper.EndOfStatementPeriodForCalculatingMatchedAmount = now;
			AssertEquals("No MatchLinks used to calculate Invoice Balance for today", 5250.32M, TransactionWrapper.Balance);

			TransactionWrapper.EndOfStatementPeriodForCalculatingMatchedAmount = now.AddDays(5);
			AssertEquals("No MatchLinks used to calculate Invoice Balance for the future", 5250.32M, TransactionWrapper.Balance);

			// Simulate paying by web service without creating MatchLinks
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			matchLinkForARInvoice.Delete();

			AssertEquals("Invoice Balance", 5250.32M, this.TransactionWrapper.Balance);

			TransactionWrapper.EndOfStatementPeriodForCalculatingMatchedAmount = now; // Period last day is today
			AssertEquals("Invoice Balance", 5250.32M, this.TransactionWrapper.Balance);

			TransactionWrapper.EndOfStatementPeriodForCalculatingMatchedAmount = now.AddDays(-5);
			AssertEquals("Invoice Balance for previous period should be calculated from MatchLinks, but they are missing", 5410.32M, TransactionWrapper.Balance);
		}

		public void TestShowingOfSystemGeneratedJournalsForReceiptMatchingDocument()
		{
			var receipt = Factory.New<ARReceipt>();
			DocTransactionHeader wrapper = DocTransactionHeader.New(receipt, Factory);
			var journal1AR = Factory.New<ARJournal>();
			var journal2AR = Factory.New<ARJournal>();

			SetTransactionHeaderSettings(receipt, -300M);
			SetTransactionHeaderSettings(journal1AR, 100M);
			SetTransactionHeaderSettings(journal2AR, 300M);
			SetTransactionHeaderSettings(Invoice, -300M);

			SetMatchLink(receipt, receipt.PK, 100M, "GROUP1");
			SetMatchLink(receipt, Invoice.PK, -300M, "GROUP1");
			SetMatchLink(receipt, journal1AR.PK, 100M, "GROUP1");
			SetMatchLink(receipt, journal2AR.PK, 100M, "GROUP1");
			journal1AR.AH_TransactionCreatedByMatching = true;
			journal1AR.AH_TransactionCategory = "CLR";
			journal2AR.AH_TransactionCreatedByMatching = true;
			journal2AR.AH_TransactionCategory = "CLR";

			receipt.AH_OutstandingAmount = 200M;
			Invoice.AH_OutstandingAmount = 0M;
			journal1AR.AH_OutstandingAmount = 0M;
			journal2AR.AH_OutstandingAmount = 200M;

			Factory.Save();

			AssertEquals("Should be 2 Transactions, an Invoice and a Receipt", 2, wrapper.FilteredFlattenedReceiptMatches.Count);

			journal1AR.AH_TransactionCategory = "STD";
			journal2AR.AH_TransactionCategory = "STD";
			wrapper = DocTransactionHeader.New(receipt, Factory);
			Factory.Save();

			AssertEquals("Should be 4 Transactions, an Invoice, a Receipd and 2 Bank Fee Journals", 4, wrapper.FilteredFlattenedReceiptMatches.Count);
		}

		public void TestShowingOfSystemGeneratedJournalsForMatchingDocument()
		{
			var receipt = Factory.New<ARReceipt>();
			DocTransactionHeader wrapper = DocTransactionHeader.New(receipt, Factory);
			var journal1AR = Factory.New<ARJournal>();
			var journal2AR = Factory.New<ARJournal>();

			SetTransactionHeaderSettings(receipt, -300M);
			SetTransactionHeaderSettings(journal1AR, 100M);
			SetTransactionHeaderSettings(journal2AR, 300M);
			SetTransactionHeaderSettings(Invoice, -300M);

			SetMatchLink(receipt, receipt.PK, 100M, "GROUP1");
			SetMatchLink(receipt, Invoice.PK, -300M, "GROUP1");
			SetMatchLink(receipt, journal1AR.PK, 100M, "GROUP1");
			SetMatchLink(receipt, journal2AR.PK, 100M, "GROUP1");
			journal1AR.AH_TransactionCreatedByMatching = true;
			journal2AR.AH_TransactionCreatedByMatching = true;

			receipt.AH_OutstandingAmount = 200M;
			Invoice.AH_OutstandingAmount = 0M;
			journal1AR.AH_OutstandingAmount = 0M;
			journal2AR.AH_OutstandingAmount = 200M;

			Factory.Save();

			AssertEquals("Should be 4 Transactions, an Invoice and a Receipt and 2 Journals(System Generated)", 4, wrapper.FlattenedReceiptMatches.Count);
		}

		public void TestInvoiceAmountBalanceAndDueDate()
		{
			AssertEquals("Invoice amount", 0M, TransactionWrapper.InvoiceAmountWithGST);
			AssertEquals("Invoice balance", 0M, TransactionWrapper.Balance);

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.SetRate_ForTestOnly(3333, 100);
			InvoicingLineBase invLine = (InvoicingLineBase)Invoice.Lines.AddNew();
			Invoice.AH_PostDate = ZDateTime.Today;
			Invoice.AH_InvoiceDate = ZDateTime.Today;
			invLine.AL_AT = taxRate.PK;
			invLine.AL_OSTaxAmount = 50M;
			invLine.AL_OSExTaxAmount = 150M;
			Invoice.AH_ExchangeRate = 1M;
			Invoice.AH_OSTotalAmount = 200M;
			Invoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Invoice.AH_OutstandingAmount = 150M;
			Invoice.AH_DueDate = ZDateTime.Today;
			AssertEquals("Invoice due date", ZDateTime.Today, TransactionWrapper.TransactionDueDate);

			AssertEquals("Pre-condition: RegistryItem is False by default", false, AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.Value);
			AssertEquals("Invoice amount", 200M, TransactionWrapper.InvoiceAmountWithGST);

			AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Invoice amount", 200M, TransactionWrapper.InvoiceAmountWithGST);
		}

		public void TestAdjustmentNoteAmounts()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.SetRate_ForTestOnly(3333, 100);
			ARAdjustmentNote adjustmentNote = Factory.New<ARAdjustmentNote>();
			TransactionWrapper = DocTransactionHeader.New(adjustmentNote, Factory);

			AssertEquals("Adjustment note amount", 0M, TransactionWrapper.InvoiceAmountWithGST);
			AssertEquals("Adjustment note balance", 0M, TransactionWrapper.Balance);

			InvoicingLineBase adjLine = (InvoicingLineBase)adjustmentNote.Lines.AddNew();
			adjustmentNote.AH_PostDate = ZDateTime.Today;
			adjustmentNote.AH_InvoiceDate = ZDateTime.Today;
			adjLine.AL_AT = taxRate.PK;
			adjLine.AL_OSTaxAmount = 50M;
			adjLine.AL_OSExTaxAmount = 150M;
			adjustmentNote.AH_ExchangeRate = 1M;
			adjustmentNote.AH_OSTotalAmount = 200M;
			adjustmentNote.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			adjustmentNote.AH_OutstandingAmount = 150M;
			adjustmentNote.AH_DueDate = ZDateTime.Today;
			AssertEquals("Adjustment note due date", ZDateTime.Today, TransactionWrapper.TransactionDueDate);

			AssertEquals("Pre-condition: RegistryItem is False by default", false, AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.Value);
			AssertEquals("Adjustment Note amount", 200M, TransactionWrapper.InvoiceAmountWithGST);

			AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Adjustment Note amount", 200M, TransactionWrapper.InvoiceAmountWithGST);
		}

		public void TestCreditNoteAmounts()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.SetRate_ForTestOnly(3333, 100);
			var creditNote = Factory.New<ARCreditNote>();
			TransactionWrapper = DocTransactionHeader.New(creditNote, Factory);

			AssertEquals("Credit note amount", 0M, TransactionWrapper.InvoiceAmountWithGST);
			AssertEquals("Credit note balance", 0M, TransactionWrapper.Balance);

			InvoicingLineBase crdLine = (InvoicingLineBase)creditNote.Lines.AddNew();
			creditNote.AH_InvoiceDate = ZDateTime.Now;
			creditNote.AH_PostDate = ZDateTime.Now;
			creditNote.AH_RX_NKTransactionCurrency = "GBP";
			creditNote.AH_ExchangeRate = 2M;
			crdLine.AL_AT = taxRate.PK;
			crdLine.AL_OSTaxAmount = 50M;
			crdLine.AL_OSExTaxAmount = 150M;
			creditNote.AH_OSTaxAmount = 50M;
			creditNote.AH_OSExTaxAmount = 150M;
			creditNote.AH_LocalOutstandingAmount = 100M;
			creditNote.AH_DueDate = ZDateTime.Today;
			creditNote.AH_FullyPaidDate = ZDateTime.Empty;
			creditNote.AH_WithholdingTax = 10M;

			AssertEquals("Credit note due date", ZDateTime.Today, TransactionWrapper.TransactionDueDate);

			AssertEquals("Pre-condition: RegistryItem is False by default", false, AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.Value);
			AssertEquals("InvoiceAmountWithGST", -200M, TransactionWrapper.InvoiceAmountWithGST);
			AssertEquals("OSOutstandingAmount", 200M, TransactionWrapper.OSOutstandingAmount);
			AssertEquals("OSGstAmount", 50M, TransactionWrapper.OSGstAmount);
			AssertEquals("OSExTaxAmount", 150M, TransactionWrapper.OSExTaxAmount);
			AssertEquals("TotalLocalInvoiceAmount", 100M, TransactionWrapper.TotalLocalInvoiceAmount);
			AssertEquals("GSTAmount", 25M, TransactionWrapper.GSTAmount);
			AssertEquals("InvoiceAmount", 75M, TransactionWrapper.InvoiceAmount);
			AssertEquals("OSTotal", 200M, TransactionWrapper.OSTotal);
			AssertEquals("OutstandingAmount", -100M, TransactionWrapper.OutstandingAmount);
			AssertEquals("WithholdingTax", 10M, TransactionWrapper.WithholdingTax);

			AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("InvoiceAmountWithGST should be with opposite sign", 200M, TransactionWrapper.InvoiceAmountWithGST);
			AssertEquals("OSOutstandingAmount should be with opposite sign", -200M, TransactionWrapper.OSOutstandingAmount);
			AssertEquals("OSGstAmount should be with opposite sign", -50M, TransactionWrapper.OSGstAmount);
			AssertEquals("OSExTaxAmount should be with opposite sign", -150M, TransactionWrapper.OSExTaxAmount);
			AssertEquals("TotalLocalInvoiceAmount should be with opposite sign", -100M, TransactionWrapper.TotalLocalInvoiceAmount);
			AssertEquals("GSTAmount should be with opposite sign", -25M, TransactionWrapper.GSTAmount);
			AssertEquals("InvoiceAmount should be with opposite sign", -75M, TransactionWrapper.InvoiceAmount);
			AssertEquals("OSTotal should be with opposite sign", -200M, TransactionWrapper.OSTotal);
			AssertEquals("OutstandingAmount should be with opposite sign", 100M, TransactionWrapper.OutstandingAmount);
			AssertEquals("WithholdingTax should be with opposite sign", -10M, TransactionWrapper.WithholdingTax);
		}

		public void TestReceiptAmountWithGST()
		{
			var receipt = Factory.New<ARReceipt>();
			TransactionWrapper = DocTransactionHeader.New(receipt, Factory);

			AssertEquals("Receipt amount", 0M, TransactionWrapper.InvoiceAmountWithGST);
			AssertEquals("Receipt balance", 0M, TransactionWrapper.Balance);
			AssertEquals("Receipt due date", ZDateTime.Empty, TransactionWrapper.TransactionDueDate);

			receipt.AH_PostDate = ZDateTime.Today;
			receipt.AH_InvoiceDate = ZDateTime.Today;
			receipt.AH_OSTaxAmount = 50M;
			receipt.AH_ExchangeRate = 1M;
			receipt.AH_OSExTaxAmount = 150M;
			receipt.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			receipt.AH_LocalOutstandingAmount = 150.00M;
			receipt.AH_DueDate = ZDateTime.Today;

			AssertEquals("Receipt due date", ZDateTime.Empty, TransactionWrapper.TransactionDueDate);

			AssertEquals("Pre-condition: RegistryItem is False by default", false, AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.Value);
			AssertEquals("Receipt amount", -200M, TransactionWrapper.InvoiceAmountWithGST);

			AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Receipt amount", -200M, TransactionWrapper.InvoiceAmountWithGST);
		}

		public void TestTransaferFromPositiveAmountWithGST()
		{
			var transfer = Factory.New<ARTransferFromRow>();
			TransactionWrapper = DocTransactionHeader.New(transfer, Factory);

			AssertEquals("Transfer amount", 0M, TransactionWrapper.InvoiceAmountWithGST);
			AssertEquals("Transfer balance", 0M, TransactionWrapper.Balance);
			AssertEquals("Transfer due date", ZDateTime.Empty, TransactionWrapper.TransactionDueDate);

			transfer.AH_PostDate = ZDateTime.Today;
			transfer.AH_InvoiceDate = ZDateTime.Today;
			transfer.AH_OSExTaxAmount = 400M;

			AssertEquals("Transfer due date", ZDateTime.Empty, TransactionWrapper.TransactionDueDate);

			AssertEquals("Pre-condition: RegistryItem is False by default", false, AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.Value);
			AssertEquals("Transfer amount", -400M, TransactionWrapper.InvoiceAmountWithGST);

			AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Transfer amount", -400M, TransactionWrapper.InvoiceAmountWithGST);
		}

		public void TestTransaferFromNegativeAmountWithGST()
		{
			var transfer = Factory.New<ARTransferFromRow>();
			TransactionWrapper = DocTransactionHeader.New(transfer, Factory);

			AssertEquals("Transfer amount", 0M, TransactionWrapper.InvoiceAmountWithGST);
			AssertEquals("Transfer balance", 0M, TransactionWrapper.Balance);
			AssertEquals("Transfer due date", ZDateTime.Empty, TransactionWrapper.TransactionDueDate);

			transfer.AH_PostDate = ZDateTime.Today;
			transfer.AH_InvoiceDate = ZDateTime.Today;
			transfer.AH_OSExTaxAmount = -400M;

			AssertEquals("Transfer due date", ZDateTime.Empty, TransactionWrapper.TransactionDueDate);

			AssertEquals("Pre-condition: RegistryItem is False by default", false, AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.Value);
			AssertEquals("Transfer amount", 400M, TransactionWrapper.InvoiceAmountWithGST);

			AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Transfer amount", 400M, TransactionWrapper.InvoiceAmountWithGST);
		}

		public void TestTransaferToNegativeAmountWithGST()
		{
			var transfer = Factory.New<ARTransferToRow>();
			TransactionWrapper = DocTransactionHeader.New(transfer, Factory);

			AssertEquals("Transfer amount", 0M, TransactionWrapper.InvoiceAmountWithGST);
			AssertEquals("Transfer balance", 0M, TransactionWrapper.Balance);
			AssertEquals("Transfer due date", ZDateTime.Empty, TransactionWrapper.TransactionDueDate);

			transfer.AH_PostDate = ZDateTime.Today;
			transfer.AH_InvoiceDate = ZDateTime.Today;
			transfer.AH_OSTotal = -400M;

			AssertEquals("Transfer due date", ZDateTime.Empty, TransactionWrapper.TransactionDueDate);

			AssertEquals("Pre-condition: RegistryItem is False by default", false, AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.Value);
			AssertEquals("Transfer amount", -400M, TransactionWrapper.InvoiceAmountWithGST);

			AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Transfer amount", -400M, TransactionWrapper.InvoiceAmountWithGST);
		}

		public void TestTransaferToPostivieAmountWithGST()
		{
			var transfer = Factory.New<ARTransferToRow>();
			TransactionWrapper = DocTransactionHeader.New(transfer, Factory);

			AssertEquals("Transfer amount", 0M, TransactionWrapper.InvoiceAmountWithGST);
			AssertEquals("Transfer balance", 0M, TransactionWrapper.Balance);
			AssertEquals("Transfer due date", ZDateTime.Empty, TransactionWrapper.TransactionDueDate);

			transfer.AH_PostDate = ZDateTime.Today;
			transfer.AH_InvoiceDate = ZDateTime.Today;
			transfer.AH_OSTotal = 400M;

			AssertEquals("Transfer due date", ZDateTime.Empty, TransactionWrapper.TransactionDueDate);

			AssertEquals("Pre-condition: RegistryItem is False by default", false, AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.Value);
			AssertEquals("Transfer amount", 400M, TransactionWrapper.InvoiceAmountWithGST);

			AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Transfer amount", 400M, TransactionWrapper.InvoiceAmountWithGST);
		}

		#endregion

		#region Remittance Advice And Payment Voucher Fields

		public void TestMatchLinks()
		{
			AssertEquals(0, TransactionWrapper.MatchedLines.Count);
			SetInvoiceSettings(Invoice, ZArchitecture.Core.TransactionTypes.Invoice);
			SetTransactionHeaderSettings(Invoice, -2000M);
			AccTransactionMatchLink matchLink = ((IMatching)Invoice).CurrentMatchGroup.AddNew();
			matchLink.AP_AH = Invoice.PK;
			matchLink.AP_Amount = -1000M;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "NewGroup";

			AccTransactionHeader header1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			header1.AH_InvoiceAmount = -matchLink.AP_Amount;

			AccTransactionMatchLink linkToMatch = ((IMatching)Invoice).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = header1.PK;
			linkToMatch.AP_Amount = -matchLink.AP_Amount;
			linkToMatch.AP_MatchGroupNum = "NewGroup";

			Invoice.AH_OutstandingAmount = 0M;
			Invoice.AH_FullyPaidDate = ZDateTime.Now;

			AccTransactionMatchLink matchLink2 = ((IMatching)Invoice).CurrentMatchGroup.AddNew();
			matchLink2.AP_AH = Invoice.PK;
			matchLink2.AP_Amount = -1000M;
			matchLink2.AP_MatchDate = ZDateTime.Today;
			matchLink2.AP_MatchGroupNum = "NewGroup2";

			AccTransactionHeader header2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			header2.AH_InvoiceAmount = -matchLink.AP_Amount;

			AccTransactionMatchLink linkToMatch2 = ((IMatching)Invoice).CurrentMatchGroup.AddNew();
			linkToMatch2.AP_AH = header2.PK;
			linkToMatch2.AP_Amount = -matchLink.AP_Amount;
			linkToMatch2.AP_MatchGroupNum = "NewGroup2";

			TestObjectCreator.SetupMatchLinkMatchDate(Invoice);

			Factory.Save();
			AssertEquals(2000M, TransactionWrapper.MatchLinks.TotalAmount);
		}

		public void TestReceiptMatchesSortedOnDateAndTransactionNum()
		{
			APInvoice invoice1 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "INV 1");
			APInvoice invoice2 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "1234");
			APInvoice invoice3 = CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice, "ABC4");

			CreateMatchLink(Invoice, 300M, "GROUP1");
			CreateMatchLink(((IMatching)Invoice).CurrentMatchGroup, invoice1, -200M, "GROUP1");
			CreateMatchLink(((IMatching)Invoice).CurrentMatchGroup, invoice2, -20M, "GROUP1");
			CreateMatchLink(((IMatching)Invoice).CurrentMatchGroup, invoice3, -80M, "GROUP1");

			SetTransactionHeaderSettings(invoice1, 200M);
			invoice1.AH_FullyPaidDate = ZDateTime.Now;
			invoice1.AH_OutstandingAmount = 0M;
			SetTransactionHeaderSettings(invoice2, 20M);
			invoice2.AH_FullyPaidDate = ZDateTime.Now;
			invoice2.AH_OutstandingAmount = 0M;
			SetTransactionHeaderSettings(invoice3, 80M);
			invoice3.AH_FullyPaidDate = ZDateTime.Now;
			invoice3.AH_OutstandingAmount = 0M;
			SetTransactionHeaderSettings(Invoice, 300M);
			Invoice.AH_FullyPaidDate = ZDateTime.Now;
			Invoice.AH_OutstandingAmount = 0M;

			Invoice.AH_TransactionNum = "XXX1";
			Invoice.IsManuallySetTransactionNumber_ForTestOnly = true;

			Factory.Save();
			DocTransactionHeaderCollection paymentColl = TransactionWrapper.ReceiptMatches;

			AssertEquals(4, paymentColl.Count);
			AssertEquals("First item in the Collection should be invoice no 1234", "1234", paymentColl[0].TransactionNumber);
			AssertEquals("Second item in the Collection should be invoice no ABC4", "ABC4", paymentColl[1].TransactionNumber);
			AssertEquals("Third item in the Collection should be invoice no INV 1", "INV 1", paymentColl[2].TransactionNumber);
			AssertEquals("Fourth item in the Collection should be invoice no XXX1", "XXX1", paymentColl[3].TransactionNumber);

			invoice1.AH_InvoiceDate = ZDateTime.Today.AddDays(1);
			invoice2.AH_InvoiceDate = ZDateTime.Today.AddDays(1);
			Factory.Save();
			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);

			paymentColl = TransactionWrapper.ReceiptMatches;
			AssertEquals("First item in the Collection should be invoice no ABC4", "ABC4", paymentColl[0].TransactionNumber);
			AssertEquals("Second item in the Collection should be invoice no XXX1", "XXX1", paymentColl[1].TransactionNumber);
			AssertEquals("Third item in the Collection should be invoice no 1234", "1234", paymentColl[2].TransactionNumber);
			AssertEquals("Fourth item in the Collection should be invoice no INV 1", "INV 1", paymentColl[3].TransactionNumber);
		}

		APInvoice CreateInvoice(ZString type, ZString transactionNumber)
		{
			var invoice = Factory.New<APInvoice>();
			invoice.AH_TransactionNum = transactionNumber;
			invoice.AH_TransactionType = type;
			return invoice;
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

		public void TestReceiptTypeDescription()
		{
			Invoice.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertEquals("Cash", TransactionWrapper.ReceiptTypeDescription);

			Invoice.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertEquals("Cheque", TransactionWrapper.ReceiptTypeDescription);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Invoice.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
				AssertEquals("Check", TransactionWrapper.ReceiptTypeDescription);
			}

			Invoice.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			AssertEquals("Credit Card", TransactionWrapper.ReceiptTypeDescription);

			Invoice.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			AssertEquals("Direct Credit", TransactionWrapper.ReceiptTypeDescription);

			Invoice.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			AssertEquals("Direct Debit", TransactionWrapper.ReceiptTypeDescription);
		}

		public void TestNumberOfTransactionLines()
		{
			AssertEquals("Default number of transaction lines", 24, TransactionWrapper.NumberOfTransactionLines);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfTransactionLines, 20);
			TransactionWrapper.SetTemplateConstants(constants);
			AssertEquals("Set number of transaction lines", 20, TransactionWrapper.NumberOfTransactionLines);
		}

		#endregion

		#region Transaction Fields

		public void TestReferenceNumber()
		{
			Invoice.AH_TransactionNum = "01";
			Invoice.AH_ConsolidatedInvoiceRef = "S01";

			DocTransactionHeader transactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			Assert("Reference Number", transactionWrapper.ReferenceNumber == Invoice.AH_ConsolidatedInvoiceRef || transactionWrapper.ReferenceNumber == Invoice.AH_TransactionNum);
		}

		public void TestJobNumber()
		{
			Invoice.AH_TransactionNum = "01";
			DocTransactionHeader transactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Job Number", "01", transactionWrapper.JobNumber);

			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_JobNum = "S00000001";
			Invoice.AH_JH = job.PK;
			transactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Job Number", "S00000001", transactionWrapper.JobNumber);
		}

		public void TestStatementDescriptionForConsolInvoice()
		{
			Invoice.Delete();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C01";
			consol.JK_MasterBillNum = "Master bill";
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);

			Invoice.AH_Desc = "Description";
			Invoice.AH_TransactionNum = "01";
			AssertEquals("Description", Invoice.AH_Desc, TransactionWrapper.StatementDescription);

			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;

			if (AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.Value)
			{
				AssertEquals("Consol number", "MASTER: " + consol.JK_MasterBillNum + " PAYMENT REF: " + Invoice.AH_TransactionNum, TransactionWrapper.StatementDescription);
			}
			else
			{
				AssertEquals("Consol number", "MASTER: " + consol.JK_MasterBillNum + " JOB: " + consol.JK_UniqueConsignRef, TransactionWrapper.StatementDescription);
			}
		}

		public void TestStatementDescription_TransactionReference()
		{
			Invoice.Delete();
			Invoice = Factory.New<ARInvoice>();
			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			Invoice.AH_GSTAmount = 10m;
			Invoice.AH_Desc = "Description";
			Invoice.AH_TransactionNum = "01";
			Invoice.AH_ComplianceSubType = "TXI";
			Invoice.AH_TransactionReference = "123456";

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Indonesia);
			AssertEquals("Description Faktur Pajak 123456", TransactionWrapper.StatementDescription);

			Invoice.AH_ComplianceSubType = "";
			AssertEquals("Description", TransactionWrapper.StatementDescription);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);
			Invoice.AH_ComplianceSubType = "TXI";
			AssertEquals("Description FACTURA 123456", TransactionWrapper.StatementDescription);

			Invoice.AH_ComplianceSubType = "";
			AssertEquals("Description", TransactionWrapper.StatementDescription);
		}

		public void TestDescriptionForShipmentInvoiceUsingJobBasedInvoiceNumbers()
		{
			AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Invoice.Delete();
			ForwardingShipment shipment = GetShipment("S01");

			SetInvoice(shipment);

			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Description", Invoice.AH_Desc, TransactionWrapper.StatementDescription);

			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			AssertEquals("Shipment number", "HOUSE: " + shipment.JS_HouseBill + " PAYMENT REF: " + Invoice.AH_TransactionNum, TransactionWrapper.StatementDescription);
		}

		public void TestDescriptionForShipmentInvoice()
		{
			AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			Invoice.Delete();
			ForwardingShipment shipment = GetShipment("S01");

			SetInvoice(shipment);

			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Description", Invoice.AH_Desc, TransactionWrapper.StatementDescription);

			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			AssertEquals("Shipment number", "HOUSE: " + shipment.JS_HouseBill + " JOB: " + shipment.JS_UniqueConsignRef, TransactionWrapper.StatementDescription);
		}

		void SetInvoice(IJobInvoicingPlugIn plugIn)
		{
			Invoice = Factory.New<ARInvoice>();
			GetInvoiceJob(plugIn, Invoice);
			Invoice.AH_Desc = "Description";
			Invoice.AH_TransactionNum = "02";
		}

		ForwardingShipment GetShipment(string consignmentRef)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = consignmentRef;
			shipment.JS_HouseBill = "House bill";
			Factory.Save();
			return shipment;
		}

		public void TestDescriptionForShipmentInvoiceWithCustomisedShipmentNo()
		{
			AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			Invoice.Delete();
			ForwardingShipment shipment = GetShipment("C01");

			SetInvoice(shipment);

			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Description", Invoice.AH_Desc, TransactionWrapper.StatementDescription);

			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			AssertEquals("Shipment number", "HOUSE: " + shipment.JS_HouseBill + " JOB: " + shipment.JS_UniqueConsignRef, TransactionWrapper.StatementDescription);
		}

		public void TestDescriptionForShipmentInvoiceWithCustomisedShipmentNo_UsingJobBasedNumber()
		{
			AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			Invoice.Delete();
			ForwardingShipment shipment = GetShipment("C01");

			SetInvoice(shipment);

			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Description", Invoice.AH_Desc, TransactionWrapper.StatementDescription);

			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			AssertEquals("Shipment number", "HOUSE: " + shipment.JS_HouseBill + " PAYMENT REF: " + Invoice.AH_TransactionNum, TransactionWrapper.StatementDescription);
		}

		#region Shipping

		AgencyShipment GetShipping(string consignmentRef)
		{
			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();
			shipment.JS_UniqueConsignRef = consignmentRef;
			shipment.JS_HouseBill = "House bill";
			Factory.Save();
			return shipment;
		}

		public void TestDescriptionForShippingInvoiceUsingJobBasedInvoiceNumbers()
		{
			AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Invoice.Delete();
			AgencyShipment shipment = GetShipping("S01");

			SetInvoice(shipment);

			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Description", Invoice.AH_Desc, TransactionWrapper.StatementDescription);

			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			AssertEquals("Shipment number", "BILL No: " + shipment.JS_HouseBill + " PAYMENT REF: " + Invoice.AH_TransactionNum, TransactionWrapper.StatementDescription);
		}

		public void TestDescriptionForShippingInvoice()
		{
			AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			Invoice.Delete();
			AgencyShipment shipment = GetShipping("S01");

			SetInvoice(shipment);

			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Description", Invoice.AH_Desc, TransactionWrapper.StatementDescription);

			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			AssertEquals("Shipment number", "BILL No: " + shipment.JS_HouseBill + " JOB: " + shipment.JS_UniqueConsignRef, TransactionWrapper.StatementDescription);
		}

		public void TestTaxBranch()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "ASI";

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_Code = "TB2";

			Invoice.AH_GB = branch1.PK;
			Invoice.AH_GB_TaxBranch = ZGuid.Empty;
			AssertNull("Pre condition:Invoice TaxBranch is null", Invoice.TaxBranch);

			AssertEquals("DocWrapper TaxBranch ", "ASI", TransactionWrapper.TaxBranch.Code);

			Invoice.AH_GB_TaxBranch = branch2.PK;
			AssertNotNull("Pre condition:Invoice TaxBranch is not null", Invoice.TaxBranch);
			AssertEquals("DocWrapper TaxBranch", "TB2", TransactionWrapper.TaxBranch.Code);
		}

		public void TestDescriptionForShippingInvoiceWithCustomisedShipmentNo()
		{
			AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			Invoice.Delete();
			AgencyShipment shipment = GetShipping("C01");

			SetInvoice(shipment);

			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Description", Invoice.AH_Desc, TransactionWrapper.StatementDescription);

			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			AssertEquals("Shipment number", "BILL No: " + shipment.JS_HouseBill + " JOB: " + shipment.JS_UniqueConsignRef, TransactionWrapper.StatementDescription);
		}

		public void TestDescriptionForShippingInvoiceWithCustomisedShipmentNo_UsingJobBasedNumber()
		{
			AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			Invoice.Delete();
			AgencyShipment shipment = GetShipping("C01");

			SetInvoice(shipment);

			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Description", Invoice.AH_Desc, TransactionWrapper.StatementDescription);

			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			AssertEquals("Shipment number", "BILL No: " + shipment.JS_HouseBill + " PAYMENT REF: " + Invoice.AH_TransactionNum, TransactionWrapper.StatementDescription);
		}

		#endregion

		public void TestDescriptionForBrokerageInvoice()
		{
			AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			Invoice.Delete();
			BaseJobDeclaration declaration = GetDeclaration("B01");

			SetInvoice(declaration);

			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Description", Invoice.AH_Desc, TransactionWrapper.StatementDescription);

			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			AssertEquals("Brokerage number", "HOUSE: " + declaration.JE_HouseBill + " JOB: " + declaration.JE_DeclarationReference, TransactionWrapper.StatementDescription);
		}

		public void TestDescriptionForBrokerageInvoiceUsingJobBasedNumber()
		{
			AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			Invoice.Delete();
			BaseJobDeclaration declaration = GetDeclaration("B01");

			SetInvoice(declaration);

			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Description", Invoice.AH_Desc, TransactionWrapper.StatementDescription);

			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			AssertEquals("Brokerage number", "HOUSE: " + declaration.JE_HouseBill + " PAYMENT REF: " + Invoice.AH_TransactionNum, TransactionWrapper.StatementDescription);
		}

		public void TestDescriptionForBrokerageInvoiceWithCustomisedDeclarationNo_JobBasedInvNo()
		{
			AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			Invoice.Delete();
			BaseJobDeclaration declaration = GetDeclaration("S01");

			SetInvoice(declaration);

			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Description", Invoice.AH_Desc, TransactionWrapper.StatementDescription);

			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			AssertEquals("Brokerage number", "HOUSE: " + declaration.JE_HouseBill + " PAYMENT REF: " + Invoice.AH_TransactionNum, TransactionWrapper.StatementDescription);
		}

		public void TestDescriptionForBrokerageInvoiceWithCustomisedDeclarationNo()
		{
			AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			Invoice.Delete();
			BaseJobDeclaration declaration = GetDeclaration("S01");

			SetInvoice(declaration);

			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Description", Invoice.AH_Desc, TransactionWrapper.StatementDescription);

			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			AssertEquals("Brokerage number", "HOUSE: " + declaration.JE_HouseBill + " JOB: " + declaration.JE_DeclarationReference, TransactionWrapper.StatementDescription);
		}

		BaseJobDeclaration GetDeclaration(string declarationRef)
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = declarationRef;
			declaration.JE_HouseBill = "House bill";
			Factory.Save();
			return declaration;
		}

		JobHeader GetInvoiceJob(IJobInvoicingPlugIn plugIn, InvoicingBase invoice)
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(plugIn.TableName);
			job.JH_ParentID = plugIn.PK;
			invoice.AH_JH = job.PK;
			return job;
		}

		public void TestBalanceForStatement()
		{
			// EndOfStatementPeriodForCalculatingMatchedAmount is 12:00:00 AM of the next day after the end of period
			// Use it as a base for other dates
			ZDateTime now = ZDateTime.Today.AddDays(1);

			Invoice.AH_InvoiceAmount = 100M;
			Invoice.AH_GSTAmount = 10M;
			Invoice.AH_OSTotal = 110M;
			Invoice.AH_OutstandingAmount = 110M;

			AssertHeaderValuesForStatement(TransactionWrapper, ZDateTime.Empty, 0M, 110M);
			AssertHeaderValuesForStatement(TransactionWrapper, now.AddDays(-10), 0M, 110M);

			AccTransactionMatchLink matchLink1 = CreateMatchLink(Invoice, 50M, now.AddDays(-30));
			AccTransactionMatchLink matchLink2 = CreateMatchLink(Invoice, 50M, now.AddDays(-20));
			AccTransactionMatchLink matchLink3 = CreateMatchLink(Invoice, 10M, now.AddDays(-10));

			AssertHeaderValuesForStatement(TransactionWrapper, ZDateTime.Empty, 110M, 0M);
			AssertHeaderValuesForStatement(TransactionWrapper, now.AddDays(10), 110M, 0M);
			AssertHeaderValuesForStatement(TransactionWrapper, now, 110M, 0M);
			AssertHeaderValuesForStatement(TransactionWrapper, now.AddDays(-10), 100M, 10M);
			AssertHeaderValuesForStatement(TransactionWrapper, now.AddDays(-20), 50M, 60M);
			AssertHeaderValuesForStatement(TransactionWrapper, now.AddDays(-30), 0M, 110M);

			// Simulate paying by web service without creating MatchLinks
			matchLink1.Delete();
			matchLink2.Delete();
			matchLink3.Delete();

			AssertHeaderValuesForStatement(TransactionWrapper, now.AddDays(10), 0M, 0M);
			AssertHeaderValuesForStatement(TransactionWrapper, now, 0M, 0M);
			// Should be calculated using MatchLinks but they are missing. That is why it is fully outstanding
			AssertHeaderValuesForStatement(TransactionWrapper, now.AddDays(-10), 0M, 110M);
		}

		public void TestBalanceForStatementCountOnOtherTaxAmounts()
		{
			// EndOfStatementPeriodForCalculatingMatchedAmount is 12:00:00 AM of the next day after the end of period
			// Use it as a base for other dates
			ZDateTime now = ZDateTime.Today.AddDays(1);

			Invoice.AH_InvoiceAmount = 100M;
			Invoice.AH_GSTAmount = 8M;
			Invoice.AH_LocalTaxAmountOtherTaxes = 2M;
			Invoice.AH_OSTotal = 110M;
			Invoice.AH_OutstandingAmount = 110M;

			AssertHeaderValuesForStatement(TransactionWrapper, ZDateTime.Empty, 0M, 110M);
			AssertHeaderValuesForStatement(TransactionWrapper, now.AddDays(-10), 0M, 110M);

			AccTransactionMatchLink matchLink1 = CreateMatchLink(Invoice, 50M, now.AddDays(-30));
			AccTransactionMatchLink matchLink2 = CreateMatchLink(Invoice, 50M, now.AddDays(-20));
			AccTransactionMatchLink matchLink3 = CreateMatchLink(Invoice, 10M, now.AddDays(-10));

			AssertHeaderValuesForStatement(TransactionWrapper, ZDateTime.Empty, 110M, 0M);
			AssertHeaderValuesForStatement(TransactionWrapper, now.AddDays(10), 110M, 0M);
			AssertHeaderValuesForStatement(TransactionWrapper, now, 110M, 0M);
			AssertHeaderValuesForStatement(TransactionWrapper, now.AddDays(-10), 100M, 10M);
			AssertHeaderValuesForStatement(TransactionWrapper, now.AddDays(-20), 50M, 60M);
			AssertHeaderValuesForStatement(TransactionWrapper, now.AddDays(-30), 0M, 110M);

			// Simulate paying by web service without creating MatchLinks
			matchLink1.Delete();
			matchLink2.Delete();
			matchLink3.Delete();

			AssertHeaderValuesForStatement(TransactionWrapper, now.AddDays(10), 0M, 0M);
			AssertHeaderValuesForStatement(TransactionWrapper, now, 0M, 0M);
			// Should be calculated using MatchLinks but they are missing. That is why it is fully outstanding
			AssertHeaderValuesForStatement(TransactionWrapper, now.AddDays(-10), 0M, 110M);
		}

		public void TestBalanceForStatement_ForNewOSOutstandingAmountFeature()
		{
			var arInvoice_OSOutstandingAmountApplicable = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.IDR, 14000m, 15000000m, 0m, 1071.43m, 0m) as ARInvoice;
			var arInvoice_OSOutstandingAmountNotApplicable = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "003", TestObjectCreator.IDR, 14000m, 15000000m, 0m, 1071.43m, 0m) as ARInvoice;

			arInvoice_OSOutstandingAmountApplicable.Lines[0].AL_OSExTaxAmount = 15000000m;
			arInvoice_OSOutstandingAmountNotApplicable.Lines[0].AL_OSExTaxAmount = 15000000m;

			TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(arInvoice_OSOutstandingAmountApplicable);
			Factory.Save();

			//Match: os amount 3000000m, local amount 214.29m
			TestObjectCreator.CreateAndMatchARReceiptForARInvoiceForNewOSOutstandingAmountFeature(arInvoice_OSOutstandingAmountApplicable, 1, 3000000m, "M001");
			TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoice_OSOutstandingAmountNotApplicable, ZDateTime.Today.AddDays(1), 3000000m, "M002");
			Factory.Save();

			//Note
			//11999944 = OS Total - highPrecisionExchangeRate * Matched Amount
			//		= 15000000 - (15000000 / 1071.43) * (1071.43 - 857.14)
			//		= 15000000 - (15000000 / 1071.43) * 214.29
			//		= 15000000 - 3000056
			//12000000 = AH_OSOutstandingAmount
			//		= 15000000 - 3000000

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			{
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals(12000000m, DocTransactionHeader.New(arInvoice_OSOutstandingAmountApplicable, Factory).Balance);
					AssertEquals(11999944m, DocTransactionHeader.New(arInvoice_OSOutstandingAmountNotApplicable, Factory).Balance);
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals(11999944m, DocTransactionHeader.New(arInvoice_OSOutstandingAmountApplicable, Factory).Balance);
					AssertEquals(11999944m, DocTransactionHeader.New(arInvoice_OSOutstandingAmountNotApplicable, Factory).Balance);
				}
			}
		}

		[TestDate(2023, 2, 6)]
		public void TestBalanceForStatement_IsPreviousPeriod_ForNewOSOutstandingAmountFeature()
		{
			var arInvoice_OSOutstandingAmountApplicable = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.IDR, 14000m, 29000000m, 0m, 2071.43m, 0m) as ARInvoice;
			var arInvoice_OSOutstandingAmountNotApplicable = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "003", TestObjectCreator.IDR, 14000m, 29000000m, 0m, 2071.43m, 0m) as ARInvoice;

			arInvoice_OSOutstandingAmountApplicable.Lines[0].AL_OSExTaxAmount = 29000000m;
			arInvoice_OSOutstandingAmountNotApplicable.Lines[0].AL_OSExTaxAmount = 29000000m;

			TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(arInvoice_OSOutstandingAmountApplicable);
			Factory.Save();

			//1st Match (yestaday): os amount 14000000m, local amount 1000m
			TestObjectCreator.CreateAndMatchARReceiptForARInvoiceForNewOSOutstandingAmountFeature(arInvoice_OSOutstandingAmountApplicable, -1, 14000000m, "M001");
			TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoice_OSOutstandingAmountNotApplicable, ZDateTime.Today.AddDays(-1), 14000000m, "M002");
			Factory.Save();

			//2nd Match (today): os amount 1000000m, local amount 71.43m
			TestObjectCreator.CreateAndMatchARReceiptForARInvoiceForNewOSOutstandingAmountFeature(arInvoice_OSOutstandingAmountApplicable, 0, 1000000m, "M003");
			TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoice_OSOutstandingAmountNotApplicable, ZDateTime.Today, 1000000m, "M004");
			Factory.Save();

			//Note
			//15000010m = OS Total - highPrecisionExchangeRate * Matched Amount before toady
			//		= 29000000m - (29000000m / 2071.43m) * 1000M
			//		= 29000000m - 13999990
			//15000000m = OS Total - SUM(AP_OSAmount)
			//		= 29000000m - 14000000m

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			{
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var wapper1 = DocTransactionHeader.New(arInvoice_OSOutstandingAmountApplicable, Factory);
					var wapper2 = DocTransactionHeader.New(arInvoice_OSOutstandingAmountNotApplicable, Factory);
					wapper1.EndOfStatementPeriodForCalculatingMatchedAmount = wapper2.EndOfStatementPeriodForCalculatingMatchedAmount = ZDateTime.Today;

					AssertEquals(15000000m, wapper1.Balance);
					AssertEquals(15000010m, wapper2.Balance);
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var wapper1 = DocTransactionHeader.New(arInvoice_OSOutstandingAmountApplicable, Factory);
					var wapper2 = DocTransactionHeader.New(arInvoice_OSOutstandingAmountNotApplicable, Factory);
					wapper1.EndOfStatementPeriodForCalculatingMatchedAmount = wapper2.EndOfStatementPeriodForCalculatingMatchedAmount = ZDateTime.Today;

					AssertEquals(15000010m, wapper1.Balance);
					AssertEquals(15000010m, wapper2.Balance);
				}
			}
		}

		public void AssertHeaderValuesForStatement(DocTransactionHeader transactionWrapper, ZDateTime statementEndDate,
			ZDecimal calculatedMatchedAmountForStatement, ZDecimal balance)
		{
			transactionWrapper.EndOfStatementPeriodForCalculatingMatchedAmount = statementEndDate;

			AssertEquals("End of Statement Period For Calculating Matched Amount", statementEndDate,
				transactionWrapper.EndOfStatementPeriodForCalculatingMatchedAmount);

			AssertEquals("Calculated Match Amount", calculatedMatchedAmountForStatement,
				transactionWrapper.CalculatedMatchedAmountForStatement);

			AssertEquals("Balance", balance, transactionWrapper.Balance);
		}

		AccTransactionMatchLink CreateMatchLink(AccTransactionHeader invoice, ZDecimal amount, ZDateTime matchDate)
		{
			AccTransactionMatchLink matchLink = Factory.New<AccTransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = amount;
			matchLink.AP_MatchDate = matchDate;
			invoice.AH_OutstandingAmount -= amount;
			return matchLink;
		}

		public void TestStatementPortsForConsolInvoice()
		{
			Invoice.Delete();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C01";

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			Invoice = Factory.New<ARInvoice>();
			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;

			AssertEquals("AUSYD", TransactionWrapper.StatementPortOfLoading.ToString().Trim());
			AssertEquals("NZAKL", TransactionWrapper.StatementPortOfDischarge.ToString());
		}

		public void TestStatementPortsForConsolInvoiceWithEmptyPorts()
		{
			Invoice.Delete();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C01";

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = ZString.Empty;
			transport.JW_RL_NKDiscPort = ZString.Empty;

			Invoice = Factory.New<ARInvoice>();
			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			Invoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;

			AssertEquals(ZString.Empty, TransactionWrapper.StatementPortOfLoading.ToString().Trim());
			AssertEquals(ZString.Empty, TransactionWrapper.StatementPortOfDischarge.ToString());
		}

		public void TestStatementPortsForShipmentInvoice()
		{
			Invoice.Delete();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S01";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			GetInvoiceJob(shipment, Invoice);

			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);

			Invoice.AH_Desc = "Description";
			Invoice.AH_TransactionNum = "02";
			AssertEquals("Description", Invoice.AH_Desc, TransactionWrapper.StatementDescription);

			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;

			AssertEquals("AUSYD", TransactionWrapper.StatementPortOfLoading.ToString());
			AssertEquals("NZAKL", TransactionWrapper.StatementPortOfDischarge.ToString());
		}

		public void TestStatementPortsForShipmentInvoiceWithEmptyPorts()
		{
			Invoice.Delete();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S01";
			shipment.JS_RL_NKOrigin = ZString.Empty;
			shipment.JS_RL_NKDestination = ZString.Empty;
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);

			Invoice.AH_Desc = "Description";
			Invoice.AH_TransactionNum = "02";
			AssertEquals("Description", Invoice.AH_Desc, TransactionWrapper.StatementDescription);

			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;

			AssertEquals(ZString.Empty, TransactionWrapper.StatementPortOfLoading.ToString());
			AssertEquals(ZString.Empty, TransactionWrapper.StatementPortOfDischarge.ToString());
		}

		public void TestStatementPortsForBrokerageInvoice()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			Invoice.Delete();
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = "B01";
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "NZAKL";
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			GetInvoiceJob(declaration, Invoice);

			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);

			Invoice.AH_Desc = "Description";
			Invoice.AH_TransactionNum = "01";
			AssertEquals("Description", Invoice.AH_Desc, TransactionWrapper.StatementDescription);

			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;

			AssertEquals("AUSYD", TransactionWrapper.StatementPortOfLoading.ToString());
			AssertEquals("NZAKL", TransactionWrapper.StatementPortOfDischarge.ToString());
		}

		public void TestStatementPortsForBrokerageInvoiceWithEmptyPorts()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			Invoice.Delete();
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = "B01";
			declaration.JE_RL_NKOrigin = ZString.Empty;
			declaration.JE_RL_NKFinalDestination = ZString.Empty;
			Factory.Save();

			Invoice = Factory.New<ARInvoice>();
			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);

			Invoice.AH_Desc = "Description";
			Invoice.AH_TransactionNum = "01";
			AssertEquals("Description", Invoice.AH_Desc, TransactionWrapper.StatementDescription);

			Invoice.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;

			AssertEquals(ZString.Empty, TransactionWrapper.StatementPortOfLoading.ToString());
			AssertEquals(ZString.Empty, TransactionWrapper.StatementPortOfDischarge.ToString());
		}

		public void TestDisbursement()
		{
			AssertEquals("No disbursement text", ZString.Empty, TransactionWrapper.Disbursement);

			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			AssertEquals("Disbursement test", " (Disbursement)", TransactionWrapper.Disbursement);
		}

		public void TestCreator()
		{
			AssertNull("Creator", TransactionWrapper.Creator);
			Factory.Save();
			AssertNotNull("Creator", TransactionWrapper.Creator);
			AssertEquals("Creator full name", GlbStaff.CurrentUser.GS_FullName, TransactionWrapper.Creator.FullName);

			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "ZZ"));
			AssertNotEquals(GlbStaff.CurrentUser.GS_FullName, staff.GS_FullName);

			Invoice.AH_SystemCreateUser = staff.GS_Code;
			Factory.Save();
			AssertNotNull("Creator", TransactionWrapper.Creator);
			AssertEquals("Creator full name", staff.GS_FullName, TransactionWrapper.Creator.FullName);
		}

		public void TestCreatingUser()
		{
			AssertNotNull("Creating user", TransactionWrapper.CreatingUser);
		}

		public void TestChequeDrawer()
		{
			ZString chequeDrawer = new ZString("ChequeDrawer");
			Invoice.AH_ChequeDrawer = chequeDrawer;
			AssertEquals("ChequeDrawer", chequeDrawer, TransactionWrapper.ChequeDrawer);
		}

		public void TestChequeOrReference()
		{
			ZString chequeOrReference = new ZString("ChequeOrReference");
			Invoice.AH_ChequeOrReference = chequeOrReference;
			AssertEquals("ChequeOrReference", chequeOrReference, TransactionWrapper.ChequeOrReference);
			AssertEquals("ChequeNumber", chequeOrReference, TransactionWrapper.ChequeNumber);
		}

		public void TestConsolidatedInvoiceRef()
		{
			ZString consolidatedInvoiceRef = new ZString("ConsolidatedInvoiceRef");
			Invoice.AH_ConsolidatedInvoiceRef = consolidatedInvoiceRef;
			AssertEquals("ConsolidatedInvoiceRef", consolidatedInvoiceRef, TransactionWrapper.ConsolidatedInvoiceRef);
		}

		public void TestDesc()
		{
			ZString desc = new ZString("Desc");
			Invoice.AH_Desc = desc;
			AssertEquals("Desc", desc, TransactionWrapper.Desc);
		}

		public void TestDrawerBank()
		{
			ZString drawerBank = new ZString("DrawerBank");
			Invoice.AH_DrawerBank = drawerBank;
			AssertEquals("DrawerBank", drawerBank, TransactionWrapper.DrawerBank);
		}

		public void TestDrawerBranch()
		{
			ZString drawerBranch = new ZString("DrawerBranch");
			Invoice.AH_DrawerBranch = drawerBranch;
			AssertEquals("DrawerBranch", drawerBranch, TransactionWrapper.DrawerBranch);
		}

		public void TestInvoiceTerm()
		{
			ZString invoiceTerm = new ZString("COD");
			Invoice.AH_InvoiceTerm = invoiceTerm;
			AssertEquals("InvoiceTerm", invoiceTerm, TransactionWrapper.InvoiceTerm);
		}

		public void TestLedger()
		{
			ZString ledger = new ZString("AR");
			Invoice.AH_Ledger = ledger;
			AssertEquals("Ledger", ledger, TransactionWrapper.Ledger);
		}

		public void TestReceiptBatchNo()
		{
			ZString receiptBatchNo = new ZString("ReceiptBatchNo");
			Invoice.AH_ReceiptBatchNo = receiptBatchNo;
			AssertEquals("ReceiptBatchNo", receiptBatchNo, TransactionWrapper.ReceiptBatchNo);
		}

		public void TestReceiptType()
		{
			ZString receiptType = new ZString("REC");
			Invoice.AH_ReceiptType = receiptType;
			AssertEquals("ReceiptType", receiptType, TransactionWrapper.ReceiptType);
		}

		public void TestTransactionNum()
		{
			ZString transactionNum = new ZString("TransactionNum");
			Invoice.AH_TransactionNum = transactionNum;
			AssertEquals("TransactionNum", transactionNum, TransactionWrapper.TransactionNum);
		}

		public void TestTransactionReference()
		{
			ZString transactionReference = new ZString("TransactionReference");
			Invoice.AH_TransactionReference = transactionReference;
			AssertEquals("TransactionReference", transactionReference, TransactionWrapper.TransactionReference);
		}

		public void TestOurReference()
		{
			AssertEquals("Our Reference should be empty", "", TransactionWrapper.OurReference);
		}

		public void TestYourReference()
		{
			AssertEquals("Our Reference should be empty", "", TransactionWrapper.YourReference);
		}

		public void TestTransactionType()
		{
			ZString transactionType = new ZString("TRA");
			Invoice.AH_TransactionType = transactionType;
			AssertEquals("TransactionType", transactionType, TransactionWrapper.TransactionType);
		}

		public void TestComplianceNumber()
		{
			Invoice.AH_TransactionReference = "abc12345";
			AssertEquals("ComplianceNumber", "abc12345", TransactionWrapper.ComplianceNumber);
		}

		public void TestComplianceSubType()
		{
			ZString complianceSubType = new ZString("TXI");
			Invoice.AH_ComplianceSubType = complianceSubType;
			AssertEquals("ComplianceSubType", complianceSubType, TransactionWrapper.ComplianceSubType);
		}

		public void TestComplianceSubTypeLocalDescription()
		{
			ZString complianceSubType = new ZString("TXI");
			Invoice.AH_ComplianceSubType = complianceSubType;

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);
			AssertEquals("ComplianceSubType", "FACTURA", TransactionWrapper.ComplianceSubTypeLocalDescription);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Indonesia);
			AssertEquals("ComplianceSubType", "Faktur Pajak", TransactionWrapper.ComplianceSubTypeLocalDescription);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			AssertEquals("ComplianceSubType", "", TransactionWrapper.ComplianceSubTypeLocalDescription);
		}

		public void TestComplianceNumberPrefixOnly()
		{
			AccComplianceSequence complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_SequenceClass = "TXI";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			Factory.Save();

			Invoice.AH_GC = GlbCompany.CurrentCompany.PK;
			Invoice.AH_ComplianceSubType = "TXI";
			Invoice.AH_TransactionReference = "abc-0010001";
			AssertEquals("ComplianceNumberPrefixOnly", "abc-001", TransactionWrapper.ComplianceNumberPrefixOnly);

			Invoice.AH_TransactionReference = "abcdefg";
			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("ComplianceNumberPrefixOnly will be blank as couldn't find a compliance book", "", TransactionWrapper.ComplianceNumberPrefixOnly);
		}

		public void TestComplianceNumberWithoutPrefix()
		{
			AccComplianceSequence complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_SequenceClass = "TXI";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			Factory.Save();

			Invoice.AH_GC = GlbCompany.CurrentCompany.PK;
			Invoice.AH_ComplianceSubType = "TXI";
			Invoice.AH_TransactionReference = "abc-0010001";
			AssertEquals("ComplianceNumberPrefixOnly", "0001", TransactionWrapper.ComplianceNumberWithoutPrefix);
		}

		public void TestParentTransaction()
		{
			var newInvoice = Factory.New<ARInvoice>();
			Invoice.AH_TransactionBelongsToGroup = newInvoice.PK;
			AssertEquals("ParentTransaction", newInvoice.PK, TransactionWrapper.ParentTransaction.TransactionPK);

			Invoice.AH_TransactionBelongsToGroup = ZGuid.Empty;
			AssertEquals("ParentTransaction", null, TransactionWrapper.ParentTransaction);
		}

		public void TestComplianceBookBranchOrgFallBackToCompanyOrg()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Org for company";
			OrgAddress mainAddress1 = org1.MainAddress;
			mainAddress1.FillWithValidTestData();
			mainAddress1.OA_RL_NKRelatedPortCode = "AUBNE";

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "Org for branch";
			OrgAddress mainAddress2 = org2.MainAddress;
			mainAddress2.FillWithValidTestData();
			mainAddress2.OA_RL_NKRelatedPortCode = "AUSYD";

			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = org1.PK;

			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = org2.PK;

			AccComplianceSequence complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = company.PK;
			complianceSequence.XD_SequenceClass = "TXI";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			complianceSequence.XD_GB_BranchOwner = branch.PK;
			Factory.Save();

			Invoice.AH_GC = company.PK;
			Invoice.AH_ComplianceSubType = "TXI";
			Invoice.AH_TransactionReference = "abc-0010001";

			AssertEquals("Org for branch", TransactionWrapper.ComplianceBookBranchFullName);
			AssertEquals("Sydney", TransactionWrapper.ComplianceBookBranchCity);
			AssertEquals("ORG FOR BRANCH\n#1\nNSW\nAUSTRALIA", TransactionWrapper.ComplianceBookBranchMainAddress);

			branch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertEquals("Org for company", TransactionWrapper.ComplianceBookBranchFullName);
			AssertEquals("Brisbane", TransactionWrapper.ComplianceBookBranchCity);
			AssertEquals("ORG FOR COMPANY\n#1\nQLD\nAUSTRALIA", TransactionWrapper.ComplianceBookBranchMainAddress);
		}

		public void TestComplianceBookPrintingAuthorizationNumber()
		{
			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_SequenceClass = "TXI";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 10;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			complianceSequence.XD_PrintingAuthorizationNumber = "printCode";
			Factory.Save();

			Invoice.AH_GC = GlbCompany.CurrentCompany.PK;
			Invoice.AH_ComplianceSubType = "TXI";
			Invoice.AH_TransactionReference = "abc-0020025";
			Factory.Save();

			AssertEquals(string.Empty, TransactionWrapper.ComplianceBookPrintingAuthorizationNumber);

			Invoice.AH_TransactionReference = "abc-0010025";
			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Sequence Number is in the range", "printCode", TransactionWrapper.ComplianceBookPrintingAuthorizationNumber);

			Invoice.AH_TransactionReference = "abc-0010001";
			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Sequence Number is under the range", string.Empty, TransactionWrapper.ComplianceBookPrintingAuthorizationNumber);

			Invoice.AH_TransactionReference = "abc-0010010";
			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Sequence Number is equals to the start number", "printCode", TransactionWrapper.ComplianceBookPrintingAuthorizationNumber);

			Invoice.AH_TransactionReference = "abc-0010099";
			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Sequence Number is equals to the end number", "printCode", TransactionWrapper.ComplianceBookPrintingAuthorizationNumber);

			Invoice.AH_TransactionReference = "abc-0010100";
			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Sequence Number is above the range", string.Empty, TransactionWrapper.ComplianceBookPrintingAuthorizationNumber);

			Invoice.AH_TransactionReference = "";
			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals("Sequence Number is empty", string.Empty, TransactionWrapper.ComplianceBookPrintingAuthorizationNumber);
		}

		public void TestBookPrintingAuthorizationHeading()
		{
			Invoice.AH_GC = GlbCompany.CurrentCompany.PK;
			Invoice.AH_ComplianceSubType = "TXI";
			Invoice.AH_TransactionReference = "abc-0010001";
			AssertEquals("Authorization Number", TransactionWrapper.BookPrintingAuthorizationHeading);

			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_SequenceClass = "TXI";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			complianceSequence.XD_PrintingAuthorizationNumber = "printCode";
			Factory.Save();

			AssertEquals("Authorization Number", TransactionWrapper.BookPrintingAuthorizationHeading);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Honduras))
			{
				AssertEquals("CAI", TransactionWrapper.BookPrintingAuthorizationHeading);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.CostaRica))
			{
				AssertEquals("RESOLUCION", TransactionWrapper.BookPrintingAuthorizationHeading);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Guatemala))
			{
				AssertEquals("RESOLUCION", TransactionWrapper.BookPrintingAuthorizationHeading);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Panama))
			{
				AssertEquals("DGI", TransactionWrapper.BookPrintingAuthorizationHeading);
			}
		}

		public void TestComplianceBookExpiryDateAndHeading()
		{
			Invoice.AH_GC = GlbCompany.CurrentCompany.PK;
			Invoice.AH_ComplianceSubType = "TXI";
			Invoice.AH_TransactionReference = "abc-0010001";
			AssertEquals(ZDateTime.Empty, TransactionWrapper.ComplianceBookExpiryDate);
			AssertEquals(string.Empty, TransactionWrapper.ComplianceBookExpiryHeading);

			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_SequenceClass = "TXI";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			complianceSequence.XD_PrintingAuthorizationNumber = "printCode";
			Factory.Save();

			AssertEquals(ZDateTime.Empty, TransactionWrapper.ComplianceBookExpiryDate);
			AssertEquals(string.Empty, TransactionWrapper.ComplianceBookExpiryHeading);

			var expiryDate = ZDateTime.Today.AddMonths(1);
			complianceSequence.XD_ExpiryDate = expiryDate;
			AssertEquals(expiryDate, TransactionWrapper.ComplianceBookExpiryDate);
			AssertEquals("Expiry Date", TransactionWrapper.ComplianceBookExpiryHeading);
		}

		public void TestTransactionUniqueNumber()
		{
			Invoice.AuthorizationNumberReference = " ";
			AssertEquals(ZString.Empty, Invoice.AuthorizationNumberReference);
			AssertEquals(ZString.Empty, TransactionWrapper.TransactionUniqueNumber);

			Invoice.AuthorizationNumberReference = ZString.Empty;
			AssertEquals(ZString.Empty, Invoice.AuthorizationNumberReference);
			AssertEquals(ZString.Empty, TransactionWrapper.TransactionUniqueNumber);

			Invoice.AuthorizationNumberReference = "printCode-0010001";
			Invoice.AH_InvoiceDate = new ZDate(2023, 01, 01);
			AssertEquals(ZString.Empty, TransactionWrapper.TransactionUniqueNumber);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Invoice.AH_InvoiceDate = new ZDate(2022, 12, 31);
				AssertEquals(ZString.Empty, TransactionWrapper.TransactionUniqueNumber);

				Invoice.AH_InvoiceDate = new ZDate(2023, 01, 01);
				AssertEquals("ATCUD:printCode-0010001", TransactionWrapper.TransactionUniqueNumber);
			}
		}

		public void TestCreditTermsLabel()
		{
			ZString transactionType = new ZString("TRA");
			Invoice.AH_TransactionType = transactionType;
			AssertEquals("CreditTermsLabel", ZString.Empty, TransactionWrapper.CreditTermsLabel);

			ZString transactionType1 = new ZString("INV");
			Invoice.AH_TransactionType = transactionType1;
			ZString ledger1 = new ZString("AP");
			Invoice.AH_Ledger = ledger1;
			AssertEquals("CreditTermsLabel", ZString.Empty, TransactionWrapper.CreditTermsLabel);

			ZString transactionType2 = new ZString("INV");
			Invoice.AH_TransactionType = transactionType2;
			ZString ledger2 = new ZString("AR");
			Invoice.AH_Ledger = ledger2;
			AssertEquals("CreditTermsLabel", "TERMS", TransactionWrapper.CreditTermsLabel);
		}

		public void TestTransactionCategory()
		{
			ZString transactionCategory = new ZString("CAT");
			Invoice.AH_TransactionCategory = transactionCategory;
			AssertEquals("TransactionCategory", transactionCategory, TransactionWrapper.TransactionCategory);
		}

		public void TestTotalAllocatedAmount()
		{
			ZQuery filter = new ZQuery(RefCurrencySchema.RX_Code, "USD");
			Invoice.AH_RX_NKTransactionCurrency = (Factory.LoadTop1<RefCurrency>(filter)).RX_Code;
			Invoice.AH_OSTotal = 700M;
			Invoice.AH_InvoiceAmount = 800M;
			Invoice.AH_OutstandingAmount = 200M;
			AssertEquals("Total OS Amount", 700M, TransactionWrapper.TotalOSAmount);
			AssertEquals("Total Outstanding amount", 175M, TransactionWrapper.OSOutstandingAmount);
			AssertEquals("Total Allocated amount", 525M, TransactionWrapper.TotalAllocatedAmount);
		}

		public void TestOSOustandingAmount()
		{
			ZQuery filter = new ZQuery(RefCurrencySchema.RX_Code, "USD");
			Invoice.AH_RX_NKTransactionCurrency = (Factory.LoadTop1<RefCurrency>(filter)).RX_Code;
			Invoice.AH_OSTotal = 1000M;
			Invoice.AH_ExchangeRate = 0.7M;
			Invoice.AH_InvoiceAmount = 1428.57M;
			Invoice.AH_OutstandingAmount = 1200M;
			AssertEquals("OS Oustanding amount", 840M, TransactionWrapper.OSOutstandingAmount);
		}

		public void TestOSGstAmount()
		{
			ZQuery filter = new ZQuery(RefCurrencySchema.RX_Code, "USD");
			Invoice.AH_RX_NKTransactionCurrency = (Factory.LoadTop1<RefCurrency>(filter)).RX_Code;
			Invoice.AH_ExchangeRate = 0.7M;
			Invoice.AH_OSTaxAmount = 588.00M;
			AssertEquals("OSGstAmount", 588.0M, TransactionWrapper.OSGstAmount);
		}

		public void TestOSExTaxAmount()
		{
			ZQuery filter = new ZQuery(RefCurrencySchema.RX_Code, "USD");
			Invoice.AH_RX_NKTransactionCurrency = (Factory.LoadTop1<RefCurrency>(filter)).RX_Code;
			Invoice.AH_OSTotal = 1000M;
			Invoice.AH_ExchangeRate = 0.7M;
			Invoice.AH_OSExTaxAmount = 1000.00M;
			Invoice.AH_OutstandingAmount = 1200M;
			AssertEquals("OSExTaxAmount", 1000.0M, TransactionWrapper.OSExTaxAmount);
		}

		public void TestTotalLocalInvoiceAmount()
		{
			ZQuery filter = new ZQuery(RefCurrencySchema.RX_Code, "USD");
			Invoice.AH_RX_NKTransactionCurrency = (Factory.LoadTop1<RefCurrency>(filter)).RX_Code;
			Invoice.AH_ExchangeRate = 0.7M;
			Invoice.AH_OSExTaxAmount = 800.00M;
			Invoice.AH_OSTaxAmount = 40.00M;
			Invoice.AH_OutstandingAmount = 840.00M;
			AssertEquals("TotalLocalInvoiceAmount", 1200.00M, TransactionWrapper.TotalLocalInvoiceAmount);
		}

		public void TestDebit()
		{
			AssertNotNull("Debit", TransactionWrapper.Debit);
		}

		public void TestCredit()
		{
			AssertNotNull("Credit", TransactionWrapper.Credit);
		}

		public void TestExchangeRate()
		{
			ZDecimal exchangeRate = new ZDecimal(12.28);
			Invoice.AH_ExchangeRate = exchangeRate;
			AssertEquals("ExchangeRate", exchangeRate, TransactionWrapper.ExchangeRate);
		}

		public void TestGSTAmount()
		{
			ZDecimal gSTAmount = new ZDecimal(12.43);
			Invoice.AH_LocalTaxAmount = gSTAmount;
			AssertEquals("GSTAmount", gSTAmount, TransactionWrapper.GSTAmount);
		}

		public void TestInvoiceAmount()
		{
			ZDecimal invoiceAmount = new ZDecimal(12.43);
			Invoice.AH_LocalExTaxAmount = invoiceAmount;
			AssertEquals("InvoiceAmount", invoiceAmount, TransactionWrapper.InvoiceAmount);
		}

		public void TestOSTotal()
		{
			ZDecimal oSTotal = new ZDecimal(123.42);
			Invoice.AH_OSTotalAmount = oSTotal;
			AssertEquals("OSTotal", oSTotal, TransactionWrapper.OSTotal);
		}

		public void TestOutstandingAmount()
		{
			ZDecimal outstandingAmount = new ZDecimal(12.28);
			Invoice.AH_OutstandingAmount = outstandingAmount;
			AssertEquals("OutstandingAmount", outstandingAmount, TransactionWrapper.OutstandingAmount);
		}

		public void TestWithholdingTax()
		{
			ZDecimal withholdingTax = new ZDecimal(12.28);
			Invoice.AH_WithholdingTax = withholdingTax;
			AssertEquals("WithholdingTax", withholdingTax, TransactionWrapper.WithholdingTax);
		}

		public void TestTransactionDueDate()
		{
			ZDateTime dueDate = new ZDateTime(2004, 01, 01);
			Invoice.AH_DueDate = dueDate;
			AssertEquals("TransactionDueDate", dueDate, TransactionWrapper.TransactionDueDate);

			Invoice.AH_TransactionType = "REC";
			AssertEquals("TransactionDueDate", ZDateTime.Empty, TransactionWrapper.TransactionDueDate);
		}

		public void TestCreatedDate()
		{
			AssertNotNull("CreatedDate", TransactionWrapper.CreatedDate);
		}

		public void TestDueDate()
		{
			ZDateTime dueDate = new ZDateTime(2004, 01, 01);
			Invoice.AH_DueDate = dueDate;
			AssertEquals("DueDate", dueDate, TransactionWrapper.DueDate);
		}

		public void TestFullyPaidDate()
		{
			ZDateTime fullyPaidDate = new ZDateTime(2004, 01, 01);
			Invoice.AH_FullyPaidDate = fullyPaidDate;
			AssertEquals("FullyPaidDate", fullyPaidDate, TransactionWrapper.FullyPaidDate);
		}

		public void TestInvoiceDate()
		{
			ZDateTime invoiceDate = new ZDateTime(2004, 01, 01);
			Invoice.AH_InvoiceDate = invoiceDate;
			AssertEquals("InvoiceDate", invoiceDate, TransactionWrapper.InvoiceDate);
		}

		public void TestComplianceDocDate()
		{
			ZDate invoiceDate = new ZDate(2004, 01, 01);
			Invoice.AH_ComplianceDocumentDate = invoiceDate;
			AssertEquals("ComplianceDocumentDate", invoiceDate, TransactionWrapper.ComplianceDocDate);
		}

		public void TestPostDate()
		{
			ZDateTime postDate = new ZDateTime(2004, 01, 01);
			Invoice.AH_PostDate = postDate;
			AssertEquals("PostDate", postDate, TransactionWrapper.PostDate);
		}

		public void TestCalcLineNumber()
		{
			TransactionWrapper.CalcLineNumber = 1;
			AssertEquals("CalcLineNumber", 1, TransactionWrapper.CalcLineNumber);

			TransactionWrapper.CalcLineNumber = 50;
			AssertEquals("CalcLineNumber", 50, TransactionWrapper.CalcLineNumber);
		}

		public void TestPostPeriod()
		{
			ZDateTime postDate = new ZDateTime(2004, 01, 01);
			Invoice.AH_PostDate = postDate;
			AssertNotNull("PostPeriod", TransactionWrapper.PostPeriod);
		}

		public void TestAgePeriod()
		{
			ZDateTime agePeriod = new ZDateTime(2004, 01, 01);
			Invoice.AH_DueDate = agePeriod;
			AssertNotNull("AgePeriod", TransactionWrapper.AgePeriod);
		}

		public void TestCurrentCompany()
		{
			AssertNotNull("CurrentCompany", TransactionWrapper.CurrentCompany);
			AssertEquals("CurrentCompany is of type DocCompany", typeof(DocCompany), TransactionWrapper.CurrentCompany.GetType());
		}

		public void TestBankAccount()
		{
			Invoice.AH_AB = ZGuid.Empty;
			AssertNull("BankAccount", TransactionWrapper.BankAccount);

			Invoice.AH_AB = Factory.New(typeof(AccBankAccount)).PK;
			AssertNotNull("BankAccount", TransactionWrapper.BankAccount);
			AssertEquals("BankAccount is of type DocBankAccount", typeof(DocBankAccount), TransactionWrapper.BankAccount.GetType());
		}

		public void TestVoucher()
		{
			AssertNull("Voucher", TransactionWrapper.Voucher);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var transactionTypes = new CodeDescriptionPairList(OLookUpEditType.TransactionTypes);
				foreach (CodeDescriptionPair transactionType in transactionTypes)
				{
					Invoice.AH_TransactionType = transactionType.Code;
					var wrapper = DocTransactionHeader.New(Invoice, Factory);
					if (VoucherProviderFactory.SupportList.Contains(wrapper.TransactionType))
					{
						AssertNotNull(wrapper.Voucher);
					}
					else
					{
						AssertNull(wrapper.Voucher);
					}
				}
			}
		}

		public void TestRoutingTransitNumber()
		{
			Invoice.AH_AB = ZGuid.Empty;
			AssertNull("BankAccount", TransactionWrapper.BankAccount);
			AssertEquals("RoutingTransitNumber", ZString.Empty, TransactionWrapper.RoutingTransitNumber);

			var bank = Factory.New<AccBankAccount>();
			bank.AB_BankName = "Test Bank Account";
			bank.AB_BSB = "123456";
			bank.AB_BankAddress = "123 Address Test";
			bank.AB_GC = GlbCompany.CurrentCompany.PK;
			bank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bank.AB_GB = GlbBranch.CurrentBranch.PK;
			Invoice.AH_AB = bank.PK;

			AssertEquals("RoutingTransitNumber", "123456", TransactionWrapper.RoutingTransitNumber);
		}

		public void TestRoutingTransitNumberFractionForm()
		{
			Invoice.AH_AB = ZGuid.Empty;
			AssertNull("BankAccount", TransactionWrapper.BankAccount);
			AssertEquals("RoutingTransitNumberFractionForm", "/", TransactionWrapper.RoutingTransitNumberFractionForm);

			var bank = Factory.New<AccBankAccount>();
			bank.AB_BankName = "Test Bank Account";
			bank.AB_BSB = "123456789";
			bank.AB_BankAddress = "123 Address Test";
			bank.AB_GC = GlbCompany.CurrentCompany.PK;
			bank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bank.AB_GB = GlbBranch.CurrentBranch.PK;
			Invoice.AH_AB = bank.PK;
			AssertEquals("RoutingTransitNumberFractionForm", "5678/1234", TransactionWrapper.RoutingTransitNumberFractionForm);
			bank.AB_BSB = "123456";
			AssertEquals("RoutingTransitNumberFractionForm", "56/1234", TransactionWrapper.RoutingTransitNumberFractionForm);
			bank.AB_BSB = "123";
			AssertEquals("RoutingTransitNumberFractionForm", "/123", TransactionWrapper.RoutingTransitNumberFractionForm);
		}

		public void TestGLAccount()
		{
			Invoice.AH_AG = ZGuid.Empty;
			AssertNull("GLAccount", TransactionWrapper.GLAccount);

			Invoice.AH_AG = Factory.New(typeof(AccGLHeader)).PK;
			AssertNotNull("GLAccount", TransactionWrapper.GLAccount);
			AssertEquals("GLAccount is of type DocGLAccount", typeof(DocGLAccount), TransactionWrapper.GLAccount.GetType());
		}

		public void TestBranch()
		{
			Invoice.AH_GB = ZGuid.Empty;
			AssertNull("Branch", TransactionWrapper.Branch);

			Invoice.AH_GB = Factory.New(typeof(GlbBranch)).PK;
			AssertNotNull("Branch", TransactionWrapper.Branch);
			AssertEquals("Branch is of type DocBranch", typeof(DocBranch), TransactionWrapper.Branch.GetType());
		}

		public void TestDepartment()
		{
			Invoice.AH_GE = ZGuid.Empty;
			AssertNull("Department", TransactionWrapper.Department);

			Invoice.AH_GE = Factory.New(typeof(GlbDepartment)).PK;
			AssertNotNull("Department", TransactionWrapper.Department);
			AssertEquals("Department is of type DocDepartment", typeof(DocDepartment), TransactionWrapper.Department.GetType());
		}

		public void TestInvoicingJob()
		{
			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = jobHeader.PK;
			Invoice.AH_JH = jobHeader.PK;
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			DocTransactionHeader docHeader = DocTransactionHeader.New(newFactory, Invoice.PK);
			AssertNotNull("Job", docHeader.JobHeader);

			ZQuery query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			AssertEquals(0, newFactory.Load<Charge>(query).Length);

			jobCharge = newFactory.Load<Charge>(jobCharge.PK);
			AssertEquals(1, newFactory.Load<Charge>(query).Length);
		}

		public void TestJob()
		{
			Invoice.AH_JH = ZGuid.Empty;
			AssertNull("Job", TransactionWrapper.JobHeader);

			Invoice.AH_JH = Factory.NewJobForTesting<JobHeader>().PK;
			AssertNotNull("Job", TransactionWrapper.JobHeader);
			AssertEquals("Job is of type DocJobHeader", typeof(DocJobHeader), TransactionWrapper.JobHeader.GetType());
		}

		public void TestOrganisation()
		{
			Invoice.AH_OH = ZGuid.Empty;
			AssertNull("Organisation", TransactionWrapper.Organisation);

			OrgHeader orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORG";
			Invoice.AH_OH = orgHeader.PK;
			AssertNotNull("Organisation", TransactionWrapper.Organisation);
			AssertEquals("Organisation is of type DocOrganisation", typeof(DocOrganisation), TransactionWrapper.Organisation.GetType());
		}

		public void TestCurrency()
		{
			Invoice.AH_RX_NKTransactionCurrency = ZString.Empty;
			AssertNull("Currency", TransactionWrapper.Currency);

			Invoice.AH_RX_NKTransactionCurrency = "AUD";
			AssertNotNull("Currency", TransactionWrapper.Currency);
			AssertEquals("Currency is of type DocCurrency", typeof(DocCurrency), TransactionWrapper.Currency.GetType());
		}

		public void TestTransactionBelongsToGroup()
		{
			ZGuid transactionBelongsToGroup = ZGuid.NewZGuid();
			Invoice.AH_TransactionBelongsToGroup = transactionBelongsToGroup;
			AssertEquals("TransactionBelongsToGroup", transactionBelongsToGroup, TransactionWrapper.TransactionBelongsToGroup);
		}

		public void TestCashBasisGSTIndicator()
		{
			Invoice.AH_CashBasisGSTIndicator = ZBool.False;
			Assert("!CashBasisGSTIndicator", !TransactionWrapper.CashBasisGSTIndicator);

			Invoice.AH_CashBasisGSTIndicator = ZBool.True;
			Assert("CashBasisGSTIndicator", TransactionWrapper.CashBasisGSTIndicator);
		}

		public void TestCashBasisGSTRealisedToGL()
		{
			Invoice.AH_CashBasisGSTRealisedToGL = ZBool.False;
			Assert("!CashBasisGSTRealisedToGL", !TransactionWrapper.CashBasisGSTRealisedToGL);

			Invoice.AH_CashBasisGSTRealisedToGL = ZBool.True;
			Assert("CashBasisGSTRealisedToGL", TransactionWrapper.CashBasisGSTRealisedToGL);
		}

		public void TestInvoiceApproved()
		{
			Invoice.AH_InvoiceApproved = ZBool.False;
			Assert("!InvoiceApproved", !TransactionWrapper.InvoiceApproved);

			Invoice.AH_InvoiceApproved = ZBool.True;
			Assert("InvoiceApproved", TransactionWrapper.InvoiceApproved);
		}

		public void TestInvoicePrinted()
		{
			Invoice.AH_InvoicePrinted = ZBool.False;
			Assert("!InvoicePrinted", !TransactionWrapper.InvoicePrinted);

			Invoice.AH_InvoicePrinted = ZBool.True;
			Assert("InvoicePrinted", TransactionWrapper.InvoicePrinted);
		}

		public void TestIsCancelled()
		{
			Invoice.AH_IsCancelled = ZBool.False;
			Assert("!IsCancelled", !TransactionWrapper.IsCancelled);

			Invoice.AH_IsCancelled = ZBool.True;
			Assert("IsCancelled", TransactionWrapper.IsCancelled);
		}

		public void TestIsClearedInCashbook()
		{
			Invoice.AH_DateClearedInCashbook = ZDateTime.Empty;
			Assert("!IsClearedInCashbook", !TransactionWrapper.IsClearedInCashbook);

			Invoice.AH_DateClearedInCashbook = ZDateTime.Today;
			Assert("IsClearedInCashbook", TransactionWrapper.IsClearedInCashbook);
		}

		public void TestIsDisbursement()
		{
			Invoice.AH_TransactionCategory = "";
			Assert("!IsDisbursement", !TransactionWrapper.IsDisbursement);

			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			Assert("IsDisbursement", TransactionWrapper.IsDisbursement);
		}

		public void TestNotAllocated()
		{
			Invoice.AH_NotAllocated = ZBool.False;
			Assert("!NotAllocated", !TransactionWrapper.NotAllocated);

			Invoice.AH_NotAllocated = ZBool.True;
			Assert("NotAllocated", TransactionWrapper.NotAllocated);
		}

		public void TestPOST1()
		{
			Invoice.AH_POST1 = ZBool.False;
			Assert("!POST1", !TransactionWrapper.POST1);

			Invoice.AH_POST1 = ZBool.True;
			Assert("POST1", TransactionWrapper.POST1);
		}

		public void TestPOST2()
		{
			Invoice.AH_POST2 = ZBool.False;
			Assert("!POST2", !TransactionWrapper.POST2);

			Invoice.AH_POST2 = ZBool.True;
			Assert("POST2", TransactionWrapper.POST2);
		}

		public void TestPOST3()
		{
			Invoice.AH_POST3 = ZBool.False;
			Assert("!POST3", !TransactionWrapper.POST3);

			Invoice.AH_POST3 = ZBool.True;
			Assert("POST3", TransactionWrapper.POST3);
		}

		public void TestPOST4()
		{
			Invoice.AH_POST4 = ZBool.False;
			Assert("!POST4", !TransactionWrapper.POST4);

			Invoice.AH_POST4 = ZBool.True;
			Assert("POST4", TransactionWrapper.POST4);
		}

		public void TestPostedToEFT()
		{
			Invoice.AH_PostedToEFT = ZBool.False;
			Assert("!PostedToEFT", !TransactionWrapper.PostedToEFT);

			Invoice.AH_PostedToEFT = ZBool.True;
			Assert("PostedToEFT", TransactionWrapper.PostedToEFT);
		}

		public void TestInvoiceTermDays()
		{
			Invoice.AH_InvoiceTermDays = (ZByte)1;
			AssertEquals("InvoiceTermDays", (ZByte)1, TransactionWrapper.InvoiceTermDays);
		}

		public void TestTransactionCount()
		{
			Invoice.AH_TransactionCount = (ZByte)1;
			AssertEquals("TransactionCount", (ZByte)1, TransactionWrapper.TransactionCount);
		}

		public void TestAccountsPayableSuppliersReference()
		{
			AssertEquals("Accounts Payable Suppliers Reference", ZString.Empty, TransactionWrapper.AccountsPayableSuppliersReference);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Invoice.AH_OH = header.PK;
			OrgCusCode code = header.CustomsCodes.AddNew();
			code.OK_CodeType = OrgCusCode.CodeTypes.AccountsPayableSuppliersReference;
			code.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			code.OK_CustomsRegNo = "test reference";
			code.OK_OH = header.PK;
			Factory.Save();

			AssertEquals("Accounts Payable Suppliers Reference", "test reference", TransactionWrapper.AccountsPayableSuppliersReference);
		}

		#endregion

		#region Receipt Matching Fields

		[Enterprise.MasterFiles.Business.Testing.SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestMatchedLines()
		{
			AssertEquals(0, TransactionWrapper.MatchedLines.Count);

			Invoice.AH_InvoiceDate = ZDateTime.Today;
			Invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			Invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			Invoice.AH_RX_NKTransactionCurrency = "AUD";

			AccTransactionMatchLink invoiceMatchLink1 = ((IMatching)Invoice).CurrentMatchGroup.AddNew();
			invoiceMatchLink1.AP_AH = Invoice.PK;
			invoiceMatchLink1.AP_MatchDate = ZDateTime.Today;
			invoiceMatchLink1.AP_MatchGroupNum = "Z0001987";

			ARInvoiceLine line1 = (ARInvoiceLine)Invoice.Lines.AddNew();
			ARInvoiceLine line2 = (ARInvoiceLine)Invoice.Lines.AddNew();
			line2.AL_RX_NKTransactionCurrency = "USD";

			AccTransLinePay linePay1 = Factory.NewWithValidTestData<AccTransLinePay>();
			linePay1.A7_AL = line1.PK;
			linePay1.A7_Amount = 555m;
			linePay1.A7_OSAmount = 500m;
			linePay1.A7_AP = invoiceMatchLink1.PK;

			AccTransLinePay linePay2 = Factory.NewWithValidTestData<AccTransLinePay>();
			linePay2.A7_AL = line2.PK;
			linePay2.A7_Amount = 777m;
			linePay2.A7_OSAmount = 700m;
			linePay2.A7_AP = invoiceMatchLink1.PK;

			Factory.Save();
			line2.AL_ExchangeRate = 0.65;
			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			TransactionWrapper.MatchLink = DocMatchLink.New(invoiceMatchLink1, Factory);
			AssertEquals(2, TransactionWrapper.MatchedLines.Count);
			AssertEquals(555m, TransactionWrapper.MatchedLines[0].MatchedAmount);
			AssertEquals(777m, TransactionWrapper.MatchedLines[1].MatchedAmount);
			AssertEquals(500m, TransactionWrapper.MatchedLines[0].MatchedOSAmount);
			AssertEquals(700m, TransactionWrapper.MatchedLines[1].MatchedOSAmount);
		}

		public void TestReceiptMatches()
		{
			var receipt = Factory.New<ARReceipt>();
			DocTransactionHeader receiptWrapper = DocTransactionHeader.New(receipt, Factory);
			AssertEquals(0, receiptWrapper.ReceiptMatches.Count);

			SetTransactionHeaderSettings(receipt, 600M);
			SetTransactionHeaderSettings(Invoice, 200M);

			SetMatchLink(receipt, receipt.PK, -200M, "GROUP1");
			SetMatchLink(receipt, Invoice.PK, 200M, "GROUP1");

			receipt.AH_OutstandingAmount = -400M;
			Invoice.AH_OutstandingAmount = 0;

			Factory.Save();
			receiptWrapper = DocTransactionHeader.New(receipt, Factory);
			AssertEquals(2, receiptWrapper.ReceiptMatches.Count);
		}

		public void TestReceiptMatchesSortedByInvoiceDate()
		{
			// Note: AH_InvoiceDate is not nullable so can't test ordering of transactions with null invoice date
			ARInvoice aRINV1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRINV1.AH_InvoiceDate = new ZDateTime(2004, 3, 3);
			aRINV1.AH_TransactionNum = "00001880";

			ARInvoice aRINV2 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRINV2.AH_InvoiceDate = new ZDateTime(2004, 4, 4);
			aRINV2.AH_TransactionNum = "00001001";

			APInvoice aPINV1 = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			aPINV1.AH_InvoiceDate = new ZDateTime(2003, 5, 5);
			aPINV1.AH_TransactionNum = "00001990";

			AccTransactionMatchLink matchlink1 = ((IMatching)aRINV1).CurrentMatchGroup.AddNew();
			matchlink1.AP_AH = aRINV1.PK;
			matchlink1.AP_MatchGroupNum = "M00001009";

			AccTransactionMatchLink matchlink2 = ((IMatching)aRINV2).CurrentMatchGroup.AddNew();
			matchlink2.AP_AH = aRINV2.PK;
			matchlink2.AP_MatchGroupNum = "M00001009";

			AccTransactionMatchLink matchlink3 = ((IMatching)aPINV1).CurrentMatchGroup.AddNew();
			matchlink3.AP_AH = aPINV1.PK;
			matchlink3.AP_MatchGroupNum = "M00001009";

			TestObjectCreator.SetupMatchLinkMatchDate(aRINV1);
			TestObjectCreator.SetupMatchLinkMatchDate(aRINV2);
			TestObjectCreator.SetupMatchLinkMatchDate(aPINV1);

			Factory.Save();

			DocTransactionHeader testDocTransHeader = DocTransactionHeader.New(Factory, aRINV1.PK);

			AssertEquals("There should be 3 transactions in ReceiptMatches", 3, testDocTransHeader.ReceiptMatches.Count);
			AssertEquals("The first transaction should have earliest invoicedate", aPINV1.AH_TransactionNum, testDocTransHeader.ReceiptMatches[0].TransactionNumber);
			AssertEquals("The second transaction should have 2nd earliest invoicedate", aRINV1.AH_TransactionNum, testDocTransHeader.ReceiptMatches[1].TransactionNumber);
			AssertEquals("The third transaction should have most recent invoicedate", aRINV2.AH_TransactionNum, testDocTransHeader.ReceiptMatches[2].TransactionNumber);
		}

		public void TestCurrentCompanyReceiptMatches()
		{
			var thisCompanyReceipt1 = Factory.New<ARReceipt>();
			var thisCompanyReceipt2 = Factory.New<ARReceipt>();

			DocTransactionHeader receiptWrapper = DocTransactionHeader.New(thisCompanyReceipt1, Factory);
			AssertEquals(0, receiptWrapper.ReceiptMatches.Count);

			SetTransactionHeaderSettings(thisCompanyReceipt1, 600M);
			SetTransactionHeaderSettings(thisCompanyReceipt2, 300M);
			SetTransactionHeaderSettings(Invoice, 300M);

			SetMatchLink(Invoice, thisCompanyReceipt1.PK, -200M, "GROUP1");
			SetMatchLink(Invoice, thisCompanyReceipt2.PK, -100M, "GROUP1");
			SetMatchLink(Invoice, Invoice.PK, 300M, "GROUP1");

			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			company.GC_RX_NKLocalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			GlbBranch branch = company.Branches.AddNew();

			var otherCompanyReceipt = Factory.New<ARReceipt>();
			var otherCompanyInvoice = Factory.New<ARInvoice>();

			SetTransactionHeaderSettings(otherCompanyReceipt, 1500M);
			otherCompanyReceipt.AH_GB = branch.PK;
			SetTransactionHeaderSettings(otherCompanyInvoice, 800M);
			otherCompanyInvoice.AH_GB = branch.PK;
			otherCompanyInvoice.Lines[0].AL_GB = otherCompanyInvoice.Company.Branches[0].PK;

			SetMatchLink(otherCompanyInvoice, otherCompanyReceipt.PK, -500M, "GROUP1");
			SetMatchLink(otherCompanyInvoice, otherCompanyInvoice.PK, 800M, "GROUP1");
			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			headerToMatch.AH_Ledger = "AR";
			headerToMatch.AH_TransactionType = "INV";
			headerToMatch.AH_RX_NKTransactionCurrency = GlbBranch.CurrentBranch.Country.RN_RX_NKLocalCurrency;
			headerToMatch.AH_InvoiceAmount = -300M;
			headerToMatch.AH_FullyPaidDate = ZDateTime.Now;
			SetMatchLink(otherCompanyInvoice, headerToMatch.PK, -300M, "GROUP1");

			thisCompanyReceipt1.AH_OutstandingAmount = -400M;
			thisCompanyReceipt2.AH_OutstandingAmount = -200M;
			Invoice.AH_OutstandingAmount = 0;
			otherCompanyReceipt.AH_OutstandingAmount = -1000M;
			otherCompanyInvoice.AH_OutstandingAmount = 0;

			Factory.Save();
			receiptWrapper = DocTransactionHeader.New(thisCompanyReceipt1, Factory);
			AssertEquals(3, receiptWrapper.ReceiptMatches.Count);
		}

		public void TestOneReceiptMultipleInvoices()
		{
			var receipt = Factory.New<ARReceipt>();
			DocTransactionHeader receiptWrapper = DocTransactionHeader.New(receipt, Factory);

			SetTransactionHeaderSettings(receipt, 840M);
			SetTransactionHeaderSettings(Invoice, 200M);

			var invoice2 = Factory.New<ARInvoice>();
			var invoice3 = Factory.New<ARInvoice>();
			SetTransactionHeaderSettings(invoice2, 20M);
			SetTransactionHeaderSettings(invoice3, 60M);

			SetMatchLink(receipt, receipt.PK, -280M, "GROUP2");
			SetMatchLink(receipt, Invoice.PK, 200M, "GROUP2");
			SetMatchLink(receipt, invoice2.PK, 20M, "GROUP2");
			SetMatchLink(receipt, invoice3.PK, 60M, "GROUP2");

			receipt.AH_OutstandingAmount = -560M;
			Invoice.AH_OutstandingAmount = 0;
			invoice2.AH_OutstandingAmount = 0;
			invoice3.AH_OutstandingAmount = 0;

			Factory.Save();
			AssertEquals(4, receiptWrapper.ReceiptMatches.Count);
		}

		public void TestManyReceiptsOneInvoice()
		{
			var receipt1 = Factory.New<ARReceipt>();
			var receipt2 = Factory.New<ARReceipt>();

			DocTransactionHeader receiptWrapper = DocTransactionHeader.New(receipt2, Factory);

			SetTransactionHeaderSettings(receipt1, 400M);
			SetTransactionHeaderSettings(receipt2, 500M);
			SetTransactionHeaderSettings(Invoice, 300M);

			SetMatchLink(Invoice, receipt1.PK, -100M, "GROUP2");
			SetMatchLink(Invoice, receipt2.PK, -200M, "GROUP2");
			SetMatchLink(Invoice, Invoice.PK, 300M, "GROUP2");

			receipt1.AH_OutstandingAmount = -300M;
			receipt2.AH_OutstandingAmount = -300M;
			Invoice.AH_OutstandingAmount = 0;

			Factory.Save();
			AssertEquals(3, receiptWrapper.ReceiptMatches.Count);
		}

		public void TestMultipleMatchLinks()
		{
			var receipt = Factory.New<ARReceipt>();
			DocTransactionHeader receiptWrapper = DocTransactionHeader.New(receipt, Factory);

			SetTransactionHeaderSettings(receipt, 3000M);
			SetTransactionHeaderSettings(Invoice, 300M);

			SetMatchLink(receipt, receipt.PK, -300M, "GROUP2");
			SetMatchLink(receipt, Invoice.PK, 300M, "GROUP2");

			var invoice2 = Factory.New<ARInvoice>();
			SetTransactionHeaderSettings(invoice2, 700M);
			SetMatchLink(receipt, receipt.PK, -700M, "GROUP1");
			SetMatchLink(receipt, invoice2.PK, 700M, "GROUP1");

			receipt.AH_OutstandingAmount = -2000M;
			Invoice.AH_OutstandingAmount = 0;
			invoice2.AH_OutstandingAmount = 0;

			Factory.Save();
			AssertEquals(4, receiptWrapper.ReceiptMatches.Count);

			DocTransactionHeader transactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			AssertEquals(2, transactionWrapper.ReceiptMatches.Count);
		}

		public void TestIsCurrentTransactionHeader()
		{
			var receipt = Factory.New<ARReceipt>();
			receipt.AH_Desc = "REC";
			Invoice.AH_Desc = "INV";
			DocTransactionHeader receiptWrapper = DocTransactionHeader.New(receipt, Factory);
			AssertEquals("", receiptWrapper.IsCurrentTransactionHeader);

			SetTransactionHeaderSettings(receipt, 600M);
			SetTransactionHeaderSettings(Invoice, 200M);

			SetMatchLink(receipt, receipt.PK, -200M, "GROUP1");
			SetMatchLink(receipt, Invoice.PK, 200M, "GROUP1");

			receipt.AH_OutstandingAmount = -400M;
			Invoice.AH_OutstandingAmount = 0;

			Factory.Save();
			AssertEquals(2, TransactionWrapper.ReceiptMatches.Count);
			foreach (DocTransactionHeader aDocTransactionHeader in receiptWrapper.ReceiptMatches)
			{
				if (aDocTransactionHeader.Desc == "REC")
				{
					AssertEquals("Should be the calling transaction", "*", aDocTransactionHeader.IsCurrentTransactionHeader);
				}
				else
				{
					AssertEquals("Not the calling transaction", "", aDocTransactionHeader.IsCurrentTransactionHeader);
				}
			}
		}

		public void TestIsCurrenctTransactionHeaderWithMultipleReceipts()
		{
			var receipt1 = Factory.New<ARReceipt>();
			receipt1.AH_Desc = "1";
			var receipt2 = Factory.New<ARReceipt>();
			receipt2.AH_Desc = "2";
			Invoice.AH_Desc = "3";
			DocTransactionHeader receiptWrapper = DocTransactionHeader.New(receipt2, Factory);

			SetTransactionHeaderSettings(receipt1, 600M);
			SetTransactionHeaderSettings(receipt2, 1500M);
			SetTransactionHeaderSettings(Invoice, 700M);

			SetMatchLink(Invoice, receipt1.PK, -200M, "GROUP2");
			SetMatchLink(Invoice, receipt2.PK, -500M, "GROUP2");
			SetMatchLink(Invoice, Invoice.PK, 700M, "GROUP2");

			receipt1.AH_OutstandingAmount = -400M;
			receipt2.AH_OutstandingAmount = -1000M;
			Invoice.AH_OutstandingAmount = 0;

			Factory.Save();
			AssertEquals(3, TransactionWrapper.ReceiptMatches.Count);
			foreach (DocTransactionHeader aDocTransactionHeader in receiptWrapper.ReceiptMatches)
			{
				if (aDocTransactionHeader.Desc == "2")
				{
					AssertEquals("Should be the calling transaction", "*", aDocTransactionHeader.IsCurrentTransactionHeader);
				}
				else
				{
					AssertEquals("Not the calling transaction", "", aDocTransactionHeader.IsCurrentTransactionHeader);
				}
			}
		}

		public void TestPartialMatch()
		{
			var receipt = Factory.New<ARReceipt>();
			DocTransactionHeader receiptWrapper = DocTransactionHeader.New(receipt, Factory);

			SetTransactionHeaderSettings(receipt, 1200M);
			SetTransactionHeaderSettings(Invoice, 100M);

			SetMatchLink(Invoice, receipt.PK, -100M, "GROUP1");
			SetMatchLink(Invoice, Invoice.PK, 100M, "GROUP1");

			receipt.AH_OutstandingAmount = -1100M;
			Invoice.AH_OutstandingAmount = 0;

			Factory.Save();
			AssertEquals(2, receiptWrapper.ReceiptMatches.Count);
			AssertEquals(100M, Math.Abs(receiptWrapper.ReceiptMatches[0].MatchLink.Amount));
			AssertEquals(100M, Math.Abs(receiptWrapper.ReceiptMatches[1].MatchLink.Amount));
		}

		public void TestDepositBatch()
		{
			var receipt = Factory.New<ARReceipt>();
			DocTransactionHeader receiptWrapper = DocTransactionHeader.New(receipt, Factory);

			SetTransactionHeaderSettings(receipt, -200M);
			SetTransactionHeaderSettings(Invoice, 200M);

			AccBankAccount[] defaultBankAccounts = (AccBankAccount[])Factory.Load(typeof(AccBankAccount), new ZQuery(AccBankAccountSchema.AB_IsDefaultReceiptBankAccount, ZBool.True).AddToFilter(AccBankAccountSchema.AB_GC, GlbCompany.CurrentCompany.PK));

			foreach (AccBankAccount account in defaultBankAccounts)
			{
				account.AB_IsDefaultReceiptBankAccount = ZBool.False;
			}

			var headerBisObj = Factory.NewWithValidTestData<AccGLHeader>();

			var bank = Factory.New<AccBankAccount>();
			bank.AB_BankName = "Test Bank Account";
			bank.AB_BankAddress = "123 Address Test";
			bank.AB_GC = GlbCompany.CurrentCompany.PK;
			bank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bank.AB_GB = GlbBranch.CurrentBranch.PK;
			bank.AB_IsDefaultReceiptBankAccount = ZBool.True;
			bank.AB_AG = headerBisObj.PK;
			bank.AB_Code = "ABCBANK";

			GlbCompany newCompany = Factory.NewWithValidTestData<GlbCompany>();

			DepositBatch depositBatchTransactionOtherCompany = Factory.NewWithValidTestData<DepositBatch>();
			depositBatchTransactionOtherCompany.AH_TransactionType = ZArchitecture.Core.TransactionTypes.ReceiptBatch;
			depositBatchTransactionOtherCompany.AH_ReceiptBatchNo = "00009999";
			depositBatchTransactionOtherCompany.AH_InvoiceDate = ZDateTime.BrettsBirthday;
			depositBatchTransactionOtherCompany.AH_GC = newCompany.PK;

			var depositBatchTransaction = Factory.New<DepositBatch>();
			depositBatchTransaction.AH_TransactionType = ZArchitecture.Core.TransactionTypes.ReceiptBatch;
			depositBatchTransaction.AH_Ledger = ZArchitecture.Core.LedgerTypes.CashBook;
			depositBatchTransaction.AH_AB = bank.PK;
			SetTransactionHeaderSettings(depositBatchTransaction, 200M);

			depositBatchTransaction.AH_ReceiptBatchNo = "00009999";
			receipt.AH_ReceiptBatchNo = "00009999";
			Factory.Save();

			AssertEquals(ZDateTime.Today, receiptWrapper.DocDepositBatch.BatchDate);
		}

		public void TestTotalOSAmount()
		{
			var receipt = Factory.New<ARReceipt>();
			var eXX = Factory.New<ARExchangeDifference>();
			DocTransactionHeader receiptWrapper = DocTransactionHeader.New(receipt, Factory);

			SetTransactionHeaderSettings(receipt, 200M);
			SetTransactionHeaderSettings(eXX, -30M);
			SetTransactionHeaderSettings(Invoice, 170M);

			SetMatchLink(receipt, receipt.PK, -200M, "GROUP1");
			SetMatchLink(receipt, eXX.PK, 30M, "GROUP1");
			SetMatchLink(receipt, Invoice.PK, 170M, "GROUP1");

			receipt.AH_OutstandingAmount = 0M;
			eXX.AH_OutstandingAmount = 0M;
			Invoice.AH_OutstandingAmount = 0M;
			receipt.AH_FullyPaidDate = ZDateTime.Now;
			eXX.AH_FullyPaidDate = ZDateTime.Now;
			Invoice.AH_FullyPaidDate = ZDateTime.Now;

			Factory.Save();
			AssertEquals(3, receiptWrapper.ReceiptMatches.Count);
			foreach (DocTransactionHeader header in receiptWrapper.ReceiptMatches)
			{
				if (header.TransactionType == ZArchitecture.Core.TransactionTypes.Invoice)
				{
					AssertEquals("Invoice TotalOSAmount", 170M, header.TotalOSAmount);
				}
				else if (header.TransactionType == ZArchitecture.Core.TransactionTypes.Receipt)
				{
					AssertEquals("Receipt TotalOSAmount", 200M, header.TotalOSAmount);
				}
				else
				{
					AssertEquals("EXX TotalOSAmount", -30M, header.TotalOSAmount);
				}
			}
		}
		public void TestDirectReceiptLines()
		{
			var directRCT = Factory.New<DirectReceipt>();
			TransactionWrapper = DocTransactionHeader.New(directRCT, Factory);
			SetInvoiceSettings(directRCT, ZArchitecture.Core.TransactionTypes.DirectReceipt);
			directRCT.AH_TransactionNum = "S0003999";

			directRCT.Lines.AddNew();
			directRCT.Lines.AddNew();
			directRCT.Lines.AddNew();

			Factory.Save();
			TransactionWrapper = DocTransactionHeader.New(directRCT, Factory);
			AssertEquals(3, TransactionWrapper.DirectReceiptLines.Count);
		}

		public void TestPrintLocalValues()
		{
			RefCurrency aUD = GetRefCurrency("AUD");
			RefCurrency uSD = GetRefCurrency("USD");

			GlbCompany.CurrentCompany.SetCountry("AU");
			var receipt = Factory.New<ARReceipt>();
			SetTransactionHeaderSettings(receipt, 3000M);
			SetTransactionHeaderSettings(Invoice, 300M);
			receipt.AH_RX_NKTransactionCurrency = aUD.RX_Code;
			Invoice.AH_RX_NKTransactionCurrency = uSD.RX_Code;

			SetMatchLink(receipt, receipt.PK, -300M, "GROUP2");
			SetMatchLink(receipt, Invoice.PK, 300M, "GROUP2");

			var invoice2 = Factory.New<ARInvoice>();
			SetTransactionHeaderSettings(invoice2, 700M);
			invoice2.AH_RX_NKTransactionCurrency = aUD.RX_Code;
			SetMatchLink(receipt, receipt.PK, -700M, "GROUP1");
			SetMatchLink(receipt, invoice2.PK, 700M, "GROUP1");

			receipt.AH_OutstandingAmount = -2000M;
			Invoice.AH_OutstandingAmount = 0;
			invoice2.AH_OutstandingAmount = 0;

			Factory.Save();
			DocTransactionHeader receiptWrapper = DocTransactionHeader.New(receipt, Factory);
			AssertEquals("Y", receiptWrapper.PrintLocalValues.ToString());

			DocTransactionHeader transactionWrapper = DocTransactionHeader.New(invoice2, Factory);
			AssertEquals("N", transactionWrapper.PrintLocalValues.ToString());
		}

		#endregion

		#region Multiplier

		public void TestMultiplier()
		{
			var invoice = Factory.NewWithValidTestData<ARCreditNote>();
			var inoviceMultiplier = (invoice as ITransactionHeader).Multiplier;

			var invoiceWrapper = DocTransactionHeader.New(invoice, Factory);

			using (AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var wrapperMultiplier = invoiceWrapper.Multiplier;
				AssertEquals(inoviceMultiplier, wrapperMultiplier);
			}

			using (AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var wrapperMultiplier = invoiceWrapper.Multiplier;
				AssertEquals("Revert AR CRD sign as registry dictates", -1 * inoviceMultiplier, wrapperMultiplier);
			}
		}

		#endregion

		#region EInvoicing Number

		public void TestEInvoicingGovernmentAllocatedNumber_RetunsEmpty_WhenInvoiceHasEmptyNumber()
		{
			Assert("Precondition", Invoice.EInvoicingGovernmentAllocatedNumber.IsEmpty);
			Assert(TransactionWrapper.EInvoicingGovernmentAllocatedNumber.IsEmpty);
		}

		public void TestEInvoicingGovernmentAllocatedNumber_RetunsValue_WhenInvoiceHasValue()
		{
			TestMockObjectCreator.CreateAndRegisterIComplianceInfoElectronicInvoicing("",AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber);
			var batch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var pivot = batch.TransactionPivots.AddNew();
			pivot.AIP_ParentID = Invoice.PK;
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Succeed;
			pivot.SetCompanyAndCountryCode(batch.Company);

			var expectedValue = "NumberFromGovernment";
			batch.AIB_GovernmentAllocatedNumber = expectedValue;

			AssertEquals("Precondition: invoice.EInvoicingGovernmentAllocatedNumber", expectedValue, Invoice.EInvoicingGovernmentAllocatedNumber);
			var result = TransactionWrapper.EInvoicingGovernmentAllocatedNumber;
			AssertEquals(expectedValue, result);
		}

		public void TestEInvoicingAutorisationNumber_RetunsEmpty_WhenInvoiceHasEmptyNumber()
		{
			Assert("Precondition", Invoice.EInvoicingAuthorisationNumber.IsEmpty);
			Assert(TransactionWrapper.EInvoicingAuthorisationNumber.IsEmpty);	
		}

		public void TestEInvoicingAutorisationNumber_RetunsValue_WhenInvoiceHasValue()
		{
			var expectedValue = "AutorisationNumber";
			var recordType = "AAA";

			TestMockObjectCreator.CreateAndRegisterIComplianceInfoElectronicInvoicing(recordType, AccTransactionHeaderAuthorisationRecordSchema.Constants.AHF_Number);

			var authorizationRecord = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authorizationRecord.AHF_ParentId = Invoice.PK;
			authorizationRecord.AHF_Number = expectedValue;
			authorizationRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			authorizationRecord.AHF_RecordType = recordType;

			AssertEquals("Precondition: invoice.EInvoicingAuthorisationNumber", expectedValue, Invoice.EInvoicingAuthorisationNumber);
			var result = TransactionWrapper.EInvoicingAuthorisationNumber;
			AssertEquals(expectedValue, result);
		}

		#endregion

		public void TestOrganisationARTermsPaymentMethod()
		{
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			client.CompanyData.ARTerms.DeleteAll();
			client.CompanyData.OB_ARCreditAgreedPaymentMethod = "TRF";
			var term1 = client.CompanyData.ARTerms.AddNew();
			term1.PY_InvoiceClass = "ALL";
			term1.PY_AgreedPaymentMethod = "CHK";
			var term2 = client.CompanyData.ARTerms.AddNew();
			term2.PY_InvoiceClass = "DSB";
			term2.PY_AgreedPaymentMethod = "CRQ";

			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = client.PK;
			DocTransactionHeader invoiceWrapper = DocTransactionHeader.New(invoice, Factory);

			invoice.AH_TransactionCategory = "FIN";
			AssertEquals("Business Check", invoiceWrapper.OrganisationARTermsPaymentMethod);
			invoice.AH_TransactionCategory = "DBT";
			AssertEquals("Collection Request (not attached to collection batch)", invoiceWrapper.OrganisationARTermsPaymentMethod);

			client.CompanyData.ARTerms.Delete(term2);
			AssertEquals("Bank Transfer", invoiceWrapper.OrganisationARTermsPaymentMethod);
		}

		public void TestTransactionARPaymentMethod()
		{
			var invoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 20, 0M, 20, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK, ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, false);
			invoice.AH_AgreedPaymentMethodOverride = "CRQ";
			var invoiceWrapper = DocTransactionHeader.New(invoice, Factory);

			AssertEquals("Collection Request (not attached to collection batch)", invoiceWrapper.TransactionARPaymentMethod);

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var batch = TestObjectCreator.CreateCollectionBatch(bankAccount, GlbCompany.CurrentCompany, "00001001", 100m, false);
			var order = TestObjectCreator.CreateCollectionOrder(batch, ZDateTime.Today.Date, TestObjectCreator.ABIGAS, "0000001", 50m, false);
			TestObjectCreator.CreateCollectionOrderLine(order, invoice, false);
			order.IncludeInBatch = true;

			Factory.Save();

			AssertEquals("Collection Request (attached to collection batch)", invoiceWrapper.TransactionARPaymentMethod);

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.CompanyData.ARTerms.DeleteAll();
			client.CompanyData.OB_ARCreditAgreedPaymentMethod = "TRF";
			var term1 = client.CompanyData.ARTerms.AddNew();
			term1.PY_InvoiceClass = "ALL";
			term1.PY_AgreedPaymentMethod = "CRQ";

			var invoice1 = Factory.New<ARInvoice>();
			invoice1.AH_OH = client.PK;
			var invoiceWrapper1 = DocTransactionHeader.New(invoice1, Factory);

			invoice1.AH_AgreedPaymentMethodOverride = "CHK";
			AssertEquals("Business Check", invoiceWrapper1.TransactionARPaymentMethod);

			client.CompanyData.ARTerms.Delete(term1);
			AssertEquals("Business Check", invoiceWrapper1.TransactionARPaymentMethod);
		}

		public void TestBarcode()
		{
			APPayment payment = Factory.New<APPayment>();
			DocTransactionHeader paymentWrapper = DocTransactionHeader.New(payment, Factory);

			payment.AH_TransactionType = "PAY";
			payment.AH_TransactionNum = "12345";
			AssertEquals(new TextBarcode("^ACP=12345;PAY@EDI|").TextAs128sFontString, paymentWrapper.Barcode);

			payment.AH_TransactionType = "DPY";
			payment.AH_TransactionNum = "19780";
			AssertEquals(new TextBarcode("^ACP=19780;DPY@EDI|").TextAs128sFontString, paymentWrapper.Barcode);
		}

		public void TestCostConfirmationRollupSetting()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			DocTransactionHeader invoiceWrapper = DocTransactionHeader.New(invoice, Factory);
			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentRollupSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				DocRollUpConstants.CostConfirmationDocumentRollupSettingsCodes.Job);
			AssertEquals("JOB", invoiceWrapper.CostConfirmationRollupSetting);
			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentRollupSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				DocRollUpConstants.CostConfirmationDocumentRollupSettingsCodes.ChargeCode);
			AssertEquals("CHG", invoiceWrapper.CostConfirmationRollupSetting);
		}

		public void TestDocumentTitle()
		{
			var testWrapper = Wrappers[0] as DocTransactionHeader;
			AssertEquals(((BusinessObject)testWrapper.WrappedObject).HumanReadableName, ((IGenericTransactionHeaderPlugIn)testWrapper).HeaderSupporter.GetDocumenTitle());
		}

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocTransactionHeader.New(Invoice, Factory) };
		}

		public class DummyDocTransactionHeader : DocTransactionHeader
		{
			protected DummyDocTransactionHeader(TransactionHeader transactionHeader, BusinessObjectFactory factoryToWrap)
				: base(transactionHeader, factoryToWrap)
			{
			}

			public static new DocTransactionHeader New(TransactionHeader transactionHeader, BusinessObjectFactory factoryToWrap)
			{
				return new DummyDocTransactionHeader(transactionHeader, factoryToWrap);
			}

			public static void Register()
			{
				OverridableNewDelegate.Value = new NewDelegate(New);
			}
		}

		ARInvoice Invoice;
		DocTransactionHeader TransactionWrapper;

		protected override void SetUp()
		{
			StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			Invoice = Factory.New<ARInvoice>();
			TransactionWrapper = DocTransactionHeader.New(Invoice, Factory);
			base.SetUp();
		}

		protected override string TestingCountry
		{
			get { return null; }
		}

		TransactionMatchLink SetMatchLink(IMatching matching, ZGuid aP_AH, ZDecimal amount, ZString groupNum)
		{
			TransactionMatchLink link = Factory.New<TransactionMatchLink>();
			matching.CurrentMatchGroup.Add(link);
			link.AP_AH = aP_AH;
			link.AP_Amount = amount;
			link.AP_MatchGroupNum = groupNum;
			link.AP_MatchDate = ZDateTime.Today;
			return link;
		}

		void SetTransactionHeaderSettings(TransactionHeader transactionHeader, ZDecimal amount)
		{
			transactionHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			transactionHeader.AH_GE = GlbDepartment.CurrentDepartment.PK;
			transactionHeader.AH_OSExTaxAmount = amount;
			transactionHeader.AH_InvoiceDate = ZDateTime.Today;
			transactionHeader.AH_DueDate = ZDateTime.Today;
			transactionHeader.AH_PostDate = ZDateTime.Today;
			if (transactionHeader is InvoicingBase)
			{
				TestObjectCreator.CreateInvoiceLine(transactionHeader as InvoicingBase, transactionHeader.TransactionCurrency, transactionHeader.AH_ExchangeRate, amount, 0m, 0m);
			}
		}

		void SetInvoiceSettings(TransactionHeader invoice, ZString type)
		{
			invoice.AH_InvoiceDate = ZDateTime.Today;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_TransactionType = type;
			invoice.AH_RX_NKTransactionCurrency = "AUD";
		}

		RefCurrency GetRefCurrency(ZString currencyCode)
		{
			return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);
		}

		#endregion
	}
}
