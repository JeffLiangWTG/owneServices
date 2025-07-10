using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815TransportDetails
{
	const int MaxIterationNumber = 99;
	public IE815TransportDetails(ITransportDetails transportDetails, int numberItaration)
	{
		this.transportDetails = Argument.NotNull(transportDetails, "transportDetails");
		var numberItarationName = nameof(numberItaration);
		NumberIteration = Argument.GreaterThanZero(numberItaration, numberItarationName);
		if (numberItaration > MaxIterationNumber)
		{
			throw new ArgumentOutOfRangeException(numberItarationName, $"{numberItarationName} must be less than {numberItarationName}");
		}
	}
	readonly ITransportDetails transportDetails;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, true)]
	[MessageFieldRules("R", "C053")]
	public ZString ItarationType => "I";

	[MessageLayout(Order = 1)]
	[MessageFieldIntegerRepresentation(3, true)]
	[MessageFieldRules("R")]
	public ZInt NumberIteration { get; }

	[MessageLayout(Order = 2)]
	[MessageFieldIntegerRepresentation(2, true)]
	[MessageFieldRules("R")]
	public ZInt TransportUnitCode => transportDetails.TransportUnitCode;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldRules("R")]
	public ZString IdentityOfTransportUnits => transportDetails.IdentityOfTransportUnits;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldRules("C", "D003")]
	public ZString CommercialSealIdentification => transportDetails.CommercialSealIdentification;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 350, false)]
	[MessageFieldRules("O", "R016")]
	public ZString SealInformation => transportDetails.SealInformation;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldRules("C")]
	public ZString SealInformationLanguage => transportDetails.SealInformationLanguage;

	[MessageLayout(Order = 7)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 350, false)]
	[MessageFieldRules("O", "R017")]
	public ZString ComplementaryInformation => transportDetails.ComplementaryInformation;

	[MessageLayout(Order = 8)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldRules("C", "C012")]
	public ZString ComplementaryInformationLanguage => transportDetails.ComplementaryInformationLanguage;
}
