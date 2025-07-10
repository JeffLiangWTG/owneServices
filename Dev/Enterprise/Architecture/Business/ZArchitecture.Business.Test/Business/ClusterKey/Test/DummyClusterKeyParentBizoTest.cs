using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.ClusterKey.Testing
{
	sealed class DummyClusterKeyParentBizoTest : TestCaseWithFactory
	{
		public void TestSetClusterKey()
		{
			var dummyParent = Factory.New<DummyClusterKeyParentBizo>();
			var dummyChild = dummyParent.AddNewDependentBizo();
			var dummyGrandChild = dummyChild.AddNewDependentBizo();

			Factory.Save();

			CombineAssertions("After saving new dummyParent", () =>
			{
				AssertEquals("Parent ClusterKey", 1, dummyParent.ClusterKeyPty.Value);
				AssertEquals("Child ClusterKey", 1, dummyChild.ClusterKeyPty.Value);
				AssertEquals("GrandChild ClusterKey", 1, dummyGrandChild.ClusterKeyPty.Value);
			});
		}
	}
}
