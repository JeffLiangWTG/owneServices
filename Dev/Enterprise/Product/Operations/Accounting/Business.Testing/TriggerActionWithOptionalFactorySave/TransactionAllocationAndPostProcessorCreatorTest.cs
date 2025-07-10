using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave.Testing
{
	class TransactionAllocationAndPostProcessorCreatorTest : TestCaseWithFactory
	{
		public void TestObjectFactoryDeclaration()
		{
			AssertType<TransactionAllocationAndPostProcessorCreator>(ObjectFactory.Get<ITransactionAllocationAndPostProcessorCreator>());
		}

		public void TestProcessorForATP_WithoutSecurityRightsForTransactionAllocateAndPostTrigger()
		{
			var parentId = new ZGuid("72A2710E-2219-4E60-A214-B51C95AE3036");
			var bizo = Factory.New(typeof(DummyBusinessObject), new Guid("432AA4C8-B4D0-4D59-B1C5-00632690EC66"));
			var logFactory = new BusinessObjectFactory();
			var queuedLogMock = CreateQueuedLog(logFactory, parentId);
			var currentCompanyPK = Env.CurrentCompanyPK;
			var triggerAction = CreateTriggerWithAction(currentCompanyPK, "ATP");

			var processor = GetCreator().Create(triggerAction, bizo, queuedLogMock);

			using (Env.SetTemporaryUserContext(TestObjectCreator.Staff.PK.ToGuid(), TestObjectCreator.NonCurrentBranch.PK.ToGuid(), TestObjectCreator.NonCurrentDepartment.PK.ToGuid()))
			{
				processor.Process(null);
			}

			var queuedLogs = GetAndAssertQueuedLogNumber(logFactory, parentId);
			AssertCreatedQueuedLog(queuedLogs[0], TestObjectCreator.Staff.GS_Code, TestObjectCreator.NonCurrentBranch.GB_Code, TestObjectCreator.NonCurrentDepartment.GE_Code,
				$"|PPK=432aa4c8-b4d0-4d59-b1c5-00632690ec66|PTP=CargoWise.EntityFramework.Testing.DummyBusinessObject, CargoWise.EntityFramework|TRA=ATP");
		}

		public void TestProcessorForATP_WithSecurityRightsForTransactionAllocateAndPostTrigger()
		{
			var parentId = new ZGuid("72A2710E-2219-4E60-A214-B51C95AE3036");
			var bizo = Factory.New(typeof(DummyBusinessObject), new Guid("432AA4C8-B4D0-4D59-B1C5-00632690EC66"));
			var logFactory = new BusinessObjectFactory();
			var queuedLogMock = CreateQueuedLog(logFactory, parentId);

			var staff = TestObjectCreator.GS1;
			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());
			var currentCompanyPK = Env.CurrentCompanyPK;
			var triggerAction = CreateTriggerWithAction(currentCompanyPK, "ATP");

			var processor = GetCreator().Create(triggerAction, bizo, queuedLogMock);

			using (Env.SetTemporaryUserContext(TestObjectCreator.Staff.PK.ToGuid(), TestObjectCreator.NonCurrentBranch.PK.ToGuid(), TestObjectCreator.NonCurrentDepartment.PK.ToGuid()))
			{
				processor.Process(null);
			}

			var queuedLogs = GetAndAssertQueuedLogNumber(logFactory, parentId);
			AssertCreatedQueuedLog(queuedLogs[0], staff.GS_Code, staff.HomeBranch.GB_Code, staff.HomeDepartment.GE_Code,
				$"|PPK=432aa4c8-b4d0-4d59-b1c5-00632690ec66|PTP=CargoWise.EntityFramework.Testing.DummyBusinessObject, CargoWise.EntityFramework|TRA=ATP");
		}

		ProcessTaskNotification CreateTriggerWithAction(Guid currentCompanyPK, string triggerActionType)
		{
			var triggerAction = Factory.NewWithValidTestData<ProcessTaskNotification>();
			triggerAction.PQ_TriggerType = triggerActionType;
			var trigger = Factory.Load<ProcessTask>(triggerAction.PQ_P9);
			trigger.P9_GC = currentCompanyPK;
			return triggerAction;
		}

		IQueuedLog[] GetLogs(BusinessObjectFactory factory,ZGuid parentId)
		{
			return factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, parentId));
		}

		IQueuedLog CreateQueuedLog(BusinessObjectFactory logFactory, ZGuid parentId)
		{
			var queuedLogMock = new Mock<IQueuedLog>();
			queuedLogMock.SetupGet(x => x.SJ_ParentID).Returns(parentId);
			queuedLogMock.SetupGet(x => x.Factory).Returns(logFactory);
			return queuedLogMock.Object;
		}

		IQueuedLog[] GetAndAssertQueuedLogNumber(BusinessObjectFactory logFactory,ZGuid parentId)
		{
			var queuedLogs = GetLogs(Factory, parentId);
			AssertEquals("Number of queued logs created in main Factory.", 0, queuedLogs.Length);
			queuedLogs = GetLogs(logFactory, parentId);
			AssertEquals("Number of queued logs created in log Factory.", 1, queuedLogs.Length);
			return queuedLogs;
		}

		void AssertCreatedQueuedLog(IQueuedLog queuedLog, ZString expectUserCode, ZString expectBranch, ZString expectDepartment, string expectReference)
		{
			AssertEquals("SJ_GS_NKUser", expectUserCode, queuedLog.SJ_GS_NKUser);
			AssertEquals("SJ_GB_NKBranch", expectBranch, queuedLog.SJ_GB_NKBranch);
			AssertEquals("SJ_GE_NKDepartment", expectDepartment, queuedLog.SJ_GE_NKDepartment);
			AssertEquals("SJ_Reference", expectReference, queuedLog.SJ_Reference);
		}

		ITransactionAllocationAndPostProcessorCreator GetCreator() => new TransactionAllocationAndPostProcessorCreator();
		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
