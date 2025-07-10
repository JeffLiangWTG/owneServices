using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class AutoSendNctsP5MessageProcessorTest : TestCaseWithFactory
{
	public void TestExceptionThrownForP4Header()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		AssertExceptionThrown<ArgumentException>(() => CreateProcessor(header));
	}

	public void TestErrorOnMessageSendingObjectParent()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_JobReference = "NCT11223344";
		var mock = new Mock<MessageSendingConfiguration> { CallBase = true };
		var messageSendingParent = new MessageSendingObjectParentForTest(nctsHeader);

		var messageTypeList = new CodeDescriptionPairList();
		messageTypeList.AddPairIfNotExist("011", "011");
		mock.Setup(m => m.MessageTypeList(It.IsAny<NctsHeader>())).Returns(messageTypeList);

		mock.Protected()
			.Setup<NctsHeaderMessageSendingObjectParent>("GetNewNctsHeaderMessageSendingObjectParentCore",
				ItExpr.IsAny<NctsHeader>()).Returns(messageSendingParent);
		mock.Protected().Setup<INctsHeaderMessageSendingObjectValidationDecider>("GetValidationDeciderCore")
			.Returns(new NctsHeaderMessageSendingObjectValidationDecider());

		using (NctsConfigurationTestHelper.TemporarilySetMessageSendingConfiguration(Factory, mock.Object))
		{
			var notifications = new NotificationBuffer();
			var processor = CreateProcessor(nctsHeader);
			processor.Process(notifications);
			AssertMultilineASCIIEquals("Error on Message Sending Object is logged.",
				"System cannot send Departure Movement because of following errors on Job:NCT11223344\r\nError - record: Test Error",
				notifications.AsString);
		}
	}

	public void TestErrorOnNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_JobReference = "NCT11223344";
		nctsHeader.BH_CarrierSCAC = "我";

		var notifications = new NotificationBuffer();
		var processor = CreateProcessor(nctsHeader);
		processor.Process(notifications);
		AssertMultilineASCIIEquals("Error on NCTS Header is logged.",
			"System cannot send Departure Movement message because of following errors on Job:NCT11223344, please fix all of them and try again.\r\nError - BH_CarrierSCAC: Carrier Code only accepts Western European languages characters.",
			notifications.AsString);
	}

	public void TestLockNctsHeaderWhenSendingMessage()
	{
		var nctsHeader = Factory.New<NctsHeader>();

		var nctsHeaderMutex = new ZGlobalMutex(ZArchitecture.Modules.MutexIDs.SendCustomsMessage, nctsHeader.PK.ToString());
		AssertEquals(true, nctsHeaderMutex.Lock());

		var notifications = new NotificationBuffer();
		var processor = CreateProcessor(nctsHeader);
		processor.Process(notifications);

		AssertContains("is trying to send the same message for this entry. Please wait until the lock has been released before trying to send the message again.", notifications.AsString);

		nctsHeaderMutex.Unlock();
		processor = CreateProcessor(nctsHeader);
		notifications = new NotificationBuffer();
		processor.Process(notifications);
		AssertContains("Please enter a Message Type", notifications.AsString);
	}

	IProcessor CreateProcessor(NctsHeader nctsHeader)
	{
		return new AutoSendNCTSP5MessageProcessor(nctsHeader);
	}

	sealed class MessageSendingObjectParentForTest : NctsHeaderMessageSendingObjectParent
	{
		public MessageSendingObjectParentForTest(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		protected override NonPersistentBusinessObjectCollection<NctsHeaderMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			return new NctsHeaderMessageSendingObjectCollection(Factory) { new SendingObjectForTest(NctsHeader) };
		}
	}

	sealed class SendingObjectForTest : NctsHeaderMessageSendingObject
	{
		public SendingObjectForTest(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		protected override NctsHeaderMessageSendingObjectValidation GetNewValidation()
		{
			return new MessageSendingValidationForTest(this);
		}
	}

	sealed class MessageSendingValidationForTest : NctsHeaderMessageSendingObjectValidation
	{
		public MessageSendingValidationForTest(AutoNctsHeaderMessageSendingObject parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			Parent.AddRowError("Test Error");
		}
	}
}
