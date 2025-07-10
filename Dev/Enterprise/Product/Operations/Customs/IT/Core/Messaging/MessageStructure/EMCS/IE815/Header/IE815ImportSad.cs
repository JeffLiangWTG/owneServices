using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815ImportSad
{
	const int MaxIterationNumber = 9;
	public IE815ImportSad(IImportSad importSad, int numberIteration)
	{
		this.importSad = Argument.NotNull(importSad, "importSad");
		var numberIterationName = nameof(numberIteration);
		NumberIteration = Argument.GreaterThanOrEqualToZero(numberIteration, numberIterationName);
		if (numberIteration > MaxIterationNumber)
		{
			throw new ArgumentOutOfRangeException(numberIterationName, FormattableString.Invariant($"{numberIterationName} must be less than {MaxIterationNumber}"));
		}
	}
	readonly IImportSad importSad;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, true)]
	[MessageFieldRules("C", "C053")]
	public ZString ItarationType => "G";

	[MessageLayout(Order = 1)]
	[MessageFieldIntegerRepresentation(3, true)]
	[MessageFieldRules("C", "C053")]
	public ZInt NumberIteration { get; }

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 21, false)]
	[MessageFieldRules("C", "C053", "R013")]
	public ZString ImportSadNumber => importSad.ImportSadNumber;
}
