using System.Collections.Generic;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5SMFormC : IImport5SMFormC
	{
		public QuestionAndAnswer[] Questions { get; set; }
		IEnumerable<IQuestionAndAnswer> IImport5SMFormC.Questions => Questions;
	}
}
