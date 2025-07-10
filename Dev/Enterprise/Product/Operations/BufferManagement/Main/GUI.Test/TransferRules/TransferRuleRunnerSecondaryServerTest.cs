using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.BufferManagement.GUI.Test
{
	class TransferRuleRunnerSecondaryServerTest : NonTransactionedTestCase
	{
		public void TestTransferringWorkflows_WhenRegistryItemEnabled_ShouldUseSecondaryServerForExecutingComplexQuery_AndPrimaryServerForCommittingTransfers()
		{
			BMSRegistry.Instance.UseSecondaryServerForTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			RunTransferRulesAndAssertServerUsed(expectSecondaryServerUsedForTransferRuleQuery: true);
		}

		public void TestTransferringWorkflows_WhenRegistryItemDisabled_ShouldUsePrimaryServerForEverything()
		{
			AssertEquals("The registry item should be disabled by default. SAD!", false, BMSRegistry.Instance.UseSecondaryServerForTransferRules.Value);

			RunTransferRulesAndAssertServerUsed(expectSecondaryServerUsedForTransferRuleQuery: false);
		}

		void RunTransferRulesAndAssertServerUsed(bool expectSecondaryServerUsedForTransferRuleQuery)
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			config.ComponentLink.FL_IsReleaseGateRuleApplied = false;

			const string componentLinkPredicate = "69=69";
			FilterStripsTestHelper.AddCustomSQLFilterStrip(config.ComponentLink.FilterRule, componentLinkPredicate);

			BMSTestHelper.CreateWorkflows(config.Bucket, numberOfWorkflows: 10, numberOfTasksPerWorkflow: 1);

			Factory.Save();

			var connectionForComplexQueries = Db.NewExtraConnectionToMainDbWithReaderCredentials();

			BMSTestHelper.AssertConnectionCannotBeUsedForWrites(connectionForComplexQueries, "Complex component link queries are run on the secondary server, so it shouldn't be used for writes");

			using (DummySecondaryServerConnectionProvider.TemporarilyEnableDummyProvider(connectionForComplexQueries))
			using (TestConnection.TrackExecutedCommands())
			using (connectionForComplexQueries.TrackExecutedCommands())
			{
				BMSTestCaseWithFactory.RunTransferRules(config.System);

				AssertCollectionContains("Updates always need to be done on the main connection", TestConnection.ExecutedCommands, s => s.Contains("UPDATE dbo.ProcessHeader"));

				if (expectSecondaryServerUsedForTransferRuleQuery)
				{
					AssertCollectionNotContains("The main connection should not be used for component link filters because they can be very complex. This can be executed on the secondary server, and then perform edits on the primary.", TestConnection.ExecutedCommands, s => s.Contains(componentLinkPredicate));
					AssertCollectionContains("The secondary server connection should be used for component link filters because they can be very complex.", connectionForComplexQueries.ExecutedCommands, s => s.Contains(componentLinkPredicate));
				}
				else
				{
					AssertCollectionContains("The main connection should be used for component link filters because the registry item controlling when the secondary server is used has been overridden.", TestConnection.ExecutedCommands, s => s.Contains(componentLinkPredicate));
					AssertContainsExactElementsInAnyOrder("Secondary server shouldn't be used for anything.", Array.Empty<string>(), connectionForComplexQueries.ExecutedCommands);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();
		}
	}
}
