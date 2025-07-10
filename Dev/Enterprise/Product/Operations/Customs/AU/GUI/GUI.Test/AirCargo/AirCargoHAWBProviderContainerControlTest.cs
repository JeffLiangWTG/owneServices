using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class AirCargoHAWBProviderContainerControlTest : TestCaseWithFactory
	{
		public void TestChildControlIsUpdated()
		{
			var hAWB1 = Factory.New<CusHAWB>();
			using (var control = new AirCargoHAWBProviderContainerControlTestHelper())
			{
				control.HAWB = hAWB1;
				AssertEquals(hAWB1, control.HAWB);
				AssertEquals("HAWB should have been updated on child", hAWB1, control.Child.HAWB);
				control.Child.Dispose();
			}
		}

		sealed class AirCargoHAWBProviderContainerControlTestHelper : AirCargoHAWBProviderContainerControl
		{
			public AirCargoHAWBProviderContainerControlTestHelperChild Child = new AirCargoHAWBProviderContainerControlTestHelperChild();
			protected internal override AirCargoHAWBProviderContainerControl ChildControl => Child;
		}

		sealed class AirCargoHAWBProviderContainerControlTestHelperChild : AirCargoHAWBProviderContainerControl
		{
		}
	}
}
