using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	public class AcdAgrResponseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessMessage()
		{
			var testBos = CreateBizObjsWithOutgoingMessage();
			var logger = new LoggingInformation();
			var testProssor = new AcdAgrResponseMessageProcessor(logger);

			var incomingInterchangeIncorrectFrom = Factory.NewWithValidTestData<EDIInterchange>();
			incomingInterchangeIncorrectFrom.EI_ApplicationCode = "CSW";
			incomingInterchangeIncorrectFrom.EI_InterchangeNum = "INT00001";
			incomingInterchangeIncorrectFrom.EI_To = "WTLDCNWZ3";
			incomingInterchangeIncorrectFrom.EI_From = "CNCustoms";
			var incomingMessageIncorrectFrom = (EDIMessage)(incomingInterchangeIncorrectFrom.ContainedMessages.FirstOrDefault() ?? incomingInterchangeIncorrectFrom.ContainedMessages.AddNew());
			incomingMessageIncorrectFrom.EM_Status = "QUE";
			incomingMessageIncorrectFrom.EM_ApplicationCode = "CSW";
			logger.ClearLogs();
			testProssor.ProcessMessage(incomingMessageIncorrectFrom);
			AssertEquals("Status: should be desarted.", "DCD", incomingMessageIncorrectFrom.EM_Status);
			Assert("Error log", logger.Logs.Any(log => log.Type == LogType.Error && log.Message.Contains("Could not find outgoing Interchange or Message for incoming Message, or incoming Message not linked to correct Business Object, InterchangeNum: INT00001")));

			var incomingInterchangeIncorrectTo = Factory.NewWithValidTestData<EDIInterchange>();
			incomingInterchangeIncorrectTo.EI_ApplicationCode = "CSW";
			incomingInterchangeIncorrectTo.EI_InterchangeNum = "INT00001";
			incomingInterchangeIncorrectTo.EI_To = "WTLDCNWZ3";
			incomingInterchangeIncorrectTo.EI_From = "CNCustoms";
			var incomingMessageIncorrectTo = (EDIMessage)(incomingInterchangeIncorrectTo.ContainedMessages.FirstOrDefault() ?? incomingInterchangeIncorrectTo.ContainedMessages.AddNew());
			incomingMessageIncorrectTo.EM_Status = "QUE";
			incomingMessageIncorrectTo.EM_ApplicationCode = "CSW";
			logger.ClearLogs();
			testProssor.ProcessMessage(incomingMessageIncorrectTo);
			AssertEquals("Status: should be desarted.", "DCD", incomingMessageIncorrectTo.EM_Status);
			Assert("Error log", logger.Logs.Any(log => log.Type == LogType.Error && log.Message.Contains("Could not find outgoing Interchange or Message for incoming Message, or incoming Message not linked to correct Business Object, InterchangeNum: INT00001")));

			var incomingMessageWithoutInterchange = Factory.NewWithValidTestData<EDIMessage>();
			incomingMessageWithoutInterchange.EM_Status = "QUE";
			incomingMessageWithoutInterchange.EM_ApplicationCode = "CSW";
			logger.ClearLogs();
			testProssor.ProcessMessage(incomingMessageWithoutInterchange);
			AssertEquals("Status: Should be processedOK", "QUE", incomingMessageWithoutInterchange.EM_Status);
			Assert("Error log for empty Interchange", logger.Logs.Any(log => log.Type == LogType.Error && log.Message.Contains("Could not find EDIInterchange for the EDIMessage, number: ")));

			var incomingInterchangeCorrect = Factory.NewWithValidTestData<EDIInterchange>();
			incomingInterchangeCorrect.EI_ApplicationCode = "CSW";
			incomingInterchangeCorrect.EI_InterchangeNum = "INT00001";
			incomingInterchangeCorrect.EI_To = "WTLDCNWZ2";
			incomingInterchangeCorrect.EI_From = "CNCustoms";
			incomingInterchangeCorrect.EI_BodyText = importAgrResponse;
			var incomingMessageCorrect = (EDIMessage)(incomingInterchangeCorrect.ContainedMessages.FirstOrDefault() ?? incomingInterchangeCorrect.ContainedMessages.AddNew());
			incomingMessageCorrect.EM_Status = "QUE";
			incomingMessageCorrect.EM_ApplicationCode = "CSW";
			incomingMessageCorrect.EM_MessageText = importAgrResponse;
			logger.ClearLogs();
			AssertNull("To make sure no NAR event is added before processing message.", testBos.EntryHeader.Logs.MostRecentLogByEventTime(Events.ChinaAgreementOfDeclarationAgentNumberReceived));
			testProssor.ProcessMessage(incomingMessageCorrect);
			AssertEquals("Status: Should be processedOK", "PRS", incomingMessageCorrect.EM_Status);
			Assert("Succeed log", logger.Logs.Any(log => log.Type == LogType.Debug && log.Message.Contains("Message processed, interchange number: INT00001")));
			AssertSame("Incoming message should have been linked to CusEntryHeader", testBos.EntryHeader, incomingMessageCorrect.EM_LinkedObject);

			var attachments = testBos.EntryInstruction.Attachments;
			attachments.Load();
			AssertEquals("Should have created a _10000001 Attachment.", true, attachments.Cast<EntryInstructionAttachment>().Any(att => att.AttachmentType == CSDDocTypeList.Codes._10000001 && att.AttachmentNumber == "20212243495988986"));
			var recentLog = testBos.EntryHeader.Logs.MostRecentLogByEventTime(Events.ChinaAgreementOfDeclarationAgentNumberReceived);
			AssertNotNull("Should have added an NAR event with ConsignNo.", recentLog);
		}

		public void TestAttachings()
		{
			var instruction = CreateBizObjsWithOutgoingMessage().EntryInstruction;
			var logger = new LoggingInformation();
			var testProssor = new AcdAgrResponseMessageProcessor(logger);

			var incomingInterchangeFailed = Factory.NewWithValidTestData<EDIInterchange>();
			incomingInterchangeFailed.EI_ApplicationCode = "CSW";
			incomingInterchangeFailed.EI_InterchangeNum = "INT00001";
			incomingInterchangeFailed.EI_To = "WTLDCNWZ2";
			incomingInterchangeFailed.EI_From = "CNCustoms";
			incomingInterchangeFailed.EI_BodyText = importAgrResponse;
			var incomingMessageFailed = (EDIMessage)(incomingInterchangeFailed.ContainedMessages.FirstOrDefault() ?? incomingInterchangeFailed.ContainedMessages.AddNew());
			incomingMessageFailed.EM_Status = "QUE";
			incomingMessageFailed.EM_ApplicationCode = "CSW";
			incomingMessageFailed.EM_MessageText = importAgrResponseFailed;
			testProssor.ProcessMessage(incomingMessageFailed);
			instruction.Attachments.Load();
			AssertEquals("Should not create Attachment for failed message.", 0, instruction.Attachments.Count);

			var incomingInterchangeSucceeded = Factory.NewWithValidTestData<EDIInterchange>();
			incomingInterchangeSucceeded.EI_ApplicationCode = "CSW";
			incomingInterchangeSucceeded.EI_InterchangeNum = "INT00001";
			incomingInterchangeSucceeded.EI_To = "WTLDCNWZ2";
			incomingInterchangeSucceeded.EI_From = "CNCustoms";
			incomingInterchangeSucceeded.EI_BodyText = importAgrResponse;
			var incomingMessageSucceeded = (EDIMessage)(incomingInterchangeSucceeded.ContainedMessages.FirstOrDefault() ?? incomingInterchangeSucceeded.ContainedMessages.AddNew());
			incomingMessageSucceeded.EM_Status = "QUE";
			incomingMessageSucceeded.EM_ApplicationCode = "CSW";
			incomingMessageSucceeded.EM_MessageText = importAgrResponse;
			testProssor.ProcessMessage(incomingMessageSucceeded);
			instruction.CusAttachments.Reload(true);
			instruction.CusStorageDocPivots.Reload(true);
			instruction.Attachments.Load();
			AssertEquals("Should have created 1 Attachment.", 1, instruction.Attachments.Count);
			AssertEquals("Should have created a _10000001 Attachment.", true, instruction.Attachments.Cast<EntryInstructionAttachment>().Any(att => att.AttachmentType == CSDDocTypeList.Codes._10000001 && att.AttachmentNumber == "20212243495988986"));

			var duplicatedInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			duplicatedInterchange.EI_ApplicationCode = "CSW";
			duplicatedInterchange.EI_InterchangeNum = "INT00001";
			duplicatedInterchange.EI_To = "WTLDCNWZ2";
			duplicatedInterchange.EI_From = "CNCustoms";
			duplicatedInterchange.EI_BodyText = importAgrResponse;
			var duplicatedInterchangeMessageCorrect = (EDIMessage)(duplicatedInterchange.ContainedMessages.FirstOrDefault() ?? duplicatedInterchange.ContainedMessages.AddNew());
			duplicatedInterchangeMessageCorrect.EM_Status = "QUE";
			duplicatedInterchangeMessageCorrect.EM_ApplicationCode = "CSW";
			duplicatedInterchangeMessageCorrect.EM_MessageText = importAgrResponse;
			testProssor.ProcessMessage(duplicatedInterchangeMessageCorrect);
			instruction.Attachments.Load();
			AssertEquals("Should not create duplicated Attachment.", 1, instruction.Attachments.Cast<EntryInstructionAttachment>().Count(att => att.AttachmentType == CSDDocTypeList.Codes._10000001 && att.AttachmentNumber == "20212243495988986"));
		}

		public void TestInterpretationSucceeded()
		{
			CreateBizObjsWithOutgoingMessage();
			var testProssor = new AcdAgrResponseMessageProcessor(new LoggingInformation());

			var incomingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			incomingInterchange.EI_ApplicationCode = "CSW";
			incomingInterchange.EI_InterchangeNum = "INT00001";
			incomingInterchange.EI_To = "WTLDCNWZ2";
			incomingInterchange.EI_From = "CNCustoms";
			incomingInterchange.EI_BodyText = importAgrResponse;
			var incomingMessage = (EDIMessage)(incomingInterchange.ContainedMessages.FirstOrDefault() ?? incomingInterchange.ContainedMessages.AddNew());
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_ApplicationCode = "CSW";
			incomingMessage.EM_MessageText = importAgrResponse;
			testProssor.ProcessMessage(incomingMessage);

			var interpretation = incomingMessage.EM_MessageInterpretation;
			AssertContains(@"<tr><td width=""75px"">响应代码</td><td>1</td></tr>", interpretation);
			AssertContains(@"<tr><td width=""75px"">响应信息</td><td>导入成功</td></tr>", interpretation);
			AssertContains(@"<tr><td width=""75px"">代理报关委托协议编号</td><td>20212243495988986</td></tr>", interpretation);
		}

		public void TestInterpretationFailed()
		{
			CreateBizObjsWithOutgoingMessage();
			var testProssor = new AcdAgrResponseMessageProcessor(new LoggingInformation());

			var incomingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			incomingInterchange.EI_ApplicationCode = "CSW";
			incomingInterchange.EI_InterchangeNum = "INT00001";
			incomingInterchange.EI_To = "WTLDCNWZ2";
			incomingInterchange.EI_From = "CNCustoms";
			incomingInterchange.EI_BodyText = importAgrResponseFailed;
			var incomingMessage = (EDIMessage)(incomingInterchange.ContainedMessages.FirstOrDefault() ?? incomingInterchange.ContainedMessages.AddNew());
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_ApplicationCode = "CSW";
			incomingMessage.EM_MessageText = importAgrResponseFailed;
			testProssor.ProcessMessage(incomingMessage);

			var interpretation = incomingMessage.EM_MessageInterpretation;
			AssertContains(@"<tr><td width=""75px"">响应代码</td><td>2</td></tr>", interpretation);
			AssertContains(@"<tr><td width=""75px"">响应信息</td><td>导入失败</td></tr>", interpretation);
			AssertContains(@"<tr><td width=""75px"">代理报关委托协议编号</td><td></td></tr>", interpretation);
		}

		public void TestSendingNotificationSucceeded()
		{
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "~~";
			broker.GS_FullName = "BROKER NAME";
			broker.GS_EmailAddress = "email1@wisetechgloabal.com";
			var testBos = CreateBizObjsWithOutgoingMessage();
			testBos.JobDeclaration.JE_DeclarationReference = "REF001";
			testBos.EntryHeader.CH_BGMReference = "BGM001";

			outgoingMessage.EM_SystemCreateUser = broker.GS_Code;
			var testProssor = new AcdAgrResponseMessageProcessor(new LoggingInformation());

			var incomingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			incomingInterchange.EI_ApplicationCode = "CSW";
			incomingInterchange.EI_InterchangeNum = "INT00001";
			incomingInterchange.EI_To = "WTLDCNWZ2";
			incomingInterchange.EI_From = "CNCustoms";
			incomingInterchange.EI_BodyText = importAgrResponse;
			var incomingMessage = (EDIMessage)(incomingInterchange.ContainedMessages.FirstOrDefault() ?? incomingInterchange.ContainedMessages.AddNew());
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_ApplicationCode = "CSW";
			incomingMessage.EM_MessageText = importAgrResponse;

			AssertEquals("To make sure no email existing before processing.", false, Env.OutgoingCustomsMailManager.EmailsCreated.Any());

			testProssor.ProcessMessage(incomingMessage);

			var savedEmails = Env.OutgoingCustomsMailManager.EmailsCreated.FindAll(email => email.Subject.StartsWith("报关单代理委托导入"));
			CombineAssertions(() =>
			{
				AssertEquals("Should create 1 email.", 1, savedEmails.Count);
				var createdEmail = savedEmails.FirstOrDefault();
				AssertEquals("Subject", "报关单代理委托导入成功: BGM001, REF001", createdEmail.Subject);
				AssertEquals("Should have 1 Recipient.", 1, createdEmail.Recipients.Count);
				AssertEquals("Should have correct Recipient.", "email1@wisetechgloabal.com", createdEmail.Recipients[0]);
				AssertContains("Email should have correct body.", incomingMessage.EM_MessageInterpretation, createdEmail.Body);
			});
		}

		public void TestSendingNotificationFailed()
		{
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "~~";
			broker.GS_FullName = "BROKER NAME";
			broker.GS_EmailAddress = "email1@wisetechgloabal.com";
			var testBos = CreateBizObjsWithOutgoingMessage();
			testBos.JobDeclaration.JE_DeclarationReference = "REF001";
			testBos.EntryHeader.CH_BGMReference = "BGM001";

			outgoingMessage.EM_SystemCreateUser = broker.GS_Code;
			var testProssor = new AcdAgrResponseMessageProcessor(new LoggingInformation());

			var incomingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			incomingInterchange.EI_ApplicationCode = "CSW";
			incomingInterchange.EI_InterchangeNum = "INT00001";
			incomingInterchange.EI_To = "WTLDCNWZ2";
			incomingInterchange.EI_From = "CNCustoms";
			incomingInterchange.EI_BodyText = importAgrResponseFailed;
			var incomingMessage = (EDIMessage)(incomingInterchange.ContainedMessages.FirstOrDefault() ?? incomingInterchange.ContainedMessages.AddNew());
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_ApplicationCode = "CSW";
			incomingMessage.EM_MessageText = importAgrResponseFailed;
			testProssor.ProcessMessage(incomingMessage);

			var savedEmails = Env.OutgoingCustomsMailManager.EmailsCreated.FindAll(email => email.Subject.StartsWith("报关单代理委托导入"));
			AssertEquals("Subject", "报关单代理委托导入失败: BGM001, REF001", savedEmails.FirstOrDefault().Subject);

			var interchangeNoConsignNo = Factory.NewWithValidTestData<EDIInterchange>();
			interchangeNoConsignNo.EI_ApplicationCode = "CSW";
			interchangeNoConsignNo.EI_InterchangeNum = "INT00001";
			interchangeNoConsignNo.EI_To = "WTLDCNWZ2";
			interchangeNoConsignNo.EI_From = "CNCustoms";
			interchangeNoConsignNo.EI_BodyText = importAgrResponseFailed.Replace("<ConsignNo></ConsignNo>", "");
			var messageNoConsignNo = (EDIMessage)(incomingInterchange.ContainedMessages.FirstOrDefault() ?? incomingInterchange.ContainedMessages.AddNew());
			messageNoConsignNo.EM_Status = "QUE";
			messageNoConsignNo.EM_ApplicationCode = "CSW";
			messageNoConsignNo.EM_MessageText = interchangeNoConsignNo.EI_BodyText;
			AssertNoExceptionThrown("Should be able to process message without ConsignNo element.", () => testProssor.ProcessMessage(messageNoConsignNo));
		}

		readonly string importAgrResponse = @"
		<ImportAgrResponse>
			<ResponseInfo>
				<ResponseCode>1</ResponseCode>
				<ResponseMessage>导入成功</ResponseMessage>
				<CopCusCode>3117980008</CopCusCode>
			</ResponseInfo>
			<ConsignNo>20212243495988986</ConsignNo>
		</ImportAgrResponse>";

		string importAgrResponseFailed => importAgrResponse
			.Replace("<ResponseCode>1</ResponseCode>", "<ResponseCode>2</ResponseCode>")
			.Replace("导入成功", "导入失败")
			.Replace("<ConsignNo>20212243495988986</ConsignNo>", "<ConsignNo></ConsignNo>");

		EDIMessage outgoingMessage;

		CNEntryHeaderTestData CreateBizObjsWithOutgoingMessage()
		{
			var testBos = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });
			var outgoingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			outgoingInterchange.EI_InterchangeNum = "INT00001";
			outgoingInterchange.EI_From = "WTLDCNWZ2";
			outgoingInterchange.EI_To = "CNCustoms";
			outgoingMessage = (EDIMessage)(outgoingInterchange.ContainedMessages.FirstOrDefault() ?? outgoingInterchange.ContainedMessages.AddNew());
			outgoingMessage.EM_LinkedObject = testBos.EntryHeader;

			return testBos;
		}
	}
}
