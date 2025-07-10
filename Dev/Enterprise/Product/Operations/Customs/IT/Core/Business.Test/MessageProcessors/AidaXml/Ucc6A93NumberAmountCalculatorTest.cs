using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class Ucc6A93NumberAmountCalculatorTest : TestCase
{
	public void TestCalculateAmount_OnlyAdditions()
	{
		var feeOne = CreateFeeMock(chargeType: "DTY", methodOfPayment: "F", amount: 12.34m);
		var feeTwo = CreateFeeMock(chargeType: "VAT", methodOfPayment: "F", amount: 10m);
		var feeThree = CreateFeeMock(chargeType: "OTh", methodOfPayment: "F", amount: 3.99m);

		var fees = new IFee[] { feeOne.Object, feeTwo.Object, feeThree.Object };
		IUcc6A93NumberAmountCalculator calculator = new Ucc6A93NumberAmountCalculator(fees);
		var amount = calculator.CalculateAmount();

		AssertEquals("Amount", 26.33m, amount);
	}

	public void TestCalculateAmount_FeesWith406And407()
	{
		var feeOne = CreateFeeMock(chargeType: "DTY", methodOfPayment: "F", amount: 12.34m);
		var feeTwo = CreateFeeMock(chargeType: "VAT", methodOfPayment: "F", amount: 10m);
		var feeThree = CreateFeeMock(chargeType: "406", methodOfPayment: "F", amount: 3.99m);

		var fees = new IFee[] { feeOne.Object, feeTwo.Object, feeThree.Object };
		IUcc6A93NumberAmountCalculator calculator = new Ucc6A93NumberAmountCalculator(fees);
		var amount = calculator.CalculateAmount();

		AssertEquals("Amount", 18.35m, amount);
	}

	public void TestCalculateAmount()
	{
		var fees = System.Array.Empty<IFee>();
		IUcc6A93NumberAmountCalculator calculator = new Ucc6A93NumberAmountCalculator(fees);
		var amount = calculator.CalculateAmount();
		AssertEquals("Amount", 0m, amount);

		var feeOne = CreateFeeMock(chargeType: "DTY", methodOfPayment: "F", amount: 12.37814m);
		var feeTwo = CreateFeeMock(chargeType: "VAT", methodOfPayment: "G", amount: 11.98213m);
		fees = new IFee[] { feeOne.Object, feeTwo.Object };
		calculator = new Ucc6A93NumberAmountCalculator(fees);
		amount = calculator.CalculateAmount();
		AssertEquals("Amount", 24.36027m, amount);

		feeOne = CreateFeeMock(chargeType: "DTY", methodOfPayment: "F", amount: 12.37814m);
		feeTwo = CreateFeeMock(chargeType: "406", methodOfPayment: "G", amount: 13.11231m);
		fees = new IFee[] { feeOne.Object, feeTwo.Object };
		calculator = new Ucc6A93NumberAmountCalculator(fees);
		amount = calculator.CalculateAmount();
		AssertEquals("Amount", -0.73417m, amount);
	}

	static Mock<IFee> CreateFeeMock(string chargeType, string methodOfPayment, decimal amount)
	{
		var feeMock = new Mock<IFee>();
		feeMock.Setup(f => f.ChargeType).Returns(chargeType);
		feeMock.Setup(f => f.MethodOfPayment).Returns(methodOfPayment);
		feeMock.Setup(f => f.Amount).Returns(amount);
		return feeMock;
	}
}
