namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class RoutingRuleValidatorRequest
	{
		public string ServiceUrl { get; set; }
		public string ClientId { get; set; }
		public string Password { get; set; }
		public string Interchange { get; set; }
	}
}
