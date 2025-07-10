using CargoWise.EntityFramework;

namespace Enterprise.Registry.GUI
{
	public partial class ExportStatementSettingControl : RegistryZUserControl
	{
		public ExportStatementSettingControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			if (BindingSource.Current != null)
			{
				((IBusiness)BindingSource.Current).SetCountedReadOnlyIncludingChildren(readOnly);
			}
			CountryExportStatementSettingGrid.ReadOnly = readOnly;
			ExportStatementSettingGrid.ReadOnly = readOnly;
			AirGroupBox.Enabled = !readOnly;
			SeaGroupBox.Enabled = !readOnly;
		}
	}
}
