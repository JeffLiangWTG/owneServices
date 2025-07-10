using Enterprise.Registry.GUI;

namespace Enterprise.Client.UPE.Registry.GUI
{
	public partial class DocumentImageTypeControl : RegistryZUserControl
	{
		public DocumentImageTypeControl()
		{
			InitializeComponent();
			Grid.DisableImportDataMenuItem = true;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			Grid.ReadOnly = readOnly;
		}
	}
}
