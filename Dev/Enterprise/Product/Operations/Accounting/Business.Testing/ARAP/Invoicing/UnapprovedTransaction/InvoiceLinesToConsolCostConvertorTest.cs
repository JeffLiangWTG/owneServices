using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.UnapprovedTransaction
{
	public class InvoiceLinesToConsolCostConvertorTest : TestCaseWithFactory
	{
		public void TestCreateSeparateConsolCostBasedOnEachLine()
		{
			TestObjectCreator.CreateTaxOverride(TestObjectCreator.CC1, TestObjectCreator.GST2.PK, costSellAll: "COS", jobType: "FCN");
			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GST1.PK;
			TestObjectCreator.Creditor1.OH_RL_NKClosestPort = "AUSYD";

			var (consol1, job1) = PrepareTestData();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "T001", organisation: TestObjectCreator.Creditor1);
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, job1, TestObjectCreator.CC1, 10m, TestObjectCreator.AUD);
			invoiceLine.AL_SupplyType = "DSB";
			invoice.SubmittedFromInvoicingForm = true;

			var linesByConsolsToCreateConsolCostsBySingleLine = new List<(IJobCostingPlugIn, InvoicingLineBase)>()
				{ ValueTuple.Create(consol1 as IJobCostingPlugIn, invoiceLine) };
			var linesInfoProvider = new LinesInfoProvider();
			var jobHeadersToDispose = new HashSet<Job>() { job1 };

			AssertEquals("Pre-condition", 0, invoice.ConsolCosting.ConsolCosts.Count);
			Assert("Pre-condition", !linesInfoProvider.IsTaxOverridden(invoiceLine));
			AssertEquals(TestObjectCreator.GST1, invoiceLine.TaxRate);
			InvoiceLinesToConsolCostConvertor.CreateSeparateConsolCostBasedOnEachLine(invoice, linesByConsolsToCreateConsolCostsBySingleLine.ToArray()
				, linesInfoProvider, jobHeadersToDispose);

			AssertEquals(1, invoice.ConsolCosting.ConsolCosts.Count);
			AssertNotEquals(TestObjectCreator.GST1, invoice.ConsolCosting.ConsolCosts[0].TaxRate);
			AssertEquals(TestObjectCreator.GST2, invoice.ConsolCosting.ConsolCosts[0].TaxRate);
			AssertEquals("DSB", invoice.ConsolCosting.ConsolCosts[0].E6_SupplyType);

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "T001", organisation: TestObjectCreator.Creditor1);
			var invoiceLine2 = TestObjectCreator.CreateInvoiceLine(invoice2, job1, TestObjectCreator.CC1, 10m, TestObjectCreator.AUD);
			invoiceLine2.AL_SupplyType = "INA";
			invoice2.SubmittedFromInvoicingForm = true;

			var linesByConsolsToCreateConsolCostsBySingleLine2 = new List<(IJobCostingPlugIn, InvoicingLineBase)>()
				{ ValueTuple.Create(consol1 as IJobCostingPlugIn, invoiceLine2) };
			var linesInfoProvider2 = new LinesInfoProvider();
			linesInfoProvider2.SetTaxOverridden(invoiceLine2);
			var jobHeadersToDispose2 = new HashSet<Job>() { job1 };

			AssertEquals("Pre-condition", 0, invoice2.ConsolCosting.ConsolCosts.Count);
			Assert("Pre-condition", linesInfoProvider2.IsTaxOverridden(invoiceLine2));
			AssertEquals(TestObjectCreator.GST1, invoiceLine2.TaxRate);
			InvoiceLinesToConsolCostConvertor.CreateSeparateConsolCostBasedOnEachLine(invoice2, linesByConsolsToCreateConsolCostsBySingleLine2.ToArray()
				, linesInfoProvider2, jobHeadersToDispose2);

			AssertEquals(1, invoice2.ConsolCosting.ConsolCosts.Count);
			AssertNotEquals(TestObjectCreator.GST2, invoice2.ConsolCosting.ConsolCosts[0].TaxRate);
			AssertEquals(TestObjectCreator.GST1, invoice2.ConsolCosting.ConsolCosts[0].TaxRate);
			AssertEquals("INA", invoice2.ConsolCosting.ConsolCosts[0].E6_SupplyType);
		}

		[TestDate(2025, 4, 22)]
		public void TestCreateSeparateConsolCostBasedOnEachLine_WhenLineLocalValueApplicable_ShouldCalculateExRateBasedOnLocalAndOverseaAmount()
			=> AssertCreateSeparateConsolCostBasedOnEachLine_ExchangeRate(true);

		[TestDate(2025, 4, 22)]
		public void TestCreateSeparateConsolCostBasedOnEachLine_WhenLineLocalValueNotApplicable_ShouldUpdateLocalAmountByDefaultConsolCostExRate()
			=> AssertCreateSeparateConsolCostBasedOnEachLine_ExchangeRate(false);

		void AssertCreateSeparateConsolCostBasedOnEachLine_ExchangeRate(bool isLineLocalValueApplicable)
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.73M, ZDateTime.Today, ZDateTime.Today.AddDays(30));

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var shipment2 = TestObjectCreator.CreateShipment("S001002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			shipment1.JS_ActualChargeable = 300m;
			shipment2.JS_ActualChargeable = 700m;

			var apps = new ApportionmentListing(Factory, consol);

			var invoicePendingTransaction = TestObjectCreator.CreateTransactionPendingAllocation("T001", TestObjectCreator.Creditor1, 0m, currency: TestObjectCreator.USD, exchangeRate: 0.73m);
			Factory.Save();

			var invoice = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingTransaction).Invoice;
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, job1, TestObjectCreator.CC1, 4321m, TestObjectCreator.USD, 0.85m);

			var linesByConsolsToCreateConsolCostsBySingleLine = new List<(IJobCostingPlugIn, InvoicingLineBase)>()
				{ ValueTuple.Create(consol as IJobCostingPlugIn, invoiceLine) };
			var linesInfoProvider = new LinesInfoProvider();
			var jobHeadersToDispose = new HashSet<Job>() { job1, job2 };

			if (isLineLocalValueApplicable)
			{
				linesInfoProvider.SetLocalAmountApplicable(invoiceLine);
			}

			AssertEquals("Pre-condition", 0, invoice.ConsolCosting.ConsolCosts.Count);
			InvoiceLinesToConsolCostConvertor.CreateSeparateConsolCostBasedOnEachLine(invoice, linesByConsolsToCreateConsolCostsBySingleLine.ToArray()
				, linesInfoProvider, jobHeadersToDispose);

			AssertEquals(1, invoice.ConsolCosting.ConsolCosts.Count);
			var consolCostOnInvoice = invoice.ConsolCosting.ConsolCosts[0];
			AssertEquals(consolCostOnInvoice.CostAmount, 4321m);
			AssertEquals(consolCostOnInvoice.E6_LocalCostAmount, isLineLocalValueApplicable ? 5083.53m : 5919.18m);
			AssertEquals("Expected consolCostExchangeRate", consolCostOnInvoice.E6_ExchangeRate, isLineLocalValueApplicable ? 0.85m : 0.73m);

			AssertEquals(2, consolCostOnInvoice.ApportionmentCharges.Count);
			var apportionedCharge1 = consolCostOnInvoice.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job1.PK);
			var apportionedCharge2 = consolCostOnInvoice.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job2.PK);
			AssertEquals(apportionedCharge1.JR_OSCostAmt, 1296.3m);
			AssertEquals(apportionedCharge1.JR_LocalCostAmt, isLineLocalValueApplicable ? 1525.06m : 1775.75m);
			AssertEquals("Expected consolCostExchangeRate", apportionedCharge1.JR_OSCostExRate, isLineLocalValueApplicable ? 0.85m : 0.73m);

			AssertEquals(apportionedCharge2.JR_OSCostAmt, 3024.7m);
			AssertEquals(apportionedCharge2.JR_LocalCostAmt, isLineLocalValueApplicable ? 3558.47m : 4143.43m);
			AssertEquals("Expected consolCostExchangeRate", apportionedCharge2.JR_OSCostExRate, isLineLocalValueApplicable ? 0.85m : 0.73m);

			AssertEquals(2, invoice.Lines.Count);
			var invoiceLine1 = invoice.Lines.Cast<InvoicingLineBase>().Single(x => x.AL_JH == job1.PK);
			var invoiceLine2 = invoice.Lines.Cast<InvoicingLineBase>().Single(x => x.AL_JH == job2.PK);
			AssertEquals(invoiceLine1.AL_OSExTaxAmount, 1296.3m);
			AssertEquals(invoiceLine1.AL_LocalExTaxAmount, isLineLocalValueApplicable ? 1525.06m : 1775.75m);
			AssertEquals("Expected consolCostExchangeRate", invoiceLine1.AL_ExchangeRate, isLineLocalValueApplicable ? 0.85m : 0.73m);

			AssertEquals(invoiceLine2.AL_OSExTaxAmount, 3024.7m);
			AssertEquals(invoiceLine2.AL_LocalExTaxAmount, isLineLocalValueApplicable ? 3558.47m : 4143.43m);
			AssertEquals("Expected consolCostExchangeRate", invoiceLine2.AL_ExchangeRate, isLineLocalValueApplicable ? 0.85m : 0.73m);
		}

		public void TestCreatedConsolCostApportionmentMethodValueBasedOnOverriddenRegistry()
			=> AssertCreatedConsolCostApportionmentMethodValueBasedOnRegistryValue(true);
		public void TestCreatedConsolCostApportionmentMethodValueBasedOnRegistryDefaults()
			=> AssertCreatedConsolCostApportionmentMethodValueBasedOnRegistryValue(false);

		void AssertCreatedConsolCostApportionmentMethodValueBasedOnRegistryValue(bool isRegistryOverridden)
		{
			var expectedAppMethod = AllocationMethod.ChargeableUnits;
			if (isRegistryOverridden)
			{
				AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
					ConsolCostDefaultApportionmentMethodConfiguration.Create_ForTestOnly(apportionment: AllocationMethod.TwentyFootEquivalentUnit));
				expectedAppMethod = AllocationMethod.TwentyFootEquivalentUnit;
			}

			TestObjectCreator.CreateTaxOverride(TestObjectCreator.CC1, TestObjectCreator.GST2.PK, costSellAll: "COS", jobType: "FCN");
			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GST1.PK;
			TestObjectCreator.Creditor1.OH_RL_NKClosestPort = "AUSYD";

			var (consol1, job1) = PrepareTestData();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "T001", organisation: TestObjectCreator.Creditor1);
			invoice.SubmittedFromInvoicingForm = true;
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, job1, TestObjectCreator.CC1, 10m, TestObjectCreator.AUD);

			var linesByConsolsToCreateConsolCostsBySingleLine = new List<(IJobCostingPlugIn, InvoicingLineBase)>()
				{ ValueTuple.Create(consol1 as IJobCostingPlugIn, invoiceLine) };
			var linesInfoProvider = new LinesInfoProvider();
			var jobHeadersToDispose = new HashSet<Job>() { job1 };

			AssertEquals("Pre-condition", 0, invoice.ConsolCosting.ConsolCosts.Count);
			InvoiceLinesToConsolCostConvertor
				.CreateSeparateConsolCostBasedOnEachLine(
				invoice,
				linesByConsolsToCreateConsolCostsBySingleLine.ToArray(),
				linesInfoProvider,
				jobHeadersToDispose);

			AssertEquals(1, invoice.ConsolCosting.ConsolCosts.Count);
			AssertEquals(expectedAppMethod, invoice.ConsolCosting.ConsolCosts[0].E6_ApportionmentMethod);
		}

		public void TestConvertToConsolCostRelatedLines()
		{
			TestObjectCreator.CreateTaxOverride(TestObjectCreator.CC1, TestObjectCreator.GST2.PK, costSellAll: "COS", jobType: "FCN");
			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GST1.PK;
			TestObjectCreator.Creditor1.OH_RL_NKClosestPort = "AUSYD";

			var (consol1, job1) = PrepareTestData();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "T001", organisation: TestObjectCreator.Creditor1);
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, job1, TestObjectCreator.CC1, 10m, TestObjectCreator.AUD);
			invoiceLine.AL_SupplyType = "DSB";
			invoice.SubmittedFromInvoicingForm = true;

			var invoicingLineBaseList = new List<InvoicingLineBase>() { invoiceLine };
			var linesForConsolCostByConsols = new List<(IJobCostingPlugIn, IEnumerable<InvoicingLineBase>)>()
				{ ValueTuple.Create(consol1 as IJobCostingPlugIn, invoicingLineBaseList as IEnumerable<InvoicingLineBase>) };
			var linesInfoProvider = new LinesInfoProvider();
			var jobHeadersToDispose = new HashSet<Job>() { job1 };

			AssertEquals("Pre-condition", 0, invoice.ConsolCosting.ConsolCosts.Count);
			Assert("Pre-condition", !linesInfoProvider.IsTaxOverridden(invoiceLine));
			AssertEquals(TestObjectCreator.GST1, invoiceLine.TaxRate);
			InvoiceLinesToConsolCostConvertor.ConvertToConsolCostRelatedLines(Factory, invoice, false, linesForConsolCostByConsols.ToArray(), linesInfoProvider, jobHeadersToDispose);

			AssertEquals(1, invoice.ConsolCosting.ConsolCosts.Count);
			AssertNotEquals(TestObjectCreator.GST1, invoice.ConsolCosting.ConsolCosts[0].TaxRate);
			AssertEquals(TestObjectCreator.GST2, invoice.ConsolCosting.ConsolCosts[0].TaxRate);
			AssertEquals("DSB", invoice.ConsolCosting.ConsolCosts[0].E6_SupplyType);

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "T001", organisation: TestObjectCreator.Creditor1);
			var invoiceLine2 = TestObjectCreator.CreateInvoiceLine(invoice2, job1, TestObjectCreator.CC1, 10m, TestObjectCreator.AUD);
			invoiceLine2.AL_SupplyType = "LOC";
			invoice2.SubmittedFromInvoicingForm = true;

			var invoicingLineBaseList2 = new List<InvoicingLineBase>() { invoiceLine2 };
			var linesForConsolCostByConsols2 = new List<(IJobCostingPlugIn, IEnumerable<InvoicingLineBase>)>()
				{ ValueTuple.Create(consol1 as IJobCostingPlugIn, invoicingLineBaseList2 as IEnumerable<InvoicingLineBase>) };
			var linesInfoProvider2 = new LinesInfoProvider();
			linesInfoProvider2.SetTaxOverridden(invoiceLine2);
			var jobHeadersToDispose2 = new HashSet<Job>() { job1 };

			AssertEquals("Pre-condition", 0, invoice2.ConsolCosting.ConsolCosts.Count);
			Assert("Pre-condition", linesInfoProvider2.IsTaxOverridden(invoiceLine2));
			AssertEquals(TestObjectCreator.GST1, invoiceLine2.TaxRate);
			InvoiceLinesToConsolCostConvertor.ConvertToConsolCostRelatedLines(Factory, invoice2, false, linesForConsolCostByConsols2.ToArray(), linesInfoProvider2, jobHeadersToDispose2);

			AssertEquals(1, invoice2.ConsolCosting.ConsolCosts.Count);
			AssertNotEquals(TestObjectCreator.GST2, invoice2.ConsolCosting.ConsolCosts[0].TaxRate);
			AssertEquals(TestObjectCreator.GST1, invoice2.ConsolCosting.ConsolCosts[0].TaxRate);
			AssertEquals("LOC", invoice2.ConsolCosting.ConsolCosts[0].E6_SupplyType);
		}

		[TestDate(2025, 4, 22)]
		public void TestConvertToConsolCostRelatedLines_WhenLineLocalValueApplicable_ShouldCalculateExRateBasedOnLocalAndOverseaAmount()
			=> AssertConvertToConsolCostRelatedLines_ExchangeRate(true);

		[TestDate(2025, 4, 22)]
		public void TestConvertToConsolCostRelatedLines_WhenLineLocalValueNotApplicable_ShouldUpdateLocalAmountByDefaultConsolCostExRate()
			=> AssertConvertToConsolCostRelatedLines_ExchangeRate(false);

		void AssertConvertToConsolCostRelatedLines_ExchangeRate(bool isLineLocalValueApplicable)
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, ExchangeRateTypes.Code.BuyRate, 0.73M, ZDateTime.Today, ZDateTime.Today.AddDays(30));

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var shipment2 = TestObjectCreator.CreateShipment("S001002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var apps = new ApportionmentListing(Factory, consol);
			var invoicePendingTransaction = TestObjectCreator.CreateTransactionPendingAllocation("T001", TestObjectCreator.Creditor1, 0m, currency: TestObjectCreator.USD, exchangeRate: 0.73m);
			Factory.Save();

			var invoice = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingTransaction).Invoice;
			var invoiceLine1 = TestObjectCreator.CreateInvoiceLine(invoice, job1, TestObjectCreator.CC1, 1100m, TestObjectCreator.USD, 0.85m);
			var invoiceLine2 = TestObjectCreator.CreateInvoiceLine(invoice, job1, TestObjectCreator.CC1, 1300m, TestObjectCreator.USD, 0.85m);
			var invoiceLine3 = TestObjectCreator.CreateInvoiceLine(invoice, job2, TestObjectCreator.CC1, 2200m, TestObjectCreator.USD, 0.85m);
			var invoiceLine4 = TestObjectCreator.CreateInvoiceLine(invoice, job2, TestObjectCreator.CC1, 2600m, TestObjectCreator.USD, 0.85m);
			invoice.SubmittedFromInvoicingForm = true;

			var invoicingLineBaseList = new List<InvoicingLineBase>() { invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4 };
			var linesForConsolCostByConsols = new List<(IJobCostingPlugIn, IEnumerable<InvoicingLineBase>)>()
				{ ValueTuple.Create(consol as IJobCostingPlugIn, invoicingLineBaseList as IEnumerable<InvoicingLineBase>) };
			var linesInfoProvider = new LinesInfoProvider();
			var jobHeadersToDispose = new HashSet<Job>() { job1, job2 };

			if (isLineLocalValueApplicable)
			{
				invoicingLineBaseList.ForEach(line => linesInfoProvider.SetLocalAmountApplicable(line));
			}

			AssertEquals("Pre-condition", 0, invoice.ConsolCosting.ConsolCosts.Count);
			InvoiceLinesToConsolCostConvertor.ConvertToConsolCostRelatedLines(Factory, invoice, false, linesForConsolCostByConsols.ToArray(), linesInfoProvider, jobHeadersToDispose);

			AssertEquals(1, invoice.ConsolCosting.ConsolCosts.Count);
			var consolCostOnInvoice = invoice.ConsolCosting.ConsolCosts[0];
			AssertEquals(consolCostOnInvoice.CostAmount, 7200m);
			AssertEquals(consolCostOnInvoice.E6_LocalCostAmount, isLineLocalValueApplicable ? 8470.59m : 9863.01m);
			AssertEquals("Expected consolCostExchangeRate", consolCostOnInvoice.E6_ExchangeRate, isLineLocalValueApplicable ? 0.85m : 0.73m);

			AssertEquals(2, consolCostOnInvoice.ApportionmentCharges.Count);
			var apportionedCharge1 = consolCostOnInvoice.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job1.PK);
			var apportionedCharge2 = consolCostOnInvoice.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job2.PK);
			AssertEquals(apportionedCharge1.JR_OSCostAmt, 2400m);
			AssertEquals(apportionedCharge1.JR_LocalCostAmt, isLineLocalValueApplicable ? 2823.53m : 3287.67m);
			AssertEquals("Expected consolCostExchangeRate", apportionedCharge1.JR_OSCostExRate, isLineLocalValueApplicable ? 0.85m : 0.73m);

			AssertEquals(apportionedCharge2.JR_OSCostAmt, 4800m);
			AssertEquals(apportionedCharge2.JR_LocalCostAmt, isLineLocalValueApplicable ? 5647.06m : 6575.34m);
			AssertEquals("Expected consolCostExchangeRate", apportionedCharge2.JR_OSCostExRate, isLineLocalValueApplicable ? 0.85m : 0.73m);

			AssertEquals(2, invoice.Lines.Count);
			invoiceLine1 = invoice.Lines.Cast<InvoicingLineBase>().Single(x => x.AL_JH == job1.PK);
			invoiceLine2 = invoice.Lines.Cast<InvoicingLineBase>().Single(x => x.AL_JH == job2.PK);
			AssertEquals(invoiceLine1.AL_OSExTaxAmount, 2400m);
			AssertEquals(invoiceLine1.AL_LocalExTaxAmount, isLineLocalValueApplicable ? 2823.53m : 3287.67m);
			AssertEquals("Expected consolCostExchangeRate", invoiceLine1.AL_ExchangeRate, isLineLocalValueApplicable ? 0.85m : 0.73m);

			AssertEquals(invoiceLine2.AL_OSExTaxAmount, 4800m);
			AssertEquals(invoiceLine2.AL_LocalExTaxAmount, isLineLocalValueApplicable ? 5647.06m : 6575.34m);
			AssertEquals("Expected consolCostExchangeRate", invoiceLine2.AL_ExchangeRate, isLineLocalValueApplicable ? 0.85m : 0.73m);
		}

		public void TestConvertToConsolCostRelatedLines_MatchApportionCharge_MapOneConsolCost_MatchPKEnabled()
		{
			AssertConvertToConsolCostRelatedLines_MatchApportionCharge_MapOneConsolCost(true, false);
		}

		public void TestConvertToConsolCostRelatedLines_MatchApportionCharge_MapOneConsolCost_MatchDisplaySequenceEnabled()
		{
			AssertConvertToConsolCostRelatedLines_MatchApportionCharge_MapOneConsolCost(false, true);
		}

		public void TestConvertToConsolCostRelatedLines_MatchApportionCharge_MapOneConsolCost_MatchCriteriaDisabled()
		{
			AssertConvertToConsolCostRelatedLines_MatchApportionCharge_MapOneConsolCost(false, false);
		}

		public void TestConvertToConsolCostRelatedLines_MatchApportionChargePKs_MapMultipleConsolCosts_MatchPKEnabled()
		{
			AssertConvertToConsolCostRelatedLines_MatchApportionCharge_MapMultipleConsolCosts(true, false);
		}

		public void TestConvertToConsolCostRelatedLines_MatchApportionChargePKs_MapMultipleConsolCosts_MatchDisplaySequenceEnabled()
		{
			AssertConvertToConsolCostRelatedLines_MatchApportionCharge_MapMultipleConsolCosts(false, true);
		}

		public void TestConvertToConsolCostRelatedLines_MatchApportionChargePKs_MapMultipleConsolCosts_MatchCriteriaDisabled()
		{
			AssertConvertToConsolCostRelatedLines_MatchApportionCharge_MapMultipleConsolCosts(false, false);
		}

		public void TestConvertToConsolCostRelatedLines_MatchApportionChargePKs_PartiallyMapOneConsolCost_MatchPKEnabled()
		{
			AssertConvertToConsolCostRelatedLines_MatchApportionCharge_PartiallyMapOneConsolCost(true, false);
		}

		public void TestConvertToConsolCostRelatedLines_MatchApportionChargePKs_PartiallyMapOneConsolCost_MatchDisplaySequenceEnabled()
		{
			AssertConvertToConsolCostRelatedLines_MatchApportionCharge_PartiallyMapOneConsolCost(false, true);
		}

		public void TestConvertToConsolCostRelatedLines_MatchApportionChargePKs_PartiallyMapOneConsolCost_MatchCriteriaDisabled()
		{
			AssertConvertToConsolCostRelatedLines_MatchApportionCharge_PartiallyMapOneConsolCost(false, false);
		}

		public void TestConvertToConsolCostRelatedLines_MatchApportionChargePKs_PartiallyMapMultipleConsolCost_MatchPKEnabled()
		{
			AssertConvertToConsolCostRelatedLines_MatchApportionCharge_PartiallyMapMultipleConsolCost(true, false);
		}

		public void TestConvertToConsolCostRelatedLines_MatchApportionChargePKs_PartiallyMapMultipleConsolCost_MatchDisplaySequenceEnabled()
		{
			AssertConvertToConsolCostRelatedLines_MatchApportionCharge_PartiallyMapMultipleConsolCost(false, true);
		}

		public void TestConvertToConsolCostRelatedLines_MatchApportionChargePKs_PartiallyMapMultipleConsolCost_MatchCriteriaDisabled()
		{
			AssertConvertToConsolCostRelatedLines_MatchApportionCharge_PartiallyMapMultipleConsolCost(false, false);
		}

		public void TestConvertToConsolCostRelatedLines_MatchApportionChargePKs_MapMultipleConsols_MatchPKEnabled()
		{
			AssertConvertToConsolCostRelatedLines_MatchApportionCharge_MapMultipleConsols(true);
		}

		public void TestConvertToConsolCostRelatedLines_MatchApportionChargePKs_MapMultipleConsols_MatchCriteriaDisabled()
		{
			AssertConvertToConsolCostRelatedLines_MatchApportionCharge_MapMultipleConsols(false);
		}

		public void TestConvertToConsolCostRelatedLines_MatchApportionCharge_MixedCriteria_PKTakePrecedence()
		{
			AssertConvertToConsolCostRelatedLines_MatchApportionCharge_MixedCriteria(true, true);
		}

		public void TestConvertToConsolCostRelatedLines_MatchApportionCharge_MixedCriteria_OnlyDisplaySequenceEnabled()
		{
			AssertConvertToConsolCostRelatedLines_MatchApportionCharge_MixedCriteria(false, true);
		}

		public void TestConvertToConsolCostRelatedLines_MatchApportionCharge_MixedCriteria_MatchCriteriaDisabled()
		{
			AssertConvertToConsolCostRelatedLines_MatchApportionCharge_MixedCriteria(false, false);
		}

		public void TestNoExceptionWhenConvertToConsolCostRelatedLinesWithoutChargeCode()
		{
			TestObjectCreator.CreateTaxOverride(TestObjectCreator.CC1, TestObjectCreator.GST2.PK, costSellAll: "COS", jobType: "FCN");
			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GST1.PK;
			TestObjectCreator.Creditor1.OH_RL_NKClosestPort = "AUSYD";

			var (consol1, job1) = PrepareTestData();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "T001", organisation: TestObjectCreator.Creditor1);
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, job1, TestObjectCreator.CC1, 10m, TestObjectCreator.AUD);
			invoiceLine.AL_AC = ZGuid.Empty;
			invoice.SubmittedFromInvoicingForm = true;

			var invoicingLineBaseList = new List<InvoicingLineBase>() { invoiceLine };
			var linesForConsolCostByConsols = new List<(IJobCostingPlugIn, IEnumerable<InvoicingLineBase>)>() { ValueTuple.Create(consol1 as IJobCostingPlugIn, invoicingLineBaseList as IEnumerable<InvoicingLineBase>) };
			var linesInfoProvider = new LinesInfoProvider();

			var jobHeadersToDispose = new HashSet<Job>() { job1 };

			AssertNoExceptionThrown(() => InvoiceLinesToConsolCostConvertor.ConvertToConsolCostRelatedLines(Factory, invoice, false, linesForConsolCostByConsols.ToArray(), linesInfoProvider,  jobHeadersToDispose));
		}

		public void TestConvertToConsolCostRelatedLines_InterCompanyInvoiceImport_VATClassAndOSTaxAmount()
		{
			TestObjectCreator.CreateTaxOverride(TestObjectCreator.CC1, TestObjectCreator.GST2.PK, TestObjectCreator.TaxMsg2.PK, jobType: "SHP");
			TestObjectCreator.CC1.AC_AT_GSTRate = TestObjectCreator.GST1.PK;
			TestObjectCreator.Creditor1.OH_RL_NKClosestPort = "AUSYD";
			TestObjectCreator.GST2.AT_A9_DefaultVatClass = TestObjectCreator.TaxMsg3.PK;

			var (consol1, job1) = PrepareTestData();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "T001", organisation: TestObjectCreator.Creditor1);
			invoice.SubmittedFromInvoicingForm = true;
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, job1, TestObjectCreator.CC1, 10m, TestObjectCreator.AUD);
			invoiceLine.AL_SupplyType = "DSB";
			invoiceLine.AL_OSTaxAmount = 5m;

			var linesByConsolsToCreateConsolCostsBySingleLine = new List<(IJobCostingPlugIn, IEnumerable<InvoicingLineBase>)>()
				{ ValueTuple.Create(consol1 as IJobCostingPlugIn, new List<InvoicingLineBase>() { invoiceLine }) };
			var jobHeadersToDispose = new HashSet<Job>() { job1 };

			AssertEquals("Pre-condition", 0, invoice.ConsolCosting.ConsolCosts.Count);
			AssertEquals(TestObjectCreator.GST2, invoiceLine.TaxRate);
			AssertEquals(TestObjectCreator.TaxMsg2, invoiceLine.VATClass);
			Factory.SetContext(BusinessContext.InterCompanyInvoiceImport);

			InvoiceLinesToConsolCostConvertor.ConvertToConsolCostRelatedLines(Factory, invoice, true, linesByConsolsToCreateConsolCostsBySingleLine.ToArray(), new TaxOverriddenLinesInfoProvider(), jobHeadersToDispose);

			AssertEquals(1, invoice.ConsolCosting.ConsolCosts.Count);
			AssertEquals("Should NOT re-default the Tax ID.", TestObjectCreator.GST2, invoice.ConsolCosting.ConsolCosts[0].TaxRate);
			AssertEquals("Should NOT re-default the Tax Message.", TestObjectCreator.TaxMsg2, invoice.ConsolCosting.ConsolCosts[0].VATClass);
			AssertEquals(5m, invoice.ConsolCosting.ConsolCosts[0].E6_OSGSTAmount_Calc);
			AssertEquals("DSB", invoice.ConsolCosting.ConsolCosts[0].E6_SupplyType);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;

		void AssertConvertToConsolCostRelatedLines_MatchApportionCharge_MapOneConsolCost(bool isMatchPKEnabled, bool isMatchDisplaySequenceEnabled)
		{
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var jobs = CreateShipmentJobsForConsol(consol, new List<(ZDecimal, ZString)> { (200m, "S001"), (300m, "S002") });
			var (job1, job2) = (jobs[0], jobs[1]);

			var apps = new ApportionmentListing(Factory, consol);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI, apps);
			consolCost.E6_OSCostAmount = 2000m;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "T001", organisation: TestObjectCreator.Creditor1);
			var invoiceLine1 = TestObjectCreator.CreateInvoiceLine(invoice, job1, TestObjectCreator.FRT, 3m, TestObjectCreator.AUD);
			var invoiceLine2 = TestObjectCreator.CreateInvoiceLine(invoice, job2, TestObjectCreator.FRT, 5m, TestObjectCreator.AUD);
			invoice.SubmittedFromInvoicingForm = true;

			Factory.Save();

			var jobCharge1 = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job1.PK);
			var jobCharge2 = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job2.PK);

			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();
			if (isMatchPKEnabled)
			{
				MapLineWithApportionedChargeAndMatchingCriteria("PrimaryKey", invoiceLine1.PK, jobCharge1.PK, additionalInfoProvider);
				MapLineWithApportionedChargeAndMatchingCriteria("PrimaryKey", invoiceLine2.PK, jobCharge2.PK, additionalInfoProvider);
			}
			if (isMatchDisplaySequenceEnabled)
			{
				MapLineWithDisplaySequenceAndMatchingCriteria("DisplaySequence", invoiceLine1.PK, new ZShort(1), additionalInfoProvider);
				MapLineWithDisplaySequenceAndMatchingCriteria("DisplaySequence", invoiceLine2.PK, new ZShort(1), additionalInfoProvider);
			}
			Factory.ServiceContainer.AddService(additionalInfoProvider);

			var invoicingLineBaseList = new List<InvoicingLineBase>() { invoiceLine1, invoiceLine2 };
			var linesForConsolCostByConsols = new List<(IJobCostingPlugIn, IEnumerable<InvoicingLineBase>)>()
				{ ValueTuple.Create(consol as IJobCostingPlugIn, invoicingLineBaseList as IEnumerable<InvoicingLineBase>) };
			var linesInfoProvider = new LinesInfoProvider();
			var jobHeadersToDispose = new HashSet<Job>() { job1, job2 };

			AssertEquals("Pre-condition", 0, invoice.ConsolCosting.ConsolCosts.Count);
			InvoiceLinesToConsolCostConvertor.ConvertToConsolCostRelatedLines(Factory, invoice, false, linesForConsolCostByConsols.ToArray(), linesInfoProvider, jobHeadersToDispose);

			AssertEquals(1, invoice.ConsolCosting.ConsolCosts.Count);
			if (isMatchPKEnabled || isMatchDisplaySequenceEnabled)
			{
				var invoiceChargeInfos = new List<(Job relatedJob, ZDecimal expectedAmount, ApportionSplitCharge expectedRelatedApportionCharge)>
				{
					(job1, 3m, jobCharge1),
					(job2, 5m, jobCharge2)
				};
				AssertInvoiceConsolCost(invoice.ConsolCosting.ConsolCosts[0], consolCost, invoiceChargeInfos);
			}
			else
			{
				var invoiceChargeInfos = new List<(Job relatedJob, ZDecimal expectedAmount, ApportionSplitCharge expectedRelatedApportionCharge)>
				{
					(job1, 3m, null),
					(job2, 5m, null)
				};
				AssertInvoiceConsolCost(invoice.ConsolCosting.ConsolCosts[0], null, invoiceChargeInfos);
			}
		}

		void AssertConvertToConsolCostRelatedLines_MatchApportionCharge_MapMultipleConsolCosts(bool isMatchPKEnabled, bool isMatchDisplaySequenceEnabled)
		{
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var jobs = CreateShipmentJobsForConsol(consol, new List<(ZDecimal, ZString)> { (200m, "S001"), (300m, "S002") });
			var (job1, job2) = (jobs[0], jobs[1]);

			var apps1 = new ApportionmentListing(Factory, consol);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI, apps1);
			consolCost1.E6_OSCostAmount = 2000m;
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI, apps1);
			consolCost2.E6_OSCostAmount = 1500m;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "T001", organisation: TestObjectCreator.Creditor1);
			var invoiceLine1 = TestObjectCreator.CreateInvoiceLine(invoice, job1, TestObjectCreator.FRT, 3m, TestObjectCreator.AUD);
			var invoiceLine2 = TestObjectCreator.CreateInvoiceLine(invoice, job2, TestObjectCreator.FRT, 5m, TestObjectCreator.AUD);
			var invoiceLine3 = TestObjectCreator.CreateInvoiceLine(invoice, job1, TestObjectCreator.FRT, 4m, TestObjectCreator.AUD);
			var invoiceLine4 = TestObjectCreator.CreateInvoiceLine(invoice, job2, TestObjectCreator.FRT, 7m, TestObjectCreator.AUD);
			invoice.SubmittedFromInvoicingForm = true;

			Factory.Save();

			var jobCharge1 = consolCost1.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job1.PK);
			var jobCharge2 = consolCost1.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job2.PK);
			var jobCharge3 = consolCost2.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job1.PK);
			var jobCharge4 = consolCost2.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job2.PK);

			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();
			if (isMatchPKEnabled)
			{
				MapLineWithApportionedChargeAndMatchingCriteria("PrimaryKey", invoiceLine1.PK, jobCharge1.PK, additionalInfoProvider);
				MapLineWithApportionedChargeAndMatchingCriteria("PrimaryKey", invoiceLine2.PK, jobCharge2.PK, additionalInfoProvider);
				MapLineWithApportionedChargeAndMatchingCriteria("PrimaryKey", invoiceLine3.PK, jobCharge3.PK, additionalInfoProvider);
				MapLineWithApportionedChargeAndMatchingCriteria("PrimaryKey", invoiceLine4.PK, jobCharge4.PK, additionalInfoProvider);
			}
			if (isMatchDisplaySequenceEnabled)
			{
				MapLineWithDisplaySequenceAndMatchingCriteria("DisplaySequence", invoiceLine1.PK, new ZShort(1), additionalInfoProvider);
				MapLineWithDisplaySequenceAndMatchingCriteria("DisplaySequence", invoiceLine2.PK, new ZShort(1), additionalInfoProvider);
				MapLineWithDisplaySequenceAndMatchingCriteria("DisplaySequence", invoiceLine3.PK, new ZShort(2), additionalInfoProvider);
				MapLineWithDisplaySequenceAndMatchingCriteria("DisplaySequence", invoiceLine4.PK, new ZShort(2), additionalInfoProvider);
			}
			Factory.ServiceContainer.AddService(additionalInfoProvider);

			var invoicingLineBaseList = new List<InvoicingLineBase>() { invoiceLine1, invoiceLine2, invoiceLine3, invoiceLine4 };
			var linesForConsolCostByConsols = new List<(IJobCostingPlugIn, IEnumerable<InvoicingLineBase>)>()
				{ ValueTuple.Create(consol as IJobCostingPlugIn, invoicingLineBaseList as IEnumerable<InvoicingLineBase>) };
			var linesInfoProvider = new LinesInfoProvider();
			var jobHeadersToDispose = new HashSet<Job>() { job1, job2 };

			AssertEquals("Pre-condition", 0, invoice.ConsolCosting.ConsolCosts.Count);
			InvoiceLinesToConsolCostConvertor.ConvertToConsolCostRelatedLines(Factory, invoice, false, linesForConsolCostByConsols.ToArray(), linesInfoProvider, jobHeadersToDispose);

			if (isMatchPKEnabled || isMatchDisplaySequenceEnabled)
			{
				AssertEquals(2, invoice.ConsolCosting.ConsolCosts.Count);

				var invoiceChargeInfos1 = new List<(Job relatedJob, ZDecimal expectedAmount, ApportionSplitCharge expectedRelatedApportionCharge)>
				{
					(job1, 3m, jobCharge1),
					(job2, 5m, jobCharge2)
				};
				AssertInvoiceConsolCost(invoice.ConsolCosting.ConsolCosts[0], consolCost1, invoiceChargeInfos1);

				var invoiceChargeInfos2 = new List<(Job relatedJob, ZDecimal expectedAmount, ApportionSplitCharge expectedRelatedApportionCharge)>
				{
					(job1, 4m, jobCharge3),
					(job2, 7m, jobCharge4)
				};
				AssertInvoiceConsolCost(invoice.ConsolCosting.ConsolCosts[1], consolCost2, invoiceChargeInfos2);
			}
			else
			{
				AssertEquals(1, invoice.ConsolCosting.ConsolCosts.Count);
				var invoiceChargeInfos = new List<(Job relatedJob, ZDecimal expectedAmount, ApportionSplitCharge expectedRelatedApportionCharge)>
				{
					(job1, 7m, null),
					(job2, 12m, null)
				};
				AssertInvoiceConsolCost(invoice.ConsolCosting.ConsolCosts[0], null, invoiceChargeInfos);
			}
		}

		void AssertConvertToConsolCostRelatedLines_MatchApportionCharge_PartiallyMapOneConsolCost(bool isMatchPKEnabled, bool isMatchDisplaySequenceEnabled)
		{
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var jobs = CreateShipmentJobsForConsol(consol, new List<(ZDecimal, ZString)> { (200m, "S001"), (300m, "S002") });
			var (job1, job2) = (jobs[0], jobs[1]);

			var apps = new ApportionmentListing(Factory, consol);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI, apps);
			consolCost.E6_OSCostAmount = 2000m;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "T001", organisation: TestObjectCreator.Creditor1);
			var invoiceLine1 = TestObjectCreator.CreateInvoiceLine(invoice, job1, TestObjectCreator.FRT, 3m, TestObjectCreator.AUD);
			var invoiceLine2 = TestObjectCreator.CreateInvoiceLine(invoice, job2, TestObjectCreator.FRT, 5m, TestObjectCreator.AUD);
			invoice.SubmittedFromInvoicingForm = true;

			Factory.Save();

			var jobCharge2 = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job2.PK);

			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();
			if (isMatchPKEnabled)
			{
				MapLineWithApportionedChargeAndMatchingCriteria("PrimaryKey", invoiceLine2.PK, jobCharge2.PK, additionalInfoProvider);
			}
			if (isMatchDisplaySequenceEnabled)
			{
				MapLineWithDisplaySequenceAndMatchingCriteria("DisplaySequence", invoiceLine2.PK, new ZShort(1), additionalInfoProvider);
			}
			Factory.ServiceContainer.AddService(additionalInfoProvider);

			var invoicingLineBaseList = new List<InvoicingLineBase>() { invoiceLine1, invoiceLine2 };
			var linesForConsolCostByConsols = new List<(IJobCostingPlugIn, IEnumerable<InvoicingLineBase>)>()
				{ ValueTuple.Create(consol as IJobCostingPlugIn, invoicingLineBaseList as IEnumerable<InvoicingLineBase>) };
			var linesInfoProvider = new LinesInfoProvider();
			var jobHeadersToDispose = new HashSet<Job>() { job1, job2 };

			AssertEquals("Pre-condition", 0, invoice.ConsolCosting.ConsolCosts.Count);
			InvoiceLinesToConsolCostConvertor.ConvertToConsolCostRelatedLines(Factory, invoice, false, linesForConsolCostByConsols.ToArray(), linesInfoProvider, jobHeadersToDispose);

			if (isMatchPKEnabled || isMatchDisplaySequenceEnabled)
			{
				AssertEquals(2, invoice.ConsolCosting.ConsolCosts.Count);

				var invoiceChargeInfos1 = new List<(Job relatedJob, ZDecimal expectedAmount, ApportionSplitCharge expectedRelatedApportionCharge)>
				{
					(job1, 3m, null),
					(job2, 0m, null)
				};
				AssertInvoiceConsolCost(invoice.ConsolCosting.ConsolCosts[0], null, invoiceChargeInfos1);

				var invoiceChargeInfos2 = new List<(Job relatedJob, ZDecimal expectedAmount, ApportionSplitCharge expectedRelatedApportionCharge)>
				{
					(job1, 0m, null),
					(job2, 5m, jobCharge2)
				};
				AssertInvoiceConsolCost(invoice.ConsolCosting.ConsolCosts[1], consolCost, invoiceChargeInfos2);
			}
			else
			{
				AssertEquals(1, invoice.ConsolCosting.ConsolCosts.Count);
				var invoiceChargeInfos = new List<(Job relatedJob, ZDecimal expectedAmount, ApportionSplitCharge expectedRelatedApportionCharge)>
				{
					(job1, 3m, null),
					(job2, 5m, null)
				};
				AssertInvoiceConsolCost(invoice.ConsolCosting.ConsolCosts[0], null, invoiceChargeInfos);
			}
		}

		void AssertConvertToConsolCostRelatedLines_MatchApportionCharge_PartiallyMapMultipleConsolCost(bool isMatchPKEnabled, bool isMatchDisplaySequenceEnabled)
		{
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var jobs = CreateShipmentJobsForConsol(consol, new List<(ZDecimal, ZString)> { (200m, "S001"), (300m, "S002") });
			var (job1, job2) = (jobs[0], jobs[1]);

			var apps1 = new ApportionmentListing(Factory, consol);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI, apps1);
			consolCost1.E6_OSCostAmount = 2000m;
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI, apps1);
			consolCost2.E6_OSCostAmount = 1500m;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "T001", organisation: TestObjectCreator.Creditor1);
			var invoiceLine1 = TestObjectCreator.CreateInvoiceLine(invoice, job1, TestObjectCreator.FRT, 3m, TestObjectCreator.AUD);
			var invoiceLine2 = TestObjectCreator.CreateInvoiceLine(invoice, job2, TestObjectCreator.FRT, 5m, TestObjectCreator.AUD);
			invoice.SubmittedFromInvoicingForm = true;

			Factory.Save();

			var jobCharge1 = consolCost1.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job1.PK);
			var jobCharge2 = consolCost2.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job2.PK);

			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();
			if (isMatchPKEnabled)
			{
				MapLineWithApportionedChargeAndMatchingCriteria("PrimaryKey", invoiceLine1.PK, jobCharge1.PK, additionalInfoProvider);
				MapLineWithApportionedChargeAndMatchingCriteria("PrimaryKey", invoiceLine2.PK, jobCharge2.PK, additionalInfoProvider);
			}
			if (isMatchDisplaySequenceEnabled)
			{
				MapLineWithDisplaySequenceAndMatchingCriteria("DisplaySequence", invoiceLine1.PK, new ZShort(1), additionalInfoProvider);
				MapLineWithDisplaySequenceAndMatchingCriteria("DisplaySequence", invoiceLine2.PK, new ZShort(2), additionalInfoProvider);
			}
			Factory.ServiceContainer.AddService(additionalInfoProvider);

			var invoicingLineBaseList = new List<InvoicingLineBase>() { invoiceLine1, invoiceLine2 };
			var linesForConsolCostByConsols = new List<(IJobCostingPlugIn, IEnumerable<InvoicingLineBase>)>()
				{ ValueTuple.Create(consol as IJobCostingPlugIn, invoicingLineBaseList as IEnumerable<InvoicingLineBase>) };
			var linesInfoProvider = new LinesInfoProvider();
			var jobHeadersToDispose = new HashSet<Job>() { job1, job2 };

			AssertEquals("Pre-condition", 0, invoice.ConsolCosting.ConsolCosts.Count);
			InvoiceLinesToConsolCostConvertor.ConvertToConsolCostRelatedLines(Factory, invoice, false, linesForConsolCostByConsols.ToArray(), linesInfoProvider, jobHeadersToDispose);

			if (isMatchPKEnabled || isMatchDisplaySequenceEnabled)
			{
				AssertEquals(2, invoice.ConsolCosting.ConsolCosts.Count);

				var invoiceChargeInfos1 = new List<(Job relatedJob, ZDecimal expectedAmount, ApportionSplitCharge expectedRelatedApportionCharge)>
				{
					(job1, 3m, jobCharge1),
					(job2, 0m, null)
				};
				AssertInvoiceConsolCost(invoice.ConsolCosting.ConsolCosts[0], consolCost1, invoiceChargeInfos1);

				var invoiceChargeInfos2 = new List<(Job relatedJob, ZDecimal expectedAmount, ApportionSplitCharge expectedRelatedApportionCharge)>
				{
					(job1, 0m, null),
					(job2, 5m, jobCharge2)
				};
				AssertInvoiceConsolCost(invoice.ConsolCosting.ConsolCosts[1], consolCost2, invoiceChargeInfos2);
			}
			else
			{
				AssertEquals(1, invoice.ConsolCosting.ConsolCosts.Count);
				var invoiceChargeInfos = new List<(Job relatedJob, ZDecimal expectedAmount, ApportionSplitCharge expectedRelatedApportionCharge)>
				{
					(job1, 3m, null),
					(job2, 5m, null)
				};
				AssertInvoiceConsolCost(invoice.ConsolCosting.ConsolCosts[0], null, invoiceChargeInfos);
			}
		}

		void AssertConvertToConsolCostRelatedLines_MatchApportionCharge_MapMultipleConsols(bool isMatchPKEnabled)
		{
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var consol1 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var jobs = CreateShipmentJobsForConsol(consol1, new List<(ZDecimal, ZString)> { (200m, "S001"), (300m, "S002") });
			var (job1, job2) = (jobs[0], jobs[1]);

			var apps1 = new ApportionmentListing(Factory, consol1);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.FRT, TestObjectCreator.AALSHI, apps1);
			consolCost1.E6_OSCostAmount = 2000m;

			var consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C002");
			var job3 = CreateShipmentJobsForConsol(consol2, new List<(ZDecimal, ZString)> { (500m, "S003") })[0];
			var apps2 = new ApportionmentListing(Factory, consol2);
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.FRT, TestObjectCreator.AALSHI, apps2);
			consolCost2.E6_OSCostAmount = 1500m;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "T001", organisation: TestObjectCreator.Creditor1);
			var invoiceLine1 = TestObjectCreator.CreateInvoiceLine(invoice, job1, TestObjectCreator.FRT, 3m, TestObjectCreator.AUD);
			var invoiceLine2 = TestObjectCreator.CreateInvoiceLine(invoice, job2, TestObjectCreator.FRT, 5m, TestObjectCreator.AUD);
			invoice.SubmittedFromInvoicingForm = true;

			Factory.Save();

			var jobCharge1 = consolCost1.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job1.PK);
			var jobCharge3 = consolCost2.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job3.PK);
			if (isMatchPKEnabled)
			{
				var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();
				MapLineWithApportionedChargeAndMatchingCriteria("PrimaryKey", invoiceLine1.PK, jobCharge1.PK, additionalInfoProvider);
				MapLineWithApportionedChargeAndMatchingCriteria("PrimaryKey", invoiceLine2.PK, jobCharge3.PK, additionalInfoProvider);
				Factory.ServiceContainer.AddService(additionalInfoProvider);
			}

			var invoicingLineBaseList = new List<InvoicingLineBase>() { invoiceLine1, invoiceLine2 };
			var linesForConsolCostByConsols = new List<(IJobCostingPlugIn, IEnumerable<InvoicingLineBase>)>()
				{ ValueTuple.Create(consol1 as IJobCostingPlugIn, invoicingLineBaseList as IEnumerable<InvoicingLineBase>) };
			var linesInfoProvider = new LinesInfoProvider();
			var jobHeadersToDispose = new HashSet<Job>() { job1, job2, job3 };

			AssertEquals("Pre-condition", 0, invoice.ConsolCosting.ConsolCosts.Count);
			InvoiceLinesToConsolCostConvertor.ConvertToConsolCostRelatedLines(Factory, invoice, false, linesForConsolCostByConsols.ToArray(), linesInfoProvider, jobHeadersToDispose);

			if (isMatchPKEnabled)
			{
				AssertEquals(2, invoice.ConsolCosting.ConsolCosts.Count);

				var invoiceChargeInfos1 = new List<(Job relatedJob, ZDecimal expectedAmount, ApportionSplitCharge expectedRelatedApportionCharge)>
				{
					(job1, 3m, jobCharge1),
					(job2, 0m, null)
				};
				AssertInvoiceConsolCost(invoice.ConsolCosting.ConsolCosts[0], consolCost1, invoiceChargeInfos1);

				var invoiceChargeInfos2 = new List<(Job relatedJob, ZDecimal expectedAmount, ApportionSplitCharge expectedRelatedApportionCharge)>
				{
					(job1, 0m, null),
					(job2, 5m, null)
				};
				AssertInvoiceConsolCost(invoice.ConsolCosting.ConsolCosts[1], null, invoiceChargeInfos2);
			}
			else
			{
				AssertEquals(1, invoice.ConsolCosting.ConsolCosts.Count);
				var invoiceChargeInfos = new List<(Job relatedJob, ZDecimal expectedAmount, ApportionSplitCharge expectedRelatedApportionCharge)>
				{
					(job1, 3m, null),
					(job2, 5m, null)
				};
				AssertInvoiceConsolCost(invoice.ConsolCosting.ConsolCosts[0], null, invoiceChargeInfos);
			}
		}

		void AssertConvertToConsolCostRelatedLines_MatchApportionCharge_MixedCriteria(bool isMatchPKEnabled, bool isMatchDisplaySequenceEnabled)
		{
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var jobs = CreateShipmentJobsForConsol(consol, new List<(ZDecimal, ZString)> { (200m, "S001"), (300m, "S002") });
			var (job1, job2) = (jobs[0], jobs[1]);

			var apps1 = new ApportionmentListing(Factory, consol);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI, apps1);
			consolCost1.E6_OSCostAmount = 2000m;
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI, apps1);
			consolCost2.E6_OSCostAmount = 1500m;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "T001", organisation: TestObjectCreator.Creditor1);
			var invoiceLine1 = TestObjectCreator.CreateInvoiceLine(invoice, job1, TestObjectCreator.FRT, 3m, TestObjectCreator.AUD);
			var invoiceLine2 = TestObjectCreator.CreateInvoiceLine(invoice, job2, TestObjectCreator.FRT, 5m, TestObjectCreator.AUD);
			invoice.SubmittedFromInvoicingForm = true;

			Factory.Save();

			var jobCharge1 = consolCost1.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job1.PK);
			var jobCharge2 = consolCost1.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job2.PK);
			var jobCharge3 = consolCost2.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job1.PK);
			var jobCharge4 = consolCost2.ApportionmentCharges.Cast<ApportionSplitCharge>().Single(x => x.JR_JH == job2.PK);

			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();
			if (isMatchPKEnabled)
			{
				MapLineWithApportionedChargeAndMatchingCriteria("PrimaryKey", invoiceLine1.PK, jobCharge1.PK, additionalInfoProvider);
				MapLineWithApportionedChargeAndMatchingCriteria("PrimaryKey", invoiceLine2.PK, jobCharge2.PK, additionalInfoProvider);
			}
			if (isMatchDisplaySequenceEnabled)
			{
				MapLineWithDisplaySequenceAndMatchingCriteria("DisplaySequence", invoiceLine1.PK, new ZShort(2), additionalInfoProvider);
				MapLineWithDisplaySequenceAndMatchingCriteria("DisplaySequence", invoiceLine2.PK, new ZShort(2), additionalInfoProvider);
			}
			Factory.ServiceContainer.AddService(additionalInfoProvider);

			var invoicingLineBaseList = new List<InvoicingLineBase>() { invoiceLine1, invoiceLine2 };
			var linesForConsolCostByConsols = new List<(IJobCostingPlugIn, IEnumerable<InvoicingLineBase>)>()
				{ ValueTuple.Create(consol as IJobCostingPlugIn, invoicingLineBaseList as IEnumerable<InvoicingLineBase>) };
			var linesInfoProvider = new LinesInfoProvider();
			var jobHeadersToDispose = new HashSet<Job>() { job1, job2 };

			AssertEquals("Pre-condition", 0, invoice.ConsolCosting.ConsolCosts.Count);
			InvoiceLinesToConsolCostConvertor.ConvertToConsolCostRelatedLines(Factory, invoice, false, linesForConsolCostByConsols.ToArray(), linesInfoProvider, jobHeadersToDispose);

			AssertEquals(1, invoice.ConsolCosting.ConsolCosts.Count);

			if (isMatchPKEnabled)
			{
				var invoiceChargeInfos = new List<(Job relatedJob, ZDecimal expectedAmount, ApportionSplitCharge expectedRelatedApportionCharge)>
				{
					(job1, 3m, jobCharge1),
					(job2, 5m, jobCharge2)
				};
				AssertInvoiceConsolCost(invoice.ConsolCosting.ConsolCosts[0], consolCost1, invoiceChargeInfos);
			}
			else if (isMatchDisplaySequenceEnabled)
			{
				var invoiceChargeInfos = new List<(Job relatedJob, ZDecimal expectedAmount, ApportionSplitCharge expectedRelatedApportionCharge)>
				{
					(job1, 3m, jobCharge3),
					(job2, 5m, jobCharge4)
				};
				AssertInvoiceConsolCost(invoice.ConsolCosting.ConsolCosts[0], consolCost2, invoiceChargeInfos);
			}
			else
			{
				var invoiceChargeInfos = new List<(Job relatedJob, ZDecimal expectedAmount, ApportionSplitCharge expectedRelatedApportionCharge)>
				{
					(job1, 3m, null),
					(job2, 5m, null)
				};
				AssertInvoiceConsolCost(invoice.ConsolCosting.ConsolCosts[0], null, invoiceChargeInfos);
			}
		}

		public void TestConvertToConsolCostRelatedLines_EnableCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting()
		{
			AssertCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting(true);
		}

		public void TestConvertToConsolCostRelatedLines_DisableCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting()
		{
			AssertCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting(false);
		}

		void AssertCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting(bool enableCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting)
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment = TestObjectCreator.CreateShipment("S001001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S001002", consol);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var jobForShipment2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var apps = new ApportionmentListing(Factory, consol);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, apps);
			consolCost.E6_OSCostAmount = 31m;

			TestObjectCreator.CC1.AC_Desc = "Default Charge Code description";
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "T001", organisation: TestObjectCreator.Creditor1);
			var invoiceLineOverrideLineDescription = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 5m, TestObjectCreator.AUD);
			invoiceLineOverrideLineDescription.AL_Desc = "Override Charge Code description S001001";

			var invoiceLineOverrideLineDescriptionForShipment2 = TestObjectCreator.CreateInvoiceLine(invoice, jobForShipment2, TestObjectCreator.CC1, 5m, TestObjectCreator.AUD);
			invoiceLineOverrideLineDescriptionForShipment2.AL_Desc = "Override Charge Code description S001002";
			invoice.SubmittedFromInvoicingForm = true;

			Factory.Save();

			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();
			additionalInfoProvider.MapTransactionLineWithConsolCost(invoiceLineOverrideLineDescription.PK, consolCost.PK);
			Factory.ServiceContainer.AddService(additionalInfoProvider);

			var invoicingLineBaseList = new List<InvoicingLineBase>() { invoiceLineOverrideLineDescription, invoiceLineOverrideLineDescriptionForShipment2 };
			var linesForConsolCostByConsols = new List<(IJobCostingPlugIn, IEnumerable<InvoicingLineBase>)>()
				{ ValueTuple.Create(consol as IJobCostingPlugIn, invoicingLineBaseList as IEnumerable<InvoicingLineBase>) };
			var linesInfoProvider = new LinesInfoProvider();
			var jobHeadersToDispose = new HashSet<Job>() { job };

			AssertEquals("Pre-condition", 0, invoice.ConsolCosting.ConsolCosts.Count);
			var electronicProcessingChargeProviderMock = new Mock<IElectronicProcessingChargeProvider>();
			electronicProcessingChargeProviderMock.Setup(x => x.ShouldCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting(It.IsAny<ZGuid>(), It.IsAny<ZGuid>())).Returns(enableCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting);

			using (ObjectFactory.Substitute(electronicProcessingChargeProviderMock.Object))
			{
				InvoiceLinesToConsolCostConvertor.ConvertToConsolCostRelatedLines(Factory, invoice, false, linesForConsolCostByConsols.ToArray(), linesInfoProvider, jobHeadersToDispose);
			}

			AssertEquals(1, invoice.ConsolCosting.ConsolCosts.Count);

			var invoiceConsolCost = invoice.ConsolCosting.ConsolCosts[0];
			AssertEquals(2, invoiceConsolCost.ApportionmentCharges.Count);

			var chargesOnInvoiceForShipment1 = invoiceConsolCost.ApportionmentCharges.OfType<ApportionSplitCharge>().FirstOrDefault(x => x.JR_JH == shipment.Job.PK);
			var chargesOnInvoiceForShipment2 = invoiceConsolCost.ApportionmentCharges.OfType<ApportionSplitCharge>().FirstOrDefault(x => x.JR_JH == shipment2.Job.PK);

			AssertEquals(enableCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting ? "Override Charge Code description S001001" : "Default Charge Code description", chargesOnInvoiceForShipment1.JR_Desc);
			AssertEquals(enableCarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting ? "Override Charge Code description S001002" : "Default Charge Code description", chargesOnInvoiceForShipment2.JR_Desc);
		}

		void AssertInvoiceConsolCost(JobConsolCost invoiceConsolCost, JobConsolCost expectedRelatedConsolCost, IEnumerable<(Job relatedJob, ZDecimal expectedAmount, ApportionSplitCharge expectedRelatedApportionCharge)> invoiceChargeInfos)
		{
			AssertEquals(expectedRelatedConsolCost?.PK ?? ZGuid.Empty, invoiceConsolCost.RelatedConsolCostPK);
			AssertEquals(invoiceChargeInfos.Count(), invoiceConsolCost.ApportionmentCharges.Count);
			AssertEquals(invoiceChargeInfos.Sum(x => x.expectedAmount), invoiceConsolCost.CostAmount);

			var invoiceConsolCostCharges = invoiceConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList();
			foreach (var invoiceChargeInfo in invoiceChargeInfos)
			{
				var invoiceChargeRelatedToJob = invoiceConsolCostCharges.Single(x => x.JR_JH == invoiceChargeInfo.relatedJob.PK);
				AssertEquals(invoiceChargeInfo.expectedAmount, invoiceChargeRelatedToJob.CostAmount);
				AssertEquals(invoiceChargeInfo.expectedRelatedApportionCharge?.PK, invoiceChargeRelatedToJob.RelatedApportionChargeFromDB?.PK);
			}
		}

		IList<Job> CreateShipmentJobsForConsol(ForwardingConsol consol, IEnumerable<(ZDecimal weight, ZString num)> shipmentInfos)
		{
			var jobs = new List<Job>();
			foreach (var shipmentInfo in shipmentInfos)
			{
				var shipment = consol.Shipments.AddNew();
				shipment.JS_ActualWeight = shipmentInfo.weight;
				shipment.JS_UniqueConsignRef = shipmentInfo.num;
				var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job;

				jobs.Add(job);
			}

			return jobs;
		}

		(ForwardingConsol consol, Job job) PrepareTestData()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			return (consol, job);
		}

		public void TestCreateSeparateConsolCostBasedOnEachLine_WhenLineMatchedToConsolCost_EnableRegistry()
		{
			AssertCreateSeparateConsolCostBasedOnEachLine_WhenLineMatchedToConsolCost(true);
		}

		public void TestCreateSeparateConsolCostBasedOnEachLine_WhenLineMatchedToConsolCost_DisableRegistry()
		{
			AssertCreateSeparateConsolCostBasedOnEachLine_WhenLineMatchedToConsolCost(false);
		}

		void AssertCreateSeparateConsolCostBasedOnEachLine_WhenLineMatchedToConsolCost(bool enableXUTImportAutoMapAccrualFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableXUTImportAutoMapAccrualFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enableXUTImportAutoMapAccrualFeature);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment1 = TestObjectCreator.CreateShipment("S001001", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var shipment2 = TestObjectCreator.CreateShipment("S001002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var apps = new ApportionmentListing(Factory, consol);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.Creditor1, apps);
			consolCost1.E6_OSCostAmount = 31m;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "T001", organisation: TestObjectCreator.Creditor1);
			var invoiceLine1 = TestObjectCreator.CreateInvoiceLine(invoice, job1, TestObjectCreator.CC1, 5m, TestObjectCreator.AUD);
			invoice.SubmittedFromInvoicingForm = true;

			Factory.Save();

			var additionalInfoProvider = new TransactionImportAdditionalInfoProvider();
			additionalInfoProvider.MapTransactionLineWithConsolCost(invoiceLine1.PK, consolCost1.PK);
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(invoiceLine1.PK, new[] { new MatchingCriteria { FieldName = "PrimaryKey", Value = invoiceLine1.PK.ToString() } });
			Factory.ServiceContainer.AddService(additionalInfoProvider);

			var linesByConsolsToCreateConsolCostsBySingleLine = new List<(IJobCostingPlugIn, InvoicingLineBase)>()
				{ ValueTuple.Create(consol as IJobCostingPlugIn, invoiceLine1) };
			var linesInfoProvider = new LinesInfoProvider();
			var jobHeadersToDispose = new HashSet<Job>() { job1, job2 };

			AssertEquals("Pre-condition", 0, invoice.ConsolCosting.ConsolCosts.Count);
			InvoiceLinesToConsolCostConvertor.CreateSeparateConsolCostBasedOnEachLine(invoice, linesByConsolsToCreateConsolCostsBySingleLine.ToArray()
				, linesInfoProvider, jobHeadersToDispose);

			AssertEquals(1, invoice.ConsolCosting.ConsolCosts.Count);

			var invoiceConsolCost = invoice.ConsolCosting.ConsolCosts[0];
			AssertEquals(2, invoiceConsolCost.ApportionmentCharges.Count);
			AssertEquals(5m, invoiceConsolCost.CostAmount);

			var chargesOnInvoice = invoiceConsolCost.ApportionmentCharges.OfType<ApportionSplitCharge>();
			var chargesOnConsol = consolCost1.ApportionmentCharges.OfType<ApportionSplitCharge>();

			if (enableXUTImportAutoMapAccrualFeature)
			{
				AssertEquals("RelatedConsolCostPK should be set", consolCost1.PK, invoiceConsolCost.RelatedConsolCostPK);
				AssertEquals("RelatedApportionChargeFromDB should be set", chargesOnConsol.Single(x => x.JR_JH == job1.PK).PK, chargesOnInvoice.Single(x => x.JR_JH == job1.PK).RelatedApportionChargeFromDB.PK);
				AssertEquals("RelatedApportionChargeFromDB should be set", chargesOnConsol.Single(x => x.JR_JH == job2.PK).PK, chargesOnInvoice.Single(x => x.JR_JH == job2.PK).RelatedApportionChargeFromDB.PK);
			}
			else
			{
				AssertEquals("RelatedConsolCostPK shouldn't be set", ZGuid.Empty, invoiceConsolCost.RelatedConsolCostPK);
				AssertEquals("RelatedApportionChargeFromDB shouldn't be set", true, chargesOnInvoice.All(x => x.RelatedApportionChargeFromDB == null));
			}
		}

		void MapLineWithApportionedChargeAndMatchingCriteria(string matchingCriteria, ZGuid linePK, ZGuid apportionedChargePK, TransactionImportAdditionalInfoProvider additionalInfoProvider)
		{
			additionalInfoProvider.MapTransactionLineWithApportionedCharge(linePK, apportionedChargePK);
			var primaryKeyMatchingCriteria = new MatchingCriteria { FieldName = matchingCriteria, Value = apportionedChargePK.ToString() };
			var mappedMatchingCriteriaCollection = additionalInfoProvider.GetMatchingCriteriaCollection(linePK)
				?.Append(primaryKeyMatchingCriteria)
				?? new[] { primaryKeyMatchingCriteria };
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(linePK, mappedMatchingCriteriaCollection);
		}

		void MapLineWithDisplaySequenceAndMatchingCriteria(string matchingCriteria, ZGuid linePK, ZShort displaySequence, TransactionImportAdditionalInfoProvider additionalInfoProvider)
		{
			additionalInfoProvider.MapTransactionLineWithApportionedChargeDisplaySequence(linePK, displaySequence);
			var displaySequenceMatchingCriteria = new MatchingCriteria { FieldName = matchingCriteria, Value = displaySequence.ToString() };
			var mappedMatchingCriteriaCollection = additionalInfoProvider.GetMatchingCriteriaCollection(linePK)
				?.Append(displaySequenceMatchingCriteria)
				?? new[] { displaySequenceMatchingCriteria };
			additionalInfoProvider.MapTransactionLineWithMatchingCriteriaCollection(linePK, mappedMatchingCriteriaCollection);
		}
	}
}
