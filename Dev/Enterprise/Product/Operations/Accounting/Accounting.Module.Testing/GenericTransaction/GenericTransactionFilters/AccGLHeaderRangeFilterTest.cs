using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccGLHeaderRangeFilter))]
	public class AccGLHeaderRangeFilterTest : ModuleFilterTestCase<AccGLHeaderRangeFilter>
	{
		protected override AccGLHeaderRangeFilter GetNewModuleFilter()
		{
			AccGLHeaderCollection list = new AccGLHeaderCollection(Factory);
			return new AccGLHeaderRangeFilter("moo", AccGLHeaderSchema.AG_AccountNum, list, AccGLHeaderSchema.AG_AccountNum, list);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.NumbersAndReferences; }
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}
	}
}
