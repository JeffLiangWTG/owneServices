using System;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class EdiAccessTokenInfo
	{
		public Guid OwnerId { get; set; }
		public string OwnerTableCode { get; set; }
		public string ResourceProduct { get; set; }
		public string ResourceSystemId { get; set; }
		public string Scope { get; set; }
	}
}