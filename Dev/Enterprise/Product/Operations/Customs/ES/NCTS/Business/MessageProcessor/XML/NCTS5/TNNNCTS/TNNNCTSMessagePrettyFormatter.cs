using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CCTNNC_v515.CCTNNCV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class TNNNCTSMessagePrettyFormatter : NCTS5CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public TNNNCTSMessagePrettyFormatter(Cctnncv1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly Cctnncv1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedDeclarationText);
			messageDetails.Append(blankLine);

			var correctResponseData = response.DatosRespuestaCorrecta;
			if (correctResponseData != null)
			{
				AppendCsvElectronicDeclarationDataIfNotEmpty(messageDetails, correctResponseData.CsvDeclaracionElectronica);
				AppendStatusData(messageDetails, correctResponseData.Estado);
				AppendDocumentData(messageDetails, correctResponseData.CsvDocumentos);
			}

			return messageDetails.ToString();
		}

		void AppendDocumentData(StringBuilder messageDetails, CsvDocumentosTnn doc)
		{
			if (doc != null)
			{
				messageDetails.Append(CSVElectronicDocumentText);

				var tableCreator = GetNewTableCreator();
				tableCreator.WriteRow(ReferenceColumnText, CSVDocumentColumnText);
				tableCreator.WriteRow(doc.NumeroReferencia, doc.CsvDocumentoDigitalizado);

				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedCommon(response.ControlRespuesta.TipoRespuesta, response);
	}
}
