using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class NLMessageSenderBaseOnlyTest : NLMessageSenderTest<NLMessageSenderForTest>
{
	public void TestSend()
	{
		entryInstruction.CEI_Style = DeclarationTypeList.Codes.B2;

		using (NLCustomsRegistry.Instance.IsNLTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		{
			messageSender.Send();
			var message = entryHeader.Messages.Cast<NLEDIMessage>().Single(x => x.EM_Status == EDIMessageStatusList.Codes.Queued);

			CombineAssertions(() =>
			{
				AssertEquals("EM_MessageType", NLEDIMessageTypes.Codes.DMS, message.EM_MessageType);
				AssertEquals("EM_IsTestMessage", ZBool.True, message.EM_IsTestMessage);
				AssertEquals("EM_ApplicationReference", "", message.EM_ApplicationReference);
				AssertSame("EM_LinkedObject", entryHeader, message.EM_LinkedObject);

				Factory.Save();
				AssertContains("CommunicationsAgreementId populated with MessageNumber in XML", "<CommunicationsAgreementID>00000000000001</CommunicationsAgreementID>", message.EM_MessageText);

				AssertContains("TODO:EM_MessageText-B2 message", "<MetaData xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"urn:wco:datamodel:WCO:DMS.Declaration:1\">", message.EM_MessageText);
				AssertNotContains("EM_MessageText-RemoveEmptyXmlElements", "<ApplicationReferenceID></ApplicationReferenceID>", message.EM_MessageText);

				AssertContains("EM_MessageInterpretation", "<font size='2' face='Courier New'>", message.EM_MessageInterpretation);
				AssertEquals("CH_Status", NLConstants.StatusNew.SentToCustoms, entryHeader.CH_Status);
			});
		}
	}

	protected override NLMessageSenderForTest GetNLMessageSender(JobDeclarationMessageSendingObject messageSendingObject)
	{
		return new NLMessageSenderForTest(messageSendingObject);
	}

	protected override ZString ExpectedMessageSubType => "XXX";

	protected override ZString ExpectedWcoType => "YYY";

	protected override ZString MessageType => ExportSendMessageTypes.Codes.DEC;
}

[TestsSubclassesOf(typeof(NLMessageSender))]
abstract class NLMessageSenderTest<TMessageSender> : TestCaseWithFactory where TMessageSender : NLMessageSender
{
	public void TestSend_MessageSubType()
	{
		messageSender.Send();
		var message = entryHeader.Messages.Cast<NLEDIMessage>().Single(x => x.EM_Status == EDIMessageStatusList.Codes.Queued);
		AssertEquals(ExpectedMessageSubType, message.EM_MessageSubType);
	}

	public void TestSend_WcoType()
	{
		messageSender.Send();
		var message = entryHeader.Messages.Cast<NLEDIMessage>().Single(x => x.EM_Status == EDIMessageStatusList.Codes.Queued);
		AssertContains($"<WCOTypeCode>{ExpectedWcoType}</WCOTypeCode>", message.EM_MessageText);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var messageSendingObject = new JobDeclarationMessageSendingObject(entryHeader);
		messageSendingObject.MessageType = MessageType;
		messageSender = GetNLMessageSender(messageSendingObject);
	}
	protected JobDeclaration declaration;
	protected CusEntryHeader entryHeader;
	protected CusEntryInstruction entryInstruction;
	protected TMessageSender messageSender;

	protected abstract ZString ExpectedMessageSubType { get; }

	protected abstract ZString ExpectedWcoType { get; }

	protected abstract ZString MessageType { get; }

	protected abstract TMessageSender GetNLMessageSender(JobDeclarationMessageSendingObject messageSendingObject);
}

sealed class NLMessageSenderForTest : NLMessageSender
{
	public NLMessageSenderForTest(JobDeclarationMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	protected override ZString MessageSubType => "XXX";

	protected override ZString WcoType => "YYY";
}
