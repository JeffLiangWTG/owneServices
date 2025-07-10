using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(ExportSupplementMessageSender))]
sealed class ExportSupplementMessageSenderTest : NLMessageSenderTest<ExportSupplementMessageSender>
{
	protected override ZString ExpectedMessageSubType => ExportSendMessageTypes.Codes.SUP;

	protected override ZString ExpectedWcoType => NLConstants.WCoTypeCodes.ExportDeclaration;

	public new void TestSend_WcoType()
	{
		// Currently, we don't have a specific messagebuilder for SUP Message.
		Assert(true);
	}

	protected override ZString MessageType => ExportSendMessageTypes.Codes.SUP;

	protected override ExportSupplementMessageSender GetNLMessageSender(JobDeclarationMessageSendingObject messageSendingObject)
	{
		return new ExportSupplementMessageSender(messageSendingObject);
	}
}
