using System;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocJobInvoicingJobCharge))]
	sealed class DocJobInvoicingJobChargeTest : DocumentWrapperTestCase
	{
		public void TestNewForJobPofit()
		{
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);
			Factory.Save();

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			var arInvLine = arInvoice.Lines[0];
			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, arInvLine.PK)).Returns(new[] { (ZDate.Today.AddDays(-7), new ZDecimal(8.32m)) });

			var apInvLine = apInvoice.Lines[0];
			taxProcessorMock.Setup(t => t.GetTaxExpenses(Factory, apInvLine.PK)).Returns(new[] { (ZDate.Today.AddDays(-10), new ZDecimal(1.56m)) });

			var charge = Factory.New<Charge>();
			var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertNull(charge.APLine);
			AssertEquals(ZDateTime.Empty, chargeWrapper.APLineReverseDate);
			charge.JR_AL_ARLine = arInvLine.PK;
			charge.JR_AL_APLine = apInvLine.PK;
			AssertNotNull(charge.APLine);
			apInvLine.AL_ReverseDate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, chargeWrapper.APLineReverseDate);

			var wrappersWithARLine = DocJobInvoicingJobCharge.NewForJobPofit(charge, charge.ARLine, Factory);
			var wrappersWithAPLine = DocJobInvoicingJobCharge.NewForJobPofit(charge, charge.APLine, Factory);
			var wrappersWithNullLine = DocJobInvoicingJobCharge.NewForJobPofit(charge, null, Factory);
			var chargesForProfitShare = DocJobInvoicingJobCharge.NewForJobProfit(charge, Factory);

			CombineAssertions(() =>
			{
				AssertEquals(2, wrappersWithARLine.Length);
				var wrapper = wrappersWithARLine[0];
				Assert(!wrapper.IsTaxExpense);

				wrapper = wrappersWithARLine[1];
				Assert(wrapper.IsTaxExpense);
				var expectedTaxExpenseDate = ZDate.Today.AddDays(-7);
				var expectedTaxExpenseAmount = 8.32m;
				AssertEquals(expectedTaxExpenseDate, wrapper.RevenueRecognizeDate);
				AssertEquals(expectedTaxExpenseDate, wrapper.RevenueReverseDate);
				AssertEquals(expectedTaxExpenseDate, wrapper.APLineReverseDate);
				AssertEquals(expectedTaxExpenseAmount, wrapper.RevenueAndCostJRJ);
				AssertEquals(-expectedTaxExpenseAmount, wrapper.LocalCostAmt);
				AssertEquals(8.32m, wrapper.Income);
				AssertEquals(-1.56m, wrapper.Expense);

				AssertEquals(2, wrappersWithAPLine.Length);
				wrapper = wrappersWithAPLine[0];
				Assert(!wrapper.IsTaxExpense);

				wrapper = wrappersWithAPLine[1];
				Assert(wrapper.IsTaxExpense);
				expectedTaxExpenseDate = ZDate.Today.AddDays(-10);
				expectedTaxExpenseAmount = 1.56m;
				AssertEquals(expectedTaxExpenseDate, wrapper.RevenueRecognizeDate);
				AssertEquals(expectedTaxExpenseDate, wrapper.RevenueReverseDate);
				AssertEquals(expectedTaxExpenseDate, wrapper.APLineReverseDate);
				AssertEquals(expectedTaxExpenseAmount, wrapper.RevenueAndCostJRJ);
				AssertEquals(-expectedTaxExpenseAmount, wrapper.LocalCostAmt);
				AssertEquals(8.32m, wrapper.Income);
				AssertEquals(-1.56m, wrapper.Expense);

				AssertEquals(1, wrappersWithNullLine.Length);
				wrapper = wrappersWithNullLine[0];
				Assert(!wrapper.IsTaxExpense);

				AssertEquals(2, chargesForProfitShare.Length);
				wrapper = chargesForProfitShare[0];
				Assert(!wrapper.IsTaxExpense);

				wrapper = chargesForProfitShare[1];
				Assert(wrapper.IsTaxExpense);
			});
		}

		public void TestShowLocalAmountAndExRateOnInvoice()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader client = orgFactory.NewWithValidTestData<OrgHeader>();
			orgFactory.Save();

			Charge charge = Factory.New<Charge>();
			DocJobInvoicingJobCharge chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);

			AssertEquals(ZBool.False, chargeWrapper.ShowLocalAmountAndExRateOnInvoice);

			client.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup group = client.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			orgFactory.Save();

			charge.JR_JH = Factory.NewJobForTesting<JobHeader>().PK;
			charge.Job.LocalChargesPK = client.PK;
			AssertEquals(false, chargeWrapper.ShowLocalAmountAndExRateOnInvoice);

			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.AllExRate;
			orgFactory.Save();
			AssertEquals(true, chargeWrapper.ShowLocalAmountAndExRateOnInvoice);
		}

		public void TestARLineAndAPLineNull()
		{
			var charge = Factory.New<Charge>();
			var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals(null, chargeWrapper.ARLine);
			AssertEquals(null, chargeWrapper.APLine);
		}

		public void TestDisplaySequence()
		{
			var arLine = Factory.New<ARInvoiceLine>();
			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;
			charge.JR_DisplaySequence = 1;

			var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);

			AssertEquals("EstimatedCost", 1, chargeWrapper.DisplaySequence);

			charge.JR_DisplaySequence = 2;
			AssertEquals("EstimatedCost", 2, chargeWrapper.DisplaySequence);
		}

		public void TestEstimatedCost()
		{
			var arLine = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;
			charge.JR_EstimatedCost = 1234m;

			var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals("EstimatedCost", 1234m, chargeWrapper.EstimatedCost);
		}

		public void TestEstimatedRevenue()
		{
			var arLine = Factory.New<ARInvoiceLine>();

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_EstimatedRevenue = 1234m;

			var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals("EstimatedCostWithCurrency", 1234m, chargeWrapper.EstimatedRevenue);
		}

		public void TestIsApproved()
		{
			var charge = Factory.New<Charge>();
			charge.JR_AL_APLine = ZGuid.Empty;
			AssertEquals(false, charge.IsApproved);

			var apLine = Factory.New<ARInvoiceLine>();
			charge.JR_AL_APLine = apLine.PK;
			apLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			var arLine = Factory.New<ARInvoiceLine>();
			charge.JR_AL_ARLine = arLine.PK;

			var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);

			AssertEquals(false, chargeWrapper.IsApproved);

			apLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			AssertEquals(false, chargeWrapper.IsApproved);

			apLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			AssertEquals(false, chargeWrapper.IsApproved);

			apLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			AssertEquals(true, chargeWrapper.IsApproved);

			apLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.UnapprovedCost;
			AssertEquals(false, chargeWrapper.IsApproved);
		}

		public void TestCFXJnl()
		{
			var arLine = Factory.New<ARInvoiceLine>();
			var cfxLine = Factory.New<ARInvoiceLine>();
			cfxLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			cfxLine.AL_LineAmount = 50m;

			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;
			charge.JR_AL_CFXLine = cfxLine.PK;

			var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals("JR_CFXAmtReverseSign", 50m, chargeWrapper.CFXJnl);
		}

		public void TestChargeCodePrintSequence()
		{
			var arLine = Factory.New<ARInvoiceLine>();
			var charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = arLine.PK;

			var chargeCode = TestObjectCreator.CreateChargeCode("CC");
			chargeCode.AC_PrintSequence = 1;
			charge.JR_AC = chargeCode.PK;

			var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals("JR_CFXAmtReverseSign", (ZShort)1, chargeWrapper.ChargeCodePrintSequence);

			chargeCode.AC_PrintSequence = 2;
			AssertEquals("JR_CFXAmtReverseSign", (ZShort)2, chargeWrapper.ChargeCodePrintSequence);
		}

		public void TestSellRecognition()
		{
			var charge = Factory.New<Charge>();
			var line = Factory.New<ARInvoiceLine>();
			line.AL_RevRecognitionType = "IMM";
			charge.JR_AL_ARLine = line.PK;

			var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals("IMM", chargeWrapper.SellRecognition);
		}

		public void TestCostRecognition()
		{
			var charge = Factory.New<Charge>();
			var transactionLine = Factory.New<ARInvoiceLine>();
			charge.JR_AL_APLine = transactionLine.PK;
			transactionLine.AL_RevRecognitionType = "IMM";
			var line = Factory.New<ARInvoiceLine>();
			charge.JR_AL_ARLine = line.PK;

			var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals("IMM", chargeWrapper.CostRecognition);
		}

		public void TestProductName()
		{
			var charge = Factory.New<Charge>();
			var attrib = charge.JobChargeAttributes.AddNew();
			attrib.EC_Name = JobChargeAttribTypeList.Codes.Product;
			attrib.EC_Value = "PRODUCT";

			var line = Factory.New<ARInvoiceLine>();
			charge.JR_AL_ARLine = line.PK;

			var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals("PRODUCT", chargeWrapper.ProductName);
		}

		public void TestOSSellExRateShouldBe1m()
		{
			var uSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			TestObjectCreator.CreateExchangeRate(uSD, 3.685m);

			var shipment = TestObjectCreator.CreateShipment("s0001");
			var job = TestObjectCreator.CreateJob(shipment, false);

			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job);
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			charge.JR_RX_NKSellCurrency = uSD.RX_Code;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_OSSellAmt = 75m;
			charge.JR_RX_NKSellInvoiceCurrency = uSD.RX_Code;
			Assert(charge.BillInInvoiceCurrency);
			var receivableCharge = charge as IReceivablesPostingCharge;
			AssertNotNull("IReceivablesPostingCharge", receivableCharge);

			var charge1 = TestObjectCreator.CreateCharge(job);
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			charge1.JR_RX_NKSellCurrency = uSD.RX_Code;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge1.JR_OSSellAmt = 375m;
			charge1.JR_RX_NKSellInvoiceCurrency = uSD.RX_Code;
			Assert(charge1.BillInInvoiceCurrency);
			var receivableCharge1 = charge1 as IReceivablesPostingCharge;
			AssertNotNull("IReceivablesPostingCharge", receivableCharge1);

			Factory.Save();

			var postManager = new InvoicingPostManager(job);
			var hashtable = postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

			Factory.Save();

			AssertEquals(3.685m, charge.JR_OSSellExRate);
			AssertEquals(3.685503686m, receivableCharge.SellExchangeRate);
			AssertEquals(3.685m, charge1.JR_OSSellExRate);
			AssertEquals(3.685141509m, receivableCharge1.SellExchangeRate);
			var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals(1m, chargeWrapper.OSSellExRate);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestOSSellExRate()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge = Factory.New<Charge>();
			charge.JR_JH = job.PK;
			var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);
			var receivableCharge = charge as IReceivablesPostingCharge;
			AssertNotNull("IReceivablesPostingCharge", receivableCharge);

			var osSellExRate = new ZDecimal(1.3);
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_OSSellAmt = 1000m;
			charge.JR_OSSellExRate = osSellExRate;
			var usdRate = ((IExchangeRateProvider)job).GetExchangeRate(TestObjectCreator.USD.RX_Code, ZGuid.Empty, ExchangeRateValidLedgerEnum.AR);
			usdRate.SetBuyRate_ForTestOnly(osSellExRate);
			AssertEquals("JR_LocalSellAmt", 769.23m, charge.JR_LocalSellAmt);
			AssertEquals("OSSellAmount", 1000m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate", osSellExRate, chargeWrapper.OSSellExRate);

			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
			var eurRate = ((IExchangeRateProvider)job).GetExchangeRate(TestObjectCreator.EUR.RX_Code, ZGuid.Empty, ExchangeRateValidLedgerEnum.AR);
			Assert("JF_BuyRate is not set yet", eurRate.Rate.IsEmpty);
			AssertEquals("OSSellAmount", 0m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate with SellInvoiceCurrency but no JF_BuyRate", 0m, chargeWrapper.OSSellExRate);

			eurRate.SetBuyRate_ForTestOnly(1.4m);
			AssertEquals("OSSellAmount", 1076.92m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate with SellInvoiceCurrency and JF_BuyRate", 0.928574m, chargeWrapper.OSSellExRate);

			TestObjectCreator.SetCurrentCompanyReciprocal(true);
			charge.JR_OSSellAmt = 2000m;
			charge.JR_OSSellExRate = osSellExRate;
			AssertEquals("JR_LocalSellAmt", 2600m, charge.JR_LocalSellAmt);
			AssertEquals("OSSellAmount", 1857.14m, receivableCharge.OSSellAmount);
			AssertEquals("OSSellExRate with SellInvoiceCurrency and JF_BuyRate", 0.92857m, chargeWrapper.OSSellExRate);
			TestObjectCreator.SetCurrentCompanyReciprocal(false);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		public void TestChargeExchangeRateWithBillInInvoiceCurrency()
		{
			var charge = Factory.New<Charge>();

			var localSellAmount = new ZDecimal(5);
			charge.JR_LocalSellAmt = localSellAmount;

			var osSellExRate = new ZDecimal(1.3);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.ABIGAS, 5m, TestObjectCreator.AALSHI, 7m);
			charge.JR_JH = job.PK;
			var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);

			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_OSSellAmt = 1000m;
			charge.JR_OSSellExRate = osSellExRate;
			var usdRate = ((IExchangeRateProvider)job).GetExchangeRate(TestObjectCreator.USD.RX_Code, charge.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR);
			usdRate.SetBuyRate_ForTestOnly(osSellExRate);

			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
			var eurRate = ((IExchangeRateProvider)job).GetExchangeRate(TestObjectCreator.EUR.RX_Code, charge.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR);
			eurRate.SetBuyRate_ForTestOnly(1.4m);
			eurRate.EnsureWillNotBeAutoDeleted();
			Assert("BillInInvoice", charge.BillInInvoiceCurrency);
			var line = Factory.New<ARInvoiceLine>();
			charge.JR_AL_ARLine = line.PK;
			AssertEquals("OSSellAmount", 0.882138m, chargeWrapper.OSSellExRate);
		}

		public void TestExchangeRateDecimalPlaces()
		{
			var charge = Factory.New<Charge>();
			var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);

			TestObjectCreator.SetCurrentCompanyReciprocal(true);
			AssertEquals("As company is reciprocal exchange rate decimal places should be 6", 6, chargeWrapper.ExchangeRateDecimalPlaces);

			TestObjectCreator.SetCurrentCompanyReciprocal(false);
			AssertEquals("As company is not reciprocial exchange rate decimal places should be 6", 6, chargeWrapper.ExchangeRateDecimalPlaces);
		}

		public void TestExchangeRateAndAmount()
		{
			var audCurr = TestObjectCreator.AUD;
			var usdCurr = TestObjectCreator.USD;
			var eurCurr = TestObjectCreator.EUR;

			var orgFactory = new BusinessObjectFactory();
			var client = orgFactory.NewWithValidTestData<OrgHeader>();
			client.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			var group = client.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			orgFactory.Save();

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge = Factory.New<Charge>();
			charge.JR_JH = job.PK;
			var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);
			var receivableCharge = charge as IReceivablesPostingCharge;
			AssertNotNull("IReceivablesPostingCharge", receivableCharge);

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = audCurr.RX_Code;
			charge.JR_RX_NKSellCurrency = audCurr.RX_Code;
			charge.JR_OSSellAmt = 1000M;
			AssertEquals("JR_LocalSellAmt", 1000m, charge.JR_LocalSellAmt);
			AssertEquals("", chargeWrapper.ExchangeRateAndAmount);

			job.LocalChargesPK = client.PK;
			AssertEquals("", chargeWrapper.ExchangeRateAndAmount);

			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.AllExRate;
			orgFactory.Save();

			AssertEquals("", chargeWrapper.ExchangeRateAndAmount);

			charge.JR_RX_NKSellCurrency = usdCurr.RX_Code;
			charge.JR_OSSellAmt = 1000M;
			charge.JR_OSSellExRate = 0.78M;
			var usdRate = ((IExchangeRateProvider)job).GetExchangeRate(TestObjectCreator.USD.RX_Code, ZGuid.Empty, ExchangeRateValidLedgerEnum.AR);
			usdRate.SetBuyRate_ForTestOnly(0.78m);
			usdRate.EnsureWillNotBeAutoDeleted();

			AssertEquals("JR_LocalSellAmt", 1282.05m, charge.JR_LocalSellAmt);
			AssertEquals("OSSellAmount", 1000m, receivableCharge.OSSellAmount);
			AssertEquals("USD 1000.00 @ 0.780000", chargeWrapper.ExchangeRateAndAmount);

			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			orgFactory.Save();

			AssertEquals("", chargeWrapper.ExchangeRateAndAmount);

			charge.JR_RX_NKSellInvoiceCurrency = eurCurr.RX_Code;
			var eurRate = ((IExchangeRateProvider)job).GetExchangeRate(eurCurr.RX_Code, ZGuid.Empty, ExchangeRateValidLedgerEnum.AR);
			eurRate.SetBuyRate_ForTestOnly(1.4m);

			AssertEquals("", chargeWrapper.ExchangeRateAndAmount);

			group.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.AllExRate;
			orgFactory.Save();

			AssertEquals("JR_LocalSellAmt", 1282.05m, charge.JR_LocalSellAmt);
			AssertEquals("OSSellAmount", 1794.87m, receivableCharge.OSSellAmount);
			AssertEquals("USD 1000.00 @ 0.557143", chargeWrapper.ExchangeRateAndAmount);

			charge.JR_RX_NKSellInvoiceCurrency = usdCurr.RX_Code;

			AssertEquals("JR_LocalSellAmt", 1282.05m, charge.JR_LocalSellAmt);
			AssertEquals("OSSellAmount", 1000m, receivableCharge.OSSellAmount);
			AssertEquals("USD 1000.00 @ 1.000000", chargeWrapper.ExchangeRateAndAmount);

			charge.JR_RX_NKSellCurrency = audCurr.RX_Code;
			charge.JR_OSSellAmt = 1000M;
			charge.JR_RX_NKSellInvoiceCurrency = usdCurr.RX_Code;
			AssertEquals("JR_LocalSellAmt", 1000m, charge.JR_LocalSellAmt);
			AssertEquals("OSSellAmount", 780m, receivableCharge.OSSellAmount);
			AssertEquals("AUD 1000.00 @ 1.282051", chargeWrapper.ExchangeRateAndAmount);
		}

		public void TestTaxAmount()
		{
			var job = Factory.NewJobForTesting<Job>();
			var charge = job.Charges.AddNew();
			var docCharge = DocJobInvoicingJobCharge.New(charge, Factory);
			charge.JR_LocalSellAmt = 70;
			charge.JR_AT_SellGSTRate = new TestObjectCreator(Factory).GST1.PK;

			AssertEquals("DocCharge.TaxAmount", 7m, docCharge.TaxAmount);

			charge.JR_OSSellExRate = 1m;
			Assert(charge.IsSellLocal);
			charge.JR_RX_NKSellInvoiceCurrency = "USD";
			var exRate = job.ExchangeRates.FindByRefCurrency(RefCurrency.LoadFromCurrencyCode(Factory, "USD"));
			AssertNotNull(exRate);
			exRate.JF_BaseRate = 0.5m;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			AssertEquals(7m, charge.JR_Calc_LocalSellTaxAmt);

			Assert("BillInInvoiceCurrencyWithLocalSellCurrency", charge.BillInInvoiceCurrencyWithLocalSellCurrency);
			AssertEquals("DocCharge.TaxAmount", 7m, docCharge.TaxAmount);

			charge.JR_LineCFX = 10;
			AssertEquals("Charge.JR_LocalSellInvoiceAmt", 77m, charge.JR_LocalSellInvoiceAmt);
			AssertEquals("DocCharge.TaxAmount", 7.7m, docCharge.TaxAmount);
		}

		public void TestTaxAmount_With_UseLocalExTaxAmountWhileCalculatingLocalTaxAmount()
		{
			TestObjectCreator.SetCurrentCompanyCountryCode(Core.Constants.CountryCodes.Chile);
			TestObjectCreator.SetCurrentCompanyReciprocal(true);

			var taxRate = TestObjectCreator.CreateTaxRate("IVA", "Chile IVA", 19);

			Job job = Factory.NewJobForTesting<Job>();
			Charge charge = job.Charges.AddNew();
			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge.JR_OSSellAmt = 10495.13M;
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(664.9865m);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			charge.JR_AT_SellGSTRate = taxRate.PK;
			AssertEquals("Precondition: Local ex tax amount", 6979120M, charge.JR_LocalSellAmt);
			AssertEquals("Local tax is calculated applying 19% tax rate to local ex tax amount", 1326033M, docCharge.TaxAmount);

			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			charge.JR_AT_SellGSTRate = Guid.Empty;

			charge.JR_AT_SellGSTRate = taxRate.PK;
			AssertEquals("Precondition: OS tax amount", 1994.07M, charge.JR_OSSellGSTAmt_Calc);
			AssertEquals("Local tax is calculated by converting os tax amount to local tax amount using exchange rate", 1326030M, docCharge.TaxAmount);
		}

		public void TestLocalSellAmt()
		{
			var job = Factory.NewJobForTesting<Job>();
			var charge = job.Charges.AddNew();
			var docCharge = DocJobInvoicingJobCharge.New(charge, Factory);
			Assert("IsSellLocal", charge.IsSellLocal);

			charge.JR_LocalSellAmt = 70;
			AssertEquals("DocCharge.LocalSellAmt", 70m, docCharge.LocalSellAmt);

			charge.JR_RX_NKSellInvoiceCurrency = "USD";
			Assert("BillInInvoiceCurrencyWithLocalSellCurrency", charge.BillInInvoiceCurrencyWithLocalSellCurrency);
			AssertEquals("Charge.JR_LocalSellInvoiceAmt", 70m, charge.JR_LocalSellInvoiceAmt);
			AssertEquals("DocCharge.LocalSellAmt", 70m, docCharge.LocalSellAmt);

			charge.JR_LineCFX = 10;
			AssertEquals("Charge.JR_LocalSellInvoiceAmt", 77m, charge.JR_LocalSellInvoiceAmt);
			AssertEquals("DocCharge.LocalSellAmt", 77m, docCharge.LocalSellAmt);
		}

		public void TestLocalSellAmountIncTax()
		{
			Job job = Factory.NewJobForTesting<Job>();
			Charge charge = job.Charges.AddNew();
			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);
			charge.JR_RX_NKSellCurrency = "USD";
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			charge.JR_LocalSellAmt = 70;
			charge.JR_AT_SellGSTRate = new TestObjectCreator(Factory).GST1.PK;
			AssertEquals("DocCharge.LocalSellAmountIncTax", 77m, docCharge.LocalSellAmountIncTax);
		}

		public void TestLocalSellAmtForReport()
		{
			Job job = Factory.NewJobForTesting<Job>();
			Charge charge = job.Charges.AddNew();
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_AT_GSTRate = CreateTaxRate("TAX1", "Test GST Rate #1", 10).PK;
			charge.JR_AC = chargeCode.PK;

			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);
			charge.JR_LocalSellAmt = 27.1234M;
			DocumentsDataRegistry.Instance.ShowLocalCurrencyonSpotQuotePricingPage.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("if registry is false this column shouldn't be shown on the report", ZString.Empty, docCharge.LocalSellAmtForReport);
			DocumentsDataRegistry.Instance.ShowLocalCurrencyonSpotQuotePricingPage.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("if registry is true this column should be shown on the report with the correct value", "27.12", docCharge.LocalSellAmtForReport);
		}

		public void TestQuoteCurrencyLabel()
		{
			Job job = Factory.NewJobForTesting<Job>();
			Charge charge = job.Charges.AddNew();
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_AT_GSTRate = CreateTaxRate("TAX1", "Test GST Rate #1", 10).PK;
			charge.JR_AC = chargeCode.PK;

			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);
			DocumentsDataRegistry.Instance.ShowLocalCurrencyonSpotQuotePricingPage.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("if registry is false this column shouldn't be shown on the report", ZString.Empty, docCharge.QuoteCurrencyLabel);
			DocumentsDataRegistry.Instance.ShowLocalCurrencyonSpotQuotePricingPage.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("if registry is true this column should be shown on the report with the correct value", "Quote Currency", docCharge.QuoteCurrencyLabel);
		}

		#region Profit Share

		public void TestAgentDeclaredSellAmount()
		{
			Charge charge = Factory.NewWithValidTestData<Charge>();
			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);

			charge.JR_OSSellAmt = 200m;
			AssertEquals(200m, docCharge.AgentDeclaredSellAmount);

			charge.JR_AgentDeclaredSellAmt = 300m;
			AssertEquals(300m, docCharge.AgentDeclaredSellAmount);
		}

		public void TestAgentDeclaredCostAmount()
		{
			Charge charge = Factory.NewWithValidTestData<Charge>();
			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);

			charge.JR_OSCostAmt = 200m;
			AssertEquals(200m, docCharge.AgentDeclaredCostAmount);

			charge.JR_AgentDeclaredCostAmt = 600m;
			AssertEquals(600m, docCharge.AgentDeclaredCostAmount);
		}

		public void TestDueAgentCosts()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.AgentCollectPK = org1.PK;
			Charge charge = job.Charges.AddNew();
			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);

			charge.JR_AgentDeclaredCostAmt = 600m;
			AssertEquals(false, docCharge.IsPayableToOverseasAgent);
			AssertEquals(0m, docCharge.AgentDeclaredCostAmountDueAgent);

			charge.JR_OH_CostAccount = org1.PK;
			AssertEquals(true, docCharge.IsPayableToOverseasAgent);
			AssertEquals(600m, docCharge.AgentDeclaredCostAmountDueAgent);

			charge.JR_OH_CostAccount = org2.PK;
			AssertEquals(false, docCharge.IsPayableToOverseasAgent);
			AssertEquals(0m, docCharge.AgentDeclaredCostAmountDueAgent);
		}

		public void TestAgentDeclaredProfit()
		{
			Charge charge = Factory.NewWithValidTestData<Charge>();
			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);

			charge.JR_AgentDeclaredCostAmt = 600m;
			charge.JR_AgentDeclaredSellAmt = 900m;
			AssertEquals(300m, docCharge.AgentDeclaredProfit);
		}

		public void TestProfitShareForCharge()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			var consignee = orgFactory.NewWithValidTestData<OrgHeader>();
			var consignor = orgFactory.NewWithValidTestData<OrgHeader>();
			var sendingAgent = orgFactory.NewWithValidTestData<OrgHeader>();
			var deliveryAgent = orgFactory.NewWithValidTestData<OrgHeader>();
			orgFactory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(sendingAgent);
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";

			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.PlugInData = shipment;
			AssertNull("No profit share agreement found", testJob.ProfitShareAgreement);

			BusinessObjectFactory profitShareFactory = new BusinessObjectFactory();
			var agentRelationship = profitShareFactory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship.O3_OH_ReceivingAgent = deliveryAgent.PK;

			OrgProfitShareDetails profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_SendingPortOrCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			profitShareAgreement.O4_ReceivingPortOrCountry = "US";
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);

			OrgProfitShareParty party = profitShareAgreement.PartyDetails.AddNew();
			party.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent;
			party.PS_PartyProfitSharePercent = 40m;

			profitShareFactory.Save();
			AssertEquals("Profit share agreement found", profitShareAgreement.PK, testJob.ProfitShareAgreement.PK);

			Charge charge = testJob.Charges.AddNew();
			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);
			charge.JR_IsIncludedInProfitShare = true;
			charge.JR_AgentDeclaredSellAmt = 300m;
			charge.JR_AgentDeclaredCostAmt = 100m;
			AssertEquals(80m, docCharge.ProfitShareForCharge);
		}

		public void TestGivenAgencyProfitShareProfileForControllingCustomer_WhenGetProfitShareAgreement_ThenProfileShouldNotBeMatched()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.DocAddresses.AddNew(controllingCustomer.MainAddress, DocAddressType.ControllingCustomer);

			var testJob = Factory.NewJobForTesting<Job>();
			testJob.PlugInData = shipment;
			AssertNull("No profit share agreement found", testJob.ProfitShareAgreement);

			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = controllingCustomer.PK;
			agentRelationship.O3_ProfitShareType = "AGY";

			var profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);

			Factory.Save();
			AssertNull("No profit share agreement found, controlling customer will not be used to match", testJob.ProfitShareAgreement);
		}

		public void TestGivenAgencyProfitShareProfileForControllingAgent_WhenGetProfitShareAgreement_ThenProfileShouldBeMatched()
		{
			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.DocAddresses.AddNew(controllingAgent.MainAddress, DocAddressType.ControllingAgent);

			var testJob = Factory.NewJobForTesting<Job>();
			testJob.PlugInData = shipment;
			AssertNull("No profit share agreement found", testJob.ProfitShareAgreement);

			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = controllingAgent.PK;
			agentRelationship.O3_ProfitShareType = "AGY";

			var profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);

			Factory.Save();
			AssertEquals("Profit share agreement found, controlling agent will be used to match", profitShareAgreement.PK, testJob.ProfitShareAgreement.PK);
		}

		#endregion

		public void TestDesc()
		{
			Job job = Factory.NewJobForTesting<Job>();
			Charge charge = job.Charges.AddNew();
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_AT_GSTRate = CreateTaxRate("TAX1", "Test GST Rate #1", 10).PK;
			charge.JR_AC = chargeCode.PK;

			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);

			charge.JR_Desc = "Description";

			AssertEquals("Description", docCharge.Desc);

			charge.Job.JH_ParentTableCode = "XX";
			AssertEquals("Description", docCharge.Desc);

			charge.Job.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
			AssertEquals("Description *", docCharge.Desc);
		}

		public void TestShowChargeOnQuotation()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			Charge charge = Factory.New<Charge>();
			charge.JR_AC = chargeCode.PK;

			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);

			chargeCode.AC_ShowOnQuotation = true;
			chargeCode.AC_SuppressOnQuoteIfZero = false;
			charge.JR_LocalSellAmt = 30m;
			AssertEquals(true, docCharge.ShowChargeOnQuotation);

			charge.JR_LocalSellAmt = 0m;
			AssertEquals(true, docCharge.ShowChargeOnQuotation);

			chargeCode.AC_SuppressOnQuoteIfZero = true;
			AssertEquals(false, docCharge.ShowChargeOnQuotation);

			charge.JR_LocalSellAmt = 10m;
			AssertEquals(true, docCharge.ShowChargeOnQuotation);

			chargeCode.AC_ShowOnQuotation = false;
			AssertEquals(false, docCharge.ShowChargeOnQuotation);
		}

		public void TestCalculationDescription()
		{
			var oldShowCalculationDescriptionOnOneOffQuotes = DocumentsDataRegistry.Instance.ShowCalculationDescriptionOnOneOffQuotes.Value;
			try
			{
				Charge charge = Factory.New<Charge>();
				charge.JR_AC = Env.Registry.FreightChargeCode;

				charge.RevenueCalculationDescription = ZBlob.FromAscii("Some Crap");
				DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);

				DocumentsDataRegistry.Instance.ShowCalculationDescriptionOnOneOffQuotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ShowCalculationDescriptionOnOneOffQuotesCode.Yes);
				Assert(docCharge.CalculationDescription.IsEmpty);

				charge.RevenueCalculationDescription = ZBlob.FromAscii("FRT: 100 KG @ USD 7.00/KG\nFound in client rate");
				docCharge = DocJobInvoicingJobCharge.New(charge, Factory);
				AssertEquals("  100 KG @ USD 7.00/KG", docCharge.CalculationDescription);

				DocumentsDataRegistry.Instance.ShowCalculationDescriptionOnOneOffQuotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ShowCalculationDescriptionOnOneOffQuotesCode.No);
				Assert(docCharge.CalculationDescription.IsEmpty);
			}
			finally
			{
				DocumentsDataRegistry.Instance.ShowCalculationDescriptionOnOneOffQuotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldShowCalculationDescriptionOnOneOffQuotes);
			}
		}

		public void TestChargeIsGSTApplicable()
		{
			Charge charge = Factory.New<Charge>();
			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);

			Assert("no charge", !docCharge.ChargeIsGSTApplicable);

			var gSTCharge = Factory.New<AccChargeCode>();
			gSTCharge.AC_AT_GSTRate = TaxRate.PK;

			charge.JR_AC = gSTCharge.PK;
			Assert("GST Is Applicable", docCharge.ChargeIsGSTApplicable);

			var zeroRate = CreateTaxRate("TAX", "Desc", 0);
			gSTCharge.AC_AT_GSTRate = zeroRate.PK;
			Assert("GST Is NOT Applicable", !docCharge.ChargeIsGSTApplicable);
		}

		public void TestBranchCode()
		{
			Charge charge = Factory.New<Charge>();
			charge.JR_GB = ZGuid.Empty;
			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals("BranchCode must be empty", ZString.Empty, docCharge.BranchCode);
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals("BranchCode must be equal current branch", GlbBranch.CurrentBranch.GB_Code, docCharge.BranchCode);
		}

		public void TestDepartmentCode()
		{
			Charge charge = Factory.New<Charge>();
			charge.JR_GE = ZGuid.Empty;
			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals("DepartmentCode must be empty", ZString.Empty, docCharge.DepartmentCode);
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			AssertEquals("DepartmentCode must be equal current Department", GlbDepartment.CurrentDepartment.GE_Code, docCharge.DepartmentCode);
		}

		public void TestJobHeaderPK()
		{
			Charge charge = Factory.New<Charge>();
			charge.JR_JH = ZGuid.Empty;
			Job job = Factory.NewJobForTesting<Job>();
			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals("JobHeaderPK must be empty", ZGuid.Empty, docCharge.JobHeaderPK);
			charge.JR_JH = job.PK;
			AssertEquals("JobHeaderPK must be equal current job.PK", job.PK, docCharge.JobHeaderPK);
		}

		public void TestARLine()
		{
			Charge charge = Factory.New<Charge>();
			charge.JR_AL_ARLine = Factory.New<AccTransactionLines>().PK;
			charge.ARLine.AL_LineAmount = -123M;
			charge.ARLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals(123M, docCharge.ARLine.Cost);
		}

		public void TestAPLine()
		{
			Charge charge = Factory.New<Charge>();
			charge.JR_AL_APLine = Factory.New<AccTransactionLines>().PK;
			charge.APLine.AL_LineAmount = -123M;
			charge.APLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals(123M, docCharge.APLine.Cost);

			var jobRevenueJournal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, TestObjectCreator.Job1, 100M);
			charge = Factory.New<Charge>();
			charge.JR_AL_APLine = jobRevenueJournal.Lines[0].PK;

			docCharge = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals(100M, docCharge.APLine.Cost);
		}

		public void TestSellLineType()
		{
			Charge charge = Factory.New<Charge>();
			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals("SellLineType must be empty", ZString.Empty, docCharge.SellLineType);

			charge.JR_AL_ARLine = Factory.New<AccTransactionLines>().PK;
			charge.ARLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			AssertEquals("SellLineType must be Cost", ZArchitecture.Core.TransactionLineTypes.Cost, docCharge.SellLineType);
		}

		public void TestCostLineType()
		{
			Charge charge = Factory.New<Charge>();
			DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals("CostLineType must be empty", ZString.Empty, docCharge.CostLineType);

			charge.JR_AL_APLine = Factory.New<AccTransactionLines>().PK;
			charge.APLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			AssertEquals("CostLineType must be Cost", ZArchitecture.Core.TransactionLineTypes.Cost, docCharge.CostLineType);
		}

		public void TestIncome()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001234", "AUSYD", "NZAKL");
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";

			var job = TestObjectCreator.CreateJob("Sx0001001", TestObjectCreator.AALSHI, 0M, TestObjectCreator.ABIGAS, 0M);
			job.JH_JobNum = "S00001234";
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipment.PK;

			var charge = Factory.NewWithValidTestData<Charge>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "BBB";
			var chargeOverride = chargeCode.ChargeTypeOverrides.AddNew();
			chargeOverride.AN_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeOverride.AN_JobType = "ALL";
			chargeOverride.AN_JobDirection = "ALL";
			charge.JR_AC = chargeCode.PK;

			Factory.Save();

			var revLine = Factory.NewWithValidTestData<AccTransactionLines>();
			revLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;

			var cstLine = Factory.NewWithValidTestData<AccTransactionLines>();
			cstLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;

			charge.JR_LocalSellAmt = 50M;
			charge.JR_AL_ARLine = revLine.PK;
			charge.JR_LocalCostAmt = 35M;
			charge.JR_AL_APLine = cstLine.PK;

			charge.JR_JH = job.PK;

			var docCharge = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals("Icome is 50", 50M, docCharge.Income);
			AssertEquals("Expense is 35", 35M, docCharge.Expense);

			AssertEquals("IncomeAfterChargeExclusion is 0 when charge is excluded", 0M, docCharge.IncomeAfterChargeExclusion);
			AssertEquals("ExpenseAfterChargeExclusion is 0 when charge is excluded", 0M, docCharge.ExpenseAfterChargeExclusion);

			try
			{
				AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AssertEquals("IncomeAfterChargeExclusion is same as Income when charge is not excluded", docCharge.Income, docCharge.IncomeAfterChargeExclusion);
				AssertEquals("ExpenseAfterChargeExclusion is same as Expense when charge is not excluded", docCharge.Expense, docCharge.ExpenseAfterChargeExclusion);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			}
		}

		public void TestPaymentBases()
		{
			var charge = Factory.NewWithValidTestData<Charge>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "BBB";
			var chargeOverride = chargeCode.ChargeTypeOverrides.AddNew();
			chargeOverride.AN_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeOverride.AN_JobType = "ALL";
			chargeOverride.AN_JobDirection = "ALL";
			charge.JR_AC = chargeCode.PK;

			Factory.Save();

			var paymentBasis1 = charge.PaymentBases.AddNew();
			paymentBasis1.PBS_AdapterID = "Consol";
			paymentBasis1.PBS_ChargeableAmount = 10;
			paymentBasis1.PBS_ChargeableUnit = "KG";
			paymentBasis1.PBS_ChargeableDescription = "AB1111111";
			paymentBasis1.PBS_PerUnitRate = 20;
			paymentBasis1.PBS_RateReference = nameof(RateInfo.RateInfoType.UNT);

			var paymentBasis2 = charge.PaymentBases.AddNew();
			paymentBasis2.PBS_AdapterID = "Shipment";
			paymentBasis2.PBS_ChargeableAmount = 20;
			paymentBasis2.PBS_ChargeableUnit = "M3";
			paymentBasis2.PBS_ChargeableDescription = "AB222222";
			paymentBasis2.PBS_JR = charge.PK;
			paymentBasis2.PBS_PerUnitRate = 30;
			paymentBasis2.PBS_RateReference = nameof(RateInfo.RateInfoType.UNT);

			var docCharge = DocJobInvoicingJobCharge.New(charge, Factory);

			Func<DocJobPaymentBasis, string> toString = basis =>
			{
				return Invariant($"{basis.Adapter}|{basis.Quantity}|{basis.QuantityUnit}|{basis.Reference}");
			};

			var expected = new[]
			{
				"Consol|10|KG|AB1111111",
				"Shipment|20|M3|AB222222",
			};

			paymentBasis1.PBS_IsCost = true;
			paymentBasis2.PBS_IsCost = true;
			AssertContainsExactElementsInAnyOrder(expected, docCharge.CostPaymentBases.Cast<DocJobPaymentBasis>().Select(toString));

			paymentBasis1.PBS_IsCost = false;
			paymentBasis2.PBS_IsCost = false;
			AssertContainsExactElementsInAnyOrder(expected, docCharge.SellPaymentBases.Cast<DocJobPaymentBasis>().Select(toString));
		}

		public void TestRevenueRecognizeDate()
		{
			var jobRevenueJournal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, TestObjectCreator.Job1, 10M);
			jobRevenueJournal.Lines[0].AL_ReverseDate = ZDateTime.BrettsBirthday;
			ACharge = Factory.New<Charge>();
			ACharge.JR_AL_APLine = jobRevenueJournal.Lines[0].PK;
			var docCharge = DocJobInvoicingJobCharge.New(ACharge, Factory);
			AssertEquals("RevenueRecognizeDate", docCharge.APLine.RecognizeDate, docCharge.RevenueRecognizeDate);

			var revLine = Factory.NewWithValidTestData<AccTransactionLines>();
			revLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			revLine.AL_ReverseDate = ZDateTime.BrettsBirthday;

			ACharge = Factory.New<Charge>();
			ACharge.JR_AL_ARLine = revLine.PK;
			ACharge.JR_LocalSellAmt = 20M;
			docCharge = DocJobInvoicingJobCharge.New(ACharge, Factory);
			AssertEquals("RevenueRecognizeDate", docCharge.ARLine.RecognizeDate, docCharge.RevenueRecognizeDate);
		}

		public void TestRevenueReverseDate()
		{
			var jobRevenueJournal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, TestObjectCreator.Job1, 10M);
			jobRevenueJournal.Lines[0].AL_ReverseDate = ZDateTime.BrettsBirthday;
			ACharge = Factory.New<Charge>();
			ACharge.JR_AL_APLine = jobRevenueJournal.Lines[0].PK;
			var docCharge = DocJobInvoicingJobCharge.New(ACharge, Factory);
			AssertEquals("RevenueRecognizeDate", docCharge.APLine.ReverseDate, docCharge.RevenueReverseDate);

			var revLine = Factory.NewWithValidTestData<AccTransactionLines>();
			revLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			revLine.AL_ReverseDate = ZDateTime.BrettsBirthday;

			ACharge = Factory.New<Charge>();
			ACharge.JR_AL_ARLine = revLine.PK;
			ACharge.JR_LocalSellAmt = 20M;
			docCharge = DocJobInvoicingJobCharge.New(ACharge, Factory);
			AssertEquals("RevenueRecognizeDate", docCharge.ARLine.ReverseDate, docCharge.RevenueReverseDate);
		}

		public void TestAmountValues()
		{
			RefCurrency chargeCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			RefExchangeRate rate = chargeCurrency.ExchangeRates.AddNew();
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			rate.RE_SellRate = 0.9m;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			try
			{
				AccTransactionLines revLine = Factory.NewWithValidTestData<AccTransactionLines>();
				revLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;

				AccTransactionLines wipLine = Factory.NewWithValidTestData<AccTransactionLines>();
				wipLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;

				AccTransactionLines cstLine = Factory.NewWithValidTestData<AccTransactionLines>();
				cstLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;

				AccTransactionLines acrLine = Factory.NewWithValidTestData<AccTransactionLines>();
				acrLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;

				ACharge = Factory.New<Charge>();

				ACharge.JR_LocalSellAmt = 10M;
				AssertChargeAmounts(0M, 0M, 10M, 0M, 0M, 10M, 0M, 10M);

				var job = TestObjectCreator.CreateJob("Sx0001001", TestObjectCreator.AALSHI, 0M, TestObjectCreator.ABIGAS, 0M);
				ACharge = Factory.New<Charge>();
				ACharge.JR_JH = job.PK;
				ACharge.JR_LocalSellAmt = 15M;
				ACharge.JR_AL_ARLine = wipLine.PK;
				AssertChargeAmounts(0M, 0M, 15M, 0M, 0M, 15M, 0M, 15M);

				ACharge.JR_RX_NKSellCurrency = chargeCurrency.RX_Code;

				ACharge.JR_OSSellAmt = 16M;
				ACharge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.8m);
				job.ExchangeRates[0].JF_CFXPercent = 5m;
				AssertEquals("CFX Amount", 5m, ACharge.JR_LineCFX);
				AssertEquals("Local Sell Amount", 21.05M, ACharge.JR_LocalSellAmt);
				AssertEquals("CFX Amount", 1.05M, ACharge.JR_CFXAmt);
				AssertChargeAmounts(0M, 0M, 20.00M, 0M, 0M, 20.00M, 0M, 20.00M);

				ACharge = Factory.New<Charge>();
				ACharge.JR_AL_ARLine = revLine.PK;
				ACharge.JR_LocalSellAmt = 20M;
				AssertChargeAmounts(20M, 20M, 0M, 0M, 0M, 20M, 0M, 20M);

				ACharge.JR_RX_NKSellCurrency = "USD";
				ACharge.JR_OSSellAmt = 16M;
				ACharge.JR_OSSellExRate = 0.8M;
				AssertEquals("Local Sell Amount", 20M, ACharge.JR_LocalSellAmt);
				AccTransactionLines cfxLine = Factory.New<AccTransactionLines>();
				cfxLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
				cfxLine.AL_LineAmount = -2.22M;
				ACharge.JR_AL_CFXLine = cfxLine.PK;
				AssertEquals("CFX Amount", 2.22M, ACharge.JR_CFXAmt);
				Assert("IsCFXPosted", ACharge.IsCFXPosted);

				AssertChargeAmounts(17.78M, 17.78M, 0M, 0M, 0M, 17.78M, 0M, 17.78M);

				ACharge = Factory.New<Charge>();
				ACharge.JR_AL_ARLine = cstLine.PK;
				ACharge.JR_LocalSellAmt = 20M;
				AssertChargeAmounts(0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M);

				ACharge = Factory.New<Charge>();
				ACharge.JR_AL_ARLine = revLine.PK;
				ACharge.JR_LocalSellAmt = 20M;
				ACharge.JR_LocalCostAmt = 30M;
				AssertChargeAmounts(20M, 20M, 0M, 0M, 30M, 20M, 30M, -10M);

				ACharge = Factory.New<Charge>();
				ACharge.JR_AL_ARLine = revLine.PK;
				ACharge.JR_LocalSellAmt = 20M;
				ACharge.JR_AL_APLine = acrLine.PK;
				ACharge.JR_LocalCostAmt = 35M;
				AssertChargeAmounts(20M, 20M, 0M, 0M, 35M, 20M, 35M, -15M);

				ACharge = Factory.New<Charge>();
				ACharge.JR_AL_ARLine = revLine.PK;
				ACharge.JR_LocalSellAmt = 20M;
				ACharge.JR_AL_APLine = cstLine.PK;
				ACharge.JR_LocalCostAmt = 40M;
				AssertChargeAmounts(20M, 20M, 0M, 40M, 0M, 20M, 40M, -20M);

				ACharge = Factory.New<Charge>();
				ACharge.JR_AL_ARLine = revLine.PK;
				ACharge.JR_LocalSellAmt = 20M;
				ACharge.JR_AL_APLine = wipLine.PK;
				ACharge.JR_LocalCostAmt = 40M;
				AssertChargeAmounts(20M, 20M, 0M, 0M, 0M, 20M, 0M, 20M);

				var jobRevenueJournal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, TestObjectCreator.Job1, 10M);
				ACharge = Factory.New<Charge>();
				ACharge.JR_AL_APLine = jobRevenueJournal.Lines[0].PK;
				ACharge.JR_LocalCostAmt = 10M;
				AssertChargeAmounts(-10M, 0M, 0M, 10M, 0M, 0M, 10M, -10M);
			}
			finally
			{
				ExchangeRateReader.GetReaderInstance().ClearCache();
			}
		}

		void AssertChargeAmounts(decimal revAndCostJRJ, decimal rev, decimal wip, decimal cst, decimal acr, decimal income, decimal expense, decimal profit)
		{
			try
			{
				AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				DocJobInvoicingJobCharge docCharge = DocJobInvoicingJobCharge.New(ACharge, Factory);
				AssertEquals("RevAndCostJRJ", revAndCostJRJ, docCharge.RevenueAndCostJRJ);
				AssertEquals("WIP", wip, docCharge.WIP);
				AssertEquals("ACR", acr, docCharge.Accrual);
				AssertEquals("CST", cst, docCharge.Cost);
				AssertEquals("REV", rev, docCharge.Revenue);
				AssertEquals("Income", income, docCharge.Income);
				AssertEquals("Expense", expense, docCharge.Expense);
				AssertEquals("Profit", profit, docCharge.Profit);
			}

			finally
			{
				AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			}
		}

		public void TestSingleJobChargeAttrib_AllCalculatorDescriptions()
		{
			var charge = Factory.New<Charge>();
			var attrib = charge.JobChargeAttributes.AddNew();
			attrib.EC_Name = JobChargeAttribTypeList.Codes.CalculatorDescription;
			attrib.EC_Value = "DESCRIPTION";

			var line = Factory.New<ARInvoiceLine>();
			charge.JR_AL_ARLine = line.PK;

			var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);
			AssertEquals("1. DESCRIPTION", chargeWrapper.JobChargeAttrib_AllCalculatorDescriptions);
		}

		public void TestMultipleJobChargeAttrib_AllCalculatorDescriptions()
		{
			var charge = Factory.New<Charge>();
			var attrib1 = charge.JobChargeAttributes.AddNew();
			attrib1.EC_Name = JobChargeAttribTypeList.Codes.CalculatorDescription;
			attrib1.EC_Value = "DESCRIPTION3";

			var attrib2 = charge.JobChargeAttributes.AddNew();
			attrib2.EC_Name = JobChargeAttribTypeList.Codes.CalculatorDescription;
			attrib2.EC_Value = "DESCRIPTION2";

			var attrib3 = charge.JobChargeAttributes.AddNew();
			attrib3.EC_Name = JobChargeAttribTypeList.Codes.CalculatorDescription;
			attrib3.EC_Value = "DESCRIPTION1";

			var line = Factory.New<ARInvoiceLine>();
			charge.JR_AL_ARLine = line.PK;

			var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);
			//the attributes are sorted alphabetically
			AssertEquals("1. DESCRIPTION1\r\n2. DESCRIPTION2\r\n3. DESCRIPTION3", chargeWrapper.JobChargeAttrib_AllCalculatorDescriptions);
		}

		public void TestIsDetentionDemurrageChargeSubGroupForOSRA_ReturnTrue()
		{
			var detentionDemurrageChargeSubGroups = new string[]
			{
				ChargeCodeSubGroupList.Storage,
				ChargeCodeSubGroupList.CarrierStorage,
				ChargeCodeSubGroupList.ContainerDetention,
			};
			foreach (var subGroup in detentionDemurrageChargeSubGroups)
			{
				var charge = Factory.NewWithValidTestData<Charge>();
				charge.ChargeCode.AC_ChargeSubGroup = subGroup;
				var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);
				Assert(chargeWrapper.IsDetentionDemurrageChargeSubGroupForOSRA);
			}
		}

		public void TestIsDetentionDemurrageChargeSubGroupForOSRA_ReturnFalse()
		{
			var detentionDemurrageChargeSubGroups = new string[]
			{
				ChargeCodeSubGroupList.Storage,
				ChargeCodeSubGroupList.CarrierStorage,
				ChargeCodeSubGroupList.ContainerDetention,
			};

			var nondetentionDemurrageChargeSubGroups = typeof(ChargeCodeSubGroupList).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(fi => fi.IsLiteral && !fi.IsInitOnly)
				.Select(fi => (string)fi.GetValue(null))
				.Where(x => !detentionDemurrageChargeSubGroups.Contains(x));

			foreach (var subGroup in nondetentionDemurrageChargeSubGroups)
			{
				var charge = Factory.NewWithValidTestData<Charge>();
				charge.ChargeCode.AC_ChargeSubGroup = subGroup;
				var chargeWrapper = DocJobInvoicingJobCharge.New(charge, Factory);
				Assert(!chargeWrapper.IsDetentionDemurrageChargeSubGroupForOSRA);
			}
		}

		#region Implementation

		AccTaxRate CreateTaxRate(string code, string description, int rate)
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = code;
			taxRate.AT_Description = description;
			taxRate.AT_IsActive = true;
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.SetRateNumerator_ForTestOnly(rate);
			return taxRate;
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocJobInvoicingJobCharge.New(ACharge, Factory)
			};
		}

		Charge ACharge;
		protected override void SetUp()
		{
			ACharge = Factory.New<Charge>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			ACharge.JR_AC = chargeCode.PK;
			base.SetUp();
		}

		AccTaxRate TaxRate
		{
			get
			{
				if (fTaxRate == null)
				{
					fTaxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
					fTaxRate.AT_Description = "Test GST Rate Number 2";
					fTaxRate.AT_IsActive = true;
					fTaxRate.SetRateNumerator_ForTestOnly(10);
				}
				return fTaxRate;
			}
		}

		AccTaxRate fTaxRate;

		#endregion
	}
}
