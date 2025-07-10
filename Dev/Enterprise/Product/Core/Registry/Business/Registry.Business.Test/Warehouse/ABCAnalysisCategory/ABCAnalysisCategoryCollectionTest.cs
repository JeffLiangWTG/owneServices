using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ABCAnalysisCategoryCollection))]
	sealed class ABCAnalysisCategoryCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ABCAnalysisCategoryCollection>
	{
		#region TestOperatorOnItemsInCollection

		public void TestOperatorOnItemsInCollection()
		{
			var collection = new ABCAnalysisCategoryCollection();
			var firstCategory = collection.AddNew();
			AssertEquals("firstCategory.Operator", "<", firstCategory.Operator);

			var secondCategory = collection.AddNew();
			AssertEquals("firstCategory.Operator", ">=", firstCategory.Operator);
			AssertEquals("secondCategory.Operator", "<", secondCategory.Operator);

			var thirdCategory = collection.AddNew();
			AssertEquals("firstCategory.Operator", ">=", firstCategory.Operator);
			AssertEquals("secondCategory.Operator", ">=", secondCategory.Operator);
			AssertEquals("thirdCategory.Operator", "<", thirdCategory.Operator);

			collection.RemoveAndDelete(thirdCategory);
			AssertEquals("firstCategory.Operator", ">=", firstCategory.Operator);
			AssertEquals("secondCategory.Operator", "<", secondCategory.Operator);
		}

		#endregion

		#region TestDefault

		public void TestDefault()
		{
			var defaultValue = ABCAnalysisCategoryCollection.Default;
			AssertEquals(4, defaultValue.Count);

			var category1 = defaultValue[0];
			AssertEquals("A", category1.CategoryName);
			AssertEquals(">=", category1.Operator);
			AssertEquals(80, category1.PercentageOfTotal);

			var category2 = defaultValue[1];
			AssertEquals("B", category2.CategoryName);
			AssertEquals(">=", category2.Operator);
			AssertEquals(15, category2.PercentageOfTotal);

			var category3 = defaultValue[2];
			AssertEquals("C", category3.CategoryName);
			AssertEquals(">=", category3.Operator);
			AssertEquals(5, category3.PercentageOfTotal);

			var category4 = defaultValue[3];
			AssertEquals("D", category4.CategoryName);
			AssertEquals("<", category4.Operator);
			AssertEquals(5, category4.PercentageOfTotal);
		}

		#endregion

		#region TestAllowNew

		public void TestAllowNew()
		{
			AssertEquals("Must allow new rows", true, Collection.AllowNew);
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ABCAnalysisCategoryCollection GetCollectionToTest()
		{
			return new ABCAnalysisCategoryCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ABCAnalysisCategory();
		}

		#endregion
	}
}
