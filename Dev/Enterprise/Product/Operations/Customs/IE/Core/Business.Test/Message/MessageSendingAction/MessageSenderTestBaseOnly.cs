using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Customs.IE.MessageContracts.AES;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.AES;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	class MessageSenderTestBaseOnly : TestCaseWithFactory
	{
		public void TestAnnotationIsClearedWhenReadOnly()
		{
			(var company, var branch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(company);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var sendingAction = new CusEntryHeaderMessageSendingActionForTesting(entryHeader);

			sendingAction.MessageType = "AAA";
			sendingAction.Annotation = "An annotation";
			sendingAction.MessageType = "ZZZ";
			AssertEquals("sendingAction.AnnotationInfo.ReadOnly", true, sendingAction.AnnotationInfo.ReadOnly);
			AssertEquals("Annotation", ZString.Empty, sendingAction.Annotation);

			sendingAction.Annotation = "Another annotation";
			sendingAction.MessageType = "AAA";
			AssertEquals("sendingAction.AnnotationInfo.ReadOnly", false, sendingAction.AnnotationInfo.ReadOnly);
			AssertEquals("Annotation", "Another annotation", sendingAction.Annotation);
		}

		[TestDate(2022, 5, 16, 14, 45, 35, 345)]
		public void TestSend()
		{
			(var company, var branch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(company);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var sendingAction = new CusEntryHeaderMessageSendingActionForTesting(entryHeader);
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportOriginal;
			var sender = new MessageSenderForTesting(sendingAction);
			CombineAssertions("Created message should have correct values on all concerned fields. ", () =>
			{
				var message = sender.Send();
				var createdMessage = (AESOutboundEDIMessage)entryHeader.Messages.Single();
				AssertSame(message, createdMessage);
				AssertEquals("EM_MessageType", AESOutgoingMessageTypeList.Codes.ExportOriginal, createdMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, createdMessage.EM_Status);
				AssertXmlRoot(
					createdMessage.EM_MessageText,
					"CC515C",
					@"http://ecs.dgtaxud.ec");
				AssertEquals("EM_GP", companyCredential.PK, createdMessage.EM_GP);
				AssertEquals("EM_GB", branch.PK, createdMessage.EM_GB);
				AssertEquals("CH_Status", LogicalStatusList.Codes.Sent, entryHeader.CH_Status);
			});
		}

		static void AssertXmlRoot(string xml, string expectedLocalName, string expectedNamespace)
		{
			var doc = XDocument.Parse(xml);

			if (doc.Root == null)
			{
				throw new Exception("XML has no root element.");
			}

			if (doc.Root.Name.LocalName != expectedLocalName)
			{
				throw new Exception($"Expected root element name '{expectedLocalName}', but found '{doc.Root.Name.LocalName}'.");
			}

			if (doc.Root.Name.NamespaceName != expectedNamespace)
			{
				throw new Exception($"Expected root namespace '{expectedNamespace}', but found '{doc.Root.Name.NamespaceName}'.");
			}
		}

		public void TestSend_CallsPreSend()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			AssertNullOrEmpty("Before Send: EntryHeader.CH_BGMReference", entryHeader.CH_BGMReference);

			var sendingAction = new CusEntryHeaderMessageSendingActionForTesting(entryHeader);
			var sender = new MessageSenderForTestingPreSendCall(sendingAction);
			sender.Send();
			AssertEquals("After Send: EntryHeader.CH_BGMReference", "PRE_REF_123", entryHeader.CH_BGMReference);
		}
	}

	class MessageSenderForTesting : MessageSender
	{
		public MessageSenderForTesting(CusEntryHeaderMessageSendingActionForTesting sendingAction) : base(sendingAction) { }

		protected new CusEntryHeaderMessageSendingActionForTesting SendingAction => (CusEntryHeaderMessageSendingActionForTesting)base.SendingAction;
		protected override IXmlMessageBuilder CreateMessageBuilder(OutboundEDIMessage relatingMessage) => new IE515MessageBuilder(new IE515MessageProvider(SendingAction.EntryHeader));
		protected override OutboundEDIMessage CreateOutboundEDIMessage(BusinessObjectFactory factory) => factory.New<AESOutboundEDIMessage>();
	}

	class MessageSenderForTestingPreSendCall : MessageSenderForTesting
	{
		public MessageSenderForTestingPreSendCall(CusEntryHeaderMessageSendingActionForTesting sendingAction) : base(sendingAction)
		{
		}

		protected override void PreSend()
		{
			SendingAction.EntryHeader.CH_BGMReference = "PRE_REF_123";
		}
	}
}
