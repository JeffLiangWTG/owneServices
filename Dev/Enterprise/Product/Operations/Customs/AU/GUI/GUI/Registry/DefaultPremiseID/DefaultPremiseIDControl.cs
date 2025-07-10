using Enterprise.Registry.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class DefaultPremiseIDControl : RegistryZUserControl
	{
		public DefaultPremiseIDControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			DefaultPremiseIDGrid.ReadOnly = readOnly;
		}
	}
}
