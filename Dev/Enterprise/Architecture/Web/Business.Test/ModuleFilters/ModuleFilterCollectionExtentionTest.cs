using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class ModuleFilterCollectionExtentionTest : TestCase
	{
		public void TestGetUniqueDescription()
		{
			var filters = new ModuleFilterCollection();
			AssertEquals("My Filter", filters.GetUniqueDescription("My Filter"));

			filters.AddTextFilter("Some Filter", Schema.TestAddInfoSchema.UZ_String);
			AssertEquals("My Filter", filters.GetUniqueDescription("My Filter"));

			filters.AddTextFilter("My Filter", Schema.TestAddInfoSchema.UZ_String);
			AssertEquals("My Filter (Web)", filters.GetUniqueDescription("My Filter"));
		}
	}
}
