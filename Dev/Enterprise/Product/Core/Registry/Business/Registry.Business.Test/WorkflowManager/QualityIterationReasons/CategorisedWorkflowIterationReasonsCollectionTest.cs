using CargoWise.EntityFramework;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CategorisedWorkflowIterationReasonsCollection))]
	sealed class CategorisedWorkflowIterationReasonsCollectionTest : CategorisedWorkflowRelatedItemsCollectionTestCase<CategorisedWorkflowIterationReasonsCollection, CategorisedWorkflowIterationReasons>
	{
		public void TestGetIterationReasonsFromWorkflowCode()
		{
			var parent1 = Collection.AddNew();
			var parent2 = Collection.AddNew();
			var parent3 = Collection.AddNew();

			parent1.Code = "P1";
			parent2.Code = "P2";
			parent3.Code = "P3";

			var child1A = parent1.IterationReasons.AddNew();
			var child1B = parent1.IterationReasons.AddNew();
			var child1C = parent1.IterationReasons.AddNew();
			child1A.Code = "C1";
			child1B.Code = "C2";
			child1C.Code = "C3";

			var child2A = parent2.IterationReasons.AddNew();
			var child2B = parent2.IterationReasons.AddNew();
			child2A.Code = "C3";
			child2A.Description = (NoResString)"Test Description";
			child2B.Code = "C4";

			var collection1 = Collection.GetIterationReasonsFromWorkflowCode("P1");
			AssertEquals(3, collection1.Count);
			AssertEquals("C1", collection1[0].Code);
			AssertEquals("C2", collection1[1].Code);
			AssertEquals("C3", collection1[2].Code);

			var collection2 = Collection.GetIterationReasonsFromWorkflowCode("P2");
			AssertEquals(2, collection2.Count);
			AssertEquals("C3", collection2[0].Code);
			AssertEquals("C4", collection2[1].Code);

			var iterationReason1 = Collection.GetIterationReason("P2", "C3");
			AssertEquals("C3", iterationReason1.Code);
			AssertEquals("Test Description", iterationReason1.Description);
			var iterationReason2 = Collection.GetIterationReason("P2", "C4");
			AssertEquals("C4", iterationReason2.Code);
			AssertEquals("", iterationReason2.Description);
			var iterationReason3 = Collection.GetIterationReason("P1", "C4");
			AssertEquals(null, iterationReason3);

			var collection3 = Collection.GetIterationReasonsFromWorkflowCode("P3");
			AssertEquals(1, collection3.Count);
			AssertEquals("UDF", collection3[0].Code);
			AssertEquals("Undefined - You can modify this in the System Registry, under Workflow Manager/Quality Iteration Reasons", collection3[0].Description);

			var collection4 = Collection.GetIterationReasonsFromWorkflowCode("XXX");
			AssertEquals("The collection should add reasons for non serialised code (if it is present in Workflows Descriptors)", 1, collection4.Count);
			AssertEquals("UDF", collection4[0].Code);
			AssertEquals("Undefined - You can modify this in the System Registry, under Workflow Manager/Quality Iteration Reasons", collection4[0].Description);
		}

		public void TestGetIterationReasonValidationFromWorkflowCode()
		{
			var parent1 = Collection.AddNew();
			var parent2 = Collection.AddNew();
			var parent3 = Collection.AddNew();

			parent1.Code = "P1";
			parent2.Code = "P2";
			parent3.Code = "P3";

			parent1.IterationReasonValidation = IterationReasonValidationList.Codes.None;
			parent2.IterationReasonValidation = IterationReasonValidationList.Codes.Warning;
			parent3.IterationReasonValidation = IterationReasonValidationList.Codes.Error;

			AssertEquals(IterationReasonValidationList.Codes.None, Collection.GetIterationReasonValidationFromWorkflowCode("P1"));
			AssertEquals(IterationReasonValidationList.Codes.Warning, Collection.GetIterationReasonValidationFromWorkflowCode("P2"));
			AssertEquals(IterationReasonValidationList.Codes.Error, Collection.GetIterationReasonValidationFromWorkflowCode("P3"));
			AssertEquals(IterationReasonValidationList.Codes.Error, Collection.GetIterationReasonValidationFromWorkflowCode("XXX"));
		}

		public void TestGetIterationReason()
		{
			var parent1 = Collection.AddNew();
			var parent2 = Collection.AddNew();

			parent1.Code = "P1";
			parent2.Code = "P2";

			var child1A = parent1.IterationReasons.AddNew();
			var child1B = parent1.IterationReasons.AddNew();
			child1A.Code = "C1";
			child1B.Code = "C2";

			var child2A = parent2.IterationReasons.AddNew();
			var child2B = parent2.IterationReasons.AddNew();
			child2A.Code = "C1";
			child2B.Code = "C2";

			AssertEquals(child1A, Collection.GetIterationReason("P1", "C1"));
			AssertEquals(child1B, Collection.GetIterationReason("P1", "C2"));

			AssertEquals(child2A, Collection.GetIterationReason("P2", "C1"));
			AssertEquals(child2B, Collection.GetIterationReason("P2", "C2"));
		}

		protected override CategorisedWorkflowIterationReasonsCollection GetCollectionToTest()
		{
			return new CategorisedWorkflowIterationReasonsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CategorisedWorkflowIterationReasons();
		}
	}
}
