using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Declaration;

public class ITCustomsValuationCalculator : EuCustomsValuationCalculator
{
	public ITCustomsValuationCalculator(IChargeApportionee chargeApportionee) : base(chargeApportionee)
	{
	}

	public ZDecimal GetExtraEUFreightChargesAmount() => GetFreightChargesAmount(includeDutiable: true, includeStatisticalValueApplicable: true, includeVATable: true);

	public ZDecimal GetEUFreightChargesAmount() => GetFreightChargesAmount(includeDutiable: false, includeStatisticalValueApplicable: true, includeVATable: true);

	public ZDecimal GetDomesticFreightChargesAmount() => GetFreightChargesAmount(includeDutiable: false, includeStatisticalValueApplicable: false, includeVATable: true);

	ZDecimal GetFreightChargesAmount(ZBool includeDutiable, ZBool includeStatisticalValueApplicable, ZBool includeVATable)
	{
		const string airTransportCostsCharge = UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge;
		const string transportCostsCharge = UCCCustomsChargeTypeList.Codes.TransportCostsCharge;

		var localCurrency = ChargeApportionee.CurrencyConverter.LocalCurrency;
		return
			ChargeApportionee.Charges.GetCharge(new ChargeCodeChargeKey(airTransportCostsCharge, includeDutiable, includeVATable, includeStatisticalValueApplicable), localCurrency) +
			ChargeApportionee.ApportionedCharges.GetCharge(new ChargeCodeChargeKey(airTransportCostsCharge, includeDutiable, includeVATable, includeStatisticalValueApplicable), localCurrency) +
			ChargeApportionee.Charges.GetCharge(new ChargeCodeChargeKey(transportCostsCharge, includeDutiable, includeVATable, includeStatisticalValueApplicable), localCurrency) +
			ChargeApportionee.ApportionedCharges.GetCharge(new ChargeCodeChargeKey(transportCostsCharge, includeDutiable, includeVATable, includeStatisticalValueApplicable), localCurrency);
	}
}
