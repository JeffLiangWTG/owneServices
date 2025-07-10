using System;

namespace CargoWise.Common
{
	[Serializable]
	public class RefDataException : Exception
	{
		public RefDataException(Exception ex, bool reportIssue = true) : base("Exception from RefData: " + ex.Message, ex)
		{
			ReportIssue = reportIssue;
		}
		public RefDataException(string message, Exception ex, bool reportIssue = true) : base(message, ex)
		{
			ReportIssue = reportIssue;
		}
		public bool ReportIssue { get; private set; }

#if NETFRAMEWORK
		protected RefDataException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
			ReportIssue = true;
		}
#endif
	}
}
