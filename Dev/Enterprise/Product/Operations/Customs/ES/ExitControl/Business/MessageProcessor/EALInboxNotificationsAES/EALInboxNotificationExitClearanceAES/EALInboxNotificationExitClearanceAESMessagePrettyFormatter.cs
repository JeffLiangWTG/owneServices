using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaLevanteSalidaV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class EALInboxNotificationExitClearanceAESMessagePrettyFormatter : AESCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public EALInboxNotificationExitClearanceAESMessagePrettyFormatter(ComunicaLevanteSalidaV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly ComunicaLevanteSalidaV1Sal response;

		string MessageTypeDescription => ResString.GetMultilingualString("2F28C11E-E76F-4DA0-9FED-E61868F71875", "CLEVSA - Exit Clearance Information");

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
				AppendCsvClearanceDataAndReleaseDateIfNotEmpty(messageDetails, correctResponseData.CsvLevanteSalida, correctResponseData.FechaLevanteSalida);
				AppendStatusData(messageDetails, correctResponseData.EstadoAes);
			}

			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected() => ZString.Empty;
	}
}
