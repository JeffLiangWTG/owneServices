using CargoWise.Customs.BR.MessageContracts.Mercante.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Manifest.Business
{
	public class FreightWrapper : IFreight
	{
		public FreightWrapper(AsycudaTax tax)
		{
			this.tax = tax;
		}
		readonly AsycudaTax tax;

		string IFreight.ComponentsCode => tax.AET_ChargeType;

		string IFreight.AmountCurrency => ZZRefCusMapCombined.MapCW1CodeToCustomsCode(tax.Factory, Core.Constants.CountryCodes.Brazil, RefCusMapTypeList.Codes.Currency, tax.AET_RX_NKCurrency, ZDate.Today);

		decimal IFreight.Amount => tax.AET_ChargeAmount;

		string IFreight.PaymentMode => tax.AET_MethodOfPayment == Core.Constants.PaymentType.Prepaid ? MercanteConstants.Prepaid : MercanteConstants.Collect;
	}
}
