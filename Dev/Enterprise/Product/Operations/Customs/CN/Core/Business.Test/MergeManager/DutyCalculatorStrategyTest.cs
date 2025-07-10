using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing.MergeManager
{
	[TestedType(typeof(DutyCalculatorStrategy))]
	class DutyCalculatorStrategyTest : Customs.Business.Testing.DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategy>
	{
		public override void TestCalculateDuties()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dtyRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, "DTY");
			var expRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, "EXP");
			var excRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, "EXC");
			var addRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, "ADD");
			var cvdRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, "CVD");
			excRateType.ZZR_CustomsValueFormula = "CV + DTY";
			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem).PK;
			helper.CreateRefCusTaxOrFeeType("OTH");
			helper.CreateTaxOrFee("VAT", 0.14, Core.Constants.CountryCodes.China, new ZDateTime(2019, 02, 18), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTaxOrFee("VAT", 0.15, Core.Constants.CountryCodes.China, new ZDateTime(2019, 02, 19), ZDateTime.MaxSmallDateTimeValue.AddDays(-1));
			helper.CreateTaxOrFee("VAT", 0.16, Core.Constants.CountryCodes.China, new ZDateTime(2019, 02, 20), ZDateTime.MaxSmallDateTimeValue.AddDays(-2));
			Factory.Save();
			var dtyRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dtyRateType.PK);
			var expRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "EXP", expRateType.PK);
			var excRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "EXC", excRateType.PK);
			var addRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "ADD", addRateType.PK);
			var cvdRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "CVD", cvdRateType.PK);
			Factory.Save();
			var stdPreference = helper.CreatePreferenceForCountry("STANDARD", "STANDARD", Core.Constants.CountryCodes.China);
			var mfnPreference = helper.CreatePreferenceForCountry("MFN", "MFN", Core.Constants.CountryCodes.China);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.China, "TEST", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			var tariffA = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "TARIFFA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "VAT");
			CreateAdditionalElements(helper, "TARIFFA");
			var aMFNRate = helper.CreateRate(tariffA, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.2", mfnPreference.PK);
			helper.CreateCusApplicability(aMFNRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var aSTDRate = helper.CreateRate(tariffA, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.9", stdPreference.PK);
			helper.CreateCusApplicability(aSTDRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var expRate = helper.CreateRate(tariffA, expRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ROUND(VFD / (1 + 0.4), 0) * 0.4");
			helper.CreateCusApplicability(expRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var excRate = helper.CreateRate(tariffA, excRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD / (1 - 0.15) * 0.15");
			helper.CreateCusApplicability(excRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var addRate = helper.CreateRate(tariffA, addRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "CV");
			helper.CreateCusApplicability(addRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cvdRate = helper.CreateRate(tariffA, cvdRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "CV");
			helper.CreateCusApplicability(cvdRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			declaration.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.China;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction1.PK;
			invoiceLine.JI_Tariff = "TARIFFA";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_DutyMode = DutyModeList.Codes._1;
			invoiceLine.JI_PrimaryPreference = "STANDARD";
			invoiceLine.JI_LinePrice = 599.33m;
			invoiceLine.XC_GoodsSpecModel = "2千克/箱||YYYYY等|||其他1/其他2/其他3|<其他澳大利亚公司><Allothers><0.736><0.69><0>";

			declaration.DoMerge();
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.CustomsEntry, 599m, 539.1m, 200.84m, 350.90m, 440.86m, 413.31m);
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.RecordListing, 599m, 0m, 0m, 0m, 0m, 0m);

			invoiceLine.JI_PrimaryPreference = "MFN";
			declaration.DoMerge();
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.CustomsEntry, 599m, 119.8m, 126.85m, 271.97m, 440.86m, 413.31m);
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.RecordListing, 599m, 0m, 0m, 0m, 0m, 0m);

			invoiceLine.JI_DutyMode = DutyModeList.Codes._2;
			declaration.DoMerge();
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.CustomsEntry, 599m, 59.9m, 126.85m, 131.19m, 440.86m, 413.31m);
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.RecordListing, 599m, 0m, 0m, 0m, 0m, 0m);

			invoiceLine.JI_DutyMode = DutyModeList.Codes._3;
			declaration.DoMerge();
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.CustomsEntry, 599m, 0m, 126.85m, 0m, 440.86m, 413.31m);
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.RecordListing, 599m, 0m, 0m, 0m, 0m, 0m);

			invoiceLine.JI_DutyMode = DutyModeList.Codes._4;
			declaration.DoMerge();
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.CustomsEntry, 599m, 0m, 126.85m, 252.80m, 440.86m, 413.31m);
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.RecordListing, 599m, 0m, 0m, 0m, 0m, 0m);

			invoiceLine.JI_DutyMode = DutyModeList.Codes._8;
			declaration.DoMerge();
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.CustomsEntry, 599m, 59.9m, 0m, 140.78m, 440.86m, 413.31m);
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.RecordListing, 599m, 0m, 0m, 0m, 0m, 0m);

			invoiceLine.JI_DutyMode = DutyModeList.Codes._9;
			declaration.DoMerge();
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.CustomsEntry, 599m, 119.8m, 0m, 0m, 440.86m, 413.31m);
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.RecordListing, 599m, 0m, 0m, 0m, 0m, 0m);

			invoiceLine.JI_LinePrice = 50m;
			invoiceLine.JI_DutyMode = DutyModeList.Codes._1;
			declaration.DoMerge();
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.CustomsEntry, 50m, 0m, 0m, 0m, 0m, 0m);
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.RecordListing, 50m, 0m, 0m, 0m, 0m, 0m);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			invoiceLine.JI_LinePrice = 599.33m;
			invoiceLine.JI_DutyMode = DutyModeList.Codes._1;
			declaration.DoMerge();
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.CustomsEntry, 599m, 171.2m, 0m, 0m, 0m, 0m);
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.RecordListing, 599m, 0m, 0m, 0m, 0m, 0m);

			invoiceLine.JI_DutyMode = DutyModeList.Codes._2;
			declaration.DoMerge();
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.CustomsEntry, 599m, 85.6m, 0m, 0m, 0m, 0m);
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.RecordListing, 599m, 0m, 0m, 0m, 0m, 0m);

			invoiceLine.JI_DutyMode = DutyModeList.Codes._3;
			declaration.DoMerge();
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.CustomsEntry, 599m, 0m, 0m, 0m, 0m, 0m);
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.RecordListing, 599m, 0m, 0m, 0m, 0m, 0m);

			invoiceLine.JI_DutyMode = DutyModeList.Codes._4;
			declaration.DoMerge();
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.CustomsEntry, 599m, 0m, 0m, 0m, 0m, 0m);
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.RecordListing, 599m, 0m, 0m, 0m, 0m, 0m);

			invoiceLine.JI_DutyMode = DutyModeList.Codes._8;
			declaration.DoMerge();
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.CustomsEntry, 599m, 85.6m, 0m, 0m, 0m, 0m);
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.RecordListing, 599m, 0m, 0m, 0m, 0m, 0m);

			invoiceLine.JI_DutyMode = DutyModeList.Codes._9;
			declaration.DoMerge();
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.CustomsEntry, 599m, 171.2m, 0m, 0m, 0m, 0m);
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.RecordListing, 599m, 0m, 0m, 0m, 0m, 0m);

			invoiceLine.JI_LinePrice = 50m;
			invoiceLine.JI_DutyMode = DutyModeList.Codes._1;
			declaration.DoMerge();
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.CustomsEntry, 50m, 0m, 0m, 0m, 0m, 0m);
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.RecordListing, 50m, 0m, 0m, 0m, 0m, 0m);
		}

		public void TestCalculateDutiesWithCVINUSD()
		{
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			usdCurrency.ExchangeRates.DeleteAll();
			var rate = usdCurrency.ExchangeRates.AddNew();
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.RE_StartDate = ZDateTime.Today;
			rate.RE_ExpiryDate = ZDateTime.Today;
			rate.RE_SellRate = 6.75m;
			Factory.Save();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dtyRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, "DTY");
			var expRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, "EXP");
			var excRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, "EXC");
			var addRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, "ADD");
			var cvdRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, "CVD");
			excRateType.ZZR_CustomsValueFormula = "CV + DTY";
			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem).PK;
			helper.CreateRefCusTaxOrFeeType("OTH");
			helper.CreateTaxOrFee("VAT", 0.16, Core.Constants.CountryCodes.China);
			Factory.Save();
			var dtyRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dtyRateType.PK);
			var expRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "EXP", expRateType.PK);
			var excRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "EXC", excRateType.PK);
			var addRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "ADD", addRateType.PK);
			var cvdRateCode = helper.LoadOrCreateNewCusRateCode(Factory, "CVD", cvdRateType.PK);
			Factory.Save();
			var stdPreference = helper.CreatePreferenceForCountry("STANDARD", "STANDARD", Core.Constants.CountryCodes.China);
			var mfnPreference = helper.CreatePreferenceForCountry("MFN", "MFN", Core.Constants.CountryCodes.China);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.China, "TEST", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			var tariffA = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "TARIFFA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "VAT");
			CreateAdditionalElements(helper, "TARIFFA");
			var aMFNRate = helper.CreateRate(tariffA, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.2", mfnPreference.PK);
			helper.CreateCusApplicability(aMFNRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var aSTDRate = helper.CreateRate(tariffA, dtyRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "IF(CVINUSD > 100 , 1.1, 0.9) * CV", stdPreference.PK);
			helper.CreateCusApplicability(aSTDRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var expRate = helper.CreateRate(tariffA, expRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ROUND(VFD / (1 + 0.4), 0) * 0.4");
			helper.CreateCusApplicability(expRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var excRate = helper.CreateRate(tariffA, excRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD / (1 - 0.15) * 0.15");
			helper.CreateCusApplicability(excRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var addRate = helper.CreateRate(tariffA, addRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "CV");
			helper.CreateCusApplicability(addRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cvdRate = helper.CreateRate(tariffA, cvdRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "CV");
			helper.CreateCusApplicability(cvdRate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			declaration.CustomsEntryInstructions.AddNew();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.China;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction1.PK;
			invoiceLine.JI_Tariff = "TARIFFA";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_DutyMode = DutyModeList.Codes._1;
			invoiceLine.JI_PrimaryPreference = "STANDARD";
			invoiceLine.JI_LinePrice = 599.33m;
			invoiceLine.XC_GoodsSpecModel = "2千克/箱||YYYYY等|||其他1/其他2/其他3|<其他澳大利亚公司><Allothers><0.736><0.69><0>";

			declaration.DoMerge();
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.CustomsEntry, 599m, 539.1m, 200.84m, 350.90m, 440.86m, 413.31m);
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.RecordListing, 599m, 0m, 0m, 0m, 0m, 0m);

			invoiceLine.JI_LinePrice = 700.50m;
			declaration.DoMerge();
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.CustomsEntry, 701, 771.1m, 259.78m, 437.04m, 515.94m, 483.69m);
			AssertDutyAndTaxAmounts(declaration, EntryTypeList.Codes.RecordListing, 701, 0m, 0m, 0m, 0m, 0m);
		}

		static void CreateAdditionalElements(UniversalReferenceTestDataHelper helper, string tariffCode)
		{
			helper.CreateCustomsTariff(tariffCode, "00000", "00010", "00423", "00352", "00009", "00005", "99999",
				OriginalManufacturerNameCNStrategy.AdditionalElementCode,
				OriginalManufacturerNameENStrategy.AdditionalElementCode,
				AntiDumpingDutyRateStrategy.AdditionalElementCode,
				CountervailingDutyRateStrategy.AdditionalElementCode,
				MeetsPricePromiseStrategy.AdditionalElementCode);

			helper.CreateAdditionalElement("00000", "品名");
			helper.CreateAdditionalElement("00423", "针入度");
			helper.CreateAdditionalElement("00352", "加工方法");
			helper.CreateAdditionalElement("00010", "包装规格");
			helper.CreateAdditionalElement("00009", "GTIN");
			helper.CreateAdditionalElement("00005", "CAS");
			helper.CreateAdditionalElement("99999", "其他");
			helper.CreateAdditionalElement(OriginalManufacturerNameCNStrategy.AdditionalElementCode, "原厂商中文名称");
			helper.CreateAdditionalElement(OriginalManufacturerNameENStrategy.AdditionalElementCode, "原厂商英文名称");
			helper.CreateAdditionalElement(AntiDumpingDutyRateStrategy.AdditionalElementCode, "反倾销税率");
			helper.CreateAdditionalElement(CountervailingDutyRateStrategy.AdditionalElementCode, "反补贴税率");
			helper.CreateAdditionalElement(MeetsPricePromiseStrategy.AdditionalElementCode, "是否符合价格承诺");
		}

		static void AssertDutyAndTaxAmounts(JobDeclaration declaration, ZString messageType, ZDecimal expectedCustomsValue, ZDecimal expectedCustomsDuty, ZDecimal expectedExcise, ZDecimal expectedVAT, ZDecimal expectedADD, ZDecimal expectedCVD)
		{
			var entryLine = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(entry => entry.CH_MessageType == messageType).MergedLines[0];
			var message = $"MessageType:{messageType} DutyMode:{entryLine.RandomLine.JI_DutyMode} Preference:{entryLine.RandomLine.JI_PrimaryPreference} ";
			CombineAssertions(() =>
			{
				AssertEquals(message + "Customs Value", expectedCustomsValue, entryLine.CL_CustomsValue);
				AssertEquals(message + "Customs Duty", expectedCustomsDuty, entryLine.Fees.GetAmount(entryLine.Header.IsEntering ? "DTY" : "EXP"));
				AssertEquals(message + "Excise", expectedExcise, entryLine.Fees.GetAmount("EXC"));
				AssertEquals(message + "VAT", expectedVAT, entryLine.Fees.GetAmount("VAT"));
				AssertEquals(message + "Duty Percent", expectedCustomsValue.IsEmpty ? 0m : decimal.Round(expectedCustomsDuty / expectedCustomsValue * 100, 5), entryLine.CL_DutyPercent);
				AssertEquals(message + "ADD", expectedADD, entryLine.Fees.GetAmount("ADD"));
				AssertEquals(message + "CVD", expectedCVD, entryLine.Fees.GetAmount("CVD"));
			});
		}

		protected override DutyCalculatorStrategy GetDutyCalculatorStrategy() => new DutyCalculatorStrategy((JobDeclaration)Declaration);

		protected override bool ExpectedCanBeNegative => true;

		protected override int ExpectedDutyDecimalPlace => 2;
	}
}
