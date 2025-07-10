using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DutyComponentCalculatorTest : TestCaseWithFactory
	{
		public void TestDutyResultHasFlatUQAndAmountSet()
		{
			SetTestData();
			dummyDutyData.CustomsValueExposed = 4m;
			rateInfo.DutyRateField = DutyRateField.FirstQty;
			rateInfo.Rate = 10m;
			rateInfo.Unit = "LA";
			dummyDutyRate.RatesApplicableExposed = new RateInfo[] { rateInfo };

			DutyResult result = calculator.CalculateDuty(dummyDutyData, dummyDutyRate, null);
			AssertEquals("Result.FlatRate", 10m, result.FlatRateAmount);
			AssertEquals("Result.FlatRateUQ", "LA", result.FlatRateUQ);
		}

		public void TestHugeDutyResult()
		{
			SetTestData();
			dummyDutyData.CustomsValueExposed = 365310m;
			dummyDutyData.FirstQtyExposed = 89100m;
			dummyDutyData.RandomLineDutyDataExposed.FirstUQ = "KG";
			dummyDutyData.RandomLineDutyDataExposed.FirstTariffNumber = "24012000";

			rateInfo.DutyRateField = DutyRateField.FirstQty;
			rateInfo.Rate = 300.50m;
			rateInfo.Unit = "KG";
			dummyDutyRate.RatesApplicableExposed = new RateInfo[] { rateInfo };

			DutyResult result = calculator.CalculateDuty(dummyDutyData, dummyDutyRate, null);
			AssertEquals(26774550.00m, result.Amount.Amount);
		}

		[ExpectNoExceptions]
		public void TestGetDutyRateDoesNotBlowOutWithNullAsAdditionalDutyRateCanBeNull()
		{
			DutyResult result = calculator.GetDutyResult(dummyDutyData, null);
			AssertEquals("Duty should be empty", 0m, result.Amount.Amount);
		}

		public void TestRoundFinalResult()
		{
			SetTestData();
			dummyDutyData.CustomsValueExposed = 4m;
			rateInfo.DutyRateField = DutyRateField.CustomsValue;
			rateInfo.Rate = 0.7m;
			dummyDutyRate.RatesApplicableExposed = new RateInfo[] { rateInfo };
			DutyResult result = calculator.CalculateDuty(dummyDutyData, dummyDutyRate, null);
			AssertEquals("Amount rounded", 0.02m, result.Amount.Amount);
		}

		public void TestCalculateDutyWithInCalcOrFreeOrInfoRate()
		{
			dummyDutyRate.CalculationTypeExposed = Constants.DutyCalcTypes.Free;
			DutyResult result = calculator.CalculateDuty(dummyDutyData, dummyDutyRate, null);
			AssertEquals("No duty with Free rate", 0m, result.Amount.Amount);

			dummyDutyRate.CalculationTypeExposed = Constants.DutyCalcTypes.InCalc;
			result = calculator.CalculateDuty(dummyDutyData, dummyDutyRate, null);
			AssertEquals("No duty with incalculatible rate", 0m, result.Amount.Amount);

			dummyDutyRate.CalculationTypeExposed = Constants.DutyCalcTypes.Info;
			result = calculator.CalculateDuty(dummyDutyData, dummyDutyRate, null);
			AssertEquals("No duty with Info only rate", 0m, result.Amount.Amount);
		}

		public void TestCalculateWithMultipleRates()
		{
			SetTestData();
			rateInfo.DutyRateField = DutyRateField.CustomsValue;

			RateInfo rate2 = new RateInfo();
			rate2.DutyRateField = DutyRateField.FirstQty;
			rate2.Rate = 50m;
			rate2.Unit = "LA";
			dummyDutyRate.RatesApplicableExposed = new RateInfo[] { rateInfo, rate2 };

			DutyResult result = calculator.CalculateDuty(dummyDutyData, dummyDutyRate, null);
			AssertEquals("Amount: 10000m * 0.05 + 50 * 200", 10500m, result.Amount.Amount);
		}

		public void TestCalculateDutyWithCalcWithCustomsValue()
		{
			SetTestData();
			rateInfo.DutyRateField = DutyRateField.CustomsValue;
			dummyDutyRate.RatesApplicableExposed = new RateInfo[] { rateInfo };
			DutyResult result = calculator.CalculateDuty(dummyDutyData, dummyDutyRate, null);
			AssertEquals("Amount", 500m, result.Amount.Amount);
		}

		public void TestCalculateDutyWithCalcWithFirstQty()
		{
			SetTestData();
			rateInfo.DutyRateField = DutyRateField.FirstQty;
			dummyDutyRate.RatesApplicableExposed = new RateInfo[] { rateInfo };
			DutyResult result = calculator.CalculateDuty(dummyDutyData, dummyDutyRate, null);
			AssertEquals("Amount", 1000m, result.Amount.Amount);
		}

		public void TestCalculateDutyWithCalcWithSecondQty()
		{
			SetTestData();
			rateInfo.DutyRateField = DutyRateField.SecondQty;
			dummyDutyRate.RatesApplicableExposed = new RateInfo[] { rateInfo };
			DutyResult result = calculator.CalculateDuty(dummyDutyData, dummyDutyRate, null);
			AssertEquals("Amount", 500m, result.Amount.Amount);
		}

		public void TestCalculateDutyWithLowerWithOtherDutyFactor()
		{
			SetTestData();
			rateInfo.DutyRateField = DutyRateField.OtherDutyFactor;
			dummyDutyRate.RatesApplicableExposed = new RateInfo[] { rateInfo };
			DutyResult result = calculator.CalculateDuty(dummyDutyData, dummyDutyRate, null);
			AssertEquals("Amount", 2500m, result.Amount.Amount);
		}

		public void TestCalculateDutyWithHigherWithSecondDutyRate()
		{
			SetTestData();
			rateInfo.DutyRateField = DutyRateField.CustomsValue;

			dummyDutyRate.CalculationTypeExposed = Constants.DutyCalcTypes.Higher;

			RateInfo additionalRateInfo = new RateInfo();
			additionalRateInfo.DutyRateField = DutyRateField.CustomsValue;
			additionalRateInfo.Rate = 7m;//7% > 5% in the first rate
			additionalDutyRate.RatesApplicableExposed = new RateInfo[] { additionalRateInfo };

			DutyResult result = calculator.CalculateDuty(dummyDutyData, dummyDutyRate, additionalDutyRate);
			AssertEquals("Rate from Additional should be picked up to calculate", 700m, result.Amount.Amount);

			additionalRateInfo.Rate = 3m;//3% < 5% in the first rate
			additionalDutyRate.RatesApplicableExposed = new RateInfo[] { additionalRateInfo };
			result = calculator.CalculateDuty(dummyDutyData, dummyDutyRate, additionalDutyRate);
			AssertEquals("Rate from Additional should be picked up to calculate", 500m, result.Amount.Amount);
		}

		public void TestCalculateDutyWithLowerWithSecondDutyRate()
		{
			SetTestData();
			rateInfo.DutyRateField = DutyRateField.CustomsValue;

			dummyDutyRate.CalculationTypeExposed = Constants.DutyCalcTypes.Lower;

			RateInfo additionalRateInfo = new RateInfo();
			additionalRateInfo.DutyRateField = DutyRateField.CustomsValue;
			additionalRateInfo.Rate = 3m;//3% < 5% in the first rate
			additionalDutyRate.RatesApplicableExposed = new RateInfo[] { additionalRateInfo };

			DutyResult result = calculator.CalculateDuty(dummyDutyData, dummyDutyRate, additionalDutyRate);
			AssertEquals("Rate from Additional should be picked up to calculate", 300m, result.Amount.Amount);

			additionalRateInfo.Rate = 7m;//7% > 5% in the first rate
			additionalDutyRate.RatesApplicableExposed = new RateInfo[] { additionalRateInfo };

			result = calculator.CalculateDuty(dummyDutyData, dummyDutyRate, additionalDutyRate);
			AssertEquals("Rate from Additional should be picked up to calculate", 500m, result.Amount.Amount);
		}

		#region Implementation

		DummyCMRDutyData dummyDutyData;
		DummyCMRDutyRate dummyDutyRate;
		DummyCMRDutyRate additionalDutyRate;
		DutyComponentCalculator calculator;
		RateInfo rateInfo;

		void SetTestData()
		{
			rateInfo = new RateInfo();
			rateInfo.Unit = "LA";
			rateInfo.Rate = 5m;
			dummyDutyRate.RatesApplicableExposed = new RateInfo[] { rateInfo };
			dummyDutyRate.CalculationTypeExposed = Constants.DutyCalcTypes.Calc;

			dummyDutyRate.RatesApplicableExposed = new RateInfo[] { rateInfo };
			dummyDutyData.CustomsValueExposed = 10000m;

			DutyDataFromInvoiceLine ramdonLineDutyData = new DutyDataFromInvoiceLine();
			ramdonLineDutyData.FirstUQ = "LA";
			ramdonLineDutyData.SecondUQ = "LA";
			dummyDutyData.RandomLineDutyDataExposed = ramdonLineDutyData;

			dummyDutyData.FirstQtyExposed = 200m;

			dummyDutyData.SecondQtyExposed = 100m;

			dummyDutyData.OtherDutyFactorExposed = 500m;
		}

		protected override void SetUp()
		{
			base.SetUp();
			dummyDutyData = new DummyCMRDutyData(Factory);
			dummyDutyRate = new DummyCMRDutyRate();
			additionalDutyRate = new DummyCMRDutyRate();
			calculator = new DutyComponentCalculator();
		}

		#endregion
	}
}
