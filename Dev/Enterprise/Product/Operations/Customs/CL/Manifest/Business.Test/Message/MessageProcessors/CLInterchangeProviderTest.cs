using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	sealed class CLInterchangeProviderTest : InterchangeProviderTestCase
	{
		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new CLInterchangeProvider(new LoggingInformation(), collection);

		public override void TestMessagesPopulateNewInterchange()
		{
			var message1 = CreateAndPopulateMessage(MessageTypes.Codes.CHB, "12345");
			var message2 = CreateAndPopulateMessage(MessageTypes.Codes.CHB, "12351");
			var message3 = CreateAndPopulateMessage(MessageTypes.Codes.CHE, "19960");

			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new EDIMessage[] { message1, message2, message3 });

			var logger = new LoggingInformation();
			var provider = new CLInterchangeProvider(logger, messages);
			provider.PackCollatedMessagesIntoInterchanges();
			var interchanges = provider.Interchanges;

			Factory.Save();

			message1.Reload();
			message2.Reload();
			message3.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("Number Of Interchanges", 3, interchanges.Length);
				AssertNotEquals("Confirmed different interchanges for each message no collation", message1.EM_EI, message2.EM_EI);
				AssertNotEquals("Confirmed different interchanges for each message no collation", message2.EM_EI, message3.EM_EI);

				var interchange1 = interchanges.FirstOrDefault(x => x.PK == message1.EM_EI);
				var interchange2 = interchanges.FirstOrDefault(x => x.PK == message2.EM_EI);
				var interchange3 = interchanges.FirstOrDefault(x => x.PK == message3.EM_EI);

				AssertNotNull("Interchange 1 is linked to message 1", interchange1);
				AssertEquals("Message 1 is sent", EDIMessage.Status.Sent, message1.EM_Status);

				AssertNotNull("Interchange 2 is linked to message 2", interchange2);
				AssertEquals("Message 2 is sent", EDIMessage.Status.Sent, message2.EM_Status);

				AssertNotNull("Interchange 3 is linked to message 3", interchange3);
				AssertEquals("Message 3 is sent", EDIMessage.Status.Sent, message3.EM_Status);
			});
		}

		public void TestDoNotSendInterchangeWithEmptyBody()
		{
			var message = CreateAndPopulateMessage(MessageTypes.Codes.CHB, "12345", ZString.Empty);

			var messageCollection = new NonDependentEDIMessageCollection(Factory);
			messageCollection.Add(message);

			var logger = new LoggingInformation();
			var provider = new CLInterchangeProvider(logger, messageCollection);

			AssertEquals(0, provider.Interchanges.Length);
			AssertContains("Message 12345 will be discarded as the message text is empty.", logger.UserLogStrings[0]);

			message.EM_MessageText = "Message Text";
			provider = new CLInterchangeProvider(logger, messageCollection);
			AssertEquals(1, provider.Interchanges.Length);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var sms = CreateSMSMessageSending();
			CLCustomsDataRegistry.Instance.CLSMSMessageSending.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sms);
		}

		EDIMessage CreateAndPopulateMessage(ZString messageType, ZString messageNumber, string bodyText = "Message Text")
		{
			var message = Factory.New<TestEdiMessage>();

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CLCustoms;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageText = bodyText;
			message.EM_MessageNum = messageNumber;
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;

			return message;
		}

		CLSMSMessageSending CreateSMSMessageSending()
		{
			var sms = new CLSMSMessageSending(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			sms.MachineName = "Machine Name";
			sms.ApplicationNodePassword = "1234";
			sms.RunningIntervalInSeconds = 15;
			sms.SendFolder = @"D:\Folders\SendFolder";
			sms.UnknownFolder = @"D:\Folders\UnknownFolder";
			sms.InvalidFolder = @"D:\Folders\InvalidFolder";
			sms.RejectedFolder = @"D:\Folders\RejectedFolder";
			sms.ReceiveFolder = @"D:\Folders\ReceiveFolder";
			sms.AcceptedFolder = @"D:\Folders\AcceptedFolder";

			Factory.Save();
			return sms;
		}
	}
}
