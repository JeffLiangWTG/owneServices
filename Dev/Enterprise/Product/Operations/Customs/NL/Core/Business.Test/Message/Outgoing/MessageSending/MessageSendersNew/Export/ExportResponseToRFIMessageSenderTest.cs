using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(ExportResponseToRFIMessageSender))]
sealed class ExportResponseToRFIMessageSenderTest : NLMessageSenderTest<ExportResponseToRFIMessageSender>
{
	protected override ZString ExpectedMessageSubType => ExportSendMessageTypes.Codes.CRE;

	protected override ZString ExpectedWcoType => NLConstants.WCoTypeCodes.ControlFindingsInformationExport;

	protected override ZString MessageType => ExportSendMessageTypes.Codes.CRE;

	protected override ExportResponseToRFIMessageSender GetNLMessageSender(JobDeclarationMessageSendingObject messageSendingObject)
	{
		return new ExportResponseToRFIMessageSender(messageSendingObject);
	}
}
