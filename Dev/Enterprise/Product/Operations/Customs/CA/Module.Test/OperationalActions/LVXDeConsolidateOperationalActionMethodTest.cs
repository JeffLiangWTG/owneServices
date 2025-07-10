using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.OperationalActions.Testing
{
	[TestedType(typeof(LVXDeConsolidationOperationalActionMethod))]
	sealed class LVXDeConsolidateOperationalActionMethodTest : OperationalActionMethodTest<LVXDeConsolidationOperationalActionMethod>
	{
		public void TestProperties()
		{
			var method = new LVXDeConsolidationOperationalActionMethod();
			CombineAssertions(() =>
			{
				AssertEquals("TestMethodName", "De-Consolidate", method.Name);
				AssertEquals("TestMethodDescription", "De-Consolidate Courier LVS Declaration Jobs", method.Description);
				AssertEquals("TestHasControl", false, method.HasControl);
				AssertEquals("TestHasSettings", false, method.HasSettings);
			});
		}

		protected override LVXDeConsolidationOperationalActionMethod NewMethod() => new LVXDeConsolidationOperationalActionMethod();
	}
}
