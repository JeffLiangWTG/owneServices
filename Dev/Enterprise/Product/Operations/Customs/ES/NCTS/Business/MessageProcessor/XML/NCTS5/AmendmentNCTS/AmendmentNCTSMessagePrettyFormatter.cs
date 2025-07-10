using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC013C_v515.CC013CV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class AmendmentNCTSMessagePrettyFormatter : NCTS5CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public AmendmentNCTSMessagePrettyFormatter(Cc013Cv1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly Cc013Cv1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedAmendmentText);

			var correctResponseData = response.DatosRespuestaCorrecta;
			if (correctResponseData != null)
			{
				messageDetails.Append(blankLine);
				AppendMrnDataIfNotEmpty(messageDetails, correctResponseData.Mrn);
				messageDetails.Append(blankLine);
				AppendCsvElectronicDeclarationDataIfNotEmpty(messageDetails, correctResponseData.CsvDeclaracionElectronica);
				AppendStatusData(messageDetails, correctResponseData.Estado);
			}

			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedCommon(response.ControlRespuesta.TipoRespuesta, response);
	}
}
