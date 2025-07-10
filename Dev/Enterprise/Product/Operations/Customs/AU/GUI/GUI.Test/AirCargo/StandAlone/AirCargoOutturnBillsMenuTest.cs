using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class AirCargoOutturnBillsMenuTest : TestCaseWithFactory
	{
		public void TestOverriddenNewDelegate()
		{
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			TestAirCargoOutturnBillsMenu.RegisterThisSubTypeOverride();
			using (AirCargoOutturnBillsMenu overriddenMenu = AirCargoOutturnBillsMenu.New(new CusUnderbondMessageManager(underbond)))
			{
				AssertEquals("New should be returning our overridden test class now", typeof(TestAirCargoOutturnBillsMenu), overriddenMenu.GetType());
			}
		}

		public void TestTypedManagerProperty()
		{
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			using (TestAirCargoOutturnBillsMenu testMenu = new TestAirCargoOutturnBillsMenu(new CusUnderbondMessageManager(underbond)))
			{
				AssertNotNull("Protected manager property should return the manager", testMenu.Manager);
			}
		}

		sealed class TestAirCargoOutturnBillsMenu : AirCargoOutturnBillsMenu
		{
			public TestAirCargoOutturnBillsMenu(CusUnderbondMessageManager manager) : base(manager)
			{
			}

			public static void RegisterThisSubTypeOverride()
			{
				OverridableNewDelegate.Value = new NewDelegate(OverriddenNew);
			}

			static AirCargoOutturnBillsMenu OverriddenNew(CusUnderbondMessageManager manager) => new TestAirCargoOutturnBillsMenu(manager);

			public new CusUnderbondMessageManager Manager => base.Manager;

			public MenuItem SendMessages => sendMessages;

			public MenuItem AmendMessages => amendMessages;

			public MenuItem WithdrawMessages => withdrawMessages;

			public MenuItem ResetToOriginal => resetToOriginal;
		}
	}
}
