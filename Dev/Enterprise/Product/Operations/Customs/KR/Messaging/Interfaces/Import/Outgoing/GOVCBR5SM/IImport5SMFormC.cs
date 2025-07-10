using System.Collections.Generic;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5SMFormC
	{
		IEnumerable<IQuestionAndAnswer> Questions { get; }
	}
}
