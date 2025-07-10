using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	sealed class DocumentFilterEvaluatorTest : TestCaseWithFactory
	{
		public void TestIsApplicable()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			bizObj.Z0_Code = "AAA";

			var filterEvaluator = new DocumentFilterEvaluator();
			AssertEquals("matching filter", true, filterEvaluator.IsApplicable(bizObj, "\"<Z0_Code>\" == \"AAA\""));
			AssertEquals("non matching filter", false, filterEvaluator.IsApplicable(bizObj, "\"<Z0_Code>\" == \"BBB\""));
			AssertEquals("null parent", false, filterEvaluator.IsApplicable(null, "\"<Z0_Code>\" == \"BBB\""));
			AssertEquals("null filter", true, filterEvaluator.IsApplicable(bizObj, null));
			AssertEquals("null parent and null filter", true, filterEvaluator.IsApplicable(null, null));
		}
	}
}
