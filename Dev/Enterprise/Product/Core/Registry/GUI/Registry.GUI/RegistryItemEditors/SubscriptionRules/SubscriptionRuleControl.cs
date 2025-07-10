using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class SubscriptionRuleControl : ZUserControl
	{
		public SubscriptionRuleControl()
		{
			InitializeComponent();
		}

		public void SetReadOnly(bool value)
		{
			nodeGrid.ReadOnly = value;
		}
	}
}
