using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IN.Business.Testing
{
	abstract class MessageInterpreterAbstractTest<T> : TestCaseWithFactory where T : IMessageInterpreter
	{
		public void TestGetMessageInterpretation()
		{
			AssertEquals(ExpectMessageInterpretation, CreateMessageInterpreterForTest().GetMessageInterpretation());
		}

		protected abstract T CreateMessageInterpreterForTest();

		protected abstract ZString ExpectMessageInterpretation { get; }
	}
}
