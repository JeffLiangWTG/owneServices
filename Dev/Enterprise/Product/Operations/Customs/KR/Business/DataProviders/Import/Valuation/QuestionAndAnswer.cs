using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class QuestionAndAnswer : IQuestionAndAnswer
	{
		public string QuestionCode { get; set; }
		public string AnswerCode { get; set; }
		public string AnswerOtherDescription { get; set; }

		ZString IQuestionAndAnswer.QuestionCode => QuestionCode;
		ZString IQuestionAndAnswer.AnswerCode => AnswerCode;
		ZString IQuestionAndAnswer.AnswerOtherDescription => AnswerOtherDescription;
	}
}
