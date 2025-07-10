using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC015C_v515.CC015CV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class DepartureNCTSMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new DepartureNCTSMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedDeclaration()
		{
			var declarationResponse = SetResponseData(new DateTime(2022, 12, 12, 00, 00, 00), new DateTime(2022, 12, 12, 00, 00, 00), new DateTime(2022, 12, 12, 00, 00, 00), "22ES000101500659J7", "V", "A198543129CBF854", "DE");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>12-12-2022, 00:00:00</td></tr>" +
				"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101500659J7</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>12-12-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>A198543129CBF854</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>12-12-2022</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Dispatch</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var declarationResponse = new Cc015Cv1Sal();
			declarationResponse.ControlRespuesta = new Cc015Cv1SalControlRespuesta()
			{
				TipoRespuesta = "EF"
			};

			var error1 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CC015C/TransitOperation/MRN", "PRLSV00000012-ES89890010F", "1234");
			var error2 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CC015C/TransitOperation/MRN", ZString.Empty, "1234");
			declarationResponse.FunctionalError = new Collection<FunctionalErrorType> { error1, error2 };

			var messagePrettyFormatter = new DepartureNCTSMessagePrettyFormatter(declarationResponse);

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
			var declarationResponse = new Cc015Cv1Sal();
			declarationResponse.ControlRespuesta = new Cc015Cv1SalControlRespuesta()
			{
				TipoRespuesta = "EX"
			};

			var error1 = SetXMLError((XmlErrorCodes)18, "14", "34", "Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC014CV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC014CV1Ent.xsd}totalAmountInvoiced", "Wrong value", "location");
			var error2 = SetXMLError(new XmlErrorCodes(), "14", "34", ZString.Empty, ZString.Empty, ZString.Empty);
			declarationResponse.XmlError = new Collection<XmlErrorType> { error1, error2 };

			var messagePrettyFormatter = new DepartureNCTSMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14 / 34</td><td>location</td><td>18</td><td>Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC014CV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC014CV1Ent.xsd}totalAmountInvoiced</td><td>Wrong value</td></tr>" +
				"<tr><td>14 / 34</td><td>&nbsp;</td><td>Item12</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>", messageInterpretationText);
		}

		public void TestLimitDateOfArrival()
		{
			var declarationResponse = SetResponseData(DateTime.Today, DateTime.Today, DateTime.Today);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedLimitDateText = "<br><table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>19-08-2021</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Limit date of arrival if it's not in the response", expectedLimitDateText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, new DateTime(2021, 08, 19), DateTime.Today);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test include Limit date of arrival if it's in the response", expectedLimitDateText, messageInterpretationText);
			});
		}

		public void TestCircuitData()
		{
			var declarationResponse = SetResponseData(DateTime.Today, DateTime.Today, DateTime.Today);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Circuit if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, DateTime.Today, DateTime.Today, circuit: "V");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, DateTime.Today, DateTime.Today, circuit: "N");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, DateTime.Today, DateTime.Today, circuit: "R");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public void TestClearanceData()
		{
			var declarationResponse = SetResponseData(DateTime.Today, DateTime.Today, DateTime.Today);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedClearanceText = "<tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>B9026422646FE2AB</td></tr>";
			var expectedClearanceDateText = "<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>01-08-2021</td></tr>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Clearance Number if it's not in the response", expectedClearanceText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, DateTime.Today, DateTime.Today, clearance: "B9026422646FE2AB");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test include Clearance Number if it's in the response", expectedClearanceText, messageInterpretationText);

				AssertNotContains("Test No include Clearance Date if it's not in the response", expectedClearanceDateText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, DateTime.Today, new DateTime(2021, 08, 01, 11, 05, 05), clearance: "B9026422646FE2AB");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test include Clearance Date if it's in the response", expectedClearanceDateText, messageInterpretationText);
			});
		}

		public void TestStatusData()
		{		 
			var expectedPAStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PA - Pre-Declaration</td></tr></table>";
			var expectedPGStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PG - Pending Guarantee</td></tr></table>";
			var expectedPDStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>";
			var expectedDEStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Dispatch</td></tr></table>";

			CombineAssertions(() =>
			{
				var declarationResponse = SetResponseData(DateTime.Today, DateTime.Today, DateTime.Today, status: "PA");
				var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PA Status", expectedPAStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, DateTime.Today, DateTime.Today, status: "PG");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PG Status", expectedPGStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, DateTime.Today, DateTime.Today, status: "PD");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test PD Status", expectedPDStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(DateTime.Today, DateTime.Today, DateTime.Today, status: "DE");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test DE Status", expectedDEStatusText, messageInterpretationText);
			});
		}

		Cc015Cv1Sal SetResponseData(DateTime acceptanceDate, DateTime limitDate, DateTime clearanceDate, string mrn = "", string circuit = "", string clearance = "", string status = "")
		{
			var response = new Cc015Cv1Sal();
			response.DatosRespuestaCorrecta = new DatosRespuestaCorrectaType015C()
			{
				FechaHoraAlta = acceptanceDate,
				Mrn = mrn,
				FechaLimiteLlegada = limitDate,
				CircuitoExpedicion = circuit,
				CsVdeDat = clearance,
				FechaLevante = clearanceDate,
				Estado = status
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(Cc015Cv1Sal response)
		{
			var messagePrettyFormatter = new DepartureNCTSMessagePrettyFormatter(response);
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
