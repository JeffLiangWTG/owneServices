using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Testing
{
	[TestedType(typeof(CategorisedWorkflowCategoriesCollection))]
	public class CategorisedWorkflowCategoriesCollectionTest : CategorisedWorkflowRelatedItemsCollectionTestCase<CategorisedWorkflowCategoriesCollection, CategorisedWorkflowCategories>
	{
		public void TestGetCategoriesFromWorkflowCode()
		{
			var parent1 = Collection.AddNew();
			var parent2 = Collection.AddNew();
			var parent3 = Collection.AddNew();

			parent1.Code = "P1";
			parent2.Code = "P2";
			parent3.Code = "P3";

			var child1A = parent1.Categories.AddNew();
			var child1B = parent1.Categories.AddNew();
			var child1C = parent1.Categories.AddNew();
			child1A.Code = "C1";
			child1B.Code = "C2";
			child1C.Code = "C3";

			var child2A = parent2.Categories.AddNew();
			var child2B = parent2.Categories.AddNew();
			child2A.Code = "C3";
			child2A.Description = (NoResString)"Test Description";
			child2B.Code = "C4";

			var collection1 = Collection.GetCategoriesFromWorkflowCode("P1");
			AssertEquals(3, collection1.Count);
			AssertEquals("C1", collection1[0].Code);
			AssertEquals("C2", collection1[1].Code);
			AssertEquals("C3", collection1[2].Code);

			var collection2 = Collection.GetCategoriesFromWorkflowCode("P2");
			AssertEquals(2, collection2.Count);
			AssertEquals("C3", collection2[0].Code);
			AssertEquals("C4", collection2[1].Code);

			var category1 = Collection.GetCategory("P2", "C3");
			AssertEquals("C3", category1.Code);
			AssertEquals("Test Description", category1.Description);
			var category2 = Collection.GetCategory("P2", "C4");
			AssertEquals("C4", category2.Code);
			AssertEquals("", category2.Description);
			var category3 = Collection.GetCategory("P1", "C4");
			AssertEquals(null, category3);

			var collection3 = Collection.GetCategoriesFromWorkflowCode("P3");
			AssertEquals(1, collection3.Count);
			AssertEquals("UDF", collection3[0].Code);
			AssertEquals("Undefined - You can modify this in the System Registry, under Workflow Manager/Buffer Management/Workflow Categories", collection3[0].Description);

			var collection4 = Collection.GetCategoriesFromWorkflowCode("XXX");
			AssertEquals("The collection should add categories for non serialised code (if it is present in Workflows Descriptors)", 1, collection4.Count);
			AssertEquals("UDF", collection4[0].Code);
			AssertEquals("Undefined - You can modify this in the System Registry, under Workflow Manager/Buffer Management/Workflow Categories", collection4[0].Description);
		}

		public void TestGetCategory()
		{
			var parent1 = Collection.AddNew();
			var parent2 = Collection.AddNew();

			parent1.Code = "P1";
			parent2.Code = "P2";

			var child1A = parent1.Categories.AddNew();
			var child1B = parent1.Categories.AddNew();
			child1A.Code = "C1";
			child1B.Code = "C2";

			var child2A = parent2.Categories.AddNew();
			var child2B = parent2.Categories.AddNew();
			child2A.Code = "C1";
			child2B.Code = "C2";

			AssertEquals(child1A, Collection.GetCategory("P1", "C1"));
			AssertEquals(child1B, Collection.GetCategory("P1", "C2"));

			AssertEquals(child2A, Collection.GetCategory("P2", "C1"));
			AssertEquals(child2B, Collection.GetCategory("P2", "C2"));
		}

		protected override CategorisedWorkflowCategoriesCollection GetCollectionToTest() => new CategorisedWorkflowCategoriesCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CategorisedWorkflowCategories();
	}
}
