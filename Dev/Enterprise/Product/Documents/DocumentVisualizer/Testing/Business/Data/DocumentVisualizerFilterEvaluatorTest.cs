using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;

namespace Enterprise.DocumentVisualizer.Testing.Business
{
	sealed class DocumentVisualizerFilterEvaluatorTest : TestCaseWithFactory
	{
		public void TestIsApplicable()
		{
			var dummy = Factory.New<DummyWithUXmlSupport>();
			dummy.Z0_Code = "AAA";

			var unsupportedDummy = Factory.New<DummyBusinessObject>();
			unsupportedDummy.Z0_Code = "AAA";

			var filterEvaluator = new DocumentVisualizerFilterEvaluator();

			AssertEquals("matching filter (for supported bizObj by Document Visualizer)", true, filterEvaluator.IsApplicable(dummy, "Z0_Code == \"AAA\""));
			AssertEquals("matching filter (for unsupported bizObj by Document Visualizer)", false, filterEvaluator.IsApplicable(unsupportedDummy, "Z0_Code == \"AAA\""));

			AssertEquals("non matching filter (for supported bizObj by Document Visualizer)", false, filterEvaluator.IsApplicable(dummy, "Z0_Code == \"BBB\""));
			AssertEquals("non matching filter (for unsupported bizObj by Document Visualizer)", false, filterEvaluator.IsApplicable(unsupportedDummy, "Z0_Code == \"BBB\""));

			AssertEquals("null bizObj", false, filterEvaluator.IsApplicable(null, "Z0_Code == \"AAA\""));
			AssertEquals("null filter", true, filterEvaluator.IsApplicable(dummy, null));
			AssertEquals("null bizObj and null filter", false, filterEvaluator.IsApplicable(null, null));
		}
	}
}
