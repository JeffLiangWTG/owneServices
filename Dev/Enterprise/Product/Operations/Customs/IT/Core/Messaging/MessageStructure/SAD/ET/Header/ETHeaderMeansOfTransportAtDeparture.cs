using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETHeaderMeansOfTransportAtDeparture
{
	public ETHeaderMeansOfTransportAtDeparture(IMeansOfTransport iMeansOfTransport)
	{
		this.iMeansOfTransport = Argument.NotNull(iMeansOfTransport, "iMeansOfTransport");
	}
	readonly IMeansOfTransport iMeansOfTransport;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 27, false)]
	[MessageFieldExportRules("D", "C5", "R831", "R838")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("D", "TR90", "90")]
	[MessageFieldInternationalRoadTransportsRules("D", "TR90", "90")]
	public ZString Identity => iMeansOfTransport.Identity;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString IdentityLng => ZString.Empty;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("D", "TR90", "95")]
	[MessageFieldTransitRules("D", "TR90", "95")]
	[MessageFieldInternationalRoadTransportsRules("D", "TR90", "95")]
	public ZString Nationality => iMeansOfTransport.Nationality;
}
