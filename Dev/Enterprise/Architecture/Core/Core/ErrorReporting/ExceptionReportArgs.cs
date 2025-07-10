using System;

namespace Enterprise.ZArchitecture.Core
{
	public class ExceptionReportArgs : EventArgs
	{
		public ExceptionReportArgs(Exception ex, string errorReportID, string key, string errorDescription)
		{
			Ex = ex ?? new ArgumentException("No exception added to report.");
			ErrorReportID = errorReportID;
			Key = key;
			ErrorDescription = errorDescription;
		}

		public readonly Exception Ex;
		public string ErrorReportID;
		public readonly string Key;
		public readonly string ErrorDescription;

		public string SubjectPrefix;
		public bool ShutDownApplication;
		public Guid SessionId = Guid.Empty;
		public int Sequence = -1;
		public bool IsSlient;
	}
}
