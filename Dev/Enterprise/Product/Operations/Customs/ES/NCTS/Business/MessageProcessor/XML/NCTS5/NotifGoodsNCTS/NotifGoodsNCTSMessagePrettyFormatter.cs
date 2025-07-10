using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC170C_v515.CC170CV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NotifGoodsNCTSMessagePrettyFormatter : NCTS5CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public NotifGoodsNCTSMessagePrettyFormatter(Cc170Cv1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly Cc170Cv1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedDeclarationText);

			var correctResponseData = response.DatosRespuestaCorrecta;
			if (correctResponseData != null)
			{
				messageDetails.Append(blankLine);
				AppendAcceptanceDataIfNotEmpty(messageDetails, correctResponseData.Mrn, acceptanceDate: (ZDateTime)correctResponseData.FechaAdmision);
				AppendLimitDateOfArrivalIfNotEmpty(messageDetails, correctResponseData.FechaLimiteLlegada);
				AppendCircuitIfNotEmpty(messageDetails, GetCircuitFromText(correctResponseData.CircuitoExpedicion));
				AppendCsvClearanceDataAndReleaseDateIfNotEmpty(messageDetails, correctResponseData.CsVdeDat, (ZDateTime?)correctResponseData.FechaLevante);
				AppendStatusData(messageDetails, correctResponseData.Estado);
			}

			return messageDetails.ToString();
		}

		protected override ZString GetAcceptanceDateFormatted(ZDateTime acceptanceDate) => acceptanceDate.ToCustomsFormatDateStringddMMyyyyWithDash();

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedCommon(response.ControlRespuesta.TipoRespuesta, response);
	}
}
