using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC015C_v515.CC015CV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class DepartureNCTSMessagePrettyFormatter : NCTS5CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public DepartureNCTSMessagePrettyFormatter(Cc015Cv1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly Cc015Cv1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedDeclarationText);

			var correctResponseData = response.DatosRespuestaCorrecta;
			if (correctResponseData != null)
			{
				messageDetails.Append(blankLine);
				AppendAcceptanceDataIfNotEmpty(messageDetails, correctResponseData.Mrn, acceptanceDate: (ZDateTime)correctResponseData.FechaHoraAlta);
				AppendLimitDateOfArrivalIfNotEmpty(messageDetails, correctResponseData.FechaLimiteLlegada);
				AppendCircuitIfNotEmpty(messageDetails, GetCircuitFromText(correctResponseData.CircuitoExpedicion));
				AppendCsvClearanceDataAndReleaseDateIfNotEmpty(messageDetails, correctResponseData.CsVdeDat, (ZDateTime?)correctResponseData.FechaLevante);
				AppendStatusData(messageDetails, correctResponseData.Estado);
			}

			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedCommon(response.ControlRespuesta.TipoRespuesta, response);
	}
}
