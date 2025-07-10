using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815ConsignorTrader
{
	public IE815ConsignorTrader(IConsignorTrader consignorTrader)
	{
		this.consignorTrader = Argument.NotNull(consignorTrader, "consignorTrader");
	}
	readonly IConsignorTrader consignorTrader;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 18, true)]
	[MessageFieldRules("R", "R004")]
	public ZString TraderExciseNumber => consignorTrader.TraderExciseNumber;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 182, false)]
	[MessageFieldRules("R")]
	public ZString TraderName => consignorTrader.TraderName;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 65, false)]
	[MessageFieldRules("R")]
	public ZString StreetName => consignorTrader.StreetName;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 11, false)]
	[MessageFieldRules("O")]
	public ZString StreetNumber => consignorTrader.StreetNumber;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 10, false)]
	[MessageFieldRules("R")]
	public ZString Postcode => consignorTrader.Postcode;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 50, false)]
	[MessageFieldRules("R")]
	public ZString City => consignorTrader.City;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldRules("R")]
	public ZString LanguageDescriptions => consignorTrader.LanguageDescriptions;
}
