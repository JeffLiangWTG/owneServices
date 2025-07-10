using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using ICustomsFee = CargoWise.Customs.IT.MessageContracts.Declaration.IFee;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class FeeWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new FeeWrapper(null));
	}

	public void TestChargeType()
	{
		var lineFeeWrapper = GetNewFeeWrapper();
		AssertEquals(nameof(ICustomsFee.ChargeType), "", lineFeeWrapper.ChargeType);

		lineFee.CF_ChargeType = "A00";
		lineFeeWrapper = GetNewFeeWrapper();
		AssertEquals(nameof(ICustomsFee.ChargeType), "A00", lineFeeWrapper.ChargeType);
	}

	public void TestMethodOfPayment()
	{
		var lineFeeWrapper = GetNewFeeWrapper();
		AssertEquals(nameof(ICustomsFee.MethodOfPayment), "", lineFeeWrapper.MethodOfPayment);

		lineFee.CF_MethodOfPayment = "X";
		lineFeeWrapper = GetNewFeeWrapper();
		AssertEquals(nameof(ICustomsFee.MethodOfPayment), "X", lineFeeWrapper.MethodOfPayment);
	}

	public void TestBaseValue()
	{
		CombineAssertions("When Method of Calcualtion is Percentage", () =>
		{
			lineFee.CF_MethodOfCalculation = "%";
			var lineFeeWrapper = GetNewFeeWrapper();
			AssertEquals(nameof(ICustomsFee.BaseValue), 0m, lineFeeWrapper.BaseValue);

			lineFee.CF_BaseValue = 15.39;
			lineFeeWrapper = GetNewFeeWrapper();
			AssertEquals(nameof(ICustomsFee.BaseValue), 15.39m, lineFeeWrapper.BaseValue);
		});

		CombineAssertions("When Method of Calcualtion is not Percentage", () =>
		{
			lineFee.CF_MethodOfCalculation = "ARG";
			var lineFeeWrapper = GetNewFeeWrapper();
			AssertEquals(nameof(ICustomsFee.BaseValue), null, lineFeeWrapper.BaseValue);

			lineFee.CF_BaseValue = 15.39;
			lineFeeWrapper = GetNewFeeWrapper();
			AssertEquals(nameof(ICustomsFee.BaseValue), null, lineFeeWrapper.BaseValue);
		});
	}

	public void TestRate()
	{
		var lineFeeWrapper = GetNewFeeWrapper();
		AssertEquals(nameof(ICustomsFee.Rate), 0m, lineFeeWrapper.Rate);

		lineFee.CF_Rate = 48.01;
		lineFeeWrapper = GetNewFeeWrapper();
		AssertEquals(nameof(ICustomsFee.Rate), 48.01m, lineFeeWrapper.Rate);
	}

	public void TestChargeAmount()
	{
		var lineFeeWrapper = GetNewFeeWrapper();
		AssertEquals(nameof(ICustomsFee.ChargeAmount), 0m, lineFeeWrapper.ChargeAmount);

		lineFee.CF_ChargeAmount = 23.9832;
		lineFeeWrapper = GetNewFeeWrapper();
		AssertEquals(nameof(ICustomsFee.ChargeAmount), 23.98m, lineFeeWrapper.ChargeAmount);
	}

	public void TestQuantity()
	{
		CombineAssertions("When Method of Calcualtion is Percentage", () =>
		{
			lineFee.CF_MethodOfCalculation = "%";
			var lineFeeWrapper = GetNewFeeWrapper();
			AssertEquals(nameof(ICustomsFee.Quantity), null, lineFeeWrapper.Quantity);

			lineFee.CF_BaseValue = 23.9832;
			lineFeeWrapper = GetNewFeeWrapper();
			AssertEquals(nameof(ICustomsFee.Quantity), null, lineFeeWrapper.Quantity);
		});

		CombineAssertions("When Method of Calcualtion is not Percentage", () =>
		{
			lineFee.CF_MethodOfCalculation = "ARG";
			lineFee.CF_BaseValue = 0m;
			var lineFeeWrapper = GetNewFeeWrapper();
			AssertEquals(nameof(ICustomsFee.Quantity), 0m, lineFeeWrapper.Quantity);

			lineFee.CF_BaseValue = 23.9832;
			lineFeeWrapper = GetNewFeeWrapper();
			AssertEquals(nameof(ICustomsFee.Quantity), 23.9832m, lineFeeWrapper.Quantity);
		});
	}

	public void TestUnitOfMeasure()
	{
		CombineAssertions("When Method of Calculation is Percentage", () =>
		{
			lineFee.CF_MethodOfCalculation = "%";
			var lineFeeWrapper = GetNewFeeWrapper();
			AssertEquals(nameof(ICustomsFee.UnitOfMeasure), null, lineFeeWrapper.UnitOfMeasure);
		});

		CombineAssertions("When Method of Calculation is not Percentage", () =>
		{
			lineFee.CF_MethodOfCalculation = "ARG";
			var lineFeeWrapper = GetNewFeeWrapper();
			AssertEquals(nameof(ICustomsFee.UnitOfMeasure), "ARG", lineFeeWrapper.UnitOfMeasure);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		lineFee = Factory.New<JobDeclaration>()
			.CustomsEntryHeaders.AddNew()
			.MergedLines.AddNew()
			.Fees.AddNew();
	}

	CusEntryLineFee lineFee;

	ICustomsFee GetNewFeeWrapper() => new FeeWrapper(lineFee);
}
