using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using eServices.eHubDataModel.eHubTransactions;
using Common.Logging;

namespace eServices.Shared.RoutingRuleEngine
{
	public abstract class Criterion
	{
		internal Criterion(eHubRoutingRule rule)
		{
			eHubRoutingRule = rule;
			successSubRule = LoadRuleType(rule.eHubRoutingRule_Success);
			failedSubRule = LoadRuleType(rule.eHubRoutingRule_Failed);
			ReferenceID = rule.RR_PK;
		}

		internal static Criterion LoadRuleType(eHubRoutingRule rule)
		{
			if (rule == null)
				return null;
			else if (rule.RR_Group_MatchMultiple.HasValue)
				return new Group(rule);
			else
				return new Condition(rule);
		}

		internal virtual void Delete()
		{
			this.SuccessSubRule = null;
			this.FailedSubRule = null;
			if (this.persistingContext != null)
				this.persistingContext.eHubRoutingRules.Remove(this.eHubRoutingRule);
		}

		internal virtual void SetRuleContexts(Rule rule, eHubTransactionsContext persistingContext)
		{
			if (this.successSubRule != null)
				this.successSubRule.SetRuleContexts(rule, persistingContext);

			if (this.failedSubRule != null)
				this.failedSubRule.SetRuleContexts(rule, persistingContext);

			if (this.eHubRoutingRule.eHubServiceProvider != null)
				this.serviceProvider = rule.ServiceProviders.SingleOrDefault(s => s.eHubServiceProvider == this.eHubRoutingRule.eHubServiceProvider);

			this.persistingContext = persistingContext;
			this.rule = rule;
		}

		internal abstract Group FindGroupRule(string groupName);

		public string ErrorCode
		{
			get { return this.eHubRoutingRule.RR_Failed_ErrorCode; }
			set { this.eHubRoutingRule.RR_Failed_ErrorCode = value; }
		}

		public string ErrorDescription
		{
			get { return this.eHubRoutingRule.RR_Failed_ErrorDescription; }
			set { this.eHubRoutingRule.RR_Failed_ErrorDescription = value; }
		}

		public string Value
		{
			get { return this.eHubRoutingRule.RR_Result_Value; }
			set { this.eHubRoutingRule.RR_Result_Value = value; }
		}

		Criterion successSubRule;
		public Criterion SuccessSubRule
		{
			get { return this.successSubRule; }
			set
			{
				if (!Object.ReferenceEquals(value, this.successSubRule))
				{
					if (this.successSubRule != null)
						this.successSubRule.Delete();
					if (value == null)
						this.eHubRoutingRule.eHubRoutingRules_Success = null;
					else
						this.eHubRoutingRule.eHubRoutingRule_Success = value.eHubRoutingRule;
					this.successSubRule = value;
				}
			}
		}

		Criterion failedSubRule;
		public Criterion FailedSubRule
		{
			get { return this.failedSubRule; }
			set
			{
				if (!Object.ReferenceEquals(value, this.failedSubRule))
				{
					if (this.failedSubRule != null)
						this.failedSubRule.Delete();
					if (value == null)
						this.eHubRoutingRule.eHubRoutingRule_Failed = null;
					else
						this.eHubRoutingRule.eHubRoutingRule_Failed = value.eHubRoutingRule;
					this.failedSubRule = value;
				}
			}
		}

		public Guid ReferenceID { get; private set; }

		public string ResultValue
		{
			get { return eHubRoutingRule.RR_Result_Value; }
			set { eHubRoutingRule.RR_Result_Value = value; }
		}

		ServiceProvider serviceProvider;
		public ServiceProvider ServiceProvider
		{
			get { return this.serviceProvider; }
			set
			{
				if (value == null)
				{
					this.eHubRoutingRule.eHubServiceProvider = null;
					this.eHubRoutingRule.RR_Success_SP_Provider = null;
				}
				else
				{
					this.eHubRoutingRule.eHubServiceProvider = value.eHubServiceProvider;
					this.eHubRoutingRule.RR_Success_SP_Provider = value.eHubServiceProvider.SP_PK;
					if (this.rule == null)
						this.serviceProvider = value;
					else
						if ((this.serviceProvider = rule.ServiceProviders.SingleOrDefault(s => s.eHubServiceProvider == value.eHubServiceProvider)) == null)
							rule.ServiceProviders.Add(new ServiceProvider(value.eHubServiceProvider));
				}
			}
		}

	    public eHubClient Recipient
		{
			get { return this.eHubRoutingRule.eHubClient_Recipient; }
			set
			{
				this.eHubRoutingRule.eHubClient_Recipient = value;
			}
		}

		internal string GetRowInfoString()
		{
			return CurrentRowData;
		}

	    internal readonly eHubRoutingRule eHubRoutingRule;
		internal Rule rule;
		internal eHubTransactionsContext persistingContext;

		protected string CurrentRowData
		{
			get
			{
				var notVirtualProperties = eHubRoutingRule.GetType().GetProperties().Where(x => !x.GetAccessors().Any(a => a.IsVirtual)).OrderBy(i => i.Name != "RR_PK").ThenBy(i => i.Name).Select(x => string.Format("{0}='{1}'", x.Name, x.GetValue(eHubRoutingRule)));
				return string.Join(", ", notVirtualProperties);
			}
		}
	}
}
