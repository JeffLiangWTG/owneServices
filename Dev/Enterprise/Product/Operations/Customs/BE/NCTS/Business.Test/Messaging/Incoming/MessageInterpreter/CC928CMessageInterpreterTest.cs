using CargoWise.Customs.BE.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.BE.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC928CMessageInterpreter))]
	sealed class CC928CMessageInterpreterTest : MessageInterpreterTestCase<CC928CMessageInterpreter, ICC928CDataProvider>
	{
		public override void TestInterpret()
		{
			var mockCC928C = new Mock<ICC928CDataProvider>();
			mockCC928C.Setup(m => m.CorrelationIdentifier).Returns("123");
			var result = Interpreter.Interpret(mockCC928C.Object, null);
			AssertEquals("New declaration status: 'Declaration Accepted'</br>Correlation id: 123", result);
		}
	}
}
