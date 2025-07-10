namespace Enterprise.ServiceManager.Business
{
	public class LogFileRecordValidation : AutoLogFileRecordValidation
	{
		public LogFileRecordValidation(AutoLogFileRecord parent)
			: base(parent) { }

		#region Implementation

		public new LogFileRecord Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (LogFileRecord)base.Parent; }
		}

		#endregion
	}
}

