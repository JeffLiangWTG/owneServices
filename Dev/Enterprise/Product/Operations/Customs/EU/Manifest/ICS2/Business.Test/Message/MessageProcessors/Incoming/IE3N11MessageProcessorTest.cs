using System;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N11;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class IE3N11MessageProcessorTest : MessageProcessorWithEmailNotificationTest<IE3N11MessageProcessor, Ie3N11Type>
	{
		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2UnmatchedOrNotSentTo;

		protected override string ExpectedEmailSubject => $"ICS2 Unmatched or Not Sent Master Bill {CommonReferenceNumber}";

		protected override string[] ExpectedEmailBody => new string[]
		{
			$"Records show that Master Bill Number {CommonReferenceNumber} has not had an ENS filed.",
		};

		protected override string EntryNumberTypeToCreate => CusEntryNumberTypes.Standard.LocalReferenceNumber;

		public void TestPreProcessMessageFailed_WhenLinkedObjectIsNull_ShouldSendNotificationEmail()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "GP1";
			group.GG_Desc = "Group 1";

			var staff = group.Staff.AddNew();
			staff.GS_Code = "GS1";
			staff.GS_FullName = "Staff 1";
			staff.GS_EmailAddress = "staff1@staff.com";

			var incomingMessage = GetIncomingMessage("TestReferenceNumber");
			incomingMessage.EM_RetryCount = IE3N11MessageProcessor.RetryCount;

			Factory.Save();

			using (Factory.AddDisposableService())
			using (ICS2CustomsDataRegistry.Instance.EnableICS2UnmatchedOrNotSentTo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new GroupNotification(Core.Constants.EmailTo.NominatedGroup, group.PK)))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);

				EUICS2MessageTestHelper.AssertEmail("ICS2 Unmatched or Not Sent Master Bill TestReferenceNumber", new string[]
					{
						"A response message has been received from Customs.<br />Shown below is a summary of relevant information received in the message.<br />",
						"Records show that Master Bill Number TestReferenceNumber has not had an ENS filed.",
						"There is no ICS2 Manifest found for this Master Bill."
					},
					new[] { staff.GS_EmailAddress });
			}
		}

		protected override TestEdiMessage GetIncomingMessage(string localReferenceNumber)
		{
			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = MessageTypes.Codes.N11;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageText = string.Format(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<IE3N11 xmlns=""urn:wco:datamodel:eu:ics2:2"">
	<notificationDate>
		<DateTime>2020-09-25T16:59:20+02:00</DateTime>
	</notificationDate>
	<transportDocument>
		<documentNumber>{0}</documentNumber>
		<type>C665</type>
	</transportDocument>
	<customsOfficeOfFirstEntry>
		<referenceNumber>DE007154</referenceNumber>
	</customsOfficeOfFirstEntry>
	<supplementaryDeclarant>
		<identificationNumber>BE08EORI1000003</identificationNumber>
	</supplementaryDeclarant>
</IE3N11>", localReferenceNumber);
			return incomingMessage;
		}

		protected override void TestProcessMessageCore_AdditionalAssertion(AsycudaManifestHeader manifestHeader)
		{
			AssertEquals("Customs status on manifest header should be updated to PND", EUICS2CustomsStatusList.Codes.PND, manifestHeader.AMA_CustomsStatus);
		}

		protected override IE3N11MessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new IE3N11MessageProcessor(logger);
		}
	}
}
