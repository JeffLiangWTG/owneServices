using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AE.Business;

class DOCERRDataProvider : IDOCERRDataProvider
{
	readonly EDIMessage ediMessage;

	public DOCERRDataProvider(EDIMessage message)
	{
		ediMessage = Argument.NotNull(message, nameof(message));
		ediMessage = message;
	}

	public ZString OutgoingAccessReference => interchangeNumber ??= ediMessage.Factory.GetOutboundInterchangeBySessionGUID(ediMessage.Interchange.EI_SessionGUID).EI_InterchangeNum;
	string interchangeNumber;
}
