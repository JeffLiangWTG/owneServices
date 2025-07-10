using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaResulSalidaV1Sal;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class InboxNotificationExitResultAESMessagePrettyFormatter : AESCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public InboxNotificationExitResultAESMessagePrettyFormatter(ComunicaResulSalidaV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly ComunicaResulSalidaV1Sal response;

		string MessageTypeDescription => ResString.GetMultilingualString("1A9BCFDE-1D77-4545-B52F-A29A9D4FCA60", "CSALID - Exit Result");

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
				AppendExitResultData(messageDetails, correctResponseData.ResultadoSalida);
				AppendDateValueWithddMMyyyFormatInNewTableIfNotEmpty(messageDetails, correctResponseData.FechaSalidaEfectiva, EffectiveExitDateText);
				AppendDateValueWithddMMyyyFormatInNewTableIfNotEmpty(messageDetails, correctResponseData.FechaParadaAduanaSalida, StopAtExitDateText);
				AppendStatusData(messageDetails, correctResponseData.EstadoAes);
			}

			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected() => ZString.Empty;

		string EffectiveExitDateText => ResString.GetMultilingualString("1F0C4FBA-C86A-468D-A28B-315EC5C7C8F0", "Effective Exit Date:");
		string StopAtExitDateText => ResString.GetMultilingualString("BAA41FA6-AA7B-4661-860D-66B488D08ED7", "Stop at Exit Date:");
	}
}
