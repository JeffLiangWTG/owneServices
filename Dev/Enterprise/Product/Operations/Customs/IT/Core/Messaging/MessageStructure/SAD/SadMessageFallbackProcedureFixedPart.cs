using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class SadMessageFallbackProcedureFixedPart : SadMessageFixedPart
{
	public SadMessageFallbackProcedureFixedPart(bool isHeader, ZString messageCode, ZString declarantTaxNumber, ZString annualProgressiveNumber, ZInt progressiveNumber)
		: base(isHeader, messageCode, annualProgressiveNumber, progressiveNumber)
	{
		DeclarantTaxNumber = declarantTaxNumber;
	}

	[MessageFieldStringRepresentation(CharType.Alphabetical, 16, true)]
	public override ZString DeclarantTaxNumber { get; }
}
