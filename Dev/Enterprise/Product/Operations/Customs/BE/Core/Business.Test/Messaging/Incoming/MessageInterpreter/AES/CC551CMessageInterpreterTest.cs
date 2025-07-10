using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC551CMessageInterpreter))]
sealed class CC551CMessageInterpreterTest : MessageInterpreterTestCase<CC551CMessageInterpreter, ICC551CDataProvider>
{
	public override void TestInterpret()
	{
		var mockCC551C = new Mock<ICC551CDataProvider>();
		mockCC551C.Setup(m => m.OtherThingsToReport).Returns("A massive amount of text");
		mockCC551C.Setup(m => m.ControlDate).Returns(new DateTime(2022, 4, 1, 12, 34, 56));
		mockCC551C.Setup(m => m.ControlText).Returns("Even more text");
		var result = Interpreter.Interpret(mockCC551C.Object, null);
		AssertEquals("The goods are not Released by customs.</br>Following was reported: A massive amount of text</br>Control was done on 01-Apr-22 12:34:56</br>Customs reported: Even more text</br>Status is set to DNR (Declaration No Release)", result);
	}
}
