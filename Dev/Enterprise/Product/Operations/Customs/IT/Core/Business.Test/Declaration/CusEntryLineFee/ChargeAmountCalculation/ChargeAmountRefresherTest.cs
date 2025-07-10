using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ChargeAmountRefresherTest : TestCaseWithFactory
{
	public void TestChargeAmountIsRecalculatedIfNotEmpty()
	{
		var lineFee = Factory.New<CusEntryLineFee>();
		lineFee.CF_RateOverrideReasonCode = "OVR";
		lineFee.CF_BaseValue = 1000;
		lineFee.CF_MethodOfCalculation = "%";
		lineFee.CF_Rate = 4m;
		AssertEquals("PRE-CONDITION: Charge Amount", 40m, lineFee.CF_ChargeAmount);

		lineFee.CF_BaseValue = 9999m;
		AssertEquals("POST-CONDITION: Charge Amount", 399.96m, lineFee.CF_ChargeAmount);
	}

	public void TestShouldRefreshChargeAmount()
	{
		var lineFee = Factory.New<CusEntryLineFee>();
		var chargeAmountRefresherForTest = new ChargeAmountRefresherForTest(lineFee);
		AssertEquals("ShouldRefreshChargeAmount", true, chargeAmountRefresherForTest.ShouldRefreshChargeAmountExposed);
	}

	public void TestCanHookChargeAmountCalculationEvents()
	{
		var lineFee = Factory.New<CusEntryLineFee>();
		var chargeAmountRefresherForTest = new ChargeAmountRefresherForTest(lineFee);

		lineFee.CF_RateOverrideReasonCode = "";
		AssertEquals("When CF_RateOverrideReasonCode=EMPTY", false, chargeAmountRefresherForTest.CanHookChargeAmountCalculationEvents_Exposed());

		lineFee.CF_RateOverrideReasonCode = "ADD";
		AssertEquals("When CF_RateOverrideReasonCode=ADD", true, chargeAmountRefresherForTest.CanHookChargeAmountCalculationEvents_Exposed());

		lineFee.CF_RateOverrideReasonCode = "OVR";
		AssertEquals("When CF_RateOverrideReasonCode=OVR", true, chargeAmountRefresherForTest.CanHookChargeAmountCalculationEvents_Exposed());

		lineFee.CF_RateOverrideReasonCode = "EXC";
		AssertEquals("When CF_RateOverrideReasonCode=EXC", true, chargeAmountRefresherForTest.CanHookChargeAmountCalculationEvents_Exposed());

		lineFee.CF_RateOverrideReasonCode = "XYZ";
		AssertEquals("When CF_RateOverrideReasonCode=XYZ", false, chargeAmountRefresherForTest.CanHookChargeAmountCalculationEvents_Exposed());
	}

	class ChargeAmountRefresherForTest : ChargeAmountRefresher
	{
		public ChargeAmountRefresherForTest(CusEntryLineFee lineFee) : base(lineFee)
		{
		}

		public bool ShouldRefreshChargeAmountExposed => ShouldRefreshChargeAmount;

		internal bool CanHookChargeAmountCalculationEvents_Exposed() => CanHookChargeAmountCalculationEvents();
	}
}
