using System;
using eServices.eHubDataModel.eHubTransactions;

namespace eServices.Shared.RoutingRuleEngine
{
	public class Fact
	{
		public Fact()
			: this(new eHubRoutingRuleFact()) { }

		internal Fact(eHubRoutingRuleFact fact)
		{
			this.eHubRoutingRuleFact = fact;
			this.computeRule = Criterion.LoadRuleType(fact.eHubRoutingRule_Compute);
		}

		internal void SetRuleContexts(Rule rule, eHubTransactionsContext persistingContext)
		{
			this.rule = rule;
			this.persistingContext = persistingContext;
			if (this.computeRule != null)
				this.computeRule.SetRuleContexts(rule, persistingContext);
		}

		public string Name
		{
			get { return this.eHubRoutingRuleFact.RX_Name; }
			set { this.eHubRoutingRuleFact.RX_Name = value; }
		}

		public string Type
		{
			get { return this.eHubRoutingRuleFact.RX_Type; }
			set { this.eHubRoutingRuleFact.RX_Type = value; }
		}

		public string Query
		{
			get { return this.eHubRoutingRuleFact.RX_Query; }
			set { this.eHubRoutingRuleFact.RX_Query = value; }
		}

		public string Value { get; set; }

		private Criterion computeRule;
		public Criterion ComputeRule
		{
			get { return this.computeRule; }
			set
			{
				if (!Object.ReferenceEquals(value, this.computeRule))
				{
					if (this.computeRule != null)
						this.computeRule.Delete();
					if (value == null)
						this.eHubRoutingRuleFact.eHubRoutingRule_Compute = null;
					else
						this.eHubRoutingRuleFact.eHubRoutingRule_Compute = value.eHubRoutingRule;
					this.computeRule = value;
				}
			}
		}

		internal eHubRoutingRuleFact eHubRoutingRuleFact;
		eHubTransactionsContext persistingContext;
		Rule rule;
	}
}
