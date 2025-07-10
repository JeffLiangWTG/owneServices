using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.CommonAnnex.EnvioDeDocumentosV1Sal;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class CommonAnnexMessagePrettyFormatter : CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public CommonAnnexMessagePrettyFormatter(EnvioDeDocumentosV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly EnvioDeDocumentosV1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedDeclarationText);
			messageDetails.Append(blankLine);
			AppendCsvElectronicDeclarationDataIfNotEmpty(messageDetails, response.CsVdeDeclaracionElectronica);
			var requestDispatch = response.InformacionAdicionalRespuesta.FirstOrDefault(x => x.NombreEtiqueta == RequestDispatchTag);
			if (requestDispatch != null)
			{
				AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, RequestDispatchText, requestDispatch.Valor);
			}

			var csvDocument = response.InformacionAdicionalRespuesta.FirstOrDefault(x => x.NombreEtiqueta == CSVDocumentTag);
			if (csvDocument != null)
			{
				messageDetails.Append(blankLine);
				messageDetails.Append(CSVElectronicDocumentsText);
				AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, CSVDocumentText, csvDocument.Valor);
			}

			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected()
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(RejectedDeclarationText);
			AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, CodeText, response.CodigoRespuesta);
			AppendDataInNewTableIfNotEmpty(messageDetails, ErrorText, response.DescripcionRespuesta);

			return messageDetails.ToString();
		}

		const string RequestDispatchTag = "SolicitudDespacho";
		const string CSVDocumentTag = "CSVdelDocumentoEnviado";
		string RequestDispatchText => ResString.GetMultilingualString("797EF1A2-4F5B-40AB-ACBC-368FE2A2C74E", "Request Dispatch:");
		string CSVDocumentText => ResString.GetMultilingualString("591D093F-1DC3-4400-90BE-ADBAD1FE1B12", "CSV Document:");
		string ErrorText => ResString.GetMultilingualString("67C0E800-1C17-4CD9-9D24-E97E60E12B5D", "Error:");
		string CodeText => ResString.GetMultilingualString("5CD69662-2637-41F1-AF11-CCD37728E79C", "Code:");
	}
}
