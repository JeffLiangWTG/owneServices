using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoUnderbondAndPackingUserControlForCMRTest : TestCaseWithFactory
	{
		public void TestSACCheckBox()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = oceanBill.HouseBills.AddNew();
			var container = oceanBill.Containers.AddNew();
			var sCAPivot = container.Pivots.AddNew();
			sCAPivot.CV_CA = house.PK;
			using (var form = new ZForm(house))
			{
				using (var control = new SeaCargoUnderbondAndPackingUserControlForCMR())
				{
					control.ShowSACForm = true;
					form.Controls.Add(control);
					form.Show();
					control.SACCheckBox.Focus();
					control.SACCheckBox.Checked = false;
					AssertEquals("No form shown", false, ZFormModaliser.LastFormShownDialogForTest is CMRSACDialogBox);
					control.SACCheckBox.Checked = true;
					AssertEquals("Form shown", true, ZFormModaliser.LastFormShownDialogForTest is CMRSACDialogBox);
					ZFormModaliser.LastFormShownDialogForTest.Dispose();
				}
			}
		}
	}
}
