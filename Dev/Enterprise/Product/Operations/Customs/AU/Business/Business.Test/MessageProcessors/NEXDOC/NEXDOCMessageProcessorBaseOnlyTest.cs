using System.Linq;
using System.Xml;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.AU.Declaration.Business.RexOwnershipSoap;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(NEXDOCMessageProcessorForTest))]
	sealed class NEXDOCMessageProcessorBaseOnlyTest : NEXDOCMessageProcessorAbstractTest
	{
		public void TestEmailGroups()
		{
			SetupEmailGroups();

			var processor = new NEXDOCMessageProcessorForTest(new LoggingInformation());
			AssertEquals(emailGroup.PK, processor.AcknowledgementEmailGroup);
			AssertEquals(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, processor.AcknowledgementEmailMode);
			AssertEquals(emailGroup.PK, processor.ImpedimentEmailGroup);
			AssertEquals(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, processor.ImpedimentEmailMode);
			AssertEquals(emailGroup.PK, processor.ErrorEmailGroup);
			AssertEquals(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, processor.ErrorEmailMode);
		}

		public void TestGetInterchangeHeaderFromMessage()
		{
			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(NEXDOCUniversalInterchange);

			var messageProcessor = new NEXDOCMessageProcessorForTest(new LoggingInformation());
			var interchangeHeader = messageProcessor.GetInterchangeHeaderFromMessage(message);

			AssertEquals("SenderID", "NEXDOCSTest", interchangeHeader.SenderID);
			AssertEquals("RecipientID", "HYEDAUCM2", interchangeHeader.RecipientID);
			AssertEquals("REX Number", "REX0000044172", interchangeHeader.DeliveryMetadata.ValueCollection.First(x => x.Name == "RexNumber").Data);
		}

		public void TestGetRexNumberFromInterchangeHeader()
		{
			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(NEXDOCUniversalInterchange);

			var messageProcessor = new NEXDOCMessageProcessorForTest(new LoggingInformation());
			AssertEquals("REX Number", "REX0000044172", messageProcessor.GetRexNumberFromInterchangeHeader(message));
		}

		public void TestProcessMessage()
		{
			SetupEmailGroups();

			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(NEXDOCUniversalInterchange);
			Factory.Save();

			var messageProcessor = new NEXDOCMessageProcessorForTest(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			AssertEquals("message.EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("No Email generated.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestProcessMessage_SendsEmailOnError_InvalidDocumentType()
		{
			SetupEmailGroups();

			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(
@"<UniversalInterchange>
	<Header />
	<Body />
</UniversalInterchange>");
			message.EM_MessageText = @"<SomeOtherResponse />";
			Factory.Save();

			var messageProcessor = new NEXDOCMessageProcessorForTest(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			AssertEquals("message.EM_Status", EDIMessage.Status.Error, message.EM_Status);

			var responseEmail = Env.OutgoingMailManager.EmailsCreated.Last();
			AssertEquals("Email Subject", "ERROR PROCESSING", responseEmail.Subject);
			AssertContains("Email Body",
@"FATAL PROCESSING ERROR: Invalid RexAcknowledgeOwnershipResponse. Cannot Process.

<SomeOtherResponse />", responseEmail.Body);
		}

		public void TestProcessMessage_SendsEmailOnError_XmlError()
		{
			SetupEmailGroups();

			var message = CreateNEXDOCInterchangeAndMessageFromUniversalXML(
@"<UniversalInterchange>
	<Header />
	<Body />
</UniversalInterchange>");
			message.EM_MessageText = "<n1:RexAcknowledgeOwnershipResponse />";
			Factory.Save();

			var messageProcessor = new NEXDOCMessageProcessorForTest(new LoggingInformation());
			messageProcessor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			AssertEquals("message.EM_Status", EDIMessage.Status.Error, message.EM_Status);

			var responseEmail = Env.OutgoingMailManager.EmailsCreated.Last();
			AssertEquals("Email Subject", "ERROR PROCESSING", responseEmail.Subject);
			AssertContains("Email Body",
@"FATAL PROCESSING ERROR: Corrupted or Malformed Response Message. Cannot Process.
'n1' is an undeclared prefix. Line 1, position 2.

<n1:RexAcknowledgeOwnershipResponse />", responseEmail.Body);
		}

		const string NEXDOCUniversalInterchange =
@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
		<SenderID>NEXDOCSTest</SenderID>
		<RecipientID>HYEDAUCM2</RecipientID>
		<DeliveryMetadata>
			<ValueCollection>
				<Value>
					<Name>RexNumber</Name>
					<Type>String</Type>
					<Data>REX0000044172</Data>
				</Value>
			</ValueCollection>
		</DeliveryMetadata>
	</Header>
	<Body xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
		<ns1:RexAcknowledgeOwnershipResponse xmlns:ns1=""http://agriculture.gov.au/nexdoc/RexOwnershipSoap_1.0"" xmlns:ns0=""http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"">
			<ns1:outcome>CLOSED_ACCEPTED</ns1:outcome>
		</ns1:RexAcknowledgeOwnershipResponse>
	</Body>
</UniversalInterchange>";
	}

	sealed class NEXDOCMessageProcessorForTest : NEXDOCMessageProcessor<RexAcknowledgeOwnershipResponse>
	{
		public NEXDOCMessageProcessorForTest(LoggingInformation logger)
			: base(logger)
		{
		}

		public override bool HandlesDocument(string documentType) => true;

		protected override void ProcessMessageInternal(EDIMessage message, RexAcknowledgeOwnershipResponse response)
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(message.EM_MessageText);
		}

		public new ZString GetRexNumberFromInterchangeHeader(EDIMessage message) => base.GetRexNumberFromInterchangeHeader(message);

		public new NEXDOCAcknowledgeInterchangeHeader GetInterchangeHeaderFromMessage(EDIMessage message) => base.GetInterchangeHeaderFromMessage(message);

		public new ZGuid AcknowledgementEmailGroup => base.AcknowledgementEmailGroup;

		public new ZString AcknowledgementEmailMode => base.AcknowledgementEmailMode;

		public new ZGuid ImpedimentEmailGroup => base.ImpedimentEmailGroup;

		public new ZString ImpedimentEmailMode => base.ImpedimentEmailMode;

		public new ZGuid ErrorEmailGroup => base.ErrorEmailGroup;

		public new ZString ErrorEmailMode => base.ErrorEmailMode;
	}
}
