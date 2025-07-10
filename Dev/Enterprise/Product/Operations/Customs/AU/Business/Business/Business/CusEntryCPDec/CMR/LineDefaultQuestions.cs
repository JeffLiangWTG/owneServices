using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class LineDefaultQuestions
	{
		public class DefaultAnswer
		{
			public ZString AnswerCode;
			public ZInt CPDecNum;
			public ZString Permit;
			public ZDateTime CPDecStartDate;
		}

		public LineDefaultQuestions()
			: this(Enumerable.Empty<DefaultAnswer>())
		{
		}

		public LineDefaultQuestions(IEnumerable<DefaultAnswer> answers)
		{
			this.defaultAnswers = CreateDefaultAnswers(answers);
		}

		public (DefaultAnswer answer, string warning) GetDefaultAnswer(LineDefaultKey defaultKey)
		{
			DefaultAnswer answer = null;
			var warning = string.Empty;
			if (defaultAnswers.TryGetValue(defaultKey.QuestionID, out var list))
			{
				answer = list.FirstOrDefault(x => x.CPDecStartDate == defaultKey.StartDate);
				if (answer == null)
				{
					warning = string.Format(CultureInfo.CurrentCulture, "The CP Question {0} has changed since recording the answer to the Product or Classification Lookup.\r\nPlease review the CP Question and/or update the Product or Classification CP Question with current answers.", defaultKey.QuestionID);
					answer = list.FirstOrDefault();
				}
			}
			return (answer, warning);
		}

		Dictionary<ZInt, DefaultAnswer[]> CreateDefaultAnswers(IEnumerable<DefaultAnswer> answers)
		{
			var result = new Dictionary<ZInt, DefaultAnswer[]>();
			foreach (var groupedAnswers in answers.GroupBy(x => x.CPDecNum))
			{
				result.Add(groupedAnswers.Key, groupedAnswers.OrderByDescending(x => x.CPDecStartDate).ToArray());
			}
			return result;
		}
		readonly Dictionary<ZInt, DefaultAnswer[]> defaultAnswers;
	}
}
