using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using eServices.eHubDataModel.eHubTransactions;
using Common.Logging;

namespace eServices.Shared.RoutingRuleEngine
{
	public sealed class Rule : Group, IRule
	{
		static Rule()
		{
			Database.SetInitializer<eHubTransactionsContext>(null);
		}

		public Rule(eHubTransactionsContext context, eHubClient client)
			: this(context, client, new eHubRoutingRule() { RR_Group_MatchMultiple = false })
		{
			if (client.eHubRoutingRule != null)
				throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Unable to create rule. Rule already exists for client ID '{0}'.", client.CC_ID));

			client.eHubRoutingRule = base.eHubRoutingRule;
		}

		public static Rule GetForReading(eHubClient client)
		{
			if (client == null) throw new ArgumentNullException("client");
			if (client.eHubRoutingRule == null)
				throw new InvalidOperationException(String.Format("Rule not found for client ID '{0}'.", client.CC_ID));

			var rule = new Rule(client, client.eHubRoutingRule);
			rule.SetRuleContexts();
			return rule;
		}

		public static Rule GetForEditing(eHubTransactionsContext context, eHubClient client)
		{
			if (context == null) throw new ArgumentNullException("context");
			if (client == null) throw new ArgumentNullException("client");
			if (client.eHubRoutingRule == null)
				return null;

			return new Rule(context, client, client.eHubRoutingRule);
		}

		internal Rule(eHubTransactionsContext context, eHubClient client, eHubRoutingRule rule)
			: this(client, rule)
		{
			if (context == null) throw new ArgumentNullException("context");
			SetRuleContexts(context);
		}

		internal Rule(eHubClient client, eHubRoutingRule rule)
			: base(rule)
		{
			if (client == null) throw new ArgumentNullException("client");
			this.client = client;
			this.Facts = new FactCollection(rule);
			this.ServiceProviders = new ServiceProviderCollection(client);
			this.Timestamp = base.eHubRoutingRule.RR_LastUpdateUTC.GetValueOrDefault();
			base.rule = this;
		}

		internal void SetRuleContexts(eHubTransactionsContext persistingContext = null)
		{
			base.SetRuleContexts(this, persistingContext);
			this.Facts.SetRuleContexts(this, persistingContext);
			this.ServiceProviders.SetRuleContexts(this, persistingContext);
		}

		public static void Delete(eHubTransactionsContext context, eHubClient client)
		{
			if (context == null) throw new ArgumentNullException("context");
			if (client == null) throw new ArgumentNullException("client");
			var rule = Rule.GetForEditing(context, client);
			if (rule == null)
				throw new InvalidOperationException(String.Format("Rule not found for client ID '{0}'.", client.CC_ID));

			rule.Delete();
		}

		internal override void Delete()
		{
			this.Facts.Clear();
			base.Delete();
		}

		public new Group FindGroupRule(string groupName)
		{
			foreach (var rule in this.SubRules)
			{
				var result = rule.FindGroupRule(groupName);
				if (result != null) return result;
			}
			if (this.FailedSubRule != null)
				return this.FailedSubRule.FindGroupRule(groupName);
			return null;
		}

		public FactCollection Facts { get; private set; }
		public ServiceProviderCollection ServiceProviders { get; private set; }

		eHubClient client;
		public string RuleID { get { return client.CC_ID; } }
		public string ServiceName { get { return client.CC_FriendlyName; } }
		public DateTime Timestamp { get; }
	}
}
