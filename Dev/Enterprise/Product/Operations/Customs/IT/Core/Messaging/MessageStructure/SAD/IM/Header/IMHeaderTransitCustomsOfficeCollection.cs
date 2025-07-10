using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMHeaderTransitCustomsOfficeCollection
{
	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	public ZInt? NumberOfOccurences => null;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 8, false)]
	public IEnumerable<ZString> TransitCustomsOffices => new ZString[] { ZString.Empty };
}
