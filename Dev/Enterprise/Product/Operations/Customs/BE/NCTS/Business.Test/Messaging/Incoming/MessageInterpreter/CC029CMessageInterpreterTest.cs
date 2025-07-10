using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC029CMessageInterpreter))]
	sealed class CC029CMessageInterpreterTest : MessageInterpreterTestCase<CC029CMessageInterpreter, ICC029CDataProvider>
	{
		public override void TestInterpret()
		{
			var mockCC029C = new Mock<ICC029CDataProvider>();
			mockCC029C.Setup(m => m.ReleaseDate).Returns(new DateTime(1994, 2, 1));
			mockCC029C.Setup(m => m.DeclarationAcceptanceDate).Returns(new DateTime(1994, 2, 2));
			var result = Interpreter.Interpret(mockCC029C.Object, null);
			AssertEquals("New detailed status: Goods Released for Transit at Departure.</br>Status granted on 01/02/1994</br>Acceptance Date 02/02/1994", result);
		}
	}
}
