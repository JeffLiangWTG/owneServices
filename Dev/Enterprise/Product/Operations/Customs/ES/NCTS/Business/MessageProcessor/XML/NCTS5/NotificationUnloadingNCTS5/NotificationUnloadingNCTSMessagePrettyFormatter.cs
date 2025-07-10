using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC044C_v515.CC044CV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NotificationUnloadingNCTSMessagePrettyFormatter : NCTS5CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public NotificationUnloadingNCTSMessagePrettyFormatter(Cc044Cv1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly Cc044Cv1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedDeclarationText);
			messageDetails.Append(blankLine);

			var correctResponseData = response.DatosRespuestaCorrecta;
			if (correctResponseData != null)
			{
				AppendMrnDataIfNotEmpty(messageDetails, correctResponseData.Mrn, referenceTextMrn: MrnText);
				AppendStatusData(messageDetails, correctResponseData.Estado);
			}
			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedCommon(response.ControlRespuesta.TipoRespuesta, response);
	}
}
