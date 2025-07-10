using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ES_CC456A;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ES_ctypes;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.TD5;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing;

public abstract class ImportH1CommonMessagePrettyFormatterTest<TPrettyFormatter, TResponse> : TestCaseWithFactory
	where TPrettyFormatter : ImportH1CommonMessagePrettyFormatter<TResponse>
	where TResponse : class, ICommonErrors
{
	public void TestConstructorNullResponse()
	{
		AssertExceptionThrown<ArgumentNullException>("Null response", () => GetNewPrettyFormatter(null));
	}

	public void TestCreateMessageDetailsAccepted()
	{
		var declarationResponse = ResponseData();
		var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
		AssertEquals("Expected Accepted declaration message interpretation text", ExpectedMessageDetailsAccepted, messageInterpretationText);
	}

	public void TestCreateMessageDetailsRejected() => AssertCreateMessageDetailsRejected(true,
		"<H3>Rejected Declaration</H3>" +
		"<H4>List of Errors:</H4>" +
		"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
		"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
		"<tr><td>14</td><td>LRN</td><td>(1234) No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.</td><td>PRLSV00000012-ES89890010F</td></tr>" +
		"<tr><td>15</td><td>MRN</td><td>(5678) No existe declaración para los valores MRN/Declarant.IdentificationNumber indicados.</td><td>&nbsp;</td></tr></table>");

	public void TestCreateMessageDetailsError() => AssertCreateMessageDetailsRejected(false,
		"<H3>Rejected Declaration</H3>" +
		"<H4>List of Errors:</H4>" +
		"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
		"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
		"<tr><td>14 / 34</td><td>location</td><td>18</td><td>Se esperaba nodo wrong1 security y ha venido wrong2 totalAmountInvoiced</td><td>Wrong value</td></tr>" +
		"<tr><td>15 / 37</td><td>&nbsp;</td><td>25</td><td>desc</td><td>&nbsp;</td></tr></table>");

	void AssertCreateMessageDetailsRejected(bool isFunctionalError, ZString expectedError)
	{
		var declarationResponse = ErrorResponseData(isFunctionalError);
		var messagePrettyFormatter = GetNewPrettyFormatter(declarationResponse);
		var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

		AssertContains(expectedError, messageInterpretationText);
	}

	protected ZString GetAcceptedInterpretationText(TResponse response)
	{
		return GetNewPrettyFormatter(response).CreateMessageDetailsAccepted(ZString.Empty);
	}

	MFunctionalErrorType01 SetFunctionalError(ZString errorCode, ZString errorDescription, ZString errorLocation, ZString wrongValue, ZString reason)
	{
		var error = new MFunctionalErrorType01();
		error.ErrorCode = errorCode;
		error.Remarks = errorDescription;
		error.ErrorPointer = errorLocation;
		error.OriginalAttributeValue = wrongValue;
		error.ErrorReason = reason;
		return error;
	}

	protected Cc456ATypeD SetErrorResponseData()
	{
		var error1 = SetFunctionalError("14", "No existe declaración para los valores LRN/Declarant.IdentificationNumber indicados.", "LRN", "PRLSV00000012-ES89890010F", "1234");
		var error2 = SetFunctionalError("15", "No existe declaración para los valores MRN/Declarant.IdentificationNumber indicados.", "MRN", ZString.Empty, "5678");
		return new Cc456ATypeD()
		{
			FunctionalError = new Collection<MFunctionalErrorType01> { error1, error2 }
		};
	}

	MXmlErrorType SetXMLError(ZString errorCode, ZString errorLineNumber, ZString errorColumnNumber, ZString errorText, ZString wrongValue, ZString location)
	{
		var error = new MXmlErrorType();
		error.ErrorCode = errorCode;
		error.ErrorLineNumber = errorLineNumber;
		error.ErrorColumnNumber = errorColumnNumber;
		error.ErrorText = errorText;
		error.OriginalAttributeValue = wrongValue;
		error.ErrorPointer = location;
		return error;
	}

	protected Cd917AType SetXMLErrorResponseData()
	{
		var error1 = SetXMLError("18", "14", "34", "Se esperaba nodo wrong1 security y ha venido wrong2 totalAmountInvoiced", "Wrong value", "location");
		var error2 = SetXMLError("25", "15", "37", "desc", ZString.Empty, ZString.Empty);
		return new Cd917AType()
		{
			XmlError = new Collection<MXmlErrorType> { error1, error2 }
		};
	}

	protected abstract TPrettyFormatter GetNewPrettyFormatter(TResponse response);

	protected abstract TResponse ResponseData();

	protected abstract TResponse ErrorResponseData(bool isFunctionalError);

	protected abstract ZString ExpectedMessageDetailsAccepted { get; }
}
