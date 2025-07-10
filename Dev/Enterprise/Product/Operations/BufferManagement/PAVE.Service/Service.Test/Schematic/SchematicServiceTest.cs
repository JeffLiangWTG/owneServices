using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Service.Test
{
	public class SchematicServiceTest : SimplePAVEServiceTestCase
	{
		#region ProcessTransferRules

		public void TestProcessTransferRules_ShouldLog_WhenProcessorThrowException()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var schematicService = new DummySchematicService
			{
				ThrowException = new Exception("Oh no something went wrong!")
			};

			var dummyTransferablePKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };

			AssertEquals(false, ProcessTask.Loader.SuppressCreatingWorkflowFromTemplate);

			AssertNoExceptionThrown("Should not throw exception", () => schematicService.ProcessTransferRules(dummyTransferablePKs, loggerMock.Object));

			AssertEquals(3, logs.Count);
			AssertEquals((LogType.Information, "Started processing transfer rules using ResponsiveTransferRuleProcessor"), logs.First());
			AssertEquals((LogType.Information, "Finished processing transfer rules using ResponsiveTransferRuleProcessor"), logs[1]);
			AssertEquals((LogType.Information, $"Error when call {nameof(DummySchematicService)}.ProcessTransferRules, transferablePKs: {dummyTransferablePKs[0]},{dummyTransferablePKs[1]} Exception: Oh no something went wrong!"), logs.Last());

			AssertEquals(false, schematicService.DummyProcessor.TransferablePKsProcessed.Any());
			AssertEquals(true, schematicService.DummyProcessor.SuppressCreatingWorkflowFromTemplate_WhenProcessTransferables);
			AssertEquals(false, ProcessTask.Loader.SuppressCreatingWorkflowFromTemplate);
		}

		public void TestProcessTransferRules_ShouldRunProcessor()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var schematicService = new DummySchematicService();
			var dummyTransferablePKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };

			AssertEquals(false, ProcessTask.Loader.SuppressCreatingWorkflowFromTemplate);

			schematicService.ProcessTransferRules(dummyTransferablePKs, loggerMock.Object);

			AssertEquals(2, logs.Count);
			AssertEquals((LogType.Information, "Started processing transfer rules using ResponsiveTransferRuleProcessor"), logs.First());
			AssertContainsExactElementsInAnyOrder(dummyTransferablePKs, schematicService.DummyProcessor.TransferablePKsProcessed.Single());
			AssertEquals((LogType.Information, "Finished processing transfer rules using ResponsiveTransferRuleProcessor"), logs.Last());

			AssertEquals(true, schematicService.DummyProcessor.SuppressCreatingWorkflowFromTemplate_WhenProcessTransferables);
			AssertEquals(false, ProcessTask.Loader.SuppressCreatingWorkflowFromTemplate);
		}

		#endregion

		#region Dummies

		class DummySchematicService : SchematicService
		{
			public Exception ThrowException { get; set; }
			public DummyResponsiveTransferRuleProcessor DummyProcessor { get; private set; }

			protected internal override ResponsiveTransferRuleProcessor GetNewResponsiveTransferRuleProcessor(ILogger logger)
			{
				DummyProcessor = new DummyResponsiveTransferRuleProcessor(logger, ThrowException);
				return DummyProcessor;
			}
		}

		class DummyResponsiveTransferRuleProcessor : ResponsiveTransferRuleProcessor
		{
			public List<IReadOnlyCollection<Guid>> TransferablePKsProcessed { get; } = new List<IReadOnlyCollection<Guid>>();
			public bool? SuppressCreatingWorkflowFromTemplate_WhenProcessTransferables { get; private set; }

			readonly Exception throwException;

			internal DummyResponsiveTransferRuleProcessor(ILogger logger, Exception throwException) : base(logger)
			{
				this.throwException = throwException;
			}

			protected override void ProcessTransferablesCore(IReadOnlyCollection<Guid> transferablePKs)
			{
				SuppressCreatingWorkflowFromTemplate_WhenProcessTransferables = ProcessTask.Loader.SuppressCreatingWorkflowFromTemplate;

				if (throwException != null)
				{
					throw throwException; //Lol throw throw
				}

				TransferablePKsProcessed.Add(transferablePKs);
			}
		}

		#endregion

		#region Test Case Overrides

		protected override IPAVEService GetService() => new SchematicService();

		protected override void CallServiceMethod(IPAVEService service, IReadOnlyCollection<Guid> processedPKs, ILogger logger) => ((SchematicService)service).ProcessTransferRules(processedPKs, logger);

		protected override string ExpectedClassNameInLogs => nameof(SchematicService);

		protected override string ExpectedMethodNameInLogs => "ProcessTransferRules";

		#endregion
	}
}
