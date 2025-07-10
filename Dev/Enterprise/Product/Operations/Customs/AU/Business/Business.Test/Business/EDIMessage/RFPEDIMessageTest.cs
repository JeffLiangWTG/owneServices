using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using ReceiveTransmitList = Enterprise.Messaging.Integration.ReceiveTransmitList.Codes;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(RFPEDIMessage))]
	class RFPEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEM_MessageInterpretation()
		{
			var message = Factory.New<RFPEDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Receive;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_MessageText = UniversalEvent_Error;
			message.EM_MessageInterpretation = "123";
			AssertContains(InterpretatedHTML_Error, message.EM_MessageInterpretation);

			message.EM_ReceiveTransmit = ReceiveTransmitList.Transmit;
			AssertEquals("123", message.EM_MessageInterpretation);
		}

		const string InterpretatedHTML_Error = @"<body><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">"
			+ "<tr><td>"
				+ "<b>Job Number: B00001307</b><br /><br />"
				+ "A response message has been received from Quarantine.<br />"
				+ "Shown below is a summary of relevant information received in this message.<br /><br />"
				+ @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">"
					+ @"<thead><tr class=""tableheadings""><th>Message</th></tr></thead>"
					+ "<tr><td>Error in operation:</td></tr>"
					+ "<tr><td>OSB Validate action failed validation</td></tr>"
					+ "<tr><td>Invalid date value: 2017-07-43</td></tr>"
					+ "<tr><td>Expected element 'ownerExporterId@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0' before the end of the content in element exporterDetails@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0</td></tr><tr><td>Expected element 'productType@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0' instead of 'category@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0' here in element productDetails@http://agriculture.gov.au/nexdoc/common/rex/CommonTypes_1.0</td></tr>"
				+ "</table><br />"
				+ "Regards,<br /><br />"
				+ "CargoWise Administrative Message Sender<br />"
			+ "</td></tr>"
			+ "</table><body>";

		const string UniversalEvent_Error = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
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
	}
}
