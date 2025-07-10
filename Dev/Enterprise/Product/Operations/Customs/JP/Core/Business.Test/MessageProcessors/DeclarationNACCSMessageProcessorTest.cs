using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Business.MessageProcessors;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(DeclarationNACCSMessageProcessor))]
	sealed class DeclarationNACCSMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessIDAACK()
		{
			var testMessageData = "   HCH01AAS0180  202311061433  XXXXX                 XXX70201@MAIL.PROD.NACCS6                                       21233784930                                                                                   1359580409                          001EP   0000000007                                                                                                    Q2           S3              0005\r\ntest";
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "NG1";

			var staff = group.Staff.AddNew();
			staff.GS_Code = "TS1";
			staff.GS_LoginName = "TestUser00";
			staff.GS_EmailAddress = "jpctest@cargowise.com";

			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_DeclarationReference = "B00200716";
			jobDeclaration.JE_HouseBill = "8523";
			jobDeclaration.JE_GB = branch.PK;
			var entry = jobDeclaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "1234567890";

			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			outgoingMessage.EM_MessageType = JPProcedureCodeList.Codes.IDC;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageNum = "JRIDA0012345678900000000001";
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			outgoingMessage.EM_LinkedObject = entry;
			outgoingMessage.EM_GB = branch.PK;

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageType = JPProcedureCodeList.Codes.IDC;
			incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes(testMessageData);
			incomingMessage.EM_MessageNum = "JRIDA00ABCDEFG12345678900000000002";
			incomingMessage.EM_LinkedObject = entry;

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes(testMessageData))).Returns(parseResultStub.Object);
			parseResultStub.Setup(m => m.HasResultCode).Returns(true);
			parseResultStub.Setup(m => m.IsSuccess).Returns(true);
			parseHeaderStub.Setup(m => m.ProcedureCode).Returns("IDA");
			parseHeaderStub.Setup(m => m.OutputInformationCode).Returns("*AIDA");
			parseHeaderStub.Setup(m => m.MessageTag).Returns("JRIDA0012345678900000000001");
			parseHeaderStub.Setup(m => m.ReceivedDateTime).Returns(DateTime.ParseExact("201802090211", "yyyyMMddhhmm", CultureInfo.CurrentCulture));
			parseHeaderStub.Setup(m => m.Subject).Returns("10321500220 KLL02174543");
			parseHeaderStub.Setup(m => m.InputReference).Returns("1234567890");
			messageStub.Setup(m => m.EntryNumber).Returns("NewEntryNumber");

			var messageProcessor = new DeclarationNACCSMessageProcessor(new LoggingInformation());
			var expectedEmailBody = ReadTestFile("IDA_AIDA.htm");

			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				CombineAssertions(() =>
				{
					messageProcessor.ProcessMessage(incomingMessage);
					incomingMessage.Factory.Save();

					AssertEquals("Should set entry header status to Acknowledged", JPMessageStatusList.Codes.Acknowledged, entry.CH_Status);
					AssertEquals("Should set entry number", "NewEntryNumber", entry.EntryNumber);
					AssertEquals("Should set status as Processed.", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);

					var email = Env.OutgoingMailManager.EmailsCreated.First();
					AssertEquals("Subject", "NACCS Response Messages - JRIDA00ABCDEFG12345678900000000002", email.Subject);
					AssertContains("Email Recipients", staff.GS_EmailAddress, email.Recipients.RecipientsAsDelimitedString(), ignoreCase: true);
					AssertEquals("Body", expectedEmailBody, email.Body);

					var log = entry.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.InterchangeAcknowledged.Code).OrderByDescending(a => a.SL_PostedTimeUtc).FirstOrDefault();
					AssertEquals("SL_Reference", testMessageData.Substring(0, 398), log.SL_Reference);

					parserMock.VerifyAll();
				});
			}
		}

		public void TestProcessIDAREJ()
		{
			var testMessageData = "   HCH01AAS0180  202311061433  XXXXX                 XXX70201@MAIL.PROD.NACCS6                                       21233784930                                                                                   1359580409                          001EP   0000000007                                                                                                    Q2           S3              0005\r\ntest";
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "NG1";

			var staff = group.Staff.AddNew();
			staff.GS_Code = "TS1";
			staff.GS_LoginName = "TestUser00";
			staff.GS_EmailAddress = "jpctest@cargowise.com";

			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_DeclarationReference = "B00200716";
			jobDeclaration.JE_HouseBill = "8523";
			jobDeclaration.JE_GB = branch.PK;
			var entry = jobDeclaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "1234567890";

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageType = JPProcedureCodeList.Codes.IDA;
			incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes(testMessageData);
			incomingMessage.EM_MessageNum = "JRIDA00ABCDEFG12345678900000000001";
			incomingMessage.EM_LinkedObject = entry;

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes(testMessageData))).Returns(parseResultStub.Object);
			parseResultStub.Setup(m => m.HasResultCode).Returns(true);
			parseResultStub.Setup(m => m.IsSuccess).Returns(false);
			parseHeaderStub.Setup(m => m.InputReference).Returns("1234567890");
			parseHeaderStub.Setup(m => m.OutputInformationCode).Returns("*IDA01");

			var messageProcessor = new DeclarationNACCSMessageProcessor(new LoggingInformation());

			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				CombineAssertions(() =>
				{
					messageProcessor.ProcessMessage(incomingMessage);
					incomingMessage.Factory.Save();
					entry.Reload();
					AssertEquals("Should set entry header status to Rejected", JPMessageStatusList.Codes.Rejected, entry.CH_Status);
					AssertNullOrEmpty("Should not set entry number", entry.EntryNumber);
					AssertEquals("Should set status as Processed.", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);

					AssertEquals("No email is sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);

					var log = entry.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.InterchangeRejected.Code).OrderByDescending(a => a.SL_PostedTimeUtc).FirstOrDefault();
					AssertEquals("SL_Reference", testMessageData.Substring(0, 398), log.SL_Reference);
				});
			}
		}

		public void TestProcessCopyRecieveEventLog()
		{
			var testMessageData = "   HCH01AAS0180  202311061433  XXXXX                 XXX70201@MAIL.PROD.NACCS6                                       21233784930                                                                                   1359580409                          001EP   0000000007                                                                                                    Q2           S3              0005\r\ntest";
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "NG1";

			var staff = group.Staff.AddNew();
			staff.GS_Code = "TS1";
			staff.GS_LoginName = "TestUser00";
			staff.GS_EmailAddress = "jpctest@cargowise.com";

			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_DeclarationReference = "B00200716";
			jobDeclaration.JE_HouseBill = "8523";
			jobDeclaration.JE_GB = branch.PK;
			var entry = jobDeclaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "1234567890";

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes(testMessageData))).Returns(parseResultStub.Object);
			parseResultStub.Setup(m => m.HasResultCode).Returns(true);
			parseResultStub.Setup(m => m.IsSuccess).Returns(true);
			parseHeaderStub.Setup(m => m.ProcedureCode).Returns("EDA");
			parseHeaderStub.Setup(m => m.OutputInformationCode).Returns("SAE0LA2");
			parseHeaderStub.Setup(m => m.ReceivedDateTime).Returns(DateTime.ParseExact("201802090211", "yyyyMMddhhmm", CultureInfo.CurrentCulture));
			parseHeaderStub.Setup(m => m.InputReference).Returns("1234567890");

			var messageProcessor = new DeclarationNACCSMessageProcessor(new LoggingInformation());

			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				CombineAssertions(() =>
				{
					var copyRecieveEventLogs = entry.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.CopyReceived.Code);
					AssertEquals(0, copyRecieveEventLogs.Count());

					var incomingMessage = Factory.New<EDIMessage>();
					incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
					incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					incomingMessage.EM_Status = EDIMessage.Status.Queued;
					incomingMessage.EM_MessageType = JPProcedureCodeList.Codes.EDA;
					incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes(testMessageData);
					incomingMessage.EM_LinkedObject = entry;

					messageProcessor.ProcessMessage(incomingMessage);
					incomingMessage.Factory.Save();
					entry.Reload();
					AssertEquals("Should set status as Processed.", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);

					copyRecieveEventLogs = entry.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.CopyReceived.Code);
					AssertEquals(1, copyRecieveEventLogs.Count());
				});
			}
		}

		public void TestImportEntryResponse()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_DeclarationReference = "B00200716";
			jobDeclaration.JE_HouseBill = "8523";
			var entry = jobDeclaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "1234567890";

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageType = JPProcedureCodeList.Codes.IDC;
			incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes("TESTMESSAGETEXT");
			incomingMessage.EM_MessageNum = "JRIDA00ABCDEFG12345678900000000002";
			incomingMessage.EM_LinkedObject = entry;

			Factory.Save();

			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes("TESTMESSAGETEXT"))).Returns(parseResultStub.Object);
			messageStub.Setup(m => m.EntryNumber).Returns("NewEntryNumber");

			CombineAssertions(() =>
			{
				var messageProcessor = new DeclarationNACCSMessageProcessor(new LoggingInformation());
				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals("Entry number added", "NewEntryNumber", entry.EntryNumber);
				AssertEquals("Entry number category", CusEntryNumber.Categories.CustomsPermitClearanceNumber, entry.CusEntryNumber.CE_Category);

				messageStub.Setup(m => m.EntryNumber).Returns("NewEntryNumber2");
				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals("Entry number changed", "NewEntryNumber2", entry.EntryNumber);

				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals("Entry number unchanged", "NewEntryNumber2", entry.EntryNumber);
			});
		}

		public void TestImportClearanceNotice()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.T;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "1234567890";
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageType = JPProcedureCodeList.Codes.IDC;
			incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes("TESTMESSAGETEXT");
			incomingMessage.EM_MessageNum = "JRIDA00ABCDEFG12345678900000000002";
			incomingMessage.EM_LinkedObject = entryHeader;

			Factory.Save();

			var today = ZDateTime.Today;

			var importClearanceNoticeMessageStub = new Mock<IImportClearanceNotice>();
			importClearanceNoticeMessageStub.Setup(m => m.ApprovalDate).Returns(today.ToDateTime());
			parseResultStub.Setup(m => m.MessageProvider).Returns(importClearanceNoticeMessageStub.Object);
			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes("TESTMESSAGETEXT"))).Returns(parseResultStub.Object);

			CombineAssertions(() =>
			{
				var messageProcessor = new DeclarationNACCSMessageProcessor(new LoggingInformation());
				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals("Bonded Date will not be updated when Declaration Type is not S/M/A", ZDateTime.Empty, invoiceLine.JI_BondedDate);

				AssertUpdateBondedDate(JPImportDeclarationTypeList.Codes.S);
				AssertUpdateBondedDate(JPImportDeclarationTypeList.Codes.M);
				AssertUpdateBondedDate(JPImportDeclarationTypeList.Codes.A);

				importClearanceNoticeMessageStub.Setup(m => m.ApprovalDate).Returns(today.AddDays(2).ToDateTime());
				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals("Bonded Date not updated when not empty", today, invoiceLine.JI_BondedDate);

				void AssertUpdateBondedDate(string declarationType)
				{
					instruction.CEI_Style = declarationType;
					invoiceLine.JI_BondedDate = ZDateTime.Empty;
					messageProcessor.ProcessMessage(incomingMessage);

					AssertEquals("Bonded Date is updated when Declaration Type is S/M/A", today, invoiceLine.JI_BondedDate);
				}
			});
		}

		public void TestImportDeclarationDate()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "1234567890";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageType = JPProcedureCodeList.Codes.IDC;
			incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes("TESTMESSAGETEXT");
			incomingMessage.EM_MessageNum = "JRIDA00ABCDEFG12345678900000000002";
			incomingMessage.EM_LinkedObject = entryHeader;

			Factory.Save();

			var today = ZDateTime.Today;

			var edaDeclarationDateMessageStub = new Mock<IExportDeclarationRegistrationCopy>();
			edaDeclarationDateMessageStub.Setup(m => m.ScheduleDeclarationDate).Returns(today.ToDateTime());
			parseResultStub.Setup(m => m.MessageProvider).Returns(edaDeclarationDateMessageStub.Object);
			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes("TESTMESSAGETEXT"))).Returns(parseResultStub.Object);

			CombineAssertions(() =>
			{
				AssertEquals(ZDateTime.Empty, entryInstruction.CEI_DateForDuty);

				var messageProcessor = new DeclarationNACCSMessageProcessor(new LoggingInformation());
				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals("Schedule declaration date should be updated when empty.", today, entryInstruction.CEI_DateForDuty);

				edaDeclarationDateMessageStub.Setup(m => m.ScheduleDeclarationDate).Returns(today.AddDays(1).ToDateTime());
				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals("Schedule declaration date should not be updated when not empty.", today, entryInstruction.CEI_DateForDuty);
			});

			var idaDeclarationDateMessageStub = new Mock<IImportDeclarationRegistrationCopy>();
			idaDeclarationDateMessageStub.Setup(m => m.ScheduleDeclarationDate).Returns(today.ToDateTime());
			parseResultStub.Setup(m => m.MessageProvider).Returns(idaDeclarationDateMessageStub.Object);
			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes("TESTMESSAGETEXT"))).Returns(parseResultStub.Object);
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;

			CombineAssertions(() =>
			{
				AssertEquals(ZDateTime.Empty, entryInstruction.CEI_DateForDuty);

				var messageProcessor = new DeclarationNACCSMessageProcessor(new LoggingInformation());
				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals("Schedule declaration date should be updated when empty.", today, entryInstruction.CEI_DateForDuty);

				idaDeclarationDateMessageStub.Setup(m => m.ScheduleDeclarationDate).Returns(today.AddDays(1).ToDateTime());
				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals("Schedule declaration date should not be updated when not empty.", today, entryInstruction.CEI_DateForDuty);
			});
		}

		public void TestProcessMessageResult()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_DeclarationReference = "B00200716";
			jobDeclaration.JE_HouseBill = "8523";
			var entry = jobDeclaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "1234567890";

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageType = JPProcedureCodeList.Codes.IDC;
			incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes("TESTMESSAGETEXT");
			incomingMessage.EM_MessageNum = "JRIDA00ABCDEFG12345678900000000002";
			incomingMessage.EM_LinkedObject = entry;

			Factory.Save();

			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes("TESTMESSAGETEXT"))).Returns(parseResultStub.Object);

			CombineAssertions(() =>
			{
				parseResultStub.Setup(m => m.HasResultCode).Returns(true);
				parseResultStub.Setup(m => m.IsSuccess).Returns(true);

				var messageProcessor = new DeclarationNACCSMessageProcessor(new LoggingInformation());
				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals("Should set entry header status to Acknowledged", JPMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				AssertNullOrEmpty("Should not set entry header customs status on success result", entry.CH_EntryStatus);

				parseResultStub.Setup(m => m.IsSuccess).Returns(false);
				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals("Should set entry header status to Rejected", JPMessageStatusList.Codes.Rejected, entry.CH_Status);
				AssertNullOrEmpty("Should not set entry header customs status on error result", entry.CH_EntryStatus);

				parseResultStub.Setup(m => m.IsSuccess).Returns(true);
				parseResultStub.Setup(m => m.HasWarnings).Returns(true);
				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals("Should set entry header status to Acknowledged with warning", JPMessageStatusList.Codes.Acknowledged, entry.CH_Status);
				AssertEquals("Should set entry header customs status to warning", CustomsStatusList.Codes.Warning, entry.CH_EntryStatus);

				parseResultStub.Setup(m => m.HasResultCode).Returns(false);
				parseResultStub.Setup(m => m.MessageProvider).Returns(new IDACopy());
				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals("Should not set copy entry header status to overwrite warning", CustomsStatusList.Codes.Warning, entry.CH_EntryStatus);

				entry.CH_EntryStatus = ZString.Empty;
				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals("Should set copy entry header status not to overwrite warning", CustomsStatusList.Codes.Copied, entry.CH_EntryStatus);
			});
		}

		public void TestReceiveECRRegistrationResponse()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "1234567890";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageType = JPProcedureCodeList.Codes.ECR;
			incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes("TESTMESSAGETEXT");
			incomingMessage.EM_MessageNum = "JRECR00ABCDEFG12345678900000000002";
			incomingMessage.EM_LinkedObject = entryHeader;

			Factory.Save();

			var ecrResponseMessageStub = new Mock<IECRResponse>();
			ecrResponseMessageStub.Setup(m => m.Schema).Returns(new ECRRegistrationInformationResponse());
			ecrResponseMessageStub.Setup(m => m.ExportControlNumber).Returns("12345678901234567890123456789012345");
			parseResultStub.Setup(m => m.MessageProvider).Returns(ecrResponseMessageStub.Object);
			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes("TESTMESSAGETEXT"))).Returns(parseResultStub.Object);

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, entryInstruction.ExportControlNumber);

				var messageProcessor = new DeclarationNACCSMessageProcessor(new LoggingInformation());
				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals("Export Control Number should be updated when empty.", "12345678901234567890123456789012345", entryInstruction.ExportControlNumber);
				Assert("Export Control Number should be readonly beacuse it is generated by system.", entryInstruction.ExportControlNumberInfo.ReadOnly);

				ecrResponseMessageStub.Setup(m => m.ExportControlNumber).Returns("12345");
				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals("Export Control Number should not be updated when not empty.", "12345678901234567890123456789012345", entryInstruction.ExportControlNumber);
			});
		}

		public void TestReceiveECRCancellationResponse()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.ExportControlNumber = "12345678901234567890123456789012345";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "1234567890";
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageType = JPProcedureCodeList.Codes.ECR;
			incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes("TESTMESSAGETEXT");
			incomingMessage.EM_MessageNum = "JRECR00ABCDEFG12345678900000000002";
			incomingMessage.EM_LinkedObject = entryHeader;

			Factory.Save();

			var ecrResponseMessageStub = new Mock<IECRResponse>();
			ecrResponseMessageStub.Setup(m => m.Schema).Returns(new ECRCancellationInformation());
			ecrResponseMessageStub.Setup(m => m.ExportControlNumber).Returns("12345");
			parseResultStub.Setup(m => m.MessageProvider).Returns(ecrResponseMessageStub.Object);
			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes("TESTMESSAGETEXT"))).Returns(parseResultStub.Object);

			CombineAssertions(() =>
			{
				var entryNumber = CusEntryNumber.Load(entryInstruction, CusEntryNumberTypes.JP.ExportControlNumber, Core.Constants.CountryCodes.Japan);
				AssertEquals("12345678901234567890123456789012345", entryInstruction.ExportControlNumber);
				AssertEquals("12345678901234567890123456789012345", entryNumber.CE_EntryNum);

				var messageProcessor = new DeclarationNACCSMessageProcessor(new LoggingInformation());

				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals("Export Control Number should not be deleted because it is not a match.", "12345678901234567890123456789012345", entryInstruction.ExportControlNumber);

				ecrResponseMessageStub.Setup(m => m.ExportControlNumber).Returns("12345678901234567890123456789012345");
				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals("Export Control Number should be empty.", ZString.Empty, entryInstruction.ExportControlNumber);
				Assert("Entry Number should be deleted.", entryNumber.IsDeleted);
			});
		}

		string ReadTestFile(string fileName)
		{
			using (var stream = GetType().Assembly.GetManifestResourceStream($"Enterprise.Customs.JP.Business.Testing.MessageProcessors.Test.{fileName}"))
			{
				return new StreamReader(stream).ReadToEnd();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			parserMock = new Mock<IJPInboundMessageParser>(MockBehavior.Strict);
			parseResultStub = new Mock<IJPInboundMessageParseResult>();
			parseHeaderStub = new Mock<IJPInboundMessageHeader>();
			messageStub = new Mock<IEntryResponse>();
			parseResultStub.Setup(m => m.ResponseHeader).Returns(parseHeaderStub.Object);
			parseResultStub.Setup(m => m.MessageProvider).Returns(messageStub.Object);
			NACCSFactoryServiceTestHelper.SetInboundMessageParser(Factory, parserMock.Object);
		}

		Mock<IJPInboundMessageParser> parserMock;
		Mock<IJPInboundMessageParseResult> parseResultStub;
		Mock<IJPInboundMessageHeader> parseHeaderStub;
		Mock<IEntryResponse> messageStub;
	}
}
