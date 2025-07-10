using System;
using System.Linq;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class AcceptabilityBandMatchingWorkflowsCalculatorTest : BMSTestCaseWithFactory
	{
		public void TestGetMatchingWorkflows_ForCountAcceptabilityBand()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var acceptabilityBand = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 1, 2, 3, 4, 5, 6, "Thingy-Ma-Jig", type: AcceptabilityBandTypes.Codes.Count);

			FilterStripsTestHelper.AddFilterStrips(acceptabilityBand.SupersetItemsFilterRule, new FilterStripsTestHelper.FilterStripDefinition {
				FilterStripName = "Current Component",
				FilterStripValueSetter = f => ((ModuleGuidFilter)f).Property = config.Buffer.PK,
				ComparisonOperatorSetter = f => ((ModuleGuidFilter)f).ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact,
			});

			Factory.Save();

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflowsInBuffer = Enumerable.Range(1, 20)
				.Select(n => CreateWorkflow(jobHeader, $"workflow{n}", currentComponent: config.Buffer))
				.ToList();

			var workflowsInBucket = Enumerable.Range(21, 40)
				.Select(n => CreateWorkflow(jobHeader, $"workflow{n}", currentComponent: config.Bucket))
				.ToList();

			Factory.Save();

			var sqlParameters = new AcceptabilityBandSqlBuilderParameters(acceptabilityBand);

			var workflowsInBufferPks = workflowsInBuffer.Select(w => w.PK.ToGuid()).ToArray();
			var workflowsInBucketPks = workflowsInBucket.Select(w => w.PK.ToGuid()).ToHashSet();

			var matchingWorkflows = GetMatchingWorkflowPks(acceptabilityBand, sqlParameters);

			AssertContainsExactElementsInAnyOrder(workflowsInBufferPks, matchingWorkflows); 
			AssertCollectionNotContains(matchingWorkflows, w => workflowsInBucketPks.Contains(w));
		}

		public void TestGetMatchingWorkflows_ThrowsError_ForSQLAcceptabilityBand()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var acceptabilityBand = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(config.Buffer, 1, 2, 3, 4, 5, 6); 

			Factory.Save();

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflowsInBuffer = Enumerable.Range(1, 20)
				.Select(n => CreateWorkflow(jobHeader, $"workflow{n}", currentComponent: config.Buffer))
				.ToList();

			var workflowsInBucket = Enumerable.Range(21, 40)
				.Select(n => CreateWorkflow(jobHeader, $"workflow{n}", currentComponent: config.Bucket))
				.ToList();

			Factory.Save();

			var sqlParameters = new AcceptabilityBandSqlBuilderParameters(acceptabilityBand);

			AssertExceptionThrown(typeof(ArgumentException), () => GetMatchingWorkflowPks(acceptabilityBand, sqlParameters));
		}

		public Guid[] GetMatchingWorkflowPks(BMComponentAcceptabilityBand band, AcceptabilityBandSqlBuilderParameters parameters, AcceptabilityBandDataProvider provider = null)
		{
			if (provider == null)
			{
				provider = new AcceptabilityBandDataProvider(SecondaryServerConnectionProviderProvider.GetProvider());
			}

			return AcceptabilityBandMatchingWorkflowsCalculator.GetMatchingWorkflows(band, provider, parameters).MatchingWorkflows.ToArray();
		}
	}
}
