using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class GoodsItemDutyCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculate_WithFormulaZero_ShouldAssumeValueForDuty()
		{
			var rateView = SetUpTariffRateData("ADD", "0");

			var result = calculatorDeparture.Calculate(rateView);

			AssertEquals($"Rate formula '0*VFD' should be used", 0m, result.ResultAmount);
		}

		public void TestCalculate_WithADDOrCVDRateTypeAndCertCondition_ShouldAssumeCertificatePresentForDeparture()
		{
			const string rateFormula = "if(has(\"CERT\",\"D008\"), VFD*0.258, VFD*0.379)";

			CombineAssertions(() =>
			{
				AssertCertificateAssumedPresentForDeparture("ADD", expectCertAssumedPresent: true);
				AssertCertificateAssumedPresentForDeparture("CVD", expectCertAssumedPresent: true);
				AssertCertificateAssumedPresentForDeparture("XYZ", expectCertAssumedPresent: false);
			});

			void AssertCertificateAssumedPresentForDeparture(string rateType, bool expectCertAssumedPresent)
			{
				var rateView = SetUpTariffRateData(rateType, rateFormula);

				var resultDeparture = calculatorDeparture.Calculate(rateView);

				AssertNotNull(resultDeparture);
				if (expectCertAssumedPresent)
				{
					AssertEquals($"Departure: '{rateFormula}' should assume CERT is present for {rateType}", 258.000m, resultDeparture.ResultAmount);
				}
				else
				{
					AssertEquals($"Departure: '{rateFormula}' should not assume CERT is present for {rateType}", 379.000m, resultDeparture.ResultAmount);
				}

				var resultArrival = calculatorArrival.Calculate(rateView);

				AssertEquals($"Arrival: '{rateFormula}' should not assume CERT is present for {rateType}", 379.000m, resultArrival.ResultAmount);
			}
		}

		public void TestCalculate_WithADDorCVDRateTypeAndNestedIfCondition_ShouldTakeHighestRateForDeparture()
		{
			const string rateFormula = "if(has(\"CERT\",\"D017\"), 12.345, if(has(\"CERT\",\"D018\"), VFD*0.258, VFD*0.003))";

			CombineAssertions(() =>
			{
				AssertHighestRateIsUsedForDeparture("ADD", expectHighestRateIsUsed: true);
				AssertHighestRateIsUsedForDeparture("CVD", expectHighestRateIsUsed: true);
				AssertHighestRateIsUsedForDeparture("XYZ", expectHighestRateIsUsed: false);
			});

			void AssertHighestRateIsUsedForDeparture(string rateType, bool expectHighestRateIsUsed)
			{
				var rateView = SetUpTariffRateData(rateType, rateFormula);

				var resultDeparture = calculatorDeparture.Calculate(rateView);

				AssertNotNull(resultDeparture);
				if (expectHighestRateIsUsed)
				{
					AssertEquals($"Departure: '{rateFormula}' should use highest rate for {rateType}", 258.000m, resultDeparture.ResultAmount);
				}
				else
				{
					AssertEquals($"Departure: '{rateFormula}' should not use highest rate for {rateType}", 3.000m, resultDeparture.ResultAmount);
				}

				var resultArrival = calculatorArrival.Calculate(rateView);

				AssertNotNull(resultArrival);
				AssertEquals($"Arrival: '{rateFormula}' should not use highest rate for {rateType}", 3.000m, resultArrival.ResultAmount);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var headerDeparture = Factory.New<NctsHeader>();
			headerDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			headerDeparture.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItemDeparture = headerDeparture.Bills.AddNew().GoodsItems.AddNew();
			goodsItemDeparture.BY_MonetaryValue = 1000;
			calculatorDeparture = new GoodsItemDutyCalculator(goodsItemDeparture);

			var headerArrival = Factory.New<NctsHeader>();
			headerArrival.SetMovementType(NctsMovementType.Codes.Arrival);
			headerArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItemArrival = headerArrival.Bills.AddNew().ArrivalGoodsItems.AddNew();
			goodsItemArrival.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			goodsItemArrival.BY_MonetaryValue = 1000;
			calculatorArrival = new GoodsItemDutyCalculator(goodsItemArrival);

			Factory.Save();
		}

		RateView SetUpTariffRateData(string rateType, string rateFormula)
		{
			const string dataGrouping = Core.Constants.CountryCodes.EuropeanUnion;
			const string tariffCode = "3826001051";
			const string tariffTypeCode = "IMP";
			const string rateCode = "A30";

			var startDate = ZDate.Today.AddMonths(-1);
			var endDate = ZDate.Today.AddMonths(1);

			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = refDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, tariffTypeCode);
			Factory.Save();

			var tariff = refDataHelper.LoadOrCreateNewTariff(dataGrouping, tariffType.PK, tariffCode, startDate, endDate);

			var rateTypeBizObj = refDataHelper.CreateNewOrGetExistingRateType(dataGrouping, rateType);
			rateTypeBizObj.ZZR_RateType = rateType;
			Factory.Save();

			var rateCodeBizObj = refDataHelper.LoadOrCreateNewCusRateCode(Factory, rateCode, rateTypeBizObj.PK);
			rateCodeBizObj.ZY1_RateCode = rateCode;
			Factory.Save();

			return refDataHelper.CreateRate(tariff, rateCodeBizObj.PK, startDate, endDate, rateFormula: rateFormula, dataGrouping: dataGrouping);
		}

		GoodsItemDutyCalculator calculatorDeparture;
		GoodsItemDutyCalculator calculatorArrival;
	}
}
