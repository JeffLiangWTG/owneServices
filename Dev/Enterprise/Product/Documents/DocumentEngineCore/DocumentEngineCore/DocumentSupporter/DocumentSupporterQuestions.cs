using System;
using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocumentSupport
{
	public class DocumentSupporterQuestion
	{
		public DocumentSupporterQuestion(ZString title, ZString questionText)
			: this(title, questionText, QuestionType.Default)
		{
		}

		public DocumentSupporterQuestion(ZString title, ZString questionText, QuestionType type)
		{
			if (questionText.IsEmpty)
			{
				throw new ArgumentException("questionText cannot be empty!");
			}

			Title = title;
			QuestionType = type;
			QuestionText = questionText;
		}

		public ZString Title;
		public ZString QuestionText;
		public AnswerType DefaultResponse;
		public QuestionType QuestionType;
	}

	public enum QuestionType
	{
		Default,
		Warning
	}

	public enum AnswerType
	{
		No,
		Yes
	}
}
