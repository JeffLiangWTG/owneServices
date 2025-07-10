using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815ConsigneeTrader
{
	public IE815ConsigneeTrader(IConsigneeTrader consigneeTrader)
	{
		this.consigneeTrader = Argument.NotNull(consigneeTrader, "consigneeTrader");
	}
	readonly IConsigneeTrader consigneeTrader;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 18, true)]
	[MessageFieldRules("C", "C004", "D019", "R006", "R039", "R054", "R079")]
	public ZString TraderId => consigneeTrader.TraderId;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 182, false)]
	[MessageFieldRules("C", "C005", "C034")]
	public ZString TraderName => consigneeTrader.TraderName;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 65, false)]
	[MessageFieldRules("C", "C005", "C034")]
	public ZString StreetName => consigneeTrader.StreetName;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 11, false)]
	[MessageFieldRules("C", "C006", "C034")]
	public ZString StreetNumber => consigneeTrader.StreetNumber;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 10, false)]
	[MessageFieldRules("C", "C005", "C034")]
	public ZString Postcode => consigneeTrader.Postcode;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 50, false)]
	[MessageFieldRules("C", "C005", "C034")]
	public ZString City => consigneeTrader.City;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldRules("C", "C005", "C034")]
	public ZString LanguageDescriptions => consigneeTrader.LanguageDescriptions;

	[MessageLayout(Order = 7)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 17, false)]
	[MessageFieldRules("C", "C075", "R084")]
	public ZString EoriNumber => consigneeTrader.EoriNumber;
}
