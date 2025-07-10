using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ClearExtraSubscriptionListenerTest : TestCaseWithDummy
	{
		public void TestClearExtraSubscription()
		{
			var pK = Dummy.PK;
			Dummy.Z0_Code = "abc";
			Factory.Save();

			var listener = new ClearExtraSubscriptionListener();
			listener.StartTest(this, ZDateTime.Now.ToDateTime());

			var factory2 = new BusinessObjectFactory();
			var dummy2 = factory2.Load(typeof(DummyBusinessObject), pK) as DummyBusinessObject;
			AssertEquals("dummy2.Z0_Code", "abc", dummy2.Z0_Code.ToString().Trim());

			Dummy.Z0_Code = "zzz";
			Factory.Save();
			AssertEquals("dummy2.Z0_Code", "zzz", dummy2.Z0_Code.ToString().Trim());

			listener.EndTest(this, ZDateTime.Now.ToDateTime());

			Dummy.Z0_Code = "hello";
			Factory.Save();
			AssertEquals("dummy2.Z0_Code", "zzz", dummy2.Z0_Code.ToString().Trim());
		}
	}
}
