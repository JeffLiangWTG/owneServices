using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolCollection))]
	sealed class CodeDescriptionBoolCollectionTestCase : CodeDescriptionBoolCollectionAbstractTest<CodeDescriptionBoolCollection>
	{
		protected override CodeDescriptionBoolCollection GetCollectionToTest()
		{
			return new CodeDescriptionBoolCollection();
		}

		public void TestSet()
		{
			var collection = new CodeDescriptionBoolCollection();
			collection.Add("MMM", (NoResString)"Chicken Sandvich", false);
			collection.Set("MMM", true);
			AssertEquals(true, collection.GetBoolFromCode("MMM"));
		}
	}
}
