using System;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocTransactionLine))]
	public class DocTransactionLineTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocTransactionLine.New(Line, Factory), };
		}

		public void TestChargeCurrency()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			var currency = Factory.New<RefCurrency>();
			currency.RX_Code = "APS";
			Charge charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_RX_NKCostCurrency = currency.RX_Code;
			charge.JR_AL_APLine = line.PK;

			DocTransactionLine docLine = DocTransactionLine.New(line, Factory);
			AssertNotNull(docLine.RelatedCharge);
			AssertEquals("APS", docLine.ChargeCurrency);

			ARInvoice aRinvoice = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoiceLine aRline = (ARInvoiceLine)aRinvoice.Lines.AddNew();
			currency = Factory.New<RefCurrency>();
			currency.RX_Code = "ARS";
			charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_RX_NKSellCurrency = currency.RX_Code;
			charge.JR_AL_ARLine = aRline.PK;

			docLine = DocTransactionLine.New(aRline, Factory);
			AssertNotNull(docLine.RelatedCharge);
			AssertEquals("ARS", docLine.ChargeCurrency);
		}

		public void TestTransactionHeaderCurrency()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			DocTransactionLine docLine = DocTransactionLine.New(line, Factory);
			AssertEquals("AUD", docLine.TransactionHeaderCurrency);

			invoice.AH_RX_NKTransactionCurrency = "USD";
			AssertEquals("USD", docLine.TransactionHeaderCurrency);
		}

		public void TestMatchedAmountInInvoiceCurrency()
			=> AssertMatchedAmountInInvoiceCurrency(isEnableNewOSOutstandingAmountFeature: false);

		public void TestMatchedAmountInInvoiceCurrency_EnableNewOSOutstandingAmountFeature()
			=> AssertMatchedAmountInInvoiceCurrency(isEnableNewOSOutstandingAmountFeature: true);

		void AssertMatchedAmountInInvoiceCurrency(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();

			DocTransactionLine docLine = DocTransactionLine.New(line, Factory);
			docLine.MatchedOSAmount = 123m;

			AssertEquals("PreCondition CurrentCompany Currency", TestObjectCreator.AUD.Code, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertCase(isLineCurrencySameToInvoice: true);
			AssertCase(isLineCurrencySameToInvoice: false);

			void AssertCase(bool isLineCurrencySameToInvoice)
			{
				invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.Code;
				line.AL_RX_NKTransactionCurrency = isLineCurrencySameToInvoice
					? TestObjectCreator.AUD.Code
					: TestObjectCreator.GBP.Code;
				docLine.MatchedAmount = 10m;
				AssertMatchedAmountInInvoiceCurrency(
					$"{GetSettingsInfo()}Case for local currency.",
					isEnableNewOSOutstandingAmountFeature && isLineCurrencySameToInvoice
						? 123m
						: 10m
				);

				invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
				line.AL_RX_NKTransactionCurrency = isLineCurrencySameToInvoice
					? TestObjectCreator.USD.Code
					: TestObjectCreator.GBP.Code;
				line.AL_LineAmount = 400m;
				line.AL_GSTVAT = 44m;
				line.AL_OSAmount = 333m;
				line.AL_ExchangeRate = 333m / 444m;
				docLine.MatchedAmount = 444m;
				AssertMatchedAmountInInvoiceCurrency(
					$"{GetSettingsInfo()}Case for fully paid.",
					isEnableNewOSOutstandingAmountFeature && isLineCurrencySameToInvoice
						? 123m
						: 333m
				);

				docLine.MatchedAmount = 222m;
				AssertMatchedAmountInInvoiceCurrency(
					$"{GetSettingsInfo()}Case for partialy paid.",
					isEnableNewOSOutstandingAmountFeature && isLineCurrencySameToInvoice
						? 123m
						: 166.5m
				);
			}

			string GetSettingsInfo()
				=> $"[EnableNewOSOutstandingAmountFeature:{isEnableNewOSOutstandingAmountFeature}][Invoice Currency:{invoice.AH_RX_NKTransactionCurrency}][Line Currency:{line.AL_RX_NKTransactionCurrency}]";

			void AssertMatchedAmountInInvoiceCurrency(string comment, decimal expectedResult)
			{
				CombineAssertions(comment, () =>
				{
					AssertEquals("MatchedAmountInInvoiceCurrency", expectedResult, docLine.MatchedAmountInInvoiceCurrency);
					AssertEquals("InvertedMatchedAmountInInvoiceCurrency", -expectedResult, docLine.InvertedMatchedAmountInInvoiceCurrency);
				});
			}
		}

		public void TestMatchedAmountInJobChargeCurrency()
			=> AssertMatchedAmountInJobChargeCurrency(isEnableNewOSOutstandingAmountFeature: false);

		public void TestMatchedAmountInJobChargeCurrency_EnableNewOSOutstandingAmountFeature()
			=> AssertMatchedAmountInJobChargeCurrency(isEnableNewOSOutstandingAmountFeature: true);

		void AssertMatchedAmountInJobChargeCurrency(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			AssertPayableInvoice(isLineCurrencySameToJobCharge: false);
			AssertPayableInvoice(isLineCurrencySameToJobCharge: true);
			AssertReceivableInvoice(isLineCurrencySameToJobCharge: false);
			AssertReceivableInvoice(isLineCurrencySameToJobCharge: true);

			void AssertPayableInvoice(bool isLineCurrencySameToJobCharge)
			{
				APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
				APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
				Charge charge = Factory.NewWithValidTestData<Charge>();
				charge.JR_AL_APLine = line.PK;

				DocTransactionLine docLine = DocTransactionLine.New(line, Factory);
				docLine.MatchedOSAmount = -123m;
				AssertNotNull(docLine.RelatedCharge);

				AssertEquals("PreCondition CurrentCompany Currency", TestObjectCreator.AUD.Code, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				charge.JR_RX_NKCostCurrency = TestObjectCreator.AUD.Code;
				line.AL_RX_NKTransactionCurrency = isLineCurrencySameToJobCharge
					? TestObjectCreator.AUD.Code
					: TestObjectCreator.GBP.Code;
				docLine.MatchedAmount = -10m;
				AssertMatchedAmountInJobChargeCurrency(
					$"{GetSettingsInfo(invoice, line)}[Cost]Case for local currency.",
					docLine,
					isEnableNewOSOutstandingAmountFeature && isLineCurrencySameToJobCharge
						? -123m
						: -10m
				);

				charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.Code;
				line.AL_RX_NKTransactionCurrency = isLineCurrencySameToJobCharge
					? TestObjectCreator.USD.Code
					: TestObjectCreator.GBP.Code;
				charge.JR_OSCostExRate = 0.75m;
				charge.JR_OSCostAmt = 300m;
				charge.JR_IsCostTaxAmountOverridden = true;
				charge.JR_AT_CostGSTRate = TestObjectCreator.GST11.PK;
				line.AL_LineAmount = -400m;
				line.AL_GSTVAT = -44m;

				AssertEquals("PreCondition JobCharge OS Cost Totoal Amt", 333m, charge.JR_Calc_OSCostAmtWithGST);

				docLine.MatchedAmount = -444m;
				AssertMatchedAmountInJobChargeCurrency(
					$"{GetSettingsInfo(invoice, line)}[Cost]Case for fully paid.",
					docLine,
					isEnableNewOSOutstandingAmountFeature && isLineCurrencySameToJobCharge
						? -123m
						: -333m
				);

				docLine.MatchedAmount = -222m;
				AssertMatchedAmountInJobChargeCurrency(
					$"{GetSettingsInfo(invoice, line)}[Cost]Case for partialy paid.",
					docLine,
					isEnableNewOSOutstandingAmountFeature && isLineCurrencySameToJobCharge
						? -123m
						: -166.5m
				);
			}

			void AssertReceivableInvoice(bool isLineCurrencySameToJobCharge)
			{
				ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
				ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();
				Charge charge = Factory.NewWithValidTestData<Charge>();
				charge.JR_AL_ARLine = line.PK;

				var docLine = DocTransactionLine.New(line, Factory);
				docLine.MatchedOSAmount = 123m;
				AssertNotNull(docLine.RelatedCharge);

				AssertEquals("PreCondition CurrentCompany Currency", TestObjectCreator.AUD.Code, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.Code;
				line.AL_RX_NKTransactionCurrency = isLineCurrencySameToJobCharge
					? TestObjectCreator.AUD.Code
					: TestObjectCreator.GBP.Code;
				docLine.MatchedAmount = 10m;
				AssertMatchedAmountInJobChargeCurrency(
					$"{GetSettingsInfo(invoice, line)}[Sell]Case for local currency.",
					docLine,
					isEnableNewOSOutstandingAmountFeature && isLineCurrencySameToJobCharge
						? 123m
						: 10m
				);

				charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.Code;
				line.AL_RX_NKTransactionCurrency = isLineCurrencySameToJobCharge
					? TestObjectCreator.USD.Code
					: TestObjectCreator.GBP.Code;
				charge.JR_OSSellExRate = 0.75m;
				charge.JR_OSSellAmt = 300m;
				charge.JR_AT_SellGSTRate = new TestObjectCreator(Factory).GST11.PK;
				line.AL_LineAmount = 400m;
				line.AL_GSTVAT = 44m;
				line.AL_OSAmount = 333m;

				AssertEquals("PreCondition JobCharge OS Sell Totoal Amt", 333m, charge.JR_Calc_OSSellAmtWithGST);

				docLine.MatchedAmount = 444m;
				AssertMatchedAmountInJobChargeCurrency(
					$"{GetSettingsInfo(invoice, line)}[Sell]Case for fully paid.",
					docLine,
					isEnableNewOSOutstandingAmountFeature && isLineCurrencySameToJobCharge
						? 123m
						: 333m
				);

				docLine.MatchedAmount = 222m;
				AssertMatchedAmountInJobChargeCurrency(
					$"{GetSettingsInfo(invoice, line)}[Sell]Case for partialy paid.",
					docLine,
					isEnableNewOSOutstandingAmountFeature && isLineCurrencySameToJobCharge
						? 123m
						: 166.5m
				);
			}
			string GetSettingsInfo(TransactionHeader invoice, TransactionLine line)
				=> $"[EnableNewOSOutstandingAmountFeature:{isEnableNewOSOutstandingAmountFeature}][Invoice Currency:{invoice.AH_RX_NKTransactionCurrency}][Line Currency:{line.AL_RX_NKTransactionCurrency}]";

			void AssertMatchedAmountInJobChargeCurrency(string comment, DocTransactionLine docLine, decimal expectedResult)
			{
				CombineAssertions(comment, () =>
				{
					AssertEquals("MatchedAmountInInvoiceCurrency", expectedResult, docLine.MatchedAmountInJobChargeCurrency);
					AssertEquals("InvertedMatchedAmountInInvoiceCurrency", -expectedResult, docLine.InvertedMatchedAmountInJobChargeCurrency);
				});
			}
		}

		public void TestChargeDescription()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			Charge charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_Desc = "Planet Express Delivery";
			charge.JR_AL_APLine = line.PK;

			DocTransactionLine docLine = DocTransactionLine.New(line, Factory);
			AssertNotNull(docLine.RelatedCharge);
			AssertEquals("Planet Express Delivery", docLine.ChargeDescription);
		}

		public void TestRelatedCharge()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			BaseCharge charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_AL_APLine = line.PK;

			DocTransactionLine docLine = DocTransactionLine.New(line, Factory);
			AssertNotNull(docLine.RelatedCharge);
			AssertEquals(charge, docLine.RelatedCharge.WrappedObject);
		}

		public void TestJobNumber()
		{
			AssertEquals("", LineWrapper.JobNumber);
			var header = Factory.NewJobForTesting<JobHeader>();
			header.JH_JobNum = "1234";
			Line.AL_JH = header.PK;
			AssertEquals("1234", LineWrapper.JobNumber);
		}

		public void TestMatchedAmount()
		{
			LineWrapper.MatchedAmount = 500m;
			AssertEquals(500M, LineWrapper.MatchedAmount);
		}

		public void TestDPYOSAmount()
		{
			var line = Factory.New<DirectPaymentLine>();
			line.AL_OSAmount = -3000M;
			LineWrapper = DocTransactionLine.New(line, Factory);
			AssertEquals(3000M, LineWrapper.DPYOSAmount);
		}

		public void TestDPYLocalAmountAndTax()
		{
			var line = Factory.New<DirectPaymentLine>();
			line.AL_LocalExTaxAmount = 300M;
			line.AL_LocalTaxAmount = 30M;
			LineWrapper = DocTransactionLine.New(line, Factory);
			AssertEquals(330M, LineWrapper.DPYLocalAmountAndTax);
		}

		public void TestDPYOSAmountAndDPYLocalAmountAndTaxAreTheSameSign()
		{
			var line = Factory.New<DirectPaymentLine>();

			line.AL_OSExTaxAmount = 300M;
			line.AL_OSTaxAmount = 30M;
			line.AL_LocalExTaxAmount = 300M;
			line.AL_LocalTaxAmount = 30M;

			LineWrapper = DocTransactionLine.New(line, Factory);
			AssertEquals(330M, LineWrapper.DPYOSAmount);
			AssertEquals(330M, LineWrapper.DPYLocalAmountAndTax);
		}

		public void TestDebitAmount()
		{
			DebitCreditAmounts.LocalUnsignedLineAmount = 1000.00M;
			DebitCreditAmounts.DebitCreditSign = DebitCreditDataEntry.DR;
			AssertEquals("1,000.00", LineWrapper.DebitAmount);
			AssertEquals(1000M, LineWrapper.DebitAmountDecimal);

			DebitCreditAmounts.DebitCreditSign = DebitCreditDataEntry.CR;
			AssertEquals("", LineWrapper.DebitAmount);
			AssertEquals(0M, LineWrapper.DebitAmountDecimal);
		}

		public void TestForeignCurrencyEquivalent()
		{
			DebitCreditAmounts.OSUnsignedLineAmount = 1000.00M;
			DebitCreditAmounts.DebitCreditSign = DebitCreditDataEntry.DR;
			AssertEquals("1,000.00 DR", LineWrapper.ForeignCurrencyEquivalent);

			DebitCreditAmounts.DebitCreditSign = DebitCreditDataEntry.CR;
			AssertEquals("1,000.00 CR", LineWrapper.ForeignCurrencyEquivalent);

			DebitCreditAmounts.OSUnsignedLineAmount = 0M;
			AssertEquals("", LineWrapper.ForeignCurrencyEquivalent);

			DebitCreditAmounts.OSUnsignedLineAmount = 1200M;
			AssertEquals("1,200.00 CR", LineWrapper.ForeignCurrencyEquivalent);

			AssertEquals("Precondition: TransactionHeader is empty", string.Empty, LineWrapper.TransactionHeaderCurrency);
			AssertNotEquals("Precondition: Line currency and transaction header currency is not the same", LineWrapper.TransactionHeaderCurrency, Line.AL_RX_NKTransactionCurrency);

			Line.AL_RX_NKTransactionCurrency = string.Empty;
			AssertEquals("Line currency and transaction header currency is now same", LineWrapper.TransactionHeaderCurrency, Line.AL_RX_NKTransactionCurrency);
			AssertEquals("", LineWrapper.ForeignCurrencyEquivalent);
		}

		public void TestCreditAmount()
		{
			DebitCreditAmounts.LocalUnsignedLineAmount = 1000.22M;
			DebitCreditAmounts.DebitCreditSign = DebitCreditDataEntry.CR;
			AssertEquals("1,000.22", LineWrapper.CreditAmount);
			AssertEquals(1000.22M, LineWrapper.CreditAmountDecimal);

			DebitCreditAmounts.DebitCreditSign = DebitCreditDataEntry.DR;
			AssertEquals("", LineWrapper.CreditAmount);
			AssertEquals(0M, LineWrapper.CreditAmountDecimal);
		}

		public void TestDebitCreditSign()
		{
			DebitCreditAmounts.DebitCreditSign = DebitCreditDataEntry.CR;
			AssertEquals("CR", LineWrapper.DebitCreditSign);

			DebitCreditAmounts.DebitCreditSign = DebitCreditDataEntry.DR;
			AssertEquals("DR", LineWrapper.DebitCreditSign);
		}

		public virtual void TestOSUnsignedAmount()
		{
			DebitCreditAmounts.OSUnsignedLineAmount = 1000.22M;
			AssertEquals("1,000.22", LineWrapper.OSUnsignedAmount);
		}

		public virtual void TestLocalUnsignedAmount()
		{
			DebitCreditAmounts.LocalUnsignedLineAmount = 1000.22M;
			AssertEquals("1,000.22", LineWrapper.LocalUnsignedAmount);
		}

		public void TestLocalExTaxAmount()
		{
			Line.AL_LocalExTaxAmount = 1234.93M;
			AssertEquals(1234.93M, LineWrapper.LocalExTaxAmount);
		}

		public void TestLocalTaxAmount()
		{
			Line.AL_LocalTaxAmount = 2000.50M;
			AssertEquals(2000.50M, LineWrapper.LocalTaxAmount);
		}

		public void TestOSExTaxAmount()
		{
			Line.AL_OSExTaxAmount = 500.34M;
			AssertEquals(500.34M, LineWrapper.OSExTaxAmount);
		}

		public void TestOSTaxAmount()
		{
			Line.AL_OSTaxAmount = 234.5M;
			AssertEquals(234.5M, LineWrapper.OSTaxAmount);
		}

		public void TestOverseasTotal()
		{
			Line.AL_OverseasTotal = 120.99M;
			AssertEquals(120.99M, LineWrapper.OverseasTotal);
		}

		public void TestChargeCode()
		{
			AssertNull(LineWrapper.ChargeCode);
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "CODE";
			Line.AL_AC = chargeCode.PK;
			AssertEquals("CODE", LineWrapper.ChargeCode.Code);
		}

		public void TestGLAccount()
		{
			AssertNull("glaccount", LineWrapper.GLAccount);
			Line.AL_AG = Factory.New(typeof(AccGLHeader)).PK;
			AssertNotNull("glaccount", LineWrapper.GLAccount);
		}

		public void TestPercentOf()
		{
			AssertNull("glaccount", LineWrapper.PercentOf);
			Line.AL_AG_PercentOf = Factory.New(typeof(AccGLHeader)).PK;
			AssertNotNull("glaccount", LineWrapper.PercentOf);
		}

		public void TestInvoice()
		{
			var journal = Factory.New<GLJournal>();
			journal.AH_Desc = "TEST";
			Line.AL_AH = journal.PK;
			AssertEquals("TEST", LineWrapper.Invoice.Desc);
		}

		public void TestTaxRate()
		{
			var taxRate = Factory.New<AccTaxRate>();
			taxRate.AT_Code = "TCODE";
			Line.AL_AT = taxRate.PK;
			AssertEquals("TCODE", LineWrapper.TaxRate.Code);
		}

		public void TestWithholdingTaxRate()
		{
			var withHolding = Factory.New<AccWithholding>();
			withHolding.AW_Code = "WCODE";
			Line.AL_AW = withHolding.PK;
			AssertEquals("WCODE", LineWrapper.WithholdingTaxRate.Code);
		}

		public void TestDesc()
		{
			Line.AL_Desc = "DESCIPTION";
			AssertEquals("DESCIPTION", LineWrapper.Description);
		}

		public virtual void TestExchangeRate()
		{
			Line.AL_ExchangeRate = 0.7890M;
			AssertEquals(0.7890M, LineWrapper.ExchangeRate);

			AccTransactionHeader header = Factory.New<AccTransactionHeader>();
			header.AH_ExchangeRate = 0.7899M;
			Line.AL_AH = header.PK;
			AssertEquals(0.7899M, LineWrapper.ExchangeRate);
		}

		public void TestBranch()
		{
			Line.AL_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbBranch.CurrentBranch.GB_BranchName, LineWrapper.Branch.BranchName);
		}

		public void TestDepartment()
		{
			Line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			AssertEquals(GlbDepartment.CurrentDepartment.GE_Code, LineWrapper.Department.Code);
		}

		public void TestGSTVAT()
		{
			Line.AL_GSTVAT = 0.4444M;
			AssertEquals(0.44M, LineWrapper.GSTVAT);
		}

		public void TestJob()
		{
			var header = Factory.NewJobForTesting<JobHeader>();
			header.JH_JobNum = "1234";
			Line.AL_JH = header.PK;
			AssertEquals("1234", LineWrapper.JobHeader.JobNumber);
		}

		public void TestLineAmount()
		{
			Line.AL_LineAmount = 0.02M;
			AssertEquals(0.02M, LineWrapper.LineAmount);
		}

		public void TestLineType()
		{
			Line.AL_LineType = "TST";
			AssertEquals("TST", LineWrapper.LineType);
		}

		public void TestOrganisation()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";
			Line.AL_OH = org.PK;
			AssertEquals("TESTORG", LineWrapper.Organisation.Code);
		}

		public void TestOSAmount()
		{
			Line.AL_OSAmount = 1200M;
			AssertEquals(1200M, LineWrapper.OSAmount);
		}

		public void TestOSUnitPrice()
		{
			Line.AL_OSUnitPrice = 1.200M;
			AssertEquals(1.200M, LineWrapper.OSUnitPrice);
		}

		public void TestPercentageOfPeriod()
		{
			Line.AL_PercentageOfPeriod = 3;
			AssertEquals(3, LineWrapper.PercentageOfPeriod);
		}

		public void TestPostDate()
		{
			Line.AL_PostDate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, LineWrapper.PostDate);
		}

		public void TestPostPeriod()
		{
			Line.AL_PostPeriod = 200412;
			AssertEquals(200412, LineWrapper.PostPeriod);
		}

		public void TestPostToGL()
		{
			Line.AL_PostToGL = "N";
			AssertEquals(ZBool.False, LineWrapper.PostToGL);
			Line.AL_PostToGL = "Y";
			Assert(LineWrapper.PostToGL);
			Line.AL_PostToGL = "M";
			Assert(!LineWrapper.PostToGL);
		}

		public void TestPreventInvoicePrintGrouping()
		{
			Line.AL_PreventInvoicePrintGrouping = ZBool.False;
			AssertEquals(ZBool.False, LineWrapper.PreventInvoicePrintGrouping);
			Line.AL_PreventInvoicePrintGrouping = ZBool.True;
			Assert(LineWrapper.PreventInvoicePrintGrouping);
		}

		public void TestReverseDate()
		{
			Line.AL_ReverseDate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, LineWrapper.ReverseDate);
		}

		public void TestReversePeriod()
		{
			Line.AL_ReversePeriod = 200512;
			AssertEquals(200512, LineWrapper.ReversePeriod);
		}

		public void TestReverseToGL()
		{
			Line.AL_ReverseToGL = "N";
			AssertEquals(ZBool.False, LineWrapper.ReverseToGL);
			Line.AL_ReverseToGL = "Y";
			Assert(LineWrapper.ReverseToGL);
			Line.AL_ReverseToGL = "M";
			Assert(!LineWrapper.ReverseToGL);
		}

		public void TestCurrency()
		{
			var currency = Factory.New<RefCurrency>();
			currency.RX_Code = "XAS";
			Line.AL_RX_NKTransactionCurrency = currency.RX_Code;
			AssertEquals("XAS", LineWrapper.Currency.Code);
		}

		public void TestSequence()
		{
			Line.AL_Sequence = ZShort.Parse("23");
			AssertEquals("23", LineWrapper.Sequence.ToString());
		}

		public void TestUnitPrice()
		{
			Line.AL_UnitPrice = 230M;
			AssertEquals(230M, LineWrapper.UnitPrice);
		}

		public void TestUnitQty()
		{
			Line.AL_UnitQty = 20;
			AssertEquals(20, LineWrapper.UnitQty);
		}

		public void TestWithholdingTax()
		{
			Line.AL_WithholdingTax = 70.5M;
			AssertEquals(70.5M, LineWrapper.WithholdingTax);
		}

		public void TestLocalTotalAmount()
		{
			Line.AL_LocalExTaxAmount = 100M;
			Line.AL_LocalTaxAmount = 10M;
			AssertEquals(110M, Line.AL_LocalTotalAmount);
			AssertEquals(110M, LineWrapper.LocalTotalAmount);
		}

		protected TransactionLine Line;
		protected IDebitCreditAmounts DebitCreditAmounts
		{
			get { return Line as IDebitCreditAmounts; }
		}
		protected DocTransactionLine LineWrapper;

		protected virtual TransactionLine GetLine()
		{
			return Factory.New<GLJournalLine>();
		}

		protected override void SetUp()
		{
			Line = GetLine();
			LineWrapper = GetLineWrapper();
			AssertNotNull(LineWrapper);
			base.SetUp();
		}

		protected virtual DocTransactionLine GetLineWrapper()
		{
			return (DocTransactionLine)GetDocumentWrappers()[0];
		}
	}

	[TestedType(typeof(DocTransactionLine))]
	public class TestDocTransactionLineForJobRevenueJournal : DocTransactionLineTest
	{
		public override void TestOSUnsignedAmount()
		{
			Line.AL_ExchangeRate = 2M;
			DebitCreditAmounts.OSUnsignedLineAmount = 1000.22M;
			AssertNotEquals("Precondition: to check that is real OSUnsignedLineAmount", DebitCreditAmounts.OSUnsignedLineAmount, DebitCreditAmounts.LocalUnsignedLineAmount);
			AssertEquals("1,000.22", LineWrapper.OSUnsignedAmount);
		}

		public override void TestLocalUnsignedAmount()
		{
			Line.AL_ExchangeRate = 2M;
			DebitCreditAmounts.LocalUnsignedLineAmount = 1000.22M;
			AssertNotEquals("Precondition: to check that is real LocalUnsignedAmount", DebitCreditAmounts.OSUnsignedLineAmount, DebitCreditAmounts.LocalUnsignedLineAmount);
			AssertEquals("1,000.22", LineWrapper.LocalUnsignedAmount);
		}

		protected override TransactionLine GetLine()
		{
			return Factory.New<JobRevenueJournalLine>();
		}
	}
}
