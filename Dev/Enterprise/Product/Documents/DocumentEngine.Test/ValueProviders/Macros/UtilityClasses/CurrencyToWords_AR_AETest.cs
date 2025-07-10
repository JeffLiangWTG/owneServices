using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.DocumentEngine.MacroValueProviders.Utilities.CurrencyConvertor;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class CurrencyToWords_AR_AETest : TestCaseWithFactory
	{
		public void TestCurrencyToWordsForDifferentCurrenciesAndDifferentAmounts()
		{
			InitializeTestSet();
			foreach (var testCase in testSet)
			{
				AssertEquals($"Amount: {testCase.amount}, Currency: {testCase.currencyCode}, format:{testCase.specialFormat}", testCase.expectedResult, new CurrencyToWords_AR_AE().ConvertToWords(testCase.amount, testCase.currencyCode, testCase.specialFormat));
			}
		}

		struct CurrencyToWords_AR_AETestCase
		{
			public string expectedResult;
			public double amount;
			public string currencyCode;
			public SpecialFormat specialFormat;

			public CurrencyToWords_AR_AETestCase(string result, double amount, string currency, SpecialFormat special_Format = SpecialFormat.None)
			{
				this.expectedResult = result;
				this.amount = amount;
				currencyCode = currency;
				specialFormat = special_Format;
			}
		}
		List<CurrencyToWords_AR_AETestCase> testSet;
		void InitializeTestSet()
		{
			var helper = new RefCurrencyTestHelper(Factory);
			var currencyAUD = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
			var currencyUSD = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var currencySYP = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "SYP"));
			var currencyIRR = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "IRR"));
			var currencyAED = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AED"));
			var currencySAR = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "SAR"));
			var currencyTND = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "TND"));
			helper.CreateRefLanguageText("RX_UnitName", currencyAUD.PK, "AR-AE", "RX", "دولار");
			helper.CreateRefLanguageText("RX_UnitName", currencyUSD.PK, "AR-AE", "RX", "دولار");
			helper.CreateRefLanguageText("RX_UnitName", currencySYP.PK, "AR-AE", "RX", "جنية");
			helper.CreateRefLanguageText("RX_UnitName", currencyIRR.PK, "AR-AE", "RX", "ريال");
			helper.CreateRefLanguageText("RX_UnitName", currencyAED.PK, "AR-AE", "RX", "درهم");
			helper.CreateRefLanguageText("RX_UnitName", currencySAR.PK, "AR-AE", "RX", "ريال");
			helper.CreateRefLanguageText("RX_UnitName", currencyTND.PK, "AR-AE", "RX", "دينار");

			helper.CreateRefLanguageText("RX_SubUnitName", currencyAUD.PK, "AR-AE", "RX", "سنتات");
			helper.CreateRefLanguageText("RX_SubUnitName", currencyUSD.PK, "AR-AE", "RX", "سنتات");
			helper.CreateRefLanguageText("RX_SubUnitName", currencySYP.PK, "AR-AE", "RX", "قرش");
			helper.CreateRefLanguageText("RX_SubUnitName", currencyIRR.PK, "AR-AE", "RX", "دينار");
			helper.CreateRefLanguageText("RX_SubUnitName", currencyAED.PK, "AR-AE", "RX", "فلس");
			helper.CreateRefLanguageText("RX_SubUnitName", currencySAR.PK, "AR-AE", "RX", "هلالات");
			helper.CreateRefLanguageText("RX_SubUnitName", currencyTND.PK, "AR-AE", "RX", "مليم");
			Factory.Save();

			testSet = new List<CurrencyToWords_AR_AETestCase>
			{   new CurrencyToWords_AR_AETestCase("صفر دولار", 0, "AUD"),
				new CurrencyToWords_AR_AETestCase("واحد دولار", 1, "AUD"),
				new CurrencyToWords_AR_AETestCase("اثنان دولار", 2, "AUD"),
				new CurrencyToWords_AR_AETestCase("ثلاثة دولار", 3, "AUD"),
				new CurrencyToWords_AR_AETestCase("عشرون دولار", 20, "AUD"),
				new CurrencyToWords_AR_AETestCase("واحد و عشرون دولار", 21, "AUD"),
				new CurrencyToWords_AR_AETestCase("ثلاثة و عشرون دولار", 23, "AUD"),
				new CurrencyToWords_AR_AETestCase("مائة دولار", 100, "AUD"),
				new CurrencyToWords_AR_AETestCase("مائة و واحد دولار", 101, "AUD"),
				new CurrencyToWords_AR_AETestCase("مائة و اثنان دولار", 102, "AUD"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة دولار", 103, "AUD"),
				new CurrencyToWords_AR_AETestCase("مئتا دولار", 200, "AUD"),
				new CurrencyToWords_AR_AETestCase("مئتان و واحد دولار", 201, "AUD"),
				new CurrencyToWords_AR_AETestCase("مئتان و اثنان دولار", 202, "AUD"),
				new CurrencyToWords_AR_AETestCase("مئتان و ثلاثة دولار", 203, "AUD"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون دولار", 123, "AUD"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون دولار", 123456, "AUD"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون مليوناً و أربعمائة و ستة و خمسون ألفاً و سبعمائة و تسعة و ثمانون دولار", 123456789, "AUD"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون دولار و 12 سنتات", 123456.123, "AUD"),
				new CurrencyToWords_AR_AETestCase("سلبية مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون دولار و 12 سنتات", -123456.123, "AUD"),

				new CurrencyToWords_AR_AETestCase("صفر دولار", 0, "USD"),
				new CurrencyToWords_AR_AETestCase("واحد دولار", 1, "USD"),
				new CurrencyToWords_AR_AETestCase("اثنان دولار", 2, "USD"),
				new CurrencyToWords_AR_AETestCase("ثلاثة دولار", 3, "USD"),
				new CurrencyToWords_AR_AETestCase("عشرون دولار", 20, "USD"),
				new CurrencyToWords_AR_AETestCase("واحد و عشرون دولار", 21, "USD"),
				new CurrencyToWords_AR_AETestCase("ثلاثة و عشرون دولار", 23, "USD"),
				new CurrencyToWords_AR_AETestCase("مائة دولار", 100, "USD"),
				new CurrencyToWords_AR_AETestCase("مائة و واحد دولار", 101, "USD"),
				new CurrencyToWords_AR_AETestCase("مائة و اثنان دولار", 102, "USD"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة دولار", 103, "USD"),
				new CurrencyToWords_AR_AETestCase("مئتا دولار", 200, "USD"),
				new CurrencyToWords_AR_AETestCase("مئتان و واحد دولار", 201, "USD"),
				new CurrencyToWords_AR_AETestCase("مئتان و اثنان دولار", 202, "USD"),
				new CurrencyToWords_AR_AETestCase("مئتان و ثلاثة دولار", 203, "USD"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون دولار", 123, "USD"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون دولار", 123456, "USD"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون مليوناً و أربعمائة و ستة و خمسون ألفاً و سبعمائة و تسعة و ثمانون دولار", 123456789, "USD"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون دولار و 12 سنتات", 123456.123, "USD"),
				new CurrencyToWords_AR_AETestCase("سلبية مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون دولار و 12 سنتات", -123456.123, "USD"),

				new CurrencyToWords_AR_AETestCase("صفر جنية", 0, "SYP"),
				new CurrencyToWords_AR_AETestCase("واحد جنية", 1, "SYP"),
				new CurrencyToWords_AR_AETestCase("اثنان جنية", 2, "SYP"),
				new CurrencyToWords_AR_AETestCase("ثلاثة جنية", 3, "SYP"),
				new CurrencyToWords_AR_AETestCase("عشرون جنية", 20, "SYP"),
				new CurrencyToWords_AR_AETestCase("واحد و عشرون جنية", 21, "SYP"),
				new CurrencyToWords_AR_AETestCase("ثلاثة و عشرون جنية", 23, "SYP"),
				new CurrencyToWords_AR_AETestCase("مائة جنية", 100, "SYP"),
				new CurrencyToWords_AR_AETestCase("مائة و واحد جنية", 101, "SYP"),
				new CurrencyToWords_AR_AETestCase("مائة و اثنان جنية", 102, "SYP"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة جنية", 103, "SYP"),
				new CurrencyToWords_AR_AETestCase("مئتا جنية", 200, "SYP"),
				new CurrencyToWords_AR_AETestCase("مئتان و واحد جنية", 201, "SYP"),
				new CurrencyToWords_AR_AETestCase("مئتان و اثنان جنية", 202, "SYP"),
				new CurrencyToWords_AR_AETestCase("مئتان و ثلاثة جنية", 203, "SYP"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون جنية", 123, "SYP"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون جنية", 123456, "SYP"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون مليوناً و أربعمائة و ستة و خمسون ألفاً و سبعمائة و تسعة و ثمانون جنية", 123456789, "SYP"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون جنية و 12 قرش", 123456.123, "SYP"),
				new CurrencyToWords_AR_AETestCase("سلبية مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون جنية و 12 قرش", -123456.123, "SYP"),

				new CurrencyToWords_AR_AETestCase("صفر ريال", 0, "IRR"),
				new CurrencyToWords_AR_AETestCase("واحد ريال", 1, "IRR"),
				new CurrencyToWords_AR_AETestCase("اثنان ريال", 2, "IRR"),
				new CurrencyToWords_AR_AETestCase("ثلاثة ريال", 3, "IRR"),
				new CurrencyToWords_AR_AETestCase("عشرون ريال", 20, "IRR"),
				new CurrencyToWords_AR_AETestCase("واحد و عشرون ريال", 21, "IRR"),
				new CurrencyToWords_AR_AETestCase("ثلاثة و عشرون ريال", 23, "IRR"),
				new CurrencyToWords_AR_AETestCase("مائة ريال", 100, "IRR"),
				new CurrencyToWords_AR_AETestCase("مائة و واحد ريال", 101, "IRR"),
				new CurrencyToWords_AR_AETestCase("مائة و اثنان ريال", 102, "IRR"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة ريال", 103, "IRR"),
				new CurrencyToWords_AR_AETestCase("مئتا ريال", 200, "IRR"),
				new CurrencyToWords_AR_AETestCase("مئتان و واحد ريال", 201, "IRR"),
				new CurrencyToWords_AR_AETestCase("مئتان و اثنان ريال", 202, "IRR"),
				new CurrencyToWords_AR_AETestCase("مئتان و ثلاثة ريال", 203, "IRR"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ريال", 123, "IRR"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون ريال", 123456, "IRR"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون مليوناً و أربعمائة و ستة و خمسون ألفاً و سبعمائة و تسعة و ثمانون ريال", 123456789, "IRR"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون ريال", 123456.123, "IRR"),
				new CurrencyToWords_AR_AETestCase("سلبية مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون ريال", -123456.123, "IRR"),

				new CurrencyToWords_AR_AETestCase("صفر درهم", 0, "AED"),
				new CurrencyToWords_AR_AETestCase("واحد درهم", 1, "AED"),
				new CurrencyToWords_AR_AETestCase("اثنان درهم", 2, "AED"),
				new CurrencyToWords_AR_AETestCase("ثلاثة درهم", 3, "AED"),
				new CurrencyToWords_AR_AETestCase("عشرون درهم", 20, "AED"),
				new CurrencyToWords_AR_AETestCase("واحد و عشرون درهم", 21, "AED"),
				new CurrencyToWords_AR_AETestCase("ثلاثة و عشرون درهم", 23, "AED"),
				new CurrencyToWords_AR_AETestCase("مائة درهم", 100, "AED"),
				new CurrencyToWords_AR_AETestCase("مائة و واحد درهم", 101, "AED"),
				new CurrencyToWords_AR_AETestCase("مائة و اثنان درهم", 102, "AED"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة درهم", 103, "AED"),
				new CurrencyToWords_AR_AETestCase("مئتا درهم", 200, "AED"),
				new CurrencyToWords_AR_AETestCase("مئتان و واحد درهم", 201, "AED"),
				new CurrencyToWords_AR_AETestCase("مئتان و اثنان درهم", 202, "AED"),
				new CurrencyToWords_AR_AETestCase("مئتان و ثلاثة درهم", 203, "AED"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون درهم", 123, "AED"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون درهم", 123456, "AED"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون مليوناً و أربعمائة و ستة و خمسون ألفاً و سبعمائة و تسعة و ثمانون درهم", 123456789, "AED"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون درهم و 12 فلس", 123456.123, "AED"),
				new CurrencyToWords_AR_AETestCase("سلبية مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون درهم و 12 فلس", -123456.123, "AED"),

				new CurrencyToWords_AR_AETestCase("صفر ريال", 0, "SAR"),
				new CurrencyToWords_AR_AETestCase("واحد ريال", 1, "SAR"),
				new CurrencyToWords_AR_AETestCase("اثنان ريال", 2, "SAR"),
				new CurrencyToWords_AR_AETestCase("ثلاثة ريال", 3, "SAR"),
				new CurrencyToWords_AR_AETestCase("عشرون ريال", 20, "SAR"),
				new CurrencyToWords_AR_AETestCase("واحد و عشرون ريال", 21, "SAR"),
				new CurrencyToWords_AR_AETestCase("ثلاثة و عشرون ريال", 23, "SAR"),
				new CurrencyToWords_AR_AETestCase("مائة ريال", 100, "SAR"),
				new CurrencyToWords_AR_AETestCase("مائة و واحد ريال", 101, "SAR"),
				new CurrencyToWords_AR_AETestCase("مائة و اثنان ريال", 102, "SAR"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة ريال", 103, "SAR"),
				new CurrencyToWords_AR_AETestCase("مئتا ريال", 200, "SAR"),
				new CurrencyToWords_AR_AETestCase("مئتان و واحد ريال", 201, "SAR"),
				new CurrencyToWords_AR_AETestCase("مئتان و اثنان ريال", 202, "SAR"),
				new CurrencyToWords_AR_AETestCase("مئتان و ثلاثة ريال", 203, "SAR"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ريال", 123, "SAR"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون ريال", 123456, "SAR"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون مليوناً و أربعمائة و ستة و خمسون ألفاً و سبعمائة و تسعة و ثمانون ريال", 123456789, "SAR"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون ريال و 12 هللة", 123456.123, "SAR"),
				new CurrencyToWords_AR_AETestCase("سلبية مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون ريال و 12 هللة", -123456.123, "SAR"),

				new CurrencyToWords_AR_AETestCase("صفر دينار", 0, "TND"),
				new CurrencyToWords_AR_AETestCase("واحد دينار", 1, "TND"),
				new CurrencyToWords_AR_AETestCase("اثنان دينار", 2, "TND"),
				new CurrencyToWords_AR_AETestCase("ثلاثة دينار", 3, "TND"),
				new CurrencyToWords_AR_AETestCase("عشرون دينار", 20, "TND"),
				new CurrencyToWords_AR_AETestCase("واحد و عشرون دينار", 21, "TND"),
				new CurrencyToWords_AR_AETestCase("ثلاثة و عشرون دينار", 23, "TND"),
				new CurrencyToWords_AR_AETestCase("مائة دينار", 100, "TND"),
				new CurrencyToWords_AR_AETestCase("مائة و واحد دينار", 101, "TND"),
				new CurrencyToWords_AR_AETestCase("مائة و اثنان دينار", 102, "TND"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة دينار", 103, "TND"),
				new CurrencyToWords_AR_AETestCase("مئتا دينار", 200, "TND"),
				new CurrencyToWords_AR_AETestCase("مئتان و واحد دينار", 201, "TND"),
				new CurrencyToWords_AR_AETestCase("مئتان و اثنان دينار", 202, "TND"),
				new CurrencyToWords_AR_AETestCase("مئتان و ثلاثة دينار", 203, "TND"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون دينار", 123, "TND"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون دينار", 123456, "TND"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون مليوناً و أربعمائة و ستة و خمسون ألفاً و سبعمائة و تسعة و ثمانون دينار", 123456789, "TND"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون دينار و 123 مليم", 123456.123, "TND"),
				new CurrencyToWords_AR_AETestCase("سلبية مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون دينار و 123 مليم", -123456.123, "TND"),

				new CurrencyToWords_AR_AETestCase("صفر BAD", 0, "BAD"),
				new CurrencyToWords_AR_AETestCase("واحد BAD", 1, "BAD"),
				new CurrencyToWords_AR_AETestCase("اثنان BAD", 2, "BAD"),
				new CurrencyToWords_AR_AETestCase("ثلاثة BAD", 3, "BAD"),
				new CurrencyToWords_AR_AETestCase("عشرون BAD", 20, "BAD"),
				new CurrencyToWords_AR_AETestCase("واحد و عشرون BAD", 21, "BAD"),
				new CurrencyToWords_AR_AETestCase("ثلاثة و عشرون BAD", 23, "BAD"),
				new CurrencyToWords_AR_AETestCase("مائة BAD", 100, "BAD"),
				new CurrencyToWords_AR_AETestCase("مائة و واحد BAD", 101, "BAD"),
				new CurrencyToWords_AR_AETestCase("مائة و اثنان BAD", 102, "BAD"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة BAD", 103, "BAD"),
				new CurrencyToWords_AR_AETestCase("مئتا BAD", 200, "BAD"),
				new CurrencyToWords_AR_AETestCase("مئتان و واحد BAD", 201, "BAD"),
				new CurrencyToWords_AR_AETestCase("مئتان و اثنان BAD", 202, "BAD"),
				new CurrencyToWords_AR_AETestCase("مئتان و ثلاثة BAD", 203, "BAD"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون BAD", 123, "BAD"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون BAD", 123456, "BAD"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون مليوناً و أربعمائة و ستة و خمسون ألفاً و سبعمائة و تسعة و ثمانون BAD", 123456789, "BAD"),
				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون BAD", 123456.123, "BAD"),
				new CurrencyToWords_AR_AETestCase("سلبية مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون BAD", -123456.123, "BAD"),

				new CurrencyToWords_AR_AETestCase("مائة و ثلاثة و عشرون ألفاً و أربعمائة و ستة و خمسون دولار و اثنا عشر سنتات", 123456.123, "USD", SpecialFormat.LTR),

				new CurrencyToWords_AR_AETestCase("صفر ريال و واحد هللة", 0.01, "SAR", SpecialFormat.LTR),
				new CurrencyToWords_AR_AETestCase("صفر ريال و اثنان هللتين", 0.02, "SAR", SpecialFormat.LTR),
				new CurrencyToWords_AR_AETestCase("صفر ريال و ثلاثة هللات", 0.03, "SAR", SpecialFormat.LTR),
				new CurrencyToWords_AR_AETestCase("صفر ريال و عشرة هللات", 0.10, "SAR", SpecialFormat.LTR),
				new CurrencyToWords_AR_AETestCase("صفر ريال و أحد عشر هللة", 0.11, "SAR", SpecialFormat.LTR),
				new CurrencyToWords_AR_AETestCase("واحد ريال و اثنان هللتين", 1.02, "SAR", SpecialFormat.LTR),
			};
		}
	}
}
