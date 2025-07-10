using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC515C_v514.CC515CV1Sal;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class DeclarationAESMessagePrettyFormatter : AESCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public DeclarationAESMessagePrettyFormatter(Cc515Cv1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly Cc515Cv1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedDeclarationText);

			var correctResponseData = response.DatosRespuestaCorrecta;
			if (correctResponseData != null)
			{
				messageDetails.Append(blankLine);
				AppendAcceptanceDateIfNotEmpty(messageDetails, (ZDateTime)correctResponseData.FechaAdmision);
				AppendMrnDataIfNotEmpty(messageDetails, correctResponseData.Mrn);
				AppendGroupCircuit(messageDetails, correctResponseData.CircuitoAeat, correctResponseData.CircuitoAtc);
				AppendGroupCSV(messageDetails, correctResponseData.CsvLevanteExportacion, (ZDateTime?)correctResponseData.FechaLevante, correctResponseData.CsvDeclaracionElectronica);
				AppendExitTypeDataIfNotEmpty(messageDetails, correctResponseData.FlagDirectaIndirecta);
				AppendStatusData(messageDetails, correctResponseData.EstadoAes);
			}

			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedCommon(response.ControlRespuesta.TipoRespuesta, response);
	}
}
