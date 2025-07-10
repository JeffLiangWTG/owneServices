using Enterprise.Registry.GUI;

namespace Enterprise.Customs.GB.GUI.Registry
{
	public partial class ItemDefaulterControl : RegistryZUserControl
	{
		public ItemDefaulterControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			TaxCodesGrid.ReadOnly = readOnly;
		}
	}
}
