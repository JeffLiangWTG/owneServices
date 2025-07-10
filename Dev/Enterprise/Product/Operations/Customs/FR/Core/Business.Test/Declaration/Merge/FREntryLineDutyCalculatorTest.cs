using CargoWise.Types;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.DutyCalculator.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(FREntryLineDutyCalculator))]
	sealed class FREntryLineDutyCalculatorTest : UniversalDutyCalculatorAbstractTest<EU.Business.Declaration.CusEntryLine, EUUniversalRateCalcData>
	{
		public void TestCalculateWithGivenRateFormula()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);

			var rateType1 = helper.CreateCusRateType(Core.Constants.CountryCodes.France, "DEV");
			rateType1.ZZR_CustomsValueFormula = "STATVAL";
			var rateCode1 = helper.CreateCusRateCode(Factory, "111", rateType1.PK);

			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.France, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.France, tariffType.PK, "10000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rateView = helper.CreateRate(tariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula: UniversalReferenceConstants.RefCusRateFormula.Precalcule);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.InvoiceLines.Add(invLine1);
			entryLine.CL_CustomsValue = 500.50;
			entryLine.CL_StatisticalValue = 1000.00;

			var dutyCalculator = new FREntryLineDutyCalculatorForTesting(entryLine);
			var result = dutyCalculator.GetConvertedFormulaForCleanUpFormula(rateView);
			AssertEquals("0", result);

			rateView.ZZ2_RateFormula = "VFD*0.5";
			result = dutyCalculator.GetConvertedFormulaForCleanUpFormula(rateView);
			AssertEquals("VFD*0.5", result);
		}

		protected override UniversalDutyCalculator<EU.Business.Declaration.CusEntryLine, EUUniversalRateCalcData> CreateDutyCalculator()
			=> new FREntryLineDutyCalculatorForTesting(Factory.New<CusEntryLine>());

		class FREntryLineDutyCalculatorForTesting : FREntryLineDutyCalculator
		{
			public FREntryLineDutyCalculatorForTesting(CusEntryLine entryLine) : base(entryLine, RateCalculationVisitorMode.Default) { }
			public new ZString GetConvertedFormulaForCleanUpFormula(RateView rateForCalculation) => base.GetConvertedFormulaForCleanUpFormula(rateForCalculation);
		}
	}
}
