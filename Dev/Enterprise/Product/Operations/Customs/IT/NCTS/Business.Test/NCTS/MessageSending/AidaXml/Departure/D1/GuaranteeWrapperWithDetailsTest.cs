using System;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class GuaranteeWrapperWithDetailsTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When guarantee is null", () => new GuaranteeWrapperWithDetails(guarantee: null));
	}

	public void TestGuaranteeType()
	{
		CombineAssertions(() =>
		{
			var wrapper = GetWrapper();
			AssertNullOrEmpty(nameof(IGuarantee.GuaranteeType), wrapper.GuaranteeType);

			guarantee.PW_BondType = "0";
			wrapper = GetWrapper();
			AssertEquals(nameof(IGuarantee.GuaranteeType), "0", wrapper.GuaranteeType);
		});
	}

	public void TestOtherGuaranteeReference()
	{
		CombineAssertions(() =>
		{
			var wrapper = GetWrapper();
			AssertNullOrEmpty(nameof(IGuarantee.OtherGuaranteeReference), wrapper.OtherGuaranteeReference);

			guarantee.PW_BondNumber2 = "BOND2";
			wrapper = GetWrapper();
			AssertEquals(nameof(IGuarantee.OtherGuaranteeReference), "BOND2", wrapper.OtherGuaranteeReference);
		});
	}

	public void TestGRN()
	{
		CombineAssertions(() =>
		{
			var wrapper = GetWrapper();
			AssertNullOrEmpty(nameof(IGuarantee.GRN), wrapper.GRN);

			guarantee.PW_BondNumber = "BOND1";
			wrapper = GetWrapper();
			AssertEquals(nameof(IGuarantee.GRN), "BOND1", wrapper.GRN);
		});
	}

	public void TestAccessCode()
	{
		CombineAssertions(() =>
		{
			var wrapper = GetWrapper();
			AssertNullOrEmpty(nameof(IGuarantee.AccessCode), wrapper.AccessCode);

			guarantee.PW_Password = "PWD1";
			wrapper = GetWrapper();
			AssertEquals(nameof(IGuarantee.AccessCode), "PWD1", wrapper.AccessCode);
		});
	}

	public void TestCurrency()
	{
		CombineAssertions(() =>
		{
			var wrapper = GetWrapper();
			AssertNullOrEmpty(nameof(IGuarantee.Currency), wrapper.Currency);

			guarantee.PW_RX_NKCurrency = "INR";
			wrapper = GetWrapper();
			AssertEquals(nameof(IGuarantee.Currency), "INR", wrapper.Currency);
		});
	}

	public void TestAmountToBeCovered()
	{
		CombineAssertions(() =>
		{
			var wrapper = GetWrapper();
			AssertNull(nameof(IGuarantee.AmountToBeCovered), wrapper.AmountToBeCovered);

			wrapper = GetWrapper();
			AssertNull($"When {nameof(guarantee.PW_BondAmount)} is empty and {nameof(guarantee.PW_RX_NKCurrency)} is Empty.", wrapper.AmountToBeCovered);

			guarantee.PW_RX_NKCurrency = "EUR";
			wrapper = GetWrapper();
			AssertEquals($"When {nameof(guarantee.PW_BondAmount)} is empty and {nameof(guarantee.PW_RX_NKCurrency)} has value.", 0m, wrapper.AmountToBeCovered);

			guarantee.PW_BondAmount = 12.4m;
			wrapper = GetWrapper();
			AssertEquals($"When {nameof(guarantee.PW_BondAmount)} has value and {nameof(guarantee.PW_RX_NKCurrency)} has value.", 12.4m, wrapper.AmountToBeCovered);
		});
	}

	public void TestCustomsOfficeOfGuarantee()
	{
		CombineAssertions(() =>
		{
			var wrapper = GetWrapper();
			AssertNullOrEmpty(nameof(IGuarantee.CustomsOfficeOfGuarantee), wrapper.CustomsOfficeOfGuarantee);

			guarantee.PW_BondFiledPort = "IT212122";
			wrapper = GetWrapper();
			AssertEquals(nameof(IGuarantee.CustomsOfficeOfGuarantee), "IT212122", wrapper.CustomsOfficeOfGuarantee);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.NewDepartureNctsHeaderPhase5();
		guarantee = header.MovementHeader.Guarantees.AddNew();
	}

	IGuarantee GetWrapper() => new GuaranteeWrapperWithDetails(guarantee);

	NctsGuarantee guarantee;
}
