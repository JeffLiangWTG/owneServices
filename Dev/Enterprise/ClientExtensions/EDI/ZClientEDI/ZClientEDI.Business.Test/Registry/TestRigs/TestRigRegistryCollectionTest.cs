using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(TestRigRegistryCollection))]
	public class TestRigRegistryCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<TestRigRegistryCollection>
	{
		public void TestGetOptionsForWorkItemCombination()
		{
			var options1 = new TestRigRegistryOptions(NewFallbackLevel(), Factory) { Product = "AAA", ProductArea = "AAA", Module = "AAA", ChangeType = "AAA" };
			var options2 = new TestRigRegistryOptions(NewFallbackLevel(), Factory) { Product = "AAA", ProductArea = "AAA", Module = "AAA", ChangeType = "BBB" };
			var options3 = new TestRigRegistryOptions(NewFallbackLevel(), Factory) { Product = "AAA", ProductArea = "AAA", Module = "BBB", ChangeType = "BBB" };
			var options4 = new TestRigRegistryOptions(NewFallbackLevel(), Factory) { Product = "AAA", ProductArea = "BBB", Module = "AAA", ChangeType = "AAA" };
			var options5 = new TestRigRegistryOptions(NewFallbackLevel(), Factory) { Product = "AAA", ProductArea = "BBB", Module = "AAA", ChangeType = "BBB" };
			var options6 = new TestRigRegistryOptions(NewFallbackLevel(), Factory) { Product = "AAA", ProductArea = "BBB", Module = "BBB", ChangeType = "BBB" };
			var options7 = new TestRigRegistryOptions(NewFallbackLevel(), Factory) { Product = "BBB", ProductArea = "BBB", Module = "BBB", ChangeType = "AAA" };
			var options8 = new TestRigRegistryOptions(NewFallbackLevel(), Factory) { Product = "BBB", ProductArea = "BBB", Module = "AAA", ChangeType = "BBB" };

			var collection = new TestRigRegistryCollection(NewFallbackLevel(), Factory) { options1, options2, options3, options4, options5, options6, options7, options8 };

			AssertEquals(options1, collection.GetOptionsForWorkItemCombination("AAA", "AAA", "AAA", "AAA").SingleOrDefault());
			AssertEquals(options2, collection.GetOptionsForWorkItemCombination("AAA", "AAA", "AAA", "BBB").SingleOrDefault());
			AssertEquals(options3, collection.GetOptionsForWorkItemCombination("AAA", "AAA", "BBB", "BBB").SingleOrDefault());
			AssertEquals(options4, collection.GetOptionsForWorkItemCombination("AAA", "BBB", "AAA", "AAA").SingleOrDefault());
			AssertEquals(options5, collection.GetOptionsForWorkItemCombination("AAA", "BBB", "AAA", "BBB").SingleOrDefault());
			AssertEquals(options6, collection.GetOptionsForWorkItemCombination("AAA", "BBB", "BBB", "BBB").SingleOrDefault());
			AssertEquals(options7, collection.GetOptionsForWorkItemCombination("BBB", "BBB", "BBB", "AAA").SingleOrDefault());
			AssertEquals(options8, collection.GetOptionsForWorkItemCombination("BBB", "BBB", "AAA", "BBB").SingleOrDefault());
		}

		#region Implementation

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override TestRigRegistryCollection GetCollectionToTest()
		{
			return new TestRigRegistryCollection(NewFallbackLevel(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TestRigRegistryOptions(NewFallbackLevel(), Factory);
		}

		#endregion
	}
}
