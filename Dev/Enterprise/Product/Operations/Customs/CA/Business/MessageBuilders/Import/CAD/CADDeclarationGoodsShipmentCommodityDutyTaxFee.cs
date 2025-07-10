using System.Collections.Generic;
using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADDeclarationGoodsShipmentCommodityDutyTaxFee : ICADMessageDeclarationGoodsShipmentCommodityDutyTaxFee
{
	public CADDeclarationGoodsShipmentCommodityDutyTaxFee(ZString typeCode, ZString dutyRegimeCode)
		: this(typeCode, (null, null), (ZDecimal.Zero, ZString.Empty), dutyRegimeCode: dutyRegimeCode)
	{
	}

	public CADDeclarationGoodsShipmentCommodityDutyTaxFee(ZString typeCode, IEnumerable<ICADMessageMoney> dutyTaxFeeAssessmentBasis)
		: this(typeCode, (null, null), (ZDecimal.Zero, ZString.Empty), dutyTaxFeeAssessmentBasis: dutyTaxFeeAssessmentBasis)
	{
	}

	public CADDeclarationGoodsShipmentCommodityDutyTaxFee(ZString typeCode, IEnumerable<ICADMessageMoney> dutyTaxFeeAssessmentBasis, ZString dutyRegimeCode)
		: this(typeCode, (null, null), (ZDecimal.Zero, ZString.Empty), dutyTaxFeeAssessmentBasis: dutyTaxFeeAssessmentBasis, dutyRegimeCode: dutyRegimeCode)
	{
	}

	public CADDeclarationGoodsShipmentCommodityDutyTaxFee(ZString typeCode, (ICADMessageMoney, ICADMessageMoney) payment)
		: this(typeCode, payment, (ZDecimal.Zero, ZString.Empty))
	{
	}

	public CADDeclarationGoodsShipmentCommodityDutyTaxFee(ZString typeCode, (ICADMessageMoney, ICADMessageMoney) payment, ZString dutyRegimeCode)
		: this(typeCode, payment, (ZDecimal.Zero, ZString.Empty), dutyRegimeCode: dutyRegimeCode)
	{
	}

	public CADDeclarationGoodsShipmentCommodityDutyTaxFee(ZString typeCode, (ICADMessageMoney, ICADMessageMoney) payment, ZString dutyRegimeCode, ZString requestOverrideCode)
	: this(typeCode, payment, (ZDecimal.Zero, ZString.Empty), dutyRegimeCode: dutyRegimeCode, requestOverrideCode: requestOverrideCode)
	{
	}

	public CADDeclarationGoodsShipmentCommodityDutyTaxFee(ZString typeCode, (ICADMessageMoney, ICADMessageMoney) payment, IEnumerable<ICADMessageMoney> dutyTaxFeeAssessmentBasis)
		: this(typeCode, payment, (ZDecimal.Zero, ZString.Empty), dutyTaxFeeAssessmentBasis: dutyTaxFeeAssessmentBasis)
	{
	}

	public CADDeclarationGoodsShipmentCommodityDutyTaxFee(ZString typeCode, ICADMessageMoney deductAmount)
		: this(typeCode, (null, null), (ZDecimal.Zero, ZString.Empty), deductAmount)
	{
	}

	public CADDeclarationGoodsShipmentCommodityDutyTaxFee(ZString typeCode, (ICADMessageMoney, ICADMessageMoney) payment, (ZDecimal, ZString) specificTax, ICADMessageMoney deductAmount = null, IEnumerable<ICADMessageMoney> dutyTaxFeeAssessmentBasis = null, string dutyRegimeCode = "", string requestOverrideCode = "")
	{
		this.dutyRegimeCode = dutyRegimeCode;
		this.typeCode = typeCode;
		this.dutyTaxFeeAssessmentBasis = dutyTaxFeeAssessmentBasis;
		this.payment = payment;
		this.specificTax = specificTax;
		this.deductAmount = deductAmount;
		this.requestOverrideCode = requestOverrideCode;
	}

	readonly ZString dutyRegimeCode;
	readonly ZString typeCode;
	readonly ZString requestOverrideCode;
	readonly (ICADMessageMoney, ICADMessageMoney) payment;
	readonly (ZDecimal, ZString) specificTax;
	readonly ICADMessageMoney deductAmount;
	readonly IEnumerable<ICADMessageMoney> dutyTaxFeeAssessmentBasis;

	#region ICADDeclarationGoodsShipmentCommodityDutyTaxFee

	ICADMessageMoney ICADMessageDeclarationGoodsShipmentCommodityDutyTaxFee.DeductAmount => (deductAmount?.Amount ?? ZDecimal.Zero) == ZDecimal.Zero ? null : deductAmount;

	string ICADMessageDeclarationGoodsShipmentCommodityDutyTaxFee.DutyRegimeCode => dutyRegimeCode;

	decimal ICADMessageDeclarationGoodsShipmentCommodityDutyTaxFee.SpecificTaxBaseQuantity => specificTax.Item1;

	string ICADMessageDeclarationGoodsShipmentCommodityDutyTaxFee.SpecificTaxBaseQtyUnit => specificTax.Item2;

	string ICADMessageDeclarationGoodsShipmentCommodityDutyTaxFee.TypeCode => typeCode;

	string ICADMessageDeclarationGoodsShipmentCommodityDutyTaxFee.RequestOverrideCode => requestOverrideCode;

	IEnumerable<ICADMessageMoney> ICADMessageDeclarationGoodsShipmentCommodityDutyTaxFee.DutyTaxFeeAssessmentBasis
	{
		get
		{
			if (dutyTaxFeeAssessmentBasis != null)
			{
				foreach (var dutyTaxFee in dutyTaxFeeAssessmentBasis)
				{
					yield return dutyTaxFee;
				}
			}
		}
	}

	ICADMessageDeclarationGoodsShipmentCommodityDutyTaxFeePayment ICADMessageDeclarationGoodsShipmentCommodityDutyTaxFee.Payment => new CADDeclarationGoodsShipmentCommodityDutyTaxFeePayment(payment.Item1, payment.Item2);

	#endregion
}
