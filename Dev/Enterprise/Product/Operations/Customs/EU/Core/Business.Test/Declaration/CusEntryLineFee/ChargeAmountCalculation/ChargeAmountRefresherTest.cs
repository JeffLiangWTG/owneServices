using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class ChargeAmountRefresherTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ChargeAmountRefresher(lineFee: null));
		}

		public void TestAttachChargeAmountCalculationEventsIfAdditionalFee()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			lineFee.CF_RateOverrideReasonCode = "ADD";
			lineFee.CF_BaseValue = 1000;
			lineFee.CF_MethodOfCalculation = "";
			lineFee.CF_Rate = 0m;
			AssertEquals("PRE-CONDITION", 0m, lineFee.CF_ChargeAmount);

			lineFee.CF_Rate = 4m;
			AssertEquals("When rate has changed, Charge Amount", 4000m, lineFee.CF_ChargeAmount);

			lineFee.CF_ChargeAmount = 0m;
			lineFee.CF_MethodOfCalculation = "%";
			AssertEquals("When method of calculation has changed, Charge Amount", 40m, lineFee.CF_ChargeAmount);

			lineFee.CF_ChargeAmount = 0m;
			lineFee.CF_BaseValue = 500;
			AssertEquals("When base value has changed, Charge Amount", 20m, lineFee.CF_ChargeAmount);
		}

		public void TestAttachChargeAmountCalculationEventsIfOverrideFee()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			lineFee.CF_RateOverrideReasonCode = "OVR";
			lineFee.CF_BaseValue = 1000;
			lineFee.CF_MethodOfCalculation = "";
			lineFee.CF_Rate = 0m;
			AssertEquals("PRE-CONDITION", 0m, lineFee.CF_ChargeAmount);

			lineFee.CF_Rate = 4m;
			AssertEquals("When rate has changed, Charge Amount", 4000m, lineFee.CF_ChargeAmount);

			lineFee.CF_ChargeAmount = 0m;
			lineFee.CF_MethodOfCalculation = "%";
			AssertEquals("When method of calculation has changed, Charge Amount", 40m, lineFee.CF_ChargeAmount);

			lineFee.CF_ChargeAmount = 0m;
			lineFee.CF_BaseValue = 500;
			AssertEquals("When base value has changed, Charge Amount", 20m, lineFee.CF_ChargeAmount);
		}

		public void TestChargeAmountIsNotRecalculatedIfNotEmpty()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			lineFee.CF_RateOverrideReasonCode = "OVR";
			lineFee.CF_BaseValue = 1000;
			lineFee.CF_MethodOfCalculation = "%";
			lineFee.CF_Rate = 4m;
			AssertEquals("PRE-CONDITION: Charge Amount", 40m, lineFee.CF_ChargeAmount);

			lineFee.CF_BaseValue = 9999m;
			AssertEquals("POST-CONDITION: Charge Amount", 40m, lineFee.CF_ChargeAmount);
		}

		public void TestDetachChargeAmountCalculationEventsIfNotAdditionalNorOverrideFee()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			lineFee.CF_RateOverrideReasonCode = "";
			lineFee.CF_BaseValue = 1000;
			lineFee.CF_MethodOfCalculation = "";
			lineFee.CF_Rate = 0m;
			lineFee.CF_ChargeAmount = 999999m;
			AssertEquals("PRE-CONDITION", 999999m, lineFee.CF_ChargeAmount);

			lineFee.CF_Rate = 4m;
			AssertEquals("When rate has changed, Charge Amount", 999999m, lineFee.CF_ChargeAmount);

			lineFee.CF_MethodOfCalculation = "%";
			AssertEquals("When method of calculation has changed, Charge Amount", 999999m, lineFee.CF_ChargeAmount);

			lineFee.CF_BaseValue = 500;
			AssertEquals("When base value has changed, Charge Amount", 999999m, lineFee.CF_ChargeAmount);
		}

		public void TestGetNewChargeAmountCalculator()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			var chargeAmountRefresher = new ChargeAmountRefresher(lineFee);

			CombineAssertions(() =>
			{
				lineFee.CF_MethodOfCalculation = "";
				AssertType<ChargeAmountCalculator>("Charge Amount Calculator Type", chargeAmountRefresher.GetNewChargeAmountCalculator());

				lineFee.CF_MethodOfCalculation = "%";
				AssertType<PercentageChargeAmountCalculator>("Charge Amount Calculator Type", chargeAmountRefresher.GetNewChargeAmountCalculator());
			});
		}

		public void TestShouldRefreshChargeAmount()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			var chargeAmountRefresherForTest = new ChargeAmountRefresherForTest(lineFee);

			lineFee.CF_ChargeAmount = 0m;
			AssertEquals("When CF_ChargeAmount is empty, ShouldRefreshChargeAmount", true, chargeAmountRefresherForTest.ShouldRefreshChargeAmountExposed);

			lineFee.CF_ChargeAmount = 1m;
			AssertEquals("When CF_ChargeAmount is not empty, ShouldRefreshChargeAmount", false, chargeAmountRefresherForTest.ShouldRefreshChargeAmountExposed);
		}

		public void TestShouldRefreshChargeAmount_CustomsChargeSource()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			var chargeAmountRefresherForTest = new ChargeAmountRefresherForTest(lineFee);

			lineFee.CF_Source = "CUS";
			lineFee.CF_ChargeAmount = 0m;
			AssertEquals("Should not Refresh Charge Amount when CF_Source is 'CUS'", false, chargeAmountRefresherForTest.ShouldRefreshChargeAmountExposed);

			lineFee.CF_ChargeAmount = 1m;
			AssertEquals("Should not refresh Charge Amount when CF_ChargeAmount is not empty", false, chargeAmountRefresherForTest.ShouldRefreshChargeAmountExposed);
		}

		public void TestUnhookEvents()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			lineFee.CF_RateOverrideReasonCode = "ADD";
			lineFee.CF_BaseValue = 1000;
			lineFee.CF_MethodOfCalculation = "";
			lineFee.CF_Rate = 1m;
			AssertEquals("PRE-CONDITION", 1000m, lineFee.CF_ChargeAmount);

			lineFee.CF_ChargeAmount = 0m;
			lineFee.ChargeAmountRefresher.UnhookEvents();
			lineFee.CF_Rate = 4m;
			AssertEquals("POST-CONDITION: Charge Amount refresh has been suspended", 0m, lineFee.CF_ChargeAmount);
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
}
