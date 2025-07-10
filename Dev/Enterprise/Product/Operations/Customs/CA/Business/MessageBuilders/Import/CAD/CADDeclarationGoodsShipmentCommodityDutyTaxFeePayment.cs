using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADDeclarationGoodsShipmentCommodityDutyTaxFeePayment : ICADMessageDeclarationGoodsShipmentCommodityDutyTaxFeePayment
{
	public CADDeclarationGoodsShipmentCommodityDutyTaxFeePayment(ICADMessageMoney taxAssessed, ICADMessageMoney payment)
	{
		this.taxAssessed = taxAssessed;
		this.payment = payment;
	}
	readonly ICADMessageMoney taxAssessed;
	readonly ICADMessageMoney payment;

	#region ICADMessageDeclarationGoodsShipmentCommodityDutyTaxFeePayment

	ICADMessageMoney ICADMessageDeclarationGoodsShipmentCommodityDutyTaxFeePayment.TaxAssessedAmount => taxAssessed?.Amount == ZDecimal.Zero ? null : taxAssessed;

	ICADMessageMoney ICADMessageDeclarationGoodsShipmentCommodityDutyTaxFeePayment.PaymentAmount => payment?.Amount == ZDecimal.Zero ? null : payment;

	#endregion
}
