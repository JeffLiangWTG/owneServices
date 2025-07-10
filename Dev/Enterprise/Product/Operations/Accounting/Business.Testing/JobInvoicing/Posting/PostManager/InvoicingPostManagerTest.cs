using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(InvoicingPostManager))]
	public class InvoicingPostManagerTest : BasePostManagerTest
	{
		protected override BasePostManager GetPostManager(IEnumerable<Job> jobs, GlbBranch taxBranch)
		{
			if (!jobs.Any() || jobs.Count() > 1)
			{
				throw new InvalidOperationException();
			}

			return new InvoicingPostManager(jobs.First());
		}

		public void TestInvoiceSplittingWhenAJobIsPosted()
		{
			using (Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001")))
			{
				AccountingConfigurationRegistry.Instance.JobInvoiceMaximumNumberOfChargesOfSplittingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
				TestObjectCreator.CreateOrgInvoiceType(TestObjectCreator.LocalClient.CompanyData, "ALL", "ALL", "ALL", "", "INV", "INV");

				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 250M, 500M);
				charge.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

				charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, 900M, 900M);
				charge.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

				charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, 150M, 320M);
				charge.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

				charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, 350M, 700M);
				charge.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

				Factory.Save();

				var periodHelper = new AccountingPeriodTestHelper(Factory);
				periodHelper.SetupPeriods();

				var postManager = new InvoicingPostManager(job);
				postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);
				AssertEquals(2, postManager.Poster.PostedInvoices.Count);
			}
		}

		[TestDate(2022, 05, 15)]
		public void TestPostChargesWithEmptyTaxDateWithEITRegistry_AP()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 2m, new DateTime(2022, 05, 13), new DateTime(2022, 05, 13));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 2.5m, new DateTime(2022, 05, 14), new DateTime(2022, 05, 14));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 3m, new DateTime(2022, 05, 15), new DateTime(2022, 05, 15));

			var collectionAP = new InvoicePostingExRateOptionCollection();
			collectionAP.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code, 1));
			collectionAP.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code, 1));

			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionAP);

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption1 = collection.AddNew();
			taxDateOption1.JobType = "SHP";
			taxDateOption1.DirectionCode = "ALL";
			taxDateOption1.Mode = "ALL";
			taxDateOption1.Ledger = "AP";
			taxDateOption1.TaxDateOption = TaxDateDefaultingOption.Code.EstimatedArrivalDate;

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001");
			shipment.JS_E_ARV = new ZDateTime(2022, 05, 13);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var chargeCode = TestObjectCreator.CC1;
			var charge = TestObjectCreator.CreateCharge(job, chargeCode, creditor: TestObjectCreator.Creditor1, invoiceNum: "INV1", costCurrency: TestObjectCreator.USD);

			AssertEquals(ZDate.Empty, charge.JR_CostTaxDate);
			AssertEquals(new ZDateTime(2022, 05, 15), charge.JR_APInvoiceDate);

			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				var transactions = new InvoicingPostManager(job).CreateTransactions(JobInvoicingPostingOption.Costs);
				AssertEquals(1, transactions.Count);
				Assert(charge.IsCostPosted);
				AssertEquals(new ZDateTime(2022, 05, 13), charge.JR_CostTaxDate);
				AssertEquals(2.5m, charge.JR_OSCostExRate);
			}
		}

		[TestDate(2022, 05, 15)]
		public void TestPostChargesWithEmptyTaxDateWithEITRegistry_AR()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 2m, new DateTime(2022, 05, 13), new DateTime(2022, 05, 13));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 2.5m, new DateTime(2022, 05, 14), new DateTime(2022, 05, 14));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 3m, new DateTime(2022, 05, 15), new DateTime(2022, 05, 15));

			var collectionAR = new InvoicePostingExRateOptionCollection();
			collectionAR.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code, -1));
			collectionAR.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code, -1));
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionAR);

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption1 = collection.AddNew();
			taxDateOption1.JobType = "SHP";
			taxDateOption1.DirectionCode = "ALL";
			taxDateOption1.Mode = "ALL";
			taxDateOption1.Ledger = "AR";
			taxDateOption1.TaxDateOption = TaxDateDefaultingOption.Code.EstimatedArrivalDate;

			Factory.Save();

			var chargeCode = TestObjectCreator.CC1;
			var org = TestObjectCreator.TestOrganisation;
			SetupDebtor(org);

			var shipment = TestObjectCreator.CreateShipment("S001");
			shipment.JS_E_ARV = new ZDateTime(2022, 05, 14);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, chargeCode, debtor: org, sellCurrency: TestObjectCreator.USD, osCostAmt: 0m);

			AssertEquals(ZDate.Empty, charge.JR_SellTaxDate);
			AssertEquals(new ZDateTime(2022, 05, 15), charge.JR_Calc_ARInvoiceDate);

			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				var transactions = new InvoicingPostManager(job).CreateTransactions(JobInvoicingPostingOption.Revenue);
				AssertEquals(1, transactions.Count);
				Assert(charge.IsRevenuePosted);
				AssertEquals(new ZDateTime(2022, 05, 14), charge.JR_SellTaxDate);
				AssertEquals(2m, charge.JR_OSSellExRate);
			}
		}

		#region TestAppendInvoiceDescription

		public void TestAppendInvoiceDescription()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingAndAutomaticllyAppendingDisbursementFeesSummary(CountryCodes.KoreaSouth, true))
			{
				var freeVAT = GetFreeVATTaxRate();
				var excludeTaxRate = GetEXCLUDETaxRate();

				SetupDSBSummaryAppendingRuleRegistry(freeVAT.PK);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				job.Company.GC_Code = CountryCodes.KoreaSouth;

				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, debtor: TestObjectCreator.Debtor, sellCurrency: TestObjectCreator.KRW, osSellAmt: 1000M);
				charge1.JR_AT_SellGSTRate = freeVAT.PK;
				charge1.JR_SellReference = "charge1";

				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, debtor: TestObjectCreator.Debtor, sellCurrency: TestObjectCreator.KRW, osSellAmt: 2000M);
				charge2.JR_AT_SellGSTRate = excludeTaxRate.PK;
				charge2.JR_SellTaxDate = ZDate.Today;
				charge2.JR_SellReference = "charge2";

				Factory.Save();

				var transactions = GetGeneratedTransactions(shipment, job);
				AssertPostedInvoice(transactions, 2);
				AssertAppendedSummary(transactions, "KRW", "FREEVAT", "ZZCC1: 2,000 총합계 2,000 원");
			}
		}

		public void TestAppendInvoiceDescription_MultipleInvoices()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingAndAutomaticllyAppendingDisbursementFeesSummary(CountryCodes.KoreaSouth, true))
			{
				var freeVAT = GetFreeVATTaxRate();
				var vatTaxRate = GetVATTaxRate();
				var excludeTaxRate = GetEXCLUDETaxRate();

				SetupDSBSummaryAppendingRuleRegistry(vatTaxRate.PK, freeVAT.PK);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				job.Company.GC_Code = CountryCodes.KoreaSouth;

				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, debtor: TestObjectCreator.Debtor, sellCurrency: TestObjectCreator.KRW, osSellAmt: 1000M);
				charge1.JR_AT_SellGSTRate = freeVAT.PK;
				charge1.JR_SellReference = "charge1";

				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, debtor: TestObjectCreator.Debtor, sellCurrency: TestObjectCreator.KRW, osSellAmt: 1000M);
				charge2.JR_AT_SellGSTRate = vatTaxRate.PK;
				charge2.JR_SellTaxDate = ZDate.Today;
				charge2.JR_SellReference = "charge2";

				var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, debtor: TestObjectCreator.Debtor, sellCurrency: TestObjectCreator.KRW, osSellAmt: 1000M);
				charge3.JR_AT_SellGSTRate = excludeTaxRate.PK;
				charge3.JR_SellTaxDate = ZDate.Today;
				charge3.JR_SellReference = "charge3";
				var charge4 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, debtor: TestObjectCreator.Debtor, sellCurrency: TestObjectCreator.KRW, osSellAmt: 2000M);
				charge4.JR_AT_SellGSTRate = excludeTaxRate.PK;
				charge4.JR_SellTaxDate = ZDate.Today;
				charge4.JR_SellReference = "charge3";

				Factory.Save();

				var transactions = GetGeneratedTransactions(shipment, job);
				AssertPostedInvoice(transactions, 3);
				AssertAppendedSummary(transactions, "KRW", "VAT", "ZZCC1: 1,000 ZZCC3: 2,000 총합계 3,000 원");
			}
		}

		public void TestAppendInvoiceDescription_MultipleCurrency()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingAndAutomaticllyAppendingDisbursementFeesSummary(CountryCodes.KoreaSouth, true))
			{
				TestObjectCreator.SetCurrentCompanyReciprocal(true);

				var freeVAT = GetFreeVATTaxRate();
				var excludeTaxRate = GetEXCLUDETaxRate();

				SetupDSBSummaryAppendingRuleRegistry(freeVAT.PK);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				job.Company.GC_Code = CountryCodes.KoreaSouth;
				TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 1000M);

				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, debtor: TestObjectCreator.Debtor, sellCurrency: TestObjectCreator.KRW, osSellAmt: 1000M);
				charge1.JR_AT_SellGSTRate = freeVAT.PK;
				charge1.JR_SellReference = "charge1";

				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, debtor: TestObjectCreator.Debtor, sellCurrency: TestObjectCreator.KRW, osSellAmt: 2000M);
				charge2.JR_AT_SellGSTRate = excludeTaxRate.PK;
				charge2.JR_SellTaxDate = ZDate.Today;
				charge2.JR_SellReference = "charge2";

				var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, debtor: TestObjectCreator.Debtor, sellCurrency: TestObjectCreator.USD, osSellAmt: 3M);
				charge3.JR_RX_NKSellInvoiceCurrency = "USD";
				charge3.JR_AT_SellGSTRate = freeVAT.PK;
				charge3.JR_SellReference = "charge3";

				var charge4 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, debtor: TestObjectCreator.Debtor, sellCurrency: TestObjectCreator.USD, osSellAmt: 4M);
				charge4.JR_RX_NKSellInvoiceCurrency = "USD";
				charge4.JR_AT_SellGSTRate = excludeTaxRate.PK;
				charge4.JR_SellTaxDate = ZDate.Today;
				charge4.JR_SellReference = "charge4";

				Factory.Save();

				var transactions = GetGeneratedTransactions(shipment, job);
				AssertPostedInvoice(transactions, 4);
				AssertAppendedSummary(transactions, "KRW", "FREEVAT", "ZZCC1: 2,000 총합계 2,000 원");
				AssertAppendedSummary(transactions, "USD", "FREEVAT", "ZZCC1: 4,000 총합계 4,000 원");
			}
		}

		public void TestAppendInvoiceDescription_NoRegistrySetup()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingAndAutomaticllyAppendingDisbursementFeesSummary(CountryCodes.KoreaSouth, true))
			{
				var freeVAT = GetFreeVATTaxRate();
				var excludeTaxRate = GetEXCLUDETaxRate();

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);

				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, debtor: TestObjectCreator.Debtor, sellCurrency: TestObjectCreator.KRW, osSellAmt: 100M);
				charge1.JR_AT_SellGSTRate = freeVAT.PK;
				charge1.JR_SellReference = "charge1";

				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, debtor: TestObjectCreator.Debtor, sellCurrency: TestObjectCreator.KRW, osSellAmt: 200M);
				charge2.JR_AT_SellGSTRate = excludeTaxRate.PK;
				charge2.JR_SellTaxDate = ZDate.Today;
				charge2.JR_SellReference = "charge2";

				Factory.Save();

				var transactions = GetGeneratedTransactions(shipment, job);
				AssertPostedInvoice(transactions, 2);
			}
		}

		public void TestAppendInvoiceDescription_NoExcludedTaxInvoice()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingAndAutomaticllyAppendingDisbursementFeesSummary(CountryCodes.KoreaSouth, true))
			{
				var freeVAT = GetFreeVATTaxRate();
				SetupDSBSummaryAppendingRuleRegistry(freeVAT.PK);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);

				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, debtor: TestObjectCreator.Debtor, sellCurrency: TestObjectCreator.KRW, osSellAmt: 100M);
				charge1.JR_AT_SellGSTRate = freeVAT.PK;

				Factory.Save();

				var transactions = GetGeneratedTransactions(shipment, job);
				AssertPostedInvoice(transactions, 1);
			}
		}

		public void TestAppendInvoiceDescription_NoVATInvoice()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingAndAutomaticllyAppendingDisbursementFeesSummary(CountryCodes.KoreaSouth, true))
			{
				var freeVAT = GetFreeVATTaxRate();
				var excludeTaxRate = GetEXCLUDETaxRate();

				SetupDSBSummaryAppendingRuleRegistry(freeVAT.PK);

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);

				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, debtor: TestObjectCreator.Debtor, sellCurrency: TestObjectCreator.KRW, osSellAmt: 100M);
				charge1.JR_AT_SellGSTRate = excludeTaxRate.PK;
				charge1.JR_SellTaxDate = ZDate.Today;

				Factory.Save();

				var transactions = GetGeneratedTransactions(shipment, job);
				AssertPostedInvoice(transactions, 1);
			}
		}

		AccTaxRate GetFreeVATTaxRate()
		{
			return AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "FREEVAT", AccTaxRate.Types.Rated, 0, 1);
		}

		AccTaxRate GetVATTaxRate()
		{
			var vat = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "VAT", AccTaxRate.Types.Rated, 0, 1);
			vat.AT_A9_DefaultVatClass = TestObjectCreator.TaxMsg1.PK;
			vat.AT_PostingGroupId = 2;

			Factory.Save();

			return vat;
		}

		AccTaxRate GetEXCLUDETaxRate()
		{
			return AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "EXCLUDE", AccTaxRate.Types.ExcludedFromTheTaxBase, 0, 1);
		}

		void SetupDSBSummaryAppendingRuleRegistry(params ZGuid[] taxIds)
		{
			var ruleCollection = new KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection();

			var order = 1;
			foreach (var taxId in taxIds)
			{
				var rule1 = ruleCollection.AddNew();
				rule1.Order = order++;
				rule1.TaxIdPK = taxId;
			}

			AccountingConfigurationRegistry.Instance.DSBSummaryAppendingRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ruleCollection);
		}

		TransactionCreatorHashtable GetGeneratedTransactions(ForwardingShipment shipment, Job job)
		{
			var postManager = new InvoicingPostManager(job);

			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);
			postManager.PerformTransactionDescriptionDefaulting(shipment, transactions);

			return transactions;
		}

		void AssertPostedInvoice(TransactionCreatorHashtable transactions, int count)
		{
			CombineAssertions("Posted Transactions", () =>
			{
				AssertEquals("posted transactions count: ", count, transactions.Count);
				AssertEquals("AR invoices count: ", count, transactions.GetAllARInvoices().Length);
			});
		}

		void AssertAppendedSummary(TransactionCreatorHashtable transactions, string currencyCode = "KRW", string taxRate = "", string appendedDescription = "")
		{
			if (!string.IsNullOrEmpty(appendedDescription))
			{
				var taxInvoice = transactions.GetAllARInvoices().Cast<InvoicingBase>().Where(x => x.AH_RX_NKTransactionCurrency == currencyCode).FirstOrDefault(x => x.Lines.Cast<InvoicingLineBase>().Any(y => y.TaxRate.AT_Code == taxRate));
				var excludeInvoice = transactions.GetAllARInvoices().Cast<InvoicingBase>().Where(x => x.AH_RX_NKTransactionCurrency == currencyCode).FirstOrDefault(x => x.Lines.Cast<InvoicingLineBase>().Any(y => y.TaxRate.AT_Code == "EXCLUDE"));
				CombineAssertions($"AR Invoices with currency {currencyCode}", () =>
				{
					AssertNotNull("Tax Invoice", taxInvoice);
					AssertNotNull("Disbursement Invoice", excludeInvoice);
					AssertContains("Tax Invoice Description", appendedDescription, taxInvoice.AH_Desc);
				});

				Factory.Save();

				var query = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ReceivableDisbursementInvoice);
				query.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_AH, excludeInvoice.PK);
				var reference = Factory.LoadTop1<AccTransactionHeaderReference>(query);

				CombineAssertions($"Reference with invoice's currency {currencyCode}", () =>
				{
					AssertNotNull("Reference", reference);
					AssertEquals("Reference Number", taxInvoice.AH_TransactionNum, reference.AH1_Reference);
				});
			}
			else
			{
				Assert(true);
			}
		}

		#endregion
	}
}
