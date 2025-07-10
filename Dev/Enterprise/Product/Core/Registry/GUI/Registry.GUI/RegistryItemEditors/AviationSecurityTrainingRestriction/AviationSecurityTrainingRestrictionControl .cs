using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	[SuppressBindingMemberBashingTestAttribute]
	public partial class AviationSecurityTrainingRestrictionControl : RegistryBusinessObjectTemplateZUserControl
	{
		public AviationSecurityTrainingRestrictionControl()
		{
			InitializeComponent();
			applyCertificateRestrictionCheckBox.Visible = yesRadioButton.Checked;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			optionGroupBox.Enabled = !readOnly;
			applyCertificateRestrictionCheckBox.ReadOnly = readOnly;
		}

		void YesRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			applyCertificateRestrictionCheckBox.Visible = yesRadioButton.Checked;
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (DataSource is AviationSecurityTrainingRestriction aviationSecurityTrainingRestriction)
			{
				yesRadioButton.Checked = aviationSecurityTrainingRestriction.Enabled;
				noRadioButton.Checked = !aviationSecurityTrainingRestriction.Enabled;
			}
		}
	}
}
