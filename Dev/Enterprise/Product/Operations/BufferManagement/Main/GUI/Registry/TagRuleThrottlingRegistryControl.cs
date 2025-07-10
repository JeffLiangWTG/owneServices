using System;
using Enterprise.Registry.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class TagRuleThrottlingRegistryControl : RegistryZUserControl
	{
		public TagRuleThrottlingRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			kSplitContainer1.Enabled = !readOnly;
			IsThrottlingEnabledCheckBox.ReadOnly = readOnly;
			UpdateGridReadOnly();
		}

		void UpdateGridReadOnly()
		{
			ThresholdGrid.SetReadOnlyIncludingColumnStyles(ShouldGridBeReadOnly());
		}

		bool ShouldGridBeReadOnly()
		{
			return IsThrottlingEnabledCheckBox.ReadOnly || !IsThrottlingEnabledCheckBox.Checked;
		}

		void IsThrottlingEnabledCheckBox_CheckStateChanged(object sender, EventArgs e)
		{
			UpdateGridReadOnly();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			UpdateGridReadOnly();
		}
	}
}
