using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Invoicing;
using Enterprise.Accounting.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class InvoiceTaxDateCacheProviderTest : TestCaseWithFactory
	{
		public void TestGetEarliestInvoiceTaxDate()
		{
			var expectedTaxDate = new ZDate(2024, 10, 10);
			AssertInvoiceTaxDate(enablePostTransactionCalculatedPropertyCache: true, expectedTaxDate, (cacheProvider) => cacheProvider.GetEarliestInvoiceTaxDate);
			AssertInvoiceTaxDate(enablePostTransactionCalculatedPropertyCache: false, expectedTaxDate, (cacheProvider) => cacheProvider.GetEarliestInvoiceTaxDate);
		}

		public void TestGetLatestInvoiceTaxDate()
		{
			var expectedTaxDate = new ZDate(2024, 10, 15);
			AssertInvoiceTaxDate(enablePostTransactionCalculatedPropertyCache: true, expectedTaxDate, (cacheProvider) => cacheProvider.GetLatestInvoiceTaxDate);
			AssertInvoiceTaxDate(enablePostTransactionCalculatedPropertyCache: false, expectedTaxDate, (cacheProvider) => cacheProvider.GetLatestInvoiceTaxDate);
		}

		void AssertInvoiceTaxDate(bool enablePostTransactionCalculatedPropertyCache, ZDateTime expectedTaxDate, Func<InvoiceTaxDateCacheProvider, Func<IEnumerable<ITransactionLineTaxDate>, ZDateTime, ZDateTime>> getInvoiceTaxDate)
		{
			AccountingConfigurationRegistry.Instance.EnablePostTransactionCalculatedPropertyCache.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enablePostTransactionCalculatedPropertyCache);
			var apInvoice = SetupAPInvoice();

			var invoiceTaxDate = getInvoiceTaxDate(new InvoiceTaxDateCacheProvider())(apInvoice.Lines.Cast<ITransactionLineTaxDate>(), new ZDateTime(2024, 8, 15));

			AssertEquals(expectedTaxDate, invoiceTaxDate);
		}

		APInvoice SetupAPInvoice()
		{
			var taxDate1 = new ZDate(2024, 10, 15);
			var taxDate2 = new ZDate(2024, 10, 10);
			var taxDate3 = new ZDate(2024, 10, 5);
			var taxDate4 = new ZDate(2024, 9, 30);

			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("0000010", TestObjectCreator.AUD, 1m, 10m, 0m, 0m, 10m, 0m, 0m);
			var lineWithEmptyTaxRate = apInvoice.Lines[0];
			lineWithEmptyTaxRate.AL_AT = ZGuid.Empty;
			lineWithEmptyTaxRate.AL_TaxDate = taxDate4;

			var lineWithCMTCharge = apInvoice.Lines.AddNew();
			var chargeCodeCMT = TestObjectCreator.CreateChargeCode("CMT");
			chargeCodeCMT.AC_ChargeType = ChargeType.Comment;
			lineWithCMTCharge.AL_AC = chargeCodeCMT.PK;
			lineWithCMTCharge.AL_AT = TestObjectCreator.GST11.PK;
			lineWithCMTCharge.AL_TaxDate = taxDate3;

			var lineWithInvalidTaxDate = apInvoice.Lines.AddNew();
			lineWithInvalidTaxDate.AL_AT = TestObjectCreator.GST11.PK;
			lineWithInvalidTaxDate.AL_TaxDate = ZDate.Invalid;

			var lineWithValidTaxDate1 = apInvoice.Lines.AddNew();
			lineWithValidTaxDate1.AL_AT = TestObjectCreator.GST11.PK;
			lineWithValidTaxDate1.AL_TaxDate = taxDate1;

			var lineWithValidTaxDate2 = apInvoice.Lines.AddNew();
			lineWithValidTaxDate2.AL_AT = TestObjectCreator.GST11.PK;
			lineWithValidTaxDate2.AL_TaxDate = taxDate2;

			return apInvoice;
		}

		public void TestGetEarliestInvoiceTaxDate_FallbackToInvoiceDate()
		{
			AssertFallbackToInvoiceDate(enablePostTransactionCalculatedPropertyCache: true, (cacheProvider) => cacheProvider.GetEarliestInvoiceTaxDate);
			AssertFallbackToInvoiceDate(enablePostTransactionCalculatedPropertyCache: false, (cacheProvider) => cacheProvider.GetEarliestInvoiceTaxDate);
		}

		public void TestGetLatestInvoiceTaxDate_FallbackToInvoiceDate()
		{
			AssertFallbackToInvoiceDate(enablePostTransactionCalculatedPropertyCache: true, (cacheProvider) => cacheProvider.GetLatestInvoiceTaxDate);
			AssertFallbackToInvoiceDate(enablePostTransactionCalculatedPropertyCache: false, (cacheProvider) => cacheProvider.GetLatestInvoiceTaxDate);
		}

		void AssertFallbackToInvoiceDate(bool enablePostTransactionCalculatedPropertyCache, Func<InvoiceTaxDateCacheProvider, Func<IEnumerable<ITransactionLineTaxDate>, ZDateTime, ZDateTime>> getInvoiceTaxDate)
		{
			AccountingConfigurationRegistry.Instance.EnablePostTransactionCalculatedPropertyCache.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enablePostTransactionCalculatedPropertyCache);
			var newFactory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(newFactory);

			var invoiceDate = new ZDateTime(2024, 8, 15);
			var apInvoice = testObjectCreator.CreateAPInvoice<APInvoice>("0000010", testObjectCreator.AUD, 1m, 10m, 0m, 0m, 10m, 0m, 0m);
			var lineWithEmptyTaxRate = apInvoice.Lines[0];
			lineWithEmptyTaxRate.AL_AT = testObjectCreator.GST11.PK;
			lineWithEmptyTaxRate.AL_TaxDate = ZDate.Invalid;

			var invoiceTaxDate = getInvoiceTaxDate(new InvoiceTaxDateCacheProvider())(apInvoice.Lines.Cast<ITransactionLineTaxDate>(), invoiceDate);

			AssertEquals(invoiceDate, invoiceTaxDate);
		}

		public void TestGetEarliestInvoiceTaxDate_EmptyResult()
		{
			AssertEmptyResult(enablePostTransactionCalculatedPropertyCache: true, (cacheProvider) => cacheProvider.GetEarliestInvoiceTaxDate);
			AssertEmptyResult(enablePostTransactionCalculatedPropertyCache: false, (cacheProvider) => cacheProvider.GetEarliestInvoiceTaxDate);
		}

		public void TestGetLatestInvoiceTaxDate_EmptyResult()
		{
			AssertEmptyResult(enablePostTransactionCalculatedPropertyCache: true, (cacheProvider) => cacheProvider.GetLatestInvoiceTaxDate);
			AssertEmptyResult(enablePostTransactionCalculatedPropertyCache: false, (cacheProvider) => cacheProvider.GetLatestInvoiceTaxDate);
		}

		void AssertEmptyResult(bool enablePostTransactionCalculatedPropertyCache, Func<InvoiceTaxDateCacheProvider, Func<IEnumerable<ITransactionLineTaxDate>, ZDateTime, ZDateTime>> getInvoiceTaxDate)
		{
			AccountingConfigurationRegistry.Instance.EnablePostTransactionCalculatedPropertyCache.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enablePostTransactionCalculatedPropertyCache);
			var newFactory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(newFactory);

			var invoiceDate = new ZDateTime(2024, 8, 15);
			var apInvoice = testObjectCreator.CreateAPInvoice<APInvoice>("0000010", testObjectCreator.AUD, 1m, 10m, 0m, 0m, 10m, 0m, 0m);
			var lineWithEmptyTaxRate = apInvoice.Lines[0];
			lineWithEmptyTaxRate.AL_AT = ZGuid.Empty;

			var earliestInvoiceTaxDate = getInvoiceTaxDate(new InvoiceTaxDateCacheProvider())(apInvoice.Lines.Cast<ITransactionLineTaxDate>(), invoiceDate);

			AssertEquals(ZDateTime.Empty, earliestInvoiceTaxDate);
		}

		public void TestStaleCache()
		{
			var taxDate = new ZDate(2024, 10, 15);
			var invoiceDate = new ZDateTime(2024, 8, 15);
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("0000010", TestObjectCreator.AUD, 1m, 10m, 0m, 0m, 10m, 0m, 0m);
			var lineWithEmptyTaxRate = apInvoice.Lines[0];
			lineWithEmptyTaxRate.AL_AT = TestObjectCreator.GST1.PK;
			lineWithEmptyTaxRate.AL_TaxDate = taxDate;
			var invoiceTaxDateCacheProvider = new InvoiceTaxDateCacheProvider();

			var earliestInvoiceTaxDate = invoiceTaxDateCacheProvider.GetEarliestInvoiceTaxDate(apInvoice.Lines.Cast<ITransactionLineTaxDate>(), invoiceDate);
			AssertEquals(taxDate, earliestInvoiceTaxDate);
			AssertEquals(taxDate, invoiceTaxDateCacheProvider.InvoiceTaxDateEarliest_ForTestOnly);

			var latestInvoiceTaxDate = invoiceTaxDateCacheProvider.GetLatestInvoiceTaxDate(apInvoice.Lines.Cast<ITransactionLineTaxDate>(), invoiceDate);
			AssertEquals(taxDate, latestInvoiceTaxDate);
			AssertEquals(taxDate, invoiceTaxDateCacheProvider.InvoiceTaxDateLatest_ForTestOnly);

			invoiceTaxDateCacheProvider.StaleCache();
			AssertEquals(ZDateTime.Invalid, invoiceTaxDateCacheProvider.InvoiceTaxDateEarliest_ForTestOnly);
			AssertEquals(ZDateTime.Invalid, invoiceTaxDateCacheProvider.InvoiceTaxDateLatest_ForTestOnly);

			earliestInvoiceTaxDate = invoiceTaxDateCacheProvider.GetEarliestInvoiceTaxDate(apInvoice.Lines.Cast<ITransactionLineTaxDate>(), invoiceDate);
			AssertEquals(taxDate, earliestInvoiceTaxDate);
			AssertEquals(taxDate, invoiceTaxDateCacheProvider.InvoiceTaxDateEarliest_ForTestOnly);

			latestInvoiceTaxDate = invoiceTaxDateCacheProvider.GetLatestInvoiceTaxDate(apInvoice.Lines.Cast<ITransactionLineTaxDate>(), invoiceDate);
			AssertEquals(taxDate, latestInvoiceTaxDate);
			AssertEquals(taxDate, invoiceTaxDateCacheProvider.InvoiceTaxDateLatest_ForTestOnly);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
