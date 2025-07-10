using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI.Testing
{
	sealed class AirCagoHouseBillPartiesUserControlTest : TestCaseWithFactory
	{
		public void TestVisibility()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			using (var form = new ZForm(mawb))
			using (var control = new CMRACAStandAloneUserControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(mawb, "");
				form.Show();
				var tabControl = form.FindSingle<ZTemplateTabControl>("miscInfoTabControl");
				var consigneeTabPage = form.FindSingle<ZTabPage>("consigneeTabPage");
				tabControl.SelectedTab = consigneeTabPage;
				Assert("ConsigneezAddressControl is visible", form.FindSingle<ZAddressControl>("ConsigneezAddressControl").Visible);
				Assert("cS_OH_ConsigneeGuidFindBox is Invisible", !form.FindSingle<ZGuidFindBox>("cS_OH_ConsigneeGuidFindBox").Visible);

				var consignorTabPage = form.FindSingle<ZTabPage>("consignorTabPage");
				tabControl.SelectedTab = consignorTabPage;
				Assert("ConsigneezAddressControl is visible", form.FindSingle<ZAddressControl>("ConsignorzAddressControl").Visible);
				Assert("cS_OH_ConsignorGuidFindBox is Invisible", !form.FindSingle<ZGuidFindBox>("cS_OH_ConsignorGuidFindBox").Visible);
			}
		}

		public void TestZAddressControlOrgListBindings()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();

			using (var userControl = new AirCagoHouseBillPartiesUserControl())
			{
				userControl.SetDataBinding(hawb, string.Empty);
				AssertEquals("Lookups+ConsigneeList", userControl.ConsigneezAddressControl.BindToOrgList);
				AssertEquals("Lookups+ConsignorList", userControl.ConsignorzAddressControl.BindToOrgList);

				userControl.SetDataBinding(mawb, nameof(CusMAWB.ChildBills));
				AssertEquals("ChildBills.Lookups+ConsigneeList", userControl.ConsigneezAddressControl.BindToOrgList);
				AssertEquals("ChildBills.Lookups+ConsignorList", userControl.ConsignorzAddressControl.BindToOrgList);
			}
		}
	}
}
