using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMHeaderSealCollection
{
	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	public ZInt? NumberOfOccurences => null;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 11, false)]
	public IEnumerable<ZString> SealsIdentity => System.Array.Empty<ZString>();
}
