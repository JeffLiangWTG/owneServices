using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class LineDefaultQuestionGenerator
	{
		public LineDefaultQuestions GenerateUniqueDefaultQuestions(ICPQALineAttachee lineAttachee, ZDateTime dateToFilter)
		{
			var defaultAnswers = new List<LineDefaultQuestions.DefaultAnswer>();
			foreach (var cpDecNumQuestions in lineAttachee.SourcesToDefault.SelectMany(x => x.Questions).Cast<CMRCusEntryCPDec>().Where(c => c.IsAnswered && (dateToFilter.IsEmpty || (dateToFilter.IsValid && IsInDateRange(c, dateToFilter)))).OrderBy(x => x.ON_CPDecNum).GroupBy(x => x.ON_CPDecNum))
			{
				var cpDecNumQuestionOrderByDates = cpDecNumQuestions.GroupBy(x => x.ON_CPDecStartDate).OrderByDescending(x => x.Key).First();
				var defaultAnswer = GenerateDefaultAnswer(cpDecNumQuestions.Key, cpDecNumQuestionOrderByDates);
				if (!defaultAnswer.AnswerCode.IsEmpty)
				{
					defaultAnswers.Add(defaultAnswer);
				}
			}
			return new LineDefaultQuestions(defaultAnswers);
		}

		static LineDefaultQuestions.DefaultAnswer GenerateDefaultAnswer(ZInt cpDecNum, IGrouping<ZDateTime, CMRCusEntryCPDec> cpDecNumQuestions)
		{
			var question = new LineDefaultQuestions.DefaultAnswer() { CPDecNum = cpDecNum, CPDecStartDate = cpDecNumQuestions.Key };
			foreach (var cpDecNumQuestion in cpDecNumQuestions)
			{
				if (question.AnswerCode.IsEmpty)
				{
					question.AnswerCode = cpDecNumQuestion.ON_AnswerCode;
				}
				else if (question.AnswerCode != cpDecNumQuestion.ON_AnswerCode)
				{
					question.AnswerCode = ZString.Empty;
					break;
				}

				if (question.Permit.IsEmpty)
				{
					question.Permit = cpDecNumQuestion.ON_Permit;
				}
			}

			return question;
		}

		bool IsInDateRange(CMRCusEntryCPDec question, ZDateTime date)
		{
			return (question.ON_CPDecStartDate.IsEmpty || question.ON_CPDecStartDate <= date) && (question.ON_CPDecEndDate.IsEmpty || question.ON_CPDecEndDate >= date);
		}
	}
}
