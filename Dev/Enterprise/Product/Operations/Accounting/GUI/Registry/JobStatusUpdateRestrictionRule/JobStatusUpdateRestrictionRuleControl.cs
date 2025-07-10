using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class JobStatusUpdateRestrictionRuleControl : RegistryZUserControl
	{
		public JobStatusUpdateRestrictionRuleControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			JobStatusUpdateRestrictionRuleGrid.ReadOnly = readOnly;
		}
	}
}
