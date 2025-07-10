namespace Enterprise.Customs.IT.Business.Declaration;

public class ChargeAmountRefresher : EU.Business.Declaration.ChargeAmountRefresher
{
	public ChargeAmountRefresher(CusEntryLineFee lineFee) : base(lineFee)
	{
	}

	protected override bool ShouldRefreshChargeAmount => true;

	protected new CusEntryLineFee LineFee => (CusEntryLineFee)base.LineFee;

	protected override bool CanHookChargeAmountCalculationEvents()
	{
		return base.CanHookChargeAmountCalculationEvents() || LineFee.IsActionExclude;
	}
}
