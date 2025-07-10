using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eHubMessaging.Business.DownloadHandler.Tests
{
	class UniversalInterchangeHeaderHandlerTest : TestCaseWithFactory
	{
		//    public void TestFailureWithUnrecognisedBodyTagsIsSplitIOntoOneMessage()
		//    {
		//      var interchange = SubmitMessageAndLoadResultingInterchange(@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
		//  <Header>
		//    <SenderID>centst</SenderID>
		//    <RecipientID>CENMEMTST</RecipientID>
		//  </Header>
		//	<Body>
		//		<GodKnowsWhatToMakeOfThis>
		//		</GodKnowsWhatToMakeOfThis>
		//	</Body>
		//</UniversalInterchange>");
		//      AssertNotNull("interchange", interchange);
		//      CombineAssertions(delegate
		//      {
		//        AssertEquals("interchange.EI_ApplicationCode", XmlEDIInterchange.ApplicationCodes.UniversalDataMessaging, interchange.EI_ApplicationCode);
		//        AssertEquals("interchange.EI_Status", XmlEDIInterchange.Status.Failed, interchange.EI_Status);
		//        AssertEquals("interchange.Messages.Count", 1, interchange.Messages.Count);
		//      });

		//      var message = interchange.Messages[0];

		//      CombineAssertions(delegate
		//      {
		//        AssertEquals("message.EM_ApplicationCode", XmlEDIMessage.ApplicationCodes.UniversalDataMessaging, message.EM_ApplicationCode);
		//        AssertEquals("message.EM_MessageType", "", message.EM_MessageType);
		//        AssertEquals("message.EM_MessageSubType", "", message.EM_MessageSubType);
		//        AssertEquals("message.EM_Status", XmlEDIMessage.Status.Discarded, message.EM_Status);
		//        AssertEquals("message.EM_MessageText", "<GodKnowsWhatToMakeOfThis />", message.EM_MessageText);
		//      });
		//    }

		public void TestFailureWithInvalidContentIsLoggedSomehow()
		{
			var interchange = SubmitMessageAndLoadResultingInterchange(@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>centst</SenderID>
    <RecipientID>CENMEMTST</RecipientID>
  </Header>
	<Body>
		<GodKnowsWhatToMakeOfThis>
	</Body>
</UniversalInterchange>");
			AssertNotNull("interchange", interchange);
			CombineAssertions(delegate
			{
				AssertEquals("interchange.EI_ApplicationCode", XmlEDIInterchange.ApplicationCodes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_Status", XmlEDIInterchange.Status.Failed, interchange.EI_Status);
			});
			var failNotes = interchange.Notes.FindByDescription("Failure Log");
			AssertNotNull("failNotes", failNotes);
			AssertEquals("failNotes.Length", 1, failNotes.Length);
			var failNote = failNotes[0];
			AssertStartsWith("failNote.ST_NoteDataAsText", @"
Length of the message content is 510 before Get Payload Sub Type
An error occurred: The 'GodKnowsWhatToMakeOfThis' start tag on line 7 position 4 does not match the end tag of 'Body'. Line 8, position 4.
			".Trim(), failNote.ST_NoteDataAsText);
		}

		public void TestFailureWhereBodyIsMissingIsLoggedInANote()
		{
			var interchange = SubmitMessageAndLoadResultingInterchange(@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>centst</SenderID>
    <RecipientID>CENMEMTST</RecipientID>
  </Header>
  <Body>
  </Body>
</UniversalInterchange>");
			AssertNotNull("interchange", interchange);
			CombineAssertions(delegate
			{
				AssertEquals("interchange.EI_ApplicationCode", XmlEDIInterchange.ApplicationCodes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_Status", XmlEDIInterchange.Status.Failed, interchange.EI_Status);
			});
			var failNotes = interchange.Notes.FindByDescription("Failure Log");
			AssertNotNull("failNotes", failNotes);
			AssertEquals("failNotes.Length", 1, failNotes.Length);
			var failNote = failNotes[0];
			AssertStartsWith("failNote.ST_NoteDataAsText", @"
Length of the message content is 454 before Get Payload Sub Type
Got Payload Sub Type Name of []
Found a Payload Match.
An error occurred: ReadSubtree()
			".Trim(), failNote.ST_NoteDataAsText);
		}

		XmlEDIInterchange SubmitMessageAndLoadResultingInterchange(string messageWithNoBody)
		{
			var trackingID = Guid.NewGuid();
			var handler = new UniversalInterchangeHeaderHandler();
			using (var messageStream = new MemoryStream(Encoding.Unicode.GetBytes(messageWithNoBody)))
			{
				IeHubMessage wrappedMessage = new eHubMessage(trackingID,
					"XXXZZZXXX", // Sender should not matter.
					GlbCompany.CurrentCompany.LicenceKeyIdentifier,
					MessageSchemaType.Xml,
					"XXX", // Apparently the "Application Code" is ignored in favour of parsing the content.
					"http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange",
					messageStream);

				handler.SaveMessageFromAdapter(wrappedMessage);
			}

			return Factory.LoadTop1<XmlEDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_SessionGUID, trackingID));
		}
		XmlEDIInterchange SubmitUniversalMessageAndLoadResultingInterchange(string messageWithNoBody, string senderID, string appCode)
		{
			var trackingID = Guid.NewGuid();
			var handler = new UniversalInterchangeHeaderHandler();
			using (var messageStream = new MemoryStream(Encoding.Unicode.GetBytes(messageWithNoBody)))
			{
				IeHubMessage wrappedMessage = new eHubMessage(trackingID,
					senderID, // Sender should not matter.
					GlbCompany.CurrentCompany.LicenceKeyIdentifier,
					MessageSchemaType.Xml,
					appCode,
					"http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange",
					messageStream);

				handler.SaveMessageFromAdapter(wrappedMessage);
			}

			return Factory.LoadTop1<XmlEDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_SessionGUID, trackingID));
		}

		//public void TestNullIEhubMessageDoesntBlowTheFarm() - Should uncomment this at some point in time to make sure no null ref exceptions get thrown.
		//{
		//  var handler = new UniversalInterchangeHeaderHandler();
		//  handler.SaveMessageFromAdapter(null);
		//}

		#region Interchange with mixed message types
		const string sampleInterchange = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Body>
		<UniversalEvent>
			<Event>
				<DataContext>
					<CodesMappedToTarget>true</CodesMappedToTarget>
					<DataTargetCollection>
						<DataTarget>
							<Type>ForwardingConsol</Type>
							<Key>C00679172</Key>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventTime>2012-10-28T09:30:00</EventTime>
				<EventType>MAA</EventType>
				<EventReference>Dakosy Port Message Acknowledged. Ref Z12000007639|DEP=Dakosy</EventReference>
				<ContextCollection>
					<Context>
						<Type>Description</Type>
						<Value>Message Accepted</Value>
					</Context>
					<Context>
						<Type>Session Number</Type>
						<Value>0000039596</Value>
					</Context>
					<Context>
						<Type>Type of Form</Type>
						<Value>HDS</Value>
					</Context>
					<Context>
						<Type>Recipient</Type>
						<Value>ZAES</Value>
					</Context>
					<Context>
						<Type>Z or B-Number</Type>
						<Value>Z12000007639</Value>
					</Context>
					<Context>
						<Type>Customs Status</Type>
						<Value>NRL</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>
		<UniversalShipment>
			<Shipment>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Type>ForwardingConsol or ForwardingShipment depending on C</Type>
							<Key>C00679172</Key>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<AdditionalReferenceCollection>
					<AdditionalReference>
						<Type>
							<Code>SZB</Code>
							<Description>SZB Number</Description>
						</Type>
						<ReferenceNumber>Z12000007639</ReferenceNumber>
						<ContextInformation>NOT RELEASED</ContextInformation>
						<IssueDate>2012-10-28T09:30:00</IssueDate>
					</AdditionalReference>
				</AdditionalReferenceCollection>
			</Shipment>
		</UniversalShipment>
		<UniversalEvent>
			<Event>
				<DataContext>
					<CodesMappedToTarget>true</CodesMappedToTarget>
					<DataTargetCollection>
						<DataTarget>
							<Type />
							<Key />
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventTime>2017-01-10T22:28:30</EventTime>
				<EventType>MPP</EventType>
				<EventReference>Dakosy Processing Report|DEP=Dakosy</EventReference>
				<ContextCollection>
					<Context>
						<Type>Description</Type>
						<Value>Message Pending Processing</Value>
					</Context>
					<Context>
						<Type>Session Number</Type>
						<Value>0000000206</Value>
					</Context>
					<Context>
						<Type>Number of messages without errors</Type>
						<Value>1</Value>
					</Context>
					<Context>
						<Type>Number of messages with errors</Type>
						<Value>0</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>
		<UniversalEvent>
			<Event>
				<DataContext>
					<CodesMappedToTarget>true</CodesMappedToTarget>
					<DataTargetCollection>
						<DataTarget>
							<Type>ForwardingConsol</Type>
							<Key>C00679172</Key>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventTime>2012-10-28T09:32:00</EventTime>
				<EventType>IRA</EventType>
				<EventReference>Quay Order Delivery Confirmation|DEP=Dakosy</EventReference>
				<ContextCollection>
					<Context>
						<Type>Description</Type>
						<Value>Interchange Receipt Acknlowledged</Value>
					</Context>
					<Context>
						<Type>Session Number</Type>
						<Value>0000039596</Value>
					</Context>
					<Context>
						<Type>Type of Form</Type>
						<Value>HDS</Value>
					</Context>
					<Context>
						<Type>Recipient</Type>
						<Value>DKY</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>
		<UniversalEvent>
			<Event>
				<DataContext>
					<CodesMappedToTarget>true</CodesMappedToTarget>
					<DataTargetCollection>
						<DataTarget>
							<Type>ForwardingShipment</Type>
							<Key>S13SZRH0005026</Key>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventTime>2013-12-18T13:29:00</EventTime>
				<EventType>MAA</EventType>
				<EventReference>Dakosy Port Message Acknowledged. Ref B13000384900|DEP=Dakosy</EventReference>
				<ContextCollection>
					<Context>
						<Type>Description</Type>
						<Value>Message Accepted</Value>
					</Context>
					<Context>
						<Type>Session Number</Type>
						<Value>0000013493</Value>
					</Context>
					<Context>
						<Type>Type of Form</Type>
						<Value>HDS</Value>
					</Context>
					<Context>
						<Type>Recipient</Type>
						<Value>ZAPP</Value>
					</Context>
					<Context>
						<Type>Z or B-Number</Type>
						<Value>B13000384900</Value>
					</Context>
					<Context>
						<Type>Customs Status</Type>
						<Value />
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>
		<UniversalShipment>
			<Shipment>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Type>ForwardingConsol or ForwardingShipment depending on S</Type>
							<Key>S13SZRH0005026</Key>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<AdditionalReferenceCollection>
					<AdditionalReference>
						<Type>
							<Code>SZB</Code>
							<Description>SZB Number</Description>
						</Type>
						<ReferenceNumber>B13000384900</ReferenceNumber>
						<ContextInformation />
						<IssueDate>2013-12-18T13:29:00</IssueDate>
					</AdditionalReference>
				</AdditionalReferenceCollection>
			</Shipment>
		</UniversalShipment>
	</Body>
</UniversalInterchange>";

		#endregion

		public void TestUniversalInterchangeContainsMixedUniversalMessageTypes()
		{
			var interchange = SubmitMessageAndLoadResultingInterchange(sampleInterchange);
			var messages = Factory.Load<XmlEDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
			var failNotes = interchange.Notes.FindByDescription("Failure Log");
			CombineAssertions(delegate
			{
				AssertNotNull("interchange", interchange);
				AssertEquals("interchange.EI_ApplicationCode", XmlEDIInterchange.ApplicationCodes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_Status", XmlEDIInterchange.Status.Received, interchange.EI_Status);
				AssertEquals("Number of message created", 6, messages.Length);
				AssertEquals("UniversalShipment", 2, messages.Where(message => message.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalShipment).Count());
				AssertEquals("UniversalEvent", 4, messages.Where(message => message.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent).Count());
				AssertEquals("No logs", 0, failNotes.Length);
			});
		}

		public void TestUniversalInterchangeFromUSEBond()
		{
			var interchange = SubmitUniversalMessageAndLoadResultingInterchange(@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>USCustomsEBond</SenderID>
    <RecipientID>DAUDAUDAU</RecipientID>
  </Header>
	<Body>
		<UniversalEvent>
		</UniversalEvent>
	</Body>
</UniversalInterchange>", "USCustomsEBond", "XXX");
			AssertNotNull("interchange", interchange);
			var messages = Factory.Load<XmlEDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
			CombineAssertions(delegate
			{
				AssertNotNull("interchange", interchange);
				AssertEquals("interchange.EI_ApplicationCode", XmlEDIInterchange.ApplicationCodes.USeBond, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_Status", XmlEDIInterchange.Status.Received, interchange.EI_Status);
				AssertEquals("Number of message created", 1, messages.Length);
				AssertEquals("UniversalEvent", 1, messages.Where(message => message.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent).Count());
			});
		}
		public void TestUniversalInterchangeFromNexDocForRexAcknowledgeOwnershipResponse()
		{
			string message = @"<s0:UniversalInterchange xmlns:s0=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:rex= ""http://agriculture.gov.au/nexdoc/RexOwnershipSoap_1.0"" xmlns:com=""http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"" >
									<s0:Header>
										<s0:SenderID/>
										<s0:RecipientID/>
										<s0:DeliveryMetadata>
											<s0:ValueCollection>
												<s0:Value>
													<s0:Name>RexNumber</s0:Name>
													<s0:Type>String</s0:Type>
													<s0:Data>REX0000028829</s0:Data>
												</s0:Value>
												<s0:Value>
													<s0:Name>JobNumber</s0:Name>
													<s0:Type>String</s0:Type>
													<s0:Data>B0000001</s0:Data>
												</s0:Value>
											</s0:ValueCollection>
										</s0:DeliveryMetadata>
										</s0:Header>
										<s0:Body>
 										<rex:RexAcknowledgeOwnershipResponse>
												<rex:outcome>CLOSED_REJECTED</rex:outcome>
										</rex:RexAcknowledgeOwnershipResponse>
									</s0:Body>
								</s0:UniversalInterchange>";
			AssertUniversalInterchangeMessage(message, XmlEDIInterchange.ApplicationCodes.NEXDOCS);
		}

		public void TestUniversalInterchangeCMD()
		{
			var message = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header>
    <SenderID>CCN</SenderID>
    <RecipientID>WTLDSGSGC</RecipientID>
  </Header>
	<Body>
		<CMD>
		</CMD>
</Body>
</UniversalInterchange>";
			AssertUniversalInterchangeMessage(message, XmlEDIInterchange.ApplicationCodes.SingaporeCMD);
		}

		public void TestUniversalInterchangeFromNexDocForRexTransferOwnershipResponse()
		{
			string message = @"<s0:UniversalInterchange xmlns:s0=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:rex= ""http://agriculture.gov.au/nexdoc/RexOwnershipSoap_1.0"" xmlns:com=""http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"">
									<s0:Header>
 										<s0:SenderID/>
 										<s0:RecipientID/>
										<s0:DeliveryMetadata>
											<s0:ValueCollection>
												<s0:Value>
													<s0:Name>RexNumber</s0:Name>
													<s0:Type>String</s0:Type>
													<s0:Data>REX0000028829</s0:Data>
												</s0:Value>
												<s0:Value>
													<s0:Name>JobNumber</s0:Name>
													<s0:Type>String</s0:Type>
													<s0:Data>B0000001</s0:Data>
												</s0:Value>
											</s0:ValueCollection>
										</s0:DeliveryMetadata>
									</s0:Header>
									<s0:Body>
										<rex:RexTransferOwnershipResponse>
											<rex:outcome>OPEN_PENDING</rex:outcome>
										</rex:RexTransferOwnershipResponse>
									</s0:Body>
								</s0:UniversalInterchange>";
			AssertUniversalInterchangeMessage(message, XmlEDIInterchange.ApplicationCodes.NEXDOCS);
		}
		public void TestUniversalInterchangeFromNexDocForRexWithdrawOwnershipResponse()
		{
			string message = @"<s0:UniversalInterchange xmlns:s0 = ""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:rex= ""http://agriculture.gov.au/nexdoc/RexOwnershipSoap_1.0"" xmlns:com=""http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"">
									<s0:Header>
										<s0:SenderID/>
										<s0:RecipientID/>
										<s0:DeliveryMetadata>
											<s0:ValueCollection>
												<s0:Value>
													<s0:Name>RexNumber</s0:Name>
													<s0:Type>String</s0:Type>
													<s0:Data>REX0000028829</s0:Data>
												</s0:Value>
												<s0:Value>
													<s0:Name>JobNumber</s0:Name>
													<s0:Type>String</s0:Type>
													<s0:Data>B0000001</s0:Data>
												</s0:Value>
											</s0:ValueCollection>
										</s0:DeliveryMetadata>
									</s0:Header>
									<s0:Body>
										<rex:RexWithdrawOwnershipResponse>
											<rex:outcome>CLOSED_WITHDRAW</rex:outcome>
										</rex:RexWithdrawOwnershipResponse>
									</s0:Body>
								</s0:UniversalInterchange>";

			AssertUniversalInterchangeMessage(message, XmlEDIInterchange.ApplicationCodes.NEXDOCS);
		}
		public void TestUniversalInterchangeFromNexDocForRexForwardOwnershipResponse()
		{
			string message = @"<s0:UniversalInterchange xmlns:s0=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:rex=""http://agriculture.gov.au/nexdoc/RexOwnershipSoap_1.0"" xmlns:com=""http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"">
													  <s0:Header>
														 <s0:SenderID/>
														 <s0:RecipientID/>
														 <s0:DeliveryMetadata>
																<s0:ValueCollection>
																	<s0:Value>
																		<s0:Name>RexNumber</s0:Name>
													 					<s0:Type>String</s0:Type>
																		<s0:Data>REX0000028829</s0:Data>
																	</s0:Value>
																	<s0:Value>
																		<s0:Name>JobNumber</s0:Name>
																		<s0:Type>String</s0:Type>
																		<s0:Data>B0000001</s0:Data>
																	</s0:Value>
																</s0:ValueCollection>
														 </s0:DeliveryMetadata>
													  </s0:Header>
													  <s0:Body>
														<rex:RexForwardOwnershipResponse>
															<rex:outcome>OPEN_PENDING</rex:outcome>
														</rex:RexForwardOwnershipResponse>
													  </s0:Body>
												   </s0:UniversalInterchange>";

			AssertUniversalInterchangeMessage(message, XmlEDIInterchange.ApplicationCodes.NEXDOCS);
		}
		public void AssertUniversalInterchangeMessage(string xmlMessage, string appCode)
		{
			var interchange = SubmitUniversalMessageAndLoadResultingInterchange(xmlMessage, "unknownSender", appCode);

			var messages = Factory.Load<XmlEDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
			CombineAssertions(delegate
			{
				AssertNotNull("interchange", interchange);
				AssertEquals("interchange.EI_ApplicationCode", appCode, interchange.EI_ApplicationCode);
				Assert("interchange.EI_HeaderNText", string.IsNullOrEmpty(interchange.EI_HeaderNText));
				Assert("interchange.EI_BodyText", interchange.EI_BodyText.Length > 0);
				AssertEquals("interchange.EI_Status", XmlEDIInterchange.Status.Received, interchange.EI_Status);
				AssertEquals("Number of message created", 1, messages.Length);
				AssertEquals("EDIMessage code", 1, messages.Where(message => message.EM_ApplicationCode == appCode).Count());
				AssertEquals("EDIMessage text", 1, messages.Where(message => !message.EM_MessageText.IsEmpty).Count());
				AssertEquals("EDIMessage status", XmlEDIMessage.Status.Queued, messages.First(message => !message.EM_MessageText.IsEmpty).EM_Status);
			});
		}
		public void TestUniversalInterchangeFromUSEBondTest()
		{
			var interchange = SubmitUniversalMessageAndLoadResultingInterchange(@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>USCustomsEBond</SenderID>
    <RecipientID>DAUDAUDAU</RecipientID>
  </Header>
	<Body>
		<UniversalEvent>
		</UniversalEvent>
</Body>
</UniversalInterchange>", "USCustomsEBondTest", "XXX");
			AssertNotNull("interchange", interchange);
			var messages = Factory.Load<XmlEDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
			CombineAssertions(delegate
			{
				AssertNotNull("interchange", interchange);
				AssertEquals("interchange.EI_ApplicationCode", XmlEDIInterchange.ApplicationCodes.USeBond, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_Status", XmlEDIInterchange.Status.Received, interchange.EI_Status);
				AssertEquals("Number of message created", 1, messages.Length);
				AssertEquals("UniversalEvent", 1, messages.Where(message => message.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent).Count());
			});
		}

		public void TestMessageWithUnknownType()
		{
			var interchange = SubmitMessageAndLoadResultingInterchange(@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>centst</SenderID>
    <RecipientID>CENMEMTST</RecipientID>
  </Header>
	<Body>
		<UniversalEvent>
		</UniversalEvent>
		<EEEEEvent>
		</EEEEEvent>
		<UniversalShipment>
		</UniversalShipment>
		<UniversalEvent>
		</UniversalEvent>
</Body>
</UniversalInterchange>");
			AssertNotNull("interchange", interchange);
			var messages = Factory.Load<XmlEDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
			CombineAssertions(delegate
			{
				AssertNotNull("interchange", interchange);
				AssertEquals("interchange.EI_ApplicationCode", XmlEDIInterchange.ApplicationCodes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_Status", XmlEDIInterchange.Status.Received, interchange.EI_Status);
				AssertEquals("Number of message created", 3, messages.Length);
			});
		}

		public void TestInvalidMessage()
		{
			var interchange = SubmitMessageAndLoadResultingInterchange(@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>centst</SenderID>
		<RecipientID>CENMEMTST</RecipientID>
  </Header>
	<Body>
		<UniversalEvent>
		</UniversalEvent>
		</UniversalShipment>
		<UniversalShipment>
		</UniversalShipment>
	</Body>
</UniversalInterchange>");
			AssertNotNull("interchange", interchange);
			var failNotes = interchange.Notes.FindByDescription("Failure Log");
			AssertNotNull("failNotes", failNotes);
			AssertEquals("failNotes.Length", 1, failNotes.Length);
			var failNote = failNotes[0];
			AssertStartsWith("failNote.ST_NoteDataAsText", @"
Length of the message content is 664 before Get Payload Sub Type
An error occurred: The 'Body' start tag on line 6 position 3 does not match the end tag of 'UniversalShipment'. Line 9, position 5."
.Trim(), failNote.ST_NoteDataAsText);
			var messages = Factory.Load<XmlEDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
			CombineAssertions(delegate
			{
				var failedMessage = $@"Failed to create message - Interchange Session GUID - {interchange.EI_SessionGUID}, Sender - XXXZZZXXX, Recipient - EDIEDIDAT, Schema - http://www.cargowise.com/Schemas/Universal/2011/11#Universa".Trim();
				AssertNotNull("interchange", interchange);
				AssertEquals("interchange.EI_ApplicationCode", XmlEDIInterchange.ApplicationCodes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_Status", XmlEDIInterchange.Status.Failed, interchange.EI_Status);
				AssertEquals("Number of message created", 0, messages.Length);
				AssertEquals("UniversalShipment", 0, messages.Where(message => message.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalShipment).Count());
				AssertEquals("UniversalEvent", 0, messages.Where(message => message.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent).Count());
			});
		}
	}
}
