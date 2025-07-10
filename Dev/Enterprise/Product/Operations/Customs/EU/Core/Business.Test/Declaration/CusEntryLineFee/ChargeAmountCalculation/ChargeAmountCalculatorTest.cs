using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class ChargeAmountCalculatorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ChargeAmountCalculator(lineFee: null));
		}

		public void TestCalculate()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			var calculator = (IChargeAmountCalculator)new ChargeAmountCalculator(lineFee);

			lineFee.CF_BaseValue = 100m;
			lineFee.CF_Rate = 2m;
			AssertEquals("Calculated Charge Amount (Base Value * Rate)", 200m, calculator.Calculate());
		}
	}
}
