using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.LogWalker;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave.Testing
{
	abstract class TriggerActionWithOptionalFactorySaveLogSubscriberTest : LogSubscriberTest<TriggerActionWithOptionalFactorySaveLogSubscriber>
	{
		public void TestInheritance()
		{
			Assert("Uses behaviour of LogSubscriberWithOptionalFactorySave", typeof(TriggerActionWithOptionalFactorySaveLogSubscriber).IsSubclassOf(typeof(LogSubscriberWithOptionalFactorySave)));
		}

		public void TestProcessLogQueueItems()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var queuedLogMock = GetQueuedLog();
			var subscriberLog = NewsTransmitter.CreateStmJobQueue(Factory, queuedLogMock.Object, new TriggerActionWithOptionalFactorySaveLogSubscriber(), Events.MiscellaneousEventCode, TestObjectCreator.Staff.GS_Code, TestObjectCreator.NonCurrentBranch.GB_Code, TestObjectCreator.NonCurrentDepartment.GE_Code,
				$"|PPK={dummy.PK}|PTP=Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business|TRA={WorkflowTriggerActionType}");
			Factory.Save();

			ZGuid processorUserPK = default, processorBranchPK = default, processorDepartmentPK = default;
			var processorMock = new Mock<IProcessor>();
			processorMock.Setup(x => x.Process(It.IsAny<INotifications>(), It.IsAny<CancellationToken>())).Callback<INotifications, CancellationToken>(Process);

			SetupTriggerProcessorMock(dummy, processorMock.Object);
			var notificationList = ProcessLogs();

			var subscriberLogAfterProcessing = new BusinessObjectFactory().Load<IQueuedLog>(subscriberLog.PK);
			AssertEquals("Postcondition: log status after processing", JobQueueStatus.StatusProcessed, subscriberLogAfterProcessing.SJ_Status);
			AssertContains("Correct Notifications are used", "My warning", notificationList);
			AssertEquals("CurrentUserPK", TestObjectCreator.Staff.PK, processorUserPK);
			AssertEquals("CurrentBranchPK", TestObjectCreator.NonCurrentBranch.PK, processorBranchPK);
			AssertEquals("CurrentDepartmentPK", TestObjectCreator.NonCurrentDepartment.PK, processorDepartmentPK);

			void Process(INotifications notifications, CancellationToken token)
			{
				notifications.AddWarning("My warning");
				processorUserPK = Env.CurrentUserPK;
				processorBranchPK = Env.CurrentBranchPK;
				processorDepartmentPK = Env.CurrentDepartmentPK;
			}
		}

		public void TestGetPrettyPrinter()
		{
			var dummyBizOWithWorkflow = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var queuedLogMock = GetQueuedLog();
			queuedLogMock.SetupGet(x => x.SJ_ParentID).Returns(dummyBizOWithWorkflow.PK);

			var subscriber = new TriggerActionWithOptionalFactorySaveLogSubscriber();
			var subscriberLog = NewsTransmitter.CreateStmJobQueue(Factory, queuedLogMock.Object, subscriber, Events.MiscellaneousEventCode,
				GlbStaff.CurrentUser.GS_Code, GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code,
				$"|PPK={dummyBizOWithWorkflow.PK}|PTP=Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business|TRA={WorkflowTriggerActionType}");

			Factory.Save();

			var processLogMethodInfo = subscriber.GetType().GetMethod("ProcessLogQueueItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			processLogMethodInfo.Invoke(subscriber, new object[] { new IQueuedLog[] { subscriberLog } });

			var expectedMsg = $@"Subscriber Name: AccTriggerWithOptSaveLogSubscriber ({WorkflowTriggerActionType}),
Parent: {dummyBizOWithWorkflow.PK} Table: {DummyWithWorkflow.Schema.TablePrefix}";

			AssertEquals(expectedMsg, subscriber.GetPrettyPrinter().PrettyPrintSubscriberErrorInfo(queuedLogMock.Object));
		}

		public void TestInvalidUserContext_User()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var queuedLogMock = GetQueuedLog();
			var subscriberLog = NewsTransmitter.CreateStmJobQueue(Factory, queuedLogMock.Object, new TriggerActionWithOptionalFactorySaveLogSubscriber(), Events.MiscellaneousEventCode, "XXX", TestObjectCreator.NonCurrentBranch.GB_Code, TestObjectCreator.NonCurrentDepartment.GE_Code,
				$"|PPK={dummy.PK}|PTP=Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business|TRA={WorkflowTriggerActionType}");
			Factory.Save();

			var notificationList = ProcessLogs();

			var subscriberLogAfterProcessing = new BusinessObjectFactory().Load<IQueuedLog>(subscriberLog.PK);
			AssertEquals("Postcondition: log status after processing", JobQueueStatus.StatusFailed, subscriberLogAfterProcessing.SJ_Status);
			var expectedErrorMessaage = "Invalid user, branch or department values. User: null, log user: XXX. Branch: SYD, log branch SYD. Department: CEA, log department CEA.";
			AssertContains("Correct Notifications are used", expectedErrorMessaage, notificationList);
			AssertEquals("LastMessageReported", expectedErrorMessaage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestInvalidUserContext_Branch()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var queuedLogMock = GetQueuedLog();
			var subscriberLog = NewsTransmitter.CreateStmJobQueue(Factory, queuedLogMock.Object, new TriggerActionWithOptionalFactorySaveLogSubscriber(), Events.MiscellaneousEventCode, TestObjectCreator.Staff.GS_Code, "BBB", TestObjectCreator.NonCurrentDepartment.GE_Code,
				$"|PPK={dummy.PK}|PTP=Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business|TRA={WorkflowTriggerActionType}");
			Factory.Save();

			var notificationList = ProcessLogs();

			var subscriberLogAfterProcessing = new BusinessObjectFactory().Load<IQueuedLog>(subscriberLog.PK);
			AssertEquals("Postcondition: log status after processing", JobQueueStatus.StatusFailed, subscriberLogAfterProcessing.SJ_Status);
			var expectedErrorMessaage = "Invalid user, branch or department values. User: TST, log user: TST. Branch: null, log branch BBB. Department: CEA, log department CEA.";
			AssertContains("Correct Notifications are used", expectedErrorMessaage, notificationList);
			AssertEquals("LastMessageReported", expectedErrorMessaage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestInvalidUserContext_Department()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var queuedLogMock = GetQueuedLog();
			var subscriberLog = NewsTransmitter.CreateStmJobQueue(Factory, queuedLogMock.Object, new TriggerActionWithOptionalFactorySaveLogSubscriber(), Events.MiscellaneousEventCode, TestObjectCreator.Staff.GS_Code, TestObjectCreator.NonCurrentBranch.GB_Code, "DDD",
				$"|PPK={dummy.PK}|PTP=Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business|TRA={WorkflowTriggerActionType}");
			Factory.Save();

			var notificationList = ProcessLogs();

			var subscriberLogAfterProcessing = new BusinessObjectFactory().Load<IQueuedLog>(subscriberLog.PK);
			AssertEquals("Postcondition: log status after processing", JobQueueStatus.StatusFailed, subscriberLogAfterProcessing.SJ_Status);
			var expectedErrorMessaage = "Invalid user, branch or department values. User: TST, log user: TST. Branch: SYD, log branch SYD. Department: null, log department DDD.";
			AssertContains("Correct Notifications are used", expectedErrorMessaage, notificationList);
			AssertEquals("LastMessageReported", expectedErrorMessaage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestInvalidLogParameters_InvalidParentPKString_Case1()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var queuedLogMock = GetQueuedLog();
			var subscriberLog = NewsTransmitter.CreateStmJobQueue(Factory, queuedLogMock.Object, new TriggerActionWithOptionalFactorySaveLogSubscriber(), Events.MiscellaneousEventCode, TestObjectCreator.Staff.GS_Code, TestObjectCreator.NonCurrentBranch.GB_Code, TestObjectCreator.NonCurrentDepartment.GE_Code,
				$"|PTP=Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business|TRA={WorkflowTriggerActionType}");
			Factory.Save();

			var notificationList = ProcessLogs();

			var subscriberLogAfterProcessing = new BusinessObjectFactory().Load<IQueuedLog>(subscriberLog.PK);
			AssertEquals("Postcondition: log status after processing", JobQueueStatus.StatusFailed, subscriberLogAfterProcessing.SJ_Status);
			var expectedErrorMessaage = "parentPKString is invalid.";
			AssertContains("Correct Notifications are used", expectedErrorMessaage, notificationList);
			AssertNotNull("LastExceptionReported", ErrorReporter.LastExceptionReported);
			AssertEquals("LastExceptionReported.Message", expectedErrorMessaage, ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestInvalidLogParameters_InvalidParentPKString_Case2()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var queuedLogMock = GetQueuedLog();
			var subscriberLog = NewsTransmitter.CreateStmJobQueue(Factory, queuedLogMock.Object, new TriggerActionWithOptionalFactorySaveLogSubscriber(), Events.MiscellaneousEventCode, TestObjectCreator.Staff.GS_Code, TestObjectCreator.NonCurrentBranch.GB_Code, TestObjectCreator.NonCurrentDepartment.GE_Code,
				$"|PPK=123213|PTP=Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business|TRA={WorkflowTriggerActionType}");
			Factory.Save();

			var notificationList = ProcessLogs();

			var subscriberLogAfterProcessing = new BusinessObjectFactory().Load<IQueuedLog>(subscriberLog.PK);
			AssertEquals("Postcondition: log status after processing", JobQueueStatus.StatusFailed, subscriberLogAfterProcessing.SJ_Status);
			var expectedErrorMessaage = "parentPKString is invalid.";
			AssertContains("Correct Notifications are used", expectedErrorMessaage, notificationList);
			AssertNotNull("LastExceptionReported", ErrorReporter.LastExceptionReported);
			AssertEquals("LastExceptionReported.Message", expectedErrorMessaage, ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestInvalidLogParameters_InvalidTypeName_Case1()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var queuedLogMock = GetQueuedLog();
			var subscriberLog = NewsTransmitter.CreateStmJobQueue(Factory, queuedLogMock.Object, new TriggerActionWithOptionalFactorySaveLogSubscriber(), Events.MiscellaneousEventCode, TestObjectCreator.Staff.GS_Code, TestObjectCreator.NonCurrentBranch.GB_Code, TestObjectCreator.NonCurrentDepartment.GE_Code,
				$"|PPK={dummy.PK}|TRA={WorkflowTriggerActionType}");
			Factory.Save();

			var notificationList = ProcessLogs();

			var subscriberLogAfterProcessing = new BusinessObjectFactory().Load<IQueuedLog>(subscriberLog.PK);
			AssertEquals("Postcondition: log status after processing", JobQueueStatus.StatusFailed, subscriberLogAfterProcessing.SJ_Status);
			var expectedErrorMessaage = "typeName is invalid.";
			AssertContains("Correct Notifications are used", expectedErrorMessaage, notificationList);
			AssertNotNull("LastExceptionReported", ErrorReporter.LastExceptionReported);
			AssertEquals("LastExceptionReported.Message", expectedErrorMessaage, ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestInvalidLogParameters_InvalidTypeName_Case2()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var queuedLogMock = GetQueuedLog();
			var subscriberLog = NewsTransmitter.CreateStmJobQueue(Factory, queuedLogMock.Object, new TriggerActionWithOptionalFactorySaveLogSubscriber(), Events.MiscellaneousEventCode, TestObjectCreator.Staff.GS_Code, TestObjectCreator.NonCurrentBranch.GB_Code, TestObjectCreator.NonCurrentDepartment.GE_Code,
				$"|PPK={dummy.PK}|PTP=Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow|TRA={WorkflowTriggerActionType}");
			Factory.Save();

			var notificationList = ProcessLogs();

			var subscriberLogAfterProcessing = new BusinessObjectFactory().Load<IQueuedLog>(subscriberLog.PK);
			AssertEquals("Postcondition: log status after processing", JobQueueStatus.StatusFailed, subscriberLogAfterProcessing.SJ_Status);
			var expectedErrorMessaage = "typeName is invalid.";
			AssertContains("Correct Notifications are used", expectedErrorMessaage, notificationList);
			AssertNotNull("LastExceptionReported", ErrorReporter.LastExceptionReported);
			AssertEquals("LastExceptionReported.Message", expectedErrorMessaage, ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestInvalidLogParameters_InvalidTriggerAction_Case1()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var queuedLogMock = GetQueuedLog();
			var subscriberLog = NewsTransmitter.CreateStmJobQueue(Factory, queuedLogMock.Object, new TriggerActionWithOptionalFactorySaveLogSubscriber(), Events.MiscellaneousEventCode, TestObjectCreator.Staff.GS_Code, TestObjectCreator.NonCurrentBranch.GB_Code, TestObjectCreator.NonCurrentDepartment.GE_Code,
				$"|PPK={dummy.PK}|PTP=Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business");
			Factory.Save();

			var notificationList = ProcessLogs();

			var subscriberLogAfterProcessing = new BusinessObjectFactory().Load<IQueuedLog>(subscriberLog.PK);
			AssertEquals("Postcondition: log status after processing", JobQueueStatus.StatusFailed, subscriberLogAfterProcessing.SJ_Status);
			var expectedErrorMessaage = "triggerAction is invalid.";
			AssertContains("Correct Notifications are used", expectedErrorMessaage, notificationList);
			AssertNotNull("LastExceptionReported", ErrorReporter.LastExceptionReported);
			AssertEquals("LastExceptionReported.Message", expectedErrorMessaage, ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestInvalidLogParameters_InvalidTriggerAction_Case2()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var queuedLogMock = GetQueuedLog();
			var subscriberLog = NewsTransmitter.CreateStmJobQueue(Factory, queuedLogMock.Object, new TriggerActionWithOptionalFactorySaveLogSubscriber(), Events.MiscellaneousEventCode, TestObjectCreator.Staff.GS_Code, TestObjectCreator.NonCurrentBranch.GB_Code, TestObjectCreator.NonCurrentDepartment.GE_Code,
				$"|PPK={dummy.PK}|PTP=Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business|TRA=");
			Factory.Save();

			var notificationList = ProcessLogs();

			var subscriberLogAfterProcessing = new BusinessObjectFactory().Load<IQueuedLog>(subscriberLog.PK);
			AssertEquals("Postcondition: log status after processing", JobQueueStatus.StatusFailed, subscriberLogAfterProcessing.SJ_Status);
			var expectedErrorMessaage = "triggerAction is invalid.";
			AssertContains("Correct Notifications are used", expectedErrorMessaage, notificationList);
			AssertNotNull("LastExceptionReported", ErrorReporter.LastExceptionReported);
			AssertEquals("LastExceptionReported.Message", expectedErrorMessaage, ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestInvalidLogParameters_CheckLogStatus_WhenParentIsInvalid()
		{
			var parentPK = "9f162354-1062-49b6-901c-67176c04fc84";
			var expectedLogMessaage = $"[AccTriggerWithOptSaveLogSubscriber] parent is invalid. It might be deleted. ParentPK : {parentPK}, parentBizoType : Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, triggerAction : {WorkflowTriggerActionType}";

			var queuedLogMock = GetQueuedLog();
			var subscriberLog = NewsTransmitter.CreateStmJobQueue(Factory, queuedLogMock.Object, new TriggerActionWithOptionalFactorySaveLogSubscriber(), Events.MiscellaneousEventCode, TestObjectCreator.Staff.GS_Code, TestObjectCreator.NonCurrentBranch.GB_Code, TestObjectCreator.NonCurrentDepartment.GE_Code,
				$"|PPK={parentPK}|PTP=Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business|TRA={WorkflowTriggerActionType}");
			Factory.Save();

			var notifications = RunLogWalkerCycleForTestWithoutPreAndPostConditionChecks();

			var subscriberLogAfterProcessing = new BusinessObjectFactory().Load<IQueuedLog>(subscriberLog.PK);
			AssertEquals("Postcondition: log status after processing", JobQueueStatus.StatusProcessed, subscriberLogAfterProcessing.SJ_Status);

			Assert("Log Message in Notifications list", !notifications.Contains(expectedLogMessaage));

			subscriberLog = NewsTransmitter.CreateStmJobQueue(Factory, queuedLogMock.Object, new TriggerActionWithOptionalFactorySaveLogSubscriber(), Events.MiscellaneousEventCode, TestObjectCreator.Staff.GS_Code, TestObjectCreator.NonCurrentBranch.GB_Code, TestObjectCreator.NonCurrentDepartment.GE_Code,
				$"|PPK={parentPK}|PTP=Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business|TRA={WorkflowTriggerActionType}");
			Factory.Save();

			var logger = new LoggerForTesting();
			var notificationsWithDebugMessages = RunLogWalkerCycleForTestWithoutPreAndPostConditionChecks(logger);

			subscriberLogAfterProcessing = new BusinessObjectFactory().Load<IQueuedLog>(subscriberLog.PK);
			AssertEquals("Postcondition: log status after processing", JobQueueStatus.StatusProcessed, subscriberLogAfterProcessing.SJ_Status);

			Assert("Log Message in Notifications list", notificationsWithDebugMessages.Contains(expectedLogMessaage));
		}

		public void TestInvalidLogParameters_InvalidWorkflowProvider()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var queuedLogMock = GetQueuedLog();
			var subscriberLog = NewsTransmitter.CreateStmJobQueue(Factory, queuedLogMock.Object, new TriggerActionWithOptionalFactorySaveLogSubscriber(), Events.MiscellaneousEventCode, TestObjectCreator.Staff.GS_Code, TestObjectCreator.NonCurrentBranch.GB_Code, TestObjectCreator.NonCurrentDepartment.GE_Code,
				$"|PPK={dummy.PK}|PTP=CargoWise.EntityFramework.Testing.DummyBusinessObject, CargoWise.EntityFramework|TRA={WorkflowTriggerActionType}");
			Factory.Save();

			var notificationList = ProcessLogs();

			var subscriberLogAfterProcessing = new BusinessObjectFactory().Load<IQueuedLog>(subscriberLog.PK);
			AssertEquals("Postcondition: log status after processing", JobQueueStatus.StatusFailed, subscriberLogAfterProcessing.SJ_Status);
			var expectedErrorMessaage = "workflowProvider is invalid.";
			AssertContains("Correct Notifications are used", expectedErrorMessaage, notificationList);
			AssertNotNull("LastExceptionReported", ErrorReporter.LastExceptionReported);
			AssertEquals("LastExceptionReported.Message", expectedErrorMessaage, ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestTriggerCompanyPKDeserialization_EmptyPK()
		{
			CreateLogSubscriber("");
			var mockOtherCompanyAPInvoiceImportCreator = new Mock<IOtherCompanyAPInvoiceImportCreator>();
			ObjectFactory.Substitute(mockOtherCompanyAPInvoiceImportCreator.Object);
			ProcessLogs();
			mockOtherCompanyAPInvoiceImportCreator.Verify(x => x.CreateOtherCompanyAPInvoiceImport(It.IsAny<IWorkflowProvider>(), ZGuid.Empty), Times.Once);
			Assert(true);
		}

		public void TestTriggerCompanyPKDeserialization_EmptyGUID()
		{
			CreateLogSubscriber(ZGuid.Empty.ToString());
			var mockOtherCompanyAPInvoiceImportCreator = new Mock<IOtherCompanyAPInvoiceImportCreator>();
			ObjectFactory.Substitute(mockOtherCompanyAPInvoiceImportCreator.Object);
			ProcessLogs();
			mockOtherCompanyAPInvoiceImportCreator.Verify(x => x.CreateOtherCompanyAPInvoiceImport(It.IsAny<IWorkflowProvider>(), ZGuid.Empty), Times.Once);
			Assert(true);
		}

		public void TestTriggerCompanyPKDeserialization_ValidGUID()
		{
			var randomCompanyPK = ZGuid.NewZGuid();
			CreateLogSubscriber(randomCompanyPK.ToString());
			var mockOtherCompanyAPInvoiceImportCreator = new Mock<IOtherCompanyAPInvoiceImportCreator>();
			ObjectFactory.Substitute(mockOtherCompanyAPInvoiceImportCreator.Object);
			ProcessLogs();
			mockOtherCompanyAPInvoiceImportCreator.Verify(x => x.CreateOtherCompanyAPInvoiceImport(It.IsAny<IWorkflowProvider>(), randomCompanyPK), Times.Once);
			Assert(true);
		}

		public void TestTriggerCompanyPKDeserialization_InvalidGUID()
		{
			CreateLogSubscriber("Invalid GUID");
			var mockOtherCompanyAPInvoiceImportCreator = new Mock<IOtherCompanyAPInvoiceImportCreator>();
			ObjectFactory.Substitute(mockOtherCompanyAPInvoiceImportCreator.Object);
			ProcessLogs();
			AssertEquals("triggerCompanyPK is not a valid GUID.", ErrorReporter.LastExceptionReported.Message);
			mockOtherCompanyAPInvoiceImportCreator.Verify(x => x.CreateOtherCompanyAPInvoiceImport(It.IsAny<IWorkflowProvider>(), ZGuid.Empty), Times.Never);
			ErrorReporter.Clear();
		}

		TriggerActionWithOptionalFactorySaveLogSubscriber CreateLogSubscriber(string companyPKString)
		{
			var dummyWorkflow = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var queuedLogMock = GetQueuedLog();
			var logSubscriber = new TriggerActionWithOptionalFactorySaveLogSubscriber();
			var subscriberLog = CreateStmJobQueue(companyPKString, queuedLogMock.Object, dummyWorkflow.PK, logSubscriber);
			Factory.Save();
			return logSubscriber;
		}

		static Mock<IQueuedLog> GetQueuedLog()
		{
			var queuedLogMock = new Mock<IQueuedLog>();
			queuedLogMock.SetupGet(x => x.SJ_EventTime).Returns(ZDateTime.Today);
			queuedLogMock.SetupGet(x => x.SJ_EventTimeUtc).Returns(ZDateTime.UtcToday);
			queuedLogMock.SetupGet(x => x.SJ_ParentTableCode).Returns(DummyWithWorkflow.Schema.TablePrefix);
			return queuedLogMock;
		}

		IQueuedLog CreateStmJobQueue(string companyPKString, IQueuedLog parentLog, ZGuid workflowPK, TriggerActionWithOptionalFactorySaveLogSubscriber logSubscriber)
		{
			return NewsTransmitter.CreateStmJobQueue(Factory, parentLog, logSubscriber,
					Events.MiscellaneousEventCode,
					TestObjectCreator.Staff.GS_Code,
					TestObjectCreator.NonCurrentBranch.GB_Code,
					TestObjectCreator.NonCurrentDepartment.GE_Code,
					$"|CMP={companyPKString}|PPK={workflowPK}|PTP=Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business|TRA={WorkflowTriggerActionTypeConstants.Codes.ImportAPInvoicesFromOtherCompanies}");
		}

		protected abstract string WorkflowTriggerActionType { get; }
		protected abstract void SetupTriggerProcessorMock(IWorkflowProvider provider, IProcessor processor);

		string ProcessLogs()
		{
			return new ZStringBuilder(RunLogWalkerCycleForTestWithoutPreAndPostConditionChecks()).ToStringWithNewLineBetweenAppends();
		}
		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
