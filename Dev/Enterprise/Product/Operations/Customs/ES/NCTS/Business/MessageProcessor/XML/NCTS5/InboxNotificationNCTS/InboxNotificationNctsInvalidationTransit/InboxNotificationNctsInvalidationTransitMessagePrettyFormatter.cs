using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ComunicaInvaliTranV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class InboxNotificationNctsInvalidationTransitMessagePrettyFormatter : NCTS5CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public InboxNotificationNctsInvalidationTransitMessagePrettyFormatter(ComunicaInvaliTranV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly ComunicaInvaliTranV1Sal response;

		string MessageTypeDescription => ResString.GetMultilingualString("DEF37EB1-7533-4F91-BC22-FCA048BD020F", "CINVAT - Invalidation Communication");

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
				messageDetails.Append(blankLine);
				AppendInvalidStartedByCustoms(messageDetails, correctResponseData.InvalidacionIniciadaPorLaAduana);
				AppendDataInNewTableIfNotEmpty(messageDetails, InvalidationDateText, ((ZDateTime)correctResponseData.FechaInvalidacion).ToCustomsFormatDateStringddMMyyyyWithDash());
				AppendDataInNewTableIfNotEmpty(messageDetails, InvalidationReasonText, correctResponseData.JustificacionDeLaInvalidacion);
				AppendStatusData(messageDetails, correctResponseData.Estado);
			}

			return messageDetails.ToString();
		}

		protected void AppendInvalidStartedByCustoms(StringBuilder messageDetails, Flag code)
		{
			var invalidationStartedByCustomsText = code == Flag.Item1 ? InvalidationByCustomsYesText : InvalidationByCustomsNoText;
			AppendDataInNewTableIfNotEmpty(messageDetails, InvalidationByCustomsText, invalidationStartedByCustomsText);
		}

		string InvalidationByCustomsText => ResString.GetMultilingualString("2A0AA589-F474-4639-AEA8-E16169388028", "Invalidation Started by Customs:");

		public ZString CreateMessageDetailsRejected() => ZString.Empty;
	}
}
