using Enterprise.Registry.GUI;

namespace Enterprise.Customs.GB.GUI.Registry
{
	public partial class CcsukNonstandardPimaControl : RegistryZUserControl
	{
		public CcsukNonstandardPimaControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			AddressesGrid.ReadOnly = readOnly;
		}
	}
}
