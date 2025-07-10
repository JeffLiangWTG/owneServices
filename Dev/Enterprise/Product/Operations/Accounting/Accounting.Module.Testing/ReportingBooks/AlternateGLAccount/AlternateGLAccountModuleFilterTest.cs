using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AlternateGLAccountModuleFilter))]
	public class AlternateGLAccountModuleFilterTest : ModuleFilterTestCase<AlternateGLAccountModuleFilter>
	{
		protected override AlternateGLAccountModuleFilter GetNewModuleFilter()
		{
			return new AlternateGLAccountModuleFilter("Percent Number", ModuleIDs.AlternateGLAccounts, AccAlternateGLAccountSchema.AGA_AGA_PercentNum, new AccAlternateGLAccountCollection(Factory));
		}

		protected override ZString ExpectedDescription => "Percent Number";

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.NumbersAndReferences;

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}
		public override void TestQueryIsEmptyByDefault()
		{
			AssertEquals("AlternateGLAccountModuleFilter.Query is empty by default", true, Filter.IsEmpty);
		}
	}
}
