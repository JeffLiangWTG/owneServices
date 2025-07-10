namespace Enterprise.Registry.GUI
{
	public partial class ManifestGroupNotificationControl : RegistryZUserControl
	{
		public ManifestGroupNotificationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.SendGroupGuidFindBox.ReadOnly = readOnly;
			this.SendModeDropEdit.ReadOnly = readOnly;
			this.SendErrorOnlyCheckBox.ReadOnly = readOnly;
		}
	}
}
