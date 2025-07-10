using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC182CMessageInterpreter))]
	sealed class CC182CMessageInterpreterTest : MessageInterpreterTestCase<CC182CMessageInterpreter, ICC182CDataProvider>
	{
		[ExpectNoExceptions]
		public override void TestInterpret()
		{
			var mockCC182C = new Mock<ICC182CDataProvider>();
			mockCC182C.Setup(x => x.IncidentDateAndTime).Returns(new DateTime(2022, 4, 1, 8, 34, 56));
			var result = Interpreter.Interpret(mockCC182C.Object, null);
			AssertEquals("Phase: FIN - Forwarded incident notification<br />Status granted on: 01/04/2022 08:34:56<br />", result);
		}
	}
}
