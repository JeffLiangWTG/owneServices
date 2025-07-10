using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class RFPMessageInterpretationGeneratorTest : TestCaseWithFactory
	{
		public void TestGetInterpretatedHTML_Failure()
		{
			var message = Factory.New<RFPEDIMessage>();
			AssertContains(RFPMessageInterpretationGenerator.ParsingFailureResult, RFPMessageInterpretationGenerator.GetInterpretatedHTML(message));
		}

		public void TestGetInterpretatedHTML_Error()
		{
			var message = Factory.New<RFPEDIMessage>();
			message.EM_MessageText = UniversalEvent_Error;
			AssertContains(InterpretatedHTML_Error, RFPMessageInterpretationGenerator.GetInterpretatedHTML(message));
		}

		public void TestGetInterpretatedHTML_Notify()
		{
			var message = Factory.New<RFPEDIMessage>();
			message.EM_MessageText = UniversalEvent_Notify;
			AssertContains(InterpretatedHTML_Notify, RFPMessageInterpretationGenerator.GetInterpretatedHTML(message));
		}

		public void TestGetInterpretatedHTML_Success()
		{
			var message = Factory.New<RFPEDIMessage>();
			message.EM_MessageText = UniversalEvent_Success;
			AssertContains(InterpretatedHTML_Success, RFPMessageInterpretationGenerator.GetInterpretatedHTML(message));
		}

		public void TestGetInterpretatedHTML_Success_NoMessages()
		{
			var message = Factory.New<RFPEDIMessage>();
			message.EM_MessageText = UniversalEvent_Success_NoMessages;
			AssertContains(InterpretatedHTML_Success_NoMessages, RFPMessageInterpretationGenerator.GetInterpretatedHTML(message));
		}

		public void TestGetInterpretatedHTML_ReissueReceip()
		{
			var message = Factory.New<RFPEDIMessage>();
			message.EM_MessageText = UniversalEvent_ReissueReceipt;
			AssertContains(InterpretatedHTML_ReissueReceipt, RFPMessageInterpretationGenerator.GetInterpretatedHTML(message));
		}

		public void TestGetInterpretatedHTML_ReplaceReceipt()
		{
			var message = Factory.New<RFPEDIMessage>();
			message.EM_MessageText = UniversalEvent_ReplaceReceipt;
			AssertContains(InterpretatedHTML_ReplaceReceipt, RFPMessageInterpretationGenerator.GetInterpretatedHTML(message));
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

		const string InterpretatedHTML_Notify = @"<body><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">"
			+ "<tr><td>"
				+ "<b>Job Number: B00001307</b><br /><br />"
				+ "A Notification response message has been received from Quarantine.<br />"
				+ "Shown below is a summary of relevant information received in this message.<br /><br />"
					+ @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">"
						+ @"<thead><tr class=""tableheadings""><th>Notification</th></tr></thead>"
						+ "<tr><td>Certificate Notification: REX0000012476</td></tr>"
						+ "<tr><td>Certification for the REX issued!</td></tr>"
					+ "</table><br />"
					+ "Regards,<br /><br />"
					+ "CargoWise Administrative Message Sender<br />"
			+ "</td></tr>"
			+ "</table><body>";

		const string InterpretatedHTML_Success = @"<body><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">"
			+ "<tr><td>"
				+ "<b>Job Number: B00001307</b><br />"
				+ "<b>RFP Number: REX0000013011</b><br />"
				+ "<b>RFP Status: HCRD - Health Certificate Ready</b><br />"
				+ "<b>Permit Number: PID0000085</b><br />"
				+ "<b>Customs Authority Number: XXXX</b><br />"
				+ "<br />"
				+ "A response message has been received from Quarantine.<br />"
				+ "Shown below is a summary of relevant information received in this message.<br /><br />"
				+ @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">"
					+ @"<thead><tr class=""tableheadings""><th>Message</th></tr></thead>"
					+ "<tr><td>StringValue1</td></tr>"
					+ "<tr><td>StringValue2</td></tr>"
				+ "</table><br />"
				+ "Regards,<br /><br />"
				+ "CargoWise Administrative Message Sender<br />"
			+ "</td></tr>"
			+ "</table><body>";

		const string InterpretatedHTML_Success_NoMessages = @"<body><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">"
			+ "<tr><td>"
				+ "<b>Job Number: B00001307</b><br />"
				+ "<b>RFP Number: REX0000013011</b><br />"
				+ "<br />"
				+ "A response message has been received from Quarantine.<br />"
				+ "<br />"
				+ "Regards,<br /><br />"
				+ "CargoWise Administrative Message Sender<br />"
			+ "</td></tr>"
			+ "</table><body>";

		const string InterpretatedHTML_ReissueReceipt = @"<body><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">"
			+ "<tr><td>"
				+ "<b>Job Number: B00001307</b><br />"
				+ "<br />"
				+ @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">"
				+ @"<thead><tr class=""tableheadings""><th>Message</th></tr></thead>"
				+ "<tr><td>Certificate number AU0000022541 has been submitted for reissue.</td></tr>"
				+ "</table><br />"
				+ "Regards,<br /><br />"
				+ "CargoWise Administrative Message Sender<br />"
			+ "</td></tr>"
			+ "</table><body>";

		const string InterpretatedHTML_ReplaceReceipt = @"<body><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">"
			+ "<tr><td>"
				+ "<b>Job Number: B60004549</b><br />"
				+ "<br />"
				+ "A Notification response message has been received from Quarantine.<br />"
				+ "Shown below is a summary of relevant information received in this message.<br /><br />"
				+ @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">"
				+ @"<thead><tr class=""tableheadings""><th>Notification</th></tr></thead>"
				+ "<tr><td>Replacement Service Request Id - 02221004827954</td></tr>"
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

		const string UniversalEvent_Notify = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
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

		const string UniversalEvent_Success = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
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
				<Type>PermitNumber</Type>
				<Value>PID0000085</Value>
			</Context>
			<Context>
				<Type>RexNumber</Type>
				<Value>REX0000013011</Value>
			</Context>
			<Context>
				<Type>ComplianceStatus</Type>
				<Value>HCRD</Value>
			</Context>
			<Context>
				<Type>ECNNumber</Type>
				<Value>XXXX</Value>
			</Context>
			<Context>
				<Type>LastAmendDateTime</Type>
				<Value>2018-01-25T14:36:37.810+11:00</Value>
			</Context>
			<Context>
				<Type>ValidationNotice</Type>
				<Value>Notice</Value>
				<SubContextCollection>
					<SubContext>
						<Type>NoticeID</Type>
						<Value>StringValue</Value>
					</SubContext>
					<SubContext>
						<Type>NoticeType</Type>
						<Value>I</Value>
					</SubContext>
					<SubContext>
						<Type>NoticeMessage</Type>
						<Value>StringValue1</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>ValidationNotice</Type>
				<Value>Notice</Value>
				<SubContextCollection>
					<SubContext>
						<Type>NoticeID</Type>
						<Value>StringValue</Value>
					</SubContext>
					<SubContext>
						<Type>NoticeType</Type>
						<Value>I</Value>
					</SubContext>
					<SubContext>
						<Type>NoticeMessage</Type>
						<Value>StringValue2</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>rexResponseType</Type>
				<Value>AP</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		const string UniversalEvent_Success_NoMessages = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
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
				<Type>RexNumber</Type>
				<Value>REX0000013011</Value>
			</Context>
			<Context>
				<Type>LastAmendDateTime</Type>
				<Value>2018-01-25T14:36:37.810+11:00</Value>
			</Context>
			<Context>
				<Type>RexResponseType</Type>
				<Value>AP</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		const string UniversalEvent_ReissueReceipt = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
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
        <EventTime>2021-03-31T20:07:07</EventTime>
        <EventType>MRR</EventType>
        <EventReference>MST=REISSUE</EventReference>
        <ContextCollection>
          <Context>
            <Type>Message</Type>
            <Value>Certificate number AU0000022541 has been submitted for reissue.</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>";

		const string UniversalEvent_ReplaceReceipt = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	 <Event>
		<DataContext>
			<DataProvider>NEXDOCS</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>B60004549</Key>
					<Type>CustomsDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
		<DataSourceCollection>
		<DataSource>
		<Type/>
		</DataSource>
		</DataSourceCollection>
		</DataContext>
		<EventTime>2022-08-10T00:04:06</EventTime>
		<EventType>MRR</EventType>
		<EventReference>MST=REPLACE</EventReference>
		<ContextCollection>
			<Context>
				<Type>Message</Type>
				<Value>02221004827954</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		public void TestGeRexCanResponseMessages()
		{
			var regCanResponseText = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Event>
		<DataContext>
			<DataProvider>NEXDOCS</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>B60002824</Key>
					<Type>CustomsDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2021-01-04T21:28:30</EventTime>
		<EventType>MRR</EventType>
		<EventReference>MST=CANREX</EventReference>
		<ContextCollection>
			<Context>
				<Type>RexNumber</Type>
				<Value>REX6000003316</Value>
			</Context>
			<Context>
				<Type>Message</Type>
				<Value>Rex id REX6000003316: Your request for REX cancellation has been received.</Value>
			</Context>
			<Context>
				<Type>ServiceRequestIdentifier</Type>
				<Value>02211000143852</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var message = Factory.New<RFPEDIMessage>();
			message.EM_MessageText = regCanResponseText;
			var interpretation = RFPMessageInterpretationGenerator.GetInterpretatedHTML(message);
			AssertContains("<tr><td>Rex id REX6000003316: Your request for REX cancellation has been received.</td></tr>", interpretation);
			AssertContains("<tr><td>Service Request Identifier: 02211000143852</td></tr>", interpretation);
		}
	}
}
