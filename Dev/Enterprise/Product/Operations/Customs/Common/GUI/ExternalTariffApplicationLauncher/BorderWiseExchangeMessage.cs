using System;

namespace Enterprise.Customs.Common.GUI
{
	public class BorderWiseExchangeMessage
	{
		public Guid ClientId { get; set; }
		public string CargoWiseClientId { get; set; }
		public string SystemNameSender { get; set; }
		public string SystemNameRecipient { get; set; }
		public BorderWiseWebSocketMessageStatus Status { get; set; }
		public string Data { get; set; }
		public string Type { get; set; }
		public string Mode { get; set; }
		public string Email { get; set; }
		public string OrgCode { get; set; }
		public int CargoWiseDatabaseNumber { get; set; }
		public string CountryCode { get; set; }
	}
}
