namespace Enterprise.Accounting.DataTransfer.DataExport
{
	public class DataExportPaidTransactionValidation : AutoDataExportPaidTransactionValidation
	{
		public DataExportPaidTransactionValidation(AutoDataExportPaidTransaction parent)
			: base(parent) { }

		#region Implementation

		public new DataExportPaidTransaction Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (DataExportPaidTransaction)base.Parent; }
		}

		#endregion
	}
}
