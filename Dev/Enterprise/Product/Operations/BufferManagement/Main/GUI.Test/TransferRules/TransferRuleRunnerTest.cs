using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class TransferRuleRunnerTest : BMSTestCaseWithFactory
	{
		#region Skip Links
		public void TestProcess_ShouldSkipLink_WhenThereIsNoFilter()
		{
			var aetChecker = new Mock<IResponsiveManagementServiceTaskChecker>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(aetChecker.Object))
			{
				aetChecker.Setup(s => s.AreDependentServiceTasksActive()).Returns(true);
				BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
				var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
				var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
				var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

				var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
				var workflow = jobHeader.ProcessHeaders[0];
				var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);
				task.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();

				workflow.FH_FC_CurrentComponent = bucket1.PK;

				Factory.Save();

				var logger = new BufferManagementLogger();
				var runner = new TestTransferRuleRunner(system, logger);
				runner.Process_ForTest();

				AssertContains("1 component link(s) were skipped", logger.ToString());
				AssertEquals("The work flow with no filter defined in link should not be transfered", workflow.FH_FC_CurrentComponent, bucket1.PK);
			}
		}

		public void TestProcess_ShouldNotSkipLink_WhenDynamicallyFilterTransferRulesIsFalse()
		{
			BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);
			task.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();

			workflow.FH_FC_CurrentComponent = bucket1.PK;

			BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(link.FilterRule);

			Factory.Save();

			var runner = new TestTransferRuleRunner(system, new DummyLogger());

			runner.Process_ForTest();

			AssertEquals(workflow.FH_FC_CurrentComponent, bucket2.PK);
		}

		public void TestProcess_ShouldNotSkipLink_When_Cdc_IsDisabled()
		{
			BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);
			task.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();

			workflow.FH_FC_CurrentComponent = bucket1.PK;

			BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(link.FilterRule);

			Factory.Save();

			var logger = new BufferManagementLogger();

			var mock = new Mock<ITransferRuleRunnerParams>();
			mock.Setup(b => b.IsCdcEnabled).Returns(false);

			var runner = new TestTransferRuleRunner(system, logger, mock.Object);
			runner.Process_ForTest();

			AssertContains("not skipped because CDC is not available or disabled", logger.ToString());
			AssertEquals(workflow.FH_FC_CurrentComponent, bucket2.PK);
		}

		public void TestProcess_ShouldNotSkipLink_When_TransferWorkflowComponentOnChanges_IsDisabled()
		{
			BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.TransferWorkflowComponentOnChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);
			task.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();

			workflow.FH_FC_CurrentComponent = bucket1.PK;

			BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(link.FilterRule);

			Factory.Save();

			var logger = new BufferManagementLogger();

			var mock = new Mock<ITransferRuleRunnerParams>();
			mock.Setup(b => b.IsCdcEnabled).Returns(true);

			var runner = new TestTransferRuleRunner(system, logger, mock.Object);
			runner.Process_ForTest();

			AssertContains("not skipped because responsive transfer is disabled just as safe guard", logger.ToString());
			AssertEquals(workflow.FH_FC_CurrentComponent, bucket2.PK);
		}

		public void TestProcess_ShouldNotSkipLink_When_ResponsiveTransfer_IsDisabled_JustAsSafeGuard()
		{
			BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			//set that responsive transfer is disabled just as safe guard
			BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);
			task.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();

			workflow.FH_FC_CurrentComponent = bucket1.PK;

			BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(link.FilterRule);

			Factory.Save();

			var logger = new BufferManagementLogger();
			var runner = new TestTransferRuleRunner(system, logger);

			runner.Process_ForTest();

			AssertContains("not skipped because responsive transfer is disabled just as safe guard", logger.ToString());
			AssertEquals(workflow.FH_FC_CurrentComponent, bucket2.PK);
		}

		public void TestProcess_ShouldNotSkipLinkAndLog_WhenProcessAllTransferRulesLinksOnNextBMSRunIsTrue_EvenWhenDynamicallyFilterTransferRulesIsTrue()
		{
			BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			var bucket3 = BMSTestHelper.CreateBucket(system, "b3");
			var link1 = BMSTestHelper.LinkComponents(bucket1, bucket2, sequence: 0);
			var link2 = BMSTestHelper.LinkComponents(bucket2, bucket3, sequence: 1);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);
			task.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();

			workflow.FH_FC_CurrentComponent = bucket1.PK;

			BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(link1.FilterRule);
			BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(link2.FilterRule);

			Factory.Save();

			var logger = new BufferManagementLogger();
			var runner = new TestTransferRuleRunner(system, logger);

			runner.Process_ForTest();

			AssertEquals("Workflow was transfered", workflow.FH_FC_CurrentComponent, bucket3.PK);
			Assert("Should not set ProcessAllTransferRulesLinksOnNextBMSRun to false, only after process all systems, see TestRun_ShouldSetProcessAllTransferRulesLinksOnNextBMSRunToFalse_WhenIsTrue on TransferRuleRunnerServiceTaskTest", BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.Value);

			var logText = logger.ToString();
			AssertContains(@"Component link [b1 -> b2, Sequence: 0]: not skipped because ProcessAllTransferRulesLinksOnNextBMSRun is true", logText);
			AssertContains(@"Component link [b2 -> b3, Sequence: 1]: not skipped because ProcessAllTransferRulesLinksOnNextBMSRun is true", logText);
		}

		public void TestProcess_ShouldSkipLinkAndLog_WhenDynamicallyFilterTransferRulesIsTrue()
		{
			var aetChecker = new Mock<IResponsiveManagementServiceTaskChecker>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(aetChecker.Object))
			{
				aetChecker.Setup(s => s.AreDependentServiceTasksActive()).Returns(true);
				BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
				var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
				var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
				var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

				var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
				var workflow = jobHeader.ProcessHeaders[0];

				workflow.FH_FC_CurrentComponent = bucket1.PK;

				BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(link.FilterRule);

				Factory.Save();

				var logger = new BufferManagementLogger();
				var runner = new TestTransferRuleRunner(system, logger);

				runner.Process_ForTest();

				AssertEquals("Workflow was NOT transfered", workflow.FH_FC_CurrentComponent, bucket1.PK);

				var logText = logger.ToString();
				AssertContains(@"1 component link(s) were skipped: [b1 -> b2, Sequence: 0]; they should be processed through Responsive Transfer", logText);
			}
		}

		public void TestProcessResponsive_ShouldNotSkipLink_WhenDynamicallyFilterTransferRulesIsTrue()
		{
			BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);
			task.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();

			workflow.FH_FC_CurrentComponent = bucket1.PK;

			BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(link.FilterRule);

			Factory.Save();

			var mock = new Mock<ITransferRuleRunnerParams>();
			mock.Setup(b => b.IsResponsive).Returns(true);
			mock.Setup(b => b.IsCdcEnabled).Returns(true);

			var runner = new TestTransferRuleRunner(system, new DummyLogger(), mock.Object);

			runner.Process_ForTest();

			AssertEquals("Workflow was transfered", workflow.FH_FC_CurrentComponent, bucket2.PK);
		}

		public void TestProcess_ShouldNotSkipLink_WhenHasAFilterThatNotTriggerResponsiveTransfer_EvenWhenDynamicallyFilterTransferRulesIsTrue()
		{
			BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);
			task.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();

			workflow.FH_FC_CurrentComponent = bucket1.PK;

			BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(link.FilterRule, ProcessHeader.ModuleFilterConstants.QueueStatus); //QueueStatus don't trigger responsive transfer

			Factory.Save();

			var logger = new BufferManagementLogger();
			var runner = new TestTransferRuleRunner(system, logger);

			runner.Process_ForTest();

			AssertEquals("Workflow was transfered", workflow.FH_FC_CurrentComponent, bucket2.PK);
		}

		public void TestProcess_ShouldSkipLink_WhenHasNoFilterAndIsNotRequired_EvenWhenDynamicallyFilterTransferRulesIsTrue()
		{
			var aetChecker = new Mock<IResponsiveManagementServiceTaskChecker>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(aetChecker.Object))
			{
				aetChecker.Setup(s => s.AreDependentServiceTasksActive()).Returns(true);
				BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
				var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
				var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
				var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

				var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
				var workflow = jobHeader.ProcessHeaders[0];

				workflow.FH_FC_CurrentComponent = bucket1.PK;

				Factory.Save();

				AssertEquals("Has no filter, but is not required. because is the first link", expected: false, link.HasNoFilterWhenRequired);

				var logger = new BufferManagementLogger();
				var runner = new TestTransferRuleRunner(system, logger);

				runner.Process_ForTest();

				AssertEquals("Workflow was not transferred", workflow.FH_FC_CurrentComponent, bucket1.PK);
			}
		}

		public void TestProcess_ShouldSkipLinkAndLog_WhenDynamicallyFilterTransferRulesIsTrue_ForUserDefinedFilter()
		{
			var aetChecker = new Mock<IResponsiveManagementServiceTaskChecker>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(aetChecker.Object))
			{
				aetChecker.Setup(s => s.AreDependentServiceTasksActive()).Returns(true);
				BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
				var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
				var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
				var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

				var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
				var workflow = jobHeader.ProcessHeaders[0];

				workflow.FH_FC_CurrentComponent = bucket1.PK;

				using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
				{
					var innerFilter = FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "InnerFilter", isPublished: true, isPublishedGlobal: true, isUserDefinedFilter: true);
					BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(innerFilter);

					var filterBizo = module.FilterBusinessObject;
					filterBizo.AddFilterStrip<ModuleUserDefinedFilter>("[USR]InnerFilter");

					filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.CompletionStatement);

					var mainFilter = FilterStripsTestHelper.SaveFilterLayout(filterBizo, "MainFilter", isPublished: true, isPublishedGlobal: true, isUserDefinedFilter: true);
					BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(mainFilter);
				}

				BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(link.FilterRule, "[USR]MainFilter");

				Factory.Save();

				var logger = new BufferManagementLogger();
				var runner = new TestTransferRuleRunner(system, logger);

				runner.Process_ForTest();

				AssertEquals("Workflow was NOT transfered", workflow.FH_FC_CurrentComponent, bucket1.PK);

				var logText = logger.ToString();
				AssertContains(@"1 component link(s) were skipped: [b1 -> b2, Sequence: 0]; they should be processed through Responsive Transfer", logText);
			}
		}

		public void TestProcess_ShouldConsolidateSkippedLinks()
		{
			var aetChecker = new Mock<IResponsiveManagementServiceTaskChecker>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(aetChecker.Object))
			{
				aetChecker.Setup(s => s.AreDependentServiceTasksActive()).Returns(true);

				BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
				var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
				var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
				var bucket3 = BMSTestHelper.CreateBucket(system, "b3");
				var link1 = BMSTestHelper.LinkComponents(bucket1, bucket2, sequence: 0);
				var link2 = BMSTestHelper.LinkComponents(bucket2, bucket3, sequence: 1);

				var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
				var workflow = jobHeader.ProcessHeaders[0];

				workflow.FH_FC_CurrentComponent = bucket1.PK;

				BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(link1.FilterRule);
				BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(link2.FilterRule);

				Factory.Save();

				var logger = new BufferManagementLogger();
				var runner = new TestTransferRuleRunner(system, logger);

				runner.Process_ForTest();

				AssertEquals("Workflow was NOT transfered", workflow.FH_FC_CurrentComponent, bucket1.PK);

				var logText = logger.ToString();
				AssertContains(@"2 component link(s) were skipped: [b1 -> b2, Sequence: 0], [b2 -> b3, Sequence: 1]; they should be processed through Responsive Transfer", logText);
			}
		}

		public void TestProcess_ShouldNotSkipLink_WhenDynamicallyFilterTransferRulesIsTrue_ForUserDefinedFilter()
		{
			BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);
			task.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();

			workflow.FH_FC_CurrentComponent = bucket1.PK;

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var innerFilter = FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "InnerFilter", isPublished: true, isPublishedGlobal: true, isUserDefinedFilter: true);
				BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(innerFilter, ProcessHeader.ModuleFilterConstants.QueueStatus); //QueueStatus don't trigger responsive transfer

				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddFilterStrip<ModuleUserDefinedFilter>("[USR]InnerFilter");

				filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.CompletionStatement);

				var mainFilter = FilterStripsTestHelper.SaveFilterLayout(filterBizo, "MainFilter", isPublished: true, isPublishedGlobal: true, isUserDefinedFilter: true);
				BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(mainFilter);
			}

			BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(link.FilterRule, "[USR]MainFilter");

			Factory.Save();

			var logger = new BufferManagementLogger();
			var runner = new TestTransferRuleRunner(system, logger);

			runner.Process_ForTest();

			AssertEquals("Workflow was transfered", workflow.FH_FC_CurrentComponent, bucket2.PK);
		}

		#endregion

		#region LoadLogs

		public void TestProcess_LoadLogsEnabled()
		{
			var system = CreateSystem("ORG");
			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");

			var link1_2 = LinkComponents(bucket1, bucket2, sequence: 69);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			Factory.Save();

			var logger = new BufferManagementLogger();
			var runner = new TestTransferRuleRunner(system, logger);
			runner.Process_ForTest();

			workflow.Reload();
			AssertEquals(bucket2.FC_Name, workflow.CurrentComponent.FC_Name);
			AssertMatch(LoadLogsRegex, logger.ToString());
		}

		public void TestProcess_LoadLogsDisabled()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system = CreateSystem("ORG");
			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");

			var link1_2 = LinkComponents(bucket1, bucket2, sequence: 69);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			Factory.Save();

			var logger = new BufferManagementLogger();
			var runner = new TestTransferRuleRunner(system, logger);
			runner.Process_ForTest();

			workflow.Reload();
			AssertEquals(bucket2.PK, workflow.FH_FC_CurrentComponent);
			AssertNoMatch(LoadLogsRegex, logger.ToString());
		}

		static Regex LoadLogsRegex
		{
			get { return new Regex(@"Performance: Component link \[bucket1 -\> bucket2, Sequence: 69\], Loading time: \[.+\] Batches processed: \[2\]"); }
		}

		#endregion

		#region Logs performance as either Debug or Warning

		[TestDate]
		public void TestProcess_LogsFastPerformanceAsDebug()
		{
			var log = TestProcess_GetPerformanceLogs(TimeSpan.FromMilliseconds(1));
			AssertContains("Debug", log);
			AssertNotContains("Warning", log);
		}

		[TestDate]
		public void TestProcess_LogsSlowPerformanceAsWarning()
		{
			var log = TestProcess_GetPerformanceLogs(TimeSpan.FromMinutes(1));
			AssertContains("Warning", log);
			AssertNotContains("Debug", log);
		}

		string TestProcess_GetPerformanceLogs(TimeSpan timeSpan)
		{
			var system = CreateSystem("ORG");
			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			LinkComponents(bucket1, bucket2, sequence: 69);
			Factory.Save();

			var logger = new BufferManagementLogger();
			var dataAccessor = new CustomTimeSpanDataAccessor(new TransferRuleRunnerLogger(logger, system), timeSpan);
			var runner = new TestTransferRuleRunner(system, logger, dataAccessor);

			runner.Process_ForTest();
			return logger.ToString();
		}

		class CustomTimeSpanDataAccessor : TransferRuleRunnerDataAccessor
		{
			readonly TimeSpan timeSpan;

			public CustomTimeSpanDataAccessor(ITransferRuleRunnerLogger logger, TimeSpan timeSpan) : base(logger)
				=> this.timeSpan = timeSpan;

			protected override IWorkflowBatchLoader GetLinkAssociatedWorkflowBatchLoaderCore(IComponentLink componentLink, bool useSecondaryServerIfAllowed)
				=> new CustomTimeSpanWorkflowBatchLoader(this, componentLink, Logger, () => GetLinkAssociatedWorkflowQuery(componentLink), timeSpan);

			class CustomTimeSpanWorkflowBatchLoader : LinkAssociatedWorkflowBatchLoader
			{
				public CustomTimeSpanWorkflowBatchLoader(TransferRuleRunnerDataAccessor dataAccessor, IComponentLink componentLink, ILogger logger, Func<(ZQuery query, string queryName)> queryProvider, TimeSpan timeSpan)
					: base(dataAccessor, componentLink, logger, queryProvider, shouldUseSecondaryServerIfAllowed: true)
				{
					this.timeSpan = timeSpan;
				}

				readonly TimeSpan timeSpan;

				protected override BatchLogger GetBatchLoggerCore() => new CustomTimeSpanBatchLogger(Logger, timeSpan);
			}

			class CustomTimeSpanBatchLogger : BatchLogger
			{
				readonly TimeSpan timeSpan;

				public CustomTimeSpanBatchLogger(ILogger logger, TimeSpan timeSpan) : base(logger, "testbatch")
					=> this.timeSpan = timeSpan;

				public override IDisposable BatchLoadStarting()
				{
					var disposableHook = base.BatchLoadStarting();
					return new DisposableAction(() =>
					{
						TestDateAttribute.Date += timeSpan;
						disposableHook.Dispose();
					});
				}
			}
		}

		#endregion

		#region Release Logs

		[TestDate(2015, 1, 2)]
		public void TestTransferAlongNonReleaseGateBufferLink_ShouldNotLogCapacityDetails()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogTransferIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			config.ComponentLink.FL_IsReleaseGateRuleApplied = false;

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Bucket);
			var task = CreateTask(workflow, resource.GS_Code, 120);

			Factory.Save();

			task.P9_TaskID = "T0010001";
			Factory.Save();

			var runner = new TestTransferRuleRunner(config.System, new DummyLogger());
			runner.Process_ForTest();

			var newFactory = Factory.CreateNewFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

			AssertSamePK(config.Buffer, loadedWorkflow.CurrentComponent);
			var noteText = loadedWorkflow.GetSuccessfulReleaseNotes();

			AssertMultilineASCIIEquals("", "It was moved into the component without considering capacity.", noteText);
		}

		#endregion
		#region Release Gate

		public void TestProcess_ShouldNotRelease()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var resource = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");

			var system = CreateSystem("ORG");
			system.FS_Name = "WTGDEV";

			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			var buffer = CreateBuffer(system, "buffer");
			LinkComponents(bucket1, bucket2, sequence: 0);
			LinkComponents(bucket2, buffer, sequence: 1);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";
			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders[0];
			var task = CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var logger = new BufferManagementLogger();
			var runner = new TestTransferRuleRunner(system, logger);

			runner.Process_ForTest();

			workflow.Reload();

			AssertEquals(bucket2.PK, workflow.FH_FC_CurrentComponent);
			AssertMultilineASCIIEquals($@"WTGDEV: Moved workflow Job Workflow (PK = {workflow.PK}, Job = Organization (MAIORGSYD)) from component bucket1 to bucket2
WTGDEV: Component link [bucket1 -> bucket2, Sequence: 0]: 1 workflow transferred
WTGDEV: Set dedicated buffer buffer on workflow Job Workflow (PK = {workflow.PK}, Job = Organization (MAIORGSYD))
WTGDEV: Component link [bucket2 -> buffer, Sequence: 1]: calculated dedicated buffer for 1 workflow, updated on 1 workflow
", logger.ToString());
		}

		#endregion

		#region Setting Dedicated Buffer

		public void TestDedicatedBuffer_ShouldBeSetFromSuitableActiveComponentLink_WithLowerLinkSequenceFirst()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var candidateComponent2 = BMSTestHelper.CreateBuffer(system, "Candidate component 2");
			var candidateComponent3 = BMSTestHelper.CreateBuffer(system, "Candidate component 3");
			var candidateComponent4 = BMSTestHelper.CreateBuffer(system, "Candidate component 4");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);
			var link2 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent2, isReleaseGate: true);
			var link3 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent3, isReleaseGate: true);
			var link4 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent4, isReleaseGate: true);
			link1.FL_Sequence = 10;
			link2.FL_Sequence = 5;
			link3.FL_Sequence = 7;
			link4.FL_Sequence = 2;
			candidateComponent1.FC_DisplaySequence = 1;
			candidateComponent2.FC_DisplaySequence = 2;
			candidateComponent3.FC_DisplaySequence = 3;
			candidateComponent4.FC_DisplaySequence = 4;

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link3.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link4.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Red",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow.Reload();

			AssertEquals("Should take Component To from the suitable link with the lowest FL_Sequence", candidateComponent2.PK, workflow.FH_FC_DedicatedBuffer);
			AssertEquals("The workflow should stay in the same component - not transferred/released", currentComponent.PK, workflow.FH_FC_CurrentComponent);

			AssertMultilineASCIIEquals($@"Test System: Set dedicated buffer Candidate component 2 on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default)
Test System: Component link [Current component -> Candidate component 2, Sequence: 5]: calculated dedicated buffer for 1 workflow, updated on 1 workflow
Test System: Component link [Current component -> Candidate component 3, Sequence: 7]: calculated dedicated buffer for 0 workflows, updated on 0 workflows
Test System: Component link [Current component -> Candidate component 1, Sequence: 10]: calculated dedicated buffer for 0 workflows, updated on 0 workflows
", logger.ToString());
		}

		public void TestDedicatedBuffer_ShouldBeSetFromSuitableComponentLink_WithLowerComponentToDisplaySequenceNext()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var candidateComponent2 = BMSTestHelper.CreateBuffer(system, "Candidate component 2");
			var candidateComponent3 = BMSTestHelper.CreateBuffer(system, "Candidate component 3");
			var candidateComponent4 = BMSTestHelper.CreateBuffer(system, "Candidate component 4");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);
			var link2 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent2, isReleaseGate: true);
			var link3 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent3, isReleaseGate: true);
			var link4 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent4, isReleaseGate: true);
			link1.FL_Sequence = 10;
			link2.FL_Sequence = 10;
			link3.FL_Sequence = 10;
			link4.FL_Sequence = 10;
			candidateComponent1.FC_DisplaySequence = 7;
			candidateComponent2.FC_DisplaySequence = 5;
			candidateComponent3.FC_DisplaySequence = 3;
			candidateComponent4.FC_DisplaySequence = 2;

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link3.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link4.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Red",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow.Reload();

			AssertEquals("Should take Component To from the suitable link with the lowest FC_DisplaySequence on To components when link sequences are the same", candidateComponent3.PK, workflow.FH_FC_DedicatedBuffer);
			AssertEquals("The workflow should stay in the same component - not transferred/released", currentComponent.PK, workflow.FH_FC_CurrentComponent);

			AssertMultilineASCIIEquals($@"Test System: Set dedicated buffer Candidate component 3 on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default)
Test System: Component link [Current component -> Candidate component 3, Sequence: 10]: calculated dedicated buffer for 1 workflow, updated on 1 workflow
Test System: Component link [Current component -> Candidate component 2, Sequence: 10]: calculated dedicated buffer for 0 workflows, updated on 0 workflows
Test System: Component link [Current component -> Candidate component 1, Sequence: 10]: calculated dedicated buffer for 0 workflows, updated on 0 workflows
", logger.ToString());
		}

		public void TestHavingDedicatedBufferSetBeforeRun_ShouldNotPreventUpdatingDedicatedBufferByLinksRequiringReleaseGate()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var initialDedicatedBuffer = BMSTestHelper.CreateBucket(system, "Initial dedicated buffer");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_FC_DedicatedBuffer = initialDedicatedBuffer.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow.Reload();

			AssertEquals("The dedicated buffer should be updated even when being set before run", candidateComponent1.PK, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Test System: Set dedicated buffer Candidate component 1 on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default)
