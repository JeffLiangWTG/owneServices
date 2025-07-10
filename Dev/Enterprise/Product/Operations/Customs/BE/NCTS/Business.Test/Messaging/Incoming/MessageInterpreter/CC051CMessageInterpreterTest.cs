using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC051CMessageInterpreter))]
	sealed class CC051CMessageInterpreterTest : MessageInterpreterTestCase<CC051CMessageInterpreter, ICC051CDataProvider>
	{
		public override void TestInterpret()
		{
			var mockCC051C = new Mock<ICC051CDataProvider>();
			mockCC051C.Setup(m => m.NoReleaseMotivationCode).Returns(NCTS5NoReleaseMotivation.Codes.G2);
			mockCC051C.Setup(m => m.NoReleaseMotivationText).Returns("a lot of text. blablablablabla");
			var result = Interpreter.Interpret(mockCC051C.Object, null);
			AssertEquals("Declaration is NOT RELEASED FOR TRANSIT AT DEPARTURE.</br>Motivation code: G2 Cash guarantee not provided</br>a lot of text. blablablablabla", result);
		}
	}
}
