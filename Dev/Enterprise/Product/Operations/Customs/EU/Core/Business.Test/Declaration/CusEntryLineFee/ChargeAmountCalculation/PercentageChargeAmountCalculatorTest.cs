using System;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class PercentageChargeAmountCalculatorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PercentageChargeAmountCalculator(chargeAmountCalculator: null));
		}

		public void TestCalculate()
		{
			var chargeAmountCalculatorMock = new Mock<IChargeAmountCalculator>();
			chargeAmountCalculatorMock
				.Setup(x => x.Calculate())
				.Returns(999m);

			var percentageChargeAmountCalculator = (IChargeAmountCalculator)new PercentageChargeAmountCalculator(chargeAmountCalculatorMock.Object);
			AssertEquals("Calculated Charge Amount (Calculate Value from input calculator / 100)", 9.99m, percentageChargeAmountCalculator.Calculate());
		}
	}
}
