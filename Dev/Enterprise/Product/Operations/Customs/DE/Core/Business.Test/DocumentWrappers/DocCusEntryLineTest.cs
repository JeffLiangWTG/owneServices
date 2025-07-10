using System.Linq;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.DE.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCusEntryLine))]
	sealed class DocCusEntryLineTest : DocBaseCusEntryLineAbstractTest<CusEntryLine, DocCusEntryLine>
	{
		public override void TestDutyAmountRounded()
		{
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.AntiDumpingDuty, EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty);

			EntryLineInternal.Fees.AddOrUpdate(EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty, 12.3453M);
			AssertEquals("DutyAmount", 12.35M, EntryLineWrapperInternal.DutyAmountRounded);
		}

		public override void TestGSTVATAmountRounded()
		{
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Vat, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);

			EntryLineInternal.Fees.AddOrUpdate(Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, 12.3453M);
			AssertEquals("GSTVATAmount", 12.35M, EntryLineWrapperInternal.GSTVATAmountRounded);
		}

		public void TestNew()
		{
			AssertNull(DocCusEntryLine.New(null, Factory));
		}

		public void TestSupportingDocuments()
		{
			invoiceLine.SupportingDocuments.AddNew();
			invoiceLine.SupportingDocuments.AddNew();
			AssertEquals(2, Wrapper.SupportingDocuments.Count);
		}

		public void TestCharges()
		{
			invoiceLine.Charges.AddNew().J7_ChargeType = "A";
			invoiceLine.Charges.AddNew().J7_ChargeType = "B";
			invoiceLine2.Charges.AddNew().J7_ChargeType = "B";
			AssertEquals(2, Wrapper.Charges.Count);
		}

		public void TestApportionCharges()
		{
			invoiceLine.ApportionedCharges.AddNew().J7_ChargeType = "A";
			invoiceLine.ApportionedCharges.AddNew().J7_ChargeType = "B";
			invoiceLine2.ApportionedCharges.AddNew().J7_ChargeType = "B";
			AssertEquals(2, Wrapper.ApportionCharges.Count);
		}

		public void TestFees()
		{
			entryLine.Fees.AddNew();
			entryLine.Fees.AddNew();
			AssertEquals(2, Wrapper.Fees.Count);
		}

		public void TestFees_Description()
		{
			const string ExpectedB00GermanDescription = "Einfuhrumsatzsteuer (EUSt)";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountryCode = Env.CurrentCompany.Country.Code;
			helper.CreateNewOrGetExistingDataGrouping(currentCountryCode);
			var rateType = helper.CreateNewOrGetExistingRateType(currentCountryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.ExportTaxes, "Export Taxes");
			var cusRateCode = helper.CreateCusRateCode(Factory, "350", rateType.PK, description: "EN Description");
			helper.CreateOrGetLanguage("DE", "German");
			var rateCodeLanguage = helper.LoadOrCreateNewCusRateCodeLanguage(Factory, cusRateCode.ZY1_RateCode, "DE");
			rateCodeLanguage.ZXC_Description = "DE Description";
			Factory.Save();

			var entryLineFee = entryLine.Fees.AddNew();
			entryLineFee.CF_ChargeType = "350";

			var entryLineFee2 = entryLine.Fees.AddNew();
			entryLineFee2.CF_ChargeType = "B00";

			var fees = Wrapper.Fees.Cast<DocEntryHeaderFee>();
			CombineAssertions(() =>
			{
				AssertEquals("350", "DE Description", fees.Single(x => x.ChargeType == "350").Description);
				AssertEquals("B00", ExpectedB00GermanDescription, fees.Single(x => x.ChargeType == "B00").Description);
			});
		}

		public void TestFeesConfirmed()
		{
			Wrapper.UseConfirmedFees = true;

			entryLine.Fees.AddNew();
			entryLine.Fees.AddNew();

			entryLine.ConfirmedFees.Add(Factory.New<Customs.Business.CusEntryLineFee>());

			AssertEquals(1, Wrapper.Fees.Count);
		}

		public void TestNumberOfPacks()
		{
			var packagePivot = invoiceLine.PackagesPivot.AddNew();
			packagePivot.CHC_NumberOfPacks = 5;
			AssertEquals(5, Wrapper.NumberOfPacks);
		}

		public void TestPackType()
		{
			var package = Factory.New<BasePackage>();
			package.CW_PackType = "CT";
			var packagePivot = invoiceLine.PackagesPivot.AddNew();
			packagePivot.CHC_CW = package.PK;
			AssertEquals("CT", Wrapper.PackType);
		}

		public void TestMarksAndNumbers()
		{
			var package = Factory.New<BasePackage>();
			package.CW_MarksAndNos = "MARKS";
			var packagePivot = invoiceLine.PackagesPivot.AddNew();
			packagePivot.CHC_CW = package.PK;
			AssertEquals("MARKS", Wrapper.MarksAndNumbers);
		}

		public void TestGrossWeight()
		{
			invoiceLine.JI_Weight = 1.2345m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine2.JI_Weight = 1000m;
			invoiceLine2.JI_WeightUQ = Constants.Weight.Grams;
			AssertEquals("2,235", Wrapper.GrossWeight);
		}

		public void TestGrossWeightUQ()
		{
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine2.JI_WeightUQ = Constants.Weight.Grams;
			AssertEquals("KG", Wrapper.GrossWeightUQ);
		}

		public void TestNetWeight()
		{
			invoiceLine.JI_NetWeight = 1.2345m;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine2.JI_NetWeight = 1000m;
			invoiceLine2.JI_NetWeightUQ = Constants.Weight.Grams;
			AssertEquals("2,235", Wrapper.NetWeight);
		}

		public void TestNetWeightUQ()
		{
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine2.JI_NetWeightUQ = Constants.Weight.Grams;
			AssertEquals("KG", Wrapper.NetWeightUQ);
		}

		public void TestProcedure()
		{
			invoiceLine.JI_Procedure = "123456";
			AssertEquals("1234", Wrapper.Procedure);
		}

		public void TestCessionFlag()
		{
			invoiceLine.JI_CessionFlag = "1";
			AssertEquals("1", Wrapper.CessionFlag);
		}

		public void TestEUCode()
		{
			invoiceLine.JI_Procedure = "1234567";
			AssertEquals("567", Wrapper.EUCode);
		}

		public void TestAdditionalCodes()
		{
			invoiceLine.JI_SupplementaryCode1 = "123456";
			invoiceLine.JI_SupplementaryCode2 = "789123";
			AssertEquals("123456 789123", Wrapper.AdditionalCodes);
		}

		public void TestLinePrice()
		{
			invoiceLine.JI_LinePrice = 1.234m;
			invoiceLine2.JI_LinePrice = 0.1m;
			AssertEquals("1,33", Wrapper.LinePrice);
		}

		public void TestLinePriceCurrency()
		{
			invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			AssertEquals("EUR", Wrapper.LinePriceCurrency);
		}

		public void TestRate()
		{
			invoiceHeader.JZ_InvoiceCurrExRate = 1.2345678m;
			AssertEquals("1,234567", Wrapper.Rate);
		}

		public void TestLinePriceEURValue()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_LinePrice = 1.234m;
				AssertEquals("Rate == 0", "0,00", Wrapper.LinePriceEURValue);

				invoiceHeader.JZ_InvoiceCurrExRate = 1.2345678m;
				AssertEquals("Rate > 0", "1,00", Wrapper.LinePriceEURValue);
			});
		}

		public void TestNetPrice()
		{
			invoiceLine.JI_NetPrice = 1.234m;
			invoiceLine2.JI_NetPrice = 0.006m;
			AssertEquals("1,24", Wrapper.NetPrice);
		}

		public void TestNetPriceUCurrency()
		{
			invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			AssertEquals("EUR", Wrapper.NetPriceCurrency);
		}

		public void TestNetPriceEURValue()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_NetPrice = 1.234m;
				invoiceLine2.JI_NetPrice = 0.001m;
				AssertEquals("Rate == 0", "0,00", Wrapper.NetPriceEURValue);

				invoiceHeader.JZ_InvoiceCurrExRate = 1.2366678m;
				AssertEquals("Rate > 0", "1,00", Wrapper.NetPriceEURValue);
			});
		}

		public void TestStatisticsValue()
		{
			declaration.JE_MessageType = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
			invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_InvoiceAmount = 1000.234m;
			invoiceLine.JI_LinePrice = 1000.234m;
			invoiceLine2.JI_LinePrice = 1000.234m;
			AssertEquals("2000,46", Wrapper.StatisticsValue);
		}

		public void TestStatisticsValueQTY()
		{
			invoiceLine.JI_CustomsSecondQuantity = 1.234567m;
			invoiceLine2.JI_CustomsSecondQuantity = 1.234567m;
			AssertEquals("2,469", Wrapper.StatisticsValueQTY);
		}

		public void TestStatisticsValueUQ()
		{
			invoiceLine.JI_CustomsSecondUnitQty = "KG";
			invoiceLine2.JI_CustomsSecondUnitQty = Constants.Weight.Grams;
			AssertEquals("KG", Wrapper.StatisticsValueUQ);
		}

		public void TestPrimaryPreference()
		{
			invoiceLine.JI_PrimaryPreference = "1";
			AssertEquals("1", Wrapper.PrimaryPreference);
		}

		public void TestCustomsQuantity()
		{
			invoiceLine.JI_CustomsThirdQuantity = 1.2345m;
			invoiceLine2.JI_CustomsThirdQuantity = 1.2346m;

			AssertEquals("2,469", Wrapper.CustomsQuantityAsString);
		}

		public void TestCustomsQuantityUQ()
		{
			invoiceLine.JI_CustomsThirdUnitQty = "KG";
			invoiceLine2.JI_CustomsThirdUnitQty = Constants.Weight.Grams;
			AssertEquals("KG", Wrapper.CustomsQuantityUQ);
		}

		public void TestCustomsQuantity2()
		{
			invoiceLine.JI_CustomsFourthQuantity = 1.2345m;
			invoiceLine2.JI_CustomsFourthQuantity = 1.2346m;

			AssertEquals("2,469", Wrapper.CustomsQuantity2);
		}

		public void TestCustomsQuantityUQ2()
		{
			invoiceLine.JI_CustomsFourthUnitQty = "KG";
			invoiceLine2.JI_CustomsFourthUnitQty = Constants.Weight.Grams;
			AssertEquals("KG", Wrapper.CustomsQuantityUQ2);
		}

		public void TestCustomsValueAsString()
		{
			entryLine.CL_CustomsValue = 1.234M;
			AssertEquals("1,23", Wrapper.CustomsValueAsString);
		}

		public void TestCountryOfOrigin()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("CountryOfOrigin empty", Wrapper.CountryOfOrigin);

				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Germany;
				AssertEquals("CountryOfOrigin 'DE'", "DE Germany", Wrapper.CountryOfOrigin);
			});
		}

		public void TestPreferentialCountry()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("CountryOfSupply empty", Wrapper.PreferentialCountry);

				invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.Germany;
				AssertEquals("CountryOfSupply 'DE'", "DE Germany", Wrapper.PreferentialCountry);
			});
		}

		public override void TestLinePricesWithCurrency()
		{
			var message = "Should be formatted with German Number Culture";
			AssertEquals(message, "200,05 EUR", DocEntryLineMergeOfTwoInvoiceLines.LinePricesWithCurrency);
		}

		#region Implementation

		protected override string TestingCountry => Constants.CountryCodes.Germany;

		protected override DocCusEntryLine CreateEntryLineWrapper(ICusEntryLine entryLineInternal)
		{
			return DocCusEntryLine.New((CusEntryLine)entryLineInternal, Factory);
		}

		Customs.Business.CusEntryLine fEntryLineMergeOfTwoInvoiceLines;

		protected override ICusEntryLine EntryLineMergeOfTwoInvoiceLines
		{
			get
			{
				if (fEntryLineMergeOfTwoInvoiceLines == null)
				{
					var declaration = (JobDeclaration)GetNewDeclaration();
					var instruction = declaration.CustomsEntryInstructions.AddNew();

					var header = declaration.Invoices.AddNew();
					header.JZ_InvoiceNumber = "1";
					header.JZ_RX_NKInvoice_Currency = "EUR";

					var invoiceLine = header.JobComInvoiceLines.AddNew();
					invoiceLine.JI_LinePrice = 200.05m;
					invoiceLine.JI_Tariff = "123";
					invoiceLine.JI_LineNo = 1;
					invoiceLine.JI_CEI = instruction.PK;

					declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
					declaration.JE_MessageType = "IMP";
					declaration.DoMerge();
					Assert("Must have at least one CustomsEntryHeader", declaration.CustomsEntryHeaders.Count > 0);
					Assert("Must have at least one CustomsEntryHeader", declaration.CustomsEntryHeaders[0].MergedLines.Count > 0);
					fEntryLineMergeOfTwoInvoiceLines = declaration.CustomsEntryHeaders[0].MergedLines[0];
				}
				return fEntryLineMergeOfTwoInvoiceLines;
			}
		}

		protected override DocCusEntryLine DocEntryLineMergeOfTwoInvoiceLines
		{
			get { return CreateEntryLineWrapper(EntryLineMergeOfTwoInvoiceLines); }
		}

		#endregion

		DocCusEntryLine Wrapper => wrapper ??= EntryLineWrapperInternal;
		DocCusEntryLine wrapper;

		protected override CusEntryLine GetNewEntryLine()
		{
			if (entryLine == null)
			{
				declaration = Factory.New<JobDeclaration>();
				invoiceHeader = declaration.Invoices.AddNew();
				invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine2.JI_CL = entryLine.PK;
			}
			return entryLine;
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine invoiceLine2;
		CusEntryLine entryLine;
	}
}
