using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ForEventKeyTest : TestCase
	{
		public void TestEquality()
		{
			ForEventKey key1 = new ForEventKey("ABC", "onload");
			ForEventKey key2 = new ForEventKey("ABC", "onload");
			AssertEquals("Should be equal", key1, key2);
			AssertEquals("HashCodes should be equal", key1.GetHashCode(), key2.GetHashCode());
		}
	}
}
