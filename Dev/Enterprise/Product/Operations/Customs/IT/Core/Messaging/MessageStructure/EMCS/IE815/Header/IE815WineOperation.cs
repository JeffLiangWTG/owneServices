using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;
using Enterprise.Customs.IT.Messaging.MessageStructure;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815WineOperation : ISegment
{
	const int MaxIterationNumber = 99;
	public IE815WineOperation(IWineOperation wineOperation, int numberIteration)
	{
		this.wineOperation = Argument.NotNull(wineOperation, "wineOperation");
		var numberIterationName = nameof(numberIteration);
		NumberIteration = Argument.GreaterThanOrEqualToZero(numberIteration, numberIterationName);
		if (numberIteration > MaxIterationNumber)
		{
			throw new ArgumentOutOfRangeException(numberIterationName, FormattableString.Invariant($"{numberIterationName} must be less than {MaxIterationNumber}"));
		}
	}
	readonly IWineOperation wineOperation;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, true)]
	[MessageFieldRules("C", "C053")]
	public ZString ItarationType => "O";

	[MessageLayout(Order = 1)]
	[MessageFieldIntegerRepresentation(3, true)]
	[MessageFieldRules("C", "C053")]
	public ZInt NumberIteration { get; }

	[MessageLayout(Order = 2)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldRules("C", "C053", "D011")]
	public ZInt TreatmentCode => wineOperation.TreatmentCode;
}
