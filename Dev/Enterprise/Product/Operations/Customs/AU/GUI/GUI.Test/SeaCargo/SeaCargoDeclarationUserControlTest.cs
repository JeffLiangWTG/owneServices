using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestPluginLoaded()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_JS = shipment.PK;

			using (var form = new ZForm(houseBill))
			using (var seaCargoDeclarationUserControl = new SeaCargoDeclarationUserControl())
			{
				form.Controls.Add(seaCargoDeclarationUserControl);
				form.Show();

				bool underbondPluginPresent = false;

				var seaCargoBoundTabControl = seaCargoDeclarationUserControl.FindSingle<ZTemplateTabControl>("SeaCargoBoundTabControl");
				foreach (ZPlugIn plugin in seaCargoBoundTabControl.PlugIns.Instances)
				{
					underbondPluginPresent |= plugin.GetType() == typeof(CMRCusUnderbondPlugin);
				}

				AssertEquals("UnderbondPluginPresent", true, underbondPluginPresent);
			}
		}

		public void TestPluginOutturnDisabled()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_JS = shipment.PK;

			using (var form = new ZForm(houseBill))
			using (var seaCargoDeclarationUserControl = new SeaCargoDeclarationUserControl())
			{
				form.Controls.Add(seaCargoDeclarationUserControl);
				form.Show();

				CusUnderbondUserControl underbondControl = null;

				var seaCargoBoundTabControl = seaCargoDeclarationUserControl.FindSingle<ZTemplateTabControl>("SeaCargoBoundTabControl");
				foreach (ZTabPage page in seaCargoBoundTabControl.TabPages)
				{
					seaCargoBoundTabControl.SelectedTab = page;
					foreach (Control control in page.Controls)
					{
						underbondControl = control as CusUnderbondUserControl;
						if (underbondControl != null)
						{
							break;
						}
					}
				}

				AssertNotNull("Failed to load underbond user control", underbondControl);
				AssertEquals("Outturns Disabled", true, underbondControl.OutturnDisabled);
			}
		}

		public void TestSaveButtonIsActivatedWhenOceanBillIsChanged()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();
			using (houseBill.SuspendSettingHasChangesIncludingChildren())
			{
				houseBill.CA_JS = shipment.PK;
			}

			using (var form = new TestFormWithSaveButton(houseBill))
			using (var seaCargoDeclarationUserControl = new SeaCargoDeclarationUserControl())
			{
				form.Controls.Add(seaCargoDeclarationUserControl);
				form.Show();

				var seaCargoShipmentUserControl = seaCargoDeclarationUserControl.FindSingle<SeaCargoShipmentUserControl>("SeaCargoShipmentUserControl");
				var houseDetailsUserControl = seaCargoShipmentUserControl.FindSingle<SeaCargoHouseDetailsUserControl>("HouseDetailsUsersControl");
				var billDetailsTabControl = houseDetailsUserControl.FindSingle<ZTemplateTabControl>("BillDetailsTabControl");
				billDetailsTabControl.SelectedTab = (ZTabPage)billDetailsTabControl.TabPages["OceanBillDetailsTabPage"];

				var oceanBillTextBox = houseDetailsUserControl.FindSingle<ZTextBox>("CB_OceanBillBoundTextBox");
				var principalIDTextBox = houseDetailsUserControl.FindSingle<ZTextBox>("PrincipalIDBoundTextBox");

				AssertEquals("Not Changed", false, houseBill.HasChanges);
				AssertEquals("Cannot Save", false, form.SaveAndCloseButton.Enabled);

				oceanBillTextBox.Focus();
				oceanBillTextBox.Text = "OB123";
				principalIDTextBox.Focus();

				AssertEquals("OceanBill Changed", true, oceanBill.HasChanges);
				AssertEquals("HouseBill reflects Change", true, houseBill.HasChanges);
				AssertEquals("Can Save", true, form.SaveAndCloseButton.Enabled);
			}
		}

		public void TestNewHouseBillSetToPlugin()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_JS = shipment.PK;

			using (var form = new TestFormWithSaveButton(houseBill))
			using (var seaCargoDeclarationUserControl = new SeaCargoDeclarationUserControl())
			{
				form.Controls.Add(seaCargoDeclarationUserControl);
				form.Show();

				ZPlugIn underbondPlugin = seaCargoDeclarationUserControl.FindSingle<ZTabPagePlugIn>(tab => tab.PlugIn is CMRCusUnderbondPlugin).PlugIn;
				AssertEquals("Intial housebill set", houseBill, underbondPlugin.BusinessEntity[0]);
				houseBill.Delete();
				var newHouseBill = oceanBill.HouseBills.AddNew();
				seaCargoDeclarationUserControl.SetDataBinding(newHouseBill, "");
				AssertEquals("Housebill replaced", newHouseBill, underbondPlugin.BusinessEntity[0]);
				AssertEquals("Initial housebill deleted", 1, underbondPlugin.BusinessEntity.Count);
			}
		}

		sealed class TestFormWithSaveButton : ZEditForm
		{
			public TestFormWithSaveButton(BusinessObject businessObject) : base(businessObject)
			{
			}

			public new ZPanel MainPanel => base.MainPanel;

			public IButton SaveAndCloseButton => ((IPostingButtonsProvider)this).CommandButtonPost;
		}
	}
}
