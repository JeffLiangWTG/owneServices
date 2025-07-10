using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Module.Test
{
	[TestedType(typeof(GuidTextFilter))]
	public class GuidTextFilterTest : ModuleFilterTestCase<GuidTextFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory
		{
			get
			{
				return FilterCategories.TextSearch;
			}
		}

		protected override GuidTextFilter GetNewModuleFilter()
		{
			return new GuidTextFilter("moo", DummyBizoSchema.Z0_Guid);
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}
	}
}
