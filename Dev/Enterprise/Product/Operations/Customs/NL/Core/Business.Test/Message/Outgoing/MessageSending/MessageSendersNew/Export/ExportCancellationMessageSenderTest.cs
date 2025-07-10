using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(ExportCancellationMessageSender))]
sealed class ExportCancellationMessageSenderTest : NLMessageSenderTest<ExportCancellationMessageSender>
{
	public void TestSend_PostSendProcess()
	{
		messageSender.Send();
		var message = entryHeader.Messages.Cast<NLEDIMessage>().Single(x => x.EM_Status == EDIMessageStatusList.Codes.Queued);
		AssertEquals($"{NLConstants.StatementTypes.Customs}|CHOU", message.CustomsMessageRemarks);
	}

	protected override ExportCancellationMessageSender GetNLMessageSender(JobDeclarationMessageSendingObject messageSendingObject)
	{
		messageSendingObject.ReasonForInvalidation = "CHOU";
		return new ExportCancellationMessageSender(messageSendingObject);
	}

	protected override ZString ExpectedMessageSubType => ExportSendMessageTypes.Codes.CAN;

	protected override ZString ExpectedWcoType => NLConstants.WCoTypeCodes.ExportInvalidationRequest;

	protected override ZString MessageType => ExportSendMessageTypes.Codes.CAN;
}

