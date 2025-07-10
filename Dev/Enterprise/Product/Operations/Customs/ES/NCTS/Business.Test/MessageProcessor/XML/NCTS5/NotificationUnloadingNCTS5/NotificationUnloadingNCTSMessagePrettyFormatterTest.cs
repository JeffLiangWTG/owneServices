using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC044C_v515.CC044CV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NotificationUnloadingNCTSMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new NotificationUnloadingNCTSMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedDeclaration()
		{
			var declarationResponse = SetResponseData("22ES000101500659J7", "RD");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>22ES000101500659J7</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RD - Pending Resolution Of Discrepancy</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var declarationResponse = new Cc044Cv1Sal();
			declarationResponse.ControlRespuesta = new Cc044Cv1SalControlRespuesta()
			{
				TipoRespuesta = "EF"
			};

			var error1 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CC015C/TransitOperation/MRN", "PRLSV00000012-ES89890010F", "1234");
			var error2 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CC015C/TransitOperation/MRN", ZString.Empty, "1234");
			declarationResponse.FunctionalError = new Collection<FunctionalErrorType> { error1, error2 };

			var messagePrettyFormatter = new NotificationUnloadingNCTSMessagePrettyFormatter(declarationResponse);

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
			var declarationResponse = new Cc044Cv1Sal();
			declarationResponse.ControlRespuesta = new Cc044Cv1SalControlRespuesta()
			{
				TipoRespuesta = "EX"
			};

			var error1 = SetXMLError((XmlErrorCodes)18, "14", "34", "Se esperaba nodo<br>\t\t\t\t\t{https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC007CV1Ent.xsd}messageSender<br>\t\t\t\t\ty ha venido<br>\t\t\t\t\t{https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC007CV1Ent.xsd}messageRecipient", "Wrong value", "location");
			var error2 = SetXMLError(new XmlErrorCodes(), "14", "34", ZString.Empty, ZString.Empty, ZString.Empty);
			declarationResponse.XmlError = new Collection<XmlErrorType> { error1, error2 };

			var messagePrettyFormatter = new NotificationUnloadingNCTSMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14 / 34</td><td>location</td><td>18</td><td>Se esperaba nodo<br>\t\t\t\t\t{https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC007CV1Ent.xsd}messageSender<br>\t\t\t\t\ty ha venido<br>\t\t\t\t\t{https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC007CV1Ent.xsd}messageRecipient</td><td>Wrong value</td></tr>" +
				"<tr><td>14 / 34</td><td>&nbsp;</td><td>Item12</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>", messageInterpretationText);
		}

		public void TestStatus()
		{
			var expectedRDStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RD - Pending Resolution Of Discrepancy</td></tr></table>";
			var expectedULStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>UL - Completed</td></tr></table>";

			CombineAssertions(() =>
			{
				var declarationResponse = SetResponseData(status: "RD");
				var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test RE flag mismatch", expectedRDStatusText, messageInterpretationText);

				declarationResponse = SetResponseData(status: "UL");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test UL flag mismatch", expectedULStatusText, messageInterpretationText);
			});
		}

		Cc044Cv1Sal SetResponseData(string mrn = "", string status = "")
		{
			var response = new Cc044Cv1Sal();
			var controlRespuesta = new Cc044Cv1SalControlRespuesta();
			controlRespuesta.CodigoRespuesta = "R";
			response.ControlRespuesta = controlRespuesta;
			response.DatosRespuestaCorrecta = new DatosRespuestaCorrectaType044C()
			{
				Mrn = mrn,
				Estado = status
			};
			return response;
		}

		ZString GetAcceptedInterpretationText(Cc044Cv1Sal response)
		{
			var messagePrettyFormatter = new NotificationUnloadingNCTSMessagePrettyFormatter(response);
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
