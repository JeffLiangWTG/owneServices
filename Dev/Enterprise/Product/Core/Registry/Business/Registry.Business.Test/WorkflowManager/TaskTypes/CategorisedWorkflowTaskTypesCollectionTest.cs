using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CategorisedWorkflowTaskTypesCollection))]
	sealed class CategorisedWorkflowTaskTypesCollectionTest : CategorisedWorkflowRelatedItemsCollectionTestCase<CategorisedWorkflowTaskTypesCollection, CategorisedWorkflowTaskTypes>
	{
		public void TestGetDefaultShouldIncludeSHOWithAPP() => AssertAPPTask(CategorisedWorkflowTaskTypesCollection.GetDefault(), hasAppTask: true);

		public void TestEmptyConstructorStillProvidesAPP() => AssertAPPTask(new CategorisedWorkflowTaskTypesCollection(), hasAppTask: false);
		public void TestConstructorWithTrueStillProvidesAPP() => AssertAPPTask(new CategorisedWorkflowTaskTypesCollection(true), hasAppTask: true);
		public void TestConstructorWithFalseStillProvidesAPP() => AssertAPPTask(new CategorisedWorkflowTaskTypesCollection(false), hasAppTask: false);

		void AssertAPPTask(CategorisedWorkflowTaskTypesCollection collection, bool hasAppTask = false)
		{
			var item = collection.GetTaskTypesFromWorkflowCode("SHO").OfType<WorkflowTaskType>().FirstOrDefault(c => c.Code == "APP");
			if (hasAppTask)
			{
				AssertNotNull(item);
				AssertEquals(item.Description, "Approve");
				AssertEquals(item.IsApprovalTask, true);
			}
			else
			{
				AssertNull(item);
			}
		}

		public void TestCheckingCompletionStatementsDoesNotAddNewItemsToThisCollection()
		{
			var count = Collection.Count;
			Collection.GetCompletionStatementTaskType("!@#");
			AssertEquals(count, Collection.Count);
		}

		public void TestGetTaskTypesFromWorkflowCode()
		{
			var parent1 = Collection.AddNew();
			var parent2 = Collection.AddNew();
			var parent3 = Collection.AddNew();

			parent1.Code = "P1";
			parent2.Code = "P2";
			parent3.Code = "P3";

			var child1A = parent1.TaskTypes.AddNew();
			var child1B = parent1.TaskTypes.AddNew();
			var child1C = parent1.TaskTypes.AddNew();
			child1A.Code = "C1";
			child1B.Code = "C2";
			child1C.Code = "C3";

			var child2A = parent2.TaskTypes.AddNew();
			var child2B = parent2.TaskTypes.AddNew();
			child2A.Code = "C3";
			child2B.Code = "C4";

			var collection1 = Collection.GetTaskTypesFromWorkflowCode("P1");
			AssertEquals(3, collection1.Count);
			AssertEquals("C1", collection1[0].Code);
			AssertEquals("C2", collection1[1].Code);
			AssertEquals("C3", collection1[2].Code);

			var collection2 = Collection.GetTaskTypesFromWorkflowCode("P2");
			AssertEquals(2, collection2.Count);
			AssertEquals("C3", collection2[0].Code);
			AssertEquals("C4", collection2[1].Code);

			var collection3 = Collection.GetTaskTypesFromWorkflowCode("P3");
			AssertEquals(1, collection3.Count);
			AssertEquals("UDF", collection3[0].Code);
			AssertEquals("Undefined - You can modify this in the System Registry, under Workflow Manager/Task Types", collection3[0].Description);

			var collection4 = Collection.GetTaskTypesFromWorkflowCode("XXX");
			AssertEquals(1, collection4.Count);
			AssertEquals("UDF", collection4[0].Code);
			AssertEquals("Undefined - You can modify this in the System Registry, under Workflow Manager/Task Types", collection4[0].Description);
		}

		public void TestGetTaskType()
		{
			var parent1 = Collection.AddNew();
			var parent2 = Collection.AddNew();

			parent1.Code = "P1";
			parent2.Code = "P2";

			var child1A = parent1.TaskTypes.AddNew();
			var child1B = parent1.TaskTypes.AddNew();
			child1A.Code = "C1";
			child1B.Code = "C2";

			var child2A = parent2.TaskTypes.AddNew();
			var child2B = parent2.TaskTypes.AddNew();
			child2A.Code = "C1";
			child2B.Code = "C2";

			AssertEquals(child1A, Collection.GetTaskType("P1", "C1"));
			AssertEquals(child1B, Collection.GetTaskType("P1", "C2"));

			AssertEquals(child2A, Collection.GetTaskType("P2", "C1"));
			AssertEquals(child2B, Collection.GetTaskType("P2", "C2"));
		}

		public void TestCloneIsDeep()
		{
			var dataType = new WorkflowTaskTypesRegistryDataType();
			var original = new CategorisedWorkflowTaskTypesCollection();
			var module = original.AddNew();
			module.Code = "BAR";

			AssertNotNull("Lazy loaded - needs to be touched", module.TaskTypes);

			var clone = (CategorisedWorkflowTaskTypesCollection)original.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);

			Assert("Freshly cloned - Should be equal", dataType.ValuesAreEqual(original, clone));

			var newType = clone[0].TaskTypes.AddNew();
			newType.Code = "FOO";
			newType.Description = (NoResString)"I shouldnt exist for original";

			Assert("Because the cloned object was modified, it must no longer be equal to the original", !dataType.ValuesAreEqual(original, clone));
		}

		public void TestICodeDescriptionPairListMembers()
		{
			CategorisedWorkflowTaskTypesCollection collection = new CategorisedWorkflowTaskTypesCollection();
			CategorisedWorkflowTaskTypes taskTypes1 = collection.AddNew();
			taskTypes1.Code = "001";
			taskTypes1.Description = (NoResString)"Numero Uno";

			CategorisedWorkflowTaskTypes taskTypes2 = collection.AddNew();
			taskTypes2.Code = "002";
			taskTypes2.Description = (NoResString)"Number Two";

			ICodeDescriptionPairList pairList = collection;
			Assert(pairList.ContainsCode("001"));
			AssertEquals("Numero Uno", pairList.GetDescriptionFromCode("001"));

			Assert(pairList.ContainsCode("002"));
			AssertEquals("Number Two", pairList.GetDescriptionFromCode("002"));

			Assert(!pairList.ContainsCode("003"));
			AssertEquals("", pairList.GetDescriptionFromCode("003"));
		}

		protected override CategorisedWorkflowTaskTypesCollection GetCollectionToTest()
		{
			return new CategorisedWorkflowTaskTypesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CategorisedWorkflowTaskTypes();
		}
	}
}
