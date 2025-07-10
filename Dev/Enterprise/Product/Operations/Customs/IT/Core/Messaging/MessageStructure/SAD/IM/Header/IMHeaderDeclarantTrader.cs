using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMHeaderDeclarantTrader
{
	public IMHeaderDeclarantTrader(IDeclarantTrader declarantTrader)
	{
		this.declarantTrader = Argument.NotNull(declarantTrader, nameof(declarantTrader));
	}

	readonly IDeclarantTrader declarantTrader;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 1, true)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString RepresentativeType => declarantTrader.RepresentativeType;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, true)]
	[MessageFieldImportRules("D", "CN31")]
	[MessageFieldDepositoRules("D", "CN31")]
	public ZString IdCountryCode => declarantTrader.IdCountryCode;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 16, false)]
	[MessageFieldImportRules("D", "CN31")]
	[MessageFieldDepositoRules("D", "CN31")]
	public ZString ID => declarantTrader.ID;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldImportRules("D", "CN31")]
	[MessageFieldDepositoRules("D", "CN31")]
	public ZString Name => declarantTrader.Name;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldImportRules("D", "CN31")]
	[MessageFieldDepositoRules("D", "CN31")]
	public ZString Address => declarantTrader.Address;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 9, false)]
	[MessageFieldImportRules("D", "CN31")]
	[MessageFieldDepositoRules("D", "CN31")]
	public ZString Postcode => declarantTrader.Postcode;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldImportRules("D", "CN31")]
	[MessageFieldDepositoRules("D", "CN31")]
	public ZString City => declarantTrader.City;

	[MessageLayout(Order = 7)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, true)]
	[MessageFieldImportRules("D", "CN31")]
	[MessageFieldDepositoRules("D", "CN31")]
	public ZString CountryCode => declarantTrader.CountryCode;
}
