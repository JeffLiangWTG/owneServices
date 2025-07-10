using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS.CDSResponse.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using AsycudaManifestHeader = Enterprise.Customs.GB.H7.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.GB.CDS.Testing
{
	sealed class CDSH7ResponseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var postmastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			var currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "yawn@soPointless.com";
			currentUserInCurrentFactory.GS_IsSystemAccount = false;
			postmastersGroup.Staff.Add(currentUserInCurrentFactory);

			var systemAccountUser = Factory.New<GlbStaff>();
			systemAccountUser.GS_Code = "AAA";
			systemAccountUser.GS_LoginName = "AAA";
			systemAccountUser.GS_IsSystemAccount = true;
			systemAccountUser.GS_EmailAddress = "AAA@evenmorepointless.com";

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.LocalReferenceNumber = "8GB123456789000-S0001000";
			bill.MovementReferenceNumber = "123";
			Factory.Save();

			var newMessage = Factory.New<CDSNewDeclarationEDIMessage>();
			newMessage.EM_SystemCreateUser = currentUserInCurrentFactory.GS_Code;
			newMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			newMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			newMessage.EM_Status = EDIMessage.Status.Acknowledged;
			newMessage.EM_MessageText = @"<MetaData><Message>Pretend that we actually sent something</Message></MetaData>";
			newMessage.EM_ApplicationReference = "CONVERSATIONID";
			bill.Messages.Add(newMessage);

			var extraMessageSentBySystemAccount = Factory.New<CDSNewDeclarationEDIMessage>();
			extraMessageSentBySystemAccount.EM_SystemCreateUser = systemAccountUser.GS_Code;
			extraMessageSentBySystemAccount.EM_MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			extraMessageSentBySystemAccount.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			extraMessageSentBySystemAccount.EM_Status = EDIMessage.Status.Acknowledged;
			extraMessageSentBySystemAccount.EM_MessageText = @"<MetaData><Message>Message sent from systems account to prove they are ignored when sending email to notification group</Message></MetaData>";
			extraMessageSentBySystemAccount.EM_ApplicationReference = "CONVERSATIONID";
			bill.Messages.Add(extraMessageSentBySystemAccount);

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_ApplicationReference = "CONVERSATIONID";
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>02</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>15GB000060100C85A5</ID>
  </Declaration>
</Response>";
			bill.Messages.Add(ediMessage);

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);
			CombineAssertions(() =>
			{
				AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
				AssertEquals("Message.EM_MessageInterpretation", "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Message has been registered</H3><p><strong>Function Code: </strong>02-RCV<br><strong>Old CHIEF Report Code: </strong>H2/P2<br><strong>MRN: </strong>15GB000060100C85A5<br><strong>LRN: </strong>8GB123456789000-S0001000<br><strong>Issued Date: </strong>2018-07-27 12:12</p>", ediMessage.EM_MessageInterpretation);

				var log = bill.Logs.GetAllLogs().OfType<StmALog>().FirstOrDefault(log => log.SL_EventTimeUtc == new ZDateTime(2018, 7, 27, 12, 12, 12));
				AssertNotNull("Log with specific SE_Code not found", log);
				AssertEquals("Log.SL_Reference", Constants.ThreeCharFunctionCodes.MessageRegistered, log.SL_Reference);

				AssertEquals(Constants.ThreeCharFunctionCodes.MessageRegistered, bill.ABL_BillStatus);
				AssertEquals(Constants.ThreeCharFunctionCodes.MessageRegistered, ediMessage.EM_MessageSubType);

				var mrn = CusEntryNumber.Load(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
				AssertNotNull("MovementReferenceNumber", mrn);
				AssertEquals("MovementReferenceNumber.CE_EntryNum", "15GB000060100C85A5", mrn.CE_EntryNum);
				AssertEquals("MovementReferenceNumber.CE_IssueDate", new ZDateTime(2018, 7, 27, 12, 12, 12), mrn.CE_IssueDate);

				AssertEquals("Message should be attached to bill", bill, ediMessage.EM_LinkedObject);
			});
		}

		public void TestRejectMessageOnNEWWithErrors()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.LocalReferenceNumber = "ABCDEF0000000001";
			bill.ABL_BillStatus = EDIMessageStatusList.Codes.Received;
			bill.ABL_MessageStatus = MessageStatusList.Codes.AcknowledgedOriginal;
			Factory.Save();

			var newMessage = Factory.New<CDSNewDeclarationEDIMessage>();
			newMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			newMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			newMessage.EM_Status = EDIMessage.Status.Acknowledged;
			newMessage.EM_MessageText = @"<MetaData><Message>Pretend that we actually sent something</Message></MetaData>";
			newMessage.EM_ApplicationReference = "NEW-Message";
			bill.Messages.Add(newMessage);

			var canMessage = Factory.New<CDSCancelDeclarationEDIMessage>();
			canMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.CancelDeclaration;
			canMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			canMessage.EM_Status = EDIMessage.Status.Acknowledged;
			canMessage.EM_MessageText = @"<MetaData><Message>Pretend that we actually sent something</Message></MetaData>";
			canMessage.EM_ApplicationReference = "CAN-Message";
			bill.Messages.Add(canMessage);

			var processor = new CDSResponseMessageProcessor(Logger);

			var reqMessage = Factory.New<CDSResponseEDIMessage>();
			reqMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsDeclarationServices;
			reqMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.Response;
			reqMessage.EM_MessageSubType = ThreeCharFunctionCode.Codes.REQ;
			reqMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			reqMessage.EM_ApplicationReference = "CAN-Message";
			reqMessage.EM_MessageText = @"<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">    <FunctionCode>11</FunctionCode>    <FunctionalReferenceID>REQMessage001</FunctionalReferenceID>    <IssueDateTime>      <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>    </IssueDateTime>    <Status>      <NameCode>39</NameCode>    </Status>    <Declaration>      <FunctionalReferenceID>ABCDEF0000000001</FunctionalReferenceID>      <ID>MRN1234567890</ID>      <VersionID>1</VersionID>    </Declaration>  </Response>";

			Factory.Save();
			processor.ProcessMessage(reqMessage);
			Factory.Save();

			bill.Reload();
			newMessage.Reload();
			canMessage.Reload();

			CombineAssertions("After REQ Msg", () =>
			{
				AssertEquals("Bill Status", EDIMessageStatusList.Codes.Received, bill.ABL_BillStatus);
				AssertEquals("Message Status", MessageStatusList.Codes.AcknowledgedDelete, bill.ABL_MessageStatus);
				AssertEquals("Cancel Message", EDIMessage.Status.Acknowledged, canMessage.EM_Status);
				AssertEquals("REQ Message", EDIMessage.Status.ProcessedOK, reqMessage.EM_Status);
			});

			var rejMessage = ProcessTestMessage(ThreeCharFunctionCode.Codes.REJ,
				@"<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">    <FunctionCode>03</FunctionCode>    <FunctionalReferenceID>REJMessage001</FunctionalReferenceID>    <IssueDateTime>      <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>    </IssueDateTime>  <Error>    <Description>How do you expect us to process your dodgy messages?</Description>    <ValidationCode>DODGY101</ValidationCode>  </Error>  <Declaration>      <FunctionalReferenceID>ABCDEF0000000001</FunctionalReferenceID>      <ID>MRN1234567890</ID>      <RejectionDateTime>        <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>      </RejectionDateTime>      <VersionID>1</VersionID>    </Declaration>  </Response>",
				"NEW-Message");

			Factory.Save();
			processor.ProcessMessage(rejMessage);
			Factory.Save();

			bill.Reload();
			newMessage.Reload();
			canMessage.Reload();

			CombineAssertions("After REJ msg", () =>
			{
				AssertEquals("Bill Status", EDIMessageStatusList.Codes.Cancelled, bill.ABL_BillStatus);
				AssertEquals("Message Status", MessageStatusList.Codes.AcknowledgedDelete, bill.ABL_MessageStatus);
				AssertEquals("New Message", EDIMessage.Status.Acknowledged, newMessage.EM_Status);
				AssertEquals("Cancel Message", EDIMessage.Status.Acknowledged, canMessage.EM_Status);
				AssertEquals("REJ Message", EDIMessage.Status.ProcessedOK, rejMessage.EM_Status);
			});
		}

		public void TestRejectMessageOnNEW_WithoutCAN()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.LocalReferenceNumber = "ABCDEF0000000001";
			bill.ABL_BillStatus = EDIMessageStatusList.Codes.Received;
			bill.ABL_MessageStatus = MessageStatusList.Codes.AcknowledgedOriginal;
			Factory.Save();

			var newMessage = Factory.New<CDSNewDeclarationEDIMessage>();
			newMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			newMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			newMessage.EM_Status = EDIMessage.Status.Acknowledged;
			newMessage.EM_MessageText = @"<MetaData><Message>Pretend that we actually sent something</Message></MetaData>";
			newMessage.EM_ApplicationReference = "NEW-Message";
			bill.Messages.Add(newMessage);

			var processor = new CDSResponseMessageProcessor(Logger);

			var rejMessage = ProcessTestMessage(ThreeCharFunctionCode.Codes.REJ,
				@"<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">    <FunctionCode>03</FunctionCode>    <FunctionalReferenceID>REJMessage001</FunctionalReferenceID>    <IssueDateTime>      <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>    </IssueDateTime>    <Declaration>      <FunctionalReferenceID>ABCDEF0000000001</FunctionalReferenceID>      <ID>MRN1234567890</ID>      <RejectionDateTime>        <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>      </RejectionDateTime>      <VersionID>1</VersionID>    </Declaration>  </Response>",
				"NEW-Message");

			bill.Reload();
			newMessage.Reload();

			CombineAssertions("After REJ msg", () =>
			{
				AssertEquals("Bill Status", EDIMessageStatusList.Codes.Cancelled, bill.ABL_BillStatus);
				AssertEquals("Message Status", MessageStatusList.Codes.AcknowledgedOriginal, bill.ABL_MessageStatus);
				AssertEquals("New Message", EDIMessage.Status.Acknowledged, newMessage.EM_Status);
				AssertEquals("REJ Message", EDIMessage.Status.ProcessedOK, rejMessage.EM_Status);
			});
		}

		public void TestRejectMessageOnCAN()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.LocalReferenceNumber = "ABCDEF0000000001";
			bill.ABL_BillStatus = EDIMessageStatusList.Codes.Received;
			bill.ABL_MessageStatus = MessageStatusList.Codes.AcknowledgedOriginal;
			Factory.Save();

			var newMessage = Factory.New<CDSNewDeclarationEDIMessage>();
			newMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			newMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			newMessage.EM_Status = EDIMessage.Status.Acknowledged;
			newMessage.EM_MessageText = @"<MetaData><Message>Pretend that we actually sent something</Message></MetaData>";
			newMessage.EM_ApplicationReference = "NEW-Message";
			bill.Messages.Add(newMessage);

			var canMessage = Factory.New<CDSCancelDeclarationEDIMessage>();
			canMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.CancelDeclaration;
			canMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			canMessage.EM_Status = EDIMessage.Status.Acknowledged;
			canMessage.EM_MessageText = @"<MetaData><Message>Pretend that we actually sent something</Message></MetaData>";
			canMessage.EM_ApplicationReference = "CAN-Message";
			bill.Messages.Add(canMessage);

			var processor = new CDSResponseMessageProcessor(Logger);

			var rejMessage = ProcessTestMessage(ThreeCharFunctionCode.Codes.REJ,
				@"<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">    <FunctionCode>03</FunctionCode>    <FunctionalReferenceID>REJMessage001</FunctionalReferenceID>    <IssueDateTime>      <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>    </IssueDateTime>    <Declaration>      <FunctionalReferenceID>ABCDEF0000000001</FunctionalReferenceID>      <ID>MRN1234567890</ID>      <RejectionDateTime>        <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>      </RejectionDateTime>      <VersionID>1</VersionID>    </Declaration>  </Response>",
				"CAN-Message");

			bill.Reload();
			newMessage.Reload();
			canMessage.Reload();

			CombineAssertions("After REJ msg", () =>
			{
				AssertEquals("Bill Status", EDIMessageStatusList.Codes.Received, bill.ABL_BillStatus);
				AssertEquals("Message Status", MessageStatusList.Codes.ErrorDelete, bill.ABL_MessageStatus);
				AssertEquals("New Message", EDIMessage.Status.Acknowledged, newMessage.EM_Status);
				AssertEquals("Cancel Message", EDIMessage.Status.Rejected, canMessage.EM_Status);
				AssertEquals("REJ Message", EDIMessage.Status.ProcessedOK, rejMessage.EM_Status);
			});
		}

		public void TestAdditionalMessageProcessedResponse_WithStatus39()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.LocalReferenceNumber = "ABCDEF0000000001";
			bill.ABL_BillStatus = EDIMessageStatusList.Codes.Received;
			bill.ABL_MessageStatus = MessageStatusList.Codes.AcknowledgedOriginal;
			Factory.Save();

			var newMessage = Factory.New<CDSNewDeclarationEDIMessage>();
			newMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			newMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			newMessage.EM_Status = EDIMessage.Status.Acknowledged;
			newMessage.EM_MessageText = @"<MetaData><Message>Pretend that we actually sent something</Message></MetaData>";
			newMessage.EM_ApplicationReference = "ConversationID";
			bill.Messages.Add(newMessage);

			var amendMessage = Factory.New<CDSNewDeclarationEDIMessage>();
			amendMessage.EM_MessageType = CDSEDIMessageTypeList.Codes.AmendDeclaration;
			amendMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			amendMessage.EM_Status = EDIMessage.Status.Acknowledged;
			amendMessage.EM_MessageText = @"<MetaData><Message>Pretend that we actually sent something</Message></MetaData>";
			amendMessage.EM_ApplicationReference = "ConversationID";
			bill.Messages.Add(amendMessage);

			string testResponse = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>11</FunctionCode>
  <FunctionalReferenceID>REQMessage001</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20200601130014Z</DateTimeString>
  </IssueDateTime>
  <AdditionalInformation>
    <StatementTypeCode>AFB</StatementTypeCode>
  </AdditionalInformation>
  <Status>
    <NameCode>39</NameCode>
  </Status>
  <Declaration>
    <FunctionalReferenceID>ABCDEF0000000001</FunctionalReferenceID>
    <ID>MRN1234567890</ID>
    <VersionID>1</VersionID>
  </Declaration>
