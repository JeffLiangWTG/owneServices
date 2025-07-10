using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestsSubclassesOf(typeof(BaseMessageInterpreter<>))]
public abstract class MessageInterpreterTestCase<TTestClass, TInboundProvider> : TestCaseWithFactory
	where TInboundProvider : class, IIncomingDataProvider
	where TTestClass : BaseMessageInterpreter<TInboundProvider>, new()
{
	public abstract string ExpectedMessageInterpretation { get; }

	protected TTestClass Interpreter => interpreter ?? (interpreter = new TTestClass());

	protected Mock<TInboundProvider> DataProviderMock => dataProviderMock ?? (dataProviderMock = new Mock<TInboundProvider>());

	protected Mock<NLEDIMessage> MessageMock => messageMock ?? (messageMock = Factory.NewMoq<NLEDIMessage>());

	[TestDate(2024, 04, 22, 09, 10, 10)]
	public void TestInterpret()
	{
		var message = MessageMock.Object;
		Factory.Save();
		AssertEquals(ExpectedMessageInterpretation, Interpreter.Interpret(DataProviderMock.Object, message));
	}

	TTestClass interpreter;

	Mock<TInboundProvider> dataProviderMock;

	Mock<NLEDIMessage> messageMock;
}
