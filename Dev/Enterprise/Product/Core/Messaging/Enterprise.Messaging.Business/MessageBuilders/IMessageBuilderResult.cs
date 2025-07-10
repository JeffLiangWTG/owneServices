using System.Collections.Generic;

namespace Enterprise.Messaging.Business.MessageBuilders
{
	public interface IMessageBuilderResult
	{
		IEnumerable<IBuilderResult> GetBuilderResults();
		bool IsSuccess { get; }
	}
}
