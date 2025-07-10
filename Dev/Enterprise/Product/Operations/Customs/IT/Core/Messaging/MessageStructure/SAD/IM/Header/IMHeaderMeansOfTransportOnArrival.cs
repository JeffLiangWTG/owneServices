using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMHeaderMeansOfTransportOnArrival
{
	public IMHeaderMeansOfTransportOnArrival(IMeansOfTransport meansOfTransport)
	{
		this.meansOfTransport = Argument.NotNull(meansOfTransport, nameof(meansOfTransport));
	}

	readonly IMeansOfTransport meansOfTransport;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldImportRules("O")]
	public ZString Nationality => meansOfTransport.Nationality;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 27, false)]
	[MessageFieldImportRules("O")]
	public ZString Identity => meansOfTransport.Identity;
}
