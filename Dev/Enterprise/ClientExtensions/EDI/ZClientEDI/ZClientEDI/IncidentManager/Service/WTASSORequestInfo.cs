namespace Enterprise.Client.EDI.IncidentManager.Service
{
	public class WTASSORequestInfo
	{
		public string Token { get; set; }
		public string TenantId { get; set; }
		public string ContactEmail { get; set; }
		public string ContactName { get; set; }
		public string ContactKey { get; set; }
		public string OrganisationKey { get; set; }
		public string OrganisationName { get; set; }
		public string PersonIDs { get; set; }
		public bool IsTesting { get; set; }
	}
}
