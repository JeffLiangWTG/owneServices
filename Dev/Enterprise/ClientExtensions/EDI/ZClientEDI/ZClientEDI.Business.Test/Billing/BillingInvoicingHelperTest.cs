using System;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class BillingInvoicingHelperTest : TestCaseWithFactory
	{
		public void TestGetChargeCodePK()
		{
			AccTaxRate gstTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, 10);
			AccChargeCode chargeCode = BillingTestHelper.CreateChargeCode(Factory, gstTaxRate, "AAA");

			GlbCompany anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch anotherBranch = anotherCompany.Branches.AddNew();
			AccChargeCode anotherChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, anotherBranch, gstTaxRate, "BBB");

			Factory.Save();

			GlbBranch currentBranch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			AssertEquals(chargeCode.PK, BillingInvoicingHelper.GetChargeCodePK(currentBranch, "AAA"));
			AssertEquals(anotherChargeCode.PK, BillingInvoicingHelper.GetChargeCodePK(anotherBranch, "BBB"));

			AssertExceptionThrown("No charge code for the branch", typeof(ArgumentException), delegate
			{
				BillingInvoicingHelper.GetChargeCodePK(currentBranch, "XXX");
			});

			AssertExceptionThrown("No charge code for the branch", typeof(ArgumentException), delegate
			{
				BillingInvoicingHelper.GetChargeCodePK(currentBranch, "BBB");
			});

			AssertExceptionThrown("No charge code for the branch", typeof(ArgumentException), delegate
			{
				BillingInvoicingHelper.GetChargeCodePK(anotherBranch, "AAA");
			});
		}

		public void TestGetAmountInInvoiceCurrency()
		{
			ZDecimal amount = 100m;
			var dateForExchangeRate = ZDateTime.Now;

			AssertEquals("Pricelist currency is empty", 0m, BillingInvoicingHelper.GetAmountInInvoiceCurrency(amount, dateForExchangeRate, "", InvoiceCurrencyNK, InvoicingBranch, InvoicingBranch.Factory));
			AssertEquals("Invoice currency is empty", 0m, BillingInvoicingHelper.GetAmountInInvoiceCurrency(amount, dateForExchangeRate, PricelistCurrencyNK, "", InvoicingBranch, InvoicingBranch.Factory));
			AssertEquals("Pricelist currency does not exist", 0m, BillingInvoicingHelper.GetAmountInInvoiceCurrency(amount, dateForExchangeRate, "XXX", InvoiceCurrencyNK, InvoicingBranch, InvoicingBranch.Factory));
			AssertEquals("Invoice currency does not exist", 0m, BillingInvoicingHelper.GetAmountInInvoiceCurrency(amount, dateForExchangeRate, PricelistCurrencyNK, "XXX", InvoicingBranch, InvoicingBranch.Factory));
			AssertEquals("Invoicing branch is null", 0m, BillingInvoicingHelper.GetAmountInInvoiceCurrency(amount, dateForExchangeRate, PricelistCurrencyNK, InvoiceCurrencyNK, null, InvoicingBranch.Factory));

			AssertEquals("Same currency", amount, BillingInvoicingHelper.GetAmountInInvoiceCurrency(amount, dateForExchangeRate, PricelistCurrencyNK, PricelistCurrencyNK, InvoicingBranch, InvoicingBranch.Factory));
			AssertEquals("Currency converted", amount * 4, BillingInvoicingHelper.GetAmountInInvoiceCurrency(amount, dateForExchangeRate, PricelistCurrencyNK, InvoiceCurrencyNK, InvoicingBranch, InvoicingBranch.Factory));

			RefCurrency invoiceCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, InvoiceCurrencyNK);
			invoiceCurrency.ExchangeRates.Last().RE_SellRate = 16;
			AssertEquals("Currency converted", amount * 8, BillingInvoicingHelper.GetAmountInInvoiceCurrency(amount, dateForExchangeRate, PricelistCurrencyNK, InvoiceCurrencyNK, InvoicingBranch, InvoicingBranch.Factory));
		}

		public void TestGetDateForExchangeRate()
		{
			var gstTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var chargeCode = BillingTestHelper.CreateChargeCode(Factory, gstTaxRate, "AAA");

			var collectionAR = new InvoicePostingExRateOptionCollection();
			collectionAR.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign,
				InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code, 1));
			collectionAR.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local,
				InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code, 0));
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionAR);

			Factory.Save();

			var postDate = ZDateTime.Today.AddDays(-5);
			var invoiceDate = ZDateTime.Today.AddDays(-10);
			var taxDate = ZDate.Today.AddDays(-15);

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_RX_NKTransactionCurrency = invoice.Company.GC_RX_NKLocalCurrency;
			AddAmountLineAndAssertResult(invoice, 200m, chargeCode.AC_Code, gstTaxRate, "newLine");
			invoice.AH_PostDate = postDate;
			invoice.AH_InvoiceDate = invoiceDate;
			invoice.Lines[0].AL_TaxDate = taxDate;
			AssertEquals(taxDate, invoice.InvoiceTaxDate);

			AssertEquals("AUD is local currency", Core.Constants.CurrencyCodes.Australia, invoice.Company.GC_RX_NKLocalCurrency);
			Assert(invoice.IsLocalCurrencyTransaction);
			AssertEquals(postDate, BillingInvoicingHelper.GetDateForExchangeRate(invoice));

			invoice.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			Assert(!invoice.IsLocalCurrencyTransaction);
			AssertEquals(invoiceDate.AddDays(1), BillingInvoicingHelper.GetDateForExchangeRate(invoice));

			AssertEquals("returns the date parameter and use the registry offset value, local offset is zero",
				postDate, BillingInvoicingHelper.GetDateForExchangeRate(postDate, true, invoice.AH_GC));
			AssertEquals("returns the date parameter and use the registry offset value, local offset is 1",
				invoiceDate.AddDays(1), BillingInvoicingHelper.GetDateForExchangeRate(invoiceDate, false, invoice.AH_GC));

			collectionAR = new InvoicePostingExRateOptionCollection();
			collectionAR.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign,
				InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code, -2));
			collectionAR.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local,
				InvoicePostingExchangeRateOption.TodayExchangeRate.Code, 0));
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionAR);

			AssertEquals(taxDate.AddDays(-2), BillingInvoicingHelper.GetDateForExchangeRate(invoice));

			invoice.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			Assert(invoice.IsLocalCurrencyTransaction);
			AssertEquals(ZDateTime.Now, BillingInvoicingHelper.GetDateForExchangeRate(invoice));
		}

		public void TestGetExchangeRate()
		{
			var dateForExchangeRate = ZDateTime.Now;
			AssertEquals("Exchange rate", 4m, BillingInvoicingHelper.GetExchangeRate(dateForExchangeRate, PricelistCurrencyNK, InvoiceCurrencyNK, InvoicingBranch));

			RefCurrency invoiceCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, InvoiceCurrencyNK);
			invoiceCurrency.ExchangeRates.Last().RE_SellRate = 10.24;
			AssertEquals("Exchange rate", new ZDecimal(10.24 / 2), BillingInvoicingHelper.GetExchangeRate(dateForExchangeRate, PricelistCurrencyNK, InvoiceCurrencyNK, InvoicingBranch));

			AssertEquals("Exchange rate not found", 0m, BillingInvoicingHelper.GetExchangeRate(dateForExchangeRate, PricelistCurrencyNK, "XXX", InvoicingBranch));
			AssertEquals("Exchange rate not found", 0m, BillingInvoicingHelper.GetExchangeRate(dateForExchangeRate, "XXX", InvoiceCurrencyNK, InvoicingBranch));
		}

		public void TestExchangeRateMatchesAmountInInvoice()
		{
			RefCurrency usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			foreach (var rate in usd.ExchangeRates)
			{
				rate.RE_SellRate = 0.794m;
			}

			RefCurrency hkd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.HongKong);
			RefExchangeRate hkdExchange = hkd.ExchangeRates.AddNew();
			hkdExchange.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			hkdExchange.RE_SellRate = 5.637m;
			hkdExchange.RE_StartDate = ZDateTime.Today.AddMonths(-1);
			hkdExchange.RE_ExpiryDate = ZDateTime.Today.AddMonths(1);

			Factory.Save();

			ZDecimal amount = 157.70m;
			var dateForExchangeRate = ZDateTime.Now;

			var exchangeRate = BillingInvoicingHelper.GetExchangeRate(dateForExchangeRate, Core.Constants.CurrencyCodes.UnitedStates, Core.Constants.CurrencyCodes.Australia, InvoicingBranch, InvoicingBranch.Factory);
			var invoiceAmount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(amount, dateForExchangeRate, Core.Constants.CurrencyCodes.UnitedStates, Core.Constants.CurrencyCodes.Australia, InvoicingBranch, InvoicingBranch.Factory);
			AssertEquals("Should be 1 / 0.794", 1.25945m, exchangeRate);
			AssertEquals("Should be 157.70 * 1 / 0.794", 198.62m, invoiceAmount);

			exchangeRate = BillingInvoicingHelper.GetExchangeRate(dateForExchangeRate, Core.Constants.CurrencyCodes.UnitedStates, Core.Constants.CurrencyCodes.HongKong, InvoicingBranch, InvoicingBranch.Factory);
			invoiceAmount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(amount, dateForExchangeRate, Core.Constants.CurrencyCodes.UnitedStates, Core.Constants.CurrencyCodes.HongKong, InvoicingBranch, InvoicingBranch.Factory);
			AssertEquals("Should be 5.637 / 0.794", 7.09950m, exchangeRate);
			AssertEquals("Should be 157.70 * 5.637 / 0.794", 1119.59m, invoiceAmount);
		}

		public void TestAddAmountLine()
		{
			AccTaxRate gstTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, 10);
			AccChargeCode chargeCode = BillingTestHelper.CreateChargeCode(Factory, gstTaxRate, "AAA");
			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = "DP1";
			chargeCode.AC_DepartmentFilterList = "DP1";
			Factory.Save();

			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_RX_NKTransactionCurrency = "AUD";
			invoice.AH_GB = Env.CurrentBranch.PK;
			invoice.AH_GC = Env.CurrentBranch.CompanyPK;

			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{
				BillingInvoicingHelper.AddAmountLine(null, 100m, chargeCode.AC_Code, null, "hello");
			});

			AddAmountLineAndAssertResult(invoice, 100m, chargeCode.AC_Code, null, "hello");

			AccTaxRate newTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "NEW", Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, 10);
			AddAmountLineAndAssertResult(invoice, 200m, chargeCode.AC_Code, newTaxRate, "world");
		}

		public void TestAddAmountLine_WithCurrencyConversion()
		{
			AccTaxRate gstTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, 10);
			AccChargeCode chargeCode = BillingTestHelper.CreateChargeCode(Factory, gstTaxRate, "AAA");
			Factory.Save();

			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_RX_NKTransactionCurrency = InvoiceCurrencyNK;
			invoice.AH_GB = Env.CurrentBranch.PK;
			invoice.AH_GC = Env.CurrentBranch.CompanyPK;

			AssertEquals("Precondition", 0, invoice.Lines.Count);
			BillingInvoicingHelper.AddAmountLine(invoice, 100m, ZDateTime.Now, chargeCode.AC_Code, PricelistCurrencyNK, null, "hello");

			AddAmountLineAndAssertResult(invoice, 100m, chargeCode.AC_Code, null, "hello");
			ARInvoiceLine amountLine = invoice.Lines[0] as ARInvoiceLine;
			AssertAmountLine(amountLine, 400m, chargeCode.AC_Code, null, "hello");
		}

		void AddAmountLineAndAssertResult(ARInvoice invoice, ZDecimal amount, ZString amountChargeCode, AccTaxRate taxRate, ZString description)
		{
			ZGuid[] linesPKsBeforeAddingHosting = Array.ConvertAll(invoice.Lines.ToArray(), x => x.PK);
			BillingInvoicingHelper.AddAmountLine(invoice, amount, amountChargeCode, taxRate, description);

			AssertEquals("One line was added", linesPKsBeforeAddingHosting.Length + 1, invoice.Lines.Count);
			ARInvoiceLine amountLine = invoice.Lines.FirstOrDefault(x => !linesPKsBeforeAddingHosting.Contains(x.PK)) as ARInvoiceLine;
			AssertAmountLine(amountLine, amount, amountChargeCode, taxRate, description);
		}

		void AssertAmountLine(ARInvoiceLine invoiceLine, ZDecimal amount, ZString amountChargeCodeName, AccTaxRate taxRate, ZString description)
		{
			AssertEquals(amount, invoiceLine.AL_OSExTaxAmount);
			AssertEquals(Enterprise.ZArchitecture.Core.TransactionLineTypes.Revenue, invoiceLine.AL_LineType);
			AssertEquals(BillingInvoicingHelper.GetChargeCodePK(invoiceLine.Invoice.Branch, amountChargeCodeName), invoiceLine.AL_AC);
			AssertEquals(description, invoiceLine.AL_Desc);
			if (invoiceLine.ChargeCode.AC_DepartmentFilterList == "ALL")
			{
				AssertEquals(invoiceLine.Department.GE_Code, Env.CurrentDepartment.Code);
			}
			else
			{
				AssertEquals(invoiceLine.Department.GE_Code, invoiceLine.ChargeCode.AC_DepartmentFilterList);
			}

			if (taxRate != null)
			{
				AssertEquals(taxRate.PK, invoiceLine.AL_AT);
			}
			else if (invoiceLine.ChargeCode.AC_AT_GSTRate.IsValid)
			{
				AssertEquals(invoiceLine.ChargeCode.AC_AT_GSTRate, invoiceLine.AL_AT);
			}
		}

		public void TestAddCommentLine()
		{
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			Factory.Save();

			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_RX_NKTransactionCurrency = "AUD";
			invoice.AH_GB = Env.CurrentBranch.PK;
			invoice.AH_GC = Env.CurrentBranch.CompanyPK;

			BillingInvoicingHelper.AddCommentLine(invoice, "Hello World!");
			AssertEquals("One line added", 1, invoice.Lines.Count);

			ARInvoiceLine commentLine = invoice.Lines[0] as ARInvoiceLine;
			AssertEquals(0m, commentLine.AL_OSExTaxAmount);
			AssertEquals(BillingInvoicingHelper.GetChargeCodePK(invoice.Branch, EDIDataRegistry.Instance.CommentChargeCode.Value), commentLine.AL_AC);
			AssertEquals("Hello World!", commentLine.AL_Desc);
			AssertEquals(ZGuid.Empty, commentLine.AL_AT);
			AssertEquals(ZGuid.Empty, commentLine.AL_AG);

			BillingInvoicingHelper.AddCommentLine(invoice, "");
			AssertEquals("Nothing added", 1, invoice.Lines.Count);

			BillingInvoicingHelper.AddCommentLine(invoice, "  ");
			AssertEquals("Nothing added", 1, invoice.Lines.Count);
		}

		public void TestBranchContext()
		{
			AssertNull("current branch needs no context change", BillingInvoicingHelper.BranchContext(Env.CurrentBranch.PK));
			var originalBranch = Env.CurrentBranch;

			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();
			IDisposable context = BillingInvoicingHelper.BranchContext(branch.PK);
			AssertEquals("current branch", branch.PK, Env.CurrentBranch.PK);
			context.Dispose();
			AssertEquals("original branch", originalBranch.PK, Env.CurrentBranch.PK);
		}

		public void TestSynchronizeChild()
		{
			// Note: Test is in ClientLicenceHeaderExTest
			Assert(true);
		}

		public void TestCreateOdplMinimumFeeRevenueBreakdown()
		{
			var gstTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var mfCharge = BillingTestHelper.CreateChargeCode(Factory, gstTaxRate, "MFCHARGE");
			var npCharge = BillingTestHelper.CreateChargeCode(Factory, gstTaxRate, "NPCHARGE");
			var charge1 = BillingTestHelper.CreateChargeCode(Factory, gstTaxRate, "SALESFEE");
			var charge2 = BillingTestHelper.CreateChargeCode(Factory, gstTaxRate, "DISCODPL");

			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			inv1.AH_RX_NKTransactionCurrency = "USD";
			var startDate = ZDateTime.Now.Date;
			var db1 = ZGuid.NewZGuid();
			var db2 = ZGuid.NewZGuid();

			var priceItemMF = Factory.New<ClientLicencePriceItem>();
			priceItemMF.L7_Code = "#MF";

			var priceItemNP = Factory.New<ClientLicencePriceItem>();
			priceItemNP.L7_Code = "#NP";

			var mf1 = new SystemMinimumFee(db1, startDate, 20m, "AUD", "MFCHARGE", priceItemMF, false);
			var np1 = new SystemMinimumFee(db2, startDate, 50m, "AUD", "NPCHARGE", priceItemNP, true);

			BillingInvoicingHelper.CreateOdplMinimumFeeRevenueBreakdown(new[] { mf1, np1 }, inv1, startDate, 10);
			var billedUsages = Factory.Load<EdiBilledUsage>(new ZQuery()).OrderBy(x => x.BU9_TransactionAmountPostDiscount).ToArray();
			AssertEquals(2, billedUsages.Length);

			CombineAssertions(() =>
			{
				AssertEquals(billedUsages[0].BU9_AC_AmountChargeCode, mfCharge.PK);
				AssertEquals(billedUsages[0].BU9_AC_DiscountChargeCode, charge1.PK);
				AssertEquals(billedUsages[0].BU9_AH_Invoice, inv1.PK);
				AssertEquals(billedUsages[0].BU9_UsageCode, BillingConstants.BillingSystem.ODM);
				AssertEquals(billedUsages[0].BU9_BillingModel, BillingConstants.PriceHeaderType.ODM);
				AssertEquals(billedUsages[0].BU9_UsageSubCode, "#MF");
				AssertEquals(billedUsages[0].BU9_LocalAmountPostDiscount, 22m);
				AssertEquals(billedUsages[0].BU9_LocalAmountPreDiscount, 20m);
				AssertEquals(billedUsages[0].BU9_LocalProcessingAmount, 2m);
				AssertEquals(billedUsages[0].BU9_PeriodStart, startDate);
				AssertEquals(billedUsages[0].BU9_PriceCurrency, "AUD");
				AssertEquals(billedUsages[0].BU9_TransactionAmountPostDiscount, 44m);
				AssertEquals(billedUsages[0].BU9_TransactionAmountPreDiscount, 40m);
				AssertEquals(billedUsages[0].BU9_TransactionProcessingAmount, 4m);
				AssertEquals(billedUsages[0].BU9_LD, db1);
				AssertEquals(billedUsages[0].BU9_L7, priceItemMF.PK);
				AssertEquals(billedUsages[0].BU9_PriceCode, "#MF");
				AssertEquals(billedUsages[0].BU9_UnitCount, 1m);
				AssertEquals(billedUsages[0].BU9_UnitPrice, 20m);

				AssertEquals(billedUsages[1].BU9_AC_AmountChargeCode, npCharge.PK);
				AssertEquals(billedUsages[1].BU9_AC_DiscountChargeCode, charge1.PK);
				AssertEquals(billedUsages[1].BU9_AH_Invoice, inv1.PK);
				AssertEquals(billedUsages[1].BU9_UsageCode, BillingConstants.BillingSystem.ODM);
				AssertEquals(billedUsages[1].BU9_BillingModel, BillingConstants.PriceHeaderType.ODM);
				AssertEquals(billedUsages[1].BU9_UsageSubCode, "#NP");
				AssertEquals(billedUsages[1].BU9_LocalAmountPostDiscount, 55m);
				AssertEquals(billedUsages[1].BU9_LocalAmountPreDiscount, 50m);
				AssertEquals(billedUsages[1].BU9_LocalProcessingAmount, 5m);
				AssertEquals(billedUsages[1].BU9_PeriodStart, startDate);
				AssertEquals(billedUsages[1].BU9_PriceCurrency, "AUD");
				AssertEquals(billedUsages[1].BU9_TransactionAmountPostDiscount, 110m);
				AssertEquals(billedUsages[1].BU9_TransactionAmountPreDiscount, 100m);
				AssertEquals(billedUsages[1].BU9_TransactionProcessingAmount, 10m);
				AssertEquals(billedUsages[1].BU9_LD, db2);
				AssertEquals(billedUsages[1].BU9_L7, priceItemNP.PK);
				AssertEquals(billedUsages[1].BU9_PriceCode, "#NP");
				AssertEquals(billedUsages[1].BU9_UnitCount, 1m);
				AssertEquals(billedUsages[1].BU9_UnitPrice, 50m);
			});
		}

		public void TestGetRawDocument()
		{
			ExceptionReporterTestListener.Instance.Clear();
			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			var docARInvoice = DocARInvoice.New(arInvoice, Factory);
			var arInvoiceTemplate = Factory.LoadTop1<StmTemplate>(new ZQuery(StmTemplateSchema.SO_Name, "ARInvoice"));
			Assert(BillingInvoicingHelper.GetRawDocumentInPdf(arInvoiceTemplate, docARInvoice).Length > 0);
			Assert(BillingInvoicingHelper.GetRawDocumentInExcel(arInvoiceTemplate, docARInvoice).rawDocument.Length > 0);

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var monthlyUsage = StlMonthlyUsageTest.CreateMonthlyUsage(lic, new ZDateTime(2022, 4, 1));
			var bill = new StlBill(Factory, GlbBranch.CurrentBranch, lic.Company.Header, "AUD", new ZDateTime(2022, 4, 1), new ZDateTime(2022, 4, 1));
			bill.LocalExchangeRate = 1;
			bill.AddMonthlyUsages(new[] { monthlyUsage });
			var stlDocWrapper = new DocStlSummaryForTest(monthlyUsage, Factory);

			var mtd = typeof(Report).GetMethod("TemporarilyGenerateRegardlessOfAnyErrors", BindingFlags.Static | BindingFlags.NonPublic);
			ErrorReporter.SuppressReportingOfErrors = true;
			using (new DisposableAction(() => ErrorReporter.SuppressReportingOfErrors = false))
			using (Globals.SetIsUserInteractiveForTest(false))
			using (mtd.Invoke(null, null) as IDisposable)
			using (Report.TemporarilyStopErrorsThrowingAnException())
			{
				var errorMsg = @"BizObj=Enterprise.Client.EDI.Billing.Business.StlMonthlyUsage
StlMonthlyUsage.PeriodStart=01 Apr 2022 00:00
StlMonthlyUsage.Bill.OrganisationCode=AAAAAA
ReportErrors=IsFatal=False|Severity=200|EnumValueName=Warning|Message=Field <Currency.Code> not found on DataSource Type [DocStlSummaryForTest].
IsFatal=False|Severity=200|EnumValueName=Warning|Message=Currency code not found : 
IsFatal=False|Severity=200|EnumValueName=Warning|Message=Field <Cartage.ThirdAddress.CompanyName> not found on DataSource Type [DocStlSummaryForTest].
IsFatal=False|Severity=200|EnumValueName=Warning|Message=Field <Cartage.ContainerLine> not found on DataSource Type [DocStlSummaryForTest].";

				AssertExceptionThrown<ExcelInterfaceException>(() => BillingInvoicingHelper.GetRawDocumentInPdf(arInvoiceTemplate, stlDocWrapper));
				AssertType<InvalidOperationException>(ExceptionReporterTestListener.Instance.Single());
				AssertContains(errorMsg, ExceptionReporterTestListener.Instance.Single().Message);

				ExceptionReporterTestListener.Instance.Clear();
				AssertNoExceptionThrown(() => BillingInvoicingHelper.GetRawDocumentInExcel(arInvoiceTemplate, stlDocWrapper));
				AssertType<InvalidOperationException>(ExceptionReporterTestListener.Instance.Single());
				AssertContains(errorMsg, ExceptionReporterTestListener.Instance.Single().Message);
			}

			ExceptionReporterTestListener.Instance.Clear();
		}

		public class DocStlSummaryForTest : DocBaseWrapper
		{
			public DocStlSummaryForTest(StlMonthlyUsage monthlyUsage, BusinessObjectFactory factory)
				: base(monthlyUsage, factory)
			{
			}
		}

		#region Implementation

		ZString PricelistCurrencyNK;
		ZString InvoiceCurrencyNK;
		GlbBranch InvoicingBranch;

		protected override void SetUp()
		{
			base.SetUp();

			PricelistCurrencyNK = Core.Constants.CurrencyCodes.UnitedStates;
			InvoiceCurrencyNK = Core.Constants.CurrencyCodes.Moldova;
			InvoicingBranch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			RefCurrency pricelistCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, PricelistCurrencyNK);
			RefCurrency invoiceCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, InvoiceCurrencyNK);

			RefExchangeRate usdExchange = pricelistCurrency.ExchangeRates.AddNew();
			usdExchange.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			usdExchange.RE_SellRate = 2;
			usdExchange.RE_StartDate = ZDateTime.Today.AddMonths(-1);
			usdExchange.RE_ExpiryDate = ZDateTime.Today.AddMonths(1);

			RefExchangeRate mdlExchange = invoiceCurrency.ExchangeRates.AddNew();
			mdlExchange.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			mdlExchange.RE_SellRate = 8;
			mdlExchange.RE_StartDate = ZDateTime.Today.AddMonths(-1);
			mdlExchange.RE_ExpiryDate = ZDateTime.Today.AddMonths(1);

			Factory.Save();
		}

		#endregion
	}
}
