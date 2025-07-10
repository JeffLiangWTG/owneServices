using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business.Testing;

class InboundMessageCreatorFactoryTest : TestCaseWithFactory
{
	public void TestGetNewThrowsException()
	{
		AssertExceptionThrown<ArgumentNullException>(() => InboundMessageCreatorFactory.GetNew(logger, null));
	}

	public void TestGetNewReturnsNull()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = ApplicationCodeList.Codes.CHCustomsEdec;
		interchange.EI_InterchangeType = MessageTypeCodeList.Codes.MSG;
		AssertNull(InboundMessageCreatorFactory.GetNew(logger, interchange));
		AssertEquals("\tInterchange type not found: ApplicationCode=CHC InterchangeType=MSG", logger.UserLogStrings[0]);
	}

	public void TestGetNewInboundMessageCreator_CHC_IMP() => AssertInboundMessageCreatorType(ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.Import, typeof(EdecInboundMessageCreator));

	public void TestGetNewInboundMessageCreator_CHC_EXP() => AssertInboundMessageCreatorType(ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.Export, typeof(EdecInboundMessageCreator));

	public void TestGetNewInboundMessageCreator_CHC_EBD() => AssertInboundMessageCreatorType(ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.EBD, typeof(EbdInboundMessageCreator));

	public void TestGetNewInboundMessageCreator_CHC_EVV() => AssertInboundMessageCreatorType(ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.EVV, typeof(EvvInboundMessageCreator));

	public void TestGetNewInboundMessageCreator_CHC_ECM() => AssertInboundMessageCreatorType(ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.ECM, typeof(EComInboundMessageCreator));

	public void TestGetNewInboundMessageCreator_CHC_BOR() => AssertInboundMessageCreatorType(ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.BOR, typeof(EdecBordereauInboundMessageCreator));

	public void TestGetNewInboundMessageCreator_CHP_TRE() => AssertInboundMessageCreatorType(ApplicationCodeList.Codes.CHCustomsPassar, MessageTypeCodeList.Codes.TRE, typeof(TokenRefreshInboundMessageCreator));

	public void TestGetNewInboundMessageCreator_CHP_MSL() => AssertInboundMessageCreatorType(ApplicationCodeList.Codes.CHCustomsPassar, MessageTypeCodeList.Codes.MSL, typeof(UniversalEventInboundMessageCreator));

	public void TestGetNewInboundMessageCreator_CHP_MSG() => AssertInboundMessageCreatorType(ApplicationCodeList.Codes.CHCustomsPassar, MessageTypeCodeList.Codes.MSG, typeof(PassarGetMessageInboundMessageCreator));

	public void TestGetNewInboundMessageCreator_CHP_NCT() => AssertInboundMessageCreatorType(ApplicationCodeList.Codes.CHCustomsPassar, MessageTypeCodeList.Codes.PassarNcts, typeof(UniversalEventInboundMessageCreator));

	public void TestGetNewInboundMessageCreator_CHP_EXP() => AssertInboundMessageCreatorType(ApplicationCodeList.Codes.CHCustomsPassar, MessageTypeCodeList.Codes.Export, typeof(UniversalEventInboundMessageCreator));

	public void TestGetNewInboundMessageCreator_CHO_MSL() => AssertInboundMessageCreatorType(ApplicationCodeList.Codes.CHCustomsCharteraOutput, MessageTypeCodeList.Codes.MSL, typeof(UniversalEventInboundMessageCreator));

	public void TestGetNewInboundMessageCreator_CHO_MSG() => AssertInboundMessageCreatorType(ApplicationCodeList.Codes.CHCustomsCharteraOutput, MessageTypeCodeList.Codes.MSG, typeof(CharteraOutputGetMessageInboundMessageCreator));

	public void TestGetNewInboundMessageCreator_CHO_REQ() => AssertInboundMessageCreatorType(ApplicationCodeList.Codes.CHCustomsCharteraOutput, MessageTypeCodeList.Codes.REQ, typeof(UniversalEventInboundMessageCreator));

	public void TestGetNewInboundMessageCreator_CHC_XER() => AssertInboundMessageCreatorType(ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.XER, typeof(UniversalEventInboundMessageCreator));

	public void TestGetNewInboundMessageCreator_CHP_XER() => AssertInboundMessageCreatorType(ApplicationCodeList.Codes.CHCustomsPassar, MessageTypeCodeList.Codes.XER, typeof(UniversalEventInboundMessageCreator));

	public void TestGetNewInboundMessageCreator_CHO_XER() => AssertInboundMessageCreatorType(ApplicationCodeList.Codes.CHCustomsCharteraOutput, MessageTypeCodeList.Codes.XER, typeof(UniversalEventInboundMessageCreator));

	protected override void SetUp()
	{
		base.SetUp();
		logger = new LoggingInformation();
	}
	LoggingInformation logger;

	void AssertInboundMessageCreatorType(string applicationCode, string interchangeType, Type expectedType)
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = applicationCode;
		interchange.EI_InterchangeType = interchangeType;
		AssertType($"InboundMessageCreatorType for '{applicationCode}/{interchangeType}'", expectedType, InboundMessageCreatorFactory.GetNew(logger, interchange));
	}
}
