using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMHeaderGuarantee
{
	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 1, false)]
	public ZString GuaranteeType => ZString.Empty;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 24, false)]
	public ZString Grn => ZString.Empty;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	public ZString OtherReference => ZString.Empty;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 4, false)]
	public ZString AccessCode => ZString.Empty;

	[MessageLayout(Order = 4)]
	[MessageFieldDecimalRepresentation(10, 2, false)]
	public ZDecimal? Amount => null;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString NotValidForEC => ZString.Empty;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString NotValidForOtherContractingParties => ZString.Empty;
}
