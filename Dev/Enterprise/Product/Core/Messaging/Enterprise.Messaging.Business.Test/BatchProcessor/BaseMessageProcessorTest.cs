using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Messaging.Business.Testing
{
	public class BaseMessageProcessorTest : TestCaseWithFactory
	{
		#region TestShouldNotLoadMessageWhenThereIsNoApplicationCodeMessageProcessor

		public void TestShouldNotLoadMessageWhenThereIsNoApplicationCodeMessageProcessor()
		{
			var message1 = CreateEDIMessage("1");
			var message2 = CreateEDIMessage("2");
			var message3 = CreateEDIMessage("3");
			Factory.Save();

			var messageProcessorMock = new Mock<BaseMessageProcessor>() { CallBase = true };
			var messageProcessor = messageProcessorMock.Object;
			messageProcessor.ExecuteBatch(CancellationToken.None);

			message1.Reload();
			message2.Reload();
			message3.Reload();

			CombineAssertions("There should be no message processed.", () =>
			{
				AssertEquals(EDIMessage.Status.Queued, message1.EM_Status);
				AssertEquals(EDIMessage.Status.Queued, message2.EM_Status);
				AssertEquals(EDIMessage.Status.Queued, message3.EM_Status);
				AssertNotContains("Failed to locate a processor for message", string.Join(",", messageProcessor.Logger.UserLogStrings.Cast<string>()));
			});
		}

		#endregion

		#region TestAlwaysGetLatestBranchesWhenRunning

		public void TestAlwaysGetLatestBranchesWhenRunning()
		{
			var factory = NewFactory();

			var company = factory.NewWithValidTestData<GlbCompany>();
			company.Branches.DeleteAll();

			var defaultBranch = company.Branches.AddNew();
			defaultBranch.FillWithValidTestData();

			var message1 = CreateEDIMessage("MSG0001", factory);
			message1.EM_GB = defaultBranch.PK;

			factory.Save();

			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				var messageProcessor = new MessageProcessorForTest();
				var query = messageProcessor.GetMessageProcessorQuery();

				factory = NewFactory();
				factory.RefreshEnabled = false;

				var messages = factory.Load<EDIMessage>(query).Select(c => c.PK);
				AssertCollectionContains("Should contains the PK of message1 as its branch is belong to the current company.", message1.PK, messages);

				var anotherFactory = NewFactory();
				anotherFactory.RefreshEnabled = false;

				var companyInAnotherFactory = anotherFactory.Load<GlbCompany>(company.PK);

				var newBranch = companyInAnotherFactory.Branches.AddNew();
				newBranch.FillWithValidTestData();

				var message2 = CreateEDIMessage("MSG0002", anotherFactory);
				message2.EM_GB = newBranch.PK;

				anotherFactory.Save();

				query = messageProcessor.GetMessageProcessorQuery();
				messages = factory.Load<EDIMessage>(query).Select(c => c.PK);
				AssertContainsExactElementsInAnyOrder("Should contains the PK of message2 as its branch is also belong to the current company.", new[] { message1.PK, message2.PK }, messages);
			}
		}

		#endregion

		#region TestCurrentCompanyNullExceptionIsReported

		public void TestCurrentCompanyNullExceptionIsReported()
		{
			var message = CreateEDIMessage("1");
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			try
			{
				using (Env.SetTemporaryUserContext(new NullCompanyUserContext(Env.CurrentUserContext)))
				{
					var messageProcessor = GetNewMessageProcessorForTest();
					AssertExceptionThrown(typeof(NullReferenceException), () => messageProcessor.Execute());
				}
				AssertContains("Current environment company PK is ", ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		class NullCompanyUserContext : UserContext, IUserContext
		{
			public NullCompanyUserContext(IUserContext currentUserContext)
			{
				this.currentUserContext = currentUserContext;
			}
			readonly IUserContext currentUserContext;

			IBranch IUserContext.Branch => currentUserContext.Branch;
			IDepartment IUserContext.Department => currentUserContext.Department;
			IUser IUserContext.User => currentUserContext.User;
			ICompany IUserContext.Company => null;
		}

		#endregion

		#region TestSaveWhenMemoryConsumptionGoesOverThreshold

		public void TestSaveWhenMemoryConsumptionGoesOverThreshold()
		{
			using (ObjectFactory.Get<Customs.Shared.ICustomsDataRegistry>().MemoryThresholdForMessageProcessing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				var message = CreateEDIMessage("1");
				message.EM_GB = GlbBranch.CurrentBranch.PK;
				var message2 = CreateEDIMessage("2");
				message.EM_GB = GlbBranch.CurrentBranch.PK;
				Factory.Save();

				var messageProcessor = GetNewMessageProcessorForTest();
				Assert("PreCondition", !messageProcessor.MessageShouldBeProcessedInASeparateFactoryExposed);
				messageProcessor.Execute();

				int countMessageProcessed = 0;
				foreach (string log in messageProcessor.Logger.UserLogStrings)
				{
					if (log.Contains("Processing Message #1"))
					{
						countMessageProcessed++;
					}

					if (countMessageProcessed > 1)
					{
						break;
					}
				}

				AssertEquals("message1 should be processed only once and saved", 1, countMessageProcessed);
				message.Reload();
				AssertEquals(EDIMessage.Status.Received, message.EM_Status);
			}
		}

		#endregion

		#region TestUniversalDataMessageProcessingBusinessFailureExceptionRetry

		public void TestUniversalDataMessageProcessingBusinessFailureExceptionRetry()
		{
			AssertEquals("Precondition: Env.OutgoingMailManager.EmailsCreated.Count", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = @"betty.boop";
			staff.GS_Code = "B.B";
			staff.GS_EmailAddress = "betty.boop@cargowise.com";
			var postMastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			staff.Groups.Add(postMastersGroup);

			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_IsActive = true;
			message.EM_MessageNum = "00001";

			Factory.Save();

			AssertEquals("message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);

			var processor = GetNewMessageProcessorForTestThrowingExceptionInProcessMessage(new MessageProcessingBusinessFailureException("Save Exception Thrown during ProcessMessage(EDIMessage message)", "Test Caption", true, "Test Log Note"));
			for (int i = 0; i < 3; i++)
			{
				processor.Execute();
				processor.Logger.Log(".");
			}

			message = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			AssertEquals("message.EM_Status", EDIMessage.Status.Failed, message.EM_Status);

			var notes = ((StmNoteCollection)message.Notes.GetAllNotes());
			AssertEquals("Should have four notes", 3, notes.Count);

			AssertLogsUsingRegex(processor.Logger, ExpectedLogTextForFailureExceptionRetry);

			foreach (StmNote note in notes)
			{
				Assert(note.ST_NoteDataAsText.Contains(@"Test Caption: Save Exception Thrown during ProcessMessage(EDIMessage message)".Trim()) ||
					note.ST_NoteDataAsText.Contains(@"Exception occurred 3 times whilst processing a message individually. The message's status has been set to 'Failed'"));
			}

			var email = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals(1, email.Count);
			AssertEquals("Error Processing Incoming EDI Message: UDM--00001", email[0].Subject);

			AssertEquals("Exception MessageProcessingBusinessFailureException is not reported to ErrorReporter.", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		protected virtual string ExpectedLogTextForFailureExceptionRetry => @"Processing Message #00001
Exception processing message #00001 individually: \[Save Exception Thrown during ProcessMessage\(EDIMessage message\)\]
.
Processing Message #00001
Exception processing message #00001 individually: \[Save Exception Thrown during ProcessMessage\(EDIMessage message\)\]
.
Processing Message #00001
Exception processing message #00001 individually: \[Save Exception Thrown during ProcessMessage\(EDIMessage message\)\]
Error Processing Incoming EDI Message: UDM--00001
Exception occurred 3 times whilst processing a message individually. The message's status has been set to 'Failed'.
Enterprise.Messaging.Integration.MessageProcessingBusinessFailureException: Save Exception Thrown during ProcessMessage\(EDIMessage message\)
\s+at Enterprise.Messaging.Business.Testing.+
\s+at Enterprise.Messaging.Business.+";

		#endregion

		#region TestUniversalDataMessageProcessingBusinessFailureExceptionNotRetry

		public void TestUniversalDataMessageProcessingBusinessFailureExceptionNotRetry()
		{
			AssertEquals("Precondition: Env.OutgoingMailManager.EmailsCreated.Count", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = @"betty.boop";
			staff.GS_Code = "B.B";
			staff.GS_EmailAddress = "betty.boop@cargowise.com";
			var postMastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			staff.Groups.Add(postMastersGroup);

			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_IsActive = true;
			message.EM_MessageNum = "00001";

			Factory.Save();

			AssertEquals("message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);

			var exceptionMessage = "Cannot Save Exception Thrown during ProcessMessage(EDIMessage message)";
			var processor = GetNewMessageProcessorForTestThrowingExceptionInProcessMessage(new MessageProcessingBusinessFailureException(exceptionMessage, "Test Caption", false, "Test Log Note"));
			processor.Execute();

			message = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			AssertEquals("message.EM_Status", EDIMessage.Status.Failed, message.EM_Status);

			var notes = ((StmNoteCollection)message.Notes.GetAllNotes());
			AssertEquals("Should have one note", 1, notes.Count);

			AssertLogsUsingRegex(processor.Logger, ExpectedLogTextForFailureExceptionNotRetry);

			var note = notes.First() as StmNote;
			AssertContains(exceptionMessage, note.ST_NoteDataAsText);

			var email = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals(1, email.Count);
			AssertEquals("Error Processing Incoming EDI Message: UDM--00001", email[0].Subject);

			AssertEquals("Exception MessageProcessingBusinessFailureException is not reported to ErrorReporter.", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		protected virtual string ExpectedLogTextForFailureExceptionNotRetry => @"Processing Message #00001
Exception processing message #00001 individually: \[Cannot Save Exception Thrown during ProcessMessage\(EDIMessage message\)\]
Error Processing Incoming EDI Message: UDM--00001
Exception occurred 1 times whilst processing a message individually. The message's status has been set to 'Failed'.
Enterprise.Messaging.Integration.MessageProcessingBusinessFailureException: Cannot Save Exception Thrown during ProcessMessage\(EDIMessage message\)
\s+at Enterprise.Messaging.Business.Testing.+
\s+at Enterprise.Messaging.Business.+";

		#endregion

		#region TestExecuteProcesses101MessagesWithoutAnyProblemsWith50RowsPerFactorySave

		public void TestExecuteProcesses101MessagesWithoutAnyProblemsWith50RowsPerFactorySave()
		{
			CreateAndSaveEDIMessage(101);
			EDIMessage notProcessMessage = CreateEDIMessage("50001");
			notProcessMessage.EM_MessageType = "XZX";
			Factory.Save();

			var messageProcessor = GetNewMessageProcessorForTest();
			messageProcessor.Execute();

			AssertEquals("messageProcessor.MessagesProcessed", 101, messageProcessor.MessagesProcessed);
			AssertEquals("messageProcessor.FactorySaveCount", ExpectedFactorySaveCountFor101Messages, messageProcessor.FactorySaveCount);
			AssertLogs(messageProcessor.Logger, ExpectedLogTextFor101Messages);
			notProcessMessage.Reload();
			AssertEquals(EDIMessage.Status.Queued, notProcessMessage.EM_Status);

			messageProcessor.MessagesProcessed = 0;
			messageProcessor.Execute();
			AssertEquals("messageProcessor.MessagesProcessed", 0, messageProcessor.MessagesProcessed);
			AssertEquals("messageProcessor.FactorySaveCount", ExpectedFactorySaveCountFor101Messages, messageProcessor.FactorySaveCount);
			AssertLogs(messageProcessor.Logger, string.Empty);
			notProcessMessage.Reload();
			AssertEquals(EDIMessage.Status.Queued, notProcessMessage.EM_Status);
		}

		protected virtual int ExpectedFactorySaveCountFor101Messages => 3;

		protected virtual string ExpectedLogTextFor101Messages => @"
Processing Message #00001
Processing Message #00002
Processing Message #00003
Processing Message #00004
Processing Message #00005
Processing Message #00006
Processing Message #00007
Processing Message #00008
Processing Message #00009
Processing Message #00010
Processing Message #00011
Processing Message #00012
Processing Message #00013
Processing Message #00014
Processing Message #00015
Processing Message #00016
Processing Message #00017
Processing Message #00018
Processing Message #00019
Processing Message #00020
Processing Message #00021
Processing Message #00022
Processing Message #00023
Processing Message #00024
Processing Message #00025
Processing Message #00026
Processing Message #00027
Processing Message #00028
Processing Message #00029
Processing Message #00030
Processing Message #00031
Processing Message #00032
Processing Message #00033
Processing Message #00034
Processing Message #00035
Processing Message #00036
Processing Message #00037
Processing Message #00038
Processing Message #00039
Processing Message #00040
Processing Message #00041
Processing Message #00042
Processing Message #00043
Processing Message #00044
Processing Message #00045
Processing Message #00046
Processing Message #00047
Processing Message #00048
Processing Message #00049
Processing Message #00050
Saving...
50 messages processed
Processing Message #00051
Processing Message #00052
Processing Message #00053
Processing Message #00054
Processing Message #00055
Processing Message #00056
Processing Message #00057
Processing Message #00058
Processing Message #00059
Processing Message #00060
Processing Message #00061
Processing Message #00062
Processing Message #00063
Processing Message #00064
Processing Message #00065
Processing Message #00066
Processing Message #00067
Processing Message #00068
Processing Message #00069
Processing Message #00070
Processing Message #00071
Processing Message #00072
Processing Message #00073
Processing Message #00074
Processing Message #00075
Processing Message #00076
Processing Message #00077
Processing Message #00078
Processing Message #00079
Processing Message #00080
Processing Message #00081
Processing Message #00082
Processing Message #00083
Processing Message #00084
Processing Message #00085
Processing Message #00086
Processing Message #00087
Processing Message #00088
Processing Message #00089
Processing Message #00090
Processing Message #00091
Processing Message #00092
Processing Message #00093
Processing Message #00094
Processing Message #00095
Processing Message #00096
Processing Message #00097
Processing Message #00098
Processing Message #00099
Processing Message #00100
Saving...
50 messages processed
Processing Message #00101
Saving...
1 message processed
";
		#endregion

		#region TestExecuteProcessesReturnsToBatchesAfterProcessOneBatchIndividually

		public void TestExecuteProcessesReturnsToBatchesAfterProcessOneBatchIndividually()
		{
			CreateAndSaveEDIMessage(110);
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, "TST");
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			query.AddToFilter(EDIMessageSchema.EM_MessageNum, "00010");
			var messageToFail = Factory.LoadTop1<EDIMessage>(query);
			var messageProcessor = GetNewMessageProcessorForTest(new ZGuid[] { messageToFail.PK });
			messageProcessor.Execute();
			AssertEquals("messageProcessor.MessagesProcessed", 160, messageProcessor.MessagesProcessed);
			AssertEquals("messageProcessor.FactorySaveCount", ExpectedFactorySaveCountForReturnsToBatches, messageProcessor.FactorySaveCount);
		}

		protected virtual int ExpectedFactorySaveCountForReturnsToBatches => 54;

		#endregion

		#region TestFirstExceptionIsLoggedButDoesntCauseTheMessageToBeRejected

		public void TestFirstExceptionIsLoggedButDoesntCauseTheMessageToBeRejected()
		{
			CreateAndSaveEDIMessage(5);

			ZQuery messagesQuery = new ZQuery();
			messagesQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, "TST");
			messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);

			EDIMessage message = Factory.LoadTop1<EDIMessage>(messagesQuery);
			AssertEquals("message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);

			var exceptionMessage = "Exception thrown during ProcessMessage(EDIMessage message)";
			var processor = GetNewMessageProcessorForTestThrowingExceptionInProcessMessage(new Exception(exceptionMessage));
			processor.Execute();

			var factory2 = new BusinessObjectFactory();
			message = factory2.Load<EDIMessage>(message.PK);
			AssertEquals(ExpectedMessageStatusFirstExceptionLogged, message.EM_Status);

			var note = message.Notes.GetAllNotes().First() as StmNote;
			AssertContains("Error was logged against the Message", exceptionMessage, note.ST_NoteDataAsText);
			AssertLogs(processor.Logger, ExpectedLogTextFirstExceptionLogged);

			var messages = factory2.Load<EDIMessage>(messagesQuery);
			AssertEquals("messages.Length", 5, messages.Length);
			AssertEquals("messages[0].EM_Status", ExpectedMessageStatusFirstExceptionLogged, messages[0].EM_Status);
			AssertEquals("messages[1].EM_Status", ExpectedMessageStatusFirstExceptionLogged, messages[1].EM_Status);
			AssertEquals("messages[2].EM_Status", ExpectedMessageStatusFirstExceptionLogged, messages[2].EM_Status);
			AssertEquals("messages[3].EM_Status", ExpectedMessageStatusFirstExceptionLogged, messages[3].EM_Status);
			AssertEquals("messages[4].EM_Status", ExpectedMessageStatusFirstExceptionLogged, messages[4].EM_Status);
		}

		protected virtual string ExpectedMessageStatusFirstExceptionLogged => EDIMessage.Status.Queued;

		protected virtual string ExpectedLogTextFirstExceptionLogged => @"
Processing Message #00001
Exception processing message #00001 individually: [Exception thrown during ProcessMessage(EDIMessage message)]
Processing Message #00002
Exception processing message #00002 individually: [Exception thrown during ProcessMessage(EDIMessage message)]
Processing Message #00003
Exception processing message #00003 individually: [Exception thrown during ProcessMessage(EDIMessage message)]
Processing Message #00004
Exception processing message #00004 individually: [Exception thrown during ProcessMessage(EDIMessage message)]
Processing Message #00005
Exception processing message #00005 individually: [Exception thrown during ProcessMessage(EDIMessage message)]
";

		#endregion

		#region TestRepeatedExceptionsOnAMessageDuringExecuteCauseFailure

		public void TestRepeatedExceptionsOnAMessageDuringExecuteCauseFailure()
		{
			AssertEquals("Precondition: Env.OutgoingMailManager.EmailsCreated.Count", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = @"betty.boop";
			staff.GS_Code = "B.B";
			staff.GS_EmailAddress = "betty.boop@cargowise.com";
			var postMastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			staff.Groups.Add(postMastersGroup);
			Factory.Save();

			CreateAndSaveEDIMessage(1);

			var message = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertEquals("message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);

			var processor = GetNewMessageProcessorForTestThrowingExceptionInProcessMessage(new Exception("Exception Thrown during ProcessMessage(EDIMessage message)"));
			var executesCount = 0;
			do
			{
				processor.Execute();
				executesCount++;
				processor.Logger.Log(".");
				message = new BusinessObjectFactory().LoadTop1<EDIMessage>(new ZQuery());
			} while (message.EM_Status != EDIMessage.Status.Failed && executesCount < 10);

			AssertEquals("message.EM_Status", EDIMessage.Status.Failed, message.EM_Status);

			AssertEquals("Env.OutgoingMailManager.EmailsCreated.Count", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("email.Recipients[0].Email", "betty.boop@cargowise.com", email.Recipients[0].Email);

			AssertEquals("ErrorReporter.LastMessageReported", "Error Processing Incoming EDI Message: TST--00001", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			var retryAttempts = eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value;
			AssertLogsUsingRegex(processor.Logger, ExpectedLogTextForRepeatedExceptions(retryAttempts));
		}

		protected virtual string ExpectedLogTextForRepeatedExceptions(int retryAttempts) => @$"Processing Message #00001
Exception processing message #00001 individually: \[Exception Thrown during ProcessMessage\(EDIMessage message\)\]{string.Concat(Enumerable.Repeat(@"
.
Processing Message #00001
Exception processing message #00001 individually: \[Exception Thrown during ProcessMessage\(EDIMessage message\)\]", retryAttempts - 1))}
Error Processing Incoming EDI Message: TST--00001
Exception occurred {retryAttempts} times whilst processing a message individually. The message's status has been set to 'Failed'.
System.Exception: Exception Thrown during ProcessMessage\(EDIMessage message\)
\s+at Enterprise.Messaging.Business.Testing.+
\s+at Enterprise.Messaging.Business.+
.";

		#endregion

		#region TestSqlLockExceptionCatched

		public void TestSqlLockExceptionCatched()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_IsActive = true;
			message.EM_MessageNum = "00001";
			Factory.Save();

			var processor = GetNewMessageProcessorForTestThrowingExceptionInProcessMessage(new SqlLockLostException(), 1);
			AssertNoExceptionThrown(delegate
			{ processor.Execute(); });
		}

		#endregion

		#region TestBatchProcessorOperationCancelledExceptionBubblesStraightOut

		public void TestBatchProcessorOperationCancelledExceptionBubblesStraightOut()
		{
			CreateAndSaveEDIMessage(1);
			var processor = GetNewMessageProcessorForTestThrowingExceptionInProcessMessage(new BatchProcessorOperationCancelledException());
			AssertExceptionThrown(typeof(BatchProcessorOperationCancelledException), delegate
			{ processor.Execute(); });
		}

		#endregion

		#region TestMessagesWithoutExceptionsWillStillGetProcessed

		public void TestMessagesWithoutExceptionsWillStillGetProcessed()
		{
			EDIMessage message1 = CreateEDIMessage("1");
			EDIMessage message2 = CreateEDIMessage("2");
			EDIMessage message3 = CreateEDIMessage("3");
			Factory.Save();

			var messageProcessor = GetNewMessageProcessorForTest(new ZGuid[] { message2.PK });
			messageProcessor.Execute();

			message1.Reload();
			message2.Reload();
			message3.Reload();

			AssertEquals("message1.EM_Status", EDIMessage.Status.Received, message1.EM_Status);
			AssertEquals("message2.EM_Status", ExpectedStatusForMessagesWithoutExceptions, message2.EM_Status);
			AssertEquals("message3.EM_Status", EDIMessage.Status.Received, message3.EM_Status);

			AssertEquals("messageProcessor.FactorySaveCount", ExpectedSaveCountForMessagesWithoutExceptions, messageProcessor.FactorySaveCount);

			AssertLogs(messageProcessor.Logger, ExpectedLogTextForMessagesWithoutExceptions);
		}

		protected virtual string ExpectedStatusForMessagesWithoutExceptions => EDIMessage.Status.Queued;
		protected virtual int ExpectedSaveCountForMessagesWithoutExceptions => 5;

#if NETFRAMEWORK
		protected virtual string ExpectedLogTextForMessagesWithoutExceptions => @"
Processing Message #1
Processing Message #2
Processing Message #3
Saving...
Exception processing a group of 3 messages: [sender cannot be null or empty.
Parameter name: sender]
Processing Message #1
Saving...
1 message processed
Processing Message #2
Saving...
Exception processing message #2 individually: [sender cannot be null or empty.
Parameter name: sender]
Processing Message #3
Saving...
1 message processed
";
#else
		protected virtual string ExpectedLogTextForMessagesWithoutExceptions => @"
Processing Message #1
Processing Message #2
Processing Message #3
Saving...
Exception processing a group of 3 messages: [sender cannot be null or empty. (Parameter 'sender')]
Processing Message #1
Saving...
1 message processed
Processing Message #2
Saving...
Exception processing message #2 individually: [sender cannot be null or empty. (Parameter 'sender')]
Processing Message #3
Saving...
1 message processed
";
#endif

		#endregion

		#region TestMessagesWithoutExceptionsWillStillGetProcessedInASeparateFactory

		public void TestMessagesWithoutExceptionsWillStillGetProcessedInASeparateFactory()
		{
			EDIMessage message1 = CreateEDIMessage("1");
			EDIMessage message2 = CreateEDIMessage("2");
			EDIMessage message3 = CreateEDIMessage("3");
			Factory.Save();

			var messageProcessor = GetMessageProcessorForTestProcessInNewFactory(new ZGuid[] { message2.PK });
			messageProcessor.Execute();

			message1.Reload();
			message2.Reload();
			message3.Reload();

			AssertEquals("message1.EM_Status", EDIMessage.Status.Received, message1.EM_Status);
			AssertEquals("message2.EM_Status", ExpectedStatusForMessagesWithoutExceptions, message2.EM_Status);
			AssertEquals("message3.EM_Status", EDIMessage.Status.Received, message3.EM_Status);

			AssertEquals("messageProcessor.FactorySaveCount", ExpectedSaveCountForMessagesWithoutExceptionsInNewFactory, messageProcessor.FactorySaveCount);

			AssertLogs(messageProcessor.Logger, ExpectedLogTextForMessagesWithoutExceptionsInNewFactory);
		}

		protected virtual int ExpectedSaveCountForMessagesWithoutExceptionsInNewFactory => 4;

#if NETFRAMEWORK
		protected virtual string ExpectedLogTextForMessagesWithoutExceptionsInNewFactory => @"
Processing Message #1
Saving...
1 message processed
Processing Message #2
Saving...
Exception processing message #2 individually: [sender cannot be null or empty.
Parameter name: sender]
Processing Message #3
Saving...
1 message processed
";
#else
		protected virtual string ExpectedLogTextForMessagesWithoutExceptionsInNewFactory => @"
Processing Message #1
Saving...
1 message processed
Processing Message #2
Saving...
Exception processing message #2 individually: [sender cannot be null or empty. (Parameter 'sender')]
Processing Message #3
Saving...
1 message processed
";

#endif

		#endregion

		#region TestBranchesAreNotLazyLoadedBecauseServiceTasksItterateCompanies

		public void TestBranchesAreNotLazyLoadedBecauseServiceTasksItterateCompanies()
		{
			var alternativeBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK) { OrderBy = GlbBranchSchema.GB_Code.Name });
			var message = CreateEDIMessage("1");
			message.EM_GB = alternativeBranch.PK;
			Factory.Save();

			var messageProcessor = GetNewMessageProcessorForTest();
			messageProcessor.Execute();
			AssertEquals(0, messageProcessor.MessagesProcessed);

			using (DisposableEnvironment.ForBranch(alternativeBranch.PK.ToGuid()))
			{
				messageProcessor.Execute();
				AssertEquals(1, messageProcessor.MessagesProcessed);
			}
		}

		#endregion

		#region TestProcessorExecutingReportsErrorWhenExceptionHappensDuringProcessing

		public void TestProcessorExecutingReportsErrorWhenExceptionHappensDuringProcessing()
		{
			var message_1 = Factory.NewWithValidTestData<EDIMessage>();
			message_1.EM_MessageText = "";
			message_1.EM_Status = EDIMessage.Status.Queued;
			message_1.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message_1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message_1.EM_IsActive = true;
			message_1.EM_MessageNum = "00001";
			var message_2 = Factory.NewWithValidTestData<EDIMessage>();
			message_2.EM_MessageText = "";
			message_2.EM_Status = EDIMessage.Status.Queued;
			message_2.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			message_2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message_2.EM_IsActive = true;
			message_2.EM_MessageNum = "00002";
			var testMailNotificationUser = Factory.New<GlbStaff>();
			testMailNotificationUser.GS_Code = "GRP";
			testMailNotificationUser.GS_LoginName = "GRP";
			testMailNotificationUser.GS_IsSystemAccount = false;
			testMailNotificationUser.GS_EmailAddress = "user@group.com";
			var testMailNotificationGroup = Factory.New<GlbGroup>();
			testMailNotificationGroup.GG_Code = "eHubGroup";
			var groupLink = Factory.New<GlbGroupLink>();
			groupLink.GK_GG = testMailNotificationGroup.PK;
			groupLink.GK_GS = testMailNotificationUser.PK;
			RawDataRegistry.Instance.NotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testMailNotificationGroup.PK.ToGuid());
			Factory.Save();

			var precheckedMails = Factory.Load<MailItem>(new ZQuery());

			AssertEquals("PRE: Should contain no email notification initially", 0, precheckedMails.Length);
			AssertEquals("PRE: message_1.EM_Status", EDIMessage.Status.Queued, message_1.EM_Status);
			AssertEquals("PRE: message_2.EM_Status", EDIMessage.Status.Queued, message_2.EM_Status);
			AssertEquals("PRE: message_1.NoteCount", 0, message_1.GetNotes().GetAllNotes().Count);
			AssertEquals("PRE: message_2.NoteCount", 0, message_2.GetNotes().GetAllNotes().Count);

			var processor = GetNewMessageProcessorForTestThrowingExceptionInProcessMessage(new Exception("Exception Thrown during ProcessMessage(EDIMessage message)"), 1);
			var logLines = new System.Text.StringBuilder();
			processor.Logger.OnLogInfoAdded += (string logMsg, LogType logType) => { logLines.AppendLine(string.Format("{0}|{1}", logType, logMsg.Substring(1))); };

			processor.Execute();

			var reloadFactory = new BusinessObjectFactory();
			var reloadMessage_1 = reloadFactory.Load<EDIMessage>(message_1.PK);
			var reloadMessage_2 = reloadFactory.Load<EDIMessage>(message_2.PK);
			var reloadMailItems = reloadFactory.Load<MailItem>(new ZQuery());

			var expectedLogTextWithType = @"Information|Processing Message #00001
Warning|Exception processing a group of 2 messages: \[Exception Thrown during ProcessMessage\(EDIMessage message\)\]
Information|Processing Message #00001
Warning|Exception processing message #00001 individually: \[Exception Thrown during ProcessMessage\(EDIMessage message\)\]
Error|Error Processing Incoming EDI Message: UDM--00001
Exception occurred 1 times whilst processing a message individually. The message's status has been set to 'Failed'.
System.Exception
\s+at Enterprise.Messaging.Business.Testing.+
\s+at Enterprise.Messaging.Business.+
Information|Processing Message #00002
Warning|Exception processing message #00002 individually: \[Exception Thrown during ProcessMessage\(EDIMessage message\)\]
Error|Error Processing Incoming EDI Message: UDM--00002
Exception occurred 1 times whilst processing a message individually. The message's status has been set to 'Failed'.
System.Exception: Exception Thrown during ProcessMessage\(EDIMessage message\)
\s+at Enterprise.Messaging.Business.Testing.+
\s+at Enterprise.Messaging.Business.+
.";

			CombineAssertions(() =>
			{
				foreach (var mailitem in reloadMailItems)
				{
					AssertEquals("Incorrect Mail Notification recipient", "user@group.com", mailitem.AllRecipients);
					AssertMatch("Incorrect Mail Notification subject", new Regex(@"Error Processing Incoming EDI Message: UDM--0000\d"), mailitem.MI_Subject);
					AssertMatch("Incorrect Mail Notification body", new Regex(@"Exception occurred 1 times whilst processing a message individually. The message's status has been set to 'Failed'.\s+System.Exception: Exception Thrown during ProcessMessage\(EDIMessage message\).+"), mailitem.BodyTextDecoded);
				}
				AssertEquals("Incorrect Number of EMail notification", 2, reloadMailItems.Length);
				var messageNotes_1 = reloadMessage_1.GetNotes().GetAllNotes();
				var messageNotes_2 = reloadMessage_2.GetNotes().GetAllNotes();
				AssertEquals("message_1.EM_Status", EDIMessage.Status.Failed, reloadMessage_1.EM_Status);
				AssertEquals("message_1.NoteCount", 1, messageNotes_1.Count);
				AssertEquals("message_1.NoteDescription", PredefinedNoteTypes.Instance.DataImportLogNote.Description, messageNotes_1.ToArray<StmNote>()[0].ST_Description);
				AssertMatch("message_1.NoteText", new Regex(@"Exception occurred 1 times whilst processing a message individually. The message's status has been set to 'Failed'.\s+System.Exception"), messageNotes_1.ToArray<StmNote>()[0].ST_NoteText);
				AssertEquals("message_2.EM_Status", EDIMessage.Status.Failed, reloadMessage_2.EM_Status);
				AssertEquals("message_2.NoteCount", 1, messageNotes_2.Count);
				AssertEquals("message_2.NoteDescription", PredefinedNoteTypes.Instance.DataImportLogNote.Description, messageNotes_2.ToArray<StmNote>()[0].ST_Description);
				AssertMatch("message_2.NoteText", new Regex(@"Exception occurred 1 times whilst processing a message individually. The message's status has been set to 'Failed'.\s+System.Exception"), messageNotes_2.ToArray<StmNote>()[0].ST_NoteText);
				AssertMatch("ErrorReporter.LastMessageReported", new Regex(@"Error Processing Incoming EDI Message: UDM--0000\d"), ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
				AssertMatch("Logger", new Regex(expectedLogTextWithType), logLines.ToString());
			});
		}

		#endregion

		#region TestErrorWhileSavingSingleMessageAsGroup

		public void TestErrorWhileSavingSingleMessageAsGroup()
		{
			CreateAndSaveEDIMessage(1);
			var messagesQuery = new ZQuery();
			messagesQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, "TST");
			messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			var message = Factory.LoadTop1<EDIMessage>(messagesQuery);

			var processor = GetMessageProcessorForTestExceptionOnSave();
			processor.Execute();

			AssertNotNull("Exception was reported", ErrorReporter.LastExceptionReported);
			AssertEquals("Exception was thrown on save", MessageProcessorForTestExceptionOnSave.ErrorMessage, ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();

			var messageAfterProcessing = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			AssertEquals("FAL", messageAfterProcessing.EM_Status);
		}

		#endregion

		#region TestMessageProcessorIsOnlyQueriedOncePerMessage

		public void TestMessageProcessorIsOnlyQueriedOncePerMessage()
		{
			EDIMessage message1 = CreateEDIMessage("1");
			EDIMessage message2 = CreateEDIMessage("2");
			EDIMessage message3 = CreateEDIMessage("3");
			Factory.Save();

			var messageProcessor = GetNewMessageProcessorForTest();
			messageProcessor.Execute();

			AssertEquals("3 messages + bracketing query lookups", 5, messageProcessor.ApplicationTypeProcessorFindCount);
		}

		#endregion

		#region TestOrderAndHint

		public void TestOrderAndHint()
		{
			using (var processor = new MessageProcessorForHintTest())
			{
				AssertEquals(EDIMessageOrder.Number, processor.GetDefaultMessageOrder());

				processor.SetMessageOrder(EDIMessageOrder.Number);
				processor.ExecuteBatch();

				var query = processor.GetExecutedEDIMessageQueries().Single();
				AssertContains("ORDER BY EM_MessageNum, EM_SystemCreateTimeUtc", query);
				AssertContains("WITH (INDEX(NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc))", query);
			}

			using (var processor = new MessageProcessorForHintTest())
			{
				AssertEquals(EDIMessageOrder.Number, processor.GetDefaultMessageOrder());

				processor.SetMessageOrder(EDIMessageOrder.CreateTime);
				processor.ExecuteBatch();

				var query = processor.GetExecutedEDIMessageQueries().Single();
				AssertContains("ORDER BY EM_SystemCreateTimeUtc, EM_MessageNum", query);
				AssertContains("WITH (INDEX(NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_SystemCreateTimeUtc_EM_MessageNum))", query);
			}
		}

		#endregion

		#region TestPostProcessOnExceptionExecuted
		public void TestPostProcessOnExceptionExecuted()
		{
			var exceptionBeingThrown = new Exception("Exception for checking if PostProcessOnException is executed.");
			var processor = new MessageProcessorForTestThrowingExceptionInProcessMessage(exceptionBeingThrown);
			AssertPostProcessOnException(exceptionBeingThrown, processor, 1, exceptionBeingThrown);
		}

		public void TestPostProcessOnExceptionNotExecuted()
		{
			var exceptionNotToBeThrown = new Exception("Exception for checking if PostProcessOnException is executed.");
			var processor = new MessageProcessorForTest();
			AssertPostProcessOnException(exceptionNotToBeThrown, processor, 0, null);
		}

		void AssertPostProcessOnException(Exception thrownException, MessageProcessorForTest processor, int expectedCount, Exception expectedException) => CombineAssertions(() =>
		{
			var retryAttempts = eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value;

			CreateEDIMessage("001");
			Factory.Save();

			var applicationProcessor = processor.ApplicationTypeProcessor;
			for (var i = 0; i < retryAttempts; i++)
			{
				AssertEquals("MessagesPostProcessOnException count", 0, applicationProcessor?.PostProcessOnExceptionCount);
				processor.Execute();
			}

			AssertEquals("LastExceptionReported", expectedException, ErrorReporter.LastExceptionReported);
			AssertEquals("MessagesPostProcessOnException count", expectedCount, applicationProcessor?.PostProcessOnExceptionCount);
			ErrorReporter.Clear();
		});

		#endregion

		public void TestGetMessageProcessors()
		{
			AssertEquals("No default MessageProcessors", 0, (new MessageProcessorForTest()).BaseGetMessageProcessorsForTest().Count);
		}

		public void TestReportOpeningTransactionInformationWhenThrowLockRequestTimeOutException()
		{
			var retryAttempts = eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value;
			Assert(!RetryMessageProcessorOnSqlException(1222, "Lock request time out period exceeded.", retryAttempts + 1, retryAttempts, DbErrorType.LockTimeoutExpired));
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestMessageProcessorDoesNotErrorReport_WhenThrowInsertConflictedWithForeignKeyException()
		{
			var retryAttempts = eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value;
			Assert(!RetryMessageProcessorOnSqlException(547, "The INSERT statement conflicted with the FOREIGN KEY constraint \"ProcessHeader_FH_P0_Template_FK2_ProcessTaskTemplate_RRR_120N\".", retryAttempts + 1, retryAttempts, DbErrorType.InsertConflictedWithForeignKey));
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestMessageProcessorDoesNotErrorReport_WhenThrowUpdateConflictedWithForeignKeyException()
		{
			var retryAttempts = eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value;
			Assert(!RetryMessageProcessorOnSqlException(547, "The Update statement conflicted with the FOREIGN KEY constraint \"ProcessHeader_FH_P0_Template_FK2_ProcessTaskTemplate_RRR_120N\".", retryAttempts + 1, retryAttempts, DbErrorType.UpdateConflictedWithForeignKey));
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestMessageProcessorDoesErrorReport_AndDoesNotRetry_WhenConversionExceptionIsThrown()
		{
			using (new DisposableAction(() => ExceptionReporterTestListener.Instance.Clear()))
			{
				Assert(!RetryMessageProcessorOnSqlException(8114, "Error converting data type numeric to decimal.", 1, eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value, DbErrorType.CannotConvertDataType));
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			}
		}

		public void TestMessageProcessorDoesErrorReport_AndDoesNotRetry_WhenArithmeticOverflowExceptionIsThrown()
		{
			using (new DisposableAction(() => ExceptionReporterTestListener.Instance.Clear()))
			{
				Assert(!RetryMessageProcessorOnSqlException(8115, "Arithmetic overflow error converting numeric to data type numeric.", 1, eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value, DbErrorType.ArithmeticOverflowConvertingToDataType));
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			}
		}

		#region Implementation

		protected virtual IMessageProcessorForTest GetNewMessageProcessorForTest(ZGuid[] messagePKsToFailOn = null)
		{
			return new MessageProcessorForTest(messagePKsToFailOn);
		}

		protected virtual IMessageProcessorForTest GetNewMessageProcessorForTestThrowingExceptionInProcessMessage(Exception exceptionToThrow, int maxRetries = 0)
		{
			return new MessageProcessorForTestThrowingExceptionInProcessMessage(exceptionToThrow, maxRetries);
		}

		protected virtual IMessageProcessorForTest GetMessageProcessorForTestExceptionOnSave()
		{
			return new MessageProcessorForTestExceptionOnSave();
		}

		protected virtual IMessageProcessorForTest GetMessageProcessorForTestProcessInNewFactory(ZGuid[] messagePKsToFailOn = null)
		{
			return new MessageProcessorForTestProcessInNewFactory(messagePKsToFailOn);
		}

		protected static void AssertLogs(LoggingInformation logger, string expectedResult)
		{
			var actualResults = new ZStringBuilder();
			foreach (string line in logger.UserLogStrings)
			{
				if (line.Length > 1)
				{
					actualResults.Append(line.Substring(1));
				}
			}

			var actualResult = actualResults.ToStringWithNewLineBetweenAppends();
			AssertMultilineASCIIEquals("Debug Log from MessageProcessor", expectedResult.Trim(), actualResult);
			logger.ClearLogs();
		}

		static void AssertLogsUsingRegex(LoggingInformation logger, string expectedResultExpression)
		{
			var actualResults = new ZStringBuilder();
			foreach (string line in logger.UserLogStrings)
			{
				if (line.Length > 1)
				{
					actualResults.Append(line.Substring(1));
				}
			}

			var actualResult = actualResults.ToStringWithNewLineBetweenAppends();
			AssertMatch("Debug Log from MessageProcessor", new Regex("^" + expectedResultExpression, RegexOptions.CultureInvariant), actualResult);
			logger.ClearLogs();
		}

		bool RetryMessageProcessorOnSqlException(int errorNumber, string errorMessage, int currentExceptionsCount, int retryAttempts, DbErrorType dbErrorType)
		{
			var messageProcessorForTest = new MessageProcessorForTest();
			var sqlException = SqlExceptionBuilder.CreateSqlException(errorNumber, errorMessage);
			AssertEquals("Precondition", dbErrorType, new DbErrorMatch(sqlException).ExceptionType);

			return messageProcessorForTest.ShouldRetryOnException(currentExceptionsCount, CreateEDIMessage("1"), sqlException, retryAttempts);
		}

		void CreateAndSaveEDIMessage(int quantity)
		{
			for (int index = 0; index < quantity; index++)
			{
				CreateEDIMessage((quantity - index).ToString().PadLeft(5, '0'));
			}

			Factory.Save();
		}

		protected EDIMessage CreateEDIMessage(string messageNumber)
		{
			return CreateEDIMessage(messageNumber, Factory);
		}

		EDIMessage CreateEDIMessage(string messageNumber, BusinessObjectFactory factory)
		{
			var message = factory.NewWithValidTestData<EDIMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = messageNumber;
			message.EM_ApplicationCode = "TST";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_IsActive = true;
			return message;
		}

		class MessageProcessorForTestThrowingExceptionInProcessMessage : MessageProcessorForTest
		{
			public MessageProcessorForTestThrowingExceptionInProcessMessage(Exception exceptionToThrow, int maxRetries = 0)
			{
				this.exceptionToThrow = exceptionToThrow;
				this.maxRetries = maxRetries;
			}

			readonly Exception exceptionToThrow;
			readonly int maxRetries;

			protected override void ProcessMessageCore(ApplicationTypeMessageProcessor processor, EDIMessage message)
			{
				Logger.Log("Processing Message #" + message.EM_MessageNum);
				throw exceptionToThrow;
			}

			protected override bool ShouldRetryOnExceptionCore(int currentExceptionsCount, EDIMessage message, Exception lastException, int retryAttempts, string additionalErrorReportMessage = null)
			{
				return maxRetries > 0
					? currentExceptionsCount < maxRetries
					: base.ShouldRetryOnExceptionCore(currentExceptionsCount, message, lastException, retryAttempts, additionalErrorReportMessage);
			}
		}

		class MessageProcessorForTestExceptionOnSave : MessageProcessorForTest
		{
			public MessageProcessorForTestExceptionOnSave()
				: base()
			{
			}

			public const string ErrorMessage = "Some error happened during saving";
			public int SaveCount { get; set; }

			protected override void SaveAfterProcessingMessagesCore(BusinessObjectFactory factory)
			{
				SaveCount++;
				if (SaveCount == 1)
				{
					throw new Exception(ErrorMessage);
				}
				else
				{
					base.SaveAfterProcessingMessagesCore(factory);
				}
			}

			protected override bool ShouldRetryOnExceptionCore(int currentExceptionsCount, EDIMessage message, Exception lastException, int retryAttempts, string additionalErrorReportMessage = null)
			{
				return false;
			}
		}

		class MessageProcessorForTestProcessInNewFactory : MessageProcessorForTest
		{
			public MessageProcessorForTestProcessInNewFactory(ZGuid[] messagePKsToFailProcessingOn)
				: base(messagePKsToFailProcessingOn)
			{
			}

			protected override bool MessageShouldBeProcessedInASeparateFactory => true;
		}

		#endregion
	}
}
