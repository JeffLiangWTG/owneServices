using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMHeaderConsignee
{
	public IMHeaderConsignee(ITrader consigneeTrader)
	{
		this.consigneeTrader = Argument.NotNull(consigneeTrader, nameof(consigneeTrader));
	}

	readonly ITrader consigneeTrader;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, true)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString IdCountryCode => consigneeTrader.IdCountryCode;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 16, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString ID => consigneeTrader.ID;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString Name => consigneeTrader.Name;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString Address => consigneeTrader.Address;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 9, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString Postcode => consigneeTrader.Postcode;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString City => consigneeTrader.City;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, true)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString CountryCode => consigneeTrader.CountryCode;
}
