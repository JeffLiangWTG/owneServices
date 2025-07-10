using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(ImportPresentationMessageSender))]
sealed class ImportPresentationMessageSenderTest : NLMessageSenderTest<ImportPresentationMessageSender>
{
	public new void TestSend_WcoType()
	{
		// Currently, we don't have a specific messagebuilder for this Message.
		Assert(true);
	}

	protected override ImportPresentationMessageSender GetNLMessageSender(JobDeclarationMessageSendingObject messageSendingObject)
	{
		return new ImportPresentationMessageSender(messageSendingObject);
	}

	protected override ZString ExpectedMessageSubType => ImportSendMessageTypes.Codes.PRE;

	protected override ZString ExpectedWcoType => NLConstants.WCoTypeCodes.ImportPresentation;

	protected override ZString MessageType => ImportSendMessageTypes.Codes.PRE;
}
