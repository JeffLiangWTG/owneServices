using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class KoreaSouthEInvoicingDSBSummaryAppendingRuleControl : RegistryZUserControl
	{
		public KoreaSouthEInvoicingDSBSummaryAppendingRuleControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ConfigurationGrid.ReadOnly = readOnly;
		}
	}
}
