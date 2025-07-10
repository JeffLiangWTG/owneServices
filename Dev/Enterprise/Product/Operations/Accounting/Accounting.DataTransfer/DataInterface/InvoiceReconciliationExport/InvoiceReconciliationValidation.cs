namespace Enterprise.Accounting.DataTransfer.DataInterface
{
	public class InvoiceReconciliationValidation : AutoInvoiceReconciliationValidation
	{
		public InvoiceReconciliationValidation(AutoInvoiceReconciliation parent)
			: base(parent) { }

		#region Implementation

		public new InvoiceReconciliation Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (InvoiceReconciliation)base.Parent; }
		}

		#endregion
	}
}
