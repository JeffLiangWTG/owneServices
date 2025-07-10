using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business.Test
{
	public class AddTagRuleRunSqlStrategyForTest : AddTagRuleRunSqlStrategy
	{
		public AddTagRuleRunSqlStrategyForTest(IConnectionProvider connectionProvider, ILogger logger, ITempTableWorker tempTableWorker = null, Action action = null)
			: base(connectionProvider, logger)
		{
			this.tableWorker = tempTableWorker;
			this.testAction = action;
		}

		ITempTableWorker tableWorker { get; }
		Action testAction { get; }

		protected override double GetFirstRunTime(TagRule rule, ZDateTime performanceTimestampStart)
		{
			if (rule.FirstRunTimeForTest != 0.0)
			{
				return rule.FirstRunTimeForTest;
			}

			return base.GetFirstRunTime(rule, performanceTimestampStart);
		}

		protected override ITempTableWorker GetWorkflowTempTableWorker(TagRule rule, IDbConnectionForReportingWrapper secondaryConnectionWrapper)
		{
			return tableWorker ?? base.GetWorkflowTempTableWorker(rule, secondaryConnectionWrapper);
		}

		protected override int PerformTaggingOnMainServer(TagRule rule, ITagDto[] headersToTag)
		{
			testAction?.Invoke();
			return base.PerformTaggingOnMainServer(rule, headersToTag);
		}
	}
}
