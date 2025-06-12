using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using eServices.eHubDataModel.eHubTransactions;

namespace eServices.Shared.RoutingRuleEngine
{
	public class FactCollection : IEnumerable<Fact>
	{
		internal FactCollection(eHubRoutingRule rule)
		{
			this.eHubRoutingRule = rule;
			foreach (var fact in rule.eHubRoutingRuleFacts)
				this.facts.Add(new Fact(fact));
		}

		public Fact Add(Fact fact)
		{
			this.facts.Add(fact);
			this.eHubRoutingRule.eHubRoutingRuleFacts.Add(fact.eHubRoutingRuleFact);
			fact.SetRuleContexts(this.rule, this.persistingContext);
			return fact;
		}

		public bool Remove(Fact item)
		{
			if (item == null) throw new ArgumentNullException("item");
			bool removed = this.facts.Remove(item);
			if (removed)
			{
				this.eHubRoutingRule.eHubRoutingRuleFacts.Remove(item.eHubRoutingRuleFact);
				if (this.persistingContext != null)
					this.persistingContext.eHubRoutingRuleFacts.Remove(item.eHubRoutingRuleFact);
			}
			return removed;
		}

		public void Clear()
		{
			this.eHubRoutingRule.eHubRoutingRuleFacts.Clear();
			if (this.persistingContext != null)
				this.persistingContext.eHubRoutingRuleFacts.RemoveRange(this.facts.Select(f => f.eHubRoutingRuleFact));
			this.facts.Clear();
		}

		public bool Contains(Fact item)
		{
			return facts.Contains(item);
		}

		public int Count
		{
			get { return this.facts.Count; }
		}

		public IEnumerator<Fact> GetEnumerator()
		{
			return (IEnumerator<Fact>)this.facts.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.facts.GetEnumerator();
		}

		public Fact this[int index]
		{
			get { return this.facts[index]; }
			set
			{
				var oldItem = this.facts[index];
				this.eHubRoutingRule.eHubRoutingRuleFacts.Remove(oldItem.eHubRoutingRuleFact);
				if (this.persistingContext != null)
					this.persistingContext.eHubRoutingRuleFacts.Remove(oldItem.eHubRoutingRuleFact);
				this.facts[index] = value;
				this.eHubRoutingRule.eHubRoutingRuleFacts.Add(value.eHubRoutingRuleFact);
			}
		}

		internal void SetRuleContexts(Rule rule, eHubTransactionsContext persistingContext)
		{
			this.facts.ForEach(p => p.SetRuleContexts(rule, persistingContext));
			this.persistingContext = persistingContext;
			this.rule = rule;
		}

		readonly eHubRoutingRule eHubRoutingRule;
		readonly List<Fact> facts = new List<Fact>();
		eHubTransactionsContext persistingContext;
		Rule rule;
	}
}
