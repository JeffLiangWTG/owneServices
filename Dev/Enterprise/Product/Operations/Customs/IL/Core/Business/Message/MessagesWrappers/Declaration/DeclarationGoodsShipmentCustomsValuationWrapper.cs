using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentCustomsValuationWrapper : IDeclarationGoodsShipmentCustomsValuation
	{
		DeclarationGoodsShipmentCustomsValuationWrapper(InvoiceCharge invoiceCharge)
		{
			this.invoiceCharge = invoiceCharge;
		}

		internal static DeclarationGoodsShipmentCustomsValuationWrapper NewOrNull(InvoiceCharge invoiceCharge)
			=> invoiceCharge != null ? new DeclarationGoodsShipmentCustomsValuationWrapper(invoiceCharge) : null;

		#region IDeclarationGoodsShipmentCustomsValuation

		ICodeType IDeclarationGoodsShipmentCustomsValuation.ChargesTypeCode => CodeTypeWrapper.NewOrNull(invoiceCharge.J7_ChargeType);

		IAmountType IDeclarationGoodsShipmentCustomsValuation.ExitToEntryChargeAmount
			=> invoiceCharge.J7_ChargeType.ToString() switch
			{
				CustomsChargeTypeList.Codes.OverseasInsurance => AmountTypeWrapper.NewOrNull(invoiceCharge.J7_Amount, invoiceCharge.J7_RX_NKCurrency),
				_ => null,
			};

		IAmountType IDeclarationGoodsShipmentCustomsValuation.FreightChargeAmount
			=> invoiceCharge.J7_ChargeType.ToString() switch
			{
				CustomsChargeTypeList.Codes.OverseasFreight => AmountTypeWrapper.NewOrNull(invoiceCharge.J7_Amount, invoiceCharge.J7_RX_NKCurrency),
				_ => null,
			};

		IAmountType IDeclarationGoodsShipmentCustomsValuation.OtherChargeDeductionAmount
			=> invoiceCharge.J7_ChargeType.ToString() switch
			{
				CustomsChargeTypeList.Codes.OverseasInsurance => null,
				CustomsChargeTypeList.Codes.OverseasFreight => null,
				_ => AmountTypeWrapper.NewOrNull(invoiceCharge.J7_Amount, invoiceCharge.J7_RX_NKCurrency),
			};

		#endregion IDeclarationGoodsShipmentCustomsValuation

		readonly InvoiceCharge invoiceCharge;
	}
}
