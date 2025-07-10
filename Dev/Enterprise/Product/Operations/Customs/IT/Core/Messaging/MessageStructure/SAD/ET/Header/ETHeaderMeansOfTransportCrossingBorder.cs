using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETHeaderMeansOfTransportCrossingBorder
{
	public ETHeaderMeansOfTransportCrossingBorder(IETHeaderMeansOfTransportCrossingBorder iETHeaderMeansOfTransportCrossingBorder)
	{
		this.iETHeaderMeansOfTransportCrossingBorder = Argument.NotNull(iETHeaderMeansOfTransportCrossingBorder, "iETHeaderMeansOfTransportCrossingBorder");
	}
	readonly IETHeaderMeansOfTransportCrossingBorder iETHeaderMeansOfTransportCrossingBorder;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 27, false)]
	[MessageFieldExportRules("D", "R838")]
	[MessageFieldExportWithTransitRules("D", "R838")]
	[MessageFieldTransitRules("D", "C11")]
	public ZString Identity => iETHeaderMeansOfTransportCrossingBorder.Identity;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString IdentityLng => ZString.Empty;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldExportRules("D", "C10")]
	[MessageFieldExportWithTransitRules("D", "C10")]
	[MessageFieldTransitRules("D", "C10", "R36")]
	public ZString Nationality => iETHeaderMeansOfTransportCrossingBorder.Nationality;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 2, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	public ZString Type => iETHeaderMeansOfTransportCrossingBorder.Type;
}
