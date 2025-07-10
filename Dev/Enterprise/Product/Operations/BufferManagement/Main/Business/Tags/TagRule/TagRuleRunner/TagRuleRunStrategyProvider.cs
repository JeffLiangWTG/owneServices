using System;
using System.Globalization;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public static class TagRuleRunStrategyProvider
	{
		public static TagRuleRunStrategyBase GetStrategy(TagRule rule, BMServiceTaskProcessor processor, IConnectionProvider connectionProvider, ILogger logger)
		{
			var isWorkQueue = rule.TagTemplate.Magnitude is WorkQueue;
			return GetStrategy(isWorkQueue, rule.TGR_ActionType, processor, connectionProvider, logger);
		}

		public static TagRuleRunStrategyBase GetStrategyForAddAndRemoveRulePreview(TagRule rule, string previewType, BMServiceTaskProcessor processor, IConnectionProvider connectionProvider, ILogger logger)
		{
			var isWorkQueue = rule.TagTemplate.Magnitude is WorkQueue;
			var actionType = rule.TGR_ActionType == TagRuleActionTypeList.Codes.AddAndRemoveTag ? previewType : rule.TGR_ActionType.ToString();

			return GetStrategy(isWorkQueue, actionType, processor, connectionProvider, logger, getNonMatchingTagLinksForRemoveRule: true);
		}

		static TagRuleRunStrategyBase GetStrategy(bool isWorkQueue, string actionType, BMServiceTaskProcessor processor, IConnectionProvider connectionProvider, ILogger logger, bool getNonMatchingTagLinksForRemoveRule = false)
		{
			if (isWorkQueue)
			{
				switch (actionType)
				{
					case TagRuleActionTypeList.Codes.AddTag:
						return new AddToQueueRuleRunBizoStrategy(processor, connectionProvider, logger);

					case TagRuleActionTypeList.Codes.RemoveTag:
						return new RemoveTagRuleRunBizoStrategy(processor, connectionProvider, logger) { GetNonMatchingTagLinks = getNonMatchingTagLinksForRemoveRule };

					case TagRuleActionTypeList.Codes.AddAndRemoveTag:
						return new AddAndRemoveTagRuleBizoStrategy(processor, connectionProvider, logger);
				}
			}

			switch (actionType)
			{
				case TagRuleActionTypeList.Codes.AddTag:
					return new AddTagRuleRunSqlStrategy(connectionProvider, logger);

				case TagRuleActionTypeList.Codes.RemoveTag:
					return new RemoveTagRuleRunSqlStrategy(connectionProvider, logger) { GetNonMatchingTagLinks = getNonMatchingTagLinksForRemoveRule };

				case TagRuleActionTypeList.Codes.MaintainMagnitude:
					return new MaintainMagnitudeTagRuleRunStrategy(connectionProvider, logger);

				case TagRuleActionTypeList.Codes.AddAndRemoveTag:
					return new AddAndRemoveTagRuleRunSqlStrategy(connectionProvider, logger);

				default:
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot apply rule with unknown action type: {0}", actionType));
			}
		}
	}
}
