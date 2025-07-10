using System;
using System.Linq;
using System.ServiceModel;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Business.RoutingRuleValidationWebService;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentVisualizer.Business
{
	public class RoutingRuleValidationCheck : IRoutingRuleValidationCheck
	{
		public RoutingRuleValidatorResponse SendValidationCheck(RoutingEvaluationOCMInput routingEvaluationOCMInput, ChannelFactory<IRoutingRuleValidationWebService> channelFactory)
		{
			try
			{
#if DEBUG
				if (Globals.IsTest)
				{
					throw new InvalidOperationException("do not use real service call in test");
				}
#endif
				var client = channelFactory.CreateChannel();
				// possible results:
				// - null
				// - collection with 1 element containing ErrorCode ERJ
				// - collection with 1 element containing RecipientId
				var result = client.EvaluateOCMAsync(routingEvaluationOCMInput).GetAwaiter().GetResult();

				return new RoutingRuleValidatorResponse
				{
					ValidationResult = result?.Length > 0
						&& result.Any(r => !string.IsNullOrWhiteSpace(r.RecipientId)),
					RecipientIds = result?.Where(r => !string.IsNullOrWhiteSpace(r.RecipientId)).Select(r => r.RecipientId).ToArray()
				};
			}
			catch (Exception exc) when (!exc.IsCriticalException())
			{
				return new RoutingRuleValidatorResponse
				{
					Exception = exc
				};
			}
		}
	}
}
