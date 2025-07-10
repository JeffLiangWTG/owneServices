using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class DiscountSettingControl : ZUserControl
	{
		public DiscountSettingControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				isManualOverrideCheckBox.DataBindings.RemoveBinding("IsVisibleForBinding");
			}
			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null)
			{
				isManualOverrideCheckBox.DataBindings.Add(new KBinding("IsVisibleForBinding", DataSource, "IsManualOverrideApplicable"));
			}
		}
	}
}
