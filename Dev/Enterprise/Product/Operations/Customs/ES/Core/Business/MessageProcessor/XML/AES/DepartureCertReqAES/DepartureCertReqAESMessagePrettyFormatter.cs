using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CCCSEC_v514.CCCSECV1Sal;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class DepartureCertReqAESMessagePrettyFormatter : AESCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public DepartureCertReqAESMessagePrettyFormatter(Cccsecv1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly Cccsecv1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();
			messageDetails.Append(AcceptedDeclarationText);

			var correctResponseData = response.DatosRespuestaCorrecta;
			if (correctResponseData != null)
			{
				messageDetails.Append(blankLine);
				AppendMrnDataIfNotEmpty(messageDetails, correctResponseData.Mrn);
				AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, CSVExitCertificateText, correctResponseData.CsvCertificadoSalida);
				messageDetails.Append(blankLine);
				AppendCsvElectronicDeclarationDataIfNotEmpty(messageDetails, correctResponseData.CsvDeclaracionElectronica);
				AppendStatusData(messageDetails, correctResponseData.EstadoAes);
			}

			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedCommon(response.ControlRespuesta.TipoRespuesta, response);
	}
}
