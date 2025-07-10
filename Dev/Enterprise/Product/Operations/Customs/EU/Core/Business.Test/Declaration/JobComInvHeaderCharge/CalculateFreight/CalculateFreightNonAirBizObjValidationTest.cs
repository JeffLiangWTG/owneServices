using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class CalculateFreightNonAirBizObjValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTotalAmount()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(nonAirCalculator.TotalAmountInfo);
		}

		public void TestCheckPercentageFreightToEUBorder_EnteredValue()
		{
			AssertEnteredValueWithinRange(nonAirCalculator.PercentageFreightToEUBorderInfo, "%Freight to EU Border must be between 0 and 100.");
		}

		public void TestCheckPercentageFreightToEUBorder_SumOfPercentages()
		{
			const string sumRangeErrorMessage = "%Freight to EU Border + %Freight EU to Destination Country cannot be greater than 100.";
			CombineAssertions(() =>
			{
				nonAirCalculator.PercentageFreightEUToDestinationCountry = 41;
				nonAirCalculator.PercentageFreightToEUBorder = 60;
				AssertHasError("Sum > 100", nonAirCalculator.PercentageFreightToEUBorderInfo, sumRangeErrorMessage);
				nonAirCalculator.PercentageFreightToEUBorder = 59;
				AssertNoError("Sum = 100", nonAirCalculator.PercentageFreightToEUBorderInfo, sumRangeErrorMessage);
			});
		}

		public void TestCheckPercentageFreightEUToDestinationCountry_EnteredValue()
		{
			AssertEnteredValueWithinRange(nonAirCalculator.PercentageFreightEUToDestinationCountryInfo, "%Freight EU to Destination Country must be between 0 and 100.");
		}

		public void TestCheckPercentageFreightEUToDestinationCountry_SumOfPercentages()
		{
			const string sumRangeErrorMessage = "%Freight to EU Border + %Freight EU to Destination Country cannot be greater than 100.";
			CombineAssertions(() =>
			{
				nonAirCalculator.PercentageFreightToEUBorder = 71;
				nonAirCalculator.PercentageFreightEUToDestinationCountry = 30;
				AssertHasError("Sum > 100", nonAirCalculator.PercentageFreightEUToDestinationCountryInfo, sumRangeErrorMessage);
				nonAirCalculator.PercentageFreightEUToDestinationCountry = 29;
				AssertNoError("Sum = 100", nonAirCalculator.PercentageFreightEUToDestinationCountryInfo, sumRangeErrorMessage);
			});
		}

		public void TestCheckCurrency_Mandatory()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(nonAirCalculator.CurrencyInfo);
		}

		public void TestCheckCurrency_List()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(nonAirCalculator.CurrencyInfo, "!@X", Core.Constants.CurrencyCodes.Australia);
		}

		public void TestCheckCurrencyVsAmount()
		{
			nonAirCalculator.Currency = ZString.Empty;
			AssertHasErrorContaining(nonAirCalculator.CurrencyInfo, CalculateFreightBizObjValidation.InconsistentCurrencies);
			nonAirCalculator.TotalAmount = 10m;
			nonAirCalculator.Validation.ValidateCurrency();
			AssertNoErrorContaining(nonAirCalculator.CurrencyInfo, CalculateFreightBizObjValidation.InconsistentCurrencies);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			nonAirCalculator = CalculateFreightNonAirBizObj.New(invoice.Charges, declaration);
		}
		CalculateFreightNonAirBizObj nonAirCalculator;

		void AssertEnteredValueWithinRange(ZPropertyInfo propertyInfo, ZString expectedError)
		{
			CombineAssertions(() =>
			{
				propertyInfo.Value = new ZDecimal(-1);
				AssertHasError("Less than min", propertyInfo, expectedError);
				propertyInfo.Value = ZDecimal.Zero;
				AssertNoError("Min", propertyInfo, expectedError);
				propertyInfo.Value = new ZDecimal(101);
				AssertHasError("Greater than max", propertyInfo, expectedError);
				propertyInfo.Value = new ZDecimal(100);
				AssertNoError("Max", propertyInfo, expectedError);
			});
		}
	}
}
