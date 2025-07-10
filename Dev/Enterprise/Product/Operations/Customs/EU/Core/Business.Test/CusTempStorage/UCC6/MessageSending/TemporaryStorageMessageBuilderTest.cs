using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public abstract class TemporaryStorageMessageBuilderTest<T> : TestCaseWithFactory
		where T : TemporaryStorageMessageBuilder
	{
		protected void AssertMessageCanBePopulated<TFunction>(string messageText) where TFunction : TemporaryStorageMessageFunction
		{
			function = typeof(TFunction) == typeof(InvalidationRequestTSDMessageFunction) ? new InvalidationRequestTSDMessageFunction(new TemporaryStorageMessageSendingObject(header)) : Activator.CreateInstance<TFunction>();
			var messageBuilder = GetMessageBuilder(header, function);
			var result = messageBuilder.PopulateMessages();
			Assert("result.IsSuccess", result.IsSuccess);
			AssertEquals("header.Messages.Count", 1, header.Messages.Count);

			var message = header.Messages[0];
			AssertEquals("EM_ApplicationCode", ApplicationCode, message.EM_ApplicationCode);
			AssertContains("EM_MessageText", messageText, message.EM_MessageText);
			AssertEquals("EM_MessageType", ExpectedEM_MessageType, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", MessageSubType, message.EM_MessageSubType);
			AssertEquals("EM_ApplicationReference", ZString.Empty, message.EM_ApplicationReference);
			AssertEquals("EM_LinkedObject", header, message.EM_LinkedObject);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("EM_GP", ExpectedEM_GP, message.EM_GP);
			AssertEquals("EM_MessageOwner", ExpectedEM_MessageOwner, message.EM_MessageOwner);
		}

		protected TemporaryStorageMessageFunction function;

		protected virtual string MessageSubType => function.MessageType;

		protected virtual ZString ExpectedEM_MessageType => TemporaryStorageConstants.MessageTypeList.EDIMessageType;

		protected virtual ZGuid ExpectedEM_GP => ZGuid.Empty;

		protected virtual ZString ExpectedEM_MessageOwner => ZString.Empty;

		protected abstract T GetMessageBuilder(TemporaryStorageHeader header, TemporaryStorageMessageFunction function);

		protected abstract ZString ApplicationCode { get; }

		protected override void SetUp()
		{
			base.SetUp();
			header = GetNewHeader();
		}
		TemporaryStorageHeader header;

		protected virtual TemporaryStorageHeader GetNewHeader() => Factory.New<TemporaryStorageHeader>();
	}
}
