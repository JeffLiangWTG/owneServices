using System;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaInvalidacionV1Sal;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class InboxNotificationInvalidationAESMessagePrettyFormatter : AESCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public InboxNotificationInvalidationAESMessagePrettyFormatter(ComunicaInvalidacionV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly ComunicaInvalidacionV1Sal response;

		string MessageTypeDescription => ResString.GetMultilingualString("63B1E5CE-A04C-44FA-B3F4-B820A14A06C4", "CINVAL - Invalidated Declaration");

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(InboxCommunicationText);

			var correctResponseData = response.DatosComunicacion;
			if (correctResponseData != null)
			{
				messageDetails.Append(blankLine);
				AppendMessageType(messageDetails, MessageTypeDescription);
				AppendMrnDataIfNotEmpty(messageDetails, correctResponseData.Mrn);
				messageDetails.Append(blankLine);
				AppendInvalidationByCustomsIfTrue(messageDetails, correctResponseData.InvalidacionIniciadaPorLaAduana);
				AppendInvalidationDate(messageDetails, correctResponseData.FechaInvalidacion);
				AppendDateWithddMMyyyyHHmmssFormatIfNotEmpty(messageDetails, correctResponseData.FechaHoraSolicitudInvalidacion ?? ZDateTime.Empty, InvalidationRequestedDateText);
				AppendDataInNewTableIfNotEmpty(messageDetails, InvalidationReasonText, correctResponseData.JustificacionDeLaInvalidacion);
				AppendStatusData(messageDetails, correctResponseData.EstadoAes);
			}

			return messageDetails.ToString();
		}

		void AppendInvalidationDate(StringBuilder messageDetails, DateTime invalidationDate)
		{
			var tableCreator = GetNewNonVisibleTableCreator();
			AppendDateWithddMMyyyyFormatIfNotEmpty((ZDateTime)invalidationDate, tableCreator, InvalidationDateText);
			messageDetails.Append(tableCreator.ToHtml());
		}

		void AppendInvalidationByCustomsIfTrue(StringBuilder messageDetails, Flag invalidatedByCustomsFlag)
		{
			if (invalidatedByCustomsFlag == Flag.Item1)
			{
				var tableCreator = GetNewNonVisibleTableCreator();
				tableCreator.WriteRow(InvalidationByCustomsText);
				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		public ZString CreateMessageDetailsRejected() => ZString.Empty;

		protected string InvalidationByCustomsText => ResString.GetMultilingualString("C83A9AA6-7FF1-489C-BB23-72EDEEA1ED2C", "Invalidation by Customs");
	}
}
