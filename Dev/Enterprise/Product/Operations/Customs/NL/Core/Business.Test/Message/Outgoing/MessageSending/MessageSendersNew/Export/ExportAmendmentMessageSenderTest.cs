using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using NUnit.Framework;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(ExportAmendmentMessageSender))]
sealed class ExportAmendmentMessageSenderTest : NLMessageSenderTest<ExportAmendmentMessageSender>
{
	public void TestSend_PostSendProcess()
	{
		messageSender.Send();

		AssertEquals(CustomsEntryPhaseStatusList.Codes._513, entryHeader.CH_PhaseStatus);
	}

	protected override ExportAmendmentMessageSender GetNLMessageSender(JobDeclarationMessageSendingObject messageSendingObject)
	{
		entryHeader.CH_EntryStatus = EntryStatus.AdvanceDeclarationReceived;
		WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.DEC, "EXA");

		return new ExportAmendmentMessageSender(messageSendingObject);
	}

	protected override ZString ExpectedMessageSubType => ExportSendMessageTypes.Codes.AMD;

	protected override ZString ExpectedWcoType => NLConstants.WCoTypeCodes.ExportAmendment;

	protected override ZString MessageType => ExportSendMessageTypes.Codes.AMD;
}
