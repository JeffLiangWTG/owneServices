namespace Enterprise.Accounting.DataTransfer.DataExport
{
	public class DataExportDirectDebitBatchValidation : AutoDataExportDirectDebitBatchValidation
	{
		public DataExportDirectDebitBatchValidation(AutoDataExportDirectDebitBatch parent)
			: base(parent) { }

		#region Implementation

		public new DataExportDirectDebitBatch Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (DataExportDirectDebitBatch)base.Parent; }
		}

		#endregion
	}
}
