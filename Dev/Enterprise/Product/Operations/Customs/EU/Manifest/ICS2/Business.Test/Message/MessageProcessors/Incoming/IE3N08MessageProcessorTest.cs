using System.Linq;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N08;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class IE3N08MessageProcessorTest : MessageProcessorWithEmailNotificationTest<IE3N08MessageProcessor, Ie3N08Type>
	{
		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo;

		protected override string ExpectedEmailSubject => $"ICS2 - Control Notification for {CommonManifestJobReference}/{CommonReferenceNumber}";

		protected override string[] ExpectedEmailBody => new string[]
		{
			$"Control Notification",
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>MRN</th><th>Notification Date</th><th>Scheduled Date</th><th>Customs Office</th><th>Representative</th></tr></thead><tr><td>EmailTestReferenceNumber</td><td>2022-09-30T00:00:00Z</td><td>2022-09-30T00:00:00Z</td><td>DE007154</td><td>123</td></tr></table>",
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><tr><td>Master</td><td>N380</td><td>DocumentID</td></tr><tr><td>Exam Place</td><td>place1</td><td>Reference Number</td><td>DE007154</td></tr><tr><td>Declarant</td><td>DE08EORI1000003</td><td>Person Notifying the arrival</td><td>person123</td><td>notify Party</td><td>ID1234</td></tr><tr><td>Receptacle</td></tr><tr><td>1234</td><td>12345</td></tr><tr><td>Shipping Marks</td><td>Type Packs</td></tr><tr><td>marks</td><td>12</td></tr><tr><td>Containers</td></tr><tr><td>123456</td></tr><tr><td>Goods Items</td><td>Item Number</td><td>Shipping Marks</td><td>Type Packs</td></tr><tr><td>&nbsp;</td><td>111</td><td>marks</td><td>12</td></tr><tr><td>&nbsp;</td><td>111</td><td>marks1</td><td>13</td></tr><tr><td>&nbsp;</td><td>111</td><td>marks2</td><td>14</td></tr><tr><td>House Bill</td><td>documentNumber1</td><td>C665</td></tr><tr><td>Goods Items</td><td>Item Number</td><td>Shipping Marks</td><td>Type Packs</td></tr><tr><td>&nbsp;</td><td>111</td><td>marks</td><td>12</td></tr><tr><td>&nbsp;</td><td>111</td><td>marks1</td><td>13</td></tr><tr><td>&nbsp;</td><td>111</td><td>marks2</td><td>14</td></tr><tr><td>Transport Means</td></tr><tr><td>789</td><td>&nbsp;</td><td>DE</td></tr><tr><td>Containers</td></tr><tr><td>123456</td><td>123457</td><td>123458</td></tr></table>",
		};

		protected override TestEdiMessage GetIncomingMessage(string masterReferenceNumber)
		{
			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = MessageTypes.Codes.N08;
			incomingMessage.EM_MessageText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<IE3N08 xmlns=""urn:wco:datamodel:eu:ics2:2"">
    <MRN>{0}</MRN>
    <notificationDate>
        <DateTime>2022-09-30T00:00:00Z</DateTime>
    </notificationDate>
    <scheduledControlDate>
        <DateTime>2022-09-30T00:00:00Z</DateTime>
    </scheduledControlDate>
    <customsOfficeOfControl>
        <referenceNumber>DE007154</referenceNumber>
    </customsOfficeOfControl>
    <representative>
        <identificationNumber>123</identificationNumber>
    </representative>
    <transportDocument>
        <documentNumber>DocumentID</documentNumber>
        <type>N380</type>
    </transportDocument>
    <control>
        <examinationPlace>
            <placeOfExamination>place1</placeOfExamination>
            <referenceNumber>DE007154</referenceNumber>
        </examinationPlace>
        <controlSubject>
            <consignmentMasterLevel>
                <receptacle>
                    <receptacleIdentificationNumber>1234</receptacleIdentificationNumber>
                </receptacle>
                <receptacle>
                    <receptacleIdentificationNumber>12345</receptacleIdentificationNumber>
                </receptacle>
				<goodsItem>
					<goodsItemNumber>111</goodsItemNumber>
					<packaging>
						<shippingMarks>marks</shippingMarks>
						<typeOfPackages>12</typeOfPackages>
					</packaging>
					<packaging>
						<shippingMarks>marks1</shippingMarks>
						<typeOfPackages>13</typeOfPackages>
					</packaging>
					<packaging>
						<shippingMarks>marks2</shippingMarks>
						<typeOfPackages>14</typeOfPackages>
					</packaging>
					<transportEquipment>
						<containerIdentificationNumber>123456</containerIdentificationNumber>
					</transportEquipment>
					<transportEquipment>
						<containerIdentificationNumber>123457</containerIdentificationNumber>
					</transportEquipment>
					<transportEquipment>
						<containerIdentificationNumber>123458</containerIdentificationNumber>
					</transportEquipment>
				</goodsItem>
                <consignmentHouseLevel>
					<goodsItem>
						<goodsItemNumber>111</goodsItemNumber>
						<packaging>
							<shippingMarks>marks</shippingMarks>
							<typeOfPackages>12</typeOfPackages>
						</packaging>
						<packaging>
							<shippingMarks>marks1</shippingMarks>
							<typeOfPackages>13</typeOfPackages>
						</packaging>
						<packaging>
							<shippingMarks>marks2</shippingMarks>
							<typeOfPackages>14</typeOfPackages>
						</packaging>
						<transportEquipment>
							<containerIdentificationNumber>123456</containerIdentificationNumber>
						</transportEquipment>
					</goodsItem>
					<passiveBorderTransportMeans>
						<identificationNumber>789</identificationNumber>
						<typeOfIdentificaiton>99</typeOfIdentificaiton>
						<nationality>DE</nationality>
					</passiveBorderTransportMeans>
                    <transportEquipment>
                        <containerIdentificationNumber>123456</containerIdentificationNumber>
                    </transportEquipment>
                    <transportEquipment>
                        <containerIdentificationNumber>123457</containerIdentificationNumber>
                    </transportEquipment>
                    <transportEquipment>
                        <containerIdentificationNumber>123458</containerIdentificationNumber>
                    </transportEquipment>
                    <transportDocumentHouseLevel>
                        <documentNumber>documentNumber1</documentNumber>
                        <type>C665</type>
                    </transportDocumentHouseLevel>
                </consignmentHouseLevel>
				<packaging>
					<shippingMarks>marks</shippingMarks>
					<typeOfPackages>12</typeOfPackages>
				</packaging>
				<transportEquipment>
					<containerIdentificationNumber>123456</containerIdentificationNumber>
				</transportEquipment>
            </consignmentMasterLevel>
        </controlSubject>
    </control>
    <declarant>
		<identificationNumber>DE08EORI1000003</identificationNumber>
	</declarant>
    <personNotifyingTheArrival>
        <identificationNumber>person123</identificationNumber>
    </personNotifyingTheArrival>
    <notifyParty>
        <identificationNumber>ID1234</identificationNumber>
    </notifyParty>
</IE3N08>
", masterReferenceNumber);
			return incomingMessage;
		}

		protected override IE3N08MessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new IE3N08MessageProcessor(logger);
		}

		protected override void TestProcessMessageCore_AdditionalAssertion(AsycudaManifestHeader manifestHeader)
		{
			base.TestProcessMessageCore_AdditionalAssertion(manifestHeader);
			AssertEquals("Customs status on manifest header should be updated to CNR", "CNR", manifestHeader.RegistrationStatus);
			var eventsOnParent = manifestHeader.Logs.Find(log => log.SL_SE_NKEvent == Events.StatusChange.Code && log.SL_Reference == $"REG to CNR - ENS Control Notification Received").FirstOrDefault();
			AssertNotNull(eventsOnParent);
		}
	}
}
