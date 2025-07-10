using Enterprise.Registry.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public partial class DCAParametersControl : RegistryZUserControl
	{
		public DCAParametersControl()
		{
			InitializeComponent();
		}

		void allServicesZRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			serviceNamesZDisplayGrid.ReadOnly = ReadOnly || allServicesZRadioButton.Checked;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			peekWayZDropEdit.ReadOnly = readOnly;
			allServicesZRadioButton.ReadOnly = readOnly;
			specificServicesZRadioButton.ReadOnly = readOnly;
			maxMessagesZIntEdit.ReadOnly = readOnly;
			serviceNamesZDisplayGrid.ReadOnly = readOnly || allServicesZRadioButton.Checked;
		}
	}
}
