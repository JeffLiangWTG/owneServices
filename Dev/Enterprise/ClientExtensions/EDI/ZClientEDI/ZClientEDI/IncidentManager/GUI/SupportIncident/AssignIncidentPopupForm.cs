using Enterprise.Client.EDI.IncidentManager.Business;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class AssignIncidentPopupForm : BaseIncidentPopupForm
	{
		public AssignIncidentPopupForm(SupportIncidentAssignStaffAction action) : base(action)
		{
			using (action.SuspendSettingHasChanges())
			{
				action.AssignToOther = true;
			}
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			InitializeComponent();
		}
	}
}
