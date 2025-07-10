using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCSecondReturnSuccessMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCSecondReturnSuccessMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "CDE" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "SUC" };

		public void TestProcessResponseMessage_MRNUpdated()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = @"<pucomexReturn>
				<message>Operação realizada com sucesso.</message>
				<due>20BR0000274180</due>
				<ruc>0BR00000000200000000000000000020403</ruc>
				<date>2020-04-07 16:07:06</date>
				<cpf>01183367708</cpf>
			</pucomexReturn>";
			Factory.Save();

			entry.MovementReferenceNumberSetter(ZString.Empty, ZDateTime.Today);
			ExecuteMessageProcessor(responseMessage);

			AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
			AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
			AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
			AssertEquals("MRN Number updated", "20BR0000274180", entry.MovementReferenceNumber);
			AssertEquals("MRN Issue Date updated", new ZDateTime(2020, 04, 07, 16, 07, 06), entry.MovementReferenceNumberIssueDate);
		}

		public void TestProcessResponseMessage_InvalidIssueDate()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = @"<pucomexReturn>
				<message>Operação realizada com sucesso.</message>
				<due>20BR0000274180</due>
				<ruc>0BR00000000200000000000000000020403</ruc>
				<chaveDeAcesso>20RQN000613376</chaveDeAcesso>
				<date>2020-04-07 16:07:06</date>
				<cpf>01183367708</cpf>
			</pucomexReturn>";

			entry.MovementReferenceNumberSetter("", ZDateTime.Today);
			var responseSecondMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseSecondMessage.EM_MessageText = @"<pucomexReturn>
				<message>Operação realizada com sucesso.</message>
				<due>20BR0000274180</due>
				<ruc>0BR00000000200000000000000000020403</ruc>
				<chaveDeAcesso>20RQN000613376</chaveDeAcesso>
				<date>2020-30-07 16:07:06</date>
				<cpf>01183367708</cpf>
			</pucomexReturn>";
			Factory.Save();

			ExecuteMessageProcessor(responseSecondMessage);
			AssertEquals("MRN Empty Issue Date", ZDateTime.Empty, entry.MovementReferenceNumberIssueDate);
		}

		public void TestProcessResponseMessage_DifferentMRNExists()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			entry.MovementReferenceNumberSetter("20BR0000274180", ZDateTime.Today);
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = @"<pucomexReturn>
				<message>Operação realizada com sucesso.</message>
				<due>20BR0000274190</due>
				<ruc>0BR00000000200000000000000000020403</ruc>
				<chaveDeAcesso>20RQN000613378</chaveDeAcesso>
				<date>2020-05-07 16:07:06</date>
				<cpf>01183367708</cpf>
			</pucomexReturn>";
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);

			AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
			AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
			AssertEquals("Logger", "Error: \tThe MRN number 20BR0000274180 must be the same as Due number\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessage_UCRUpdated()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = @"<pucomexReturn>
				<message>Operação realizada com sucesso.</message>
				<due>20BR0000274180</due>
				<ruc>0BR00000000200000000000000000020403</ruc>
				<chaveDeAcesso>20RQN000613376</chaveDeAcesso>
				<date>2020-04-07 16:07:06</date>
				<cpf>01183367708</cpf>
			</pucomexReturn>";
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);

			AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
			AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
			AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
			AssertEquals("RUC Number updated", "0BR00000000200000000000000000020403", entry.UniqueConsignmentReference);

			responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = @"<pucomexReturn>
				<message>Operação realizada com sucesso.</message>
				<due>20BR0000274180</due>
				<ruc>0BR00000000200000000000000000020404</ruc>
				<chaveDeAcesso>20RQN000613376</chaveDeAcesso>
				<date>2020-04-07 16:07:06</date>
				<cpf>01183367708</cpf>
			</pucomexReturn>";
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);

			AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
			AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
			AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
			AssertEquals("RUC Number NOT updated", "0BR00000000200000000000000000020403", entry.UniqueConsignmentReference);
			AssertEquals("Error: \tThe UCR number 0BR00000000200000000000000000020403 must be the same as RUC number\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessage_DifferentUCRExists()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			entry.UniqueConsignmentReference = "0BR00000000200000000000000000020403";
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = @"<pucomexReturn>
				<message>Operação realizada com sucesso.</message>
				<due>20BR0000274180</due>
				<ruc>0BR00000000200000000000000000025789</ruc>
				<chaveDeAcesso>20RQN000613378</chaveDeAcesso>
				<date>2020-05-07 16:07:06</date>
				<cpf>01183367708</cpf>
			</pucomexReturn>";
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);

			AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
			AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
			AssertEquals("Logger", "Error: \tThe UCR number 0BR00000000200000000000000000020403 must be the same as RUC number\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessage_StatusAccept()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "test";
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = ExportSuccessMessageBody;
			Factory.Save();

			entry.MovementReferenceNumberSetter(ZString.Empty, ZDateTime.Today);
			ExecuteMessageProcessor(responseMessage);

			CombineAssertions(() =>
			{
				AssertEquals("CH_Status should be ACC", BRMessageStatusList.Codes.Accepted, entry.CH_Status);
				AssertEquals("CH_EntryStatus should be E10", Constants.EntryStatus.Registered, entry.CH_EntryStatus);
				AssertEquals("EM_MessageInterpretation", ExportSuccessMessagePrettyFormatterTest.GetExpectedExportSuccessHTML(EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference)), responseMessage.EM_MessageInterpretation);
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);

				var log = entry.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus);
				AssertEquals("CES Log Reference", Constants.EntryStatus.Registered, log.SL_Reference);
				AssertEquals("CES Log Event Time", new ZDateTime(2020, 04, 07, 16, 07, 06), log.SL_EventTime);
			});
		}

		public void TestProcessResponseMessageSuccess_StatusMessageFailed()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = @"<pucomexReturn></pucomexReturn>";
			Factory.Save();

			entry.MovementReferenceNumberSetter(ZString.Empty, ZDateTime.Today);
			var logger = ExecuteMessageProcessor(responseMessage);

			AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
			AssertEquals("EM_Status should be FAL", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			AssertEquals("Logger", "Error: \tMessage #1: Message deserialization was failed.\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessageSuccess_WithExceptionInDeserialization()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = @"aaaaaa";
			Factory.Save();

			entry.MovementReferenceNumberSetter(ZString.Empty, ZDateTime.Today);
			AssertExceptionThrown<InvalidOperationException>(() => { ExecuteMessageProcessor(responseMessage); });
		}

		public void TestProcessResponseMessage_AccessKeyUpdated()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = @"<pucomexReturn>
				<message>Operação realizada com sucesso.</message>
				<due>20BR0000274180</due>
				<ruc>0BR00000000200000000000000000020403</ruc>
				<chaveDeAcesso>20RQN000613376</chaveDeAcesso>
				<date>2020-04-07 16:07:06</date>
				<cpf>01183367708</cpf>
			</pucomexReturn>";
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);

			AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
			AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
			AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
			AssertEquals("Entry Access Key updated", "20RQN000613376", entry.EntryAccessKey);

			responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = @"<pucomexReturn>
				<message>Operação realizada com sucesso.</message>
				<due>20BR0000274180</due>
				<ruc>0BR00000000200000000000000000020403</ruc>
				<chaveDeAcesso>20RQN000613377</chaveDeAcesso>
				<date>2020-04-07 16:07:06</date>
				<cpf>01183367708</cpf>
			</pucomexReturn>";
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);

			AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
			AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
			AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
			AssertEquals("Entry Access Key NOT updated", "20RQN000613376", entry.EntryAccessKey);
			AssertEquals("Error: \tThe Entry Access Key 20RQN000613376 must be the same as Access Key\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessage_DifferentAccessKeyExists()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			entry.EntryAccessKey = "20RQN000613376";
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = @"<pucomexReturn>
				<message>Operação realizada com sucesso.</message>
				<due>20BR0000274180</due>
				<ruc>0BR00000000200000000000000000025789</ruc>
				<chaveDeAcesso>20RQN000613378</chaveDeAcesso>
				<date>2020-05-07 16:07:06</date>
				<cpf>01183367708</cpf>
			</pucomexReturn>";
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);

			AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
			AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
			AssertEquals("Entry Access Key NOT updated", "20RQN000613376", entry.EntryAccessKey);
			AssertEquals("Logger", "Error: \tThe Entry Access Key 20RQN000613376 must be the same as Access Key\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessage_IssueDate()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = @"<pucomexReturn>
				<message>Operação realizada com sucesso.</message>
				<due>20BR0000274180</due>
				<ruc>0BR00000000200000000000000000020403</ruc>
				<chaveDeAcesso>20RQN000613376</chaveDeAcesso>
				<date>2020-04-07 16:07:06.0</date>
				<cpf>01183367708</cpf>
			</pucomexReturn>";
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			AssertEquals("MovementReferenceNumberIssueDate should be equal", new ZDateTime(2020, 04, 07, 16, 7, 6), entry.MovementReferenceNumberIssueDate);
		}

		public void TestProcessResponseMessage_IssueDateInvalidDateFormat()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = @"<pucomexReturn>
				<message>Operação realizada com sucesso.</message>
				<due>20BR0000274180</due>
				<ruc>0BR00000000200000000000000000020403</ruc>
				<chaveDeAcesso>20RQN000613376</chaveDeAcesso>
				<date>07-04-2020 16:07:06.0</date>
				<cpf>01183367708</cpf>
			</pucomexReturn>";
			Factory.Save();

			AssertNoExceptionThrown("No exception expected", () => ExecuteMessageProcessor(responseMessage));
			Assert("MovementReferenceNumberIssueDate should be empty", entry.MovementReferenceNumberIssueDate.IsEmpty);
		}

		public static string ExportSuccessMessageBody = @"<pucomexReturn>
				<message>Operação realizada com sucesso.</message>
				<due>20BR0000274180</due>
				<ruc>0BR00000000200000000000000000020403</ruc>
				<chaveDeAcesso>20RQN000613376</chaveDeAcesso>
				<date>2020-04-07 16:07:06</date>
				<cpf>01183367708</cpf>
			</pucomexReturn>";
	}
}
