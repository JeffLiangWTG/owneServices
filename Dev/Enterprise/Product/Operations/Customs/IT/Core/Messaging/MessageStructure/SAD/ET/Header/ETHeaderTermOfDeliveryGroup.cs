using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETHeaderTermOfDeliveryGroup
{
	public ETHeaderTermOfDeliveryGroup(ITermOfDeliveryGroup iTermOfDeliveryGroup)
	{
		this.iTermOfDeliveryGroup = Argument.NotNull(iTermOfDeliveryGroup, "iTermOfDeliveryGroup");
	}
	readonly ITermOfDeliveryGroup iTermOfDeliveryGroup;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 3, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	public ZString IncotermCode => iTermOfDeliveryGroup.IncotermCode;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 1, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	public ZString ComplementaryCode => iTermOfDeliveryGroup.ComplementaryCode;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	public ZString ComplementOfInfo => iTermOfDeliveryGroup.ComplementOfInfo;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString ComplementOfInfoLng => iTermOfDeliveryGroup.ComplementOfInfoLng;
}
