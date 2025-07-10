using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC140CMessageInterpreter))]
	sealed class CC140CMessageInterpreterTest : MessageInterpreterTestCase<CC140CMessageInterpreter, ICC140CDataProvider>
	{
		public override void TestInterpret()
		{
			var mockCC140C = new Mock<ICC140CDataProvider>();
			mockCC140C.Setup(m => m.RequestOnNonArrivedMovementDate).Returns(new DateTime(2022, 4, 1));
			mockCC140C.Setup(m => m.LimitForResponseDate).Returns(new DateTime(2022, 4, 2));
			var result = Interpreter.Interpret(mockCC140C.Object, null);
			AssertEquals("New Customs Status: 'Request on Non-Arrived Movement'</br>Status granted on 01/04/2022</br>Response on the Request for info on Non-Arrived Movement, must be sent to customs before 02/04/2022", result);
		}
	}
}
