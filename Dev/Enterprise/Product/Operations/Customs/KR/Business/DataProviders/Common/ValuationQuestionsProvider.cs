using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public class ValuationQuestionsProvider
	{
		public QuestionAndAnswer[] PopulateQuestionAndAnswer(JobComInvoiceHeader invoice, ZString questionCodeEA, ZString questionCodeEB, ZString questionCodeE)
		{
			var result = new List<QuestionAndAnswer>();
			var valuationQuestions = invoice.ValuationQuestions.Cast<ValuationQuestion>().OrderBy(x => x.CY_Code);

			foreach (var codeData in valuationQuestions)
			{
				if (codeData.CY_Code == questionCodeEB)
				{
					var question7E = result.FirstOrDefault(x => x.QuestionCode == questionCodeE);

					if (question7E != null)
					{
						question7E.AnswerOtherDescription = codeData.CY_Data;
					}
				}
				else
				{
					var code = codeData.CY_Code == questionCodeEA ? questionCodeE.ToString() : codeData.CY_Code.ToString();
					result.Add(new QuestionAndAnswer() { QuestionCode = code, AnswerCode = codeData.CY_Data });
				}
			}

			return result.ToArray();
		}
	}
}
