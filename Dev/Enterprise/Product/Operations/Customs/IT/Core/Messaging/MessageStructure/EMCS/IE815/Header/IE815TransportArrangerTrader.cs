using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815TransportArrangerTrader
{
	public IE815TransportArrangerTrader(ITransportTrader transportTrader)
	{
		this.transportTrader = Argument.NotNull(transportTrader, "transportTrader");
	}
	readonly ITransportTrader transportTrader;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, true)]
	[MessageFieldRules("C", "C023")]
	public ZString VatNumber => transportTrader.VatNumber;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 182, false)]
	[MessageFieldRules("C", "C024")]
	public ZString TraderName => transportTrader.TraderName;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 65, false)]
	[MessageFieldRules("C", "C024")]
	public ZString StreetName => transportTrader.StreetName;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 11, false)]
	[MessageFieldRules("C", "C023", "C021")]
	public ZString StreetNumber => transportTrader.StreetNumber;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 10, false)]
	[MessageFieldRules("C", "C024")]
	public ZString Postcode => transportTrader.Postcode;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 50, false)]
	[MessageFieldRules("C", "C024")]
	public ZString City => transportTrader.City;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, true)]
	[MessageFieldRules("C", "C024", "C012")]
	public ZString LanguageDescriptions => transportTrader.LanguageDescriptions;
}
