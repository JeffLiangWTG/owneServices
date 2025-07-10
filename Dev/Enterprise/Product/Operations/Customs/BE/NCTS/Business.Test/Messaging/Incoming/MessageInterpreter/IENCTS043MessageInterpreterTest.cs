using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(IENCTS043MessageInterpreter))]
	sealed class IENCTS043MessageInterpreterTest : MessageInterpreterTestCase<IENCTS043MessageInterpreter, ICC043CAndIENCTS043CDataProvider>
	{
		public override void TestInterpret()
		{
			var mockIENCTS043 = new Mock<ICC043CAndIENCTS043CDataProvider>();
			var result = Interpreter.Interpret(mockIENCTS043.Object, null);
			AssertContains("Unloading Permission<br />Status granted on:", result);
		}
	}
}
