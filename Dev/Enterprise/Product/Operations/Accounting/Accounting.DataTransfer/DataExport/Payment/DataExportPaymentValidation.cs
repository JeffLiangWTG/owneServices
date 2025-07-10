namespace Enterprise.Accounting.DataTransfer.DataExport
{
	public class DataExportPaymentValidation : AutoDataExportPaymentValidation
	{
		public DataExportPaymentValidation(AutoDataExportPayment parent)
			: base(parent) { }

		#region Implementation

		public new DataExportPayment Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (DataExportPayment)base.Parent; }
		}

		#endregion
	}
}
