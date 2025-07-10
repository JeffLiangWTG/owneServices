using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815Package
{
	const int MaxIterationNumber = 99;
	public IE815Package(IPackage package, int numberIteration)
	{
		this.package = Argument.NotNull(package, "package");
		var numberIterationName = nameof(numberIteration);
		NumberIteration = Argument.GreaterThanZero(numberIteration, numberIterationName);
		if (numberIteration > MaxIterationNumber)
		{
			throw new ArgumentOutOfRangeException(numberIterationName, FormattableString.Invariant($"{numberIterationName} must be less than {MaxIterationNumber}"));
		}
	}
	readonly IPackage package;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, true)]
	[MessageFieldRules("R")]
	public ZString ItarationType => "M";

	[MessageLayout(Order = 1)]
	[MessageFieldIntegerRepresentation(3, true)]
	[MessageFieldRules("R")]
	public ZInt NumberIteration { get; }

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldRules("R")]
	public ZString KindOfPackages => package.KindOfPackages;

	[MessageLayout(Order = 3)]
	[MessageFieldIntegerRepresentation(15, false)]
	[MessageFieldRules("C", "C028")]
	public ZInt NumberOfPackages => package.NumberOfPackages;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldRules("C", "D009")]
	public ZString CommercialSealIdentification => package.CommercialSealIdentification;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 350, false)]
	[MessageFieldRules("O", "R016")]
	public ZString SealInformation => package.SealInformation;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, true)]
	[MessageFieldRules("O", "R016")]
	public ZString SealInformationLanguage => package.SealInformationLanguage;
}
