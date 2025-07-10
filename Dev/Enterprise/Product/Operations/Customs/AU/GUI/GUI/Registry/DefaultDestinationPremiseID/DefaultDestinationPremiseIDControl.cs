using Enterprise.Registry.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class DefaultDestinationPremiseIDControl : RegistryZUserControl
	{
		public DefaultDestinationPremiseIDControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.DefaultDestinationPremiseIDGrid.ReadOnly = readOnly;
		}
	}
}
