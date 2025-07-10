using System;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.Testing;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Edifact.D96A.Elements;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class EDIReleaseResponseMessageProcessorTest : EDIFACTMessageProcessorTest
	{
		public void TestRNSResponsesNotMatchOriginalTransactionNumer()
		{
			var request = GetRNSRequest("CCN1234561", RNSMessageTypes.Codes.ArrivalCertification, "10207000008576");
			request.EM_ApplicationReference = ZString.Empty;
			request.EM_MessageOwner = ZString.Empty;
			request.EM_LinkUniqueID = entryHeader.PK;
			request.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			request.EM_SystemCreateUser = userToNotify.GS_Code;
			Factory.Save();

			const string errorMessage = "Could not find an associated business object (Job) for document reference = '10207000008576'";

			string messageText = ZString.Format(@"UNH+1+CUSRES:D:96A:UN'
BGM+:::489+10207000008576+11'
DTM+9:201101130748:203'
GIS+2'
ERP+2:{0}'
ERC+34'
RFF+XC:CCN1234561'
UNT+8+1'", request.EM_MessageNum);

			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);
			AssertContains("EM_MessageInterpretation", errorMessage, ediMessage.EM_MessageInterpretation);
			AssertContains("could not find object when TransactionNumber not match", "EDI Release Response Message Processor: " + errorMessage, logger.DebugLogStrings[0]);
		}

		public void TestCannotMatchIfValidTransactionNumberNotMatch()
		{
			var declaration = (JobDeclaration)JobDeclaration.New(Factory);
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "10006789";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ZString.Empty;
			declaration.JE_IsCancelled = false;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

			var declarationCCN = declaration.AdditionalReferenceNumbers.AddNew();
			declarationCCN.CE_EntryNum = "CCN123856";
			declarationCCN.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			declarationCCN.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			Factory.Save();

			const string messageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::125+12345110067897+11'
LOC+22+0495'
DTM+58:201006221028:203'
GIS+14'
RFF+CN:CCN123856'
UNT+7+1'";
			var releaseDate = new ZDateTime(2010, 06, 22, 10, 28, 00);
			entryHeader.CH_Status = MessageStatusList.Codes.ClearOriginal;
			Factory.Save();

			const string errorMessage = "Could not find an associated business object (Job) for document reference = '12345110067897'";

			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals(EDIMessage.Status.Failed, ediMessage.EM_Status);
			AssertContains("EM_MessageInterpretation", errorMessage, ediMessage.EM_MessageInterpretation);
			AssertContains("could not find object when TransactionNumber not match", "EDI Release Response Message Processor: " + errorMessage, logger.DebugLogStrings[0]);
		}

		public void TestErrorResponseWithoutCCNAndTransactionNumber()
		{
			var request = GetRNSRequest("CCN1234561", RNSMessageTypes.Codes.ArrivalCertification, "9463TKWB1438742C");
			request.EM_ApplicationReference = ZString.Empty;
			request.EM_MessageOwner = ZString.Empty;
			request.EM_SystemCreateUser = userToNotify.GS_Code;
			Factory.Save();

			string messageText = ZString.Format(@"UNH+1+CUSRES:D:96A:UN'
BGM+:::489+9463TKWB1438742C+11'
DTM+9:201101130748:203'
GIS+2'
ERP+2:{0}'
ERC+34'
RFF+XC:CCN1234561'
UNT+8+1'", request.EM_MessageNum);

			const string expectedBody = @"</strong>An ERROR response message has been received from the CBSA for a Warehouse Arrival Certification Message.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />

<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Code</th><th>Error Text</th></tr></thead><tr><td>34</td><td>Client Supplied Request ID: Request already in arrived status</td></tr></table>";

			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEmail("Error Warehouse Arrival Certification Message Response for CCN1234561", expectedBody, messageText.Replace("\r\n", ""), string.Empty);
		}

		public void TestErrorResponseGetReferenceFromLinkedObject()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_IsCancelled = false;

			var request = Factory.New<RNSRequestMessage>();
			request.EM_MessageSubType = RNSMessageTypes.Codes.ArrivalCertification;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_Status = EDIMessage.Status.Sent;
			request.EM_SystemCreateTimeUtc = ZDateTime.Now;
			request.EM_MessageText = string.Format("UNH+{0}+CUSREP:D:96A:UN'BGM+998'DTM+132:201009200644:203'UNT+4+145'", EDIMessage.MessageNumberPlaceHolder);
			request.EM_LinkUniqueID = shipment.PK;
			request.EM_LinkTable = "JobShipment";
			Factory.Save();
			string messageText = ZString.Format(@"UNH+1+CUSRES:D:96A:UN'
BGM+:::489+9463TKWB1438742C+11'
DTM+9:201101130748:203'
GIS+2'
ERP+2:{0}'
ERC+34'
RFF+XC:CCN1234561'
UNT+8+1'", request.EM_MessageNum);

			const string expectedBody = @"</strong>An ERROR response message has been received from the CBSA for a Warehouse Arrival Certification Message.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />

<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Code</th><th>Error Text</th></tr></thead><tr><td>34</td><td>Client Supplied Request ID: Request already in arrived status</td></tr></table>";

			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEmail(string.Format("Error Warehouse Arrival Certification Message Response for {0}", shipment.JS_UniqueConsignRef), expectedBody, messageText.Replace("\r\n", ""), string.Empty);
		}

		public void TestMessageWithNoCSTSegment()
		{
			const string messageText = @"UNH+257+CUSRES:D:96A:UN'BGM+:::257+B99999999+11'RFF+XC:RC123420021100001'UNT+6+257'";
			const string errorMessage = "The message processor was unable to interpret received message";

			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals(EDIMessage.Status.Failed, ediMessage.EM_Status);
			AssertContains("EM_MessageInterpretation", errorMessage, ediMessage.EM_MessageInterpretation);
			AssertContains("LastLog", "EDI Release Response Message Processor: " + errorMessage, logger.DebugLogStrings[0]);
		}

		public void TestErrorResponseWithNoAssociatedRequestsAndLinkedObjects()
		{
			const string messageText = @"UNH+257+CUSRES:D:96A:UN'BGM+:::257+B99999999+11'DTM+9:201006221028:203'GIS+14'RFF+XC:RC123420021100001'UNT+6+257'";
			const string errorMessage = "Could not find an associated business object (Job) for document reference = ''";

			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals(EDIMessage.Status.Failed, ediMessage.EM_Status);
			AssertContains("EM_MessageInterpretation", errorMessage, ediMessage.EM_MessageInterpretation);
			AssertContains("LastLog", "EDI Release Response Message Processor: " + errorMessage, logger.DebugLogStrings[0]);
		}

		public void TestStatusUpdateWithNoAssociatedRequestsAndLinkedObjects()
		{
			const string messageText = @"UNH+257+CUSRES:D:96A:UN'BGM+:::257+B99999999+11'LOC+22+0351:129::3021'DTM+9:201006221028:203'GIS+9'RFF+XC:CCN888'UNT+6+257'";

			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			const string expectedBody = @"</strong>A 'Status Update' message has been received from the CBSA for an Across/IID EDI Release.<br />
Please see message details below.<br />
<br />NOTE: No job has been found that matches the reference details in this message.
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Indicator</td><td>9 - Declaration Accepted, Awaiting arrival of Goods, PARS is set - OK to cross the border</td></tr><tr><td>Service Option</td><td>257 - Post-arrival EDI Release</td></tr><tr><td>Document Reference</td><td>B99999999</td></tr><tr><td>Release Office Code</td><td>0351</td></tr><tr><td>Warehouse Code</td><td>3021</td></tr><tr><td>Cargo Control Number</td><td>CCN888</td></tr><tr><td>Processing Date</td><td>22-Jun-10 10:28</td></tr></table>
<br />";

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed to warehouse code associated branch", notifyBranch.PK, ediMessage.EM_GB);
			AssertNull("No linked object", ediMessage.EM_LinkedObject);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted, ediMessage.EM_MessageSubType);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEmail("Across/IID EDI Release Status Update Message for CCN888", expectedBody, messageText.Replace("\r\n", ""), string.Empty, ccRecipient: "blah@blah.com;staff1@wisetechglobal.com");
			AssertEquals("RNSProcessingDate", new ZDateTime(2010, 6, 22, 10, 28, 0), ediMessage.RNSProcessingDate);
			AssertEquals("RNSReleaseDate", ZDateTime.Empty, ediMessage.RNSReleaseDate);
		}

		public void TestLastSentRNSMessageWhenNoMatchedMessageButHasLinkedObject()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_IsCancelled = false;

			var request = GetRNSRequest("803688889999", RNSMessageTypes.Codes.ArrivalCertification);
			request.EM_MessageText = @"UNH+12150+CUSREP:D:96A:UN'BGM+631'DTM+132:202103231926:203'RFF+ABT:803688889999'LOC+14+0497:129::2005'UNT+6+12150'";
			request.EM_LinkUniqueID = shipment.PK;
			request.EM_LinkTable = "JobShipment";
			request.EM_SystemCreateUser = userToNotify.GS_Code;

			shipment.Messages.Add(request);

			var consol = Factory.New<ForwardingConsol>();
			var entryNumber = consol.Numbers.AddNew();
			entryNumber = consol.Numbers.AddNew();
			entryNumber.CE_EntryNum = "803688889999";
			entryNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			entryNumber = shipment.Numbers.AddNew();
			entryNumber.CE_EntryNum = "803688889999";
			entryNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			Factory.Save();
			const string messageText = @"UNH+257+CUSRES:D:96A:UN'BGM+:::257+B99999999+11'DTM+9:201006221028:203'GIS+14'
ERP+2:389'
ERC+30'
ERC+31'
ERC+32'
ERC+72'
ERC+73'
ERC+74'
ERC+79'
ERC+80'
RFF+XC:803688889999'UNT+6+257'";
			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			const string expectedBody = @"
Reference Number : 803688889999<br />
<br />
</strong>An ERROR response message has been received from the CBSA for a Warehouse Arrival Certification Message.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />

<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Code</th><th>Error Text</th></tr></thead><tr><td>30</td><td>Warehouse Office: Warehouse is mandatory</td></tr><tr><td>31</td><td>Client Supplied Request ID: Arrival is already on file with matching Port and Warehouse</td></tr><tr><td>32</td><td>Client Supplied Request ID: Arrival document accepted but no related document on file (Warning)</td></tr><tr><td>72</td><td>Warehouse Office: The Arrival Subloc provided to CBSA does not match the Port of Destination Subloc for the referenced Cargo document (Warning)</td></tr><tr><td>73</td><td>Arrival Document: The Arrival document has been expired</td></tr><tr><td>74</td><td>Work Location Code (Destination): Arrival Port provided to CBSA does not match the Port of Destination for the referenced Cargo document (Warning)</td></tr><tr><td>79</td><td>Warehouse office: Warehouse must be related to Port of Arrival provided</td></tr><tr><td>80</td><td>Warehouse office: Warehouse cannot be Customs Office</td></tr></table>
<br />
<hr />
<br />
<!--EndSection Details-->
Regards,<br />
<br />
CargoWise One Administrative Messages Sender<br />
<br />
<br />
<br />";

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertNotNull("Linked object", ediMessage.EM_LinkedObject);
			AssertContains("Across/IID EDI Release Status Update Message for S00001001", expectedBody, ediMessage.EM_MessageInterpretation);
		}

		public void TestNoErrorForAeroSpace()
		{
			const string messageText = @"UNH+1+CUSRES:D:96A:UN'BGM+:::34+12997555212742+11'LOC+22+0497'DTM+58:201205241526:203'GIS+4'RFF+XC:8036YAS13605340'UNT+7+1'";
			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);
			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
		}

		public void TestNoErrorForCSAEDIGighway()
		{
			const string messageText612 = @"UNH+1+CUSRES:D:96A:UN'BGM+:::612+20133498190472+11'LOC+22+0410:129:: 'DTM+9:202303181219:203'GIS+9'R";
			var ediMessage612 = GetEDIReleaseMessage(messageText612);
			impMessageProcessor.ProcessMessage(ediMessage612);
			AssertEquals("Received", EDIMessage.Status.Received, ediMessage612.EM_Status);

			const string messageText513 = @"UNH+1+CUSRES:D:96A:UN'BGM+:::513+20133498190472+11'LOC+22+0410:129:: 'DTM+9:202303181219:203'GIS+9'R";
			var ediMessage513 = GetEDIReleaseMessage(messageText513);
			impMessageProcessor.ProcessMessage(ediMessage513);
			AssertEquals("Received", EDIMessage.Status.Received, ediMessage513.EM_Status);
		}

		public void TestEmailTitleWithWACM()
		{
			var request = GetRNSRequest("CCN777", RNSMessageTypes.Codes.ArrivalCertification);
			request.EM_SystemCreateUser = userToNotify.GS_Code;
			Factory.Save();

			const string messageText = @"UNH+257+CUSRES:D:96A:UN'BGM+:::257+B99999999+11'DTM+9:201006221028:203'GIS+14'
ERP+2:389'
ERC+30'
ERC+31'
ERC+32'
ERC+72'
ERC+73'
ERC+74'
ERC+79'
ERC+80'
RFF+XC:CCN777'UNT+6+257'";
			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			const string expectedBody = @"<br />
<strong>Job Number : <br />
Reference Number : CCN777<br />
<br />
</strong>An ERROR response message has been received from the CBSA for a Warehouse Arrival Certification Message.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />

<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Code</th><th>Error Text</th></tr></thead><tr><td>30</td><td>Warehouse Office: Warehouse is mandatory</td></tr><tr><td>31</td><td>Client Supplied Request ID: Arrival is already on file with matching Port and Warehouse</td></tr><tr><td>32</td><td>Client Supplied Request ID: Arrival document accepted but no related document on file (Warning)</td></tr><tr><td>72</td><td>Warehouse Office: The Arrival Subloc provided to CBSA does not match the Port of Destination Subloc for the referenced Cargo document (Warning)</td></tr><tr><td>73</td><td>Arrival Document: The Arrival document has been expired</td></tr><tr><td>74</td><td>Work Location Code (Destination): Arrival Port provided to CBSA does not match the Port of Destination for the referenced Cargo document (Warning)</td></tr><tr><td>79</td><td>Warehouse office: Warehouse must be related to Port of Arrival provided</td></tr><tr><td>80</td><td>Warehouse office: Warehouse cannot be Customs Office</td></tr></table>
<br />
<hr />
<br />
<!--EndSection Details-->
Regards,<br />
<br />
CargoWise One Administrative Messages Sender<br />
<br />
<br />
<br />";
			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertNull("No linked object", ediMessage.EM_LinkedObject);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.Error, ediMessage.EM_MessageSubType);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
		}

		public void TestStatusUpdateWithAssociatedRequestWithNoLinkedObject()
		{
			var request = GetRNSRequest("CCN777", RNSMessageTypes.Codes.StatusQuery);
			request.EM_SystemCreateUser = userToNotify.GS_Code;
			Factory.Save();

			const string messageText = @"UNH+257+CUSRES:D:96A:UN'BGM+:::257+B99999999+11'DTM+9:201006221028:203'GIS+9'RFF+XC:CCN777'UNT+6+257'";
			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			const string expectedBody = @"</strong>A 'Status Update' message has been received from the CBSA for an Across/IID EDI Release.<br />
Please see message details below.<br />
<br />NOTE: No job has been found that matches the reference details in this message.
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Indicator</td><td>9 - Declaration Accepted, Awaiting arrival of Goods, PARS is set - OK to cross the border</td></tr><tr><td>Service Option</td><td>257 - Post-arrival EDI Release</td></tr><tr><td>Document Reference</td><td>B99999999</td></tr><tr><td>Cargo Control Number</td><td>CCN777</td></tr><tr><td>Processing Date</td><td>22-Jun-10 10:28</td></tr></table>
<br />";

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertNull("No linked object", ediMessage.EM_LinkedObject);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted, ediMessage.EM_MessageSubType);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);

			//TODO: Enhance the message processor to send notifications to the user who sends RNS request from Release Notifications module and remove the last parameter from the following assertion
			AssertEmail("Across/IID EDI Release Status Update Message for CCN777", expectedBody, messageText.Replace("\r\n", ""), string.Empty);
		}

		public void TestStatusUpdateWithMultipleAssociatedRequestsWithLinkedObjectsMatchedByCCN()
		{
			const string messageText = @"UNH+257+CUSRES:D:96A:UN'BGM+:::257+B99999999+11'DTM+9:201006221028:203'GIS+9'RFF+XC:CCN 123456'UNT+6+257'";
			const string declarationEmailBody = @"Reference Number : CCN123456<br />
<br />
</strong>A 'Message Accepted' response has been received from the CBSA for an Across/IID EDI Release.<br />
Please see message details below.<br />
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Indicator</td><td>9 - Declaration Accepted, Awaiting arrival of Goods, PARS is set - OK to cross the border</td></tr><tr><td>Service Option</td><td>257 - Post-arrival EDI Release</td></tr><tr><td>Document Reference</td><td>B99999999</td></tr><tr><td>Cargo Control Number</td><td>CCN123456</td></tr><tr><td>Processing Date</td><td>22-Jun-10 10:28</td></tr></table>
<br />";

			const string shipmentEmailBody = @"Reference Number : CCN123456<br />
<br />
</strong>A 'Status Update' message has been received from the CBSA for an Across/IID EDI Release.<br />";

			var requestMessage = GetRNSRequest("CCN 123456", RNSMessageTypes.Codes.StatusQuery);
			requestMessage.EM_LinkUniqueID = entryHeader.PK;
			requestMessage.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			requestMessage.EM_SystemCreateUser = userToNotify.GS_Code;
			var systemCreateTime = requestMessage.EM_SystemCreateTimeUtc;

			requestMessage = GetRNSRequest("CCN 123456", RNSMessageTypes.Codes.ArrivalCertification);
			requestMessage.EM_LinkUniqueID = shipment.PK;
			requestMessage.EM_LinkTable = JobShipmentSchema.Constants.TableName;
			requestMessage.EM_SystemCreateTimeUtc = systemCreateTime.AddMinutes(1);
			requestMessage.EM_SystemCreateUser = shipmentUserToNotify.GS_Code;
			entryHeader.Declaration.JE_DeclarationReference = "B00001001";
			shipment.JS_UniqueConsignRef = "S00001000";
			shipment.Messages.Load();
			Factory.Save();

			var ediMessage = GetEDIReleaseMessage(messageText);
			ediMessage.EM_SystemCreateTimeUtc = systemCreateTime.AddMinutes(2);
			var entrHeaderMessagesCount = entryHeader.Messages.Count;
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertEquals("Linked object", entryHeader.PK, ediMessage.EM_LinkedObject.PK);
			AssertEquals("Message added entry header messages", entrHeaderMessagesCount + 1, entryHeader.Messages.Count);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted, ediMessage.EM_MessageSubType);
			AssertContains("EM_MessageInterpretation", declarationEmailBody, ediMessage.EM_MessageInterpretation);
			AssertEmail("Accepted Across/IID EDI Release Response for CCN123456", declarationEmailBody, messageText.Replace("\r\n", ""), jobReferece: entryHeader.Declaration.JE_DeclarationReference);

			var messaging = RNSMessagingBOTest.GetRNSMessagingBO(shipment);
			AssertEquals("Message is attached to shipment", 3, messaging.Messages.Count);
			AssertContains("Shipment Release Status", EDIReleaseImportEntryStatusList.Descriptions.DeclarationAccepted, messaging.RN_ReleaseStatus);
			AssertEquals("Shipment Release Date", ZDateTime.Empty, messaging.RN_ReleaseDate);

			ediMessage = (EDIReleaseMessage)messaging.Messages[2];
			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to shipment", shipment.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", shipmentEmailBody, ediMessage.EM_MessageInterpretation);
			AssertEmail("Across/IID EDI Release Status Update Message for CCN123456", shipmentEmailBody, messageText.Replace("\r\n", ""), recipient: "ShipmentUserToNotify@blah.com", jobReferece: shipment.JS_UniqueConsignRef);

			foreach (var sentMessage in shipment.Messages.Find(x => x.EM_ReceiveTransmit == EDIInterchange.Direction.Transmit))
			{
				sentMessage.EM_ApplicationCode = "XXX";
			}
			ediMessage.Interchange.EI_InterchangeNum += 1;
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			Factory.Save();

			ediMessage = GetEDIReleaseMessage(messageText);
			ediMessage.EM_SystemCreateTimeUtc = systemCreateTime.AddMinutes(4);
			entrHeaderMessagesCount = entryHeader.Messages.Count;
			Env.OutgoingMailManager.EmailsCreated.RemoveAll(x => true);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Linked object", entryHeader.PK, ediMessage.EM_LinkedObject.PK);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertEquals("Message added entry header messages", entrHeaderMessagesCount + 1, entryHeader.Messages.Count);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted, ediMessage.EM_MessageSubType);
			AssertContains("EM_MessageInterpretation", declarationEmailBody, ediMessage.EM_MessageInterpretation);
			AssertEmail("Accepted Across/IID EDI Release Response for CCN123456", declarationEmailBody, messageText.Replace("\r\n", ""), jobReferece: entryHeader.Declaration.JE_DeclarationReference);

			messaging = RNSMessagingBOTest.GetRNSMessagingBO(shipment);
			AssertEquals("Message is attached to shipment", 4, messaging.Messages.Count);
			AssertContains("Shipment Release Status", EDIReleaseImportEntryStatusList.Descriptions.DeclarationAccepted, messaging.RN_ReleaseStatus);
			AssertEquals("Shipment Release Date", ZDateTime.Empty, messaging.RN_ReleaseDate);

			ediMessage = (EDIReleaseMessage)messaging.Messages[3];
			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to shipment", shipment.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", shipmentEmailBody, ediMessage.EM_MessageInterpretation);
			AssertEmail("Across/IID EDI Release Status Update Message for CCN123456", shipmentEmailBody, messageText.Replace("\r\n", ""), recipient: "", jobReferece: shipment.JS_UniqueConsignRef, expectNoRecipiants: true);
		}

		public void TestStatusUpdateWithMultipleAssociatedRequestsWithLinkedObjects()
		{
			const string messageText = @"UNH+257+CUSRES:D:96A:UN'BGM+:::257+B99999999+11'DTM+9:201006221028:203'GIS+9'RFF+XC:CCN777'UNT+6+257'";

			const string declarationEmailBody = @"Reference Number : CCN777<br />
<br />
</strong>A 'Status Update' message has been received from the CBSA for an Across/IID EDI Release.<br />
Please see message details below.<br />
<br />NOTE: The reference details in this received message do not match the job. Please review this job.
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Indicator</td><td>9 - Declaration Accepted, Awaiting arrival of Goods, PARS is set - OK to cross the border</td></tr><tr><td>Service Option</td><td>257 - Post-arrival EDI Release</td></tr><tr><td>Document Reference</td><td>B99999999</td></tr><tr><td>Cargo Control Number</td><td>CCN777</td></tr><tr><td>Processing Date</td><td>22-Jun-10 10:28</td></tr></table>
<br />";

			const string shipmentEmailBody = @"Reference Number : CCN777<br />
<br />
</strong>A 'Status Update' message has been received from the CBSA for an Across/IID EDI Release.<br />";

			var requestMessage = GetRNSRequest("CCN777", RNSMessageTypes.Codes.StatusQuery);
			requestMessage.EM_LinkUniqueID = entryHeader.PK;
			requestMessage.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			requestMessage.EM_SystemCreateUser = userToNotify.GS_Code;
			var systemCreateTime = requestMessage.EM_SystemCreateTimeUtc;

			requestMessage = GetRNSRequest("CCN777", RNSMessageTypes.Codes.ArrivalCertification);
			requestMessage.EM_LinkUniqueID = shipment.PK;
			requestMessage.EM_LinkTable = JobShipmentSchema.Constants.TableName;
			requestMessage.EM_SystemCreateTimeUtc = systemCreateTime.AddMinutes(1);
			requestMessage.EM_SystemCreateUser = shipmentUserToNotify.GS_Code;
			entryHeader.Declaration.JE_DeclarationReference = "B00001001";
			shipment.JS_UniqueConsignRef = "S00001000";
			Factory.Save();

			var ediMessage = GetEDIReleaseMessage(messageText);
			ediMessage.EM_SystemCreateTimeUtc = systemCreateTime.AddMinutes(2);
			var entrHeaderMessagesCount = entryHeader.Messages.Count;
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertEquals("Linked object", entryHeader.PK, ediMessage.EM_LinkedObject.PK);
			AssertEquals("Message added entry header messages", entrHeaderMessagesCount + 1, entryHeader.Messages.Count);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted, ediMessage.EM_MessageSubType);
			AssertContains("EM_MessageInterpretation", declarationEmailBody, ediMessage.EM_MessageInterpretation);
			AssertEmail("Across/IID EDI Release Status Update Message for CCN777", declarationEmailBody, messageText.Replace("\r\n", ""), jobReferece: entryHeader.Declaration.JE_DeclarationReference);

			var messaging = RNSMessagingBOTest.GetRNSMessagingBO(shipment);
			AssertEquals("Message is attached to shipment", 3, messaging.Messages.Count);
			AssertContains("Shipment Release Status", EDIReleaseImportEntryStatusList.Descriptions.DeclarationAccepted, messaging.RN_ReleaseStatus);
			AssertEquals("Shipment Release Date", ZDateTime.Empty, messaging.RN_ReleaseDate);

			ediMessage = (EDIReleaseMessage)messaging.Messages[2];
			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to shipment", shipment.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", shipmentEmailBody, ediMessage.EM_MessageInterpretation);
			AssertEmail("Across/IID EDI Release Status Update Message for CCN777", shipmentEmailBody, messageText.Replace("\r\n", ""), recipient: "ShipmentUserToNotify@blah.com", jobReferece: shipment.JS_UniqueConsignRef);
		}

		public void TestErrorRNSResponseWithAssociatedRequestWithoutLinkedObject()
		{
			var request = GetRNSRequest("CCN1234560", RNSMessageTypes.Codes.ArrivalCertification, "9463TKWB1438742C");
			request.EM_SystemCreateUser = userToNotify.GS_Code;
			Factory.Save();

			string messageText = ZString.Format(@"UNH+1+CUSRES:D:96A:UN'
BGM+:::489+9463TKWB1438742C+11'
DTM+9:201101130748:203'
GIS+2'
ERP+2:{0}'
ERC+09'
RFF+XC:CCN1234560'
UNT+8+1'", request.EM_MessageNum);

			const string expectedBody = @"</strong>An ERROR response message has been received from the CBSA for a Warehouse Arrival Certification Message.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />

<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Code</th><th>Error Text</th></tr></thead><tr><td>09</td><td>Work Location Code (Destination): ARRIVAL OFFICE DOES NOT MATCH RELEASE</td></tr></table>";

			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.MessageContentRejected, ediMessage.EM_MessageSubType);
			AssertNull("No linked object", ediMessage.EM_LinkedObject);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);

			//TODO: Enhance the message processor to send notifications to the user who sends RNS request from Release Notifications module and remove the last parameter from the following assertion
			AssertEmail("Error Warehouse Arrival Certification Message Response for 9463TKWB1438742C", expectedBody, messageText.Replace("\r\n", ""), string.Empty);
		}

		public void TestErrorRNSResponseWithEManifestErrorCode()
		{
			var request = GetRNSRequest("CCN1234561", RNSMessageTypes.Codes.ArrivalCertification, "9463TKWB1438742C");
			request.EM_SystemCreateUser = userToNotify.GS_Code;
			Factory.Save();

			string messageText = ZString.Format(@"UNH+1+CUSRES:D:96A:UN'
BGM+:::489+9463TKWB1438742C+11'
DTM+9:201101130748:203'
GIS+2'
ERP+2:{0}'
ERC+34'
RFF+XC:CCN1234561'
UNT+8+1'", request.EM_MessageNum);

			const string expectedBody = @"</strong>An ERROR response message has been received from the CBSA for a Warehouse Arrival Certification Message.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />

<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Code</th><th>Error Text</th></tr></thead><tr><td>34</td><td>Client Supplied Request ID: Request already in arrived status</td></tr></table>";

			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.MessageContentRejected, ediMessage.EM_MessageSubType);
			AssertNull("No linked object", ediMessage.EM_LinkedObject);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEmail("Error Warehouse Arrival Certification Message Response for 9463TKWB1438742C", expectedBody, messageText.Replace("\r\n", ""), string.Empty);
		}

		public void TestErrorRNSResponseWithAssociatedMultipleRequestsFromOneLinkedObject()
		{
			entryHeader.Messages.RemoveAndDeleteAllFromTest();
			var requestQRY = GetRNSRequest("CCN123456", RNSMessageTypes.Codes.StatusQuery);
			requestQRY.EM_LinkUniqueID = entryHeader.PK;
			requestQRY.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			requestQRY.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			requestQRY.EM_SystemCreateUser = userToNotify.GS_Code;

			var requestACR = GetRNSRequest("CCN123456", RNSMessageTypes.Codes.ArrivalCertification);
			requestACR.EM_LinkUniqueID = entryHeader.PK;
			requestACR.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			requestACR.EM_SystemCreateTimeUtc = ZDateTime.Now;
			requestACR.EM_SystemCreateUser = userToNotify.GS_Code;

			Factory.Save();
			entryHeader.Messages.Load();

			string messageText = ZString.Format("UNH+257+CUSRES:D:96A:UN'BGM+:::489+B99999998+11'DTM+9:201006221028:203'GIS+2'ERP+2:{0}'ERC+09'RFF+XC:CCN123456'UNT+8+1'", requestACR.EM_MessageNum);
			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			var expectedBody = @"An ERROR response message has been received from the CBSA for a Warehouse Arrival Certification Message.";

			AssertEquals("One received message attached", 3, entryHeader.Messages.Count);
			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.MessageContentRejected, ediMessage.EM_MessageSubType);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEmail("Error Warehouse Arrival Certification Message Response for CCN123456", expectedBody, messageText.Replace("\r\n", ""));
			AssertEquals("Sent message status is REJ", EDIMessage.Status.Rejected, requestACR.EM_Status);

			messageText = ZString.Format("UNH+257+CUSRES:D:96A:UN'BGM+:::489+B99999998+11'DTM+9:201006221028:203'GIS+2'ERP+2:{0}'ERC+09'RFF+XC:CCN123456'UNT+8+1'", requestQRY.EM_MessageNum);
			ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			expectedBody = @"An ERROR response message has been received from the CBSA for a RNS Status Query";

			AssertEquals("One received message attached", 4, entryHeader.Messages.Count);
			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.MessageContentRejected, ediMessage.EM_MessageSubType);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEmail("Error RNS Status Query Response for CCN123456", expectedBody, messageText.Replace("\r\n", ""));
			AssertEquals("Sent message status is REJ", EDIMessage.Status.Rejected, requestQRY.EM_Status);
		}

		public void TestErrorRNSResponseWithAssociatedMultipleRequestsWithoutLinkedObjects()
		{
			var request = GetRNSRequest("", RNSMessageTypes.Codes.ArrivalCertification, "B99999999");
			request.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			request.EM_SystemCreateUser = userToNotify.GS_Code;

			var request2 = GetRNSRequest("", RNSMessageTypes.Codes.StatusQuery, "B99999999");
			request2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			request2.EM_SystemCreateUser = userToNotify.GS_Code;
			Factory.Save();

			string messageText = ZString.Format("UNH+257+CUSRES:D:96A:UN'BGM+:::489+B99999999+11'DTM+9:201006221028:203'GIS+2'ERP+2:{0}'ERC+09'RFF+XC:CCN780'UNT+8+1'", request2.EM_MessageNum);
			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			var expectedBody = @"An ERROR response message has been received from the CBSA for a RNS Status Query";

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertNull("No linked object", ediMessage.EM_LinkedObject);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.MessageContentRejected, ediMessage.EM_MessageSubType);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);

			//TODO: Enhance the message processor to send notifications to the user who sends RNS request from Release Notifications module and remove the last parameter from the following assertion
			AssertEmail("Error RNS Status Query Response for B99999999", expectedBody, messageText.Replace("\r\n", ""), string.Empty);

			messageText = ZString.Format("UNH+257+CUSRES:D:96A:UN'BGM+:::489+B99999999+11'DTM+9:201006221028:203'GIS+2'ERP+2:{0}'ERC+09'RFF+XC:CCN780'UNT+8+1'", request.EM_MessageNum);
			ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			expectedBody = @"An ERROR response message has been received from the CBSA for a Warehouse Arrival Certification Message.";

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertNull("No linked object", ediMessage.EM_LinkedObject);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.MessageContentRejected, ediMessage.EM_MessageSubType);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);

			//TODO: Enhance the message processor to send notifications to the user who sends RNS request from Release Notifications module and remove the last parameter from the following assertion
			AssertEmail("Error Warehouse Arrival Certification Message Response for B99999999", expectedBody, messageText.Replace("\r\n", ""), string.Empty);
		}

		public void TestErrorRNSResponseWithAssociatedMultipleRequestsWithLinkedObjects()
		{
			shipment.Messages.RemoveAndDeleteAllFromTest();
			consol.Messages.RemoveAndDeleteAllFromTest();
			shipmentCCN.CE_EntryNum = "CCN223456";
			string messageFormat = "UNH+257+CUSRES:D:96A:UN'BGM+:::489+B99999999+11'DTM+9:201006221028:203'GIS+2'ERP+2:{0}'ERC+09'RFF+XC:CCN223456'UNT+8+1'";
			const string consolEmailBody = @"Reference Number : CCN223456<br />
<br />
</strong>An ERROR response message has been received from the CBSA for a Warehouse Arrival Certification Message.<br />
";
			const string shipmentEmailBody = @"Reference Number : CCN223456<br />
<br />
</strong>An ERROR response message has been received from the CBSA for a RNS Status Query.<br />";

			var request = GetRNSRequest("CCN223456", RNSMessageTypes.Codes.StatusQuery);
			request.EM_LinkUniqueID = shipment.PK;
			request.EM_LinkTable = JobShipmentSchema.Constants.TableName;
			request.EM_SystemCreateUser = userToNotify.GS_Code;

			consolCCN.CE_EntryNum = "CCN223456";
			var request2 = GetRNSRequest("CCN223456", RNSMessageTypes.Codes.ArrivalCertification);
			request2.EM_LinkUniqueID = consol.PK;
			request2.EM_LinkTable = JobConsolSchema.Constants.TableName;
			request2.EM_SystemCreateTimeUtc = request.EM_SystemCreateTimeUtc.AddMinutes(1);
			request2.EM_SystemCreateUser = userToNotify.GS_Code;
			Factory.Save();
			shipment.Messages.Load();
			consol.Messages.Load();

			AssertEquals("Message on shipment", 1, shipment.Messages.Count);
			AssertEquals("Message on consol", 1, consol.Messages.Count);

			var messageText = ZString.Format(messageFormat, request.EM_MessageNum);
			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			var messaging = RNSMessagingBOTest.GetRNSMessagingBO(shipment);
			AssertEquals("Message is attached to shipment", 2, messaging.Messages.Count);
			AssertContains("Shipment Release Status", EDIReleaseImportEntryStatusList.Descriptions.MessageContentRejected, messaging.RN_ReleaseStatus);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.MessageContentRejected, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", shipment.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation contains link to shipment", shipment.JS_UniqueConsignRef, ediMessage.EM_MessageInterpretation);
			AssertContains("EM_MessageInterpretation", shipmentEmailBody, ediMessage.EM_MessageInterpretation);
			AssertEmail("Error RNS Status Query Response for CCN223456", shipmentEmailBody, messageText.Replace("\r\n", ""));

			messaging = RNSMessagingBOTest.GetRNSMessagingBO(consol);
			AssertEquals("Message is NOT attached to consol", 1, messaging.Messages.Count);

			messageText = ZString.Format(messageFormat, request2.EM_MessageNum);
			ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			messaging = RNSMessagingBOTest.GetRNSMessagingBO(shipment);
			AssertEquals("Message is NOT attached to shipment", 2, messaging.Messages.Count);

			messaging = RNSMessagingBOTest.GetRNSMessagingBO(consol);
			AssertEquals("Message is attached to consol", 2, messaging.Messages.Count);
			AssertContains("Shipment Release Status", EDIReleaseImportEntryStatusList.Descriptions.MessageContentRejected, messaging.RN_ReleaseStatus);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.MessageContentRejected, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", consol.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation contains link to consol", consol.JK_UniqueConsignRef, ediMessage.EM_MessageInterpretation);
			AssertContains("EM_MessageInterpretation", consolEmailBody, ediMessage.EM_MessageInterpretation);
			AssertEmail("Error Warehouse Arrival Certification Message Response for CCN223456", consolEmailBody, messageText.Replace("\r\n", ""));
		}

		public void TestMultipleErrorRNSResponsesForMultipleRequests()
		{
			const string moduleMessageText = @"UNH+257+CUSRES:D:96A:UN'BGM+:::257+BXXXXXXXXX+11'DTM+9:201006221028:203'GIS+2'RFF+XC:CCNXXXXXX'UNT+6+257'";
			string messageTextFormat = "UNH+257+CUSRES:D:96A:UN'BGM+:::489+B99999998+11'DTM+9:201006221028:203'GIS+2'ERP+2:{0}'ERC+09'RFF+XC:CCN123456'UNT+8+1'";

			var request1 = GetRNSRequest("CCN123456", RNSMessageTypes.Codes.StatusQuery);
			request1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-5);

			var request2 = GetRNSRequest("CCN123456", RNSMessageTypes.Codes.StatusQuery);
			request2.EM_LinkUniqueID = shipment.PK;
			request2.EM_LinkTable = JobShipmentSchema.Constants.TableName;
			request2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-4);

			var request3 = GetRNSRequest("CCN123456", RNSMessageTypes.Codes.ArrivalCertification);
			request3.EM_LinkUniqueID = consol.PK;
			request3.EM_LinkTable = JobConsolSchema.Constants.TableName;
			request3.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-3);

			var request4 = GetRNSRequest("CCN123456", RNSMessageTypes.Codes.ArrivalCertification);
			request4.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);

			var request5 = GetRNSRequest("CCN123456", RNSMessageTypes.Codes.ArrivalCertification);
			request5.EM_LinkUniqueID = entryHeader.PK;
			request5.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			request5.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			Factory.Save();

			var ediMessage = GetEDIReleaseMessage(moduleMessageText);
			impMessageProcessor.ProcessMessage(ediMessage);
			AssertNull("Linked to Release Notifications module", ediMessage.EM_LinkedObject);

			var messageText = ZString.Format(messageTextFormat, request2.EM_MessageNum);
			ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);
			AssertEquals("Linked to shipment", shipment.PK, ediMessage.EM_LinkUniqueID);

			messageText = ZString.Format(messageTextFormat, request3.EM_MessageNum);
			ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);
			AssertEquals("Linked to consol", consol.PK, ediMessage.EM_LinkUniqueID);

			messageText = ZString.Format(messageTextFormat, request4.EM_MessageNum);
			ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);
			AssertNull("Linked to Release Notifications module", ediMessage.EM_LinkedObject);

			messageText = ZString.Format(messageTextFormat, request5.EM_MessageNum);
			ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);

			var releaseNotifications = new CAReleaseNotificationsCollection(Factory);
			releaseNotifications.Load();
			entryHeader.Messages.Load();
			AssertEquals("Messages attached to Release Notifications module", 4, releaseNotifications.Count);
			AssertEquals("Messages attached to shipment", 3, RNSMessagingBOTest.GetRNSMessagingBO(shipment).Messages.Count);
			AssertEquals("Messages attached to consol", 3, RNSMessagingBOTest.GetRNSMessagingBO(consol).Messages.Count);
			AssertEquals("Messages attached to entry", 3, entryHeader.Messages.Count);
		}

		public void TestAcceptedMessageWhereLinkedObjectsGottenByDirectReference()
		{
			#region Message Text

			const string messageText =
				@"UNH+1+CUSRES:D:96A:UN'
BGM+:::125+12345000067897+11'
LOC+22+0351:129::3021'
DTM+9:200912221030:203'
GIS+9'
FTX+AAG+++DELIVERY INSTRUCTIONS LINE 11111111111111111111111111111111:LINE 22222222222222'
FTX+AAG+++OTHER DELIVERY INSTRUCTIONS LINE 11111111111111111111111111:OTHER DELIVERY INSTRUCTIONS LINE 22222222222222'
EQD+CN+CONTAINER 1'
EQD+CN+CONTAINER 2'
EQD+CN+CONTAINER 3'
EQD+CN+CONTAINER 4'
EQD+CN+CONTAINER 5'
EQD+CN+CONTAINER 6'
EQD+CN+CONTAINER 7'
EQD+CN+CONTAINER 8'
EQD+CN+CONTAINER 9'
EQD+CN+CONTAINER10'
EQD+CN+CONTAINER11'
EQD+CN+CONTAINER12'
EQD+CN+CONTAINER13'
EQD+CN+CONTAINER14'
ERP+2::20'
RFF+CN:CCN123456'
UNT+26+257'";

			#endregion

			#region Expected Email Body

			//A 'Message Accepted' response has been received from the CBSA for an Across/IID EDI Release.
			//Please see message details below.

			//Field					Value
			//Processing Indicator	9 - Declaration Accepted, Awaiting arrival of Goods, PARS is set - OK to cross the border
			//Service Option		125 - Pre-arrival EDI Release
			//Document Reference	12345000067897
			//Release Office Code	0351
			//Warehouse Code		3021
			//Cargo Control Number	2ITN12345678987654321
			//Processing Date		22-Dec-09 10:30
			//Container Numbers		CONTAINER 1, CONTAINER 2, CONTAINER 3, CONTAINER 4, CONTAINER 5, CONTAINER 6,
			//						CONTAINER 7, CONTAINER 8, CONTAINER 9, CONTAINER10, CONTAINER11, CONTAINER12,
			//						CONTAINER13, CONTAINER14
			//Delivery Instructions	DELIVERY INSTRUCTIONS LINE 11111111111111111111111111111111 LINE 22222222222222
			//						OTHER DELIVERY INSTRUCTIONS LINE 11111111111111111111111111 OTHER DELIVERY INSTRUCTIONS LINE 22222222222222

			const string declarationEmailBody = @"Reference Number : 12345000067897<br />
<br />
</strong>A 'Message Accepted' response has been received from the CBSA for an Across/IID EDI Release.<br />
Please see message details below.<br />
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Indicator</td><td>9 - Declaration Accepted, Awaiting arrival of Goods, PARS is set - OK to cross the border</td></tr><tr><td>Service Option</td><td>125 - Pre-arrival EDI Release</td></tr><tr><td>Document Reference</td><td>12345000067897</td></tr><tr><td>Release Office Code</td><td>0351</td></tr><tr><td>Warehouse Code</td><td>3021</td></tr><tr><td>Cargo Control Number</td><td>CCN123456</td></tr><tr><td>Processing Date</td><td>22-Dec-09 10:30</td></tr><tr><td>Container Numbers</td><td>CONTAINER 1, CONTAINER 2, CONTAINER 3, CONTAINER 4, CONTAINER 5, CONTAINER 6,<br>CONTAINER 7, CONTAINER 8, CONTAINER 9, CONTAINER10, CONTAINER11, CONTAINER12,<br>CONTAINER13, CONTAINER14</td></tr><tr><td>Delivery Instructions</td><td>DELIVERY INSTRUCTIONS LINE 11111111111111111111111111111111 LINE 22222222222222<br>OTHER DELIVERY INSTRUCTIONS LINE 11111111111111111111111111 OTHER DELIVERY INSTRUCTIONS LINE 22222222222222</td></tr></table>
<br />
<hr />";

			const string shipmentAndConsolEmailBody = @"Reference Number : CCN123456<br />
<br />
</strong>A 'Status Update' message has been received from the CBSA for an Across/IID EDI Release.<br />";

			#endregion

			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation contains link to declaration", declaration.JE_DeclarationReference, ediMessage.EM_MessageInterpretation);
			AssertContains("EM_MessageInterpretation", declarationEmailBody, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status", EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ClearOriginal, entryHeader.CH_Status);
			AssertEmail("Accepted Across/IID EDI Release Response for 12345000067897", declarationEmailBody, messageText.Replace("\r\n", ""), ccRecipient: "blah@blah.com;staff1@wisetechglobal.com");
			AssertEquals("Release date not set", ZDateTime.Empty, declaration.JE_EntryAuthorisationDate);

			var messaging = RNSMessagingBOTest.GetRNSMessagingBO(shipment);
			AssertEquals("Message is attached to shipment", 2, messaging.Messages.Count);
			AssertContains("Shipment Release Status", EDIReleaseImportEntryStatusList.Descriptions.DeclarationAccepted, messaging.RN_ReleaseStatus);
			AssertEquals("Shipment Release Date", ZDateTime.Empty, messaging.RN_ReleaseDate);

			ediMessage = (EDIReleaseMessage)messaging.Messages[1];
			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to shipment", shipment.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation contains link to shipment", shipment.JS_UniqueConsignRef, ediMessage.EM_MessageInterpretation);
			AssertContains("EM_MessageInterpretation", shipmentAndConsolEmailBody, ediMessage.EM_MessageInterpretation);
			AssertEmail("Across/IID EDI Release Status Update Message for CCN123456", shipmentAndConsolEmailBody, messageText.Replace("\r\n", ""), ccRecipient: "blah@blah.com;staff1@wisetechglobal.com");

			messaging = RNSMessagingBOTest.GetRNSMessagingBO(consol);
			AssertEquals("Message is attached to consol", 2, messaging.Messages.Count);
			AssertContains("Shipment Release Status", EDIReleaseImportEntryStatusList.Descriptions.DeclarationAccepted, messaging.RN_ReleaseStatus);
			AssertEquals("Shipment Release Date", ZDateTime.Empty, messaging.RN_ReleaseDate);

			ediMessage = (EDIReleaseMessage)messaging.Messages[1];
			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to consol", consol.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation contains link to consol", consol.JK_UniqueConsignRef, ediMessage.EM_MessageInterpretation);
			AssertContains("EM_MessageInterpretation", shipmentAndConsolEmailBody, ediMessage.EM_MessageInterpretation);
			AssertEmail("Across/IID EDI Release Status Update Message for CCN123456", shipmentAndConsolEmailBody, messageText.Replace("\r\n", ""), ccRecipient: "blah@blah.com;staff1@wisetechglobal.com");
		}

		public void TestAcceptedMessageForCancellationWhereLinkedObjectsGottenByDirectReference()
		{
			#region Message Text

			const string acceptedMessageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::125+12345000067897+11'
DTM+9:201006221028:203'
GIS+4'
UNT+5+1'";

			const string cancellationMessageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::125+12345000067897+11'
DTM+9:201006221028:203'
GIS+9'
UNT+5+1'";

			#endregion

			#region Expected Email Body

			const string declarationAcceptedEmailBody = @"Reference Number : 12345000067897<br />
<br />
</strong>A 'Status Update' message has been received from the CBSA for an Across/IID EDI Release.<br />
Please see message details below.<br />
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Indicator</td><td>4 - Goods Released</td></tr><tr><td>Service Option</td><td>125 - Pre-arrival EDI Release</td></tr><tr><td>Document Reference</td><td>12345000067897</td></tr><tr><td>Processing Date</td><td>22-Jun-10 10:28</td></tr></table>
<br />
<hr />";

			//A 'Message Accepted' response has been received from the CBSA for an Across/IID EDI Release.
			//Please see message details below.

			//Field					Value
			//Processing Indicator	9 - Release Cancelled
			//Service Option		125 - Pre-arrival EDI Release
			//Document Reference	12345000067897
			//Processing Date		22-Jun-10 10:28

			const string declarationCancelledEmailBody = @"Reference Number : 12345000067897<br />
<br />
</strong>A 'Message Accepted' response has been received from the CBSA for an Across/IID EDI Release.<br />
Please see message details below.<br />
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Indicator</td><td>9 - Release Canceled</td></tr><tr><td>Service Option</td><td>125 - Pre-arrival EDI Release</td></tr><tr><td>Document Reference</td><td>12345000067897</td></tr><tr><td>Processing Date</td><td>22-Jun-10 10:28</td></tr></table>
<br />
<hr />";

			const string shipmentAndConsolEmailBody = @"Reference Number : CCN123456<br />
<br />
</strong>A 'Status Update' message has been received from the CBSA for an Across/IID EDI Release.<br />";

			const string shipmentAndConsolEmailBody2 = @"Reference Number : CCN2<br />
<br />
</strong>A 'Status Update' message has been received from the CBSA for an Across/IID EDI Release.<br />";

			#endregion

			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN2";
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingDelete;

			var consol2 = Factory.New<ForwardingConsol>();
			var entryNumber = consol2.Numbers.AddNew();
			entryNumber.CE_EntryNum = "CCN2";
			entryNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			var sentMessage = helper.GetEDIReleaseMessage(EDIMessage.MessageNumberPlaceHolder, ZDateTime.UtcNow.AddDays(-2), EDIMessage.Direction.Transmit, "4");
			sentMessage.EM_SystemCreateUser = userToNotify.GS_Code;
			consol2.Messages.Add(sentMessage);

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_IsCancelled = false;
			entryNumber = shipment2.Numbers.AddNew();
			entryNumber.CE_EntryNum = "CCN2";
			entryNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			sentMessage = helper.GetEDIReleaseMessage(EDIMessage.MessageNumberPlaceHolder, ZDateTime.UtcNow.AddDays(-2), EDIMessage.Direction.Transmit, "5");
			sentMessage.EM_SystemCreateUser = userToNotify.GS_Code;
			shipment2.Messages.Add(sentMessage);

			Factory.Save();

			var ediMessage = GetEDIReleaseMessage(acceptedMessageText);
			ediMessage.EM_SystemCreateUser = userToNotify.GS_Code;
			impMessageProcessor.ProcessMessage(ediMessage);
			Factory.Save();

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.GoodsReleased, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation contains link to declaration", declaration.JE_DeclarationReference, ediMessage.EM_MessageInterpretation);
			AssertContains("EM_MessageInterpretation", declarationAcceptedEmailBody, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status", EDIReleaseImportEntryStatusList.Codes.GoodsReleased, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.AwaitingDelete, entryHeader.CH_Status);
			AssertEmail("Across/IID EDI Release Status Update Message for 12345000067897", declarationAcceptedEmailBody, acceptedMessageText.Replace("\r\n", ""));

			var clearedEvent = shipment.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
			AssertNotNull("Customs Cleared event should exist on shipment", clearedEvent);

			clearedEvent = shipment2.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
			AssertNotNull("Customs Cleared event should exist on shipment 2", clearedEvent);

			clearedEvent = consol.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
			AssertNotNull("Customs Cleared event should exist on consol", clearedEvent);

			clearedEvent = consol2.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
			AssertNotNull("Customs Cleared event should exist on consol 2", clearedEvent);

			ediMessage = GetEDIReleaseMessage(cancellationMessageText);
			ediMessage.EM_SystemCreateUser = userToNotify.GS_Code;
			impMessageProcessor.ProcessMessage(ediMessage);
			Factory.Save();

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.Cancelled, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation contains link to declaration", declaration.JE_DeclarationReference, ediMessage.EM_MessageInterpretation);
			AssertContains("EM_MessageInterpretation", declarationCancelledEmailBody, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status", EDIReleaseImportEntryStatusList.Codes.Cancelled, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ClearDelete, entryHeader.CH_Status);
			AssertEmail("Cancellation accepted Across/IID EDI Release Response for 12345000067897", declarationCancelledEmailBody, cancellationMessageText.Replace("\r\n", ""));
			Assert("Release date not set", !declaration.JE_EntryAuthorisationDate.IsValid);

			//Related shipment by CCN 1
			var messaging = RNSMessagingBOTest.GetRNSMessagingBO(shipment);
			AssertEquals("Message is attached to shipment", 3, messaging.Messages.Count);
			AssertContains("Shipment Release Status", EDIReleaseImportEntryStatusList.Descriptions.Cancelled, messaging.RN_ReleaseStatus);
			AssertEquals("Shipment Release Date", ZDateTime.Empty, messaging.RN_ReleaseDate);

			ediMessage = (EDIReleaseMessage)messaging.Messages[2];
			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.Cancelled, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to shipment", shipment.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation contains link to shipment", shipment.JS_UniqueConsignRef, ediMessage.EM_MessageInterpretation);
			AssertContains("EM_MessageInterpretation", shipmentAndConsolEmailBody, ediMessage.EM_MessageInterpretation);
			AssertEmail("Cancellation accepted Across/IID EDI Release Response for CCN123456", shipmentAndConsolEmailBody, cancellationMessageText.Replace("\r\n", ""));

			clearedEvent = shipment.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
			AssertNull("Customs Cleared event should be cancelled on shipment", clearedEvent);
			var cancelledEvent = shipment.Logs.MostRecentLogByEventTime(Events.Cancelled);
			AssertNotNull("Customs Cleared event cancelled should exist on shipment", cancelledEvent);

			//Related shipment by CCN 2
			messaging = RNSMessagingBOTest.GetRNSMessagingBO(shipment2);
			AssertEquals("Message is attached to shipment 2", 3, messaging.Messages.Count);
			AssertContains("Shipment 2 Release Status", EDIReleaseImportEntryStatusList.Descriptions.Cancelled, messaging.RN_ReleaseStatus);
			AssertEquals("Shipment 2 Release Date", ZDateTime.Empty, messaging.RN_ReleaseDate);

			ediMessage = (EDIReleaseMessage)messaging.Messages[2];
			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.Cancelled, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to shipment 2", shipment2.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation contains link to shipment 2", shipment2.JS_UniqueConsignRef, ediMessage.EM_MessageInterpretation);
			AssertContains("EM_MessageInterpretation", shipmentAndConsolEmailBody2, ediMessage.EM_MessageInterpretation);
			AssertEmail("Cancellation accepted Across/IID EDI Release Response for CCN2", shipmentAndConsolEmailBody2, cancellationMessageText.Replace("\r\n", ""));

			clearedEvent = shipment2.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
			AssertNull("Customs Cleared event should be cancelled on shipment", clearedEvent);
			cancelledEvent = shipment2.Logs.MostRecentLogByEventTime(Events.Cancelled);
			AssertNotNull("Customs Cleared event cancelled should exist on shipment", cancelledEvent);

			//Related consol by CCN 1
			messaging = RNSMessagingBOTest.GetRNSMessagingBO(consol);
			AssertEquals("Message is attached to consol", 3, messaging.Messages.Count);
			AssertContains("Consol Release Status", EDIReleaseImportEntryStatusList.Descriptions.Cancelled, messaging.RN_ReleaseStatus);
			AssertEquals("Consol Release Date", ZDateTime.Empty, messaging.RN_ReleaseDate);

			ediMessage = (EDIReleaseMessage)messaging.Messages[2];
			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.Cancelled, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to consol", consol.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation contains link to consol", consol.JK_UniqueConsignRef, ediMessage.EM_MessageInterpretation);
			AssertContains("EM_MessageInterpretation", shipmentAndConsolEmailBody, ediMessage.EM_MessageInterpretation);
			AssertEmail("Cancellation accepted Across/IID EDI Release Response for CCN123456", shipmentAndConsolEmailBody, cancellationMessageText.Replace("\r\n", ""));

			clearedEvent = consol.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
			AssertNull("Customs Cleared event should be cancelled on consol", clearedEvent);
			cancelledEvent = consol.Logs.MostRecentLogByEventTime(Events.Cancelled);
			AssertNotNull("Customs Cleared event cancelled should exist on consol", cancelledEvent);

			//Related consol by CCN 2
			messaging = RNSMessagingBOTest.GetRNSMessagingBO(consol2);
			AssertEquals("Message is attached to consol 2", 3, messaging.Messages.Count);
			AssertContains("Consol 2 Release Status", EDIReleaseImportEntryStatusList.Descriptions.Cancelled, messaging.RN_ReleaseStatus);
			AssertEquals("Consol 2 Release Date", ZDateTime.Empty, messaging.RN_ReleaseDate);

			ediMessage = (EDIReleaseMessage)messaging.Messages[2];
			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.Cancelled, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to consol 2", consol2.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation contains link to consol", consol2.JK_UniqueConsignRef, ediMessage.EM_MessageInterpretation);
			AssertContains("EM_MessageInterpretation", shipmentAndConsolEmailBody2, ediMessage.EM_MessageInterpretation);
			AssertEmail("Cancellation accepted Across/IID EDI Release Response for CCN2", shipmentAndConsolEmailBody2, cancellationMessageText.Replace("\r\n", ""));

			clearedEvent = consol2.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
			AssertNull("Customs Cleared event should be cancelled on consol", clearedEvent);
			cancelledEvent = consol2.Logs.MostRecentLogByEventTime(Events.Cancelled);
			AssertNotNull("Customs Cleared event cancelled should exist on consol", cancelledEvent);
		}

		public void TestAcceptedReleasedMessageUpdatesReleasedDateAndOffice()
		{
			#region Message Text

			const string messageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::125+12345000067897+11'
LOC+22+0495'
DTM+58:201006221028:203'
GIS+4'
RFF+CN:CCN123456'
UNT+7+1'";

			#endregion

			#region Expected Email Body

			const string expectedBody = @"</strong>A 'Status Update' message has been received from the CBSA for an Across/IID EDI Release.<br />
Please see message details below.<br />
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Indicator</td><td>4 - Goods Released</td></tr><tr><td>Service Option</td><td>125 - Pre-arrival EDI Release</td></tr><tr><td>Document Reference</td><td>12345000067897</td></tr><tr><td>Release Office Code</td><td>0495</td></tr><tr><td>Warehouse Code</td><td>&nbsp;</td></tr><tr><td>Cargo Control Number</td><td>CCN123456</td></tr><tr><td>Clearance Date</td><td>22-Jun-10 10:28</td></tr></table>
<br />
<hr />";

			#endregion

			var releaseDate = new ZDateTime(2010, 06, 22, 10, 28, 00);
			entryHeader.CH_Status = MessageStatusList.Codes.ClearOriginal;
			Factory.Save();
			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);
			Factory.Save();

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.GoodsReleased, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status", EDIReleaseImportEntryStatusList.Codes.GoodsReleased, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ClearOriginal, entryHeader.CH_Status);
			AssertEmail("Across/IID EDI Release Status Update Message for 12345000067897", expectedBody, messageText.Replace("\r\n", ""));
			AssertEquals("Release date set", releaseDate, declaration.JE_EntryAuthorisationDate);
			AssertEquals("Release office set", "0495", declaration.CA_ReleaseOffice);
			var clearedEvent = declaration.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
			AssertNotNull("Customs Cleared event should exist on declaration", clearedEvent);
			clearedEvent = entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
			AssertNotNull("Customs Cleared event should exist on entry header", clearedEvent);
			AssertEquals("Cleared date", declaration.JE_EntryAuthorisationDate, clearedEvent.SL_EventTime);
			AssertEquals("Cleared status code", EDIReleaseImportEntryStatusList.Codes.GoodsReleased, clearedEvent.SL_Reference);
			AssertEquals("RNSProcessingDate", releaseDate, ediMessage.RNSProcessingDate);
			AssertEquals("RNSReleaseDate", releaseDate, ediMessage.RNSReleaseDate);

			var messaging = RNSMessagingBOTest.GetRNSMessagingBO(shipment);
			AssertEquals("Message is attached to shipment", 2, messaging.Messages.Count);
			AssertContains("Shipment Release Status", EDIReleaseImportEntryStatusList.Descriptions.GoodsReleased, messaging.RN_ReleaseStatus);
			AssertEquals("Shipment Release Date", releaseDate, messaging.RN_ReleaseDate);
			clearedEvent = shipment.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
			AssertNotNull("Customs Cleared event should exist on shipment", clearedEvent);
			AssertEquals("Cleared date", messaging.RN_ReleaseDate, clearedEvent.SL_EventTime);
			AssertEquals("Cleared status", messaging.RN_ReleaseStatus, clearedEvent.SL_Reference);

			messaging = RNSMessagingBOTest.GetRNSMessagingBO(consol);
			AssertEquals("Message is attached to consol", 2, messaging.Messages.Count);
			AssertContains("Shipment Release Status", EDIReleaseImportEntryStatusList.Descriptions.GoodsReleased, messaging.RN_ReleaseStatus);
			AssertEquals("Shipment Release Date", releaseDate, messaging.RN_ReleaseDate);
			clearedEvent = consol.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
			AssertNotNull("Customs Cleared event should exist on consol", clearedEvent);
			AssertEquals("Cleared date", messaging.RN_ReleaseDate, clearedEvent.SL_EventTime);
			AssertEquals("Cleared status", messaging.RN_ReleaseStatus, clearedEvent.SL_Reference);
		}

		public void TestAcceptedReleasedMessageRetainsEarliestReleasedDateAndOffice()
		{
			#region Message Text

			const string messageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::125+12345000067897+11'
LOC+22+0495'
DTM+58:201006221028:203'
GIS+4'
RFF+CN:CCN123456'
UNT+7+1'";

			#endregion

			var ccn2 = declaration.CargoControlNumbers.AddNew();
			ccn2.CY_CargoControlNumber = "CCN2";
			entryHeader.Messages.Add(helper.GetEDIReleaseResponseMessage("", "CCN2", "4", false));
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2010, 6, 20, 1, 2, 3);
			declaration.CA_ReleaseOffice = "1000";

			entryHeader.CH_Status = MessageStatusList.Codes.ClearOriginal;
			Factory.Save();
			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);
			Factory.Save();

			CombineAssertions("Second Release with Later Date (i.e. when updating ETA)", () =>
			{
				AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
				AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.GoodsReleased, ediMessage.EM_MessageSubType);
				AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
				AssertEquals("Entry Status", EDIReleaseImportEntryStatusList.Codes.GoodsReleased, entryHeader.CH_EntryStatus);
				AssertEquals("Message Status", MessageStatusList.Codes.ClearOriginal, entryHeader.CH_Status);
				AssertEquals("Release date set", new ZDateTime(2010, 6, 22, 10, 28, 0), declaration.JE_EntryAuthorisationDate);
				AssertEquals("Release office set", "0495", declaration.CA_ReleaseOffice);
			});

			ediMessage = GetEDIReleaseMessage(messageText.Replace("201006221028", "201006181028"));
			impMessageProcessor.ProcessMessage(ediMessage);
			Factory.Save();

			CombineAssertions("Second Release with Customs Back Dating (i.e. when transferred from another broker)", () =>
			{
				AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
				AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.GoodsReleased, ediMessage.EM_MessageSubType);
				AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
				AssertEquals("Entry Status", EDIReleaseImportEntryStatusList.Codes.GoodsReleased, entryHeader.CH_EntryStatus);
				AssertEquals("Message Status", MessageStatusList.Codes.ClearOriginal, entryHeader.CH_Status);
				AssertEquals("Release date set", new ZDateTime(2010, 6, 18, 10, 28, 00), declaration.JE_EntryAuthorisationDate);
				AssertEquals("Release office set", "0495", declaration.CA_ReleaseOffice);
			});
		}

		public void TestErrorMessageWhereMessageStatusIsNotAwaitingReply()
		{
			#region Message Text

			const string messageText = @"UNH+257+CUSRES:D:96A:UN'
BGM+:::257+12345000067897+11'
LOC+22+0351:129::3021'
DTM+9:200912221030:203'
GIS+14'
FTX+AAG+++DELIVERY INSTRUCTIONS LINE 11111111111111111111111111111111:LINE 22222222222222'
FTX+AAG+++OTHER DELIVERY INSTRUCTIONS LINE 11111111111111111111111111:OTHER DELIVERY INSTRUCTIONS LINE 22222222222222'
FTX+AAO+++COMMODITY 1-3 DESCRIPTION TOO GENERIC, REQUIRES MORE DETAIL:COMMODITY 2-1 REQUIRES FURTHER DETAIL ON TYPE & SIZES'
FTX+AAO+++OTHER REJECT COMMENT LINE 111111111111111111111111111111111:OTHER REJECT COMMENT LINE 22222222222222'
ERP+2::20'
ERC+531'
ERC+01'
ERC+D40'
RFF+CN:CCN123456'
UNT+26+257'";

			#endregion

			#region Expected Email Body

			//An ERROR response message has been received from the CBSA for an Across/IID EDI Release.
			//The message sent for the above mentioned job had the following errors.

			//Reject Comments:
			//COMMODITY 1-3 DESCRIPTION TOO GENERIC, REQUIRES MORE DETAIL COMMODITY 2-1 REQUIRES FURTHER DETAIL ON TYPE & SIZES
			//OTHER REJECT COMMENT LINE 111111111111111111111111111111111 OTHER REJECT COMMENT LINE 22222222222222

			//Code	Error Text
			//531	Unknown error (531), please report to CargoWise
			//01	CCN not on file
			//D40	Consignee Postal Code: DECIMALS NOT VALID FOR ELEMENT VALUE

			const string expectedBody = @"</strong>An ERROR response message has been received from the CBSA for an Across/IID EDI Release.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
<strong>Reject Comments:</strong><br />
<p>COMMODITY 1-3 DESCRIPTION TOO GENERIC, REQUIRES MORE DETAIL COMMODITY 2-1 REQUIRES FURTHER DETAIL ON TYPE & SIZES</p>
<p>OTHER REJECT COMMENT LINE 111111111111111111111111111111111 OTHER REJECT COMMENT LINE 22222222222222</p>
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Code</th><th>Error Text</th></tr></thead><tr><td>531</td><td>Officer Manual Reject: Officer reject-Originating Country</td></tr><tr><td>01</td><td>CCN not on file</td></tr><tr><td>D40</td><td>Consignee Postal Code: DECIMALS NOT VALID FOR ELEMENT VALUE</td></tr></table>";

			#endregion

			entryHeader.CH_Status = EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.Error, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status", EDIReleaseImportEntryStatusList.Codes.Error, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status", EDIReleaseImportEntryStatusList.Codes.GoodsReleased, entryHeader.CH_Status);
			AssertEmail("Error Across/IID EDI Release Response for 12345000067897", expectedBody, messageText.Replace("\r\n", ""), ccRecipient: "blah@blah.com;staff1@wisetechglobal.com");
		}

		public void TestAcceptedMessageWhereLinkedObjectGottenByPCNNumber()
		{
			var entryNumber = declaration.AdditionalReferenceNumbers.AddNew();
			entryNumber.CE_EntryNum = "PCN123456";
			entryNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;
			Factory.Save();

			const string messageText = @"UNH+257+CUSRES:D:96A:UN'BGM+:::257+B99999999+11'DTM+9:200209251015:203'GIS+9'RFF+ED:PCN 123456'UNT+6+257'";
			const string expectedBody = "A 'Message Accepted' response has been received from the CBSA for an Across/IID EDI Release.";

			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status", EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ClearOriginal, entryHeader.CH_Status);
			AssertEmail("Accepted Across/IID EDI Release Response for PCN123456", expectedBody, messageText);
		}

		public void TestMessageWithInvalidGIS()
		{
			const string messageText = @"UNH+257+CUSRES:D:96A:UN'BGM+:::257+12345000067897+11'DTM+9:200209251015:203'GIS+1XX'RFF+ED:RC123420021100001'UNT+6+257'";
			const string errorMessage = "The message processor was unable to interpret received message.";

			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Failed, ediMessage.EM_Status);
			AssertEquals("Linked to entry", ZGuid.Empty, ediMessage.EM_LinkUniqueID);
			AssertNotEquals("Branch does not changed", newBranch.PK, ediMessage.EM_GB);
			AssertContains("EM_MessageInterpretation", errorMessage, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status does not change", string.Empty, entryHeader.CH_EntryStatus);
			AssertEquals("Entry Message Status does not change", MessageStatusList.Codes.AwaitingOriginal, entryHeader.CH_Status);

			AssertContains("LastLog", "EDI Release Response Message Processor: " + errorMessage, logger.DebugLogStrings[0]);
			AssertEmail("EDI Release Response Message Processor Error Report", errorMessage, messageText, string.Empty);
		}

		public void TestSyntaxErrorMessageWhereLinkedObjectGottenByDirectReference()
		{
			const string messageText = @"UNH+257+CUSRES:D:96A:UN'BGM+:::257+12345000067897+11'DTM+9:200902131330:203'GIS+14'ERP+2:237:28'UNT+5+1'";
			const string expectedBody = "An ERROR response message has been received from the CBSA for an Across/IID EDI Release.";

			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.Error, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status does not change", string.Empty, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status", MessageStatusList.Codes.ErrorOriginal, entryHeader.CH_Status);
			AssertEmail("Error Across/IID EDI Release Response for 12345000067897", expectedBody, messageText);
		}

		public void TestAcceptedMessageWhereMessageStatusIsNotAwaitingReply()
		{
			#region Expected Email Body

			//A 'Status Update' response has been received from the CBSA for an Across/IID EDI Release.
			//Please see message details below.

			//Field					Value
			//Processing Indicator	4 - Goods Released
			//Service Option		257 - Post-arrival EDI Release
			//Document Reference	12345000067897
			//Cargo Control Number	RC123420021100001
			//Processing Date		25-Sep-02 10:15

			const string expectedBody = @"</strong>A 'Status Update' message has been received from the CBSA for an Across/IID EDI Release.<br />
Please see message details below.<br />
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Field</th><th>Value</th></tr></thead><tr><td>Processing Indicator</td><td>4 - Goods Released</td></tr><tr><td>Service Option</td><td>257 - Post-arrival EDI Release</td></tr><tr><td>Document Reference</td><td>12345000067897</td></tr><tr><td>Cargo Control Number</td><td>RC123420021100001</td></tr><tr><td>Processing Date</td><td>25-Sep-02 10:15</td></tr></table>
<br />
<hr />";

			#endregion

			entryHeader.CH_Status = MessageStatusList.Codes.ErrorOriginal;
			Factory.Save();
			const string messageText = @"UNH+257+CUSRES:D:96A:UN'BGM+:::257+12345000067897+11'DTM+9:200209251015:203'GIS+4'RFF+ED:RC123420021100001'UNT+6+257'";
			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Received", EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.GoodsReleased, ediMessage.EM_MessageSubType);
			AssertEquals("Linked to entry", entryHeader.PK, ediMessage.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", expectedBody, ediMessage.EM_MessageInterpretation);
			AssertEquals("Entry Status", EDIReleaseImportEntryStatusList.Codes.GoodsReleased, entryHeader.CH_EntryStatus);
			AssertEquals("Message Status not changed", MessageStatusList.Codes.ErrorOriginal, entryHeader.CH_Status);
			AssertEmail("Across/IID EDI Release Status Update Message for 12345000067897", expectedBody, messageText.Replace("\r\n", ""));
		}

		public void TestEDIReleaseMessagesForJobsWithMultipleCCNs()
		{
			const string ccn1 = "CCN1", ccn2 = "CCN2";
			const string acceptedEmailBody = "A 'Message Accepted' response has been received from the CBSA for an Across/IID EDI Release.";
			const string statusUpdateEmailBody = "A 'Status Update' message has been received from the CBSA for an Across/IID EDI Release.";

			declaration.AdditionalReferenceNumbers[0].CE_EntryNum = ccn1;
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = ccn2;
			entryHeader.Messages.Add(helper.GetEDIReleaseMessage(EDIMessage.MessageNumberPlaceHolder, ZDateTime.Now, EDIMessage.Direction.Transmit, "4"));
			Factory.Save();

			var messageText = @"UNH+257+CUSRES:D:96A:UN'BGM+:::257+12345000067897+11'DTM+9:200902131330:203'GIS+14'UNT+5+1'";
			var ediMessage = (EDIReleaseMessage)helper.GetEDIReleaseResponseMessage(messageText, ZDateTime.Now.AddDays(1), "5");
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.Error, ediMessage.EM_MessageSubType);
			AssertEquals("Application Reference", ZString.Empty, ediMessage.EM_ApplicationReference);
			AssertEquals("Entry Status when error message received", EDIReleaseImportEntryStatusList.Codes.Error, entryHeader.CH_EntryStatus);

			entryHeader.Messages.Add(helper.GetEDIReleaseMessage(EDIMessage.MessageNumberPlaceHolder, ZDateTime.Now.AddDays(2), EDIMessage.Direction.Transmit, "6"));
			Factory.Save();

			ediMessage = GetEDIReleaseMessage(ProcessingIndicatorCodedList.GoodsReleased, ZDateTime.Now.AddDays(3), ccn1, "7");
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.GoodsReleased, ediMessage.EM_MessageSubType);
			AssertEquals("Application Reference", "CCN1", ediMessage.EM_ApplicationReference);
			AssertEquals("Entry Status when only one ccn released", EDIReleaseImportEntryStatusList.Codes.MultipleCargoControlNumber, entryHeader.CH_EntryStatus);
			AssertContains("EM_MessageInterpretation", acceptedEmailBody, ediMessage.EM_MessageInterpretation);

			ediMessage = GetEDIReleaseMessage(ProcessingIndicatorCodedList.GoodsRequiredForExamination, ZDateTime.Now.AddDays(3), ccn2, "8");
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.GoodsRequiredForExamination, ediMessage.EM_MessageSubType);
			AssertEquals("Application Reference", "CCN2", ediMessage.EM_ApplicationReference);
			AssertEquals("Entry Status when both ccns released but with different status", EDIReleaseImportEntryStatusList.Codes.MultipleCargoControlNumber, entryHeader.CH_EntryStatus);
			AssertContains("EM_MessageInterpretation", acceptedEmailBody, ediMessage.EM_MessageInterpretation);

			entryHeader.Messages.Add(GetRNSRequest(ccn1, RNSMessageTypes.Codes.StatusQuery));
			Factory.Save();

			ediMessage = GetEDIReleaseMessage(ProcessingIndicatorCodedList.GoodsReleased, ZDateTime.Now.AddDays(4), ccn2, "9");
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.GoodsReleased, ediMessage.EM_MessageSubType);
			AssertEquals("Application Reference", "CCN2", ediMessage.EM_ApplicationReference);
			AssertEquals("Entry Status when release statuses for both ccns are identical", EDIReleaseImportEntryStatusList.Codes.GoodsReleased, entryHeader.CH_EntryStatus);
			AssertContains("EM_MessageInterpretation", statusUpdateEmailBody, ediMessage.EM_MessageInterpretation);

			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingDelete;
			messageText = @"UNH+257+CUSRES:D:96A:UN'BGM+:::257+12345000067897+11'DTM+9:200902131330:203'GIS+9'UNT+5+1'";
			ediMessage = (EDIReleaseMessage)helper.GetEDIReleaseResponseMessage(messageText, ZDateTime.Now.AddDays(5), "10");
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("Message Sub Type", EDIReleaseImportEntryStatusList.Codes.Cancelled, ediMessage.EM_MessageSubType);
			AssertEquals("Branch changed", newBranch.PK, ediMessage.EM_GB);
			AssertEquals("Application Reference", ZString.Empty, ediMessage.EM_ApplicationReference);
			AssertEquals("Entry Status when job is cancelled", EDIReleaseImportEntryStatusList.Codes.Cancelled, entryHeader.CH_EntryStatus);
			AssertContains("EM_MessageInterpretation", acceptedEmailBody, ediMessage.EM_MessageInterpretation);
		}

		public void TestMessageInterpretationShipmentLink()
		{
			var shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_IsCancelled = false;
			shipment2.JS_UniqueConsignRef = "H10000001";
			var entryNumber = shipment2.Numbers.AddNew();
			entryNumber.CE_EntryNum = "CCN888";
			entryNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			Factory.Save();

			const string messageText1 = @"UNH+1+CUSRES:D:96A:UN'BGM+:::125+12345000067897+11'LOC+22+0351:129::3021'DTM+9:200912221030:203'GIS+9'ERP+2::20'RFF+CN:CCN123456'UNT+26+257'";

			var ediMessage1 = GetEDIReleaseMessage(messageText1);
			impMessageProcessor.ProcessMessage(ediMessage1);

			var messaging1 = RNSMessagingBOTest.GetRNSMessagingBO(shipment);
			AssertEquals("Message is attached to shipment", 2, messaging1.Messages.Count);

			ediMessage1 = (EDIReleaseMessage)messaging1.Messages[1];
			AssertEquals("Linked to shipment", shipment.PK, ediMessage1.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation contains link to Forwarding Shipment", "ControllerID=JobShipment&BusinessEntityPK=" + shipment.PK, ediMessage1.EM_MessageInterpretation);

			const string messageText2 = @"UNH+1+CUSRES:D:96A:UN'BGM+:::125+12345000067897+11'LOC+22+0351:129::3021'DTM+9:200912221030:203'GIS+9'ERP+2::20'RFF+CN:CCN888'UNT+26+257'";

			var ediMessage2 = GetEDIReleaseMessage(messageText2);
			var impMessageProcessor2 = new IMPMessageProcessor(logger);
			impMessageProcessor2.ProcessMessage(ediMessage2);

			var messaging2 = RNSMessagingBOTest.GetRNSMessagingBO(shipment2);
			AssertEquals("Message is attached to shipment", 1, messaging2.Messages.Count);

			ediMessage2 = (EDIReleaseMessage)messaging2.Messages[0];
			AssertEquals("Linked to shipment", shipment2.PK, ediMessage2.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation contains link to CFS Shipment", "ControllerID=ShipmentReceival&BusinessEntityPK=" + shipment2.PK, ediMessage2.EM_MessageInterpretation);
		}

		public void TestSettingSystemDefinedValue()
		{
			const string messageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::257+10207000008576+11'
LOC+22+0497:129::4570'
DTM+9:201505150909:203'
GIS+34'
RFF+XC:80368347464'
UNT+7+1'
";
			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			var transactionNum = ediMessage.GetSystemDefinedValue<ZString>(EDIReleaseMessage.Schema.TransactionNumber);
			var warehouse = ediMessage.GetSystemDefinedValue<ZString>(ReleaseStatus.Schema.RL_WarehouseCode);
			var releaseOffice = ediMessage.GetSystemDefinedValue<ZString>(ReleaseStatus.Schema.RL_ReleaseOffice);
			var ccNum = ediMessage.GetSystemDefinedValue<ZString>(EDIReleaseMessage.Schema.CargoControlNumber);

			AssertEquals("TransactionNumber", "10207000008576", transactionNum);
			AssertEquals("ReleaseOffice", "0497", releaseOffice);
			AssertEquals("WareHouseCode", "4570", warehouse);
			AssertEquals("CargoControlNumber", "80368347464", ccNum);
			AssertEquals("TransactionNumber", "10207000008576", ediMessage.TransactionNumber);
			AssertEquals("ReleaseOffice", "0497", ediMessage.ReleaseOffice);
			AssertEquals("WareHouseCode", "4570", ediMessage.WarehouseCode);
			AssertEquals("CargoControlNumber", "80368347464", ediMessage.CargoControlNumber);
		}

		public void TestProcessMessageWhenGISIs128()
		{
			const string messageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::489+80316106594186+11'
DTM+9:202406101428:203'
GIS+128'
ERP+2:3'
ERC+74'
RFF+XC:80316106594186'
UNT+8+1'
";

			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);
			AssertEquals(EDIMessage.Status.Received, ediMessage.EM_Status);
			AssertEquals(EDIReleaseImportEntryStatusList.Codes.AcceptedWithWarning, ediMessage.EM_MessageSubType);
		}

		public void TestSystemDefinedTransactionNumberWillNotSetIfInvalid()
		{
			const string messageText = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::489+0+11'
LOC+22+0497:129::4570'
DTM+9:201505150909:203'
GIS+34'
RFF+XC:80368347464'
UNT+7+1'
";
			var ediMessage = GetEDIReleaseMessage(messageText);
			impMessageProcessor.ProcessMessage(ediMessage);

			AssertEquals("TransactionNumber", ZString.Empty, ediMessage.TransactionNumber);
			AssertEquals("CargoControlNumber", "80368347464", ediMessage.CargoControlNumber);
		}

		public void TestKeepReleaseStatusWhenGetWTAAfterCLR()
		{
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARS;
			declaration.JE_EntryStatus = ZString.Empty;
			declaration.JE_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;

			var messageTextOfCLR = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::125+12345000067897+11'
LOC+22+0495:129::3046'
DTM+58:201207180851:203'
GIS+4'
RFF+XC:801036463535'
UNT+7+1'
";
			var ediMessageOfCLR = GetEDIReleaseMessage(messageTextOfCLR);
			ediMessageOfCLR.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			ediMessageOfCLR.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
			ediMessageOfCLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessageOfCLR.EM_Status = EDIMessage.Status.Queued;

			impMessageProcessor.ProcessMessage(ediMessageOfCLR);
			AssertEquals(MessageStatusList.Codes.ClearOriginal, entryHeader.CH_Status);
			AssertEquals(EDIReleaseImportEntryStatusList.Codes.GoodsReleased, entryHeader.CH_EntryStatus);

			var messageTextOfWTA = @"UNH+1+CUSRES:D:96A:UN'
BGM+:::125+12345000067897+11'
DTM+9:201207180848:203'
GIS+9'
UNT+5+1'
";
			var ediMessageOfWTA = GetEDIReleaseMessage(messageTextOfWTA);
			ediMessageOfWTA.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			ediMessageOfWTA.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.DeclarationAccepted;
			ediMessageOfWTA.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessageOfWTA.EM_Status = EDIMessage.Status.Queued;

			impMessageProcessor.ProcessMessage(ediMessageOfWTA);
			AssertEquals(MessageStatusList.Codes.ClearOriginal, entryHeader.CH_Status);
			AssertEquals(EDIReleaseImportEntryStatusList.Codes.GoodsReleased, entryHeader.CH_EntryStatus);
		}

		#region Implementation

		EDIReleaseMessage GetEDIReleaseMessage(ProcessingIndicatorCodedList indicator, ZDateTime createTime, string ccn = "", string interchangeNum = "1")
		{
			var messageText = string.Format(@"UNH+257+CUSRES:D:96A:UN'BGM+:::257+B99999999+11'DTM+9:200902131330:203'GIS+{0}'RFF+XC:{1}'UNT+5+1'", indicator, ccn);
			var result = (EDIReleaseMessage)helper.GetEDIReleaseResponseMessage(messageText, createTime, interchangeNum);
			result.EM_Status = EDIMessage.Status.Queued;
			result.EM_ApplicationReference = ccn;
			return result;
		}

		EDIReleaseMessage GetEDIReleaseMessage(string messageText)
		{
			var interchangeNum = ZDateTime.Now.ToString("yyMMddHHmmssfff");
			var result = (EDIReleaseMessage)helper.GetEDIReleaseResponseMessage(messageText, ZDateTime.UtcNow, interchangeNum);
			result.EM_Status = EDIMessage.Status.Queued;
			return result;
		}

		RNSRequestMessage GetRNSRequest(string ccn, string subType, string transactionNumber = "")
		{
			var request = helper.GetRNSRequest(ccn, transactionNumber, subType, ZDateTime.UtcNow);
			request.EM_GB = newBranch.PK;
			return request;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var refDatahelper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = Core.Constants.CountryCodes.Canada;
			var codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CBSAErrorCodes;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			refDatahelper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "R30", "Warehouse Office: Warehouse is mandatory", startDate, endDate);
			refDatahelper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "R31", "Client Supplied Request ID: Arrival is already on file with matching Port and Warehouse", startDate, endDate);
			refDatahelper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "R32", "Client Supplied Request ID: Arrival document accepted but no related document on file (Warning)", startDate, endDate);
			refDatahelper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "R72", "Warehouse Office: The Arrival Subloc provided to CBSA does not match the Port of Destination Subloc for the referenced Cargo document (Warning)", startDate, endDate);
			refDatahelper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "R73", "Arrival Document: The Arrival document has been expired", startDate, endDate);
			refDatahelper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "R74", "Work Location Code (Destination): Arrival Port provided to CBSA does not match the Port of Destination for the referenced Cargo document (Warning)", startDate, endDate);
			refDatahelper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "R79", "Warehouse office: Warehouse must be related to Port of Arrival provided", startDate, endDate);
			refDatahelper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "R80", "Warehouse office: Warehouse cannot be Customs Office", startDate, endDate);
			refDatahelper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "531", "Officer Manual Reject: Officer reject-Originating Country", startDate, endDate);
			refDatahelper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "D40", "Consignee Postal Code: DECIMALS NOT VALID FOR ELEMENT VALUE", startDate, endDate);
			refDatahelper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "W34", "Client Supplied Request ID: Request already in arrived status", startDate, endDate);
			refDatahelper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "R09", "Work Location Code (Destination): ARRIVAL OFFICE DOES NOT MATCH RELEASE", startDate, endDate);

			newBranch = Factory.New<GlbBranch>();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			newBranch.GB_Code = "B1";
			newBranch.GB_IsActive = true;
			newBranch.GB_RL_NKHomePort = "CAVAN";
			Factory.Save();

			using (DisposableEnvironment.ForBranch(newBranch.PK.ToGuid()))
			{
				helper = new DeclarationTestHelper(Factory, true);
				impMessageProcessor = new IMPMessageProcessor(logger);

				declaration = (JobDeclaration)JobDeclaration.New(Factory);
				declaration.TransactionNumber.AccountSecurityCode = "12345";
				declaration.TransactionNumber.SequentialNumber = "00006098";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_ServiceOption = ZString.Empty;
				declaration.JE_IsCancelled = false;

				entryHeader = declaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
				entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

				var sentMessage = helper.GetEDIReleaseMessage(EDIMessage.MessageNumberPlaceHolder, ZDateTime.Now.AddDays(-5), EDIMessage.Direction.Transmit, "20");
				sentMessage.EM_SystemCreateUser = userToNotify.GS_Code;
				entryHeader.Messages.Add(sentMessage);

				sentMessage = helper.GetRNSRequest("CNN987456", "", RNSMessageTypes.Codes.StatusQuery, ZDateTime.Now.AddDays(-4));
				sentMessage.EM_LinkUniqueID = entryHeader.PK;
				sentMessage.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
				sentMessage.EM_SystemCreateUser = userToNotify.GS_Code;

				declaration = (JobDeclaration)JobDeclaration.New(Factory);
				declaration.TransactionNumber.AccountSecurityCode = "12345";
				declaration.TransactionNumber.SequentialNumber = "00006789";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_ServiceOption = ZString.Empty;
				declaration.JE_IsCancelled = false;

				entryHeader = declaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
				entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

				sentMessage = helper.GetEDIReleaseMessage(EDIMessage.MessageNumberPlaceHolder, ZDateTime.Now.AddDays(-2));
				sentMessage.EM_SystemCreateUser = userToNotify.GS_Code;
				entryHeader.Messages.Add(sentMessage);

				declarationCCN = declaration.AdditionalReferenceNumbers.AddNew();
				declarationCCN.CE_EntryNum = "CCN123456";
				declarationCCN.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
				declarationCCN.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

				consol = Factory.New<ForwardingConsol>();
				consolCCN = consol.Numbers.AddNew();
				consolCCN.CE_EntryNum = "CCN123456";
				consolCCN.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
				consolCCN.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

				sentMessage = helper.GetEDIReleaseMessage(EDIMessage.MessageNumberPlaceHolder, ZDateTime.Now.AddDays(-2), EDIMessage.Direction.Transmit, "2");
				sentMessage.EM_SystemCreateUser = userToNotify.GS_Code;
				consol.Messages.Add(sentMessage);

				shipment = Factory.New<ForwardingShipment>();
				shipment.JS_IsCancelled = false;
				shipmentCCN = shipment.Numbers.AddNew();
				shipmentCCN.CE_EntryNum = "CCN123456";
				shipmentCCN.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
				shipmentCCN.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

				sentMessage = helper.GetEDIReleaseMessage(EDIMessage.MessageNumberPlaceHolder, ZDateTime.Now.AddDays(-2), EDIMessage.Direction.Transmit, "3");
				sentMessage.EM_SystemCreateUser = userToNotify.GS_Code;
				shipment.Messages.Add(sentMessage);

				shipmentUserToNotify = Factory.NewWithValidTestData<GlbStaff>();
				shipmentUserToNotify.GS_FullName = "ShipmentUserToNotify";
				shipmentUserToNotify.GS_EmailAddress = "ShipmentUserToNotify@blah.com";
				shipmentUserToNotify.GS_Code = "SNU";
			}

			var notifyCompany = Factory.NewWithValidTestData<GlbCompany>();
			notifyCompany.GC_RN_NKCountryCode = "CA";
			notifyCompany.GC_Code = "CA1";
			notifyCompany.GC_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			notifyCompany.OrgProxy.OH_Code = "CA1OH";
			notifyBranch = notifyCompany.Branches.AddNew();
			notifyBranch.GB_Code = "CB1";
			notifyBranch.GB_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			notifyBranch.OrgProxy.OH_Code = "CB1OH";
			notifyBranch.OrgProxy.CustomsCodes.AddNew("CCP", "3021", "CA");

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "T1";
			staff.GS_LoginName = "STAFF1";
			staff.GS_EmailAddress = "staff1@wisetechglobal.com";
			var mailGroup = Factory.New<GlbGroup>();
			mailGroup.GG_Code = "GG1";
			mailGroup.Staff.Add(staff);
			CACustomsDataRegistry.Instance.SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries.SetValue(Guid.Empty, notifyBranch.PK.ToGuid(), Guid.Empty, mailGroup.PK.ToGuid());
			Factory.Save();
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		IMPMessageProcessor impMessageProcessor;
		GlbBranch newBranch;
		ForwardingConsol consol;
		ForwardingShipment shipment;
		DeclarationTestHelper helper;
		GlbStaff shipmentUserToNotify;
		GlbBranch notifyBranch;
		CusEntryNumber declarationCCN;
		CusEntryNumber consolCCN;
		CusEntryNumber shipmentCCN;

		#endregion
	}
}
