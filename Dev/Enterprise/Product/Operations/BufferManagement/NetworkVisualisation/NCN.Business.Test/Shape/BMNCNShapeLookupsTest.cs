using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class BMNCNShapeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestJobTypesLookupShouldBeEmptyIfThereIsNoBMSystem()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var diagram = NetworkTestCase.CreateDiagram(jobHeader);
			AssertEquals("Lookups should contain zero job types before first BM system creation", 0, diagram.Lookups.JobTypes.Count);
		}

		public void TestJobTypesLookupShouldContainAllSupportedJobTypesEnabledForBM()
		{
			var workflowCodesThatSupportPAVE = new List<string>();

			foreach (var workflowDescriptor in WorkflowDescriptors.Instance.Values)
			{
				if (workflowDescriptor.SupportsBufferManagement)
				{
					BMSTestHelper.CreateSystem(Factory, workflowDescriptor.Code);
					workflowCodesThatSupportPAVE.Add(workflowDescriptor.Code);

					var diagram = NetworkTestCase.CreateDiagram(Factory);

					Factory.ClearCachedValue<CodeDescriptionPairList>(BMNCNShapeLookups.JobTypesCacheKey);
					AssertContainsExactElementsInAnyOrder("The lookup should contain all job types that support PAVE", workflowCodesThatSupportPAVE, diagram.Lookups.JobTypes.GetAllCodes());
				}
			}
		}

		public void TestJobTypesLookupIsSharedBetweenObjectsInTheSameFactory()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var diagram1 = NetworkTestCase.CreateDiagram(Factory, "Diagram 1");
			var diagram2 = NetworkTestCase.CreateDiagram(Factory, "Diagram 2");
			Factory.Save();

			var jobTypes1 = diagram1.Lookups.JobTypes;
			var jobTypes2 = diagram2.Lookups.JobTypes;
			Assert("JobTypes should share BMCNCShapeLookup instances that belong to the same factory (via the factory cache)", ReferenceEquals(jobTypes1, jobTypes2)); // This is important for perf as the JobTypes property is frequently accessed by validation routines, often across many objects belonging to the same factory.
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}
	}
}
