using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETLineContainer
{
	public ETLineContainer(ZString container)
	{
		Container = Argument.NotNullOrEmpty(container, "container");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 17, false)]
	[MessageFieldExportRules("D", "C55")]
	[MessageFieldExportWithTransitRules("D", "C55")]
	[MessageFieldTransitRules("D", "C55")]
	[MessageFieldInternationalRoadTransportsRules("D", "C55")]
	public ZString Container { get; }

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 3, false)]
	public ZString TotalPartialUnloadContainerIndicator => ZString.Empty;
}
