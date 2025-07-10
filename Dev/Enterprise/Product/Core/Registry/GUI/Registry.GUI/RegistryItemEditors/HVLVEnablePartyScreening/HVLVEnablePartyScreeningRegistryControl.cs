using System;
using CargoWise.Windows.UI;

namespace Enterprise.Registry.GUI
{
	public partial class HVLVEnablePartyScreeningRegistryControl : RegistryZUserControl
	{
		public HVLVEnablePartyScreeningRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			enableHVLVPartyScreeningCheckBox.ReadOnly = readOnly;
			enableNewDPSResultFormCheckBox.ReadOnly = shouldDPSResultFormCheckBoxBeReadOnly;
		}

		bool shouldDPSResultFormCheckBoxBeReadOnly => !enableHVLVPartyScreeningCheckBox.Checked || enableHVLVPartyScreeningCheckBox.ReadOnly;

		void UpdateDPSResultForm()
		{
			if (shouldDPSResultFormCheckBoxBeReadOnly)
			{
				enableNewDPSResultFormCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			}
			enableNewDPSResultFormCheckBox.SetReadOnly(shouldDPSResultFormCheckBoxBeReadOnly);
		}

		void EnableHVLVPartyScreeningCheckBox_CheckStateChanged(object sender, EventArgs e)
		{
			UpdateDPSResultForm();
		}
		
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			UpdateDPSResultForm();
		}
	}
}
