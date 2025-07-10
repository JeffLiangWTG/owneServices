using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public interface IApportionmentChargeToImport
	{
		ZGuid JR_JH { get; }
		ZGuid JR_AC { get; }
		ZGuid JR_GB { get; }
		ZGuid JR_GE { get; }
		ZString JR_RX_NKCostCurrency { get; }
		ZDecimal JR_OSCostExRate { get; }
		ZDecimal JR_OSCostAmt { get; }
		ZGuid JR_OH_SellAccount { get; }
		ZBool JR_IsRevenuePosted { get; }
	}
}
