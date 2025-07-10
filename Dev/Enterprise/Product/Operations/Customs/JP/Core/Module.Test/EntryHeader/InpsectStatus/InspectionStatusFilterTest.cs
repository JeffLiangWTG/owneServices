using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Module.Test
{
	[TestedType(typeof(InspectionStatusFilter))]
	public class InspectionStatusFilterTest : ModuleFilterTestCase<InspectionStatusFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.TextSearch;

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override InspectionStatusFilter GetNewModuleFilter() => new InspectionStatusFilter("moo", (val1, val2) => new ZQuery(), Factory);
	}
}
