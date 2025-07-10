using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815DeliveryPlaceCustomsOffice
{
	public IE815DeliveryPlaceCustomsOffice(IOffice office)
	{
		this.office = Argument.NotNull(office, "office");
	}
	readonly IOffice office;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 8, true)]
	[MessageFieldRules("C", "C016", "C034", "R007", "R074")]
	public ZString ReferenceNumber => office.ReferenceNumber;
}
