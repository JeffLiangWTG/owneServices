using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	sealed class CLOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestCreateNewInterchangeProviderForSeaMode()
		{
			var message1 = CreateAndPopulateMessage(MessageTypes.Codes.CHB, "12345");
			var message2 = CreateAndPopulateMessage(MessageTypes.Codes.CHB, "12351");

			var logger = new LoggingInformation();
			var processor = new CHLOutgoingMessageProcessor(logger);
			processor.ProcessMessage(CancellationToken.None);

			CombineAssertions(() =>
			{
				var interchangesCreated = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("NumberOfInterchanges", 2, interchangesCreated.Length);

				message1.Reload();
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message1.EM_Status);
				var interchange1 = interchangesCreated.Single(i => i.PK == message1.EM_EI);
				AssertEquals("EI_InterchangeNum", message1.EM_MessageNum, interchange1.EI_InterchangeNum);
				AssertEquals("EI_InterchangeNum", "0000000001", interchange1.EI_InterchangeNum);

				message2.Reload();
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message2.EM_Status);
				var interchange2 = interchangesCreated.Single(i => i.PK == message2.EM_EI);
				AssertEquals("EI_InterchangeNum", message2.EM_MessageNum, interchange2.EI_InterchangeNum);
				AssertEquals("EI_InterchangeNum", "0000000002", interchange2.EI_InterchangeNum);
			});
		}

		public void TestCreateNewInterchangeProviderForAirMode()
		{
			var message = CreateAndPopulateMessage(MessageTypes.Codes.CHE, "19960");

			var logger = new LoggingInformation();
			var processor = new CHLOutgoingMessageProcessor(logger);
			processor.ProcessMessage(CancellationToken.None);

			CombineAssertions(() =>
			{
				var interchangesCreated = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("NumberOfInterchanges", 1, interchangesCreated.Length);

				message.Reload();
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message.EM_Status);
				var interchange1 = interchangesCreated.Single(i => i.PK == message.EM_EI);
				AssertEquals("EI_InterchangeNum", message.EM_MessageNum, interchange1.EI_InterchangeNum);
				AssertEquals("EI_InterchangeNum", "0000000001", interchange1.EI_InterchangeNum);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var sms = CreateSMSMessageSending();
			CLCustomsDataRegistry.Instance.CLSMSMessageSending.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sms);
		}

		EDIMessage CreateAndPopulateMessage(ZString messageType, ZString messageNumber)
		{
			var message = Factory.New<CLMessage>();

			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CLCustoms;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageText = "Message Text";
			message.EM_MessageNum = messageNumber;
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();
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
