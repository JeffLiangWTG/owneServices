using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business.Testing;

public class BaseMessageManagerBaseOnlyTest : TestCaseWithFactory
{
	public void TestGenerateMessages()
	{
		var sendingObject = new BaseMessageSendingObjectForTesting(Factory);
		sendingObject.MessageTypeForEDIMessage = "TYP";
		sendingObject.MessageSubTypeForEDIMessage = "SUB";
		sendingObject.MessageString = "<xml />";
		sendingObject.GlbExternalPasswordPK = ZGuid.NewZGuid();

		var manager = new BaseMessageManagerForTesting(sendingObject);
		var ediMessages = manager.GenerateMessages();
		CombineAssertions(() =>
		{
			AssertEquals("# of messages", 1, ediMessages.Length);
			var ediMessage = ediMessages[0];
			AssertEquals("EM_ApplicationCode", "XXX", ediMessage.EM_ApplicationCode);
			AssertEquals("EM_ApplicationReference", "ApplicationReference", ediMessage.EM_ApplicationReference);
			AssertEquals("EM_MessageType", "TYP", ediMessage.EM_MessageType);
			AssertEquals("EM_MessageType", "TYP", ediMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", "SUB", ediMessage.EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, ediMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, ediMessage.EM_Status);
			AssertEquals("EM_MessageText", "<xml />", ediMessage.EM_MessageText);
			AssertEquals("EM_GP", sendingObject.GlbExternalPasswordPK, ediMessage.EM_GP);

			AssertEquals("# of added messages", 1, sendingObject.AddedMessages.Count);
			AssertSame(ediMessage, sendingObject.AddedMessages[0]);

			AssertEquals("# of AfterGenerateMessage invocations", 1, manager.AfterGenerateMessageInvocations.Count);
			AssertSame(sendingObject, manager.AfterGenerateMessageInvocations[0]);
		});
	}

	public void TestGenerateMessages_NoMessage()
	{
		var manager = new BaseMessageManagerForTesting(new BaseMessageSendingObjectForTesting(Factory));
		manager.DoNotCreateMessage = true;
		AssertEquals("Empty array expected", 0, manager.GenerateMessages().Length);
	}

	class BaseMessageSendingObjectForTesting : BaseMessageSendingObject, IMessageSendingObject
	{
		internal BaseMessageSendingObjectForTesting(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ZString ApplicationCode => "XXX";

		public ZString GetApplicationReference() => "ApplicationReference";

		public ZString MessageTypeForEDIMessage { get; internal set; }

		public ZString MessageSubTypeForEDIMessage { get; internal set; }

		internal ZGuid GlbExternalPasswordPK { get; set; }
		public ZGuid GetCredentialPK() => GlbExternalPasswordPK;

		internal ZString MessageString { get; set; }
		public ZString ToMessageString() => MessageString;

		internal List<EDIMessage> AddedMessages { get; } = new List<EDIMessage>();
	}

	class BaseMessageManagerForTesting : BaseMessageManager<BaseMessageSendingObjectForTesting>
	{
		internal BaseMessageManagerForTesting(BaseMessageSendingObjectForTesting sendingObject) : base(sendingObject)
		{
		}

		public override string MessageFriendlyName => throw new NotImplementedException();

		protected override EDIMessage CreateEDIMessageCore(BaseMessageSendingObjectForTesting sendingObject)
		{
			return DoNotCreateMessage ? null : base.CreateEDIMessageCore(sendingObject);
		}

		internal bool DoNotCreateMessage { get; set; }

		protected override void AfterGenerateMessage(BaseMessageSendingObjectForTesting sendingObject, EDIMessage message)
		{
			sendingObject.AddedMessages.Add(message);
			AfterGenerateMessageInvocations.Add(sendingObject);
		}

		public override void RollbackOnSaveFailed()
		{
		}

		internal List<BaseMessageSendingObjectForTesting> AfterGenerateMessageInvocations = new List<BaseMessageSendingObjectForTesting>();
	}
}
