using System;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public class PrintJobFailed
	{
		public PrintJobFailed()
		{
		}

		public PrintJobFailed(Guid pk, string reason)
		{
			JobPk = pk;
			FailureReason = reason;
		}
		public Guid JobPk { get; set; }
		public string FailureReason { get; set; }
	}
}
