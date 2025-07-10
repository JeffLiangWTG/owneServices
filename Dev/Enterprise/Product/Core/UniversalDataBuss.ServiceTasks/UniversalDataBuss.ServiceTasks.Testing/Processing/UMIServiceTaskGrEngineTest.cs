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
using CargoWise.Data.Utils.Tests;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
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
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.UniversalDataBuss.ServiceTasks.Testing
{
	/// <summary>
	/// Ensure that none of the service tasks break with GrEngine enabled.
	/// </summary>
	[TestedType(typeof(UMIServiceTask))]
	[UseSnapshotProtection]
	class UMIServiceTaskGrEngineTest : ServiceTaskTestCase<UMIServiceTask>
	{
		#region Setup
		BusinessObjectFactory factory;

		protected override void SetUpCore()
		{
			base.SetUpCore();
			eAdaptorRegistry.Instance.AllowParallelUMI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.ParallelUMIQueueHistoryInHours.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 24);
			Db.ConnectionOverrideForTest = Db.NewExtraConnectionToMainDb();
			factory = new BusinessObjectFactory();
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
			Db.ConnectionOverrideForTest.Dispose();
			Db.ConnectionOverrideForTest = null;
		}

		#endregion

		#region Assertions

		void AssertIsProcessed(params EDIMessage[] messages)
		{
			UMITestHelper.AssertIsProcessed(null, factory, messages);
		}

		void AssertNotInQueue(params EDIMessage[] messages)
		{
			UMITestHelper.AssertNotInQueue(null, factory, messages);
		}

		BusinessObject SetupConsol() => UMIServiceTaskTest.SetupConsol(factory);

		EDIMessage GetMessageRowWithValidMessageContent() => UMIServiceTaskTest.GetMessageRowWithValidMessageContent(factory);
		EDIMessage GetMessageRowWithRandomMessageContent(int index) => UMIServiceTaskTest.GetMessageRowWithRandomMessageContent(factory, index);

		static void RunAllServiceTasks()
		{
			// Setup Service Task and Run.
			var serviceTask1 = new UMIServiceTaskKeyGen { ServiceLogger = new TestServiceLogger() };
			var serviceTask2 = new UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			var serviceTask3 = new UMIServiceTaskWorker { ServiceLogger = new TestServiceLogger() };
			serviceTask1.RunTask();
			serviceTask2.RunTask();
			serviceTask3.RunTask();
			serviceTask2.RunTask();
		}

		#endregion

		public void TestWorkerDoesNotEnqueue()
		{
			var consol = SetupConsol();
			var message1 = GetMessageRowWithValidMessageContent();
			var message2 = GetMessageRowWithValidMessageContent();
			var message3 = GetMessageRowWithValidMessageContent();

			factory.Save();

			// Setup Service Task and Run.
			var serviceTask = new UMIServiceTaskWorker { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			AssertNotInQueue(message1, message2, message3);
		}

		public void TestRetry_DoNotRecalculate()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				eAdaptorRegistry.Instance.MessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
				eAdaptorRegistry.Instance.MessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
				eAdaptorRegistry.Instance.MessageQueueCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);

				var messages = Enumerable.Range(0, 5).Select(i => UMIServiceTaskTest.GetMessageRowWithRandomMessageContent(factory, i)).ToArray();
				factory.Save();

				var processors = new ExceptionProcessManagers(new string[] { messages[0].EM_MessageSubType },
					new Exception("Exception Thrown during ProcessMessage(EDIMessage message)"));
				processors.KeyGen.ExecuteBatch(CancellationToken.None);
				processors.Flipper.FlipGrEngine();
				processors.Worker.ExecuteBatch(CancellationToken.None);

				var log = string.Join("\r\n", processors.Worker.Logger.UserLogStrings.Cast<string>());
				AssertContains("Exception processing message", log);

				processors.Worker.Logger.ClearLogs();

				processors.Worker.ExecuteBatch(CancellationToken.None);
				log = string.Join("\r\n", processors.Worker.Logger.UserLogStrings.Cast<string>());
				AssertContains("The second time this runs, we still process logs.", "Exception processing message", log);
				ErrorReporter.Clear();
			}
		}

		class ExceptionProcessManagers
		{
			public ExceptionProcessManagers(string[] codes, Exception exception)
			{
				KeyGen = new ProcessingManagerThatThrowsExceptionOnSave(codes, null, GrEngineServiceSetting.KeyGen);
				Flipper = new ProcessingManagerThatThrowsExceptionOnSave(codes, null, GrEngineServiceSetting.Flipper);
				Worker = new ProcessingManagerThatThrowsExceptionOnSave(codes, exception, GrEngineServiceSetting.Worker);
			}

			public ProcessingManagerThatThrowsExceptionOnSave KeyGen { get; }
			public ProcessingManagerThatThrowsExceptionOnSave Flipper { get; }
			public ProcessingManagerThatThrowsExceptionOnSave Worker { get; }
		}

		public void TestStmQueueStateContainsLogs()
		{
			var consol = SetupConsol();
			var message1 = GetMessageRowWithRandomMessageContent(0);
			var message2 = GetMessageRowWithRandomMessageContent(77);
			var message3 = GetMessageRowWithRandomMessageContent(999);

			factory.Save();
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				RunAllServiceTasks();
			}

			AssertIsProcessed(message1, message2, message3);
		}

		public void TestCanRequeue()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var consol = SetupConsol();
				var message1 = GetMessageRowWithValidMessageContent();

				factory.Save();

				RunAllServiceTasks();

				AssertIsProcessed(message1);

				message1.Reload();
				AssertNotEquals(XmlEDIMessage.Status.Queued, message1.EM_Status);
				message1.EM_Status = XmlEDIMessage.Status.Queued;
				message1.Factory.Save();

				RunAllServiceTasks();

				message1.Reload();
				AssertNotEquals(XmlEDIMessage.Status.Queued, message1.EM_Status);
			}
		}

		public void TestFlipperBlockedByEarlierUDMMessage()
		{
			var messages = Enumerable.Range(0, 5).Select(i => UMIServiceTaskTest.GetMessageRowWithRandomMessageContent(factory, i)).ToArray();
			messages[0].EM_IsActive = false;

			for (int i = 0; i < messages.Length; i++)
			{
				messages[i].EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(i);
			}

			factory.Save();

			var keyGen = new UniversalProcessingManager(new UMIServiceTaskKeyGen().SupportedMessageSubtypes, new UMIServiceTaskKeyGen().ExcludedMessageSubtypes, GrEngineServiceSetting.KeyGen);
			var flipper = new UniversalProcessingManager(new UMIServiceTask().SupportedMessageSubtypes, new UMIServiceTask().ExcludedMessageSubtypes, GrEngineServiceSetting.Flipper);
			var worker = new UniversalProcessingManager(new UMIServiceTaskWorker().SupportedMessageSubtypes, new UMIServiceTaskWorker().ExcludedMessageSubtypes, GrEngineServiceSetting.Worker);

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				keyGen.ExecuteBatch(CancellationToken.None);

				var stms = flipper.GrEngine.Dequeuer.LoadAll().ToArray();
				AssertEquals(4, stms.Length);
				stms.ForEach(stm => AssertEquals("PKE", stm.Status));

				messages[0].EM_IsActive = true;
				factory.Save();

				// Assert the unprocessed message blocks the flipper
				flipper.FlipGrEngine();
				stms = flipper.GrEngine.Dequeuer.LoadAll().ToArray();
				stms.ForEach(stm => AssertEquals("PKE", stm.Status));

				keyGen.ExecuteBatch(CancellationToken.None);

				stms = flipper.GrEngine.Dequeuer.LoadAll().ToArray();
				AssertEquals(5, stms.Length);
				stms.ForEach(stm => AssertEquals("PKE", stm.Status));

				flipper.FlipGrEngine();
				stms = flipper.GrEngine.Dequeuer.LoadAll().ToArray();
				stms.ForEach(stm => AssertEquals("QUE", stm.Status));
			}
		}

		public void TestFlipperNotBlockedByEarlierNonUDMMessage()
		{
			var messages = Enumerable.Range(0, 5).Select(i => UMIServiceTaskTest.GetMessageRowWithRandomMessageContent(factory, i)).ToArray();
			messages[0].EM_IsActive = false;
			messages[0].EM_ApplicationCode = "TTT";

			for (int i = 0; i < messages.Length; i++)
			{
				messages[i].EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(i);
			}

			factory.Save();

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var keyGen = new UniversalProcessingManager(new UMIServiceTaskKeyGen().SupportedMessageSubtypes, new UMIServiceTaskKeyGen().ExcludedMessageSubtypes, GrEngineServiceSetting.KeyGen);
				var flipper = new UniversalProcessingManager(new UMIServiceTask().SupportedMessageSubtypes, new UMIServiceTask().ExcludedMessageSubtypes, GrEngineServiceSetting.Flipper);
				var worker = new UniversalProcessingManager(new UMIServiceTaskWorker().SupportedMessageSubtypes, new UMIServiceTaskWorker().ExcludedMessageSubtypes, GrEngineServiceSetting.Worker);

				keyGen.ExecuteBatch(CancellationToken.None);

				var stms = flipper.GrEngine.Dequeuer.LoadAll().ToArray();
				AssertEquals(4, stms.Length);
				stms.ForEach(stm => AssertEquals("PKE", stm.Status));

				messages[0].EM_IsActive = true;
				factory.Save();

				// Assert the unprocessed message does not block the flipper
				flipper.FlipGrEngine();
				stms = flipper.GrEngine.Dequeuer.LoadAll().ToArray();
				stms.ForEach(stm => AssertEquals("QUE", stm.Status));

				keyGen.ExecuteBatch(CancellationToken.None);

				// Assert the Keygen ignores the non UDM message
				stms = flipper.GrEngine.Dequeuer.LoadAll().ToArray();
				AssertEquals(4, stms.Length);
				stms.ForEach(stm => AssertEquals("QUE", stm.Status));
			}
		}

		public void TestUnsupportedMessagesAreRejectedByKeyGen()
		{
			var invalidMessage1 = GetMessageRowWithValidMessageContent();
			invalidMessage1.EM_ApplicationCode = "UDM";
			invalidMessage1.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalActivityRequest;

			factory.Save();

			// Setup Service Task and Run.
			var keyGen = new UniversalProcessingManager(new UMIServiceTaskKeyGen().SupportedMessageSubtypes, new UMIServiceTaskKeyGen().ExcludedMessageSubtypes, GrEngineServiceSetting.KeyGen);
			keyGen.ExecuteBatch(CancellationToken.None);

			var newFactory = new BusinessObjectFactory();
			invalidMessage1 = newFactory.Load<EDIMessage>(invalidMessage1.PK);
			AssertEquals("InvalidMessage1 should be rejected", EDIMessage.Status.Rejected, invalidMessage1.EM_Status);

			var noteText = ((StmNoteCollection)invalidMessage1.Notes.GetAllNotes())[0].ST_NoteDataAsText;
			AssertContains("Unsupported Messaged should have a note", $"Messages of Sub Type [{EDIMessageSubTypeList.Codes.XmlUniversalActivityRequest}] can only be processed using eAdaptor HTTP+XML", noteText);
		}

		public void TestUnkownMessagesAreRejectedByKeyGen()
		{
			var invalidMessage1 = GetMessageRowWithValidMessageContent();
			invalidMessage1.EM_ApplicationCode = "UDM";
			invalidMessage1.EM_MessageSubType = "XXX";

			factory.Save();

			// Setup Service Task and Run.
			var keyGen = new UniversalProcessingManager(new UMIServiceTaskKeyGen().SupportedMessageSubtypes, new UMIServiceTaskKeyGen().ExcludedMessageSubtypes, GrEngineServiceSetting.KeyGen);
			keyGen.ExecuteBatch(CancellationToken.None);

			var newFactory = new BusinessObjectFactory();
			invalidMessage1 = newFactory.Load<EDIMessage>(invalidMessage1.PK);
			AssertEquals("InvalidMessage1 should be rejected", EDIMessage.Status.Rejected, invalidMessage1.EM_Status);

			var noteText = ((StmNoteCollection)invalidMessage1.Notes.GetAllNotes())[0].ST_NoteDataAsText;
			AssertContains("Unsupported Messaged should have a note", "Invalid Message Sub Type [XXX]", noteText);
		}

		public void TestIfBacklogIsFullKeyGeneratedForLeftOutMessages()
		{
			var mockedBacklogChecker = new Mock<IPreKeyBacklogChecker>();
			var isBacklogFull = false;
			mockedBacklogChecker.Setup(f => f.IsBacklogTooBig).Returns(() => isBacklogFull);

			using (ObjectFactory.Substitute(mockedBacklogChecker.Object))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var keyGen = new UniversalProcessingManager(new UMIServiceTaskKeyGen().SupportedMessageSubtypes, new UMIServiceTaskKeyGen().ExcludedMessageSubtypes, GrEngineServiceSetting.KeyGen);
				var flipper = new UniversalProcessingManager(new UMIServiceTask().SupportedMessageSubtypes, new UMIServiceTask().ExcludedMessageSubtypes, GrEngineServiceSetting.Flipper);

				var messages = Enumerable.Range(0, 10)
					.Select(i => UMIServiceTaskTest.GetMessageRowWithRandomMessageContent(factory, i)).ToArray();
				for (int i = 0; i < messages.Length; i++)
				{
					if (i < 3 || i > 7)
					{
						messages[i].EM_IsActive = false;
					}
					messages[i].EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(i);
				}

				factory.Save();

				keyGen.ExecuteBatch(CancellationToken.None);
				AssertEquals(5, flipper.GrEngine.Dequeuer.LoadAll().Count());
				keyGen.ExecuteBatch(CancellationToken.None);
				AssertEquals(5, flipper.GrEngine.Dequeuer.LoadAll().Count());

				foreach (var message in messages)
				{
					message.EM_IsActive = true;
				}
				factory.Save();
				isBacklogFull = true;

				keyGen.ExecuteBatch(CancellationToken.None);

				var stms = flipper.GrEngine.Dequeuer.LoadAll().ToArray();
				AssertEquals(8, stms.Length);
				for (int i = 0; i < 8; i++)
				{
					Assert(stms.Any(s => s.ParentMessageNumber == messages[i].EM_MessageNum));
				}
			}
		}

		public void TestPreEnqueuerLoadsLeftOutMessagesIfBacklogIsFull()
		{
			var keyGen = new UniversalProcessingManager(new UMIServiceTaskKeyGen().SupportedMessageSubtypes, new UMIServiceTaskKeyGen().ExcludedMessageSubtypes, GrEngineServiceSetting.KeyGen);

			var getLeftOutMessagesCalled = false;
			ZQuery GetLeftOutMessagesQuery()
			{
				getLeftOutMessagesCalled = true;
				return null;
			}
			var mockedBacklogChecker = new Mock<IPreKeyBacklogChecker>();
			var isBacklogFull = false;
			mockedBacklogChecker.Setup(f => f.IsBacklogTooBig).Returns(() => isBacklogFull);

			using (ObjectFactory.Substitute(mockedBacklogChecker.Object))
			{
				var appLockLogger = new AppLockLogger<EDIMessage>(null, null);
				var businessObjectFactory = new BusinessObjectFactory();
				keyGen.GrEngine.PreEnqueuer.LoadPreKeyBatch(businessObjectFactory, "", ZQuery.NoResultQuery,
					ZQuery.NoResultQuery, 10, appLockLogger, GetLeftOutMessagesQuery);
				Assert("GetLeftOutMessagesQuery should not be called", !getLeftOutMessagesCalled);

				isBacklogFull = true;
				keyGen.GrEngine.PreEnqueuer.LoadPreKeyBatch(businessObjectFactory, "", ZQuery.NoResultQuery,
					ZQuery.NoResultQuery, 10, appLockLogger, GetLeftOutMessagesQuery);
				Assert("GetLeftOutMessagesQuery should be called", getLeftOutMessagesCalled);
			}
		}

		public void TestGetMessagesWaitingForKeyGenerationQuery()
		{
			var keyGen = new UniversalProcessingManager(new UMIServiceTaskKeyGen().SupportedMessageSubtypes, new UMIServiceTaskKeyGen().ExcludedMessageSubtypes, GrEngineServiceSetting.KeyGen);
			var query = keyGen.GetMessagesWaitingForKeyGenerationQueryForTest();
			AssertEquals("", query.LiteralTextSqlFormatted);

			var messages = Enumerable.Range(0, 3)
				.Select(i => UMIServiceTaskTest.GetMessageRowWithRandomMessageContent(factory, i)).ToArray();
			factory.Save();
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				keyGen.ExecuteBatch(CancellationToken.None);
			}

			query = keyGen.GetMessagesWaitingForKeyGenerationQueryForTest();
			AssertEquals($"EM_MessageNum <= '{messages.Max(m => m.EM_MessageNum)}'\r\n", query.LiteralTextSqlFormatted);
		}

		public void TestProcessingManagerABACorrectness()
		{
			var consol = SetupConsol();
			var message1 = GetMessageRowWithValidMessageContent();

			factory.Save();

			var testLockProvider = TestLockProvider.CreateDbLockProvider(Db.Connection);
			var processingManager = new UniversalProcessingManager(
				new UMIServiceTask().SupportedMessageSubtypes,
				new UMIServiceTask().ExcludedMessageSubtypes,
				GrEngineServiceSetting.KeyGen,
				null,
				testLockProvider);

			bool stopRecursion = false;
			testLockProvider.PreFunc = () =>
			{
				if (stopRecursion)
				{
					return true;
				}

				// Validate that we cannot retrieve the message after the SQL statement but before the applock has been applied
				stopRecursion = true;
				string error = string.Empty;

				var thread = new Thread(() =>
				{
					try
					{
						using (Db.DisposableActionForDbConnection())
						{
							var testProcessingManager = new TestProcessingManager(
								new UMIServiceTask().SupportedMessageSubtypes,
								new UMIServiceTask().ExcludedMessageSubtypes,
								GrEngineServiceSetting.KeyGen,
								null,
								null);

							testProcessingManager.AssertNoMoreMessages();
						}
					}
					catch (AssertionFailedError ex)
					{
						error = ex.ToString();
					}
				});

				thread.Start();
				thread.Join();

				if (!string.IsNullOrEmpty(error))
				{
					HtmlFail(error);
				}

				return true;
			};

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (processingManager)
			{
				processingManager.ExecuteBatch(CancellationToken.None);
			}

			AssertEquals("Duplicate message detected", 1, GraphEngineTestExtensions.CountInQueue(new[] { message1 }));
		}

		[TestDate(2018, 7, 3)]
		public void TestRememberToDeleteStmQueueStates()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var consol = SetupConsol();
				var message1 = GetMessageRowWithValidMessageContent();

				factory.Save();
				RunAllServiceTasks();
				AssertIsProcessed(message1);
				AssertEquals("Message is processed", 1, GraphEngineTestExtensions.CountInQueue(new[] { message1 }, QueueStatusCodes.Codes.Processed));

				TestDateAttribute.Date = TestDateAttribute.Date.AddDays(2);
				RunAllServiceTasks();
				AssertEquals("Message should be deleted.", 0, GraphEngineTestExtensions.CountInQueue(new[] { message1 }, QueueStatusCodes.Codes.Processed));
			}
		}

		public void TestBatchSize()
		{
			var anUnusualBatchSize = 7;
			eAdaptorRegistry.Instance.MessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, anUnusualBatchSize);

			using (var x = new XmlEDIGrEngine(GrEngineServiceSetting.KeyGen))
			{
				AssertEquals(anUnusualBatchSize, x.BatchSize);
			}
		}

		[TestDate(2012, 12, 12)]
		public void TestWorkersProcessQueue()
		{
			eAdaptorRegistry.Instance.MessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			eAdaptorRegistry.Instance.MessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			eAdaptorRegistry.Instance.MessageQueueCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 40);
			SetupConsol();
			var messages = Enumerable.Range(0, 40).Select(GetMessageRowWithRandomMessageContent).ToArray();

			for (int i = 0; i < messages.Length; i++)
			{
				messages[i].EM_MessageNum = i.ToString("D20");
				messages[i].EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(i);
			}

			factory.Save();
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var pre = new UMIServiceTaskKeyGen();
				var masterTask = new UMIServiceTask();
				var worker1 = new UMIServiceTaskWorker();
				var worker2 = new UMIServiceTaskWorker();

				pre.RunTask();
				pre.RunTask();
				masterTask.RunTask();
				masterTask.RunTask();
				AssertEquals(20, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Queued));
				pre.RunTask();
				AssertEquals(10, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.PreKey));
				masterTask.RunTask();
				AssertEquals(30, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Queued));

				worker1.RunTask();
				AssertEquals(10, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Notify));
				worker2.RunTask();
				AssertEquals(20, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Notify));
				worker2.RunTask();
				AssertEquals(30, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Notify));
			}
		}

		public void TestNudgeWorkers()
		{
			eAdaptorRegistry.Instance.MessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			eAdaptorRegistry.Instance.MessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			eAdaptorRegistry.Instance.MessageQueueCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20);
			var inteceptor = new NudgeInterceptor(UMIServiceTaskWorker.CODE);
			using (ObjectFactory.Substitute<IServiceTaskNudger>(inteceptor))
			{
				var consol = SetupConsol();
				var messages = Enumerable.Range(0, 5).Select(i => GetMessageRowWithValidMessageContent()).ToArray();
				factory.Save();
				new UMIServiceTaskKeyGen().RunTask();
				var masterTask = new UMIServiceTask();
				masterTask.RunTask();

				AssertEquals(4, inteceptor.FireCount);
			}
		}

		public void TestNudgeKeyGen()
		{
			eAdaptorRegistry.Instance.MessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			eAdaptorRegistry.Instance.MessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			eAdaptorRegistry.Instance.MessageQueueCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20);
			var inteceptor = new NudgeInterceptor(UMIServiceTaskKeyGen.CODE);
			using (ObjectFactory.Substitute<IServiceTaskNudger>(inteceptor))
			{
				var consol = SetupConsol();
				var messages = Enumerable.Range(0, 5).Select(i => GetMessageRowWithValidMessageContent()).ToArray();
				factory.Save();
				new UMIServiceTaskKeyGen().RunTask();
				var masterTask = new UMIServiceTask();
				masterTask.RunTask();

				AssertEquals(1, inteceptor.FireCount);
			}
		}

		public void TestNudgeMaster()
		{
			eAdaptorRegistry.Instance.MessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			eAdaptorRegistry.Instance.MessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			eAdaptorRegistry.Instance.MessageQueueCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20);
			var inteceptor = new NudgeInterceptor(UMIServiceTask.CODE);
			using (ObjectFactory.Substitute<IServiceTaskNudger>(inteceptor))
			{
				var consol = SetupConsol();
				var messages = Enumerable.Range(0, 5).Select(i => GetMessageRowWithValidMessageContent()).ToArray();
				factory.Save();
				new UMIServiceTaskKeyGen().RunTask();
				AssertEquals(10, inteceptor.FireCount);
			}
		}

		public void TestNudgeMaster_FromWorker()
		{
			eAdaptorRegistry.Instance.MessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			eAdaptorRegistry.Instance.MessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			eAdaptorRegistry.Instance.MessageQueueCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20);
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var consol = SetupConsol();
				var messages = Enumerable.Range(0, 5).Select(i => GetMessageRowWithValidMessageContent()).ToArray();
				factory.Save();
				new UMIServiceTaskKeyGen().RunTask();
				new UMIServiceTask().RunTask();

				var inteceptor = new NudgeInterceptor(UMIServiceTask.CODE);
				using (ObjectFactory.Substitute<IServiceTaskNudger>(inteceptor))
				{
					new UMIServiceTaskWorker().RunTask();

					AssertEquals(5, inteceptor.FireCount);
				}
			}
		}

		public void TestNoNudge()
		{
			var inteceptor = new NudgeInterceptor(UMIServiceTask.CODE);
			using (ObjectFactory.Substitute<IServiceTaskNudger>(inteceptor))
			{
				factory.Save();
				var masterTask = new UMIServiceTask();
				masterTask.RunTask();
				AssertEquals(0, inteceptor.FireCount);
			}
		}

		[TestDate(2012, 12, 12)]
		public void TestWatermarkQueryIsLazy()
		{
			eAdaptorRegistry.Instance.MessagesPerBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			eAdaptorRegistry.Instance.MessagesPerExecution.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);
			eAdaptorRegistry.Instance.MessageQueueCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			SetupConsol();
			var messages = Enumerable.Range(0, 3).Select(GetMessageRowWithRandomMessageContent).ToArray();

			for (int i = 0; i < messages.Length; i++)
			{
				messages[i].EM_MessageNum = i.ToString("D20");
				messages[i].EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(i);
			}

			factory.Save();
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var pre = new UMIServiceTaskKeyGen();
				var masterTask = new UMIServiceTask();

				pre.RunTask();
				AssertEquals(3, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.PreKey));

				var regex = new Regex($"select .* from dbo\\.{EDIMessage.Schema.TableName}", RegexOptions.IgnoreCase);

				using (Db.Connection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: false))
				{
					masterTask.RunTask();
					AssertEquals(1, Db.Connection.ExecutedCommandsAndQueryPlans.Count(q => regex.IsMatch(q.Item1)));
				}

				using (Db.Connection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: false))
				{
					masterTask.RunTask();
					AssertEquals("Should not hit EDI message table when message queue is full", 0, Db.Connection.ExecutedCommandsAndQueryPlans.Count(q => regex.IsMatch(q.Item1)));
				}

				AssertEquals(2, GraphEngineTestExtensions.CountInQueue(messages, QueueStatusCodes.Codes.Queued));
			}
		}

		public void TestFlipperShouldReinitializeGrEngineAfterFirstIteration()
		{
			var messages = Enumerable.Range(0, 5).Select(i => UMIServiceTaskTest.GetMessageRowWithRandomMessageContent(factory, i)).ToArray();
			messages[0].EM_IsActive = false;

			for (var i = 0; i < messages.Length; i++)
			{
				messages[i].EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(i);
			}

			factory.Save();

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var keyGen = new UniversalProcessingManager(new UMIServiceTaskKeyGen().SupportedMessageSubtypes, new UMIServiceTaskKeyGen().ExcludedMessageSubtypes, GrEngineServiceSetting.KeyGen);
				keyGen.ExecuteBatch(CancellationToken.None);

				var list = new List<XmlEDIGrEngine>();
				UniversalProcessingManager.SetIterationCompletedHookForTest((grEngine) =>
				{
					list.Add(grEngine);
				});

				var flipper = new UniversalProcessingManager(new UMIServiceTask().SupportedMessageSubtypes, new UMIServiceTask().ExcludedMessageSubtypes, GrEngineServiceSetting.Flipper);
				flipper.FlipGrEngine();
				AssertEquals(4, list.Distinct().Count());
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"XML Universal Shipment",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalShipment),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"XML Universal Event",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalEvent),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"XML Universal Transaction",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalTransaction),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"XML Universal Transaction Batch",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch),
				};
			}
		}

		class TestProcessingManager : IUniversalProcessingManager
		{
			readonly UniversalProcessingManager universalProcessingManager;
			public TestProcessingManager(IEnumerable<string> messageSubTypes, IEnumerable<string> excludedMessageSubTypes, GrEngineServiceSetting settings = GrEngineServiceSetting.Disabled, IFactoryService factoryService = null, ISqlApplicationLockProvider lockProvider = null)
			{
				universalProcessingManager = new UniversalProcessingManager(messageSubTypes, excludedMessageSubTypes, settings, factoryService, lockProvider);
			}

			public void AssertNoMoreMessages()
			{
				using (var messages = universalProcessingManager.GetMessagesForTest())
				{
					Assert("Should be no messages here", messages.Length == 0);
				}
			}

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
	}
}
