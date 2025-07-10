using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(DutyCalculatorStrategy))]
	class DutyCalculatorStrategyTest : DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategy>
	{
		protected override DutyCalculatorStrategy GetDutyCalculatorStrategy() => new DutyCalculatorStrategy((JobDeclaration)Declaration);

		public override void TestCalculateDuties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			var header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header.JZ_OH_Supplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true)).PK;
			header.JZ_IncoTerm = "FOB";
			header.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var invLine1 = header.JobComInvoiceLines.AddNew();
			invLine1.JI_LinePrice = 107;
			invLine1.JI_Tariff = "1010";
			var invLine2 = header.JobComInvoiceLines.AddNew();
			invLine2.JI_LinePrice = 207;
			invLine2.JI_Tariff = "1010";
			var invLine3 = header.JobComInvoiceLines.AddNew();
			invLine3.JI_LinePrice = 307;
			invLine3.JI_Tariff = "1010";

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals("Entry Lines count", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("Entry Line Assiette TVA", 621m, declaration.CustomsEntryHeaders[0].MergedLines[0].CL_ValueForVAT);
		}

		public void TestCalculateEntryLineFeesForClearOriginalFee()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);

			var currentCountryCode = Core.Constants.CountryCodes.France;
			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);
			var ordVat = helper.CreateTaxOrFee("ORD", 0.20m, currentCountryCode, startDate, endDate);

			var dut = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.France, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, "Duty");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");
			var mop1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Y", "Deferred", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Yes);

			helper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dut.PK);

			var testTradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "STANDARD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));

			var rateType1 = helper.CreateCusRateType(Core.Constants.CountryCodes.France, "AMC");
			var rateCode1 = helper.CreateCusRateCode(Factory, "111", rateType1.PK);
			var rateCode2 = helper.CreateCusRateCode(Factory, "222", rateType1.PK);

			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.France, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.France, tariffType.PK, "10000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: UniversalReferenceConstants.RefCusRateFormula.Precalcule);
			var testApplicability1 = helper.CreateCusApplicability(rate1, testTradeGroup1, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			var rate2 = helper.CreateRefCusRate(tariff.PK, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: UniversalReferenceConstants.RefCusRateFormula.Precalcule);
			var testApplicability2 = helper.CreateCusApplicability(rate2, testTradeGroup1, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_InvoiceAmount = 1000;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invLine1 = invoice.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Tariff = "10000001";
			invLine1.JI_ZZF_NKTaxType = ordVat.ZZF_Code;
			invLine1.JI_LinePrice = 1000;
			invLine1.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.SouthAfrica;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var fee = entryLine.Fees.AddNew();
			fee.CF_ChargeType = "111";
			fee.CF_ChargeAmount = 20m;
			fee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;

			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_ChargeType = "222";
			fee2.CF_ChargeAmount = 30M;
			fee2.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Precalcule;

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
			AssertEquals("new fee created", 4, entryLine.Fees.Count);
			Assert(entryLine.Fees.Any(x => x.PK == fee.PK));
			Assert(!entryLine.Fees.Any(x => x.PK == fee2.PK));
			Assert(entryLine.Fees.Cast<CusEntryLineFee>().Any(x => x.CF_ChargeType == "222"));
		}

		public void TestCalculateEntryLineFeesForRateOverrideReasonCodeAndNationalFeeTypeCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: grouping);

			var testTradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "STANDARD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));

			var rateType1 = helper.CreateCusRateType(Core.Constants.CountryCodes.France, "AMC");
			var rateCode1 = helper.CreateCusRateCode(Factory, "M830", rateType1.PK);

			var rateType2 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode2 = helper.CreateCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.U165, rateType2.PK);

			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.France, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.France, tariffType.PK, "10000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: UniversalReferenceConstants.RefCusRateFormula.Precalcule);
			var testApplicability1 = helper.CreateCusApplicability(rate1, testTradeGroup1, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			var rate2 = helper.CreateRefCusRate(tariff.PK, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: "0");
			var testApplicability2 = helper.CreateCusApplicability(rate2, testTradeGroup1, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));

			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("STD", 0.1m, Core.Constants.CountryCodes.France);
			helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.France, "STD", category: "N381");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_InvoiceAmount = 1000;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invLine1 = invoice.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Tariff = "10000001";
			invLine1.JI_LinePrice = 1000;
			invLine1.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.SouthAfrica;
			invLine1.JI_ZZF_NKTaxType = "STD";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();
			entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
			AssertEquals("Should not contains fee with 0 rate and non 'PRE' code. ", 2, entryLine.Fees.Count);
			var fee1 = entryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Precalcule);
			AssertEquals("PRE", fee1.CF_RateOverrideReasonCode);
			AssertEquals("1I1", fee1.CF_ChargeType);
			AssertEquals("M830", fee1.NationalFeeTypeCode);

			var fee2 = entryLine.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "B00");
			AssertEquals("", fee2.CF_RateOverrideReasonCode);
			AssertEquals("B00", fee2.CF_ChargeType);
			AssertEquals("N381", fee2.NationalFeeTypeCode);
		}

		public void TestSetNationalTypeForDifferentRateTypes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: grouping);

			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusRateCodes.U167, UniversalReferenceConstants.RefCusRateCodes.U167);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusRateCodes.U397, UniversalReferenceConstants.RefCusRateCodes.U397);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusRateCodes.U167, "1674001000", "1674001000", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusRateCodes.U397, "3974001000", "3974001000", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			var rateTypeDTY = helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Universal.Constants.RateTypes.Duty);
			var rateCodeA20 = helper.LoadOrCreateNewCusRateCode(Factory, "A20", rateTypeDTY.PK);

			var rateTypeADD = helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Universal.Constants.RateTypes.AntiDumping);
			var rateCodeA30 = helper.LoadOrCreateNewCusRateCode(Factory, "A30", rateTypeADD.PK);
			var rateCodeA35 = helper.LoadOrCreateNewCusRateCode(Factory, "A35", rateTypeADD.PK);

			var rateTypeCVD = helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Universal.Constants.RateTypes.Countervailing);
			var rateCodeA40 = helper.LoadOrCreateNewCusRateCode(Factory, "A30", rateTypeCVD.PK);
			var rateCodeA45 = helper.LoadOrCreateNewCusRateCode(Factory, "A35", rateTypeCVD.PK);

			var rateViewA20 = Factory.New<RateView>();
			rateViewA20.ZZ2_ZZZ_NKDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			rateViewA20.ZZ2_RateFormula = "24.3 * [LPA]";
			rateViewA20.ZZ2_ZY1_RateCode = rateCodeA20.PK;

			var rateViewA30 = Factory.New<RateView>();
			rateViewA30.ZZ2_ZZZ_NKDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			rateViewA30.ZZ2_RateFormula = "24.3 * [LPA]";
			rateViewA30.ZZ2_ZY1_RateCode = rateCodeA30.PK;

			var rateViewA35 = Factory.New<RateView>();
			rateViewA35.ZZ2_ZZZ_NKDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			rateViewA35.ZZ2_RateFormula = "24.3 * [LPA]";
			rateViewA35.ZZ2_ZY1_RateCode = rateCodeA35.PK;

			var rateViewA40 = Factory.New<RateView>();
			rateViewA40.ZZ2_ZZZ_NKDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			rateViewA40.ZZ2_RateFormula = "24.3 * [LPA]";
			rateViewA40.ZZ2_ZY1_RateCode = rateCodeA40.PK;

			var rateViewA45 = Factory.New<RateView>();
			rateViewA45.ZZ2_ZZZ_NKDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			rateViewA45.ZZ2_RateFormula = "24.3 * [LPA]";
			rateViewA45.ZZ2_ZY1_RateCode = rateCodeA45.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_InvoiceAmount = 1000;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invLine1 = invoice.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Tariff = "1674001000";
			invLine1.JI_LinePrice = 1000;
			invLine1.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.SouthAfrica;
			invLine1.JI_ZZF_NKTaxType = "STD";

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];

			var calculatorStrategy = new DutyCalculatorStrategyForTest(declaration);

			var fee = entryLine.Fees.AddNew();
			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A00;
			calculatorStrategy.SetNationalType(entryLine, fee, rateViewA20);
			AssertEquals("chargetype is A00 and tariff is in U167 list then NationalFeeTypeCode is U167", UniversalReferenceConstants.RefCusRateCodes.U167, fee.NationalFeeTypeCode);

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A20;
			calculatorStrategy.SetNationalType(entryLine, fee, rateViewA20);
			AssertEquals("chargetype is A20 and tariff is not in U397 or U437 list then NationalFeeTypeCode is U425", UniversalReferenceConstants.RefCusRateCodes.U425, fee.NationalFeeTypeCode);

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A30;
			calculatorStrategy.SetNationalType(entryLine, fee, rateViewA30);
			AssertEquals("chargetype is A30 then NationalFeeTypeCode is U235", UniversalReferenceConstants.RefCusRateCodes.U235, fee.NationalFeeTypeCode);

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A35;
			calculatorStrategy.SetNationalType(entryLine, fee, rateViewA35);
			AssertEquals("chargetype is A35 then NationalFeeTypeCode is U235", UniversalReferenceConstants.RefCusRateCodes.U235, fee.NationalFeeTypeCode);

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A40;
			calculatorStrategy.SetNationalType(entryLine, fee, rateViewA40);
			AssertEquals("chargetype is A40 then NationalFeeTypeCode is U235", UniversalReferenceConstants.RefCusRateCodes.U235, fee.NationalFeeTypeCode);

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A45;
			calculatorStrategy.SetNationalType(entryLine, fee, rateViewA45);
			AssertEquals("chargetype is A45 then NationalFeeTypeCode is U235", UniversalReferenceConstants.RefCusRateCodes.U235, fee.NationalFeeTypeCode);

			invLine1.JI_Tariff = "3974001000";

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A00;
			calculatorStrategy.SetNationalType(entryLine, fee, rateViewA20);
			AssertEquals("chargetype is A00 and tariff is not in U167 or U397 list then NationalFeeTypeCode is U165", UniversalReferenceConstants.RefCusRateCodes.U165, fee.NationalFeeTypeCode);

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A20;
			calculatorStrategy.SetNationalType(entryLine, fee, rateViewA20);
			AssertEquals("chargetype is A20 and tariff is in U397 list then NationalFeeTypeCode is U397", UniversalReferenceConstants.RefCusRateCodes.U397, fee.NationalFeeTypeCode);

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A30;
			calculatorStrategy.SetNationalType(entryLine, fee, rateViewA30);
			AssertEquals("chargetype is A30 then NationalFeeTypeCode is U235", UniversalReferenceConstants.RefCusRateCodes.U235, fee.NationalFeeTypeCode);

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A35;
			calculatorStrategy.SetNationalType(entryLine, fee, rateViewA35);
			AssertEquals("chargetype is A35 then NationalFeeTypeCode is U235", UniversalReferenceConstants.RefCusRateCodes.U235, fee.NationalFeeTypeCode);

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A40;
			calculatorStrategy.SetNationalType(entryLine, fee, rateViewA40);
			AssertEquals("chargetype is A40 then NationalFeeTypeCode is U235", UniversalReferenceConstants.RefCusRateCodes.U235, fee.NationalFeeTypeCode);

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A45;
			calculatorStrategy.SetNationalType(entryLine, fee, rateViewA45);
			AssertEquals("chargetype is A45 then NationalFeeTypeCode is U235", UniversalReferenceConstants.RefCusRateCodes.U235, fee.NationalFeeTypeCode);
		}

		public override void TestCalculateValueForVAT()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_InvoiceAmount = 1000;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invLine1 = invoice.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Tariff = "1";

			var usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			var rate = usdCurrency.ExchangeRates.AddNew();
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.RE_StartDate = ZDateTime.Now.AddDays(-2);
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(2);
			rate.RE_SellRate = 2;
			rate.RE_GC = GlbCompany.CurrentCompany.PK;

			DoMerge(declaration);

			AssertEquals("Entries count", 1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Entry Lines count", 1, entry.MergedLines.Count);
			var entryLine = entry.MergedLines[0];
			AssertEquals("CL_ValueForVAT", 0m, entryLine.CL_ValueForVAT);

			invLine1.JI_LinePrice = 1000.34M;
			DoMerge(declaration);
			AssertEquals("CL_ValueForVAT", CalculateValueForVAT_ExpectedLocalCurrencyValue, entryLine.CL_ValueForVAT);

			invoice.JZ_RX_NKInvoice_Currency = "USD";
			DoMerge(declaration);
			AssertEquals("CL_ValueForVAT", CalculateValueForVAT_ExpectedUSDCurrencyValue, entryLine.CL_ValueForVAT);

			if (NonVATableDeductionChargeCode != EmptyChargeCode)
			{
				var deduction = invLine1.Charges.AddNew(NonVATableDeductionChargeCode, 20, declaration.LocalCurrencyCode);
				deduction.J7_IsGSTApplicable = false;
				DoMerge(declaration);
				AssertEquals("CL_ValueForVAT", CalculateValueForVAT_ExpectedUSDCurrencyValue - 20, entryLine.CL_ValueForVAT);
			}

			if (VATableAdditionChargeCode != EmptyChargeCode)
			{
				invLine1.Charges.RemoveAll();
				var addition = invLine1.Charges.AddNew(VATableAdditionChargeCode, 50);
				addition.J7_IsGSTApplicable = true;
				DoMerge(declaration);
				AssertEquals("CL_ValueForVAT", CalculateValueForVAT_ExpectedUSDCurrencyValue + 25M, entryLine.CL_ValueForVAT);

				invLine1.Charges.RemoveAll();
				var additionInvoiceHeader = invoice.Charges.AddNew(VATableAdditionChargeCode, 200);
				additionInvoiceHeader.J7_IsGSTApplicable = true;
				DoMerge(declaration);
				AssertEquals("CL_ValueForVAT", CalculateValueForVAT_ExpectedUSDCurrencyValue + 100M, entryLine.CL_ValueForVAT);

				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2";
				invoiceLine2.JI_LinePrice = 1000;
				DoMerge(declaration);
				AssertEquals("Entries count", 1, declaration.CustomsEntryHeaders.Count);
				var entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals("Entry Lines count", 2, entryHeader.MergedLines.Count);
				var entryLines = entryHeader.MergedLines.Cast<CusEntryLine>();
				AssertEquals("CL_ValueForVAT for Entry Line 1", CalculateValueForVAT_ExpectedEntryLine1Value, entryLines.SingleOrDefault(x => x.Tariff == "1").CL_ValueForVAT);
				AssertEquals("CL_ValueForVAT for Entry Line 2", CalculateValueForVAT_ExpectedEntryLine2Value, entryLines.SingleOrDefault(x => x.Tariff == "2").CL_ValueForVAT);
			}
		}

		public class DutyCalculatorStrategyForTest : DutyCalculatorStrategy
		{
			public DutyCalculatorStrategyForTest(JobDeclaration declaration)
				: base(declaration)
			{
			}

			public new void SetNationalType(EU.Business.Declaration.CusEntryLine entryLine, EU.Business.Declaration.CusEntryLineFee entryLineFee, RateView rateForCalculation) => base.SetNationalType(entryLine, entryLineFee, rateForCalculation);
		}

		public void TestCalculateEntryLineVatFeeIsDoneOnlyForImport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);

			var currentCountryCode = Core.Constants.CountryCodes.France;
			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);
			var ordVat = helper.CreateTaxOrFee("ORD", 0.20m, currentCountryCode, startDate, endDate);

			var dut = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.France, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, "Duty");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");
			var mop1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Y", "Deferred", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Yes);

			helper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dut.PK);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_InvoiceAmount = 1000;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invLine1 = invoice.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Tariff = "1";
			invLine1.JI_ZZF_NKTaxType = ordVat.ZZF_Code;
			invLine1.JI_LinePrice = 1000;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var fee = entryLine.Fees.AddNew();
			fee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			fee.CF_ChargeAmount = 20m;
			fee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();
			entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];

			AssertEquals("no new fee created", 1, entryLine.Fees.Count);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			merger = new LineMerger(declaration);
			merger.DoMerge();

			entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
			AssertEquals("new fee created", 2, entryLine.Fees.Count);
		}

		public void TestAddNewEntryLineFeeWhenZeroAdjustedRateAndPREOverrideReasonCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: grouping);

			var testTradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "STANDARD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));

			var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.France, "AMC");
			var rateCode = helper.CreateCusRateCode(Factory, "M830", rateType.PK);

			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.France, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.France, tariffType.PK, "10000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rate = helper.CreateRefCusRate(tariff.PK, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: UniversalReferenceConstants.RefCusRateFormula.Precalcule);
			var testApplicability = helper.CreateCusApplicability(rate, testTradeGroup1, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));

			Factory.Save();

			var entryLine = SetupCusEntryLineForTestAddNewEntryLineFee();

			AssertEquals("new fee created", 1, entryLine.Fees.Count);
		}

		public void TestAddNewEntryLineFeeWhenAdjustedRateAndNotPREOverrideReasonCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: grouping);

			var testTradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "STANDARD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));

			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.France, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.France, tariffType.PK, "10000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateRefCusTaxOrFeeType("VAT");
			helper.CreateTaxOrFee("STD", 0.1m, Core.Constants.CountryCodes.France);
			helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.France, "STD", category: "N381");
			Factory.Save();

			var entryLine = SetupCusEntryLineForTestAddNewEntryLineFee();

			AssertEquals("new fee created", 1, entryLine.Fees.Count);
		}

		CusEntryLine SetupCusEntryLineForTestAddNewEntryLineFee()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_InvoiceAmount = 1000;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invLine1 = invoice.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Tariff = "10000001";
			invLine1.JI_LinePrice = 1000;
			invLine1.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.SouthAfrica;
			invLine1.JI_ZZF_NKTaxType = "STD";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MergedLines.AddNew();

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();
			entryHeader = declaration.CustomsEntryHeaders[0];
			return entryHeader.MergedLines[0];
		}

		protected override ZString VATableAdditionChargeCode => FRCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge;
		protected override ZString NonVATableDeductionChargeCode => EmptyChargeCode;
		protected override ZDecimal ExpectedVATBaseValueA => 364m;
		protected override ZDecimal ExpectedVATBaseValueB => 92m;
	}
}
