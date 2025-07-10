using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Filters.Testing
{
	[TestedType(typeof(DependentListFilter))]
	public class DependentListFilterTest : ModuleFilterTestCase<DependentListFilter>
	{
		protected override DependentListFilter GetNewModuleFilter()
		{
			return new DependentListFilter("moo", AccGLHeaderSchema.AG_AccountNum, AccGLHeaderSchema.AG_AccountNum, new CodeDescriptionPairList(), new CodeDescriptionPairList());
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}
	}
}
