namespace Enterprise.Registry.GUI
{
	public partial class GroupNotificationControl : RegistryZUserControl
	{
		public GroupNotificationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.GroupToSendGuidFindBox.ReadOnly = readOnly;
			this.SendNotificationsDropEdit.ReadOnly = readOnly;
		}
	}
}
