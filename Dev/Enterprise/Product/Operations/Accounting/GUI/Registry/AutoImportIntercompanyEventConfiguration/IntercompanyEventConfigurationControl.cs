using System;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class IntercompanyEventConfigurationControl : RegistryZUserControl
	{
		public IntercompanyEventConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			IntercompanyEventSettingGrid.ReadOnly = readOnly;
			enableEventConfiguration.ReadOnly = readOnly;
			disableEventConfiguration.ReadOnly = readOnly;
			dropEditEventCode.IsReadOnly = readOnly;
			zDateEditStartDate.IsReadOnly = readOnly;
		}

		IntercompanyEventConfiguration BizObj => BoundBusinessObject as IntercompanyEventConfiguration;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (BizObj != null)
			{
				if (BizObj.EnableEventConfiguration)
				{
					enableEventConfiguration.Checked = true;
				}
				else
				{
					disableEventConfiguration.Checked = true;
				}
			}
		}

		void enableEventConfiguration_CheckedChanged(object sender, EventArgs e)
		{
			IntercompanyEventSettingGrid.ReadOnly = false;
			if (BizObj != null)
			{
				BizObj.EnableEventConfiguration = true;
			}
		}

		void disableEventConfiguration_CheckedChanged(object sender, EventArgs e)
		{
			IntercompanyEventSettingGrid.ReadOnly = true;
			if (BizObj != null)
			{
				BizObj.EnableEventConfiguration = false;
			}
		}
	}
}
