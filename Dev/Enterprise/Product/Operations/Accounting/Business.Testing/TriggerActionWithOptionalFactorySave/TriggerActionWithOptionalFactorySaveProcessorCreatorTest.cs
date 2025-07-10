using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave.Testing
{
	class TriggerActionWithOptionalFactorySaveProcessorCreatorTest : TestCaseWithFactory
	{
		public void TestObjectFactoryDeclaration()
		{
			AssertType(typeof(TriggerActionWithOptionalFactorySaveProcessorCreator), ObjectFactory.Get<IAccountingTriggerActionWithOptionalFactorySaveProcessorCreator>());
		}

		public void TestProcessor()
		{
			var parentId = new ZGuid("72A2710E-2219-4E60-A214-B51C95AE3036");
			var bizo = Factory.New(typeof(DummyBusinessObject), new Guid("432AA4C8-B4D0-4D59-B1C5-00632690EC66"));
			var queuedLogMock = new Mock<IQueuedLog>();
			var logFactory = new BusinessObjectFactory();
			queuedLogMock.SetupGet(x => x.SJ_ParentID).Returns(parentId);
			queuedLogMock.SetupGet(x => x.Factory).Returns(logFactory);
			var currentCompanyPK = Env.CurrentCompanyPK;
			var triggerAction = CreateTriggerWithAction(currentCompanyPK, "1X1");

			var processor = GetCreator().Create(triggerAction, bizo, queuedLogMock.Object);

			using (Env.SetTemporaryUserContext(TestObjectCreator.Staff.PK.ToGuid(), TestObjectCreator.NonCurrentBranch.PK.ToGuid(), TestObjectCreator.NonCurrentDepartment.PK.ToGuid()))
			{
				processor.Process(null);
			}

			Func<BusinessObjectFactory, IQueuedLog[]> getLogs = f => f.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, parentId));
			var queuedLogs = getLogs(Factory);
			AssertEquals("Number of queued logs created in main Factory.", 0, queuedLogs.Length);
			queuedLogs = getLogs(logFactory);
			AssertEquals("Number of queued logs created in log Factory.", 1, queuedLogs.Length);
			var queuedLog = queuedLogs[0];
			CombineAssertions(() =>
			{
				AssertEquals("SJ_FilterName", new TriggerActionWithOptionalFactorySaveLogSubscriber().Name, ((BusinessObject)queuedLog)[StmJobQueueSchema.SJ_FilterName.Name]);
				AssertEquals("SJ_SE_NKEvent", Events.MiscellaneousEventCode, queuedLog.SJ_SE_NKEvent);
				AssertEquals("SJ_GS_NKUser", TestObjectCreator.Staff.GS_Code, queuedLog.SJ_GS_NKUser);
				AssertEquals("SJ_GB_NKBranch", TestObjectCreator.NonCurrentBranch.GB_Code, queuedLog.SJ_GB_NKBranch);
				AssertEquals("SJ_GE_NKDepartment", TestObjectCreator.NonCurrentDepartment.GE_Code, queuedLog.SJ_GE_NKDepartment);
				AssertEquals("SJ_IsCancelled", false, ((BusinessObject)queuedLog)[StmJobQueueSchema.SJ_IsCancelled.Name]);
				AssertEquals("SJ_Status", JobQueueStatus.StatusQueued, queuedLog.SJ_Status);

				AssertEquals("SJ_Reference", $"|PPK=432aa4c8-b4d0-4d59-b1c5-00632690ec66|PTP=CargoWise.EntityFramework.Testing.DummyBusinessObject, CargoWise.EntityFramework|TRA=1X1", queuedLog.SJ_Reference);
			});
		}

		ProcessTaskNotification CreateTriggerWithAction(Guid currentCompanyPK, string triggerActionType)
		{
			var triggerAction = Factory.NewWithValidTestData<ProcessTaskNotification>();
			triggerAction.PQ_TriggerType = triggerActionType;
			var trigger = Factory.Load<ProcessTask>(triggerAction.PQ_P9);
			trigger.P9_GC = currentCompanyPK;
			return triggerAction;
		}

		IAccountingTriggerActionWithOptionalFactorySaveProcessorCreator GetCreator() => new TriggerActionWithOptionalFactorySaveProcessorCreator();
		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
