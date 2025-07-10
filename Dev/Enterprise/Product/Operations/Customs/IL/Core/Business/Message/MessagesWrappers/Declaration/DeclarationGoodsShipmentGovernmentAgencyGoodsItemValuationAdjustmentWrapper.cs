using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustmentWrapper : IDeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustment
	{
		DeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustmentWrapper(InvoiceLineCharge charge)
		{
			this.charge = charge;
		}

		internal static IDeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustment NewOrNull(InvoiceLineCharge charge) => charge == null ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustmentWrapper(charge);

		ICodeType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustment.AdditionCode => CodeTypeWrapper.NewOrNull(charge.J7_ChargeType);

		IAmountType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustment.AmountAmount => AmountTypeWrapper.NewOrNull(charge.J7_Amount, charge.J7_RX_NKCurrency);

		readonly InvoiceLineCharge charge;
	}
}
