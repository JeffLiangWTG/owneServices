using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815DeliveryPlaceTrader
{
	public IE815DeliveryPlaceTrader(IDeliveryPlaceTrader deliveryPlaceTrader)
	{
		this.deliveryPlaceTrader = Argument.NotNull(deliveryPlaceTrader, "deliveryPlaceTrader");
	}
	readonly IDeliveryPlaceTrader deliveryPlaceTrader;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 18, true)]
	[MessageFieldRules("C", "C009", "C034", "R008")]
	public ZString TraderId => deliveryPlaceTrader.TraderId;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 182, false)]
	[MessageFieldRules("C", "C010", "C015", "C034")]
	public ZString TraderName => deliveryPlaceTrader.TraderName;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 65, false)]
	[MessageFieldRules("C", "C011", "C015", "C034")]
	public ZString StreetName => deliveryPlaceTrader.StreetName;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 11, false)]
	[MessageFieldRules("C", "C013", "C015", "C034")]
	public ZString StreetNumber => deliveryPlaceTrader.StreetNumber;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 10, false)]
	[MessageFieldRules("C", "C011", "C015", "C034")]
	public ZString Postcode => deliveryPlaceTrader.Postcode;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 50, false)]
	[MessageFieldRules("C", "C011", "C015", "C034")]
	public ZString City => deliveryPlaceTrader.City;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldRules("C", "C012", "C034")]
	public ZString LanguageDescriptions => deliveryPlaceTrader.LanguageDescriptions;
}
