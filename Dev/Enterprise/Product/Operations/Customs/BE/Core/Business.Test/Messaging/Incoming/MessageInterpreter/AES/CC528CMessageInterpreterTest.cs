using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC528CMessageInterpreter))]
sealed class CC528CMessageInterpreterTest : MessageInterpreterTestCase<CC528CMessageInterpreter, ICC528CDataProvider>
{
	public override void TestInterpret()
	{
		var mockCC528C = new Mock<ICC528CDataProvider>();
		mockCC528C.Setup(m => m.DeclarationAcceptanceDate).Returns(new DateTime(2022, 4, 1, 12, 34, 56));
		var result = Interpreter.Interpret(mockCC528C.Object, null);
		AssertEquals("Declaration is accepted on 01-Apr-22 12:34:56</br>Status is changed to MRN", result);
	}
}
