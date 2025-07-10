using System;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.SoapEnvelope.S12;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SoapEnvelopeMessageProcessorTest : MessageProcessorWithEmailNotificationTest<SoapEnvelopeMessageProcessor, Envelope>
	{
		public void TestProcessMessageWithReceipt()
		{
			var incomingMessage = GetMessageWithExpectedContent("IC2_RCV_F24_47_WithReceipt.xml");
			Factory.Save();

			using (Factory.AddDisposableService())
			{
				incomingMessage.EM_Status = "QUE";

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				using (EmailGroupNotificationRegistryItem.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, new GroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, notificationGroupPK)))
				{
					Processor.PreProcessMessage(incomingMessage);
					Processor.ProcessMessage(incomingMessage);

					CombineAssertions(() =>
					{
						AssertEquals("No email should be created when the message contains a valid Receipt element.", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
						AssertEquals("Should attach on the source manifest header.", manifestHeader.PK, incomingMessage.EM_LinkUniqueID);
						AssertEquals("Message status on manifest header should be updated to SNT", MessageStatusCodeList.Codes.Sent, manifestHeader.AMA_MessageStatus);
					});
				}
			}
		}

		public void TestProcessMessageWithReceipt_MessageStatusNotUpdated()
		{
			var incomingMessage = GetMessageWithExpectedContent("IC2_RCV_F24_47_WithReceipt.xml");
			Factory.Save();

			using (Factory.AddDisposableService())
			{
				incomingMessage.EM_Status = "QUE";

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				using (EmailGroupNotificationRegistryItem.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, new GroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, notificationGroupPK)))
				{
					manifestHeader.AMA_MessageStatus = MessageStatusCodeList.Codes.Error;

					Processor.PreProcessMessage(incomingMessage);
					Processor.ProcessMessage(incomingMessage);

					AssertEquals("Message status on manifest header should NOT be updated", MessageStatusCodeList.Codes.Error, manifestHeader.AMA_MessageStatus);
				}
			}
		}
		public void TestProcessMessageWithError()
		{
			var incomingMessage = GetMessageWithExpectedContent("IC2_RCV_F24_52_WithFailure.xml");
			Factory.Save();

			using (Factory.AddDisposableService())
			{
				incomingMessage.EM_Status = "QUE";

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				using (EmailGroupNotificationRegistryItem.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, new GroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, notificationGroupPK)))
				{
					Processor.PreProcessMessage(incomingMessage);
					Processor.ProcessMessage(incomingMessage);

					AssertEquals("Message status on manifest header should be updated to 'ERR'", MessageStatusCodeList.Codes.Error, manifestHeader.AMA_MessageStatus);
				}
			}
		}

		public void TestProcessMessageWithError_MessageStatusNotUpdated()
		{
			var incomingMessage = GetMessageWithExpectedContent("IC2_RCV_F24_52_WithFailure.xml");
			Factory.Save();

			using (Factory.AddDisposableService())
			{
				incomingMessage.EM_Status = "QUE";

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				using (EmailGroupNotificationRegistryItem.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, new GroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, notificationGroupPK)))
				{
					manifestHeader.AMA_MessageStatus = MessageStatusCodeList.Codes.Sent;
					Processor.PreProcessMessage(incomingMessage);
					Processor.ProcessMessage(incomingMessage);

					AssertEquals("Message status on manifest header should NOT be updated", MessageStatusCodeList.Codes.Sent, manifestHeader.AMA_MessageStatus);
				}
			}
		}

		public void TestProcessMessageAndUpdateRequestHeaders()
		{
			var incomingMessage = GetMessageWithExpectedContent("IC2_RCV_F24_47_WithReceipt.xml");

			var requestHeader1 = manifestHeader.RequestHeaders.AddNew();
			requestHeader1.EUS_Identifier = "A70";
			requestHeader1.EUS_Status = MessageStatusCodeList.Codes.Awaiting;
			requestHeader1.EUS_Type = "YYY";

			var requestHeader2 = manifestHeader.RequestHeaders.AddNew();
			requestHeader2.EUS_Identifier = "B90";
			requestHeader2.EUS_Status = MessageStatusCodeList.Codes.Accepted;
			requestHeader2.EUS_Type = "YYY";

			var requestHeader3 = manifestHeader.RequestHeaders.AddNew();
			requestHeader3.EUS_Identifier = "C85";
			requestHeader3.EUS_Status = MessageStatusCodeList.Codes.Awaiting;
			requestHeader3.EUS_Type = "YYY";

			var requestHeader4 = manifestHeader.RequestHeaders.AddNew();
			requestHeader4.EUS_Identifier = "D26";
			requestHeader4.EUS_Status = MessageStatusCodeList.Codes.Awaiting;
			requestHeader4.EUS_Type = "YYY";

			var outgoingMessage = outboundInterchange.ContainedMessages[0];

			outgoingMessage.EM_MessageType = MessageTypes.Codes.R02;
			outgoingMessage.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?><IE3R02 xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:eu:ics2:2""></IE3R02>";

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				incomingMessage.EM_Status = "QUE";
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);

				CombineAssertions(() =>
				{
					AssertEquals("Should update its status as its status is AWA.", MessageStatusCodeList.Codes.Sent, requestHeader1.EUS_Status);
					AssertEquals("Should not update the status as its status is not AWA.", MessageStatusCodeList.Codes.Accepted, requestHeader2.EUS_Status);
					AssertEquals("Should update the status as its status is AWA.", MessageStatusCodeList.Codes.Sent, requestHeader3.EUS_Status);
					AssertEquals("Should update its status as its status is AWA", MessageStatusCodeList.Codes.Sent, requestHeader4.EUS_Status);
				});
			}
		}

		public void TestMessageNotProcessed_EM_StatusNotPRS()
		{
			var incomingMessage = GetMessageWithExpectedContent("IC2_RCV_F24_47_WithReceipt.xml");
			incomingMessage.EM_RetryCount = 2;

			var outgoingMessage = outboundInterchange.ContainedMessages[0];

			outgoingMessage.EM_MessageType = MessageTypes.Codes.R02;
			outgoingMessage.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?><IE3R02 xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:eu:ics2:2""></IE3R02>";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);

				AssertEquals("Message is not processed", EDIMessage.Status.Discarded, incomingMessage.EM_Status);
				AssertEquals("Header Message Status not chenged", MessageStatusCodeList.Codes.Awaiting, manifestHeader.AMA_MessageStatus);
			}
		}

		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2ErrorsTo;

		protected override string ExpectedEmailSubject => $"ICS2 - Confirmation Response with Failure(s) for {CommonManifestJobReference}";

		protected override string[] ExpectedEmailBody
		{
			get
			{
				if (expectedEmailBody == null)
				{
					var reader = new Customs.Business.Testing.TestFileReader(typeof(SoapEnvelopeMessageProcessorTest));
					var emailBody = reader.GetEmbeddedFileText("Enterprise.Customs.EU.Manifest.ICS2.Business.Test.Message.MessageProcessors.Incoming.TestFiles", "IC2_RCV_F24_52_WithFailure_EmailBody.html");

					expectedEmailBody = new[] { emailBody };
				}

				return expectedEmailBody;
			}
		}

		string[] expectedEmailBody;

		protected override TestEdiMessage GetIncomingMessage(string registrationNumber)
		{
			return GetMessageWithExpectedContent("IC2_RCV_F24_52_WithFailure.xml");
		}

		protected override TestEdiMessage GetIncomingMessageWithInvalidLinkedObject()
		{
			return GetMessageWithExpectedContent("IC2_RCV_F24_55_WithoutRefToMessageId.xml");
		}

		TestEdiMessage GetMessageWithExpectedContent(string fileName)
		{
			var reader = new Customs.Business.Testing.TestFileReader(typeof(SoapEnvelopeMessageProcessorTest));
			var messageText = reader.GetEmbeddedFileText("Enterprise.Customs.EU.Manifest.ICS2.Business.Test.Message.MessageProcessors.Incoming.TestFiles", fileName);
			messageText = messageText.Replace("[InterchangePK]", outboundInterchange.PK.ToString());

			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageType = "TST";
			incomingMessage.EM_MessageSubType = ICS2InboundEDIMessage.UndefinedSubType;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageText = messageText;

			return incomingMessage;
		}

		protected override (AsycudaManifestHeader, TestEdiMessage) PrepareForEmailTesting()
		{
			var incomingMessage = GetMessageWithExpectedContent("IC2_RCV_F24_52_WithFailure.xml");
			return (manifestHeader, incomingMessage);
		}

		protected override SoapEnvelopeMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new SoapEnvelopeMessageProcessor(logger);
		}

		protected override void TestProcessMessageCore_AdditionalAssertion(AsycudaManifestHeader manifestHeader)
		{
			AssertEquals("Message status on manifest header should be updated to ERR", MessageStatusCodeList.Codes.Error, manifestHeader.AMA_MessageStatus);
		}

		protected override void SetUp()
		{
			base.SetUp();

			manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = CommonManifestJobReference;
			manifestHeader.AMA_MasterBill = CommonMasterBill;
			manifestHeader.AMA_MessageStatus = MessageStatusCodeList.Codes.Awaiting;

			var outgoingMessage = Factory.New<TestEdiMessage>();
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			outgoingMessage.EM_LinkedObject = manifestHeader;
			outgoingMessage.EM_MessageText = "Test Message";
			outgoingMessage.EM_SystemCreateUser = staffCode;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = "TST";
			outgoingMessage.EM_Status = EDIMessage.Status.ProcessedOK;

			outboundInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			outboundInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_InterchangeType = "TST";
			outboundInterchange.EI_Status = EDIInterchange.Status.Sent;
			outboundInterchange.ContainedMessages.Add(outgoingMessage);

			Factory.Save();
		}

		AsycudaManifestHeader manifestHeader;
		EDIInterchange outboundInterchange;
	}
}
