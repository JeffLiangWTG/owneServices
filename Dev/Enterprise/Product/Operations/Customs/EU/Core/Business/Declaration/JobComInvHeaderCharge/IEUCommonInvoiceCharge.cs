using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration;

public interface IEUCommonInvoiceCharge
{
	ZString J7_ChargeType { get; set; }

	ZDecimal J7_Amount { get; set; }

	ZString J7_RX_NKCurrency {  get; set; }

	ZDecimal J7_ExchangeRate { get; set; }

	ZDecimal J7_Percentage { get; set; }

	ZString J7_DistributeBy { get; set; }

	ZBool J7_IsIncludedInITOT { get; set; }

	ZString J7_FullOrPartialApportionment { get; set; }

	ZBool J7_IsStatisticalValueApplicable { get; set; }

	ZDecimal AmountInLocalCurrency { get; }

	ZDecimal AmountCorrection { get; }
}
