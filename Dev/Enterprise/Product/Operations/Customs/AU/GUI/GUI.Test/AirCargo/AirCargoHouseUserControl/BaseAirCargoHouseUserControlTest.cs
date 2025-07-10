using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class BaseAirCargoHouseUserControlTest : TestCaseWithFactory
	{
		public void TestAirCargoCustomFieldsControl()
		{
			var hawb = Factory.New<CusHAWB>();
			using (var form = new ZForm(hawb))
			using (var control = new BaseAirCargoHouseUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(hawb, "");
				var masterTabControl = form.FindSingle<ZTemplateTabControl>("MasterTabControl");
				var customFieldsTabPage = form.FindSingle<ZTabPage>("HouseCustomFieldsTabPage");
				masterTabControl.SelectTab(customFieldsTabPage);
				var airCargoHouseCustomFieldsControl = customFieldsTabPage.FindSingle<Control>("AirCargoHouseCustomFieldsControl");
				Assert("Custom Fields tab is in tabs bar and has AirCargoCustomFieldsControl loaded", airCargoHouseCustomFieldsControl.Visible);
				var wrapperControl = (Customs.GUI.CustomFieldsWrapperControl)airCargoHouseCustomFieldsControl;
				AssertEquals("To make use of this tab, please setup Air Cargo House (HAC) custom fields in Workflow Manager", wrapperControl.NothingSetupMessageLabelText);
			}
		}

		public void TestBillParties()
		{
			using (var control = new BaseAirCargoHouseUserControl())
			{
				AssertEquals(true, control.airCargoHouseBillPartiesUserControl.Enabled);
			}
		}
	}
}
