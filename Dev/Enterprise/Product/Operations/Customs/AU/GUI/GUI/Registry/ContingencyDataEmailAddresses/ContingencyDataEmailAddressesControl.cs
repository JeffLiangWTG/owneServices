using Enterprise.Registry.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class ContingencyDataEmailAddressesControl : RegistryZUserControl
	{
		public ContingencyDataEmailAddressesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ContingencyDataEmailAddressesGrid.ReadOnly = readOnly;
		}
	}
}
