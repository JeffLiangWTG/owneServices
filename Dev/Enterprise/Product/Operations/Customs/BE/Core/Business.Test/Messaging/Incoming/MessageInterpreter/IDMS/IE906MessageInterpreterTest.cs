using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(IE906MessageInterpreter))]
sealed class IE906MessageInterpreterTest : MessageInterpreterTestCase<IE906MessageInterpreter, IIE906DataProvider>
{
	public override void TestInterpret()
	{
		var mockIDMSFunctionError = new Mock<IIDMSFunctionalError>();
		mockIDMSFunctionError.Setup(f => f.SequenceNumber).Returns("01");
		mockIDMSFunctionError.Setup(f => f.ErrorCode).Returns("13");
		mockIDMSFunctionError.Setup(f => f.ErrorPointer).Returns("EP1");
		mockIDMSFunctionError.Setup(f => f.ErrorReason).Returns("nb");
		mockIDMSFunctionError.Setup(f => f.OriginalAttributeValue).Returns("O1");

		var mockIDMSFunctionError2 = new Mock<IIDMSFunctionalError>();
		mockIDMSFunctionError2.Setup(f => f.SequenceNumber).Returns("02");
		mockIDMSFunctionError2.Setup(f => f.ErrorCode).Returns("14");
		mockIDMSFunctionError2.Setup(f => f.ErrorPointer).Returns("EP2");
		mockIDMSFunctionError2.Setup(f => f.ErrorReason).Returns("nb2");
		mockIDMSFunctionError2.Setup(f => f.OriginalAttributeValue).Returns("O2");

		var mockIE906 = new Mock<IIE906DataProvider>();
		mockIE906.Setup(m => m.FunctionalErrors).Returns(new List<IIDMSFunctionalError> { mockIDMSFunctionError.Object, mockIDMSFunctionError2.Object });
		var result = Interpreter.Interpret(mockIE906.Object, null);
		AssertEquals("New status: XML error. Xml gives technical errors.</br></br>Error SequenceNumber: 01</br>Error Pointer: EP1</br>Error Code: 13</br>Error Reason: nb</br>Value associated with error: O1</br></br>Error SequenceNumber: 02</br>Error Pointer: EP2</br>Error Code: 14</br>Error Reason: nb2</br>Value associated with error: O2", result);
	}
}
