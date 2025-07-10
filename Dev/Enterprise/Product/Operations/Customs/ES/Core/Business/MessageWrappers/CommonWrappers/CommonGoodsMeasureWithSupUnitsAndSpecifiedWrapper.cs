using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class CommonGoodsMeasureWithSupUnitsAndSpecifiedWrapper : CommonGoodsMeasureWithSpecifiedWrapper, ICommonGoodsMeasureWithSupUnitsAndSpecified
{
	public CommonGoodsMeasureWithSupUnitsAndSpecifiedWrapper(CusEntryLine entryLine) : base(entryLine)
	{
	}

	public ZDecimal SupplementaryUnits => entryLine.SupplementaryQuantity;

	public ZBool SupplementaryUnitsSpecified => !entryLine.SupplementaryUQ.IsEmpty && !entryLine.SupplementaryQuantity.IsEmpty;
}
