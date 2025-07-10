using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETHeaderWarehouseIdentification
{
	readonly IWarehouseIdentification iWarehouseIdentification;

	public ETHeaderWarehouseIdentification(IWarehouseIdentification iWarehouseIdentification)
	{
		this.iWarehouseIdentification = Argument.NotNull(iWarehouseIdentification, "iWarehouseIdentification");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString WarehouseType => iWarehouseIdentification.Type;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 14, false)]
	[MessageFieldExportRules("D", "CN95")]
	[MessageFieldExportWithTransitRules("D", "CN95")]
	[MessageFieldTransitRules("D", "CN95")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN95")]
	public ZString WarehouseIdentification => FormattableString.Invariant($"{iWarehouseIdentification.Identification}{iWarehouseIdentification.CinIdentification}");

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString AuthorizingCountry => iWarehouseIdentification.AuthorizingCountry;
}