Test System: Component link [Current component -> Candidate component 1, Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 1 workflow
", logger.ToString());
		}

		public void TestHavingDedicatedBufferSetBeforeRun_ShouldNotPreventTransferByLinksNotRequiringReleaseGate()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var initialDedicatedBuffer = BMSTestHelper.CreateBucket(system, "Initial dedicated buffer");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: false); // does not require release gate

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_FC_DedicatedBuffer = initialDedicatedBuffer.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow.Reload();

			AssertEquals("The workflow should be transferred even having dedicated buffer set before run", candidateComponent1.PK, workflow.FH_FC_CurrentComponent);
			AssertEquals("The dedicated buffer should be reset after the transfer", Guid.Empty, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Test System: Moved workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default) from component Current component to Candidate component 1
Test System: Component link [Current component -> Candidate component 1, Sequence: 0]: 1 workflow transferred
", logger.ToString());
		}

		public void TestSettingDedicatedBuffer_ShouldExcludeFurtherTransfersOrSettingDedicatedBufferAgainInTheSameRun()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1 (Buffer)");
			var candidateComponent2 = BMSTestHelper.CreateBuffer(system, "Candidate component 2 (Buffer)");
			var candidateComponent3 = BMSTestHelper.CreateBucket(system, "Candidate component 3 (Bucket)");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);
			var link2 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent2, isReleaseGate: true);
			var link3 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent3, isReleaseGate: false);
			link1.FL_Sequence = 0;
			link2.FL_Sequence = 1;
			link3.FL_Sequence = 2;

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "ABC",
			});
			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "AB",
			});
			FilterStripsTestHelper.AddFilterStrips(link3.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "A",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = currentComponent.PK;
			workflow1.FH_CompletionStatement = "ABC___";

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = currentComponent.PK;
			workflow2.FH_CompletionStatement = "AB___";

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_FC_CurrentComponent = currentComponent.PK;
			workflow3.FH_CompletionStatement = "A___";

			Factory.Save();

			var logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow1.Reload();
			workflow2.Reload();
			workflow2.Reload();

			CombineAssertions("When running the first time", () =>
			{
				AssertEquals("Workflow1 should stay in the same component - not transferred", currentComponent.PK, workflow1.FH_FC_CurrentComponent);
				AssertEquals("Should set dedicated buffer for workflow1 using the first link", candidateComponent1.PK, workflow1.FH_FC_DedicatedBuffer);

				AssertEquals("Workflow2 should stay in the same component - not transferred", currentComponent.PK, workflow2.FH_FC_CurrentComponent);
				AssertEquals("Should set dedicated buffer for workflow2 using the second link", candidateComponent2.PK, workflow2.FH_FC_DedicatedBuffer);

				AssertEquals("Workflow3 should be transferred", candidateComponent3.PK, workflow3.FH_FC_CurrentComponent);
				AssertEquals("Should not set dedicated buffer for workflow3", ZGuid.Empty, workflow3.FH_FC_DedicatedBuffer);

				AssertMultilineASCIIEquals($@"Test System: Set dedicated buffer Candidate component 1 (Buffer) on workflow ABC___ (PK = {workflow1.PK}, Job = Dummy Business Object Default)
Test System: Component link [Current component -> Candidate component 1 (Buffer), Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 1 workflow
Test System: Set dedicated buffer Candidate component 2 (Buffer) on workflow AB___ (PK = {workflow2.PK}, Job = Dummy Business Object Default)
Test System: Component link [Current component -> Candidate component 2 (Buffer), Sequence: 1]: calculated dedicated buffer for 1 workflow, updated on 1 workflow
Test System: Moved workflow A___ (PK = {workflow3.PK}, Job = Dummy Business Object Default) from component Current component to Candidate component 3 (Bucket)
Test System: Component link [Current component -> Candidate component 3 (Bucket), Sequence: 2]: 1 workflow transferred
", logger.ToString());
			});

			logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow1.Reload();
			workflow2.Reload();
			workflow2.Reload();

			CombineAssertions("When running the second time", () =>
			{
				AssertEquals("Workflow1 should stay in the same component - not transferred", currentComponent.PK, workflow1.FH_FC_CurrentComponent);
				AssertEquals("The dedicated buffer for workflow1 should stay the same", candidateComponent1.PK, workflow1.FH_FC_DedicatedBuffer);

				AssertEquals("Workflow2 should stay in the same component - not transferred", currentComponent.PK, workflow2.FH_FC_CurrentComponent);
				AssertEquals("The dedicated buffer for workflow2 should stay the same", candidateComponent2.PK, workflow2.FH_FC_DedicatedBuffer);

				AssertEquals("Workflow3 should stay in the same component - it already transferred", candidateComponent3.PK, workflow3.FH_FC_CurrentComponent);
				AssertEquals("Should not set dedicated buffer for workflow3", ZGuid.Empty, workflow3.FH_FC_DedicatedBuffer);

				AssertMultilineASCIIEquals($@"Test System: Component link [Current component -> Candidate component 1 (Buffer), Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 0 workflows
Test System: Component link [Current component -> Candidate component 2 (Buffer), Sequence: 1]: calculated dedicated buffer for 1 workflow, updated on 0 workflows
Test System: Component link [Current component -> Candidate component 3 (Bucket), Sequence: 2]: 0 workflows transferred
", logger.ToString());
			});
		}

		public void TestDedicatedBufferCalculation_ShouldIgnoreInactiveComponentLinks()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var candidateComponent2 = BMSTestHelper.CreateBuffer(system, "Candidate component 2");
			var candidateComponent3 = BMSTestHelper.CreateBuffer(system, "Candidate component 3");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);
			var link2 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent2, isReleaseGate: true);
			var link3 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent3, isReleaseGate: true);
			link1.FL_Sequence = 10;
			link2.FL_Sequence = 9;
			link3.FL_Sequence = 8;

			link3.FL_TransferRulesEnabled = false;

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link3.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow.Reload();

			AssertEquals("Should ignore inactive component links", candidateComponent2.PK, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Test System: Set dedicated buffer Candidate component 2 on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default)
