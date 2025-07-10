using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.Scheduler.GraphEngine;
using Enterprise.Scheduler.GraphEngine.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.UniversalDataBuss.ServiceTasks.Testing.Processing
{
	[UseSnapshotProtection]
	class ParallelServiceTaskTest : DbErrorHandlerNonTransactionedTest
	{
		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			eAdaptorRegistry.Instance.AllowParallelUMI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.ParallelUMIQueueHistoryInHours.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 24);
			Factory = new BusinessObjectFactory { NameForDebugging = nameof(ParallelServiceTaskTest) };
		}

		BusinessObjectFactory Factory { get; set; }

		#endregion

		#region Helpers

		public EDIMessage GetMessageRowWithValidMessageContent() => UMIServiceTaskTest.GetMessageRowWithValidMessageContent(Factory);
		public EDIMessage GetMessageRowWithRandomMessageContent(int index) => UMIServiceTaskTest.GetMessageRowWithRandomMessageContent(Factory, index);
		public EDIMessage GetMessageRowWithDiscardingMessageContent() => UMIServiceTaskTest.GetMessageRowWithDiscardingMessageContent(Factory);
		public BusinessObject SetupConsol() => UMIServiceTaskTest.SetupConsol(Factory);

		ZGuid[] CreateMessageBatchInNewFactory(int batchSize)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = "BatchCreatorFactory", RefreshEnabled = false };
			var pks = Enumerable.Range(0, batchSize).Select(i => UMITestHelper.GetMessageRowWithValidMessageContent(factory).PK).ToArray();
			factory.Save();
			return pks;
		}

		static void AssertIsProcessed(string message, IEnumerable<ZGuid> allMessages)
		{
			GraphEngineTestExtensions.AssertStatusInQueue<EDIMessage>(message, new BusinessObjectFactory(), QueueStatusCodes.Codes.Processed, EDIMessageSchema.PK, allMessages.ToArray());
		}

		#endregion

		public void TestProcessUniversalSchedules()
		{
			var message = USIServiceTaskTest.GetMessageRowWithValidMessageContent(Factory,
				XmlEDIMessage.ApplicationCodes.UniversalDataMessaging,
				EDIMessageTypeList.Codes.XDC,
				EDIMessageSubTypeList.Codes.XmlUniversalSchedule,
				XmlEDIMessage.Status.Queued);

			Factory.Save();

			var serviceTask1 = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (var manager1 = new UniversalProcessingManager(serviceTask1.SupportedMessageSubtypes, serviceTask1.ExcludedMessageSubtypes, GrEngineServiceSetting.KeyGen))
			using (var manager2 = new UniversalProcessingManager(serviceTask1.SupportedMessageSubtypes, serviceTask1.ExcludedMessageSubtypes, GrEngineServiceSetting.Flipper))
			using (var manager3 = new UniversalProcessingManager(serviceTask1.SupportedMessageSubtypes, serviceTask1.ExcludedMessageSubtypes, GrEngineServiceSetting.Worker))
			{
				manager1.ExecuteBatch();
				manager2.FlipGrEngine();
				manager3.ExecuteBatch();
			}

			message.Reload();
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		public void TestMulti_Runner()
		{
			var messages = Enumerable.Range(0, 12).Select(GetMessageRowWithRandomMessageContent).ToArray();
			Factory.Save();

			var serviceTask1 = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (var manager1 = new UniversalProcessingManager(serviceTask1.SupportedMessageSubtypes, serviceTask1.ExcludedMessageSubtypes, GrEngineServiceSetting.KeyGen))
			using (var manager2 = new UniversalProcessingManager(serviceTask1.SupportedMessageSubtypes, serviceTask1.ExcludedMessageSubtypes, GrEngineServiceSetting.Flipper))
			using (var manager3 = new UniversalProcessingManager(serviceTask1.SupportedMessageSubtypes, serviceTask1.ExcludedMessageSubtypes, GrEngineServiceSetting.Worker))
			{
				manager1.ExecuteBatch();
				manager2.FlipGrEngine();
				manager3.ExecuteBatch();

				manager1.ExecuteBatch();
				manager2.FlipGrEngine();
				manager3.ExecuteBatch();

				manager1.ExecuteBatch();
				manager2.FlipGrEngine();
				manager3.ExecuteBatch();

				manager2.FlipGrEngine();
			}

			UMITestHelper.AssertIsProcessed("Everything should be done.", Factory, messages);
		}

		public void TestMulti_BatchSize()
		{
			eAdaptorRegistry.Instance.MessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);
			eAdaptorRegistry.Instance.MessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);
			eAdaptorRegistry.Instance.MessageQueueCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);
			var consol = SetupConsol();
			var messages = Enumerable.Range(0, 12).Select(GetMessageRowWithRandomMessageContent).ToArray();
			Factory.Save();

			var serviceTask1 = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (var manager1 = new UniversalProcessingManager(serviceTask1.SupportedMessageSubtypes, serviceTask1.ExcludedMessageSubtypes, GrEngineServiceSetting.KeyGen))
			using (var manager2 = new UniversalProcessingManager(serviceTask1.SupportedMessageSubtypes, serviceTask1.ExcludedMessageSubtypes, GrEngineServiceSetting.Flipper))
			using (var manager3 = new UniversalProcessingManager(serviceTask1.SupportedMessageSubtypes, serviceTask1.ExcludedMessageSubtypes, GrEngineServiceSetting.Worker))
			{
				manager1.ExecuteBatch();
				AssertEquals(4, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.PreKey));
				manager1.ExecuteBatch();
				manager1.ExecuteBatch();

				manager2.FlipGrEngine();
				AssertEquals(4, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Queued));

				manager3.ExecuteBatch();
				AssertEquals(4, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Notify));
			}
		}

		public void TestMulti_DoNotRecoverFromDisconnect()
		{
			var consol = SetupConsol();
			var messages = Enumerable.Range(0, 4).Select(i => GetMessageRowWithValidMessageContent()).ToArray();

			eAdaptorRegistry.Instance.MessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);
			eAdaptorRegistry.Instance.MessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);
			eAdaptorRegistry.Instance.MessageQueueCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);

			Factory.Save();

			var serviceTask1 = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (var manager1 = new UniversalProcessingManager(serviceTask1.SupportedMessageSubtypes, serviceTask1.ExcludedMessageSubtypes, GrEngineServiceSetting.KeyGen))
			using (var manager2 = new UniversalProcessingManager(serviceTask1.SupportedMessageSubtypes, serviceTask1.ExcludedMessageSubtypes, GrEngineServiceSetting.Flipper))
			using (var manager3 = new DisconnectingProcessingManager(serviceTask1.SupportedMessageSubtypes, serviceTask1.ExcludedMessageSubtypes, GrEngineServiceSetting.Worker))
			{
				manager1.ExecuteBatch();
				AssertEquals(4, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.PreKey));

				manager2.FlipGrEngine();
				AssertEquals(1, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Queued));

				AssertExceptionThrown<SqlLockLostException>(() => manager3.ExecuteBatch(CancellationToken.None));

				messages.ForEach(m => m.Reload());

				CombineAssertions(() =>
				{
					AssertEquals(string.Join(",", messages.Select(m => m.EM_Status).ToArray()), 3, messages.Count(m => m.EM_Status == XmlEDIMessage.Status.Queued));
					AssertEquals("Nothing should be processed.", 0, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Processed));
				});

				manager3.ExecuteBatch(CancellationToken.None);
				messages.ForEach(m => m.Reload());

				CombineAssertions(() =>
				{
					AssertEquals(string.Join(",", messages.Select(m => m.EM_Status).ToArray()), 4, messages.Count(m => m.EM_Status == XmlEDIMessage.Status.ProcessedOK));
					AssertEquals("The batch was poisoned, so everything ought to be processed.", 0, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Processed));
				});
			}
		}

		sealed class DisconnectingProcessingManager : IUniversalProcessingManager
		{
			readonly IUniversalProcessingManager universalProcessingManager;

			public DisconnectingProcessingManager(IEnumerable<string> messageSubTypes, IEnumerable<string> excludedMessageSubTypes, GrEngineServiceSetting settings = GrEngineServiceSetting.Disabled)
			{
				universalProcessingManager = new UniversalProcessingManager(messageSubTypes, excludedMessageSubTypes, settings);
				BusinessObjectFactory.SetOnFactorySaveHookForTest((thefactory) =>
				{
					if (thefactory.NameForDebugging == "Universal Message Processing")
					{
						if (MessagesProcessed == 1)
						{
							Db.Connection.CloseConnection();
						}

						MessagesProcessed++;
					}
				});
			}

			int MessagesProcessed { get; set; }

			public XmlEDIGrEngine GrEngine => universalProcessingManager.GrEngine;

			public LoggingInformation Logger { get => universalProcessingManager.Logger; }

			public void ProcessBatch(BaseMessageProcessor<XmlEDIMessage>.DisposableBatch messages, CancellationToken token, FailedMessagesManager failedMessagesManager)
			{
				universalProcessingManager.ProcessBatch(messages, token, failedMessagesManager);
			}

			public void ExecuteBatch(CancellationToken token)
			{
				universalProcessingManager.ExecuteBatch(token);
			}

			public void FlipGrEngine()
			{
				universalProcessingManager.FlipGrEngine();
			}

			public void Dispose()
			{
				universalProcessingManager.Dispose();
			}
		}

		public void TestKeyGen_IgnoresDiscardedMessages()
		{
			eAdaptorRegistry.Instance.MessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);
			eAdaptorRegistry.Instance.MessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 8);
			var messages = Enumerable.Range(0, 12).Select(i => GetMessageRowWithDiscardingMessageContent()).ToArray();
			Factory.Save();

			var task = new UMIServiceTaskWorker { ServiceLogger = new TestServiceLogger() };

			using (var manager = new UniversalProcessingManager(task.SupportedMessageSubtypes, task.ExcludedMessageSubtypes, GrEngineServiceSetting.KeyGen))
			{
				manager.ExecuteBatch();

				AssertEquals("Nothing is in the queue because all of the messages should have been discarded!", 0, GraphEngineTestExtensions.CountInQueue(messages, null));
				Factory.Load<XmlEDIMessage>(new ZQuery() { ReLoadExistingRows = true });
				AssertEquals(4, messages.Count(m => m.EM_Status == XmlEDIMessage.Status.Queued));
			}
		}

		public void TestKeyGen_DiscardedMessagesStillAttachErrors()
		{
			AssertDiscardedMessageHasErrors(GrEngineServiceSetting.KeyGen);
		}

		public void TestDefault_DiscardedMessagesStillAttachErrors()
		{
			AssertDiscardedMessageHasErrors(GrEngineServiceSetting.Disabled);
		}

		void AssertDiscardedMessageHasErrors(GrEngineServiceSetting grEngineSetting)
		{
			eAdaptorRegistry.Instance.MessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);
			eAdaptorRegistry.Instance.MessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 8);
			var message = GetMessageRowWithDiscardingMessageContent();
			Factory.Save();

			var task = new UMIServiceTaskWorker { ServiceLogger = new TestServiceLogger() };

			using (var manager = new UniversalProcessingManager(task.SupportedMessageSubtypes, task.ExcludedMessageSubtypes, grEngineSetting))
			{
				manager.ExecuteBatch();

				Factory.Load<XmlEDIMessage>(new ZQuery() { ReLoadExistingRows = true });

				AssertEquals(XmlEDIMessage.Status.Rejected, message.EM_Status);
				var notes = (StmNoteCollection)message.Notes.GetAllNotes();
				AssertEquals(1, notes.Count);
				var note = notes[0].ST_NoteDataAsText;
				AssertEquals("There should be an error message attached to the failed message",
					@"Error - Line 4: Top Level Element <Event> opened at line 3 cannot be imported as it is missing mandatory elements. Missing: EventTime, EventType.
Message Rejected.", note);
			}
		}

		public void TestContainsLogs()
		{
			eAdaptorRegistry.Instance.MessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);
			eAdaptorRegistry.Instance.MessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 8);
			var consol = SetupConsol();
			var messages = Enumerable.Range(0, 12).Select(GetMessageRowWithRandomMessageContent).ToArray();
			Factory.Save();

			var serviceTask1 = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (var manager1 = new UniversalProcessingManager(serviceTask1.SupportedMessageSubtypes, serviceTask1.ExcludedMessageSubtypes, GrEngineServiceSetting.KeyGen))
			{
				manager1.ExecuteBatch();
				manager1.ExecuteBatch();
				var logstr = string.Join("\r\n ", manager1.Logger.Logs.Select(l => l.Message));
				var startedExecutingBatchCount = Regex.Matches(logstr, "Starting executing batch").Count;
				var finishedExecutingBatchCount = Regex.Matches(logstr, @"Finished executing batch \(Elapsed time: \d+ ms, Processed \d+ messages\)").Count;
				var startedProcessingBatchCount = Regex.Matches(logstr, "Starting processing batch").Count;
				var finishedProcessingBatchCount = Regex.Matches(logstr, @"Finished processing batch \(Elapsed time: \d+ ms\)").Count;
				var startedRetrievingMessagesCount = Regex.Matches(logstr, "Starting retrieving next processable messages").Count;
				var finishedRetrievingMessagesCount = Regex.Matches(logstr, @"Finished retrieving next processable messages \(Elapsed time: \d+ ms, Number of records: \d+\)").Count;
				var startedProcessingMessageCount = Regex.Matches(logstr, @"Starting processing Message #[^\s]+").Count;
				var finishedProcessingMessageCount = Regex.Matches(logstr, @"Finished processing Message #[^\s]+ \(Elapsed time: \d+ ms\)").Count;

				AssertEquals(2, startedExecutingBatchCount);
				AssertEquals(2, finishedExecutingBatchCount);
				AssertEquals(4, startedProcessingBatchCount);
				AssertEquals(4, finishedProcessingBatchCount);
				AssertEquals(4, startedRetrievingMessagesCount);
				AssertEquals(4, finishedRetrievingMessagesCount);
				AssertEquals(12, startedProcessingMessageCount);
				AssertEquals(12, finishedProcessingMessageCount);
			}
		}
	}
}
