using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC043CMessageInterpreter))]
	sealed class CC043CMessageInterpreterTest : MessageInterpreterTestCase<CC043CMessageInterpreter, ICC043CDataProvider>
	{
		public override void TestInterpret()
		{
			var mockCC043C = new Mock<ICC043CDataProvider>();
			var result = Interpreter.Interpret(mockCC043C.Object, null);
			AssertContains("Unloading Permission: Started <br />Status granted on:", result);
		}

		public void TestInterpretWithContinueUnloadingFalse()
		{
			var mockCC043C = new Mock<ICC043CDataProvider>();
			mockCC043C.Setup(m => m.ContinueUnloading).Returns(false);
			var result = Interpreter.Interpret(mockCC043C.Object, null);
			AssertContains("Unloading Permission: Final <br />Status granted on:", result);
		}

		public void TestInterpretWithContinueUnloadingTrue()
		{
			var mockCC043C = new Mock<ICC043CDataProvider>();
			mockCC043C.Setup(m => m.ContinueUnloading).Returns(true);
			var result = Interpreter.Interpret(mockCC043C.Object, null);
			AssertContains("Unloading Permission: Continue <br />Status granted on:", result);
		}
	}
}
