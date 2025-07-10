using Enterprise.Registry.GUI;

namespace Enterprise.Customs.EU.GUI.Registry
{
	public partial class NctsDefaultConsignorConsigneeRegistryItemUserControl : RegistryZUserControl
	{
		public NctsDefaultConsignorConsigneeRegistryItemUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			LeaveBlankCheckBox.ReadOnly = ConsigneeCheckBox.ReadOnly = ConsignorCheckBox.ReadOnly = ValueFromCheckBox.ReadOnly = readOnly;
		}

		void ValueFromCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			ConsignorCheckBox.Visible = ValueFromCheckBox.Checked;
			ConsigneeCheckBox.Visible = ValueFromCheckBox.Checked;
		}
	}
}
