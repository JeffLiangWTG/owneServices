using System.Collections.Generic;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CachedAnsweredQuestions
	{
		public CachedAnsweredQuestions()
		{
			cachedQuestions = new Dictionary<JobComInvoiceLine, List<CachedAnswer>>();
		}

		readonly Dictionary<JobComInvoiceLine, List<CachedAnswer>> cachedQuestions;

		public void ClearQuestions()
		{
			foreach (List<CachedAnswer> questions in cachedQuestions.Values)
			{
				questions.Clear();
			}

			cachedQuestions.Clear();
		}

		public void CacheAnsweredQuestions(JobComInvoiceLine invoiceLine, CMRCusEntryCPDecCollection sourceQuestions)
		{
			if (!invoiceLine.IsNull)
			{
				List<CachedAnswer> result;

				if (!cachedQuestions.TryGetValue(invoiceLine, out result))
				{
					result = new List<CachedAnswer>();
					cachedQuestions.Add(invoiceLine, result);
				}

				foreach (CMRCusEntryCPDec question in sourceQuestions.ToArray())
				{
					if (question.IsAnswered)
					{
						result.Add(new CachedAnswer(question));
					}
				}
			}
		}

		public CachedAnswer GetCachedQuestionWithKey(CusEntryLine entryLine, LineDefaultKey qKey)
		{
			CachedAnswer result = null;

			foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
			{
				if (result == null)
				{
					result = GetCachedQuestionWithKey(invoiceLine, qKey);
				}
				else
				{
					var result2 = GetCachedQuestionWithKey(invoiceLine, qKey);
					if (!CachedAnswer.HasIdenticalAnswerPermit(result, result2))
					{
						result = null;
						break;
					}
				}
			}
			return result;
		}

		CachedAnswer GetCachedQuestionWithKey(JobComInvoiceLine invoiceLine, LineDefaultKey qKey)
		{
			List<CachedAnswer> list;
			CachedAnswer result = null;

			if (cachedQuestions.TryGetValue(invoiceLine, out list))
			{
				result = list.Find(x => x.QuestionID == qKey.QuestionID && x.StartDate == qKey.StartDate);
			}

			return result;
		}
	}
}
