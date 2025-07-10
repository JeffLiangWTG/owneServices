using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public static class SadMessageFixedPartFactory
{
	public static ISadMessageFixedPart GetSadMessageFixedPart(bool isHeader, ISadMessageSendingObject sadMessageSendingObject, ZString messageCode, ZString annualProgressiveNumber, ZInt progressiveNumber)
	{
		if (sadMessageSendingObject.FallbackProcedure)
		{
			return new SadMessageFallbackProcedureFixedPart(isHeader, messageCode, sadMessageSendingObject.DeclarantTaxNumber, annualProgressiveNumber, progressiveNumber);
		}
		return new SadMessageFixedPart(isHeader, messageCode, annualProgressiveNumber, progressiveNumber);
	}
}
