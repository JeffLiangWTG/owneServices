using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815TransportMode
{
	public IE815TransportMode(ITransportMode transportMode)
	{
		this.transportMode = Argument.NotNull(transportMode, "transportMode");
	}
	readonly ITransportMode transportMode;

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldRules("R", "R071", "R073")]
	public ZInt TransportModeCode => transportMode.TransportModeCode;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 350, false)]
	[MessageFieldRules("C", "C067")]
	public ZString ComplementaryInformation => transportMode.ComplementaryInformation;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, true)]
	[MessageFieldRules("C", "C012")]
	public ZString ComplementaryInformationLanguage => transportMode.ComplementaryInformationLanguage;
}
