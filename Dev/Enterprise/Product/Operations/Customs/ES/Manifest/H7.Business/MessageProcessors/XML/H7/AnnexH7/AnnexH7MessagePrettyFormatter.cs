using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.CommonAnnex.EnvioDeDocumentosV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class AnnexH7MessagePrettyFormatter : H7CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public AnnexH7MessagePrettyFormatter(EnvioDeDocumentosV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}

		protected readonly EnvioDeDocumentosV1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();
			AppendResponseDetails(messageDetails);

			var csvDocument = response.InformacionAdicionalRespuesta.FirstOrDefault(x => x.NombreEtiqueta == CsvDocumentTag);
			var requestDispatch = response.InformacionAdicionalRespuesta.FirstOrDefault(x => x.NombreEtiqueta == RequestDispatchTag);

			if (!string.IsNullOrEmpty(csvDocument?.Valor) || !string.IsNullOrEmpty(requestDispatch?.Valor))
			{
				messageDetails.Append(AdditionalInformationText);

				AppendDataInNewTableIfNotEmpty(messageDetails, DocumentCsvIdText, csvDocument?.Valor);
				AppendDataInNewTableIfNotEmpty(messageDetails, ClearanceRequestText, requestDispatch?.Valor);
			}

			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected()
		{
			var messageDetails = new StringBuilder();
			AppendResponseDetails(messageDetails);
			return messageDetails.ToString();
		}

		void AppendResponseDetails(StringBuilder messageDetails)
		{
			if (!string.IsNullOrEmpty(response.CsVdeDeclaracionElectronica))
			{
				AppendDataInNewTableIfNotEmpty(messageDetails, CsvIdText, response.CsVdeDeclaracionElectronica);
				messageDetails.Append(blankLine);
			}

			AppendDataInNewTableIfNotEmpty(messageDetails, ResponseCodeText, response.CodigoRespuesta);
			AppendDataInNewTableIfNotEmpty(messageDetails, ResponseDescriptionText, response.DescripcionRespuesta);
		}

		const string RequestDispatchTag = "SolicitudDespacho";
		const string CsvDocumentTag = "CSVdelDocumentoEnviado";
	}
}
