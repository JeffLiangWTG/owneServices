using System.Globalization;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	class AddToQueueRuleRunBizoStrategy : AddTagRuleRunBizoStrategy
	{
		internal AddToQueueRuleRunBizoStrategy(BMServiceTaskProcessor processHeaderProcessor, IConnectionProvider connectionProvider, ILogger logger)
			: base(processHeaderProcessor, connectionProvider, logger)
		{
		}

		protected override string OrderByClause
		{
			get { return string.Format(CultureInfo.InvariantCulture, "{0}, {1}", ProcessHeaderSchema.Constants.FH_FH_ParentHeader, ProcessHeaderSchema.Constants.FH_CompletionStatement); }
		}
	}
}
