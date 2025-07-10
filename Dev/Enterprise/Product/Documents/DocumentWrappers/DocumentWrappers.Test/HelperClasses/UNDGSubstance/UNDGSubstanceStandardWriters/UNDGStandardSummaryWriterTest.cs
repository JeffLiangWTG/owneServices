using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestsSubclassesOf(typeof(IUNDGStandardSummaryWriter))]
	abstract class UNDGStandardSummaryWriterTest : TestCase
	{
		public abstract IUNDGStandardSummaryWriter GetWriter();

		public abstract IList<IUNDGSummaryWriterComponent> GetExpectedComponents();

		public void TestOrderOfComponents()
		{
			var expectedComponents = GetExpectedComponents();
			var components = GetWriter().Components;
			Assert("Writer does not contain any components", components?.Count > 0);
			AssertEquals("Writer does not contain expected number of components", expectedComponents.Count, components.Count);
			CombineAssertions(() =>
			{
				var componentsList = components.ToList();
				for (var i = 0; i < expectedComponents.Count; i++)
				{
					AssertEquals($"Incorrect component at position {i + 1}", expectedComponents[i].GetType(), componentsList[i].GetType());
				}
			});
		}
	}
}
