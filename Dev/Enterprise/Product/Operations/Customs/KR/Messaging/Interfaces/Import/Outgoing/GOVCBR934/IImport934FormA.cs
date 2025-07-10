using System.Collections.Generic;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport934FormA
	{
		IEnumerable<IQuestionAndAnswer> Questions { get; }
		IValuation1_MethodData Method1ValuationData { get; }
	}
}
