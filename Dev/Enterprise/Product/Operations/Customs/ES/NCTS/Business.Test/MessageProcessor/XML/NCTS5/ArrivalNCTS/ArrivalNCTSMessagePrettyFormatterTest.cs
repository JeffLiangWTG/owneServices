using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC007C_v515.CC007CV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class ArrivalNCTSMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new ArrivalNCTSMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedDeclaration()
		{
			var declarationResponse = SetResponseData(new DateTime(2022, 12, 12, 00, 00, 00), "R", "22ES000101500659J7", "V", "SM98543129CBF854", "1", "RE");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>12-12-2022, 00:00:00</td></tr>" +
				"<tr><td>MRN Arrival:</td><td>&nbsp;&nbsp;</td><td>22ES000101500659J7</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Summary Decl:</td><td>&nbsp;&nbsp;</td><td>SM98543129CBF854</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Discrepancies previous summary:</td><td>&nbsp;&nbsp;</td><td>No deviations between Transit and Previous Summary</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RE - Received</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsAcceptedDeclarationTypeS()
		{
			var declarationResponse = SetResponseData(new DateTime(2022, 12, 12, 00, 00, 00), "S");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>Requesting data to Customs Departure. Please, send message again after 15 minutes</td></tr>" +
				"</table><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>12-12-2022, 00:00:00</td></tr></table>";

			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var declarationResponse = new Cc007Cv1Sal();
			declarationResponse.ControlRespuesta = new Cc007Cv1SalControlRespuesta()
			{
				TipoRespuesta = "EF"
			};

			var error1 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CC015C/TransitOperation/MRN", "PRLSV00000012-ES89890010F", "1234");
			var error2 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CC015C/TransitOperation/MRN", ZString.Empty, "1234");
			declarationResponse.FunctionalError = new Collection<FunctionalErrorType> { error1, error2 };

			var messagePrettyFormatter = new ArrivalNCTSMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14</td><td>/CC015C/TransitOperation/MRN</td><td>(1234) No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.</td><td>PRLSV00000012-ES89890010F</td></tr>" +
				"<tr><td>14</td><td>/CC015C/TransitOperation/MRN</td><td>(1234) No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.</td><td>&nbsp;</td></tr></table>", messageInterpretationText);
		}

		public void TestCreateMessageDetailsError()
		{
			var declarationResponse = new Cc007Cv1Sal();
			declarationResponse.ControlRespuesta = new Cc007Cv1SalControlRespuesta()
			{
				TipoRespuesta = "EX"
			};

			var error1 = SetXMLError((XmlErrorCodes)18, "14", "34", "Se esperaba nodo<br>\t\t\t\t\t{https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC007CV1Ent.xsd}messageSender<br>\t\t\t\t\ty ha venido<br>\t\t\t\t\t{https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC007CV1Ent.xsd}messageRecipient", "Wrong value", "location");
			var error2 = SetXMLError(new XmlErrorCodes(), "14", "34", ZString.Empty, ZString.Empty, ZString.Empty);
			declarationResponse.XmlError = new Collection<XmlErrorType> { error1, error2 };

			var messagePrettyFormatter = new ArrivalNCTSMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14 / 34</td><td>location</td><td>18</td><td>Se esperaba nodo<br>\t\t\t\t\t{https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC007CV1Ent.xsd}messageSender<br>\t\t\t\t\ty ha venido<br>\t\t\t\t\t{https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC007CV1Ent.xsd}messageRecipient</td><td>Wrong value</td></tr>" +
				"<tr><td>14 / 34</td><td>&nbsp;</td><td>Item12</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>", messageInterpretationText);
		}

		public void TestRegisterData()
		{
			var expectedAcceptanceText = "<tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>12-12-2022, 00:00:00</td></tr>";
			var expectedRegisterText = "<tr><td>MRN Arrival:</td><td>&nbsp;&nbsp;</td><td>21ES00999912345678</td></tr>";

			CombineAssertions(() =>
			{
				var declarationResponse = SetResponseData(new DateTime(2022, 12, 12, 00, 00, 00), responseCode: "R");
				var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test include Acceptance if it's in the response", expectedAcceptanceText, messageInterpretationText);

				AssertNotContains("Test No include Register if it's not in the response", expectedRegisterText, messageInterpretationText);

				declarationResponse = SetResponseData(new DateTime(2022, 12, 12, 00, 00, 00), mrn: "21ES00999912345678", responseCode: "R");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test include Register if it's in the response", expectedRegisterText, messageInterpretationText);
			});
		}

		public void TestCircuitData()
		{
			var declarationResponse = SetResponseData(DateTime.Today);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Circuit if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, circuit: "V");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, circuit: "N");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, circuit: "R");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public void TestFlagMismatchData()
		{
			var expected1FlagMismatchText = "<tr><td>Discrepancies previous summary:</td><td>&nbsp;&nbsp;</td><td>No deviations between Transit and Previous Summary</td></tr>";
			var expected2FlagMismatchText = "<tr><td>Discrepancies previous summary:</td><td>&nbsp;&nbsp;</td><td>Deviation in packages quantity between Transit and Previous Summary</td></tr>";
			var expected3FlagMismatchText = "<tr><td>Discrepancies previous summary:</td><td>&nbsp;&nbsp;</td><td>Deviation in gross weight  between Transit and Previous Summary</td></tr>";
			var expected4FlagMismatchText = "<tr><td>Discrepancies previous summary:</td><td>&nbsp;&nbsp;</td><td>Deviation in gross weight  and packages quantity between Transit and Previous Summary</td></tr>";

			CombineAssertions(() =>
			{
				var declarationResponse = SetResponseData(DateTime.Today, flagMismatch: "1");
				var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test 1 flag mismatch", expected1FlagMismatchText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, flagMismatch: "2");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test 2 flag mismatch", expected2FlagMismatchText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, flagMismatch: "3");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test 3 flag mismatch", expected3FlagMismatchText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, flagMismatch: "4");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test 4 flag mismatch", expected4FlagMismatchText, messageInterpretationText);
			});
		}

		public void TestSummaryData()
		{
			var declarationResponse = SetResponseData(DateTime.Today);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedSummaryText = "<tr><td>Summary Decl:</td><td>&nbsp;&nbsp;</td><td>SM026422646FE2AB</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Summary Number if it's not in the response", expectedSummaryText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, summary: "SM026422646FE2AB");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test include Summary Number if it's in the response", expectedSummaryText, messageInterpretationText);
			});
		}

		public void TestStatus()
		{
			var expectedREStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RE - Received</td></tr></table>";
			var expectedULStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>UL - Completed</td></tr></table>";
			var expectedRQStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RQ - Requesting to the country of departure</td></tr></table>";

			CombineAssertions(() =>
			{
				var declarationResponse = SetResponseData(DateTime.Today, status: "RE");
				var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test RE flag mismatch", expectedREStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, status: "UL");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test UL flag mismatch", expectedULStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, status: "RQ");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test RQ flag mismatch", expectedRQStatusText, messageInterpretationText);
			});
		}

		public void TestResponseCode()
		{
			var declarationResponse = SetResponseData(DateTime.Today, responseCode: "");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedSummaryText = "<br><table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>Requesting data to Customs Departure. Please, send message again after 15 minutes</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Response Code if it's not in the response", expectedSummaryText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, responseCode: "S");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test include Response Code,  will not appear in the message", expectedSummaryText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, responseCode: "R");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertNotContains("Test include Response Code = R, will not appear in the message", expectedSummaryText, messageInterpretationText);
			});
		}

		Cc007Cv1Sal SetResponseData(DateTime preparationDate, string responseCode = "S", string mrn = "", string circuit = "", string summary = "", string flagMismatch = "", string status = "")
		{
			var response = new Cc007Cv1Sal();
			var controlRespuesta = new Cc007Cv1SalControlRespuesta();
			controlRespuesta.CodigoRespuesta = responseCode;
			response.ControlRespuesta = controlRespuesta;
			response.DatosRespuestaCorrecta = new DatosRespuestaCorrectaType007C()
			{
				FechaHoraRecepcion = preparationDate,
				Mrn = mrn,
				CircuitoRecepcion = circuit,
				NumeroSumariaRecepcionG4Ultimacion = summary,
				IndicadorDescuadreConPrevia = flagMismatch,
				Estado = status
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(Cc007Cv1Sal response)
		{
			var messagePrettyFormatter = new ArrivalNCTSMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}

		FunctionalErrorType SetFunctionalError(ZString errorCode, ZString errorDescription, ZString errorLocation, ZString wrongValue, ZString reason)
		{
			var error = new FunctionalErrorType();
			error.ErrorCode = errorCode;
			error.ErrorDescription = errorDescription;
			error.ErrorPointer = errorLocation;
			error.OriginalAttributeValue = wrongValue;
			error.ErrorReason = reason;
			return error;
		}

		XmlErrorType SetXMLError(XmlErrorCodes errorCode, ZString errorLineNumber, ZString errorColumnNumber, ZString errorText, ZString wrongValue, ZString location)
		{
			var error = new XmlErrorType();
			error.ErrorCode = errorCode;
			error.ErrorLineNumber = errorLineNumber;
			error.ErrorColumnNumber = errorColumnNumber;
			error.ErrorText = errorText;
			error.OriginalAttributeValue = wrongValue;
			error.ErrorPointer = location;
			return error;
		}
	}
}
