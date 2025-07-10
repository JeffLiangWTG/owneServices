using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class InboundMessageCreatorFactoryTest : TestCaseWithFactory
{
	public void TestGetNewThrowsException()
	{
		AssertExceptionThrown<ArgumentNullException>(() => InboundMessageCreatorFactory.GetNew(null));
	}

	public void TestGetNewReturnsGenericCreator()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = "ITM";
		AssertType<InboundMessageCreator>(InboundMessageCreatorFactory.GetNew(interchange));
	}

	public void TestGetNewReturnsExitVerificationCreator()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_InterchangeType = "MRN";
		AssertType<ExitVerificationInboundMessageCreator>(InboundMessageCreatorFactory.GetNew(interchange));
	}

	public void TestGetNewReturnXtMessageCreator()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = "ITH";
		AssertType<XTradeInboundMessageCreator>(InboundMessageCreatorFactory.GetNew(interchange));
	}
}
