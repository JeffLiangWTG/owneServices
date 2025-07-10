using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815CompetentAuthorityDispatchOffice
{
	public IE815CompetentAuthorityDispatchOffice(IOffice office)
	{
		this.office = Argument.NotNull(office, "office");
	}
	readonly IOffice office;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 8, true)]
	[MessageFieldRules("C", "R007")]
	public ZString ReferenceNumber => office.ReferenceNumber;
}
