using Enterprise.Registry.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class DefaultFreightPercentageControl : RegistryZUserControl
	{
		public DefaultFreightPercentageControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			DefaultFreightPercentageGrid.ReadOnly = readOnly;
		}
	}
}
