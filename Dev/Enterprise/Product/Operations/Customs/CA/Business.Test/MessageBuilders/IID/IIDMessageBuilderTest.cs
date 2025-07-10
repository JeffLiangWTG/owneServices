using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class IIDMessageBuilderTest : TestCaseWithFactory
	{
		public void TestPupulatePackingLineCollectionOnShipment()
		{
			var actionPurpose = IIDMessageSubTypeList.Codes.Original;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001111";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_MasterBill = "ABC123";

			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 15;
			package1.CW_PackType = Core.Constants.PkgUnit.Bag;
			package1.CW_HouseBill = "ABC123";

			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 2;
			package2.CW_PackType = Core.Constants.PkgUnit.Box;
			package2.CW_CW_Parent = package1.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			SendsMessagesToCustomsShutterUpperer mergeResult = new SendsMessagesToCustomsShutterUpperer(false);
			mergeResult.AnswerToContinueWithAction = true;
			declaration.DoMerge(mergeResult);
			var entry = declaration.ReleaseEntryHeader;

			var dataWrapper = new IIDMessageWrapper(entry);
			var builder = new IIDMessageBuilder(actionPurpose, dataWrapper);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;

			AssertContains("Content", "<PackingLineCollection Content=\"Complete\">", message.EM_MessageText);
			AssertContains("Contact", "<BillNumber>ABC123</BillNumber>", message.EM_MessageText);
			AssertContains("Contact", "<PackQty>15</PackQty>", message.EM_MessageText);
		}

		public void TestPopulateMessagesCoreWhenLatestSentAcceptedMessageEM_MessageTextIsEmpty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			declaration.JE_DeclarationReference = "B00001111";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_BrandName = "BRN";
			invoiceLine.JI_Description = "ABC";

			Factory.Save();

			var createTimeUtc = ZDateTime.UtcNow;
			var dataWrapper = new IIDMessageWrapper(entryHeader);
			var builder = new IIDMessageBuilder(IIDMessageSubTypeList.Codes.Original, dataWrapper);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			message.EM_Status = MessageStatusList.Codes.Sent;
			message.EM_SystemCreateTimeUtc = createTimeUtc;
			message.EM_MessageText = @"<s0:UniversalEvent><s0:Event><s0:EventType>MAA</s0:EventType><s0:EventReference>IID Accepted</s0:EventReference><s0:ContextCollection><s0:Context><s0:Type>InterchangeNumber</s0:Type><s0:Value>10484</s0:Value></s0:Context><s0:Context><s0:Type>MessageNumber</s0:Type><s0:Value>1</s0:Value></s0:Context><s0:Context><s0:Type>IsTest</s0:Type><s0:Value>Y</s0:Value></s0:Context><s0:Context><s0:Type>OrganizationReference</s0:Type><s0:Value>B00172152</s0:Value></s0:Context></s0:ContextCollection></s0:Event></s0:UniversalEvent>";

			var messageMAA = Factory.New<UniversalEventMessage>();
			messageMAA.EM_MessageSubType = UniversalEventMessageTypes.Codes.IIDResponses;
			messageMAA.EM_MessageText = message.EM_MessageText;
			messageMAA.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageMAA.EM_SystemCreateTimeUtc = createTimeUtc;

			var stmAlog = Factory.New<StmALog>();
			using (stmAlog.LockForUpdatingKeyFieldsForTesting())
			{
				stmAlog.SL_Parent = declaration.PK;
				stmAlog.SL_Table = JobDeclaration.Schema.TableName;
			}
			var genPivot = Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = messageMAA.PK;
			genPivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;

			Factory.Save();

			var messages = ((IEDIMessageCollectionProvider)dataWrapper).Messages;
			AssertEquals(1, messages.Count);

			var builder3 = new IIDMessageBuilder(IIDMessageSubTypeList.Codes.Cancellation, dataWrapper);
			var exception = AssertExceptionThrown<InvalidMessageContentException>(() => builder3.PopulateMessages());
			AssertEquals("Failed to create IID Cancellation message. Please try to save the changes and send the IID Cancellation message again.", exception.Message.Trim());
			AssertEquals(@"Message: Object reference not set to an instance of an object.
EDIMessage Information:
Type: Enterprise.Customs.CA.Business.IIDUniversalShipmentMessage
Message Type: IID
MessageType Sub Type: ORG
Application Code: CAI
Message Number: 1
Status: SNT
Direction: TRX
Message Text: 
Message NText: 
Message Data: System.Byte[]
Branch: BNE
Company: EDI
DataContext is Null: True", exception.InnerException.Message.Trim());
			messages = ((IEDIMessageCollectionProvider)dataWrapper).Messages;
			AssertEquals(1, messages.Count);
		}

		public void TestPopulateMessagesCoreWhenUniversalShipmentIsNull()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			declaration.JE_DeclarationReference = "B00001111";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_BrandName = "BRN";
			invoiceLine.JI_Description = "ABC";

			var dataWrapper = new IIDMessageWrapperForTest(entryHeader);

			var builder3 = new IIDMessageBuilder(IIDMessageSubTypeList.Codes.Original, dataWrapper);
			var exception = AssertExceptionThrown<InvalidMessageContentException>(() => builder3.PopulateMessages());
			AssertEquals("Failed to create IID Original message. Please try to save the changes and send the IID Original message again.", exception.Message.Trim());
			AssertEquals(@"Message: Object reference not set to an instance of an object.
TopLevelBusinessObject is null: True
UniversalDataContextAttribute: 
TopLevelBusinessObject is Declaration: True
Declaration Application Code: 
Declaration Country/Region Code: 
IShipmentDataContextManager is null: True
ITopLevelDataObjectWriter is null: True", exception.InnerException.Message.Trim());
		}

		public void TestBuildUniversalShipmentVersion402()
		{
			var actionPurpose = IIDMessageSubTypeList.Codes.Original;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001111";
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "importer";
			var contact = importer.Contacts.AddNew();
			contact.OC_ContactName = "contactname";
			var alloc = contact.Allocations.AddNew();
			alloc.PC_Type = OrgConstants.ContactAllocationType.CAPGA;
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			SendsMessagesToCustomsShutterUpperer mergeResult = new SendsMessagesToCustomsShutterUpperer(false);
			mergeResult.AnswerToContinueWithAction = true;
			declaration.DoMerge(mergeResult);
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			var dataWrapper = new IIDMessageWrapper(entry);
			var builder = new IIDMessageBuilder(actionPurpose, dataWrapper);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;

			AssertContains("DataContext", "<EventReference>MST=IID|MSB=ORG|RFN=4.02</EventReference>", message.EM_MessageText);
			AssertContains("Contact", "<Contact>contactname</Contact>", message.EM_MessageText);

			var dataWrapper2 = new IIDMessageWrapper(entry);
			var builder2 = new IIDMessageBuilder(actionPurpose, dataWrapper);
			var message2 = builder.PopulateMessages().GetBuilderResults().First().Message;
			AssertContains("DataContext", "<EventReference>MST=IID|MSB=ORG|RFN=4.02</EventReference>", message2.EM_MessageText);
		}

		public void TestLatestAcceptedMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			declaration.JE_DeclarationReference = "B00001111";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_BrandName = "BRN";
			invoiceLine.JI_Description = "ABC";

			Factory.Save();

			var dataWrapper = new IIDMessageWrapper(entryHeader);
			var builder = new IIDMessageBuilder(IIDMessageSubTypeList.Codes.Original, dataWrapper);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			message.EM_Status = MessageStatusList.Codes.Sent;

			var builder2 = new IIDMessageBuilder(IIDMessageSubTypeList.Codes.Cancellation, dataWrapper);
			var message2 = builder2.PopulateMessages().GetBuilderResults().First().Message;
			message2.EM_Status = MessageStatusList.Codes.Sent;

			var messageMAA = Factory.New<UniversalEventMessage>();
			messageMAA.EM_MessageSubType = UniversalEventMessageTypes.Codes.IIDResponses;
			messageMAA.EM_MessageText = mmaMessageText;
			messageMAA.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertEquals("EventyType", AutoEvents.MessageAcceptedCode, messageMAA.EventType);

			var stmAlog = Factory.New<StmALog>();
			using (stmAlog.LockForUpdatingKeyFieldsForTesting())
			{
				stmAlog.SL_Parent = declaration.PK;
				stmAlog.SL_Table = JobDeclaration.Schema.TableName;
			}
			var genPivot = Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = messageMAA.PK;
			genPivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;

			Factory.Save();
			AssertEquals("Get the last accepted message", true, dataWrapper.LatestSentAcceptedMessage.EM_MessageText.Contains("<Description>ABC</Description>"));

			invoiceLine.JI_Description = "DEF";
			Factory.Save();

			var builder3 = new IIDMessageBuilder(IIDMessageSubTypeList.Codes.Cancellation, dataWrapper);
			var message3 = builder3.PopulateMessages().GetBuilderResults().First().Message;
			message3.EM_Status = MessageStatusList.Codes.Sent;
			AssertEquals("Get the last accepted message", true, dataWrapper.LatestSentAcceptedMessage.EM_MessageText.Contains("<Description>ABC</Description>"));

			AssertContains("DataContext has MSB=CNL under <EventReference>", "<EventReference>MST=IID|MSB=CNL|RFN=4.02</EventReference>", message3.EM_MessageText);
		}

		readonly string mmaMessageText = @"<s0:UniversalEvent><s0:Event><s0:DataContext><s0:DataTargetCollection><s0:DataTarget><s0:Type>CAIntegratedImportDeclaration</s0:Type><s0:Key>10207000019090</s0:Key></s0:DataTarget></s0:DataTargetCollection><s0:RecipientRoleCollection><s0:RecipientRole><s0:Code>CD4</s0:Code><s0:Description>CA Customs IID/D4 Status Notice</s0:Description></s0:RecipientRole></s0:RecipientRoleCollection></s0:DataContext><s0:EventTime>2018-03-19T11:00:00</s0:EventTime><s0:EventType>MAA</s0:EventType><s0:EventReference>IID Accepted</s0:EventReference><s0:ContextCollection><s0:Context><s0:Type>InterchangeNumber</s0:Type><s0:Value>10484</s0:Value></s0:Context><s0:Context><s0:Type>MessageNumber</s0:Type><s0:Value>1</s0:Value></s0:Context><s0:Context><s0:Type>IsTest</s0:Type><s0:Value>Y</s0:Value></s0:Context><s0:Context><s0:Type>OrganizationReference</s0:Type><s0:Value>B00172152</s0:Value></s0:Context></s0:ContextCollection></s0:Event></s0:UniversalEvent>";
	}
}
