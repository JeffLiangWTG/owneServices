using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class FilterFieldDocumenterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			IReportDocumenter documenter = new FilterBuilderDocumenter("BIG", "bone", new List<string>(), true);
			AssertEquals("documenter.Useage", "BIG", documenter.Useage);
			AssertEquals("documenter.Explanation", "bone", documenter.Explanation);
			AssertEquals("documenter.SupportedProperties", 0, documenter.SupportedProperties.Count);
			Assert("documenter.SupportLookup", documenter.SupportLookup);
		}

		public void TestExplanation()
		{
			IReportDocumenter documenter = new FilterBuilderDocumenter("BIG", "bone", new List<string>(), true);
			AssertEquals("documenter.Explanation", "bone", documenter.Explanation);

			documenter.DefaultOptions = "default";
			AssertEquals("documenter.Explanation", "bone" + System.Environment.NewLine + "default", documenter.Explanation);
		}
	}
}
