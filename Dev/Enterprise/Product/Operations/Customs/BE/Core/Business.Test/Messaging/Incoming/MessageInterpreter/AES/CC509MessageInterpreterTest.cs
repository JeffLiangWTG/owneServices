using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC509CMessageInterpreter))]
sealed class CC509CMessageInterpreterTest : MessageInterpreterTestCase<CC509CMessageInterpreter, ICC509CDataProvider>
{
	public override void TestInterpret()
	{
		var mockProvider = new Mock<ICC509CDataProvider>();
		mockProvider.CallBase = true;
		mockProvider.Setup(m => m.InvalidationDecisionDateAndTime).Returns(new DateTime(2023, 1, 13, 11, 54, 38));
		var result = Interpreter.Interpret(mockProvider.Object, null);
		AssertEquals("Declaration is invalidated/cancelled on 13-Jan-23 11:54:38<br />Status is set to CAN<br />", result);
	}
}
