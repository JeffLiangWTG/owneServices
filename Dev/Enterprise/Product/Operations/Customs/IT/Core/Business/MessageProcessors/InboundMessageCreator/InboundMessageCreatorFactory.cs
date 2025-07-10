using CargoWise.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

public static class InboundMessageCreatorFactory
{
	public static IInboundMessageCreator GetNew(EDIInterchange interchange)
	{
		Argument.NotNull(interchange, nameof(interchange));

		if (interchange.EI_InterchangeType == MessageProcessorConstants.InterchangeTypes.ExitVerification)
		{
			return new ExitVerificationInboundMessageCreator();
		}

		if (interchange.EI_ApplicationCode == Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ITCustomsXTrade)
		{
			return new XTradeInboundMessageCreator();
		}

		return new InboundMessageCreator();
	}
}
