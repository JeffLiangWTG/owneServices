using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using JobDeclaration = Enterprise.Customs.GB.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.GB.DocumentWrappers.Testing
{
	[TestedType(typeof(DutyCalculatorStrategySimulator))]
	class DutyCalculatorStrategySimulatorTest : DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategySimulator>
	{
		public void TestCalculateEntryLineFeesForTaxEstimator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

			var (stdTradeGroup, stdPreference, dtyTariff, dtyRateType) = PopulateTestReferenceData(declaration.GetDefaultDataGroupingCode(Customs.Business.DefaultDataGroupingType.Tariff));
			var rateCodeDtyA00 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dtyRateType.PK);
			var tariffDtyRateA00 = RefDataHelper.CreateRefCusRate(dtyTariff.PK, rateCodeDtyA00.PK, startDate, endDate, rateFormula: "VFD*0.7", preferencePk: stdPreference.PK);
			RefDataHelper.CreateCusApplicability(tariffDtyRateA00.PK, stdTradeGroup, startDate, endDate);

			tariffDtyRateA00.Factory.Save();
			Factory.Save();

			var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine1.JI_PrimaryPreference = stdPreference.ZZS_Preference;
			invLine1.JI_Tariff = dtyTariff.ZZ1_TariffCode;
			invLine1.JI_CustomsQuantity = 3;
			invLine1.JI_CustomsUnitQty = "KGM";

			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invLine1);
			entryLine.CL_CustomsValue = 20;

			var dutyCalculatorStrategy = new DutyCalculatorStrategySimulator(declaration, entryLine);
			var calculatedFees = dutyCalculatorStrategy.CalculateEntryLineFeesForTaxEstimator();

			CombineAssertions(() =>
			{
				AssertEquals("Count", 1, calculatedFees.Count);
				AssertEquals("Fee: 20m * 0.7", 14m, calculatedFees[0].result.Amount);
				AssertEquals("Ratecode", UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, calculatedFees[0].code);
			});
		}

		protected override DutyCalculatorStrategySimulator GetDutyCalculatorStrategy()
		{
			var declaration = (JobDeclaration)Declaration;
			return new DutyCalculatorStrategySimulator(declaration, declaration.ActiveEntryHeaders[0].AllEntryLines[0] as CusEntryLine);
		}

		protected override EU.Business.Declaration.JobDeclaration GetDeclarationForTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			return declaration;
		}

		protected override LineMergerTestHelper GetLineMergerTestHelper() => new Business.Testing.LineMergerTestHelper(Factory);

		(CusRefTradeGroupView, CusRefPreferenceView, TariffView, RefCusRateType) PopulateTestReferenceData(string dataGrouping)
		{
			var stdTradeGroup = RefDataHelper.CreateTradeGroup(dataGrouping, "STANDARD", startDate, endDate);
			var impTariffType = RefDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			var stdPreference = RefDataHelper.CreatePreferenceView("STD", "Standard", dataGrouping);
			Factory.Save();
			var dtyTariff = RefDataHelper.CreateTariff(dataGrouping, impTariffType.PK, "1111111", startDate, endDate, taxOrFeeCode: "DTY");
			var dtyRateType = RefDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, "Duty");
			return (stdTradeGroup, stdPreference, dtyTariff, dtyRateType);
		}

		protected override ZDecimal ExpectedVATChargeAmountA => 70.9588m;
		protected override ZDecimal ExpectedVATBaseValueA => 322.54m;
		protected override ZDecimal ExpectedVATChargeAmountB => 17.6704m;
		protected override ZDecimal ExpectedVATBaseValueB => 80.32m;

		protected override IReadOnlyList<FeeAssertionObject> EntryLineAExpectedFees => new FeeAssertionObject[]
		{
			new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A00RateCode, ChargeAmount = 8.538m, BaseValue = 42.69m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
			new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A00RateCode, ChargeAmount = 15m, BaseValue = 3m, Rate = 5m, MethodOfCalculation = "HLT", OverrideReason = "" },
			new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A00RateCode, ChargeAmount = 60m, BaseValue = 500m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" },
			new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A20RateCode, ChargeAmount = 150m, BaseValue = 500m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
			new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A30RateCode, ChargeAmount = 3.5m, BaseValue = 7m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
			new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A40RateCode, ChargeAmount = 12.807m, BaseValue = 42.69m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
			new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A40RateCode, ChargeAmount = 30m, BaseValue = 500m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
			new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.VatRateCode, ChargeAmount = ExpectedVATChargeAmountA, BaseValue = ExpectedVATBaseValueA, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
		};

		protected override IReadOnlyList<FeeAssertionObject> EntryLineBExpectedFees => new FeeAssertionObject[]
		{
			new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A00RateCode, ChargeAmount = 2.196m, BaseValue = 10.98m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
			new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A00RateCode, ChargeAmount = 15.6m, BaseValue = 130m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" },
			new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A20RateCode, ChargeAmount = 39m, BaseValue = 130m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
			new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A20RateCode, ChargeAmount = 0.8m, BaseValue = 4m, Rate = 0.2m, MethodOfCalculation = "LTR", OverrideReason = "" },
			new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A30RateCode, ChargeAmount = 0.65m, BaseValue = 1.3m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
			new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A40RateCode, ChargeAmount = 3.294m, BaseValue = 10.98m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
			new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A40RateCode, ChargeAmount = 7.8m, BaseValue = 130m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
			new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.VatRateCode, ChargeAmount = ExpectedVATChargeAmountB, BaseValue = ExpectedVATBaseValueB, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
		};

		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;

		readonly ZDateTime startDate = ZDateTime.Today.AddYears(-1);
		readonly ZDateTime endDate = ZDateTime.Today.AddYears(1);
	}
}
