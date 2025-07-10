using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtGoodsItemAmountWrapper : IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtGoodsItemAmount
	{
		DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtGoodsItemAmountWrapper(string amountType, decimal amount, ZString currency)
		{
			this.amountType = amountType;
			this.amount = amount;
			this.currency = currency;
		}

		internal static IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtGoodsItemAmount NewOrNull(string amountType, ZDecimal amount, ZString currency)
			=> amountType.IsNullOrEmpty() || amount.IsEmpty || currency.IsEmpty
			? null
			: new DeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtGoodsItemAmountWrapper(amountType, amount, currency);

		ICodeType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtGoodsItemAmount.AmountType => CodeTypeWrapper.NewOrNull(amountType);

		IAmountType IDeclarationGoodsShipmentGovernmentAgencyGoodsItemDmExtGoodsItemAmount.CustomsValueAmount => AmountTypeWrapper.NewOrNull(amount.Round(CusEntryLine.Schema.CL_InvoiceAmount_DecimalPlaces), currency);

		readonly string amountType;
		readonly ZDecimal amount;
		readonly ZString currency;
	}
}
