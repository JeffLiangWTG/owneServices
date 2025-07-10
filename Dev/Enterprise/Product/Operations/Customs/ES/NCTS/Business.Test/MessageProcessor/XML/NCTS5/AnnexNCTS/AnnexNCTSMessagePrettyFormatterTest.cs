using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CCDOTC_v515.CCDOTCV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class AnnexNCTSMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new AnnexNCTSMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedDeclaration()
		{
			var doc1 = SetDocument("reference1", "EDQGYLR8BHGKCVR9");
			var doc2 = SetDocument("reference2", "3AG5G6SSCJJ93NML");
			var doc3 = SetDocument("reference3", "AAAAAAAAAAAAAAAA");
			var doc4 = SetDocument("reference4", "BBBBBBBBBBBBBBBB");

			var declarationResponse = SetResponseData("21ES00999912345678", "ABCDEFGHIJKLMNOP", "PD", new Collection<CsvDocumentosDot> { doc1, doc2, doc3, doc4 });
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>ABCDEFGHIJKLMNOP</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>" +
				"<br><H4>CSV Electronic Documents</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Reference</strong></td><td><strong>CSV Document</strong></td></tr>" +
				"<tr><td>reference1</td><td>EDQGYLR8BHGKCVR9</td></tr>" +
				"<tr><td>reference2</td><td>3AG5G6SSCJJ93NML</td></tr>" +
				"<tr><td>reference3</td><td>AAAAAAAAAAAAAAAA</td></tr>" +
				"<tr><td>reference4</td><td>BBBBBBBBBBBBBBBB</td></tr></table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var declarationResponse = new Ccdotcv1Sal();
			declarationResponse.ControlRespuesta = new Ccdotcv1SalControlRespuesta()
			{
				TipoRespuesta = "EF"
			};

			var error1 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CC015C/TransitOperation/MRN", "PRLSV00000012-ES89890010F", "1234");
			var error2 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "/CC015C/TransitOperation/MRN", ZString.Empty, "1234");
			declarationResponse.FunctionalError = new Collection<FunctionalErrorType> { error1, error2 };

			var messagePrettyFormatter = new AnnexNCTSMessagePrettyFormatter(declarationResponse);

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
			var declarationResponse = new Ccdotcv1Sal();
			declarationResponse.ControlRespuesta = new Ccdotcv1SalControlRespuesta()
			{
				TipoRespuesta = "EX"
			};

			var error1 = SetXMLError((XmlErrorCodes)18, "14", "34", "Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC013CV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC014CV1Ent.xsd}totalAmountInvoiced", "Wrong value", "location");
			var error2 = SetXMLError(new XmlErrorCodes(), "14", "34", ZString.Empty, ZString.Empty, ZString.Empty);
			declarationResponse.XmlError = new Collection<XmlErrorType> { error1, error2 };

			var messagePrettyFormatter = new AnnexNCTSMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14 / 34</td><td>location</td><td>18</td><td>Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC013CV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC014CV1Ent.xsd}totalAmountInvoiced</td><td>Wrong value</td></tr>" +
				"<tr><td>14 / 34</td><td>&nbsp;</td><td>Item12</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>", messageInterpretationText);
		}

		public void TestStatusData()
		{
			var expectedPDStatusText = "<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>";

			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, "PD", null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
			AssertContains("Test PD Status", expectedPDStatusText, messageInterpretationText);
		}

		Ccdotcv1Sal SetResponseData(ZString mrn, ZString declarationCSV, ZString status, Collection<CsvDocumentosDot> docs)
		{
			var response = new Ccdotcv1Sal();
			response.DatosRespuestaCorrecta = new DatosRespuestaCorrectaTypeDot()
			{
				Mrn = mrn,
				CsvDeclaracionElectronica = declarationCSV,
				Estado = status,
				CsvDocumentos = docs
			};
			return response;
		}

		CsvDocumentosDot SetDocument(ZString refernce, ZString csvCode)
		{
			var doc = new CsvDocumentosDot()
			{
				NumeroReferencia = refernce,
				CsvDocumentoDigitalizado = csvCode,
			};
			return doc;
		}

		ZString GetAcceptedInterpretationText(Ccdotcv1Sal response)
		{
			var messagePrettyFormatter = new AnnexNCTSMessagePrettyFormatter(response);
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
