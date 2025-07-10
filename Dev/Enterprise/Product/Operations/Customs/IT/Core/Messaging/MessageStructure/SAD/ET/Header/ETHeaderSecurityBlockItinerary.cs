using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETHeaderSecurityBlockItinerary
{
	readonly IEnumerable<ZString> transitCountryCodes;

	public ETHeaderSecurityBlockItinerary(IEnumerable<ZString> transitCountryCodes)
	{
		this.transitCountryCodes = Argument.NotNull(transitCountryCodes, "transitCountryCodes");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZInt NumberOfOccurences => transitCountryCodes.Count();

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public IEnumerable<ZString> TransitCountryCodes => transitCountryCodes;
}