Test System: Component link [Current component -> Candidate component 2, Sequence: 9]: calculated dedicated buffer for 1 workflow, updated on 1 workflow
Test System: Component link [Current component -> Candidate component 1, Sequence: 10]: calculated dedicated buffer for 0 workflows, updated on 0 workflows
", logger.ToString());
		}

		public void TestDedicatedBufferCalculation_ShouldIgnoreLinksLeadingToInactiveComponents()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var candidateComponent2 = BMSTestHelper.CreateBuffer(system, "Candidate component 2");
			var candidateComponent3 = BMSTestHelper.CreateBuffer(system, "Candidate component 3");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);
			var link2 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent2, isReleaseGate: true);
			var link3 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent3, isReleaseGate: true);
			link1.FL_Sequence = 10;
			link2.FL_Sequence = 11;
			link3.FL_Sequence = 12;

			candidateComponent1.FC_IsActive = false;

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link3.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);
			task.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();

			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow.Reload();

			AssertEquals("Should ignore links that lead to inactive components", candidateComponent2.PK, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Test System: Set dedicated buffer Candidate component 2 on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default)
Test System: Component link [Current component -> Candidate component 2, Sequence: 11]: calculated dedicated buffer for 1 workflow, updated on 1 workflow
Test System: Component link [Current component -> Candidate component 3, Sequence: 12]: calculated dedicated buffer for 0 workflows, updated on 0 workflows
", logger.ToString());
		}

		public void TestDedicatedBufferCalculation_ShouldIgnoreInactiveWorkflows()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";
			workflow.FH_IsActive = false;

			Factory.Save();

			var logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow.Reload();

			AssertEquals("Inactive workflow should not update its dedicated buffer", Guid.Empty, workflow.FH_FC_DedicatedBuffer);

			AssertNullOrEmpty(logger.ToString());

			workflow.FH_IsActive = true;
			Factory.Save();

			logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow.Reload();

			AssertEquals("Active workflow should update its dedicated buffer", candidateComponent1.PK, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Test System: Set dedicated buffer Candidate component 1 on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default)
Test System: Component link [Current component -> Candidate component 1, Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 1 workflow
", logger.ToString());
		}

		public void TestDedicatedBufferCalculation_ShouldIgnoreWorkflowsWithNoCurrentComponent()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent = BMSTestHelper.CreateBuffer(system, "Candidate component");
			var link = BMSTestHelper.LinkComponents(currentComponent, candidateComponent, isReleaseGate: true);

			FilterStripsTestHelper.AddFilterStrips(link.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = ZGuid.Empty;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			AssertEquals("A workflow with no current component set should not update its dedicated buffer", Guid.Empty, workflow.FH_FC_DedicatedBuffer);

			AssertNullOrEmpty(logger.ToString());

			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			Factory.Save();

			logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow.Reload();

			AssertEquals("A workflow with a current component set should update its dedicated buffer", candidateComponent.PK, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Test System: Set dedicated buffer Candidate component on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default)
Test System: Component link [Current component -> Candidate component, Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 1 workflow
", logger.ToString());
		}

		public void TestDedicatedBuffer_ShouldStayUnchanged_WhileBMSIsNotLive()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var candidateComponent2 = BMSTestHelper.CreateBuffer(system, "Candidate component 2");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);
			var link2 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent2, isReleaseGate: true);
			link1.FL_Sequence = 0;
			link2.FL_Sequence = 1;

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow.Reload();

			AssertEquals("Should take Component To from the suitable link with the lowest FL_Sequence", candidateComponent1.PK, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Test System: Set dedicated buffer Candidate component 1 on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default)
