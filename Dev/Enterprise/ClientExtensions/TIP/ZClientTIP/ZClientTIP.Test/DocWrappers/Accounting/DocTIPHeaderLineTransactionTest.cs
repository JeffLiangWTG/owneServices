using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TIP.DocWrappers.Testing
{
	[TestedType(typeof(DocTIPHeaderLineTransaction))]
	public class DocTIPHeaderLineTransactionTest : DocumentWrapperTestCase
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorThrowsExceptionWhenNullParameter()
		{
			DocTIPHeaderLineTransaction.New(null, InvoiceLine, Factory);
		}

		public void TestInvoice()
		{
			AssertNotNull(DocTIPWrapper.Invoice);
			AssertEquals("Invoice", Invoice.PK, ((BusinessObject)DocTIPWrapper.Invoice.WrappedObject).PK);
		}

		public void TestTransactionType()
		{
			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			AssertEquals("Transaction Type", DocTIPWrapper.TransactionType, ZArchitecture.Core.TransactionTypes.Invoice);
		}

		public void TestTransactionNumber()
		{
			Invoice.AH_TransactionNum = "0001";
			AssertEquals("Transaction Number", DocTIPWrapper.TransactionNumber, "0001");
		}

		public void TestMatchLink()
		{
			TransactionMatchLink matchLink = ((IMatching)Invoice).CurrentMatchGroup.AddNew();
			matchLink.AP_AH = Invoice.PK;
			matchLink.AP_MatchDate = ZDateTime.Today;
			matchLink.AP_MatchGroupNum = "Z0001987";
			ItemWrapper.MatchLink = DocMatchLink.New(matchLink, Factory);
			AssertNotNull(DocTIPWrapper.MatchLink);
			AssertEquals("Z0001987", DocTIPWrapper.MatchLink.MatchGroupNum);
		}

		public void TestInvoiceDate()
		{
			Invoice.AH_InvoiceDate = new ZDateTime(2006, 10, 25);
			AssertEquals("InvoiceDate", new ZDateTime(2006, 10, 25), DocTIPWrapper.InvoiceDate);
		}

		public void TestDesc()
		{
			Invoice.AH_Desc = "AP INVOICE";
			AssertEquals("Desc", "AP INVOICE", DocTIPWrapper.Desc);
		}

		public void TestCurrency()
		{
			Invoice.AH_RX_NKTransactionCurrency = "USD";
			AssertEquals("Currency", "USD", DocTIPWrapper.Currency.Code);
		}

		public void TestExchangeRate()
		{
			Invoice.AH_ExchangeRate = 0.75m;
			AssertEquals("Exchange Rate", 0.75m, DocTIPWrapper.ExchangeRate);
		}

		public void TestChargeCode()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "CODE";
			InvoiceLine.AL_AC = chargeCode.PK;
			AssertEquals("ChargeCode", "CODE", DocTIPWrapper.ChargeCode);
		}

		public void TestJobNumber()
		{
			JobHeader jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "S000001";
			InvoiceLine.AL_JH = jobHeader.PK;
			AssertEquals("JobNumber", "S000001", DocTIPWrapper.JobNumber);
		}

		public void TestJobNumberEmptyWhenNoLine()
		{
			DocTIPHeaderLineTransaction wrapper = DocTIPHeaderLineTransaction.New(ItemWrapper, null, Factory);
			AssertEquals("JobNumber should be Empty When No Line", ZString.Empty, wrapper.JobNumber);
		}

		public void TestLocalExTaxAmountAsString()
		{
			InvoiceLine.AL_LocalExTaxAmount = 100.0m;
			AssertEquals("LocalExTaxAmountAsString", new ZString("100.0"), DocTIPWrapper.LocalExTaxAmountAsString);
		}

		public void TestLocalExTaxAmountAsStringEmptyWhenNoLine()
		{
			DocTIPHeaderLineTransaction wrapper = DocTIPHeaderLineTransaction.New(ItemWrapper, null, Factory);
			AssertEquals("LocalExTaxAmountAsString", ZString.Empty, wrapper.LocalExTaxAmountAsString);
		}

		public void TestLocalTaxAmountAsString()
		{
			InvoiceLine.AL_LocalTaxAmount = 10.0m;
			AssertEquals("Local Tax amount", new ZString("10.0"), DocTIPWrapper.LocalTaxAmountAsString);
		}

		public void TestLocalTaxAmountAsStringEmptyWhenNoLine()
		{
			DocTIPHeaderLineTransaction wrapper = DocTIPHeaderLineTransaction.New(ItemWrapper, null, Factory);
			AssertEquals("Local Tax amount", ZString.Empty, wrapper.LocalTaxAmountAsString);
		}

		public void TestLocalTotalAmountAsString()
		{
			InvoiceLine.AL_LocalExTaxAmount = 100.0m;
			InvoiceLine.AL_LocalTaxAmount = 10.0m;
			AssertEquals("LocalTotalAmountAsString", "110.0", DocTIPWrapper.LocalTotalAmountAsString);
		}

		public void TestLocalTotalAmountAsStringEmptyWhenNoLine()
		{
			DocTIPHeaderLineTransaction wrapper = DocTIPHeaderLineTransaction.New(ItemWrapper, null, Factory);
			AssertEquals("LocalTotalAmountAsString", ZString.Empty, wrapper.LocalTotalAmountAsString);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocTIPHeaderLineTransaction.New(InvoiceLine, Factory) };
		}

		APInvoice Invoice;
		APInvoiceLine InvoiceLine;
		protected override void SetUp()
		{
			Invoice = Factory.New<APInvoice>();
			InvoiceLine = (APInvoiceLine)Invoice.Lines.AddNew();
			base.SetUp();
		}

		DocTransactionHeader ItemWrapper
		{
			get
			{
				if (fItemWrapper == null)
				{
					fItemWrapper = DocTransactionHeader.New(Invoice, Factory);
				}

				return fItemWrapper;
			}
		}

		DocTransactionHeader fItemWrapper;
		DocTIPHeaderLineTransaction DocTIPWrapper
		{
			get
			{
				if (fDocTIPWrapper == null)
				{
					fDocTIPWrapper = DocTIPHeaderLineTransaction.New(ItemWrapper, InvoiceLine, Factory);
				}

				return fDocTIPWrapper;
			}
		}

		DocTIPHeaderLineTransaction fDocTIPWrapper;
	}
}
