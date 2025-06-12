using System;
using System.Collections.ObjectModel;
using eServices.eHubDataModel.eHubTransactions;
using Common.Logging;

namespace eServices.Shared.RoutingRuleEngine
{
	public interface IRule
	{
		FactCollection Facts { get; }
		string RuleID { get; }
		string ServiceName { get; }
		DateTime Timestamp { get; }
		ServiceProviderCollection ServiceProviders { get; }

		Group FindGroupRule(string groupName);
	}
}