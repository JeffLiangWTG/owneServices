using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMHeaderTermOfDeliveryGroup
{
	public IMHeaderTermOfDeliveryGroup(ITermOfDeliveryGroup termsOfDelivery)
	{
		this.termsOfDelivery = Argument.NotNull(termsOfDelivery, nameof(termsOfDelivery));
	}

	readonly ITermOfDeliveryGroup termsOfDelivery;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 3, false)]
	[MessageFieldImportRules("R")]
	public ZString IncotermCode => termsOfDelivery.IncotermCode;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldImportRules("R")]
	public ZString ComplementOfInfo => termsOfDelivery.ComplementOfInfo;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 1, false)]
	[MessageFieldImportRules("R")]
	public ZString ComplementaryCode => termsOfDelivery.ComplementaryCode;
}
