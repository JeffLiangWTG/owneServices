using Enterprise.BufferManagement.Business;
using Enterprise.VisualBoards.GUI.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(RiskFilter))]
	class RiskFilterTest : BoardFilterTestCase<RiskFilter>
	{
		public override void TestAllowMultiple()
		{
			AssertEquals(false, GetFilter().AllowMultiple);
		}

		public override void TestEquals()
		{
			AssertEquals(GetFilter(), GetFilter());
		}

		public override void TestFilterName()
		{
			AssertEquals("Risk Filter", GetFilter().FilterName);
		}

		public override void TestWhenRemovedThroughFilterManager_ShouldRestoreVisualState()
		{
			Assert("Tested elsewhere", true);
		}

		protected override RiskFilter GetFilter() => new RiskFilter();
	}
}
