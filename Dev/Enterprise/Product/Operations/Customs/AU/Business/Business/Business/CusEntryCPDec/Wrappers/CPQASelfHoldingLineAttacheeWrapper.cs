using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CPQASelfHoldingLineAttacheeWrapper
	{
		public CPQASelfHoldingLineAttacheeWrapper(ISelfHoldingLineAttachee lineAttacheeHolder)
		{
			this.lineAttacheeHolder = lineAttacheeHolder;
			NeedToGenerateQuestions = lineAttacheeHolder.Questions != null && lineAttacheeHolder.Questions.Count == 0;
		}

		public void GenerateQuestion()
		{
			if (NeedToGenerateQuestions)
			{
				QuestionGenerator.GenerateQuestions();
				NeedToGenerateQuestions = false;
			}
		}

		public bool NeedToGenerateQuestions
		{
			get { return fNeedToGenerateQuestions; }
			set
			{
				fNeedToGenerateQuestions = value;
				if (value && lineAttacheeHolder.Questions != null && lineAttacheeHolder.Questions.Count > 0 && !lineAttacheeHolder.IsRiskHistorySupported)
				{
					lineAttacheeHolder.Questions.RemoveAndDeleteAll();
				}
			}
		}
		bool fNeedToGenerateQuestions;

		public ICPQAHeaderAttachee[] Headers
		{
			get { return System.Array.Empty<ICPQAHeaderAttachee>(); }
		}

		public ICPQALineAttachee[] Lines
		{
			get { return new ICPQALineAttachee[] { lineAttacheeHolder }; }
		}

		public CachedAnsweredQuestions CachedQuestions
		{
			get { return null; }
		}

		readonly ISelfHoldingLineAttachee lineAttacheeHolder;

		public LineDefaultQuestions DefaultUniqueQuestions
		{
			get
			{
				if (NeedToGenerateQuestions || fDefaultUniqueQuestions == null)
				{
					fDefaultUniqueQuestions = DefaultQuestionGenerator.GenerateUniqueDefaultQuestions(lineAttacheeHolder, ZDateTime.Empty);
				}
				return fDefaultUniqueQuestions;
			}
		}
		LineDefaultQuestions fDefaultUniqueQuestions;

		#region Implementation

		CMRCPDecQuestionGenerator QuestionGenerator
		{
			get
			{
				if (fGenerator == null)
				{
					fGenerator = new CMRCPDecQuestionGenerator(lineAttacheeHolder);
				}
				return fGenerator;
			}
		}
		CMRCPDecQuestionGenerator fGenerator;

		LineDefaultQuestionGenerator DefaultQuestionGenerator
		{
			get
			{
				if (fDefaultQuestionGenerator == null)
				{
					fDefaultQuestionGenerator = new LineDefaultQuestionGenerator();
				}
				return fDefaultQuestionGenerator;
			}
		}
		LineDefaultQuestionGenerator fDefaultQuestionGenerator;

		#endregion
	}
}
