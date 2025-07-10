using System;
using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	public partial class SubscriptionRulesRegistryControl : RegistryZUserControl
	{
		public SubscriptionRulesRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			newButton.ReadOnly = readOnly;
			subscriptionRuleControl.SetReadOnly(readOnly);
			subscriptionRuleGrid.AllowReadOnlyRowsToBeDeleted = !readOnly;
		}

		void NewButton_Click(object sender, EventArgs e)
		{
			var newRule = ((SubscriptionRuleCollection)DataSource).AddNew();
			subscriptionRuleGrid.SelectSingleElement(newRule);
			subscriptionRuleControl.Focus();
		}
	}
}
