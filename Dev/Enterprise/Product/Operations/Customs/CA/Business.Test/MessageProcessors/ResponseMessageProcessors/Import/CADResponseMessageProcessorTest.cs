using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Customs.Business.MessageProcessors.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class CADResponseMessageProcessorTest : EDIFACTMessageProcessorTest
	{
		public void TestUpdateDatesForIMP()
		{
			AssertUpdateDates(JobMessageTypeList.Codes.Import);
		}

		public void TestUpdateDatesForLVS()
		{
			AssertUpdateDates(JobMessageTypeList.Codes.LowValueShipments);
		}

		void AssertUpdateDates(ZString messageType)
		{
			var newFactory = new BusinessObjectFactory();
			var receiveMessage = newFactory.New<CADMessage>();
			receiveMessage.EM_MessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.MessageProcessors.ResponseMessageProcessors.Import.TestFiles.CADTOTDutyTaxFee.xml");

			var processor = new CADResponseMessageProcessor(logger);
			newFactory.Save();

			var declaration = newFactory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.TransactionNumber.AccountSecurityCode = "33333";
			declaration.TransactionNumber.SequentialNumber = "33333333";
			declaration.IsCancelled = false;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceDisplaySequence = 1;
			var invoiceLine1 = invoice.InvoiceLines.AddNew();

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entry.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entry.CH_BGMReference = declaration.TransactionNumber.ToString();
			var entryLine1 = entry.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			invoiceLine1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);

			newFactory.Save();
			AssertEquals(ZDateTime.Empty, declaration.CA_K84AccountingDate);
			AssertEquals(ZDateTime.Empty, declaration.CA_K84StatementDate);
			AssertNoExceptionThrown(() =>
			{
				processor.ProcessMessage(receiveMessage);
			});
			AssertEquals(new ZDateTime(2023, 03, 06, 01, 56, 09), declaration.CA_K84AccountingDate);
			AssertEquals(new ZDateTime(2023, 03, 06, 01, 56, 09), declaration.CA_K84StatementDate);
			AssertEquals(new ZDateTime(2023, 03, 07, 00, 00, 00), declaration.JE_ValuationDate);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoExceptionThrownWhenResponseStatusIs40()
		{
			var errorMessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetaData xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
    <CommunicationMetaData>
        <ApplicationReferenceID>1020700300058500001001</ApplicationReferenceID>
        <Recipient>
          <ID>822066668RM0002</ID>
        </Recipient>
    </CommunicationMetaData>
    <Response> 
        <IssueDateTime>
          <DateTimeString>20241119234732</DateTimeString> 
        </IssueDateTime>
        <Status>
          <NameCode>40</NameCode>
        </Status>
    </Response>
</DocumentMetaData>";

			var declaration = CreatedDeclaration("C0ECDC26-682C-44B4-BC59-FE88C6F14A01", "B00001000", "10207003000585");
			Factory.Save();

			var cadMessage = Factory.New<CADMessage>();
			cadMessage.EM_MessageText = errorMessageText;
			cadMessage.EM_ReceiveTransmit = "RCV";
			var entry = declaration.B3EntryHeader;
			AssertNoExceptionThrown(() =>
			{
				processor.ProcessMessage(cadMessage);
				AssertEquals(1, entry.Messages.Count);
				AssertEquals(CusEntryHeader.Schema.TableName, cadMessage.EM_LinkTable);
				AssertEquals(entry.PK, cadMessage.EM_LinkUniqueID);
				AssertEquals(EDIMessage.Status.Received, cadMessage.EM_Status);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, cadMessage.EM_MessageSubType);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, entry.CH_Status);
				AssertEquals(CADEntryStatusList.Codes.Unknown, entry.CH_EntryStatus);
				AssertEquals(0, logger.Logs.Count());
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoExceptionThrownWhenResponseStatusIs502()
		{
			var errorMessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetaData xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
    <CommunicationMetaData>
        <ApplicationReferenceID>1020700300058500001001</ApplicationReferenceID>
        <Recipient>
          <ID>822066668RM0002</ID>
        </Recipient>
    </CommunicationMetaData>
    <Response> 
        <IssueDateTime>
          <DateTimeString>20241119234732</DateTimeString> 
        </IssueDateTime>
        <Error>
          <Description>connection error</Description>
        </Error>
        <Status>
          <NameCode>502</NameCode>
        </Status>
    </Response>
</DocumentMetaData>";

			var declaration = CreatedDeclaration("C0ECDC26-682C-44B4-BC59-FE88C6F14A01", "B00001000", "10207003000585");
			Factory.Save();

			var cadMessage = Factory.New<CADMessage>();
			cadMessage.EM_MessageText = errorMessageText;
			cadMessage.EM_ReceiveTransmit = "RCV";
			var entry = declaration.B3EntryHeader;
			AssertNoExceptionThrown(() =>
			{
				processor.ProcessMessage(cadMessage);
				AssertEquals(1, entry.Messages.Count);
				AssertEquals(CusEntryHeader.Schema.TableName, cadMessage.EM_LinkTable);
				AssertEquals(entry.PK, cadMessage.EM_LinkUniqueID);
				AssertEquals(EDIMessage.Status.Received, cadMessage.EM_Status);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, cadMessage.EM_MessageSubType);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, entry.CH_Status);
				AssertEquals(CADEntryErrorResponseStatusList.Codes.ConnectionError, entry.CH_EntryStatus);
				AssertEquals(0, logger.Logs.Count());
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessMessageThatResponseStatusIs503()
		{
			var errorMessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetaData xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
    <CommunicationMetaData>
        <ApplicationReferenceID>3333333333333700002001</ApplicationReferenceID>
        <Recipient>
          <ID>822066668RM0002</ID>
        </Recipient>
    </CommunicationMetaData>
    <Response> 
        <IssueDateTime>
          <DateTimeString>20200209125901</DateTimeString> 
        </IssueDateTime>
        <Error>
          <Description>cvc--particle 2.1: in element{urn:wco:datamodel:wco:declaration:1}goodsshipment with anonymous type, found &lt;exitdatetime> (in namespace urn:wco:datamodel:wco:declaration:1), but next itemshould be {urn:wco:datamodel:wco:declaration:1}invoice</Description>
        </Error>
        <Status>
          <NameCode>503</NameCode>
        </Status>
    </Response>
</DocumentMetaData>";

			var expectedBody = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\CADResponseInterpretation\ReminderFor503.html");
			expectedBody = expectedBody.Replace("{VersionNumber}", $"VersionNumber={new EnterpriseInformationRetriever().VersionNumber}");

			var declaration = CreatedDeclaration("C0ECDC26-682C-44B4-BC59-FE88C6F14A01", "B00001000", "33333333333337");
			Factory.Save();

			var cadMessage = Factory.New<CADMessage>();
			cadMessage.EM_MessageText = errorMessageText;
			cadMessage.EM_ReceiveTransmit = "RCV";
			var entry = declaration.B3EntryHeader;
			AssertNoExceptionThrown(() =>
			{
				processor.ProcessMessage(cadMessage);
				AssertEquals(1, entry.Messages.Count);
				AssertEquals(CusEntryHeader.Schema.TableName, cadMessage.EM_LinkTable);
				AssertEquals(entry.PK, cadMessage.EM_LinkUniqueID);
				AssertEquals(EDIMessage.Status.Received, cadMessage.EM_Status);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, cadMessage.EM_MessageSubType);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, entry.CH_Status);
				AssertEquals(CADEntryErrorResponseStatusList.Codes.GoodsShipmentWithAnonymousType, entry.CH_EntryStatus);
				AssertEquals(0, logger.Logs.Count());
				AssertMultilineASCIIEquals(expectedBody, cadMessage.EM_MessageInterpretation);
				AssertEmail("Commercial Accounting Declaration response has been received for 33333333333337", expectedBody, cadMessage.EM_MessageText, ZString.Empty);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoExceptionThrownWhenResponseStatusIs504()
		{
			var errorMessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetaData xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
    <CommunicationMetaData>
        <ApplicationReferenceID>1020700300058500001001</ApplicationReferenceID>
        <Recipient>
          <ID>822066668RM0002</ID>
        </Recipient>
    </CommunicationMetaData>
    <Response> 
        <IssueDateTime>
          <DateTimeString>20241119234732</DateTimeString> 
        </IssueDateTime>
        <Error>
          <Description>failed to establish a backside connection</Description>
        </Error>
        <Status>
          <NameCode>504</NameCode>
        </Status>
    </Response>
</DocumentMetaData>";

			var declaration = CreatedDeclaration("C0ECDC26-682C-44B4-BC59-FE88C6F14A01", "B00001000", "10207003000585");
			Factory.Save();

			var cadMessage = Factory.New<CADMessage>();
			cadMessage.EM_MessageText = errorMessageText;
			cadMessage.EM_ReceiveTransmit = "RCV";
			var entry = declaration.B3EntryHeader;
			AssertNoExceptionThrown(() =>
			{
				processor.ProcessMessage(cadMessage);
				AssertEquals(1, entry.Messages.Count);
				AssertEquals(CusEntryHeader.Schema.TableName, cadMessage.EM_LinkTable);
				AssertEquals(entry.PK, cadMessage.EM_LinkUniqueID);
				AssertEquals(EDIMessage.Status.Received, cadMessage.EM_Status);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, cadMessage.EM_MessageSubType);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, entry.CH_Status);
				AssertEquals(CADEntryErrorResponseStatusList.Codes.FailedToEstablishABacksideConnection, entry.CH_EntryStatus);
				AssertEquals(0, logger.Logs.Count());
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoExceptionThrownWhenResponseStatusIs505()
		{
			var errorMessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetaData xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
    <CommunicationMetaData>
        <ApplicationReferenceID>1020700300058500001001</ApplicationReferenceID>
        <Recipient>
          <ID>822066668RM0002</ID>
        </Recipient>
    </CommunicationMetaData>
    <Response> 
        <IssueDateTime>
          <DateTimeString>20241119234732</DateTimeString> 
        </IssueDateTime>
        <Error>
          <Description>internal error</Description>
        </Error>
        <Status>
          <NameCode>505</NameCode>
        </Status>
    </Response>
</DocumentMetaData>";

			var declaration = CreatedDeclaration("C0ECDC26-682C-44B4-BC59-FE88C6F14A01", "B00001000", "10207003000585");
			Factory.Save();

			var cadMessage = Factory.New<CADMessage>();
			cadMessage.EM_MessageText = errorMessageText;
			cadMessage.EM_ReceiveTransmit = "RCV";
			var entry = declaration.B3EntryHeader;
			AssertNoExceptionThrown(() =>
			{
				processor.ProcessMessage(cadMessage);
				AssertEquals(1, entry.Messages.Count);
				AssertEquals(CusEntryHeader.Schema.TableName, cadMessage.EM_LinkTable);
				AssertEquals(entry.PK, cadMessage.EM_LinkUniqueID);
				AssertEquals(EDIMessage.Status.Received, cadMessage.EM_Status);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, cadMessage.EM_MessageSubType);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, entry.CH_Status);
				AssertEquals(CADEntryErrorResponseStatusList.Codes.InternalError, entry.CH_EntryStatus);
				AssertEquals(0, logger.Logs.Count());
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessMessage_SenderInfoInconsistent()
		{
			var errorMessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetaData xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
    <CommunicationMetaData>
        <ApplicationReferenceID>3333333333333700002001</ApplicationReferenceID>
        <Recipient>
            <ID>822066668RM0002</ID>
        </Recipient>
    </CommunicationMetaData>
    <Response> 
        <IssueDateTime>
          <DateTimeString>20200209125901</DateTimeString> 
        </IssueDateTime>
        <Error>
          <Description>Sender ID does not match what is provided in the declaration</Description>
          <ValidationCode>69</ValidationCode>
          <Pointer>
            <Location>DocumentMetaData/Declaration</Location>
          </Pointer>
        </Error>
        <Status>
          <NameCode>412</NameCode>
        </Status>
    </Response>
</DocumentMetaData>";

			var expectedBody = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\CADResponseInterpretation\SenderInfoInconsistent.html");
			expectedBody = expectedBody.Replace("{VersionNumber}", $"VersionNumber={new EnterpriseInformationRetriever().VersionNumber}");

			var declaration = CreatedDeclaration("C0ECDC26-682C-44B4-BC59-FE88C6F14A01", "B00001000", "33333333333337");
			Factory.Save();

			var cadMessage = Factory.New<CADMessage>();
			cadMessage.EM_MessageText = errorMessageText;
			cadMessage.EM_ReceiveTransmit = "RCV";
			var entry = declaration.B3EntryHeader;
			AssertNoExceptionThrown(() =>
			{
				ErrorReporter.Clear();
				processor.ProcessMessage(cadMessage);
				AssertEquals(1, entry.Messages.Count);
				AssertEquals(CusEntryHeader.Schema.TableName, cadMessage.EM_LinkTable);
				AssertEquals(entry.PK, cadMessage.EM_LinkUniqueID);
				AssertEquals(EDIMessage.Status.Received, cadMessage.EM_Status);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, cadMessage.EM_MessageSubType);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, entry.CH_Status);
				AssertEquals(CADEntryStatusList.Codes.SenderInfoInconsistent, entry.CH_EntryStatus);
				AssertEquals(0, logger.Logs.Count());
				AssertMultilineASCIIEquals(expectedBody, cadMessage.EM_MessageInterpretation);
				AssertEmail("Sender Info Inconsistent Commercial Accounting Declaration response has been received for 33333333333337", expectedBody, cadMessage.EM_MessageText, ZString.Empty);
				AssertEquals(ZString.Empty, ErrorReporter.LastKeyReported);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessMessage_ProfileDoesNotExist()
		{
			var errorMessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetaData xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
    <CommunicationMetaData>
        <ApplicationReferenceID>3333333333333700002001</ApplicationReferenceID>
        <Recipient>
            <ID>822066668RM0002</ID>
        </Recipient>
    </CommunicationMetaData>
    <Response> 
        <IssueDateTime>
          <DateTimeString>20200209125901</DateTimeString> 
        </IssueDateTime>
        <Error>
          <Description>TCP profile does not match</Description>
          <ValidationCode>90</ValidationCode>
          <Pointer>
            <Location>DocumentMetaData/CommunicationMetaData/Sender</Location>
          </Pointer>
         </Error>
        <Status>
          <NameCode>415</NameCode>
        </Status>
    </Response>
</DocumentMetaData>";

			var expectedBody = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\CADResponseInterpretation\ProfileDoesNotExist.html");
			expectedBody = expectedBody.Replace("{VersionNumber}", $"VersionNumber={new EnterpriseInformationRetriever().VersionNumber}");

			var declaration = CreatedDeclaration("C0ECDC26-682C-44B4-BC59-FE88C6F14A01", "B00001000", "33333333333337");
			Factory.Save();

			var cadMessage = Factory.New<CADMessage>();
			cadMessage.EM_MessageText = errorMessageText;
			cadMessage.EM_ReceiveTransmit = "RCV";
			var entry = declaration.B3EntryHeader;
			AssertNoExceptionThrown(() =>
			{
				ErrorReporter.Clear();
				processor.ProcessMessage(cadMessage);
				AssertEquals(1, entry.Messages.Count);
				AssertEquals(CusEntryHeader.Schema.TableName, cadMessage.EM_LinkTable);
				AssertEquals(entry.PK, cadMessage.EM_LinkUniqueID);
				AssertEquals(EDIMessage.Status.Received, cadMessage.EM_Status);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, cadMessage.EM_MessageSubType);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, entry.CH_Status);
				AssertEquals(CADEntryStatusList.Codes.ProfileDoesNotExist, entry.CH_EntryStatus);
				AssertEquals(0, logger.Logs.Count());
				AssertMultilineASCIIEquals(expectedBody, cadMessage.EM_MessageInterpretation);
				AssertEmail("Profile Does Not Exist Commercial Accounting Declaration response has been received for 33333333333337", expectedBody, cadMessage.EM_MessageText, ZString.Empty);
				AssertEquals(ZString.Empty, ErrorReporter.LastKeyReported);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessMessageThatResponseStatusIsUnknown_ErrorReportIsThrown()
		{
			var errorMessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetaData xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
    <CommunicationMetaData>
        <ApplicationReferenceID>3333333333333700002001</ApplicationReferenceID>
        <Recipient>
            <ID>822066668RM0002</ID>
        </Recipient>
    </CommunicationMetaData>
    <Response> 
        <IssueDateTime>
          <DateTimeString>20200209125901</DateTimeString> 
        </IssueDateTime>
        <Status>
          <NameCode>HXU</NameCode>
        </Status>
    </Response>
</DocumentMetaData>";

			var expectedBody = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\CADResponseInterpretation\ReminderForUnknownResponseStatus.html");
			expectedBody = expectedBody.Replace("{VersionNumber}", $"VersionNumber={new EnterpriseInformationRetriever().VersionNumber}").Replace("{StatusDescription}", "").Replace("{UnknowStatus}", "HXU");

			var declaration = CreatedDeclaration("C0ECDC26-682C-44B4-BC59-FE88C6F14A01", "B00001000", "33333333333337");
			Factory.Save();

			var cadMessage = Factory.New<CADMessage>();
			cadMessage.EM_MessageText = errorMessageText;
			cadMessage.EM_ReceiveTransmit = "RCV";
			var entry = declaration.B3EntryHeader;
			AssertNoExceptionThrown(() =>
			{
				ErrorReporter.Clear();
				processor.ProcessMessage(cadMessage);
				AssertEquals(1, entry.Messages.Count);
				AssertEquals(CusEntryHeader.Schema.TableName, cadMessage.EM_LinkTable);
				AssertEquals(entry.PK, cadMessage.EM_LinkUniqueID);
				AssertEquals(EDIMessage.Status.Received, cadMessage.EM_Status);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, cadMessage.EM_MessageSubType);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, entry.CH_Status);
				AssertEquals(CADEntryStatusList.Codes.Unknown, entry.CH_EntryStatus);
				AssertEquals(0, logger.Logs.Count());
				AssertMultilineASCIIEquals(expectedBody, cadMessage.EM_MessageInterpretation);
				AssertEmail("Commercial Accounting Declaration response has been received for 33333333333337", expectedBody, cadMessage.EM_MessageText, ZString.Empty);
				AssertEquals("UnknownCADMessage_UnknownResponseStatus", ErrorReporter.LastKeyReported);
				AssertEquals(cadMessage.EM_MessageText, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessMessageThatResponseStatusIsMissing_ErrorReportIsThrown()
		{
			var errorMessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetaData xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
    <CommunicationMetaData>
        <ApplicationReferenceID>3333333333333700002001</ApplicationReferenceID>
        <Recipient>
            <ID>822066668RM0002</ID>
        </Recipient>
    </CommunicationMetaData>
    <Response> 
        <IssueDateTime>
          <DateTimeString>20200209125901</DateTimeString> 
        </IssueDateTime>
    </Response>
</DocumentMetaData>";

			var expectedBody = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\CADResponseInterpretation\ReminderForUnknownResponseStatus.html");
			expectedBody = expectedBody.Replace("{VersionNumber}", $"VersionNumber={new EnterpriseInformationRetriever().VersionNumber}").Replace("{StatusDescription}", "Unknown Status ").Replace("{UnknowStatus}", "UNK");

			var declaration = CreatedDeclaration("C0ECDC26-682C-44B4-BC59-FE88C6F14A01", "B00001000", "33333333333337");
			Factory.Save();

			var cadMessage = Factory.New<CADMessage>();
			cadMessage.EM_MessageText = errorMessageText;
			cadMessage.EM_ReceiveTransmit = "RCV";
			var entry = declaration.B3EntryHeader;
			AssertNoExceptionThrown(() =>
			{
				ErrorReporter.Clear();
				processor.ProcessMessage(cadMessage);
				AssertEquals(1, entry.Messages.Count);
				AssertEquals(CusEntryHeader.Schema.TableName, cadMessage.EM_LinkTable);
				AssertEquals(entry.PK, cadMessage.EM_LinkUniqueID);
				AssertEquals(EDIMessage.Status.Received, cadMessage.EM_Status);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, cadMessage.EM_MessageSubType);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, entry.CH_Status);
				AssertEquals(CADEntryStatusList.Codes.Unknown, entry.CH_EntryStatus);
				AssertEquals(0, logger.Logs.Count());
				AssertMultilineASCIIEquals(expectedBody, cadMessage.EM_MessageInterpretation);
				AssertEmail("Unknown Status Commercial Accounting Declaration response has been received for 33333333333337", expectedBody, cadMessage.EM_MessageText, ZString.Empty);
				AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			});
		}

		public void TestProcessDutyTaxFee()
		{
			var newFactory = new BusinessObjectFactory();
			var receiveMessage = newFactory.New<CADMessage>();
			receiveMessage.EM_MessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.MessageProcessors.ResponseMessageProcessors.Import.TestFiles.CADDutyTaxFee.xml");

			var processor = new CADResponseMessageProcessor(logger);
			newFactory.Save();

			var declaration = newFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "33333";
			declaration.TransactionNumber.SequentialNumber = "33333333";
			declaration.IsCancelled = false;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceDisplaySequence = 1;
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceDisplaySequence = 2;
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			var invoiceLine4 = invoice2.InvoiceLines.AddNew();
			var invoiceLine5 = invoice2.InvoiceLines.AddNew();

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entry.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entry.CH_BGMReference = declaration.TransactionNumber.ToString();

			var entryLine1 = entry.AllEntryLines.AddNew();
			var entryLine2 = entry.AllEntryLines.AddNew();
			var entryLine3 = entry.AllEntryLines.AddNew();
			var entryLine4 = entry.AllEntryLines.AddNew();
			var entryLine5 = entry.AllEntryLines.AddNew();

			entryLine1.CL_LineNumber = 1;
			entryLine2.CL_LineNumber = 2;
			entryLine3.CL_LineNumber = 3;
			entryLine4.CL_LineNumber = 4;
			entryLine5.CL_LineNumber = 5;

			invoiceLine1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			invoiceLine2.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine2);
			invoiceLine3.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine3);
			invoiceLine4.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine4);
			invoiceLine5.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine5);

			newFactory.Save();

			AssertNoExceptionThrown(() =>
			{
				processor.ProcessMessage(receiveMessage);
			});

			AssertEquals(4, entry.ConfirmedCharges.Count);
			var headerCUD = entry.ConfirmedCharges.FirstOrDefault(f => f.C1_ChargeType == CADDutyTaxFeeTypeCodes.Codes.CUD);
			AssertEquals("CUD", 11684.12m, headerCUD.C1_ChargeAmount);
			var headerGST = entry.ConfirmedCharges.FirstOrDefault(f => f.C1_ChargeType == CADDutyTaxFeeTypeCodes.Codes.GST);
			AssertEquals("GST", 6255.55m, headerGST.C1_ChargeAmount);
			var headerTOT = entry.ConfirmedCharges.FirstOrDefault(f => f.C1_ChargeType == CADDutyTaxFeeTypeCodes.Codes.TOT);
			AssertEquals("TOT", 113427m, headerTOT.C1_ChargeAmount);
			var headerAAI = entry.ConfirmedCharges.FirstOrDefault(f => f.C1_ChargeType == CADDutyTaxFeeTypeCodes.Codes.AAI);
			AssertEquals("AAI", 0m, headerAAI.C1_ChargeAmount);

			AssertEquals(4, entryLine1.ConfirmedFees.Count);
			var charges = entryLine1.ConfirmedFees.Cast<CusEntryLineFee>();
			var cud = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.CUD);
			AssertCusEntryLineFee(cud, 123.45m, 5m, 24.69m, "TNE");
			var gst = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.GST);
			AssertCusEntryLineFee(gst, 163.71m, 3274.20m, 5m, "%");
			var tot = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.TOT);
			AssertCusEntryLineFee(tot, 287.16m, 0m, 0m, "");
			var vft = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.VFT);
			AssertCusEntryLineFee(vft, 0m, 0m, 0m, "");

			AssertEquals(4, entryLine2.ConfirmedFees.Count);
			charges = entryLine2.ConfirmedFees.Cast<CusEntryLineFee>();
			cud = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.CUD);
			AssertCusEntryLineFee(cud, 1984.97m, 18904.50m, 10.50m, "%");
			gst = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.GST);
			AssertCusEntryLineFee(gst, 1044.47m, 20889.47m, 5m, "%");
			tot = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.TOT);
			AssertCusEntryLineFee(tot, 3029.44m, 0m, 0m, "");
			vft = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.VFT);
			AssertCusEntryLineFee(vft, 0m, 0m, 0m, "");

			AssertEquals(4, entryLine3.ConfirmedFees.Count);
			charges = entryLine3.ConfirmedFees.Cast<CusEntryLineFee>();
			cud = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.CUD);
			AssertCusEntryLineFee(cud, 123.45m, 5m, 24.69m, "TNE");
			gst = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.GST);
			AssertCusEntryLineFee(gst, 163.71m, 3274.20m, 5m, "%");
			tot = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.TOT);
			AssertCusEntryLineFee(tot, 287.16m, 0m, 0m, "");
			vft = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.VFT);
			AssertCusEntryLineFee(vft, 0m, 0m, 0m, "");

			AssertEquals(4, entryLine4.ConfirmedFees.Count);
			charges = entryLine4.ConfirmedFees.Cast<CusEntryLineFee>();
			cud = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.CUD);
			AssertCusEntryLineFee(cud, 9452.25m, 75618m, 12.5m, "%");
			gst = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.GST);
			AssertCusEntryLineFee(gst, 4253.51m, 85070.25m, 5m, "%");
			tot = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.TOT);
			AssertCusEntryLineFee(tot, 13705.76m, 0m, 0m, "");
			vft = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.VFT);
			AssertCusEntryLineFee(vft, 0m, 0m, 0m, "");

			AssertEquals(4, entryLine5.ConfirmedFees.Count);
			charges = entryLine5.ConfirmedFees.Cast<CusEntryLineFee>();
			cud = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.CUD);
			AssertCusEntryLineFee(cud, 0m, 0m, 0m, "");
			gst = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.GST);
			AssertCusEntryLineFee(gst, 630.15m, 12603m, 5m, "%");
			tot = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.TOT);
			AssertCusEntryLineFee(tot, 630.15m, 0m, 0m, "");
			vft = charges.FirstOrDefault(x => x.CF_ChargeType == CADDutyTaxFeeTypeCodes.Codes.VFT);
			AssertCusEntryLineFee(vft, 0m, 0m, 0m, "");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestApprovalPendingMessage()
		{
			var approvalPendingMessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetaData xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
    <CommunicationMetaData>
        <ApplicationReferenceID>3333333333333700002001</ApplicationReferenceID>
        <Recipient>
            <ID>822066668RM0002</ID>
       </Recipient>
    </CommunicationMetaData>
    <Response>
        <IssueDateTime>
            <DateTimeString>20220222170330</DateTimeString>
        </IssueDateTime>
        <Status>
            <NameCode>42</NameCode>
        </Status>
    </Response>
</DocumentMetaData>";

			var expectedBody = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\CADResponseInterpretation\ApprovalPending.html");

			var declaration = CreatedDeclaration("C0ECDC26-682C-44B4-BC59-FE88C6F14A01", "B00001000", "33333333333337");
			Factory.Save();

			var approvalPendingMessage = Factory.New<CADMessage>();
			approvalPendingMessage.EM_ReceiveTransmit = "RCV";
			approvalPendingMessage.EM_MessageText = approvalPendingMessageText;

			var entry = declaration.B3EntryHeader;

			AssertNoExceptionThrown(() =>
			{
				processor.ProcessMessage(approvalPendingMessage);
				AssertEquals(1, entry.Messages.Count);
				AssertEquals(CusEntryHeader.Schema.TableName, approvalPendingMessage.EM_LinkTable);
				AssertEquals(entry.PK, approvalPendingMessage.EM_LinkUniqueID);
				AssertEquals(EDIMessage.Status.Received, approvalPendingMessage.EM_Status);
				AssertEquals(MessageStatusList.Codes.AwaitingOriginal, approvalPendingMessage.EM_MessageSubType);
				AssertEquals(MessageStatusList.Codes.AwaitingOriginal, entry.CH_Status);
				AssertEquals(CADEntryStatusList.Codes.ApprovalPending, entry.CH_EntryStatus);
				AssertEquals(0, logger.Logs.Count());
				AssertMultilineASCIIEquals(expectedBody.Replace("\r\n<tr><td>", "<tr><td>").Replace("{VersionNumber}", $"VersionNumber={new EnterpriseInformationRetriever().VersionNumber}"), approvalPendingMessage.EM_MessageInterpretation);
				AssertEmail("Approval pending Commercial Accounting Declaration response has been received for 33333333333337", expectedBody.Replace("{VersionNumber}", $"VersionNumber={new EnterpriseInformationRetriever().VersionNumber}"), approvalPendingMessage.EM_MessageText, ZString.Empty);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFunctionalAcknowledgementMessage()
		{
			var functionalAcknowledgementMessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetaData xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
    <CommunicationMetaData>
        <ApplicationReferenceID>3333333333333700002001</ApplicationReferenceID>
        <Recipient>
            <ID>822066668RM0002</ID>
       </Recipient>
    </CommunicationMetaData>
    <Response>
        <IssueDateTime>
            <DateTimeString>20220222170330</DateTimeString>
        </IssueDateTime>
        <Status>
            <NameCode>200</NameCode>
        </Status>
    </Response>
</DocumentMetaData>";

			var functionalAcknowledgementMessageTextWithoutAwaiting = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetaData xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
    <CommunicationMetaData>
        <ApplicationReferenceID>1111111111111700002001</ApplicationReferenceID>
        <Recipient>
            <ID>811066668RM0002</ID>
       </Recipient>
    </CommunicationMetaData>
    <Response>
        <IssueDateTime>
            <DateTimeString>20241111170330</DateTimeString>
        </IssueDateTime>
        <Status>
            <NameCode>200</NameCode>
        </Status>
    </Response>
</DocumentMetaData>";

			var expectedBody = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\CADResponseInterpretation\FunctionalAcknowledgement.html");
			expectedBody = expectedBody.Replace("{VersionNumber}", $"VersionNumber={new EnterpriseInformationRetriever().VersionNumber}");

			var declaration = CreatedDeclaration("C0ECDC26-682C-44B4-BC59-FE88C6F14A01", "B00001000", "33333333333337");
			Factory.Save();

			var functionalAckMessage = Factory.New<CADMessage>();
			functionalAckMessage.EM_MessageText = functionalAcknowledgementMessageText;
			functionalAckMessage.EM_ReceiveTransmit = "RCV";
			var entry = declaration.B3EntryHeader;
			AssertNoExceptionThrown(() =>
			{
				processor.ProcessMessage(functionalAckMessage);
				AssertEquals(1, entry.Messages.Count);
				AssertEquals(CusEntryHeader.Schema.TableName, functionalAckMessage.EM_LinkTable);
				AssertEquals(entry.PK, functionalAckMessage.EM_LinkUniqueID);
				AssertEquals(EDIMessage.Status.Received, functionalAckMessage.EM_Status);
				AssertEquals(MessageStatusList.Codes.AcknowledgedOriginal, functionalAckMessage.EM_MessageSubType);
				AssertEquals(MessageStatusList.Codes.AcknowledgedOriginal, entry.CH_Status);
				AssertEquals(CADEntryStatusList.Codes.Acknowledged, entry.CH_EntryStatus);
				AssertEquals(0, logger.Logs.Count());
				AssertMultilineASCIIEquals(expectedBody, functionalAckMessage.EM_MessageInterpretation);
				AssertEmail("Acknowledged Commercial Accounting Declaration response has been received for 33333333333337", expectedBody, functionalAckMessage.EM_MessageText, ZString.Empty);
			});

			var functionalAckMessageWithoutAwaiting = Factory.New<CADMessage>();
			functionalAckMessageWithoutAwaiting.EM_MessageText = functionalAcknowledgementMessageTextWithoutAwaiting;
			functionalAckMessageWithoutAwaiting.EM_ReceiveTransmit = "RCV";
			var declarationWithoutAwaiting = CreatedDeclaration(ZGuid.NewZGuid().ToString(), "B00003000", "22222222222227");
			var entryWithoutAwaiting = declarationWithoutAwaiting.B3EntryHeader;
			entryWithoutAwaiting.CH_Status = MessageStatusList.Codes.Unknown;
			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				processor.ProcessMessage(functionalAckMessageWithoutAwaiting);
				AssertEquals(MessageStatusList.Codes.AcknowledgedOriginal, functionalAckMessageWithoutAwaiting.EM_MessageSubType);
				AssertEquals(MessageStatusList.Codes.Unknown, entryWithoutAwaiting.CH_Status);
				AssertEquals(ZString.Empty, entryWithoutAwaiting.CH_EntryStatus);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSyntaxErrorMessage()
		{
			var syntaxErrorMessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetaData xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
    <CommunicationMetaData>
        <ApplicationReferenceID>3333333333333700002001</ApplicationReferenceID>
        <Recipient>
            <ID>822066668RM0002</ID>
        </Recipient>
    </CommunicationMetaData>
    <Response> 
        <IssueDateTime>
        <DateTimeString>20200209125901</DateTimeString> 
        </IssueDateTime>
        <Error>
            <Description>Missing Importer Name</Description>
            <ValidationCode>4</ValidationCode>
            <Pointer>
                <Location>DocumentMetaData/Declaration</Location>
            </Pointer>
        </Error>
        <Status>
        <NameCode>400</NameCode>
        </Status>
    </Response>
</DocumentMetaData>";

			var expectedBody = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\CADResponseInterpretation\SyntaxError.html");
			expectedBody = expectedBody.Replace("{VersionNumber}", $"VersionNumber={new EnterpriseInformationRetriever().VersionNumber}");

			var declaration = CreatedDeclaration("C0ECDC26-682C-44B4-BC59-FE88C6F14A01", "B00001000", "33333333333337");
			Factory.Save();

			var syntaxErrorAckMessage = Factory.New<CADMessage>();
			syntaxErrorAckMessage.EM_MessageText = syntaxErrorMessageText;
			syntaxErrorAckMessage.EM_ReceiveTransmit = "RCV";
			var entry = declaration.B3EntryHeader;
			AssertNoExceptionThrown(() =>
			{
				processor.ProcessMessage(syntaxErrorAckMessage);
				AssertEquals(1, entry.Messages.Count);
				AssertEquals(CusEntryHeader.Schema.TableName, syntaxErrorAckMessage.EM_LinkTable);
				AssertEquals(entry.PK, syntaxErrorAckMessage.EM_LinkUniqueID);
				AssertEquals(EDIMessage.Status.Received, syntaxErrorAckMessage.EM_Status);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, syntaxErrorAckMessage.EM_MessageSubType);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, entry.CH_Status);
				AssertEquals(CADEntryStatusList.Codes.InvalidSyntax, entry.CH_EntryStatus);
				AssertEquals(0, logger.Logs.Count());
				AssertMultilineASCIIEquals(expectedBody, syntaxErrorAckMessage.EM_MessageInterpretation);
				AssertEmail("Invalid Syntax Commercial Accounting Declaration response has been received for 33333333333337", expectedBody, syntaxErrorAckMessage.EM_MessageText, ZString.Empty);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestErrorMessage()
		{
			var errorMessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetaData xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
    <CommunicationMetaData>
        <ApplicationReferenceID>3333333333333700002001</ApplicationReferenceID>
        <Recipient>
            <ID>822066668RM0002</ID>
        </Recipient>
    </CommunicationMetaData>
    <Response> 
        <IssueDateTime>
        <DateTimeString>20200209125901</DateTimeString> 
        </IssueDateTime>
        <Error>
            <Description>Missing Importer Name</Description>
            <ValidationCode>4</ValidationCode>
            <Pointer>
                <Location>DocumentMetaData/Declaration</Location>
            </Pointer>
        </Error>
        <Status>
        <NameCode>41</NameCode>
        </Status>
    </Response>
</DocumentMetaData>";

			var expectedBody = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\CADResponseInterpretation\Error.html");
			expectedBody = expectedBody.Replace("{VersionNumber}", $"VersionNumber={new EnterpriseInformationRetriever().VersionNumber}");

			var declaration = CreatedDeclaration("C0ECDC26-682C-44B4-BC59-FE88C6F14A01", "B00001000", "33333333333337");
			Factory.Save();

			var errorAckMessage = Factory.New<CADMessage>();
			errorAckMessage.EM_MessageText = errorMessageText;
			errorAckMessage.EM_ReceiveTransmit = "RCV";
			var entry = declaration.B3EntryHeader;
			AssertNoExceptionThrown(() =>
			{
				processor.ProcessMessage(errorAckMessage);
				AssertEquals(1, entry.Messages.Count);
				AssertEquals(CusEntryHeader.Schema.TableName, errorAckMessage.EM_LinkTable);
				AssertEquals(entry.PK, errorAckMessage.EM_LinkUniqueID);
				AssertEquals(EDIMessage.Status.Received, errorAckMessage.EM_Status);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, errorAckMessage.EM_MessageSubType);
				AssertEquals(MessageStatusList.Codes.ErrorOriginal, entry.CH_Status);
				AssertEquals(CADEntryStatusList.Codes.Rejected, entry.CH_EntryStatus);
				AssertEquals(0, logger.Logs.Count());
				AssertMultilineASCIIEquals(expectedBody, errorAckMessage.EM_MessageInterpretation);
				AssertEmail("Rejected Commercial Accounting Declaration response has been received for 33333333333337", expectedBody, errorAckMessage.EM_MessageText, ZString.Empty);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDoProcessingReturningStatus()
		{
			var expectedBody = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\CADResponseInterpretation\Accept.html");
			expectedBody = expectedBody.Replace("{VersionNumber}", $"VersionNumber={new EnterpriseInformationRetriever().VersionNumber}");

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var declaration = CreatedDeclaration("C0ECDC26-682C-44B4-BC59-FE88C6F14A01", "B00001000", "33333333333337");
				var topGroupInvoice = declaration.JobComInvoiceGroupHeaders.AddNew();
				topGroupInvoice.JZ_InvoiceNumber = JobComInvoiceGroupHeader.AllInvoices;
				declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;

				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceDisplaySequence = 1;
				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_InvoiceDisplaySequence = 2;
				var invoiceLine = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
				invoiceLine.JI_LineNo = 1;
				var invoiceLine2 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
				invoiceLine2.JI_LineNo = 2;
				var invoiceLine3 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
				invoiceLine3.JI_LineNo = 3;
				var invoiceLine4 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
				invoiceLine4.JI_LineNo = 4;
				var invoiceLine5 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
				invoiceLine5.JI_LineNo = 5;
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				var entry = declaration.B3EntryHeader;
				var entryLine = entry.MergedLines.First();
				var entryLineFee1 = entryLine.ConfirmedFees.AddNew();
				entryLineFee1.CF_Source = "CUS";
				entryLineFee1.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount;
				entryLineFee1.CF_ChargeAmount = 100m;

				var entryLineFee2 = entryLine.Fees.AddNew();
				entryLineFee2.CF_Source = "CW1";
				entryLineFee2.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount;
				entryLineFee2.CF_ChargeAmount = 100m;
				entry.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
				Factory.Save();

				AssertEquals(1, entryLine.Fees.Count);
				AssertEquals(1, entryLine.ConfirmedFees.Count);

				resourceRetriever = new EmbeddedResourceRetriever();
				expectedMessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.MessageProcessors.ResponseMessageProcessors.Import.TestFiles.CADReceivedMessageWithMultipleLines.xml");
				var receivedMessage = Factory.New<CADMessage>();
				receivedMessage.EM_MessageText = expectedMessageText;

				var entryLine2 = entry.MergedLines[1];
				var entryLine3 = entry.MergedLines[2];
				var entryLine4 = entry.MergedLines[3];
				var entryLine5 = entry.MergedLines[4];

				AssertNoExceptionThrown(() =>
				{
					processor.ProcessMessage(receivedMessage);
					AssertEquals(CusEntryHeader.Schema.TableName, receivedMessage.EM_LinkTable);
					AssertEquals(entry.PK, receivedMessage.EM_LinkUniqueID);
					AssertEquals(EDIMessage.Status.Received, receivedMessage.EM_Status);
					AssertEquals(MessageStatusList.Codes.ClearOriginal, receivedMessage.EM_MessageSubType);
					AssertEquals(CADEntryStatusList.Codes.Approved, entry.CH_EntryStatus);
					AssertEquals(MessageStatusList.Codes.ClearOriginal, entry.CH_Status);
					AssertEquals(0, logger.Logs.Count());
					AssertEquals((ZShort)1, entryLine.CL_GoodsShipmentSequence);
					AssertEquals((ZShort)1, entryLine.CL_CommoditySequence);
					AssertEquals(1, entryLine.Fees.Count);
					AssertEquals(4, entryLine.ConfirmedFees.Count);
					AssertEquals(582.26m, entryLineFee1.CF_ChargeAmount);
					AssertEquals(100m, entryLineFee2.CF_ChargeAmount);
					AssertMultilineASCIIEquals(expectedBody, receivedMessage.EM_MessageInterpretation);
					AssertEmail("Approved Commercial Accounting Declaration response has been received for 33333333333337", expectedBody, receivedMessage.EM_MessageText, ZString.Empty);

					AssertEquals((ZShort)1, entryLine2.CL_GoodsShipmentSequence);
					AssertEquals((ZShort)2, entryLine2.CL_CommoditySequence);
					AssertEquals((ZShort)2, entryLine3.CL_GoodsShipmentSequence);
					AssertEquals((ZShort)3, entryLine3.CL_CommoditySequence);
					AssertEquals((ZShort)2, entryLine4.CL_GoodsShipmentSequence);
					AssertEquals((ZShort)4, entryLine4.CL_CommoditySequence);
					AssertEquals((ZShort)2, entryLine5.CL_GoodsShipmentSequence);
					AssertEquals((ZShort)5, entryLine5.CL_CommoditySequence);
				});

				var newDeclaration = declaration.GetNewCopyToPRECARMAdjustmentDeclaration();
				newDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				newDeclaration.JE_IsCancelled = false;

				entry = newDeclaration.ActiveEntryHeaders.AddNew();
				entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
				entry.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
				entry = newDeclaration.B3EntryHeader;
				newDeclaration.JE_DeclarationReference = "B00001001";
				entryLine = entry.MergedLines.AddNew();
				entryLine.CL_LineNumber = 1;
				entryLineFee1 = entryLine.ConfirmedFees.AddNew();
				entryLineFee1.CF_Source = "CUS";
				entryLineFee1.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount;
				entryLineFee1.CF_ChargeAmount = 100m;

				entryLineFee2 = entryLine.Fees.AddNew();
				entryLineFee2.CF_Source = "CW1";
				entryLineFee2.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount;
				entryLineFee2.CF_ChargeAmount = 100m;

				entryLine2 = entry.MergedLines.AddNew();
				entryLine2.CL_LineNumber = 2;
				entryLine3 = entry.MergedLines.AddNew();
				entryLine3.CL_LineNumber = 3;
				entryLine4 = entry.MergedLines.AddNew();
				entryLine4.CL_LineNumber = 4;
				entryLine5 = entry.MergedLines.AddNew();
				entryLine5.CL_LineNumber = 5;
				var allInvoiceLines = newDeclaration.FilteredInvoiceLines.OfType<JobComInvoiceLine>().OrderBy(x => x.JI_B3LineNumber).ToArray();
				allInvoiceLines[0].AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
				allInvoiceLines[1].AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine2);
				allInvoiceLines[2].AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine3);
				allInvoiceLines[3].AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine4);
				allInvoiceLines[4].AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine5);

				Factory.Save();

				receivedMessage = Factory.New<CADMessage>();
				receivedMessage.EM_MessageText = expectedMessageText;

				AssertEquals(1, entryLine.Fees.Count);
				AssertEquals(1, entryLine.ConfirmedFees.Count);

				AssertNoExceptionThrown(() =>
				{
					processor.ProcessMessage(receivedMessage);
					AssertEquals(CusEntryHeader.Schema.TableName, receivedMessage.EM_LinkTable);
					AssertEquals(entry.PK, receivedMessage.EM_LinkUniqueID);
					AssertEquals(EDIMessage.Status.Received, receivedMessage.EM_Status);
					AssertEquals(MessageStatusList.Codes.ClearOriginal, receivedMessage.EM_MessageSubType);
					AssertEquals(CADEntryStatusList.Codes.Approved, entry.CH_EntryStatus);
					AssertEquals(MessageStatusList.Codes.ClearOriginal, entry.CH_Status);
					AssertEquals(0, logger.Logs.Count());
					AssertEquals((ZShort)1, entryLine.CL_GoodsShipmentSequence);
					AssertEquals((ZShort)1, entryLine.CL_CommoditySequence);
					AssertEquals(1, entryLine.Fees.Count);
					AssertEquals(4, entryLine.ConfirmedFees.Count);
					AssertEquals(582.26m, entryLineFee1.CF_ChargeAmount);
					AssertEquals(100m, entryLineFee2.CF_ChargeAmount);
					AssertEmail("Approved Commercial Accounting Declaration response has been received for 33333333333337", expectedBody, receivedMessage.EM_MessageText, ZString.Empty);

					AssertEquals((ZShort)1, entryLine2.CL_GoodsShipmentSequence);
					AssertEquals((ZShort)2, entryLine2.CL_CommoditySequence);
					AssertEquals((ZShort)2, entryLine3.CL_GoodsShipmentSequence);
					AssertEquals((ZShort)3, entryLine3.CL_CommoditySequence);
					AssertEquals((ZShort)2, entryLine4.CL_GoodsShipmentSequence);
					AssertEquals((ZShort)4, entryLine4.CL_CommoditySequence);
					AssertEquals((ZShort)2, entryLine5.CL_GoodsShipmentSequence);
					AssertEquals((ZShort)5, entryLine5.CL_CommoditySequence);
				});
			}
		}

		public void TestSetupEntryDetails_NoExceptionThrown()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var declaration = CreatedDeclaration("48779FFC-AD58-4C4E-8910-60C851D9D0A1", "B00001000", "33333333333337");
				var topGroupInvoice = declaration.JobComInvoiceGroupHeaders.AddNew();
				topGroupInvoice.JZ_InvoiceNumber = JobComInvoiceGroupHeader.AllInvoices;
				declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
				declaration.CA_DeclarationException = CAExceptionCodeList.Codes.CustomsAmountDueDiscrepancy;

				var invoice1 = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
				invoiceLine1.CA_TreatmentCode = "02";
				var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
				invoiceLine2.CA_TreatmentCode = "10";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				var entry = declaration.B3EntryHeader;
				var entryLine = entry.MergedLines.First();
				var entryLineFee1 = entryLine.ConfirmedFees.AddNew();
				entryLineFee1.CF_Source = "CUS";
				entryLineFee1.CF_ChargeType = CADDutyTaxFeeTypeCodes.Codes.GST;
				entryLineFee1.CF_ChargeAmount = 100m;

				var entryLineFee2 = entryLine.Fees.AddNew();
				entryLineFee2.CF_Source = "CW1";
				entryLineFee2.CF_ChargeType = EntryChargeTypeList.Codes.TotalGSTAmount;
				entryLineFee2.CF_ChargeAmount = 100m;
				entry.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
				Factory.Save();

				resourceRetriever = new EmbeddedResourceRetriever();
				expectedMessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.MessageProcessors.ResponseMessageProcessors.Import.TestFiles.CADReceivedMessageNoExceptionThrown.xml");
				var receivedMessage = Factory.New<CADMessage>();
				receivedMessage.EM_MessageText = expectedMessageText;

				AssertNoExceptionThrown(() => { processor.ProcessMessage(receivedMessage); });
				AssertEquals(CAExceptionCodeList.Codes.CustomsAmountDueDiscrepancy, declaration.CA_DeclarationException);

				entryLineFee2.CF_ChargeAmount = 582.26m;
				entry.MergedLines[1].Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 582.26m);
				processor.ProcessMessage(receivedMessage);
				AssertEquals(ZString.Empty, declaration.CA_DeclarationException);
			}
		}

		public void TestFailToFindEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "33333";
			declaration.TransactionNumber.SequentialNumber = "33333333";
			declaration.IsCancelled = false;

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.ACIForwarderClose;
			entry.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entry.CH_BGMReference = declaration.TransactionNumber.ToString();
			Factory.Save();
			var receivedMessage = Factory.New<CADMessage>();
			receivedMessage.EM_MessageText = expectedMessageText;
			AssertExceptionThrown<CouldNotFindLinkedObjectException>(() =>
			{
				processor.ProcessMessage(receivedMessage);
			});
			AssertEquals(ZString.Empty, receivedMessage.EM_LinkTable);
			AssertEquals(ZGuid.Empty, receivedMessage.EM_LinkUniqueID);
			AssertEquals(EDIMessage.Status.Failed, receivedMessage.EM_Status);
			var logs = logger.Logs.ToArray();
			AssertEquals(0, logs.Length);
		}

		public void TestDifferentAmount()
		{
			var declaration = CreatedDeclaration("C0ECDC26-682C-44B4-BC59-FE88C6F14A01", "B00001000", "33333333333337");
			Factory.Save();

			var entry = declaration.B3EntryHeader;

			var receivedMessage = Factory.New<CADMessage>();
			receivedMessage.EM_MessageText = expectedMessageText;

			AssertExceptionThrown<UnableToInterpretMessageException>(() =>
			{
				processor.ProcessMessage(receivedMessage);
			});
			AssertEquals(CusEntryHeader.Schema.TableName, receivedMessage.EM_LinkTable);
			AssertEquals(entry.PK, receivedMessage.EM_LinkUniqueID);
			AssertEquals(EDIMessage.Status.Failed, receivedMessage.EM_Status);
			var logs = logger.Logs.ToArray();
			AssertEquals(1, logs.Length);
			AssertEquals(Integration.LogType.Error, logs[0].Type);
			AssertContains("Different amount. Commodity amount: 1, Entry line amount: 0", logs[0].Message);
		}

		public void TestDifferentAmountWithNoException()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var declaration = CreatedDeclaration("C0ECDC26-682C-44B4-BC59-FE88C6F14A01", "B00001000", "33333333333337");
				var topGroupInvoice = declaration.JobComInvoiceGroupHeaders.AddNew();
				topGroupInvoice.JZ_InvoiceNumber = JobComInvoiceGroupHeader.AllInvoices;
				declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;

				var invoice1 = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
				invoiceLine1.CA_TreatmentCode = "02";
				var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
				invoiceLine2.CA_TreatmentCode = "10";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				Factory.Save();

				var receivedMessage = Factory.New<CADMessage>();
				receivedMessage.EM_MessageText = expectedMessageText;
				invoiceLine2.Delete();

				AssertNoExceptionThrown(() =>
				{
					processor.ProcessMessage(receivedMessage);
				});
			}
		}

		public void TestNoExceptionThrowWhenSetupIncorrectVersionID()
		{
			var newFactory = new BusinessObjectFactory();
			var receiveMessage = newFactory.New<CADMessage>();
			receiveMessage.EM_MessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.MessageProcessors.ResponseMessageProcessors.Import.TestFiles.CADReceivedMessageWithIncorrectData.xml");

			var processor = new CADResponseMessageProcessor(logger);
			newFactory.Save();

			var declaration = newFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "33333";
			declaration.TransactionNumber.SequentialNumber = "33333333";
			declaration.IsCancelled = false;

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entry.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entry.CH_BGMReference = declaration.TransactionNumber.ToString();

			var entryLine = entry.AllEntryLines.AddNew();
			var entryLineFee1 = entryLine.ConfirmedFees.AddNew();
			entryLineFee1.CF_Source = "CUS";
			entryLineFee1.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount;
			entryLineFee1.CF_ChargeAmount = 100m;

			var entryLineFee2 = entryLine.Fees.AddNew();
			entryLineFee2.CF_Source = "CW1";
			entryLineFee2.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount;
			entryLineFee2.CF_ChargeAmount = 100m;
			newFactory.Save();

			AssertExceptionThrown<UnableToInterpretMessageException>(() =>
			{
				processor.ProcessMessage(receiveMessage);
			});

			var logs = logger.Logs.ToArray();
			AssertEquals(1, logs.Length);
			AssertEquals(Integration.LogType.Error, logs[0].Type);
			AssertContains("Incorrect Version Id in response message.", logs[0].Message);
		}

		public void TestSyntaxErrorResponse()
		{
			const string messageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetaData xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
	<CommunicationMetaData>
		<ApplicationReferenceID>3333333333333700002001</ApplicationReferenceID>
		<Recipient>
			<ID>207461995RM0001</ID>
		</Recipient>
	</CommunicationMetaData>
</DocumentMetaData>";
			var newFactory = new BusinessObjectFactory();
			var receiveMessage = newFactory.New<CADMessage>();
			receiveMessage.EM_MessageText = messageText;

			var processor = new CADResponseMessageProcessor(logger);
			newFactory.Save();

			AssertExceptionThrown<UnableToInterpretMessageException>(() =>
			{
				processor.ProcessMessage(receiveMessage);
			});

			var logs = logger.Logs.ToArray();
			AssertEquals(1, logs.Length);
			AssertEquals(Integration.LogType.Error, logs[0].Type);
			AssertContains("Syntax error in CAD response message.", logs[0].Message);
		}

		public void TestSyntaxErrorResponse_NoDeclaration()
		{
			const string messageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetaData xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:Declaration:1"">
    <CommunicationMetaData>
        <ApplicationReferenceID>3333333333333700002001</ApplicationReferenceID>
        <Recipient>
           <ID>207461995RM0001</ID>
        </Recipient>
    </CommunicationMetaData>
    <Response>
        <Status>
            <NameCode>39</NameCode>
        </Status>
    </Response>
</DocumentMetaData>";
			var newFactory = new BusinessObjectFactory();
			var receiveMessage = newFactory.New<CADMessage>();
			receiveMessage.EM_MessageText = messageText;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "33333";
			declaration.TransactionNumber.SequentialNumber = "33333333";
			declaration.IsCancelled = false;

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entry.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entry.CH_BGMReference = declaration.TransactionNumber.ToString();
			Factory.Save();

			var processor = new CADResponseMessageProcessor(logger);
			newFactory.Save();

			AssertExceptionThrown<UnableToInterpretMessageException>(() =>
			{
				processor.ProcessMessage(receiveMessage);
			});

			var logs = logger.Logs.ToArray();
			AssertEquals(1, logs.Length);
			AssertEquals(Integration.LogType.Error, logs[0].Type);
			AssertContains("Syntax error in CAD response message.", logs[0].Message);
		}

		IDisposable instanceDetailsDisposable;

		protected override void SetUp()
		{
			base.SetUp();

			instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest();
			resourceRetriever = new EmbeddedResourceRetriever();
			expectedMessageText = resourceRetriever.GetString("Enterprise.Customs.CA.Business.Test.MessageProcessors.ResponseMessageProcessors.Import.TestFiles.CADReceivedMessage.xml");

			processor = new CADResponseMessageProcessor(logger);
			Factory.Save();
		}

		protected override void TearDown()
		{
			instanceDetailsDisposable?.Dispose();
			base.TearDown();
		}

		JobDeclaration CreatedDeclaration(string pk, string jobNumber, string transactionNo)
		{
			var declaration = Factory.NewWithPrimaryKey<JobDeclaration>(new Guid(pk));
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.JE_DeclarationReference = jobNumber;
			declaration.JE_IsCancelled = false;
			var transactionNumber = CusEntryNumber.LoadOrCreate(declaration, CusEntryNumber.EntryType.CATransactionNumber, "CA");
			transactionNumber.CE_EntryNum = transactionNo;

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entry.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entry.CH_BGMReference = declaration.TransactionNumber.ToString();

			return declaration;
		}

		void AssertCusEntryLineFee(CusEntryLineFee fee, ZDecimal amount, ZDecimal baseAmount, ZDecimal rate, ZString method)
		{
			CombineAssertions(() =>
			{
				AssertEquals("CF_ChargeAmount", amount, fee.CF_ChargeAmount);
				AssertEquals("CF_BaseValue", baseAmount, fee.CF_BaseValue);
				AssertEquals("CF_Rate", rate, fee.CF_Rate);
				AssertEquals("CF_MethodOfCalculation", method, fee.CF_MethodOfCalculation);
			});
		}

		EmbeddedResourceRetriever resourceRetriever;
		ZString expectedMessageText;
		CADResponseMessageProcessor processor;
	}
}
