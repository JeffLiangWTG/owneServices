using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business.RexOwnershipSoap;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class TransferForwardBaseProcessorAbstractTest : NEXDOCMessageProcessorAbstractTest
	{
		public void TestCLOSED_ACCEPTED()
		{
			AssertOwnershipResponseSetsEntryStatus(RexOwnershipOutcomeType.CLOSED_ACCEPTED, NEXDOCMessageStatus.Codes.ClosedAccepted);
		}

		public void TestCLOSED_REJECTED()
		{
			AssertOwnershipResponseSetsEntryStatus(RexOwnershipOutcomeType.CLOSED_REJECTED, NEXDOCMessageStatus.Codes.ClosedRejected);
		}

		public void TestCLOSED_WITHDRAWN()
		{
			AssertOwnershipResponseSetsEntryStatus(RexOwnershipOutcomeType.CLOSED_WITHDRAWN, NEXDOCMessageStatus.Codes.ClosedWithdrawn);
		}

		public void TestFORWARD_ON_HOLD()
		{
			AssertOwnershipResponseSetsEntryStatus(RexOwnershipOutcomeType.FORWARD_ON_HOLD, NEXDOCMessageStatus.Codes.ForwardOnHold);
		}

		public void TestOPEN_PENDING()
		{
			AssertOwnershipResponseSetsEntryStatus(RexOwnershipOutcomeType.OPEN_PENDING, NEXDOCMessageStatus.Codes.OpenPending);
		}

		public void AssertOwnershipResponseSetsEntryStatus(RexOwnershipOutcomeType outcomeType, string expectedEntryStatus)
		{
			SetupEmailGroups();

			var rexNumber = "REX001234";
			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(
$@"<?xml version=""1.0"" encoding=""UTF-8""?>
<n1:UniversalInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
xmlns:n1=""http://www.cargowise.com/Schemas/Universal/2011/11"" 
xsi:schemaLocation=""http://www.cargowise.com/Schemas/Universal/2011/11 UniversalInterchange.xsd"">
	<n1:Header>
		<n1:SenderID>NEXDOCSTest</n1:SenderID>
		<n1:RecipientID>HYEDAUCM2</n1:RecipientID>
		<n1:DeliveryMetadata>
			<n1:ValueCollection>
				<n1:Value>
					<n1:Name>RexNumber</n1:Name>
					<n1:Type>String</n1:Type>
					<n1:Data>{rexNumber}</n1:Data>
				</n1:Value>
			</n1:ValueCollection>
		</n1:DeliveryMetadata>
	</n1:Header>
	<n1:Body xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
		<ns1:{OwnershipResponseType} xmlns:ns1=""http://agriculture.gov.au/nexdoc/RexOwnershipSoap_1.0"" xmlns:ns0=""http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"">
			<ns1:outcome>{outcomeType}</ns1:outcome>
		</ns1:{OwnershipResponseType}>
	</n1:Body>
</n1:UniversalInterchange>");

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Quarantine;
			invoice.JZ_InvoiceNumber = "INV001";
			invoice.JZ_ExporterReference = "EXPREF001";
			invoice.QuarantineExDocHeader.QH_RequestForPermitNumber = rexNumber;

			var declaration = Factory.New<JobDeclaration>();
			invoice.JZ_JE = declaration.PK;
			Factory.Save();

			OwnershipResponseProcessor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			AssertEquals("Message Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("QuarantineExDocHeader.RFPNumber.CE_EntryStatus", expectedEntryStatus, invoice.QuarantineExDocHeader.RequestForPermitStatus);
			Assert("Message is on QuarantineExDocHeader", invoice.QuarantineExDocHeader.Messages.Any(x => x.PK == message.PK));

			var responseEmail = Env.OutgoingMailManager.EmailsCreated.Last();
			AssertEquals("Response Email Subject", "NEXDOC Notification Advice", responseEmail.Subject);
			AssertContains("Response Email Body contains Status", $"A {EmailTitle} for REX {rexNumber} has been received", responseEmail.Body);
			AssertContains("Response Email Body contains Status", "REX Number: " + rexNumber, responseEmail.Body);
			AssertContains("Response Email Body contains Status", "Exporter Reference: EXPREF001", responseEmail.Body);
			AssertContains("Response Email Body contains Status", "Outcome: " + outcomeType, responseEmail.Body);
		}

		public void TestProcessingError_UnexpectedRexNumber()
		{
			SetupEmailGroups();

			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(
$@"<?xml version=""1.0"" encoding=""UTF-8""?>
<n1:UniversalInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
xmlns:n1=""http://www.cargowise.com/Schemas/Universal/2011/11"" 
xsi:schemaLocation=""http://www.cargowise.com/Schemas/Universal/2011/11 UniversalInterchange.xsd"">
	<n1:Header>
		<n1:SenderID>NEXDOCSTest</n1:SenderID>
		<n1:RecipientID>HYEDAUCM2</n1:RecipientID>
		<n1:DeliveryMetadata>
			<n1:ValueCollection>
				<n1:Value>
					<n1:Name>RexNumber</n1:Name>
					<n1:Type>String</n1:Type>
					<n1:Data>REX1111</n1:Data>
				</n1:Value>
			</n1:ValueCollection>
		</n1:DeliveryMetadata>
	</n1:Header>
	<n1:Body xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
		<ns1:{OwnershipResponseType} xmlns:ns1=""http://agriculture.gov.au/nexdoc/RexOwnershipSoap_1.0"" xmlns:ns0=""http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"">
			<ns1:outcome>CLOSED_ACCEPTED</ns1:outcome>
		</ns1:{OwnershipResponseType}>
	</n1:Body>
</n1:UniversalInterchange>");
			Factory.Save();

			OwnershipResponseProcessor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			AssertEquals("message.EM_Status", EDIMessage.Status.Error, message.EM_Status);

			var responseEmail = Env.OutgoingMailManager.EmailsCreated.Last();
			AssertEquals("Response Email Subject", "ERROR PROCESSING", responseEmail.Subject);
			AssertContains("Response Email Body contains Status", "FATAL PROCESSING ERROR: Unexpected or Invalid Rex Number 'REX1111'. Cannot Process.", responseEmail.Body);
		}

		protected abstract ZString EmailTitle { get; }

		protected abstract NEXDOCMessageProcessor OwnershipResponseProcessor { get; }

		protected abstract string OwnershipResponseType { get; }
	}
}
