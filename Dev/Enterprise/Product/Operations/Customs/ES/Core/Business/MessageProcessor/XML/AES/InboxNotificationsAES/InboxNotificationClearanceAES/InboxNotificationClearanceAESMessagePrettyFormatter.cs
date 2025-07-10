using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaLevanteExporV1Sal;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class InboxNotificationClearanceAESMessagePrettyFormatter : AESCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public InboxNotificationClearanceAESMessagePrettyFormatter(ComunicaLevanteExporV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly ComunicaLevanteExporV1Sal response;

		string MessageTypeDescription => ResString.GetMultilingualString("93390A55-BF67-492A-976B-BC8876C690FB", "CLEVEX - Clearance Information");

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(InboxCommunicationText);

			var correctResponseData = response.DatosComunicacion;
			if (correctResponseData != null)
			{
				messageDetails.Append(blankLine);
				AppendMessageType(messageDetails, MessageTypeDescription);
				AppendDateWithddMMyyyyHHmmssFormatIfNotEmpty(messageDetails, (ZDateTime)response.PreparationDateAndTime);
				AppendMrnDataIfNotEmpty(messageDetails, correctResponseData.Mrn);
				AppendCsvClearanceDataAndReleaseDateIfNotEmpty(messageDetails, correctResponseData.CsvLevanteExportacion, correctResponseData.FechaLevante);
				AppendExitTypeDataIfNotEmpty(messageDetails, correctResponseData.FlagDirectaIndirecta);
				AppendStatusData(messageDetails, correctResponseData.EstadoAes);
			}

			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected() => ZString.Empty;
	}
}
