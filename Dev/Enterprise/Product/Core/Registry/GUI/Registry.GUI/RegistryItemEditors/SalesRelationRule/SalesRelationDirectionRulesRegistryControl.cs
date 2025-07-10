using System;
using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	public partial class SalesRelationDirectionRulesRegistryControl : RegistryZUserControl
	{
		public SalesRelationDirectionRulesRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			newButton.ReadOnly = readOnly;
			directionRuleControl.SetReadOnly(readOnly);
			directionRuleGrid.AllowReadOnlyRowsToBeDeleted = !readOnly;
		}

		void newButton_Click(object sender, EventArgs e)
		{
			var newRule = ((SalesRelationDirectionRuleCollection)DataSource).AddNew();
			directionRuleGrid.SelectSingleElement(newRule);
			directionRuleControl.Focus();
		}
	}
}
