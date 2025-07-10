using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	class InvoicingBaseBehaviorTest : TestCaseWithFactory
	{
		[TestDate(2015, 2, 10)]
		public void TestImportJobChargesIntoInvoiceWhenExchangeRateChangedByInvoiceDateWithReciprocal()
		{
			Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).GC_IsReciprocal = true;
			Factory.Save();

			PrepareChargeTestDataImportJobChargesIntoInvoice();

			using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code))
			using (AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				PrepareInvoiceTestDataForImportJobChargesIntoInvoice();

				AssertEquals("Precondition", 5m, charge.CostExchangeRate.Rate);
				AssertEquals(150m, charge.JR_LocalCostAmt);
				AssertEquals(30m, charge.JR_OSCostAmt);
				AssertEquals(27m, charge.JR_Cost_LocalGSTAmount);
				AssertEquals(true, GlbCompany.CurrentCompany.GC_IsReciprocal);
				AssertEquals(true, invoice.AH_PostedToEFT);
				AssertEquals(invoice.AH_RX_NKTransactionCurrency, charge.JR_CostCurrency);

				invoice.ImportJobChargesIntoInvoice(TestObjectCreator.Job1.Charges.ToArray<Charge>(), line, false);

				AssertEquals(@"Local tax amount is calculated by: 'OS-excludate-tax-amount' x 'tax-rate' x 'exchange-rate'",
							 34.56m, invoice.AH_LocalTaxAmount);
				AssertEquals(5.4m, invoice.AH_OSTaxAmount);
				AssertEquals(192m, invoice.AH_LocalExTaxAmount);
				AssertEquals(30m, invoice.AH_OSExTaxAmount);
				AssertEquals(6.4m, invoice.ExchangeRate.Rate);
			}
		}

		[TestDate(2015, 2, 10)]
		public void TestImportJobChargesIntoInvoiceWhenExchangeRateChangedByInvoiceDateNoneReciprocal()
		{
			PrepareChargeTestDataImportJobChargesIntoInvoice(0.15625m, 0.2m);

			using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code))
			using (AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				PrepareInvoiceTestDataForImportJobChargesIntoInvoice();

				AssertEquals("Precondition", 0.2m, charge.CostExchangeRate.Rate);
				AssertEquals(150m, charge.JR_LocalCostAmt);
				AssertEquals(30m, charge.JR_OSCostAmt);
				AssertEquals(27m, charge.JR_Cost_LocalGSTAmount);
				AssertEquals(false, GlbCompany.CurrentCompany.GC_IsReciprocal);
				AssertEquals(true, invoice.AH_PostedToEFT);
				AssertEquals(invoice.AH_RX_NKTransactionCurrency, charge.JR_CostCurrency);

				invoice.ImportJobChargesIntoInvoice(TestObjectCreator.Job1.Charges.ToArray<Charge>(), line, false);

				AssertEquals(@"Local tax amount is calculated by: 'OS-excludate-tax-amount' x 'tax-rate' x 'exchange-rate'",
							 34.56m, invoice.AH_LocalTaxAmount);
				AssertEquals(5.4m, invoice.AH_OSTaxAmount);
				AssertEquals(192m, invoice.AH_LocalExTaxAmount);
				AssertEquals(30m, invoice.AH_OSExTaxAmount);
				AssertEquals(0.15625m, invoice.ExchangeRate.Rate);
			}
		}

		[TestDate(2015, 2, 10)]
		public void TestImportJobChargesIntoInvoiceWhenExchangeRateChangedByInvoiceDateWithReciprocalButNotPostToEFT()
		{
			Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).GC_IsReciprocal = true;
			Factory.Save();

			PrepareChargeTestDataImportJobChargesIntoInvoice();

			using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code))
			{
				PrepareInvoiceTestDataForImportJobChargesIntoInvoice();

				AssertEquals("Precondition", 5m, charge.CostExchangeRate.Rate);
				AssertEquals(150m, charge.JR_LocalCostAmt);
				AssertEquals(30m, charge.JR_OSCostAmt);
				AssertEquals(27m, charge.JR_Cost_LocalGSTAmount);
				AssertEquals(true, GlbCompany.CurrentCompany.GC_IsReciprocal);
				AssertEquals(false, invoice.AH_PostedToEFT);
				AssertEquals(invoice.AH_RX_NKTransactionCurrency, charge.JR_CostCurrency);

				invoice.ImportJobChargesIntoInvoice(TestObjectCreator.Job1.Charges.ToArray<Charge>(), line, false);

				AssertEquals(27.01m, invoice.AH_LocalTaxAmount);
				AssertEquals(4.22m, invoice.AH_OSTaxAmount);
				AssertEquals(150.02m, invoice.AH_LocalExTaxAmount);
				AssertEquals(@"Local exclude tax amount is calculated by: 'OS-exclude-tax-amount' / 'Exchange Rate'",
							 23.44m, invoice.AH_OSExTaxAmount);
				AssertEquals(6.4m, invoice.ExchangeRate.Rate);
			}
		}

		[TestDate(2015, 2, 10)]
		public void TestImportJobChargesIntoLocalCurrencyInvoiceWithPostDateExchangeRate()
		{
			var invoice = TestImportJobChargesIntoLocalCurrencyInvoiceCore(AccountingConstants.InvoicePostingExchangeRateOption.Default.Code);

			var line = invoice.Lines[0];
			AssertEquals("Import Charge with Post date exchange rate when Header has local currency and Charge has foreign currency.", CurrencyCodes.EuropeanUnion, line.ExchangeRate.Currency);
			AssertEquals(75m, line.AL_OSExTaxAmount);
			AssertEquals(60m, line.AL_LocalExTaxAmount);
			AssertEquals(1.25m, line.ExchangeRate.Rate);
			AssertEquals(7.5m, line.AL_OSTaxAmount);
			AssertEquals(6m, line.AL_LocalTaxAmount);
			AssertEquals("expect to use shipment's ARV date", new ZDate(2015, 2, 8), line.AL_TaxDate);
		}

		[TestDate(2015, 2, 10)]
		public void TestImportJobChargesIntoLocalCurrencyInvoiceWithInvoiceDateExchangeRate()
		{
			var invoice = TestImportJobChargesIntoLocalCurrencyInvoiceCore(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);

			AssertEquals("Import Charge with Invoice date exchange rate when Header has local currency and Charge has foreign currency.", CurrencyCodes.EuropeanUnion, invoice.Lines[0].ExchangeRate.Currency);

			var line = invoice.Lines[0];
			AssertEquals(75m, line.AL_OSExTaxAmount);
			AssertEquals(50m, line.AL_LocalExTaxAmount);
			AssertEquals(1.5m, line.ExchangeRate.Rate);
			AssertEquals(7.5m, line.AL_OSTaxAmount);
			AssertEquals(5m, line.AL_LocalTaxAmount);
			AssertEquals("expect to use shipment's ARV date", new ZDate(2015, 2, 8), line.AL_TaxDate);
		}

		InvoicingBase TestImportJobChargesIntoLocalCurrencyInvoiceCore(string exchangeRateOption)
		{
			var shipmentExchangeRate = 1.25m;
			var localCurrency = TestObjectCreator.CNY;
			var otherCurrency = TestObjectCreator.EUR;
			var localRate = 1m;
			var todayRate = 2m;
			var yesterdayRate = 1.5m;

			TestObjectCreator.CreateExchangeRate(localCurrency, ExchangeRateTypes.Code.BuyRate, localRate);
			TestObjectCreator.CreateExchangeRate(otherCurrency, ExchangeRateTypes.Code.BuyRate, todayRate, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateExchangeRate(otherCurrency, ExchangeRateTypes.Code.BuyRate, yesterdayRate, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1));

			var newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.CompanyData.SetAPTaxApplicable(true);

			var forwardingShipment = TestObjectCreator.CreateShipment("S00008800");
			forwardingShipment.JS_E_ARV = ZDateTime.Today.AddDays(-2);
			var shipmentJob = TestObjectCreator.CreateJob(forwardingShipment, false);
			var chargeCode = TestObjectCreator.CC1;
			TestObjectCreator.SetExchangeRate(shipmentJob, TestObjectCreator.EUR, shipmentExchangeRate);
			var importJobCharge = TestObjectCreator.CreateCharge(shipmentJob, chargeCode, "charge1", TestObjectCreator.EUR, 75M, newOrganisation, null, 0M, null);
			importJobCharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "SHP";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AP";
			taxDateOption.TaxDateOption = TaxDateDefaultingOption.Code.ArrivalDate;
			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(TestObjectCreator.CNY.Code))
			using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exchangeRateOption))
			{
				var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
				invoice.AH_InvoiceDate = ZDateTime.Today.AddDays(-1);
				invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.CNY.Code;
				invoice.AH_OH = newOrganisation.PK;
				var jobCharges = new List<Charge>();
				jobCharges.Add(importJobCharge);

				AssertEquals("Precondition", CurrencyCodes.China, invoice.ExchangeRate.Currency);
				AssertEquals(CurrencyCodes.China, GlbCompany.CurrentCompany.LocalCurrency.Code);
				AssertEquals(false, invoice.UseJobExchangeRate);
				AssertEquals(1m, invoice.ExchangeRate.Rate);
				AssertEquals(CurrencyCodes.EuropeanUnion, importJobCharge.CostCurrency.Code);

				var lineFromImportShipment = (InvoicingLineBase)invoice.Lines.AddNew();
				lineFromImportShipment.AL_JH = shipmentJob.PK;
				lineFromImportShipment.AL_AT = TestObjectCreator.GST1.PK;
				invoice.ImportJobChargesIntoInvoice(jobCharges, lineFromImportShipment);

				return invoice;
			}
		}

		[TestDate(2015, 2, 10)]
		public void TestImportJobChargesIntoInvoiceWithForeignCurrencyAndInvoiceDateExchangeRate()
		{
			var invoice = TestImportJobChargesIntoInvoiceWithForeignCurrencyCore(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);

			var line = invoice.Lines[0];
			AssertEquals("Import Charge with Invoice date exchange rate when Header has foreign currency and Charge has another foreign currency.", CurrencyCodes.UnitedStates, line.ExchangeRate.Currency);
			AssertEquals(150m, line.AL_OSExTaxAmount);
			AssertEquals(60m, line.AL_LocalExTaxAmount);
			AssertEquals(2.5m, line.ExchangeRate.Rate);
			AssertEquals(15m, line.AL_OSTaxAmount);
			AssertEquals(6m, line.AL_LocalTaxAmount);
		}

		[TestDate(2015, 2, 10)]
		public void TestImportJobChargesIntoInvoiceWithForeignCurrencyAndPostDateExchangeRate()
		{
			var invoice = TestImportJobChargesIntoInvoiceWithForeignCurrencyCore(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code);

			var line = invoice.Lines[0];
			AssertEquals("Import Charge with Post date exchange rate when Header has foreign currency and Charge has another foreign currency.", CurrencyCodes.UnitedStates, line.ExchangeRate.Currency);
			AssertEquals(300m, line.AL_OSExTaxAmount);
			AssertEquals(60m, line.AL_LocalExTaxAmount);
			AssertEquals(5m, line.ExchangeRate.Rate);
			AssertEquals(30m, line.AL_OSTaxAmount);
			AssertEquals(6m, line.AL_LocalTaxAmount);
		}

		InvoicingBase TestImportJobChargesIntoInvoiceWithForeignCurrencyCore(string exchangeRateOption)
		{
			var shipmentExchangeRate = 1.25m;
			var localCurrency = TestObjectCreator.CNY;
			var otherCurrency1 = TestObjectCreator.EUR;
			var otherCurrency2 = TestObjectCreator.USD;
			var localRate = 1m;
			var todayRate1 = 2m;
			var yesterdayRate1 = 1.5m;
			var todayRate2 = 5m;
			var yesterdayRate2 = 2.5m;

			TestObjectCreator.CreateExchangeRate(localCurrency, ExchangeRateTypes.Code.BuyRate, localRate);
			TestObjectCreator.CreateExchangeRate(otherCurrency1, ExchangeRateTypes.Code.BuyRate, todayRate1, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateExchangeRate(otherCurrency1, ExchangeRateTypes.Code.BuyRate, yesterdayRate1, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1));
			TestObjectCreator.CreateExchangeRate(otherCurrency2, ExchangeRateTypes.Code.BuyRate, todayRate2, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateExchangeRate(otherCurrency2, ExchangeRateTypes.Code.BuyRate, yesterdayRate2, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1));

			var newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.CompanyData.SetAPTaxApplicable(true);

			var forwardingShipment = TestObjectCreator.CreateShipment("S00008800");
			var shipmentJob = TestObjectCreator.CreateJob(forwardingShipment, false);
			TestObjectCreator.SetExchangeRate(shipmentJob, otherCurrency1, shipmentExchangeRate);
			var importJobCharge = TestObjectCreator.CreateCharge(shipmentJob, TestObjectCreator.CC1, "charge1", otherCurrency1, 75M, newOrganisation, null, 0M, null);
			importJobCharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;

			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(localCurrency.Code))
			using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exchangeRateOption))
			{
				var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
				invoice.AH_InvoiceDate = ZDateTime.Today.AddDays(-1);
				invoice.AH_RX_NKTransactionCurrency = otherCurrency2.Code;
				invoice.AH_OH = newOrganisation.PK;
				var jobCharges = new List<Charge>();
				jobCharges.Add(importJobCharge);

				AssertEquals("Precondition", CurrencyCodes.UnitedStates, invoice.ExchangeRate.Currency);
				AssertEquals(CurrencyCodes.China, GlbCompany.CurrentCompany.LocalCurrency.Code);
				AssertEquals(false, invoice.UseJobExchangeRate);
				if (exchangeRateOption == AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code)
				{
					AssertEquals(2.5m, invoice.ExchangeRate.Rate);
				}
				else if (exchangeRateOption == AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code)
				{
					AssertEquals(5m, invoice.ExchangeRate.Rate);
				}
				AssertEquals(CurrencyCodes.EuropeanUnion, importJobCharge.CostCurrency.Code);

				var lineFromImportShipment = (InvoicingLineBase)invoice.Lines.AddNew();
				lineFromImportShipment.AL_JH = shipmentJob.PK;
				lineFromImportShipment.AL_AT = TestObjectCreator.GST1.PK;
				invoice.ImportJobChargesIntoInvoice(jobCharges, lineFromImportShipment);

				return invoice;
			}
		}

		#region Implementation

		void PrepareChargeTestDataImportJobChargesIntoInvoice(decimal oldRate = 6.4m, decimal newRate = 5m)
		{
			TestObjectCreator.GST1.SetRate_ForTestOnly(18, 1);
			ExchangeRateReader.GetReaderInstance().ClearCache();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", oldRate, new DateTime(2015, 1, 1), new DateTime(2015, 1, 31));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "SEL", oldRate, new DateTime(2015, 1, 1), new DateTime(2015, 1, 31));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", newRate, new DateTime(2015, 2, 1), new DateTime(2015, 2, 20));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "SEL", newRate, new DateTime(2015, 2, 1), new DateTime(2015, 2, 20));
			TestObjectCreator.Job1.Parent = TestObjectCreator.GetTestShipmentPlugIn();
			Factory.Save();

			charge = TestObjectCreator.Job1.Charges.AddNew();
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			charge.JR_OSCostAmt = 30m;
			Factory.Save();

			AssertEquals("Precondition", newRate, charge.CostExchangeRate.Rate);
		}

		void PrepareInvoiceTestDataForImportJobChargesIntoInvoice()
		{
			var previousDate = new ZDateTime(2015, 1, 20);

			invoice = Factory.New<APInvoice>();
			invoice.UseJobExchangeRate = true;
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			invoice.AH_InvoiceDate = previousDate;
			invoice.AH_PostDate = previousDate;

			line = (APInvoiceLine)invoice.Lines.AddNew();
		}

		Charge charge;
		InvoicingBase invoice;
		InvoicingLineBase line;

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;

		#endregion
	}
}
