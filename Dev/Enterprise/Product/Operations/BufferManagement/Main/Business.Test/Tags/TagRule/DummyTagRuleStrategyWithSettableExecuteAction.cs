using System;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class DummyTagRuleStrategyWithSettableExecuteAction : DummyTagRuleStrategyWithSettableDuration
	{
		readonly Action OnExecuteAction;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public DummyTagRuleStrategyWithSettableExecuteAction(Action onExecuteAction, int secondsToWait)
			: base(secondsToWait)
		{
			OnExecuteAction = onExecuteAction;
		}

		public override long Execute(TagRule rule)
		{
			OnExecuteAction?.Invoke();
			base.Execute(rule);

			return 1;
		}

		public override ZQuery GetAffectedWorkflowsQuery(TagRule rule, bool asSubQuery = false)
		{
			return new ZQuery();
		}
	}
}
