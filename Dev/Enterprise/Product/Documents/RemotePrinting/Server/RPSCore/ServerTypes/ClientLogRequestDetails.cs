using System;
using Enterprise.RemotePrinting.Types;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public class ClientLogRequestDetails
	{
		public string EmailAddress { get; set; }

		public DateTime FromDate { get; set; }

		public DateTime ToDate { get; set; }

		public LogTypes LogTypes { get; set; }
	}
}
