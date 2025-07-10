using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.GUI;

public static class CustomsMessageExporterFactory
{
	public static ICustomsMessageExporter GetMessageExporter(ZString messageSendingMode, ITEDIMessage message)
	{
		Argument.NotNullOrEmpty(messageSendingMode, nameof(messageSendingMode));
		Argument.NotNull(message, nameof(message));

		ICustomsMessageExporter messageExporter;
		switch (messageSendingMode)
		{
			case CustomsMessageSendingModeList.Codes.FallbackProcedure:
				messageExporter = new FallbackProcedureCustomsMessageExporter();
				break;
			case CustomsMessageSendingModeList.Codes.ManualProcedure:
				messageExporter = new ManualProcedureCustomsMessageExporter();
				break;
			default:
				messageExporter = new NoActionCustomsMessageExporter();
				break;
		}
		return messageExporter;
	}
}
