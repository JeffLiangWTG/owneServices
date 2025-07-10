using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETLineConsignor
{
	public ETLineConsignor(ITrader iTrader)
	{
		this.iTrader = Argument.NotNull(iTrader, "iTrader");
	}
	readonly ITrader iTrader;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 18, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString EORICode => FormattableString.Invariant($"{iTrader.IdCountryCode}{iTrader.ID}");

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString Name => iTrader.Name;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString Address => iTrader.Address;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 9, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString Postcode => iTrader.Postcode;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString City => iTrader.City;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString CountryCode => iTrader.CountryCode;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString Lng => ZString.Empty;
}
