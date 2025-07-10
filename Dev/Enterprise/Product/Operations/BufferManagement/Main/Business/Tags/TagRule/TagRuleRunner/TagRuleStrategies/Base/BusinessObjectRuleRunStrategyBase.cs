using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	abstract class BusinessObjectTagRuleRunStrategyBase : TagRuleRunStrategyBase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public BusinessObjectTagRuleRunStrategyBase(BMServiceTaskProcessor processor, IConnectionProvider connectionProvider, ILogger logger)
			: base(connectionProvider, logger)
		{
			this.processor = processor;
		}

		readonly protected BMServiceTaskProcessor processor;

		protected static int BatchSize
		{
			get { return BMSRegistry.Instance.TagRuleRunnerBatchSize.Value; }
		}
	}
}
