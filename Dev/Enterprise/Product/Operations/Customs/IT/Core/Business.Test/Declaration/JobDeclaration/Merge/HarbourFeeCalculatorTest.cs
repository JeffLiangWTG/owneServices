using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(HarbourFeeCalculator))]
sealed class HarbourFeeCalculatorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(
			"When feeCalculationCriteria parameter is null",
			() => new HarbourFeeCalculator(feeCalculationData: null));

		var feeCalculationCriteriaMock = new Mock<IHarbourFeeCalculationDataProvider>();
		AssertExceptionThrown<ArgumentNullException>(
			"When harbourRateProvider parameter is null",
			() => new HarbourFeeCalculator(feeCalculationCriteriaMock.Object));
	}

	public void TestRateCode()
	{
		var harbourRate = Factory.New<RefHarbourRate>();
		harbourRate.ZXF_PortTaxType = "9AA";

		var feeCalculationCriteriaMock = new Mock<IHarbourFeeCalculationDataProvider>();
		feeCalculationCriteriaMock.Setup(x => x.HarbourRateProvider.HarbourRate).Returns(harbourRate);

		var harbourFeeCalculator = (IExtraFeeCalculator)new HarbourFeeCalculator(feeCalculationCriteriaMock.Object);
		AssertEquals("RateCode", "9AA", harbourFeeCalculator.RateCode);
	}

	public void TestRateCode_WhenHarbourRateDoesNotMatch()
	{
		var feeCalculationCriteriaMock = new Mock<IHarbourFeeCalculationDataProvider>();
		feeCalculationCriteriaMock.Setup(x => x.HarbourRateProvider.HarbourRate).Returns(value: null);

		var harbourFeeCalculator = (IExtraFeeCalculator)new HarbourFeeCalculator(feeCalculationCriteriaMock.Object);
		AssertEquals("RateCode", "", harbourFeeCalculator.RateCode);
	}

	public void TestCalculateExtraFees_WhenHarbourRateDoesNotMatch()
	{
		var feeCalculationCriteriaMock = new Mock<IHarbourFeeCalculationDataProvider>();
		feeCalculationCriteriaMock.Setup(x => x.HarbourRateProvider.HarbourRate).Returns(value: null);

		var harbourFeeCalculator = (IExtraFeeCalculator)new HarbourFeeCalculator(feeCalculationCriteriaMock.Object);
		var intermediateResults = harbourFeeCalculator.CalculateExtraFees();
		AssertEquals("When HarbourRate does not match, IntermediateResults count", 0, intermediateResults.Count());
	}

	public void TestCalculateExtraFees()
	{
		var harbourRate = Factory.New<RefHarbourRate>();
		harbourRate.ZXF_RateFormula = "0.5 * [TNE]";

		var feeCalculationCriteriaMock = new Mock<IHarbourFeeCalculationDataProvider>();
		feeCalculationCriteriaMock.Setup(x => x.HarbourRateProvider.HarbourRate).Returns(harbourRate);
		feeCalculationCriteriaMock.Setup(x => x.RateCalculationData).Returns(new HarbourFeeUniversalRateData(new ZWeight(200, "T")));

		var harbourFeeCalculator = (IExtraFeeCalculator)new HarbourFeeCalculator(feeCalculationCriteriaMock.Object);
		var intermediateResults = harbourFeeCalculator.CalculateExtraFees();
		AssertHarbourFees(intermediateResults, new FeeAssertionObject() { BaseValue = 200m, ChargeAmount = 100m, Rate = 0.5m });

		harbourRate.ZXF_RateFormula = "0.1 * [TNE]";
		feeCalculationCriteriaMock.Setup(x => x.RateCalculationData).Returns(new HarbourFeeUniversalRateData(new ZWeight(205, "KT")));
		harbourFeeCalculator = new HarbourFeeCalculator(feeCalculationCriteriaMock.Object);
		intermediateResults = harbourFeeCalculator.CalculateExtraFees();
		AssertHarbourFees(intermediateResults, new FeeAssertionObject() { BaseValue = 205000m, ChargeAmount = 20500m, Rate = 0.1m });
	}

	public void TestHarbourFeeAreCalculatedRoundingMetricTon()
	{
		var harbourRate = Factory.New<RefHarbourRate>();
		harbourRate.ZXF_RateFormula = "0.5 * [TNE]";

		var feeCalculationCriteriaMock = new Mock<IHarbourFeeCalculationDataProvider>();
		feeCalculationCriteriaMock.Setup(x => x.HarbourRateProvider.HarbourRate).Returns(harbourRate);
		feeCalculationCriteriaMock.Setup(x => x.RateCalculationData).Returns(new HarbourFeeUniversalRateData(new ZWeight(1.1, "T")));
		var harbourFeeCalculator = (IExtraFeeCalculator)new HarbourFeeCalculator(feeCalculationCriteriaMock.Object);
		var intermediateResults = harbourFeeCalculator.CalculateExtraFees();
		AssertHarbourFees(intermediateResults, new FeeAssertionObject() { BaseValue = 1m, ChargeAmount = 0.5m, Rate = 0.5m });

		feeCalculationCriteriaMock.Setup(x => x.RateCalculationData).Returns(new HarbourFeeUniversalRateData(new ZWeight(1.101, "T")));
		harbourFeeCalculator = new HarbourFeeCalculator(feeCalculationCriteriaMock.Object);
		intermediateResults = harbourFeeCalculator.CalculateExtraFees();
		AssertHarbourFees(intermediateResults, new FeeAssertionObject() { BaseValue = 2m, ChargeAmount = 1m, Rate = 0.5m });

		feeCalculationCriteriaMock.Setup(x => x.RateCalculationData).Returns(new HarbourFeeUniversalRateData(new ZWeight(0.101, "T")));
		harbourFeeCalculator = new HarbourFeeCalculator(feeCalculationCriteriaMock.Object);
		intermediateResults = harbourFeeCalculator.CalculateExtraFees();
		AssertHarbourFees(intermediateResults, new FeeAssertionObject() { BaseValue = 1m, ChargeAmount = 0.5m, Rate = 0.5m });

		feeCalculationCriteriaMock.Setup(x => x.RateCalculationData).Returns(new HarbourFeeUniversalRateData(new ZWeight(0.08, "T")));
		harbourFeeCalculator = new HarbourFeeCalculator(feeCalculationCriteriaMock.Object);
		intermediateResults = harbourFeeCalculator.CalculateExtraFees();
		AssertHarbourFees(intermediateResults, new FeeAssertionObject() { BaseValue = 0m, ChargeAmount = 0m, Rate = 0.5m });
	}

	void AssertHarbourFees(IEnumerable<IDutyCalculationIntermediateResult> intermediateResults, FeeAssertionObject feeAssertionObject)
	{
		AssertEquals("Harbour Fees Intermediate Results count count", 1, intermediateResults.Count());

		CombineAssertions("Calculated Harbour Fee", () =>
		{
			var harbourFee = intermediateResults.Single();

			AssertEquals("BaseValue", feeAssertionObject.BaseValue, harbourFee.BaseValue);
			AssertEquals("Amount", feeAssertionObject.ChargeAmount, harbourFee.Amount);
			AssertEquals("MethodOfCalculation", "TNE", harbourFee.MethodOfCalculation);
			AssertEquals("Rate", feeAssertionObject.Rate, harbourFee.Rate);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		new ITUniversalReferenceTestDataHelper(Factory).SetupHarbourRates();
	}
}
