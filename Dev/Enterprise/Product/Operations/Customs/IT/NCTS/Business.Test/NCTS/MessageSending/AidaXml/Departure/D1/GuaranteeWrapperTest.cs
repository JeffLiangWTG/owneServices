using System;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class GuaranteeWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When guarantee is null", () => new GuaranteeWrapper(guarantee: null));
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
		guarantee.PW_BondNumber = "BOND1";
		var wrapper = GetWrapper();
		AssertNull(nameof(IGuarantee.GRN), wrapper.GRN);
	}

	public void TestAccessCode()
	{
		guarantee.PW_Password = "PWD1";
		var wrapper = GetWrapper();
		AssertNull(nameof(IGuarantee.AccessCode), wrapper.AccessCode);
	}

	public void TestCurrency()
	{
		guarantee.PW_RX_NKCurrency = "INR";
		var wrapper = GetWrapper();
		AssertNull(nameof(IGuarantee.Currency), wrapper.Currency);
	}

	public void TestAmountToBeCovered()
	{
		guarantee.PW_BondAmount = 12.4m;
		var wrapper = GetWrapper();
		AssertNull(nameof(IGuarantee.AmountToBeCovered), wrapper.AmountToBeCovered);
	}

	public void TestCustomsOfficeOfGuarantee()
	{
		guarantee.PW_BondFiledPort = "IT212122";
		var wrapper = GetWrapper();
		AssertNull(nameof(IGuarantee.CustomsOfficeOfGuarantee), wrapper.CustomsOfficeOfGuarantee);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.NewDepartureNctsHeaderPhase5();
		guarantee = header.MovementHeader.Guarantees.AddNew();
	}

	IGuarantee GetWrapper() => new GuaranteeWrapper(guarantee);

	NctsGuarantee guarantee;
}
