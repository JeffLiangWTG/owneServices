using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.OperationalActions.Testing
{
	[TestedType(typeof(MergeEntriesOperationalActionMethod))]
	sealed class MergeEntriesOperationalActionMethodTest : OperationalActionMethodTest<MergeEntriesOperationalActionMethod>
	{
		public void TestProperties()
		{
			var method = new MergeEntriesOperationalActionMethod();
			CombineAssertions(() =>
			{
				AssertEquals("TestMethodName", "Merge Entries", method.Name);
				AssertEquals("TestMethodDescription", "Merge Entries for Declarations", method.Description);
				AssertEquals("TestHasControl", false, method.HasControl);
				AssertEquals("TestHasSettings", false, method.HasSettings);
			});
		}

		protected override MergeEntriesOperationalActionMethod NewMethod() => new MergeEntriesOperationalActionMethod();
	}
}
