
using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ChargeCodeInvoiceTaxMessageOverrideUserControl : RegistryZUserControl
	{
		public ChargeCodeInvoiceTaxMessageOverrideUserControl()
		{
			InitializeComponent();
		}

		#region Overriden

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ChargeCodeInvoiceTaxMessageOverrideGrid.ReadOnly = readOnly;
		}

		#endregion
	}
}
