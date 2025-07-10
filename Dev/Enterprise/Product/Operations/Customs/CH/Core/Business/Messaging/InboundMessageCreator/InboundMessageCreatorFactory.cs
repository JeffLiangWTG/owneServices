using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business;

public static class InboundMessageCreatorFactory
{
	public static IInboundMessageCreator GetNew(LoggingInformation logger, EDIInterchange interchange)
	{
		Argument.NotNull(interchange, nameof(interchange));
		IInboundMessageCreator creator = null;

		if (interchange.EI_InterchangeType == MessageTypeCodeList.Codes.XER)
		{
			return new UniversalEventInboundMessageCreator(logger);
		}
		else
		{
			switch (interchange.EI_ApplicationCode)
			{
				case ApplicationCodeList.Codes.CHCustomsEdec:
					creator = NewForEdec(logger, interchange);
					break;
				case ApplicationCodeList.Codes.CHCustomsPassar:
					creator = NewForPassar(logger, interchange);
					break;
				case ApplicationCodeList.Codes.CHCustomsCharteraOutput:
					creator = NewForCharteraOutput(logger, interchange);
					break;
			}
		}

		if (creator == null)
		{
			logger.LogError($"Interchange type not found: ApplicationCode={interchange.EI_ApplicationCode} InterchangeType={interchange.EI_InterchangeType}");
		}
		return creator;
	}

	static IInboundMessageCreator NewForEdec(LoggingInformation logger, EDIInterchange interchange)
	{
		switch (interchange.EI_InterchangeType)
		{
			case MessageTypeCodeList.Codes.Import:
			case MessageTypeCodeList.Codes.Export:
				return new EdecInboundMessageCreator(logger);
			case MessageTypeCodeList.Codes.EBD:
				return new EbdInboundMessageCreator(logger);
			case MessageTypeCodeList.Codes.EVV:
				return new EvvInboundMessageCreator(logger);
			case MessageTypeCodeList.Codes.ECM:
				return new EComInboundMessageCreator(logger);
			case MessageTypeCodeList.Codes.BOR:
				return new EdecBordereauInboundMessageCreator(logger);
			default:
				return null;
		}
	}

	static IInboundMessageCreator NewForPassar(LoggingInformation logger, EDIInterchange interchange)
	{
		switch (interchange.EI_InterchangeType)
		{
			case MessageTypeCodeList.Codes.TRE:
				return new TokenRefreshInboundMessageCreator(logger);
			case MessageTypeCodeList.Codes.PassarNcts:
			case MessageTypeCodeList.Codes.Export:
			case MessageTypeCodeList.Codes.MSL:
				return new UniversalEventInboundMessageCreator(logger);
			case MessageTypeCodeList.Codes.MSG:
				return new PassarGetMessageInboundMessageCreator(logger);
			default:
				return null;
		}
	}

	static IInboundMessageCreator NewForCharteraOutput(LoggingInformation logger, EDIInterchange interchange)
	{
		switch (interchange.EI_InterchangeType)
		{
			case MessageTypeCodeList.Codes.REQ:
			case MessageTypeCodeList.Codes.MSL:
				return new UniversalEventInboundMessageCreator(logger);
			case MessageTypeCodeList.Codes.MSG:
				return new CharteraOutputGetMessageInboundMessageCreator(logger);
			default:
				return null;
		}
	}
}
