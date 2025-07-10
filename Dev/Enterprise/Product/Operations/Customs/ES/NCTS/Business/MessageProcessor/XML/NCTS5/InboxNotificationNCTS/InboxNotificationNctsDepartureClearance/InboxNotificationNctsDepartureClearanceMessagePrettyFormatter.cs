using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ComunicaLevanteParV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class InboxNotificationNctsDepartureClearanceMessagePrettyFormatter : NCTS5CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public InboxNotificationNctsDepartureClearanceMessagePrettyFormatter(ComunicaLevanteParV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly ComunicaLevanteParV1Sal response;

		string MessageTypeDescription => ResString.GetMultilingualString("A98BCDEE-0E7A-4F1D-B2D2-182347E6BBFF", "CLETPA - Clearance Communication");

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
				AppendLimitDateOfArrivalIfNotEmpty(messageDetails, correctResponseData.FechaLimiteLlegada);
				AppendCircuitIfNotEmpty(messageDetails, GetCircuitFromText(correctResponseData.CircuitoExpedicion));
				AppendCsvClearanceDataAndReleaseDateIfNotEmpty(messageDetails, correctResponseData.CsVdeDat, correctResponseData.FechaLevante);
				AppendStatusData(messageDetails, correctResponseData.Estado);
			}

			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected() => ZString.Empty;
	}
}
