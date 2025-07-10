using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestsSubclassesOf(typeof(BaseMessageInterpreter<>))]
public abstract class MessageInterpreterTestCase<TTestClass, TInboundProvider> : TestCaseWithFactory
	where TInboundProvider : class, IInboundProvider
	where TTestClass : BaseMessageInterpreter<TInboundProvider>, new()
{
	public abstract void TestInterpret();

	protected TTestClass Interpreter => interpreter ?? (interpreter = new TTestClass());

	TTestClass interpreter;
}
