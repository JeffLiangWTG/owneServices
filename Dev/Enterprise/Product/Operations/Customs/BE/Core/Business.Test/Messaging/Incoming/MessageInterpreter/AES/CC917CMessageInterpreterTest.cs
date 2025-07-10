using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC917CMessageInterpreter))]
sealed class CC917CMessageInterpreterTest : MessageInterpreterTestCase<CC917CMessageInterpreter, ICC917CDataProvider>
{
	public override void TestInterpret()
	{
		var xmlError = new List<XMLErrorXmlProvider>
		{
			new XMLErrorXmlProvider
			{
				ErrorLineNumber = "1", ErrorColumnNumber = "2", ErrorPointer = "EP1", ErrorCode = "13", ErrorText = "M1", OriginalAttributeValue = "O1"
			},
			new XMLErrorXmlProvider
			{
				ErrorLineNumber = "3", ErrorColumnNumber = "4", ErrorPointer = "EP2", ErrorCode = "12", ErrorText = "M2", OriginalAttributeValue = "O2"
			},
		};
		var mockCC917C = new Mock<ICC917CDataProvider>();
		mockCC917C.Setup(m => m.XMLErrorList).Returns(xmlError);
		var result = Interpreter.Interpret(mockCC917C.Object, null);
		AssertEquals("New transaction status: XML error. Xml gives xsd errors.</br></br>Error Line: 1</br>Error Column: 2</br>Error Pointer: EP1</br>Error Code: 13 Missing</br>Error Text: M1</br>Original value: O1</br></br>Error Line: 3</br>Error Column: 4</br>Error Pointer: EP2</br>Error Code: 12 Incorrect enumeration</br>Error Text: M2</br>Original value: O2", result);
	}
}
