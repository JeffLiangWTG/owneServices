using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceConsolCosting;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceConsolSummaryCollection))]
	public class APInvoiceConsolSummaryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<APInvoiceConsolSummaryCollection>
	{
		protected override APInvoiceConsolSummaryCollection GetCollectionToTest()
		{
			return new APInvoiceConsolSummaryCollection(new APInvoiceConsolCosting(Factory, ParentInvoice));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			return new APInvoiceConsolSummary(consol, ParentInvoice);
		}

		#region Implementation

		APInvoice fParentInvoice;
		APInvoice ParentInvoice
		{
			get { return fParentInvoice ?? (fParentInvoice = Factory.New<APInvoice>()); }
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion

		public void TestUpdate_TaxDate()
		{
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.USD, 2);
			var summaryCollection = new APInvoiceConsolSummaryCollection(invoice.ConsolCosting);

			var consolCost = TestObjectCreator.CreateConsolCost(invoice, consol, TestObjectCreator.CC1, 100);
			consolCost.E6_RX_NKCurrency = TestObjectCreator.GBP.Code;
			consolCost.E6_ExchangeRate = 5;
			consolCost.E6_AT_TaxRate = TestObjectCreator.GSTANDQST1WithDates.PK;

			AssertEquals("Precondition: summaryCollection.Count", 0, summaryCollection.Count);
			summaryCollection.Update();
			AssertEquals("Postcondition: summaryCollection.Count", 1, summaryCollection.Count);
			AssertEquals("LocalTotalTaxAmount", 3m, summaryCollection[0].LocalTotalTaxAmount);
			var previousValue = summaryCollection[0].LocalTotalTaxAmount;

			consolCost.E6_TaxDate = TestObjectCreator.GSTANDQST1WithDates_DateWithNoExtraRate;
			AssertEquals("Precondition: LocalTotalTaxAmount", previousValue, summaryCollection[0].LocalTotalTaxAmount);
			summaryCollection.Update();
			AssertEquals("Postcondition: summaryCollection.Count", 1, summaryCollection.Count);
			AssertNotEquals("Postcondition: LocalTotalTaxAmount", previousValue, summaryCollection[0].LocalTotalTaxAmount);
			AssertEquals("LocalTotalTaxAmount", 0.4m, summaryCollection[0].LocalTotalTaxAmount);
		}

		public void TestUpdateWithUseJobExchangeRate()
		{
			TestObjectCreator testObjCreator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();

			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_RX_NKTransactionCurrency = "GBP";
			invoice.AH_ExchangeRate = 1m;
			invoice.AH_PostedToEFT = true; // Use Job Exchange Rate = Y

			APInvoiceConsolCosting costing = new APInvoiceConsolCosting(Factory, invoice);
			APInvoiceConsolSummaryCollection summaryCollection = new APInvoiceConsolSummaryCollection(costing);

			JobConsolCost cost1 = costing.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost1.E6_AC_ChargeCode = testObjCreator.CC1.PK;
			cost1.E6_RX_NKCurrency = "GBP";
			cost1.E6_ExchangeRate = 0.5m;
			cost1.E6_OSCostAmount = 100m;
			cost1.E6_AT_TaxRate = testObjCreator.GSTFREE1.PK;

			JobConsolCost cost2 = costing.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost2.E6_AC_ChargeCode = testObjCreator.CC3.PK;
			cost2.E6_RX_NKCurrency = "GBP";
			cost2.E6_ExchangeRate = 0.5m;
			cost2.E6_OSCostAmount = 50m;
			cost2.E6_AT_TaxRate = testObjCreator.GST1.PK;

			summaryCollection.Update();

			AssertEquals("InvoiceCurrencyTotalAmount", 150m, summaryCollection[0].InvoiceCurrencyTotalAmount);
			AssertEquals("InvoiceCurrencyTotalTaxAmount", 5m, summaryCollection[0].InvoiceCurrencyTotalTaxAmount);
			AssertEquals("InvoiceCurrencyTotalAmountWithTax", 155m, summaryCollection[0].InvoiceCurrencyTotalAmountWithTax);

			AssertEquals("LocalTotalAmount", 300m, summaryCollection[0].LocalTotalAmount);
			AssertEquals("LocalTotalTaxAmount", 10m, summaryCollection[0].LocalTotalTaxAmount);
			AssertEquals("LocalTotalAmountWithTax", 310m, summaryCollection[0].LocalTotalAmountWithTax);
		}

		public void TestUpdateWithoutUseJobExchangeRate()
		{
			TestObjectCreator testObjCreator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();

			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_RX_NKTransactionCurrency = "GBP";
			invoice.AH_ExchangeRate = 1m;
			invoice.AH_PostedToEFT = false; // Use Job Exchange Rate = Y

			APInvoiceConsolCosting costing = new APInvoiceConsolCosting(Factory, invoice);
			APInvoiceConsolSummaryCollection summaryCollection = new APInvoiceConsolSummaryCollection(costing);

			JobConsolCost cost1 = costing.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost1.E6_AC_ChargeCode = testObjCreator.CC1.PK;
			cost1.E6_RX_NKCurrency = "GBP";
			cost1.E6_ExchangeRate = 0.5m;
			cost1.E6_OSCostAmount = 100m;
			cost1.E6_AT_TaxRate = testObjCreator.GSTFREE1.PK;

			JobConsolCost cost2 = costing.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost2.E6_AC_ChargeCode = testObjCreator.CC3.PK;
			cost2.E6_RX_NKCurrency = "GBP";
			cost2.E6_ExchangeRate = 0.5m;
			cost2.E6_OSCostAmount = 50m;
			cost2.E6_AT_TaxRate = testObjCreator.GST1.PK;

			summaryCollection.Update();

			AssertEquals("InvoiceCurrencyTotalAmount", 300m, summaryCollection[0].InvoiceCurrencyTotalAmount);
			AssertEquals("InvoiceCurrencyTotalTaxAmount", 10m, summaryCollection[0].InvoiceCurrencyTotalTaxAmount);
			AssertEquals("InvoiceCurrencyTotalAmountWithTax", 310m, summaryCollection[0].InvoiceCurrencyTotalAmountWithTax);

			AssertEquals("LocalTotalAmount", 300m, summaryCollection[0].LocalTotalAmount);
			AssertEquals("LocalTotalTaxAmount", 10m, summaryCollection[0].LocalTotalTaxAmount);
			AssertEquals("LocalTotalAmountWithTax", 310m, summaryCollection[0].LocalTotalAmountWithTax);
		}

		public void TestUpdateSuspender()
		{
			TestObjectCreator testObjCreator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_RX_NKTransactionCurrency = "GBP";
			invoice.AH_ExchangeRate = 1m;
			invoice.AH_PostedToEFT = false; // Use Job Exchange Rate = Y

			APInvoiceConsolCosting costing = new APInvoiceConsolCosting(Factory, invoice);
			APInvoiceConsolSummaryCollection summaryCollection = new APInvoiceConsolSummaryCollection(costing);

			JobConsolCost cost1 = costing.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost1.E6_AC_ChargeCode = testObjCreator.CC1.PK;
			cost1.E6_RX_NKCurrency = "GBP";
			cost1.E6_ExchangeRate = 0.5m;
			cost1.E6_OSCostAmount = 100m;
			cost1.E6_AT_TaxRate = testObjCreator.GSTFREE1.PK;

			JobConsolCost cost2 = costing.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost2.E6_AC_ChargeCode = testObjCreator.CC3.PK;
			cost2.E6_RX_NKCurrency = "GBP";
			cost2.E6_ExchangeRate = 0.5m;
			cost2.E6_OSCostAmount = 50m;
			cost2.E6_AT_TaxRate = testObjCreator.GST1.PK;

			AssertEquals("Pre-condition: Summary collection should be empty", 0, summaryCollection.Count);

			using (summaryCollection.UpdateSuspender.GetSuspender())
			{
				summaryCollection.Update();

				AssertEquals("Summary collection should not be updated", 0, summaryCollection.Count);
			}

			AssertEquals("Summary collection should be updated now", 1, summaryCollection.Count);

			AssertEquals("InvoiceCurrencyTotalAmount", 300m, summaryCollection[0].InvoiceCurrencyTotalAmount);
			AssertEquals("InvoiceCurrencyTotalTaxAmount", 10m, summaryCollection[0].InvoiceCurrencyTotalTaxAmount);
			AssertEquals("InvoiceCurrencyTotalAmountWithTax", 310m, summaryCollection[0].InvoiceCurrencyTotalAmountWithTax);

			AssertEquals("LocalTotalAmount", 300m, summaryCollection[0].LocalTotalAmount);
			AssertEquals("LocalTotalTaxAmount", 10m, summaryCollection[0].LocalTotalTaxAmount);
			AssertEquals("LocalTotalAmountWithTax", 310m, summaryCollection[0].LocalTotalAmountWithTax);
		}
	}
}
