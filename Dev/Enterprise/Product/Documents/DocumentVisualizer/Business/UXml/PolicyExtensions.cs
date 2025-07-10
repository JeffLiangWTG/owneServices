using System;
using Polly;

namespace Enterprise.DocumentVisualizer.Business
{
	public static class PolicyExtensions
	{
		public static PolicyBuilder<RoutingRuleValidatorResponse> OrTransientException(this PolicyBuilder policyBuilder)
		{
			if (policyBuilder == null)
			{
				throw new ArgumentNullException(nameof(policyBuilder));
			}

			return policyBuilder.Or<Exception>().OrTransientInnerException();
		}

		public static PolicyBuilder<RoutingRuleValidatorResponse> OrTransientInnerException(this PolicyBuilder policyBuilder)
		{
			if (policyBuilder == null)
			{
				throw new ArgumentNullException(nameof(policyBuilder));
			}

			return policyBuilder.OrResult(TransientExceptionPredicate);
		}
		static readonly Func<RoutingRuleValidatorResponse, bool> TransientExceptionPredicate =
			(RoutingRuleValidatorResponse response) => response.Exception != null;
	}
}
