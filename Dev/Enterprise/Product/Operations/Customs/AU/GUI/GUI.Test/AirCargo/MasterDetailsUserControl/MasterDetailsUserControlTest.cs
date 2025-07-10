using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class TestMasterDetailsUserControl : TestCaseWithFactory
	{
		public void TestVisibility()
		{
			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			using (ZForm form = new ZForm(hawb))
			{
				using (MasterDetailsUserControl control = new MasterDetailsUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(hawb, "");
					Assert(!control.AltPartShipModelCheckBox.Visible);
				}
			}

			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			using (ZForm form = new ZForm(hawb))
			{
				using (MasterDetailsUserControl control = new MasterDetailsUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(hawb, "");
					Assert(control.AltPartShipModelCheckBox.Visible);
				}
			}
		}
	}
}
