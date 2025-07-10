using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(ExportFallbackMessageSender))]
sealed class ExportFallbackMessageSenderTest : NLMessageSenderTest<ExportFallbackMessageSender>
{
	public void TestSend_PostSendProcess()
	{
		messageSender.Send();
		var message = entryHeader.Messages.Cast<NLEDIMessage>().Single(x => x.EM_Status == EDIMessageStatusList.Codes.Queued);
		AssertEquals(ZDateTime.MaxSmallDateTime, message.EM_HeldUntilDate);
	}

	[TestDate(2025, 05, 14, 17, 39, 00)]
	public void TestSend_SetMessageTextAndInterpretation()
	{
		entryHeader.DMSFallbackIsActive = true;
		messageSender.Send();
		var message = entryHeader.Messages.Cast<NLEDIMessage>().Single(x => x.EM_Status == EDIMessageStatusList.Codes.Queued);
		Assert(message.EM_MessageText.Contains("<AcceptanceDateTime formatCode=\"102\">20250514</AcceptanceDateTime>"));
		AssertEquals(new ZDateTime(2025, 05, 14, 17, 39, 00), entryHeader.FallbackEntryNumberIssueDate);
	}

	protected override ExportFallbackMessageSender GetNLMessageSender(JobDeclarationMessageSendingObject messageSendingObject)
	{
		return new ExportFallbackMessageSender(messageSendingObject);
	}

	protected override ZString ExpectedMessageSubType => ExportSendMessageTypes.Codes.FBK;

	protected override ZString ExpectedWcoType => NLConstants.WCoTypeCodes.ExportDeclaration;

	protected override ZString MessageType => ExportSendMessageTypes.Codes.FBK;
}

