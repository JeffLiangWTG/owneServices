using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaDisconformeExporV1Sal;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class InboxNotificationNonConformityAESMessagePrettyFormatter : AESCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public InboxNotificationNonConformityAESMessagePrettyFormatter(ComunicaDisconformeExporV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly ComunicaDisconformeExporV1Sal response;

		string MessageTypeDescription => ResString.GetMultilingualString("61E5834E-A4A0-4E80-8CB6-E9FE1E3732CD", "CDISEX - Non-Conformity Communication");

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
				AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, RemarksText, correctResponseData.OtrasCosasAInformar);
				AppendStatusData(messageDetails, correctResponseData.EstadoAes);
			}

			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected() => ZString.Empty;
	}
}
