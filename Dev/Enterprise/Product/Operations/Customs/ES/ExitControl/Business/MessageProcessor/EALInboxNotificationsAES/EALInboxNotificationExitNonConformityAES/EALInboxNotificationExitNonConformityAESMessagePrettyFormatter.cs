using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaDisconformeSalidaV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class EALInboxNotificationExitNonConformityAESMessagePrettyFormatter : AESCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public EALInboxNotificationExitNonConformityAESMessagePrettyFormatter(ComunicaDisconformeSalidaV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly ComunicaDisconformeSalidaV1Sal response;

		string MessageTypeDescription => ResString.GetMultilingualString("1694B7EE-C54E-48CE-943F-64BD365C0269", "CDISSA - Exit Stopped, Non-Conformity");

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(InboxCommunicationText);

			var correctResponseData = response.DatosComunicacion;
			if (correctResponseData != null)
			{
				messageDetails.Append(blankLine);
				AppendMessageType(messageDetails, MessageTypeDescription);
				AppendDateWithddMMyyyyHHmmssFormatIfNotEmpty(messageDetails, response.PreparationDateAndTime);
				AppendMrnDataIfNotEmpty(messageDetails, correctResponseData.Mrn);
				messageDetails.Append(blankLine);
				AppendDateValueWithddMMyyyFormatInNewTableIfNotEmpty(messageDetails, correctResponseData.FechaDisconformeSalida, NonConformityDateText);
				AppendDataInNewTableIfNotEmpty(messageDetails, RemarksText, correctResponseData.OtrasCosasAInformar);
				AppendStatusData(messageDetails, correctResponseData.EstadoAes);
			}

			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected() => ZString.Empty;
	}
}
