using System.Collections.ObjectModel;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CCDOTC_v515.CCDOTCV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class AnnexNCTSMessagePrettyFormatter : NCTS5CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public AnnexNCTSMessagePrettyFormatter(Ccdotcv1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly Ccdotcv1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedDeclarationText);

			var correctResponseData = response.DatosRespuestaCorrecta;
			if (correctResponseData != null)
			{
				messageDetails.Append(blankLine);
				AppendCsvElectronicDeclarationDataIfNotEmpty(messageDetails, correctResponseData.CsvDeclaracionElectronica);
				AppendStatusData(messageDetails, correctResponseData.Estado);
				messageDetails.Append(blankLine);
				AppendDocumentsData(messageDetails, correctResponseData.CsvDocumentos);
			}

			return messageDetails.ToString();
		}

		void AppendDocumentsData(StringBuilder messageDetails, Collection<CsvDocumentosDot> documentCollection)
		{
			if (documentCollection != null)
			{
				messageDetails.Append(CSVElectronicDocumentsText);

				var tableCreator = GetNewTableCreator();
				tableCreator.WriteRow(ReferenceColumnText, CSVDocumentColumnText);

				foreach (var doc in documentCollection)
				{
					tableCreator.WriteRow(doc.NumeroReferencia, doc.CsvDocumentoDigitalizado);
				}

				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedCommon(response.ControlRespuesta.TipoRespuesta, response);
	}
}
