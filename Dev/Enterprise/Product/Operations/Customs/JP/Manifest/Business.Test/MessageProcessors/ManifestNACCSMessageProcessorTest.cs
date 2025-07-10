using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.JP.Manifest.Business.MessageProcessors;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using EDIMessage = Enterprise.Customs.JP.Common.EDIMessage;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(ManifestNACCSMessageProcessor))]
	sealed class ManifestNACCSMessageProcessorTest : TestCaseWithFactory
	{
		public void TestSendEmail()
		{
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

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "MAN0000001";
			manifestHeader.AMA_GB = branch.PK;

			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			outgoingMessage.EM_MessageType = "HCH";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageNum = "HCH0112345678900000000001";
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			outgoingMessage.EM_LinkedObject = manifestHeader;
			outgoingMessage.EM_GB = branch.PK;

			var incomingMessage = GetEDIMessageForTesting(manifestHeader, "TESTMESSAGETEXT");

			Env.OutgoingMailManager.EmailsCreated.Clear();

			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes("TESTMESSAGETEXT"))).Returns(parseResultStub.Object);
			parseResultStub.Setup(m => m.HasResultCode).Returns(true);
			parseResultStub.Setup(m => m.IsSuccess).Returns(true);
			parseHeaderStub.Setup(m => m.ProcedureCode).Returns("HCH01");
			parseHeaderStub.Setup(m => m.OutputInformationCode).Returns("*AHCH01");
			parseHeaderStub.Setup(m => m.MessageTag).Returns("HCH0112345678900000000001");
			parseHeaderStub.Setup(m => m.ReceivedDateTime).Returns(DateTime.ParseExact("201802090211", "yyyyMMddhhmm", CultureInfo.CurrentCulture));
			parseHeaderStub.Setup(m => m.Subject).Returns("10321500220 KLL02174543");
			parseHeaderStub.Setup(m => m.InputReference).Returns("1234567890");

			var messageProcessor = new ManifestNACCSMessageProcessor(new LoggingInformation());

			var expectedEmailBody = ReadTestFile("HCH01_AHCH01.htm");

			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				CombineAssertions(() =>
				{
					messageProcessor.ProcessMessage(incomingMessage);
					incomingMessage.Factory.Save();

					AssertEquals("Should set status as Processed.", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);

					var email = Env.OutgoingMailManager.EmailsCreated.First();
					AssertEquals("Subject", "NACCS Response Messages - HCH01ABCDEFG12345678900000000002", email.Subject);
					AssertContains("Email Recipients", staff.GS_EmailAddress, email.Recipients.RecipientsAsDelimitedString(), ignoreCase: true);
					AssertEquals("Body", expectedEmailBody, email.Body);

					Env.OutgoingMailManager.EmailsCreated.Clear();
					parseResultStub.Setup(m => m.IsSuccess).Returns(false);
					incomingMessage.EM_Status = EDIMessage.Status.Queued;

					messageProcessor.ProcessMessage(incomingMessage);
					incomingMessage.Factory.Save();

					AssertEquals("Should set status as Processed.", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
					AssertEquals("No email is sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);

					parserMock.VerifyAll();
				});
			}
		}

		public void TestReceiveHDF01Result_UpdateBillStatus()
		{
			TestBillStatus(JPCustomsStatusList.Codes.AWR, JPCustomsStatusList.Codes.REG, Events.BillInformationRegistered.Code);
			TestBillStatus(JPCustomsStatusList.Codes.AWC, JPCustomsStatusList.Codes.CAN, Events.BillInformationCanceled.Code);
			TestBillStatus(JPCustomsStatusList.Codes.AWD, JPCustomsStatusList.Codes.Deleted, Events.BillInformationDeleted.Code);

			void TestBillStatus(string billStatusBefore, string billStatusNew, string eventCode)
			{
				var messageType = "HDF";
				var outputInformationCode = "*AHDF01";
				var schema = new HDF01Result();
				var testMessageData = "   HDF01*AHDF01  202311061433  XXXXX                 XXX70201@MAIL.PROD.NACCS6                                       21233784930                                                                                   1359580409                          001EP   0000000007                                                                                                    Q2           S3              0005\r\ntest";
				var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				manifestHeader.AMA_Nature = JPJobMessageTypeList.Codes.Export;
				manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Air;
				manifestHeader.AMA_JobReference = $"{messageType}123";
				var bill = manifestHeader.Bills.AddNew();
				bill.ABL_BillNumber = $"{messageType}22000002222200001";
				bill.ABL_BillStatus = billStatusBefore;

				var incomingMessage = Factory.New<EDIMessage>();
				incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
				incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
				incomingMessage.EM_MessageType = messageType;
				incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes(testMessageData);
				incomingMessage.EM_MessageNum = $"{messageType}01ABCDEFG12345678900000000002";
				incomingMessage.EM_LinkUniqueID = manifestHeader.PK;
				incomingMessage.EM_LinkedObject = manifestHeader;

				var importMAWBAndHAWBResponseMessageStub = new Mock<IResultWithOneHouseBill>();
				importMAWBAndHAWBResponseMessageStub.Setup(m => m.HouseBillNumber).Returns(bill.ABL_BillNumber);
				importMAWBAndHAWBResponseMessageStub.Setup(m => m.Schema).Returns(schema);
				parseResultStub.Setup(m => m.MessageProvider).Returns(importMAWBAndHAWBResponseMessageStub.Object);
				parseResultStub.Setup(m => m.HasResultCode).Returns(true);
				parseResultStub.Setup(m => m.IsSuccess).Returns(false);
				parseHeaderStub.Setup(m => m.OutputInformationCode).Returns(outputInformationCode);
				parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes(testMessageData))).Returns(parseResultStub.Object);

				var messageProcessor = new ManifestNACCSMessageProcessor(new LoggingInformation());
				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals(ZString.Empty, bill.ABL_BillStatus);
				var log = manifestHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.InterchangeRejected.Code)).OrderByDescending(a => a.SL_PostedTimeUtc).First();
				AssertEquals("SL_Reference", "   HDF01*AHDF01  202311061433  XXXXX                 XXX70201@MAIL.PROD.NACCS6                                       21233784930                                                                                   1359580409                          001EP   0000000007                                                                                                    Q2           S3              0005", log.SL_Reference);

				parseResultStub.Setup(m => m.IsSuccess).Returns(true);
				bill.ABL_BillStatus = billStatusBefore;
				messageProcessor.ProcessMessage(incomingMessage);
				AssertEquals(billStatusNew, bill.ABL_BillStatus);
				log = manifestHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, eventCode)).OrderByDescending(a => a.SL_PostedTimeUtc).First();
				AssertEquals("SL_Reference", "HDF01ABCDEFG12345678900000000002", log.SL_Reference);
			}
		}

		public void TestReceiveResultMessage()
		{
			AssertReceiveResultMessageFrom("HCH", "*AHCH01", new HCH01Result());
			AssertReceiveResultMessageFrom("HDF", "*AHDF01", new HDF01Result());
		}

		public void AssertReceiveResultMessageFrom(string messageType, ZString outputInformationCode, SchemaBase schema)
		{
			var testMessageData = "   HCH01AAS0180  202311061433  XXXXX                 XXX70201@MAIL.PROD.NACCS6                                       21233784930                                                                                   1359580409                          001EP   0000000007                                                                                                    Q2           S3              0005\r\ntest";
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = $"{messageType}123";
			var bill1 = manifestHeader.Bills.AddNew();
			bill1.ABL_BillNumber = $"{messageType}22000002222200001";
			var bill2 = manifestHeader.Bills.AddNew();
			bill2.ABL_BillNumber = $"{messageType}22000002222200002";

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageType = messageType;
			incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes(testMessageData);
			incomingMessage.EM_MessageNum = $"{messageType}01ABCDEFG12345678900000000002";
			incomingMessage.EM_LinkUniqueID = manifestHeader.PK;
			incomingMessage.EM_LinkedObject = manifestHeader;

			Factory.Save();

			var resultWithOneHouseBillMessageStub = new Mock<IResultWithOneHouseBill>();
			resultWithOneHouseBillMessageStub.Setup(m => m.HouseBillNumber).Returns(bill1.ABL_BillNumber);
			resultWithOneHouseBillMessageStub.Setup(m => m.Schema).Returns(schema);
			parseResultStub.Setup(m => m.MessageProvider).Returns(resultWithOneHouseBillMessageStub.Object);
			parseResultStub.Setup(m => m.HasResultCode).Returns(true);
			parseResultStub.Setup(m => m.IsSuccess).Returns(false);
			parseHeaderStub.Setup(m => m.OutputInformationCode).Returns(outputInformationCode);
			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes(testMessageData))).Returns(parseResultStub.Object);

			var messageProcessor = new ManifestNACCSMessageProcessor(new LoggingInformation());
			messageProcessor.ProcessMessage(incomingMessage);
			Factory.Save();

			if (messageType == "*AHCH01")
			{
				AssertEquals(string.Empty, bill1.ABL_BillStatus);
			}
			AssertEquals(JPMessageStatusList.Codes.Rejected, bill1.ABL_MessageStatus);
			AssertEquals(ZString.Empty, bill2.ABL_MessageStatus);
			AssertEquals(JPMessageStatusList.Codes.MultipleStatus, manifestHeader.AMA_MessageStatus);

			var log = manifestHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.InterchangeRejected.Code)).OrderByDescending(a => a.SL_PostedTimeUtc).First();
			AssertEquals("SL_Reference", testMessageData.Substring(0, 398), log.SL_Reference);

			parseResultStub.Setup(m => m.IsSuccess).Returns(true);
			messageProcessor.ProcessMessage(incomingMessage);
			Factory.Save();

			if (messageType == "*AHCH01")
			{
				AssertEquals(JPCustomsStatusList.Codes.REG, bill1.ABL_BillStatus);
			}
			AssertEquals(JPMessageStatusList.Codes.Acknowledged, bill1.ABL_MessageStatus);
			AssertEquals(ZString.Empty, bill2.ABL_MessageStatus);
			AssertEquals(JPMessageStatusList.Codes.MultipleStatus, manifestHeader.AMA_MessageStatus);

			resultWithOneHouseBillMessageStub.Setup(m => m.HouseBillNumber).Returns(bill2.ABL_BillNumber);
			parseResultStub.Setup(m => m.IsSuccess).Returns(false);
			messageProcessor.ProcessMessage(incomingMessage);
			Factory.Save();

			AssertEquals(JPMessageStatusList.Codes.Acknowledged, bill1.ABL_MessageStatus);
			AssertEquals(JPMessageStatusList.Codes.Rejected, bill2.ABL_MessageStatus);
			AssertEquals(JPMessageStatusList.Codes.MultipleStatus, manifestHeader.AMA_MessageStatus);

			parseResultStub.Setup(m => m.IsSuccess).Returns(true);
			messageProcessor.ProcessMessage(incomingMessage);
			Factory.Save();

			AssertEquals(JPMessageStatusList.Codes.Acknowledged, bill1.ABL_MessageStatus);
			AssertEquals(JPMessageStatusList.Codes.Acknowledged, bill2.ABL_MessageStatus);
			AssertEquals(JPMessageStatusList.Codes.Acknowledged, manifestHeader.AMA_MessageStatus);

			log = manifestHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.InterchangeAcknowledged.Code)).OrderByDescending(a => a.SL_PostedTimeUtc).First();
			AssertEquals("SL_Reference", testMessageData.Substring(0, 398), log.SL_Reference);
		}

		EDIMessage GetEDIMessageForTesting(AsycudaManifestHeader header, ZString testMessageData)
		{
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageType = "HCH";
			incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes(testMessageData);
			incomingMessage.EM_MessageNum = "HCH01ABCDEFG12345678900000000002";
			incomingMessage.EM_LinkUniqueID = header.PK;
			incomingMessage.EM_LinkedObject = header;

			Factory.Save();

			return incomingMessage;
		}

		public void TestCopyRecieveEventLog()
		{
			var testMessageData = "   EDASAE0LA2  202311061433  XXXXX                 XXX70201@MAIL.PROD.NACCS6                                       21233784930                                                                                   1359580409                          001EP   0000000007                                                                                                    Q2           S3              0005\r\ntest";
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "EDA123";
			var bill1 = manifestHeader.Bills.AddNew();
			bill1.ABL_BillNumber = "EDA22000002222200001";

			Factory.Save();

			parseResultStub.Setup(m => m.HasResultCode).Returns(true);
			parseResultStub.Setup(m => m.IsSuccess).Returns(true);
			parseHeaderStub.Setup(m => m.OutputInformationCode).Returns("SAE0LA2");
			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes(testMessageData))).Returns(parseResultStub.Object);

			var messageProcessor = new ManifestNACCSMessageProcessor(new LoggingInformation());

			var logs = manifestHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.CopyReceived.Code));
			AssertEquals(0, logs.Length);

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageType = "EDA";
			incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes(testMessageData);
			incomingMessage.EM_LinkUniqueID = manifestHeader.PK;
			incomingMessage.EM_LinkedObject = manifestHeader;

			messageProcessor.ProcessMessage(incomingMessage);

			incomingMessage.Factory.Save();
			manifestHeader.Reload();

			logs = manifestHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.CopyReceived.Code));
			AssertEquals(1, logs.Length);
		}

		public void TestUpdateCustomsStatusWhenReceiveHCH01Response()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = manifestHeader.Bills.AddNew();
			var bill2 = manifestHeader.Bills.AddNew();
			bill1.ABL_BillNumber = "22222000002222200001";
			bill2.ABL_BillNumber = "22222000002222200002";

			var incomingMessage = GetEDIMessageForTesting(manifestHeader, "TESTMESSAGETEXT");

			string[] houseBillNumbers = ["22222000002222200001"];

			var ccdLogs = manifestHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.CargoCheckinDiscrepancy.Code));
			var cciLogs = manifestHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.CargoCheckin.Code));
			AssertEquals(0, ccdLogs.Length);
			AssertEquals(0, cciLogs.Length);

			var responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageStub = new Mock<IResponseWithOneMasterBillAndHouseBillsInNormalRepeat>();
			responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageStub.Setup(m => m.HouseBillNumbers).Returns(houseBillNumbers);
			responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageStub.Setup(m => m.Schema).Returns(new MismatchInformation());
			parseResultStub.Setup(m => m.MessageProvider).Returns(responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageStub.Object);
			parseHeaderStub.Setup(m => m.OutputInformationCode).Returns("AAS0180");
			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes("TESTMESSAGETEXT"))).Returns(parseResultStub.Object);

			var messageProcessor = new ManifestNACCSMessageProcessor(new LoggingInformation());
			messageProcessor.ProcessMessage(incomingMessage);
			Factory.Save();

			AssertEquals(JPCustomsStatusList.Codes.Mismatch, bill1.ABL_BillStatus);
			ccdLogs = manifestHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.CargoCheckinDiscrepancy.Code));
			AssertEquals(1, ccdLogs.Length);
			AssertEquals(ZString.Empty, bill2.ABL_BillStatus);
			AssertEquals(JPCustomsStatusList.Codes.Mismatch, manifestHeader.RegistrationStatus);

			houseBillNumbers = ["22222000002222200001"];
			parseHeaderStub.Setup(m => m.OutputInformationCode).Returns("AAS0110");
			responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageStub.Setup(m => m.Schema).Returns(new NotificationOfCarryInStatus());
			responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageStub.Setup(m => m.HouseBillNumbers).Returns(houseBillNumbers);
			messageProcessor.ProcessMessage(incomingMessage);
			Factory.Save();

			AssertEquals(JPCustomsStatusList.Codes.MOV, bill1.ABL_BillStatus);
			cciLogs = manifestHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.CargoCheckinDiscrepancy.Code));
			AssertEquals(1, cciLogs.Length);
			AssertEquals(ZString.Empty, bill2.ABL_BillStatus);
			AssertEquals(JPCustomsStatusList.Codes.MultipleStatus, manifestHeader.RegistrationStatus);

			houseBillNumbers = ["22222000002222200002"];
			responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageStub.Setup(m => m.HouseBillNumbers).Returns(houseBillNumbers);
			messageProcessor.ProcessMessage(incomingMessage);
			Factory.Save();

			AssertEquals(JPCustomsStatusList.Codes.MOV, bill1.ABL_BillStatus);
			AssertEquals(JPCustomsStatusList.Codes.MOV, bill2.ABL_BillStatus);
			AssertEquals(JPCustomsStatusList.Codes.MOV, manifestHeader.RegistrationStatus);

			houseBillNumbers = ["22222000002222200001"];
			parseHeaderStub.Setup(m => m.OutputInformationCode).Returns("AAS1650");
			responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageStub.Setup(m => m.Schema).Returns(new HCH01ErrorNotificationAdvanceCargoInformationForHouseManifest());
			responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageStub.Setup(m => m.HouseBillNumbers).Returns(houseBillNumbers);
			messageProcessor.ProcessMessage(incomingMessage);
			Factory.Save();

			AssertEquals(JPCustomsStatusList.Codes.Mismatch, bill1.ABL_BillStatus);
			ccdLogs = manifestHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.CargoCheckinDiscrepancy.Code));
			AssertEquals(2, ccdLogs.Length);
		}

		public void TestHCH01_End_MasterBillCustomsStatus()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;

			using (manifestHeader.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HCH01 }))
			{
				var bill1 = manifestHeader.Bills.AddNew();
				bill1.ABL_BillNumber = "22222000002222200001";
				manifestHeader.MasterBill.ABL_BillStatus = JPMasterBillStatusList.Codes.AWE;

				var incomingMessage = GetEDIMessageForTesting(manifestHeader, "TESTMESSAGETEXT");

				var resultWithOneHouseBillStub = new Mock<IResultWithOneHouseBill>();
				resultWithOneHouseBillStub.Setup(m => m.HouseBillNumber).Returns(bill1.ABL_BillNumber);
				resultWithOneHouseBillStub.Setup(m => m.Schema).Returns(new HCH01Result());
				parseResultStub.Setup(m => m.MessageProvider).Returns(resultWithOneHouseBillStub.Object);
				parseResultStub.Setup(m => m.IsSuccess).Returns(true);
				parseHeaderStub.Setup(m => m.OutputInformationCode).Returns("*AHCH01");
				parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes("TESTMESSAGETEXT"))).Returns(parseResultStub.Object);

				var messageProcessor = new ManifestNACCSMessageProcessor(new LoggingInformation());
				messageProcessor.ProcessMessage(incomingMessage);
				Factory.Save();

				AssertEquals("Master Bill Customs status is END", JPMasterBillStatusList.Codes.END, manifestHeader.MasterBill.ABL_BillStatus);

				manifestHeader.MasterBill.ABL_BillStatus = JPMasterBillStatusList.Codes.AWE;
				parseResultStub.Setup(m => m.IsSuccess).Returns(false);

				messageProcessor = new ManifestNACCSMessageProcessor(new LoggingInformation());
				messageProcessor.ProcessMessage(incomingMessage);
				Factory.Save();

				AssertEquals("Master Bill Customs status is empty", ZString.Empty, manifestHeader.MasterBill.ABL_BillStatus);
			}
		}

		public void TestUpdateCustomsStatusWhenReceiveSNVC01()
		{
			AssertUpdateCustomsStatusWhenReceiveSNVC("NVC01", new NVC01Result());
		}

		void AssertUpdateCustomsStatusWhenReceiveSNVC(ZString outputInformationCode, SchemaBase schemaBase)
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = manifestHeader.Bills.AddNew();
			var bill2 = manifestHeader.Bills.AddNew();
			bill1.ABL_BillNumber = "22222000002222200001";
			bill2.ABL_BillNumber = "22222000002222200002";
			bill1.ABL_BillStatus = JPCustomsStatusList.Codes.AWA;
			bill2.ABL_BillStatus = JPCustomsStatusList.Codes.AWA;

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageType = "NVC";
			incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes("TESTMESSAGETEXT");
			incomingMessage.EM_MessageNum = "NVCABCDEFG12345678900000000002";
			incomingMessage.EM_LinkUniqueID = manifestHeader.PK;
			incomingMessage.EM_LinkedObject = manifestHeader;

			Factory.Save();

			string[] houseBillNumbers = { "22222000002222200001" };

			var resultWithOneMasterBillAndHouseBillsInNormalRepeatMessageStub = new Mock<IResultWithOneMasterBillAndHouseBillsInNormalRepeat>();
			resultWithOneMasterBillAndHouseBillsInNormalRepeatMessageStub.Setup(m => m.HouseBillNumbers).Returns(houseBillNumbers);
			resultWithOneMasterBillAndHouseBillsInNormalRepeatMessageStub.Setup(m => m.Schema).Returns(schemaBase);
			parseResultStub.Setup(m => m.MessageProvider).Returns(resultWithOneMasterBillAndHouseBillsInNormalRepeatMessageStub.Object);
			parseHeaderStub.Setup(m => m.OutputInformationCode).Returns(outputInformationCode);
			parseResultStub.Setup(m => m.IsSuccess).Returns(false);
			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes("TESTMESSAGETEXT"))).Returns(parseResultStub.Object);

			var messageProcessor = new ManifestNACCSMessageProcessor(new LoggingInformation());
			messageProcessor.ProcessMessage(incomingMessage);
			AssertEquals("REJ", JPMessageStatusList.Codes.Rejected, bill1.ABL_MessageStatus);
			AssertNullOrEmpty("Should be empty", bill2.ABL_MessageStatus);
			AssertEquals("Should be set to empty when failed", ZString.Empty, bill1.ABL_BillStatus);
			AssertEquals("Should not be affected as bill number does not match", JPCustomsStatusList.Codes.AWA, bill2.ABL_BillStatus);

			bill1.ABL_BillStatus = JPCustomsStatusList.Codes.AWA;
			parseResultStub.Setup(m => m.IsSuccess).Returns(true);
			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes("TESTMESSAGETEXT"))).Returns(parseResultStub.Object);
			messageProcessor = new ManifestNACCSMessageProcessor(new LoggingInformation());
			messageProcessor.ProcessMessage(incomingMessage);
			AssertEquals("ACK", JPMessageStatusList.Codes.Acknowledged, bill1.ABL_MessageStatus);
			AssertNullOrEmpty("Should be empty", bill2.ABL_MessageStatus);
			AssertEquals("Should not be affected when succeeded", JPCustomsStatusList.Codes.AWA, bill1.ABL_BillStatus);
			AssertEquals("Should not be affected as bill number does not match", JPCustomsStatusList.Codes.AWA, bill2.ABL_BillStatus);
		}

		public void TestUpdateCustomsStatusWhenReceiveHBLRegistrationInformation()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = manifestHeader.Bills.AddNew();
			var bill2 = manifestHeader.Bills.AddNew();
			bill1.ABL_BillNumber = "22222000002222200001";
			bill2.ABL_BillNumber = "22222000002222200002";

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageType = "HBL";
			incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes("TESTMESSAGETEXT");
			incomingMessage.EM_MessageNum = "HBL01ABCDEFG12345678900000000002";
			incomingMessage.EM_LinkUniqueID = manifestHeader.PK;
			incomingMessage.EM_LinkedObject = manifestHeader;

			Factory.Save();

			string[] houseBillNumbers = { "22222000002222200001" };

			var responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageSetup = new Mock<IResponseWithOneMasterBillAndHouseBillsInNormalRepeat>();
			responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageSetup.Setup(m => m.HouseBillNumbers).Returns(houseBillNumbers);
			responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageSetup.Setup(m => m.Schema).Returns(new HBLRegistrationInformation());
			parseResultStub.Setup(m => m.MessageProvider).Returns(responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageSetup.Object);
			parseHeaderStub.Setup(m => m.OutputInformationCode).Returns("SAS0711");
			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes("TESTMESSAGETEXT"))).Returns(parseResultStub.Object);

			var messageProcessor = new ManifestNACCSMessageProcessor(new LoggingInformation());
			messageProcessor.ProcessMessage(incomingMessage);
			Factory.Save();

			var logs = manifestHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BillInformationRegistered.Code)).OrderByDescending(a => a.SL_PostedTimeUtc).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("bill1.ABL_BillStatus", JPCustomsStatusList.Codes.REG, bill1.ABL_BillStatus);
				AssertEquals("bill2.ABL_BillStatus", ZString.Empty, bill2.ABL_BillStatus);
				AssertEquals("RegistrationStatus", JPCustomsStatusList.Codes.MultipleStatus, manifestHeader.RegistrationStatus);
				AssertEquals("Should log 1 BIR event", 1, logs.Length);
				AssertEquals("Reference should be message number.", incomingMessage.EM_MessageNum, logs[0].SL_Reference);
			});

			houseBillNumbers = new string[] { "22222000002222200002" };
			responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageSetup.Setup(m => m.HouseBillNumbers).Returns(houseBillNumbers);
			messageProcessor.ProcessMessage(incomingMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("bill1.ABL_BillStatus", JPCustomsStatusList.Codes.REG, bill1.ABL_BillStatus);
				AssertEquals("bill2.ABL_BillStatus", JPCustomsStatusList.Codes.REG, bill2.ABL_BillStatus);
				AssertEquals("RegistrationStatus", JPCustomsStatusList.Codes.REG, manifestHeader.RegistrationStatus);
			});
		}

		public void TestUpdateCustomsStatusWhenReceiveHBLAmendmentInformation()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = manifestHeader.Bills.AddNew();
			var bill2 = manifestHeader.Bills.AddNew();
			bill1.ABL_BillNumber = "22222000002222200001";
			bill2.ABL_BillNumber = "22222000002222200002";

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageType = "HBL";
			incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes("TESTMESSAGETEXT");
			incomingMessage.EM_MessageNum = "HBL01ABCDEFG12345678900000000002";
			incomingMessage.EM_LinkUniqueID = manifestHeader.PK;
			incomingMessage.EM_LinkedObject = manifestHeader;

			Factory.Save();

			string[] houseBillNumbers = { "22222000002222200001" };

			var responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageSetup = new Mock<IResponseWithOneMasterBillAndHouseBillsInNormalRepeat>();
			responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageSetup.Setup(m => m.HouseBillNumbers).Returns(houseBillNumbers);
			responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageSetup.Setup(m => m.Schema).Returns(new HBLAmendmentInformation());
			parseResultStub.Setup(m => m.MessageProvider).Returns(responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageSetup.Object);
			parseHeaderStub.Setup(m => m.OutputInformationCode).Returns("SAS0721");
			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes("TESTMESSAGETEXT"))).Returns(parseResultStub.Object);

			var messageProcessor = new ManifestNACCSMessageProcessor(new LoggingInformation());
			messageProcessor.ProcessMessage(incomingMessage);
			Factory.Save();

			var logs = manifestHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BillInformationAmended.Code)).OrderByDescending(a => a.SL_PostedTimeUtc).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("bill1.ABL_BillStatus", JPCustomsStatusList.Codes.AMD, bill1.ABL_BillStatus);
				AssertEquals("bill2.ABL_BillStatus", ZString.Empty, bill2.ABL_BillStatus);
				AssertEquals("RegistrationStatus", JPCustomsStatusList.Codes.AMD, manifestHeader.RegistrationStatus);
				AssertEquals("Should log 1 BIA event", 1, logs.Length);
				AssertEquals("Reference should be message number.", incomingMessage.EM_MessageNum, logs[0].SL_Reference);
			});

			houseBillNumbers = new string[] { "22222000002222200002" };
			responseWithOneMasterBillAndHouseBillsInNormalRepeatMessageSetup.Setup(m => m.HouseBillNumbers).Returns(houseBillNumbers);
			messageProcessor.ProcessMessage(incomingMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("bill1.ABL_BillStatus", JPCustomsStatusList.Codes.AMD, bill1.ABL_BillStatus);
				AssertEquals("bill2.ABL_BillStatus", JPCustomsStatusList.Codes.AMD, bill2.ABL_BillStatus);
				AssertEquals("RegistrationStatus", JPCustomsStatusList.Codes.AMD, manifestHeader.RegistrationStatus);
			});
		}

		public void TestUpdateTemporaryLandingNumberAndStatusWhenReceiveTransshipmentNoticeSubmissionInformation()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = manifestHeader.Bills.AddNew();
			var bill2 = manifestHeader.Bills.AddNew();
			bill1.ABL_BillNumber = "22222000002222200001";
			bill2.ABL_BillNumber = "22222000002222200002";

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageType = "NVC";
			incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes("TESTMESSAGETEXT");
			incomingMessage.EM_MessageNum = "NVC01ABCDEFG12345678900000000002";
			incomingMessage.EM_LinkUniqueID = manifestHeader.PK;
			incomingMessage.EM_LinkedObject = manifestHeader;

			Factory.Save();
			var responseWithOneMasterBillAndHouseBillsInDoubleRepeatMessageSetup = new Mock<IResponseWithOneMasterBillAndHouseBillsInDoubleRepeat>();
			var houseBillResponse = new HouseBillFieldsResponseRecord() { HouseBillNumber = "22222000002222200001", TemporaryLandingRegistrationNumber = "123456" };
			var houseBillResponses = new HouseBillFieldsResponse() { ResponseRecords = new List<HouseBillFieldsResponseRecord>() { houseBillResponse } };
			var houseBillNumberFields = new List<HouseBillFieldsResponse>() { houseBillResponses };
			responseWithOneMasterBillAndHouseBillsInDoubleRepeatMessageSetup.Setup(m => m.Schema).Returns(new TransshipmentNoticeSubmissionInformation());
			responseWithOneMasterBillAndHouseBillsInDoubleRepeatMessageSetup.Setup(m => m.HouseBillFields).Returns(houseBillNumberFields);
			parseResultStub.Setup(m => m.MessageProvider).Returns(responseWithOneMasterBillAndHouseBillsInDoubleRepeatMessageSetup.Object);
			parseHeaderStub.Setup(m => m.OutputInformationCode).Returns("SAS0120");
			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes("TESTMESSAGETEXT"))).Returns(parseResultStub.Object);

			var messageProcessor = new ManifestNACCSMessageProcessor(new LoggingInformation());
			messageProcessor.ProcessMessage(incomingMessage);
			CombineAssertions(() =>
			{
				AssertEquals("bill1.TemporaryLandingNumber", "123456", bill1.TemporaryLandingNumber);
				AssertEquals("bill1.TemporaryLandingStatus", TemporaryLandingStatusCodeList.Codes.REG, bill1.TemporaryLandingStatus);

				AssertEquals("bill2.TemporaryLandingNumber", ZString.Empty, bill2.TemporaryLandingNumber);
				AssertEquals("bill2.TemporaryLandingStatus", ZString.Empty, bill2.TemporaryLandingStatus);
			});

			houseBillResponse = new HouseBillFieldsResponseRecord() { HouseBillNumber = "22222000002222200002", TemporaryLandingRegistrationNumber = "654321" };
			houseBillResponses = new HouseBillFieldsResponse() { ResponseRecords = new List<HouseBillFieldsResponseRecord>() { houseBillResponse } };
			houseBillNumberFields = new List<HouseBillFieldsResponse>() { houseBillResponses };
			responseWithOneMasterBillAndHouseBillsInDoubleRepeatMessageSetup.Setup(m => m.HouseBillFields).Returns(houseBillNumberFields);
			messageProcessor.ProcessMessage(incomingMessage);
			CombineAssertions(() =>
			{
				AssertEquals("bill1.TemporaryLandingNumber", "123456", bill1.TemporaryLandingNumber);
				AssertEquals("bill1.TemporaryLandingStatus", TemporaryLandingStatusCodeList.Codes.REG, bill1.TemporaryLandingStatus);

				AssertEquals("bill2.TemporaryLandingNumber", "654321", bill2.TemporaryLandingNumber);
				AssertEquals("bill2.TemporaryLandingStatus", TemporaryLandingStatusCodeList.Codes.REG, bill2.TemporaryLandingStatus);
			});
		}

		public void TestUpdateTemporaryLandingStatusWhenReceiveTemporaryLandingCancellationInformation()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = manifestHeader.Bills.AddNew();
			var bill2 = manifestHeader.Bills.AddNew();
			bill1.ABL_BillNumber = "22222000002222200001";
			bill2.ABL_BillNumber = "22222000002222200002";

			bill1.TemporaryLandingNumber = "123456";
			bill1.TemporaryLandingStatus = TemporaryLandingStatusCodeList.Codes.REG;
			bill2.TemporaryLandingNumber = "654321";
			bill2.TemporaryLandingStatus = TemporaryLandingStatusCodeList.Codes.REG;

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageType = "NVC";
			incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes("TESTMESSAGETEXT");
			incomingMessage.EM_MessageNum = "NVC01ABCDEFG12345678900000000002";
			incomingMessage.EM_LinkUniqueID = manifestHeader.PK;
			incomingMessage.EM_LinkedObject = manifestHeader;

			Factory.Save();
			var responseWithOneMasterBillAndHouseBillsInDoubleRepeatMessageSetup = new Mock<IResponseWithOneMasterBillAndHouseBillsInNormalRepeat>();
			var houseBillNumbers = new string[] { "22222000002222200001" };
			responseWithOneMasterBillAndHouseBillsInDoubleRepeatMessageSetup.Setup(m => m.Schema).Returns(new CancellationOfTransshipmentReport());
			responseWithOneMasterBillAndHouseBillsInDoubleRepeatMessageSetup.Setup(m => m.HouseBillNumbers).Returns(houseBillNumbers);
			parseResultStub.Setup(m => m.MessageProvider).Returns(responseWithOneMasterBillAndHouseBillsInDoubleRepeatMessageSetup.Object);
			parseHeaderStub.Setup(m => m.OutputInformationCode).Returns("SAS0740");
			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes("TESTMESSAGETEXT"))).Returns(parseResultStub.Object);

			var messageProcessor = new ManifestNACCSMessageProcessor(new LoggingInformation());
			messageProcessor.ProcessMessage(incomingMessage);
			CombineAssertions(() =>
			{
				AssertEquals("bill1.TemporaryLandingNumber", "123456", bill1.TemporaryLandingNumber);
				AssertEquals("bill1.TemporaryLandingStatus", TemporaryLandingStatusCodeList.Codes.CAN, bill1.TemporaryLandingStatus);

				AssertEquals("bill2.TemporaryLandingNumber", "654321", bill2.TemporaryLandingNumber);
				AssertEquals("bill2.TemporaryLandingStatus", TemporaryLandingStatusCodeList.Codes.REG, bill2.TemporaryLandingStatus);
			});

			houseBillNumbers = ["22222000002222200002"];
			responseWithOneMasterBillAndHouseBillsInDoubleRepeatMessageSetup.Setup(m => m.HouseBillNumbers).Returns(houseBillNumbers);
			messageProcessor.ProcessMessage(incomingMessage);
			CombineAssertions(() =>
			{
				AssertEquals("bill1.TemporaryLandingNumber", "123456", bill1.TemporaryLandingNumber);
				AssertEquals("bill1.TemporaryLandingStatus", TemporaryLandingStatusCodeList.Codes.CAN, bill1.TemporaryLandingStatus);

				AssertEquals("bill2.TemporaryLandingNumber", "654321", bill2.TemporaryLandingNumber);
				AssertEquals("bill2.TemporaryLandingStatus", TemporaryLandingStatusCodeList.Codes.CAN, bill2.TemporaryLandingStatus);
			});
		}

		public void TestUpdateDataWhenReceiveHBLCancellationInformation()
		{
			var manifestHeader1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var manifestHeader2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader1.Bills.AddNew();
			manifestHeader1.AMA_MasterBill = "22222000002222200001";
			manifestHeader1.AMA_JobReference = "A123456";
			manifestHeader2.AMA_MasterBill = "22222000002222200002";
			manifestHeader2.AMA_JobReference = "B123456";

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageType = "HBL";
			incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes("TESTMESSAGETEXT");
			incomingMessage.EM_MessageNum = "HBL01ABCDEFG12345678900000000002";
			incomingMessage.EM_LinkUniqueID = manifestHeader1.PK;
			incomingMessage.EM_LinkedObject = manifestHeader1;

			Factory.Save();
			var hblCancellationInformationMessageSetup = new Mock<IHBLCancellationInformation>();
			hblCancellationInformationMessageSetup.Setup(m => m.MasterBillNumber).Returns("22222000002222200001");
			hblCancellationInformationMessageSetup.Setup(m => m.Schema).Returns(new HBLCancellationInformation());
			parseResultStub.Setup(m => m.MessageProvider).Returns(hblCancellationInformationMessageSetup.Object);
			parseHeaderStub.Setup(m => m.OutputInformationCode).Returns("SAS0731");
			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes("TESTMESSAGETEXT"))).Returns(parseResultStub.Object);

			var messageProcessor = new ManifestNACCSMessageProcessor(new LoggingInformation());
			messageProcessor.ProcessMessage(incomingMessage);

			var logs = manifestHeader1.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BillInformationDeleted.Code)).OrderByDescending(a => a.SL_PostedTimeUtc).ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("manifestHeader1.RegistrationStatus", JPCustomsStatusList.Codes.DEL, manifestHeader1.RegistrationStatus);
				AssertEquals("manifestHeader1's bill's CustomsStatus", JPCustomsStatusList.Codes.DEL, bill.ABL_BillStatus);
				AssertEquals("manifestHeader2.RegistrationStatus", ZString.Empty, manifestHeader2.RegistrationStatus);
				AssertEquals("Should log 1 BID event", 1, logs.Length);
				AssertEquals("Reference should be message number.", incomingMessage.EM_MessageNum, logs[0].SL_Reference);
			});
		}

		public void TestReceiveHDEResult()
		{
			var testMessageData = "   HDE  *AHDE    2025031816031AABC                 1AABC@MAIL.PROD.NACCS7                                                                                                                                                                                    0000000007                                                                                                                                       \r\n11111-2222-8888                                                            \r\nS3E000056201        \r\n";
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "0000000007";
			var masterBill = manifestHeader.MasterBill;
			masterBill.ABL_BillNumber = "S3E000056201";
			masterBill.ABL_BillStatus = JPCustomsStatusList.Codes.REG;

			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.JPCustoms;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EDIMessage.Status.Queued;
			incomingMessage.EM_MessageData = Encoding.ASCII.GetBytes(testMessageData);
			incomingMessage.EM_LinkUniqueID = manifestHeader.PK;
			incomingMessage.EM_LinkedObject = manifestHeader;

			Factory.Save();

			AssertEquals("REG", manifestHeader.MasterBill.ABL_BillStatus);

			var importManifestResponseMessageStub = new Mock<IResultWithOneMasterBill>();
			importManifestResponseMessageStub.Setup(m => m.MasterBillNumber).Returns(masterBill.ABL_BillNumber);
			importManifestResponseMessageStub.Setup(m => m.Schema).Returns(new HDEResult());
			parseResultStub.Setup(m => m.MessageProvider).Returns(importManifestResponseMessageStub.Object);
			parseResultStub.Setup(m => m.HasResultCode).Returns(true);
			parseResultStub.Setup(m => m.IsSuccess).Returns(false);
			parseHeaderStub.Setup(m => m.OutputInformationCode).Returns("*AHDE");
			parserMock.Setup(m => m.Parse(Encoding.ASCII.GetBytes(testMessageData))).Returns(parseResultStub.Object);

			var messageProcessor = new ManifestNACCSMessageProcessor(new LoggingInformation());
			messageProcessor.ProcessMessage(incomingMessage);

			AssertEquals("", manifestHeader.MasterBill.ABL_BillStatus);
			AssertEquals(JPMessageStatusList.Codes.Rejected, manifestHeader.MasterBill.ABL_MessageStatus);

			parseResultStub.Setup(m => m.IsSuccess).Returns(true);
			messageProcessor.ProcessMessage(incomingMessage);
			AssertEquals(JPMasterBillStatusList.Codes.END, manifestHeader.MasterBill.ABL_BillStatus);
			AssertEquals(JPMessageStatusList.Codes.Acknowledged, manifestHeader.MasterBill.ABL_MessageStatus);
		}

		string ReadTestFile(string fileName)
		{
			using var stream = GetType().Assembly.GetManifestResourceStream($"Enterprise.Customs.JP.Manifest.Business.Test.MessageProcessors.Test.{fileName}");
			return new StreamReader(stream).ReadToEnd();
		}

		protected override void SetUp()
		{
			base.SetUp();
			parserMock = new Mock<IJPInboundMessageParser>(MockBehavior.Strict);
			parseResultStub = new Mock<IJPInboundMessageParseResult>();
			parseHeaderStub = new Mock<IJPInboundMessageHeader>();
			parseResultStub.Setup(m => m.ResponseHeader).Returns(parseHeaderStub.Object);
			NACCSFactoryServiceTestHelper.SetInboundMessageParser(Factory, parserMock.Object);
		}

		Mock<IJPInboundMessageParser> parserMock;
		Mock<IJPInboundMessageParseResult> parseResultStub;
		Mock<IJPInboundMessageHeader> parseHeaderStub;
	}
}
