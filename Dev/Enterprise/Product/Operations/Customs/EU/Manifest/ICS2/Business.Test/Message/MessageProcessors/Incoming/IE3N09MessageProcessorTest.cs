using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N09;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class IE3N09MessageProcessorTest : MessageProcessorWithEmailNotificationTest<IE3N09MessageProcessor, Ie3N09Type>
	{
		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo;

		protected override string ExpectedEmailSubject => $"ICS2 - AEO Control Notification for {CommonManifestJobReference}/{CommonReferenceNumber}";

		protected override string[] ExpectedEmailBody => new string[]
		{
			$"ICS2 - AEO Control Notification for {CommonManifestJobReference}/{CommonReferenceNumber}"
		};

		protected override TestEdiMessage GetIncomingMessage(string registrationNumber)
		{
			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = MessageTypes.Codes.N03;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageText = string.Format(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<IE3N09 xmlns=""urn:wco:datamodel:eu:ics2:2"">
	<MRN>{0}</MRN>
	<notificationDate>
		<DateTime>2020-09-25T16:56:31+02:00</DateTime>
	</notificationDate>
	<scheduledControlDate>
		<DateTime>2020-09-25T16:56:31+02:00</DateTime>
	</scheduledControlDate>
	<customsOfficeOfControl>
	    <referenceNumber>DE007154</referenceNumber>
	</customsOfficeOfControl>
	<control>
		<examinationPlace>
			<referenceNumber>DE007154</referenceNumber>
		</examinationPlace>
		<controlSubject>
			<consignmentMasterLevel>
				<consignmentHouseLevel>
					<transportDocumentHouseLevel>
						<documentNumber>54SBNKSB3A1D74N7TMVTQSY5GZG68S5NY18CMYIUGSFF2KPHBNR9Y</documentNumber>
						<type>C665</type>
					</transportDocumentHouseLevel>
				</consignmentHouseLevel>
			</consignmentMasterLevel>
		</controlSubject>
	</control>
	<declarant>
		<identificationNumber>DE08EORI1000003</identificationNumber>
	</declarant>
</IE3N09>", registrationNumber);
			return incomingMessage;
		}

		protected override void TestProcessMessageCore_AdditionalAssertion(AsycudaManifestHeader manifestHeader)
		{
			AssertEquals("Customs status on manifest header should be updated to AEO", EUICS2CustomsStatusList.Codes.AEO, manifestHeader.AMA_CustomsStatus);
		}

		protected override IE3N09MessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new IE3N09MessageProcessor(logger);
		}
	}
}
