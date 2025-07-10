using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(ExportPresentationMessageSender))]
sealed class ExportPresentationMessageSenderTest : NLMessageSenderTest<ExportPresentationMessageSender>
{
	public new void TestSend_WcoType()
	{
		// Currently, we don't have a specific messagebuilder for this Message.
		Assert(true);
	}

	public void TestSend_PostSendProcess()
	{
		messageSender.Send();

		AssertEquals(CustomsEntryPhaseStatusList.Codes._511, entryHeader.CH_PhaseStatus);
		AssertEquals(NLConstants.EntryStatusNew.PreLodged, entryHeader.CH_EntryStatus);
	}

	protected override ZString ExpectedMessageSubType => ExportSendMessageTypes.Codes.PRE;

	protected override ZString ExpectedWcoType => NLConstants.WCoTypeCodes.ExportPresentation;

	protected override ZString MessageType => ExportSendMessageTypes.Codes.PRE;

	protected override ExportPresentationMessageSender GetNLMessageSender(JobDeclarationMessageSendingObject messageSendingObject)
	{
		return new ExportPresentationMessageSender(messageSendingObject);
	}
}
