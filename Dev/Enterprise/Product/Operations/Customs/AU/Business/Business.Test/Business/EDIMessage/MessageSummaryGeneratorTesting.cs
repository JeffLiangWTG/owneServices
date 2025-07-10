using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class MessageSummaryGeneratorTesting : TestCaseWithFactory
	{
		public void TestGetMessageSummary()
		{
			var rfpMessage = Factory.New<RFPEDIMessage>();
			rfpMessage.EM_MessageText = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataProvider>NEXDOCS</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>B00001307</Key>
					<Type>CustomsDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2017-09-29T03:20:49Z</EventTime>
		<EventType>MRR</EventType>
		<EventReference>MST=NOTIF</EventReference>
		<ContextCollection>
			<Context>
				<Type>NotificationDate</Type>
				<Value>2018-01-18T07:51:44.219+11:00</Value>
			</Context>
			<Context>
				<Type>NotificationTitle</Type>
				<Value>Certificate Notification: REX0000012476</Value>
			</Context>
			<Context>
				<Type>NotificationText</Type>
				<Value>Certification for the REX issued!</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var testResult = MessageSummaryGenerator.GetMessageSummary(rfpMessage);
			AssertEquals("REQ1, received notifications",
				@"Notification Title: Certificate Notification: REX0000012476
Notification Text: Certification for the REX issued!",
				testResult);

			rfpMessage.EM_MessageText = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataProvider>NEXDOCS</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>B00001307</Key>
					<Type>CustomsDeclaration</Type>	
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2017-09-29T03:20:49Z</EventTime>
		<EventType>MRR</EventType> 
		<EventReference>MST=LODGE</EventReference>
		<ContextCollection>
			<Context>
				<Type>MessageStatus</Type>
				<Value>ERO</Value>
			</Context>
			<Context>
				<Type>message</Type>
				<Value>Error in operation:</Value>
			</Context>
			<Context>
				<Type>Message</Type>
				<Value>OSB Validate action failed validation</Value>
			</Context>
			<Context>
				<Type>message</Type>
				<Value>Invalid date value: 2017-07-43</Value>
			</Context>
			<Context>
				<Type>message</Type>
				<Value>Expected element 'ownerExporterId@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0' before the end of the content in element exporterDetails@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0</Value>
			</Context>
			<Context>
				<Type>message</Type>
				<Value>Expected element 'productType@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0' instead of 'category@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0' here in element productDetails@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			testResult = MessageSummaryGenerator.GetMessageSummary(rfpMessage);
			AssertEquals("REQ2, received error messages",
				@"Error
Error in operation:
OSB Validate action failed validation
Invalid date value: 2017-07-43
Expected element 'ownerExporterId@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0' before the end of the content in element exporterDetails@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0
Expected element 'productType@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0' instead of 'category@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0' here in element productDetails@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0",
				testResult);

			var rexMessage = Factory.New<REXMessage>();

			rexMessage.EM_MessageText = @"<NST1:RexAcknowledgeOwnership xmlns:NST2=""http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"" xmlns:NST1=""http://agriculture.gov.au/nexdoc/RexOwnershipSoap_1.0"">
  <NST1:identification>
    <NST2:rexNumber>REX00001</NST2:rexNumber>
  </NST1:identification>
  <NST1:isAccepted>true</NST1:isAccepted>
</NST1:RexAcknowledgeOwnership>";
			testResult = MessageSummaryGenerator.GetMessageSummary(rexMessage);
			AssertEquals("REQ3, sent Acknowledge", "REX00001 Acknowledgement Accepted", testResult);

			rexMessage.EM_MessageText = @"<ns1:RexAcknowledgeOwnershipResponse xmlns:ns1=""http://agriculture.gov.au/nexdoc/RexOwnershipSoap_1.0"" xmlns:ns0=""http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"">
	<ns1:outcome>CLOSED_ACCEPTED</ns1:outcome>
</ns1:RexAcknowledgeOwnershipResponse>";
			testResult = MessageSummaryGenerator.GetMessageSummary(rexMessage);
			AssertEquals("REQ4, Acknowledge response", "Message Status: CLOSED_ACCEPTED", testResult);
		}
	}
}
