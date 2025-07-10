using System;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class UnifiedDeclarationResponseMessageTest : TestCase
{
	public void TestNewThrowsExceptionWhenArgumentIsEmpty()
	{
		AssertExceptionThrown<ArgumentException>("When parameter is empty", () => UnifiedDeclarationResponseMessage.New(ZString.Empty));
	}

	public void TestNewThrowsExceptionWhenArgumentHasInvalidLength()
	{
		AssertExceptionThrown<ArgumentException>("When parameter length is not valid", () => UnifiedDeclarationResponseMessage.New(new ZString('0', 27)));
		AssertNoExceptionThrown("When parameter length is valid", () => UnifiedDeclarationResponseMessage.New(new ZString('0', 28)));
	}

	public void TestNewForImportResponses()
	{
		AssertResponseMessageType<SadPositiveResponseMessage>(IrispConstants.MessageTypes.IM, IrispConstants.OperationResults.P);
		AssertResponseMessageType<SadNegativeResponseMessage>(IrispConstants.MessageTypes.IM, IrispConstants.OperationResults.N);
	}

	public void TestNewForExportResponses()
	{
		AssertResponseMessageType<SadPositiveResponseMessage>(IrispConstants.MessageTypes.ET, IrispConstants.OperationResults.P);
		AssertResponseMessageType<SadNegativeResponseMessage>(IrispConstants.MessageTypes.ET, IrispConstants.OperationResults.N);
	}

	public void TestNewForNbResponses()
	{
		AssertResponseMessageType<SadNbPositiveResponseMessage>(IrispConstants.MessageTypes.NB, IrispConstants.OperationResults.P);
		AssertResponseMessageType<SadNbNegativeResponseMessage>(IrispConstants.MessageTypes.NB, IrispConstants.OperationResults.N);
	}

	public void TestNewForUnrecognizedResponses()
	{
		AssertNull("When message code and operation result are unrecognized", UnifiedDeclarationResponseMessage.New(GetResponseMessageBlock("XY", "A")));
	}

	void AssertResponseMessageType<TResult>(ZString messageCode, ZString operationResult)
	{
		var responseMessage = UnifiedDeclarationResponseMessage.New(GetResponseMessageBlock(messageCode, operationResult));
		AssertNotNull(nameof(responseMessage), responseMessage);
		AssertType<TResult>($"{nameof(responseMessage)} type", responseMessage);
	}

	ZString GetResponseMessageBlock(ZString messageCode, ZString operationResult)
	{
		return $@"R{messageCode}          64660201371101{operationResult}";
	}
}
