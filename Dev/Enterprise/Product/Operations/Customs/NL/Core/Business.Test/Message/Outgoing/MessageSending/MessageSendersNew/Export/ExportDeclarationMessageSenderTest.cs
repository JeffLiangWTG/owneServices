using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(ExportDeclarationMessageSender))]
sealed class ExportDeclarationMessageSenderTest : NLMessageSenderTest<ExportDeclarationMessageSender>
{
	public void TestSend_PostSendProcess()
	{
		messageSender.Send();

		AssertEquals(CustomsEntryPhaseStatusList.Codes._515, entryHeader.CH_PhaseStatus);
	}

	protected override ExportDeclarationMessageSender GetNLMessageSender(JobDeclarationMessageSendingObject messageSendingObject)
	{
		return new ExportDeclarationMessageSender(messageSendingObject);
	}

	protected override ZString ExpectedMessageSubType => ExportSendMessageTypes.Codes.DEC;

	protected override ZString ExpectedWcoType => NLConstants.WCoTypeCodes.ExportDeclaration;

	protected override ZString MessageType => ExportSendMessageTypes.Codes.DEC;
}
