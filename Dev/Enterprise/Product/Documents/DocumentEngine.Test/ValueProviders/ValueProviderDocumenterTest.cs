using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.ValueProviders.Testing
{
	sealed class ValueProviderDocumenterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			ValueProviderDocumenter documenter = new ValueProviderDocumenter("BIG", (NoResString)"bone", new List<(string example, object expectedResult)> { ("I am an example", null), ("I am an example too.", null) });
			AssertEquals("documenter.Useage", "BIG", documenter.Useage);

			var expectedExplanation = @"bone
E.g: I am an example
E.g: I am an example too.";
			AssertEquals("documenter.Explanation", expectedExplanation, documenter.Explanation);
			AssertArrayEqualsByElements("documenter.Examples", new string[] { "I am an example", "I am an example too." }, documenter.ExamplesAndResults.Select(e => e.example).ToArray());
		}
	}
}
