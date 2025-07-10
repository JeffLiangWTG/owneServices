using CargoWise.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class EntryLineCustomsValuation
{
	public EntryLineCustomsValuation(Money freightAdjustments, Money lineValue)
	{
		FreightAdjustments = Argument.NotNull(freightAdjustments, nameof(freightAdjustments));
		LineValue = Argument.NotNull(lineValue, nameof(lineValue));
	}

	public Money FreightAdjustments { get; }
	public Money LineValue { get; }
}
