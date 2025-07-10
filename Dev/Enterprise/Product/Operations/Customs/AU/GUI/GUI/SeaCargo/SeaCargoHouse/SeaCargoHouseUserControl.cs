using System;
using System.Linq;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoHouseUserControl : ZUserControl
	{
		public SeaCargoHouseUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupPlugIn(ZArchitecture.Modules.ControllerIDs.Customs.AU.CusSCAHouseCusUnderbondPluginController);
			OceanBillDetailsUserControl.SetMessagingModeVisiblity(false);
			HouseBillTabControl.SelectedIndexChanged += HouseBillTabControl_SelectedIndexChanged;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (!HouseBillCustomFieldsControl.IsDisposed)
			{
				HouseBillCustomFieldsControl.ForceBindingIncludingParents();
			}
		}

		public void SetupPlugIn(ZArchitecture.Modules.ControllerID controller)
		{
			if (HouseBillTabControl.PlugIns.GetPlugIn(controller) == null)
			{
				HouseBillTabControl.PlugIns.Add(controller);
			}
		}

		void HouseBillTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (HouseBillTabControl.SelectedTab is ZTabPage tabPage)
			{
				if (tabPage.Name == "CustomsUnderbondMovementTabPage")
				{
					foreach (var underbondControl in tabPage.Controls.OfType<CusUnderbondUserControl>())
					{
						underbondControl.OutturnDisabled = true;
					}
				}
			}
		}
	}
}
