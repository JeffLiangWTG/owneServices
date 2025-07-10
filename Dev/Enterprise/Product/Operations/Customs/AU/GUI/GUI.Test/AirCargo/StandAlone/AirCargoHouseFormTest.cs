using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class AirCargoHouseFormTest : TestCaseWithFactory
	{
		public void TestShowPreSaveDialogs()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			using (AirCargoHouseForm form = new AirCargoHouseForm(hAWB))
			{
				AssertEquals(ContinueWithSave.Yes, ((IShowPreSaveDialog)form).ShowPreSaveDialogs());
			}
		}
	}
}
