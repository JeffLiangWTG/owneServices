using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business.Test
{
	public class DummyTagRuleRunner : TagRuleRunner
	{
		public DummyTagRuleRunner(ILogger logger, params TagRule[] tagRulesToRun_ForTest)
			: this(logger, setDurationInSeconds: null, tagRulesToRun_ForTest)
		{
		}
		public DummyTagRuleRunner(bool runRulesRegardlessOfLastRunTimeConsiderations, params TagRule[] tagRulesToRun_ForTest)
			: this(logger: new DummyLogger(), setDurationInSeconds: null, tagRulesToRun_ForTest)
		{
			this.runRulesRegardlessOfLastRunTimeConsiderations = runRulesRegardlessOfLastRunTimeConsiderations;
		}

		public DummyTagRuleRunner(ILogger logger, int? setDurationInSeconds, params TagRule[] tagRulesToRun_ForTest)
			: base(logger)
		{
			this.tagRulesToRun_ForTest = tagRulesToRun_ForTest;
			this.overrideDurationInSeconds = setDurationInSeconds;
		}

		readonly bool runRulesRegardlessOfLastRunTimeConsiderations;
		readonly TagRule[] tagRulesToRun_ForTest;
		readonly int? overrideDurationInSeconds;

		protected override long ProcessRuleCore(TagRule rule, TagRuleRunStrategyBase ruleRunStrategy)
		{
			var result = base.ProcessRuleCore(rule, ruleRunStrategy);
			if (overrideDurationInSeconds != null)
			{
				rule.TGR_LastRunDurationInSeconds = overrideDurationInSeconds.Value;
			}

			return result;
		}
		protected override TagRule[] GetTagRules()
		{
			return tagRulesToRun_ForTest;
		}

		protected override IEnumerable<TagRule> GetTagRulesToRun(TagRule[] tagRules)
		{
			return runRulesRegardlessOfLastRunTimeConsiderations ? tagRules : base.GetTagRulesToRun(tagRules);
		}

		protected override bool DoesRuleMeetThrottlingRequirements(TagRule rule, TagRuleThrottlingHeader registryItem)
		{
			return runRulesRegardlessOfLastRunTimeConsiderations || base.DoesRuleMeetThrottlingRequirements(rule, registryItem);
		}

		protected override TagRuleRunStrategyBase GetStrategy(TagRule rule, IConnectionProvider connectionProvider)
		{
			GetStrategyCalled?.Invoke(rule, EventArgs.Empty);

			return Strategy ?? base.GetStrategy(rule, connectionProvider);
		}

		public event EventHandler GetStrategyCalled;

		protected override IDisposable SwitchBranchAndDepartment(IDisposable context, Guid branchPK, Guid departmentPK)
		{
			contextSwitchCount++;
			return base.SwitchBranchAndDepartment(context, branchPK, departmentPK);
		}

		public TagRuleRunStrategyBase Strategy { get; set; }

		public int ContextSwitchCount
		{
			get { return contextSwitchCount; }
		}

		int contextSwitchCount;
	}
}
