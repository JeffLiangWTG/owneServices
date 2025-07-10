using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETHeaderPrincipalTrader
{
	public ETHeaderPrincipalTrader(IETHeaderPrincipalTrader iETHeaderPrincipalTrader)
	{
		this.iETHeaderPrincipalTrader = Argument.NotNull(iETHeaderPrincipalTrader, "iETHeaderPrincipalTrader");
	}
	readonly IETHeaderPrincipalTrader iETHeaderPrincipalTrader;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 18, false)]
	[MessageFieldExportWithTransitRules("D", "C111", "C236")]
	[MessageFieldTransitRules("D", "C111", "C236")]
	[MessageFieldInternationalRoadTransportsRules("D", "C111", "C236")]
	public ZString EORICode => FormattableString.Invariant($"{iETHeaderPrincipalTrader.IdCountryCode}{iETHeaderPrincipalTrader.ID}");

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString Name => iETHeaderPrincipalTrader.Name;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString Address => iETHeaderPrincipalTrader.Address;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 9, false)]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString PostCode => iETHeaderPrincipalTrader.Postcode;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString City => iETHeaderPrincipalTrader.City;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString CountryCode => iETHeaderPrincipalTrader.CountryCode;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString Lng => ZString.Empty;

	[MessageLayout(Order = 7)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 17, false)]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	public ZString TraderGuaranteeTaxIdentificationNumber => iETHeaderPrincipalTrader.TraderGuaranteeTaxIdentificationNumber;

	[MessageLayout(Order = 8)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 17, false)]
	[MessageFieldInternationalRoadTransportsRules("D", "C904")]
	public ZString TIRHolderIdentification => iETHeaderPrincipalTrader.TIRHolderIdentification;

	[MessageLayout(Order = 9)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 17, false)]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	public ZString RepresentativeGuaranteeTaxIdentificationNumber => iETHeaderPrincipalTrader.RepresentativeGuaranteeTaxIdentificationNumber;

	[MessageLayout(Order = 10)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	public ZString RepresentativeName => iETHeaderPrincipalTrader.RepresentativeName;

	[MessageLayout(Order = 11)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	public ZString RepresentativeType => iETHeaderPrincipalTrader.RepresentativeType;

	[MessageLayout(Order = 12)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString RepresentativeTypeLng => ZString.Empty;
}
