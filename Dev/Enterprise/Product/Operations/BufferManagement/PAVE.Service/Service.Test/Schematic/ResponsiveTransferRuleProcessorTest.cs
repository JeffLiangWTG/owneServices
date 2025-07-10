using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.BufferManagement.Service.Test
{
	public class ResponsiveTransferRuleProcessorTest : BMSTestCaseWithFactory
	{
		#region Log

		public void TestProcessTransferables_ShouldLogWhenStartAndFinish()
		{
			var dummyTransferablePKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
			var processor = new DummyResponsiveTransferRuleProcessor(loggerMock.Object);

			processor.ProcessTransferables(dummyTransferablePKs);

			AssertContainsExactElementsInExactOrder([
				(LogType.Information, "Started processing transfer rules using ResponsiveTransferRuleProcessor"),
				(LogType.Information, "Finished processing transfer rules using ResponsiveTransferRuleProcessor")
			], logs.ToArray());
			AssertContainsExactElementsInAnyOrder(dummyTransferablePKs, processor.TransferablePKsProcessed.Single());
		}

		public void TestProcessTransferables_ShouldStillLogWhenStartAndFinish_WhenException()
		{
			var system = CreateSystem();

			var bucketFrom = CreateBucket(system);
			var workflows = CreateWorkflows(bucketFrom, 2);

			Factory.Save();

			var workflowsPks = workflows.Select(w => w.PK.ToGuid()).ToArray();
			var processor = new DummyResponsiveTransferRuleProcessor(loggerMock.Object)
			{
				ThrowException = new Exception("Oh no something went wrong!")
			};

			AssertExceptionThrown<Exception>(() => processor.ProcessTransferables(workflowsPks));

			AssertContainsExactElementsInExactOrder([
				(LogType.Information, "Started processing transfer rules using ResponsiveTransferRuleProcessor"),
				(LogType.Debug, "ZJ321WUNM8ISO6VDSJSPUZX5LL7F60: 2 workflows to process [ResponsiveTransferRuleRunner]"),
				(LogType.Information, "Finished processing transfer rules using ResponsiveTransferRuleProcessor")
			], logs.ToArray());
			AssertEquals(false, processor.TransferablePKsProcessed.Any());
		}

		#endregion

		#region Process

		public void TestProcessTransferable_ShouldEnableSuppressCreatingWorkflowFromTemplate_WhenStart_AndDisable_WhenFinish()
		{
			var dummyTransferablePKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
			var processor = new DummyResponsiveTransferRuleProcessor(loggerMock.Object);

			AssertEquals(false, ProcessTask.Loader.SuppressCreatingWorkflowFromTemplate);

			processor.ProcessTransferables(dummyTransferablePKs);

			AssertEquals(true, processor.SuppressCreatingWorkflowFromTemplate_WhenProcessTransferables);
			AssertEquals(false, ProcessTask.Loader.SuppressCreatingWorkflowFromTemplate);
		}

		#endregion

		#region ProcessCore

		#region GetLiveSystems

		public void TestProcessTransferable_ShouldOnlyLoadLiveSystems()
		{
			var liveSystem = CreateSystem();

			var notLiveSystem = CreateSystem();
			notLiveSystem.FS_IsLive = false;

			var bucketFromInLiveSystem = CreateBucket(liveSystem);
			var bucketFromInNotLiveSystem = CreateBucket(notLiveSystem);
			var workflowsInLiveSystem = CreateWorkflows(bucketFromInLiveSystem, 2);
			var workflowsInNotLiveSystem = CreateWorkflows(bucketFromInNotLiveSystem, 2);
			var allWorkflowPKstoProcess = workflowsInLiveSystem.Select(w => w.PK.ToGuid())
				.Union(workflowsInNotLiveSystem.Select(w => w.PK.ToGuid())).ToArray();

			Factory.Save();

			var processor = new DummyResponsiveTransferRuleProcessor(loggerMock.Object);

			processor.ProcessTransferables(allWorkflowPKstoProcess);

			AssertEquals(1, processor.CreatedRunners.Count);
			AssertContainsExactElementsInAnyOrder(allWorkflowPKstoProcess, processor.TransferablePKsProcessed.Single());
			AssertEquals(liveSystem.PK, processor.CreatedRunners.Single().System.PK.ToGuid());
		}

		public void TestProcessTransferable_ShouldOnlyLoadSystemsWithActiveComponents()
		{
			var systemWithActiveComponent = CreateSystem();

			var systemWithoutActiveComponent = CreateSystem();

			var activeBucket = CreateBucket(systemWithActiveComponent);
			var inactiveBucket = CreateBucket(systemWithoutActiveComponent);
			inactiveBucket.FC_IsActive = false;
			var workflowsInActiveComponent = CreateWorkflows(activeBucket, 2);
			var workflowsIninactiveComponent = CreateWorkflows(inactiveBucket, 2);
			var allWorkflowPKstoProcess = workflowsInActiveComponent.Select(w => w.PK.ToGuid())
				.Union(workflowsIninactiveComponent.Select(w => w.PK.ToGuid())).ToArray();

			Factory.Save();

			var processor = new DummyResponsiveTransferRuleProcessor(loggerMock.Object);
			processor.ProcessTransferables(allWorkflowPKstoProcess);

			AssertEquals(1, processor.CreatedRunners.Count);
			AssertContainsExactElementsInAnyOrder(allWorkflowPKstoProcess, processor.TransferablePKsProcessed.Single());
			AssertEquals(systemWithActiveComponent.PK, processor.CreatedRunners.Single().System.PK.ToGuid());
		}

		public void TestProcessTransferable_ShouldOnlyLoadSystemsWithLiveProcessHeaders()
		{
			var systemWithActiveProcessHeaders = CreateSystem();

			var systemWithoutActiveProcessHeaders = CreateSystem();

			var systemWithNotProcessedProcessHeaders = CreateSystem();

			var bucketWithActiveProcessHeaders = CreateBucket(systemWithActiveProcessHeaders);
			var bucketWithoutActiveProcessHeaders = CreateBucket(systemWithoutActiveProcessHeaders);
			var bucketWithNotProcessedProcessHeaders = CreateBucket(systemWithNotProcessedProcessHeaders);

			var activeWorkflows = CreateWorkflows(bucketWithActiveProcessHeaders, 3);
			var inactiveWorkflows = CreateWorkflows(bucketWithoutActiveProcessHeaders, 2);
			inactiveWorkflows[0].FH_IsActive = false;
			inactiveWorkflows[1].FH_IsActive = false;

			CreateWorkflows(bucketWithNotProcessedProcessHeaders, 2);

			//only: activeWorkflows + inactiveWorkflows
			//not added notProccessedWorkflows to assert we not process all systems
			var allWorkflowPKstoProcess = activeWorkflows.Select(w => w.PK.ToGuid())
				.Union(inactiveWorkflows.Select(w => w.PK.ToGuid())).ToArray();

			Factory.Save();

			var processor = new DummyResponsiveTransferRuleProcessor(loggerMock.Object);

			processor.ProcessTransferables(allWorkflowPKstoProcess);

			AssertEquals(1, processor.CreatedRunners.Count);
			AssertContainsExactElementsInAnyOrder(allWorkflowPKstoProcess, processor.TransferablePKsProcessed.Single());
			AssertEquals(systemWithActiveProcessHeaders.PK, processor.CreatedRunners.Single().System.PK.ToGuid());
		}

		public void TestProcessTransferable_ShouldLoadSystemsIgnoringUberFactoryCache()
		{
			var system1 = CreateSystem();
			var system2 = CreateSystem();
			var bucket1 = CreateBucket(system1);
			var bucket2 = CreateBucket(system2);
			var workflowsInBucket1 = CreateWorkflows(bucket1, 2);
			var workflowsInBucket2 = CreateWorkflows(bucket2, 2);
			var allWorkflowPKs = workflowsInBucket1.Union(workflowsInBucket2).Select(w => w.PK.ToGuid()).ToArray();

			Factory.Save();

			using (var command = Db.Connection.Command($@"
UPDATE dbo.BMComponent
SET FC_IsActive = 0,
	FC_SystemLastEditTimeUtc = getdate(),
	FC_SystemLastEditUser = '~BP'
WHERE FC_PK = '{bucket2.PK}'
"))
			{
				command.ExecuteNonQuery();
			}

			var processor = new DummyResponsiveTransferRuleProcessor(loggerMock.Object);
			processor.ProcessTransferables(allWorkflowPKs);

			AssertContainsExactElementsInAnyOrder("Should not process workflows in inactive components - should use BMComponent data read from db ignoring the Uber Factory cache",
				[system1.PK], processor.CreatedRunners.Select(r => r.System.PK));

			using (var command = Db.Connection.Command($@"
UPDATE dbo.BMComponent
SET FC_IsActive = 1,
	FC_SystemLastEditTimeUtc = getdate(),
	FC_SystemLastEditUser = '~BP'
WHERE FC_PK = '{bucket2.PK}'
"))
			{
				command.ExecuteNonQuery();
			}

			processor = new DummyResponsiveTransferRuleProcessor(loggerMock.Object);
			processor.ProcessTransferables(allWorkflowPKs);

			AssertContainsExactElementsInAnyOrder("Should now process all workflows - should use BMComponent data read from db ignoring the Uber Factory cache",
				[system1.PK, system2.PK], processor.CreatedRunners.Select(r => r.System.PK));
		}

		#endregion

		#endregion

		#region SetUp

		#region Dummies

		class DummyResponsiveTransferRuleRunner : ResponsiveTransferRuleRunner
		{
			readonly Exception throwException;

			public DummyResponsiveTransferRuleRunner(
					IPAVESystem system,
					ITransferRuleRunnerDataAccessor dataAccessor,
					ITransferRuleRunnerLogger logger,
					IReadOnlyCollection<Guid> transferablesPKs,
					Exception throwException)
				: base(system, dataAccessor, logger, transferablesPKs)
			{
				this.throwException = throwException;
			}

			public new BMSystem System => (BMSystem)base.System;

			protected override void ProcessCore(CancellationToken token)
			{
				if (throwException != null)
				{
					throw throwException;
				}
			}
		}

		class DummyResponsiveTransferRuleProcessor : ResponsiveTransferRuleProcessor
		{
			public bool? SuppressCreatingWorkflowFromTemplate_WhenProcessTransferables { get; private set; }
			public List<IReadOnlyCollection<Guid>> TransferablePKsProcessed { get; } = new List<IReadOnlyCollection<Guid>>();
			public Exception ThrowException { get; set; }

			public List<DummyResponsiveTransferRuleRunner> CreatedRunners { get; } = new List<DummyResponsiveTransferRuleRunner>();

			internal DummyResponsiveTransferRuleProcessor(ILogger logger) : base(logger)
			{
			}

			protected override ResponsiveTransferRuleRunner GetNewResponsiveTransferRuleRunnerCore(
				IPAVESystem system,
				ITransferRuleRunnerDataAccessor dataAccessor,
				ITransferRuleRunnerLogger logger,
				IReadOnlyCollection<Guid> transferablesPKs)
			{
				var newRunner = new DummyResponsiveTransferRuleRunner(system, dataAccessor, logger, transferablesPKs, ThrowException);

				CreatedRunners.Add(newRunner);

				return newRunner;
			}

			protected override void ProcessTransferablesCore(IReadOnlyCollection<Guid> transferablePKs)
			{
				SuppressCreatingWorkflowFromTemplate_WhenProcessTransferables = ProcessTask.Loader.SuppressCreatingWorkflowFromTemplate;

				base.ProcessTransferablesCore(transferablePKs);

				TransferablePKsProcessed.Add(transferablePKs);
			}
		}

		#endregion

		readonly Mock<ILogger> loggerMock = new Mock<ILogger>();
		readonly List<(LogType type, string message)> logs = new List<(LogType, string)>();
		readonly IDisposable disableAsyncBehaviourDisposable = BMSTestCaseWithFactory.DisableAsyncBehaviour();

		protected override void SetUp()
		{
			base.SetUp();

			loggerMock.Setup(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType type, string message) => logs.Add((type, message)));
		}

		protected override void TearDown()
		{
			disableAsyncBehaviourDisposable.Dispose();
		}

		#endregion
	}
}
