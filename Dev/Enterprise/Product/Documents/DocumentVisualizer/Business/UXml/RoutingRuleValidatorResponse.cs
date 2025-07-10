using System;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class RoutingRuleValidatorResponse
	{
		public bool? ValidationResult { get; set; }
		public Exception Exception { get; set; }
		public string[] RecipientIds { get; set; }
	}
}
