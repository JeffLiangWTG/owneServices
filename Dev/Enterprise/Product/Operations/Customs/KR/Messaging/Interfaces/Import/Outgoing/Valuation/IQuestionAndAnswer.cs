using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IQuestionAndAnswer
	{
		ZString QuestionCode { get; }
		ZString AnswerCode { get; }
		ZString AnswerOtherDescription { get; }
	}
}
