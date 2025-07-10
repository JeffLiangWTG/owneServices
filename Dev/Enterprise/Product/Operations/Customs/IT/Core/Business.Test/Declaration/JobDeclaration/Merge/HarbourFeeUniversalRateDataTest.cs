using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class HarbourFeeUniversalRateDataTest : TestCaseWithFactory
{
	public void TestDateOfValuation()
	{
		var harbourFeeUniversalRateData = new HarbourFeeUniversalRateData(ZWeight.Empty);
		AssertExceptionThrown<InvalidOperationException>("Must be exception when invoking DateOfValuation", "Harbour Fee formula should not require the value: DateOfValuation", () => _ = harbourFeeUniversalRateData.DateOfValuation);
	}

	public void TestValueForDuty()
	{
		var harbourFeeUniversalRateData = new HarbourFeeUniversalRateData(ZWeight.Empty);
		AssertExceptionThrown<InvalidOperationException>("Must be exception when invoking ValueForDuty", "Harbour Fee formula should not require the value: ValueForDuty", () => _ = harbourFeeUniversalRateData.ValueForDuty);
	}

	public void TestCustomsValue()
	{
		var harbourFeeUniversalRateData = new HarbourFeeUniversalRateData(ZWeight.Empty);
		AssertExceptionThrown<InvalidOperationException>("Must be exception when invoking CustomsValue", "Harbour Fee formula should not require the value: CustomsValue", () => _ = harbourFeeUniversalRateData.CustomsValue);
	}

	public void TestUnitOfMeasureValueList()
	{
		var harbourFeeUniversalRateData = new HarbourFeeUniversalRateData(new ZWeight(2000, "KG"));
		var unitOfMeasureValueList = harbourFeeUniversalRateData.UnitOfMeasureValueList;
		AssertEquals("Unit of Measure Value List count", 1, unitOfMeasureValueList.Count);
		var unitOfMeasureValue = unitOfMeasureValueList.Single();
		CombineAssertions("Assert Unit of Measure", () =>
		{
			AssertEquals("Key", "TNE", unitOfMeasureValue.Key);
			AssertEquals("Value", 2m, unitOfMeasureValue.Value);
		});

		harbourFeeUniversalRateData = new HarbourFeeUniversalRateData(new ZWeight(2101, "KG"));
		unitOfMeasureValueList = harbourFeeUniversalRateData.UnitOfMeasureValueList;
		AssertEquals("Unit of Measure Value List count", 1, unitOfMeasureValueList.Count);
		unitOfMeasureValue = unitOfMeasureValueList.Single();
		CombineAssertions("Assert Unit of Measure", () =>
		{
			AssertEquals("Key", "TNE", unitOfMeasureValue.Key);
			AssertEquals("Value", 3m, unitOfMeasureValue.Value);
		});

		harbourFeeUniversalRateData = new HarbourFeeUniversalRateData(new ZWeight(0, "KG"));
		unitOfMeasureValueList = harbourFeeUniversalRateData.UnitOfMeasureValueList;
		AssertEquals("Unit of Measure Value List count", 1, unitOfMeasureValueList.Count);
		unitOfMeasureValue = unitOfMeasureValueList.Single();
		CombineAssertions("Assert Unit of Measure", () =>
		{
			AssertEquals("Key", "TNE", unitOfMeasureValue.Key);
			AssertEquals("Value", 0m, unitOfMeasureValue.Value);
		});
	}

	public void TestUnitOfMeasureValueListWhenGrossWeightUQIsInvalid()
	{
		var harbourFeeUniversalRateData = new HarbourFeeUniversalRateData(new ZWeight(2000, "Q"));
		var unitOfMeasureValueList = harbourFeeUniversalRateData.UnitOfMeasureValueList;
		AssertEquals("When WeightUQ is not a valid UoM, Unit of Measure Value List count", 0, unitOfMeasureValueList.Count);
	}

	public void TestUnitOfMeasureValueListAreInRoundingMetricTon()
	{
		var harbourFeeUniversalRateData = new HarbourFeeUniversalRateData(new ZWeight(1.1, "T"));
		var unitOfMeasureValueList = harbourFeeUniversalRateData.UnitOfMeasureValueList;
		AssertEquals("Unit of Measure Value List count", 1, unitOfMeasureValueList.Count);
		var unitOfMeasureValue = unitOfMeasureValueList.Single();
		CombineAssertions("Assert Unit of Measure", () =>
		{
			AssertEquals("Key", "TNE", unitOfMeasureValue.Key);
			AssertEquals("Value", 1m, unitOfMeasureValue.Value);
		});

		harbourFeeUniversalRateData = new HarbourFeeUniversalRateData(new ZWeight(1.101, "T"));
		unitOfMeasureValueList = harbourFeeUniversalRateData.UnitOfMeasureValueList;
		AssertEquals("Unit of Measure Value List count", 1, unitOfMeasureValueList.Count);
		unitOfMeasureValue = unitOfMeasureValueList.Single();
		CombineAssertions("Assert Unit of Measure", () =>
		{
			AssertEquals("Key", "TNE", unitOfMeasureValue.Key);
			AssertEquals("Value", 2m, unitOfMeasureValue.Value);
		});
	}

	public void TestCountrySpecificValueList()
	{
		var harbourFeeUniversalRateData = new HarbourFeeUniversalRateData(ZWeight.Empty);
		AssertNotNull("CountrySpecificValueList", harbourFeeUniversalRateData.CountrySpecificValueList);
		AssertEquals("CountrySpecificValueList count", 0, harbourFeeUniversalRateData.CountrySpecificValueList.Count);
	}

	public void TestAdditionalInformationList()
	{
		var harbourFeeUniversalRateData = new HarbourFeeUniversalRateData(ZWeight.Empty);
		AssertNotNull("AdditionalInformationList", harbourFeeUniversalRateData.AdditionalInformationList);
		AssertEquals("AdditionalInformationList count", 0, harbourFeeUniversalRateData.AdditionalInformationList.Count);
	}

	public void TestMeursingExpressionList()
	{
		var harbourFeeUniversalRateData = new HarbourFeeUniversalRateData(ZWeight.Empty);
		AssertNotNull("MeursingExpressionList", harbourFeeUniversalRateData.MeursingExpressionList);
		AssertEquals("MeursingExpressionList count", 0, harbourFeeUniversalRateData.MeursingExpressionList.Count);
	}
}
