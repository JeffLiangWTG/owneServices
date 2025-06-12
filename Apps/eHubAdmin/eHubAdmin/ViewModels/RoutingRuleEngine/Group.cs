using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using eServices.eHubDataModel.eHubTransactions;
using Common.Logging;

namespace eServices.Shared.RoutingRuleEngine
{
	public class Group : Criterion
	{
		public Group()
			: this(new eHubRoutingRule() { RR_Group_MatchMultiple = false })
		{
		}

		public Group(IEnumerable<Criterion> subRules)
			: this(new eHubRoutingRule() { RR_Group_MatchMultiple = false })
		{
			this.SubRules.Load(subRules);
		}

		internal Group(eHubRoutingRule rule)
			: base(rule)
		{
			this.SubRules = new SubRuleCollection(rule);
		}

		internal override void Delete()
		{
			this.SubRules.Clear();
			base.Delete();
		}

		internal override void SetRuleContexts(Rule rule, eHubTransactionsContext persistingContext)
		{
			this.SubRules.SetRuleContexts(rule, persistingContext);
			base.SetRuleContexts(rule, persistingContext);
		}

		internal override Group FindGroupRule(string groupName)
		{
			if (this.GroupName == groupName) return this;
			foreach (var rule in this.SubRules)
			{
				var result = rule.FindGroupRule(groupName);
				if (result != null) return result;
			}
			if (this.FailedSubRule != null)
				return this.FailedSubRule.FindGroupRule(groupName);
			return null;
		}

		public string GroupName
		{
			get { return this.eHubRoutingRule.RR_Group_Name; }
			set { this.eHubRoutingRule.RR_Group_Name = value; }
		}

		public bool MatchMultiple
		{
			get { return base.eHubRoutingRule.RR_Group_MatchMultiple.Value; }
			set { base.eHubRoutingRule.RR_Group_MatchMultiple = value; }
		}

		public ServiceProvider DefaultServiceProvider
		{
			get { return base.ServiceProvider; }
			set { base.ServiceProvider = value; }
		}

		public eHubClient DefaultRecipient
		{
			get { return base.Recipient; }
			set { base.Recipient = value; }
		}

		public SubRuleCollection SubRules { get; private set; }
	}
}
