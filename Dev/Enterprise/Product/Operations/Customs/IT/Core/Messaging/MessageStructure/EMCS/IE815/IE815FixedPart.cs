using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

[MessageFixedLength]
public class IE815FixedPart
{
	public IE815FixedPart(IIE815Attributes attributes)
	{
		this.attributes = Argument.NotNull(attributes, "attributes");
	}
	readonly IIE815Attributes attributes;

	[MessageLayout(Position = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 5, true)]
	[MessageFieldRules("R")]
	public ZString MessageCode => "IE815";

	[MessageLayout(Position = 6)]
	[MessageFieldRules("R", "R049", "R065")]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 18, true)]
	public ZString IDRegistrantCode => attributes.IDRegistrantCode;

	[MessageLayout(Position = 24)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 22, true)]
	[MessageFieldRules("R", "R009")]
	public ZString LocalIdentityNumber => attributes.LocalIdentityNumber;

	[MessageLayout(Position = 46)]
	[MessageFieldDateYYYYMMDDRepresentation()]
	[MessageFieldRules("R")]
	public ZDate DateOfTrasmission => attributes.DateOfTrasmission;
}
