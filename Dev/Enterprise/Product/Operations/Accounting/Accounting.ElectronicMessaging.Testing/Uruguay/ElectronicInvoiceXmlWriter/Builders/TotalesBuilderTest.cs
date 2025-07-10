using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay.Testing
{
	class TotalesBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<TotalesBuilder>(new EncabezadoBuilder().TotalesBuilder_ExposedForTestOnly);
		}

		public void TestBuildToTalesInfo_WithTransactionInLocalCurrency()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OSCurrency = new Currency() { Code = "UYU" }
			};

			var builder = new TotalesBuilder() as ITotalesBuilder;
			var totales = builder.BuildTotales(transaction, Array.Empty<Item_Det_Fact>());

			AssertEquals(nameof(Totales.TpoMoneda), TipMonType.UYU, totales.TpoMoneda);
			AssertEquals(nameof(Totales.TpoCambio), 0m, totales.TpoCambio);
			Assert(nameof(Totales.TpoCambioSpecified), !totales.TpoCambioSpecified);
		}

		public void TestBuildToTalesInf_WithTransactionInForeignCurrency()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OSCurrency = new Currency() { Code = "USD" },
				ExchangeRate = 15.22m
			};

			var builder = new TotalesBuilder() as ITotalesBuilder;
			var totales = builder.BuildTotales(transaction, Array.Empty<Item_Det_Fact>());

			AssertEquals(nameof(Totales.TpoMoneda), TipMonType.USD, totales.TpoMoneda);
			AssertEquals(nameof(Totales.TpoCambio), 15.22m, totales.TpoCambio);
			Assert(nameof(Totales.TpoCambioSpecified), totales.TpoCambioSpecified);
		}

		public void TestBuildTotales_TransactionWithIncorrectOsCurrency()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OSCurrency = new Currency { Code = "A2Y" };
			var builder = new TotalesBuilder() as ITotalesBuilder;
			var totales = builder.BuildTotales(transaction, Array.Empty<Item_Det_Fact>());

			AssertEquals(nameof(totales.TpoCambioSpecified), false, totales.TpoCambioSpecified);
			AssertEquals(nameof(totales.TpoCambio), 0m, totales.TpoCambio);
		}

		public void TestBuildToTalesInf_WithTransactionInForeignCurrencyWhitOutExchangeRate()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OSCurrency = new Currency() { Code = "USD" },
			};

			var builder = new TotalesBuilder() as ITotalesBuilder;
			var totales = builder.BuildTotales(transaction, Array.Empty<Item_Det_Fact>());

			AssertEquals(nameof(Totales.TpoMoneda), TipMonType.USD, totales.TpoMoneda);
			AssertEquals(nameof(Totales.TpoCambio), 0m, totales.TpoCambio);
			Assert(nameof(Totales.TpoCambioSpecified), !totales.TpoCambioSpecified);
		}

		public void TestBuildToTalesInf_WithNullData()
		{
			var builder = new TotalesBuilder() as ITotalesBuilder;
			var totales = builder.BuildTotales(null, Array.Empty<Item_Det_Fact>());

			AssertEquals(nameof(totales.TpoCambioSpecified), false, totales.TpoCambioSpecified);
			AssertEquals(nameof(totales.TpoCambio), 0m, totales.TpoCambio);

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			totales = builder.BuildTotales(transaction, Array.Empty<Item_Det_Fact>());

			AssertEquals(nameof(totales.TpoCambioSpecified), false, totales.TpoCambioSpecified);
			AssertEquals(nameof(totales.TpoCambio), 0m, totales.TpoCambio);

			transaction.OSCurrency = new Currency() { Code = "UYU" };
			totales = builder.BuildTotales(transaction, null);

			Assert(!totales.MntNoGrvSpecified);
			Assert(!totales.MntExpoyAsimSpecified);
			Assert(!totales.MntNetoIvaTasaMinSpecified);
			Assert(!totales.MntNetoIVATasaBasicaSpecified);
		}

		#region VATTotals

		public void TestCheckNulls()
		{
			var builder = new TotalesBuilder() as ITotalesBuilder;

			TransactionInfo transaction = null;
			AssertBuildTotalesInfo_NullChecks();

			transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			AssertBuildTotalesInfo_NullChecks();

			transaction.TransactionType = TransactionType.INV;
			AssertBuildTotalesInfo_NullChecks();

			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			AssertBuildTotalesInfo_NullChecks();

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { VATTaxID = new TaxID() });
			AssertBuildTotalesInfo_NullChecks();

			void AssertBuildTotalesInfo_NullChecks()
			{
				var totales = builder.BuildTotales(transaction, Array.Empty<Item_Det_Fact>());
				AssertMntIVATasaBasica(builder, transaction, 0m, false);
				AssertMntIVATasaMin(builder, transaction, 0m, false);
			}
		}

		#region VATBasicAmount

		public void TestMntIVATasaBasica_Amounts_PositivesAndNegatives_INV()
		{
			var builder = new TotalesBuilder() as ITotalesBuilder;
			var transaction = CreateTransactionInfo(TransactionType.INV);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 73.3400m, VATTaxID = CreateVatTaxID("IVA") });
			AssertMntIVATasaBasica(builder, transaction, 73.34m, true);

			transaction.PostingJournalCollection.Clear();
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 49.7400m, VATTaxID = CreateVatTaxID("IVA.66") });
			AssertMntIVATasaBasica(builder, transaction, 49.74m, true);

			transaction.PostingJournalCollection.Clear();
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 187.5200m, VATTaxID = CreateVatTaxID("CAPIVA") });
			AssertMntIVATasaBasica(builder, transaction, 187.52m, true);

			transaction.PostingJournalCollection.Clear();
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -22.0000m, VATTaxID = CreateVatTaxID("IVA") });
			AssertMntIVATasaBasica(builder, transaction, -22.00m, true);

			transaction.PostingJournalCollection.Clear();
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -49.7400m, VATTaxID = CreateVatTaxID("IVA.66") });
			AssertMntIVATasaBasica(builder, transaction, -49.74m, true);

			transaction.PostingJournalCollection.Clear();
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -687.4600m, VATTaxID = CreateVatTaxID("CAPIVA") });
			AssertMntIVATasaBasica(builder, transaction, -687.46m, true);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 687.4600m, VATTaxID = CreateVatTaxID("CAPIVA") });
			AssertMntIVATasaBasica(builder, transaction, 0m, true);
		}

		public void TestMntIVATasaBasica_Amounts_PositivesAndNegatives_CRD()
		{
			var builder = new TotalesBuilder() as ITotalesBuilder;
			var transaction = CreateTransactionInfo(TransactionType.CRD);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -73.3400m, VATTaxID = CreateVatTaxID("IVA") });
			AssertMntIVATasaBasica(builder, transaction, 73.34m, true);

			transaction.PostingJournalCollection.Clear();
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -187.5200m, VATTaxID = CreateVatTaxID("CAPIVA") });
			AssertMntIVATasaBasica(builder, transaction, 187.52m, true);

			transaction.PostingJournalCollection.Clear();
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -49.7400m, VATTaxID = CreateVatTaxID("IVA.66") });
			AssertMntIVATasaBasica(builder, transaction, 49.74m, true);

			transaction.PostingJournalCollection.Clear();
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 22.0000m, VATTaxID = CreateVatTaxID("IVA") });
			AssertMntIVATasaBasica(builder, transaction, -22.00m, true);

			transaction.PostingJournalCollection.Clear();
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 187.5200m, VATTaxID = CreateVatTaxID("CAPIVA") });
			AssertMntIVATasaBasica(builder, transaction, -187.52m, true);

			transaction.PostingJournalCollection.Clear();
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 49.7400m, VATTaxID = CreateVatTaxID("IVA.66") });
			AssertMntIVATasaBasica(builder, transaction, -49.74m, true);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -49.7400m, VATTaxID = CreateVatTaxID("IVA.66") });
			AssertMntIVATasaBasica(builder, transaction, 0m, true);
		}

		#endregion

		#region VATMinAmount

		public void TestMntTasaMin_Amounts_PositivesAndNegatives_INV()
		{
			var builder = new TotalesBuilder() as ITotalesBuilder;
			var transaction = CreateTransactionInfo(TransactionType.INV);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 158.9900m, VATTaxID = CreateVatTaxID("IVA10") });
			AssertMntIVATasaMin(builder, transaction, 158.99m, true);

			transaction.PostingJournalCollection.Clear();
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -23.0700m, VATTaxID = CreateVatTaxID("IVA10") });
			AssertMntIVATasaMin(builder, transaction, -23.07m, true);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 10.0000m, VATTaxID = CreateVatTaxID("IVA10") });
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 13.0700m, VATTaxID = CreateVatTaxID("IVA10") });
			AssertMntIVATasaMin(builder, transaction, 0m, true);
		}

		public void TestMntTasaMin_Amounts_PositivesAndNegatives_CRD()
		{
			var builder = new TotalesBuilder() as ITotalesBuilder;
			var transaction = CreateTransactionInfo(TransactionType.CRD);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -158.9900m, VATTaxID = CreateVatTaxID("IVA10") });
			AssertMntIVATasaMin(builder, transaction, 158.99m, true);

			transaction.PostingJournalCollection.Clear();
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 23.0700m, VATTaxID = CreateVatTaxID("IVA10") });
			AssertMntIVATasaMin(builder, transaction, -23.07m, true);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -10.0000m, VATTaxID = CreateVatTaxID("IVA10") });
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -13.0700m, VATTaxID = CreateVatTaxID("IVA10") });
			AssertMntIVATasaMin(builder, transaction, 0m, true);
		}

		#endregion

		#region VATMixAmount

		public void TestMixOfTaxRates_INV()
		{
			var builder = new TotalesBuilder() as ITotalesBuilder;
			var transaction = CreateTransactionInfo(TransactionType.INV);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 15.5500m, VATTaxID = CreateVatTaxID(UruguayConstants.EXEMPT) });
			AssertMntIVATasaMin(builder, transaction, 0m, false);
			AssertMntIVATasaBasica(builder, transaction, 0m, false);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 73.3400m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA) });
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -22.0000m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA) });
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 49.7400m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA66) });
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -7.4600m, VATTaxID = CreateVatTaxID(UruguayConstants.CAPIVA) });
			AssertMntIVATasaMin(builder, transaction, 0m, false);
			AssertMntIVATasaBasica(builder, transaction, 93.62m, true);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -100.0200m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA10) });
			AssertMntIVATasaMin(builder, transaction, -100.02m, true);
			AssertMntIVATasaBasica(builder, transaction, 93.62m, true);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 100.0200m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA10) });
			AssertMntIVATasaMin(builder, transaction, 0m, true);
			AssertMntIVATasaBasica(builder, transaction, 93.62m, true);

			transaction.PostingJournalCollection.Clear();

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -26.3200m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA10) });
			AssertMntIVATasaMin(builder, transaction, -26.32m, true);
			AssertMntIVATasaBasica(builder, transaction, 0m, false);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -22.0000m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA) });
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 49.7400m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA66) });
			AssertMntIVATasaMin(builder, transaction, -26.32m, true);
			AssertMntIVATasaBasica(builder, transaction, 27.74m, true);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -27.7400m, VATTaxID = CreateVatTaxID(UruguayConstants.CAPIVA) });
			AssertMntIVATasaMin(builder, transaction, -26.32m, true);
			AssertMntIVATasaBasica(builder, transaction, 0m, true);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 47.7400m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA77) });
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 15.8600m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA396) });
			AssertMntIVATasaMin(builder, transaction, -26.32m, true);
			AssertMntIVATasaBasica(builder, transaction, 63.60m, true);
		}

		public void TestMixOfTaxRates_CRD()
		{
			var builder = new TotalesBuilder() as ITotalesBuilder;
			var transaction = CreateTransactionInfo(TransactionType.CRD);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 15.5500m, VATTaxID = CreateVatTaxID(UruguayConstants.EXEMPT) });
			AssertMntIVATasaMin(builder, transaction, 0m, false);
			AssertMntIVATasaBasica(builder, transaction, 0m, false);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 73.3400m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA) });
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -22.0000m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA) });
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 49.7400m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA66) });
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -7.4600m, VATTaxID = CreateVatTaxID(UruguayConstants.CAPIVA) });
			AssertMntIVATasaMin(builder, transaction, 0m, false);
			AssertMntIVATasaBasica(builder, transaction, -93.62m, true);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -100.0200m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA10) });
			AssertMntIVATasaMin(builder, transaction, 100.02m, true);
			AssertMntIVATasaBasica(builder, transaction, -93.62m, true);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 100.0200m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA10) });
			AssertMntIVATasaMin(builder, transaction, 0m, true);
			AssertMntIVATasaBasica(builder, transaction, -93.62m, true);

			transaction.PostingJournalCollection.Clear();

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -26.3200m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA10) });
			AssertMntIVATasaMin(builder, transaction, 26.32m, true);
			AssertMntIVATasaBasica(builder, transaction, 0m, false);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -22.0000m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA) });
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 49.7400m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA66) });
			AssertMntIVATasaMin(builder, transaction, 26.32m, true);
			AssertMntIVATasaBasica(builder, transaction, -27.74m, true);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -27.7400m, VATTaxID = CreateVatTaxID(UruguayConstants.CAPIVA) });
			AssertMntIVATasaMin(builder, transaction, 26.32m, true);
			AssertMntIVATasaBasica(builder, transaction, 0m, true);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -47.7400m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA77) });
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 15.8600m, VATTaxID = CreateVatTaxID(UruguayConstants.IVA396) });
			AssertMntIVATasaMin(builder, transaction, 26.32m, true);
			AssertMntIVATasaBasica(builder, transaction, 31.88m, true);
		}

		#endregion

		#region MissingVATTaxID

		public void TestChargeLinesWithMissingVATTaxID()
		{
			var builder = new TotalesBuilder() as ITotalesBuilder;

			var transaction = CreateTransactionInfo(TransactionType.INV);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 73.3400m });
			AssertMntIVATasaMin(builder, transaction, 0m, false);
			AssertMntIVATasaBasica(builder, transaction, 0m, false);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -22.0000m, VATTaxID = CreateVatTaxID("IVA") });
			AssertMntIVATasaMin(builder, transaction, 0m, false);
			AssertMntIVATasaBasica(builder, transaction, -22m, true);

			transaction.PostingJournalCollection.Clear();

			transaction = CreateTransactionInfo(TransactionType.CRD);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 73.3400m });
			AssertMntIVATasaMin(builder, transaction, 0m, false);
			AssertMntIVATasaBasica(builder, transaction, 0m, false);

			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -22.0000m, VATTaxID = CreateVatTaxID("IVA") });
			AssertMntIVATasaMin(builder, transaction, 0m, false);
			AssertMntIVATasaBasica(builder, transaction, 22m, true);
		}

		#endregion

		#region VATTaxRates

		public void TestIVATasas_WithoutRegistriesOnRefAccTaxRates()
		{
			RefAccTaxRateCollection taxRateCollection = new RefAccTaxRateCollection(Factory);
			Factory.Save();

			TotalesBuilder builder = new TotalesBuilder();
			builder.SubstituteFactory_ForTestOnly(Factory);
			TransactionInfo transaction = null;
			AssertTaxMinRate(builder, transaction, 0m);
			AssertTaxBasicRate(builder, transaction, 0m);
		}

		[TestDate(2020, 06, 15)]
		public void TestIVATasas_WithRegistriesOnRefAccTaxRates()
		{
			TransactionInfo transaction = null;
			TotalesBuilder builder = new TotalesBuilder();
			var taxRateCollection = new RefAccTaxRateCollection(Factory);

			MasterFilesTestHelper.CreateRefTaxRate(Factory, CountryCodes.Australia, "GST", 10, 1, new ZDate(2020, 06, 1), new ZDate(2020, 12, 31), taxRateCollection.AddNew());
			MasterFilesTestHelper.CreateRefTaxRate(Factory, CountryCodes.Uruguay, "CAPSTD", 22, 1, new ZDate(2020, 06, 1), new ZDate(2020, 12, 31), taxRateCollection.AddNew());
			Factory.Save();

			builder.SubstituteFactory_ForTestOnly(Factory);
			AssertTaxMinRate(builder, transaction, 0m);
			AssertTaxBasicRate(builder, transaction, 0m);

			var taxRate1 = MasterFilesTestHelper.CreateRefTaxRate(Factory, CountryCodes.Uruguay, "MID", 10, 1, new ZDate(2020, 06, 1), new ZDate(2020, 06, 10));
			var taxRate2 = MasterFilesTestHelper.CreateRefTaxRate(Factory, CountryCodes.Uruguay, "STD", 22, 1, new ZDate(2020, 06, 1), new ZDate(2020, 06, 10));
			Factory.Save();

			builder.SubstituteFactory_ForTestOnly(Factory);
			AssertTaxMinRate(builder, transaction, 0m);
			AssertTaxBasicRate(builder, transaction, 0m);

			taxRate1.ZAT_EndDate = new ZDate(2020, 06, 18);
			taxRate2.ZAT_EndDate = new ZDate(2020, 06, 18);
			Factory.Save();

			builder.SubstituteFactory_ForTestOnly(Factory);
			AssertTaxMinRate(builder, transaction, 10m);
			AssertTaxBasicRate(builder, transaction, 22m);
		}

		#endregion

		void AssertTaxBasicRate(ITotalesBuilder builder, TransactionInfo transaction, ZDecimal taxBasicRate)
		{
			var totales = builder.BuildTotales(transaction, Array.Empty<Item_Det_Fact>());
			AssertEquals(nameof(totales.IVATasaBasica), taxBasicRate, totales.IVATasaBasica);
			AssertEquals(nameof(totales.IVATasaBasicaSpecified), true, totales.IVATasaBasicaSpecified);
		}

		void AssertTaxMinRate(ITotalesBuilder builder, TransactionInfo transaction, ZDecimal taxMinRate)
		{
			var totales = builder.BuildTotales(transaction, Array.Empty<Item_Det_Fact>());
			AssertEquals(nameof(totales.IVATasaMin), taxMinRate, totales.IVATasaMin);
			AssertEquals(nameof(totales.IVATasaMinSpecified), true, totales.IVATasaMinSpecified);
		}

		void AssertMntIVATasaBasica(ITotalesBuilder builder, TransactionInfo transaction, decimal mntIVATasaBasica, bool mntIVATasaBasicaSpecified)
		{
			var totales = builder.BuildTotales(transaction, Array.Empty<Item_Det_Fact>());
			AssertEquals(nameof(totales.MntIVATasaBasica), mntIVATasaBasica, totales.MntIVATasaBasica);
			AssertEquals(nameof(totales.MntIVATasaBasicaSpecified), mntIVATasaBasicaSpecified, totales.MntIVATasaBasicaSpecified);
		}

		void AssertMntIVATasaMin(ITotalesBuilder builder, TransactionInfo transaction, ZDecimal mntIVATasaMin, bool mntIVATasaMinSpecified)
		{
			var totales = builder.BuildTotales(transaction, Array.Empty<Item_Det_Fact>());
			AssertEquals(nameof(totales.MntIVATasaMin), mntIVATasaMin, totales.MntIVATasaMin);
			AssertEquals(nameof(totales.MntIVATasaMinSpecified), mntIVATasaMinSpecified, totales.MntIVATasaMinSpecified);
		}

		#endregion

		public void TestCalculateTotals()
		{
			var items = new List<Item_Det_Fact>()
			{
				new Item_Det_Fact() { MontoItem = 0, IndFact = Item_Det_FactIndFact.Item1 },
				new Item_Det_Fact() { MontoItem = 10.898m, IndFact = Item_Det_FactIndFact.Item10 },
				new Item_Det_Fact() { MontoItem = 20.255m, IndFact = Item_Det_FactIndFact.Item10 },
				new Item_Det_Fact() { MontoItem = 30.05m, IndFact = Item_Det_FactIndFact.Item2 },
				new Item_Det_Fact() { MontoItem = -40.21m, IndFact = Item_Det_FactIndFact.Item2 },
				new Item_Det_Fact() { MontoItem = 40.999m, IndFact = Item_Det_FactIndFact.Item3 },
				new Item_Det_Fact() { MontoItem = 50, IndFact = Item_Det_FactIndFact.Item3 },
				new Item_Det_Fact() { MontoItem = 60.222m, IndFact = Item_Det_FactIndFact.Item3 },
			};

			AssertTotals(items, 0, true, 31.16m, true, -10.16m, true, 151.22m, true);

			items.RemoveAll(x => x.IndFact == Item_Det_FactIndFact.Item10);

			AssertTotals(items, 0, true, 0, false, -10.16m, true, 151.22m, true);

			void AssertTotals(List<Item_Det_Fact> itemDetFacts, decimal expectedMntNoGrv, bool exectedMntNoGrvSpecified, decimal expectedMntExpoyAsim, bool expectedMntExpoyAsimSpecified, decimal expectedMntNetoIvaTasaMin, bool exectedMntNetoIvaTasaMinSpecified, decimal expectedMntNetoIVATasaBasica, bool epectedMntNetoIVATasaBasicaSpecified)
			{
				var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
				{
					OSCurrency = new Currency() { Code = "UYU" }
				};

				var totalesBuilder = (new TotalesBuilder() as ITotalesBuilder).BuildTotales(transactionInfo, itemDetFacts.ToArray());

				AssertEquals(expectedMntNoGrv, totalesBuilder.MntNoGrv);
				AssertEquals(exectedMntNoGrvSpecified, totalesBuilder.MntNoGrvSpecified);

				AssertEquals(expectedMntExpoyAsim, totalesBuilder.MntExpoyAsim);
				AssertEquals(expectedMntExpoyAsimSpecified, totalesBuilder.MntExpoyAsimSpecified);

				AssertEquals(expectedMntNetoIvaTasaMin, totalesBuilder.MntNetoIvaTasaMin);
				AssertEquals(exectedMntNetoIvaTasaMinSpecified, totalesBuilder.MntNetoIvaTasaMinSpecified);

				AssertEquals(expectedMntNetoIVATasaBasica, totalesBuilder.MntNetoIVATasaBasica);
				AssertEquals(epectedMntNetoIVATasaBasicaSpecified, totalesBuilder.MntNetoIVATasaBasicaSpecified);
			}
		}

		public void TestHeadlineTotalsTotalAmount()
		{
			var items = new List<Item_Det_Fact>()
			{
				new Item_Det_Fact() { MontoItem = 10.234m, IndFact = Item_Det_FactIndFact.Item1 },
				new Item_Det_Fact() { MontoItem = 20.9889m, IndFact = Item_Det_FactIndFact.Item10 },
				new Item_Det_Fact() { MontoItem = 30.6788m, IndFact = Item_Det_FactIndFact.Item2 },
				new Item_Det_Fact() { MontoItem = 40.1278m, IndFact = Item_Det_FactIndFact.Item3 },
				new Item_Det_Fact() { MontoItem = 50.5m, IndFact = Item_Det_FactIndFact.Item6 },
				new Item_Det_Fact() { MontoItem = 60.033m, IndFact = Item_Det_FactIndFact.Item6 },
				new Item_Det_Fact() { MontoItem = 70.033m, IndFact = Item_Det_FactIndFact.Item7 }
			};
			var builder = new TotalesBuilder() as ITotalesBuilder;
			var transaction = CreateTransactionInfo(TransactionType.INV);
			transaction.OSCurrency = new Currency() { Code = "UYU" };
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 80.4589, VATTaxID = CreateVatTaxID("IVA") });
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 90.3892, VATTaxID = CreateVatTaxID("IVA10") });

			var totales = builder.BuildTotales(transaction, items.ToArray());

			AssertEquals(40.50m, totales.MontoNF);
			AssertEquals(true, totales.MontoNFSpecified);
			AssertEquals("007", totales.CantLinDet);
			AssertEquals(272.88m, totales.MntTotal);
			AssertEquals(313.38m, totales.MntPagar);

			items.RemoveAll(x => x.IndFact == Item_Det_FactIndFact.Item6);
			items.RemoveAll(x => x.IndFact == Item_Det_FactIndFact.Item7);

			totales = builder.BuildTotales(transaction, items.ToArray());

			AssertEquals(false, totales.MontoNFSpecified);
			AssertEquals("004", totales.CantLinDet);

			transaction.PostingJournalCollection.Clear();
			totales = builder.BuildTotales(transaction, null);

			AssertEquals(0m, totales.MntTotal);
			AssertEquals(0m, totales.MntPagar);
			AssertNull(totales.CantLinDet);
		}

		public void TestTotalAmountsRounding_withOutIVA()
		{
			var transaction = CreateTransactionInfo(TransactionType.INV);
			transaction.OSCurrency = new Currency() { Code = "JOD" };

			var items = new List<Item_Det_Fact>()
			{
				new Item_Det_Fact() { MontoItem = 1595.785m, IndFact = Item_Det_FactIndFact.Item1 },
				new Item_Det_Fact() { MontoItem = -458.962m, IndFact = Item_Det_FactIndFact.Item1 },
				new Item_Det_Fact() { MontoItem = 567.895m, IndFact = Item_Det_FactIndFact.Item10 },
				new Item_Det_Fact() { MontoItem = 378.888m, IndFact = Item_Det_FactIndFact.Item10 },
				new Item_Det_Fact() { MontoItem = -58.789m, IndFact = Item_Det_FactIndFact.Item10 },
			};

			var builder = new TotalesBuilder() as ITotalesBuilder;
			var totales = builder.BuildTotales(transaction, items.ToArray());

			AssertEquals(1136.83m, totales.MntNoGrv);
			AssertEquals(888.00m, totales.MntExpoyAsim);
			AssertEquals(0m, totales.MntNetoIvaTasaMin);
			AssertEquals(0m, totales.MntNetoIVATasaBasica);
			AssertEquals(2024.83m, totales.MntTotal);
			AssertEquals(2024.83m, totales.MntPagar);
		}

		public void TestTotalAmountsRounding_withIVA()
		{
			var transaction = CreateTransactionInfo(TransactionType.INV);
			transaction.OSCurrency = new Currency() { Code = "JOD" };
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 159.5790m, VATTaxID = CreateVatTaxID("IVA10") });
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = -45.8960m, VATTaxID = CreateVatTaxID("IVA10") });
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 71.6000m, VATTaxID = CreateVatTaxID("IVA") });
			transaction.PostingJournalCollection.Add(new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSGSTVATAmount = 3.6650m, VATTaxID = CreateVatTaxID("IVA.66") });

			var items = new List<Item_Det_Fact>()
			{
				new Item_Det_Fact() { MontoItem = 1595.785m, IndFact = Item_Det_FactIndFact.Item2 },
				new Item_Det_Fact() { MontoItem = -458.962m, IndFact = Item_Det_FactIndFact.Item2 },
				new Item_Det_Fact() { MontoItem = 325.456m, IndFact = Item_Det_FactIndFact.Item3 },
				new Item_Det_Fact() { MontoItem = 538.58571m, IndFact = Item_Det_FactIndFact.Item1 },
				new Item_Det_Fact() { MontoItem = 16.65729m, IndFact = Item_Det_FactIndFact.Item3 }
			};

			var builder = new TotalesBuilder() as ITotalesBuilder;
			var totales = builder.BuildTotales(transaction, items.ToArray());

			AssertEquals(538.59m, totales.MntNoGrv);
			AssertEquals(0m, totales.MntExpoyAsim);
			AssertEquals(1136.83m, totales.MntNetoIvaTasaMin);
			AssertEquals(342.12m, totales.MntNetoIVATasaBasica);
			AssertEquals(2206.49m, totales.MntTotal);
			AssertEquals(2206.49m, totales.MntPagar);
		}

		public void TestMontoNF_With_FacIndFact6_OnZero()
		{
			var items = new List<Item_Det_Fact>()
			{
				new Item_Det_Fact() { MontoItem = 0m, IndFact = Item_Det_FactIndFact.Item6 },
				new Item_Det_Fact() { MontoItem = 500.871m, IndFact = Item_Det_FactIndFact.Item7 }
			};

			var builder = new TotalesBuilder() as ITotalesBuilder;
			var transaction = CreateTransactionInfo(TransactionType.INV);
			transaction.OSCurrency = new Currency() { Code = "UYU" };

			var totales = builder.BuildTotales(transaction, items.ToArray());

			AssertEquals(-500.87m, totales.MontoNF);
			AssertEquals(true, totales.MontoNFSpecified);
		}

		public void TestMontoNF_With_FacIndFact7_OnZero()
		{
			var items = new List<Item_Det_Fact>()
			{
				new Item_Det_Fact() { MontoItem = 200.567m, IndFact = Item_Det_FactIndFact.Item6 },
				new Item_Det_Fact() { MontoItem = 0m, IndFact = Item_Det_FactIndFact.Item7 }
			};

			var builder = new TotalesBuilder() as ITotalesBuilder;
			var transaction = CreateTransactionInfo(TransactionType.INV);
			transaction.OSCurrency = new Currency() { Code = "UYU" };

			var totales = builder.BuildTotales(transaction, items.ToArray());

			AssertEquals(200.57m, totales.MontoNF);
			AssertEquals(true, totales.MontoNFSpecified);
		}

		public void TestMontoNF_With_FacIndFact6And7_RoudingValues()
		{
			var items = new List<Item_Det_Fact>()
			{
				new Item_Det_Fact() { MontoItem = 200.4034m, IndFact = Item_Det_FactIndFact.Item6 },
				new Item_Det_Fact() { MontoItem = 500.439m, IndFact = Item_Det_FactIndFact.Item7 }
			};

			var builder = new TotalesBuilder() as ITotalesBuilder;
			var transaction = CreateTransactionInfo(TransactionType.INV);
			transaction.OSCurrency = new Currency() { Code = "UYU" };

			var totales = builder.BuildTotales(transaction, items.ToArray());

			AssertEquals(-300.04m, totales.MontoNF);
			AssertEquals(true, totales.MontoNFSpecified);
		}

		public void TestMontoNF_Without_FacIndFact6And7()
		{
			var items = new List<Item_Det_Fact>()
			{
				new Item_Det_Fact() { MontoItem = 100m, IndFact = Item_Det_FactIndFact.Item3 },
				new Item_Det_Fact() { MontoItem = 300m, IndFact = Item_Det_FactIndFact.Item4 }
			};

			var builder = new TotalesBuilder() as ITotalesBuilder;
			var transaction = CreateTransactionInfo(TransactionType.INV);
			transaction.OSCurrency = new Currency() { Code = "UYU" };

			var totales = builder.BuildTotales(transaction, items.ToArray());

			AssertEquals(false, totales.MontoNFSpecified);
		}

		#region Implementation

		TransactionInfo CreateTransactionInfo(TransactionType transactionType)
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.TransactionType = transactionType;
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			return transaction;
		}

		TaxID CreateVatTaxID(ZString taxCode)
		{
			return new TaxID() { TaxCode = taxCode };
		}

		#endregion
	}
}
