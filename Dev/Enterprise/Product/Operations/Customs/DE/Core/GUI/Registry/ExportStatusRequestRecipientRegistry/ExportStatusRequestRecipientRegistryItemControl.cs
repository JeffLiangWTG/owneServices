using Enterprise.Registry.GUI;

namespace Enterprise.Customs.DE.GUI.Registry
{
	public partial class ExportStatusRequestRecipientRegistryItemControl : RegistryZUserControl
	{
		public ExportStatusRequestRecipientRegistryItemControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			MessageRecipientGrid.ReadOnly = readOnly;
		}
	}
}
