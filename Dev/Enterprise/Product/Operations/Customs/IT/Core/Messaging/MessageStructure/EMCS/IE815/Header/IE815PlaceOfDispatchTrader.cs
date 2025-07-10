using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815PlaceOfDispatchTrader
{
	public IE815PlaceOfDispatchTrader(IPlaceOfDispatchTrader placeOfDispatchTrader)
	{
		this.placeOfDispatchTrader = Argument.NotNull(placeOfDispatchTrader, "placeOfDispatchTrader");
	}
	readonly IPlaceOfDispatchTrader placeOfDispatchTrader;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 18, true)]
	[MessageFieldRules("C", "C001", "R005")]
	public ZString ReferenceOfTaxWarehouse => placeOfDispatchTrader.ReferenceOfTaxWarehouse;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 182, false)]
	[MessageFieldRules("C", "C002")]
	public ZString TraderName => placeOfDispatchTrader.TraderName;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 65, false)]
	[MessageFieldRules("C", "C002")]
	public ZString StreetName => placeOfDispatchTrader.StreetName;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 11, false)]
	[MessageFieldRules("C", "C002")]
	public ZString StreetNumber => placeOfDispatchTrader.StreetNumber;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 10, false)]
	[MessageFieldRules("C", "C002")]
	public ZString Postcode => placeOfDispatchTrader.Postcode;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 50, false)]
	[MessageFieldRules("C", "C002")]
	public ZString City => placeOfDispatchTrader.City;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldRules("C", "C002", "C012")]
	public ZString LanguageDescriptions => placeOfDispatchTrader.LanguageDescriptions;
}
