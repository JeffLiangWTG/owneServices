using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.ExitControl.Business;

public class EALAESCommodityWrapper : IEALAESCommodity
{
	public EALAESCommodityWrapper(CusExitConsignmentItem consignmentItem, ZDecimal grossMass, ZDecimal netMass)
	{
		Argument.NotNull(consignmentItem, nameof(consignmentItem));

		this.grossMass = grossMass != consignmentItem.CCI_GrossMass ? grossMass : ZDecimal.Zero;
		this.netMass = netMass != consignmentItem.CCI_NetMass ? netMass : ZDecimal.Zero;
	}
	readonly ZDecimal grossMass;
	readonly ZDecimal netMass;

	public ICommonGoodsMeasureWithSpecified GoodsMeasure => goodsMeasure ?? (goodsMeasure = new CommonGoodsMeasureWithSpecifiedWrapper(grossMass, netMass));
	CommonGoodsMeasureWithSpecifiedWrapper goodsMeasure;
}