Test System: Component link [Current component -> Candidate component 1, Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 1 workflow
Test System: Component link [Current component -> Candidate component 2, Sequence: 1]: calculated dedicated buffer for 0 workflows, updated on 0 workflows
", logger.ToString());

			system.FS_IsLive = false;

			Factory.Save();

			link1.FL_Sequence = 1;
			link2.FL_Sequence = 0;

			Factory.Save();

			logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow.Reload();

			AssertEquals("Dedicated buffer should stay untouched when the system is deactivated", candidateComponent1.PK, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals(string.Empty, logger.ToString());

			system.FS_IsLive = true;

			Factory.Save();

			logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow.Reload();

			AssertEquals("Dedicated buffer should be taken from another link now as the system is active again", candidateComponent2.PK, workflow.FH_FC_DedicatedBuffer);

			AssertMultilineASCIIEquals($@"Test System: Set dedicated buffer Candidate component 2 on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default)
Test System: Component link [Current component -> Candidate component 2, Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 1 workflow
Test System: Component link [Current component -> Candidate component 1, Sequence: 1]: calculated dedicated buffer for 0 workflows, updated on 0 workflows
", logger.ToString());
		}

		public void TestSettingDedicatedBuffer_ShouldNotCauseWorkflowDeactivation()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			var link1 = BMSTestHelper.LinkComponents(bucket1, bucket2, sequence: 0);
			var link2 = BMSTestHelper.LinkComponents(bucket2, buffer, sequence: 1);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = bucket1.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow.Reload();

			AssertEquals("Should transfer", bucket2.PK, workflow.FH_FC_CurrentComponent);
			AssertEquals("Should determine the dedicated buffer", buffer.PK, workflow.FH_FC_DedicatedBuffer);
			AssertEquals("Should stay active", true, workflow.FH_IsActive);
		}

		public void TestDedicatedBuffer_ShouldBeSetToNullAfterTransfer()
		{
			var system = CreateSystem("ORG");
			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			var buffer = CreateBuffer(system, "Buffer");

			LinkComponents(bucket1, bucket2);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = bucket1.PK;
			workflow.FH_FC_DedicatedBuffer = buffer.PK;

			Factory.Save();

			var logger = new BufferManagementLogger();
			var runner = new TestTransferRuleRunner(system, logger);
			runner.Process_ForTest();

			workflow.Reload();
			AssertEquals(bucket2.FC_Name, workflow.CurrentComponent.FC_Name);
			AssertEquals(ZGuid.Empty, workflow.FH_FC_DedicatedBuffer);
		}

		#endregion

		#region Logging

		public void TestLog_Transfer_LogWorkflowInfoOnTransferEnabled()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBucket(system, "Candidate component 1");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: false);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow.Reload();

			AssertMultilineASCIIEquals($@"Test System: Moved workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default) from component Current component to Candidate component 1
Test System: Component link [Current component -> Candidate component 1, Sequence: 0]: 1 workflow transferred
", logger.ToString());
		}

		public void TestLog_Transfer_LogWorkflowInfoOnTransferDisabled()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBucket(system, "Candidate component 1");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: false);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow.Reload();

			AssertMultilineASCIIEquals($@"Test System: Component link [Current component -> Candidate component 1, Sequence: 0]: 1 workflow transferred
", logger.ToString());
		}

		public void TestLog_SettingDedicatedBuffer_LogWorkflowInfoOnTransferEnabled()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow.Reload();

			AssertMultilineASCIIEquals($@"Test System: Set dedicated buffer Candidate component 1 on workflow Green (PK = {workflow.PK}, Job = Dummy Business Object Default)
Test System: Component link [Current component -> Candidate component 1, Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 1 workflow
", logger.ToString());
		}

		public void TestLog_SettingDedicatedBuffer_LogWorkflowInfoOnTransferDisabled()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test System";
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var link1 = BMSTestHelper.LinkComponents(currentComponent, candidateComponent1, isReleaseGate: true);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_CompletionStatement = "Green";

			Factory.Save();

			var logger = new BufferManagementLogger();
			new TestTransferRuleRunner(system, logger).Process_ForTest();

			workflow.Reload();

			AssertMultilineASCIIEquals($@"Test System: Component link [Current component -> Candidate component 1, Sequence: 0]: calculated dedicated buffer for 1 workflow, updated on 1 workflow
", logger.ToString());
		}

		#endregion

		#region Deactivating Workflows

		public void TestWorkflowDeleted_DuringBatchProcessing_ShouldThrowException()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var firstBucket = BMSTestHelper.CreateBucket(config.System, "Entry to your face");
			BMSTestHelper.LinkComponents(firstBucket, config.Bucket);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", firstBucket);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 10);

			Factory.Save();

			var runner = new TransferRuleRunnerThatDeletesProcessedWorkflows(config.System, new DummyLogger(), workflow.PK);

			AssertExceptionThrown<ApplicationException>("Workflow NEVER should be deleted in a context of a service task", () => runner.Process_ForTest());
		}

		class TransferRuleRunnerThatDeletesProcessedWorkflows : TestTransferRuleRunner
		{
			readonly ZGuid workflowToDeletePK;

			public TransferRuleRunnerThatDeletesProcessedWorkflows(BMSystem system, ILogger logger, ZGuid workflowToDeletePK)
				: base(system, logger)
			{
				this.workflowToDeletePK = workflowToDeletePK;
			}

			protected override void OnTransferWorkflows()
			{
				using (DataAccessor.GetTemporaryEnvironmentForServiceTaskBranch())
				{
					((TransferRuleRunnerDataAccessor)DataAccessor).CurrentFactory_ExposedForTest.Load<ProcessHeader>(workflowToDeletePK).Delete();
				}

				base.OnTransferWorkflows();
			}
		}

		[TestDate(2019, 04, 05)]
		public void TestWorkflowsShouldNotDeactivate_WhenGettingSaveConcurrencyException()
		{
			var system = CreateSystem("ORG");

			var bucket1 = CreateBucket(system, "bucket1", sequence: 1);
			var bucket2 = CreateBucket(system, "bucket2", sequence: 2);

			var link1_2 = LinkComponents(bucket1, bucket2, sequence: 2);
			FilterStripsTestHelper.AddFilterStrips(link1_2.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = bucket1.PK;
			workflow.Name = "Nobody needs me";

			var workflowToGetSaveConcurrencyError = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflowToGetSaveConcurrencyError.FH_FC_CurrentComponent = bucket1.PK;
			workflowToGetSaveConcurrencyError.FH_AgreedDeliveryDate = ZDateTime.Now; // to generate concurrency error
			workflowToGetSaveConcurrencyError.Name = "I am on high demand";

			Factory.Save();

			var logger = new BufferManagementLogger();
			var runner = new DummyTransferRuleRunner(system, logger);

			runner.OnBeforeSaveTransferredWorkflowsAction = () =>
			{
				TestDateAttribute.AddMinutes(1);

				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var loadedWorkflowToGetSaveConcurrencyError = newFactory.Load<ProcessHeader>(workflowToGetSaveConcurrencyError.PK);

				loadedWorkflowToGetSaveConcurrencyError.FH_AgreedDeliveryDate = ZDateTime.Now.AddHours(1);

				newFactory.Save();
			};

			Assert("Precondition", !runner.HadConcurrencyException_ForTest);

			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			runner.Process_ForTest();

			Assert(runner.HadConcurrencyException_ForTest);

			var loadedWorkflow1 = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var loadedWorkflow2 = new BusinessObjectFactory().Load<ProcessHeader>(workflowToGetSaveConcurrencyError.PK);
			AssertEquals("Should not deactivate", expected: true, loadedWorkflow1.FH_IsActive);
			AssertEquals("Should not deactivate", expected: true, loadedWorkflow2.FH_IsActive);
		}

		public void TestTransfer_ShouldNotTransferInactiveWorkflows()
		{
			var system = BMSTestHelper.CreateSystemAndRelatedWorkflowType(Factory, "DUM", isActive: true);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			BMSTestHelper.LinkComponents(bucket1, bucket2);
			var activeWorkflow1 = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "activeWorkflow1", bucket1);
			var activeWorkflow2 = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "activeWorkflow2", bucket1);
			var inactiveWorkflow1 = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "inactiveWorkflow1", bucket1);
			var inactiveWorkflow2 = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "inactiveWorkflow2", bucket1);
			inactiveWorkflow1.FH_IsActive = false;
			inactiveWorkflow2.FH_IsActive = false;

			Factory.Save();

			var logger = new BufferManagementLogger();
			var runner = new DummyTransferRuleRunner(system, logger);

			runner.Process_ForTest();

			activeWorkflow1.Reload();
			activeWorkflow2.Reload();
			inactiveWorkflow1.Reload();
			inactiveWorkflow2.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("Should not transfer inactiveWorkflow1", bucket1.PK, inactiveWorkflow1.FH_FC_CurrentComponent);
				AssertEquals("Should not transfer inactiveWorkflow2", bucket1.PK, inactiveWorkflow2.FH_FC_CurrentComponent);
				AssertEquals("Should transfer activeWorkflow1", bucket2.PK, activeWorkflow1.FH_FC_CurrentComponent);
				AssertEquals("Should transfer activeWorkflow2", bucket2.PK, activeWorkflow2.FH_FC_CurrentComponent);
			});
		}

		#endregion

		#region Component Transfer Event

		[TestDate(2015, 7, 14)]
		[TestDateIncremental(seconds: 1)]
		public void TestProcess_ShouldLogXFREventOnWorkflow()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "B1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "B2");

			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "werkflow");
			BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			AssertEquals(bucket1, workflow.CurrentComponent);

			RunTransferRules(system);

			AssertEquals(bucket2, workflow.CurrentComponent);
			var log = BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.SchematicTransfer, bucket1.PK, bucket2.PK, link.PK);

			AssertEquals("Moved from component [B1] to [B2]. It was moved by the BMS service task along component link [B1 -> B2, Sequence: 0].", log.DisplayEventReference);
			AssertEquals($"|FRM={bucket1.PK}|LNK={link.PK}|MOD=XFR|STS=OPN|TO={bucket2.PK}", log.SL_Reference);
		}

		public void TestProcess_ShouldLogRXREventOnWorkflow()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "B1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "B2");

			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "werkflow");
			BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			AssertEquals(bucket1, workflow.CurrentComponent);

			var mock = new Mock<ITransferRuleRunnerParams>();
			mock.Setup(b => b.IsResponsive).Returns(true);
			mock.Setup(b => b.IsCdcEnabled).Returns(true);

			RunTransferRules(system, mock.Object);

			AssertEquals(bucket2, workflow.CurrentComponent);
			var log = BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.ResponsiveTransfer, bucket1.PK, bucket2.PK, link.PK);

			AssertEquals("Moved from component [B1] to [B2]. It was moved by the Responsive Transfer along component link [B1 -> B2, Sequence: 0].", log.DisplayEventReference);
			AssertEquals($"|FRM={bucket1.PK}|LNK={link.PK}|MOD=RXR|STS=OPN|TO={bucket2.PK}", log.SL_Reference);
		}

		#endregion

		#region Save Concurrency Exception Handled Properly

		[TestDate(2016, 11, 11)]
		public void TestShouldRetryProcessingLink_OnSingleSaveConcurrencyException()
		{
			AssertHandlesConcurrencyExceptionProperly(generateConcurrencyExceptionJustOnce: true, expectWorkflowIsTransferred: true, out BufferManagementLogger logger);
			AssertNotContains("CONCURRENCY Error Saving Record", logger.ToString(), ignoreCase: true);
		}

		[TestDate(2016, 11, 11)]
		public void TestShouldLogConcurrencyError_WhenReachingMaxAttemptLimit()
		{
			AssertHandlesConcurrencyExceptionProperly(generateConcurrencyExceptionJustOnce: false, expectWorkflowIsTransferred: false, out BufferManagementLogger logger);
			AssertContains("CONCURRENCY Error Saving Record", logger.ToString(), ignoreCase: true);
		}

		[TestDate(2016, 11, 11)]
		void AssertHandlesConcurrencyExceptionProperly(bool generateConcurrencyExceptionJustOnce, bool expectWorkflowIsTransferred, out BufferManagementLogger logger)
		{
			var system = CreateSystem("ORG");

			var bucket1 = CreateBucket(system, "bucket1", sequence: 0);
			var bucket2 = CreateBucket(system, "bucket2", sequence: 1);
			var bucket3 = CreateBucket(system, "bucket3", sequence: 2);
			var buffer = CreateBucket(system, "buffer", sequence: 3);

			var link1_2 = LinkComponents(bucket1, bucket2, sequence: 2);

			var link2_3 = LinkComponents(bucket2, bucket3, sequence: 2);
			FilterStripsTestHelper.AddFilterStrips(link2_3.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank
			});

			var link3_4 = LinkComponents(bucket3, buffer, sequence: 5, isReleaseGate: true);
			FilterStripsTestHelper.AddFilterStrips(link3_4.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank
			});

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = bucket1.PK;
			workflow.FH_AgreedDeliveryDate = ZDateTime.Now; // to generate concurrency error

			Factory.Save();

			logger = new BufferManagementLogger();
			var runner = new DummyTransferRuleRunner(system, logger);

			var generateConcurrencyErrorOnNextSave = true;
			runner.OnBeforeSaveTransferredWorkflowsAction = () =>
			{
				if (generateConcurrencyErrorOnNextSave || !generateConcurrencyExceptionJustOnce)
				{
					TestDateAttribute.AddMinutes(1);

					var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
					var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

					loadedWorkflow.FH_AgreedDeliveryDate = ZDateTime.Now.AddHours(1);

					newFactory.Save();
				}
				generateConcurrencyErrorOnNextSave = !generateConcurrencyErrorOnNextSave;
			};

			Assert("Precondition", !runner.HadConcurrencyException_ForTest);

			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			runner.Process_ForTest();

			Assert(runner.HadConcurrencyException_ForTest);

			if (expectWorkflowIsTransferred)
			{
				AssertContains("bucket1 to bucket2", logger.ToString(), ignoreCase: true);
				AssertContains("bucket2 to bucket3", logger.ToString(), ignoreCase: true);
				AssertContains("Set dedicated buffer buffer", logger.ToString(), ignoreCase: true);

				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
				AssertEquals("Should transfer", bucket3.PK, loadedWorkflow.FH_FC_CurrentComponent);
				AssertEquals("Should set dedicated buffer", buffer.PK, loadedWorkflow.FH_FC_DedicatedBuffer);
			}
		}

		#endregion

		#region Circular Dependency Exception

		public void TestCircularDependency_ShouldProcessAllNonProblematicWorkflowsInLink()
		{
			var postmasterQuery = new ZQuery();
			postmasterQuery.AddToFilter(GlbStaffSchema.GS_LoginName, SQLComparisonOperator.Equal, User.PostMasterUserName);
			var postmaster = Factory.Load<GlbStaff>(postmasterQuery).Single();
			postmaster.GS_EmailAddress = "x@wisetechglobal.com";
			var system = CreateSystem("ORG");
			var bucket1 = CreateBucket(system, "Bucket 1");
			var bucket2 = CreateBucket(system, "Bucket 2");
			var bucket3 = CreateBucket(system, "Bucket 3");
			var bucket4 = CreateBucket(system, "Bucket 4");

			var link1_2 = LinkComponents(bucket1, bucket2, sequence: 1);
			var link2_3 = LinkComponents(bucket2, bucket3, sequence: 2);
			FilterStripsTestHelper.AddFilterStrips(link2_3.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "K"
			});

			var link3_4 = LinkComponents(bucket3, bucket4, sequence: 3);
			FilterStripsTestHelper.AddFilterStrips(link3_4.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank
			});

			var workflowsForLink1_2 = BMSTestHelper.CreateWorkflows(bucket1, 10, 1);
			var first = workflowsForLink1_2[0];
			var second = workflowsForLink1_2[1];

			first.FH_CompletionStatement = "AAArdvark"; // to ensure they're both in the first batch.
			second.FH_CompletionStatement = "AAArdvarl";
			AssertEquals("The first two workflows should be the first ones ordered by completion statement, ensuring that they'll be in the same batch when processed, and yet...", -1, string.Compare(second.FH_CompletionStatement, workflowsForLink1_2[2].FH_CompletionStatement));

			var workflowsForLink3_4 = BMSTestHelper.CreateWorkflows(bucket3, 10, 1);

			Factory.Save();

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			BMSTestHelper.CreateCircularDependency(first, second, TestConnection);

			var factoryToEnsureCircularDependencyIsPresent = Factory.CreateNewFactory();
			var loadedTroubleWorkflows = factoryToEnsureCircularDependencyIsPresent.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, new[] { first.PK, second.PK }));
			loadedTroubleWorkflows.First().Tasks.First().P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			factoryToEnsureCircularDependencyIsPresent.Save();

			var logger = new DetailedLoggerForTest();
			var runner = new DummyTransferRuleRunner(system, logger, batchSize: 5, shouldSortByCompletionStatement: true);
			runner.Process_ForTest();

			var newFactory = Factory.CreateNewFactory();
			var loadedWorkflowsForLink1_2 = newFactory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, workflowsForLink1_2.Select(x => x.PK)));

			foreach (var workflow in loadedWorkflowsForLink1_2)
			{
				AssertEquals(string.Format("{0}: All workflows (including circular-dependant workflows) should have been processed in one processing of the runner", workflow.Description),
					"Bucket 2",
					workflow.CurrentComponent.FC_Name);
			}

			var loadedWorkflowsForLink3_4 = newFactory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, workflowsForLink3_4.Select(x => x.PK)));

			foreach (var workflow in loadedWorkflowsForLink3_4)
			{
				AssertEquals("All the workflows in the unaffected link should have been processed, and yet...", "Bucket 4", workflow.CurrentComponent.FC_Name);
			}
		}

		#endregion

		#region Link Inclusion

		public void TestProcess_ShouldUseEnabledLinksOnly()
		{
			var system = CreateSystem("ORG");
			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			var bucket3 = CreateBucket(system, "bucket3");

			var link1_2 = LinkComponents(bucket1, bucket2, sequence: 0);
			var link1_3 = LinkComponents(bucket1, bucket3, sequence: 1);
			link1_2.FL_TransferRulesEnabled = false;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			Factory.Save();

			var runner = new TestTransferRuleRunner(system, new DummyLogger());
			runner.Process_ForTest();

			workflow.Reload();
			AssertEquals(bucket3.PK, workflow.FH_FC_CurrentComponent);
		}

		public void TestProcess_BucketToBufferLink_IsReleaseGate()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			var link = LinkComponents(bucket, buffer, isReleaseGate: true);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];

			Factory.Save();

			var runner = new TestTransferRuleRunner(system, new DummyLogger());
			runner.Process_ForTest();

			workflow.Reload();
			AssertEquals(bucket.PK, workflow.FH_FC_CurrentComponent);
		}

		public void TestProcess_BucketToBufferLink_NotReleaseGate()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			var link = LinkComponents(bucket, buffer, isReleaseGate: false);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];

			Factory.Save();

			ReleaseGateKeeperTest.RunReleaseGate(system);
			workflow.Reload();
			AssertEquals("Should not be eligible for release because the component link has been marked as a non-release gate link", bucket.FC_Name, workflow.CurrentComponent.FC_Name);

			var runner = new TestTransferRuleRunner(system, new DummyLogger());
			runner.Process_ForTest();

			workflow.Reload();
			AssertEquals(buffer.FC_Name, workflow.CurrentComponent.FC_Name);
		}

		public void TestProcess_ShouldOrderLinksByLinkSequence()
		{
			var system = CreateSystem("ORG");
			var bucket1 = CreateBucket(system, "bucket1", sequence: 0);
			var bucket2 = CreateBucket(system, "bucket2", sequence: 1);
			var bucket3 = CreateBucket(system, "bucket3", sequence: 2);

			var link1_2 = LinkComponents(bucket1, bucket2, sequence: 2);

			var link1_3 = LinkComponents(bucket1, bucket3, sequence: 1);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];

			Factory.Save();

			var runner = new TestTransferRuleRunner(system, new DummyLogger());
			runner.Process_ForTest();

			workflow.Reload();
			AssertEquals("Should move to the component with the lowest BMComponentLinkSequence", "bucket3", workflow.CurrentComponent.FC_Name);
		}

		#endregion

		#region Error Handling 

		public void TestCanTransfer_WithInvalidResource_ShouldNotCrash()
		{
			var emailNotificationResource = Factory.NewWithValidTestData<GlbStaff>();
			emailNotificationResource.GS_EmailAddress = "me@you.com";
			var emailNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			emailNotificationGroup.Staff.Add(emailNotificationResource);
			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailNotificationGroup.PK.ToGuid());

			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			var link = LinkComponents(bucket, buffer, isReleaseGate: false);
			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];

			var task1 = CreateTask(workflow, "ABC", 60);
			var task2 = CreateTask(workflow, "Z69", 60);

			Factory.Save();

			AssertNull("Assigned staff is not valid", task1.AssignedStaffMember);
			AssertNull("Assigned staff is not valid", task2.AssignedStaffMember);

			AssertCanReleaseToBuffer("Workflow should not be eligible for release because we can't determine capacity of an invalid resource", workflow, buffer, BufferReleaseOutcome.BlockedByMissingResource, "Cannot release this workflow as one or more tasks are assigned to the following staff members which cannot be found: ABC, Z69");
			AssertEquals("Should not send emails because no one will read them", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestProcess_RulePasses_ShouldLeaveReleaseFailure()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;
			staff.GS_GB_HomeBranch = Env.CurrentBranch.PK;

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var bucket = BMSTestHelper.CreateBucket(system, "bucket");
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer1");

			var link = bucket.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = buffer.PK;

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			workflow.FH_FC_CurrentComponent = bucket.PK;

			AssertEquals("Pre-condition", ProcessHeader.ReleaseFailureReasonNotAvailableMessage, workflow.GetReleaseFailureReasonForBuffer(buffer));
			BMSTestHelper.SetLastReleaseFailureReason(workflow, "Sheet got real", buffer, ReleaseLogFailureServiceForTest);

			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_FH_ProcessHeader = workflow.PK;

			Factory.Save();

			var ruleRunner = new TestTransferRuleRunner(system, new DummyLogger());
			ruleRunner.Process_ForTest();

			workflow.Reload();

			AssertEquals("Should not overwrite release failure reason", "Sheet got real", workflow.GetReleaseFailureReasonForBuffer(buffer));
		}

		public void TestProcess_BMSReport()
		{
			var system = CreateSystem("ORG");
			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			var bucket3 = CreateBucket(system, "bucket3");

			var link1_2 = LinkComponents(bucket1, bucket2, sequence: 0);
			var link1_3 = LinkComponents(bucket1, bucket3, sequence: 1);
			link1_2.FL_TransferRulesEnabled = false;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			Factory.Save();

			var runner = new DummyTransferRulerRunnerQueryException(system, new DummyLogger());
			var exception = AssertExceptionThrown<Exception>(runner.Process_ForTest);
			AssertType<SqlException>(exception.InnerException);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestProcess_ZConcurrencyCheckFailureException()
		{
			var system = CreateSystem("ORG");
			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			var bucket3 = CreateBucket(system, "bucket3");

			var link1_2 = LinkComponents(bucket1, bucket2, sequence: 0);
			var link1_3 = LinkComponents(bucket1, bucket3, sequence: 1);
			link1_2.FL_TransferRulesEnabled = false;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			Factory.Save();

			var runner = new ZConcurrencyCheckFailureExceptionRunner(system, new DummyLogger());
			AssertNoExceptionThrown(runner.Process_ForTest);
		}

		public void TestLinkWithFilterWithCountrySpecificModule_WhenModuleNotAvailableInServiceTaskContext_ShouldDisableLink_AndNotThrowExceptions()
		{
			BMSTestHelper.SetupEmailAndGroup(Factory);

			var company = Factory.New<IGlbCompany>();
			((BusinessObject)company).FillWithValidTestData();
			company.GC_RN_NKCountryCode = "DE";

			var branch = Factory.New<IGlbBranch>();
			((BusinessObject)branch).FillWithValidTestData();
			branch.GB_GC = company.PK;

			var system = CreateSystem("ORG");
			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			bucket2.FC_GB_AgingBranch = branch.PK;

			var link = LinkComponents(bucket1, bucket2, sequence: 0);

			FilterStripsTestHelper.AddFilterStrip<ModuleGuidModuleSpecifiedFilter>(link.FilterRule, "Parent Job",
				(filter) => filter.SelectedModule = ModuleIDs.Customs.AU.AirCargoOutturnBills.Name,
				(filter) => filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch);

			Factory.Save();

			var logger = new BufferManagementLogger();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var ruleRunner = new TestTransferRuleRunner(system, logger);
				AssertNoExceptionThrown("Processing links with unavailable company-specific modules should not throw exceptions. SAD!", ruleRunner.Process_ForTest);
			}

			AssertContains("The following component links have been deactivated since they have no filters or invalid filters:", logger.ToString());

			link.Reload();
			AssertEquals("The link should be disabled. SAD!", expected: false, link.FL_TransferRulesEnabled);
		}

		public void TestSqlTimeout_ShouldSendEmailAndLogError()
		{
			var postmasterQuery = new ZQuery(GlbStaffSchema.GS_LoginName, SQLComparisonOperator.Equal, User.PostMasterUserName);
			var postmaster = Factory.Load<GlbStaff>(postmasterQuery).Single();
			postmaster.GS_EmailAddress = "x@wisetechglobal.com";

			var system = CreateSystem("ORG");
			system.FS_Name = "Test System";
			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");

			var link1_2 = LinkComponents(bucket1, bucket2);
			link1_2.FL_TransferRulesEnabled = true;
			link1_2.FL_Sequence = 69;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			Factory.Save();

			var logger = new BufferManagementLogger();
			var runner = new DummyTransferRulerRunnerWhichThrowsReaderException(system, logger, -2, "It timed out... oh my dear!" + DbCommand.ExecuteAsReaderFlagComments);
			runner.Process_ForTest();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var body = Env.OutgoingMailManager.EmailsCreated[0].Body;
			var executeAsReaderFlag = DbCommand.ExecuteAsReaderFlagComments.Replace("\n", "").Trim();
			AssertNotContains(executeAsReaderFlag, body);
			AssertContains(DbCommand.ExecuteAsReaderFlagMask, body);

			AssertNotContains(executeAsReaderFlag, logger.ToString());

			string dataProvider;
#if NETFRAMEWORK
			dataProvider = ".Net SqlClient Data Provider";
#else
			if ((Db.Connection as IDbConnectionInternals).ADOConnection is Microsoft.Data.SqlClient.SqlConnection)
			{
				dataProvider = "Core Microsoft SqlClient Data Provider";
			}
			else
			{
				dataProvider = "Core .Net SqlClient Data Provider";
			}
#endif

			AssertStartsWith("", $@"Error - Test System: A SQL exception has occurred when trying to process Component link [bucket1 -> bucket2]: 

The following SQL Exception(s) were caught:
Index #0
Message: It timed out... oh my dear!
 {DbCommand.SanitizeExecuteAsReaderFlags(executeAsReaderFlag)} 
Error Number: -2
Line Number: 1234
Source: {dataProvider}
Procedure: 

Statement:
-- bucket1 -> bucket2, Sequence: 69
SELECT ", logger.ToString());
		}

		public void TestWorkflowValidToTransferAlongTwoParallelLinks_WhenWorkflowAlreadyMovedAlongFirstLink_ShouldNotTransferAlongSecondLink()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket2");
			var bucket3 = BMSTestHelper.CreateBucket(system, "Bucket3");

			var link1 = BMSTestHelper.LinkComponents(bucket1, bucket2, sequence: 1);
			var link2 = BMSTestHelper.LinkComponents(bucket1, bucket3, sequence: 2);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "I hope our links aren't too slow!");

			Factory.Save();

			var logger = new BufferManagementLogger();
			var runner = new DummyTransferRulerRunnerWhichReturnsSpecificWorkflows(system, logger, workflow);

			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			runner.Process_ForTest();

			workflow.Reload();

			// This simulates what happens when updates happen on the primary server, but determination of workflows to transfer happens on the secondary server, which may not yet have received the updates from the primary server.
			AssertSamePK("Workflow should have been transferred along the first valid link, and not been considered transferrable on the second one because it's no longer in that link's 'from' component.", bucket2, workflow.CurrentComponent);

			AssertMultilineASCIIEquals($@"ZJ321WUNM8ISO6VDSJSPUZX5LL7F60: ZJ321WUNM8ISO6VDSJSPUZX5LL7F60: Moved workflow I hope our links aren't too slow! (PK = {workflow.PK}, Job = Organization (XVBQP68SIYXQ)) from component Bucket1 to Bucket2
ZJ321WUNM8ISO6VDSJSPUZX5LL7F60: ZJ321WUNM8ISO6VDSJSPUZX5LL7F60: Component link [Bucket1 -> Bucket2, Sequence: 1]: 1 workflow transferred
ZJ321WUNM8ISO6VDSJSPUZX5LL7F60: ZJ321WUNM8ISO6VDSJSPUZX5LL7F60: Component link [Bucket1 -> Bucket3, Sequence: 2]: 0 workflows transferred
", logger.ToString());
		}

		public void TestTransferSomeWorkflows_ShouldLogNumberOfTransferredWorkflowsOnly()
		{
			CreateAndTransferSomeWorkflows_AssertingExpectedLogContents(shouldLogContainWorkflowInfo: false);
		}

		public void TestTransferSomeWorkflows_WhenExtendedLoggingRegistryItemEnabled_ShouldLogNumberOfTransferredWorkflows_AndDetailsForEachWorkflow()
		{
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CreateAndTransferSomeWorkflows_AssertingExpectedLogContents(shouldLogContainWorkflowInfo: true);
		}

		void CreateAndTransferSomeWorkflows_AssertingExpectedLogContents(bool shouldLogContainWorkflowInfo)
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			for (var i = 0; i < 10; i++)
			{
				var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Some people have been saying, and they’ve been saying it very strongly, that PAVE is mainly responsible for Visual Boards. What do you say to that?");
			}

			config.ComponentLink.FL_IsReleaseGateRuleApplied = false;
			config.ComponentLink.FL_Name = "That’s a half truth!";

			Factory.Save();

			var logger = new BufferManagementLogger();
			var runner = new TestTransferRuleRunner(config.System, logger);

			runner.Process_ForTest();

			var log = logger.ToString();

			if (shouldLogContainWorkflowInfo)
			{
				AssertContains("PAVE is mainly responsible for Visual Boards", log);
			}
			else
			{
				AssertNotContains("PAVE is mainly responsible for Visual Boards", log);
			}

			AssertContains("Component link [That’s a half truth!]: 10 workflows transferred", log);
		}

		#endregion

		#region Performance

		public void TestQueryHeader()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, systemName: "Rona");

			config.ComponentLink.FL_IsReleaseGateRuleApplied = false;
			config.ComponentLink.FL_Name = ZString.Empty; // the version with name is tested in the next test.

			Factory.Save();

			using (TestConnection.TrackExecutedCommands())
			{
				new DummyTransferRuleRunner(config.System, new DummyLogger()).Process_ForTest();

				var command = TestConnection.ExecutedCommands.Single(c => c.Contains("SELECT \r\nFH_PK\r\n\tFROM dbo.ProcessHeader"));

				AssertStartsWith("The query should include the database name and system name so that we can track down bad queries that show up in Kibana. SAD!",
					$@"-- 
-- Database:  {TestConnection.CurrentDatabase}
-- System(s): Rona - ORG
-- Link:      bucket -> buffer, Sequence: 0
-- Type:	  Transfer Rules
SELECT", command);
			}
		}

		public void TestQueryHeader_MultipleSystems()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory, "ORG");
			system1.FS_Name = "You don't know the system!";
			var bucket1 = BMSTestHelper.CreateBucket(system1, "Bucket 1");

			var system2 = BMSTestHelper.CreateSystem(Factory, "INQ");
			system2.FS_Name = "Nobody mess with the system!";
			var bucket2 = BMSTestHelper.CreateBucket(system2, "Bucket 2");

			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);
			FilterStripsTestHelper.AddStartsWithFilter(link.FilterRule, ProcessHeader.ModuleFilterConstants.CompletionStatement, "W");

			Factory.Save();

			using (TestConnection.TrackExecutedCommands())
			{
				new DummyTransferRuleRunner(system1, new DummyLogger()).Process_ForTest();

				var command = TestConnection.ExecutedCommands.Single(c => c.Contains("SELECT \r\nFH_PK\r\n\tFROM dbo.ProcessHeader"));

				AssertStartsWith("The query should include the database name and both system names so that we can track down bad queries that show up in Kibana. SAD!",
					$@"-- 
-- Database:  {TestConnection.CurrentDatabase}
-- System(s): You don't know the system!/Nobody mess with the system!
-- Link:      Bucket 1 -> Bucket 2, Sequence: 0
-- Type:	  Transfer Rules
SELECT", command);
			}
		}

		public void TestTransferManyWorkflows_DbHits()
		{
			RunTransferManyWorkflows_DbHits(shouldContainExtraLogging: false);
		}

		public void TestTransferManyWorkflows_DbHits_WhenExtraLoggingEnabled()
		{
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			RunTransferManyWorkflows_DbHits(shouldContainExtraLogging: true);
		}

		void RunTransferManyWorkflows_DbHits(bool shouldContainExtraLogging)
		{
			var system = BMSTestHelper.CreateSystem(Factory, "INQ");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");
			var buffer = BMSTestHelper.CreateBucket(system, "buffer");
			var bucket3 = BMSTestHelper.CreateBucket(system, "bucket3");

			var link1 = BMSTestHelper.LinkComponents(bucket1, bucket2, sequence: 0);
			var link2 = BMSTestHelper.LinkComponents(bucket2, buffer, sequence: 1);
			var link3 = BMSTestHelper.LinkComponents(buffer, bucket3, sequence: 2);
			var link4 = BMSTestHelper.LinkComponents(bucket3, bucket2, sequence: 3);

			FilterStripsTestHelper.AddStartsWithFilter(link1.FilterRule, ProcessHeader.ModuleFilterConstants.CompletionStatement, "Allowed in bucket2");
			FilterStripsTestHelper.AddStartsWithFilter(link2.FilterRule, ProcessHeader.ModuleFilterConstants.CompletionStatement, "Allowed in buffer");
			FilterStripsTestHelper.AddStartsWithFilter(link3.FilterRule, ProcessHeader.ModuleFilterConstants.CompletionStatement, "Allowed in bucket3");
			FilterStripsTestHelper.AddStartsWithFilter(link4.FilterRule, ProcessHeader.ModuleFilterConstants.CompletionStatement, "Allowed in bucket2");

			const int batchSize = 10;

			foreach (var link in new[] { link1, link2, link3, link4 })
			{
				for (var i = 0; i < batchSize; i++)
				{
					BMSTestHelper.CreateWorkflowAndTask(Factory, "Allowed in " + link.ComponentTo.FC_Name, link.ComponentFrom);
					BMSTestHelper.CreateWorkflowAndTask(Factory, "Not allowed in " + link.ComponentTo.FC_Name, link.ComponentFrom);
				}
			}

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var logger = new SimpleLogger();
			var runner = new DummyTransferRuleRunner(newFactory.Load<BMSystem>(system.PK), logger, batchSize);

			const int numberOfBatchesProcessed = 4;

			var hits = new Dictionary<string, int>
			{
				{ OrgHeaderSchema.Constants.TableName, numberOfBatchesProcessed }, // We probably don't need these hits when logging is disabled. TODO: remove these hits in WI00233443.
				{ ProcessHeaderSchema.Constants.TableName, numberOfBatchesProcessed * 3 }, // It's 3 per batch. We load workflows in a fresh factory when transferring them. This is mainly so that the factory doing the initial heavy queries can be for a secondary server, but also so when we save the factory it doesn't have many bizos in it, running their OnFactorySaving methods.
				{ ProcessHeaderLinkSchema.Constants.TableName, numberOfBatchesProcessed * 2 },
				{ ProcessTasksSchema.Constants.TableName, numberOfBatchesProcessed }, // Per batch, when saving the transferred workflows.
				{ TagLinkSchema.Constants.TableName, 0 },
				{ BMComponentSchema.Constants.TableName, 0 }, // cached in Uber Factory, but may still be potentially loaded directly from db - we ensure there is no direct loading from db
			};

			// ignoreHitsFromTablesCachedInUberFactory is set to false, because we want to be sure we don't excessively reload BMComponents ignoring User Factory cache
			using (AssertDbHitsForAllFactories(hits, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: false, ignoreUnspecified: true))
			{
				runner.Process_ForTest();
			}

			if (shouldContainExtraLogging)
			{
				AssertContains("bucket1 to bucket2", logger.ToString());
			}
		}

		public void TestDbHits_WorkflowTransferWithoutDeactivation()
		{
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system1 = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var currentComponent1 = BMSTestHelper.CreateBucket(system1, "Current component 1", sequence: 0);
			var candidateComponent1 = BMSTestHelper.CreateBucket(system1, "Candidate component 1", sequence: 1);
			var candidateComponent2 = BMSTestHelper.CreateBucket(system1, "Candidate component 2", sequence: 2);
			var link1 = BMSTestHelper.LinkComponents(currentComponent1, candidateComponent1, isReleaseGate: false);
			var link2 = BMSTestHelper.LinkComponents(currentComponent1, candidateComponent2, isReleaseGate: false);
			link1.FL_Sequence = 0;
			link2.FL_Sequence = 1;

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Gate1",
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith
			});
			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Gate2",
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith
			});

			const int batchSize = 10;

			const int workflowBatchesForLink1Count = 4;
			const int workflowsForLink1Count = workflowBatchesForLink1Count * batchSize;
			var workflowsForLink1 = new ProcessHeader[workflowsForLink1Count];

			for (int i = 0; i < workflowsForLink1Count; i++)
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				workflow.FH_FC_CurrentComponent = currentComponent1.PK;
				workflow.FH_CompletionStatement = $"Gate1_{i}";
				workflowsForLink1[i] = workflow;
			}

			const int workflowBatchesForLink2Count = 2;
			const int workflowsForLink2Count = workflowBatchesForLink2Count * batchSize;
			var workflowsForLink2 = new ProcessHeader[workflowsForLink2Count];

			for (int i = 0; i < workflowsForLink2Count; i++)
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				workflow.FH_FC_CurrentComponent = currentComponent1.PK;
				workflow.FH_CompletionStatement = $"Gate2_{i}";
				workflowsForLink2[i] = workflow;
			}

			const int batchCount = workflowBatchesForLink1Count + workflowBatchesForLink2Count;

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var logger = new SimpleLogger();
			var runner = new DummyTransferRuleRunner(newFactory.Load<BMSystem>(system1.PK), logger, batchSize);

			var expectedHits = new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 0 }, // cached in Uber Factory, but may still be potentially loaded directly from db - we ensure there is no direct loading from db
				{ BMComponentLinkSchema.Constants.TableName, 0 },

					// 3 hits per batch:
					// 1) LinksProcessorWithDeactivation.ProcessLink - loading a batch
					// 2) LinksProcessorWithDeactivation.TransferWorkflowsOrSetDedicatedBuffer - TransferRuleRunnerDataAccessor.LoadWorkflowsToTransferCore
					// 3) LinksProcessorWithDeactivation.TransferWorkflowsOrSetDedicatedBuffer - TransferRuleRunnerDataAccessor.AddFetchHintsForWorkflowsToTransfer
				{ ProcessHeaderSchema.Constants.TableName, batchCount * 3 },

					// 2 hits per batch:
					// 1) TransferRuleRunnerDataAccessor.LoadWorkflowsToTransfer - ProcessHeaderLink.LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects - ProcessHeaderLink.LoadLinksIntoFactory, loading linksFrom
					// 2) The same, but loading linksTo
				{ ProcessHeaderLinkSchema.Constants.TableName, batchCount * 2 },

					// 1 hits per batch in TransferRuleRunner.TransferWorkflowsOrSetDedicatedBuffer() for updating Planned Duration on save in ProcessHeader.OnSavingForService
				{ ProcessTasksSchema.Constants.TableName, batchCount },

					// 1 hit per batch: TransferRuleRunner.TransferWorkflowsOrSetDedicatedBuffer - SavePendingWorkflows - ProcessHeader.UpdateDatesFromJob - Parent
				{ DummyBizoSchema.Constants.TableName, batchCount },
				{ TagLinkSchema.Constants.TableName, 0 }
			};

			// ignoreHitsFromTablesCachedInUberFactory is set to false, because we want to be sure we don't excessively reload BMComponents ignoring User Factory cache
			using (AssertDbHitsForAllFactories(expectedHits, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: false, ignoreUnspecified: true))
			{
				runner.Process_ForTest();
			}

			foreach (var workflow in workflowsForLink1)
			{
				AssertEquals(candidateComponent1.PK, workflow.FH_FC_CurrentComponent);
				AssertEquals("Should not be deactivated", true, workflow.FH_IsActive);
			}

			foreach (var workflow in workflowsForLink2)
			{
				AssertEquals(candidateComponent2.PK, workflow.FH_FC_CurrentComponent);
				AssertEquals("Should not be deactivated", true, workflow.FH_IsActive);
			}
		}

		public void TestDbHits_WorkflowTransferAndDeactivation()
		{
			BMSRegistry.Instance.TransferRuleBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system1 = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var currentComponent1 = BMSTestHelper.CreateBucket(system1, "Current component 1", sequence: 0);
			var candidateComponent1 = BMSTestHelper.CreateBucket(system1, "Candidate component 1", sequence: 1);
			var candidateComponent2 = BMSTestHelper.CreateBucket(system1, "Candidate component 2", sequence: 2);
			var link1_1 = BMSTestHelper.LinkComponents(currentComponent1, candidateComponent1, sequence: 0, isReleaseGate: false);
			var link1_2 = BMSTestHelper.LinkComponents(candidateComponent1, currentComponent1, sequence: 1, isReleaseGate: false); // loop
			var link2_1 = BMSTestHelper.LinkComponents(currentComponent1, candidateComponent2, sequence: 2, isReleaseGate: false);
			var link2_2 = BMSTestHelper.LinkComponents(candidateComponent2, currentComponent1, sequence: 3, isReleaseGate: false); // loop
			link1_1.FL_Sequence = 0;
			link2_1.FL_Sequence = 1;

			FilterStripsTestHelper.AddFilterStrips(link1_1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Gate1",
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith
			});
			FilterStripsTestHelper.AddFilterStrips(link1_2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Gate1",
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith
			});
			FilterStripsTestHelper.AddFilterStrips(link2_1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Gate2",
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith
			});
			FilterStripsTestHelper.AddFilterStrips(link2_2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Gate2",
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith
			});

			const int batchSize = 10;

			const int workflowBatchesForLinks1Count = 4;
			const int workflowsForLink1Count = workflowBatchesForLinks1Count * batchSize;
			var workflowsForLink1 = new ProcessHeader[workflowsForLink1Count];

			for (int i = 0; i < workflowsForLink1Count; i++)
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				workflow.FH_FC_CurrentComponent = currentComponent1.PK;
				workflow.FH_CompletionStatement = $"Gate1_{i}";
				workflowsForLink1[i] = workflow;
			}

			const int workflowBatchesForLinks2Count = 2;
			const int workflowsForLink2Count = workflowBatchesForLinks2Count * batchSize;
			var workflowsForLink2 = new ProcessHeader[workflowsForLink2Count];

			for (int i = 0; i < workflowsForLink2Count; i++)
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				workflow.FH_FC_CurrentComponent = currentComponent1.PK;
				workflow.FH_CompletionStatement = $"Gate2_{i}";
				workflowsForLink2[i] = workflow;
			}

			const int batchCount = (workflowBatchesForLinks1Count + workflowBatchesForLinks2Count) * 2; // each workflow is processed by two links (back and forth)

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var logger = new SimpleLogger();
			var runner = new DummyTransferRuleRunner(newFactory.Load<BMSystem>(system1.PK), logger, batchSize);

			var expectedHits = new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 1 }, // cached in Uber Factory, but may still be potentially loaded directly from db - we ensure there is no direct loading from db
				{ BMComponentLinkSchema.Constants.TableName, 0 },

					// 3 hits per batch:
					// 1) LinksProcessorWithDeactivation.ProcessLink - loading a batch
					// 2) LinksProcessorWithDeactivation.TransferWorkflowsOrSetDedicatedBuffer - TransferRuleRunnerDataAccessor.LoadWorkflowsToTransferCore
					// 3) LinksProcessorWithDeactivation.TransferWorkflowsOrSetDedicatedBuffer - TransferRuleRunnerDataAccessor.AddFetchHintsForWorkflowsToTransfer

					// Plus 2 extra db hits:
					//1) TransferRuleRunnerDataAccessor.LoadWorkflowsToDeactivate - WorkflowLoader.LoadWorkflowsByPKUsingTableValuedParameter
					//2) TransferRuleRunnerDataAccessor.LoadWorkflowsToDeactivate - AddFetchHintsForWorkflowsToTransfer - ProcessHeader.JobHeader
				{ ProcessHeaderSchema.Constants.TableName, batchCount * 3 + 2 },

					// 2 hits per batch:
					// 1) TransferRuleRunnerDataAccessor.LoadWorkflowsToTransfer - ProcessHeaderLink.LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects - ProcessHeaderLink.LoadLinksIntoFactory, loading linksFrom
					// 2) The same, but loading linksTo

					// Plus 2 extra db hits:
					// 1) TransferRuleRunnerDataAccessor.LoadWorkflowsToDeactivate - ProcessHeaderLink.LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects - ProcessHeaderLink.LoadLinksIntoFactory, loading linksFrom
					// 2) The same, but loading linksTo
				{ ProcessHeaderLinkSchema.Constants.TableName, batchCount * 2 + 2 },

					// 1 hits per batch in TransferRuleRunner.TransferWorkflowsOrSetDedicatedBuffer() for updating Planned Duration on save in ProcessHeader.OnSavingForService
					// Plus 1 extra db hit for doing the same when deactivating workflows
				{ ProcessTasksSchema.Constants.TableName, batchCount + 1 },

					// 1 hit per batch: TransferRuleRunner.TransferWorkflowsOrSetDedicatedBuffer - SavePendingWorkflows - ProcessHeader.UpdateDatesFromJob - Parent
					// Plus one extra db hit for displaying job names in logs when deactivating workflows: TransferRuleRunner.ShouldSaveOnProcessedFunc - ProcessHeader.ParentJobDescription
				{ DummyBizoSchema.Constants.TableName, batchCount + 1 },
				{ TagLinkSchema.Constants.TableName, 0 }
			};

			// ignoreHitsFromTablesCachedInUberFactory is set to false, because we want to be sure we don't excessively reload BMComponents ignoring User Factory cache
			using (AssertDbHitsForAllFactories(expectedHits, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: false, ignoreUnspecified: true))
			{
				runner.Process_ForTest();
			}

			foreach (var workflow in workflowsForLink1.Union(workflowsForLink2))
			{
				AssertEquals(currentComponent1.PK, workflow.FH_FC_CurrentComponent);
				AssertEquals("Should be deactivated", false, workflow.FH_IsActive);
			}
		}

		public void TestDbHits_SettingDedicatedBuffer()
		{
			BMSRegistry.Instance.TransferRuleBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var currentComponent1 = BMSTestHelper.CreateBucket(system, "Current component 1");
			var candidateComponent1 = BMSTestHelper.CreateBuffer(system, "Candidate component 1");
			var candidateComponent2 = BMSTestHelper.CreateBuffer(system, "Candidate component 2");
			var link1 = BMSTestHelper.LinkComponents(currentComponent1, candidateComponent1, isReleaseGate: true);
			var link2 = BMSTestHelper.LinkComponents(currentComponent1, candidateComponent2, isReleaseGate: true);
			link1.FL_Sequence = 0;
			link2.FL_Sequence = 1;

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Gate1",
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith
			});
			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Gate2",
				ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith
			});

			const int batchSize = 10;

			const int workflowBatchesForLink1Count = 4;
			const int workflowsForLink1Count = workflowBatchesForLink1Count * batchSize;
			var workflowsForLink1 = new ProcessHeader[workflowsForLink1Count];

			for (int i = 0; i < workflowsForLink1Count; i++)
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				workflow.FH_FC_CurrentComponent = currentComponent1.PK;
				workflow.FH_CompletionStatement = $"Gate1_{i}";
				workflowsForLink1[i] = workflow;
			}

			const int workflowBatchesForLink2Count = 2;
			const int workflowsForLink2Count = workflowBatchesForLink2Count * batchSize;
			var workflowsForLink2 = new ProcessHeader[workflowsForLink2Count];

			for (int i = 0; i < workflowsForLink2Count; i++)
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = jobHeader.ProcessHeaders.AddNew();
				workflow.FH_FC_CurrentComponent = currentComponent1.PK;
				workflow.FH_CompletionStatement = $"Gate2_{i}";
				workflowsForLink2[i] = workflow;
			}

			const int batchCount = workflowBatchesForLink1Count + workflowBatchesForLink2Count;

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var logger = new SimpleLogger();
			var runner = new DummyTransferRuleRunner(newFactory.Load<BMSystem>(system.PK), logger, batchSize);

			var expectedHits = new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 0 }, // cached in Uber Factory, but may still be potentially loaded directly from db - we ensure there is no direct loading from db
				{ BMComponentLinkSchema.Constants.TableName, 0 },

					// 3 hits per batch:
					// 1) LinksProcessorWithDeactivation.ProcessLink - loading a batch
					// 2) LinksProcessorWithDeactivation.TransferWorkflowsOrSetDedicatedBuffer - TransferRuleRunnerDataAccessor.LoadWorkflowsToTransferCore
					// 3) LinksProcessorWithDeactivation.TransferWorkflowsOrSetDedicatedBuffer - TransferRuleRunnerDataAccessor.AddFetchHintsForWorkflowsToTransfer
				{ ProcessHeaderSchema.Constants.TableName, batchCount * 3 },

					// 2 hits per batch:
					// 1) TransferRuleRunnerDataAccessor.LoadWorkflowsToTransfer - ProcessHeaderLink.LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects - ProcessHeaderLink.LoadLinksIntoFactory, loading linksFrom
					// 2) The same, but loading linksTo
				{ ProcessHeaderLinkSchema.Constants.TableName, batchCount * 2 },

					// 1 hits per batch in TransferRuleRunner.TransferWorkflowsOrSetDedicatedBuffer() for updating Planned Duration on save in ProcessHeader.OnSavingForService
				{ ProcessTasksSchema.Constants.TableName, batchCount },

					// 1 hit per batch: TransferRuleRunner.TransferWorkflowsOrSetDedicatedBuffer - SavePendingWorkflows - ProcessHeader.UpdateDatesFromJob - Parent
				{ DummyBizoSchema.Constants.TableName, batchCount }
			};

			// ignoreHitsFromTablesCachedInUberFactory is set to false, because we want to be sure we don't excessively reload BMComponents ignoring User Factory cache
			using (AssertDbHitsForAllFactories(expectedHits, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: false, ignoreUnspecified: true))
			{
				runner.Process_ForTest();
			}
		}

		public void TestTransfer_ShouldNotCreateTerribleProcessHeaderLinkQueries()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "INQ");
			var buffer = BMSTestHelper.CreateBucket(system, "buffer");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");

			var link = BMSTestHelper.LinkComponents(bucket2, buffer);

			FilterStripsTestHelper.AddStartsWithFilter(link.FilterRule, ProcessHeader.ModuleFilterConstants.CompletionStatement, "Allowed in buffer");
			BMSTestHelper.CreateWorkflowAndTask(Factory, "Allowed in " + link.ComponentTo.FC_Name, link.ComponentFrom);
			BMSTestHelper.CreateWorkflowAndTask(Factory, "Not allowed in " + link.ComponentTo.FC_Name, link.ComponentFrom);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var runner = new DummyTransferRuleRunner(newFactory.Load<BMSystem>(system.PK), new DummyLogger(), batchSize: 10);
			IEnumerable<string> trackedCommands;

			using (TestConnection.TrackExecutedCommands())
			{
				runner.Process_ForTest();
				trackedCommands = TestConnection.ExecutedCommands;
			}

			var processHeaderLinkCommands = trackedCommands.Where(x => x.Contains(nameof(ProcessHeaderLink))).ToArray();
			var badCommands = processHeaderLinkCommands.Where(x => x.Contains(" or FP_FH_HeaderTo", StringComparison.OrdinalIgnoreCase) || x.Contains(" or FP_FH_HeaderFrom", StringComparison.OrdinalIgnoreCase));
			AssertContainsExactElementsInAnyOrder("Should not use queries with billions and billions of ORs", Enumerable.Empty<string>(), badCommands);
		}

		public void TestTransfer_ShouldNotUseTableValuedParameters_WhenOnlyOneComponetAndNoTableOrIndexScans()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = CreateBucket(system, "b1");
			var bucket2 = CreateBucket(system, "b2");

			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);
			link.FL_Name = "test link";

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, completionStatement: "workflow");
			workflow.FH_FC_CurrentComponent = bucket1.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false, };

			var config = new WorkflowLoadConfig();
			var source = new WorkflowComponentsData(Array.Empty<ZQuery>(), WorkflowLoader.GetStmModuleFilterQuery(link.FilterRule, shouldOptimiseForWorkflowsOnly: true), new[] { link.FL_FC_ComponentFrom });
			var query = WorkflowLoader.GetQuery(config, source);

			var logger = new BufferManagementLogger();
			var runner = new DummyTransferRuleRunner(system, logger);

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				runner.Process_ForTest();

				AssertEquals("Should transfer workflow from b1 to b2", bucket2.PK, workflow.FH_FC_CurrentComponent);

				var (sqlQuery, plan) = TestConnection.ExecutedCommandsAndQueryPlans.First(c => c.Item1.Contains(link.FL_Name));
				var queryPlanAnalyzer = new QueryPlanalyzer(plan.First());

				AssertContains("Should not use select in", "FH_FC_CurrentComponent = ", sqlQuery);
				AssertEquals("Should be no index scans", expected: false, queryPlanAnalyzer.IndexScans.Any());
				AssertEquals("Should be no table scans", expected: false, queryPlanAnalyzer.TableScans.Any());
				AssertEquals("Should be only one index seek", 1, queryPlanAnalyzer.IndexSeeks.Count());
				AssertEquals("IndexSeek should be on NR_RX__FH_FC_CurrentComponent_FH_IsActive_FH_P0_Template_FH_PK", "NR_RX__FH_FC_CurrentComponent_FH_IsActive_FH_P0_Template_FH_PK", queryPlanAnalyzer.IndexSeeks.Single().IndexName);
			}
		}

		#endregion

		#region Branch and Department

		[TestDate(2024, 05, 15, 10, 30, 00)]
		public void TestLinksUsesAgingBranchAndDepartmentCorrectly()
		{
			AssertEquals(DayOfWeek.Wednesday, ZDateTime.Now.DayOfWeek);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var fromBucket = BMSTestHelper.CreateBucket(system, "fromBucket");
			var toBucket = BMSTestHelper.CreateBucket(system, "toBucket");
			var link = BMSTestHelper.LinkComponents(fromBucket, toBucket);

			Guid defaultBranchPk;
			Guid defaultDepartmentPk;

			using (new BMSServiceTaskHelper().GetTemporaryEnvironmentForServiceTaskBranch())
			{
				defaultBranchPk = Env.CurrentBranchPK;
				defaultDepartmentPk = Env.CurrentDepartmentPK;
			}

			var fromBranch = Factory.NewWithValidTestData<GlbBranch>();
			var fromBranchPK = fromBranch.PK;
			var fromDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var fromDepartmentPK = fromDepartment.PK;
			fromBucket.FC_GB_AgingBranch = fromBranch.PK;
			fromBucket.FC_GE_AgingDepartment = fromDepartment.PK;

			var toBranch = Factory.NewWithValidTestData<GlbBranch>();
			var toBranchPK = toBranch.PK;
			var toDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var toDepartmentPK = toDepartment.PK;
			toBucket.FC_GB_AgingBranch = toBranch.PK;
			toBucket.FC_GE_AgingDepartment = toDepartment.PK;

			var filter = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate,
				FilterStripValueSetter = f =>
				{
					var dataFilter = f as ModuleDateFilter;
					dataFilter.PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange;
					dataFilter.Property2 = new ZDateTime(2024, 1, 1, 8, 0, 0);
					dataFilter.PropertyDecimal2 = 8m;
					dataFilter.FilterOption = DateOffsetRangeFilterOptions.Codes.Future;
				}
			};

			FilterStripsTestHelper.AddFilterStrips(link.FilterRule, filter);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(1);
			AssertEquals(DayOfWeek.Thursday, workflow.FH_AgreedDeliveryDate.DayOfWeek);

			Factory.Save();

			var logger = new BufferManagementLogger();
			var runner = new TestTransferRuleRunner(system, logger);

			Guid transferDepartmentPK, transferBranchPK;
			transferDepartmentPK = transferBranchPK = Guid.Empty;
			var workingDaysProvider = ObjectFactory.Get<IWorkingDaysProvider>();
			var mockWorkingDaysProvider = new Mock<IWorkingDaysProvider>();
			mockWorkingDaysProvider
				.Setup(m => m.GetWorkingDays(It.IsAny<BusinessObjectFactory>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>()))
				.Returns((BusinessObjectFactory factory, ZGuid departmentPK, ZGuid branchPK, ZGuid staffPK) =>
				{
					transferDepartmentPK = departmentPK.ToGuid();
					transferBranchPK = branchPK.ToGuid();
					return workingDaysProvider.GetWorkingDays(factory, departmentPK, branchPK, staffPK);
				});

			using (ObjectFactory.Substitute(mockWorkingDaysProvider.Object))
			{
				runner.Process_ForTest();
			}

			workflow.Reload();
			AssertEquals(toBranchPK, transferBranchPK);
			AssertEquals(toDepartmentPK, transferDepartmentPK);
			AssertEquals("Not transferred because today is a working day in the toDepartment", fromBucket.PK, workflow.FH_FC_CurrentComponent);

			toBucket.FC_GB_AgingBranch = ZGuid.Empty;
			toBucket.FC_GE_AgingDepartment = ZGuid.Empty;
			Factory.Save();

			using (ObjectFactory.Substitute(mockWorkingDaysProvider.Object))
			{
				runner.Process_ForTest();
			}

			workflow.Reload();
			AssertEquals(fromBranchPK, transferBranchPK);
			AssertEquals(fromDepartmentPK, transferDepartmentPK);
			AssertEquals("Not transferred because today is a working day in the fromDepartment", fromBucket.PK, workflow.FH_FC_CurrentComponent);

			fromBucket.FC_GB_AgingBranch = ZGuid.Empty;
			fromBucket.FC_GE_AgingDepartment = ZGuid.Empty;
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, defaultDepartmentPk, DayOfWeek.Friday, WorkingDaysTestHelper.NineToFive);
			Factory.Save();

			using (ObjectFactory.Substitute(mockWorkingDaysProvider.Object))
			{
				runner.Process_ForTest();
			}

			workflow.Reload();
			AssertEquals(defaultBranchPk, transferBranchPK);
			AssertEquals(defaultDepartmentPk, transferDepartmentPK);
			AssertEquals("Transferred because the next work day is Friday after the AgreedDeliveryDate", toBucket.PK, workflow.FH_FC_CurrentComponent);
		}

		public void TestShouldUpdateEffectiveBranchDepartmentOnTransfer_IgnoringUberFactoryCache_WhenMovingToBufferRequiresReleaseGate()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket 2");
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var link1 = BMSTestHelper.LinkComponents(bucket1, bucket2, sequence: 0);
			var link2 = BMSTestHelper.LinkComponents(bucket2, buffer, sequence: 1);

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var agingBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			var agingBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			var agingDepartment1 = Factory.NewWithValidTestData<GlbDepartment>();
			var agingDepartment2 = Factory.NewWithValidTestData<GlbDepartment>();
			buffer.FC_GB_AgingBranch = agingBranch1.PK;
			buffer.FC_GE_AgingDepartment = agingDepartment1.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_CompletionStatement = "Green";
			workflow.FH_FC_CurrentComponent = bucket1.PK;
			BMSTestHelper.CreateTask(workflow, taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			Factory.Save();

			using (var command = Db.Connection.Command($@"
UPDATE dbo.BMComponent
SET FC_GB_AgingBranch = '{agingBranch2.PK}',
	FC_GE_AgingDepartment = '{agingDepartment2.PK}',
	FC_SystemLastEditTimeUtc = getdate(),
	FC_SystemLastEditUser = '~BP'
WHERE FC_PK = '{buffer.PK}'
"))
			{
				command.ExecuteNonQuery();
			}

			var logger = new BufferManagementLogger();
			var runner = new TestTransferRuleRunner(system, logger);
			runner.Process_ForTest();

			workflow.Reload();

			AssertEquals("Should move to the pre-buffer component", bucket2.PK, workflow.FH_FC_CurrentComponent);
			AssertEquals("Should set the dedicated buffer", buffer.PK, workflow.FH_FC_DedicatedBuffer);
			AssertEquals("Should take the effective branch from the dedicated buffer - should ignore Uber Cache", agingBranch2.PK, workflow.FH_GB_EffectiveBranch);
			AssertEquals("Should take the effective department from the dedicated buffer - should ignore Uber Cache", agingDepartment2.PK, workflow.FH_GE_EffectiveDepartment);

			workflow.FH_FC_CurrentComponent = bucket1.PK;
			Factory.Save();

			using (var command = Db.Connection.Command($@"
UPDATE dbo.BMComponent
SET FC_GB_AgingBranch = '{agingBranch1.PK}',
	FC_GE_AgingDepartment = '{agingDepartment1.PK}',
	FC_SystemLastEditTimeUtc = getdate(),
	FC_SystemLastEditUser = '~BP'
WHERE FC_PK = '{buffer.PK}'
"))
			{
				command.ExecuteNonQuery();
			}

			logger = new BufferManagementLogger();
			runner = new TestTransferRuleRunner(system, logger);
			runner.Process_ForTest();

			workflow.Reload();

			AssertEquals("Should move to the pre-buffer component", bucket2.PK, workflow.FH_FC_CurrentComponent);
			AssertEquals("Should set the dedicated buffer", buffer.PK, workflow.FH_FC_DedicatedBuffer);
			AssertEquals("Should take the effective branch from the dedicated buffer - should ignore Uber Cache", agingBranch1.PK, workflow.FH_GB_EffectiveBranch);
			AssertEquals("Should take the effective department from the dedicated buffer - should ignore Uber Cache", agingDepartment1.PK, workflow.FH_GE_EffectiveDepartment);
		}

		public void TestShouldUpdateEffectiveBranchDepartmentOnTransfer_IgnoringUberFactoryCache_WhenMovingToBufferDoesNotRequireReleaseGate()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket 2");
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var link1 = BMSTestHelper.LinkComponents(bucket1, bucket2, sequence: 0);
			var link2 = BMSTestHelper.LinkComponents(bucket2, buffer, sequence: 1, isReleaseGate: false); // bypass

			FilterStripsTestHelper.AddFilterStrips(link1.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});
			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Green",
			});

			var agingBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			var agingBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			var agingDepartment1 = Factory.NewWithValidTestData<GlbDepartment>();
			var agingDepartment2 = Factory.NewWithValidTestData<GlbDepartment>();
			buffer.FC_GB_AgingBranch = agingBranch1.PK;
			buffer.FC_GE_AgingDepartment = agingDepartment1.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_CompletionStatement = "Green";
			workflow.FH_FC_CurrentComponent = bucket1.PK;

			BMSTestHelper.CreateTask(workflow, taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			Factory.Save();

			using (var command = Db.Connection.Command($@"
UPDATE dbo.BMComponent
SET FC_GB_AgingBranch = '{agingBranch2.PK}',
	FC_GE_AgingDepartment = '{agingDepartment2.PK}',
	FC_SystemLastEditTimeUtc = getdate(),
	FC_SystemLastEditUser = '~BP'
WHERE FC_PK = '{buffer.PK}'
"))
			{
				command.ExecuteNonQuery();
			}

			var logger = new BufferManagementLogger();
			var runner = new TestTransferRuleRunner(system, logger);
			runner.Process_ForTest();

			workflow.Reload();

			AssertEquals("Should move to the buffer component", buffer.PK, workflow.FH_FC_CurrentComponent);
			AssertEquals("Should have no dedicated buffer set", ZGuid.Empty, workflow.FH_FC_DedicatedBuffer);
			AssertEquals("Should take the effective branch from the buffer - should ignore Uber Cache", agingBranch2.PK, workflow.FH_GB_EffectiveBranch);
			AssertEquals("Should take the effective department from the buffer - should ignore Uber Cache", agingDepartment2.PK, workflow.FH_GE_EffectiveDepartment);

			workflow.FH_FC_CurrentComponent = bucket1.PK;
			Factory.Save();

			using (var command = Db.Connection.Command($@"
UPDATE dbo.BMComponent
SET FC_GB_AgingBranch = '{agingBranch1.PK}',
	FC_GE_AgingDepartment = '{agingDepartment1.PK}',
	FC_SystemLastEditTimeUtc = getdate(),
	FC_SystemLastEditUser = '~BP'
WHERE FC_PK = '{buffer.PK}'
"))
			{
				command.ExecuteNonQuery();
			}

			logger = new BufferManagementLogger();
			runner = new TestTransferRuleRunner(system, logger);
			runner.Process_ForTest();

			workflow.Reload();

			AssertEquals("Should move to the buffer component", buffer.PK, workflow.FH_FC_CurrentComponent);
			AssertEquals("Should have no dedicated buffer set", ZGuid.Empty, workflow.FH_FC_DedicatedBuffer);
			AssertEquals("Should take the effective branch from the buffer - should ignore Uber Cache", agingBranch1.PK, workflow.FH_GB_EffectiveBranch);
			AssertEquals("Should take the effective department from the buffer - should ignore Uber Cache", agingDepartment1.PK, workflow.FH_GE_EffectiveDepartment);
		}

		#endregion

		#region Implementation

		class DummyTransferRulerRunnerQueryException : TestTransferRuleRunner
		{
			internal DummyTransferRulerRunnerQueryException(BMSystem system, ILogger logger)
				: this(system, new TransferRuleRunnerLogger(logger, system))
			{
			}

			DummyTransferRulerRunnerQueryException(BMSystem system, ITransferRuleRunnerLogger logger)
				: base(system, logger, dataAccessor: new DummyTransferRulerRunnerQueryExceptionDataAccessor(logger))
			{
			}

			class DummyTransferRulerRunnerQueryExceptionDataAccessor : TransferRuleRunnerDataAccessor
			{
				public DummyTransferRulerRunnerQueryExceptionDataAccessor(ITransferRuleRunnerLogger logger)
					: base(logger)
				{
				}

				protected override ZQuery GetLinkAssociatedWorkflowQueryCore(BMComponentLink link)
				{
					var query = new ZDBOnlyQuery(typeof(ProcessHeader));
					query.AddFilterAndZSQLParameterCollection("NOT A REAL QUERY", new ZSqlParameterCollection());
					return query;
				}
			}
		}

		class ZConcurrencyCheckFailureExceptionRunner : TestTransferRuleRunner
		{
			internal ZConcurrencyCheckFailureExceptionRunner(BMSystem system, ILogger logger)
				: this(system, new TransferRuleRunnerLogger(logger, system))
			{
			}

			ZConcurrencyCheckFailureExceptionRunner(BMSystem system, ITransferRuleRunnerLogger logger)
				: base(system, logger, dataAccessor: new ZConcurrencyCheckFailureExceptionRunnerDataAccessor(logger))
			{
			}

			class ZConcurrencyCheckFailureExceptionRunnerDataAccessor : TransferRuleRunnerDataAccessor
			{
				public ZConcurrencyCheckFailureExceptionRunnerDataAccessor(ITransferRuleRunnerLogger logger)
					: base(logger)
				{
				}

				protected override ZQuery GetLinkAssociatedWorkflowQueryCore(BMComponentLink link)
				{
					throw new ZConcurrencyCheckFailureException("test ZConcurrencyCheckFailureException", "ZConcurrencyCheckFailureException", true);
				}
			}
		}

		class DummyTransferRulerRunnerWhichReturnsSpecificWorkflows : TestTransferRuleRunner
		{
			internal DummyTransferRulerRunnerWhichReturnsSpecificWorkflows(BMSystem system, ILogger logger, params ProcessHeader[] workflowsToTransferOnAllLinks)
				: this(system, new TransferRuleRunnerLogger(logger, system), workflowsToTransferOnAllLinks)
			{
			}

			DummyTransferRulerRunnerWhichReturnsSpecificWorkflows(BMSystem system, ITransferRuleRunnerLogger logger, params ProcessHeader[] workflowsToTransferOnAllLinks)
				: base(system, logger, dataAccessor: new DummyTransferRulerRunnerWhichReturnsSpecificWorkflowsDataAccessor(logger, workflowsToTransferOnAllLinks))
			{
			}

			class DummyTransferRulerRunnerWhichReturnsSpecificWorkflowsDataAccessor : TransferRuleRunnerDataAccessor
			{
				public DummyTransferRulerRunnerWhichReturnsSpecificWorkflowsDataAccessor(ITransferRuleRunnerLogger logger, IEnumerable<ProcessHeader> workflowsToTransferOnAllLinks)
					: base(logger)
				{
					this.workflowsToTransferOnAllLinks = workflowsToTransferOnAllLinks;
				}

				readonly IEnumerable<ProcessHeader> workflowsToTransferOnAllLinks;

				protected override ZQuery GetLinkAssociatedWorkflowQueryCore(BMComponentLink link)
				{
					return new ZQuery(ProcessHeaderSchema.PK, workflowsToTransferOnAllLinks.Select(w => w.PK));
				}
			}
		}

		class DummyTransferRulerRunnerWhichThrowsReaderException : TestTransferRuleRunner
		{
			internal DummyTransferRulerRunnerWhichThrowsReaderException(BMSystem system, ILogger logger, int sqlExceptionNumber, string exceptionMessage)
				: this(system, new TransferRuleRunnerLogger(logger, system), sqlExceptionNumber, exceptionMessage)
			{
			}
			DummyTransferRulerRunnerWhichThrowsReaderException(BMSystem system, ITransferRuleRunnerLogger logger, int sqlExceptionNumber, string exceptionMessage)
				: base(system, logger, dataAccessor: new DummyTransferRulerRunnerWhichThrowsReaderExceptionDataAccessor(logger, sqlExceptionNumber, exceptionMessage))
			{
			}

			class DummyTransferRulerRunnerWhichThrowsReaderExceptionDataAccessor : TransferRuleRunnerDataAccessor
			{
				public DummyTransferRulerRunnerWhichThrowsReaderExceptionDataAccessor(ITransferRuleRunnerLogger logger, int sqlExceptionNumber, string exceptionMessage)
					: base(logger)
				{
					SqlExceptionNumber = sqlExceptionNumber;
					ExceptionMessage = exceptionMessage;
				}

				internal readonly int SqlExceptionNumber;
				internal readonly string ExceptionMessage;

				protected override string GetWorkflowQueryNameCore(BMComponentLink link) => link.DisplayText;

				protected override IWorkflowBatchLoader GetLinkAssociatedWorkflowBatchLoaderCore(IComponentLink componentLink, bool useSecondaryServerIfAllowed)
					=> new DummyTransferRulerRunnerWhichThrowsReaderExceptionWorkflowBatchLoader(this, componentLink, Logger, () => GetLinkAssociatedWorkflowQuery(componentLink));
			}

			class DummyTransferRulerRunnerWhichThrowsReaderExceptionWorkflowBatchLoader : LinkAssociatedWorkflowBatchLoader
			{
				public DummyTransferRulerRunnerWhichThrowsReaderExceptionWorkflowBatchLoader(TransferRuleRunnerDataAccessor dataAccessor, IComponentLink componentLink, ILogger logger, Func<(ZQuery query, string queryName)> queryProvider)
					: base(dataAccessor, componentLink, logger, queryProvider, shouldUseSecondaryServerIfAllowed: true)
				{
				}

				protected override BatchLogger GetBatchLoggerCore() => new DodgyLogger((DummyTransferRulerRunnerWhichThrowsReaderExceptionDataAccessor)FactoryProvider, Logger);
			}

			class DodgyLogger : BatchLogger
			{
				public DodgyLogger(DummyTransferRulerRunnerWhichThrowsReaderExceptionDataAccessor parent, ILogger logger)
					: base(logger, null)
				{
					this.parent = parent;
				}

				readonly DummyTransferRulerRunnerWhichThrowsReaderExceptionDataAccessor parent;

				public override IDisposable BatchLoadStarting()
				{
					return new DisposableAction(() =>
					{
						throw SqlExceptionBuilder.CreateSqlException(SqlExceptionBuilder.CreateSqlErrorCollection(SqlExceptionBuilder.CreateSqlError(parent.SqlExceptionNumber, 0, 0, string.Empty, parent.ExceptionMessage, string.Empty, 1234)));
					});
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "DE";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "DEHAM";
			branch.GB_RN_NKCountryCode = "DE";

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: branch.PK.ToGuid());
			Factory.Save();
		}

		#endregion
	}
}
