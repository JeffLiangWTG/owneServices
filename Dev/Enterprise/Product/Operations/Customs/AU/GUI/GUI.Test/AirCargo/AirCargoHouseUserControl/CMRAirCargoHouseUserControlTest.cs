namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class CMRAirCargoHouseUserControlTest : NUnit.Framework.TestCase
	{
		public void TestChildControl()
		{
			using (var control = new CMRAirCargoHouseUserControl())
			{
				AssertEquals(control.cmrHouseDetailsUserControl, control.ChildControl);
			}
		}

		public void TestBillParties()
		{
			using (var control = new CMRAirCargoHouseUserControl())
			{
				AssertEquals(true, control.airCargoHouseBillPartiesUserControl.Enabled);
			}
		}
	}
}
