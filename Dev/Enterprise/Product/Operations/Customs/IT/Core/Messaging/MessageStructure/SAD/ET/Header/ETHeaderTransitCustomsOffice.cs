using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETHeaderTransitCustomsOffice
{
	readonly IETHeaderTransitCustomsOffice iETHeaderTransitCustomsOffice;

	public ETHeaderTransitCustomsOffice(IETHeaderTransitCustomsOffice iETHeaderTransitCustomsOffice)
	{
		this.iETHeaderTransitCustomsOffice = Argument.NotNull(iETHeaderTransitCustomsOffice, "iETHeaderTransitCustomsOffice");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 8, false)]
	[MessageFieldExportWithTransitRules("R", "R906", "R907", "R908", "R910")]
	[MessageFieldTransitRules("R", "R906", "R907", "R908", "R910")]
	public ZString ReferenceNumber => iETHeaderTransitCustomsOffice.ReferenceNumber;

	[MessageLayout(Order = 1)]
	[MessageFieldDateYYYYMMDDHHMMRepresentation]
	[MessageFieldExportWithTransitRules("D", "C598", "R660")]
	[MessageFieldTransitRules("D", "C598", "R660")]
	public ZDateTime EstimatedArrivalTime => iETHeaderTransitCustomsOffice.EstimatedArrivalTime;
}
