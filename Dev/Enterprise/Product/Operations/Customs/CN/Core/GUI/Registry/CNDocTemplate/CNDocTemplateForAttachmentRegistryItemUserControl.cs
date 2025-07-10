using Enterprise.Registry.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class CNDocTemplateForAttachmentRegistryItemUserControl : RegistryZUserControl
	{
		public CNDocTemplateForAttachmentRegistryItemUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			MainGrid.ReadOnly = readOnly;
		}
	}
}
