using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCusEntryCPDecLookups : CusEntryCPDecLookups
	{
		public CMRCusEntryCPDecLookups(AutoCusEntryCPDec parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ON_AnswerCode_List
		{
			get
			{
				if (fON_AnswerCode_List == null)
				{
					fON_AnswerCode_List = new CMRCusEntryCPDecAnswerCodeList();
				}

				return fON_AnswerCode_List;
			}
		}
		CodeDescriptionPairList fON_AnswerCode_List;

		public CMRLodgementQuestionCollection CPQuestions
		{
			get
			{
				if (fCPQuestions == null)
				{
					fCPQuestions = new CMRLodgementQuestionCollection(Factory);
					fCPQuestions.Load();
				}
				return fCPQuestions;
			}
		}
		CMRLodgementQuestionCollection fCPQuestions;
	}
}
