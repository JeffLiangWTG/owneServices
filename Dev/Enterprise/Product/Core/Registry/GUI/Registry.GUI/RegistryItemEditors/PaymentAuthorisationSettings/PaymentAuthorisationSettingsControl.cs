using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	public partial class PaymentAuthorisationSettingsControl : RegistryZUserControl
	{
		public PaymentAuthorisationSettingsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			PaymentAuthorisationSettingsGrid.ReadOnly = readOnly;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (BindingSource.Current != null)
			{
				((AmountBasedAuthorisationRequirementCollection)BindingSource.Current).Sort();
			}
		}
	}
}
