using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business.Testing;

class NLInboundInterchangeProcessorTest : TestCaseWithFactory
{
	public void TestApplicationCodes()
	{
		var interchangeProc = new NLInboundInterchangeProcessorForTest(GetNewLoggerForTesting());
		AssertEquals(EDIMessage.ApplicationCodes.NLCustoms, interchangeProc.ApplicationCodesExposed[0]);
	}

	public void TestTypeOfInterchangeToCreate()
	{
		var interchangeProc = new NLInboundInterchangeProcessorForTest(GetNewLoggerForTesting());
		Assert(interchangeProc.TypeOfInterchangeToCreateExposed() == typeof(EDIInterchange));
	}

	public void TestIsNoBranchFilter()
	{
		var interchangeProc = new NLInboundInterchangeProcessorForTest(GetNewLoggerForTesting());
		AssertEquals(true, interchangeProc.IsNoBranchFilterExposed);
	}
	public void TestSupportEnvironmentSwitch()
	{
		var interchangeProc = new NLInboundInterchangeProcessorForTest(GetNewLoggerForTesting());
		AssertEquals(true, interchangeProc.SupportEnvironmentSwitchExposed);
	}

	public void TestGetMessageCreator()
	{
		var interchangeProc = new NLInboundInterchangeProcessorForTest(GetNewLoggerForTesting());
		var interchange = Factory.New<EDIInterchange>();
		AssertType<InboundMessageCreator>(interchangeProc.GetMessageCreatorExposed(interchange));
	}
	internal protected static LoggingInformation GetNewLoggerForTesting() => new LoggingInformationForTesting();
}

class NLInboundInterchangeProcessorForTest : NLInboundInterchangeProcessor
{
	public NLInboundInterchangeProcessorForTest(LoggingInformation logger) : base(logger)
	{
	}

	public bool IsNoBranchFilterExposed => base.IsNoBranchFilter;
	public bool SupportEnvironmentSwitchExposed => base.SupportEnvironmentSwitch;
	public string[] ApplicationCodesExposed => base.ApplicationCodes;
	public Type TypeOfInterchangeToCreateExposed() => base.TypeOfInterchangeToCreate();
	public IInboundMessageCreator GetMessageCreatorExposed(EDIInterchange interchange) => base.GetMessageCreator(interchange);
}
