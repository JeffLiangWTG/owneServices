using System.Collections.Generic;
using Enterprise.Messaging.Business.MessageBuilders;

namespace Enterprise.Customs.Common.MessageBuilders
{
	public class MessageBuilderResult : IMessageBuilderResult
	{
		public void AddBuilderResult(IBuilderResult builderResult)
		{
			var result = builderResult as BuilderResult;
			if (result != null)
			{
				builderResults.Add(result);
			}
		}

		public IEnumerable<IBuilderResult> GetBuilderResults()
		{
			return builderResults;
		}

		public bool IsSuccess
		{
			get
			{
				foreach (IBuilderResult builderResult in builderResults)
				{
					if (builderResult.Errors.Length > 0)
					{
						return false;
					}
				}
				return true;
			}
		}

		readonly List<IBuilderResult> builderResults = new List<IBuilderResult>();
	}
}
