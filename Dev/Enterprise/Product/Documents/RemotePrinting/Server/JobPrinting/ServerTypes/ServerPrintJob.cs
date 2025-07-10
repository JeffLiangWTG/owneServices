using System;

namespace Enterprise.RemotePrinting.Server.JobPrinting
{
	public class ServerPrintJob
	{
		public ServerPrintJob()
		{
		}

		#region Properties

		public Guid JobPk { get; set; }

		public byte[] Contents { get; set; }

		public string BlobType { get; set; }

		public string JobType { get; set; } = "PRN";

		public string EmailSubjectLine { get; set; }

		public byte[] EscapeSequence { get; set; }

		public int Copies { get; set; }

		public bool HasWatermark { get; set; }

		public string QueueName { get; set; }

		public Guid QueueStateChangedStamp { get; set; }

		#endregion
	}
}
