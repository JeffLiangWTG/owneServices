using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(HideComparisonOperatorModuleTextFilter))]
	sealed class HideComparisonOperatorModuleTextFilterTest : ModuleTextFilterTest
	{
		public void TestHasComparisonOperatorIsFalse()
		{
			var filter = new HideComparisonOperatorModuleTextFilter("moo", DummyBizoSchema.Z0_NVarChar);
			Assert(!filter.HasComparisonOperator);
		}

		protected override ModuleTextFilter GetNewModuleFilter() => new HideComparisonOperatorModuleTextFilter("moo", DummyBizoSchema.Z0_NVarChar);
	}
}
