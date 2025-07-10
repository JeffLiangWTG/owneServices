using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SADDutyTaxFeeWrapperTest : TestCaseWithFactory
{
	public void TestType()
	{
		CombineAssertions(() =>
		{
			fee.Setup(m => m.ChargeType).Returns("A00");
			AssertEquals(nameof(wrapper.Type), "A00", wrapper.Type);

			fee.Setup(m => m.ChargeType).Returns("B00");
			AssertEquals(nameof(wrapper.Type), "405", wrapper.Type);
		});
	}

	public void TestBase()
	{
		fee.Setup(m => m.BaseValue).Returns(10m);
		AssertEquals(nameof(wrapper.Base), 10m, wrapper.Base);
	}

	public void TestCalculationFactor1()
	{
		AssertEquals(nameof(wrapper.CalculationFactor1), "X", wrapper.CalculationFactor1);
	}

	public void TestRate1()
	{
		fee.Setup(m => m.Rate).Returns(1m);
		AssertEquals(nameof(wrapper.Rate1), 1m, wrapper.Rate1);
	}

	public void TestCalculationFactor2()
	{
		fee.Setup(m => m.MethodOfCalculation).Returns("%");
		AssertEquals(nameof(wrapper.CalculationFactor2), "%", wrapper.CalculationFactor2);

		fee.Setup(m => m.MethodOfCalculation).Returns(ZString.Empty);
		AssertEquals($"{nameof(wrapper.CalculationFactor2)} when MethodOfCalculation is empty", ZString.Empty, wrapper.CalculationFactor2);

		fee.Setup(m => m.MethodOfCalculation).Returns("DTN");
		AssertEquals($"{nameof(wrapper.CalculationFactor2)} when MethodOfCalculation is 'DTN'", ZString.Empty, wrapper.CalculationFactor2);
	}

	public void TestRate2()
	{
		AssertNull(nameof(wrapper.Rate2), wrapper.Rate2);
	}

	public void TestCalculationFactor3()
	{
		AssertEquals(nameof(wrapper.CalculationFactor3), ZString.Empty, wrapper.CalculationFactor3);
	}

	public void TestRate3()
	{
		AssertNull(nameof(wrapper.Rate3), wrapper.Rate3);
	}

	public void TestCalculationFactor4()
	{
		AssertEquals(nameof(wrapper.CalculationFactor4), ZString.Empty, wrapper.CalculationFactor4);
	}

	public void TestAmount()
	{
		fee.Setup(m => m.Amount).Returns(112.12m);
		AssertEquals(nameof(wrapper.Amount), 112.12m, wrapper.Amount);
	}

	public void TestMethodOfPayment()
	{
		fee.Setup(m => m.MethodOfPayment).Returns("1");
		AssertEquals(nameof(wrapper.MethodOfPayment), "1", wrapper.MethodOfPayment);
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("The fee is required", () => new SADDutyTaxFeeWrapper(null));
	}

	protected override void SetUp()
	{
		base.SetUp();
		fee = new Mock<IFee>();
		wrapper = new SADDutyTaxFeeWrapper(fee.Object);
	}

	Mock<IFee> fee;
	SADDutyTaxFeeWrapper wrapper;
}
