using Enterprise.Registry.Business.Web;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class AccessRulesTest : TestCase
	{
		public void TestGetDefaultCollection()
		{
			OrgsRoleAccessCollection collection = new DummyAccessRules().GetDefaultCollection();

			AssertEquals(3, collection.Count);
			foreach (OrgsRoleAccess access in collection)
			{
				AssertEquals(3, access.LastUsedPropertyNum);
			}

			AssertEquals("role1", collection[0].Role);
			Assert(collection[0].Property1);
			Assert(collection[0].Property2);
			Assert(!collection[0].Property3);

			AssertEquals("role2", collection[1].Role);
			Assert(!collection[1].Property1);
			Assert(!collection[1].Property2);
			Assert(collection[1].Property3);

			AssertEquals("role3", collection[2].Role);
			Assert(!collection[2].Property1);
			Assert(!collection[2].Property2);
			Assert(!collection[2].Property3);
		}
	}
}
