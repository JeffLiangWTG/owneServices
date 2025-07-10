using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC507C_v514.CC507CV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class EALAESMessagePrettyFormatter : AESCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public EALAESMessagePrettyFormatter(Cc507Cv1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly Cc507Cv1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedDeclarationText);

			var correctResponseData = response.DatosRespuestaCorrecta;
			if (correctResponseData != null)
			{
				AppendMrnDataIfNotEmpty(messageDetails, correctResponseData.Mrn);
				AppendDataInNewTableIfNotEmpty(messageDetails, ArrivalDateText, ((ZDateTime)correctResponseData.FechaLlegada).ToCustomsFormatDateStringddMMyyyyWithDash());
				AppendResponseCode(messageDetails, response.ControlRespuesta.CodigoRespuesta);
				AppendGroupCircuit(messageDetails, correctResponseData.CircuitoLlegada, ZString.Empty);
				AppendGroupCSV(messageDetails, correctResponseData.CsvLevanteSalida, (ZDateTime?)correctResponseData.FechaLevanteSalida, correctResponseData.CsvDeclaracionElectronica);
				AppendStatusData(messageDetails, correctResponseData.EstadoAes);
			}

			return messageDetails.ToString();
		}

		void AppendResponseCode(StringBuilder messageDetails, ZString responseCode)
		{
			AppendDataInNewTableIfNotEmpty(messageDetails, ResponseCodeText, GetResponseCodeDescription(responseCode));
			if (responseCode == EALResponseCodeList.Codes.RequestingDataToExportCustomsOffice)
			{
				var tableCreator = GetNewNonVisibleTableCreator();
				tableCreator.WriteRow(ResponseCodeSExtraText);
				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		ZString GetResponseCodeDescription(string responseCode)
		{
			var description = responseCode switch
			{
				EALResponseCodeList.Codes.Clearance => EALResponseCodeList.Descriptions.Clearance,
				EALResponseCodeList.Codes.GoodsUnderCustomsControl => EALResponseCodeList.Descriptions.GoodsUnderCustomsControl,
				EALResponseCodeList.Codes.RequestingDataToExportCustomsOffice => EALResponseCodeList.Descriptions.RequestingDataToExportCustomsOffice,
				_ => ZString.Empty
			};
			return responseCode + " - " + description;
		}

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedCommon(response.ControlRespuesta.TipoRespuesta, response);
	}
}