</Response>";

			var reqMessage = ProcessTestMessage(ThreeCharFunctionCode.Codes.REQ, testResponse);

			bill.Reload();
			newMessage.Reload();
			amendMessage.Reload();
			reqMessage.Reload();

			CombineAssertions("After this REQ Msg the status should not be changed to CAN", () =>
			{
				AssertEquals("Bill Status", EDIMessageStatusList.Codes.Received, bill.ABL_BillStatus);
				AssertEquals("REQ Message", EDIMessage.Status.ProcessedOK, reqMessage.EM_Status);
			});
		}

		public void TestProcessWithDaylightSavingTime()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.LocalReferenceNumber = "8GB123456789000-S0001000";
			bill.MovementReferenceNumber = "123";
			Factory.Save();

			var ediMessage = Factory.New<CDSResponseEDIMessage>();

			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>02</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212+01</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>15GB000060100C85A5</ID>
  </Declaration>
</Response>";
			bill.Messages.Add(ediMessage);

			var processor = new CDSResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);
			CombineAssertions(() =>
			{
				AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
				AssertEquals("Message.EM_MessageInterpretation", "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Message has been registered</H3><p><strong>Function Code: </strong>02-RCV<br><strong>Old CHIEF Report Code: </strong>H2/P2<br><strong>MRN: </strong>15GB000060100C85A5<br><strong>LRN: </strong>8GB123456789000-S0001000<br><strong>Issued Date: </strong>2018-07-27 12:12</p>", ediMessage.EM_MessageInterpretation);

				var log = bill.Logs.GetAllLogs().OfType<StmALog>().FirstOrDefault(log => log.SL_EventTimeUtc == new ZDateTime(2018, 7, 27, 12, 12, 12));
				AssertNotNull("Log with specific SE_Code not found", log);
				AssertEquals("Log.SL_Reference", Constants.ThreeCharFunctionCodes.MessageRegistered, log.SL_Reference);
				AssertEquals(Constants.ThreeCharFunctionCodes.MessageRegistered, bill.ABL_BillStatus);
				AssertEquals(Constants.ThreeCharFunctionCodes.MessageRegistered, ediMessage.EM_MessageSubType);

				var mrn = CusEntryNumber.Load(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
				AssertNotNull("MovementReferenceNumber", mrn);
				AssertEquals("MovementReferenceNumber.CE_EntryNum", "15GB000060100C85A5", mrn.CE_EntryNum);
				AssertEquals("MovementReferenceNumber.CE_IssueDate", new ZDateTime(2018, 7, 27, 12, 12, 12), mrn.CE_IssueDate);

				AssertEquals(bill, ediMessage.EM_LinkedObject);
			});
		}

		public void TestUpdateMovementReferenceNumber()
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.LocalReferenceNumber = "8GB123456789000-S0001000";
			bill.MovementReferenceNumber = "123";
			Factory.Save();

			var ediMessage = Factory.New<CDSResponseEDIMessage>();
			ediMessage.EM_MessageNum = "00000001";
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>01</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>456</ID>
  </Declaration>
