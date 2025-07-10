using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class UsersAuthorizedToReopenClosedPeriodsControl : RegistryZUserControl
	{
		public UsersAuthorizedToReopenClosedPeriodsControl()
		{
			InitializeComponent();
		}

		#region Overriden

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			UsersAuthorizedToReopenClosedPeriodsGrid.ReadOnly = readOnly;
		}

		#endregion
	}
}

