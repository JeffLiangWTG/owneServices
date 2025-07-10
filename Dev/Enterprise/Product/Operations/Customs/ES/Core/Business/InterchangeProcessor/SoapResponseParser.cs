using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using HtmlAgilityPack;

namespace Enterprise.Customs.ES.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Response Parser")]
	public class SoapResponseParser
	{
		public SoapResponseParser(string responseContent)
		{
			this.responseContent = responseContent;
		}

		readonly string responseContent;

		const string NoCertificateErrorCode = "403";
		const string NoCertificateErrorDescription = "No se detecta certificado digital o no se ha seleccionado correctamente.";
		const string ClientErrorCode = "400";
		const string ClientErrorDescription = "Error del cliente. Se ha producido un error en la solicitud que impide al servidor procesarla.";
		const string DocumentErrorTitle = "Cotejo de Documentos";
		const string SoapNamespace = "http://schemas.xmlsoap.org/soap/envelope/";

		public MessageProcessingResult Parse()
		{
			if (responseContent.Contains(NoCertificateErrorCode) && responseContent.Contains(NoCertificateErrorDescription))
			{
				return MessageProcessingResult.Unauthorized("403: Digital certificate not detected or not selected correctly.");
			}

			if (responseContent.Contains(ClientErrorCode) && responseContent.Contains(ClientErrorDescription))
			{
				return MessageProcessingResult.BadRequest("Client Error. An error has occurred in the request that prevents the server from processing it.");
			}

			if (responseContent.Contains(DocumentErrorTitle))
			{
				return MessageProcessingResult.BadRequest(GetDocumentErrorDescription(responseContent));
			}

			if (!TryParseXmlDocument(responseContent, out var document) || !IsSoapEnvelope(document))
			{
				return MessageProcessingResult.RemoteServerError(responseContent);
			}

			if (IsSoapFault(document, out var faultCode, out var faultDescription))
			{
				return faultCode.Trim().ToLower() == "soapenv:client"
					? MessageProcessingResult.BadRequest(faultDescription)
					: MessageProcessingResult.RemoteServerError(faultDescription);
			}

			var message = Encoding.UTF8.GetBytes(document.OuterXml);
			return MessageProcessingResult.Success(message);
		}

		bool TryParseXmlDocument(string content, out XmlDocument document)
		{
			try
			{
				var xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(content);
				document = xmlDocument;
				return true;
			}
			catch (XmlException)
			{
				document = null;
				return false;
			}
		}

		bool IsSoapEnvelope(XmlDocument document)
		{
			var root = document.DocumentElement;
			return root != null && root.NamespaceURI == SoapNamespace && root.LocalName == "Envelope";
		}

		bool IsSoapFault(XmlDocument document, out string faultCode, out string faultDescription)
		{
			var nsmgr = new XmlNamespaceManager(document.NameTable);
			nsmgr.AddNamespace("soapenv", SoapNamespace);

			var soapFault = document.SelectSingleNode(@"//soapenv:Fault", nsmgr);
			if (soapFault == null)
			{
				faultCode = null;
				faultDescription = null;
				return false;
			}

			faultCode = soapFault.SelectSingleNode(@"//faultcode", nsmgr)?.InnerText ?? string.Empty;
			faultDescription = soapFault.SelectSingleNode(@"//faultstring", nsmgr)?.InnerText ?? string.Empty;
			return true;
		}

		string GetDocumentErrorDescription(string content)
		{
			var errorDescription = string.Empty;

			var htmlContent = WebUtility.HtmlDecode(responseContent);

			var document = new HtmlDocument();
			document.LoadHtml(htmlContent);

			var documentNode = document.DocumentNode;

			var descriptionList = documentNode.SelectSingleNode(@"//body//div[starts-with(@class, ""AEAT_bloque_errores"")]//ul");

			if (descriptionList != null)
			{
				foreach (var liNode in descriptionList.ChildNodes.Where(x => x.Name == "li"))
				{
					errorDescription += liNode.InnerText + " ";
				}
			}
			return errorDescription;
		}
	}
}
