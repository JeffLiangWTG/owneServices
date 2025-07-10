using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class BackDateInvoicesConfigurationControl : RegistryZUserControl
	{
		ZArchitecture.GUI.ZCheckBox OverridePostDateCheckBox;
		ZArchitecture.ZGrid InvoiceDateConfigurationGrid;
		CargoWise.Windows.UI.KSplitContainer splitContainer;
		ZArchitecture.GUI.ZGroupBox PostDateGroupBox;
		ZArchitecture.GUI.ZGroupBox InvoiceDateGroupBox;
		ZArchitecture.GUI.ZCheckBox DefaultPostDateFromInvoiceDateCheckBox;

		public BackDateInvoicesConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			OverridePostDateCheckBox.ReadOnly = readOnly;
			DefaultPostDateFromInvoiceDateCheckBox.ReadOnly = readOnly;
			InvoiceDateConfigurationGrid.ReadOnly = readOnly;
		}
	}
}

