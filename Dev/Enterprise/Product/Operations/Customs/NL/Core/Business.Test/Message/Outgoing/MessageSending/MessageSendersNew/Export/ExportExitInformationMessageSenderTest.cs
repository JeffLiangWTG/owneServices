using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(ExportExitInformationMessageSender))]
sealed class ExportExitInformationMessageSenderTest : NLMessageSenderTest<ExportExitInformationMessageSender>
{
	public new void TestSend_WcoType()
	{
		// Currently, we don't have a specific messagebuilder for this Message.
		Assert(true);
	}

	public void TestSend_PostSendProcess()
	{
		messageSender.Send();

		AssertEquals(CustomsEntryPhaseStatusList.Codes._583, entryHeader.CH_PhaseStatus);
	}

	protected override ZString ExpectedMessageSubType => ExportSendMessageTypes.Codes.EXT;

	protected override ZString ExpectedWcoType => NLConstants.WCoTypeCodes.InformationOnNonExitedExport;

	protected override ZString MessageType => ExportSendMessageTypes.Codes.EXT;

	protected override ExportExitInformationMessageSender GetNLMessageSender(JobDeclarationMessageSendingObject messageSendingObject)
	{
		return new ExportExitInformationMessageSender(messageSendingObject);
	}
}

