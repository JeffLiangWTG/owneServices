using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.OperationalActions.Testing
{
	[TestedType(typeof(LVXConsolidationOperationalActionMethod))]
	sealed class LVXConsolidateOperationalActionMethodTest : OperationalActionMethodTest<LVXConsolidationOperationalActionMethod>
	{
		public void TestProperties()
		{
			var method = new LVXConsolidationOperationalActionMethod();
			CombineAssertions(() =>
			{
				AssertEquals("TestMethodName", "Consolidate", method.Name);
				AssertEquals("TestMethodDescription", "Courier LVS Declaration Jobs", method.Description);
				AssertEquals("TestHasControl", false, method.HasControl);
				AssertEquals("TestHasSettings", false, method.HasSettings);
			});
		}

		protected override LVXConsolidationOperationalActionMethod NewMethod() => new LVXConsolidationOperationalActionMethod();
	}
}
