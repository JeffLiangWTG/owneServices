using System.Collections.ObjectModel;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CCDOCC_v514.CCDOCCV1Sal;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class AnnexAESMessagePrettyFormatter : AESCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public AnnexAESMessagePrettyFormatter(Ccdoccv1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly Ccdoccv1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedDeclarationText);

			var correctResponseData = response.DatosRespuestaCorrecta;
			if (correctResponseData != null)
			{
				messageDetails.Append(blankLine);
				AppendCsvElectronicDeclarationDataIfNotEmpty(messageDetails, correctResponseData.CsvDeclaracionElectronica);
				AppendStatusData(messageDetails, correctResponseData.EstadoAes);
				messageDetails.Append(blankLine);
				AppendDocumentsData(messageDetails, correctResponseData.CsvDocumentos);
			}

			return messageDetails.ToString();
		}

		void AppendDocumentsData(StringBuilder messageDetails, Collection<CsvDocumentosDoc> documentCollection)
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
