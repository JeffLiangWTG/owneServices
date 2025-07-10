using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.DocumentosSimplifiV1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DJPImportMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestCreateMessageDetailsAccepted()
		{
			var declarationResponse = SetResponseData("20211004", "103045", "21ES00999912345678", CircuitoTd.V, responseDescription: "DUA Accepted", responseCode: "0", csvCode: "3AG5G6SSCJJ93NML");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>(0)DUA Accepted</td></tr></table>" +
				"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>04-10-2021, 10:30:45</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var declarationResponse = new DocumentosSimplifiV1Sal();
			declarationResponse.CodigoRespuesta = "1005";
			declarationResponse.DescripcionRespuesta = "Accepted without process imports because some exceed the maximum updates supported";

			var error1 = SetError("6112", "Hay un documento declarado no necesario", 0, 0, null, "N740");
			var error2 = SetError("6113", "Error 2", 1, 1, "test", "N750");

			var errors = new Collection<ErrorTd> { error1, error2 };
			declarationResponse.DeclaracionErronea = new Collection<DeclaracionErrTd>
			{
				new DeclaracionErrTd
				{
					Error = errors
				}
			};

			var messagePrettyFormatter = new DJPImportMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			var expectedMessageInterpretation = "<H3>Rejected Declaration</H3>" +
						"<table border=\"0\">" +
						"<tr><td>1005:</td><td>&nbsp;&nbsp;</td><td>Accepted without process imports because some exceed the maximum updates supported</td></tr>" +
						"</table><br>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Error</strong></td><td><strong>Location/Description</strong></td></tr>" +
						"<tr><td>6112</td><td>0.0<br>Hay un documento declarado no necesario..N740</td></tr>" +
						"<tr><td>6113</td><td>1.1<br>Error 2.test.N750</td></tr>" +
						"</table>";

			AssertEquals("Expected Error message interpretation", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestMessageAndCircuitData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<H3>Accepted Declaration</H3><br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<H3>Accepted Declaration</H3><br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<H3>Accepted Declaration</H3><br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";
			var expectedYellowCircuitText = "<H3>Accepted Declaration</H3><br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#C6C600\">YELLOW</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Circuit if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, CircuitoTd.V, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, CircuitoTd.N, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, CircuitoTd.R, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, CircuitoTd.A, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Yellow Circuit", expectedYellowCircuitText, messageInterpretationText);
			});
		}

		public void TestAcceptanceData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>04-10-2021, 10:30:45</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Acceptance data if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData("20211004", "103045", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Acceptance data", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestReferenceResponseData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedReferenceText = "<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Reference response data if it's not in the response", expectedReferenceText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "21ES00999912345678", null, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Reference response data", expectedReferenceText, messageInterpretationText);
			});
		}

		DocumentosSimplifiV1Sal SetResponseData(ZString date, ZString hour, ZString reference, CircuitoTd? circuit, ZString responseDescription, ZString responseCode, ZString csvCode)
		{
			var response = new DocumentosSimplifiV1Sal();
			response.SegmentosDeServicio = new SegmDeServicioTd();
			response.SegmentosDeServicio.Fecha = date;
			response.SegmentosDeServicio.Hora = hour;
			response.NumeroDeReferencia = reference;
			response.Circuito = circuit;
			response.CodigoRespuesta = responseCode;
			response.DescripcionRespuesta = responseDescription;
			response.CsVdeDeclaracionElectronica = csvCode;
			return response;
		}

		ZString GetAcceptedInterpretationText(DocumentosSimplifiV1Sal response)
		{
			var messagePrettyFormatter = new DJPImportMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}

		ErrorTd SetError(ZString errorCode, ZString errorDescription, ZInt errorLineNumber, ZInt errorElementNumber, ZString errorTag, ZString wrongValue)
		{
			var error = new ErrorTd();
			error.CodigoError = errorCode;
			error.DescripcionError = errorDescription;
			error.NumeroOrdenPartidaConError = errorLineNumber;
			error.NumeroOrdenElementoErroneo = errorElementNumber;
			error.EtiquetaConError = errorTag;
			error.ValorErroneo = wrongValue;
			return error;
		}
	}
}
