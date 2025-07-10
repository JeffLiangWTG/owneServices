using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class NBPreviousAdministrativeReference
{
	public NBPreviousAdministrativeReference(IPreviousAdministrativeReference previousAdministrativeReference)
	{
		this.previousAdministrativeReference = previousAdministrativeReference;
	}

	readonly IPreviousAdministrativeReference previousAdministrativeReference;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 4, false)]
	[MessageFieldRules("R")]
	public ZString Register => previousAdministrativeReference?.Register ?? ZString.Empty;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 8, false)]
	[MessageFieldRules("R")]
	public ZString Reference => previousAdministrativeReference?.ReferenceNumber ?? ZString.Empty;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldRules("R")]
	public ZString ReferenceCIN => previousAdministrativeReference?.ReferenceCIN ?? ZString.Empty;

	[MessageLayout(Order = 5)]
	[MessageFieldDateDDMMYYRepresentation]
	[MessageFieldRules("R")]
	public ZDate Date => previousAdministrativeReference?.Date ?? ZDate.Empty;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 2, false)]
	[MessageFieldRules("R")]
	public ZString Series => previousAdministrativeReference?.Series ?? ZString.Empty;

	[MessageLayout(Order = 7)]
	[MessageFieldIntegerRepresentation(3, false)]
	[MessageFieldRules("R")]
	public ZInt? ItemNumber => previousAdministrativeReference?.ItemNumber;

	[MessageLayout(Order = 8)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 8, false)]
	[MessageFieldRules("R")]
	public ZString CustomsOffice => previousAdministrativeReference?.CustomsOffice ?? ZString.Empty;
}
