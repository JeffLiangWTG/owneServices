using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETHeaderDeclarantTrader
{
	public ETHeaderDeclarantTrader(IDeclarantTrader iDeclarantTrader)
	{
		this.iDeclarantTrader = Argument.NotNull(iDeclarantTrader, "iDeclarantTrader");
	}
	readonly IDeclarantTrader iDeclarantTrader;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 1, true)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString RepresentativeType => iDeclarantTrader.RepresentativeType;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 18, false)]
	[MessageFieldExportRules("D", "CN31")]
	[MessageFieldExportWithTransitRules("D", "CN31")]
	[MessageFieldTransitRules("D", "CN31")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN31")]
	public ZString EORICode => FormattableString.Invariant($"{iDeclarantTrader.IdCountryCode}{iDeclarantTrader.ID}");

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldExportRules("D", "CN31")]
	[MessageFieldExportWithTransitRules("D", "CN31")]
	[MessageFieldTransitRules("D", "CN31")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN31")]
	public ZString Name => iDeclarantTrader.Name;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldExportRules("D", "CN31")]
	[MessageFieldExportWithTransitRules("D", "CN31")]
	[MessageFieldTransitRules("D", "CN31")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN31")]
	public ZString Address => iDeclarantTrader.Address;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 9, false)]
	[MessageFieldExportRules("D", "CN31")]
	[MessageFieldExportWithTransitRules("D", "CN31")]
	[MessageFieldTransitRules("D", "CN31")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN31")]
	public ZString Postcode => iDeclarantTrader.Postcode;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldExportRules("D", "CN31")]
	[MessageFieldExportWithTransitRules("D", "CN31")]
	[MessageFieldTransitRules("D", "CN31")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN31")]
	public ZString City => iDeclarantTrader.City;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldExportRules("D", "CN31")]
	[MessageFieldExportWithTransitRules("D", "CN31")]
	[MessageFieldTransitRules("D", "CN31")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN31")]
	public ZString CountryCode => iDeclarantTrader.CountryCode;

	[MessageLayout(Order = 7)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString Lng => ZString.Empty;
}
