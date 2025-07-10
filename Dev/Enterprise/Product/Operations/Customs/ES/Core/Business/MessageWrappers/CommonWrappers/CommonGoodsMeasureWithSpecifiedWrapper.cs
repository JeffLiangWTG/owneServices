using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class CommonGoodsMeasureWithSpecifiedWrapper : GoodsMeasureCommonWrapper, ICommonGoodsMeasureWithSpecified
{
	public CommonGoodsMeasureWithSpecifiedWrapper(ZDecimal grossWeight, ZDecimal netWeight) : base(grossWeight, netWeight)
	{
		GrossWeightSpecified = !grossWeight.IsEmpty;
		NetWeightSpecified = !netWeight.IsEmpty;
	}

	public CommonGoodsMeasureWithSpecifiedWrapper(CusEntryLine entryLine) : base(entryLine)
	{
		GrossWeightSpecified = true;
		NetWeightSpecified = true;
	}

	ZBool isTransitionPeriod => entryLine.Declaration.IsTransitionPeriodAES30;

	protected override int WeightMaxDecimals => !isTransitionPeriod && entryLine.Declaration.IsExport ? 6 : 3;

	public ZBool GrossWeightSpecified { get; }

	public ZBool NetWeightSpecified { get; }
}
