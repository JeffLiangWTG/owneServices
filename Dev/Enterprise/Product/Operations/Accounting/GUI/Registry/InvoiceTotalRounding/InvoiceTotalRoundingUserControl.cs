using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class InvoiceTotalRoundingUserControl : RegistryZUserControl
	{
		public InvoiceTotalRoundingUserControl()
		{
			InitializeComponent();
		}

		#region Overriden

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			InvoiceTotalRoundingGrid.ReadOnly = readOnly;
		}

		#endregion
	}
}

