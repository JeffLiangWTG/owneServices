namespace Enterprise.Accounting.DataTransfer.DataExport
{
	public class DataExportPositivePayValidation : AutoDataExportPositivePayValidation
	{
		public DataExportPositivePayValidation(AutoDataExportPositivePay parent)
			: base(parent) { }

		#region Implementation

		public new DataExportPositivePay Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (DataExportPositivePay)base.Parent; }
		}

		#endregion
	}
}
