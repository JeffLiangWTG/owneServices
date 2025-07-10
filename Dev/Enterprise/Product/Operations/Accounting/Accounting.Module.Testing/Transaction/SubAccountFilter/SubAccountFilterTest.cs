using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(SubAccountFilter))]
	public class SubAccountFilterTest : ModuleFilterTestCase<SubAccountFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Organisations; }
		}

		protected override SubAccountFilter GetNewModuleFilter()
		{
			return new SubAccountFilter("moo");
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override FilterCategory InitialTestCatergory
		{
			get { return FilterCategories.TextSearch; }
		}
	}
}
