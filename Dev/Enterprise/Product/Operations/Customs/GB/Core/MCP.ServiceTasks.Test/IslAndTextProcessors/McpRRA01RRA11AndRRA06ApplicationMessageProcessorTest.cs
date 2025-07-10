using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.MCP.ServiceTasks.RRA01AndRRA11;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq.Protected;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.MCP.ServiceTasks.IslAndTextProcessors.Testing
{
	public class McpRRA01RRA11AndRRA06ApplicationMessageProcessorTest : TestCaseWithFactory
	{
		public void TestLoggerInfoLinkedToServiceLogger()
		{
			var msg = "Like the jewel of the crown, the precious stone glittered in the queen's round metal hat";
			var logInfo = new LoggingInformation();
			var serviceLogger = new TestServiceLogger();
			var proc = new McpRRA01RRA11AndRRA06ApplicationMessageProcessorForTest(logInfo, serviceLogger);

			proc.WriteToLog(msg);

			AssertContains("Should contain message", msg, serviceLogger.ToString());
		}

		public void TestEmailResponseProcessor_GenerateEmail()
		{
			var expectedMessage1 = "Unknown entry email generated";
			var expectedMessage2 = "Email sent to PostMasterGroup";
			var expectedMessage3 = "Release/Removal Advice email generated";

			AssertProcessorLogs(CreateMessage(RRA01_NotMatchingMessage), new string[] { expectedMessage1, expectedMessage2 });
			AssertProcessorLogs(CreateMessage(RRA01_MatchingMessage), new string[] { expectedMessage3 });
		}

		public void TestEmailResponseProcessor_SetUserForThisJob()
		{
			var expectedMessage1 = "User for job set to: DJC";
			var expectedMessage2 = "No user for job found";

			AssertProcessorLogs(CreateMessage(RRA01_MatchingMessage), new string[] { expectedMessage1 });
			AssertProcessorLogs(CreateMessage(RRA01_MatchingMessageNoUser), new string[] { expectedMessage2 });
		}

		public void TestEmailResponseProcessor_QueueEmail()
		{
			var expectedMessage1 = "Notification Code: RRA01 Item: CustomsResponseNotificationsToGroupMCPRRA01 Response Notifications: ESG";
			var expectedMessage2 = "Sending notification to user email:";
			var expectedMessage3 = "Email sent to Mcp notification group";

			AssertProcessorLogs(CreateMessage(RRA01_MatchingMessage), new string[] { expectedMessage1, expectedMessage2 });
			AssertProcessorLogs(CreateMessage(RRA01_MatchingMessageNoUser), new string[] { expectedMessage1, expectedMessage3 });
		}

		EDIMessage CreateMessage(string msgText)
		{
			var message = Factory.New<EDIMessage>();

			message.EM_MessageText = msgText;

			return message;
		}

		void AssertProcessorLogs(EDIMessage message, IEnumerable<string> expectedLogs)
		{
			var logInfo = new LoggingInformation();
			var serviceLogger = new TestServiceLogger();
			var proc = new McpRRA01RRA11AndRRA06ApplicationMessageProcessorForTest(logInfo, serviceLogger);

			proc.ProcessMessage(message);

			var actualLogs = serviceLogger.ToString();

			CombineAssertions(() =>
			{
				foreach (var expected in expectedLogs)
				{
					AssertContains(expected, actualLogs);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			SetUpStaffAccounts();

			CreateDeclarationAndEntry("341560045", "B000069", "071-001006L", true);
			CreateDeclarationAndEntry("341560046", "B000070", "071-001007L", false);

			Factory.Save();
		}

		void CreateDeclarationAndEntry(string masterUCR, string job, string entryNum, bool addUser)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_MasterUCR = masterUCR;
			declaration.JE_DeclarationReference = job;
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();

			cusEntryHeader.EntryNumber = entryNum;
			cusEntryHeader.CusEntryNumber.CE_IssueDate = new ZDateTime(2013, 1, 31, 23, 59, 00);
			cusEntryHeader.CH_EntryStatus = "ABC";

			if (addUser)
			{
				var mockMessage = Factory.NewMoq<EDIMessageDummyForTest_123>();
				mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("123");
				var outMsg = mockMessage.Object;
				outMsg.EM_ApplicationReference = "XXX";
				outMsg.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				outMsg.EM_MessageNum = "123";
				outMsg.EM_SystemCreateUser = "DJC";
				cusEntryHeader.Messages.Add(outMsg);
			}
		}

		void SetUpStaffAccounts()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "DJC";
			staff.GS_IsSystemAccount = false;
			staff.GS_EmailAddress = "foo@bar.com";
			staff.Groups.Add(Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup));
			Factory.Save();

			var staffSystemAccount = Factory.New<GlbStaff>();
			staffSystemAccount.GS_Code = "AAA";
			staffSystemAccount.GS_IsSystemAccount = true;
			staffSystemAccount.GS_LoginName = "AAA";
			staffSystemAccount.GS_EmailAddress = "AAA@bar.com";
			staffSystemAccount.Groups.Add(Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup));
			Factory.Save();
		}

		const string RRA01_MatchingMessage = "=RRA01~TTY~34156004500000~CSLU1137752 ~         ~         ~                                        ~01216~00014751~0809021718~YTNF057108~D02~MDB                 ~YTNF057108                         }";
		const string RRA01_MatchingMessageNoUser = "=RRA01~TTY~34156004600000~CSLU1137753 ~         ~         ~                                        ~01216~00014751~0809021718~YTNF057108~D02~MDB                 ~YTNF057108                         }";
		const string RRA01_NotMatchingMessage = "=RRA01~TTY~34156009900000~CSLU1137752 ~         ~         ~                                        ~01216~00014751~0809021718~YTNF057108~D02~MDB                 ~YTNF057108                         }";

		class McpRRA01RRA11AndRRA06ApplicationMessageProcessorForTest : McpRRA01RRA11AndRRA06ApplicationMessageProcessor, IEDocsDelayedSaver
		{
			public McpRRA01RRA11AndRRA06ApplicationMessageProcessorForTest(LoggingInformation logger, ILogger serviceLogger) : base(logger, serviceLogger, new McpRRA01RRA11AndRRA06MessageProcessor(serviceLogger))
			{
			}

			public void WriteToLog(string msg)
			{
				Logger.DebugLog(msg);
			}

			void IEDocsDelayedSaver.QueueForSaving(DocManagerInfo docManagerInfo) { }
		}
	}

	public class EDIMessageDummyForTest_123 : EDIMessage
	{
		public EDIMessageDummyForTest_123(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override string GetMessageReferenceNumber()
		{
			return "123";
		}
	}
}
