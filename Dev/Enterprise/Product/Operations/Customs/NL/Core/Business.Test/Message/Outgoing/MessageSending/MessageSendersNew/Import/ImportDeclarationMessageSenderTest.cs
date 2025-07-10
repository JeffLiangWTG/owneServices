using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using NUnit.Framework;
namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(ImportDeclarationMessageSender))]
sealed class ImportDeclarationMessageSenderTest : NLMessageSenderTest<ImportDeclarationMessageSender>
{
	protected override ImportDeclarationMessageSender GetNLMessageSender(JobDeclarationMessageSendingObject messageSendingObject)
	{
		return new ImportDeclarationMessageSender(messageSendingObject);
	}

	protected override ZString ExpectedMessageSubType => ImportSendMessageTypes.Codes.DEC;

	protected override ZString ExpectedWcoType => NLConstants.WCoTypeCodes.ImportDeclaration;

	protected override ZString MessageType => ImportSendMessageTypes.Codes.DEC;
}

