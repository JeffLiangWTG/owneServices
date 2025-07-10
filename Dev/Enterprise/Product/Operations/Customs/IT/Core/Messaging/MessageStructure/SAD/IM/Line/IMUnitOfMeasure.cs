using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMUnitOfMeasure
{
	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 3, false)]
	public ZString LiquidationUnitOfMeasure => ZString.Empty;

	[MessageLayout(Order = 1)]
	[MessageFieldDecimalRepresentation(14, 5, false)]
	public ZDecimal? LiquidationQuantity => null;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 5, false)]
	public ZString LiquidationQualifier => ZString.Empty;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 5, false)]
	public ZString AdditionalLiquidationQualifier => ZString.Empty;

	[MessageLayout(Order = 4)]
	[MessageFieldDecimalRepresentation(14, 5, false)]
	public ZDecimal? AdditionalLiquidationQuantity => null;
}
