using System.IO;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class SoapResponseParserTest : TestCaseWithFactory
	{
		public void TestParseResponseWithInvalidCertificate()
		{
			var content = GetManifestResourceString("ResponseWithInvalidCertificate.html");
			var parser = new SoapResponseParser(content);
			var result = parser.Parse();

			CombineAssertions(() =>
			{
				AssertEquals(ResponseStatus.Unauthorized, result.ResponseStatus);
				AssertEquals("403: Digital certificate not detected or not selected correctly.", result.ErrorMessage);
				AssertNull(result.ResponseMessage);
			});
		}

		public void TestParseResponseWithClientError()
		{
			var content = GetManifestResourceString("ResponseWithClientError.html");
			var parser = new SoapResponseParser(content);
			var result = parser.Parse();

			CombineAssertions(() =>
			{
				AssertEquals(ResponseStatus.BadRequest, result.ResponseStatus);
				AssertEquals("Client Error. An error has occurred in the request that prevents the server from processing it.", result.ErrorMessage);
				AssertNull(result.ResponseMessage);
			});
		}

		public void TestParseResponseWithDocumentError()
		{
			var content = GetManifestResourceString("ResponseWithDocumentError.html");
			var parser = new SoapResponseParser(content);
			var result = parser.Parse();

			CombineAssertions(() =>
			{
				AssertEquals(ResponseStatus.BadRequest, result.ResponseStatus);
				AssertEquals("El código que ha utilizado en el cotejo no es un CSV válido:  8M6A78YWSYTPPZ5G. ERROR: El CSV no existe. ", result.ErrorMessage);
				AssertNull(result.ResponseMessage);
			});
		}

		public void TestParseResponseWithFlatResponse()
		{
			var content = "I am not a SOAP service.";
			var parser = new SoapResponseParser(content);
			var result = parser.Parse();

			CombineAssertions(() =>
			{
				AssertEquals(ResponseStatus.InternalServerError, result.ResponseStatus);
				AssertEquals("I am not a SOAP service.", result.ErrorMessage);
				AssertNull(result.ResponseMessage);
			});
		}

		public void TestParseResponseWithHtmlResponse()
		{
			var content = "<html><body>I am not a SOAP service.</body></html>";
			var parser = new SoapResponseParser(content);
			var result = parser.Parse();

			CombineAssertions(() =>
			{
				AssertEquals(ResponseStatus.InternalServerError, result.ResponseStatus);
				AssertEquals("<html><body>I am not a SOAP service.</body></html>", result.ErrorMessage);
				AssertNull(result.ResponseMessage);
			});
		}

		public void TestParseResponseWithXmlResponse()
		{
			var content = GetManifestResourceString("ResponseWithCorrectResponse.xml");
			var parser = new SoapResponseParser(content);
			var result = parser.Parse();

			var xml = new XmlDocument();
			xml.LoadXml(content);

			CombineAssertions(() =>
			{
				AssertEquals(ResponseStatus.Successful, result.ResponseStatus);
				AssertNull(result.ErrorMessage);
				AssertEquals(xml.OuterXml, Encoding.UTF8.GetString(result.ResponseMessage));
			});
		}

		public void TestParseResponseWithSoapClientFault()
		{
			var content = GetManifestResourceString("ResponseWithClientFault.xml");
			var parser = new SoapResponseParser(content);
			var result = parser.Parse();

			var xml = new XmlDocument();
			xml.LoadXml(content);

			CombineAssertions(() =>
			{
				AssertEquals(ResponseStatus.BadRequest, result.ResponseStatus);
				AssertEquals("UserException", result.ErrorMessage);
				AssertNull(result.ResponseMessage);
			});
		}

		public void TestParseResponseWithSoapServerFault()
		{
			var content = GetManifestResourceString("ResponseWithServerFault.xml");
			var parser = new SoapResponseParser(content);
			var result = parser.Parse();

			var xml = new XmlDocument();
			xml.LoadXml(content);

			CombineAssertions(() =>
			{
				AssertEquals(ResponseStatus.InternalServerError, result.ResponseStatus);
				AssertEquals("ServerException", result.ErrorMessage);
				AssertNull(result.ResponseMessage);
			});
		}

		static string GetManifestResourceString(string fileName)
		{
			using (var stream = typeof(EdifactResponseParserTest).Assembly.GetManifestResourceStream($"Enterprise.Customs.ES.Business.Testing.InterchangeProcessor.Resources.SoapMessages.{fileName}"))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
