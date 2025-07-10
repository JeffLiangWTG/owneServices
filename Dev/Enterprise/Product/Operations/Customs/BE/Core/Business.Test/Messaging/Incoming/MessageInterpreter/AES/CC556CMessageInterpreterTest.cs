using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC556CMessageInterpreter))]
sealed class CC556CMessageInterpreterTest : MessageInterpreterTestCase<CC556CMessageInterpreter, ICC556CDataProvider>
{
	public override void TestInterpret()
	{
		var mockCC556C = new Mock<ICC556CDataProvider>();
		mockCC556C.Setup(m => m.BusinessRejectionType).Returns("22BEE00000000012J1");
		mockCC556C.Setup(m => m.RejectionDateAndTime).Returns(new DateTime(2022, 4, 1, 12, 34, 56));
		mockCC556C.Setup(m => m.RejectionCode).Returns("4");
		mockCC556C.Setup(m => m.RejectionReason).Returns("invalid value transport type");
		var mockFunctionError = new Mock<IFunctionalError>();
		mockFunctionError.Setup(m => m.ErrorCode).Returns("12");
		mockFunctionError.Setup(m => m.ErrorReason).Returns("Type of transport does not exist");
		mockFunctionError.Setup(m => m.ErrorPointer).Returns("cc515c.DepartureTransportMeans(2).typeOfIdentification");
		mockFunctionError.Setup(m => m.OriginalAttributeValue).Returns("32");
		mockCC556C.Setup(m => m.FunctionalErrorList).Returns(new List<IFunctionalError> { mockFunctionError.Object });
		var result = Interpreter.Interpret(mockCC556C.Object, null);
		AssertEquals("Declaration received an <span style='color:red'> error </span> for type (22BEE00000000012J1) on 01/04/2022 12:34:56<br />Reason:(4 - Other reasons) invalid value transport type<br /><table cellpadding=\"3\" cellspacing=\"0\" width=\"100%\" border=\"1\" style=\"font-size: 14px; border: 1px solid gray; border-collapse: collapse; font-family: Arial, sans-serif;\"><tr><td width=\"300px\">Functional error code:</td><td>(12 - Code list violation (incorrect enumeration))</td></tr><tr><td width=\"300px\">Reason:</td><td>Type of transport does not exist</td></tr><tr><td width=\"300px\">Attribute:</td><td>cc515c.DepartureTransportMeans(2).typeOfIdentification</td></tr><tr><td width=\"300px\">Element in declaration contains now the value:</td><td>32</td></tr></table><br />", result);
	}
}