</Response>";

			var logger = new LoggingInformation();
			var processor = new CDSResponseMessageProcessor(logger);
			processor.ProcessMessage(ediMessage);
			CombineAssertions(() =>
			{
				var mrn = CusEntryNumber.Load(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
				AssertNotNull("MovementReferenceNumber", mrn);
				AssertEquals("MovementReferenceNumber.CE_EntryNum", "456", mrn.CE_EntryNum);
				AssertEquals("MovementReferenceNumber.CE_IssueDate", new ZDateTime(2018, 7, 27, 12, 12, 12), mrn.CE_IssueDate);
				Assert(logger.UserLogStrings.Contains("\tWe're about to change the MRN from 123 to 456 while processing message number 00000001."));
			});

			logger.ClearLogs();
			var ediMessage2 = Factory.New<CDSResponseEDIMessage>();
			ediMessage2.EM_MessageNum = "00000002";
			ediMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage2.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>03</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180728121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>789</ID>
  </Declaration>
</Response>";
			bill.Messages.Add(ediMessage2);

			processor.ProcessMessage(ediMessage2);
			CombineAssertions(() =>
			{
				var mrn = CusEntryNumber.Load(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
				AssertNotNull("MovementReferenceNumber", mrn);
				AssertEquals("MovementReferenceNumber.CE_EntryNum", "789", mrn.CE_EntryNum);
				AssertEquals("MovementReferenceNumber.CE_IssueDate", new ZDateTime(2018, 7, 28, 12, 12, 12), mrn.CE_IssueDate);
				Assert(logger.UserLogStrings.Contains("\tWe're about to change the MRN from 456 to 789 while processing message number 00000002."));
			});
		}

		CDSResponseEDIMessage ProcessTestMessage(string messageSubType, string messageText, string applicationReference = "ConversationID")
		{
			var message = Factory.New<CDSResponseEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsDeclarationServices;
			message.EM_MessageType = CDSEDIMessageTypeList.Codes.Response;
			message.EM_MessageSubType = messageSubType;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_ApplicationReference = applicationReference;
			message.EM_MessageText = messageText;

			var processor = new CDSResponseMessageProcessor(Logger);

			Factory.Save();
			processor.ProcessMessage(message);
			Factory.Save();

			return message;
		}

		LoggingInformation Logger => logger ?? (logger = new LoggingInformation());
		LoggingInformation logger;
	}
}
