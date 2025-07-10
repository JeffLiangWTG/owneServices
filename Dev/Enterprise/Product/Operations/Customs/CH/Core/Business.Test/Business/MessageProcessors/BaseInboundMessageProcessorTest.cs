using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business.Testing;

abstract class BaseInboundMessageProcessorTest : TestCaseWithFactory
{
	protected abstract ApplicationTypeMessageProcessor GetMessageProcessor(LoggingInformation logger);

	protected abstract (string MessageType, Event ExpectedEvent)[] MessageTypes { get; }

	protected abstract ZString ApplicationCode { get; }

	protected abstract ZString MessageSubType { get; }

	protected abstract ZString BGMReference { get; }

	protected abstract ZString ReceivedMessage { get; }

	protected abstract ZString ExpectedMessageFriendlyName { get; }

	public void TestMessageFriendlyName()
	{
		AssertEquals("MessageFriendlyName", ExpectedMessageFriendlyName, Processor.MessageFriendlyName);
	}

	public virtual void TestMessageFilter()
	{
		CombineAssertions(() =>
		{
			foreach (var messageType in MessageTypes)
			{
				var factory = new BusinessObjectFactory();

				var message1 = MessageProcessorTestHelper.CreateEDIMessage(factory, messageType: messageType.MessageType, messageSubType: MessageSubType, applicationCode: ApplicationCode);
				var message2 = MessageProcessorTestHelper.CreateEDIMessage(factory, messageType: MessageSubTypeCodeList.Codes.Undefined, messageSubType: MessageSubType, applicationCode: ApplicationCode);
				var message3 = MessageProcessorTestHelper.CreateEDIMessage(factory, messageType: messageType.MessageType, messageSubType: MessageSubTypeCodeList.Codes.Undefined, applicationCode: ApplicationCode);
				var message4 = MessageProcessorTestHelper.CreateEDIMessage(factory, messageType: messageType.MessageType, messageSubType: MessageSubType, applicationCode: ApplicationCodeList.Codes.SYS);
				var message5 = MessageProcessorTestHelper.CreateEDIMessage(factory, messageType: messageType.MessageType, messageSubType: string.Empty, applicationCode: ApplicationCode);

				var selectedMessages = factory.Load<EDIMessage>(Processor.MessageFilter);
				AssertEquals("1 messages selected", 1, selectedMessages.Length);
				Assert("Contains message1", selectedMessages.Contains(message1));
				Assert("Not containing message2", !selectedMessages.Contains(message2));
				Assert("Not containing message3", !selectedMessages.Contains(message3));
				Assert("Not containing message4", !selectedMessages.Contains(message4));
				Assert("Not containing message5", !selectedMessages.Contains(message5));
			}
		});
	}

	public virtual (CusEntryHeader entryHeader, EDIMessage ediMessage) PrepareDataForLinkToParent_Success(BusinessObjectFactory factory, ZString messageType) => MessageProcessorTestHelper.CreateHeaderAndMessage(factory, messageType, MessageSubType, ReceivedMessage, BGMReference);

	public virtual void TestLinkToParent_Success()
	{
		CombineAssertions(() =>
		{
			foreach (var messageType in MessageTypes)
			{
				var factory = new BusinessObjectFactory();
				CusEntryHeader entryHeader;
				EDIMessage ediMessage;
				using (DisposableEnvironment.ForBranch(UserBranchPK.ToGuid()))
				{
					(entryHeader, ediMessage) = PrepareDataForLinkToParent_Success(factory, messageType.MessageType);
				}
				ediMessage.EM_GB = GlbBranch.CurrentBranch.PK;
				factory.Save();

				AssertNotEquals("Pre-condition: EM_GB", UserBranchPK, ediMessage.EM_GB);
				ProcessMessage(ediMessage);

				AssertEquals("Status", EDIMessage.Status.ProcessedOK, ediMessage.EM_Status);
				AssertEquals("Linked Table", CusEntryHeader.Schema.TableName, ediMessage.EM_LinkTable);
				AssertEquals("Linked Entry Header", entryHeader.PK, ediMessage.EM_LinkUniqueID);
				AssertEquals("EM_GB", UserBranchPK, ediMessage.EM_GB);
				AssertEquals("EI_GB", UserBranchPK, ediMessage.Interchange?.EI_GB);
			}
		});
	}

	public virtual void TestLinkToParent_MissingParent()
	{
		foreach (var messageType in MessageTypes)
		{
			EDIMessage ediMessage = MessageProcessorTestHelper.CreateEDIMessage(Factory, messageType.MessageType, MessageSubType, messageText: ReceivedMessage);

			Processor.ProcessMessage(ediMessage);

			CombineAssertions(() =>
			{
				AssertEquals("Status", EDIMessage.Status.Discarded, ediMessage.EM_Status);
				Assert("Logged Warning", Logger.ContainsLogEntry("Unable to link EDI Message "));
			});
		}
	}

	public virtual void TestDeserializeXML_Failure()
	{
		foreach (var messageType in MessageTypes)
		{
			var ediMessage = MessageProcessorTestHelper.CreateEDIMessage(Factory, messageType: messageType.MessageType, messageSubType: MessageSubType, messageText: "This will cause a deserialize failure!");
			Factory.Save();

			AssertExceptionThrown<NotSupportedException>(() => Processor.ProcessMessage(ediMessage));
			Factory.Save();
		}
	}

	protected void ProcessMessage(EDIMessage message)
	{
		MessageProcessorTestHelper.ProcessMessage(Processor, message);
	}

	internal LoggingInformationForTesting Logger => logger ?? (logger = new LoggingInformationForTesting());
	LoggingInformationForTesting logger;

	internal ApplicationTypeMessageProcessor Processor => processor ?? (processor = GetMessageProcessor(Logger));
	ApplicationTypeMessageProcessor processor;
	ZGuid UserBranchPK => userBranchPK ??= CreateUserBranch().PK;
	ZGuid? userBranchPK;

	GlbBranch CreateUserBranch()
	{
		var branch = GlbCompany.CurrentCompany.Branches.AddNew();
		branch.GB_Code = "ZZZ";
		branch.GB_IsActive = ZBool.True;
		branch.Factory.Save();
		return branch;
	}
}
