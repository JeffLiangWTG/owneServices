using System;
using CargoWise.Common;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class ChargeAmountRefresher
	{
		public ChargeAmountRefresher(CusEntryLineFee lineFee)
		{
			LineFee = Argument.NotNull(lineFee, nameof(lineFee));
		}

		protected CusEntryLineFee LineFee { get; }

		public void HookEvents()
		{
			LineFee.CF_RateOverrideReasonCodeInfo.ValueChanged += CF_RateOverrideReasonCodeInfo_ValueChanged;
			CF_RateOverrideReasonCodeInfo_ValueChanged(this, EventArgs.Empty);
		}

		public void UnhookEvents()
		{
			UnhookEventsCore();
		}

		protected virtual void UnhookEventsCore()
		{
			LineFee.CF_RateOverrideReasonCodeInfo.ValueChanged -= CF_RateOverrideReasonCodeInfo_ValueChanged;
			UnhookChargeAmountCalculationEvents();
		}

		void CF_RateOverrideReasonCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			UnhookChargeAmountCalculationEvents();

			if (CanHookChargeAmountCalculationEvents())
			{
				HookChargeAmountCalculationEvents();
			}
		}

		void HookChargeAmountCalculationEvents()
		{
			LineFee.CF_BaseValueInfo.ValueChanged += TriggerChargeAmountCalculation;
			LineFee.CF_MethodOfCalculationInfo.ValueChanged += TriggerChargeAmountCalculation;
			LineFee.CF_RateInfo.ValueChanged += TriggerChargeAmountCalculation;
		}

		void UnhookChargeAmountCalculationEvents()
		{
			LineFee.CF_BaseValueInfo.ValueChanged -= TriggerChargeAmountCalculation;
			LineFee.CF_MethodOfCalculationInfo.ValueChanged -= TriggerChargeAmountCalculation;
			LineFee.CF_RateInfo.ValueChanged -= TriggerChargeAmountCalculation;
		}

		void TriggerChargeAmountCalculation(object sender, EventArgs args)
		{
			if (ShouldRefreshChargeAmount)
			{
				var chargeAmountCalculator = GetNewChargeAmountCalculator();
				LineFee.CF_ChargeAmount = chargeAmountCalculator.Calculate();
			}
		}

		protected virtual bool ShouldRefreshChargeAmount => LineFee.CF_ChargeAmount.IsEmpty && LineFee.CF_Source != CusEntryLineFeeSourceCodeList.Codes.CUS;

		public IChargeAmountCalculator GetNewChargeAmountCalculator() => GetNewChargeAmountCalculatorCore();

		protected virtual IChargeAmountCalculator GetNewChargeAmountCalculatorCore()
		{
			var defaultCalculator = (IChargeAmountCalculator)new ChargeAmountCalculator(LineFee);
			return LineFee.CF_MethodOfCalculation != Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage
				? defaultCalculator
				: new PercentageChargeAmountCalculator(defaultCalculator);
		}

		protected virtual bool CanHookChargeAmountCalculationEvents()
		{
			return LineFee.CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Additional
				|| LineFee.CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Override;
		}
	}
}
