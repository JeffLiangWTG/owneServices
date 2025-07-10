using System.IO;
using System.Text;
using CargoWise.Customs.ES.MessageDefinitions.EdifactResponse;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class EdifactResponseParserTest : TestCaseWithFactory
	{
		public void TestParseSuccessResponseContent()
		{
			var content = GetManifestResourceString("ResponseWithData.html");
			var parser = new EdifactResponseParser(content);
			var result = parser.Parse();

			CombineAssertions(() =>
			{
				AssertEquals(ResponseStatus.Successful, result.ResponseStatus);
				AssertNull(result.ErrorMessage);

				var data = XmlUtils.Deserialize(typeof(Response), result.ResponseMessage) as Response;

				AssertNotNull(data);
				AssertNull(data.ErrorStatus);
				AssertEquals("0", data.DeclarationStatus);
				AssertEquals("UNB+UNOA:1+AEATADUE:ZZ+BUZON:ZZ+200102:1100+02110053624233++&EE'UNH+1+CUSRES:1:921:UN:ECS001'", data.Respuesta);
			});
		}

		public void TestParseSuccessButDuplicatedResponseContent()
		{
			var content = GetManifestResourceString("ResponseWithDataDuplicado.html");
			var parser = new EdifactResponseParser(content);
			var result = parser.Parse();

			CombineAssertions(() =>
			{
				AssertEquals(ResponseStatus.Successful, result.ResponseStatus);
				AssertNull(result.ErrorMessage);

				var data = XmlUtils.Deserialize(typeof(Response), result.ResponseMessage) as Response;

				AssertNotNull(data);
				AssertNull(data.ErrorStatus);
				AssertEquals("0", data.DeclarationStatus);
				AssertEquals("Processe But Duplicated", data.Respuesta);
			});
		}

		public void TestParseSuccessResponseButWithSquareBracketsNodes()
		{
			var content = GetManifestResourceString("ResponseWithSquareBracketsNodes.html");
			var parser = new EdifactResponseParser(content);
			var result = parser.Parse();

			CombineAssertions(() =>
			{
				AssertEquals(ResponseStatus.InternalServerError, result.ResponseStatus);
				AssertNull(result.ResponseMessage);

				AssertEquals(@"ERROR PROCESO EDI
<Response>
  <DeclarationStatus>102</DeclarationStatus>
  <ErrorStatus>TestError</ErrorStatus>
</Response>", result.ErrorMessage
	.Replace(@" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""", string.Empty)
	.Replace(@" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", string.Empty));
			});
		}

		public void TestParseSuccessResponseContentWithInvalidDeclarationStatus()
		{
			TestParseSuccessResponseContentWithSpecficStatus("1", ResponseStatus.Successful);
			TestParseSuccessResponseContentWithSpecficStatus("2", ResponseStatus.Successful);
			TestParseSuccessResponseContentWithSpecficStatus("6", ResponseStatus.InternalServerError);
			TestParseSuccessResponseContentWithSpecficStatus("7", ResponseStatus.InternalServerError);
		}

		void TestParseSuccessResponseContentWithSpecficStatus(string status, ResponseStatus responseStatus)
		{
			var content = GetManifestResourceString("ResponseWithData.html");
			content = content.Replace("<cod-ret>0</cod-ret>", $"<cod-ret>{status}</cod-ret>");

			var parser = new EdifactResponseParser(content);
			var result = parser.Parse();

			CombineAssertions(() =>
			{
				AssertEquals(responseStatus, result.ResponseStatus);

				if (responseStatus == ResponseStatus.InternalServerError)
				{
					AssertEquals($"Declaration Status is {status}.", result.ErrorMessage);
					AssertNull(result.ResponseMessage);
				}
				else
				{
					AssertNull(result.ErrorMessage);
					AssertNotNull(result.ResponseMessage);
				}
			});
		}

		public void TestParseResponseContentWith500Error()
		{
			var content = GetManifestResourceString("ResponseWith500Error.html");

			var parser = new EdifactResponseParser(content);
			var result = parser.Parse();

			CombineAssertions(() =>
			{
				AssertEquals(ResponseStatus.InternalServerError, result.ResponseStatus);
				AssertEquals("ERROR 500", result.ErrorMessage);
				AssertNull(result.ResponseMessage);
			});
		}

		public void TestParseErrorResponseContent()
		{
			var content = GetManifestResourceString("ResponseWithError.html");
			var parser = new EdifactResponseParser(content);
			var result = parser.Parse();

			CombineAssertions(() =>
			{
				AssertEquals(ResponseStatus.BadRequest, result.ResponseStatus);
				AssertNull(result.ResponseMessage);

				AssertEquals(@"Se ha producido un error:
904 Firma no válida: -8: El campo 3 no se ajusta al formato PKCSÑ7 y la opción es 2 ó 3
Para más información sobre este error pulse aquí
Detalles del error
Código del error:
904
Fecha y hora del error:
12-04-2021/08:14
Descripción:
Firma no válida: -8: El campo 3 no se ajusta al formato PKCSÑ7 y la opción es 2 ó 3
Servidor:
WLP00401
Aplicación:
ADEX
Módulo donde se detecta el error:
es.aeat.dit.adu.adex.edi.serv.ECS003
Id. Error:
50314896
Puede contactar con el servicio de atención al contribuyente pulsando en el enlace inferior
Comunicar Incidencia", result.ErrorMessage);
			});
		}

		public void TestParseResponseContentFor403InvalidCertificate()
		{
			var content = GetManifestResourceString("ResponseWithInvalidCertificate403.html");
			var parser = new EdifactResponseParser(content);
			var result = parser.Parse();

			CombineAssertions(() =>
			{
				AssertEquals(ResponseStatus.BadRequest, result.ResponseStatus);
				AssertNull(result.ResponseMessage);

				AssertEquals(@"Se ha producido un error:
403
Error de identificación. No se detecta certificado digital o no se ha seleccionado correctamente.
Puede contactar con el servicio de atención al contribuyente indicando el código de error 403.
Comunicar incidencia", result.ErrorMessage);
			});
		}

		public void TestParseResponseContentFor401InvalidCertificate()
		{
			var content = GetManifestResourceString("ResponseWithInvalidCertificate401.html");
			var parser = new EdifactResponseParser(content);
			var result = parser.Parse();

			CombineAssertions(() =>
			{
				AssertEquals(ResponseStatus.BadRequest, result.ResponseStatus);
				AssertNull(result.ResponseMessage);

				AssertEquals(@"Se ha producido un error:
401
No autorizado. Se ha producido un error al verificar el certificado presentado. Las causas más probables de este error son:
- El certificado no ha sido firmado por una autoridad reconocida.
- El tipo de certificado no es válido para el servicio al que se quiere acceder.
- El certificado ha expirado.
Puede contactar con el servicio de atenciÃ³n al contribuyente indicando el cÃ³digo de error 401.
Comunicar incidencia", result.ErrorMessage);
			});
		}

		public void TestParseInvalidResponseContent()
		{
			var responseContent = "<html>Invalid Response Html Content</html>";
			var parser = new EdifactResponseParser(responseContent);
			var result = parser.Parse();

			CombineAssertions(() =>
			{
				AssertEquals(ResponseStatus.InternalServerError, result.ResponseStatus);
				AssertNull(result.ResponseMessage);

				AssertEquals($@"Can not parse the response content:{System.Environment.NewLine}{responseContent}", result.ErrorMessage);
			});
		}

		static string GetManifestResourceString(string fileName)
		{
			using (var stream = typeof(EdifactResponseParserTest).Assembly.GetManifestResourceStream($"Enterprise.Customs.ES.Business.Testing.InterchangeProcessor.Resources.EdifactMessages.{fileName}"))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
