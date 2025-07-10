using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class MultipleEmailTemplatesControl : RegistryZUserControl
	{
		public MultipleEmailTemplatesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.CategoryGrid.ReadOnly = readOnly;
			this.notificationEmailTemplateRegistryControl1.ReadOnly = readOnly;
		}
	}
}
