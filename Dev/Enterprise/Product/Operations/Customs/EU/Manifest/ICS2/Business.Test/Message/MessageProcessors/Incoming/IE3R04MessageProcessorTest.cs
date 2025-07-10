using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3R04;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class IE3R04MessageProcessorTest : MessageProcessorWithEmailNotificationTest<IE3R04MessageProcessor, Ie3R04Type>
	{
		public void TestEmailContent_DataElementOmittedFromTheEDIMessage_OmitTheRowFromTheTable()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = CommonManifestJobReference;
			manifestHeader.AMA_MasterBill = CommonMasterBill;
			EUICS2MessageTestHelper.SetManifestHeaderEntryNumberForTesting(manifestHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, CommonReferenceNumber);

			var outgoingMessage = Factory.New<TestEdiMessage>();
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			outgoingMessage.EM_LinkedObject = manifestHeader;
			outgoingMessage.EM_MessageText = $"<?xml version=\"1.0\" encoding=\"utf-8\"?><IE3F10 xmlns=\"urn:wco:datamodel:eu:ics2:2\"><LRN>{CommonReferenceNumber}</LRN></IE3F10>";
			outgoingMessage.EM_SystemCreateUser = staffCode;

			var incomingMessage = GetIncomingMessage(CommonReferenceNumber);
			incomingMessage.EM_MessageText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
															<IE3R04 xmlns=""urn:wco:datamodel:eu:ics2:2"">
																<LRN>{0}</LRN>
																<MRN>MRN1</MRN>
															</IE3R04>"
			, CommonReferenceNumber);

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);

				var expectedEmailBody = new string[] {
					$"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"></tr></thead><tr><th colspan=\"2\" align=\"center\">Arrival Response Received</th></tr><tr><td>Job Number</td><td>MAN0009999</td></tr><tr><td>LRN</td><td>EmailTestReferenceNumber</td></tr><tr><td>MRN</td><td>MRN1</td></tr><tr><td>Registration Date</td><td>&nbsp;</td></tr></table>"
				};

				EUICS2MessageTestHelper.AssertEmail(ExpectedEmailSubject, expectedEmailBody, new[] { new ZString(StaffEmail) });
			}
		}

		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo;

		protected override string ExpectedEmailSubject => "ICS2 - Arrival Response Received";

		protected override string[] ExpectedEmailBody => new string[] {
			"An Arrival Response message has been received.",
			"Records show that Job Number MAN0009999 for Master Bill Number CommonMasterBill has arrived.",
			$"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"></tr></thead><tr><th colspan=\"2\" align=\"center\">Arrival Response Received</th></tr><tr><td>Job Number</td><td>MAN0009999</td></tr><tr><td>LRN</td><td>EmailTestReferenceNumber</td></tr><tr><td>MRN</td><td>MRN1</td></tr><tr><td>Registration Date</td><td>2023-03-30T00:00:00Z</td></tr><tr><td>Notify Party</td><td>identificationNu1</td></tr><tr><td>Person Notifying at Arrival</td><td>identificationNu1</td></tr><tr><td>Customs Office of First Entry</td><td>referen1</td></tr></table>"
		};

		protected override TestEdiMessage GetIncomingMessage(string primaryReferenceNumber)
		{
			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = MessageTypes.Codes.R04;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
															<IE3R04 xmlns=""urn:wco:datamodel:eu:ics2:2"">
																<LRN>{0}</LRN>
																<MRN>MRN1</MRN>
																<registrationDate>
																<DateTime>2023-03-30T00:00:00Z</DateTime>
																</registrationDate>
																<notifyParty>
																<identificationNumber>identificationNu1</identificationNumber>
																</notifyParty>
																<personNotifyingTheArrival>
																<identificationNumber>identificationNu1</identificationNumber>
																</personNotifyingTheArrival>
																<customsOfficeOfFirstEntry>
																<referenceNumber>referen1</referenceNumber>
																</customsOfficeOfFirstEntry>
															</IE3R04>"
			, primaryReferenceNumber);
			return incomingMessage;
		}

		protected override string EntryNumberTypeToCreate => CusEntryNumberTypes.Standard.LocalReferenceNumber;

		protected override void TestProcessMessageCore_AdditionalAssertion(AsycudaManifestHeader manifestHeader)
		{
			AssertEquals("MRN1", manifestHeader.ArrivalReferenceNumber);
		}

		protected override IE3R04MessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new IE3R04MessageProcessor(logger);
		}
	}
}
