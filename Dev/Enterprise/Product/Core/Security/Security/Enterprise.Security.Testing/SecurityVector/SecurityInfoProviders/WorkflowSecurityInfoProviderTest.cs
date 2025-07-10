using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.Provider;

namespace Enterprise.Security.Testing
{
	sealed class WorkflowSecurityInfoProviderTest : TestCaseWithFactory
	{
		WorkflowSecurityInfoProvider workflowSecurityInfoProvider;

		public void TestChildrenElements()
		{
			var result = workflowSecurityInfoProvider.GetChildren().Select(provider => provider.Checkpoint.Code).ToArray();

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"OperationsWorkflowView Tasks",
				"OperationsWorkflowView Milestones",
				"OperationsWorkflowView Exceptions",
				"OperationsWorkflowView Triggers",
				"OperationsWorkflowView Events",
				"OperationsWorkflowTasks",
				"OperationsWorkflowMilestones",
				"OperationsWorkflowExceptions",
				"OperationsWorkflowTriggers",
				"OperationsWorkflowEvents",
			}, result);
		}

		public void TestEditElementsContainChildren()
		{
			CombineAssertions(() =>
			{
				var securityInfoProviders = workflowSecurityInfoProvider.GetChildren().ToArray();
				AssertSecurityCheckpoint(securityInfoProviders, "OperationsWorkflowTasks", new[]
				{
					"OperationsWorkflowTasksAdd Tasks",
					"OperationsWorkflowTasksDelete Tasks",
				});
				AssertSecurityCheckpoint(securityInfoProviders, "OperationsWorkflowMilestones", new[]
				{
					"OperationsWorkflowMilestonesAdd Milestones",
					"OperationsWorkflowMilestonesDelete Milestones",
				});
				AssertSecurityCheckpoint(securityInfoProviders, "OperationsWorkflowExceptions", new[]
				{
					"OperationsWorkflowExceptionsAdd Exceptions",
					"OperationsWorkflowExceptionsDelete Exceptions",
					"OperationsWorkflowExceptionsOverride Exception Duration"
				});
				AssertSecurityCheckpoint(securityInfoProviders, "OperationsWorkflowTriggers", new[]
				{
					"OperationsWorkflowTriggersAdd Triggers",
					"OperationsWorkflowTriggersDelete Triggers",
				});
			});

			void AssertSecurityCheckpoint(IEnumerable<SecurityInfoProvider> securityInfoProviders, string operationsWorkflowCode, string[] expected)
			{
				var securityInfoProvider = securityInfoProviders.Single(provider => provider.Checkpoint.Code == operationsWorkflowCode);
				var result = securityInfoProvider.GetChildren().Select(provider => provider.Checkpoint.Code).ToArray();

				AssertContainsExactElementsInAnyOrder(expected, result);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			workflowSecurityInfoProvider = new WorkflowSecurityInfoProvider(new RootSecurityInfoProvider(security).GetChildren().First());
		}
	}
}
