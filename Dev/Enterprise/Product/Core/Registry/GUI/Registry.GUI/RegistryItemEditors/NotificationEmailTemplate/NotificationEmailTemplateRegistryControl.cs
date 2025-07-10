namespace Enterprise.Registry.GUI
{
	public partial class NotificationEmailTemplateRegistryControl : RegistryBusinessObjectTemplateZUserControl
	{
		public NotificationEmailTemplateRegistryControl()
		{
			InitializeComponent();
		}

		public NotificationEmailTemplateRegistryControl(string groupBoxText, string subjectLabelText, string bodyLabelText, string helpText)
			: this()
		{
			this.notificationEmailTemplateControl1.SetAlternativeText(groupBoxText, subjectLabelText, bodyLabelText, helpText);
		}
	}
}
