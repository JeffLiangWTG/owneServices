using System.Collections.Generic;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import934FormA : IImport934FormA
	{
		public QuestionAndAnswer[] Questions { get; set; }
		public Valuation1_MethodData Method1ValuationData { get; set; }

		IEnumerable<IQuestionAndAnswer> IImport934FormA.Questions => Questions;
		IValuation1_MethodData IImport934FormA.Method1ValuationData => Method1ValuationData;
	}
}
