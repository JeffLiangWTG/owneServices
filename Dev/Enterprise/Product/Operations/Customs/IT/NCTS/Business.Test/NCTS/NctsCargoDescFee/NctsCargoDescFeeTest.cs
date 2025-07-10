using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsCargoDescFee))]
sealed class NctsCargoDescFeeTest : EnterpriseBusinessObjectTestCase
{
	public void TestLookups_Phase4()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var fee = goodsItem.Fees.AddNew();
		AssertType<NctsCargoDescFeePhase4Lookups>("Lookups Type", fee.Lookups);
	}

	public void TestLookups_Phase5()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		var fee = goodsItem.Fees.AddNew();
		AssertType<NctsCargoDescFeePhase5Lookups>("Lookups Type", fee.Lookups);
	}

	public void TestValidation_Phase4()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var fee = goodsItem.Fees.AddNew();
		AssertType<NctsCargoDescFeePhase4Validation>("Validation Type", fee.Validation);
	}

	public void TestValidation_Phase5()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		var fee = goodsItem.Fees.AddNew();
		AssertType<EU.NCTS.Business.NctsCargoDescFeeValidation>("Validation Type", fee.Validation);
	}

	public void TestParentCargoDesc()
	{
		var nctsCargoDesc = Factory.New<NctsDepartureCargoDesc>();
		var fee = nctsCargoDesc.Fees.AddNew();
		AssertNotNull("Parent not null", fee.CargoDesc);
		AssertSame("Same object", nctsCargoDesc, fee.CargoDesc);
	}

	public void TestIFeeMembers()
	{
		var fee = Factory.New<NctsCargoDescFee>();
		fee.BFE_ChargeType = "A00";
		fee.BFE_BaseValue = 1m;
		fee.BFE_Rate = 2m;
		fee.BFE_MethodOfCalculation = "A";
		fee.BFE_ChargeAmount = 123m;
		fee.BFE_MethodOfPayment = "X";
		CombineAssertions(() =>
		{
			var iFee = (IFee)fee;
			AssertEquals(nameof(iFee.ChargeType), "A00", iFee.ChargeType);
			AssertEquals(nameof(iFee.BaseValue), 1m, iFee.BaseValue);
			AssertEquals(nameof(iFee.Rate), 2m, iFee.Rate);
			AssertEquals(nameof(iFee.MethodOfCalculation), "A", iFee.MethodOfCalculation);
			AssertEquals(nameof(iFee.Amount), 123m, iFee.Amount);
			AssertEquals(nameof(iFee.MethodOfPayment), "X", iFee.MethodOfPayment);
		});
	}

	public void TestCalculateTotalAmount()
	{
		var nctsCargoDesc = Factory.New<NctsDepartureCargoDesc>();
		var fee = nctsCargoDesc.Fees.AddNew();
		fee.BFE_RateOverrideReasonCode = "ADD";

		fee.BFE_MethodOfCalculation = "";
		fee.BFE_BaseValue = 15;
		fee.BFE_Rate = 0.5;
		AssertEquals($"When Method Of Calculation is empty, {nameof(NctsCargoDescFee.BFE_ChargeAmount)}", 0m, fee.BFE_ChargeAmount);

		fee.BFE_MethodOfCalculation = "X";
		AssertEquals($"When Method Of Calculation is X, {nameof(NctsCargoDescFee.BFE_ChargeAmount)}", 7.5m, fee.BFE_ChargeAmount);

		fee.BFE_ChargeAmount = 0;
		fee.BFE_BaseValue = 30;
		AssertEquals("When BaseValue changing and MoC is X, CalculateTotalAmount should be triggered", 15m, fee.BFE_ChargeAmount);

		fee.BFE_Rate = 0.2;
		AssertEquals("When Rate changing and MoC is X but ChargeAmount is not empty, CalculateTotalAmount should not be triggered", 15m, fee.BFE_ChargeAmount);

		fee.BFE_ChargeAmount = 0;
		fee.BFE_Rate = 0.1;
		AssertEquals("When Rate changing and MoC is X, CalculateTotalAmount should be triggered", 3m, fee.BFE_ChargeAmount);

		fee.BFE_ChargeAmount = 0;
		fee.BFE_MethodOfCalculation = "DTN";
		fee.BFE_Rate = 0.5;
		AssertEquals("When Rate changing but MoC is DTN, CalculateTotalAmount should not be triggered", 0m, fee.BFE_ChargeAmount);

		fee.BFE_ChargeAmount = 0;
		fee.BFE_RateOverrideReasonCode = "";
		fee.BFE_MethodOfCalculation = "X";
		fee.BFE_BaseValue = 100;
		AssertEquals($"When RateOverrideReasonCode is not OVR or ADD, CalculateTotalAmount should not be triggered", 0m, fee.BFE_ChargeAmount);
	}

	public void TestBFE_ChargeAmount()
	{
		var nctsCargoDesc = Factory.New<NctsDepartureCargoDesc>();
		var fee = nctsCargoDesc.Fees.AddNew();

		fee.BFE_ChargeAmount = 12.788;
		AssertEquals(nameof(NctsCargoDescFee.BFE_ChargeAmount), 12.79m, fee.BFE_ChargeAmount);

		fee.BFE_ChargeAmount = 0.003m;
		AssertEquals($"Assert {nameof(NctsCargoDescFee.BFE_ChargeAmount)} when value is less than 0.005 but greater than 0", 0.01m, fee.BFE_ChargeAmount);

		fee.BFE_ChargeAmount = -0.146m;
		AssertEquals($"Assert {nameof(NctsCargoDescFee.BFE_ChargeAmount)} when value is less than -0.005", -0.15m, fee.BFE_ChargeAmount);

		fee.BFE_ChargeAmount = -0.004m;
		AssertEquals($"Assert {nameof(NctsCargoDescFee.BFE_ChargeAmount)} when value is less than 0 but greater than -0.005", -0.01m, fee.BFE_ChargeAmount);
	}

	public void TestReadOnlyFields()
	{
		var nctsCargoDesc = Factory.New<NctsDepartureCargoDesc>();
		var fee = nctsCargoDesc.Fees.AddNew();

		fee.BFE_RateOverrideReasonCode = "";
		AssertProperties(expectedReadOnly: true);

		fee.BFE_RateOverrideReasonCode = "ADD";
		AssertProperties(expectedReadOnly: false);

		fee.BFE_RateOverrideReasonCode = "OVR";
		AssertProperties(expectedReadOnly: false);

		void AssertProperties(ZBool expectedReadOnly)
		{
			CombineAssertions("Assert read only", () =>
			{
				AssertEquals($"{nameof(NctsCargoDescFee.BFE_ChargeType)} Read only", expectedReadOnly, fee.BFE_ChargeTypeInfo.ReadOnly);
				AssertEquals($"{nameof(NctsCargoDescFee.BFE_RateOverrideReasonCode)} Read only", false, fee.BFE_RateOverrideReasonCodeInfo.ReadOnly);
				AssertEquals($"{nameof(NctsCargoDescFee.BFE_BaseValue)} Read only", expectedReadOnly, fee.BFE_BaseValueInfo.ReadOnly);
				AssertEquals($"{nameof(NctsCargoDescFee.BFE_MethodOfCalculation)} Read only", expectedReadOnly, fee.BFE_MethodOfCalculationInfo.ReadOnly);
				AssertEquals($"{nameof(NctsCargoDescFee.BFE_Rate)} Read only", expectedReadOnly, fee.BFE_RateInfo.ReadOnly);
				AssertEquals($"{nameof(NctsCargoDescFee.BFE_ChargeAmount)} Read only", expectedReadOnly, fee.BFE_ChargeAmountInfo.ReadOnly);
				AssertEquals($"{nameof(NctsCargoDescFee.BFE_MethodOfPayment)} Read only", false, fee.BFE_MethodOfPaymentInfo.ReadOnly);
			});
		}
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewNctsCargoDescFee(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewNctsCargoDescFee(factory);

	NctsCargoDescFee GetNewNctsCargoDescFee(BusinessObjectFactory factory)
	{
		var header = factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
		var fee = goodsItem.Fees.AddNew();
		fee.BFE_ChargeType = NctsCargoDescFeeChargeTypeList.Codes._149;
		fee.BFE_MethodOfPayment = "A";
		fee.BFE_MethodOfCalculation = "A";
		return fee;
	}
}
